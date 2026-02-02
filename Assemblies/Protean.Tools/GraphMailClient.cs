
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;

namespace Protean.Tools
{
    public class GraphMailClient
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _tenantId;
        private readonly string _accessToken;

        public string sendFromAcct;
        public string errorMsg;

        public GraphMailClient(string clientId, string clientSecret, string tenantId)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _tenantId = tenantId;
            _accessToken = GetAccessToken();
        }

        private string GetAccessToken()
        {
            var authority = $"https://login.microsoftonline.com/{_tenantId}";
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            var app = ConfidentialClientApplicationBuilder.Create(_clientId)
                .WithClientSecret(_clientSecret)
                .WithAuthority(new Uri(authority))
                .Build();

            var result = app.AcquireTokenForClient(scopes).ExecuteAsync().Result;
            return result.AccessToken;
        }

        public void SendMail(MailMessage smtpMessage)
        {
            try {
            var graphMessage = ConvertToGraphMessage(smtpMessage);
            string senderEmail = smtpMessage.From.Address;
            SendGraphMessage(graphMessage, senderEmail);  
            }
            catch (Exception ex)
            {

                errorMsg = ex.Message;
                // if (gbDebug)
                // {
                //     returnException(ref msException, mcModuleName, "addAttachment", ex, "", cProcessInfo, gbDebug);
                // }
            }
        }

        private object ConvertToGraphMessage(MailMessage smtpMessage)
        {
            var message = new
            {
                subject = smtpMessage.Subject,
                body = new
                {
                    contentType = smtpMessage.IsBodyHtml ? "HTML" : "Text",
                    content = smtpMessage.Body
                },
                sender =  new {emailAddress = new { address = smtpMessage.From.Address }},
                replyto = ConvertRecipients(smtpMessage.ReplyToList),
                toRecipients = ConvertRecipients(smtpMessage.To),
                ccRecipients = ConvertRecipients(smtpMessage.CC),
                bccRecipients = ConvertRecipients(smtpMessage.Bcc),
                attachments = ConvertAttachments(smtpMessage.Attachments)
            };

            return new
            {
                message,
                saveToSentItems = true
            };
        }

        private List<object> ConvertRecipients(MailAddressCollection addresses)
        {
            var list = new List<object>();
            foreach (var address in addresses)
            {
                list.Add(new
                {
                    emailAddress = new { address = address.Address }
                });
            }
            return list;
        }


        public class GraphFileAttachment
        {
            [JsonProperty("@odata.type")]
            public string ODataType { get; set; } = "#microsoft.graph.fileAttachment";

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("contentType")]
            public string ContentType { get; set; }

            [JsonProperty("contentBytes")]
            public string ContentBytes { get; set; }
        }


        private List<GraphFileAttachment> ConvertAttachments(AttachmentCollection attachments)
        {
            var list = new List<GraphFileAttachment>();
            foreach (Attachment att in attachments)
            {
                using (var ms = new MemoryStream())
                {
                    att.ContentStream.CopyTo(ms);
                    var base64 = Convert.ToBase64String(ms.ToArray());

                    list.Add(new GraphFileAttachment
                    {
                        Name = att.Name,
                        ContentType = att.ContentType.MediaType,
                        ContentBytes = base64
                    });

                }
            }
            return list;
        }

        private void SendGraphMessage(object graphMessage, string senderEmail)
        {
            if (!string.IsNullOrEmpty(sendFromAcct)) {
                senderEmail = sendFromAcct;
            }

                var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            
            var json = JsonConvert.SerializeObject(graphMessage);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = httpClient.PostAsync("https://graph.microsoft.com/v1.0/users/" + senderEmail + "/sendMail", content).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Graph API send failed: {response.StatusCode} - {response.Content.ReadAsStringAsync().Result}");
            }
        }
    }
}

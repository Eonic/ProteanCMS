using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Web.Configuration;
using Newtonsoft.Json;

namespace Protean.Tools.RecaptchaV3
{
    public class Recaptcha
    {
        private readonly string _secretKey;

        public Recaptcha()
        {
            // Read from protean/web section
            NameValueCollection moConfig = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
            _secretKey = moConfig["ReCaptchaKeySecretV3"];
        }

        private class RecaptchaResponse
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("score")]
            public double Score { get; set; }

            [JsonProperty("action")]
            public string Action { get; set; }

            [JsonProperty("error-codes")]
            public string[] ErrorCodes { get; set; }
        }

        public bool Validate(string token, string action = "", double threshold = 0.5)
        {
            if (string.IsNullOrEmpty(token)) return false;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://www.google.com/recaptcha/api/siteverify");
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                string postData = $"secret={_secretKey}&response={token}";
                if (!string.IsNullOrEmpty(action))
                    postData += $"&action={action}";

                using (var stream = request.GetRequestStream())
                {
                    var data = System.Text.Encoding.UTF8.GetBytes(postData);
                    stream.Write(data, 0, data.Length);
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var json = reader.ReadToEnd();
                    var result = JsonConvert.DeserializeObject<RecaptchaResponse>(json);

                    if (!string.IsNullOrEmpty(action) && result.Action != action) return false;

                    return result.Success && result.Score >= threshold;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}

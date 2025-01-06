using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Web.Configuration;
using System.Net.Http.Headers;
using static QRCoder.PayloadGenerator;
using Newtonsoft.Json.Serialization;
using System.Security.Authentication;


namespace Protean.Providers
{
    public class CloudflareService
    {
        
        private readonly HttpClient _httpClient = new HttpClient(new HttpClientHandler { SslProtocols = SslProtocols.Tls12 });
        NameValueCollection oConfig = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
        public CloudflareService(HttpClient httpClient)
        {
            _httpClient = httpClient;           
        }

        public string GetZoneIdAsync(string domain)
        {
            string apiToken = Convert.ToString(oConfig["CloudapiKey"]);
            string apiUrl = Convert.ToString(oConfig["CloudflareApiUrl"]);

            // Construct the API URL with domain
            string url = $"{apiUrl}";

            // Set up the request headers (Authorization: Bearer API Token)
            //_httpClient.DefaultRequestHeaders.Clear();
            //_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiToken}");
           
            _httpClient.DefaultRequestHeaders.Add("X-Auth-Email",Convert.ToString(oConfig["Cloudemail"]));
            _httpClient.DefaultRequestHeaders.Add("X-Auth-Key", apiToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Send the request
            var response = _httpClient.GetAsync(url).Result;

            // Check if the response is successful
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Request failed with status code {response.StatusCode}");                
            }

            // Parse the JSON response
            var content = response.Content.ReadAsStringAsync().Result;
            var jsonResponse = JsonConvert.DeserializeObject<JObject>(content);

            // Extract zone_id
            var zoneId = jsonResponse["result"]?.FirstOrDefault()?["id"]?.ToString();
            if (string.IsNullOrEmpty(zoneId))
            {
                throw new Exception("Zone ID not found.");
            }

            return zoneId;
        }
    }
}

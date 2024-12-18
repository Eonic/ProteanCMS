using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Web.Configuration;
using Newtonsoft.Json.Linq;

namespace Protean.Providers
{
    namespace Cache
    {
        public interface ICacheProvider
        {
            void Initiate(ref Cms myWeb);
            Task<bool> PurgeImageCacheAsync(string[] imageUrl);
            Task PurgeAllCacheAsync();
            //Task<string> GetZoneIdAsync(string domain);
        }

        public class DefaultProvider : ICacheProvider
        {
            private readonly JsonSerializer Serializer = new JsonSerializer();
            private static readonly HttpClient client = new HttpClient();
            public System.Web.HttpContext moCtx = System.Web.HttpContext.Current;

            // Your Cloudflare Zone ID and API Token (Global API Key or Custom Token)
            //All this four setting will go in web.config
            //private const string CloudflareApiUrl = "https://api.cloudflare.com/client/v4/zones/{zone_id}/purge_cache";
            //private readonly string apiKey = "2e9f3dbae298de7d62ec74c62713d5423705f";
            //private readonly string email = "james@intotheblue.co.uk";
           // private readonly string zoneId = "75cbc596c98fa794c5eb2975850f1b9e";  // Or use Global API Key with 'X-Auth-Key'
            NameValueCollection oConfig = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
            NameValueCollection moCartConfig = (NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/cart");

            public DefaultProvider()
            {
                // do nothing
            } 
            public void Initiate(ref Cms myWeb)
            {
                throw new NotImplementedException();
            }           

            public async Task<bool> PurgeImageCacheAsync(string[] imageUrl)
            {
                try
                {

                    using (var client = new HttpClient())
                    {
                        // Prepare the API URL
                        var cloudfalreservice = new CloudflareService(client);
                        string zoneId = cloudfalreservice.GetZoneIdAsync(moCartConfig["SiteURL"]);
                        var url = oConfig["CloudflareApiUrl"] + "/"+ zoneId + "/purge_cache";

                        // Set up the headers
                        client.DefaultRequestHeaders.Add("X-Auth-Email", Convert.ToString(oConfig["Cloudemail"]));
                        client.DefaultRequestHeaders.Add("X-Auth-Key", Convert.ToString(oConfig["CloudapiKey"]));
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));                       


                        // Prepare the body
                        var body = new
                        {
                            files = new[] { imageUrl }
                        };

                        var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

                        // Send the POST request to purge the cache for the image
                        var response = await client.PostAsync(url, content);

                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine("Cache purged successfully!");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("Failed to purge cache: " + response.ReasonPhrase);
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while purging cache: " + ex.Message);
                    return false;
                }
            }


            public async Task PurgeAllCacheAsync()
            {
                using (var client = new HttpClient())
                {
                    var cloudfalreservice = new CloudflareService(client);
                    string zoneId = cloudfalreservice.GetZoneIdAsync(moCartConfig["SiteURL"]);

                    // Set up the request headers
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Convert.ToString(oConfig["CloudapiKey"]));
                    client.DefaultRequestHeaders.Add("Content-Type", "application/json");

                    // Build the request body for purging all cache
                    var purgeRequest = new
                    {
                        purge_everything = true
                    };

                    string jsonPayload = JsonConvert.SerializeObject(purgeRequest);

                    // Send POST request to Cloudflare API
                    var response = await client.PostAsync(
                        oConfig["CloudflareApiUrl"] + "/" + zoneId + "/purge_cache",
                        new StringContent(jsonPayload, Encoding.UTF8, "application/json")
                    );

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("All cache purged successfully.");
                    }
                    else
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Error purging cache: " + errorResponse);
                    }
                }
            }

            
        }
    }
}

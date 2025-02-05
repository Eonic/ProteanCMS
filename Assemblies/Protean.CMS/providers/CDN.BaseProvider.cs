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
using System.Security.Authentication;
using Protean.Providers.Payment;
using System.Reflection;
using static Protean.stdTools;
using Protean.Tools;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Ajax.Utilities;
using System.Configuration.Provider;
using Microsoft.VisualBasic;
using System.Xml;


namespace Protean.Providers
{
    namespace CDN
    {
        public interface ICDNProvider
        {
            ICDNProvider Initiate(ref Cms myWeb);
            ICDNAdminXforms AdminXforms { get; set; }          
        }
        public interface ICDNAdminXforms
        {
            string PurgeImageCacheAsync(string[] imageUrl, ref Cms myWeb);
            Task PurgeAllCacheAsync();
        }
        public class ReturnProvider
        {
            private const string mcModuleName = "Protean.Providers.CDN.GetProvider";
          
            public ICDNProvider Get(ref Cms myWeb)
            {
                try
                {
                    Type calledType;
                    string ProviderClass = "";
                    Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection("protean/CDNProviders");
                    if(moPrvConfig != null)
                    {
                        ProviderClass = Convert.ToString(moPrvConfig.Providers[0].Name);

                        if (string.IsNullOrEmpty(ProviderClass))
                        {
                            ProviderClass = "Protean.Providers.CDN.DefaultProvider";
                            calledType = Type.GetType(ProviderClass, true);
                        }
                        else
                        {
                            if (moPrvConfig.Providers[0].Name != null)
                            {
                                var assemblyInstance = Assembly.Load(moPrvConfig.Providers[0].Type);
                                calledType = assemblyInstance.GetType("Protean.Providers.CDN." + ProviderClass, true);
                            }
                            else
                            {
                                calledType = Type.GetType("Protean.Providers.CDN." + ProviderClass, true);
                            }
                        }

                        var o = Activator.CreateInstance(calledType);
                        var args = new object[1];
                        args[0] = myWeb;

                        return (ICDNProvider)calledType.InvokeMember("Initiate", BindingFlags.InvokeMethod, null, o, args);
                    }else
                    {
                        return null;
                    }
                    
                }
                catch (Exception ex)
                {
                    // TS commented this out as if we have an old payment provider that has been retired we do not want errors.
                    //stdTools.returnException(ref myWeb.msException, mcModuleName, "New", ex, "", ProviderName + " Could Not be Loaded", gbDebug);
                    return null;
                }
            }

        }
        public class DefaultProvider : ICDNProvider
        {
            private ICDNAdminXforms _AdminXforms;
            ICDNAdminXforms ICDNProvider.AdminXforms
            {
                set
                {
                    _AdminXforms = value;
                }
                get
                {
                    return _AdminXforms;
                }
            }
            public DefaultProvider()
            {
                // do nothing
            }
            public ICDNProvider Initiate(ref Cms myWeb)
            {
                _AdminXforms = new AdminXForms(ref myWeb);               
                return this;
            }
           

            public class AdminXForms : ICDNAdminXforms
            {
                private const string mcModuleName = "Providers.CDN.Default.AdminXForms";

                public Cms myWeb;
                public AdminXForms(ref Cms myWeb)
                {
                }
                public string PurgeImageCacheAsync(string[] imageUrl, ref Cms myWeb)
                {
                    throw new NotImplementedException();
                }
                public async Task PurgeAllCacheAsync()
                {
                    throw new NotImplementedException();
                }
            }              
            
        }
    }
}

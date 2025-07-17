using Microsoft.Ajax.Utilities;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Protean.Providers.Payment;
using Protean.Tools;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Policy;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Security;
using System.Web.SessionState;
using System.Xml;
using static Protean.Cms;
using static Protean.stdTools;
using static Protean.Tools.Xml;


namespace Protean.Providers
{
    namespace Discount
    {
        public interface IdiscountProvider
        {
            IdiscountProvider Initiate(Cms myWeb, NameValueCollection config);

            string CheckDiscountApplicable(ref DataRow oDrDiscount,ref  Protean.Cms.Cart oCart);

            string ApplyDiscount(ref DataRow oDrDiscount, ref Protean.Cms.Cart oCart);
            //long CheckAuthenticationResponse(HttpRequest request, HttpSessionState session, HttpResponse response); // returns userid

            System.Collections.Specialized.NameValueCollection config
            { get; }

            string name { get; }

        }

        public class ReturnProvider
        {
            private const string mcModuleName = "Protean.Providers.Authentication.GetProvider";
          
            public IEnumerable<IdiscountProvider> Get(ref Cms myWeb)
            {
                try
                {

                                      
                    Type calledType;
                    string ProviderClass = "";
                    Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection("protean/discountProviders");

                    ICollection<IdiscountProvider> providerList = new IdiscountProvider[0];
                    var modifiable = providerList.ToList();

                    //if (moPrvConfig != null)
                    //{
                    //    foreach (System.Configuration.ProviderSettings authProvider in moPrvConfig.Providers) { 

                    //        ProviderClass = Convert.ToString(authProvider.Name);

                    //        if (string.IsNullOrEmpty(ProviderClass))
                    //        {
                    //            ProviderClass = "Protean.Providers.Authentication.DefaultProvider";
                    //            calledType = Type.GetType(ProviderClass, true);
                    //        }
                    //        else
                    //        {
                    //            if (authProvider.Type != "")
                    //            {
                    //                var assemblyInstance = Assembly.Load(authProvider.Type);
                    //                calledType = assemblyInstance.GetType("Protean.Providers.Authentication." + ProviderClass, true);
                    //            }
                    //            else
                    //            {
                    //                calledType = Type.GetType("Protean.Providers.Authentication." + ProviderClass, true);
                    //            }
                    //        }

                    //        var o = Activator.CreateInstance(calledType);
                    //        var args = new object[2];
                    //        args[0] = myWeb;
                    //        args[1] = authProvider.Parameters;

                    //        modifiable.Add((IauthenticaitonProvider)calledType.InvokeMember("Initiate", BindingFlags.InvokeMethod, null, o, args));

                    //    }

                    //    providerList = modifiable;
                    //    return providerList;

                    //}else
                    //{
                    //    return null;
                    //}


                    if (moPrvConfig != null)
                    {
                        foreach (ProviderSettings authProvider in moPrvConfig.Providers)
                        {
                            string providerTypeName = authProvider.Type;
                            if (string.IsNullOrEmpty(providerTypeName))
                            {                                
                                providerTypeName = "Protean.Providers.Discount.DefaultProvider, ProteanCMS";
                            }

                            try
                            {
                                // Load the full type (with namespace + assembly)
                                calledType = Type.GetType(providerTypeName, throwOnError: true);

                                object instance = Activator.CreateInstance(calledType);

                                var configParams = new NameValueCollection(authProvider.Parameters);
                                configParams["name"] = authProvider.Name;

                                var args = new object[2];
                                args[0] = myWeb;
                                args[1] = configParams;

                                var provider = (IdiscountProvider)calledType.InvokeMember("Initiate", BindingFlags.InvokeMethod, null, instance, args);

                                modifiable.Add(provider);
                            }
                            catch (Exception ex)
                            {
                                // Log each individual provider error, skip to next
                                Console.WriteLine($"[!] Failed to load provider '{authProvider.Name}': {ex.Message}");
                            }
                        }
                        return modifiable;
                    }
                    else
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
        public class Default : IdiscountProvider
        {
            private string _Name = "Default";
            public Protean.Cms _myWeb;
            System.Collections.Specialized.NameValueCollection moCartConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/cart");

            private NameValueCollection _Config;
            public Default()
            {
                // do nothing
            }

            string IdiscountProvider.name
            {
                get
                {
                    return GetType().Name; ;
                }
            }

            public NameValueCollection config
            {
                get
                {
                    return _Config; 
                }
            }
          
            public IdiscountProvider Initiate(Cms myWeb, NameValueCollection config)
            {
                _myWeb = myWeb;
                _Config = config;
                return this;
            }

            public string CheckDiscountApplicable(ref DataRow oDrDiscount, ref Cart oCart)
            {
                throw new NotImplementedException();
            }

            public string ApplyDiscount(ref DataRow oDrDiscount, ref Cart oCart)
            {
                throw new NotImplementedException();
            }


        }
    }
}


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;


using System.Web.Configuration;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PayPal.Api;
using Protean.Providers.Membership;

namespace Protean
{

    public class rest : Protean.Base
    {

        public bool gbDebug = false;


        public rest() : base(System.Web.HttpContext.Current)
        {
            InitialiseVariables();

        }

        public void InitialiseVariables()
        {
            this.PerfMon.Log("API", "Open");
            string sProcessInfo = "";
            string cCloneContext = string.Empty;
            string rootPageIdFromConfig = string.Empty;
            try
            {

                // if we access base via soap the session is not available
                if (this.moSession != null)
                {

                    // below code has beem moved to membership base provider

                    Protean.Cms myWeb = new Cms();
                    ReturnProvider RetProv = new Protean.Providers.Membership.ReturnProvider();
                    IMembershipProvider oMembershipProv = RetProv.Get(ref myWeb, this.moConfig["MembershipProvider"]);
                    this.mnUserId = Conversions.ToInteger(oMembershipProv.Activities.GetUserId(ref myWeb));

                    if (Conversions.ToBoolean(Operators.ConditionalCompareObjectEqual(this.moSession["adminMode"], "true", false)))
                    {
                        this.mbAdminMode = true;
                        // moDbHelper.gbAdminMode = mbAdminMode
                    }
                }

                // We need the userId placed into dbhelper.
                // moDbHelper.mnUserId = mnUserId

                if (this.moConfig["Debug"] != null)
                {
                    switch (Strings.LCase(this.moConfig["Debug"]) ?? "")
                    {
                        case "on":
                            {
                                gbDebug = true;
                                break;
                            }
                        case "off":
                            {
                                gbDebug = false;
                                break;
                            }

                        default:
                            {
                                gbDebug = false;
                                break;
                            }
                    }
                }
            }


            catch (Exception ex)
            {

                this.OnComponentError(this, new Tools.Errors.ErrorEventArgs(this.mcModuleName, "Open", ex, sProcessInfo));

            }

        }

        public virtual void JSONRequest()
        {

            this.PerfMon.Log("API", "Request");
            string sProcessInfo = "";
            try
            {

                string path = this.moRequest.ServerVariables["HTTP_X_ORIGINAL_URL"];

                if (path.Contains("?"))
                {
                    path = path.Substring(0, path.IndexOf("?"));
                }

                string[] pathsplit = Strings.Split(path, "/");
                // URL = /API/ProviderName/methodName

                string ProviderName = pathsplit[2];
                string methodName = pathsplit[3];
                string classPath = ProviderName + ".JSONActions";
                string assemblytype = string.Empty;

                var s = this.moRequest.InputStream;
                var sr = new StreamReader(s);
                string jsonString = sr.ReadLine();
                if (string.IsNullOrEmpty(jsonString))
                {
                    jsonString = this.moRequest["data"];
                }

                Newtonsoft.Json.Linq.JObject jObj = null;
                Dictionary<string, string> paramDictionary = null;
                if (jsonString != null)
                {
                    try
                    {
                        jObj = Newtonsoft.Json.Linq.JObject.Parse(jsonString);
                    }
                    catch (Exception)
                    {
                        // Not a valid json string we want to make the request anyway
                        string query = System.Web.HttpUtility.UrlDecode(jsonString);
                        var formData = System.Web.HttpUtility.ParseQueryString(query);
                        try
                        {
                            paramDictionary = formData.AllKeys.ToDictionary(k => k, k => formData[k]);
                        }
                        catch
                        {
                        }
                    }
                }

                if (this.moCtx.Request.QueryString.Count > 1)
                {
                    paramDictionary = moCtx.Request.QueryString.AllKeys.ToDictionary(k => k, k => this.moCtx.Request.QueryString[k]);
                }
                // add paramDict to jObj

                //if (paramDictionary != null)
                //{
                //    if (jObj is null)
                //    {
                //        jObj = new Newtonsoft.Json.Linq.JObject();
                //    }
                //    foreach (KeyValuePair<string, string> kvp in paramDictionary)
                //        jObj.Add(new Newtonsoft.Json.Linq.JProperty(kvp.Key, kvp.Value));
                //}

                Type calledType;


                if (Strings.LCase(ProviderName) == "cms.cart" | Strings.LCase(ProviderName) == "cms.content" | Strings.LCase(ProviderName) == "cms.admin")
                    ProviderName = "";

                if (!string.IsNullOrEmpty(ProviderName))
                {
                    // case for external Providers
                    if (ProviderName.Contains("."))
                    {
                        string[] pnArr = ProviderName.Split('.');
                        Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection("protean/" + Strings.LCase(pnArr[0]) + "Providers");
                        // Dim assemblyInstance As [Assembly] = [Assembly].LoadFrom(goServer.MapPath(moPrvConfig.Providers(pnArr(1)).Parameters("path")))
                        var assemblyInstance = Assembly.Load(moPrvConfig.Providers[pnArr[1]].Type);
                        classPath = "Protean.Providers." + pnArr[0] + "." + pnArr[1] + "Extras.JSONActions";
                        calledType = assemblyInstance.GetType(classPath, true);
                    }
                    else
                    {
                        Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection("protean/messagingProviders");
                        var assemblyInstance = Assembly.LoadFrom(this.goServer.MapPath(moPrvConfig.Providers[ProviderName].Parameters["path"]));
                        // Dim assemblyInstance As [Assembly] = [Assembly].Load(moPrvConfig.Providers(ProviderName).Type)
                        classPath = moPrvConfig.Providers[ProviderName].Parameters["className"] + "+JSONActions";
                       // classPath = "JSONActions";
                        calledType = assemblyInstance.GetType(classPath, true);
                    }
                }
                else
                {
                    // case for methods within ProteanCMS Core DLL
                    calledType = Type.GetType("Protean." + Strings.Replace(classPath, ".", "+"), true);
                }

                var o = Activator.CreateInstance(calledType);

                var args = new object[1];
                args[0] = this;
                if (jObj != null)
                {
                    Array.Resize(ref args, 2);
                    args[1] = jObj;
                }
                else if (paramDictionary != null) 
                {
                    Array.Resize(ref args, 2);
                    args[1] = paramDictionary;
                }
                else
                {
                    Array.Resize(ref args, 2);
                    args[1] = null;
                }

                // check the response whatever is coming like with code 400, 200, based on the output- return in Json
                string myResponse = Conversions.ToString(calledType.InvokeMember(methodName, BindingFlags.InvokeMethod, null, o, args));
                this.moResponse.Write(myResponse);
            }

            catch (Exception ex)
            {
                this.moResponse.StatusCode = 400;
                
                this.OnComponentError(this, new Tools.Errors.Error(mcModuleName, "JSONRequest", ex, sProcessInfo,0,null, this.moResponse.Status,this.moResponse.StatusCode, "", "", ""));

                //this.OnComponentError(this, new Tools.Errors.ErrorEventArgs(this.mcModuleName, "JSONRequest", ex, sProcessInfo));
                // returnException(mcModuleName, "getPageHtml", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                if (gbDebug)
                {
                    this.moResponse.StatusCode = 404;
                    this.moResponse.Write(JsonConvert.SerializeObject(ex));
                }
                else
                {
                    this.moResponse.StatusCode = 404;
                    this.moResponse.Write(ex.Message);
                }

                //Finalize();
            }
            finally
            {

            }
            this.PerfMon.Write();
        }


        public class JsonActions
        {

            public bool ValidateAPICall(ref Cms myWeb, string sGroupName, string cSchemaName = "Role")
            {
                // Create -InsertOrder Group and pass as a input
                // check user present in the group
                bool bIsAuthorized = false;
                string authHeader = string.Empty;
                string encodedUsernamePassword = string.Empty;
                string usernamePassword = string.Empty;
                var encoding = Encoding.GetEncoding("iso-8859-1");
                int seperatorIndex;
                string username = string.Empty;
                string password = string.Empty;
                int nUserId = 0;
                string sValidResponse = string.Empty;

                try
                {

                    if (myWeb.mnUserId != 0)
                    {
                        nUserId = myWeb.mnUserId;
                    }
                    if (nUserId == 0)
                    {
                        if(myWeb.moSession!=null)
                        {
                            if (myWeb.moSession["nUserId"] != null)
                            {
                                nUserId = Convert.ToInt32(myWeb.moSession["nUserId"]);
                            }
                        }
                    }
                    // HttpContext httpContext = HttpContext.Current;
                    if (myWeb.moCtx.Request.Headers != null)
                    {
                        if (myWeb.moCtx.Request.Headers["Authorization"] != null)
                        {
                            authHeader = myWeb.moCtx.Request.Headers["Authorization"];
                            if (authHeader.Substring("Basic ".Length).Trim().Length != 0)
                            {
                                encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
                                usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));
                                seperatorIndex = usernamePassword.IndexOf(":");
                                username = usernamePassword.Substring(0, seperatorIndex);
                                password = usernamePassword.Substring(seperatorIndex + 1);
                                sValidResponse = myWeb.moDbHelper.validateUser(username, password);
                                if (Information.IsNumeric(sValidResponse))
                                {
                                    nUserId = (int)Conversions.ToLong(sValidResponse);
                                }
                            }
                        }
                    }
                    if (nUserId != 0)
                    {
                        bIsAuthorized = myWeb.moDbHelper.checkUserRole(sGroupName, cSchemaName, (long)nUserId);
                        if (bIsAuthorized)
                        {
                            myWeb.mnUserId = nUserId;
                        }
                    }
                    else
                    {
                        bIsAuthorized = false;
                    }
                }


                catch (Exception)
                {
                    // OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs("API", "ValidateAPICall", ex, ""))

                    return false;
                }
                return bIsAuthorized;
            }


        }
    }
}
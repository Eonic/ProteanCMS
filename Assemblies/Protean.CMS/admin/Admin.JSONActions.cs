using System;
using System.Collections;
using System.Data;
using System.Web.Configuration;
using System.Xml;
using Microsoft.VisualBasic.CompilerServices;
using static Protean.stdTools;

namespace Protean
{

    public partial class Cms
    {

        public partial class Admin
        {


            #region JSON Actions

            public class JSONActions : Protean.Rest.JsonActions
            {

                public event OnErrorEventHandler OnError;

                public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
                private const string mcModuleName = "Cms.Admin.JSONActions";
                private System.Collections.Specialized.NameValueCollection moLmsConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/lms");
                private Cms myWeb;
                private Cms.Cart myCart;
                public Cms.xForm moAdXfm;
                public Admin.AdminXforms moAdminXfm;
                public Admin.Redirects moAdminRedirect;
                public System.Collections.Specialized.NameValueCollection goConfig;
                public System.Web.HttpContext moCtx;




                public JSONActions()
                {
                    string ctest = "this constructor is being hit"; // for testing
                    myWeb = new Cms();
                    myWeb.InitializeVariables();
                    myWeb.Open();
                    myCart = new Cms.Cart(ref myWeb);
                    moAdXfm = new Cms.xForm(ref myWeb);
                    moAdminRedirect = new Admin.Redirects();
                    moAdminXfm = (Admin.AdminXforms)myWeb.getAdminXform();
                    goConfig = myWeb.moConfig;
                    moCtx = myWeb.moCtx;

                }

                public new void Open(XmlDocument oPageXml)
                {
                    string cProcessInfo = "";
                    try
                    {
                        moAdXfm.moPageXML = oPageXml;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "Open", ex, "", cProcessInfo, gbDebug);
                    }
                }



                public string DeleteObject(ref Protean.Rest myApi, ref Newtonsoft.Json.Linq.JObject inputJson)
                {
                    string ObjType = "";  // Just declare at top
                    var result = default(long);
                    string ObjId = "";
                    try
                    {

                        if (this.ValidateAPICall(ref myWeb, "Administrator"))
                        {

                            if (inputJson["objType"] != null)
                            {
                                ObjType = inputJson["ObjType"].ToObject<string>();
                            }

                            if (inputJson["objId"] != null)
                            {
                                ObjId = inputJson["objId"].ToObject<string>();
                            }
                            result = myWeb.moDbHelper.DeleteObject((Cms.dbHelper.objectTypes)Conversions.ToInteger(ObjType), Conversions.ToLong(ObjId), false);

                        }
                        return "[{\"Key\":\"" + ObjId + "\",\"Value\":\"" + result + "\"}]";
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "Query", ex, ""));
                        return ex.Message;
                    }
                }

                public string QueryValue(ref Protean.Rest myApi, ref Newtonsoft.Json.Linq.JObject jObj)
                {
                    try
                    {
                        string count = "0";
                        bool bIsAuthorized = false;
                        bIsAuthorized = this.ValidateAPICall(ref myWeb, "Administrator");
                        if (bIsAuthorized)
                        {

                            object sSql = myApi.moConfig[myApi.moRequest["query"]];

                            var result = myWeb.moDbHelper.GetDataValue(Conversions.ToString(sSql), CommandType.StoredProcedure);
                            count = result is null ? "" : Convert.ToString(result);

                        }
                        return "[{\"Key\":\"" + myApi.moRequest["query"] + "\",\"Value\":\"" + count + "\"}]";
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "Query", ex, ""));
                        return ex.Message;
                    }
                }

                public string LoadUrlsForPagination(ref Protean.Rest myApi, ref Newtonsoft.Json.Linq.JObject inputJson)
                {

                    string redirectType = "";
                    if (inputJson["redirectType"] != null)
                    {
                        redirectType = inputJson["redirectType"].ToObject<string>();
                    }
                    int pageloadCount = inputJson["loadCount"].ToObject<int>();
                    string JsonResult = "";
                    try
                    {
                        if (myApi.mbAdminMode)
                        {
                            JsonResult = moAdminRedirect.LoadUrlsForPegination(ref redirectType, ref pageloadCount);
                        }
                        return JsonResult;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "LoadUrlsForPagination", ex, ""));
                        return ex.Message;
                    }

                }

                public string AddNewUrl(ref Protean.Rest myApi, ref Newtonsoft.Json.Linq.JObject inputJson)
                {
                    string JsonResult = "";
                    string redirectType = "";
                    string oldUrl = "";
                    string newUrl = "";
                    if (inputJson["redirectType"] != null)
                    {
                        redirectType = inputJson["redirectType"].ToObject<string>();
                    }

                    if (inputJson["oldUrl"] != null)
                    {
                        oldUrl = inputJson["oldUrl"].ToObject<string>();
                    }

                    if (inputJson["newUrl"] != null)
                    {
                        newUrl = inputJson["newUrl"].ToObject<string>();
                    }
                    try
                    {
                        if (myApi.mbAdminMode)
                        {
                            JsonResult = moAdminRedirect.CreateRedirect(ref redirectType, ref oldUrl, ref newUrl);
                        }

                        return JsonResult;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddNewUrl", ex, ""));
                        return ex.Message;
                    }

                }

                public string SearchUrl(ref Protean.Rest myApi, ref Newtonsoft.Json.Linq.JObject inputJson)
                {
                    string redirectType = "";
                    string searchObj = "";
                    int pageloadCount = 0;

                    if (inputJson["redirectType"] != null)
                    {
                        redirectType = inputJson["redirectType"].ToObject<string>();
                    }
                    if (inputJson["searchObj"] != null)
                    {
                        searchObj = inputJson["searchObj"].ToObject<string>();
                    }
                    if (inputJson["loadCount"] != null)
                    {
                        pageloadCount = inputJson["loadCount"].ToObject<int>();
                    }

                    try
                    {
                        string JsonResult = "";
                        if (myApi.mbAdminMode)
                        {
                            JsonResult = moAdminRedirect.SearchUrl(ref redirectType, ref searchObj, ref pageloadCount);
                        }
                        return JsonResult;
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "SearchUrl", ex, ""));
                        return ex.Message;
                    }

                }



                public string SaveUrls(ref Protean.Rest myApi, ref Newtonsoft.Json.Linq.JObject inputJson)
                {
                    string redirectType = "";
                    string oldUrl = "";
                    string newUrl = "";
                    string hiddenOldUrl = "";
                    string JsonResult = "";
                    if (inputJson["redirectType"] != null)
                    {
                        redirectType = inputJson["redirectType"].ToObject<string>();
                    }

                    if (inputJson["oldUrl"] != null)
                    {
                        oldUrl = inputJson["oldUrl"].ToObject<string>();
                    }

                    if (inputJson["NewUrl"] != null)
                    {
                        newUrl = inputJson["NewUrl"].ToObject<string>();
                    }
                    if (inputJson["hiddenOldUrl"] != null)
                    {
                        hiddenOldUrl = inputJson["hiddenOldUrl"].ToObject<string>();
                    }

                    try
                    {
                        if (myApi.mbAdminMode)
                        {
                            JsonResult = moAdminRedirect.CreateRedirect(ref redirectType, ref oldUrl, ref newUrl, hiddenOldUrl);
                        }
                        return JsonResult;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "SaveUrls", ex, ""));
                        return ex.Message;
                    }

                    return JsonResult;
                }

                public string DeleteUrls(ref Protean.Rest myApi, ref Newtonsoft.Json.Linq.JObject inputJson)
                {
                    string redirectType = "";
                    string oldUrl = "";
                    string newUrl = "";
                    string JsonResult = "";
                    if (inputJson["redirectType"] != null)
                    {
                        redirectType = inputJson["redirectType"].ToObject<string>();
                    }
                    if (inputJson["oldUrl"] != null)
                    {
                        oldUrl = inputJson["oldUrl"].ToObject<string>();
                    }

                    if (inputJson["NewUrl"] != null)
                    {
                        newUrl = inputJson["NewUrl"].ToObject<string>();
                    }
                    try
                    {
                        if (myApi.mbAdminMode)
                        {
                            JsonResult = moAdminRedirect.DeleteUrls(ref redirectType, ref oldUrl, ref newUrl);
                        }

                        return JsonResult;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "DeleteUrls", ex, ""));
                        return ex.Message;
                    }
                }

                public string IsUrlPresent(ref Protean.Rest myApi, ref Newtonsoft.Json.Linq.JObject inputJson)
                {
                    string JsonResult = "";
                    string redirectType = "";
                    string oldUrl = "";
                    if (inputJson["redirectType"] != null)
                    {
                        redirectType = inputJson["redirectType"].ToObject<string>();
                    }
                    if (inputJson["oldUrl"] != null)
                    {
                        oldUrl = inputJson["oldUrl"].ToObject<string>();
                    }
                    try
                    {
                        if (myApi.mbAdminMode)
                        {
                            JsonResult = moAdminRedirect.IsUrlPresent(ref redirectType, ref oldUrl);
                        }
                        return JsonResult;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "IsUrlPresent", ex, ""));
                        return ex.Message;
                    }
                }

                public string GetTotalNumberOfUrls(ref Protean.Rest myApi, ref Newtonsoft.Json.Linq.JObject inputJson)
                {
                    string JsonResult = "";
                    string redirectType = "";
                    if (inputJson["redirectType"] != null)
                    {
                        redirectType = inputJson["redirectType"].ToObject<string>();
                    }
                    try
                    {
                        if (myApi.mbAdminMode)
                        {
                            JsonResult = moAdminRedirect.GetTotalNumberOfUrls(ref redirectType);
                        }
                        return JsonResult;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetTotalNumberOfUrls", ex, ""));
                        return ex.Message;
                    }
                }
                public string GetTotalNumberOfSearchUrls(ref Protean.Rest myApi, ref Newtonsoft.Json.Linq.JObject inputJson)
                {
                    string JsonResult = "";
                    string redirectType = "";
                    string SearchObj = "";
                    if (inputJson["redirectType"] != null)
                    {
                        redirectType = inputJson["redirectType"].ToObject<string>();
                    }
                    if (inputJson["searchObj"] != null)
                    {
                        SearchObj = inputJson["searchObj"].ToObject<string>();
                    }

                    try
                    {
                        if (myApi.mbAdminMode)
                        {
                            JsonResult = moAdminRedirect.GetTotalNumberOfSearchUrls(ref redirectType, ref SearchObj);
                        }
                        return JsonResult;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetTotalNumberOfSearchUrls", ex, ""));
                        return ex.Message;
                    }
                }

                public string LoadAllUrls(ref Protean.Rest myApi, ref Newtonsoft.Json.Linq.JObject inputJson)
                {

                    string redirectType = "";
                    int pageloadCount = 0;
                    string actionFlag = "";
                    if (inputJson["redirectType"] != null)
                    {
                        redirectType = inputJson["redirectType"].ToObject<string>();
                    }
                    if (inputJson["loadCount"] != null)
                    {
                        pageloadCount = inputJson["loadCount"].ToObject<int>();
                    }
                    if (inputJson["flag"] != null)
                    {
                        actionFlag = inputJson["flag"].ToObject<string>();
                    }

                    string JsonResult = "";
                    try
                    {
                        if (myApi.mbAdminMode)
                        {

                            JsonResult = moAdminRedirect.LoadAllurls(ref redirectType, ref pageloadCount, ref actionFlag);
                        }
                        return JsonResult;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "LoadAllUrls", ex, ""));
                        return ex.Message;
                    }

                }



                public string IsParentPage(ref Protean.Rest myApi, ref Newtonsoft.Json.Linq.JObject inputJson)
                {
                    string JsonResult = "";
                    string pageId = 0.ToString();
                    if (inputJson["pageId"] != null)
                    {
                        pageId = inputJson["pageId"].ToObject<int>().ToString();
                    }
                    try
                    {
                        if (myApi.mbAdminMode)
                        {
                            int argpageId = Conversions.ToInteger(pageId);
                            JsonResult = Conversions.ToString(moAdminRedirect.IsParentPage(ref argpageId));
                            pageId = argpageId.ToString();
                        }
                        return JsonResult;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "IsParentPage", ex, ""));
                        return ex.Message;
                    }
                }

                public string RedirectPage(ref Protean.Rest myApi, ref Newtonsoft.Json.Linq.JObject inputJson)
                {
                    string JsonResult = "";
                    string redirectType = "";
                    string oldUrl = "";
                    string newUrl = "";
                    string hiddenOldUrl = "";
                    string pageId = "";
                    string isParentPage = "";
                    string sType = "";

                    if (inputJson["redirectType"] != null)
                    {
                        redirectType = inputJson["redirectType"].ToObject<string>();
                    }

                    if (inputJson["oldUrl"] != null)
                    {
                        oldUrl = inputJson["oldUrl"].ToObject<string>();
                    }

                    if (inputJson["newUrl"] != null)
                    {
                        newUrl = inputJson["newUrl"].ToObject<string>();
                    }

                    if (inputJson["pageId"] != null)
                    {
                        pageId = inputJson["pageId"].ToObject<string>();
                    }
                    if (inputJson["isParent"] != null)
                    {
                        isParentPage = inputJson["isParent"].ToObject<string>();
                    }

                    if (inputJson["pageType"] != null)
                    {
                        sType = inputJson["pageType"].ToObject<string>();
                    }

                    if (inputJson["pageurl"] != null)
                    {
                        hiddenOldUrl = inputJson["pageurl"].ToObject<string>();
                    }
                    try
                    {
                        if (myApi.mbAdminMode)
                        {
                            JsonResult = moAdminRedirect.RedirectPage(ref redirectType, ref oldUrl, ref newUrl, ref hiddenOldUrl, Conversions.ToBoolean(isParentPage), sType, Conversions.ToInteger(pageId));
                        }

                        return JsonResult;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "RedirectPage", ex, ""));
                        return ex.Message;
                    }
                }

                public string ReIndexingAPI(ref Protean.Rest myApi, ref Newtonsoft.Json.Linq.JObject inputJson)
                {
                    string sString;
                    try
                    {
                        var objservices = new Protean.Services();

                        if (objservices.CheckUserIP())
                        {
                            bool bIsAuthorized = false;
                            bIsAuthorized = this.ValidateAPICall(ref myWeb, "Administrator");
                            if (bIsAuthorized)
                            {
                                var objAdmin = new Admin();
                                objAdmin.ReIndexing(ref myWeb);
                                sString = "success";
                            }
                            else
                            {
                                sString = "Invalid authentication";
                            }
                        }

                        else
                        {
                            sString = "No access to this IPAddress";
                        }
                        return sString;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "IsParentPage", ex, ""));
                        return ex.Message;
                    }

                }

                public string CleanfileName(ref Protean.Rest myApi, ref Newtonsoft.Json.Linq.JObject inputJson)
                {
                    string JsonResult = "";
                    string Filename = string.Empty;
                    var fsHelper = new Protean.fsHelper();

                    try
                    {
                        if (myApi.mbAdminMode)
                        {

                            if (myApi.moRequest.QueryString["Filename"] != null)
                            {
                                Filename = myApi.moRequest.QueryString["Filename"];
                            }
                            if (!string.IsNullOrEmpty(Filename))
                            {
                                JsonResult = fsHelper.CleanfileName(Filename);
                            }

                        }
                        return JsonResult;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ReplaceRegularExpression", ex, ""));
                        return ex.Message;
                    }
                }
                public string GetExistsFileName(ref Protean.Rest myApi, ref Newtonsoft.Json.Linq.JObject jObj)
                {
                    try
                    {
                        return Conversions.ToString(moCtx.Session["ExistsFileName"]);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        moCtx.Session["ExistsFileName"] = null;
                    }

                    return default;
                }
                public string CompressImage(ref Protean.Rest myApi, ref Newtonsoft.Json.Linq.JObject inputJson)
                {
                    string JsonResult = "0";
                    string Filename = string.Empty;
                    var fsHelper = new Protean.fsHelper();
                    string TinyAPIKey = goConfig["TinifyKey"];
                    try
                    {
                        if (myApi.mbAdminMode)
                        {

                            if (myApi.moRequest.QueryString["Filename"] != null)
                            {
                                var oImgTool = new Tools.Image("");
                                oImgTool.TinifyKey = TinyAPIKey;
                                var oFile = new System.IO.FileInfo(myApi.goServer.MapPath(myApi.moRequest.QueryString["Filename"]));
                                JsonResult = "reduction:'" + oImgTool.CompressImage(oFile, true) / 1000d + "'";
                                oFile.Refresh();
                                JsonResult = JsonResult + ",new_size:'" + oFile.Length / 1000d + "'";
                            }

                        }
                        return JsonResult;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ReplaceRegularExpression", ex, ""));
                        return ex.Message;
                    }
                }
                public string SendReviewCompleteEmail(ref Protean.Rest myApi, ref Newtonsoft.Json.Linq.JObject inputJson)
                {
                    string JsonResult = "";
                    string ReviewId = string.Empty;
                    var fsHelper = new Protean.fsHelper();

                    try
                    {
                        if (inputJson != null)
                        {
                            if (inputJson["ReviewId"].ToString() != null)
                            {
                                ReviewId = inputJson["ReviewId"].ToString();
                            }
                        }
                        else
                        {
                            ReviewId = myWeb.moRequest["id"];
                        }
                        // Send Email
                        var oMsg = new Protean.Messaging();
                        XmlDocument doc = new XmlDocument();
                        string strUrl = "https://www.intotheblue.co.uk";
                        string CustomerName = string.Empty;
                        string strEmail = "";
                        System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                        string xsltPath = moMailConfig["ReviewCompleteEmailTemplatePath"];

                        var dtReviewerEmail = new DataTable();
                        Hashtable arrParms = new Hashtable();
                        arrParms.Add("ReviewId", ReviewId);
                        using (var oDr = myWeb.moDbHelper.getDataReaderDisposable("spSendEmailAfterSubmitReview", CommandType.StoredProcedure, (Hashtable)arrParms))
                        {
                            if (oDr != null)
                            {
                                while (oDr.Read())
                                {
                                    XmlNode ReviewContentXML = (XmlNode)doc.CreateElement("ReviewContentXML");
                                    string cContentXmlDetail = Convert.ToString(oDr["cContentXmlDetail"]);
                                    if (!string.IsNullOrEmpty(cContentXmlDetail))
                                    {
                                        ReviewContentXML.InnerXml = cContentXmlDetail;
                                        CustomerName = Convert.ToString(ReviewContentXML.SelectSingleNode("Content/Reviewer/node()").Value);

                                        if (ReviewContentXML.SelectSingleNode("Content/ReviewerEmail/node()") != null)
                                        {
                                            strEmail = Convert.ToString(ReviewContentXML.SelectSingleNode("Content/ReviewerEmail/node()").Value);
                                            XmlElement xmlDetails = (XmlElement)doc.CreateElement("Order");
                                            xmlDetails.InnerXml = "<strUrl>" + strUrl + "</strUrl>";
                                            xmlDetails.InnerXml = xmlDetails.InnerXml + "<CustomerName>" + CustomerName + "</CustomerName>";
                                            // Send Email
                                            Cms.dbHelper argodbHelper = null;
                                            oMsg.emailer(xmlDetails, xsltPath, "INTOTHEBLUE experience completion", moMailConfig["FromEmail"], strEmail, "Thank you", odbHelper: ref argodbHelper);
                                        }
                                    }
                                }
                            }
                        }

                        return JsonResult;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ReplaceRegularExpression", ex, ""));
                        return ex.Message;
                    }
                }

                public string SaveMultipleLibraryImages(ref Protean.Rest myApi, ref Newtonsoft.Json.Linq.JObject inputJson)
                {
                    string JsonResult = "";
                    string nContentId = "";
                    string cRelatedLibraryImages = "";
                    string cSkipAttribute = "false";
                    int count = moCtx.Request.Files.Count;

                    if (moCtx.Request["contentId"] != null)
                    {
                        nContentId = moCtx.Request["contentId"];
                    }

                    if (moCtx.Request["cRelatedLibraryImages"] != null)
                    {
                        cRelatedLibraryImages = moCtx.Request["cRelatedLibraryImages"];
                    }

                    try
                    {
                        if (myApi.mbAdminMode)
                        {
                            JsonResult = myWeb.moDbHelper.CreateLibraryImages(Conversions.ToInteger(nContentId), cRelatedLibraryImages, cSkipAttribute, "LibraryImage");
                        }

                        return JsonResult;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "RedirectPage", ex, ""));
                        return ex.Message;
                    }
                }

            }
            #endregion

        }

    }
}
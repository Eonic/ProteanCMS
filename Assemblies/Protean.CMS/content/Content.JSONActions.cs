using System;
using System.Data;
using System.Web.Configuration;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using Protean.Tools.Integration.Twitter;

namespace Protean
{

    public partial class Cms
    {

        public partial class Content
        {

            #region JSON Actions

            public class JSONActions
            {

                public event OnErrorEventHandler OnError;

                public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
                private const string mcModuleName = "Eonic.Content.JSONActions";
                private System.Collections.Specialized.NameValueCollection moLmsConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/lms");
                private Cms myWeb;
                private Protean.Cms.Cart myCart;
                public System.Web.HttpContext moCtx = System.Web.HttpContext.Current;
                public string cleanUploadedPaths;
                public JSONActions()
                {
                    //string ctest = "this constructor is being hit"; // for testing
                    myWeb = new Cms();
                    myWeb.InitializeVariables();
                    myWeb.Open();
                    myCart = new Cms.Cart(ref myWeb);

                }

                public string TwitterFeed(ref Protean.rest myApi, ref Newtonsoft.Json.Linq.JObject jObj)
                {
                    try
                    {
                        //string cProcessInfo = "";
                        string JsonResult = "";
                        // Dim twtKey As String
                        // Dim twtSecret As String
                        // Dim _accessToken As String
                        // Dim _accessTokenSecret As String
                        // Dim twtSvc As New TwitterService(twtKey, twtSecret)
                        // twtSvc.AuthenticateWith(_accessToken, _accessTokenSecret)
                        // Dim uto As New ListTweetsOnUserTimelineOptions()
                        // uto.ScreenName =
                        // uto.Count = 10
                        // Dim tweets = twtSvc.ListTweetsOnUserTimeline(uto)
                        // Dim tweet As TwitterStatus
                        // ' For Each tweet In tweets
                        // ' tweet.TextAsHtml
                        // ' Next
                        // JsonResult = tweets.ToString()
                        return JsonResult;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""));
                        return ex.Message;
                    }
                }

                public string UpdateContentValue(ref Protean.rest myApi, ref Newtonsoft.Json.Linq.JObject jObj)
                {
                    try
                    {

                        // first check the user privages
                        // myApi.mnUserId

                        string JsonResult = "";
                        //string contentId = "";
                        //string xpath = "";
                        // Dim value As String 'JSON convert to XML and save ensure the xml schemas match.

                        return JsonResult;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""));
                        return ex.Message;
                    }
                }

                public string GetContent(ref Protean.rest myApi, ref Newtonsoft.Json.Linq.JObject jObj)
                {
                    try
                    {

                        string JsonResult = "";
                        return JsonResult;
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""));
                        return ex.Message;
                    }
                }

                public string IsUnique(ref Protean.rest myApi, ref Newtonsoft.Json.Linq.JObject jObj)
                {
                    try
                    {
                        // first check the user is validated we do not want this open to non admin users.
                        // Dim cContentType As String
                        // Dim cTableName As String
                        // Dim xPath As String
                        // Dim DataField As String


                        string JsonResult = "";
                        return JsonResult;
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""));
                        return ex.Message;
                    }
                }

                public string SearchContent(ref Protean.rest myApi, ref Newtonsoft.Json.Linq.JObject searchFilter)
                {
                    try
                    {
                        string nRoot = (string)searchFilter["nRoot"]; // Page to search
                        string cContentType = (string)searchFilter["cContentType"]; // Comma separated list of content types
                        string cExpression = (string)searchFilter["cExpression"];
                        bool bChilds = (bool)searchFilter["bChilds"]; // Search in child pages
                        string nParId = (string)searchFilter["nParId"];
                        string bIgnoreParID = (string)searchFilter["bIgnoreParID"];
                        bool bIncRelated = (bool)searchFilter["bIncRelated"];
                        string cTableName = (string)searchFilter["cTableName"];
                        string cSelectField = (string)searchFilter["cSelectField"];
                        string cFilterField = (string)searchFilter["cFilterField"];

                        string cTmp = string.Empty;
                        if (bIncRelated == true)
                        {
                            string sSQL = "Select " + cSelectField + " From " + cTableName + " WHERE " + cFilterField + " = " + nParId;
                            using (var oDre = myWeb.moDbHelper.getDataReaderDisposable(sSQL))  // Done by nita on 6/7/22
                            {
                                while (oDre.Read())
                                    cTmp = Conversions.ToString(cTmp + Operators.ConcatenateObject(oDre[0], ","));
                                oDre.Close();
                            }
                            if (!string.IsNullOrEmpty(cTmp))
                                cTmp = Strings.Left(cTmp, Strings.Len(cTmp) - 1);
                        }

                        XmlElement searchResultXML;
                        searchResultXML = myWeb.moDbHelper.RelatedContentSearch(Conversions.ToInteger(nRoot), cContentType, bChilds, cExpression, Conversions.ToInteger(nParId), Conversions.ToInteger(Interaction.IIf(Conversions.ToBoolean(bIgnoreParID), 0, nParId)), cTmp.Split(','), bIncRelated);

                        string jsonString = JsonConvert.SerializeXmlNode(searchResultXML, Newtonsoft.Json.Formatting.Indented);
                        return jsonString.Replace("\"@", "\"_");
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "SearchContent", ex, ""));
                        return ex.Message;
                    }
                }

                // to call /ewapi/Cms.Content/SearchIndex?data={query:'driving',hitsLimit:'30'}

                public string SearchIndex(ref Protean.rest myApi, ref Newtonsoft.Json.Linq.JObject searchFilter)
                {
                    try
                    {
                        string SearchString = "";
                        int HitsLimit = 50;
                        string fuzzySearch = "";
                        if (searchFilter != null)
                        {
                            SearchString = (string)searchFilter["query"];
                            fuzzySearch = (string)searchFilter["fuzzysearch"];
                            if ((string)searchFilter["hitslimit"] != "")
                            {
                                HitsLimit = (int)searchFilter["hitslimit"];
                            }
                        }


                        var oSrch = new Cms.Search(ref myApi);
                        var oResultsXml = new XmlDocument();
                        oResultsXml.AppendChild(oResultsXml.CreateElement("Results"));
                        oSrch.moContextNode = (XmlElement)oResultsXml.FirstChild;
                        oSrch.IndexQuery(ref myApi, SearchString, HitsLimit, fuzzySearch);
                        string jsonString = JsonConvert.SerializeXmlNode(oResultsXml, Newtonsoft.Json.Formatting.Indented);
                        jsonString = jsonString.Replace("/*?xml:namespace prefix = o ns = \"urn:schemas-microsoft-com:office:office\" /*/", "");
                        return jsonString.Replace("\"@", "\"_");
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "SearchContent", ex, ""));
                        return ex.Message;
                    }
                }


                public string LogActivity(ref Protean.rest myApi, ref Newtonsoft.Json.Linq.JObject jObj)
                {
                    try
                    {
                        var oActivityType = default(Cms.dbHelper.ActivityType);
                        switch (jObj["type"].ToString() ?? "")
                        {
                            case "PageViewed":
                                {
                                    oActivityType = Cms.dbHelper.ActivityType.PageViewed;
                                    break;
                                }
                        }
                        long nUserDirId = Conversions.ToLong("0" + jObj["userId"].ToString());
                        long nPageId = Conversions.ToLong("0" + jObj["pageId"].ToString());
                        long nArtId = Conversions.ToLong("0" + jObj["artId"].ToString());

                        if (myApi.mnUserId > 0)
                        {
                            myWeb.moDbHelper.logActivity(oActivityType, nUserDirId, nPageId, nArtId);
                        }

                        return Conversions.ToString(true);
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""));
                        return ex.Message;
                    }
                }

                // Charts
                public string GetChartData(ref Protean.rest myApi, ref Newtonsoft.Json.Linq.JObject inputJson)
                {

                    string JsonResult = "";
                    int chartContentKey;
                    if (!int.TryParse((string)inputJson["chartContentKey"], out chartContentKey))
                    {
                        chartContentKey = 0;
                    }

                    try
                    {
                        if (chartContentKey > 0)
                        {
                            DataSet dsChartData;

                            string sSql = "SELECT C.nContentKey,";
                            sSql += " C.cContentName,";
                            sSql += " Convert(Xml, C.cContentXmlBrief).value('(Content/cDisplayName)[1]', 'Varchar(50)') AS displayName,";
                            sSql += " CONVERT(XML, C.cContentXmlBrief).value('(Content/@lineColor)[1]', 'Varchar(50)') AS lineColor,";
                            sSql += " CONVERT(XML, C.cContentXmlBrief).value('(Content/@lineTension)[1]', 'Varchar(10)') AS lineTension,";
                            sSql += " CONVERT(XML, C.cContentXmlBrief).value('(Content/@label-x)[1]', 'Varchar(10)') AS xLabelPosition,";
                            sSql += " CONVERT(XML, C.cContentXmlBrief).value('(Content/@label-y)[1]', 'Varchar(10)') AS yLabelPosition,";
                            sSql += " CONVERT(XML, C.cContentXmlBrief).value('(Content/@priority)[1]', 'integer') AS priority,";
                            sSql += " '' AS url,";
                            sSql += " P.ProductId AS productId,";
                            sSql += " CD.D.value('(@x)[1]', 'Varchar(10)') AS xLoc,";
                            sSql += " CD.D.value('(@y)[1]', 'Varchar(10)') AS yLoc";
                            sSql += " FROM tblContentRelation CR";
                            sSql += " JOIN tblContent C ON C.nContentKey = CR.nContentChildId";
                            sSql += " JOIN tblAudit A ON A.nAuditKey = C.nAuditId";
                            sSql += " OUTER APPLY";
                            sSql += " (";
                            sSql += " SELECT CR1.nContentChildId AS ProductId";
                            sSql += " FROM tblContentRelation CR1";
                            sSql += " JOIN tblContent C1 ON C1.nContentKey = CR1.nContentChildId";
                            sSql += " WHERE CR1.nContentParentId = C.nContentKey AND C1.cContentSchemaName = 'Product'";
                            sSql += " ) P";
                            sSql += " OUTER APPLY (SELECT CAST(C.cContentXmlBrief as xml) as cContentXmlBriefxml) CB";
                            sSql += " OUTER APPLY CB.cContentXmlBriefxml.nodes('/Content/dataset/datapoint') as CD(D) ";
                            sSql += " WHERE nContentParentId = " + chartContentKey + " AND C.cContentSchemaName = 'ChartDataSet'";
                            sSql += " AND A.nStatus = 1 ORDER BY priority, C.nContentKey";

                            dsChartData = myWeb.moDbHelper.GetDataSet(sSql, "ChartDataSet", "Chart");

                            if (dsChartData != null)
                            {
                                // Update the contentUrls
                                if (dsChartData.Tables.Count > 0)
                                {
                                    foreach (DataRow oRow in dsChartData.Tables[0].Rows)
                                    {
                                        if (oRow["productId"] != null & !ReferenceEquals(oRow["productId"], DBNull.Value))
                                        {
                                            oRow["url"] = myWeb.GetContentUrl(Conversions.ToLong(oRow["productId"]));
                                        }
                                    }
                                }

                                string chartXml = dsChartData.GetXml();
                                var xmlDoc = new XmlDocument();
                                xmlDoc.LoadXml(chartXml);

                                string jsonString = JsonConvert.SerializeXmlNode(xmlDoc.DocumentElement, Newtonsoft.Json.Formatting.Indented);
                                return jsonString.Replace("\"@", "\"_");
                            }

                            return string.Empty;

                        }
                        return JsonResult;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""));
                        return ex.Message;
                    }

                }


                // Review Path
                public string ImageUpload(ref Protean.rest myApi, ref Newtonsoft.Json.Linq.JObject jObj)
                {
                    try
                    {

                        var moFSHelper = new Protean.fsHelper(moCtx);

                        string cPageContentId = string.Empty;
                        string cContentName = string.Empty;
                        string uploadedfiles = string.Empty;
                        string JsonResult = string.Empty;

                        string encryptedContentId = Conversions.ToString(myApi.moSession["contentId"]);  // rename this to contentId
                        string UploadDirPath = string.Empty;

                        if (jObj != null)
                        {
                            if (jObj["contentId"] != null)
                            {
                                cPageContentId =Convert.ToString(jObj["contentId"]);
                            }
                            if (jObj["cContentName"] != null)
                            {
                                cContentName =Convert.ToString(jObj["cContentName"]);
                            }
                            if (jObj["uploadFiles"] != null)
                            {
                                uploadedfiles = Convert.ToString(jObj["uploadFiles"]);
                            }
                            if (jObj["storagePath"] != null)
                            {
                                UploadDirPath = Convert.ToString(jObj["storagePath"]);
                            }
                        }
                        else
                        {
                            cPageContentId = moCtx.Request["contentId"];
                            UploadDirPath = moCtx.Request["storagePath"];
                        }

                        if (moCtx.Request.Files.Count > 0)
                        {
                            if ((encryptedContentId ?? "") == (cPageContentId ?? ""))
                            {
                                moFSHelper.initialiseVariables(Protean.fsHelper.LibraryType.Image);
                                if (cContentName != null)
                                {
                                    string cleanPathName = moFSHelper.UploadRequest(moCtx, UploadDirPath);
                                    moCtx.Session["lastUploadedFilePath"] = cleanPathName;
                                    // JsonResult = JsonConvert.SerializeObject(cleanPathName)
                                }
                                moFSHelper = (Protean.fsHelper)null;
                            }
                        }
                        return JsonResult;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ReviewImagePath", ex, ""));
                        return ex.Message;
                    }
                }
                public string GetLastUploadedFilePath(ref Protean.rest myApi, ref Newtonsoft.Json.Linq.JObject jObj)
                {
                    try
                    {
                        return Conversions.ToString(moCtx.Session["lastUploadedFilePath"]);
                    }
                    catch
                    {
                    }
                    finally
                    {
                        moCtx.Session["lastUploadedFilePath"] = null;
                    }

                    return default;


                }



            }



            #endregion

        }

    }
}
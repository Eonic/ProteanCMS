using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web.Configuration;
using System.Xml;
using DocumentFormat.OpenXml.Office2010.Ink;
using Microsoft.Ajax.Utilities;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                private System.Collections.Specialized.NameValueCollection moWebConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
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
                        long newContentId = 0;
                        if (myApi.mbAdminMode)
                        {
                            // Extract fields from JSON
                            long contentId = Convert.ToInt64(jObj["contentId"] ?? 0);
                            string contentType = Convert.ToString(jObj["contentType"]);
                            string ContentName = Convert.ToString(jObj["ContentName"]);
                            JArray values = (JArray)jObj["values"];

                            long pageId = Convert.ToInt64(jObj["pageId"] ?? 0);
                            string position = Convert.ToString(jObj["position"]);
                            string relatedParent = Convert.ToString(jObj["relatedParent"]);
                            string relationType = Convert.ToString(jObj["relationType"]);

                            XmlElement oContentInstance;
                            Cms.Admin.AdminXforms moXform = null;
                            string xRootBriefPath = "tblContent/cContentXmlBrief/";
                            string xRootDetailPath = "tblContent/cContentXmlDetail";


                            if (contentId > 0)
                            {
                                // UPDATE existing content
                                oContentInstance = myWeb.moDbHelper.moPageXml.CreateElement("instance");
                                oContentInstance.InnerXml = myWeb.moDbHelper.getObjectInstance(Protean.Cms.dbHelper.objectTypes.Content, contentId);
                                ApplyValuesToXml(oContentInstance, values, xRootBriefPath, xRootDetailPath);
                                newContentId = Convert.ToInt64(myWeb.moDbHelper.setObjectInstance(Protean.Cms.dbHelper.objectTypes.Content, oContentInstance));
                            }
                            else
                            {
                                // INSERT new content
                                moXform = (Admin.AdminXforms)myWeb.getAdminXform();
                                string xformPath = moXform.GetContentFormPath(contentType);
                                moXform.load(xformPath + ".xml", myWeb.maCommonFolders);

                                if (moXform.Instance != null)
                                {
                                    moXform.Instance.SelectSingleNode("tblContent/cContentName").InnerText = ContentName;
                                    moXform.Instance.SelectSingleNode("tblContent/dPublishDate").InnerText = Protean.Tools.Xml.XmlDate(DateTime.Now);
                                    ApplyValuesToXml(moXform.Instance, values, xRootBriefPath, xRootDetailPath);
                                    // Save and link to page or parent
                                    newContentId = Convert.ToInt64(myWeb.moDbHelper.setObjectInstance(Protean.Cms.dbHelper.objectTypes.Content, moXform.Instance, 0));
                                    if (newContentId > 0)
                                    {
                                        if (pageId > 0)
                                        {
                                            myWeb.moDbHelper.setContentLocation(pageId, newContentId, true, false, false, position, false);
                                        }
                                        else if (!string.IsNullOrEmpty(relatedParent))
                                        {
                                            myWeb.moDbHelper.insertContentRelation(Convert.ToInt32(relatedParent), Convert.ToString(newContentId), false, relationType ?? "default");
                                        }
                                    }
                                }
                            }
                        }
                        return $"{{ \"id\": \"{newContentId}\" }}";

                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs("Content.JsonActions", "UpdateContentValue", ex, ""));
                        return $"{{ \"error\": \"{ex.Message}\" }}";
                    }
                }
                private void ApplyValuesToXml(XmlElement oContentInstance, JArray values, string xRootBriefPath, string xRootDetailPath)
                {
                    foreach (var data in values)
                    {
                        var item = (JObject)data;
                        string xpath = item["xpath"]?.ToString() ?? "";
                        string value = item["value"]?.ToString() ?? "";
                        int mode = item["mode"]?.ToObject<int>() ?? 3;

                        string fullXPath = xpath;

                        if (mode == 1)
                        {
                            fullXPath = xRootBriefPath + xpath;
                            XmlNode node = oContentInstance.SelectSingleNode(fullXPath);
                            if (node != null) node.InnerXml = value;
                        }
                        else if (mode == 2)
                        {
                            fullXPath = xRootDetailPath + xpath;
                            XmlNode node = oContentInstance.SelectSingleNode(fullXPath);
                            if (node != null) node.InnerXml = value;
                        }
                        else if (mode == 3)
                        {
                            XmlNode node = oContentInstance.SelectSingleNode(xRootBriefPath + xpath);
                            if (node != null) node.InnerXml = value;
                            XmlNode node1 = oContentInstance.SelectSingleNode(xRootDetailPath + xpath);
                            if (node1 != null)
                            {
                                node1.InnerXml = value;
                            }
                            else
                            {
                                node1 = oContentInstance.SelectSingleNode(xRootDetailPath);
                                node1.InnerXml = value;
                            }
                        }
                    }
                }

                public string GetGeoContent(ref Protean.rest myApi, ref Newtonsoft.Json.Linq.JObject jObj)
                {
                    try
                    {
                        string cSelectField = (string)jObj["cSchemaName"];
                        string swLat = (string)jObj["swLat"];
                        string swLng = (string)jObj["swLng"];
                        string neLat = (string)jObj["nwLat"];
                        string neLng = (string)jObj["nwLng"];

                        string latIdx = (string)jObj["latIdx"];
                        string lngIdx = (string)jObj["lngIdx"];

                        string bBox = (string)jObj["bBox"];


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

                // to call /ewapi/Cms.Content/SearchIndex?data={query:'driving',hitslimit:'30'}
                // in postman path ewapi/Cms.Content/SearchIndex/ set to post and raw json is {"query":"jack","hitlimits":"30","fuzzysearch":"on"}

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
                            if ((string)searchFilter["hitslimit"] != null)
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
                                cPageContentId = Convert.ToString(jObj["contentId"]);
                            }
                            if (jObj["cContentName"] != null)
                            {
                                cContentName = Convert.ToString(jObj["cContentName"]);
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
                        if (moCtx.Session["lastUploadedFilePath"] != null)
                        {
                            return Conversions.ToString(moCtx.Session["lastUploadedFilePath"]);
                        }
                        else
                        {
                            return "No Filepath Stored";
                        }

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





                public string ConvertXFormToJSON(ref Protean.rest myApi, ref Newtonsoft.Json.Linq.JObject jObj)
                {
                    try
                    {
                        // Validate input
                        if (jObj == null || !jObj.ContainsKey("xFormXml"))
                        {
                            throw new ArgumentException("Missing xFormXml input in the JSON request.");
                        }

                        // Extract XML string from JSON
                        string xFormXml = jObj["xFormXml"].ToString();

                        // Load XML document
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(xFormXml);

                        // Wrap specific HTML tags in CDATA
                        WrapHtmlTagsInCData(xmlDoc, new[] { "div", "span", "p" });

                        // Convert XML to JSON
                        string jsonString = JsonConvert.SerializeXmlNode(xmlDoc, Newtonsoft.Json.Formatting.Indented);

                        return jsonString;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ConvertXFormToJSON", ex, ""));
                        return JsonConvert.SerializeObject(new { error = ex.Message });
                    }
                }

                public string ConvertJSONToXForm(ref Protean.rest myApi, ref Newtonsoft.Json.Linq.JObject jObj)
                {
                    try
                    {
                        // Validate input
                        if (jObj == null || !jObj.ContainsKey("xFormJson"))
                        {
                            throw new ArgumentException("Missing xFormJson input in the JSON request.");
                        }

                        // Extract JSON string from the input object
                        string xFormJson = jObj["xFormJson"].ToString();

                        // Convert JSON string to XML
                        XmlDocument xmlDoc = JsonConvert.DeserializeXmlNode(xFormJson, "form");

                        // Return the XML as a string
                        using (StringWriter stringWriter = new StringWriter())
                        using (XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter))
                        {
                            xmlDoc.WriteTo(xmlTextWriter);
                            return stringWriter.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ConvertJSONToXForm", ex, ""));
                        return JsonConvert.SerializeObject(new { error = ex.Message });
                    }
                }

                private void WrapHtmlTagsInCData(XmlDocument xmlDoc, string[] tagsToWrap)
                {
                    foreach (string tag in tagsToWrap)
                    {
                        XmlNodeList nodes = xmlDoc.GetElementsByTagName(tag);
                        foreach (XmlNode node in nodes)
                        {
                            if (node.InnerText != null && !node.InnerText.StartsWith("<![CDATA["))
                            {
                                // Wrap content in CDATA
                                node.InnerXml = $"<![CDATA[{node.InnerXml}]]>";
                            }
                        }
                    }
                }

                public string GetGoogleReviews(ref Protean.rest myApi, ref Newtonsoft.Json.Linq.JObject apiUrl)
                {
                    string jsonResult = string.Empty;
                    XmlElement cReviewNode = myWeb.moPageXml.CreateElement("GoogleReview");

                    try
                    {
                        if (moWebConfig["PlaceId"] != null && moWebConfig["PlaceId"] != "" &&
                            moWebConfig["GoogleReviewAPIKey"] != null && moWebConfig["GoogleReviewAPIKey"] != "")
                        {
                            string placeId = moWebConfig["PlaceId"].ToString();
                            string apiKey = moWebConfig["GoogleReviewAPIKey"].ToString();

                            string cUrl = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={placeId}&fields=name,rating,user_ratings_total,reviews&key={apiKey}";

                            var request = WebRequest.Create(cUrl);
                            using (var response = request.GetResponse())
                            {
                                if (response != null)
                                {
                                    using (var content = response.GetResponseStream())
                                    using (var reader = new StreamReader(content))
                                    {
                                        var jsonString = reader.ReadToEnd();
                                        var json = JObject.Parse(jsonString);

                                        //  Add total review count
                                        var totalCount = json["result"]?["user_ratings_total"]?.ToString() ?? "0";
                                        XmlElement totalNode = myWeb.moPageXml.CreateElement("TotalReviewCount");
                                        totalNode.InnerText = totalCount;
                                        cReviewNode.AppendChild(totalNode);

                                        var avgRating = json["result"]?["rating"]?.ToString() ?? "0";
                                        XmlElement avgRatingNode = myWeb.moPageXml.CreateElement("AverageRating");
                                        avgRatingNode.InnerText = avgRating;
                                        cReviewNode.AppendChild(avgRatingNode);

                                        var allReviews = json["result"]?["reviews"];
                                        if (allReviews != null)
                                        {
                                            foreach (var r in allReviews)
                                            {
                                                XmlElement cContentNode = myWeb.moPageXml.CreateElement("Content");

                                                cContentNode.SetAttribute("name", r["author_name"]?.ToString() ?? "");
                                                cContentNode.SetAttribute("type", "Review");
                                                cContentNode.SetAttribute("status", "1");
                                                cContentNode.SetAttribute("parId", myApi.mnPageId.ToString());
                                                cContentNode.SetAttribute("showRelated", "Tag");

                                                XmlElement reviewer = myWeb.moPageXml.CreateElement("Reviewer");
                                                reviewer.InnerText = r["author_name"]?.ToString() ?? "";

                                                XmlElement reviewDate = myWeb.moPageXml.CreateElement("ReviewDate");
                                                reviewDate.InnerText = r["relative_time_description"]?.ToString() ?? "";

                                                XmlElement url = myWeb.moPageXml.CreateElement("Url");
                                                url.InnerText = r["author_url"]?.ToString() ?? "";

                                                XmlElement summary = myWeb.moPageXml.CreateElement("Summary");
                                                summary.InnerText = r["text"]?.ToString() ?? "";

                                                XmlElement description = myWeb.moPageXml.CreateElement("Description");
                                                description.InnerText = r["text"]?.ToString() ?? "";

                                                XmlElement rating = myWeb.moPageXml.CreateElement("Rating");
                                                rating.InnerText = r["rating"]?.ToString() ?? "";

                                                XmlElement images = myWeb.moPageXml.CreateElement("Images");
                                                string profilePhotoUrl = r["profile_photo_url"]?.ToString();
                                                if (!string.IsNullOrEmpty(profilePhotoUrl))
                                                {
                                                    XmlElement imgThumb = myWeb.moPageXml.CreateElement("img");
                                                    imgThumb.SetAttribute("src", profilePhotoUrl);
                                                    imgThumb.SetAttribute("width", "80");
                                                    imgThumb.SetAttribute("height", "80");
                                                    imgThumb.SetAttribute("class", "thumbnail");
                                                    images.AppendChild(imgThumb);
                                                }

                                                cContentNode.AppendChild(reviewer);
                                                cContentNode.AppendChild(reviewDate);
                                                cContentNode.AppendChild(url);
                                                cContentNode.AppendChild(summary);
                                                cContentNode.AppendChild(description);
                                                cContentNode.AppendChild(rating);
                                                cContentNode.AppendChild(images);
                                                cReviewNode.AppendChild(cContentNode);
                                            }
                                        }

                                        // ✅ Add rating limit
                                        XmlElement cRatingLimit = myWeb.moPageXml.CreateElement("RatingLimit");
                                        string limit = moWebConfig["ReviewRatingLimit"]?.ToString() ?? "0";
                                        cRatingLimit.SetAttribute("ratingLimit", limit);
                                        cReviewNode.AppendChild(cRatingLimit);
                                    }
                                }
                            }

                            jsonResult = JsonConvert.SerializeXmlNode(cReviewNode, Newtonsoft.Json.Formatting.Indented);
                            jsonResult = jsonResult.Replace("\"@", "\"_");
                            return jsonResult;
                        }
                    }
                    catch (Exception ex)
                    {
                        jsonResult = JsonConvert.SerializeObject(new { error = ex.Message });
                    }

                    return jsonResult;
                }


                //public string GetGoogleReviews(ref Protean.rest myApi, ref Newtonsoft.Json.Linq.JObject apiUrl)
                //{
                //    string jsonResult = string.Empty;
                //    XmlElement cReviewNode = myWeb.moPageXml.CreateElement("GoogleReview");

                //    try
                //    {
                //        string cGoogleReviewAPIKey = string.Empty;

                //        //if (apiUrl.ContainsKey("apiurl"))
                //        //{
                //        //    cGoogleReviewAPIKey = apiUrl["apiurl"]?.ToString();
                //        //}

                //        //if (!string.IsNullOrEmpty(cGoogleReviewAPIKey))
                //        //{
                //            if (moWebConfig["PlaceId"] != null && moWebConfig["PlaceId"] != "" && moWebConfig["GoogleReviewAPIKey"] != null && moWebConfig["GoogleReviewAPIKey"] != "")
                //            {

                //                string cUrl = "https://maps.googleapis.com/maps/api/place/details/json?place_id=" + moWebConfig["PlaceId"].ToString() + "& fields=name,rating,user_ratings_total,reviews&key=" + moWebConfig["GoogleReviewAPIKey"].ToString();
                //                var request = WebRequest.Create(cUrl);
                //                using (var response = request.GetResponse())
                //                {
                //                    if (response != null)
                //                    {
                //                        using (var content = response.GetResponseStream())
                //                        using (var reader = new StreamReader(content))
                //                        {
                //                            var jsonString = reader.ReadToEnd();

                //                            var json = JObject.Parse(jsonString);
                //                            var reviews = json["result"]?["reviews"];
                //                            if (reviews == null) return "";



                //                            foreach (var r in reviews)
                //                            {
                //                                XmlElement cContentNode = myWeb.moPageXml.CreateElement("Content");

                //                                cContentNode.SetAttribute("id", "");
                //                                cContentNode.SetAttribute("ref", "");
                //                                cContentNode.SetAttribute("name", r["author_name"]?.ToString() ?? "");
                //                                cContentNode.SetAttribute("type", "Review");
                //                                cContentNode.SetAttribute("status", "1");
                //                                cContentNode.SetAttribute("publish", "");
                //                                cContentNode.SetAttribute("owner", "0");
                //                                cContentNode.SetAttribute("parId", myApi.mnPageId.ToString());
                //                                cContentNode.SetAttribute("showRelated", "Tag");
                //                                cContentNode.SetAttribute("Intro", "");
                //                                cContentNode.SetAttribute("rtype", "");

                //                                XmlElement reviewer = myWeb.moPageXml.CreateElement("Reviewer");
                //                                reviewer.InnerText = r["author_name"]?.ToString() ?? "";

                //                                XmlElement reviewDate = myWeb.moPageXml.CreateElement("ReviewDate");
                //                                reviewDate.InnerText = r["relative_time_description"]?.ToString() ?? "";

                //                                XmlElement url = myWeb.moPageXml.CreateElement("Url");
                //                                url.InnerText = r["author_url"]?.ToString() ?? "";

                //                                XmlElement summary = myWeb.moPageXml.CreateElement("Summary");
                //                                summary.InnerText = r["text"]?.ToString() ?? "";

                //                                XmlElement description = myWeb.moPageXml.CreateElement("Description");
                //                                description.InnerText = r["text"]?.ToString() ?? "";

                //                                XmlElement rating = myWeb.moPageXml.CreateElement("Rating");
                //                                rating.InnerText = r["rating"]?.ToString() ?? "";
                //                                //Image
                //                                XmlElement images = myWeb.moPageXml.CreateElement("Images");
                //                                string profilePhotoUrl = r["profile_photo_url"]?.ToString();
                //                                if (!string.IsNullOrEmpty(profilePhotoUrl))
                //                                {
                //                                    XmlElement imgThumb = myWeb.moPageXml.CreateElement("img");
                //                                    imgThumb.SetAttribute("src", profilePhotoUrl);
                //                                    imgThumb.SetAttribute("width", "80");
                //                                    imgThumb.SetAttribute("height", "80");
                //                                    imgThumb.SetAttribute("alt", "");
                //                                    imgThumb.SetAttribute("class", "thumbnail");
                //                                    imgThumb.SetAttribute("type", "thumbnail");
                //                                    images.AppendChild(imgThumb);
                //                                }

                //                                XmlElement path = myWeb.moPageXml.CreateElement("Path");
                //                                XmlElement emailSent = myWeb.moPageXml.CreateElement("EmailSent");
                //                                emailSent.InnerText = "False";
                //                                XmlElement showImage = myWeb.moPageXml.CreateElement("ShowImage");
                //                                showImage.InnerText = string.IsNullOrEmpty(profilePhotoUrl) ? "False" : "True";
                //                                XmlElement topReview = myWeb.moPageXml.CreateElement("TopReview");
                //                                XmlElement reviewSinceDate = myWeb.moPageXml.CreateElement("reviewSinceDate");
                //                                reviewSinceDate.InnerText = r["relative_time_description"]?.ToString() ?? "";

                //                                cContentNode.AppendChild(reviewer);
                //                                cContentNode.AppendChild(reviewDate);
                //                                cContentNode.AppendChild(url);
                //                                cContentNode.AppendChild(summary);
                //                                cContentNode.AppendChild(description);
                //                                cContentNode.AppendChild(rating);
                //                                cContentNode.AppendChild(images);
                //                                cContentNode.AppendChild(path);
                //                                cContentNode.AppendChild(emailSent);
                //                                cContentNode.AppendChild(showImage);
                //                                cContentNode.AppendChild(topReview);
                //                                cContentNode.AppendChild(reviewSinceDate);
                //                                cReviewNode.AppendChild(cContentNode);
                //                            }
                //                            XmlElement cRatingLimit = myWeb.moPageXml.CreateElement("RatingLimit");
                //                            if (moWebConfig["ReviewRatingLimit"] != null && moWebConfig["ReviewRatingLimit"] != "")
                //                            {
                //                                cRatingLimit.SetAttribute("ratingLimit", moWebConfig["ReviewRatingLimit"].ToString());
                //                            }
                //                            else
                //                            {
                //                                cRatingLimit.SetAttribute("ratingLimit", "0");
                //                            }
                //                            cReviewNode.AppendChild(cRatingLimit);
                //                        }
                //                    }
                //                }
                //                jsonResult = JsonConvert.SerializeXmlNode(cReviewNode, Newtonsoft.Json.Formatting.Indented);
                //                jsonResult = jsonResult.Replace("\"@", "\"_");
                //                return jsonResult;
                //            }
                //        //}
                //    }
                //    catch (Exception ex)
                //    {
                //        // Optional: log or handle error
                //        jsonResult = JsonConvert.SerializeObject(new { error = ex.Message });
                //    }

                //    return jsonResult;
                //}

                #endregion
            }
        }

    }
}
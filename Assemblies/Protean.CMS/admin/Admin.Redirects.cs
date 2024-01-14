using System;
using System.Collections;
using System.Xml;
using Microsoft.VisualBasic.CompilerServices;

namespace Protean
{

    public partial class Cms
    {
        public partial class Admin
        {
            public class Redirects
            {
                public event OnErrorEventHandler OnError;

                public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
                private const string mcModuleName = "Cms.Admin.Redirects";
                private Cms myWeb;
                private Cms.Cart myCart;
                public Cms.xForm moAdXfm;
                public Cms.dbHelper moDbHelper;

                public enum RedirectType
                {
                    Redirect301 = 301,
                    Redirect302 = 302,
                    Redirect404 = 404
                }

                public Redirects()
                {
                    //string ctest = "this constructor is being hit"; // for testing
                    myWeb = new Cms();
                    myWeb.InitializeVariables();
                    myWeb.Open();
                    myCart = new Cms.Cart(ref myWeb);
                    moAdXfm = new Cms.xForm(ref myWeb);
                    moDbHelper = myWeb.moDbHelper;
                }

                public string CreateRedirect(ref string redirectType, ref string OldUrl, ref string NewUrl, string hiddenOldUrl = "", int pageId = 0, string isParentPage = "false")
                {

                    try
                    {

                        var rewriteXml = new XmlDocument();
                        string Result = "success";
                        rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"));
                        string oCgfSectPath = "rewriteMaps/rewriteMap[@name='" + redirectType + "']";
                        var redirectSectionXmlNode = rewriteXml.SelectSingleNode(oCgfSectPath);
                        // 'Check we do not have a redirect for the OLD URL allready. Remove if exists
                        XmlNodeList existingRedirectsForOldUrlAsKey;
                        XmlNodeList existingRedirectsForOldUrlAsValue;
                        XmlNodeList existingRedirectsForNewUrlAsKey;
                        XmlNodeList existingRedirectsForNewUrlAsValue;
                        if (string.IsNullOrEmpty(hiddenOldUrl))
                        {
                            existingRedirectsForOldUrlAsKey = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" + redirectType + "']/add[@key='" + OldUrl + "']");
                            existingRedirectsForOldUrlAsValue = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" + redirectType + "']/add[@value='" + OldUrl + "']");

                            existingRedirectsForNewUrlAsKey = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" + redirectType + "']/add[@key='" + NewUrl + "']");
                        }
                        // existingRedirectsForOldUrlAsValue = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" & redirectType & "']/add[@value='" & OldUrl & "']")

                        else
                        {
                            existingRedirectsForOldUrlAsKey = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" + redirectType + "']/add[@key='" + hiddenOldUrl + "']");
                            existingRedirectsForOldUrlAsValue = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" + redirectType + "']/add[@value='" + hiddenOldUrl + "']");
                            existingRedirectsForNewUrlAsKey = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" + redirectType + "']/add[@key='" + NewUrl + "']");

                        }

                        if (existingRedirectsForOldUrlAsKey.Count > 0 | existingRedirectsForOldUrlAsValue.Count > 0)
                        {
                            if (existingRedirectsForOldUrlAsKey.Count > 0)
                            {
                                foreach (XmlNode existingNode in existingRedirectsForOldUrlAsKey)
                                {
                                    var newNode = existingNode;
                                    newNode.Attributes.Item(0).InnerXml = OldUrl;
                                    newNode.Attributes.Item(1).InnerXml = NewUrl;
                                    rewriteXml.Save(myWeb.goServer.MapPath("/rewriteMaps.config"));
                                }
                            }
                            if (existingRedirectsForNewUrlAsKey.Count > 0)
                            {
                                foreach (XmlNode existingNewUrlNode in existingRedirectsForNewUrlAsKey)
                                {

                                    var newUrlNode = existingNewUrlNode;
                                    string existingNewUrl = newUrlNode.Attributes.Item(1).InnerXml;
                                    // newNode.Attributes.Item(1).InnerXml = 

                                    foreach (XmlNode existingNode in existingRedirectsForOldUrlAsKey)
                                    {

                                        var newNode = existingNode;
                                        newNode.Attributes.Item(0).InnerXml = OldUrl;
                                        newNode.Attributes.Item(1).InnerXml = existingNewUrl;
                                        rewriteXml.Save(myWeb.goServer.MapPath("/rewriteMaps.config"));
                                    }
                                }
                            }
                            if (existingRedirectsForOldUrlAsValue.Count > 0)
                            {

                                foreach (XmlNode existingNode in existingRedirectsForOldUrlAsValue)
                                {
                                    var newNode = existingNode;
                                    // newNode.Attributes.Item(0).InnerXml = OldUrl
                                    newNode.Attributes.Item(1).InnerXml = NewUrl;
                                    rewriteXml.Save(myWeb.goServer.MapPath("/rewriteMaps.config"));

                                }
                                if (existingRedirectsForOldUrlAsKey.Count == 0)
                                {
                                    if (redirectSectionXmlNode != null)
                                    {
                                        var replacingElement = rewriteXml.CreateElement("RedirectInfo");
                                        replacingElement.InnerXml = $"<add key='{OldUrl}' value='{NewUrl}'/>";
                                        rewriteXml.SelectSingleNode(oCgfSectPath).AppendChild(replacingElement.FirstChild);
                                        rewriteXml.Save(myWeb.goServer.MapPath("/rewriteMaps.config"));

                                    }
                                }
                            }
                        }
                        // Add redirect
                        else if (isParentPage.ToLower() == "false")
                        {                           
                            if (redirectSectionXmlNode != null)
                            {
                                var replacingElement = rewriteXml.CreateElement("RedirectInfo");
                                replacingElement.InnerXml = $"<add key='{OldUrl}' value='{NewUrl}'/>";

                                // rewriteXml.SelectSingleNode(oCgfSectPath).FirstChild.AppendChild(replacingElement.FirstChild)
                                rewriteXml.SelectSingleNode(oCgfSectPath).AppendChild(replacingElement.FirstChild);
                                rewriteXml.Save(myWeb.goServer.MapPath("/rewriteMaps.config"));

                            }
                        }
                        // Determine all the paths that need to be redirected
                        // If redirectType = "301Redirect" Then
                        if (pageId > 0)
                        {

                            if (isParentPage == "True")
                            {
                                switch (redirectType ?? "")
                                {
                                    case "301Redirect":
                                        {

                                            redirectType = "301 Redirects";
                                            break;
                                        }

                                    case "302Redirect":
                                        {
                                            redirectType = "302 Redirects";
                                            break;
                                        }

                                    case "Redirect404":
                                        {
                                            redirectType = "404 Redirects";
                                            break;
                                        }

                                    default:
                                        {
                                            break;
                                        }

                                }

                                // step through and create rules to deal with paths
                                var folderRules = new ArrayList();
                                var rulesXml = new XmlDocument();
                                rulesXml.Load(myWeb.goServer.MapPath("/RewriteRules.config"));
                                XmlElement insertAfterElment = (XmlElement)rulesXml.SelectSingleNode("descendant-or-self::rule[@name='EW: " + redirectType + "']");
                                // Dim oRule As XmlElement

                                // For Each oRule In replacerNode.SelectNodes("add")
                                XmlElement CurrentRule = (XmlElement)rulesXml.SelectSingleNode("descendant-or-self::rule[@name='Folder: " + OldUrl + "']");
                                var newRule = rulesXml.CreateElement("newRule");
                                string matchString = OldUrl;
                                if (matchString.StartsWith("/"))
                                {
                                    matchString = matchString.TrimStart('/');
                                }

                                folderRules.Add("Folder: " + OldUrl);
                                newRule.InnerXml = "<rule name=\"Folder: " + OldUrl + "\"><match url=\"^" + matchString + "(.*)\"/><action type=\"Redirect\" url=\"" + NewUrl + "{R:1}\" /></rule>";
                                if (CurrentRule is null)
                                {
                                    insertAfterElment.ParentNode.InsertAfter(newRule.FirstChild, insertAfterElment);
                                }
                                else
                                {
                                    CurrentRule.ParentNode.ReplaceChild(newRule.FirstChild, CurrentRule);
                                }
                                // Next

                                // For Each oRule In rulesXml.SelectNodes("descendant-or-self::rule[starts-with(@name,'Folder: ')]")
                                // If Not folderRules.Contains(oRule.GetAttribute("name")) Then
                                // oRule.ParentNode.RemoveChild(oRule)
                                // End If
                                // Next

                                rulesXml.Save(myWeb.goServer.MapPath("/RewriteRules.config"));
                                myWeb.bRestartApp = true;
                            }
                        }

                        return Result;
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "CreateRedirect", ex, ""));
                        return ex.Message;
                    }

                }


                public string LoadUrlsForPegination(ref string redirectType, ref int pageloadCount)
                {
                    try
                    {

                        string Result = "";
                        var rewriteXml = new XmlDocument();
                        rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"));

                        //string oCgfSectName = "system.webServer";
                        string oCgfSectPath = "rewriteMaps/rewriteMap[@name='" + redirectType + "']";
                        var props = rewriteXml.SelectSingleNode(oCgfSectPath);

                        if (rewriteXml.SelectSingleNode(oCgfSectPath) != null)
                        {

                            int PerPageCount = 50;
                            int TotalCount = 0;
                            int skipRecords = 0;
                            if (myWeb.moSession["loadCount"] is null)
                            {
                                if (PerPageCount >= props.ChildNodes.Count)
                                {
                                    myWeb.moSession["loadCount"] = (object)props.ChildNodes.Count;

                                }
                                // Add Value To Session first time When load 50 records. changed by nita on 29march22
                                myWeb.moSession["loadCount"] = (object)(Convert.ToInt32(myWeb.moSession["loadCount"]) + PerPageCount);
                            }
                            else if (pageloadCount == 0)
                            {

                                myWeb.moSession["loadCount"] = (object)PerPageCount;
                                moAdXfm.goSession["oTempInstance"] = (object)null;
                            }

                            else
                            {
                                skipRecords = Convert.ToInt32(myWeb.moSession["loadCount"]);
                                myWeb.moSession["loadCount"] = (object)(Convert.ToInt32(myWeb.moSession["loadCount"]) + PerPageCount);
                            }

                            int takeRecord = PerPageCount;

                            TotalCount = props.ChildNodes.Count;

                            if (props.ChildNodes.Count >= PerPageCount)
                            {
                                string xmlstring = "<rewriteMap name='" + redirectType + "'>";
                                string xmlstringend = "</rewriteMap>";

                                //int count = 0;

                                for (int i = skipRecords, loopTo = props.ChildNodes.Count - 1; i <= loopTo; i++)
                                {
                                    if (i > skipRecords + takeRecord - 1)
                                    {
                                        break;
                                    }
                                    else if (props.ChildNodes[i].Name == "add")
                                    {
                                        xmlstring = xmlstring + props.ChildNodes[i].OuterXml;
                                    }
                                }
                                Result = xmlstring + xmlstringend;
                            }

                            else if (pageloadCount == 0)
                            {
                                Result = rewriteXml.SelectSingleNode(oCgfSectPath).OuterXml;
                            }
                            else
                            {
                                Result = "";

                            }

                        }

                        return Result;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""));
                        return ex.Message;
                    }

                }

                public string SearchUrl(ref string redirectType, ref string searchObj, ref int pageloadCount)
                {

                    try
                    {

                        string Result = "";
                        var rewriteXml = new XmlDocument();

                        rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"));

                        //string oCgfSectName = "system.webServer";
                        string oCgfSectPath = "rewriteMaps/rewriteMap[@name='" + redirectType + "']";

                        if (rewriteXml.SelectSingleNode(oCgfSectPath) != null)
                        {

                            int PerPageCount = 50;
                            int TotalCount = 0;
                            int skipRecords = 0;
                            if (myWeb.moSession["searchLoadCount"] is null)
                            {
                                myWeb.moSession["searchLoadCount"] = (object)PerPageCount;
                            }
                            else if (pageloadCount == 0)
                            {
                                myWeb.moSession["searchLoadCount"] = (object)PerPageCount;
                                moAdXfm.goSession["oTempInstance"] = (object)null;
                            }
                            else
                            {
                                skipRecords = Convert.ToInt32(myWeb.moSession["searchLoadCount"]);
                                myWeb.moSession["searchLoadCount"] = (object)(Convert.ToInt32(myWeb.moSession["searchLoadCount"]) + PerPageCount);
                            }

                            int takeRecord = PerPageCount;
                            var props = rewriteXml.SelectSingleNode(oCgfSectPath);
                            TotalCount = props.ChildNodes.Count;

                            string xmlstring = "<rewriteMap name='" + redirectType + "'>";
                            string xmlstringend = "</rewriteMap>";
                            string searchString = "<rewriteMap name='" + redirectType + "'>";

                            var searchProps = new XmlDocument();
                            //int count = 0;

                            for (int i = 0, loopTo = props.ChildNodes.Count - 1; i <= loopTo; i++)
                            {
                                if (props.ChildNodes[i].OuterXml.IndexOf(searchObj, 0, StringComparison.CurrentCultureIgnoreCase) > -1)
                                {

                                    xmlstring = xmlstring + props.ChildNodes[i].OuterXml;

                                }
                            }
                            searchProps.LoadXml(xmlstring + xmlstringend);


                            for (int i = skipRecords, loopTo1 = searchProps.ChildNodes[0].ChildNodes.Count - 1; i <= loopTo1; i++)
                            {

                                if (searchProps.ChildNodes[0].ChildNodes[i].OuterXml.IndexOf(searchObj, 0, StringComparison.CurrentCultureIgnoreCase) > -1)
                                {
                                    if (i > skipRecords + takeRecord - 1)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        searchString = searchString + searchProps.ChildNodes[0].ChildNodes[i].OuterXml;
                                    }
                                }
                            }


                            Result = searchString + xmlstringend;
                        }

                        return Result;
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""));
                        return ex.Message;
                    }

                }

                public string DeleteUrls(ref string redirectType, ref string oldUrl, ref string newUrl)
                {

                    string Result = "";
                    var rewriteXml = new XmlDocument();
                    rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"));
                    try
                    {

                        var existingRedirects = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" + redirectType + "']/add[@key='" + oldUrl + "']");
                        if (existingRedirects.Count > 0)
                        {

                            foreach (XmlNode existingNode in existingRedirects)
                            {
                                existingNode.ParentNode.RemoveChild(existingNode);
                                rewriteXml.Save(myWeb.goServer.MapPath("/rewriteMaps.config"));
                            }
                        }
                        Result = "success";
                        return Result;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""));
                        return ex.Message;
                    }
                }

                public string IsUrlPresent(ref string redirectType, ref string oldUrl)
                {
                    string Result = "";

                    var rewriteXml = new XmlDocument();
                    rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"));

                    // 'Check we do not have a redirect for the OLD URL allready. Remove if exists
                    var existingRedirectsForOldUrl = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" + redirectType + "']/add[@key='" + oldUrl + "']");
                    var existingRedirectsForNewUrl = rewriteXml.SelectNodes("rewriteMaps/rewriteMap[@name='" + redirectType + "']/add[@value='" + oldUrl + "']");

                    if (existingRedirectsForOldUrl.Count > 0 | existingRedirectsForNewUrl.Count > 0)
                    {
                        Result = "True";
                    }
                    else
                    {
                        Result = "false";
                    }
                    return Result;
                }
                public string GetTotalNumberOfUrls(ref string redirectType)
                {

                    string Result = "";
                    var rewriteXml = new XmlDocument();
                    rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"));

                    //string oCgfSectName = "system.webServer";
                    string oCgfSectPath = "rewriteMaps/rewriteMap[@name='" + redirectType + "']";

                    var props = rewriteXml.SelectSingleNode(oCgfSectPath);
                    int TotalCount = props.ChildNodes.Count;

                    Result = TotalCount.ToString();

                    return Result;
                }

                public string LoadAllurls(ref string redirectType, ref int pageloadCount, ref string flag)
                {
                    try
                    {

                        string Result = "";
                        var rewriteXml = new XmlDocument();
                        rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"));

                        //string oCgfSectName = "system.webServer";
                        string oCgfSectPath = "rewriteMaps/rewriteMap[@name='" + redirectType + "']";

                        if (rewriteXml.SelectSingleNode(oCgfSectPath) != null)
                        {

                            int PerPageCount = 50;
                            int TotalCount = 0;
                            int skipRecords = 0;



                            int takeRecord = pageloadCount;
                            var props = rewriteXml.SelectSingleNode(oCgfSectPath);
                            TotalCount = props.ChildNodes.Count;

                            if (props.ChildNodes.Count >= PerPageCount)
                            {
                                string xmlstring = "<rewriteMap name='" + redirectType + "Redirect'>";
                                string xmlstringend = "</rewriteMap>";

                                //int count = 0;

                                for (int i = skipRecords, loopTo = props.ChildNodes.Count - 1; i <= loopTo; i++)
                                {
                                    if (i > skipRecords + takeRecord - 1)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        xmlstring = xmlstring + props.ChildNodes[i].OuterXml;
                                    }

                                }
                                Result = xmlstring + xmlstringend;
                            }

                            else
                            {
                                Result = rewriteXml.SelectSingleNode(oCgfSectPath).OuterXml;
                            }

                        }

                        return Result;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""));
                        return ex.Message;
                    }

                }
                public string GetTotalNumberOfSearchUrls(ref string redirectType, ref string searchObj)
                {

                    string Result = "";
                    var rewriteXml = new XmlDocument();
                    rewriteXml.Load(myWeb.goServer.MapPath("/rewriteMaps.config"));

                    //string oCgfSectName = "system.webServer";
                    string oCgfSectPath = "rewriteMaps/rewriteMap[@name='" + redirectType + "']";

                    var props = rewriteXml.SelectSingleNode(oCgfSectPath);

                    string xmlstringend = "</rewriteMap>";
                    string searchString = "<rewriteMap name='" + redirectType + "'>";

                    var searchProps = new XmlDocument();

                    for (int i = 0, loopTo = props.ChildNodes.Count - 1; i <= loopTo; i++)
                    {
                        if (props.ChildNodes[i].OuterXml.IndexOf(searchObj, 0, StringComparison.CurrentCultureIgnoreCase) > -1)
                        {

                            searchString = searchString + props.ChildNodes[i].OuterXml;

                        }
                    }
                    searchProps.LoadXml(searchString + xmlstringend);

                    int TotalCount = searchProps.ChildNodes[0].ChildNodes.Count;

                    Result = TotalCount.ToString();

                    return Result;
                }

                public bool IsParentPage(ref int pageId)
                {

                    string Result = "";
                    if (pageId > 0)
                    {
                        Result = Conversions.ToString(moDbHelper.isParent(pageId));
                    }
                    return Conversions.ToBoolean(Result);
                }
                public string RedirectPage(ref string sRedirectType, ref string sOldUrl, ref string sNewUrl, ref string sPageUrl, bool bRedirectChildPage = false, string sType = "", int nPageId = 0)
                {

                    string result = "success";
                    if (sRedirectType != null & !string.IsNullOrEmpty(sRedirectType))
                    {

                        string sUrl = "";
                        if (myWeb.moConfig["PageURLFormat"] == "hyphens")
                        {
                            sNewUrl = sNewUrl.TrimEnd();
                            sOldUrl = sOldUrl.Replace(" ", "-");
                            sNewUrl = sNewUrl.Replace(" ", "-");
                        }
                        if (sPageUrl != null & !string.IsNullOrEmpty(sPageUrl))
                        {
                            sUrl = sPageUrl;
                            string[] arr;
                            arr = sUrl.Split('?');
                            sUrl = arr[0];
                            // sUrl = sUrl.Substring(0, sUrl.LastIndexOf("/"))
                        }

                        switch (sType ?? "")
                        {
                            case "Page":
                                {
                                    if (!string.IsNullOrEmpty(sUrl))
                                    {
                                        sNewUrl = sUrl.Replace(sOldUrl, sNewUrl);
                                        sOldUrl = sUrl;
                                    }

                                    break;
                                }

                            default:
                                {

                                    // If (sType = "Product") Then
                                    if (myWeb.moConfig["DetailPrefix"] != null & !string.IsNullOrEmpty(myWeb.moConfig["DetailPrefix"]))
                                    {
                                        string[] prefixs = myWeb.moConfig["DetailPrefix"].Split(',');
                                        string thisPrefix = "";
                                        string thisContentType = "";

                                        int i;
                                        var loopTo = prefixs.Length - 1;
                                        for (i = 0; i <= loopTo; i++)
                                        {
                                            thisPrefix = prefixs[i].Substring(0, prefixs[i].IndexOf("/"));
                                            thisContentType = prefixs[i].Substring(prefixs[i].IndexOf("/") + 1, prefixs[i].Length - prefixs[i].IndexOf("/") - 1);
                                            if ((thisContentType ?? "") == (sType ?? ""))
                                            {
                                                sNewUrl = "/" + thisPrefix + "/" + sNewUrl;
                                                sOldUrl = "/" + thisPrefix + "/" + sOldUrl;
                                            }
                                        }
                                    }

                                    else
                                    {

                                        string url = myWeb.GetContentUrl((long)nPageId);
                                        sOldUrl = sUrl + url + "/" + sOldUrl;
                                        sNewUrl = sUrl + url + "/" + sNewUrl;

                                    }
                                    if (myWeb.moConfig["TrailingSlash"] != null & myWeb.moConfig["TrailingSlash"] == "on")
                                    {
                                        sOldUrl = sOldUrl + "/";
                                        sNewUrl = sNewUrl + "/";
                                    }

                                    break;
                                }
                                // End If
                        }

                        switch (sRedirectType ?? "")
                        {
                            case "301Redirect":
                                {

                                    CreateRedirect(ref sRedirectType, ref sOldUrl, ref sNewUrl, "", nPageId, Conversions.ToString(bRedirectChildPage));
                                    break;
                                }

                            case "302Redirect":
                                {
                                    CreateRedirect(ref sRedirectType, ref sOldUrl, ref sNewUrl, "", nPageId, Conversions.ToString(bRedirectChildPage));
                                    break;
                                }

                            default:
                                {
                                    break;
                                }
                                // do nothing
                        }
                    }
                    return result;
                }
            }
        }
    }
}
using Protean.Providers.Membership;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using static Protean.stdTools;
using static Protean.Tools.Xml;
using static System.Web.HttpUtility;

namespace Protean
{

    public partial class Cms : Protean.Base, IDisposable
    {

        /// <summary>
        /// 
        /// </summary>
        public virtual void AddCurrency()
        {
            string sProcessInfo = "PerfMon";
            PerfMon.Log("Web", "AddCurrency");
            // Isolated function to provide the facility to overload the cart when called from an overloaded .web
            try
            {
                if (gbCart)
                {

                    sProcessInfo = "Begin AddCurrency";
                    if (moCart is null)
                    {
                        var argaWeb = this;
                        moCart = new Cms.Cart(ref argaWeb);
                    }
                    moCart.SelectCurrency();
                    sProcessInfo = "End AddCurrency";

                }
            }
            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddCurrency", ex, sProcessInfo));
            }
        }

        public virtual void InitialiseCart()
        {
            string sProcessInfo = "PerfMon";
            PerfMon.Log("Web", "addCart");
            // Isolated function to provide the facility to overload the cart when called from an overloaded .web
            try
            {
                if (gbCart)
                {
                    if (moSession != null)
                    {
                        if (moCart is null)
                        {
                            // we should not hit this because addCurrency should establish it.
                            var argaWeb = this;
                            moCart = new Cms.Cart(ref argaWeb);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "addCart", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddCart", ex, sProcessInfo));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void AddCart()
        {
            string sProcessInfo = "PerfMon";
            PerfMon.Log("Web", "addCart");
            // Isolated function to provide the facility to overload the cart when called from an overloaded .web
            try
            {
                if (gbCart)
                {

                    sProcessInfo = "Begin Cart";
                    if (moCart is null)
                    {
                        // we should not hit this because addCurrency should establish it.
                        var argaWeb = this;
                        moCart = new Cms.Cart(ref argaWeb);
                    }
                    // reinitialize variables because we might've changed some
                    moCart.InitializeVariables();

                    moCart.apply();
                    // get any discount information for this page
                    XmlElement RootElmt = moPageXml.DocumentElement;
                    if (moDiscount != null)
                    {
                        moDiscount.getAvailableDiscounts(ref RootElmt);
                    }
                    sProcessInfo = "End Cart";
                }
            }
            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "addCart", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddCart", ex, sProcessInfo));
            }
        }

        public virtual void ProcessReports()
        {
            string sProcessInfo = "PerfMon";
            PerfMon.Log("Web", "ProcessReports");
            // Isolated function to provide the facility to overload the cart when called from an overloaded .web
            try
            {
                if (gbReport)
                {
                    sProcessInfo = "Begin Report";
                    var argaWeb = this;
                    var oEr = new Cms.Report(ref argaWeb);
                    oEr.apply();
                    oEr.close();
                    oEr = (Cms.Report)null;
                    sProcessInfo = "End Report";
                }
            }
            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "ProcessReports", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ProcessReports", ex, sProcessInfo));

            }
        }

        public virtual void ProcessCalendar()
        {
            string sProcessInfo = "PerfMon";
            PerfMon.Log("Web", "ProcessCalendar");
            try
            {
                sProcessInfo = "Begin Calendar";
                var argaWeb = this;
                moCalendar = new Cms.Calendar(ref argaWeb);
                moCalendar.apply();
                moCalendar = (Cms.Calendar)null;
                sProcessInfo = "End Calendar";
            }
            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ProcessCalendar", ex, sProcessInfo));
            }
        }


        private void ProcessPolls()
        {
            PerfMon.Log("Web", "ProcessPolls");

            string sProcessInfo = "PerfMon";
            string sPollField = "";
            string sPollItemField = "";
            string sPollId = "";
            string sTable = "";
            string sSql = "";
            string sVoteFrequency = "";
            var dCurrentVotesExpiryDate = default(DateTime);
            var dPreviousVotesCreationDate = default(DateTime);
            bool bVoteOnce = false;
            string sVoteIdentifiers = "";

            string sEmail = "";
            string cValidationError = "";

            // Dim oDr As SqlDataReader

            bool bUseUserId = false;
            bool bUseCookies = false;
            bool bUseEmail = false;
            bool bUseIPAddress = false;

            bool bCanVote = true;
            var nVoteBlockReason = PollBlockReason.None;
            string cCookieName = "";
            string cLastVotedSql = string.Empty;

            var openDate = DateTime.MinValue;
            var closeDate = DateTime.MaxValue;


            bool bHasVoted = false;

            try
            {
                sProcessInfo = "Begin Poll Processing";

                foreach (XmlElement ocNode in moPageXml.SelectNodes("/Page/Contents/Content[@type='Poll']"))
                {

                    // Reset variables - this is why I would like this to be a class instead.
                    bCanVote = true;
                    nVoteBlockReason = PollBlockReason.None;
                    cValidationError = "";
                    cCookieName = "";
                    cLastVotedSql = "";
                    bVoteOnce = false;
                    sVoteIdentifiers = "";
                    bUseUserId = false;
                    bUseCookies = false;
                    bUseEmail = false;
                    bUseIPAddress = false;
                    openDate = DateTime.MinValue;
                    closeDate = DateTime.MaxValue;

                    // Look for required nodes
                    if (ocNode.SelectSingleNode("Restrictions/Frequency") is null | ocNode.SelectSingleNode("Restrictions/RegisteredVotersOnly") is null | ocNode.SelectSingleNode("Restrictions/Identifiers") is null)

                    {
                    }
                    // Required nodes are missing
                    else
                    {
                        // Set the config options for this poll
                        sVoteFrequency = ocNode.SelectSingleNode("Restrictions/Frequency").InnerText;
                        bUseUserId = ocNode.SelectSingleNode("Restrictions/RegisteredVotersOnly").InnerText == "true";
                        sVoteIdentifiers = ocNode.SelectSingleNode("Restrictions/Identifiers").InnerText;

                        // Check open and close dates
                        if (ocNode.SelectSingleNode("dOpenDate") != null && DateTime.TryParse(ocNode.SelectSingleNode("dOpenDate").InnerText, out DateTime parsedOpenDate))
                        {
                            openDate = parsedOpenDate;
                        }
                        if (ocNode.SelectSingleNode("dCloseDate") != null && DateTime.TryParse(ocNode.SelectSingleNode("dCloseDate").InnerText, out DateTime parsedCloseDate))
                        {
                            closeDate = parsedCloseDate;
                        }
                        if (openDate > DateTime.Now | closeDate < DateTime.Now)
                        {
                            bCanVote = false;
                            nVoteBlockReason = PollBlockReason.PollNotAvailable;
                        }


                        // Sort out the vote frequency
                        switch (sVoteFrequency ?? "")
                        {
                            case "once":
                                {
                                    bVoteOnce = true;
                                    break;
                                }
                            case "daily":
                                {
                                    dCurrentVotesExpiryDate = DateTime.Now.AddHours(24);
                                    dPreviousVotesCreationDate = DateTime.Now.AddHours(-24);
                                    break;
                                }
                            case "weekly":
                                {
                                    dCurrentVotesExpiryDate = DateTime.Now.AddDays(7);
                                    dPreviousVotesCreationDate = DateTime.Now.AddDays(-7);
                                    break;
                                }
                            case "monthly":
                                {
                                    dCurrentVotesExpiryDate = DateTime.Now.AddMonths(1);
                                    dPreviousVotesCreationDate = DateTime.Now.AddMonths(-1);
                                    break;
                                }
                        }


                        // If we're not using a registered user then set the other identifiers
                        if (!bUseUserId)
                        {
                            bUseCookies = sVoteIdentifiers.Contains("cookies");
                            bUseEmail = sVoteIdentifiers.Contains("email");
                            bUseIPAddress = sVoteIdentifiers.Contains("ipaddress");
                            if (bUseIPAddress)
                            {
                                gbIPLogging = true;
                                gbIPLogging = true;
                            }
                        }


                        // Set the metadata
                        sPollField = "nArtId";
                        sPollItemField = "nOtherId";
                        sTable = "tblActivityLog";
                        sPollId = ocNode.GetAttribute("id");
                        cCookieName = "pvote-" + sPollId;

                        // Get the poll items
                        var oCRNode = moPageXml.CreateElement("PollItems");
                        moDbHelper.addRelatedContent(ref oCRNode, Convert.ToInt32(sPollId), true);
                        ocNode.AppendChild(oCRNode);

                        // ===================================================================
                        // Check the restrictions
                        // ===================================================================
                        // This should tell us if the user can vote, and has voted.

                        // First - is this restricted to logged on users only
                        if (bUseUserId & !(mnUserId > 0))
                        {
                            bCanVote = false;
                            nVoteBlockReason = PollBlockReason.RegisteredUsersOnly;
                        }

                        if (bCanVote)
                        {

                            // There are two places that votes can be identified
                            // tblActivityLog - if User Id, IP address or email are being used
                            // Cookie - if cookies are being used.

                            // We need to check for both and then assess whether they can vote.
                            sSql = "";

                            if (bUseUserId)
                            {
                                sSql += " AND nUserDirId=" + mnUserId;
                            }
                            else
                            {
                                // First check if a cookie is being used - if it exists then we block voting.
                                if (bUseCookies && moRequest.Cookies[cCookieName] != null)
                                {
                                    bCanVote = false;
                                    nVoteBlockReason = PollBlockReason.CookieFound;
                                }

                                // If no cookie formulate the sql for the other restrictions.
                                if (bCanVote)
                                {

                                    if (bUseIPAddress)
                                    {
                                        moDbHelper.checkForIpAddressCol();
                                        string ipAddress = moRequest.ServerVariables["REMOTE_ADDR"];
                                        if (ipAddress.Length > 15)
                                            ipAddress = ipAddress.Substring(0, 15);
                                        sSql += " AND cIPAddress=" + Tools.Database.SqlString(ipAddress);
                                    }

                                    if (moRequest["poll-email"] != null)
                                    {
                                        sEmail = moRequest["poll-email"].ToString();
                                        if (bUseEmail)
                                            sSql += " AND cActivityDetail=" + Tools.Database.SqlString(sEmail);
                                    }
                                }
                            }

                            // Finally, check the tblActivityLog for votes
                            if (bCanVote)
                            {


                                // Get the last tblActivityVote
                                if (!string.IsNullOrEmpty(sSql))
                                {


                                    // Check for blocking
                                    // First get the scope of blocking, either Global or Poll
                                    string blockingScope = moConfig["PollBlockingScope"];
                                    string blockingScopeQuery = "";
                                    if (string.IsNullOrEmpty(blockingScope))
                                        blockingScope = "Global";
                                    if (blockingScope.ToLower() == "poll")
                                    {
                                        blockingScopeQuery = "AND nArtId=" + sPollId + " ";
                                    }

                                    // Check for blocks / exclusions
                                    blockingScopeQuery = "SELECT TOP 1 nActivityKey " + "FROM	dbo.tblActivityLog " + "WHERE   nActivityType=" + ((int)Cms.dbHelper.ActivityType.VoteExcluded).ToString() + " " + blockingScopeQuery + sSql;



                                    string blocked = Convert.ToString(moDbHelper.GetDataValue(blockingScopeQuery, CommandType.Text, null, ""));
                                    if (!string.IsNullOrEmpty(blocked))
                                    {
                                        // Block has been found
                                        bCanVote = false;
                                        nVoteBlockReason = PollBlockReason.Excluded;
                                    }
                                    else
                                    {
                                        // Get the last tblActivityVote
                                        sSql = "SELECT  TOP 1 dDateTime As LastVoted " + "FROM	dbo.tblActivityLog " + "WHERE   nActivityType=" + ((int)Cms.dbHelper.ActivityType.SubmitVote).ToString() + " " + "AND nArtId=" + sPollId + " " + sSql;



                                        string cLastVoted = Convert.ToString(moDbHelper.GetDataValue(sSql, CommandType.Text, null, ""));
                                        if (!string.IsNullOrEmpty(cLastVoted) && DateTime.TryParse(cLastVoted, out DateTime lastVotedDate))
                                        {
                                            // We found a vote, check the date
                                            if (bVoteOnce || lastVotedDate > dPreviousVotesCreationDate)
                                            {
                                                // Block the vote
                                                bCanVote = false;
                                                nVoteBlockReason = PollBlockReason.LogFound;
                                            }
                                        }

                                    }


                                }
                            }
                        }




                        // ===================================================================
                        // Look for votes being submitted
                        // ===================================================================

                        if (bCanVote && moRequest["pollsubmit-" + sPollId] != null && !string.IsNullOrEmpty(moRequest["pollsubmit-" + sPollId].ToString()) && moRequest["polloption-" + sPollId] != null)


                        {

                            string sResult = moRequest["polloption-" + sPollId].ToString();

                            sEmail = "";
                            if (moRequest["poll-email"] != null)
                            {
                                sEmail = moRequest["poll-email"].ToString();
                            }

                            bHasVoted = true;

                            if (!bUseEmail & string.IsNullOrEmpty(sEmail) | Tools.Text.IsEmail(sEmail))
                            {
                                moDbHelper.logActivity(Cms.dbHelper.ActivityType.SubmitVote, (long)mnUserId, (long)mnPageId, (long)Convert.ToInt64(sPollId), (long)Convert.ToInt64(sResult), sEmail);

                                if (bUseCookies)
                                {
                                    var oCookie = new System.Web.HttpCookie(cCookieName, "voted");
                                    oCookie.Expires = dCurrentVotesExpiryDate;
                                    moResponse.Cookies.Add(oCookie);
                                }


                                bCanVote = false;
                                nVoteBlockReason = PollBlockReason.JustVoted;
                            }
                            else if (bUseEmail & string.IsNullOrEmpty(sEmail))
                            {
                                cValidationError = "Email address required";
                            }
                            else
                            {
                                cValidationError = "Invalid email address";
                            }

                        }

                        // Add the status node

                        var oCStatusNode = moPageXml.CreateElement("Status");
                        oCStatusNode.SetAttribute("canVote", bCanVote ? "true" : "false");
                        if (bHasVoted)
                            oCStatusNode.SetAttribute("justVoted", "true");
                        if (nVoteBlockReason != PollBlockReason.None)
                            oCStatusNode.SetAttribute("blockReason", nVoteBlockReason.ToString());
                        if (!string.IsNullOrEmpty(cValidationError))
                            oCStatusNode.SetAttribute("validationError", cValidationError);
                        ocNode.AppendChild(oCStatusNode);


                        // Calculate the results
                        var oCResNode = moPageXml.CreateElement("Results");
                        // Do we need to check the table exists?
                        sSql = "SELECT DISTINCT " + sPollItemField + " AS PollOption, Count(" + sPollItemField + ") AS ResultsCount FROM " + sTable + " WHERE nActivityType=" + ((int)Cms.dbHelper.ActivityType.SubmitVote).ToString() + " AND " + sPollField + " = '" + sPollId + "' GROUP BY " + sPollItemField;
                        using (var oDr = moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                        {

                            while (oDr.Read())
                            {
                                var oResNode = moPageXml.CreateElement("PollResult");
                                oResNode.SetAttribute("entryId", Convert.ToString(oDr[0]));
                                oResNode.SetAttribute("votes", Convert.ToString(oDr[1]));
                                oCResNode.AppendChild(oResNode);
                            }
                            oDr.Close();
                        }

                        ocNode.AppendChild(oCResNode);
                    }


                }

                sProcessInfo = "End  Poll Processing";
            }
            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ProcessPolls", ex, sProcessInfo));
            }
        }

        public virtual void AddSearch(ref Cms aWeb)
        {
            var argaWeb = this;
            oSrch = new Cms.Search(ref argaWeb);
            oSrch.apply();
        }

        /// <summary>
        /// Executes processing on specific content types.
        /// </summary>
        /// <remarks>In most cases, plugins (e.g. Search, Reports) and LayoutActions do this, so this is for content types that require a non-layout, non-function specific approach.</remarks>
        private void ContentActions()
        {

            PerfMon.Log("Web", "ContentActions - Start");
            string sProcessInfo = "";
            XmlElement ocNode;

            try
            {

                // Load in any specified content files

                foreach (XmlElement currentOcNode in moPageXml.SelectNodes("/Page/Contents/Content[@contentFile!=''] | /Page/ContentDetail/descendant-or-self::Content[@contentFile!='']"))
                {
                    ocNode = currentOcNode;

                    if (File.Exists(goServer.MapPath("/" + gcProjectPath + ocNode.GetAttribute("contentFile"))))
                    {
                        var newXml = new XmlDocument();
                        newXml.PreserveWhitespace = true;
                        // copy related nodes
                        newXml.Load(goServer.MapPath("/" + gcProjectPath + ocNode.GetAttribute("contentFile")));
                        foreach (XmlElement relElem in ocNode.SelectNodes("Content"))
                        {
                            var argnodeToAddTo = newXml.DocumentElement;
                            Tools.Xml.AddExistingNode(ref argnodeToAddTo, relElem);

                        }
                        ocNode.InnerXml = newXml.DocumentElement.InnerXml;
                        // ocNode.AppendChild(moPageXml.ImportNode(Protean.Tools.Xml.firstElement(newXml.DocumentElement), True))
                    }

                }

                foreach (XmlElement currentOcNode1 in moPageXml.SelectNodes("/Page/*/Content[@appendFile!='']"))
                {
                    ocNode = currentOcNode1;

                    if (File.Exists(goServer.MapPath("/" + gcProjectPath + ocNode.GetAttribute("appendFile"))))
                    {
                        var newXml = new XmlDocument();
                        newXml.PreserveWhitespace = true;
                        newXml.Load(goServer.MapPath("/" + gcProjectPath + ocNode.GetAttribute("appendFile")));

                        ocNode.AppendChild(moPageXml.ImportNode(newXml.DocumentElement, true));
                    }

                }

                string ContentActionXpath = "";
                if (mnArtId > 0 & moConfig["ActionOnDetail"]?.ToLower() != "true")
                {
                    ContentActionXpath = "/Page/Contents/Content[@action!='' and @actionOnDetail='true'] | /Page/ContentDetail/Content[@action!=''] | /Page/ContentDetail/Content/Content[@action!=''] | /Page/Contents/Content[@action!='' and @id='" + mnArtId + "']";
                }
                else
                {
                    ContentActionXpath = "/Page/Contents/Content[@action!='']";
                }


                foreach (XmlElement currentOcNode2 in moPageXml.SelectNodes(ContentActionXpath))
                {
                    ocNode = currentOcNode2;
                    string classPath = ocNode.GetAttribute("action");
                    string assemblyName = ocNode.GetAttribute("assembly");
                    string assemblyType = ocNode.GetAttribute("assemblyType");
                    string providerName = ocNode.GetAttribute("providerName");
                    string providerType = ocNode.GetAttribute("providerType");

                    if (providerType == "")
                    {
                        providerType = "messaging";
                    }
                    string methodName = classPath.Substring(classPath.LastIndexOf(".") + 1);

                    classPath = classPath.Substring(0, classPath.LastIndexOf("."));

                    if (!string.IsNullOrEmpty(classPath))
                    {
                        try
                        {
                            Type calledType = null;

                            if (!string.IsNullOrEmpty(assemblyName))
                            {
                                classPath = classPath + ", " + assemblyName;
                            }
                            // Dim oModules As New Protean.Cms.Membership.Modules
                            if (!string.IsNullOrEmpty(providerName))
                            {
                                // case for external Providers
                                Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection("protean/" + providerType + "Providers");
                                Assembly assemblyInstance;

                                if (moPrvConfig.Providers[providerName + "Local"] != null)
                                {
                                    if (!string.IsNullOrEmpty(moPrvConfig.Providers[providerName + "Local"].Parameters["path"]))
                                    {
                                        assemblyInstance = Assembly.LoadFrom(goServer.MapPath(moPrvConfig.Providers[providerName + "Local"].Parameters["path"]));
                                        calledType = assemblyInstance.GetType(classPath, true);
                                    }
                                    else
                                    {
                                        assemblyInstance = Assembly.Load(moPrvConfig.Providers[providerName + "Local"].Type);
                                        calledType = assemblyInstance.GetType(classPath, true);
                                    }
                                }
                                else
                                {
                                    if (moPrvConfig.Providers[providerName] != null)
                                    {
                                        switch (moPrvConfig.Providers[providerName].Parameters["path"])
                                        {
                                            case var @case when @case == "":
                                                {
                                                    assemblyInstance = Assembly.Load(moPrvConfig.Providers[providerName].Type);
                                                    calledType = assemblyInstance.GetType(classPath, true);
                                                    break;
                                                }
                                            case "builtin":
                                                {
                                                    string prepProviderName; // = Replace(moPrvConfig.Providers(providerName).Type, ".", "+")
                                                                             // prepProviderName = (New Regex("\+")).Replace(prepProviderName, ".", 1)
                                                    prepProviderName = moPrvConfig.Providers[providerName].Type;
                                                    calledType = Type.GetType(prepProviderName + "+" + classPath, true);
                                                    break;
                                                }

                                            default:
                                                {
                                                    assemblyInstance = Assembly.LoadFrom(goServer.MapPath(moPrvConfig.Providers[providerName].Parameters["path"]));
                                                    classPath = moPrvConfig.Providers[providerName].Parameters["classPrefix"] + classPath;
                                                    calledType = assemblyInstance.GetType(classPath, true);
                                                    break;
                                                }
                                        }
                                    }


                                }
                            }
                            else if (!string.IsNullOrEmpty(assemblyType))
                            {
                                // case for external DLL's
                                var assemblyInstance = Assembly.Load(assemblyType);
                                calledType = assemblyInstance.GetType(classPath, true);
                            }
                            else
                            {
                                // case for methods within EonicWeb Core DLL
                                calledType = Type.GetType(classPath, true);
                            }
                            if (calledType != null)
                            {

                                var o = Activator.CreateInstance(calledType);

                                var args = new object[2];
                                args[0] = this;
                                args[1] = ocNode;

                                calledType.InvokeMember(methodName, BindingFlags.InvokeMethod, null, o, args);

                                // Error Handling ?
                                // Object Clearup ?
                            }
                            calledType = null;
                        }

                        catch (Exception ex)
                        {
                            // OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ContentActions", ex, sProcessInfo))
                            sProcessInfo = classPath + "." + methodName + " not found";
                            ocNode.InnerXml = "<Content type=\"error\"><div>" + HtmlEncode(sProcessInfo + ex.Message + ex.StackTrace) + "</div></Content>";
                        }
                    }

                }

                // Content Type : ContentGrabber
                foreach (XmlElement currentOcNode3 in moPageXml.SelectNodes("/Page/Contents/Content[@display='grabber']"))
                {
                    ocNode = currentOcNode3;
                    moDbHelper.getContentFromModuleGrabber(ref ocNode);
                }

                if (!gcBlockContentType.Contains("Product"))
                {
                    foreach (XmlElement currentOcNode4 in moPageXml.SelectNodes("/Page/Contents/Content[@display='group']"))
                    {
                        ocNode = currentOcNode4;
                        moDbHelper.getContentFromProductGroup(ref ocNode);
                    }
                }
                // Content Type : ContentGrabber
                foreach (XmlElement currentOcNode5 in moPageXml.SelectNodes("/Page/Contents/Content[@type='ContentGrabber']"))
                {
                    ocNode = currentOcNode5;
                    moDbHelper.getContentFromContentGrabber(ref ocNode);
                }

                // Content Type : Poll
                if (moPageXml.SelectNodes("/Page/Contents/Content[@type='Poll']").Count > 0)
                {
                    ProcessPolls();
                }



                // Count Relations
                string cContentIdsForRelatedCount = "";
                foreach (XmlElement currentOcNode6 in moPageXml.SelectNodes("/Page/Contents/descendant-or-self::Content[@type='Tag' or @relatedCount='true']"))
                {
                    ocNode = currentOcNode6;
                    cContentIdsForRelatedCount += ocNode.GetAttribute("id") + ",";
                }
                if (!string.IsNullOrEmpty(cContentIdsForRelatedCount))
                {
                    cContentIdsForRelatedCount = cContentIdsForRelatedCount.Remove(cContentIdsForRelatedCount.Length - 1);
                    string sSql = "select Distinct COUNT(nContentParentid) as count, nContentChildid from tblContentRelation where nContentChildId in (" + cContentIdsForRelatedCount + ")  group by nContentChildid";
                    // Dim oDr As SqlDataReader = moDbHelper.getDataReader(sSql)
                    using (var oDr = moDbHelper.getDataReaderDisposable(sSql)) // Done by sonali on 13/7/2022
                    {

                        while (oDr.Read())
                        {
                            foreach (XmlElement currentOcNode7 in moPageXml.SelectNodes($"/Page/Contents/descendant-or-self::Content[@id='{oDr["nContentChildId"]}']" ))
                            {
                                ocNode = currentOcNode7;
                                ocNode.SetAttribute("relatedCount", Convert.ToString(oDr["count"]));
                            }
                        }
                    }
                }


                // Content Type : xForm with ContentAction
                if (!mbAdminMode)
                {
                    string formXpath = "/Page/Contents/Content[(@type='xform' and model/submission/@SOAPAction) or (@process='xform') or (@moduleType='xForm' and model/submission/@SOAPAction)]";
                    if (mnArtId > 0)
                    {
                        // if the current contentDetail has child xform then that is all we process.
                        if (moPageXml.SelectNodes("/Page/ContentDetail/descendant-or-self::Content[(@type='xform' and model/submission/@SOAPAction) or (@process='xform')]").Count > 0)
                        {
                            formXpath = "/Page/ContentDetail/descendant-or-self::Content[(@type='xform' and model/submission/@SOAPAction) or (@process='xform')]";
                        }
                    }

                    foreach (XmlElement currentOcNode8 in moPageXml.SelectNodes(formXpath))
                    {
                        ocNode = currentOcNode8;
                        // If oXform Is Nothing Then oXform = New xForm
                        if (oXform is null)
                            oXform = (Protean.xForm)getXform();

                        oXform.moPageXML = moPageXml;
                        oXform.load(ref ocNode, true);
                        if (oXform.isSubmitted())
                        {
                            oXform.submit();
                        }
                        oXform.addValues();
                        oXform = (Protean.xForm)null;
                    }
                    // just want to add submitted values but not handle submit
                    formXpath = "/Page/Contents/Content[(@process='addValues')]";
                    if (mnArtId > 0)
                        formXpath = "/Page/ContentDetail/descendant-or-self::Content[(@process='addValues')]";
                    foreach (XmlElement currentOcNode9 in moPageXml.SelectNodes(formXpath))
                    {
                        ocNode = currentOcNode9;
                        if (oXform is null)
                            oXform = (Protean.xForm)getXform();
                        oXform.moPageXML = moPageXml;
                        oXform.load(ref ocNode, true);
                        if (oXform.isSubmitted())
                        {
                            oXform.updateInstanceFromRequest();
                        }
                        oXform.addValues();
                        oXform = (Protean.xForm)null;
                    }
                }

                BespokeActions();
                PerfMon.Log("Web", "ContentActions - End");
            }




            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ContentActions", ex, sProcessInfo));
            }
        }


        /// <summary>
        /// Use of this is for discussion, but it is my intention that this could contain a repository of actions
        /// that are not necearrily triggered by Content, but rather by command; and yet could be accessible to different 
        /// faculties of EonicWeb, such as Admin calls, Ajax calls and Content building calls.
        /// </summary>
        /// <remarks></remarks>
        protected virtual void CommonActions()
        {
            try
            {
                // Integration calls - trying to use a similar methodology as above
                // Commented out by adding False.
                string integrationCommand = "";

                if (moRequest.QueryString.Count > 0)
                {
                    integrationCommand = moRequest["integration"];
                }

                if (!string.IsNullOrEmpty(integrationCommand))
                {


                    // Directory integrations take a directory ID
                    string requestedDirectoryId = moRequest["dirId"];
                    long directoryId = (long)mnUserId;
                    if (!string.IsNullOrEmpty(requestedDirectoryId) && Tools.Number.IsNumeric(requestedDirectoryId) && Convert.ToInt16(requestedDirectoryId) > 0)

                    {
                        directoryId = Convert.ToInt64(requestedDirectoryId);
                    }

                    if (directoryId > 0L)
                    {
                        // Invoke the integration method.
                        object[] constructorArguments = new object[] { this, Convert.ToInt64(directoryId) };
                        Invoke.InvokeObjectMethod("Protean.Integration.Directory." + integrationCommand, constructorArguments, null, this, "OnComponentError", "OnError");
                    }

                }
            }
            catch (Exception ex)
            {
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "CommonActions", ex, ""));

            }
        }


        /// <summary>
        /// To allow for bespoke content actions to be overloaded
        /// </summary>
        /// <remarks></remarks>
        public virtual void BespokeActions()
        {

        }

        /// <summary>
        /// Executes Actions based on Specific Page Layouts.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public virtual string LayoutActions()
        {
            PerfMon.Log("Web", "LayoutActions");
            string sProcessInfo = "";
            bool bRunSearches = false;


            try
            {

                switch (moPageXml.SelectSingleNode("/Page/@layout").Value ?? "")
                {
                    case "Search_Results":
                    case "Search_Results_Products_Index":
                    case "Search":
                    case "Quick_Find":
                        {
                            if (!ibIndexMode)
                            {
                                bRunSearches = true;
                            }

                            break;
                        }
                    case "List_Quotes":
                        {
                            if (mnUserId > 0)
                            {
                                Cms.Quote oQuote;
                                var argaWeb = this;
                                oQuote = new Cms.Quote(ref argaWeb);
                                XmlElement argoPageDetail = null;
                                oQuote.ListOrders(("0" + moRequest["OrderId"]).ToString(), true, 0, oPageDetail: ref argoPageDetail);
                                oQuote = (Cms.Quote)null;
                            }

                            break;
                        }
                    case "List_Orders":
                        {
                            if (mnUserId > 0)
                            {
                                Cms.Cart oCart;
                                var argaWeb1 = this;
                                oCart = new Cms.Cart(ref argaWeb1);
                                XmlElement argoPageDetail1 = null;
                                oCart.ListOrders(("0" + moRequest["OrderId"]).ToString(), true, 0, oPageDetail: ref argoPageDetail1);
                                oCart = (Cms.Cart)null;
                            }

                            break;
                        }
                }

                if ((gbCart | gbQuote) & moPageXml.SelectSingleNode("/Page/@layout").Value.Contains("Discounts_"))
                {
                    var argPageElmt = moPageXml.DocumentElement;
                    moDiscount.getDiscountXML(ref argPageElmt);
                }

                // extra bit for searches
                if (moPageXml.SelectSingleNode("/Page/@layout").Value.Contains("Search_Template"))
                {
                    bRunSearches = true;
                }
                // commented out by TS because this case is hit in v5 when using a module and this means it runs twice.
                if (moPageXml.SelectSingleNode("/Page/Contents/Content[@action='Protean.Cms+Search+Modules.GetResults']") is null)
                {
                    if (moRequest.QueryString.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(moRequest["searchMode"]))
                        {
                            bRunSearches = true;
                        }
                    }
                }

                if (bRunSearches)
                {
                    var argaWeb2 = this;
                    AddSearch(ref argaWeb2);
                }

                // extra bit for user control panels
                if (moPageXml.SelectSingleNode("/Page/@layout").Value.Contains("User_Control_Panel") | moPageXml.SelectSingleNode("/Page/@layout").Value.Contains("Internal_Feed"))
                {
                    string cContentTypes = moConfig["ControlPanelTypes"];
                    if (cContentTypes != null & !string.IsNullOrEmpty(cContentTypes))
                    {
                        string[] oTypes = cContentTypes.Split(',');
                        int i;
                        var loopTo = oTypes.Length - 1;
                        for (i = 0; i <= loopTo; i++)
                        {
                            // add all of those types of content to the pagecontent to the page
                            var argoPageElmt = moPageXml.DocumentElement;
                            GetContentXMLByType(ref argoPageElmt, oTypes[i]);
                        }
                    }
                }

                return "";
            }

            catch (Exception ex)
            {
                // returnException(msException, mcModuleName, "LayoutActions", ex, gcEwSiteXsl, sProcessInfo, gbDebug)
                OnComponentError(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "LayoutActions", ex, sProcessInfo));
                return "";
            }

        }



    }
}
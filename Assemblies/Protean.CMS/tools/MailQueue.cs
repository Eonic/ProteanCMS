using System;
using System.Data;
using System.Web.Configuration;
using System.Xml;
using Protean.Tools.Integration.Twitter;
using static Protean.stdTools;

namespace Protean
{

    public class MailQueue
    {

        public string mcModuleName = "Eonic.MailQueue";
        public System.Collections.Specialized.NameValueCollection moConfig;
        public string msException;


        public MailQueue() : base()
        {
            moConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
            // PerfMon.Log("MailQueue", "New")
            mcModuleName = "MailQueue";
        }

        private System.Collections.Specialized.NameValueCollection oSchedulerConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/scheduler");
        private Protean.Cms.dbHelper oDBT_Local;
        private Protean.Cms.dbHelper oDBT_Remote;



        public int Add(int nPageId, string cFromEmail, string cFromName, string cSubject, string cBody = "", string cGroups_CSV = "", int nUserId = 0, bool bSkipQue = false)
        {
            // PerfMon.Log("MailQueue", "Add")
            try
            {
                oDBT_Local = new Cms.dbHelper("Data Source=" + moConfig["DatabaseServer"] + "; " + "Initial Catalog=" + moConfig["DatabaseName"] + "; " + moConfig["DatabaseAuth"], 1);



                string cConStr;

                if (oSchedulerConfig != null)
                {
                    cConStr = "Data Source=" + oSchedulerConfig["DatabaseServer"] + "; ";
                    cConStr += "Initial Catalog=" + oSchedulerConfig["DatabaseName"] + "; ";
                    cConStr += oSchedulerConfig["DatabaseAuth"];
                    oDBT_Remote.ResetConnection(cConStr);
                }
                else
                {
                    return 0;
                }

                oDBT_Remote = new Cms.dbHelper(cConStr, 1);

                string cSQL = "";
                if (!string.IsNullOrEmpty(cGroups_CSV) & nUserId == 0)
                {
                    cSQL = "SELECT nDirKey, cDirXml" + " FROM tblDirectory" + " WHERE (((SELECT TOP 1 nDirChildId" + " FROM tblDirectoryRelation" + " WHERE (nDirParentId IN (" + cGroups_CSV + ")) AND (nDirChildId = tblDirectory.ndirKey))) IS NOT NULL)";
                }
                else if (string.IsNullOrEmpty(cGroups_CSV) & !(nUserId == 0))
                {
                    // send to individual
                    cSQL = "SELECT nDirKey, cDirXml FROM tblDirectory WHERE nDirKey = " + nUserId;
                }
                else
                {
                    return 0;
                }

                int nRequestID = AddRequest(nPageId, cFromEmail, cFromName, cSubject, cBody, bSkipQue);
                if (nRequestID == 0)
                    return 0;
                var oUserXML = new XmlDocument();
                DataSet oDS = oDBT_Local.GetDataSet(cSQL, "Users");
                foreach (DataRow oDR in oDS.Tables["Users"].Rows)
                {
                    var oElmt = oUserXML.CreateElement("UserDetails");
                    oElmt.InnerXml = Convert.ToString(oDR["cDirXML"]).Replace("&gt;", ">").Replace("&lt;", "<");
                    string cEmail = "";
                    XmlElement oEmailElmt = (XmlElement)oElmt.SelectSingleNode("User/Email");
                    if (oEmailElmt != null)
                    {
                        cEmail = oEmailElmt.InnerText;
                    }
                    string cName = "";
                    XmlElement oFNameElmt = (XmlElement)oElmt.SelectSingleNode("User/FirstName");
                    if (oFNameElmt != null)
                        cName = oFNameElmt.InnerText;
                    XmlElement oLNameElmt = (XmlElement)oElmt.SelectSingleNode("User/LastName");
                    if (oLNameElmt != null)
                    {
                        if (!string.IsNullOrEmpty(cName))
                            cName += " " + oLNameElmt.InnerText;
                    }
                    AddRecipient(nRequestID, Convert.ToInt16(oDR["nDirKey"]), cEmail, cName);
                }
                FinishRequest(nRequestID);
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "Add", ex, "", "", gbDebug);
            }

            return default;
        }

        private int AddRequest(int nPageId, string cFromEmail, string cFromName, string cSubject, string cBody = "", bool bSkipQue = false)
        {
            // PerfMon.Log("MailQueue", "AddRequest")
            try
            {
                string cSQL = "INSERT INTO tblMailRequests (nPageId, cBody, cFromEmail, cFromName, cSubject, cSiteURL, cMailServer, nStatus, nPriority) VALUES (";
                cSQL += nPageId + ",";
                cSQL += "'" + cBody + "',";
                cSQL += "'" + cFromEmail + "',";
                cSQL += "'" + cFromName + "',";
                cSQL += "'" + cSubject + "',";
                cSQL += "'" + moConfig["BaseUrl"] + "',";
                cSQL += "" + moConfig["MailServer"] + ",";
                cSQL += "0,";
                cSQL = cSQL + (bSkipQue ? "1" : "0") + ")";
                return Convert.ToInt32(oDBT_Remote.GetIdInsertSql(cSQL));
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "Add Request", ex, "", "", gbDebug);
            }

            return default;
        }

        private int AddRecipient(int nRequestKey, int nUserID, string cToEmail, string cToName)
        {
            // PerfMon.Log("MailQueue", "AddRecipient")
            try
            {
                string cSQL = "INSERT INTO tblMailRequestRecipients (nRequestId, nUserId, nStatus, nRetries, cEmail, cName) VALUES (";
                cSQL += nRequestKey + ",";
                cSQL += nUserID + ",";
                cSQL += "0,";
                cSQL += "0,";
                cSQL += "'" + cToEmail + "',";
                cSQL += "'" + cToName + "')";
                return Convert.ToInt32(oDBT_Remote.GetIdInsertSql(cSQL));
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "Add Recipient", ex, "", "", gbDebug);
            }

            return default;
        }

        private void FinishRequest(int nRequestId)
        {
            // PerfMon.Log("MailQueue", "FinishRequest")
            try
            {
                string cSQL = "UPDATE tblMailRequests SET nStatus = 1 WHERE nMailRequestKey = " + nRequestId;
                oDBT_Remote.ExeProcessSql(cSQL);
            }
            catch (Exception ex)
            {
                returnException(ref msException, mcModuleName, "Finish Request", ex, "", "", gbDebug);
            }
        }

        public string GetEmailPage(int nPageId, int nUserId)
        {
            string GetEmailPageRet = default;
            // PerfMon.Log("MailQueue", "GetEmaiPage")
            try
            {
                System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                var oWeb = new Cms();

                oWeb.InitializeVariables();
                oWeb.Open();
                oWeb.mnPageId = nPageId;
                oWeb.mnUserId = nUserId;
                oWeb.mbAdminMode = false;
                oWeb.mcEwSiteXsl = moMailConfig["MailingXsl"];
                oWeb.mnMailMenuId = Convert.ToInt64(moMailConfig["RootPageId"]);
                string cReturnString = oWeb.ReturnPageHTML(nPageId, true);
                cReturnString = NameEntities(cReturnString);
                GetEmailPageRet = cReturnString;
            }
            catch (Exception)
            {
                GetEmailPageRet = "";
            }
            GC.Collect();
            return GetEmailPageRet;
        }

        private string NameEntities(string cString)
        {
            // PerfMon.Log("MailQueue", "NameEntities")
            cString = cString.Replace("'", "&apos; "); // &#39;
            cString = cString.Replace("¡", "&iexcl;"); // &#161;
            cString = cString.Replace("¤", "&curren;"); // &#164;
            cString = cString.Replace("¢", "&cent;"); // &#162;
            cString = cString.Replace("£", "&pound;"); // &#163;
            cString = cString.Replace("¥", "&yen;"); // &#165;
            cString = cString.Replace("¦", "&brvbar;"); // &#166;
            cString = cString.Replace("§", "&sect;"); // &#167;
            cString = cString.Replace("¨", "&uml;"); // &#168;
            cString = cString.Replace("©", "&copy;"); // &#169;
            cString = cString.Replace("ª", "&ordf;"); // &#170;
            cString = cString.Replace("¬", "&not;"); // &#172;
            cString = cString.Replace("­", "&shy;"); // &#173;
            cString = cString.Replace("®", "&reg;"); // &#174;
            cString = cString.Replace("™", "&trade;"); // &#8482;
            cString = cString.Replace("¯", "&macr;"); // &#175;
            cString = cString.Replace("°", "&deg;"); // &#176;
            cString = cString.Replace("±", "&plusmn;"); // &#177;
            cString = cString.Replace("²", "&sup2;"); // &#178;
            cString = cString.Replace("³", "&sup3;"); // &#179;
            cString = cString.Replace("´", "&acute;"); // &#180;
            cString = cString.Replace("µ", "&micro;"); // &#181;
            cString = cString.Replace("¶", "&para;"); // &#182;
            cString = cString.Replace("·", "&middot;"); // &#183;
            cString = cString.Replace("¸", "&cedil;"); // &#184;
            cString = cString.Replace("¹", "&sup1;"); // &#185;
            cString = cString.Replace("º", "&ordm;"); // &#186;
            cString = cString.Replace("»", "&raquo;"); // &#187;
            cString = cString.Replace("¼", "&frac14;"); // &#188;
            cString = cString.Replace("½", "&frac12;"); // &#189;
            cString = cString.Replace("¾", "&frac34;"); // &#190;
            cString = cString.Replace("¿", "&iquest;"); // &#191;
            cString = cString.Replace("×", "&times;"); // &#215;
            cString = cString.Replace("÷", "&divide;"); // &#247;
            cString = cString.Replace("À", "&Agrave;"); // &#192;
            cString = cString.Replace("Á", "&Aacute;"); // &#193;
            cString = cString.Replace("Â", "&Acirc;"); // &#194;
            cString = cString.Replace("Ã", "&Atilde;"); // &#195;
            cString = cString.Replace("Ä", "&Auml;"); // &#196;
            cString = cString.Replace("Å", "&Aring;"); // &#197;
            cString = cString.Replace("Æ", "&AElig;"); // &#198;
            cString = cString.Replace("Ç", "&Ccedil;"); // &#199;
            cString = cString.Replace("È", "&Egrave;"); // &#200;
            cString = cString.Replace("É", "&Eacute;"); // &#201;
            cString = cString.Replace("Ê", "&Ecirc;"); // &#202;
            cString = cString.Replace("Ë", "&Euml;"); // &#203;
            cString = cString.Replace("Ì", "&Igrave;"); // &#204;
            cString = cString.Replace("Í", "&Iacute;"); // &#205;
            cString = cString.Replace("Î", "&Icirc;"); // &#206;
            cString = cString.Replace("Ï", "&Iuml;"); // &#207;
            cString = cString.Replace("Ð", "&ETH;"); // &#208;
            cString = cString.Replace("Ñ", "&Ntilde;"); // &#209;
            cString = cString.Replace("Ò", "&Ograve;"); // &#210;
            cString = cString.Replace("Ó", "&Oacute;"); // &#211;
            cString = cString.Replace("Ô", "&Ocirc;"); // &#212;
            cString = cString.Replace("Õ", "&Otilde;"); // &#213;
            cString = cString.Replace("Ö", "&Ouml;"); // &#214;
            cString = cString.Replace("Ø", "&Oslash;"); // &#216;
            cString = cString.Replace("Ù", "&Ugrave;"); // &#217;
            cString = cString.Replace("Ú", "&Uacute;"); // &#218;
            cString = cString.Replace("Û", "&Ucirc;"); // &#219;
            cString = cString.Replace("Ü", "&Uuml;"); // &#220;
            cString = cString.Replace("Ý", "&Yacute;"); // &#221;
            cString = cString.Replace("Þ", "&THORN;"); // &#222;
            cString = cString.Replace("ß", "&szlig;"); // &#223;
            cString = cString.Replace("à", "&agrave;"); // &#224;
            cString = cString.Replace("á", "&aacute;"); // &#225;
            cString = cString.Replace("â", "&acirc;"); // &#226;
            cString = cString.Replace("ã", "&atilde;"); // &#227;
            cString = cString.Replace("ä", "&auml;"); // &#228;
            cString = cString.Replace("å", "&aring;"); // &#229;
            cString = cString.Replace("æ", "&aelig;"); // &#230;
            cString = cString.Replace("ç", "&ccedil;"); // &#231;
            cString = cString.Replace("è", "&egrave;"); // &#232;
            cString = cString.Replace("é", "&eacute;"); // &#233;
            cString = cString.Replace("ê", "&ecirc;"); // &#234;
            cString = cString.Replace("ë", "&euml;"); // &#235;
            cString = cString.Replace("ì", "&igrave;"); // &#236;
            cString = cString.Replace("í", "&iacute;"); // &#237;
            cString = cString.Replace("î", "&icirc;"); // &#238;
            cString = cString.Replace("ï", "&iuml;"); // &#239;
            cString = cString.Replace("ð", "&eth;"); // &#240;
            cString = cString.Replace("ñ", "&ntilde;"); // &#241;
            cString = cString.Replace("ò", "&ograve;"); // &#242;
            cString = cString.Replace("ó", "&oacute;"); // &#243;
            cString = cString.Replace("ô", "&ocirc;"); // &#244;
            cString = cString.Replace("õ", "&otilde;"); // &#245;
            cString = cString.Replace("ö", "&ouml;"); // &#246;
            cString = cString.Replace("ø", "&oslash;"); // &#248;
            cString = cString.Replace("ù", "&ugrave;"); // &#249;
            cString = cString.Replace("ú", "&uacute;"); // &#250;
            cString = cString.Replace("û", "&ucirc;"); // &#251;
            cString = cString.Replace("ü", "&uuml;"); // &#252;
            cString = cString.Replace("ý", "&yacute;"); // &#253;
            cString = cString.Replace("þ", "&thorn;"); // &#254;
            cString = cString.Replace("ÿ", "&yuml;"); // &#255;
            cString = cString.Replace("Œ", "&OElig;"); // &#338;
            cString = cString.Replace("œ", "&oelig;"); // &#339;
            cString = cString.Replace("Š", "&Scaron;"); // &#352;
            cString = cString.Replace("š", "&scaron;"); // &#353;
            cString = cString.Replace("Ÿ", "&Yuml;"); // &#376;
            cString = cString.Replace("ˆ", "&circ;"); // &#710;
            cString = cString.Replace("˜", "&tilde;"); // &#732;
            cString = cString.Replace("–", "&ndash;"); // &#8211;
            cString = cString.Replace("—", "&mdash;"); // &#8212;
            cString = cString.Replace("‘", "&lsquo;"); // &#8216;
            cString = cString.Replace("’", "&rsquo;"); // &#8217;
            cString = cString.Replace("„", "&bdquo;"); // &#8222;
            cString = cString.Replace("†", "&dagger;"); // &#8224;
            cString = cString.Replace("‡", "&Dagger;"); // &#8225;
            cString = cString.Replace("‰", "&permil;"); // &#8240;
            cString = cString.Replace("€", "&euro;"); // &#8364;
            cString = cString
      .Replace("\r", "")
      .Replace("\t", "")
      .Replace(((char)160).ToString(), " ")
      .Replace("\n", "")
      .Replace(Environment.NewLine, "")
      .Replace("\t", "");

            // Dim i As Integer = 1
            // Do Until i <= 0
            // i = InStr(cString, "  ")
            // cString = Replace(cString, "  ", " ")
            // Loop
            cString = cString.Replace("> <", ">&nbsp;<");
            return cString;
        }
    }
}
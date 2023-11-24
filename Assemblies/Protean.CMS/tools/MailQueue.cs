using System;
using System.Data;
using System.Web.Configuration;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
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
                    oElmt.InnerXml = Strings.Replace(Strings.Replace(Conversions.ToString(oDR["cDirXML"]), "&gt;", ">"), "&lt;", "<");
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
                    AddRecipient(nRequestID, Conversions.ToInteger(oDR["nDirKey"]), cEmail, cName);
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
                cSQL = Conversions.ToString(cSQL + Operators.ConcatenateObject(Interaction.IIf(bSkipQue, 1, 0), ")"));
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
            catch (Exception ex)
            {
                GetEmailPageRet = "";
            }
            GC.Collect();
            return GetEmailPageRet;
        }

        private string NameEntities(string cString)
        {
            // PerfMon.Log("MailQueue", "NameEntities")
            cString = Strings.Replace(cString, "'", "&apos; "); // &#39;
            cString = Strings.Replace(cString, "¡", "&iexcl;"); // &#161;
            cString = Strings.Replace(cString, "¤", "&curren;"); // &#164;
            cString = Strings.Replace(cString, "¢", "&cent;"); // &#162;
            cString = Strings.Replace(cString, "£", "&pound;"); // &#163;
            cString = Strings.Replace(cString, "¥", "&yen;"); // &#165;
            cString = Strings.Replace(cString, "¦", "&brvbar;"); // &#166;
            cString = Strings.Replace(cString, "§", "&sect;"); // &#167;
            cString = Strings.Replace(cString, "¨", "&uml;"); // &#168;
            cString = Strings.Replace(cString, "©", "&copy;"); // &#169;
            cString = Strings.Replace(cString, "ª", "&ordf;"); // &#170;
            cString = Strings.Replace(cString, "¬", "&not;"); // &#172;
            cString = Strings.Replace(cString, "­", "&shy;"); // &#173;
            cString = Strings.Replace(cString, "®", "&reg;"); // &#174;
            cString = Strings.Replace(cString, "™", "&trade;"); // &#8482;
            cString = Strings.Replace(cString, "¯", "&macr;"); // &#175;
            cString = Strings.Replace(cString, "°", "&deg;"); // &#176;
            cString = Strings.Replace(cString, "±", "&plusmn;"); // &#177;
            cString = Strings.Replace(cString, "²", "&sup2;"); // &#178;
            cString = Strings.Replace(cString, "³", "&sup3;"); // &#179;
            cString = Strings.Replace(cString, "´", "&acute;"); // &#180;
            cString = Strings.Replace(cString, "µ", "&micro;"); // &#181;
            cString = Strings.Replace(cString, "¶", "&para;"); // &#182;
            cString = Strings.Replace(cString, "·", "&middot;"); // &#183;
            cString = Strings.Replace(cString, "¸", "&cedil;"); // &#184;
            cString = Strings.Replace(cString, "¹", "&sup1;"); // &#185;
            cString = Strings.Replace(cString, "º", "&ordm;"); // &#186;
            cString = Strings.Replace(cString, "»", "&raquo;"); // &#187;
            cString = Strings.Replace(cString, "¼", "&frac14;"); // &#188;
            cString = Strings.Replace(cString, "½", "&frac12;"); // &#189;
            cString = Strings.Replace(cString, "¾", "&frac34;"); // &#190;
            cString = Strings.Replace(cString, "¿", "&iquest;"); // &#191;
            cString = Strings.Replace(cString, "×", "&times;"); // &#215;
            cString = Strings.Replace(cString, "÷", "&divide;"); // &#247;
            cString = Strings.Replace(cString, "À", "&Agrave;"); // &#192;
            cString = Strings.Replace(cString, "Á", "&Aacute;"); // &#193;
            cString = Strings.Replace(cString, "Â", "&Acirc;"); // &#194;
            cString = Strings.Replace(cString, "Ã", "&Atilde;"); // &#195;
            cString = Strings.Replace(cString, "Ä", "&Auml;"); // &#196;
            cString = Strings.Replace(cString, "Å", "&Aring;"); // &#197;
            cString = Strings.Replace(cString, "Æ", "&AElig;"); // &#198;
            cString = Strings.Replace(cString, "Ç", "&Ccedil;"); // &#199;
            cString = Strings.Replace(cString, "È", "&Egrave;"); // &#200;
            cString = Strings.Replace(cString, "É", "&Eacute;"); // &#201;
            cString = Strings.Replace(cString, "Ê", "&Ecirc;"); // &#202;
            cString = Strings.Replace(cString, "Ë", "&Euml;"); // &#203;
            cString = Strings.Replace(cString, "Ì", "&Igrave;"); // &#204;
            cString = Strings.Replace(cString, "Í", "&Iacute;"); // &#205;
            cString = Strings.Replace(cString, "Î", "&Icirc;"); // &#206;
            cString = Strings.Replace(cString, "Ï", "&Iuml;"); // &#207;
            cString = Strings.Replace(cString, "Ð", "&ETH;"); // &#208;
            cString = Strings.Replace(cString, "Ñ", "&Ntilde;"); // &#209;
            cString = Strings.Replace(cString, "Ò", "&Ograve;"); // &#210;
            cString = Strings.Replace(cString, "Ó", "&Oacute;"); // &#211;
            cString = Strings.Replace(cString, "Ô", "&Ocirc;"); // &#212;
            cString = Strings.Replace(cString, "Õ", "&Otilde;"); // &#213;
            cString = Strings.Replace(cString, "Ö", "&Ouml;"); // &#214;
            cString = Strings.Replace(cString, "Ø", "&Oslash;"); // &#216;
            cString = Strings.Replace(cString, "Ù", "&Ugrave;"); // &#217;
            cString = Strings.Replace(cString, "Ú", "&Uacute;"); // &#218;
            cString = Strings.Replace(cString, "Û", "&Ucirc;"); // &#219;
            cString = Strings.Replace(cString, "Ü", "&Uuml;"); // &#220;
            cString = Strings.Replace(cString, "Ý", "&Yacute;"); // &#221;
            cString = Strings.Replace(cString, "Þ", "&THORN;"); // &#222;
            cString = Strings.Replace(cString, "ß", "&szlig;"); // &#223;
            cString = Strings.Replace(cString, "à", "&agrave;"); // &#224;
            cString = Strings.Replace(cString, "á", "&aacute;"); // &#225;
            cString = Strings.Replace(cString, "â", "&acirc;"); // &#226;
            cString = Strings.Replace(cString, "ã", "&atilde;"); // &#227;
            cString = Strings.Replace(cString, "ä", "&auml;"); // &#228;
            cString = Strings.Replace(cString, "å", "&aring;"); // &#229;
            cString = Strings.Replace(cString, "æ", "&aelig;"); // &#230;
            cString = Strings.Replace(cString, "ç", "&ccedil;"); // &#231;
            cString = Strings.Replace(cString, "è", "&egrave;"); // &#232;
            cString = Strings.Replace(cString, "é", "&eacute;"); // &#233;
            cString = Strings.Replace(cString, "ê", "&ecirc;"); // &#234;
            cString = Strings.Replace(cString, "ë", "&euml;"); // &#235;
            cString = Strings.Replace(cString, "ì", "&igrave;"); // &#236;
            cString = Strings.Replace(cString, "í", "&iacute;"); // &#237;
            cString = Strings.Replace(cString, "î", "&icirc;"); // &#238;
            cString = Strings.Replace(cString, "ï", "&iuml;"); // &#239;
            cString = Strings.Replace(cString, "ð", "&eth;"); // &#240;
            cString = Strings.Replace(cString, "ñ", "&ntilde;"); // &#241;
            cString = Strings.Replace(cString, "ò", "&ograve;"); // &#242;
            cString = Strings.Replace(cString, "ó", "&oacute;"); // &#243;
            cString = Strings.Replace(cString, "ô", "&ocirc;"); // &#244;
            cString = Strings.Replace(cString, "õ", "&otilde;"); // &#245;
            cString = Strings.Replace(cString, "ö", "&ouml;"); // &#246;
            cString = Strings.Replace(cString, "ø", "&oslash;"); // &#248;
            cString = Strings.Replace(cString, "ù", "&ugrave;"); // &#249;
            cString = Strings.Replace(cString, "ú", "&uacute;"); // &#250;
            cString = Strings.Replace(cString, "û", "&ucirc;"); // &#251;
            cString = Strings.Replace(cString, "ü", "&uuml;"); // &#252;
            cString = Strings.Replace(cString, "ý", "&yacute;"); // &#253;
            cString = Strings.Replace(cString, "þ", "&thorn;"); // &#254;
            cString = Strings.Replace(cString, "ÿ", "&yuml;"); // &#255;
            cString = Strings.Replace(cString, "Œ", "&OElig;"); // &#338;
            cString = Strings.Replace(cString, "œ", "&oelig;"); // &#339;
            cString = Strings.Replace(cString, "Š", "&Scaron;"); // &#352;
            cString = Strings.Replace(cString, "š", "&scaron;"); // &#353;
            cString = Strings.Replace(cString, "Ÿ", "&Yuml;"); // &#376;
            cString = Strings.Replace(cString, "ˆ", "&circ;"); // &#710;
            cString = Strings.Replace(cString, "˜", "&tilde;"); // &#732;
            cString = Strings.Replace(cString, "–", "&ndash;"); // &#8211;
            cString = Strings.Replace(cString, "—", "&mdash;"); // &#8212;
            cString = Strings.Replace(cString, "‘", "&lsquo;"); // &#8216;
            cString = Strings.Replace(cString, "’", "&rsquo;"); // &#8217;
            cString = Strings.Replace(cString, "„", "&bdquo;"); // &#8222;
            cString = Strings.Replace(cString, "†", "&dagger;"); // &#8224;
            cString = Strings.Replace(cString, "‡", "&Dagger;"); // &#8225;
            cString = Strings.Replace(cString, "‰", "&permil;"); // &#8240;
            cString = Strings.Replace(cString, "€", "&euro;"); // &#8364;
            cString = Strings.Replace(cString, Conversions.ToString('\r'), "");
            cString = Strings.Replace(cString, Conversions.ToString('\t'), "");
            cString = Strings.Replace(cString, Conversions.ToString(Strings.Chr(160)), " ");
            cString = Strings.Replace(cString, Conversions.ToString('\n'), "");
            cString = Strings.Replace(cString, Constants.vbNewLine, "");
            cString = Strings.Replace(cString, Constants.vbTab, "");
            // Dim i As Integer = 1
            // Do Until i <= 0
            // i = InStr(cString, "  ")
            // cString = Replace(cString, "  ", " ")
            // Loop
            cString = Strings.Replace(cString, "> <", ">&nbsp;<");
            return cString;
        }
    }
}
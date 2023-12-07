using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Web.Configuration;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Protean.Providers.Payment;
using static Protean.stdTools;
using static Protean.Tools.Xml;

namespace Protean
{

    public partial class Cms
    {
        public partial class Cart
        {

            public partial class Subscriptions
            {

                private string mcModuleName = "Subscriptions";
                private System.Collections.Specialized.NameValueCollection oSubConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/subscriptions");

                private Cms myWeb;
                private Cart myCart;
                public bool mbOveridePrices = false;

                public Subscriptions(ref Cms aWeb)
                {
                    myWeb = aWeb;
                    myCart = myWeb.moCart;
                    if (oSubConfig != null)
                    {
                        if (oSubConfig["OveridePrices"] == "on")
                        {
                            mbOveridePrices = true;
                        }
                    }
                }

                public Subscriptions(ref Cart aCart)
                {
                    myCart = aCart;
                    myWeb = myCart.myWeb;
                    if (oSubConfig != null)
                    {
                        if (oSubConfig["OveridePrices"] == "on")
                        {
                            mbOveridePrices = true;
                        }
                    }
                }

                public Subscriptions()
                {
                    // wont do anything here
                }


                #region Admin

                public void ListSubscriptions(ref XmlElement oParentElmt)
                {
                    try
                    {


                        // List Subscription groups and thier subscriptions.
                        var oDS = myWeb.moDbHelper.GetDataSet("Select * From tblCartProductCategories WHERE cCatSchemaName = 'Subscription'", "SubscriptionGroups");
                        myWeb.moDbHelper.addTableToDataSet(ref oDS, "SELECT tblContent.*, tblCartCatProductRelations.nCatId FROM tblContent LEFT OUTER JOIN tblCartCatProductRelations ON tblContent.nContentKey = tblCartCatProductRelations.nContentId WHERE (tblContent.cContentSchemaName = 'Subscription') Order By tblCartCatProductRelations.nDisplayOrder", "Subscriptions");
                        if (oDS.Tables.Count == 2)
                        {
                            oDS.Relations.Add("GrpRel", oDS.Tables["SubscriptionGroups"].Columns["nCatKey"], oDS.Tables["Subscriptions"].Columns["nCatId"], false);
                            oDS.Relations["GrpRel"].Nested = true;
                        }
                        foreach (DataTable oDT in oDS.Tables)
                        {
                            foreach (DataColumn oDC in oDT.Columns)
                                oDC.ColumnMapping = MappingType.Attribute;
                        }
                        if (oDS.Tables.Contains("Subscriptions"))
                        {
                            oDS.Tables["Subscriptions"].Columns["cContentXmlBrief"].ColumnMapping = MappingType.Hidden;
                            oDS.Tables["Subscriptions"].Columns["nCatId"].ColumnMapping = MappingType.Hidden;
                            oDS.Tables["Subscriptions"].Columns["cContentXmlDetail"].ColumnMapping = MappingType.SimpleContent;
                        }
                        var oXML = new XmlDocument();
                        oXML.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&lt;", "<"), "&gt;", ">");
                        foreach (XmlElement oElmt in oXML.DocumentElement.SelectNodes("*"))
                            oParentElmt.AppendChild(oParentElmt.OwnerDocument.ImportNode(oElmt, true));
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "ListSubscriptions", ex, "", "", gbDebug);
                    }
                }

                public int SubscriptionToGroup(int nSubId, int nSubGroup)
                {
                    try
                    {
                        string cSQL = "";
                        int nID = 0;
                        // check if exists
                        cSQL = "SELECT nCatProductRelKey FROM tblCartCatProductRelations WHERE nContentId = " + nSubId + " AND nCatId = " + nSubGroup;
                        nID = Conversions.ToInteger(myWeb.moDbHelper.ExeProcessSqlScalar(cSQL));
                        // if it does then fine, just return id
                        if (nID > 0)
                            return nID;
                        // if not need to get the last order number
                        cSQL = "SELECT nCatProductRelKey, nDisplayOrder FROM tblCartCatProductRelations WHERE nCatId = " + nSubGroup + " ORDER BY nDisplayOrder DESC";
                        nID = Conversions.ToInteger(myWeb.moDbHelper.ExeProcessSqlScalar(cSQL));
                        // add to group as bottom
                        cSQL = "INSERT INTO tblCartCatProductRelations (nContentId, nCatId, nDisplayOrder, nAuditId) VALUES (" + nSubId + ", " + nSubGroup + ", " + (nID + 1) + ", " + myWeb.moDbHelper.getAuditId() + ")";
                        nID = Conversions.ToInteger(myWeb.moDbHelper.GetIdInsertSql(cSQL));
                        return nID;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "SubscriptionToGroup", ex, "", "", gbDebug);
                    }

                    return default;
                }

                public void ListSubscribers(ref XmlElement oParentElmt)
                {
                    try
                    {
                        string sSql = "select dir.cDirName, dir.cDirXml, sub.*, a.* from tblSubscription sub inner join tblDirectory dir on dir.nDirKey = sub.nDirId inner join tblAudit a on a.nAuditKey = sub.nAuditId  where sub.nSubContentId = " + myWeb.moRequest["id"];

                        // List Subscription groups and thier subscriptions.
                        var oDS = myWeb.moDbHelper.GetDataSet(sSql, "Subscribers");
                        var oXML = new XmlDocument();

                        oXML.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&lt;", "<"), "&gt;", ">");
                        string sContent;

                        foreach (XmlElement oElmt in oXML.SelectNodes("descendant-or-self::cDirXml | descendant-or-self::cSubXml"))
                        {
                            sContent = oElmt.InnerXml;
                            if (!string.IsNullOrEmpty(sContent))
                            {
                                oElmt.InnerXml = sContent;
                            }
                        }
                        foreach (XmlElement oElmt2 in oXML.DocumentElement.SelectNodes("*"))
                            oParentElmt.AppendChild(oParentElmt.OwnerDocument.ImportNode(oElmt2, true));
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "ListSubscriptions", ex, "", "", gbDebug);
                    }
                }


                public void ListUpcomingRenewals(ref XmlElement oParentElmt, short expiredMarginDays = -5, string renewRangePeriod = "month", short renewRangeCount = 12, string action = "")
                {
                    try
                    {
                        var StartRangeDate = default(DateTime);
                        var ExpireRange = default(DateTime);
                        switch (Strings.LCase(renewRangePeriod) ?? "")
                        {
                            case "month":
                                {
                                    ExpireRange = DateTime.Now.AddMonths(renewRangeCount * 1);
                                    break;
                                }
                            case "week":
                                {
                                    ExpireRange = DateTime.Now.AddDays(renewRangeCount * 7);
                                    break;
                                }
                            case "day":
                                {
                                    ExpireRange = DateTime.Now.AddDays(renewRangeCount * 1);
                                    break;
                                }
                        }

                        if (expiredMarginDays == Conversions.ToDouble("0"))
                        {
                            XmlElement NextElmt = (XmlElement)oParentElmt.NextSibling;
                            string sNextElementAction = "";
                            if (NextElmt != null)
                            {
                                sNextElementAction = NextElmt.GetAttribute("action");
                            }

                            if ((action ?? "") == (sNextElementAction ?? ""))
                            {
                                switch (Strings.LCase(NextElmt.GetAttribute("period")) ?? "")
                                {
                                    case "month":
                                        {
                                            StartRangeDate = DateTime.Now.AddMonths((int)Math.Round(Conversions.ToDouble(NextElmt.GetAttribute("count")) * 1d));
                                            break;
                                        }
                                    case "week":
                                        {
                                            StartRangeDate = DateTime.Now.AddDays(Conversions.ToDouble(NextElmt.GetAttribute("count")) * 7d);
                                            break;
                                        }
                                    case "day":
                                        {
                                            StartRangeDate = DateTime.Now.AddDays(Conversions.ToDouble(NextElmt.GetAttribute("count")) * 1d);
                                            break;
                                        }
                                }
                            }
                            else
                            {
                                StartRangeDate = DateTime.Now.AddDays(expiredMarginDays);
                            }
                        }
                        else
                        {
                            StartRangeDate = DateTime.Now.AddDays(expiredMarginDays);
                        }


                        string sSql = "select dir.cDirName, dir.cDirXml, sub.*, a.*, pay.cPayMthdProviderName, pay.cPayMthdCardType,pay.cPayMthdDescription, pay.cPayMthdDetailXml, pma.nStatus as nPayMethodStatus, pma.dExpireDate as dPayMethodExpireDate,  al.dDateTime as dActionDate from tblSubscription sub" + " inner join tblDirectory dir on dir.nDirKey = sub.nDirId" + " inner join tblAudit a on a.nAuditKey = sub.nAuditId" + " LEFT OUTER JOIN tblCartPaymentMethod pay on sub.nPaymentMethodId = pay.nPayMthdKey" + " inner join tblAudit pma on pma.nAuditKey = pay.nAuditId" + " LEFT OUTER JOIN tblActivityLog al on nUserDirId = sub.nDirId and nOtherId = sub.nSubKey and cActivityDetail like '" + action + "'" + " where a.dExpireDate >= " + sqlDate(StartRangeDate) + "and a.dExpireDate <= " + sqlDate(ExpireRange) + " and sub.cRenewalStatus = 'Rolling' order by a.dExpireDate";







                        // List Subscription groups and thier subscriptions.
                        var oDS = myWeb.moDbHelper.GetDataSet(sSql, "Subscribers");
                        var oXML = new XmlDocument();

                        oXML.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&lt;", "<"), "&gt;", ">");
                        string sContent;

                        foreach (XmlElement oElmt in oXML.SelectNodes("descendant-or-self::cDirXml | descendant-or-self::cSubXml | descendant-or-self::cPayMthdDetailXml"))
                        {
                            sContent = oElmt.InnerXml;
                            if (!string.IsNullOrEmpty(sContent))
                            {
                                oElmt.InnerXml = sContent;
                            }
                        }
                        foreach (XmlElement oElmt2 in oXML.DocumentElement.SelectNodes("*"))
                            oParentElmt.AppendChild(oParentElmt.OwnerDocument.ImportNode(oElmt2, true));
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "ListSubscriptions", ex, "", "", gbDebug);
                    }
                }

                public void ListRenewalDue(ref XmlElement oParentElmt, short expiredMarginDays = -5, string renewRangePeriod = "month", short renewRangeCount = 12, string action = "")
                {
                    try
                    {
                        DateTime StartRangeDate;
                        var ExpireRange = default(DateTime);
                        switch (Strings.LCase(renewRangePeriod) ?? "")
                        {
                            case "month":
                                {
                                    ExpireRange = DateTime.Now.AddMonths(renewRangeCount * 1);
                                    break;
                                }
                            case "week":
                                {
                                    ExpireRange = DateTime.Now.AddDays(renewRangeCount * 7);
                                    break;
                                }
                            case "day":
                                {
                                    ExpireRange = DateTime.Now.AddDays(renewRangeCount * 1);
                                    break;
                                }
                        }

                        if (expiredMarginDays == Conversions.ToDouble("0"))
                        {
                            XmlElement NextElmt = (XmlElement)oParentElmt.NextSibling;
                            string sNextElementAction = "";
                            if (NextElmt != null)
                            {
                                sNextElementAction = NextElmt.GetAttribute("action");
                            }

                            if ((action ?? "") == (sNextElementAction ?? ""))
                            {
                                switch (Strings.LCase(NextElmt.GetAttribute("period")) ?? "")
                                {
                                    case "month":
                                        {
                                            StartRangeDate = DateTime.Now.AddMonths((int)Math.Round(Conversions.ToDouble(NextElmt.GetAttribute("count")) * 1d));
                                            break;
                                        }
                                    case "week":
                                        {
                                            StartRangeDate = DateTime.Now.AddDays(Conversions.ToDouble(NextElmt.GetAttribute("count")) * 7d);
                                            break;
                                        }
                                    case "day":
                                        {
                                            StartRangeDate = DateTime.Now.AddDays(Conversions.ToDouble(NextElmt.GetAttribute("count")) * 1d);
                                            break;
                                        }
                                }
                            }
                            else
                            {
                                StartRangeDate = DateTime.Now.AddDays(expiredMarginDays);
                            }
                        }
                        else
                        {
                            StartRangeDate = DateTime.Now.AddDays(expiredMarginDays);
                        }


                        string sSql = "select dir.cDirName, dir.cDirXml, sub.*, pay.cPayMthdProviderName, pay.cPayMthdCardType,pay.cPayMthdDescription, pay.cPayMthdDetailXml, a.*, al.dDateTime as dActionDate from tblSubscription sub" + " inner join tblDirectory dir on dir.nDirKey = sub.nDirId" + " inner join tblAudit a on a.nAuditKey = sub.nAuditId" + " LEFT OUTER JOIN tblCartPaymentMethod pay on sub.nPaymentMethodId = pay.nPayMthdKey" + " LEFT OUTER JOIN tblActivityLog al on nUserDirId = sub.nDirId and nOtherId = sub.nSubKey and cActivityDetail like '" + action + "'" + " where a.dExpireDate <= " + sqlDate(ExpireRange) + " and sub.cRenewalStatus = 'Rolling' order by a.dExpireDate";






                        // List Subscription groups and thier subscriptions.
                        var oDS = myWeb.moDbHelper.GetDataSet(sSql, "Subscribers");
                        var oXML = new XmlDocument();

                        oXML.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&lt;", "<"), "&gt;", ">");
                        string sContent;

                        foreach (XmlElement oElmt in oXML.SelectNodes("descendant-or-self::cDirXml | descendant-or-self::cSubXml | descendant-or-self::cPayMthdDetailXml"))
                        {
                            sContent = oElmt.InnerXml;
                            if (!string.IsNullOrEmpty(sContent))
                            {
                                oElmt.InnerXml = sContent;
                            }
                        }
                        foreach (XmlElement oElmt2 in oXML.DocumentElement.SelectNodes("*"))
                            oParentElmt.AppendChild(oParentElmt.OwnerDocument.ImportNode(oElmt2, true));
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "ListSubscriptions", ex, "", "", gbDebug);
                    }
                }

                public void ListRecentRenewals(ref XmlElement oParentElmt)
                {
                    try
                    {
                        string sSql = "select dir.cDirName, dir.cDirXml, sub.*, pay.cPayMthdProviderName, pay.cPayMthdCardType,pay.cPayMthdDescription, pay.cPayMthdDetailXml, a.* from tblSubscription sub" + " inner join tblDirectory dir on dir.nDirKey = sub.nDirId" + " inner join tblAudit a on a.nAuditKey = sub.nAuditId" + " LEFT OUTER JOIN tblCartPaymentMethod pay on sub.nPaymentMethodId = pay.nPayMthdKey" + " where a.dExpireDate >= " + sqlDate(DateTime.Now.AddDays(-5)) + "and a.dExpireDate <= " + sqlDate(DateTime.Now.AddMonths(3)) + " and sub.cRenewalStatus = 'Rolling' order by a.dExpireDate";





                        // List Subscription groups and thier subscriptions.
                        var oDS = myWeb.moDbHelper.GetDataSet(sSql, "Subscribers");
                        var oXML = new XmlDocument();

                        oXML.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&lt;", "<"), "&gt;", ">");
                        string sContent;

                        foreach (XmlElement oElmt in oXML.SelectNodes("descendant-or-self::cDirXml | descendant-or-self::cSubXml | descendant-or-self::cPayMthdDetailXml"))
                        {
                            sContent = oElmt.InnerXml;
                            if (!string.IsNullOrEmpty(sContent))
                            {
                                oElmt.InnerXml = sContent;
                            }
                        }
                        foreach (XmlElement oElmt2 in oXML.DocumentElement.SelectNodes("*"))
                            oParentElmt.AppendChild(oParentElmt.OwnerDocument.ImportNode(oElmt2, true));
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "ListSubscriptions", ex, "", "", gbDebug);
                    }
                }

                public void ListExpiredSubscriptions(ref XmlElement oParentElmt, short expiredMarginDays = 0, string renewRangePeriod = "", short renewRangeCount = 0, string SubType = "", string action = "")
                {
                    try
                    {

                        string ExpireRange = "";
                        switch (Strings.LCase(renewRangePeriod) ?? "")
                        {
                            case "month":
                                {
                                    ExpireRange = sqlDate(DateTime.Now.AddMonths(renewRangeCount * -1));
                                    break;
                                }
                            case "week":
                                {
                                    ExpireRange = sqlDate(DateTime.Now.AddDays(renewRangeCount * -7));
                                    break;
                                }
                            case "day":
                                {
                                    ExpireRange = sqlDate(DateTime.Now.AddDays(renewRangeCount * -1));
                                    break;
                                }
                        }


                        string sSql = "select dir.cDirName, dir.cDirXml, sub.*, pay.cPayMthdProviderName, pay.cPayMthdCardType,pay.cPayMthdDescription, pay.cPayMthdDetailXml, a.*, al.dDateTime as dActionDate from tblSubscription sub" + " inner join tblDirectory dir on dir.nDirKey = sub.nDirId" + " inner join tblAudit a on a.nAuditKey = sub.nAuditId" + " LEFT OUTER JOIN tblCartPaymentMethod pay on sub.nPaymentMethodId = pay.nPayMthdKey" + " LEFT OUTER JOIN tblActivityLog al on nUserDirId = sub.nDirId and nOtherId = sub.nSubKey and cActivityDetail like '" + action + "' and al.dDateTime = (SELECT MAX(dDateTime) FROM   tblActivityLog WHERE  cActivityDetail like '" + action + "' and nOtherId = sub.nSubKey ) ";



                        if (renewRangeCount < 0)
                        {
                            if (renewRangeCount < 0)
                            {
                                sSql = sSql + " where a.dExpireDate <= " + ExpireRange + "and a.dExpireDate >= " + sqlDate(DateTime.Now.AddDays(expiredMarginDays * -1));
                            }

                            else
                            {
                                sSql = sSql + " where a.dExpireDate >= " + ExpireRange + "and a.dExpireDate <= " + sqlDate(DateTime.Now.AddDays(expiredMarginDays * -1));
                            }
                        }

                        else
                        {
                            sSql = sSql + " where a.dExpireDate <= " + sqlDate(DateTime.Now.AddDays(expiredMarginDays * -1));
                        }

                        if (!string.IsNullOrEmpty(SubType))
                        {
                            sSql = sSql + " and sub.cRenewalStatus  = '" + SubType + "'";
                        }

                        sSql = sSql + " and sub.cRenewalStatus <> 'Cancelled'  order by a.dExpireDate desc";

                        // List Subscription groups and thier subscriptions.
                        var oDS = myWeb.moDbHelper.GetDataSet(sSql, "Subscribers");
                        var oXML = new XmlDocument();

                        oXML.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&lt;", "<"), "&gt;", ">");
                        string sContent;

                        foreach (XmlElement oElmt in oXML.SelectNodes("descendant-or-self::cDirXml | descendant-or-self::cSubXml | descendant-or-self::cPayMthdDetailXml"))
                        {
                            sContent = oElmt.InnerXml;
                            if (!string.IsNullOrEmpty(sContent))
                            {
                                oElmt.InnerXml = sContent;
                            }
                        }
                        foreach (XmlElement oElmt2 in oXML.DocumentElement.SelectNodes("*"))
                            oParentElmt.AppendChild(oParentElmt.OwnerDocument.ImportNode(oElmt2, true));
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "ListSubscriptions", ex, "", "", gbDebug);
                    }
                }

                public void ListCancelledSubscriptions(ref XmlElement oParentElmt)
                {
                    try
                    {
                        string sSql = "select dir.cDirName, dir.cDirXml, sub.*, pay.cPayMthdProviderName, pay.cPayMthdCardType,pay.cPayMthdDescription, pay.cPayMthdDetailXml, a.* from tblSubscription sub" + " inner join tblDirectory dir on dir.nDirKey = sub.nDirId" + " inner join tblAudit a on a.nAuditKey = sub.nAuditId" + " LEFT OUTER JOIN tblCartPaymentMethod pay on sub.nPaymentMethodId = pay.nPayMthdKey" + " where sub.cRenewalStatus = 'Cancelled'  order by a.dExpireDate desc";




                        // List Subscription groups and thier subscriptions.
                        var oDS = myWeb.moDbHelper.GetDataSet(sSql, "Subscribers");
                        var oXML = new XmlDocument();

                        oXML.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&lt;", "<"), "&gt;", ">");
                        string sContent;

                        foreach (XmlElement oElmt in oXML.SelectNodes("descendant-or-self::cDirXml | descendant-or-self::cSubXml | descendant-or-self::cPayMthdDetailXml"))
                        {
                            sContent = oElmt.InnerXml;
                            if (!string.IsNullOrEmpty(sContent))
                            {
                                oElmt.InnerXml = sContent;
                            }
                        }
                        foreach (XmlElement oElmt2 in oXML.DocumentElement.SelectNodes("*"))
                            oParentElmt.AppendChild(oParentElmt.OwnerDocument.ImportNode(oElmt2, true));
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "ListSubscriptions", ex, "", "", gbDebug);
                    }
                }

                public XmlElement GetSubscriptionDetail(ref XmlElement oParentElmt, int nSubId)
                {
                    string sSQL;
                    DataSet oDs;
                    XmlElement oElmt;
                    try
                    {

                        if (oParentElmt is null)
                        {
                            oParentElmt = myWeb.moPageXml.CreateElement("Subscription");
                        }

                        sSQL = "select a.nStatus as status,  nSubKey as id, nSubContentId as contentId, nDirId as userId, nOrderId as orderId, cSubName as name, cSubXml, dStartDate, a.dPublishDate, a.dExpireDate, a.cDescription as cancelReason,a.dUpdateDate as cancelDate,a.nUpdateDirId as cancelUserId, nPeriod as period, cPeriodUnit as periodUnit, nValueNet as value, cRenewalStatus as renewalStatus, pay.nPayMthdKey as providerId,pay.cPayMthdProviderName as providerName, pay.cPayMthdProviderRef as providerRef" + " from tblSubscription sub INNER JOIN tblAudit a ON sub.nAuditId = a.nAuditKey " + " LEFT OUTER JOIN tblCartPaymentMethod pay on pay.nPayMthdKey = sub.nPaymentMethodId " + " where sub.nSubKey = " + nSubId;
                        myWeb.moContentDetail = myWeb.moPageXml.CreateElement("ContentDetail");

                        oDs = myWeb.moDbHelper.GetDataSet(sSQL, "tblSubscription", "OrderList");
                        foreach (DataRow oDr in oDs.Tables[0].Rows)
                        {
                            oElmt = myWeb.moPageXml.CreateElement("Subscription");
                            oElmt.InnerXml = Conversions.ToString(oDr["cSubXml"]);
                            oElmt.SetAttribute("status", oDr["status"].ToString());
                            oElmt.SetAttribute("id", oDr["id"].ToString());
                            oElmt.SetAttribute("contentId", oDr["contentId"].ToString());
                            oElmt.SetAttribute("userId", oDr["userId"].ToString());
                            oElmt.SetAttribute("orderId", oDr["orderId"].ToString());
                            oElmt.SetAttribute("name", oDr["name"].ToString());
                            oElmt.SetAttribute("startDate", XmlDate(oDr["dStartDate"]));
                            oElmt.SetAttribute("publishDate", XmlDate(oDr["dPublishDate"]));
                            oElmt.SetAttribute("expireDate", XmlDate(oDr["dExpireDate"]));
                            oElmt.SetAttribute("period", oDr["period"].ToString());
                            oElmt.SetAttribute("periodUnit", Strings.Trim(oDr["periodUnit"].ToString()));
                            oElmt.SetAttribute("value", oDr["value"].ToString());
                            oElmt.SetAttribute("renewalStatus", oDr["renewalStatus"].ToString());
                            oElmt.SetAttribute("providerId", oDr["providerId"].ToString());
                            oElmt.SetAttribute("providerName", oDr["providerName"].ToString());
                            oElmt.SetAttribute("providerRef", oDr["providerRef"].ToString());
                            oElmt.SetAttribute("cancelReason", oDr["cancelReason"].ToString());
                            oElmt.SetAttribute("cancelDate", oDr["cancelDate"].ToString());
                            oElmt.SetAttribute("cancelUserId", oDr["cancelUserId"].ToString());

                            if (!string.IsNullOrEmpty(oDr["providerName"].ToString()))
                            {
                                //var oPayProv = new Providers.Payment.BaseProvider(ref myWeb, oDr["providerName"].ToString());
                                Protean.Providers.Payment.ReturnProvider oPayProv = new Protean.Providers.Payment.ReturnProvider();
                                IPaymentProvider oPaymentProv = oPayProv.Get(ref myWeb, oDr["providerName"].ToString());
                                   // Dim paymentStatus As String
                                try
                                {
                                    // paymentStatus = oPayProv.Activities.CheckStatus(myWeb, oDr("providerId"))
                                    // oElmt.SetAttribute("paymentStatus", paymentStatus)
                                    long providerid = Convert.ToInt64(oDr["providerId"]);
                                    oElmt.AppendChild((XmlNode)oPaymentProv.Activities.UpdatePaymentStatus(ref myWeb, ref providerid));
                                }
                                catch (Exception)
                                {
                                    oElmt.SetAttribute("paymentStatus", "error");
                                }
                            }
                            else
                            {
                                oElmt.SetAttribute("paymentStatus", "unknown");
                            }

                            // Get user Info
                            oElmt.AppendChild(myWeb.GetUserXML(Conversions.ToLong(oDr["userId"])));

                            // Get the renewal Info
                            sSQL = "select a.dPublishDate as startDate, a.dExpireDate as endDate, sub.nPaymentMethodId as payMthdId,  pay.cPayMthdProviderName as providerName, sub.xNotesXml, sub.nOrderId as orderId" + " from tblSubscriptionRenewal sub INNER JOIN tblAudit a ON sub.nAuditId = a.nAuditKey " + " LEFT OUTER JOIN tblCartPaymentMethod pay on pay.nPayMthdKey = sub.nPaymentMethodId " + " where sub.nSubId = " + nSubId;

                            oDs = myWeb.moDbHelper.GetDataSet(sSQL, "Renewal", "Renewals");
                            if (oDs != null)
                            {
                                oDs.Tables[0].Columns["startDate"].ColumnMapping = MappingType.Attribute;
                                oDs.Tables[0].Columns["endDate"].ColumnMapping = MappingType.Attribute;
                                oDs.Tables[0].Columns["providerName"].ColumnMapping = MappingType.Attribute;
                                oDs.Tables[0].Columns["orderId"].ColumnMapping = MappingType.Attribute;
                                oDs.Tables[0].Columns["payMthdId"].ColumnMapping = MappingType.Attribute;
                                oDs.Tables[0].Columns["xNotesXml"].ColumnMapping = MappingType.SimpleContent;

                                var elmtRenewals = myWeb.moPageXml.CreateElement("Renewals");
                                elmtRenewals.InnerXml = oDs.GetXml();
                                foreach (XmlElement renewalElmt in elmtRenewals.SelectNodes("Renewals/Renewal"))
                                    renewalElmt.InnerXml = renewalElmt.InnerText;
                                oElmt.AppendChild(elmtRenewals.FirstChild);
                            }

                            oParentElmt.AppendChild(oElmt);
                            oDs = null;
                        }
                        return oParentElmt;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "GetSubscriptionDetail", ex, "", "", gbDebug);
                        return null;
                    }
                }

                public void ListRenewalAlerts(ref XmlElement oParentElmt, bool bProcess = false)
                {
                    try
                    {
                        XmlElement moReminderCfg = (XmlElement)WebConfigurationManager.GetWebApplicationSection("protean/subscriptionReminders");
                        oParentElmt.InnerXml = moReminderCfg.OuterXml;
                        long ProcessedCount = 0L;

                        if (myWeb.moRequest["ewCmd2"] == "processAll")
                        {
                            bProcess = true;
                        }

                        foreach (XmlElement oReminder in oParentElmt.SelectNodes("subscriptionReminders/reminder"))
                        {

                            switch (oReminder.GetAttribute("action") ?? "")
                            {
                                case "renewalreminder":
                                    {
                                        // Select the subscriptions that are caught up in this case
                                        XmlElement xmloReminder = oReminder;
                                        ListUpcomingRenewals(ref xmloReminder, (short)Conversions.ToInteger("0" + xmloReminder.GetAttribute("startRange")), oReminder.GetAttribute("period"), Conversions.ToShort(oReminder.GetAttribute("count")), oReminder.GetAttribute("name"));
                                        foreach (XmlElement subxml in oReminder.SelectNodes("Subscribers"))
                                        {
                                            bool force = false;
                                            bool ingoreIfPaymentActive = false;
                                            string actionResult;
                                            if ((myWeb.moRequest["name"] ?? "") == (oReminder.GetAttribute("name") ?? "") & (myWeb.moRequest["SendId"] ?? "") == (subxml.SelectSingleNode("nSubKey").InnerText ?? ""))
                                            {
                                                force = true;
                                            }

                                            if (oReminder.GetAttribute("invalidPaymentOnly") != null)
                                            {

                                                if (oReminder.GetAttribute("invalidPaymentOnly") == "true")
                                                {
                                                    ingoreIfPaymentActive = true;
                                                }
                                            }
                                            DateTime ActionDate = default;
                                            if (subxml.SelectSingleNode("dActionDate") != null)
                                            {
                                                ActionDate = Conversions.ToDate(subxml.SelectSingleNode("dActionDate").InnerText);
                                            }



                                            long argSubId = Conversions.ToLong(subxml.SelectSingleNode("nSubKey").InnerText);
                                            actionResult = RenewalAction(subxml, ref argSubId, oReminder.GetAttribute("action"), ref ProcessedCount, oReminder.GetAttribute("name"), bProcess, force, ingoreIfPaymentActive, ActionDate);
                                            subxml.SetAttribute("actionResult", actionResult);
                                        }

                                        break;
                                    }
                                case "renew":
                                    {
                                        // Select the subscriptions that are caught up in this case
                                        XmlElement xmloReminder = oReminder;
                                        ListRenewalDue(ref xmloReminder, (short)Conversions.ToInteger("0" + oReminder.GetAttribute("startRange")), oReminder.GetAttribute("period"), Conversions.ToShort(oReminder.GetAttribute("count")), oReminder.GetAttribute("name"));
                                        foreach (XmlElement subxml in oReminder.SelectNodes("Subscribers"))
                                        {
                                            bool force = false;
                                            bool ingoreIfPaymentActive = false;
                                            string actionResult;
                                            if ((myWeb.moRequest["name"] ?? "") == (oReminder.GetAttribute("name") ?? "") & (myWeb.moRequest["SendId"] ?? "") == (subxml.SelectSingleNode("nSubKey").InnerText ?? ""))
                                            {
                                                force = true;
                                            }

                                            if (oReminder.GetAttribute("invalidPaymentOnly") != null)
                                            {

                                                if (oReminder.GetAttribute("invalidPaymentOnly") == "true")
                                                {
                                                    ingoreIfPaymentActive = true;
                                                }
                                            }
                                            DateTime ActionDate = default;
                                            if (subxml.SelectSingleNode("dActionDate") != null)
                                            {
                                                ActionDate = Conversions.ToDate(subxml.SelectSingleNode("dActionDate").InnerText);
                                            }

                                            long argSubId1 = Conversions.ToLong(subxml.SelectSingleNode("nSubKey").InnerText);
                                            actionResult = RenewalAction(subxml, ref argSubId1, oReminder.GetAttribute("action"), ref ProcessedCount, oReminder.GetAttribute("name"), bProcess, force, ingoreIfPaymentActive, ActionDate);
                                            subxml.SetAttribute("actionResult", actionResult);
                                        }

                                        break;
                                    }
                                case "expire":
                                case "expired":
                                case "expirewarning":
                                    {
                                        XmlElement xmloReminder = oReminder;
                                        if (oReminder.GetAttribute("action") == "expire")
                                        {                                           
                                            ListExpiredSubscriptions(ref xmloReminder, Conversions.ToShort(oReminder.GetAttribute("count")), "", 0, Conversions.ToString(true));
                                        }
                                        else
                                        {
                                            ListExpiredSubscriptions(ref xmloReminder, 0, oReminder.GetAttribute("period"), Conversions.ToShort(oReminder.GetAttribute("count")), oReminder.GetAttribute("subType"), oReminder.GetAttribute("name"));
                                        }
                                        foreach (XmlElement subxml in oReminder.SelectNodes("Subscribers"))
                                        {
                                            bool force = false;
                                            if ((myWeb.moRequest["name"] ?? "") == (oReminder.GetAttribute("name") ?? "") & (myWeb.moRequest["SendId"] ?? "") == (subxml.SelectSingleNode("nSubKey").InnerText ?? ""))
                                            {
                                                force = true;
                                            }
                                            if (bProcess)
                                            {
                                                force = true;
                                            }
                                            bool ingoreIfPaymentActive = false;
                                            string actionResult;
                                            if ((myWeb.moRequest["name"] ?? "") == (oReminder.GetAttribute("name") ?? "") & (myWeb.moRequest["SendId"] ?? "") == (subxml.SelectSingleNode("nSubKey").InnerText ?? ""))
                                            {
                                                force = true;
                                            }
                                            if (oReminder.GetAttribute("invalidPaymentOnly") != null)
                                            {
                                                if (oReminder.GetAttribute("invalidPaymentOnly") == "true")
                                                {
                                                    ingoreIfPaymentActive = true;
                                                }
                                            }
                                            DateTime ActionDate = default;
                                            if (subxml.SelectSingleNode("dActionDate") != null)
                                            {
                                                ActionDate = Conversions.ToDate(subxml.SelectSingleNode("dActionDate").InnerText);
                                            }
                                            long argSubId2 = Conversions.ToLong(subxml.SelectSingleNode("nSubKey").InnerText);
                                            actionResult = RenewalAction(subxml, ref argSubId2, oReminder.GetAttribute("action"), ref ProcessedCount, oReminder.GetAttribute("name"), bProcess, force, ingoreIfPaymentActive, ActionDate);
                                            subxml.SetAttribute("actionResult", actionResult);
                                        }

                                        break;
                                    }

                            }

                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "GetSubscriptionDetail", ex, "", "", gbDebug);
                    }
                }

                public string RenewalAction(XmlElement RootXml, ref long SubId, string Action, ref long ProcessedCount, string messageType, bool process, bool force, bool ingoreIfPaymentActive, DateTime actionDate = default)
                {
                    string actionResult = "";
                    ProcessedCount = ProcessedCount + 1L;

                    try
                    {
                        XmlElement argoParentElmt = null;
                        var SubXml = GetSubscriptionDetail(ref argoParentElmt, (int)SubId);

                        SubXml.SetAttribute("messageType", messageType);
                        SubXml.SetAttribute("action", Action);

                        RootXml.AppendChild(SubXml);

                        string UserEmail = SubXml.SelectSingleNode("Subscription/User/Email").InnerText;
                        string UserId = SubXml.SelectSingleNode("Subscription/User/@id").InnerText;
                        var oMessager = new Protean.Messaging(ref myWeb.msException);

                        bool PaymentActive = false;
                        // If SubXml.SelectSingleNode("Subscription/@paymentStatus").InnerText = "active" Then
                        // PaymentActive = True
                        // End If

                        switch (Action ?? "")
                        {
                            case "renewalreminder":
                                {

                                    // Dim sSql As String = "Select dDateTime from tblActivityLog where nUserDirId = " & UserId & " and nOtherId = " & SubId & " and cActivityDetail like '" & SqlFmt(messageType) & "'"
                                    // Dim actionDate As DateTime = myWeb.moDbHelper.GetDataValue(sSql)

                                    if (actionDate == Conversions.ToDate("#1/1/0001 12:00:00 AM#") | force & gbDebug)
                                    {

                                        if (PaymentActive & ingoreIfPaymentActive)
                                        {
                                            actionResult = "not required";
                                        }
                                        else if (force | process)
                                        {
                                            Cms.dbHelper argodbHelper = null;
                                            string cRetMessage = Conversions.ToString(oMessager.emailer(SubXml, oSubConfig["ReminderXSL"], oSubConfig["SubscriptionEmailName"], oSubConfig["SubscriptionEmail"], UserEmail, "", bccRecipient: oSubConfig["bccReminders"], odbHelper: ref argodbHelper));
                                            myWeb.moDbHelper.logActivity(Cms.dbHelper.ActivityType.SubscriptionAlert, Conversions.ToLong(UserId), 0L, 0L, SubId, messageType, false);
                                            actionResult = "sent";
                                        }
                                        else
                                        {
                                            actionResult = "not sent";
                                        }
                                    }
                                    else
                                    {
                                        actionResult = Conversions.ToString(actionDate);
                                    }

                                    break;
                                }

                            case "renew":
                                {
                                    if (force | process)
                                    {
                                        switch (RenewSubscription((XmlElement)SubXml.FirstChild, true) ?? "")
                                        {
                                            case "Success":
                                                {
                                                    actionResult = "Renewed";
                                                    break;
                                                }
                                            case "Failed":
                                                {
                                                    actionResult = "Renewal Failed";
                                                    SubXml.SetAttribute("actionResult", actionResult);
                                                    Cms.dbHelper argodbHelper1 = null;
                                                    string cRetMessage = Conversions.ToString(oMessager.emailer(SubXml, oSubConfig["ReminderXSL"], oSubConfig["SubscriptionEmailName"], oSubConfig["SubscriptionEmail"], UserEmail, "", bccRecipient: oSubConfig["bccReminders"], odbHelper: ref argodbHelper1));
                                                    break;
                                                }
                                        }
                                    }
                                    else
                                    {
                                        actionResult = "To Renew";
                                    }

                                    break;
                                }
                            case "expire":
                                {
                                    if (force)
                                    {
                                        actionResult = ExpireSubscription((int)SubId, "Scheduled Expiration");
                                    }
                                    else
                                    {
                                        actionResult = "To Expire";
                                    }

                                    break;
                                }

                            case "expired":
                            case "expirewarning":
                                {
                                    // Dim sSql As String = "Select dDateTime from tblActivityLog where nUserDirId = " & UserId & " and nOtherId = " & SubId & " and cActivityDetail like '" & SqlFmt(messageType) & "'"
                                    // Dim actionDate As DateTime = myWeb.moDbHelper.GetDataValue(sSql)
                                    if (actionDate == Conversions.ToDate("#1/1/0001 12:00:00 AM#") | force & gbDebug)
                                    {

                                        if (PaymentActive & ingoreIfPaymentActive)
                                        {
                                            actionResult = "not required";
                                        }
                                        else if (force)
                                        {
                                            Cms.dbHelper argodbHelper2 = null;
                                            string cRetMessage = Conversions.ToString(oMessager.emailer(SubXml, oSubConfig["ReminderXSL"], oSubConfig["SubscriptionEmailName"], oSubConfig["SubscriptionEmail"], UserEmail, "", bccRecipient: oSubConfig["bccReminders"], odbHelper: ref argodbHelper2));
                                            myWeb.moDbHelper.logActivity(Cms.dbHelper.ActivityType.SubscriptionAlert, Conversions.ToLong(UserId), 0L, 0L, SubId, messageType, false);
                                            actionResult = "sent";
                                        }
                                        else
                                        {
                                            actionResult = "not sent";
                                        }
                                    }
                                    else
                                    {
                                        actionResult = Conversions.ToString(actionDate);
                                    }

                                    break;
                                }
                        }

                        return actionResult;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "RenewalAction", ex, "", "", gbDebug);
                        return ex.Message;
                    }


                }


                #endregion

                #region Cart

                // add repeating payment instructions to order.
                public void UpdateSubscriptionsTotals(ref XmlElement oCartXml)
                {

                    var repeatPrice = default(double);
                    string repeatInterval = string.Empty;
                    int repeatFrequency = 1;
                    string interval = string.Empty;
                    var length = default(int);
                    var minimumTerm = default(int);
                    var renewalTerm = default(int);
                    string startDate = string.Empty;
                    string delayStart = string.Empty;
                    var vatAmt = default(double);

                    bool mbRoundup = false;
                    try
                    {

                        double VatRate = Conversions.ToDouble(oCartXml.GetAttribute("vatRate"));


                        foreach (XmlElement oelmt in oCartXml.SelectNodes("descendant-or-self::Item[productDetail/SubscriptionPrices]"))
                        {
                            if (oelmt.SelectSingleNode("productDetail/StartDate") != null)
                            {
                                if (Information.IsDate(oelmt.SelectSingleNode("productDetail/StartDate").InnerText))
                                {
                                    startDate = XmlDate(oelmt.SelectSingleNode("productDetail/StartDate").InnerText);
                                }
                                else
                                {
                                    startDate = XmlDate(DateTime.Now);
                                }
                            }
                            else
                            {
                                startDate = XmlDate(DateTime.Now);
                            }
                            repeatPrice = repeatPrice + Conversions.ToDouble("0" + oelmt.SelectSingleNode("productDetail/SubscriptionPrices/Price[@type='sale']").InnerText);
                            repeatInterval = oelmt.SelectSingleNode("productDetail/PaymentUnit").InnerText;
                            repeatFrequency = 1;
                            if (oelmt.SelectSingleNode("productDetail/PaymentFrequency") != null)
                            {
                                if (Information.IsNumeric(oelmt.SelectSingleNode("productDetail/PaymentFrequency").InnerText))
                                {
                                    repeatFrequency = Conversions.ToInteger(oelmt.SelectSingleNode("productDetail/PaymentFrequency").InnerText);
                                }
                            }
                            interval = oelmt.SelectSingleNode("productDetail/Duration/Unit").InnerText;
                            length = Conversions.ToInteger("0" + oelmt.SelectSingleNode("productDetail/Duration/Length").InnerText);
                            minimumTerm = Conversions.ToInteger(oelmt.SelectSingleNode("productDetail/Duration/MinimumTerm").InnerText);
                            renewalTerm = Conversions.ToInteger(oelmt.SelectSingleNode("productDetail/Duration/RenewalTerm").InnerText);
                            XmlElement SubPrices = (XmlElement)oelmt.SelectSingleNode("productDetail/SubscriptionPrices");
                            delayStart = SubPrices.GetAttribute("delayStart");
                            vatAmt = (double)Round(repeatPrice * (VatRate / 100d), bForceRoundup: mbRoundup);
                        }
                        repeatPrice = Conversions.ToDouble(Strings.FormatNumber(repeatPrice + vatAmt, (int)TriState.True, TriState.False, TriState.False));
                        oCartXml.SetAttribute("repeatPrice", repeatPrice.ToString());
                        oCartXml.SetAttribute("repeatVAT", Strings.FormatNumber(vatAmt, 2, TriState.True, TriState.False, TriState.False));
                        oCartXml.SetAttribute("repeatInterval", repeatInterval);
                        oCartXml.SetAttribute("repeatFrequency", repeatFrequency.ToString());
                        oCartXml.SetAttribute("interval", interval);
                        oCartXml.SetAttribute("repeatLength", length.ToString());
                        oCartXml.SetAttribute("repeatMinimumTerm", minimumTerm.ToString());
                        oCartXml.SetAttribute("repeatRenewalTerm", renewalTerm.ToString());
                        oCartXml.SetAttribute("delayStart", delayStart);
                        oCartXml.SetAttribute("startDate", startDate);

                        // oCartXml.SetAttribute("payableAmount", oCartXml.GetAttribute("total") - SubscriptionPrice(repeatPrice, repeatInterval, length, interval, xmlDate(Now())))
                        // Payable amount should be the setup cost TS commented out the above line 01/11/2017

                        oCartXml.SetAttribute("payableAmount", oCartXml.GetAttribute("total"));

                        oCartXml.SetAttribute("payableType", "Initial Payment");
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "UpdateSubsTotals", ex, "", "", gbDebug);
                    }
                }

                public bool CheckCartForSubscriptions(int nCartID, int nSubUserId)
                {
                    try
                    {


                        // this will:
                        // 1) Make sure there is only 1 subscription per subscritpion group (will remove the least valuable)
                        // 2) Change all subscription quantities to 1 (you dont want more)
                        // 3) Return true if there are subscription and user is logged in, OR, no subscriptions. Returns false if there are subscriptions but not logged in
                        string cSQL = "SELECT tblCartItem.nCartItemKey, tblContent.nContentKey, tblContent.cContentXmlDetail, tblCartCatProductRelations.nCatId, tblCartCatProductRelations.nDisplayOrder" + " FROM tblCartItem INNER JOIN" + " tblContent ON tblCartItem.nItemId = tblContent.nContentKey LEFT OUTER JOIN" + " tblCartCatProductRelations ON tblContent.nContentKey = tblCartCatProductRelations.nContentId" + " WHERE (tblCartItem.nCartOrderId = " + nCartID + ") AND (tblContent.cContentSchemaName = N'Subscription')" + " ORDER BY tblCartItem.nCartItemKey";
                        // " ORDER BY tblCartCatProductRelations.nDisplayOrder"
                        var oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Subs");
                        if (oDS.Tables["Subs"].Rows.Count > 0)
                        {
                            // create a copy table for the end results
                            var oDT = oDS.Tables["Subs"].Copy();
                            oDT.TableName = "ActualSubs";
                            oDS.Tables.Add(oDT);
                            DataRow oDR2;
                            // first lets see if it is the only one in that group
                            foreach (DataRow oDR in oDS.Tables["Subs"].Rows)
                            {
                                if (Information.IsNumeric(oDR["nCatId"]))
                                {
                                // It has a category so we go through the actual table and remove others of a lower value
                                RedoCheck:
                                    ;

                                    foreach (DataRow currentODR2 in oDS.Tables["ActualSubs"].Rows)
                                    {
                                        oDR2 = currentODR2;
                                        if (!(oDR2.RowState == DataRowState.Deleted))
                                        {

                                            if (Conversions.ToBoolean(Operators.AndObject(Operators.AndObject(Operators.ConditionalCompareObjectEqual(oDR2["nCatId"], oDR["nCatId"], false), !Operators.ConditionalCompareObjectEqual(oDR2["nContentKey"], oDR["nContentKey"], false)), Operators.ConditionalCompareObjectLess(oDR2["nCartItemKey"], oDR["nCartItemKey"], false))))
                                            {
                                                // oDR2("nDisplayOrder") < oDR("nDisplayOrder") Then

                                                myCart.RemoveItem(Convert.ToInt64(oDR2["nCartItemKey"]));
                                                oDR2.Delete();
                                                goto RedoCheck;
                                            }
                                        }
                                    }
                                }
                            }
                            // now we go through and make sure all quantities are 0
                            foreach (DataRow currentODR21 in oDS.Tables["ActualSubs"].Rows)
                            {
                                oDR2 = currentODR21;
                                if (!(oDR2.RowState == DataRowState.Deleted))
                                {
                                    cSQL = Conversions.ToString(Operators.ConcatenateObject("UPDATE tblCartItem SET nQuantity = 1 WHERE nCartItemKey = ", oDR2["nCartItemKey"]));
                                    myWeb.moDbHelper.ExeProcessSql(cSQL);
                                }
                            }
                            // now check the user is logged in
                            if (nSubUserId > 0)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            // no subscriptions
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CheckCartForSubscriptions", ex, "", "", gbDebug);
                    }

                    return default;
                }

                public double CartSubscriptionPrice(int nSubscriptionID, int nSubUserId)
                {
                    try
                    {
                        double nTotalPrice = 0d;

                        // first we need to fin out if its:
                        // 1) New (No existing ones in the same group or none at all)
                        // 2) Renewal (so it tacks onto the end)
                        // 3) Upgrade/Downgrade (so it takes the remaining credit and starts it from today

                        // First we'll get the xml for this subscription
                        string cSQL = "SELECT tblContent.*, tblCartCatProductRelations.nCatId FROM tblContent Left Outer JOIN tblCartCatProductRelations ON tblContent.nContentKey = tblCartCatProductRelations.nContentId where tblContent.nContentKey = " + nSubscriptionID;
                        var oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Content");
                        DataColumn oDC;
                        foreach (DataColumn currentODC in oDS.Tables["Content"].Columns)
                        {
                            oDC = currentODC;
                            oDC.ColumnMapping = MappingType.Attribute;
                        }
                        oDS.Tables["Content"].Columns["cContentXmlBrief"].ColumnMapping = MappingType.Hidden;
                        oDS.Tables["Content"].Columns["cContentXmlDetail"].ColumnMapping = MappingType.SimpleContent;
                        var oXML = new XmlDocument();

                        oXML.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&gt;", ">"), "&lt;", "<");
                        XmlElement oCurSubElmt = (XmlElement)oXML.DocumentElement.FirstChild;
                        string cGroup = "";
                        if (oCurSubElmt != null)
                        {
                            cGroup = oCurSubElmt.GetAttribute("nCatId");
                        }

                        // Ok, lets get any other  subscriptions in the same  group
                        cSQL = "Select tblSubscription.*, tblAudit.*, tblCartCatProductRelations.nCatId" + " FROM tblSubscription INNER JOIN tblAudit ON tblSubscription.nAuditId = tblAudit.nAuditKey LEFT OUTER JOIN" + " tblCartCatProductRelations ON tblSubscription.nSubContentId = tblCartCatProductRelations.nContentId";
                        if (string.IsNullOrEmpty(cGroup))
                        {
                            cSQL += " WHERE tblSubscription.nDirId = " + nSubUserId + " AND tblSubscription.nSubContentId = " + nSubscriptionID;
                        }
                        else
                        {
                            cSQL += " WHERE (tblSubscription.nDirId = " + nSubUserId + ") AND (tblSubscription.nSubContentId = " + nSubscriptionID + " OR tblCartCatProductRelations.nCatId = " + cGroup + ")";
                        }
                        cSQL += " AND tblSubscription.cRenewalStatus <> 'Cancelled' AND tblAudit.dExpireDate > " + sqlDate(DateTime.Now) + " AND tblAudit.dPublishDate < " + sqlDate(DateTime.Now) + " order by tblAudit.dExpireDate DESC";

                        oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Content");
                        if (oDS.Tables["Content"].Rows.Count == 0)
                        {
                            // No Other subscriptions or subscriptions in the same group
                            double nPrice = myCart.getProductPricesByXml(oCurSubElmt.InnerXml, "", 1);
                            double nRptPrice = myCart.getProductPricesByXml(oCurSubElmt.InnerXml, "", 1, "SubscriptionPrices");
                            int nPaymentFrequency = 1;
                            if (oCurSubElmt.SelectSingleNode("Content/PaymentFrequency") != null)
                            {
                                if (Information.IsNumeric(oCurSubElmt.SelectSingleNode("Content/PaymentFrequency").InnerText))
                                {
                                    nPaymentFrequency = Conversions.ToInteger(oCurSubElmt.SelectSingleNode("Content/PaymentFrequency").InnerText);
                                }
                            }
                            if (oCurSubElmt.SelectSingleNode("Content/SubscriptionPrices/@delayStart").Value == "true")
                            {
                                nTotalPrice = nPrice;
                            }
                            else
                            {
                                nTotalPrice = nPrice + SubscriptionPrice(nRptPrice, oCurSubElmt.SelectSingleNode("Content/PaymentUnit").InnerText, nPaymentFrequency, oCurSubElmt.SelectSingleNode("Content/Duration/Unit").InnerText, DateTime.Now);
                            }
                        }


                        else
                        {
                            // okies, there is some stuff in here.
                            // now there should only be one.
                            // Either the same or one in the same group
                            var oExXML = new XmlDocument();
                            foreach (DataColumn currentODC1 in oDS.Tables["Content"].Columns)
                            {
                                oDC = currentODC1;
                                oDC.ColumnMapping = MappingType.Attribute;
                            }
                            // oDS.Tables("Content").Columns("cContentXmlBrief").ColumnMapping = MappingType.Hidden
                            // oDS.Tables("Content").Columns("cContentXmlDetail").ColumnMapping = MappingType.SimpleContent
                            oDS.Tables["Content"].Columns["cSubXML"].ColumnMapping = MappingType.SimpleContent;
                            oExXML.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&gt;", ">"), "&lt;", "<");
                            XmlElement oNewElmt = (XmlElement)oXML.DocumentElement.FirstChild;
                            XmlElement oOldElmt = (XmlElement)oExXML.DocumentElement.FirstChild;
                            double nCurPrice = myCart.getProductPricesByXml(oNewElmt.InnerXml, "", 1);
                            nTotalPrice = SubscriptionPrice(nCurPrice, oNewElmt.SelectSingleNode("Content/PaymentUnit").InnerText, Conversions.ToInteger(oNewElmt.SelectSingleNode("Content/Duration/Length").InnerText), oNewElmt.SelectSingleNode("Content/Duration/Unit").InnerText, Conversions.ToDate(oOldElmt.GetAttribute("dExpireDate")));
                            if (Conversions.ToBoolean(!Operators.ConditionalCompareObjectEqual(oDS.Tables["Content"].Rows[0]["nSubContentId"], nSubscriptionID, false)))
                            {
                                nTotalPrice = (double)(nTotalPrice - UpgradeCredit(myCart.getProductPricesByXml(oOldElmt.InnerXml, "", 1), oOldElmt.SelectSingleNode("Content/PaymentUnit").InnerText, DateTime.Now, Conversions.ToDate(oOldElmt.GetAttribute("dExpireDate"))));
                            }
                        }
                        return nTotalPrice;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CartSubscriptionPrice", ex, "", "", gbDebug);
                    }

                    return default;
                }

                public void UpdateSubscriptionPrice(XmlElement oSubscriptionXml, int nSubUserId)
                {
                    try
                    {
                        double nTotalPrice = 0d;
                        int nSubscriptionId = Conversions.ToInteger(oSubscriptionXml.GetAttribute("id"));
                        // first we need to find out if its:
                        // 1) New (No existing ones in the same group or none at all)
                        // 2) Renewal (so it tacks onto the end)
                        // 3) Upgrade/Downgrade (so it takes the remaining credit and starts it from today

                        // First we'll get the xml for this subscription
                        string cSQL = "SELECT tblContent.*, tblCartCatProductRelations.nCatId FROM tblContent Left Outer JOIN tblCartCatProductRelations ON tblContent.nContentKey = tblCartCatProductRelations.nContentId where tblContent.nContentKey = " + nSubscriptionId;
                        var oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Content");
                        DataColumn oDC;
                        foreach (DataColumn currentODC in oDS.Tables["Content"].Columns)
                        {
                            oDC = currentODC;
                            oDC.ColumnMapping = MappingType.Attribute;
                        }
                        oDS.Tables["Content"].Columns["cContentXmlBrief"].ColumnMapping = MappingType.Hidden;
                        oDS.Tables["Content"].Columns["cContentXmlDetail"].ColumnMapping = MappingType.SimpleContent;
                        var oXML = new XmlDocument();

                        oXML.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&gt;", ">"), "&lt;", "<");
                        XmlElement oCurSubElmt = (XmlElement)oXML.DocumentElement.FirstChild;
                        string cGroup = "";
                        if (oCurSubElmt != null)
                        {
                            cGroup = oCurSubElmt.GetAttribute("nCatId");
                        }

                        // Ok, lets get any other  subscriptions in the same  group
                        cSQL = "Select tblSubscription.*, tblAudit.*, tblCartCatProductRelations.nCatId" + " FROM tblSubscription INNER JOIN tblAudit ON tblSubscription.nAuditId = tblAudit.nAuditKey LEFT OUTER JOIN" + " tblCartCatProductRelations ON tblSubscription.nSubContentId = tblCartCatProductRelations.nContentId";
                        if (string.IsNullOrEmpty(cGroup))
                        {
                            cSQL += " WHERE tblSubscription.nDirId = " + nSubUserId + " AND tblSubscription.nSubContentId = " + nSubscriptionId;
                        }
                        else
                        {
                            cSQL += " WHERE (tblSubscription.nDirId = " + nSubUserId + ") AND (tblSubscription.nSubContentId = " + nSubscriptionId + " OR tblCartCatProductRelations.nCatId = " + cGroup + ")";
                        }
                        cSQL += " AND tblSubscription.cRenewalStatus <> 'Cancelled' AND tblAudit.dExpireDate > " + sqlDate(DateTime.Now) + " AND tblAudit.dPublishDate < " + sqlDate(DateTime.Now) + " order by tblAudit.dPublishDate DESC";

                        oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Content");

                        if (oDS.Tables["Content"].Rows.Count == 0)
                        {
                        }

                        // No Other subscriptions or subscriptions in the same group

                        else
                        {
                            // okies, there is some stuff in here.
                            // now there should only be one.
                            // Either the same or one in the same group
                            // latest valid subscription comes first and this is the one we upgrade.
                            var oExXML = new XmlDocument();
                            foreach (DataColumn currentODC1 in oDS.Tables["Content"].Columns)
                            {
                                oDC = currentODC1;
                                oDC.ColumnMapping = MappingType.Attribute;
                            }
                            oDS.Tables["Content"].Columns["cSubXML"].ColumnMapping = MappingType.SimpleContent;
                            oExXML.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&gt;", ">"), "&lt;", "<");
                            XmlElement oNewElmt = (XmlElement)oXML.DocumentElement.FirstChild;
                            XmlElement oOldElmt = (XmlElement)oExXML.DocumentElement.FirstChild;
                            double nCurPrice = myCart.getProductPricesByXml(oNewElmt.InnerXml, "", 1);
                            nTotalPrice = SubscriptionPrice(nCurPrice, oNewElmt.SelectSingleNode("Content/PaymentUnit").InnerText, Conversions.ToInteger(oNewElmt.SelectSingleNode("Content/Duration/Length").InnerText), oNewElmt.SelectSingleNode("Content/Duration/Unit").InnerText, Conversions.ToDate(oOldElmt.GetAttribute("dExpireDate")));
                            if (Conversions.ToBoolean(!Operators.ConditionalCompareObjectEqual(oDS.Tables["Content"].Rows[0]["nSubContentId"], nSubscriptionId, false)))
                            {
                                nTotalPrice = (double)(nTotalPrice - UpgradeCredit(myCart.getProductPricesByXml(oOldElmt.InnerXml, "", 1), oOldElmt.SelectSingleNode("Content/PaymentUnit").InnerText, DateTime.Now, Conversions.ToDate(oOldElmt.GetAttribute("dExpireDate"))));
                                XmlElement PricesNode = (XmlElement)oSubscriptionXml.SelectSingleNode("Prices/Price[@type='sale']");
                                PricesNode.SetAttribute("originalPrice", PricesNode.InnerText);
                                PricesNode.SetAttribute("discountValue", (Conversions.ToDouble(PricesNode.InnerText) - nTotalPrice).ToString());
                                PricesNode.SetAttribute("upgradeFrom", oOldElmt.SelectSingleNode("Content/Name").InnerText);
                                PricesNode.InnerText = nTotalPrice.ToString();
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CartSubscriptionPrice", ex, "", "", gbDebug);
                    }
                }

                public double SubscriptionPrice(double nPrice, string cPriceUnit, int nDuration, string cDurationUnit, DateTime dStart)
                {
                    // Gets the price of the subscription
                    try
                    {
                        DateTime dFinish;
                        switch (cDurationUnit ?? "")
                        {
                            case "Day":
                                {
                                    dFinish = dStart.AddDays(nDuration);
                                    break;
                                }
                            case "Week":
                                {
                                    dFinish = dStart.AddDays(nDuration * 7);
                                    break;
                                }
                            case "Month":
                                {
                                    dFinish = dStart.AddMonths(nDuration);
                                    break;
                                }
                            case "Year":
                                {
                                    dFinish = dStart.AddYears(nDuration);
                                    break;
                                }

                            default:
                                {
                                    dFinish = dStart;
                                    break;
                                }
                        }

                        double nEndPrice = 0d;
                        switch (cPriceUnit ?? "")
                        {
                            case "Day":
                                {
                                    nEndPrice = (int)Math.Round(dFinish.Subtract(dStart).TotalDays) * nPrice;
                                    break;
                                }
                            case "Week":
                                {
                                    nEndPrice = (double)RoundUp(dFinish.Subtract(dStart).TotalDays / 7d, 0, 0) * nPrice;
                                    break;
                                }
                            case "Month":
                                {
                                    // this will be trickier
                                    // need to step through each month
                                    var dCurrent = dStart;
                                    int nMonths = 0;
                                    dFinish = dFinish.AddDays(-1);
                                    while (dCurrent < dFinish)
                                    {
                                        dCurrent = dCurrent.AddMonths(1);
                                        nMonths += 1;
                                    }
                                    nEndPrice = nMonths * nPrice;
                                    break;
                                }
                            case "Year":
                                {
                                    // same as months
                                    var dCurrent = dStart;
                                    int nYears = 0;
                                    while (dCurrent < dFinish)
                                    {
                                        dCurrent = dCurrent.AddYears(1);
                                        nYears += 1;
                                    }
                                    nEndPrice = nYears * nPrice;
                                    break;
                                }

                            default:
                                {
                                    nEndPrice = 0d;
                                    break;
                                }
                        }
                        return nEndPrice;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "SubscriptionPrice", ex, "", "", gbDebug);
                        return 0d;
                    }
                }

                private int UpgradeCredit(double nPrice, string cPriceUnit, DateTime dStart, DateTime dFinish)
                {
                    // gets the remaining credit of the old subscription
                    // so that it may be subtracted from the new one
                    try
                    {
                        double nUnitsCredit = 0d;
                        switch (cPriceUnit ?? "")
                        {
                            case "Day":
                                {
                                    nUnitsCredit = dFinish.Subtract(dStart).TotalDays;
                                    break;
                                }
                            case "Week":
                                {
                                    nUnitsCredit = dFinish.Subtract(dStart).TotalDays / 7d;
                                    break;
                                }
                            case "Month":
                                {
                                    var dCurrent = dStart;
                                    int nMonths = 0;
                                    while (dCurrent < dFinish)
                                    {
                                        dCurrent = dCurrent.AddMonths(1);
                                        nMonths += 1;
                                    }
                                    nUnitsCredit = nMonths;
                                    break;
                                }
                            case "Year":
                                {
                                    // same as months
                                    var dCurrent = dStart;
                                    string divBy = "Month";

                                    switch (divBy ?? "")
                                    {
                                        case "Year":
                                            {
                                                int nYears = 0;
                                                // By Min Full Year
                                                while (dCurrent < dFinish)
                                                {
                                                    dCurrent = dCurrent.AddYears(1);
                                                    nYears += 1;
                                                }
                                                nUnitsCredit = nYears;
                                                break;
                                            }
                                        case "Month":
                                            {
                                                int nMonths = 0;
                                                // By Min Full Month
                                                while (dCurrent < dFinish)
                                                {
                                                    dCurrent = dCurrent.AddMonths(1);
                                                    nMonths += 1;
                                                }
                                                nUnitsCredit = nMonths / 12d;
                                                break;
                                            }
                                        case "Day":
                                            {
                                                int nDays = 0;
                                                // By Min Full Month
                                                while (dCurrent < dFinish)
                                                {
                                                    dCurrent = dCurrent.AddDays(1d);
                                                    nDays += 1;
                                                }
                                                nUnitsCredit = nDays / 365d;
                                                break;
                                            }
                                    }

                                    break;
                                }

                        }
                        nPrice = Conversions.ToDouble(Strings.FormatNumber(nPrice * nUnitsCredit, 2));
                        return (int)Math.Round(nPrice);
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "UpgradeCredit", ex, "", "", gbDebug);
                    }

                    return default;
                }

                public DateTime SubscriptionEndDate(DateTime dStart, XmlElement oSubDetailElmt)
                {
                    try
                    {
                        int cDuration = 0;
                        string cDurationUnit;
                        if (Information.IsNumeric(oSubDetailElmt.SelectSingleNode("Duration/Length").InnerText))
                        {
                            cDuration = Conversions.ToInteger(oSubDetailElmt.SelectSingleNode("Duration/Length").InnerText);
                        }
                        if (cDuration == 0)
                        {
                            return default;
                        }
                        else
                        {
                            cDurationUnit = oSubDetailElmt.SelectSingleNode("Duration/Unit").InnerText;
                            switch (cDurationUnit ?? "")
                            {
                                case "Day":
                                    {
                                        return dStart.AddDays(cDuration).AddDays(-1);
                                    }

                                case "Week":
                                    {
                                        return dStart.AddDays(cDuration * 7).AddDays(-1);
                                    }
                                case "Month":
                                    {
                                        return dStart.AddMonths(cDuration).AddDays(-1);
                                    }
                                case "Year":
                                    {
                                        return dStart.AddYears(cDuration).AddDays(-1);
                                    }
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "SubscriptionEndDate", ex, "", "", gbDebug);
                    }

                    return default;
                }

                public DateTime GetRenewalDate(int nId)
                {
                    try
                    {
                        string cSQL = "SELECT tblAudit.dExpireDate FROM tblSubscription INNER JOIN tblAudit ON tblSubscription.nAuditId = tblAudit.nAuditKey where nSubKey = " + nId;
                        string sRenew = myWeb.moDbHelper.ExeProcessSqlScalar(cSQL);
                        if (Information.IsDate(sRenew))
                        {
                            DateTime dRenew = Conversions.ToDate(sRenew);
                            if (dRenew < DateTime.Now.Date)
                                dRenew = DateTime.Now.AddDays(-1).Date;
                            return dRenew;
                        }
                        else
                        {

                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "GetRenewalDate", ex, "", "", gbDebug);
                    }

                    return default;
                }

                public virtual void AddUserSubscriptions(int nCartId, int nSubUserId, ref XmlElement oCartXml, int nPaymentMethodId = 0)
                {

                    string cLastSubXml = "";
                    System.Collections.Specialized.NameValueCollection oSubConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/subscriptions");
                    string SubscrptionSchemaTypes = "Subscription";
                    string SubscrptionSchemaTypesTemp = "";
                    try
                    {
                        // check for subscriptions being turned on with no subscription config section set.
                        if (oSubConfig != null)
                        {

                            if (!string.IsNullOrEmpty(oSubConfig["SubscrptionSchemaTypes"]))
                            {
                                SubscrptionSchemaTypes = oSubConfig["SubscrptionSchemaTypes"];
                            }

                            foreach (var type in Strings.Split(SubscrptionSchemaTypes, ","))
                                SubscrptionSchemaTypesTemp += "'" + Strings.Trim(type) + "',";

                            SubscrptionSchemaTypes = SubscrptionSchemaTypesTemp.TrimEnd(',');

                            string cSQL = "SELECT tblContent.nContentKey, tblContent.cContentXmlBrief, tblCartItem.xItemXml" + " FROM tblContent INNER JOIN tblCartItem ON tblContent.nContentKey = tblCartItem.nItemId" + " WHERE (tblCartItem.nCartOrderId = " + nCartId + ") AND (tblContent.cContentSchemaName IN (" + SubscrptionSchemaTypes + "))";

                            var oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Subs");
                            if (oDS.Tables["Subs"].Rows.Count > 0)
                            {
                                foreach (DataRow oDR in oDS.Tables["Subs"].Rows)
                                {
                                    var xItemDoc = new XmlDocument();
                                    if (myWeb.moDbHelper.checkTableColumnExists("tblCartItem", "xItemXml"))
                                    {
                                        xItemDoc.LoadXml(Conversions.ToString(oDR["xItemXml"]));
                                    }
                                    else
                                    {
                                        xItemDoc.LoadXml(Conversions.ToString(oDR["cContentXmlBrief"]));
                                    }

                                    // Add OrderNotes to Subscription XML
                                    cSQL = "Select cClientNotes from tblCartOrder where nCartOrderKey=" + nCartId;
                                    var oNotes = xItemDoc.CreateElement("Notes");
                                    string notes = "" + myWeb.moDbHelper.ExeProcessSqlScalar(cSQL);
                                    oNotes.InnerXml = notes;
                                    xItemDoc.FirstChild.AppendChild(oNotes);

                                    AddUserSubscription(Conversions.ToInteger(oDR["nContentKey"]), nSubUserId, nPaymentMethodId, xItemDoc.DocumentElement, nCartId);

                                    // Hustle in the renewal end so we can show on receipt.
                                    if (oCartXml != null)
                                    {
                                        if (!string.IsNullOrEmpty(xItemDoc.DocumentElement.GetAttribute("id")))
                                        {
                                            long contentId = Conversions.ToLong(xItemDoc.DocumentElement.GetAttribute("id"));
                                            XmlElement ItemXml = (XmlElement)oCartXml.SelectSingleNode("Order/Item[@contentId='" + contentId + "']");
                                            if (ItemXml != null)
                                            {
                                                XmlElement ProductDetailXml = (XmlElement)ItemXml.SelectSingleNode("productDetail");
                                                ProductDetailXml.SetAttribute("renewalEnd", xItemDoc.DocumentElement.GetAttribute("renewalEnd"));
                                            }
                                        }
                                        else
                                        {
                                            XmlElement ItemXml = (XmlElement)oCartXml.SelectSingleNode("Order/Item[0]");
                                            if (ItemXml != null)
                                            {
                                                XmlElement ProductDetailXml = (XmlElement)ItemXml.SelectSingleNode("productDetail");
                                                ProductDetailXml.SetAttribute("renewalEnd", xItemDoc.DocumentElement.GetAttribute("renewalEnd"));
                                            }
                                        }
                                    }

                                    cLastSubXml = xItemDoc.OuterXml; // oDR("cContentXmlBrief")
                                }
                            }





                            // Clear the cache
                            // -- we need to also reload the menu and check if the page we came from is still accessible.
                            myWeb.moDbHelper.clearStructureCacheUser();

                            // Reload the menu
                            var oMenu = myWeb.GetStructureXML(0L, 0L, 0L, "Site", false, false, true, false, false, "MenuItem", "Menu");
                            XmlElement oCurrentMenu = (XmlElement)myWeb.moPageXml.SelectSingleNode("/Page/Menu");
                            oCurrentMenu.ParentNode.ReplaceChild(oCurrentMenu.OwnerDocument.ImportNode(oMenu, true), oCurrentMenu);

                            // Check if the current page is still valid.
                            if (oMenu.SelectSingleNode("//MenuItem[@id=" + myWeb.mnPageId + "]") is null)
                            {

                                int nRootId = Conversions.ToInteger("0" + Conversions.ToString(Interaction.IIf(myWeb.moConfig["AuthenticatedRootPageId"] is null, Interaction.IIf(myWeb.moConfig["RootPageId"] is null, (object)1, myWeb.moConfig["RootPageId"]), myWeb.moConfig["AuthenticatedRootPageId"])));

                                // Load in the last subscription
                                if (string.IsNullOrEmpty(cLastSubXml))
                                {

                                    // No sub set the page to be the root id
                                    myWeb.mnPageId = nRootId;
                                }

                                else
                                {

                                    var oSub = myWeb.moPageXml.CreateElement("Subscription");
                                    try
                                    {
                                        oSub.InnerXml = cLastSubXml;
                                        if (oSub.SelectSingleNode("//AccessPage[.!='']") is null)
                                        {
                                            myWeb.mnPageId = nRootId;
                                        }
                                        else
                                        {
                                            myWeb.mnPageId = Conversions.ToInteger(oSub.SelectSingleNode("//AccessPage[.!='']").InnerText);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        // No xml sub set the page to be the root id
                                        myWeb.mnPageId = nRootId;
                                    }

                                }

                                // Set the page id.
                                myWeb.moPageXml.DocumentElement.SetAttribute("id", myWeb.mnPageId.ToString());
                                if (myWeb.moConfig["UsePageIdsForURLs"] == "on")
                                {
                                    myWeb.moSession["returnPage"] = "?pgid=" + myWeb.mnPageId;
                                }
                                else
                                {
                                    XmlElement oMenuItem = (XmlElement)oMenu.SelectSingleNode("//MenuItem[@id=" + myWeb.mnPageId + "]");
                                    string cMenuUrl;
                                    if (oMenuItem is null)
                                    {
                                        cMenuUrl = "/";
                                    }
                                    else
                                    {
                                        cMenuUrl = oMenuItem.GetAttribute("url");
                                        if (myCart.mcSiteURL.EndsWith("/"))
                                            cMenuUrl = cMenuUrl.TrimStart(@"/\".ToCharArray());
                                    }
                                    myWeb.moSession["returnPage"] = cMenuUrl;

                                }

                            }
                        }
                    }


                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "AddUserSubscriptions", ex, "", "", gbDebug);
                    }

                }

                public void AddUserSubscription(int nSubscriptionID, int nSubUserId, int nPaymentMethodId = 0, XmlElement cartItemXml = null, long nCartId = default)
                {
                    try
                    {
                        // same sort of process as getting prices.
                        // this time we need to replace if regrading
                        // add a new one for renewals to start the next day (for the scheduler to pickup)
                        // add new ones straight in.

                        // First we'll get the xml for this subscription
                        XmlElement oCurSubElmt;
                        string cSQL;
                        DataSet oDS;
                        DataColumn oDC;
                        if (cartItemXml != null)
                        {
                            oCurSubElmt = cartItemXml;
                        }
                        else
                        {
                            cSQL = "SELECT tblContent.*, tblCartCatProductRelations.nCatId FROM tblContent LEFT OUTER JOIN tblCartCatProductRelations ON tblContent.nContentKey = tblCartCatProductRelations.nContentId where tblContent.nContentKey = " + nSubscriptionID;
                            oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Content");

                            foreach (DataColumn currentODC in oDS.Tables["Content"].Columns)
                            {
                                oDC = currentODC;
                                oDC.ColumnMapping = MappingType.Attribute;
                            }
                            oDS.Tables["Content"].Columns["cContentXmlBrief"].ColumnMapping = MappingType.Hidden;
                            oDS.Tables["Content"].Columns["cContentXmlDetail"].ColumnMapping = MappingType.SimpleContent;
                            var oXML = new XmlDocument();

                            oXML.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&gt;", ">"), "&lt;", "<");
                            oCurSubElmt = (XmlElement)oXML.DocumentElement.FirstChild;
                        }

                        string cGroup = "";
                        if (oCurSubElmt != null)
                        {
                            cGroup = oCurSubElmt.GetAttribute("nCatId");
                        }
                        // Ok, lets get any other  subscriptions in the same  group

                        if (string.IsNullOrEmpty(cGroup))
                        {
                            cSQL = "SELECT tblSubscription.*, tblContent.* FROM tblContent INNER JOIN tblSubscription ON tblContent.nContentKey = tblSubscription.nSubContentId";
                            cSQL += " WHERE tblSubscription.nDirId = " + nSubUserId + " AND tblSubscription.nSubContentId = " + nSubscriptionID;
                        }
                        else
                        {
                            cSQL = "SELECT tblSubscription.*, tblCartCatProductRelations.nCatId, tblContent.*" + " FROM tblContent INNER JOIN" + " tblCartCatProductRelations ON tblContent.nContentKey = tblCartCatProductRelations.nContentId RIGHT OUTER JOIN" + " tblSubscription ON tblCartCatProductRelations.nContentId = tblSubscription.nSubContentId RIGHT OUTER JOIN" + " tblAudit a ON tblSubscription.nAuditId = a.nAuditKey";
                            cSQL += " WHERE (tblSubscription.nDirId = " + nSubUserId + ") AND (tblSubscription.nSubContentId = " + nSubscriptionID + " OR tblCartCatProductRelations.nCatId = " + cGroup + ") order by a.dExpireDate DESC";
                        }

                        var SubStartDate = DateTime.Now;
                        if (!string.IsNullOrEmpty(oSubConfig["SubscriptionStartDateXpath"]))
                        {
                            string dateString = oCurSubElmt.SelectSingleNode(oSubConfig["SubscriptionStartDateXpath"]).InnerText;
                            if (Information.IsDate(dateString))
                            {
                                SubStartDate = Conversions.ToDate(oCurSubElmt.SelectSingleNode(oSubConfig["SubscriptionStartDateXpath"]).InnerText);
                            }
                        }

                        var SubEndDate = SubscriptionEndDate(SubStartDate, oCurSubElmt);
                        if (cartItemXml != null)
                        {
                            cartItemXml.SetAttribute("renewalEnd", XmlDate(SubEndDate));
                        }

                        oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Content");

                        if (Strings.LCase(oSubConfig["AllowRenewals"]) == "off")
                        {

                            AddSubscription(nSubscriptionID, oCurSubElmt, SubStartDate, SubEndDate, nSubUserId, nPaymentMethodId, nCartId);
                        }


                        else if (oDS.Tables["Content"].Rows.Count == 0)
                        {
                            // No Other subscriptions or subscriptions in the same group
                            // so we just add it
                            AddSubscription(nSubscriptionID, oCurSubElmt, SubStartDate, SubEndDate, nSubUserId, nPaymentMethodId, nCartId);
                        }
                        else
                        {
                            // okies, there is some stuff in here.
                            // now there should only be one.
                            // Either the same or one in the same group
                            var oExXML = new XmlDocument();
                            foreach (DataColumn currentODC1 in oDS.Tables["Content"].Columns)
                            {
                                oDC = currentODC1;
                                oDC.ColumnMapping = MappingType.Attribute;
                            }
                            oDS.Tables["Content"].Columns["cContentXmlBrief"].ColumnMapping = MappingType.Hidden;
                            oDS.Tables["Content"].Columns["cSubXml"].ColumnMapping = MappingType.Hidden;
                            oDS.Tables["Content"].Columns["cContentXmlDetail"].ColumnMapping = MappingType.SimpleContent;
                            oExXML.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&gt;", ">"), "&lt;", "<");
                            XmlElement oNewElmt = (XmlElement)oExXML.DocumentElement.FirstChild;
                            if (oDS.Tables["Content"].Rows[0]["nContentKey"] is DBNull)
                            {
                            }
                            // its one with no group

                            // its a regrade
                            // cancel earlier subscription
                            else if (Conversions.ToBoolean(!Operators.ConditionalCompareObjectEqual(oDS.Tables["Content"].Rows[0]["nContentKey"], nSubscriptionID, false)))
                            {
                                foreach (DataRow oRows in oDS.Tables["Content"].Rows)
                                    CancelSubscription(Conversions.ToInteger(oRows["nSubKey"]));
                                AddSubscription(nSubscriptionID, oCurSubElmt, SubStartDate, SubscriptionEndDate(DateTime.Now, oCurSubElmt), nSubUserId, nPaymentMethodId);
                            }
                            else
                            {
                                // its a renewal of an existing policy
                                // add
                                var dRenStart = GetRenewalDate(Conversions.ToInteger(oNewElmt.GetAttribute("nSubKey")));
                                dRenStart = dRenStart.AddDays(1d);
                                AddSubscription(nSubscriptionID, oCurSubElmt, dRenStart, SubscriptionEndDate(dRenStart, oCurSubElmt), nSubUserId, nPaymentMethodId);
                            }

                        }
                    }


                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "AddUserSubscription", ex, "", "", gbDebug);
                    }

                }

                #endregion

                #region DBHELPER

                public void AddSubscriptionToUserXML(ref XmlElement oElmt, int nSubUserId)
                {
                    try
                    {
                        string cSQL = "SELECT s.nSubKey,s.cRenewalStatus, s.nSubContentId, s.dStartDate, s.nPeriod, s.cPeriodUnit, s.nValueNet, s.nPaymentMethodId, pm.cPayMthdProviderName, s.bPaymentMethodActive, a.nStatus, a.dPublishDate, a.dExpireDate, s.cSubXML" + " FROM tblSubscription s INNER JOIN" + " tblAudit a ON s.nAuditId = a.nAuditKey LEFT OUTER JOIN" + " tblCartPaymentMethod pm On s.nPaymentMethodId = pm.nPayMthdKey" + " WHERE s.nDirId = " + nSubUserId;

                        var oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Subscriptions", "Subscription");
                        if (oDS.Tables["Subscriptions"].Rows.Count > 0)
                        {
                            oDS.Tables["Subscriptions"].Columns["nSubKey"].ColumnMapping = MappingType.Attribute;
                            oDS.Tables["Subscriptions"].Columns["nSubContentId"].ColumnMapping = MappingType.Attribute;
                            oDS.Tables["Subscriptions"].Columns["cRenewalStatus"].ColumnMapping = MappingType.Attribute;
                            oDS.Tables["Subscriptions"].Columns["dStartDate"].ColumnMapping = MappingType.Attribute;
                            // oDS.Tables("Subscriptions").Columns("dEndDate").ColumnMapping = MappingType.Attribute
                            oDS.Tables["Subscriptions"].Columns["nPeriod"].ColumnMapping = MappingType.Attribute;
                            oDS.Tables["Subscriptions"].Columns["cPeriodUnit"].ColumnMapping = MappingType.Attribute;
                            oDS.Tables["Subscriptions"].Columns["nValueNet"].ColumnMapping = MappingType.Attribute;
                            oDS.Tables["Subscriptions"].Columns["nPaymentMethodId"].ColumnMapping = MappingType.Attribute;
                            oDS.Tables["Subscriptions"].Columns["cPayMthdProviderName"].ColumnMapping = MappingType.Attribute;
                            oDS.Tables["Subscriptions"].Columns["bPaymentMethodActive"].ColumnMapping = MappingType.Attribute;
                            oDS.Tables["Subscriptions"].Columns["nStatus"].ColumnMapping = MappingType.Attribute;
                            oDS.Tables["Subscriptions"].Columns["dPublishDate"].ColumnMapping = MappingType.Attribute;
                            oDS.Tables["Subscriptions"].Columns["dExpireDate"].ColumnMapping = MappingType.Attribute;
                            oDS.Tables["Subscriptions"].Columns["cSubXML"].ColumnMapping = MappingType.SimpleContent;
                            var oSubElmt = oElmt.OwnerDocument.CreateElement("Subscriptions");
                            var oTmp = new XmlDocument();
                            oTmp.InnerXml = Strings.Replace(Strings.Replace(oDS.GetXml(), "&lt;", "<"), "&gt;", ">");
                            oSubElmt.InnerXml = oTmp.DocumentElement.InnerXml;
                            oElmt.AppendChild(oSubElmt);
                        }
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "AddSubscriptionToUserXML", ex, "", "", gbDebug);
                    }
                }

                public void AddSubscription(int nid, XmlElement oSubDetailElmt, DateTime dStart, DateTime dFinish, int nSubUserId, int nPaymentMethodId = 0, long nCartId = 0L)
                {
                    try
                    {
                        var oXml = new XmlDocument();
                        var oInstance = oXml.CreateElement("instance");
                        XmlNode oElmt = oXml.CreateElement("tblSubscription");
                        addNewTextNode("nDirId", ref oElmt, nSubUserId.ToString());
                        addNewTextNode("nSubContentId", ref oElmt, nid.ToString());
                        var oElmt2 = addNewTextNode("cSubXml", ref oElmt);
                        oElmt2.InnerXml = oSubDetailElmt.OuterXml;
                        addNewTextNode("dStartDate", ref oElmt, XmlDate(dStart));


                        addNewTextNode("cSubName", ref oElmt, oSubDetailElmt.SelectSingleNode("Name").InnerText);
                        addNewTextNode("nPeriod", ref oElmt, oSubDetailElmt.SelectSingleNode("Duration/Length").InnerText);
                        addNewTextNode("cPeriodUnit", ref oElmt, oSubDetailElmt.SelectSingleNode("Duration/Unit").InnerText);
                        addNewTextNode("nValueNet", ref oElmt, Conversions.ToDouble("0" + oSubDetailElmt.SelectSingleNode("SubscriptionPrices/Price[@type='sale']").InnerText).ToString());
                        addNewTextNode("nPaymentMethodId", ref oElmt, nPaymentMethodId.ToString());
                        addNewTextNode("bPaymentMethodActive", ref oElmt, "true");
                        addNewTextNode("nMinimumTerm", ref oElmt, oSubDetailElmt.SelectSingleNode("Duration/MinimumTerm").InnerText);
                        addNewTextNode("nRenewalTerm", ref oElmt, oSubDetailElmt.SelectSingleNode("Duration/RenewalTerm").InnerText);
                        addNewTextNode("cRenewalStatus", ref oElmt, oSubDetailElmt.SelectSingleNode("Type").InnerText);
                        if (myWeb.moDbHelper.checkTableColumnExists("tblSubscription", "nOrderId"))
                        {
                            addNewTextNode("nOrderId", ref oElmt, nCartId.ToString());
                        }

                        // addNewTextNode("dEndDate", oElmt, dFinish)
                        addNewTextNode("nAuditId", ref oElmt);
                        // addNewTextNode("nAuditId", oElmt, myWeb.moDbHelper.getAuditId(1, myWeb.mnUserId, "Subscription", dStart, dFinish, Now))
                        addNewTextNode("nAuditKey", ref oElmt);
                        addNewTextNode("dPublishDate", ref oElmt, Conversions.ToString(dStart));
                        addNewTextNode("dExpireDate", ref oElmt, Conversions.ToString(dFinish));
                        addNewTextNode("dInsertDate", ref oElmt);
                        addNewTextNode("nInsertDirId", ref oElmt);
                        addNewTextNode("dUpdateDate", ref oElmt);
                        addNewTextNode("nUpdateDirId", ref oElmt);
                        addNewTextNode("nStatus", ref oElmt, 1.ToString());
                        addNewTextNode("cDescription", ref oElmt);

                        oInstance.AppendChild(oElmt);
                        int nSubId = Conversions.ToInteger(myWeb.moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Subscription, oInstance));
                        if (nSubId > 0)
                        {
                            foreach (XmlNode currentOElmt in oSubDetailElmt.SelectNodes("UserGroups/Group[@id!='']"))
                            {
                                oElmt = currentOElmt;
                                int nGrpID = Conversions.ToInteger(oElmt.Attributes["id"].Value);
                                myWeb.moDbHelper.saveDirectoryRelations((long)nSubUserId, nGrpID.ToString());
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "AddSubscription", ex, "", "", gbDebug);
                    }
                }

                public void CancelSubscription(int nId, string cReason = "", bool bEmailCustomer = true)
                {
                    try
                    {

                        var SubInstance = new XmlDocument();
                        SubInstance.LoadXml("<instance>" + myWeb.moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Subscription, (long)nId) + "</instance>");

                        XmlElement editElmt = (XmlElement)SubInstance.DocumentElement.FirstChild;

                        editElmt.SelectSingleNode("nStatus").InnerText = ((int)Cms.dbHelper.Status.Rejected).ToString();
                        editElmt.SelectSingleNode("cRenewalStatus").InnerText = "Cancelled";

                        editElmt.SelectSingleNode("dUpdateDate").InnerText = XmlDate(DateTime.Now);
                        editElmt.SelectSingleNode("nUpdateDirId").InnerText = myWeb.mnUserId.ToString();
                        editElmt.SelectSingleNode("cDescription").InnerText = cReason;

                        // Cancel the payment method
                        long PaymentMethodId = Conversions.ToLong("0" + editElmt.SelectSingleNode("nPaymentMethodId").InnerText);

                        // Cancel the payment method
                        if (PaymentMethodId > 0L)
                        {
                            CancelPaymentMethod(Conversions.ToInteger(editElmt.SelectSingleNode("nPaymentMethodId").InnerText));
                        }

                        // We only remove user from groups (this needs to happen by schduler to remove once expired)

                        myWeb.moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Subscription, SubInstance.DocumentElement);

                        ExpireSubscriptionGroups(nId);

                        // Email the site owner to inform of cancelation !!!
                        if (!string.IsNullOrEmpty(oSubConfig["CancellationXSL"]) & bEmailCustomer)
                        {
                            var oMessager = new Protean.Messaging(ref myWeb.msException);
                            XmlElement argoParentElmt = null;
                            var SubXml = GetSubscriptionDetail(ref argoParentElmt, nId);
                            string CustomerEmail = SubXml.FirstChild.SelectSingleNode("User/Email").InnerText;
                            // Inform the client
                            Cms.dbHelper argodbHelper = null;
                            string cRetMessage = Conversions.ToString(oMessager.emailer((XmlElement)SubXml.FirstChild, oSubConfig["CancellationXSL"], oSubConfig["SubscriptionEmailName"], oSubConfig["SubscriptionEmail"], CustomerEmail, "Cancel Subscription", odbHelper: ref argodbHelper));
                            // Inform the site owner
                            Cms.dbHelper argodbHelper1 = null;
                            string cRetMessage2 = Conversions.ToString(oMessager.emailer((XmlElement)SubXml.FirstChild, oSubConfig["CancellationXSL"], oSubConfig["SubscriptionEmailName"], oSubConfig["SubscriptionEmail"], oSubConfig["SubscriptionEmail"], "Cancel Subscription", odbHelper: ref argodbHelper1));
                        }
                    }


                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CancelSubscription", ex, "", "", gbDebug);
                    }
                }


                public void ResendCancelation(int nId, string cReason = "")
                {
                    try
                    {

                        // Email the site owner to inform of cancelation !!!
                        if (!string.IsNullOrEmpty(oSubConfig["CancellationXSL"]))
                        {
                            var oMessager = new Protean.Messaging(ref myWeb.msException);
                            XmlElement argoParentElmt = null;
                            var SubXml = GetSubscriptionDetail(ref argoParentElmt, nId);
                            string CustomerEmail = SubXml.FirstChild.SelectSingleNode("User/Email").InnerText;
                            // Inform the client
                            Cms.dbHelper argodbHelper = null;
                            string cRetMessage = Conversions.ToString(oMessager.emailer((XmlElement)SubXml.FirstChild, oSubConfig["CancellationXSL"], oSubConfig["SubscriptionEmailName"], oSubConfig["SubscriptionEmail"], CustomerEmail, "Cancel Subscription", odbHelper: ref argodbHelper));
                            // Inform the site owner
                            Cms.dbHelper argodbHelper1 = null;
                            string cRetMessage2 = Conversions.ToString(oMessager.emailer((XmlElement)SubXml.FirstChild, oSubConfig["CancellationXSL"], oSubConfig["SubscriptionEmailName"], oSubConfig["SubscriptionEmail"], oSubConfig["SubscriptionEmail"], "Cancel Subscription", odbHelper: ref argodbHelper1));
                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "ResendCancelation", ex, "", "", gbDebug);
                    }
                }

                public string ExpireSubscription(int nId, string cReason = "")
                {
                    try
                    {

                        var SubInstance = new XmlDocument();
                        SubInstance.LoadXml("<instance>" + myWeb.moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Subscription, (long)nId) + "</instance>");

                        XmlElement editElmt = (XmlElement)SubInstance.DocumentElement.FirstChild;

                        editElmt.SelectSingleNode("nStatus").InnerText = ((int)Cms.dbHelper.Status.Rejected).ToString();
                        editElmt.SelectSingleNode("cRenewalStatus").InnerText = "Fixed";

                        editElmt.SelectSingleNode("dUpdateDate").InnerText = XmlDate(DateTime.Now);
                        editElmt.SelectSingleNode("nUpdateDirId").InnerText = myWeb.mnUserId.ToString();
                        editElmt.SelectSingleNode("cDescription").InnerText = cReason;

                        long PaymentMethodId = Conversions.ToLong("0" + editElmt.SelectSingleNode("nPaymentMethodId").InnerText);

                        // Cancel the payment method
                        if (PaymentMethodId > 0L)
                        {
                            CancelPaymentMethod(Conversions.ToInteger(editElmt.SelectSingleNode("nPaymentMethodId").InnerText));
                        }

                        myWeb.moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Subscription, SubInstance.DocumentElement);

                        // Remove the user from any user groups
                        if (Conversions.ToDate(editElmt.SelectSingleNode("dExpireDate").InnerText) < Conversions.ToDate(XmlDate(DateTime.Now)))
                        {
                            ExpireSubscriptionGroups(nId);
                        }

                        return "Subscription Expired" + cReason;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "ExpireSubscription", ex, "", "", gbDebug);
                        return "Expiry Failed";
                    }
                }

                public void CancelPaymentMethod(int nPaymentMethodId)
                {

                    try
                    {
                        var PayInstance = new XmlDocument();
                        PayInstance.LoadXml(myWeb.moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartPaymentMethod, (long)nPaymentMethodId));
                        string PaymentMethod = PayInstance.DocumentElement.SelectSingleNode("cPayMthdProviderName").InnerText;
                        if (!string.IsNullOrEmpty(PaymentMethod))
                        {
                            //var oPayProv = new Providers.Payment.BaseProvider(ref myWeb, PaymentMethod);
                            Protean.Providers.Payment.ReturnProvider oPayProv = new Protean.Providers.Payment.ReturnProvider();
                            IPaymentProvider oPaymentProv = oPayProv.Get(ref myWeb, PaymentMethod);
                            string paymentStatus;
                            try
                            {
                                string providerRef = PayInstance.DocumentElement.SelectSingleNode("cPayMthdProviderRef").InnerText;
                                paymentStatus = Conversions.ToString(oPaymentProv.Activities.CancelPayments(ref myWeb,ref providerRef));
                            }
                            catch (Exception)
                            {
                                // no payment method to cancel. to we email site owner.
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CancelSubscription", ex, "", "", gbDebug);
                    }
                }

                #endregion

                #region Scheduler

                public XmlElement SubcriptionReminders()
                {
                    // send reminders to out for closing subscriptions 
                    System.Collections.Specialized.NameValueCollection oSubConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/subscriptions");
                    XmlElement moReminderCfg = (XmlElement)WebConfigurationManager.GetWebApplicationSection("protean/subscriptionReminders");
                    var moResponse = myWeb.moPageXml.CreateElement("Response");
                    try
                    {
                        // WRITE CODE TO CHECK WHEN LAST RUN
                        string sSQL = "select TOP 1 cActivityDetail from tblActivityLog where nActivityType = 300 and  dDateTime > " + sqlDateTime(DateAndTime.DateAdd(DateInterval.Day, -1, DateTime.Now)) + " order by dDateTime DESC";
                        string FeedCheck = myWeb.moDbHelper.ExeProcessSqlScalar(sSQL);
                        if (!string.IsNullOrEmpty(FeedCheck))
                        {
                            myWeb.moDbHelper.logActivity(Cms.dbHelper.ActivityType.SubscriptionProcessAttempt, (long)myWeb.mnUserId, 0L, 0L, "Subscription Process Already Run:" + FeedCheck);
                            moResponse.InnerXml = "<Error>Subscription Process allready run</Error>";
                        }
                        else
                        {
                            // Only run daily

                            // FIRST LETS CHECK FOR EXPIRING ACTIONS
                            // CheckExpiringSubscriptions()

                            long LogId = myWeb.moDbHelper.logActivity(Cms.dbHelper.ActivityType.SubscriptionProcess, (long)myWeb.mnUserId, 0L, 0L, "Subscription Process Started");
                            var oMessager = new Protean.Messaging(ref myWeb.msException);

                            string cSQL = "";

                            if (moReminderCfg is null)
                            {
                                moResponse.InnerXml = "<Error>No Subscription Reminder Config Found</Error>";
                            }
                            else
                            {
                                foreach (XmlElement reminderNode in moReminderCfg.SelectNodes("reminder"))
                                {
                                    cSQL = "SELECT tblSubscription.*,DATEDIFF(dd,tblAudit.dExpireDate,getdate()) as renewaldays, tblDirectory.cDirXml, tblCartPaymentMethod.*, tblAudit.*" + " FROM tblSubscription  INNER JOIN tblAudit ON tblSubscription.nAuditId = tblAudit.nAuditKey " + " INNER JOIN tblDirectory On tblSubscription.nDirId = tblDirectory.nDirKey" + " INNER Join tblCartPaymentMethod ON tblSubscription.nPaymentMethodId = tblCartPaymentMethod.nPayMthdKey ";

                                    double count = Conversions.ToDouble(reminderNode.GetAttribute("count"));
                                    string startDate = Tools.Database.SqlDate(DateTime.Now.AddDays(count * -1));
                                    string endDate = Tools.Database.SqlDate(DateTime.Now.AddDays(count * -1 + 1d));
                                    string action = reminderNode.GetAttribute("action");

                                    switch (action ?? "")
                                    {
                                        case "renewal":
                                            {
                                                cSQL += "where (tblAudit.dExpireDate >= " + startDate + " AND tblAudit.dExpireDate < " + endDate + " AND cRenewalStatus LIKE 'Rolling')";
                                                break;
                                            }
                                        case "expired":
                                            {
                                                cSQL += "where (tblAudit.dExpireDate >= " + startDate + " AND tblAudit.dExpireDate < " + endDate + " AND cRenewalStatus LIKE 'Expired')";
                                                break;
                                            }
                                        case "expire":
                                            {
                                                cSQL += "where (tblAudit.dExpireDate >= " + startDate + " AND tblAudit.dExpireDate < " + endDate + " AND cRenewalStatus LIKE 'Fixed')";
                                                break;
                                            }
                                        case "invalidPayment":
                                            {
                                                cSQL += "where (tblAudit.dExpireDate >= " + startDate + " AND tblAudit.dExpireDate < " + endDate + " AND cRenewalStatus LIKE 'Rolling' AND bPaymentMethodActive=0)";
                                                break;
                                            }
                                    }

                                    // thats the sql sorted
                                    // now to make it xml
                                    var oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Subscriptions");
                                    long ProcessedCount = 0L;
                                    if (oDS.Tables["Subscriptions"].Rows.Count > 0)
                                    {
                                        foreach (DataRow oDr in oDS.Tables["Subscriptions"].Rows)
                                        {
                                            ProcessedCount = ProcessedCount + 1L;
                                            string actionResult = "";
                                            XmlElement argoParentElmt = null;
                                            var SubXml = GetSubscriptionDetail(ref argoParentElmt, Conversions.ToInteger(oDr["nSubKey"]));
                                            SubXml.SetAttribute("messageType", reminderNode.GetAttribute("name"));
                                            SubXml.SetAttribute("action", action);
                                            string UserEmail = SubXml.SelectSingleNode("Subscription/User/Email").InnerText;

                                            if (action == "renewal" & count == 0d)
                                            {
                                                actionResult = RenewSubscription(SubXml, Conversions.ToBoolean(1));
                                                if (actionResult == "Failed")
                                                {
                                                    SubXml.SetAttribute("actionResult", actionResult);
                                                    Cms.dbHelper argodbHelper = null;
                                                    string cRetMessage = Conversions.ToString(oMessager.emailer(SubXml, oSubConfig["ReminderXSL"], oSubConfig["SubscriptionEmailName"], oSubConfig["SubscriptionEmail"], UserEmail, reminderNode.GetAttribute("subject"), odbHelper: ref argodbHelper));
                                                }
                                            }
                                            else if (action == "expire" & count == 0d)
                                            {
                                                actionResult = ExpireSubscription(Conversions.ToInteger(oDr["nSubKey"]), "Scheduled Expiration");
                                            }
                                            else
                                            {
                                                Cms.dbHelper argodbHelper1 = null;
                                                string cRetMessage = Conversions.ToString(oMessager.emailer(SubXml, oSubConfig["ReminderXSL"], oSubConfig["SubscriptionEmailName"], oSubConfig["SubscriptionEmail"], UserEmail, reminderNode.GetAttribute("subject"), odbHelper: ref argodbHelper1));
                                                actionResult = cRetMessage;
                                            }
                                            SubXml.SetAttribute("actionResult", actionResult);
                                            moResponse.AppendChild(SubXml);
                                        }
                                    }
                                    reminderNode.SetAttribute("updated", ProcessedCount.ToString());
                                }
                            }

                        }

                        return moResponse;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "SubcriptionReminders", ex, "", "", gbDebug);
                        moReminderCfg.SetAttribute("exception", ex.Message);
                        moReminderCfg.SetAttribute("stackTrace", ex.StackTrace);
                        return moReminderCfg;
                    }
                }

                public string RenewSubscription(long SubKey, bool bEmailClient, bool bSkipPayment = false)
                {

                    // Get the subscription XML
                    XmlElement argoParentElmt = null;
                    var SubXml = GetSubscriptionDetail(ref argoParentElmt, (int)SubKey);
                    return RenewSubscription((XmlElement)SubXml.FirstChild, bEmailClient, bSkipPayment);

                }

                public string RenewSubscription(XmlElement SubXml, bool bEmailClient, bool bSkipPayment = false)
                {
                    string cProcessInfo;
                    try
                    {
                        var renewInterval = DateInterval.Day;
                        switch (SubXml.GetAttribute("periodUnit") ?? "")
                        {
                            case "Week":
                                {
                                    renewInterval = DateInterval.WeekOfYear;
                                    break;
                                }
                            case "Year":
                                {
                                    renewInterval = DateInterval.Year;
                                    break;
                                }
                        }
                        long SubId = Conversions.ToLong("0" + SubXml.GetAttribute("id"));

                        var dNewStart = DateAndTime.DateAdd(DateInterval.Day, 1d, Conversions.ToDate(SubXml.GetAttribute("expireDate")));
                        var dNewEnd = DateAndTime.DateAdd(renewInterval, Conversions.ToInteger(SubXml.GetAttribute("period")), Conversions.ToDate(SubXml.GetAttribute("expireDate")));

                        double Amount = Conversions.ToDouble(SubXml.GetAttribute("value"));
                        long OrderId = Conversions.ToLong(0 + SubXml.GetAttribute("orderId"));
                        long SubContentId = Conversions.ToLong(SubXml.GetAttribute("contentId"));
                        long UserId = Conversions.ToLong(SubXml.GetAttribute("userId"));
                        string SubName = SubXml.GetAttribute("name") + " Renewal";
                        long nPaymentMethodId = Conversions.ToLong(SubXml.GetAttribute("providerId"));

                        // Create the invoice
                        // Add quote to cart
                        myWeb.InitialiseCart();
                        myWeb.moCart.CreateCartElement(myWeb.moPageXml);
                        myWeb.moCart.mnEwUserId = Convert.ToInt32(UserId);
                        myWeb.moCart.CreateNewCart(ref myWeb.moCart.moCartXml);
                        myWeb.moCart.SetPaymentMethod(nPaymentMethodId);
                        if (SubXml.SelectSingleNode("Content/Notes") != null)
                        {
                            myWeb.moCart.SetClientNotes(SubXml.SelectSingleNode("Content/Notes").InnerXml);
                        }
                        XmlElement oSubContent = (XmlElement)SubXml.SelectSingleNode("Content");
                        oSubContent.SetAttribute("renewal", "true");
                        oSubContent.SetAttribute("renewalStart", XmlDate(dNewStart));
                        oSubContent.SetAttribute("renewalEnd", XmlDate(dNewEnd));
                        myWeb.moCart.mnProcessId = 1; // add the process id because it needs to be less than 5
                        myWeb.moCart.AddItem(SubContentId, 1, default, SubName, Amount, oSubContent.OuterXml);

                        long billingId = Conversions.ToLong(myWeb.moDbHelper.GetDataValue("select nContactKey from tblCartContact where cContactType = 'Billing Address' and nContactCartId = 0 and nContactDirId = " + UserId));
                        long deliveryId = billingId;
                        myWeb.moCart.useSavedAddressesOnCart(billingId, deliveryId);

                        // Collect the payment
                        string CurrencyCode = "GBP";
                        string PaymentDescription = "Renewal of " + oSubContent.GetAttribute("name") + "ref:" + SubId;
                        string paymentStatus = "Success"; // Allow policy to renew if no payment method

                        var PayInstance = new XmlDocument();
                        PayInstance.LoadXml(myWeb.moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartPaymentMethod, nPaymentMethodId));

                        if (!bSkipPayment)
                        {
                            string PaymentMethod = PayInstance.DocumentElement.SelectSingleNode("cPayMthdProviderName").InnerText;
                            if (!string.IsNullOrEmpty(PaymentMethod))
                            {
                                paymentStatus = "Failed";
                                //var oPayProv = new Providers.Payment.BaseProvider(ref myWeb, PaymentMethod);
                                Protean.Providers.Payment.ReturnProvider oPayProv = new Protean.Providers.Payment.ReturnProvider();
                                IPaymentProvider oPaymentProv = oPayProv.Get(ref myWeb, PaymentMethod);
                                try
                                {
                                    paymentStatus = Conversions.ToString(oPaymentProv.Activities.CollectPayment(ref myWeb, nPaymentMethodId, Amount, CurrencyCode, PaymentDescription,ref myWeb.moCart));
                                }
                                catch (Exception ex2)
                                {
                                    cProcessInfo = ex2.Message;
                                    // no payment method to cancel. to we email site owner.
                                }
                            }
                        }

                        if (paymentStatus == "Success")
                        {
                            string strSuccess = "Success";
                            myWeb.moCart.mnProcessId = 6;
                            myWeb.moCart.updateCart(ref strSuccess);
                            myWeb.moCart.GetCart();
                            XmlElement xmlmoCartXmlElmt = (XmlElement)myWeb.moCart.moCartXml.FirstChild;
                            myWeb.moCart.addDateAndRef(ref xmlmoCartXmlElmt, dNewStart);

                            // Send the invoice
                            if (bEmailClient)
                            {
                                string RenewalEmailCC = "";
                                if (!string.IsNullOrEmpty(oSubConfig["RenewalEmailCCXpath"]))
                                {
                                    if (myWeb.moCart.moCartXml.SelectSingleNode(oSubConfig["RenewalEmailCCXpath"]) != null)
                                    {
                                        RenewalEmailCC = myWeb.moCart.moCartXml.SelectSingleNode(oSubConfig["RenewalEmailCCXpath"]).Value;
                                    }
                                }

                                myWeb.moCart.emailReceipts(ref myWeb.moCart.moCartXml, RenewalEmailCC);
                            }

                            // myWeb.moCart.updateCart("Success")

                            myWeb.moCart.SaveCartXML((XmlElement)myWeb.moCart.moCartXml.FirstChild);

                            // On Success update subscription
                            var SubInstance = new XmlDocument();
                            SubInstance.LoadXml("<instance>" + myWeb.moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Subscription, Conversions.ToLong(SubXml.GetAttribute("id"))) + "</instance>");
                            XmlElement editElmt = (XmlElement)SubInstance.DocumentElement.FirstChild;

                            editElmt.SelectSingleNode("dExpireDate").InnerText = XmlDate(dNewEnd);

                            editElmt.SelectSingleNode("dUpdateDate").InnerText = XmlDate(DateTime.Now);
                            editElmt.SelectSingleNode("nUpdateDirId").InnerText = myWeb.mnUserId.ToString();
                            editElmt.SelectSingleNode("cDescription").InnerText = "Policy Renewed " + Conversions.ToString(DateTime.Now);

                            // We only remove user from groups (this needs to happen by schduler to remove once expired)

                            myWeb.moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Subscription, SubInstance.DocumentElement);

                            // Create renewal record
                            var renewalInstance = new XmlDocument();
                            renewalInstance.LoadXml("<instance>" + myWeb.moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.SubscriptionRenewal, 0L) + "</instance>");
                            XmlElement editElmt2 = (XmlElement)renewalInstance.DocumentElement.FirstChild;

                            editElmt2.SelectSingleNode("nSubId").InnerText = SubId.ToString();
                            editElmt2.SelectSingleNode("nPaymentMethodId").InnerText = nPaymentMethodId.ToString();
                            editElmt2.SelectSingleNode("nOrderId").InnerText = myWeb.moCart.mnCartId.ToString();
                            editElmt2.SelectSingleNode("nPaymentStatus").InnerText = "1";
                            editElmt2.SelectSingleNode("xNotesXml").InnerXml = SubInstance.DocumentElement.InnerXml;
                            editElmt2.SelectSingleNode("dPublishDate").InnerText = XmlDate(dNewStart);
                            editElmt2.SelectSingleNode("dExpireDate").InnerText = XmlDate(dNewEnd);

                            myWeb.moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.SubscriptionRenewal, renewalInstance.DocumentElement);

                            return "Success";
                        }

                        else
                        {
                            string strFailed = "Failed";
                            myWeb.moCart.mnProcessId = 5;
                            myWeb.moCart.updateCart(ref strFailed);

                            // On Failure
                            // Record the failure
                            var renewalInstance = new XmlDocument();
                            renewalInstance.LoadXml("<instance>" + myWeb.moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.SubscriptionRenewal, 0L) + "</instance>");
                            XmlElement editElmt2 = (XmlElement)renewalInstance.DocumentElement.FirstChild;

                            editElmt2.SelectSingleNode("nSubId").InnerText = SubId.ToString();
                            editElmt2.SelectSingleNode("nPaymentMethodId").InnerText = nPaymentMethodId.ToString();
                            editElmt2.SelectSingleNode("nPaymentStatus").InnerText = "0";
                            editElmt2.SelectSingleNode("xNotesXml").InnerText = "<error>" + paymentStatus + "</error>";

                            myWeb.moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.SubscriptionRenewal, renewalInstance.DocumentElement);

                            return paymentStatus;

                            // ExpireSubscription(SubKey)

                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "SubcriptionReminders", ex, "", "", gbDebug);
                        return null;
                    }


                }

                public string RefreshSubscriptionOrder(XmlElement SubXml, bool bEmailClient, long nCartId)
                {
                    // TS written to fix subscription orders that got created without sending emails
                    string cProcessInfo = "RefreshSubscriptionOrder";
                    try
                    {

                        long SubId = Conversions.ToLong("0" + SubXml.GetAttribute("id"));


                        var renewInterval = DateInterval.Day;
                        switch (SubXml.GetAttribute("periodUnit") ?? "")
                        {
                            case "Week":
                                {
                                    renewInterval = DateInterval.WeekOfYear;
                                    break;
                                }
                            case "Year":
                                {
                                    renewInterval = DateInterval.Year;
                                    break;
                                }
                        }

                        var dNewStart = DateAndTime.DateAdd(renewInterval, Conversions.ToInteger(SubXml.GetAttribute("period")) * -1, Conversions.ToDate(SubXml.GetAttribute("expireDate")));
                        DateTime dNewEnd = Conversions.ToDate(SubXml.GetAttribute("expireDate"));

                        double Amount = Conversions.ToDouble(SubXml.GetAttribute("value"));
                        long OrderId = Conversions.ToLong(0 + SubXml.GetAttribute("orderId"));
                        long SubContentId = Conversions.ToLong(SubXml.GetAttribute("contentId"));
                        long UserId = Conversions.ToLong(SubXml.GetAttribute("userId"));
                        string SubName = SubXml.GetAttribute("name") + " Renewal";
                        long nPaymentMethodId = Conversions.ToLong(SubXml.GetAttribute("providerId"));

                        // Create the invoice
                        // Add quote to cart
                        myWeb.InitialiseCart();
                        myWeb.moCart.mnCartId = Convert.ToInt32(nCartId);
                        myWeb.moCart.mnEwUserId = Convert.ToInt32(UserId);
                        myWeb.moCart.CreateCartElement(myWeb.moPageXml);
                        XmlElement xmlCartFirstchild = (XmlElement)myWeb.moCart.moCartXml.FirstChild;
                        myWeb.moCart.GetCart(ref xmlCartFirstchild);

                        if (SubXml.SelectSingleNode("Content/Notes") != null)
                        {
                            myWeb.moCart.SetClientNotes(SubXml.SelectSingleNode("Content/Notes").InnerXml);
                        }
                        XmlElement oSubContent = (XmlElement)SubXml.SelectSingleNode("Content");
                        oSubContent.SetAttribute("renewal", "true");
                        oSubContent.SetAttribute("renewalStart", XmlDate(dNewStart));
                        oSubContent.SetAttribute("renewalEnd", XmlDate(dNewEnd));
                        myWeb.moCart.mnProcessId = 1; // add the process id because it needs to be less than 5
                        myWeb.moCart.AddItem(SubContentId, 1, default, SubName, Amount, oSubContent.OuterXml);

                        long billingId = Conversions.ToLong(myWeb.moDbHelper.GetDataValue("select nContactKey from tblCartContact where cContactType = 'Billing Address' and nContactCartId = 0 and nContactDirId = " + UserId));
                        long deliveryId = billingId;
                        myWeb.moCart.useSavedAddressesOnCart(billingId, deliveryId);

                        myWeb.moCart.mnProcessId = 6;
                        string strSuccuess = "Success";
                        myWeb.moCart.updateCart(ref strSuccuess);
                        myWeb.moCart.GetCart();
                        XmlElement xmlmoCartElmt = (XmlElement)myWeb.moCart.moCartXml.FirstChild;
                        myWeb.moCart.addDateAndRef(ref xmlmoCartElmt, dNewStart);

                        // Send the invoice
                        if (bEmailClient)
                        {
                            myWeb.moCart.emailReceipts(ref myWeb.moCart.moCartXml);
                        }

                        myWeb.moCart.SaveCartXML((XmlElement)myWeb.moCart.moCartXml.FirstChild);

                        return "Success";
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, cProcessInfo, ex, "", "", gbDebug);
                        return null;
                    }


                }

                public void ExpireSubscriptionGroups(long SubKey)
                {
                    try
                    {
                        XmlElement argoParentElmt = null;
                        var SubXml = GetSubscriptionDetail(ref argoParentElmt, (int)SubKey);
                        ExpireSubscriptionGroups((XmlElement)SubXml.FirstChild);
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "ExpireSubscriptionGroups", ex, "", "", gbDebug);
                    }
                }

                public void ExpireSubscriptionGroups(XmlElement SubXml)
                {

                    try
                    {
                        string cSQL;
                        long nSubId = Conversions.ToLong(SubXml.GetAttribute("id"));
                        long nUserId = Conversions.ToLong(SubXml.GetAttribute("userId"));

                        // Set status
                        // Remove member from groups subscription allows (checking first any other active subscriptions so we don't block the user if they have another active subscription at this time)

                        // gets a list of other active subscriptions
                        cSQL = "SELECT s.cSubXml" + " FROM tblSubscription s  INNER JOIN tblAudit a ON s.nAuditId = a.nAuditKey " + " WHERE a.nStatus = 1 and a.dExpireDate >= " + sqlDate(DateTime.Now) + " and a.dPublishDate <= " + sqlDate(DateTime.Now) + " AND s.nSubKey <> " + nSubId + " AND s.nDirId = " + nUserId;

                        var aPermittedGroups = new long[1];
                        var oDS = myWeb.moDbHelper.GetDataSet(cSQL, "Subscriptions");
                        long ProcessedCount = 0L;
                        if (oDS.Tables["Subscriptions"].Rows.Count > 0)
                        {
                            foreach (DataRow oDr in oDS.Tables["Subscriptions"].Rows)
                            {
                                var thisSubXml = new XmlDocument();
                                thisSubXml.LoadXml(Conversions.ToString(oDr["cSubXml"]));
                                foreach (XmlElement grpElmt in thisSubXml.SelectNodes("Content/UserGroups/Group[@id!='']"))
                                {
                                    Array.Resize(ref aPermittedGroups, aPermittedGroups.Length + 1);
                                    aPermittedGroups[aPermittedGroups.Length - 1] = Conversions.ToLong(grpElmt.GetAttribute("id"));
                                }
                            }

                            // steps through to remove groups from the current sub
                        }
                        foreach (XmlElement grpElmt2 in SubXml.SelectNodes("Content/UserGroups/Group[@id!='']"))
                        {
                            long grpId = Conversions.ToLong(grpElmt2.GetAttribute("id"));
                            // takes user out of 
                            bool bDelete = true;
                            foreach (var compareGrpId in aPermittedGroups)
                            {
                                if (compareGrpId == grpId)
                                {
                                    bDelete = false;
                                }
                            }
                            if (bDelete)
                            {
                                myWeb.moDbHelper.maintainDirectoryRelation(grpId, nUserId, true, (object)DateTime.Now);
                            }
                        }
                    }

                    // Alert customer & merchant elsewhere

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "ExpireSubscriptionGroups", ex, "", "", gbDebug);
                    }

                }


                // Public Function CheckExpiringSubscriptions() As Boolean

                // ' TS ALL THIS NEEDS TO BE REWRITTEN AS THESE TABLES HAVE CHANGED...


                // 'checks upcoming renewals that dont have another set up waiting and sends emails
                // 'based on settings from config and the subscription
                // Try
                // Dim cSQL As String = "SELECT tblDirectorySubscriptions.*, tblCartCatProductRelations.nCatId, tblAudit.*" &
                // " FROM tblDirectorySubscriptions INNER JOIN" &
                // " tblDirectory ON tblDirectorySubscriptions.nUserId = tblDirectory.nDirKey INNER JOIN" &
                // " tblAudit ON tblDirectorySubscriptions.nAuditId = tblAudit.nAuditKey LEFT OUTER JOIN" &
                // " tblCartCatProductRelations ON tblDirectorySubscriptions.nSubscriptionId = tblCartCatProductRelations.nContentId" &
                // " WHERE tblAudit.dExpireDate <= " & Protean.Tools.Database.SqlDate(Now)
                // Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(cSQL, "Content")
                // Dim oDC As DataColumn
                // For Each oDC In oDS.Tables("Content").Columns
                // oDC.ColumnMapping = MappingType.Attribute
                // Next
                // oDS.Tables("Content").Columns("cSubscriptionXML").ColumnMapping = MappingType.SimpleContent


                // Dim oXML As New XmlDocument
                // oXML.InnerXml = Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<")

                // Dim oTmpElmt As XmlElement = oXML.CreateElement("Subscriptions")
                // oTmpElmt.InnerXml = oXML.DocumentElement.InnerXml

                // Dim oElmt As XmlElement 'Initial loop

                // For Each oElmt In oTmpElmt.SelectNodes("Content")
                // Dim nID As Integer = oElmt.GetAttribute("nSubscriptionKey")
                // Dim nGroup As Integer = CInt(0 & oElmt.GetAttribute("nCatId"))
                // Dim nUser As Integer = oElmt.GetAttribute("nUserId")
                // 'check the date so we can see if its an expiry or a the next in line
                // If CDate(oElmt.GetAttribute("dExpireDate")) <= Now Then
                // 'its one coming up for renewal
                // 'so we need to check if it get auto paid and renewed
                // Dim oChkElmt As XmlElement = oElmt.SelectSingleNode("Content/Type")
                // If oChkElmt.InnerXml = "Rolling" Then
                // RepeatSubscriptions(nID, nUser)
                // End If
                // 'then delete it (we don't want to delete just change status)
                // 'myWeb.moDbHelper.DeleteObject(dbHelper.objectTypes.Subscription, nID)

                // 'We simply want to remove the user from the group to let the subscription lapse.
                // End If
                // Next
                // Return True
                // Catch ex As Exception
                // returnException(myWeb.msException, mcModuleName, "CheckExpiringSubscriptions", ex, "", "", gbDebug)
                // Return False
                // End Try
                // End Function

                // Public Function RepeatSubscriptions(ByVal nID As Integer, ByVal nSubUserId As Integer) As Boolean
                // Try
                // 'This is no longer used Moved to RenewSubscription

                // 'will set up new subscriptions and deduct payments

                // 'need to do this after we have added the code to save payment codes
                // Dim cSQL As String = "SELECT TOP 1 tblDirectorySubscriptions.nUserId, tblCartPaymentMethod.nPayMthdKey, tblDirectorySubscriptions.nSubscriptionId, tblAudit.dExpireDate" &
                // " FROM tblDirectorySubscriptions INNER JOIN" &
                // " tblCartPaymentMethod ON tblDirectorySubscriptions.nUserId = tblCartPaymentMethod.nPayMthdUserId INNER JOIN" &
                // " tblAudit ON tblCartPaymentMethod.nAuditId = tblAudit.nAuditKey" &
                // " WHERE (tblAudit.dExpireDate > " & Protean.Tools.Database.SqlDate(Now) & ") AND (tblDirectorySubscriptions.nUserId = " & nSubUserId & ")" &
                // " ORDER BY tblCartPaymentMethod.nPayMthdKey DESC"
                // '" WHERE (tblCartPaymentMethod.dPayMthdExpire > " & sqlDate(Now) & ")" & _
                // Dim oDR As SqlDataReader = myWeb.moDbHelper.getDataReader(cSQL)
                // Dim nUser As Integer = 0
                // Dim nPayment As Integer = 0
                // Dim nContentId As Integer = 0
                // Do While oDR.Read
                // nUser = oDR(0)
                // nPayment = oDR(1)
                // nContentId = oDR(2)
                // Loop
                // If nUser = 0 Or nPayment = 0 Or nContentId = 0 Then Exit Function
                // 'check the subscription still exists.
                // cSQL = "SELECT * " &
                // " FROM tblContent INNER JOIN " &
                // " tblAudit ON tblContent.nAuditId = tblAudit.nAuditKey " &
                // " WHERE nContentKey = " & nContentId &
                // " and nStatus = 1 " &
                // " and (dPublishDate is null or dPublishDate = 0 or dPublishDate <= " & Protean.Tools.Database.SqlDate(Now) & " )" &
                // " and (dExpireDate is null or dExpireDate = 0 or dExpireDate >= " & Protean.Tools.Database.SqlDate(Now) & " )"

                // Dim oDS As DataSet = myWeb.moDbHelper.GetDataSet(cSQL, "Item", "RepeatSubscription")
                // If oDS.Tables("Item").Rows.Count > 0 Then
                // Dim oDC As DataColumn
                // For Each oDC In oDS.Tables("Item").Columns
                // oDC.ColumnMapping = MappingType.Attribute
                // Next
                // oDS.Tables("Item").Columns("cContentXmlBrief").ColumnMapping = MappingType.Hidden
                // oDS.Tables("Item").Columns("cContentXmlDetail").ColumnMapping = MappingType.SimpleContent
                // 'fine
                // 'lets get the amount
                // If myCart Is Nothing Then myCart = New Cart(myWeb)
                // myCart.mnEwUserId = nUser
                // Dim oXML As New XmlDocument
                // Dim oCartElmt As XmlElement = oXML.CreateElement("Order")

                // Dim nPrice As Double = CartSubscriptionPrice(nContentId, nUser)
                // myCart.CreateNewCart(oCartElmt)
                // myCart.AddItem(nContentId, 1, Nothing)
                // myCart.GetCart(oCartElmt)
                // Dim oEwProv As New PaymentProviders(myWeb)

                // '##Setting up PaymentProviders
                // oEwProv.mcCurrency = IIf(myCart.mcCurrencyCode = "", myCart.mcCurrency, myCart.mcCurrencyCode)
                // oEwProv.mcCurrencySymbol = myCart.mcCurrencySymbol

                // oEwProv.mnPaymentAmount = nPrice
                // oEwProv.mcPaymentOrderDescription = "Repeat Ref: An online purchase from: " & myCart.mcSiteURL & " on " & niceDate(Now) & " " & TimeValue(Now) '"Ref:" & mcOrderNoPrefix & CStr(mnCartId) & " An online purchase from: " & mcSiteURL & " on " & niceDate(Now) & " " & TimeValue(Now)
                // oEwProv.mnCartId = myCart.mnCartId
                // oEwProv.mcPaymentOrderDescription = "Repeat Ref:" & myCart.OrderNoPrefix & CStr(myCart.mnCartId) & " An online purchase from: " & myCart.mcSiteURL & " on " & niceDate(Now) & " " & TimeValue(Now)
                // oEwProv.mcOrderRef = myCart.OrderNoPrefix & myCart.mnCartId

                // '#########
                // oXML = New XmlDocument
                // oXML.InnerXml = Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<")
                // Dim oElmt As XmlElement = oXML.DocumentElement.FirstChild
                // oElmt.SetAttribute("price", nPrice)
                // oElmt.SetAttribute("quantity", 1)

                // Dim cResult As String = oEwProv.payRepeat(nPayment, oXML.DocumentElement, "")
                // If cResult = "" Then
                // 'paid
                // 'need to setup the subscription
                // myCart.mnProcessId = Cart.cartProcess.Complete
                // myCart.GetCart(oCartElmt)
                // myCart.addDateAndRef(oElmt)
                // myCart.emailReceipts(oElmt)
                // myCart.updateCart("Cart")
                // AddUserSubscription(nContentId, nUser)

                // Else
                // 'not paid
                // 'cant do allot here really
                // myCart.mnProcessId = Cart.cartProcess.Failed
                // myCart.GetCart(oCartElmt)
                // End If
                // End If
                // Return True
                // Catch ex As Exception
                // returnException(myWeb.msException, mcModuleName, "RepeatSubscriptions", ex, "", "", gbDebug)
                // Return False
                // End Try
                // End Function

                #endregion
                public class Forms : Cms.xForm
                {

                    private const string mcModuleName = "Protean.Cms.Cart.Subscriptions.Forms";
                    // Private Const gbDebug As Boolean = True
                    public Cms.dbHelper moDbHelper;
                    public System.Collections.Specialized.NameValueCollection goConfig; // = WebConfigurationManager.GetWebApplicationSection("protean/web")
                    public System.Collections.Specialized.NameValueCollection moCartConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/cart");
                    public bool mbAdminMode = false;
                    public System.Web.HttpRequest moRequest;

                    // Error Handling hasn't been formally set up for AdminXforms so this is just for method invocation found in xfrmEditContent
                    public new event OnErrorEventHandler OnError;

                    public new delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs err);

                    private void _OnError(object sender, Tools.Errors.ErrorEventArgs err)
                    {
                        stdTools.returnException(ref Protean.xForm.msException, err.ModuleName, err.ProcedureName, err.Exception, "", err.AddtionalInformation, gbDebug);
                    }

                    // Public myWeb As Protean.Cms

                    public Forms(ref Cms aWeb) : base(ref aWeb)
                    {

                        aWeb.PerfMon.Log("AdminXforms", "New");
                        try
                        {
                            this.myWeb = aWeb;
                            goConfig = this.myWeb.moConfig;
                            moDbHelper = this.myWeb.moDbHelper;
                            moRequest = this.myWeb.moRequest;

                            base.cLanguage = this.myWeb.mcPageLanguage;
                        }

                        catch (Exception ex)
                        {
                            stdTools.returnException(ref this.myWeb.msException, mcModuleName, "New", ex, "", "", gbDebug);
                        }

                        OnError += _OnError;
                    }

                    public Forms(ref string sException) : base(ref sException)
                    {
                        OnError += _OnError;
                    }

                    public virtual XmlElement xFrmConfirmSubscription(long SubscriptionId, string FormName = "ConfirmSubscription")
                    {

                        // Called to get XML for the User Logon.

                        XmlElement oFrmElmt;
                        string cProcessInfo = "";
                        //bool bRememberMe = false;
                        try
                        {                          

                            if (mbAdminMode & this.myWeb.mnUserId == 0)
                                goto BuildForm;

                            // maCommonFolders is an array of folder locations used to look locally, then in wellardscommon and finally eoniccommon.
                            if (!this.load("/xforms/subscription/" + FormName + ".xml", this.myWeb.maCommonFolders))
                            {
                                // If this does not load manually then build a form to do it.
                                goto BuildForm;
                            }
                            else
                            {
                                if (SubscriptionId > 0L)
                                {



                                    this.Instance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Subscription, SubscriptionId);

                                    long PaymentMethodId = Conversions.ToLong("0" + this.Instance.SelectSingleNode("tblSubscription/nPaymentMethodId").InnerText);

                                    var contactXml = this.Instance.OwnerDocument.CreateElement("Contact");

                                    if (this.myWeb.GetUserXML((long)this.myWeb.mnUserId).SelectSingleNode("Contacts/Contact[cContactType='Billing Address']") is null)
                                    {
                                        contactXml.InnerXml = "<Contact><nContactKey/><nContactDirId/><nContactCartId/><cContactType>Billing Address</cContactType><cContactName/><cContactCompany/><cContactAddress/><cContactCity/><cContactState/><cContactZip/><cContactCountry/><cContactTel/><cContactFax/><cContactEmail/><cContactXml><OptIn /></cContactXml><nAuditId/><cContactForiegnRef/><cContactForeignRef/></Contact>";
                                    }
                                    else
                                    {
                                        contactXml.InnerXml = this.myWeb.GetUserXML((long)this.myWeb.mnUserId).SelectSingleNode("Contacts/Contact[cContactType='Billing Address']").OuterXml;
                                    }


                                    this.Instance.InnerXml = this.Instance.InnerXml + moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartPaymentMethod, PaymentMethodId) + contactXml.InnerXml;



                                    XmlElement PaymentOptionsSelect = (XmlElement)this.moXformElmt.SelectSingleNode("descendant-or-self::select1[@bind='cPaymentMethod']");

                                    double PaymentAmount = Conversions.ToDouble("0" + this.Instance.SelectSingleNode("tblSubscription/nValueNet").InnerText);
                                    string PaymentMethod = "0" + this.Instance.SelectSingleNode("tblCartPaymentMethod/cPayMthdProviderName").InnerText;

                                    XmlElement xfrmGroup = (XmlElement)PaymentOptionsSelect.SelectSingleNode("ancestor::group[1]");

                                    PaymentProviders oPay;
                                    //bool bDeny = false;
                                    oPay = new PaymentProviders(ref this.myWeb);
                                    oPay.mcCurrency = moCartConfig["Currency"];

                                    if (Strings.LCase(moCartConfig["PaymentTypeButtons"]) == "on")
                                    {
                                        // remove submit button
                                        XmlElement xSubmit = (XmlElement)this.moXformElmt.SelectSingleNode("descendant-or-self::submit");
                                        if (xSubmit != null)
                                            xSubmit.ParentNode.RemoveChild(xSubmit);

                                        // add new submit button
                                        PaymentOptionsSelect.ParentNode.RemoveChild(PaymentOptionsSelect);
                                        // remove binding
                                        XmlElement PaymentBinding = (XmlElement)this.moXformElmt.SelectSingleNode("descendant-or-self::bind[@id='cPaymentMethod']");
                                        PaymentBinding.ParentNode.RemoveChild(PaymentBinding);
                                        Cms.xForm thisforms = (Cms.xForm)this;
                                        oPay.getPaymentMethodButtons(ref thisforms, ref xfrmGroup, PaymentAmount);
                                    }
                                    else
                                    {
                                        string emptyvalue = string.Empty;
                                        Cms.xForm thisforms = (Cms.xForm)this;
                                        oPay.getPaymentMethods(ref thisforms, ref PaymentOptionsSelect, PaymentAmount, ref emptyvalue);
                                    }

                                }
                                goto Check;
                            }



                        BuildForm:
                            ;

                            this.NewFrm("Subscription");
                            this.submission("Subscription", "", "post", "form_check(this)");

                            oFrmElmt = this.addGroup(ref this.moXformElmt, "Subscription", "", "No '/xforms/subscription/" + FormName + "' Form Specified");

                        Check:
                            ;

                            // MyBase.updateInstanceFromRequest()

                            if (this.isSubmitted())
                            {
                                this.validate();
                                if (this.valid)
                                {
                                }
                                // do stuff here...

                                else
                                {
                                    this.valid = false;
                                }
                            }

                            base.addValues();
                            return this.moXformElmt;
                        }


                        catch (Exception ex)
                        {
                            stdTools.returnException(ref this.myWeb.msException, mcModuleName, "xFrmConfirmSubscription", ex, "", cProcessInfo, gbDebug);
                            return null;
                        }
                    }

                }
            }
        }
    }
}
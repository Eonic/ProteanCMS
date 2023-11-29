using System;
using System.Data;
using System.IO;
using System.Xml;
using Microsoft.VisualBasic.CompilerServices;
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

                public class Modules
                {

                    public event OnErrorEventHandler OnError;

                    public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs err);
                    private const string mcModuleName = "Protean.Cms.Membership.Modules";

                    public Modules()
                    {

                        // do nowt

                    }

                    public void UpdateSubscriptionPaymentMethod(ref Cms myWeb, ref XmlElement contentNode)
                    {

                        var pseudoCart = new Cart(myWeb);
                        Models.Order pseudoOrder = null;

                        string ewCmd = myWeb.moRequest["subCmd2"];
                        var ContentDetailElmt = myWeb.moPageXml.CreateElement("ContentDetail");
                        string SelectedPaymentMethod = string.Empty;
                        bool bPaymentMethodUpdated = false;

                        try
                        {
                        processFlow:
                            ;

                            switch (ewCmd ?? "")
                            {
                                case var @case when @case == "":
                                    {
                                        // Confirm Subscription Details Form
                                        var oSubForm = new Subscriptions.Forms(ref myWeb);
                                        var confSubForm = oSubForm.xFrmConfirmSubscription(Conversions.ToLong(myWeb.moRequest["subId"]));

                                        XmlElement oSubmitBtn = (XmlElement)confSubForm.SelectSingleNode("group/submit");
                                        string buttonRef = oSubmitBtn.GetAttribute("ref");

                                        if (bPaymentMethodUpdated)
                                        {
                                            XmlNode argoNode = (XmlNode)oSubForm.moXformElmt;
                                            oSubForm.addNote(ref argoNode, Protean.xForm.noteTypes.Help, "Thank you, your payment method has been updated", true, "term4060");
                                        }

                                        SelectedPaymentMethod = myWeb.moRequest["ewSubmit"];
                                        if (SelectedPaymentMethod == "Update Payment Details")
                                        {
                                            SelectedPaymentMethod = myWeb.moRequest["cPaymentMethod"];
                                        }


                                        if (!string.IsNullOrEmpty(SelectedPaymentMethod)) // equates to is submitted
                                        {
                                            oSubForm.updateInstanceFromRequest();
                                            oSubForm.validate();
                                            DateTime dRenewalDate = Conversions.ToDate(oSubForm.Instance.SelectSingleNode("tblSubscription/dExpireDate").InnerText);
                                            double nFirstPayment = 0d;

                                            if (dRenewalDate < DateTime.Now)
                                            {
                                                nFirstPayment = Conversions.ToDouble(oSubForm.Instance.SelectSingleNode("tblSubscription/cSubXml/Content/SubscriptionPrices/Price[@type='sale']").InnerText);
                                                var oSub = new Subscriptions();
                                                dRenewalDate = oSub.SubscriptionEndDate(dRenewalDate, (XmlElement)oSubForm.Instance.SelectSingleNode("tblSubscription/cSubXml/Content"));
                                                if (dRenewalDate < DateTime.Now)
                                                {
                                                    oSubForm.valid = false;
                                                    XmlNode argoNode1 = (XmlNode)oSubForm.moXformElmt;
                                                    oSubForm.addNote(ref argoNode1, Protean.xForm.noteTypes.Alert, "Your subscription has gone beyond the date it can be renewed you must get a new subscription.", true, "term4060");

                                                }
                                            }

                                            if (oSubForm.valid)
                                            {
                                                ewCmd = "PaymentForm";
                                                pseudoOrder = new Models.Order(ref myWeb.moPageXml);

                                                pseudoOrder.PaymentMethod = SelectedPaymentMethod;
                                                var RandGen = new Random();

                                                pseudoOrder.TransactionRef = "SUB" + Conversions.ToDouble(oSubForm.Instance.SelectSingleNode("tblSubscription/nSubKey").InnerText) + "-" + Conversions.ToDouble("0" + oSubForm.Instance.SelectSingleNode("tblSubscription/nPaymentMethodId").InnerText) + "-" + RandGen.Next(1000, 9999).ToString();

                                                pseudoOrder.firstPayment = nFirstPayment;
                                                pseudoOrder.repeatPayment = Conversions.ToDouble(oSubForm.Instance.SelectSingleNode("tblSubscription/cSubXml/Content/SubscriptionPrices/Price[@type='sale']").InnerText);
                                                pseudoOrder.delayStart = false; // IIf(oSubForm.Instance.SelectSingleNode("tblSubscription/cSubXml/Content/SubscriptionPrices/@delayStart").InnerText = "true", True, False)
                                                pseudoOrder.startDate = dRenewalDate;
                                                pseudoOrder.repeatInterval = oSubForm.Instance.SelectSingleNode("tblSubscription/cSubXml/Content/Duration/Unit").InnerText;
                                                pseudoOrder.repeatLength = Conversions.ToInteger(oSubForm.Instance.SelectSingleNode("tblSubscription/cSubXml/Content/Duration/Length").InnerText);

                                                pseudoOrder.SetAddress(oSubForm.Instance.SelectSingleNode("Contact/cContactName").InnerText, oSubForm.Instance.SelectSingleNode("Contact/cContactEmail").InnerText, oSubForm.Instance.SelectSingleNode("Contact/cContactTel").InnerText, oSubForm.Instance.SelectSingleNode("Contact/cContactTelCountryCode").InnerText, oSubForm.Instance.SelectSingleNode("Contact/cContactCompany").InnerText, oSubForm.Instance.SelectSingleNode("Contact/cContactAddress").InnerText, oSubForm.Instance.SelectSingleNode("Contact/cContactCity").InnerText, oSubForm.Instance.SelectSingleNode("Contact/cContactState").InnerText, oSubForm.Instance.SelectSingleNode("Contact/cContactZip").InnerText, oSubForm.Instance.SelectSingleNode("Contact/cContactCountry").InnerText);

                                                goto processFlow;
                                            }
                                            else
                                            {
                                                ContentDetailElmt.AppendChild(confSubForm);
                                            }
                                        }

                                        else
                                        {
                                            ContentDetailElmt.AppendChild(confSubForm);
                                        }

                                        break;
                                    }


                                case "PaymentForm":
                                    {
                                        // Call Payment Form
                                        // restore the order details from the session
                                        if (pseudoOrder is null)
                                        {
                                            pseudoOrder = (Models.Order)myWeb.moSession["pseudoOrder"];
                                            SelectedPaymentMethod = pseudoOrder.xml.GetAttribute("paymentMethod");
                                        }
                                        else
                                        {
                                            myWeb.moSession["pseudoOrder"] = pseudoOrder;
                                        }


                                        var oPayProv = new Providers.Payment.BaseProvider(ref myWeb, SelectedPaymentMethod);

                                        var ccPaymentXform = new Protean.xForm(ref myWeb.msException);

                                        pseudoCart.mcPagePath = pseudoCart.mcCartURL + myWeb.mcPagePath;

                                        ccPaymentXform = (Protean.xForm)oPayProv.Activities.GetPaymentForm(myWeb, pseudoCart, pseudoOrder.xml, "?subCmd=updateSubPayment&subCmd2=PaymentForm&subId=" + myWeb.moRequest["subId"]);

                                        // 

                                        if (ccPaymentXform.valid)
                                        {
                                            ewCmd = "UpdateSubscription";
                                            // Cancel Old Payment Method
                                            long oldPaymentId = Conversions.ToLong(myWeb.moDbHelper.ExeProcessSqlScalar("select nPaymentMethodId from tblSubscription where nSubKey = " + myWeb.moRequest["subId"]));
                                            var oSubs = new Subscriptions(ref myWeb);
                                            oSubs.CancelPaymentMethod((int)oldPaymentId);
                                            // Set new payment method Id
                                            myWeb.moDbHelper.ExeProcessSql("update tblSubscription set nPaymentMethodId=" + pseudoCart.mnPaymentId + "where nSubKey = " + myWeb.moRequest["subId"]);
                                            ewCmd = "";
                                            bPaymentMethodUpdated = true;
                                            goto processFlow;
                                        }
                                        else
                                        {
                                            myWeb.moPageXml.FirstChild.AppendChild(pseudoOrder.xml);
                                            ContentDetailElmt.AppendChild(ccPaymentXform.moXformElmt);
                                        }

                                        break;
                                    }

                                case "UpdateSubscription":
                                    {
                                        // Save updated subscription details
                                        ContentDetailElmt.SetAttribute("status", "SubscriptionUpdated");
                                        break;
                                    }
                                    // Cancel old method

                            }

                            myWeb.moPageXml.FirstChild.AppendChild(ContentDetailElmt);
                        }
                        catch (Exception ex)
                        {
                            stdTools.returnException(ref myWeb.msException, mcModuleName, "UpdateSubscriptionPaymentMethod", ex, "", "", gbDebug);
                            // Return Nothing
                        }

                    }

                    public void ManageUserSubscriptions(ref Cms myWeb, ref XmlElement contentNode)
                    {

                        string sSql;
                        DataSet oDS;
                        XmlElement oElmt;
                        bool listSubs = true;
                        bool listDetail = true;
                        try
                        {

                            if (myWeb.mnUserId > 0)
                            {

                                sSql = "select a.nStatus as status, nSubKey as id, cSubName as name, cSubXml, dStartDate, a.dPublishDate, a.dExpireDate, nPeriod as period, cPeriodUnit as periodUnit, nValueNet as value, cRenewalStatus as renewalStatus, pay.nPayMthdKey as paymentMethodId, pay.cPayMthdProviderName as providerName, pay.cPayMthdProviderRef as providerRef, pay.cPayMthdDetailXml" + " from tblSubscription sub INNER JOIN tblAudit a ON sub.nAuditId = a.nAuditKey " + " LEFT OUTER JOIN tblCartPaymentMethod pay on pay.nPayMthdKey = sub.nPaymentMethodId " + " where sub.nDirId = " + myWeb.mnUserId;
                                object cmd = myWeb.moRequest["subCmd"];
                            logicstart:
                                ;


                                switch (cmd)
                                {
                                    case "updateSubPayment":
                                        {
                                            UpdateSubscriptionPaymentMethod(ref myWeb, ref contentNode);
                                            listSubs = false;
                                            break;
                                        }
                                    case "cancelSub":
                                    case "cancelSubscription":
                                        {
                                            var oSubs = new Subscriptions(ref myWeb);
                                            oSubs.CancelSubscription(Conversions.ToInteger(myWeb.moRequest["subId"]));
                                            break;
                                        }
                                    case "edit":
                                        {
                                            myWeb.moContentDetail = myWeb.moPageXml.CreateElement("ContentDetail");
                                            contentNode = myWeb.moContentDetail;
                                            myWeb.moPageXml.DocumentElement.AppendChild(contentNode);
                                            var oADX = new Cms.Admin.AdminXforms(ref myWeb);
                                            XmlElement subXform = oADX.xFrmEditUserSubscription(Convert.ToInt32(myWeb.moRequest["subId"]), "/xforms/Subscription/EditUserSubscription.xml");
                                            if (oADX.valid)
                                            {
                                                cmd = "details";
                                                goto logicstart;
                                            }
                                            else
                                            {
                                                myWeb.moContentDetail.AppendChild(myWeb.moContentDetail.OwnerDocument.ImportNode(subXform, true));
                                                listSubs = false;
                                            }

                                            break;
                                        }

                                    case "details":
                                        {
                                            myWeb.moContentDetail = myWeb.moPageXml.CreateElement("ContentDetail");
                                            contentNode = myWeb.moContentDetail;
                                            myWeb.moPageXml.DocumentElement.AppendChild(contentNode);
                                            myWeb.mnArtId = Conversions.ToInteger(myWeb.moRequest["subId"]);
                                            sSql = sSql + " and nSubKey = " + myWeb.moRequest["subId"];
                                            listSubs = true;
                                            break;
                                        }
                                }


                                if (listSubs)
                                {
                                    oDS = myWeb.moDbHelper.GetDataSet(sSql, "tblSubscription", "OrderList");
                                    foreach (DataRow oDr in oDS.Tables[0].Rows)
                                    {
                                        oElmt = myWeb.moPageXml.CreateElement("Subscription");
                                        oElmt.InnerXml = Conversions.ToString(oDr["cSubXml"]);
                                        oElmt.SetAttribute("status", oDr["status"].ToString());
                                        oElmt.SetAttribute("id", oDr["id"].ToString());
                                        oElmt.SetAttribute("name", oDr["name"].ToString());
                                        oElmt.SetAttribute("startDate", XmlDate(oDr["dStartDate"]));
                                        oElmt.SetAttribute("publishDate", XmlDate(oDr["dPublishDate"]));
                                        oElmt.SetAttribute("expireDate", XmlDate(oDr["dExpireDate"]));
                                        oElmt.SetAttribute("period", oDr["period"].ToString());
                                        oElmt.SetAttribute("periodUnit", oDr["periodUnit"].ToString());
                                        oElmt.SetAttribute("value", oDr["value"].ToString());
                                        oElmt.SetAttribute("renewalStatus", oDr["renewalStatus"].ToString());
                                        oElmt.SetAttribute("providerName", oDr["providerName"].ToString());
                                        oElmt.SetAttribute("providerRef", oDr["providerRef"].ToString());


                                        if (!string.IsNullOrEmpty(oDr["providerName"].ToString()))
                                        {
                                            var oPayProv = new Providers.Payment.BaseProvider(ref myWeb, oDr["providerName"].ToString());

                                            string paymentStatus;
                                            try
                                            {
                                                paymentStatus = Conversions.ToString(oPayProv.Activities.CheckStatus(myWeb, oDr["paymentMethodId"].ToString()));
                                                oElmt.SetAttribute("paymentStatus", paymentStatus);
                                                var oPaymentMethodDetails = myWeb.moPageXml.CreateElement("PaymentMethodDetails");
                                                oPaymentMethodDetails.InnerXml = Conversions.ToString(oPayProv.Activities.GetMethodDetail(myWeb, oDr["paymentMethodId"].ToString()));
                                                oElmt.AppendChild(oPaymentMethodDetails);
                                            }
                                            catch (Exception ex2)
                                            {
                                                oElmt.SetAttribute("paymentStatus", "error");
                                            }
                                        }
                                        else
                                        {
                                            oElmt.SetAttribute("paymentStatus", "unknown");
                                        }

                                        // Get Payment Method Details
                                        var oPaymentMethod = myWeb.moPageXml.CreateElement("PaymentMethod");
                                        if (!(oDr["paymentMethodId"] is DBNull))
                                        {
                                            oPaymentMethod.InnerXml = myWeb.moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.CartPaymentMethod, Conversions.ToLong(oDr["paymentMethodId"]));
                                        }
                                        // oPaymentMethod.InnerXml = CStr(oDr("cPayMthdDetailXml") & "")
                                        oElmt.AppendChild(oPaymentMethod);

                                        contentNode.AppendChild(oElmt);
                                    }
                                }

                                if (listDetail)
                                {

                                    // show the renewal/payment history

                                }


                            }
                        }

                        catch (Exception ex)
                        {
                            stdTools.returnException(ref myWeb.msException, mcModuleName, "ManageUserSubscriptions", ex, "", "", gbDebug);
                            // Return Nothing
                        }

                    }

                    public void Subscribe(ref Cms myWeb, ref XmlElement contentNode)
                    {

                        bool listSubs = true;
                        string sProcessInfo = "Subscribe";
                        try
                        {

                            // First we check if free trail

                            if (Conversions.ToInteger("0" + contentNode.SelectSingleNode("Prices/Price[@type='sale']").InnerText) == 0 & Conversions.ToInteger("0" + contentNode.SelectSingleNode("SubscriptionPrices/Price[@type='sale']").InnerText) == 0)
                            {

                                if (myWeb.mnUserId > 0)
                                {

                                    if (myWeb.moRequest["subCmd"] == "Subscribe")
                                    {

                                        var oSubs = new Subscriptions(ref myWeb);
                                        oSubs.AddUserSubscription(myWeb.mnArtId, myWeb.mnUserId);

                                        // Email site owner with new subscription details
                                        // send registration confirmation
                                        var oUserElmt = myWeb.GetUserXML((long)myWeb.mnUserId);
                                        var oMsg = new Protean.Messaging(ref myWeb.msException);
                                        XmlElement oUserEmail = (XmlElement)oUserElmt.SelectSingleNode("Email");
                                        string fromName = myWeb.moConfig["SiteAdminName"];
                                        string fromEmail = myWeb.moConfig["SiteAdminEmail"];
                                        string recipientEmail = "";
                                        if (oUserEmail != null)
                                            recipientEmail = oUserEmail.InnerText;

                                        // send an email to the new registrant
                                        var ofs = new Protean.fsHelper();
                                        string xsltPath = ofs.checkCommonFilePath(myWeb.moConfig["ProjectPath"] + "/xsl/email/subscribeTrial.xsl");

                                        var EmailContent = myWeb.moPageXml.CreateElement("Page");
                                        EmailContent.AppendChild(contentNode.CloneNode(true));
                                        EmailContent.AppendChild(oUserElmt);

                                        if (File.Exists(myWeb.goServer.MapPath(xsltPath)))
                                        {


                                            string SubjectLine = "Your Trial Subscription";

                                            if (!string.IsNullOrEmpty(recipientEmail))
                                            {
                                                Cms.dbHelper argodbHelper = null;
                                                sProcessInfo = Conversions.ToString(oMsg.emailer(EmailContent, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, odbHelper: ref argodbHelper, "Message Sent", "Message Failed"));
                                            }
                                            // send an email to the webadmin

                                        }

                                        recipientEmail = myWeb.moConfig["SiteAdminEmail"];
                                        string xsltPath2 = ofs.checkCommonFilePath(myWeb.moConfig["ProjectPath"] + "/xsl/email/subscribeTrialAlert.xsl");

                                        if (File.Exists(myWeb.goServer.MapPath(xsltPath2)))
                                        {
                                            string SubjectLine = "New Trial Subscription";
                                            Cms.dbHelper argodbHelper1 = null;
                                            sProcessInfo = Conversions.ToString(oMsg.emailer(EmailContent, xsltPath2, "New User", recipientEmail, fromEmail, SubjectLine, odbHelper: ref argodbHelper1, "Message Sent", "Message Failed"));
                                        }
                                        // Adding user to group could have added them to a triggered email list.

                                        oMsg = (Protean.Messaging)null;

                                        // Redirect user to page
                                        myWeb.msRedirectOnEnd = myWeb.mcPagePath;

                                    }
                                }

                                else
                                {
                                    // If not registered we need to do so

                                    var newContent = myWeb.moPageXml.CreateElement("Content");
                                    newContent.SetAttribute("type", "xform");

                                    var memberMods = new Cms.Membership.Modules();
                                    memberMods.Register(ref myWeb, ref newContent);

                                    contentNode.AppendChild(newContent);

                                }

                            }
                        }

                        catch (Exception ex)
                        {
                            stdTools.returnException(ref myWeb.msException, mcModuleName, "ManageUserSubscriptions", ex, sProcessInfo, "", gbDebug);
                            // Return Nothing
                        }

                    }


                    public void CheckUpgrade(ref Cms myWeb, ref XmlElement contentNode)
                    {

                        string sProcessInfo = "CheckUpgrade";
                        try
                        {

                            // Check User Logged on
                            if (myWeb.mnUserId > 0)
                            {

                                var oSub = new Subscriptions(ref myWeb);
                                // Upgrade Price
                                oSub.UpdateSubscriptionPrice(contentNode, myWeb.mnUserId);

                            }
                        }



                        catch (Exception ex)
                        {
                            stdTools.returnException(ref myWeb.msException, mcModuleName, "CheckUpgrade", ex, sProcessInfo, "", gbDebug);
                            // Return Nothing
                        }

                    }

                }

            }
        }
    }
}
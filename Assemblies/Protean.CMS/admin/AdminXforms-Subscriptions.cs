// ***********************************************************************
// $Library:     protean.cms.adminXforms
// $Revision:    3.1  
// $Date:        2006-03-02
// $Author:      Trevor Spink (trevor@eonic.com)
// &Website:     eonic.com
// &Licence:     Apache-2.0 license
// $Copyright:   Copyright (c) 2002 - 2022 Eonic Digital LLP.
// ***********************************************************************

using System;
using System.Xml;
using static Protean.stdTools;
using static Protean.Tools.Xml;

namespace Protean
{

    public partial class Cms
    {
        public partial class Admin
        {
            public partial class AdminXforms : xForm, IDisposable
            {

                public XmlElement xFrmEditUserSubscription(int nSubId, string xFormPath = "/xforms/Subscription/EditSubscription.xml")
                {
                    string cProcessInfo = "";
                    try
                    {

                        if (moRequest["reset"]?.ToLower() == "true")
                        {
                            myWeb.moSession["tempInstance"] = (object)null;
                        }

                        base.NewFrm("EditUserSubscription");
                        base.bProcessRepeats = false;
                        base.load(xFormPath, myWeb.maCommonFolders);

                        if (nSubId > 0)
                        {
                            base.bProcessRepeats = true;
                            if (myWeb.moSession["tempInstance"] is null)
                            {
                                var existingInstance = base.moXformElmt.OwnerDocument.CreateElement("instance");
                                existingInstance.InnerXml = moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Subscription, (long)nSubId).Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "").Replace("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
                                base.LoadInstance(existingInstance);
                                myWeb.moSession["tempInstance"] = base.Instance;
                            }
                            else
                            {
                                base.LoadInstance((XmlElement)myWeb.moSession["tempInstance"]);
                            }
                        }

                        moXformElmt.SelectSingleNode("descendant-or-self::instance").InnerXml = base.Instance.InnerXml;
                        //int i = 1;
                        // bool bDone = false;
                        /// string cItems = "";
                        long initialSubContentId = Convert.ToInt64("0" + base.Instance.SelectSingleNode("tblSubscription/nSubContentId").InnerText);



                        if (base.isSubmitted() | base.isTriggered)
                        {
                            base.updateInstanceFromRequest();

                            long ContentId = Convert.ToInt64("0" + base.Instance.SelectSingleNode("tblSubscription/nSubContentId").InnerText);
                            var ContentXml = myWeb.moPageXml.CreateElement("Content");
                            ContentXml.InnerXml = moDbHelper.getContentBrief((int)ContentId);

                            if (initialSubContentId != ContentId)
                            {
                                // Now we populate the instance
                                base.Instance.SelectSingleNode("tblSubscription/cSubName").InnerText = ContentXml.SelectSingleNode("Content/Name").InnerText;
                                base.Instance.SelectSingleNode("tblSubscription/cSubXml").InnerXml = ContentXml.InnerXml;
                                // dStartDate Populated by form
                                base.Instance.SelectSingleNode("tblSubscription/nPeriod").InnerText = ContentXml.SelectSingleNode("Content/Duration/Length").InnerText;
                                base.Instance.SelectSingleNode("tblSubscription/cPeriodUnit").InnerText = ContentXml.SelectSingleNode("Content/Duration/Unit").InnerText;
                                base.Instance.SelectSingleNode("tblSubscription/nMinimumTerm").InnerText = ContentXml.SelectSingleNode("Content/Duration/MinimumTerm").InnerText;
                                base.Instance.SelectSingleNode("tblSubscription/nRenewalTerm").InnerText = ContentXml.SelectSingleNode("Content/Duration/RenewalTerm").InnerText;
                                base.Instance.SelectSingleNode("tblSubscription/nValueNet").InnerText = ContentXml.SelectSingleNode("Content/SubscriptionPrices/Price[@type='sale']").InnerText;
                                base.Instance.SelectSingleNode("tblSubscription/cRenewalStatus").InnerText = ContentXml.SelectSingleNode("Content/Type").InnerText;
                                base.Instance.SelectSingleNode("tblSubscription/dPublishDate").InnerText = base.Instance.SelectSingleNode("tblSubscription/dStartDate").InnerText;
                            }

                            if (nSubId == 0)
                            {
                                // we are creating a new subscription
                                // first we get the subscription content XML
                                base.Instance.SelectSingleNode("tblSubscription/nDirId").InnerText = myWeb.moRequest["userId"];
                                base.Instance.SelectSingleNode("tblSubscription/nDirType").InnerText = "user";
                                base.Instance.SelectSingleNode("tblSubscription/dPublishDate").InnerText = base.Instance.SelectSingleNode("tblSubscription/dStartDate").InnerText;

                                // Calculate Renewal Date
                                var oSub = new Cms.Cart.Subscriptions();
                                var dSubEndDate = oSub.SubscriptionEndDate(Convert.ToDateTime(base.Instance.SelectSingleNode("tblSubscription/dStartDate").InnerText), (XmlElement)ContentXml.SelectSingleNode("Content"));
                                base.Instance.SelectSingleNode("tblSubscription/dExpireDate").InnerText = XmlDate((object)dSubEndDate);
                            }

                            // updating an existing subscription
                            else if (base.Instance.SelectSingleNode("tblSubscription/cRenewalStatus").InnerText != "Cancelled")
                            {
                                base.Instance.SelectSingleNode("tblSubscription/nStatus").InnerText = "1";
                            }
                            if (base.isSubmitted())
                            {
                                base.validate();
                            }

                            if (base.valid)
                            {
                                int nCId = Convert.ToInt16(moDbHelper.setObjectInstance(Cms.dbHelper.objectTypes.Subscription, base.Instance, (long)nSubId));
                                if (base.Instance.SelectSingleNode("tblSubscription/cRenewalStatus").InnerText != "Cancelled")
                                {
                                    if (nSubId > 0)
                                    {
                                        foreach (XmlElement oElmt in base.Instance.SelectNodes("tblSubscription/cSubXml/Content/UserGroups/Group[@id!='']"))
                                        {
                                            int nGrpID = Convert.ToInt16(oElmt.Attributes["id"].Value);
                                            myWeb.moDbHelper.saveDirectoryRelations((long)Convert.ToInt16(base.Instance.SelectSingleNode("tblSubscription/nDirId").InnerText), nGrpID.ToString());
                                        }
                                    }
                                }
                                myWeb.moSession["tempInstance"] = (object)null;
                            }

                            else if (base.isTriggered)
                            {
                                // we have clicked a trigger so we must update the instance
                                base.updateInstanceFromRequest();
                                // lets save the instance
                                goSession["tempInstance"] = base.Instance;
                            }
                            else
                            {
                                goSession["tempInstance"] = base.Instance;
                            }
                        }

                        base.addValues();

                        return base.moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmEditUserSubscription", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public XmlElement xFrmRenewSubscription(string nSubscriptionId)
                {
                    string cProcessInfo = "";
                    try
                    {

                        var oSub = new Cms.Cart.Subscriptions(ref myWeb);

                        base.NewFrm("RenewSubscription");
                        base.submission("RenewSubscription", "", "post");
                        XmlElement oFrmElmt;

                        XmlElement InstanceElmt = base.Instance;
                        oSub.GetSubscriptionDetail(ref InstanceElmt, Convert.ToInt16(nSubscriptionId));



                        base.Instance = InstanceElmt;
                        XmlElement SubXml = (XmlElement)base.Instance.FirstChild;
                        // calculate new expiry date

                        int renewIntervalDays = 1;
                        switch (SubXml.GetAttribute("periodUnit"))
                        {
                            case "Week":
                                {
                                    renewIntervalDays = 7;
                                    break;
                                }
                            case "Year":
                                {
                                    renewIntervalDays = 365;
                                    break;
                                }
                        }
                        long SubId = Convert.ToInt64(SubXml.GetAttribute("id"));

                        var expireDate = Convert.ToDateTime(SubXml.GetAttribute("expireDate"));
                        var dNewStart = expireDate.AddDays(1);
                        
                        DateTime dNewEnd;
                        switch (SubXml.GetAttribute("periodUnit"))
                        {
                            case "Week":
                                dNewEnd = expireDate.AddDays(Convert.ToInt16(SubXml.GetAttribute("period")) * 7);
                                break;
                            case "Year":
                                dNewEnd = expireDate.AddYears(Convert.ToInt16(SubXml.GetAttribute("period")));
                                break;
                            default:
                                dNewEnd = expireDate.AddDays(Convert.ToInt16(SubXml.GetAttribute("period")));
                                break;
                        }
                        
                        double RenewalCost = Convert.ToDouble(SubXml.GetAttribute("value"));
                        SubXml.SetAttribute("newStart", XmlDate(dNewStart));
                        SubXml.SetAttribute("newExpire", XmlDate(dNewEnd));

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "RenewSubscription");

                        base.addInput(ref oFrmElmt, "nUserID", false, "UserId", "hidden");
                        base.addInput(ref oFrmElmt, "nSubscriptionId", false, "SubscriptionId", "hidden");

                        var oSkipPay = base.addSelect(ref oFrmElmt, "skipPayment", true, "", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSkipPay, "Renew Without Payment", "yes");
                        // MyBase.addValue(oSkipPay, "no")

                        var oSelElmt = base.addSelect(ref oFrmElmt, "emailClient", true, "", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Email Renewal Invoice", "yes");
                        base.addValue(ref oSelElmt, "yes");

                        //XmlNode argoNode = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Hint, "Renew Subscription", true, "renew-sub");
                        // oFrmElmt = (XmlElement)argoNode;

                        base.addSubmit(ref oFrmElmt, "Back", "Back", "Back", "btn-default", "fa-chevron-left");
                        base.addSubmit(ref oFrmElmt, "Confirm", "Confirm Renewal", "Confirm", "btn-success principle", "fa-repeat");

                        if (isSubmitted())
                        {
                            if (base.getSubmitted() == "Back")
                            {
                                myWeb.msRedirectOnEnd = "/?ewCmd=RenewSubscription";
                                return base.moXformElmt;
                            }
                            else if (base.getSubmitted() == "Confirm")
                            {
                                bool bEmailClient = false;
                                if (myWeb.moRequest["emailClient"] == "yes")
                                    bEmailClient = true;
                                string RenewResponse;
                                bool skipPayment = false;
                                if (myWeb.moRequest["skipPayment"] == "yes")
                                    skipPayment = true;

                                RenewResponse = oSub.RenewSubscription(Convert.ToInt64(nSubscriptionId), bEmailClient, skipPayment);
                                if (RenewResponse == "Success")
                                {
                                    base.valid = true;
                                    return base.moXformElmt;
                                }
                                else
                                {
                                    //XmlNode argoNode1 = oFrmElmt;
                                    base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, RenewResponse);
                                    //oFrmElmt = (XmlElement)argoNode1;
                                    base.valid = false;

                                }

                            }
                        }
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmSchedulerItem", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public XmlElement xFrmResendSubscription(string nOrderId)
                {
                    string cProcessInfo = "";
                    try
                    {

                        string nSubscriptionId = moDbHelper.ExeProcessSqlScalar("select nSubId from tblSubscriptionRenewal where nOrderId = " + nOrderId);

                        var oSub = new Cms.Cart.Subscriptions(ref myWeb);

                        base.NewFrm("RenewSubscription");
                        base.submission("RenewSubscription", "", "post");
                        XmlElement oFrmElmt;

                        var argoParentElmt = base.Instance;
                        oSub.GetSubscriptionDetail(ref argoParentElmt, Convert.ToInt16(nSubscriptionId));
                        base.Instance = argoParentElmt;
                        object SubXml = base.Instance.FirstChild;

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "RenewSubscription");

                        base.addInput(ref oFrmElmt, "nUserID", false, "UserId", "hidden");
                        base.addInput(ref oFrmElmt, "nSubscriptionId", false, "SubscriptionId", "hidden");
                        var oSelElmt = base.addSelect(ref oFrmElmt, "emailClient", true, "", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Email Renewal Invoice", "yes");
                        base.addValue(ref oSelElmt, "yes");

                        // XmlNode argoNode = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Hint, "Resend Subscription", true, "resend-sub");
                        // oFrmElmt = (XmlElement)argoNode;

                        base.addSubmit(ref oFrmElmt, "Back", "Back", "Back", "btn-default", "fa-chevron-left");
                        base.addSubmit(ref oFrmElmt, "Confirm", "Confirm Refresh and Resend", "Confirm", "btn-success principle", "fa-repeat");

                        if (isSubmitted())
                        {
                            if (base.getSubmitted() == "Back")
                            {
                                myWeb.msRedirectOnEnd = "/?ewCmd=ResendSubscription";
                                return base.moXformElmt;
                            }
                            else if (base.getSubmitted() == "Confirm")
                            {
                                bool bEmailClient = false;
                                if (myWeb.moRequest["emailClient"] == "yes")
                                    bEmailClient = true;
                                string RenewResponse;
                                RenewResponse = oSub.RefreshSubscriptionOrder((XmlElement)base.Instance.FirstChild, bEmailClient, Convert.ToInt64(nOrderId));
                                if (RenewResponse == "Success")
                                {
                                    base.valid = true;
                                }
                                else
                                {
                                    //XmlNode argoNode1 = oFrmElmt;
                                    base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Renewal Resend Failed");
                                    //oFrmElmt = (XmlElement)argoNode1;
                                    base.valid = false;
                                }

                                return base.moXformElmt;
                            }
                        }
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmSchedulerItem", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmConfirmCancelSubscription(string nUserId, string nSubscriptionId, long nCurrentUser, bool bAdminMode)
                {

                    try
                    {
                        if (!Tools.Number.IsNumeric(nUserId))
                            nUserId = 0.ToString();
                        if (!Tools.Number.IsNumeric(nSubscriptionId))
                            nSubscriptionId = 0.ToString();
                        base.NewFrm("CancelSubscription");
                        base.submission("CancelSubscription", "", "post");
                        XmlElement oFrmElmt;

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "CancelSubscription");

                        base.addInput(ref oFrmElmt, "nUserID", false, "UserId", "hidden");
                        base.addInput(ref oFrmElmt, "nSubscriptionId", false, "SubscriptionId", "hidden");

                        base.addInput(ref oFrmElmt, "cStatedReason", false, "Reason for cancelation");

                        var oSelElmt = base.addSelect(ref oFrmElmt, "emailClient", true, "", "", Protean.xForm.ApperanceTypes.Full);
                        base.addOption(ref oSelElmt, "Email Cancelation Notice", "yes");
                        base.addValue(ref oSelElmt, "yes");

                        //XmlNode argoNode = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Hint, "Are you sure you wish to cancel this subscription", true);
                        //oFrmElmt = (XmlElement)argoNode;

                        base.addSubmit(ref oFrmElmt, "Back", "Back", "Back", "btn-default", "fa-chevron-left");
                        base.addSubmit(ref oFrmElmt, "Cancel", "Cancel Subscription", "Cancel", "btn-warning principle", "fa-stop");

                        if (isSubmitted())
                        {
                            if (base.getSubmitted() == "Back")
                            {
                                return base.moXformElmt;
                            }
                            else if (base.getSubmitted() == "Cancel")
                            {

                                bool bEmailClient = true;
                                if (moRequest["emailClient"] != "yes")
                                    bEmailClient = false;
                                var oSub = new Cms.Cart.Subscriptions(ref myWeb);
                                oSub.CancelSubscription(Convert.ToInt16(nSubscriptionId), myWeb.moRequest["cStatedReason"], bEmailClient);
                                base.valid = true;
                                return base.moXformElmt;
                            }
                        }
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmConfirmCancelSubscription", ex, "", bDebug: gbDebug);
                        return null;
                    }
                }

                public XmlElement xFrmConfirmExpireSubscription(string nUserId, string nSubscriptionId, int nCurrentUser, bool bAdminMode)
                {

                    try
                    {
                        if (!Tools.Number.IsNumeric(nUserId))
                            nUserId = 0.ToString();
                        if (!Tools.Number.IsNumeric(nSubscriptionId))
                            nSubscriptionId = 0.ToString();
                        base.NewFrm("CancelSubscription");
                        base.submission("CancelSubscription", "", "post");
                        XmlElement oFrmElmt;

                        oFrmElmt = base.addGroup(ref base.moXformElmt, "ExpireSubscription");

                        base.addInput(ref oFrmElmt, "nUserID", false, "UserId", "hidden");
                        base.addInput(ref oFrmElmt, "nSubscriptionId", false, "SubscriptionId", "hidden");

                        base.addInput(ref oFrmElmt, "cStatedReason", false, "Reason for expiry");

                        //XmlNode argoNode = oFrmElmt;
                        base.addNote(ref oFrmElmt, Protean.xForm.noteTypes.Hint, "Are you sure you wish this subscription to expire", true);
                        //oFrmElmt = (XmlElement)argoNode;

                        base.addSubmit(ref oFrmElmt, "Back", "Back", "Back", "btn-default", "fa-chevron-left");
                        base.addSubmit(ref oFrmElmt, "Expire", "Expire Subscription", "Expire", "btn-warning principle", "fa-stop");

                        if (isSubmitted())
                        {
                            if (base.getSubmitted() == "Back")
                            {
                                return base.moXformElmt;
                            }
                            else if (base.getSubmitted() == "Expire")
                            {
                                var oSub = new Cms.Cart.Subscriptions(ref myWeb);
                                oSub.ExpireSubscription(Convert.ToInt16(nSubscriptionId), myWeb.moRequest["cStatedReason"]);
                                base.valid = true;
                                return base.moXformElmt;
                            }
                        }
                        return base.moXformElmt;
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmConfirmCancelSubscription", ex, "", bDebug: gbDebug);
                        return null;
                    }
                }


            }
        }
    }
}
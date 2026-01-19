using Protean.Providers.Membership;
using Protean.Providers.Messaging;
using Protean.Providers.Payment;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using static Protean.Cms;
using static Protean.Cms.dbHelper;
using static Protean.stdTools;
using static Protean.Tools.Xml;

namespace Protean
{


    public partial class Cms
    {

        public partial class Cart : IDisposable
        {


            public virtual Cms.xForm optionsXform(ref XmlElement cartElmt)
            {

                myWeb.PerfMon.Log("Cart", "optionsXform");
                DataSet ods;
                DataSet ods2;
                DataRow oRow;

                string sSql;
                string sSql2;

                XmlElement oGrpElmt;

                short nQuantity;
                double nAmount;
                double nWeight;
                string cDestinationCountry;
                string cDestinationPostalCode = "";
                var nShippingCost = default(double);
                string cShippingDesc = "";

                string cHidden = string.Empty;
                bool bHideDelivery = false;
                bool bHidePayment = false;
                bool bFirstRow = true;

                // Dim oElmt As XmlElement

                string sProcessInfo = string.Empty;
                bool bForceValidation = false;
                bool bAdjustTitle = true;

                // Dim cFormURL As String
                // Dim cExternalGateway As String
                // Dim cBillingAddress As String
                // Dim cPaymentResponse As String
                string cProcessInfo = "";
                bool bAddTerms = false;
                PaymentProviders oPay;
                bool bDeny = false;
                var AllowedPaymentMethods = new System.Collections.Specialized.StringCollection();

                if (moPay is null)
                {
                    oPay = new PaymentProviders(ref myWeb);
                }
                else
                {
                    oPay = moPay;
                }

                oPay.mcCurrency = mcCurrency;
                string cDenyFilter = string.Empty;

                try
                {

                    if (moDBHelper.checkTableColumnExists("tblCartShippingPermission", "nPermLevel"))
                    {
                        bDeny = true;
                        cDenyFilter = " and nPermLevel <> 0";
                    }

                    if (string.IsNullOrEmpty(moCartConfig["TermsContentId"]) & string.IsNullOrEmpty(moCartConfig["TermsAndConditions"]))
                        bAddTerms = false;

                    nQuantity = (short)Convert.ToInt16("0" + cartElmt.GetAttribute("itemCount"));
                    nAmount = Convert.ToDouble("0" + cartElmt.GetAttribute("totalNet")) - Convert.ToDouble("0" + cartElmt.GetAttribute("shippingCost"));
                    nWeight = Convert.ToDouble("0" + cartElmt.GetAttribute("weight"));

                    double nRepeatAmount = Convert.ToDouble("0" + cartElmt.GetAttribute("repeatPrice"));

                    int nShippingMethodId = (int)Math.Round(Convert.ToDouble("0" + cartElmt.GetAttribute("shippingType")));

                    if (cartElmt.SelectSingleNode("Contact[@type='Delivery Address']/Country") is null)
                    {
                        sProcessInfo = "Destination Country not specified in Delivery Address";
                        cDestinationCountry = "";
                        string sTarget = "";
                        foreach (XmlElement oAddressElmt in cartElmt.SelectSingleNode("Contact[@type='Delivery Address']/*"))
                        {
                            if (!string.IsNullOrEmpty(sTarget))
                                sTarget = sTarget + ", ";
                            sTarget = sTarget + oAddressElmt.InnerText;
                        }
                        throw new ApplicationException($"Error 1004 in getParentCountries: {sTarget} Destination Country not specified in Delivery Address.");
                    }
                    else
                    {
                        cDestinationCountry = cartElmt.SelectSingleNode("Contact[@type='Delivery Address']/Country").InnerText;
                        cDestinationPostalCode = cartElmt.SelectSingleNode("Contact[@type='Delivery Address']/PostalCode").InnerText;
                    }
                    if (string.IsNullOrEmpty(cDestinationCountry))
                        cDestinationCountry = moCartConfig["DefaultCountry"];
                    // Go and collect the valid shipping options available for this order
                    ods = getValidShippingOptionsDS(cDestinationCountry, cDestinationPostalCode, nAmount, nQuantity, nWeight, "", 0);

                    var oOptXform = new Cms.xForm(ref myWeb.msException);
                    oOptXform.moPageXML = moPageXml;

                    if (!oOptXform.load("/xforms/Cart/Options.xml"))
                    {
                        string notesXml = "";
                        if (cartElmt.SelectSingleNode("Notes") != null)
                        {
                            notesXml = cartElmt.SelectSingleNode("Notes").OuterXml;
                        }
                        oOptXform.NewFrm("optionsForm");
                        oOptXform.Instance.InnerXml = "<nShipOptKey/><cPaymentMethod/><terms/><confirmterms>No</confirmterms><tblCartOrder><cShippingDesc/><cClientNotes>" + notesXml + "</cClientNotes></tblCartOrder>";
                        if (!(string.IsNullOrEmpty(moCartConfig["TermsContentId"]) & string.IsNullOrEmpty(moCartConfig["TermsAndConditions"])))
                        {
                            bAddTerms = true;
                        }
                    }
                    else
                    {
                        bAdjustTitle = false;
                        bForceValidation = true;
                        if (!(string.IsNullOrEmpty(moCartConfig["TermsContentId"]) & string.IsNullOrEmpty(moCartConfig["TermsAndConditions"])))
                        {
                            bAddTerms = true;
                        }
                    }

                    // If there is already a submit item in the form, then maintain the event node
                    // Would rather that this whole form obeyed xform validation, but hey-ho. Ali
                    string cEvent = "";
                    XmlElement oSub = (XmlElement)oOptXform.model.SelectSingleNode("submission");
                    if (oSub is null)
                    {
                        cEvent = "return form_check(this);";
                    }
                    else
                    {
                        cEvent = oSub.GetAttribute("event");

                        // now remove the origional submit node coz we are going to add another. TS.
                        oSub.ParentNode.RemoveChild(oSub);
                    }

                    oOptXform.submission("optionsForm", mcPagePath + "cartCmd=ChoosePaymentShippingOption", "POST", cEvent);

                    string cUserGroups = "";

                    long rowCount = ods.Tables["Option"].Rows.Count;

                    if (bDeny)
                    {
                        // remove denied delivery methods
                        if (myWeb.mnUserId > 0)
                        {
                            foreach (XmlElement grpElmt in moPageXml.SelectNodes("/Page/User/Group[@isMember='yes']"))
                                cUserGroups = cUserGroups + grpElmt.GetAttribute("id") + ",";
                            cUserGroups = cUserGroups + Cms.gnAuthUsers;
                        }
                        else
                        {
                            cUserGroups = Cms.gnNonAuthUsers.ToString();
                        }

                        foreach (DataRow currentORow in ods.Tables["Option"].Rows)
                        {
                            oRow = currentORow;
                            int denyCount = 0;
                            if (bDeny)
                            {
                                string permSQL;
                                // check option is not denied
                                if (!string.IsNullOrEmpty(cUserGroups))
                                {
                                    permSQL = "select count(*) from tblCartShippingPermission where nPermLevel = 0 and nDirId IN (" + cUserGroups + ") and nShippingMethodId = " + oRow["nShipOptKey"].ToString();
                                    denyCount = Convert.ToInt16(moDBHelper.ExeProcessSqlScalar(permSQL));
                                }
                            }
                            if (denyCount > 0)
                            {
                                oRow.Delete();
                                rowCount = rowCount - 1L;
                            }
                        }
                    }

                    if (rowCount == 0L)
                    {

                        oOptXform.addGroup(ref oOptXform.moXformElmt, "options");
                        cartElmt.SetAttribute("errorMsg", 3.ToString());
                    }

                    else
                    {

                        // Build the Payment Options
                        // if the root group element exists i.e. we have loaded a form in.
                        oGrpElmt = (XmlElement)oOptXform.moXformElmt.SelectSingleNode("group");
                        if (oGrpElmt is null)
                        {
                            oGrpElmt = oOptXform.addGroup(ref oOptXform.moXformElmt, "options", "", "Select Payment Method");
                        }


                        // Even if there is only 1 option we still want to display it, if it is a non-zero value - the visitor should know the description of their delivery option
                        if (ods.Tables["Option"].Rows.Count == 1)
                        {
                            foreach (DataRow currentORow1 in ods.Tables["Option"].Rows)
                            {
                                oRow = currentORow1;
                                bool bCollection = false;
                                if (!(oRow["bCollection"] is DBNull))
                                {
                                    if (!oRow["bCollection"].Equals("1"))
                                        bCollection = true;
                                }
                                if (oRow["nShippingTotal"] is DBNull)
                                {
                                    cHidden = " hidden";
                                    bHideDelivery = true;
                                }
                                else if (oRow["nShippingTotal"] != null && Convert.ToDouble(oRow["nShippingTotal"]) == 0 && !bCollection)
                                {
                                    cHidden = " hidden";
                                    bHideDelivery = true;
                                }
                                else
                                {

                                    // Calculate any shipping cost overage
                                    nShippingCost = Math.Round(Convert.ToDouble(oRow["nShippingTotal"]), 2);
                                    double overageUnit = 0;
                                    if (oRow["nShipOptWeightOverageUnit"] != DBNull.Value)
                                    {
                                        double.TryParse(oRow["nShipOptWeightOverageUnit"].ToString(), out overageUnit);
                                    }

                                    double overageRate = 0;
                                    if (oRow["nShipOptWeightOverageRate"] != DBNull.Value)
                                    {
                                        double.TryParse(oRow["nShipOptWeightOverageRate"].ToString(), out overageRate);
                                    }

                                    double overageWeightMax = Convert.ToDouble(oRow["nShipOptWeightMax"]);
                                    nShippingCost = calcShippingCost(nShippingCost, overageUnit, overageRate, nWeight, overageWeightMax);

                                    oOptXform.addInput(ref oGrpElmt, "nShipOptKey", false, oRow["cShipOptName"].ToString() + "-" + oRow["cShipOptCarrier"].ToString(), "hidden");
                                    oOptXform.Instance.SelectSingleNode("nShipOptKey").InnerText = Convert.ToString(oRow["nShipOptKey"]);

                                    var DelInputElmt = oOptXform.addInput(ref oGrpElmt, "tblCartOrder/cShippingDesc", false, "Delivery", "readonly term4047");
                                    XmlElement DelInputElmtLabel = (XmlElement)DelInputElmt.SelectSingleNode("label");
                                    DelInputElmtLabel.SetAttribute("name", Convert.ToString(oRow["cShipOptName"]));
                                    DelInputElmtLabel.SetAttribute("carrier", Convert.ToString(oRow["cShipOptCarrier"]));
                                    DelInputElmtLabel.SetAttribute("cost", nShippingCost.ToString("0.00"));

                                    XmlElement DescElement = (XmlElement)oOptXform.Instance.SelectSingleNode("tblCartOrder/cShippingDesc");
                                    DescElement.InnerText = $"{oRow["cShipOptName"]}-{oRow["cShipOptCarrier"]}: {mcCurrencySymbol}{nShippingCost:0.00}";
                                    DescElement.SetAttribute("name", Convert.ToString(oRow["cShipOptName"]));
                                    DescElement.SetAttribute("carrier", Convert.ToString(oRow["cShipOptCarrier"]));
                                    DescElement.SetAttribute("cost", nShippingCost.ToString("0.00"));
                                }
                            }
                        }
                        else
                        {
                            oOptXform.addSelect1(ref oGrpElmt, "nShipOptKey", false, "Select Delivery", "radios multiline", Protean.xForm.ApperanceTypes.Full);
                            bFirstRow = true;
                            int nLastID = 0;

                            // If selected shipping method is still in those available (because we now )
                            if (nShippingMethodId != 0)
                            {
                                bool bIsAvail = false;
                                foreach (DataRow currentORow2 in ods.Tables["Option"].Rows)
                                {
                                    oRow = currentORow2;
                                    if (!(oRow.RowState == DataRowState.Deleted))
                                    {
                                        if (!nShippingMethodId.Equals(oRow["nShipOptKey"]))
                                        {
                                            bIsAvail = true;
                                        }
                                    }
                                }
                                // If not then strip it out.
                                if (bIsAvail == false)
                                {
                                    nShippingMethodId = 0;
                                    cartElmt.SetAttribute("shippingType", "0");
                                    cartElmt.SetAttribute("shippingCost", "");
                                    cartElmt.SetAttribute("shippingDesc", "");
                                    string cSqlUpdate = "UPDATE tblCartOrder SET cShippingDesc= null, nShippingCost=null, nShippingMethodId = 0 WHERE nCartOrderKey=" + mnCartId;
                                    moDBHelper.ExeProcessSql(cSqlUpdate);
                                }
                            }

                            // If shipping option selected is collection don't change
                            bool bCollectionSelected = false;
                            foreach (DataRow currentORow3 in ods.Tables["Option"].Rows)
                            {
                                oRow = currentORow3;
                                if (!(oRow.RowState == DataRowState.Deleted))
                                {
                                    if (!(oRow["bCollection"] is DBNull))
                                    {
                                        if (oRow["nShipOptKey"] != null && oRow["bCollection"] != null && oRow["nShipOptKey"].Equals(nShippingMethodId) && Convert.ToBoolean(oRow["bCollection"]))
                                        {
                                            bCollectionSelected = true;
                                        }
                                    }
                                }
                            }

                            foreach (DataRow currentORow4 in ods.Tables["Option"].Rows)
                            {
                                oRow = currentORow4;
                                if (!(oRow.RowState == DataRowState.Deleted))
                                {
                                    if (!oRow["nShipOptKey"].Equals(nLastID))
                                    {

                                        if (bCollectionSelected)
                                        {
                                            // if collection allready selected... Show only this option
                                            if (!nShippingMethodId.Equals(oRow["nShipOptKey"]))
                                            {
                                                oOptXform.Instance.SelectSingleNode("nShipOptKey").InnerText = Convert.ToString(oRow["nShipOptKey"]);
                                                double ShippingCost = 0;
                                                if (oRow["nShippingTotal"] != DBNull.Value)
                                                {
                                                    double.TryParse(oRow["nShippingTotal"].ToString(), out ShippingCost);
                                                }
                                                nShippingCost = Math.Round(nShippingCost, 2);

                                                XmlElement argoSelectNode = (XmlElement)oGrpElmt.LastChild;
                                                var optElmt = oOptXform.addOption(ref argoSelectNode, $"{oRow["cShipOptName"]}-{oRow["cShipOptCarrier"]}: {mcCurrencySymbol}{nShippingCost:0.00}", Convert.ToString(oRow["nShipOptKey"]));
                                                XmlElement optLabel = (XmlElement)optElmt.SelectSingleNode("label");
                                                optLabel.SetAttribute("name", Convert.ToString(oRow["cShipOptName"]));
                                                optLabel.SetAttribute("carrier", Convert.ToString(oRow["cShipOptCarrier"]));
                                                optLabel.SetAttribute("cost", nShippingCost.ToString("0.00"));
                                            }
                                        }
                                        else
                                        {
                                            bool bShowMethod = true;
                                            // Don't show if a collection method
                                            if (moDBHelper.checkTableColumnExists("tblCartShippingMethods", "bCollection"))
                                            {
                                                if (!(oRow["bCollection"] is DBNull))
                                                {
                                                    if (!oRow["bCollection"].Equals(true))
                                                    {
                                                        bShowMethod = false;
                                                    }
                                                }
                                            }
                                            if (bShowMethod)
                                            {
                                                if (bFirstRow)
                                                    oOptXform.Instance.SelectSingleNode("nShipOptKey").InnerText = Convert.ToString(oRow["nShipOptKey"]);
                                                nShippingCost = 0;
                                                if (oRow["nShippingTotal"] != DBNull.Value)
                                                {
                                                    double.TryParse(oRow["nShippingTotal"].ToString(), out nShippingCost);
                                                }
                                                nShippingCost = Math.Round(nShippingCost, 2);

                                                XmlElement argoSelectNode1 = (XmlElement)oGrpElmt.LastChild;
                                                var optElmt = oOptXform.addOption(ref argoSelectNode1, $"{oRow["cShipOptName"]}-{oRow["cShipOptCarrier"]}: {mcCurrencySymbol}{nShippingCost:0.00}", Convert.ToString(oRow["nShipOptKey"]));
                                                XmlElement optLabel = (XmlElement)optElmt.SelectSingleNode("label");
                                                optLabel.SetAttribute("name", Convert.ToString(oRow["cShipOptName"]));
                                                optLabel.SetAttribute("carrier", Convert.ToString(oRow["cShipOptCarrier"]));
                                                optLabel.SetAttribute("cost", nShippingCost.ToString("0.00"));
                                                bFirstRow = false;
                                                nLastID = Convert.ToInt16(oRow["nShipOptKey"]);
                                            }
                                        }

                                    }
                                }

                            }
                        }

                        ods = null;

                        if ((moCartConfig["NotesOnOptions"]).ToLower() == "on")
                        {

                            // Dim oNotesGrp As XmlElement = oOptXform.addGroup(oOptXform.moXformElmt, "notes", "term4051", "Please add any details for the delivery here")
                            string argsClass = "";
                            int argnRows = 0;
                            int argnCols = 0;
                            oOptXform.addTextArea(ref oGrpElmt, "tblCartOrder/cClientNotes/Notes/Notes", false, "Please add any details for the delivery here", ref argsClass, nRows: ref argnRows, nCols: ref argnCols);
                            // oGrpElmt.AppendChild(oNotesGrp)

                        }



                        // Allow to Select Multiple Payment Methods or just one
                        XmlNode oPaymentCfg;

                        oPaymentCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/payment");
                        // more than one..

                        bool bPaymentTypeButtons = false;
                        if ((moCartConfig["PaymentTypeButtons"]).ToLower() == "on")
                            bPaymentTypeButtons = true;

                        bFirstRow = true;
                        if (oPaymentCfg != null)
                        {
                            if (nAmount == 0d & nRepeatAmount == 0d)
                            {
                                if (!bPaymentTypeButtons)
                                {
                                    oOptXform.Instance.SelectSingleNode("cPaymentMethod").InnerText = "No Charge";
                                    var oSelectElmt = oOptXform.addSelect1(ref oGrpElmt, "cPaymentMethod", false, "Payment Method", "radios multiline", Protean.xForm.ApperanceTypes.Full);
                                    oOptXform.addOption(ref oSelectElmt, "No Charge", "No Charge");
                                    bHidePayment = false;
                                    AllowedPaymentMethods.Add("No Charge");
                                }
                            }

                            else if (oPaymentCfg.SelectNodes("provider").Count > 1)
                            {

                                if (!bPaymentTypeButtons)
                                {
                                    XmlElement oSelectElmt;
                                    oSelectElmt = (XmlElement)oOptXform.moXformElmt.SelectSingleNode("descendant-or-self::select1[@ref='cPaymentMethod']");
                                    if (oSelectElmt is null)
                                    {
                                        oSelectElmt = oOptXform.addSelect1(ref oGrpElmt, "cPaymentMethod", false, "Payment Method", "radios multiline", Protean.xForm.ApperanceTypes.Full);
                                    }
                                    int nOptCount = oPay.getPaymentMethods(ref oOptXform, ref oSelectElmt, nAmount, ref mcPaymentMethod);

                                    // Code Moved to Get PaymentMethods

                                    if (nOptCount == 0)
                                    {
                                        oOptXform.valid = false;
                                        //XmlNode argoNode1 = oGrpElmt;
                                        oOptXform.addNote(ref oGrpElmt, Protean.xForm.noteTypes.Alert, "There is no method of payment available for your account - please contact the site administrator.");
                                        //oGrpElmt = (XmlElement)argoNode1;
                                    }
                                    else if (nOptCount == 1)
                                    {
                                        // hide the options
                                        oSelectElmt.SetAttribute("class", "hidden");

                                        // step throught the payment methods to set as allowed.
                                    }
                                    foreach (XmlElement oOptElmt in oSelectElmt.SelectNodes("item"))
                                        AllowedPaymentMethods.Add(oOptElmt.SelectSingleNode("value").InnerText);
                                }
                            }




                            else if (oPaymentCfg.SelectNodes("provider").Count == 1)
                            {
                                // or just one
                                if (!bPaymentTypeButtons)
                                {
                                    if (Convert.ToBoolean(oPay.HasRepeatPayments()))
                                    {
                                        var oSelectElmt = oOptXform.addSelect1(ref oGrpElmt, "cPaymentMethod", false, "Payment Method", "radios multiline", Protean.xForm.ApperanceTypes.Full);
                                        oPay.ReturnRepeatPayments(oPaymentCfg.SelectSingleNode("provider/@name").InnerText, ref oOptXform, ref oSelectElmt);

                                        oOptXform.addOption(ref oSelectElmt, oPaymentCfg.SelectSingleNode("provider/description").Attributes["value"].Value, oPaymentCfg.SelectSingleNode("provider").Attributes["name"].Value);
                                        bHidePayment = false;
                                        AllowedPaymentMethods.Add(oPaymentCfg.SelectSingleNode("provider/@name").InnerText);
                                    }
                                    else
                                    {
                                        bHidePayment = true;
                                        oOptXform.addInput(ref oGrpElmt, "cPaymentMethod", false, oPaymentCfg.SelectSingleNode("provider/@name").InnerText, "hidden");
                                        oOptXform.Instance.SelectSingleNode("cPaymentMethod").InnerText = oPaymentCfg.SelectSingleNode("provider/@name").InnerText;
                                        AllowedPaymentMethods.Add(oPaymentCfg.SelectSingleNode("provider/@name").InnerText);
                                    }
                                }
                            }
                            else
                            {
                                oOptXform.valid = false;
                                //XmlNode argoNode = oGrpElmt;
                                oOptXform.addNote(ref oGrpElmt, Protean.xForm.noteTypes.Alert, "There is no method of payment setup on this site - please contact the site administrator.");
                                //oGrpElmt = (XmlElement)argoNode;
                            }
                        }
                        else
                        {
                            oOptXform.valid = false;
                            //XmlNode argoNode2 = oGrpElmt;
                            oOptXform.addNote(ref oGrpElmt, Protean.xForm.noteTypes.Alert, "There is no method of payment setup on this site - please contact the site administrator.");
                            //oGrpElmt = (XmlElement)argoNode2;
                        }

                        string cTermsTitle = "Terms and Conditions";

                        // Adjust the group title
                        if (bAdjustTitle)
                        {
                            string cGroupTitle = "Select Delivery and Payment Option";
                            if (bHideDelivery & bHidePayment)
                                cGroupTitle = "Terms and Conditions";
                            if (bHideDelivery & !bHidePayment)
                                cGroupTitle = "Select Payment Option";
                            if (!bHideDelivery & bHidePayment)
                                cGroupTitle = "Select Shipping Option";
                            XmlElement labelElmt = (XmlElement)oGrpElmt.SelectSingleNode("label");
                            labelElmt.InnerText = cGroupTitle;
                            labelElmt.SetAttribute("class", "term3019");

                            // Just so we don't show the terms and conditions title twice

                            if (cGroupTitle == "Terms and Conditions")
                            {
                                cTermsTitle = "";
                            }
                        }

                        if (bAddTerms)
                        {

                            if (oGrpElmt.SelectSingleNode("*[@ref='terms']") is null)
                            {
                                string argsClass1 = "readonly terms-and-conditons";
                                int argnRows1 = 0;
                                int argnCols1 = 0;
                                oOptXform.addTextArea(ref oGrpElmt, "terms", false, cTermsTitle, ref argsClass1, nRows: ref argnRows1, nCols: ref argnCols1);
                            }

                            if (oGrpElmt.SelectSingleNode("*[@ref='confirmterms']") is null)
                            {
                                oOptXform.addSelect(ref oGrpElmt, "confirmterms", false, "&#160;", "", Protean.xForm.ApperanceTypes.Full);
                                XmlElement argoSelectNode2 = (XmlElement)oGrpElmt.LastChild;
                                oOptXform.addOption(ref argoSelectNode2, "I agree to the Terms and Conditions", "Agree");
                            }

                            if (Convert.ToInt16("0" + moCartConfig["TermsContentId"]) > 0)
                            {
                                var termsElmt = new XmlDocument();
                                termsElmt.LoadXml(moDBHelper.getContentBrief(Convert.ToInt16(moCartConfig["TermsContentId"])));
                                mcTermsAndConditions = termsElmt.DocumentElement.InnerXml;
                            }
                            else
                            {
                                mcTermsAndConditions = moCartConfig["TermsAndConditions"];
                            }

                            if (mcTermsAndConditions is null)
                                mcTermsAndConditions = "";

                            oOptXform.Instance.SelectSingleNode("terms").InnerXml = mcTermsAndConditions;

                        }

                        oOptXform.addSubmit(ref oGrpElmt, "optionsForm", "Make Secure Payment");

                        if (bPaymentTypeButtons)
                        {
                            XmlElement xmlXfromGroup = (XmlElement)oOptXform.moXformElmt.SelectSingleNode("group");

                            // added by TS, if you need just the product amount without VAT or shipping we need to talk.
                            double totalAmount = Convert.ToDouble(cartElmt.GetAttribute("total"));
                            oPay.getPaymentMethodButtons(ref oOptXform, ref xmlXfromGroup, totalAmount);

                            foreach (XmlElement oSubmitBtn in oOptXform.moXformElmt.SelectNodes("descendant-or-self::submit"))
                                AllowedPaymentMethods.Add(oSubmitBtn.GetAttribute("value"));

                            if (nAmount == 0d & nRepeatAmount == 0d)
                            {
                                // oOptXform.addSubmit(oGrpElmt, "optionsForm", "Complete Order")
                                AllowedPaymentMethods.Add("No Charge");
                                oOptXform.addSubmit(ref oGrpElmt, "No Charge", "Complete Order", "submit", "pay-button pay-nothing", "fas fa-check", "No Charge");

                            }

                        }
                    }

                    oOptXform.valid = false;

                    string submittedPaymentMethod = myWeb.moRequest["submit"];
                    if (submittedPaymentMethod == "Make Secure Payment")
                    {
                        submittedPaymentMethod = myWeb.moRequest["cPaymentMethod"];
                    }

                    if (AllowedPaymentMethods.Contains(submittedPaymentMethod)) // equates to is submitted
                    {

                        // Save notes to cart

                        if ((moCartConfig["NotesOnOptions"]).ToLower() == "on")
                        {
                            // If myWeb.moRequest("tblCartOrder/cClientNotes/Notes/Notes") <> "" Then
                            this.AddClientNotes(myWeb.moRequest["tblCartOrder/cClientNotes/Notes/Notes"]);
                            // End If
                        }

                        if (myWeb.moRequest["confirmterms"] == "Agree" | !bAddTerms)
                        {

                            mcPaymentMethod = submittedPaymentMethod;

                            // if we have a profile split it out, allows for more than one set of settings for each payment method, only done for SecPay right now.
                            if (mcPaymentMethod?.Contains("-") == true)
                            {
                                string[] aPayMth = mcPaymentMethod.Split('-');
                                mcPaymentMethod = aPayMth[0];
                                mcPaymentProfile = aPayMth[1];
                            }

                            sSql2 = "select * from tblCartOrder where nCartOrderKey = " + mnCartId;
                            ods2 = moDBHelper.GetDataSet(sSql2, "Order", "Cart");
                            string cSqlUpdate;
                            foreach (DataRow oRow2 in ods2.Tables["Order"].Rows)
                            {
                                long nShipOptKey;

                                if (myWeb.moRequest["nShipOptKey"] != null)
                                {
                                    oRow2["nShippingMethodId"] = myWeb.moRequest["nShipOptKey"];
                                }
                                nShipOptKey = Convert.ToInt64(oRow2["nShippingMethodId"]);
                                sSql = "select * from tblCartShippingMethods ";
                                sSql = sSql + " where nShipOptKey = " + nShipOptKey;
                                ods = moDBHelper.GetDataSet(sSql, "Order", "Cart");

                                foreach (DataRow currentORow5 in ods.Tables["Order"].Rows)
                                {
                                    oRow = currentORow5;
                                    cShippingDesc = oRow["cShipOptName"].ToString() + "-" + oRow["cShipOptCarrier"].ToString();

                                    nShippingCost = 0;
                                    if (oRow["nShipOptCost"] != DBNull.Value)
                                    {
                                        double.TryParse(oRow["nShipOptCost"].ToString(), out nShippingCost);
                                    }

                                    cSqlUpdate = $@"UPDATE tblCartOrder  SET cShippingDesc = '{SqlFmt(cShippingDesc)}', nShippingCost = {SqlFmt(nShippingCost.ToString())}, nShippingMethodId = {nShipOptKey} WHERE nCartOrderKey = {mnCartId}";
                                    moDBHelper.ExeProcessSql(cSqlUpdate);
                                }

                                // update the cart xml

                                updateTotals(ref cartElmt, nAmount, nShippingCost, nShipOptKey.ToString());

                                ods2 = null;

                                if (bForceValidation)
                                {
                                    oOptXform.updateInstanceFromRequest();
                                    oOptXform.validate();
                                }
                                else
                                {
                                    oOptXform.valid = true;
                                }
                            }
                        }
                        else
                        {
                            oOptXform.addNote("confirmterms", Protean.xForm.noteTypes.Alert, "You must agree to the terms and conditions to proceed");
                        }
                    }

                    if (oOptXform.valid)
                    {
                        // If we have any order notes we save them
                        if (oOptXform.Instance.SelectSingleNode("Notes") != null)
                        {
                            // Open database for reading and writing
                            sSql = "select * from tblCartOrder where nCartOrderKey=" + mnCartId;
                            ods = moDBHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                            foreach (DataRow currentORow6 in ods.Tables["Order"].Rows)
                            {
                                oRow = currentORow6;
                                oRow["cClientNotes"] = oOptXform.Instance.SelectSingleNode("Notes").OuterXml;
                                moDBHelper.updateDataset(ref ods, "Order", true);
                            }
                            ods.Clear();
                            ods = null;
                        }
                    }
                    oOptXform.addValues();

                    return oOptXform;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "optionsXform", ex, "", cProcessInfo, gbDebug);
                    return (Cms.xForm)null;
                }

            }


            public void AddShippingCosts(ref XmlElement xmlProduct, string nPrice, string nWeight)
            {
                try
                {
                    //long nQuantity = 1L;
                    xmlProduct.SetAttribute("test1", "1");
                    var xmlShippingOptions = makeShippingOptionsXML();

                    // 'step through oShipping Options and add to oElmt those options 
                    // 'that are valid for our price and weight.
                    var xmlShippingOptionsValid = moPageXml.CreateElement("ShippingOptions");

                    var strXpath = new System.Text.StringBuilder();
                    strXpath.Append("Method[ ");
                    strXpath.Append("(WeightMin=0 or WeightMin<=" + nWeight.ToString() + ") ");
                    strXpath.Append(" and ");
                    strXpath.Append("(WeightMax=0 or WeightMax>=" + nWeight.ToString() + ") ");
                    strXpath.Append(" and ");
                    strXpath.Append("(PriceMin=0 or PriceMin<=" + nPrice.ToString() + ") ");
                    strXpath.Append(" and ");
                    strXpath.Append("(PriceMax=0 or PriceMax>=" + nPrice.ToString() + ") ");
                    strXpath.Append(" ]");


                    // add filtered list to xmlShippingOptionsValid
                    XmlElement xmlMethod;
                    foreach (XmlElement currentXmlMethod in xmlShippingOptions.SelectNodes(strXpath.ToString()))
                    {
                        xmlMethod = currentXmlMethod;
                        // add to 
                        xmlShippingOptionsValid.AppendChild(xmlMethod.CloneNode(true));
                    }

                    // itterate though xmlShippingOptionsValid and get cheapest for each Location
                    string cShippingLocation = "";
                    string cShippingLocationPrev = "";

                    foreach (XmlElement currentXmlMethod1 in xmlShippingOptionsValid.SelectNodes("Method"))
                    {
                        xmlMethod = currentXmlMethod1;
                        try
                        {
                            cShippingLocation = xmlMethod.SelectSingleNode("Location").InnerText;
                            if (!string.IsNullOrEmpty(cShippingLocationPrev) & (cShippingLocationPrev ?? "") == (cShippingLocation ?? ""))
                            {
                                xmlShippingOptionsValid.RemoveChild(xmlMethod);
                            }

                            cShippingLocationPrev = cShippingLocation; // set cShippingLocationPrev for next loop
                        }
                        catch (Exception ex)
                        {
                            //xmlMethod = xmlMethod;
                            //xmlProduct = xmlProduct;
                            stdTools.returnException(ref myWeb.msException, mcModuleName, "AddShippingCosts", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                        }

                    }

                    // add to product XML
                    xmlProduct.AppendChild(xmlShippingOptionsValid.CloneNode(true));
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "AddShippingCosts", ex, vstrFurtherInfo: "", bDebug: gbDebug);

                }



            }

            public DataSet getValidShippingOptionsDS(string cDestinationCountry, double nAmount, long nQuantity, double nWeight)
            {
                try
                {
                    var dsShippingOption = getValidShippingOptionsDS(cDestinationCountry, nAmount, nQuantity, nWeight, string.Empty, 0);
                    return dsShippingOption;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "getValidShippingOptionsDS", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }
            }
            public DataSet getValidShippingOptionsDS(string cDestinationCountry, double nAmount, long nQuantity, double nWeight, long ProductId)
            {
                try
                {
                    var dsShippingOption = getValidShippingOptionsDS(cDestinationCountry, nAmount, nQuantity, nWeight, string.Empty, ProductId);
                    return dsShippingOption;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "getValidShippingOptionsDS", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }
            }
            public DataSet getValidShippingOptionsDS(string cDestinationCountry, double nAmount, long nQuantity, double nWeight, string cPromoCode)
            {
                try
                {
                    var dsShippingOption = getValidShippingOptionsDS(cDestinationCountry, nAmount, nQuantity, nWeight, cPromoCode, 0);
                    return dsShippingOption;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "getValidShippingOptionsDS", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }
            }

            public DataSet getValidShippingOptionsDS(string cDestinationCountry, double nAmount, long nQuantity, double nWeight, string cPromoCode, long ProductId)
            {
                try
                {
                    var dsShippingOption = getValidShippingOptionsDS(cDestinationCountry, "", nAmount, nQuantity, nWeight, cPromoCode, ProductId);
                    return dsShippingOption;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "getValidShippingOptionsDS", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }
            }
            public DataSet getValidShippingOptionsDS(string cDestinationCountry, string cDestinationPostalCode, double nAmount, long nQuantity, double nWeight, string cPromoCode, long ProductId)
            {

                try
                {
                    long userId = 0;
                    if (myWeb.moSession != null)
                    {
                        if (Convert.ToInt32(myWeb.moSession["nUserId"]) == 0)
                        {
                            userId = Convert.ToInt16(myWeb.moSession["nUserId"]);
                        }
                        else
                        {
                            userId = myWeb.mnUserId;
                        }
                    }
                    else
                    {
                        userId = myWeb.mnUserId;

                    }
                    int argnIndex = 1;
                    string sCountryList = "";
                    // Add code for checking shipping group is included/Excluded for delievry methods
                    var PublishExpireDate = DateTime.Now;
                    if (moCartConfig["ShippingPostcodes"] == "on" && cDestinationPostalCode != "")
                    {
                        try
                        {
                            string PostcodePrefix = System.Text.RegularExpressions.Regex.Split(cDestinationPostalCode, "(?m)^([A-Z0-9]{2,4})(?:\\s*[A-Z0-9]{3})?$")[1];
                            sCountryList = getParentCountries(ref PostcodePrefix, ref argnIndex);
                        }
                        catch
                        {
                            sCountryList = "";
                        }

                    }

                    if (sCountryList == "")
                    {
                        sCountryList = getParentCountries(ref cDestinationCountry, ref argnIndex);
                    }

                    DataSet oDS;
                    if (myWeb.moDbHelper.checkDBObjectExists("spGetValidShippingOptions", Tools.Database.objectTypes.StoredProcedure))
                    {
                        // ' call stored procedure else existing code.
                        // ' Passing parameter: nCartId

                        var param = new Hashtable();
                        param.Add("CartOrderId", mnCartId);
                        param.Add("Amount", nAmount);
                        param.Add("Quantity", nQuantity);
                        param.Add("Weight", nWeight);
                        param.Add("Currency", mcCurrency);
                        param.Add("userId", userId);
                        param.Add("AuthUsers", (object)Cms.gnAuthUsers);
                        param.Add("NonAuthUsers", (object)Cms.gnNonAuthUsers);
                        param.Add("CountryList", sCountryList);
                        param.Add("dValidDate", PublishExpireDate);
                        param.Add("PromoCode", cPromoCode);
                        param.Add("ProductId", ProductId);
                        oDS = moDBHelper.GetDataSet("spGetValidShippingOptions", "Option", "Shipping", false, param, CommandType.StoredProcedure);
                    }
                    // End If
                    else
                    {

                        string sSql;

                        sSql = "select opt.*, dbo.fxn_shippingTotal(opt.nShipOptKey," + nAmount + "," + nQuantity + "," + nWeight + ") as nShippingTotal  from tblCartShippingLocations Loc ";
                        sSql = sSql + "Inner Join tblCartShippingRelations rel ON Loc.nLocationKey = rel.nShpLocId ";
                        sSql = sSql + "Inner Join tblCartShippingMethods opt ON rel.nShpOptId = opt.nShipOptKey ";
                        sSql += "INNER JOIN tblAudit ON opt.nAuditId = tblAudit.nAuditKey";

                        sSql = sSql + " WHERE (nShipOptQuantMin <= 0 or nShipOptQuantMin <= " + nQuantity + ") and (nShipOptQuantMax <= 0 or nShipOptQuantMax >= " + nQuantity + ") and ";
                        sSql = sSql + "(nShipOptPriceMin <= 0 or nShipOptPriceMin <= " + nAmount + ") and (nShipOptPriceMax <= 0 or nShipOptPriceMax >= " + nAmount + ") and ";
                        sSql = sSql + "(nShipOptWeightMin <= 0 or nShipOptWeightMin <= " + nWeight + ") and (nShipOptWeightMax <= 0 or nShipOptWeightMax >= " + nWeight + ") ";

                        sSql += " and ((opt.cCurrency Is Null) or (opt.cCurrency = '') or (opt.cCurrency = '" + mcCurrency + "'))";
                        // If myWeb.mnUserId > 0 Then
                        // ' if user in group then return it
                        // sSql &= " and ((SELECT COUNT(perm.nCartShippingPermissionKey) from tblCartShippingPermission perm" &
                        // " Inner join tblDirectoryRelation PermGroup ON perm.nDirId = PermGroup.nDirParentId" &
                        // "  where perm.nShippingMethodId = opt.nShipOptKey and PermGroup.nDirChildId = " & myWeb.mnUserId & " and perm.nPermLevel = 1) > 0"
                        // sSql &= " and not((SELECT COUNT(perm.nCartShippingPermissionKey) from tblCartShippingPermission perm" &
                        // " Inner join tblDirectoryRelation PermGroup ON perm.nDirId = PermGroup.nDirParentId" &
                        // "  where perm.nShippingMethodId = opt.nShipOptKey and PermGroup.nDirChildId = " & myWeb.mnUserId & " and perm.nPermLevel = 0) > 0)"
                        if (userId > 0)
                        {
                            // if user in group then return it
                            sSql += " and ((SELECT COUNT(perm.nCartShippingPermissionKey) from tblCartShippingPermission perm" + " Inner join tblDirectoryRelation PermGroup ON perm.nDirId = PermGroup.nDirParentId" + "  where perm.nShippingMethodId = opt.nShipOptKey and PermGroup.nDirChildId = " + userId + " and perm.nPermLevel = 1) > 0";
                            sSql += " and not((SELECT COUNT(perm.nCartShippingPermissionKey) from tblCartShippingPermission perm" + " Inner join tblDirectoryRelation PermGroup ON perm.nDirId = PermGroup.nDirParentId" + "  where perm.nShippingMethodId = opt.nShipOptKey and PermGroup.nDirChildId = " + userId + " and perm.nPermLevel = 0) > 0)";
                            // method allowed for authenticated or imporsonating CS users.
                            string shippingGroupCondition;

                            shippingGroupCondition = "perm.nDirId = " + Cms.gnAuthUsers;

                            sSql += " Or (SELECT COUNT(perm.nCartShippingPermissionKey) from tblCartShippingPermission perm" + "  where perm.nShippingMethodId = opt.nShipOptKey And " + shippingGroupCondition + " And perm.nPermLevel = 1) > 0";

                            // if no group exists return it.
                            sSql += " or (SELECT COUNT(*) from tblCartShippingPermission perm where opt.nShipOptKey = perm.nShippingMethodId and perm.nPermLevel = 1) = 0)";

                            sSql += @" And opt.nShipOptKey not in ( select nShippingMethodId
                                from tblCartShippingPermission perm 
                                Inner join tblDirectoryRelation PermGroup ON perm.nDirId = PermGroup.nDirParentId  
                                 and  nPermLevel = 0  and PermGroup.nDirChildId =" + userId + ")";
                        }

                        else
                        {
                            long nonAuthID = (long)Cms.gnNonAuthUsers;
                            long AuthID = (long)Cms.gnAuthUsers;
                            // method allowed for non-authenticated
                            sSql += " and ((SELECT COUNT(perm.nCartShippingPermissionKey) from tblCartShippingPermission perm" + "  where perm.nShippingMethodId = opt.nShipOptKey and perm.nDirId = " + Cms.gnNonAuthUsers + "  and perm.nPermLevel = 1) > 0";
                            // method has no group 
                            sSql += " or (SELECT COUNT(*) from tblCartShippingPermission perm where opt.nShipOptKey = perm.nShippingMethodId and perm.nPermLevel = 1) = 0)";

                        }
                        // Restrict the shipping options by looking at the delivery country currently selected.  
                        // Of course, if we are hiding the delivery address then this can be ignored.

                        if (!string.IsNullOrEmpty(sCountryList))
                        {
                            sSql = sSql + " and ((loc.cLocationNameShort IN " + sCountryList + ") or (loc.cLocationNameFull IN " + sCountryList + ")) ";
                        }

                        // Active methods

                        sSql += " AND (tblAudit.nStatus >0)";
                        sSql += " AND ((tblAudit.dPublishDate = 0) or (tblAudit.dPublishDate Is Null) or (tblAudit.dPublishDate <= " + Tools.Database.SqlDate(DateTime.Now) + "))";
                        sSql += " AND ((tblAudit.dExpireDate = 0) or (tblAudit.dExpireDate Is Null) or (tblAudit.dExpireDate >= " + Tools.Database.SqlDate(DateTime.Now) + "))";
                        // Build Form

                        // Go and collect the valid shipping options available for this order
                        oDS = moDBHelper.GetDataSet(sSql + " order by opt.nDisplayPriority, nShippingTotal", "Option", "Shipping");
                    }

                    //// fix for bOverrideForWholeOrder mot required as SP now does this.
                    //if (oDS.Tables["Option"].Columns["bOverrideForWholeOrder"] != null)
                    //{
                    //    bool hasOverideForWholeOrder = false;
                    //    foreach (DataRow oRow in oDS.Tables["Option"].Rows)
                    //    {
                    //        if (Convert.ToInt16(oRow["bOverrideForWholeOrder"]) == 1) {
                    //            hasOverideForWholeOrder = true;
                    //        }
                    //    }
                    //    if (hasOverideForWholeOrder) {
                    //        foreach (DataRow oRow in oDS.Tables["Option"].Rows)
                    //        {
                    //            if (Convert.ToInt16(oRow["bOverrideForWholeOrder"]) != 1)
                    //            {
                    //                oRow.Delete();
                    //            }
                    //        }
                    //    }
                    //}

                    if (oDS.Tables["Option"].Columns["cLocationNameShort"] != null)
                    {
                        string overiddenLocations = "";
                        foreach (DataRow oRow in oDS.Tables["Option"].Rows)
                        {

                            // Calculate any shipping cost overage
                            double nShippingCost = Math.Round(Convert.IsDBNull(oRow["nShippingTotal"]) ? 0 : Convert.ToDouble(oRow["nShippingTotal"]), 2);
                            double overageUnit = Convert.IsDBNull(oRow["nShipOptWeightOverageUnit"]) ? 0 : Convert.ToDouble(oRow["nShipOptWeightOverageUnit"]);
                            double overageRate = Convert.IsDBNull(oRow["nShipOptWeightOverageRate"]) ? 0 : Convert.ToDouble(oRow["nShipOptWeightOverageRate"]);
                            double overageWeightMax = Convert.IsDBNull(oRow["nShipOptWeightMax"]) ? 0 : Convert.ToDouble(oRow["nShipOptWeightMax"]);

                            nShippingCost = calcShippingCost(nShippingCost, overageUnit, overageRate, nWeight, overageWeightMax);

                            oRow["nShipOptCost"] = nShippingCost;

                            // TODO delete any parent relations /  or remove if allready have child
                            string delLocation = oRow["cLocationNameShort"].ToString();
                            if (overiddenLocations.Contains("'" + delLocation + "'") == false && sCountryList != "")
                            {
                                Int32 startPos = sCountryList.IndexOf(delLocation) + delLocation.Length + 1;
                                Int32 endPos = sCountryList.Length - sCountryList.IndexOf(delLocation) - delLocation.Length - 1;
                                overiddenLocations = sCountryList.Substring(startPos, endPos);
                            }
                        }
                        if (overiddenLocations != "")
                        {
                            foreach (DataRow oRow in oDS.Tables["Option"].Rows)
                            {
                                string delLocation = oRow["cLocationNameShort"].ToString();
                                if (overiddenLocations.Contains("'" + delLocation + "'"))
                                {
                                    oRow.Delete();
                                }
                            }
                        }
                    }

                    oDS.AcceptChanges();
                    return oDS;


                }

                catch (Exception ex)
                {

                    stdTools.returnException(ref myWeb.msException, mcModuleName, "getValidShippingOptionsDS", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }

            }

            private double calcShippingCost(double baseCost, double overageUnit, double overageRate, double nWeight, double nWeightMax)
            {
                try
                {
                    double nShippingCost = baseCost;
                    nShippingCost = Math.Round(nShippingCost, 2);

                    if (overageUnit > 0)
                    {
                        double multiplier = 0;
                        if (nWeight > nWeightMax)
                        {
                            multiplier = Math.Ceiling(nWeight - nWeightMax);
                        }
                        nShippingCost = nShippingCost + ((multiplier / overageUnit) * overageRate);
                    }

                    return nShippingCost;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "calcShippingCost", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return 0;
                }
            }

            private XmlElement makeShippingOptionsXML()
            {

                try
                {


                    if (oShippingOptions is null)
                    {

                        XmlElement xmlTemp;

                        // create XML of all possible shipping methods and add it to the page XML
                        oShippingOptions = moPageXml.CreateElement("oShippingOptions");


                        // get all the shipping options for a given shipping weight and price
                        var strSql = new System.Text.StringBuilder();
                        strSql.Append("SELECT opt.cShipOptCarrier as Carrier, opt.cShipOptTime AS ShippingTime, ");
                        strSql.Append("opt.nShipOptCost AS Cost, ");
                        strSql.Append("tblCartShippingLocations.cLocationNameShort as Location, tblCartShippingLocations.cLocationISOa2 as LocationISOa2, ");
                        strSql.Append("opt.nShipOptWeightMin AS WeightMin, opt.nShipOptWeightMax AS WeightMax,  ");
                        strSql.Append("opt.nShipOptPriceMin AS PriceMin, opt.nShipOptPriceMax AS PriceMax,  ");
                        strSql.Append("opt.nShipOptQuantMin AS QuantMin, opt.nShipOptQuantMax AS QuantMax, ");
                        strSql.Append("tblCartShippingLocations.nLocationType, tblCartShippingLocations.cLocationNameFull, opt.cShipOptName,opt.nShipOptKey ");
                        strSql.Append("FROM tblCartShippingLocations ");
                        strSql.Append("INNER JOIN tblCartShippingRelations ON tblCartShippingLocations.nLocationKey = tblCartShippingRelations.nShpLocId ");
                        strSql.Append("RIGHT OUTER JOIN tblCartShippingMethods AS opt ");
                        strSql.Append("INNER JOIN tblAudit ON opt.nAuditId = tblAudit.nAuditKey ON tblCartShippingRelations.nShpOptId = opt.nShipOptKey ");

                        strSql.Append("WHERE (tblAudit.nStatus > 0) ");
                        strSql.Append("AND (tblAudit.dPublishDate = 0 OR tblAudit.dPublishDate IS NULL OR tblAudit.dPublishDate <= " + Tools.Database.SqlDate(DateTime.Now) + ") ");
                        strSql.Append("AND (tblAudit.dExpireDate = 0 OR tblAudit.dExpireDate IS NULL OR tblAudit.dExpireDate >= " + Tools.Database.SqlDate(DateTime.Now) + ") ");
                        strSql.Append("AND (tblCartShippingLocations.cLocationNameShort IS NOT NULL) ");
                        strSql.Append("ORDER BY tblCartShippingLocations.cLocationNameShort, opt.nShipOptCost ");



                        var oDs = moDBHelper.GetDataSet(strSql.ToString(), "Method", "ShippingMethods");
                        oShippingOptions.InnerXml = oDs.GetXml();

                        // move all the shipping methods up a level
                        foreach (XmlElement currentXmlTemp in oShippingOptions.SelectNodes("ShippingMethods/Method"))
                        {
                            xmlTemp = currentXmlTemp;
                            oShippingOptions.AppendChild(xmlTemp);
                        }

                        foreach (XmlElement currentXmlTemp1 in oShippingOptions.SelectNodes("ShippingMethods"))
                        {
                            xmlTemp = currentXmlTemp1;
                            oShippingOptions.RemoveChild(xmlTemp);
                        }

                    }

                    return oShippingOptions;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "makeShippingOptionsXML", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }

            }

            public string updatePackagingForFreeGiftDiscount(string nCartItemKey, decimal AmountToDiscount)
            {
                try
                {
                    string cSqlUpdate;
                    // cSqlUpdate = " update tblCartItem set nPrice=0.00, nDiscountValue=" & AmountToDiscount & ", cItemName =  '" & moConfig("GiftPack") & "' where  nitemid=0 and nParentid = " & nCartItemKey
                    cSqlUpdate = " update tblCartItem set nPrice=" + AmountToDiscount + ", nDiscountValue=" + AmountToDiscount + ", cItemName =  '" + moConfig["GiftPack"] + "' where  nitemid=0 and nParentid = " + nCartItemKey;
                    moDBHelper.ExeProcessSql(cSqlUpdate);
                    return null;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "updatePackagingForFreeGiftDiscount", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }
            }

            private string updatePackagingForRemovingFreeGiftDiscount(string nCartOrderId, decimal AmountToDiscount)
            {
                try
                {
                    string cSqlUpdate;
                    cSqlUpdate = " update tblCartItem set nDiscountValue=" + AmountToDiscount + " where  nitemid=0 and nCartOrderId = " + nCartOrderId;
                    moDBHelper.ExeProcessSql(cSqlUpdate);
                    return null;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "updatePackagingForFreeGiftDiscount", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }
            }

            public string updateGCgetValidShippingOptionsDS(string nShipOptKey)
            {
                try
                {
                    //tidy up this logic
                    // Dim ods As DataSet
                    // Dim oRow As DataRow
                    string sSql;
                    string cShippingDesc;
                    string nShippingCost;
                    string cSqlUpdate;

                    sSql = "select * from tblCartShippingMethods ";
                    sSql = sSql + " where nShipOptKey in ( " + nShipOptKey + ")";
                    // ods = moDBHelper.GetDataSet(sSql, "Order", "Cart")

                    // Check if shipping option contains multiple option then get lowest 
                    if (nShipOptKey.Contains(","))
                    {
                        nShipOptKey = nShipOptKey.Split(',')[0];
                    }
                    // For Each oRow In ods.Tables("Order").Rows

                    // cShippingDesc = oRow("cShipOptName") & "-" & oRow("cShipOptCarrier")
                    // nShippingCost = oRow("nShipOptCost")
                    // cSqlUpdate = "UPDATE tblCartOrder Set cShippingDesc='" & SqlFmt(cShippingDesc) & "', nShippingCost=" & SqlFmt(nShippingCost) & ", nShippingMethodId = " & nShipOptKey & " WHERE nCartOrderKey=" & mnCartId
                    // moDBHelper.ExeProcessSql(cSqlUpdate)
                    // Next

                    using (var oDr = myWeb.moDbHelper.getDataReaderDisposable(sSql))
                    {
                        if (oDr != null)
                        {
                            while (oDr.Read())
                            {
                                cShippingDesc = oDr["cShipOptName"] + "-" + oDr["cShipOptCarrier"];
                                nShippingCost = oDr["nShipOptCost"].ToString();

                                cSqlUpdate = "UPDATE tblCartOrder Set cShippingDesc='" + SqlFmt(cShippingDesc) +
                                             "', nShippingCost=" + SqlFmt(nShippingCost) +
                                             ", nShippingMethodId=" + nShipOptKey +
                                             " WHERE nCartOrderKey=" + mnCartId;

                                moDBHelper.ExeProcessSql(cSqlUpdate);
                            }
                        }
                    }
                    return null;
                }
                catch (Exception ex)
                {

                    stdTools.returnException(ref myWeb.msException, mcModuleName, "updateGCgetValidShippingOptionsDS", ex, vstrFurtherInfo: "", bDebug: gbDebug);
                    return null;
                }
            }



        }
    }
}
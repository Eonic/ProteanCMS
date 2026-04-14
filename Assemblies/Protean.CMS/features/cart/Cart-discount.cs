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

            public virtual string discountsProcess(XmlElement oElmt)
            {
                string sCartCmd = mcCartCmd;
                string cProcessInfo = "";
                bool bAlwaysAskForDiscountCode = moCartConfig["AlwaysAskForDiscountCode"]?.ToString().ToLower() == "on";
                bool bSkipDiscountCode = moCartConfig["SkipDiscountCode"]?.ToString().ToLower() == "on";

                try
                {

                    myWeb.moSession["cLogonCmd"] = "";
                    GetCart(ref oElmt);
                    if (bSkipDiscountCode)
                    {
                        oElmt.RemoveAll();
                        sCartCmd = "RedirectSecure";
                    }
                    else if (moDiscount.bHasPromotionalDiscounts | bAlwaysAskForDiscountCode)
                    {
                        var oDiscountsXform = this.discountsXform("discountsForm", "?pgid=" + myWeb.mnPageId + "&cartCmd=Discounts");
                        if (oDiscountsXform.valid == false)
                        {
                            moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oDiscountsXform.moXformElmt);
                        }

                        else
                        {
                            oElmt.RemoveAll();
                            sCartCmd = "RedirectSecure";
                        }
                    }
                    else
                    {
                        oElmt.RemoveAll();
                        sCartCmd = "RedirectSecure";
                    }


                    // if this returns Notes then we display for otherwise we goto processflow
                    return sCartCmd;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "discountsProcess", ex, "", cProcessInfo, gbDebug);
                    return "";
                }

            }


            public virtual Cms.xForm discountsXform(string formName = "notesForm", string action = "?cartCmd=Discounts")
            {
                myWeb.PerfMon.Log("Cart", "discountsXform");
                // this function is called for the collection from a form and addition to the database
                // of address information.

                DataSet oDs;
                string sSql;
                XmlElement oFormGrp;
                string sXmlContent;
                XmlElement promocodeElement = null;
                bool usedPromocodeFromExternalRef = false;

                string cProcessInfo = "";
                try
                {
                    // Get notes XML
                    var oXform = new Cms.xForm(ref myWeb.msException);
                    oXform.moPageXML = moPageXml;
                    // oXform.NewFrm(formName)
                    string cDiscountsXform = moCartConfig["DiscountsXform"];

                    if (!string.IsNullOrEmpty(cDiscountsXform))
                    {
                        if (!oXform.load(cDiscountsXform))
                        {
                            oXform.NewFrm(formName);
                            oFormGrp = oXform.addGroup(ref oXform.moXformElmt, "discounts", sLabel: "Missing File: " + mcNotesXForm);
                        }
                        else
                        {
                            // add missing submission or submit buttons
                            if (oXform.moXformElmt.SelectSingleNode("model/submission") is null)
                            {
                                // If oXform.moXformElmt.SelectSingleNode("model/instance/submission") Is Nothing Then
                                oXform.submission(formName, action, "POST", "return form_check(this);");
                            }
                            if (oXform.moXformElmt.SelectSingleNode("descendant-or-self::submit") is null)
                            {
                                oXform.addSubmit(ref oXform.moXformElmt, "Submit", "Continue");
                            }

                            XmlElement oSubmit = (XmlElement)oXform.moXformElmt.SelectSingleNode("descendant-or-self::submit");
                            if (oSubmit != null)
                            {
                                oFormGrp = (XmlElement)oSubmit.ParentNode;
                            }
                            else
                            {
                                oFormGrp = oXform.addGroup(ref oXform.moXformElmt, "Promo", sLabel: "Enter Promotional Code");
                            }
                            if (oXform.Instance.SelectSingleNode("descendant-or-self::PromotionalCode") is null)
                            {
                                if (oXform.Instance.FirstChild.SelectSingleNode("Notes") is null)
                                {
                                    oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"));
                                }
                                promocodeElement = (XmlElement)oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"));
                                oXform.addInput(ref oFormGrp, "Notes/PromotionalCode", false, "Promotional Code", "");
                            }

                        }
                    }
                    else
                    {
                        oXform.NewFrm(formName);
                        oXform.submission(formName, action, "POST", "return form_check(this);");
                        oXform.Instance.InnerXml = "<Notes/>";
                        oFormGrp = oXform.addGroup(ref oXform.moXformElmt, "notes", sLabel: "");

                        if (oXform.Instance.FirstChild.SelectSingleNode("Notes") is null)
                        {
                            oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"));
                        }

                        promocodeElement = (XmlElement)oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"));
                        oXform.addInput(ref oFormGrp, "Notes/PromotionalCode", false, "Promotional Code", "");
                        oXform.addSubmit(ref oFormGrp, "Submit", "Continue");

                    }
                    // Open database for reading and writing


                    // External promo code checks
                    if (promocodeElement != null & !string.IsNullOrEmpty(promocodeFromExternalRef))
                    {

                        usedPromocodeFromExternalRef = true;
                    }

                    sSql = "select * from tblCartOrder where nCartOrderKey=" + mnCartId;
                    oDs = moDBHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                    foreach (DataRow oRow in oDs.Tables["Order"].Rows)
                    {
                        // load existing notes from Cart
                        sXmlContent = oRow["cClientNotes"] + "".ToString();
                        if (!string.IsNullOrEmpty(sXmlContent))
                        {
                            oXform.Instance.InnerXml = sXmlContent;
                        }

                        // If this xform is being submitted
                        bool isSubmitted = oXform.isSubmitted();
                        if (isSubmitted | myWeb.moRequest["Submit"] == "Continue" | myWeb.moRequest["Submit"] == "Search")
                        {
                            oXform.updateInstanceFromRequest();
                            oXform.validate();
                            if (oXform.valid == true)
                            {
                                oRow["cClientNotes"] = oXform.Instance.InnerXml;
                                mcCartCmd = "RedirectSecure";
                            }
                        }
                        else if (!isSubmitted & usedPromocodeFromExternalRef)
                        {
                            // If an external promo code is in the system then save it, even before it has been submitted
                            promocodeElement = (XmlElement)oXform.Instance.SelectSingleNode("//PromotionalCode");
                            if (promocodeElement != null)
                            {
                                promocodeElement.InnerText = promocodeFromExternalRef;
                                oRow["cClientNotes"] = oXform.Instance.InnerXml;
                                // Promo code is officially in the process, so we can ditch any transitory variables.
                                promocodeFromExternalRef = "";
                            }
                        }
                    }
                    moDBHelper.updateDataset(ref oDs, "Order", true);

                    oDs.Clear();
                    oDs = null;
                    oXform.addValues();

                    return oXform;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "discountsXform", ex, "", cProcessInfo, gbDebug);
                    return (Cms.xForm)null;
                }

            }




        }
    }
}
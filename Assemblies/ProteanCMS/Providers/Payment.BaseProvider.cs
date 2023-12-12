// ***********************************************************************
// $Library:     Protean.Providers.messaging.base
// $Revision:    3.1  
// $Date:        2010-03-02
// $Author:      Trevor Spink (trevor@eonic.co.uk)
// &Website:     www.eonic.co.uk
// &Licence:     All Rights Reserved.
// $Copyright:   Copyright (c) 2002 - 2010 Eonic Ltd.
// ***********************************************************************


using System;
using System.Reflection;
using System.Web.Configuration;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using static Protean.Cms;
using static Protean.stdTools;
using static Protean.Tools.Xml;

namespace Protean.Providers
{
    namespace Payment
    {

        public class BaseProvider
        {
            private const string mcModuleName = "Protean.Providers.Payment.BaseProvider";

            private object _AdminXforms;
            private object _AdminProcess;
            private object _Activities;

            protected XmlNode moPaymentCfg;

            public object AdminXforms
            {
                set
                {
                    _AdminXforms = value;
                }
                get
                {
                    return _AdminXforms;
                }
            }

            public object AdminProcess
            {
                set
                {
                    _AdminProcess = value;
                }
                get
                {
                    return _AdminProcess;
                }
            }

            public object Activities
            {
                set
                {
                    _Activities = value;
                }
                get
                {
                    return _Activities;
                }
            }

            public BaseProvider(ref Cms myWeb, string ProviderName)
            {
                try
                {
                    Type calledType;
                    XmlElement oProviderCfg;

                    moPaymentCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/payment");
                    oProviderCfg = (XmlElement)moPaymentCfg.SelectSingleNode("provider[@name='" + ProviderName + "']");

                    string ProviderClass = "";
                    if (oProviderCfg is not null)
                    {
                        if (oProviderCfg.HasAttribute("class"))
                        {
                            ProviderClass = oProviderCfg.GetAttribute("class");
                        }
                    }
                    else
                    {
                        // Asssume Eonic Provider
                    }

                    if (string.IsNullOrEmpty(ProviderClass))
                    {
                        ProviderClass = "Protean.Providers.Payment.DefaultProvider";
                        calledType = Type.GetType(ProviderClass, true);
                    }
                    else
                    {
                        Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection("protean/paymentProviders");
                        if (moPrvConfig.Providers[ProviderClass] is not null)
                        {
                            var assemblyInstance = Assembly.Load(moPrvConfig.Providers[ProviderClass].Type);
                            calledType = assemblyInstance.GetType("Protean.Providers.Payment." + ProviderClass, true);
                        }
                        else
                        {
                            calledType = Type.GetType("Protean.Providers.Payment." + ProviderClass, true);
                        }

                    }

                    var o = Activator.CreateInstance(calledType);

                    var args = new object[5];
                    args[0] = _AdminXforms;
                    args[1] = _AdminProcess;
                    args[2] = _Activities;
                    args[3] = this;
                    args[4] = myWeb;

                    calledType.InvokeMember("Initiate", BindingFlags.InvokeMethod, null, o, args);
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "New", ex, "", ProviderName + " Could Not be Loaded", gbDebug);
                }

            }

        }

        public class DefaultProvider
        {

            public DefaultProvider()
            {
                // do nothing
            }

            public void Initiate(ref object _AdminXforms, ref object _AdminProcess, ref object _Activities, ref object MemProvider, ref Cms myWeb)
            {

                MemProvider.AdminXforms = new AdminXForms(ref myWeb);
                MemProvider.AdminProcess = new AdminProcess(ref myWeb);
                MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms;
                MemProvider.Activities = new Activities();

            }

            public class AdminXForms : Admin.AdminXforms
            {
                private const string mcModuleName = "Providers.Providers.Eonic.AdminXForms";

                public AdminXForms(ref Cms aWeb) : base(aWeb)
                {
                }

            }

            public class AdminProcess : Admin
            {

                private AdminXForms _oAdXfm;

                public object oAdXfm
                {
                    set
                    {
                        _oAdXfm = (AdminXForms)value;
                    }
                    get
                    {
                        return _oAdXfm;
                    }
                }

                public AdminProcess(ref Cms aWeb) : base(aWeb)
                {
                }
            }


            public class Activities
            {
                private const string mcModuleName = "Providers.Payment.Eonic.Activities";

                public Cms myWeb;

                public Activities()
                {

                }

                public Protean.xForm GetPaymentForm(ref Cms myWeb, ref Global.Protean.Cms.Cart oCart, ref XmlElement oOrder, string returnCmd = "cartCmd=SubmitPaymentDetails")
                {

                    string cBillingAddress;
                    System.Collections.Specialized.NameValueCollection moCartConfig = oCart.moCartConfig;
                    string mcPaymentMethod = oCart.mcPaymentMethod;
                    var oEwProv = new PaymentProviders(oCart.myWeb);
                    string cProcessInfo = "";
                    string cRepeatPaymentError = "";

                    try
                    {

                        oEwProv.mcCurrency = Interaction.IIf(oCart.mcCurrencyCode == "", oCart.mcCurrency, oCart.mcCurrencyCode);
                        oEwProv.mcCurrencySymbol = oCart.mcCurrencySymbol;
                        if (string.IsNullOrEmpty(oOrder.GetAttribute("payableType")))
                        {
                            oEwProv.mnPaymentAmount = oOrder.GetAttribute("total");
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(oOrder.GetAttribute("payableAmount")))
                            {
                                oEwProv.mnPaymentAmount = oOrder.GetAttribute("total");
                            }
                            else
                            {
                                oEwProv.mnPaymentAmount = oOrder.GetAttribute("payableAmount");
                                oEwProv.mnPaymentMaxAmount = oOrder.GetAttribute("total");
                            }
                            oEwProv.mcPaymentType = oOrder.GetAttribute("payableType");
                        }
                        oEwProv.mnCartId = oCart.mnCartId;
                        oEwProv.mcPaymentOrderDescription = "Ref:" + oCart.OrderNoPrefix + oCart.mnCartId + " An online purchase from: " + oCart.mcSiteURL + " on " + niceDate(DateTime.Now) + " " + DateAndTime.TimeValue(Conversions.ToString(DateTime.Now));

                        if (oOrder.SelectSingleNode("Contact[@type='Billing Address']/GivenName") is not null)
                        {
                            oEwProv.mcCardHolderName = oOrder.SelectSingleNode("Contact[@type='Billing Address']/GivenName").InnerText;
                        }

                        // Build the billing address string
                        object localgetNodeValueByType() { XmlNode argoParent = oOrder; var ret = getNodeValueByType(ref argoParent, "Contact[@type='Billing Address']/Street"); oOrder = (XmlElement)argoParent; return ret; }
                        object localgetNodeValueByType1() { XmlNode argoParent1 = oOrder; var ret = getNodeValueByType(ref argoParent1, "Contact[@type='Billing Address']/City"); oOrder = (XmlElement)argoParent1; return ret; }
                        object localgetNodeValueByType2() { XmlNode argoParent2 = oOrder; var ret = getNodeValueByType(ref argoParent2, "Contact[@type='Billing Address']/State"); oOrder = (XmlElement)argoParent2; return ret; }
                        object localgetNodeValueByType3() { XmlNode argoParent3 = oOrder; var ret = getNodeValueByType(ref argoParent3, "Contact[@type='Billing Address']/Country"); oOrder = (XmlElement)argoParent3; return ret; }

                        cBillingAddress = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(localgetNodeValueByType(), Constants.vbLf), localgetNodeValueByType1()), Constants.vbLf), localgetNodeValueByType2()), Constants.vbLf), localgetNodeValueByType3()), Constants.vbLf));

                        oEwProv.mcCardHolderAddress = cBillingAddress;
                        oEwProv.moBillingContact = oOrder.SelectSingleNode("Contact[@type='Billing Address']");

                        XmlNode argoParent = oOrder;
                        oEwProv.mcCardHolderPostcode = getNodeValueByType(ref argoParent, "Contact[@type='Billing Address']/PostalCode");
                        oOrder = (XmlElement)argoParent;

                        // check products for a fullfillment date.
                        oEwProv.mcOrderRef = oCart.OrderNoPrefix + oCart.mnCartId;
                        var dFulfillment = DateTime.Now;
                        foreach (XmlNode oNode in oOrder.SelectNodes("Item/productDetail/FulfillmentDate[node()!='']"))
                        {
                            if (Conversions.ToDate(oNode.InnerText) > dFulfillment)
                            {
                                dFulfillment = Conversions.ToDate(oNode.InnerText);
                            }
                        }

                        // check Notes for a fullfillment date
                        if (!string.IsNullOrEmpty(moCartConfig["FullfillmentDateXpath"]))
                        {
                            if (oOrder.SelectSingleNode(moCartConfig["FullfillmentDateXpath"]) is null)
                            {
                                Information.Err().Raise(1009, "invalidFullfilmentXpath", moCartConfig["FullfillmentDateXpath"] + " is invalid.");
                            }
                            else
                            {
                                dFulfillment = DateTime.Parse(oOrder.SelectSingleNode(moCartConfig["FullfillmentDateXpath"]).Value);
                            }
                        }

                        oEwProv.mdFulfillmentDate = dFulfillment;

                        var ccPaymentXform = new Protean.xForm(ref myWeb.msException);

                        switch (mcPaymentMethod ?? "")
                        {
                            // Case "ePDQ"
                            // ccPaymentXform = oEwProv.PayePDQ(oOrder, oCart.mcPagePath & "cartCmd=SubmitPaymentDetails")
                            case "SecPay":
                            case "PayPoint": // Paypoint.Net
                                {
                                    ccPaymentXform = oEwProv.paySecPay(oOrder, oCart.mcPagePath + returnCmd, oCart.mcPaymentProfile);
                                    break;
                                }
                            case "PremiumCredit":
                                {
                                    ccPaymentXform = oEwProv.payPremiumCredit(oOrder, oCart.mcPagePath + returnCmd);
                                    break;
                                }
                            case "SecPayUkash":
                                {
                                    ccPaymentXform = oEwProv.paySecPayUkash(oOrder, oCart.mcPagePath + returnCmd);
                                    break;
                                }
                            case "WorldPay":
                                {
                                    ccPaymentXform = oEwProv.payWorldPay(oOrder, oCart.mcPagePath + returnCmd, oCart.myWeb.mnPageId);
                                    break;
                                }
                            case "NetBanx":
                                {
                                    ccPaymentXform = oEwProv.payNetBanx(oOrder, oCart.mcPagePath + returnCmd, oCart.myWeb.mnPageId);
                                    break;
                                }
                            case "SagePay":
                            case "ProTx":
                                {
                                    ccPaymentXform = oEwProv.paySagePay(oOrder, oCart.mcPagePath + returnCmd);
                                    break;
                                }
                            case "SecureTrading":
                                {
                                    string cReturnMethod = "";
                                    string cReturnParentTransRef = "";
                                    ccPaymentXform = oEwProv.paySecureTrading(oOrder, oCart.mcPagePath + returnCmd, cReturnMethod, cReturnParentTransRef);
                                    if (!string.IsNullOrEmpty(cReturnMethod))
                                    {
                                        ccPaymentXform = oEwProv.paySecureTrading(oOrder, oCart.mcPagePath + returnCmd, cReturnMethod, cReturnParentTransRef);
                                    }

                                    break;
                                }
                            case "MetaCharge":
                            case "PayPointMetaCharge":
                                {
                                    ccPaymentXform = oEwProv.payMetaCharge(oOrder, oCart.mcPagePath + returnCmd);
                                    break;
                                }
                            case "Secure Email":
                            case "SecureEmail":
                                {
                                    oEwProv.mcCartEmailAddress = moCartConfig["MerchantEmail"];
                                    ccPaymentXform = oEwProv.paySecureEmail(oOrder, oCart.mcPagePath + returnCmd);
                                    break;
                                }
                            case "Pay On Account":
                            case "PayOnAccount":
                                {
                                    ccPaymentXform = oEwProv.payOnAccount(oOrder, oCart.mcPagePath + returnCmd);
                                    break;
                                }
                            case "Save Order":
                            case "SaveOrder":
                                {
                                    ccPaymentXform = oEwProv.saveOrder(oOrder, oCart.mcPagePath + returnCmd);
                                    break;
                                }
                            case "Pay By Cash":
                            case "PayByCash":
                                {
                                    ccPaymentXform = oEwProv.payByCash(oOrder, oCart.mcPagePath + returnCmd);
                                    if (ccPaymentXform.valid)
                                    {
                                        myWeb.moCart.mnProcessId = oEwProv.mnProcessIdOnComplete;
                                    }

                                    break;
                                }
                            case "AuthorizeNet":
                                {
                                    ccPaymentXform = oEwProv.payAuthorizeNet(oOrder, oCart.mcPagePath + returnCmd);
                                    break;
                                }
                            case "DirectDebitSecureEmail":
                                {
                                    ccPaymentXform = oEwProv.payDirectDebitSecureEmail(oOrder, oCart.mcPagePath + returnCmd);
                                    break;
                                }

                            case "Cheque":
                                {
                                    break;
                                }
                            // to be done
                            // Case "PayPalPro"(MOVED To PROVIDER Lib)
                            // ccPaymentXform = oEwProv.payPayPalPro(oOrder, oCart.mcPagePath & "cartCmd=SubmitPaymentDetails", oCart.mcPaymentProfile)

                            case "PayPalExpress":
                                {
                                    if (myWeb.moRequest("ppCmd") == "cancel")
                                    {
                                        oCart.mcPaymentMethod = (object)null;
                                        var ccXform = new Protean.xForm(myWeb.moCtx, ref myWeb.msException);
                                        ccXform.NewFrm("Return");
                                        ccXform.valid = false;
                                        return ccXform;
                                    }
                                    else
                                    {
                                        ccPaymentXform = oEwProv.payPayPalExpress(oOrder, oCart.mcPagePath + returnCmd, oCart.mcPaymentProfile);
                                    }

                                    break;
                                }

                            default:
                                {
                                    if (Strings.InStr(mcPaymentMethod, "Repeat_") > 0)
                                    {
                                        // get repeat id
                                        string cOld = "";
                                        string cNew = "";
                                        int i = 1;
                                        int nStart = Strings.InStr(mcPaymentMethod, "Repeat_") + 6;
                                        while (!(!Information.IsNumeric(cNew) & !string.IsNullOrEmpty(cNew) | nStart + (i - 1) >= mcPaymentMethod.Length))
                                        {
                                            cOld = cNew;
                                            cNew = mcPaymentMethod.Substring(nStart, i);
                                            i += 1;
                                        }
                                        if (string.IsNullOrEmpty(cOld) | nStart + (i - 1) >= mcPaymentMethod.Length)
                                            cOld = cNew;
                                        // ok, cOld is the new id
                                        ccPaymentXform = oEwProv.RepeatConfirmation(cOld, oOrder, oCart.mcPagePath + returnCmd, cRepeatPaymentError);
                                        if (ccPaymentXform.valid == true)
                                        {
                                            oEwProv.ValidatePaymentByCart(oCart.mnCartId, true);
                                        }
                                    }

                                    else
                                    {
                                        // TS 16 Jan 2020 we should never default to pay on account.

                                        // ccPaymentXform = oEwProv.payOnAccount(oOrder, oCart.mcPagePath & returnCmd)
                                    }

                                    break;
                                }
                        }

                        oCart.mnPaymentId = oEwProv.savedPaymentId;

                        if (!string.IsNullOrEmpty(cRepeatPaymentError))
                        {
                            ccPaymentXform.addNote("RepeatError", Protean.xForm.noteTypes.Alert, cRepeatPaymentError, true);
                        }

                        if (oOrder.SelectSingleNode("error/msg") is not null)
                        {
                            oOrder.SelectSingleNode("error").PrependChild(oOrder.OwnerDocument.CreateElement("msg"));
                            oOrder.SelectSingleNode("error").FirstChild.InnerXml = "<strong>PAYMENT CANNOT PROCEED UNTIL QUANTITIES ARE ADJUSTED</strong>";
                        }
                        else if (ccPaymentXform.valid == true)
                        {
                            // added ability to change the success ID in payment providers



                        }

                        return ccPaymentXform;

                        oEwProv = default;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "GetPaymentForm", ex, "", cProcessInfo, gbDebug);
                        return (Protean.xForm)null;
                    }

                }

                public Protean.xForm GetRedirect3dsForm(ref Cms myWeb)
                {
                    // PerfMon.Log("EPDQ", "xfrmSecure3DReturn")
                    System.Collections.Specialized.NameValueCollection moCartConfig = myWeb.moCart.moCartConfig;
                    Protean.xForm oXform = new xForm(myWeb.msException);
                    XmlElement oFrmInstance;
                    XmlElement oFrmGroup;
                    string RedirectURL;

                    string cProcessInfo = "xfrmSecure3DReturn";
                    try
                    {
                        oXform.moPageXML = myWeb.moPageXml;

                        if (moCartConfig["SecureURL"].EndsWith("/"))
                        {
                            RedirectURL = moCartConfig["SecureURL"] + "?cartCmd=SubmitPaymentDetails&paymentMethod=" + myWeb.moCart.mcPaymentMethod;
                        }
                        else
                        {
                            RedirectURL = moCartConfig["SecureURL"] + "/?cartCmd=SubmitPaymentDetails&paymentMethod=" + myWeb.moCart.mcPaymentMethod;
                        }

                        // create the instance
                        oXform.NewFrm("Secure3DReturn");
                        oXform.submission("Secure3DReturn", goServer.UrlDecode(RedirectURL), "POST", "return form_check(this);");
                        oFrmInstance = oXform.moPageXML.CreateElement("Secure3DReturn");
                        oXform.Instance.AppendChild(oFrmInstance);
                        oFrmGroup = oXform.addGroup(ref oXform.moXformElmt, "Secure3DReturn1", "Secure3DReturn1", "Redirecting... Please do not refresh");

                        foreach (var item in myWeb.moRequest.Form)
                        {
                            XmlNode newInput = oXform.addInput(ref oFrmGroup, Conversions.ToString(item), false, Conversions.ToString(item), "hidden");
                            oXform.addValue(ref newInput, myWeb.moRequest.Form(Conversions.ToString(item)));
                        }

                        // build the form and the binds
                        // oXform.addDiv(oFrmGroup, "<SCRIPT LANGUAGE=""Javascript"">function onXformLoad(){document.Secure3DReturn.submit();};appendLoader(onXformLoad);</SCRIPT>")
                        oXform.addSubmit(ref oFrmGroup, "Secure3DReturn", "Continue", "ewSubmit");
                        oXform.addValues();
                        return oXform;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "GetRedirect3dsForm", ex, "", cProcessInfo, gbDebug);
                        return (Protean.xForm)null;
                    }

                }


                public string CheckStatus(ref Cms oWeb, ref string nPaymentProviderRef)
                {
                    string cProcessInfo = "";
                    try
                    {

                        return "Manual";
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CheckStatus", ex, "", cProcessInfo, gbDebug);
                        return "";
                    }

                }


                public XmlElement UpdatePaymentStatus(ref Cms oWeb, ref long nPaymentMethodKey)
                {
                    string cProcessInfo = "";
                    try
                    {
                        XmlElement oDoc = oWeb.moPageXml.CreateElement("PaymentMethod");
                        oDoc.InnerXml = oWeb.moDbHelper.getObjectInstance(dbHelper.objectTypes.CartPaymentMethod, nPaymentMethodKey);
                        return (XmlElement)oDoc.FirstChild;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "UpdatePaymentStatus", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }

                }


                public string GetMethodDetail(ref Cms oWeb, ref string nPaymentProviderRef)
                {
                    string cProcessInfo = "";
                    try
                    {

                        return "Not Available";
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CheckStatus", ex, "", cProcessInfo, gbDebug);
                        return "";
                    }

                }


                public bool AddPaymentButton(ref Protean.xForm oOptXform, ref XmlElement oFrmElmt, XmlElement configXml, double nPaymentAmount, string submissionValue, string refValue)
                {

                    string PaymentLabel = configXml.SelectSingleNode("description/@value").InnerText;
                    // allow html in description node...
                    bool bXmlLabel = false;

                    if (!string.IsNullOrEmpty(configXml.SelectSingleNode("description").InnerXml))
                    {
                        PaymentLabel = configXml.SelectSingleNode("description").InnerXml;
                        bXmlLabel = true;
                    }

                    string iconclass = "";
                    if (configXml.SelectSingleNode("icon/@value") is not null)
                    {
                        iconclass = configXml.SelectSingleNode("icon/@value").InnerText;
                    }

                    oOptXform.addSubmit(ref oFrmElmt, submissionValue, PaymentLabel, refValue, "pay-button pay-" + configXml.GetAttribute("name"), iconclass, configXml.GetAttribute("name"));
                    return default;

                }

                public void ValidatePaymentByCart(int nCartId, bool bValid)
                {
                    try
                    {
                        // sets the validity of the payment
                        // get the audit id 
                        string cSQL = "SELECT tblAudit.nAuditKey FROM tblCartOrder INNER JOIN tblCartPaymentMethod ON tblCartOrder.nPayMthdId = tblCartPaymentMethod.nPayMthdKey INNER JOIN tblAudit ON tblCartPaymentMethod.nAuditId = tblAudit.nAuditKey WHERE tblCartOrder.nCartOrderKey = " + nCartId;
                        string nAuditId = myWeb.moDbHelper.ExeProcessSqlScalar(cSQL);
                        if (Information.IsNumeric(nAuditId))
                        {

                            var oXml = new XmlDocument();
                            var oInstance = oXml.CreateElement("Instance");
                            var oElmt = oXml.CreateElement("tblAudit");
                            XmlNode argoNode = oElmt;
                            addNewTextNode("nAuditKey", ref argoNode, nAuditId);
                            oElmt = (XmlElement)argoNode;
                            XmlNode argoNode1 = oElmt;
                            addNewTextNode("nStatus", ref argoNode1, Conversions.ToString(Interaction.IIf(bValid, 1, 0)));
                            oElmt = (XmlElement)argoNode1;
                            oInstance.AppendChild(oElmt);

                            myWeb.moDbHelper.setObjectInstance(dbHelper.objectTypes.Audit, oInstance, nAuditId);

                        }
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "ValidatePaymentByCart", ex, "", "", gbDebug);
                    }
                }



            }
        }

    }
}
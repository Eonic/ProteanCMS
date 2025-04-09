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
using Protean.Tools.Integration.Twitter;
using static Protean.Cms;
using static Protean.Cms.Cart;
using static Protean.stdTools;
using static Protean.Tools.Xml;
using System.Dynamic;
using Protean.Providers.Membership;

namespace Protean.Providers
{
    namespace Payment
    {
        public interface IPaymentProvider
        {
            IPaymentProvider Initiate(ref Cms myWeb);
            IPaymentAdminXforms AdminXforms { get; set; }
            IPaymentAdminProcess AdminProcess { get; set; }
            IPaymentActivities Activities { get; set; }
        }
        public interface IPaymentAdminXforms
        {            
        }

        public interface IPaymentAdminProcess
        {           
        }
        public interface IPaymentActivities
        {            
            Protean.xForm GetPaymentForm(ref Cms myWeb, ref Protean.Cms.Cart oCart, ref XmlElement oOrder, string returnCmd = "cartCmd=SubmitPaymentDetails");
            Protean.xForm GetRedirect3dsForm(ref Cms myWeb);
            string CheckStatus(ref Cms oWeb, ref string nPaymentProviderRef);
            XmlElement UpdatePaymentStatus(ref Cms oWeb, ref long nPaymentMethodKey);
            string GetMethodDetail(ref Cms oWeb, ref string nPaymentProviderRef);
            bool AddPaymentButton(ref Cms myWeb, ref Protean.Cms.Cart oCart, ref Cms.xForm oOptXform, ref XmlElement oFrmElmt, XmlElement configXml, double nPaymentAmount, string submissionValue, string refValue);
            void ValidatePaymentByCart(int nCartId, bool bValid);
            string RefundPayment(string providerPaymentReference, decimal amount, string validGroup = "");
            string CancelPayments(ref Cms oWeb, ref string nPaymentProviderRef);
            string CollectPayment(ref Cms myWeb, long nPaymentMethodId, double Amount, string CurrencyCode, string PaymentDescription, ref Cms.Cart oCart);
            string UpdateOrderWithPaymentResponse(string AuthNumber, string validGroup = "");
            string ProcessNewPayment(string orderId, decimal amount, string cardNumber, string cV2, string expiryDate, String startDate, String cardHolderName, string address1, string address2, string town, string postCode, string cCounty = "", string cCountry = "", string validGroup = "");
        }
        public class ReturnProvider
        {
            private const string mcModuleName = "Protean.Providers.Payment.GetProvider";
            protected XmlNode moPaymentCfg;            
            //private IPaymentAdminXforms AdminXforms;
            //private IPaymentAdminProcess AdminProcess;
            //private IPaymentActivities Activities;           
            public IPaymentProvider Get(ref Cms myWeb, string ProviderName)
            {
                try
                {
                    Type calledType;
                    XmlElement oProviderCfg;

                    moPaymentCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/payment");
                    oProviderCfg = (XmlElement)moPaymentCfg.SelectSingleNode("provider[@name='" + ProviderName + "']");

                    string ProviderClass = "";
                    if (oProviderCfg != null)
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
                        if (moPrvConfig.Providers[ProviderClass] != null)
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

                    var args = new object[1];
                    args[0] = myWeb;

                    return (IPaymentProvider)calledType.InvokeMember("Initiate", BindingFlags.InvokeMethod, null, o, args);
                }
                catch (Exception ex)
                {
                    // TS commented this out as if we have an old payment provider that has been retired we do not want errors.
                    //stdTools.returnException(ref myWeb.msException, mcModuleName, "New", ex, "", ProviderName + " Could Not be Loaded", gbDebug);
                    return null;
                }

            }

        }

        public class DefaultProvider : IPaymentProvider
        {

            public DefaultProvider()
            {
                // do nothing
            }
            private IPaymentAdminXforms _AdminXforms;
            private IPaymentAdminProcess _AdminProcess;
            private IPaymentActivities _Activities;
            IPaymentAdminXforms IPaymentProvider.AdminXforms
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
            IPaymentAdminProcess IPaymentProvider.AdminProcess
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
            IPaymentActivities IPaymentProvider.Activities
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

            public IPaymentProvider Initiate(ref Cms myWeb)
            {
                _AdminXforms = new AdminXForms(ref myWeb);
                _AdminProcess = new AdminProcess(ref myWeb);
                // MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms
                _Activities = new Activities();
                return this;
            }

            public class AdminXForms : Admin.AdminXforms, IPaymentAdminXforms
            {
                private const string mcModuleName = "Providers.Providers.Eonic.AdminXForms";

                public AdminXForms(ref Cms aWeb) : base(ref aWeb)
                {
                }

            }

            public class AdminProcess : Admin, IPaymentAdminProcess
            {

                private AdminXForms _oAdXfm;

                public DefaultProvider.AdminXForms oAdXfm
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

                public AdminProcess(ref Cms aWeb) : base(ref aWeb)
                {
                }
            }


            public class Activities : IPaymentActivities
            {
                private const string mcModuleName = "Providers.Payment.Eonic.Activities";

                public Cms myWeb;
                public Activities()
                {
                }

                public Protean.xForm GetPaymentForm(ref Cms myWeb, ref Protean.Cms.Cart oCart, ref XmlElement oOrder, string returnCmd = "cartCmd=SubmitPaymentDetails")
                {

                    string cBillingAddress;
                    System.Collections.Specialized.NameValueCollection moCartConfig = oCart.moCartConfig;
                    string mcPaymentMethod = oCart.mcPaymentMethod;
                    var oEwProv = new PaymentProviders(ref oCart.myWeb);
                    string cProcessInfo = "";
                    string cRepeatPaymentError = "";

                    try
                    {

                        oEwProv.mcCurrency =Convert.ToString(Interaction.IIf(oCart.mcCurrencyCode == "", oCart.mcCurrency, oCart.mcCurrencyCode));
                        oEwProv.mcCurrencySymbol = oCart.mcCurrencySymbol;
                        if (string.IsNullOrEmpty(oOrder.GetAttribute("payableType")))
                        {
                            oEwProv.mnPaymentAmount =Convert.ToDouble(oOrder.GetAttribute("total"));
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(oOrder.GetAttribute("payableAmount")))
                            {
                                oEwProv.mnPaymentAmount = Convert.ToDouble(oOrder.GetAttribute("total"));
                            }
                            else
                            {
                                oEwProv.mnPaymentAmount = Convert.ToDouble(oOrder.GetAttribute("payableAmount"));
                                oEwProv.mnPaymentMaxAmount = Convert.ToDouble(oOrder.GetAttribute("total"));
                            }
                            oEwProv.mcPaymentType = oOrder.GetAttribute("payableType");
                        }
                        oEwProv.mnCartId = oCart.mnCartId;
                        oEwProv.mcPaymentOrderDescription = "Ref:" + oCart.OrderNoPrefix + oCart.mnCartId + " An online purchase from: " + oCart.mcSiteURL + " on " + niceDate(DateTime.Now) + " " + DateAndTime.TimeValue(Conversions.ToString(DateTime.Now));

                        if (oOrder.SelectSingleNode("Contact[@type='Billing Address']/GivenName") != null)
                        {
                            oEwProv.mcCardHolderName = oOrder.SelectSingleNode("Contact[@type='Billing Address']/GivenName").InnerText;
                        }

                        // Build the billing address string
                        //object localgetNodeValueByType() { XmlNode argoParent = oOrder; var ret = getNodeValueByType(ref argoParent, "Contact[@type='Billing Address']/Street"); oOrder = (XmlElement)argoParent; return ret; }
                        //object localgetNodeValueByType1() { XmlNode argoParent1 = oOrder; var ret = getNodeValueByType(ref argoParent1, "Contact[@type='Billing Address']/City"); oOrder = (XmlElement)argoParent1; return ret; }
                        //object localgetNodeValueByType2() { XmlNode argoParent2 = oOrder; var ret = getNodeValueByType(ref argoParent2, "Contact[@type='Billing Address']/State"); oOrder = (XmlElement)argoParent2; return ret; }
                        //object localgetNodeValueByType3() { XmlNode argoParent3 = oOrder; var ret = getNodeValueByType(ref argoParent3, "Contact[@type='Billing Address']/Country"); oOrder = (XmlElement)argoParent3; return ret; }

                        //cBillingAddress = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(localgetNodeValueByType(), Constants.vbLf), localgetNodeValueByType1()), Constants.vbLf), localgetNodeValueByType2()), Constants.vbLf), localgetNodeValueByType3()), Constants.vbLf));

                        //Build the billing address string
                        XmlNode xmloOrder = oOrder;
                        cBillingAddress = getNodeValueByType(ref xmloOrder, "Contact[@type='Billing Address']/Street") + Constants.vbLf +
                            getNodeValueByType(ref xmloOrder, "Contact[@type='Billing Address']/City") + Constants.vbLf +
                            getNodeValueByType(ref xmloOrder, "Contact[@type='Billing Address']/State") + Constants.vbLf +
                            getNodeValueByType(ref xmloOrder, "Contact[@type='Billing Address']/Country") + Constants.vbLf;

                        oEwProv.mcCardHolderAddress = cBillingAddress;
                        oEwProv.moBillingContact = (XmlElement)oOrder.SelectSingleNode("Contact[@type='Billing Address']");

                        XmlNode argoParent = oOrder;
                        oEwProv.mcCardHolderPostcode =Convert.ToString(getNodeValueByType(ref argoParent, "Contact[@type='Billing Address']/PostalCode"));
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
                            
                            case "Secure Email":
                            case "SecureEmail":
                                {
                                    oEwProv.mcCartEmailAddress = moCartConfig["MerchantEmail"];
                                    ccPaymentXform = oEwProv.paySecureEmail(ref oOrder, oCart.mcPagePath + returnCmd);
                                    break;
                                }
                            case "Pay On Account":
                            case "PayOnAccount":
                                {
                                    ccPaymentXform = oEwProv.payOnAccount(ref oOrder, oCart.mcPagePath + returnCmd);
                                    break;
                                }
                            case "Save Order":
                            case "SaveOrder":
                                {
                                    ccPaymentXform = oEwProv.saveOrder(ref oOrder, oCart.mcPagePath + returnCmd);
                                    break;
                                }
                            case "Pay By Cash":
                            case "PayByCash":
                                {
                                    ccPaymentXform = oEwProv.payByCash(ref oOrder, oCart.mcPagePath + returnCmd);
                                    if (ccPaymentXform.valid)
                                    {
                                        myWeb.moCart.mnProcessId = (short)oEwProv.mnProcessIdOnComplete;
                                    }

                                    break;
                                }


                            case "Cheque":
                                {
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
                                        ccPaymentXform = oEwProv.RepeatConfirmation(Convert.ToInt32(cOld),ref oOrder, oCart.mcPagePath + returnCmd, ref cRepeatPaymentError);
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

                        oCart.mnPaymentId =Convert.ToInt32(oEwProv.savedPaymentId);

                        if (!string.IsNullOrEmpty(cRepeatPaymentError))
                        {
                            ccPaymentXform.addNote("RepeatError", Protean.xForm.noteTypes.Alert, cRepeatPaymentError, true);
                        }

                        if (oOrder.SelectSingleNode("error/msg") != null)
                        {
                            oOrder.SelectSingleNode("error").PrependChild(oOrder.OwnerDocument.CreateElement("msg"));
                            oOrder.SelectSingleNode("error").FirstChild.InnerXml = "<strong>PAYMENT CANNOT PROCEED UNTIL QUANTITIES ARE ADJUSTED</strong>";
                        }
                        else if (ccPaymentXform.valid == true)
                        {
                            // added ability to change the success ID in payment providers



                        }
                        oEwProv = default;
                        return ccPaymentXform;                        
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
                    Protean.xForm oXform = new xForm(ref myWeb.msException);
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
                            XmlElement newInputElmt = (XmlElement)newInput;
                            oXform.addValue(ref newInputElmt, myWeb.moRequest.Form[Conversions.ToString(item)]);

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


                public bool AddPaymentButton(ref Cms myWeb, ref Protean.Cms.Cart oCart, ref Cms.xForm oOptXform, ref XmlElement oFrmElmt, XmlElement configXml, double nPaymentAmount, string submissionValue, string refValue)
                {

                    string PaymentLabel = "Pay Now";

                    XmlNode LabelNode = configXml.SelectSingleNode("description/@value");
                    if (LabelNode != null)
                    {
                        PaymentLabel = LabelNode.InnerText;
                    }

                    // allow html in description node...
                   // bool bXmlLabel = false;

                    LabelNode = configXml.SelectSingleNode("description/@value");
                    if (LabelNode != null)
                    {
                        PaymentLabel = configXml.SelectSingleNode("description/@value").InnerXml;
                        //bXmlLabel = true;
                    }

                    string iconclass = "";
                    if (configXml.SelectSingleNode("icon/@value") != null)
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

                            myWeb.moDbHelper.setObjectInstance(dbHelper.objectTypes.Audit, oInstance, Convert.ToInt32(nAuditId));

                        }
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "ValidatePaymentByCart", ex, "", "", gbDebug);
                    }
                }

                public string RefundPayment(string providerPaymentReference, decimal amount, string validGroup = "")
                {
                    throw new NotImplementedException();
                }

                public string CancelPayments(ref Cms oWeb, ref string nPaymentProviderRef)
                {
                    throw new NotImplementedException();
                }

                public string CollectPayment(ref Cms myWeb, long nPaymentMethodId, double Amount, string CurrencyCode, string PaymentDescription, ref Cart oCart)
                {
                    throw new NotImplementedException();
                }

                public string UpdateOrderWithPaymentResponse(string AuthNumber, string validGroup = "")
                {
                    throw new NotImplementedException();
                }

                public string ProcessNewPayment(string orderId, decimal amount, string cardNumber, string cV2, string expiryDate, string startDate, string cardHolderName, string address1, string address2, string town, string postCode, string cCounty = "", string cCountry = "", string validGroup = "")
                {
                    throw new NotImplementedException();
                }
            }
        }

    }
}
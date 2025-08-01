﻿using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Xml;
using static Protean.stdTools;


namespace Protean.Providers
{
    namespace Payment
    {
        public class SkipPayment : IPaymentProvider
        {

            private const string mcModuleName = "Providers.Payment.SkipPayment";

            public SkipPayment()
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
                string cProcessInfo = "";
                try
                {

                    _AdminXforms = new AdminXForms(ref myWeb);
                    _AdminProcess = new AdminProcess(ref myWeb);
                    // MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms
                    _Activities = new Activities();
                    return this;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "Initiate", ex, "", cProcessInfo, gbDebug);
                    return this;
                }
            }

            public class AdminXForms : Cms.Admin.AdminXforms, IPaymentAdminXforms
            {
                private const string mcModuleName = "Providers.Providers.Eonic.AdminXForms";

                public AdminXForms(ref Cms aWeb) : base(ref aWeb)
                {
                }

            }

            public class AdminProcess : Cms.Admin, IPaymentAdminProcess
            {

                //private Protean.Providers.Payment.PayPalPro.AdminXForms _oAdXfm;

                public object oAdXfm
                {
                    set
                    {
                        // _oAdXfm = value;
                    }
                    get
                    {
                        // return _oAdXfm;
                        return "";
                    }
                }

                public AdminProcess(ref Cms aWeb) : base(ref aWeb)
                {
                }
            }


            public class Activities : IPaymentActivities
            {

                private const string mcModuleName = "Providers.Payment.PayPalPro.Activities";
                private Cms myWeb;
                protected XmlNode moPaymentCfg;
                //private TransactionMode nTransactionMode;

                public enum TransactionMode
                {
                    Live = 0,
                    Test = 1,
                    Fail = 2
                }


                public string CollectPayment(ref Cms oWeb, long nPaymentMethodId, double Amount, string CurrencyCode, string PaymentDescription, ref Cms.Cart oCart)
                {
                    string cProcessInfo = "";
                    try
                    {

                        // Do nothing because recurring payments are automatic

                        return "Success";
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CollectPayment", ex, "", cProcessInfo, gbDebug);
                        return "Payment Error";
                    }
                }

                public Protean.xForm GetPaymentForm(ref Cms myWeb, ref Cms.Cart oCart, ref XmlElement oOrder, string returnCmd = "cartCmd=SubmitPaymentDetails")
                {
                    string cProcessInfo = "";
                    try
                    {

                        var ccXform = new Protean.xForm(myWeb.moCtx, ref myWeb.msException);

                        ccXform.NewFrm("SkipPayment");

                        // Dim ccXform As New Protean.xForm(myWeb.moCtx)
                        ccXform.valid = true;
                        return ccXform;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "GetPaymentForm", ex, "", cProcessInfo, gbDebug);
                        return (Protean.xForm)null;
                    }
                }




                public string CheckStatus(ref Cms oWeb, ref string nPaymentProviderRef)
                {
                    string cProcessInfo = "";
                    try
                    {

                        return "Error - no value returned";
                    }


                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CheckStatus", ex, "", cProcessInfo, gbDebug);
                        return "";
                    }

                }

                public string CancelPayments(ref Cms oWeb, ref string nPaymentProviderRef)
                {
                    string cProcessInfo = "";

                    try
                    {

                        return CheckStatus(ref oWeb, ref nPaymentProviderRef);
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, mcModuleName, "CancelPayments", ex, "", cProcessInfo, gbDebug);
                        return "";
                    }

                }

                public bool AddPaymentButton(ref Cms myWeb, ref Cms.Cart myCart, ref Cms.xForm oOptXform, ref XmlElement oFrmElmt, XmlElement configXml, double nPaymentAmount, string submissionValue, string refValue)
                {

                    try
                    {

                        double nMaxAmt = Conversions.ToDouble("0" + configXml.SelectSingleNode("MaxValue").InnerText);
                        double nMinAmt = Conversions.ToDouble("0" + configXml.SelectSingleNode("MinValue").InnerText);

                        if (nMaxAmt <= nPaymentAmount & nMinAmt >= nPaymentAmount)
                        {

                            string PaymentLabel = configXml.SelectSingleNode("description/@value").InnerText;
                            // allow html in description node...
                            // bool bXmlLabel = false;

                            if (!string.IsNullOrEmpty(configXml.SelectSingleNode("description").InnerXml))
                            {
                                PaymentLabel = configXml.SelectSingleNode("description").InnerXml;
                                // bXmlLabel = true;
                            }

                            string iconclass = "";
                            if (configXml.SelectSingleNode("icon/@value") != null)
                            {
                                iconclass = configXml.SelectSingleNode("icon/@value").InnerText;
                            }

                            oOptXform.addSubmit(ref oFrmElmt, submissionValue, PaymentLabel, refValue, "pay-button pay-" + configXml.GetAttribute("name"), iconclass, configXml.GetAttribute("name"));

                        }
                    }

                    catch (Exception)
                    {

                    }

                    return default;
                }

                public xForm GetRedirect3dsForm(ref Cms myWeb)
                {
                    throw new NotImplementedException();
                }

                public XmlElement UpdatePaymentStatus(ref Cms oWeb, ref long nPaymentMethodKey)
                {
                    throw new NotImplementedException();
                }

                public string GetMethodDetail(ref Cms oWeb, ref string nPaymentProviderRef)
                {
                    throw new NotImplementedException();
                }

                public void ValidatePaymentByCart(int nCartId, bool bValid)
                {
                    throw new NotImplementedException();
                }

                public string RefundPayment(string providerPaymentReference, decimal amount, string validGroup = "")
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

                public XmlElement GetWalletPaymentDetails(XmlElement opElemt)
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
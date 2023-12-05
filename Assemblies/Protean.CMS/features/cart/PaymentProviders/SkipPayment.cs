using System;
using System.Xml;
using Microsoft.VisualBasic.CompilerServices;
using static Protean.stdTools;


namespace Protean.Providers
{
    namespace Payment
    {
        public class SkipPayment
        {

            private const string mcModuleName = "Providers.Payment.SkipPayment";

            public SkipPayment()
            {
                // do nothing
            }

            public void Initiate(ref object _AdminXforms, ref object _AdminProcess, ref object _Activities, ref Payment.DefaultProvider PayProvider, ref Cms myWeb)
            {
                string cProcessInfo = "";
                try
                {

                    PayProvider.AdminXforms = new AdminXForms(ref myWeb);
                    PayProvider.AdminProcess = new AdminProcess(ref myWeb);
                    // PayProvider.AdminProcess.oAdXfm = PayProvider.AdminXforms
                    PayProvider.Activities = new Activities();
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "Initiate", ex, "", cProcessInfo, gbDebug);

                }
            }

            public class AdminXForms : Cms.Admin.AdminXforms
            {
                private const string mcModuleName = "Providers.Providers.Eonic.AdminXForms";

                public AdminXForms(ref Cms aWeb) : base(ref aWeb)
                {
                }

            }

            public class AdminProcess : Cms.Admin
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


            public class Activities
            {

                private const string mcModuleName = "Providers.Payment.PayPalPro.Activities";
                private Cms myWeb;
                protected XmlNode moPaymentCfg;
                private TransactionMode nTransactionMode;

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

                public bool AddPaymentButton(ref Protean.xForm oOptXform, ref XmlElement oFrmElmt, XmlElement configXml, double nPaymentAmount, string submissionValue, string refValue)
                {

                    try
                    {

                        double nMaxAmt = Conversions.ToDouble("0" + configXml.SelectSingleNode("MaxValue").InnerText);
                        double nMinAmt = Conversions.ToDouble("0" + configXml.SelectSingleNode("MinValue").InnerText);

                        if (nMaxAmt <= nPaymentAmount & nMinAmt >= nPaymentAmount)
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


            }
        }
    }
}
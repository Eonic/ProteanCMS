using Newtonsoft.Json;
using Protean.Providers.Membership;
using Protean.Providers.Messaging;
using Protean.Providers.Payment;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using System.Xml.Linq;
using static Lucene.Net.QueryParsers.QueryParser;
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
            private System.Collections.Specialized.NameValueCollection moWebConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");


            public void CompleteOrder(XmlDocument oCartXML, ref XmlElement oContentElmt, ref XmlElement oElmt)
            {
                string cProcessInfo = "Cart.CompleteOrder";
                try
                {

                    oContentElmt = (XmlElement)CreateCartElement(oCartXML);
                    oElmt = (XmlElement)oContentElmt.FirstChild;
                    PersistVariables();

                    if (oElmt.FirstChild is null)
                    {
                        GetCart(ref oElmt);
                    }

                    if (mnProcessId == (int)cartProcess.Complete | mnProcessId == (int)cartProcess.DepositPaid | mnProcessId == (int)cartProcess.AwaitingPayment)
                    {

                        if (moCartConfig["StockControl"] == "on")
                        {
                            UpdateStockLevels(ref oElmt);
                        }
                        UpdateGiftListLevels();
                        addDateAndRef(ref oElmt);
                        if (myWeb.mnUserId > 0)
                        {
                            var userXml = myWeb.moDbHelper.GetUserXML((long)myWeb.mnUserId, false);
                            if (userXml != null)
                            {
                                XmlElement cartElement = (XmlElement)oContentElmt.SelectSingleNode("Cart");
                                if (cartElement != null)
                                {
                                    cartElement.AppendChild(cartElement.OwnerDocument.ImportNode(userXml, true));
                                }
                            }
                        }

                        if (string.Equals(myWeb.moSession["Settlement"]?.ToString(), "true", StringComparison.OrdinalIgnoreCase))
                        {
                            // modifiy the cartXml in line with settlement
                            if (mnProcessId == (int)cartProcess.DepositPaid)
                            {
                                mnProcessId = (short)cartProcess.Complete;

                            }
                            myWeb.moSession["Settlement"] = (object)null;
                        }



                        if (mnProcessId == (int)cartProcess.DepositPaid)
                        {
                            AddToLists("Deposit", ref oContentElmt);
                        }
                        else
                        {
                            AddToLists("Invoice", ref oContentElmt);
                        }

                        purchaseActions(oContentElmt);
                        // update the cart if purchase actions have changed it
                        // GetCart(oElmt)
                        // done for ammerdown as we have removed a product.



                        if (myWeb.mnUserId > 0)
                        {
                            if (moSubscription != null)
                            {
                                moSubscription.AddUserSubscriptions(mnCartId, myWeb.mnUserId, ref oContentElmt, mnPaymentId);
                            }
                        }

                        if (moCartConfig["SendReceiptEmailForAwaitingPaymentStatusId"] != null)
                        {
                            if ((oElmt.GetAttribute("statusId") ?? "") != (moCartConfig["SendReceiptEmailForAwaitingPaymentStatusId"] ?? ""))
                            {
                                emailReceipts(ref oContentElmt);
                            }
                        }
                        else
                        {
                            emailReceipts(ref oContentElmt);
                        }


                        moDiscount.DisablePromotionalDiscounts();

                    }



                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, " CompleteOrder", ex, "", cProcessInfo, gbDebug);

                }
            }


            public void addDateAndRef(ref XmlElement oCartElmt, DateTime invoiceDate = default, long nCartId = 0L)
            {
                myWeb.PerfMon.Log("Cart", "addDateAndRef");
                // adds current date and an invoice reference number to the cart object.
                // so the cart now contains all details needed for an invoice
                string cProcessInfo = "";
                if (nCartId == 0L)
                    nCartId = mnCartId;
                try
                {
                    if (invoiceDate == default)
                        invoiceDate = DateTime.Now;
                    if (nCartId == 0L)
                        nCartId = Convert.ToInt64(oCartElmt.GetAttribute("cartId"));
                    oCartElmt.SetAttribute("InvoiceDate", niceDate(invoiceDate));
                    oCartElmt.SetAttribute("InvoiceDateTime", XmlDate(invoiceDate, true));
                    oCartElmt.SetAttribute("InvoiceRef", OrderNoPrefix + nCartId.ToString());
                    if (!string.IsNullOrEmpty(mcVoucherNumber))
                    {
                        oCartElmt.SetAttribute("payableType", "Voucher");
                        oCartElmt.SetAttribute("voucherNumber", mcVoucherNumber);
                        oCartElmt.SetAttribute("voucherValue", mcVoucherValue);
                        oCartElmt.SetAttribute("voucherExpires", mcVoucherExpires);
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "addDateAndRef", ex, "", cProcessInfo, gbDebug);
                }

            }


            public object emailCart(ref XmlElement oCartXML, string xsltPath, string fromName, string fromEmail, string recipientEmail, string SubjectLine, bool bEncrypt = false, string cAttachementTemplatePath = "", string cBCCEmail = "", string cCCEmail = "")
            {
                myWeb.PerfMon.Log("Cart", "emailCart");
                var oXml = new XmlDocument();
                string cProcessInfo = "emailCart";
                try
                {
                    // check file path

                    var ofs = new Protean.fsHelper();

                    oXml.LoadXml(oCartXML.OuterXml);
                    xsltPath = ofs.checkCommonFilePath(moConfig["ProjectPath"] + xsltPath);

                    oCartXML.SetAttribute("lang", myWeb.mcPageLanguage);

                    var oMsg = new Protean.Messaging(ref myWeb.msException);
                    if (string.IsNullOrEmpty(cAttachementTemplatePath))
                    {

                        Cms.dbHelper argodbHelper = null;
                        cProcessInfo = Convert.ToString(oMsg.emailer(oCartXML, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, odbHelper: ref argodbHelper, "Message Sent", "Message Failed", ccRecipient: cCCEmail, bccRecipient: cBCCEmail));
                    }
                    else
                    {
                        cAttachementTemplatePath = moServer.MapPath(cAttachementTemplatePath);
                        string cFontPath = moServer.MapPath("/fonts");
                        var oPDF = new Tools.PDF();

                        // create the xmlFO document
                        Protean.XmlHelper.Transform oTransform;

                        string styleFile = cAttachementTemplatePath;
                        myWeb.PerfMon.Log("Web", "ReturnPageHTML - loaded Style");
                        oTransform = new Protean.XmlHelper.Transform(ref myWeb, styleFile, false);

                        myWeb.msException = "";

                        oTransform.mbDebug = gbDebug;
                        TextWriter oTW = new StringWriter();
                        XmlWriter icXmlWriter = XmlWriter.Create(oTW);
                        var OrderDoc = new XmlDocument();
                        OrderDoc.LoadXml(oCartXML.OuterXml);

                        XmlReader oXMLReaderInstance = new XmlNodeReader(oCartXML);

                        oTransform.ProcessTimed(oXMLReaderInstance, ref icXmlWriter);
                        OrderDoc = null;

                        string foNetXml = oTW.ToString();
                        string FileName = "Attachment.pdf";

                        var FoDoc = new XmlDocument();
                        FoDoc.LoadXml(foNetXml);
                        var nsMgr = new XmlNamespaceManager(FoDoc.NameTable);
                        nsMgr.AddNamespace("fo", "http://www.w3.org/1999/XSL/Format");
                        if (FoDoc.DocumentElement.SelectSingleNode("descendant::fo:title", nsMgr) != null)
                        {
                            FileName = FoDoc.DocumentElement.SelectSingleNode("descendant::fo:title", nsMgr).InnerText.Replace(" ", "-") + ".pdf";
                        }
                        FoDoc = null;

                        oMsg.addAttachment(oPDF.GetPDFstream(foNetXml, cFontPath), FileName);
                        Cms.dbHelper argodbHelper1 = null;
                        cProcessInfo = Convert.ToString(oMsg.emailer(oCartXML, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, odbHelper: ref argodbHelper1, "Message Sent", "Message Failed", ccRecipient: cCCEmail, bccRecipient: cBCCEmail));

                    }
                    oMsg = (Protean.Messaging)null;

                    return cProcessInfo;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "emailCart", ex, "", cProcessInfo, gbDebug);
                    return null;
                }

            }



            public virtual void emailReceipts(ref XmlElement oCartElmt)
            {
                emailReceipts(ref oCartElmt, "");
            }
            public virtual void emailReceipts(ref XmlElement oCartElmt, string ccCustomerEmail = "")
            {
                myWeb.PerfMon.Log("Cart", "emailReceipts");
                string sMessageResponse;
                string cProcessInfo = "";
                try
                {
                    if ((moCartConfig["EmailReceipts"]?.ToString().ToLower()) != "off")
                    {
                        // Default subject line
                        string cSubject = moCartConfig["OrderEmailSubject"];
                        if (string.IsNullOrEmpty(cSubject))
                            cSubject = "Website Order";

                        string CustomerEmailTemplatePath = "/xsl/Cart/mailOrderCustomer.xsl";
                        string MerchantEmailTemplatePath = "/xsl/Cart/mailOrderMerchant.xsl";
                        if (myWeb.bs5)
                        {
                            CustomerEmailTemplatePath = "/features/cart/email/order-customer.xsl";
                            MerchantEmailTemplatePath = "/features/cart/email/order-merchant.xsl";
                        }
                        if (!string.IsNullOrEmpty(moCartConfig["CustomerEmailTemplatePath"]))
                        {
                            CustomerEmailTemplatePath = moCartConfig["CustomerEmailTemplatePath"];
                        }
                        if (!string.IsNullOrEmpty(moCartConfig["MerchantEmailTemplatePath"]))
                        {
                            MerchantEmailTemplatePath = moCartConfig["MerchantEmailTemplatePath"];
                        }

                        // send to customer
                        sMessageResponse = Convert.ToString(emailCart(ref oCartElmt, CustomerEmailTemplatePath, moCartConfig["MerchantName"], moCartConfig["MerchantEmail"], oCartElmt.FirstChild.SelectSingleNode("Contact[@type='Billing Address']/Email").InnerText, cSubject, cAttachementTemplatePath: moCartConfig["CustomerAttachmentTemplatePath"], cCCEmail: ccCustomerEmail));

                        // Send to merchant
                        sMessageResponse = Convert.ToString(emailCart(ref oCartElmt, MerchantEmailTemplatePath, oCartElmt.FirstChild.SelectSingleNode("Contact[@type='Billing Address']/GivenName").InnerText, oCartElmt.FirstChild.SelectSingleNode("Contact[@type='Billing Address']/Email").InnerText, moCartConfig["MerchantEmail"], cSubject, false, moCartConfig["MerchantAttachmentTemplatePath"], moCartConfig["MerchantEmailBcc"]));

                        XmlElement oElmtEmail;
                        oElmtEmail = moPageXml.CreateElement("Reciept");
                        oCartElmt.AppendChild(oCartElmt.OwnerDocument.ImportNode(oElmtEmail, true));
                        oElmtEmail.InnerText = sMessageResponse;

                        if (sMessageResponse == "Message Sent")
                        {
                            oElmtEmail.SetAttribute("status", "sent");
                        }
                        else
                        {
                            oElmtEmail.SetAttribute("status", "failed");
                        }
                    }
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "emailReceipts", ex, "", cProcessInfo, gbDebug);
                }

            }

            public object AddPayment(double amountPaid, string Description)
            {
                string cProcessInfo = "";
                string sSql;
                try
                {
                    var nAmountReceived = default(double);
                    // Get the amount received so far
                    sSql = "select * from tblCartOrder where nCartOrderKey = " + mnCartId;
                    using (var oDr = moDBHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                    {
                        if (oDr.HasRows)
                        {
                            while (oDr.Read())
                                nAmountReceived = Convert.ToDouble(oDr["nAmountReceived"]?.ToString() ?? "0");
                        }
                    }
                    nAmountReceived = nAmountReceived + amountPaid;

                    sSql = "update tblCartOrder set nAmountReceived = " + nAmountReceived + " where nCartOrderKey = " + mnCartId;
                    moDBHelper.ExeProcessSql(sSql);

                    mnPaymentId = moDBHelper.savePayment(mnCartId, (long)myWeb.mnUserId, "", "", Description, (XmlElement)null, DateTime.Now, false, amountPaid, "deduction");
                    return null;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "ConfirmPayment", ex, "", cProcessInfo, gbDebug);
                    return null;
                }
                finally
                {
                    // oDr = Nothing
                }
            }


            public object ConfirmPayment(ref XmlElement oCartElmt, ref XmlElement PaymentDetailXml, string providerPaymentRef, string providerName, double amountPaid)
            {
                string cProcessInfo = "ConfirmPayment";
                try
                {
                    string PayableType = oCartElmt.GetAttribute("payableType");

                    // Add processing for deposits.
                    switch (PayableType ?? "")
                    {
                        case "deposit":
                            {
                                mcDepositAmount = Convert.ToDouble("0" + oCartElmt.GetAttribute("payableAmount")).ToString();
                                double outstandingAmount;
                                if (Convert.ToDouble(mcDepositAmount) == 0d)
                                {
                                    // no deposit payment paid in full
                                    outstandingAmount = 0d;
                                }
                                else
                                {
                                    outstandingAmount = Convert.ToDouble("0" + oCartElmt.GetAttribute("total")) - Convert.ToDouble(mcDepositAmount);
                                }

                                // Let's update the cart element
                                oCartElmt.SetAttribute("paymentMade", mcDepositAmount);
                                oCartElmt.SetAttribute("outstandingAmount", outstandingAmount.ToString("N2"));

                                // Let's create a unique link for settlement
                                // Make a unique link
                                string cUniqueLink = "";
                                while (string.IsNullOrEmpty(cUniqueLink))
                                {
                                    object testLink = Guid.NewGuid().ToString();
                                    string sSql = "select * from tblCartOrder where cSettlementID = '" + testLink + "'";
                                    using (var oDr = moDBHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                    {
                                        if (!oDr.HasRows)
                                            cUniqueLink = Convert.ToString(testLink);
                                    }
                                }
                                if (outstandingAmount == 0d)
                                {
                                    PayableType = "full";
                                    oCartElmt.SetAttribute("transStatus", "Paid In Full");
                                    mnProcessId = 6;
                                }
                                else
                                {
                                    oCartElmt.SetAttribute("settlementID", cUniqueLink);
                                    oCartElmt.SetAttribute("transStatus", "Deposit Paid");
                                    mnProcessId = 10;
                                }

                                UpdateCartDeposit(ref oCartElmt, amountPaid, PayableType);
                                break;
                            }


                        case "settlement":
                            {
                                mnProcessId = 6;
                                double totalPaid = Convert.ToDouble(oCartElmt.GetAttribute("paymentMade"));
                                totalPaid = totalPaid + amountPaid;
                                double outstandingAmount = Convert.ToDouble("0" + oCartElmt.GetAttribute("total")) - totalPaid;
                                oCartElmt.SetAttribute("paymentMade", amountPaid.ToString());
                                oCartElmt.SetAttribute("outstandingAmount", outstandingAmount.ToString());
                                oCartElmt.SetAttribute("payableAmount", outstandingAmount.ToString());
                                oCartElmt.SetAttribute("transStatus", "Settlement Paid");
                                oCartElmt.SetAttribute("status", "Settlement Paid");
                                oCartElmt.SetAttribute("statusId", mnProcessId.ToString());
                                UpdateCartDeposit(ref oCartElmt, amountPaid, PayableType);
                                break;
                            }

                        default:
                            {
                                PayableType = "full";
                                UpdateCartDeposit(ref oCartElmt, amountPaid, PayableType);
                                oCartElmt.SetAttribute("transStatus", "Paid In Full");
                                mnProcessId = 6;
                                break;
                            }
                    }

                    mnPaymentId = moDBHelper.savePayment(mnCartId, (long)myWeb.mnUserId, providerName, providerPaymentRef, providerName, PaymentDetailXml, DateTime.Now, false, amountPaid, PayableType);
                    return null;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "ConfirmPayment", ex, "", cProcessInfo, gbDebug);
                    return null;
                }
                finally
                {
                    // oDr = Nothing
                }
            }

            public virtual void purchaseActions(XmlElement oCartElmt, bool bRenderScriptOnly = false)
            {
                myWeb.PerfMon.Log("Cart", "purchaseActions");
                // Dim sMessageResponse As String
                string cProcessInfo = "";

                try
                {

                    if (!string.IsNullOrEmpty(moCartConfig["AccountingProvider"]))
                    {
                        object providerName = moCartConfig["AccountingProvider"];
                        Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection("protean/accountingProviders");
                        var assemblyInstance = Assembly.Load(moPrvConfig.Providers[providerName.ToString()].Type.ToString());
                        Type calledType;
                        string classPath = Convert.ToString(moPrvConfig.Providers[providerName.ToString()].Parameters["rootClass"]);

                        string passCMS = Convert.ToString(moPrvConfig.Providers[providerName.ToString()].Parameters["passCMS"]);

                        string methodName = "ProcessOrder";
                        calledType = assemblyInstance.GetType(classPath, true);
                        var o = Activator.CreateInstance(calledType);

                        var args = new object[1];

                        if (passCMS == "true")
                        {
                            args = new object[3];
                            args[0] = myWeb;
                            args[1] = oCartElmt;
                            args[2] = bRenderScriptOnly;
                        }
                        //else if (bRenderScriptOnly != null)
                        //{
                        //    args = new object[2];
                        //    args[0] = oCartElmt;
                        //    args[1] = bRenderScriptOnly;
                        //}
                        else
                        {
                            args[0] = oCartElmt;
                        }

                        if (oCartElmt.FirstChild.SelectSingleNode("Notes/PromotionalCode") != null)
                        {
                            moDiscount.RecordDiscountUsage(ref oCartElmt);
                        }
                        calledType.InvokeMember(methodName, BindingFlags.InvokeMethod, null, o, args);
                    }


                    foreach (XmlElement ocNode in oCartElmt.SelectNodes("descendant-or-self::Order/Item/productDetail[@purchaseAction!='']"))
                    {
                        string classPath = ocNode.GetAttribute("purchaseAction");
                        string assemblyName = ocNode.GetAttribute("assembly");
                        string providerName = ocNode.GetAttribute("providerName");
                        string assemblyType = ocNode.GetAttribute("assemblyType");

                        string methodName = classPath.Substring(classPath.LastIndexOf('.') + 1);
                        classPath = classPath.Substring(0, classPath.LastIndexOf('.'));

                        if (!string.IsNullOrEmpty(classPath))
                        {
                            try
                            {
                                Type calledType;

                                if (!string.IsNullOrEmpty(assemblyName))
                                {
                                    classPath = classPath + ", " + assemblyName;
                                }
                                // Dim oModules As New Protean.Cms.Membership.Modules

                                if (!string.IsNullOrEmpty(providerName))
                                {
                                    // case for external Providers
                                    Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection("protean/messagingProviders");
                                    var assemblyInstance = Assembly.Load(moPrvConfig.Providers[providerName].Type);
                                    calledType = assemblyInstance.GetType(classPath, true);
                                }

                                else if (!string.IsNullOrEmpty(assemblyType))
                                {
                                    // case for external DLL's
                                    var assemblyInstance = Assembly.Load(assemblyType);
                                    calledType = assemblyInstance.GetType(classPath, true);
                                }
                                else
                                {
                                    // case for methods within ProteanCMS Core DLL
                                    calledType = Type.GetType(classPath, true);
                                }

                                var o = Activator.CreateInstance(calledType);

                                var args = new object[2];
                                if (bRenderScriptOnly == true)
                                {
                                    args = new object[3];
                                    args[0] = myWeb;
                                    args[1] = ocNode;
                                    args[2] = bRenderScriptOnly;
                                }
                                else
                                {
                                    args[0] = myWeb;
                                    args[1] = ocNode;
                                }

                                calledType.InvokeMember(methodName, BindingFlags.InvokeMethod, null, o, args);
                            }

                            // Error Handling ?
                            // Object Clearup ?


                            catch (Exception)
                            {
                                // OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ContentActions", ex, sProcessInfo))
                                cProcessInfo = classPath + "." + methodName + " not found";
                                ocNode.InnerXml = "<Content type=\"error\"><div>" + cProcessInfo + "</div></Content>";
                            }
                        }

                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "purchaseActions", ex, "", cProcessInfo, gbDebug);
                }

            }

            public async Task SendPurchaseEventToGA4(string cartXml, string clientId = null)
            {
                string measurementId;
                string apiSecret;
                try
                {
                    if (moWebConfig["GoogleGA4MeasurementID"] != null && moWebConfig["GoogleGA4MeasurementID"] != "" && moWebConfig["GA4ApiSecret"] != null && moWebConfig["GA4ApiSecret"] != "")

                    {
                        measurementId = moWebConfig["GoogleGA4MeasurementID"];
                        apiSecret = moWebConfig["GA4ApiSecret"];
                        var xml = XDocument.Parse(cartXml);

                        var order = xml.Descendants("Order").FirstOrDefault();
                        if (order == null) return;


                        string transactionId = order.Attribute("InvoiceRef")?.Value;
                        double value = Convert.ToDouble(order.Attribute("totalNet")?.Value ?? "0");
                        double tax = Convert.ToDouble(order.Attribute("vatAmt")?.Value ?? "0");
                        double shipping = Convert.ToDouble(order.Attribute("shippingCost")?.Value ?? "0");
                        string currency = order.Attribute("currency")?.Value;


                        var items = xml.Descendants("Item").Select(x => new
                        {
                            item_id = x.Descendants("StockCode").FirstOrDefault()?.Value ?? x.Attribute("id")?.Value,
                            item_name = x.Descendants("Name").FirstOrDefault()?.Value,
                            item_brand = x.Descendants("Manufacturer").FirstOrDefault()?.Value,
                            price = Convert.ToDouble(x.Descendants("Price").FirstOrDefault()?.Value ?? "0"),
                            quantity = Convert.ToInt32(x.Attribute("quantity")?.Value ?? "1")
                        }).ToList();

                        //  fallback client id if no cookie
                        if (string.IsNullOrEmpty(clientId))
                            clientId = Guid.NewGuid().ToString();

                        //  Build GA4 payload
                        var payload = new
                        {
                            client_id = clientId,
                            events = new[]
                            {
                    new
                    {
                        name = "purchase",
                        @params = new
                        {
                            transaction_id = transactionId,
                            value = value,
                            currency = currency,
                            tax = tax,
                            shipping = shipping,
                            items = items
                        }
                    }
                        }
                        };

                        string url = $"https://www.google-analytics.com/mp/collect?measurement_id={measurementId}&api_secret={apiSecret}";

                        using (var client = new HttpClient())
                        {
                            var json = JsonConvert.SerializeObject(payload);
                            var content = new StringContent(json, Encoding.UTF8, "application/json");

                            var response = await client.PostAsync(url, content);

                            if (!response.IsSuccessStatusCode)
                            {
                                string error = await response.Content.ReadAsStringAsync();
                                //  LogError("GA4 Error: " + error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // LogError("GA4 Exception: " + ex.Message);
                }
            }

        }
    }
}
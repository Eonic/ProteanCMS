using System;
using System.Xml;
using Microsoft.VisualBasic.CompilerServices;

namespace Protean
{

    public partial class Cms
    {

        public partial class Cart
        {

            #region Module Behaviour

            public class Modules
            {

                public event OnErrorEventHandler OnError;

                public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
                private const string mcModuleName = "Protean.Cms.Membership.Modules";

                public Modules()
                {

                    // do nowt

                }

                public void ListOrders(ref Cms myWeb, ref XmlElement oContentNode)
                {
                    try
                    {
                        if (myWeb.mnUserId > 0)
                        {
                            myWeb.moDbHelper.mnUserId = (long)myWeb.mnUserId;
                            myWeb.moDbHelper.ListUserOrders(ref oContentNode, "Order");

                            if (!string.IsNullOrEmpty(myWeb.moRequest["OrderId"]))
                            {
                                Cart oCart;
                                oCart = new Cart(ref myWeb);
                                XmlElement argoPageDetail = null;
                                oCart.ListOrders(Conversions.ToInteger("0" + myWeb.moRequest["OrderId"]).ToString(),false,0, oPageDetail: ref argoPageDetail);
                                oCart = null;
                            }

                        }
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ListOrders", ex, ""));
                    }
                }


                public void ListQuotes(ref Cms myWeb, ref XmlElement oContentNode)
                {
                    try
                    {

                        if (myWeb.mnUserId > 0)
                        {
                            myWeb.moDbHelper.mnUserId = (long)myWeb.mnUserId;
                            myWeb.moDbHelper.ListUserOrders(ref oContentNode, "Quote");

                            if (!string.IsNullOrEmpty(myWeb.moRequest["QuoteId"]))
                            {
                                Cart oCart;
                                oCart = new Cart(ref myWeb);
                                XmlElement argoPageDetail = null;
                                oCart.ListOrders(Conversions.ToInteger("0" + myWeb.moRequest["QuoteId"]).ToString(),false,0, oPageDetail: ref argoPageDetail);
                                oCart = null;
                            }

                        }
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ListQuotes", ex, ""));
                    }
                }

                public void VoucherAction(ref Cms myWeb, ref XmlElement oContentNode)
                {
                    try
                    {

                        // Check codes available

                        switch (oContentNode.ParentNode.Name ?? "")
                        {
                            case "Item": // case for item in shopping cart
                                {

                                    int CodeGroup = Conversions.ToInteger("0" + oContentNode.SelectSingleNode("CodeGroup").InnerText);

                                    if (CodeGroup > 0)
                                    {
                                        // Save with current stock available
                                        string sSql = "select count(nCodeKey) from tblCodes where nUseId is null and nIssuedDirId is null and nCodeParentId = " + CodeGroup.ToString();
                                        int codesAvailable = Conversions.ToInteger(myWeb.moDbHelper.ExeProcessSqlScalar(sSql));
                                        var stockElmt = myWeb.moPageXml.CreateElement("Stock");
                                        stockElmt.InnerText = ((int)Math.Round(Conversions.ToDouble("0") + codesAvailable)).ToString();
                                        oContentNode.AppendChild(stockElmt);

                                        // getQuantity
                                        XmlElement ItemParent = (XmlElement)oContentNode.ParentNode;
                                        int VoucherQuantity = Conversions.ToInteger(ItemParent.GetAttribute("quantity"));
                                        int i;
                                        var loopTo = VoucherQuantity;
                                        for (i = 1; i <= loopTo; i++)
                                        {
                                            // Get Any Product Specific Notes that have been completed and save against code
                                            string productName = oContentNode.SelectSingleNode("Name").InnerText;
                                            string stockCode = oContentNode.SelectSingleNode("StockCode").InnerText;
                                            XmlElement copyContentNode = (XmlElement)oContentNode.CloneNode(true);

                                            XmlElement oNotes = (XmlElement)oContentNode.ParentNode.ParentNode.SelectSingleNode("Notes/descendant-or-self::Item[@name='" + productName + "-" + stockCode + "' and @number='" + i + "']");
                                            if (oNotes != null)
                                            {
                                                copyContentNode.AppendChild(oNotes.CloneNode(true));
                                            }

                                            string newcode = "";
                                            if (myWeb.mnUserId != 0)
                                            {
                                                newcode = myWeb.moDbHelper.IssueCode(CodeGroup, myWeb.mnUserId, false, copyContentNode);
                                            }
                                            // Save Issued Code back to user
                                            var codeElmt = myWeb.moPageXml.CreateElement("IssuedCode");
                                            codeElmt.InnerText = newcode;
                                            oContentNode.AppendChild(codeElmt);
                                        }

                                    }

                                    break;
                                }



                            case "Contents":
                                {

                                    int CodeGroup = Conversions.ToInteger("0" + oContentNode.SelectSingleNode("CodeGroup").InnerText);

                                    if (CodeGroup > 0)
                                    {
                                        string sSql = "select count(nCodeKey) from tblCodes where nUseId is null and nCodeParentId = " + CodeGroup.ToString();
                                        int codesAvailable = Conversions.ToInteger(myWeb.moDbHelper.ExeProcessSqlScalar(sSql));
                                        var stockElmt = myWeb.moPageXml.CreateElement("Stock");
                                        stockElmt.InnerText = ((int)Math.Round(Conversions.ToDouble("0") + codesAvailable)).ToString();
                                        oContentNode.AppendChild(stockElmt);
                                    }

                                    break;
                                }

                            case "ContentDetail":
                                {
                                    break;
                                }

                        }
                    }


                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ListQuotes", ex, ""));
                    }
                }


                public void ManageVouchers(ref Cms myWeb, ref XmlElement oContentNode)
                {
                    try
                    {
                        if (myWeb.mnUserId > 0)
                        {
                            myWeb.moDbHelper.mnUserId = (long)myWeb.mnUserId;
                            myWeb.moDbHelper.ListUserVouchers(ref oContentNode);
                        }

                        if (!string.IsNullOrEmpty(myWeb.moRequest["VoucherId"]))
                        {
                            // Edit Voucher Code here....
                            Admin.AdminXforms moAdXfm = myWeb.getAdminXform();

                            moAdXfm.xFrmVoucherCode(Convert.ToInt32(myWeb.moRequest["VoucherId"]));

                            if (!moAdXfm.valid)
                            {
                                if (myWeb.moContentDetail is null)
                                {
                                    myWeb.moContentDetail = myWeb.moPageXml.CreateElement("ContentDetail");
                                    myWeb.moPageXml.DocumentElement.AppendChild(myWeb.moContentDetail);
                                }
                                myWeb.moContentDetail.AppendChild(moAdXfm.moXformElmt);
                            }

                        }
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ListQuotes", ex, ""));
                    }
                }

                public void RedeemTickets(ref Cms myWeb, ref XmlElement oContentNode)
                {
                    try
                    {

                        // Dim redeemticketsGroupId = myWeb.moConfig("TicketOfficeGroupId")
                        long userId = (long)myWeb.mnUserId;
                        bool mbIsTicketOffice = myWeb.moDbHelper.checkUserRole("Ticket Office", "Role", userId);

                        oContentNode.RemoveAttribute("ticketValid");
                        oContentNode.SetAttribute("enteredTicketCode", myWeb.moRequest["code"]);

                        string tktCode = myWeb.moRequest["code"];
                        string tktKey = string.Empty;

                        // get the key of the ticketcode - join against cartitem and cartorder
                        string cdchkStr = "select tblCodes.nCodeKey, tblCodes.dUseDate, tblCartOrder.nCartOrderKey, tblCartItem.nCartItemKey from tblCodes inner join tblCartItem on tblCodes.nUseId = tblCartItem.nCartItemKey ";
                        cdchkStr = cdchkStr + "inner join tblCartOrder on tblCartItem.nCartOrderId = tblCartOrder.nCartOrderKey ";
                        cdchkStr = cdchkStr + "where nCartStatus in (6, 9 ,17) and tblCodes.cCode = '" + tktCode + "'";
                        using (var oDr = myWeb.moDbHelper.getDataReaderDisposable(cdchkStr))  // Done by nita on 6/7/22
                        {

                            oContentNode.SetAttribute("ticketValid", "invalid");

                            while (oDr.Read())
                            {
                                if (oDr["dUseDate"] is DBNull) // tkt has not been validated yet
                                {
                                    // get event name, datetime and purchaser name
                                    string tktDet = "select (CAST(cCartXml AS XML)).value('/Order[1]/Contact[1]/GivenName[1]', 'VARCHAR(255)') AS 'PurchaserName',";
                                    tktDet += "(CAST(xItemXml AS XML)).value('/Content[1]/Name[1]', 'VARCHAR(255)') AS 'EventName', (CAST(xItemXml AS XML)).value('/Content[1]/Description[1]', 'VARCHAR(1000)') AS 'Venue',";
                                    tktDet += "(CAST(xItemXml AS XML)).value('/Content[1]/StartDate[1]', 'VARCHAR(255)') + ' ' + (CAST(xItemXml AS XML)).value('/Content[1]/Times[1]/@start', 'VARCHAR(255)') AS 'Time',";
                                    tktDet += "(CAST(xItemXml AS XML)).value('/Content[1]/StartDate[1]', 'VARCHAR(255)') AS 'EventDate'";
                                    tktDet += " From tblCartItem inner Join tblCartOrder On tblCartOrder.nCartOrderKey = tblCartItem.nCartOrderId";
                                    tktDet = Conversions.ToString(tktDet + Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(" Where tblCartOrder.nCartOrderKey = ", oDr["nCartOrderKey"]), " and tblCartItem.nCartItemKey = "), oDr["nCartItemKey"]));
                                    using (var oDr1 = myWeb.moDbHelper.getDataReaderDisposable(tktDet))  // Done by nita on 6/7/22
                                    {
                                        while (oDr1.Read())
                                        {
                                            oContentNode.SetAttribute("PurchaserName", Conversions.ToString(oDr1["PurchaserName"]));
                                            oContentNode.SetAttribute("EventName", Conversions.ToString(oDr1["EventName"]));
                                            oContentNode.SetAttribute("Venue", Conversions.ToString(oDr1["Venue"]));
                                            oContentNode.SetAttribute("Time", Conversions.ToString(oDr1["Time"]));

                                            DateTime eDay = Conversions.ToDate(oDr1["EventDate"]);
                                            if (eDay != DateTime.Today)
                                            {
                                                oContentNode.SetAttribute("ticketValid", "notToday");
                                                return;
                                            }
                                        }
                                    }
                                    if (mbIsTicketOffice)
                                    {
                                        if (myWeb.moDbHelper.RedeemCode(myWeb.moRequest["code"])) // if ticket got validated successfully
                                        {
                                            oContentNode.SetAttribute("ticketValid", "validated");
                                        }
                                    }
                                    else
                                    {
                                        oContentNode.SetAttribute("ticketValid", "valid");
                                    }
                                }
                                else
                                {
                                    string useStr = "select top 1 dUseDate from tblCodes where cCode = '" + tktCode + "'";
                                    using (var oDr2 = myWeb.moDbHelper.getDataReaderDisposable(useStr))  // Done by nita on 6/7/22
                                    {
                                        while (oDr2.Read())
                                            oContentNode.SetAttribute("lastUsedTime", Conversions.ToString(oDr2["dUseDate"]));
                                    }
                                    oContentNode.SetAttribute("ticketValid", "used");
                                }
                            }


                        }
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ListQuotes", ex, ""));
                    }
                }


            }
            #endregion
        }

    }
}
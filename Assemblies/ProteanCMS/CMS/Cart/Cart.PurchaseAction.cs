using System;
using System.Web.Configuration;
using System.Xml;
using Microsoft.VisualBasic.CompilerServices;

namespace Protean
{


    public partial class Cms
    {

        public partial class Cart
        {

            #region JSON Actions

            public class PurchaseAction
            {
                public event OnErrorEventHandler OnError;

                public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
                private const string mcModuleName = "Protean.Cms.Cart.PurchaseActions";
                private System.Collections.Specialized.NameValueCollection moLmsConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/lms");
                private Cms myWeb;
                private Cart myCart;

                public PurchaseAction()
                {
                    string ctest = "this constructor is being hit"; // for testing
                    myWeb = new Cms();
                    myWeb.InitializeVariables();
                    myWeb.Open();
                    myCart = new Cart(ref myWeb);

                }



                public bool IssueTickets(ref Cms myWeb, ref XmlElement oCartItemProductDetailXml)
                {
                    try
                    {
                        XmlElement cartItem = (XmlElement)oCartItemProductDetailXml.ParentNode;

                        short CodeSetId = (short)Conversions.ToInteger("0" + oCartItemProductDetailXml.GetAttribute("codeBank"));
                        short Quantity = (short)Conversions.ToInteger(cartItem.GetAttribute("quantity"));
                        short thisQty;
                        short CartItemId = (short)Conversions.ToInteger(cartItem.GetAttribute("id"));
                        if (CodeSetId > 0)
                        {
                            for (thisQty = Quantity; thisQty >= 1; thisQty += -1)
                            {
                                // fix to set xIssueDate - earlier it was put to xUseDate
                                string Code = myWeb.moDbHelper.IssueCode((int)CodeSetId, (int)CartItemId, false, (XmlElement)null);
                                var TicketElement = oCartItemProductDetailXml.OwnerDocument.CreateElement("Ticket");
                                TicketElement.SetAttribute("code", Code);
                                oCartItemProductDetailXml.AppendChild(TicketElement);
                            }
                        }
                        string copyNode = oCartItemProductDetailXml.OuterXml;

                        copyNode = copyNode.Replace("<productDetail", "<Content");
                        copyNode = copyNode.Replace("</productDetail>", "</Content>");

                        myWeb.moDbHelper.updateInstanceField(Cms.dbHelper.objectTypes.CartItem, (int)CartItemId, "xItemXml", copyNode.Replace("&lt;", "<").Replace("&gt;", ">"));

                        myWeb.moCart.SaveCartXML((XmlElement)cartItem.ParentNode);
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "IssueTickets", ex, ""));
                        return Conversions.ToBoolean(ex.Message);
                    }

                    return default;
                }


            }

            #endregion
        }

    }
}
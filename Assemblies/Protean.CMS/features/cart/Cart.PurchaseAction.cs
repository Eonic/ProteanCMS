using System;
using System.Web.Configuration;
using System.Xml;

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
                    //string ctest = "this constructor is being hit"; // for testing
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
                        short CartItemId = (short)Convert.ToInt16(cartItem.GetAttribute("id"));

                        if (oCartItemProductDetailXml.SelectSingleNode("IssueCodes") != null)
                        {
                            foreach (XmlNode codeNode in oCartItemProductDetailXml.SelectNodes("IssueCodes/code"))
                            {
                                XmlElement codeElmt = (XmlElement)codeNode;

                                short CodeSetId = (short)Convert.ToInt16("0" + codeElmt.GetAttribute("codeBank"));
                                short Quantity = (short)(Convert.ToInt16(cartItem.GetAttribute("quantity")) + Convert.ToInt16(codeElmt.GetAttribute("noOfCodes")));
                                string SetName = cartItem.GetAttribute("name");
                                AddCode(ref oCartItemProductDetailXml, CartItemId, CodeSetId, Quantity, SetName);
                            }
                        }
                        else
                        {

                            short CodeSetId = (short)Convert.ToInt16("0" + oCartItemProductDetailXml.GetAttribute("codeBank"));
                            short Quantity = (short)Convert.ToInt16(cartItem.GetAttribute("quantity"));
                            AddCode(ref oCartItemProductDetailXml, CartItemId, CodeSetId, Quantity, "");
                        }
                        myWeb.moCart.SaveCartXML((XmlElement)cartItem.ParentNode);
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "IssueTickets", ex, ""));
                        return Convert.ToBoolean(ex.Message);
                    }

                    return default;
                }



                private void AddCode(ref XmlElement ProductXml, int CartItemId, long CodeSetId, short Quantity, string codeName)
                {
                    try
                    {
                        short thisQty;
                        if (CodeSetId > 0)
                        {
                            for (thisQty = Quantity; thisQty >= 1; thisQty += -1)
                            {
                                // fix to set xIssueDate - earlier it was put to xUseDate
                                string Code = myWeb.moDbHelper.IssueCode((int)CodeSetId, (int)CartItemId, false, (XmlElement)null);
                                var TicketElement = ProductXml.OwnerDocument.CreateElement("Ticket");
                                TicketElement.SetAttribute("code", Code);
                                TicketElement.SetAttribute("code", codeName);
                                ProductXml.AppendChild(TicketElement);
                            }
                        }
                        string copyNode = ProductXml.OuterXml;

                        copyNode = copyNode.Replace("<productDetail", "<Content");
                        copyNode = copyNode.Replace("</productDetail>", "</Content>");

                        myWeb.moDbHelper.updateInstanceField(Cms.dbHelper.objectTypes.CartItem, (int)CartItemId, "xItemXml", copyNode.Replace("&lt;", "<").Replace("&gt;", ">"));
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddCode", ex, ""));
                        // return Convert.ToBoolean(ex.Message);
                    }

                }
                #endregion
            }

        }
    }
}
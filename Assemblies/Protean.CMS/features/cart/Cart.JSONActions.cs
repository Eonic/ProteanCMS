
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Protean.Providers.Messaging;
using Protean.Providers.Payment;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Xml;
using static Protean.Tools.Xml;

namespace Protean
{
    public partial class Cms
    {
        public partial class Cart
        {
            #region JSON Actions
            public class LocationList
            {
                public string Text { get; set; }
                public string Value { get; set; }
            }

            public class JSONActions : Protean.rest.JSONActions
            {

                //public event OnErrorEventHandler OnError;

                //  public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
                private const string mcModuleName = "Eonic.Cart.JSONActions";
                private const string cContactType = "Venue";
                private System.Collections.Specialized.NameValueCollection moWebConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
                private Cms myWeb;
                private Cart myCart;
                public bool bNoClose;

                public JSONActions(Cms.dbHelper.utils.APILog ApiLog)
                {
                    // string ctest = "this constructor is being hit"; // for testing
                    myWeb = new Cms();
                    myWeb.InitializeVariables();
                    myWeb.Open();
                    myCart = new Cart(ref myWeb);
                    this.apiLog = ApiLog;

                }



                private XmlElement updateCartforJSON(XmlElement CartXml)
                {
                    string newstring = CartXml.InnerXml.Replace("<Item ", "<CartItem ").Replace("</Item>", "</CartItem>");
                    CartXml.InnerXml = newstring;
                    var cartItems = myWeb.moPageXml.CreateElement("CartItems");
                    short ItemCount = 0;

                    foreach (XmlElement oItem in CartXml.SelectNodes("Order/CartItem"))
                    {
                        cartItems.AppendChild(oItem);
                        ItemCount = (short)(ItemCount + 1);
                    }

                    if (ItemCount == 1)
                    {
                        var oItems = myWeb.moPageXml.CreateElement("CartItem");
                        oItems.SetAttribute("dummy", "true");
                        cartItems.AppendChild(oItems);
                    }
                    CartXml.FirstChild.AppendChild(cartItems);

                    TidyHtmltoCData(ref CartXml);

                    return CartXml;
                }

                public string GetCart(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {
                        string cProcessInfo = string.Empty;

                        // Dim CartXml As XmlElement = myWeb.moCart.CreateCartElement(myWeb.moPageXml)
                        // myCart.GetCart(CartXml.FirstChild)
                        string cShipOptKey = "0";


                        XmlElement CartXml = (XmlElement)myWeb.moCart.CreateCartElement(myWeb.moPageXml);
                        XmlElement argoCartElmt = (XmlElement)CartXml.FirstChild;
                        myCart.GetCart(ref argoCartElmt);

                        if (CartXml.SelectSingleNode("Order") != null && CartXml.SelectSingleNode("Order").Attributes["shippingType"] != null)
                        {
                            cShipOptKey = CartXml.SelectSingleNode("Order").Attributes["shippingType"].Value;
                            myCart.updateGCgetValidShippingOptionsDS(cShipOptKey);
                            argoCartElmt = (XmlElement)CartXml.FirstChild;
                            myCart.GetCart(ref argoCartElmt);
                        }
                        CartXml = updateCartforJSON(CartXml);

                        string jsonString = JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.Indented);
                        jsonString = jsonString.Replace("\"@", "\"_");
                        jsonString = jsonString.Replace("#cdata-section", "cDataValue");
                        // persist cart
                        myCart.close();
                        return jsonString;
                    }

                    catch (Exception ex)
                    {
                        RaiseOnError(new Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""));
                        return ex.Message;
                    }
                }

                public string AddItems(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {
                        string cProcessInfo = string.Empty;
                        // jsonObject("artId")
                        // myCart.AddItem()
                        // Output the new cart
                        var oDoc = new XmlDocument();
                        XmlElement CartXml = (XmlElement)myWeb.moCart.CreateCartElement(myWeb.moPageXml);
                        // myCart.bNoClose = true;

                        if (myCart.mnCartId < 1)
                        {
                            myCart.CreateNewCart(ref CartXml, "Order");
                            if (!string.IsNullOrEmpty(myCart.mcItemOrderType))
                            {
                                myCart.mmcOrderType = myCart.mcItemOrderType;
                            }
                            else
                            {
                                myCart.mmcOrderType = "";
                            }
                            myCart.mnProcessId = (short)1;
                        }
                        var cBlockCartUpdate = myCart.GetBlockCartUpdatesConfig();

                        if ((int)myCart.mnProcessId > 4 &&
                            !string.Equals(cBlockCartUpdate?.Trim(), "off", StringComparison.OrdinalIgnoreCase))
                        {
                            return "";
                        }
                        else
                        {
                            if (jObj["Item"] != null)
                            {
                                foreach (JObject item in jObj["Item"])
                                {
                                    bool bUnique = false;
                                    double cProductPrice = 0d;
                                    string sProductName = "";
                                    //bool bPackegingRequired = false;
                                    string sOverideURL = "";
                                    string sProductOptionName = "";
                                    double dProductOptionPrice = 0d;
                                    string[][] aProductOptions = null;

                                    if (item.ContainsKey("UniqueProduct"))
                                    {
                                        bUnique = (bool)item["UniqueProduct"];
                                    }
                                    if (item.ContainsKey("itemPrice"))
                                    {
                                        cProductPrice = (double)item["itemPrice"];
                                    }
                                    if (item.ContainsKey("productName"))
                                    {
                                        sProductName = (string)item["productName"];
                                    }
                                    if (item.ContainsKey("url"))
                                    {
                                        sOverideURL = (string)item["url"];
                                    }

                                    if (item.ContainsKey("productOption"))
                                    {
                                        sProductOptionName = (string)item["productOption"];
                                    }
                                    if (item.ContainsKey("productOptions"))
                                    {
                                        string productOptionsString = (string)item["productOptions"];
                                        if (!string.IsNullOrEmpty(productOptionsString))
                                        {
                                            string[] optionPairs = productOptionsString.Split(',');
                                            aProductOptions = new string[optionPairs.Length][];
                                            for (int i = 0; i < optionPairs.Length; i++)
                                            {
                                                aProductOptions[i] = optionPairs[i].Split('_');
                                            }
                                        }
                                    }

                                    if (item.ContainsKey("productOptionPrice"))
                                    {
                                        dProductOptionPrice = (double)item["productOptionPrice"];
                                    }
                                    if (jObj.ContainsKey("overridePriceSession"))
                                    {
                                        myCart.myWeb.moSession["overridePriceSession"] = (string)jObj["overridePriceSession"];
                                    }

                                    myCart.AddItem((long)item["contentId"], (long)item["qty"], aProductOptions, sProductName, cProductPrice, "", bUnique, sOverideURL, false, sProductOptionName, dProductOptionPrice);

                                }
                            }

                            //Reset the cart shipping option so new value is calculated
                            myCart.updateOrderShippingOption(myCart.mnCartId, 0);

                            // Output the new cart
                            XmlElement argoCartElmt = (XmlElement)CartXml.FirstChild;
                            myCart.GetCart(ref argoCartElmt);
                            CartXml = updateCartforJSON(CartXml);
                            // persist cart

                            myCart.close(bNoClose);

                            string jsonString = JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.None);
                            jsonString = jsonString.Replace("\"@", "\"_");
                            jsonString = jsonString.Replace("#cdata-section", "cDataValue");

                            return jsonString;
                        }
                    }
                    catch (Exception ex)
                    {
                        RaiseOnError(new Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""));
                        return ex.Message;
                    }

                }

                public string RemoveItems(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {
                        string mcBlockCartUpdate = myCart.GetBlockCartUpdatesConfig();
                        if ((int)myCart.mnProcessId > 4 &&
                            !string.Equals(mcBlockCartUpdate?.Trim(), "off", StringComparison.OrdinalIgnoreCase))
                        {
                            return "";
                        }
                        else
                        {
                            string cProcessInfo = string.Empty;
                            long ItemCount = 1L;

                            foreach (JObject item in jObj["Item"])
                            {
                                if (item["contentId"] is null)
                                {
                                    ItemCount = (long)myCart.RemoveItem((long)item["itemId"], 0L);
                                }
                                else
                                {
                                    ItemCount = (long)myCart.RemoveItem(0L, (long)item["contentId"]);
                                }
                            }


                            if (ItemCount == 0L)
                            {
                                myCart.QuitCart();
                                myCart.EndSession();
                            }


                            //Reset the cart shipping option so new value is calculated
                            myCart.updateOrderShippingOption(myCart.mnCartId, 0);
                            // Output the new cart   
                            XmlElement CartXml = (XmlElement)myWeb.moCart.CreateCartElement(myWeb.moPageXml);
                            XmlElement argoCartElmt = (XmlElement)CartXml.FirstChild;
                            myCart.GetCart(ref argoCartElmt);
                            // persist cart
                            myCart.close();
                            CartXml = updateCartforJSON(CartXml);

                            string jsonString = JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.Indented);
                            jsonString = jsonString.Replace("\"@", "\"_");
                            jsonString = jsonString.Replace("#cdata-section", "cDataValue");
                            return jsonString;
                        }
                    }
                    catch (Exception ex)
                    {
                        RaiseOnError(new Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""));
                        return ex.Message;
                    }

                }

                public string UpdateItems(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {

                        string cProcessInfo = string.Empty;
                        long ItemCount = 1L;
                        XmlElement CartXml = (XmlElement)myWeb.moCart.CreateCartElement(myWeb.moPageXml);

                        if (myCart.mnCartId < 1)
                        {
                            myCart.CreateNewCart(ref CartXml);
                            if (!string.IsNullOrEmpty(myCart.mcItemOrderType))
                            {
                                myCart.mmcOrderType = myCart.mcItemOrderType;
                            }
                            else
                            {
                                myCart.mmcOrderType = "";
                            }
                            myCart.mnProcessId = (short)1;
                        }

                        foreach (JObject item in jObj["Item"])
                        {
                            if (item["contentId"] is null)
                            {
                                if ((string)item["qty"] == "0")
                                {
                                    ItemCount = (long)myCart.RemoveItem((long)item["itemId"], 0L);
                                }
                                else
                                {
                                    ItemCount = (long)myCart.UpdateItem((long)item["itemId"], 0L, (long)item["qty"], (bool)item["skipPackaging"]);
                                }
                            }
                            else if ((string)item["qty"] == "0")
                            {
                                ItemCount = (long)myCart.RemoveItem(0L, (long)item["contentId"]);
                            }
                            else
                            {
                                ItemCount = (long)myCart.UpdateItem(0L, (long)item["contentId"], (long)item["qty"]);
                            }
                        }

                        if (ItemCount == 0L)
                        {
                            myCart.QuitCart();
                            myCart.EndSession();
                        }
                        //Reset the cart shipping option so new value is calculated
                        myCart.updateOrderShippingOption(myCart.mnCartId, 0);
                        // Output the new cart
                        XmlElement argoCartElmt = (XmlElement)CartXml.FirstChild;
                        myCart.GetCart(ref argoCartElmt);
                        // persist cart
                        myCart.close();
                        CartXml = updateCartforJSON(CartXml);

                        string jsonString = JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.Indented);
                        jsonString = jsonString.Replace("\"@", "\"_");
                        jsonString = jsonString.Replace("#cdata-section", "cDataValue");
                        return jsonString;
                    }

                    catch (Exception ex)
                    {
                        RaiseOnError(new Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""));
                        return ex.Message;
                    }

                }

                public string UpdateShippingOption(Protean.rest myApi, Newtonsoft.Json.Linq.JObject jObj)
                {
                    Newtonsoft.Json.Linq.JObject json = jObj;
                    Protean.Cms.Cart.JSONActions jSONActions = new Protean.Cms.Cart.JSONActions(apiLog);
                    long nCartOrderId = Convert.ToInt64(json.SelectToken("CartOrderId"));
                    int nShippingKey = Convert.ToInt32(json.SelectToken("ShipOptKey").ToString());
                    myCart.updateOrderShippingOption(nCartOrderId, nShippingKey);

                    return jSONActions.GetCart(ref myApi, ref jObj);
                }

                public string GetShippingOptions(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {
                        string cProcessInfo = string.Empty;
                        DataSet dsShippingOption;

                        string cDestinationCountry = myCart.moCartConfig["DefaultDeliveryCountry"];
                        // call it from cart
                        var nAmount = default(long);
                        var nQuantity = default(long);
                        var nWeight = default(long);
                        string promocode = "";

                        if (jObj != null)
                        {
                            if ((string)jObj["country"] != "")
                            {
                                cDestinationCountry = (string)jObj["country"];
                            }
                            else
                            {
                                cDestinationCountry = myCart.moCartConfig["DefaultDeliveryCountry"];
                            }

                            if ((string)jObj["qty"] == "0")
                            {
                                nQuantity = (long)jObj["qty"];
                            }
                            else
                            {
                                nQuantity = 0L;
                            }

                            if ((string)jObj["amount"] == "0")
                            {
                                nAmount = (long)jObj["amount"];
                            }
                            else
                            {
                                nAmount = 0L;
                            }

                            if ((string)jObj["Weight"] == "0")
                            {
                                nWeight = (long)jObj["Weight"];
                            }
                            else
                            {
                                nWeight = 0L;
                            }

                            if (jObj["promocode"] != null)
                            {
                                promocode = (string)jObj["promocode"];
                            }
                            else
                            {
                                promocode = "";
                            }
                        }

                        dsShippingOption = myCart.getValidShippingOptionsDS(cDestinationCountry, (double)nAmount, nQuantity, (double)nWeight, promocode);

                        string ShippingOptionXml = dsShippingOption.GetXml();
                        var xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(ShippingOptionXml);


                        string jsonString = JsonConvert.SerializeXmlNode(xmlDoc.DocumentElement, Newtonsoft.Json.Formatting.Indented);
                        return jsonString.Replace("\"@", "\"_");
                    }


                    catch (Exception ex)
                    {
                        RaiseOnError(new Tools.Errors.ErrorEventArgs(mcModuleName, "GetShippingOptions", ex, ""));
                        return ex.Message;
                    }
                }

                public string UpdatedCartShippingOptions(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {

                        string cProcessInfo = string.Empty;
                        string ShipOptKey;
                        var json = jObj;
                        ShipOptKey = (string)json.SelectToken("ShipOptKey");
                        myCart.updateGCgetValidShippingOptionsDS(ShipOptKey);
                        XmlElement CartXml = (XmlElement)myWeb.moCart.CreateCartElement(myWeb.moPageXml);
                        XmlElement argoCartElmt = (XmlElement)CartXml.FirstChild;
                        myCart.GetCart(ref argoCartElmt);

                        // persist cart
                        myCart.close();
                        CartXml = updateCartforJSON(CartXml);

                        string jsonString = JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.Indented);
                        jsonString = jsonString.Replace("\"@", "\"_");
                        jsonString = jsonString.Replace("#cdata-section", "cDataValue");
                        return jsonString;
                    }


                    catch (Exception ex)
                    {
                        RaiseOnError(new Tools.Errors.ErrorEventArgs(mcModuleName, "UpdatedCartShippingOptions", ex, ""));
                        return ex.Message;
                    }

                }

                public string UpdateDeliveryOptionByCountry(ref Protean.rest myApi, ref JObject jObj)
                {
                    string mcBlockCartUpdate = myCart.GetBlockCartUpdatesConfig();
                    if ((int)myCart.mnProcessId > 4 &&
                        !string.Equals(mcBlockCartUpdate?.Trim(), "off", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(mcBlockCartUpdate))
                    {
                        return "";
                    }
                    else
                    {

                        string country = string.Empty;
                        string cOrderofDeliveryOption = myCart.moCartConfig["ShippingTotalIsNotZero"];
                        XmlElement CartXml = (XmlElement)myWeb.moCart.CreateCartElement(myWeb.moPageXml);
                        if (jObj["country"] != null)
                        {
                            if ((string)jObj["country"] != string.Empty)
                            {
                                country = (string)jObj["country"];
                            }
                        }
                        if (jObj["ShipOptKey"] != null)
                        {
                            if ((string)jObj["ShipOptKey"] != string.Empty)
                            {
                                cOrderofDeliveryOption = (string)jObj["ShipOptKey"];
                            }
                        }
                        // check config setting here so that it will take order option which is optional.

                        XmlElement argoCartElmt = (XmlElement)CartXml.FirstChild;
                        cOrderofDeliveryOption = myCart.UpdateDeliveryOptionByCountry(ref argoCartElmt, country, cOrderofDeliveryOption);
                        if (!string.IsNullOrEmpty(myCart.CheckPromocodeAppliedForDelivery()))
                        {
                            RemoveDiscountCode(ref myApi, ref jObj);
                            // this will remove discount section from address page in vuemain.js
                            cOrderofDeliveryOption = cOrderofDeliveryOption + "#1" + "#" + myCart.mnCartId;
                        }
                        else
                        {
                            cOrderofDeliveryOption = cOrderofDeliveryOption + "#0" + "#" + myCart.mnCartId;
                        }

                        return cOrderofDeliveryOption;
                    }
                }

                public string GetContacts(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {
                        string JsonResult = "";
                        string dirId = (string)jObj["dirId"];
                        // Dim offerId As String = jObj("offerId")

                        object userContacts = myWeb.moDbHelper.GetUserContactsXml(Convert.ToInt32(dirId));
                        JsonResult = JsonConvert.SerializeObject(userContacts);
                        return JsonResult;
                    }

                    catch (Exception ex)
                    {
                        RaiseOnError(new Tools.Errors.ErrorEventArgs(mcModuleName, "GetLocations", ex, ""));
                        return ex.Message;
                    }
                }

                public string GetContactForm(ref Protean.rest myApi, ref JObject jObj)
                {
                    //int nId;
                    try
                    {

                        string JsonResult = "";
                        XmlDocument oDdoc = new XmlDocument();
                        int contactId = (int)jObj["contactId"];
                        string cAddressType = (string)jObj["addressType"];

                        var oForm = myCart.contactXform(cAddressType, "", "");
                        string oFormXml = oForm.Instance.SelectSingleNode("tblCartContact").OuterXml;

                        oDdoc.LoadXml(oFormXml);
                        JsonResult = JsonConvert.SerializeXmlNode((XmlNode)oDdoc);
                        return JsonResult;
                    }
                    catch (Exception ex)
                    {
                        RaiseOnError(new Tools.Errors.ErrorEventArgs(mcModuleName, "GetLocations", ex, ""));
                        return ex.Message;
                    }
                    //return JsonConvert.ToString(nId);
                }

                public string SetContact(ref Protean.rest myApi, ref JObject jObj)
                {
                    int nId;
                    try
                    {
                        int supplierId = (int)jObj["supplierId"];
                        var contact = jObj["venue"].ToObject<model.Contact>();
                        contact.cContactType = cContactType;
                        contact.cContactForeignRef = string.Format("SUP-{0}", supplierId);

                        nId = myWeb.moDbHelper.SetContact(ref contact);
                    }
                    catch (Exception ex)
                    {
                        RaiseOnError(new Tools.Errors.ErrorEventArgs(mcModuleName, "AddContact", ex, ""));
                        return ex.Message;
                    }
                    return JsonConvert.ToString(nId);
                }

                public string DeleteContact(ref Protean.rest myApi, ref JObject jObj)
                {
                    bool isSuccess;
                    try
                    {
                        string cContactKey = (string)jObj["nContactKey"];
                        int argnContactKey = Convert.ToInt32(cContactKey);
                        isSuccess = myWeb.moDbHelper.DeleteContact(ref argnContactKey);
                        cContactKey = argnContactKey.ToString();
                    }
                    catch (Exception ex)
                    {
                        RaiseOnError(new Tools.Errors.ErrorEventArgs(mcModuleName, "DeleteContact", ex, ""));
                        return ex.Message;
                    }
                    return JsonConvert.ToString(isSuccess);
                }

                public string AddProductOption(Protean.rest myApi, JObject jObj)
                {
                    string jsonString = string.Empty;

                    try
                    {


                        XmlElement CartXml = (XmlElement)myWeb.moCart.CreateCartElement(myWeb.moPageXml);
                        // myCart.GetCart(CartXml.FirstChild)

                        // add product option
                        myCart.AddProductOption(jObj);
                        // myCart.UpdatePackagingANdDeliveryType()
                        // myCart.GetCart(CartXml.FirstChild)   //Comment out this extra called method because this code already added in UpdatePackagingDeliveryOptions method - change on 5th jan 23
                        /// persist cart
                        //myCart.close();

                        // CartXml = updateCartforJSON(CartXml)

                        // jsonString = Newtonsoft.Json.JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.Indented)
                        // jsonString = jsonString.Replace("""@", """_")
                        // jsonString = jsonString.Replace("#cdata-section", "cDataValue")
                        return jsonString;
                    }

                    catch (Exception)
                    {
                        return null;
                    }
                }


                public string UpdatePackagingForRemovingFreeGiftDiscount(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {
                        myCart.moDiscount.RemoveDiscountCode();
                        // update packaging while removing giftbox promocode
                        myCart.updatePackagingForRemovingFreeGiftDiscount((string)jObj["CartOrderId"], (decimal)jObj["AmountToDiscount"]);

                        return "True";
                    }

                    catch (Exception)
                    {
                        return null;
                    }

                }

                public string AddDiscountCode(ref Protean.rest myApi, ref JObject jObj)
                {
                    string strMessage = string.Empty;
                    try
                    {
                        string mcBlockCartUpdate = myCart.GetBlockCartUpdatesConfig();

                        if ((int)myCart.mnProcessId > 4 &&
                            !string.Equals(mcBlockCartUpdate?.Trim(), "off", StringComparison.OrdinalIgnoreCase))
                        {
                            return "";
                        }
                        else
                        {
                            XmlElement CartXml = (XmlElement)myWeb.moCart.CreateCartElement(myWeb.moPageXml);

                            string jsonString = string.Empty;
                            if (jObj["Code"] != null)
                            {
                                strMessage = myCart.moDiscount.AddDiscountCode((string)jObj["Code"]);
                                if (strMessage == (string)jObj["Code"])
                                {
                                    XmlElement argoCartElmt = (XmlElement)CartXml.FirstChild;
                                    myCart.GetCart(ref argoCartElmt);
                                    // persist cart
                                    myCart.close();
                                    CartXml = updateCartforJSON(CartXml);

                                    // jsonString = Newtonsoft.Json.JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.Indented)
                                    // jsonString = jsonString.Replace("""@", """_")
                                    // jsonString = jsonString.Replace("#cdata-section", "cDataValue")

                                }
                                if (!string.IsNullOrEmpty(strMessage))
                                {
                                    return strMessage;
                                }

                            }
                            return strMessage;
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }

                public string RemoveDiscountCode(ref Protean.rest myApi, ref JObject jObj)
                {
                    string jsonString = string.Empty;
                    try
                    {
                        var cBlockCartUpdate = myCart.GetBlockCartUpdatesConfig();

                        if ((int)myCart.mnProcessId > 4 &&
                            !string.Equals(cBlockCartUpdate?.Trim(), "off", StringComparison.OrdinalIgnoreCase))
                        {
                            return "";
                        }
                        else
                        {
                            XmlElement CartXml = (XmlElement)myWeb.moCart.CreateCartElement(myWeb.moPageXml);

                            myCart.moDiscount.RemoveDiscountCode();
                            XmlElement argoCartElmt = (XmlElement)CartXml.FirstChild;
                            myCart.GetCart(ref argoCartElmt);
                            // persist cart
                            myCart.close();
                            CartXml = updateCartforJSON(CartXml);

                            jsonString = JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.Indented);
                            jsonString = jsonString.Replace("\"@", "\"_");
                            jsonString = jsonString.Replace("#cdata-section", "cDataValue");
                            return jsonString;
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }

                public string UpdateCartProductPrice(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {
                        string cProcessInfo = string.Empty;
                        double cProductPrice = (double)jObj["itemPrice"];
                        long cartItemId = (long)jObj["itemId"];

                        long userId = Convert.ToInt64("0" + (myWeb.moSession["nUserId"]?.ToString() ?? "0"));

                        if (myWeb.moDbHelper.checkUserRole(myCart.moCartConfig["AllowPriceUpdateRole"], "Role", userId))
                        {
                            myCart.UpdateItemPrice(cartItemId, cProductPrice);
                        }

                        XmlElement CartXml = (XmlElement)myWeb.moCart.CreateCartElement(myWeb.moPageXml);

                        XmlElement argoCartElmt = (XmlElement)CartXml.FirstChild;
                        myCart.GetCart(ref argoCartElmt);
                        // persist cart
                        myCart.close();
                        CartXml = updateCartforJSON(CartXml);

                        string jsonString = JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.Indented);
                        jsonString = jsonString.Replace("\"@", "\"_");
                        jsonString = jsonString.Replace("#cdata-section", "cDataValue");
                        return jsonString;
                    }

                    catch (Exception)
                    {
                        return null;
                    }
                }

                public int AddCartAddress(ref Protean.rest myApi, ref JObject jObj, string contactType, int cartId, string emailAddress = "", string telphone = "")
                {
                    try
                    {
                        var contact = new Cms.model.Contact();
                        int nId;
                        if (jObj != null)
                        {
                            contact.cContactEmail = emailAddress;
                            contact.cContactTel = telphone;
                            contact.cContactType = contactType;
                            contact.nContactCartId = cartId;
                            if (jObj["Forename"] != null)
                            {
                                contact.cContactFirstName = (string)jObj["Forename"];
                            }
                            if (jObj["Surname"] != null)
                            {
                                contact.cContactLastName = (string)jObj["Surname"];
                            }
                            if (jObj["Title"] != null)
                            {
                                contact.cContactTitle = (string)jObj["Title"];
                            }
                            if (jObj["cContactCompany"] != null)
                            {
                                contact.cContactCompany = (string)jObj["cContactCompany"];
                            }
                            if (jObj["CartId"] != null)
                            {
                                contact.nContactCartId = cartId;
                            }
                            if (jObj["Address1"] != null)
                            {
                                contact.cContactAddress = (string)jObj["Address1"];
                            }

                            if (jObj["Address2"] != null)
                            {
                                contact.cContactAddress = contact.cContactAddress + " " + jObj["Address2"].ToString();
                            }
                            if (jObj["City"] != null)
                            {
                                contact.cContactCity = (string)jObj["City"];
                            }
                            if (jObj["State"] != null)
                            {
                                contact.cContactState = (string)jObj["State"];
                            }
                            if (jObj["Country"] != null)
                            {
                                contact.cContactCountry = (string)jObj["Country"];
                            }
                            if (jObj["Postcode"] != null)
                            {
                                contact.cContactZip = (string)jObj["Postcode"];
                            }
                            if (jObj["Fax"] != null)
                            {
                                contact.cContactFax = (string)jObj["Fax"];
                            }

                            contact.cContactName = contact.cContactTitle + " " + contact.cContactFirstName + " " + contact.cContactLastName;

                        }

                        nId = myWeb.moDbHelper.SetContact(ref contact);
                        return nId;
                    }
                    catch (Exception ex)
                    {
                        return Convert.ToInt32(ex.Message);
                    }
                }


                // Public Function SubmitAddressForm(ByRef myApi As Protean.API, ByRef jObj As Dictionary(Of String, String)) As String
                // Try
                // 'Submit the address form as per Cart > Apply > Billing
                // myCart.mcCartCmd = "Billing"
                // myCart.apply()

                // 'assigning gateway
                // If (myApi.moRequest("paymentTypeValue") IsNot Nothing) Then
                // myCart.mcPaymentMethod = myApi.moRequest("paymentTypeValue") 'JudoPay payment method
                // myWeb.moSession.Remove("mcPaymentMethod")
                // myWeb.moSession.Add("mcPaymentMethod", myApi.moRequest("paymentTypeValue"))
                // Else
                // Return "Error"
                // End If

                // If myCart.mcPaymentMethod <> "" And Not myCart.moCartXml.SelectSingleNode("Order/Contact[@type='Shipping Address']") Is Nothing Then
                // myCart.mnProcessId = 4
                // ElseIf myCart.mcPaymentMethod <> "" And Not myCart.moCartXml.SelectSingleNode("Order/Contact[@type='Billing Address']") Is Nothing Then
                // myCart.mnProcessId = 5
                // End If

                // 'get updated cart
                // Dim moPageXml As XmlDocument
                // moPageXml = myWeb.moPageXml
                // Dim oCartXML As XmlDocument = moPageXml
                // Dim oElmt As XmlElement
                // Dim oContentElmt As XmlElement
                // oContentElmt = myCart.CreateCartElement(oCartXML)
                // oElmt = oContentElmt.FirstChild
                // myCart.GetCart(oElmt)

                // Dim jsonString As String = ""
                // If (myCart.mcPaymentMethod = "JudoPay") Then
                // 'paymentform 
                // Dim oPayProv As New Providers.Payment.BaseProvider(myWeb, myCart.mcPaymentMethod)
                // Dim ccPaymentXform As Protean.xForm = New Protean.xForm(myWeb.msException)
                // ccPaymentXform = oPayProv.Activities.GetPaymentForm(myWeb, myCart, oElmt)
                // jsonString = Newtonsoft.Json.JsonConvert.SerializeXmlNode(ccPaymentXform.moXformElmt, Newtonsoft.Json.Formatting.Indented)
                // jsonString = jsonString.Replace("""@", """_")
                // jsonString = jsonString.Replace("#cdata-section", "cDataValue")
                // ElseIf (myCart.mcPaymentMethod = "GooglePay") Then

                // End If

                // Return jsonString
                // 'Return "true"
                // Catch ex As Exception
                // Return "error" 'ex.Message
                // End Try
                // End Function

                public string CompleteOrder(string sProviderName, int nCartId, string sAuthNo, double dAmount, string ShippingType)
                {
                    try
                    {
                        var oXml = new XmlDocument();
                        string cShippingType = string.Empty;
                        var oDetailXml = oXml.CreateElement("Response");
                        XmlElement CartXml = (XmlElement)myWeb.moCart.CreateCartElement(myWeb.moPageXml);
                        XmlNode argoNode = oDetailXml;
                        addNewTextNode("AuthCode", ref argoNode, sAuthNo);
                        oDetailXml = (XmlElement)argoNode;

                        if (string.IsNullOrEmpty(ShippingType))
                        {
                            myCart.updateGCgetValidShippingOptionsDS(65.ToString());
                        }
                        else
                        {
                            var shippingXml = myCart.makeShippingOptionsXML();

                            // Dim nShipOptKey As Integer = Convert.ToInt32(shippingXml.SelectSingleNode("Method[cShipOptName='" + ShippingType + "']").SelectSingleNode("nShipOptKey").InnerText)
                            myCart.updateGCgetValidShippingOptionsDS(ShippingType);
                        }

                        myWeb.moDbHelper.savePayment(nCartId, 0L, sProviderName, sAuthNo, sProviderName, oDetailXml, DateTime.Now, false, dAmount);
                        myWeb.moDbHelper.SaveCartStatus(nCartId, (int)Cart.cartProcess.Complete);

                        XmlElement argoCartElmt = (XmlElement)CartXml.FirstChild;
                        myCart.GetCart(ref argoCartElmt);
                        myCart.purchaseActions(CartXml);

                        CartXml = updateCartforJSON(CartXml);
                        // persist cart
                        myCart.close();
                        string jsonString = JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.Indented);
                        jsonString = jsonString.Replace("\"@", "\"_");
                        jsonString = jsonString.Replace("#cdata-section", "cDataValue");
                        return jsonString;
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="myApi"></param>
                /// <param name="jObj"></param>
                /// <returns></returns>
                //public string CreatePaypalOrder(ref Protean.rest myApi, ref JObject jObj)
                //{
                //    try
                //    {
                //        string cProcessInfo = string.Empty;
                //        string josResult = "SUCCESS";

                //        // input params
                //        // Dim cProductPrice As Double = CDbl(jObj("orderId"))

                //        try
                //        {
                //            // if we receive any response from judopay pass it from PaymentReceipt
                //            // response should contain payment related all references like result, status, cardtoken, receiptId etc
                //            // validate if weather success or declined in Judopay.cs and redirect accordingly

                //            var myWeb = new Cms();
                //            Protean.Providers.Payment.ReturnProvider oPayProv = new Protean.Providers.Payment.ReturnProvider();
                //            IPaymentProvider oPaymentProv = oPayProv.Get(ref myWeb, "PayPalCommerce");
                //            //var oPayProv = new Providers.Payment.BaseProvider(ref myWeb, "PayPalCommerce");
                //            oPaymentProv.Activities.CreateOrder((object)true).Wait();
                //        }

                //        catch (Exception)
                //        {
                //            josResult = "ERROR";
                //        }


                //        return josResult;
                //    }

                //    catch (Exception ex)
                //    {
                //        return ex.Message;
                //    }
                //}

                //public string GetPaypalOrder(ref Protean.rest myApi, ref JObject jObj)
                //{
                //    try
                //    {
                //        string cProcessInfo = string.Empty;
                //        string josResult = "SUCCESS";

                //        // input params
                //        double cOrderId = (double)jObj["orderId"];

                //        try
                //        {
                //            var myWeb = new Cms();
                //            //var oPayProv = new Providers.Payment.BaseProvider(ref myWeb, "PayPalCommerce");
                //            Protean.Providers.Payment.ReturnProvider oPayProv = new Protean.Providers.Payment.ReturnProvider();
                //            IPaymentProvider oPaymentProv = oPayProv.Get(ref myWeb, "PayPalCommerce");
                //            oPaymentProv.Activities.GetOrder(cOrderId).Wait();
                //            oPaymentProv.Activities.CaptureOrder(cOrderId, (object)true).Wait();
                //            oPaymentProv.Activities.AuthorizeOrder(cOrderId, (object)true).Wait();
                //        }
                //        catch (Exception)
                //        {
                //            josResult = "ERROR";
                //        }
                //        return josResult;
                //    }

                //    catch (Exception ex)
                //    {
                //        return ex.Message;
                //    }
                //}

                //public string CapturePaypalOrder(ref Protean.rest myApi, ref JObject jObj)
                //{
                //    try
                //    {
                //        string cProcessInfo = string.Empty;
                //        string josResult = "SUCCESS";

                //        // input params
                //        double cOrderId = (double)jObj["orderId"];

                //        try
                //        {
                //            var myWeb = new Cms();

                //            //var oPayProv = new Providers.Payment.BaseProvider(ref myWeb, "PayPalCommerce");
                //            Protean.Providers.Payment.ReturnProvider oPayProv = new Protean.Providers.Payment.ReturnProvider();
                //            IPaymentProvider oPaymentProv = oPayProv.Get(ref myWeb, "PayPalCommerce");
                //            oPaymentProv.Activities.CaptureOrder(cOrderId, (object)true).Wait();
                //        }
                //        catch (Exception)
                //        {
                //            josResult = "ERROR";
                //        }

                //        return josResult;
                //    }

                //    catch (Exception ex)
                //    {
                //        return ex.Message;
                //    }
                //}

                public bool SaveToSellerNotes(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {
                        string cProcessInfo = string.Empty;
                        string cResponse = jObj.ToString();
                        string sSql;
                        var myWeb = new Cms();
                        var oCart = new Cart(ref myWeb);
                        string message = cResponse.Replace("{", "");
                        string errorMessage = message.Replace("}", "");
                        // Update Seller Notes:
                        sSql = "select * from tblCartOrder where nCartOrderKey = " + oCart.mnCartId;
                        DataSet oDs;
                        oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                        foreach (DataRow oRow in oDs.Tables["Order"].Rows)
                        {
                            oRow["cSellerNotes"] = (oRow["cSellerNotes"]?.ToString() ?? "") + Environment.NewLine + DateTime.Today.ToString("d") + " " + DateTime.Now.ToString("T") + ": " + errorMessage + "'";
                        }

                        myWeb.moDbHelper.updateDataset(ref oDs, "Order");

                        return true;
                    }

                    catch (Exception ex)
                    {
                        RaiseOnError(new Tools.Errors.ErrorEventArgs(mcModuleName, "SaveToSellerNotes", ex, ""));
                        return Convert.ToBoolean(ex.Message);
                    }
                }

                #region judopay functionality api functionality
                // get county for selected country
                public string PopulateCounty(Protean.rest myApi, JObject searchFilter)
                {
                    try
                    {

                        string JsonResult = "";
                        string sSql = "";

                        string strCountry = searchFilter["strCountry"].ToObject<string>();
                        System.Collections.Specialized.NameValueCollection moConfig;
                        moConfig = myApi.moConfig;
                        if (moConfig["CountryListforJudopayISOCode"] != null)
                        {
                            if (moConfig["CountryListforJudopayISOCode"].ToLower().Contains(strCountry.ToLower()) & !string.IsNullOrEmpty(strCountry))
                            {
                                sSql = "SELECT DISTINCT cLocationNameFull as Text, cLocationNameShort as Value FROM tblCartShippingLocations WHERE nLocationType = 4 And nLocationParId IN ";
                                sSql = sSql + " (SELECT nLocationKey FROM tblCartShippingLocations WHERE nLocationType = 2 And (cLocationNameShort Like '" + strCountry + "')) ORDER BY cLocationNameShort";

                                var countySelectList = new List<LocationList>();
                                using (var sdr = myWeb.moDbHelper.getDataReaderDisposable(sSql, CommandType.Text))
                                {
                                    while (sdr.Read())
                                        countySelectList.Add(new LocationList()
                                        {
                                            Text = sdr["Text"].ToString(),
                                            Value = sdr["Value"].ToString()
                                        });
                                }
                                JsonResult = JsonConvert.SerializeObject(countySelectList);
                            }
                        }

                        return JsonResult;
                    }
                    catch (Exception ex)
                    {
                        RaiseOnError(new Tools.Errors.ErrorEventArgs(mcModuleName, "PopulateCounty", ex, ""));
                        return ex.Message;
                    }
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="myApi"></param>
                /// <param name="searchFilter"></param>
                /// <returns></returns>
                //public string GetISOCodeforState(Protean.rest myApi, JObject searchFilter)
                //{
                //    try
                //    {

                //        // Dim JsonResult As String = ""
                //        string strISOCode = "";
                //        string strCountry = searchFilter["sCountry"].ToObject<string>();
                //        string strCounty = searchFilter["sCounty"].ToObject<string>();
                //        var cProviderName = Interaction.IIf(searchFilter["sProviderName"] != null, (string)searchFilter["sProviderName"], "");

                //        //var oPayProv = new Providers.Payment.BaseProvider(ref myWeb, Conversions.ToString(cProviderName));
                //        Protean.Providers.Payment.ReturnProvider oPayProv = new Protean.Providers.Payment.ReturnProvider();
                //        IPaymentProvider oPaymentProv = oPayProv.Get(ref myWeb, Conversions.ToString(cProviderName));

                //        System.Collections.Specialized.NameValueCollection moConfig;
                //        moConfig = myApi.moConfig;

                //        if (moConfig["CountryListforJudopayISOCode"] != null)
                //        {
                //            if (moConfig["CountryListforJudopayISOCode"].ToLower().Contains(strCountry.ToLower()) & !string.IsNullOrEmpty(strCountry))
                //            {
                //                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(cProviderName, "", false)))
                //                {
                //                    strISOCode = Conversions.ToString(oPaymentProv.Activities.getStateISOCode(strCounty, strCountry));
                //                }
                //            }
                //        }

                //        return strISOCode;
                //    }
                //    catch (Exception ex)
                //    {
                //        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "PopulateCounty", ex, ""));
                //        return ex.Message;
                //    }
                //}

                /// <summary>
                /// Refund order 
                /// </summary>
                /// <param name="myApi"></param>
                /// <param name="jObj"></param>
                /// <returns></returns>
                public string RefundOrder(ref Protean.rest myApi, ref JObject jObj)
                {
                    string josResult = string.Empty;

                    try
                    {

                        bool bIsAuthorized = false;
                        string validGroup = jObj["validGroup"] != null ? (string)jObj["validGroup"] : "";
                        bIsAuthorized = this.ValidateAPICall(Convert.ToString(validGroup));

                        if (bIsAuthorized == false)
                            return "Error -Authorization Failed";


                        var oCart = new Cart(ref myWeb);
                        oCart.moPageXml = myWeb.moPageXml;

                        long nProviderReference = jObj["nProviderReference"] != null ? (long)jObj["nProviderReference"] : 0;
                        decimal nAmount = jObj["nAmount"] != null ? Convert.ToDecimal(jObj["nAmount"]) : 0m;
                        string cProviderName = jObj["sProviderName"] != null ? (string)jObj["sProviderName"] : "";
                        object cRefundPaymentReceipt = "";

                        if (!string.IsNullOrEmpty(cProviderName))
                        {
                            Protean.Providers.Payment.ReturnProvider oPayProv = new Protean.Providers.Payment.ReturnProvider();
                            IPaymentProvider oPaymentProv = oPayProv.Get(ref myWeb, cProviderName);
                            cRefundPaymentReceipt = oPaymentProv.Activities.RefundPayment(nProviderReference.ToString(), nAmount);


                            if (moWebConfig["KlaviyoAPIPrivateKey"] != null && moWebConfig["KlaviyoAPIPrivateKey"] != "")
                            {
                                System.Collections.Specialized.NameValueCollection moMailConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/mailinglist");
                                if (moMailConfig != null)
                                {

                                    string sMessagingProvider = "";

                                    if (moMailConfig != null)
                                    {
                                        sMessagingProvider = moMailConfig["MessagingProvider"];
                                    }

                                    if (!string.IsNullOrEmpty(sMessagingProvider) | !string.IsNullOrEmpty(moMailConfig["InvoiceList"]) & !string.IsNullOrEmpty(moMailConfig["QuoteList"]))
                                    {
                                        Protean.Providers.Messaging.ReturnProvider RetProv = new Protean.Providers.Messaging.ReturnProvider();
                                        IMessagingProvider oMessaging = RetProv.Get(ref myWeb, sMessagingProvider);
                                        String sSql = "select cCartXml from tblcartorder  where nPayMthdId in (select nPayMthdKey from tblCartPaymentMethod where cPayMthdProviderRef='" + nProviderReference + "')";
                                        String scartXml = Convert.ToString(myWeb.moDbHelper.GetDataValue(sSql));

                                        XmlDocument doc = new XmlDocument();
                                        doc.LoadXml(scartXml);

                                        XmlElement cartxml = doc.DocumentElement;
                                        XmlNode orderNode = cartxml.SelectSingleNode("descendant-or-self::Order");


                                        oMessaging.Activities.TrackRefundEvent(orderNode, nAmount);

                                    }
                                }
                            }
                        }
                        var xmlDoc = new XmlDocument();
                        var xmlResponse = xmlDoc.CreateElement("Response");
                        xmlResponse.InnerXml = "<RefundPaymentReceiptId>" + cRefundPaymentReceipt + "</RefundPaymentReceiptId>";
                        xmlDoc.LoadXml(xmlResponse.InnerXml);

                        josResult = JsonConvert.SerializeXmlNode(xmlDoc.DocumentElement, Newtonsoft.Json.Formatting.Indented);
                        josResult = josResult.Replace("\"@", "\"_");
                        josResult = josResult.Replace("#cdata-section", "cDataValue");

                        return josResult;
                    }



                    catch (Exception ex)
                    {
                        RaiseOnError(new Tools.Errors.ErrorEventArgs(mcModuleName, "RefundOrder", ex, ""));
                        return "Error"; // ex.Message
                    }

                }

                /// <summary>
                /// Add Missing order 
                /// </summary>
                /// <param name="myApi"></param>
                /// <param name="jObj"></param>
                /// <returns></returns>
                public string UpdateOrderWithPaymentResponse(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {
                        string josResult = "";
                        bool bIsAuthorized = false;
                        string validGroup = jObj["validGroup"] != null ? (string)jObj["validGroup"] : "";
                        bIsAuthorized = this.ValidateAPICall(Convert.ToString(validGroup));

                        // If bIsAuthorized = False Then Return "Error -Authorization Failed"

                        // method name UpdateOrderWithPaymentResponse
                        string receiptID = jObj["AuthNumber"].ToString();
                        string cProviderName = jObj["sProviderName"] != null ? (string)jObj["sProviderName"] : "";
                        object strConsumerRef = "";
                        if (!string.IsNullOrEmpty(cProviderName) && !string.IsNullOrEmpty(receiptID))
                        {
                            // var oPayProv = new Providers.Payment.BaseProvider(ref myWeb, Conversions.ToString(cProviderName));
                            Protean.Providers.Payment.ReturnProvider oPayProv = new Protean.Providers.Payment.ReturnProvider();
                            IPaymentProvider oPaymentProv = oPayProv.Get(ref myWeb, Convert.ToString(cProviderName));
                            strConsumerRef = oPaymentProv.Activities.UpdateOrderWithPaymentResponse(receiptID);
                            josResult = Convert.ToString(strConsumerRef);
                        }
                        return josResult;
                    }
                    catch (Exception ex)
                    {
                        RaiseOnError(new Tools.Errors.ErrorEventArgs(mcModuleName, "UpdateOrderWithPaymentResponse", ex, ""));
                        return "Error"; // ex.Message
                    }

                }

                /// <summary>
                /// Process New payment
                /// </summary>
                /// <param name="myApi"></param>
                /// <param name="jObj"></param>
                /// <returns></returns>
                public string ProcessNewPayment(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {
                        bool bIsAuthorized = false;
                        string cValidGroup = (jObj["validGroup"] != null) ? (string)jObj["validGroup"] : "";
                        bIsAuthorized = this.ValidateAPICall(Convert.ToString(cValidGroup));

                        if (bIsAuthorized == false)
                            return "Error -Authorization Failed";

                        var oCart = new Cart(ref myWeb);
                        oCart.moPageXml = myWeb.moPageXml;

                        string cProviderName = (jObj["sProviderName"] != null) ? jObj["sProviderName"].ToString() : "";
                        string nOrderId = (jObj["orderId"] != null) ? jObj["orderId"].ToString() : "0";
                        decimal nAmount = (jObj["amount"] != null) ? Convert.ToDecimal(jObj["amount"]) : 0;
                        string cCardNumber = (jObj["cardNumber"] != null) ? jObj["cardNumber"].ToString() : "";
                        string cCV2 = (jObj["cV2"] != null) ? jObj["cV2"].ToString() : "";
                        string dExpiryDate = (jObj["expiryDate"] != null) ? jObj["expiryDate"].ToString() : "";
                        string dStartDate = (jObj["startDate"] != null) ? jObj["startDate"].ToString() : "";
                        string cCardHolderName = (jObj["cardHolderName"] != null) ? jObj["cardHolderName"].ToString() : "";
                        string cAddress1 = (jObj["address1"] != null) ? jObj["address1"].ToString() : "";
                        string cAddress2 = (jObj["address2"] != null) ? jObj["address2"].ToString() : "";
                        string cTown = (jObj["town"] != null) ? jObj["town"].ToString() : "";
                        string cPostCode = (jObj["postCode"] != null) ? jObj["postCode"].ToString() : "";
                        string cCountry = (jObj["country"] != null) ? jObj["country"].ToString() : "";
                        string cCounty = (jObj["county"] != null) ? jObj["county"].ToString() : "";

                        string cPaymentReceipt = "";
                        string josResult = "";
                        if (!string.IsNullOrEmpty(cProviderName))
                        {
                            //var oPayProv = new Providers.Payment.BaseProvider(ref myWeb, Conversions.ToString(cProviderName));
                            Protean.Providers.Payment.ReturnProvider oPayProv = new Protean.Providers.Payment.ReturnProvider();
                            IPaymentProvider oPaymentProv = oPayProv.Get(ref myWeb, Convert.ToString(cProviderName));
                            cPaymentReceipt = Convert.ToString(oPaymentProv.Activities.ProcessNewPayment(nOrderId, nAmount, cCardNumber, cCV2, dExpiryDate, dStartDate, cCardHolderName, cAddress1, cAddress2, cTown, cPostCode, cCounty, cCountry, cValidGroup));
                            var xmlDoc = new XmlDocument();
                            var xmlResponse = xmlDoc.CreateElement("Response");
                            xmlResponse.InnerXml = "<PaymentReceiptId>" + cPaymentReceipt + "</PaymentReceiptId>";
                            xmlDoc.LoadXml(xmlResponse.InnerXml.ToString());
                            josResult = JsonConvert.SerializeXmlNode(xmlDoc.DocumentElement, Newtonsoft.Json.Formatting.Indented);
                            josResult = josResult.Replace("\"@", "\"_");
                            josResult = josResult.Replace("#cdata-section", "cDataValue");
                        }
                        return josResult;
                    }

                    catch (Exception ex)
                    {
                        RaiseOnError(new Tools.Errors.ErrorEventArgs(mcModuleName, "ProcessNewPayment", ex, ""));
                        return "Error"; // ex.Message
                    }

                }
                /// <summary>
                ///Anonymize GDPR
                /// </summary>
                /// <param name="myApi"></param>
                /// <param name="jObj"></param>
                /// <returns></returns>
                public string AnonymizeGDPRData(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {
                        string josResult = "";
                        bool bIsAuthorized = false;
                        string cValidGroup = (jObj["validGroup"] != null) ? (string)jObj["validGroup"] : "";
                        bIsAuthorized = this.ValidateAPICall(Convert.ToString(cValidGroup));
                        if (bIsAuthorized == false)
                            return "Error -Authorization Failed";
                        if (jObj["cEmailAddress"] != null && jObj["cEmailAddress"].ToString() != "")
                        {
                            var cEmailAddress = jObj["cEmailAddress"].ToString();
                            josResult = myCart.GDPRAnonomize(cEmailAddress);
                        }

                        return josResult;
                    }
                    catch (Exception ex)
                    {
                        RaiseOnError(new Tools.Errors.ErrorEventArgs(mcModuleName, "AnonymizeGDPRData", ex, ""));
                        return "Error"; // ex.Message
                    }

                }

               
                #endregion




                // Address Lookup for postcode functionality
                public class PostcodeSearchAddressResult
                {
                    public string Status { get; set; }
                    public string Message { get; set; }
                    public List<PostcodeSearchAddress> Addresses { get; set; }
                }

                public class PostcodeSearchAddress
                {
                    public string Organisation { get; set; }
                    public string BuildingName { get; set; }
                    public string SubBuildingName { get; set; }

                    public string Address1 { get; set; }
                    public string Address2 { get; set; }
                    public string Address3 { get; set; }
                    public string TownCity { get; set; }
                    public string Postcode { get; set; }
                    public string DependentLocality { get; set; }

                    public string CountyTraditional { get; set; }
                    public string CountyFormerPostal { get; set; }
                    public string CountyAdministrative { get; set; }

                    public string FullAddress { get; set; }
                }

                public string AddressLookup(Protean.rest myApi, Newtonsoft.Json.Linq.JObject searchFilter)
                {
                    try
                    {
                        string JsonResult = "";
                        string strPostcode = searchFilter["strPostcode"].ToObject<string>();
                        string SelectedAddress = searchFilter["SelectedAddress"]?.ToObject<string>();
                        var SearchPostcodedetails = DoSearch(strPostcode, SelectedAddress);
                        JsonResult = JsonConvert.SerializeObject(SearchPostcodedetails);
                        return JsonResult;
                    }
                    catch (Exception ex)
                    {
                        RaiseOnError(new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AddressDetails", ex, ""));
                        return ex.Message;
                    }
                }

                private PostcodeSearchAddressResult DoSearch(string postcode, string nameNumber)
                {

                    epostcode.PostcodeServices13SoapClient WS = null;
                    epostcode.ListAllAddressPremises addressPremises = null;

                    bool ErrorFlag = false;
                    string sTmp = string.Empty;

                    PostcodeSearchAddressResult result = new PostcodeSearchAddressResult();

                    if (string.IsNullOrEmpty(postcode))
                    {
                        // WriteToLog("Blank postcode")
                        //result.Status = "err";
                        //result.Message = "blank postcode";
                    }
                    else
                    {
                        WS = new epostcode.PostcodeServices13SoapClient();
                        // WS.Timeout = 3000;

                        // WriteToLog("Looking up postcode: """ & Postcode & """")
                        try
                        {
                            string msePostcodeAcctName = myCart.moCartConfig["ePostcodeAcctName"];
                            string msePostcodeKey = myCart.moCartConfig["ePostcodeKey"];
                            //addressPremises = WS.GetPremiseAddressesFromPostcodeAndHouseNumber(strPostcode, SelectedAddress, "100", msAccountNameDemo, msGUIDDemo, "");
                            addressPremises = WS.GetPremiseAddressesFromPostcodeAndHouseNumber(postcode, "", "100", msePostcodeAcctName, msePostcodeKey, "");


                        }
                        catch (Exception ex)
                        {
                            // WriteToLog("Caught WS error - " & ex.Message)
                            //result.Status = "err";
                            //result.Message = "WS error 1"; // helpful message...
                            RaiseOnError(new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "DoSearch", ex, ""));
                            ErrorFlag = true;
                        }

                        if (!ErrorFlag)
                        {
                            if (addressPremises == null)
                            {
                                // think this might be an error,  mais tant pis...
                                //WriteToLog("addresses = null")
                                result.Status = "not found";
                                result.Message = "";
                            }
                            else
                            {
                                if (addressPremises.IsError)
                                {
                                    // WriteToLog("WS IsError = " & addressPremises.ErrorMessage)
                                    result.Status = "err";
                                    result.Message = "WS error 2";
                                }
                                else
                                {
                                    // WriteToDebugLog("Addresses.list.length=" & addressPremises.List.Length)
                                    if (addressPremises.List.Length == 0)
                                    {
                                        // WriteToDebugLog("Addresses.list.length=0: Nothing found")
                                        result.Status = "not found";
                                        result.Message = "";
                                    }
                                    else if ((addressPremises.List.Length == 1) && (addressPremises.List[0].Return_Code == "0"))
                                    {
                                        // WriteToDebugLog("Address.list.length=1 + Return_Code=0: Nothing found")
                                        result.Status = "not found";
                                        result.Message = "";
                                    }
                                    else
                                    {
                                        Regex re = new Regex(@"\s{2,}");
                                        result.Status = "OK";
                                        result.Message = "";
                                        result.Addresses = new List<PostcodeSearchAddress>();
                                        foreach (epostcode.AddressPremise a in addressPremises.List)
                                        {


                                            if ((!string.IsNullOrEmpty(nameNumber?.Trim())))
                                            {
                                                if ((a.FullAddress.ToString().Contains(nameNumber.Trim())))
                                                {
                                                    PostcodeSearchAddress address = new PostcodeSearchAddress();
                                                    if (a.Organisation.Length > 0)
                                                    {

                                                        address.Organisation = a.Organisation;
                                                    }

                                                    if (a.Sub_Building_Name.Length > 0)
                                                    {
                                                        //	"Flat 10"
                                                        //	Prepend this to the building name
                                                        if (a.Building_Name.Length == 0)
                                                            a.Building_Name = a.Sub_Building_Name;
                                                        else
                                                            a.Building_Name = a.Sub_Building_Name + ", " + a.Building_Name;
                                                    }

                                                    if (a.Building_Name.Length > 0)
                                                    {
                                                        // Check if this building name is just a number,  e.g. "9-17".  In which case,  if the Number is empty,  use this as the number
                                                        if (Regex.IsMatch(a.Building_Name, @"^[\d\-\s]+$"))
                                                        {
                                                            // it's just a number...
                                                            if (a.Number.Length > 0)
                                                            {
                                                                // But they're using a number too.  So treat this building name as a building name
                                                                address.BuildingName = a.Building_Name;
                                                            }
                                                            else
                                                                a.Number = a.Building_Name;
                                                        }
                                                        else
                                                            address.BuildingName = a.Building_Name;
                                                    }

                                                    if (a.Number.Length + a.Street.Length > 0)
                                                    {
                                                        if (a.Number.Length == 0)
                                                            address.Address1 = a.Street;
                                                        else
                                                        {
                                                            if (a.Street.Length == 0)
                                                                address.Address1 = a.Number;
                                                            else
                                                                address.Address1 = a.Number + " " + a.Street;

                                                        }
                                                    }


                                                    if (a.County_FormerPostal.Length > 0) address.CountyFormerPostal = a.County_FormerPostal;
                                                    if (a.County_Traditional.Length > 0) address.CountyTraditional = a.County_Traditional;

                                                    if (a.Post_Town.Length > 0 && a.County_Administrative.Length > 0 && a.Post_Town == a.County_Administrative)
                                                        address.TownCity = a.Post_Town; // '		London, Manchester, etc. - don't add the county
                                                    else
                                                    {
                                                        if (a.Post_Town.Length > 0)
                                                            address.TownCity = a.Post_Town;

                                                        if (a.County_Administrative.Length > 0)
                                                            address.CountyAdministrative = a.County_Administrative;
                                                    }

                                                    if (a.Postcode.Length > 0)
                                                    {
                                                        address.Postcode = a.Postcode;
                                                    }

                                                    var tmp = string.Empty;
                                                    if (!string.IsNullOrWhiteSpace(address.BuildingName))
                                                        tmp = address.BuildingName;
                                                    if (!string.IsNullOrWhiteSpace(address.SubBuildingName))
                                                    {
                                                        tmp = (tmp.Length > 0 ? tmp + ", " + address.SubBuildingName : address.SubBuildingName);
                                                    }
                                                    if (!string.IsNullOrWhiteSpace(address.Address1))
                                                    {
                                                        tmp = (tmp.Length > 0 ? tmp + ", " + address.Address1 : address.Address1);
                                                    }
                                                    address.Address1 = tmp; // merge the building and street into line 1.

                                                    address.FullAddress = Regex.Replace(a.FullAddress, @"(^\\r\\n)|(\\r\\n$)|(\\r\\n\s+(?=\\r))", "");
                                                    address.FullAddress = Regex.Replace(address.FullAddress, @"\\r\\n", ", ");

                                                    result.Addresses.Add(address);
                                                }
                                                //break;
                                            }
                                            else
                                            {



                                                PostcodeSearchAddress address = new PostcodeSearchAddress();
                                                if (a.Organisation.Length > 0)
                                                {

                                                    address.Organisation = a.Organisation;
                                                }

                                                if (a.Sub_Building_Name.Length > 0)
                                                {
                                                    //	"Flat 10"
                                                    //	Prepend this to the building name
                                                    if (a.Building_Name.Length == 0)
                                                        a.Building_Name = a.Sub_Building_Name;
                                                    else
                                                        a.Building_Name = a.Sub_Building_Name + ", " + a.Building_Name;
                                                }

                                                if (a.Building_Name.Length > 0)
                                                {
                                                    // Check if this building name is just a number,  e.g. "9-17".  In which case,  if the Number is empty,  use this as the number
                                                    if (Regex.IsMatch(a.Building_Name, @"^[\d\-\s]+$"))
                                                    {
                                                        // it's just a number...
                                                        if (a.Number.Length > 0)
                                                        {
                                                            // But they're using a number too.  So treat this building name as a building name
                                                            address.BuildingName = a.Building_Name;
                                                        }
                                                        else
                                                            a.Number = a.Building_Name;
                                                    }
                                                    else
                                                        address.BuildingName = a.Building_Name;
                                                }

                                                if (a.Number.Length + a.Street.Length > 0)
                                                {
                                                    if (a.Number.Length == 0)
                                                        address.Address1 = a.Street;
                                                    else
                                                    {
                                                        if (a.Street.Length == 0)
                                                            address.Address1 = a.Number;
                                                        else
                                                            address.Address1 = a.Number + " " + a.Street;

                                                    }
                                                }


                                                if (a.County_FormerPostal.Length > 0) address.CountyFormerPostal = a.County_FormerPostal;
                                                if (a.County_Traditional.Length > 0) address.CountyTraditional = a.County_Traditional;

                                                if (a.Post_Town.Length > 0 && a.County_Administrative.Length > 0 && a.Post_Town == a.County_Administrative)
                                                    address.TownCity = a.Post_Town; // '		London, Manchester, etc. - don't add the county
                                                else
                                                {
                                                    if (a.Post_Town.Length > 0)
                                                        address.TownCity = a.Post_Town;

                                                    if (a.County_Administrative.Length > 0)
                                                        address.CountyAdministrative = a.County_Administrative;
                                                }

                                                if (a.Postcode.Length > 0)
                                                {
                                                    address.Postcode = a.Postcode;
                                                }

                                                var tmp = string.Empty;
                                                if (!string.IsNullOrWhiteSpace(address.BuildingName))
                                                    tmp = address.BuildingName;
                                                if (!string.IsNullOrWhiteSpace(address.SubBuildingName))
                                                {
                                                    tmp = (tmp.Length > 0 ? tmp + ", " + address.SubBuildingName : address.SubBuildingName);
                                                }
                                                if (!string.IsNullOrWhiteSpace(address.Address1))
                                                {
                                                    tmp = (tmp.Length > 0 ? tmp + ", " + address.Address1 : address.Address1);
                                                }
                                                address.Address1 = tmp; // merge the building and street into line 1.

                                                address.FullAddress = Regex.Replace(a.FullAddress, @"(^\\r\\n)|(\\r\\n$)|(\\r\\n\s+(?=\\r))", "");
                                                address.FullAddress = Regex.Replace(address.FullAddress, @"\\r\\n", ", ");

                                                result.Addresses.Add(address);
                                            }
                                        }


                                    }
                                }
                            }
                        }
                    }
                    return result;
                }
            }
            #endregion
        }

    }
}
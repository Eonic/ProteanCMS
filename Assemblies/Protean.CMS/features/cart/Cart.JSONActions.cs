using System;
using System.Collections.Generic;
using System.Data;
using System.Web.Configuration;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

            public class JSONActions : Protean.rest.JsonActions
            {

                public event OnErrorEventHandler OnError;

                public delegate void OnErrorEventHandler(object sender, Tools.Errors.ErrorEventArgs e);
                private const string mcModuleName = "Eonic.Cart.JSONActions";
                private const string cContactType = "Venue";
                private System.Collections.Specialized.NameValueCollection moLmsConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/lms");
                private Cms myWeb;
                private Cart myCart;


                public JSONActions()
                {
                    string ctest = "this constructor is being hit"; // for testing
                    myWeb = new Cms();
                    myWeb.InitializeVariables();
                    myWeb.Open();
                    myCart = new Cart(ref myWeb);

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
                        string cProcessInfo = "";

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
                        }


                        CartXml = updateCartforJSON(CartXml);

                        string jsonString = JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.Indented);
                        jsonString = jsonString.Replace("\"@", "\"_");
                        jsonString = jsonString.Replace("#cdata-section", "cDataValue");

                        return jsonString;
                        // persist cart
                        myCart.close();
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""));
                        return ex.Message;
                    }
                }

                public string AddItems(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {
                        string cProcessInfo = "";
                        // jsonObject("artId")
                        // myCart.AddItem()
                        // Output the new cart
                        var oDoc = new XmlDocument();
                        XmlElement CartXml = (XmlElement)myWeb.moCart.CreateCartElement(myWeb.moPageXml);


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
                        if ((int)myCart.mnProcessId > 4)
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
                                    bool bPackegingRequired = false;
                                    string sOverideURL = "";
                                    string sProductOptionName = "";
                                    double dProductOptionPrice = 0d;
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
                                    if (item.ContainsKey("productOptionPrice"))
                                    {
                                        dProductOptionPrice = (double)item["productOptionPrice"];
                                    }

                                    myCart.AddItem((long)item["contentId"], (long)item["qty"], (Array)null, sProductName, cProductPrice, "", bUnique, sOverideURL, false, sProductOptionName, dProductOptionPrice);

                                }
                            }

                            // Output the new cart
                            XmlElement argoCartElmt = (XmlElement)CartXml.FirstChild;
                            myCart.GetCart(ref argoCartElmt);
                            CartXml = updateCartforJSON(CartXml);
                            // persist cart
                            myCart.close();

                            string jsonString = JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.None);
                            jsonString = jsonString.Replace("\"@", "\"_");
                            jsonString = jsonString.Replace("#cdata-section", "cDataValue");

                            return jsonString;
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""));
                        return ex.Message;
                    }

                }

                public string RemoveItems(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {
                        if ((int)myCart.mnProcessId > 4)
                        {
                            return "";
                        }
                        else
                        {
                            string cProcessInfo = "";
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
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""));
                        return ex.Message;
                    }

                }

                public string UpdateItems(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {

                        string cProcessInfo = "";
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
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetCart", ex, ""));
                        return ex.Message;
                    }

                }

                public string GetShippingOptions(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {
                        string cProcessInfo = "";
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
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetShippingOptions", ex, ""));
                        return ex.Message;
                    }
                }

                public string UpdatedCartShippingOptions(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {

                        string cProcessInfo = "";
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
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "UpdatedCartShippingOptions", ex, ""));
                        return ex.Message;
                    }

                }

                public string UpdateDeliveryOptionByCountry(ref Protean.rest myApi, ref JObject jObj)
                {
                    if ((int)myCart.mnProcessId > 4)
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

                        object userContacts = myWeb.moDbHelper.GetUserContactsXml(Conversions.ToInteger(dirId));
                        JsonResult = JsonConvert.SerializeObject(userContacts);
                        return JsonResult;
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetLocations", ex, ""));
                        return ex.Message;
                    }
                }

                public string GetContactForm(ref Protean.rest myApi, ref JObject jObj)
                {
                    int nId;
                    try
                    {

                        string JsonResult = "";
                        object oDdoc = new XmlDocument();
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
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "GetLocations", ex, ""));
                        return ex.Message;
                    }
                    return JsonConvert.ToString(nId);
                }

                public string SetContact(ref Protean.rest myApi, ref JObject jObj)
                {
                    int nId;
                    try
                    {
                        int supplierId = (int)jObj["supplierId"];
                        var contact = jObj["venue"].ToObject<Contact>();
                        contact.cContactType = cContactType;
                        contact.cContactForeignRef = string.Format("SUP-{0}", supplierId);

                        nId = myWeb.moDbHelper.SetContact(ref contact);
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "AddContact", ex, ""));
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
                        int argnContactKey = Conversions.ToInteger(cContactKey);
                        isSuccess = myWeb.moDbHelper.DeleteContact(ref argnContactKey);
                        cContactKey = argnContactKey.ToString();
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "DeleteContact", ex, ""));
                        return ex.Message;
                    }
                    return JsonConvert.ToString(isSuccess);
                }

                public string AddProductOption(ref Protean.rest myApi, ref JObject jObj)
                {
                    string jsonString = string.Empty;

                    try
                    {

                        XmlElement CartXml = (XmlElement)myWeb.moCart.CreateCartElement(myWeb.moPageXml);
                        // myCart.GetCart(CartXml.FirstChild)

                        // add product option
                        myCart.AddProductOption(ref jObj);
                        // myCart.UpdatePackagingANdDeliveryType()
                        // myCart.GetCart(CartXml.FirstChild)   //Comment out this extra called method because this code already added in UpdatePackagingDeliveryOptions method - change on 5th jan 23
                        /// persist cart
                        myCart.close();

                        // CartXml = updateCartforJSON(CartXml)

                        // jsonString = Newtonsoft.Json.JsonConvert.SerializeXmlNode(CartXml, Newtonsoft.Json.Formatting.Indented)
                        // jsonString = jsonString.Replace("""@", """_")
                        // jsonString = jsonString.Replace("#cdata-section", "cDataValue")
                        return jsonString;
                    }

                    catch (Exception ex)
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

                    catch (Exception ex)
                    {
                        return null;
                    }

                }

                public string AddDiscountCode(ref Protean.rest myApi, ref JObject jObj)
                {
                    string strMessage = string.Empty;
                    try
                    {

                        if ((int)myCart.mnProcessId > 4)
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
                    catch (Exception ex)
                    {
                        return null;
                    }
                }

                public string RemoveDiscountCode(ref Protean.rest myApi, ref JObject jObj)
                {
                    string jsonString = string.Empty;
                    try
                    {
                        if ((int)myCart.mnProcessId > 4)
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
                    catch (Exception ex)
                    {
                        return null;
                    }
                }

                public string UpdateCartProductPrice(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {
                        string cProcessInfo = "";
                        double cProductPrice = (double)jObj["itemPrice"];
                        long cartItemId = (long)jObj["itemId"];

                        if (myWeb.moDbHelper.checkUserRole(myCart.moCartConfig["AllowPriceUpdateRole"], "Role", Conversions.ToLong(Operators.ConcatenateObject("0", myWeb.moSession["nUserId"]))))
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

                    catch (Exception ex)
                    {
                        return null;
                    }
                }

                public int AddCartAddress(ref Protean.rest myApi, ref JObject jObj, string contactType, int cartId, string emailAddress = "", string telphone = "")
                {
                    try
                    {
                        var contact = new Cms.Contact();
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
                        return Conversions.ToInteger(ex.Message);
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
                        myCart.purchaseActions(ref CartXml);
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
                        return ex.Message;
                    }
                }

                /// <summary>
                /// 
                /// </summary>
                /// <param name="myApi"></param>
                /// <param name="jObj"></param>
                /// <returns></returns>
                public string CreatePaypalOrder(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {
                        string cProcessInfo = "";
                        string josResult = "SUCCESS";

                        // input params
                        // Dim cProductPrice As Double = CDbl(jObj("orderId"))

                        try
                        {
                            // if we receive any response from judopay pass it from PaymentReceipt
                            // response should contain payment related all references like result, status, cardtoken, receiptId etc
                            // validate if weather success or declined in Judopay.cs and redirect accordingly

                            var myWeb = new Cms();

                            var oPayProv = new Providers.Payment.BaseProvider(ref myWeb, "PayPalCommerce");
                            oPayProv.Activities.CreateOrder((object)true).Wait();
                        }

                        catch (Exception ex)
                        {
                            josResult = "ERROR";
                        }


                        return josResult;
                    }

                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }

                public string GetPaypalOrder(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {
                        string cProcessInfo = "";
                        string josResult = "SUCCESS";

                        // input params
                        double cOrderId = (double)jObj["orderId"];

                        try
                        {
                            var myWeb = new Cms();

                            var oPayProv = new Providers.Payment.BaseProvider(ref myWeb, "PayPalCommerce");
                            oPayProv.Activities.GetOrder(cOrderId).Wait();

                            oPayProv.Activities.CaptureOrder(cOrderId, (object)true).Wait();

                            oPayProv.Activities.AuthorizeOrder(cOrderId, (object)true).Wait();
                        }

                        catch (Exception ex)
                        {
                            josResult = "ERROR";
                        }


                        return josResult;
                    }

                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }

                public string CapturePaypalOrder(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {
                        string cProcessInfo = "";
                        string josResult = "SUCCESS";

                        // input params
                        double cOrderId = (double)jObj["orderId"];

                        try
                        {
                            var myWeb = new Cms();

                            var oPayProv = new Providers.Payment.BaseProvider(ref myWeb, "PayPalCommerce");

                            oPayProv.Activities.CaptureOrder(cOrderId, (object)true).Wait();
                        }

                        catch (Exception ex)
                        {
                            josResult = "ERROR";
                        }


                        return josResult;
                    }

                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }

                public bool SaveToSellerNotes(ref Protean.rest myApi, ref JObject jObj)
                {
                    try
                    {
                        string cProcessInfo = "";
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
                            oRow["cSellerNotes"] = Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject(oRow["cSellerNotes"], Constants.vbLf), DateTime.Today), " "), DateAndTime.TimeOfDay), ": "), errorMessage), "'");
                        myWeb.moDbHelper.updateDataset(ref oDs, "Order");
                        return true;
                    }

                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "SaveToSellerNotes", ex, ""));
                        return Conversions.ToBoolean(ex.Message);
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
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "PopulateCounty", ex, ""));
                        return ex.Message;
                    }
                }
                /// <summary>
                /// 
                /// </summary>
                /// <param name="myApi"></param>
                /// <param name="searchFilter"></param>
                /// <returns></returns>
                public string GetISOCodeforState(Protean.rest myApi, JObject searchFilter)
                {
                    try
                    {

                        // Dim JsonResult As String = ""
                        string strISOCode = "";
                        string strCountry = searchFilter["sCountry"].ToObject<string>();
                        string strCounty = searchFilter["sCounty"].ToObject<string>();
                        var cProviderName = Interaction.IIf(searchFilter["sProviderName"] != null, (string)searchFilter["sProviderName"], "");

                        var oPayProv = new Providers.Payment.BaseProvider(ref myWeb, Conversions.ToString(cProviderName));

                        System.Collections.Specialized.NameValueCollection moConfig;
                        moConfig = myApi.moConfig;

                        if (moConfig["CountryListforJudopayISOCode"] != null)
                        {
                            if (moConfig["CountryListforJudopayISOCode"].ToLower().Contains(strCountry.ToLower()) & !string.IsNullOrEmpty(strCountry))
                            {
                                if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(cProviderName, "", false)))
                                {
                                    strISOCode = Conversions.ToString(oPayProv.Activities.getStateISOCode(strCounty, strCountry));
                                }
                            }
                        }

                        return strISOCode;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "PopulateCounty", ex, ""));
                        return ex.Message;
                    }
                }

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
                        var validGroup = Interaction.IIf(jObj["validGroup"] != null, (string)jObj["validGroup"], "");
                        bIsAuthorized = this.ValidateAPICall(ref myWeb, Conversions.ToString(validGroup));

                        if (bIsAuthorized == false)
                            return "Error -Authorization Failed";


                        var oCart = new Cart(ref myWeb);
                        oCart.moPageXml = myWeb.moPageXml;

                        var nProviderReference = Interaction.IIf(jObj["nProviderReference"] != null, (long)jObj["nProviderReference"], 0);
                        var nAmount = Interaction.IIf(jObj["nAmount"] != null, (decimal)jObj["nAmount"], "0");
                        var cProviderName = Interaction.IIf(jObj["sProviderName"] != null, (string)jObj["sProviderName"], "");
                        object cRefundPaymentReceipt = "";
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(cProviderName, "", false)))
                        {
                            var oPayProv = new Providers.Payment.BaseProvider(ref myWeb, Conversions.ToString(cProviderName));
                            cRefundPaymentReceipt = oPayProv.Activities.RefundPayment(nProviderReference, nAmount);

                            var xmlDoc = new XmlDocument();
                            var xmlResponse = xmlDoc.CreateElement("Response");
                            xmlResponse.InnerXml = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("<RefundPaymentReceiptId>", cRefundPaymentReceipt), "</RefundPaymentReceiptId>"));
                            xmlDoc.LoadXml(xmlResponse.InnerXml.ToString());
                            josResult = JsonConvert.SerializeXmlNode(xmlDoc.DocumentElement, Newtonsoft.Json.Formatting.Indented);

                            josResult = josResult.Replace("\"@", "\"_");
                            josResult = josResult.Replace("#cdata-section", "cDataValue");

                            return josResult;
                        }
                        return josResult;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "RefundOrder", ex, ""));
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
                        var validGroup = Interaction.IIf(jObj["validGroup"] != null, (string)jObj["validGroup"], "");
                        bIsAuthorized = this.ValidateAPICall(ref myWeb, Conversions.ToString(validGroup));

                        // If bIsAuthorized = False Then Return "Error -Authorization Failed"

                        // method name UpdateOrderWithPaymentResponse
                        object receiptID = jObj["AuthNumber"];
                        var cProviderName = Interaction.IIf(jObj["sProviderName"] != null, (string)jObj["sProviderName"], "");
                        object strConsumerRef = "";
                        if (Conversions.ToBoolean(Operators.AndObject(Operators.ConditionalCompareObjectNotEqual(cProviderName, "", false), Operators.ConditionalCompareObjectNotEqual(receiptID, 0, false))))
                        {
                            var oPayProv = new Providers.Payment.BaseProvider(ref myWeb, Conversions.ToString(cProviderName));
                            strConsumerRef = oPayProv.Activities.UpdateOrderWithPaymentResponse(receiptID);
                            josResult = Conversions.ToString(strConsumerRef);
                        }
                        return josResult;
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "UpdateOrderWithPaymentResponse", ex, ""));
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
                        var cValidGroup = Interaction.IIf(jObj["validGroup"] != null, (string)jObj["validGroup"], "");
                        bIsAuthorized = this.ValidateAPICall(ref myWeb, Conversions.ToString(cValidGroup));

                        if (bIsAuthorized == false)
                            return "Error -Authorization Failed";

                        var oCart = new Cart(ref myWeb);
                        oCart.moPageXml = myWeb.moPageXml;

                        var cProviderName = Interaction.IIf(jObj["sProviderName"] != null, (string)jObj["sProviderName"], "");
                        var nOrderId = Interaction.IIf(jObj["orderId"] != null, (string)jObj["orderId"], "0");
                        var nAmount = Interaction.IIf(jObj["amount"] != null, (decimal)jObj["amount"], "0");
                        var cCardNumber = Interaction.IIf(jObj["cardNumber"] != null, (string)jObj["cardNumber"], "");
                        var cCV2 = Interaction.IIf(jObj["cV2"] != null, (string)jObj["cV2"], "");
                        var dExpiryDate = Interaction.IIf(jObj["expiryDate"] != null, (string)jObj["expiryDate"], "");
                        var dStartDate = Interaction.IIf(jObj["startDate"] != null, (string)jObj["startDate"], "");
                        var cCardHolderName = Interaction.IIf(jObj["cardHolderName"] != null, (string)jObj["cardHolderName"], "");
                        var cAddress1 = Interaction.IIf(jObj["address1"] != null, (string)jObj["address1"], "");
                        var cAddress2 = Interaction.IIf(jObj["address2"] != null, (string)jObj["address2"], "");
                        var cTown = Interaction.IIf(jObj["town"] != null, (string)jObj["town"], "");
                        var cPostCode = Interaction.IIf(jObj["postCode"] != null, (string)jObj["postCode"], "");
                        var cCountry = Interaction.IIf(jObj["country"] != null, (string)jObj["country"], "");
                        var cCounty = Interaction.IIf(jObj["county"] != null, (string)jObj["county"], "");

                        string cPaymentReceipt = "";
                        string josResult = "";
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(cProviderName, "", false)))
                        {
                            var oPayProv = new Providers.Payment.BaseProvider(ref myWeb, Conversions.ToString(cProviderName));
                            cPaymentReceipt = Conversions.ToString(oPayProv.Activities.ProcessNewPayment(nOrderId, nAmount, cCardNumber, cCV2, dExpiryDate, dStartDate, cCardHolderName, cAddress1, cAddress2, cTown, cPostCode, cCounty, cCountry, cValidGroup));
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
                        OnError?.Invoke(this, new Tools.Errors.ErrorEventArgs(mcModuleName, "ProcessNewPayment", ex, ""));
                        return "Error"; // ex.Message
                    }

                }

                #endregion

            }

            #endregion
        }

    }
}
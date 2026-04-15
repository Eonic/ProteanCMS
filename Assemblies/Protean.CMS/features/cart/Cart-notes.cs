using Protean.Providers.Membership;
using Protean.Providers.Messaging;
using Protean.Providers.Payment;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using System.Xml;
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


            public virtual string notesProcess(XmlElement oElmt)
            {
                string sCartCmd = mcCartCmd;
                string cProcessInfo = "";
                try
                {

                    // should never get this far for subscriptions unless logged on.

                    if (moSubscription != null)
                    {
                        if (!moSubscription.CheckCartForSubscriptions(mnCartId, myWeb.mnUserId))
                        {
                            if (myWeb.mnUserId == 0)
                            {
                                sCartCmd = "LogonSubs";
                            }
                        }
                    }

                    myWeb.moSession["cLogonCmd"] = "";

                    GetCart(ref oElmt);

                    if (!string.IsNullOrEmpty(mcNotesXForm))
                    {
                        var oNotesXform = notesXform("notesForm", mcPagePath + "cartCmd=Notes", oElmt);
                        if (oNotesXform.valid == false)
                        {
                            moPageXml.SelectSingleNode("/Page/Contents").AppendChild(oNotesXform.moXformElmt);
                        }
                        else
                        {
                            oElmt.RemoveAll();
                            sCartCmd = "SkipAddress";
                        }
                    }
                    else
                    {
                        oElmt.RemoveAll();
                        sCartCmd = "SkipAddress";
                    }

                    // if this returns Notes then we display for otherwise we goto processflow
                    return sCartCmd;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "notesProcess", ex, "", cProcessInfo, gbDebug);
                    return "";
                }




            }






            public virtual Cms.xForm notesXform(string formName = "notesForm", string action = "?cartCmd=Notes", XmlElement oCart = null)
            {
                Cms.xForm notesXformRet = default;
                myWeb.PerfMon.Log("Cart", "notesXform");
                // this function is called for the collection from a form and addition to the database
                // of address information.

                DataSet oDs;
                string sSql;
                XmlElement oFormGrp;
                string sXmlContent;
                XmlElement promocodeElement = null;
                string cProcessInfo = "";
                try
                {
                    // Get notes XML
                    var oXform = new Cms.xForm(ref myWeb.msException);
                    oXform.moPageXML = moPageXml;
                    // 

                    switch ((mcNotesXForm).ToLower() ?? "")
                    {
                        case "default":
                            {
                                oXform.NewFrm(formName);
                                oXform.submission(formName, action, "POST", "return form_check(this);");
                                oXform.Instance.InnerXml = "<Notes><Notes/></Notes>";
                                oFormGrp = oXform.addGroup(ref oXform.moXformElmt, "notes", "term4051", "Please enter any comments on your order here");
                                string argsClass = "";
                                int argnRows = 0;
                                int argnCols = 0;
                                oXform.addTextArea(ref oFormGrp, "Notes/Notes", false, "", ref argsClass, nRows: ref argnRows, nCols: ref argnCols);
                                if (moDiscount.bHasPromotionalDiscounts)
                                {
                                    // If oXform.Instance.FirstChild.SelectSingleNode("Notes") Is Nothing Then
                                    XmlElement localfirstElement1() { var argoElement = oXform.Instance; var ret = Tools.Xml.firstElement(ref argoElement); oXform.Instance = argoElement; return ret; }

                                    if (localfirstElement1().SelectSingleNode("Notes") is null)
                                    {
                                        // oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"))
                                        // Protean.Tools.Xml.firstElement(oXform.Instance).AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"))
                                        XmlElement localfirstElement() { XmlElement argoElement1 = (XmlElement)oXform.moXformElmt.SelectSingleNode("descendant-or-self::instance"); var ret = firstElement(ref argoElement1); return ret; }

                                        localfirstElement().AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"));
                                    }
                                    // oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"))
                                    // Protean.Tools.Xml.firstElement(oXform.Instance).AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"))
                                    XmlElement localfirstElement2() { XmlElement argoElement2 = (XmlElement)oXform.moXformElmt.SelectSingleNode("descendant-or-self::instance"); var ret = firstElement(ref argoElement2); return ret; }

                                    promocodeElement = (XmlElement)localfirstElement2().AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"));
                                    oXform.addInput(ref oFormGrp, "Notes/PromotionalCode", false, "Promotional Code", "");
                                }
                                oXform.addSubmit(ref oFormGrp, "Submit", "Continue");
                                break;
                            }
                        case "productspecific":
                            {
                                XmlElement oMasterFormXml = null;
                                foreach (XmlElement oOrderLine in oCart.SelectNodes("Item"))
                                {
                                    // get any Xform related to cart items
                                    long contentId = Convert.ToInt64(oOrderLine.GetAttribute("contentId"));
                                    sSql = "select nContentKey from tblContent c inner join tblContentRelation cr on cr.nContentChildId = c.nContentKey where c.cContentSchemaName = 'xform' and cr.nContentParentId = " + contentId;
                                    long FormId = Convert.ToInt64(myWeb.moDbHelper.GetDataValue(sSql));
                                    if (FormId != default)
                                    {
                                        var oFormXml = moPageXml.CreateElement("NewXform");
                                        oFormXml.InnerXml = myWeb.moDbHelper.getContentBrief((int)FormId);

                                        if (oMasterFormXml is null)
                                        {
                                            // Duplication the items for each qty in cart
                                            int n = 1;
                                            XmlElement oItem = (XmlElement)oFormXml.SelectSingleNode("descendant-or-self::Item");
                                            oItem.SetAttribute("name", oOrderLine.SelectSingleNode("Name").InnerText);
                                            oItem.SetAttribute("stockCode", oOrderLine.SelectSingleNode("productDetail/StockCode").InnerText);
                                            oItem.SetAttribute("number", n.ToString());

                                            int i;
                                            var loopTo = Convert.ToInt16(oOrderLine.GetAttribute("quantity"));
                                            for (i = 2; i <= loopTo; i++)
                                            {
                                                n = n + 1;
                                                XmlElement newItem = (XmlElement)oItem.CloneNode(true);
                                                newItem.SetAttribute("number", n.ToString());
                                                oItem.ParentNode.InsertAfter(newItem, oItem.ParentNode.LastChild);
                                            }
                                            oMasterFormXml = oFormXml;
                                        }
                                        else
                                        {
                                            // behaviour for appending additioanl product forms
                                            int n = 1;
                                            XmlElement oItem = (XmlElement)oFormXml.SelectSingleNode("descendant-or-self::Item");
                                            oItem.SetAttribute("name", oOrderLine.SelectSingleNode("Name").InnerText);
                                            oItem.SetAttribute("stockCode", oOrderLine.SelectSingleNode("productDetail/StockCode").InnerText);
                                            oItem.SetAttribute("number", n.ToString());

                                            int i;
                                            var loopTo1 = Convert.ToInt16(oOrderLine.GetAttribute("quantity"));
                                            for (i = 1; i <= loopTo1; i++)
                                            {
                                                XmlElement newItem = (XmlElement)oItem.CloneNode(true);
                                                newItem.SetAttribute("number", n.ToString());
                                                n = n + 1;
                                                XmlElement AddAfterNode = (XmlElement)oMasterFormXml.SelectSingleNode("Content/model/instance/Notes/Item[last()]");
                                                AddAfterNode.ParentNode.InsertAfter(newItem, AddAfterNode);
                                            }
                                        }
                                    }

                                }
                                // Load with repeats.
                                if (oMasterFormXml != null)
                                {
                                    var argoNode = oMasterFormXml.SelectSingleNode("descendant-or-self::Content");
                                    oXform.load(ref argoNode, true);
                                }

                                if (moDiscount.bHasPromotionalDiscounts)
                                {

                                    XmlElement oNotesRoot = (XmlElement)oXform.Instance.SelectSingleNode("Notes");
                                    if (oNotesRoot.SelectSingleNode("PromotionalCode") is null)
                                    {
                                        promocodeElement = (XmlElement)oNotesRoot.AppendChild(oMasterFormXml.OwnerDocument.CreateElement("PromotionalCode"));
                                    }

                                    oFormGrp = (XmlElement)oXform.moXformElmt.SelectSingleNode("descendant-or-self::group[1]");
                                    oXform.addInput(ref oFormGrp, "Notes/PromotionalCode", false, "Promotional Code", "");

                                }

                                if (oXform.moXformElmt != null)
                                {
                                    // add missing submission or submit buttons
                                    if (oXform.moXformElmt.SelectSingleNode("model/submission") is null)
                                    {
                                        // If oXform.moXformElmt.SelectSingleNode("model/instance/submission") Is Nothing Then
                                        oXform.submission(formName, action, "POST", "return form_check(this);");
                                    }
                                    if (oXform.moXformElmt.SelectSingleNode("descendant-or-self::submit") is null)
                                    {
                                        oXform.addSubmit(ref oXform.moXformElmt, "Submit", "Continue");
                                    }
                                    oXform.moXformElmt.SetAttribute("type", "xform");
                                    oXform.moXformElmt.SetAttribute("name", "notesForm");
                                }
                                else
                                {
                                    oXform.NewFrm(formName);
                                    oFormGrp = oXform.addGroup(ref oXform.moXformElmt, "notes", sLabel: "Missing File: Product has no form request ");
                                    // force to true so we move on.
                                    oXform.valid = true;
                                }

                                break;
                            }

                        default:
                            {
                                if (!oXform.load(mcNotesXForm))
                                {
                                    oXform.NewFrm(formName);
                                    oFormGrp = oXform.addGroup(ref oXform.moXformElmt, "notes", sLabel: "Missing File: " + mcNotesXForm);
                                }
                                else
                                {
                                    string cTicketTypes = moCartConfig["TicketTypes"];
                                    // 'Modify the notes for dependant on tickets
                                    int totalAttendees = 0;
                                    if (!string.IsNullOrEmpty(cTicketTypes))
                                    {
                                        if (!(moCartXml.SelectNodes("Order/Item[productDetail/Name[@ticketType!='']]").Count == 0))
                                        {
                                            string ticketType;
                                            // For Each ticketType In Split(cTicketTypes, ",")
                                            XmlElement oNotesRoot = (XmlElement)oXform.Instance.SelectSingleNode("Notes/Notes");
                                            XmlElement oBindRoot = (XmlElement)oXform.model.SelectSingleNode("bind");
                                            XmlElement oControlRoot = (XmlElement)oXform.moXformElmt.SelectSingleNode("group");

                                            // Case for Run
                                            XmlElement newElmt2;
                                            int nCount = 0;
                                            int i = 0;

                                            // For Each oItemElmt In moCartXml.SelectNodes("Order/Item[productDetail/Name[@ticketType='" & ticketType & "']]")
                                            foreach (XmlElement oItemElmt in moCartXml.SelectNodes("Order/Item"))
                                            {

                                                ticketType = oItemElmt.SelectSingleNode("productDetail/Name/@ticketType").InnerText;

                                                XmlElement blankElmt = (XmlElement)oXform.Instance.SelectSingleNode("Notes/Notes/Attendee[@type='" + ticketType + "'][1]");
                                                XmlElement blankBind = (XmlElement)oXform.model.SelectSingleNode("bind/bind[@nodeset='Attendee' and @class='" + ticketType + "'][1]");
                                                XmlElement blankControl = (XmlElement)oXform.moXformElmt.SelectSingleNode("group/group[contains(@class,'" + ticketType + "')][1]");

                                                var loopTo2 = Convert.ToInt16(oItemElmt.GetAttribute("quantity"));
                                                for (i = 1; i <= loopTo2; i++)
                                                {
                                                    totalAttendees = totalAttendees + 1;
                                                    // Update the instance
                                                    oNotesRoot.AppendChild(blankElmt.CloneNode(true));
                                                    XmlElement newElmt = (XmlElement)oNotesRoot.LastChild;
                                                    newElmt.SelectSingleNode("AttTicketType").InnerText = oItemElmt.SelectSingleNode("Name").InnerText + " - " + moCartConfig["TicketAttendeeLabel"] + " " + i;
                                                    newElmt.SetAttribute("id", ticketType + nCount);
                                                    newElmt.SetAttribute("itemId", oItemElmt.GetAttribute("id"));

                                                    newElmt = null;

                                                    // Update the binds
                                                    oBindRoot.AppendChild(blankBind.CloneNode(true));
                                                    newElmt = (XmlElement)oBindRoot.LastChild;
                                                    newElmt.SetAttribute("nodeset", "Attendee[@id='" + ticketType + nCount + "']");
                                                    foreach (XmlElement currentNewElmt2 in newElmt.SelectNodes("descendant-or-self::*"))
                                                    {
                                                        newElmt2 = currentNewElmt2;
                                                        if (!string.IsNullOrEmpty(newElmt2.GetAttribute("id")))
                                                        {
                                                            newElmt2.SetAttribute("id", newElmt2.GetAttribute("id") + "-" + ticketType + nCount);
                                                        }
                                                        // remove lead booker from all subsequent tickets
                                                        if (totalAttendees > 1 & newElmt2.GetAttribute("lead-booker-only") == "true")
                                                        {
                                                            newElmt2.SetAttribute("required", "false()");
                                                        }
                                                    }
                                                    newElmt = null;
                                                    // Update the controls
                                                    if (blankControl != null)
                                                    {
                                                        blankControl.SetAttribute("id", "ticket-form-" + totalAttendees);
                                                        oControlRoot.AppendChild(blankControl.CloneNode(true));
                                                    }
                                                    newElmt = (XmlElement)oControlRoot.LastChild;

                                                    var labelElmt = moPageXml.CreateElement("label");
                                                    labelElmt.InnerText = oItemElmt.SelectSingleNode("Name").InnerText + " - " + moCartConfig["TicketAttendeeLabel"] + " " + i;
                                                    newElmt.InsertBefore(labelElmt, newElmt.FirstChild);

                                                    foreach (XmlElement currentNewElmt21 in newElmt.SelectNodes("descendant-or-self::*[@bind]"))
                                                    {
                                                        newElmt2 = currentNewElmt21;
                                                        if (!string.IsNullOrEmpty(newElmt2.GetAttribute("bind")))
                                                        {
                                                            if (i != Convert.ToDouble(oItemElmt.GetAttribute("quantity")))
                                                            {
                                                                // remove all but the last delcarations
                                                                if (newElmt2.GetAttribute("bind").StartsWith("AttDeclaration"))
                                                                {
                                                                    // newElmt2.ParentNode.RemoveChild(newElmt2.PreviousSibling)
                                                                    XmlElement delGroup = (XmlElement)newElmt2.ParentNode;
                                                                    delGroup.SetAttribute("delete", Convert.ToString(true));
                                                                }
                                                            }

                                                            newElmt2.SetAttribute("bind", newElmt2.GetAttribute("bind") + "-" + ticketType + nCount);
                                                        }
                                                    }
                                                    if (totalAttendees > 1)
                                                    {
                                                        foreach (XmlElement currentNewElmt22 in newElmt.SelectNodes("descendant-or-self::*[@lead-booker-only='true']"))
                                                        {
                                                            newElmt2 = currentNewElmt22;
                                                            // remove lead booker from all subsequent tickets
                                                            newElmt2.ParentNode.RemoveChild(newElmt2);
                                                        }
                                                    }

                                                    newElmt = null;
                                                    nCount = nCount + 1;
                                                }

                                            }

                                            // remove the blanks
                                            foreach (XmlElement currentNewElmt23 in oControlRoot.SelectNodes("descendant-or-self::*[@delete]"))
                                            {
                                                newElmt2 = currentNewElmt23;
                                                newElmt2.ParentNode.RemoveChild(newElmt2);
                                            }


                                            foreach (var currentTicketType in cTicketTypes.Split(','))
                                            {
                                                ticketType = currentTicketType;
                                                // remove the initial versions
                                                XmlElement blankElmt = (XmlElement)oXform.Instance.SelectSingleNode("Notes/Notes/Attendee[@type='" + ticketType + "'][1]");
                                                XmlElement blankBind = (XmlElement)oXform.model.SelectSingleNode("bind/bind[@nodeset='Attendee' and @class='" + ticketType + "'][1]");
                                                XmlElement blankControl = (XmlElement)oXform.moXformElmt.SelectSingleNode("group/group[contains(@class,'" + ticketType + "')][1]");

                                                blankElmt.ParentNode.RemoveChild(blankElmt);
                                                blankBind.ParentNode.RemoveChild(blankBind);
                                                blankControl.ParentNode.RemoveChild(blankControl);
                                            }



                                        }
                                    }


                                    // add missing submission or submit buttons
                                    if (oXform.moXformElmt.SelectSingleNode("model/submission") is null)
                                    {
                                        // If oXform.moXformElmt.SelectSingleNode("model/instance/submission") Is Nothing Then
                                        oXform.submission(formName, action, "POST", "return form_check(this);");
                                    }
                                    if (oXform.moXformElmt.SelectSingleNode("descendant-or-self::submit") is null)
                                    {
                                        oXform.addSubmit(ref oXform.moXformElmt, "Submit", "Continue");
                                    }
                                    if (moDiscount.bHasPromotionalDiscounts)
                                    {
                                        XmlElement oSubmit = (XmlElement)oXform.moXformElmt.SelectSingleNode("descendant-or-self::submit");
                                        if (oSubmit != null)
                                        {
                                            oFormGrp = (XmlElement)oSubmit.ParentNode;
                                        }
                                        else
                                        {
                                            oFormGrp = oXform.addGroup(ref oXform.moXformElmt, "Promo", sLabel: "Enter Promotional Code");
                                        }
                                        if (oXform.Instance.SelectSingleNode("descendant-or-self::PromotionalCode") is null)
                                        {
                                            // If oXform.Instance.FirstChild.SelectSingleNode("Notes") Is Nothing Then
                                            XmlElement localfirstElement4() { var argoElement3 = oXform.Instance; var ret = Tools.Xml.firstElement(ref argoElement3); oXform.Instance = argoElement3; return ret; }

                                            if (localfirstElement4().SelectSingleNode("Notes") is null)
                                            {
                                                // ocNode.AppendChild(moPageXml.ImportNode(Protean.Tools.Xml.firstElement(newXml.DocumentElement), True))
                                                // oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"))
                                                // Protean.Tools.Xml.firstElement(oXform.Instance).AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"))
                                                XmlElement localfirstElement3() { XmlElement argoElement4 = (XmlElement)oXform.moXformElmt.SelectSingleNode("descendant-or-self::instance"); var ret = firstElement(ref argoElement4); return ret; }

                                                localfirstElement3().AppendChild(oXform.Instance.OwnerDocument.CreateElement("Notes"));
                                            }
                                            // oXform.Instance.FirstChild.AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"))
                                            // Protean.Tools.Xml.firstElement(oXform.Instance).AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"))
                                            XmlElement localfirstElement5() { XmlElement argoElement5 = (XmlElement)oXform.moXformElmt.SelectSingleNode("descendant-or-self::instance"); var ret = firstElement(ref argoElement5); return ret; }

                                            promocodeElement = (XmlElement)localfirstElement5().AppendChild(oXform.Instance.OwnerDocument.CreateElement("PromotionalCode"));
                                            oXform.addInput(ref oFormGrp, "Notes/PromotionalCode", false, "Promotional Code", "");
                                        }
                                    }
                                }

                                break;
                            }
                    }

                    // External promo code checks
                    if (promocodeElement != null & !string.IsNullOrEmpty(promocodeFromExternalRef))
                    {
                        promocodeElement.InnerText = promocodeFromExternalRef;
                        // Promo code is officially in the process, so we can ditch any transitory variables.
                        promocodeFromExternalRef = "";
                    }

                    // Open database for reading and writing

                    sSql = "select * from tblCartOrder where nCartOrderKey=" + mnCartId;
                    oDs = moDBHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                    foreach (DataRow oRow in oDs.Tables["Order"].Rows)
                    {
                        // load existing notes from Cart
                        sXmlContent = oRow["cClientNotes"] + "".ToString();
                        if (!string.IsNullOrEmpty(sXmlContent))
                        {

                            var savedInstance = moPageXml.CreateElement("instance");
                            moPageXml.PreserveWhitespace = false;
                            savedInstance.InnerXml = sXmlContent;

                            if (oXform.Instance.SelectNodes("*/*/*").Count > savedInstance.SelectNodes("*/*/*").Count)
                            {
                                // we have a greater amount of childnodes we need to merge....

                                // Dim oStepElmtCount As Integer = 0

                                // step through each child element and replace where attributes match, leaving final
                                foreach (XmlElement oStepElmt in oXform.Instance.SelectNodes("*/*/*"))
                                {

                                    XmlElement replacementNode = (XmlElement)savedInstance.SelectSingleNode("*/*/*[@id='" + oStepElmt.GetAttribute("id") + "']");

                                    if (replacementNode != null)
                                    {
                                        oStepElmt.ParentNode.ReplaceChild(replacementNode.CloneNode(true), oStepElmt);
                                    }


                                    // Dim attXpath As String = oStepElmt.Name
                                    // Dim attElmt As XmlAttribute
                                    // Dim bfirst As Boolean = True
                                    // For Each attElmt In oStepElmt.Attributes
                                    // If bfirst Then attXpath = attXpath & "["
                                    // If Not bfirst Then attXpath = attXpath & " and "
                                    // attXpath = attXpath + "@" & attElmt.Name & "='" & attElmt.Value & "'"
                                    // bfirst = False
                                    // Next
                                    // If Not bfirst Then attXpath = attXpath & "]"

                                    // If Not savedInstance.SelectSingleNode("*/*/" & attXpath) Is Nothing Then
                                    // oStepElmt.ParentNode.ReplaceChild(savedInstance.SelectSingleNode("*/*/" & attXpath).CloneNode(True), oStepElmt)
                                    // End If
                                    // oStepElmtCount = oStepElmtCount + 1
                                }
                            }

                            else
                            {
                                oXform.Instance.InnerXml = sXmlContent;
                            }

                        }

                        // If this xform is being submitted

                        if (oXform.isSubmitted() | myWeb.moRequest["Submit"] == "Continue" | myWeb.moRequest["Submit"] == "Search")
                        {
                            oXform.updateInstanceFromRequest();
                            oXform.validate();
                            if (!string.IsNullOrEmpty(moCartConfig["NotesToContactsXSL"]))
                            {

                                oXform.Instance.SetAttribute("userId", mnEwUserId.ToString());
                                oXform.Instance.SetAttribute("cartId", mnCartId.ToString());

                                var oInstanceDoc = new XmlDocument();
                                oInstanceDoc.LoadXml(oXform.Instance.OuterXml);

                                var oTransform = new Protean.XmlHelper.Transform(ref myWeb, moServer.MapPath(moCartConfig["NotesToContactsXSL"]), false);
                                var ImportElmt = oTransform.ProcessDocument(oInstanceDoc).DocumentElement;

                                moDBHelper.importObjects(ImportElmt, mnCartId.ToString(), "");

                                oTransform = (Protean.XmlHelper.Transform)null;

                            }
                            if (oXform.valid == true)
                            {
                                oRow["cClientNotes"] = oXform.Instance.InnerXml;
                                // if we are useing the notes as a search facility for products
                                if (myWeb.moRequest["Submit"] == "Search")
                                {
                                    mcCartCmd = "Search";
                                }
                                else
                                {
                                    mcCartCmd = "SkipAddress";
                                }
                            }
                        }
                    }
                    moDBHelper.updateDataset(ref oDs, "Order", true);

                    oDs.Clear();
                    oDs = null;
                    oXform.addValues();
                    notesXformRet = oXform;
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "notesXform", ex, "", cProcessInfo, gbDebug);
                    return (Cms.xForm)null;
                }

                return notesXformRet;

            }


            public void SetClientNotes(string Notes)
            {
                string sSql = "";
                DataSet oDs;
                string cProcessInfo = "SetClientNotes";
                try
                {
                    if (mnCartId > 0)
                    {
                        // Update Seller Notes:
                        sSql = "select * from tblCartOrder where nCartOrderKey = " + mnCartId;
                        oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                        foreach (DataRow oRow in oDs.Tables["Order"].Rows)
                            oRow["cClientNotes"] = Notes;
                        myWeb.moDbHelper.updateDataset(ref oDs, "Order");
                    }
                }

                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "UpdateSellerNotes", ex, "", cProcessInfo, gbDebug);
                }

            }


            public string AddClientNotes(string sNotesText)
            {
                string cProcessInfo = "AddClientNotes";
                string sSql;
                DataSet oDs;
                string sXmlContent;
                try
                {
                    // myCart.moCartXml
                    if (mnCartId > 0)
                    {
                        sSql = "select * from tblCartOrder where nCartOrderKey=" + mnCartId;
                        oDs = myWeb.moDbHelper.getDataSetForUpdate(sSql, "Order", "Cart");
                        foreach (DataRow oRow in oDs.Tables["Order"].Rows)
                        {
                            // load existing notes from Cart
                            sXmlContent = oRow["cClientNotes"]?.ToString() ?? "";
                            if (string.IsNullOrEmpty(sXmlContent))
                            {
                                sXmlContent = "<Notes><Notes/><PromotionalCode/></Notes>";
                            }
                            var NotesXml = new XmlDocument();
                            NotesXml.LoadXml(sXmlContent);

                            if (NotesXml.SelectSingleNode("Notes/Notes") is null)
                            {
                                NotesXml.DocumentElement.AppendChild(NotesXml.CreateElement("Notes"));
                            }

                            NotesXml.SelectSingleNode("Notes/Notes").InnerText = sNotesText;

                            oRow["cClientNotes"] = NotesXml.OuterXml;
                        }
                        myWeb.moDbHelper.updateDataset(ref oDs, "Order", true);
                        oDs.Clear();
                        oDs = null;

                        return sNotesText;
                    }
                    else
                    {

                        return "";
                    }
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, mcModuleName, "AddDiscountCode", ex, "", cProcessInfo, gbDebug);
                    return null;
                }
            }

        }
    }
}
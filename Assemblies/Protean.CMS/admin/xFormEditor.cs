using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Protean
{
    public partial class Cms
    {
        public partial class Admin
        {
            public class XFormEditor : Protean.xForm
            {

                private const string _moduleName = "Protean.Cms.Admin.XFormEditor";
                public const string FORMPATH = "/xforms/content";
                private bool gbDebug = false;
                private XmlElement _masterInstance;
                protected Protean.xForm _masterXform;
                private System.Web.HttpRequest _request;
                protected string _schema = "generic";
                public Cms myWeb;

                public XFormEditor(ref Cms aWeb, long contentId = 0L) : base(ref aWeb.msException)
                {

                    // Set the Web context variables
                    myWeb = aWeb;
                    _request = myWeb.moRequest;
                    moPageXML = myWeb.moPageXml;
                    gbDebug = false;

                    // Create the form
                    CreateMasterForm(contentId);

                }

                public XmlElement MasterInstance
                {
                    get
                    {
                        return _masterInstance;
                    }
                }


                public bool Ready
                {
                    get
                    {
                        try
                        {
                            return _masterXform != null;
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                }

                public string ContentSchema
                {
                    get
                    {
                        return _schema;
                    }
                }

                protected virtual string[] schemaPreferenceList
                {
                    get
                    {
                        var schemaList = new string[3];
                        schemaList[0] = _schema;
                        schemaList[1] = "generic";
                        return schemaList;
                    }
                }

                protected void CreateMasterForm(long contentId = 0L)
                {
                    try
                    {
                        _masterXform = new Protean.xForm(ref myWeb.msException);
                        _masterXform.moPageXML = moPageXML;
                        _masterInstance = moPageXML.CreateElement("instance");

                        // If content id has been set, then get the instance
                        if (contentId > 0L)
                        {
                            _masterInstance.InnerXml = myWeb.moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Content, contentId);
                            var argoNode = _masterInstance.SelectSingleNode("descendant-or-self::cContentXmlDetail/Content");
                            _masterXform.load(ref argoNode);
                            _schema = _masterInstance.SelectSingleNode("descendant-or-self::cContentSchemaName").InnerText;
                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "CreateMasterForm", ex, "", "", gbDebug);
                    }
                }

                protected bool loadControlForm(string controlType)
                {
                    //bool success = false;
                    try
                    {

                        foreach (string schemaType in schemaPreferenceList)
                        {

                            if (load(FORMPATH + "/" + schemaType + "." + controlType + ".xml", myWeb.maCommonFolders))
                            {
                                //success = true;
                                break;
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "loadForm", ex, "", "", gbDebug);
                        return false;
                    }

                    return default;

                }


                /// <summary>
                /// This function returns an xForm to update the form being edited
                /// </summary>
                /// <param name="cRef">The Ref or Bind of the form element to be edited</param>
                /// <param name="cParRef">The Ref or Bind of the form element under which the new element will be inserted</param>
                /// <returns></returns>
                /// <remarks></remarks>

                public virtual XmlElement xFrmEditXFormGroup(string cRef, string cParRef = "")
                {

                    XmlElement oElmt;
                    string cProcessInfo = "cRef = " + cRef + ", ParRef=" + cParRef;
                    string cMode;
                    string newRef;

                    try
                    {
                        // Set the update mode
                        cMode = Interaction.IIf(string.IsNullOrEmpty(cRef), "Add", "Edit").ToString();

                        // Create the form that we're going to populate for updating this xform control
                        NewFrm("EditGroup");

                        // Load in the form from a file
                        loadControlForm("group");

                        // load a default content xform if no alternative.
                        if (!string.IsNullOrEmpty(cRef))
                        {
                            oElmt = (XmlElement)_masterXform.moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']");
                            LoadInstanceFromInnerXml(oElmt.OuterXml);
                        }

                        if (isSubmitted())
                        {
                            updateInstanceFromRequest();
                            validate();
                            if (valid)
                            {
                                if (!string.IsNullOrEmpty(cRef))
                                {
                                    // drop the instance back into the full xform
                                    var oNode = _masterXform.moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']");
                                    oNode.ParentNode.ReplaceChild(Instance.FirstChild, oNode);
                                }
                                else
                                {
                                    // add new
                                    var oNode = _masterXform.moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='" + cParRef + "' or @bind='" + cParRef + "']");
                                    newRef = _masterXform.getNewRef(goRequest["cRef"]);
                                    oElmt = (XmlElement)Instance.FirstChild;
                                    oElmt.SetAttribute("ref", newRef);
                                    oNode.AppendChild(Instance.FirstChild);
                                }
                            }
                            else
                            {
                                addValues();
                            }
                        }
                        else
                        {
                            addValues();
                        }

                        return moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }


                public virtual XmlElement xFrmDeleteElement(string cRef, string cPosIndex)
                {

                    XmlElement oFrmElmt;
                    XmlNode oNode;
                    string cProcessInfo = "cRef = " + cRef + ", cPosIndex=" + cPosIndex;

                    try
                    {

                        // Generate the Delete form within the component

                        // Indentify the node by ref and position, if given.
                        if (!string.IsNullOrEmpty(cPosIndex))
                        {
                            oNode = _masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']/item[" + cPosIndex + "]");
                        }
                        else
                        {
                            oNode = _masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']");
                        }

                        // Create the form
                        NewFrm("EditSelect");
                        submission("EditInput", "", "post");
                        oFrmElmt = addGroup(ref moXformElmt, "EditGroup", "", "Delete Element");
                        //XmlNode argoNode = oFrmElmt;
                        addNote(ref oFrmElmt, Protean.xForm.noteTypes.Alert, "Are you sure you want to delete this element - \"" + oNode.SelectSingleNode("label").InnerText + "\"");
                        //oFrmElmt = (XmlElement)argoNode;
                        addSubmit(ref oFrmElmt, "", "Delete Element");
                        LoadInstanceFromInnerXml("<delete/>");

                        // Handle the submission
                        if (isSubmitted())
                        {
                            updateInstanceFromRequest();
                            validate();
                            if (valid)
                            {

                                // Delete the node and/or bind
                                deleteElementAction(ref oNode, cRef, cPosIndex);
                            }

                            else
                            {
                                addValues();
                            }
                        }
                        else
                        {
                            addValues();
                        }

                        return moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmDeleteElement", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                /// <summary>
                /// Deletes nodes that relate to the element / item.
                /// Problem we face is that we don't know what to delete from the instance, 
                /// so we will assume that the instance node we're loooking for has a matching @ref attribute.
                /// If it's an item we will also assume that the @ref node has children that have a child node of value which
                /// matches the value of the item being deleted.
                /// </summary>
                /// <param name="ref"></param>
                /// <param name="position"></param>
                /// <remarks></remarks>
                protected virtual void deleteElementAction(ref XmlNode node, string @ref, string position = "")
                {
                    string processInfo = "";
                    string value = "";
                    XmlNode node2;
                    try
                    {

                        // If we're deleting an item, then we don't need to remove the bind, but we do need to remove the item
                        if (!string.IsNullOrEmpty(position))
                        {
                            // remove related node's item
                            value = node.SelectSingleNode("value").InnerText;
                            node2 = _masterXform.Instance.SelectSingleNode("//*[@ref='" + @ref + "']");
                            if (node2.SelectSingleNode("*[value/node()='" + value + "']") != null)
                            {
                                // remove existing node
                                node2.RemoveChild(node2.SelectSingleNode("*[value/node()='" + value + "']"));
                            }
                        }
                        else
                        {
                            // remove related node
                            node2 = _masterXform.Instance.SelectSingleNode("//*[@ref='" + @ref + "']");
                            if (node2 != null)
                            {
                                node2.ParentNode.RemoveChild(node2);
                            }

                            // remove bind
                            if (_masterXform.model.SelectSingleNode("descendant-or-self::bind[@id='" + @ref + "']") != null)
                            {
                                node2 = _masterXform.model.SelectSingleNode("descendant-or-self::bind[@id='" + @ref + "']");
                                node2.ParentNode.RemoveChild(node2);
                            }

                        }

                        // remove the element itself
                        node.ParentNode.RemoveChild(node);
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "deleteElementAction", ex, "", processInfo, gbDebug);
                    }
                }

                public void moveElement(long nContentId, string cRef, string ewCmd, long nItemIndex = 0L)
                {

                    XmlElement oElmt;

                    string cProcessInfo = "nContentId = " + nContentId + ",cRef = " + cRef + ",ewCmd = " + ewCmd + ", nItemIndex=" + nItemIndex;

                    try
                    {
                        if (nItemIndex == 0L)
                        {
                            oElmt = (XmlElement)_masterXform.moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']");
                        }
                        else
                        {
                            oElmt = (XmlElement)_masterXform.moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']/item[" + nItemIndex + "]");
                        }
                        switch (ewCmd ?? "")
                        {
                            case "MoveTop":
                            case "MoveItemTop":
                                {
                                    oElmt.ParentNode.InsertBefore(oElmt.CloneNode(true), oElmt.ParentNode.FirstChild);
                                    oElmt.ParentNode.RemoveChild(oElmt);
                                    break;
                                }
                            case "MoveUp":
                            case "MoveItemUp":
                                {
                                    oElmt.ParentNode.InsertBefore(oElmt.CloneNode(true), oElmt.PreviousSibling);
                                    oElmt.ParentNode.RemoveChild(oElmt);
                                    break;
                                }
                            case "MoveDown":
                            case "MoveItemDown":
                                {
                                    oElmt.ParentNode.InsertAfter(oElmt.CloneNode(true), oElmt.NextSibling);
                                    oElmt.ParentNode.RemoveChild(oElmt);
                                    break;
                                }
                            case "MoveBottom":
                            case "MoveItemBottom":
                                {
                                    oElmt.ParentNode.AppendChild(oElmt.CloneNode(true));
                                    oElmt.ParentNode.RemoveChild(oElmt);
                                    break;
                                }
                            case "DeleteItem":
                                {
                                    oElmt.ParentNode.RemoveChild(oElmt);
                                    break;
                                }
                        }
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "moveElement", ex, "", cProcessInfo, gbDebug);
                    }

                }

                public virtual XmlElement xFrmEditXFormItem(string cRef, long nItemIndex)
                {

                    XmlElement oElmt = null;
                    XmlNode oNode;
                    string sValue;

                    var nCount = default(long);

                    string cProcessInfo = "cRef = " + cRef + ", nItemIndex=" + nItemIndex;

                    try
                    {
                        // load the xform to be edited

                        if (nItemIndex != 0L)
                        {
                            oElmt = (XmlElement)_masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']/item[" + nItemIndex + "]");
                        }

                        // Create the form that we're going to populate for updating this xform control
                        NewFrm("EditSelect");

                        // Load in the form from a file
                        loadControlForm("item");

                        // set the instance from the item loaded from the xform
                        if (nItemIndex != 0L)
                        {

                            Instance.AppendChild(oElmt.CloneNode(true));

                            // add weighting and correct flag to the item node from answer
                            oElmt = (XmlElement)Instance.FirstChild;
                            sValue = oElmt.SelectSingleNode("value").InnerText;
                        }

                        else
                        {

                            // Multi choice, auto indexing
                            foreach (XmlNode currentONode in _masterXform.moXformElmt.SelectNodes("group/descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']/item"))
                            {
                                oNode = currentONode;
                                if (Convert.ToInt16(oNode.SelectSingleNode("value").InnerText) > nCount)
                                {
                                    nCount = Convert.ToInt16(oNode.SelectSingleNode("value").InnerText);
                                }
                            }
                            LoadInstanceFromInnerXml("<item><label/><value>" + (nCount + 1L) + "</value></item>");
                        }

                        if (isSubmitted())
                        {
                            updateInstanceFromRequest();
                            validate();
                            if (valid)
                            {

                                sValue = Instance.SelectSingleNode("item/value").InnerText;

                                if (nItemIndex != 0L)
                                {
                                    // drop the instance back into the full xform
                                    oNode = _masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']/item[" + nItemIndex + "]");
                                    oNode.ParentNode.ReplaceChild(Instance.FirstChild, oNode);
                                }
                                else
                                {
                                    // add new
                                    oNode = _masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']");
                                    oNode.AppendChild(Instance.FirstChild);
                                }
                                oNode = null;
                            }
                            else
                            {
                                addValues();
                            }
                        }
                        else
                        {
                            addValues();
                        }

                        return moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmEditXFormItem", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }

                public virtual XmlElement xFrmEditXFormInput(string cRef, string cParRef = "", string cElementType = "")
                {

                    XmlElement oElmt;
                    //string cBind = "";
                    XmlNode oNode;
                    string newRef;
                    string cProcessInfo = "";

                    try
                    {

                        // Determine the node we're looking at
                        if (!string.IsNullOrEmpty(cRef))
                        {
                            oElmt = (XmlElement)_masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']");
                            cElementType = oElmt.Name;
                        }

                        // Load in the form
                        switch (cElementType ?? "")
                        {
                            case "select1":
                                {

                                    // Create a new form
                                    NewFrm("EditSelect");

                                    // Load in the form from a file
                                    loadControlForm("select1");
                                    break;
                                }

                            case "select":
                                {

                                    // Create a new form
                                    NewFrm("EditSelect");

                                    // Load in the form from a file
                                    loadControlForm("select");
                                    break;
                                }

                            case "input":
                                {

                                    // Create a new form
                                    NewFrm("EditSelect");

                                    // Load in the form from a file
                                    loadControlForm("input");
                                    break;
                                }

                            case "textarea":
                                {

                                    // Create a new form
                                    NewFrm("EditSelect");

                                    // Load in the form from a file
                                    loadControlForm("textarea");
                                    break;
                                }

                        }

                        // Load existing data
                        if (!string.IsNullOrEmpty(cRef))
                        {
                            oElmt = (XmlElement)_masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']");
                            LoadInstanceFromInnerXml(oElmt.OuterXml);
                        }


                        if (isSubmitted())
                        {
                            updateInstanceFromRequest();
                            validate();

                            if (valid)
                            {

                                if (string.IsNullOrEmpty(cRef))
                                {
                                    newRef = _masterXform.getNewRef("control");
                                }
                                else
                                {
                                    newRef = cRef;
                                }

                                oElmt = null;

                                // remove anything unnessesary before save
                                switch (cElementType ?? "")
                                {
                                    case "select1":
                                    case "select":
                                        {
                                            // remove any empty item nodes
                                            foreach (XmlNode currentONode in Instance.FirstChild.SelectNodes("item"))
                                            {
                                                oNode = currentONode;
                                                if (oNode.FirstChild is null)
                                                {
                                                    oNode.ParentNode.RemoveChild(oNode);
                                                }
                                                else if (string.IsNullOrEmpty(oNode.FirstChild.InnerText))
                                                {
                                                    oNode.ParentNode.RemoveChild(oNode);
                                                }
                                            }

                                            break;
                                        }
                                }

                                // drop the instance back into the full xform
                                if (!string.IsNullOrEmpty(cRef))
                                {
                                    // replace existing
                                    oNode = _masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" + cRef + "' or @bind='" + cRef + "']");
                                    oNode.ParentNode.ReplaceChild(Instance.FirstChild, oNode);
                                }
                                else
                                {
                                    // add new
                                    oNode = _masterXform.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" + cParRef + "' or @bind='" + cParRef + "']");
                                    oElmt = (XmlElement)Instance.FirstChild;

                                    // TODO: XFormEditor (Generic) how do we identify the node xpath to create the bind.
                                    // This is just for the EonicWeb Generic xformeditor function
                                    // not for anything overridden like EonicLMS

                                    // cBind = "whatgoeshere[@ref='" & newRef & "']"
                                    // oElmt.SetAttribute("bind", newRef)
                                    // _masterXform.addBind(newRef, cBind)

                                    oNode.AppendChild(Instance.FirstChild);
                                }
                            }

                            else
                            {
                                addValues();
                            }
                        }
                        else
                        {
                            addValues();
                        }

                        return moXformElmt;
                    }

                    catch (Exception ex)
                    {
                        stdTools.returnException(ref myWeb.msException, _moduleName, "xFrmEditXFormInput", ex, "", cProcessInfo, gbDebug);
                        return null;
                    }
                }








                // Public Overridable Function xFrmEditXFormInput(ByVal cRef As String, Optional ByVal cParRef As String = "", Optional ByVal cElementType As String = "") As XmlElement
                // Dim oFrmElmt As XmlElement
                // Dim oRpt1Elmt As XmlElement
                // Dim oSelElmt As XmlElement

                // Dim oElmt As XmlElement
                // Dim oElmt1 As XmlElement
                // Dim oElmt2 As XmlElement
                // Dim oElmt3 As XmlElement
                // Dim oElmt4 As XmlElement
                // Dim nCount As Long
                // Dim sValidAnswers As String = ""

                // Dim nAnswerCount As Long
                // Dim cReqd As String
                // Dim cBind As String = ""

                // Dim oNode As XmlNode

                // Dim i As Integer

                // Dim newRef As String

                // Dim cProcessInfo As String = ""

                // Try

                // If cRef <> "" Then
                // oElmt = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']")
                // cElementType = oElmt.Name
                // nAnswerCount = oElmt.SelectNodes("item").Count + 1
                // Else
                // nAnswerCount = 4
                // End If

                // Select Case cElementType
                // Case "select1"

                // Me.NewFrm("EditSelect")

                // Me.submission("EditInput", "", "post", "return form_check(this)")

                // oFrmElmt = Me.addGroup(Me.moXformElmt, "EditSelect1", "", "Edit Select1")

                // Me.addInput(oFrmElmt, "ref", True, "Question Ref", "hidden")
                // Me.addBind("ref", "select1/@ref", "true()")

                // Me.addTextArea(oFrmElmt, "cName", True, "Question", "xhtml")
                // Me.addBind("cName", "select1/label", "true()")

                // Me.addTextArea(oFrmElmt, "cDesc", True, "Further Details", "xhtml")
                // Me.addBind("cDesc", "select1/div[@class='description']", "false()")

                // Me.addTextArea(oFrmElmt, "cRecRead", True, "Recommended Reading", "xhtml")
                // Me.addBind("cRecRead", "select1/div[@class='recRead']", "false()")

                // oSelElmt = Me.addSelect1(oFrmElmt, "cAppearance", True, "Appearance", "", ApperanceTypes.Minimal)
                // Me.addOption(oSelElmt, "Radio Buttons", "full")
                // Me.addOption(oSelElmt, "Dropdown Selector", "minimal")
                // Me.addBind("cAppearance", "select1/@appearance", "true()")

                // For i = 1 To nAnswerCount
                // oRpt1Elmt = Me.addGroup(oFrmElmt, "Answer " & i, "horizontal", "Answer " & i)
                // Me.addTextArea(oRpt1Elmt, "A" & i, True, "Answer", "xhtml answerEditor")
                // If i = nAnswerCount Then
                // cReqd = "false()"
                // Else
                // cReqd = "true()"
                // End If
                // Me.addBind("A" & i, "select1/item[" & i & "]/label", cReqd)

                // oSelElmt = Me.addSelect1(oRpt1Elmt, "ATick", True, "Correct ", "", ApperanceTypes.Full)
                // Me.addOption(oSelElmt, "", i)
                // Next
                // Me.addBind("ATick", "select1/@correctIndex", "false()")

                // Me.addRange(oFrmElmt, "AWeighting", True, "Weighting", "0", "100", "10", "short")

                // Me.addBind("AWeighting", "select1/@weighting", "true()")

                // Me.addSubmit(oFrmElmt, "", "Save Group")

                // Case "select"

                // Me.NewFrm("EditSelect")

                // Me.submission("EditInput", "", "post", "return form_check(this)")

                // oFrmElmt = Me.addGroup(Me.moXformElmt, "EditSelect", "", "Edit Select")

                // Me.addInput(oFrmElmt, "ref", True, "Question Ref", "hidden")
                // Me.addBind("ref", "select/@ref", "true()")

                // Me.addTextArea(oFrmElmt, "cName", True, "Question", "xhtml")
                // Me.addBind("cName", "select/label", "true()")

                // Me.addTextArea(oFrmElmt, "cDesc", True, "Further Details", "xhtml")
                // Me.addBind("cDesc", "select/div[@class='description']", "false()")

                // Me.addTextArea(oFrmElmt, "cRecRead", True, "Recommended Reading", "xhtml")
                // Me.addBind("cRecRead", "select/div[@class='recRead']", "false()")

                // oSelElmt = Me.addSelect1(oFrmElmt, "cAppearance", True, "Appearance", "", ApperanceTypes.Minimal)
                // Me.addOption(oSelElmt, "CheckBoxes", "full")
                // Me.addOption(oSelElmt, "Dropdown Selector", "minimal")
                // Me.addBind("cAppearance", "select/@appearance", "true()")

                // For i = 1 To nAnswerCount
                // oRpt1Elmt = Me.addGroup(oFrmElmt, "Answer " & i, "horizontal", "Answer " & i)
                // Me.addTextArea(oRpt1Elmt, "A" & i, True, "Answer", "xhtml answerEditor")
                // Me.addBind("A" & i, "select/item[" & i & "]/label", "false()")

                // oSelElmt = Me.addSelect(oRpt1Elmt, "A" & i & "Tick", True, "Correct ", "", ApperanceTypes.Full)
                // Me.addOption(oSelElmt, "", "true")
                // Me.addBind("A" & i & "Tick", "select/item[" & i & "]/@correct", "false()")

                // Me.addRange(oRpt1Elmt, "A" & i & "Weighting", True, "Weighting", "0", "100", "10", "short")

                // Me.addBind("A" & i & "Weighting", "select/item[" & i & "]/@weighting", "false()")
                // Next

                // Me.addSubmit(oFrmElmt, "", "Save Group")

                // Case "input"

                // Me.NewFrm("EditSelect")

                // Me.submission("EditInput", "", "post", "return form_check(this)")

                // oFrmElmt = Me.addGroup(Me.moXformElmt, "EditInput", "", "Edit Input")

                // '  Me.addInput(oFrmElmt, "ref", True, "Question Ref", "hidden")
                // ' Me.addBind("ref", "input/@ref", "true()")

                // Me.addTextArea(oFrmElmt, "cName", True, "Question", "xhtml")
                // Me.addBind("cName", "input/label", "true()")

                // Me.addTextArea(oFrmElmt, "cDesc", True, "Further Details", "xhtml")
                // Me.addBind("cDesc", "input/div[@class='description']", "false()")

                // Me.addTextArea(oFrmElmt, "cRecRead", True, "Recommended Reading", "xhtml")
                // Me.addBind("cRecRead", "input/div[@class='recRead']", "false()")

                // Me.addInput(oFrmElmt, "cAnswers", True, "Valid Answers")
                // Me.addBind("cAnswers", "input/@validAnswers", "false()")
                // Me.addNote("cAnswers", xForm.noteTypes.Hint, "Comma separated list of valid answers")

                // Me.addRange(oFrmElmt, "cWeighting", True, "Weighting", "0", "100", "10")
                // Me.addBind("cWeighting", "input/@weighting", "true()")

                // Me.addSubmit(oFrmElmt, "", "Save Group")

                // Case "textarea"

                // Me.NewFrm("EditSelect")

                // Me.submission("EditInput", "", "post", "return form_check(this)")

                // oFrmElmt = Me.addGroup(Me.moXformElmt, "EditInput", "", "Edit Comprehension Question")

                // Me.addTextArea(oFrmElmt, "cName", True, "Question", "xhtml")
                // Me.addBind("cName", "textarea/label", "true()")

                // Me.addTextArea(oFrmElmt, "cDesc", True, "Further Details", "xhtml")
                // Me.addBind("cDesc", "textarea/div[@class='description']", "false()")

                // Me.addTextArea(oFrmElmt, "cRecRead", True, "Recommended Reading", "xhtml")
                // Me.addBind("cRecRead", "textarea/div[@class='recRead']", "false()")

                // Me.addSubmit(oFrmElmt, "", "Save Group")

                // End Select

                // ' load a default content xform if no alternative.

                // If cRef <> "" Then
                // oElmt = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']")
                // Select Case cElementType
                // Case "select1"
                // 'step through the answers in the question were editing
                // i = 1
                // Dim bResult As Boolean = False
                // For Each oElmt2 In oElmt.SelectNodes("item")

                // 'get the answer value
                // Dim sVal As String = oElmt2.SelectSingleNode("value").InnerText
                // 'see if that value is in score of the answers fot that question
                // If Not moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']/score[value/node()='" & sVal & "']") Is Nothing Then
                // oElmt.SetAttribute("correctIndex", i)
                // oElmt3 = moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']/score[value/node()='" & sVal & "']")
                // oElmt.SetAttribute("weighting", oElmt3.GetAttribute("weighting"))
                // bResult = True
                // End If
                // i = i + 1

                // Next
                // If Not bResult Then ' add in the empties
                // oElmt.SetAttribute("correctIndex", "")
                // oElmt.SetAttribute("weighting", "")
                // End If
                // Case "select"
                // 'step through the answers in the question were editing
                // For Each oElmt2 In oElmt.SelectNodes("item")
                // 'get the answer value
                // Dim sVal As String = oElmt2.SelectSingleNode("value").InnerText
                // 'see if that value is in score of the answers fot that question
                // If Not moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']/score[value/node()='" & sVal & "']") Is Nothing Then
                // oElmt2.SetAttribute("correct", "true")
                // oElmt3 = moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']/score[value/node()='" & sVal & "']")
                // oElmt2.SetAttribute("weighting", oElmt3.GetAttribute("weighting"))
                // Else
                // oElmt2.SetAttribute("correct", "false")
                // oElmt2.SetAttribute("weighting", "")
                // End If
                // Next
                // Case "input"
                // For Each oElmt2 In moXForm2Edit.Instance.SelectNodes("results/answers/answer[@ref='" & cRef & "']/score/value")
                // If sValidAnswers = "" Then
                // sValidAnswers = oElmt2.InnerText
                // Else
                // sValidAnswers = sValidAnswers & ", " & oElmt2.InnerText
                // End If
                // Next
                // oElmt.SetAttribute("validAnswers", sValidAnswers)
                // oElmt.SetAttribute("weighting", moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']/score/@weighting").Value)
                // End Select

                // Me.Instance.AppendChild(oElmt.CloneNode(True))

                // Else
                // Select Case cElementType
                // Case "select1"
                // Me.Instance.InnerXml = "<select1 bind="""" appearance="""" correctIndex="""" weighting=""""><label/><div class=""description""/><div class=""recRead""/></select1>"
                // Case "select"
                // Me.Instance.InnerXml = "<select bind="""" appearance=""""><label/><div class=""description""/><div class=""recRead""/></select>"
                // Case "input"
                // Me.Instance.InnerXml = "<input bind="""" validAnswers="""" weighting=""""><label/><div class=""description""/><div class=""recRead""/></input>"
                // Case "textarea"
                // Me.Instance.InnerXml = "<textarea bind="""" rows=""20"" class=""textarea_compquiz""><label/><div class=""description""/><div class=""recRead""/></textarea>"

                // End Select
                // End If

                // Select Case cElementType
                // Case "select1", "select"
                // 'ensure we have the right number of blank item nodes to update.
                // For i = 1 To nAnswerCount
                // If Me.Instance.FirstChild.SelectSingleNode("item[" & i & "]") Is Nothing Then
                // 'populate new nodes with values
                // For Each oNode In Me.Instance.FirstChild.SelectNodes("item")
                // If CInt(oNode.SelectSingleNode("value").InnerText) > nCount Then
                // nCount = CInt(oNode.SelectSingleNode("value").InnerText)
                // End If
                // Next
                // oElmt1 = Me.addOption(Me.Instance.FirstChild, "", nCount + 1)
                // If cElementType = "select" Then
                // oElmt1.SetAttribute("correct", "")
                // oElmt1.SetAttribute("weighting", "")
                // End If
                // End If
                // Next

                // End Select

                // If Me.isSubmitted Then
                // Me.updateInstanceFromRequest()
                // Me.validate()

                // If Me.valid Then

                // If cRef = "" Then
                // newRef = moXForm2Edit.getNewRef("Q")
                // Else
                // newRef = cRef
                // End If

                // oElmt = Nothing

                // 'Update the Answer Node
                // Select Case cElementType
                // Case "select1"
                // 'Code to update single answer node
                // 'get the correctIndex and find the value
                // oElmt2 = Me.Instance.SelectSingleNode("select1")
                // If oElmt2.GetAttribute("correctIndex") <> "" Then
                // Dim nCorIdx As Integer = oElmt2.GetAttribute("correctIndex")

                // If Not Me.Instance.SelectSingleNode("select1/item[" & nCorIdx & "]") Is Nothing Then
                // oElmt3 = Me.Instance.SelectSingleNode("select1/item[" & nCorIdx & "]")
                // oElmt4 = oElmt3.SelectSingleNode("value")
                // oElmt = addAnswerElmt(moXForm2Edit, newRef, oElmt2.GetAttribute("weighting"), oElmt4.InnerText, True)
                // End If
                // End If

                // Case "select"
                // 'Code to update any answer nodes required
                // 'remove any existing answernodes
                // clearAnswerScores(moXForm2Edit, newRef)
                // 'step through the answers in the select instance to find valid correct ones
                // For Each oElmt2 In Me.Instance.SelectNodes("select/item")
                // If oElmt2.GetAttribute("correct") = "true" Then
                // oElmt3 = oElmt2.SelectSingleNode("value")
                // oElmt = addAnswerElmt(moXForm2Edit, newRef, oElmt2.GetAttribute("weighting"), oElmt3.InnerText)
                // End If
                // Next
                // Case "input"
                // 'Code to update answer node
                // oElmt = addAnswerElmt(moXForm2Edit, newRef, moRequest("cWeighting"), "", True)
                // If moRequest("cAnswers") <> "" Then
                // Dim aCorrect() As String = Split(moRequest("cAnswers"), ",")
                // For i = 0 To UBound(aCorrect)
                // addCorrectAnswer(oElmt, Trim(aCorrect(i)))
                // Next
                // End If
                // Case "textarea"
                // 'Code to update answer node
                // oElmt = addAnswerElmt(moXForm2Edit, newRef, moRequest("cWeighting"), "", True, cElementType)
                // End Select


                // 'remove anything unnessesary before save
                // Select Case cElementType
                // Case "select1", "select"
                // 'remove any empty item nodes
                // For Each oNode In Me.Instance.FirstChild.SelectNodes("item")
                // If oNode.FirstChild Is Nothing Then
                // oNode.ParentNode.RemoveChild(oNode)
                // Else
                // If oNode.FirstChild.InnerText = "" Then
                // oNode.ParentNode.RemoveChild(oNode)
                // End If
                // End If
                // If cElementType = "select" Then
                // oElmt2 = oNode
                // oElmt2.RemoveAttribute("correct")
                // oElmt2.RemoveAttribute("weighting")
                // oElmt2 = Nothing
                // End If
                // Next
                // If cElementType = "select" Then
                // oElmt2 = Me.Instance.FirstChild
                // oElmt2.RemoveAttribute("correctIndex")
                // oElmt2.RemoveAttribute("weighting")
                // oElmt2 = Nothing
                // End If

                // Case "input"
                // 'remove validAnswers Attrib
                // oElmt = Me.Instance.FirstChild
                // oElmt.RemoveAttribute("validAnswers")

                // End Select

                // 'drop the instance back into the full xform
                // If cRef <> "" Then
                // 'replace existing
                // oNode = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']")
                // oNode.ParentNode.ReplaceChild(Me.Instance.FirstChild, oNode)
                // Else
                // 'add new
                // oNode = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cParRef & "' or @bind='" & cParRef & "']")
                // oElmt = Me.Instance.FirstChild()

                // 'everything is bound to a single answer node
                // Select Case cElementType
                // Case "textarea"
                // cBind = "results/answers/answer[@ref='" & newRef & "']/given"
                // Case Else
                // cBind = "results/answers/answer[@ref='" & newRef & "']/@given"
                // End Select
                // oElmt.SetAttribute("bind", newRef)
                // moXForm2Edit.addBind(newRef, cBind)

                // oNode.AppendChild(Me.Instance.FirstChild())
                // End If

                // Else
                // Me.addValues()
                // End If
                // Else
                // Me.addValues()
                // End If

                // Return Me.moXformElmt

                // Catch ex As Exception
                // returnException(myWeb.msException, _moduleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, gbDebug)
                // Return Nothing
                // End Try
                // End Function


                // Public Overridable Function xFrmEditXFormItem(ByVal cRef As String, ByVal nItemIndex As Long) As XmlElement
                // Dim oFrmElmt As XmlElement
                // Dim oSelElmt As XmlElement

                // Dim oElmt As XmlElement = Nothing

                // Dim oNode As XmlNode

                // Dim sQuestionType As String
                // Dim sValue As String

                // Dim nCount As Long

                // Dim cProcessInfo As String = ""

                // Try
                // 'load the xform to be edited

                // If nItemIndex <> 0 Then
                // oElmt = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']/item[" & nItemIndex & "]")
                // End If

                // 'What Q type are we editing...?
                // sQuestionType = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']").Name


                // Me.NewFrm("EditSelect")

                // Me.submission("EditInput", "", "post", "return form_check(this)")
                // oFrmElmt = Me.addGroup(Me.moXformElmt, "EditGroup", "", "Edit Answer")

                // Me.addTextArea(oFrmElmt, "cName", True, "Answer", "xhtml answerEditor")
                // Me.addBind("cName", "item/label", "true()")

                // oSelElmt = Me.addSelect(oFrmElmt, "bCorrect", True, "Correct Answer", "", ApperanceTypes.Full)
                // Me.addOption(oSelElmt, "Correct", "true")
                // Me.addBind("bCorrect", "item/@correct", "false()")

                // Me.addRange(oFrmElmt, "cWeighting", True, "Weighting", "0", "100", "5")
                // Me.addBind("cWeighting", "item/@weighting", "false()")

                // Me.addSubmit(oFrmElmt, "", "Save Answer")

                // ' set the instance from the item loaded from the xform
                // If nItemIndex <> 0 Then
                // Me.Instance.AppendChild(oElmt.CloneNode(True))
                // ' add weighting and correct flag to the item node from answer
                // oElmt = Me.Instance.FirstChild

                // sValue = oElmt.SelectSingleNode("value").InnerText

                // If moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']/score[value/node()='" & sValue & "']") Is Nothing Then
                // oElmt.SetAttribute("correct", "false")
                // oElmt.SetAttribute("weighting", "")
                // Else
                // oElmt.SetAttribute("correct", "true")
                // oElmt.SetAttribute("weighting", moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']/score[value/node()='" & sValue & "']/@weighting").Value)
                // End If

                // Else
                // For Each oNode In moXForm2Edit.moXformElmt.SelectNodes("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']/item")
                // If CInt(oNode.SelectSingleNode("value").InnerText) > nCount Then
                // nCount = CInt(oNode.SelectSingleNode("value").InnerText)
                // End If
                // Next
                // Me.Instance.InnerXml = "<item><label/><value>" & nCount + 1 & "</value></item>"
                // End If

                // If Me.isSubmitted Then
                // Me.updateInstanceFromRequest()
                // Me.validate()
                // If Me.valid Then
                // 'remove the weighting and correct attribs
                // Me.Instance.RemoveAttribute("correct")
                // Me.Instance.RemoveAttribute("weighting")

                // sValue = Me.Instance.SelectSingleNode("item/value").InnerText

                // If nItemIndex <> 0 Then
                // 'drop the instance back into the full xform
                // oNode = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']/item[" & nItemIndex & "]")
                // oNode.ParentNode.ReplaceChild(Me.Instance.FirstChild, oNode)
                // Else
                // 'add new
                // oNode = moXForm2Edit.moXformElmt.SelectSingleNode("group/descendant-or-self::*[@ref='" & cRef & "' or @bind='" & cRef & "']")
                // oNode.AppendChild(Me.Instance.FirstChild)
                // End If
                // oNode = Nothing

                // If moRequest("bCorrect") = "true" Then
                // Select Case sQuestionType
                // Case "select1"
                // oElmt = addAnswerElmt(moXForm2Edit, cRef, moRequest("cWeighting"), sValue, True)
                // Case Else
                // oElmt = addAnswerElmt(moXForm2Edit, cRef, moRequest("cWeighting"), sValue, False)
                // End Select

                // Else
                // 'if we are a select (multi choice) we need to remove the score node if exists.
                // oNode = moXForm2Edit.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']")
                // Select Case sQuestionType
                // Case "select"
                // If Not oNode.SelectSingleNode("score[value/node()='" & sValue & "']") Is Nothing Then
                // 'remove existing node
                // oNode.RemoveChild(oNode.SelectSingleNode("score[value/node()='" & sValue & "']"))

                // End If
                // End Select
                // End If
                // Else
                // Me.addValues()
                // End If
                // Else
                // Me.addValues()
                // End If

                // Return Me.moXformElmt

                // Catch ex As Exception
                // returnException(myWeb.msException, _moduleName, "xFrmEditXFormGroup", ex, "", cProcessInfo, gbDebug)
                // Return Nothing
                // End Try
                // End Function

                // Private Function addAnswerElmt(ByRef oxFrm As Protean.xForm, ByVal cRef As String, ByVal cWeighting As String, Optional ByVal cValue As String = "", Optional ByVal bSingleAnswer As Boolean = False, Optional ByVal cElementType As String = "") As XmlElement

                // Dim oElmt As XmlElement
                // Dim oElmt2 As XmlElement
                // Dim oElmt3 As XmlElement

                // Dim oNode As XmlNode


                // Dim cProcessInfo As String = ""
                // Try
                // If oxFrm.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']") Is Nothing Then

                // oElmt = oxFrm.moPageXML.CreateElement("answer")
                // oElmt.SetAttribute("ref", cRef)
                // Select Case cElementType
                // Case "textarea"
                // addNewTextNode("given", oElmt)
                // Case Else
                // oElmt.SetAttribute("given", "")
                // End Select
                // oElmt.SetAttribute("mark", "")
                // Else
                // oElmt = oxFrm.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']").CloneNode(True)
                // If bSingleAnswer Then
                // 'remove existing score
                // For Each oNode In oElmt.SelectNodes("score")
                // oNode.ParentNode.RemoveChild(oNode)
                // Next
                // End If
                // End If
                // If Not oElmt.SelectSingleNode("score[value/node()='" & cValue & "']") Is Nothing Then
                // 'score exists update the weighting
                // oElmt2 = oElmt.SelectSingleNode("score[value/node()='" & cValue & "']")
                // oElmt2.SetAttribute("weighting", cWeighting)
                // Else
                // oElmt2 = oxFrm.moPageXML.CreateElement("score")
                // oElmt2.SetAttribute("weighting", cWeighting)
                // If cValue <> "" Then
                // oElmt3 = oxFrm.moPageXML.CreateElement("value")
                // oElmt3.InnerText = cValue
                // oElmt2.AppendChild(oElmt3)
                // End If
                // oElmt.AppendChild(oElmt2)
                // End If

                // If oxFrm.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']") Is Nothing Then
                // 'Create a new one
                // oNode = oxFrm.Instance.SelectSingleNode("results/answers")
                // oNode.AppendChild(oElmt)
                // Else
                // 'replace existing
                // oNode = oxFrm.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']")
                // 'replace the whole lot
                // oNode.ParentNode.ReplaceChild(oElmt, oNode)
                // End If

                // Return oElmt

                // Catch ex As Exception
                // returnException(myWeb.msException, _moduleName, "addAnswerNode", ex, "", cProcessInfo, gbDebug)
                // Return Nothing
                // End Try
                // End Function

                // Private Sub clearAnswerScores(ByRef oxFrm As Protean.xForm, ByVal cRef As String)

                // Dim oElmt As XmlElement

                // Dim oNode As XmlNode


                // Dim cProcessInfo As String = ""
                // Try
                // If Not oxFrm.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']") Is Nothing Then
                // oElmt = oxFrm.Instance.SelectSingleNode("results/answers/answer[@ref='" & cRef & "']")
                // For Each oNode In oElmt.SelectNodes("score")
                // oNode.ParentNode.RemoveChild(oNode)
                // Next
                // End If

                // Catch ex As Exception
                // returnException(myWeb.msException, _moduleName, "clearAnswerScores", ex, "", cProcessInfo, gbDebug)
                // End Try
                // End Sub

                // Private Function addCorrectAnswer(ByRef oElmt As XmlElement, ByVal cValue As String) As XmlElement

                // Dim oElmt3 As XmlElement

                // Dim cProcessInfo As String = ""
                // Try

                // oElmt3 = oElmt.OwnerDocument.CreateElement("value")
                // oElmt3.InnerText = cValue
                // oElmt.FirstChild.AppendChild(oElmt3)

                // Return oElmt
                // Catch ex As Exception
                // returnException(myWeb.msException, _moduleName, "addCorrectAnswer", ex, "", cProcessInfo, gbDebug)
                // Return Nothing
                // End Try
                // End Function 

            }
        }
    }
}
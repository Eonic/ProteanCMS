using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;

namespace Protean.Tools
{

    public class Xml
    {
        public static event OnErrorEventHandler OnError;

        public delegate void OnErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);

        private const string mcModuleName = "Protean.Tools.Xml";

        public enum XmlDataType
        {
            TypeString = 1,
            TypeNumber = 2,
            TypeDate = 3
        }

        public enum XmlNodeState
        {
            NotInstantiated = 0,
            IsEmpty = 1,
            HasContents = 2,
            IsEmptyOrHasContents = 3
        }

        private static void CombineInstance_Sub1(XmlElement oMasterElmt, XmlElement oExistingInstance, string cXPath)
        {
            // Looks for stuff with the same xpath
            try
            {
                if (!(cXPath == ""))
                    cXPath = cXPath + "/";
                cXPath += oMasterElmt.Name;

                XmlElement oInstanceElmt = (XmlElement)oExistingInstance.SelectSingleNode(cXPath);
                if (!(oInstanceElmt == null))
                {
                    var loopTo = oMasterElmt.Attributes.Count - 1;
                    // Master Attributes first
                    for (int i = 0; i <= loopTo; i++)
                    {
                        XmlAttribute oMasterAtt = oMasterElmt.Attributes[i];
                        XmlAttribute oInstanceAtt = oInstanceElmt.Attributes[oMasterAtt.Name];
                        if (!(oInstanceAtt == null))
                            oMasterAtt.Value = oInstanceAtt.Value;
                    }

                    var loopTo1 = oInstanceElmt.Attributes.Count - 1;
                    // Now Existing
                    for (int i = 0; i <= loopTo1; i++)
                    {
                        XmlAttribute oInstanceAtt = oInstanceElmt.Attributes[i];
                        XmlAttribute oMasterAtt = oMasterElmt.Attributes[oInstanceAtt.Name];
                        if (oMasterAtt == null)
                            oMasterElmt.SetAttribute(oInstanceAtt.Name, oInstanceAtt.Value);
                    }
                    // Now we need to check child nodes
                    if (oMasterElmt.SelectNodes("*").Count > 0)
                    {
                        foreach (XmlElement oChild in oMasterElmt.SelectNodes("*"))
                            CombineInstance_Sub1(oChild, oExistingInstance, cXPath);
                    }
                    else if (oInstanceElmt.SelectNodes("*").Count > 0)
                    {
                        foreach (XmlElement oChild in oInstanceElmt.SelectNodes("*"))
                            oMasterElmt.AppendChild(oMasterElmt.OwnerDocument.ImportNode(oChild, true));
                        CombineInstance_MarkSubElmts(oInstanceElmt);
                    }
                    else if (!(oInstanceElmt.InnerText == ""))
                        oMasterElmt.InnerText = oInstanceElmt.InnerText;
                    // now mark both elements as found
                    oMasterElmt.SetAttribute("ewCombineInstanceFound", "TRUE");
                    oInstanceElmt.SetAttribute("ewCombineInstanceFound", "TRUE");
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CombineInstance_Sub1", ex, ""));
            }
        }

        private static void CombineInstance_Sub2(XmlElement oExisting, XmlElement oMasterInstance, string cRootName)
        {
            // Gets any unmarked in Existing data and copies over to new
            try
            {
                string cXPath = "";
                CombineInstance_GetXPath(oExisting, ref cXPath, cRootName);
                string[] oParts = Strings.Split(cXPath, "/");
                XmlElement oRefPart = oMasterInstance;
                var loopTo = oParts.Length - 2;
                for (int i = 0; i <= loopTo; i++)
                {
                    if (!(oRefPart.SelectSingleNode(oParts[i]) == null))
                        oRefPart = (XmlElement)oRefPart.SelectSingleNode(oParts[i]);
                    else
                    {
                        XmlElement oElmt = oMasterInstance.OwnerDocument.CreateElement(oParts[i]);
                        oRefPart.AppendChild(oElmt);
                        oRefPart = oElmt;
                    }
                }
                // still need to create last part
                oRefPart.AppendChild(oMasterInstance.OwnerDocument.ImportNode(oExisting, true));
                CombineInstance_MarkSubElmts(oExisting);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CombineInstance_Sub2", ex, ""));
            }
        }

        private static void CombineInstance_GetXPath(XmlElement oElmt, ref string xPath, string cRootName)
        {
            try
            {
                if (xPath == "")
                    xPath = oElmt.Name;
                if (!(oElmt.ParentNode == null))
                {
                    if (!(oElmt.ParentNode.Name == cRootName))
                    {
                        xPath = oElmt.ParentNode.Name + "/" + xPath;
                        CombineInstance_GetXPath((XmlElement)oElmt.ParentNode, ref xPath, cRootName);
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CombineInstance_GetXPath", ex, ""));
            }
        }

        private static void CombineInstance_MarkSubElmts(XmlElement oElmt, bool bRemove = false)
        {
            try
            {
                if (bRemove)
                    oElmt.RemoveAttribute("ewCombineInstanceFound");
                else
                    oElmt.SetAttribute("ewCombineInstanceFound", "TRUE");

                foreach (XmlElement oChild in oElmt.SelectNodes("*"))
                    CombineInstance_MarkSubElmts(oChild, bRemove);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CombineInstance_MarkSubElmts", ex, ""));
            }
        }




        public static XmlNodeList SortedNodeList_UNCONNECTED(XmlElement oRootObject, string cXPath, string cSortItem, XmlSortOrder cSortOrder, System.Xml.XPath.XmlDataType cSortDataType, XmlCaseOrder cCaseOrder)
        {
            XmlElement oElmt = oRootObject.OwnerDocument.CreateElement("List");
            try
            {
                XPathNavigator oNavigator = oRootObject.CreateNavigator();
                XPathExpression oExpression = oNavigator.Compile(cXPath);
                oExpression.AddSort(cSortItem, cSortOrder, cCaseOrder, "", cSortDataType);
                XPathNodeIterator oIterator = oNavigator.Select(oExpression);

                while (oIterator.MoveNext())
                    oElmt.InnerXml += oIterator.Current.OuterXml;

                return oElmt.ChildNodes;
            }
            catch (Exception ex)
            {
                return oElmt.ChildNodes;
            }
        }

        public static string XmlToForm(string sString)
        {
            string sProcessInfo = "";
            try
            {
                sString = Strings.Replace(sString, "<", "&lt;");
                sString = Strings.Replace(sString, ">", "&gt;");
                return sString + "";
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "XmlToForm", ex, ""));
                return "";
            }
        }

        public static XmlDocument htmlToXmlDoc(string shtml)
        {
            XmlDocument oXmlDoc = new XmlDocument();
            string sProcessInfo = "";

            try
            {
                shtml = Strings.Replace(shtml, "<!DOCTYPE html public shared \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">", "");
                shtml = Strings.Replace(shtml, " Xmlns = \"http://www.w3.org/1999/xhtml\"", "");
                shtml = Strings.Replace(shtml, " Xmlns=\"http://www.w3.org/1999/xhtml\"", "");
                shtml = Strings.Replace(shtml, " Xml:lang=\"\"", "");
                oXmlDoc.LoadXml(shtml);

                return oXmlDoc;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "htmlToXmlDoc", ex, ""));
                return null;
            }
        }

        public static XmlNamespaceManager getNsMgr(ref XmlNode oNode, ref XmlDocument oXml)
        {
            try
            {
                XmlElement oElmt;
                string cNsUri = "";
                XmlNamespaceManager nsMgr = new XmlNamespaceManager(oXml.NameTable);

                // does the instance have a namespace, if so give it a standard prefix of EWS
                oElmt = (XmlElement)oNode;

                if (!(oElmt == null))
                {
                    cNsUri = oElmt.GetAttribute("Xmlns");
                    if (cNsUri == "")
                        cNsUri = oElmt.GetAttribute("xmlns");
                }

                if (cNsUri != "")
                    nsMgr.AddNamespace("ews", cNsUri);

                return nsMgr;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getNsMgr", ex, ""));
                return null;
            }
        }


        public static XmlNamespaceManager getNsMgrWithURI(ref XmlNode oNode, ref XmlDocument oXml)
        {
            XmlElement oElmt;
            string cNsUri;
            try
            {
                XmlNamespaceManager nsMgr = new XmlNamespaceManager(oXml.NameTable);
                // does the instance have a namespace, if so give it a standard prefix of EWS
                oElmt = (XmlElement)oNode;
                cNsUri = oElmt.GetAttribute("xmlns");

                if (cNsUri != "")
                    nsMgr.AddNamespace("ews", cNsUri);
                else if (oElmt.NamespaceURI != "")
                    nsMgr.AddNamespace("ews", oElmt.NamespaceURI);

                return nsMgr;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getNsMgrWithURI", ex, ""));
                return null;
            }
        }

        public static XmlNamespaceManager getNsMgrRecursive(ref XmlNode oNode, ref XmlDocument oXml)
        {
            try
            {
               
                string cNsUri = "";
                XmlNamespaceManager nsMgr = new XmlNamespaceManager(oXml.NameTable);
                string cPrefix = "ews";

                // does the instance have a namespace? if so give it a standard prefix of EWS
                // If a subnode of the instance has a namespace we give it a prefix of ews2
                // allows us to specifiy ews2 in bind paths.

                foreach (XmlElement oElmt in oNode.SelectNodes("descendant-or-self::*"))
                {
                    if (!(oElmt == null))
                    {
                        cNsUri = oElmt.GetAttribute("Xmlns");
                        if (cNsUri == "")
                            cNsUri = oElmt.GetAttribute("xmlns");
                    }
                    if (cNsUri != "")
                        nsMgr.AddNamespace(cPrefix, cNsUri);
                    if (cPrefix == "ews")
                        cPrefix = "ews2";
                }

                return nsMgr;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getNsMgr", ex, ""));
                return null;
            }
        }


        public static string addNsToXpath(string cXpath, ref XmlNamespaceManager nsMgr)
        {
            try
            {
                // check namespace not allready specified
                if (Strings.InStr(cXpath, "ews:") == 0)
                {
                    if (nsMgr.HasNamespace("ews"))
                    {
                        cXpath = "ews:" + Strings.Replace(cXpath, "/", "/ews:");

                        // find any [ not followed by number
                        string[] substrings = Regex.Split(cXpath, @"(\[(?!\d))");
                        if (substrings.Length > 1)
                        {
                            string cNewXpath = "";
                            foreach (string match in substrings)
                            {
                                if (match.StartsWith("["))
                                    cNewXpath = cNewXpath + "[ews:" + match.Substring(1);
                                else
                                    cNewXpath = cNewXpath + match;
                            }
                            cXpath = cNewXpath;
                        }

                        // undo any attributes
                        cXpath = Strings.Replace(cXpath, "/ews:@", "/@");
                        cXpath = Strings.Replace(cXpath, "[ews:@", "[@");
                        cXpath = Strings.Replace(cXpath, "[ews:position()", "[position()");
                    }
                }

                return cXpath;
            }
            catch (Exception ex)
            {
                OnError?.Invoke( null, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "addNsToXpath", ex, ""));
                return "";
            }
        }

        public static XmlElement addElement(ref XmlElement oParent, string cNewNodeName, string cNewNodeValue = "", bool bAddAsXml = false, bool bPrepend = false, XmlNode oNodeFromXPath = null, string sPrefix = null, string NamespaceURI = "")
        {
            try
            {
                XmlElement oNewNode;

                if (sPrefix == null)
                    oNewNode = oParent.OwnerDocument.CreateElement(cNewNodeName);
                else
                    oNewNode = oParent.OwnerDocument.CreateElement(sPrefix, cNewNodeName, NamespaceURI);

                if (cNewNodeValue == "" & !(oNodeFromXPath == null))
                {
                    if (bAddAsXml)
                        cNewNodeValue = oNodeFromXPath.InnerXml;
                    else
                        cNewNodeValue = oNodeFromXPath.InnerText;
                }
                if (cNewNodeValue != "")
                {
                    if (bAddAsXml)
                        oNewNode.InnerXml = cNewNodeValue;
                    else
                        oNewNode.InnerText = cNewNodeValue;
                }
                if (bPrepend)
                    oParent.PrependChild(oNewNode);
                else
                    oParent.AppendChild(oNewNode);
                return oNewNode;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "addElement", ex, ""));
                return null;
            }
        }

        public static XmlElement firstElement(ref XmlElement oElement)
        {
   
            XmlElement oReturnNode = null;
            try
            {
                foreach (XmlNode oNode in oElement.ChildNodes)
                {
                    if (oNode.NodeType == XmlNodeType.Element)
                    {
                        oReturnNode = (XmlElement)oNode;
                        break;
                    }
                }
                return oReturnNode;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "firstElement", ex, ""));
                return null;
            }
        }

        public static void renameNode(ref XmlElement oNode, string cNewNodeName)
        {
            try
            {
                XmlElement oNewNode;

                oNewNode = oNode.OwnerDocument.CreateElement(cNewNodeName);

                foreach (XmlElement oChildElmt in oNode.SelectNodes("*"))
                    oNewNode.AppendChild(oChildElmt);
                oNode.ParentNode.ReplaceChild(oNewNode, oNode);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "renameNode", ex, ""));
            }
        }

        public static void TidyHtmltoCData(ref XmlElement oElmt)
        {
            try
            {
                var htmlNodeList = oElmt.SelectNodes("descendant-or-self::*[p | div | ul | span | b | strong | h | section | a]");
                foreach (XmlNode htmlItem in htmlNodeList)
                {
                    if (htmlItem.ParentNode != null)
                    {
                        htmlItem.InnerXml = "<![CDATA[" + htmlItem.InnerXml + "]]>";
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "removeChildByName", ex, ""));
            }
        }

        public static void removeChildByName(ref XmlElement oNode, string cRemoveNodeName)
        {
            try
            {
                XmlElement removeElmt = (XmlElement)oNode.SelectSingleNode(cRemoveNodeName);
                if (removeElmt != null)
                    oNode.RemoveChild(removeElmt);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "removeChildByName", ex, ""));
            }
        }

        public static XmlElement addNewTextNode(string cNodeName, ref XmlNode oNode, string cNodeValue = "", bool bAppendNotPrepend = true, bool bOverwriteExistingNode = false)
        {
            try
            {
                XmlElement oElem;

                if (bOverwriteExistingNode)
                {
                    // Search for an existing node - delete it if it exists
                    oElem = (XmlElement)oNode.SelectSingleNode(cNodeName);
                    if (!(oElem == null))
                        oNode.RemoveChild(oElem);
                }

                oElem = oNode.OwnerDocument.CreateElement(cNodeName);
                if (cNodeValue != "")
                    oElem.InnerText = cNodeValue;
                if (bAppendNotPrepend)
                    oNode.AppendChild(oElem);
                else
                    oNode.PrependChild(oElem);

                return oElem;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "addNewTextNode", ex, ""));
                return null;
            }
        }

        // Public Shared Function getNodeValueByType(ByVal oParent As XmlNode, ByVal cXPath As String, Optional ByVal nNodeType As XmlDataType = XmlDataType.TypeString, Optional ByVal vDefaultValue As Object = "") As Object
        // Try
        // Dim oNode As XmlNode
        // Dim cValue As Object
        // oNode = oParent.SelectSingleNode(cXPath)
        // If oNode Is Nothing Then
        // ' No xPath match, let's return a default value
        // ' If there are no default values, then let's return a nominal default value
        // Select Case nNodeType
        // Case XmlDataType.TypeNumber
        // cValue = IIf(IsNumeric(vDefaultValue), vDefaultValue, 0)
        // Case XmlDataType.TypeDate
        // cValue = IIf(IsDate(vDefaultValue), vDefaultValue, Now())
        // Case Else ' Assume default type is string
        // cValue = IIf(vDefaultValue <> "", vDefaultValue.ToString, "")
        // End Select
        // Else
        // ' XPath  match, let's check it against type
        // cValue = oNode.InnerText
        // Select Case nNodeType
        // Case XmlDataType.TypeNumber
        // cValue = IIf(IsNumeric(cValue), cValue, IIf(IsNumeric(vDefaultValue), vDefaultValue, 0))
        // Case XmlDataType.TypeDate
        // cValue = IIf(IsDate(cValue), cValue, IIf(IsDate(vDefaultValue), vDefaultValue, Now()))
        // Case Else ' Assume default type is string
        // cValue = IIf(cValue <> "", cValue.ToString, IIf(vDefaultValue <> "", vDefaultValue.ToString, ""))
        // End Select
        // End If

        // Return cValue
        // Catch ex As Exception
        // RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getNodeValueByType", ex, ""))
        // Return Nothing
        // End Try

        // End Function

        public static object getNodeValueByType(ref XmlNode oParent, string cXPath, XmlDataType nNodeType = XmlDataType.TypeString, object vDefaultValue = null, XmlNamespaceManager nsMgr = null)
        {
            XmlNode oNode;
            object cValue;
            if (nsMgr == null)
                oNode = oParent.SelectSingleNode(cXPath);
            else
                oNode = oParent.SelectSingleNode(addNsToXpath(cXPath, ref nsMgr), nsMgr);
            if (oNode == null)
            {
                // No xPath match, let's return a default value
                // If there are no default values, then let's return a nominal default value
                switch (nNodeType)
                {
                    case XmlDataType.TypeNumber:
                        {
                            cValue = Information.IsNumeric(vDefaultValue)? vDefaultValue: 0;
                            break;
                        }

                    case XmlDataType.TypeDate:
                        {
                            cValue = Information.IsDate(vDefaultValue)? vDefaultValue: DateTime.Now;
                            break;
                        }

                    default:
                        {
                            cValue = (string)vDefaultValue != "" ? vDefaultValue.ToString(): "";
                            break;
                        }
                }
            }
            else
            {
                // XPath  match, let's check it against type
                cValue = oNode.InnerText;
                switch (nNodeType)
                {
                    case XmlDataType.TypeNumber:
                        {
                            cValue = Information.IsNumeric(cValue)? cValue: Information.IsNumeric(vDefaultValue)? vDefaultValue: 0;
                            break;
                        }

                    case XmlDataType.TypeDate:
                        {
                            cValue = Information.IsDate(cValue)? cValue: Information.IsDate(vDefaultValue)? vDefaultValue: DateTime.Now;
                            break;
                        }

                    default:
                        {
                            cValue = (string)cValue != ""? cValue.ToString(): (string)vDefaultValue != ""? vDefaultValue.ToString(): "";
                            break;
                        }
                }
            }

            return cValue;
        }

        /// <summary>
        /// <para>This allows XML to be loaded in to an element from a given file</para>
        /// <para>The following checks are made:</para>
        /// <list>
        /// <item><term>File Exists?</term><description>Does the file provided by cFilePath actually exist</description></item>
        /// <item><term>Valid XML</term><description>Is the loaded XMl actually valid?</description></item>
        /// </list>
        /// <para>If either check fails, then either Nothing or an empty element with a node name determined by <c>cErrorNodeName</c>.</para>
        /// </summary>
        /// <param name="cFilePath">The file path of the file that is being loaded</param>
        /// <param name="oXmlDocument">Optional - the xml document to generate the return element from</param>
        /// <param name="cErrorNodeName">Optional - if the checks fail, then this param determines the name of an empty returned element.</param>
        /// <returns>The loaded XML's DocumentElement, or Nothing/empty element</returns>
        /// <remarks></remarks>
        public static XmlElement loadElement(string cFilePath, XmlDocument oXmlDocument = null, string cErrorNodeName = "")
        {
            try
            {
                XmlDocument oXmlLoadDocument = new XmlDocument();
                XmlElement oReturnElement = null;
                bool bSuccess = false;

                if (oXmlDocument == null)
                    oXmlDocument = new XmlDocument();

                // Try to find and load the element
                if (System.IO.File.Exists(cFilePath))
                {
                    try
                    {
                        oXmlLoadDocument.Load(cFilePath);
                        oReturnElement = oXmlLoadDocument.DocumentElement;
                        bSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        bSuccess = false;
                    }
                }

                // Did the checks fail?
                if (!(bSuccess))
                {
                    // return Nothing or empty element
                    if (!(string.IsNullOrEmpty(cErrorNodeName)))
                        oReturnElement = oXmlDocument.CreateElement(cErrorNodeName);
                }
                else
                    oReturnElement = (XmlElement)oXmlDocument.ImportNode(oReturnElement, true);

                return oReturnElement;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "loadElement", ex, ""));
                return null;
            }
        }

        /// <summary>
        /// NodeState - Given a node, looks to see if it has been instantiated and has contents
        /// </summary>
        /// <param name="oNode">The root node for testing</param>
        /// <param name="xPath">The xpath to apply to the root node.  If this is not supplied, the root node is tested.</param>
        /// <param name="populateAsText">Text that can be added to the test node.</param>
        /// <param name="populateAsXml">Xml that can be added to the test node</param>
        /// <param name="populateState">The state that the test node must be in before it can be populated</param>
        /// <param name="returnElement">The test node</param>
        /// <param name="returnAsXml">The test node's inner xml</param>
        /// <param name="returnAsText">The test node's inner text</param>
        /// <param name="bCheckTrimmedInnerText">The matched node will have its innertext trimmed and examined</param>
        /// <returns>XmlNodeState - the state of the test node.</returns>
        /// <remarks></remarks>
        public static Xml.XmlNodeState NodeState(ref XmlElement oNode, string xPath = "", string populateAsText = "", string populateAsXml = "", Xml.XmlNodeState populateState = XmlNodeState.IsEmpty, XmlElement returnElement = null, string returnAsXml = "", string returnAsText = "", bool bCheckTrimmedInnerText = false)
        {
            try
            {
                Xml.XmlNodeState oReturnState = XmlNodeState.NotInstantiated;

                // Find the xPath if appropriate
                if (!(string.IsNullOrEmpty(xPath)) & !(oNode == null))
                    returnElement = (XmlElement)oNode.SelectSingleNode(xPath);
                else
                    returnElement = oNode;

                if (!(returnElement == null))
                {
                    // Determine the node status
                    if (returnElement.IsEmpty | (bCheckTrimmedInnerText & returnElement.InnerText.Trim() == ""))
                        oReturnState = XmlNodeState.IsEmpty;
                    else
                        oReturnState = XmlNodeState.HasContents;

                    // Populate if necessary
                    if ((populateAsText != "" | populateAsXml != "") & ((populateState == oReturnState) | (populateState == XmlNodeState.IsEmptyOrHasContents)))
                    {
                        if (populateAsText != "")
                            returnElement.InnerText = populateAsText;
                        else
                            try
                            {
                                returnElement.InnerXml = populateAsXml;
                            }
                            catch (Exception ex)
                            {
                            }
                    }

                    // Return the optional params
                    returnAsXml = returnElement.InnerXml;
                    returnAsText = returnElement.InnerText;
                }

                return oReturnState;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "NodeState", ex, ""));
                return default(XmlNodeState);
            }
        }

        // RJP 7 Nove 2012. Added email address checker.
        public static bool EmailAddressCheck(string emailAddress)
        {
            string pattern = @"^[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$";
            Match emailAddressMatch = Regex.Match(emailAddress, pattern);
            if (emailAddressMatch.Success)
                return true;
            else
                return false;
        }

        // RJP 7 Nov 2012. Moved from Setup.vb
        public static string EncodeForXml(string data)
        {

            Regex badAmpersand = new Regex("&(?![a-zA-Z]{2,6};|#[0-9]{2,4};)");
            data = badAmpersand.Replace(data, "&amp;");
            return data.Replace("<", "&lt;").Replace("\"", "&quot;").Replace(">", "gt;");
        }

        public static string convertEntitiesToCodes(string sString)
        {
            try
            {
                string startString = sString;
                if (sString == null)
                    return "";
                else
                {
                    sString = Strings.Replace(sString, "Xmlns=\"\"", "");

                    // sString = Strings.Replace(sString, "& ", "&amp; ")

                    // strip out any empty tags left by tinyMCE as the complier converts the to <h1/>
                    sString = Strings.Replace(sString, "<h1></h1>", "");
                    sString = Strings.Replace(sString, "<h2></h2>", "");
                    sString = Strings.Replace(sString, "<h3></h3>", "");
                    sString = Strings.Replace(sString, "<h4></h4>", "");
                    sString = Strings.Replace(sString, "<h5></h5>", "");
                    sString = Strings.Replace(sString, "<h6></h6>", "");
                    sString = Strings.Replace(sString, "<span></span>", "");

                    // re.Replace(sString, "&^(;)" "&amp;")

                    // xhtml Tidy
                    sString = Strings.Replace(sString, "<o:p>", "<p>");
                    sString = Strings.Replace(sString, "</o:p>", "</p>");
                    sString = Strings.Replace(sString, "<o:p/>", "<p/>");

                    // sString = Strings.Replace(sString, "&lt;", "&#60;")
                    // sString = Strings.Replace(sString, "&gt;", "&#92;")

                    sString = Strings.Replace(sString, "&quot;", "&#34;");
                    sString = Strings.Replace(sString, "&apos;", "&#39;");
                    sString = Strings.Replace(sString, "&nbsp;", "&#160;");
                    sString = Strings.Replace(sString, "&iexcl;", "&#161;");
                    sString = Strings.Replace(sString, "&cent;", "&#162;");
                    sString = Strings.Replace(sString, "&pound;", "&#163;");
                    sString = Strings.Replace(sString, "&curren;", "&#164;");
                    sString = Strings.Replace(sString, "&yen;", "&#165;");
                    sString = Strings.Replace(sString, "&brvbar;", "&#166;");
                    sString = Strings.Replace(sString, "&sect;", "&#167;");
                    sString = Strings.Replace(sString, "&uml;", "&#168;");
                    sString = Strings.Replace(sString, "&copy;", "&#169;");
                    sString = Strings.Replace(sString, "&ordf;", "&#170;");
                    sString = Strings.Replace(sString, "&laquo;", "&#171;");
                    sString = Strings.Replace(sString, "&not;", "&#172;");
                    sString = Strings.Replace(sString, "&shy;", "&#173;");
                    sString = Strings.Replace(sString, "&reg;", "&#174;");
                    sString = Strings.Replace(sString, "&macr;", "&#175;");
                    sString = Strings.Replace(sString, "&deg;", "&#176;");
                    sString = Strings.Replace(sString, "&plusmn;", "&#177;");
                    sString = Strings.Replace(sString, "&sup2;", "&#178;");
                    sString = Strings.Replace(sString, "&sup3;", "&#179;");
                    sString = Strings.Replace(sString, "&acute;", "&#180;");
                    sString = Strings.Replace(sString, "&micro;", "&#181;");
                    sString = Strings.Replace(sString, "&para;", "&#182;");
                    sString = Strings.Replace(sString, "&middot;", "&#183;");
                    sString = Strings.Replace(sString, "&cedil;", "&#184;");
                    sString = Strings.Replace(sString, "&sup1;", "&#185;");
                    sString = Strings.Replace(sString, "&ordm;", "&#186;");
                    sString = Strings.Replace(sString, "&raquo;", "&#187;");
                    sString = Strings.Replace(sString, "&frac14;", "&#188;");
                    sString = Strings.Replace(sString, "&frac12;", "&#189;");
                    sString = Strings.Replace(sString, "&frac34;", "&#190;");
                    sString = Strings.Replace(sString, "&iquest;", "&#191;");
                    sString = Strings.Replace(sString, "&Agrave;", "&#192;");
                    sString = Strings.Replace(sString, "&Aacute;", "&#193;");
                    sString = Strings.Replace(sString, "&Acirc;", "&#194;");
                    sString = Strings.Replace(sString, "&Atilde;", "&#195;");
                    sString = Strings.Replace(sString, "&Auml;", "&#196;");
                    sString = Strings.Replace(sString, "&Aring;", "&#197;");
                    sString = Strings.Replace(sString, "&AElig;", "&#198;");
                    sString = Strings.Replace(sString, "&Ccedil;", "&#199;");
                    sString = Strings.Replace(sString, "&Egrave;", "&#200;");
                    sString = Strings.Replace(sString, "&Eacute;", "&#201;");
                    sString = Strings.Replace(sString, "&Ecirc;", "&#202;");
                    sString = Strings.Replace(sString, "&Euml;", "&#203;");
                    sString = Strings.Replace(sString, "&Igrave;", "&#204;");
                    sString = Strings.Replace(sString, "&Iacute;", "&#205;");
                    sString = Strings.Replace(sString, "&Icirc;", "&#206;");
                    sString = Strings.Replace(sString, "&Iuml;", "&#207;");
                    sString = Strings.Replace(sString, "&ETH;", "&#208;");
                    sString = Strings.Replace(sString, "&Ntilde;", "&#209;");
                    sString = Strings.Replace(sString, "&Ograve;", "&#210;");
                    sString = Strings.Replace(sString, "&Oacute;", "&#211;");
                    sString = Strings.Replace(sString, "&Ocirc;", "&#212;");
                    sString = Strings.Replace(sString, "&Otilde;", "&#213;");
                    sString = Strings.Replace(sString, "&Ouml;", "&#214;");
                    sString = Strings.Replace(sString, "&times;", "&#215;");
                    sString = Strings.Replace(sString, "&Oslash;", "&#216;");
                    sString = Strings.Replace(sString, "&Ugrave;", "&#217;");
                    sString = Strings.Replace(sString, "&Uacute;", "&#218;");
                    sString = Strings.Replace(sString, "&Ucirc;", "&#219;");
                    sString = Strings.Replace(sString, "&Uuml;", "&#220;");
                    sString = Strings.Replace(sString, "&Yacute;", "&#221;");
                    sString = Strings.Replace(sString, "&THORN;", "&#222;");
                    sString = Strings.Replace(sString, "&szlig;", "&#223;");
                    sString = Strings.Replace(sString, "&agrave;", "&#224;");
                    sString = Strings.Replace(sString, "&aacute;", "&#225;");
                    sString = Strings.Replace(sString, "&acirc;", "&#226;");
                    sString = Strings.Replace(sString, "&atilde;", "&#227;");
                    sString = Strings.Replace(sString, "&auml;", "&#228;");
                    sString = Strings.Replace(sString, "&aring;", "&#229;");
                    sString = Strings.Replace(sString, "&aelig;", "&#230;");
                    sString = Strings.Replace(sString, "&ccedil;", "&#231;");
                    sString = Strings.Replace(sString, "&egrave;", "&#232;");
                    sString = Strings.Replace(sString, "&eacute;", "&#233;");
                    sString = Strings.Replace(sString, "&ecirc;", "&#234;");
                    sString = Strings.Replace(sString, "&euml;", "&#235;");
                    sString = Strings.Replace(sString, "&igrave;", "&#236;");
                    sString = Strings.Replace(sString, "&iacute;", "&#237;");
                    sString = Strings.Replace(sString, "&icirc;", "&#238;");
                    sString = Strings.Replace(sString, "&iuml;", "&#239;");
                    sString = Strings.Replace(sString, "&eth;", "&#240;");
                    sString = Strings.Replace(sString, "&ntilde;", "&#241;");
                    sString = Strings.Replace(sString, "&ograve;", "&#242;");
                    sString = Strings.Replace(sString, "&oacute;", "&#243;");
                    sString = Strings.Replace(sString, "&ocirc;", "&#244;");
                    sString = Strings.Replace(sString, "&otilde;", "&#245;");
                    sString = Strings.Replace(sString, "&ouml;", "&#246;");
                    sString = Strings.Replace(sString, "&divide;", "&#247;");
                    sString = Strings.Replace(sString, "&oslash;", "&#248;");
                    sString = Strings.Replace(sString, "&ugrave;", "&#249;");
                    sString = Strings.Replace(sString, "&uacute;", "&#250;");
                    sString = Strings.Replace(sString, "&ucirc;", "&#251;");
                    sString = Strings.Replace(sString, "&uuml;", "&#252;");
                    sString = Strings.Replace(sString, "&yacute;", "&#253;");
                    sString = Strings.Replace(sString, "&thorn;", "&#254;");
                    sString = Strings.Replace(sString, "&yuml;", "&#255;");
                    sString = Strings.Replace(sString, "&OElig;", "&#338;");
                    sString = Strings.Replace(sString, "&oelig;", "&#339;");
                    sString = Strings.Replace(sString, "&Scaron;", "&#352;");
                    sString = Strings.Replace(sString, "&scaron;", "&#353;");
                    sString = Strings.Replace(sString, "&Yuml;", "&#376;");
                    sString = Strings.Replace(sString, "&fnof;", "&#402;");
                    sString = Strings.Replace(sString, "&circ;", "&#710;");
                    sString = Strings.Replace(sString, "&tilde;", "&#732;");
                    sString = Strings.Replace(sString, "&Alpha;", "&#913;");
                    sString = Strings.Replace(sString, "&Beta;", "&#914;");
                    sString = Strings.Replace(sString, "&Gamma;", "&#915;");
                    sString = Strings.Replace(sString, "&Delta;", "&#916;");
                    sString = Strings.Replace(sString, "&Epsilon;", "&#917;");
                    sString = Strings.Replace(sString, "&Zeta;", "&#918;");
                    sString = Strings.Replace(sString, "&Eta;", "&#919;");
                    sString = Strings.Replace(sString, "&Theta;", "&#920;");
                    sString = Strings.Replace(sString, "&Iota;", "&#921;");
                    sString = Strings.Replace(sString, "&Kappa;", "&#922;");
                    sString = Strings.Replace(sString, "&Lambda;", "&#923;");
                    sString = Strings.Replace(sString, "&Mu;", "&#924;");
                    sString = Strings.Replace(sString, "&Nu;", "&#925;");
                    sString = Strings.Replace(sString, "&Xi;", "&#926;");
                    sString = Strings.Replace(sString, "&Omicron;", "&#927;");
                    sString = Strings.Replace(sString, "&Pi;", "&#928;");
                    sString = Strings.Replace(sString, "&Rho;", "&#929;");
                    sString = Strings.Replace(sString, "&Sigma;", "&#931;");
                    sString = Strings.Replace(sString, "&Tau;", "&#932;");
                    sString = Strings.Replace(sString, "&Upsilon;", "&#933;");
                    sString = Strings.Replace(sString, "&Phi;", "&#934;");
                    sString = Strings.Replace(sString, "&Chi;", "&#935;");
                    sString = Strings.Replace(sString, "&Psi;", "&#936;");
                    sString = Strings.Replace(sString, "&Omega;", "&#937;");
                    sString = Strings.Replace(sString, "&alpha;", "&#945;");
                    sString = Strings.Replace(sString, "&beta;", "&#946;");
                    sString = Strings.Replace(sString, "&gamma;", "&#947;");
                    sString = Strings.Replace(sString, "&delta;", "&#948;");
                    sString = Strings.Replace(sString, "&epsilon;", "&#949;");
                    sString = Strings.Replace(sString, "&zeta;", "&#950;");
                    sString = Strings.Replace(sString, "&eta;", "&#951;");
                    sString = Strings.Replace(sString, "&theta;", "&#952;");
                    sString = Strings.Replace(sString, "&iota;", "&#953;");
                    sString = Strings.Replace(sString, "&kappa;", "&#954;");
                    sString = Strings.Replace(sString, "&lambda;", "&#955;");
                    sString = Strings.Replace(sString, "&mu;", "&#956;");
                    sString = Strings.Replace(sString, "&nu;", "&#957;");
                    sString = Strings.Replace(sString, "&xi;", "&#958;");
                    sString = Strings.Replace(sString, "&omicron;", "&#959;");
                    sString = Strings.Replace(sString, "&pi;", "&#960;");
                    sString = Strings.Replace(sString, "&rho;", "&#961;");
                    sString = Strings.Replace(sString, "&sigmaf;", "&#962;");
                    sString = Strings.Replace(sString, "&sigma;", "&#963;");
                    sString = Strings.Replace(sString, "&tau;", "&#964;");
                    sString = Strings.Replace(sString, "&upsilon;", "&#965;");
                    sString = Strings.Replace(sString, "&phi;", "&#966;");
                    sString = Strings.Replace(sString, "&chi;", "&#967;");
                    sString = Strings.Replace(sString, "&psi;", "&#968;");
                    sString = Strings.Replace(sString, "&omega;", "&#969;");
                    sString = Strings.Replace(sString, "&thetasym;", "&#977;");
                    sString = Strings.Replace(sString, "&upsih;", "&#978;");
                    sString = Strings.Replace(sString, "&piv;", "&#982;");
                    sString = Strings.Replace(sString, "&ensp;", "&#8194;");
                    sString = Strings.Replace(sString, "&emsp;", "&#8195;");
                    sString = Strings.Replace(sString, "&thinsp;", "&#8201;");
                    sString = Strings.Replace(sString, "&zwnj;", "&#8204;");
                    sString = Strings.Replace(sString, "&zwj;", "&#8205;");
                    sString = Strings.Replace(sString, "&lrm;", "&#8206;");
                    sString = Strings.Replace(sString, "&rlm;", "&#8207;");
                    sString = Strings.Replace(sString, "&ndash;", "&#8211;");
                    sString = Strings.Replace(sString, "&mdash;", "&#8212;");
                    sString = Strings.Replace(sString, "&lsquo;", "&#8216;");
                    sString = Strings.Replace(sString, "&rsquo;", "&#8217;");
                    sString = Strings.Replace(sString, "&sbquo;", "&#8218;");
                    sString = Strings.Replace(sString, "&ldquo;", "&#8220;");
                    sString = Strings.Replace(sString, "&rdquo;", "&#8221;");
                    sString = Strings.Replace(sString, "&bdquo;", "&#8222;");
                    sString = Strings.Replace(sString, "&dagger;", "&#8224;");
                    sString = Strings.Replace(sString, "&Dagger;", "&#8225;");
                    sString = Strings.Replace(sString, "&bull;", "&#8226;");
                    sString = Strings.Replace(sString, "&hellip;", "&#8230;");
                    sString = Strings.Replace(sString, "&permil;", "&#8240;");
                    sString = Strings.Replace(sString, "&prime;", "&#8242;");
                    sString = Strings.Replace(sString, "&Prime;", "&#8243;");
                    sString = Strings.Replace(sString, "&lsaquo;", "&#8249;");
                    sString = Strings.Replace(sString, "&rsaquo;", "&#8250;");
                    sString = Strings.Replace(sString, "&oline;", "&#8254;");
                    sString = Strings.Replace(sString, "&frasl;", "&#8260;");
                    sString = Strings.Replace(sString, "&euro;", "&#8364;");
                    sString = Strings.Replace(sString, "&image;", "&#8465;");
                    sString = Strings.Replace(sString, "&weierp;", "&#8472;");
                    sString = Strings.Replace(sString, "&real;", "&#8476;");
                    sString = Strings.Replace(sString, "&trade;", "&#8482;");
                    sString = Strings.Replace(sString, "&alefsym;", "&#8501;");
                    sString = Strings.Replace(sString, "&larr;", "&#8592;");
                    sString = Strings.Replace(sString, "&uarr;", "&#8593;");
                    sString = Strings.Replace(sString, "&rarr;", "&#8594;");
                    sString = Strings.Replace(sString, "&darr;", "&#8595;");
                    sString = Strings.Replace(sString, "&harr;", "&#8596;");
                    sString = Strings.Replace(sString, "&crarr;", "&#8629;");
                    sString = Strings.Replace(sString, "&lArr;", "&#8656;");
                    sString = Strings.Replace(sString, "&uArr;", "&#8657;");
                    sString = Strings.Replace(sString, "&rArr;", "&#8658;");
                    sString = Strings.Replace(sString, "&dArr;", "&#8659;");
                    sString = Strings.Replace(sString, "&hArr;", "&#8660;");
                    sString = Strings.Replace(sString, "&forall;", "&#8704;");
                    sString = Strings.Replace(sString, "&part;", "&#8706;");
                    sString = Strings.Replace(sString, "&exist;", "&#8707;");
                    sString = Strings.Replace(sString, "&empty;", "&#8709;");
                    sString = Strings.Replace(sString, "&nabla;", "&#8711;");
                    sString = Strings.Replace(sString, "&isin;", "&#8712;");
                    sString = Strings.Replace(sString, "&notin;", "&#8713;");
                    sString = Strings.Replace(sString, "&ni;", "&#8715;");
                    sString = Strings.Replace(sString, "&prod;", "&#8719;");
                    sString = Strings.Replace(sString, "&sum;", "&#8721;");
                    sString = Strings.Replace(sString, "&minus;", "&#8722;");
                    sString = Strings.Replace(sString, "&lowast;", "&#8727;");
                    sString = Strings.Replace(sString, "&radic;", "&#8730;");
                    sString = Strings.Replace(sString, "&prop;", "&#8733;");
                    sString = Strings.Replace(sString, "&infin;", "&#8734;");
                    sString = Strings.Replace(sString, "&ang;", "&#8736;");
                    sString = Strings.Replace(sString, "&and;", "&#8743;");
                    sString = Strings.Replace(sString, "&or;", "&#8744;");
                    sString = Strings.Replace(sString, "&cap;", "&#8745;");
                    sString = Strings.Replace(sString, "&cup;", "&#8746;");
                    sString = Strings.Replace(sString, "&int;", "&#8747;");
                    sString = Strings.Replace(sString, "&there4;", "&#8756;");
                    sString = Strings.Replace(sString, "&sim;", "&#8764;");
                    sString = Strings.Replace(sString, "&cong;", "&#8773;");
                    sString = Strings.Replace(sString, "&asymp;", "&#8776;");
                    sString = Strings.Replace(sString, "&ne;", "&#8800;");
                    sString = Strings.Replace(sString, "&equiv;", "&#8801;");
                    sString = Strings.Replace(sString, "&le;", "&#8804;");
                    sString = Strings.Replace(sString, "&ge;", "&#8805;");
                    sString = Strings.Replace(sString, "&sub;", "&#8834;");
                    sString = Strings.Replace(sString, "&sup;", "&#8835;");
                    sString = Strings.Replace(sString, "&nsub;", "&#8836;");
                    sString = Strings.Replace(sString, "&sube;", "&#8838;");
                    sString = Strings.Replace(sString, "&supe;", "&#8839;");
                    sString = Strings.Replace(sString, "&oplus;", "&#8853;");
                    sString = Strings.Replace(sString, "&otimes;", "&#8855;");
                    sString = Strings.Replace(sString, "&perp;", "&#8869;");
                    sString = Strings.Replace(sString, "&sdot;", "&#8901;");
                    sString = Strings.Replace(sString, "&lceil;", "&#8968;");
                    sString = Strings.Replace(sString, "&rceil;", "&#8969;");
                    sString = Strings.Replace(sString, "&lfloor;", "&#8970;");
                    sString = Strings.Replace(sString, "&rfloor;", "&#8971;");
                    sString = Strings.Replace(sString, "&lang;", "&#9001;");
                    sString = Strings.Replace(sString, "&rang;", "&#9002;");
                    sString = Strings.Replace(sString, "&loz;", "&#9674;");
                    sString = Strings.Replace(sString, "&spades;", "&#9824;");
                    sString = Strings.Replace(sString, "&clubs;", "&#9827;");
                    sString = Strings.Replace(sString, "&hearts;", "&#9829;");
                    sString = Strings.Replace(sString, "&diams;", "&#9830;");

                    if (sString == null)
                        return "";
                    else
                    {
                        sString = Regex.Replace(sString, @"&(?!#?\w+;)", "&amp;");
                        return sString;
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "convertEntitiesToCodes", ex, ""));
                return "";
            }
        }

        public static string convertEntitiesToCodesFast(string sString)
        {
            try
            {
                string startString = sString;
                if (sString == null)
                    return "";
                else
                {
                    sString = Strings.Replace(sString, "Xmlns=\"\"", "");

                    char[] chars = sString.ToCharArray();
                    StringBuilder result = new StringBuilder(sString.Length + System.Convert.ToInt32((sString.Length * 0.1)));
                    foreach (char c in chars)
                    {
                        int value = Convert.ToInt32(c);
                        if (value > 127)
                            result.AppendFormat("&#{0};", value);
                        else
                            result.Append(c);
                    }

                    sString = result.ToString();

                    if (sString == null)
                        return "";
                    else
                    {
                        sString = Regex.Replace(sString, @"&(?!#?\w+;)", "&amp;");
                        return sString;
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "convertEntitiesToCodes", ex, ""));
                return "";
            }
        }


        public static string convertEntitiesToString(string sString)
        {
            try
            {
                sString = sString.Replace("&amp;", "&");
                sString = sString.Replace("&lt;", "<");
                sString = sString.Replace("&gt;", ">");

                return sString == null? "": sString;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "convertEntitiesToString", ex, ""));
                return "";
            }
        }

        public static string XmlDate(object dDate, bool bIncludeTime = false)
        {
            try
            {
                string cReturn;

                if (Information.IsDBNull(dDate) | !(Information.IsDate(dDate)))
                    cReturn = "";
                else if(dDate is DateTime){
                    string cFormat = "yyyy-MM-dd";
                    DateTime thisdate = (DateTime)dDate;
                    cReturn = thisdate.ToString(cFormat);
                }
                else
                {
                    string cFormat = "yyyy-MM-dd";
                    if (bIncludeTime)
                        cFormat += "THH:mm:ss";
                    cReturn = DateTime.Parse((string)dDate).ToString(cFormat);
                }
                return cReturn;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "XmlDate", ex, ""));
                return "";
            }
        }

        public static string GenerateConcatForXPath(string a_xPathQueryString)
        {
            string returnString = string.Empty;
            string searchString = a_xPathQueryString;
            char[] quoteChars = new char[] { '\'', '"' };

            int quotePos = searchString.IndexOfAny(quoteChars);
            if (quotePos == -1)
                returnString = "'" + searchString + "'";
            else
            {
                returnString = "concat(";
                while (quotePos != -1)
                {
                    string subString = searchString.Substring(0, quotePos);
                    returnString += "'" + subString + "', ";
                    if (searchString.Substring(quotePos, 1) == "'")
                        returnString += "\"'\", ";
                    else
                        // must be a double quote 
                        returnString += "'\"', ";
                    searchString = searchString.Substring(quotePos + 1, searchString.Length - quotePos - 1);
                    quotePos = searchString.IndexOfAny(quoteChars);
                }
                returnString += "'" + searchString + "')";
            }
            return returnString;
        }

        public static string encodeAllHTML(string shtml)
        {
            shtml = Strings.Replace(shtml, "&", "&amp;");
            shtml = Strings.Replace(shtml, "\"", "&quot;");
            shtml = Strings.Replace(shtml, "<", "&lt;");
            shtml = Strings.Replace(shtml, ">", "&gt;");
            return shtml;
        }

        public static XmlElement CombineNodes(XmlElement oMasterStructureNode, XmlElement oExistingDataNode)
        {
            // combines an existing instance with a new 
            try
            {
                // firstly we will go through and find like for like
                // we will skip the immediate node below instance as this is usually fairly static
                // except for attributes

                // Attributes of Root
                XmlElement oMasterFirstElement = firstElement(ref oMasterStructureNode);
                XmlElement oExistingFirstElement = firstElement(ref oExistingDataNode);

                if (oMasterFirstElement != null)
                {
                    if (oMasterFirstElement.HasAttributes)
                    {
                        var loopTo = oMasterFirstElement.Attributes.Count - 1;
                        for (int i = 0; i <= loopTo; i++)
                        {
                            XmlAttribute oMainAtt = oMasterFirstElement.Attributes[i];
                            if (!(oExistingDataNode.Attributes[oMainAtt.Name].Value == ""))
                                oMainAtt.Value = oExistingDataNode.Attributes[oMainAtt.Name].Value;
                        }
                    }

                    // Navigate Down Elmts and find with same xpath

                    foreach (XmlElement oMainElmt in oMasterFirstElement.SelectNodes("*"))
                        CombineInstance_Sub1(oMainElmt, oExistingDataNode, oMasterFirstElement.Name);

                    // Now for any existing stuff that has not been brought across
                    if (oExistingFirstElement != null)
                    {
                        foreach (XmlElement oExistElmt in oExistingFirstElement.SelectNodes("descendant-or-self::*[not(@ewCombineInstanceFound)]"))
                        {
                            if (!(oExistElmt.OuterXml == oExistingFirstElement.OuterXml))
                                CombineInstance_Sub2(oExistElmt, oMasterStructureNode, oMasterFirstElement.Name);
                        }
                    }


                    CombineInstance_MarkSubElmts(oMasterStructureNode, true);
                }

                return oMasterStructureNode;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CombineNodes", ex, ""));
                return oMasterStructureNode;
            }
        }

        public static XmlElement GetOrCreateSingleChildElement(ref XmlNode parentNode, string nodeName)
        {
            try
            {
                XmlElement returnNode = (XmlElement)parentNode.SelectSingleNode(nodeName);
                if (returnNode == null)
                {
                    returnNode = parentNode.OwnerDocument.CreateElement(nodeName);
                    parentNode.AppendChild(returnNode);
                }

                return returnNode;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetOrCreateSingleChildElement", ex, ""));
                return null;
            }
        }

        /// <summary>
        /// Attempts to set an element's inner xml from a string - if it fails it will set the inner text instead
        /// </summary>
        /// <param name="element">the element to set</param>
        /// <param name="value">the string to try and set the element for</param>
        /// <remarks></remarks>
        public static void SetInnerXmlThenInnerText(ref XmlElement element, string value)
        {
            if (element != null)
            {
                try
                {
                    element.InnerXml = value;
                }
                catch (XmlException ex)
                {
                    element.InnerText = value;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SetInnerXmlThenInnerText", ex, ""));
                }
            }
        }

        public static System.Collections.Generic.Dictionary<string, string> XmltoDictionary(XmlElement oXml)
        {
            try
            {
                var myDict = new System.Collections.Generic.Dictionary<string, string>();

                XmltoDictionaryNode(oXml, ref myDict, oXml.Name);

                return myDict;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "XmltoDictionary", ex, ""));
                return null;
            }
        }

        private static void XmltoDictionaryNode(XmlElement oThisElmt, ref Dictionary<string, string> myDict, string Prefix)
        {
            try
            {
                foreach (XmlAttribute Attribute in oThisElmt.Attributes)
                    myDict.Add(Prefix + ".-" + Attribute.Name, Attribute.Value);
                if (oThisElmt.SelectNodes("*").Count == 0)
                {
                    if (oThisElmt.InnerText != "" & !(myDict.ContainsKey(Prefix + "." + oThisElmt.Name)))
                        myDict.Add(Prefix, oThisElmt.InnerText);
                }
                else
                    foreach (XmlElement oElmt in oThisElmt.SelectNodes("*"))
                        XmltoDictionaryNode(oElmt, ref myDict, Prefix + "." + oElmt.Name);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "XmltoDictionaryNode", ex, ""));
            }
        }





        /// <summary>
        /// Serializes an object and returns the serialization as a string representation of UTF-8 encoded XML
        /// </summary>
        /// <typeparam name="T">The type of object to serialize</typeparam>
        /// <param name="value">The object to serialize</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string Serialize<T>(T value)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            System.IO.MemoryStream mStream = new System.IO.MemoryStream();
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UTF8Encoding(false);
            settings.Indent = false;
            settings.OmitXmlDeclaration = false;
            XmlWriter xWriter = XmlWriter.Create(mStream, settings);
            serializer.Serialize(xWriter, value);

            string serializedString = Tools.Text.UTF8ByteArrayToString(mStream.ToArray());
            mStream.Close();

            return serializedString;
        }

        public static XmlNode SerializeToXml<T>(T value)
        {
            string cProcessInfo = "";
            try
            {

                // Get the serialized object
                string serializedObject = Serialize<T>(value);

                cProcessInfo = serializedObject;

                if (!(serializedObject == null))
                {
                    // Remove the namespaces
                    serializedObject = System.Text.RegularExpressions.Regex.Replace(serializedObject, @"xmlns(:\w+?)?="".*?""", "");
                    serializedObject = System.Text.RegularExpressions.Regex.Replace(serializedObject, @"<\?xml.+\?>", "");
                    serializedObject = System.Text.RegularExpressions.Regex.Replace(serializedObject, @"xsi:nil=""true""", "");
                }

                // Remove the declaration

                // Load into XML
                XmlDocument serializedObjectDocument = new XmlDocument();
                serializedObjectDocument.LoadXml(serializedObject);

                return serializedObjectDocument.DocumentElement;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Read", ex, cProcessInfo));
                return null;
            }
        }

        public static void AddExistingNode(ref XmlElement nodeToAddTo, XmlNode nodeToBeAdded)
        {
            nodeToAddTo.AppendChild(nodeToAddTo.OwnerDocument.ImportNode(nodeToBeAdded, true));
        }

        public static void AddExistingNode(ref XmlNode nodeToAddTo, XmlNode nodeToBeAdded)
        {
            nodeToAddTo.AppendChild(nodeToAddTo.OwnerDocument.ImportNode(nodeToBeAdded, true));
        }



        public class XmlNoNamespaceWriter : System.Xml.XmlTextWriter
        {
            private bool skipAttribute = false;

            public XmlNoNamespaceWriter(System.IO.TextWriter writer) : base(writer)
            {
            }

            public XmlNoNamespaceWriter(System.IO.Stream stream, System.Text.Encoding encoding) : base(stream, encoding)
            {
            }

            public override void WriteStartElement(string prefix, string localName, string ns)
            {
                base.WriteStartElement(null, localName, null);
            }

            public override void WriteStartAttribute(string prefix, string localName, string ns)
            {
                if (prefix.CompareTo("Xmlns") == 0 | localName.CompareTo("xmlns") == 0)
                    skipAttribute = true;
                else
                    base.WriteStartAttribute(null, localName, null);
            }

            public override void WriteString(string text)
            {
                if (!skipAttribute)
                    base.WriteString(text);
            }

            public override void WriteEndAttribute()
            {
                if (!skipAttribute)
                    base.WriteEndAttribute();
                skipAttribute = false;
            }

            public override void WriteQualifiedName(string localName, string ns)
            {
                base.WriteQualifiedName(localName, null);
            }
        }

        public class XmlSanitizingStream : System.IO.StreamReader
        {
            public static event OnErrorEventHandler OnError;

            public delegate void OnErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);

            private const string mcModuleName = "Protean.Tools.Xml.XmlSanitizingStream";


            // Pass 'true' to automatically detect encoding using BOMs.
            // BOMs: http://en.wikipedia.org/wiki/Byte-order_mark

            public XmlSanitizingStream(System.IO.Stream streamToSanitize) : base(streamToSanitize, true)
            {
            }

            /// <summary>
            /// Whether a given character is allowed by XML 1.0.
            /// </summary>
            public static bool IsLegalXmlChar(int character)
            {
                // == '\t' == 9   
                // == '\n' == 10  
                // == '\r' == 13  
                return (character == 0x9 || character == 0xA || character == 0xD || (character >= 0x20 && character <= 0xD7FF) || (character >= 0xE000 && character <= 0xFFFD) || (character >= 0x10000 && character <= 0x10FFFF));
            }

            private const int EOF = -1;

            public override int Read()
            {
                // Read each char, skipping ones XML has prohibited

                int nextCharacter = 0;

                do
                {
                    // Read a character

                    if ((InlineAssignHelper(nextCharacter, base.Read())) == EOF)
                        // If the char denotes end of file, stop
                        break;
                }
                while (!XmlSanitizingStream.IsLegalXmlChar(nextCharacter));

                return nextCharacter;
            }

            public override int Peek()
            {
                // Return next legal XML char w/o reading it 

                int nextCharacter;

                do
                    // See what the next character is 
                    nextCharacter = base.Peek();
                while (!XmlSanitizingStream.IsLegalXmlChar(nextCharacter) && (InlineAssignHelper(nextCharacter, base.Read())) != EOF);

                return nextCharacter;
            }

            // Public Overrides Function Read(buffer As Char(), index As Integer, count As Integer) As Integer
            // Dim cProcessInfo As String = Nothing
            // Try
            // If buffer Is Nothing Then
            // Throw New ArgumentNullException("buffer")
            // End If
            // If index < 0 Then
            // Throw New ArgumentOutOfRangeException("index")
            // End If
            // If count < 0 Then
            // Throw New ArgumentOutOfRangeException("count")
            // End If
            // If (buffer.Length - index) < count Then
            // Throw New ArgumentException()
            // End If
            // Dim num As Integer = 0
            // Do
            // Dim num2 As Integer = Me.Read()
            // If num2 = -1 Then
            // Return num
            // End If
            // cProcessInfo = buffer.Length
            // buffer(index + System.Math.Max(System.Threading.Interlocked.Increment(num), num - 1)) = ChrW(num2)
            // Loop While num < count
            // Return num
            // Catch ex As Exception
            // RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Read", ex, cProcessInfo))
            // Return Nothing
            // End Try
            // End Function

            public override int ReadBlock(char[] buffer, int index, int count)
            {
                int num = 0;
                int num2 = 0;
                do
                    num2 += InlineAssignHelper(num, this.Read(buffer, index + num2, count - num2));
                while ((num > 0) && (num2 < count));
                return num2;
            }

            public override string ReadLine()
            {
                StringBuilder builder = new StringBuilder();
                while (true)
                {
                    int num = this.Read();
                    switch (num)
                    {
                        case -1:
                            {
                                if (builder.Length > 0)
                                    return builder.ToString();
                                return null;
                            }

                        case 13:
                        case 10:
                            {
                                if ((num == 13) && (this.Peek() == 10))
                                    this.Read();
                                return builder.ToString();
                            }

                        default:
                            {
                                return null;
                            }
                    }
                    builder.Append((char)num);
                }
            }

            public override string ReadToEnd()
            {
                int num = 0;
                char[] buffer = new char[4096];
                StringBuilder builder = new StringBuilder(0x1000);
                while ((InlineAssignHelper(num, this.Read(buffer, 0, buffer.Length))) != 0)
                    builder.Append(buffer, 0, num);
                return builder.ToString();
            }

            private static T InlineAssignHelper<T>(T target, T value)
            {
                target = value;
                return value;
            }
        }
    }

}

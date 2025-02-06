using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

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
                string[] oParts = cXPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
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
            catch (Exception)
            {
                return oElmt.ChildNodes;
            }
        }

        public static string XmlToForm(string sString)
        {
            string sProcessInfo = "XmlToForm";
            try
            {
                sString = sString.Replace("<", "&lt;");
                sString = sString.Replace(">", "&gt;");
                return sString + "";
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, sProcessInfo, ex, ""));
                return "";
            }
        }

        public static class HtmlConverter
        {
            public static XmlDocument htmlToXmlDoc(string shtml)
            {
                XmlDocument oXmlDoc = new XmlDocument();
                string sProcessInfo = "htmlToXmlDoc";

                try
                {
                    //shtml = Strings.Replace(shtml, "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">", "");
                    string regexOfDoctype = "<!DOCTYPE((.|\n|\r)*?)\">";
                    shtml = Regex.Replace(shtml, regexOfDoctype, string.Empty, RegexOptions.IgnoreCase);
                    shtml = shtml.Replace(" Xmlns = \"http://www.w3.org/1999/xhtml\"", "");
                    shtml = shtml.Replace(" xmlns=\"http://www.w3.org/1999/xhtml\"", "");
                    shtml = shtml.Replace(" Xml:lang=\"\"", "");
                    shtml = shtml.Replace(" xml:lang=\"\"", "");

                    oXmlDoc.LoadXml(shtml);

                    return oXmlDoc;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, sProcessInfo, ex, ""));
                    return null;
                }
            }

            public static event Action<object, Protean.Tools.Errors.ErrorEventArgs> OnError;
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
                if (!cXpath.Contains("ews:"))
                {
                    if (nsMgr.HasNamespace("ews"))
                    {
                        cXpath = "ews:" + cXpath.Replace("/", "/ews:");

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

                        cXpath = cXpath.Replace("/ews:node()", "/node()");
                        cXpath = cXpath.Replace("/ews:@", "/@");
                        cXpath = cXpath.Replace("[ews:@", "[@");
                        cXpath = cXpath.Replace("[ews:position()", "[position()");
                    }
                }

                return cXpath;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "addNsToXpath", ex, ""));
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
                XmlNodeList htmlNodeList = oElmt.SelectNodes("descendant-or-self::*[p | div | ul | span | b | strong | h | section | a]");
                if (htmlNodeList.Count > 0) //this is required to have htmlNodeList return all correct descendant nodes.
                {
                    foreach (XmlNode htmlItem in htmlNodeList)
                    {
                        if (htmlItem.ParentNode != null)
                        {
                            htmlItem.InnerXml = "<![CDATA[" + htmlItem.InnerXml + "]]>";
                        }
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

        public static XmlElement addNewTextNode(string cNodeName, ref XmlElement oElmtIn, string cNodeValue = "", bool bAppendNotPrepend = true, bool bOverwriteExistingNode = false)
        {
            try
            {
                XmlElement oElem;
                if (bOverwriteExistingNode)
                {
                    // Search for an existing node - delete it if it exists
                    oElem = (XmlElement)oElmtIn.SelectSingleNode(cNodeName);
                    if (!(oElem == null))
                        oElmtIn.RemoveChild(oElem);
                }
                oElem = oElmtIn.OwnerDocument.CreateElement(cNodeName);
                if (cNodeValue != "")
                    oElem.InnerText = cNodeValue;
                if (bAppendNotPrepend)
                    oElmtIn.AppendChild(oElem);
                else
                    oElmtIn.PrependChild(oElem);
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
                            cValue = double.TryParse(Convert.ToString(vDefaultValue), out double parsedValue) ? parsedValue : 0;
                            break;
                        }

                    case XmlDataType.TypeDate:
                        {
                            cValue = DateTime.TryParse(Convert.ToString(vDefaultValue), out DateTime parsedDate) ? parsedDate : DateTime.Now;

                            break;
                        }

                    default:
                        {
                            cValue = (string)vDefaultValue != "" ? vDefaultValue.ToString() : "";
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
                            cValue = double.TryParse(Convert.ToString(cValue), out double numberValue) ? numberValue :
                                      double.TryParse(Convert.ToString(vDefaultValue), out numberValue) ? numberValue : 0;
                            break;
                        }

                    case XmlDataType.TypeDate:
                        {
                            cValue = DateTime.TryParse(Convert.ToString(cValue), out DateTime dateValue) ? dateValue :
                                      DateTime.TryParse(Convert.ToString(vDefaultValue), out dateValue) ? dateValue : DateTime.Now;
                            break;
                        }

                    default:
                        {
                            cValue = !string.IsNullOrEmpty(Convert.ToString(cValue)) ?
                                      cValue.ToString() :
                                      !string.IsNullOrEmpty(Convert.ToString(vDefaultValue)) ?
                                      Convert.ToString(vDefaultValue) : "";
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
                    catch (Exception)
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
                return NodeStateWithReturns(ref oNode, xPath, populateAsText, populateAsXml, populateState, ref returnElement, ref returnAsXml, ref returnAsText, bCheckTrimmedInnerText);

            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "NodeState", ex, ""));
                return default(XmlNodeState);
            }
        }


        public static Xml.XmlNodeState NodeState(ref XmlElement oNode, string xPath, ref XmlElement returnElement)
        {
            string populateAsText = "";
            string populateAsXml = "";
            Xml.XmlNodeState populateState = XmlNodeState.IsEmpty;
            string returnAsXml = "";
            string returnAsText = "";
            bool bCheckTrimmedInnerText = false;

            try
            {
                return NodeStateWithReturns(ref oNode, xPath, populateAsText, populateAsXml, populateState, ref returnElement, ref returnAsXml, ref returnAsText, bCheckTrimmedInnerText);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "NodeState", ex, ""));
                return default(XmlNodeState);
            }
        }


        private static Xml.XmlNodeState NodeStateWithReturns(ref XmlElement oNode, string xPath, string populateAsText, string populateAsXml, Xml.XmlNodeState populateState, ref XmlElement returnElement, ref string returnAsXml, ref string returnAsText, bool bCheckTrimmedInnerText)
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
                            catch (Exception)
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
                    sString = sString.Replace("Xmlns=\"\"", "");

                    // sString = sString.Replace( "& ", "&amp; ")
                    sString = sString.Replace("’", "'");

                    // strip out any empty tags left by tinyMCE as the complier converts the to <h1/>
                    sString = sString.Replace("<h1></h1>", "");
                    sString = sString.Replace("<h2></h2>", "");
                    sString = sString.Replace("<h3></h3>", "");
                    sString = sString.Replace("<h4></h4>", "");
                    sString = sString.Replace("<h5></h5>", "");
                    sString = sString.Replace("<h6></h6>", "");
                    sString = sString.Replace("<span></span>", "");

                    // re.Replace(sString, "&^(;)" "&amp;")

                    // xhtml Tidy
                    sString = sString.Replace("<o:p>", "<p>");
                    sString = sString.Replace("</o:p>", "</p>");
                    sString = sString.Replace("<o:p/>", "<p/>");

                    // sString = sString.Replace( "&lt;", "&#60;")
                    // sString = sString.Replace( "&gt;", "&#92;")

                    //sString = sString.Replace( "&amp;", "&");

                    sString = sString.Replace("&quot;", "&#34;");
                    sString = sString.Replace("&apos;", "&#39;");
                    sString = sString.Replace("&nbsp;", "&#160;");
                    sString = sString.Replace("&iexcl;", "&#161;");
                    sString = sString.Replace("&cent;", "&#162;");
                    sString = sString.Replace("&pound;", "&#163;");
                    sString = sString.Replace("&curren;", "&#164;");
                    sString = sString.Replace("&yen;", "&#165;");
                    sString = sString.Replace("&brvbar;", "&#166;");
                    sString = sString.Replace("&sect;", "&#167;");
                    sString = sString.Replace("&uml;", "&#168;");
                    sString = sString.Replace("&copy;", "&#169;");
                    sString = sString.Replace("&ordf;", "&#170;");
                    sString = sString.Replace("&laquo;", "&#171;");
                    sString = sString.Replace("&not;", "&#172;");
                    sString = sString.Replace("&shy;", "&#173;");
                    sString = sString.Replace("&reg;", "&#174;");
                    sString = sString.Replace("&macr;", "&#175;");
                    sString = sString.Replace("&deg;", "&#176;");
                    sString = sString.Replace("&plusmn;", "&#177;");
                    sString = sString.Replace("&sup2;", "&#178;");
                    sString = sString.Replace("&sup3;", "&#179;");
                    sString = sString.Replace("&acute;", "&#180;");
                    sString = sString.Replace("&micro;", "&#181;");
                    sString = sString.Replace("&para;", "&#182;");
                    sString = sString.Replace("&middot;", "&#183;");
                    sString = sString.Replace("&cedil;", "&#184;");
                    sString = sString.Replace("&sup1;", "&#185;");
                    sString = sString.Replace("&ordm;", "&#186;");
                    sString = sString.Replace("&raquo;", "&#187;");
                    sString = sString.Replace("&frac14;", "&#188;");
                    sString = sString.Replace("&frac12;", "&#189;");
                    sString = sString.Replace("&frac34;", "&#190;");
                    sString = sString.Replace("&iquest;", "&#191;");
                    sString = sString.Replace("&Agrave;", "&#192;");
                    sString = sString.Replace("&Aacute;", "&#193;");
                    sString = sString.Replace("&Acirc;", "&#194;");
                    sString = sString.Replace("&Atilde;", "&#195;");
                    sString = sString.Replace("&Auml;", "&#196;");
                    sString = sString.Replace("&Aring;", "&#197;");
                    sString = sString.Replace("&AElig;", "&#198;");
                    sString = sString.Replace("&Ccedil;", "&#199;");
                    sString = sString.Replace("&Egrave;", "&#200;");
                    sString = sString.Replace("&Eacute;", "&#201;");
                    sString = sString.Replace("&Ecirc;", "&#202;");
                    sString = sString.Replace("&Euml;", "&#203;");
                    sString = sString.Replace("&Igrave;", "&#204;");
                    sString = sString.Replace("&Iacute;", "&#205;");
                    sString = sString.Replace("&Icirc;", "&#206;");
                    sString = sString.Replace("&Iuml;", "&#207;");
                    sString = sString.Replace("&ETH;", "&#208;");
                    sString = sString.Replace("&Ntilde;", "&#209;");
                    sString = sString.Replace("&Ograve;", "&#210;");
                    sString = sString.Replace("&Oacute;", "&#211;");
                    sString = sString.Replace("&Ocirc;", "&#212;");
                    sString = sString.Replace("&Otilde;", "&#213;");
                    sString = sString.Replace("&Ouml;", "&#214;");
                    sString = sString.Replace("&times;", "&#215;");
                    sString = sString.Replace("&Oslash;", "&#216;");
                    sString = sString.Replace("&Ugrave;", "&#217;");
                    sString = sString.Replace("&Uacute;", "&#218;");
                    sString = sString.Replace("&Ucirc;", "&#219;");
                    sString = sString.Replace("&Uuml;", "&#220;");
                    sString = sString.Replace("&Yacute;", "&#221;");
                    sString = sString.Replace("&THORN;", "&#222;");
                    sString = sString.Replace("&szlig;", "&#223;");
                    sString = sString.Replace("&agrave;", "&#224;");
                    sString = sString.Replace("&aacute;", "&#225;");
                    sString = sString.Replace("&acirc;", "&#226;");
                    sString = sString.Replace("&atilde;", "&#227;");
                    sString = sString.Replace("&auml;", "&#228;");
                    sString = sString.Replace("&aring;", "&#229;");
                    sString = sString.Replace("&aelig;", "&#230;");
                    sString = sString.Replace("&ccedil;", "&#231;");
                    sString = sString.Replace("&egrave;", "&#232;");
                    sString = sString.Replace("&eacute;", "&#233;");
                    sString = sString.Replace("&ecirc;", "&#234;");
                    sString = sString.Replace("&euml;", "&#235;");
                    sString = sString.Replace("&igrave;", "&#236;");
                    sString = sString.Replace("&iacute;", "&#237;");
                    sString = sString.Replace("&icirc;", "&#238;");
                    sString = sString.Replace("&iuml;", "&#239;");
                    sString = sString.Replace("&eth;", "&#240;");
                    sString = sString.Replace("&ntilde;", "&#241;");
                    sString = sString.Replace("&ograve;", "&#242;");
                    sString = sString.Replace("&oacute;", "&#243;");
                    sString = sString.Replace("&ocirc;", "&#244;");
                    sString = sString.Replace("&otilde;", "&#245;");
                    sString = sString.Replace("&ouml;", "&#246;");
                    sString = sString.Replace("&divide;", "&#247;");
                    sString = sString.Replace("&oslash;", "&#248;");
                    sString = sString.Replace("&ugrave;", "&#249;");
                    sString = sString.Replace("&uacute;", "&#250;");
                    sString = sString.Replace("&ucirc;", "&#251;");
                    sString = sString.Replace("&uuml;", "&#252;");
                    sString = sString.Replace("&yacute;", "&#253;");
                    sString = sString.Replace("&thorn;", "&#254;");
                    sString = sString.Replace("&yuml;", "&#255;");
                    sString = sString.Replace("&OElig;", "&#338;");
                    sString = sString.Replace("&oelig;", "&#339;");
                    sString = sString.Replace("&Scaron;", "&#352;");
                    sString = sString.Replace("&scaron;", "&#353;");
                    sString = sString.Replace("&Yuml;", "&#376;");
                    sString = sString.Replace("&fnof;", "&#402;");
                    sString = sString.Replace("&circ;", "&#710;");
                    sString = sString.Replace("&tilde;", "&#732;");
                    sString = sString.Replace("&Alpha;", "&#913;");
                    sString = sString.Replace("&Beta;", "&#914;");
                    sString = sString.Replace("&Gamma;", "&#915;");
                    sString = sString.Replace("&Delta;", "&#916;");
                    sString = sString.Replace("&Epsilon;", "&#917;");
                    sString = sString.Replace("&Zeta;", "&#918;");
                    sString = sString.Replace("&Eta;", "&#919;");
                    sString = sString.Replace("&Theta;", "&#920;");
                    sString = sString.Replace("&Iota;", "&#921;");
                    sString = sString.Replace("&Kappa;", "&#922;");
                    sString = sString.Replace("&Lambda;", "&#923;");
                    sString = sString.Replace("&Mu;", "&#924;");
                    sString = sString.Replace("&Nu;", "&#925;");
                    sString = sString.Replace("&Xi;", "&#926;");
                    sString = sString.Replace("&Omicron;", "&#927;");
                    sString = sString.Replace("&Pi;", "&#928;");
                    sString = sString.Replace("&Rho;", "&#929;");
                    sString = sString.Replace("&Sigma;", "&#931;");
                    sString = sString.Replace("&Tau;", "&#932;");
                    sString = sString.Replace("&Upsilon;", "&#933;");
                    sString = sString.Replace("&Phi;", "&#934;");
                    sString = sString.Replace("&Chi;", "&#935;");
                    sString = sString.Replace("&Psi;", "&#936;");
                    sString = sString.Replace("&Omega;", "&#937;");
                    sString = sString.Replace("&alpha;", "&#945;");
                    sString = sString.Replace("&beta;", "&#946;");
                    sString = sString.Replace("&gamma;", "&#947;");
                    sString = sString.Replace("&delta;", "&#948;");
                    sString = sString.Replace("&epsilon;", "&#949;");
                    sString = sString.Replace("&zeta;", "&#950;");
                    sString = sString.Replace("&eta;", "&#951;");
                    sString = sString.Replace("&theta;", "&#952;");
                    sString = sString.Replace("&iota;", "&#953;");
                    sString = sString.Replace("&kappa;", "&#954;");
                    sString = sString.Replace("&lambda;", "&#955;");
                    sString = sString.Replace("&mu;", "&#956;");
                    sString = sString.Replace("&nu;", "&#957;");
                    sString = sString.Replace("&xi;", "&#958;");
                    sString = sString.Replace("&omicron;", "&#959;");
                    sString = sString.Replace("&pi;", "&#960;");
                    sString = sString.Replace("&rho;", "&#961;");
                    sString = sString.Replace("&sigmaf;", "&#962;");
                    sString = sString.Replace("&sigma;", "&#963;");
                    sString = sString.Replace("&tau;", "&#964;");
                    sString = sString.Replace("&upsilon;", "&#965;");
                    sString = sString.Replace("&phi;", "&#966;");
                    sString = sString.Replace("&chi;", "&#967;");
                    sString = sString.Replace("&psi;", "&#968;");
                    sString = sString.Replace("&omega;", "&#969;");
                    sString = sString.Replace("&thetasym;", "&#977;");
                    sString = sString.Replace("&upsih;", "&#978;");
                    sString = sString.Replace("&piv;", "&#982;");
                    sString = sString.Replace("&ensp;", "&#8194;");
                    sString = sString.Replace("&emsp;", "&#8195;");
                    sString = sString.Replace("&thinsp;", "&#8201;");
                    sString = sString.Replace("&zwnj;", "&#8204;");
                    sString = sString.Replace("&zwj;", "&#8205;");
                    sString = sString.Replace("&lrm;", "&#8206;");
                    sString = sString.Replace("&rlm;", "&#8207;");
                    sString = sString.Replace("&ndash;", "&#8211;");
                    sString = sString.Replace("&mdash;", "&#8212;");
                    sString = sString.Replace("&lsquo;", "&#8216;");
                    sString = sString.Replace("&rsquo;", "&#8217;");
                    sString = sString.Replace("&sbquo;", "&#8218;");
                    sString = sString.Replace("&ldquo;", "&#8220;");
                    sString = sString.Replace("&rdquo;", "&#8221;");
                    sString = sString.Replace("&bdquo;", "&#8222;");
                    sString = sString.Replace("&dagger;", "&#8224;");
                    sString = sString.Replace("&Dagger;", "&#8225;");
                    sString = sString.Replace("&bull;", "&#8226;");
                    sString = sString.Replace("&hellip;", "&#8230;");
                    sString = sString.Replace("&permil;", "&#8240;");
                    sString = sString.Replace("&prime;", "&#8242;");
                    sString = sString.Replace("&Prime;", "&#8243;");
                    sString = sString.Replace("&lsaquo;", "&#8249;");
                    sString = sString.Replace("&rsaquo;", "&#8250;");
                    sString = sString.Replace("&oline;", "&#8254;");
                    sString = sString.Replace("&frasl;", "&#8260;");
                    sString = sString.Replace("&euro;", "&#8364;");
                    sString = sString.Replace("&image;", "&#8465;");
                    sString = sString.Replace("&weierp;", "&#8472;");
                    sString = sString.Replace("&real;", "&#8476;");
                    sString = sString.Replace("&trade;", "&#8482;");
                    sString = sString.Replace("&alefsym;", "&#8501;");
                    sString = sString.Replace("&larr;", "&#8592;");
                    sString = sString.Replace("&uarr;", "&#8593;");
                    sString = sString.Replace("&rarr;", "&#8594;");
                    sString = sString.Replace("&darr;", "&#8595;");
                    sString = sString.Replace("&harr;", "&#8596;");
                    sString = sString.Replace("&crarr;", "&#8629;");
                    sString = sString.Replace("&lArr;", "&#8656;");
                    sString = sString.Replace("&uArr;", "&#8657;");
                    sString = sString.Replace("&rArr;", "&#8658;");
                    sString = sString.Replace("&dArr;", "&#8659;");
                    sString = sString.Replace("&hArr;", "&#8660;");
                    sString = sString.Replace("&forall;", "&#8704;");
                    sString = sString.Replace("&part;", "&#8706;");
                    sString = sString.Replace("&exist;", "&#8707;");
                    sString = sString.Replace("&empty;", "&#8709;");
                    sString = sString.Replace("&nabla;", "&#8711;");
                    sString = sString.Replace("&isin;", "&#8712;");
                    sString = sString.Replace("&notin;", "&#8713;");
                    sString = sString.Replace("&ni;", "&#8715;");
                    sString = sString.Replace("&prod;", "&#8719;");
                    sString = sString.Replace("&sum;", "&#8721;");
                    sString = sString.Replace("&minus;", "&#8722;");
                    sString = sString.Replace("&lowast;", "&#8727;");
                    sString = sString.Replace("&radic;", "&#8730;");
                    sString = sString.Replace("&prop;", "&#8733;");
                    sString = sString.Replace("&infin;", "&#8734;");
                    sString = sString.Replace("&ang;", "&#8736;");
                    sString = sString.Replace("&and;", "&#8743;");
                    sString = sString.Replace("&or;", "&#8744;");
                    sString = sString.Replace("&cap;", "&#8745;");
                    sString = sString.Replace("&cup;", "&#8746;");
                    sString = sString.Replace("&int;", "&#8747;");
                    sString = sString.Replace("&there4;", "&#8756;");
                    sString = sString.Replace("&sim;", "&#8764;");
                    sString = sString.Replace("&cong;", "&#8773;");
                    sString = sString.Replace("&asymp;", "&#8776;");
                    sString = sString.Replace("&ne;", "&#8800;");
                    sString = sString.Replace("&equiv;", "&#8801;");
                    sString = sString.Replace("&le;", "&#8804;");
                    sString = sString.Replace("&ge;", "&#8805;");
                    sString = sString.Replace("&sub;", "&#8834;");
                    sString = sString.Replace("&sup;", "&#8835;");
                    sString = sString.Replace("&nsub;", "&#8836;");
                    sString = sString.Replace("&sube;", "&#8838;");
                    sString = sString.Replace("&supe;", "&#8839;");
                    sString = sString.Replace("&oplus;", "&#8853;");
                    sString = sString.Replace("&otimes;", "&#8855;");
                    sString = sString.Replace("&perp;", "&#8869;");
                    sString = sString.Replace("&sdot;", "&#8901;");
                    sString = sString.Replace("&lceil;", "&#8968;");
                    sString = sString.Replace("&rceil;", "&#8969;");
                    sString = sString.Replace("&lfloor;", "&#8970;");
                    sString = sString.Replace("&rfloor;", "&#8971;");
                    sString = sString.Replace("&lang;", "&#9001;");
                    sString = sString.Replace("&rang;", "&#9002;");
                    sString = sString.Replace("&loz;", "&#9674;");
                    sString = sString.Replace("&spades;", "&#9824;");
                    sString = sString.Replace("&clubs;", "&#9827;");
                    sString = sString.Replace("&hearts;", "&#9829;");
                    sString = sString.Replace("&diams;", "&#9830;");

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
                if (sString == null | sString == "")
                    return "";
                else
                {
                    sString = sString.Replace("Xmlns=\"\"", "");

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

                return sString == null ? "" : sString;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "convertEntitiesToString", ex, ""));
                return "";
            }
        }

        public static string convertStringToEntityCodes(string sString)
        {
            try
            {

                sString = sString.Replace(((char)0).ToString(), ((char)32).ToString());
                sString = sString.Replace(((char)8).ToString(), ((char)32).ToString());
                sString = sString.Replace(((char)20).ToString(), ((char)32).ToString());
                sString = sString.Replace("&nbsp;", "&#160;");
                sString = sString.Replace("¢", "&#162;");
                sString = sString.Replace("£", "&#163;");
                sString = sString.Replace("‘", "&#8216;");
                sString = sString.Replace("’", "&#8217;");
                sString = sString.Replace("§", "&#167;");
                sString = sString.Replace("©", "&#169;");
                sString = sString.Replace("«", "&#171;");
                sString = sString.Replace("»", "&#187;");
                sString = sString.Replace("®", "&#174;");
                sString = sString.Replace("°", "&#176;");
                sString = sString.Replace("±", "&#177;");
                sString = sString.Replace("¶", "&#182;");
                sString = sString.Replace("·", "&#183;");
                sString = sString.Replace("½", "&#188;");
                sString = sString.Replace("–", "&#8211;");
                sString = sString.Replace("—", "&#8212;");
                sString = sString.Replace("‚", "&#8218;");
                sString = sString.Replace("“", "&#8220;");
                sString = sString.Replace("”", "&#8221;");
                sString = sString.Replace("„", "&#8222;");
                sString = sString.Replace("†", "&#8224;");
                sString = sString.Replace("‡", "&#8224;");
                sString = sString.Replace("•", "&#8226;");
                sString = sString.Replace("…", "&#8230;");
                sString = sString.Replace("′", "&#8242;");
                sString = sString.Replace("″", "&#8243;");
                sString = sString.Replace("™", "&#8482;");
                sString = sString.Replace("≈", "&#8776;");
                sString = sString.Replace("≠", "&#8800;");
                sString = sString.Replace("≤", "&#8804;");
                sString = sString.Replace("≥", "&#8805;");
                sString = sString.Replace("≠", "&#8800;");

                return sString == null ? "" : sString;
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

                if (dDate == DBNull.Value || !DateTime.TryParse(Convert.ToString(dDate), out _))
                    cReturn = "";
                else if (dDate is DateTime)
                {
                    string cFormat = "yyyy-MM-dd";
                    if (bIncludeTime)
                        cFormat += "THH:mm:ss";
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
            shtml = shtml.Replace("&", "&amp;");
            shtml = shtml.Replace("\"", "&quot;");
            shtml = shtml.Replace("<", "&lt;");
            shtml = shtml.Replace(">", "&gt;");
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
                   OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SetInnerXmlThenInnerText", ex, ""));
                    element.InnerText = value;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SetInnerXmlThenInnerText", ex, ""));
                }
            }
        }

        public static System.Collections.Generic.Dictionary<string, string> XmltoDictionary(XmlElement oXml, bool skipPrefix = false)
        {
            try
            {
                var myDict = new System.Collections.Generic.Dictionary<string, string>();
                if (skipPrefix)
                {
                    XmltoDictionaryNodeSkippingPrefix(oXml, ref myDict);
                }
                else
                {
                    XmltoDictionaryNode(oXml, ref myDict, oXml.Name);
                }

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
                    if (oThisElmt.InnerText != "" & !(myDict.ContainsKey(Prefix)))
                        myDict.Add(Prefix, oThisElmt.InnerText);
                }
                else
                {
                    foreach (XmlElement oElmt in oThisElmt.SelectNodes("*"))
                    {
                        XmltoDictionaryNode(oElmt, ref myDict, Prefix + "." + oElmt.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "XmltoDictionaryNode", ex, ""));
            }
        }

        private static void XmltoDictionaryNodeSkippingPrefix(XmlElement oThisElmt, ref Dictionary<string, string> myDict)
        {
            try
            {
                foreach (XmlAttribute Attribute in oThisElmt.Attributes)
                    myDict.Add(Attribute.Value, oThisElmt.InnerText);
                if (oThisElmt.SelectNodes("*").Count == 0)
                {
                    if (oThisElmt.InnerText != "" & !(myDict.ContainsKey(oThisElmt.Name)))
                        myDict.Add(oThisElmt.Name, oThisElmt.InnerText);
                }
                else
                    foreach (XmlElement oElmt in oThisElmt.SelectNodes("*"))
                        XmltoDictionaryNodeSkippingPrefix(oElmt, ref myDict);
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

        public static Hashtable xmlToHashTable(XmlNodeList oNodeList, string cNamedAttribute = "")
        {

            // This converts a nodelist to a HashTable
            // The key will be the node name, the value will either be the Inner Text or a named attribute

            var oHT = new Hashtable();
            string cKey;
            string cValue;

            foreach (XmlElement oElmt in oNodeList)
            {
                cKey = oElmt.LocalName;
                if (!string.IsNullOrEmpty(cNamedAttribute))
                {
                    cValue = oElmt.GetAttribute(cNamedAttribute);
                }
                else
                {
                    cValue = oElmt.InnerText;
                }
                oHT.Add(cKey, cValue);
            }

            return oHT;

        }

        public static Hashtable xmlToHashTable(XmlNode oNode, string cNamedAttribute = "")
        {

            // This converts a node's child nodes to a HashTable
            return xmlToHashTable(oNode.ChildNodes, cNamedAttribute);

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
            //public static event OnErrorEventHandler OnError;

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

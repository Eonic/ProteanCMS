using Protean.CMS;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.XPath;

public partial class Proxy : MarshalByRefObject
{
    public Assembly GetAssembly(string assemblyPath)
    {
        try
        {
            return Assembly.LoadFile(assemblyPath);
        }
        catch (Exception)
        {
            // throw new InvalidOperationException(ex);
            return null;
        }
    }
}

public static partial class xmlTools
{

    public static XmlNodeList SortedNodeList_UNCONNECTED(XmlElement oRootObject, string cXPath, string cSortItem, XmlSortOrder cSortOrder, XmlDataType cSortDataType, XmlCaseOrder cCaseOrder)
    {
        var oElmt = oRootObject.OwnerDocument.CreateElement("List");
        try
        {
            var oNavigator = oRootObject.CreateNavigator();
            var oExpression = oNavigator.Compile(cXPath);
            oExpression.AddSort(cSortItem, cSortOrder, cCaseOrder, "", cSortDataType);
            var oIterator = oNavigator.Select(oExpression);

            while (oIterator.MoveNext())
                oElmt.InnerXml += oIterator.Current.OuterXml;

            return oElmt.ChildNodes;
        }
        catch (Exception)
        {
            return oElmt.ChildNodes;
        }
    }

    public enum dataType
    {

        TypeString = 1,
        TypeNumber = 2,
        TypeDate = 3

    }

    public static string xmlToForm(string sString)
    {
        //string sProcessInfo = "";

        try
        {
            sString = sString.Replace("<", "&lt;");
            sString = sString.Replace(">", "&gt;");
            return sString + "";
        }
        catch
        {
            return "";
        }

    }

    public static XmlDocument htmlToXmlDoc(string shtml)
    {
        var oXmlDoc = new XmlDocument();
       // string sProcessInfo = "";

        try
        {

            shtml = shtml.Replace("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">", "");
            shtml = shtml.Replace(" xmlns = \"http://www.w3.org/1999/xhtml\"", "");
            shtml = shtml.Replace(" xmlns=\"http://www.w3.org/1999/xhtml\"", "");
            shtml = shtml.Replace(" xml:lang=\"\"", "");
            oXmlDoc.LoadXml(shtml);

            return oXmlDoc;
        }
        catch (Exception)
        {
            // It is the desired behaviour for this to return nothing if not valid html don't turn this on apart from in development.
            // If gbDebug Then
            // returnException("xmlTools", "htmlToXmlDoc", ex, "", sProcessInfo)
            // End If
            return null;
        }
    }

    public static string convertEntitiesToCodes(string sString)
    {
        return Protean.Tools.Xml.convertEntitiesToCodes(sString);

    }

    public static string tidyXhtmlFrag(string shtml, bool bReturnNumbericEntities = false, bool bEncloseText = true, string removeTags = "")
    {

        // PerfMon.Log("Web", "tidyXhtmlFrag")
        //string sProcessInfo = "tidyXhtmlFrag";
        //string sTidyXhtml = "";

        try
        {

            return Protean.Tools.Text.tidyXhtmlFrag(shtml, bReturnNumbericEntities, bEncloseText, removeTags);
        }

        catch (Exception)
        {
            // It is the desired behaviour for this to return nothing if not valid html don't turn this on apart from in development.            Return Nothing
            // Return ex.Message
            return null;
        }
    }

    // Public Function tidyXhtmlFragOld(ByVal shtml As String, Optional ByVal bReturnNumbericEntities As Boolean = False, Optional ByVal bEncloseText As Boolean = True, Optional ByVal removeTags As String = "") As String

    // '   PerfMon.Log("Web", "tidyXhtmlFrag")
    // Dim sProcessInfo As String = "tidyXhtmlFrag"
    // Dim sTidyXhtml As String = ""
    // Dim oTdy As TidyNet.Tidy = New TidyNet.Tidy
    // Dim oTmc As New TidyNet.TidyMessageCollection
    // Dim msIn As MemoryStream = New MemoryStream
    // Dim msOut As MemoryStream = New MemoryStream
    // Dim aBytes As Byte()

    // Try

    // If Not removeTags = "" Then
    // shtml = removeTagFromXml(shtml, removeTags)
    // End If

    // oTdy.Options.MakeClean = True
    // oTdy.Options.DropFontTags = True
    // oTdy.Options.EncloseText = bEncloseText
    // oTdy.Options.Xhtml = True
    // oTdy.Options.SmartIndent = True
    // oTdy.Options.LiteralAttribs = False
    // 'oTdy.Options.XmlOut = True
    // 'oTdy.Options.XmlTags = True

    // If bReturnNumbericEntities Then
    // oTdy.Options.NumEntities = True
    // End If

    // aBytes = Text.Encoding.UTF8.GetBytes(shtml)
    // msIn.Write(aBytes, 0, aBytes.Length)
    // msIn.Position = 0
    // oTdy.Parse(msIn, msOut, oTmc)

    // sTidyXhtml = Text.Encoding.UTF8.GetString(msOut.ToArray())
    // Dim isError As Boolean = False
    // Dim errorhtml As String = ""
    // errorhtml &= "<div class=""XhtmlError"">"
    // Dim msg As TidyNet.TidyMessage
    // For Each msg In oTmc
    // Dim level As String = ""
    // Select Case msg.Level
    // Case TidyNet.MessageLevel.Error
    // level = "Error"
    // isError = True
    // Case TidyNet.MessageLevel.Warning
    // level = "Warning"
    // Case TidyNet.MessageLevel.Info
    // level = "Info"
    // End Select
    // errorhtml &= "<h3>" & level & " at line" & msg.Line & " column " & msg.Column & "</h3>"
    // errorhtml &= "<div>" & Replace(Replace(msg.Message, ">", "&gt;"), "<", "&lt;") & "</div>"
    // Next
    // errorhtml &= "</div>"
    // msg = Nothing

    // If isError = True Then

    // '  Dim myGumbo As New GumboWrapper()


    // sTidyXhtml = errorhtml
    // Else
    // 'now we need to strip out everything before and after the body tags
    // If sTidyXhtml.Contains("<body>") Then
    // sTidyXhtml = sTidyXhtml.Substring(sTidyXhtml.IndexOf("<body>") + 6)
    // sTidyXhtml = sTidyXhtml.Substring(0, sTidyXhtml.IndexOf("</body>") - 1)
    // ' Remove the leading and trailing whitespace.
    // sTidyXhtml = sTidyXhtml.Trim()
    // End If
    // End If

    // Return sTidyXhtml

    // Catch ex As Exception
    // ' It is the desired behaviour for this to return nothing if not valid html don't turn this on apart from in development.            Return Nothing
    // Return Nothing
    // Finally

    // aBytes = Nothing
    // msIn.Dispose()
    // msIn = Nothing
    // msOut.Dispose()

    // msOut = Nothing
    // oTmc = Nothing

    // oTdy = Nothing
    // sTidyXhtml = Nothing
    // End Try
    // End Function

    public static string removeTagFromXml(string xmlString, string tagNames)
    {

        tagNames = tagNames.Replace(" ", "");
        tagNames = tagNames.Replace(",", "|");

        xmlString = Regex.Replace(xmlString, "<[/]?(" + tagNames + @":\w+)[^>]*?>", "", RegexOptions.IgnoreCase);

        return xmlString;

    }

    public static XmlNamespaceManager getNsMgr(ref XmlNode oNode, ref XmlDocument oXml)
    {

        return Protean.Tools.Xml.getNsMgr(ref oNode, ref oXml);

    }

    public static string addNsToXpath(string cXpath, ref XmlNamespaceManager nsMgr)
    {

        return Protean.Tools.Xml.addNsToXpath(cXpath, ref nsMgr);

    }

    // Public Function addElement(ByRef oParent As XmlElement, ByVal cNewNodeName As String, Optional ByVal cNewNodeValue As String = "", Optional ByVal bAddAsXml As Boolean = False, Optional ByVal bPrepend As Boolean = False, Optional ByRef oNodeFromXPath As XmlNode = Nothing) As XmlElement
    // Dim parent As XmlNode = oParent
    // Return addElement(parent, cNewNodeName, cNewNodeValue, bAddAsXml, bPrepend, oNodeFromXPath)
    // End Function

    public static XmlElement addElement(ref XmlNode oParent, string cNewNodeName, string cNewNodeValue = "", bool bAddAsXml = false, bool bPrepend = false, [Optional, DefaultParameterValue(null)] ref XmlNode oNodeFromXPath)
    {
        XmlElement oNewNode;
        oNewNode = oParent.OwnerDocument.CreateElement(cNewNodeName);
        if (string.IsNullOrEmpty(cNewNodeValue) & oNodeFromXPath != null)
        {
            if (bAddAsXml)
                cNewNodeValue = oNodeFromXPath.InnerXml;
            else
                cNewNodeValue = oNodeFromXPath.InnerText;
        }
        if (!string.IsNullOrEmpty(cNewNodeValue))
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

    public static XmlElement addNewTextNode(string cNodeName, ref XmlNode oNode, string cNodeValue = "", bool bAppendNotPrepend = true, bool bOverwriteExistingNode = false)
    {
        try
        {


            XmlElement oElem;

            if (bOverwriteExistingNode)
            {
                // Search for an existing node - delete it if it exists
                oElem = (XmlElement)oNode.SelectSingleNode(cNodeName);
                if (oElem != null)
                    oNode.RemoveChild(oElem);
            }

            oElem = oNode.OwnerDocument.CreateElement(cNodeName);
            if (!string.IsNullOrEmpty(cNodeValue))
                oElem.InnerText = cNodeValue;
            if (bAppendNotPrepend)
            {
                oNode.AppendChild(oElem);
            }
            else
            {
                oNode.PrependChild(oElem);
            }

            return oElem;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Adding" + cNodeName, ex);
            //return null;
        }
    }

    public static string getXpathFromQueryXml(XmlElement oInstance, string sXsltPath = "")
    {


        var oTransform = new XmlHelper.Transform();

        TextWriter sWriter = new StringWriter();

        if (!string.IsNullOrEmpty(sXsltPath))
        {
            oTransform.Compiled = false;
            oTransform.XSLFile = goServer.MapPath(sXsltPath);
            var oXML = new XmlDocument();
            oXML.InnerXml = oInstance.OuterXml;
            oTransform.Process(oXML, sWriter);
            // Run transformation


            oInstance.InnerXml = sWriter.ToString();
            sWriter.Close();
            sWriter = null;

        }

        // build an xpath query based on the xform instance
        // Example:

        // <instance>
        // <Query ewCmd="xpathSearch" xPathMask="//*[VARating>$RequiredVA and Uptime>$Uptime and Format=$Format]">
        // <RequiredVA>100</RequiredVA>
        // <Redundancy>Yes</Redundancy>
        // <Uptime>20</Uptime>
        // <Format>Rack</Format>
        // </Query>
        // </instance>

        string sXpath;

        // :TODO [TS] handle the instance tranform if xsl provided

        sXpath = oInstance.SelectSingleNode("Query/@xPathMask").InnerText;
        foreach (XmlElement oElmt in oInstance.SelectNodes("Query/*"))
            // step through each of the child nodes and replace values in the xPathMask
            sXpath = Strings.Replace(sXpath, "$" + oElmt.Name, "\"" + oElmt.InnerText + "\"");

        return sXpath;

    }

    public static object getNodeValueByType(ref XmlNode oParent, string cXPath, dataType nNodeType = dataType.TypeString, object vDefaultValue = "")
    {

        XmlNode oNode;
        object cValue;
        oNode = oParent.SelectSingleNode(cXPath);
        if (oNode is null)
        {
            // No xPath match, let's return a default value
            // If there are no default values, then let's return a nominal default value
            switch (nNodeType)
            {
                case dataType.TypeNumber:
                    {
                        cValue = Interaction.IIf(Information.IsNumeric(vDefaultValue), vDefaultValue, 0);
                        break;
                    }
                case dataType.TypeDate:
                    {
                        cValue = Interaction.IIf(Information.IsDate(vDefaultValue), vDefaultValue, DateTime.Now); // Assume default type is string
                        break;
                    }

                default:
                    {
                        cValue = Interaction.IIf(Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(vDefaultValue, "", false)), vDefaultValue.ToString(), "");
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
                case dataType.TypeNumber:
                    {
                        cValue = Interaction.IIf(Information.IsNumeric(cValue), cValue, Interaction.IIf(Information.IsNumeric(vDefaultValue), vDefaultValue, 0));
                        break;
                    }
                case dataType.TypeDate:
                    {
                        cValue = Interaction.IIf(Information.IsDate(cValue), cValue, Interaction.IIf(Information.IsDate(vDefaultValue), vDefaultValue, DateTime.Now)); // Assume default type is string
                        break;
                    }

                default:
                    {
                        cValue = Interaction.IIf(Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(cValue, "", false)), cValue.ToString(), Interaction.IIf(Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(vDefaultValue, "", false)), vDefaultValue.ToString(), ""));
                        break;
                    }
            }
        }

        return cValue;
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

    public partial class XmlNoNamespaceWriter : XmlTextWriter
    {

        private bool skipAttribute = false;

        public XmlNoNamespaceWriter(TextWriter writer) : base(writer)
        {
        }

        public XmlNoNamespaceWriter(Stream stream, Encoding encoding) : base(stream, encoding)
        {
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            base.WriteStartElement(null, localName, null);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            if (prefix.CompareTo("xmlns") == 0 || localName.CompareTo("xmlns") == 0)
            {
                skipAttribute = true;
            }
            else
            {
                base.WriteStartAttribute(null, localName, null);
            }
        }

        public override void WriteString(string text)
        {
            if (!skipAttribute)
            {
                base.WriteString(text);
            }
        }

        public override void WriteEndAttribute()
        {
            if (!skipAttribute)
            {
                base.WriteEndAttribute();
            }
            skipAttribute = false;
        }

        public override void WriteQualifiedName(string localName, string ns)
        {
            base.WriteQualifiedName(localName, null);
        }
    }

}

public partial class XmlHelper
{

    public partial class Transform
    {

        public Cms myWeb;
        private string msXslFile = "";
        private string msXslLastFile = "";
        private bool mbCompiled = false;
        private long mnTimeoutSec = 20000L;
        private bool bXSLFileIsPath = true;
        public bool mbDebug = false;
        public Exception transformException;
        public string AssemblyPath;
        public string ClassName;
        private System.Xml.Xsl.XslTransform oStyle;
        private System.Xml.Xsl.XslCompiledTransform oCStyle;
        private bool bFinished = false;
        public bool bError = false;
        public Exception currentError;
        public System.Xml.Xsl.XsltArgumentList xsltArgs;
        private string compiledFolder = @"\xsltc\";
        public AppDomain xsltDomain;

        public string XslFilePath
        {
            get
            {
                return msXslFile;
            }
            set
            {
                try
                {
                    if (goApp("XsltCompileVersion") is null)
                    {
                        goApp("XsltCompileVersion") = "0";
                    }
                    string sCompileVersion = "v" + goApp("XsltCompileVersion");
                    msXslFile = value.Replace("/", @"\");
                    ClassName = msXslFile.Substring(msXslFile.LastIndexOf(@"\") + 1);
                    ClassName = ClassName.Replace(".", "_") + sCompileVersion;
                    if (mbCompiled)
                    {

                        Assembly assemblyInstance = null;
                        AssemblyPath = goServer.MapPath(compiledFolder) + ClassName + ".dll";
                        Type CalledType;

                        if (goApp(ClassName))
                        {
                            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
                            {
                                if (ass.GetName().ToString().StartsWith(ClassName))
                                {
                                    assemblyInstance = ass;
                                }
                            }
                            if (assemblyInstance is null)
                            {
                                assemblyInstance = Assembly.LoadFrom(AssemblyPath);
                            }
                        }
                        else if (File.Exists(AssemblyPath))
                        {
                            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                            assemblyInstance = Assembly.LoadFrom(AssemblyPath);
                            if (assemblyInstance is not null)
                            {
                                goApp(ClassName) = true;
                            }
                        }

                        else
                        {
                            string compileResponse = CompileXSLTassembly(ClassName);
                            if ((compileResponse ?? "") == (ClassName ?? ""))
                            {
                                // Dim assemblyBuffer As Byte() = File.ReadAllBytes(assemblypath)
                                // assemblyInstance = xsltDomain.Load(assemblyBuffer)
                                assemblyInstance = Assembly.LoadFrom(AssemblyPath);
                            }
                            else
                            {
                                Information.Err().Raise(8000, msXslFile, compileResponse);
                                assemblyInstance = null;
                            }
                        }

                        CalledType = assemblyInstance.GetType(ClassName, true);

                        oCStyle = new System.Xml.Xsl.XslCompiledTransform(mbDebug);
                        var resolver = new XmlUrlResolver();
                        resolver.Credentials = System.Net.CredentialCache.DefaultCredentials;
                        oCStyle.Load(CalledType);
                    }

                    // the old method is quicker for realtime loading of xslt
                    else if ((msXslLastFile ?? "") != (msXslFile ?? "") & oStyle is null)
                    {
                        // modification to allow for XSL to only be loaded once.
                        oStyle = new System.Xml.Xsl.XslTransform();
                        if (!string.IsNullOrEmpty(msXslFile))
                        {
                            oStyle.Load(msXslFile);
                        }
                        msXslLastFile = msXslFile;
                    }
                }
                catch (Exception ex)
                {

                    transformException = ex;
                    returnException(myWeb.msException, "Protean.XmlHelper.Transform", "XslFilePath.Set", ex, msXslFile, value, gbDebug);
                    bError = true;
                }
            }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly assembly, objExecutingAssemblies;
            string strTempAsmbPath = "";
            try
            {
                objExecutingAssemblies = args.RequestingAssembly;
                if (objExecutingAssemblies != default)
                {

                    AssemblyName[] arrReferencedAssmbNames = objExecutingAssemblies.GetReferencedAssemblies();

                    foreach (AssemblyName strAssmbName in arrReferencedAssmbNames)
                    {
                        if ((strAssmbName.FullName.Substring(0, strAssmbName.FullName.IndexOf(",")) ?? "") == (args.Name.Substring(0, args.Name.IndexOf(",")) ?? ""))
                        {
                            strTempAsmbPath = goServer.MapPath(compiledFolder) + (@"\" + args.Name.Substring(0, args.Name.IndexOf(","))) + ".dll";
                            break;
                        }
                    }

                    assembly = Assembly.LoadFrom(strTempAsmbPath);
                    return assembly;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                transformException = ex;
                // returnException(myWeb.msException, "Protean.XmlHelper.Transform", "CurrentDomain_AssemblyResolve", ex, msXslFile, Nothing, gbDebug)
                bError = true;
                return null;
            }

        }

        public string XSLFile
        {
            get
            {
                return msXslFile;
            }
            set
            {
                try
                {
                    msXslFile = value;
                    if (bXSLFileIsPath)
                    {
                        XslFilePath = value;
                    }
                    else
                    {
                        msXslFile = value;
                        oStyle = new System.Xml.Xsl.XslTransform();
                        var oXSL = new XmlDocument();
                        oXSL.InnerXml = msXslFile.Trim();
                        oStyle.Load(oXSL);
                    }
                }
                catch (Exception ex)
                {
                    transformException = ex;
                    returnException(myWeb.msException, "Protean.XmlHelper.Transform", "XSLFile.Set", ex, msXslFile, value);
                    bError = true;
                }
            }
        }

        public bool Compiled
        {
            get
            {
                return mbCompiled;
            }
            set
            {
                mbCompiled = value;
            }
        }

        public long TimeOut
        {
            get
            {
                return mnTimeoutSec;
            }
            set
            {
                mnTimeoutSec = value;
            }
        }

        private bool CanProcess
        {
            get
            {
                if (string.IsNullOrEmpty(msXslFile))
                    return false;
                else
                    return true;
            }
        }

        public bool XSLFileIsPath
        {
            get
            {
                return bXSLFileIsPath;
            }
            set
            {
                bXSLFileIsPath = value;
            }
        }

        public bool HasError
        {
            get
            {
                return bError;
            }
        }

        public Transform()
        {
            string sProcessInfo = "";
            try
            {
                myWeb = (object)null;
                xsltArgs = new System.Xml.Xsl.XsltArgumentList();
                var ewXsltExt = new xsltExtensions();
                xsltArgs.AddExtensionObject("urn:ew", ewXsltExt);
            }
            catch (Exception ex)
            {
                transformException = ex;
                stdTools.returnException(myWeb.msException, "Protean.XmlHelper.Transform", "New", ex, msXslFile, sProcessInfo, mbDebug);
                bError = true;
            }
        }

        public Transform(ref Cms aWeb, string sXslFile, bool bCompiled, long nTimeoutSec = 15000L, bool recompile = false)
        {
            string sProcessInfo = "";
            try
            {
                myWeb = aWeb;
                mbCompiled = bCompiled;
                if (myWeb.moConfig("ProjectPath") != "")
                {
                    compiledFolder = myWeb.moConfig("ProjectPath") + compiledFolder;
                }

                // If Not goApp("ewStarted") = True Then
                // 'If Not goApp("xsltDomain") Is Nothing Then
                // '    Dim xsltDomain As AppDomain = goApp("xsltDomain")
                // '    AppDomain.Unload(xsltDomain)
                // 'End If

                if (recompile)
                {

                    goApp("XsltCompileVersion") = (goApp("XsltCompileVersion") + 1).ToString();

                }

                // goApp("ewStarted") = True
                // End If

                XslFilePath = sXslFile;

                string className = msXslFile.Substring(msXslFile.LastIndexOf(@"\") + 1);
                className = className.Replace(".", "_");

                if (mbCompiled == true & goApp.Item(className) is null)
                {
                    mnTimeoutSec = 60000L;
                }
                else
                {
                    mnTimeoutSec = nTimeoutSec;
                }

                xsltArgs = new System.Xml.Xsl.XsltArgumentList();
                var ewXsltExt = new xsltExtensions(myWeb);
                xsltArgs.AddExtensionObject("urn:ew", ewXsltExt);
            }

            catch (Exception ex)
            {
                transformException = ex;
                returnException(myWeb.msException, "Protean.XmlHelper.Transform", "New", ex, msXslFile, sProcessInfo, mbDebug);
                bError = true;
            }
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            string sProcessInfo = "";
            try
            {
                // AppDomain.Unload(xsltDomain)
                oStyle = null;
                oCStyle = null;
                xsltArgs = null;
            }
            catch (Exception ex)
            {
                stdTools.returnException(myWeb.msException, "Protean.XmlHelper.Transform", "Dispose", ex, msXslFile, sProcessInfo, mbDebug);

            }
        }


        public delegate void ProcessDelegate(XmlDocument oXml, HttpResponse oResponse);
        public delegate void ProcessDelegate2(XmlDocument oXml, ref TextWriter oWriter);
        public delegate XmlDocument ProcessDelegateDocument(XmlDocument oXml);

        public void ProcessTimed(XmlDocument oXml, ref HttpResponse oResponse)
        {

            string sProcessInfo = "";
            try
            {
                var d = new ProcessDelegate(Process);
                var res = d.BeginInvoke(oXml, oResponse, default, default);
                if (res.IsCompleted == false)
                {
                    res.AsyncWaitHandle.WaitOne((int)mnTimeoutSec, false);
                    if (res.IsCompleted == false)
                    {
                        d.EndInvoke((System.Runtime.Remoting.Messaging.AsyncResult)res);
                        d = null;
                        Information.Err().Raise(1010, "TranformXSL", "The XSL took longer than " + mnTimeoutSec / 1000d + " seconds to process");
                        bError = true;
                    }
                }
                d.EndInvoke((System.Runtime.Remoting.Messaging.AsyncResult)res);
            }
            catch (Exception ex)
            {
                stdTools.returnException(ref myWeb.msException, "Protean.XmlHelper.TransformTimed", "Process", ex, msXslFile, sProcessInfo, mbDebug);
                oResponse.Write(myWeb.msException);
                bError = true;
            }
        }

        public void ProcessTimed(XmlDocument oXml, TextWriter oWriter)
        {

            string sProcessInfo = "";
            try
            {
                var d = new ProcessDelegate2(Process);
                var res = d.BeginInvoke(oXml, ref oWriter, null, null);
                if (res.IsCompleted == false)
                {
                    res.AsyncWaitHandle.WaitOne((int)mnTimeoutSec, false);
                    if (res.IsCompleted == false)
                    {
                        d.EndInvoke(ref oWriter, (System.Runtime.Remoting.Messaging.AsyncResult)res);
                        d = null;
                       // Information.Err().Raise(1010, "TranformXSL", "The XSL took longer than " + mnTimeoutSec / 1000d + " seconds to process");
                        bError = true;
                    }
                }
                d.EndInvoke(ref oWriter, (System.Runtime.Remoting.Messaging.AsyncResult)res);
            }
            catch (Exception ex)
            {
                stdTools.returnException(ref myWeb.msException, "Protean.XmlHelper.TransformTimed", "Process", ex, msXslFile, sProcessInfo);
                oWriter.Write(myWeb.msException);
                bError = true;
            }
        }

        public XmlDocument ProcessTimedDocument(XmlDocument oXml)
        {
            if (!CanProcess)
                return null;
            string sProcessInfo = "";
            TextWriter oWriter = new StringWriter();
            try
            {
                var d = new ProcessDelegate2(Process);

                var res = d.BeginInvoke(oXml, ref oWriter, null, null);
                if (res.IsCompleted == false)
                {
                    res.AsyncWaitHandle.WaitOne((int)mnTimeoutSec, false);
                    if (res.IsCompleted == false)
                    {

                        d.EndInvoke(ref oWriter, (System.Runtime.Remoting.Messaging.AsyncResult)res);
                        d = null;
                        Information.Err().Raise(1010, "TranformXSL", "The XSL took longer than " + mnTimeoutSec / 1000d + " seconds to process");
                    }
                }
                d.EndInvoke(ref oWriter, (System.Runtime.Remoting.Messaging.AsyncResult)res);
                var oXMLNew = new XmlDocument();
                oXml.InnerXml = oWriter.ToString();
                return oXml;
            }
            catch (Exception ex)
            {
                returnException(myWeb.msException, "Protean.XmlHelper.Transform", "Process", ex, msXslFile, sProcessInfo, mbDebug);
                oWriter.Write(myWeb.msException);
                bError = true;
                return null;
            }
        }
        public new void Process(XmlReader xReader, XmlWriter xWriter)
        {

            string sProcessInfo = "Processing:" + msXslFile;

            try
            {
                // If msException = "" Then
                var resolver = new XmlUrlResolver();
                resolver.Credentials = System.Net.CredentialCache.DefaultCredentials;
                if (oCStyle is null)
                {
                    oStyle.Transform((IXPathNavigable)xReader, xsltArgs, xWriter);
                }
                else
                {
                    oCStyle.Transform(xReader, xsltArgs, xWriter);
                }
            }
            // ' oCStyle.Transform(xReader, xsltArgs, xWriter) ', resolver)
            // Else
            // xWriter.Write(msException)
            // End If

            catch (Exception ex)
            {
                transformException = ex;
                returnException(myWeb.msException, "Protean.XmlHelper.Transform", "Process", ex, msXslFile, sProcessInfo, mbDebug);
                // oResponse.Write(msException)
                bError = true;
            }

        }


        public new void Process(XmlDocument oXml, HttpResponse oResponse)
        {
            if (!CanProcess)
                return;
            string sProcessInfo = "Processing:" + msXslFile;
            try
            {
                if (mbCompiled)
                {

                    var resolver = new XmlUrlResolver();
                    resolver.Credentials = System.Net.CredentialCache.DefaultCredentials;

                    var ws = oCStyle.OutputSettings.Clone();

                    // load the pagexml into a reader
                    var oReader = new XmlTextReader(new StringReader(oXml.OuterXml));
                    var sWriter = new StringWriter();

                    if (myWeb.msException == "")
                    {
                        // Run transformation

                        // Dim xsltDomainProxy As ProxyDomain = xsltDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, GetType(ProxyDomain).FullName)
                        // xsltDomainProxy._LocalContext = myWeb.moCtx
                        // Dim responseString As String = xsltDomainProxy.RunTransform(AssemblyPath, ClassName, oXml.OuterXml)
                        // oResponse.Write(responseString)

                        oCStyle.Transform(oReader, xsltArgs, XmlWriter.Create(oResponse.OutputStream, ws), resolver);
                    }
                    else
                    {
                        oResponse.Write(myWeb.msException);
                    }
                    oReader.Close();
                    sWriter.Dispose();
                }

                else
                {

                    // Change the xmlDocument to xPathDocument to improve performance
                    var oXmlNodeReader = new XmlNodeReader(oXml);
                    var xpathDoc = new XPathDocument(oXmlNodeReader);

                    if (myWeb is null)
                    {
                        oStyle.Transform(xpathDoc, xsltArgs, oResponse.OutputStream, default);
                    }
                    else if (myWeb.msException == "")
                    {
                        // Run transformation
                        oStyle.Transform(xpathDoc, xsltArgs, oResponse.OutputStream, default);
                    }
                    else
                    {
                        oResponse.Write(myWeb.msException);
                    }

                    oXmlNodeReader.Close();
                    xpathDoc = null;

                }
            }
            catch (Exception ex)
            {
                transformException = ex;
                returnException(myWeb.msException, "Protean.XmlHelper.Transform", "Process", ex, msXslFile, sProcessInfo, mbDebug);
                oResponse.Write(myWeb.msException);
                bError = true;
            }
        }

        public string stripNonValidXMLCharacters(string textIn)
        {
            var textOut = new StringBuilder();
            var textOuterr = new StringBuilder();
            // Used to hold the output.
            char current;
            // Used to reference the current character.
            int currenti;


            if (textIn is null || string.IsNullOrEmpty(textIn))
            {
                return string.Empty;
            }
            // vacancy test.
            for (int i = 0, loopTo = textIn.Length - 1; i <= loopTo; i++)
            {
                current = textIn[i];
                currenti = Strings.AscW(current);

                if (currenti == Conversions.ToInteger("&H9") || currenti == Conversions.ToInteger("&HA") || currenti == Conversions.ToInteger("&HD") || currenti >= Conversions.ToInteger("&H20") && currenti <= Conversions.ToInteger("&HD7FF") || currenti >= Conversions.ToInteger("&HE000") && currenti <= Conversions.ToInteger("&HFFFD") || currenti >= Conversions.ToInteger("&H10000") && currenti <= Conversions.ToInteger("&H10FFFF"))
                {
                    textOut.Append(current);
                }
                else
                {
                    textOuterr.Append(current);
                }
            }
            return textOut.ToString();
        }



        public new void Process(XmlDocument oXml, ref TextWriter oWriter)
        {
            if (!CanProcess)
                return;
            bError = false;
            string sProcessInfo = "Processing: " + msXslFile;
            try
            {
                if (mbCompiled)
                {
                    if (myWeb is null)
                    {
                        var oReader = new XmlTextReader(new StringReader(oXml.OuterXml));
                        var sWriter = new StringWriter();
                        oCStyle.Transform(oReader, xsltArgs, oWriter);
                    }
                    else if (myWeb.msException == "")
                    {
                        // Run transformation 
                        var oReader = new XmlTextReader(new StringReader(oXml.OuterXml));
                        var sWriter = new StringWriter();
                        oCStyle.Transform(oReader, xsltArgs, oWriter);
                    }
                    else
                    {
                        oWriter.Write(myWeb.msException);
                    }
                }
                else
                {
                    // Change the xmlDocument to xPathDocument to improve performance
                    var xpathDoc = new XPathDocument(new XmlNodeReader(oXml));
                    if (myWeb is null)
                    {
                        oStyle.Transform(xpathDoc, xsltArgs, oWriter, null);
                    }
                    else if (myWeb.msException == "" | myWeb.msException is null)
                    {
                        // Run transformation
                        oStyle.Transform(xpathDoc, xsltArgs, oWriter, null);
                    }
                    else
                    {
                        oWriter.Write(myWeb.msException);
                    }
                }
            }
            catch (Exception ex)
            {
                transformException = ex;
                if (myWeb is not null)
                {
                    returnException(myWeb.msException, "Protean.XmlHelper.Transform", "Process", ex, msXslFile, sProcessInfo, mbDebug);
                    oWriter.Write(myWeb.msException);
                }
                bError = true;
            }
        }

        public XmlDocument ProcessDocument(XmlDocument oXml)
        {
            if (!CanProcess)
                return null;
            string sProcessInfo = "Proceesing:" + msXslFile;
            TextWriter oWriter = new StringWriter();
            try
            {

                if (mbCompiled)
                {

                    System.Xml.Xsl.XslCompiledTransform oStyle;
                    // hear we cache the loaded xslt in the application object.                    
                    var resolver = new XmlUrlResolver();
                    resolver.Credentials = System.Net.CredentialCache.DefaultCredentials;

                    // here we store the stylesheet in the application object
                    if (goApp(msXslFile) is null)
                    {

                        // load the xslt
                        oStyle = new System.Xml.Xsl.XslCompiledTransform();
                        // this is the line that takes the time
                        oStyle.Load(msXslFile, System.Xml.Xsl.XsltSettings.TrustedXslt, resolver);
                        goApp.Add(msXslFile, oStyle);
                    }
                    else
                    {
                        // get the loaded xslt from the application variable
                        oStyle = goApp(msXslFile);
                    }

                    // add Eonic Bespoke Functions

                    var xsltArgs = new System.Xml.Xsl.XsltArgumentList();
                    var ewXsltExt = new xsltExtensions(myWeb);
                    xsltArgs.AddExtensionObject("urn:ew", ewXsltExt);


                    var ws = oStyle.OutputSettings.Clone();

                    // load the pagexml into a reader
                    var oReader = new XmlTextReader(new StringReader(oXml.OuterXml));
                    var sWriter = new StringWriter();

                    if (myWeb.msException == "")
                    {
                        // Run transformation
                        oStyle.Transform(oReader, xsltArgs, oWriter);
                    }
                    else
                    {
                        oWriter.Write(myWeb.msException);
                    }
                }

                else
                {
                    // the old method is quicker for realtime loading of xslt


                    // load the xslt
                    oStyle.Load(msXslFile);

                    // add Eonic Bespoke Functions
                    var xsltArgs = new System.Xml.Xsl.XsltArgumentList();
                    var ewXsltExt = new xsltExtensions(myWeb);
                    xsltArgs.AddExtensionObject("urn:ew", ewXsltExt);


                    // Change the xmlDocument to xPathDocument to improve performance
                    var xpathDoc = new XPathDocument(new XmlNodeReader(oXml));

                    if (myWeb.msException == "")
                    {
                        // Run transformation
                        oStyle.Transform(xpathDoc, xsltArgs, oWriter, null);
                    }
                    else
                    {
                        oWriter.Write(myWeb.msException);
                    }


                }
                var oXMLNew = new XmlDocument();
                oXml.InnerXml = oWriter.ToString();
                return oXml;
            }
            catch (Exception ex)
            {
                bError = true;
                currentError = ex;
                returnException(myWeb.msException, "Protean.XmlHelper.Transform", "ProcessDocument", ex, msXslFile, sProcessInfo, mbDebug);
                oWriter.Write(myWeb.msException);
                return null;
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr LoadLibrary([In()][MarshalAs(UnmanagedType.LPStr)] string lpFileName);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool FreeLibrary([In()] IntPtr hModule);


        public bool ClearXSLTassemblyCache()
        {
            string sProcessInfo = "ClearXSLTassemblyCache";
            try
            {

                Protean.Tools.Security.Impersonate oImp = default;
                if (myWeb.moConfig("AdminAcct") == "")
                {
                    oImp = new Protean.Tools.Security.Impersonate();
                    oImp.ImpersonateValidUser(myWeb.moConfig("AdminAcct"), myWeb.moConfig("AdminDomain"), myWeb.moConfig("AdminPassword"), true, myWeb.moConfig("AdminGroup"));
                }

                string cWorkingDirectory = goServer.MapPath(compiledFolder);
                sProcessInfo = "clearing " + cWorkingDirectory;
                var di = new DirectoryInfo(cWorkingDirectory);

                foreach (var fi in di.EnumerateFiles())
                {
                    try
                    {
                        var fso = new fsHelper();
                        fso.DeleteFile(fi.FullName);
                    }
                    catch (Exception ex2)
                    {
                        // returnException("Protean.XmlHelper.Transform", "ClearXSLTassemblyCache", ex2, msXslFile, sProcessInfo)
                    }
                }

                if (myWeb.moConfig("AdminAcct") == "")
                {
                    oImp.UndoImpersonation();
                }

                // reset config to on
                Protean.Config.UpdateConfigValue(myWeb, "protean/web", "CompliedTransform", "on");
            }

            // di.Delete(True)

            catch (Exception ex)
            {
                bError = true;
                returnException(myWeb.msException, "Protean.XmlHelper.Transform", "ClearXSLTassemblyCache", ex, msXslFile, sProcessInfo, mbDebug);
                return default;
            }

            return default;
        }


        public string CompileXSLTassembly(string classname)
        {

            string compilerPath = goServer.MapPath("/ewcommon/xsl/compiler/xsltc.exe");
            if (myWeb.bs5)
            {
                compilerPath = goServer.MapPath("/ptn/tools/compiler/xsltc.exe");
            }
            string xsltPath = "\"" + msXslFile + "\"";
            string sProcessInfo = "compiling: " + xsltPath;
            string outFile = classname + ".dll";
            string cmdLine = " /class:" + classname + " /out:" + outFile + " " + xsltPath;
            var process1 = new Process();
            string output = "";

            try
            {

                if (goApp("compileLock-" + classname) is null)
                {

                    goApp("compileLock-" + classname) = true;

                    process1.EnableRaisingEvents = true;
                    process1.StartInfo.FileName = compilerPath;
                    process1.StartInfo.Arguments = cmdLine;
                    process1.StartInfo.UseShellExecute = false;
                    process1.StartInfo.RedirectStandardOutput = true;
                    process1.StartInfo.RedirectStandardInput = true;
                    process1.StartInfo.RedirectStandardError = true;

                    // check if local bin exists
                    string cWorkingDirectory = goServer.MapPath(compiledFolder);
                    var di = new DirectoryInfo(cWorkingDirectory);
                    if (!di.Exists)
                    {
                        di.Create();
                    }

                    process1.StartInfo.WorkingDirectory = cWorkingDirectory;
                    // Start the process
                    process1.Start();
                    output = process1.StandardOutput.ReadToEnd();

                    // Wait for process to finish
                    process1.WaitForExit();

                    process1.Close();

                    goApp("compileLock-" + classname) = (object)null;

                }

                if (output.Contains("error"))
                {
                    return output;
                }
                else
                {
                    return classname;
                }
            }

            catch (Exception ex)
            {
                bError = true;
                returnException(myWeb.msException, "Protean.XmlHelper.Transform", "CompileXSLTassembly", ex, msXslFile, sProcessInfo, mbDebug);
                return null;
            }
        }


    }





    private partial class ProxyDomain : MarshalByRefObject
    {
        public HttpContext _LocalContext;

        public void GetAssembly(string AssemblyPath, string className)
        {
            try
            {
                var newAssem = Assembly.LoadFrom(AssemblyPath);
            }


            // If you want to do anything further to that assembly, you need to do it here.


            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
        }

        public string RunTransform(string AssemblyPath, string className, string PageXml)
        {
            try
            {
                HttpContext.Current = _LocalContext;

                var oReader = new XmlTextReader(new StringReader(PageXml));

                var xsltArgs = new System.Xml.Xsl.XsltArgumentList();
                var ewXsltExt = new xsltExtensions();
                xsltArgs.AddExtensionObject("urn:ew", ewXsltExt);

                var newAssem = Assembly.LoadFrom(AssemblyPath);
                var CalledType = newAssem.GetType(className, true);
                var oCStyle = new System.Xml.Xsl.XslCompiledTransform(true);
                var sWriter = new StringWriter();

                oCStyle.Load(CalledType);
                TextWriter oWriter = new StringWriter();

                oCStyle.Transform(oReader, xsltArgs, oWriter);

                return oWriter.ToString();
            }

            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message, ex);
            }
        }
    }


}
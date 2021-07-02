
Imports System.Xml
Imports System.Web
Imports System.IO
Imports System.Xml.XPath
Imports System.Text.RegularExpressions
Imports System.Reflection
Imports System.Runtime.InteropServices


Public Class Proxy
    Inherits MarshalByRefObject
    Public Function GetAssembly(assemblyPath As String) As Assembly
        Try
            Return Assembly.LoadFile(assemblyPath)
        Catch generatedExceptionName As Exception
            ' throw new InvalidOperationException(ex);
            Return Nothing
        End Try
    End Function
End Class

Partial Public Module xmlTools

    Function SortedNodeList_UNCONNECTED(ByVal oRootObject As XmlElement, ByVal cXPath As String, ByVal cSortItem As String, ByVal cSortOrder As XmlSortOrder, ByVal cSortDataType As XmlDataType, ByVal cCaseOrder As XmlCaseOrder) As XmlNodeList
        Dim oElmt As XmlElement = oRootObject.OwnerDocument.CreateElement("List")
        Try
            Dim oNavigator As XPathNavigator = oRootObject.CreateNavigator()
            Dim oExpression As XPathExpression = oNavigator.Compile(cXPath)
            oExpression.AddSort(cSortItem, cSortOrder, cCaseOrder, "", cSortDataType)
            Dim oIterator As XPathNodeIterator = oNavigator.Select(oExpression)

            Do Until Not oIterator.MoveNext
                oElmt.InnerXml &= oIterator.Current.OuterXml
            Loop

            Return oElmt.ChildNodes
        Catch ex As Exception
            Return oElmt.ChildNodes
        End Try
    End Function

    Enum dataType

        TypeString = 1
        TypeNumber = 2
        TypeDate = 3

    End Enum

    Function xmlToForm(ByVal sString As String) As String
        Dim sProcessInfo As String = ""

        Try
            sString = Replace(sString, "<", "&lt;")
            sString = Replace(sString, ">", "&gt;")
            Return sString & ""
        Catch
            Return ""
        End Try

    End Function

    Function htmlToXmlDoc(ByVal shtml As String) As XmlDocument
        Dim oXmlDoc As New XmlDocument
        Dim sProcessInfo As String = ""

        Try

            shtml = Replace(shtml, "<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.1//EN"" ""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"">", "")
            shtml = Replace(shtml, " xmlns = ""http://www.w3.org/1999/xhtml""", "")
            shtml = Replace(shtml, " xmlns=""http://www.w3.org/1999/xhtml""", "")
            shtml = Replace(shtml, " xml:lang=""""", "")
            oXmlDoc.LoadXml(shtml)

            Return oXmlDoc
        Catch ex As Exception
            ' It is the desired behaviour for this to return nothing if not valid html don't turn this on apart from in development.
            'If gbDebug Then
            '     returnException("xmlTools", "htmlToXmlDoc", ex, "", sProcessInfo)
            'End If
            Return Nothing
        End Try
    End Function

    Function convertEntitiesToCodes(ByVal sString As String) As String
        Return Protean.Tools.Xml.convertEntitiesToCodes(sString)

    End Function

    Public Function tidyXhtmlFrag(ByVal shtml As String, Optional ByVal bReturnNumbericEntities As Boolean = False, Optional ByVal bEncloseText As Boolean = True, Optional ByVal removeTags As String = "") As String

        '   PerfMon.Log("Web", "tidyXhtmlFrag")
        Dim sProcessInfo As String = "tidyXhtmlFrag"
        Dim sTidyXhtml As String = ""

        Try

            Return Protean.Tools.Text.tidyXhtmlFrag(shtml, bReturnNumbericEntities, bEncloseText, removeTags)

        Catch ex As Exception
            ' It is the desired behaviour for this to return nothing if not valid html don't turn this on apart from in development.            Return Nothing
            ' Return ex.Message
            Return Nothing
        End Try
    End Function

    'Public Function tidyXhtmlFragOld(ByVal shtml As String, Optional ByVal bReturnNumbericEntities As Boolean = False, Optional ByVal bEncloseText As Boolean = True, Optional ByVal removeTags As String = "") As String

    '    '   PerfMon.Log("Web", "tidyXhtmlFrag")
    '    Dim sProcessInfo As String = "tidyXhtmlFrag"
    '    Dim sTidyXhtml As String = ""
    '    Dim oTdy As TidyNet.Tidy = New TidyNet.Tidy
    '    Dim oTmc As New TidyNet.TidyMessageCollection
    '    Dim msIn As MemoryStream = New MemoryStream
    '    Dim msOut As MemoryStream = New MemoryStream
    '    Dim aBytes As Byte()

    '    Try

    '        If Not removeTags = "" Then
    '            shtml = removeTagFromXml(shtml, removeTags)
    '        End If

    '        oTdy.Options.MakeClean = True
    '        oTdy.Options.DropFontTags = True
    '        oTdy.Options.EncloseText = bEncloseText
    '        oTdy.Options.Xhtml = True
    '        oTdy.Options.SmartIndent = True
    '        oTdy.Options.LiteralAttribs = False
    '        'oTdy.Options.XmlOut = True
    '        'oTdy.Options.XmlTags = True

    '        If bReturnNumbericEntities Then
    '            oTdy.Options.NumEntities = True
    '        End If

    '        aBytes = Text.Encoding.UTF8.GetBytes(shtml)
    '        msIn.Write(aBytes, 0, aBytes.Length)
    '        msIn.Position = 0
    '        oTdy.Parse(msIn, msOut, oTmc)

    '        sTidyXhtml = Text.Encoding.UTF8.GetString(msOut.ToArray())
    '        Dim isError As Boolean = False
    '        Dim errorhtml As String = ""
    '        errorhtml &= "<div class=""XhtmlError"">"
    '        Dim msg As TidyNet.TidyMessage
    '        For Each msg In oTmc
    '            Dim level As String = ""
    '            Select Case msg.Level
    '                Case TidyNet.MessageLevel.Error
    '                    level = "Error"
    '                    isError = True
    '                Case TidyNet.MessageLevel.Warning
    '                    level = "Warning"
    '                Case TidyNet.MessageLevel.Info
    '                    level = "Info"
    '            End Select
    '            errorhtml &= "<h3>" & level & " at line" & msg.Line & " column " & msg.Column & "</h3>"
    '            errorhtml &= "<div>" & Replace(Replace(msg.Message, ">", "&gt;"), "<", "&lt;") & "</div>"
    '        Next
    '        errorhtml &= "</div>"
    '        msg = Nothing

    '        If isError = True Then

    '            '  Dim myGumbo As New GumboWrapper()


    '            sTidyXhtml = errorhtml
    '        Else
    '            'now we need to strip out everything before and after the body tags
    '            If sTidyXhtml.Contains("<body>") Then
    '                sTidyXhtml = sTidyXhtml.Substring(sTidyXhtml.IndexOf("<body>") + 6)
    '                sTidyXhtml = sTidyXhtml.Substring(0, sTidyXhtml.IndexOf("</body>") - 1)
    '                ' Remove the leading and trailing whitespace.
    '                sTidyXhtml = sTidyXhtml.Trim()
    '            End If
    '        End If

    '        Return sTidyXhtml

    '    Catch ex As Exception
    '        ' It is the desired behaviour for this to return nothing if not valid html don't turn this on apart from in development.            Return Nothing
    '        Return Nothing
    '    Finally

    '        aBytes = Nothing
    '        msIn.Dispose()
    '        msIn = Nothing
    '        msOut.Dispose()

    '        msOut = Nothing
    '        oTmc = Nothing

    '        oTdy = Nothing
    '        sTidyXhtml = Nothing
    '    End Try
    'End Function

    Public Function removeTagFromXml(ByVal xmlString As String, ByVal tagNames As String) As String

        tagNames = Replace(tagNames, " ", "")
        tagNames = Replace(tagNames, ",", "|")

        xmlString = Regex.Replace(xmlString, "<[/]?(" & tagNames & ":\w+)[^>]*?>", "", RegexOptions.IgnoreCase)

        Return xmlString

    End Function

    Public Function getNsMgr(ByRef oNode As XmlNode, ByRef oXml As XmlDocument) As XmlNamespaceManager

        Return Protean.Tools.Xml.getNsMgr(oNode, oXml)

    End Function

    Public Function addNsToXpath(ByVal cXpath As String, ByRef nsMgr As XmlNamespaceManager) As String

        Return Protean.Tools.Xml.addNsToXpath(cXpath, nsMgr)

    End Function

    'Public Function addElement(ByRef oParent As XmlElement, ByVal cNewNodeName As String, Optional ByVal cNewNodeValue As String = "", Optional ByVal bAddAsXml As Boolean = False, Optional ByVal bPrepend As Boolean = False, Optional ByRef oNodeFromXPath As XmlNode = Nothing) As XmlElement
    '    Dim parent As XmlNode = oParent
    '    Return addElement(parent, cNewNodeName, cNewNodeValue, bAddAsXml, bPrepend, oNodeFromXPath)
    'End Function

    Public Function addElement(ByRef oParent As XmlNode, ByVal cNewNodeName As String, Optional ByVal cNewNodeValue As String = "", Optional ByVal bAddAsXml As Boolean = False, Optional ByVal bPrepend As Boolean = False, Optional ByRef oNodeFromXPath As XmlNode = Nothing) As XmlElement
        Dim oNewNode As XmlElement
        oNewNode = oParent.OwnerDocument.CreateElement(cNewNodeName)
        If cNewNodeValue = "" And Not (oNodeFromXPath Is Nothing) Then
            If bAddAsXml Then cNewNodeValue = oNodeFromXPath.InnerXml Else cNewNodeValue = oNodeFromXPath.InnerText
        End If
        If cNewNodeValue <> "" Then
            If bAddAsXml Then oNewNode.InnerXml = cNewNodeValue Else oNewNode.InnerText = cNewNodeValue
        End If
        If bPrepend Then oParent.PrependChild(oNewNode) Else oParent.AppendChild(oNewNode)
        Return oNewNode
    End Function

    Public Function addNewTextNode(ByVal cNodeName As String, ByRef oNode As XmlNode, Optional ByVal cNodeValue As String = "", Optional ByVal bAppendNotPrepend As Boolean = True, Optional ByVal bOverwriteExistingNode As Boolean = False) As XmlElement
        Try


            Dim oElem As XmlElement

            If bOverwriteExistingNode Then
                ' Search for an existing node - delete it if it exists
                oElem = oNode.SelectSingleNode(cNodeName)
                If Not (oElem Is Nothing) Then oNode.RemoveChild(oElem)
            End If

            oElem = oNode.OwnerDocument.CreateElement(cNodeName)
            If cNodeValue <> "" Then oElem.InnerText = cNodeValue
            If bAppendNotPrepend Then
                oNode.AppendChild(oElem)
            Else
                oNode.PrependChild(oElem)
            End If

            Return oElem
        Catch ex As Exception
            Err.Raise(8000, "Adding" & cNodeName, ex.Message)
        End Try
    End Function

    Public Function getXpathFromQueryXml(ByVal oInstance As XmlElement, Optional ByVal sXsltPath As String = "") As String


        Dim oTransform As New Protean.XmlHelper.Transform()

        Dim sWriter As IO.TextWriter = New IO.StringWriter

        If sXsltPath <> "" Then
            oTransform.Compiled = False
            oTransform.XSLFile = goServer.MapPath(sXsltPath)
            Dim oXML As New XmlDocument
            oXML.InnerXml = oInstance.OuterXml
            oTransform.Process(oXML, sWriter)
            'Run transformation


            oInstance.InnerXml = sWriter.ToString()
            sWriter.Close()
            sWriter = Nothing

        End If

        'build an xpath query based on the xform instance
        'Example:

        '<instance>
        '<Query ewCmd="xpathSearch" xPathMask="//*[VARating>$RequiredVA and Uptime>$Uptime and Format=$Format]">
        '<RequiredVA>100</RequiredVA>
        '<Redundancy>Yes</Redundancy>
        '<Uptime>20</Uptime>
        '<Format>Rack</Format>
        '</Query>
        '</instance>

        Dim sXpath As String

        ':TODO [TS] handle the instance tranform if xsl provided

        sXpath = oInstance.SelectSingleNode("Query/@xPathMask").InnerText
        Dim oElmt As XmlElement
        For Each oElmt In oInstance.SelectNodes("Query/*")
            'step through each of the child nodes and replace values in the xPathMask
            sXpath = Replace(sXpath, "$" & oElmt.Name, """" & oElmt.InnerText & """")
        Next

        Return sXpath

    End Function

    Public Function getNodeValueByType(ByRef oParent As XmlNode, ByVal cXPath As String, Optional ByVal nNodeType As dataType = dataType.TypeString, Optional ByVal vDefaultValue As Object = "") As Object

        Dim oNode As XmlNode
        Dim cValue As Object
        oNode = oParent.SelectSingleNode(cXPath)
        If oNode Is Nothing Then
            ' No xPath match, let's return a default value
            ' If there are no default values, then let's return a nominal default value
            Select Case nNodeType
                Case dataType.TypeNumber
                    cValue = IIf(IsNumeric(vDefaultValue), vDefaultValue, 0)
                Case dataType.TypeDate
                    cValue = IIf(IsDate(vDefaultValue), vDefaultValue, Now())
                Case Else ' Assume default type is string
                    cValue = IIf(vDefaultValue <> "", vDefaultValue.ToString, "")
            End Select
        Else
            ' XPath  match, let's check it against type
            cValue = oNode.InnerText
            Select Case nNodeType
                Case dataType.TypeNumber
                    cValue = IIf(IsNumeric(cValue), cValue, IIf(IsNumeric(vDefaultValue), vDefaultValue, 0))
                Case dataType.TypeDate
                    cValue = IIf(IsDate(cValue), cValue, IIf(IsDate(vDefaultValue), vDefaultValue, Now()))
                Case Else ' Assume default type is string
                    cValue = IIf(cValue <> "", cValue.ToString, IIf(vDefaultValue <> "", vDefaultValue.ToString, ""))
            End Select
        End If

        Return cValue
    End Function

    Public Function xmlToHashTable(ByVal oNodeList As XmlNodeList, Optional ByVal cNamedAttribute As String = "") As Hashtable

        ' This converts a nodelist to a HashTable
        ' The key will be the node name, the value will either be the Inner Text or a named attribute

        Dim oHT As New Hashtable
        Dim oElmt As XmlElement
        Dim cKey As String
        Dim cValue As String

        For Each oElmt In oNodeList
            cKey = oElmt.LocalName
            If cNamedAttribute <> "" Then
                cValue = oElmt.GetAttribute(cNamedAttribute)
            Else
                cValue = oElmt.InnerText
            End If
            oHT.Add(cKey, cValue)
        Next

        Return oHT

    End Function

    Public Function xmlToHashTable(ByVal oNode As XmlNode, Optional ByVal cNamedAttribute As String = "") As Hashtable

        ' This converts a node's child nodes to a HashTable
        Return xmlToHashTable(oNode.ChildNodes, cNamedAttribute)

    End Function

    Public Class XmlNoNamespaceWriter
        Inherits System.Xml.XmlTextWriter

        Dim skipAttribute As Boolean = False

        Public Sub New(ByVal writer As System.IO.TextWriter)
            MyBase.New(writer)
        End Sub

        Public Sub New(ByVal stream As System.IO.Stream, ByVal encoding As System.Text.Encoding)
            MyBase.New(stream, encoding)
        End Sub

        Public Overloads Overrides Sub WriteStartElement(ByVal prefix As String, ByVal localName As String, ByVal ns As String)
            MyBase.WriteStartElement(Nothing, localName, Nothing)
        End Sub

        Public Overloads Overrides Sub WriteStartAttribute(ByVal prefix As String, ByVal localName As String, ByVal ns As String)
            If prefix.CompareTo("xmlns") = 0 OrElse localName.CompareTo("xmlns") = 0 Then
                skipAttribute = True
            Else
                MyBase.WriteStartAttribute(Nothing, localName, Nothing)
            End If
        End Sub

        Public Overloads Overrides Sub WriteString(ByVal text As String)
            If Not skipAttribute Then
                MyBase.WriteString(text)
            End If
        End Sub

        Public Overloads Overrides Sub WriteEndAttribute()
            If Not skipAttribute Then
                MyBase.WriteEndAttribute()
            End If
            skipAttribute = False
        End Sub

        Public Overloads Overrides Sub WriteQualifiedName(ByVal localName As String, ByVal ns As String)
            MyBase.WriteQualifiedName(localName, Nothing)
        End Sub
    End Class

End Module

Public Class XmlHelper

    Class Transform

        Public myWeb As Cms
        Private msXslFile As String = ""
        Private msXslLastFile As String = ""
        Private mbCompiled As Boolean = False
        Private mnTimeoutSec As Long = 20000
        Private bXSLFileIsPath As Boolean = True
        Public mbDebug As Boolean = False
        Public transformException As Exception
        Public AssemblyPath As String
        Public ClassName As String
        Dim oStyle As Xsl.XslTransform
        Dim oCStyle As Xsl.XslCompiledTransform
        Dim bFinished As Boolean = False
        Public bError As Boolean = False
        Public currentError As Exception
        Public xsltArgs As Xsl.XsltArgumentList
        Dim compiledFolder As String = "\xsltc\"
        Public xsltDomain As AppDomain

        Public Property XslFilePath() As String
            Get
                Return msXslFile
            End Get
            Set(ByVal value As String)
                Try
                    If goApp("XsltCompileVersion") Is Nothing Then
                        goApp("XsltCompileVersion") = "0"
                    End If
                    Dim sCompileVersion As String = "v" & goApp("XsltCompileVersion")
                    msXslFile = value.Replace("/", "\")
                    ClassName = msXslFile.Substring(msXslFile.LastIndexOf("\") + 1)
                    ClassName = ClassName.Replace(".", "_") & sCompileVersion
                    If mbCompiled Then

                        Dim assemblyInstance As [Assembly]
                        AssemblyPath = goServer.MapPath(compiledFolder) & ClassName & ".dll"
                        Dim CalledType As System.Type

                        If goApp(ClassName) Then
                            Dim ass As Assembly
                            For Each ass In AppDomain.CurrentDomain.GetAssemblies
                                If ass.GetName.ToString().StartsWith(ClassName) Then
                                    assemblyInstance = ass
                                End If
                            Next
                            If assemblyInstance Is Nothing Then
                                assemblyInstance = [Assembly].LoadFrom(AssemblyPath)
                            End If
                        Else
                            If IO.File.Exists(AssemblyPath) Then
                                AddHandler AppDomain.CurrentDomain.AssemblyResolve, AddressOf CurrentDomain_AssemblyResolve
                                assemblyInstance = [Assembly].LoadFrom(AssemblyPath)
                                If assemblyInstance IsNot Nothing Then
                                    goApp(ClassName) = True
                                End If

                            Else
                                Dim compileResponse As String = CompileXSLTassembly(ClassName)
                                If compileResponse = ClassName Then
                                    'Dim assemblyBuffer As Byte() = File.ReadAllBytes(assemblypath)
                                    'assemblyInstance = xsltDomain.Load(assemblyBuffer)
                                    assemblyInstance = [Assembly].LoadFrom(AssemblyPath)
                                Else
                                    Err.Raise(8000, msXslFile, compileResponse)
                                    assemblyInstance = Nothing
                                End If
                            End If
                        End If

                        CalledType = assemblyInstance.GetType(ClassName, True)

                        oCStyle = New Xsl.XslCompiledTransform(mbDebug)
                        Dim resolver As New XmlUrlResolver()
                        resolver.Credentials = System.Net.CredentialCache.DefaultCredentials
                        oCStyle.Load(CalledType)

                    Else
                        'the old method is quicker for realtime loading of xslt
                        If msXslLastFile <> msXslFile And oStyle Is Nothing Then
                            'modification to allow for XSL to only be loaded once.
                            oStyle = New Xsl.XslTransform
                            If msXslFile <> "" Then
                                oStyle.Load(msXslFile)
                            End If
                            msXslLastFile = msXslFile
                        End If
                    End If
                Catch ex As Exception

                    'To be removed.
                    'Dim reflection As ReflectionTypeLoadException = TryCast(ex, ReflectionTypeLoadException)
                    'If reflection IsNot Nothing Then
                    '    For Each expct As Exception In reflection.LoaderExceptions
                    '        Using eventLog As New EventLog("Application")
                    '            eventLog.WriteEntry(expct.Message)
                    '            eventLog.WriteEntry(expct.InnerException.Message)
                    '        End Using
                    '    Next
                    'End If

                    transformException = ex
                    returnException(myWeb.msException, "Protean.XmlHelper.Transform", "XslFilePath.Set", ex, msXslFile, value, gbDebug)
                    bError = True
                End Try
            End Set
        End Property

        Private Function CurrentDomain_AssemblyResolve(ByVal sender As Object, ByVal args As ResolveEventArgs) As Assembly
            Dim assembly, objExecutingAssemblies As Assembly
            Dim strTempAsmbPath As String = ""
            objExecutingAssemblies = args.RequestingAssembly
            Dim arrReferencedAssmbNames As AssemblyName() = objExecutingAssemblies.GetReferencedAssemblies()

            For Each strAssmbName As AssemblyName In arrReferencedAssmbNames
                If strAssmbName.FullName.Substring(0, strAssmbName.FullName.IndexOf(",")) = args.Name.Substring(0, args.Name.IndexOf(",")) Then
                    strTempAsmbPath = goServer.MapPath(compiledFolder) & "\" + args.Name.Substring(0, args.Name.IndexOf(",")) & ".dll"
                    Exit For
                End If
            Next

            assembly = Assembly.LoadFrom(strTempAsmbPath)
            Return assembly
        End Function

        Public Property XSLFile() As String
            Get
                Return msXslFile
            End Get
            Set(ByVal value As String)
                Try
                    msXslFile = value
                    If Me.bXSLFileIsPath Then
                        XslFilePath = value
                    Else
                        msXslFile = value
                        oStyle = New Xsl.XslTransform
                        Dim oXSL As New XmlDocument
                        oXSL.InnerXml = msXslFile.Trim
                        oStyle.Load(oXSL)
                    End If
                Catch ex As Exception
                    transformException = ex
                    returnException(myWeb.msException, "Protean.XmlHelper.Transform", "XSLFile.Set", ex, msXslFile, value)
                    bError = True
                End Try
            End Set
        End Property

        Public Property Compiled() As Boolean
            Get
                Return mbCompiled
            End Get
            Set(ByVal value As Boolean)
                mbCompiled = value
            End Set
        End Property

        Public Property TimeOut() As Long
            Get
                Return mnTimeoutSec
            End Get
            Set(ByVal value As Long)
                mnTimeoutSec = value
            End Set
        End Property

        Private ReadOnly Property CanProcess() As Boolean
            Get
                If msXslFile = "" Then Return False Else Return True
            End Get
        End Property

        Public Property XSLFileIsPath() As Boolean
            Get
                Return bXSLFileIsPath
            End Get
            Set(ByVal value As Boolean)
                bXSLFileIsPath = value
            End Set
        End Property

        Public ReadOnly Property HasError() As Boolean
            Get
                Return bError
            End Get
        End Property

        Public Sub New()
            Dim sProcessInfo As String = ""
            Try
                myWeb = Nothing
                xsltArgs = New Xsl.XsltArgumentList
                Dim ewXsltExt As New xsltExtensions()
                xsltArgs.AddExtensionObject("urn:ew", ewXsltExt)
            Catch ex As Exception
                transformException = ex
                returnException(myWeb.msException, "Protean.XmlHelper.Transform", "New", ex, msXslFile, sProcessInfo, mbDebug)
                bError = True
            End Try
        End Sub

        Public Sub New(ByRef aWeb As Cms, ByVal sXslFile As String, ByVal bCompiled As Boolean, Optional ByVal nTimeoutSec As Long = 15000, Optional recompile As Boolean = False)
            Dim sProcessInfo As String = ""
            Try
                myWeb = aWeb
                mbCompiled = bCompiled
                If myWeb.moConfig("ProjectPath") <> "" Then
                    compiledFolder = myWeb.moConfig("ProjectPath") & compiledFolder
                End If

                'If Not goApp("ewStarted") = True Then
                '    'If Not goApp("xsltDomain") Is Nothing Then
                '    '    Dim xsltDomain As AppDomain = goApp("xsltDomain")
                '    '    AppDomain.Unload(xsltDomain)
                '    'End If

                If recompile Then

                    goApp("XsltCompileVersion") = CStr(CInt(goApp("XsltCompileVersion")) + 1)

                End If

                '    goApp("ewStarted") = True
                'End If

                XslFilePath = sXslFile

                Dim className As String = msXslFile.Substring(msXslFile.LastIndexOf("\") + 1)
                className = className.Replace(".", "_")

                If mbCompiled = True And goApp.Item(className) Is Nothing Then
                    mnTimeoutSec = 60000
                Else
                    mnTimeoutSec = nTimeoutSec
                End If

                xsltArgs = New Xsl.XsltArgumentList
                Dim ewXsltExt As New xsltExtensions(myWeb)
                xsltArgs.AddExtensionObject("urn:ew", ewXsltExt)

            Catch ex As Exception
                transformException = ex
                returnException(myWeb.msException, "Protean.XmlHelper.Transform", "New", ex, msXslFile, sProcessInfo, mbDebug)
                bError = True
            End Try
        End Sub

        Public Sub Close()
            Me.Dispose()
        End Sub

        Public Sub Dispose()
            Dim sProcessInfo As String = ""
            Try
                'AppDomain.Unload(xsltDomain)
                oStyle = Nothing
                oCStyle = Nothing
                xsltArgs = Nothing
            Catch ex As Exception
                returnException(myWeb.msException, "Protean.XmlHelper.Transform", "Dispose", ex, msXslFile, sProcessInfo, mbDebug)

            End Try
        End Sub


        Delegate Sub ProcessDelegate(ByVal oXml As XmlDocument, ByVal oResponse As HttpResponse)
        Delegate Sub ProcessDelegate2(ByVal oXml As XmlDocument, ByRef oWriter As IO.TextWriter)
        Delegate Function ProcessDelegateDocument(ByVal oXml As XmlDocument) As XmlDocument

        Public Overloads Sub ProcessTimed(ByVal oXml As XmlDocument, ByRef oResponse As HttpResponse)

            Dim sProcessInfo As String = ""
            Try
                Dim d As New ProcessDelegate(AddressOf Process)
                Dim res As IAsyncResult = d.BeginInvoke(oXml, oResponse, Nothing, Nothing)
                If res.IsCompleted = False Then
                    res.AsyncWaitHandle.WaitOne(mnTimeoutSec, False)
                    If res.IsCompleted = False Then
                        d.EndInvoke(DirectCast(res, Runtime.Remoting.Messaging.AsyncResult))
                        d = Nothing
                        Err.Raise(1010, "TranformXSL", "The XSL took longer than " & (mnTimeoutSec / 1000) & " seconds to process")
                        bError = True
                    End If
                End If
                d.EndInvoke(DirectCast(res, Runtime.Remoting.Messaging.AsyncResult))
            Catch ex As Exception
                returnException(myWeb.msException, "Protean.XmlHelper.TransformTimed", "Process", ex, msXslFile, sProcessInfo, mbDebug)
                oResponse.Write(myWeb.msException)
                bError = True
            End Try
        End Sub

        Public Overloads Sub ProcessTimed(ByVal oXml As XmlDocument, ByRef oWriter As IO.TextWriter)

            Dim sProcessInfo As String = ""
            Try
                Dim d As New ProcessDelegate2(AddressOf Process)
                Dim res As IAsyncResult = d.BeginInvoke(oXml, oWriter, Nothing, Nothing)
                If res.IsCompleted = False Then
                    res.AsyncWaitHandle.WaitOne(mnTimeoutSec, False)
                    If res.IsCompleted = False Then
                        d.EndInvoke(oWriter, DirectCast(res, Runtime.Remoting.Messaging.AsyncResult))
                        d = Nothing
                        Err.Raise(1010, "TranformXSL", "The XSL took longer than " & (mnTimeoutSec / 1000) & " seconds to process")
                        bError = True
                    End If
                End If
                d.EndInvoke(oWriter, DirectCast(res, Runtime.Remoting.Messaging.AsyncResult))
            Catch ex As Exception
                returnException(myWeb.msException, "Protean.XmlHelper.TransformTimed", "Process", ex, msXslFile, sProcessInfo)
                oWriter.Write(myWeb.msException)
                bError = True
            End Try
        End Sub

        Public Function ProcessTimedDocument(ByVal oXml As XmlDocument) As XmlDocument
            If Not CanProcess Then Return Nothing
            Dim sProcessInfo As String = ""
            Dim oWriter As TextWriter = New StringWriter
            Try
                Dim d As New ProcessDelegate2(AddressOf Process)

                Dim res As IAsyncResult = d.BeginInvoke(oXml, oWriter, Nothing, Nothing)
                If res.IsCompleted = False Then
                    res.AsyncWaitHandle.WaitOne(mnTimeoutSec, False)
                    If res.IsCompleted = False Then

                        d.EndInvoke(oWriter, DirectCast(res, Runtime.Remoting.Messaging.AsyncResult))
                        d = Nothing
                        Err.Raise(1010, "TranformXSL", "The XSL took longer than " & (mnTimeoutSec / 1000) & " seconds to process")
                    End If
                End If
                d.EndInvoke(oWriter, DirectCast(res, Runtime.Remoting.Messaging.AsyncResult))
                Dim oXMLNew As New XmlDocument
                oXml.InnerXml = oWriter.ToString
                Return oXml
            Catch ex As Exception
                returnException(myWeb.msException, "Protean.XmlHelper.Transform", "Process", ex, msXslFile, sProcessInfo, mbDebug)
                oWriter.Write(myWeb.msException)
                bError = True
                Return Nothing
            End Try
        End Function
        Public Shadows Sub Process(ByVal xReader As XmlReader, ByVal xWriter As XmlWriter)

            Dim sProcessInfo As String = "Processing:" & msXslFile

            Try
                ' If msException = "" Then
                Dim resolver As New XmlUrlResolver()
                resolver.Credentials = System.Net.CredentialCache.DefaultCredentials
                oCStyle.Transform(xReader, xsltArgs, xWriter) ', resolver)
                '  Else
                ' xWriter.Write(msException)
                ' End If

            Catch ex As Exception
                transformException = ex
                returnException(myWeb.msException, "Protean.XmlHelper.Transform", "Process", ex, msXslFile, sProcessInfo, mbDebug)
                ' oResponse.Write(msException)
                bError = True
            End Try

        End Sub


        Public Shadows Sub Process(ByVal oXml As XmlDocument, ByVal oResponse As HttpResponse)
            If Not CanProcess Then Exit Sub
            Dim sProcessInfo As String = "Processing:" & msXslFile
            Try
                If mbCompiled Then

                    'Dim xsltDomain As AppDomain
                    'If goApp("xsltDomain") Is Nothing Then
                    '    Dim ads As New AppDomainSetup
                    '    ads.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory & "\bin"
                    '    ads.DisallowBindingRedirects = False
                    '    ads.DisallowCodeDownload = True
                    '    ' ads.LoaderOptimization = LoaderOptimization.SingleDomain
                    '    ads.ConfigurationFile = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Config\web.config"
                    '    'ads.ConfigurationFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile
                    '    ads.ApplicationTrust = AppDomain.CurrentDomain.ApplicationTrust
                    '    Dim adevidence As System.Security.Policy.Evidence = AppDomain.CurrentDomain.Evidence
                    '    xsltDomain = AppDomain.CreateDomain("xslt", adevidence, ads)
                    '    goApp("xsltDomain") = xsltDomain
                    'Else
                    '    xsltDomain = goApp("xsltDomain")
                    'End If


                    Dim resolver As New XmlUrlResolver()
                    resolver.Credentials = System.Net.CredentialCache.DefaultCredentials

                    Dim ws As Xml.XmlWriterSettings = oCStyle.OutputSettings.Clone()

                    'load the pagexml into a reader
                    Dim oReader As Xml.XmlTextReader = New XmlTextReader(New StringReader(oXml.OuterXml))
                    Dim sWriter As IO.StringWriter = New IO.StringWriter

                    If myWeb.msException = "" Then
                        'Run transformation

                        ' Dim xsltDomainProxy As ProxyDomain = xsltDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, GetType(ProxyDomain).FullName)
                        ' xsltDomainProxy._LocalContext = myWeb.moCtx
                        ' Dim responseString As String = xsltDomainProxy.RunTransform(AssemblyPath, ClassName, oXml.OuterXml)
                        ' oResponse.Write(responseString)

                        oCStyle.Transform(oReader, xsltArgs, XmlWriter.Create(oResponse.OutputStream, ws), resolver)
                    Else
                        oResponse.Write(myWeb.msException)
                    End If
                    oReader.Close()
                    sWriter.Dispose()

                Else

                    'Change the xmlDocument to xPathDocument to improve performance
                    Dim oXmlNodeReader As XmlNodeReader = New XmlNodeReader(oXml)
                    Dim xpathDoc As XPath.XPathDocument = New XPath.XPathDocument(oXmlNodeReader)

                    If myWeb Is Nothing Then
                        oStyle.Transform(xpathDoc, xsltArgs, oResponse.OutputStream, Nothing)
                    Else
                        If myWeb.msException = "" Then
                            'Run transformation
                            oStyle.Transform(xpathDoc, xsltArgs, oResponse.OutputStream, Nothing)
                        Else
                            oResponse.Write(myWeb.msException)
                        End If
                    End If

                    oXmlNodeReader.Close()
                    xpathDoc = Nothing

                End If
            Catch ex As Exception
                transformException = ex
                returnException(myWeb.msException, "Protean.XmlHelper.Transform", "Process", ex, msXslFile, sProcessInfo, mbDebug)
                oResponse.Write(myWeb.msException)
                bError = True
            End Try
        End Sub

        Public Function stripNonValidXMLCharacters(textIn As String) As [String]
            Dim textOut As New System.Text.StringBuilder()
            Dim textOuterr As New System.Text.StringBuilder()
            ' Used to hold the output.
            Dim current As Char
            ' Used to reference the current character.
            Dim currenti As Integer


            If textIn Is Nothing OrElse textIn = String.Empty Then
                Return String.Empty
            End If
            ' vacancy test.
            For i As Integer = 0 To textIn.Length - 1
                current = textIn(i)
                currenti = AscW(current)

                If (currenti = CInt("&H9") OrElse
                    currenti = CInt("&HA") OrElse
                    currenti = CInt("&HD")) OrElse
                ((currenti >= CInt("&H20")) AndAlso (currenti <= CInt("&HD7FF"))) OrElse
                ((currenti >= CInt("&HE000")) AndAlso (currenti <= CInt("&HFFFD"))) OrElse
                ((currenti >= CInt("&H10000")) AndAlso (currenti <= CInt("&H10FFFF"))) _
                Then
                    textOut.Append(current)
                Else
                    textOuterr.Append(current)
                End If
            Next
            Return textOut.ToString()
        End Function



        Public Shadows Sub Process(ByVal oXml As XmlDocument, ByRef oWriter As IO.TextWriter)
            If Not CanProcess Then Exit Sub
            bError = False
            Dim sProcessInfo As String = "Processing: " & msXslFile
            Try
                If mbCompiled Then
                    If myWeb Is Nothing Then
                        Dim oReader As Xml.XmlTextReader = New XmlTextReader(New StringReader(oXml.OuterXml))
                        Dim sWriter As IO.StringWriter = New IO.StringWriter
                        oCStyle.Transform(oReader, xsltArgs, oWriter)
                    Else
                        If myWeb.msException = "" Then
                            'Run transformation 
                            Dim oReader As Xml.XmlTextReader = New XmlTextReader(New StringReader(oXml.OuterXml))
                            Dim sWriter As IO.StringWriter = New IO.StringWriter
                            oCStyle.Transform(oReader, xsltArgs, oWriter)
                        Else
                            oWriter.Write(myWeb.msException)
                        End If
                    End If
                Else
                    'Change the xmlDocument to xPathDocument to improve performance
                    Dim xpathDoc As XPath.XPathDocument = New XPath.XPathDocument(New XmlNodeReader(oXml))
                    If myWeb Is Nothing Then
                        oStyle.Transform(xpathDoc, xsltArgs, oWriter, Nothing)
                    Else
                        If myWeb.msException = "" Or myWeb.msException Is Nothing Then
                            'Run transformation
                            oStyle.Transform(xpathDoc, xsltArgs, oWriter, Nothing)
                        Else
                            oWriter.Write(myWeb.msException)
                        End If
                    End If
                End If
            Catch ex As Exception
                transformException = ex
                If Not myWeb Is Nothing Then
                    returnException(myWeb.msException, "Protean.XmlHelper.Transform", "Process", ex, msXslFile, sProcessInfo, mbDebug)
                    oWriter.Write(myWeb.msException)
                End If
                bError = True
            End Try
        End Sub

        Public Function ProcessDocument(ByVal oXml As XmlDocument) As XmlDocument
            If Not CanProcess Then Return Nothing
            Dim sProcessInfo As String = "Proceesing:" & msXslFile
            Dim oWriter As TextWriter = New StringWriter
            Try

                If mbCompiled Then

                    Dim oStyle As Xsl.XslCompiledTransform
                    'hear we cache the loaded xslt in the application object.                    
                    Dim resolver As New XmlUrlResolver()
                    resolver.Credentials = System.Net.CredentialCache.DefaultCredentials

                    'here we store the stylesheet in the application object
                    If goApp(msXslFile) Is Nothing Then

                        'load the xslt
                        oStyle = New Xsl.XslCompiledTransform
                        'this is the line that takes the time
                        oStyle.Load(msXslFile, Xsl.XsltSettings.TrustedXslt, resolver)
                        goApp.Add(msXslFile, oStyle)
                    Else
                        'get the loaded xslt from the application variable
                        oStyle = goApp(msXslFile)
                    End If

                    'add Eonic Bespoke Functions

                    Dim xsltArgs As Xsl.XsltArgumentList = New Xsl.XsltArgumentList
                    Dim ewXsltExt As New xsltExtensions(myWeb)
                    xsltArgs.AddExtensionObject("urn:ew", ewXsltExt)


                    Dim ws As Xml.XmlWriterSettings = oStyle.OutputSettings.Clone()

                    'load the pagexml into a reader
                    Dim oReader As Xml.XmlTextReader = New XmlTextReader(New StringReader(oXml.OuterXml))
                    Dim sWriter As IO.StringWriter = New IO.StringWriter

                    If myWeb.msException = "" Then
                        'Run transformation
                        oStyle.Transform(oReader, xsltArgs, oWriter)
                    Else
                        oWriter.Write(myWeb.msException)
                    End If

                Else
                    'the old method is quicker for realtime loading of xslt


                    'load the xslt
                    oStyle.Load(msXslFile)

                    'add Eonic Bespoke Functions
                    Dim xsltArgs As Xsl.XsltArgumentList = New Xsl.XsltArgumentList
                    Dim ewXsltExt As New xsltExtensions(myWeb)
                    xsltArgs.AddExtensionObject("urn:ew", ewXsltExt)


                    'Change the xmlDocument to xPathDocument to improve performance
                    Dim xpathDoc As XPath.XPathDocument = New XPath.XPathDocument(New XmlNodeReader(oXml))

                    If myWeb.msException = "" Then
                        'Run transformation
                        oStyle.Transform(xpathDoc, xsltArgs, oWriter, Nothing)
                    Else
                        oWriter.Write(myWeb.msException)
                    End If


                End If
                Dim oXMLNew As New XmlDocument
                oXml.InnerXml = oWriter.ToString
                Return oXml
            Catch ex As Exception
                bError = True
                currentError = ex
                returnException(myWeb.msException, "Protean.XmlHelper.Transform", "ProcessDocument", ex, msXslFile, sProcessInfo, mbDebug)
                oWriter.Write(myWeb.msException)
                Return Nothing
            End Try
        End Function

        <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
        Public Shared Function LoadLibrary(<[In](), MarshalAs(UnmanagedType.LPStr)> ByVal lpFileName As String) As IntPtr
        End Function
        <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
        Public Shared Function GetModuleHandle(ByVal lpModuleName As String) As IntPtr
        End Function
        <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
        Public Shared Function FreeLibrary(<[In]()> ByVal hModule As IntPtr) As Boolean
        End Function


        Public Function ClearXSLTassemblyCache() As Boolean
            Dim sProcessInfo As String = "ClearXSLTassemblyCache"
            Try

                Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
                If oImp.ImpersonateValidUser(myWeb.moConfig("AdminAcct"), myWeb.moConfig("AdminDomain"), myWeb.moConfig("AdminPassword"), True, myWeb.moConfig("AdminGroup")) Then

                    Dim cWorkingDirectory As String = goServer.MapPath(compiledFolder)
                    sProcessInfo = "clearing " & cWorkingDirectory
                    Dim di As New IO.DirectoryInfo(cWorkingDirectory)
                    Dim fi As IO.FileInfo

                    For Each fi In di.EnumerateFiles
                        Try
                            Dim fso As New fsHelper()
                            fso.DeleteFile(fi.FullName)
                        Catch ex2 As Exception
                            ' returnException("Protean.XmlHelper.Transform", "ClearXSLTassemblyCache", ex2, msXslFile, sProcessInfo)
                        End Try
                    Next

                    oImp.UndoImpersonation()
                End If


                'reset config to on
                Protean.Config.UpdateConfigValue(myWeb, "protean/web", "CompliedTransform", "on")

                'di.Delete(True)

            Catch ex As Exception
                bError = True
                returnException(myWeb.msException, "Protean.XmlHelper.Transform", "ClearXSLTassemblyCache", ex, msXslFile, sProcessInfo, mbDebug)
                Return Nothing
            End Try
        End Function


        Public Function CompileXSLTassembly(ByVal classname As String) As String

            Dim compilerPath As String = goServer.MapPath("/ewcommon/xsl/compiler/xsltc.exe")
            Dim xsltPath As String = """" & msXslFile & """"
            Dim sProcessInfo As String = "compiling: " & xsltPath
            Dim outFile As String = classname & ".dll"
            Dim cmdLine As String = " /class:" & classname & " /out:" & outFile & " " & xsltPath
            Dim process1 As New System.Diagnostics.Process
            Dim output As String = ""

            Try

                If goApp("compileLock-" & classname) Is Nothing Then

                    goApp("compileLock-" & classname) = True

                    process1.EnableRaisingEvents = True
                    process1.StartInfo.FileName = compilerPath
                    process1.StartInfo.Arguments = cmdLine
                    process1.StartInfo.UseShellExecute = False
                    process1.StartInfo.RedirectStandardOutput = True
                    process1.StartInfo.RedirectStandardInput = True
                    process1.StartInfo.RedirectStandardError = True

                    'check if local bin exists
                    Dim cWorkingDirectory As String = goServer.MapPath(compiledFolder)
                    Dim di As DirectoryInfo = New DirectoryInfo(cWorkingDirectory)
                    If Not di.Exists Then
                        di.Create()
                    End If

                    process1.StartInfo.WorkingDirectory = cWorkingDirectory
                    'Start the process
                    process1.Start()
                    output = process1.StandardOutput.ReadToEnd()

                    'Wait for process to finish
                    process1.WaitForExit()

                    process1.Close()

                    goApp("compileLock-" & classname) = Nothing

                End If

                If output.Contains("error") Then
                    Return output
                Else
                    Return classname
                End If

            Catch ex As Exception
                bError = True
                returnException(myWeb.msException, "Protean.XmlHelper.Transform", "CompileXSLTassembly", ex, msXslFile, sProcessInfo, mbDebug)
                Return Nothing
            End Try
        End Function


    End Class





    Private Class ProxyDomain
        Inherits MarshalByRefObject
        Public _LocalContext As HttpContext

        Public Sub GetAssembly(AssemblyPath As String, className As String)
            Try
                Dim newAssem As Assembly = Assembly.LoadFrom(AssemblyPath)


                'If you want to do anything further to that assembly, you need to do it here.


            Catch ex As Exception
                Throw New InvalidOperationException(ex.Message, ex)
            End Try
        End Sub

        Public Function RunTransform(AssemblyPath As String, className As String, PageXml As String) As String
            Try
                HttpContext.Current = _LocalContext

                Dim oReader As Xml.XmlTextReader = New XmlTextReader(New StringReader(PageXml))

                Dim xsltArgs As Xsl.XsltArgumentList = New Xsl.XsltArgumentList
                Dim ewXsltExt As New xsltExtensions()
                xsltArgs.AddExtensionObject("urn:ew", ewXsltExt)

                Dim newAssem As Assembly = Assembly.LoadFrom(AssemblyPath)
                Dim CalledType As System.Type = newAssem.GetType(className, True)
                Dim oCStyle As Xsl.XslCompiledTransform = New Xsl.XslCompiledTransform(True)
                Dim sWriter As IO.StringWriter = New IO.StringWriter

                oCStyle.Load(CalledType)
                Dim oWriter As TextWriter = New StringWriter

                oCStyle.Transform(oReader, xsltArgs, oWriter)

                Return oWriter.ToString

            Catch ex As Exception
                Throw New InvalidOperationException(ex.Message, ex)
            End Try
        End Function
    End Class


End Class

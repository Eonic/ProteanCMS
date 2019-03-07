Imports System.Text
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Xml.XPath
Imports System.Text.RegularExpressions

<Assembly: CLSCompliant(True)> 

<CLSCompliant(True)> _
Public Class Xml

#Region "Declarations"

    Public Shared Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
    Private Const mcModuleName As String = "Protean.Tools.Xml"

    Public Enum XmlDataType

        TypeString = 1
        TypeNumber = 2
        TypeDate = 3

    End Enum

    Public Enum XmlNodeState

        NotInstantiated = 0
        IsEmpty = 1
        HasContents = 2
        IsEmptyOrHasContents = 3

    End Enum


#End Region

#Region "Private Procedures"
    Private Shared Sub CombineInstance_Sub1(ByRef oMasterElmt As XmlElement, ByVal oExistingInstance As XmlElement, ByVal cXPath As String)
        'Looks for stuff with the same xpath
        Try
            If Not cXPath = "" Then cXPath = cXPath & "/"
            cXPath &= oMasterElmt.Name

            Dim oInstanceElmt As XmlElement = oExistingInstance.SelectSingleNode(cXPath)
            If Not oInstanceElmt Is Nothing Then
                'Master Attributes first
                For i As Integer = 0 To oMasterElmt.Attributes.Count - 1
                    Dim oMasterAtt As XmlAttribute = oMasterElmt.Attributes(i)
                    Dim oInstanceAtt As XmlAttribute = oInstanceElmt.Attributes.ItemOf(oMasterAtt.Name)
                    If Not oInstanceAtt Is Nothing Then oMasterAtt.Value = oInstanceAtt.Value
                Next
                'Now Existing
                For i As Integer = 0 To oInstanceElmt.Attributes.Count - 1
                    Dim oInstanceAtt As XmlAttribute = oInstanceElmt.Attributes(i)
                    Dim oMasterAtt As XmlAttribute = oMasterElmt.Attributes.ItemOf(oInstanceAtt.Name)
                    If oMasterAtt Is Nothing Then oMasterElmt.SetAttribute(oInstanceAtt.Name, oInstanceAtt.Value)
                Next
                'Now we need to check child nodes
                If oMasterElmt.SelectNodes("*").Count > 0 Then
                    For Each oChild As XmlElement In oMasterElmt.SelectNodes("*")
                        CombineInstance_Sub1(oChild, oExistingInstance, cXPath)
                    Next
                ElseIf oInstanceElmt.SelectNodes("*").Count > 0 Then
                    For Each oChild As XmlElement In oInstanceElmt.SelectNodes("*")
                        oMasterElmt.AppendChild(oMasterElmt.OwnerDocument.ImportNode(oChild, True))
                    Next
                    CombineInstance_MarkSubElmts(oInstanceElmt)
                ElseIf Not oInstanceElmt.InnerText = "" Then
                    oMasterElmt.InnerText = oInstanceElmt.InnerText
                End If
                'now mark both elements as found
                oMasterElmt.SetAttribute("ewCombineInstanceFound", "TRUE")
                oInstanceElmt.SetAttribute("ewCombineInstanceFound", "TRUE")

                'We wont go for others just yet as we want to complete this before finding
                'other less obvious stuff

            End If

        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CombineInstance_Sub1", ex, ""))
        End Try
    End Sub

    Private Shared Sub CombineInstance_Sub2(ByRef oExisting As XmlElement, ByVal oMasterInstance As XmlElement, ByVal cRootName As String)
        'Gets any unmarked in Existing data and copies over to new
        Try
            Dim cXPath As String = ""
            CombineInstance_GetXPath(oExisting, cXPath, cRootName)
            Dim oParts() As String = Split(cXPath, "/")
            Dim oRefPart As XmlElement = oMasterInstance
            For i As Integer = 0 To oParts.Length - 2
                If Not oRefPart.SelectSingleNode(oParts(i)) Is Nothing Then
                    oRefPart = oRefPart.SelectSingleNode(oParts(i))
                Else
                    Dim oElmt As XmlElement = oMasterInstance.OwnerDocument.CreateElement(oParts(i))
                    oRefPart.AppendChild(oElmt)
                    oRefPart = oElmt
                End If
            Next
            'still need to create last part
            oRefPart.AppendChild(oMasterInstance.OwnerDocument.ImportNode(oExisting, True))
            CombineInstance_MarkSubElmts(oExisting)
        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CombineInstance_Sub2", ex, ""))
        End Try
    End Sub

    Private Shared Sub CombineInstance_GetXPath(ByVal oElmt As XmlElement, ByRef xPath As String, ByVal cRootName As String)
        Try
            If xPath = "" Then xPath = oElmt.Name
            If Not oElmt.ParentNode Is Nothing Then
                If Not oElmt.ParentNode.Name = cRootName Then
                    xPath = oElmt.ParentNode.Name & "/" & xPath
                    CombineInstance_GetXPath(oElmt.ParentNode, xPath, cRootName)
                End If
            End If
        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CombineInstance_GetXPath", ex, ""))
        End Try
    End Sub

    Private Shared Sub CombineInstance_MarkSubElmts(ByRef oElmt As XmlElement, Optional ByVal bRemove As Boolean = False)
        Try
            If bRemove Then
                oElmt.RemoveAttribute("ewCombineInstanceFound")
            Else
                oElmt.SetAttribute("ewCombineInstanceFound", "TRUE")
            End If

            For Each oChild As XmlElement In oElmt.SelectNodes("*")
                CombineInstance_MarkSubElmts(oChild, bRemove)
            Next
        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CombineInstance_MarkSubElmts", ex, ""))
        End Try
    End Sub


#End Region

#Region "Public Functions"

    Public Shared Function SortedNodeList_UNCONNECTED(ByVal oRootObject As XmlElement, ByVal cXPath As String, ByVal cSortItem As String, ByVal cSortOrder As XmlSortOrder, ByVal cSortDataType As XmlDataType, ByVal cCaseOrder As XmlCaseOrder) As XmlNodeList
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

    Public Shared Function XmlToForm(ByVal sString As String) As String
        Dim sProcessInfo As String = ""
        Try
            sString = Replace(sString, "<", "&lt;")
            sString = Replace(sString, ">", "&gt;")
            Return sString & ""
        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "XmlToForm", ex, ""))
            Return ""
        End Try
    End Function

    Public Shared Function htmlToXmlDoc(ByVal shtml As String) As XmlDocument
        Dim oXmlDoc As New XmlDocument
        Dim sProcessInfo As String = ""

        Try

            shtml = Replace(shtml, "<!DOCTYPE html public shared ""-//W3C//DTD XHTML 1.1//EN"" ""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"">", "")
            shtml = Replace(shtml, " Xmlns = ""http://www.w3.org/1999/xhtml""", "")
            shtml = Replace(shtml, " Xmlns=""http://www.w3.org/1999/xhtml""", "")
            shtml = Replace(shtml, " Xml:lang=""""", "")
            oXmlDoc.LoadXml(shtml)

            Return oXmlDoc
        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "htmlToXmlDoc", ex, ""))
            Return Nothing
        End Try
    End Function

    Public Shared Function getNsMgr(ByRef oNode As XmlNode, ByRef oXml As XmlDocument) As XmlNamespaceManager
        Try
            Dim oElmt As XmlElement
            Dim cNsUri As String = ""
            Dim nsMgr As XmlNamespaceManager = New XmlNamespaceManager(oXml.NameTable)

            'does the instance have a namespace, if so give it a standard prefix of EWS
            oElmt = oNode

            If Not oElmt Is Nothing Then
                cNsUri = oElmt.GetAttribute("Xmlns")
                If cNsUri = "" Then
                    cNsUri = oElmt.GetAttribute("xmlns")
                End If
            End If

            If cNsUri <> "" Then
                nsMgr.AddNamespace("ews", cNsUri)
            End If

            Return nsMgr
        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getNsMgr", ex, ""))
            Return Nothing
        End Try

    End Function


    Public Shared Function getNsMgrWithURI(ByRef oNode As XmlNode, ByRef oXml As XmlDocument) As XmlNamespaceManager

        Dim oElmt As XmlElement
        Dim cNsUri As String
        Try

            Dim nsMgr As XmlNamespaceManager = New XmlNamespaceManager(oXml.NameTable)
            'does the instance have a namespace, if so give it a standard prefix of EWS
            oElmt = oNode
            cNsUri = oElmt.GetAttribute("xmlns")

            If cNsUri <> "" Then
                nsMgr.AddNamespace("ews", cNsUri)
            ElseIf oElmt.NamespaceURI <> "" Then
                nsMgr.AddNamespace("ews", oElmt.NamespaceURI)
            End If

            Return nsMgr
        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getNsMgrWithURI", ex, ""))
            Return Nothing
        End Try

    End Function

    Public Shared Function getNsMgrRecursive(ByRef oNode As XmlNode, ByRef oXml As XmlDocument) As XmlNamespaceManager
        Try
            Dim oElmt As XmlElement
            Dim cNsUri As String = ""
            Dim nsMgr As XmlNamespaceManager = New XmlNamespaceManager(oXml.NameTable)
            Dim cPrefix As String = "ews"

            'does the instance have a namespace? if so give it a standard prefix of EWS
            'If a subnode of the instance has a namespace we give it a prefix of ews2
            'allows us to specifiy ews2 in bind paths.

            For Each oElmt In oNode.SelectNodes("descendant-or-self::*")
                If Not oElmt Is Nothing Then
                    cNsUri = oElmt.GetAttribute("Xmlns")
                    If cNsUri = "" Then
                        cNsUri = oElmt.GetAttribute("xmlns")
                    End If
                End If
                If cNsUri <> "" Then
                    nsMgr.AddNamespace(cPrefix, cNsUri)
                End If
                If cPrefix = "ews" Then cPrefix = "ews2"
            Next

            Return nsMgr
        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getNsMgr", ex, ""))
            Return Nothing
        End Try

    End Function


    Public Shared Function addNsToXpath(ByVal cXpath As String, ByRef nsMgr As XmlNamespaceManager) As String
        Try
            'check namespace not allready specified
            If InStr(cXpath, "ews:") = 0 Then
                If nsMgr.HasNamespace("ews") Then
                    cXpath = "ews:" & Replace(cXpath, "/", "/ews:")

                    'find any [ not followed by number
                    Dim substrings() As String = Regex.Split(cXpath, "(\[(?!\d))")
                    If substrings.Length > 1 Then
                        Dim cNewXpath As String = ""
                        For Each match As String In substrings
                            If match.StartsWith("[") Then
                                cNewXpath = cNewXpath & "[ews:" & match.Substring(1)
                            Else

                                cNewXpath = cNewXpath & match
                            End If
                        Next
                        cXpath = cNewXpath
                    End If

                    'undo any attributes
                    cXpath = Replace(cXpath, "/ews:@", "/@")
                    cXpath = Replace(cXpath, "[ews:@", "[@")
                    cXpath = Replace(cXpath, "[ews:position()", "[position()")
                End If
            End If

            Return cXpath
        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "addNsToXpath", ex, ""))
            Return ""
        End Try


    End Function

    Public Shared Function addElement(ByRef oParent As XmlElement, ByVal cNewNodeName As String, Optional ByVal cNewNodeValue As String = "", Optional ByVal bAddAsXml As Boolean = False, Optional ByVal bPrepend As Boolean = False, Optional ByRef oNodeFromXPath As XmlNode = Nothing, Optional ByVal sPrefix As String = Nothing, Optional ByVal NamespaceURI As String = "") As XmlElement
        Try
            Dim oNewNode As XmlElement

            If sPrefix Is Nothing Then
                oNewNode = oParent.OwnerDocument.CreateElement(cNewNodeName)
            Else
                oNewNode = oParent.OwnerDocument.CreateElement(sPrefix, cNewNodeName, NamespaceURI)
            End If

            If cNewNodeValue = "" And Not (oNodeFromXPath Is Nothing) Then
                If bAddAsXml Then cNewNodeValue = oNodeFromXPath.InnerXml Else cNewNodeValue = oNodeFromXPath.InnerText
            End If
            If cNewNodeValue <> "" Then
                If bAddAsXml Then oNewNode.InnerXml = cNewNodeValue Else oNewNode.InnerText = cNewNodeValue
            End If
            If bPrepend Then oParent.PrependChild(oNewNode) Else oParent.AppendChild(oNewNode)
            Return oNewNode
        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "addElement", ex, ""))
            Return Nothing
        End Try


    End Function

    Public Shared Function firstElement(ByRef oElement As XmlElement) As XmlElement
        Dim oNode As XmlNode
        Dim oReturnNode As XmlElement = Nothing
        Try
            For Each oNode In oElement.ChildNodes
                If oNode.NodeType = XmlNodeType.Element Then
                    oReturnNode = oNode
                    Exit For
                End If
            Next
            Return oReturnNode
        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "firstElement", ex, ""))
            Return Nothing
        End Try
    End Function

    Public Shared Sub renameNode(ByRef oNode As XmlElement, ByVal cNewNodeName As String)
        Try
            Dim oNewNode As XmlElement
            Dim oChildElmt As XmlElement

            oNewNode = oNode.OwnerDocument.CreateElement(cNewNodeName)

            For Each oChildElmt In oNode.SelectNodes("*")
                oNewNode.AppendChild(oChildElmt)
            Next
            oNode.ParentNode.ReplaceChild(oNewNode, oNode)

        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "renameNode", ex, ""))
        End Try
    End Sub

    Public Shared Sub removeChildByName(ByRef oNode As XmlElement, ByVal cRemoveNodeName As String)
        Try
            Dim removeElmt As XmlElement = oNode.SelectSingleNode(cRemoveNodeName)
            If removeElmt IsNot Nothing Then
                oNode.RemoveChild(removeElmt)
            End If

        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "removeChildByName", ex, ""))
        End Try
    End Sub

    Public Shared Function addNewTextNode(ByVal cNodeName As String, ByRef oNode As XmlNode, Optional ByVal cNodeValue As String = "", Optional ByVal bAppendNotPrepend As Boolean = True, Optional ByVal bOverwriteExistingNode As Boolean = False) As XmlElement
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
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "addNewTextNode", ex, ""))
            Return Nothing
        End Try


    End Function

    'Public Shared Function getNodeValueByType(ByVal oParent As XmlNode, ByVal cXPath As String, Optional ByVal nNodeType As XmlDataType = XmlDataType.TypeString, Optional ByVal vDefaultValue As Object = "") As Object
    '    Try
    '        Dim oNode As XmlNode
    '        Dim cValue As Object
    '        oNode = oParent.SelectSingleNode(cXPath)
    '        If oNode Is Nothing Then
    '            ' No xPath match, let's return a default value
    '            ' If there are no default values, then let's return a nominal default value
    '            Select Case nNodeType
    '                Case XmlDataType.TypeNumber
    '                    cValue = IIf(IsNumeric(vDefaultValue), vDefaultValue, 0)
    '                Case XmlDataType.TypeDate
    '                    cValue = IIf(IsDate(vDefaultValue), vDefaultValue, Now())
    '                Case Else ' Assume default type is string
    '                    cValue = IIf(vDefaultValue <> "", vDefaultValue.ToString, "")
    '            End Select
    '        Else
    '            ' XPath  match, let's check it against type
    '            cValue = oNode.InnerText
    '            Select Case nNodeType
    '                Case XmlDataType.TypeNumber
    '                    cValue = IIf(IsNumeric(cValue), cValue, IIf(IsNumeric(vDefaultValue), vDefaultValue, 0))
    '                Case XmlDataType.TypeDate
    '                    cValue = IIf(IsDate(cValue), cValue, IIf(IsDate(vDefaultValue), vDefaultValue, Now()))
    '                Case Else ' Assume default type is string
    '                    cValue = IIf(cValue <> "", cValue.ToString, IIf(vDefaultValue <> "", vDefaultValue.ToString, ""))
    '            End Select
    '        End If

    '        Return cValue
    '    Catch ex As Exception
    '        RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "getNodeValueByType", ex, ""))
    '        Return Nothing
    '    End Try

    'End Function

    Public Shared Function getNodeValueByType(ByRef oParent As XmlNode, ByVal cXPath As String, Optional ByVal nNodeType As XmlDataType = XmlDataType.TypeString, Optional ByVal vDefaultValue As Object = "", Optional ByRef nsMgr As XmlNamespaceManager = Nothing) As Object

        Dim oNode As XmlNode
        Dim cValue As Object
        If nsMgr Is Nothing Then
            oNode = oParent.SelectSingleNode(cXPath)
        Else
            oNode = oParent.SelectSingleNode(addNsToXpath(cXPath, nsMgr), nsMgr)
        End If
        If oNode Is Nothing Then
            ' No xPath match, let's return a default value
            ' If there are no default values, then let's return a nominal default value
            Select Case nNodeType
                Case XmlDataType.TypeNumber
                    cValue = IIf(IsNumeric(vDefaultValue), vDefaultValue, 0)
                Case XmlDataType.TypeDate
                    cValue = IIf(IsDate(vDefaultValue), vDefaultValue, Now())
                Case Else ' Assume default type is string
                    cValue = IIf(vDefaultValue <> "", vDefaultValue.ToString, "")
            End Select
        Else
            ' XPath  match, let's check it against type
            cValue = oNode.InnerText
            Select Case nNodeType
                Case XmlDataType.TypeNumber
                    cValue = IIf(IsNumeric(cValue), cValue, IIf(IsNumeric(vDefaultValue), vDefaultValue, 0))
                Case XmlDataType.TypeDate
                    cValue = IIf(IsDate(cValue), cValue, IIf(IsDate(vDefaultValue), vDefaultValue, Now()))
                Case Else ' Assume default type is string
                    cValue = IIf(cValue <> "", cValue.ToString, IIf(vDefaultValue <> "", vDefaultValue.ToString, ""))
            End Select
        End If

        Return cValue
    End Function

    ''' <summary>
    ''' <para>This allows XML to be loaded in to an element from a given file</para>
    ''' <para>The following checks are made:</para>
    ''' <list>
    ''' <item><term>File Exists?</term><description>Does the file provided by cFilePath actually exist</description></item>
    ''' <item><term>Valid XML</term><description>Is the loaded XMl actually valid?</description></item>
    ''' </list>
    ''' <para>If either check fails, then either Nothing or an empty element with a node name determined by <c>cErrorNodeName</c>.</para>
    ''' </summary>
    ''' <param name="cFilePath">The file path of the file that is being loaded</param>
    ''' <param name="oXmlDocument">Optional - the xml document to generate the return element from</param>
    ''' <param name="cErrorNodeName">Optional - if the checks fail, then this param determines the name of an empty returned element.</param>
    ''' <returns>The loaded XML's DocumentElement, or Nothing/empty element</returns>
    ''' <remarks></remarks>
    Public Shared Function loadElement(ByVal cFilePath As String, Optional ByRef oXmlDocument As XmlDocument = Nothing, Optional ByVal cErrorNodeName As String = "") As XmlElement
        Try

            Dim oXmlLoadDocument As XmlDocument = New XmlDocument()
            Dim oReturnElement As XmlElement = Nothing
            Dim bSuccess As Boolean = False

            If oXmlDocument Is Nothing Then oXmlDocument = New XmlDocument()

            ' Try to find and load the element
            If IO.File.Exists(cFilePath) Then
                Try
                    oXmlLoadDocument.Load(cFilePath)
                    oReturnElement = oXmlLoadDocument.DocumentElement
                    bSuccess = True
                Catch ex As Exception
                    bSuccess = False
                End Try
            End If

            ' Did the checks fail?
            If Not (bSuccess) Then
                ' return Nothing or empty element
                If Not (String.IsNullOrEmpty(cErrorNodeName)) Then
                    oReturnElement = oXmlDocument.CreateElement(cErrorNodeName)
                End If
            Else
                oReturnElement = oXmlDocument.ImportNode(oReturnElement, True)
            End If

            Return oReturnElement

        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "loadElement", ex, ""))
            Return Nothing
        End Try


    End Function

    ''' <summary>
    ''' NodeState - Given a node, looks to see if it has been instantiated and has contents
    ''' </summary>
    ''' <param name="oNode">The root node for testing</param>
    ''' <param name="xPath">The xpath to apply to the root node.  If this is not supplied, the root node is tested.</param>
    ''' <param name="populateAsText">Text that can be added to the test node.</param>
    ''' <param name="populateAsXml">Xml that can be added to the test node</param>
    ''' <param name="populateState">The state that the test node must be in before it can be populated</param>
    ''' <param name="returnElement">The test node</param>
    ''' <param name="returnAsXml">The test node's inner xml</param>
    ''' <param name="returnAsText">The test node's inner text</param>
    ''' <param name="bCheckTrimmedInnerText">The matched node will have its innertext trimmed and examined</param>
    ''' <returns>XmlNodeState - the state of the test node.</returns>
    ''' <remarks></remarks>
    Public Shared Function NodeState( _
                ByRef oNode As XmlElement, _
                Optional ByVal xPath As String = "", _
                Optional ByVal populateAsText As String = "", _
                Optional ByVal populateAsXml As String = "", _
                Optional ByVal populateState As Xml.XmlNodeState = XmlNodeState.IsEmpty, _
                Optional ByRef returnElement As XmlElement = Nothing, _
                Optional ByRef returnAsXml As String = "", _
                Optional ByRef returnAsText As String = "", _
                Optional ByVal bCheckTrimmedInnerText As Boolean = False _
                ) As Xml.XmlNodeState

        Try

            Dim oReturnState As Xml.XmlNodeState = XmlNodeState.NotInstantiated

            ' Find the xPath if appropriate
            If Not (String.IsNullOrEmpty(xPath)) And Not (oNode Is Nothing) Then
                returnElement = oNode.SelectSingleNode(xPath)
            Else
                returnElement = oNode
            End If

            If Not (returnElement Is Nothing) Then
                ' Determine the node status
                If returnElement.IsEmpty Or (bCheckTrimmedInnerText And returnElement.InnerText.Trim() = "") Then
                    oReturnState = XmlNodeState.IsEmpty
                Else
                    oReturnState = XmlNodeState.HasContents
                End If

                ' Populate if necessary
                If (populateAsText <> "" Or populateAsXml <> "") And ((populateState = oReturnState) Or (populateState = XmlNodeState.IsEmptyOrHasContents)) Then
                    If populateAsText <> "" Then
                        returnElement.InnerText = populateAsText
                    Else
                        Try
                            returnElement.InnerXml = populateAsXml
                        Catch ex As Exception
                            ' Could not populate the Xml - what to do?
                        End Try
                    End If
                End If

                ' Return the optional params
                returnAsXml = returnElement.InnerXml
                returnAsText = returnElement.InnerText

            End If

            Return oReturnState

        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "NodeState", ex, ""))
            Return Nothing
        End Try

    End Function
    'RJP 7 Nove 2012. Added email address checker.
    Public Shared Function EmailAddressCheck(ByVal emailAddress As String) As Boolean

        Dim pattern As String = "^[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$"
        Dim emailAddressMatch As Match = Regex.Match(emailAddress, pattern)
        If emailAddressMatch.Success Then
            EmailAddressCheck = True
        Else
            EmailAddressCheck = False
        End If
    End Function

    'RJP 7 Nov 2012. Moved from Setup.vb
    Public Shared Function EncodeForXml(ByVal data As String) As String

        Static badAmpersand As New Regex("&(?![a-zA-Z]{2,6};|#[0-9]{2,4};)")

        data = badAmpersand.Replace(data, "&amp;")

        Return data.Replace("<", "&lt;").Replace("""", "&quot;").Replace(">", "gt;")
    End Function
    Public Shared Function convertEntitiesToCodes(ByVal sString As String) As String
        Try
            Dim startString As String = sString
            If sString Is Nothing Then
                Return ""
            Else

                sString = Replace(sString, "Xmlns=""""", "")

                ' sString = Replace(sString, "& ", "&amp; ")

                'strip out any empty tags left by tinyMCE as the complier converts the to <h1/>
                sString = Replace(sString, "<h1></h1>", "")
                sString = Replace(sString, "<h2></h2>", "")
                sString = Replace(sString, "<h3></h3>", "")
                sString = Replace(sString, "<h4></h4>", "")
                sString = Replace(sString, "<h5></h5>", "")
                sString = Replace(sString, "<h6></h6>", "")
                sString = Replace(sString, "<span></span>", "")

                're.Replace(sString, "&^(;)" "&amp;")

                'xhtml Tidy
                sString = Replace(sString, "<o:p>", "<p>")
                sString = Replace(sString, "</o:p>", "</p>")
                sString = Replace(sString, "<o:p/>", "<p/>")

                'sString = Replace(sString, "&lt;", "&#60;")
                'sString = Replace(sString, "&gt;", "&#92;")

                sString = Replace(sString, "&quot;", "&#34;")
                sString = Replace(sString, "&apos;", "&#39;")
                sString = Replace(sString, "&nbsp;", "&#160;")
                sString = Replace(sString, "&iexcl;", "&#161;")
                sString = Replace(sString, "&cent;", "&#162;")
                sString = Replace(sString, "&pound;", "&#163;")
                sString = Replace(sString, "&curren;", "&#164;")
                sString = Replace(sString, "&yen;", "&#165;")
                sString = Replace(sString, "&brvbar;", "&#166;")
                sString = Replace(sString, "&sect;", "&#167;")
                sString = Replace(sString, "&uml;", "&#168;")
                sString = Replace(sString, "&copy;", "&#169;")
                sString = Replace(sString, "&ordf;", "&#170;")
                sString = Replace(sString, "&laquo;", "&#171;")
                sString = Replace(sString, "&not;", "&#172;")
                sString = Replace(sString, "&shy;", "&#173;")
                sString = Replace(sString, "&reg;", "&#174;")
                sString = Replace(sString, "&macr;", "&#175;")
                sString = Replace(sString, "&deg;", "&#176;")
                sString = Replace(sString, "&plusmn;", "&#177;")
                sString = Replace(sString, "&sup2;", "&#178;")
                sString = Replace(sString, "&sup3;", "&#179;")
                sString = Replace(sString, "&acute;", "&#180;")
                sString = Replace(sString, "&micro;", "&#181;")
                sString = Replace(sString, "&para;", "&#182;")
                sString = Replace(sString, "&middot;", "&#183;")
                sString = Replace(sString, "&cedil;", "&#184;")
                sString = Replace(sString, "&sup1;", "&#185;")
                sString = Replace(sString, "&ordm;", "&#186;")
                sString = Replace(sString, "&raquo;", "&#187;")
                sString = Replace(sString, "&frac14;", "&#188;")
                sString = Replace(sString, "&frac12;", "&#189;")
                sString = Replace(sString, "&frac34;", "&#190;")
                sString = Replace(sString, "&iquest;", "&#191;")
                sString = Replace(sString, "&Agrave;", "&#192;")
                sString = Replace(sString, "&Aacute;", "&#193;")
                sString = Replace(sString, "&Acirc;", "&#194;")
                sString = Replace(sString, "&Atilde;", "&#195;")
                sString = Replace(sString, "&Auml;", "&#196;")
                sString = Replace(sString, "&Aring;", "&#197;")
                sString = Replace(sString, "&AElig;", "&#198;")
                sString = Replace(sString, "&Ccedil;", "&#199;")
                sString = Replace(sString, "&Egrave;", "&#200;")
                sString = Replace(sString, "&Eacute;", "&#201;")
                sString = Replace(sString, "&Ecirc;", "&#202;")
                sString = Replace(sString, "&Euml;", "&#203;")
                sString = Replace(sString, "&Igrave;", "&#204;")
                sString = Replace(sString, "&Iacute;", "&#205;")
                sString = Replace(sString, "&Icirc;", "&#206;")
                sString = Replace(sString, "&Iuml;", "&#207;")
                sString = Replace(sString, "&ETH;", "&#208;")
                sString = Replace(sString, "&Ntilde;", "&#209;")
                sString = Replace(sString, "&Ograve;", "&#210;")
                sString = Replace(sString, "&Oacute;", "&#211;")
                sString = Replace(sString, "&Ocirc;", "&#212;")
                sString = Replace(sString, "&Otilde;", "&#213;")
                sString = Replace(sString, "&Ouml;", "&#214;")
                sString = Replace(sString, "&times;", "&#215;")
                sString = Replace(sString, "&Oslash;", "&#216;")
                sString = Replace(sString, "&Ugrave;", "&#217;")
                sString = Replace(sString, "&Uacute;", "&#218;")
                sString = Replace(sString, "&Ucirc;", "&#219;")
                sString = Replace(sString, "&Uuml;", "&#220;")
                sString = Replace(sString, "&Yacute;", "&#221;")
                sString = Replace(sString, "&THORN;", "&#222;")
                sString = Replace(sString, "&szlig;", "&#223;")
                sString = Replace(sString, "&agrave;", "&#224;")
                sString = Replace(sString, "&aacute;", "&#225;")
                sString = Replace(sString, "&acirc;", "&#226;")
                sString = Replace(sString, "&atilde;", "&#227;")
                sString = Replace(sString, "&auml;", "&#228;")
                sString = Replace(sString, "&aring;", "&#229;")
                sString = Replace(sString, "&aelig;", "&#230;")
                sString = Replace(sString, "&ccedil;", "&#231;")
                sString = Replace(sString, "&egrave;", "&#232;")
                sString = Replace(sString, "&eacute;", "&#233;")
                sString = Replace(sString, "&ecirc;", "&#234;")
                sString = Replace(sString, "&euml;", "&#235;")
                sString = Replace(sString, "&igrave;", "&#236;")
                sString = Replace(sString, "&iacute;", "&#237;")
                sString = Replace(sString, "&icirc;", "&#238;")
                sString = Replace(sString, "&iuml;", "&#239;")
                sString = Replace(sString, "&eth;", "&#240;")
                sString = Replace(sString, "&ntilde;", "&#241;")
                sString = Replace(sString, "&ograve;", "&#242;")
                sString = Replace(sString, "&oacute;", "&#243;")
                sString = Replace(sString, "&ocirc;", "&#244;")
                sString = Replace(sString, "&otilde;", "&#245;")
                sString = Replace(sString, "&ouml;", "&#246;")
                sString = Replace(sString, "&divide;", "&#247;")
                sString = Replace(sString, "&oslash;", "&#248;")
                sString = Replace(sString, "&ugrave;", "&#249;")
                sString = Replace(sString, "&uacute;", "&#250;")
                sString = Replace(sString, "&ucirc;", "&#251;")
                sString = Replace(sString, "&uuml;", "&#252;")
                sString = Replace(sString, "&yacute;", "&#253;")
                sString = Replace(sString, "&thorn;", "&#254;")
                sString = Replace(sString, "&yuml;", "&#255;")
                sString = Replace(sString, "&OElig;", "&#338;")
                sString = Replace(sString, "&oelig;", "&#339;")
                sString = Replace(sString, "&Scaron;", "&#352;")
                sString = Replace(sString, "&scaron;", "&#353;")
                sString = Replace(sString, "&Yuml;", "&#376;")
                sString = Replace(sString, "&fnof;", "&#402;")
                sString = Replace(sString, "&circ;", "&#710;")
                sString = Replace(sString, "&tilde;", "&#732;")
                sString = Replace(sString, "&Alpha;", "&#913;")
                sString = Replace(sString, "&Beta;", "&#914;")
                sString = Replace(sString, "&Gamma;", "&#915;")
                sString = Replace(sString, "&Delta;", "&#916;")
                sString = Replace(sString, "&Epsilon;", "&#917;")
                sString = Replace(sString, "&Zeta;", "&#918;")
                sString = Replace(sString, "&Eta;", "&#919;")
                sString = Replace(sString, "&Theta;", "&#920;")
                sString = Replace(sString, "&Iota;", "&#921;")
                sString = Replace(sString, "&Kappa;", "&#922;")
                sString = Replace(sString, "&Lambda;", "&#923;")
                sString = Replace(sString, "&Mu;", "&#924;")
                sString = Replace(sString, "&Nu;", "&#925;")
                sString = Replace(sString, "&Xi;", "&#926;")
                sString = Replace(sString, "&Omicron;", "&#927;")
                sString = Replace(sString, "&Pi;", "&#928;")
                sString = Replace(sString, "&Rho;", "&#929;")
                sString = Replace(sString, "&Sigma;", "&#931;")
                sString = Replace(sString, "&Tau;", "&#932;")
                sString = Replace(sString, "&Upsilon;", "&#933;")
                sString = Replace(sString, "&Phi;", "&#934;")
                sString = Replace(sString, "&Chi;", "&#935;")
                sString = Replace(sString, "&Psi;", "&#936;")
                sString = Replace(sString, "&Omega;", "&#937;")
                sString = Replace(sString, "&alpha;", "&#945;")
                sString = Replace(sString, "&beta;", "&#946;")
                sString = Replace(sString, "&gamma;", "&#947;")
                sString = Replace(sString, "&delta;", "&#948;")
                sString = Replace(sString, "&epsilon;", "&#949;")
                sString = Replace(sString, "&zeta;", "&#950;")
                sString = Replace(sString, "&eta;", "&#951;")
                sString = Replace(sString, "&theta;", "&#952;")
                sString = Replace(sString, "&iota;", "&#953;")
                sString = Replace(sString, "&kappa;", "&#954;")
                sString = Replace(sString, "&lambda;", "&#955;")
                sString = Replace(sString, "&mu;", "&#956;")
                sString = Replace(sString, "&nu;", "&#957;")
                sString = Replace(sString, "&xi;", "&#958;")
                sString = Replace(sString, "&omicron;", "&#959;")
                sString = Replace(sString, "&pi;", "&#960;")
                sString = Replace(sString, "&rho;", "&#961;")
                sString = Replace(sString, "&sigmaf;", "&#962;")
                sString = Replace(sString, "&sigma;", "&#963;")
                sString = Replace(sString, "&tau;", "&#964;")
                sString = Replace(sString, "&upsilon;", "&#965;")
                sString = Replace(sString, "&phi;", "&#966;")
                sString = Replace(sString, "&chi;", "&#967;")
                sString = Replace(sString, "&psi;", "&#968;")
                sString = Replace(sString, "&omega;", "&#969;")
                sString = Replace(sString, "&thetasym;", "&#977;")
                sString = Replace(sString, "&upsih;", "&#978;")
                sString = Replace(sString, "&piv;", "&#982;")
                sString = Replace(sString, "&ensp;", "&#8194;")
                sString = Replace(sString, "&emsp;", "&#8195;")
                sString = Replace(sString, "&thinsp;", "&#8201;")
                sString = Replace(sString, "&zwnj;", "&#8204;")
                sString = Replace(sString, "&zwj;", "&#8205;")
                sString = Replace(sString, "&lrm;", "&#8206;")
                sString = Replace(sString, "&rlm;", "&#8207;")
                sString = Replace(sString, "&ndash;", "&#8211;")
                sString = Replace(sString, "&mdash;", "&#8212;")
                sString = Replace(sString, "&lsquo;", "&#8216;")
                sString = Replace(sString, "&rsquo;", "&#8217;")
                sString = Replace(sString, "&sbquo;", "&#8218;")
                sString = Replace(sString, "&ldquo;", "&#8220;")
                sString = Replace(sString, "&rdquo;", "&#8221;")
                sString = Replace(sString, "&bdquo;", "&#8222;")
                sString = Replace(sString, "&dagger;", "&#8224;")
                sString = Replace(sString, "&Dagger;", "&#8225;")
                sString = Replace(sString, "&bull;", "&#8226;")
                sString = Replace(sString, "&hellip;", "&#8230;")
                sString = Replace(sString, "&permil;", "&#8240;")
                sString = Replace(sString, "&prime;", "&#8242;")
                sString = Replace(sString, "&Prime;", "&#8243;")
                sString = Replace(sString, "&lsaquo;", "&#8249;")
                sString = Replace(sString, "&rsaquo;", "&#8250;")
                sString = Replace(sString, "&oline;", "&#8254;")
                sString = Replace(sString, "&frasl;", "&#8260;")
                sString = Replace(sString, "&euro;", "&#8364;")
                sString = Replace(sString, "&image;", "&#8465;")
                sString = Replace(sString, "&weierp;", "&#8472;")
                sString = Replace(sString, "&real;", "&#8476;")
                sString = Replace(sString, "&trade;", "&#8482;")
                sString = Replace(sString, "&alefsym;", "&#8501;")
                sString = Replace(sString, "&larr;", "&#8592;")
                sString = Replace(sString, "&uarr;", "&#8593;")
                sString = Replace(sString, "&rarr;", "&#8594;")
                sString = Replace(sString, "&darr;", "&#8595;")
                sString = Replace(sString, "&harr;", "&#8596;")
                sString = Replace(sString, "&crarr;", "&#8629;")
                sString = Replace(sString, "&lArr;", "&#8656;")
                sString = Replace(sString, "&uArr;", "&#8657;")
                sString = Replace(sString, "&rArr;", "&#8658;")
                sString = Replace(sString, "&dArr;", "&#8659;")
                sString = Replace(sString, "&hArr;", "&#8660;")
                sString = Replace(sString, "&forall;", "&#8704;")
                sString = Replace(sString, "&part;", "&#8706;")
                sString = Replace(sString, "&exist;", "&#8707;")
                sString = Replace(sString, "&empty;", "&#8709;")
                sString = Replace(sString, "&nabla;", "&#8711;")
                sString = Replace(sString, "&isin;", "&#8712;")
                sString = Replace(sString, "&notin;", "&#8713;")
                sString = Replace(sString, "&ni;", "&#8715;")
                sString = Replace(sString, "&prod;", "&#8719;")
                sString = Replace(sString, "&sum;", "&#8721;")
                sString = Replace(sString, "&minus;", "&#8722;")
                sString = Replace(sString, "&lowast;", "&#8727;")
                sString = Replace(sString, "&radic;", "&#8730;")
                sString = Replace(sString, "&prop;", "&#8733;")
                sString = Replace(sString, "&infin;", "&#8734;")
                sString = Replace(sString, "&ang;", "&#8736;")
                sString = Replace(sString, "&and;", "&#8743;")
                sString = Replace(sString, "&or;", "&#8744;")
                sString = Replace(sString, "&cap;", "&#8745;")
                sString = Replace(sString, "&cup;", "&#8746;")
                sString = Replace(sString, "&int;", "&#8747;")
                sString = Replace(sString, "&there4;", "&#8756;")
                sString = Replace(sString, "&sim;", "&#8764;")
                sString = Replace(sString, "&cong;", "&#8773;")
                sString = Replace(sString, "&asymp;", "&#8776;")
                sString = Replace(sString, "&ne;", "&#8800;")
                sString = Replace(sString, "&equiv;", "&#8801;")
                sString = Replace(sString, "&le;", "&#8804;")
                sString = Replace(sString, "&ge;", "&#8805;")
                sString = Replace(sString, "&sub;", "&#8834;")
                sString = Replace(sString, "&sup;", "&#8835;")
                sString = Replace(sString, "&nsub;", "&#8836;")
                sString = Replace(sString, "&sube;", "&#8838;")
                sString = Replace(sString, "&supe;", "&#8839;")
                sString = Replace(sString, "&oplus;", "&#8853;")
                sString = Replace(sString, "&otimes;", "&#8855;")
                sString = Replace(sString, "&perp;", "&#8869;")
                sString = Replace(sString, "&sdot;", "&#8901;")
                sString = Replace(sString, "&lceil;", "&#8968;")
                sString = Replace(sString, "&rceil;", "&#8969;")
                sString = Replace(sString, "&lfloor;", "&#8970;")
                sString = Replace(sString, "&rfloor;", "&#8971;")
                sString = Replace(sString, "&lang;", "&#9001;")
                sString = Replace(sString, "&rang;", "&#9002;")
                sString = Replace(sString, "&loz;", "&#9674;")
                sString = Replace(sString, "&spades;", "&#9824;")
                sString = Replace(sString, "&clubs;", "&#9827;")
                sString = Replace(sString, "&hearts;", "&#9829;")
                sString = Replace(sString, "&diams;", "&#9830;")

                If sString Is Nothing Then
                    Return ""
                Else
                    sString = Regex.Replace(sString, "&(?!#?\w+;)", "&amp;")
                    Return sString
                End If


            End If

        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "convertEntitiesToCodes", ex, ""))
            Return ""
        End Try
    End Function

    Public Shared Function convertEntitiesToCodesFast(ByVal sString As String) As String
        Try
            Dim startString As String = sString
            If sString Is Nothing Then
                Return ""
            Else

                sString = Replace(sString, "Xmlns=""""", "")

                Dim chars As Char() = sString.ToCharArray()
                Dim result As StringBuilder = New StringBuilder(sString.Length + CInt((sString.Length * 0.1)))
                For Each c As Char In chars
                    Dim value As Integer = Convert.ToInt32(c)
                    If value > 127 Then result.AppendFormat("&#{0};", value) Else result.Append(c)
                Next

                sString = result.ToString()

                If sString Is Nothing Then
                    Return ""
                Else
                    sString = Regex.Replace(sString, "&(?!#?\w+;)", "&amp;")
                    Return sString
                End If


            End If

        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "convertEntitiesToCodes", ex, ""))
            Return ""
        End Try
    End Function


    Public Shared Function convertEntitiesToString(ByVal sString As String) As String
        Try

            sString = sString.Replace("&amp;", "&")
            sString = sString.Replace("&lt;", "<")
            sString = sString.Replace("&gt;", ">")

            Return IIf(sString Is Nothing, "", sString)

        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "convertEntitiesToString", ex, ""))
            Return ""
        End Try
    End Function

    Public Shared Function XmlDate(ByVal dDate As Object, Optional ByVal bIncludeTime As Boolean = False) As String
        Try
            Dim cReturn As String

            If IsDBNull(dDate) Or Not (IsDate(dDate)) Then
                cReturn = ""
            Else
                Dim cFormat As String = "yyyy-MM-dd"
                If bIncludeTime Then cFormat += "THH:mm:ss"
                cReturn = CDate(dDate).ToString(cFormat)
            End If
            Return cReturn
        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "XmlDate", ex, ""))
            Return ""
        End Try

    End Function

    Public Shared Function GenerateConcatForXPath(ByVal a_xPathQueryString As String) As String
        Dim returnString As String = String.Empty
        Dim searchString As String = a_xPathQueryString
        Dim quoteChars As Char() = New Char() {"'"c, """"c}

        Dim quotePos As Integer = searchString.IndexOfAny(quoteChars)
        If quotePos = -1 Then
            returnString = "'" + searchString + "'"
        Else
            returnString = "concat("
            While quotePos <> -1
                Dim subString As String = searchString.Substring(0, quotePos)
                returnString += "'" + subString + "', "
                If searchString.Substring(quotePos, 1) = "'" Then
                    returnString += """'"", "
                Else
                    'must be a double quote 
                    returnString += "'""', "
                End If
                searchString = searchString.Substring(quotePos + 1, searchString.Length - quotePos - 1)
                quotePos = searchString.IndexOfAny(quoteChars)
            End While
            returnString += "'" + searchString + "')"
        End If
        Return returnString
    End Function

    Public Shared Function encodeAllHTML(ByVal shtml As String)

        shtml = Replace(shtml, "&", "&amp;")
        shtml = Replace(shtml, """", "&quot;")
        shtml = Replace(shtml, "<", "&lt;")
        shtml = Replace(shtml, ">", "&gt;")
        Return shtml

    End Function

    Public Shared Function CombineNodes(ByVal oMasterStructureNode As XmlElement, ByVal oExistingDataNode As XmlElement) As XmlElement
        'combines an existing instance with a new 
        Try
            'firstly we will go through and find like for like
            'we will skip the immediate node below instance as this is usually fairly static
            'except for attributes

            'Attributes of Root
            Dim oMasterFirstElement As XmlElement = firstElement(oMasterStructureNode)
            Dim oExistingFirstElement As XmlElement = firstElement(oExistingDataNode)

            If oMasterFirstElement IsNot Nothing Then

                If oMasterFirstElement.HasAttributes Then
                    For i As Integer = 0 To oMasterFirstElement.Attributes.Count - 1
                        Dim oMainAtt As XmlAttribute = oMasterFirstElement.Attributes(i)
                        If Not oExistingDataNode.Attributes.ItemOf(oMainAtt.Name).Value = "" Then
                            oMainAtt.Value = oExistingDataNode.Attributes.ItemOf(oMainAtt.Name).Value
                        End If
                    Next
                End If

                'Navigate Down Elmts and find with same xpath

                For Each oMainElmt As XmlElement In oMasterFirstElement.SelectNodes("*")
                    CombineInstance_Sub1(oMainElmt, oExistingDataNode, oMasterFirstElement.Name)
                Next

                'Now for any existing stuff that has not been brought across
                If oExistingFirstElement IsNot Nothing Then
                    For Each oExistElmt As XmlElement In oExistingFirstElement.SelectNodes("descendant-or-self::*[not(@ewCombineInstanceFound)]")
                        If Not oExistElmt.OuterXml = oExistingFirstElement.OuterXml Then
                            CombineInstance_Sub2(oExistElmt, oMasterStructureNode, oMasterFirstElement.Name)
                        End If
                    Next
                End If


                CombineInstance_MarkSubElmts(oMasterStructureNode, True)
            End If

            Return oMasterStructureNode
        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CombineNodes", ex, ""))
            Return oMasterStructureNode
        End Try
    End Function

    Shared Function GetOrCreateSingleChildElement(ByRef parentNode As XmlNode, ByVal nodeName As String) As XmlElement
        Try
            Dim returnNode As XmlElement = parentNode.SelectSingleNode(nodeName)
            If returnNode Is Nothing Then
                returnNode = parentNode.OwnerDocument.CreateElement(nodeName)
                parentNode.AppendChild(returnNode)
            End If

            Return returnNode

        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetOrCreateSingleChildElement", ex, ""))
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Attempts to set an element's inner xml from a string - if it fails it will set the inner text instead
    ''' </summary>
    ''' <param name="element">the element to set</param>
    ''' <param name="value">the string to try and set the element for</param>
    ''' <remarks></remarks>
    Public Shared Sub SetInnerXmlThenInnerText(ByRef element As XmlElement, ByVal value As String)

        If element IsNot Nothing Then
            Try

                element.InnerXml = value

            Catch ex As XmlException

                element.InnerText = value

            Catch ex As Exception

                RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SetInnerXmlThenInnerText", ex, ""))

            End Try
        End If



    End Sub

    Public Shared Function XmltoDictionary(oXml As XmlElement) As System.Collections.Generic.Dictionary(Of String, String)
        Try

            Dim myDict = New System.Collections.Generic.Dictionary(Of String, String)

            XmltoDictionaryNode(oXml, myDict, oXml.Name())

            Return myDict

        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "XmltoDictionary", ex, ""))
        End Try
    End Function

    Private Shared Sub XmltoDictionaryNode(oThisElmt As XmlElement, ByRef myDict As Dictionary(Of String, String), ByVal Prefix As String)
        Dim oElmt As XmlElement
        Try
            For Each Attribute As XmlAttribute In oThisElmt.Attributes
                myDict.Add(Prefix & ".-" & Attribute.Name, Attribute.Value)
            Next
            If oThisElmt.SelectNodes("*").Count = 0 Then
                If oThisElmt.InnerText <> "" And Not (myDict.ContainsKey(Prefix & "." & oThisElmt.Name)) Then
                    myDict.Add(Prefix, oThisElmt.InnerText)
                End If
            Else
                For Each oElmt In oThisElmt.SelectNodes("*")
                    XmltoDictionaryNode(oElmt, myDict, Prefix & "." & oElmt.Name())
                Next
            End If
        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "XmltoDictionaryNode", ex, ""))
        End Try
    End Sub

#End Region




    ''' <summary>
    ''' Serializes an object and returns the serialization as a string representation of UTF-8 encoded XML
    ''' </summary>
    ''' <typeparam name="T">The type of object to serialize</typeparam>
    ''' <param name="value">The object to serialize</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function Serialize(Of T)(ByVal value As T) As String

        Dim serializer As New XmlSerializer(GetType(T))
        Dim mStream As New IO.MemoryStream()
        Dim settings As New XmlWriterSettings()
        settings.Encoding = New UTF8Encoding(False)
        settings.Indent = False
        settings.OmitXmlDeclaration = False
        Dim xWriter As XmlWriter = XmlWriter.Create(mStream, settings)
        serializer.Serialize(xWriter, value)

        Dim serializedString As String = Tools.Text.UTF8ByteArrayToString(mStream.ToArray())
        mStream.Close()

        Return serializedString

    End Function

    Public Shared Function SerializeToXml(Of T)(ByVal value As T) As XmlNode
        Dim cProcessInfo As String = ""
        Try

            ' Get the serialized object
            Dim serializedObject As String = Serialize(Of T)(value)

            cProcessInfo = serializedObject

            If Not serializedObject = Nothing Then
                ' Remove the namespaces
                serializedObject = System.Text.RegularExpressions.Regex.Replace(serializedObject, "xmlns(:\w+?)?="".*?""", "")
                serializedObject = System.Text.RegularExpressions.Regex.Replace(serializedObject, "<\?xml.+\?>", "")
            End If

            ' Remove the declaration

            ' Load into XML
            Dim serializedObjectDocument As New XmlDocument
            serializedObjectDocument.LoadXml(serializedObject)

            Return serializedObjectDocument.DocumentElement

        Catch ex As Exception
            RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Read", ex, cProcessInfo))
            Return Nothing
        End Try

    End Function

    Public Shared Sub AddExistingNode(ByRef nodeToAddTo As XmlElement, ByVal nodeToBeAdded As XmlNode)
        nodeToAddTo.AppendChild(nodeToAddTo.OwnerDocument.ImportNode(nodeToBeAdded, True))
    End Sub

    Public Shared Sub AddExistingNode(ByRef nodeToAddTo As XmlNode, ByVal nodeToBeAdded As XmlNode)
        nodeToAddTo.AppendChild(nodeToAddTo.OwnerDocument.ImportNode(nodeToBeAdded, True))
    End Sub



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
            If prefix.CompareTo("Xmlns") = 0 Or localName.CompareTo("xmlns") = 0 Then
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

    Public Class XmlSanitizingStream
        Inherits System.IO.StreamReader


        Public Shared Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
        Private Const mcModuleName As String = "Protean.Tools.Xml.XmlSanitizingStream"


        ' Pass 'true' to automatically detect encoding using BOMs.
        ' BOMs: http://en.wikipedia.org/wiki/Byte-order_mark

        Public Sub New(streamToSanitize As System.IO.Stream)
            MyBase.New(streamToSanitize, True)
        End Sub

        ''' <summary>
        ''' Whether a given character is allowed by XML 1.0.
        ''' </summary>
        Public Shared Function IsLegalXmlChar(character As Integer) As Boolean
            ' == '\t' == 9   
            ' == '\n' == 10  
            ' == '\r' == 13  
            Return (character = &H9 OrElse character = &HA OrElse character = &HD OrElse (character >= &H20 AndAlso character <= &HD7FF) OrElse (character >= &HE000 AndAlso character <= &HFFFD) OrElse (character >= &H10000 AndAlso character <= &H10FFFF))
        End Function

        Private Const EOF As Integer = -1

        Public Overrides Function Read() As Integer
            ' Read each char, skipping ones XML has prohibited

            Dim nextCharacter As Integer

            Do
                ' Read a character

                If (InlineAssignHelper(nextCharacter, MyBase.Read())) = EOF Then
                    ' If the char denotes end of file, stop
                    Exit Do
                End If

                ' Skip char if it's illegal, and try the next

            Loop While Not XmlSanitizingStream.IsLegalXmlChar(nextCharacter)

            Return nextCharacter
        End Function

        Public Overrides Function Peek() As Integer
            ' Return next legal XML char w/o reading it 

            Dim nextCharacter As Integer

            Do
                ' See what the next character is 
                nextCharacter = MyBase.Peek()
                ' If it's illegal, skip over 
                ' and try the next.

            Loop While Not XmlSanitizingStream.IsLegalXmlChar(nextCharacter) AndAlso (InlineAssignHelper(nextCharacter, MyBase.Read())) <> EOF

            Return nextCharacter

        End Function

        'Public Overrides Function Read(buffer As Char(), index As Integer, count As Integer) As Integer
        '    Dim cProcessInfo As String = Nothing
        '    Try
        '        If buffer Is Nothing Then
        '            Throw New ArgumentNullException("buffer")
        '        End If
        '        If index < 0 Then
        '            Throw New ArgumentOutOfRangeException("index")
        '        End If
        '        If count < 0 Then
        '            Throw New ArgumentOutOfRangeException("count")
        '        End If
        '        If (buffer.Length - index) < count Then
        '            Throw New ArgumentException()
        '        End If
        '        Dim num As Integer = 0
        '        Do
        '            Dim num2 As Integer = Me.Read()
        '            If num2 = -1 Then
        '                Return num
        '            End If
        '            cProcessInfo = buffer.Length
        '            buffer(index + System.Math.Max(System.Threading.Interlocked.Increment(num), num - 1)) = ChrW(num2)
        '        Loop While num < count
        '        Return num
        '    Catch ex As Exception
        '        RaiseEvent OnError(Nothing, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Read", ex, cProcessInfo))
        '        Return Nothing
        '    End Try
        'End Function

        Public Overrides Function ReadBlock(buffer As Char(), index As Integer, count As Integer) As Integer
            Dim num As Integer
            Dim num2 As Integer = 0
            Do
                num2 += InlineAssignHelper(num, Me.Read(buffer, index + num2, count - num2))
            Loop While (num > 0) AndAlso (num2 < count)
            Return num2
        End Function

        Public Overrides Function ReadLine() As String
            Dim builder As New StringBuilder()
            While True
                Dim num As Integer = Me.Read()
                Select Case num
                    Case -1
                        If builder.Length > 0 Then
                            Return builder.ToString()
                        End If
                        Return Nothing

                    Case 13, 10
                        If (num = 13) AndAlso (Me.Peek() = 10) Then
                            Me.Read()
                        End If
                        Return builder.ToString()
                    Case Else
                        Return Nothing
                End Select
                builder.Append(ChrW(num))
            End While
        End Function

        Public Overrides Function ReadToEnd() As String
            Dim num As Integer
            Dim buffer As Char() = New Char(4095) {}
            Dim builder As New StringBuilder(&H1000)
            While (InlineAssignHelper(num, Me.Read(buffer, 0, buffer.Length))) <> 0
                builder.Append(buffer, 0, num)
            End While
            Return builder.ToString()
        End Function

        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
            target = value
            Return value
        End Function

    End Class

End Class

Namespace Xslt

    Public Class XsltFunctions

        Public Function stringcompare(ByVal stringX As String, ByVal stringY As String) As Integer

            stringX = UCase(stringX)
            stringY = UCase(stringY)

            If stringX = stringY Then
                Return 0
            ElseIf stringX < stringY Then
                Return -1
            ElseIf stringX > stringY Then
                Return 1
            End If

        End Function

        Public Function replacestring(ByVal text As String, ByVal replace As String, ByVal replaceWith As String) As String
            Try
                Return text.Replace(replace, replaceWith)
            Catch ex As Exception
                Return text
            End Try
        End Function

        Public Function getdate(ByVal dateString As String) As String
            Try
                Select Case dateString
                    Case "now()", "Now()", "now", "Now"
                        dateString = CStr(Protean.Tools.Xml.XmlDate(Now(), True))
                End Select
                Return dateString
            Catch ex As Exception
                Return dateString
            End Try
        End Function


        Public Function formatdate(ByVal dateString As String, ByVal dateFormat As String) As String
            Try
                If IsDate(dateString) Then
                    dateString = New Date(dateString).ToString(dateFormat)
                End If
                Return dateString
            Catch ex As Exception
                Return dateString
            End Try
        End Function

        Public Function textafterlast(ByVal text As String, ByVal search As String) As String
            Try
                If text.LastIndexOf(search) > 0 Then
                    text = text.Substring(text.LastIndexOf(search) + search.Length())
                End If
                Return text
            Catch ex As Exception
                Return text
            End Try
        End Function

        Public Function textbeforelast(ByVal text As String, ByVal search As String) As String
            Try
                If text.LastIndexOf(search) > 0 Then
                    text = text.Substring(0, text.LastIndexOf(search))
                End If
                Return text
            Catch ex As Exception
                Return text
            End Try
        End Function

        Public Function randomnumber(ByVal min As Integer, ByVal max As Integer) As Integer
            Try
                Dim oRand As New Random
                Return oRand.Next(min, max)
            Catch ex As Exception
                Return min
            End Try
        End Function

        Public Function datediff(ByVal date1String As String, ByVal date2String As String, ByVal datePart As String) As String
            Dim nDiff As String = ""
            Try
                Dim ValidDatePart As String() = {"d", "y", "h", "n", "m", "q", "s", "w", "ww", "yyyy"}
                If IsDate(date1String) _
                    AndAlso IsDate(date2String) _
                    AndAlso Array.IndexOf(ValidDatePart, datePart) > (ValidDatePart.GetLowerBound(0) - 1) Then

                    nDiff = Microsoft.VisualBasic.DateDiff(datePart, CDate(date1String), CDate(date2String)).ToString()
                End If
                Return nDiff
            Catch ex As Exception
                Return nDiff
            End Try
        End Function

   




    End Class

    Public Class Transform

#Region "    Declarations"
        Private cFileLocation As String = "" 'Xsl File path
        Private cXslText As String = "" 'Xsl Blob
        Private oXml As System.Xml.XPath.IXPathNavigable 'Xml Document
        Private bCompiled As Boolean  'If we want it to be compiled
        Private nTimeoutMillisec As Integer = 0 'Timeout in milliseconds (0 means not timed)
        Private bDebuggable As Boolean = False 'If we want to be able to step through the code
        Private bClearXml As Boolean = True 'Do we want to clear the Xml object after a transform (to reduce object size for caching)
        Private bClearXmlDec As Boolean = True 'Remove Xml Declaration

        Private oXslTExtensionsObject As Object 'Any Class we want to use for additional functionality
        Private cXslTExtensionsURN As String 'The Urn for the class

        Private oClassicTransform As Xsl.XslTransform ' Classic Transform Object
        Private oCompiledTransform As Xsl.XslCompiledTransform 'Compiled Transform Object
        Private XsltArgs As Xsl.XsltArgumentList 'Argurment List
        'Error Handling
        'General Error
        Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
        'Private TimeOutError
        Private Event OnTimeoutError_Private(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
        'Public TimeOutError
        Public Event OnTimeoutError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)

        Private oXslReader As XmlReader 'to read the Xsl
        Private oXslReaderSettings As XmlReaderSettings

        Private Const mcModuleName As String = "Protean.Tools.Xml.XslTransform"

        Private Sub EonicPrivateError_Handle(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs) Handles Me.OnTimeoutError_Private
            Try
                'make sure we close the reader properly
                oXslReader.Close()
            Catch ex As Exception
                'dont do anyrhing
            End Try
            'raise the public timeout event
            RaiseEvent OnTimeoutError(sender, e)
        End Sub


#End Region

#Region "    Properties"

        Public Property XslTFile() As String
            Get
                Return cFileLocation
            End Get
            Set(ByVal value As String)
                cFileLocation = value
            End Set
        End Property

        Public Property XslText() As String
            Get
                Return cXslText
            End Get
            Set(ByVal value As String)
                cXslText = value
            End Set
        End Property

        Public Property Compiled() As Boolean
            Get
                Return bCompiled
            End Get
            Set(ByVal value As Boolean)
                bCompiled = value
            End Set
        End Property

        Public Property TimeoutSeconds() As Integer
            Get
                Dim oTimeSpan As New TimeSpan(0, 0, 0, 0, nTimeoutMillisec)
                Return oTimeSpan.TotalSeconds
            End Get
            Set(ByVal value As Integer)
                Dim oTimeSpan As New TimeSpan(0, 0, 0, value)
                nTimeoutMillisec = oTimeSpan.TotalMilliseconds
            End Set
        End Property

        Public Property Debuggable() As Boolean
            Get
                Return bDebuggable
            End Get
            Set(ByVal value As Boolean)
                bDebuggable = value
            End Set
        End Property

        Public Property Xml() As System.Xml.XPath.IXPathNavigable
            Get
                Return oXml
            End Get
            Set(ByVal value As System.Xml.XPath.IXPathNavigable)
                oXml = value
            End Set
        End Property

        Public Property XslTExtensionObject() As Object
            Get
                Return oXslTExtensionsObject
            End Get
            Set(ByVal value As Object)
                oXslTExtensionsObject = value
            End Set
        End Property

        Public Property XslTExtensionURN() As String
            Get
                Return cXslTExtensionsURN
            End Get
            Set(ByVal value As String)
                cXslTExtensionsURN = value
                If Not cXslTExtensionsURN.Contains("urn:") Then cXslTExtensionsURN = "urn:" & value
            End Set
        End Property

        Public Property ClearXmlAfterTransform() As Boolean
            Get
                Return bClearXml
            End Get
            Set(ByVal value As Boolean)
                bClearXml = value
            End Set
        End Property

        Public Property RemoveXmlDeclaration() As Boolean
            Get
                Return bClearXmlDec
            End Get
            Set(ByVal value As Boolean)
                bClearXmlDec = value
            End Set
        End Property
#End Region

#Region "       Compiled"
        Private Delegate Sub TransformCompiledDelegate(ByRef cResult As String)

        Private Sub ProcessCompiledTimed(ByRef cResult As String)
            Try
                Dim d As New TransformCompiledDelegate(AddressOf ProcessCompiled)
                Dim res As IAsyncResult = d.BeginInvoke(cResult, Nothing, Nothing)
                If res.IsCompleted = False Then
                    res.AsyncWaitHandle.WaitOne(nTimeoutMillisec, False)
                    If res.IsCompleted = False Then
                        d.EndInvoke(cResult, DirectCast(res, Runtime.Remoting.Messaging.AsyncResult))
                        d = Nothing
                        Dim oSettings As New Hashtable
                        oSettings.Add("TimeoutSeconds", Me.TimeoutSeconds)
                        oSettings.Add("Compiled", Me.Compiled)
                        RaiseEvent OnTimeoutError_Private(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ProcessCompiledTimed", New Exception("XsltTransformTimeout"), "", , oSettings))
                    End If
                End If
                If Not d Is Nothing Then d.EndInvoke(cResult, DirectCast(res, Runtime.Remoting.Messaging.AsyncResult))
            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ProcessCompiledTimed", ex, ""))
            End Try
        End Sub

        Private Sub ProcessCompiled(ByRef cResult As String)
            Try
                'make a resolver/setting
                Dim resolver As New XmlUrlResolver()
                resolver.Credentials = System.Net.CredentialCache.DefaultCredentials

                'create the transform object
                If oCompiledTransform Is Nothing Then
                    'if it is not nothing we will have already used it "cache"
                    oCompiledTransform = New Xsl.XslCompiledTransform(bDebuggable)
                    'get the Xsl
                    If Not cFileLocation = "" Then
                        oXslReader = XmlReader.Create(cFileLocation, oXslReaderSettings)
                        oCompiledTransform.Load(oXslReader, Xsl.XsltSettings.TrustedXslt, resolver)
                        oXslReader.Close()
                    Else
                        Dim oXsl As New XmlDocument
                        oXsl.InnerXml = cXslText
                        oCompiledTransform.Load(oXsl, Xsl.XsltSettings.TrustedXslt, resolver)
                    End If
                    'any vb functions we want to use make an extension object
                    XsltArgs = New Xsl.XsltArgumentList
                    If Not oXslTExtensionsObject Is Nothing And Not cXslTExtensionsURN = "" Then
                        XsltArgs.AddExtensionObject(cXslTExtensionsURN, oXslTExtensionsObject)
                    End If
                End If
                'make a writer
                Dim osWriter As IO.TextWriter = New IO.StringWriter
                'transform
                'Change the XmlDocument to xPathDocument to improve performance

                Dim xpathDoc As XPath.XPathDocument = New XPath.XPathDocument(New XmlNodeReader(oXml))
                oCompiledTransform.Transform(xpathDoc, XsltArgs, osWriter)
                'Return results
                cResult = osWriter.ToString
            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "TransformCompiled", ex, ""))
                cResult = ""
            End Try
        End Sub
#End Region

#Region "       Classic"
        Private Delegate Sub TransformClassicDelegate(ByRef cResult As String)

        Private Sub ProcessClassicTimed(ByRef cResult As String)
            Dim sProcessInfo As String = ""
            Try
                Dim d As New TransformClassicDelegate(AddressOf ProcessClassic)
                Dim res As IAsyncResult = d.BeginInvoke(cResult, Nothing, Nothing)
                If res.IsCompleted = False Then
                    res.AsyncWaitHandle.WaitOne(nTimeoutMillisec, False)
                    If res.IsCompleted = False Then
                        d.EndInvoke(cResult, DirectCast(res, Runtime.Remoting.Messaging.AsyncResult))
                        d = Nothing
                        Dim oSettings As New Hashtable
                        oSettings.Add("TimeoutSeconds", Me.TimeoutSeconds)
                        oSettings.Add("Compiled", Me.Compiled)
                        RaiseEvent OnTimeoutError_Private(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ProcessClassicTimed", New Exception("XsltTransformTimeout"), "", , oSettings))
                    End If
                End If
                If Not d Is Nothing Then d.EndInvoke(cResult, DirectCast(res, Runtime.Remoting.Messaging.AsyncResult))
            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ProcessClassicTimed", ex, ""))
            End Try
        End Sub

        Private Sub ProcessClassic(ByRef cResult As String)
            Try
                'make a resolver/setting
                Dim resolver As New XmlUrlResolver()
                resolver.Credentials = System.Net.CredentialCache.DefaultCredentials

                'create the transform object

                oClassicTransform = New Xsl.XslTransform()
                'get the Xsl
                If Not cFileLocation = "" Then
                    oXslReader = XmlReader.Create(cFileLocation, oXslReaderSettings)
                    oClassicTransform.Load(oXslReader, resolver)
                    oXslReader.Close()
                Else
                    Dim oXsl As New XmlDocument
                    oXsl.InnerXml = cXslText
                    oClassicTransform.Load(oXsl, resolver)
                End If
                'any vb functions we want to use make an extension object
                XsltArgs = New Xsl.XsltArgumentList
                If Not oXslTExtensionsObject Is Nothing And Not cXslTExtensionsURN = "" Then
                    XsltArgs.AddExtensionObject(cXslTExtensionsURN, oXslTExtensionsObject)
                End If
                'make a writer

                Dim osWriter As IO.TextWriter = New IO.StringWriter

                'transform
                'Change the XmlDocument to xPathDocument to improve performance
                Dim xpathDoc As XPath.XPathDocument = New XPath.XPathDocument(New XmlNodeReader(oXml))


                oClassicTransform.Transform(xpathDoc, XsltArgs, osWriter, Nothing)
                'Return results
                cResult = osWriter.ToString
            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "TransformClassic", ex, ""))
                cResult = ""
            End Try
        End Sub
#End Region

        Public Function Process() As String
            Try
                Dim cResult As String = ""
                If bCompiled And nTimeoutMillisec > 0 Then
                    ProcessCompiledTimed(cResult)
                ElseIf Not bCompiled And nTimeoutMillisec > 0 Then
                    ProcessClassicTimed(cResult)
                ElseIf bCompiled And nTimeoutMillisec = 0 Then
                    ProcessCompiled(cResult)
                ElseIf Not bCompiled And nTimeoutMillisec = 0 Then
                    ProcessClassic(cResult)
                End If
                If RemoveXmlDeclaration Then
                    Me.RemoveDeclaration(cResult)
                End If
                Return cResult
            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Transform", ex, ""))
                Return ""
            Finally
                If bClearXml Then oXml = Nothing
            End Try
        End Function

        Private Sub RemoveDeclaration(ByRef cResult As String)
            cResult = Replace(cResult, "<?Xml version=""1.0"" encoding=""utf-8""?>", "")
            cResult = Replace(cResult, "<?Xml version=""1.0"" encoding=""utf-16""?>", "")
            cResult = Trim(cResult)
        End Sub

        Public Sub New()
            Try
                oXslReaderSettings = New XmlReaderSettings
                oXslReaderSettings.ProhibitDtd = False
            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
            End Try
        End Sub
    End Class





End Namespace




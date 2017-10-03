Imports VB = Microsoft.VisualBasic
Imports System.Xml
Imports System.Web
Imports System.IO
Imports System.Xml.XPath
Imports System.Text.RegularExpressions
Imports System.Reflection
Imports System.Web.Configuration
Imports System.Globalization
Imports System.Globalization.CultureInfo
Imports System.Windows.Media.Imaging
Imports System
Imports System.Runtime.InteropServices

Imports BundleTransformer.Core.Builders
Imports BundleTransformer.Core.Orderers
Imports BundleTransformer.Core.Resolvers
Imports BundleTransformer.Core.Transformers

Imports System.Linq
Imports System.Collections.Generic

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
        Return Eonic.Tools.Xml.convertEntitiesToCodes(sString)

    End Function

    Public Function tidyXhtmlFrag(ByVal shtml As String, Optional ByVal bReturnNumbericEntities As Boolean = False, Optional ByVal bEncloseText As Boolean = True, Optional ByVal removeTags As String = "") As String

        '   PerfMon.Log("Web", "tidyXhtmlFrag")
        Dim sProcessInfo As String = "tidyXhtmlFrag"
        Dim sTidyXhtml As String = ""

        Try

            If Not removeTags = "" Then
                shtml = removeTagFromXml(shtml, removeTags)
            End If

            Using oTdyManaged As TidyManaged.Document = TidyManaged.Document.FromString(shtml)
                oTdyManaged.OutputBodyOnly = TidyManaged.AutoBool.Yes
                oTdyManaged.MakeClean = True
                oTdyManaged.DropFontTags = True
                oTdyManaged.ShowWarnings = True
                oTdyManaged.OutputXhtml = True

                If bReturnNumbericEntities Then
                    '    oTdyManaged.NumEntities = True
                End If
                oTdyManaged.CleanAndRepair()
                sTidyXhtml = oTdyManaged.Save()
                oTdyManaged.Dispose()
            End Using



            Return sTidyXhtml

        Catch ex As Exception
            ' It is the desired behaviour for this to return nothing if not valid html don't turn this on apart from in development.            Return Nothing
            ' Return ex.Message
            Return Nothing
        Finally

            sTidyXhtml = Nothing
        End Try
    End Function

    Public Function tidyXhtmlFragOld(ByVal shtml As String, Optional ByVal bReturnNumbericEntities As Boolean = False, Optional ByVal bEncloseText As Boolean = True, Optional ByVal removeTags As String = "") As String

        '   PerfMon.Log("Web", "tidyXhtmlFrag")
        Dim sProcessInfo As String = "tidyXhtmlFrag"
        Dim sTidyXhtml As String = ""
        Dim oTdy As TidyNet.Tidy = New TidyNet.Tidy
        Dim oTmc As New TidyNet.TidyMessageCollection
        Dim msIn As MemoryStream = New MemoryStream
        Dim msOut As MemoryStream = New MemoryStream
        Dim aBytes As Byte()

        Try

            If Not removeTags = "" Then
                shtml = removeTagFromXml(shtml, removeTags)
            End If

            oTdy.Options.MakeClean = True
            oTdy.Options.DropFontTags = True
            oTdy.Options.EncloseText = bEncloseText
            oTdy.Options.Xhtml = True
            oTdy.Options.SmartIndent = True
            oTdy.Options.LiteralAttribs = False
            'oTdy.Options.XmlOut = True
            'oTdy.Options.XmlTags = True

            If bReturnNumbericEntities Then
                oTdy.Options.NumEntities = True
            End If

            aBytes = Text.Encoding.UTF8.GetBytes(shtml)
            msIn.Write(aBytes, 0, aBytes.Length)
            msIn.Position = 0
            oTdy.Parse(msIn, msOut, oTmc)

            sTidyXhtml = Text.Encoding.UTF8.GetString(msOut.ToArray())
            Dim isError As Boolean = False
            Dim errorhtml As String = ""
            errorhtml &= "<div class=""XhtmlError"">"
            Dim msg As TidyNet.TidyMessage
            For Each msg In oTmc
                Dim level As String = ""
                Select Case msg.Level
                    Case TidyNet.MessageLevel.Error
                        level = "Error"
                        isError = True
                    Case TidyNet.MessageLevel.Warning
                        level = "Warning"
                    Case TidyNet.MessageLevel.Info
                        level = "Info"
                End Select
                errorhtml &= "<h3>" & level & " at line" & msg.Line & " column " & msg.Column & "</h3>"
                errorhtml &= "<div>" & Replace(Replace(msg.Message, ">", "&gt;"), "<", "&lt;") & "</div>"
            Next
            errorhtml &= "</div>"
            msg = Nothing

            If isError = True Then

                '  Dim myGumbo As New GumboWrapper()


                sTidyXhtml = errorhtml
            Else
                'now we need to strip out everything before and after the body tags
                If sTidyXhtml.Contains("<body>") Then
                    sTidyXhtml = sTidyXhtml.Substring(sTidyXhtml.IndexOf("<body>") + 6)
                    sTidyXhtml = sTidyXhtml.Substring(0, sTidyXhtml.IndexOf("</body>") - 1)
                    ' Remove the leading and trailing whitespace.
                    sTidyXhtml = sTidyXhtml.Trim()
                End If
            End If

            Return sTidyXhtml

        Catch ex As Exception
            ' It is the desired behaviour for this to return nothing if not valid html don't turn this on apart from in development.            Return Nothing
            Return Nothing
        Finally

            aBytes = Nothing
            msIn.Dispose()
            msIn = Nothing
            msOut.Dispose()

            msOut = Nothing
            oTmc = Nothing

            oTdy = Nothing
            sTidyXhtml = Nothing
        End Try
    End Function

    Public Function removeTagFromXml(ByVal xmlString As String, ByVal tagNames As String) As String

        tagNames = Replace(tagNames, " ", "")
        tagNames = Replace(tagNames, ",", "|")

        xmlString = Regex.Replace(xmlString, "<[/]?(" & tagNames & ":\w+)[^>]*?>", "", RegexOptions.IgnoreCase)

        Return xmlString

    End Function

    Public Function getNsMgr(ByRef oNode As XmlNode, ByRef oXml As XmlDocument) As XmlNamespaceManager

        Return Eonic.Tools.Xml.getNsMgr(oNode, oXml)

    End Function

    Public Function addNsToXpath(ByVal cXpath As String, ByRef nsMgr As XmlNamespaceManager) As String

        Return Eonic.Tools.Xml.addNsToXpath(cXpath, nsMgr)

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

    End Function

    Public Function getXpathFromQueryXml(ByVal oInstance As XmlElement, Optional ByVal sXsltPath As String = "") As String


        Dim oTransform As New Eonic.XmlHelper.Transform()

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

    Public Class xsltExtensions

#Region "Declarations"

        Private myWeb As Web
        'For saving object, dont want to keep chewing processing power during a transform
        Public oSaveHash As Hashtable

        ''for anything controlling web
        'Public Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)

        'Protected Overridable Sub OnComponentError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs) ' Handles Me.OnError
        '    'deals with the error ' ALWAYS IN DEBUG MODE HERE !!!
        '    returnException(e.ModuleName, e.ProcedureName, e.Exception, myWeb.mcEwSiteXsl, e.AddtionalInformation, True)
        '    'close connection pooling
        '    If Not myWeb.moDbHelper Is Nothing Then
        '        Try
        '            myWeb.moDbHelper.CloseConnection()
        '        Catch ex As Exception

        '        End Try
        '    End If
        '    'then raises a public event
        '    RaiseEvent OnError(sender, e)
        'End Sub

#End Region

#Region "Initialisation/Private"
        Public Sub New(ByRef aWeb As Web)
            myWeb = aWeb
        End Sub

        Public Sub New()

        End Sub
#End Region

#Region "XSLT Functions"

        Private Sub SaveObject(ByVal Name As String, ByVal Item As Object)
            Try
                If oSaveHash Is Nothing Then oSaveHash = New Hashtable
                If oSaveHash.Contains(Name) Then
                    oSaveHash(Name) = Item
                Else
                    oSaveHash.Add(Name, Item)
                End If
            Catch ex As Exception
                'Do Nothing
            End Try
        End Sub

        Private Function GetObject(ByVal Name As String) As Object
            If Not oSaveHash Is Nothing Then
                If oSaveHash.Contains(Name) Then
                    Return oSaveHash(Name)
                Else
                    Return Nothing
                End If
            Else
                Return Nothing
            End If
        End Function


        Function GetContentInstance(ByVal cXformName As String, ByVal cModuleType As String) As XmlElement

            Dim oXfrm As New Eonic.xForm

            If cModuleType <> "" Then cXformName = cXformName & "/" & cModuleType

            Dim commonfolders As New ArrayList
            commonfolders.Add("")
            commonfolders.Add("/ewcommon")

            oXfrm.load("/xforms/content/" & cXformName & ".xml", commonfolders.ToArray(GetType(String)))

            Return oXfrm.Instance

        End Function

        Function stringcompare(ByVal stringX As String, ByVal stringY As String) As Integer

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




        Function RegexTest(ByVal input As String, ByVal pattern As String) As Boolean

            Dim result As Boolean = False


            If Not (String.IsNullOrEmpty(pattern) Or String.IsNullOrEmpty(input)) Then

                Try
                    result = Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase)
                Catch ex As Exception
                    ' Do nothing - jsut here is case the input or pattern are crap
                End Try

            End If

            Return result

        End Function


        Function ToTitleCase(ByVal input As String) As String

            Return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input)

        End Function

        Function Trim(ByVal input As String) As String

            Return input.Trim

        End Function

        Public Function TruncateString(ByVal input As String, ByVal limit As Long) As String

            Return Tools.Text.TruncateString(input, limit)

        End Function

        Public Function replacestring(ByVal text As String, ByVal replace As String, ByVal replaceWith As String) As String
            Try
                If Not (text = "") And Not (replace = "") Then
                    Return text.Replace(replace, replaceWith)
                Else
                    Return text
                End If
            Catch ex As Exception
                Return text
            End Try
        End Function

        Public Function escapeJs(ByVal text As String) As String
            Try

                Dim orig As String = text
                If Not (text = "") Then
                    text = text.Replace("\", "\\")
                    text = text.Replace("&#13;", "\r")
                    text = text.Replace("&#10;", "\n")
                    text = text.Replace("#9;", "\t")
                    text = text.Replace("""", "\""")
                    text = text.Replace("'", "\'")
                    text = text.Replace("’", "\'")
                End If
                If orig <> text Then
                    Return text
                Else
                    Return orig
                End If
            Catch ex As Exception
                Return text
            End Try
        End Function

        Public Function cleantitle(ByVal text As String) As String
            Try
                Return "<span>" & text.Replace("&lt;", "<") & "</span>"
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

        Public Function getdate(ByVal dateString As String) As String
            Try
                Select Case dateString
                    Case "now()", "Now()", "now", "Now"
                        dateString = CStr(Eonic.Tools.Xml.XmlDate(Now(), True))
                End Select
                Return dateString
            Catch ex As Exception
                Return dateString
            End Try
        End Function


        Public Function dateadd(ByVal dateString As String, ByVal Interval As Long, ByVal IntervalType As String) As String
            Try
                If IsDate(dateString) Then
                    Select Case IntervalType
                        Case "d", "D", "Day", "day", "DAY"
                            dateString = CDate(dateString).AddDays(Interval)
                        Case "m", "M", "Month", "month", "MONTH"
                            dateString = CDate(dateString).AddMonths(Interval)
                        Case "y", "Y", "Year", "year", "YEAR"
                            dateString = CDate(dateString).AddYears(Interval)
                    End Select

                End If
                Return Eonic.Tools.Xml.XmlDate(dateString, False)

            Catch ex As Exception
                Return dateString
            End Try
        End Function

        Public Function formatdate(ByVal dateString As String, ByVal dateFormat As String) As String
            Try
                If IsDate(dateString) Then
                    dateString = CDate(dateString).ToString(dateFormat)
                End If
                Return dateString
            Catch ex As Exception
                Return dateString
            End Try
        End Function

        Public Function formatdate(ByVal dateString As String, ByVal dateFormat As String, ByVal cultureIdentifier As String) As String
            Try
                If IsDate(dateString) Then
                    Dim culture As CultureInfo
                    ' Try to work out the culture.
                    If String.IsNullOrEmpty(cultureIdentifier) Then
                        culture = CurrentCulture()
                    Else
                        Dim re As Regex = New Regex("^([a-z]{2})(-([a-z]{2,3}))?$", RegexOptions.IgnoreCase)
                        Dim cultureParams As Match = re.Match(cultureIdentifier)
                        If cultureParams.Groups.Count = 4 Then
                            Dim language As String = cultureParams.Groups(1).Value
                            Dim country As String = cultureParams.Groups(3).Value
                            If country = "" Then country = language
                            culture = New CultureInfo(
                                                            language.ToLower &
                                                            "-" &
                                                            country.ToUpper
                                                        )
                        Else
                            culture = CurrentCulture()
                        End If
                    End If

                    dateString = CDate(dateString).ToString(dateFormat, culture.DateTimeFormat)
                End If
                Return dateString
            Catch ex As Exception
                Return dateString
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

        Public Function datedaylightsaving(ByVal sInput As String) As String
            'returns the date with the offset if it's in daylight saving
            Dim sReturn As String
            Dim dteWorking As Date

            Try
                dteWorking = CDate(sInput)
                If dteWorking.IsDaylightSavingTime = True Then
                    dateadd(DateInterval.Hour, 1, dteWorking)
                    sReturn = formatDateISO8601(dteWorking) & "+01:00"
                Else
                    sReturn = sInput
                End If

            Catch ex As Exception
                sReturn = sInput
            End Try

            Return sReturn

        End Function

        Private Function formatDateISO8601(ByVal dteIn As Date) As String
            Try
                Dim strResult As New Text.StringBuilder
                strResult.Append(dteIn.Year)
                strResult.Append("-")
                strResult.Append(normaliseTwoCharacters(dteIn.Month.ToString))
                strResult.Append("-")
                strResult.Append(normaliseTwoCharacters(dteIn.Day.ToString))
                strResult.Append("T")
                strResult.Append(normaliseTwoCharacters(dteIn.Hour.ToString))
                strResult.Append(":")
                strResult.Append(normaliseTwoCharacters(dteIn.Minute.ToString))
                strResult.Append(":")
                strResult.Append(normaliseTwoCharacters(dteIn.Second.ToString))
                Return strResult.ToString
            Catch ex As Exception
                Return dteIn.ToString
            End Try
        End Function

        Private Function normaliseTwoCharacters(ByVal strIn As String) As String
            Try
                Select Case strIn.Length
                    Case Is > 1
                        Return strIn
                    Case Is = 1
                        Return "0" & strIn
                    Case Else
                        Return "00"
                End Select
            Catch ex As Exception
                Return "00"
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

        Public Function SubscriptionPrice(ByVal nPrice As String, ByVal cPriceUnit As String, ByVal nDuration As Integer, ByVal cDurationUnit As String) As String

            'Gets the price of the subscription
            Try
                Dim dFinish As Date
                Select Case cDurationUnit
                    Case "Day"
                        dFinish = Now.AddDays(nDuration)
                    Case "Week"
                        dFinish = Now.AddDays(nDuration * 7)
                    Case "Month"
                        dFinish = Now.AddMonths(nDuration)
                    Case "Year"
                        dFinish = Now.AddYears(nDuration)
                    Case Else
                        dFinish = Now
                End Select

                Dim nEndPrice As Double = 0
                Select Case cPriceUnit
                    Case "Day"
                        nEndPrice = CInt(dFinish.Subtract(Now).TotalDays) * nPrice
                    Case "Week"
                        nEndPrice = RoundUp(dFinish.Subtract(Now).TotalDays / 7, 0, 0) * nPrice
                    Case "Month"
                        'this will be trickier
                        'need to step through each month
                        Dim dCurrent As Date = Now
                        Dim nMonths As Integer = 0
                        Do Until dCurrent >= dFinish
                            dCurrent = dCurrent.AddMonths(1)
                            nMonths += 1
                        Loop
                        nEndPrice = nMonths * nPrice
                    Case "Year"
                        'same as months
                        Dim dCurrent As Date = Now
                        Dim nYears As Integer = 0
                        Do Until dCurrent >= dFinish
                            dCurrent = dCurrent.AddYears(1)
                            nYears += 1
                        Loop
                        nEndPrice = nYears * nPrice
                    Case Else
                        nEndPrice = 0
                End Select
                Return nEndPrice
            Catch ex As Exception
                Return 0
            End Try
        End Function

        Public Function CleanHTML(ByVal cHTMLString As String) As String
            Try
                Dim cTheString As String = Replace(Replace(cHTMLString, "&gt;", ">"), "&lt;", "<")
                cTheString = Eonic.Tools.Xml.convertEntitiesToCodes(cTheString)
                cTheString = tidyXhtmlFrag(cTheString, True)
                Return cTheString
            Catch ex As Exception
                Return cHTMLString
            End Try
        End Function

        Public Function CleanHTMLNode(ByVal oHtmlNode As XPathNodeIterator, ByVal RemoveTags As String) As Object

            Dim oXML As New XmlDocument
            Dim cHtml As String
            Dim cHtmlOut As String

            If oHtmlNode Is Nothing Then
                cHtml = ""
                Return cHtml
            Else
                cHtml = oHtmlNode.Current.InnerXml
                cHtml = Eonic.Tools.Xml.convertEntitiesToCodes(cHtml)
                cHtml = Replace(Replace(cHtml, "&gt;", ">"), "&lt;", "<")
                cHtml = "<div>" & cHtml & "</div>"

                cHtmlOut = tidyXhtmlFrag(cHtml, True, True, RemoveTags)

                cHtmlOut = Replace(cHtmlOut, "&#x0;", "")
                cHtmlOut = Replace(cHtmlOut, " &#0;", "")

                If cHtmlOut = Nothing Or cHtmlOut = "" Then
                    Return Nothing
                Else
                    Try
                        oXML.LoadXml(cHtmlOut)
                        Return oXML.DocumentElement
                    Catch ex As Exception
                        'Lets try option 2 first before we raise an error
                        ' RaiseEvent XSLTError(ex.ToString)
                        Try
                            oXML = New XmlDocument
                            oXML.AppendChild(oXML.CreateElement("div"))
                            oXML.DocumentElement.InnerXml = cHtmlOut
                            Return oXML.DocumentElement
                        Catch ex2 As Exception
                            Return cHtmlOut
                        End Try
                    End Try
                End If
            End If
        End Function

        Public Function CleanHTMLElement(ByVal oContextNode As Object, ByVal RemoveTags As String) As Object

            Dim oXML As New XmlDocument
            Dim cHtml As String
            Dim cHtmlOut As String

            If oContextNode Is Nothing Then
                cHtml = ""
                Return cHtml
            Else
                oContextNode.MoveNext()

                cHtml = oContextNode.Current.InnerXml
                cHtml = Eonic.Tools.Xml.convertEntitiesToCodes(cHtml)
                cHtml = Replace(Replace(cHtml, "&gt;", ">"), "&lt;", "<")
                cHtml = "<div>" & cHtml & "</div>"

                cHtmlOut = tidyXhtmlFrag(cHtml, True, True, RemoveTags)

                cHtmlOut = Replace(cHtmlOut, "&#x0;", "")
                cHtmlOut = Replace(cHtmlOut, " &#0;", "")

                cHtmlOut = convertEntitiesToCodes(cHtmlOut)

                If cHtmlOut = Nothing Or cHtmlOut = "" Then
                    Return ""
                Else
                    Try
                        ' Return cHtmlOut
                        cHtmlOut = cHtmlOut.Replace("&amp;#", "&#")
                        oXML.LoadXml(cHtmlOut)
                        Return oXML.DocumentElement
                    Catch ex As Exception
                        'Lets try option 2 first before we raise an error
                        ' RaiseEvent XSLTError(ex.ToString)
                        Try
                            oXML = New XmlDocument
                            oXML.AppendChild(oXML.CreateElement("div"))
                            oXML.DocumentElement.InnerXml = cHtmlOut
                            Return oXML.DocumentElement
                        Catch ex2 As Exception
                            Return cHtmlOut
                        End Try
                    End Try
                End If
            End If
        End Function

        Public Function EonicConfigValue(ByVal SectionName As String, ByVal ValueName As String) As String
            Try
                If LCase(SectionName) = "payment" Then Return ""
                If LCase(ValueName).Contains("password") Then Return ""
                Dim oConfig As System.Collections.Specialized.NameValueCollection = GetObject("EonicConfig_" & SectionName)

                If oConfig Is Nothing Then
                    oConfig = System.Web.Configuration.WebConfigurationManager.GetWebApplicationSection("eonic/" & SectionName)
                    If Not oConfig Is Nothing Then
                        SaveObject("EonicConfig_" & SectionName, oConfig)
                    End If
                End If

                If oConfig Is Nothing Then
                    Return ""
                Else
                    Dim returnVal As String
                    returnVal = IIf(oConfig(ValueName) Is Nothing, "", oConfig(ValueName))
                    If returnVal Is Nothing Then
                        Return ""
                    Else
                        Return returnVal
                    End If
                End If


            Catch ex As Exception
                Return ""
            End Try
        End Function

        Public Function ServerVariable(ByVal ValueName As String) As String
            Try
                If Not myWeb Is Nothing Then
                    Return myWeb.moRequest.ServerVariables(ValueName)
                Else
                    Return ""
                End If
            Catch ex As Exception
                Return ""
            End Try
        End Function

        Public Function SessionVariable(ByVal ValueName As String) As String
            Try
                If Not myWeb Is Nothing Then
                    Return myWeb.moSession(ValueName)
                Else
                    Return ""
                End If
            Catch ex As Exception
                Return ""
            End Try
        End Function

        Public Function GetContentLocations(ByVal nContentId As Integer) As String

            Return GetContentLocations(nContentId, False, 0, False)

        End Function

        Public Function GetContentLocations(ByVal nContentId As Integer, ByVal bIncludePrimary As Boolean) As String

            Return GetContentLocations(nContentId, bIncludePrimary, 0, False)

        End Function

        Public Function GetContentLocations(ByVal nContentId As Integer, ByVal bIncludePrimary As Boolean, ByVal bExcludeLocation As Integer) As String

            Return GetContentLocations(nContentId, bIncludePrimary, 0, False)

        End Function

        Public Function GetContentLocations(ByVal nContentId As Integer, ByVal bIncludePrimary As Boolean, ByVal nExcludeLocation As Integer, ByVal bShowHiddenPages As Boolean) As String


            ' Tried to do this as a node set, but returning a node set in non-XslCompiledTransform ways doesn't seem to work.
            Dim cLocations As String = ""

            Try

                If Not (myWeb Is Nothing) Then


                    Dim cSql As String
                    Dim oDr As SqlClient.SqlDataReader

                    cSql = "SELECT  l.nStructId As LocationID " _
                            & "FROM	dbo.tblContentStructure s INNER JOIN dbo.tblAudit a ON s.nauditid = a.nauditKey INNER JOIN dbo.tblContentLocation l ON s.nStructKey = l.nStructId " _
                            & "WHERE	nContentId = " & IIf(IsNumeric(nContentId), nContentId, -1) & " "


                    If bIncludePrimary Then cSql += " AND (l.bPrimary = 0) "
                    If IsNumeric(nExcludeLocation) Then cSql += " AND (l.nStructId <> " & nExcludeLocation & ") "
                    If Not (bShowHiddenPages) Then cSql += " AND (a.dExpireDate IS NULL OR a.dExpireDate >= GETDATE()) AND (a.dPublishDate IS NULL OR a.dPublishDate <= GETDATE()) AND (a.nStatus <> 0) "

                    cSql += " ORDER BY s.cStructName "

                    oDr = myWeb.moDbHelper.getDataReader(cSql)

                    Do While oDr.Read
                        cLocations += "," & oDr.Item(0).ToString()
                    Loop
                    oDr.Close()
                    oDr = Nothing

                    cLocations = cLocations.TrimStart(",")

                End If
                Return cLocations
            Catch ex As Exception
                Return ""
            End Try


        End Function

        Public Function GetDirIdFromFref(ByVal fRef As String) As String

            Dim ids() As String

            ids = myWeb.moDbHelper.getObjectsByRef(Web.dbHelper.objectTypes.Directory, fRef)

            If IsNumeric(ids(0)) Then
                Return CStr(ids(0))
            Else
                Return ""
            End If

        End Function


        Public Function GetPageFref(ByVal nPageId As Integer) As String

            Return myWeb.moDbHelper.getFRefFromPageId(nPageId)

        End Function


        Public Function GetUserXML(ByVal nId As Integer) As XmlElement

            Return myWeb.moDbHelper.GetUserXML(nId)

        End Function

        Public Function FlattenNodeXml(ByVal oContextNode As Object) As String
            Try
                oContextNode.MoveNext()
                Return oContextNode.Current.InnerXml
            Catch ex As Exception
                Return ""
            End Try
        End Function

        Public Function VirtualFileExists(ByVal cVirtualPath As String) As Integer
            Try
                Dim cVP As String = goServer.MapPath(cVirtualPath)
                If IO.File.Exists(cVP) Then
                    Return 1
                Else
                    Return 0
                End If
            Catch ex As Exception
                Return 0
            End Try
        End Function

        Public Function CompareDateIsNewer(ByVal cOriginalPath As String, ByVal cCheckNewerPath As String) As Integer
            Try
                Dim cOP As String = goServer.MapPath(cOriginalPath)
                Dim cCNP As String = goServer.MapPath(cCheckNewerPath)
                Dim cOPwritetime As Date = IO.File.GetLastWriteTime(cOP)
                Dim cCNPwritetime As Date = IO.File.GetLastWriteTime(cCNP)
                If cOPwritetime > cCNPwritetime Then
                    Return 1
                Else
                    Return 0
                End If
            Catch ex As Exception
                Return 0
            End Try
        End Function

        Public Function SaveImage(ByVal imageUrl As String, ByVal cVirtualPath As String) As String

            Try
                Dim oFS As New Eonic.fsHelper(myWeb.moCtx)

                oFS.mcStartFolder = goServer.MapPath("/")

                Return oFS.SaveFile(imageUrl, cVirtualPath)

            Catch ex As Exception
                Return ""
            End Try
        End Function
        Public Function EncryptPassword(ByVal sPassword As String) As String
            'RJP 7 Nov 2012. Amended HashString to add use of config setting.
            Try
                'Encrypt password.
                sPassword = Eonic.Tools.Encryption.HashString(sPassword, LCase(myWeb.moConfig("MembershipEncryption")), True) 'plain - md5 - sha1
                'RJP removed the following two lines as they appear to be doing nothing  the encrypted string.
                'sPassword = Eonic.Tools.Xml.EncodeForXml(sPassword)
                'sPassword = Eonic.Tools.Xml.convertEntitiesToCodes(sPassword)
                Return sPassword

            Catch ex As Exception
                Return "encryptionFailed"
            End Try
        End Function

        Public Function ImageSize(ByVal cVirtualPath As String) As String

            Try
                If VirtualFileExists(cVirtualPath) > 0 Then
                    Dim oURI As Uri = New Uri(goServer.MapPath(cVirtualPath))
                    Dim imageFromFile As BitmapSource = BitmapFrame.Create(oURI)
                    Dim nWidth As Integer = imageFromFile.PixelWidth
                    Dim nHeight As Integer = imageFromFile.PixelHeight
                    oURI = Nothing
                    imageFromFile = Nothing
                    Return nWidth & "x" & nHeight
                Else
                    Return ""
                End If
            Catch ex As Exception
                Return ""
            End Try
        End Function

        Public Function ImageWidth(ByVal cVirtualPath As String) As String

            Try
                If VirtualFileExists(cVirtualPath) > 0 Then
                    Dim imageFromFile As BitmapSource = BitmapFrame.Create(New Uri(goServer.MapPath(cVirtualPath)))
                    Dim nWidth As Integer = imageFromFile.PixelWidth
                    imageFromFile = Nothing
                    Return nWidth
                Else
                    Return ""
                End If
            Catch ex As Exception
                Return ""
            End Try
        End Function

        Public Function ImageHeight(ByVal cVirtualPath As String) As String

            Try
                If VirtualFileExists(cVirtualPath) > 0 Then
                    Dim imageFromFile As BitmapSource = BitmapFrame.Create(New Uri(goServer.MapPath(cVirtualPath)))
                    Dim nWidth As Integer = imageFromFile.PixelHeight
                    imageFromFile = Nothing
                    Return nWidth
                Else
                    Return ""
                End If
            Catch ex As Exception
                Return ""
            End Try
        End Function


        'Public Function getShippingMethods(ByVal nWeight As Decimal, ByVal nPrice As Decimal, ByVal nQuantity As Integer) As XmlElement

        '    Dim xmlResult As XmlElement = myWeb.moPageXml.CreateElement("ShippingMethods")
        '    Dim xmlShippingMethods As XmlElement = myWeb.moPageXml.CreateElement("ShippingMethods")
        '    Dim xmlTemp As XmlElement


        '    'get all the shipping options for a given shipping weight and price
        '    Dim strSql As New Text.StringBuilder
        '    strSql.Append("SELECT opt.cShipOptCarrier, opt.cShipOptTime, ")
        '    strSql.Append("dbo.fxn_shippingTotal(opt.nShipOptKey, " & nPrice.ToString & ", " & nQuantity.ToString & ", " & nWeight.ToString & ") AS nShippingTotal, ")
        '    strSql.Append("tblCartShippingLocations.cLocationNameShort, tblCartShippingLocations.cLocationISOa2 ")

        '    strSql.Append("FROM tblCartShippingLocations ")
        '    strSql.Append("INNER JOIN tblCartShippingRelations ON tblCartShippingLocations.nLocationKey = tblCartShippingRelations.nShpLocId ")
        '    strSql.Append("RIGHT OUTER JOIN tblCartShippingMethods AS opt ")
        '    strSql.Append("INNER JOIN tblAudit ON opt.nAuditId = tblAudit.nAuditKey ON tblCartShippingRelations.nShpOptId = opt.nShipOptKey ")

        '    strSql.Append("WHERE (opt.nShipOptQuantMin <= 0 OR opt.nShipOptQuantMin <= " & nQuantity.ToString & ") ")
        '    strSql.Append("AND (opt.nShipOptQuantMax <= 0 OR opt.nShipOptQuantMax >= " & nQuantity.ToString & ") ")
        '    strSql.Append("AND (opt.nShipOptPriceMin <= 0 OR opt.nShipOptPriceMin <= " & nPrice.ToString & ") ")
        '    strSql.Append("AND (opt.nShipOptWeightMin <= 0 OR opt.nShipOptWeightMin <= " & nWeight.ToString & ") ")
        '    strSql.Append("AND (opt.cCurrency IS NULL OR opt.cCurrency = '' OR opt.cCurrency = '') ")
        '    strSql.Append("AND (tblAudit.nStatus > 0) ")
        '    strSql.Append("AND (tblAudit.dPublishDate = 0 OR tblAudit.dPublishDate IS NULL OR tblAudit.dPublishDate <= " & Eonic.Tools.Database.SqlDate(Now) & ") ")
        '    strSql.Append("AND (tblAudit.dExpireDate = 0 OR tblAudit.dExpireDate IS NULL OR tblAudit.dExpireDate >= " & Eonic.Tools.Database.SqlDate(Now) & ") ")


        '    Dim oDs As DataSet = myWeb.moDbHelper.GetDataSet(strSql.ToString, "Method", "ShippingMethods", )
        '    xmlShippingMethods.InnerXml = oDs.GetXml()

        '    'move all the shipping methods up a level
        '    For Each xmlTemp In xmlShippingMethods.SelectNodes("ShippingMethods/Method")
        '        xmlShippingMethods.AppendChild(xmlTemp)
        '    Next

        '    For Each xmlTemp In xmlShippingMethods.SelectNodes("ShippingMethods")
        '        xmlShippingMethods.RemoveChild(xmlTemp)
        '    Next


        '    Return xmlShippingMethods

        'End Function


        ''' <summary>
        ''' Loads an image and creates a resized image in the same directory adding a suffix.
        ''' </summary>
        ''' <param name="cVirtualPath"></param>
        ''' <param name="maxWidth"></param>
        ''' <param name="maxHeight"></param>
        ''' <param name="sSuffix"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ResizeImage(ByVal cVirtualPath As String, ByVal maxWidth As Long, ByVal maxHeight As Long, ByVal sSuffix As String) As String
            Try
                Return ResizeImage(cVirtualPath, maxWidth, maxHeight, "", sSuffix, 99, False, False)
            Catch ex As Exception
                Return "Error"
            End Try
        End Function

        Public Function ResizeImage(ByVal cVirtualPath As String, ByVal maxWidth As Long, ByVal maxHeight As Long, ByVal sSuffix As String, ByVal nCompression As Integer) As String
            Try
                Return ResizeImage(cVirtualPath, maxWidth, maxHeight, "", sSuffix, nCompression, False, False)
            Catch ex As Exception
                Return "Error"
            End Try
        End Function

        Public Function ResizeImage(ByVal cVirtualPath As String, ByVal maxWidth As Long, ByVal maxHeight As Long, ByVal sPrefix As String, ByVal sSuffix As String, ByVal nCompression As Integer) As String
            Dim newFilepath As String = ""

            Try
                Return ResizeImage(cVirtualPath, maxWidth, maxHeight, sPrefix, sSuffix, nCompression, False, False)
            Catch ex As Exception
                Return "Error - " & ex.Message
            End Try
        End Function

        Public Function ResizeImage(ByVal cVirtualPath As String, ByVal maxWidth As Long, ByVal maxHeight As Long, ByVal sPrefix As String, ByVal sSuffix As String, ByVal nCompression As Integer, ByVal noStretch As Boolean) As String
            Dim newFilepath As String = ""

            Try
                Return ResizeImage(cVirtualPath, maxWidth, maxHeight, sPrefix, sSuffix, nCompression, noStretch, False)
            Catch ex As Exception
                Return "Error - " & ex.Message
            End Try
        End Function

        Public Function ResizeImage(ByVal cVirtualPath As String, ByVal maxWidth As Long, ByVal maxHeight As Long, ByVal sPrefix As String, ByVal sSuffix As String, ByVal nCompression As Integer, ByVal noStretch As Boolean, ByVal isCrop As Boolean) As String
            Dim newFilepath As String = ""

            Try
                Return ResizeImage2(cVirtualPath, maxWidth, maxHeight, sPrefix, sSuffix, nCompression, noStretch, isCrop, False)
            Catch ex As Exception
                Return "Error - " & ex.Message
            End Try
        End Function

        Public Function ResizeImage2(ByVal cVirtualPath As String, ByVal maxWidth As Long, ByVal maxHeight As Long, ByVal sPrefix As String, ByVal sSuffix As String, ByVal nCompression As Integer, ByVal noStretch As Boolean, ByVal isCrop As Boolean, ByVal forceCheck As Boolean) As String
            Dim newFilepath As String = ""
            Dim cProcessInfo As String = "Resizing - " & cVirtualPath
            Try
                ' PerfMon.Log("xmlTools", "ResizeImage - Start")
                If myWeb.moRequest Is Nothing Then

                Else
                    Try
                        If myWeb.moRequest("imgRefresh") <> "" Then
                            forceCheck = True
                        End If
                    Catch ex As Exception

                    End Try

                End If

                cVirtualPath = cVirtualPath.Replace("%20", " ")
                'calculate the new filename
                'dim get the filename
                Dim filename As String = cVirtualPath.Substring(cVirtualPath.LastIndexOf("/") + 1)
                Dim filetype As String = filename.Substring(filename.LastIndexOf(".") + 1)
                Dim directoryPath As String = cVirtualPath.Substring(0, cVirtualPath.LastIndexOf("/") + 1)
                Dim cVirtualPath2 As String = directoryPath & sPrefix & filename

                cVirtualPath2 = Replace(cVirtualPath2, "//", "/")

                'Save any resized freestock to local appart from standard thumbnails
                If Not (sPrefix = "~ew/tn-" And maxWidth = 85 And maxHeight = 85) Then
                    If cVirtualPath2.StartsWith("/images/FreeStock") Then
                        cVirtualPath2 = Replace(cVirtualPath2, "/images/FreeStock", "/images/~ew/FreeStock")
                    End If
                End If

                Select Case filetype
                    Case "pdf", "doc", "docx", "gif"
                        newFilepath = Replace(cVirtualPath2, "." & filetype, sSuffix & ".png")
                    Case Else
                        newFilepath = Replace(cVirtualPath2, "." & filetype, sSuffix & "." & filetype)
                End Select

                If (Not myWeb.mbAdminMode) And forceCheck = False Then
                    Return newFilepath
                Else

                    If VirtualFileExists(cVirtualPath) > 0 Then

                        If (Not (VirtualFileExists(newFilepath) > 0)) Or CompareDateIsNewer(cVirtualPath, newFilepath) > 0 Then
                            Select Case filetype
                                Case "pdf"

                                    Dim ihelp As New ImageHelper("")

                                    System.Threading.ThreadPool.SetMaxThreads(10, 10)
                                    Dim doneEvents(1) As System.Threading.ManualResetEvent

                                    Dim newThumbnail As New ImageHelper.PDFThumbNail
                                    newThumbnail.FilePath = cVirtualPath
                                    newThumbnail.newImageFilepath = newFilepath
                                    newThumbnail.goServer = goServer
                                    newThumbnail.maxWidth = maxWidth

                                    System.Threading.ThreadPool.QueueUserWorkItem(New System.Threading.WaitCallback(AddressOf ihelp.GeneratePDFThumbNail), newThumbnail)
                                    newThumbnail = Nothing
                                    ihelp.Close()
                                    ihelp = Nothing

                                Case Else
                                    Dim cCheckServerPath As String = newFilepath.Substring(0, newFilepath.LastIndexOf("/") + 1)
                                    cCheckServerPath = goServer.MapPath(cCheckServerPath)
                                    'load the orignal image and resize
                                    Dim oImage As New Eonic.Tools.Image(goServer.MapPath(cVirtualPath))
                                    oImage.KeepXYRelation = True
                                    oImage.NoStretch = noStretch
                                    oImage.IsCrop = isCrop
                                    oImage.SetMaxSize(maxWidth, maxHeight)

                                    oImage.Save(goServer.MapPath(newFilepath), nCompression, cCheckServerPath)

                                    oImage.Close()
                                    oImage = Nothing

                            End Select


                            'PerfMon.Log("xmlTools", "ResizeImage - End")
                            Return newFilepath
                        Else
                            'PerfMon.Log("xmlTools", "ResizeImage - End")
                            Return newFilepath
                        End If

                    Else
                        'PerfMon.Log("xmlTools", "ResizeImage - End")
                        Return "/ewcommon/images/awaiting-image-thumbnail.gif"
                    End If

                End If


            Catch ex As Exception
                ' PerfMon.Log("xmlTools", "ResizeImage - End")
                If LCase(myWeb.moConfig("Debug")) = "on" Then
                    reportException("xmlTools.xsltExtensions", "ResizeImage2", ex, , cProcessInfo)
                    Return "/ewcommon/images/awaiting-image-thumbnail.gif?Error=" & ex.InnerException.Message & " - " & ex.Message & " - " & ex.StackTrace
                Else
                    Return "/ewcommon/images/awaiting-image-thumbnail.gif?Error=" & ex.Message
                End If

            End Try
        End Function

        Public Function ContentQuery(ByVal cContentName As String, ByVal cXpath As String, Optional ByVal nPrimaryId As Long = 0) As Object
            Dim oDocInstance As XmlDocument = New XmlDocument
            Dim targetElmt As XmlElement
            Dim queryResult As XmlNode
            Dim sSql As String
            Try

                targetElmt = oDocInstance.CreateElement("instance")
                oDocInstance.AppendChild(targetElmt)
                If nPrimaryId > 0 Then
                    sSql = "inner join tblContentLocation on tblContent.nContentKey = tblContentLocation.nContentId  where cContentName='" & cContentName & "' and tblContentLocation.bPrimary = true and tblContentLocation.nStructId=" & nPrimaryId
                Else
                    sSql = "where cContentName='" & cContentName & "'"
                End If

                targetElmt.InnerXml = myWeb.moDbHelper.getObjectInstance(Web.dbHelper.objectTypes.Content, , "where cContentName='" & cContentName & "'")

                queryResult = oDocInstance.DocumentElement.SelectSingleNode(cXpath)
                If Not queryResult Is Nothing Then
                    Select Case queryResult.NodeType
                        Case XmlNodeType.Attribute
                            Return queryResult.Value
                        Case XmlNodeType.Text
                            Return queryResult.Value
                        Case XmlNodeType.Element
                            Return queryResult
                        Case Else
                            Return "Unrecognised NodeType"
                    End Select
                Else
                    If targetElmt.SelectSingleNode("descendant-or-self::nContentKey").InnerText = "" Then
                        Return "Error-ContentMissing"
                    Else
                        Return "Error-PathNotResolved: " & cXpath
                    End If
                End If

            Catch ex As Exception
                Return "Error-" & ex.Message
            End Try

        End Function

        Public Function ContentQuery(fRef As String) As Object
            Dim oDocInstance As XmlDocument = New XmlDocument
            Dim returnNode As XmlNode
            Try

                oDocInstance.LoadXml(myWeb.moDbHelper.getObjectInstance(Web.dbHelper.objectTypes.Content, , "where cContentForiegnRef='" & fRef & "'"))
                returnNode = oDocInstance.DocumentElement
                Return returnNode

            Catch ex As Exception
                Return "Error-" & ex.Message
            End Try

        End Function

        Public Function evaluateXpath(nodes As XPathNodeIterator, xpath As String) As String
            Try
                While nodes.MoveNext()
                    Dim n As XPathNavigator = TryCast(nodes.Current, XPathNavigator)
                    Return n.Evaluate(xpath).ToString()
                End While
            Catch ex As Exception
                Return "Error - Not Deleted" & ex.Message
            End Try
            Return Nothing
        End Function

        Public Function DeleteContent(ByVal nContentId As String) As String

            Try
                If CLng("0" & nContentId) > 0 Then
                    Return CStr(myWeb.moDbHelper.DeleteObject(Web.dbHelper.objectTypes.Content, nContentId))
                Else
                    Return Nothing
                End If

            Catch ex As Exception
                Return "Error - Not Deleted" & ex.Message
            End Try

        End Function

        Public Function UpdatePositions(ByVal nContentId As Long, ByVal cPosition As String) As String

            Dim cPositions As String
            Dim aPositions() As String
            Dim i As Integer

            Try

                cPositions = GetContentLocations(nContentId, True)
                aPositions = Split(cPositions, ",")
                For i = 0 To UBound(aPositions)
                    myWeb.moDbHelper.updatePagePosition(CLng(aPositions(i)), nContentId, cPosition)
                Next

                Return cPosition

            Catch ex As Exception
                Return "Error - Changed Position" & ex.Message
            End Try

        End Function

        Public Function GetSelectOptions(ByVal Query As String) As Object
            'Dim SelectDoc As New XmlDocument()
            Dim SelectElmt As XmlElement = myWeb.moPageXml.CreateElement("select1")
            Dim Query1 As String = ""
            Dim Query2 As String = ""
            Dim sql As String
            Try
                Dim QueryArr() As String = Split(Query, ".")
                Query1 = QueryArr(0)
                If UBound(QueryArr) > 0 Then Query2 = QueryArr(1)
                Dim oXfrms As New Eonic.Web.xForm
                oXfrms.moPageXML = myWeb.moPageXml
                Select Case Query1
                    Case "SiteTree"
                        Dim StructElmt As XmlElement = myWeb.GetStructureXML("Site", myWeb.mnUserId)
                        Dim MenuElmt As XmlElement
                        Dim ParElmt As XmlElement

                        For Each MenuElmt In StructElmt.SelectNodes("descendant-or-self::MenuItem")
                            Dim Label As String = MenuElmt.GetAttribute("name")
                            Dim Value As String = MenuElmt.GetAttribute("id")
                            For Each ParElmt In MenuElmt.SelectNodes("ancestor::MenuItem")
                                Label = "-" & Label
                            Next
                            oXfrms.addOption(SelectElmt, Label, Value)
                        Next
                    Case "Directory"
                        'Returns all of a specified type in the directory to specify the type use attribute "query2"

                        sql = "select nDirKey as value, cDirName as name from tblDirectory where cDirSchema='" & Query2 & "'"
                        Dim oDr As System.Data.SqlClient.SqlDataReader = myWeb.moDbHelper.getDataReader(sql)
                        oXfrms.addOptionsFromSqlDataReader(SelectElmt, oDr)


                    Case "DirectoryName"
                        'Returns all of a specified type in the directory to specify the type use attribute "query2"

                        sql = "select cDirName as value, cDirName as name from tblDirectory where cDirSchema='" & Query2 & "'"
                        Dim oDr As System.Data.SqlClient.SqlDataReader = myWeb.moDbHelper.getDataReader(sql)
                        oXfrms.addOption(SelectElmt, "All", "all")
                        oXfrms.addOptionsFromSqlDataReader(SelectElmt, oDr)

                    Case "Users"

                        ' This is different from Directory in that it tries to format the users nicely
                        ' We'll use a subquery to get the data and then reduce it down to the calculated columns which we can order
                        Dim subqueryBuilder As New Text.StringBuilder()
                        subqueryBuilder.Append("SELECT nDirKey").Append(",")
                        subqueryBuilder.Append("CAST(CAST(users.cDirXml as xml).query('string(/User[1]/FirstName[1])') as nvarchar(MAX)) As FirstName").Append(",")
                        subqueryBuilder.Append("CAST(CAST(users.cDirXml as xml).query('string(/User[1]/LastName[1])') as nvarchar(MAX)) As LastName").Append(",")
                        subqueryBuilder.Append("users.cDirName").Append(" ")
                        subqueryBuilder.Append("FROM dbo.tblDirectory users").Append(" ")

                        ' Check whether we're filtering users by a parent id
                        If Eonic.Tools.Number.IsReallyNumeric(Query2) AndAlso Convert.ToInt32(Query2) > 0 Then
                            subqueryBuilder.Append(String.Format("INNER JOIN dbo.fxn_getMembers({0},1,'User',0,0,GETDATE(),0) members", Query2)).Append(" ")
                            subqueryBuilder.Append("ON users.nDirKey = members.nDirId").Append(" ")
                        End If
                        subqueryBuilder.Append("WHERE users.cDirSchema='User'")

                        ' Now reduce the query down to what is needed from it and in what order.
                        Dim queryBuilder As New Text.StringBuilder()
                        queryBuilder.Append("SELECT nDirKey As value").Append(",")
                        queryBuilder.Append("(FirstName + ' ' + LastName + ' (' + cDirName + ')') As name").Append(" ")
                        queryBuilder.Append(String.Format("FROM ({0}) u", subqueryBuilder.ToString())).Append(" ")
                        queryBuilder.Append("ORDER BY Lastname, FirstName")

                        Dim oDr As System.Data.SqlClient.SqlDataReader = myWeb.moDbHelper.getDataReader(queryBuilder.ToString())
                        oXfrms.addOptionsFromSqlDataReader(SelectElmt, oDr)


                    Case "Content"

                        sql = "select nContentKey as value, cContentName as name from tblContent where cContentSchemaName='" & Query2 & "' order by cContentName ASC"
                        Dim oDr As System.Data.SqlClient.SqlDataReader = myWeb.moDbHelper.getDataReader(sql)
                        oXfrms.addOptionsFromSqlDataReader(SelectElmt, oDr)

                    Case "CartStatus"
                        For Each process As Eonic.Web.Cart.cartProcess In [Enum].GetValues(GetType(Eonic.Web.Cart.cartProcess))
                            oXfrms.addOption(SelectElmt, process.ToString, process.ToString("D"))
                        Next

                    Case "Language"

                        If Not myWeb.goLangConfig Is Nothing Then
                            Dim langNode As XmlElement
                            oXfrms.addOption(SelectElmt, myWeb.goLangConfig.GetAttribute("default"), myWeb.goLangConfig.GetAttribute("code"))
                            For Each langNode In myWeb.goLangConfig.SelectNodes("Language")
                                oXfrms.addOption(SelectElmt, langNode.GetAttribute("systemName"), langNode.GetAttribute("code"))
                            Next
                        Else
                            oXfrms.addOption(SelectElmt, "English-UK", "en-gb")
                        End If

                    Case "Countries"
                        Dim oCart As New Eonic.Web.Cart(myWeb)
                        oCart.populateCountriesDropDown(oXfrms, SelectElmt, "")

                    Case "CountriesId"
                        Dim oCart As New Eonic.Web.Cart(myWeb)
                        oCart.populateCountriesDropDown(oXfrms, SelectElmt, "", True)
                    Case "Currency"
                        Dim moPaymentCfg As XmlNode
                        moPaymentCfg = WebConfigurationManager.GetWebApplicationSection("eonic/payment")
                        Dim oCurrencyElmt As XmlElement
                        For Each oCurrencyElmt In moPaymentCfg.SelectNodes("currencies/Currency")
                            'going to need to do something about languages
                            oXfrms.addOption(SelectElmt, oCurrencyElmt.SelectSingleNode("name").InnerText, oCurrencyElmt.GetAttribute("ref"))
                        Next

                    Case "Library"

                        Dim ProviderName As String
                        Dim calledType As Type
                        Dim classPath As String = ""
                        Dim methodName As String = ""

                        Dim moPrvConfig As Eonic.ProviderSectionHandler = WebConfigurationManager.GetWebApplicationSection("eonic/messagingProviders")
                        Dim assemblyInstance As [Assembly] = [Assembly].Load(moPrvConfig.Providers(ProviderName).Type)
                        calledType = assemblyInstance.GetType(classPath, True)

                        Dim o As Object = Activator.CreateInstance(calledType)

                        Dim args(1) As Object
                        args(0) = SelectElmt

                        calledType.InvokeMember(methodName, BindingFlags.InvokeMethod, Nothing, o, args)


                    Case "FolderList"

                        Dim library As fsHelper.LibraryType = Nothing
                        If Tools.EnumUtility.TryParse(GetType(Eonic.fsHelper.LibraryType), Query2, False, library) Then

                            Dim path As String = fsHelper.GetFileLibraryPath(library)
                            If Not String.IsNullOrEmpty(path) Then

                                Dim rootfolder As New DirectoryInfo(myWeb.goServer.MapPath("/") & path.Trim("/\".ToCharArray))
                                Dim prefixFolderPath As String = rootfolder.FullName.Substring(0, rootfolder.FullName.LastIndexOf("\"))


                                If rootfolder.Exists Then
                                    Dim folderList As Generic.List(Of String) = Eonic.fsHelper.EnumerateFolders(rootfolder)
                                    Dim tidypath As String = ""
                                    For Each folderPath As String In folderList
                                        tidypath = folderPath.Replace(prefixFolderPath, "").Replace("\", "/")
                                        oXfrms.addOption(SelectElmt, tidypath, tidypath)
                                    Next
                                End If

                            End If


                        End If
                    Case "FileList"

                        Dim path As String = myWeb.goServer.MapPath("/") & Query2.Trim("/\".ToCharArray)
                        If Not String.IsNullOrEmpty(path) Then

                            Dim rootfolder As New DirectoryInfo(path)

                            Dim files As FileInfo() = rootfolder.GetFiles()
                            Dim fi As FileInfo

                            For Each fi In files
                                Dim cExt As String = LCase(fi.Extension)
                                Dim tidypath As String = "/" & Query2.Trim("/\".ToCharArray) & "/" & fi.Name

                                oXfrms.addOption(SelectElmt, fi.Name.Replace(fi.Extension, ""), tidypath)

                            Next fi

                        End If

                    Case "CodeGroups"
                        'Returns all of a specified type in the directory to specify the type use attribute "query2"

                        sql = "select nCodeKey as value, cCodeName as name from tblCodes where nCodeParentId is NULL"
                        Dim oDr As System.Data.SqlClient.SqlDataReader = myWeb.moDbHelper.getDataReader(sql)
                        oXfrms.addOptionsFromSqlDataReader(SelectElmt, oDr)

                    Case "Lookup"
                        'Returns all of a specified type in the directory to specify the type use attribute "query2"

                        sql = "select cLkpValue as value, cLkpKey as name from tblLookup where cLkpCategory like '" & Query2 & "' order by nDisplayOrder, nLkpID"
                        Dim oDr As System.Data.SqlClient.SqlDataReader = myWeb.moDbHelper.getDataReader(sql)
                        oXfrms.addOptionsFromSqlDataReader(SelectElmt, oDr)

                    Case "availableIcons"

                        If IO.File.Exists(goServer.MapPath("/ewcommon/icons/icons.xml")) Then
                            Dim newXml As New XmlDocument
                            newXml.PreserveWhitespace = True
                            newXml.Load(goServer.MapPath("/ewcommon/icons/icons.xml"))
                            SelectElmt.InnerXml = newXml.DocumentElement.InnerXml
                        End If

                    Case "themePresets"
                        Dim moThemeConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/theme")
                        Dim currenttheme As String = moThemeConfig("CurrentTheme")

                        If IO.File.Exists(goServer.MapPath("/ewthemes/" & currenttheme & "/themeManifest.xml")) Then
                            Dim newXml As New XmlDocument
                            newXml.PreserveWhitespace = True
                            newXml.Load(goServer.MapPath("/ewthemes/" & currenttheme & "/themeManifest.xml"))
                            Dim oElmt As XmlElement
                            For Each oElmt In newXml.SelectNodes("/Theme/Presets/Preset")
                                Dim ItemElmt As XmlElement = SelectElmt.OwnerDocument.CreateElement("item")
                                Dim LabelElmt As XmlElement = SelectElmt.OwnerDocument.CreateElement("label")
                                LabelElmt.InnerText = oElmt.GetAttribute("name")
                                ItemElmt.AppendChild(LabelElmt)
                                Dim ValueElmt As XmlElement = SelectElmt.OwnerDocument.CreateElement("value")
                                ValueElmt.InnerText = oElmt.GetAttribute("name")
                                ItemElmt.AppendChild(ValueElmt)
                                SelectElmt.AppendChild(ItemElmt)
                            Next
                        End If

                    Case "ProductGroups"

                        sql = "select nCatKey as value, cCatName as name from tblCartProductCategories order by cCatName"
                        Dim oDr As System.Data.SqlClient.SqlDataReader = myWeb.moDbHelper.getDataReader(sql)
                        oXfrms.addOptionsFromSqlDataReader(SelectElmt, oDr)


                    Case Else
                        sql = Query1
                        Dim oDr As System.Data.SqlClient.SqlDataReader = myWeb.moDbHelper.getDataReader(sql)
                        oXfrms.addOptionsFromSqlDataReader(SelectElmt, oDr)

                End Select

                'Return cPosition
                oXfrms = Nothing

                Return SelectElmt

            Catch ex As Exception
                Return "Error - Changed Position" & ex.Message
            End Try

        End Function

        Public Function GetSelectOptions(ByVal ProviderName As String, ByVal classPath As String, ByVal methodName As String) As Object
            'Dim SelectDoc As New XmlDocument()
            Dim SelectElmt As XmlElement = myWeb.moPageXml.CreateElement("select1")

            Try

                Dim moPrvConfig As Eonic.ProviderSectionHandler = WebConfigurationManager.GetWebApplicationSection("eonic/messagingProviders")
                'Dim assemblyInstance As [Assembly] = [Assembly].Load(moPrvConfig.Providers(ProviderName).Type)
                '
                Dim ourProvider As Object
                If Not moPrvConfig.Providers(ProviderName & "Local") Is Nothing Then
                    ourProvider = moPrvConfig.Providers(ProviderName & "Local")
                Else
                    ourProvider = moPrvConfig.Providers(ProviderName)
                End If

                Dim assemblyInstance As [Assembly]

                If ourProvider.parameters("path") <> "" Then
                    assemblyInstance = [Assembly].LoadFrom(goServer.MapPath(ourProvider.parameters("path")))
                Else
                    assemblyInstance = [Assembly].Load(ourProvider.Type)
                End If

                Dim calledType As Type = assemblyInstance.GetType(classPath, True)

                Dim args(0) As Object
                args(0) = myWeb
                Dim o As Object = Activator.CreateInstance(calledType, args)

                Dim args2(0) As Object
                args2(0) = SelectElmt
                calledType.InvokeMember(methodName, BindingFlags.InvokeMethod, Nothing, o, args2)

                Return SelectElmt

            Catch ex As Exception
                SelectElmt.InnerXml = "<item><label>GetSelectOptions Error - " & ex.Message & "</label><value>error</value></item>"

                Return SelectElmt

            End Try

        End Function

        Public Function GetPageXml(ByVal PageId As String, ByVal xPath As String) As Object
            'Dim SelectDoc As New XmlDocument()
            Dim SelectElmt As XmlElement = myWeb.moPageXml.CreateElement("select1")
            Dim oPageXml As XmlDocument
            Try
                Dim existingPageId As Long = myWeb.mnPageId

                Dim newWeb As New Eonic.Web(myWeb.moCtx)
                newWeb.InitializeVariables()
                'newWeb.ibIndexMode = True
                newWeb.mnPageId = PageId
                newWeb.mbIgnorePath = True

                oPageXml = newWeb.GetPageXML()
                Dim oNodelist As XmlNodeList

                If xPath <> "" Then
                    oNodelist = oPageXml.SelectNodes(xPath)
                    Dim oReturnXml As XmlDocument = New XmlDocument
                    Dim oReturnElmt As XmlElement = oReturnXml.CreateElement("Page")
                    oReturnXml.AppendChild(oReturnElmt)
                    Dim oNode As XmlNode
                    For Each oNode In oNodelist
                        Eonic.Tools.Xml.AddExistingNode(oReturnElmt, oNode)
                        'oReturnElmt.AppendChild(oNode.CloneNode(True))
                    Next
                    Return oReturnXml
                Else
                    Return oPageXml

                End If
                Return oNodelist

                newWeb = Nothing

            Catch ex As Exception
                SelectElmt.InnerXml = "<item><label>GetPageXml Error - " & ex.Message & "</label><value>error</value></item>"

                Return SelectElmt

            End Try

        End Function

        Public Function GetContentDetailXml(ByVal ArtId As String) As Object
            'Dim SelectDoc As New XmlDocument()
            Dim SelectElmt As XmlElement = myWeb.moPageXml.CreateElement("select1")
            Dim oReturnXml As XmlDocument = New XmlDocument
            Dim oReturnElmt As XmlElement = oReturnXml.CreateElement("Page")
            oReturnXml.AppendChild(oReturnElmt)
            Try

                ' myWeb.GetContentDetailXml(Nothing, ArtId, True, False)

                Eonic.Tools.Xml.AddExistingNode(oReturnElmt, myWeb.GetContentDetailXml(Nothing, ArtId, True, False))

                Return oReturnXml


            Catch ex As Exception
                oReturnXml.DocumentElement.InnerXml = "<item><label>GetContentDetailXml Error - " & ex.Message & "</label><value>error</value></item>"
                Return oReturnXml

            End Try

        End Function

        Public Function GetContentBriefXml(ByVal ArtId As String) As Object
            'Dim SelectDoc As New XmlDocument()
            Dim SelectElmt As XmlElement = myWeb.moPageXml.CreateElement("select1")
            Dim oReturnXml As XmlDocument = New XmlDocument
            Dim oReturnElmt As XmlElement = oReturnXml.CreateElement("Page")
            oReturnXml.AppendChild(oReturnElmt)
            Try

                ' myWeb.GetContentDetailXml(Nothing, ArtId, True, False)

                Eonic.Tools.Xml.AddExistingNode(oReturnElmt, myWeb.GetContentBriefXml(Nothing, ArtId))

                Return oReturnXml


            Catch ex As Exception
                oReturnXml.DocumentElement.InnerXml = "<item><label>GetContentDetailXml Error - " & ex.Message & "</label><value>error</value></item>"
                Return oReturnXml

            End Try

        End Function


        'Public Function SplitToNodeset(ByVal SourceNode As Object, ByVal delimiter As String) As Object

        '    Dim oReturnXml As XmlDocument = New XmlDocument
        '    Dim SourceSplit As String()
        '    Dim i As Integer
        '    Dim oContentPage As XmlElement
        '    Try
        '        oReturnXml.LoadXml(SourceNode)

        '        SourceSplit = Split(oReturnXml.InnerXml, delimiter)

        '        oReturnXml.InnerXml = ""
        '        For i = 0 To SourceSplit.Length
        '            oContentPage = oReturnXml.CreateElement("ContentPage")
        '            oContentPage.SetAttribute("pageNo", i)
        '            oContentPage.InnerXml = SourceSplit(i)
        '            oReturnXml.AppendChild(oContentPage)
        '        Next

        '        Return oReturnXml

        '    Catch ex As Exception
        '        oReturnXml.DocumentElement.InnerXml = "<item><label>GetContentDetailXml Error - " & ex.Message & "</label><value>error</value></item>"
        '        Return oReturnXml

        '    End Try

        'End Function

        Public Function BundleJS(ByVal CommaSeparatedFilenames As String, ByVal TargetFile As String) As Object

            Dim sReturnString As String

            Try

                Dim bReset As Boolean = False
                If myWeb Is Nothing Or gbDebug Then
                    'likely to be in error condition
                    sReturnString = CommaSeparatedFilenames.Replace("~", "")
                Else
                    If Not myWeb.moRequest("reBundle") Is Nothing Then
                        bReset = True
                    End If

                    If Not myWeb.moCtx.Application.Get(TargetFile) Is Nothing And bReset = False Then

                        sReturnString = myWeb.moCtx.Application.Get(TargetFile)

                    Else
                        Dim appPath As String = myWeb.moRequest.ApplicationPath
                        If appPath.EndsWith("ewcommon") Then
                            CommaSeparatedFilenames = CommaSeparatedFilenames.Replace("~/", "~/../")
                        End If

                        CommaSeparatedFilenames = CommaSeparatedFilenames.TrimEnd(CChar(","))

                        Dim bundleFilePaths As String() = Split(CommaSeparatedFilenames, ",")
                        'we build the file
                        Dim nullBuilder As New NullBuilder()
                        Dim scriptTransformer As New ScriptTransformer()
                        Dim nullOrderer As New NullOrderer()
                        Dim Bundles As New Optimization.BundleCollection()

                        Dim CtxBase As New System.Web.HttpContextWrapper(myWeb.moCtx)
                        Dim BundlesCtx As New Optimization.BundleContext(CtxBase, Bundles, "~/js/")
                        Dim jsBundle As New BundleTransformer.Core.Bundles.CustomScriptBundle(TargetFile)

                        BundlesCtx.EnableInstrumentation = False

                        jsBundle.Include(bundleFilePaths)

                        jsBundle.Builder = nullBuilder
                        jsBundle.Transforms.Add(scriptTransformer)
                        jsBundle.Orderer = nullOrderer

                        Bundles.Add(jsBundle)

                        '  Dim instance As New CustomBundleResolver(Bundles, CtxBase)

                        Dim scriptFile As String

                        scriptFile = TargetFile & "/script.js"

                        Dim fsh As New Eonic.fsHelper(myWeb.moCtx)
                        fsh.initialiseVariables(fsHelper.LibraryType.Scripts)

                        Dim br As Optimization.BundleResponse = Bundles.GetBundleFor(TargetFile).GenerateBundleResponse(BundlesCtx)
                        Dim info As Byte() = New System.Text.UTF8Encoding(True).GetBytes(br.Content)
                        fsh.DeleteFile(goServer.MapPath("/js" & scriptFile.TrimStart("~")))

                        scriptFile = "/js" & fsh.SaveFile("script.js", TargetFile, info)

                        If scriptFile.StartsWith("/js" & TargetFile.TrimStart("~")) Then
                            'file has been saved successfully.
                            myWeb.moCtx.Application.Set(TargetFile, scriptFile)
                        Else
                            'we have a file save error we should try again next request.

                        End If

                        sReturnString = scriptFile

                        info = Nothing
                        fsh = Nothing
                        BundlesCtx = Nothing
                        jsBundle = Nothing
                        Bundles = Nothing
                        nullBuilder = Nothing
                        scriptTransformer = Nothing
                        bundleFilePaths = Nothing

                    End If
                End If

                Return sReturnString
                sReturnString = Nothing

            Catch ex As Exception
                Return ex.Message
            End Try

        End Function



        Public Function BundleCSS(ByVal CommaSeparatedFilenames As String, ByVal TargetFile As String) As Object
            If Split(CommaSeparatedFilenames, ",").Count > 1 Then
                Throw New NotSupportedException("BundleCSS: this function does not currently support multiple less files")
            End If

            Dim sReturnString As String = ""
            Dim sReturnError As String = ""
            Dim bReset As Boolean = False

            Try
                If myWeb Is Nothing Then
                    'ONLY HAPPENS ON ERROR PAGES
                    gbDebug = True
                Else
                    If Not myWeb.moRequest Is Nothing Then
                        If Not myWeb.moRequest("reBundle") Is Nothing Then
                            bReset = True
                        End If
                    End If
                End If

                If gbDebug Then
                    sReturnString = CommaSeparatedFilenames.Replace("~", "")
                Else
                    If Not myWeb.moCtx.Application.Get(TargetFile) Is Nothing And bReset = False Then
                        sReturnString = myWeb.moCtx.Application.Get(TargetFile)
                    Else

                        Dim oImp As Eonic.Tools.Security.Impersonate = New Eonic.Tools.Security.Impersonate
                        If oImp.ImpersonateValidUser(myWeb.moConfig("AdminAcct"), myWeb.moConfig("AdminDomain"), myWeb.moConfig("AdminPassword"), True, myWeb.moConfig("AdminGroup")) Then

                            Dim appPath As String = myWeb.moRequest.ApplicationPath
                            If appPath.EndsWith("ewcommon") Then
                                CommaSeparatedFilenames = CommaSeparatedFilenames.Replace("~/", "~/../")
                            End If

                            'set the services urls list and call the handler request
                            Dim oCssWebClient As CssWebClient = New CssWebClient(myWeb.moCtx) With {.ServiceUrlsList = Split(CommaSeparatedFilenames, ",").ToList()}
                            oCssWebClient.SendCssHttpHandlerRequest()

                            Dim scriptFile As String = ""
                            Dim fsh As New Eonic.fsHelper(myWeb.moCtx)
                            fsh.initialiseVariables(fsHelper.LibraryType.Style)

                            scriptFile = String.Format("{0}/style.css", TargetFile)
                            Dim info As Byte() = New System.Text.UTF8Encoding(True).GetBytes(oCssWebClient.FullCssFile)
                            fsh.DeleteFile(goServer.MapPath("/css" & scriptFile.TrimStart("~")))

                            scriptFile = "/css" & fsh.SaveFile("style.css", TargetFile, info)

                            If scriptFile.StartsWith("/css" & TargetFile.TrimStart("~")) Then
                                sReturnString += scriptFile
                            End If


                            'hdlrClient will store the resultant cssSplits, store them to disk and application state, and return the file list to the xslt transformation
                            For i As Integer = 0 To oCssWebClient.CssSplits.Count - 1
                                scriptFile = String.Format("{0}/style{1}.css", TargetFile, i)

                                info = New System.Text.UTF8Encoding(True).GetBytes(oCssWebClient.CssSplits(i))
                                fsh.DeleteFile(goServer.MapPath("/css" & TargetFile.TrimStart("~") & "/" & String.Format("style{0}.css", i)))

                                scriptFile = "/css" & fsh.SaveFile(String.Format("style{0}.css", i), TargetFile, info)

                                If scriptFile.StartsWith("/css" & TargetFile.TrimStart("~")) Then
                                    'file has been saved successfully.
                                    sReturnString += "," & scriptFile
                                Else
                                    'we have a file save error we should try again next request.
                                    sReturnError += scriptFile
                                End If
                            Next
                            If sReturnString.StartsWith("/css") Then
                                myWeb.moCtx.Application.Set(TargetFile, sReturnString)
                            Else
                                sReturnString = sReturnString & sReturnError
                            End If

                            oCssWebClient = Nothing
                            fsh = Nothing
                            If Not IsNothing(oImp) Then
                                oImp.UndoImpersonation()
                                oImp = Nothing
                            End If
                        End If

                    End If
                End If

                Return sReturnString
                sReturnString = Nothing

            Catch ex As Exception
                'OnComponentError(myWeb, New Eonic.Tools.Errors.ErrorEventArgs("xslt.BundleCSS", "LayoutActions", ex, CommaSeparatedFilenames))

                'Return ex.StackTrace.Replace(vbCr & vbLf, String.Empty).Replace(vbLf, String.Empty).Replace(vbCr, String.Empty) & ex.Message

                Return ex.Message
            End Try

        End Function

#End Region

    End Class

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

        Public myWeb As Web
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
                    msXslFile = value


                    ClassName = msXslFile.Substring(msXslFile.LastIndexOf("\") + 1)
                    ClassName = className.Replace(".", "_")
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

                        CalledType = assemblyInstance.GetType(className, True)

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
                    transformException = ex
                    returnException("Eonic.XmlHelper.Transform", "XslFilePath.Set", ex, msXslFile, value, gbDebug)
                    bError = True
                End Try
            End Set
        End Property

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
                    returnException("Eonic.XmlHelper.Transform", "XSLFile.Set", ex, msXslFile, value)
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
                returnException("Eonic.XmlHelper.Transform", "New", ex, msXslFile, sProcessInfo, mbDebug)
                bError = True
            End Try
        End Sub

        Public Sub New(ByRef aWeb As Web, ByVal sXslFile As String, ByVal bCompiled As Boolean, Optional ByVal nTimeoutSec As Long = 15000, Optional recompile As Boolean = False)
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
                    ClearXSLTassemblyCache()
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
                returnException("Eonic.XmlHelper.Transform", "New", ex, msXslFile, sProcessInfo, mbDebug)
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
                returnException("Eonic.XmlHelper.Transform", "Dispose", ex, msXslFile, sProcessInfo, mbDebug)

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
                returnException("Eonic.XmlHelper.TransformTimed", "Process", ex, msXslFile, sProcessInfo, mbDebug)
                oResponse.Write(msException)
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
                returnException("Eonic.XmlHelper.TransformTimed", "Process", ex, msXslFile, sProcessInfo)
                oWriter.Write(msException)
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
                returnException("Eonic.XmlHelper.Transform", "Process", ex, msXslFile, sProcessInfo, mbDebug)
                oWriter.Write(msException)
                bError = True
                Return Nothing
            End Try
        End Function

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

                    If msException = "" Then
                        'Run transformation

                        ' Dim xsltDomainProxy As ProxyDomain = xsltDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, GetType(ProxyDomain).FullName)
                        ' xsltDomainProxy._LocalContext = myWeb.moCtx
                        ' Dim responseString As String = xsltDomainProxy.RunTransform(AssemblyPath, ClassName, oXml.OuterXml)
                        ' oResponse.Write(responseString)

                        oCStyle.Transform(oReader, xsltArgs, XmlWriter.Create(oResponse.OutputStream, ws), resolver)
                    Else
                        oResponse.Write(msException)
                    End If
                    oReader.Close()
                    sWriter.Dispose()

                Else

                    'Change the xmlDocument to xPathDocument to improve performance
                    Dim oXmlNodeReader As XmlNodeReader = New XmlNodeReader(oXml)
                    Dim xpathDoc As XPath.XPathDocument = New XPath.XPathDocument(oXmlNodeReader)

                    If msException = "" Then
                        'Run transformation
                        oStyle.Transform(xpathDoc, xsltArgs, oResponse.OutputStream, Nothing)
                    Else
                        oResponse.Write(msException)
                    End If

                    oXmlNodeReader.Close()
                    xpathDoc = Nothing

                End If
            Catch ex As Exception
                transformException = ex
                returnException("Eonic.XmlHelper.Transform", "Process", ex, msXslFile, sProcessInfo, mbDebug)
                oResponse.Write(msException)
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

                If (currenti = CInt("&H9") OrElse _
                    currenti = CInt("&HA") OrElse _
                    currenti = CInt("&HD")) OrElse _
                ((currenti >= CInt("&H20")) AndAlso (currenti <= CInt("&HD7FF"))) OrElse _
                ((currenti >= CInt("&HE000")) AndAlso (currenti <= CInt("&HFFFD"))) OrElse _
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
                    If msException = "" Then
                        'Run transformation 
                        Dim oReader As Xml.XmlTextReader = New XmlTextReader(New StringReader(oXml.OuterXml))
                        Dim sWriter As IO.StringWriter = New IO.StringWriter
                        oCStyle.Transform(oReader, xsltArgs, oWriter)
                    Else
                        oWriter.Write(msException)
                    End If

                Else
                    'Change the xmlDocument to xPathDocument to improve performance
                    Dim xpathDoc As XPath.XPathDocument = New XPath.XPathDocument(New XmlNodeReader(oXml))

                    If msException = "" Or msException Is Nothing Then
                        'Run transformation
                        oStyle.Transform(xpathDoc, xsltArgs, oWriter, Nothing)
                    Else
                        oWriter.Write(msException)
                    End If

                End If
            Catch ex As Exception
                transformException = ex
                returnException("Eonic.XmlHelper.Transform", "Process", ex, msXslFile, sProcessInfo, mbDebug)
                oWriter.Write(msException)
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

                    If msException = "" Then
                        'Run transformation
                        oStyle.Transform(oReader, xsltArgs, oWriter)
                    Else
                        oWriter.Write(msException)
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

                    If msException = "" Then
                        'Run transformation
                        oStyle.Transform(xpathDoc, xsltArgs, oWriter, Nothing)
                    Else
                        oWriter.Write(msException)
                    End If


                End If
                Dim oXMLNew As New XmlDocument
                oXml.InnerXml = oWriter.ToString
                Return oXml
            Catch ex As Exception
                bError = True
                currentError = ex
                returnException("Eonic.XmlHelper.Transform", "ProcessDocument", ex, msXslFile, sProcessInfo, mbDebug)
                oWriter.Write(msException)
                Return Nothing
            End Try
        End Function

        <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)> _
        Public Shared Function LoadLibrary(<[In](), MarshalAs(UnmanagedType.LPStr)> ByVal lpFileName As String) As IntPtr
        End Function
        <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)> _
        Public Shared Function GetModuleHandle(ByVal lpModuleName As String) As IntPtr
        End Function
        <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)> _
        Public Shared Function FreeLibrary(<[In]()> ByVal hModule As IntPtr) As Boolean
        End Function


        Public Function ClearXSLTassemblyCache() As Boolean
            Dim sProcessInfo As String = "ClearXSLTassemblyCache"
            Try

                Dim oImp As Eonic.Tools.Security.Impersonate = New Eonic.Tools.Security.Impersonate
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
                            returnException("Eonic.XmlHelper.Transform", "ClearXSLTassemblyCache", ex2, msXslFile, sProcessInfo)
                        End Try
                    Next

                    oImp.UndoImpersonation()
                End If


                'reset config to on
                Eonic.Config.UpdateConfigValue(myWeb, "eonic/web", "CompliedTransform", "on")

                'di.Delete(True)

            Catch ex As Exception
                bError = True
                returnException("Eonic.XmlHelper.Transform", "ClearXSLTassemblyCache", ex, msXslFile, sProcessInfo, mbDebug)
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
            Dim output As String

            Try
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

                If output.Contains("error") Then
                    Return output
                Else
                    Return classname
                End If

            Catch ex As Exception
                bError = True
                returnException("Eonic.XmlHelper.Transform", "CompileXSLTassembly", ex, msXslFile, sProcessInfo, mbDebug)
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

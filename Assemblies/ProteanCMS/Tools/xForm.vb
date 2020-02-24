Option Strict Off
Option Explicit On
Imports System.Xml
Imports System.Data
Imports System.IO
Imports System.Web.HttpUtility
Imports System.Text.RegularExpressions
Imports VB = Microsoft.VisualBasic
Imports System.Web.Configuration
Imports System.Configuration
Imports System



Public Class xForm

    Public moCtx As System.Web.HttpContext

    Public goApp As System.Web.HttpApplicationState
    Public goRequest As System.Web.HttpRequest
    Public goResponse As System.Web.HttpResponse
    Public goSession As System.Web.SessionState.HttpSessionState
    Public goServer As System.Web.HttpServerUtility

    Private bValid As Boolean = False
    Private cValidationError As String = ""
    Private _formParameters() As String = Nothing

    Public moPageXML As XmlDocument = New XmlDocument
    Public moXformElmt As XmlElement
    Public model As XmlElement
    Private oInstance As XmlElement
    Private oInitialInstance As XmlElement
    Public mnUserId As Long

    Protected cLanguage As String = ""

    Public result As XmlDocument
    Public bProcessRepeats As Boolean = True

    Public BindsToSkip As New System.Collections.Specialized.StringCollection

    Private Const mcModuleName As String = "Protean.Cms.xForm"
    Private bTriggered As Boolean = False
    Private bDeleted As Boolean = False

    Private _stopHackyLocalPathCheck As Boolean = False

    Private _preLoadedInstanceXml As String = ""

    Private sNoteTypes() As String = {"help", "hint", "alert"}
    Private sBindAttributes() As String = {"calculate", "constraint", "readonly", "relevant", "required", "type"}



    Enum noteTypes
        Help = 0
        Hint = 1
        Alert = 2
    End Enum

    Enum ApperanceTypes
        Full = 0
        Minimal = 1
    End Enum

    Enum BindAttributes
        Calculate = 0
        Constraint = 1
        ReadOnlyBind = 2
        Relevant = 3
        Required = 4
        Type = 5
    End Enum

    Property valid() As Boolean
        Get
            Return bValid
        End Get
        Set(ByVal Value As Boolean)
            bValid = Value
        End Set
    End Property

    Public Property Instance() As XmlElement
        Get
            Return oInstance
        End Get
        Set(ByVal value As XmlElement)
            LoadInstance(value)

        End Set
    End Property

    Public Property isTriggered() As Boolean
        Get
            Return bTriggered
        End Get
        Set(ByVal value As Boolean)
            bTriggered = value
        End Set
    End Property

    Public ReadOnly Property RootGroup() As XmlElement
        Get
            Dim oRootGroup As XmlElement = Nothing
            Try
                If Tools.Xml.NodeState(Me.moXformElmt, "group") = Tools.Xml.XmlNodeState.NotInstantiated Then

                    oRootGroup = Nothing
                Else
                    oRootGroup = Me.moXformElmt.SelectSingleNode("group")
                End If
            Catch ex As Exception
                returnException(mcModuleName, "RootGroup-xmlElement", ex, "", "", gbDebug)
            End Try
            Return oRootGroup
        End Get
    End Property

    Public Property FormParameters() As String()
        Get
            Return _formParameters
        End Get
        Set(ByVal value As String())
            _formParameters = value
        End Set
    End Property

    Public Property PreLoadedInstanceXml() As String
        Get
            Return _preLoadedInstanceXml
        End Get
        Set(ByVal value As String)
            _preLoadedInstanceXml = value
        End Set
    End Property

    ReadOnly Property validationError() As String
        Get
            Return cValidationError
        End Get
    End Property

    Public Sub New()

        Me.New(System.Web.HttpContext.Current)

    End Sub

    Public Sub New(ByVal Context As System.Web.HttpContext)

        Dim sProcessInfo As String = ""
        Try
            moCtx = Context

            If Not moCtx Is Nothing Then
                goApp = moCtx.Application
                goRequest = moCtx.Request
                goResponse = moCtx.Response
                goSession = moCtx.Session
                goServer = moCtx.Server
            End If

        Catch ex As Exception
            returnException(mcModuleName, "New", ex, "", "", gbDebug)
        End Try
    End Sub



    Sub LoadInstance(ByVal xml As XmlDocument)
        ' add instance to model from xmlDocument
        oInstance = xml.DocumentElement

        If oInitialInstance Is Nothing Then
            oInitialInstance = oInstance.Clone()
        End If

        processRepeats(moXformElmt)
    End Sub

    Sub LoadInstance(ByVal oElmt As XmlElement)
        Dim cProcessInfo As String = ""

        Try
            ' add instance to model from xmlDocument
            If oInstance Is Nothing Then
                oInstance = oElmt
            Else
                oInstance.InnerXml = oElmt.InnerXml
            End If

            If oInitialInstance Is Nothing Then
                oInitialInstance = oInstance.Clone()
            End If

            processRepeats(moXformElmt)

        Catch ex As Exception
            returnException(mcModuleName, "Loadinstance-xmlElement", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub

    Sub LoadInstanceFromInnerXml(ByVal xml As String)
        ' add instance to model from xmlDocument
        oInstance.InnerXml = xml
        Instance = oInstance
        'load(oInstance, bProcessRepeats)
        'If oInitialInstance Is Nothing Then
        '    oInitialInstance = oInstance.Clone()
        'End If
        'processRepeats(moXformElmt)
    End Sub

    Sub LoadInstance(ByVal path As String)
        ' add instance to model from file path
    End Sub

    Sub updateInstance(ByVal oElmt As XmlElement)
        Dim cProcessInfo As String = ""

        Try

            'Dim oSb As Text.StringBuilder = New Text.StringBuilder

            'Dim oWriter As Xml.XmlTextWriter = New Xml.XmlTextWriter(New StringWriter(oSb))

            ''step through all nodes and attributes from the provided data and populate the instance allready specified.
            ''using microsoft DiffPatch Tool.

            'Dim oDiff As Microsoft.XmlDiffPatch.XmlDiff = New Microsoft.XmlDiffPatch.XmlDiff
            'oDiff.Options = Microsoft.XmlDiffPatch.XmlDiffOptions.IgnorePI Or Microsoft.XmlDiffPatch.XmlDiffOptions.IgnoreComments

            'oDiff.Compare(instance, oElmt, oWriter)
            'Dim oXml As XmlDocument = New XmlDocument
            'Dim oNode As XmlElement

            'oXml.LoadXml(oSb.ToString)
            'Dim nsMgr As XmlNamespaceManager = getNsMgr(oXml.DocumentElement, oXml)
            'nsMgr.AddNamespace("xd", "http://schemas.microsoft.com/xmltools/2002/xmldiff")

            ''step through and delete the remove instructions
            'For Each oNode In oXml.DocumentElement.SelectNodes("descendant-or-self::xd:remove", nsMgr)
            '    oNode.ParentNode.RemoveChild(oNode)
            'Next

            'Dim oReader As Xml.XmlTextReader = New XmlTextReader(New StringReader(oXml.OuterXml))

            'Dim oPatch As Microsoft.XmlDiffPatch.XmlPatch = New Microsoft.XmlDiffPatch.XmlPatch
            'oPatch.Patch(instance, oReader)

            'oDiff = Nothing
            'oReader = Nothing
            'oPatch = Nothing
            'After all this we have an elaborate copy!

            'Dim ds As DataSet = New DataSet
            'Dim ds2 As DataSet = New DataSet
            'Dim oReader As Xml.XmlTextReader = New XmlTextReader(New StringReader(instance.InnerXml))
            'Dim oReader2 As Xml.XmlTextReader = New XmlTextReader(New StringReader(oElmt.InnerXml))
            'ds.ReadXml(oReader)
            'ds2.ReadXml(oReader2)
            'ds.Merge(ds2)
            'instance.InnerXml = ds.GetXml

            Instance = oElmt


        Catch ex As Exception
            returnException(mcModuleName, "Patchinstance-xmlElement", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub


    Sub NewFrm(Optional ByVal sName As String = "")
        Dim oFrmElmt As XmlElement
        Dim cProcessInfo As String = ""
        Try
            oFrmElmt = moPageXML.CreateElement("Content")
            oFrmElmt.SetAttribute("type", "xform")
            If sName <> "" Then
                oFrmElmt.SetAttribute("name", sName)
            End If
            model = moPageXML.CreateElement("model")
            oInstance = moPageXML.CreateElement("instance")
            model.AppendChild(oInstance)
            oFrmElmt.AppendChild(model)
            moXformElmt = oFrmElmt
        Catch ex As Exception
            returnException(mcModuleName, "NewFrm", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub

    Public Sub submission(ByVal id As String, ByVal action As String, ByVal method As String, Optional ByVal submitEvent As String = "")
        Dim oSubElmt As XmlElement
        Dim cProcessInfo As String = ""
        Try
            '  If moXformElmt Is Nothing Then Me.NewFrm()
            If moPageXML.SelectSingleNode("submission[@id='" & id & "']") Is Nothing Then
                oSubElmt = moPageXML.CreateElement("submission")
                oSubElmt.SetAttribute("id", id)
                'TS change because instance not a child of model for some reason
                model.InsertAfter(oSubElmt, model.SelectSingleNode("instance"))
                'model.InsertAfter(oSubElmt, oInstance)
            Else
                oSubElmt = moPageXML.SelectSingleNode("submission[@id='" & id & "']")
            End If

            oSubElmt.SetAttribute("action", action)
            oSubElmt.SetAttribute("method", method)
            oSubElmt.SetAttribute("event", submitEvent)

        Catch ex As Exception
            returnException(mcModuleName, "submission", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub

    ''' <summary>
    ''' Loads an xform from a filepath
    ''' </summary>
    ''' <param name="cXformPath">The filepath relative to Server.MapPath</param>
    ''' <returns>
    ''' <para>Boolean</para>
    ''' <list>
    ''' <item>True - if the file exists and can be loaded into the xform</item>
    ''' <item>False - if the file does not exist, or an error is encountered while loading the file.</item>
    ''' </list></returns>
    ''' <remarks></remarks>
    Public Overridable Function load(ByVal cXformPath As String) As Boolean

        Dim fileExists As Boolean = False
        Dim returnValue As Boolean = False
        Dim oFrmElmt As XmlElement
        Dim oXformDoc As XmlDocument = New XmlDocument
        Dim cProcessInfo As String = "Loading..." & cXformPath

        Try
            ' Test for existence of the filepath
            fileExists = IO.File.Exists(goServer.MapPath(cXformPath))

            'oXformDoc.PreserveWhitespace = True

            ' If no joy, then test for the filepath trimmed of leading /
            ' Is this here to account for local path errors, or double slashes maybe?
            ' AG note - I don't understand why this is here, it mollycoddles bad web.config inputs.
            ' and prevents the client folder check working if the site is running from ewcommon say.
            ' so I have added the variable _stopHackyLocalPathCheck to avoid if alternative path checks exists 
            If Not fileExists _
                AndAlso cXformPath.StartsWith("/") _
                AndAlso cXformPath.Length > 2 _
                AndAlso Not _stopHackyLocalPathCheck Then
                cXformPath = cXformPath.Substring(1)
                fileExists = IO.File.Exists(goServer.MapPath(cXformPath))
            End If

            ' If we have found a file then try to load it into the xform
            If fileExists Then
                oXformDoc.Load(goServer.MapPath(cXformPath))
                oFrmElmt = moPageXML.CreateElement("Content")
                oFrmElmt.SetAttribute("type", "xform")
                oFrmElmt.SetAttribute("name", oXformDoc.DocumentElement.GetAttribute("name"))
                oFrmElmt.InnerXml = oXformDoc.DocumentElement.InnerXml

                moXformElmt = oFrmElmt
                processFormParameters()

                'set the model and instance
                model = moXformElmt.SelectSingleNode("model")

                Instance = model.SelectSingleNode("instance")

                ' Not used at the moment - intended for repeats where the instance needs to be loaded pre load PreLoadInstance()
                ' processRepeats(moXformElmt)


                'XformInclude Features....
                Dim oInc As XmlElement

                Dim moThemeConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/theme")
                Dim currentTheme As String = ""
                If Not moThemeConfig Is Nothing Then
                    currentTheme = moThemeConfig("CurrentTheme")
                End If


                For Each oInc In moXformElmt.SelectNodes("descendant-or-self::ewInclude")

                    Dim filepath As String = oInc.GetAttribute("filePath").Replace("ewthemes", "ewthemes/" & currentTheme)
                    Dim xpath As String = oInc.GetAttribute("xPath")
                    Dim LoadDoc As New XmlDocument
                    If IO.File.Exists(goServer.MapPath(filepath)) Then
                        LoadDoc.Load(goServer.MapPath(filepath))
                        Dim newElmt As XmlElement = moXformElmt.OwnerDocument.CreateElement("new")
                        If Not LoadDoc.SelectSingleNode(xpath) Is Nothing Then
                            newElmt.InnerXml = LoadDoc.SelectSingleNode(xpath).OuterXml
                            oInc.ParentNode.ReplaceChild(newElmt.FirstChild, oInc)
                        Else
                            oInc.InnerText = "xPath Not Found"
                        End If
                    Else
                        If filepath.Contains("ewthemes") Then
                            oInc.ParentNode.RemoveChild(oInc)
                        Else
                            oInc.InnerText = "File Not Found"
                        End If
                    End If
                Next

                returnValue = True
            End If





            ' Return the value
            Return returnValue

        Catch ex As Exception
            returnException(mcModuleName, "load", ex, "", cProcessInfo, gbDebug)
            ' If any problems are encountered return false.
            ' Return False
        End Try
    End Function

    ''' <summary>
    ''' Loads an xform from a filepath, while checking some alternative paths
    ''' </summary>
    ''' <param name="xformPath">Is the path of the xform to be loaded e.g. /myxform.xml</param>
    ''' <param name="AlternativePrefixFolders">Is a string arrary of possible prefixed folders to try out e.g. {"/test1","/test2"} would try /test1/myxform.xml etc. Note that this sets the preferred order to try out.  If a match is made, then no other folders are tried.</param>
    ''' <returns>
    ''' <para>Boolean</para>
    ''' <list>
    ''' <item>True - if the file was found either natively or in the modified prefix paths</item>
    ''' <item>False - if the file was not found, or an error was encountered while loading the file.</item>
    ''' </list></returns>
    ''' <remarks></remarks>
    Public Function load(ByVal xformPath As String, ByVal AlternativePrefixFolders As String()) As Boolean

        Dim filePath As String = ""
        Dim returnValue As Boolean = False
        Try

            If AlternativePrefixFolders.Length > 1 Then _stopHackyLocalPathCheck = True

            ' Test for local path
            filePath = xformPath
            returnValue = load(filePath)

            ' If no local path, then run through the Alternative in order
            If Not returnValue Then

                ' Tidy up the path for prefixing
                xformPath = "/" & xformPath.TrimStart("/\".ToCharArray)


                For Each AltFolder As String In AlternativePrefixFolders
                    If Not String.IsNullOrEmpty(AltFolder) Then
                        filePath = AltFolder.TrimEnd("/\".ToCharArray) & xformPath
                        returnValue = load(filePath)

                        ' If anything loads then let's get out of here
                        If returnValue Then Exit For
                    End If
                Next
            End If

            Return returnValue

        Catch ex As Exception
            returnException(mcModuleName, "load(String,String())", ex, "", filePath, gbDebug)
            Return False
        End Try
    End Function


    Public Sub load(ByRef oNode As XmlNode, Optional ByVal bWithRepeats As Boolean = False)
        Dim cProcessInfo As String = ""
        Dim oElmt As XmlElement
        Try
            'loads both model and xform from a string of xml
            oElmt = oNode
            load(oElmt, bWithRepeats)

        Catch ex As Exception
            returnException(mcModuleName, "load", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub

    Overridable Sub load(ByRef oElmt As XmlElement, Optional ByVal bWithRepeats As Boolean = False)
        Dim cProcessInfo As String = ""
        'Boolean to determine if the XML has been loaded from a file
        'Dim bXmlLoad As Boolean = False
        Try


            bProcessRepeats = bWithRepeats

            'If SRC Node exists, then use that xform instead
            If Not oElmt.SelectSingleNode("descendant-or-self::model") Is Nothing Then
                If Not oElmt.SelectSingleNode("descendant-or-self::model").Attributes.GetNamedItem("src") Is Nothing Then
                    Dim cFilePath As String = oElmt.SelectSingleNode("descendant-or-self::model").Attributes.GetNamedItem("src").Value.ToString
                    If load(cFilePath) Then
                        'bXmlLoad = True
                        oElmt.InnerXml = moXformElmt.InnerXml
                    End If
                End If
            End If


            'Moved Lower Section into this for now as is it needed, doesn't the file load path method do it?
            'Moved back again
            'If Not bXmlLoad Then
            moXformElmt = oElmt
            'set the model and instance
            model = moXformElmt.SelectSingleNode("descendant-or-self::model")
            If Not model Is Nothing Then
                If bWithRepeats And Not goSession Is Nothing Then

                    If goSession("tempInstance") Is Nothing Then
                        Instance = model.SelectSingleNode("descendant-or-self::instance")
                    Else
                        Instance = goSession("tempInstance")
                    End If

                    If isTriggered Then
                        'we have clicked a trigger so we must update the instance
                        updateInstanceFromRequest()
                        'lets save the instance
                        goSession("tempInstance") = Instance
                    Else
                        'This has moved into validate as we must ensure valid form prior to removal
                        'goSession("tempInstance") = Nothing

                    End If

                Else
                    oInstance = model.SelectSingleNode("descendant-or-self::instance")
                End If

            End If
            'End If


        Catch ex As Exception
            returnException(mcModuleName, "load", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub

    Sub loadtext(ByVal cXform As String)
        Dim cProcessInfo As String = ""
        Dim oFrmElmt As XmlElement
        Dim oXformDoc As XmlDocument = New XmlDocument

        Try
            'loads both model and xform from a string of xml

            oXformDoc.Load(New IO.StringReader(cXform))
            oFrmElmt = moPageXML.CreateElement("Content")
            oFrmElmt.InnerXml = oXformDoc.DocumentElement.OuterXml
            moXformElmt = oFrmElmt.FirstChild


            'set the model and instance
            model = moXformElmt.SelectSingleNode("model")
            Instance = model.SelectSingleNode("instance")

        Catch ex As Exception
            returnException(mcModuleName, "submission", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub

    Public Sub validate()

        Dim bIsValid As Boolean = True
        Dim bIsThisBindValid As Boolean = True
        Dim oBindNode As XmlNode
        Dim oBindElmt As XmlElement
        Dim sAttribute As String
        Dim updateElmt As XmlElement
        'Dim sBind As String
        'Dim cNode As String
        'Dim cNsURI As String
        Dim sXpath As String
        Dim sXpathNoAtt As String
        Dim sMessage As String = ""
        Dim obj As Xml.XPath.XPathNodeIterator
        Dim objValue As Object
        Dim missedError As Boolean = False

        Dim cProcessInfo As String = ""
        Try

            ' validates an html form submission against a bind requirements
            ' updates xform or loads result.
            'get our bind node

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12

            Dim nsMgr As XmlNamespaceManager = Protean.Tools.Xml.getNsMgrRecursive(oInstance.SelectSingleNode("*[1]"), moPageXML)

            ''' HANDLING FOR GOOGLE ReCAPTCHA
            If Not moXformElmt.SelectSingleNode("descendant-or-self::*[contains(@class,'recaptcha') and not(ancestor::instance)]") Is Nothing Then
                cValidationError = "<span class=""msg-1032"">Please confirm you are not a robot</span>"
                bIsValid = False
                missedError = True
            End If

            If goRequest("g-recaptcha-response") <> "" Then

                Dim moConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/web")
                Dim recap As New Protean.Tools.RecaptchaV2.Recaptcha()
                Dim recapResult As Protean.Tools.RecaptchaV2.RecaptchaValidationResult = recap.Validate(goRequest("g-recaptcha-response"), moConfig("ReCaptchaKeySecret"))

                If recapResult.Succeeded Then
                    cValidationError = ""
                    bIsValid = True
                    missedError = False
                End If

            End If
            ''' END HANDLING FOR GOOGLE ReCAPTCHA
            ''' 


            'lets get all the binds but check that they don't occur in the instance
            For Each oBindNode In model.SelectNodes("descendant-or-self::bind[not(ancestor::instance)]")
                oBindElmt = oBindNode
                sXpath = ""
                bIsThisBindValid = True

                'NB REMOVE THIS
                If oBindElmt.GetAttribute("type") = "fileUpload" Then
                    Dim DELETE As String = ""
                End If
                'NB

                Dim xPathNav As Xml.XPath.XPathNavigator = oInstance.CreateNavigator()
                Dim xPathNav2 As Xml.XPath.XPathNavigator = oInstance.CreateNavigator()
                Dim expr As Xml.XPath.XPathExpression
                sAttribute = ""
                If Not oBindElmt.SelectSingleNode("@nodeset") Is Nothing Then

                    'if we are an attribute then get the parent xpath
                    If InStr(oBindElmt.GetAttribute("nodeset"), "@") = 1 Then
                        sAttribute = Right(oBindElmt.GetAttribute("nodeset"), Len(oBindElmt.GetAttribute("nodeset")) - 1)
                    End If

                    sXpathNoAtt = getBindXpath(oBindElmt)
                    sXpathNoAtt = addNsToXpath(sXpathNoAtt, nsMgr)
                    If sAttribute <> "" Then
                        sXpath = sXpathNoAtt + "/@" & sAttribute
                    Else
                        sXpath = sXpathNoAtt
                    End If

                    cProcessInfo = "Error Processing:" & sXpath

                    ' Get the current object value

                    expr = xPathNav.Compile(sXpath)
                    expr.SetContext(nsMgr)
                    obj = xPathNav.Select(expr)
                    obj.MoveNext()

                    If LCase(obj.Current.Name) = "instance" Then
                        'object not found so we returns root instance we don't want all nodes returned in a dirty long string
                        objValue = ""
                    Else
                        objValue = obj.Current.Value
                    End If
                Else
                    'nodeset has not been specified so value = submitted value
                    objValue = goRequest(oBindElmt.GetAttribute("id"))
                End If

                ' Evaulate the value by type
                Dim cExtensions As String = oBindElmt.GetAttribute("allowedExt")

                ' NB : 23-01-2009 To support file upload checks
                If oBindElmt.GetAttribute("type") = "fileUpload" Then
                    If cExtensions = "" Then
                        cExtensions = "doc,docx,xls,xlsx,pdf,ppt,jpg,gif,png"
                    End If
                End If

                If oBindElmt.GetAttribute("type") <> "" Then
                    sMessage = evaluateByType(objValue, oBindElmt.GetAttribute("type"), cExtensions, LCase(oBindElmt.GetAttribute("required")) = "true()")
                End If

                If sMessage <> "" Then
                    bIsValid = False
                    bIsThisBindValid = False
                    If addNoteFromBind(oBindElmt, noteTypes.Alert, BindAttributes.Type, sMessage) = False Then
                        missedError = True
                    End If
                    cValidationError += oBindElmt.GetAttribute("id") & " - " & sMessage
                End If

                'case for calculate
                If Not oBindElmt.GetAttribute("calculate") = Nothing And bIsThisBindValid Then

                    ' Get the current object value
                    expr = xPathNav2.Compile(oBindElmt.GetAttribute("calculate"))
                    expr.SetContext(nsMgr)
                    Dim sValue2 As String = CStr(xPathNav2.Evaluate(expr))

                    If sAttribute <> "" Then
                        updateElmt = oInstance.SelectSingleNode(sXpathNoAtt, nsMgr)
                        updateElmt.SetAttribute(sAttribute, sValue2)
                    Else

                        If Not String.IsNullOrEmpty(sXpath) AndAlso Not oInstance.SelectSingleNode(sXpath) Is Nothing Then
                            oInstance.SelectSingleNode(sXpath).InnerText = sValue2
                            objValue = sValue2
                        End If
                    End If
                End If

                'case for required 
                If Not oBindElmt.GetAttribute("required") = Nothing And bIsThisBindValid Then

                    cProcessInfo = cProcessInfo & " - Required Compile Error: " & oBindElmt.GetAttribute("required")
                    expr = xPathNav2.Compile(oBindElmt.GetAttribute("required"))

                    cProcessInfo = cProcessInfo & " - Required Expression Error: " & oBindElmt.GetAttribute("required")
                    expr.SetContext(nsMgr)

                    ' Looking for true() or false()
                    If xPathNav2.Evaluate(expr) Then

                        ' Look for data
                        If Not objValue <> "" Then
                            ' No data - error message
                            bIsValid = False
                            bIsThisBindValid = False
                            If addNoteFromBind(oBindElmt, noteTypes.Alert, BindAttributes.Required, "<span class=""msg-1007"">This must be completed</span>") = False Then
                                missedError = True
                            End If
                            cValidationError += oBindElmt.GetAttribute("id") & " - This must be completed"
                        End If
                    End If
                End If

                'case for constraint
                If Not oBindElmt.GetAttribute("constraint") = Nothing And bIsThisBindValid Then

                    cProcessInfo = cProcessInfo & " - Constraint Compile Error: " & oBindElmt.GetAttribute("constraint")
                    Dim constraintXpath As String = addNsToXpath(oBindElmt.GetAttribute("constraint"), nsMgr)
                    'Dim constraintXpath As String = oBindElmt.GetAttribute("constraint")
                    expr = xPathNav2.Compile(constraintXpath)

                    cProcessInfo = cProcessInfo & " - Constraint Context Error: " & oBindElmt.GetAttribute("constraint")

                    expr.SetContext(nsMgr)

                    ' Evaluate the constraint
                    If Not xPathNav2.Evaluate(expr) Then
                        ' Constraint not met
                        bIsValid = False
                        bIsThisBindValid = False
                        If addNoteFromBind(oBindElmt, noteTypes.Alert, BindAttributes.Constraint, "<span class=""msg-1008"">This information must be valid</span>") Then
                            missedError = True
                        End If
                        cValidationError += oBindElmt.GetAttribute("id") & " - This information must be valid"
                    End If
                End If



                If oBindElmt.GetAttribute("unique") <> "" And bIsThisBindValid Then
                    If objValue <> "" Then
                        If isUnique(objValue, oBindElmt.GetAttribute("unique")) Then
                        Else
                            bIsValid = False
                            bIsThisBindValid = False
                            If addNoteFromBind(oBindElmt, noteTypes.Alert, BindAttributes.Constraint, "<span class=""msg-1008"">This must be unique</span>") Then
                                missedError = True
                            End If
                            cValidationError += oBindElmt.GetAttribute("id") & " - This must be unique"
                        End If
                    End If

                End If

                'case for relevant
                'case for read-only


            Next


            ' Validate any fileUpload binds that have a maximum file size specified against them.
            ' note that "number(NODE)=number(NODE)" is an XPath test to see if NODE is a number
            For Each oFileCheck As XmlElement In model.SelectNodes("descendant-or-self::bind[not(ancestor::instance) and @type='fileUpload' and number(@maxSize)=number(@maxSize)]")
                cProcessInfo = "Checking maximum file sizes"
                ' Check that the bind exists in the uploaded files
                If oFileCheck.GetAttribute("id") <> "" _
                    AndAlso Not (goRequest.Files.Item(oFileCheck.GetAttribute("id")) Is Nothing) Then

                    ' maxSize has been found and an item for that bind has been submitted.
                    ' Compare the sizes.
                    If goRequest.Files.Item(oFileCheck.GetAttribute("id")).ContentLength > CInt(oFileCheck.GetAttribute("maxSize")) * 1024 Then
                        If oFileCheck Is Nothing Then
                            missedError = True
                        Else
                            addNoteFromBind(oFileCheck, noteTypes.Alert, BindAttributes.Constraint, "<span class=""msg-1009"">The file you are uploading is too large</span>")
                        End If
                        bIsValid = False
                        bIsThisBindValid = False
                        cValidationError += oFileCheck.GetAttribute("id") & " - The file you are uploading is too large"
                    End If

                    End If

            Next

            If bIsValid And bProcessRepeats Then
                goSession("tempInstance") = Nothing
            End If

            If cValidationError <> "" And missedError Then
                'the 2nd node should be group first should be the first group
                Dim lastGroup As XmlElement = moXformElmt.SelectSingleNode("*[2]/*[last()]")

                If Not lastGroup Is Nothing Then
                    If lastGroup.Name = "submit" Then
                        addNote(lastGroup.ParentNode, noteTypes.Alert, cValidationError)
                    Else
                        addNote(lastGroup, noteTypes.Alert, cValidationError)
                    End If
                End If

            End If

            If bIsValid And Not goSession Is Nothing Then goSession("formFileUploaded") = Nothing

            oInstance.SetAttribute("valid", bIsValid.ToString.ToLower)

            valid = bIsValid

        Catch ex As Exception
            returnException(mcModuleName, "validate", ex, "", cProcessInfo, gbDebug)
        End Try

    End Sub

    Overridable Function evaluateByType(ByVal sValue As String, ByVal sType As String, Optional ByVal cExtensions As String = "", Optional ByVal isRequired As Boolean = False) As String
        Dim cProcessInfo As String = ""
        Dim cReturn As String = "" ' Set this as a clear return string

        Try
            ' Only evaulate if there is data to evaluate against!
            If sValue <> "" Then
                Select Case LCase(sType)
                    Case "float", "number"
                        ' IsNumeric is a bit rubbish
                        If Not (Tools.Number.IsReallyNumeric(sValue & "")) Then cReturn = "<span class=""msg-1000"">This must be a number</span>"
                    Case "date"
                        If Not (IsDate(sValue)) Then cReturn = "<span class=""msg-1001"">This must be a valid date</span>"
                    Case "email"
                        If Not (Tools.Text.IsEmail(sValue)) Then cReturn = "<span class=""msg-1002"">This must be a valid email address</span>"
                    Case "emails"
                        If sValue.Contains(",") Then
                            Dim sEmail As String
                            For Each sEmail In sValue.Split(",")
                                If Not (Tools.Text.IsEmail(sEmail.Trim())) Then cReturn = "<span class=""msg-1002a"">These all must be a valid email address</span>"
                            Next
                        Else
                            If Not (Tools.Text.IsEmail(sValue)) Then cReturn = "<span class=""msg-1002"">This must be a valid email address</span>"
                        End If

                    Case "imgverification"
                        If Not LCase(sValue) = LCase(goSession("imgVerification")) Then cReturn = "<span class=""msg-1003"">Please enter the correct letters and numbers as shown.</span>"
                    Case "strongpassword"
                        If Not (strongPassword(sValue & "")) Then cReturn = "<span class=""msg-1005"">This password must be stronger</span>"
                    Case "fileupload"
                        cProcessInfo = sValue

                        Dim cExtension As String = System.IO.Path.GetExtension(sValue)
                        If Not cExtensions.Contains(Right(cExtension, 3)) Then cReturn = "<span class=""msg-1004"">Invalid File Extension</span>"
                    Case Else
                        ' RegExp hack: allow the user to specify a dynamic format for type
                        If sType.StartsWith("format:") Then
                            ' Extract the regexp and compare it.
                            Dim cPattern As String = ""
                            cPattern = VB.Mid(sType, 8)
                            If cPattern <> "" Then
                                Try
                                    ' In case it's an invalid Regexp don't catch errors
                                    Dim oRe As Regex = New Regex(cPattern, RegexOptions.IgnoreCase)
                                    If Not (oRe.IsMatch(sValue)) Then cReturn = "<span class=""msg-1005"">This must be a valid format</span>"
                                Catch ex As Exception
                                    ' Do Nothing
                                End Try
                            End If
                        End If
                End Select
            ElseIf sType = "fileUpload" And isRequired Then
                ' Only show this if the file upload is required
                If goRequest.Files.Count > 0 Then cReturn = "<span class=""msg-1006"">No File Selected</span>"
            End If
            Return cReturn
        Catch ex As Exception
            returnException(mcModuleName, "evaluateByType", ex, "", cProcessInfo, gbDebug)
            Return ""
        End Try
    End Function


    Overridable Function isUnique(ByVal sValue As String, ByVal sPath As String) As Boolean
        Dim cProcessInfo As String = ""
        'Placeholder for overide
        Try
            Return True
        Catch ex As Exception
            returnException(mcModuleName, "isUnique", ex, "", cProcessInfo, gbDebug)
            Return ""
        End Try
    End Function

    Public Function addNoteFromBind(ByRef oBindElmt As XmlElement, ByVal oNoteType As noteTypes, ByVal oBindType As BindAttributes, ByVal sDefaultMessage As String) As Boolean
        Dim sRef As String = oBindElmt.GetAttribute("id")
        Dim cProcessInfo As String = "error with field - " & sRef
        'Dim sMessage As String
        Try

            If Not moXformElmt.SelectSingleNode("descendant-or-self::*[(@ref='" & sRef & "'or @bind='" & sRef & "') and @class!='hidden']") Is Nothing Then
                ' Look for a child node in the bind element that has a matching note type and bind attribute type
                Dim sXPath As String = sNoteTypes(oNoteType) & "[@type='" & sBindAttributes(oBindType) & "']"
                Dim oElmt As XmlElement = oBindElmt.SelectSingleNode(sXPath)

                ' If there's no match then check for a generic note (i.e. one without a type specified)
                If oElmt Is Nothing Then oElmt = oBindElmt.SelectSingleNode(sNoteTypes(oNoteType) & "[not(@type)]")

                ' Override the default message
                If Not oElmt Is Nothing Then
                    sDefaultMessage = oElmt.InnerText
                End If

                ' Add the note
                addNote(oBindElmt.GetAttribute("id"), oNoteType, sDefaultMessage)
                If moXformElmt.SelectSingleNode("descendant-or-self::*[(@ref='" & sRef & "'or @bind='" & sRef & "') and @class!='hidden']") Is Nothing Then
                    Return True
                Else
                    Return False
                End If
            Else
                Return False
            End If
        Catch ex As Exception
            returnException(mcModuleName, "addNoteFromBind", ex, "", cProcessInfo, gbDebug)
            Return False
        End Try

    End Function

    Sub updateInstanceFromRequest()
        'Dim sItem As String
        'Dim xPath As String
        'Dim value As String
        Dim oNodes As XmlNodeList
        Dim oNode As XmlNode
        Dim oElmt As XmlElement
        Dim oinstanceElmt As XmlElement
        Dim sXpath As String
        Dim sBind As String
        Dim sRequest As String
        Dim oBindNode As XmlElement
        Dim oBindElmt As XmlElement
        Dim sAttribute As String
        Dim sValue As String
        Dim bIsXml As Boolean
        Dim cProcessInfo As String = ""
        Dim sDataType As String

        Try

            'scan each form item

            'add the soap namespace to the nametable of the xmlDocument to allow xpath to query the namespace
            Dim nsMgr As XmlNamespaceManager = Protean.Tools.Xml.getNsMgrRecursive(oInstance.SelectSingleNode("*[1]"), moPageXML)

            oNodes = moXformElmt.SelectNodes("descendant::*[(@ref or @bind) and not(ancestor::model or ancestor::trigger or self::group or self::repeat)]")

            For Each oNode In oNodes
                bIsXml = False
                oElmt = oNode
                sAttribute = ""

                ' Readonly Textarea need to treat any value as Xml
                If oElmt.Name = "textarea" And oElmt.GetAttribute("class") = "readonly" Then bIsXml = True
                If oElmt.Name = "textarea" And InStr(oElmt.GetAttribute("class"), "xhtml") > 0 Then bIsXml = True
                If oElmt.Name = "textarea" And InStr(oElmt.GetAttribute("class"), "xml") > 0 Then bIsXml = True
                If oElmt.Name = "textarea" And InStr(oElmt.GetAttribute("class"), "xsledit") > 0 Then bIsXml = True
                If oElmt.Name = "input" And InStr(oElmt.GetAttribute("class"), "pickImage") > 0 Then bIsXml = True

                'if the ref contains the instance xpath
                sXpath = oElmt.GetAttribute("ref")
                'unfinished, enter code for handling attribute paths
                sRequest = sXpath

                'if not then there should be a binding
                If Not sXpath <> "" Then

                    sBind = oElmt.GetAttribute("bind")

                    'check for binds added in processRepeats

                    If BindsToSkip.Contains(sBind) Then

                        cProcessInfo = "Skipped Bind " & sBind
                    Else

                        sRequest = sBind
                        Try
                            'get our bind node
                            For Each oBindNode In model.SelectNodes("descendant-or-self::bind[@id='" & sBind & "']")
                                oBindElmt = oBindNode
                                sDataType = oBindElmt.GetAttribute("type")
                                Dim submittedValue As String = CStr("" & goRequest(sRequest))

                                'If we haven't specified a nodeset we are validating the value but not updating the instance.
                                If Not oBindElmt.SelectSingleNode("@nodeset") Is Nothing Then
                                    'if we are an attribute then get the parent xpath
                                    If InStr(oBindElmt.GetAttribute("nodeset"), "@") = 1 Then
                                        sAttribute = Right(oBindElmt.GetAttribute("nodeset"), Len(oBindElmt.GetAttribute("nodeset")) - 1)

                                    End If
                                    sXpath = getBindXpath(oBindElmt)
                                    sXpath = addNsToXpath(sXpath, nsMgr)

                                    'Allow the submitted value to substitue in the Xpath
                                    If InStr(sXpath, "$submittedValue") > 0 Then

                                        'first we need to reset the other values
                                        Dim optionNode As XmlElement
                                        Dim falseValue As String = oBindNode.GetAttribute("falseValue")
                                        If falseValue = "" Then falseValue = "false"
                                        For Each optionNode In oElmt.SelectNodes("item/value")
                                            Dim optionXPath As String = sXpath.Replace("$submittedValue", optionNode.InnerText)
                                            If Not oInstance.SelectSingleNode(optionXPath, nsMgr) Is Nothing Then
                                                oInstance.SelectSingleNode(optionXPath, nsMgr).InnerText = falseValue
                                            End If
                                        Next

                                        'set the actual submitted value Xpath and update the submitted value with the option selected.
                                        sXpath = sXpath.Replace("$submittedValue", submittedValue)
                                        submittedValue = oBindNode.GetAttribute("value")
                                    End If



                                    If sXpath = "" Then sXpath = "."
                                        cProcessInfo = "sXPath:" & sXpath & " value:" & submittedValue
                                        'update for each bind element match
                                        If oInstance.SelectSingleNode(sXpath, nsMgr) Is Nothing Then
                                            sValue = "invalid path"
                                            cValidationError += cValidationError & "<p>The following xpath could not be located in the instance: " & sXpath & "</p>"
                                        Else
                                            If sAttribute <> "" Then
                                                oinstanceElmt = oInstance.SelectSingleNode(sXpath, nsMgr)
                                                oinstanceElmt.SetAttribute(sAttribute, submittedValue)
                                            Else
                                                Select Case sDataType

                                                    Case "xml-replace"
                                                        Dim oElmtTemp As XmlElement
                                                        oElmtTemp = moPageXML.CreateElement("Temp")
                                                        If submittedValue Is Nothing Then
                                                            cProcessInfo = sRequest
                                                        ElseIf submittedValue = "" Then
                                                            oInstance.SelectSingleNode(sXpath, nsMgr).ParentNode.RemoveChild(oInstance.SelectSingleNode(sXpath, nsMgr))
                                                        Else
                                                            oElmtTemp.InnerXml = (Protean.Tools.Xml.convertEntitiesToCodes(submittedValue) & "").Trim
                                                            oInstance.SelectSingleNode(sXpath, nsMgr).ParentNode.ReplaceChild(oElmtTemp.FirstChild.Clone, oInstance.SelectSingleNode(sXpath, nsMgr))
                                                        End If
                                                        oElmtTemp = Nothing

                                                    Case "xml-replace-img"

                                                        'Specific behaviour for an image tag.

                                                        Dim oElmtTemp As XmlElement
                                                        oElmtTemp = moPageXML.CreateElement("Temp")
                                                        If submittedValue Is Nothing Then
                                                            cProcessInfo = sRequest
                                                        ElseIf submittedValue = "" Then
                                                            oInstance.SelectSingleNode(sXpath, nsMgr).ParentNode.RemoveChild(oInstance.SelectSingleNode(sXpath, nsMgr))
                                                        Else
                                                            oElmtTemp.InnerXml = (Protean.Tools.Xml.convertEntitiesToCodes(submittedValue) & "").Trim
                                                            oInstance.SelectSingleNode(sXpath, nsMgr).ParentNode.ReplaceChild(oElmtTemp.FirstChild.Clone, oInstance.SelectSingleNode(sXpath, nsMgr))
                                                        End If
                                                        oElmtTemp = Nothing

                                                    Case "image"
                                                        ' Submitted Image File - not image HTML
                                                        updateImageElement(oInstance.SelectSingleNode(sXpath, nsMgr), oBindElmt, goRequest.Files(sRequest))

                                                    Case "base64Binary"
                                                        Dim oElmtFile As XmlElement
                                                        oElmtFile = oInstance.SelectSingleNode(sXpath, nsMgr)

                                                        Dim oElmtFileStream As XmlElement = moPageXML.CreateElement(oElmtFile.Name & "Stream")
                                                        oElmtFile.ParentNode.InsertAfter(oElmtFileStream, oElmtFile)

                                                        Dim fUpld As System.Web.HttpPostedFile
                                                        fUpld = goRequest.Files(sRequest)

                                                        oElmtFile.InnerText = Tools.Text.filenameFromPath(fUpld.FileName)

                                                        Dim body As System.IO.Stream = fUpld.InputStream

                                                        'body.Read(bodyBytes, 0, fUpld.ContentLength - 1)
                                                        '
                                                        Dim encoding As System.Text.Encoding = System.Text.Encoding.Default
                                                        Dim reader As New System.IO.StreamReader(body)
                                                        Dim bodyBytes As Byte() = (New System.Text.UTF8Encoding).GetBytes(reader.ReadToEnd)

                                                        oElmtFileStream.InnerText = Convert.ToBase64String(bodyBytes)
                                                    Case "base64Binary_Leave"
                                                        'Do nothing, this is left to another part of the program to use
                                                        'all we will do is pput the name of the file in

                                                        oInstance.SelectSingleNode(sXpath, nsMgr).InnerXml = Protean.Tools.Xml.convertEntitiesToCodes(Tools.Text.filenameFromPath(goRequest.Files(sRequest).FileName) & "").Trim

                                                    Case "fileUpload"
                                                        Dim cSavePath As String = oBindElmt.GetAttribute("saveTo")
                                                        Dim cExtensions As String = oBindElmt.GetAttribute("allowedExt")
                                                        Dim bAddTimeStamp As Boolean = (oBindElmt.GetAttribute("timeStamp") = "true")
                                                        Dim Filename As String


                                                        If cExtensions = "" Then
                                                            cExtensions = "doc,docx,xls,xlsx,pdf,ppt,jpg,gif,png"
                                                        End If

                                                        If (goRequest.Files.Count > 0) _
                                                       AndAlso Not (goRequest.Files.Count = 1 And goRequest.Files(0).ContentLength = 0) Then
                                                            Dim oFile As System.Web.HttpPostedFile = goRequest.Files(0)

                                                        cSavePath = cSavePath.Replace("$userId$", mnUserId)
                                                        cSavePath = cSavePath.Replace("$id$", goRequest("id"))
                                                        cSavePath.Replace("/", "\")
                                                            If Not cSavePath.EndsWith("\") Then
                                                                cSavePath &= "\"
                                                            End If

                                                            If bAddTimeStamp Then
                                                                Filename = System.IO.Path.GetFileNameWithoutExtension(oFile.FileName) & Now().ToString("yyyyMMddhhmmss") & "." & System.IO.Path.GetExtension(oFile.FileName)
                                                            Else
                                                                Filename = System.IO.Path.GetFileName(oFile.FileName)
                                                            End If

                                                            Dim upload As Boolean = False
                                                            If cSavePath <> "" Then
                                                                Dim cExtension As String = System.IO.Path.GetExtension(Filename)
                                                                If cExtensions.Contains(Right(cExtension, 3)) Then
                                                                    upload = True
                                                                Else
                                                                    sValue = "invalid file extension"
                                                                    cValidationError += cValidationError & "<p>Invalid File Extension: " & cExtension & "</p>"
                                                                    'NB Added to test bad extensions
                                                                    oInstance.SelectSingleNode(sXpath, nsMgr).InnerText = Filename
                                                                End If
                                                                'Else
                                                                ' Why would we try to upload nothing?
                                                                '    upload = True
                                                            End If

                                                            If upload Then
                                                                ' Need a way to check directory exists, if not then create it
                                                                Dim cFullPath As String = goServer.MapPath("/").TrimEnd("/\".ToCharArray)

                                                                'ensure the folder exists
                                                                Dim oFs As New fsHelper()
                                                                oFs.mcStartFolder = cFullPath
                                                                cValidationError += oFs.CreatePath(cSavePath)
                                                                If cValidationError = "1" Then cValidationError = ""

                                                                If Directory.Exists(cFullPath & cSavePath) = True Then

                                                                    Dim cFinalFullSavePath As String = oFs.getUniqueFilename(cFullPath & cSavePath & Filename)
                                                                    oFile.SaveAs(cFinalFullSavePath)
                                                                    oInstance.SelectSingleNode(sXpath, nsMgr).InnerText = cFinalFullSavePath.Replace(cFullPath, "")

                                                                    ' Working on the assumption that only one file has been submitted, then store this in a session object
                                                                    If Not goSession Is Nothing Then goSession("formFileUploaded") = cFinalFullSavePath.Replace(cFullPath, "")
                                                                Else
                                                                    sValue = "invalid save path"
                                                                    cValidationError += cValidationError & "<p>Save Path does not exist: " & cFullPath & cSavePath & "</p>"
                                                                End If

                                                            End If

                                                        Else
                                                            'No Files

                                                            ' First check if there's a value in the session variable, which would indicate that 
                                                            ' this has been uploaded but the form had to go through a couple of stages of validation
                                                            If Not goSession Is Nothing AndAlso Not String.IsNullOrEmpty(CStr(goSession("formFileUploaded"))) Then
                                                                oInstance.SelectSingleNode(sXpath, nsMgr).InnerText = (goSession("formFileUploaded") & "").Trim
                                                            Else
                                                                sValue = "no files attached"
                                                                cValidationError += cValidationError & "<p>No Files Have Been Attached For Upload</p>"
                                                            End If


                                                        End If

                                                    Case Else
                                                        'If goRequest(sRequest) <> "" Then "This is removed because we need to clear empty checkbox forms"
                                                        If bIsXml Then
                                                            If submittedValue <> "" Then
                                                                oInstance.SelectSingleNode(sXpath, nsMgr).InnerXml = Protean.Tools.Xml.convertEntitiesToCodes(submittedValue & "").Trim
                                                            Else
                                                                oInstance.SelectSingleNode(sXpath, nsMgr).InnerXml = ""
                                                            End If
                                                        Else
                                                        'take the form value over the querystring.
                                                        oInstance.SelectSingleNode(sXpath, nsMgr).InnerText = submittedValue.Trim
                                                    End If

                                                End Select

                                            End If
                                        End If
                                    End If
                            Next
                        Catch ex2 As Exception
                            'no bind element found, do nothing
                            returnException(mcModuleName, "updateInstanceFromRequest", ex2, "", cProcessInfo, gbDebug)
                        End Try
                    End If
                Else
                    'We have an xpath to the instance, lets update the node
                    If oInstance.SelectSingleNode(sXpath, nsMgr) Is Nothing Then
                        sValue = "invalid path"
                        If sXpath <> "submit" Then
                            ' cValidationError = cValidationError & "<p>The following xpath could not be located in the instance: " & sXpath & "</p>"
                        End If
                    Else
                        If sAttribute <> "" Then
                            oinstanceElmt = oInstance.SelectSingleNode(sXpath, nsMgr)
                            oinstanceElmt.SetAttribute(sAttribute, (goRequest(sRequest) & "").Trim)
                        Else
                            oInstance.SelectSingleNode(sXpath, nsMgr).InnerText = (goRequest(sRequest) & "").Trim
                        End If
                    End If
                End If

            Next

            If cValidationError <> "" Then
                'the 2nd node should be group first should be the first group
                Dim firstGroup As XmlElement = moXformElmt.SelectSingleNode("*[2]")

                If firstGroup Is Nothing Then
                    addNote(firstGroup, noteTypes.Alert, cValidationError)
                End If

                valid = False
            End If

        Catch ex As Exception
            returnException(mcModuleName, "updateInstanceFromRequest", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub

    Public Function getRequest(ByVal sRequest As String) As String

        Dim submittedValue As String = goRequest.Form(sRequest) & ""
        If submittedValue = "" And goRequest.QueryString(sRequest) <> "" Then
            submittedValue = goRequest.QueryString(sRequest)
        End If
        Return submittedValue

    End Function





    Sub updateInstanceFromRequestOnly()
        'Dim sItem As String
        'Dim xPath As String
        'Dim value As String
        Dim oinstanceElmt As XmlElement
        Dim sXpath As String
        Dim sBind As String
        Dim oBindNode As XmlElement
        Dim oBindElmt As XmlElement
        Dim sAttribute As String = ""
        Dim sValue As String
        Dim cProcessInfo As String = ""
        Dim item As Object

        Try

            'add the soap namespace to the nametable of the xmlDocument to allow xpath to query the namespace
            Dim nsMgr As XmlNamespaceManager = getNsMgr(oInstance.FirstChild, moPageXML)

            'scan each form item
            For Each item In goRequest.Form
                sBind = CStr(item)

                For Each oBindNode In model.SelectNodes("descendant-or-self::bind[@id='" & sBind & "']")
                    oBindElmt = oBindNode
                    'if we are an attribute then get the parent xpath
                    If InStr(oBindElmt.GetAttribute("nodeset"), "@") = 1 Then
                        sAttribute = Right(oBindElmt.GetAttribute("nodeset"), Len(oBindElmt.GetAttribute("nodeset")) - 1)
                    End If
                    sXpath = getBindXpath(oBindElmt)
                    sXpath = addNsToXpath(sXpath, nsMgr)

                    'update for each bind element match
                    If oInstance.SelectSingleNode(sXpath, nsMgr) Is Nothing Then
                        sValue = "invalid path"
                        cValidationError = cValidationError & "<p>The following xpath could not be located in the instance: " & sXpath & "</p>"
                    Else
                        If sAttribute <> "" Then
                            oinstanceElmt = oInstance.SelectSingleNode(sXpath, nsMgr)
                            oinstanceElmt.SetAttribute(sAttribute, (goRequest(item) & "").Trim)
                        Else
                            oInstance.SelectSingleNode(sXpath, nsMgr).InnerText = (goRequest(item) & "").Trim
                        End If
                    End If
                Next
            Next

            If cValidationError <> "" Then
                addNote(moXformElmt, noteTypes.Alert, cValidationError)
                valid = False
            End If

        Catch ex As Exception
            returnException(mcModuleName, "updateInstanceFromRequest", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub

    ''' <summary>
    '''     <para>Uploads a submitted image and adds the details to the xform</para>
    ''' <list>
    ''' <listheader>Bind node attribute for ewImageHandler are as follows:</listheader>
    ''' <item>fileName - name of the file</item>
    ''' <item>filePath - where to save the path</item>
    ''' <item>uniqueFileName - when true, this will append a tiemstamp to the filename, hopefully making it unique.</item>
    ''' <item>maxwidth - maximum width of the image</item>
    ''' <item>maxheight - maximum height of the image</item>
    ''' <item>crop - when resizing, crop image to the dimensions required rather than stretching it</item>
    ''' <item>noStretch - when true, prevents images from being upscaled when dimensions are smaller than maxwidth or maxheight</item>
    ''' <item>quality - compression level (0-100)</item>
    ''' </list>
    ''' </summary>
    ''' <param name="imgElmt">The element in the xform instance to add references to</param>
    ''' <param name="bindElmt">The bind element</param>
    ''' <param name="postedFile">The file uploaded</param>
    ''' <remarks></remarks>
    Private Sub updateImageElement(ByRef imgElmt As XmlElement, ByRef bindElmt As XmlElement, ByRef postedFile As System.Web.HttpPostedFile)

        'TS To Do

        Dim maxWidth As Long = 0
        Dim maxHeight As Long = 0
        Dim filePath As String = "\images"
        Dim fileName As String = ""
        Dim makeFileNameUnique As Boolean = False
        Dim newFileName As String = ""
        Dim cProcessInfo As String = ""
        Dim cIsCrop As String = ""
        Dim cNoStretch As String = ""
        Dim nQuality As Long = 0
        Dim savedImgPath As String = ""
        Dim alreadyUploaded As String = ""

        Dim imgPath As String = ""
        Dim imgWidth As String = ""
        Dim imgHeight As String = ""


        Try

            If Not (postedFile.FileName = "") Then

                Dim eonicImgElmt As XmlElement = bindElmt.SelectSingleNode("ewImageHandler")

                alreadyUploaded = eonicImgElmt.GetAttribute("alreadyUploaded")


                ' Avoid duplicate uploads
                If String.IsNullOrEmpty(alreadyUploaded) Then


                    Dim oFs As fsHelper = New fsHelper

                    fileName = postedFile.FileName
                    fileName = Right(fileName, fileName.Length - fileName.LastIndexOf("\") - 1)
                    fileName = Replace(fileName, " ", "-")

                    'lets load the settings from the bind node

                    'this sits in the bind node
                    '<ewImageHandler fileName="profile_tn.jpg" filePath="/images/profiles/$userId$" maxwidth="50" maxheight="50"/>
                    Dim idPath As String = eonicImgElmt.GetAttribute("idPath")
                    Dim uniqueId As String = ""
                    If idPath <> "" Then
                        uniqueId = bindElmt.SelectSingleNode("ancestor::model/instance/" & idPath).InnerText & ""
                    End If

                    maxWidth = CLng("0" & eonicImgElmt.GetAttribute("maxwidth"))
                    maxHeight = CLng("0" & eonicImgElmt.GetAttribute("maxheight"))
                    cIsCrop = eonicImgElmt.GetAttribute("crop")
                    cNoStretch = eonicImgElmt.GetAttribute("noStretch")
                    newFileName = eonicImgElmt.GetAttribute("fileName")
                    filePath = eonicImgElmt.GetAttribute("filePath").Replace("$userId$", mnUserId)
                    If uniqueId <> "" Then filePath = filePath.Replace("$id$", uniqueId)
                    nQuality = CLng("0" & eonicImgElmt.GetAttribute("quality"))
                    makeFileNameUnique = eonicImgElmt.GetAttribute("uniqueFileName").ToLower = "true"


                    'first lets save our file

                    oFs.initialiseVariables(fsHelper.LibraryType.Image)

                    cProcessInfo = oFs.CreatePath(filePath)

                    If cProcessInfo <> "1" Then
                        Err.Raise(1009, "updateImageElement", "EonicWeb Filesystem: you don't have permissions to write to " & filePath)
                    End If

                    cProcessInfo = oFs.SaveFile(postedFile, filePath)

                    'now lets load the origonal into our image helper
                    Dim oIh As Protean.Tools.Image = New Protean.Tools.Image(fsHelper.URLToPath(goServer.MapPath("\images") & filePath & "\" & fileName))
                    'now we have loaded it into our image object we can delete it
                    oFs.DeleteFile(filePath, fileName)

                    'If Not (maxWidth = 0 Or maxHeight = 0) Then
                    If (cIsCrop = "true") Then
                        oIh.IsCrop = True
                    End If
                    If (cNoStretch = "true") Then
                        oIh.NoStretch = True
                    End If

                    'we always keep the aspect ratio
                    oIh.KeepXYRelation = True

                    oIh.SetMaxSize(maxWidth, maxHeight)
                    'and save


                    ' If duplicate filename, then update the fileName
                    If makeFileNameUnique AndAlso IO.File.Exists(fsHelper.URLToPath(goServer.MapPath("\images") & filePath & "\" & newFileName)) Then
                        ' Create a new filename
                        newFileName = newFileName.Substring(0, newFileName.LastIndexOf(".")) & "-" & Now().ToString("yyyyMMddhhmmss") & newFileName.Substring(newFileName.LastIndexOf("."))
                    End If

                    Dim newFilePath As String = fsHelper.URLToPath(goServer.MapPath("\images") & filePath & "\" & newFileName)

                    'now lets set the image element variables
                    imgPath = fsHelper.PathToURL("\images" & filePath & "\" & newFileName)
                    imgWidth = oIh.Width.ToString
                    imgHeight = oIh.Height.ToString

                    If nQuality > 0 Then
                        oIh.Save(newFilePath, nQuality)
                    Else
                        oIh.Save(newFilePath)
                    End If

                    ' To avoid duplicate uploads for duplicate bind elements, check for ewImagehandlers that match ids
                    ' By setting attributes we can pass these on to other handlers later on in processing 
                    For Each imgHandler As XmlElement In Me.model.SelectNodes("//bind[@id='" & bindElmt.GetAttribute("id") & "' and ewImageHander/@maxwidth='" & maxWidth & "'  and ewImageHander/@maxheight='" & maxHeight & "']/ewImageHandler")
                        imgHandler.SetAttribute("alreadyUploaded", imgPath)
                        imgHandler.SetAttribute("actualWidth", imgWidth)
                        imgHandler.SetAttribute("actualHeight", imgHeight)
                    Next
                Else
                    'now lets set the image element variables
                    imgPath = eonicImgElmt.GetAttribute("alreadyUploaded")
                    imgWidth = eonicImgElmt.GetAttribute("actualWidth")
                    imgHeight = eonicImgElmt.GetAttribute("actualHeight")

                End If

                imgElmt.SetAttribute("src", imgPath)
                imgElmt.SetAttribute("width", imgWidth)
                imgElmt.SetAttribute("height", imgHeight)

            End If


        Catch ex As Exception
            returnException(mcModuleName, "updateImageElement", ex, "", cProcessInfo, gbDebug)
        End Try

    End Sub

    Sub addValues()
        'takes the values from the instance and adds them as an attribute
        'This is non standard for XForm Specification but gets around the inability of xslt to evaluate dynamic xpath references 
        'This should no longer be requried in XPath2 or once xforms is available in standard browsers.

        'for this reason we are also unable to handle repeating elements in XSLT
        'We therefore use this to itterate through any repeat elements, 
        'look at the lines in the instance, and replicate using a group elements instead,
        'creating the equivenlent binds


        Dim oNodes As XmlNodeList
        Dim oNode As XmlNode
        Dim oElmt As XmlElement
        Dim oValElmt As XmlElement
        Dim oinstanceElmt As XmlElement
        Dim sXpath As String
        Dim sBind As String
        Dim oBindNode As XmlElement
        Dim oBindElmt As XmlElement
        Dim sAttribute As String
        Dim sValue As String = ""
        Dim bIsXml As Boolean
        Dim bReadOnly As Boolean = False
        Dim cProcessInfo As String = ""
        Dim sDataType As String = ""
        Dim cleanXpath As String = ""

        Try

            oNodes = moXformElmt.SelectNodes("descendant::*[(@ref or @bind) and not(ancestor::label or ancestor::model or self::group or self::repeat or self::trigger or self::delete)]")


            ' if the instance is empty we have no values to add.
            If oInstance Is Nothing Then Exit Sub

            If oInstance.SelectSingleNode("*[1]") Is Nothing Then Exit Sub

            Dim nsMgr As XmlNamespaceManager = Protean.Tools.Xml.getNsMgrRecursive(oInstance.SelectSingleNode("*[1]"), moPageXML)

            For Each oNode In oNodes
                bIsXml = False
                oElmt = oNode
                sAttribute = ""
                Dim populate As Boolean = True

                ' Readonly Textarea need to treat any value as Xml
                If oElmt.Name = "textarea" And oElmt.GetAttribute("class").Contains("readonly") Then
                    bIsXml = True
                    bReadOnly = True
                End If

                If oElmt.Name = "textarea" And InStr(oElmt.GetAttribute("class"), "xhtml") > 0 Then bIsXml = True
                If oElmt.Name = "textarea" And InStr(oElmt.GetAttribute("class"), "xml") > 0 Then bIsXml = True
                If oElmt.Name = "textarea" And InStr(oElmt.GetAttribute("class"), "xsl") > 0 Then bIsXml = True

                sXpath = oElmt.GetAttribute("ref")
                If Not sXpath <> "" Then
                    'case for bind
                    sBind = oElmt.GetAttribute("bind")
                    Try
                        'if we have more than one bind node we want to select the last one found
                        Dim oBindNodes As XmlNodeList = model.SelectNodes("descendant-or-self::bind[@id='" & sBind & "' and not(@populate='false')]")
                        If oBindNodes.Count > 0 Then
                            oBindNode = oBindNodes(oBindNodes.Count - 1)
                        Else
                            oBindNode = Nothing
                        End If



                        'no bind element found, do nothing
                        If Not oBindNode Is Nothing Then
                            oBindElmt = oBindNode
                            sDataType = oBindElmt.GetAttribute("type")

                            'If LCase(oBindElmt.GetAttribute("populate")) = "false" Then
                            '    populate = False
                            'End If

                            'if we are an attribute then get the parent xpath
                            If InStr(oBindElmt.GetAttribute("nodeset"), "@") = 1 Then
                                sAttribute = Right(oBindElmt.GetAttribute("nodeset"), Len(oBindElmt.GetAttribute("nodeset")) - 1)
                            End If
                            sXpath = getBindXpath(oBindElmt)
                            If sXpath = "" Then sXpath = "."
                            cleanXpath = sXpath

                            sXpath = addNsToXpath(sXpath, nsMgr)

                            'if bind has selected value then we need some clever stuff
                            If InStr(sXpath, "$submittedValue") > 0 Then
                                'first we step through the possible values
                                Dim valueElmt As XmlElement
                                Dim oldXpath As String = sXpath
                                Dim modifiedXpath As String
                                Dim isModified As Boolean = False
                                For Each valueElmt In oElmt.SelectNodes("item/value")
                                    modifiedXpath = oldXpath.Replace("$submittedValue", valueElmt.InnerText)
                                    If Not Instance.SelectSingleNode(modifiedXpath, nsMgr) Is Nothing Then
                                        If oBindElmt.GetAttribute("value") = Instance.SelectSingleNode(modifiedXpath, nsMgr).InnerText Then
                                            sXpath = modifiedXpath
                                            sValue = valueElmt.InnerText
                                            sDataType = "SelectedValueDropdown"
                                        End If
                                    End If

                                Next
                                If Not sDataType = "SelectedValueDropdown" Then
                                    sXpath = "."
                                End If
                            End If

                            cProcessInfo = "Error Binding to Node at:" & sXpath

                        End If


                    Catch ex2 As Exception
                        returnException(mcModuleName, "addValues", ex2, "", cProcessInfo, gbDebug)
                    End Try

                End If
                If sXpath <> "" Then
                    Try
                        If Instance.SelectSingleNode(sXpath, nsMgr) Is Nothing Then

                        End If
                    Catch ex As Exception
                        cProcessInfo = sXpath & cleanXpath
                    End Try

                    If Instance.SelectSingleNode(sXpath, nsMgr) Is Nothing Then
                        sValue = "invalid path"
                    Else

                        Select Case sDataType
                            Case "xml-replace"
                                sValue = Instance.SelectSingleNode(sXpath, nsMgr).OuterXml
                                bIsXml = True
                            Case "SelectedValueDropdown"
                                'do nothing we allready have our value
                                sValue = sValue
                            Case Else
                                If bIsXml Then
                                    If bReadOnly Then
                                        sValue = Instance.SelectSingleNode(sXpath, nsMgr).InnerXml
                                    Else
                                        sValue = xmlToForm(Instance.SelectSingleNode(sXpath, nsMgr).InnerXml)
                                    End If
                                Else
                                    If sAttribute <> "" Then
                                        oinstanceElmt = Instance.SelectSingleNode(sXpath, nsMgr)
                                        sValue = oinstanceElmt.GetAttribute(sAttribute)
                                    Else
                                        If bIsXml Then
                                            sValue = xmlToForm(Instance.SelectSingleNode(sXpath, nsMgr).InnerXml)
                                        Else

                                            sValue = Instance.SelectSingleNode(sXpath, nsMgr).InnerText

                                            'NB 8th October 2009
                                            'New type - Unique Identifiers (currently for repeating elements in Polls)
                                            'If no value is present, then assign a unique identifier
                                            If oElmt.GetAttribute("class").ToString.ToLower.Contains("generateuniqueid") And sValue = "" Then

                                                    Dim Now As DateTime = DateTime.Now

                                                    sValue = "uID_" & goSession("pgid") & "_" & Now.Year.ToString & Now.DayOfYear.ToString & Now.Hour.ToString & Now.Minute.ToString & Now.Second.ToString & Now.Millisecond.ToString

                                                End If
                                                'If Not moXformElmt.OwnerDocument.SelectSingleNode("Page/Request/Form/Item[@name='" & oElmt.GetAttribute("bind") & "']") Is Nothing Then
                                                'Dim tempNode As XmlElement = moXformElmt.OwnerDocument.SelectSingleNode("Page/Request/Form/Item[@name='" & oElmt.GetAttribute("bind") & "']")
                                                'sValue = tempNode.InnerText
                                                'End If



                                            End If
                                        End If
                                End If

                        End Select

                        Dim sValueXpath As String
                        sValueXpath = "value"

                        ' If populate Then
                        If oElmt.SelectSingleNode("value") Is Nothing Then
                            oValElmt = moPageXML.CreateElement("value")
                            If bIsXml Then oValElmt.InnerXml = sValue Else oValElmt.InnerText = sValue
                            oElmt.AppendChild(oValElmt)
                        Else
                            oValElmt = oElmt.SelectSingleNode("value")
                            If bIsXml Then oValElmt.InnerXml = sValue Else oValElmt.InnerText = sValue
                        End If
                        'Else
                        '    cProcessInfo = "no value set"

                        'End If

                        'NB: Added to clear the uniqueID value that seemed to spread to nearby nodes that had no value. Not sure why this wasn't clearing here originally
                        sValue = ""
                    End If
                End If
            Next
        Catch ex As Exception
            returnException(mcModuleName, "addValues", ex, "", cProcessInfo, gbDebug)

        End Try

    End Sub

    ''' <summary>
    ''' This has been superceded by Web.ProcessPageXMLForLanguage, but is left in for now for possibly making xforms a container class. 
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub ProcessLanguageLabels_Superceded()
        Try
            If Not String.IsNullOrEmpty(Me.cLanguage) Then
                Dim nsMgr As XmlNamespaceManager = New XmlNamespaceManager(RootGroup.OwnerDocument.NameTable)
                nsMgr.AddNamespace("xml", "http://www.w3.org/XML/1998/namespace")
                ' Find all the labels which do not match the language
                For Each node As XmlElement In RootGroup.SelectNodes("//label[@xml:lang and not(@xml:lang='" & Me.cLanguage & "')]", nsMgr)
                    node.ParentNode.RemoveChild(node)
                Next
            End If
        Catch ex As Exception
            returnException(mcModuleName, "ProcessLanguageLabels", ex, "", "", gbDebug)
        End Try
    End Sub

    Sub resetXFormUI()
        'This does the opposite of addValues() - it looks for all the value nodes that are not in the instance, and removes them
        ' Useful if you are wnating to run through an existing xForm object a second time after already calling addValues().

        Dim oNodes As XmlNodeList
        Dim oNode As XmlNode
        Dim oOldNode As XmlNode
        Dim cProcessInfo As String = ""
        Try

            ' Note : avoiding item stops us from removing dropdown values!
            oNodes = moXformElmt.SelectNodes("//value[not(ancestor::instance) and not(ancestor::item)]")

            For Each oNode In oNodes
                oOldNode = oNode.ParentNode.RemoveChild(oNode)
            Next
        Catch ex As Exception
            returnException(mcModuleName, "resetXFormUI", ex, "", cProcessInfo, gbDebug)
        End Try

    End Sub



    Function addGroup(ByRef oContextNode As XmlElement, ByVal sRef As String, Optional ByVal sClass As String = "", Optional ByVal sLabel As String = "", Optional ByVal oInsertBeforeNode As XmlElement = Nothing) As XmlElement
        Dim oGrpElmt As XmlElement
        Dim oLabelElmt As XmlElement
        Dim cProcessInfo As String = ""
        Try
            oGrpElmt = moPageXML.CreateElement("group")
            oGrpElmt.SetAttribute("ref", sRef)
            If sClass <> "" Then
                oGrpElmt.SetAttribute("class", sClass)
            End If
            If sLabel <> "" Then
                oLabelElmt = moPageXML.CreateElement("label")
                oLabelElmt.InnerXml = sLabel
                oGrpElmt.AppendChild(oLabelElmt)
            End If

            If oInsertBeforeNode Is Nothing Then
                oContextNode.AppendChild(oGrpElmt)
            Else
                oContextNode.InsertBefore(oGrpElmt, oInsertBeforeNode)
            End If

            Return oGrpElmt

        Catch ex As Exception
            returnException(mcModuleName, "addGroup", ex, "", cProcessInfo, gbDebug)
            Return Nothing
        End Try
    End Function



    Function addSwitch(ByRef oContextNode As XmlElement, ByVal sRef As String, Optional ByVal sClass As String = "", Optional ByVal sLabel As String = "", Optional ByRef oInsertBeforeNode As XmlElement = Nothing) As XmlElement
        Dim oGrpElmt As XmlElement
        Dim oLabelElmt As XmlElement
        Dim cProcessInfo As String = ""
        Try
            oGrpElmt = moPageXML.CreateElement("switch")
            oGrpElmt.SetAttribute("ref", sRef)
            If sClass <> "" Then
                oGrpElmt.SetAttribute("class", "disable " & sClass)
            Else
                oGrpElmt.SetAttribute("class", "disable")
            End If
            If sLabel <> "" Then
                oLabelElmt = moPageXML.CreateElement("label")
                oLabelElmt.InnerXml = sLabel
                oGrpElmt.AppendChild(oLabelElmt)
            End If

            If oInsertBeforeNode Is Nothing Then
                oContextNode.AppendChild(oGrpElmt)
            Else
                oContextNode.InsertBefore(oGrpElmt, oInsertBeforeNode)
            End If

            Return oGrpElmt

        Catch ex As Exception
            returnException(mcModuleName, "addGroup", ex, "", cProcessInfo, gbDebug)
            Return Nothing
        End Try
    End Function

    Function addCase(ByRef oContextNode As XmlElement, ByVal sId As String) As XmlElement
        Dim oGrpElmt As XmlElement
        Dim cProcessInfo As String = ""
        Try
            oGrpElmt = moPageXML.CreateElement("case")
            oGrpElmt.SetAttribute("id", sId)

            oContextNode.AppendChild(oGrpElmt)

            Return oGrpElmt

        Catch ex As Exception
            returnException(mcModuleName, "addGroup", ex, "", cProcessInfo, gbDebug)
            Return Nothing
        End Try
    End Function

    Function addRepeat(ByRef oContextNode As XmlElement, ByVal sRef As String, Optional ByVal sClass As String = "", Optional ByVal sLabel As String = "") As XmlElement
        Dim oGrpElmt As XmlElement
        Dim oLabelElmt As XmlElement
        Dim cProcessInfo As String = ""
        Try
            oGrpElmt = moPageXML.CreateElement("repeat")
            oGrpElmt.SetAttribute("ref", sRef)
            If sClass <> "" Then
                oGrpElmt.SetAttribute("class", sClass)
            End If
            If sLabel <> "" Then
                oLabelElmt = moPageXML.CreateElement("label")
                oLabelElmt.InnerText = sLabel
                oGrpElmt.AppendChild(oLabelElmt)
            End If

            oContextNode.AppendChild(oGrpElmt)

            Return oGrpElmt

        Catch ex As Exception
            returnException(mcModuleName, "addRepeat", ex, "", cProcessInfo, gbDebug)
            Return Nothing
        End Try
    End Function

    Function addInput(ByRef oContextNode As XmlElement, ByVal sRef As String, ByVal bBound As Boolean, ByVal sLabel As String) As XmlElement

        Dim cProcessInfo As String = ""
        Try

            Return addInput(oContextNode, sRef, bBound, sLabel, "")
        Catch ex As Exception
            returnException(mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug)
            Return Nothing
        End Try
    End Function


    Function addInput(ByRef oContextNode As XmlElement, ByVal sRef As String, ByVal bBound As Boolean, ByVal sLabel As String, ByVal sClass As String) As XmlElement
        Dim oIptElmt As XmlElement
        Dim oLabelElmt As XmlElement
        Dim cProcessInfo As String = ""
        Try
            oIptElmt = moPageXML.CreateElement("input")
            If bBound Then
                oIptElmt.SetAttribute("bind", sRef)
            Else
                oIptElmt.SetAttribute("ref", sRef)
            End If
            If sClass <> "" Then
                oIptElmt.SetAttribute("class", sClass)
            End If
            If sLabel <> "" Then
                oLabelElmt = moPageXML.CreateElement("label")
                oLabelElmt.InnerText = sLabel
                oIptElmt.AppendChild(oLabelElmt)
            End If

            oContextNode.AppendChild(oIptElmt)
            addInput = oIptElmt
        Catch ex As Exception
            returnException(mcModuleName, "addInput", ex, "", cProcessInfo, gbDebug)
            Return Nothing
        End Try
    End Function


    Function addBind(ByVal sId As String, ByVal sXpath As String, Optional ByVal sRequired As String = "false()", Optional ByVal sType As String = "string", Optional ByRef oBindParent As XmlElement = Nothing, Optional ByVal sConstraint As String = "") As XmlElement

        Dim oBindElmt As XmlElement

        Dim cProcessInfo As String = ""
        Try
            'Optional code can be added here to nest bind elements.
            If oBindParent Is Nothing Then oBindParent = model

            'Or just add a bind element straight in
            oBindElmt = moPageXML.CreateElement("bind")
            If Not sId = "" Then
                oBindElmt.SetAttribute("id", sId)
            End If
            oBindElmt.SetAttribute("nodeset", sXpath)
            If Not sId = "" Then
                oBindElmt.SetAttribute("required", sRequired)
                oBindElmt.SetAttribute("type", sType)
                If Not (String.IsNullOrEmpty(sConstraint)) Then oBindElmt.SetAttribute("constraint", sConstraint)
            End If
            oBindParent.AppendChild(oBindElmt)

            Return oBindElmt

        Catch ex As Exception
            returnException(mcModuleName, "addBind", ex, "", cProcessInfo, gbDebug)
            Return Nothing
        End Try

    End Function

    Function addSecret(ByRef oContextNode As XmlElement, ByVal sRef As String, ByVal bBound As Boolean, ByVal sLabel As String, Optional ByRef sClass As String = "") As XmlElement
        Dim oIptElmt As XmlElement
        Dim oLabelElmt As XmlElement
        Dim cProcessInfo As String = ""
        Try
            oIptElmt = moPageXML.CreateElement("secret")
            If bBound Then
                oIptElmt.SetAttribute("bind", sRef)
            Else
                oIptElmt.SetAttribute("ref", sRef)
            End If
            If sClass <> "" Then
                oIptElmt.SetAttribute("class", sClass)
            End If
            If sLabel <> "" Then
                oLabelElmt = moPageXML.CreateElement("label")
                oLabelElmt.InnerText = sLabel
                oIptElmt.AppendChild(oLabelElmt)
            End If

            oContextNode.AppendChild(oIptElmt)

            addSecret = oIptElmt
        Catch ex As Exception
            returnException(mcModuleName, "addSecret", ex, "", cProcessInfo, gbDebug)
            Return Nothing
        End Try
    End Function

    Function addTextArea(ByRef oContextNode As XmlElement, ByVal sRef As String, ByVal bBound As Boolean, ByVal sLabel As String, Optional ByRef sClass As String = "", Optional ByRef nRows As Integer = 0, Optional ByRef nCols As Integer = 0) As XmlElement
        Dim oIptElmt As XmlElement
        Dim oLabelElmt As XmlElement
        Dim cProcessInfo As String = ""
        Try
            oIptElmt = moPageXML.CreateElement("textarea")
            If bBound Then
                oIptElmt.SetAttribute("bind", sRef)
            Else
                oIptElmt.SetAttribute("ref", sRef)
            End If
            If sClass <> "" Then
                oIptElmt.SetAttribute("class", sClass)
            End If
            If nRows > 0 Then
                oIptElmt.SetAttribute("rows", CStr(nRows))
            End If
            If nCols > 0 Then
                oIptElmt.SetAttribute("cols", CStr(nCols))
            End If
            If sLabel <> "" Then
                oLabelElmt = moPageXML.CreateElement("label")
                oLabelElmt.InnerText = sLabel
                oIptElmt.AppendChild(oLabelElmt)
            End If

            oContextNode.AppendChild(oIptElmt)

            addTextArea = oIptElmt
        Catch ex As Exception
            returnException(mcModuleName, "addTextArea", ex, "", cProcessInfo, gbDebug)
            Return Nothing
        End Try
    End Function



    Function addRange(ByRef oContextNode As XmlElement, ByVal sRef As String, ByVal bBound As Boolean, ByVal sLabel As String, ByVal oStart As Object, ByVal oEnd As Object, Optional ByVal oStep As Object = "", Optional ByVal sClass As String = "") As XmlElement
        Dim oIptElmt As XmlElement
        Dim oLabelElmt As XmlElement
        Dim cProcessInfo As String = ""
        Try
            oIptElmt = moPageXML.CreateElement("range")
            If bBound Then
                oIptElmt.SetAttribute("bind", sRef)
            Else
                oIptElmt.SetAttribute("ref", sRef)
            End If
            If sClass <> "" Then
                oIptElmt.SetAttribute("class", sClass)
            End If
            If IsDate(oStart) Then
                oIptElmt.SetAttribute("start", Protean.Tools.Xml.XmlDate(oStart))
            Else
                oIptElmt.SetAttribute("start", CStr(oStart))
            End If
            If IsDate(oEnd) Then
                oIptElmt.SetAttribute("end", Protean.Tools.Xml.XmlDate(oEnd))
            Else
                oIptElmt.SetAttribute("end", CStr(oEnd))
            End If
            oIptElmt.SetAttribute("step", CStr(oStep))
            If oStep = "" Then
                oIptElmt.SetAttribute("step", CStr(oStep))
            End If
            If sLabel <> "" Then
                oLabelElmt = moPageXML.CreateElement("label")
                oLabelElmt.InnerText = sLabel
                oIptElmt.AppendChild(oLabelElmt)
            End If

            oContextNode.AppendChild(oIptElmt)

            addRange = oIptElmt
        Catch ex As Exception
            returnException(mcModuleName, "addRange", ex, "", cProcessInfo, gbDebug)
            Return Nothing
        End Try
    End Function

    Function addUpload(ByRef oContextNode As XmlElement, ByVal sRef As String, ByVal bBound As Boolean, ByVal sMediaType As String, ByVal sLabel As String, Optional ByRef sClass As String = "") As XmlElement
        Dim oIptElmt As XmlElement
        Dim oLabelElmt As XmlElement
        Dim cProcessInfo As String = ""
        Try
            oIptElmt = moPageXML.CreateElement("upload")
            If bBound Then
                oIptElmt.SetAttribute("bind", sRef)
            Else
                oIptElmt.SetAttribute("ref", sRef)
            End If
            oIptElmt.SetAttribute("mediatype", sMediaType)
            If sClass <> "" Then
                oIptElmt.SetAttribute("class", sClass)
            End If
            If sLabel <> "" Then
                oLabelElmt = moPageXML.CreateElement("label")
                oLabelElmt.InnerText = sLabel
                oIptElmt.AppendChild(oLabelElmt)
            End If
            oLabelElmt = moPageXML.CreateElement("filename")
            oLabelElmt.SetAttribute("ref", "@filename")
            oIptElmt.AppendChild(oLabelElmt)

            oLabelElmt = moPageXML.CreateElement("mediatype")
            oLabelElmt.SetAttribute("ref", "@mediatype")
            oIptElmt.AppendChild(oLabelElmt)

            oContextNode.AppendChild(oIptElmt)

            addUpload = oIptElmt
        Catch ex As Exception
            returnException(mcModuleName, "addUpload", ex, "", cProcessInfo, gbDebug)
            Return Nothing
        End Try
    End Function

    Function addSelect(ByRef oContextNode As XmlElement, ByVal sRef As String, ByVal bBound As Boolean, ByVal sLabel As String, Optional ByVal sClass As String = "", Optional ByVal nAppearance As ApperanceTypes = ApperanceTypes.Minimal) As XmlElement
        Dim oIptElmt As XmlElement
        Dim oLabelElmt As XmlElement
        Dim cProcessInfo As String = ""
        Try
            oIptElmt = moPageXML.CreateElement("select")
            If bBound Then
                oIptElmt.SetAttribute("bind", sRef)
            Else
                oIptElmt.SetAttribute("ref", sRef)
            End If
            If sClass <> "" Then
                oIptElmt.SetAttribute("class", sClass)
            End If
            Select Case (nAppearance)
                Case ApperanceTypes.Full
                    oIptElmt.SetAttribute("appearance", "full")
                Case ApperanceTypes.Minimal
                    oIptElmt.SetAttribute("appearance", "minimal")
            End Select

            If sLabel <> "" Then
                oLabelElmt = moPageXML.CreateElement("label")
                oLabelElmt.InnerXml = sLabel
                oIptElmt.AppendChild(oLabelElmt)
            End If

            ' Removed choices as per request from Trevor.
            ' oLabelElmt = moPageXML.CreateElement("choices")
            ' oIptElmt.AppendChild(oLabelElmt)

            oContextNode.AppendChild(oIptElmt)

            addSelect = oIptElmt
        Catch ex As Exception
            returnException(mcModuleName, "addSelect", ex, "", cProcessInfo, gbDebug)
            Return Nothing
        End Try
    End Function

    Function addSelect1(ByRef oContextNode As XmlElement, ByVal sRef As String, ByVal bBound As Boolean, ByVal sLabel As String, Optional ByVal sClass As String = "", Optional ByVal nAppearance As ApperanceTypes = ApperanceTypes.Minimal) As XmlElement
        Dim oIptElmt As XmlElement
        Dim oLabelElmt As XmlElement
        Dim cProcessInfo As String = ""
        Try
            oIptElmt = moPageXML.CreateElement("select1")
            If bBound Then
                oIptElmt.SetAttribute("bind", sRef)
            Else
                oIptElmt.SetAttribute("ref", sRef)
            End If
            If sClass <> "" Then
                oIptElmt.SetAttribute("class", sClass)
            End If
            Select Case (nAppearance)
                Case ApperanceTypes.Full
                    oIptElmt.SetAttribute("appearance", "full")
                Case ApperanceTypes.Minimal
                    oIptElmt.SetAttribute("appearance", "minimal")
            End Select
            If sLabel <> "" Then
                oLabelElmt = moPageXML.CreateElement("label")
                oLabelElmt.InnerXml = sLabel
                oIptElmt.AppendChild(oLabelElmt)
            End If

            'oLabelElmt = moPageXML.CreateElement("choices")
            'oIptElmt.AppendChild(oLabelElmt)

            oContextNode.AppendChild(oIptElmt)

            addSelect1 = oIptElmt

        Catch ex As Exception
            returnException(mcModuleName, "addSelect1", ex, "", cProcessInfo, gbDebug)
            Return Nothing
        End Try
    End Function

    Sub addValue(ByRef oInputNode As XmlElement, ByVal sValue As String)

        Dim oValueElmt As XmlElement
        Dim cProcessInfo As String = ""
        Try
            oValueElmt = moPageXML.CreateElement("value")
            oValueElmt.InnerText = sValue

            oInputNode.AppendChild(oValueElmt)


        Catch ex As Exception
            returnException(mcModuleName, "addValue", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub


    Function addChoices(ByRef oSelectNode As XmlElement, ByVal sLabel As String) As XmlElement
        Dim oOptElmt As XmlElement
        Dim oLabelElmt As XmlElement
        Dim cProcessInfo As String = ""
        Try
            oOptElmt = moPageXML.CreateElement("choices")
            If sLabel <> "" Then
                oLabelElmt = moPageXML.CreateElement("label")
                oLabelElmt.InnerText = sLabel
                oOptElmt.AppendChild(oLabelElmt)
            End If

            oSelectNode.AppendChild(oOptElmt)

            Return oOptElmt

        Catch ex As Exception
            returnException(mcModuleName, "addOption", ex, "", cProcessInfo, gbDebug)
            Return Nothing
        End Try
    End Function

    Function addOption(ByRef oSelectNode As XmlElement, ByVal sLabel As String, ByVal sValue As String, Optional ByVal bXmlLabel As Boolean = False, Optional ByVal ToggleCase As String = "") As XmlElement
        Dim oOptElmt As XmlElement
        Dim oLabelElmt As XmlElement
        Dim oValueElmt As XmlElement
        Dim cProcessInfo As String = ""
        Try
            oOptElmt = moPageXML.CreateElement("item")
            'If sLabel <> "" Then
            oLabelElmt = moPageXML.CreateElement("label")
            If bXmlLabel Then
                oLabelElmt.InnerXml = sLabel & " "
            Else
                oLabelElmt.InnerText = sLabel & " "
            End If
            oOptElmt.AppendChild(oLabelElmt)
            'End If
            'If sValue <> "" Then
            oValueElmt = moPageXML.CreateElement("value")
            oValueElmt.InnerText = sValue
            oOptElmt.AppendChild(oValueElmt)
            'End If
            If ToggleCase <> "" Then
                Dim oToggleElmt As XmlElement = moPageXML.CreateElement("toggle")
                oToggleElmt.SetAttribute("event", "DOMActivate")
                oToggleElmt.SetAttribute("case", ToggleCase)
                oOptElmt.AppendChild(oToggleElmt)
            End If

            oSelectNode.AppendChild(oOptElmt)

            Return oOptElmt

        Catch ex As Exception
            returnException(mcModuleName, "addOption", ex, "", cProcessInfo, gbDebug)
            Return Nothing

        End Try
    End Function

    Sub addOptionsFromSqlDataReader(ByRef oSelectNode As XmlElement, ByRef oDr As Data.SqlClient.SqlDataReader, Optional ByVal sNameFld As String = "name", Optional ByVal sValueFld As String = "value")

        Dim cProcessInfo As String = ""

        Dim ordinalsChecked As Boolean = False

        Dim nameOrdinal As Integer = 0
        Dim valueOrdinal As Integer = 1
        Try


            ' AG - I'm adding this ordinal check in because previously this relied on the fields being called "name" and "value"
            ' which is really annoying when you go to the trouble of passing only two column.
            ' If name and value do not exist, then assume that the first column is name and the second is value.
            If Tools.Database.HasColumn(oDr, sNameFld) Then
                nameOrdinal = oDr.GetOrdinal(sNameFld)
            End If

            If Tools.Database.HasColumn(oDr, sValueFld) Then
                valueOrdinal = oDr.GetOrdinal(sValueFld)
            End If


            While oDr.Read
                'update audit
                'NB Change! Needs auth :S
                addOption(oSelectNode, Replace(oDr(nameOrdinal).ToString, "&amp;", "&"), oDr(valueOrdinal).ToString)
            End While
            oDr.Close()
            oDr = Nothing

        Catch ex As Exception
            returnException(mcModuleName, "addOptionsFromSqlDataReader", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub

    Sub addUserOptionsFromSqlDataReader(ByRef oSelectNode As XmlElement, ByRef oDr As Data.SqlClient.SqlDataReader, Optional ByVal sNameFld As String = "name", Optional ByVal sValueFld As String = "value")

        Dim cProcessInfo As String = ""
        Dim cName As String
        Try

            While oDr.Read

                'hack for users to add full names
                Dim oXml As XmlDataDocument = New XmlDataDocument
                If oDr.FieldCount > 2 Then
                    oXml.LoadXml(oDr("detail"))
                    If oXml.DocumentElement.Name = "User" Then
                        cName = oXml.SelectSingleNode("User/LastName").InnerText & ", " & oXml.SelectSingleNode("User/FirstName").InnerText
                    Else
                        cName = oDr(sNameFld).ToString
                    End If
                Else
                    cName = oDr(sNameFld).ToString
                End If


                addOption(oSelectNode, cName, oDr(sValueFld).ToString)
            End While
            oDr.Close()
            oDr = Nothing

        Catch ex As Exception
            returnException(mcModuleName, "addOptionsFromRecordSet", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub

    Sub addOptionsFilesFromDirectory(ByRef oSelectNode As XmlElement, ByVal DirectoryPath As String, Optional ByVal Extension As String = "")

        Dim cProcessInfo As String = ""
        Dim fullDirPath As String = DirectoryPath
        Try
            If DirectoryPath.StartsWith("/") Then
                fullDirPath = goServer.MapPath(DirectoryPath)
            End If

            Dim dir As New DirectoryInfo(fullDirPath)
            Dim files As FileInfo() = dir.GetFiles()
            Dim fi As FileInfo


            If Extension <> "" Then
                If Not Extension.StartsWith(".") Then
                    Extension = "." & Extension
                End If
            End If

            For Each fi In files
                If Extension = "" Or Extension = fi.Extension Then
                    addOption(oSelectNode, fi.Name, DirectoryPath & "\" & fi.Name)
                End If
            Next

        Catch ex As Exception
            returnException(mcModuleName, "addOptionsFromRecordSet", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub

    Sub addOptionsFoldersFromDirectory(ByRef oSelectNode As XmlElement, ByVal DirectoryPath As String)

        Dim cProcessInfo As String = ""
        Dim fullDirPath As String = DirectoryPath
        Try
            If DirectoryPath.StartsWith("/") Then
                fullDirPath = goServer.MapPath(DirectoryPath)
            End If


            Dim dir As New DirectoryInfo(fullDirPath)
            Dim folders As DirectoryInfo() = dir.GetDirectories
            Dim fo As DirectoryInfo

            For Each fo In folders
                If Not fo.Name.StartsWith("~") And Not fo.Name.StartsWith("_vti") Then
                    addOption(oSelectNode, DirectoryPath.Replace("\", "/") & "/" & fo.Name, DirectoryPath & "/" & fo.Name)
                    addOptionsFoldersFromDirectory(oSelectNode, DirectoryPath & "\" & fo.Name)
                End If
            Next

        Catch ex As Exception
            returnException(mcModuleName, "addOptionsFromRecordSet", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub

    Public Sub addNote(ByVal sRef As String, ByVal nTypes As noteTypes, ByVal sMessage As String, Optional ByVal bInsertFirst As Boolean = False, Optional ByVal sClass As String = "")

        Dim oIptElmt As XmlElement
        Dim oNoteElmt As XmlElement = Nothing
        Dim cProcessInfo As String = ""
        Try
            If Not moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='" & sRef & "' or @bind='" & sRef & "']") Is Nothing Then
                oIptElmt = moXformElmt.SelectSingleNode("descendant-or-self::*[@ref='" & sRef & "' or @bind='" & sRef & "']")
                Select Case nTypes
                    Case 0
                        oNoteElmt = moPageXML.CreateElement("hint")
                    Case 1
                        oNoteElmt = moPageXML.CreateElement("help")
                    Case 2
                        oNoteElmt = moPageXML.CreateElement("alert")
                End Select

                If sClass <> "" Then
                    oNoteElmt.SetAttribute("class", sClass)
                End If

                oNoteElmt.InnerXml = sMessage & ""
                If bInsertFirst And Not oIptElmt.FirstChild Is Nothing Then
                    oIptElmt.InsertBefore(oNoteElmt, oIptElmt.FirstChild)
                Else
                    oIptElmt.AppendChild(oNoteElmt)
                End If

            End If
        Catch ex As Exception
            cProcessInfo = sRef & " - " & nTypes & " - " & sMessage
            returnException(mcModuleName, "addNote - by ref", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub

    Public Sub addNote(ByRef oNode As XmlNode, ByVal nTypes As noteTypes, ByVal sMessage As String, Optional ByVal bInsertFirst As Boolean = False, Optional sClass As String = "")
        If sMessage Is Nothing Then sMessage = ""
        Dim oNoteElmt As XmlElement = Nothing
        Dim cProcessInfo As String = sMessage
        Try

            Select Case nTypes
                Case 0
                    oNoteElmt = moPageXML.CreateElement("hint")
                Case 1
                    oNoteElmt = moPageXML.CreateElement("help")
                Case 2
                    oNoteElmt = moPageXML.CreateElement("alert")
            End Select

            Tools.Xml.SetInnerXmlThenInnerText(oNoteElmt, sMessage)

            If sClass <> "" Then
                oNoteElmt.SetAttribute("class", sClass)
            End If


            If Not oNode Is Nothing Then
                If bInsertFirst And Not oNode.FirstChild Is Nothing Then
                    oNode.InsertBefore(oNoteElmt, oNode.FirstChild)
                Else
                    oNode.AppendChild(oNoteElmt)
                End If
            End If


        Catch ex As Exception
            returnException(mcModuleName, "addNote - by node", ex, "", cProcessInfo, gbDebug)
        End Try
    End Sub

    Function addDiv(ByRef oContextNode As XmlElement, ByVal sXhtml As String, Optional ByVal sClass As String = "principle", Optional ByVal addAsXml As Boolean = True) As Object

        Dim oIptElmt As XmlElement
        Dim cProcessInfo As String = ""
        Try
            oIptElmt = moPageXML.CreateElement("div")
            oIptElmt.SetAttribute("class", sClass)

            If addAsXml Then
                oIptElmt.InnerXml = sXhtml
            Else
                oIptElmt.InnerText = sXhtml
            End If

            oContextNode.AppendChild(oIptElmt)

            addDiv = oIptElmt
        Catch ex As Exception
            returnException(mcModuleName, "addDiv", ex, "", cProcessInfo, gbDebug)
            Return Nothing
        End Try
    End Function




    Function addSubmit(ByRef oContextNode As XmlElement, ByVal sSubmission As String, ByVal sLabel As String, Optional ByVal sRef As String = "submit", Optional ByVal sClass As String = "principle", Optional ByVal sIcon As String = "", Optional ByVal sValue As String = "") As Object

        Dim oIptElmt As XmlElement
        Dim oLabelElmt As XmlElement
        Dim cProcessInfo As String = ""
        Try
            oIptElmt = oContextNode.OwnerDocument.CreateElement("submit")
            oIptElmt.SetAttribute("submission", sSubmission)
            oIptElmt.SetAttribute("ref", sRef)
            oIptElmt.SetAttribute("class", sClass)
            If sIcon <> "" Then
                oIptElmt.SetAttribute("icon", sIcon)
            End If
            If sValue <> "" Then
                oIptElmt.SetAttribute("value", sValue)
            End If

            If sLabel <> "" Then
                oLabelElmt = oContextNode.OwnerDocument.CreateElement("label")
                oLabelElmt.InnerText = sLabel
                oIptElmt.AppendChild(oLabelElmt)
            End If

            oContextNode.AppendChild(oIptElmt)

            addSubmit = oIptElmt
        Catch ex As Exception
            returnException(mcModuleName, "addSubmit", ex, "", cProcessInfo, gbDebug)
            Return Nothing
        End Try
    End Function

    Overridable Function submit() As Boolean
        Dim cProcessInfo As String = ""
        Dim oNode As XmlNode
        Dim oElmt As XmlElement
        Dim oSubElmt As XmlElement
        Dim sServiceUrl As String
        Dim sSoapAction As String
        Dim sActionName As String
        Dim SoapRequestXml As String
        Dim soapBody As String
        Dim sResponse As String
        Dim oSoapElmt As XmlElement
        Dim cNsURI As String

        Try
            updateInstanceFromRequest()
            validate()

            If valid Then
                Dim soapClnt As SoapClient = New SoapClient


                For Each oNode In moXformElmt.SelectNodes("descendant-or-self::submit")
                    'ok get the ref or the bind name of the button
                    oElmt = oNode
                    If goRequest(oElmt.GetAttribute("submission")) <> "" Then

                        For Each oSubElmt In model.SelectNodes("submission")

                            sServiceUrl = oSubElmt.GetAttribute("action")
                            sSoapAction = oSubElmt.GetAttribute("SOAPAction")
                            sActionName = sSoapAction.Substring(sSoapAction.LastIndexOf("/") + 1, sSoapAction.Length - sSoapAction.LastIndexOf("/") - 1)

                            soapClnt.ServiceUrl = sServiceUrl
                            If Instance.SelectNodes("*").Count > 1 Then
                                Dim newElmt As XmlElement
                                For Each newElmt In Instance.SelectNodes("*")
                                    If newElmt.GetAttribute("id") = oSubElmt.GetAttribute("id") Then
                                        SoapRequestXml = newElmt.OuterXml
                                    End If
                                Next
                                ' do this way to skip the query namespace issue
                                ' SoapRequestXml = Instance.SelectSingleNode(sActionName & "[@id='" & oSubElmt.GetAttribute("id") & "']").OuterXml
                            Else
                                SoapRequestXml = Instance.SelectSingleNode("*").OuterXml
                            End If

                            soapBody = "<?xml version=""1.0"" encoding=""utf-8""?>" & soapClnt.getSoapEnvelope(SoapRequestXml).OuterXml

                            soapClnt.SendSoapRequest(sActionName, soapBody)

                            'replace the instance with the response

                            Dim nsmgr As XmlNamespaceManager = New XmlNamespaceManager(moPageXML.NameTable)
                            nsmgr.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/")

                            oSoapElmt = soapClnt.results.SelectSingleNode("soap:Envelope/soap:Body", nsmgr).FirstChild

                            Try
                                cNsURI = oSoapElmt.GetAttribute("xmlns")
                                nsmgr.AddNamespace("ews", cNsURI)
                                sResponse = oSoapElmt.SelectSingleNode("ews:" & sActionName & "Result", nsmgr).InnerText
                            Catch ex As Exception
                                sResponse = oSoapElmt.SelectSingleNode(sActionName & "Result", nsmgr).InnerText
                            End Try

                            ' Try to add the response
                            Try

                                addNote(moXformElmt, noteTypes.Alert, sResponse, True, "alert-success")

                            Catch ex As System.Xml.XmlException
                                Try
                                    sResponse = Tools.Xml.encodeAllHTML(sResponse)
                                    addNote(moXformElmt, noteTypes.Alert, sResponse)
                                Catch ex2 As Exception
                                    addNote(moXformElmt, noteTypes.Alert, "Response could not be added due to an Xml Error")
                                End Try
                            Catch ex As Exception
                                addNote(moXformElmt, noteTypes.Alert, "Response could not be added.")
                            End Try
                        Next

                    End If
                Next

            End If

        Catch ex As Exception
            returnException(mcModuleName, "submit", ex, "", cProcessInfo, gbDebug)
        End Try

    End Function

    Overridable Function isSubmitted() As Boolean
        'Steps through all of the submit buttons to see if they have been pressed
        Dim oNode As XmlNode
        Dim oElmt As XmlElement

        Dim cProcessInfo As String = ""
        Try
            isSubmitted = False
            If moXformElmt Is Nothing Then
                cProcessInfo = "xFormElement not set"
            Else
                For Each oNode In moXformElmt.SelectNodes("descendant-or-self::submit[not(ancestor::instance)]")
                    'ok get the ref or the bind name of the button
                    oElmt = oNode
                    If oElmt.GetAttribute("submission") <> "" And goRequest.Form(oElmt.GetAttribute("submission")) <> "" Then
                        isSubmitted = True
                    ElseIf goRequest(oElmt.GetAttribute("ref")) <> "" Then
                        isSubmitted = True
                    ElseIf goRequest(oElmt.GetAttribute("bind")) <> "" Then
                        isSubmitted = True
                    ElseIf goRequest("ewSubmitClone_" & oElmt.GetAttribute("ref")) <> "" Then
                        isSubmitted = True
                    End If
                Next
            End If


        Catch ex As Exception
            returnException(mcModuleName, "getSubmitted", ex, "", cProcessInfo, gbDebug)
        End Try

    End Function

    Overridable Function isDeleted() As Boolean
        'Steps through all of the submit buttons to see if they have been pressed
        Dim oNode As XmlNode
        Dim oElmt As XmlElement
        Dim cProcessInfo As String = ""
        Try
            'toggles bDeleted if case = true
            If Not bDeleted Then
                For Each oNode In moXformElmt.SelectNodes("descendant-or-self::submit[@ref='delete']")
                    'ok get the ref or the bind name of the button
                    oElmt = oNode
                    If goRequest(oElmt.GetAttribute("ref")) <> "" Then
                        bDeleted = True
                    End If
                Next
            Else
                bDeleted = False
            End If

            isDeleted = bDeleted


        Catch ex As Exception
            returnException(mcModuleName, "getSubmitted", ex, "", cProcessInfo, gbDebug)
        End Try

    End Function

    Function getSubmitted() As String
        'Steps through all of the submit buttons to see if they have been pressed
        Dim oNode As XmlNode
        Dim oElmt As XmlElement
        Dim strReturn As String = ""
        Dim cProcessInfo As String = ""
        Try

            For Each oNode In moXformElmt.SelectNodes("descendant-or-self::submit")
                'ok get the ref or the bind name of the button
                oElmt = oNode
                If goRequest(oElmt.GetAttribute("submission")) <> "" Then
                    strReturn = oElmt.GetAttribute("submission")
                ElseIf goRequest(oElmt.GetAttribute("ref")) <> "" Then
                    strReturn = oElmt.GetAttribute("ref")
                ElseIf goRequest(oElmt.GetAttribute("bind")) <> "" Then
                    strReturn = oElmt.GetAttribute("bind")
                End If
            Next
            Return strReturn
        Catch ex As Exception
            returnException(mcModuleName, "isSubmitted", ex, "", cProcessInfo, gbDebug)
            Return Nothing
        End Try

    End Function

    Private Function getBindXpath(ByRef oBindElmt As XmlElement, Optional ByVal sXpath As String = "") As String

        Dim oBindParent As XmlNode
        Dim oBindParentElmt As XmlElement
        Dim sNodeSet As String

        If sXpath <> "" Then
            sNodeSet = sXpath
        Else
            If InStr(oBindElmt.GetAttribute("nodeset"), "@") = 1 Then
                'ignore for attributes
                sNodeSet = ""
            Else
                sNodeSet = oBindElmt.GetAttribute("nodeset")
            End If
        End If
        If Not oBindElmt.SelectSingleNode("parent::bind") Is Nothing Then
            oBindParent = oBindElmt.SelectSingleNode("parent::bind")
            oBindParentElmt = oBindParent
            If sNodeSet = "" Then
                sNodeSet = getBindXpath(oBindParent, oBindParentElmt.GetAttribute("nodeset"))
            Else
                sNodeSet = getBindXpath(oBindParent, oBindParentElmt.GetAttribute("nodeset") & "/" & sNodeSet)
            End If
        End If

        Return sNodeSet

    End Function

    Public Function getNewRef(ByVal refPrefix As String) As String
        Dim oNodeList As XmlNodeList
        Dim oNode As XmlNode
        Dim oElmt As XmlElement
        Dim nLastNum As Long
        Dim cProcessInfo As String = ""
        Try
            oNodeList = moXformElmt.SelectNodes("descendant-or-self::*[starts-with(@ref,'" & refPrefix & "') or starts-with(@bind,'" & refPrefix & "')]")
            nLastNum = 0
            For Each oNode In oNodeList
                oElmt = oNode
                If oElmt.GetAttribute("ref") = "" Then ' its not a ref its a bind
                    If CLng("0" & Replace(oElmt.GetAttribute("bind"), refPrefix, "")) > nLastNum Then
                        nLastNum = CLng("0" & Replace(oElmt.GetAttribute("bind"), refPrefix, ""))
                    End If
                Else ' it is a ref
                    If CLng("0" & Replace(oElmt.GetAttribute("ref"), refPrefix, "")) > nLastNum Then
                        nLastNum = CLng("0" & Replace(oElmt.GetAttribute("ref"), refPrefix, ""))
                    End If
                End If

            Next
            Return refPrefix & CStr(nLastNum + 1)

        Catch ex As Exception
            returnException(mcModuleName, "getNewRef", ex, "", cProcessInfo, gbDebug)
            Return Nothing
        End Try
    End Function


    ''' <summary>
    ''' this deletes the specified nodes in the instance
    ''' </summary>
    ''' <remarks></remarks>
    Private Function checkForDeleteCommand(ByRef xmlForm As XmlElement) As Boolean

        Dim cProcessInfo As String = ""
        Dim bDeletionDone As Boolean = False

        Try

            'check for delete command
            Dim cRequestForm As String = goRequest.Form.ToString
            Dim rxDelete As New Regex("delete%3a(.+?)_(\d+)=Del")

            If rxDelete.IsMatch(cRequestForm) Then

                'get the index of the options group and node from the delete command
                Dim cControlName As String = rxDelete.Match(cRequestForm).Groups(1).ToString
                Dim nNodeIndex As Integer = rxDelete.Match(cRequestForm).Groups(2).ToString
                Dim cXpathOptions As String
                Dim bindNode As XmlElement

                For Each bindNode In xmlForm.SelectNodes("descendant-or-self::bind[@id='" & cControlName & "']")
                    If Not bindNode Is Nothing Then

                        cXpathOptions = Me.getBindXpath(bindNode)
                        Dim nsMgr As XmlNamespaceManager = Protean.Tools.Xml.getNsMgrRecursive(oInstance.SelectSingleNode("*[1]"), moPageXML)
                        cXpathOptions = addNsToXpath(cXpathOptions, nsMgr)

                        Dim nodeToDelete As XmlElement = oInstance.SelectSingleNode(cXpathOptions & "[position()=" & nNodeIndex + 1 & "]", nsMgr)
                        If Not nodeToDelete Is Nothing Then
                            nodeToDelete.SetAttribute("deletefromXformInstance", "true")
                            bDeletionDone = True
                        End If
                    End If
                Next
            End If

            Return bDeletionDone

        Catch ex As Exception
            returnException(mcModuleName, "checkForDeleteCommand", ex, "", cProcessInfo, gbDebug)
        End Try
    End Function


    Private Sub processRepeats(ByRef xFormElmt As XmlElement)
        Try
            Dim oRptElmt As XmlElement
            Dim oRptElmtCopy As XmlElement
            Dim oRptElmtCopySub As XmlElement
            Dim oBindNode As XmlElement
            Dim sBindXpath As String
            Dim oInstanceNodeSet As XmlNodeList
            Dim oNode As XmlElement
            Dim oBindCopy As XmlElement
            Dim nNodePosition As Long
            Dim obindElmt As XmlElement
            Dim isInserted As Boolean = False


            If bProcessRepeats Then

                If checkForDeleteCommand(xFormElmt) Then
                    isTriggered = True
                End If

                Dim nsMgr As XmlNamespaceManager = getNsMgr(oInstance.SelectSingleNode("*[1]"), moPageXML)

                For Each oRptElmt In xFormElmt.SelectNodes("descendant-or-self::repeat[not(contains(@class,'relatedContent') or contains(@class,'repeated'))]")
                    isInserted = False
                    If oRptElmt.GetAttribute("bind") <> "" Then

                        'get the bind elements
                        For Each oBindNode In model.SelectNodes("descendant-or-self::bind[not(ancestor::instance) and @id='" & oRptElmt.GetAttribute("bind") & "']")
                            'build the bind xpath
                            sBindXpath = getBindXpath(oBindNode)
                            sBindXpath = addNsToXpath(sBindXpath, nsMgr)
                            'get the nodeset of repeating elements

                            Dim updateSubsquentRequested As Boolean = False

                            If goRequest("insert:" & oRptElmt.GetAttribute("bind")) <> "" Then

                                'get the first node in the instance to copy
                                Dim oInitialNode As XmlElement = oInitialInstance.SelectSingleNode(sBindXpath & "[position() = 1]", nsMgr)
                                Dim oFirstNode As XmlElement = oInstance.SelectSingleNode(sBindXpath & "[position() = 1]", nsMgr)
                                Dim oNewNode As XmlElement = oInitialNode.CloneNode(True)
                                oFirstNode.ParentNode.InsertAfter(oNewNode, oInstance.SelectSingleNode(sBindXpath & "[last()]", nsMgr))
                                isTriggered = True
                                isInserted = True

                            End If

                            oInstanceNodeSet = oInstance.SelectNodes(sBindXpath, nsMgr)
                            nNodePosition = 0
                            Dim nNodeCount As Long = 0
                            If oInstanceNodeSet.Count = 0 Then

                                Me.addNote(Me.moXformElmt, noteTypes.Alert, "The repeat with bind='" & oRptElmt.GetAttribute("bind") & "' could not find the node in the instance on xpath '" & sBindXpath & "'")

                            Else
                                Dim oInstanceNodeSetCount = 0
                                Dim bSkipBinds As Boolean = False
                                For Each oNode In oInstanceNodeSet
                                    oInstanceNodeSetCount = oInstanceNodeSetCount + 1
                                    If oInstanceNodeSet.Count = oInstanceNodeSetCount And isInserted Then
                                        bskipbinds = True
                                    End If
                                    If Not (oNode.GetAttribute("deletefromXformInstance") = "true") Then

                                        'step through and 
                                        'build the repeating binds
                                        oBindCopy = oBindNode.CloneNode(True)
                                        'update the bindCopyId with position
                                        oBindCopy.SetAttribute("id", oBindNode.GetAttribute("id") & "_" & CStr(nNodePosition))
                                        oBindCopy.SetAttribute("nodeset", oBindNode.GetAttribute("nodeset") & "[position() = " & CStr(nNodePosition + 1) & "]")
                                        'update child bind id's
                                        For Each obindElmt In oBindCopy.SelectNodes("bind")
                                            Dim newBindId As String = obindElmt.GetAttribute("id") & "_" & CStr(nNodePosition)
                                            obindElmt.SetAttribute("id", newBindId)

                                            If bSkipBinds Then
                                                BindsToSkip.Add(newBindId)
                                            End If

                                        Next

                                        'test if the current node is one we want to delete, note that nodeCount is current nodes whereas nodePosition is the new numbering
                                        Dim bDelete As Boolean = False

                                        'build the repeating form groups
                                        oRptElmtCopy = oRptElmt.CloneNode(True)
                                        'update the elmtId with position

                                        'On Repeated Form Controls make binds unique
                                        For Each oRptElmtCopySub In oRptElmtCopy.SelectNodes("descendant-or-self::*[@bind!='']")
                                            Dim cBindId As String = oRptElmtCopySub.GetAttribute("bind") & "_" & nNodePosition.ToString
                                            oRptElmtCopySub.SetAttribute("bind", cBindId)
                                        Next

                                        ' make toggle case unique
                                        For Each oRptElmtCopySub In oRptElmtCopy.SelectNodes("descendant-or-self::toggle[@case!='']")
                                            Dim cCase As String = oRptElmtCopySub.GetAttribute("case") & "_" & nNodePosition.ToString
                                            oRptElmtCopySub.SetAttribute("case", cCase)
                                        Next

                                        For Each oRptElmtCopySub In oRptElmtCopy.SelectNodes("descendant-or-self::case[@id!='']")
                                            Dim cCase As String = oRptElmtCopySub.GetAttribute("id") & "_" & nNodePosition.ToString
                                            oRptElmtCopySub.SetAttribute("id", cCase)
                                        Next

                                        'NB Mark Nodes so if processRepeats is called again, they don't get stuck recursing themselves
                                        If Not oRptElmtCopy.GetAttribute("class") Is Nothing Then
                                            oRptElmtCopy.SetAttribute("class", oRptElmtCopy.GetAttribute("class").ToString & " repeated rpt-" & nNodePosition.ToString)
                                        Else
                                            oRptElmtCopy.SetAttribute("class", "repeated rpt-" & nNodePosition.ToString)
                                        End If

                                        'insert the repeated binds
                                        oBindNode.ParentNode.InsertAfter(oBindCopy, oBindNode.ParentNode.LastChild)
                                        'insert the repeated repeat but only once not for each bind
                                        If Not oRptElmt.ParentNode Is Nothing Then
                                            'Adding the repeated controls but only once
                                            oRptElmt.ParentNode.InsertAfter(oRptElmtCopy, oRptElmt.ParentNode.LastChild)

                                            'set oForm to not be readonly
                                            Dim oForm As Collections.Specialized.NameValueCollection = goRequest.Form
                                            oForm = CType(goRequest.GetType().GetField("_form", System.Reflection.BindingFlags.NonPublic Or System.Reflection.BindingFlags.Instance).GetValue(goRequest), Collections.Specialized.NameValueCollection)
                                            Dim readOnlyInfo As System.Reflection.PropertyInfo = oForm.GetType().GetProperty("IsReadOnly", System.Reflection.BindingFlags.NonPublic Or System.Reflection.BindingFlags.Instance)
                                            readOnlyInfo.SetValue(oForm, False, Nothing)

                                            If updateSubsquentRequested Then
                                                'we have removed a repeat element therefore all binds 
                                                'on the request object that come after must be reduced by one 
                                                'so we can call updateinstancefromRequest and values are stored in the correct location.
                                                'For Each oRptElmtCopySub In oRptElmtCopy.SelectNodes("descendant-or-self::*")
                                                For Each oRptElmtCopySub In oRptElmtCopy.SelectNodes("descendant::*[@bind!='' and name()!='delete']")
                                                    Dim cBindStart As String = oRptElmtCopySub.GetAttribute("bind").Split("_")(0)
                                                    Dim cOldBindId As String = cBindStart & "_" & nNodePosition.ToString + 1
                                                    Dim cNewBindId As String = cBindStart & "_" & nNodePosition.ToString

                                                    oForm.Set(cNewBindId, oForm(cOldBindId))
                                                Next
                                            End If
                                            'Set oForm back to readonly
                                            readOnlyInfo.SetValue(oForm, True, Nothing)

                                        End If

                                        'increment our keys
                                        nNodePosition = nNodePosition + 1
                                        nNodeCount = nNodeCount + 1

                                    Else
                                        updateSubsquentRequested = True
                                    End If
                                Next

                                updateSubsquentRequested = False

                                'delete items from the instance that are set for deletion.
                                For Each oNode In oInstanceNodeSet
                                    If oNode.GetAttribute("deletefromXformInstance") = "true" Then
                                        oNode.ParentNode.RemoveChild(oNode)
                                    End If
                                Next

                                'delete the origional bind and repeat controls.
                                oBindNode.ParentNode.RemoveChild(oBindNode)
                                If Not oRptElmt.ParentNode Is Nothing Then
                                    oRptElmt.ParentNode.RemoveChild(oRptElmt)
                                End If

                            End If
                        Next
                    End If
                Next
            End If

        Catch ex As Exception
            returnException(mcModuleName, "processRepeats", ex, "", , gbDebug)
        End Try
    End Sub

    Private Sub processFormParameters()
        Dim processInfo As String = ""
        Dim formXml As String = ""
        Try

            If _formParameters IsNot Nothing AndAlso _formParameters.Length > 0 Then
                formXml = String.Format(moXformElmt.InnerXml, _formParameters)
                If formXml <> moXformElmt.InnerXml Then
                    ' Something has changed so let's update the references.
                    processInfo = "Parameters loaded, updating form xml"
                    moXformElmt.InnerXml = formXml
                    model = moXformElmt.SelectSingleNode("descendant-or-self::model")
                    If Not model Is Nothing Then
                        oInstance = model.SelectSingleNode("descendant-or-self::instance")
                    Else
                        Instance = Nothing
                        oInstance = Nothing
                    End If
                End If

            End If

        Catch ex As Exception
            returnException(mcModuleName, "processFormParameters", ex, "", processInfo, gbDebug)
        End Try
    End Sub

    Private Sub PreLoadInstance()
        Try
            If Not String.IsNullOrEmpty(_preLoadedInstanceXml) Then
                ' Instance has been preloaded
                oInstance.InnerXml = _preLoadedInstanceXml
            End If
        Catch ex As Exception

        End Try
    End Sub

    'Public Overridable Sub CombineInstance(ByRef oMasterInstance As XmlElement, ByRef oExistingInstance As XmlElement)
    '    'combines an existing instance with a new 
    '    Try
    '        'firstly we will go through and find like for like
    '        'we will skip the immediate node below instance as this is usually fairly static
    '        'except for attributes

    '        'Attributes of Root
    '        For i As Integer = 0 To oMasterInstance.FirstChild.Attributes.Count - 1
    '            Dim oMainAtt As XmlAttribute = oMasterInstance.FirstChild.Attributes(i)
    '            If Not oExistingInstance.Attributes.ItemOf(oMainAtt.Name).Value = "" Then
    '                oMainAtt.Value = oExistingInstance.Attributes.ItemOf(oMainAtt.Name).Value
    '            End If
    '        Next

    '        'Navigate Down Elmts and find with same xpath
    '        For Each oMainElmt As XmlElement In oMasterInstance.FirstChild.SelectNodes("*")
    '            CombineInstance_Sub1(oMainElmt, oExistingInstance, oMasterInstance.FirstChild.Name)
    '        Next

    '        'Now for any existing stuff that has not been brought across
    '        For Each oExistElmt As XmlElement In oExistingInstance.FirstChild.SelectNodes("descendant-or-self::*[not(@ewCombineInstanceFound)]")
    '            If Not oExistElmt.OuterXml = oExistingInstance.FirstChild.OuterXml Then
    '                CombineInstance_Sub2(oExistElmt, oMasterInstance)
    '            End If
    '        Next

    '        CombineInstance_MarkSubElmts(oMasterInstance, True)


    '    Catch ex As Exception
    '        returnException(mcModuleName, "CombineInstance", ex, "", "", gbDebug)

    '    End Try
    'End Sub

    'Protected Sub CombineInstance_Sub1(ByRef oMasterElmt As XmlElement, ByVal oExistingInstance As XmlElement, ByVal cXPath As String)
    '    'Looks for stuff with the same xpath
    '    Try
    '        If Not cXPath = "" Then cXPath = cXPath & "/"
    '        cXPath &= oMasterElmt.Name

    '        Dim oInstanceElmt As XmlElement = oExistingInstance.SelectSingleNode(cXPath)
    '        If Not oInstanceElmt Is Nothing Then
    '            'Master Attributes first
    '            For i As Integer = 0 To oMasterElmt.Attributes.Count - 1
    '                Dim oMasterAtt As XmlAttribute = oMasterElmt.Attributes(i)
    '                Dim oInstanceAtt As XmlAttribute = oInstanceElmt.Attributes.ItemOf(oMasterAtt.Name)
    '                If Not oInstanceAtt Is Nothing Then oMasterAtt.Value = oInstanceAtt.Value
    '            Next
    '            'Now Existing
    '            For i As Integer = 0 To oExistingInstance.Attributes.Count - 1
    '                Dim oInstanceAtt As XmlAttribute = oExistingInstance.Attributes(i)
    '                Dim oMasterAtt As XmlAttribute = oMasterElmt.Attributes.ItemOf(oInstanceAtt.Name)
    '                If oMasterAtt Is Nothing Then oMasterElmt.SetAttribute(oInstanceAtt.Name, oInstanceAtt.Value)
    '            Next
    '            'Now we need to check child nodes
    '            If oMasterElmt.SelectNodes("*").Count > 0 Then
    '                For Each oChild As XmlElement In oMasterElmt.SelectNodes("*")
    '                    CombineInstance_Sub1(oChild, oExistingInstance, cXPath)
    '                Next
    '            ElseIf oInstanceElmt.SelectNodes("*").Count > 0 Then
    '                For Each oChild As XmlElement In oInstanceElmt.SelectNodes("*")
    '                    oMasterElmt.AppendChild(oMasterElmt.OwnerDocument.ImportNode(oChild, True))
    '                Next
    '                CombineInstance_MarkSubElmts(oInstanceElmt)
    '            ElseIf Not oInstanceElmt.InnerText = "" Then
    '                oMasterElmt.InnerText = oInstanceElmt.InnerText
    '            End If
    '            'now mark both elements as found
    '            oMasterElmt.SetAttribute("ewCombineInstanceFound", "TRUE")
    '            oInstanceElmt.SetAttribute("ewCombineInstanceFound", "TRUE")

    '            'We wont go for others just yet as we want to complete this before finding
    '            'other less obvious stuff

    '        End If

    '    Catch ex As Exception
    '        returnException(mcModuleName, "CombineInstance_Sub1", ex, "", "", gbDebug)
    '    End Try
    'End Sub

    'Protected Sub CombineInstance_Sub2(ByRef oExisting As XmlElement, ByVal oMasterInstance As XmlElement)
    '    'Gets any unmarked in Existing data and copies over to new
    '    Try
    '        Dim cXPath As String = ""
    '        CombineInstance_GetXPath(oExisting, cXPath)
    '        Dim oParts() As String = Split(cXPath, "/")
    '        Dim oRefPart As XmlElement = oMasterInstance
    '        For i As Integer = 0 To oParts.Length - 2
    '            If Not oRefPart.SelectSingleNode(oParts(i)) Is Nothing Then
    '                oRefPart = oRefPart.SelectSingleNode(oParts(i))
    '            Else
    '                Dim oElmt As XmlElement = oMasterInstance.OwnerDocument.CreateElement(oParts(i))
    '                oRefPart.AppendChild(oElmt)
    '                oRefPart = oElmt
    '            End If
    '        Next
    '        'still need to create last part
    '        oRefPart.AppendChild(oMasterInstance.OwnerDocument.ImportNode(oExisting, True))
    '        CombineInstance_MarkSubElmts(oExisting)
    '    Catch ex As Exception
    '        returnException(mcModuleName, "CombineInstance_Sub2", ex, "", "", gbDebug)
    '    End Try
    'End Sub

    'Private Sub CombineInstance_GetXPath(ByVal oElmt As XmlElement, ByRef xPath As String)
    '    Try
    '        If xPath = "" Then xPath = oElmt.Name
    '        If Not oElmt.ParentNode Is Nothing Then
    '            If Not oElmt.ParentNode.Name = "instance" Then
    '                xPath = oElmt.ParentNode.Name & "/" & xPath
    '                CombineInstance_GetXPath(oElmt.ParentNode, xPath)
    '            End If
    '        End If
    '    Catch ex As Exception
    '        returnException(mcModuleName, "CombineInstance_GetXPath", ex, "", , gbDebug)
    '    End Try
    'End Sub

    'Private Sub CombineInstance_MarkSubElmts(ByRef oElmt As XmlElement, Optional ByVal bRemove As Boolean = False)
    '    Try
    '        If bRemove Then
    '            oElmt.RemoveAttribute("ewCombineInstanceFound")
    '        Else
    '            oElmt.SetAttribute("ewCombineInstanceFound", "TRUE")
    '        End If

    '        For Each oChild As XmlElement In oElmt.SelectNodes("*")
    '            CombineInstance_MarkSubElmts(oChild, bRemove)
    '        Next
    '    Catch ex As Exception
    '        returnException(mcModuleName, "CombineInstance_MarkSubElmts", ex, "", "", gbDebug)
    '    End Try
    'End Sub

    Sub AddValidationError(ByVal errorText As String)

        cValidationError += errorText

    End Sub

End Class

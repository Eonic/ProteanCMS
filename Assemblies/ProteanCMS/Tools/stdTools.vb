Option Strict Off
Option Explicit On 
Imports System.Xml
Imports System.Web.HttpUtility
Imports System.web.Configuration
Imports System.Web.Mail
Imports System.Reflection
Imports VB = Microsoft.VisualBasic
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Net.Mail
Imports System

Public Module stdTools

    Public mbException As Boolean
    '  Public oConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/web")

    Public goServer As System.Web.HttpServerUtility = System.Web.HttpContext.Current.Server
    Public goApp As System.Web.HttpApplicationState = System.Web.HttpContext.Current.Application

    Public PerfMon As New PerfLog("")

    ' Public msException As String = "" 'TODO !-!IMPORTANT!-! WHEN ERROR EVENTS ARE ESTABLISHED THIS SHOULD BE MOVED INSIDE WEB!!!!!
    Public mbDBError As Boolean = False

    Public gbDebug As Boolean = False ' make sure this is False when doing a release

    Public Enum SortDirection

        Ascending = 1
        Descending = 0

    End Enum

    Public Enum PollBlockReason
        None = 0
        RegisteredUsersOnly = 1
        CookieFound = 2
        LogFound = 3
        JustVoted = 4
        Excluded = 5
        PollNotAvailable = 6
    End Enum

    Public Enum ResponseType
        Undefined = 0
        Alert = 1
        Hint = 2
        Help = 3
        Redirect = 4
    End Enum

    Public SortDirectionVal() As String = {"descending", "ascending"}


    Public Sub returnException(ByRef sException As String, ByVal vstrModuleName As String, ByVal vstrRoutineName As String, ByVal oException As Exception, Optional ByVal xsltTemplatePath As String = "/ewcommon/xsl/standard.xsl", Optional ByVal vstrFurtherInfo As String = "", Optional ByVal bDebug As Boolean = False, Optional ByVal cSubjectLinePrefix As String = "")
        'Author:        Trevor Spink
        'Copyright:     Eonic Ltd 2005
        'Date:          2005-07-08

        Dim sProcessInfo As String = "Beginning"
        Dim strErrorHtml As String
        Dim strMessageHtml As String
        Dim oExceptionXml As XmlDocument = New XmlDocument
        Dim oElmt As XmlElement

        Dim styleFile As String
        Dim oStyle As New Xsl.XslTransform
        Dim sWriter As IO.StringWriter = New IO.StringWriter
        Dim sReturnHtml As String = ""
        Dim cHost As String = ""
        Dim oConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/web")

        'Dim moRequest As System.Web.HttpRequest = System.Web.HttpContext.Current.Request
        sProcessInfo = "Getting Host"



        If Not System.Web.HttpContext.Current Is Nothing Then
            cHost = System.Web.HttpContext.Current.Request.ServerVariables("HTTP_HOST")
        End If
        If sException = "" Then
            If xsltTemplatePath = "" Then
                xsltTemplatePath = oConfig("SiteXsl")
            End If

            Dim exMessage As String
            If (oException.Message = Nothing) Then
                exMessage = ""
            Else
                exMessage = oException.Message.ToString
            End If

            'want to skip this as is the result of a response redirect so not a real error.
            If ((oException.GetType.ToString <> "System.Threading.ThreadAbortException") And (Not (exMessage.Contains("The remote host closed the connection")))) Then
                'Test expanded from 1 to 2 NOT's to avoid Web Crawler based errors

                oExceptionXml.LoadXml("<Page layout=""Error""><Contents/></Page>")
                'oExceptionXml.DocumentElement.SetAttribute("baseUrl", "http://" & moRequest.ServerVariables("HTTP_HOST"))
                oExceptionXml.DocumentElement.SetAttribute("baseUrl", "http://" & cHost)
                oElmt = oExceptionXml.CreateElement("Content")
                oElmt.SetAttribute("type", "Formatted Text")
                oElmt.SetAttribute("name", "column1")

                strErrorHtml = exceptionReport(oException, vstrModuleName & "." & vstrRoutineName, vstrFurtherInfo)
                strMessageHtml = "<div style=""font-family:Verdana,Tahoma,Arial""><h2>Unfortunately this site has experienced an error.</h2>" &
                "<h3>We take all errors very seriously.</h3>" &
                "<p>" &
                "This error has been recorded and details sent to <a href=""http://eonic.com"">Eonic</a> who provide technical support for this website." &
                "</p>" &
                "<p>" &
                "Eonic welcome any feedback that helps us improve our service and that of our clients, please email any supporting information you might have as to how this error arose to <a href=""mailto:support@eonic.co.uk"">support@eonic.co.uk</a> or alternatively you are welcome call us on +44 (0)1892 534044 between 9.30am and 5.00pm GMT." &
                "</p>" &
                "<p>Please contact the owner of this website for any enquiries specific to the products and services outlined within this site.</p>" &
                "<a href=""javascript:history.back();"">Click Here to return to the previous page.</a></div>"

                Try
                    ' bDebug = True

                    If bDebug Then
                        sProcessInfo = "In Debug"
                        oElmt.InnerXml = strErrorHtml
                        oExceptionXml.SelectSingleNode("/Page/Contents").AppendChild(oElmt)

                    Else
                        'Next If statement to remove 404 errors sending two emails to error (just use the one in web.vb)
                        If (Not (vstrFurtherInfo.Contains("File Not Found"))) Then
                            sProcessInfo = "Creating Email"
                            'oElmt.InnerXml = strMessageHtml
                            ' send an email to the site admin and V3error@eonic.co.uk
                            Dim errorEmail As String = "error@protean.site"
                            If oConfig("errorEmail") <> "" Then
                                errorEmail = oConfig("errorEmail")
                            End If
                            Dim adFrom As New MailAddress(errorEmail, "ProteanCMS")
                            Dim adTo As New MailAddress("error@protean.site", "ErrorLog")
                            sProcessInfo = "Creating Email - New MailMessage Object"
                            Dim oMail As New Net.Mail.MailMessage(adFrom, adTo)
                            oMail.IsBodyHtml = True
                            'oMail.Subject = moRequest.ServerVariables("HTTP_HOST") & " has generated an Error"
                            oMail.Subject = cSubjectLinePrefix & cHost & " has generated an Error"
                            sProcessInfo = "Creating Email - Adding Body"
                            Try
                                oMail.Body = strErrorHtml
                            Catch ex2 As Exception
                                oMail.Body = "Error sending error html:" & ex2.Message
                            End Try


                            'Send the email
                            Dim oSmtp As New SmtpClient
                            Try
                                sProcessInfo = "Trying Email 1 -" & oConfig("MailServer")
                                oSmtp.Host = oConfig("MailServer")
                                If oConfig("MailServerPort") <> "" Then
                                    oSmtp.Port = oConfig("MailServerPort")
                                End If
                                If oConfig("MailServerUsername") <> "" Then
                                    oSmtp.Credentials = New System.Net.NetworkCredential(oConfig("MailServerUsername"), oConfig("MailServerPassword"))
                                End If
                                If LCase(oConfig("MailServerSSL")) = "on" Then
                                    oSmtp.EnableSsl = True
                                End If
                                If LCase(oConfig("MailServerSSL")) = "off" Then
                                    oSmtp.EnableSsl = False
                                End If
                                oSmtp.Send(oMail)
                            Catch ex As Exception
                                Try
                                    sProcessInfo = "Trying Email 2 -" & oConfig("MailServer")
                                    oSmtp.Host.Insert(0, oConfig("MailServer"))
                                    oSmtp.Send(oMail)
                                Catch exp As Exception
                                    AddExceptionToEventLog(exp, sProcessInfo, oException, vstrFurtherInfo)
                                End Try
                            End Try
                        End If
                        '############################
                        ' Load the content from the Error Page, if it exists.
                        '############################

                        Try
                            If Not mbDBError Then

                                sProcessInfo = "Loading Error Page Settings - ConStr"
                                Dim dbAuth As String

                                If oConfig("DatabasePassword") <> "" Then
                                    dbAuth = "user id=" & oConfig("DatabaseUsername") & "; password=" & oConfig("DatabasePassword")
                                Else
                                    dbAuth = oConfig("DatabaseAuth")
                                End If

                                Dim cEwConnStr As String = "Data Source=" & oConfig("DatabaseServer") & "; " &
                                "Initial Catalog=" & oConfig("DatabaseName") & "; " & dbAuth

                                sProcessInfo = "Loading Error Page Settings - Conn Open"
                                Dim cSQL As String = "SELECT nStructKey, cStructLayout FROM tblContentStructure WHERE cStructName = 'Eonic Error'"
                                Dim oCN As New SqlClient.SqlConnection(cEwConnStr)
                                Dim oCMD As New SqlClient.SqlCommand(cSQL, oCN)
                                oCMD.Connection.Open()

                                sProcessInfo = "Loading Error Page Settings - Exec Reader"
                                Dim oDR As SqlClient.SqlDataReader = oCMD.ExecuteReader

                                Dim nErrorPageId As Integer = 0
                                Dim cLayout As String = ""

                                sProcessInfo = "Data Reader"
                                Do While oDR.Read
                                    nErrorPageId = oDR(0)
                                    cLayout = oDR(1)
                                    Exit Do
                                Loop
                                oDR.Close()
                                If oCMD.Connection.State <> ConnectionState.Closed Then oCMD.Connection.Close()
                                If nErrorPageId > 0 Then
                                    sProcessInfo = "Loading Error Page"
                                    oExceptionXml.DocumentElement.SetAttribute("layout", cLayout)
                                    cSQL = "select c.nContentKey as id, dbo.fxn_getContentParents(c.nContentKey) as parId ,cContentForiegnRef as ref, cContentName as name, cContentSchemaName as type, cContentXmlBrief as content, a.nStatus as status, a.dpublishDate as publish, a.dExpireDate as expire from tblContent c inner join tblContentLocation CL on c.nContentKey = CL.nContentId inner join tblAudit a on c.nAuditId = a.nAuditKey where( CL.nStructId = " & nErrorPageId
                                    cSQL &= ") order by type, cl.nDisplayOrder"
                                    Dim oDataAdpt As New SqlClient.SqlDataAdapter(cSQL, oCN)
                                    oCN.Open()
                                    Dim oDs As New DataSet
                                    oDs.DataSetName = "Contents"
                                    oDataAdpt.Fill(oDs, "Content")
                                    oCN.Close()
                                    oDs.Tables(0).Columns("id").ColumnMapping = Data.MappingType.Attribute
                                    If oDs.Tables(0).Columns.Contains("parID") Then
                                        oDs.Tables(0).Columns("parId").ColumnMapping = Data.MappingType.Attribute
                                    End If
                                    oDs.Tables(0).Columns("ref").ColumnMapping = Data.MappingType.Attribute
                                    oDs.Tables(0).Columns("name").ColumnMapping = Data.MappingType.Attribute
                                    oDs.Tables(0).Columns("type").ColumnMapping = Data.MappingType.Attribute
                                    oDs.Tables(0).Columns("status").ColumnMapping = Data.MappingType.Attribute
                                    oDs.Tables(0).Columns("publish").ColumnMapping = Data.MappingType.Attribute
                                    oDs.Tables(0).Columns("expire").ColumnMapping = Data.MappingType.Attribute
                                    oDs.Tables(0).Columns("content").ColumnMapping = Data.MappingType.SimpleContent
                                    oDs.EnforceConstraints = False
                                    'convert to Xml Dom
                                    Dim oXml As XmlDataDocument = New XmlDataDocument(oDs)
                                    oXml.PreserveWhitespace = False
                                    oExceptionXml.SelectSingleNode("/Page/Contents").InnerXml = Replace(Replace(oXml.DocumentElement.InnerXml, "&gt;", ">"), "&lt;", "<")
                                End If

                            End If
                        Catch ex As Exception

                            mbDBError = True
                            'sProcessInfo = "Found Error"
                            AddExceptionToEventLog(ex, sProcessInfo, oException, vstrFurtherInfo)
                            oElmt.InnerXml = strMessageHtml
                            oExceptionXml.SelectSingleNode("/Page/Contents").AppendChild(oElmt)
                        End Try
                        '###########################
                    End If

                    sProcessInfo = "Loading XSLT"

                    If LCase(xsltTemplatePath).StartsWith("c:\") Or LCase(xsltTemplatePath).StartsWith("d:\") Then
                        styleFile = CType(xsltTemplatePath, String)
                    Else
                        styleFile = CType(goServer.MapPath(xsltTemplatePath), String)
                    End If

                    oStyle.Load(styleFile)

                    'add Eonic Bespoke Functions
                    Dim xsltArgs As Xsl.XsltArgumentList = New Xsl.XsltArgumentList
                    Dim errWeb As Protean.Cms
                    If Not System.Web.HttpContext.Current Is Nothing Then
                        'so we compile errors out of debug mode too.
                        errWeb = New Protean.Cms(System.Web.HttpContext.Current)
                        errWeb.InitializeVariables()
                        Dim ewXsltExt As New xsltExtensions(errWeb)
                        xsltArgs.AddExtensionObject("urn:ew", ewXsltExt)
                    Else
                        Dim ewXsltExt As New xsltExtensions()
                        xsltArgs.AddExtensionObject("urn:ew", ewXsltExt)
                    End If


                    sProcessInfo = "Transform"
                    ' Would be useful to have some indicator that this is an error, even if loading from another page.
                    oElmt = oExceptionXml.SelectSingleNode("Page")
                    oElmt.SetAttribute("error", "true")
                    oStyle.Transform(oExceptionXml, xsltArgs, sWriter, Nothing)
                    sProcessInfo = "Setting Return"
                    sReturnHtml = sWriter.ToString()
                    errWeb = Nothing

                Catch ex As Exception
                    AddExceptionToEventLog(ex, sProcessInfo, oException, vstrFurtherInfo)
                    If Not (gbDebug) And strMessageHtml <> "" Then
                        strErrorHtml = strMessageHtml
                    End If
                    sReturnHtml = "<html><title>Error Message</title><body>" & strErrorHtml & "</body></html>"

                Finally
                    sException = sReturnHtml
                End Try
            End If
        End If
    End Sub

    Public Sub reportException(ByRef sException As String, ByVal vstrModuleName As String, ByVal vstrRoutineName As String, ByVal oException As Exception, Optional ByVal xsltTemplatePath As String = "/ewcommon/xsl/standard.xsl", Optional ByVal vstrFurtherInfo As String = "", Optional ByVal bDebug As Boolean = False, Optional ByVal cSubjectLinePrefix As String = "")

        'Author:        Trevor Spink
        'Copyright:     Eonic Ltd 2005
        'Date:          2005-07-08

        Dim sProcessInfo As String = "Beginning"
        Dim strErrorHtml As String
        Dim strMessageHtml As String
        Dim oExceptionXml As XmlDocument = New XmlDocument
        Dim oElmt As XmlElement

        Dim oStyle As New Xsl.XslTransform
        Dim sWriter As IO.StringWriter = New IO.StringWriter
        Dim sReturnHtml As String = ""
        Dim cHost As String = ""
        Dim oConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/web")

        'Dim moRequest As System.Web.HttpRequest = System.Web.HttpContext.Current.Request
        sProcessInfo = "Getting Host"
        If Not System.Web.HttpContext.Current Is Nothing Then
            cHost = System.Web.HttpContext.Current.Request.ServerVariables("HTTP_HOST")
        End If
        If sException = "" Then
            If xsltTemplatePath = "" Then
                xsltTemplatePath = oConfig("SiteXsl")
            End If

            Dim exMessage As String
            If (oException.Message = Nothing) Then
                exMessage = ""
            Else
                exMessage = oException.Message.ToString
            End If

            'want to skip this as is the result of a response redirect so not a real error.
            If ((oException.GetType.ToString <> "System.Threading.ThreadAbortException") And (Not (exMessage.Contains("The remote host closed the connection")))) Then
                'Test expanded from 1 to 2 NOT's to avoid Web Crawler based errors

                oExceptionXml.LoadXml("<Page layout=""Error""><Contents/></Page>")
                'oExceptionXml.DocumentElement.SetAttribute("baseUrl", "http://" & moRequest.ServerVariables("HTTP_HOST"))
                oExceptionXml.DocumentElement.SetAttribute("baseUrl", "http://" & cHost)
                oElmt = oExceptionXml.CreateElement("Content")
                oElmt.SetAttribute("type", "Formatted Text")
                oElmt.SetAttribute("name", "column1")

                strErrorHtml = exceptionReport(oException, vstrModuleName & "." & vstrRoutineName, vstrFurtherInfo)
                strMessageHtml = "<div style=""font-family:Verdana,Tahoma,Arial""><h2>Unfortunately this site has experienced an error.</h2>" &
                "<h3>We take all errors very seriously.</h3>" &
                "<p>" &
                "This error has been recorded and details sent to <a href=""http://www.eonic.co.uk"">Eonic</a> who provide technical support for this website." &
                "</p>" &
                "<p>" &
                "Eonic welcome any feedback that helps us improve our service and that of our clients, please email any supporting information you might have as to how this error arose to <a href=""mailto:support@eonic.co.uk"">support@eonic.co.uk</a> or alternatively you are welcome call us on +44 (0)1892 534044 between 9.30am and 5.00pm GMT." &
                "</p>" &
                "<p>Please contact the owner of this website for any enquiries specific to the products and services outlined within this site.</p>" &
                "<a href=""javascript:history.back();"">Click Here to return to the previous page.</a></div>"

                Try

                    If (Not (vstrFurtherInfo.Contains("File Not Found"))) Then
                        sProcessInfo = "Creating Email"
                        ' send an email to the site admin and error@eonic.co.uk
                        Dim errorEmail As String = "error@protean.site"
                        If oConfig("errorEmail") <> "" Then
                            errorEmail = oConfig("errorEmail")
                        End If
                        Dim adFrom As New MailAddress(errorEmail, "ProteanCMS")
                        Dim adTo As New MailAddress("error@protean.site", "ErrorLog")
                        sProcessInfo = "Creating Email - New MailMessage Object"
                        Dim oMail As New Net.Mail.MailMessage(adFrom, adTo)
                        oMail.IsBodyHtml = True
                        oMail.Subject = cSubjectLinePrefix & cHost & " has generated an Error"
                        sProcessInfo = "Creating Email - Adding Body"
                        Try
                            oMail.Body = strErrorHtml
                        Catch ex2 As Exception
                            oMail.Body = "Error sending error html:" & ex2.Message
                        End Try

                        'Send the email
                        Dim oSmtp As New SmtpClient
                        Try
                            sProcessInfo = "Trying Email 1 -" & oConfig("MailServer")
                            oSmtp.Host = oConfig("MailServer")
                            If oConfig("MailServerPort") <> "" Then
                                oSmtp.Port = oConfig("MailServerPort")
                            End If
                            If oConfig("MailServerUsername") <> "" Then
                                oSmtp.Credentials = New System.Net.NetworkCredential(oConfig("MailServerUsername"), oConfig("MailServerPassword"))
                            End If
                            If LCase(oConfig("MailServerSSL")) = "on" Then
                                oSmtp.EnableSsl = True
                            End If
                            If LCase(oConfig("MailServerSSL")) = "off" Then
                                oSmtp.EnableSsl = False
                            End If
                            oSmtp.Send(oMail)
                        Catch ex As Exception
                            Try
                                sProcessInfo = "Trying Email 2 -" & oConfig("MailServer")
                                oSmtp.Host.Insert(0, oConfig("MailServer"))
                                oSmtp.Send(oMail)
                            Catch exp As Exception
                                AddExceptionToEventLog(exp, sProcessInfo, oException, vstrFurtherInfo)
                            End Try
                        End Try
                    End If

                Catch ex As Exception
                    AddExceptionToEventLog(ex, sProcessInfo, oException, vstrFurtherInfo)
                End Try
            End If
        End If
    End Sub


    Private Sub TransformErrorHandle(ByVal cModuleName As String, ByVal cRoutineName As String, ByVal oException As Exception, ByVal cFurtherInfo As String)
        AddExceptionToEventLog(oException, cFurtherInfo)
    End Sub

    Friend Sub AddExceptionToEventLog(ByVal oCurrentException As Exception, ByVal cCurrentInfo As String, Optional ByVal oOriginalError As Exception = Nothing, Optional ByVal cOriginalInfo As String = "")
        'writes an event to the even log under the heading "EonicWebV4.1"
        Try
            Dim oEventLog As System.Diagnostics.EventLog = Nothing
            Dim oELs() As System.Diagnostics.EventLog = System.Diagnostics.EventLog.GetEventLogs
            Dim i As Integer = 0
            For i = 0 To oELs.Length - 1
                If oELs(i).Log = "ProteanCMS" Then
                    oEventLog = oELs(i)
                    Exit For
                End If
            Next
            If oEventLog Is Nothing Then Exit Sub
            Dim cSource As String = "ProteanCMS Site"

            oEventLog.Source = cSource

            Dim cMessage As String = ""
            'The Current Error

            cMessage = "Site: " & System.Web.HttpContext.Current.Request.ServerVariables("HTTP_HOST") & vbNewLine & vbNewLine

            If Not oCurrentException Is Nothing Then
                cMessage &= vbNewLine & "Current Error: " & vbNewLine
                cMessage &= "Info:" & cCurrentInfo & vbNewLine
                cMessage &= "Exception Type:" & oCurrentException.GetType.ToString & vbNewLine
                cMessage &= "Message:" & oCurrentException.Message() & vbNewLine
                cMessage &= "Source:" & oCurrentException.Source() & vbNewLine
                cMessage &= "Stack:" & oCurrentException.StackTrace & vbNewLine
                cMessage &= "Full Exception:" & oCurrentException.ToString & vbNewLine
            End If
            'We might be coming from an error handling procedure so lets get the orignal error that sent us there too
            If Not oOriginalError Is Nothing Then
                cMessage &= vbNewLine & "Original Error: " & vbNewLine
                cMessage &= "Info:" & cOriginalInfo & vbNewLine
                cMessage &= "Exception Type:" & oOriginalError.GetType.ToString & vbNewLine
                cMessage &= "Message:" & oOriginalError.Message() & vbNewLine
                cMessage &= "Source:" & oOriginalError.Source() & vbNewLine
                cMessage &= "Stack:" & oOriginalError.StackTrace & vbNewLine
                cMessage &= "Full Exception:" & oOriginalError.ToString & vbNewLine
            End If



            oEventLog.WriteEntry(cMessage, EventLogEntryType.Error)
            oEventLog = Nothing

        Catch ex As Exception
            'cant do diddly but cry 
        End Try
    End Sub


    Public Function exceptionReport(ByVal oException As Exception, ByVal sComponent As String, ByVal sInfo As String) As String

        Dim cReport As String
        Dim cSV As String
        Dim cAssembly As String = ""
        Dim moRequest As System.Web.HttpRequest = Nothing
        Dim moSession As System.Web.SessionState.HttpSessionState = Nothing

        If Not System.Web.HttpContext.Current Is Nothing Then
            moRequest = System.Web.HttpContext.Current.Request
            moSession = System.Web.HttpContext.Current.Session

            System.Web.HttpContext.Current.Server.MapPath("")
        End If

        cReport = "<div style=""font: normal .75em/1.5 Verdana, Tahoma, sans-serif;""><h2>ProteanCMS has returned the following Error</h2>" &
                  "<table cellpadding=""1"" cellspacing=""0"" border=""0"">"

        ' Report Information
        addExceptionHeader(cReport, "Report Info")
        addExceptionLine(cReport, "Date + Time", FormatDateTime(Now, DateFormat.GeneralDate))
        addExceptionLine(cReport, "Webserver:", System.Environment.MachineName)
        addExceptionLine(cReport, "SiteName:", System.Web.Hosting.HostingEnvironment.ApplicationHost.GetSiteName())

        Dim a As System.Reflection.Assembly = System.Reflection.Assembly.GetExecutingAssembly()
        addExceptionLine(cReport, "Assembly", a.FullName)
        addExceptionLine(cReport, "Codebase", a.CodeBase)

        ' Referenced Assemblies
        Dim an As AssemblyName
        For Each an In a.GetReferencedAssemblies()
            cAssembly = cAssembly & an.Name & " (" & an.Version.ToString & "); "
        Next
        addExceptionLine(cReport, "Referenced Assemblies", cAssembly)

        ' Exception Details
        addExceptionHeader(cReport, "Exception Details")
        addExceptionLine(cReport, "Component:", sComponent)
        addExceptionLine(cReport, "Info:", sInfo)
        addExceptionLine(cReport, "Exception Type:", oException.GetType.ToString)
        addExceptionLine(cReport, "Message:", oException.Message())
        addExceptionLine(cReport, "Source:", oException.Source())
        addExceptionLine(cReport, "Stack:", oException.StackTrace)
        addExceptionLine(cReport, "Full Exception:", oException.ToString)

        ' Session Variables
        If Not moSession Is Nothing Then
            If moSession.Count > 0 Then
                addExceptionHeader(cReport, "Session Variables")
                For Each cSV In moSession
                    Try
                        addExceptionLine(cReport, cSV, moSession(cSV))
                    Catch
                        addExceptionLine(cReport, cSV, "object cannot be converted to string")
                    End Try
                Next
            Else
                addExceptionHeader(cReport, "No Session variables found")
            End If
        End If

        ' Querystring Variables
        If Not moRequest Is Nothing Then
            addExceptionHeader(cReport, "Server Variables")
            addExceptionLine(cReport, "AppPath", moRequest.ApplicationPath)
            addExceptionLine(cReport, "AppPath", moRequest.ApplicationPath)
            '++++++++++++ Only re-enable this temporarily if needed for debugging +++++ Can cause credit card numbers to show in error messages.... bad karma man!

            'If moRequest.QueryString.Count > 0 Then
            '    addExceptionHeader(cReport, "Querystring Variables")
            '    For Each cSV In moRequest.QueryString
            '        addExceptionLine(cReport, cSV, moRequest.QueryString(cSV))
            '    Next
            'Else
            '    addExceptionHeader(cReport, "No Querystring variables found")
            'End If

            '' Form Variables
            'If moRequest.Form.Count > 0 Then
            '    addExceptionHeader(cReport, "Form Variables")
            '    For Each cSV In moRequest.Form
            '        If TypeName(moRequest.Form(cSV)) = "String" Then addExceptionLine(cReport, cSV, moRequest.Form(cSV))
            '    Next
            'Else
            '    addExceptionHeader(cReport, "No Form variables found")
            'End If

            '' Server Variables
            addExceptionHeader(cReport, "Server Variables")
            For Each cSV In moRequest.ServerVariables
                'If Left(cSV, 5) = "HTTP_" Then addExceptionLine(cReport, cSV, moRequest.ServerVariables(cSV))
                addExceptionLine(cReport, cSV, moRequest.ServerVariables(cSV))
            Next
        End If

        cReport = cReport & "</table></div>"
        exceptionReport = Protean.Tools.Xml.convertEntitiesToCodes(cReport)

        Select Case oException.GetType.ToString

            ' case ""

        End Select

    End Function

    Public Sub addExceptionHeader(ByRef cReport As String, ByVal cHeader As String)

        cReport = cReport & "<tr><th colspan=""2"" valign=""top"" align=""left"" style=""padding-top:2em""><h3>" & cHeader & "</h3></th></tr>"

    End Sub
    Public Sub addExceptionLine(ByRef cReport As String, ByVal cHeader As String, ByVal cValue As String)

        cValue = Replace(cValue, "<", "&lt;")
        cValue = Replace(cValue, ">", "&gt;")
        cValue = Replace(cValue, Chr(10), "<br/>")
        cReport = cReport & "<tr><th valign=""top"" align=""left"">" & cHeader & "</th><td valign=""top"">" & cValue & "</td></tr>"

    End Sub


    Public Function SqlFmt(ByVal sText As String) As Object
        'PerfMon.Log("stdTools", "SqlFmt")
        SqlFmt = Replace(sText, "'", "''")

    End Function


    Public Function xPathEscapeQuote(ByVal sText As String) As Object
        If sText.Contains("'") Then

            Return "concat('" & sText.Replace("'", "', ""'"", '") & "')"
        Else
            Return "'" & sText & "'"
        End If

    End Function

    Public Function niceDate(ByVal dDate As Object) As String
        PerfMon.Log("stdTools", "niceDate")
        Dim sdate As String

        If IsDate(dDate) Then
            sdate = CStr(CDate(dDate))
            niceDate = VB.Day(CDate(sdate)) & " " & MonthName(Month(CDate(sdate)), True) & " " & Year(CDate(sdate))
        Else
            If dDate <> "00:00:00" Then
                niceDate = ""
            Else
                niceDate = ""
            End If
        End If

    End Function

#Region "SQL DATE TIME FUNCTIONS"
    'Public Function sqlDate(ByVal dDate As Object) As String
    '    'PerfMon.Log("stdTools", "sqlDate")

    '    If IsDate(dDate) Then
    '        sqlDate = "'" & Format(dDate, "dd MMMM yyyy") & "'"
    '    Else
    '        If dDate <> "00:00:00" Then
    '            sqlDate = "null"
    '        Else
    '            sqlDate = "null"
    '        End If
    '    End If
    '    If dDate = #12:00:00 AM# Then sqlDate = "null"
    '    If dDate = #12:00:00 PM# Then sqlDate = "null"
    'End Function

    'Public Function sqlDateTime(ByVal dDateTime As Object) As String
    '    If IsDate(dDateTime) Then
    '        sqlDateTime = "'" & Format(dDateTime, "dd-MMM-yy HH:mm:ss") & "'"
    '    Else
    '        sqlDateTime = "null"
    '    End If
    'End Function

    'Public Function sqlDateTime(ByVal dDate As Object, ByVal stime As Object) As String
    '    PerfMon.Log("stdTools", "sqlDateTime")

    '    If IsDate(dDate) Then
    '        sqlDateTime = "'" & Format(dDate, "dd MMMM yyyy") & " " & stime & "'"
    '    Else
    '        If dDate <> "00:00:00" Then
    '            sqlDateTime = "null"
    '        Else
    '            sqlDateTime = "null"
    '        End If
    '    End If

    'End Function

    'Left here for external applications to use
    Public Function sqlDate(ByVal dDate As Object) As String
        Return Protean.Tools.Database.SqlDate(dDate, False)
    End Function

    Public Function sqlDateTime(ByVal dDateTime As Object) As String
        Return Protean.Tools.Database.SqlDate(dDateTime, True)
    End Function

    Public Function sqlDateTime(ByVal dDate As Object, ByVal stime As Object) As String
        Return Protean.Tools.Database.SqlDate(Format(dDate, "dd MMMM yyyy") & " " & stime, True)
    End Function



#End Region
#Region "XML DATE TIME FUNCTIONS"

    'Public Function xmlDate(ByVal dDate As Object) As String
    '    PerfMon.Log("stdTools", "xmlDate")
    '    Dim sDay As String
    '    Dim sMonth As String

    '    If IsDBNull(dDate) Then
    '        xmlDate = ""
    '    Else
    '        sDay = CStr(VB.Day(dDate))
    '        If Len(sDay) = 1 Then sDay = "0" & sDay

    '        sMonth = CStr(Month(dDate))
    '        If Len(sMonth) = 1 Then sMonth = "0" & sMonth

    '        xmlDate = Year(dDate) & "-" & sMonth & "-" & sDay

    '    End If

    'End Function

    'Public Function xmlDateTime(ByVal dDate As Object) As String
    '    PerfMon.Log("stdTools", "xmlDateTime")
    '    Dim sDay As String
    '    Dim sMonth As String

    '    If IsDBNull(dDate) Then
    '        xmlDateTime = ""
    '    Else
    '        sDay = CStr(VB.Day(dDate))
    '        If Len(sDay) = 1 Then sDay = "0" & sDay

    '        sMonth = CStr(Month(dDate))
    '        If Len(sMonth) = 1 Then sMonth = "0" & sMonth

    '        xmlDateTime = Trim(Year(dDate) & "-" & sMonth & "-" & sDay & "T" & TimeValue(dDate))

    '    End If

    'End Function

    Public Function xmlDate(ByVal dDate As Object) As String
        Return Protean.Tools.Xml.XmlDate(dDate)
    End Function

    Public Function xmlDateTime(ByVal dDate As Object) As String
        Return Protean.Tools.Xml.XmlDate(dDate, True)
    End Function
#End Region

  


    Public Function ButtonSubmitted(ByRef mroRequest As System.Web.HttpRequest, ByVal cButtonName As String) As Boolean
        '  PerfMon.Log("stdTools", "ButtonSubmitted")
        Dim bSubmitted As Boolean = False

        Dim cNameX As String = cButtonName + ".x"
        Dim cNameY As String = cButtonName + ".y"

        If Not (mroRequest(cButtonName) Is Nothing) Or Not (mroRequest(cNameX) Is Nothing) Or Not (mroRequest(cNameY) Is Nothing) Then bSubmitted = True
        ButtonSubmitted = bSubmitted

    End Function


    Public Function UrlResponseToHashTable(ByVal sResponse As String, Optional ByVal sFieldSeperator As String = "&", Optional ByVal sValueSeperator As String = "=", Optional ByVal bURLDecodeValue As Boolean = True) As Hashtable
        PerfMon.Log("stdTools", "UrlResponseToHashTable")
        Try

            Dim aResponse As String() = Split(sResponse, sFieldSeperator)
            Dim cKey As String
            Dim cValue As String
            Dim nPos As Integer
            Dim oResponseDict As New Hashtable
            Dim i As Integer
            For i = 0 To UBound(aResponse)

                ' It's not a guarantee that the value separator won't also appear in the value!
                ' e.g. ACSURL=http://someurl/?key=foo
                '      PaReq=isdfjeariogj/vw==
                '
                ' So rather than use Split, we should look for the first instance of the value separator.
                nPos = aResponse(i).IndexOf(sValueSeperator)

                ' Ignore blank keys (0) and separators that are not found (-1)
                If nPos > 0 Then
                    cKey = aResponse(i).Substring(0, nPos)
                    If nPos = aResponse(i).Length - 1 Then
                        cValue = ""
                    Else
                        cValue = aResponse(i).Substring(nPos + 1)
                    End If
                    If bURLDecodeValue Then cValue = goServer.UrlDecode(cValue)
                    oResponseDict.Add(cKey, cValue)
                End If

            Next

            Return oResponseDict
        Catch ex As Exception
            'returnException("stdTools", "UrlResponseToHashTable", ex, "", "", gbDebug)
            Return Nothing
        End Try
    End Function


    Public Function GenerateMD5Hash(ByVal SourceText As String) As String
        PerfMon.Log("stdTools", "GenerateMD5Hash")
        'This generates a PHP compatible MD5 Hex string for the source value.

        Dim md5 As MD5 = MD5CryptoServiceProvider.Create
        Dim dataMd5 As Byte() = md5.ComputeHash(Encoding.Default.GetBytes(SourceText))
        Dim sb As StringBuilder = New StringBuilder
        Dim i As Integer = 0
        While i < dataMd5.Length
            sb.AppendFormat("{0:x2}", dataMd5(i))
            System.Math.Min(System.Threading.Interlocked.Increment(i), i - 1)
        End While
        Return sb.ToString
    End Function

    Public Function EncryptString(ByVal SourceText As String, Optional ByVal bUseAsymmetric As Boolean = True, Optional ByVal cSalt As String = "") As String

        Dim encryptedData As Tools.Encryption.EncData
        Try


            If bUseAsymmetric Then

                Dim asym As New Tools.Encryption.Asymmetric
                Dim pubkey As New Tools.Encryption.Asymmetric.PublicKey
                Dim privkey As New Tools.Encryption.Asymmetric.PrivateKey

                If WebConfigurationManager.AppSettings("PublicKey.Modulus") = "" Then
                    'at the moment this writes a new file that needs to be included in web.config
                    asym.GenerateNewKeyset(pubkey, privkey)
                    pubkey.ExportToConfigFile(goServer.MapPath("encrypt.config"))
                    privkey.ExportToConfigFile(goServer.MapPath("encrypt.config"))
                Else
                    pubkey.LoadFromConfig()
                    privkey.LoadFromConfig()
                End If

                'End Try


                encryptedData = asym.Encrypt(New Tools.Encryption.EncData(SourceText), pubkey)

            Else

                ' This may seem unnecessary, but symmetric encryption is relatively weak (often because of the choice of key),
                ' so why not encrypt the symmetric key asymettrically to end up with a bastard-hard sym-key to guess.
                ' UPDATE For some reason the assym encryption wouldn't consistently betwenn ENC and DEC so I've commented it out
                ' for now - rely on salt.
                ' UPDATE Get the right key size for Rijndael - try using the PrivateKey.D
                Dim sym As New Tools.Encryption.Symmetric(Tools.Encryption.Symmetric.Provider.Rijndael)
                sym.InitializationVector = New Tools.Encryption.EncData(WebConfigurationManager.AppSettings("SymmetricIV"))
                sym.Key = New Tools.Encryption.EncData(WebConfigurationManager.AppSettings("PrivateKey.D"))
                encryptedData = sym.Encrypt(New Tools.Encryption.EncData(SourceText))

            End If

            Return encryptedData.ToHex

        Catch ex As Exception
            Return ""
        End Try


    End Function

    Public Function DecryptString(ByVal encryptedText As String, Optional ByVal bUseAsymmetric As Boolean = True, Optional ByVal cSalt As String = "") As String
        Try

            Dim decryptedData As Tools.Encryption.EncData = New Tools.Encryption.EncData

            Dim encryptedData As Tools.Encryption.EncData = New Tools.Encryption.EncData
            encryptedData.Hex = encryptedText

            If bUseAsymmetric Then
                Dim asym As New Tools.Encryption.Asymmetric
                Dim pubkey As New Tools.Encryption.Asymmetric.PublicKey
                Dim privkey As New Tools.Encryption.Asymmetric.PrivateKey
                pubkey.LoadFromConfig()
                privkey.LoadFromConfig()

                Dim asym2 As New Tools.Encryption.Asymmetric
                decryptedData = asym2.Decrypt(encryptedData, privkey)

            Else
                ' UPDATE For some reason the assym encryption wouldn't consistently betwenn ENC and DEC so I've commented it out
                ' for now - rely on salt.
                Dim sym As New Tools.Encryption.Symmetric(Tools.Encryption.Symmetric.Provider.Rijndael)
                sym.InitializationVector = New Tools.Encryption.EncData(WebConfigurationManager.AppSettings("SymmetricIV"))
                sym.Key = New Tools.Encryption.EncData(WebConfigurationManager.AppSettings("PrivateKey.D"))
                decryptedData = sym.Decrypt(encryptedData)
            End If

            Return decryptedData.ToString

        Catch ex As Exception
            Return ""
        End Try

    End Function

    Public Function EncryptStringOLD(ByVal SourceText As String) As String
        PerfMon.Log("stdTools", "EncryptString")
        Dim asym As New Tools.Encryption.Asymmetric
        Dim pubkey As New Tools.Encryption.Asymmetric.PublicKey
        Dim privkey As New Tools.Encryption.Asymmetric.PrivateKey

        If WebConfigurationManager.AppSettings("PublicKey.Modulus") = "" Then
            'at the moment this writes a new file that needs to be included in web.config
            asym.GenerateNewKeyset(pubkey, privkey)
            pubkey.ExportToConfigFile(goServer.MapPath("encrypt.config"))
            privkey.ExportToConfigFile(goServer.MapPath("encrypt.config"))
        Else
            pubkey.LoadFromConfig()
            privkey.LoadFromConfig()
        End If

        'End Try

        Dim encryptedData As Tools.Encryption.EncData
        encryptedData = asym.Encrypt(New Tools.Encryption.EncData(SourceText), pubkey)

        Return encryptedData.ToHex

    End Function

    Public Function DecryptStringOLD(ByVal encryptedText As String) As String
        PerfMon.Log("stdTools", "DecryptString")
        Try
            Dim asym As New Tools.Encryption.Asymmetric
            Dim pubkey As New Tools.Encryption.Asymmetric.PublicKey
            Dim privkey As New Tools.Encryption.Asymmetric.PrivateKey
            pubkey.LoadFromConfig()
            privkey.LoadFromConfig()

            Dim encryptedData As Tools.Encryption.EncData = New Tools.Encryption.EncData
            encryptedData.Hex = encryptedText

            Dim decryptedData As Tools.Encryption.EncData = New Tools.Encryption.EncData
            Dim asym2 As New Tools.Encryption.Asymmetric
            decryptedData = asym2.Decrypt(encryptedData, privkey)

            Return decryptedData.ToString
        Catch
            Return ""
        End Try

    End Function

    Public Enum DiscountCategory

        'describes the type of discount and how its worked out
        Basic = 1 'Percent or Value Off = Product Specific
        BreakProduct = 2 ' Quantity or Value Breaks = Product Specific
        X4PriceY = 3 'Buy X for the price of Y (Buy X Get Y Free) = Product Specific
        CheapestFree = 4 'Cheapest Item Free = Group specific
        BreakGroup = 5 'Cheapest Item Free = Group specific
    End Enum

    Public Function Round(ByVal nNumber As Object, Optional ByVal nDecimalPlaces As Integer = 2, Optional ByVal nSplitNo As Integer = 5, Optional ByVal bForceRoundup As Boolean = True, Optional ByVal bForceRoundDown As Boolean = False) As Decimal
        'PerfMon.Log("stdTools", "RoundUp")
        Try
            Dim RetVal As Decimal
            If bForceRoundup Then
                RetVal = RoundUp(nNumber, nDecimalPlaces, nSplitNo)
            Else
                If bForceRoundDown Then
                    Dim adjustment As Double = Math.Pow(10, nDecimalPlaces)
                    RetVal = Math.Floor(nNumber * adjustment) / adjustment
                    'RetVal = Math.Round(nNumber, nDecimalPlaces, MidpointRounding.ToEven)
                Else
                    RetVal = FormatNumber(nNumber, nDecimalPlaces)
                End If
            End If
            Return RetVal
        Catch ex As Exception
            Return 0
        End Try
    End Function

    Public Function RoundUp(ByVal nNumber As Object, Optional ByVal nDecimalPlaces As Integer = 2, Optional ByVal nSplitNo As Integer = 5) As Decimal
        PerfMon.Log("stdTools", "RoundUp")
        Try
            'get the dross over with
            If Not IsNumeric(nNumber) Then Return 0
            'no decimal places to deal with
            If Not nNumber.ToString.Contains(".") Then Return nNumber
            'has correct number of decimal places
            If Split(nNumber.ToString, ".")(1).Length <= nDecimalPlaces Then Return nNumber

            'now the fun
            Dim nWholeNo As Integer = Split(nNumber.ToString, ".")(0) 'the whole number before decimal point
            Dim nTotalLength As Integer = Split(nNumber.ToString, ".")(1).Length 'the total number of decimal places

            Dim nI As Integer 'a counter

            Dim nCarry As Integer = 0 'number to carry to next number

            'loop through until we reach the correct number of decimal places
            For nI = 0 To nTotalLength - nDecimalPlaces
                Dim nCurrent As Integer 'the number we are working on
                nCurrent = Right(Left(Split(nNumber.ToString, ".")(1), nTotalLength - nI), 1)
                nCurrent += nCarry 'add the carry
                If nCurrent >= nSplitNo Then nCarry = 1 Else nCarry = 0 'make a new carry dependant on whaere we are
            Next
            Dim nDecimal As Integer = Left(Split(nNumber.ToString, ".")(1), nDecimalPlaces) 'the decimal value
            nDecimal += nCarry 'add last carry
            If nDecimal.ToString.Length > nDecimalPlaces Then 'if we have now gone over the number of decimal places then need to sort it
                nCarry = 1
                nDecimal = Right(nDecimal.ToString, nDecimalPlaces)
            Else
                nCarry = 0
            End If
            nWholeNo += nCarry
            Return CDec(nWholeNo & "." & nDecimal)
        Catch ex As Exception
            Return 0
            'returnException("stdTools", "Round Up", ex, "", "", gbDebug)
        End Try
    End Function



    Public Function MaskString(ByVal cInitialString As String, Optional ByVal cMaskchar As String = "*", Optional ByVal bKeepSpaces As Boolean = False, Optional ByVal nNoCharsToLeave As Integer = 4) As String
        Dim cNewString As String = ""
        Try
            If Not bKeepSpaces Then cInitialString = cInitialString.Replace(" ", "")
            Dim i As Integer
            For i = 0 To (cInitialString.Length - (nNoCharsToLeave + 1))
                If Not cInitialString.Substring(i, 1) = " " Then
                    cNewString &= cMaskchar
                Else
                    cNewString &= " "
                End If
            Next
            cNewString &= Right(cInitialString, nNoCharsToLeave)
            Return cNewString
        Catch ex As Exception
            '   returnException("stdTools", "MaskString", ex, "", "", gbDebug)
            Return cNewString
        End Try
    End Function



    Public Function objectToNumeric(ByVal oRequestItem As Object, Optional ByVal bDefaultValue As Long = 0, Optional ByVal bAllowNegatives As Long = False) As Long

        Dim nRequest As Long

        If IsNumeric(oRequestItem) Then
            If bAllowNegatives Then
                nRequest = CLng(oRequestItem)
            ElseIf oRequestItem > 0 Then
                nRequest = CLng(oRequestItem)
            Else
                nRequest = bDefaultValue
            End If
        Else
            nRequest = bDefaultValue
        End If

        Return nRequest

    End Function

    Public Sub HTTPRedirect(ByRef oCtx As System.Web.HttpContext, ByVal cURL As String, Optional ByRef nStatusCode As Integer = 302)

        ' Response.Redirect always results in 302 (unless you use the .NET 3.0 Web.Extensions)
        ' so you have to go through this rigmorale.

        Try

            ' Look for valid codes
            Select Case nStatusCode
                Case 301, 302, 303, 304, 307
                    ' Do nothing
                Case Else
                    nStatusCode = 302
            End Select

            oCtx.Response.StatusCode = nStatusCode

            ' Set the description
            Select Case nStatusCode

                Case 301
                    oCtx.Response.StatusDescription = "Moved Permanently"

                Case 302
                    oCtx.Response.StatusDescription = "Found"

                Case 303
                    oCtx.Response.StatusDescription = "See Other"

                Case 304
                    oCtx.Response.StatusDescription = "Not Modified"

                Case 307
                    oCtx.Response.StatusDescription = "Temporary Redirect"

            End Select

            oCtx.Response.RedirectLocation = cURL
            oCtx.ApplicationInstance.CompleteRequest()
            oCtx.Response.End()

        Catch ex As Exception

        End Try

    End Sub


    Public Function strongPassword(ByVal password As String) As Boolean
        Dim pwdRegEx As String = ""

        Dim moPolicy As xmlElement

        moPolicy = WebConfigurationManager.GetWebApplicationSection("protean/PasswordPolicy")

        pwdRegEx = "(?=^.{" & moPolicy.FirstChild.SelectSingleNode("minLength").InnerText & _
            "," & moPolicy.FirstChild.SelectSingleNode("maxLength").InnerText & "}$)" & _
            "(?=(?:.*?\d){" & moPolicy.FirstChild.SelectSingleNode("numsLength").InnerText & "})" & _
            "(?=.*[a-z])" & _
            "(?=(?:.*?[A-Z]){" & moPolicy.FirstChild.SelectSingleNode("upperLength").InnerText & "})" & _
            "(?=(?:.*?[" & moPolicy.FirstChild.SelectSingleNode("specialChars").InnerText & "]){" & moPolicy.FirstChild.SelectSingleNode("specialLength").InnerText & "})" & _
            "(?!.*\s)[0-9a-zA-Z" & moPolicy.FirstChild.SelectSingleNode("specialChars").InnerText & "]*$"

        ' Validate the e-mail address
        Return New Regex(pwdRegEx, RegexOptions.IgnoreCase).IsMatch(password & "")


    End Function

    'Sub SetDefaultSortColumn(ByRef moPageXml As XmlDocument, ByVal nSortColumn As Long, Optional ByVal nSortDirection As SortDirection = SortDirection.Ascending)
    '    PerfMon.Log("stdTools", "SetDefaultSortColumn")
    '    Try
    '        Dim oElmt As XmlElement
    '        If moPageXml.SelectSingleNode("/Page/Request/QueryString/Item[@name='sortCol']") Is Nothing Then
    '            ' Add a default column sort
    '            oElmt = addNewTextNode("Item", moPageXml.SelectSingleNode("/Page/Request/QueryString"), nSortColumn, , False)
    '            oElmt.SetAttribute("name", "sortCol")
    '            oElmt = addNewTextNode("Item", moPageXml.SelectSingleNode("/Page/Request/QueryString"), SortDirectionVal(nSortDirection), , False)
    '            oElmt.SetAttribute("name", "sortDir")
    '        End If
    '    Catch ex As Exception
    '        returnException("stdTools", "SetDefaultSortColumn", ex, "", "", gbDebug)
    '    End Try
    'End Sub

    Public Class StringWriterWithEncoding
        Inherits IO.StringWriter

        Private _encoding As System.Text.Encoding

        Public Sub New(encoding As System.Text.Encoding)
            MyBase.New()
            _encoding = encoding
        End Sub

        Public Sub New(encoding As System.Text.Encoding, formatProvider As IFormatProvider)
            MyBase.New(formatProvider)
            _encoding = encoding
        End Sub

        Public Sub New(encoding As System.Text.Encoding, sb As System.Text.StringBuilder)
            MyBase.New(sb)
            _encoding = encoding
        End Sub

        Public Sub New(encoding As System.Text.Encoding, sb As System.Text.StringBuilder, formatProvider As IFormatProvider)
            MyBase.New(sb, formatProvider)
            _encoding = encoding
        End Sub

        Public Overrides ReadOnly Property Encoding As System.Text.Encoding
            Get
                Return _encoding
            End Get
        End Property

    End Class

#Region "Deprecated"

    <Obsolete("This method is deprecated, please use Protean.Tools.Text.IsEmail instead")> _
    Public Function is_valid_email(ByRef str_Renamed As String) As Boolean
        Return Tools.Text.IsEmail(str_Renamed)
    End Function

    <Obsolete("This method is deprecated, use Protean.Tools.Text.SimpleRegexFind instead")> _
    Public Function SimpleRegexFind(ByVal cSearchString As String, ByVal cRegexPattern As String, Optional ByVal nReturnGroup As Integer = 0, Optional ByVal oRegexOptions As RegexOptions = RegexOptions.None) As String
        Return Protean.Tools.Text.SimpleRegexFind(cSearchString, cRegexPattern, nReturnGroup, oRegexOptions)
    End Function

    <Obsolete("This method is deprecated, use Protean.Tools.FileHelper.GetMIMEType instead")> _
    Function GetMIMEType(ByVal Extension As String) As String
        Return Protean.Tools.FileHelper.GetMIMEType(Extension)
    End Function



#End Region


End Module

Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Configuration
Imports System.IO
Imports System.Collections
Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.Mail
Imports System.Net.Mail
Imports VB = Microsoft.VisualBasic
Imports System.Web.Configuration
Imports System.Text.RegularExpressions
Imports System
Imports PreMailer.Net

Public Class Messaging

    Public mbEncrypt As Boolean = False
    Public msGnuDirectory As String = "C:\GNU\GNUPG"
    Public msGnuOriginator As String = ""
    Public msGnuPassphrase As String = ""
    Public mbIsBodyHtml As Boolean = True

    Public moCtx As System.Web.HttpContext = System.Web.HttpContext.Current

    Public goApp As System.Web.HttpApplicationState = IIf(moCtx Is Nothing, Nothing, moCtx.Application)

    Public goRequest As System.Web.HttpRequest = moCtx.Request
    Public goResponse As System.Web.HttpResponse = moCtx.Response
    Public goSession As System.Web.SessionState.HttpSessionState = moCtx.Session
    Public goServer As System.Web.HttpServerUtility = moCtx.Server
    Public goConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/web")

    Public mcModuleName As String = "Protean.Messaging"
    Public sendAsync As Boolean = False

    Private msAttachmentPath As String = ""
    Private Attachments As Collection
    Private Shared mailSent As Boolean = False

    Private _language As String = ""

    Public Property Language() As String
        Get
            Return _language
        End Get
        Set(ByVal value As String)
            _language = value
        End Set
    End Property

    Public ReadOnly Property HasLanguage() As Boolean
        Get
            Return Not String.IsNullOrEmpty(_language)
        End Get
    End Property

    Public Sub addAttachment(ByVal contentStream As System.IO.Stream, ByVal name As String)
        Dim cProcessInfo As String = "emailCart"
        Try

            Dim oAtt As New Attachment(contentStream, name)
            If Attachments Is Nothing Then
                Attachments = New Collection
            End If
            Attachments.Add(oAtt)

        Catch ex As Exception
            If gbDebug Then
                returnException(mcModuleName, "addAttachment", ex, "", cProcessInfo, gbDebug)
            End If
        End Try
    End Sub
    Public Sub addAttachmentAsPdf(ByVal contentStream As System.IO.Stream, ByVal name As String)
        Dim cProcessInfo As String = "emailCart"
        Try
            contentStream.Position = 0

            Dim oAtt As New Attachment(contentStream, name, "application/pdf")
            If Attachments Is Nothing Then
                Attachments = New Collection
            End If
            Attachments.Add(oAtt)

        Catch ex As Exception
            If gbDebug Then
                returnException(mcModuleName, "addAttachment", ex, "", cProcessInfo, gbDebug)
            End If
        End Try
    End Sub

    Public Sub addAttachment(ByVal fileLocation As String, Optional ByVal deleteAfterAttach As Boolean = False)
        Dim cProcessInfo As String = "emailCart"
        Try
            If fileLocation <> "" Then
                'check if filesystem path allready supplied
                If Not fileLocation.Contains(":/") Then
                    fileLocation = goServer.MapPath("/") & fileLocation
                End If

                Dim oAtt As New Attachment(fileLocation)
                'rewrite the name
                oAtt.Name = System.IO.Path.GetFileName(fileLocation)

                If Attachments Is Nothing Then
                    Attachments = New Collection
                End If
                Attachments.Add(oAtt)

                'NB : 01-09-2009 Commented, See deleteAttachment
                ' As here only causes "File in Use" exceptions in the fsHelper
                'If deleteAfterAttach Then
                '    Dim fsh As Protean.fsHelper = New fsHelper
                '    fsh.DeleteFile(goServer.MapPath("") & fileLocation)
                'End If

            End If

        Catch ex As Exception
            If gbDebug Then
                returnException(mcModuleName, "addAttachment", ex, "", cProcessInfo, gbDebug)
            End If
        End Try
    End Sub

    Public Sub addAttachment(ByVal bytes() As Byte, ByVal name As String, ByVal contenttype As String)
        Try
            Dim contentStream As IO.Stream
            contentStream = New MemoryStream(bytes)
            Dim oAtt As New Attachment(contentStream, name, contenttype)
            If Attachments Is Nothing Then
                Attachments = New Collection
            End If
            Attachments.Add(oAtt)
        Catch ex As Exception
            If gbDebug Then
                returnException(mcModuleName, "addAttachment", ex, "", "", gbDebug)
            End If
        End Try
    End Sub


    Public Sub AddAttachmentsByIds(ByRef RootElmt As XmlElement, ByVal ids As String, ByVal xPath As String)
        Try
            Dim sSql As String
            Dim oDs As DataSet
            Dim dsRow As DataRow
            Dim strFilePath As String
            Dim strError As String = ""

            Dim dbAuth As String
            If goConfig("DatabasePassword") <> "" Then
                dbAuth = "user id=" & goConfig("DatabaseUsername") & "; password=" & goConfig("DatabasePassword")
            Else
                If goConfig("DatabaseAuth") <> "" Then
                    dbAuth = goConfig("DatabaseAuth")
                Else
                    dbAuth = "Integrated Security=SSPI;"
                End If
            End If

            Dim dbConn As String = "Data Source=" & goConfig("DatabaseServer") & "; " &
                                "Initial Catalog=" & goConfig("DatabaseName") & "; " &
                                dbAuth

            Dim moDbhelper As New Protean.Cms.dbHelper(dbConn, 1)
            moDbhelper.moPageXml = RootElmt.OwnerDocument

            If Not (String.IsNullOrEmpty(ids)) Then
                sSql = "select * from tblContent where nContentKey in (" & ids & ")"
                oDs = moDbhelper.GetDataSet(sSql, "Item")
                For Each dsRow In oDs.Tables("Item").Rows
                    RootElmt.AppendChild(moDbhelper.GetContentDetailXml(dsRow("nContentKey")))
                    strFilePath = moDbhelper.getContentFilePath(dsRow, xPath)
                    Try
                        Dim oAtt As New Attachment(strFilePath)
                        If Attachments Is Nothing Then
                            Attachments = New Collection
                        End If
                        Attachments.Add(oAtt)
                    Catch ex As Exception
                        strError += "Missing file:" & strFilePath & "<br/>"
                    End Try
                Next
                oDs = Nothing
            End If

        Catch ex As Exception
            If gbDebug Then
                returnException(mcModuleName, "addAttachment", ex, "", "", gbDebug)
            End If
        End Try
    End Sub

    Public Sub clearAttachments()
        Dim cProcessInfo As String = "emailCart"
        Try

            If Not Attachments Is Nothing Then
                Attachments.Clear()
            End If

        Catch ex As Exception
            If gbDebug Then
                returnException(mcModuleName, "addAttachment", ex, "", cProcessInfo, gbDebug)
            End If
        End Try
    End Sub

    Public Sub deleteAttachment(ByVal fileLocation As String)
        Dim cProcessInfo As String = "emailCart"
        Try
            If fileLocation <> "" Then

                If Not Attachments Is Nothing Then
                    Attachments.Clear()
                End If

                Dim fsh As Protean.fsHelper = New fsHelper
                fsh.DeleteFile(goServer.MapPath("/") & fileLocation)

            End If

        Catch ex As Exception
            If gbDebug Then
                returnException(mcModuleName, "addAttachment", ex, "", cProcessInfo, gbDebug)
            End If
        End Try
    End Sub

    Public Function emailer(
                                ByVal oBodyXML As XmlElement,
                                ByVal xsltPath As String,
                                ByVal fromName As String,
                                ByVal fromEmail As String,
                                ByVal recipientEmail As String,
                                ByVal SubjectLine As String,
                                Optional ByVal successMessage As String = "Message Sent",
                                Optional ByVal failureMessage As String = "Message Failed",
                                Optional ByVal recipientName As String = "",
                                Optional ByVal ccRecipient As String = "",
                                Optional ByVal bccRecipient As String = "",
                                Optional ByVal cSeperator As String = "",
                                Optional ByRef odbHelper As Protean.Cms.dbHelper = Nothing,
                                Optional ByVal cPickupHost As String = "",
                                Optional ByVal cPickupLocation As String = ""
                            ) As Object
        PerfMon.Log("Messaging", "emailer")
        If cSeperator Is Nothing Then
            cSeperator = ","
        ElseIf cSeperator = "" Then
            cSeperator = ","
        End If
        Dim styleFile As String
        Dim sWriter As IO.TextWriter = New IO.StringWriter
        Dim oXml As XmlDocument = New XmlDocument
        Dim cProcessInfo As String = "Emailer"
        Dim oTransform As New Protean.XmlHelper.Transform()

        ' User counts
        Dim nTotalAddressesAttempted As Integer = 0
        Dim nTotalAddressesSkipped As Integer = 0
        Dim cAddressesSkipped As String = ""

        Try
            'Dim nsmgr As XmlNamespaceManager = New XmlNamespaceManager(oBodyXML.OwnerDocument.NameTable)
            'Dim cNsURI As String = oBodyXML.NamespaceURI
            'nsmgr.AddNamespace("ews", cNsURI)
            '' sResponse = oSoapElmt.SelectSingleNode("ews:" & sActionName & "Result", nsmgr).InnerText

            Dim XmlString As String = oBodyXML.OuterXml
            XmlString = XmlString.Replace("xmlns=""http://www.eonic.co.uk/ewcommon/Services""", "")

            oXml.LoadXml(XmlString)

            oXml.DocumentElement.SetAttribute("subjectLine", SubjectLine)

            oXml.DocumentElement.SetAttribute("sessionReferrer", goRequest.Headers("SessionReferer"))
            oXml.DocumentElement.SetAttribute("websiteURL", goRequest.ServerVariables("HTTP_HOST"))

            Dim oAttIdsElmt As XmlElement = oXml.DocumentElement.SelectSingleNode("AttachmentIds")
            If Not oAttIdsElmt Is Nothing Then
                AddAttachmentsByIds(oAttIdsElmt, oAttIdsElmt.GetAttribute("ids"), oAttIdsElmt.GetAttribute("xpath"))
            End If
            'added to test plain text emails - remove on build
            'xsltPath = "/xsl/Email/vMessagingSMS.xsl"

            'Load the XSL from the passed filename string styleFileName
            cProcessInfo = xsltPath

            If xsltPath = "" Then
                styleFile = goServer.MapPath(goConfig("ProjectPath") & "/xsl/emailer.xsl")
            Else
                styleFile = goServer.MapPath(xsltPath)
            End If

            If Me.HasLanguage And oXml.DocumentElement IsNot Nothing Then
                oXml.DocumentElement.SetAttribute("translang", Me.Language)
            End If

            ' We run the transformation twice
            ' Once for HTML, and once for Plain Text
            Dim messageHtml As String = ""
            Dim messagePlainText As String = ""
            oTransform.XSLFile = styleFile
            oTransform.Compiled = False

            ' Transform for HTML
            oXml.DocumentElement.SetAttribute("output", "html")
            oTransform.Process(oXml, sWriter)
            If oTransform.HasError Then
                Throw New Exception("There was an error transforming the email (Output: HTML).")
            End If
            messageHtml = sWriter.ToString()
            sWriter.Close()
            'Call to Pre-Mailer to move css to inline style attributes.
            Dim hostUrl As String = goRequest.Url.Host
            Dim urlScheme As String = "http://"
            If goRequest.IsSecureConnection Then
                urlScheme = "https://"
            End If
            If Not hostUrl.StartsWith(urlScheme, StringComparison.OrdinalIgnoreCase) Then
                hostUrl = urlScheme + hostUrl
            End If
            Dim preMailerResult As InlineResult = PreMailer.Net.PreMailer.MoveCssInline(New Uri(hostUrl), messageHtml)
            messageHtml = preMailerResult.Html


            ' Transform for Plain Text
            sWriter = New StringWriter
            oXml.DocumentElement.SetAttribute("output", "plaintext")
            oTransform.Process(oXml, sWriter)
            If oTransform.HasError Then Throw New Exception("There was an error transforming the email (Output: Plain Text).")
            messagePlainText = sWriter.ToString()

            sWriter.Close()
            sWriter = Nothing

            'is there's no HTML, set is as plain text
            Dim nHtmlPos As Int32 = InStr(LCase(messagePlainText), "<html")
            If nHtmlPos <= 0 Then
                mbIsBodyHtml = False
            End If

            'lets get the subjectline form the html title
            Dim oEmailXmlDoc As XmlDocument = htmlToXmlDoc(messageHtml)
            If Not oEmailXmlDoc Is Nothing Then
                'override the subject line from the template.
                If Not oEmailXmlDoc.SelectSingleNode("html/head/title") Is Nothing Then
                    Dim oElmt2 As XmlElement = oEmailXmlDoc.SelectSingleNode("html/head/title")
                    If oElmt2.InnerText <> "" Then
                        SubjectLine = Trim(oElmt2.InnerText)
                    End If
                End If
            End If


            ' Check that we have a valid from address
            If Not (Tools.Text.IsEmail(fromEmail)) Then
                fromEmail = CStr(goConfig("SiteAdminEmail"))
                fromName = CStr(goConfig("SiteAdminName"))
            End If
            If Not (Tools.Text.IsEmail(fromEmail)) Then
                fromEmail = "webmaster@genericcms.com"
            End If
            If fromName = "" Then fromName = "Webmaster"

            Dim adFrom As New MailAddress(fromEmail, CStr(fromName))
            Dim oMailn As New Net.Mail.MailMessage()
            oMailn.From = adFrom
            oMailn.BodyEncoding = System.Text.Encoding.GetEncoding("utf-8")
            oMailn.IsBodyHtml = mbIsBodyHtml


            ' Implement the site sender
            If goConfig("DisableServerSenderEmail") <> "true" Then
                Dim serverSenderEmail As String = goConfig("ServerSenderEmail") & ""
                Dim serverSenderEmailName As String = goConfig("ServerSenderEmailName") & ""
                If Not (Tools.Text.IsEmail(serverSenderEmail)) Then
                    serverSenderEmail = "emailsender@protean.site"
                    serverSenderEmailName = "ProteanCMS Email Sender"
                End If

                Dim mailSender As New MailAddress(serverSenderEmail, serverSenderEmailName)


                If LCase(goConfig("overrideFromEmail")) = "on" Then
                    oMailn.From = mailSender
                Else
                    ' Don't add the sender if it's the same address as the from
                    If Not MailAddress.Equals(mailSender, adFrom) Then
                        oMailn.Sender = mailSender
                    End If
                End If
            End If

            ' All throught the process check if the e-mail address is actually valid.
            Dim cEmailOverride As String
            cEmailOverride = goConfig("EmailOverride")
            If cEmailOverride <> "" Then
                ' If override email has been specified in the global config then send all mails to the override e-amil address
                ' Useful for testing.
                nTotalAddressesAttempted = 1
                If Tools.Text.IsEmail(cEmailOverride) Then
                    oMailn.To.Add(New MailAddress(goConfig("EmailOverride"), recipientName))
                Else
                    failureMessage += ""
                    nTotalAddressesSkipped = 1
                    cAddressesSkipped = cEmailOverride
                End If
            Else

                If recipientEmail = "" Then
                    recipientEmail = goConfig("SiteAdminEmail")
                End If

                'using multiple addresses here
                If recipientEmail.Contains(cSeperator) Then
                    Dim oTos() As String = Split(recipientEmail, cSeperator)
                    Dim i As Integer
                    For i = 0 To oTos.Length - 1
                        nTotalAddressesAttempted += 1
                        If Tools.Text.IsEmail(oTos(i)) Then
                            oMailn.To.Add(oTos(i))
                        Else
                            nTotalAddressesSkipped += 1
                            cAddressesSkipped += ", " & oTos(i)
                        End If
                    Next
                Else
                    nTotalAddressesAttempted += 1
                    If Tools.Text.IsEmail(recipientEmail) Then
                        oMailn.To.Add(New MailAddress(recipientEmail, recipientName))
                    Else
                        nTotalAddressesSkipped += 1
                        cAddressesSkipped += ", " & recipientEmail
                    End If
                End If
                'cc
                If Not ccRecipient Is Nothing Then
                    If ccRecipient.Contains(cSeperator) Then
                        Dim oTos() As String = Split(ccRecipient, cSeperator)
                        Dim i As Integer

                        For i = 0 To oTos.Length - 1
                            nTotalAddressesAttempted += 1
                            If Tools.Text.IsEmail(oTos(i)) Then
                                oMailn.CC.Add(oTos(i))
                            Else
                                nTotalAddressesSkipped += 1
                                cAddressesSkipped += ", " & oTos(i)
                            End If
                        Next
                    ElseIf Not ccRecipient = "" Then
                        nTotalAddressesAttempted += 1
                        If Tools.Text.IsEmail(ccRecipient) Then
                            oMailn.CC.Add(ccRecipient)
                        Else
                            nTotalAddressesSkipped += 1
                            cAddressesSkipped += ", " & ccRecipient
                        End If
                    End If
                End If
                'bcc
                If Not bccRecipient Is Nothing Then
                    If bccRecipient.Contains(cSeperator) Then
                        Dim oTos() As String = Split(bccRecipient, cSeperator)
                        Dim i As Integer
                        For i = 0 To oTos.Length - 1
                            nTotalAddressesAttempted += 1
                            If Tools.Text.IsEmail(oTos(i)) Then
                                oMailn.Bcc.Add(oTos(i))
                            Else
                                nTotalAddressesSkipped += 1
                                cAddressesSkipped += ", " & oTos(i)
                            End If

                        Next
                    ElseIf Not bccRecipient = "" Then
                        nTotalAddressesAttempted += 1
                        If Tools.Text.IsEmail(bccRecipient) Then
                            oMailn.Bcc.Add(bccRecipient)
                        Else
                            nTotalAddressesSkipped += 1
                            cAddressesSkipped += ", " & bccRecipient
                        End If
                    End If
                End If
            End If

            ' Check if we need to send the e-mail
            If goConfig("MailServer") = "" Then
                Return "Mailserver Not Specified."
            ElseIf nTotalAddressesAttempted = nTotalAddressesSkipped Then
                ' If all the e-mails failed validation then don't bother sending them
                Select Case nTotalAddressesAttempted
                    Case 0 : Return failureMessage & ": No e-mail addresses were provided"
                    Case 1 : Return failureMessage & ": E-mail address provided is invalid"
                    Case Else : Return failureMessage & ": E-mail addresses provided are invalid"
                End Select
            Else

                ' Override the HTML setting if BodyHtml is explicitly being set
                If (oMailn.IsBodyHtml = False) Or mbEncrypt Then

                    If String.IsNullOrEmpty(messagePlainText) Then
                        messagePlainText = messageHtml
                    End If
                    ' messageHtml = ""

                End If

                cProcessInfo = "Encrypt the Email"
                If mbEncrypt = True Then
                    ' encrypt the email using GNU

                    Dim oGnuPg As Protean.Tools.GnuPG.GnuPGWrapper = New Tools.GnuPG.GnuPGWrapper

                    oGnuPg.homedirectory = msGnuDirectory
                    oGnuPg.originator = msGnuOriginator
                    oGnuPg.passphrase = msGnuPassphrase
                    oGnuPg.recipient = recipientEmail
                    oGnuPg.command = Tools.GnuPG.Commands.SignAndEncrypt
                    oGnuPg.ExecuteCommand(messagePlainText, messagePlainText)
                End If

                ' Transform and output to page
                cProcessInfo = "create ewMailer Component"

                'lets just tidy up the subject line
                If String.IsNullOrEmpty(SubjectLine) Then SubjectLine = ""
                SubjectLine = Regex.Replace(SubjectLine, "[\r\f\n\t\f]", "").Trim
                cProcessInfo = "Subject:" & SubjectLine
                oMailn.Subject = SubjectLine

                ' Set the message body
                oMailn.IsBodyHtml = mbIsBodyHtml

                'mbIsBodyHtml = False ' added for testing. remove for deployment.

                If mbIsBodyHtml = False Then
                    ' Because we're processing the same XSLs for both plain text and html we can't tell
                    ' the xsl:output element to be conditional and do things like output text and omit docTypes
                    ' so a little manual tidy up is needed
                    messagePlainText = Regex.Replace(messagePlainText, "<!DOCTYPE[^>]+?>", "", RegexOptions.IgnoreCase)
                    oMailn.Body = messagePlainText
                    oMailn.Headers.Set("Content-Type", "text/plain")
                    'moCtx.Response.ContentType = "text/plain"

                    If InStr(LCase(messageHtml), "<html") > 0 Then
                        Dim htmlView As AlternateView = AlternateView.CreateAlternateViewFromString(messageHtml, New System.Net.Mime.ContentType("text/html; charset=UTF-8"))
                        oMailn.AlternateViews.Add(htmlView)
                    End If

                Else
                    oMailn.Body = messagePlainText
                    Dim htmlView As AlternateView = AlternateView.CreateAlternateViewFromString(messageHtml, New System.Net.Mime.ContentType("text/html; charset=UTF-8"))
                    oMailn.AlternateViews.Add(htmlView)

                End If

                'ok lets add an attachment if we need too
                If Not Attachments Is Nothing Then

                    Dim oAtt As Attachment
                    For Each oAtt In Attachments
                        'reset to begining if allready used
                        oMailn.Attachments.Add(oAtt)
                    Next

                End If

                Dim oSmtpn As New SmtpClient

                cProcessInfo = "Mailserver Failed:" & goConfig("MailServer")


                ' Work out whether to send this to a queued server
                If cPickupHost <> "" And cPickupLocation <> "" Then
                    ' Send queued
                    If SendQueuedMail(oMailn, cPickupHost, cPickupLocation) = "Error" Then
                        Return failureMessage & " - Queuing message failed"
                    End If
                Else
                    ' Send direct
                    Try
                        oSmtpn.Host = goConfig("MailServer")
                        If goConfig("MailServerPort") <> "" Then
                            oSmtpn.Port = goConfig("MailServerPort")
                        End If
                        If goConfig("MailServerUsername") <> "" Then
                            oSmtpn.UseDefaultCredentials = False
                            oSmtpn.Credentials = New System.Net.NetworkCredential(goConfig("MailServerUsername"), goConfig("MailServerPassword").Replace("&lt;", "<").Replace("&gt;", "<"), goConfig("MailServerUsernameDomain"))
                            'oSmtpn.Credentials = New System.Net.NetworkCredential(goConfig("MailServerUsername"), goConfig("MailServerPassword"))
                        End If
                        If LCase(goConfig("MailServerSSL")) = "on" Then
                            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
                            oSmtpn.EnableSsl = True
                            oSmtpn.DeliveryMethod = SmtpDeliveryMethod.Network
                        End If
                        If LCase(goConfig("MailServerSSL")) = "off" Then
                            oSmtpn.EnableSsl = False
                        End If
                        If sendAsync Then
                            ' Set the method that is called back when the send operation ends. 
                            AddHandler oSmtpn.SendCompleted, AddressOf SendCompletedCallback
                            ' The userState can be any object that allows your callback  
                            ' method to identify this send operation. 
                            ' For this example, the userToken is a string constant. 
                            Dim userState As String = SubjectLine
                            oSmtpn.SendAsync(oMailn, userState)

                        Else
                            Try
                                oSmtpn.Send(oMailn)
                            Catch ex As Exception
                                If goConfig("MailServer2") = "" Then
                                    If gbDebug Then
                                        returnException(mcModuleName, "emailer", ex, "", cProcessInfo, gbDebug)
                                        Return "ex: " & ex.ToString
                                    Else
                                        Return failureMessage & " - Error1: " & ex.Message & " - " & cProcessInfo & " - " & ex.StackTrace
                                    End If
                                Else
                                    Try
                                        oSmtpn.Host = goConfig("MailServer2")
                                        oSmtpn.Send(oMailn)
                                    Catch ex3 As Exception
                                        If gbDebug Then
                                            returnException(mcModuleName, "emailer", ex3, "", cProcessInfo, gbDebug)
                                            Return "ex3: " & ex3.ToString
                                        Else
                                            Return failureMessage & " - Error1: " & ex3.Message & " - " & cProcessInfo & " - " & ex.StackTrace
                                        End If

                                    End Try
                                End If

                            End Try
                        End If
                    Catch ex As Exception
                        Try
                            oSmtpn.Host.Insert(0, goConfig("MailServer"))
                            Try
                                oSmtpn.Send(oMailn)
                            Catch ex2 As Exception
                                If goConfig("MailServer2") <> "" Then
                                    oSmtpn.Host.Insert(0, goConfig("MailServer2"))
                                    oSmtpn.Send(oMailn)
                                End If
                            End Try
                        Catch ex2 As Exception
                            If gbDebug Then
                                returnException(mcModuleName, "emailer", ex2, "", cProcessInfo, gbDebug)
                                Return "ex2: " & ex2.ToString
                            Else
                                Return failureMessage & " - Error1: " & ex2.Message & " - " & cProcessInfo & " - " & ex.StackTrace
                            End If
                        End Try
                    End Try
                End If

                If LCase(goConfig("LogEmail")) = "on" Then
                    Try
                        Dim cActivityDetail As String = ""
                        Try
                            Dim oBodyElmt As XmlElement = oEmailXmlDoc.SelectSingleNode("html/body")
                            If oBodyElmt.InnerText <> "" Then
                                cActivityDetail = oBodyElmt.InnerText
                            Else
                                cActivityDetail = oMailn.Body
                            End If
                        Catch ex As Exception
                            cActivityDetail = oMailn.Body
                        End Try
                        ' Trim out multiple whitespace - saves space
                        Dim oRE As New Regex("(\s)\s+") ' Pattern looks for a whitespace character followed by one or more whitespace chars
                        cActivityDetail = Trim(oRE.Replace(cActivityDetail, " ")) ' Replaces the whole lot with a space

                        Dim mnUserId As Integer = 0
                        If Not goSession Is Nothing Then mnUserId = goSession("mnUserId")
                        If odbHelper Is Nothing Then
                            Dim cCon As String = "Data Source=" & goConfig("DatabaseServer") & "; Initial Catalog=" & goConfig("DatabaseName") & ";"
                            If goConfig("DatabasePassword") <> "" Then
                                cCon &= "user id=" & goConfig("DatabaseUsername") & "; password=" & goConfig("DatabasePassword")
                            Else
                                cCon &= goConfig("DatabaseAuth") & "; "
                            End If
                            odbHelper = New Protean.Cms.dbHelper(cCon, mnUserId)
                        End If

                        Dim SessionId As String = Nothing
                        If Not goSession Is Nothing Then
                            SessionId = goSession.SessionID
                        End If

                        If odbHelper.checkTableColumnExists("tblEmailActivityLog", "cActivityXml") And LCase(goConfig("LogEmailXml")) <> "off" Then
                            Dim activitySchema As String = "Default"
                            If oBodyXML.GetAttribute("id") = "" Then
                                activitySchema = oBodyXML.Name
                            Else
                                activitySchema = oBodyXML.GetAttribute("id")
                            End If
                            Dim logId As Long = odbHelper.emailActivity(mnUserId, cActivityDetail, oMailn.To.ToString, oMailn.From.ToString, oBodyXML.OuterXml)
                            odbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.Email, mnUserId, SessionId, Now, logId, 0, activitySchema)
                        Else
                            odbHelper.emailActivity(mnUserId, cActivityDetail, oMailn.To.ToString, oMailn.From.ToString)
                            odbHelper.CommitLogToDB(Cms.dbHelper.ActivityType.Email, mnUserId, SessionId, Now, 0, 0, "")
                        End If

                    Catch ex As Exception
                        'Dont do anything
                    End Try
                End If

                ' Check if any e-mails were skipped.
                If nTotalAddressesSkipped > 0 Then
                    successMessage += " (" & nTotalAddressesSkipped & " out of " & nTotalAddressesAttempted & " e-mails were not sent, because the e-mail addresses were invalid. "
                    If gbDebug Then successMessage += "Skipped: " & cAddressesSkipped.TrimStart(",")
                    successMessage += ")"
                End If
                If Not sendAsync Then
                    oMailn.Dispose()
                End If

                'handle opt-in behaviour
                If Not oXml.SelectSingleNode("descendant-or-self::optIn[node()='true']") Is Nothing Then

                    Dim moMessaging As Protean.Providers.Messaging.BaseProvider
                    Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")
                    Dim sMessagingProvider As String = ""
                    If Not moMailConfig Is Nothing Then
                        sMessagingProvider = moMailConfig("MessagingProvider")
                        Dim myWeb As New Protean.Cms(moCtx)
                        moMessaging = New Protean.Providers.Messaging.BaseProvider(myWeb, sMessagingProvider)
                        Try

                            Dim email As String = oXml.SelectSingleNode("descendant-or-self::Email").InnerText
                            Dim name As String = oXml.SelectSingleNode("descendant-or-self::Name").InnerText
                            Dim values As New System.Collections.Generic.Dictionary(Of String, String)
                            Dim oElmt As XmlElement
                            For Each oElmt In oXml.SelectNodes("/*/*")
                                If Not (oElmt.Name = "Name" Or oElmt.Name = "Email" Or oElmt.Name = "ListId") Then
                                    values.Add(oElmt.Name, oElmt.InnerText)
                                End If
                            Next
                            moMessaging.Activities.AddToList(moMailConfig("OptInList"), name, email, values)

                        Catch ex As Exception
                            cProcessInfo = ex.StackTrace
                        End Try
                    End If

                End If

                Return successMessage

            End If

        Catch ex As Exception
            If gbDebug Then
                returnException(mcModuleName, "emailer", ex, "", cProcessInfo, gbDebug)
                Return ex.ToString
            Else
                Return failureMessage & " - Error2: " & ex.Message & " - " & cProcessInfo & " - " & ex.StackTrace
            End If
        End Try

    End Function


    Private Shared Sub SendCompletedCallback(ByVal sender As Object, ByVal e As System.ComponentModel.AsyncCompletedEventArgs)
        ' Get the unique identifier for this asynchronous operation. 
        Dim token As String = CStr(e.UserState)

        If e.Cancelled Then
            'Console.WriteLine("[{0}] Send canceled.", token)
        End If
        If e.Error IsNot Nothing Then
            returnException("messaging", "SendCompletedCallback", e.Error, "", "[{0}] Send Error.", gbDebug)
            'Console.WriteLine("[{0}] {1}", token, e.Error.ToString())
        Else
            sender.Dispose()
            ' Console.WriteLine("Message sent.")
        End If
        mailSent = True
    End Sub


    'Public Function emailerPlainText( _
    '                        ByVal oBodyXML As XmlElement, _
    '                        ByVal xsltPath As String, _
    '                        ByVal fromName As String, _
    '                        ByVal fromEmail As String, _
    '                        ByVal recipientEmail As String, _
    '                        ByVal SubjectLine As String, _
    '                        Optional ByVal successMessage As String = "Message Sent", _
    '                        Optional ByVal failureMessage As String = "Message Failed", _
    '                        Optional ByVal recipientName As String = "", _
    '                        Optional ByVal ccRecipient As String = "", _
    '                        Optional ByVal bccRecipient As String = "", _
    '                        Optional ByVal cSeperator As String = "", _
    '                        Optional ByRef odbHelper As Protean.Cms.dbHelper = Nothing, _
    '                        Optional ByVal cPickupHost As String = "", _
    '                        Optional ByVal cPickupLocation As String = "" _
    '                    ) As Object

    '    Dim cProcessInfo As String = "emailerPlainText"

    '    Try

    '        mbIsBodyHtml = False
    '        Dim cMessage As Object = emailerPlainText(oBodyXML, xsltPath, fromName, fromEmail, recipientEmail, SubjectLine, successMessage, failureMessage, recipientName, ccRecipient, bccRecipient, cSeperator, odbHelper, cPickupHost, cPickupLocation)

    '        mbIsBodyHtml = True

    '        Return cMessage


    '    Catch ex As Exception
    '        If gbDebug Then
    '            returnException(mcModuleName, "emailer", ex, "", cProcessInfo, gbDebug)
    '            Return ex.ToString
    '        Else
    '            Return failureMessage & " - Error: " & ex.Message & " - " & cProcessInfo
    '        End If
    '    End Try


    'End Function

    Public Function emailerMultiUsers(
                            ByVal oBodyXML As XmlElement,
                            ByVal xsltPath As String,
                            ByVal fromName As String,
                            ByVal fromEmail As String,
                            ByVal recipientIds As String,
                            ByVal SubjectLine As String,
                            Optional ByVal successMessage As String = "Message Sent",
                            Optional ByVal failureMessage As String = "Message Failed",
                            Optional ByVal cPickupHost As String = "",
                            Optional ByVal cPickupLocation As String = ""
                            ) As Object

        PerfMon.Log("Messaging", "emailerMultiUsers")

        Dim cProcessInfo As String = "emailerMultiUsers Initialise"

        Dim cRecipientId As String = ""
        Dim cRecipientName As String = ""
        Dim cRecipientEmail As String = ""

        Dim cResult As String = ""

        Try

            ' First get Host and Location from Web.config, only if not passed in already
            If cPickupHost = "" Then
                cPickupHost = goConfig("SMPTHost") ' Typo - included for backwards compatibility
                If String.IsNullOrEmpty(cPickupHost) Then cPickupHost = goConfig("SMTPHost")
                If String.IsNullOrEmpty(cPickupHost) Then cPickupHost = goConfig("PickupHost")
            End If
            If cPickupLocation = "" Then
                cPickupLocation = goConfig("SMPTLocation") ' Typo - included for backwards compatibility
                If String.IsNullOrEmpty(cPickupLocation) Then cPickupLocation = goConfig("SMTPLocation")
                If String.IsNullOrEmpty(cPickupLocation) Then cPickupLocation = goConfig("PickupLocation")
            End If

            Dim recipientIdsSplit() As String = Split(recipientIds, ",")

            ' Are these both needed?
            If (recipientIdsSplit) IsNot Nothing Then
                If recipientIdsSplit.GetValue(0) <> "" Then ' Empty Check


                    cProcessInfo = "IDs Detected, Initialising DBHelper"

                    ' Get a Database Helper
                    Dim odbHelper As Protean.Cms.dbHelper = Nothing
                    Dim mnUserId As Integer = 0

                    If Not goSession Is Nothing Then mnUserId = goSession("mnUserId")
                    If odbHelper Is Nothing Then
                        Dim cCon As String = "Data Source=" & goConfig("DatabaseServer") & "; Initial Catalog=" & goConfig("DatabaseName") & ";"
                        If goConfig("DatabasePassword") <> "" Then
                            cCon &= "user id=" & goConfig("DatabaseUsername") & "; password=" & goConfig("DatabasePassword")
                        Else
                            cCon &= goConfig("DatabaseAuth") & "; "
                        End If
                        odbHelper = New Protean.Cms.dbHelper(cCon, mnUserId)
                        cProcessInfo = "DBHelper Started"
                    End If

                    Dim iCounter As Integer = 0
                    Dim iSuccess As Integer = 0
                    ' Results populating by given Year(s)
                    For Each cRecipientId In recipientIdsSplit

                        iCounter = iCounter + 1
                        cProcessInfo = "Processing ID to Email for " + cRecipientId.ToString

                        Dim oDr As SqlClient.SqlDataReader
                        Dim cSQL As String = ""

                        cSQL = "SELECT * FROM tblDirectory INNER JOIN tblContentStructure ON tblDirectory.cDirName = tblContentStructure.cStructForiegnRef WHERE tblContentStructure.cStructName = 'Student_" & cRecipientId & "'"

                        oDr = odbHelper.getDataReader(cSQL)

                        While oDr.Read()
                            Dim oResultsDoc As XmlDocument = New XmlDocument
                            Dim oResults As XmlElement = oResultsDoc.CreateElement("Results")
                            oResults.InnerXml = oDr("cDirXml")

                            If oResults IsNot Nothing Then
                                cRecipientName = oResults.SelectSingleNode("User/FirstName").InnerText.ToString & " " & oResults.SelectSingleNode("User/LastName").InnerText.ToString
                                cRecipientEmail = oResults.SelectSingleNode("User/Email").InnerText.ToString
                            End If
                        End While


                        Dim cEmailResult As String = ""

                        If cRecipientEmail <> "" Then
                            cProcessInfo = "Sending Email for #" + iCounter.ToString
                            cEmailResult = emailer(oBodyXML, xsltPath, fromName, fromEmail, cRecipientEmail, SubjectLine,
                            , , cRecipientName, , , , , cPickupHost, cPickupLocation).ToString
                        Else
                            cEmailResult = failureMessage & ": E-mail address provided is invalid"
                        End If

                        ' Append to Final Message
                        cResult = cResult & "<email id=" & Chr(34) & cRecipientId.ToString & Chr(34) & ">" & cEmailResult & "</email>"

                        If cEmailResult.Equals(successMessage) Then
                            iSuccess = iSuccess + 1
                        End If

                        'reset
                        cRecipientName = ""
                        cRecipientEmail = ""
                        cSQL = ""
                    Next
                    Dim iFail As Integer = iCounter - iSuccess
                    cResult = "<emails count=" & Chr(34) & iCounter & Chr(34) & " success=" & Chr(34) & iSuccess & Chr(34) & " fail=" & Chr(34) & iFail & Chr(34) & " >" & cResult & "</emails>"
                End If
            End If

        Catch ex As Exception
            returnException(mcModuleName, "emailerMultiUsers", ex, "", cProcessInfo)
            Return ex.ToString
        End Try

        If cResult = "" Then
            cResult = "No Emails Processed"
        End If
        Return cResult

    End Function
    Public Function emailerWithXmlAttachment(
                                    ByVal oBodyXML As XmlElement,
                                    ByVal xsltPath As String,
                                    ByVal fromName As String,
                                    ByVal fromEmail As String,
                                    ByVal recipientEmail As String,
                                    ByVal SubjectLine As String,
                                    ByVal XSLPath As String,
                                    ByVal XSLType As String,
                                    ByVal emailAttachName As String,
                                    Optional ByVal successMessage As String = "Message Sent",
                                    Optional ByVal failureMessage As String = "Message Failed",
                                    Optional ByVal recipientName As String = "",
                                    Optional ByVal ccRecipient As String = "",
                                    Optional ByVal bccRecipient As String = "",
                                    Optional ByVal cSeperator As String = "",
                                    Optional ByRef odbHelper As Protean.Cms.dbHelper = Nothing,
                                    Optional ByVal cPickupHost As String = "",
                                    Optional ByVal cPickupLocation As String = ""
                                ) As Object

        PerfMon.Log("Messaging", "emailerWithXmlAttachment")
        Dim cProcessInfo As String = "emailerWithXmlAttachment Initialise"

        Dim oXml As XmlDocument = New XmlDocument
        Dim cResults As String = ""

        ' 22-Apr-09 Added to handle specific email attachment names
        Dim attachName As String = emailAttachName
        If attachName = "" Then
            attachName = "attached"
        End If
        '

        Try
            PerfMon.Log("Messaging", "emailerWithXmlAttachment - Get Xml")
            oXml.LoadXml(oBodyXML.OuterXml)

            If Not oXml Is Nothing Then
                If XSLType.ToLower = "xml" Then

                    PerfMon.Log("Messaging", "emailerWithXmlAttachment - Create Save Path")

                    Dim cSourceXmlPath As String = CStr(goConfig("XmlAttachmentPath"))
                    Dim cXmlPath As String = ""

                    If cSourceXmlPath Is Nothing Then
                        cXmlPath = goServer.MapPath("/") & "..\imports\" & attachName & ".xml"
                    Else
                        cXmlPath = goServer.MapPath("/") & cSourceXmlPath & attachName & ".xml"
                    End If

                    '''''Dim cXmlPath As String = goServer.MapPath("") & "..\..\imports\" & attachName & ".xml"

                    'Dim cXmlPath As String = goServer.MapPath("../") & "..\imports\Email.xml"

                    If cXmlPath <> "" Then
                        PerfMon.Log("Messaging", "emailerWithXmlAttachment - Save Xml File")

                        If XSLPath <> "" Then
                            Dim sWriter As IO.TextWriter = New IO.StringWriter
                            Dim sMessage As String
                            Dim oTransform As New Protean.XmlHelper.Transform()

                            cProcessInfo = xsltPath

                            oTransform.XSLFile = CStr(goServer.MapPath(XSLPath))
                            oTransform.Compiled = False
                            oTransform.Process(oXml, sWriter)
                            If oTransform.HasError Then Throw New Exception("There was an error transforming the email.")

                            sMessage = sWriter.ToString()

                            sWriter.Close()
                            sWriter = Nothing

                            oXml = htmlToXmlDoc(sMessage)
                        End If

                        Dim oXmlRemoveHeader As XmlNode = oXml.FirstChild
                        oXml.RemoveChild(oXmlRemoveHeader)

                        Dim oXmlDec As XmlDeclaration
                        oXmlDec = oXml.CreateXmlDeclaration("1.0", Nothing, Nothing)
                        oXmlDec.Encoding = "UTF-8"

                        Dim oXmlRoot As XmlElement = oXml.DocumentElement
                        oXml.InsertBefore(oXmlDec, oXmlRoot)

                        oXml.Save(cXmlPath)

                        PerfMon.Log("Messaging", "emailerWithXmlAttachment - Add Attachment")
                        Dim oAtt As New Attachment(cXmlPath)
                        If Attachments Is Nothing Then
                            Attachments = New Collection
                        End If
                        Attachments.Add(oAtt)
                        PerfMon.Log("Messaging", "emailerWithXmlAttachment - Delete Xml File")
                        Dim fsh As Protean.fsHelper = New fsHelper
                        fsh.DeleteFile(cXmlPath)
                    End If

                    PerfMon.Log("Messaging", "emailerWithXmlAttachment - Call Emailer")
                    cResults = emailer(
                                    oBodyXML,
                                    xsltPath,
                                    fromName,
                                    fromEmail,
                                    recipientEmail,
                                    SubjectLine,
                                    successMessage,
                                    failureMessage,
                                    recipientName,
                                    ccRecipient,
                                    bccRecipient,
                                    cSeperator,
                                    odbHelper,
                                    cPickupHost,
                                    cPickupLocation).ToString
                End If
            Else
                cResults = "Mailform failed to load"
            End If


            Return cResults

        Catch ex As Exception
            returnException(mcModuleName, "emailerWithXmlAttachment", ex, "", cProcessInfo)
            Return "Error: " & ex.ToString
        End Try

    End Function
    Public Function emailerPostToHTMLForm(ByVal oFormXML As XmlElement) As Object
        'Name?!!?

        Dim cProcessInfo As String = "PostToHTMLForm Initialise"
        Dim cResponse As String = ""

        Try
            If Not oFormXML.Attributes.GetNamedItem("action") Is Nothing Then

                Dim cPostTo As String = oFormXML.Attributes.GetNamedItem("action").Value.ToString
                Dim cPostString As String = ""
                Dim oPostNode As XmlNode

                For Each oPostNode In oFormXML.GetElementsByTagName("Post")
                    If Not oPostNode.Attributes.GetNamedItem("name").Value Is Nothing Then
                        cPostString = cPostString & System.Web.HttpUtility.UrlEncode("" & oPostNode.Attributes.GetNamedItem("name").Value.ToString) & "=" & System.Web.HttpUtility.UrlEncode("" & oPostNode.InnerText) & "&"
                    End If
                    'what happens to IDs?
                Next

                If cPostString.Length > 1 Then
                    cPostString = cPostString.Substring(0, cPostString.Length - 1)
                    'cPostString = System.Web.HttpUtility.UrlEncode(cPostString)
                End If

                Dim httpRequest As New Protean.Tools.Http.WebRequest("application/x-www-form-urlencoded")
                httpRequest.IncludeResponse = False
                cResponse = httpRequest.Send(cPostTo, cPostString)
            Else
                cResponse = "Error: No Post To Path Defined"
            End If

            cResponse = "Message Sent"
            Return cResponse

        Catch ex As Exception
            returnException(mcModuleName, "PostToHTMLForm", ex, "", cProcessInfo)
            Return "Error: " & ex.ToString
        End Try
    End Function


    Public Function SendQueuedMail(ByVal oMailn As Net.Mail.MailMessage, ByVal cHost As String, ByVal cPickupLocation As String) As String
        PerfMon.Log("Messaging", "SendQueuedMail")
        Try
            If oMailn Is Nothing Then Return "No Email Supplied"
            Dim oSmtpn As New SmtpClient
            oSmtpn.Host = cHost '"127.0.0.1"
            oSmtpn.PickupDirectoryLocation = cPickupLocation '"C:\Inetpub\mailroot\Pickup"
            oSmtpn.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory
            '=#=#=#=#=#=#=#=#=#=#=#=#=#==#=#=#=
            'Exit Function
            '=#=#=#=#=#=#=#=#=#=#=#=#=#==#=#=#=
            oSmtpn.Send(oMailn)
            Return "Sent"
        Catch ex As Exception
            returnException(mcModuleName, "SendQueuedMail", ex, "", "", gbDebug)
            Return "Error"
        End Try
    End Function

    Function SendMailToList_Queued(ByVal nPageId As Integer, ByVal cEmailXSL As String, ByVal cGroups As String, ByVal cFromEmail As String, ByVal cFromName As String, ByVal cSubject As String) As Boolean
        PerfMon.Log("Messaging", "SendMailToList_Queued")

        Dim cProcessInfo As String = ""

        Try
            Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")
            Dim oAddressDic As UserEmailDictionary = GetGroupEmails(cGroups)
            'max number of bcc


            Dim oWeb As New Cms
            oWeb.InitializeVariables()
            oWeb.Open()
            oWeb.mnPageId = nPageId
            oWeb.mbAdminMode = False

            oWeb.mcEwSiteXsl = cEmailXSL
            'get body
            oWeb.mnMailMenuId = moMailConfig("RootPageId")
            Dim oEmailBody As String = oWeb.ReturnPageHTML(oWeb.mnPageId)

            Dim i2 As Integer

            Dim oEmail As Net.Mail.MailMessage = Nothing
            Dim cRepientMail As String

            cFromEmail = cFromEmail.Trim()

            If Protean.Tools.Text.IsEmail(cFromEmail) Then
                For Each cRepientMail In oAddressDic.Keys
                    'create the message
                    If oEmail Is Nothing Then
                        oEmail = New Net.Mail.MailMessage
                        oEmail.IsBodyHtml = True
                        cProcessInfo = "Sending from: " & cFromEmail
                        oEmail.From = New Net.Mail.MailAddress(cFromEmail, cFromName)
                        oEmail.Body = oEmailBody
                        oEmail.Subject = cSubject
                    End If
                    'if we are not at the bcc limit then we add the addres
                    If i2 < CInt(moMailConfig("BCCLimit")) Then
                        If Protean.Tools.Text.IsEmail(cRepientMail.Trim()) Then
                            cProcessInfo = "Sending to: " & cRepientMail.Trim()
                            oEmail.Bcc.Add(New Net.Mail.MailAddress(cRepientMail.Trim()))
                            i2 += 1
                        End If
                    Else
                        'otherwise we send it
                        cProcessInfo = "Sending queued mail"
                        SendQueuedMail(oEmail, moMailConfig("PickupHost"), moMailConfig("PickupLocation"))
                        'and reset the counter
                        i2 = 0
                        oEmail = Nothing
                    End If
                Next
                'try a send after in case we havent reached the last send
                If i2 < CInt(moMailConfig("BCCLimit")) Then
                    cProcessInfo = "Sending queued mail (last)"
                    SendQueuedMail(oEmail, moMailConfig("PickupHost"), moMailConfig("PickupLocation"))
                    'and reset the counter
                    i2 = 0
                    oEmail = Nothing
                End If
                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            returnException(mcModuleName, "SendMailToList_Queued", ex, "", cProcessInfo, gbDebug)
            Return False
        End Try
    End Function

    Function SendSingleMail_Queued(ByVal nPageId As Integer, ByVal cEmailXSL As String, ByVal cRepientMail As String, ByVal cFromEmail As String, ByVal cFromName As String, ByVal cSubject As String) As Boolean
        PerfMon.Log("Messaging", "SendSingleMail_Queued")
        Try
            Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")


            Dim oWeb As New Cms
            oWeb.InitializeVariables()
            oWeb.Open()
            oWeb.mnPageId = nPageId
            oWeb.mbAdminMode = False

            oWeb.mcEwSiteXsl = cEmailXSL
            'get body
            oWeb.mnMailMenuId = moMailConfig("RootPageId")

            If Protean.Tools.Text.IsEmail(cFromEmail.Trim()) And Protean.Tools.Text.IsEmail(cRepientMail.Trim()) Then
                Dim sEmailBody As String = oWeb.ReturnPageHTML(oWeb.mnPageId)

                'Lets get the title and override the one provided
                Dim oXml As New XmlDocument

                oXml = htmlToXmlDoc(sEmailBody)

                If Not oXml Is Nothing Then
                    'override the subject line from the template.
                    If Not oXml.SelectSingleNode("html/head/title") Is Nothing Then
                        Dim oElmt2 As XmlElement = oXml.SelectSingleNode("html/head/title")
                        If oElmt2.InnerText <> "" Then
                            cSubject = Trim(oElmt2.InnerText)
                        End If
                    End If
                End If
                oXml = Nothing

                Dim oEmail As Net.Mail.MailMessage

                oEmail = New Net.Mail.MailMessage
                oEmail.IsBodyHtml = True
                oEmail.From = New Net.Mail.MailAddress(cFromEmail.Trim(), cFromName)
                oEmail.Body = sEmailBody
                oEmail.To.Add(New Net.Mail.MailAddress(cRepientMail.Trim()))
                oEmail.Subject = cSubject


                'otherwise we send it
                SendQueuedMail(oEmail, moMailConfig("PickupHost"), moMailConfig("PickupLocation"))
                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            returnException(mcModuleName, "SendSingleMail_Queued", ex, "", "", gbDebug)
            Return False
        End Try
    End Function


    Function SendSingleMail_Direct(ByVal nPageId As Integer, ByVal cEmailXSL As String, ByVal cRepientMail As String, ByVal cFromEmail As String, ByVal cFromName As String, ByVal cSubject As String) As Boolean
        PerfMon.Log("Messaging", "SendSingleMail_Queued")
        Try
            Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")


            Dim oWeb As New Cms
            oWeb.InitializeVariables()
            oWeb.Open()
            oWeb.mnPageId = nPageId
            oWeb.mbAdminMode = False

            oWeb.mcEwSiteXsl = cEmailXSL
            'get body
            oWeb.mnMailMenuId = moMailConfig("RootPageId")

            If Protean.Tools.Text.IsEmail(cFromEmail.Trim()) And Protean.Tools.Text.IsEmail(cRepientMail.Trim()) Then
                Dim sEmailBody As String = oWeb.ReturnPageHTML(oWeb.mnPageId)

                'Lets get the title and override the one provided
                Dim oXml As New XmlDocument

                oXml = htmlToXmlDoc(sEmailBody)

                If Not oXml Is Nothing Then
                    'override the subject line from the template.
                    If Not oXml.SelectSingleNode("html/head/title") Is Nothing Then
                        Dim oElmt2 As XmlElement = oXml.SelectSingleNode("html/head/title")
                        If oElmt2.InnerText <> "" Then
                            cSubject = Trim(oElmt2.InnerText)
                        End If
                    End If
                End If
                oXml = Nothing

                Dim oSmtpn As New SmtpClient

                oSmtpn.Host = goConfig("MailServer")
                If goConfig("MailServerPort") <> "" Then
                    oSmtpn.Port = goConfig("MailServerPort")
                End If

                If goConfig("MailServerUsername") <> "" Then
                    oSmtpn.UseDefaultCredentials = False
                    oSmtpn.Credentials = New System.Net.NetworkCredential(goConfig("MailServerUsername"), goConfig("MailServerPassword").Replace("&lt;", "<").Replace("&gt;", "<"), goConfig("MailServerUsernameDomain"))
                End If
                If LCase(goConfig("MailServerSSL")) = "on" Then
                    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12
                    oSmtpn.EnableSsl = True
                    oSmtpn.DeliveryMethod = SmtpDeliveryMethod.Network
                End If

                Dim oEmail As Net.Mail.MailMessage

                oEmail = New Net.Mail.MailMessage
                oEmail.IsBodyHtml = True

                If LCase(goConfig("overrideFromEmail")) = "on" Then
                    oEmail.From = New Net.Mail.MailAddress(goConfig("ServerSenderEmail"), cFromName)
                Else
                    oEmail.From = New Net.Mail.MailAddress(cFromEmail.Trim(), cFromName)
                End If

                oEmail.Body = sEmailBody
                oEmail.To.Add(New Net.Mail.MailAddress(cRepientMail.Trim()))
                oEmail.Subject = cSubject

                oSmtpn.Send(oEmail)

                'otherwise we send it
                'SendQueuedMail(oEmail, moMailConfig("PickupHost"), moMailConfig("PickupLocation"))
                Return True
            Else
                Return False
            End If

        Catch ex As Exception
            returnException(mcModuleName, "SendSingleMail_Queued", ex, "", "", gbDebug)
            Return False
        End Try
    End Function

    Public Function GetGroupEmails(ByVal groupIds As String) As UserEmailDictionary
        PerfMon.Log("Messaging", "GetGroupEmails")
        'Retrieves a list of email addresses from the groups
        'checks them against opted out adress
        'adds them to a master list and checks duplicates
        'outputs as a dictionary
        Try
            'dictionary object
            Dim oDic As New UserEmailDictionary
            Dim oDBH As New Cms.dbHelper("Data Source=" & goConfig("DatabaseServer") & "; " & _
            "Initial Catalog=" & goConfig("DatabaseName") & "; " & _
            goConfig("DatabaseAuth"), 1)
            Dim cSQL As String = "SELECT nDirKey, cDirXml" & _
            " FROM tblDirectory" & _
            " WHERE (((SELECT TOP 1 tblDirectoryRelation.nDirChildId" & _
            " FROM tblDirectoryRelation INNER JOIN" & _
            " tblDirectory Groups ON tblDirectoryRelation.nDirParentId = Groups.nDirKey" & _
            " WHERE (Groups.nDirKey IN (" & groupIds & ")) AND (tblDirectoryRelation.nDirChildId = tblDirectory.nDirKey)" & _
            " GROUP BY tblDirectoryRelation.nDirChildId)) IS NOT NULL)"

            Dim oDS As DataSet = oDBH.GetDataSet(cSQL, "Users", "Addresses")
            'set the table ready to be xml
            If oDS.Tables("Users").Rows.Count > 0 Then
                oDS.Tables("Users").Columns("nDirKey").ColumnMapping = MappingType.Attribute
                oDS.Tables("Users").Columns("cDirXml").ColumnMapping = MappingType.Element
            End If
            'get the opt out addresses
            cSQL = "SELECT EmailAddress FROM tblOptOutAddresses"
            oDBH.addTableToDataSet(oDS, cSQL, "OptOut")
            'set the table ready to be xml
            If oDS.Tables("OptOut").Rows.Count > 0 Then
                oDS.Tables("OptOut").Columns("EmailAddress").ColumnMapping = MappingType.Attribute
            End If
            'lower case the xml so that we can compare properely
            Dim oXML As New XmlDocument
            oXML.InnerXml = LCase(Replace(Replace(oDS.GetXml, "&gt;", ">"), "&lt;", "<"))

            Dim oElmt As XmlElement


            'now cycle through the users
            For Each oElmt In oXML.DocumentElement.SelectNodes("users")
                Dim cEmail As String = ""
                Dim nID As Integer = 0
                'get the user id
                'we will need this for personalised to get the user xml
                nID = oElmt.GetAttribute("ndirkey")
                'get the email address

                Dim oEmailElmt As XmlElement = oElmt.SelectSingleNode("descendant-or-self::user/email")
                If Not oEmailElmt Is Nothing Then
                    cEmail = oEmailElmt.InnerText
                End If

                Dim oCheckElmt As XmlElement = oXML.DocumentElement.SelectSingleNode("optout[@emailaddress='" & cEmail & "']")

                If oCheckElmt Is Nothing Then
                    oDic.Add(cEmail, nID)
                End If

            Next
            Return oDic
        Catch ex As Exception
            returnException(mcModuleName, "emailer", ex, "GetGroupEmails", "", gbDebug)
            Return Nothing
        End Try

    End Function



    ''Public Sub courseReminder(ByRef oResultsXml As XmlElement, ByVal cUserName As String, ByVal cUserEmail As String, ByVal subject As String, ByVal sEmailTemplate As String, ByRef fForm As Collections.Specialized.NameValueCollection) 'As String
    ''    PerfMon.Log("Messaging", "courseReminder")
    ''    ' This takes the candidate course results Report xml and sends email to all the users highlighted in the form.

    ''    Dim cProcessInfo As String = ""
    ''    Dim aUserId() As String
    ''    Dim sReturn As String
    ''    Dim oUserElmt As XmlElement
    ''    'Dim sProjectPath As String = ConfigurationSettings.AppSettings("ewProjectPath")
    ''    Dim sProjectPath As String = System.Configuration.ConfigurationManager.AppSettings("ewProjectPath")
    ''    Dim cRecipientEmail As String
    ''    Dim cCourseName As String

    ''    Try
    ''        cCourseName = oResultsXml.GetAttribute("CourseName")


    ''        'step through selected users
    ''        aUserId = Split(fForm("userId"), ",")

    ''        Dim i As Long

    ''        For i = 0 To UBound(aUserId)

    ''            'extract the users XML
    ''            oUserElmt = oResultsXml.SelectSingleNode("candidate[@id='" & aUserId(i) & "']")
    ''            cRecipientEmail = oUserElmt.SelectSingleNode("Email").InnerText
    ''            oUserElmt.SetAttribute("courseName", cCourseName)
    ''            Try
    ''                sReturn = emailer(oUserElmt, sProjectPath & "/xsl/email/" & sEmailTemplate & ".xsl" _
    ''                                    , cUserName _
    ''                                    , cUserEmail _
    ''                                    , cRecipientEmail _
    ''                                    , subject _
    ''                                    , "Sent")
    ''            Catch ex As Exception

    ''                sReturn = "Failed"

    ''            End Try
    ''            oUserElmt.SetAttribute("message", sReturn)

    ''        Next

    ''    Catch ex As Exception
    ''        returnException(mcModuleName, "courseReminder", ex, "", cProcessInfo, gbDebug)
    ''    End Try
    ''End Sub



    Public Class UserEmailDictionary
        Inherits DictionaryBase

        Default Public Property Item(ByVal key As String) As Integer
            Get
                Return CType(Dictionary(key), Integer)
            End Get
            Set(ByVal value As Integer)
                Dictionary(key) = value
            End Set
        End Property


        Public ReadOnly Property Keys() As ICollection
            Get
                Return Dictionary.Keys
            End Get
        End Property

        Public ReadOnly Property Values() As ICollection
            Get
                Return Dictionary.Values
            End Get
        End Property

        Public Sub Add(ByVal key As String, ByVal value As Integer)
            If Not Me.Contains(key) Then Dictionary.Add(key, value)
        End Sub 'Add

        Public Function Contains(ByVal key As String) As Boolean
            Return Dictionary.Contains(key)
        End Function 'Contains

        Public Sub Remove(ByVal key As String)
            Dictionary.Remove(key)
        End Sub 'Remove

        Protected Overrides Sub OnInsert(ByVal key As Object, ByVal value As Object)
            If Not GetType(System.String).IsAssignableFrom(key.GetType()) Then
                Throw New ArgumentException("key must be of type Recipient.", "key")
            End If
        End Sub 'OnInsert

        Protected Overrides Sub OnRemove(ByVal key As Object, ByVal value As Object)
            If Not GetType(System.String).IsAssignableFrom(key.GetType()) Then
                Throw New ArgumentException("key must be of type Recipient.", "key")
            End If
        End Sub 'OnRemove

        Protected Overrides Sub OnSet(ByVal key As Object, ByVal oldValue As Object, ByVal newValue As Object)
            If Not GetType(System.String).IsAssignableFrom(key.GetType()) Then
                Throw New ArgumentException("key must be of type Recipient.", "key")
            End If
        End Sub 'OnSet

        Protected Overrides Sub OnValidate(ByVal key As Object, ByVal value As Object)
            If Not GetType(System.String).IsAssignableFrom(key.GetType()) Then
                Throw New ArgumentException("key must be of type Recipient.", "key")
            End If
        End Sub 'OnValidate 


    End Class


End Class

Public Class POP3

    Public Server As System.Net.Sockets.TcpClient
    Public NetStrm As System.Net.Sockets.NetworkStream
    Public RdStrm As StreamReader
    Public Data As String
    Public szData As Byte()
    Public CRLF As String = vbCrLf '"\r\n"

    Sub ReadMail(ByVal cServer As String, ByVal cUser As String, ByVal cPassword As String)
        PerfMon.Log("POP3", "ReadMail")
        Try
            Dim nNoEmails As Integer = Connect(cServer, cUser, cPassword)

            Dim i As Integer
            Dim oXML(nNoEmails - 1) As XMLEmail

            For i = 1 To nNoEmails
                Dim Message As String = Retrieve(i)
                Dim oMsgXML As XmlDocument = CreateMailXML(Message, i)
                oXML(i - 1) = ConvertToMail(oMsgXML)
                Dim oElmt As XmlElement = oXML(i - 1).Full
                Debug.WriteLine(UBound(oXML))
            Next
            Disconnect()
        Catch ex As Exception
            Debug.WriteLine(ex.ToString)
        End Try

    End Sub


    Private Function Connect(ByVal cServer As String, ByVal cUser As String, ByVal cPassword As String) As String
        PerfMon.Log("POP3", "Connect")

        ' create server POP3 with port 110
        Server = New System.Net.Sockets.TcpClient(cServer, 110)
        Try

            ' initialization
            NetStrm = Server.GetStream()
            RdStrm = New StreamReader(Server.GetStream())
            Debug.WriteLine(RdStrm.ReadLine())

            ' Login Process
            Data = "USER " & cUser & CRLF
            szData = System.Text.Encoding.ASCII.GetBytes(Data.ToCharArray())
            NetStrm.Write(szData, 0, szData.Length)
            Debug.WriteLine(RdStrm.ReadLine())

            Data = "PASS " & cPassword & CRLF
            szData = System.Text.Encoding.ASCII.GetBytes(Data.ToCharArray())
            NetStrm.Write(szData, 0, szData.Length)
            Debug.WriteLine(RdStrm.ReadLine())

            ' Send STAT command to get information ie: number of mail and size
            Data = "STAT " & CRLF
            szData = System.Text.Encoding.ASCII.GetBytes(Data.ToCharArray())
            NetStrm.Write(szData, 0, szData.Length)

            Dim tmpArray() As String
            tmpArray = Split(RdStrm.ReadLine(), " ")
            Dim numMess As String = tmpArray(1)
            Return CInt(numMess)
        Catch ex As Exception
            Debug.WriteLine(ex.ToString)
            Return 0
        End Try
    End Function

    Private Function Retrieve(ByVal nMessageNo As Integer) As String
        PerfMon.Log("POP3", "Retrieve")

        Dim szTemp As String
        Try
            Dim Message As String = ""
            ' retrieve mail with number mail parameter
            Data = "RETR " & nMessageNo & CRLF
            szData = System.Text.Encoding.ASCII.GetBytes(Data.ToCharArray())
            NetStrm.Write(szData, 0, szData.Length)

            szTemp = RdStrm.ReadLine()
            If Not Left(szTemp, 0) = "-" Then
                While Not szTemp = "."
                    If Not Left(szTemp, 0) = "+" Then Message &= szTemp & CRLF
                    szTemp = RdStrm.ReadLine()
                End While
            Else

            End If

            Return Message
        Catch ex As Exception

            '  Status.Items.Add("Error: " + Err.ToString())
            Return ""
        End Try


    End Function

    Private Function Disconnect() As Boolean
        PerfMon.Log("POP3", "Disconnect")
        'Send QUIT command to close session from POP server
        Try
            Data = "QUIT " & CRLF
            szData = System.Text.Encoding.ASCII.GetBytes(Data.ToCharArray())
            NetStrm.Write(szData, 0, szData.Length)
            'close connection
            NetStrm.Close()
            RdStrm.Close()
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


    Public Function CreateMailXML(ByVal cMessage As String, ByVal nId As Integer) As XmlDocument
        PerfMon.Log("POP3", "CreateMailXML")
        Dim cCurrent As String = ""
        Try
            cMessage = Replace(cMessage, "+OK", "")
            cMessage = Replace(cMessage, ";" & vbCrLf, ";")
            cMessage = Replace(cMessage, vbTab, "")
            cMessage = Replace(cMessage, ">," & vbCrLf & "<", ">,<")
            Dim cSplitStr As String = ""
            Dim bMessageStarted As Boolean = False
            Dim bMessageFinished As Boolean = False
            Dim oXML As New XmlDocument
            oXML.AppendChild(oXML.CreateElement("MailMessage"))
            Dim oMessageElement As XmlElement = oXML.CreateElement("Message")

            Dim oItems() As String = Split(cMessage, vbCrLf)
            Dim i As Integer
            For i = 0 To UBound(oItems)
                Dim cCleanLine As String = Trim(oItems(i))
                If cCleanLine.Contains(": ") Then
                    Dim oItemSplit() As String = Split(oItems(i), ": ")
                    If UBound(oItemSplit) = 1 Then
                        If Not AddMailElement(oXML.DocumentElement, oItemSplit(0), oItemSplit(1)) Then
                            AddMailElement(oXML.DocumentElement, "Item", cCleanLine)
                        End If
                    Else
                        AddMailElement(oXML.DocumentElement, "Item", cCleanLine)
                    End If
                Else
                    AddMailElement(oXML.DocumentElement, "Item", cCleanLine)
                End If
                'check if we are in the messaqge
                If oXML.DocumentElement.LastChild.InnerText.Contains(cSplitStr) And Not bMessageStarted And Not cSplitStr = "" Then
                    bMessageStarted = True
                    'ElseIf oXML.DocumentElement.LastChild.InnerText.Contains(cSplitStr) And bMessageStarted And Not bMessageFinished And Not cSplitStr = "" Then
                    '    bMessageFinished = True
                End If

                If oXML.DocumentElement.LastChild.Name = "Content-Type" Then
                    If oXML.DocumentElement.LastChild.InnerText.Contains(";boundary=") Then
                        Dim nStart As Integer = InStr(oXML.DocumentElement.LastChild.InnerText, ";boundary=") + 10
                        cSplitStr = Right(oXML.DocumentElement.LastChild.InnerText, oXML.DocumentElement.LastChild.InnerText.Length - nStart)
                        cSplitStr = Left(cSplitStr, cSplitStr.Length - 1)
                    End If
                End If



                If oXML.DocumentElement.LastChild.Name = "Item" And bMessageStarted And Not oXML.DocumentElement.LastChild.InnerText.Contains(cSplitStr) Then '                If oXML.DocumentElement.LastChild.Name = "Item" And bMessageStarted And Not bMessageFinished And Not oXML.DocumentElement.LastChild.InnerText.Contains(cSplitStr) Then
                    oMessageElement.InnerText &= oXML.DocumentElement.LastChild.InnerText & CRLF
                End If
            Next
            oXML.DocumentElement.AppendChild(oMessageElement)
            Return oXML
        Catch ex As Exception
            returnException("POP3", "EmailListTransform", ex, "", "EmailListTransform", )
            Return Nothing
        End Try


    End Function

    Function AddMailElement(ByVal oParent As XmlElement, ByVal oItemName As String, ByVal oItemValue As String) As Boolean
        PerfMon.Log("POP3", "AddMailElement")
        Try
            If oItemValue = "" Or oItemValue = "." Then Return True
            Dim oElmt As XmlElement = oParent.OwnerDocument.CreateElement(Trim(oItemName))
            oElmt.InnerText = Trim(oItemValue)
            oParent.AppendChild(oElmt)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Function ConvertToMail(ByVal RawXML As XmlDocument) As XMLEmail
        PerfMon.Log("POP3", "ConvertToMail")
        Try
            Dim oElmt As XmlElement
            oElmt = RawXML.DocumentElement.SelectSingleNode("To")
            If oElmt Is Nothing Then Return Nothing
            oElmt = RawXML.DocumentElement.SelectSingleNode("From")
            If oElmt Is Nothing Then Return Nothing
            oElmt = RawXML.DocumentElement.SelectSingleNode("Thread-Topic")
            If oElmt Is Nothing Then Return Nothing
            oElmt = RawXML.DocumentElement.SelectSingleNode("Message")
            If oElmt Is Nothing Then Return Nothing

            Dim oEmail As New XMLEmail
            oEmail.Subject = RawXML.DocumentElement.SelectSingleNode("Thread-Topic").InnerText
            oEmail.Body = RawXML.DocumentElement.SelectSingleNode("Message").InnerText
            oEmail.AddAddress(XMLEmail.ToType.From, oEmail.ConvertTextAddress(RawXML.DocumentElement.SelectSingleNode("From").InnerText))
            Dim i As Integer
            Dim oAdds() As String = Split(RawXML.DocumentElement.SelectSingleNode("To").InnerText, ",")
            For i = 0 To UBound(oAdds)
                oEmail.AddAddress(XMLEmail.ToType.To, oEmail.ConvertTextAddress(oAdds(i)))
            Next
            If Not RawXML.DocumentElement.SelectSingleNode("Cc") Is Nothing Then
                oAdds = Split(RawXML.DocumentElement.SelectSingleNode("Cc").InnerText, ",")
                For i = 0 To UBound(oAdds)
                    oEmail.AddAddress(XMLEmail.ToType.CC, oEmail.ConvertTextAddress(oAdds(i)))
                Next
            End If
            If Not RawXML.DocumentElement.SelectSingleNode("Bcc") Is Nothing Then
                oAdds = Split(RawXML.DocumentElement.SelectSingleNode("Bcc").InnerText, ",")
                For i = 0 To UBound(oAdds)
                    oEmail.AddAddress(XMLEmail.ToType.BCC, oEmail.ConvertTextAddress(oAdds(i)))
                Next
            End If
            Return oEmail
        Catch ex As Exception
            Debug.WriteLine(ex.ToString)
            Return Nothing
        End Try
    End Function


    Public Sub New()

    End Sub
End Class

Public Class XMLEmail
    Dim oEmailXML As XmlDocument

    Public Sub New()
        PerfMon.Log("XMLEmail", "New")
        Try
            oEmailXML = New XmlDocument
            Dim oElmt As XmlElement = oEmailXML.CreateElement("Email")
            oEmailXML.AppendChild(oElmt)
            Dim oElmt1 As XmlElement
            oElmt1 = oEmailXML.CreateElement("From")
            oElmt.AppendChild(oElmt1)
            oElmt1 = oEmailXML.CreateElement("To")
            oElmt.AppendChild(oElmt1)
            oElmt1 = oEmailXML.CreateElement("CC")
            oElmt.AppendChild(oElmt1)
            oElmt1 = oEmailXML.CreateElement("BCC")
            oElmt.AppendChild(oElmt1)
            oElmt1 = oEmailXML.CreateElement("Subject")
            oElmt.AppendChild(oElmt1)
            oElmt1 = oEmailXML.CreateElement("Body")
            oElmt.AppendChild(oElmt1)
        Catch ex As Exception
            Debug.WriteLine(ex.ToString)
        End Try
    End Sub

    Public Function AddAddress(ByVal AddType As ToType, ByVal cEmailAddress As String, Optional ByVal cDisplayName As String = "") As Boolean
        PerfMon.Log("XMLEmail", "AddAddress")
        Try
            Dim oAddElmt As XmlElement = oEmailXML.CreateElement("EmailAddress")
            Dim oEmailElmt As XmlElement = oEmailXML.CreateElement("Address")
            oEmailElmt.InnerText = cEmailAddress
            Dim oNameElmt As XmlElement = oEmailXML.CreateElement("DisplayName")
            oNameElmt.InnerText = cDisplayName
            oAddElmt.AppendChild(oEmailElmt)
            oAddElmt.AppendChild(oNameElmt)
            oEmailXML.DocumentElement.SelectSingleNode(System.Enum.GetName(GetType(ToType), AddType)).AppendChild(oAddElmt)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function AddAddress(ByVal AddType As ToType, ByVal oAddress As Net.Mail.MailAddress) As Boolean
        PerfMon.Log("XMLEmail", "AddAddress")
        Try
            Dim oAddElmt As XmlElement = oEmailXML.CreateElement("EmailAddress")
            Dim oEmailElmt As XmlElement = oEmailXML.CreateElement("Address")
            oEmailElmt.InnerText = oAddress.Address
            Dim oNameElmt As XmlElement = oEmailXML.CreateElement("DisplayName")
            oNameElmt.InnerText = oAddress.DisplayName
            oAddElmt.AppendChild(oEmailElmt)
            oAddElmt.AppendChild(oNameElmt)
            oEmailXML.DocumentElement.SelectSingleNode(System.Enum.GetName(GetType(ToType), AddType)).AppendChild(oAddElmt)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function RemoveAddress(ByVal AddType As ToType, ByVal cEmailAddress As String) As Boolean
        PerfMon.Log("XMLEmail", "RemoveAddress")
        Try
            Dim oMainElmt As XmlElement = oEmailXML.DocumentElement.SelectSingleNode(System.Enum.GetName(GetType(ToType), AddType))
            Dim oElmt As XmlElement
            For Each oElmt In oMainElmt.SelectNodes("EmailAddress[Address='" & cEmailAddress & "']")
                oElmt.ParentNode.RemoveChild(oElmt)
            Next
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Property Body() As String
        Get
            Return oEmailXML.DocumentElement.SelectSingleNode("Body").InnerText
        End Get
        Set(ByVal value As String)
            oEmailXML.DocumentElement.SelectSingleNode("Body").InnerText = value
        End Set
    End Property

    Public Property Subject() As String
        Get
            Return oEmailXML.DocumentElement.SelectSingleNode("Subject").InnerText
        End Get
        Set(ByVal value As String)
            oEmailXML.DocumentElement.SelectSingleNode("Subject").InnerText = value
        End Set
    End Property

    Public ReadOnly Property Full() As XmlElement
        Get
            Return oEmailXML.DocumentElement
        End Get
    End Property

    Public ReadOnly Property ToList(ByVal AddType As ToType) As Net.Mail.MailAddress()
        Get
            Dim oElmt As XmlElement
            Dim oAddArr(0) As Net.Mail.MailAddress
            Dim i As Integer = 0
            For Each oElmt In oEmailXML.DocumentElement.SelectSingleNode(System.Enum.GetName(GetType(ToType), AddType)).ChildNodes
                Dim oAdd As New Net.Mail.MailAddress(oElmt.SelectSingleNode("Address").InnerText, oElmt.SelectSingleNode("DisplayName").InnerText)
                ReDim Preserve oAddArr(i + 1)
                oAddArr(i) = oAdd
            Next
            Return oAddArr
        End Get
    End Property

    Public Function ConvertTextAddress(ByVal cTextAddress As String) As Net.Mail.MailAddress
        PerfMon.Log("XMLEmail", "ConvertTextAddress")
        Try
            '"Barry Rushton" <barryr@eonic.co.uk>
            Dim cName As String = ""
            If cTextAddress.Contains(Chr(34)) Then
                cName = Right(cTextAddress, cTextAddress.Length - InStr(cTextAddress, Chr(34)))
                If Not cName = "" Then cName = Left(cName, InStr(cName, Chr(34)) - 1)
            End If
            Dim cAddress As String
            cAddress = Right(cTextAddress, cTextAddress.Length - InStr(cTextAddress, "<"))
            If Not cAddress = "" Then cAddress = Left(cAddress, InStr(cAddress, ">") - 1)

            Return New Net.Mail.MailAddress(Trim(cAddress), Trim(cName))
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Enum ToType
        [To] = 0
        CC = 1
        BCC = 2
        From = 3
    End Enum

End Class



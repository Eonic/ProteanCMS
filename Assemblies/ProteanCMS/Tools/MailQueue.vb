Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Web.Configuration
Imports System.IO
Imports System.Data
Imports System.Data.SqlClient
Imports System

Public Class MailQueue

    Public mcModuleName As String = "Eonic.MailQueue"
    Public moConfig As System.Collections.Specialized.NameValueCollection
    Public msException As String


    Public Sub New()
        MyBase.New()
        moConfig = WebConfigurationManager.GetWebApplicationSection("protean/web")
        'PerfMon.Log("MailQueue", "New")
        mcModuleName = "MailQueue"
    End Sub

    Dim oSchedulerConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/scheduler")
    Dim oDBT_Local As Protean.Cms.dbHelper
    Dim oDBT_Remote As Protean.Cms.dbHelper



    Public Function Add(ByVal nPageId As Integer, ByVal cFromEmail As String, ByVal cFromName As String, ByVal cSubject As String, Optional ByVal cBody As String = "", Optional ByVal cGroups_CSV As String = "", Optional ByVal nUserId As Integer = 0, Optional ByVal bSkipQue As Boolean = False) As Integer
        'PerfMon.Log("MailQueue", "Add")
        Try
            oDBT_Local = New Cms.dbHelper("Data Source=" & moConfig("DatabaseServer") & "; " &
            "Initial Catalog=" & moConfig("DatabaseName") & "; " &
            moConfig("DatabaseAuth"), 1)



            Dim cConStr As String

            If Not oSchedulerConfig Is Nothing Then
                cConStr = "Data Source=" & oSchedulerConfig("DatabaseServer") & "; "
                cConStr &= "Initial Catalog=" & oSchedulerConfig("DatabaseName") & "; "
                cConStr &= oSchedulerConfig("DatabaseAuth")
                oDBT_Remote.ResetConnection(cConStr)
            Else
                Return 0
            End If

            oDBT_Remote = New Cms.dbHelper(cConStr, 1)

            Dim cSQL As String = ""
            If Not cGroups_CSV = "" And nUserId = 0 Then
                cSQL = "SELECT nDirKey, cDirXml" &
                " FROM tblDirectory" &
                " WHERE (((SELECT TOP 1 nDirChildId" &
                    " FROM tblDirectoryRelation" &
                    " WHERE (nDirParentId IN (" & cGroups_CSV & ")) AND (nDirChildId = tblDirectory.ndirKey))) IS NOT NULL)"
            ElseIf cGroups_CSV = "" And Not nUserId = 0 Then
                'send to individual
                cSQL = "SELECT nDirKey, cDirXml FROM tblDirectory WHERE nDirKey = " & nUserId
            Else
                Return 0
            End If

            Dim nRequestID As Integer = AddRequest(nPageId, cFromEmail, cFromName, cSubject, cBody, bSkipQue)
            If nRequestID = 0 Then Return 0
            Dim oUserXML As New XmlDocument
            Dim oDS As DataSet = oDBT_Local.GetDataSet(cSQL, "Users")
            Dim oDR As DataRow
            For Each oDR In oDS.Tables("Users").Rows
                Dim oElmt As XmlElement = oUserXML.CreateElement("UserDetails")
                oElmt.InnerXml = Replace(Replace(oDR("cDirXML"), "&gt;", ">"), "&lt;", "<")
                Dim cEmail As String = ""
                Dim oEmailElmt As XmlElement = oElmt.SelectSingleNode("User/Email")
                If Not oEmailElmt Is Nothing Then
                    cEmail = oEmailElmt.InnerText
                End If
                Dim cName As String = ""
                Dim oFNameElmt As XmlElement = oElmt.SelectSingleNode("User/FirstName")
                If Not oFNameElmt Is Nothing Then cName = oFNameElmt.InnerText
                Dim oLNameElmt As XmlElement = oElmt.SelectSingleNode("User/LastName")
                If Not oLNameElmt Is Nothing Then
                    If Not cName = "" Then cName &= " " & oLNameElmt.InnerText
                End If
                AddRecipient(nRequestID, oDR("nDirKey"), cEmail, cName)
            Next
            FinishRequest(nRequestID)
        Catch ex As Exception
            returnException(msException, mcModuleName, "Add", ex, "", "", gbDebug)
        End Try
    End Function

    Private Function AddRequest(ByVal nPageId As Integer, ByVal cFromEmail As String, ByVal cFromName As String, ByVal cSubject As String, Optional ByVal cBody As String = "", Optional ByVal bSkipQue As Boolean = False) As Integer
        'PerfMon.Log("MailQueue", "AddRequest")
        Try
            Dim cSQL As String = "INSERT INTO tblMailRequests (nPageId, cBody, cFromEmail, cFromName, cSubject, cSiteURL, cMailServer, nStatus, nPriority) VALUES ("
            cSQL &= nPageId & ","
            cSQL &= "'" & cBody & "',"
            cSQL &= "'" & cFromEmail & "',"
            cSQL &= "'" & cFromName & "',"
            cSQL &= "'" & cSubject & "',"
            cSQL &= "'" & moConfig("BaseUrl") & "',"
            cSQL &= "" & moConfig("MailServer") & ","
            cSQL &= "0,"
            cSQL &= IIf(bSkipQue, 1, 0) & ")"
            Return oDBT_Remote.GetIdInsertSql(cSQL)
        Catch ex As Exception
            returnException(msException, mcModuleName, "Add Request", ex, "", "", gbDebug)
        End Try
    End Function

    Private Function AddRecipient(ByVal nRequestKey As Integer, ByVal nUserID As Integer, ByVal cToEmail As String, ByVal cToName As String) As Integer
        'PerfMon.Log("MailQueue", "AddRecipient")
        Try
            Dim cSQL As String = "INSERT INTO tblMailRequestRecipients (nRequestId, nUserId, nStatus, nRetries, cEmail, cName) VALUES ("
            cSQL &= nRequestKey & ","
            cSQL &= nUserID & ","
            cSQL &= "0,"
            cSQL &= "0,"
            cSQL &= "'" & cToEmail & "',"
            cSQL &= "'" & cToName & "')"
            Return oDBT_Remote.GetIdInsertSql(cSQL)
        Catch ex As Exception
            returnException(msException, mcModuleName, "Add Recipient", ex, "", "", gbDebug)
        End Try
    End Function

    Private Sub FinishRequest(ByVal nRequestId As Integer)
        'PerfMon.Log("MailQueue", "FinishRequest")
        Try
            Dim cSQL As String = "UPDATE tblMailRequests SET nStatus = 1 WHERE nMailRequestKey = " & nRequestId
            oDBT_Remote.ExeProcessSql(cSQL)
        Catch ex As Exception
            returnException(msException, mcModuleName, "Finish Request", ex, "", "", gbDebug)
        End Try
    End Sub

    Public Function GetEmailPage(ByVal nPageId As Integer, ByVal nUserId As Integer) As String
        'PerfMon.Log("MailQueue", "GetEmaiPage")
        Try
            Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")
            Dim oWeb As New Protean.Cms

            oWeb.InitializeVariables()
            oWeb.Open()
            oWeb.mnPageId = nPageId
            oWeb.mnUserId = nUserId
            oWeb.mbAdminMode = False
            oWeb.mcEwSiteXsl = moMailConfig("MailingXsl")
            oWeb.mnMailMenuId = moMailConfig("RootPageId")
            Dim cReturnString As String = oWeb.ReturnPageHTML(nPageId, True)
            cReturnString = NameEntities(cReturnString)
            GetEmailPage = cReturnString
        Catch ex As Exception
            GetEmailPage = ""
        End Try
        GC.Collect()
    End Function

    Private Function NameEntities(ByVal cString As String) As String
        'PerfMon.Log("MailQueue", "NameEntities")
        cString = Replace(cString, "'", "&apos; ") '&#39;
        cString = Replace(cString, "¡", "&iexcl;") '&#161;
        cString = Replace(cString, "¤", "&curren;") '&#164;
        cString = Replace(cString, "¢", "&cent;") '&#162;
        cString = Replace(cString, "£", "&pound;") '&#163;
        cString = Replace(cString, "¥", "&yen;") '&#165;
        cString = Replace(cString, "¦", "&brvbar;") '&#166;
        cString = Replace(cString, "§", "&sect;") '&#167;
        cString = Replace(cString, "¨", "&uml;") '&#168;
        cString = Replace(cString, "©", "&copy;") '&#169;
        cString = Replace(cString, "ª", "&ordf;") '&#170;
        cString = Replace(cString, "¬", "&not;") '&#172;
        cString = Replace(cString, "­", "&shy;") '&#173;
        cString = Replace(cString, "®", "&reg;") '&#174;
        cString = Replace(cString, "™", "&trade;") '&#8482;
        cString = Replace(cString, "¯", "&macr;") '&#175;
        cString = Replace(cString, "°", "&deg;") '&#176;
        cString = Replace(cString, "±", "&plusmn;") '&#177;
        cString = Replace(cString, "²", "&sup2;") '&#178;
        cString = Replace(cString, "³", "&sup3;") '&#179;
        cString = Replace(cString, "´", "&acute;") '&#180;
        cString = Replace(cString, "µ", "&micro;") '&#181;
        cString = Replace(cString, "¶", "&para;") '&#182;
        cString = Replace(cString, "·", "&middot;") '&#183;
        cString = Replace(cString, "¸", "&cedil;") '&#184;
        cString = Replace(cString, "¹", "&sup1;") '&#185;
        cString = Replace(cString, "º", "&ordm;") '&#186;
        cString = Replace(cString, "»", "&raquo;") '&#187;
        cString = Replace(cString, "¼", "&frac14;") '&#188;
        cString = Replace(cString, "½", "&frac12;") '&#189;
        cString = Replace(cString, "¾", "&frac34;") '&#190;
        cString = Replace(cString, "¿", "&iquest;") '&#191;
        cString = Replace(cString, "×", "&times;") '&#215;
        cString = Replace(cString, "÷", "&divide;") '&#247;
        cString = Replace(cString, "À", "&Agrave;") '&#192;
        cString = Replace(cString, "Á", "&Aacute;") '&#193;
        cString = Replace(cString, "Â", "&Acirc;") '&#194;
        cString = Replace(cString, "Ã", "&Atilde;") '&#195;
        cString = Replace(cString, "Ä", "&Auml;") '&#196;
        cString = Replace(cString, "Å", "&Aring;") '&#197;
        cString = Replace(cString, "Æ", "&AElig;") '&#198;
        cString = Replace(cString, "Ç", "&Ccedil;") '&#199;
        cString = Replace(cString, "È", "&Egrave;") '&#200;
        cString = Replace(cString, "É", "&Eacute;") '&#201;
        cString = Replace(cString, "Ê", "&Ecirc;") '&#202;
        cString = Replace(cString, "Ë", "&Euml;") '&#203;
        cString = Replace(cString, "Ì", "&Igrave;") '&#204;
        cString = Replace(cString, "Í", "&Iacute;") '&#205;
        cString = Replace(cString, "Î", "&Icirc;") '&#206;
        cString = Replace(cString, "Ï", "&Iuml;") '&#207;
        cString = Replace(cString, "Ð", "&ETH;") '&#208;
        cString = Replace(cString, "Ñ", "&Ntilde;") '&#209;
        cString = Replace(cString, "Ò", "&Ograve;") '&#210;
        cString = Replace(cString, "Ó", "&Oacute;") '&#211;
        cString = Replace(cString, "Ô", "&Ocirc;") '&#212;
        cString = Replace(cString, "Õ", "&Otilde;") '&#213;
        cString = Replace(cString, "Ö", "&Ouml;") '&#214;
        cString = Replace(cString, "Ø", "&Oslash;") '&#216;
        cString = Replace(cString, "Ù", "&Ugrave;") '&#217;
        cString = Replace(cString, "Ú", "&Uacute;") '&#218;
        cString = Replace(cString, "Û", "&Ucirc;") '&#219;
        cString = Replace(cString, "Ü", "&Uuml;") '&#220;
        cString = Replace(cString, "Ý", "&Yacute;") '&#221;
        cString = Replace(cString, "Þ", "&THORN;") '&#222;
        cString = Replace(cString, "ß", "&szlig;") '&#223;
        cString = Replace(cString, "à", "&agrave;") '&#224;
        cString = Replace(cString, "á", "&aacute;") '&#225;
        cString = Replace(cString, "â", "&acirc;") '&#226;
        cString = Replace(cString, "ã", "&atilde;") '&#227;
        cString = Replace(cString, "ä", "&auml;") '&#228;
        cString = Replace(cString, "å", "&aring;") '&#229;
        cString = Replace(cString, "æ", "&aelig;") '&#230;
        cString = Replace(cString, "ç", "&ccedil;") '&#231;
        cString = Replace(cString, "è", "&egrave;") '&#232;
        cString = Replace(cString, "é", "&eacute;") '&#233;
        cString = Replace(cString, "ê", "&ecirc;") '&#234;
        cString = Replace(cString, "ë", "&euml;") '&#235;
        cString = Replace(cString, "ì", "&igrave;") '&#236;
        cString = Replace(cString, "í", "&iacute;") '&#237;
        cString = Replace(cString, "î", "&icirc;") '&#238;
        cString = Replace(cString, "ï", "&iuml;") '&#239;
        cString = Replace(cString, "ð", "&eth;") '&#240;
        cString = Replace(cString, "ñ", "&ntilde;") '&#241;
        cString = Replace(cString, "ò", "&ograve;") '&#242;
        cString = Replace(cString, "ó", "&oacute;") '&#243;
        cString = Replace(cString, "ô", "&ocirc;") '&#244;
        cString = Replace(cString, "õ", "&otilde;") '&#245;
        cString = Replace(cString, "ö", "&ouml;") '&#246;
        cString = Replace(cString, "ø", "&oslash;") '&#248;
        cString = Replace(cString, "ù", "&ugrave;") '&#249;
        cString = Replace(cString, "ú", "&uacute;") '&#250;
        cString = Replace(cString, "û", "&ucirc;") '&#251;
        cString = Replace(cString, "ü", "&uuml;") '&#252;
        cString = Replace(cString, "ý", "&yacute;") '&#253;
        cString = Replace(cString, "þ", "&thorn;") '&#254;
        cString = Replace(cString, "ÿ", "&yuml;") '&#255;
        cString = Replace(cString, "Œ", "&OElig;") '&#338;
        cString = Replace(cString, "œ", "&oelig;") '&#339;
        cString = Replace(cString, "Š", "&Scaron;") '&#352;
        cString = Replace(cString, "š", "&scaron;") '&#353;
        cString = Replace(cString, "Ÿ", "&Yuml;") '&#376;
        cString = Replace(cString, "ˆ", "&circ;") '&#710;
        cString = Replace(cString, "˜", "&tilde;") '&#732;
        cString = Replace(cString, "–", "&ndash;") '&#8211;
        cString = Replace(cString, "—", "&mdash;") '&#8212;
        cString = Replace(cString, "‘", "&lsquo;") '&#8216;
        cString = Replace(cString, "’", "&rsquo;") '&#8217;
        cString = Replace(cString, "„", "&bdquo;") '&#8222;
        cString = Replace(cString, "†", "&dagger;") '&#8224;
        cString = Replace(cString, "‡", "&Dagger;") '&#8225;
        cString = Replace(cString, "‰", "&permil;") '&#8240;
        cString = Replace(cString, "€", "&euro;") '&#8364;
        cString = Replace(cString, Chr(13), "")
        cString = Replace(cString, Chr(9), "")
        cString = Replace(cString, Chr(160), " ")
        cString = Replace(cString, Chr(10), "")
        cString = Replace(cString, vbCrLf, "")
        cString = Replace(cString, vbTab, "")
        'Dim i As Integer = 1
        'Do Until i <= 0
        '    i = InStr(cString, "  ")
        '    cString = Replace(cString, "  ", " ")
        'Loop
        cString = Replace(cString, "> <", ">&nbsp;<")
        Return cString
    End Function
End Class

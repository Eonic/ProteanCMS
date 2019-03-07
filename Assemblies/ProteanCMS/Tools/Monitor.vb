Imports System.web.Configuration
Imports Eonic.Web
Imports System.Data
Imports System.Xml
Imports System

Public Class Monitor

#Region "Declarations"

    Dim myWeb As Eonic.Web
    Dim mcModuleName As String = "Eonic.Monitor"

#End Region
#Region "New Error Handling"
    Public Shadows Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)

    Private Sub _OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)
        RaiseEvent OnError(sender, e)
    End Sub
#End Region

#Region "  Initialise"

    Public Sub New(ByRef aWeb As Eonic.Web)
        myWeb = aWeb
    End Sub

#End Region

#Region "  Public Procedures"

    Public Function EmailMonitorScheduler() As String

        Dim cProcessInfo As String = ""
        Dim cMonitorEmail As String = "alerts@eonic.co.uk"
        Dim cMonitorXsl As String = "/ewcommon/xsl/Tools/schedulermonitor.xsl"
        Dim oMonitorXml As XmlElement
        Dim cResponse As String = "Not Sent"
        Try

            Dim oSchedulerConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/scheduler")
            Dim oMsg As New Eonic.Messaging

            If Not oSchedulerConfig Is Nothing Then
                cMonitorEmail = oSchedulerConfig("SchedulerMonitorEmail")
                If cMonitorEmail Is Nothing Then cMonitorXsl = "alerts@eonic.co.uk"
                cMonitorXsl = oSchedulerConfig("SchedulerMonitorXsl")
                If cMonitorXsl Is Nothing Then cMonitorXsl = "/ewcommon/xsl/Tools/schedulermonitor.xsl"
                oMonitorXml = GetMonitorSchedulerXml()
                cResponse = oMsg.emailer(oMonitorXml, cMonitorXsl, "EonicWebV5", "eonicwebV5@eonic.co.uk", cMonitorEmail, "")
            End If

        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "EmailMonitorScheduler", ex, cProcessInfo))
        End Try

        Return cResponse
    End Function

    Public Function GetMonitorSchedulerXml() As XmlElement

        Dim cProcessInfo As String = ""
        Dim cUrl As String = ""

        Dim cConStr As String
        Dim oDBh As New dbHelper(myWeb)
        Dim oDS As DataSet
        Dim oMXML As New XmlDataDocument
        Dim oElmt As XmlElement = oMXML.CreateElement("NoData")
        Dim oSchedulerConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/scheduler")

        Try

            If Not oSchedulerConfig Is Nothing Then

                ' If Scheduler Info is present then
                cProcessInfo = "Connecting to the Scheduler"

                cConStr = "Data Source=" & oSchedulerConfig("DatabaseServer") & "; "
                cConStr &= "Initial Catalog=" & oSchedulerConfig("DatabaseName") & "; "
                If oSchedulerConfig("DatabaseAuth") <> "" Then
                    cConStr &= oSchedulerConfig("DatabaseAuth")
                Else
                    cConStr &= "user id=" & oSchedulerConfig("DatabaseUsername") & ";password=" & oSchedulerConfig("DatabasePassword") & ";"
                End If

                oDBh.ResetConnection(cConStr)

                oDS = oDBh.GetDataSet("EXEC spGetSchedulerSummary @date=" & Eonic.Tools.Database.SqlDate(Now().AddDays(-1), True), "Scan", "Monitor")
                oDBh.ReturnNullsEmpty(oDS)
                oMXML = New XmlDataDocument(oDS)
                If Not (oMXML Is Nothing) AndAlso Not (oMXML.DocumentElement Is Nothing) Then
                    oElmt = oMXML.DocumentElement
                End If
                oDS.EnforceConstraints = False

            End If
        Catch ex As Exception
            RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "GetMonitorSchedulerXml", ex, cProcessInfo))
        Finally
            oDBh.CloseConnection()
            oDBh = Nothing
        End Try

        If Not oSchedulerConfig Is Nothing Then
            oElmt.SetAttribute("DatabaseServer", oSchedulerConfig("DatabaseServer"))
            oElmt.SetAttribute("Database", oSchedulerConfig("DatabaseName"))
        End If

        Return oElmt


    End Function

#End Region

End Class

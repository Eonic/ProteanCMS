Option Strict Off
Option Explicit On
Imports System.Web.Configuration
Imports System.Text
Imports Protean.stdTools

Public Class PerfLog

    Public cSiteName As String
    Public cDataConn As String = ""

    Private bLoggingOn As Boolean
    Private nStep As Integer
    Private oBuilder As StringBuilder
    Private oPerfMonRequests As PerformanceCounter
    Private Entries() As String
    Dim dLast As Date = Now
    Dim nTimeAccumalative As Double = 0
    Dim nMemLast As Integer = 0
    Dim nProcLast As Integer = 0
    Dim LatestLog As String = ""


    ' Counters
    Private _workingSetPrivateMemoryCounter As PerformanceCounter
    Private _workingSetMemoryCounter As PerformanceCounter


    Private moCtx As System.Web.HttpContext = System.Web.HttpContext.Current

    'Session / Request Level Properties
    Public moRequest As System.Web.HttpRequest
    Public moResponse As System.Web.HttpResponse
    Public moSession As System.Web.SessionState.HttpSessionState
    Public moServer As System.Web.HttpServerUtility

    Public moConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/web")




    Public Property Logging() As Boolean
        Get
            Return bLoggingOn
        End Get
        Set(ByVal value As Boolean)
            bLoggingOn = value
        End Set
    End Property

    Public Sub New(ByVal SiteName As String)
        Try
            cSiteName = SiteName

            If Not moCtx Is Nothing Then
                moRequest = moCtx.Request
                moResponse = moCtx.Response
                If Not moCtx.Session Is Nothing Then moSession = moCtx.Session
                moServer = moCtx.Server
            End If


            If Not moSession Is Nothing Then
                If moSession("Logging") = "On" Then
                    TurnOn()
                End If
            End If
        Catch ex As Exception

        End Try


    End Sub

    Private Sub TurnOn()
        'If bLoggingOn Then Exit Sub
        Try
            If Not bLoggingOn Then
                ReDim Entries(1000)
                bLoggingOn = True
                nStep = 0
                moSession("Logging") = "On"
                Try
                    ' oPerfMonRequests = New System.Diagnostics.PerformanceCounter("ASP.NET v4.0.30319", "Requests Current")
                Catch ex As Exception
                    'do nothing
                End Try

                Dim cSessionRequest As String = moSession("SessionRequest")
                If IsNumeric(cSessionRequest) Then
                    moSession("SessionRequest") = CInt(cSessionRequest) + 1
                    dLast = Now
                    nTimeAccumalative = 0
                    nMemLast = 0
                    nProcLast = 0
                Else
                    moSession("SessionRequest") = 0
                    dLast = Now
                    nTimeAccumalative = 0
                    nMemLast = 0
                    nProcLast = 0
                End If

                _workingSetPrivateMemoryCounter = New PerformanceCounter("Process", "Working Set - Private", Process.GetCurrentProcess.ProcessName)
                _workingSetMemoryCounter = New PerformanceCounter("Process", "Working Set", Process.GetCurrentProcess.ProcessName)

            End If
        Catch ex As Exception
            Debug.WriteLine(ex.ToString)
        End Try
    End Sub

    Public Sub Start()
        TurnOn()
    End Sub

    Public Sub [Stop]()
        bLoggingOn = False
        moSession("Logging") = "Off"
    End Sub

    Public Sub Log(ByVal cModuleName As String, ByVal cProcessName As String, Optional ByVal cDescription As String = "")
        'If Not bLoggingOn Then Exit Sub
        Try

            'TS moved to run regardless as this seems to improve peformance if you call these values.
            If bLoggingOn Then
                Dim oLN As TimeSpan = Now - dLast
                nTimeAccumalative += oLN.TotalMilliseconds

                Dim memoryPrivate As Long
                If _workingSetPrivateMemoryCounter Is Nothing Then
                    memoryPrivate = 0
                Else
                    memoryPrivate = _workingSetPrivateMemoryCounter.NextValue()
                End If

                nMemLast = memoryPrivate
                nProcLast = Process.GetCurrentProcess.PrivilegedProcessorTime.Milliseconds

                Dim nMemDif As Long = memoryPrivate - nMemLast
                Dim nProcDif As Long = Process.GetCurrentProcess.PrivilegedProcessorTime.Milliseconds - nProcLast
                Dim nMemoryCounterNextVal As Long = Nothing
                If Not _workingSetMemoryCounter Is Nothing Then
                    nMemoryCounterNextVal = CLng(_workingSetMemoryCounter.NextValue())
                End If

                '    If bLoggingOn Then

                Dim cEntryFull As String = "INSERT INTO tblPerfMon" &
                " ( MachineName, Website, SessionID, SessionRequest, Path, [Module], [Procedure],Description, Step, [Time],TimeAccumalative, Requests, PrivateMemorySize64, PrivilegedProcessorTimeMilliseconds)" &
                " VALUES("
                cEntryFull &= "'"
                cEntryFull &= moServer.MachineName & "','"
                cEntryFull &= cSiteName & "','"
                If moSession.SessionID IsNot Nothing Then
                    Try
                        cEntryFull &= CStr(moSession.SessionID & "") & "','"
                        cEntryFull &= CStr(moSession("SessionRequest") & "") & "','"
                    Catch ex As Exception
                        cEntryFull &= "','','"
                    End Try
                Else
                    cEntryFull &= "','','"
                End If

                'If moSession.SessionID Is Nothing Then

                'Else
                '    cEntryFull &= CStr(moSession.SessionID & "") & "','"
                '    cEntryFull &= CStr(moSession("SessionRequest") & "") & "','"
                'End If
                Dim cPath As String = ""
                If Not System.Web.HttpContext.Current Is Nothing Then
                    If Not System.Web.HttpContext.Current.Request Is Nothing Then
                        cPath = System.Web.HttpContext.Current.Request("path")
                    End If
                End If

                cEntryFull &= SqlFmt(cPath) & "','"
                cEntryFull &= SqlFmt(cModuleName) & "','"
                cEntryFull &= Left(SqlFmt(cProcessName), 254) & "','"
                cEntryFull &= Left(SqlFmt(cDescription), 3999) & "',"
                cEntryFull &= nStep & ","
                cEntryFull &= oLN.TotalMilliseconds & ","
                cEntryFull &= nTimeAccumalative & ","
                If oPerfMonRequests Is Nothing Then
                    cEntryFull &= "null,'"
                Else
                    cEntryFull &= oPerfMonRequests.RawValue & ",'"
                End If

                cEntryFull &= nMemLast & "','"
                If _workingSetMemoryCounter Is Nothing Then
                    cEntryFull &= ""
                Else
                    cEntryFull &= CLng(_workingSetMemoryCounter.NextValue())
                End If
                'cEntryFull &= Process.GetCurrentProcess.PrivateMemorySize64 & "','"
                'cEntryFull &= Process.GetCurrentProcess.WorkingSet64
                cEntryFull &= "')"
                nStep += 1

                If nStep > 128 Then
                    Dim test As String = "text"
                End If

                '   ReDim Preserve Entries(nStep)
                Entries(nStep - 1) = cEntryFull

                'nMemLast = Process.GetCurrentProcess.PrivateMemorySize64
                'nProcLast = Process.GetCurrentProcess.PrivilegedProcessorTime.Milliseconds
                dLast = Now

                'Else

                '   Dim nMemDif As Long = Process.GetCurrentProcess.WorkingSet64
                '   Dim nMemTotal As Long = Process.GetCurrentProcess.PrivateMemorySize64
            Else
                'TS this is to be viewed in a memory dump to see how far the CMS object has proceeded.
                LatestLog = cModuleName + "-" + cProcessName + "-" + cDescription
            End If
        Catch ex As Exception
            Debug.WriteLine(ex.ToString)
        End Try
    End Sub

    Public Sub Write()
        'If Not bLoggingOn Then Exit Sub
        Dim cProcessInfo As String = Nothing
        Try
            If bLoggingOn Then
                Dim ConStr As String = "" 'moConfig("PerfMonConnection")
                If ConStr = "" Then
                    Dim dbAuth As String
                    If moConfig("DatabasePassword") <> "" Then
                        dbAuth = "user id=" & moConfig("DatabaseUsername") & "; password=" & moConfig("DatabasePassword")
                    Else
                        If moConfig("DatabaseAuth") <> "" Then
                            dbAuth = moConfig("DatabaseAuth")
                        Else
                            dbAuth = "Integrated Security=SSPI;"
                        End If
                    End If
                    ConStr = "Data Source=" & moConfig("DatabaseServer") & "; " &
                        "Initial Catalog=" & moConfig("DatabaseName") & "; " & dbAuth
                End If
                Dim oCon As New SqlClient.SqlConnection(ConStr)
                Dim oCmd As New SqlClient.SqlCommand
                oCmd.Connection = oCon
                oCon.Open()
                Dim i As Integer
                For i = 0 To UBound(Entries)
                    If Not Entries(i) = "" Then
                        cProcessInfo = Entries(i)
                        oCmd.CommandText = Entries(i)
                        Try
                            oCmd.ExecuteNonQuery()
                        Catch ex As Exception
                            cProcessInfo = oCmd.CommandText
                        End Try
                    End If
                Next
                oCmd.Dispose()
                oCon.Close()
                oCon.Dispose()
                oCon = Nothing
                bLoggingOn = False
            End If
        Catch ex As Exception

            Debug.WriteLine(cProcessInfo & " - errormsg - " & ex.ToString)
        Finally
            Entries = Nothing
        End Try
    End Sub

End Class

Imports System

Partial Public Class Web
    Public Class xForm
        Inherits Eonic.xForm

#Region "New Error Handling"

        Public Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)

        Private Sub _OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs) Handles Me.OnError
            returnException(e.ModuleName, e.ProcedureName, e.Exception, "", e.AddtionalInformation, gbDebug)
        End Sub

#End Region

#Region "Declarations"
        Public myWeb As Eonic.Web
        Private mcModuleName As String = "Web.xForm"
#End Region
        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByRef aWeb As Eonic.Web)
            MyBase.New()
            PerfMon.Log(mcModuleName, "New")
            Try

                myWeb = aWeb
                moPageXML = myWeb.moPageXml
                mnUserId = myWeb.mnUserId
                moCtx = myWeb.moCtx

                If Not moCtx Is Nothing Then
                    goApp = moCtx.Application
                    goRequest = moCtx.Request
                    goResponse = moCtx.Response
                    goSession = moCtx.Session
                    goServer = moCtx.Server
                End If

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
            End Try
        End Sub


        Overrides Function evaluateByType(ByVal sValue As String, ByVal sType As String, Optional ByVal cExtensions As String = "", Optional ByVal isRequired As Boolean = False) As String
            Dim cProcessInfo As String = ""
            Dim cReturn As String = "" ' Set this as a clear return string

            Try
                cReturn = MyBase.evaluateByType(sValue, sType, cExtensions, isRequired)

                ' Only evaulate if there is data to evaluate against!

                If Not myWeb Is Nothing And sValue <> "" Then
                    Select Case sType
                        Case "reValidateUser"
                            cReturn = myWeb.moDbHelper.validateUser(mnUserId, sValue)
                            If IsNumeric(cReturn) Then cReturn = ""
                    End Select
                End If

                Return cReturn

            Catch ex As Exception
                RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "evaluteByType", ex, ""))
                Return ""
            End Try
        End Function

    End Class
End Class


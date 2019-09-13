Imports System
Imports System.Xml

Partial Public Class Cms
    Public Class xForm
        Inherits Protean.xForm

#Region "New Error Handling"

        Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)

        Private Sub _OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs) Handles Me.OnError
            returnException(e.ModuleName, e.ProcedureName, e.Exception, "", e.AddtionalInformation, gbDebug)
        End Sub

#End Region

#Region "Declarations"
        Public myWeb As Protean.Cms
        Private mcModuleName As String = "Web.xForm"
#End Region
        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByRef aWeb As Protean.Cms)
            MyBase.New()
            PerfMon.Log(mcModuleName, "New")
            Try

                myWeb = aWeb
                moPageXml = myWeb.moPageXml
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
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""))
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
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "evaluteByType", ex, ""))
                Return ""
            End Try
        End Function


        Overrides Function isUnique(ByVal sValue As String, ByVal sPath As String) As Boolean
            Dim cProcessInfo As String = ""
            'Placeholder for overide
            Try

                'Confirm the contenttype and the field
                Dim unqiueVal() As String = sPath.Split("|")
                Dim tableName As String = unqiueVal(0)
                Dim schemaName As String = unqiueVal(1)
                Dim fieldName As String = unqiueVal(2)
                Dim xPath As String = unqiueVal(3)
                'Generate the xpath if value is in XML within the field


                'Query the database to confirm if this value is unique.

                Return myWeb.moDbHelper.isUnique(tableName, schemaName, fieldName, xPath, sValue)


            Catch ex As Exception
                returnException(mcModuleName, "isUnique", ex, "", cProcessInfo, gbDebug)
                Return ""
            End Try
        End Function

    End Class
End Class


Namespace Errors
    Public Class ErrorEventArgs
        Inherits System.EventArgs
#Region "Declarations"
        Private cModuleName As String
        Private cProcedureName As String
        Private cInfo As String
        Private oOtherSettings As Hashtable
        Private oException As Exception
        Private nImportance As Integer
#End Region
#Region "Properties"
        Public ReadOnly Property ModuleName() As String
            Get
                Return cModuleName
            End Get
        End Property
        Public ReadOnly Property ProcedureName() As String
            Get
                Return cProcedureName
            End Get
        End Property
        Public ReadOnly Property AddtionalInformation() As String
            Get
                Return cInfo
            End Get
        End Property
        Public ReadOnly Property OtherSettings() As Hashtable
            Get
                Return oOtherSettings
            End Get
        End Property
        Public ReadOnly Property Exception() As Exception
            Get
                Return oException
            End Get
        End Property
        Public ReadOnly Property importance()
            Get
                Return nImportance
            End Get
        End Property
#End Region
        Public Sub New(ByVal [Module] As String, ByVal Procedure As String, ByVal ex As Exception, ByVal Info As String, Optional ByVal importance As Integer = 0, Optional ByVal Settings As Hashtable = Nothing)
            cModuleName = [Module]
            cProcedureName = Procedure
            cInfo = Info
            oOtherSettings = Settings
            oException = ex
        End Sub

        Public Overrides Function ToString() As String
            Dim cMessage As String = ""
            cMessage &= "Module: " & cModuleName
            cMessage &= "Procedure: " & cProcedureName & vbCrLf
            cMessage &= "Exception: " & oException.ToString & vbCrLf
            cMessage &= "Additional Info: " & cInfo

            If Not (oOtherSettings Is Nothing) AndAlso oOtherSettings.Count > 0 Then
                cMessage &= vbCrLf & "Other Settings: " & oOtherSettings.ToString
            End If

            Return cMessage

        End Function
    End Class
End Namespace
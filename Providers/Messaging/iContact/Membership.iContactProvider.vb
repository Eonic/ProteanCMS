'***********************************************************************
' $Library:     eonic.messaging.campaignMonitorProvider
' $Revision:    3.1  
' $Date:        2010-03-02
' $Author:      Trevor Spink (trevor@eonic.co.uk)
' &Website:     www.eonic.co.uk
' &Licence:     All Rights Reserved.
' $Copyright:   Copyright (c) 2002 - 2010 Eonic Ltd.
'***********************************************************************

Option Strict Off
Option Explicit On

Imports System.Xml
Imports System.Web.HttpUtility
Imports System.Web.Configuration
Imports System.Configuration
Imports System.IO
Imports System.Collections
Imports System.Data
Imports System.Data.SqlClient
Imports System.Text.RegularExpressions
Imports System.Threading
Imports Eonic.Web
Imports Eonic.Tools
Imports Eonic.Tools.Xml
Imports System.Net.Mail
Imports System.Reflection
Imports System.Net
Imports VB = Microsoft.VisualBasic


Namespace Providers.Membership

    Public Class iContactProvider

        Public Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)
        Public Event OnErrorWithWeb(ByRef myweb As Eonic.Web, ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)

        Public Sub New()
            'do nothing
        End Sub

        Public Sub Initiate(ByRef _AdminXforms As Object, ByRef _AdminProcess As Object, ByRef _Activities As Object, ByRef MemProvider As Object, ByRef myWeb As Eonic.Web)

            MemProvider.AdminXforms = New AdminXForms(myWeb)
            'MemProvider.AdminProcess = New AdminProcess(myWeb)
            'MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms
            MemProvider.Activities = New Activities()

        End Sub

        Public Class AdminXForms
            Inherits Eonic.Providers.Membership.EonicProvider.AdminXForms
            Private Const mcModuleName As String = "Providers.Messaging.Generic.AdminXForms"
            Public Shadows Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)
            Public Event OnErrorWithWeb(ByRef myweb As Eonic.Web, ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)

            Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/mailinglist")

            Sub New(ByRef aWeb As Web)
                MyBase.New(aWeb)
            End Sub

            Public Overrides Function xFrmUserLogon(Optional ByVal FormName As String = "UserLogon") As XmlElement

                Dim cProcessInfo As String = ""
                Dim bRememberMe As Boolean = False
                Try

                    'We want to validate the user on MySQL first
                    'if we have a corresponding account in eonicweb all good
                    'otherwise create one.

                    'if we have an account in eonicweb but none in MySQL then create one there.

                    Return MyBase.xFrmUserLogon(FormName)

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "xFrmUserLogon", ex, ""))
                    Return Nothing
                End Try
            End Function


            Public Overrides Function xFrmEditDirectoryItem(Optional ByVal id As Long = 0, Optional ByVal cDirectorySchemaName As String = "User", Optional ByVal parId As Long = 0, Optional ByVal cXformName As String = "", Optional ByVal FormXML As String = "") As XmlElement

                Dim cProcessInfo As String = ""
                Dim bRememberMe As Boolean = False
                Try

                    'We want to validate the user on MySQL first
                    'if we have a corresponding account in eonicweb all good
                    'otherwise create one.

                    'if we have an account in eonicweb but none in MySQL then create one there.
                    Dim dirXform As XmlElement

                    MyBase.maintainMembershipsOnAdd = False

                    dirXform = MyBase.xFrmEditDirectoryItem(id, cDirectorySchemaName, parId, cXformName, FormXML)

                    If MyBase.valid Then
                        Select Case cDirectorySchemaName
                            Case "User"
                                Dim iContactApi As New iContactAPI(moMailConfig("ApiID"), moMailConfig("AccountName"), moMailConfig("ApplicationPassword"), moMailConfig("ClientID"), moMailConfig("FolderID"), False, goServer.MapPath("/xsl/iContact/Transforms.xsl"))
                                'does this contact exist ?
                                iContactApi.SyncContact(MyBase.Instance)
                                iContactApi = Nothing
                                MyBase.maintainMembershipsFromXForm(MyBase.mnUserId, , moRequest("cDirName"), True)
                            Case "Group"
                                Dim iContactApi As New iContactAPI(moMailConfig("ApiID"), moMailConfig("AccountName"), moMailConfig("ApplicationPassword"), moMailConfig("ClientID"), moMailConfig("FolderID"), False, goServer.MapPath("/xsl/iContact/Transforms.xsl"))
                                'does this contact exist ?
                                iContactApi.SyncList(MyBase.Instance)
                                iContactApi = Nothing
                        End Select


                    End If

                    Return dirXform

                Catch ex As Exception
                    RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "xFrmUserLogon", ex, ""))
                    Return Nothing
                End Try
            End Function


        End Class

        Public Class AdminProcess
            Inherits Web.Admin
            Public Shadows Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)
            Public Event OnErrorWithWeb(ByRef myweb As Eonic.Web, ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)

            Dim _oAdXfm As AdminXForms


            Public Property oAdXfm() As Object
                Set(ByVal value As Object)
                    _oAdXfm = value
                End Set
                Get
                    Return _oAdXfm
                End Get
            End Property

            Sub New(ByRef aWeb As Web)
                MyBase.New(aWeb)
            End Sub


        End Class

        Public Class Activities
            Inherits Eonic.Providers.Membership.EonicProvider.Activities
            Dim mcModuleName As String = "Eonic.iContact.Providers.Membership.Activities"
            Dim moMailinglistConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/mailinglist")





        End Class
    End Class
End Namespace






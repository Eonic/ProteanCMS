'***********************************************************************
' $Library:     eonic.providers.messaging.base
' $Revision:    3.1  
' $Date:        2010-03-02
' $Author:      Trevor Spink (trevor@eonic.co.uk)
' &Website:     www.eonic.co.uk
' &Licence:     All Rights Reserved.
' $Copyright:   Copyright (c) 2002 - 2010 Eonic Ltd.
'***********************************************************************

Option Strict Off
Option Explicit On

Imports System
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
Imports Eonic.Web.Cart
Imports System.Net.Mail
Imports System.Reflection
Imports System.Net
Imports VB = Microsoft.VisualBasic
Imports Microsoft.WindowsAzure.MediaServices.Client
Imports System.Linq

Namespace Providers
    Namespace Streaming

        Public Class BaseProvider
            Private Const mcModuleName As String = "Eonic.Providers.Streaming.BaseProvider"

            Private _AdminXforms As Object
            Private _AdminProcess As Object
            Private _Activities As Object

            Protected moStreamingCfg As XmlNode

            Public Property AdminXforms() As Object
                Set(ByVal value As Object)
                    _AdminXforms = value
                End Set
                Get
                    Return _AdminXforms
                End Get
            End Property

            Public Property AdminProcess() As Object
                Set(ByVal value As Object)
                    _AdminProcess = value
                End Set
                Get
                    Return _AdminProcess
                End Get
            End Property

            Public Property Activities() As Object
                Set(ByVal value As Object)
                    _Activities = value
                End Set
                Get
                    Return _Activities
                End Get
            End Property

            Public Sub New(ByRef myWeb As Eonic.Web, ByVal ProviderName As String)
                Try
                    Dim calledType As Type
                    Dim oProviderCfg As XmlElement

                    moStreamingCfg = WebConfigurationManager.GetWebApplicationSection("eonic/streaming")
                    oProviderCfg = moStreamingCfg.SelectSingleNode("provider[@name='" & ProviderName & "']")

                    Dim ProviderClass As String = ""
                    If Not oProviderCfg Is Nothing Then
                        If oProviderCfg.HasAttribute("class") Then
                            ProviderClass = oProviderCfg.GetAttribute("class")
                        End If
                    Else
                        'Asssume Eonic Provider
                    End If

                    If ProviderClass = "" Then
                        ProviderClass = "Eonic.Providers.Streaming.EonicProvider"
                        calledType = System.Type.GetType(ProviderClass, True)
                    Else
                        Dim moPrvConfig As Eonic.ProviderSectionHandler = WebConfigurationManager.GetWebApplicationSection("eonic/paymentProviders")
                        If Not moPrvConfig.Providers(ProviderClass) Is Nothing Then
                            Dim assemblyInstance As [Assembly] = [Assembly].Load(moPrvConfig.Providers(ProviderClass).Type)
                            calledType = assemblyInstance.GetType("Eonic.Providers.Payment." & ProviderClass, True)
                        Else
                            calledType = System.Type.GetType("Eonic.Providers.Payment." & ProviderClass, True)
                        End If

                    End If

                    Dim o As Object = Activator.CreateInstance(calledType)

                    Dim args(4) As Object
                    args(0) = _AdminXforms
                    args(1) = _AdminProcess
                    args(2) = _Activities
                    args(3) = Me
                    args(4) = myWeb

                    calledType.InvokeMember("Initiate", BindingFlags.InvokeMethod, Nothing, o, args)

                Catch ex As Exception
                    returnException(mcModuleName, "New", ex, "", ProviderName & " Could Not be Loaded", gbDebug)
                End Try

            End Sub

        End Class

        Public Class EonicProvider

            Public Sub New()
                'do nothing
            End Sub

            Public Sub Initiate(ByRef _AdminXforms As Object, ByRef _AdminProcess As Object, ByRef _Activities As Object, ByRef MemProvider As Object, ByRef myWeb As Eonic.Web)

                MemProvider.AdminXforms = New AdminXForms(myWeb)
                MemProvider.AdminProcess = New AdminProcess(myWeb)
                MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms
                MemProvider.Activities = New Activities()

            End Sub

            Public Class AdminXForms
                Inherits Web.Admin.AdminXforms
                Private Const mcModuleName As String = "Eonic.Providers.Streaming.AdminXForms"

                Sub New(ByRef aWeb As Web)
                    MyBase.New(aWeb)
                End Sub

            End Class

            Public Class AdminProcess
                Inherits Web.Admin

                Dim _oAdXfm As Eonic.Providers.Streaming.EonicProvider.AdminXForms

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
                Private Const mcModuleName As String = "Eonic.Providers.Streaming.Activities"



            End Class

            Public Class Modules

                Public Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)
                Private Const mcModuleName As String = "Eonic.CampaignMonitorTools.Modules"

                Dim moStreamingConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/streaming")


                Public Sub New()

                    'do nowt

                End Sub

                Public Sub GetSteamURL(ByRef myWeb As Eonic.Web, ByRef oContentNode As XmlElement)

                    Try

                    Catch ex As Exception
                        RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "GetSteamURL", ex, ""))
                    End Try
                End Sub
            End Class
        End Class


    End Namespace

End Namespace
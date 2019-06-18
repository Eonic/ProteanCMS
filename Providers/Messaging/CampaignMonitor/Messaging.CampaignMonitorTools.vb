'***********************************************************************
' $Library:     eonic.messaging.CampaignMonitorTools
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

Imports CampaignMonitorAPIWrapper

Public Class CampaignMonitorTools

    Public Sub New()
        'do nothing
    End Sub

    Public Class Modules

        Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)
        Private Const mcModuleName As String = "Protean.CampaignMonitorTools.Modules"

        Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/mailinglist")


        Public Sub New()

            'do nowt
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12

        End Sub

        Public Sub Subscribe(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement)

            Try
                Dim oXform As Protean.Cms.xForm = New Protean.Cms.xForm(myWeb)
                oXform.moPageXML = myWeb.moPageXml
                oXform.load(oContentNode, True)
                If oXform.isSubmitted Then
                    oXform.updateInstanceFromRequest()
                    oXform.validate()
                    If oXform.valid Then

                        'We have an Xform within this content we need to process.
                        Dim listId As String = oContentNode.GetAttribute("listID")
                        If Not oXform.Instance.SelectSingleNode("Subscribe/Items/ListId") Is Nothing Then
                            If oXform.Instance.SelectSingleNode("Subscribe/Items/ListId").InnerText <> "" Then
                                listId = oXform.Instance.SelectSingleNode("Subscribe/Items/ListId").InnerText
                            End If
                        End If
                        Dim apiKey As String = moMailConfig("ApiKey")
                        Dim email As String = oXform.Instance.SelectSingleNode("Subscribe/Items/Email").InnerText
                        Dim name As String = oXform.Instance.SelectSingleNode("Subscribe/Items/Name").InnerText
                        Dim cmAuth As New createsend_dotnet.ApiKeyAuthenticationDetails(moMailConfig("ApiKey"))
                        Dim subscriber As New createsend_dotnet.Subscriber(cmAuth, listId)
                        'lets loop through the nodes to add custom fields
                        Dim i As Integer = 0

                        For Each oElmt In oXform.Instance.SelectNodes("Subscribe/Items/*")
                            If Not (oElmt.Name = "Name" Or oElmt.Name = "Email" Or oElmt.Name = "ListId") Then
                                i = i + 1
                            End If
                        Next
                        Dim customFields As New List(Of createsend_dotnet.SubscriberCustomField)()
                        i = 0
                        For Each oElmt In oXform.Instance.SelectNodes("Subscribe/Items/*")
                            If Not (oElmt.Name = "Name" Or oElmt.Name = "Email" Or oElmt.Name = "ListId") Then
                                Dim customField As New createsend_dotnet.SubscriberCustomField
                                customField.Key = oElmt.Name
                                customField.Value = oElmt.innertext
                                customFields.Add(customField)
                                i = i + 1
                            End If
                        Next
                        Try
                            subscriber.Add(email, name, customFields, False)
                            oXform.RootGroup.InnerXml = "<div class=""subscribed"">" & oXform.Instance.SelectSingleNode("SubscribedMessage").InnerXml & "</div>"
                        Catch ex As Exception
                            oXform.RootGroup.InnerXml = "<div class=""error"">" & ex.Message & "</div>"
                        End Try

                    End If
                End If
                oXform.addValues()
                oXform = Nothing
            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Subscribe", ex, ""))
            End Try
        End Sub

        Public Sub UnSubscribe(ByRef myWeb As Protean.Cms, ByRef oContentNode As XmlElement)

            Try
                Dim oXform As Protean.Cms.xForm = New Protean.Cms.xForm(myWeb)
                oXform.moPageXML = myWeb.moPageXml
                oXform.load(oContentNode, True)
                If oXform.isSubmitted Then
                    oXform.updateInstanceFromRequest()
                    oXform.validate()
                    If oXform.valid Then
                        'We have an Xform within this content we need to process.
                        Dim listId As String = oContentNode.GetAttribute("listID")
                        Dim apiKey As String = moMailConfig("ApiKey")
                        Dim email As String = oXform.Instance.SelectSingleNode("Subscribe/Items/Email").InnerText
                        Dim _api As CampaignMonitorAPIWrapper.CampaignMonitorAPI.api = New CampaignMonitorAPI.api()
                        Dim subscriber As Object 'CampaignMonitorAPIWrapper.CampaignMonitorAPI.Subscriber
                        'lets loop through the nodes to add custom fields
                        Dim customFields(0) As CampaignMonitorAPIWrapper.CampaignMonitorAPI.SubscriberCustomField
                        Dim i As Integer = 0

                        'subscriber = _api.AddSubscriberWithCustomFields(apiKey, listId, email, name, customFields)
                        subscriber = _api.Unsubscribe(apiKey, listId, email)

                        oXform.RootGroup.InnerXml = "<div>" & oXform.Instance.SelectSingleNode("SubscribedMessage").InnerXml & "</div>"
                    End If
                End If
                oXform.addValues()
                oXform = Nothing
            Catch ex As Exception
                RaiseEvent OnError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Subscribe", ex, ""))
            End Try
        End Sub

    End Class

End Class

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

Namespace Providers.Messaging
    Public Class iContactTools

        Public Sub New()
            'do nothing
        End Sub

        Public Class Modules

            Public Event OnError(ByVal sender As Object, ByVal e As Eonic.Tools.Errors.ErrorEventArgs)
            Private Const mcModuleName As String = "Eonic.CampaignMonitorTools.Modules"

            Dim moMailConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/mailinglist")


            Public Sub New()

                'do nowt

            End Sub

            Public Sub Subscribe(ByRef myWeb As Eonic.Web, ByRef oContentNode As XmlElement)

                Try
                    Dim oXform As Eonic.Web.xForm = New Eonic.Web.xForm(myWeb)
                    oXform.moPageXML = myWeb.moPageXml
                    oXform.load(oContentNode, True)
                    If oXform.isSubmitted Then
                        oXform.updateInstanceFromRequest()
                        oXform.validate()
                        If oXform.valid Then
                            'We have an Xform within this content we need to process.
                            Dim iContactApi As New iContactAPI(moMailConfig("ApiID"), moMailConfig("AccountName"), moMailConfig("ApplicationPassword"), moMailConfig("ClientID"), moMailConfig("FolderID"), False, goServer.MapPath("/xsl/iContact/Transforms.xsl"))
                            'does this contact exist ?
                            iContactApi.SyncContact(oXform.Instance, CInt("0" & oContentNode.GetAttribute("listID")))
                            iContactApi = Nothing
                            oXform.RootGroup.InnerXml = "<div>" & oXform.Instance.SelectSingleNode("SubscribedMessage").InnerXml & "</div>"
                        End If
                    End If
                    oXform.addValues()
                    oXform = Nothing
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "Subscribe", ex, ""))
                End Try
            End Sub

            Public Sub UnSubscribe(ByRef myWeb As Eonic.Web, ByRef oContentNode As XmlElement)

                Try
                    'Dim oXform As Eonic.Web.xForm = New Eonic.Web.xForm(myWeb)
                    'oXform.moPageXML = myWeb.moPageXml
                    'oXform.load(oContentNode, True)
                    'If oXform.isSubmitted Then
                    '    oXform.updateInstanceFromRequest()
                    '    oXform.validate()
                    '    If oXform.valid Then
                    '        'We have an Xform within this content we need to process.
                    '        Dim listId As String = oContentNode.GetAttribute("listID")
                    '        Dim apiKey As String = moMailConfig("ApiKey")
                    '        Dim email As String = oXform.Instance.SelectSingleNode("Subscribe/Items/Email").InnerText
                    '        Dim _api As CampaignMonitorAPIWrapper.CampaignMonitorAPI.api = New CampaignMonitorAPI.api()
                    '        Dim subscriber As Object 'CampaignMonitorAPIWrapper.CampaignMonitorAPI.Subscriber
                    '        'lets loop through the nodes to add custom fields
                    '        Dim customFields(0) As CampaignMonitorAPIWrapper.CampaignMonitorAPI.SubscriberCustomField
                    '        Dim i As Integer = 0

                    '        'subscriber = _api.AddSubscriberWithCustomFields(apiKey, listId, email, name, customFields)
                    '        subscriber = _api.Unsubscribe(apiKey, listId, email)

                    '        oXform.RootGroup.InnerXml = "<div>" & oXform.Instance.SelectSingleNode("SubscribedMessage").InnerXml & "</div>"
                    '    End If
                    'End If
                    'oXform.addValues()
                    'oXform = Nothing
                Catch ex As Exception
                    RaiseEvent OnError(Me, New Eonic.Tools.Errors.ErrorEventArgs(mcModuleName, "Subscribe", ex, ""))
                End Try
            End Sub

        End Class

    End Class
End Namespace

'***********************************************************************
' $Library:     Protean.Providers.messaging.base
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
Imports Protean.Cms
Imports Protean.Tools
Imports Protean.Tools.Xml
Imports Protean.Cms.Cart
Imports System.Net.Mail
Imports System.Reflection
Imports System.Net
Imports VB = Microsoft.VisualBasic
Imports System.Linq

Namespace Providers
    Namespace Filter

        Public Class BaseProvider
            Private Const mcModuleName As String = "Protean.Providers.Filter.BaseProvider"

            Private _AdminXforms As Object
            Private _AdminProcess As Object
            Private _Activities As Object

            Protected moFilterCfg As XmlNode

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

            Public Sub New(ByRef myWeb As Protean.Cms, ByVal ProviderName As String)
                Try
                    Dim calledType As Type
                    Dim oProviderCfg As XmlElement

                    moFilterCfg = WebConfigurationManager.GetWebApplicationSection("protean/Filter")
                    oProviderCfg = moFilterCfg.SelectSingleNode("provider[@name='" & ProviderName & "']")

                    Dim ProviderClass As String = ""
                    If Not oProviderCfg Is Nothing Then
                        If oProviderCfg.HasAttribute("class") Then
                            ProviderClass = oProviderCfg.GetAttribute("class")
                        End If
                    Else
                        'Asssume Eonic Provider
                    End If

                    If ProviderClass = "" Then
                        ProviderClass = "Protean.Providers.Filter.DefaultProvider"
                        calledType = System.Type.GetType(ProviderClass, True)
                    Else
                        Dim moPrvConfig As Protean.ProviderSectionHandler = WebConfigurationManager.GetWebApplicationSection("protean/fliterProviders")
                        If Not moPrvConfig.Providers(ProviderClass) Is Nothing Then
                            Dim assemblyInstance As [Assembly] = [Assembly].Load(moPrvConfig.Providers(ProviderClass).Type)
                            calledType = assemblyInstance.GetType("Protean.Providers.Filter." & ProviderClass, True)
                        Else
                            calledType = System.Type.GetType("Protean.Providers.Filter." & ProviderClass, True)
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
                    returnException(myWeb.msException, mcModuleName, "New", ex, "", ProviderName & " Could Not be Loaded", gbDebug)
                End Try

            End Sub

        End Class

        Public Class DefaultProvider

            Public Sub New()
                'do nothing
            End Sub

            Public Sub Initiate(ByRef _AdminXforms As Object, ByRef _AdminProcess As Object, ByRef _Activities As Object, ByRef MemProvider As Object, ByRef myWeb As Protean.Cms)
                MemProvider.AdminXforms = New AdminXForms(myWeb)
                MemProvider.AdminProcess = New AdminProcess(myWeb)
                MemProvider.AdminProcess.oAdXfm = MemProvider.AdminXforms
                '   MemProvider.Activities = New Activities()
                MemProvider.Filters = New Filters()

            End Sub

            Public Class AdminXForms
                Inherits Cms.Admin.AdminXforms
                Private Const mcModuleName As String = "Providers.Providers.Eonic.AdminXForms"

                Sub New(ByRef aWeb As Cms)
                    MyBase.New(aWeb)
                End Sub

            End Class

            Public Class AdminProcess
                Inherits Cms.Admin

                Dim _oAdXfm As Protean.Providers.Filter.DefaultProvider.AdminXForms

                Public Property oAdXfm() As Object
                    Set(ByVal value As Object)
                        _oAdXfm = value
                    End Set
                    Get
                        Return _oAdXfm
                    End Get
                End Property

                Sub New(ByRef aWeb As Cms)
                    MyBase.New(aWeb)
                End Sub
            End Class


            Public Sub DoContentIndex()


            End Sub

            Public Class Filters

                Private Const mcModuleName As String = "Protean.Providers.Filter.DefaultProvider.Filters"

                'Work 3 Ways
                '1. on page build with URL/form/session parameters
                '2. JSON API to return products in JSON for Vue refresh.
                '3. Once all products are rendered, we could show/hide with clientside javascript using XSLT templates only


                Public FilterQueries() As String


                Sub PageFilter(ByRef aWeb As Cms, ByRef oFilterNode As XmlElement)
                    Try

                        Dim nPageId As Integer = 0

                        nPageId = oFilterNode.SelectNodes("Filter/PageId").ToString()
                        Dim cWhereSql As String = String.Empty

                        Dim oMenuItem As XmlElement
                        If (nPageId <> 0) Then
                            'Dim oMenuElmt As XmlElement = aWeb.GetStructureXML(aWeb.mnUserId, nPageId, 0, "Site", False, False, False, True, False, "MenuItem", "Menu")
                            Dim oSubMenuList As XmlNodeList = aWeb.moPageXml.SelectSingleNode("/Page/Menu/MenuItem/descendant-or-self::MenuItem[@id='" & nPageId & "']").SelectNodes("MenuItem")
                            For Each oMenuItem In oSubMenuList
                                cWhereSql = cWhereSql + oMenuItem.Attributes("id").InnerText.ToString() + ","
                            Next
                            'call sp and return xml data
                            If (cWhereSql <> String.Empty) Then

                                cWhereSql = " nStructId IN (" + cWhereSql + ")"
                                aWeb.GetPageContentFromSelect(cWhereSql,,,,,,,,,,, "Product")
                            End If

                        End If

                    Catch ex As Exception

                    End Try
                End Sub

                Sub PriceRangeFilter(ByRef aWeb As Cms, ByRef oFilterNode As XmlElement)

                    ' sets a low and high price returns 

                End Sub


                Sub BrandFilter(ByRef aWeb As Cms, ByRef oFilterNode As XmlElement)
                    ' will not be required for ITB


                End Sub

                Sub InStockFilter(ByRef aWeb As Cms, ByRef oFilterNode As XmlElement)
                    ' will not be required for ITB


                End Sub


            End Class


        End Class


    End Namespace

End Namespace
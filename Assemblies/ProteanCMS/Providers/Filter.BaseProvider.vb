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

            Protected moStreamingCfg As XmlNode

            Public Sub New(ByRef myWeb As Protean.Cms, ByVal ProviderName As String)
                Try
                    Dim calledType As Type
                    Dim oProviderCfg As XmlElement

                    moStreamingCfg = WebConfigurationManager.GetWebApplicationSection("protean/streaming")
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
                        ProviderClass = "Protean.Providers.Streaming.EonicProvider"
                        calledType = System.Type.GetType(ProviderClass, True)
                    Else
                        Dim moPrvConfig As Protean.ProviderSectionHandler = WebConfigurationManager.GetWebApplicationSection("protean/paymentProviders")
                        If Not moPrvConfig.Providers(ProviderClass) Is Nothing Then
                            Dim assemblyInstance As [Assembly] = [Assembly].Load(moPrvConfig.Providers(ProviderClass).Type)
                            calledType = assemblyInstance.GetType("Protean.Providers.Payment." & ProviderClass, True)
                        Else
                            calledType = System.Type.GetType("Protean.Providers.Payment." & ProviderClass, True)
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


            End Sub


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

                    'This pulls in all the products to be subsequently filtered.

                    'Removes the Products on the current page to be replaced by the products in this query.

                    'additive
                    'Pull in products from other pages specificed
                    'Pull in product form all child pages by default
                    'Also list the child pages within the filterNode to easily render the control on the page

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
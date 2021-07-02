Option Strict Off
Option Explicit On


Imports System.Web.Configuration

Public Module Startup

    Public Sub Go()

        JSStart.InitialiseJSEngine()

        ClearXSLCache()

    End Sub

    Public Sub ClearXSLCache()

        Dim compiledFolder As String = "\xsltc\"
        Dim sProcessInfo As String = ""
        Dim myServer As System.Web.HttpServerUtility = System.Web.HttpContext.Current.Server
        Dim myConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("protean/web")

        Dim oImp As Protean.Tools.Security.Impersonate = New Protean.Tools.Security.Impersonate
        If oImp.ImpersonateValidUser(myConfig("AdminAcct"), myConfig("AdminDomain"), myConfig("AdminPassword"), True, myConfig("AdminGroup")) Then

            Dim cWorkingDirectory As String = myServer.MapPath(compiledFolder)
            sProcessInfo = "clearing " & cWorkingDirectory
            Dim di As New IO.DirectoryInfo(cWorkingDirectory)
            Dim fi As IO.FileInfo

            For Each fi In di.EnumerateFiles
                Try

                    Dim oFileInfo As IO.FileInfo = New IO.FileInfo(fi.FullName)
                    oFileInfo.IsReadOnly = False
                    IO.File.Delete(fi.FullName)

                Catch ex2 As Exception
                    ' returnException("Protean.XmlHelper.Transform", "ClearXSLTassemblyCache", ex2, msXslFile, sProcessInfo)
                End Try
            Next

            oImp.UndoImpersonation()

        End If

    End Sub

End Module

Public Module JSStart

    Public Sub InitialiseJSEngine()
        Dim sProcessInfo As String = "V8JsEngine"
        Dim moApp As System.Web.HttpApplicationState = Nothing
        If Not System.Web.HttpContext.Current Is Nothing Then
            moApp = System.Web.HttpContext.Current.Application
        End If

        Try
            If Not moApp Is Nothing Then
                'ensures we have configured JSEngine for Bundle Transformer
                If moApp("JSEngineEnabled") Is Nothing Then
                    '  Dim msieCfg As New JavaScriptEngineSwitcher.Msie.MsieSettings()
                    '  msieCfg.EngineMode = JavaScriptEngineSwitcher.Msie.JsEngineMode.ChakraIeJsRt
                    Dim engineSwitcher As JavaScriptEngineSwitcher.Core.JsEngineSwitcher = JavaScriptEngineSwitcher.Core.JsEngineSwitcher.Current
                    'engineSwitcher.EngineFactories.Add(New JavaScriptEngineSwitcher.ChakraCore.ChakraCoreJsEngineFactory())
                    '  engineSwitcher.EngineFactories.Add(New JavaScriptEngineSwitcher.Msie.MsieJsEngineFactory(msieCfg))
                    engineSwitcher.EngineFactories.Add(New JavaScriptEngineSwitcher.V8.V8JsEngineFactory())
                    Dim sJsEngine As String = "V8JsEngine"
                    'Dim sJsEngine As String = "MsieJsEngine"

                    'Dim sJsEngine As String = "ChakraCoreJsEngine"
                    '   If moConfig("JSEngine") <> "" Then
                    '   sJsEngine = moConfig("JSEngine")
                    '    End If
                    engineSwitcher.DefaultEngineName = sJsEngine
                    moApp("JSEngineEnabled") = sJsEngine
                End If
            End If
        Catch ex As Exception
            sProcessInfo = ex.Message
            ' OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "InitialiseJSEngine", ex, sProcessInfo))
        End Try

    End Sub

End Module

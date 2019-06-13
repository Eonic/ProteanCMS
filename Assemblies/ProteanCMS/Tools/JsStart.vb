Option Strict Off
Option Explicit On
Public Module JSStart



    Public Sub InitialiseJSEngine()
        Dim sProcessInfo As String = "V8JsEngine"
        Dim moApp As System.Web.HttpApplicationState = System.Web.HttpContext.Current.Application
        Try
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
        Catch ex As Exception
            ' OnComponentError(Me, New Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "InitialiseJSEngine", ex, sProcessInfo))
        End Try

    End Sub

End Module

﻿<%@ Application Language="VB" %>

<script runat="server">

    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' Code that runs on application startup
        Dim engineSwitcher As JavaScriptEngineSwitcher.Core.JsEngineSwitcher = JavaScriptEngineSwitcher.Core.JsEngineSwitcher.Current
        engineSwitcher.EngineFactories.Add(New JavaScriptEngineSwitcher.V8.V8JsEngineFactory())
        Dim sJsEngine As String = "V8JsEngine"
        engineSwitcher.DefaultEngineName = sJsEngine
    End Sub

    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
        ' Code that runs on application shutdown
    End Sub

    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
        ' Code that runs when an unhandled error occurs
    End Sub

    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' Code that runs when a new session is started
    End Sub

    Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)
        ' Code that runs when a session ends. 
        ' Note: The Session_End event is raised only when the sessionstate mode
        ' is set to InProc in the Web.config file. If session mode is set to StateServer 
        ' or SQLServer, the event is not raised.
    End Sub

</script>
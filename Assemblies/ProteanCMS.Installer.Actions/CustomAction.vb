Imports System.ComponentModel
Imports System.Configuration.Install
Imports System.Web
Imports System.Web.Configuration
Imports System.Configuration
Imports System.Xml
Imports Microsoft.Web.Administration
Imports System.windows.forms


Public Class CustomActions

    Public Shared ewAssemblyVersion As String = "6.0.55.0"
    Public Shared ptnAppStartAssemblyVersion As String = "6.0.0.0"
    Public Shared bundleAssemblyVersion As String = "1.10.0.0"
    Public Shared bundleLessAssemblyVersion As String = "1.12.44.0"
    Public Shared bundleSassAssemblyVersion As String = "1.12.36.0"
    Public Shared LibSassHostAssemblyVersion As String = "1.3.3.0"
    Public Shared jsSwitcherAssemblyVersion As String = "3.3.0.0"
    Public Shared jsSwitcherV8AssemblyVersion As String = "3.17.4.0"
    Public Shared jsSwitcherV8NativeWinx64AssemblyVersion As String = "3.17.4.0"
    Public Shared WebGreaseAssemblyVersion As String = "1.6.5135.21930"
    Public Shared JsonAssemblyVersion As String = "13.0.0.0" '"8.0.1.19229""
    Public Shared YUIAssemblyVersion As String = "1.9.23.0"
    Public Shared MicrosoftAjaxAssemblyVersion As String = "1.10.0.0"
    Public Shared AjaxMinAssemblyVersion As String = "5.14.5506.26196"
    Public Shared ECMAAssemblyVersion As String = "1.0.1.0"
    Public Shared DynamicImagePDFAssemblyVersion As String = "1.0.0.4"
    Public Shared SystemNetFTPClientAssemblyVersion As String = "1.0.5824.34026"
    Public Shared SystemTextEncodingCodePagesAssemblyVersion As String = "4.0.2.0"
    'We have real problems with this specific DLL' the one PreMailer.Net references is 4.6.0.0 but installs as 4.0.2.0 in GAC therefore old version encompasses 4.6.0.0 to scale back to 4.0.2.0
    Public Shared CreateSendAssemblyVersion As String = "4.2.2.0"
    Public Shared TidyHTML5ManagedAssemblyVersion As String = "1.1.5.0"
    Public Shared ClearScriptAssemblyVersion As String = "5.5.6.0"
    Public Shared MicrosoftClearScriptV8AssemblyVersion As String = "7.2.4.0"
    Public Shared AlphaFSAssemblyVersion As String = "2.2.0.0"
    Public Shared MagickNETAssemblyVersion As String = "8.5.0.0"
    Public Shared MagickNETCoreAssemblyVersion As String = "11.1.0.0"

    Public Shared GoogleProtoBufAssemblyVersion As String = "3.20.1.0"
    Public Shared SharpZipLibAssemblyVersion As String = "1.3.3.11"
    Public Shared SystemBuffersVersion As String = "4.0.3.0"

    <CustomAction()> _
    Public Shared Function LoadGuide(ByVal session As Session) As ActionResult
        session.Log("Begin CustomAction1")
        System.Diagnostics.Process.Start("http://www.ProteanCMS.com/Support/Web-Designers-Guide/Installing-ProteanCMS")
        Return ActionResult.Success
    End Function

    <CustomAction()> _
    Public Shared Function UnInstall(ByVal session As Session) As ActionResult

        Try

            System.Diagnostics.Process.Start("http://www.ProteanCMS.com/Support/Web-Designers-Guide/UnInstalling-ProteanCMS")

            Return ActionResult.Success

        Catch ex As Exception
            Dim errorstr As String = ex.InnerException.StackTrace
            System.Diagnostics.Process.Start("http://www.ProteanCMS.com/Support/Web-Designers-Guide/UnInstalling-ProteanCMS")
            Return ActionResult.Failure
        End Try
    End Function

    <CustomAction()>
    Public Shared Function Install(ByVal session As Session) As ActionResult

        Try

            ' System.Diagnostics.Debugger.Launch()

            ' MessageBox.Show("Please attach a debugger.")

            'Edit the ApplicationHost.config in 
            'no edits required right now.

            'Edit the machine level web.Config to include...

            Dim machineConfig As System.Configuration.Configuration = System.Web.Configuration.WebConfigurationManager.OpenMachineConfiguration()
            Dim FilePath As String = machineConfig.FilePath
            Dim FilePath64 As String = machineConfig.FilePath.Replace("Framework", "Framework64")

            Dim machineWebConfig As System.Configuration.Configuration = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(Nothing)

            Dim WebFilePaths(1) As String
            WebFilePaths(0) = machineWebConfig.FilePath
            WebFilePaths(1) = machineWebConfig.FilePath.Replace("Framework", "Framework64")

            'Dim webConfig64 As System.Configuration.Configuration = WebConfigurationManager.OpenMappedWebConfiguration(webConfigFile64, Nothing)
            Dim WebFilePath As String
            For Each WebFilePath In WebFilePaths

                Dim ConfigRoot As XmlElement

                Dim webConfig As New XmlDocument
                webConfig.Load(WebFilePath)
                ConfigRoot = webConfig.SelectSingleNode("configuration")
                Dim oCgfSect As XmlElement = webConfig.SelectSingleNode("configuration/configSections")

                If oCgfSect Is Nothing Then
                    'create runtime
                    oCgfSect = webConfig.CreateElement("configSections")
                    ConfigRoot.InsertBefore(oCgfSect, ConfigRoot.FirstChild)

                End If

                UpdateConfigSection(oCgfSect, "protean", "web", "System.Configuration.NameValueFileSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", False)
                UpdateConfigSection(oCgfSect, "protean", "cart", "System.Configuration.NameValueFileSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", False)
                UpdateConfigSection(oCgfSect, "protean", "quote", "System.Configuration.NameValueFileSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", False)
                UpdateConfigSection(oCgfSect, "protean", "mailinglist", "System.Configuration.NameValueFileSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", False)
                UpdateConfigSection(oCgfSect, "protean", "scheduler", "System.Configuration.NameValueFileSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", False)
                UpdateConfigSection(oCgfSect, "protean", "subscriptions", "System.Configuration.NameValueFileSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", False)
                UpdateConfigSection(oCgfSect, "protean", "custom", "System.Configuration.NameValueFileSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", False)
                UpdateConfigSection(oCgfSect, "protean", "synchronisation", "System.Configuration.NameValueFileSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", False)
                UpdateConfigSection(oCgfSect, "protean", "alerts", "System.Configuration.NameValueFileSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", False)
                UpdateConfigSection(oCgfSect, "protean", "versioncontrol", "System.Configuration.NameValueFileSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", False)
                UpdateConfigSection(oCgfSect, "protean", "theme", "System.Configuration.NameValueFileSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", False)
                UpdateConfigSection(oCgfSect, "protean", "lms", "System.Configuration.NameValueFileSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", False)
                ', Version=" & ewAssemblyVersion & ", Culture=neutral, PublicKeyToken=0e5e11efc3341916
                UpdateConfigSection(oCgfSect, "protean", "messagingProviders", "Protean.ProviderSectionHandler, ProteanCMS", False)
                UpdateConfigSection(oCgfSect, "protean", "paymentProviders", "Protean.ProviderSectionHandler, ProteanCMS", False)
                UpdateConfigSection(oCgfSect, "protean", "membershipProviders", "Protean.ProviderSectionHandler, ProteanCMS", False)
                UpdateConfigSection(oCgfSect, "protean", "languages", "Protean.XmlSectionHandler, ProteanCMS", False)
                UpdateConfigSection(oCgfSect, "protean", "payment", "Protean.XmlSectionHandler, ProteanCMS", False)
                UpdateConfigSection(oCgfSect, "protean", "PasswordPolicy", "Protean.XmlSectionHandler, ProteanCMS", False)

                UpdateConfigSection(oCgfSect, "bundleTransformer", "core", "BundleTransformer.Core.Configuration.CoreSettings, BundleTransformer.Core, Version=" & bundleAssemblyVersion & ", Culture=neutral, PublicKeyToken=973C344C93AAC60D", False)
                UpdateConfigSection(oCgfSect, "bundleTransformer", "less", "BundleTransformer.Less.Configuration.LessSettings, BundleTransformer.Less, Version=" & bundleLessAssemblyVersion & ", Culture=neutral, PublicKeyToken=973C344C93AAC60D", False)
                UpdateConfigSection(oCgfSect, "bundleTransformer", "sassAndScss", "BundleTransformer.SassAndScss.Configuration.SassAndScssSettings, BundleTransformer.SassAndScss, Version=" & bundleSassAssemblyVersion & ", Culture=neutral, PublicKeyToken=973C344C93AAC60D", False)

                '       UpdateConfigSection(oCgfSect, "bundleTransformer", "yui", "BundleTransformer.Yui.Configuration.YuiSettings, BundleTransformer.Yui, Version=" & YUIAssemblyVersion & ", Culture=neutral, PublicKeyToken=973C344C93AAC60D", False)
                UpdateConfigSection(oCgfSect, "bundleTransformer", "microsoftAjax", "BundleTransformer.MicrosoftAjax.Configuration.MicrosoftAjaxSettings, BundleTransformer.MicrosoftAjax, Version=" & MicrosoftAjaxAssemblyVersion & ", Culture=neutral, PublicKeyToken=973C344C93AAC60D", False)



                'Add the globally Installed providers
                AddProvider(ConfigRoot, "protean", "Messaging", "CampaignMonitor", "Protean.Providers.Messaging.CampaignMonitor, Version=" & ewAssemblyVersion & ", Culture=neutral, PublicKeyToken=0e5e11efc3341916")

                Dim oCgfProteanSect As XmlElement = webConfig.SelectSingleNode("configuration/protean")

                If oCgfProteanSect Is Nothing Then
                    'create section
                    oCgfProteanSect = webConfig.CreateElement("protean")

                    'server level web settings
                    Dim oCgfProteanCMSSect As XmlElement = webConfig.CreateElement("web")
                    AddConfigKey(oCgfProteanCMSSect, "AdminAcct", "")
                    AddConfigKey(oCgfProteanCMSSect, "AdminDomain", "")
                    AddConfigKey(oCgfProteanCMSSect, "AdminPassword", "")
                    AddConfigKey(oCgfProteanCMSSect, "AdminGroup", "")
                    AddConfigKey(oCgfProteanCMSSect, "MailServer", "")
                    AddConfigKey(oCgfProteanCMSSect, "SoapIPs", "")
                    AddConfigKey(oCgfProteanCMSSect, "DatabaseType", "SQL")
                    AddConfigKey(oCgfProteanCMSSect, "DatabaseServer", "")
                    AddConfigKey(oCgfProteanCMSSect, "DatabaseAuth", "")
                    AddConfigKey(oCgfProteanCMSSect, "DatabaseUsername", "")
                    AddConfigKey(oCgfProteanCMSSect, "DatabasePassword", "")
                    AddConfigKey(oCgfProteanCMSSect, "DatabaseFtpUsername", "")
                    AddConfigKey(oCgfProteanCMSSect, "DatabaseFtpPassword", "")
                    AddConfigKey(oCgfProteanCMSSect, "ServerName", "")
                    AddConfigKey(oCgfProteanCMSSect, "CommonDirectoryPath", "")
                    oCgfProteanSect.AppendChild(oCgfProteanCMSSect)

                    Dim oCgfProteanSchedSect As XmlElement = webConfig.CreateElement("scheduler")
                    AddConfigKey(oCgfProteanSchedSect, "DatabaseServer", "")
                    AddConfigKey(oCgfProteanSchedSect, "DatabaseName", "")
                    AddConfigKey(oCgfProteanSchedSect, "DatabaseAuth", "")
                    AddConfigKey(oCgfProteanSchedSect, "SchedulerMonitorXsl", "")
                    AddConfigKey(oCgfProteanSchedSect, "SchedulerMonitorEmail", "")
                    oCgfProteanSect.AppendChild(oCgfProteanSchedSect)

                    ConfigRoot.InsertAfter(oCgfProteanSect, oCgfSect)

                End If

                'Add Password Policy
                Dim oCgfPasswordSect As XmlElement = webConfig.SelectSingleNode("configuration/protean/PasswordPolicy")
                If oCgfPasswordSect Is Nothing Then
                    'create section
                    oCgfPasswordSect = webConfig.CreateElement("PasswordPolicy")
                    oCgfPasswordSect.InnerXml = "<Password><retrys>0</retrys><minLength>6</minLength><maxLength>25</maxLength><numsLength>1</numsLength><upperLength>1</upperLength><specialLength>0</specialLength><barWidth>200</barWidth><barColor>Green</barColor><specialChars>!@#\\$%*()_+^&amp;}{:;?.</specialChars><useMultipleColors>1</useMultipleColors><blockHistoricPassword>0</blockHistoricPassword></Password>"
                    oCgfProteanSect.AppendChild(oCgfPasswordSect)
                End If

                'TO DO !!! Check request validation mode
                '<httpRuntime requestValidationMode="2.0" />

                'Update the Assemblies
                Dim oAssembliesSect As XmlElement = webConfig.SelectSingleNode("/configuration/system.web/compilation/assemblies")
                If oAssembliesSect Is Nothing Then
                    Dim oerr As String = "Oh CRAP ! Missed again"
                Else
                    '  Don't want a global assembly ref for ProteanCMS makes impossible to overload.
                    '  UpdateAssemblyRef(oAssembliesSect, "ProteanCMS, Version=" & ewAssemblyVersion & ", Culture=neutral, PublicKeyToken=0e5e11efc3341916")
                    UpdateAssemblyRef(oAssembliesSect, "BundleTransformer.Core, Version=" & bundleAssemblyVersion & ", Culture=neutral, PublicKeyToken=973C344C93AAC60D")
                    UpdateAssemblyRef(oAssembliesSect, "BundleTransformer.Less, Version=" & bundleLessAssemblyVersion & ", Culture=neutral, PublicKeyToken=973C344C93AAC60D")
                    UpdateAssemblyRef(oAssembliesSect, "BundleTransformer.SassAndScss, Version=" & bundleSassAssemblyVersion & ", Culture=neutral, PublicKeyToken=973C344C93AAC60D")
                    UpdateAssemblyRef(oAssembliesSect, "LibSassHost, Version=" & LibSassHostAssemblyVersion & ", Culture=neutral, PublicKeyToken=3e24e88796a38e46")
                    ' Not required for runtimes   UpdateAssemblyRef(oAssembliesSect, "LibSassHost.Native.win-x64, Version=" & LibSassHostAssemblyVersion & ", Culture=neutral, PublicKeyToken=3e24e88796a38e46")

                    UpdateAssemblyRef(oAssembliesSect, "BundleTransformer.MicrosoftAjax, Version=" & MicrosoftAjaxAssemblyVersion & ", Culture=neutral, PublicKeyToken=973C344C93AAC60D")
                    UpdateAssemblyRef(oAssembliesSect, "AjaxMin, Version=" & AjaxMinAssemblyVersion & ", Culture=neutral, PublicKeyToken=21ef50ce11b5d80f")
                    UpdateAssemblyRef(oAssembliesSect, "JavaScriptEngineSwitcher.Core, Version=" & jsSwitcherAssemblyVersion & ", Culture=neutral, PublicKeyToken=C608B2A8CC9E4472")
                    'UpdateAssemblyRef(oAssembliesSect, "JavaScriptEngineSwitcher.Msie, Version=" & jsSwitcherMSIEAssemblyVersion & ", Culture=neutral, PublicKeyToken=C608B2A8CC9E4472")
                    'UpdateAssemblyRef(oAssembliesSect, "JavaScriptEngineSwitcher.ChakraCore, Version=" & jsSwitcherChakraAssemblyVersion & ", Culture=neutral, PublicKeyToken=C608B2A8CC9E4472")
                    UpdateAssemblyRef(oAssembliesSect, "JavaScriptEngineSwitcher.V8, Version=" & jsSwitcherV8AssemblyVersion & ", Culture=neutral, PublicKeyToken=C608B2A8CC9E4472")
                    ' Not required for runtimes      UpdateAssemblyRef(oAssembliesSect, "JavaScriptEngineSwitcher.V8.Native.win-x64, Version=" & jsSwitcherV8NativeWinx64AssemblyVersion & ", Culture=neutral, PublicKeyToken=C608B2A8CC9E4472")
                    UpdateAssemblyRef(oAssembliesSect, "ClearScript.Core, Version=" & MicrosoftClearScriptV8AssemblyVersion & ", Culture=neutral, PublicKeyToken=31bf3856ad364e35")
                    UpdateAssemblyRef(oAssembliesSect, "ClearScript.V8, Version=" & MicrosoftClearScriptV8AssemblyVersion & ", Culture=neutral, PublicKeyToken=31bf3856ad364e35")

                    ' UpdateAssemblyRef(oAssembliesSect, "MsieJavaScriptEngine, Version=" & MSIEJsEngineAssemblyVersion & ", Culture=neutral, PublicKeyToken=A3A2846A37AC0D3E")
                    ' UpdateAssemblyRef(oAssembliesSect, "EcmaScript.NET, Version=" & ECMAAssemblyVersion & ", Culture=neutral, PublicKeyToken=0e5e11efc3341916")

                    UpdateAssemblyRef(oAssembliesSect, "WebGrease, Version=" & WebGreaseAssemblyVersion & ", Culture=neutral, PublicKeyToken=31BF3856AD364E35")
                    UpdateAssemblyRef(oAssembliesSect, "Newtonsoft.Json, Version=" & JsonAssemblyVersion & ", Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed")
                    UpdateAssemblyRef(oAssembliesSect, "System.Net.FtpClient, Version=" & SystemNetFTPClientAssemblyVersion & ", Culture=neutral, PublicKeyToken=fa4be07daa57c2b7")
                    UpdateAssemblyRef(oAssembliesSect, "TidyHTML5Managed, Version=" & TidyHTML5ManagedAssemblyVersion & ", Culture=neutral, PublicKeyToken=0e5e11efc3341916")
                    UpdateAssemblyRef(oAssembliesSect, "Protean.AppStart, Version=" & ptnAppStartAssemblyVersion & ", Culture=neutral, PublicKeyToken=0e5e11efc3341916")

                    UpdateAssemblyRef(oAssembliesSect, "TidyHTML5Managed, Version=" & TidyHTML5ManagedAssemblyVersion & ", Culture=neutral, PublicKeyToken=0e5e11efc3341916")


                    UpdateAssemblyRef(oAssembliesSect, "ICSharpCode.SharpZipLib, Version=" & SharpZipLibAssemblyVersion & ", Culture=neutral, PublicKeyToken=1b03e6acf1164f73")
                    UpdateAssemblyRef(oAssembliesSect, "Google.Protobuf, Version=" & GoogleProtoBufAssemblyVersion & ", Culture=neutral, PublicKeyToken=a7d26565bac4d604")

                    UpdateAssemblyRef(oAssembliesSect, "Magick.NET.Core, Version=" & MagickNETCoreAssemblyVersion & ", Culture=neutral, PublicKeyToken=2004825badfa91ec")
                    UpdateAssemblyRef(oAssembliesSect, "Magick.NET-Q8-AnyCPU, Version=" & MagickNETAssemblyVersion & ", Culture=neutral, PublicKeyToken=2004825badfa91ec")

                    UpdateAssemblyRef(oAssembliesSect, "System.Buffers, Version=" & SystemBuffersVersion & ", Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51")

                    ' UpdateAssemblyRef(oAssembliesSect, "ProteanCms, Version=" & ewAssemblyVersion & ", Culture=neutral, PublicKeyToken=0e5e11efc3341916")
                    ' UpdateAssemblyRef(oAssembliesSect, "Protean.Tools, Version=" & ewAssemblyVersion & ", Culture=neutral, PublicKeyToken=2030ce1af675e93f")
                    ' UpdateAssemblyRef(oAssembliesSect, "Protean.Tools.Csharp, Version=" & ewAssemblyVersion & ", Culture=neutral, PublicKeyToken=0e5e11efc3341916")
                End If

                'UpdateAssemblyRef(oAssembliesSect, "Antlr3.Runtime, Version=3.5.0.2, Culture=neutral, PublicKeyToken=EB42632606E9261F")

                'Update the Less Handler

                Dim ohttpHandlers As XmlElement = webConfig.SelectSingleNode("/configuration/system.web/httpHandlers")
                Dim oHandler As XmlElement
                If ohttpHandlers.SelectSingleNode("add[@path='*.less']") Is Nothing Then
                    oHandler = webConfig.CreateElement("add")
                    ohttpHandlers.AppendChild(oHandler)
                Else
                    oHandler = ohttpHandlers.SelectSingleNode("add[@path='*.less']")
                End If
                oHandler.SetAttribute("path", "*.less")
                oHandler.SetAttribute("verb", "GET")
                oHandler.SetAttribute("type", "BundleTransformer.Less.HttpHandlers.LessAssetHandler, BundleTransformer.Less, Version=" & bundleLessAssemblyVersion & ", Culture=neutral, PublicKeyToken=973C344C93AAC60D")
                oHandler.SetAttribute("validate", "True")

                'add Bundle Transformer config
                'quick fiddle to avoid namespace
                Dim obt As XmlElement = webConfig.SelectSingleNode("/configuration/*[local-name() = 'bundleTransformer']")
                If obt Is Nothing Then
                    obt = webConfig.CreateElement("bundleTransformer")
                    webConfig.DocumentElement.AppendChild(obt)
                End If
                obt.InnerXml = "<core>" &
                                "<css defaultPostProcessors=""UrlRewritingCssPostProcessor"" defaultMinifier=""MicrosoftAjaxCssMinifier"" usePreMinifiedFiles=""true"" combineFilesBeforeMinification = ""false"" > " &
                                "<minifiers>" &
                                "<add name=""NullMinifier"" type=""BundleTransformer.Core.Minifiers.NullMinifier, BundleTransformer.Core, Version=" & bundleAssemblyVersion & ", Culture=neutral, PublicKeyToken=973C344C93AAC60D"" />" &
                                "<add name=""MicrosoftAjaxCssMinifier"" type=""BundleTransformer.MicrosoftAjax.Minifiers.MicrosoftAjaxCssMinifier, BundleTransformer.MicrosoftAjax, Version=" & MicrosoftAjaxAssemblyVersion & ", Culture=neutral, PublicKeyToken=973C344C93AAC60D"" />" &
                                "</minifiers>" &
                                "<translators>" &
                                "<add name=""NullTranslator"" type=""BundleTransformer.Core.Translators.NullTranslator, BundleTransformer.Core, Version=" & bundleAssemblyVersion & ", Culture=neutral, PublicKeyToken=973C344C93AAC60D"" enabled=""false"" />" &
                                "<add name=""LessTranslator"" type=""BundleTransformer.Less.Translators.LessTranslator, BundleTransformer.Less, Version=" & bundleLessAssemblyVersion & ", Culture=neutral, PublicKeyToken=973C344C93AAC60D"" />" &
                                "<add name=""SassAndScssTranslator"" type=""BundleTransformer.SassAndScss.Translators.SassAndScssTranslator, BundleTransformer.SassAndScss, Version=" & bundleSassAssemblyVersion & ", Culture=neutral, PublicKeyToken=973C344C93AAC60D""/>" &
                                "</translators>" &
                                "<postProcessors>" &
                                    "<add name=""UrlRewritingCssPostProcessor"" type=""BundleTransformer.Core.PostProcessors.UrlRewritingCssPostProcessor, BundleTransformer.Core, Version=" & bundleAssemblyVersion & ", Culture=neutral, PublicKeyToken=973C344C93AAC60D"" useInDebugMode=""false""/>" &
                                "</postProcessors>" &
                                "<fileExtensions>" &
                                    "<add fileExtension="".css"" assetTypeCode=""Css""/>" &
                                    "<add fileExtension="".less"" assetTypeCode=""Less""/>" &
                                    "<add fileExtension="".sass"" assetTypeCode=""Sass""/>" &
                                    "<add fileExtension="".scss"" assetTypeCode=""Scss""/>" &
                                "</fileExtensions>" &
                                "</css>" &
                               "<js defaultPostProcessors="""" defaultMinifier=""MicrosoftAjaxJsMinifier"" usePreMinifiedFiles=""true"" combineFilesBeforeMinification = ""false"" > " &
                                   "<minifiers>" &
                                       "<add name=""NullMinifier"" type=""BundleTransformer.Core.Minifiers.NullMinifier, BundleTransformer.Core, Version=" & bundleAssemblyVersion & ", Culture=neutral, PublicKeyToken=973C344C93AAC60D""/>" &
                                       "<add name=""MicrosoftAjaxJsMinifier"" type=""BundleTransformer.MicrosoftAjax.Minifiers.MicrosoftAjaxJsMinifier, BundleTransformer.MicrosoftAjax, Version=" & MicrosoftAjaxAssemblyVersion & ", Culture=neutral, PublicKeyToken=973C344C93AAC60D"" />" &
                                   "</minifiers>" &
                                   "<translators>" &
                                       "<add name=""NullTranslator"" type=""BundleTransformer.Core.Translators.NullTranslator, BundleTransformer.Core, Version=" & bundleAssemblyVersion & ", Culture=neutral, PublicKeyToken=973C344C93AAC60D"" enabled=""false""/>" &
                                   "</translators>" &
                                   "<fileExtensions>" &
                                       "<add fileExtension="".js"" assetTypeCode=""JavaScript""/>" &
                                   "</fileExtensions>" &
                               "</js>" &
                                "</core>" &
                                "<less useNativeMinification=""true"" ieCompat=""true"" strictUnits=""false"" dumpLineNumbers=""None"" javascriptEnabled=""true"">" &
                                "<jsEngine name=""V8JsEngine"" />" &
                                "</less>"

                obt.SetAttribute("xmlns", "http://tempuri.org/BundleTransformer.Configuration.xsd")


                'quick fiddle to avoid namespace
                Dim jsEng As XmlElement = webConfig.SelectSingleNode("configuration/*[local-name() = 'jsEngineSwitcher']")
                If Not jsEng Is Nothing Then
                    webConfig.SelectSingleNode("configuration").RemoveChild(jsEng)
                End If

                RemoveConfigSection(oCgfSect, "jsEngineSwitcher", "core")
                RemoveConfigSection(oCgfSect, "jsEngineSwitcher", "msie")

                ' jsEng = webConfig.CreateElement("jsEngineSwitcher")
                '     webConfig.DocumentElement.AppendChild(jsEng)
                ' End If
                ' jsEng.InnerXml = "<core><engines><add name=""MsieJsEngine"" type=""JavaScriptEngineSwitcher.Msie.MsieJsEngine, JavaScriptEngineSwitcher.Msie, Version=" & jsSwitcherMSIEAssemblyVersion & ", Culture=neutral, PublicKeyToken=C608B2A8CC9E4472"" /></engines></core>"
                ' jsEng.SetAttribute("xmlns", "http://tempuri.org/JavaScriptEngineSwitcher.Configuration.xsd")




                webConfig.Save(WebFilePath)

            Next


            '' and the WebService Bindings....

            ''    <bindings>
            ''        <basicHttpBinding>
            ''            <binding name="ewAdminProxySoap" closeTimeout="00:01:00" openTimeout="00:01:00"
            ''                receiveTimeout="00:10:00" sendTimeout="00:01:00" allowCookies="false"
            ''                bypassProxyOnLocal="false" hostNameComparisonMode="StrongWildcard"
            ''                maxBufferSize="65536" maxBufferPoolSize="524288" maxReceivedMessageSize="65536"
            ''                messageEncoding="Text" textEncoding="utf-8" transferMode="Buffered"
            ''                useDefaultWebProxy="true">
            ''                <readerQuotas maxDepth="32" maxStringContentLength="8192" maxArrayLength="16384"
            ''                    maxBytesPerRead="4096" maxNameTableCharCount="16384" />
            ''                <security mode="None">
            ''                    <transport clientCredentialType="None" proxyCredentialType="None"
            ''                        realm="" />
            ''                    <message clientCredentialType="UserName" algorithmSuite="Default" />
            ''                </security>
            ''            </binding>
            ''            <binding name="PayPalAPISoapBinding" closeTimeout="00:01:00"
            ''                openTimeout="00:01:00" receiveTimeout="00:10:00" sendTimeout="00:01:00"
            ''                allowCookies="false" bypassProxyOnLocal="false" hostNameComparisonMode="StrongWildcard"
            ''                maxBufferSize="65536" maxBufferPoolSize="524288" maxReceivedMessageSize="65536"
            ''                messageEncoding="Text" textEncoding="utf-8" transferMode="Buffered"
            ''                useDefaultWebProxy="true">
            ''                <readerQuotas maxDepth="32" maxStringContentLength="8192" maxArrayLength="16384"
            ''                    maxBytesPerRead="4096" maxNameTableCharCount="16384" />
            ''                <security mode="Transport">
            ''                    <transport clientCredentialType="None" proxyCredentialType="None"
            ''                        realm="" />
            ''                    <message clientCredentialType="UserName" algorithmSuite="Default" />
            ''                </security>
            ''            </binding>
            ''            <binding name="PayPalAPIAASoapBinding" closeTimeout="00:01:00"
            ''                openTimeout="00:01:00" receiveTimeout="00:10:00" sendTimeout="00:01:00"
            ''                allowCookies="false" bypassProxyOnLocal="false" hostNameComparisonMode="StrongWildcard"
            ''                maxBufferSize="65536" maxBufferPoolSize="524288" maxReceivedMessageSize="65536"
            ''                messageEncoding="Text" textEncoding="utf-8" transferMode="Buffered"
            ''                useDefaultWebProxy="true">
            ''                <readerQuotas maxDepth="32" maxStringContentLength="8192" maxArrayLength="16384"
            ''                    maxBytesPerRead="4096" maxNameTableCharCount="16384" />
            ''                <security mode="Transport">
            ''                    <transport clientCredentialType="None" proxyCredentialType="None"
            ''                        realm="" />
            ''                    <message clientCredentialType="UserName" algorithmSuite="Default" />
            ''                </security>
            ''            </binding>
            ''            <binding name="PayPalAPISoapBinding1" closeTimeout="00:01:00"
            ''                openTimeout="00:01:00" receiveTimeout="00:10:00" sendTimeout="00:01:00"
            ''                allowCookies="false" bypassProxyOnLocal="false" hostNameComparisonMode="StrongWildcard"
            ''                maxBufferSize="65536" maxBufferPoolSize="524288" maxReceivedMessageSize="65536"
            ''                messageEncoding="Text" textEncoding="utf-8" transferMode="Buffered"
            ''                useDefaultWebProxy="true">
            ''                <readerQuotas maxDepth="32" maxStringContentLength="8192" maxArrayLength="16384"
            ''                    maxBytesPerRead="4096" maxNameTableCharCount="16384" />
            ''                <security mode="None">
            ''                    <transport clientCredentialType="None" proxyCredentialType="None"
            ''                        realm="" />
            ''                    <message clientCredentialType="UserName" algorithmSuite="Default" />
            ''                </security>
            ''            </binding>
            ''            <binding name="PayPalAPIAASoapBinding1" closeTimeout="00:01:00"
            ''                openTimeout="00:01:00" receiveTimeout="00:10:00" sendTimeout="00:01:00"
            ''                allowCookies="false" bypassProxyOnLocal="false" hostNameComparisonMode="StrongWildcard"
            ''                maxBufferSize="65536" maxBufferPoolSize="524288" maxReceivedMessageSize="65536"
            ''                messageEncoding="Text" textEncoding="utf-8" transferMode="Buffered"
            ''                useDefaultWebProxy="true">
            ''                <readerQuotas maxDepth="32" maxStringContentLength="8192" maxArrayLength="16384"
            ''                    maxBytesPerRead="4096" maxNameTableCharCount="16384" />
            ''                <security mode="None">
            ''                    <transport clientCredentialType="None" proxyCredentialType="None"
            ''                        realm="" />
            ''                    <message clientCredentialType="UserName" algorithmSuite="Default" />
            ''                </security>
            ''            </binding>
            ''            <binding name="SECCardServiceSoapBinding" closeTimeout="00:01:00"
            ''                openTimeout="00:01:00" receiveTimeout="00:10:00" sendTimeout="00:01:00"
            ''                allowCookies="false" bypassProxyOnLocal="false" hostNameComparisonMode="StrongWildcard"
            ''                maxBufferSize="65536" maxBufferPoolSize="524288" maxReceivedMessageSize="65536"
            ''                messageEncoding="Text" textEncoding="utf-8" transferMode="Buffered"
            ''                useDefaultWebProxy="true">
            ''                <readerQuotas maxDepth="32" maxStringContentLength="8192" maxArrayLength="16384"
            ''                    maxBytesPerRead="4096" maxNameTableCharCount="16384" />
            ''                <security mode="Transport">
            ''                    <transport clientCredentialType="None" proxyCredentialType="None"
            ''                        realm="" />
            ''                    <message clientCredentialType="UserName" algorithmSuite="Default" />
            ''                </security>
            ''            </binding>
            ''            <binding name="SECCardServiceSoapBinding1" closeTimeout="00:01:00"
            ''                openTimeout="00:01:00" receiveTimeout="00:10:00" sendTimeout="00:01:00"
            ''                allowCookies="false" bypassProxyOnLocal="false" hostNameComparisonMode="StrongWildcard"
            ''                maxBufferSize="65536" maxBufferPoolSize="524288" maxReceivedMessageSize="65536"
            ''                messageEncoding="Text" textEncoding="utf-8" transferMode="Buffered"
            ''                useDefaultWebProxy="true">
            ''                <readerQuotas maxDepth="32" maxStringContentLength="8192" maxArrayLength="16384"
            ''                    maxBytesPerRead="4096" maxNameTableCharCount="16384" />
            ''                <security mode="None">
            ''                    <transport clientCredentialType="None" proxyCredentialType="None"
            ''                        realm="" />
            ''                    <message clientCredentialType="UserName" algorithmSuite="Default" />
            ''                </security>
            ''            </binding>
            ''        </basicHttpBinding>
            ''    </bindings>
            ''    <client>
            ''        <endpoint address="http://www.eonicweb.net/ewAdminProxy.asmx"
            ''            binding="basicHttpBinding" bindingConfiguration="ewAdminProxySoap"
            ''            contract="eonicweb.com.ewAdminProxySoap" name="ewAdminProxySoap" />
            ''        <endpoint address="https://api.sandbox.paypal.com/2.0/" binding="basicHttpBinding"
            ''            bindingConfiguration="PayPalAPISoapBinding" contract="PayPalAPI.PayPalAPIInterface"
            ''            name="PayPalAPI" />
            ''        <endpoint address="https://api-aa.sandbox.paypal.com/2.0/" binding="basicHttpBinding"
            ''            bindingConfiguration="PayPalAPIAASoapBinding" contract="PayPalAPI.PayPalAPIAAInterface"
            ''            name="PayPalAPIAA" />
            ''        <endpoint address="https://www.secpay.com/java-bin/services/SECCardService"
            ''            binding="basicHttpBinding" bindingConfiguration="SECCardServiceSoapBinding"
            ''            contract="Paypoint.SECVPN" name="SECCardService" />
            ''    </client>
            ''</system.serviceModel>

            Dim configFile As New System.Configuration.ConfigurationFileMap(FilePath)
            Dim config As System.Configuration.Configuration = System.Web.Configuration.WebConfigurationManager.OpenMappedMachineConfiguration(configFile)

            Dim configFile64 As New System.Configuration.ConfigurationFileMap(FilePath64)
            Dim config64 As System.Configuration.Configuration = System.Web.Configuration.WebConfigurationManager.OpenMappedMachineConfiguration(configFile64)

            Dim oCgfRuntimeSect As System.Configuration.IgnoreSection = config.GetSection("runtime")
            Dim oCgfRuntimeSect64 As System.Configuration.IgnoreSection = config64.GetSection("runtime")
            Dim oSectXml As New XmlDocument

            If oCgfRuntimeSect.SectionInformation.GetRawXml Is Nothing Then
                'create runtime
                oSectXml.LoadXml("<runtime><assemblyBinding/></runtime>")
            Else
                oSectXml.LoadXml(oCgfRuntimeSect64.SectionInformation.GetRawXml.Replace("xmlns=""urn:schemas-microsoft-com:asm.v1""", ""))
                Dim testString As String = oCgfRuntimeSect64.SectionInformation.GetRawXml
            End If

            'Protean
            UpdateDependantAssembly(oSectXml, "ProteanCMS", "0e5e11efc3341916", ewAssemblyVersion, "5.0.0.0")
            UpdateDependantAssembly(oSectXml, "Protean.Tools", "2030ce1af675e93f", ewAssemblyVersion, "5.0.0.0")
            UpdateDependantAssembly(oSectXml, "Protean.Tools.Csharp", "0e5e11efc3341916", ewAssemblyVersion, "5.0.0.0")
            UpdateDependantAssembly(oSectXml, "Protean.Providers.Messaging.CampaignMonitor", "0e5e11efc3341916", ewAssemblyVersion, "5.0.0.0")

            'BundleTransformer
            UpdateDependantAssembly(oSectXml, "BundleTransformer.Core", "973C344C93AAC60D", bundleAssemblyVersion)
            UpdateDependantAssembly(oSectXml, "BundleTransformer.Less", "973C344C93AAC60D", bundleLessAssemblyVersion)
            UpdateDependantAssembly(oSectXml, "BundleTransformer.SassAndScss", "973C344C93AAC60D", bundleSassAssemblyVersion)
            UpdateDependantAssembly(oSectXml, "LibSassHost", "3e24e88796a38e46", LibSassHostAssemblyVersion)
            UpdateDependantAssembly(oSectXml, "LibSassHost.Native.win-x64", "3e24e88796a38e46", LibSassHostAssemblyVersion)

            'JavascriptEngineSwitcher
            UpdateDependantAssembly(oSectXml, "JavaScriptEngineSwitcher.Core", "C608B2A8CC9E4472", jsSwitcherAssemblyVersion)
            'UpdateDependantAssembly(oSectXml, "JavaScriptEngineSwitcher.Msie", "C608B2A8CC9E4472", jsSwitcherMSIEAssemblyVersion)
            'UpdateDependantAssembly(oSectXml, "MsieJavaScriptEngine", "A3A2846A37AC0D3E", MSIEJsEngineAssemblyVersion)
            'UpdateDependantAssembly(oSectXml, "JavaScriptEngineSwitcher.ChakraCore", "C608B2A8CC9E4472", jsSwitcherChakraAssemblyVersion)
            UpdateDependantAssembly(oSectXml, "JavaScriptEngineSwitcher.V8", "C608B2A8CC9E4472", jsSwitcherV8AssemblyVersion)
            UpdateDependantAssembly(oSectXml, "JavaScriptEngineSwitcher.V8.Native.win-x64", "C608B2A8CC9E4472", jsSwitcherV8NativeWinx64AssemblyVersion)

            UpdateDependantAssembly(oSectXml, "ClearScript.Core", "31bf3856ad364e35", MicrosoftClearScriptV8AssemblyVersion)
            UpdateDependantAssembly(oSectXml, "ClearScript.V8", "31bf3856ad364e35", MicrosoftClearScriptV8AssemblyVersion)


            UpdateDependantAssembly(oSectXml, "Newtonsoft.Json", "30AD4FE6B2A6AEED", JsonAssemblyVersion, "10.0.0.0")
            UpdateDependantAssembly(oSectXml, "WebGrease", "31bf3856ad364e35", WebGreaseAssemblyVersion)
            UpdateDependantAssembly(oSectXml, "SoundInTheory.DynamicImage.Extensions.Pdf", "fa44558110383067", DynamicImagePDFAssemblyVersion)
            UpdateDependantAssembly(oSectXml, "System.Net.FTPClient", "fa4be07daa57c2b7", SystemNetFTPClientAssemblyVersion)
            UpdateDependantAssembly(oSectXml, "createsend-dotnet", "0e5e11efc3341916", CreateSendAssemblyVersion)
            UpdateDependantAssembly(oSectXml, "TidyHTML5Managed", "0E50e5e11efc3341916", TidyHTML5ManagedAssemblyVersion)
            UpdateDependantAssembly(oSectXml, "ClearScript", "935d0c957da47c73", ClearScriptAssemblyVersion)
            UpdateDependantAssembly(oSectXml, "System.Text.Encoding.CodePages", "b03f5f7f11d50a3a", SystemTextEncodingCodePagesAssemblyVersion)
            UpdateDependantAssembly(oSectXml, "AlphaFS", "4d31a58f7d7ad5c9", AlphaFSAssemblyVersion)
            UpdateDependantAssembly(oSectXml, "Magick.NET.Core", "2004825badfa91ec", MagickNETCoreAssemblyVersion)
            UpdateDependantAssembly(oSectXml, "Magick.NET-Q8-AnyCPU", "2004825badfa91ec", MagickNETAssemblyVersion)
            UpdateDependantAssembly(oSectXml, "System.Buffers", "cc7b13ffcd2ddd51", MagickNETAssemblyVersion)

            Dim BindingElmt As XmlElement = oSectXml.DocumentElement.SelectSingleNode("assemblyBinding")
            BindingElmt.SetAttribute("xmlns", "urn:schemas-microsoft-com:asm.v1")

            oCgfRuntimeSect.SectionInformation.SetRawXml(oSectXml.OuterXml)
            config.Save()

            oCgfRuntimeSect64.SectionInformation.SetRawXml(oSectXml.OuterXml)
            config64.Save()

            'Add the AssetHandler for .Less
            Dim sm As New Microsoft.Web.Administration.ServerManager

            '    Dim appHostConfig As Microsoft.Web.Administration.Configuration = sm.GetApplicationHostConfiguration()
            'get default config section
            Dim webDefaultLocationConfig As Microsoft.Web.Administration.Configuration = sm.GetWebConfiguration("")

            If Not webDefaultLocationConfig Is Nothing Then

                'Dim handlerSection As Microsoft.Web.Administration.ConfigurationElement = webDefaultLocationConfig.GetSection("system.webServer/handlers")

                'If handlerSection Is Nothing Then

                '    Dim handlerCollection As Microsoft.Web.Administration.ConfigurationElementCollection = handlerSection.GetCollection()

                '    Dim handlerElmt As Microsoft.Web.Administration.ConfigurationElement
                '    Dim lessHandlerElmt As Microsoft.Web.Administration.ConfigurationElement = Nothing

                '    For Each handlerElmt In handlerCollection
                '        If handlerElmt.GetAttribute("name").Value = "LessAssetHandler" Then
                '            lessHandlerElmt = handlerElmt
                '        End If
                '    Next
                '    If lessHandlerElmt Is Nothing Then
                '        lessHandlerElmt = handlerCollection.CreateElement("add")
                '        lessHandlerElmt.SetAttributeValue("name", "LessAssetHandler")
                '        lessHandlerElmt.SetAttributeValue("path", "*.less")
                '        lessHandlerElmt.SetAttributeValue("verb", "GET")
                '        lessHandlerElmt.SetAttributeValue("resourceType", "Unspecified")
                '        lessHandlerElmt.SetAttributeValue("requireAccess", "Script")
                '        lessHandlerElmt.SetAttributeValue("preCondition", "integratedMode")
                '        handlerCollection.AddAt(0, lessHandlerElmt)
                '    End If

                '    lessHandlerElmt.SetAttributeValue("type", "BundleTransformer.Less.HttpHandlers.LessAssetHandler, BundleTransformer.Less, Version=" & bundleLessAssemblyVersion & ", Culture=neutral, PublicKeyToken=973C344C93AAC60D")

                '    sm.CommitChanges()
                '    sm = Nothing
                'Else
                '    Try
                '        System.Diagnostics.Process.Start("IExplore.exe", "https://www.ProteanCMS.com/Support/Web-Designers-Guide/Installing-ProteanCMS/setting-up-less?error=system.webServer/handlers-is-missing")
                '    Catch ex As Exception
                '        'do nuffing
                '    End Try
                'End If

            Else
                Try
                    System.Diagnostics.Process.Start("IExplore.exe", "https://www.ProteanCMS.com/Support/Web-Designers-Guide/Installing-ProteanCMS/setting-up-less?error=cannot-get-appHostConfig")
                Catch ex As Exception
                    'do nuffing
                End Try
            End If
            Try
                System.Diagnostics.Process.Start("IExplore.exe", "https://www.ProteanCMS.com/Support/Web-Designers-Guide/Installing-ProteanCMS")
            Catch ex As Exception
                'do nuffing
            End Try
            'MyBase.Install(savedState)

            Return ActionResult.Success


        Catch ex As Exception

            Dim errorstr As String = ex.Message & ex.StackTrace

            If Not ex.InnerException Is Nothing Then
                errorstr = errorstr + ex.InnerException.Message & ex.InnerException.StackTrace
            End If

            My.Computer.FileSystem.WriteAllText("C:\ProteanInstallError.txt", errorstr, True)
            Try
                System.Diagnostics.Process.Start("IExplore.exe", "https://www.ProteanCMS.com/Support/Web-Designers-Guide/Installing-ProteanCMS-troubleshoot")
            Catch ex2 As Exception
                'do nuffing
            End Try

            Return ActionResult.Failure

            'MyBase.Install(savedState)

        End Try

    End Function

    <CustomAction()>
    Public Shared Function InstallFinal(ByVal session As Session) As ActionResult

        Try


            'Move files to correct folders in GAC
            Dim installFolder As String = "C:\Program Files\Eonic Associates LLP\ProteanCMS into GAC " & ewAssemblyVersion.Trim("0").Trim(".") & " (64bit)\Global Assembly Cache"
            Dim GACFolder As String = "C:\Windows\Microsoft.NET\assembly\GAC_MSIL"
            Dim System32Folder As String = "C:\Windows\System32"
            Dim fs As New FileIO.FileSystem
            If System.IO.Directory.Exists(installFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\") Then
                '64bit files
                If Not System.IO.File.Exists(GACFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\v8-x64.dll") Then
                    System.IO.File.Move(installFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\v8-x64.dll", GACFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\v8-x64.dll")
                End If
                If Not System.IO.File.Exists(System32Folder & "\v8-x64.dll") Then
                    System.IO.File.Move(installFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\v8-x64.dll", System32Folder & "\v8-x64.dll")
                End If
                If Not System.IO.File.Exists(GACFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\v8-base-x64.dll") Then
                    System.IO.File.Move(installFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\v8-base-x64.dll", GACFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\v8-base-x64.dll")
                End If
                If Not System.IO.File.Exists(System32Folder & "\v8-base-x64.dll") Then
                    System.IO.File.Move(installFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\v8-base-x64.dll", System32Folder & "\v8-base-x64.dll")
                End If
                If Not System.IO.File.Exists(GACFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\ClearScriptV8-64.dll") Then
                    System.IO.File.Move(installFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\ClearScriptV8-64.dll", GACFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\ClearScriptV8-64.dll")
                End If
                '32bit files
                If Not System.IO.File.Exists(GACFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\v8-ia32.dll") Then
                    System.IO.File.Move(installFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\v8-ia32.dll", GACFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\v8-ia32.dll")
                End If
                If Not System.IO.File.Exists(System32Folder & "\v8-ia32.dll") Then
                    System.IO.File.Move(installFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\v8-ia32.dll", System32Folder & "\v8-ia32.dll")
                End If
                If Not System.IO.File.Exists(GACFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\v8-base-ia32.dll") Then
                    System.IO.File.Move(installFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\v8-base-ia32.dll", GACFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\v8-base-ia32.dll")
                End If
                If Not System.IO.File.Exists(System32Folder & "\v8-base-ia32.dll") Then
                    System.IO.File.Move(installFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\v8-base-ia32.dll", System32Folder & "\v8-base-ia32.dll")
                End If
                If Not System.IO.File.Exists(GACFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\ClearScriptV8-32.dll") Then
                    System.IO.File.Move(installFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\ClearScriptV8-32.dll", GACFolder & "\ClearScript\v4.0_" & ClearScriptAssemblyVersion & "__935d0c957da47c73\ClearScriptV8-32.dll")
                End If
            End If
            If System.IO.Directory.Exists(GACFolder & "\TidyHTML5Managed\v4.0_1.1.5.0__0e5e11efc3341916\") Then
                If Not System.IO.File.Exists(GACFolder & "\TidyHTML5Managed\v4.0_1.1.5.0__0e5e11efc3341916\tidy.x64.dll") Then
                    System.IO.File.Move(installFolder & "\tidy.x64.dll", GACFolder & "\TidyHTML5Managed\v4.0_1.1.5.0__0e5e11efc3341916\tidy.x64.dll")
                    System.IO.File.Move(installFolder & "\tidy.x86.dll", GACFolder & "\TidyHTML5Managed\v4.0_1.1.5.0__0e5e11efc3341916\tidy.x86.dll")
                End If
            End If

            'MyBase.Install(savedState)

            Return ActionResult.Success
        Catch ex As Exception

            Dim errorstr As String = ex.Message & ex.StackTrace

            If Not ex.InnerException Is Nothing Then
                errorstr = errorstr + ex.InnerException.Message & ex.InnerException.StackTrace
            End If

            My.Computer.FileSystem.WriteAllText("C:\ProteanInstallError.txt", errorstr, True)
            Try
                System.Diagnostics.Process.Start("IExplore.exe", "https://www.ProteanCMS.com/Support/Web-Designers-Guide/Installing-ProteanCMS-troubleshoot")
            Catch ex2 As Exception
                'do nuffing
            End Try

            Return ActionResult.Success


            'MyBase.Install(savedState)

        End Try

    End Function

    Public Shared Sub UpdateConfigSection(ByRef SectElmt As XmlElement, ByVal sectionGroupName As String, ByVal sectionName As String, ByVal type As String, ByVal restartOnExernalChanges As Boolean)
        Dim GroupElmt As XmlElement

        Try
            Dim sectionGroupElmt As XmlElement

            If SectElmt.SelectSingleNode("sectionGroup[@name='" & sectionGroupName & "']") Is Nothing Then
                sectionGroupElmt = SectElmt.OwnerDocument.CreateElement("sectionGroup")
                sectionGroupElmt.SetAttribute("name", sectionGroupName)
                SectElmt.AppendChild(sectionGroupElmt)
            End If

            For Each GroupElmt In SectElmt.SelectNodes("sectionGroup[@name='" & sectionGroupName & "']")
                Dim UpdateElmt As XmlElement
                If GroupElmt.SelectSingleNode("section[@name='" & sectionName & "']") Is Nothing Then
                    UpdateElmt = SectElmt.OwnerDocument.CreateElement("section")
                    UpdateElmt.SetAttribute("name", sectionName)
                    GroupElmt.AppendChild(UpdateElmt)
                Else
                    UpdateElmt = GroupElmt.SelectSingleNode("section[@name='" & sectionName & "']")
                End If
                UpdateElmt.SetAttribute("type", type)
                UpdateElmt.SetAttribute("restartOnExternalChanges", LCase(restartOnExernalChanges.ToString))
            Next

        Catch ex As Exception
            Dim errorstr As String = ex.InnerException.StackTrace
            '     My.Computer.FileSystem.WriteAllText("C:\installError.txt", errorstr, True)
        End Try

    End Sub

    Public Shared Sub RemoveConfigSection(ByRef SectElmt As XmlElement, ByVal sectionGroupName As String, ByVal sectionName As String)

        Try

            If Not SectElmt.SelectSingleNode("sectionGroup[@name='" & sectionGroupName & "']") Is Nothing Then
                Dim delElmt As XmlElement = SectElmt.SelectSingleNode("sectionGroup[@name='" & sectionGroupName & "']")
                delElmt.ParentNode.RemoveChild(delElmt)
            End If

        Catch ex As Exception
            Dim errorstr As String = ex.InnerException.StackTrace
            '     My.Computer.FileSystem.WriteAllText("C:\installError.txt", errorstr, True)
        End Try

    End Sub

    Public Shared Sub AddProvider(ByRef ConfigElmt As XmlElement, ByVal GroupName As String, ByVal providerType As String, ByVal Name As String, ByVal AssemblyReference As String)
        Dim GroupElmt As XmlElement
        Dim providerGroupElmt As XmlElement
        Dim providerElmt As XmlElement
        Dim addElmt As XmlElement

        Try
            GroupElmt = ConfigElmt.SelectSingleNode(LCase(GroupName))
            If Not GroupElmt Is Nothing Then
                If GroupElmt.SelectSingleNode(LCase(providerType) & "Providers") Is Nothing Then
                    providerGroupElmt = ConfigElmt.OwnerDocument.CreateElement(LCase(providerType) & "Providers")
                    GroupElmt.InsertAfter(providerGroupElmt, GroupElmt.LastChild)
                    providerElmt = ConfigElmt.OwnerDocument.CreateElement("providers")
                    providerGroupElmt.AppendChild(providerElmt)
                    addElmt = ConfigElmt.OwnerDocument.CreateElement("add")
                    providerElmt.AppendChild(addElmt)
                Else
                    addElmt = GroupElmt.SelectSingleNode(LCase(providerType) & "Providers/providers/add[@name='" & Name & "']")
                    providerElmt = GroupElmt.SelectSingleNode(LCase(providerType) & "Providers/providers")
                    If addElmt Is Nothing Then
                        addElmt = ConfigElmt.OwnerDocument.CreateElement("add")
                        providerElmt.AppendChild(addElmt)
                    End If
                End If
                addElmt.SetAttribute("name", Name)
                addElmt.SetAttribute("type", AssemblyReference)
            End If

        Catch ex As Exception
            Dim errorstr As String = ex.InnerException.StackTrace
            '    My.Computer.FileSystem.WriteAllText("C:\installError.txt", errorstr, True)
        End Try

    End Sub


    Public Shared Sub AddConfigKey(ByRef ConfigElmt As XmlElement, ByVal Name As String, ByVal Value As String)
        Dim KeyElmt As XmlElement

        Try
            KeyElmt = ConfigElmt.SelectSingleNode("add[@key='" & Name & "']")

            If KeyElmt Is Nothing Then
                KeyElmt = ConfigElmt.OwnerDocument.CreateElement("add")
                KeyElmt.SetAttribute("key", Name)
                KeyElmt.SetAttribute("value", Value)
                ConfigElmt.AppendChild(KeyElmt)
            Else
                If Value <> "" Then
                    KeyElmt.SetAttribute("value", Value)
                End If
            End If

        Catch ex As Exception
            Dim errorstr As String = ex.InnerException.StackTrace
            '    My.Computer.FileSystem.WriteAllText("C:\installError.txt", errorstr, True)
        End Try

    End Sub


    Public Shared Sub UpdateAssemblyRef(ByRef oAssemblies As XmlElement, ByVal type As String)
        Dim typeName() As String = type.Split(",")

        Try

            Dim UpdateElmt As XmlElement
            If oAssemblies.SelectSingleNode("add[starts-with(@assembly,'" & typeName(0) & ",')]") Is Nothing Then
                UpdateElmt = oAssemblies.OwnerDocument.CreateElement("add")
                oAssemblies.AppendChild(UpdateElmt)
            Else
                UpdateElmt = oAssemblies.SelectSingleNode("add[starts-with(@assembly,'" & typeName(0) & ",')]")
            End If
            UpdateElmt.SetAttribute("assembly", type)


        Catch ex As Exception
            Dim errorstr As String = ex.InnerException.StackTrace
            '   My.Computer.FileSystem.WriteAllText("C:\installError.txt", errorstr, True)
        End Try

    End Sub

    Public Shared Sub UpdateDependantAssembly(ByRef oSectXml As XmlDocument, ByVal type As String, ByVal token As String, ByVal NewAssemblyVersion As String, Optional ByVal OldestAssemblyVerion As String = "0.0.0.0")

        Try

            Dim BindingElmt As XmlElement = oSectXml.DocumentElement.SelectSingleNode("assemblyBinding")
            If BindingElmt Is Nothing Then
                BindingElmt = oSectXml.CreateElement("assemblyBinding")
                BindingElmt.SetAttribute("xmlns", "urn:schemas-microsoft-com:asm.v1")
                oSectXml.DocumentElement.AppendChild(BindingElmt)
            End If

            If oSectXml.SelectSingleNode("/runtime/assemblyBinding/dependentAssembly[assemblyIdentity/@name='" & type & "']") Is Nothing Then
                Dim newElmt As XmlElement = oSectXml.CreateElement("dependentAssembly")
                newElmt.InnerXml = "<assemblyIdentity name=""" & type & """ publicKeyToken=""" & token & """/><bindingRedirect/>"
                BindingElmt.AppendChild(newElmt)
            End If
            For Each oElmt In oSectXml.SelectNodes("/runtime/assemblyBinding/dependentAssembly[assemblyIdentity/@name='" & type & "']/bindingRedirect")
                oElmt.SetAttribute("oldVersion", OldestAssemblyVerion & "-" & NewAssemblyVersion)
                oElmt.SetAttribute("newVersion", NewAssemblyVersion)
            Next

        Catch ex As Exception
            Dim errorstr As String = ex.InnerException.StackTrace
        End Try

    End Sub


End Class

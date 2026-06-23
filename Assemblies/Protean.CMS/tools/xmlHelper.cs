
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using DocumentFormat.OpenXml.Features;
using static Protean.stdTools;

namespace Protean
{

    //public class Proxy : MarshalByRefObject
    //{
    //    public Assembly GetAssembly(string assemblyPath)
    //    {
    //        try
    //        {
    //            return Assembly.LoadFile(assemblyPath);
    //        }
    //        catch (Exception generatedExceptionName)
    //        {
    //            // throw new InvalidOperationException(ex);
    //            return null;
    //        }
    //    }
    //}

    public class XmlHelper
    {

        public class Transform : IDisposable
        {
            private bool disposedValue = false; // To detect redundant calls

            public Cms myWeb;
            private string msXslFile = "";
            private string msXslLastFile = "";
            private bool mbCompiled = false;
            private long mnTimeoutSec = 20000L;
            private bool bXSLFileIsPath = true;
            public bool mbDebug = false;
            public Exception transformException;
            public string AssemblyPath;
            public string ClassName;
            private System.Xml.Xsl.XslTransform oStyle;
            private System.Xml.Xsl.XslCompiledTransform oCStyle;
            //private bool bFinished = false;
            public bool bError = false;
            public Exception currentError;
            public System.Xml.Xsl.XsltArgumentList xsltArgs;
            private string compiledFolder = @"\xsltc\";
            public AppDomain xsltDomain;

            public string XslFilePath
            {
                get
                {
                    return msXslFile;
                }
                set
                {
                    try
                    {
                        if (goApp["XsltCompileVersion"] is null)
                        {
                            goApp["XsltCompileVersion"] = "0";
                        }
                        string sCompileVersion = $"vg{goApp["XsltCompileVersion"]}";
                        msXslFile = value.Replace("/", @"\");
                        ClassName = msXslFile.Substring(msXslFile.LastIndexOf(@"\") + 1);
                        ClassName = ClassName.Replace(".", "_") + sCompileVersion;
                        if (mbCompiled)
                        {

                            Assembly assemblyInstance = null;
                            AssemblyPath = goServer.MapPath(compiledFolder) + ClassName + ".dll";
                            Type CalledType;

                            if (Convert.ToBoolean(goApp[ClassName]))
                            {
                                foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
                                {
                                    if (ass.GetName().ToString().StartsWith(ClassName))
                                    {
                                        assemblyInstance = ass;
                                    }
                                }
                                if (assemblyInstance is null)
                                {
                                    assemblyInstance = Assembly.LoadFrom(AssemblyPath);
                                }
                            }
                            else if (File.Exists(AssemblyPath))
                            {
                                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                                assemblyInstance = Assembly.LoadFrom(AssemblyPath);
                                if (assemblyInstance != null)
                                {
                                    goApp[ClassName] = true;
                                }
                            }

                            else
                            {
                                // Compile the XSLT to assembly (will throw exception on failure)
                                CompileXSLTassembly(ClassName);
                                if (bError) {
                                    // Re-throw the original compilation exception with full error details
                                    if (transformException != null)
                                    {
                                        throw transformException;
                                    }
                                    else
                                    {
                                        throw new InvalidOperationException($"XSLT compilation or loading failed for: {msXslFile}");
                                    }
                                }
                                // If we reach here, compilation succeeded - load the assembly
                                assemblyInstance = Assembly.LoadFrom(AssemblyPath);
                            }

                            CalledType = assemblyInstance.GetType(ClassName, true);

                            oCStyle = new System.Xml.Xsl.XslCompiledTransform(mbDebug);
                            var resolver = new XmlUrlResolver();
                            resolver.Credentials = System.Net.CredentialCache.DefaultCredentials;
                            oCStyle.Load(CalledType);
                        }

                        // the old method is quicker for realtime loading of xslt
                        else if ((msXslLastFile ?? "") != (msXslFile ?? "") & oStyle is null)
                        {
                            // modification to allow for XSL to only be loaded once.
                            oStyle = new System.Xml.Xsl.XslTransform();
                            if (!string.IsNullOrEmpty(msXslFile))
                            {
                               if (myWeb != null) {
                                myWeb.PerfMon.Log("XmlHelper", "LoadXSL");
                               }

                                oStyle.Load(msXslFile);

                                if (myWeb != null)
                                {
                                    myWeb.PerfMon.Log("XmlHelper", "LoadXSL-end");
                                }
                            }
                            msXslLastFile = msXslFile;
                        }
                    }
                    catch (Exception ex)
                    {                        
                        transformException = ex;
                        stdTools.returnException(ref myWeb.msException, "Protean.XmlHelper.Transform", "XslFilePath.Set", ex, msXslFile, value, gbDebug);
                        bError = true;
                        // Only redirect in production mode; in debug mode, allow error details to be displayed
                        if (mbCompiled && !gbDebug)  // Use gbDebug (global debug flag from stdTools)
                        {
                            Protean.Config.UpdateConfigValue(ref myWeb, "", "recompile", "recreate");
                            myWeb.moResponse.Redirect("/",false);
                        }
                        // In debug mode (gbDebug == true), error details in myWeb.msException will be displayed to the browser
                    }
                }
            }

            private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
            {
                Assembly assembly, objExecutingAssemblies;
                string strTempAsmbPath = "";
                try
                {
                    objExecutingAssemblies = args.RequestingAssembly;
                    if (objExecutingAssemblies != default)
                    {

                        AssemblyName[] arrReferencedAssmbNames = objExecutingAssemblies.GetReferencedAssemblies();

                        foreach (AssemblyName strAssmbName in arrReferencedAssmbNames)
                        {
                            if ((strAssmbName.FullName.Substring(0, strAssmbName.FullName.IndexOf(",")) ?? "") == (args.Name.Substring(0, args.Name.IndexOf(",")) ?? ""))
                            {
                                strTempAsmbPath = goServer.MapPath(compiledFolder) + @"\" + args.Name.Substring(0, args.Name.IndexOf(",")) + ".dll";
                                break;
                            }
                        }

                        assembly = Assembly.LoadFrom(strTempAsmbPath);
                        return assembly;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    transformException = ex;
                    // returnException(myWeb.msException, "Protean.XmlHelper.Transform", "CurrentDomain_AssemblyResolve", ex, msXslFile, Nothing, gbDebug)
                    bError = true;
                    return null;
                }

            }

            public string XSLFile
            {
                get
                {
                    return msXslFile;
                }
                set
                {
                    try
                    {
                        msXslFile = value;
                        if (bXSLFileIsPath)
                        {
                            XslFilePath = value;
                        }
                        else
                        {
                            msXslFile = value;
                            oStyle = new System.Xml.Xsl.XslTransform();
                            var oXSL = new XmlDocument();
                            oXSL.InnerXml = msXslFile.Trim();
                            oStyle.Load(oXSL);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (transformException is null)
                        {
                            transformException = ex;
                        }
                        stdTools.returnException(ref myWeb.msException, "Protean.XmlHelper.Transform", "XSLFile.Set", ex, msXslFile, value);
                        bError = true;
                    }
                }
            }

            public bool Compiled
            {
                get
                {
                    return mbCompiled;
                }
                set
                {
                    mbCompiled = value;
                }
            }

            public long TimeOut
            {
                get
                {
                    return mnTimeoutSec;
                }
                set
                {
                    mnTimeoutSec = value;
                }
            }

            private bool CanProcess
            {
                get
                {
                    if (string.IsNullOrEmpty(msXslFile))
                        return false;
                    else
                        return true;
                }
            }

            public bool XSLFileIsPath
            {
                get
                {
                    return bXSLFileIsPath;
                }
                set
                {
                    bXSLFileIsPath = value;
                }
            }

            public bool HasError
            {
                get
                {
                    return bError;
                }
            }

            public Transform()
            {
                string sProcessInfo = "";
                try
                {
                    myWeb = null;
                    xsltArgs = new System.Xml.Xsl.XsltArgumentList();
                    var ewXsltExt = new Protean.xmlTools.xsltExtensions();
                    xsltArgs.AddExtensionObject("urn:ew", ewXsltExt);
                }
                catch (Exception ex)
                {
                    transformException = ex;
                    stdTools.returnException(ref myWeb.msException, "Protean.XmlHelper.Transform", "New", ex, msXslFile, sProcessInfo, mbDebug);
                    bError = true;
                }
            }

            public Transform(ref Cms aWeb, string sXslFile, bool bCompiled, long nTimeoutSec = 15000L, bool recompile = false, bool bDebug = false)
            {
                mbDebug = bDebug;
                string sProcessInfo = "";
                try
                {
                    aWeb.PerfMon.Log("Web", "Transform-New-Start");
                    myWeb = aWeb;
                    mbCompiled = bCompiled;
                    if (myWeb.moConfig["ProjectPath"] != "")
                    {
                        compiledFolder = myWeb.moConfig["ProjectPath"] + compiledFolder;
                    }

                    // If Not goApp("ewStarted") = True Then
                    // 'If Not goApp("xsltDomain") Is Nothing Then
                    // '    Dim xsltDomain As AppDomain = goApp("xsltDomain")
                    // '    AppDomain.Unload(xsltDomain)
                    // 'End If
                    aWeb.PerfMon.Log("Web", "Transform-New-Start2");
                    if (recompile)
                    {

                        goApp["XsltCompileVersion"] = (Convert.ToInt16(goApp["XsltCompileVersion"]) + 1).ToString();

                    }

                    // goApp("ewStarted") = True
                    // End If

                    XslFilePath = sXslFile;
                    if (bError) {
                        // Re-throw the original compilation exception with full error details
                        if (transformException != null)
                        {
                            throw transformException;
                        }
                        else
                        {
                            throw new InvalidOperationException($"XSLT compilation or loading failed for: {sXslFile}");
                        }
                    }
                    string className = msXslFile.Substring(msXslFile.LastIndexOf(@"\") + 1);
                    className = className.Replace(".", "_");


                    aWeb.PerfMon.Log("Web", "Transform-New-END");

                    if (mbCompiled == true & goApp[className] is null)
                    {
                        mnTimeoutSec = 60000L;
                    }
                    else
                    {
                        mnTimeoutSec = nTimeoutSec;
                    }
                    aWeb.PerfMon.Log("Web", "Transform-loadxslExt");
                    xsltArgs = new System.Xml.Xsl.XsltArgumentList();
                    var ewXsltExt = new Protean.xmlTools.xsltExtensions(ref myWeb);
                    aWeb.PerfMon.Log("Web", "Transform-loadxslExt-create");
                    xsltArgs.AddExtensionObject("urn:ew", ewXsltExt);
                    aWeb.PerfMon.Log("Web", "Transform-loadxslExt-add");
                }

                catch (Exception ex)
                {
                    transformException = ex;
                    stdTools.returnException(ref myWeb.msException, "Protean.XmlHelper.Transform", "New", ex, msXslFile, sProcessInfo, mbDebug);
                    bError = true;
                }
            }

          


            public delegate void ProcessDelegate(XmlDocument oXml, HttpResponse oResponse);
            public delegate void ProcessDelegate2(XmlDocument oXml, ref TextWriter oWriter);
            public delegate void ProcessDelegate3(XmlReader xReader, ref XmlWriter xWriter);
            public delegate XmlDocument ProcessDelegateDocument(XmlDocument oXml);

            public void ProcessTimed(XmlDocument oXml, ref HttpResponse oResponse)
            {

                string sProcessInfo = "";
                try
                {
                    var d = new ProcessDelegate(Process);
                    var res = d.BeginInvoke(oXml, oResponse, null, null);
                    if (res.IsCompleted == false)
                    {
                        res.AsyncWaitHandle.WaitOne((int)mnTimeoutSec, false);
                        if (res.IsCompleted == false)
                        {
                            d.EndInvoke((System.Runtime.Remoting.Messaging.AsyncResult)res);
                            d = null; 
                            bError = true;
                            throw new InvalidOperationException($"The XSL took longer than { mnTimeoutSec / 1000d }seconds to process");
                        }
                    }
                    d.EndInvoke((System.Runtime.Remoting.Messaging.AsyncResult)res);
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, "Protean.XmlHelper.TransformTimed", "Process", ex, msXslFile, sProcessInfo, mbDebug);
                    oResponse.Write(myWeb.msException);
                    bError = true;
                }
            }
            public void ProcessTimed(XmlDocument oXml, ref TextWriter oWriter)
            {

                string sProcessInfo = "";
                try
                {
                    var d = new ProcessDelegate2(Process);
                    var res = d.BeginInvoke(oXml, ref oWriter, null, null);
                    if (res.IsCompleted == false)
                    {
                        res.AsyncWaitHandle.WaitOne((int)mnTimeoutSec, false);
                        if (res.IsCompleted == false)
                        {
                            d.EndInvoke(ref oWriter, (System.Runtime.Remoting.Messaging.AsyncResult)res);
                            d = null; 
                            bError = true;
                            throw new InvalidOperationException($"The XSL took longer than {mnTimeoutSec / 1000d} seconds to process");
                        }
                    }
                    d.EndInvoke(ref oWriter, (System.Runtime.Remoting.Messaging.AsyncResult)res);
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, "Protean.XmlHelper.TransformTimed", "Process", ex, msXslFile, sProcessInfo);
                    oWriter.Write(myWeb.msException);
                    bError = true;
                }
            }

            public void ProcessTimed(XmlReader xReader, ref XmlWriter xWriter)
            {

                string sProcessInfo = "";
                try
                {
                    var d = new ProcessDelegate3(Process);
                    var res = d.BeginInvoke(xReader, ref xWriter, null, null);
                    if (res.IsCompleted == false)
                    {
                        res.AsyncWaitHandle.WaitOne((int)mnTimeoutSec, false);
                        if (res.IsCompleted == false)
                        {
                            d.EndInvoke(ref xWriter, (System.Runtime.Remoting.Messaging.AsyncResult)res);
                            d = null;
                            bError = true;
                            throw new InvalidOperationException($"The XSL took longer than { mnTimeoutSec / 1000d } seconds to process");
                        }
                    }
                    d.EndInvoke(ref xWriter, (System.Runtime.Remoting.Messaging.AsyncResult)res);
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, "Protean.XmlHelper.TransformTimed", "Process", ex, msXslFile, sProcessInfo);
                    xWriter.WriteCData(myWeb.msException);
                    bError = true;
                }
            }
            public XmlDocument ProcessTimedDocument(XmlDocument oXml)
            {
                if (!CanProcess)
                    return null;
                string sProcessInfo = "";
                TextWriter oWriter = new StringWriter();
                try
                {
                    var d = new ProcessDelegate2(Process);

                    var res = d.BeginInvoke(oXml, ref oWriter, null, null);
                    if (res.IsCompleted == false)
                    {
                        res.AsyncWaitHandle.WaitOne((int)mnTimeoutSec, false);
                        if (res.IsCompleted == false)
                        {

                            d.EndInvoke(ref oWriter, (System.Runtime.Remoting.Messaging.AsyncResult)res);
                            d = null;
                            throw new InvalidOperationException($"The XSL took longer than { mnTimeoutSec / 1000d } seconds to process");
                        }
                    }
                    d.EndInvoke(ref oWriter, (System.Runtime.Remoting.Messaging.AsyncResult)res);
                    var oXMLNew = new XmlDocument();
                    oXml.InnerXml = oWriter.ToString();
                    return oXml;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, "Protean.XmlHelper.Transform", "Process", ex, msXslFile, sProcessInfo, mbDebug);
                    oWriter.Write(myWeb.msException);
                    bError = true;
                    return null;
                }
            }

            public void Process(XmlReader xReader, ref XmlWriter xWriter)
            {
                string sProcessInfo = "Processing:" + msXslFile;
                try
                {
                    var resolver = new XmlUrlResolver();
                    resolver.Credentials = System.Net.CredentialCache.DefaultCredentials;
                    if (oCStyle is null)
                    {
                        XPathDocument xpathDoc = new XPathDocument(xReader);
                        oStyle.Transform(xpathDoc, xsltArgs, xWriter);
                    }
                    else
                    {
                        oCStyle.Transform(xReader, xsltArgs, xWriter);
                    }
                }
                catch (Exception ex)
                {
                    transformException = ex;
                    stdTools.returnException(ref myWeb.msException, "Protean.XmlHelper.Transform", "Process", ex, msXslFile, sProcessInfo, mbDebug);
                    // oResponse.Write(msException)
                    bError = true;
                }
            }

            public void Process(XmlDocument oXml, HttpResponse oResponse)
            {
                if (!CanProcess)
                    return;
                string sProcessInfo = "Processing:" + msXslFile;
                try
                {
                    if (mbCompiled)
                    {
                        // Check if oCStyle was successfully initialized before attempting to use it
                        if (oCStyle != null)
                        {
                            var resolver = new XmlUrlResolver();
                            resolver.Credentials = System.Net.CredentialCache.DefaultCredentials;

                            var ws = oCStyle.OutputSettings.Clone();

                            // load the pagexml into a reader
                            var oReader = new XmlTextReader(new StringReader(oXml.OuterXml));
                            var sWriter = new StringWriter();

                            if (myWeb.msException == "")
                            {
                                // Run transformation

                                // Dim xsltDomainProxy As ProxyDomain = xsltDomain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, GetType(ProxyDomain).FullName)
                                // xsltDomainProxy._LocalContext = myWeb.moCtx
                                // Dim responseString As String = xsltDomainProxy.RunTransform(AssemblyPath, ClassName, oXml.OuterXml)
                                // oResponse.Write(responseString)

                                oCStyle.Transform(oReader, xsltArgs, XmlWriter.Create(oResponse.OutputStream, ws), resolver);
                            }
                            else
                            {
                                oResponse.Write(myWeb.msException);
                            }
                            oReader.Close();
                            sWriter.Dispose();
                        }
                        else
                        {
                            // oCStyle is null - compilation or loading failed
                            // Error details should already be in myWeb.msException
                            oResponse.Write(myWeb.msException);
                        }
                    }

                    else
                    {

                        // Change the xmlDocument to xPathDocument to improve performance
                        var oXmlNodeReader = new XmlNodeReader(oXml);
                        var xpathDoc = new XPathDocument(oXmlNodeReader);

                        if (myWeb is null)
                        {
                            oStyle.Transform(xpathDoc, xsltArgs, oResponse.OutputStream, null);
                        }
                        else if (myWeb.msException == "")
                        {
                            // Run transformation
                            oStyle.Transform(xpathDoc, xsltArgs, oResponse.OutputStream, null);
                        }
                        else
                        {
                            oResponse.Write(myWeb.msException);
                        }

                        oXmlNodeReader.Close();
                        xpathDoc = null;

                    }
                }
                catch (Exception ex)
                {
                    transformException = ex;
                    stdTools.returnException(ref myWeb.msException, "Protean.XmlHelper.Transform", "Process", ex, msXslFile, sProcessInfo, mbDebug);
                    oResponse.Write(myWeb.msException);
                    bError = true;
                }
            }
            public void Process(XmlDocument oXml, ref TextWriter oWriter)
            {
                if (!CanProcess)
                    return;
                bError = false;
                string sProcessInfo = "Processing: " + msXslFile;
                try
                {
                    if (mbCompiled)
                    {
                        if (myWeb is null)
                        {
                            var oReader = new XmlTextReader(new StringReader(oXml.OuterXml));
                            var sWriter = new StringWriter();
                            oCStyle.Transform(oReader, xsltArgs, oWriter);
                        }
                        else if (myWeb.msException == "")
                        {
                            // Run transformation 
                            var oReader = new XmlTextReader(new StringReader(oXml.OuterXml));
                            var sWriter = new StringWriter();
                            oCStyle.Transform(oReader, xsltArgs, oWriter);
                        }
                        else
                        {
                            oWriter.Write(myWeb.msException);
                        }
                    }
                    else
                    {
                        // Change the xmlDocument to xPathDocument to improve performance
                        var xpathDoc = new XPathDocument(new XmlNodeReader(oXml));
                        if (myWeb is null)
                        {
                            oStyle.Transform(xpathDoc, xsltArgs, oWriter, null);
                        }
                        else if (myWeb.msException == "" | myWeb.msException is null)
                        {
                            // Run transformation
                            oStyle.Transform(xpathDoc, xsltArgs, oWriter, null);
                        }
                        else
                        {
                            oWriter.Write(myWeb.msException);
                        }
                    }
                }
                catch (Exception ex)
                {
                    transformException = ex;
                    if (myWeb != null)
                    {
                        stdTools.returnException(ref myWeb.msException, "Protean.XmlHelper.Transform", "Process", ex, msXslFile, sProcessInfo, mbDebug);
                        oWriter.Write(myWeb.msException);
                    }
                    bError = true;
                }
            }

            public string stripNonValidXMLCharacters(string textIn)
            {
                var textOut = new System.Text.StringBuilder();
                var textOuterr = new System.Text.StringBuilder();
                // Used to hold the output.
                char current;
                // Used to reference the current character.
                int currenti;


                if (textIn is null || string.IsNullOrEmpty(textIn))
                {
                    return string.Empty;
                }
                // vacancy test.
                for (int i = 0, loopTo = textIn.Length - 1; i <= loopTo; i++)
                {
                    current = textIn[i];
                    currenti = (char)current;

                    if (currenti == Convert.ToInt16("&H9") || currenti == Convert.ToInt16("&HA") || currenti == Convert.ToInt16("&HD") || currenti >= Convert.ToInt16("&H20") && currenti <= Convert.ToInt16("&HD7FF") || currenti >= Convert.ToInt16("&HE000") && currenti <= Convert.ToInt16("&HFFFD") || currenti >= Convert.ToInt16("&H10000") && currenti <= Convert.ToInt16("&H10FFFF"))
                    {
                        textOut.Append(current);
                    }
                    else
                    {
                        textOuterr.Append(current);
                    }
                }
                return textOut.ToString();
            }

            public XmlDocument ProcessDocument(XmlDocument oXml)
            {
                if (!CanProcess)
                    return null;
                string sProcessInfo = "Proceesing:" + msXslFile;
                TextWriter oWriter = new StringWriter();
                try
                {

                    if (mbCompiled)
                    {

                        System.Xml.Xsl.XslCompiledTransform oStyle;
                        // hear we cache the loaded xslt in the application object.                    
                        var resolver = new XmlUrlResolver();
                        resolver.Credentials = System.Net.CredentialCache.DefaultCredentials;

                        // here we store the stylesheet in the application object
                        if (goApp[msXslFile] is null)
                        {

                            // load the xslt
                            oStyle = new System.Xml.Xsl.XslCompiledTransform();
                            // this is the line that takes the time
                            oStyle.Load(msXslFile, System.Xml.Xsl.XsltSettings.TrustedXslt, resolver);
                            goApp.Add(msXslFile, oStyle);
                        }
                        else
                        {
                            // get the loaded xslt from the application variable
                            oStyle = (System.Xml.Xsl.XslCompiledTransform)goApp[msXslFile];
                        }

                        // add Eonic Bespoke Functions

                        var xsltArgs = new System.Xml.Xsl.XsltArgumentList();
                        var ewXsltExt = new Protean.xmlTools.xsltExtensions(ref myWeb);
                        xsltArgs.AddExtensionObject("urn:ew", ewXsltExt);


                        var ws = oStyle.OutputSettings.Clone();

                        // load the pagexml into a reader
                        var oReader = new XmlTextReader(new StringReader(oXml.OuterXml));
                        var sWriter = new StringWriter();

                        if (myWeb.msException == "")
                        {
                            // Run transformation
                            oStyle.Transform(oReader, xsltArgs, oWriter);
                        }
                        else
                        {
                            oWriter.Write(myWeb.msException);
                        }
                    }

                    else
                    {
                        // the old method is quicker for realtime loading of xslt


                        // load the xslt
                        oStyle.Load(msXslFile);

                        // add Eonic Bespoke Functions
                        var xsltArgs = new System.Xml.Xsl.XsltArgumentList();
                        var ewXsltExt = new Protean.xmlTools.xsltExtensions(ref myWeb);
                        xsltArgs.AddExtensionObject("urn:ew", ewXsltExt);


                        // Change the xmlDocument to xPathDocument to improve performance
                        var xpathDoc = new XPathDocument(new XmlNodeReader(oXml));

                        if (myWeb.msException == "")
                        {
                            // Run transformation
                            oStyle.Transform(xpathDoc, xsltArgs, oWriter, null);
                        }
                        else
                        {
                            oWriter.Write(myWeb.msException);
                        }


                    }
                    var oXMLNew = new XmlDocument();
                    oXml.InnerXml = oWriter.ToString();
                    return oXml;
                }
                catch (Exception ex)
                {
                    if (goApp[msXslFile] != null)
                    {
                        goApp.Remove(msXslFile);
                    }
                    bError = true;
                    currentError = ex;
                    stdTools.returnException(ref myWeb.msException, "Protean.XmlHelper.Transform", "ProcessDocument", ex, msXslFile, sProcessInfo, mbDebug);
                    oWriter.Write(myWeb.msException);
                    return null;
                }
            }

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr LoadLibrary([In()][MarshalAs(UnmanagedType.LPStr)] string lpFileName);
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr GetModuleHandle(string lpModuleName);
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern bool FreeLibrary([In()] IntPtr hModule);


            public bool ClearXSLTassemblyCache()
            {
                string sProcessInfo = "ClearXSLTassemblyCache";
                try
                {

                    Tools.Security.Impersonate oImp = null;
                    if (myWeb.moConfig["AdminAcct"] == "")
                    {
                        oImp = new Tools.Security.Impersonate();
                        oImp.ImpersonateValidUser(myWeb.moConfig["AdminAcct"], myWeb.moConfig["AdminDomain"], myWeb.moConfig["AdminPassword"], true, myWeb.moConfig["AdminGroup"]);
                    }

                    string cWorkingDirectory = goServer.MapPath(compiledFolder);
                    sProcessInfo = "clearing " + cWorkingDirectory;
                    var di = new DirectoryInfo(cWorkingDirectory);

                    foreach (var fi in di.EnumerateFiles())
                    {
                        try
                        {
                            var fso = new fsHelper();
                            fso.DeleteFile(fi.FullName);
                        }
                        catch (Exception ex2)
                        {
                            transformException = ex2;
                            bError = true;
                            //returnException("Protean.XmlHelper.Transform", "ClearXSLTassemblyCache", ex2, msXslFile, sProcessInfo)
                        }
                    }

                    if (myWeb.moConfig["AdminAcct"] == "")
                    {
                        oImp.UndoImpersonation();
                    }

                    // reset config to on
                   // Config.UpdateConfigValue(ref myWeb, "protean/web", "CompiledTransform", "on");
                }

                // di.Delete(True)

                catch (Exception ex)
                {
                    transformException = ex;
                    bError = true;
                    //stdTools.returnException(ref myWeb.msException, "Protean.XmlHelper.Transform", "ClearXSLTassemblyCache", ex, msXslFile, sProcessInfo, mbDebug);
                    return default;
                }

                return default;
            }


            /// <summary>
            /// Compiles an XSLT stylesheet into a .NET assembly for improved transformation performance.
            /// </summary>
            /// <param name="classname">The class name to use for the compiled assembly (derived from XSLT filename)</param>
            /// <returns>
            /// Returns the <paramref name="classname"/> string on successful compilation, 
            /// or <c>null</c> if compilation fails.
            /// </returns>
            /// <remarks>
            /// <para><strong>CURRENT ERROR-HANDLING BEHAVIOR (BASELINE DOCUMENTATION):</strong></para>
            /// <para>
            /// When XSLT compilation fails, this method:
            /// 1. Catches the exception (line 924)
            /// 2. Calls <see cref="stdTools.returnException"/> to store error details in <c>myWeb.msException</c> (line 928)
            /// 3. Sets <c>bError = true</c> (line 927)
            /// 4. Returns <c>null</c> (line 929) ⚠️ <strong>ISSUE: This loses exception context</strong>
            /// </para>
            /// <para>
            /// <strong>⚠️ KNOWN ISSUE:</strong> The calling code in <c>XslFilePath</c> property setter (line 108-119) 
            /// receives <c>null</c> and throws a new <see cref="InvalidOperationException"/> with a null message (line 117),
            /// which overwrites the original error details stored in <c>myWeb.msException</c>.
            /// When <c>mbCompiled == true</c>, the code redirects to "/" (line 159), preventing error display even in debug mode.
            /// </para>
            /// <para><strong>DEPENDENCIES:</strong></para>
            /// <list type="bullet">
            ///   <item><c>myWeb.msException</c> - Stores exception information for error display</item>
            ///   <item><c>bError</c> - Instance flag indicating error state</item>
            ///   <item><c>transformException</c> - Instance field to store exceptions</item>
            ///   <item><c>msXslFile</c> - Path to the XSLT file being compiled</item>
            ///   <item><c>compiledFolder</c> - Output directory for compiled assemblies</item>
            ///   <item><c>goApp["compileLock-{classname}"]</c> - Application-level compilation lock</item>
            /// </list>
            /// <para><strong>PROCESS FLOW:</strong></para>
            /// <list type="number">
            ///   <item>Determine compiler path based on <c>myWeb.bs5</c> flag</item>
            ///   <item>Check for compilation lock in application state</item>
            ///   <item>Execute xsltc.exe compiler as external process</item>
            ///   <item>Read StandardOutput for compilation results</item>
            ///   <item>Check output for "error" keyword</item>
            ///   <item>On success: return classname; On failure: return null after logging exception</item>
            /// </list>
            /// </remarks>
            /// <example>
            /// <strong>Typical calling pattern (from XslFilePath property setter):</strong>
            /// <code>
            /// string compileResponse = CompileXSLTassembly(ClassName);
            /// if ((compileResponse ?? "") == (ClassName ?? ""))
            /// {
            ///     assemblyInstance = Assembly.LoadFrom(AssemblyPath);
            /// }
            /// else
            /// {
            ///     // ⚠️ ISSUE: compileResponse is null, so this creates exception with null message
            ///     throw new InvalidOperationException(compileResponse);
            /// }
            /// </code>
            /// </example>
            public string CompileXSLTassembly(string classname)
            {

                string compilerPath = goServer.MapPath("/ewcommon/xsl/compiler/xsltc.exe");
                if (myWeb.bs5)
                {
                    compilerPath = goServer.MapPath("/ptn/tools/compiler/xsltc.exe");
                }
                string xsltPath = "\"" + msXslFile + "\"";
                string sProcessInfo = "compiling: " + xsltPath;
                string outFile = classname + ".dll";
                string cmdLine = " /class:" + classname + " /out:" + outFile + " " + xsltPath;
                var process1 = new Process();
                string output = "";

                try
                {

                    if (goApp["compileLock-" + classname] is null)
                    {
                        goApp["compileLock-" + classname] = true;

                        process1.EnableRaisingEvents = true;
                        process1.StartInfo.FileName = compilerPath;
                        process1.StartInfo.Arguments = cmdLine;
                        process1.StartInfo.UseShellExecute = false;
                        process1.StartInfo.RedirectStandardOutput = true;
                        process1.StartInfo.RedirectStandardInput = true;
                        process1.StartInfo.RedirectStandardError = true;

                        // check if local bin exists
                        string cWorkingDirectory = goServer.MapPath(compiledFolder);
                        var di = new DirectoryInfo(cWorkingDirectory);
                        if (!di.Exists)
                        {
                            di.Create();
                        }

                        process1.StartInfo.WorkingDirectory = cWorkingDirectory;
                        // Start the process
                        process1.Start();

                        // Read both StandardOutput and StandardError
                        output = process1.StandardOutput.ReadToEnd();
                        string errorOutput = process1.StandardError.ReadToEnd();

                        // Wait for process to finish
                        process1.WaitForExit();

                        int exitCode = process1.ExitCode;
                        process1.Close();

                        goApp["compileLock-" + classname] = null;

                        // Check if compilation failed based on exit code or error output
                        bool compilationFailed = false;
                        string compilationError = "";

                        if (exitCode != 0)
                        {
                            compilationFailed = true;
                            compilationError = $"XSLT Compiler exited with code {exitCode}.";
                        }

                        if (!string.IsNullOrEmpty(errorOutput))
                        {
                            compilationFailed = true;
                            compilationError += (compilationError != "" ? "\n" : "") + "Error Output:\n" + errorOutput;
                        }

                        if (output.Contains("error") || output.Contains("Error"))
                        {
                            compilationFailed = true;
                            compilationError += (compilationError != "" ? "\n" : "") + "Standard Output:\n" + output;
                        }

                        // Verify the DLL file was actually created
                        string dllPath = Path.Combine(cWorkingDirectory, outFile);
                        if (!File.Exists(dllPath))
                        {
                            compilationFailed = true;
                            compilationError += (compilationError != "" ? "\n" : "") + $"Expected output file not created: {dllPath}";
                            if (!string.IsNullOrEmpty(output))
                            {
                                compilationError += "\nCompiler Output:\n" + output;
                            }
                        }

                        if (compilationFailed)
                        {
                            throw new Exception($"XSLT Compilation Failed for '{msXslFile}':\n{compilationError}");
                        }

                    }

                    return classname;
                }

                catch (Exception ex)
                {
                    goApp["compileLock-" + classname] = null;
                    bError = true;
                    transformException = ex;  // Store exception for reference
                    stdTools.returnException(ref myWeb.msException, "Protean.XmlHelper.Transform", "CompileXSLTassembly", ex, msXslFile, sProcessInfo, mbDebug);
                    // Re-throw to preserve exception context and allow proper error handling up the call stack
                    throw;
                }
            }
#pragma warning restore 618



            #region IDisposable Implementation

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        try
                        {
                            // ====================
                            // 1. DISPOSE MANAGED RESOURCES
                            // ====================

                            // XSLT Transform objects
                            if (oStyle != null)
                            {
                                try
                                {
                                    oStyle = null; // XslTransform doesn't implement IDisposable
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine(
                                        $"Error disposing oStyle: {ex.Message}");
                                }
                            }

                            if (oCStyle != null)
                            {
                                try
                                {
                                    oCStyle = null; // XslCompiledTransform doesn't implement IDisposable
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine(
                                        $"Error disposing oCStyle: {ex.Message}");
                                }
                            }

                            // XSLT Arguments
                            if (xsltArgs != null)
                            {
                                try
                                {
                                    xsltArgs.Clear();
                                    xsltArgs = null;
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine(
                                        $"Error disposing xsltArgs: {ex.Message}");
                                }
                            }

                            // AppDomain (if created)
                            if (xsltDomain != null)
                            {
                                try
                                {
                                    // Only unload if we created it and it's not the current domain
                                    if (xsltDomain != AppDomain.CurrentDomain)
                                    {
                                        AppDomain.Unload(xsltDomain);
                                    }
                                    xsltDomain = null;
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine(
                                        $"Error unloading xsltDomain: {ex.Message}");
                                }
                            }

                            // ====================
                            // 2. CLEAR REFERENCES (NOT OWNED - DO NOT DISPOSE)
                            // ====================

                            // Parent reference - owned by parent Cms object
                            myWeb = null;

                            // Clear exception references
                            transformException = null;
                            currentError = null;

                            // Clear path strings
                            msXslFile = null;
                            msXslLastFile = null;
                            AssemblyPath = null;
                            ClassName = null;
                        }
                        catch (Exception ex)
                        {
                            // Log disposal errors but don't throw
                            System.Diagnostics.Debug.WriteLine(
                                $"Error in Transform.Dispose: {ex.Message}");
                        }
                    }

                    // Free unmanaged resources (if any)
                    // No unmanaged resources to free

                    disposedValue = true;
                }
            }

            // Finalizer
            ~Transform()
            {
                Dispose(false);
            }

            // Public Dispose method
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            // Legacy Close() method for backward compatibility
            public void Close()
            {
                // Simply call Dispose() - maintains backward compatibility
                Dispose();
            }

            // Helper method to prevent use after disposal
            protected void ThrowIfDisposed()
            {
                if (disposedValue)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
            }

            #endregion
        }





        private class ProxyDomain : MarshalByRefObject
        {
            public HttpContext _LocalContext;

            public void GetAssembly(string AssemblyPath, string className)
            {
                try
                {
                    var newAssem = Assembly.LoadFrom(AssemblyPath);
                }


                // If you want to do anything further to that assembly, you need to do it here.


                catch (Exception ex)
                {
                    throw new InvalidOperationException(ex.Message, ex);
                }
            }

            public string RunTransform(string AssemblyPath, string className, string PageXml)
            {
                try
                {
                    HttpContext.Current = _LocalContext;

                    var oReader = new XmlTextReader(new StringReader(PageXml));

                    var xsltArgs = new System.Xml.Xsl.XsltArgumentList();
                    var ewXsltExt = new Protean.xmlTools.xsltExtensions();
                    xsltArgs.AddExtensionObject("urn:ew", ewXsltExt);

                    var newAssem = Assembly.LoadFrom(AssemblyPath);
                    var CalledType = newAssem.GetType(className, true);
                    var oCStyle = new System.Xml.Xsl.XslCompiledTransform(true);
                    var sWriter = new StringWriter();

                    oCStyle.Load(CalledType);
                    TextWriter oWriter = new StringWriter();

                    oCStyle.Transform(oReader, xsltArgs, oWriter);

                    return oWriter.ToString();
                }

                catch (Exception ex)
                {
                    throw new InvalidOperationException(ex.Message, ex);
                }
            }
        }


    }
}
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;
using System.Xml;
using System.Xml.XPath;
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

        public class Transform
        {
#pragma warning disable 618
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
                        string sCompileVersion = Conversions.ToString(Operators.ConcatenateObject("v", goApp["XsltCompileVersion"]));
                        msXslFile = value.Replace("/", @"\");
                        ClassName = msXslFile.Substring(msXslFile.LastIndexOf(@"\") + 1);
                        ClassName = ClassName.Replace(".", "_") + sCompileVersion;
                        if (mbCompiled)
                        {

                            Assembly assemblyInstance = null;
                            AssemblyPath = goServer.MapPath(compiledFolder) + ClassName + ".dll";
                            Type CalledType;

                            if (Conversions.ToBoolean(goApp[ClassName]))
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
                                string compileResponse = CompileXSLTassembly(ClassName);
                                if ((compileResponse ?? "") == (ClassName ?? ""))
                                {
                                    // Dim assemblyBuffer As Byte() = File.ReadAllBytes(assemblypath)
                                    // assemblyInstance = xsltDomain.Load(assemblyBuffer)
                                    assemblyInstance = Assembly.LoadFrom(AssemblyPath);
                                }
                                else
                                {
                                    Information.Err().Raise(8000, msXslFile, compileResponse);
                                    assemblyInstance = null;
                                }
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
                                oStyle.Load(msXslFile);
                            }
                            msXslLastFile = msXslFile;
                        }
                    }
                    catch (Exception ex)
                    {
                        transformException = ex;
                        stdTools.returnException(ref myWeb.msException, "Protean.XmlHelper.Transform", "XslFilePath.Set", ex, msXslFile, value, gbDebug);
                        bError = true;
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

            public Transform(ref Cms aWeb, string sXslFile, bool bCompiled, long nTimeoutSec = 15000L, bool recompile = false)
            {
                string sProcessInfo = "";
                try
                {
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

                    if (recompile)
                    {

                        goApp["XsltCompileVersion"] = (Conversions.ToInteger(goApp["XsltCompileVersion"]) + 1).ToString();

                    }

                    // goApp("ewStarted") = True
                    // End If

                    XslFilePath = sXslFile;

                    string className = msXslFile.Substring(msXslFile.LastIndexOf(@"\") + 1);
                    className = className.Replace(".", "_");

                    if (mbCompiled == true & goApp[className] is null)
                    {
                        mnTimeoutSec = 60000L;
                    }
                    else
                    {
                        mnTimeoutSec = nTimeoutSec;
                    }

                    xsltArgs = new System.Xml.Xsl.XsltArgumentList();
                    var ewXsltExt = new Protean.xmlTools.xsltExtensions(ref myWeb);
                    xsltArgs.AddExtensionObject("urn:ew", ewXsltExt);
                }

                catch (Exception ex)
                {
                    transformException = ex;
                    stdTools.returnException(ref myWeb.msException, "Protean.XmlHelper.Transform", "New", ex, msXslFile, sProcessInfo, mbDebug);
                    bError = true;
                }
            }

            public void Close()
            {
                Dispose();
            }

            public void Dispose()
            {
                string sProcessInfo = "";
                try
                {
                    // AppDomain.Unload(xsltDomain)
                    oStyle = null;
                    oCStyle = null;
                    xsltArgs = null;
                }
                catch (Exception ex)
                {
                    stdTools.returnException(ref myWeb.msException, "Protean.XmlHelper.Transform", "Dispose", ex, msXslFile, sProcessInfo, mbDebug);

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
                            Information.Err().Raise(1010, "TranformXSL", "The XSL took longer than " + mnTimeoutSec / 1000d + " seconds to process");
                            bError = true;
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
                            Information.Err().Raise(1010, "TranformXSL", "The XSL took longer than " + mnTimeoutSec / 1000d + " seconds to process");
                            bError = true;
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
                            Information.Err().Raise(1010, "TranformXSL", "The XSL took longer than " + mnTimeoutSec / 1000d + " seconds to process");
                            bError = true;
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
                            Information.Err().Raise(1010, "TranformXSL", "The XSL took longer than " + mnTimeoutSec / 1000d + " seconds to process");
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
                        var resolver = new XmlUrlResolver();
                        resolver.Credentials = System.Net.CredentialCache.DefaultCredentials;

                        var ws = oCStyle.OutputSettings.Clone();
                        if (oCStyle != null)
                        {
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
                    currenti = Strings.AscW(current);

                    if (currenti == Conversions.ToInteger("&H9") || currenti == Conversions.ToInteger("&HA") || currenti == Conversions.ToInteger("&HD") || currenti >= Conversions.ToInteger("&H20") && currenti <= Conversions.ToInteger("&HD7FF") || currenti >= Conversions.ToInteger("&HE000") && currenti <= Conversions.ToInteger("&HFFFD") || currenti >= Conversions.ToInteger("&H10000") && currenti <= Conversions.ToInteger("&H10FFFF"))
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
                        catch (Exception)
                        {
                            // returnException("Protean.XmlHelper.Transform", "ClearXSLTassemblyCache", ex2, msXslFile, sProcessInfo)
                        }
                    }

                    if (myWeb.moConfig["AdminAcct"] == "")
                    {
                        oImp.UndoImpersonation();
                    }

                    // reset config to on
                    Config.UpdateConfigValue(ref myWeb, "protean/web", "CompliedTransform", "on");
                }

                // di.Delete(True)

                catch (Exception ex)
                {
                    bError = true;
                    stdTools.returnException(ref myWeb.msException, "Protean.XmlHelper.Transform", "ClearXSLTassemblyCache", ex, msXslFile, sProcessInfo, mbDebug);
                    return default;
                }

                return default;
            }


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
                        output = process1.StandardOutput.ReadToEnd();

                        // Wait for process to finish
                        process1.WaitForExit();

                        process1.Close();

                        goApp["compileLock-" + classname] = null;

                    }

                    if (output.Contains("error"))
                    {
                        return output;
                    }
                    else
                    {
                        return classname;
                    }
                }

                catch (Exception ex)
                {
                    bError = true;
                    stdTools.returnException(ref myWeb.msException, "Protean.XmlHelper.Transform", "CompileXSLTassembly", ex, msXslFile, sProcessInfo, mbDebug);
                    return null;
                }
            }
#pragma warning restore 618

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
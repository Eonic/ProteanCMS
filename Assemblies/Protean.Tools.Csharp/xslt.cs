using System;
using System.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Xml;
using System.Xml.Xsl;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Xml.XPath;

namespace Protean.Tools.Xslt
{
    public class XsltFunctions
    {
        public int stringcompare(string stringX, string stringY)
        {
            stringX = Strings.UCase(stringX);
            stringY = Strings.UCase(stringY);

            if (stringX == stringY)
                return 0;
            else if (Convert.ToDouble(stringX) < Convert.ToDouble(stringY))
                return -1;
            else if (Convert.ToDouble(stringX) > Convert.ToDouble(stringY))
                return 1;
            else
                return 0;
        }

        public string replacestring(string text, string replace, string replaceWith)
        {
            try
            {
                return text.Replace(replace, replaceWith);
            }
            catch (Exception ex)
            {
                return text;
            }
        }

        public string getdate(string dateString)
        {
            try
            {
                switch (dateString)
                {
                    case "now()":
                    case "Now()":
                    case "now":
                    case "Now":
                        {
                            dateString = System.Convert.ToString(Protean.Tools.Xml.XmlDate(DateTime.Now, true));
                            break;
                        }
                }
                return dateString;
            }
            catch (Exception ex)
            {
                return dateString;
            }
        }


        public string formatdate(string dateString, string dateFormat)
        {
            try
            {
                if (Information.IsDate(dateString))
                {
                    return DateTime.Parse(dateString).ToString();
                }
                else
                {
                    return dateString;
                }
            }
            catch (Exception ex)
            {
                return dateString;
            }
        }

        public string textafterlast(string text, string search)
        {
            try
            {
                if (text.LastIndexOf(search) > 0)
                    text = text.Substring(text.LastIndexOf(search) + search.Length);
                return text;
            }
            catch (Exception ex)
            {
                return text;
            }
        }

        public string textbeforelast(string text, string search)
        {
            try
            {
                if (text.LastIndexOf(search) > 0)
                    text = text.Substring(0, text.LastIndexOf(search));
                return text;
            }
            catch (Exception ex)
            {
                return text;
            }
        }

        public int randomnumber(int min, int max)
        {
            try
            {
                Random oRand = new Random();
                return oRand.Next(min, max);
            }
            catch (Exception ex)
            {
                return min;
            }
        }

        public string datediff(string date1String, string date2String, string datePart)
        {
            string nDiff = "";
            try
            {
                string[] ValidDatePart = new[] { "d", "y", "h", "n", "m", "q", "s", "w", "ww", "yyyy" };
                if (Information.IsDate(date1String) && Information.IsDate(date2String) && Array.IndexOf(ValidDatePart, datePart) > (ValidDatePart.GetLowerBound(0) - 1))

                    nDiff = Microsoft.VisualBasic.DateAndTime.DateDiff(datePart, DateTime.Parse(date1String), DateTime.Parse(date2String)).ToString();
                return nDiff;
            }
            catch (Exception ex)
            {
                return nDiff;
            }
        }
    }

    public class Transform
    {
        private string cFileLocation = ""; // Xsl File path
        private string cXslText = ""; // Xsl Blob
        private System.Xml.XPath.IXPathNavigable oXml; // Xml Document
        private bool bCompiled;  // If we want it to be compiled
        private int nTimeoutMillisec = 0; // Timeout in milliseconds (0 means not timed)
        private bool bDebuggable = false; // If we want to be able to step through the code
        private bool bClearXml = true; // Do we want to clear the Xml object after a transform (to reduce object size for caching)
        private bool bClearXmlDec = true; // Remove Xml Declaration

        private object oXslTExtensionsObject; // Any Class we want to use for additional functionality
        private string cXslTExtensionsURN; // The Urn for the class

        private XslTransform oClassicTransform; // Classic Transform Object
        private XslCompiledTransform oCompiledTransform; // Compiled Transform Object
        private XsltArgumentList XsltArgs; // Argurment List
        // Error Handling
        // General Error
        public event OnErrorEventHandler OnError;

        public delegate void OnErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);

        // Private TimeOutError
        private event OnTimeoutError_PrivateEventHandler OnTimeoutError_Private;

        private delegate void OnTimeoutError_PrivateEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);

        // Public TimeOutError
        public event OnTimeoutErrorEventHandler OnTimeoutError;

        public delegate void OnTimeoutErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);

        private XmlReader oXslReader; // to read the Xsl
        private XmlReaderSettings oXslReaderSettings;

        private const string mcModuleName = "Protean.Tools.Xml.XslTransform";

        private void EonicPrivateError_Handle(object sender, Protean.Tools.Errors.ErrorEventArgs e)
        {
            try
            {
                // make sure we close the reader properly
                oXslReader.Close();
            }
            catch (Exception ex)
            {
            }
            // raise the public timeout event
            OnTimeoutError?.Invoke(sender, e);
        }




        public string XslTFile
        {
            get
            {
                return cFileLocation;
            }
            set
            {
                cFileLocation = value;
            }
        }

        public string XslText
        {
            get
            {
                return cXslText;
            }
            set
            {
                cXslText = value;
            }
        }

        public bool Compiled
        {
            get
            {
                return bCompiled;
            }
            set
            {
                bCompiled = value;
            }
        }

        public int TimeoutSeconds
        {
            get
            {
                TimeSpan oTimeSpan = new TimeSpan(0, 0, 0, 0, nTimeoutMillisec);
                return (int)oTimeSpan.TotalSeconds;
            }
            set
            {
                TimeSpan oTimeSpan = new TimeSpan(0, 0, 0, value);
                nTimeoutMillisec = (int)oTimeSpan.TotalMilliseconds;
            }
        }

        public bool Debuggable
        {
            get
            {
                return bDebuggable;
            }
            set
            {
                bDebuggable = value;
            }
        }

        public System.Xml.XPath.IXPathNavigable Xml
        {
            get
            {
                return oXml;
            }
            set
            {
                oXml = value;
            }
        }

        public object XslTExtensionObject
        {
            get
            {
                return oXslTExtensionsObject;
            }
            set
            {
                oXslTExtensionsObject = value;
            }
        }

        public string XslTExtensionURN
        {
            get
            {
                return cXslTExtensionsURN;
            }
            set
            {
                cXslTExtensionsURN = value;
                if (!cXslTExtensionsURN.Contains("urn:"))
                    cXslTExtensionsURN = "urn:" + value;
            }
        }

        public bool ClearXmlAfterTransform
        {
            get
            {
                return bClearXml;
            }
            set
            {
                bClearXml = value;
            }
        }

        public bool RemoveXmlDeclaration
        {
            get
            {
                return bClearXmlDec;
            }
            set
            {
                bClearXmlDec = value;
            }
        }

        private delegate void TransformCompiledDelegate(ref string cResult);

        private void ProcessCompiledTimed(ref string cResult)
        {
            try
            {
                TransformCompiledDelegate d = new TransformCompiledDelegate(ProcessCompiled);
                IAsyncResult res = d.BeginInvoke(ref cResult, null, null);
                if (res.IsCompleted == false)
                {
                    res.AsyncWaitHandle.WaitOne(nTimeoutMillisec, false);
                    if (res.IsCompleted == false)
                    {
                        d.EndInvoke(ref cResult, (System.Runtime.Remoting.Messaging.AsyncResult)res);
                        d = null;
                        Hashtable oSettings = new Hashtable();
                        oSettings.Add("TimeoutSeconds", this.TimeoutSeconds);
                        oSettings.Add("Compiled", this.Compiled);
                        OnTimeoutError_Private?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ProcessCompiledTimed", new Exception("XsltTransformTimeout"), "", 0, oSettings));
                    }
                }
                if (!(d == null))
                    d.EndInvoke(ref cResult, (System.Runtime.Remoting.Messaging.AsyncResult)res);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ProcessCompiledTimed", ex, ""));
            }
        }

        private void ProcessCompiled(ref string cResult)
        {
            try
            {
                // make a resolver/setting
                XmlUrlResolver resolver = new XmlUrlResolver();
                resolver.Credentials = System.Net.CredentialCache.DefaultCredentials;

                // create the transform object
                if (oCompiledTransform == null)
                {
                    // if it is not nothing we will have already used it "cache"
                    oCompiledTransform = new XslCompiledTransform(bDebuggable);
                    // get the Xsl
                    if (!(cFileLocation == ""))
                    {
                        oXslReader = XmlReader.Create(cFileLocation, oXslReaderSettings);
                        oCompiledTransform.Load(oXslReader, XsltSettings.TrustedXslt, resolver);
                        oXslReader.Close();
                    }
                    else
                    {
                        XmlDocument oXsl = new XmlDocument();
                        oXsl.InnerXml = cXslText;
                        oCompiledTransform.Load(oXsl, XsltSettings.TrustedXslt, resolver);
                    }
                    // any vb functions we want to use make an extension object
                    XsltArgs = new XsltArgumentList();
                    if (!(oXslTExtensionsObject == null) & !(cXslTExtensionsURN == ""))
                        XsltArgs.AddExtensionObject(cXslTExtensionsURN, oXslTExtensionsObject);
                }
                // make a writer
                System.IO.TextWriter osWriter = new System.IO.StringWriter();
                // transform
                // Change the XmlDocument to xPathDocument to improve performance

                XPathDocument xpathDoc = new XPathDocument(new XmlNodeReader((XmlNode)oXml));
                oCompiledTransform.Transform(xpathDoc, XsltArgs, osWriter);
                // Return results
                cResult = osWriter.ToString();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "TransformCompiled", ex, ""));
                cResult = "";
            }
        }

        private delegate void TransformClassicDelegate(ref string cResult);

        private void ProcessClassicTimed(ref string cResult)
        {
            string sProcessInfo = "";
            try
            {
                TransformClassicDelegate d = new TransformClassicDelegate(ProcessClassic);
                IAsyncResult res = d.BeginInvoke(ref cResult, null, null);
                if (res.IsCompleted == false)
                {
                    res.AsyncWaitHandle.WaitOne(nTimeoutMillisec, false);
                    if (res.IsCompleted == false)
                    {
                        d.EndInvoke(ref cResult, (System.Runtime.Remoting.Messaging.AsyncResult)res);
                        d = null;
                        Hashtable oSettings = new Hashtable();
                        oSettings.Add("TimeoutSeconds", this.TimeoutSeconds);
                        oSettings.Add("Compiled", this.Compiled);
                        OnTimeoutError_Private?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ProcessClassicTimed", new Exception("XsltTransformTimeout"), "", 0, oSettings));
                    }
                }
                if (!(d == null))
                    d.EndInvoke(ref cResult, (System.Runtime.Remoting.Messaging.AsyncResult)res);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ProcessClassicTimed", ex, ""));
            }
        }

        private void ProcessClassic(ref string cResult)
        {
            try
            {
                // make a resolver/setting
                XmlUrlResolver resolver = new XmlUrlResolver();
                resolver.Credentials = System.Net.CredentialCache.DefaultCredentials;

                // create the transform object

                oClassicTransform = new XslTransform();
                // get the Xsl
                if (!(cFileLocation == ""))
                {
                    oXslReader = XmlReader.Create(cFileLocation, oXslReaderSettings);
                    oClassicTransform.Load(oXslReader, resolver);
                    oXslReader.Close();
                }
                else
                {
                    XmlDocument oXsl = new XmlDocument();
                    oXsl.InnerXml = cXslText;
                    oClassicTransform.Load(oXsl, resolver);
                }
                // any vb functions we want to use make an extension object
                XsltArgs = new XsltArgumentList();
                if (!(oXslTExtensionsObject == null) & !(cXslTExtensionsURN == ""))
                    XsltArgs.AddExtensionObject(cXslTExtensionsURN, oXslTExtensionsObject);
                // make a writer

                System.IO.TextWriter osWriter = new System.IO.StringWriter();

                // transform
                // Change the XmlDocument to xPathDocument to improve performance
                XPathDocument xpathDoc = new XPathDocument(new XmlNodeReader((XmlNode)oXml));


                oClassicTransform.Transform(xpathDoc, XsltArgs, osWriter, null/* TODO Change to default(_) if this is not a reference type */);
                // Return results
                cResult = osWriter.ToString();
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "TransformClassic", ex, ""));
                cResult = "";
            }
        }

        public string Process()
        {
            try
            {
                string cResult = "";
                if (bCompiled & nTimeoutMillisec > 0)
                    ProcessCompiledTimed(ref cResult);
                else if (!bCompiled & nTimeoutMillisec > 0)
                    ProcessClassicTimed(ref cResult);
                else if (bCompiled & nTimeoutMillisec == 0)
                    ProcessCompiled(ref cResult);
                else if (!bCompiled & nTimeoutMillisec == 0)
                    ProcessClassic(ref cResult);
                if (RemoveXmlDeclaration)
                    this.RemoveDeclaration(ref cResult);
                return cResult;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Transform", ex, ""));
                return "";
            }
            finally
            {
                if (bClearXml)
                    oXml = null;
            }
        }

        private void RemoveDeclaration(ref string cResult)
        {
            cResult = Strings.Replace(cResult, "<?Xml version=\"1.0\" encoding=\"utf-8\"?>", "");
            cResult = Strings.Replace(cResult, "<?Xml version=\"1.0\" encoding=\"utf-16\"?>", "");
            cResult = Strings.Trim(cResult);
        }

        public Transform()
        {
            try
            {
                oXslReaderSettings = new XmlReaderSettings();
                oXslReaderSettings.ProhibitDtd = false;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""));
            }
        }
    }
}


using System;
using System.Collections;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;


namespace Protean.Tools.Xslt
{
    public class XsltFunctions
    {
        public int stringcompare(string stringX, string stringY)
        {
            stringX = stringX.ToUpper();
            stringY = stringY.ToUpper();

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
            catch (Exception)
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
            catch (Exception)
            {
                return dateString;
            }
        }


        public string formatdate(string dateString, string dateFormat)
        {
            try
            {
                if (DateTime.TryParse(dateString, out _))
                {
                    return DateTime.Parse(dateString).ToString();
                }
                else
                {
                    return dateString;
                }
            }
            catch (Exception)
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
            catch (Exception)
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
            catch (Exception)
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
            catch (Exception)
            {
                return min;
            }
        }

        public string getGUID()
        {
            try
            {

                return System.Guid.NewGuid().ToString();
            }
            catch (Exception)
            {
                return "noguid";
            }
        }

        public string cleanname(string name)
        {
            try
            {
                return Protean.Tools.Text.CleanName(name, true, true);
            }
            catch (Exception)
            {
                return "error";
            }
        }

        public string datediff(string date1String, string date2String, string datePart)
        {
            string nDiff = "";
            try
            {
                string[] ValidDatePart = new[] { "d", "y", "h", "n", "m", "q", "s", "w", "ww", "yyyy" };

                // Check if date strings are valid and datePart is recognized
                if (DateTime.TryParse(date1String, out DateTime date1) &&
                    DateTime.TryParse(date2String, out DateTime date2) &&
                    Array.IndexOf(ValidDatePart, datePart) > ValidDatePart.GetLowerBound(0) - 1)
                {
                    // Calculate date difference using TimeSpan or custom logic for specific cases
                    TimeSpan ts = date2 - date1;

                    switch (datePart.ToLower())
                    {
                        case "d": // Days
                            nDiff = ts.TotalDays.ToString();
                            break;
                        case "h": // Hours
                            nDiff = ts.TotalHours.ToString();
                            break;
                        case "n": // Minutes
                            nDiff = ts.TotalMinutes.ToString();
                            break;
                        case "s": // Seconds
                            nDiff = ts.TotalSeconds.ToString();
                            break;
                        case "y": // Years (custom logic)
                            nDiff = (date2.Year - date1.Year).ToString();
                            break;
                        case "m": // Months (custom logic)
                            nDiff = ((date2.Year - date1.Year) * 12 + date2.Month - date1.Month).ToString();
                            break;
                        case "q": // Quarters (custom logic)
                            int quarter1 = (date1.Month - 1) / 3 + 1;
                            int quarter2 = (date2.Month - 1) / 3 + 1;
                            nDiff = ((date2.Year - date1.Year) * 4 + quarter2 - quarter1).ToString();
                            break;
                        case "w": // Weeks (approximate by dividing days by 7)
                            nDiff = (ts.TotalDays / 7).ToString();
                            break;
                        case "ww": // Calendar weeks (custom logic)
                            nDiff = (CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date2, CalendarWeekRule.FirstDay, DayOfWeek.Sunday) -
                                     CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date1, CalendarWeekRule.FirstDay, DayOfWeek.Sunday)).ToString();
                            break;
                        case "yyyy": // Full years
                            nDiff = (date2.Year - date1.Year).ToString();
                            break;
                        default:
                            nDiff = "Invalid date part";
                            break;
                    }
                }

                return nDiff;
            }
            catch (Exception)
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


        // private XslTransform oClassicTransform; // Classic Transform Object
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
            catch (Exception)
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

        private async Task ProcessCompiledTimed(string cResult)
        {
            try
            {
                var task = Task.Run(() => ProcessCompiled(ref cResult));

                if (await Task.WhenAny(task, Task.Delay(nTimeoutMillisec)) == task)
                {
                    await task;
                }
                else
                {
                    Hashtable oSettings = new Hashtable
            {
                { "TimeoutSeconds", this.TimeoutSeconds },
                { "Compiled", this.Compiled }
            };
                    OnTimeoutError_Private?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ProcessCompiledTimed", new Exception("XsltTransformTimeout"), "", 0, oSettings));
                }
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

        private async Task ProcessClassicTimed(string cResult)
        {
            string sProcessInfo = "ProcessClassicTimed";
            try
            {
                var task = Task.Run(() => ProcessClassic(ref cResult));

                if (await Task.WhenAny(task, Task.Delay(nTimeoutMillisec)) == task)
                {
                    await task;
                }
                else
                {
                    // Timeout occurred
                    Hashtable oSettings = new Hashtable
            {
                { "TimeoutSeconds", this.TimeoutSeconds },
                { "Compiled", this.Compiled }
            };
                    OnTimeoutError_Private?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, sProcessInfo, new Exception("XsltTransformTimeout"), "", 0, oSettings));
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, sProcessInfo, ex, ""));
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

                XslTransform oClassicTransform = new XslTransform();
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
                    ProcessCompiledTimed(cResult);
                else if (!bCompiled & nTimeoutMillisec > 0)
                    ProcessClassicTimed(cResult);
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
            cResult = cResult.Replace("<?Xml version=\"1.0\" encoding=\"utf-8\"?>", "");
            cResult = cResult.Replace("<?Xml version=\"1.0\" encoding=\"utf-16\"?>", "");
            cResult = cResult.Trim();
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


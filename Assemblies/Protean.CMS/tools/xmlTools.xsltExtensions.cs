using BundleTransformer.Core.Builders;
using BundleTransformer.Core.Bundles;
using BundleTransformer.Core.Orderers;
using BundleTransformer.Core.Transformers;
using Imazen.WebP;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using System.Web.Optimization;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.XPath;
using static Protean.stdTools;
using static Protean.Tools.Xml;
using static System.Globalization.CultureInfo;

namespace Protean
{
    public static partial class xmlTools
    {

        public class xsltExtensions
        {

            #region Declarations

            private Cms myWeb;
            // For saving object, dont want to keep chewing processing power during a transform
            public Hashtable oSaveHash;

            // 'for anything controlling web
            // Public Event OnError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs)

            // Protected Overridable Sub OnComponentError(ByVal sender As Object, ByVal e As Protean.Tools.Errors.ErrorEventArgs) ' Handles Me.OnError
            // 'deals with the error ' ALWAYS IN DEBUG MODE HERE !!!
            // returnException(e.ModuleName, e.ProcedureName, e.Exception, myWeb.mcEwSiteXsl, e.AddtionalInformation, True)
            // 'close connection pooling
            // If Not myWeb.moDbHelper Is Nothing Then
            // Try
            // myWeb.moDbHelper.CloseConnection()
            // Catch ex As Exception

            // End Try
            // End If
            // 'then raises a public event
            // RaiseEvent OnError(sender, e)
            // End Sub

            public string awaitingImgPath = "/ewcommon/images/awaiting-image-thumbnail.gif";

            #endregion

            #region Initialisation/Private
            public xsltExtensions(ref Cms aWeb)
            {
                myWeb = aWeb;
                if (myWeb.bs5)
                    awaitingImgPath = "/ptn/core/images/img-missing.png";

            }

            public xsltExtensions()
            {
                myWeb = null;
            }
            #endregion


            #region XSLT Functions

            private void SaveObject(string Name, object Item)
            {
                try
                {
                    if (oSaveHash is null)
                        oSaveHash = new Hashtable();
                    if (oSaveHash.Contains(Name))
                    {
                        oSaveHash[Name] = Item;
                    }
                    else
                    {
                        oSaveHash.Add(Name, Item);
                    }
                }
                catch (Exception)
                {
                    // Do Nothing
                }
            }

            private object GetObject(string Name)
            {
                if (oSaveHash != null)
                {
                    if (oSaveHash.Contains(Name))
                    {
                        return oSaveHash[Name];
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }


            public XmlElement GetContentInstance(string cXformName, string cModuleType)
            {

                var oXfrm = new xForm(ref myWeb.msException);

                if (!string.IsNullOrEmpty(cModuleType))
                    cXformName = cXformName + "/" + cModuleType;

                //ArrayList commonfolders = new ArrayList();
                string[] commonfolders = Array.Empty<string>();
                commonfolders.Append("");
                commonfolders.Append("/ewcommon");

                oXfrm.load("/xforms/content/" + cXformName + ".xml", commonfolders);

                return oXfrm.Instance;

            }

            public int stringcompare(string stringX, string stringY)
            {

                stringX = Strings.UCase(stringX);
                stringY = Strings.UCase(stringY);

                if ((stringX ?? "") == (stringY ?? ""))
                {
                    return 0;
                }
                else if (Operators.CompareString(stringX, stringY, false) < 0)
                {
                    return -1;
                }
                else if (Operators.CompareString(stringX, stringY, false) > 0)
                {
                    return 1;
                }

                return default;

            }




            public bool RegexTest(string input, string pattern)
            {

                bool result = false;


                if (!(string.IsNullOrEmpty(pattern) | string.IsNullOrEmpty(input)))
                {

                    try
                    {
                        result = Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase);
                    }
                    catch (Exception)
                    {
                        // Do nothing - jsut here is case the input or pattern are crap
                    }

                }

                return result;

            }


            public string RegexResult(string input, string pattern, string resultIndex)
            {

                string[] results;
                if (resultIndex == "")
                {
                    resultIndex = "0";
                }

                if (!(string.IsNullOrEmpty(pattern) | string.IsNullOrEmpty(input)))
                {
                    try
                    {
                        results = Regex.Split(input, pattern);
                        return results[Convert.ToInt16(resultIndex)];
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }
                else
                {
                    return "input or pattern not defined";
                }
            }



            public string ToTitleCase(string input)
            {

                return CurrentCulture.TextInfo.ToTitleCase(input);

            }

            public string Trim(string input)
            {

                return input.Trim();

            }

            public string TruncateString(string input, long limit)
            {

                return Tools.Text.TruncateString(input, limit);

            }

            public string replacestring(string text, string replace, string replaceWith)
            {
                try
                {
                    if (!string.IsNullOrEmpty(text) & !string.IsNullOrEmpty(replace))
                    {
                        return text.Replace(replace, replaceWith);
                    }
                    else
                    {
                        return text;
                    }
                }
                catch (Exception)
                {
                    return text;
                }
            }

            public string escapeJs(string text)
            {
                try
                {
                    return Tools.Text.EscapeJS(text);
                }
                catch (Exception)
                {
                    return text;
                }
            }

            public string escapeJson(string text)
            {
                try
                {

                    string orig = text;
                    if (!string.IsNullOrEmpty(text))
                    {
                        text = text.Replace(@"\", @"\\");
                        text = text.Replace("/", @"\/");
                        text = text.Replace("&#8;", @"\b");
                        text = text.Replace("&#13;", @"\r");
                        text = text.Replace("&#10;", @"\n");
                        text = text.Replace("#9;", @"\t");
                        text = text.Replace("\"", @"\""");
                    }
                    if ((orig ?? "") != (text ?? ""))
                    {
                        return text;
                    }
                    else
                    {
                        return orig;
                    }
                }
                catch (Exception)
                {
                    return text;
                }
            }

            public object escapeJsHTML(XmlNode oContextNode)
            {
                string Text = "";
                try
                {

                    Text = Conversions.ToString(oContextNode.OuterXml);

                    string orig = Text;
                    if (!string.IsNullOrEmpty(Text))
                    {
                        Text = Text.Replace("xmlns=\"http://www.w3.org/1999/xhtml\"", "");
                        Text = Text.Replace(@"\", @"\\");
                        Text = Text.Replace("&#13;", @"\r");
                        Text = Text.Replace("&#10;", @"\n");
                        Text = Text.Replace("#9;", @"\t");
                        Text = Text.Replace("'", @"\'");
                        Text = Text.Replace("’", @"\'");
                        Text = Text.Replace(Constants.vbCrLf, "");
                        Text = Text.Replace(Constants.vbLf, "");
                        Text = Text.Replace(Constants.vbCr, "");
                    }
                    if ((orig ?? "") != (Text ?? ""))
                    {
                        var oXML = new XmlDocument();
                        oXML.AppendChild(oXML.CreateElement("div"));
                        oXML.DocumentElement.InnerXml = Text;
                        return oXML.DocumentElement;
                    }
                    else
                    {
                        return orig;
                    }
                }
                catch (Exception)
                {
                    return Text;
                }
            }

            public string safeURL(string text)
            {
                try
                {
                    string orig = text;
                    text = HttpUtility.UrlEncode(text);
                    if ((orig ?? "") != (text ?? ""))
                    {
                        return text;
                    }
                    else
                    {
                        return orig;
                    }
                }
                catch (Exception)
                {
                    return text;
                }
            }

            public string cleantitle(string text)
            {
                try
                {
                    return "<span>" + text.Replace("&lt;", "<") + "</span>";
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
                    var oRand = new Random();
                    return oRand.Next(min, max);
                }
                catch (Exception)
                {
                    return min;
                }
            }

            public string randomseries(int min, int max, int count)
            {
                try
                {
                    var list = new List<int>();
                    var oRand = new Random();

                    if (max - min < count)
                    {
                        count = max - min;
                    }

                    while (count != 0)
                    {
                        int newNo = oRand.Next(min, max);
                        if (!list.Contains(newNo))
                        {
                            list.Add(newNo);
                            count = count - 1;
                        }
                    }

                    string returnStr = string.Empty;
                    int i;
                    var loopTo = list.Count - 1;
                    for (i = 0; i <= loopTo; i++)
                        returnStr = returnStr + list[i].ToString() + ",";
                    return returnStr.TrimEnd(',');
                }

                catch (Exception)
                {
                    return min.ToString();
                }
            }

            public string getGUID()
            {
                try
                {
                    return Guid.NewGuid().ToString();
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
                    return Tools.Text.CleanName(name, false, true);
                }
                catch (Exception)
                {
                    return "error";
                }
            }


            public string getdate(string dateString)
            {
                try
                {
                    switch (dateString ?? "")
                    {
                        case "now()":
                        case "Now()":
                        case "now":
                        case "Now":
                            {
                                dateString = XmlDate(DateTime.Now, true);
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


            public string dateadd(string dateString, long Interval, string IntervalType)
            {
                try
                {
                    if (Information.IsDate(dateString))
                    {
                        switch (IntervalType ?? "")
                        {
                            case "d":
                            case "D":
                            case "Day":
                            case "day":
                            case "DAY":
                                {
                                    dateString = Conversions.ToString(Conversions.ToDate(dateString).AddDays(Interval));
                                    break;
                                }
                            case "m":
                            case "M":
                            case "Month":
                            case "month":
                            case "MONTH":
                                {
                                    dateString = Conversions.ToString(Conversions.ToDate(dateString).AddMonths((int)Interval));
                                    break;
                                }
                            case "y":
                            case "Y":
                            case "Year":
                            case "year":
                            case "YEAR":
                                {
                                    dateString = Conversions.ToString(Conversions.ToDate(dateString).AddYears((int)Interval));
                                    break;
                                }
                        }

                    }
                    return XmlDate(dateString, false);
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
                    if (Information.IsDate(dateString))
                    {
                        dateString = Conversions.ToDate(dateString).ToString(dateFormat);
                    }
                    return dateString;
                }
                catch (Exception)
                {
                    return dateString;
                }
            }

            public string formatdate(string dateString, string dateFormat, string cultureIdentifier)
            {
                try
                {
                    if (Information.IsDate(dateString))
                    {
                        CultureInfo culture;
                        // Try to work out the culture.
                        if (string.IsNullOrEmpty(cultureIdentifier))
                        {
                            culture = CurrentCulture;
                        }
                        else
                        {
                            var re = new Regex("^([a-z]{2})(-([a-z]{2,3}))?$", RegexOptions.IgnoreCase);
                            var cultureParams = re.Match(cultureIdentifier);
                            if (cultureParams.Groups.Count == 4)
                            {
                                string language = cultureParams.Groups[1].Value;
                                string country = cultureParams.Groups[3].Value;
                                if (string.IsNullOrEmpty(country))
                                    country = language;
                                culture = new CultureInfo(language.ToLower() + "-" + country.ToUpper());
                            }
                            else
                            {
                                culture = CurrentCulture;
                            }
                        }

                        dateString = Conversions.ToDate(dateString).ToString(dateFormat, culture.DateTimeFormat);
                    }
                    return dateString;
                }
                catch (Exception)
                {
                    return dateString;
                }
            }

            public string datediff(string date1String, string date2String, string datePart)
            {
                string nDiff = "";
                try
                {
                    string[] ValidDatePart = new[] { "d", "y", "h", "n", "m", "q", "s", "w", "ww", "yyyy" };
                    if (Information.IsDate(date1String) && Information.IsDate(date2String) && Array.IndexOf(ValidDatePart, datePart) > ValidDatePart.GetLowerBound(0) - 1)

                    {

                        nDiff = DateAndTime.DateDiff(datePart, Conversions.ToDate(date1String), Conversions.ToDate(date2String)).ToString();
                    }
                    return nDiff;
                }
                catch (Exception)
                {
                    return nDiff;
                }
            }

            public string datedaylightsaving(string sInput)
            {
                // returns the date with the offset if it's in daylight saving
                string sReturn;
                DateTime dteWorking;

                try
                {
                    dteWorking = Conversions.ToDate(sInput);
                    if (dteWorking.IsDaylightSavingTime() == true)
                    {
                        dateadd(((int)DateInterval.Hour).ToString(), 1L, Conversions.ToString(dteWorking));
                        sReturn = formatDateISO8601(dteWorking) + "+01:00";
                    }
                    else
                    {
                        sReturn = sInput;
                    }
                }
                catch (Exception)
                {
                    sReturn = sInput;
                }
                return sReturn;
            }

            private string formatDateISO8601(DateTime dteIn)
            {
                try
                {
                    var strResult = new System.Text.StringBuilder();
                    strResult.Append(dteIn.Year);
                    strResult.Append("-");
                    strResult.Append(normaliseTwoCharacters(dteIn.Month.ToString()));
                    strResult.Append("-");
                    strResult.Append(normaliseTwoCharacters(dteIn.Day.ToString()));
                    strResult.Append("T");
                    strResult.Append(normaliseTwoCharacters(dteIn.Hour.ToString()));
                    strResult.Append(":");
                    strResult.Append(normaliseTwoCharacters(dteIn.Minute.ToString()));
                    strResult.Append(":");
                    strResult.Append(normaliseTwoCharacters(dteIn.Second.ToString()));
                    return strResult.ToString();
                }
                catch (Exception)
                {
                    return dteIn.ToString();
                }
            }

            private string normaliseTwoCharacters(string strIn)
            {
                try
                {
                    switch (strIn.Length)
                    {
                        case var @case when @case > 1:
                            {
                                return strIn;
                            }
                        case var case1 when case1 == 1:
                            {
                                return "0" + strIn;
                            }

                        default:
                            {
                                return "00";
                            }
                    }
                }
                catch (Exception)
                {
                    return "00";
                }
            }

            public string textafterlast(string text, string search)
            {
                try
                {
                    if (text.LastIndexOf(search) > 0)
                    {
                        text = text.Substring(text.LastIndexOf(search) + search.Length);
                    }
                    return text;
                }
                catch (Exception)
                {
                    return text;
                }
            }

            public string SubscriptionPrice(string nPrice, string cPriceUnit, int nDuration, string cDurationUnit)
            {

                // Gets the price of the subscription
                try
                {
                    DateTime dFinish;
                    switch (cDurationUnit ?? "")
                    {
                        case "Day":
                            {
                                dFinish = DateTime.Now.AddDays(nDuration);
                                break;
                            }
                        case "Week":
                            {
                                dFinish = DateTime.Now.AddDays(nDuration * 7);
                                break;
                            }
                        case "Month":
                            {
                                dFinish = DateTime.Now.AddMonths(nDuration);
                                break;
                            }
                        case "Year":
                            {
                                dFinish = DateTime.Now.AddYears(nDuration);
                                break;
                            }

                        default:
                            {
                                dFinish = DateTime.Now;
                                break;
                            }
                    }

                    double nEndPrice = 0d;
                    switch (cPriceUnit ?? "")
                    {
                        case "Day":
                            {
                                nEndPrice = (int)Math.Round(dFinish.Subtract(DateTime.Now).TotalDays) * Conversions.ToDouble(nPrice);
                                break;
                            }
                        case "Week":
                            {
                                nEndPrice = (double)RoundUp(dFinish.Subtract(DateTime.Now).TotalDays / 7d, 0, 0) * Conversions.ToDouble(nPrice);
                                break;
                            }
                        case "Month":
                            {
                                // this will be trickier
                                // need to step through each month
                                var dCurrent = DateTime.Now;
                                int nMonths = 0;
                                while (dCurrent < dFinish)
                                {
                                    dCurrent = dCurrent.AddMonths(1);
                                    nMonths += 1;
                                }
                                nEndPrice = nMonths * Conversions.ToDouble(nPrice);
                                break;
                            }
                        case "Year":
                            {
                                // same as months
                                var dCurrent = DateTime.Now;
                                int nYears = 0;
                                while (dCurrent < dFinish)
                                {
                                    dCurrent = dCurrent.AddYears(1);
                                    nYears += 1;
                                }
                                nEndPrice = nYears * Conversions.ToDouble(nPrice);
                                break;
                            }

                        default:
                            {
                                nEndPrice = 0d;
                                break;
                            }
                    }
                    return nEndPrice.ToString();
                }
                catch (Exception)
                {
                    return 0.ToString();
                }
            }

            public string CleanHTML(string cHTMLString)
            {
                try
                {
                    string cTheString = Strings.Replace(Strings.Replace(cHTMLString, "&gt;", ">"), "&lt;", "<");
                    cTheString = convertEntitiesToCodes(cTheString);
                    cTheString = stdTools.tidyXhtmlFrag(cTheString, true);
                    return cTheString;
                }
                catch (Exception)
                {
                    return cHTMLString;
                }
            }

            public object CleanHTMLNode(XPathNodeIterator oHtmlNode, string RemoveTags)
            {

                var oXML = new XmlDocument();
                string cHtml;
                string cHtmlOut;
                try
                {


                    if (oHtmlNode is null | string.IsNullOrEmpty(oHtmlNode.Current.InnerXml.Trim()))
                    {
                        cHtml = "";
                        return cHtml;
                    }
                    else
                    {
                        cHtml = oHtmlNode.Current.InnerXml;

                        cHtml = Strings.Replace(cHtml, "&amp;", "&");



                        cHtml = convertEntitiesToCodes(cHtml);
                        cHtml = convertStringToEntityCodes(cHtml);

                        cHtml = Strings.Replace(Strings.Replace(cHtml, "&gt;", ">"), "&lt;", "<");
                        cHtml = cHtml.Replace("&amp;#", "&#");
                        cHtml = "<div>" + cHtml + "</div>";
                        if (cHtml.Contains("<?xml"))
                        {
                            cHtml = Regex.Replace(cHtml, @"<\?xml*\?>/i", "", RegexOptions.IgnoreCase);
                            cHtml = cHtml.Replace("<?xml:namespace prefix = o ns = \"urn:schemas-microsoft-com:office:office\" />", "");

                            //cHtml = cHtml;
                        }

                        cHtmlOut = stdTools.tidyXhtmlFrag(cHtml, true, true, RemoveTags);

                        cHtmlOut = Strings.Replace(cHtmlOut, "&#x0;", "");
                        cHtmlOut = Strings.Replace(cHtmlOut, " &#0;", "");

                        if (string.IsNullOrEmpty(cHtmlOut) | string.IsNullOrEmpty(cHtmlOut))
                        {
                            return null;
                        }
                        else
                        {
                            try
                            {
                                cHtmlOut = cHtmlOut.Replace("&amp;#", "&#");
                                oXML.LoadXml(cHtmlOut);
                                return oXML.DocumentElement;
                            }
                            catch (Exception)
                            {
                                // Lets try option 2 first before we raise an error
                                // RaiseEvent XSLTError(ex.ToString)
                                try
                                {
                                    oXML = new XmlDocument();
                                    oXML.AppendChild(oXML.CreateElement("div"));
                                    oXML.DocumentElement.InnerXml = cHtmlOut;
                                    return oXML.DocumentElement;
                                }
                                catch (Exception)
                                {
                                    return cHtmlOut;
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    cHtml = "";
                    return cHtml;
                }
            }

            public object CleanHTMLElement(XPathNodeIterator oContextNode, string RemoveTags)
            {

                var oXML = new XmlDocument();
                string cHtml;
                string cHtmlOut;
                string cError;

                if (oContextNode is null)
                {
                    cHtml = "";
                    return cHtml;
                }
                else
                {
                    oContextNode.MoveNext();

                    cHtml = Conversions.ToString(oContextNode.Current.InnerXml);
                    cHtml = convertStringToEntityCodes(cHtml);
                    cHtml = convertEntitiesToCodesFast(cHtml);
                    cHtml = Strings.Replace(Strings.Replace(cHtml, "&gt;", ">"), "&lt;", "<");
                    cHtml = "<div>" + cHtml + "</div>";



                    cHtmlOut = stdTools.tidyXhtmlFrag(cHtml, true, true, RemoveTags);

                    cHtmlOut = Strings.Replace(cHtmlOut, "&#x0;", "");
                    cHtmlOut = Strings.Replace(cHtmlOut, " &#0;", "");

                    cHtmlOut = convertEntitiesToCodesFast(cHtmlOut);

                    if (string.IsNullOrEmpty(cHtmlOut) | string.IsNullOrEmpty(cHtmlOut) | (cHtmlOut ?? "") == Constants.vbCrLf)
                    {
                        return "";
                    }
                    else
                    {
                        try
                        {
                            // Return cHtmlOut
                            cHtmlOut = cHtmlOut.Replace("&amp;#", "&#");
                            oXML.LoadXml(cHtmlOut);
                            return oXML.DocumentElement;
                        }
                        catch (Exception ex)
                        {
                            // Lets try option 2 first before we raise an error
                            // RaiseEvent XSLTError(ex.ToString)
                            try
                            {
                                cError = ex.Message; //for breakpoint
                                oXML = new XmlDocument();
                                oXML.AppendChild(oXML.CreateElement("div"));
                                oXML.DocumentElement.InnerXml = cHtmlOut;
                                return oXML.DocumentElement;
                            }
                            catch (Exception ex2)
                            {
                                cError = ex2.Message; //for breakpoint
                                return cHtmlOut;
                            }
                        }
                    }
                }
            }

            public string EonicConfigValue(string SectionName, string ValueName)
            {
                return PtnConfigValue(SectionName, ValueName);
            }


            public string PtnConfigValue(string SectionName, string ValueName)
            {
                try
                {
                    if (Strings.LCase(SectionName) == "payment")
                        return "";
                    if (Strings.LCase(ValueName).Contains("password"))
                        return "";
                    System.Collections.Specialized.NameValueCollection oConfig = (System.Collections.Specialized.NameValueCollection)GetObject("EonicConfig_" + SectionName);

                    if (oConfig is null)
                    {
                        oConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/" + SectionName);
                        if (oConfig != null)
                        {
                            SaveObject("EonicConfig_" + SectionName, oConfig);
                        }
                    }

                    if (oConfig is null)
                    {
                        return "";
                    }
                    else
                    {
                        string returnVal;
                        returnVal = Conversions.ToString(Interaction.IIf(oConfig[ValueName] is null, "", oConfig[ValueName]));
                        if (returnVal is null)
                        {
                            return "";
                        }
                        else
                        {
                            return returnVal;
                        }
                    }
                }
                catch (Exception)
                {
                    return "";
                }
            }

            public string ServerVariable(string ValueName)
            {
                try
                {
                    if (myWeb != null)
                    {
                        return myWeb.moRequest.ServerVariables[ValueName];
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (Exception)
                {
                    return "";
                }
            }

            public string SessionVariable(string ValueName)
            {
                try
                {
                    if (myWeb != null)
                    {
                        return Convert.ToString(myWeb.moSession[ValueName]);
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (Exception)
                {
                    return "";
                }
            }

            public string GetContentLocations(int nContentId)
            {

                return GetContentLocations(nContentId, false, 0, false);

            }

            public string GetContentLocations(int nContentId, bool bIncludePrimary)
            {

                return GetContentLocations(nContentId, bIncludePrimary, 0, false);

            }

            public string GetContentLocations(int nContentId, bool bIncludePrimary, int bExcludeLocation)
            {

                return GetContentLocations(nContentId, bIncludePrimary, 0, false);

            }

            public string GetContentLocations(int nContentId, bool bIncludePrimary, int nExcludeLocation, bool bShowHiddenPages)
            {


                // Tried to do this as a node set, but returning a node set in non-XslCompiledTransform ways doesn't seem to work.
                string cLocations = "";

                try
                {

                    if (myWeb != null)
                    {


                        string cSql;
                        // Dim oDr As SqlClient.SqlDataReader

                        cSql = Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject("SELECT  l.nStructId As LocationID " + "FROM	dbo.tblContentStructure s INNER JOIN dbo.tblAudit a ON s.nauditid = a.nauditKey INNER JOIN dbo.tblContentLocation l ON s.nStructKey = l.nStructId " + "WHERE	nContentId = ", Interaction.IIf(Information.IsNumeric(nContentId), nContentId, -1)), " "));



                        if (bIncludePrimary)
                            cSql += " AND (l.bPrimary = 0) ";
                        if (Information.IsNumeric(nExcludeLocation))
                            cSql += " And (l.nStructId <> " + nExcludeLocation + ") ";
                        if (!bShowHiddenPages)
                            cSql += " And (a.dExpireDate Is NULL Or a.dExpireDate >= GETDATE()) And (a.dPublishDate Is NULL Or a.dPublishDate <= GETDATE()) And (a.nStatus <> 0) ";

                        cSql += " ORDER BY s.cStructName ";

                        using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable(cSql))  // Done by nita on 6/7/22
                        {

                            while (oDr.Read())
                                cLocations += "," + oDr[0].ToString();
                            oDr.Close();
                        }

                        cLocations = cLocations.TrimStart(',');

                    }
                    return cLocations;
                }
                catch (Exception)
                {
                    return "";
                }


            }
            public XmlElement GetSpecsOnPageItems(string pageId, string artId)
            {
                try
                {

                    int nPgId = Convert.ToInt16(pageId);


                    Protean.Cms myCMS = new Protean.Cms(myWeb.moCtx);
                    myCMS.InitializeVariables();
                    myCMS.mnPageId = nPgId;
                    myCMS.ibIndexMode = true;
                    myCMS.mbAdminMode = false;
                    XmlDocument myPageXml = myCMS.GetPageXML();

                    XmlElement grpElmt = myPageXml.CreateElement("group");
                    foreach (XmlElement SpecElmt in myPageXml.SelectNodes("descendant-or-self::Spec"))
                    {
                        string name = SpecElmt.GetAttribute("name");
                        if (name != "")
                        {
                            if (grpElmt.SelectSingleNode($"Spec[@name='{name}']") == null)
                            {
                                SpecElmt.InnerText = "";
                                grpElmt.AppendChild(SpecElmt);
                            }
                        }
                    }

                    if (artId != "")
                    {


                    };


                    return grpElmt;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            public string GetDirIdFromFref(string fRef)
            {

                string[] ids;

                ids = myWeb.moDbHelper.getObjectsByRef(Cms.dbHelper.objectTypes.Directory, fRef);

                if (ids != null)
                {
                    return ids[0];
                }
                else
                {
                    return "";
                }

            }
            //GetContentIdFromOrder(string orderRef, string ContentName)
            public string GetContentIdFromOrder(string orderRef, string ContentName)
            {
                if (orderRef.Contains("V4-"))
                {
                    orderRef = orderRef.Replace("V4-", "");
                }
                string nContentID = myWeb.moDbHelper.getContentIdFromOrder(orderRef, ContentName);

                if (nContentID != "")
                {
                    return nContentID.ToString();
                }
                else
                {
                    return "0";
                }

            }
            public string GetPageIdFromFref(string fRef)
            {

                string[] ids;

                ids = myWeb.moDbHelper.getObjectsByRef(Cms.dbHelper.objectTypes.ContentStructure, fRef);

                if (ids != null)
                {
                    return ids[0];
                }
                else
                {
                    return "";
                }

            }

            public string GetContentIdFromFref(string fRef)
            {

                long ids;
                myWeb.moDbHelper.ResetConnection(myWeb.moDbHelper.DatabaseConnectionString);
                ids = myWeb.moDbHelper.getObjectByRef(Cms.dbHelper.objectTypes.Content, fRef);

                if (!(ids == 0L))
                {
                    return ids.ToString();
                }
                else
                {
                    return "";
                }

            }

            public string GetProductGroupIdFromFref(string fRef)
            {

                long ids;
                myWeb.moDbHelper.ResetConnection(myWeb.moDbHelper.DatabaseConnectionString);
                ids = myWeb.moDbHelper.getObjectByRef(Cms.dbHelper.objectTypes.CartProductCategories, fRef);
                if (!(ids == 0L))
                {
                    return ids.ToString();
                }
                else
                {
                    return "";
                }

            }

            public string RelateProductToGroup(long ProductId, long ProductGroupId)
            {

                string ids;
                myWeb.moDbHelper.ResetConnection(myWeb.moDbHelper.DatabaseConnectionString);
                ids = myWeb.moDbHelper.insertProductGroupRelation((Int16)ProductId, ProductGroupId.ToString());
                return ids;

            }


            public string GetPageFref(int nPageId)
            {

                return myWeb.moDbHelper.getFRefFromPageId(nPageId);

            }


            public XmlElement GetUserXML(int nId)
            {

                return myWeb.moDbHelper.GetUserXML(nId);

            }

            public string FlattenNodeXml(XPathNodeIterator oContextNode)
            {
                try
                {
                    oContextNode.MoveNext();
                    return Conversions.ToString(oContextNode.Current.InnerXml);
                }
                catch (Exception)
                {
                    return "";
                }
            }

            public int VirtualFileExists(string cVirtualPath)
            {
                try
                {
                    string cVP = goServer.MapPath(cVirtualPath);
                    if (File.Exists(cVP))
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch (Exception)
                {
                    return 0;
                }
            }

            public int CompareDateIsNewer(string cOriginalPath, string cCheckNewerPath)
            {
                try
                {
                    string cOP = goServer.MapPath(cOriginalPath);
                    string cCNP = goServer.MapPath(cCheckNewerPath);
                    var cOPwritetime = File.GetLastWriteTime(cOP);
                    var cCNPwritetime = File.GetLastWriteTime(cCNP);
                    if (cOPwritetime > cCNPwritetime)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch (Exception)
                {
                    return 0;
                }
            }

            public string SaveImage(string imageUrl, string cVirtualPath)
            {

                try
                {
                    var oFS = new fsHelper(myWeb.moCtx);

                    // Get the file system parent folder of the first level folder incase it is mapped in IIS
                    string FirstFolder = cVirtualPath.TrimStart('/').Substring(0, cVirtualPath.TrimStart('/').IndexOf("/"));
                    string imgPath = goServer.MapPath("/" + FirstFolder + "/");
                    var newDir = new DirectoryInfo(imgPath);
                    oFS.mcStartFolder = newDir.Parent.FullName;

                    //handling for a mapped folder of a different name
                    if (newDir.Name != "images") {
                        cVirtualPath = cVirtualPath.Replace("images", newDir.Name);
                    }
                    // 'check to see if images path is mapped.
                    // If cVirtualPath.StartsWith("/images/") Then
                    // Dim imgPath As String = goServer.MapPath("/images/")
                    // Dim newDir As New DirectoryInfo(imgPath)
                    // oFS.mcStartFolder = newDir.Parent.FullName
                    // Else
                    // oFS.mcStartFolder = goServer.MapPath("/")
                    // End If

                    string savedFile = oFS.SaveFile(imageUrl, cVirtualPath);
                    if (newDir.Name != "images")
                    {
                        savedFile = savedFile.Replace(newDir.Name, "images");
                    }
                        return savedFile;
                }
                catch (Exception)
                {
                    return "";
                }
            }

            public string EncryptPassword(string sPassword)
            {
                // RJP 7 Nov 2012. Amended HashString to add use of config setting.
                try
                {
                    // Encrypt password.
                    sPassword = Tools.Encryption.HashString(sPassword, (myWeb.moConfig["MembershipEncryption"]).ToLower(), true); // plain - md5 - sha1
                                                                                                                                  // RJP removed the following two lines as they appear to be doing nothing  the encrypted string.
                                                                                                                                  // sPassword = Protean.Tools.Xml.EncodeForXml(sPassword)
                                                                                                                                  // sPassword = Protean.Tools.Xml.convertEntitiesToCodes(sPassword)
                    return sPassword;
                }
                catch (Exception)
                {
                    return "encryptionFailed";
                }
            }

            public string GenerateHashFromEncryptedPassword(string encryptionKey, string sPassword)
            {
                try
                {
                    // Decrypt password using provided key.
                    string sPlainPassword = Tools.Encryption.DecryptData(encryptionKey, sPassword);
                    // Encrypt using the protean.
                    sPassword = EncryptPassword(sPlainPassword);

                    return sPassword;
                }
                catch (Exception)
                {
                    return "encryptionFailed";
                }
            }

            public string ImageSize(string cVirtualPath)
            {

                try
                {
                    if (VirtualFileExists(cVirtualPath) > 0)
                    {
                        var oURI = new Uri(goServer.MapPath(cVirtualPath));
                        BitmapSource imageFromFile = BitmapFrame.Create(oURI);
                        int nWidth = imageFromFile.PixelWidth;
                        int nHeight = imageFromFile.PixelHeight;
                        oURI = null;
                        imageFromFile = null;
                        return nWidth + "x" + nHeight;
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (Exception)
                {
                    return "";
                }
            }

            public string ImageWidth(string cVirtualPath)
            {

                try
                {
                    if (VirtualFileExists(cVirtualPath) > 0)
                    {
                        BitmapSource imageFromFile = BitmapFrame.Create(new Uri(goServer.MapPath(cVirtualPath)));
                        int nWidth = imageFromFile.PixelWidth;
                        imageFromFile = null;
                        return nWidth.ToString();
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (Exception)
                {
                    return "";
                }
            }

            public string ImageHeight(string cVirtualPath)
            {

                try
                {
                    if (VirtualFileExists(cVirtualPath) > 0)
                    {
                        BitmapSource imageFromFile = BitmapFrame.Create(new Uri(goServer.MapPath(cVirtualPath)));
                        int nWidth = imageFromFile.PixelHeight;
                        imageFromFile = null;
                        return nWidth.ToString();
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (Exception)
                {
                    return "";
                }
            }


            // Public Function getShippingMethods(ByVal nWeight As Decimal, ByVal nPrice As Decimal, ByVal nQuantity As Integer) As XmlElement

            // Dim xmlResult As XmlElement = myWeb.moPageXml.CreateElement("ShippingMethods")
            // Dim xmlShippingMethods As XmlElement = myWeb.moPageXml.CreateElement("ShippingMethods")
            // Dim xmlTemp As XmlElement


            // 'get all the shipping options for a given shipping weight and price
            // Dim strSql As New Text.StringBuilder
            // strSql.Append("Select opt.cShipOptCarrier, opt.cShipOptTime, ")
            // strSql.Append("dbo.fxn_shippingTotal(opt.nShipOptKey, " & nPrice.ToString & ", " & nQuantity.ToString & ", " & nWeight.ToString & ") As nShippingTotal, ")
            // strSql.Append("tblCartShippingLocations.cLocationNameShort, tblCartShippingLocations.cLocationISOa2 ")

            // strSql.Append("FROM tblCartShippingLocations ")
            // strSql.Append("INNER JOIN tblCartShippingRelations On tblCartShippingLocations.nLocationKey = tblCartShippingRelations.nShpLocId ")
            // strSql.Append("RIGHT OUTER JOIN tblCartShippingMethods As opt ")
            // strSql.Append("INNER JOIN tblAudit On opt.nAuditId = tblAudit.nAuditKey On tblCartShippingRelations.nShpOptId = opt.nShipOptKey ")

            // strSql.Append("WHERE (opt.nShipOptQuantMin <= 0 Or opt.nShipOptQuantMin <= " & nQuantity.ToString & ") ")
            // strSql.Append("And (opt.nShipOptQuantMax <= 0 Or opt.nShipOptQuantMax >= " & nQuantity.ToString & ") ")
            // strSql.Append("And (opt.nShipOptPriceMin <= 0 Or opt.nShipOptPriceMin <= " & nPrice.ToString & ") ")
            // strSql.Append("And (opt.nShipOptWeightMin <= 0 Or opt.nShipOptWeightMin <= " & nWeight.ToString & ") ")
            // strSql.Append("And (opt.cCurrency Is NULL Or opt.cCurrency = '' OR opt.cCurrency = '') ")
            // strSql.Append("AND (tblAudit.nStatus > 0) ")
            // strSql.Append("AND (tblAudit.dPublishDate = 0 OR tblAudit.dPublishDate IS NULL OR tblAudit.dPublishDate <= " & Protean.Tools.Database.SqlDate(Now) & ") ")
            // strSql.Append("AND (tblAudit.dExpireDate = 0 OR tblAudit.dExpireDate IS NULL OR tblAudit.dExpireDate >= " & Protean.Tools.Database.SqlDate(Now) & ") ")


            // Dim oDs As DataSet = myWeb.moDbHelper.GetDataSet(strSql.ToString, "Method", "ShippingMethods", )
            // xmlShippingMethods.InnerXml = oDs.GetXml()

            // 'move all the shipping methods up a level
            // For Each xmlTemp In xmlShippingMethods.SelectNodes("ShippingMethods/Method")
            // xmlShippingMethods.AppendChild(xmlTemp)
            // Next

            // For Each xmlTemp In xmlShippingMethods.SelectNodes("ShippingMethods")
            // xmlShippingMethods.RemoveChild(xmlTemp)
            // Next


            // Return xmlShippingMethods

            // End Function


            /// <summary>
            /// Loads an image and creates a resized image in the same directory adding a suffix.
            /// </summary>
            /// <param name="cVirtualPath"></param>
            /// <param name="maxWidth"></param>
            /// <param name="maxHeight"></param>
            /// <param name="sSuffix"></param>
            /// <returns></returns>
            /// <remarks></remarks>
            public string ResizeImage(string cVirtualPath, long maxWidth, long maxHeight, string sSuffix)
            {
                try
                {
                    return ResizeImage(cVirtualPath, maxWidth, maxHeight, "", sSuffix, 99, false, false);
                }
                catch (Exception)
                {
                    return "Error";
                }
            }

            public string ResizeImage(string cVirtualPath, long maxWidth, long maxHeight, string sSuffix, int nCompression)
            {
                try
                {
                    return ResizeImage(cVirtualPath, maxWidth, maxHeight, "", sSuffix, nCompression, false, false);
                }
                catch (Exception)
                {
                    return "Error";
                }
            }

            public string ResizeImage(string cVirtualPath, long maxWidth, long maxHeight, string sPrefix, string sSuffix, int nCompression)
            {
                string newFilepath = string.Empty;
                try
                {
                    return ResizeImage(cVirtualPath, maxWidth, maxHeight, sPrefix, sSuffix, nCompression, false, false);
                }
                catch (Exception ex)
                {
                    return "Error - " + ex.Message;
                }
            }

            public string ResizeImage(string cVirtualPath, long maxWidth, long maxHeight, string sPrefix, string sSuffix, int nCompression, bool noStretch)
            {
                string newFilepath = string.Empty;
                try
                {
                    return ResizeImage(cVirtualPath, maxWidth, maxHeight, sPrefix, sSuffix, nCompression, noStretch, false);
                }
                catch (Exception ex)
                {
                    return "Error - " + ex.Message;
                }
            }

            public string ResizeImage(string cVirtualPath, long maxWidth, long maxHeight, string sPrefix, string sSuffix, int nCompression, bool noStretch, bool isCrop)
            {
                string newFilepath = string.Empty;
                try
                {
                    return ResizeImage2(cVirtualPath, maxWidth, maxHeight, sPrefix, sSuffix, nCompression, noStretch, isCrop, false);
                }
                catch (Exception ex)
                {
                    return "Error - " + ex.Message;
                }
            }

            public string ResizeImage2(string cVirtualPath, long maxWidth, long maxHeight, string sPrefix, string sSuffix, int nCompression, bool noStretch, bool isCrop, bool forceCheck)
            {
                string newFilepath = "";
                string cProcessInfo = "Resizing - " + cVirtualPath;


                try
                {
                    System.Collections.Specialized.NameValueCollection moConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/web");
                    if (!string.IsNullOrEmpty(moConfig["JpegQuality"]))
                    {
                        nCompression = Conversions.ToInteger(moConfig["JpegQuality"]);
                    }

                    // PerfMon.Log("xmlTools", "ResizeImage - Start")
                    if (myWeb != null)
                    {
                        if (myWeb.moRequest is null)
                        {
                        }

                        else
                        {
                            try
                            {
                                if (myWeb.moRequest["imgRefresh"] != "")
                                {
                                    forceCheck = true;
                                }
                            }
                            catch (Exception)
                            {

                            }

                        }
                    }


                    cVirtualPath = cVirtualPath.Replace("%20", " ");
                    // calculate the new filename
                    // dim get the filename
                    string filename = cVirtualPath.Substring(cVirtualPath.LastIndexOf("/") + 1);
                    string filetype = filename.Substring(filename.LastIndexOf(".") + 1);
                    string directoryPath = cVirtualPath.Substring(0, cVirtualPath.LastIndexOf("/") + 1);
                    string cVirtualPath2 = directoryPath + sPrefix + filename;

                    cVirtualPath2 = Strings.Replace(cVirtualPath2, "//", "/");

                    switch (filetype ?? "")
                    {
                        case "pdf":
                        case "doc":
                        case "docx":
                        case "gif":
                            {
                                newFilepath = Strings.Replace(cVirtualPath2, "." + filetype, sSuffix + ".png");
                                break;
                            }

                        default:
                            {
                                newFilepath = Strings.Replace(cVirtualPath2, "." + filetype, sSuffix + "." + filetype);
                                break;
                            }
                    }
                    if (myWeb is null)
                    {
                        return newFilepath;
                    }


                    else if (!myWeb.mbAdminMode & forceCheck == false)
                    {
                        return newFilepath;
                    }

                    else if (VirtualFileExists(cVirtualPath) > 0)
                    {

                        if (!(VirtualFileExists(newFilepath) > 0) | CompareDateIsNewer(cVirtualPath, newFilepath) > 0)
                        {
                            switch (filetype ?? "")
                            {
                                case "pdf":
                                    {

                                        var ihelp = new Tools.PDF();

                                        System.Threading.ThreadPool.SetMaxThreads(10, 10);
                                        var doneEvents = new System.Threading.ManualResetEvent[2];

                                        var newThumbnail = new Tools.PDF.PDFThumbNail();
                                        newThumbnail.FilePath = cVirtualPath;
                                        newThumbnail.newImageFilepath = newFilepath;
                                        newThumbnail.goServer = goServer;
                                        newThumbnail.maxWidth = (short)maxWidth;
                                        ihelp.GeneratePDFThumbNail(newThumbnail);


                                        // System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback((_) => ihelp.GeneratePDFThumbNail(newThumbnail)));
                                        newThumbnail = null;
                                        ihelp.Close();
                                        ihelp = null;
                                        break;
                                    }

                                default:
                                    {
                                        string cCheckServerPath = newFilepath.Substring(0, newFilepath.LastIndexOf("/") + 1);
                                        cCheckServerPath = goServer.MapPath(cCheckServerPath);
                                        // load the orignal image and resize
                                        var oImage = new Tools.Image(goServer.MapPath(cVirtualPath));
                                        oImage.KeepXYRelation = true;
                                        oImage.NoStretch = noStretch;
                                        oImage.IsCrop = isCrop;
                                        oImage.SetMaxSize((int)maxWidth, (int)maxHeight);

                                        oImage.Save(goServer.MapPath(newFilepath), nCompression, cCheckServerPath);

                                        var imgFile = new FileInfo(goServer.MapPath(newFilepath));
                                        var ptnImg = new Tools.Image("");
                                        ptnImg.TinifyKey = moConfig["TinifyKey"];
                                        ptnImg.CompressImage(imgFile, false);
                                        oImage.Close();
                                        oImage = null;
                                        break;
                                    }

                            }
                            // PerfMon.Log("xmlTools", "ResizeImage - End")
                            return newFilepath;
                        }
                        else
                        {
                            // PerfMon.Log("xmlTools", "ResizeImage - End")
                            return newFilepath;
                        }
                    }
                    else
                    {
                        // PerfMon.Log("xmlTools", "ResizeImage - End")
                        return awaitingImgPath;
                    }
                }
                catch (Exception ex)
                {
                    // PerfMon.Log("xmlTools", "ResizeImage - End")
                    if (gbDebug)
                    {
                        stdTools.reportException(ref myWeb.msException, "xmlTools.xsltExtensions", "ResizeImage2", ex, vstrFurtherInfo: cProcessInfo);
                        return awaitingImgPath + "?error=" + ex.Message + " - " + ex.StackTrace;
                    }
                    else
                    {
                        return awaitingImgPath + "error=" + ex.Message;
                    }
                }
            }

            public string CreateWebP(string cVirtualPath, string sForceCheck)
            {
                Boolean bForceCheck = false;
                if (sForceCheck.ToLower().Contains("true"))
                { bForceCheck = true; }
                return CreateWebPAlt(cVirtualPath, bForceCheck);
            }

            public string CreateWebPAlt(string cVirtualPath, bool forceCheck)
            {
                string cProcessInfo = string.Empty;
                try
                {

                    if (string.IsNullOrEmpty(cVirtualPath))
                    {

                        return awaitingImgPath;
                    }
                    else
                    {
                        short WebPQuality = 60;

                        if (myWeb.moConfig["WebPQuality"] != null)
                        {
                            WebPQuality = Convert.ToInt16(myWeb.moConfig["WebPQuality"]);
                        }

                        cVirtualPath = cVirtualPath.Replace("%20", " ");

                        string filename = cVirtualPath.Substring(cVirtualPath.LastIndexOf("/") + 1);
                        string filetype = filename.Substring(filename.LastIndexOf(".") + 1);
                        string directoryPath = cVirtualPath.Substring(0, cVirtualPath.LastIndexOf("/") + 1);


                        string webpFileName = Strings.Replace(cVirtualPath, "." + filetype, ".webp");
                        string newFilepath = string.Empty;
                        if (myWeb.mbAdminMode | forceCheck)
                        {
                            // create a WEBP version of the image.
                            if (VirtualFileExists(webpFileName) == 0)
                            {
                                using (var bitMap = new Bitmap(goServer.MapPath(cVirtualPath)))
                                {
                                    using (var saveImageStream = File.Open(goServer.MapPath(webpFileName), FileMode.Create))
                                    {
                                        var encoder = new SimpleEncoder();
                                        encoder.Encode(bitMap, saveImageStream, WebPQuality);
                                        encoder = null;
                                    }
                                }
                            }
                        }
                        return webpFileName;
                    }
                }


                catch (Exception ex)
                {
                    if ((myWeb.moConfig["Debug"]).ToLower() == "on")
                    {
                        // reportException("xmlTools.xsltExtensions", "ResizeImage2", ex, , cProcessInfo)
                        return awaitingImgPath + "?Error=" + ex.Message + " - " + ex.StackTrace;
                    }
                    else
                    {
                        return awaitingImgPath + "?Error=" + ex.Message;
                    }

                }
            }

            public string CreateWebP(string cVirtualPath)
            {
                string cProcessInfo = string.Empty;
                try
                {
                    return CreateWebPAlt(cVirtualPath, false);
                }
                catch (Exception ex)
                {
                    if ((myWeb.moConfig["Debug"]).ToLower() == "on")
                    {
                        // reportException("xmlTools.xsltExtensions", "ResizeImage2", ex, , cProcessInfo)
                        return awaitingImgPath + "?Error=" + ex.Message + " - " + ex.StackTrace;
                    }
                    else
                    {
                        return awaitingImgPath + "?Error=" + ex.Message;
                    }
                }
            }


            public string Watermark(string cVirtualPath, long maxWidth, long maxHeight, string sPrefix, string sSuffix, int nCompression, bool noStretch, bool isCrop, bool forceCheck, string WatermarkText, string WatermarkImage)
            {
                string newFilepath = "";
                string cProcessInfo = "Resizing - " + cVirtualPath;
                try
                {
                    // PerfMon.Log("xmlTools", "ResizeImage - Start")
                    if (myWeb.moRequest is null)
                    {
                    }

                    else
                    {
                        try
                        {
                            if (myWeb.moRequest["imgRefresh"] != "")
                            {
                                forceCheck = true;
                            }
                        }
                        catch (Exception)
                        {

                        }

                    }

                    cVirtualPath = cVirtualPath.Replace("%20", " ");
                    // calculate the new filename
                    // dim get the filename
                    string filename = cVirtualPath.Substring(cVirtualPath.LastIndexOf("/") + 1);
                    string filetype = filename.Substring(filename.LastIndexOf(".") + 1);
                    string directoryPath = cVirtualPath.Substring(0, cVirtualPath.LastIndexOf("/") + 1);
                    string cVirtualPath2 = directoryPath + sPrefix + filename;

                    cVirtualPath2 = Strings.Replace(cVirtualPath2, "//", "/");


                    switch (filetype ?? "")
                    {
                        case "pdf":
                        case "doc":
                        case "docx":
                        case "gif":
                            {
                                newFilepath = Strings.Replace(cVirtualPath2, "." + filetype, sSuffix + ".png");
                                break;
                            }

                        default:
                            {
                                newFilepath = Strings.Replace(cVirtualPath2, "." + filetype, sSuffix + "." + filetype);
                                break;
                            }
                    }

                    if (!myWeb.mbAdminMode & forceCheck == false)
                    {
                        return Strings.Replace(newFilepath, " ", "%20");
                    }

                    else if (VirtualFileExists(cVirtualPath) > 0)
                    {

                        if (!(VirtualFileExists(newFilepath) > 0) | CompareDateIsNewer(cVirtualPath, newFilepath) > 0)
                        {
                            switch (filetype ?? "")
                            {
                                case "pdf":
                                    {

                                        //var ihelp = new Tools.PDF.PDFThumbNail();

                                        //System.Threading.ThreadPool.SetMaxThreads(10, 10);
                                        //var doneEvents = new System.Threading.ManualResetEvent[2];

                                        //var newThumbnail = new Tools.PDF.PDFThumbNail();
                                        //newThumbnail.FilePath = cVirtualPath;
                                        //newThumbnail.newImageFilepath = newFilepath;
                                        //newThumbnail.goServer = goServer;
                                        //newThumbnail.maxWidth = (short)maxWidth;

                                        //System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback((_) => ihelp.GeneratePDFThumbNail(newThumbnail)));
                                        //newThumbnail = null;
                                        //ihelp.Close();
                                        //ihelp = null;
                                        break;
                                    }

                                default:
                                    {
                                        string cCheckServerPath = newFilepath.Substring(0, newFilepath.LastIndexOf("/") + 1);
                                        cCheckServerPath = goServer.MapPath(cCheckServerPath);
                                        // load the orignal image and resize
                                        var oImage = new Tools.Image(goServer.MapPath(cVirtualPath));
                                        oImage.KeepXYRelation = true;
                                        oImage.NoStretch = noStretch;
                                        oImage.IsCrop = isCrop;
                                        oImage.SetMaxSize((int)maxWidth, (int)maxHeight);

                                        if (!string.IsNullOrEmpty(WatermarkImage))
                                        {
                                            oImage.AddWatermark(WatermarkText, goServer.MapPath(WatermarkImage));
                                        }

                                        oImage.Save(goServer.MapPath(newFilepath), nCompression, cCheckServerPath);

                                        oImage.Close();
                                        oImage = null;
                                        break;
                                    }

                            }


                            // PerfMon.Log("xmlTools", "ResizeImage - End")
                            return Strings.Replace(newFilepath, " ", "%20");
                        }
                        else
                        {
                            // PerfMon.Log("xmlTools", "ResizeImage - End")
                            return Strings.Replace(newFilepath, " ", "%20");
                        }
                    }

                    else
                    {
                        // PerfMon.Log("xmlTools", "ResizeImage - End")
                        return awaitingImgPath;

                    }
                }


                catch (Exception ex)
                {
                    // PerfMon.Log("xmlTools", "ResizeImage - End")
                    if ((myWeb.moConfig["Debug"]).ToLower() == "on")
                    {
                        stdTools.reportException(ref myWeb.msException, "xmlTools.xsltExtensions", "ResizeImage2", ex, vstrFurtherInfo: cProcessInfo);
                        return awaitingImgPath + "?Error=" + ex.InnerException.Message + " - " + ex.Message + " - " + ex.StackTrace;
                    }
                    else
                    {
                        return awaitingImgPath + "?Error=" + ex.Message;
                    }

                }
            }


            public object ContentQuery(string cContentName, string cXpath, long nPrimaryId = 0L)
            {
                var oDocInstance = new XmlDocument();
                XmlElement targetElmt;
                XmlNode queryResult;
                string sSql;
                try
                {

                    targetElmt = oDocInstance.CreateElement("instance");
                    oDocInstance.AppendChild(targetElmt);
                    if (nPrimaryId > 0L)
                    {
                        sSql = "inner join tblContentLocation on tblContent.nContentKey = tblContentLocation.nContentId  where cContentName='" + cContentName + "' and tblContentLocation.bPrimary = true and tblContentLocation.nStructId=" + nPrimaryId;
                    }
                    else
                    {
                        sSql = "where cContentName='" + cContentName + "'";
                    }

                    targetElmt.InnerXml = myWeb.moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Content, default, "where cContentName='" + cContentName + "'");

                    queryResult = oDocInstance.DocumentElement.SelectSingleNode(cXpath);
                    if (queryResult != null)
                    {
                        switch (queryResult.NodeType)
                        {
                            case XmlNodeType.Attribute:
                                {
                                    return queryResult.Value;
                                }
                            case XmlNodeType.Text:
                                {
                                    return queryResult.Value;
                                }
                            case XmlNodeType.Element:
                                {
                                    return queryResult;
                                }

                            default:
                                {
                                    return "Unrecognised NodeType";
                                }
                        }
                    }
                    else if (string.IsNullOrEmpty(targetElmt.SelectSingleNode("descendant-or-self::nContentKey").InnerText))
                    {
                        return "Error-ContentMissing";
                    }
                    else
                    {
                        return "Error-PathNotResolved: " + cXpath;
                    }
                }

                catch (Exception ex)
                {
                    return "Error-" + ex.Message;
                }

            }

            public object ContentQuery(string fRef)
            {
                var oDocInstance = new XmlDocument();
                XmlNode returnNode;
                try
                {

                    oDocInstance.LoadXml(myWeb.moDbHelper.getObjectInstance(Cms.dbHelper.objectTypes.Content, default, "where cContentForiegnRef='" + fRef + "'"));
                    returnNode = oDocInstance.DocumentElement;
                    return returnNode;
                }

                catch (Exception ex)
                {
                    return "Error-" + ex.Message;
                }

            }

            public string evaluateXpath(XPathNodeIterator nodes, string xpath)
            {
                try
                {
                    while (nodes.MoveNext())
                    {
                        var n = nodes.Current as XPathNavigator;
                        return n.Evaluate(xpath).ToString();
                    }
                }
                catch (Exception ex)
                {
                    return "Error - Not Deleted" + ex.Message;
                }
                return null;
            }

            public object evaluateXpathObj(XPathNodeIterator nodes, string xpath)
            {
                try
                {
                    while (nodes.MoveNext())
                    {
                        var n = nodes.Current as XPathNavigator;
                        return n.Evaluate(xpath);
                    }
                }
                catch (Exception ex)
                {
                    return "Error - Not Deleted" + ex.Message;
                }
                return null;
            }

            public string DeleteContent(string nContentId)
            {
                try
                {
                    if (Conversions.ToLong("0" + nContentId) > 0L)
                    {
                        return myWeb.moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.Content, Convert.ToInt64(nContentId)).ToString();
                    }
                    else
                    {
                        return null;
                    }
                }

                catch (Exception ex)
                {
                    return "Error - Not Deleted" + ex.Message;
                }
            }

            public string DeletePage(string nPageId)
            {
                try
                {
                    if (Conversions.ToLong("0" + nPageId) > 0L)
                    {
                        return myWeb.moDbHelper.DeleteObject(Cms.dbHelper.objectTypes.ContentStructure, Convert.ToInt64(nPageId)).ToString();
                    }
                    else
                    {
                        return null;
                    }
                }

                catch (Exception ex)
                {
                    return "Error - Not Deleted" + ex.Message;
                }
            }

            public string UpdatePositions(long nContentId, string cPosition)
            {

                string cPositions;
                string[] aPositions;
                int i;

                try
                {

                    cPositions = GetContentLocations((int)nContentId, true);
                    aPositions = Strings.Split(cPositions, ",");
                    var loopTo = Information.UBound(aPositions);
                    for (i = 0; i <= loopTo; i++)
                        myWeb.moDbHelper.updatePagePosition(Conversions.ToLong(aPositions[i]), nContentId, cPosition);

                    return cPosition;
                }

                catch (Exception ex)
                {
                    return "Error - Changed Position" + ex.Message;
                }

            }
            /// <summary>
            /// Its a m
            /// </summary>
            /// <returns></returns>
            public object GetFilterButtons()
            {
                try
                {
                    XmlDocument oXformDoc;
                    string buttonName = string.Empty;
                    string projectPath = "";
                    if (myWeb.moConfig["ProjectPath"] != string.Empty)
                    {
                        projectPath = myWeb.moConfig["ProjectPath"];
                    }
                    var oAdminXforms = new Cms.Admin.AdminXforms(ref myWeb);
                    oXformDoc = oAdminXforms.GetSiteManifest(myWeb.moConfig["cssFramework"]);

                    var xmlButtons = oXformDoc.CreateElement("buttons");
                    foreach (XmlElement oChoices in oXformDoc.SelectNodes("/PageLayouts/FilterTypes/Filter"))
                    {
                        var buttonElement = oXformDoc.CreateElement("button");
                        buttonElement.InnerText = oChoices.InnerText;
                        buttonElement.SetAttribute("name", oChoices.InnerText);
                        buttonElement.SetAttribute("filterType", oChoices.Attributes["type"].Value);
                        xmlButtons.AppendChild(buttonElement);

                    }
                    return xmlButtons;
                }

                catch (Exception ex)
                {
                    return "Error - Filter Buttons" + ex.Message;
                }


            }


            public object GetSelectOptions(string Query)
            {
                // Dim SelectDoc As New XmlDocument()
                XmlElement SelectElmt = myWeb.moPageXml.CreateElement("select1");
                string Query1 = "";
                string Query2 = "";
                string Query3 = "";
                string sql = string.Empty;
                try
                {


                    string[] QueryArr = Strings.Split(Query, ".");
                    Query1 = QueryArr[0];
                    if (Information.UBound(QueryArr) > 0)
                        Query2 = QueryArr[1];
                    if (Information.UBound(QueryArr) > 1)
                        Query3 = QueryArr[2];
                    var oXfrms = new Cms.xForm(ref myWeb.msException);
                    oXfrms.moPageXML = myWeb.moPageXml;
                    switch (Query1 ?? "")
                    {
                        case "SiteTree":
                            {
                                XmlElement StructElmt = myWeb.GetStructureXML(myWeb.mnUserId, 0, 0, "Site", false, false, false, true, false, "MenuItem", "Menu");

                                foreach (XmlElement MenuElmt in StructElmt.SelectNodes("descendant-or-self::MenuItem"))
                                {
                                    string Label = MenuElmt.GetAttribute("name");
                                    string Value = MenuElmt.GetAttribute("id");
                                    foreach (XmlElement ParElmt in MenuElmt.SelectNodes("ancestor::MenuItem"))
                                        Label = "-" + Label;
                                    oXfrms.addOption(ref SelectElmt, Label, Value);
                                }

                                break;
                            }
                        case "Directory":
                            {
                                // Returns all of a specified type in the directory to specify the type use attribute "query2"

                                sql = "select nDirKey as value, cDirName as name from tblDirectory where cDirSchema='" + Query2 + "'";
                                using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable(sql))  // Done by nita on 6/7/22
                                {
                                    SqlDataReader sqloDr = (SqlDataReader)oDr;
                                    oXfrms.addOptionsFromSqlDataReader(ref SelectElmt, ref sqloDr);
                                }

                                break;
                            }

                        case "DirectoryName":
                            {
                                // Returns all of a specified type in the directory to specify the type use attribute "query2"

                                sql = "select cDirName as value, cDirName as name from tblDirectory where cDirSchema='" + Query2 + "'";
                                using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable(sql))  // Done by nita on 6/7/22
                                {
                                    oXfrms.addOption(ref SelectElmt, "All", "all");
                                    SqlDataReader sqloDr = (SqlDataReader)oDr;
                                    oXfrms.addOptionsFromSqlDataReader(ref SelectElmt, ref sqloDr);
                                }

                                break;
                            }

                        case "ShippingMethods":
                            {
                                // Returns all of a specified type in the directory to specify the type use attribute "query2"

                                sql = "select nShipOptKey as value, cShipOptName as name from tblCartShippingMethods";
                                using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable(sql))  // Done by nita on 6/7/22
                                {
                                    oXfrms.addOption(ref SelectElmt, "All", "all");
                                    SqlDataReader sqloDr = (SqlDataReader)oDr;
                                    oXfrms.addOptionsFromSqlDataReader(ref SelectElmt, ref sqloDr);
                                }

                                break;
                            }
                        case "Users":
                            {

                                // This is different from Directory in that it tries to format the users nicely
                                // We'll use a subquery to get the data and then reduce it down to the calculated columns which we can order
                                var subqueryBuilder = new System.Text.StringBuilder();
                                subqueryBuilder.Append("SELECT nDirKey").Append(",");
                                subqueryBuilder.Append("CAST(CAST(users.cDirXml as xml).query('string(/User[1]/FirstName[1])') as nvarchar(MAX)) As FirstName").Append(",");
                                subqueryBuilder.Append("CAST(CAST(users.cDirXml as xml).query('string(/User[1]/LastName[1])') as nvarchar(MAX)) As LastName").Append(",");
                                subqueryBuilder.Append("users.cDirName").Append(" ");
                                subqueryBuilder.Append("FROM dbo.tblDirectory users").Append(" ");

                                // Check whether we're filtering users by a parent id
                                if (Tools.Number.IsReallyNumeric(Query2) && Convert.ToInt32(Query2) > 0)
                                {
                                    subqueryBuilder.Append(string.Format("INNER JOIN dbo.fxn_getMembers({0},1,'User',0,0,GETDATE(),0) members", Query2)).Append(" ");
                                    subqueryBuilder.Append("ON users.nDirKey = members.nDirId").Append(" ");
                                }
                                subqueryBuilder.Append("WHERE users.cDirSchema='User'");

                                // Now reduce the query down to what is needed from it and in what order.
                                var queryBuilder = new System.Text.StringBuilder();
                                queryBuilder.Append("SELECT nDirKey As value").Append(",");
                                queryBuilder.Append("(FirstName + ' ' + LastName + ' (' + cDirName + ')') As name").Append(" ");
                                queryBuilder.Append(string.Format("FROM ({0}) u", subqueryBuilder.ToString())).Append(" ");
                                queryBuilder.Append("ORDER BY Lastname, FirstName");

                                // Dim oDr As System.Data.SqlClient.SqlDataReader = myWeb.moDbHelper.getDataReader(queryBuilder.ToString())
                                using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable(queryBuilder.ToString()))  // Done by nita on 6/7/22
                                {
                                    SqlDataReader sqloDr = (SqlDataReader)oDr;
                                    oXfrms.addOptionsFromSqlDataReader(ref SelectElmt, ref sqloDr);
                                }

                                break;
                            }

                        case "Content":
                            {

                                sql = "select nContentKey as value, cContentName as name from tblContent where cContentSchemaName='" + Query2 + "' order by cContentName ASC";
                                using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable(sql))  // Done by nita on 6/7/22
                                {
                                    SqlDataReader sqloDr = (SqlDataReader)oDr;
                                    oXfrms.addOptionsFromSqlDataReader(ref SelectElmt, ref sqloDr);
                                }

                                break;
                            }

                        case "CartStatus":
                            {
                                foreach (Protean.Cms.Cart.cartProcess process in Enum.GetValues(typeof(Cms.Cart.cartProcess)))
                                    oXfrms.addOption(ref SelectElmt, process.ToString(), process.ToString("D"));
                                break;
                            }

                        case "Language":
                            {

                                if (myWeb.goLangConfig != null)
                                {
                                    oXfrms.addOption(ref SelectElmt, myWeb.goLangConfig.GetAttribute("default"), myWeb.goLangConfig.GetAttribute("code"));
                                    foreach (XmlElement langNode in myWeb.goLangConfig.SelectNodes("Language"))
                                        oXfrms.addOption(ref SelectElmt, langNode.GetAttribute("systemName"), langNode.GetAttribute("code"));
                                }
                                else
                                {
                                    oXfrms.addOption(ref SelectElmt, "English-UK", "en-gb");
                                }

                                break;
                            }

                        case "Countries":
                            {
                                var oCart = new Cms.Cart(ref myWeb);
                                oCart.populateCountriesDropDown(ref oXfrms, ref SelectElmt, "");
                                break;
                            }

                        case "CountriesId":
                            {
                                var oCart = new Cms.Cart(ref myWeb);
                                oCart.populateCountriesDropDown(ref oXfrms, ref SelectElmt, "", true);
                                break;
                            }
                        case "Currency":
                            {
                                XmlNode moPaymentCfg;
                                moPaymentCfg = (XmlNode)WebConfigurationManager.GetWebApplicationSection("protean/payment");
                                foreach (XmlElement oCurrencyElmt in moPaymentCfg.SelectNodes("currencies/Currency"))
                                    // going to need to do something about languages
                                    oXfrms.addOption(ref SelectElmt, oCurrencyElmt.SelectSingleNode("name").InnerText, oCurrencyElmt.GetAttribute("ref"));
                                break;
                            }

                        case "Library":
                            {

                                string ProviderName = string.Empty;
                                Type calledType;
                                string classPath = string.Empty;
                                string methodName = string.Empty;

                                Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection("protean/messagingProviders");
                                var assemblyInstance = Assembly.Load(moPrvConfig.Providers[ProviderName].Type);
                                calledType = assemblyInstance.GetType(classPath, true);

                                var o = Activator.CreateInstance(calledType);

                                var args = new object[2];
                                args[0] = SelectElmt;

                                calledType.InvokeMember(methodName, BindingFlags.InvokeMethod, null, o, args);
                                break;
                            }


                        case "FolderList":
                            {

                                fsHelper.LibraryType library = 0; object output = "";
                                if (Tools.EnumUtility.TryParse(typeof(Protean.fsHelper.LibraryType), Query2, false, ref output))
                                {

                                    string path = fsHelper.GetFileLibraryPath(library);
                                    if (!string.IsNullOrEmpty(path))
                                    {

                                        var rootfolder = new DirectoryInfo(myWeb.goServer.MapPath("/") + path.Trim(@"/\".ToCharArray()));
                                        string prefixFolderPath = rootfolder.FullName.Substring(0, rootfolder.FullName.LastIndexOf(@"\"));


                                        if (rootfolder.Exists)
                                        {
                                            List<string> folderList = fsHelper.EnumerateFolders(rootfolder);
                                            string tidypath = "";
                                            foreach (string folderPath in folderList)
                                            {
                                                tidypath = folderPath.Replace(prefixFolderPath, "").Replace(@"\", "/");
                                                oXfrms.addOption(ref SelectElmt, tidypath, tidypath);
                                            }
                                        }

                                    }


                                }

                                break;
                            }
                        case "FileList":
                            {

                                string path = myWeb.goServer.MapPath("/") + Query2.Trim(@"/\".ToCharArray());
                                if (!string.IsNullOrEmpty(path))
                                {

                                    var rootfolder = new DirectoryInfo(path);

                                    FileInfo[] files = rootfolder.GetFiles();

                                    foreach (var fi in files)
                                    {
                                        string cExt = Strings.LCase(fi.Extension);
                                        string tidypath = "/" + Query2.Trim(@"/\".ToCharArray()) + "/" + fi.Name;

                                        oXfrms.addOption(ref SelectElmt, fi.Name.Replace(fi.Extension, ""), tidypath);

                                    }

                                }

                                break;
                            }

                        case "CodeGroups":
                            {
                                // Returns all of a specified type in the directory to specify the type use attribute "query2"

                                sql = "select nCodeKey as value, cCodeName as name from tblCodes where nCodeParentId is NULL or nCodeParentId = 0";
                                using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable(sql))  // Done by nita on 6/7/22
                                {
                                    SqlDataReader sqloDr = (SqlDataReader)oDr;
                                    oXfrms.addOptionsFromSqlDataReader(ref SelectElmt, ref sqloDr);
                                }

                                break;
                            }
                        case "Lookup":
                            {
                                // Returns all of a specified type in the directory to specify the type use attribute "query2"
                                if (!string.IsNullOrEmpty(Query3))
                                {
                                    switch (Query3 ?? "")
                                    {
                                        case "sortaz":
                                            {
                                                // check sorting is present
                                                sql = "select cLkpValue as value, cLkpKey as name from tblLookup where cLkpCategory like '" + Query2 + "' order by cLkpKey, nDisplayOrder, nLkpID";
                                                break;
                                            }
                                    }
                                }
                                else
                                {
                                    sql = "select cLkpValue as value, cLkpKey as name from tblLookup where cLkpCategory like '" + Query2 + "' order by nDisplayOrder, nLkpID";
                                }

                                using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable(sql))  // Done by nita on 6/7/22
                                {
                                    SqlDataReader sqloDr = (SqlDataReader)oDr;
                                    oXfrms.addOptionsFromSqlDataReader(ref SelectElmt, ref sqloDr);
                                }

                                break;
                            }
                        case "availableIcons":
                            {
                                string iconPath = "/ewcommon/icons/icons.xml";
                                if (myWeb.bs5)
                                    iconPath = "/ptn/core/icons/icons.xml";

                                if (File.Exists(goServer.MapPath(iconPath)))
                                {
                                    var newXml = new XmlDocument();
                                    newXml.PreserveWhitespace = true;
                                    newXml.Load(goServer.MapPath(iconPath));
                                    SelectElmt.InnerXml = newXml.DocumentElement.InnerXml;
                                }

                                break;
                            }

                        case "themePresets":
                            {
                                System.Collections.Specialized.NameValueCollection moThemeConfig = (System.Collections.Specialized.NameValueCollection)WebConfigurationManager.GetWebApplicationSection("protean/theme");
                                string currenttheme = moThemeConfig["CurrentTheme"];

                                if (File.Exists(goServer.MapPath("/ewthemes/" + currenttheme + "/themeManifest.xml")))
                                {
                                    var newXml = new XmlDocument();
                                    newXml.PreserveWhitespace = true;
                                    newXml.Load(goServer.MapPath("/ewthemes/" + currenttheme + "/themeManifest.xml"));
                                    foreach (XmlElement oElmt in newXml.SelectNodes("/Theme/Presets/Preset"))
                                    {
                                        var ItemElmt = SelectElmt.OwnerDocument.CreateElement("item");
                                        var LabelElmt = SelectElmt.OwnerDocument.CreateElement("label");
                                        LabelElmt.InnerText = oElmt.GetAttribute("name");
                                        ItemElmt.AppendChild(LabelElmt);
                                        var ValueElmt = SelectElmt.OwnerDocument.CreateElement("value");
                                        ValueElmt.InnerText = oElmt.GetAttribute("name");
                                        ItemElmt.AppendChild(ValueElmt);
                                        SelectElmt.AppendChild(ItemElmt);
                                    }
                                }

                                break;
                            }

                        case "ProductGroups":
                            {

                                sql = "select nCatKey as value, cCatName as name from tblCartProductCategories";
                                if (!string.IsNullOrEmpty(Query2))
                                {
                                    sql = sql + " where cCatSchemaName='" + Query2 + "'";
                                }
                                sql = sql + " order by cCatName";
                                using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable(sql))  // Done by nita on 6/7/22
                                {
                                    SqlDataReader sqloDr = (SqlDataReader)oDr;
                                    oXfrms.addOptionsFromSqlDataReader(ref SelectElmt, ref sqloDr);
                                }

                                break;
                            }
                        case "Subscriptions":
                            {

                                string sSql = "SELECT nContentKey as value, cContentName as name  FROM tblContent LEFT OUTER JOIN tblCartCatProductRelations ON tblContent.nContentKey = tblCartCatProductRelations.nContentId WHERE (tblContent.cContentSchemaName = 'Subscription') Order By tblCartCatProductRelations.nDisplayOrder";
                                using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable(sSql))  // Done by nita on 6/7/22
                                {
                                    SqlDataReader sqloDr = (SqlDataReader)oDr;
                                    oXfrms.addOptionsFromSqlDataReader(ref SelectElmt, ref sqloDr);
                                }

                                break;
                            }

                        default:
                            {
                                sql = Query1;
                                using (SqlDataReader oDr = myWeb.moDbHelper.getDataReaderDisposable(sql))  // Done by nita on 6/7/22
                                {
                                    SqlDataReader sqloDr = (SqlDataReader)oDr;
                                    oXfrms.addOptionsFromSqlDataReader(ref SelectElmt, ref sqloDr);
                                }

                                break;
                            }
                    }

                    // Return cPosition
                    oXfrms = default;

                    return SelectElmt;
                }

                catch (Exception ex)
                {
                    return "Error - Changed Position" + ex.Message;
                }

            }

            public object GetSelectOptions(string ProviderName, string classPath, string methodName)
            {
                // Dim SelectDoc As New XmlDocument()
                XmlElement SelectElmt = myWeb.moPageXml.CreateElement("select1");

                try
                {
                    var aMethodName = default(string[]);
                    if (methodName.Contains("."))
                    {
                        aMethodName = methodName.Split('.');
                        methodName = aMethodName[0];
                    }

                    Protean.ProviderSectionHandler moPrvConfig = (Protean.ProviderSectionHandler)WebConfigurationManager.GetWebApplicationSection("protean/messagingProviders");
                    // Dim assemblyInstance As [Assembly] = [Assembly].Load(moPrvConfig.Providers(ProviderName).Type)
                    // 
                    System.Configuration.ProviderSettings ourProvider;
                    if (moPrvConfig.Providers[ProviderName + "Local"] != null)
                    {
                        ourProvider = moPrvConfig.Providers[ProviderName + "Local"];
                    }
                    else
                    {
                        ourProvider = moPrvConfig.Providers[ProviderName];
                    }

                    Assembly assemblyInstance;

                    if (ourProvider != null)
                    {
                        if (Conversions.ToBoolean(Operators.ConditionalCompareObjectNotEqual(ourProvider.Parameters["path"], "", false)))
                        {
                            assemblyInstance = Assembly.LoadFrom(goServer.MapPath(Conversions.ToString(ourProvider.Parameters["path"])));
                        }
                        else
                        {
                            assemblyInstance = Assembly.Load(ourProvider.Type);
                        }
                    }
                    else
                    {
                        assemblyInstance = Assembly.Load(ProviderName);
                    }


                    var calledType = assemblyInstance.GetType(classPath, true);

                    var args = new object[1];
                    args[0] = myWeb;
                    var o = Activator.CreateInstance(calledType, args);

                    var args2 = new object[1];
                    args2[0] = SelectElmt;
                    if (aMethodName != null)
                    {
                        short i;
                        args2 = new object[aMethodName.Length];
                        args2[0] = SelectElmt;
                        var loopTo = (short)(aMethodName.Length - 1);
                        for (i = 1; i <= loopTo; i++)
                            args2[i] = aMethodName[i];

                    }

                    calledType.InvokeMember(methodName, BindingFlags.InvokeMethod, null, o, args2);

                    return SelectElmt;
                }

                catch (Exception ex)
                {
                    SelectElmt.InnerXml = "<item><label>GetSelectOptions Error - " + ex.Message + "</label><value>error</value></item>";

                    return SelectElmt;

                }

            }

            public object GetPageXml(string PageId, string xPath)
            {
                // Dim SelectDoc As New XmlDocument()
                XmlElement SelectElmt = myWeb.moPageXml.CreateElement("select1");
                XmlDocument oPageXml;
                try
                {
                    long existingPageId = myWeb.mnPageId;

                    var newWeb = new Cms(myWeb.moCtx);
                    newWeb.InitializeVariables();
                    newWeb.Open();
                    // newWeb.ibIndexMode = True
                    newWeb.mnPageId = Convert.ToInt32(PageId);
                    newWeb.mbIgnorePath = true;

                    oPageXml = newWeb.GetPageXML();
                    XmlNodeList oNodelist;

                    if (!string.IsNullOrEmpty(xPath))
                    {
                        oNodelist = oPageXml.SelectNodes(xPath);
                        var oReturnXml = new XmlDocument();
                        var oReturnElmt = oReturnXml.CreateElement("Page");
                        oReturnXml.AppendChild(oReturnElmt);
                        foreach (XmlNode oNode in oNodelist)
                            // oReturnElmt.AppendChild(oNode.CloneNode(True))
                            AddExistingNode(ref oReturnElmt, oNode);
                        return oReturnXml;
                    }
                    else
                    {
                        return oPageXml;
                    }
                    newWeb = default;
                    return oNodelist;

                }

                catch (Exception ex)
                {
                    SelectElmt.InnerXml = "<item><label>GetPageXml Error - " + ex.Message + "</label><value>error</value></item>";

                    return SelectElmt;

                }

            }

            public object GetContentDetailXml(string ArtId)
            {
                // Dim SelectDoc As New XmlDocument()
                XmlElement SelectElmt = myWeb.moPageXml.CreateElement("select1");
                var oReturnXml = new XmlDocument();
                var oReturnElmt = oReturnXml.CreateElement("Page");
                oReturnXml.AppendChild(oReturnElmt);
                try
                {

                    // myWeb.GetContentDetailXml(Nothing, ArtId, True, False)

                    Tools.Xml.AddExistingNode(ref oReturnElmt, myWeb.GetContentDetailXml(default, Convert.ToInt64(ArtId), true, false));

                    return oReturnXml;
                }


                catch (Exception ex)
                {
                    oReturnXml.DocumentElement.InnerXml = "<item><label>GetContentDetailXml Error - " + ex.Message + "</label><value>error</value></item>";
                    return oReturnXml;

                }

            }

            public object GetContentBriefXml(string ArtId)
            {
                // Dim SelectDoc As New XmlDocument()
                XmlElement SelectElmt = myWeb.moPageXml.CreateElement("select1");
                var oReturnXml = new XmlDocument();
                var oReturnElmt = oReturnXml.CreateElement("Page");
                oReturnXml.AppendChild(oReturnElmt);
                try
                {

                    // myWeb.GetContentDetailXml(Nothing, ArtId, True, False)

                    Tools.Xml.AddExistingNode(ref oReturnElmt, myWeb.GetContentBriefXml(default, Convert.ToInt64(ArtId)));

                    return oReturnXml;
                }


                catch (Exception ex)
                {
                    oReturnXml.DocumentElement.InnerXml = "<item><label>GetContentDetailXml Error - " + ex.Message + "</label><value>error</value></item>";
                    return oReturnXml;

                }

            }


            // Public Function SplitToNodeset(ByVal SourceNode As Object, ByVal delimiter As String) As Object

            // Dim oReturnXml As XmlDocument = New XmlDocument
            // Dim SourceSplit As String()
            // Dim i As Integer
            // Dim oContentPage As XmlElement
            // Try
            // oReturnXml.LoadXml(SourceNode)

            // SourceSplit = Split(oReturnXml.InnerXml, delimiter)

            // oReturnXml.InnerXml = ""
            // For i = 0 To SourceSplit.Length
            // oContentPage = oReturnXml.CreateElement("ContentPage")
            // oContentPage.SetAttribute("pageNo", i)
            // oContentPage.InnerXml = SourceSplit(i)
            // oReturnXml.AppendChild(oContentPage)
            // Next

            // Return oReturnXml

            // Catch ex As Exception
            // oReturnXml.DocumentElement.InnerXml = "<item><label>GetContentDetailXml Error - " & ex.Message & "</label><value>error</value></item>"
            // Return oReturnXml

            // End Try

            // End Function

            public object BundleJS(string CommaSeparatedFilenames, string TargetPath)
            {
                string sReturnString;
                try
                {
                    object AppVariableName = Strings.LCase("js" + TargetPath.Replace("~", ""));

                    bool bReset = false;
                    if (myWeb is null | gbDebug)
                    {
                        // likely to be in error condition
                        sReturnString = CommaSeparatedFilenames.Replace("~", "");
                    }
                    else
                    {
                        // Dim fsh As New Protean.fsHelper(myWeb.moCtx)
                        if (myWeb.moRequest["rebundle"] != null)
                        {
                            // 'code for deleting script.js file from the bundle folders.


                            if (myWeb.mbAdminMode)
                            {
                                bReset = true;
                            }
                        }
                        bool bAppVarExists = false;
                        if (myWeb.moCtx.Application.Get(AppVariableName.ToString()) != null)
                        {
                            bAppVarExists = true;
                        }

                        if (bAppVarExists == false)
                        {
                            // check if the file exists.
                            if (Conversions.ToBoolean(VirtualFileExists("/" + myWeb.moConfig["ProjectPath"] + "js" + TargetPath.Replace("~", "") + "/script.js")))
                            {
                                // regenerate the application variable from the files in the folder
                                // we do not want to recreate all js everytime the application pool is reset anymore.
                                myWeb.moCtx.Application.Set(AppVariableName.ToString(), "/" + myWeb.moConfig["ProjectPath"] + "js" + TargetPath.Replace("~", "") + "/script.js");
                                bAppVarExists = true;
                            }
                        }

                        if (myWeb.moCtx.Application.Get(AppVariableName.ToString()) != null & bReset == false)
                        {
                            sReturnString = Convert.ToString(myWeb.moCtx.Application.Get(AppVariableName.ToString()));
                        }
                        else
                        {
                            string appPath = myWeb.moRequest.ApplicationPath;
                            if (appPath.EndsWith("ewcommon") || appPath.EndsWith("ptn"))
                            {
                                CommaSeparatedFilenames = CommaSeparatedFilenames.Replace("~/", "~/../");
                            }
                          
                            CommaSeparatedFilenames = CommaSeparatedFilenames.TrimEnd(',');

                            string[] bundleFilePaths = Strings.Split(CommaSeparatedFilenames, ",");
                            // we build the file
                            var nullBuilder = new NullBuilder();
                            var scriptTransformer = new ScriptTransformer();
                            var nullOrderer = new NullOrderer();
                            var Bundles = new System.Web.Optimization.BundleCollection();
                            string url;
                            string fileNameToSave = "";
                            string cmd = string.Empty;
                            int cntFile = 0;


                            var loopTo = bundleFilePaths.Length - 1;
                            for (cntFile = 0; cntFile <= loopTo; cntFile++)
                            {
                                url = Convert.ToString(bundleFilePaths[cntFile]);
                                if (!string.IsNullOrEmpty(url) & url.Contains("https"))
                                {
                                    fileNameToSave = url.Substring(url.LastIndexOf("/") + 1);
                                    if (!fileNameToSave.Contains(".js"))
                                    {
                                        fileNameToSave = fileNameToSave + ".js";
                                    }
                                    if (!Directory.Exists(goServer.MapPath("~/" + myWeb.moConfig["ProjectPath"] + "js/external/")))
                                    {
                                        Directory.CreateDirectory(goServer.MapPath("~/" + myWeb.moConfig["ProjectPath"] + "js/external/"));
                                    }
                                    if (!File.Exists(goServer.MapPath("~/" + myWeb.moConfig["ProjectPath"] + "js/external/") + fileNameToSave))
                                    {

                                        using (var wc = new System.Net.WebClient())
                                        {
                                            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                                            wc.DownloadFile(url, goServer.MapPath("~/" + myWeb.moConfig["ProjectPath"] + "js/external/") + fileNameToSave);
                                        }
                                    }
                                    bundleFilePaths[cntFile] = "~/" + myWeb.moConfig["ProjectPath"] + ("js/external/" + fileNameToSave);
                                }
                            }

                            HttpContextWrapper CtxBase = new HttpContextWrapper(myWeb.moCtx);
                            BundleContext BundlesCtx = new System.Web.Optimization.BundleContext(CtxBase, Bundles, "~/" + myWeb.moConfig["ProjectPath"] + "js//");
                            BundlesCtx.EnableInstrumentation = false;

                            CustomScriptBundle jsBundle = new BundleTransformer.Core.Bundles.CustomScriptBundle(TargetPath);
                            jsBundle.Include(bundleFilePaths);
                            jsBundle.Builder = nullBuilder;
                            jsBundle.Transforms.Add(scriptTransformer);
                            jsBundle.Orderer = nullOrderer;

                            BundleTable.EnableOptimizations = true;

                            Bundles.Add(jsBundle);

                            // Dim instance As New CustomBundleResolver(Bundles, CtxBase)

                            string scriptFile;
                            scriptFile = TargetPath + "/script.js";

                            var fsh = new fsHelper(myWeb.moCtx);
                            fsh.initialiseVariables(fsHelper.LibraryType.Scripts);

                            BundleResponse br = Bundles.GetBundleFor(TargetPath).GenerateBundleResponse(BundlesCtx);
                            byte[] info = new System.Text.UTF8Encoding(true).GetBytes(br.Content);

                            string strFileName = "script.js";

                            if (info.Length < 10)
                            {
                                sReturnString = CommaSeparatedFilenames.Replace("~", "");

                                //string emptyerror = "This file is empty: " + bundleFilePaths.ToString();
                                //info = new System.Text.UTF8Encoding(true).GetBytes(emptyerror);
                                //strFileName = "empty.js";
                                //scriptFile = fsh.SaveFile(ref strFileName, TargetPath, info);//
                                //myWeb.moCtx.Application.Set(AppVariableName.ToString(), null);

                            }
                            else
                            {
                                scriptFile = fsh.SaveFile(ref strFileName, TargetPath, info);
                            }
                            if (scriptFile.StartsWith("ERROR: "))
                            {
                                myWeb.bPageCache = false;
                            }

                            if (scriptFile.StartsWith(TargetPath.TrimStart('~')))
                            {
                                // file has been saved successfully.
                                scriptFile = "/" + myWeb.moConfig["ProjectPath"] + "js" + scriptFile;
                                if (Conversions.ToBoolean(VirtualFileExists(scriptFile)))
                                {
                                    myWeb.moCtx.Application.Set(AppVariableName.ToString(), scriptFile);
                                }
                            }
                            else
                            {
                                // we have a file save error we should try again next request.
                                myWeb.bPageCache = false;
                            }

                            sReturnString = scriptFile;

                            info = null;
                            fsh = default;
                            BundlesCtx = null;
                            jsBundle = null;
                            Bundles = null;
                            nullBuilder = null;
                            scriptTransformer = null;
                            bundleFilePaths = null;

                        }
                    }
                    return sReturnString;
                    //sReturnString = null;
                }

                catch (IOException ex)    // New changes on 9/12/21'
                {
                    myWeb.bPageCache = false;
                    sReturnString = TargetPath + "/script.js?error=" + ex.Message;
                    // Return ioex.StackTrace
                    return sReturnString;
                }

                catch (Exception ex)
                {
                    myWeb.bPageCache = false;
                    return ex.Message;
                }

            }



            public object BundleCSS(string CommaSeparatedFilenames, string TargetPath)
            {
                if (Strings.Split(CommaSeparatedFilenames, ",").Count() > 1)
                {
                    throw new NotSupportedException("BundleCSS: this function does not currently support multiple less files");
                }

                string sReturnString = "";
                string sReturnError = "";
                bool bReset = false;
                string cProjectPath = string.Empty;
                if (myWeb != null) { 
                    if (myWeb.moConfig["ProjectPath"] != null)
                    {
                        cProjectPath = myWeb.moConfig["ProjectPath"];
                    }
                }
                string AppVariableName = Strings.LCase("css" + TargetPath.Replace("~", ""));
                do
                {
                    try
                    {
                        // Throw New System.Exception("An exception has occurred.")

                        if (myWeb is null)
                        {
                            // ONLY HAPPENS ON ERROR PAGES
                            gbDebug = true;
                        }
                        else if (myWeb.moRequest != null)
                        {
                            if (myWeb.moRequest["reBundle"] != null)
                            {
                                bReset = true;
                            }
                        }

                        if (gbDebug)
                        {
                            // in debug mode we simply return the files as a list, for XSLT to render.
                            sReturnString = CommaSeparatedFilenames.Replace("~", "");
                        }
                        else
                        {

                            bool bAppVarExists = false;
                            // New logic to stop rebuilding css when application is killed or restarted.
                            if (myWeb.moCtx.Application.Get(AppVariableName) != null)
                            {
                                bAppVarExists = true;
                            }

                            if (bAppVarExists == false)
                            {
                                // check if the file exists.
                                if (Conversions.ToBoolean(VirtualFileExists("/" + cProjectPath + "css" + TargetPath.Replace("~", "") + "/style.css")))
                                {
                                    // regenerate the application variable from the files in the folder
                                    // we do not want to recreate all css everytime the application pool is reset anymore.
                                    string sReturnStringNew = "";
                                    foreach (var myFile in Directory.GetFiles(goServer.MapPath("/" + cProjectPath + "css" + TargetPath.Replace("~", "")), "*.css"))
                                        sReturnStringNew = sReturnStringNew + "/" + cProjectPath + "css" + TargetPath.Replace("~", "") + "/" + Path.GetFileName(myFile) + ",";
                                    myWeb.moCtx.Application.Set(AppVariableName, sReturnStringNew.Trim(','));
                                    bAppVarExists = true;
                                }
                            }


                            if (bAppVarExists & bReset == false)
                            {
                                // check to see if the filename is saved in the application variable.

                                sReturnString = Convert.ToString(myWeb.moCtx.Application.Get(AppVariableName));

                                if (!sReturnString.StartsWith("/" + cProjectPath + "css" + TargetPath.TrimStart('~')))
                                {
                                    myWeb.bPageCache = false;
                                }
                            }

                            else
                            {
                                Tools.Security.Impersonate oImp = null;
                                if (Convert.ToBoolean(myWeb.impersonationMode))
                                {
                                    oImp = new Tools.Security.Impersonate();
                                    if (oImp.ImpersonateValidUser(myWeb.moConfig["AdminAcct"], myWeb.moConfig["AdminDomain"], myWeb.moConfig["AdminPassword"], true, myWeb.moConfig["AdminGroup"]) == false)
                                    {
                                        sReturnString = "Admin-Account-Logon-Failure";
                                        return sReturnString;
                                    }
                                }

                                string appPath = myWeb.moRequest.ApplicationPath;
                                if (appPath.EndsWith("ewcommon") || appPath.EndsWith("ptn"))
                                {
                                    CommaSeparatedFilenames = CommaSeparatedFilenames.Replace("~/", "~/../");
                                }

                                // set the services urls list and call the handler request
                                var oCssWebClient = new CssWebClient(myWeb.moCtx, ref myWeb.msException) { ServiceUrlsList = Strings.Split(CommaSeparatedFilenames, ",").ToList() };
                                oCssWebClient.SendCssHttpHandlerRequest();

                                string scriptFile = "";
                                var fsh = new fsHelper(myWeb.moCtx);

                                fsh.initialiseVariables(fsHelper.LibraryType.Style);

                                scriptFile = string.Format("{0}/style.css", TargetPath);
                                sReturnError = "error getting " + CommaSeparatedFilenames;
                                // sReturnError = "error getting " & oCssWebClient.FullCssFile

                                byte[] info = new System.Text.UTF8Encoding(true).GetBytes(oCssWebClient.FullCssFile);
                                // fsh.DeleteFile(goServer.MapPath("/" & myWeb.moConfig("ProjectPath") & "css" & scriptFile.TrimStart("~")))
                                // TS commented out as modified save file to overwrite by using WriteAllBytes
                                var cnt = default(int);
                                string maxAttempt = 5.ToString();
                                try
                                {
                                    var loopTo = Conversions.ToInteger(maxAttempt);
                                    for (cnt = 1; cnt <= loopTo; cnt++)
                                    {
                                        string strStylecss = "style.css";
                                        scriptFile = fsh.SaveFile(ref strStylecss, TargetPath, info);
                                        if (!scriptFile.Contains("ERROR:"))
                                        {
                                            break;
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    if (cnt < Conversions.ToDouble(maxAttempt))
                                    {
                                        System.Threading.Thread.Sleep(500 * cnt);
                                    }
                                }

                                if (scriptFile.Contains("ERROR:"))
                                {
                                    myWeb.bPageCache = false;
                                    scriptFile = "/" + cProjectPath + "css" + string.Format("{0}/style.css", TargetPath);
                                }  // we can try adding addional error handling here if we have exact code returned from issue

                                if (scriptFile.StartsWith(TargetPath.TrimStart('~'), StringComparison.InvariantCultureIgnoreCase))
                                {
                                    sReturnString = "/" + cProjectPath + "css" + scriptFile;
                                }
                                else
                                {
                                    myWeb.bPageCache = false;
                                    sReturnString = "/" + cProjectPath + "css" + scriptFile;
                                }


                                // hdlrClient will store the resultant cssSplits, store them to disk and application state, and return the file list to the xslt transformation
                                for (int i = 0, loopTo1 = oCssWebClient.CssSplits.Count - 1; i <= loopTo1; i++)
                                {
                                    scriptFile = string.Format("{0}/style{1}.css", TargetPath, i);

                                    info = new System.Text.UTF8Encoding(true).GetBytes(oCssWebClient.CssSplits[i]);
                                    // fsh.DeleteFile(goServer.MapPath("/" & myWeb.moConfig("ProjectPath") & "css" & TargetFile.TrimStart("~") & "/" & String.Format("style{0}.css", i)))
                                    // TS commented out as modified save file to overwrite by using WriteAllBytes
                                    string strStyle = string.Format("style{0}.css", i);
                                    scriptFile = "/" + cProjectPath + "css" + fsh.SaveFile(ref strStyle, TargetPath, info);

                                    if (scriptFile.StartsWith("/" + cProjectPath + "css" + "ERROR: "))
                                    {
                                        myWeb.bPageCache = false;

                                    }

                                    if (scriptFile.StartsWith("/" + cProjectPath + "css" + TargetPath.TrimStart('~')))
                                    {
                                        // file has been saved successfully.
                                        sReturnString += "," + scriptFile;
                                    }
                                    else
                                    {
                                        // we have a file save error we should try again next request.
                                        sReturnError += scriptFile;
                                    }


                                }

                                if (sReturnString.StartsWith("/" + cProjectPath + "css"))
                                {
                                    // check the file exists before we set the application variable...
                                    if (Conversions.ToBoolean(VirtualFileExists("/" + cProjectPath + "css" + TargetPath.Replace("~", "") + "/style.css")))
                                    {
                                        myWeb.moCtx.Application.Set(AppVariableName, sReturnString);
                                    }
                                }
                                else
                                {
                                    sReturnString = sReturnString + sReturnError;
                                }

                                oCssWebClient = default;
                                fsh = default;

                                if ((bool)myWeb.impersonationMode)
                                {
                                    if (!(oImp == null))
                                    {
                                        oImp.UndoImpersonation();
                                        oImp = null;
                                    }
                                }
                            }
                        }
                        //sReturnString = null;
                        return sReturnString.Replace("~", "");
                    }
                    catch (IOException ioex)    // New changes on 9/12/21'
                    {
                        myWeb.bPageCache = false;
                        sReturnString = "/" + cProjectPath + string.Format("{0}/style.css", TargetPath) + "?ioException=" + ioex.Message;
                        // Return ioex.StackTrace
                        return sReturnString;
                    }
                    catch (Exception ex)
                    {
                        // OnComponentError(myWeb, New Protean.Tools.Errors.ErrorEventArgs("xslt.BundleCSS", "LayoutActions", ex, CommaSeparatedFilenames))
                        // My.Application.Log.WriteException(ex)

                        AddExceptionToEventLog(ex, sReturnString);

                        // regardless we should return the filename.
                        sReturnString = "/" + cProjectPath + "css" + string.Format("{0}/style.css", TargetPath);

                        myWeb.bPageCache = false; // This is not working 100% - can we understand why?????B

                        return sReturnString.Replace("~", "") + "?error=" + sReturnError + ex.Message + ex.StackTrace.Replace(",", "");
                    }
                }

                while (false);

            }

            #endregion

        }


    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;
using System.Xml;

namespace Protean.Tools { 
public static class Text
{
    public enum TextOptions
    {
        LowerCase = 1,
        UpperCase = 2,
        IgnoreCase = 4,
        UseAlpha = 8,
        UseNumeric = 16,
        UseSymbols = 32,
        UnambiguousCharacters = 64
    }

        public static long ToUnixTime(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }


        public static string HtmlHeaderDateTime(this DateTime date)
        {
            return date.ToString("ddd") + ", " + date.ToString("dd MM yyyy HH:mm:ss") + " GMT";
        }


        public static string MaskString(string cInitialString, string cMaskchar = "*", bool bKeepSpaces = false, int nNoCharsToLeave = 4)
    {
        string cNewString = "";
        try
        {
            if (!bKeepSpaces)
                cInitialString = cInitialString.Replace(" ", "");
            int i;
            var loopTo = (cInitialString.Length - (nNoCharsToLeave + 1));
            for (i = 0; i <= loopTo; i++)
            {
                if (!(cInitialString.Substring(i, 1) == " "))
                    cNewString += cMaskchar;
                else
                    cNewString += " ";
            }
            cNewString += Strings.Right(cInitialString, nNoCharsToLeave);
            return cNewString;
        }
        catch (Exception ex)
        {
            return cNewString;
        }
    }

    public static bool IsEmail(string cEmail)
    {
        // checks the validity of the email address by assuming it is false and running a series of tests.
        // if the email string passes all tests then this function returns True
        // checks are:-
        // is empty?
        // no spaces inside?
        // @ separator in place
        // last . is less than 4 chars from end

        // OR... Do it in one very efficient line (more efficient than nested text searches)


        // Validate the e-mail address
        return new Regex(@"^[A-Z0-9.'_%-]+@[A-Z0-9-]+(\.[A-Z0-9-]+)*\.[A-Z]{2,24}$", RegexOptions.IgnoreCase).IsMatch(cEmail + "");
    }

    public static string IntegerToString(int nNumber, int nMinLength)
    {
        string cReturn = nNumber.ToString();
        while (cReturn.Length < nMinLength)
            cReturn = "0" + cReturn;
        return cReturn;
    }

    public static string filenameFromPath(string path)
    {
        string filename;
        filename = Strings.Mid(path, Strings.InStrRev(path, @"\") + 1);
        filename = Strings.Mid(filename, Strings.InStrRev(filename, "/") + 1);
        filename = Strings.Replace(filename, " ", "-");
        return filename;
    }

    public static double EmptyNumber(object Value)
    {
        try
        {
            if (Value == null)
                return 0;
            if (Information.IsDBNull(Value))
                return 0;
            if (System.Convert.ToString(Value) == "")
                return 0;
            if (!Information.IsNumeric(Value))
                return 0;
            return System.Convert.ToDouble(Value);
        }
        catch (Exception ex)
        {
            return 0;
        }
    }

    public static DateTime EmptyDate(object Value, DateTime DefaultDate = default(DateTime))
    {
        try
        {
            if (Value == null)
                return DefaultDate;
            if (Information.IsDBNull(Value))
                return DefaultDate;
            if (System.Convert.ToString(Value) == "")
                return DefaultDate;
            if (!Information.IsDate(Value))
                return DefaultDate;
            return (DateTime)Value;
        }
        catch (Exception ex)
        {
            return default(DateTime);
        }
    }

    public static string AscString(string OriginalString, string Delimiter = ",")
    {
        try
        {
            int i = 0;
            string cReturn = "";
            var loopTo = OriginalString.Length - 1;
            for (i = 0; i <= loopTo; i++)
            {
                if (!(cReturn == ""))
                    cReturn += Delimiter;
                cReturn += Strings.Asc(OriginalString.Substring(i, 1));
            }
            return cReturn;
        }
        catch (Exception ex)
        {
            return OriginalString;
        }
    }

    public static string DeAscString(string AscString, string Delimiter = ",")
    {
        try
        {
            string[] oAsc = Strings.Split(AscString, Delimiter);
            int i = 0;
            string cReturn = "";
            var loopTo = oAsc.Length - 1;
            for (i = 0; i <= loopTo; i++)
                cReturn += Strings.Chr(int.Parse(oAsc[i]));
            return cReturn;
        }
        catch (Exception ex)
        {
            return AscString;
        }
    }

    public static string[] CodeGen(string PrecedingText, int StartNumber, int NumberOfCodes, bool bKeepProceedingZeros, bool MD5Results)
    {
        try
        {
            int nMinLength = System.Convert.ToString(StartNumber + NumberOfCodes).Length;
            if (System.Convert.ToString(StartNumber).Length > nMinLength)
                nMinLength = System.Convert.ToString(StartNumber).Length;
            string cResult = "";
            var loopTo = NumberOfCodes - 1;
            for (int i = 0; i <= loopTo; i++)
            {
                int newStart = StartNumber + i;
                string cPart = newStart.ToString();
                if (bKeepProceedingZeros)
                    cPart = IntegerToString(int.Parse(cPart), nMinLength);
                cPart = PrecedingText + cPart;
                if (MD5Results)
                {
                    System.Text.UnicodeEncoding encode = new System.Text.UnicodeEncoding();
                    byte[] inputDigest = encode.GetBytes(cPart);
                    byte[] hash;
                    // get hash
                    System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                    hash = md5.ComputeHash(inputDigest);
                    // convert hash value to hex string
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
           
                    foreach (byte outputByte in hash)
                        // convert each byte to a Hexadecimal upper case string
                        sb.Append(outputByte.ToString("x2"));
                    cPart = sb.ToString();
                }
                if (!(cResult == ""))
                    cResult += ",";
                cResult += cPart;
            }
            return Strings.Split(cResult, ",");
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    /// <summary>
    /// Checks an IP address against a comma delimited list of distinct IP addresses (not ranges)
    /// </summary>
    /// <param name="cIPAddress">The IP address to look for (e.g. 1.2.3.4)</param>
    /// <param name="cIPAddressList">A comma separated list of IP addresses (e.g. 1.2.3.4,2.3.4.5)</param>
    /// <returns>Boolean - True if found, False if not or error encountered.</returns>
    /// <remarks>Doesn't attempt to validate the IP address at the moment, so is a glorified list finder at the mo.</remarks>
    public static bool IsIPAddressInList(string cIPAddress, string cIPAddressList)
    {
        try
        {
            // Find if an IP address exists in a comma/pipe-delimited IP address range.
            bool bFound = false;

            if (cIPAddress != "" & !(Information.IsNothing(cIPAddressList)))
            {
                System.Text.RegularExpressions.Regex oRE = new System.Text.RegularExpressions.Regex("(,|^)" + Strings.Replace(cIPAddress, ".", @"\.") + "(,|$)");
                if (oRE.IsMatch(cIPAddressList))
                    bFound = true;
            }

            return bFound;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    /// <summary>
    /// Generates a Random Password
    /// </summary>
    /// <param name="size">The size of the password required</param>
    /// <param name="charSet">Optional - specify the characters you want to use</param>
    /// <param name="options">Optional - options available are UseAlpha, UseNumeric, UseSymbols, UnambiguousCharacters, LowerCase and UpperCase.  Default value is UseAlph, UseNumeric and Lowercase</param>
    /// <param name="oRandomObject">Optional - a precreated Random object (useful if repeatedly running through passwords) </param>
    /// <returns>String - The randomly generated password.</returns>
    /// <remarks></remarks>
    public static string RandomPassword(int size, string charSet = "", Text.TextOptions options = TextOptions.LowerCase | TextOptions.UseAlpha | TextOptions.UseNumeric, Protean.Tools.Number.Random oRandomObject = null/* TODO Change to default(_) if this is not a reference type */)
    {
        StringBuilder s = new StringBuilder();
        Tools.Number.Random r;
        string output = "";

        bool bUnambiguous;
        char[] charSetArray;

        try
        {
            if (oRandomObject == null)
                r = new Tools.Number.Random();
            else
                r = oRandomObject;

            bUnambiguous = (options & TextOptions.UnambiguousCharacters) != 0;

            if (string.IsNullOrEmpty(charSet))
            {
                // Build the Character Set

                // Check for empty character set options - add a default if none are selected.
                if (!((options & TextOptions.UseAlpha) != 0 | (options & TextOptions.UseNumeric) != 0 | (options & TextOptions.UseSymbols) != 0))

                    options = options | TextOptions.UseAlpha | TextOptions.UseNumeric;

                if ((options & TextOptions.UseAlpha) != 0)
                    s.Append("ABCDEFGHJKMNPQRTVWXY").Append(bUnambiguous? "": "ILOSUZ");
                if ((options & TextOptions.UseNumeric) != 0)
                    s.Append("346789").Append(bUnambiguous? "": "1250");
                if ((options & TextOptions.UseSymbols) != 0)
                    s.Append(")$%*?#~").Append(bUnambiguous?"": "!(){}@");

                charSet = s.ToString();
                s = new StringBuilder();
            }

            charSetArray = charSet.ToCharArray();
            var loopTo = size - 1;
            for (int i = 0; i <= loopTo; i++)
            {
                Int32 n = Convert.ToInt32(Math.Floor(charSetArray.Length * r.NextDouble()));
                s.Append(charSetArray[n]);
            }

            output = s.ToString();

            if ((options & TextOptions.UpperCase) != 0)
                output = output.ToUpper();
            if ((options & TextOptions.LowerCase) != 0)
                output = output.ToLower();

            return output;
        }
        catch (Exception ex)
        {
            return "";
        }
    }

    /// <summary>
    /// Replaces the last instance of a specific character in a string with a replacement string.
    /// </summary>
    /// <param name="searchString">The string to search</param>
    /// <param name="charToFind">The character to find</param>
    /// <param name="replacement">The replacement string</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static string ReplaceLastCharacter(ref string searchString, char charToFind, string replacement)
    {
        return Regex.Replace(searchString, "(?=" + charToFind.ToString() + "[^" + charToFind.ToString() + "]*$)" + charToFind.ToString() + "", replacement);
    }

    /// <summary>
    /// Removes the last instance of a specific character in a string with a replacement string.
    /// </summary>
    /// <param name="searchString">The string to search</param>
    /// <param name="charToFind">The character to find</param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static string RemoveLastCharacter(ref string searchString, char charToFind)
    {
        return ReplaceLastCharacter(ref searchString, charToFind, "");
    }

    /// <summary>
    /// Truncates strings up to the last word before a length limit.
    /// </summary>
    /// <param name="input">The string to truncate</param>
    /// <param name="limit">The maximum length of the string</param>
    /// <returns></returns>
    /// <remarks>If a string is truncated ellipses (...) will be added.  In this case the length limit will be lowered by the 3 characters for ellipses to accomodate them</remarks>
    public static string TruncateString(string input, long limit)
    {
        try
        {
            string truncate = input;

            if (input.Length > limit)
            {

                // Truncate the string minus the ellipses (...) 
                long newLimit = limit - 3;
                truncate = input.Substring(0, (int)newLimit);

                // Set a default return value
                long index = limit;

                // Now find the last letter before a word boundary
                Regex oRe = new Regex(@"\w\b");
                MatchCollection oMatches = oRe.Matches(truncate);
                if (oMatches.Count > 2)
                    // We have matches (more than the beginning and the end of the string at least), get the index of the last but one 
                    index = oMatches[oMatches.Count - 2].Index + 1;


                //truncate = truncate.Substring(0, (int)index) + "...";
                    truncate = truncate + "...";
                }

            return truncate;
        }
        catch (Exception ex)
        {
            return input;
        }
    }

    /// <summary>
    /// String Based Coalesce - i.e. returns the first non Nothing, NUll And Empty string
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    /// <remarks></remarks>
    public static string Coalesce(params object[] args)
    {
        try
        {
            foreach (object arg in args)
            {
                if (arg != null && !string.IsNullOrEmpty((string)arg))
                    return arg.ToString();
            }
            return "";
        }
        catch (Exception ex)
        {
            return "";
        }
    }



    public static string SimpleRegexFind(string cSearchString, string cRegexPattern, int nReturnGroup = 0, RegexOptions oRegexOptions = RegexOptions.None)
    {

        // Given a string and a regex pattern, this will try to find the pattern in the string and return what is matched
        // It is possible to return a submatch (i.e. specified in parentheses in the regex pattern) by settign nReturnGroup 
        // to the required parenthes grouping number.

        // If nothing is found then "" is returned.

        try
        {
            Regex oRe = new Regex(cRegexPattern, oRegexOptions);
            Match oFind = oRe.Match(cSearchString);
            if (oFind.Groups.Count >= nReturnGroup)
                return oFind.Groups[nReturnGroup].Value;
            else
                return "";
        }
        catch
        {
            return "";
        }
    }

    public static string UTF8ByteArrayToString(byte[] characters)
    {
        UTF8Encoding encoding = new UTF8Encoding(false);
        return encoding.GetString(characters);
    }

    public static byte[] StringToUTF8ByteArray(string characters)
    {
        UTF8Encoding encoding = new UTF8Encoding(false);
        return encoding.GetBytes(characters);
    }

    public static object RegularExpressions()
    {
        throw new NotImplementedException();
    }

    public static string CleanName(string cName, bool bLeaveAmp = false, bool bURLSafe = false)
    {
        // Valid Chars
        string cValids;
        cValids = "0123456789";
        cValids += "abcdefghijklmnopqrstuvwxyz";
        cValids += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        cValids += " -&";
        try
        {
           if (cName != null) { 

                cName = Strings.Replace(cName, "'", "");
                cName = Strings.Replace(cName, "&amp;", "&");
                if (!bLeaveAmp)
                    cName = Strings.Replace(cName, "&", "and");
                int i;
                string cBuilt = "";
                    var loopTo = 0;
                        if (cName!= null) {                         
                        loopTo = loopTo + cName.Length;
                    }
                for (i = 0; i <= loopTo; i++)
                {
                    string cTest = Strings.Right(Strings.Left(cName, i), 1);
                    if (cValids.Contains(cTest))
                        cBuilt += cTest;
                }
                cName = cBuilt;
                // replace double spaces a few times
                cName = "" + Strings.Replace(cName, "  ", " ");
                cName = "" + Strings.Replace(cName, "  ", " ");
                cName = "" + Strings.Replace(cName, "  ", " ");

                if (bURLSafe) { 
                    cName = "" + cName.Replace(" ", "-");
                    // replace double hyphens a few times
                    cName = "" + Strings.Replace(cName, "---", "-");
                    cName = "" + Strings.Replace(cName, "--", "-");
                        //trim to max filename length with .html

                        if (cName.Length > 249) {
                            cName = cName.Substring(0, 249);
                    }
                }

            }
            return cName;

        }
        catch (Exception ex)
        {
            return cName;
        }
    }


    public static string tidyXhtmlFrag(string shtml, bool bReturnNumbericEntities = false, bool bEncloseText = true, string removeTags = "")
    {

        // PerfMon.Log("Web", "tidyXhtmlFrag")
        string sProcessInfo = "tidyXhtmlFrag";
        string sTidyXhtml = "";
        int crResult = 0;

        if (!(removeTags == ""))
            shtml = removeTagFromXml(shtml, removeTags);
        TidyManaged.Document oTdyManaged;
        // Using 
        try
        {
                // clear some nasties I haven't allready captured.
            shtml = shtml.Replace("&amp;nbsp;", "&#160;");
            shtml = Regex.Replace(shtml, "<\\?xml.*\\?>", "", RegexOptions.IgnoreCase);

                oTdyManaged = TidyManaged.Document.FromString(shtml);
            oTdyManaged.OutputBodyOnly = TidyManaged.AutoBool.Yes;
            oTdyManaged.MakeClean = true;
            oTdyManaged.DropFontTags = true;
            //oTdyManaged.ErrorBuffer = true;
            oTdyManaged.ShowWarnings = true;
            oTdyManaged.OutputXhtml = true;
            oTdyManaged.MakeBare = true;//removed word tags
            oTdyManaged.CleanWord2000 = true;//removed word tags

                if (bReturnNumbericEntities)
                {
                oTdyManaged.OutputNumericEntities = true;
                }
           int tidyResult = oTdyManaged.CleanAndRepair();
           try
            {
                sTidyXhtml = oTdyManaged.Save();
            }
            catch (Exception ex)
            {
                sTidyXhtml = "<div>html import conversion error result=" + tidyResult + " <br/></div>";
            }
           
            oTdyManaged.Dispose();
           oTdyManaged = null/* TODO Change to default(_) if this is not a reference type */;
            // End Using

            return sTidyXhtml;
        }
        catch (Exception ex)
        {
                // It is the desired behaviour for this to return nothing if not valid html don't turn this on apart from in development.            Return Nothing
                return crResult + " - " + ex.Message + ex.StackTrace;
        }
        // Return Nothing
        finally
        {
            sTidyXhtml = null;
        }
    }

    public static string removeTagFromXml(string xmlString, string tagNames)
    {
        tagNames = Strings.Replace(tagNames, " ", "");
        tagNames = Strings.Replace(tagNames, ",", "|");

        xmlString = Regex.Replace(xmlString, "<[/]?(" + tagNames + @":\w+)[^>]*?>", "", RegexOptions.IgnoreCase);

        return xmlString;
    }
}
}
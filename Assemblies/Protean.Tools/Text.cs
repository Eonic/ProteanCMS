using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tidy.Core;


namespace Protean.Tools
{
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
                cNewString += cInitialString.Substring(cInitialString.Length - nNoCharsToLeave);

                return cNewString;
            }
            catch (Exception)
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
            int lastBackslashIndex = path.LastIndexOf('\\');
            int lastForwardSlashIndex = path.LastIndexOf('/');

            int startIndex = Math.Max(lastBackslashIndex, lastForwardSlashIndex);

            string filename = (startIndex >= 0) ? path.Substring(startIndex + 1) : path;

            filename = filename.Replace(" ", "-");

            return filename;
        }

        public static double EmptyNumber(object value)
        {
            try
            {
                // Check if the value is null or empty
                if (value == null || System.Convert.ToString(value).Trim() == "")
                {
                    return 0;
                }

                // Try to convert the value to double
                if (double.TryParse(System.Convert.ToString(value), out double result))
                {
                    return result;
                }

                return 0; // Return 0 if conversion fails
            }
            catch (Exception)
            {
                return 0; // Return 0 in case of an exception
            }
        }

        public static DateTime EmptyDate(object value, DateTime defaultDate = default)
        {
            try
            {
                // Check if the value is null, DBNull, or empty
                if (value == null ||
                    (value is DBNull) ||
                    string.IsNullOrWhiteSpace(System.Convert.ToString(value)))
                {
                    return defaultDate;
                }

                // Try to convert the value to DateTime
                if (DateTime.TryParse(System.Convert.ToString(value), out DateTime result))
                {
                    return result;
                }

                return defaultDate; // Return default date if conversion fails
            }
            catch (Exception)
            {
                return default; // Return the default DateTime in case of an exception
            }
        }


        public static string AscString(string originalString, string delimiter = ",")
        {
            try
            {
                // Initialize a StringBuilder for efficient string concatenation
                var cReturn = new System.Text.StringBuilder();

                // Loop through each character in the original string
                for (int i = 0; i < originalString.Length; i++)
                {
                    // Append the delimiter if it's not the first element
                    if (i > 0)
                    {
                        cReturn.Append(delimiter);
                    }

                    // Append the ASCII value of the current character
                    cReturn.Append((int)originalString[i]);
                }

                return cReturn.ToString(); // Convert StringBuilder to string
            }
            catch (Exception)
            {
                return originalString; // Return the original string in case of an exception
            }
        }


        public static string DeAscString(string ascString, string delimiter = ",")
        {
            try
            {
                // Split the input string by the specified delimiter
                string[] oAsc = ascString.Split(new[] { delimiter }, StringSplitOptions.None);

                // Initialize a StringBuilder for efficient string concatenation
                var cReturn = new System.Text.StringBuilder();

                // Loop through the array of ASCII values
                foreach (string asciiValue in oAsc)
                {
                    // Parse the ASCII value and convert to character, then append to result
                    cReturn.Append(Convert.ToChar(int.Parse(asciiValue)));
                }

                return cReturn.ToString(); // Convert StringBuilder to string
            }
            catch (Exception)
            {
                return ascString; // Return the original string in case of an exception
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
                return cResult.Split(new[] { ',' }, StringSplitOptions.None);
            }
            catch (Exception)
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
                // Initialize found flag
                bool bFound = false;

                // Check if the IP address is not empty and the list is not null
                if (!string.IsNullOrEmpty(cIPAddress) && cIPAddressList != null)
                {
                    // Create a regex pattern to match the IP address
                    Regex oRE = new Regex(@"(,|^)" + Regex.Escape(cIPAddress) + "(,|$)");

                    // Check if the IP address matches the list
                    if (oRE.IsMatch(cIPAddressList))
                        bFound = true;
                }

                return bFound;
            }
            catch (Exception)
            {
                return false; // Return false in case of any exceptions
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
                        s.Append("ABCDEFGHJKMNPQRTVWXY").Append(bUnambiguous ? "" : "ILOSUZ");
                    if ((options & TextOptions.UseNumeric) != 0)
                        s.Append("346789").Append(bUnambiguous ? "" : "1250");
                    if ((options & TextOptions.UseSymbols) != 0)
                        s.Append(")$%*?#~").Append(bUnambiguous ? "" : "!(){}@");

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
            catch (Exception)
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
            catch (Exception)
            {
                return input;
            }
        }

        public static string TruncateString(string input, long limit, string endWith)
        {
            try
            {
                string truncate = input;

                if (input.Length > limit)
                {

                    // Truncate the string minus the ellipses (...) 
                    long newLimit = limit - endWith.Length;
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
                    truncate = truncate + endWith;
                }

                return truncate;
            }
            catch (Exception)
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
            catch (Exception)
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

        public static string EscapeJS(string text)
        {
            try
            {
                string orig = text;
                if (!(text == ""))
                {

                    text = text.Replace(@"\", @"\\");
                    text = text.Replace("&#13;", @"\r");
                    text = text.Replace("&#10;", @"\n");
                    text = text.Replace("#9;", @"\t");
                    text = text.Replace("\"", @"\""");
                    text = text.Replace("'", @"\'");
                    text = text.Replace("’", @"\'");


                }

                if ((orig != text))
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

        public static string CleanName(string cName, bool bLeaveAmp = false, bool bURLSafe = false)
        {
            // Valid Chars
            string cValids;
            cValids = "0123456789";
            cValids += "abcdefghijklmnopqrstuvwxyz";
            cValids += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            cValids += " -&*,";
            try
            {
                if (cName != null)
                {
                    cName = cName.Replace("/", "*");//TS need to remove any slashes from names replace with * so we can wildcard later.
                    cName = cName.Replace("'", "*");//TS need to remove any slashes from names replace with * so we can wildcard later.
                    cName = cName.Replace("(", "*");
                    cName = cName.Replace(")", "*");
                    cName = cName.Replace(".", "*");
                    cName = cName.Replace("'", "");
                    cName = cName.Replace("&amp;", "&");
                    if (!bLeaveAmp)
                        cName = cName.Replace("&", "and");
                    int i;
                    string cBuilt = "";
                    var loopTo = 0;
                    if (cName != null)
                    {
                        loopTo = loopTo + cName.Length;
                    }
                    for (i = 0; i <= loopTo - 1; i++)
                    {
                        string cTest = cName.Substring(0, i + 1).Last().ToString();
                        if (cValids.Contains(cTest))
                            cBuilt += cTest;
                    }
                    cName = cBuilt;
                    // replace double spaces a few times
                    cName = "" + cName.Replace("  ", " ");
                    cName = "" + cName.Replace("  ", " ");
                    cName = "" + cName.Replace("  ", " ");

                    if (bURLSafe)
                    {
                        cName = "" + cName.Replace(" ", "-");
                        // replace double hyphens a few times
                        cName = "" + cName.Replace("---", "-");
                        cName = "" + cName.Replace("--", "-");
                        //trim to max filename length with .html

                        if (cName.Length > 249)
                        {
                            cName = cName.Substring(0, 249);
                        }
                    }

                }
                return cName;

            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return cName;
            }
        }


        public static string tidyXhtmlFrag(string shtml, bool bReturnNumbericEntities = false, bool bEncloseText = true, string removeTags = "")
        {

            // PerfMon.Log("Web", "tidyXhtmlFrag")
            // string sProcessInfo = "tidyXhtmlFrag";
            string sTidyXhtml = "";
            int crResult = 0;

            if (!(removeTags == ""))
                shtml = removeTagFromXml(shtml, removeTags);
            Tidy.Core.HtmlTidy oTdy = new Tidy.Core.HtmlTidy();
            Tidy.Core.TidyOptions oTdyOptions = new Tidy.Core.TidyOptions();

            // Using 
            try
            {
                // clear some nasties I haven't allready captured.
                shtml = shtml.Replace("&amp;nbsp;", "&#160;");
                shtml = Regex.Replace(shtml, "<\\?xml.*\\?>", "", RegexOptions.IgnoreCase);

                //oTdy.Options.XmlOut = true;

                oTdy.Options.Word2000 = true;
                oTdy.Options.XmlOut = true;

                oTdy.Options.MakeClean = true;
                oTdy.Options.MakeBare = true;
                oTdy.Options.Xhtml = true;
                oTdy.Options.DropFontTags = true;
                oTdy.Options.BodyOnly = true;

                if (bReturnNumbericEntities)
                {
                    oTdy.Options.NumEntities = true;
                }

                Tidy.Core.TidyMessageCollection tidyMsg = new Tidy.Core.TidyMessageCollection();



                //Stream stout = new MemoryStream();

                //oTdy.Parse(shtml, stout, tidyMsg);

                //StreamReader sr = new StreamReader(stout);
                //sTidyXhtml = sr.ReadToEnd();

                sTidyXhtml = oTdy.Parse(shtml, tidyMsg);


                //sTidyXhtml = oTdy.ToString();

                foreach (TidyMessage tm in tidyMsg)
                {
                    if (tm.Level == MessageLevel.Error)
                    {
                        sTidyXhtml = "<div>html import conversion error result=" + tm.Message + " <br/></div>";
                    }
                }


                //oTdy = TidyManaged.Document.FromString(shtml);
                //oTdy.OutputBodyOnly = TidyManaged.AutoBool.Yes;
                //oTdy.MakeClean = true;
                //oTdy.DropFontTags = true;
                ////oTdy.ErrorBuffer = true;
                //oTdy.ShowWarnings = true;
                //oTdy.OutputXhtml = true;
                //oTdy.MakeBare = true;//removed word tags
                //oTdy.CleanWord2000 = true;//removed word tags

                //if (bReturnNumbericEntities)
                //{
                //    oTdy.OutputNumericEntities = true;
                //}
                //oTdy.CleanAndRepair();
                //try
                //{
                //    oTdy.Save();
                //}
                //catch (Exception)
                //{
                //    sTidyXhtml = "<div>html import conversion error result=" + " <br/></div>";
                //}

                //oTdy.Dispose();
                oTdy = null;
                oTdyOptions = null;/* TODO Change to default(_) if this is not a reference type */
                // End Using

                return sTidyXhtml.Replace("<body>", "").Replace("</body>", "");
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
            tagNames = tagNames.Replace(" ", "");
            tagNames = tagNames.Replace(",", "|");

            xmlString = Regex.Replace(xmlString, "<[/]?(" + tagNames + @":\w+)[^>]*?>", "", RegexOptions.IgnoreCase);

            return xmlString;
        }
    }
}
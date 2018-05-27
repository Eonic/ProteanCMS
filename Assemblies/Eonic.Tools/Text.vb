Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml

Public Module Text
    Public Enum TextOptions
        LowerCase = 1
        UpperCase = 2
        IgnoreCase = 4
        UseAlpha = 8
        UseNumeric = 16
        UseSymbols = 32
        UnambiguousCharacters = 64
    End Enum


    Public Function MaskString(ByVal cInitialString As String, Optional ByVal cMaskchar As String = "*", Optional ByVal bKeepSpaces As Boolean = False, Optional ByVal nNoCharsToLeave As Integer = 4) As String
        Dim cNewString As String = ""
        Try
            If Not bKeepSpaces Then cInitialString = cInitialString.Replace(" ", "")
            Dim i As Integer
            For i = 0 To (cInitialString.Length - (nNoCharsToLeave + 1))
                If Not cInitialString.Substring(i, 1) = " " Then
                    cNewString &= cMaskchar
                Else
                    cNewString &= " "
                End If
            Next
            cNewString &= Right(cInitialString, nNoCharsToLeave)
            Return cNewString
        Catch ex As Exception
            Return cNewString
        End Try
    End Function

    Public Function IsEmail(ByVal cEmail As String) As Boolean
        '   checks the validity of the email address by assuming it is false and running a series of tests.
        '   if the email string passes all tests then this function returns True
        '       checks are:-
        '       is empty?
        '       no spaces inside?
        '       @ separator in place
        '       last . is less than 4 chars from end

        ' OR... Do it in one very efficient line (more efficient than nested text searches)


        ' Validate the e-mail address
        Return New Regex("^[A-Z0-9.'_%-]+@[A-Z0-9-]+(\.[A-Z0-9-]+)*\.[A-Z]{2,24}$", RegexOptions.IgnoreCase).IsMatch(cEmail & "")
        'TS extended to cater for really long TLD's

        'Dim res As Short
        'Dim test As String
        'Dim bResult As Boolean = False '   false until proven true
        'Dim sProcessInfo As String = ""
        ''If mDebugMode <> "Debug" Then On Error GoTo ErrorHandler

        'test = Trim(cEmail) '   get rid of surrounding spaces (can't hurt to do this!)


        'If (Not test = "") And InStr(1, test, " ", CompareMethod.Text) = 0 Then
        '    '   Isn't empty and doesn't contain whitespace
        '    res = InStr(1, test, "@", CompareMethod.Text)
        '    If res > 1 Then
        '        '   there is an at sign and it isn't the first letter
        '        res = Len(test) - InStrRev(test, ".", -1, CompareMethod.Binary)
        '        If res < 4 And res > 1 Then
        '            '   last occurrence of a period is in the 2nd or 3rd position from the end
        '            bResult = True
        '            '   the address is valid
        '        End If
        '    End If
        'End If
        'Return bResult
    End Function


   

    Public Function IntegerToString(ByVal nNumber As Integer, ByVal nMinLength As Integer) As String
        Dim cReturn As String = nNumber
        Do Until cReturn.Length >= nMinLength
            cReturn = "0" & cReturn
        Loop
        Return cReturn
    End Function

    Public Function filenameFromPath(ByVal path As String)

        Dim filename As String
        filename = Mid(path, InStrRev(path, "\") + 1)
        filename = Mid(filename, InStrRev(filename, "/") + 1)
        filename = Replace(filename, " ", "-")
        Return filename

    End Function

    Public Function EmptyNumber(ByVal Value As Object) As Double
        Try
            If Value Is Nothing Then Return 0
            If IsDBNull(Value) Then Return 0
            If CStr(Value) = "" Then Return 0
            If Not IsNumeric(Value) Then Return 0
            Return CType(Value, Double)
        Catch ex As Exception
            Return 0
        End Try
    End Function

    Public Function EmptyDate(ByVal Value As Object, Optional ByVal DefaultDate As Date = Nothing) As Date
        Try
            If Value Is Nothing Then Return DefaultDate
            If IsDBNull(Value) Then Return DefaultDate
            If CStr(Value) = "" Then Return DefaultDate
            If Not IsDate(Value) Then Return DefaultDate
            Return CDate(Value)
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Public Function AscString(ByVal OriginalString As String, Optional ByVal Delimiter As String = ",") As String
        Try
            Dim i As Integer = 0
            Dim cReturn As String = ""
            For i = 0 To OriginalString.Length - 1
                If Not cReturn = "" Then cReturn &= Delimiter
                cReturn &= Asc(OriginalString.Substring(i, 1))
            Next
            Return cReturn
        Catch ex As Exception
            Return OriginalString
        End Try
    End Function

    Public Function DeAscString(ByVal AscString As String, Optional ByVal Delimiter As String = ",") As String
        Try
            Dim oAsc() As String = Split(AscString, Delimiter)
            Dim i As Integer = 0
            Dim cReturn As String = ""
            For i = 0 To oAsc.Length - 1
                cReturn &= Chr(oAsc(i))
            Next
            Return cReturn
        Catch ex As Exception
            Return AscString
        End Try
    End Function

    Public Function CodeGen(ByVal PrecedingText As String, ByVal StartNumber As Integer, ByVal NumberOfCodes As Integer, ByVal bKeepProceedingZeros As Boolean, ByVal MD5Results As Boolean) As String()
        Try
            Dim nMinLength As Integer = CStr(StartNumber + NumberOfCodes).Length
            If CStr(StartNumber).Length > nMinLength Then nMinLength = CStr(StartNumber).Length
            Dim cResult As String = ""

            For i As Integer = 0 To NumberOfCodes - 1
                Dim cPart As String = StartNumber + i
                If bKeepProceedingZeros Then
                    cPart = IntegerToString(cPart, nMinLength)
                End If
                cPart = PrecedingText & cPart
                If MD5Results Then
                    Dim encode As New System.Text.UnicodeEncoding
                    Dim inputDigest() As Byte = encode.GetBytes(cPart)
                    Dim hash() As Byte
                    ' get hash
                    Dim md5 As New System.Security.Cryptography.MD5CryptoServiceProvider
                    hash = md5.ComputeHash(inputDigest)
                    ' convert hash value to hex string
                    Dim sb As New System.Text.StringBuilder
                    Dim outputByte As Byte
                    For Each outputByte In hash
                        ' convert each byte to a Hexadecimal upper case string
                        sb.Append(outputByte.ToString("x2"))
                    Next outputByte
                    cPart = sb.ToString
                End If
                If Not cResult = "" Then cResult &= ","
                cResult &= cPart
            Next
            Return Split(cResult, ",")
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Checks an IP address against a comma delimited list of distinct IP addresses (not ranges)
    ''' </summary>
    ''' <param name="cIPAddress">The IP address to look for (e.g. 1.2.3.4)</param>
    ''' <param name="cIPAddressList">A comma separated list of IP addresses (e.g. 1.2.3.4,2.3.4.5)</param>
    ''' <returns>Boolean - True if found, False if not or error encountered.</returns>
    ''' <remarks>Doesn't attempt to validate the IP address at the moment, so is a glorified list finder at the mo.</remarks>
    Public Function IsIPAddressInList(ByVal cIPAddress As String, ByVal cIPAddressList As String) As Boolean

        Try
            ' Find if an IP address exists in a comma/pipe-delimited IP address range.
            Dim bFound As Boolean = False

            If cIPAddress <> "" And Not (IsNothing(cIPAddressList)) Then

                Dim oRE As System.Text.RegularExpressions.Regex = New System.Text.RegularExpressions.Regex("(,|^)" & Replace(cIPAddress, ".", "\.") & "(,|$)")
                If oRE.IsMatch(cIPAddressList) Then bFound = True
            End If

            Return bFound
        Catch ex As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Generates a Random Password
    ''' </summary>
    ''' <param name="size">The size of the password required</param>
    ''' <param name="charSet">Optional - specify the characters you want to use</param>
    ''' <param name="options">Optional - options available are UseAlpha, UseNumeric, UseSymbols, UnambiguousCharacters, LowerCase and UpperCase.  Default value is UseAlph, UseNumeric and Lowercase</param>
    ''' <param name="oRandomObject">Optional - a precreated Random object (useful if repeatedly running through passwords) </param>
    ''' <returns>String - The randomly generated password.</returns>
    ''' <remarks></remarks>
    Public Function RandomPassword(ByVal size As Integer, Optional ByVal charSet As String = "", Optional ByVal options As Text.TextOptions = TextOptions.LowerCase Or TextOptions.UseAlpha Or TextOptions.UseNumeric, Optional ByVal oRandomObject As Eonic.Tools.Number.Random = Nothing) As String

        Dim s As New StringBuilder()
        Dim r As Tools.Number.Random
        Dim output As String = ""

        Dim bUnambiguous As Boolean
        Dim charSetArray As Char()

        Try
            If oRandomObject Is Nothing Then
                r = New Tools.Number.Random()
            Else
                r = oRandomObject
            End If

            bUnambiguous = (options And TextOptions.UnambiguousCharacters) <> 0

            If String.IsNullOrEmpty(charSet) Then
                ' Build the Character Set

                ' Check for empty character set options - add a default if none are selected.
                If Not ( _
                    (options And TextOptions.UseAlpha) <> 0 _
                    Or (options And TextOptions.UseNumeric) <> 0 _
                    Or (options And TextOptions.UseSymbols) <> 0) Then

                    options = options Or TextOptions.UseAlpha Or TextOptions.UseNumeric

                End If

                If (options And TextOptions.UseAlpha) <> 0 Then s.Append("ABCDEFGHJKMNPQRTVWXY").Append(IIf(bUnambiguous, "", "ILOSUZ"))
                If (options And TextOptions.UseNumeric) <> 0 Then s.Append("346789").Append(IIf(bUnambiguous, "", "1250"))
                If (options And TextOptions.UseSymbols) <> 0 Then s.Append(")$%*?#~").Append(IIf(bUnambiguous, "", "!(){}@"))

                charSet = s.ToString()
                s = New StringBuilder()

            End If

            charSetArray = charSet.ToCharArray()


            For i As Integer = 0 To size - 1
                Dim n As Int32 = Convert.ToInt32(Math.Floor(charSetArray.Length * r.NextDouble()))
                s.Append(charSetArray(n))
            Next

            output = s.ToString()

            If (options And TextOptions.UpperCase) <> 0 Then output = output.ToUpper()
            If (options And TextOptions.LowerCase) <> 0 Then output = output.ToLower()

            Return output

        Catch ex As Exception
            Return ""
        End Try
    End Function

    ''' <summary>
    ''' Replaces the last instance of a specific character in a string with a replacement string.
    ''' </summary>
    ''' <param name="searchString">The string to search</param>
    ''' <param name="charToFind">The character to find</param>
    ''' <param name="replacement">The replacement string</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ReplaceLastCharacter(ByRef searchString As String, ByVal charToFind As Char, ByVal replacement As String) As String
        Return Regex.Replace(searchString, "(?=" & charToFind.ToString & "[^" & charToFind.ToString & "]*$)" & charToFind.ToString & "", replacement)
    End Function

    ''' <summary>
    ''' Removes the last instance of a specific character in a string with a replacement string.
    ''' </summary>
    ''' <param name="searchString">The string to search</param>
    ''' <param name="charToFind">The character to find</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function RemoveLastCharacter(ByRef searchString As String, ByVal charToFind As Char) As String
        Return ReplaceLastCharacter(searchString, charToFind, "")
    End Function

    ''' <summary>
    ''' Truncates strings up to the last word before a length limit.
    ''' </summary>
    ''' <param name="input">The string to truncate</param>
    ''' <param name="limit">The maximum length of the string</param>
    ''' <returns></returns>
    ''' <remarks>If a string is truncated ellipses (...) will be added.  In this case the length limit will be lowered by the 3 characters for ellipses to accomodate them</remarks>
    Public Function TruncateString(ByVal input As String, ByVal limit As Long) As String

        Try

            Dim truncate As String = input

            If input.Length > limit Then

                ' Truncate the string minus the ellipses (...) 
                truncate = input.Substring(0, limit - 3)

                ' Set a default return value
                Dim index As Long = limit

                ' Now find the last letter before a word boundary
                Dim oRe As New Regex("\w\b")
                Dim oMatches As MatchCollection = oRe.Matches(truncate)
                If oMatches.Count > 2 Then
                    ' We have matches (more than the beginning and the end of the string at least), get the index of the last but one 
                    index = oMatches(oMatches.Count - 2).Index + 1
                End If


                truncate = truncate.Substring(0, index) & "..."

            End If

            Return truncate

        Catch ex As Exception
            Return input
        End Try
    End Function

    ''' <summary>
    ''' String Based Coalesce - i.e. returns the first non Nothing, NUll And Empty string
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Coalesce(ByVal ParamArray args As Object()) As String

        Try
            For Each arg As Object In args
                If arg IsNot Nothing AndAlso Not String.IsNullOrEmpty(arg) Then
                    Return arg.ToString
                End If
            Next
            Return ""
        Catch ex As Exception
            Return ""
        End Try

    End Function



    Public Function SimpleRegexFind(ByVal cSearchString As String, ByVal cRegexPattern As String, Optional ByVal nReturnGroup As Integer = 0, Optional ByVal oRegexOptions As RegexOptions = RegexOptions.None) As String

        ' Given a string and a regex pattern, this will try to find the pattern in the string and return what is matched
        ' It is possible to return a submatch (i.e. specified in parentheses in the regex pattern) by settign nReturnGroup 
        ' to the required parenthes grouping number.

        ' If nothing is found then "" is returned.

        Try
            Dim oRe As Regex = New Regex(cRegexPattern, oRegexOptions)
            Dim oFind As Match = oRe.Match(cSearchString)
            If oFind.Groups.Count >= nReturnGroup Then
                Return oFind.Groups(nReturnGroup).Value
            Else
                Return ""
            End If

        Catch
            Return ""
        End Try

    End Function

    Public Function UTF8ByteArrayToString(ByVal characters As Byte()) As String
        Dim encoding As New UTF8Encoding(False)
        Return encoding.GetString(characters)
    End Function

    Public Function StringToUTF8ByteArray(ByVal characters As String) As Byte()
        Dim encoding As New UTF8Encoding(False)
        Return encoding.GetBytes(characters)
    End Function

    Function RegularExpressions() As Object
        Throw New NotImplementedException
    End Function

    Public Function CleanName(ByVal cName As String, Optional ByVal bLeaveAmp As Boolean = False, Optional ByVal bURLSafe As Boolean = False) As String
        'Valid Chars
        Dim cValids As String
        cValids = "0123456789"
        cValids &= "abcdefghijklmnopqrstuvwxyz"
        cValids &= "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
        cValids &= " -&"
        Try
            cName = Replace(cName, "'", "")
            cName = Replace(cName, "&amp;", "&")
            If Not bLeaveAmp Then cName = Replace(cName, "&", "and")
            Dim i As Integer
            Dim cBuilt As String = ""
            For i = 0 To cName.Length
                Dim cTest As String = Right(Left(cName, i), 1)
                If cValids.Contains(cTest) Then cBuilt &= cTest
            Next
            cName = cBuilt
            'replace double spaces a few times
            cName = Replace(cName, "  ", " ")
            cName = Replace(cName, "  ", " ")
            cName = Replace(cName, "  ", " ")
            If bURLSafe Then
                cName = cName.Replace(" ", "-")
            End If
            Return cName
        Catch ex As Exception
            Return cName
        End Try
    End Function


    Public Function tidyXhtmlFrag(ByVal shtml As String, Optional ByVal bReturnNumbericEntities As Boolean = False, Optional ByVal bEncloseText As Boolean = True, Optional ByVal removeTags As String = "") As String

        '   PerfMon.Log("Web", "tidyXhtmlFrag")
        Dim sProcessInfo As String = "tidyXhtmlFrag"
        Dim sTidyXhtml As String = ""

        Try

            If Not removeTags = "" Then
                shtml = removeTagFromXml(shtml, removeTags)
            End If

            Using oTdyManaged As TidyManaged.Document = TidyManaged.Document.FromString(shtml)
                oTdyManaged.OutputBodyOnly = TidyManaged.AutoBool.Yes
                oTdyManaged.MakeClean = True
                oTdyManaged.DropFontTags = True
                oTdyManaged.ShowWarnings = True
                oTdyManaged.OutputXhtml = True

                If bReturnNumbericEntities Then
                    '    oTdyManaged.NumEntities = True
                End If
                oTdyManaged.CleanAndRepair()
                sTidyXhtml = oTdyManaged.Save()
                oTdyManaged.Dispose()
            End Using

            Return sTidyXhtml

        Catch ex As Exception
            ' It is the desired behaviour for this to return nothing if not valid html don't turn this on apart from in development.            Return Nothing
            Return ex.Message
            'Return Nothing
        Finally

            sTidyXhtml = Nothing
        End Try
    End Function

    Public Function removeTagFromXml(ByVal xmlString As String, ByVal tagNames As String) As String

        tagNames = Replace(tagNames, " ", "")
        tagNames = Replace(tagNames, ",", "|")

        xmlString = Regex.Replace(xmlString, "<[/]?(" & tagNames & ":\w+)[^>]*?>", "", RegexOptions.IgnoreCase)

        Return xmlString

    End Function

End Module



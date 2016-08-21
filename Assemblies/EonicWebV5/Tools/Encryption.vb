Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web.Configuration
Imports System


Class Encryption

    Public Function GenerateMD5Hash(ByVal SourceText As String) As String
        PerfMon.Log("stdTools", "GenerateMD5Hash")
        'This generates a PHP compatible MD5 Hex string for the source value.

        Dim md5 As MD5 = MD5CryptoServiceProvider.Create
        Dim dataMd5 As Byte() = md5.ComputeHash(Encoding.Default.GetBytes(SourceText))
        Dim sb As StringBuilder = New StringBuilder
        Dim i As Integer = 0
        While i < dataMd5.Length
            sb.AppendFormat("{0:x2}", dataMd5(i))
            System.Math.Min(System.Threading.Interlocked.Increment(i), i - 1)
        End While
        Return sb.ToString
    End Function

    Friend Class Utils

        ''' <summary>
        ''' converts an array of bytes to a string Hex representation
        ''' </summary>
        Friend Shared Function ToHex(ByVal ba() As Byte) As String
            If ba Is Nothing OrElse ba.Length = 0 Then
                Return ""
            End If
            Const HexFormat As String = "{0:X2}"
            Dim sb As New StringBuilder
            For Each b As Byte In ba
                sb.Append(String.Format(HexFormat, b))
            Next
            Return sb.ToString
        End Function

        ''' <summary>
        ''' converts from a string Hex representation to an array of bytes
        ''' </summary>
        Friend Shared Function FromHex(ByVal hexEncoded As String) As Byte()
            If hexEncoded Is Nothing OrElse hexEncoded.Length = 0 Then
                Return Nothing
            End If
            Try
                Dim l As Integer = Convert.ToInt32(hexEncoded.Length / 2)
                Dim b(l - 1) As Byte
                For i As Integer = 0 To l - 1
                    b(i) = Convert.ToByte(hexEncoded.Substring(i * 2, 2), 16)
                Next
                Return b
            Catch ex As Exception
                Throw New System.FormatException("The provided string does not appear to be Hex encoded:" & _
                    Environment.NewLine & hexEncoded & Environment.NewLine, ex)
            End Try
        End Function

        ''' <summary>
        ''' converts from a string Base64 representation to an array of bytes
        ''' </summary>
        Friend Shared Function FromBase64(ByVal base64Encoded As String) As Byte()
            If base64Encoded Is Nothing OrElse base64Encoded.Length = 0 Then
                Return Nothing
            End If
            Try
                Return Convert.FromBase64String(base64Encoded)
            Catch ex As System.FormatException
                Throw New System.FormatException("The provided string does not appear to be Base64 encoded:" & _
                    Environment.NewLine & base64Encoded & Environment.NewLine, ex)
            End Try
        End Function

        ''' <summary>
        ''' converts from an array of bytes to a string Base64 representation
        ''' </summary>
        Friend Shared Function ToBase64(ByVal b() As Byte) As String
            If b Is Nothing OrElse b.Length = 0 Then
                Return ""
            End If
            Return Convert.ToBase64String(b)
        End Function

        ''' <summary>
        ''' retrieve an element from an Xml string
        ''' </summary>
        Friend Shared Function GetXmlElement(ByVal Xml As String, ByVal element As String) As String
            Dim m As Match
            m = Regex.Match(Xml, "<" & element & ">(?<Element>[^>]*)</" & element & ">", RegexOptions.IgnoreCase)
            If m Is Nothing Then
                Throw New Exception("Could not find <" & element & "></" & element & "> in provided Public Key Xml.")
            End If
            Return m.Groups("Element").ToString
        End Function

        ''' <summary>
        ''' Returns the specified string value from the application .config file
        ''' </summary>
        Friend Shared Function GetConfigString(ByVal key As String, _
            Optional ByVal isRequired As Boolean = True) As String
            Dim strReturn As String = ""
            Dim s As String = CType(WebConfigurationManager.AppSettings.Get(key), String)
            If s = Nothing Then
                If isRequired Then
                    ' Throw New ConfigurationException("key <" & key & "> is missing from .config file")
                Else
                    strReturn = ""
                End If
            Else
                strReturn = s
            End If
            Return strReturn
        End Function

        Friend Shared Function WriteConfigKey(ByVal key As String, ByVal value As String) As String
            Dim s As String = "<add key=""{0}"" value=""{1}"" />" & Environment.NewLine
            Return String.Format(s, key, value)
        End Function

        Friend Shared Function WriteXmlElement(ByVal element As String, ByVal value As String) As String
            Dim s As String = "<{0}>{1}</{0}>" & Environment.NewLine
            Return String.Format(s, element, value)
        End Function

        Friend Shared Function WriteXmlNode(ByVal element As String, Optional ByVal isClosing As Boolean = False) As String
            Dim s As String
            If isClosing Then
                s = "</{0}>" & Environment.NewLine
            Else
                s = "<{0}>" & Environment.NewLine
            End If
            Return String.Format(s, element)
        End Function

    End Class

End Class

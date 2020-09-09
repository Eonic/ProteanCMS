<% 
Response.Expires = -1
Response.Buffer=True 
Dim MyConnection, cFeed, cURL

' Specifying the URL
cFeed = request("feed")

Select Case cFeed

	Case "currency_converter"
		cURL = "http://www.ecb.int/stats/eurofxref/eurofxref-daily.xml"
	
	Case Else
		cURL = request("url")

End Select

If cURL <> "" Then
	'Set MyConnection = Server.CreateObject("Microsoft.XMLHTTP")
    Set MyConnection = Server.CreateObject("MSXML2.ServerXMLHTTP.4.0")
	' Connecting to the URL
	MyConnection.Open "GET", cURL, False

	' Sending and getting data
	MyConnection.Send 
	TheData = MyConnection.responseText

	'Set the appropriate content type
	Response.ContentType = MyConnection.getResponseHeader("Content-Type")
	Response.Write (TheData)

	'MyConnection.Close
	Set MyConnection = Nothing
End If
%>
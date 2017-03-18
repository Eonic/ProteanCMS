<%@ Page Language="VB" %>
<%@ Import NameSpace = "System.Security.Principal" %>
<script runat="server">
Sub Page_Load()
Dim tmp As String = WindowsIdentity.GetCurrent.Name()
Label1.Text = tmp
End Sub
</script>
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
<title>WindowsIdentity.GetCurrent.Name()</title>
</head>
<body>
<form id="form1" runat="server">
<div>
<asp:Label ID="Label1" Runat="server" Text="Label"></asp:Label>
</div>
</form>
</body>
</html>


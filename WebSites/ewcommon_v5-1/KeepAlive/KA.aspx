<%@ Page Language="VB" AutoEventWireup="false" CodeFile="KA.aspx.vb" Inherits="KA" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Keep Alive Progres</title>
    <link href="KA.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Table ID="tblProgress" runat="server" Height="1px" Width="288px">
            <asp:TableRow runat="server">
                <asp:TableCell ID="lblDescription" runat="server" CssClass="DecriptionRow" ColumnSpan="20">Decription</asp:TableCell>
            </asp:TableRow>
            <asp:TableRow runat="server" CssClass="ProgressRow">
                <asp:TableCell ID="ProgressCell0" runat="server" CssClass="ProgressCell" Wrap="False">&#160</asp:TableCell>
                <asp:TableCell ID="ProgressCell1" runat="server" CssClass="ProgressCell" Wrap="False">&#160</asp:TableCell>
                <asp:TableCell ID="ProgressCell2" runat="server" CssClass="ProgressCell" Wrap="False">&#160</asp:TableCell>
                <asp:TableCell ID="ProgressCell3" runat="server" CssClass="ProgressCell" Wrap="False">&#160</asp:TableCell>
                <asp:TableCell ID="ProgressCell4" runat="server" CssClass="ProgressCell" Wrap="False">&#160</asp:TableCell>
                <asp:TableCell ID="ProgressCell5" runat="server" CssClass="ProgressCell" Wrap="False">&#160</asp:TableCell>
                <asp:TableCell ID="ProgressCell6" runat="server" CssClass="ProgressCell" Wrap="False">&#160</asp:TableCell>
                <asp:TableCell ID="ProgressCell7" runat="server" CssClass="ProgressCell" Wrap="False">&#160</asp:TableCell>
                <asp:TableCell ID="ProgressCell8" runat="server" CssClass="ProgressCell" Wrap="False">&#160</asp:TableCell>
                <asp:TableCell ID="ProgressCell9" runat="server" CssClass="ProgressCell" Wrap="False">&#160</asp:TableCell>
                <asp:TableCell ID="ProgressCell10" runat="server" CssClass="ProgressCell" Wrap="False">&#160</asp:TableCell>
                <asp:TableCell ID="ProgressCell11" runat="server" CssClass="ProgressCell" Wrap="False">&#160</asp:TableCell>
                <asp:TableCell ID="ProgressCell12" runat="server" CssClass="ProgressCell" Wrap="False">&#160</asp:TableCell>
                <asp:TableCell ID="ProgressCell13" runat="server" CssClass="ProgressCell" Wrap="False">&#160</asp:TableCell>
                <asp:TableCell ID="ProgressCell14" runat="server" CssClass="ProgressCell" Wrap="False">&#160</asp:TableCell>
                <asp:TableCell ID="ProgressCell15" runat="server" CssClass="ProgressCell" Wrap="False">&#160</asp:TableCell>
                <asp:TableCell ID="ProgressCell16" runat="server" CssClass="ProgressCell" Wrap="False">&#160</asp:TableCell>
                <asp:TableCell ID="ProgressCell17" runat="server" CssClass="ProgressCell" Wrap="False">&#160</asp:TableCell>
                <asp:TableCell ID="ProgressCell18" runat="server" CssClass="ProgressCell" Wrap="False">&#160</asp:TableCell>
                <asp:TableCell ID="ProgressCell19" runat="server" CssClass="ProgressCell" Wrap="False">&#160</asp:TableCell>
            </asp:TableRow>
            <asp:TableRow runat="server">
                <asp:TableCell ID="clDescription" runat="server" CssClass="DecriptionRow" ColumnSpan="20">Progress</asp:TableCell>
            </asp:TableRow>
            <asp:TableRow runat="server">
                <asp:TableCell ID="clExtraInfo" runat="server" ColumnSpan="20" CssClass="ExtraInfo">Extra Info</asp:TableCell>
            </asp:TableRow>
        </asp:Table>
    
    </div>
    </form>
</body>
</html>

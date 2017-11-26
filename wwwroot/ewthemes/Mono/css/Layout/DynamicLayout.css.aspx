<%@ Page Language="VB" AutoEventWireup="false" CodeFile="DynamicLayout.css.aspx.vb" Inherits="css_Layout_DynamicLayout" %>
<%  
    Response.Cache.SetExpires(DateTime.Now.AddDays(1))
    Response.ContentType = "text/css"
    
    Dim fullWidth As Integer = IIf(CInt("0" & Request("fullWidth")) = 0, 984, CInt(Request("fullWidth")))
    Dim NavWidth As Integer = IIf(CInt("0" & Request("NavWidth")) = 0, 220, CInt(Request("NavWidth")))
    Dim fwGrv() As String = getGoldenArray(fullWidth)
    
    'TS changed how this works to look for empty string so we can pass through 0 value.
    Dim colPad As Integer = IIf(Request("colPad") = "", fwGrv(8), CInt(Request("colPad")))
    Dim boxPad As Integer = IIf(Request("boxPad") = "", fwGrv(9), CInt(Request("boxPad")))
    
    Dim pageWidth As Integer = fullWidth - (NavWidth + colPad)
    Dim pwGrv() As String = getGoldenArray(pageWidth)
    Dim i As Long
    Dim colPadHalf As Integer = rd(colPad / 2)
    
    'Common used column widths =========================================
    'pageWidth column widths
    Dim pw50column As Integer = rd((pageWidth - colPad) / 2)
    Dim pw50columnLast As Integer = pageWidth - (pw50column + colPad)
    
    Dim pw25columnFirst As Integer = rd((pw50column - colPad) / 2)
    Dim pw25columnFirstLast As Integer = pw50column - (pw25columnFirst + colPad)
    Dim pw25columnLast As Integer = rd((pw50columnLast - colPad) / 2)
    Dim pw25columnLastLast As Integer = pw50columnLast - (pw25columnLast + colPad)
    
    Dim pw66column As Integer = rd(pwGrv(1) - colPadHalf)
    Dim pw33column As Integer = rd(pwGrv(2) - colPadHalf)
    
    Dim pw3column As Integer = rd((pageWidth - (colPad * 2)) / 3)
    Dim pw3columnLast As Integer = pageWidth - ((pw3column + colPad) * 2)
    
    Dim pw75column As Integer = rd(((pageWidth - colPad) / 4) * 3)
    Dim pw25column As Integer = rd(pageWidth - colPad - pw75column)
    
    Dim pw4column As Integer = rd((pageWidth - (colPad * 3)) / 4)
    Dim pw4columnLast As Integer = pageWidth - ((pw4column + colPad) * 3)
    
    'fullWidth column widths
    Dim fw50column As Integer = rd((fullWidth - colPad) / 2)
    Dim fw50columnLast As Integer = fullWidth - (fw50column + colPad)
    
    Dim fw25columnFirst As Integer = rd((fw50column - colPad) / 2)
    Dim fw25columnFirstLast As Integer = fw50column - (fw25columnFirst + colPad)
    Dim fw25columnLast As Integer = rd((fw50columnLast - colPad) / 2)
    Dim fw25columnLastLast As Integer = fw50columnLast - (fw25columnLast + colPad)
    
    Dim fw66column As Integer = rd(fwGrv(1) - colPadHalf)
    Dim fw33column As Integer = rd(fwGrv(2) - colPadHalf)
    
    Dim fw3column As Integer = rd((fullWidth - (colPad * 2)) / 3)
    Dim fw3columnLast As Integer = fullWidth - ((fw3column + colPad) * 2)
    
    Dim fw75column As Integer = rd(((fullWidth - colPad) / 4) * 3)
    Dim fw25column As Integer = rd(fullWidth - colPad - fw75column)
    
    Dim fw4column As Integer = rd((fullWidth - (colPad * 3)) / 4)
    Dim fw4columnLast As Integer = fullWidth - ((fw4column + colPad) * 3)
    
%>
/*  
    Automated column calculations for eonicweb
    © Eonicweb Ltd 2011 - Authors: Will Hancock, Trevor Spink 
*/

/* full Width Golden Ratio Values
<%  For i = 0 To UBound(fwGrv)
        Response.Write(i & "-" & fwGrv(i) & " / ")
    Next
%>*/
/* page Width Golden Ratio Values
<%  For i = 0 To UBound(pwGrv)
        Response.Write(i & "-" & pwGrv(i) & " / ")
    Next
%>*/


.rowMargin{margin-right:<%=colPad%>px;}
div.module,
.box{margin-bottom:<%=colPad%>px}
.box div.module{margin-bottom:0px}
.box h2.title,
.box .content{padding:<%=boxPad%>px}
#leftCol{width:<%=NavWidth%>px;margin-bottom:<%=colPad%>px;}
.wrapper{width:<%=fullWidth%>px;}
#mainLayout{float:left;width:<%=pageWidth%>px;margin-left:<%=colPad%>px;margin-bottom:<%=colPad%>px;}
div.fullwidth{width: <%=fullWidth%>px !important; margin-left: 0 !important;}
#mainLayoutContainer{width:<%=fullWidth%>px;padding-top:<%=colPad%>px;}
#mainWrapper{width:<%= (fullWidth + (colPad * 2))%>px;}
#mainMenuWrapper{margin-bottom:<%=colPad%>px}
#mainHeader{margin-bottom:<%=colPad%>px;padding-top:<%=colPad%>px;}

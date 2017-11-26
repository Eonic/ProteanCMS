<%@ Page Language="VB" AutoEventWireup="false"  %>
<%@ Import Namespace="System.Web.Configuration" %>
<%  
    Response.ContentType = "text/css"
    
    Dim moConfig As System.Collections.Specialized.NameValueCollection = WebConfigurationManager.GetWebApplicationSection("eonic/theme")
    Dim font As String = IIf(moConfig("Mono.FontColor") = Nothing, "#000", moConfig("Mono.FontColor"))
    Dim bodyfont As String = IIf(moConfig("Mono.BodyFont") = Nothing, "Arial, Arial, Helvetica, sans-serif", moConfig("Mono.BodyFont"))
    Dim headingfont As String = IIf(moConfig("Mono.HeadingFont") = Nothing, "Arial, Arial, Helvetica, sans-serif", moConfig("Mono.HeadingFont"))
    Dim base As String = IIf(moConfig("Mono.BaseColor") = Nothing, "#ababab", moConfig("Mono.BaseColor"))
    Dim primary As String = IIf(moConfig("Mono.PrimaryColor") = Nothing, "#6e9855", moConfig("Mono.PrimaryColor"))
    Dim secondary As String = IIf(moConfig("Mono.SecondaryColor") = Nothing, "#FF6600", moConfig("Mono.SecondaryColor"))
    Dim tertiary As String = IIf(moConfig("Mono.TertiaryColor") = Nothing, "#666", moConfig("Mono.TertiaryColor"))
    Dim quartary As String = IIf(moConfig("Mono.QuartaryColor") = Nothing, "#666", moConfig("Mono.QuartaryColor"))
    Dim footer As String = IIf(moConfig("Mono.FooterColor") = Nothing, "#666", moConfig("Mono.FooterColor"))
    Dim topfooter As String = IIf(moConfig("Mono.TopFooterColor") = Nothing, "#666", moConfig("Mono.FooterColor"))
    Dim background As String = IIf(moConfig("Mono.BackgroundColor") = Nothing, "#666", moConfig("Mono.BackgroundColor"))
    Dim backgroundimage As String = IIf(moConfig("Mono.BackgroundImage") = Nothing, "", moConfig("Mono.BackgroundImage"))
    Dim backgroundrepeat As String = IIf(moConfig("Mono.BackgroundRepeat") = Nothing, "", moConfig("Mono.BackgroundRepeat"))
    Dim backgroundposition As String = IIf(moConfig("Mono.BackgroundPosition") = Nothing, "", moConfig("Mono.BackgroundPosition"))
    Dim backgroundattachment As String = IIf(moConfig("Mono.BackgroundAttachment") = Nothing, "", moConfig("Mono.BackgroundAttachment"))
    
%>

<script runat="server">
	Private Function ColorToRgba(ByVal HexColorIn As String, ByVal Opacity As Double) As String

		Dim strOutput As New StringBuilder
		Dim colArgb As System.Drawing.Color = Drawing.ColorTranslator.FromHtml(HexColorIn)
		Dim strOpacity As String = Opacity.ToString

		strOutput.Append("rgba(")
		strOutput.Append(colArgb.R.ToString)
		strOutput.Append(", ")
		strOutput.Append(colArgb.G.ToString)
		strOutput.Append(", ")
		strOutput.Append(colArgb.B.ToString)
		strOutput.Append(", ")
		strOutput.Append(strOpacity)
		strOutput.Append(")")

		Return strOutput.ToString

	End Function
</script>

<%
	Dim background75 As String = ColorToRgba(background, 0.85)
	Dim base75 As String = ColorToRgba(base, 0.85)
    Dim primary75 As String = ColorToRgba(primary, 0.85)
    Dim secondary75 As String = ColorToRgba(secondary, 0.85)
    Dim tertiary75 As String = ColorToRgba(tertiary, 0.85)
    Dim quartary75 As String = ColorToRgba(quartary, 0.85)
%>
/*  Automated Colour Calculations for Mono Theme
    © Eonic Ltd 2011 - Author: Dan 'Danger' Meek
    Date Started - 03/11/2011
*/    

body, html{font-family:<%= bodyfont%>!important;color:<%=font%>;background-color:<%= background%>;background-image: url(<%= backgroundimage%>);background-position:<%= backgroundposition%>;background-repeat:<%= backgroundrepeat%>;background-attachment:<%= backgroundattachment%>;}
.Site h1,
.Site h2,
.Site h3,
.Site h4,
.Site h5,
.Site h6{color:<%=font%>;font-family:<%= headingfont%>!important;}
.Site a{color:<%=primary%>;}
.Site a:hover{color:<%=secondary%>;}

/*################## BOXES ###################*/

.Site .box h2.title,
.Site .box h2.title a{color:<%= font%>;border-color:<%= tertiary%>;background:<%=primary%>;}
.Site .box .content{border-color:<%= tertiary%>}

/*################## MENU ###################*/

.Site #mainMenu,
.Site #location,
.Site #mainHeader,
.Site #mainLayoutContainer{border-color:<%= tertiary%>}

/* -- Main Menu -- */
.Site #mainMenu ul a{border-color:<%= tertiary%>}
.Site #mainMenu ul a{color:<%= primary%>;}
.Site #mainMenu ul a:hover{color:<%=secondary%>;}
.Site #mainMenu ul a.on,
.Site #mainMenu ul a.active{color:<%=secondary%>;}

/* -- Top Sub Menu -- */
.Site #mainMenu1,
.Site #mainMenu2,
.Site #mainMenu3,
.Site #mainMenu4,
.Site #mainMenu5{border-color:<%= tertiary%>}
.Site #mainMenu1 ul li a,
.Site #mainMenu2 ul li a,
.Site #mainMenu3 ul li a,
.Site #mainMenu4 ul li a,
.Site #mainMenu5 ul li a{border-color:<%= tertiary%>}
.Site #mainMenu1 ul a.on,
.Site #mainMenu1 ul a.active{color:<%= secondary%>}
.Site #mainMenu2 ul a.on,
.Site #mainMenu2 ul a.active{color:<%= secondary%>}
.Site #mainMenu3 ul a.on,
.Site #mainMenu3 ul a.active{color:<%= secondary%>}
.Site #mainMenu4 ul a.on,
.Site #mainMenu4 ul a.active{color:<%=secondary%>}

/* -- Left Sub Menu -- */
.Site #subMenu {background-color:<%=base%>;}
.Site #subMenu ul a{border-color:<%=base%>;}
.Site #subMenu ul a.active,
.Site #subMenu ul a:hover,
.Site #subMenu ul a.on{color:<%=secondary%>;}
.Site #subMenu ul ul ul a{color:<%=font%>}

/*################## FOOTER ###################*/

.Site #mainFooter {border-color:<%= tertiary%>;background-color:<%= background%>;color:<%= font%>;}
.Site #mainFooter h2.title {color:<%= primary%>}

/*################## FORMS ###################*/

/* -- Principle Button -- */
.Site #mainLayout form .principle,
.Site #headerInfo .principle,
.Site #cartFull .principle,
.Site .EmailForm .principle,
.Site .ewXform input.principle{color:<%= base%>!important;background:<%= primary%>}
.Site #mainLayout form .principle:hover,
.Site #headerInfo .principle:hover,
.Site #cartFull .principle:hover,
.Site .EmailForm .principle:hover,
.Site .ewXform input:hover.principle{color:<%= base%>!important;background:<%= secondary%>}

/* -- Buttons -- */
.Site .hproduct .cartButtons .ewXform .button{color:<%=font%>;}
.Site .searchForm .submitButton, .Site #UserLogon .loginButton, .Site .userName .button{color:<%=font%>;}

/* -- Xforms -- */
.Site .ewXform span.alert, .ewXform span.hint, .ewXform span.help {color:<%=font%>;}
.Site .ewXform input.button, #cartFull input.principle {}

/* -- Cart -- */
.Site #cartFull #quoteSteps{background:<%=secondary%>;}
.Site #cartFull #quoteSteps{color:<%=font%>;}
.Site #cartListing TH.heading,#cartListing TD.amount {background:none;}
.Site #cartFull #quoteSteps .active{color:<%=font%>;}
.Site #cartFull input.continue, #cartFull input.update, #cartFull input.empty {background-color:<%=base%>!important;}
.Site #cartFull input.continue:hover, #cartFull input.update:hover, #cartFull input.empty:hover {color:<%=tertiary%>;}

/*################## BESPOKE ###################*/

.Site #imageText h1,
.Site #imageText h2,
.Site #imageText h3,
.Site #imageText h4,
.Site #imageText h5,
.Site #imageText h6,
.Site #imageText p{color:#FFF}
.Site #imageText h1{background-color:<%=secondary%>}
.Site #imageText h2{background-color:<%=primary%>}
.Site #imageText h3{background-color:<%=tertiary%>}
.Site #imageText h4{background-color:<%=base%>}
.Site #imageText h5{background-color:<%=quartary%>;}
.Site #imageText h6{background-color:<%=secondary%>}
.Site #imageText p{background-color:<%=primary%>}
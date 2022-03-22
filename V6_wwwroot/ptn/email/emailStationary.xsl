<?xml version="1.0" encoding="UTF-8" ?>
<!-- 
	THIS IS THE EMAIL STATIONARY XSL FOR EONIC
	IT IS NOT A TEMPLATE.
-->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  
	<xsl:import href="../Tools/Functions.xsl"/>
  
  <xsl:variable name="siteURL">
    
    <xsl:variable name="serverVariableURL">
			<xsl:call-template name="getServerVariable">
				<xsl:with-param name="valueName" select="'HTTP_HOST'"/>
			</xsl:call-template>
    </xsl:variable>
    
    <xsl:variable name="cartUrl">
      <xsl:call-template name="getSettings">
        <xsl:with-param name="sectionName" select="'cart'"/>
        <xsl:with-param name="valueName" select="'SiteURL'"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="$cartUrl!=''">
        <xsl:value-of select="$cartUrl"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>http://</xsl:text><xsl:value-of select="$serverVariableURL"/>
      </xsl:otherwise>
    </xsl:choose>
  
	</xsl:variable>
		
  <xsl:variable name="siteTitle">
			<xsl:call-template name="getSettings">
				<xsl:with-param name="sectionName" select="'web'"/>
				<xsl:with-param name="valueName" select="'SiteName'"/>
			</xsl:call-template>
	</xsl:variable>

  <xsl:variable name="SiteLogo">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'SiteLogo'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name="CompanyName">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'CompanyName'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name="CompanyAddress">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'CompanyAddress'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name="CompanyTel">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'CompanyTel'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name="CompanyEmail">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'CompanyEmail'"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="VATnumber">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'VATnumber'"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="CompanyRegNo">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'CompanyRegNo'"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="CharityRegNo">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'CharityRegNo'"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:template match="*">
    <html>
      <head>
        <title>
          <xsl:apply-templates select="." mode="subject"/>
        </title>
        <xsl:apply-templates select="." mode="emailStyle"/>
        <xsl:if test="/Page/@baseUrl">
          <base href="{/Page/@baseUrl}"/>
        </xsl:if>
        <xsl:if test="/Page/@adminMode">
          <xsl:apply-templates select="." mode="commonStyle"/>
          <xsl:apply-templates select="." mode="js"/>
        </xsl:if>
      </head>
      <xsl:apply-templates select="." mode="emailBody"/>
    </html>
  </xsl:template>
  <xsl:template match="*" mode="pageTitle">
    <xsl:apply-templates select="." mode="subject"/>
  </xsl:template>
  
  <xsl:template match="*" mode="emailStyle">
    <style>
      h1
      {font-family:calibri,Verdana,Trebuchet MS,Lusidia Sans Unicode;margin:5px;margin-left:0;margin-right:0;font-size:1.4em;}
      h2
      {margin:5px;font-family:calibri,Verdana,Trebuchet MS,Lusidia Sans Unicode;margin-left:0;margin-right:0;font-size:1.3em;}
      h3
      {margin:5px;margin-left:0;margin-right:0;font-size:1.2em;}
      h4
      {margin:5px;margin-left:0;margin-right:0;font-size:1.1em;}
      h5
      {margin:5px;margin-left:0;margin-right:0;font-size:1em;}
      h6
      {margin:5px;margin-left:0;margin-right:0;font-size:0.9em;}
      img
      {border:none;}
      p
      {margin:0 0 1em;}
      td, th
      {line-height:1.7;}
      #msgBody td,
      #msgBody th
      {text-align: left;vertical-align:top !important;line-height:1.2;}
      .alternative th
      {text-align:left;}
      td.price,td.lineTotal,td.amount,td.heading,th.price,th.lineTotal
      {text-align:right !important;}
      td.total
      {background:#F46E30;color:#000000;font-weight:bold;}
      {text-decoration:underline;}
      #mainTable a{color:#31588f;font-weight:bold;text-decoration:none;}
      #mainTable a:hover{color:#6dbde9 !important}
      #mainTable a:hover img{border:1px solid #E25919 !important}
      #footer a
      {color: #31588f !important;text-decoration:none;}
      .productContainer h4{font-size:12px}
      a.devLink{color:#BFCCD2 !important;text-decoration:none !important;font-weight:normal;}
      a.devLink:hover{color:#EE6603 !important}
      hr{border:1px solid #5C85B7;}
      img.imageFloatRight{float:right; !important}
      img.imageFloatLeft{float:left; !important}
      p {font-size: 11px;}
      #cartListing td.cell,
      #cartListing td.heading{border-bottom:2px solid #ddd;}
      td.price,td.lineTotal,td.amount,td.heading,th.price,th.lineTotal{text-align:right;}
      td.total,#cartListing th{background:#eee;color:#000000;font-weight:bold;}
      #cartListing th{border-bottom:2px solid #ddd;padding:5px;}
      #cartListing td{border-bottom:2px solid #ddd;padding:5px;}
      .sortArrows {display:none;}
    </style>
  </xsl:template>
  
  <xsl:template match="*" mode="emailBody">
      <body bgcolor="#fff" style="font-family:calibri,Verdana,Trebuchet MS,Lusidia Sans Unicode,sans-serif;color:#000;font-size:11px;">
        <xsl:if test="@adminMode='false'">
          <xsl:attribute name="class">normalMode Email</xsl:attribute>
        </xsl:if>
        <center>
          <table id="mainTable" cellpadding="0" cellspacing="0" width="650" style="width:650px !important;">
            <tr>
              <td id="header" width="650" align="left" style="text-align:left;padding-top:10px;padding-bottom:10px">
                <xsl:choose>
                  <xsl:when test="$SiteLogo!=''">
                    <a href="{$siteURL}" title="{$siteTitle}">
                      <img src="{$siteURL}/{$SiteLogo}" alt="{$siteTitle}"/>
                    </a>
                  </xsl:when>
                  <xsl:otherwise>
                    <a href="{$siteURL}" title="{$siteTitle}">
                      <h2>
                        <xsl:value-of select="$siteTitle"/>
                      </h2>
                    </a>
                  </xsl:otherwise>
                </xsl:choose>
              </td>
            </tr>
            <tr>
              <td id="header2" width="650" align="left" style="text-align:left;background-color: #ffffff;padding-left:10px;padding-top:0;margin-top:0;font-weight:bold;">
                <h1>
                  <font face="verdana" color="#000000">
                      <xsl:apply-templates select="." mode="pageTitle"/>
                  </font>
                </h1>
              </td>
            </tr>
            <tr>
              <td id="msgBody" width="650" align="left" style="text-align:left;border:1px solid #FFF;background:#FFF">
                <font face="calibri,verdana" size="2" color="#000000">
                  <xsl:apply-templates select="." mode="bodyLayout"/>
                </font>
                <br/>
                <br/>
                <br/>
              </td>
            </tr>
            <tr>
              <td id="footer" width="650" align="left" height="30" style="text-align:left;background-color:#ddd;padding:10px;">
                
                  <xsl:choose>
                    <xsl:when test="$CompanyName!=''">
		    
		  <font face="calibri,verdana" size="3" color="#000000">
                      <strong><xsl:value-of select="$CompanyName"/>
                      </strong>
                      <br/>
                      <xsl:if test="$CompanyAddress!=''">
                        <xsl:value-of select="$CompanyAddress"/>
                        <br/>
                      </xsl:if>
                      <xsl:if test="$CompanyTel!=''">
                       <strong>Tel:</strong>&#160;<xsl:value-of select="$CompanyTel"/>&#160;
                      </xsl:if>
                      <xsl:if test="$CompanyTel!=''">
                        <strong>Email:</strong>&#160;<a href="mailto:{$CompanyEmail}">
                          <xsl:value-of select="$CompanyEmail"/>
                        </a>&#160;
                      </xsl:if>
		      <br/>
		      </font>
		      <font face="calibri,verdana" size="3" color="#999">
                      <xsl:if test="$CompanyRegNo!=''">
                        <br/><strong>Registered in UK:</strong>&#160;<xsl:value-of select="$CompanyRegNo"/>&#160;
                      </xsl:if>
                      <xsl:if test="$CharityRegNo!=''">
                        &#160;<strong>Charity Number:</strong>&#160;<xsl:value-of select="$CharityRegNo"/>&#160;
                      </xsl:if>
                      <xsl:if test="$VATnumber!=''">
                        &#160;<strong>VAT Number:</strong>&#160;<xsl:value-of select="$VATnumber"/>&#160;
                      </xsl:if>
		      </font>
                    </xsl:when>
                    <xsl:otherwise>
		    <font face="calibri,verdana" size="3" color="#000000">
                      Sent From <xsl:value-of select="$siteTitle"/>
                      <br/><xsl:value-of select="$siteURL"/>
		      </font>
                    </xsl:otherwise>
                  </xsl:choose>
                
              </td>
            </tr>
          </table>
        </center>
      </body>
  </xsl:template>
  
  
</xsl:stylesheet>
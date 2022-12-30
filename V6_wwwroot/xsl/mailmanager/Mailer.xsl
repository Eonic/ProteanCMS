<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
  <xsl:variable name="currentPage" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]"/>
  <xsl:variable name="sectionPage" select="/Page/Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]"/>
  <xsl:variable name="siteURL">
    <xsl:text>http://</xsl:text>
    <xsl:value-of select="/Page/Request/ServerVariables/Item[@name='SERVER_NAME']"/>
  </xsl:variable>
  <!-- -->
  <xsl:template match="Page">
    <html>
      <xsl:choose>
        <xsl:when test="/Page/Contents/Content[@name='XmlLang']">
          <xsl:attribute name="xml:lang">
            <xsl:value-of select="Contents/Content[@name='XmlLang']"/>
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="xml:lang">en:gb</xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
      <head>
        <title>
          <xsl:apply-templates select="$currentPage" mode="getDisplayName"/>
        </title>
        <xsl:if test="@adminMode">
          <xsl:apply-templates select="." mode="commonStyle"/>
        </xsl:if>
        <style>
          h1
          {margin:5px;margin-left:0;margin-right:0;font-size:1.4em;}
          h2
          {margin:5px;margin-left:0;margin-right:0;font-size:1.3em;}
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
          {background:#F46E30;color:#FFF;font-weight:bold;}
          {text-decoration:underline;}
          #mainTable a{color:#000000;font-weight:bold;text-decoration:none;}
          #mainTable a:hover{color:##696969 !important}
          #mainTable a:hover img{border:1px solid #E25919 !important}
          #footer a
          {color: #ffffff !important;text-decoration:none;font-weight:bold;}
          .productContainer h4{font-size:12px}
          a.devLink{color:#ffffff !important;text-decoration:none !important;font-weight:normal !important;}
          a.devLink:hover{color:#EE6603 !important}
          hr{border:1px solid #5C85B7;}
          img.imageFloatRight{float:right; !important}
          img.imageFloatLeft{float:left; !important}
          p {font-size: 11px;}
          #cartListing{border-right:1px solid #474747;}
          #cartListing td.cell,
          #cartListing td.heading
          {border-left:1px solid #474747;border-bottom:1px solid #474747;}
          td.price,td.lineTotal,td.amount,td.heading,th.price,th.lineTotal
          {text-align:right;}
          td.total,#cartListing th
          {background:#a3a3a3 url(/images/layout/cartListing_th.gif) top left repeat-x !important;color:#FFF;font-weight:bold;}
          #cartListing th{border: 1px solid #474747;border-right:0;}
          #cartListing td.vat,
          #cartListing td.subTotal,
          #cartListing td.shipping
          {border-left:1px solid #474747;border-bottom:1px solid #474747;background:#eeeeee;}
        <xsl:if test="contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 6.0') and not(contains(Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'Opera')) and /Page/@adminMode">
            <xsl:text>.editable {height:20px !important}</xsl:text>
          </xsl:if>
        </style>
      </head>
      <body bgcolor="#bcbcbc" style="font-family:Verdana, Verdana,Lusidia Sans Unicode,sans-serif;color:#000;font-size:11px;margin-top: 20px;">
        <xsl:if test="@adminMode">
          <xsl:attribute name="class">ewAdmin</xsl:attribute>
          <ul id="popup_menu" class="inlinePopupOptions" onmouseover="adminMenu(this,'onMenu')" onmouseout="adminMenu(this,'offMenu')">&#160;</ul>
        </xsl:if>
        <xsl:if test="@adminMode='true'">

          <xsl:apply-templates select="AdminMenu"/>

          <div id="adminLayout">
            <xsl:apply-templates select="." mode="Admin"/>
          </div>
        </xsl:if>
        <xsl:if test="@adminMode='false'">
          <xsl:apply-templates select="AdminMenu"/>
        </xsl:if>
        <xsl:if test="not(@adminMode='true')">
          <center>
            <table id="mainTable" cellpadding="0" cellspacing="0" width="650" style="width:650px !important">
              <tr>
                <td id="header" width="650" height="87" align="left" style="text-align:left;background-color: #333333;">
                  <a href="{$siteURL}" title="Eonic">
                    <img src="{$siteURL}/images/layout/emailStationary/header.jpg" width="650" height="87" alt="Eonic"  style="border:0 !important;"/>
                  </a>
                </td>
              </tr>
              <tr>
                <td id="header2" width="650" height="31" align="left" style="text-align:left;background: #333333; padding-left: 10px;">
                  <font face="Verdana" size="4" color="#FFFFFF">
                    <strong>
                      <xsl:choose>
                        <xsl:when test="Contents/Content[@name='title']">
                          <xsl:apply-templates select="Contents/Content[@name='title']" mode="displayContent"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="$currentPage/@name"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </strong>
                  </font>
                </td>
              </tr>
              <tr>
                <td id="msgBody" width="650" style="border:1px solid #FFF;background:#F2F2F2;">
                  <table cellspaciong="0" cellpadding="10" border="0">
                    <tr>
                      <td>
                        <font align="left" face="Verdana" size="1">
                          <xsl:apply-templates select="." mode="mainLayout"/>
                        </font>
                      </td>
                    </tr>
                  </table>
                </td>
              </tr>
              <tr>
                <td id="footer" width="490" align="left" height="98" style="font-family:Arial;font-size:7pt;text-align:left;color:#ffffff;background-color: #333333; padding-left: 10px;">
                  Tel:: 08708 361 755 &#160;&#160;&#160; Email:: <a style="color:#ffffff;font-weight:bold;" href="mailto:info@eonic.co.uk">info@eonic.co.uk</a>&#160;&#160;&#160; Web::
                  <a style="color:#ffffff;font-weight:bold;"  href="www.eonic.co.uk">www.eonic.co.uk</a>
                  <br/>Eonic Ltd | 43 High Street | Tunbridge Wells | Kent | TN1 1XL<br/><br/>
                  Registered in the UK: 031 610 57<br/>
                  <br/>Would you like email stationery like ours? Give us a call.
                </td>
              </tr>
              <tr>
                <td align="center" style="padding:10px;">
                  <font align="center" face="Verdana" size="1">
                    <xsl:apply-templates select="/" mode="optOutStatement">
                      <xsl:with-param name="style">text-align:center;color:#333333;padding:5px;</xsl:with-param>
                    </xsl:apply-templates>
                  </font>
                  <font align="center" face="Verdana" size="0.8">
                    <p style="text-align:center;color: #333;">
                      <xsl:call-template name="eonicDeveloperLink">
                        <xsl:with-param name="style" select="'color:#333;font-size:11px;text-decoration:none;'"/>
                      </xsl:call-template>
                    </p>
                  </font>
                </td>
              </tr>
            </table>
          </center>
        </xsl:if>
        <xsl:if test="@adminMode">
          <xsl:apply-templates select="/" mode="adminFooter"/>
        </xsl:if>
      </body>
    </html>
  </xsl:template>
  <!--   ##########################################################   Content Boxes   ##########################################################   -->
  <!-- -->
  <xsl:template match="/" mode="contentBox">
    <xsl:param name="contentMode"/>
    <xsl:param name="contentName"/>
    <xsl:param name="contentTitle"/>
    <xsl:param name="contentId"/>
    <div class="box" style="height:1%;margin-bottom:20px;">
      <div class="tl" style="background: #333333">
        <div class="tr" style="margin-left:5px;">
          <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
            <xsl:with-param name="type">PlainText</xsl:with-param>
            <xsl:with-param name="text">
              Add <xsl:value-of select="$contentTitle"/>
            </xsl:with-param>
            <xsl:with-param name="name">
              <xsl:value-of select="$contentTitle"/>
            </xsl:with-param>
            <xsl:with-param name="class">title</xsl:with-param>
          </xsl:apply-templates>
          <h3 style="margin:0;font-size:16px;color:#FFF;">
            <xsl:choose>
              <xsl:when test="/Page/Contents/Content[@name=$contentTitle]">
                <xsl:copy-of select="/Page/Contents/Content[@name=$contentTitle]/node()"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:text>&#160;</xsl:text>
              </xsl:otherwise>
            </xsl:choose>
          </h3>
        </div>
      </div>
      <div class="content" style="border:1px solid #333333;padding:10px;">
        <xsl:apply-templates select="/" mode="contentBox_Contents">
          <xsl:with-param name="contentMode" select="$contentMode"/>
          <xsl:with-param name="contentName" select="$contentName"/>
          <xsl:with-param name="contentTitle" select="$contentTitle"/>
          <xsl:with-param name="contentId" select="$contentId"/>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>
  <!-- -->
</xsl:stylesheet>
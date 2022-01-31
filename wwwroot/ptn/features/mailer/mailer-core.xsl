<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
  <xsl:import href="../Email/EmailStationary.xsl"/>
  <xsl:import href="MailerImports.xsl"/>
  
  <xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/>

  <xsl:template match="/">
    <xsl:apply-imports/>
  </xsl:template>

  <xsl:template match="*" mode="subject">
    <xsl:text> </xsl:text>
  </xsl:template>


  <xsl:template match="Page[@adminMode='false']" mode="adminStyle">
    <link type="text/css" rel="stylesheet" href="/ewcommon/css/admin/admin.less"/>

    <!-- IF IE6 BRING IN IE6 files -->
    <xsl:if test="contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 6.0') and not(contains(Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 7')) and not(contains(Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'Opera'))">
      <link type="text/css" rel="stylesheet" href="/ewcommon/css/admin/skins/ie6.css"/>
    </xsl:if>
  </xsl:template>


  <!--   ########################   InlineStyle   ############################   -->

  <xsl:template match="Page" mode="siteStyle">
    <style>
      body{BACKGROUND: #FFF !important;MARGIN: 0px;padding:0;color:#000;font-family:Verdana, Arial, sans-serif;}
      table{border-collapse:collapse;}
      #eonicMainBody{text-align: left;}
      A{color: #c41330;text-decoration:none;}
      A:hover{color: #944}
      p{padding-bottom:0;margin-bottom:10px;margin-top:0;}
      #pagefooter{padding-left:11px;width:474px;}
      #pagefooter p{padding-bottom:0;margin-bottom:7px}
      #pagefooter a{color:#FFF;text-decoration:none;font-weight:bold;}
      .Default-Box h2.title, .Default-Box h2.title a{color:#FFF !important}
      img.alignleft{margin:0 10px 0 0 !important; }
      img.alignright{margin:0 0 0 10px !important;}
    </style>
  </xsl:template>

  <!--   ########################   To Edit the Body Tag   ############################   -->

  <!-- ACTUAL EMAIL TRANSMISSION TEMPLATE -->
  <xsl:template match="Page[not(@adminMode)]" mode="bodyBuilder">

    <xsl:apply-templates select="." mode="emailBody"/>

  </xsl:template>
  
  <xsl:template match="Page[@adminMode='false']" mode="bodyBuilder">
    <body>
      <div class="ewAdmin">
        <xsl:apply-templates select="AdminMenu"/>
      </div>
      <xsl:apply-templates select="." mode="emailBody"/>
      <div class="ewAdmin">
        <xsl:apply-templates select="." mode="adminFooter"/>
      </div>
    </body>
  </xsl:template>

  <xsl:template match="Page[@previewMode]" mode="bodyBuilder">
    <body style="margin:0;padding:0;background-color:#ffffff;font-size:12pt;" bgcolor="#ffffff" class="Email">
      <xsl:apply-templates select="PreviewMenu"/>
      <xsl:apply-templates select="." mode="emailBody"/>
    </body>
  </xsl:template>

  <!--   ########################   Main Email Layout   ############################   -->

  <xsl:template match="Page" mode="bodyLayout">
      <xsl:apply-templates select="." mode="mainLayout"/>
  </xsl:template>

  <xsl:template match="/" mode="contentBox">
    <xsl:param name="contentMode"/>
    <xsl:param name="contentName"/>
    <xsl:param name="contentTitle"/>
    <xsl:param name="contentId"/>
    <div class="box" style="height:1%;margin-bottom:20px;border:3px solid #6b3c8c">
      <div class="tl" style="background:#6b3c8c;">
        <div class="tr" style="background:#6b3c8c;">
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
      <div class="content" style="border:1px solid #006E9C;padding:10px;">
      </div>
    </div>
  </xsl:template>

</xsl:stylesheet>
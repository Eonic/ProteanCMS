<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">

  <!--<xsl:import href="../../../../../ewcommon_v5-1/xsl/Mailer/MailerImports.xsl"/>-->
  <xsl:import href="ResponsiveMailerImports.xsl"/>
  <xsl:import href="../Email/ResponsiveEmailStationery.xsl"/>

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

  <!--   ########################   Admin Only   ############################   -->


  <!-- ACTUAL EMAIL TRANSMISSION TEMPLATE -->
  <xsl:template match="Page[not(@adminMode)]" mode="bodyBuilder">
    <body style="margin:0;padding:0;" >
      <xsl:apply-templates select="." mode="emailBody"/>
    </body>
  </xsl:template>

  <xsl:template match="Page[@adminMode='false']" mode="bodyBuilder">
    <body style="margin:0;padding:0;margin-top:106px!important;" >
      <div class="ewAdmin">
        <xsl:apply-templates select="AdminMenu"/>
      </div>
      <div>
        <xsl:apply-templates select="." mode="emailBody"/>
      </div>
      <div class="ewAdmin">
        <xsl:apply-templates select="." mode="adminFooter"/>
      </div>
    </body>
  </xsl:template>

  <xsl:template match="Page[@previewMode]" mode="bodyBuilder">
    <body style="margin:0;padding:0;" class="Site">
      <xsl:apply-templates select="PreviewMenu"/>
      <xsl:apply-templates select="." mode="emailBody"/>
      <xsl:apply-templates select="." mode="emailStyle"/>
    </body>
  </xsl:template>


  <!--   ########################   Main Email Layout   ############################   -->

  <xsl:template match="Page" mode="bodyLayout">
    <xsl:apply-templates select="." mode="mainLayout"/>
  </xsl:template>

  <!--<xsl:template match="Content[@box='Default Box']" mode="moduleBox">
    <table width="100%" style="width:100%;" border="0" cellpadding="0" cellspacing="0">
      --><!-- define classes for box --><!--
      <xsl:attribute name="class">
        <xsl:text>box </xsl:text>
        <xsl:value-of select="translate(@box,' ','-')"/>
        --><!-- if no title, we may still want TL/TR for rounded boxs with no title bar,
              stled differently to a title bar. --><!--
        <xsl:if test="@title=''">
          <xsl:text> boxnotitle test </xsl:text>
        </xsl:if>
      </xsl:attribute>
      <tr>
        <td width="100%;" style="width:100%;padding:0 {$hPadding}px {$boxMargin}px;">
          <table width="100%" style="width:100%;" border="0" cellpadding="0" cellspacing="0">
            <xsl:if test="/Page/@adminMode">
              <tr>
                <td>
                  <xsl:if test="@rss and @rss!='false'">
                    <xsl:attribute name="colspan">2</xsl:attribute>
                  </xsl:if>
                  <div>
                    <xsl:apply-templates select="." mode="inlinePopupOptions" />
                  </div>
                </td>
              </tr>
            </xsl:if>
            <tr>
              <xsl:if test="@title!=''">
                <td style="background-color:{$BoxBG}; padding:{$emailBoxPad}px {$emailBoxPad}px 0;">
                  <h2 style="padding-top:0;padding-bottom:0;padding-right:0;margin:0px; font-size: 16px;color:{$HxBoxColour}">
                    <xsl:apply-templates select="." mode="moduleLink">
                      <xsl:with-param name="headingColour">
                        <xsl:value-of select="$HxBoxColour"/>
                      </xsl:with-param>
                    </xsl:apply-templates>
                  </h2>
                </td>
              </xsl:if>
              <xsl:if test="@rss and @rss!='false'">
                <td width="20" style="width:20px;" >
                  <xsl:apply-templates select="." mode="rssLink" />
                </td>
              </xsl:if>
            </tr>
            <tr>
              <td width="100%" style="width:100%;padding:{$emailBoxPad}px; color:{$BoxColour};background-color:{$BoxBG}" class="content">
                <xsl:if test="@rss and @rss!='false'">
                  <xsl:attribute name="colspan">2</xsl:attribute>
                </xsl:if>
                <xsl:apply-templates select="." mode="displayBrief"/>
              </td>
            </tr>
            <xsl:if test="@linkText!='' and @link!=''">
              <tr>
                <td width="100%" style="width:100%;padding:0 {$emailBoxPad}px {$emailBoxPad}px;background-color:{$BoxBG}" align="right">
                  <xsl:if test="@rss and @rss!='false'">
                    <xsl:attribute name="colspan">2</xsl:attribute>
                  </xsl:if>
                  <table cellpadding="0" cellspacing="0" >
                    <tr>
                      <td style="white-space:nowrap;background:{$BoxBtnBg};padding:{$btnPad};">
                        <a style="white-space:nowrap; color:{$BoxBtnColour}; font-family:	{$bodyFont}; text-decoration:none;">
                          <xsl:attribute name="href">
                            <xsl:choose>
                              <xsl:when test="format-number(@link,'0')!='NaN'">
                                <xsl:variable name="pageId" select="@link"/>
                                <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]" mode="getHref"/>
                              </xsl:when>
                              <xsl:otherwise>
                                <xsl:value-of select="@link"/>
                              </xsl:otherwise>
                            </xsl:choose>
                          </xsl:attribute>
                          <xsl:attribute name="title">
                            <xsl:value-of select="@title"/>
                          </xsl:attribute>
                          <xsl:value-of select="@linkText"/>
                        </a>
                      </td>
                    </tr>
                  </table>
                </td>
              </tr>
            </xsl:if>
          </table>
        </td>
      </tr>
    </table>
  </xsl:template>-->

  <!--<xsl:template match="/" mode="mailBoxStyles">
    <div data-value="panel-primary">
      <div class="panel panel-primary">
        <div class="panel-heading">
          <h6 class="panel-title">panel-primary</h6>
        </div>
        <div class="panel-body">
          <xsl:text>Example Text </xsl:text>
        </div>
      </div>
    </div>
  </xsl:template>-->
  

  <!--<xsl:template match="p" mode="cleanXhtml">
    <font face="{$bodyFont}" size="{$bodySize}" style="line-height:{$lineHeight}">
      <xsl:element name="{name()}">
        --><!-- process attributes --><!--
        <xsl:for-each select="@*">
          --><!-- remove attribute prefix (if any) --><!--
          <xsl:attribute name="{name()}">
            <xsl:value-of select="." />
          </xsl:attribute>
        </xsl:for-each>
        <xsl:apply-templates mode="cleanXhtml"/>
      </xsl:element>
    </font>
  </xsl:template>-->
</xsl:stylesheet>
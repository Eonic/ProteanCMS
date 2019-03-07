<?xml version="1.0" encoding="UTF-8" ?>
<!-- 
	THIS IS THE EMAIL STATIONARY XSL FOR EONIC
	IT IS NOT A TEMPLATE.
-->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">


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

  <xsl:template match="*" mode="emailStyle">
    <style>
      .Mail p{padding-top:0px; margin-top:0px;}
      .Mail .footer-contact a{color:#fff;}
      .Mail table{margin:0;}
      .Mail a{color:<xsl:value-of select="$LinkColour"/>;text-decoration:none}
      .Mail .thumbnail{border:0;padding:0;border-radius:0;}
      .Mail .Default-Box a{color:<xsl:value-of select="$BoxLink"/>;text-decoration:none}
    </style>
  </xsl:template>

  <xsl:variable name="HeroBG">
    <xsl:value-of select="$currentPage/DisplayName/@hero-background"/>
  </xsl:variable>

  <!--layout-->
  <xsl:variable name="hPadding">15</xsl:variable>
  <xsl:variable name="boxMargin">30</xsl:variable>
  
  <!--backgrounds-->
  <xsl:variable name="MainBG">#ECECEC</xsl:variable>

  <!--headings-->
  <xsl:variable name="HxFont">Arial</xsl:variable>
  <xsl:variable name="HxWeight">Bold</xsl:variable>
  <xsl:variable name="HxColour">#23bec5</xsl:variable>
  <xsl:variable name="HxSize">4</xsl:variable>

  <!--body-->
  <xsl:variable name="bodyFont">Arial</xsl:variable>
  <xsl:variable name="bodyColour">#010101</xsl:variable>
  <xsl:variable name="bodySize">2</xsl:variable>
  <xsl:variable name="bodySizePx">12px</xsl:variable>
  <xsl:variable name="lineHeight">140%</xsl:variable>
  <xsl:variable name="LinkColour">#23bec5</xsl:variable>

  <!--boxes-->
  <xsl:variable name="BoxBG">#23bec5</xsl:variable>
  <xsl:variable name="BoxColour">#ffffff</xsl:variable>
  <xsl:variable name="BoxLink">#a9fbff</xsl:variable>
  <xsl:variable name="BoxBtnBg">#000000</xsl:variable>
  <xsl:variable name="BoxBtnColour">#ffffff</xsl:variable>
  <xsl:variable name="HxBoxColour">#ffffff</xsl:variable>
  <xsl:variable name="emailBoxPad">25</xsl:variable>

  <!--buttons-->
  <xsl:variable name="btnPadV">7px</xsl:variable>
  <xsl:variable name="btnPadH">13px</xsl:variable>
  <xsl:variable name="btnPad">
    <xsl:value-of select="$btnPadV"/>
    <xsl:text> </xsl:text>
    <xsl:value-of select="$btnPadH"/>
  </xsl:variable>
  <xsl:variable name="btnColour">#ffffff</xsl:variable>
  <xsl:variable name="btnBackground">#23bec5</xsl:variable>

  <!--header-->
  <xsl:variable name="HeaderBG">#ffffff</xsl:variable>
  <xsl:variable name="HeaderBorder">5px solid #23bec5</xsl:variable>
  <xsl:variable name="HeaderLink">#23bec5</xsl:variable>

  <!--footer-->
  <xsl:variable name="footerBackground">#000000</xsl:variable>
  <xsl:variable name="footerColour">#999999</xsl:variable>
  <xsl:variable name="footerLink">#ffffff</xsl:variable>

  <xsl:template match="*" mode="unsubscribeMsg">
    <span style="font-size:{$bodySizePx};text-decoration:none;font-family:{$bodyFont};color:{$LinkColour}">
      <webversion>View in your web browser</webversion> | <unsubscribe>Unsubscribe</unsubscribe>
    </span>
  </xsl:template>

  <xsl:template match="*" mode="emailBody">
    <xsl:if test="@adminMode='false'">
      <xsl:attribute name="class">normalMode</xsl:attribute>
    </xsl:if>
    <div id="mainTable" class="Mail" style="margin:0;padding:0;background-color:{$MainBG};">
      <table width="100%" style="background:{$MainBG};margin-bottom:0;" cellpadding="0" cellspacing="0">
        <tr>
          <td style="padding:0;border:0;color:{$bodyColour};font-family:{$bodyFont};font-size:{$bodySizePx};background:{$HeaderBG};border-bottom:{$HeaderBorder}">
            <center>
              <table cellpadding="0" cellspacing="0" width="630" style="width:630px !important;">
                <tr>
                  <td style="padding:0;border:0;">
                    <table cellpadding="0" cellspacing="0" border="0" width="630" align="center" style="padding-top:0;padding-bottom:0;padding-right:0;padding-left:0;margin:0;">
                      <tbody>
                        <tr>
                          <td style="padding:15px {$hPadding}px;border:0;">
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
                          <td style="padding:0 {$hPadding}px 10px;border:0;font-size:17px;vertical-align:middle;text-align:right;color:{$bodyColour};vertical-align:bottom;" align="right">
                            <xsl:apply-templates select="." mode="unsubscribeMsg"/>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </td>
                </tr>
              </table>
            </center>
          </td>
        </tr>
      </table>
      <table width="100%" style="background:{$MainBG};font-family:{$bodyFont};margin-bottom:0;" cellpadding="0" cellspacing="0">
        <tr>
          <td style="padding:0;border:0;">
            <center>
              <table class="Email" cellpadding="0" cellspacing="0" width="630" style="width:630px !important;background:#{$HeroBG};">
                <tr>
                  <td style="padding:25px {$hPadding}px;border:0;">
                    <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
                      <xsl:with-param name="type">Image</xsl:with-param>
                      <xsl:with-param name="text">Add Hero Image</xsl:with-param>
                      <xsl:with-param name="name">Hero</xsl:with-param>
                    </xsl:apply-templates>
                    <xsl:apply-templates select="/Page/Contents/Content[@name='Hero']" mode="displayBrief">
                      <xsl:with-param name="maxWidth" select="'630'"/>
                      <xsl:with-param name="maxHeight" select="'400'"/>
                    </xsl:apply-templates>
                  </td>
                </tr>
              </table>
            </center>
          </td>
        </tr>
      </table>
      <table width="100%" style="background:{$MainBG};font-family:{$bodyFont};margin-bottom:0;" cellpadding="0" cellspacing="0">
        <tr>
          <td style="padding:0;border:0;">
            <center>
              <table cellpadding="0" cellspacing="0" width="630" style="width:630px !important;">
                <tr>
                  <td style="padding:0;border:0;">
                    <!-- mainLayout -->
                    <table cellpadding="0" cellspacing="0" border="0" width="630" align="center" style="padding-top:0;padding-bottom:0;padding-right:0;padding-left:0;">
                      <tbody>
                        <tr>
                          <td style="padding:0 {$hPadding}px">
                            <xsl:apply-templates select="." mode="mainLayout"/>
                          </td>
                        </tr>
                      </tbody>
                    </table>
                  </td>
                </tr>
              </table>
            </center>
          </td>
        </tr>
      </table>
      <table id="footer" width="100%" style="background:{$footerBackground};font-family:{$bodyFont};margin-bottom:0;" cellpadding="0" cellspacing="0">
        <tr>
          <td style="padding:0;border:0;background:{$footerBackground};">
            <center>
              <table id="footer" cellpadding="0" cellspacing="0" width="630" style="width:630px !important;">
                <tr>
                  <td style="padding:20px {$hPadding}px;border:0;font-size:11px;text-align:left;line-height:{$lineHeight}" valign="top">
                    <font color="{$footerColour}">
                      <xsl:if test="$CompanyTel!=''">
                        <strong>Tel:</strong>&#160;<a href="tel:{$CompanyTel}" style="color:{$footerLink};text-decoration:none;">
                          <xsl:value-of select="$CompanyTel"/>
                        </a>&#160;<br/>
                      </xsl:if>
                      <xsl:if test="$CompanyEmail!=''">
                        <strong>Email:</strong>&#160;<a href="mailto:{$CompanyEmail}" style="color:{$footerLink};text-decoration:none;">
                          <xsl:value-of select="$CompanyEmail"/>
                        </a>&#160;<br/>
                      </xsl:if>
                      <xsl:if test="$siteURL!=''">
                        <strong>Website:</strong>&#160;<a href="{$siteURL}" style="color:{$footerLink};text-decoration:none;">
                          <xsl:value-of select="$siteURL"/>
                        </a>&#160;
                      </xsl:if>
                    </font>
                  </td>
                  <td style="padding:20px {$hPadding}px;border:0;font-size:11px;text-align:right;line-height:{$lineHeight}" valign="top">
                    <font color="{$footerColour}">
                      <xsl:if test="$CompanyAddress!=''">
                        <xsl:value-of select="$CompanyAddress"/>
                      </xsl:if>
                      <xsl:if test="$CompanyRegNo!=''">
                        <br/><strong>Registered in UK:</strong>&#160;<xsl:value-of select="$CompanyRegNo"/>
                      </xsl:if>
                      <xsl:if test="$VATnumber!=''">
                        <br/><strong>VAT Number:</strong>&#160;<xsl:value-of select="$VATnumber"/>
                      </xsl:if>
                    </font>
                  </td>
                </tr>
              </table>
            </center>
          </td>
        </tr>
      </table>
    </div>
  </xsl:template>
</xsl:stylesheet>

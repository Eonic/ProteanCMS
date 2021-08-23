<?xml version="1.0" encoding="UTF-8" ?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <!--<xsl:import href="../../../../../ewcommon_v5-1/xsl/Tools/Functions.xsl"/>-->
  <xsl:import href="ResponsiveEmailStyles.xsl"/>
  
  <xsl:template match="*" mode="emailBody">
    <xsl:if test="@adminMode='false'">
      <xsl:attribute name="class">normalMode</xsl:attribute>
    </xsl:if>
    <table width="100%" style="margin-bottom:0;" cellpadding="0" cellspacing="0" id="mainTable" class="Mail">
      <tr>
        <td class="MailTD">
          <table width="100%" cellpadding="0" cellspacing="0" id="emailHeader">
            <tr>
              <td>
                <center>
                  <xsl:comment>
                    <xsl:text>[if mso]&gt;
            &lt;table width="620" cellpadding="0" cellspacing="0"&gt;
              &lt;tr&gt;
                &lt;td&gt;
                  &lt;![endif]</xsl:text>
                  </xsl:comment>
                  <table cellpadding="0" cellspacing="0" style="width:100%;max-width:{$emailWidth}px;margin:0 auto;" class="emailWidthContainer">
                    <tr>
                      <td id="siteLogo" style="padding:10px {$hPadding}px">
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
                      <td align="right" style="padding:10px {$hPadding}px">
                        <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
                          <xsl:with-param name="type">PlainText</xsl:with-param>
                          <xsl:with-param name="text">Add Strapline</xsl:with-param>
                          <xsl:with-param name="name">Strapline</xsl:with-param>
                        </xsl:apply-templates>
                        <xsl:apply-templates select="/Page/Contents/Content[@name='Strapline']" mode="displayBrief"/>
                      </td>
                    </tr>
                  </table>
                  <xsl:comment>
                    <xsl:text>[if mso]&gt;
          &lt;/td&gt;
            &lt;/tr&gt;
          &lt;/table&gt;
          &lt;![endif]</xsl:text>
                  </xsl:comment>
          </center>
        </td>
            </tr>
          </table>
          <table width="100%" style="margin-bottom:0;" cellpadding="0" cellspacing="0">
            <tr>
              <td style="padding:0;border:0;" id="emailContent">
                <center>
                  <xsl:comment>
                    <xsl:text>[if mso]&gt;
                    &lt;table width="620" cellpadding="0" cellspacing="0"&gt;
                    &lt;tr&gt;
                    &lt;td&gt;
                    &lt;![endif]</xsl:text>
                  </xsl:comment>
                  <table cellpadding="0" cellspacing="0" style="width:100%;max-width:{$emailWidth}px;margin:0 auto;" class="emailWidthContainer">
                    <tr>
                      <td class="emailContentWrapper">
                        <xsl:apply-templates select="." mode="mainLayout"/>
                      </td>
                    </tr>
                  </table>
                  <xsl:comment>
                    <xsl:text>[if mso]&gt;
                    &lt;/td&gt;
                    &lt;/tr&gt;
                    &lt;/table&gt;
                    &lt;![endif]</xsl:text>
                  </xsl:comment>
                </center>
              </td>
            </tr>
          </table>
          <table id="emailFooter" width="100%" cellpadding="0" cellspacing="0">
            <tr>
              <td class="emailFooterWrapper">
                <center>
                  <xsl:comment>
                    <xsl:text>[if mso]&gt;
                    &lt;table width="620" cellpadding="0" cellspacing="0"&gt;
                    &lt;tr&gt;
                    &lt;td&gt;
                    &lt;![endif]</xsl:text>
                  </xsl:comment>
                  <table cellpadding="0" cellspacing="0" style="width:100%;max-width:{$emailWidth}px;margin:0 auto;" class="emailWidthContainer">
                    <tr>
                      <td style="padding:0px {$hPadding}px" width="50%" valign="top" class="emailCol">
                        <xsl:if test="$CompanyTel!=''">
                          <strong>Tel: </strong><a href="tel:{$CompanyTel}">
                            <xsl:value-of select="$CompanyTel"/>
                          </a>&#160;<br/>
                        </xsl:if>
                        <xsl:if test="$CompanyEmail!=''">
                          <strong>Email: </strong><a href="mailto:{$CompanyEmail}">
                            <xsl:value-of select="$CompanyEmail"/>
                          </a>&#160;<br/>
                        </xsl:if>
                        <xsl:if test="$siteURL!=''">
                          <strong>Website: </strong><a href="{$siteURL}">
                            <xsl:value-of select="$siteURL"/>
                          </a>&#160;
                        </xsl:if>
                      </td>
                      <td style="padding:0px {$hPadding}px" valign="top" class="emailCol">
                        <xsl:if test="$CompanyAddress!=''">
                          <xsl:value-of select="$CompanyAddress"/>
                        </xsl:if>
                        <xsl:if test="$CompanyRegNo!=''">
                          <br/>
                          <strong>Registered in UK: </strong>
                          <xsl:value-of select="$CompanyRegNo"/>
                        </xsl:if>
                        <xsl:if test="$VATnumber!=''">
                          <br/>
                          <strong>VAT Number: </strong>
                          <xsl:value-of select="$VATnumber"/>
                        </xsl:if>
                        <br/>
                        <unsubscribe>Unsubscribe</unsubscribe>
                      </td>
                    </tr>
                  </table>
                  <xsl:comment>
                    <xsl:text>[if mso]&gt;
                    &lt;/td&gt;
                    &lt;/tr&gt;
                    &lt;/table&gt;
                    &lt;![endif]</xsl:text>
                  </xsl:comment>
                </center>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
            
  </xsl:template>

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
</xsl:stylesheet>

<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:import href="ResponsiveEmailStationeryForms.xsl"/>

  <xsl:template match="/">
    <xsl:apply-imports/>
  </xsl:template>
  <xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/>

  <xsl:template match="*" mode="subject">
    <xsl:value-of select="$siteTitle"/> - <xsl:value-of select="@subjectLine"/>
  </xsl:template>

  <xsl:template match="*" mode="bodyLayout">
    <table cellspacing="0" cellpadding="0">
      <tr>
        <td class="emailModuleHeadingPadding emailPaddingBottom">
          <h2>Email Enquiry</h2>
        </td>
      </tr>
      <tr>
        <td class="emailContentPadding">
          <table cellspacing="0" cellpadding="0" id="emailSummary" summary="Content submitted from website email form">
            <xsl:for-each select="*">
              <tr>
                <th class="emailLabel">
                  <xsl:choose>
                    <xsl:when test="@label and @label!=''">
                      <xsl:value-of select="@label"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="name()"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </th>
                <xsl:choose>
                  <xsl:when test="name()='Email'">
                    <td class="emailAnswer">
                      <a href="mailto:{node()}" title="reply to sender">
                        <xsl:value-of select="node()"/>
                      </a>
                    </td>
                  </xsl:when>
                  <xsl:when test="contains(@type,'date')">
                    <td class="emailAnswer">
                      <xsl:call-template name="DD_Mon_YYYY">
                        <xsl:with-param name="date" select="node()"/>
                      </xsl:call-template>
                    </td>
                  </xsl:when>
                  <xsl:otherwise>
                    <td class="emailAnswer">
                      <xsl:value-of select="node()"/>
                    </td>
                  </xsl:otherwise>
                </xsl:choose>
              </tr>
            </xsl:for-each>
          </table>
        </td>
      </tr>
      <tr>
        <td class="emailContentFooter">
          email sent from:<xsl:value-of select="@sessionReferrer"/>
        </td>
      </tr>
    </table>
  </xsl:template>
</xsl:stylesheet>
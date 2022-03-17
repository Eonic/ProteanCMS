<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <xsl:template match="Content[@type='Module' and @moduleType='FormattedText']" mode="displayBrief">
    <xsl:if test="node()">
      <div class="FormattedText">
        <xsl:if test="@maxWidth!=''">
          <xsl:choose>
            <xsl:when test="@iconStyle='Centre' or @iconStyle='CentreSmall'">
              <xsl:attribute name="class">FormattedText central-text</xsl:attribute>
              <xsl:attribute name='style'>
                <xsl:text>max-width:</xsl:text>
                <xsl:value-of select="@maxWidth"/>
              </xsl:attribute>
            </xsl:when>
            <xsl:otherwise>
              <xsl:attribute name='style'>
                <xsl:text>max-width:</xsl:text>
                <xsl:value-of select="@maxWidth"/>
              </xsl:attribute>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>
        <xsl:apply-templates select="node()" mode="cleanXhtml"/>
      </div>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>
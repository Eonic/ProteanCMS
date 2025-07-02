<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

	<xsl:template match="Content[@type='Module' and @moduleType='accessibilitystatement']" mode="displayBrief">
		<xsl:variable name="div-class">
			<xsl:text>accessibilitystatement</xsl:text>
			<xsl:if test="@position='column1' or @position='custom' or @position='header' or @position='footer'"> character-width-80</xsl:if>
		</xsl:variable>
		<xsl:if test="node()">
			<div class="{$div-class}">
				<xsl:apply-templates select="node()" mode="cleanXhtml"/>
			</div>
		</xsl:if>
    <xsl:if test="node()">
      <div class="{$div-class}">
        <xsl:apply-templates select="node()" mode="cleanXhtml"/>
      </div>
    </xsl:if>
	</xsl:template>

</xsl:stylesheet>
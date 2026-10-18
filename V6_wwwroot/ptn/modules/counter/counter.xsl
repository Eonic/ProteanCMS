<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
 
  <xsl:template match="Content[@type='Module' and @moduleType='Counter']" mode="displayBrief">
		<xsl:variable name="div-class">
			<xsl:text>counter</xsl:text>
		</xsl:variable>
		<xsl:if test="node()">
			<div class="{$div-class}">
        <xsl:if test="@icon-offset and @icon-offset!='' and @iconStyle='Left'">
          <xsl:attribute name="style">
            <xsl:text>padding-left:</xsl:text>
            <xsl:value-of select="@icon-offset"/>
          </xsl:attribute>
        </xsl:if>
				<span class="counter-prefix">
				<xsl:value-of select="@prefix"/>
					</span>
				<span class="counter-number">
				  <xsl:value-of select="@counterValue"/>
				</span>
			<span class="counter-suffix">
				<xsl:value-of select="@suffix"/>
					</span>
			</div>
		</xsl:if>
	</xsl:template>
  
</xsl:stylesheet>
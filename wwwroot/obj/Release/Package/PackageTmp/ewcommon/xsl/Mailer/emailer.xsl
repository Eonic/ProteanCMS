<?xml version="1.0" encoding="UTF-8"?>
	<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:import href="../email/emailStationary.xsl"/>
    
    <xsl:template match="/">
		  <xsl:apply-imports/>
	  </xsl:template>
    
	  <xsl:output method="html" indent="yes"/>

    <xsl:template match="*" mode="subject">
      <xsl:value-of select="Subject/node()" />
    </xsl:template>

    <xsl:template match="*" mode="bodyLayout">
		<div id="mainTable">
		<h2>Email From Website</h2>
      <xsl:for-each select="*">
				<b>
          <xsl:choose>
            <xsl:when test="@label and @label!=''">
              <xsl:value-of select="@label"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="name()"/>
            </xsl:otherwise>
          </xsl:choose>
        </b><xsl:text> - </xsl:text><xsl:value-of select="node()"/><br/>		
			</xsl:for-each>
			</div>
	</xsl:template>
    
</xsl:stylesheet>
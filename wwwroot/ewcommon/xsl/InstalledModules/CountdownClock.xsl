<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
	<xsl:template match="Content[@type='Module' and @contentType='CountdownClock']" mode="displayBrief">
		<xsl:choose>
			<xsl:when test="Content[@type='xform']">
				<xsl:apply-templates select="Content[@type='xform']" mode="xform"/>
			</xsl:when>
		</xsl:choose>
	
	</xsl:template>

	

</xsl:stylesheet>

<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

	<xsl:template match="Content[@type='Module' and @moduleType='EmbeddedHtml' ]" mode="displayBrief">
		<xsl:apply-templates select="*" mode="cleanXhtml"/>
		<xsl:text> </xsl:text>
	</xsl:template>

</xsl:stylesheet>
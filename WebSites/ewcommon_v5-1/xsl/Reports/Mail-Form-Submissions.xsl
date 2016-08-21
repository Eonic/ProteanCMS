<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet">
	<xsl:output method="text" indent="yes" omit-xml-declaration="yes" encoding="utf-8"/>
	<xsl:template match="Page">

		<xsl:for-each select="ContentDetail/Report">
			<xsl:apply-templates select="Item[1]" mode="reportTitles"/>
			<xsl:apply-templates select="Item" mode="reportRow"/>
		</xsl:for-each>
	</xsl:template>
	
	<xsl:template match="*"  mode="reportTitles">
		<xsl:for-each select="descendant-or-self::*">
			<xsl:apply-templates select="." mode="TitleCol"/>
		</xsl:for-each>
		<xsl:text>&#xD;</xsl:text>
	</xsl:template>

	<xsl:template match="*" mode="TitleCol">
		<xsl:if test="count(*)=0">
			<xsl:text>"</xsl:text><xsl:value-of select="local-name()"/><xsl:text>",</xsl:text>
		</xsl:if>
	</xsl:template>

	<xsl:template match="*" mode="reportRow">
		<xsl:for-each select="descendant-or-self::*">
			<xsl:apply-templates select="." mode="reportCol"/>
		</xsl:for-each>
		<xsl:text>&#xD;</xsl:text>
	</xsl:template>

	<xsl:template match="*" mode="reportCol">
		<xsl:if test="count(*)=0">
			<xsl:text>"</xsl:text>
			<xsl:value-of select="node()"/>
			<xsl:text>",</xsl:text>
		</xsl:if>
	</xsl:template>
	
</xsl:stylesheet>

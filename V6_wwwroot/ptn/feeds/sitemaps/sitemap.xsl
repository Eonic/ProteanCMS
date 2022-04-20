<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes">
	<xsl:output method="xml" version="1.0" encoding="utf-8" indent="no"/>
	<xsl:variable name="baseURL">
		<xsl:text>http://</xsl:text>
		<xsl:value-of select="/Page/Request/ServerVariables/Item[@name='SERVER_NAME']"/>
	</xsl:variable>
	<xsl:variable name="today" select="/Page/Request/ServerVariables/Item[@name='Date']"/>

	<xsl:template match="Page">
		
		<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
			<xsl:apply-templates select="Menu/MenuItem" mode="listPages">
				<xsl:with-param name="level" select="10"/>
			</xsl:apply-templates>
		</urlset>
		
	</xsl:template>

	<xsl:template match="MenuItem" mode="listPages">
		<xsl:param name="level"/>
		<xsl:if test="not(contains(@url, 'http'))">
			<url xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
				<loc>
					<xsl:value-of select="$baseURL"/>
					<xsl:value-of select="@url"/>
				</loc>
				<!--<lastmod>
					<xsl:value-of select="$today"/>
				</lastmod>
				<changefreq>weekly</changefreq>-->
				<priority>
					<xsl:choose>
						<xsl:when test="$level=10">
							<xsl:text>1.0</xsl:text>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="$level div 10"/>
						</xsl:otherwise>
					</xsl:choose>
					
				</priority>
			</url>
		</xsl:if>
		<xsl:if test="count(child::MenuItem)&gt;0">
			<xsl:apply-templates select="MenuItem" mode="listPages">
				<xsl:with-param name="level">
					<xsl:choose>
						<xsl:when test="$level=1"><xsl:value-of select="$level"/></xsl:when>
						<xsl:otherwise><xsl:value-of select="$level - 1"/></xsl:otherwise>
					</xsl:choose>
				</xsl:with-param>
			</xsl:apply-templates>
		</xsl:if> 
	</xsl:template>
</xsl:stylesheet> 


<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms"  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"  xmlns:ms="urn:schemas-microsoft-com:xslt">

	<xsl:import href="Report-Base.xsl"/>
	<xsl:import href="Formats/Report-Format-Loader.xsl"/>

	<xsl:template match="/">
		<xsl:apply-imports/>
	</xsl:template>

	 <!--Mail Form Activity Just Gets all the child nodes of cActivityXml - it's not a flat report--> 
	<xsl:template match="Report" mode="reportHeaders">
		<xsl:variable name="activityHeader">
			<Item>
				<xsl:for-each select="Item[1]/descendant::*[local-name()!='cActivityXml' and local-name()!='Items']">
					<xsl:element name="{local-name()}"/>
				</xsl:for-each>
			</Item>
		</xsl:variable>
		<xsl:apply-templates select="ms:node-set($activityHeader)" mode="reportHeaderRow"/>
	</xsl:template>


	 <!--ROW CELL CHOOSER--> 
	<xsl:template match="Item" mode="reportRowCellFilter">
		<xsl:apply-templates select="descendant::*[local-name()!='cActivityXml' and local-name()!='Items']" mode="reportCell"/>
	</xsl:template>
	
</xsl:stylesheet>

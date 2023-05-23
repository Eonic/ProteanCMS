<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms"  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"  xmlns:ms="urn:schemas-microsoft-com:xslt">

	<xsl:import href="Report-Base.xsl"/>
	<xsl:import href="Formats/Report-Format-Loader.xsl"/>

	<xsl:output method="html" indent="no" omit-xml-declaration="yes" encoding="utf-8"/>

	<xsl:template match="/">
		<xsl:apply-imports/>
	</xsl:template>

	 <!--Mail Form Activity Just Gets all the child nodes of cActivityXml - it's not a flat report--> 
	<xsl:template match="Report" mode="reportHeaders">
		<xsl:apply-templates select="Item[1]/DateTime" mode="reportHeader"/>
		<xsl:apply-templates select="Item[1]/cActivityXml/descendant::*" mode="reportHeader"/>
		<xsl:text>&#xD;</xsl:text>
	</xsl:template>


	<!--ROW CELL CHOOSER-->
	<xsl:template match="Item" mode="reportRowCellFilter">
		<xsl:apply-templates select="DateTime" mode="reportCell"/>
		<xsl:apply-templates select="cActivityXml/descendant::*" mode="reportCell"/>
	</xsl:template>
	
	<xsl:template match="Items" mode ="reportHeader">
	
	</xsl:template>
	
	<xsl:template match="Attachment" mode ="reportHeader">	
	
	</xsl:template>
	
	<xsl:template match="Content" mode ="reportHeader">
	
	</xsl:template>
	
	<xsl:template match="Attachments" mode="reportHeader">
		<xsl:text>,"Document Title"</xsl:text>
	</xsl:template>	
	
	<xsl:template match="Items" mode ="reportCell">
		
	
	</xsl:template>

	<xsl:template match="AttachmentIds" mode="reportCell">
		<xsl:text>"</xsl:text><xsl:value-of select="@ids"/><xsl:text>",</xsl:text>
	</xsl:template>
	
		
	<xsl:template match="Attachements | Attachments" mode ="reportCell">
		<xsl:text>"</xsl:text>
		<xsl:for-each select="Attachement | Attachment">
			<xsl:value-of select="Content/@name"/>
		</xsl:for-each>
		<xsl:text>"</xsl:text>
		<xsl:text>,</xsl:text>
	</xsl:template>
	
</xsl:stylesheet>

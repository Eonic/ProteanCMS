<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

	<xsl:import href="../../email/emailStationary.xsl"/>
	<xsl:template match="/">
		<xsl:apply-imports/>
	</xsl:template>

	<xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/>

	<xsl:template match="Items" mode="subject">

		<xsl:for-each select="AttachmentIds/Content">
			<xsl:value-of select="@name"/>
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="Items" mode="bodyLayout">

		<p style="font-family:Verdana,Trebuchet MS,Lusidia Sans Unicode,sans-serif;color:#000;font-size:14px;">
			Thank you for requesting our document; we hope you find it informative and of interest. <br/><br/>
			Please contact us if you require any further details. <br/><br/>
			Best regards<br/><br/>
		</p>

		<h4>You have submitted this information:</h4>
		<xsl:apply-templates select="*[name()!='AttachmentIds']" mode="outputItem"/>
	</xsl:template>

	<xsl:template match="*" mode="outputItem">
		<xsl:if test="node()!=''">
			<p>
				<strong>
					<xsl:apply-templates select="." mode="getLabel"/>
				</strong>
				<xsl:text>: </xsl:text>
				<xsl:value-of select="node()"/>
			</p>
		</xsl:if>
	</xsl:template>

	<xsl:template match="*" mode="getLabel">
		<xsl:choose>
			<xsl:when test="@label and @label!=''">
				<xsl:value-of select="@label"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="name()"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

</xsl:stylesheet>
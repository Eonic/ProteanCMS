<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">

	<!--   ################   Document Download Form   ###############   -->

	<xsl:template match="Content[@type='Module' and @moduleType='documentdownloadxform']" mode="displayBrief">
		<xsl:apply-templates select="." mode="xform"/>
	</xsl:template>

	<xsl:template match="select[@appearance='full' and @class='relatedDocs']" mode="xform_control">

		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<xsl:if test="contains(@class,'selectAll')">
			<span>
				<xsl:attribute name="class">
					<xsl:text>radiocheckbox</xsl:text>
					<xsl:if test="contains(@class,'multiline')">
						<xsl:text> multiline</xsl:text>
					</xsl:if>
				</xsl:attribute>

				<label for="{@ref}_selectAll">
					<input type="checkbox" name="{@ref}_selectAll" id="{@ref}_selectAll" class="selectAll"/>  Select All
				</label>
			</span>
		</xsl:if>

		<xsl:for-each select="ancestor::Content/Content[@type='Document']">
			<span class="radiocheckbox">

				<label for="{@id}_attachementContentIds">
					<input type="checkbox" checked="checked" name="{$ref}" id="{@id}_attachementContentIds" class="selectAll" value="{@id}" />
					<xsl:value-of select="@name"/>
				</label>
				<div class="terminus">&#160;</div>
			</span>
		</xsl:for-each>

		<xsl:for-each select="ancestor::Content[@type='Company']/Content[@type='Application']">
			<span class="radiocheckbox">

				<label for="{@id}_attachementContentIds">
					<input type="checkbox" name="{$ref}" id="{@id}_attachementContentIds" class="selectAll" value="{@id}"/>
					<xsl:value-of select="@name"/>
				</label>
				<div class="terminus">&#160;</div>
			</span>
		</xsl:for-each>



	</xsl:template>

	
</xsl:stylesheet>

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
			<div>
				<xsl:attribute name="class">
					<xsl:text>form-check</xsl:text>
					<xsl:if test="contains(@class,'multiline')">
						<xsl:text> multiline</xsl:text>
					</xsl:if>
				</xsl:attribute>
				<input type="checkbox" name="{@ref}_selectAll" id="{@ref}_selectAll" class="selectAll form-check-input"/>
		
				<label for="{@ref}_selectAll">
					Select All
				</label>
			</div>
		</xsl:if>
		<xsl:for-each select="ancestor::Content/Content[@type='Document']">
			<div class="form-check">
				<input class="selectAll form-check-input"  type="checkbox" checked="checked" name="{$ref}" id="{@id}_attachementContentIds" value="{@id}" />
				
				<label for="{@id}_attachementContentIds" class="form-check-label">
					<!--<xsl:choose>
						<xsl:when test="contains(Path,'.pdf')">
						<i class="fa-regular fa-file-pdf">&#160;</i>&#160;
					</xsl:when>
						<xsl:when test="contains(Path,'.doc')">
						<i class="fa-regular fa-file-word">&#160;</i>&#160;
					</xsl:when>
						<xsl:when test="contains(Path,'.docx')">
							<i class="fa-regular fa-file-word">&#160;</i>&#160;
						</xsl:when>
						<xsl:otherwise>
							<i class="fa-regular fa-file">&#160;</i>&#160;
						</xsl:otherwise>
					</xsl:choose>
					-->
					<xsl:value-of select="@name"/>
				</label>
			</div>
		</xsl:for-each>


	</xsl:template>

	
</xsl:stylesheet>

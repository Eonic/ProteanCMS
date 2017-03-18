<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes">

	<xsl:import href="../Tools/Functions.xsl"/>
	
	<!-- ######################################## INTEGRATION CONTENT POST ########################################### -->

	<!-- 
		Integrations Content Post expects to receive simplified Page XML
		with one item of Content (brief) in the ContentDetail node
		Under the Page element there will be a number of credentials nodes,
		which are iterated through and the Content formatted for each
		
		Output is
		<posts>
		    <post provider="provider" url="url to content">content to post</post>
		</posts>
	-->

	<!-- ############################################## OUTPUT TYPE ################################################# -->

  <xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="no" encoding="UTF-8"/>
  <xsl:template match="/">
    <xsl:apply-imports/>
  </xsl:template>
	
  <!-- ############################################ OUTPUT BUILDER ############################################### -->
	<xsl:template match="Page">
		<posts>
			<xsl:apply-templates select="Credentials"/>
		</posts>
	</xsl:template>

  <!-- ############################################ PROVIDER SPECIFIC OUTPUT ############################################### -->

	<xsl:template match="Credentials">
		<post>
			<xsl:attribute name="provider">
				<xsl:value-of select="@provider"/>
			</xsl:attribute>
			<xsl:attribute name="url">
				<xsl:apply-templates select="/Page/Contents/ContentDetail/Content" mode="url"/>
			</xsl:attribute>
			<xsl:apply-templates select="." mode="content"/>
		</post>
	</xsl:template>

	<xsl:template match="Credentials" mode="content">
		<xsl:apply-templates select="/Page/Contents/ContentDetail/Content"/>
	</xsl:template>


	<!-- ############################################ CONTENT TYPE OUTPUT ############################################### -->
	<!-- Don't include a generic repsonse - this means we have control over what content types are displayed -->
	<xsl:template match="Content"/>

	<xsl:template match="Content[@type='NewsArticle' or @type='BlogArticle' or @type='Event']">
		<xsl:apply-templates select="." mode="getDisplayName"/>
	</xsl:template>


	<!-- ############################################ CONTENT URL GENERATION ############################################### -->
	<xsl:template match="Content" mode="url">
		<xsl:variable name="url">
			<xsl:apply-templates select="." mode="getHref"/>
		</xsl:variable>
		<!-- account for home page -->
		<xsl:if test="starts-with($url,'/')">
			<xsl:choose>
				<xsl:when test="/Page/@baseUrl!=''">
					<xsl:value-of select="/Page/@baseUrl"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>http://</xsl:text>
					<xsl:value-of select="/Page/Request/ServerVariables/Item[@name='HTTP_HOST']"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
		<xsl:value-of select="$url"/>
	</xsl:template>

</xsl:stylesheet>
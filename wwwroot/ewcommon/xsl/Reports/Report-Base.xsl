<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">


  <xsl:import href="../Tools/Functions.xsl"/>
  <xsl:import href="../localisation/SystemTranslations.xsl"/>
  
  <xsl:output method="html" indent="no" omit-xml-declaration="yes" encoding="utf-8"/>

	<!-- 
	
		Reporting Generic Base Handler
		
		
	
	-->
	
	
	<xsl:template match="/">
		<xsl:apply-imports/>
	</xsl:template>

	<!-- PAGE BUILDER -->
	<xsl:template match="Page">
		<xsl:apply-templates select="ContentDetail/Report"/>
	</xsl:template>

	<!-- REPORT BUILDER -->
	<xsl:template match="Report">
		<xsl:apply-templates select="." mode="reportHeaders"/>
		<xsl:apply-templates select="." mode="reportRow"/>
	</xsl:template>

	<xsl:template match="Report" mode="reportHeaders">
		<xsl:apply-templates select="Item[1]" mode="reportHeaderRow"/>
	</xsl:template>

	<xsl:template match="Report" mode="reportRow">
		<xsl:apply-templates select="Item" mode="reportRow"/>
	</xsl:template>

	<!-- HEADER BUILDER -->
	<xsl:template match="Item" mode="reportHeaderRow">
		<xsl:apply-templates select="*" mode="reportHeader"/>
		<xsl:text>&#xD;</xsl:text>
	</xsl:template>

	<xsl:template match="*" mode="reportHeader">
		<xsl:if test="position()!=1">
			<xsl:text>,</xsl:text>
		</xsl:if>
		<xsl:text>"</xsl:text>
		<xsl:apply-templates select="." mode="reportHeaderTitle"/>
		<xsl:text>"</xsl:text>
	</xsl:template>

	<xsl:template match="*" mode="reportHeaderTitle">
		<xsl:choose>
			<xsl:when test="@label">
				<xsl:value-of select="@label"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="translate(local-name(),'_',' ')"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>


	<!-- ROW BUILDER -->
	<xsl:template match="Item" mode="reportRow">
		<xsl:apply-templates select="." mode="reportRowCellFilter"/>
		<xsl:text>&#xD;</xsl:text>
	</xsl:template>


	<!-- ROW CELL CHOOSER -->
	<xsl:template match="Item" mode="reportRowCellFilter">
		<xsl:apply-templates select="*" mode="reportCell"/>
	</xsl:template>

	<!-- CELL BUILDER -->
	<xsl:template match="*" mode="reportCell">
		<xsl:text>"</xsl:text>
		<xsl:apply-templates select="." mode="reportCellValue"/>
		<xsl:text>"</xsl:text>
		<xsl:text>,</xsl:text>
	</xsl:template>

  <!-- CELL BUILDER -->



  
	<!-- CELL VALUE FORMATTER -->
	<xsl:template match="*" mode="reportCellValue">
		<xsl:variable name="name" select="local-name()"/>
		<xsl:variable name="metadata" select="ancestor::Report/Item[1]/*[local-name()=$name]"/>
		<xsl:variable name="datatype" select="$metadata/@datatype"/>
		<xsl:choose>
			<!-- Type Specific Options -->
			<!-- Date -->
			<xsl:when test="contains(name(),'Date') or $datatype='date'">
				<xsl:if test="not(contains(node(),'0001-01-01T00:00:00'))">
					<xsl:variable name="dateFormat">
						<xsl:text>dd MMM yyyy</xsl:text>
						<xsl:if test="not(contains(name(),'NoTime'))">
							<xsl:text> hh:mm</xsl:text>
						</xsl:if>
					</xsl:variable>
					<xsl:call-template name="formatdate">
						<xsl:with-param name="date" select="node()"/>
						<xsl:with-param name="format" select="$dateFormat"/>
					</xsl:call-template>
				</xsl:if>
			</xsl:when>

			<!-- Status -->
			<xsl:when test="name() = 'Status'">
				<xsl:choose>
					<xsl:when test="node()='0'">In-active</xsl:when>
					<xsl:when test="node()='1' or node()='-1' ">Active</xsl:when>
				</xsl:choose>
			</xsl:when>

			<!-- Status -->
			<xsl:when test="name() = 'User' or name() = 'UserXml'">
				<xsl:choose>
					<xsl:when test="FirstName and LastName">
						<xsl:value-of select="LastName"/>, <xsl:value-of select="FirstName"/>
					</xsl:when>
					<xsl:when test="User/FirstName and User/LastName">
						<xsl:value-of select="User/LastName"/>, <xsl:value-of select="User/FirstName"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="node()"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			
			<xsl:otherwise>
				<xsl:value-of select="node()"/>
			</xsl:otherwise>
		</xsl:choose>
		
	</xsl:template>


</xsl:stylesheet>

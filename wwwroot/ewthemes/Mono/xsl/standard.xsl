<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
	<xsl:import href="../../../ewcommon_v4/xsl/Admin/ewAdminV4_0.xsl"/>
	<xsl:import href="../../../ewcommon_v4/xsl/Tools/EwCommonFunctions.xsl"/>
	<xsl:import href="../../../ewcommon_v4/xsl/xForms/xFormsV4_0.xsl"/>
	<xsl:import href="../../../ewcommon_v4/xsl/PageLayouts/EwCommonV4_0.xsl"/>
	<xsl:import href="../../../ewcommon_v4/xsl/Cart/EwCartV4_0.xsl"/>
	<xsl:import href="ewPro_TN.xsl"/>

	<xsl:template match="Page" mode="siteStyle">
		<link rel="stylesheet" type="text/css" href="/css/ewPro_TN.css"/>
		<link rel="stylesheet" type="text/css" href="/css/ewPro_ColourScheme.css"/>
	</xsl:template>
	
	<xsl:template match="Page" mode="siteJs"></xsl:template>

	<xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/>
	<xsl:template match="/">
		<xsl:apply-imports/>
	</xsl:template>
</xsl:stylesheet>
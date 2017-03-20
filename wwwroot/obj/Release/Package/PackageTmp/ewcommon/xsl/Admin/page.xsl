<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">

  <!-- ######################################## IMPORT ALL COMMON XSL's ########################################### -->
  
  <xsl:import href="../Tools/Functions.xsl"/>
  <xsl:import href="../xForms/xForms-bs-mininal.xsl"/>
  <xsl:import href="Admin.xsl"/>
  <xsl:import href="AdminXForms.xsl"/>
  <xsl:import href="../localisation/SystemTranslations.xsl"/>

  <xsl:template name="initialiseSocialBookmarks"></xsl:template>
  <xsl:template match="Page" mode="googleMapJS"></xsl:template>

  <xsl:output method="html" indent="yes" standalone="yes" omit-xml-declaration="yes" encoding="UTF-8"/>

  <xsl:template match="/">
    <xsl:text disable-output-escaping='yes'>&lt;!DOCTYPE html></xsl:text>
    <xsl:apply-imports/>
  </xsl:template>

  <xsl:variable name="theme">
    <xsl:value-of select="/Page/Settings/add[@key='theme.CurrentTheme']/@value"/>
  </xsl:variable>
  
  <xsl:template match="Page" mode="siteStyle">
    <xsl:if test="$theme!=''">
      <xsl:call-template name="bundle-css">
        <xsl:with-param name="comma-separated-files">
          <xsl:text>/ewThemes/</xsl:text>
          <xsl:value-of select="$theme"/>
          <xsl:text>/css/bootstrapBase.less</xsl:text>
        </xsl:with-param>
        <xsl:with-param name="bundle-path">
          <xsl:text>~/Bundles/</xsl:text>
          <xsl:value-of select="$theme"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
  
</xsl:stylesheet>
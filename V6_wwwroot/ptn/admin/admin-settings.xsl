<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                  xmlns:v-bind="http://example.com/xml/v-bind" xmlns:v-on="http://example.com/xml/v-on"
                  xmlns:v-for="http://example.com/xml/v-for" xmlns:v-slot="http://example.com/xml/v-slot"
                  xmlns:v-if="http://example.com/xml/v-if" xmlns:v-else="http://example.com/xml/v-else"
                  xmlns:v-model="http://example.com/xml/v-model">



  <xsl:template name="proteanProductName">
    <xsl:choose>
      <xsl:when test="$page/Settings/add[@key='web.proteanProductName']/@value!=''">
        <xsl:value-of select="$page/Settings/add[@key='web.proteanProductName']/@value"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Protean</xsl:text>
        <strong>CMS</strong>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="proteanCMSName">
    <xsl:choose>
      <xsl:when test="$page/Settings/add[@key='web.proteanCMSName']/@value!=''">
        <xsl:value-of select="$page/Settings/add[@key='web.proteanCMSName']/@value"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="proteanProductName"/>
        <xsl:text> - Content Management System</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="proteanAdminSystemName">
    <xsl:choose>
      <xsl:when test="$page/Settings/add[@key='web.proteanAdminSystemName']/@value!=''">
        <xsl:value-of select="$page/Settings/add[@key='web.proteanAdminSystemName']/@value"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="proteanProductName"/>
        <xsl:text> admin system</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="proteanCopyright">
    <xsl:choose>
      <xsl:when test="$page/Settings/add[@key='web.proteanCopyright']/@value!=''">
        <xsl:value-of select="$page/Settings/add[@key='web.proteanCopyright']/@value"/>
      </xsl:when>
      <xsl:otherwise>
        Eonic Digital LLP.
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="proteanSupportTelephone">
    <xsl:choose>
      <xsl:when test="$page/Settings/add[@key='web.proteanSupportTelephone']/@value!=''">
        <xsl:value-of select="$page/Settings/add[@key='web.proteanSupportTelephone']/@value"/>
      </xsl:when>
      <xsl:otherwise>
		  +44 (0)1273 761 586
	  </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="proteanWebsite">
    <xsl:choose>
      <xsl:when test="$page/Settings/add[@key='web.proteanWebsite']/@value!=''">
        <xsl:value-of select="$page/Settings/add[@key='web.proteanWebsite']/@value"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>eonic.digital</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="proteanSupportEmail">
    <xsl:choose>
      <xsl:when test="$page/Settings/add[@key='web.proteanSupportEmail']/@value!=''">
        <xsl:value-of select="$page/Settings/add[@key='web.proteanSupportEmail']/@value"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>support@eonic.digital</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="proteanLogo">
    <xsl:choose>
      <xsl:when test="$page/Settings/add[@key='web.proteanLogo']/@value!=''">
        <xsl:value-of select="$page/Settings/add[@key='web.proteanLogo']/@value"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>/ptn/admin/skin/images/ptn-logo.png</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- Used across this xsl to generate Admin menus and Breadcrumbs-->
  <xsl:variable name="subMenuCommand">
    <xsl:choose>
      <xsl:when test="/Page/@ewCmd!=''">
        <xsl:value-of select="/Page/@ewCmd"/>
        <xsl:text> </xsl:text>
      </xsl:when>
      <xsl:otherwise>Don't search for this</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

</xsl:stylesheet>

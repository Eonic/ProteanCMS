<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                  xmlns:v-bind="http://example.com/xml/v-bind" xmlns:v-on="http://example.com/xml/v-on"
                  xmlns:v-for="http://example.com/xml/v-for" xmlns:v-slot="http://example.com/xml/v-slot"
                  xmlns:v-if="http://example.com/xml/v-if" xmlns:v-else="http://example.com/xml/v-else"
                  xmlns:v-model="http://example.com/xml/v-model">



  <xsl:template name="eonicwebProductName">
    <xsl:choose>
      <xsl:when test="$page/Settings/add[@key='web.eonicwebProductName']/@value!=''">
        <xsl:value-of select="$page/Settings/add[@key='web.eonicwebProductName']/@value"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Protean</xsl:text>
        <strong>CMS</strong>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="eonicwebCMSName">
    <xsl:choose>
      <xsl:when test="$page/Settings/add[@key='web.eonicwebCMSName']/@value!=''">
        <xsl:value-of select="$page/Settings/add[@key='web.eonicwebCMSName']/@value"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="eonicwebProductName"/>
        <xsl:text> - Content Management System</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="eonicwebAdminSystemName">
    <xsl:choose>
      <xsl:when test="$page/Settings/add[@key='web.eonicwebAdminSystemName']/@value!=''">
        <xsl:value-of select="$page/Settings/add[@key='web.eonicwebAdminSystemName']/@value"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="eonicwebProductName"/>
        <xsl:text> admin system</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="eonicwebCopyright">
    <xsl:choose>
      <xsl:when test="$page/Settings/add[@key='web.eonicwebCopyright']/@value!=''">
        <xsl:value-of select="$page/Settings/add[@key='web.eonicwebCopyright']/@value"/>
      </xsl:when>
      <xsl:otherwise>
        Eonic Digital LLP.
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="eonicwebSupportTelephone">
    <xsl:choose>
      <xsl:when test="$page/Settings/add[@key='web.eonicwebSupportTelephone']/@value!=''">
        <xsl:value-of select="$page/Settings/add[@key='web.eonicwebSupportTelephone']/@value"/>
      </xsl:when>
      <xsl:otherwise>
        +44 (0)1892 534044
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="eonicwebWebsite">
    <xsl:choose>
      <xsl:when test="$page/Settings/add[@key='web.eonicwebWebsite']/@value!=''">
        <xsl:value-of select="$page/Settings/add[@key='web.eonicwebWebsite']/@value"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>eonic.com</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="eonicwebSupportEmail">
    <xsl:choose>
      <xsl:when test="$page/Settings/add[@key='web.eonicwebSupportEmail']/@value!=''">
        <xsl:value-of select="$page/Settings/add[@key='web.eonicwebSupportEmail']/@value"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>support@eonic.co.uk</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="eonicwebLogo">
    <xsl:choose>
      <xsl:when test="$page/Settings/add[@key='web.eonicwebLogo']/@value!=''">
        <xsl:value-of select="$page/Settings/add[@key='web.eonicwebLogo']/@value"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>/ewcommon/images/admin/skin/protean-admin-white.png</xsl:text>
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

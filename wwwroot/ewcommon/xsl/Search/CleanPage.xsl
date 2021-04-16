<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes">
  <xsl:import href="../Tools/Functions.xsl"/>
  <xsl:import href="../PageLayouts/CommonLayouts.xsl"/>
  <xsl:import href="../Admin/Admin.xsl"/>
  <xsl:import href="../Search/Search.xsl"/>
  <xsl:import href="../localisation/SystemTranslations.xsl"/>

  <xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" encoding="UTF-8"/>

  <xsl:template match="/">
    <xsl:apply-imports/>
  </xsl:template>

  <xsl:template match="Page">
    <html>
      <head>
        <title>
          <xsl:apply-templates select="." mode="pagetitle"/>
        </title>
        <xsl:if test="/Page/@baseUrl">
          <base href="{/Page/@baseUrl}"/>
        </xsl:if>
        <xsl:apply-templates select="." mode="metadata"/>
        <xsl:apply-templates select="." mode="pageurl"/>
      </head>
      <xsl:apply-templates select="." mode="bodyBuilder"/>
    </html>
  </xsl:template>

  <xsl:template match="Page" mode="pageurl"></xsl:template>

  <xsl:template match="Page[ContentDetail]" mode="pageurl">
    <xsl:variable name="href">
      <xsl:apply-templates select="ContentDetail/Content" mode="getHref"/>
    </xsl:variable>
    <link rel="canonical" href="{$href}" />
  </xsl:template>

  <xsl:template match="Page" mode="pagetitle">
    <xsl:choose>
      <xsl:when test="//ContentDetail">
        <xsl:apply-templates select="//ContentDetail/Content" mode="getDisplayName"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]" mode="getDisplayName"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!-- NORMAL PAGE META TAGS -->
  <xsl:template match="Page" mode="metadata">
    <meta name="pgid" content="{@id}"/>
    <meta name="status" content="{@status}"/>
    <meta name="contenttype" content="page"/>
    <meta name="name">
      <xsl:attribute name="content">
        <xsl:apply-templates select="." mode="pagetitle"/>
      </xsl:attribute>
    </meta>
    <meta name="abstract">
      <xsl:attribute name="content">
        <xsl:variable name="content">
          <xsl:for-each select="/Page/Contents/Content[@moduleType='FormattedText' and @parId!='1']">
            <xsl:apply-templates select="*" mode="getValues"/>
          </xsl:for-each>
        </xsl:variable>
        <xsl:call-template name="truncateString">
          <xsl:with-param name="string" select="$content"/>
          <xsl:with-param name="length" select="'500'"/>
        </xsl:call-template>
      </xsl:attribute>
    </meta>
    <meta name="publishDate" content="{/Page/Menu/MenuItem/descendant-or-self::MenuItem[@id=/Page/@id]/@publish}"/>
    <xsl:if test="Contents/Content[@name='MetaDescription' or @name='metaDescription']">
      <meta name="description" content="{Contents/Content[@name='MetaDescription' or @name='metaDescription']}{Content[@name='MetaDescription-Specific']}"/>
    </xsl:if>
    <xsl:if test="Contents/Content[@name='MetaKeywords' or @name='metaKeywords']">
      <meta name="keywords" content="{Contents/Content[@name='MetaKeywords' or @name='metaKeywords']}{Contents/Content[@name='MetaKeywords-Specific']}"/>
    </xsl:if>
    <xsl:if test="Contents/Content[@name='MetaAbstract' or @name='metaAbstract']">
      <meta name="abstract" content="{Contents/Content[@name='MetaAbstract' or @name='metaAbstract']}"/>
    </xsl:if>
  </xsl:template>

  <!-- DETAIL PAGE -->
  <xsl:template match="Page[@artid and @artid!='']" mode="metadata">
    <meta name="pgid" content="{@id}"/>
    <meta name="artid" content="{@artid}"/>
    <meta name="status" content="{@status}"/>
    <meta name="contenttype" content="{//ContentDetail/Content/@type}"/>
    <meta name="name">
      <xsl:attribute name="content">
        <xsl:apply-templates select="//ContentDetail/Content" mode="getDisplayName"/>
      </xsl:attribute>
    </meta>
    <meta name="abstract">
      <xsl:attribute name="content">
        <xsl:variable name="content">
          <xsl:for-each select="ContentDetail/Content">
            <xsl:apply-templates select="*" mode="getValues"/>
          </xsl:for-each>
        </xsl:variable>
        <xsl:call-template name="truncateString">
          <xsl:with-param name="string" select="$content"/>
          <xsl:with-param name="length" select="'500'"/>
        </xsl:call-template>
      </xsl:attribute>
    </meta>

    <xsl:if test="not(//ContentDetail/Content[@parId=/Page/@id])">
      <meta name="ROBOTS" content="NOINDEX, NOFOLLOW"/>
    </xsl:if>
    <xsl:if test="ContentDetail/Content[@type='Module' or @type='Link']">
      <meta name="ROBOTS" content="NOINDEX, NOFOLLOW"/>
    </xsl:if>
  </xsl:template>

  <xsl:template match="*" mode="getValues">
    <xsl:value-of select="."/>
    <xsl:text> </xsl:text>
    <xsl:if test="*">
      <xsl:text> </xsl:text>
      <xsl:apply-templates select="*" mode="getValues" />
    </xsl:if>
  </xsl:template>

  <xsl:template match="PublishDate | StartDate | EndDate" mode="getValues">
    <xsl:call-template name="DD_Mon_YYYY">
      <xsl:with-param name="date" select="node()"/>
    </xsl:call-template>
    <xsl:text> </xsl:text>
    <xsl:if test="*">
      <xsl:apply-templates select="*" mode="getValues" />
    </xsl:if>
  </xsl:template>

  <xsl:template match="Page" mode="siteStyle"></xsl:template>


  <xsl:template match="Page" mode="bodyBuilder">
    <body>
      <xsl:apply-templates select="." mode="bodyDisplay"/>
    </body>
  </xsl:template>
  <!-- -->
  <xsl:template match="Page" mode="bodyDisplay">
    <h1>
      <xsl:apply-templates select="." mode="pagetitle"/>
    </h1>
    <xsl:apply-templates select="." mode="mainLayout"/>
  </xsl:template>
  <!-- -->
  <xsl:template match="Page" mode="Layout">
    <div>
      <xsl:for-each select="/Page/Contents/Content[@moduleType='FormattedText']">
        <xsl:apply-templates select="." mode="displayModule"/>
      </xsl:for-each>
    </div>
  </xsl:template>

  <xsl:template name="getSiteURL"/>
  <xsl:template match="Content[@type='xform']" mode="displayBrief"></xsl:template>
  <xsl:template match="Content[@type='xform']" mode="xform"></xsl:template>
  <xsl:template match="div[@type='xform']" mode="xform"></xsl:template>
  <xsl:template match="Content | MenuItem" mode="displayThumbnail"></xsl:template>
  <xsl:template match="Content | MenuItem" mode="displayDetailImage"></xsl:template>
  <xsl:template match="*" mode="backLink"></xsl:template>
</xsl:stylesheet>
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" 
  exclude-result-prefixes="#default ms dt ew" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"  
  xmlns:ms="urn:schemas-microsoft-com:xslt" 
  xmlns:dt="urn:schemas-microsoft-com:datatypes" 
  xmlns:date= "http://exslt.org/dates-and-times"  
  extension-element-prefixes="date" 
  xmlns:atom="http://www.w3.org/2005/Atom" 
  xmlns:ew="urn:ew">
  
  <xsl:import href="../../../../ewcommon_v5-1/xsl/tools/Functions.xsl"/>
  
  <xsl:output method="xml" omit-xml-declaration="no" indent="yes" cdata-section-elements="description content"/>
  
  <xsl:variable name="siteURL" select="concat('http://',/Page/Request/ServerVariables/Item[@name='SERVER_NAME'])"/>
  <xsl:variable name="siteName">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'SiteName'"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="contentType" select="/Page/Request/QueryString/Item[@name='contentSchema']" />
  <xsl:variable name="moduleId" select="/Page/Request/QueryString/Item[@name='moduleId']" />
  <xsl:variable name="module" select="/Page/Contents/Content[@id=$moduleId]" />
  <xsl:variable name="feedHref">
    <xsl:choose>
      <xsl:when test="$moduleId!=''">
        <xsl:apply-templates select="$module" mode="getRssHref"/>
      </xsl:when>
      <xsl:otherwise>
          <xsl:value-of select="$siteURL"/>
          <xsl:text>/ewcommon/feeds/generic/feed.ashx</xsl:text>
          <xsl:text>?contentSchema=</xsl:text>
          <xsl:value-of select="$contentType"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- FEED SETTINGS -->
  <xsl:template match="Page">
    <rss version="2.0">
      <channel>
        <title>
          <xsl:choose>
            <xsl:when test="$moduleId!=''">
              <xsl:apply-templates select="$module" mode="channelTitle"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:apply-templates select="/Page/Contents" mode="channelTitle"/>
            </xsl:otherwise>
          </xsl:choose>
        </title>
        <link>
          <xsl:value-of select="$siteURL"/>
        </link>
        <atom:link href="{$feedHref}" rel="self" type="application/rss+xml" />
        <managingEditor>
          <xsl:call-template name="getSettings">
            <xsl:with-param name="sectionName" select="'web'"/>
            <xsl:with-param name="valueName" select="'SiteAdminEmail'"/>
          </xsl:call-template>
          <xsl:text> (</xsl:text>
          <xsl:value-of select="$siteName" />
          <xsl:text>)</xsl:text>
        </managingEditor>
        <webMaster>
          <xsl:text>support@eonic.co.uk (eonicweb support)</xsl:text>
        </webMaster>
        <generator>eonicweb5</generator>
        <language>
          <xsl:value-of select="/Page/@translang"/>
        </language>
        <description>
          <xsl:choose>
            <xsl:when test="$moduleId!=''">
              <xsl:apply-templates select="$module" mode="channelTitle"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:apply-templates select="/Page/Contents" mode="channelTitle"/>
            </xsl:otherwise>
          </xsl:choose>
        </description>
        
        <xsl:choose>
          <xsl:when test="$moduleId!=''">
            <xsl:apply-templates select="$module" mode="channelItems"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="/Page/Contents" mode="channelItems"/>
          </xsl:otherwise>
        </xsl:choose>
      </channel>
    </rss>
  </xsl:template>

  <!-- Channel Title for Module Only Content -->
  <xsl:template match="Content" mode="channelTitle">
    <xsl:value-of select="$siteName"/>
    
    <xsl:choose>
      <xsl:when test="@rss='this' and @title!=''">
        <xsl:text> - </xsl:text>
        <xsl:value-of select="@title"/>
      </xsl:when>
      <xsl:when test="@rss='all'">
        <xsl:text> - </xsl:text>
        <xsl:value-of select="@contentType" />
        <xsl:text>'s</xsl:text>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- Channel Title for ALL Content -->
  <xsl:template match="Contents" mode="channelTitle">
    <xsl:value-of select="$siteName"/>
    <xsl:text> - </xsl:text>
    <xsl:text>All </xsl:text>
    <xsl:choose>
      <xsl:when test="$contentType='NewsArticle'">News</xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$contentType"/>
        <xsl:text>s</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
    
  </xsl:template>

  <!-- LIST ONLY MODULE CONTENT -->
  <xsl:template match="Content[@type='Module']" mode="channelItems">
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <!-- List Content in order dictated by the Module -->
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:apply-templates select="ms:node-set($contentList)/*" mode="contentItem"/>
  </xsl:template>


  <!-- LIST ALL CONTENT -->
  <xsl:template match="Contents" mode="channelItems">
    <xsl:for-each select="Content[@type=$contentType]">
      <xsl:sort select="translate(@publish,'-','')" order="descending" data-type="number"/>
      <xsl:sort select="translate(@update,'-','')" order="descending" data-type="number"/>
      <!-- although should be all, over 1000 is too slow, 600 loads in about 6s.-->
      <xsl:if test="position()&lt;=600">
        <xsl:apply-templates select="." mode="contentItem"/>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>

  <!-- Standard Content Item -->
  <xsl:template match="Content" mode="contentItem">

    <item>
      <guid isPermaLink="false">
        <!-- http://validator.w3.org/feed/docs/error/InvalidHttpGUID.html -->
        <xsl:value-of select="@id"/>
      </guid>
      <title>
        <xsl:apply-templates select="." mode="getDisplayName"/>
      </title>
      <link>
        <xsl:apply-templates select="." mode="getHref"/>
      </link>
      <description>
        <xsl:variable name="description">
          <xsl:apply-templates select="." mode="buildDescription"/>
        </xsl:variable>
        <xsl:apply-templates select="ms:node-set($description)/*" mode="encodeXhtml" />
      </description>
      <pubDate>
        <xsl:apply-templates select="." mode="getPublishDate" />
      </pubDate>
    </item>
  </xsl:template>

  <!-- Add site URL to Images -->
  <xsl:template match="img | *[name()='img']" mode="encodeXhtml">
    <xsl:text>&lt;</xsl:text>
    <xsl:value-of select="name()"/>
    <xsl:for-each select="@*[name()!='xmlns']">
      <xsl:text>&#160;</xsl:text>
      <xsl:value-of select="name()"/>
      <xsl:text>="</xsl:text>
      <xsl:if test="name()='src'">
        <xsl:value-of select="$siteURL"/>
      </xsl:if>
      <xsl:value-of select="." />
      <xsl:text>"</xsl:text>
    </xsl:for-each>
    <xsl:text>/&gt;</xsl:text>
  </xsl:template>

  <!-- Get Publish Date, can be overwritten and changed. -->
  <xsl:template match="Content" mode="getPublishDate">
    <!--<xsl:value-of select="substring(@publish,1,10)"/>-->
    <xsl:call-template name="date-RFC822">
      <xsl:with-param name="date" select="@publish" />
    </xsl:call-template>
  </xsl:template>

  <!-- Generic Description/Strapline logic -->
  <xsl:template match="Content" mode="buildDescription">
    <div>
      <xsl:variable name="image">
        <xsl:apply-templates select="." mode="displayThumbnail"/>
      </xsl:variable>
      <xsl:apply-templates select="ms:node-set($image)/*" mode="encodeXhtml" />
      <xsl:choose>
        <xsl:when test="Strapline/node()">
          <xsl:apply-templates select="Strapline/node()" mode="encodeXhtml" />
        </xsl:when>
        <xsl:when test="Strap/node()">
          <xsl:apply-templates select="Strap/node()" mode="encodeXhtml" />
        </xsl:when>
        <xsl:when test="Introduction/node()">
          <xsl:apply-templates select="Introduction/node()" mode="encodeXhtml" />
        </xsl:when>
        <xsl:when test="Summary/node()">
          <xsl:apply-templates select="Summary/node()" mode="encodeXhtml" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="Body/node()" mode="encodeXhtml" />
        </xsl:otherwise>
      </xsl:choose>
    </div>
  </xsl:template>

  <!-- Build URL to CONTENT -->
  <xsl:template match="Content" mode="getHref">
    <xsl:param name="parId"/>
    <xsl:param name="itemMode"/>
    <!-- WHEN currentPageDetail TRUE, the detail can display on the Current page instead of jumping to the Parent page-->
    <xsl:param name="currentPageDetail"/>
    <xsl:variable name="contentParId">
      <xsl:choose>
        <xsl:when test="$parId!=''">
          <xsl:value-of select="$parId"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@parId"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- Get the correct page URL, Current or Parent -->
    <xsl:choose>
      <xsl:when test="$currentPageDetail='true'">
        <xsl:apply-templates select="$menu/MenuItem/descendant-or-self::MenuItem[@id=/Page/@id]" mode="getHref"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="$menu/descendant-or-self::MenuItem[@id=$contentParId]/@url='/'">
            <xsl:value-of select="$siteURL"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="$menu/descendant-or-self::MenuItem[@id=$contentParId]" mode="getHref"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>

    <!-- Get the correct content detail call -->
    <xsl:variable name="safeURLName">
      <xsl:apply-templates select="." mode="getSafeURLName"/>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="/Page/@pageExt and /Page/@pageExt!=''">
        <!-- in format = '/{@id}-/{@name}' -->
        <xsl:text>/</xsl:text>
        <xsl:value-of select="@id"/>
        <xsl:text>~</xsl:text>
        <xsl:value-of select="translate($safeURLName,' ','+')"/>
        <xsl:value-of select="/Page/@pageExt"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="$itemMode='artid' or (contains($menu/descendant-or-self::MenuItem[@id=$parId]/@url,'?pgid') or $adminMode)">
            <xsl:text>&amp;artid=</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:choose>
              <xsl:when test="$DescriptiveContentURLs='true'">
                <xsl:text>/</xsl:text>
                <xsl:value-of select="@id"/>
                <xsl:text>-/</xsl:text>
                <xsl:value-of select="$safeURLName"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:text>/item</xsl:text>
                <xsl:value-of select="@id"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  
  <!-- BUILD ANCHOR/HYPERLINK to a PAGE -->
  <xsl:template match="MenuItem" mode="getHref">
    <xsl:choose>
      <xsl:when test="@url!=''">
        <xsl:choose>
          
          <xsl:when test="contains(@url,'http')">
            <xsl:value-of select="@url"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$siteURL"/>
            <xsl:value-of select="@url"/>
            <xsl:value-of select="/Page/@pageExt"/>
            <xsl:if test="/Page/@adminMode and /Page/@pageExt!=''">
              <xsl:text>?pgid=</xsl:text>
              <xsl:value-of select="@id"/>
            </xsl:if>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$siteURL"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
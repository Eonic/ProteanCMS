<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">


  <!-- ## IMAGE BASIC CONTENT TYPE - Cater for a link  ###############################################   -->
  <xsl:template match="Content[@type='Image']" mode="displayBrief">
    <xsl:param name="maxWidth"/>
    <xsl:param name="maxHeight"/>
    <xsl:param name="noLazy"/>
    <xsl:choose>
      <xsl:when test="@internalLink!=''">
        <xsl:variable name="pageId" select="@internalLink"/>
        <xsl:variable name="href">
          <xsl:apply-templates select="//MenuItem[@id=$pageId]" mode="getHref" />
        </xsl:variable>
        <xsl:variable name="title">
          <xsl:apply-templates select="//MenuItem[@id=$pageId]" mode="getTitleAttr" />
        </xsl:variable>

        <a href="{$href}" title="{$title}">
          <xsl:choose>
            <xsl:when test="img[contains(@src,'.svg')]">
              <svg id="svg-{@position}" width="{img/@width}" height="{img/@height}" viewbox="0 0 {img/@width} {img/@height}" xmlns="http://www.w3.org/2000/svg" xmlns:ev="http://www.w3.org/2001/xml-events" xmlns:xlink="http://www.w3.org/1999/xlink">
                <image id="svg-img-{@position}" xlink:href="{img/@src}" src="{@svgFallback}" width="{img/@width}" height="{img/@height}" class="img-responsive">
                  <xsl:text> </xsl:text>
                </image>
              </svg>
            </xsl:when>
            <xsl:when test="@resize='true'">
              <xsl:apply-templates select="." mode="resize-image">
                <xsl:with-param name="noLazy" select="$noLazy"/>
              </xsl:apply-templates>
            </xsl:when>
            <xsl:when test="$maxWidth!='' or $maxHeight!=''">
              <xsl:apply-templates select="." mode="resize-image">
                <xsl:with-param name="maxWidth" select="$maxWidth"/>
                <xsl:with-param name="maxHeight" select="$maxHeight"/>
                <xsl:with-param name="noLazy" select="$noLazy"/>
              </xsl:apply-templates>
            </xsl:when>
            <xsl:otherwise>
              <xsl:apply-templates select="./node()" mode="cleanXhtml"/>
            </xsl:otherwise>
          </xsl:choose>
        </a>
      </xsl:when>
      <xsl:when test="@externalLink!=''">
        <a href="{@externalLink}" title="Go to {@externalLink}">
          <xsl:if test="not(contains(@externalLink,/Page/Request/ServerVariables/Item[@name='SERVER_NAME']/node())) and contains(@externalLink,'http')">
            <xsl:attribute name="rel">external</xsl:attribute>
            <!-- All browsers open rel externals as new windows anyway. Target not a valid attribute -->
          </xsl:if>
          <xsl:apply-templates select="./node()" mode="cleanXhtml">
            <xsl:with-param name="noLazy" select="'true'"/>
          </xsl:apply-templates>
        </a>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="node()" mode="cleanXhtml"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>



  <xsl:template match="Content[@type='Module' and @moduleType='Image']" mode="moduleTitle">
    <xsl:value-of select="@title"/>
  </xsl:template>

  <xsl:template match="Content[@moduleType='Image']" mode="displayBrief">
    <xsl:param name="maxWidth"/>
    <xsl:param name="maxHeight"/>
    <xsl:apply-templates select="." mode="displayBriefImg">
      <xsl:with-param name="maxWidth" select="$maxWidth"/>
      <xsl:with-param name="maxHeight" select="$maxHeight"/>
    </xsl:apply-templates>
  </xsl:template>

  <xsl:template match="Content[@moduleType='Image']" mode="displayBriefImg">
    <xsl:param name="maxWidth"/>
    <xsl:param name="maxHeight"/>
    <xsl:variable name="crop">
      <xsl:choose>
        <xsl:when test="@crop='true'">true</xsl:when>
        <xsl:otherwise>false</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="no-stretch">
      <xsl:choose>
        <xsl:when test="@stretch='true'">false</xsl:when>
        <xsl:otherwise>true</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test="img/@src!=''">
      <xsl:choose>
        <xsl:when test="@resize='true'">
          <xsl:apply-templates select="." mode="resize-image">
            <xsl:with-param name="crop" select="$crop"/>
            <xsl:with-param name="no-stretch" select="$no-stretch"/>
          </xsl:apply-templates>
        </xsl:when>
        <xsl:when test="$maxWidth!='' or $maxHeight!=''">
          <xsl:apply-templates select="." mode="resize-image">
            <xsl:with-param name="maxWidth" select="$maxWidth"/>
            <xsl:with-param name="maxHeight" select="$maxHeight"/>
            <xsl:with-param name="crop" select="$crop"/>
          </xsl:apply-templates>
        </xsl:when>
        <xsl:when test="(@imgDetail and @imgDetail!='') or @lightbox='true'">
          <xsl:choose>
            <xsl:when test="@imgDetail and @imgDetail!=''">
              <a href="{@imgDetail}" title="{@title}" class="responsive-lightbox">
                <xsl:apply-templates select="node()" mode="cleanXhtml"/>
              </a>
            </xsl:when>
            <xsl:otherwise>
              <a href="{node()/@src}" title="{@title}" class="responsive-lightbox">
                <xsl:apply-templates select="node()" mode="cleanXhtml"/>
              </a>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="node()" mode="cleanXhtml"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>

  <!-- Image Module Display with Link -->
  <xsl:template match="Content[@type='Module' and @moduleType='Image' and @link!='']" mode="displayBrief">
    <xsl:param name="maxWidth"/>
    <xsl:param name="maxHeight"/>
    <xsl:param name="lazy"/>
    <a>
      <xsl:attribute name="href">
        <xsl:choose>
          <xsl:when test="format-number(@link,'0')!='NaN'">
            <xsl:variable name="pageId" select="@link"/>
            <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]" mode="getHref"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="@link"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <xsl:attribute name="title">
        <xsl:choose>
          <xsl:when test="format-number(@link,'0')!='NaN'">
            <xsl:variable name="pageId" select="@url"/>
            <xsl:value-of select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]/@name"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="@linkText"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <xsl:if test="@linkType='external' and starts-with(@link,'http')">
        <xsl:attribute name="rel">external</xsl:attribute>
      </xsl:if>
      <xsl:apply-templates select="." mode="displayBriefImg">
        <xsl:with-param name="maxWidth" select="$maxWidth"/>
        <xsl:with-param name="maxHeight" select="$maxHeight"/>
      </xsl:apply-templates>
    </a>
  </xsl:template>


</xsl:stylesheet>
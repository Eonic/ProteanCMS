<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew fb g xlink" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew" xmlns:fb="https://www.facebook.com/2008/fbml" xmlns:xlink="http://www.w3.org/2000/svg" xmlns:g="http://base.google.com/ns/1.0">

  <!-- Tags Display -->
  <xsl:template match="Content" mode="displayTags">
    <xsl:param name="sortBy"/>
    <xsl:variable name="articleList">
      <xsl:for-each select="Content[@type='Tag']">
        <xsl:copy-of select="."/>
      </xsl:for-each>
    </xsl:variable>
    <xsl:if test="count(Content[@type='Tag'])&gt;0">
      <div class="tags">
        <p>
          <!--Tags-->
          <xsl:call-template name="term2039" />
          <xsl:text>: </xsl:text>
          <xsl:apply-templates select="ms:node-set($articleList)" mode="displayBrief">
            <xsl:with-param name="sortBy" select="@sortBy"/>
          </xsl:apply-templates>
        </p>
      </div>
    </xsl:if>
  </xsl:template>

  <!-- Tags Brief -->
  <xsl:template match="Content[@type='Tag']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <xsl:variable name="name" select="Name/node()"/>
    <span>
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <a href="{$parentURL}" rel="tag">
        <xsl:apply-templates select="Name" mode="displayBrief"/>
        <xsl:if test="@relatedCount!=''">
          &#160;(<xsl:value-of select="@relatedCount"/>)
        </xsl:if>
      </a>
      <xsl:if test="position()!=last()">
        <span class="tag-comma">
          <xsl:text>, </xsl:text>
        </span>
      </xsl:if>
    </span>
  </xsl:template>

  <!-- Tags Brief -->
  <xsl:template match="Content[@type='Tag']" mode="displayBriefNoLink">
    <xsl:param name="sortBy"/>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <xsl:variable name="name" select="Name/node()"/>
    <span>
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <xsl:apply-templates select="Name" mode="displayBrief"/>
      <xsl:if test="@relatedCount!=''">
        &#160;(<xsl:value-of select="@relatedCount"/>)
      </xsl:if>
      <xsl:if test="position()!=last()">
        <span class="tag-comma">
          <xsl:text>, </xsl:text>
        </span>
      </xsl:if>
    </span>
  </xsl:template>

  <!-- Tags Detail -->
  <xsl:template match="Content[@type='Tag']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <div class="detail tag">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail tag'"/>
      </xsl:apply-templates>
      <h1>
        <xsl:value-of select="Name/node()"/>
      </h1>
      <div class="tags cols cols3">
        <xsl:apply-templates select="Content" mode="displayBrief">
          <xsl:sort select="@publish" order="descending"/>
        </xsl:apply-templates>
        <xsl:text> </xsl:text>
      </div>
      <!-- Terminus class fix to floating content -->
      <div class="terminus">&#160;</div>
      <div class="entryFooter">
        <div class="tags">
          <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
        </div>
        <xsl:apply-templates select="." mode="backLink">
          <xsl:with-param name="link" select="$thisURL"/>
          <xsl:with-param name="altText">
            <!--click here to return to the tags list-->
            <xsl:call-template name="term2040" />
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div class="terminus">&#160;</div>
    </div>
  </xsl:template>

  <!-- Tags Display -->
  <xsl:template match="Content" mode="displayTagsNoLink">
    <xsl:param name="sortBy"/>
    <xsl:variable name="articleList">
      <xsl:for-each select="Content[@type='Tag']">
        <xsl:copy-of select="."/>
      </xsl:for-each>
    </xsl:variable>
    <xsl:if test="count(Content[@type='Tag'])&gt;0">
      <div class="tags">
        <!--Tags-->
        <xsl:apply-templates select="ms:node-set($articleList)" mode="displayBriefNoLink">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
      </div>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Content" mode="contentColumns">
    <xsl:variable name="xsColsToShow">
      <xsl:choose>
        <xsl:when test="@xsCol='2'">2</xsl:when>
        <xsl:otherwise>1</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="smColsToShow">
      <xsl:choose>
        <xsl:when test="@smCol and @smCol!=''">
          <xsl:value-of select="@smCol"/>
        </xsl:when>
        <xsl:otherwise>1</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="mdColsToShow">
      <xsl:choose>
        <xsl:when test="@mdCol and @mdCol!=''">
          <xsl:value-of select="@mdCol"/>
        </xsl:when>
        <xsl:otherwise>1</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="lgColsToShow">
      <xsl:choose>
        <xsl:when test="@lgCol and @lgCol!=''">
          <xsl:value-of select="@lgCol"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@cols"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="xlColsToShow">
      <xsl:choose>
        <xsl:when test="@xlCol and @xlCol!=''">
          <xsl:value-of select="@xlCol"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@cols"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:attribute name="data-xscols">
      <xsl:value-of select="$xsColsToShow"/>
    </xsl:attribute>
    <xsl:attribute name="data-smcols">
      <xsl:value-of select="$smColsToShow"/>
    </xsl:attribute>
    <xsl:attribute name="data-mdcols">
      <xsl:value-of select="$mdColsToShow"/>
    </xsl:attribute>
    <xsl:attribute name="data-lgcols">
      <xsl:value-of select="$lgColsToShow"/>
    </xsl:attribute>
    <xsl:attribute name="data-xlcols">
      <xsl:value-of select="$xlColsToShow"/>
    </xsl:attribute>
    <xsl:attribute name="class">
      <xsl:text>row cols row-cols-1</xsl:text>
      <xsl:if test="@xsCol='2'"> row-cols-2</xsl:if>
      <xsl:if test="@smCol and @smCol!=''">
        <xsl:text> row-cols-sm-</xsl:text>
        <xsl:value-of select="@smCol"/>
      </xsl:if>
      <xsl:if test="@mdCol and @mdCol!=''">
        <xsl:text> row-cols-md-</xsl:text>
        <xsl:value-of select="@mdCol"/>
      </xsl:if>
      <xsl:if test="@lgCol and @lgCol!=''">
        <xsl:text> row-cols-lg-</xsl:text>
        <xsl:value-of select="@lgCol"/>
      </xsl:if>
      <xsl:if test="@xlCol and @xlCol!=''">
        <xsl:text> row-cols-xl-</xsl:text>
        <xsl:value-of select="@xlCol"/>
      </xsl:if>
      <xsl:if test="@cols and @cols!=''">
        <xsl:text> row-cols-xxl-</xsl:text>
        <xsl:value-of select="@cols"/>
      </xsl:if>
    </xsl:attribute>
  </xsl:template>
</xsl:stylesheet>

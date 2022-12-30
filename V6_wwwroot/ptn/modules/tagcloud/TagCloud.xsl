<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!-- Tag Cloud Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='TagCloud']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- Output Module -->
    <div class="TagsCloud">
      <div class="cols{@cols}">
        <!-- If Stepper, display Stepper -->
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="articleList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <div id="tagcloud">
          <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefCloudItem">
            <xsl:with-param name="sortBy" select="@sortBy"/>
          </xsl:apply-templates>
        </div>
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- Tags Cloud Brief -->
  <xsl:template match="Content[@type='Tag']" mode="displayBriefCloudItem">
    <xsl:param name="sortBy"/>
    <xsl:variable name="name" select="Name/node()"/>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <xsl:variable name="articleCount" select="count($page/Contents/Content[@type='NewsArticle']/Content[@type='Tag' and Name/node() = $name])"/>
    <xsl:variable name="totalArticleCount" select="count($page/Contents/Content/Content[@type='NewsArticle'])"/>
    <xsl:variable name="tagCloudCount" select="round((($articleCount div $totalArticleCount) * 100)) div 10" />
    <a href="{$parentURL}" rel="tag" class="tag tag{$tagCloudCount}">
      <xsl:apply-templates select="Name" mode="displayBrief"/>
      <xsl:text>&#160;</xsl:text>
    </a>
  </xsl:template>

  
</xsl:stylesheet>
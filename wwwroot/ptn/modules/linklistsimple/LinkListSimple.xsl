<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
  <!-- Simple Links Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='LinkListSimple']" mode="displayBrief">
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
    <div class="clearfix LinkListSimple">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix LinkListSimple content-scroller</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <div class="cols cols{@cols}" data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1" data-dots="{@carouselBullets}">
        <xsl:if test="@autoplay !=''">
          <xsl:attribute name="data-autoplay">
            <xsl:value-of select="@autoplay"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="@autoPlaySpeed !=''">
          <xsl:attribute name="data-autoPlaySpeed">
            <xsl:value-of select="@autoPlaySpeed"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="linkList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefSimple">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <!-- Simple Links Brief -->
  <xsl:template match="Content[@type='Link']" mode="displayBriefSimple">
    <xsl:param name="sortBy"/>
    <xsl:variable name="preURL" select="substring(Url,1,3)" />
    <xsl:variable name="url" select="Url/node()" />
    <xsl:variable name="linkURL">
      <xsl:choose>
        <xsl:when test="format-number($url,'0')!='NaN'">
          <xsl:apply-templates select="$page/descendant-or-self::MenuItem[@id=$url]" mode="getHref"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:if test="$preURL='www' or $preURL='WWW'">
            <xsl:text>http://</xsl:text>
          </xsl:if>
          <xsl:value-of select="$url"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <div class="list-group-item listItem linkSimple">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'list-group-item listItem linkSimple'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <xsl:if test="Images/img/@src!=''">
        <a href="{$linkURL}" title="Click here to link to {Name}">
          <xsl:if test="not(substring(@linkURL,1,1)='/') and (contains(@linkURL,'http://') and Url/@type='external')">
            <xsl:attribute name="rel">external</xsl:attribute>
          </xsl:if>
          <xsl:apply-templates select="." mode="displayThumbnail"/>
        </a>
      </xsl:if>
      <a href="{$linkURL}" title="{Name}">
        <xsl:if test="not(substring(@linkURL,1,1)='/') and (contains(@linkURL,'http://') and Url/@type='external')">
          <xsl:attribute name="rel">external</xsl:attribute>
          <xsl:attribute name="class">extLink</xsl:attribute>
        </xsl:if>
        <xsl:value-of select="Name"/>
      </a>
    </div>
  </xsl:template>
</xsl:stylesheet>
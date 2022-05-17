<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!--   ################   Products   ###############   -->
  <!-- Product Gallery Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='ProductGallery']" mode="displayBrief">
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
    <div class="ProductGallery Grid">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix ProductGallery Grid content-scroller</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <div data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1" >
        <xsl:apply-templates select="." mode="contentColumns"/>
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
            <xsl:with-param name="articleList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefGallery">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
        <xsl:text> </xsl:text>
      </div>
    </div>
  </xsl:template>

  <!-- Product Gallery Brief -->
  <xsl:template match="Content[@type='Product']" mode="displayBriefGallery">
    <xsl:param name="sortBy"/>
    <xsl:param name="pos"/>
    <xsl:variable name="parId">
      <xsl:choose>
        <xsl:when test="@parId &gt; 0">
          <xsl:value-of select="@parId"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="/Page/@id"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="self::Content" mode="getHref">
        <xsl:with-param name="parId" select="$parId"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="grid-item">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'grid-item hproduct'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <a href="{$parentURL}" title="{@name}" class="url">
        <div class="thumbnail">
          <img src="{Images/img[@class='detail']/@src}" class="img-responsive" style="overflow:hidden;"/>
          <div class="caption">
            <h4>
              <xsl:value-of select="Name/node()"/>
            </h4>
            <xsl:apply-templates select="Body/node()" mode="cleanXhtml" />
            <xsl:choose>
              <xsl:when test="Content[@type='SKU']">
                <xsl:apply-templates select="Content[@type='SKU'][1]" mode="displayPrice" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:apply-templates select="." mode="displayPrice" />
              </xsl:otherwise>
            </xsl:choose>
          </div>
        </div>
      </a>
    </div>
  </xsl:template>

  <!-- Specification Link -->
  <xsl:template match="Content" mode="SpecLink">
    <xsl:if test="SpecificationDocument/node()!=''">
      <p class="doclink">
        <a class="{substring-after(SpecificationDocument/node(),'.')}icon" href="{SpecificationDocument/node()}" title="Click here to download a copy of this document">Download Product Specification</a>
      </p>
    </xsl:if>
  </xsl:template>
  
</xsl:stylesheet>
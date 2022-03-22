<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!--   ################   Testimonials   ###############   -->

  <!-- Testimonial Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='TestimonialList']" mode="displayBrief">
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
    <div class="clearfix TestimonialList">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix TestimonialList content-scroller</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <div class="cols cols{@cols}" data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1" data-dots="{@carouselBullets}" height="{@carouselHeight}">
        <!--responsive columns-->

        <xsl:apply-templates select="." mode="contentColumns"/>
        <!--end responsive columns-->
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
        <!-- If Stepper, display Stepper -->
        <xsl:choose>
          <xsl:when test="@linkArticle='true'">
            <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefTestimonialLinked">
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="articleList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:text> </xsl:text>
      </div>
    </div>
  </xsl:template>

  <!-- Testimonial Brief -->
  <xsl:template match="Content[@type='Testimonial']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <!-- testimonialBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="listItem">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <blockquote>
          <xsl:if test="Images/img/@src!=''">
            <a href="{$parentURL}">
              <xsl:attribute name="title">
                <xsl:call-template name="term2042" />
                <xsl:text> - </xsl:text>
                <xsl:value-of select="SourceCompany/node()"/>
              </xsl:attribute>
              <xsl:apply-templates select="." mode="displayThumbnail"/>
            </a>
            <!--Accessiblity fix : Separate adjacent links with more than whitespace-->
            <span class="hidden">|</span>
          </xsl:if>
          <div class="summary">
            <xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
          </div>
          <footer>
            <cite title="{SourceName/node()}">
              <xsl:value-of select="SourceName/node()"/>
            </cite>
            <br/>
            <xsl:apply-templates select="SourceCompany" mode="displayBrief"/>

          </footer>
        </blockquote>
        <xsl:if test="not(@noLink='true')">
          <div class="entryFooter">
            <xsl:apply-templates select="." mode="displayTags"/>
            <xsl:apply-templates select="." mode="moreLink">
              <xsl:with-param name="link" select="$parentURL"/>
              <xsl:with-param name="altText">
                <!--Testimonial from-->
                <xsl:call-template name="term2041" />
                <xsl:text>&#160;</xsl:text>
                <xsl:value-of select="SourceCompany/node()"/>
              </xsl:with-param>
            </xsl:apply-templates>
            <xsl:text> </xsl:text>
          </div>
        </xsl:if>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='Testimonial']" mode="displayBriefTestimonialLinked">
    <xsl:param name="sortBy"/>
    <xsl:param name="link"/>
    <xsl:param name="altText"/>
    <xsl:param name="linkType"/>
    <!-- articleBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="listItem list-group-item newsarticle">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'newsarticle'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <a href="{$parentURL}">
        <xsl:if test="not(substring($link,1,1)='/') and (contains($link,'http://') and $linkType='external')">
          <xsl:attribute name="rel">external</xsl:attribute>
          <xsl:attribute name="class">extLink listItem list-group-item newsarticle</xsl:attribute>
        </xsl:if>
        <div class="lIinner">
          <h3 class="title">
            <xsl:apply-templates select="." mode="getDisplayName"/>
          </h3>
          <xsl:if test="Images/img/@src!=''">
            <xsl:apply-templates select="." mode="displayThumbnail"/>
            <!--Accessiblity fix : Separate adjacent links with more than whitespace-->
            <span class="hidden">|</span>
          </xsl:if>
          <xsl:apply-templates select="Content[@type='Contact']" mode="displayAuthorBrief"/>
          <xsl:if test="@publish!=''">
            <p class="date">
              <xsl:value-of select="/Page/Contents/Content[@name='articleLabel']"/>
              <xsl:call-template name="DisplayDate">
                <xsl:with-param name="date" select="@publish"/>
              </xsl:call-template>
            </p>
          </xsl:if>
          <xsl:if test="Strapline/node()!=''">
            <div class="summary">
              <xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
            </div>
          </xsl:if>
        </div>
      </a>
    </div>
  </xsl:template>


  <!-- Testimonial Detail -->
  <xsl:template match="Content[@type='Testimonial']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <div class="detail testimonial">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail testimonial'"/>
      </xsl:apply-templates>
      <xsl:apply-templates select="." mode="displayDetailImage"/>
      <h2 class="entry-title content-title">
        <xsl:value-of select="SourceCompany/node()"/>
      </h2>
      <p class="lead">
        <xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
      </p>
      <div class="entry-content">
        <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
      </div>
      <div class="source">
        <p>
          <xsl:if test="SourceName/node()!=''">
            <span class="sourceName">
              <xsl:apply-templates select="SourceName/node()" mode="displayBrief"/>
            </span>
          </xsl:if>
          <xsl:if test="SourceCompany/node()!=''">
            <br />
            <xsl:apply-templates select="SourceCompany/node()" mode="displayBrief"/>
          </xsl:if>
        </p>
      </div>
      <div class="entryFooter">
        <div class="tags">
          <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
          <xsl:text> </xsl:text>
        </div>
        <xsl:apply-templates select="." mode="backLink">
          <xsl:with-param name="link" select="$thisURL"/>
          <xsl:with-param name="altText">
            <!--click here to return to the testimonial list-->
            <xsl:call-template name="term2043" />
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

</xsl:stylesheet>
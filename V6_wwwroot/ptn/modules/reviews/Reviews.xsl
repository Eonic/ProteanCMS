<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!--  ======================================================================================  -->
  <!--  ==  REVIEWS  =========================================================================  -->
  <!--  ======================================================================================  -->
  <!-- Old Template for legacy matching -->
  <xsl:template match="/" mode="List_Related_Reviews">
    <xsl:apply-templates mode="relatedReviews" select="$page/ContentDetail/Content" />
  </xsl:template>

  <!-- related review template, matches on parent Content e.g Product or Recipe -->
  <xsl:template match="Content" mode="relatedReviews">
    <xsl:if test="Content[@type='Review']">
      <div class="relatedcontent reviews">
        <h4>
          <xsl:call-template name="term2016" />
        </h4>
        <div class="hreview-aggregate">
          <xsl:apply-templates select="." mode="aggregateReviews" />
        </div>
        <xsl:apply-templates select="Content[@type='Review']" mode="displayBrief">
          <xsl:sort select="@publish" order="ascending"/>
          <xsl:sort select="@update" order="ascending"/>
        </xsl:apply-templates>
        <div class="terminus">&#160;</div>
      </div>
    </xsl:if>
  </xsl:template>

  <!-- aggregate reviews -->
  <xsl:template match="Content" mode="aggregateReviews">
    <p class="rating-foreground rating">
      <xsl:variable name="OverallRating">
        <xsl:apply-templates select="." mode="getAverageReviewRating"/>
      </xsl:variable>
      <span>
        <xsl:attribute name="class">
          <xsl:text>value-title reviewRate rating</xsl:text>
          <xsl:value-of select="$OverallRating"/>
        </xsl:attribute>
        <xsl:attribute name="title">
          <xsl:value-of select="$OverallRating"/>
        </xsl:attribute>
        <xsl:attribute name="alt">
          <xsl:call-template name="term2020" />
          <xsl:text>:&#160;</xsl:text>
          <xsl:value-of select="$OverallRating"/>
          <xsl:text>&#160;</xsl:text>
          <xsl:call-template name="term2021" />
        </xsl:attribute>
        <xsl:text>&#160;</xsl:text>
      </span>
      <xsl:text> based on </xsl:text>
      <span class="count">
        <xsl:value-of select="count(Content[@type='Review'])"/>
      </span>
      <xsl:text> review</xsl:text>
      <xsl:if test="count(Content[@type='Review']) &gt; 1">
        <xsl:text>s</xsl:text>
      </xsl:if>
    </p>
  </xsl:template>

  <!-- Review Brief -->
  <xsl:template match="Content[@type='Review']" mode="displayBrief">
    <xsl:param name="pos"/>
    <xsl:param name="class"/>
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
    <div class="listItem hreview {$class}">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="concat('listItem hreview ',$class)"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <h3 style="display:none;" class="title item">
          <a class="fn" href="{$parentURL}" title="{@name}">
            <xsl:value-of select="/Page/ContentDetail/Content/@name"/>
          </a>
        </h3>
        <xsl:choose>
          <xsl:when test="Path!=''">
            <!-- When is a pdf -->
            <a rel="external">
              <xsl:attribute name="href">
                <xsl:choose>
                  <xsl:when test="contains(Path,'http://')">
                    <xsl:value-of select="Path/node()"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$appPath"/>
                    <xsl:text>ewcommon/tools/download.ashx?docId=</xsl:text>
                    <xsl:value-of select="@id"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
              <xsl:attribute name="title">
              </xsl:attribute>
              <xsl:apply-templates select="." mode="displayThumbnail"/>
            </a>
          </xsl:when>
          <!-- When is a image -->
          <xsl:otherwise>
            <xsl:apply-templates select="." mode="displayDetailImage"/>
          </xsl:otherwise>
        </xsl:choose>

        <xsl:if test="Rating/node()!=''">
          <span class="rating-foreground rating">
            <span>
              <xsl:attribute name="class">
                <xsl:text>value-title reviewRate rating</xsl:text>
                <xsl:value-of select="Rating"/>
              </xsl:attribute>
              <xsl:attribute name="title">
                <xsl:value-of select="Rating"/>
              </xsl:attribute>
              <xsl:attribute name="alt">
                <xsl:call-template name="term2020" />
                <xsl:text>:&#160;</xsl:text>
                <xsl:value-of select="Rating"/>
                <xsl:text>&#160;</xsl:text>
                <xsl:call-template name="term2021" />
              </xsl:attribute>
              <xsl:choose>
                <xsl:when test="Rating='5'">
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                </xsl:when>
                <xsl:when test="Rating='4'">
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                </xsl:when>
                <xsl:when test="Rating='3'">
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>

                </xsl:when>
                <xsl:when test="Rating='2'">
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>

                </xsl:when>
                <xsl:when test="Rating='1'">
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                </xsl:when>
              </xsl:choose>
            </span>
            <br/>
          </span>
        </xsl:if>
        <xsl:call-template name="term2018" />
        <xsl:text>&#160;</xsl:text>
        <span class="reviewer">
          <xsl:value-of select="Reviewer"/>
        </span>
        <xsl:text>&#160;</xsl:text>
        <!--<xsl:call-template name="term2019" />-->
        <xsl:text>&#160;</xsl:text>
        <!--<span class="dtreviewed">
          <xsl:call-template name="DisplayDate">
            <xsl:with-param name="date" select="ReviewDate/node()"/>
          </xsl:call-template>
          <span class="value-title">
            <xsl:attribute name="title">
              <xsl:value-of select="ReviewDate/node()"/>
            </xsl:attribute>
          </span>
        </span>-->



        <span class="summary">
          <xsl:apply-templates select="Summary/node()" mode="cleanXhtml"/>
        </span>
        <!-- Terminus class fix to floating columns -->
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- Review Detail  - a review shouldn't really have a detail - is a simple bit of content. -->
  <xsl:template match="Content[@type='Review']" mode="ContentDetail">
    <xsl:param name="pos"/>
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"/>
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
    <div class="detail hreview">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem'"/>
      </xsl:apply-templates>
      <h3 class="title item content-title">
        <a class="fn" href="{$parentURL}" title="{@name}">
          <xsl:value-of select="/Page/ContentDetail/Content/@name"/>
        </a>
      </h3>
      <span class="rating-foreground rating">
        <span>
          <xsl:attribute name="class">
            <xsl:text>value-title reviewRate rating</xsl:text>
            <xsl:value-of select="Rating"/>
          </xsl:attribute>
          <xsl:attribute name="title">
            <xsl:value-of select="Rating"/>
          </xsl:attribute>
          <xsl:attribute name="alt">
            <xsl:call-template name="term2020" />
            <xsl:text>:&#160;</xsl:text>
            <xsl:value-of select="Rating"/>
            <xsl:text>&#160;</xsl:text>
            <xsl:call-template name="term2021" />
          </xsl:attribute>
        </span>
      </span>

      <xsl:apply-templates select="Path/node()" mode="displayThumbnail"/>
      <xsl:call-template name="term2018" />
      <xsl:text>&#160;</xsl:text>
      <span class="reviewer">
        <xsl:value-of select="Reviewer"/>
      </span>
      <xsl:text>&#160;</xsl:text>
      <xsl:call-template name="term2019" />
      <xsl:text>&#160;</xsl:text>
      <!--<span class="dtreviewed">
        <xsl:call-template name="DisplayDate">
          <xsl:with-param name="date" select="ReviewDate/node()"/>
        </xsl:call-template>
        <span class="value-title">
          <xsl:attribute name="title">
            <xsl:value-of select="ReviewDate/node()"/>
          </xsl:attribute>
        </span>
      </span>-->
      <span class="summary">
        <xsl:apply-templates select="Summary" mode="cleanXhtml"/>
      </span>
      <span class="description">
        <xsl:apply-templates select="Description/node()" mode="cleanXhtml"/>
      </span>
      <div class="entryFooter">
        <div class="tags">
          <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
          <xsl:text> </xsl:text>
        </div>
        <xsl:apply-templates select="." mode="backLink">
          <xsl:with-param name="link" select="/Page/Request/ServerVariables/Item[@name='PREVIOUS_PAGE']/node()"/>
        </xsl:apply-templates>
      </div>
      <!-- Terminus class fix to floating columns -->
      <div class="terminus">&#160;</div>
    </div>
  </xsl:template>


</xsl:stylesheet>
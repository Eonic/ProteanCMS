<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
 
    <!--  ======================================================================================  -->
  <!--  ==  BACKGROUND CAROUSEL - based on bootstrap carousel=================================  -->
  <!--  ======================================================================================  -->
  <xsl:template match="Content[@moduleType='BackgroundCarousel']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <Content>
        <xsl:for-each select="@*">
          <xsl:attribute name="{name()}">
            <xsl:value-of select="."/>
          </xsl:attribute>
        </xsl:for-each>
        <xsl:apply-templates select="." mode="getContent">
          <xsl:with-param name="contentType" select="$contentType" />
          <xsl:with-param name="startPos" select="$startPos" />
        </xsl:apply-templates>
      </Content>
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
    <xsl:variable name="id" select="concat('bscarousel-',@id)"></xsl:variable>
    <div id="{$id}" class="carousel slide background-carousel" data-ride="carousel" data-interval="{@interval}" pause="hover" wrap="true">
      <xsl:if test="@bullets!='true'">
        <ol class="carousel-indicators">
          <xsl:for-each select="Content[@type='LibraryImageWithLink']">
            <li data-target="#{$id}" data-slide-to="{position()-1}">
              <xsl:if test="position()=1">
                <xsl:attribute name="class">active</xsl:attribute>
              </xsl:if>
              <xsl:text></xsl:text>
            </li>
          </xsl:for-each>
        </ol>
      </xsl:if>
      <div class="carousel-inner" style="height:{@height}px">
        <xsl:apply-templates select="Content[@type='LibraryImageWithLink' or @type='BackgroundCarouselSlide']" mode="displayBriefSliderGalleryBackground">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
      </div>
      <xsl:if test="@arrows!='true'">
        <a class="left carousel-control" href="#{$id}" data-slide="prev">
          <span class="glyphicon glyphicon-chevron-left"></span>
        </a>
        <a class="right carousel-control" href="#{$id}" data-slide="next">
          <span class="glyphicon glyphicon-chevron-right"></span>
        </a>
      </xsl:if>
    </div>
  </xsl:template>
  <!-- Library Image Brief -->
  <xsl:template match="Content[@type='LibraryImageWithLink' or @type='BackgroundCarouselSlide']" mode="displayBriefSliderGalleryBackground">
    <div class="item" style="background-image:url({Images/img[@class='detail']/@src})">
      <xsl:if test="position()=1">
        <xsl:attribute name="class">item active</xsl:attribute>
      </xsl:if>
      <xsl:if test="(Title/node()!='' and not(@showHeading='false')) or Body/node()!=''">
        <div class="carousel-caption">
          <xsl:attribute name="class">
            <xsl:text>carousel-caption container carousel-v-</xsl:text>
            <xsl:value-of select="@position-vertical"/>
            <xsl:text> carousel-h-</xsl:text>
            <xsl:value-of select="@position-horizontal"/>
          </xsl:attribute>
          <div class="carousel-caption-inner">
            <xsl:if test="Title/node()!='' and not(@showHeading='false')">
              <h3 class="caption-title">
                <xsl:value-of select="Title/node()"/>
              </h3>
            </xsl:if>
            <xsl:apply-templates select="Body/node()" mode="cleanXhtml"></xsl:apply-templates>
            <xsl:if test="@link!=''">

              <xsl:apply-templates select="." mode="moreLink">
                <xsl:with-param name="link">
                  <xsl:choose>
                    <xsl:when test="format-number(@link,'0')!='NaN'">
                      <xsl:variable name="pageId" select="@link"/>
                      <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId or PageVersion[@vParId=$pageId]]" mode="getHref"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="@link"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:with-param>
                <xsl:with-param name="linkText" select="@linkText"/>
                <xsl:with-param name="altText" select="@title"/>
              </xsl:apply-templates>
            </xsl:if>
          </div>
        </div>
      </xsl:if>
    </div>
  </xsl:template>
  
</xsl:stylesheet>
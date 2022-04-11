<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
 
   <!-- Slide Carousel -->
  <xsl:template match="Content[@type='Module' and @moduleType='SlideCarousel']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
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
    <!--InnerFade Module JS -->
    <xsl:if test="not(/Page/Request/QueryString/Item[@name='innerFade']/node()='disabled')">
      <xsl:variable name="animationtype">
        <xsl:value-of select="@animationtype"/>
      </xsl:variable>
      <xsl:variable name="SlidesShowing">
        <xsl:value-of select="@SlidesShowing"/>
      </xsl:variable>
      <xsl:variable name="AutoPlay">
        <xsl:value-of select="@AutoPlay"/>
      </xsl:variable>
      <xsl:variable name="hAlign">
        <xsl:value-of select="@hAlign"/>
      </xsl:variable>
      <xsl:variable name="vAlign">
        <xsl:value-of select="@vAlign"/>
      </xsl:variable>
      <xsl:variable name="SlideHeight">
        <xsl:value-of select="@SlideHeight"/>
      </xsl:variable>
      <xsl:variable name="SlideWidth">
        <xsl:value-of select="@SlideWidth"/>
      </xsl:variable>
      <xsl:variable name="CarouselHeight">
        <xsl:value-of select="@CarouselHeight"/>
      </xsl:variable>
      <xsl:variable name="CarouselWidth">
        <xsl:value-of select="@CarouselWidth"/>
      </xsl:variable>
      <xsl:variable name="fadebetween">
        <xsl:value-of select="@fadebetween"/>
      </xsl:variable>
      <xsl:variable name="slideshowtype">
        <xsl:value-of select="@slideshowtype"/>
      </xsl:variable>
      <xsl:variable name="containerheight">
        <xsl:apply-templates select="Content[1]" mode="maxFaderHeight">
          <xsl:with-param name="maxheight">
            <xsl:value-of select="Content[1]/img/@height"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </xsl:variable>
      <script type="text/javascript">
        <xsl:text>
        $(document).ready(function(){
        $('.carousel').carousel({hAlign:'</xsl:text>
        <xsl:value-of select="$hAlign"/>
        <xsl:text>',
        vAlign:'</xsl:text>
        <xsl:value-of select="$vAlign"/>
        <xsl:text>',
        hMargin:0.4,
        vMargin:0.2,
        frontWidth:</xsl:text>
        <xsl:value-of select="$SlideWidth"/>
        <xsl:text>,
        frontHeight:</xsl:text>
        <xsl:value-of select="$SlideHeight"/>
        <xsl:text>,
        carouselWidth:</xsl:text>
        <xsl:value-of select="$CarouselWidth"/>
        <xsl:text>,
        carouselHeight:</xsl:text>
        <xsl:value-of select="$CarouselHeight"/>
        <xsl:text>,
        left:0,
        right:0,
        top:27,
        bottom:0,
        backZoom:0.8,
        slidesPerScroll:</xsl:text>
        <xsl:value-of select="$SlidesShowing"/>
        <xsl:text>,
        speed:500,
        buttonNav:'none',
        directionNav:true,
        autoplay:</xsl:text>
        <xsl:value-of select="$AutoPlay"/>
        <xsl:text>,
        autoplayInterval:</xsl:text>
        <xsl:value-of select="$fadebetween"/>
        <xsl:text>,
        pauseOnHover:true,
        mouse:true,
        shadow:true,
        reflection:false,
        reflectionHeight:0.2,
        reflectionOpacity:0.5,
        reflectionColor:'255,255,255',
        description:false, descriptionContainer:'.description',
        backOpacity:1,
        before: function(carousel){},
        after: function(carousel){}
        });
        });
      </xsl:text>
      </script>
    </xsl:if>
    <!-- ###### -->
    <div class="carousel">
      <div class="slides">
        <!--<xsl:apply-templates select="Content[@type='Image']" mode="faderImage"/>-->
        <xsl:apply-templates select="Content[@type='Image']" mode="faderImage2">
          <xsl:with-param name="max-width" select="@SlideWidth"/>
          <xsl:with-param name="max-height" select="@SlideHeight"/>
          <xsl:with-param name="crop" select="'true'"/>
        </xsl:apply-templates>
      </div>
      <div class="terminus">&#160;</div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='Image']" mode="faderImage">
    <div>
      <xsl:apply-templates select="." mode="inlinePopupOptions"/>
      <xsl:apply-templates select="." mode="displayBrief"/>
    </div>
  </xsl:template>

  <xsl:template match="Content" mode="maxFaderHeight">
    <xsl:param name="maxheight"/>
    <xsl:choose>
      <xsl:when test="following-sibling::Content">
        <xsl:apply-templates select="following-sibling::Content[1]" mode="maxFaderHeight">
          <xsl:with-param name="maxheight">
            <xsl:choose>
              <xsl:when test="following-sibling::Content[1]/img/@height &gt; $maxheight">
                <xsl:value-of select="following-sibling::Content[1]/img/@height"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$maxheight"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:with-param>
        </xsl:apply-templates>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$maxheight"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
</xsl:stylesheet>
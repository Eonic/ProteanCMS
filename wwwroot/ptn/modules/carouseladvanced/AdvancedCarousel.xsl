<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
 
  <!--  ======================================================================================  -->
  <!--  ==  Advanced Carousel  ========================================================================  -->
  <!--  ======================================================================================  -->

  <xsl:template match="Content[@type='Module' and @moduleType='AdvancedCarousel']" mode="displayBrief">
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

    <!-- ###### -->
    <div class="advanced-carousel-container" style="height:{@CarouselHeight}px">
      <div class="cover-container">
        <div class="advanced-carousel">
          <ul style="display:none">
            <xsl:choose>
              <xsl:when test="Content[@type='AdvancedCarouselSlide']">
                <xsl:apply-templates select="Content[@type='AdvancedCarouselSlide']" mode="displayBrief"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:apply-templates select="$page/Contents/Content[@type='AdvancedCarouselSlide']" mode="displayBrief"/>
              </xsl:otherwise>
            </xsl:choose>
          </ul>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='Module' and @moduleType='AdvancedCarousel']" mode="contentJS">
    <script type="text/javascript">
      <xsl:text>jQuery(document).ready(function() {</xsl:text>
      <xsl:text>revapi = jQuery('#mod_</xsl:text><xsl:value-of select="@id"/><xsl:text> .advanced-carousel').revolution({</xsl:text>
      delay:<xsl:value-of select="@fadebetween"/>,
      startwidth:<xsl:value-of select="@CarouselWidth"/>,
      startheight:<xsl:value-of select="@CarouselHeight"/>,
      startWithSlide:0,
      fullScreenAlignForce:"off",
      autoHeight:"off",
      minHeight:"off",
      shuffle:"off",
      onHoverStop:"on",
      thumbWidth:100,
      thumbHeight:50,
      <xsl:choose>
        <xsl:when test="count(Content[@type='AdvancedCarouselSlide'])>2">
          thumbAmount:3,
        </xsl:when>
        <xsl:otherwise>
          thumbAmount:<xsl:value-of select="count(Content[@type='AdvancedCarouselSlide'])"/>,
        </xsl:otherwise>
      </xsl:choose>
      hideThumbsOnMobile:"off",
      hideNavDelayOnMobile:1500,
      hideBulletsOnMobile:"off",
      hideArrowsOnMobile:"off",
      hideThumbsUnderResoluition:0,

      hideThumbs:0,
      hideTimerBar:"<xsl:value-of select="@hideTimerBar"/>",

      keyboardNavigation:"on",

      navigationType:"<xsl:value-of select="@navigationType"/>",
      navigationArrows:"<xsl:value-of select="@navigationArrows"/>",
      navigationStyle:"<xsl:value-of select="@navigationStyle"/>",

      navigationHAlign:"<xsl:value-of select="@navigationHAlign"/>",
      navigationVAlign:"<xsl:value-of select="@navigationVAlign"/>",
      navigationHOffset:<xsl:value-of select="@navigationHOffset"/>,
      navigationVOffset:<xsl:value-of select="@navigationVOffset"/>,

      soloArrowLeftHalign:"<xsl:value-of select="@soloArrowLeftHalign"/>",
      soloArrowLeftValign:"<xsl:value-of select="@soloArrowLeftValign"/>",
      soloArrowLeftHOffset:<xsl:value-of select="@soloArrowLeftHOffset"/>,
      soloArrowLeftVOffset:<xsl:value-of select="@soloArrowLeftVOffset"/>,

      soloArrowRightHalign:"<xsl:value-of select="@soloArrowRightHalign"/>",
      soloArrowRightValign:"<xsl:value-of select="@soloArrowRightValign"/>",
      soloArrowRightHOffset:<xsl:value-of select="@soloArrowRightHOffset"/>,
      soloArrowRightVOffset:<xsl:value-of select="@soloArrowRightVOffset"/>,


      touchenabled:"on",
      swipe_velocity:"0.7",
      swipe_max_touches:"1",
      swipe_min_touches:"1",
      drag_block_vertical:"false",

      parallax:"mouse",
      parallaxBgFreeze:"on",
      parallaxLevels:[10,7,4,3,2,5,4,3,2,1],
      parallaxDisableOnMobile:"off",

      stopAtSlide:-1,
      stopAfterLoops:-1,
      hideCaptionAtLimit:0,
      hideAllCaptionAtLilmit:0,
      hideSliderAtLimit:0,

      dottedOverlay:"none",

      spinned:"<xsl:value-of select="@spinner"/>",

      fullWidth:"<xsl:value-of select="@fullWidth"/>",
      forceFullWidth:"off",
      fullScreen:"off",
      fullScreenOffsetContainer:"#topheader-to-offset",
      fullScreenOffset:"0px",

      panZoomDisableOnMobile:"off",

      simplifyAll:"off",

      shadow:0
      <xsl:text>});</xsl:text>
      revapi.bind("revolution.slide.onloaded",function (e) {
      jQuery('.advanced-carousel').find('.dropdown-menu').removeAttr('style');
      });
      revapi.bind("revolution.slide.onchange",function (e) {
      jQuery('.caption').each(function() {
      var thisWidth = $(this).width();
      var leftVal =  parseInt('0' + $(this).css('left').replace(/[^0-9\.]/g, '')) ;
      var dataWidth = $(this).data('width');
      if (dataWidth != 'done') {
      if (dataWidth === undefined) {
      dataWidth = 100
      }
      if (dataWidth == '') {
      dataWidth = 0
      };
      if (dataWidth == '0') {
      $(this).width('auto');
      }
      else
      {
      if (leftVal != 0) {
      $(this).width((thisWidth - leftVal) * (dataWidth / 100));
      $(this).data('width','done');
      }
      }
      };
      });
      });
      <xsl:text>});</xsl:text>


    </script>
  </xsl:template>

  <!-- Library Image Brief -->
  <xsl:template match="Content[@type='AdvancedCarouselSlide']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <xsl:variable name="lg-max-width">
      <xsl:apply-templates select="." mode="getFullSizeWidth"/>
    </xsl:variable>
    <xsl:variable name="lg-max-height">
      <xsl:apply-templates select="." mode="getFullSizeHeight"/>
    </xsl:variable>
    <xsl:variable name="fullSize">
      <xsl:call-template name="resize-image">
        <xsl:with-param name="path" select="Images/img[@class='detail']/@src"/>
        <xsl:with-param name="max-width" select="$lg-max-width"/>
        <xsl:with-param name="max-height" select="$lg-max-height"/>
        <xsl:with-param name="file-prefix">
          <xsl:text>~lg-</xsl:text>
          <xsl:value-of select="$lg-max-width"/>
          <xsl:text>x</xsl:text>
          <xsl:value-of select="$lg-max-width"/>
          <xsl:text>/~lg-</xsl:text>
        </xsl:with-param>
        <xsl:with-param name="file-suffix" select="''"/>
        <xsl:with-param name="quality" select="100"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="lgImgSrc">
      <xsl:choose>
        <xsl:when test="Images/img[@class='detail']/@src != ''">
          <xsl:value-of select="Images/img[@class='detail']/@src"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="Images/img[@class='display']/@src"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="slideLink">
      <xsl:choose>
        <xsl:when test="@internalLink!=''">
          <xsl:variable name="pageId" select="@internalLink"/>
          <xsl:variable name="href">
            <xsl:apply-templates select="//MenuItem[@id=$pageId]" mode="getHref" />
          </xsl:variable>
          <xsl:variable name="title">
            <xsl:apply-templates select="//MenuItem[@id=$pageId]" mode="getTitleAttr" />
          </xsl:variable>
          <xsl:value-of select="$href"/>
        </xsl:when>
        <xsl:when test="@externalLink!=''">
          <xsl:value-of select="@externalLink"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <li data-transition="{@data-transition}" data-masterspeed="{@data-masterspeed}" data-slotamount="{@data-slotamount}" data-title="{Title/node()}" data-thumb="{Images/img[@class='detail']/@src}">
      <xsl:if test="not(/Page/@adminMode) and $slideLink!=''">
        <xsl:attribute name="data-link">
          <xsl:value-of select="$slideLink"/>
        </xsl:attribute>
      </xsl:if>
      <img src="{Images/img[@class='detail']/@src}" title="{Title/node()} - {Body/node()}" data-bgrepeat="{Images/@data-bgrepeat}" data-bgfit="{Images/@data-bgfit}" data-bgposition="{Images/@data-bgposition}">
        <xsl:if test="Images/@data-bgfitend!=''">
          <xsl:attribute name="data-bgfitend">
            <xsl:value-of select="Images/@data-bgfitend"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="Images/@data-bgpositionend!=''">
          <xsl:attribute name="data-bgpositionend">
            <xsl:value-of select="Images/@data-bgpositionend"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="Images/@data-kenburns!=''">
          <xsl:attribute name="data-kenburns">
            <xsl:value-of select="Images/@data-kenburns"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="Images/@data-duration!=''">
          <xsl:attribute name="data-duration">
            <xsl:value-of select="Images/@data-duration"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="Images/@data-ease!=''">
          <xsl:attribute name="data-ease">
            <xsl:value-of select="Images/@data-ease"/>
          </xsl:attribute>
        </xsl:if>
      </img>
      <xsl:apply-templates select="Captions/*" mode="slideCaption"/>
      <xsl:if test="/Page/@adminMode">
        <div class="caption"  data-x="right" data-hoffset="-10" data-y="top" data-voffset="10"  data-speed="100" data-start="100">
          <xsl:apply-templates select="." mode="inlinePopupOptions">
            <xsl:with-param name="class" select="'caption'"/>
            <xsl:with-param name="sortBy" select="$sortBy"/>
          </xsl:apply-templates>
        </div>
      </xsl:if>
    </li>
  </xsl:template>

  <xsl:template match="Caption" mode="slideCaption">
    <div data-easing="{@data-easing}" data-x="{@data-x}" data-hoffset="{@data-hoffset}" data-y="{@data-y}" data-voffset="{@data-voffset}" data-start="{@data-start}" data-speed="{@data-speed}" data-end="{@data-end}" data-endspeed="{@data-endspeed}" data-endeasing="{@data-endeasing}" data-width="{@data-width}%">
      <xsl:attribute name="class">
        <xsl:text>caption </xsl:text>
        <xsl:value-of select="@style"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@styleIn"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@styleOut"/>
      </xsl:attribute>
      <xsl:apply-templates select="div/node()" mode="cleanXhtml"/>
    </div>
  </xsl:template>

  <xsl:template match="Caption[@type='image']" mode="slideCaption">
    <div data-easing="{@data-easing}" data-x="{@data-x}" data-hoffset="{@data-hoffset}" data-y="{@data-y}" data-voffset="{@data-voffset}" data-start="{@data-start}" data-speed="{@data-speed}" data-end="{@data-end}" data-endspeed="{@data-endspeed}" data-endeasing="{@data-endeasing}" data-width="{@data-width}%">
      <xsl:attribute name="class">
        <xsl:text>caption image </xsl:text>
        <xsl:value-of select="@style"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@styleIn"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@styleOut"/>
      </xsl:attribute>
      <img src="{@Image}"/>
    </div>
  </xsl:template>

  <!---Vimeo-->
  <xsl:template match="Caption[@type='Vimeo']" mode="slideCaption">
    <div data-easing="{@data-easing}" data-x="{@data-x}" data-hoffset="{@data-hoffset}" data-y="{@data-y}" data-voffset="{@data-voffset}" data-start="{@data-start}" data-speed="{@data-speed}" data-end="{@data-end}" data-endspeed="{@data-endspeed}" data-endeasing="{@data-endeasing}" data-width="{@data-width}%">
      <xsl:attribute name="class">
        <xsl:text>tp-caption tp-videolayer</xsl:text>
        <xsl:value-of select="@style"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@styleIn"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@styleOut"/>
      </xsl:attribute>
      <xsl:if test="@data-autoplayVimeo!=''">
        <xsl:attribute name="data-autoplay">
          <xsl:value-of select="@data-autoplayVimeo"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-autoplayonlyfirsttimeVimeo!=''">
        <xsl:attribute name="data-autoplayonlyfirsttime">
          <xsl:value-of select="@data-autoplayonlyfirsttimeVimeo"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videowidthVimeo!=''">
        <xsl:attribute name="data-videowidth">
          <xsl:value-of select="@data-videowidthVimeo"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videoheightVimeo!=''">
        <xsl:attribute name="data-videoheight">
          <xsl:value-of select="@data-videoheightVimeo"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-nextslideatendVimeo!=''">
        <xsl:attribute name="data-nextslideatend">
          <xsl:value-of select="@data-nextslideatendVimeo"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-vimeoid!=''">
        <xsl:attribute name="data-vimeoid">
          <xsl:value-of select="@data-vimeoid"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videocontrolsVimeo!=''">
        <xsl:attribute name="data-videocontrols">
          <xsl:value-of select="@data-videocontrolsVimeo"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videoattributesVimeo!=''">
        <xsl:attribute name="data-videoattributes">
          <xsl:value-of select="@data-videoattributesVimeo"/>
        </xsl:attribute>
      </xsl:if>
    </div>
  </xsl:template>

  <!--YouTube-->
  <xsl:template match="Caption[@type='YouTube']" mode="slideCaption">
    <div data-easing="{@data-easing}" data-x="{@data-x}" data-hoffset="{@data-hoffset}" data-y="{@data-y}" data-voffset="{@data-voffset}" data-start="{@data-start}" data-speed="{@data-speed}" data-end="{@data-end}" data-endspeed="{@data-endspeed}" data-endeasing="{@data-endeasing}" data-width="{@data-width}%" data-videoattributes="enablejsapi=1&amp;html5=1&amp;hd=1&amp;wmode=opaque&amp;showinfo=0&amp;rel=0">
      <xsl:attribute name="class">
        <xsl:text>tp-caption tp-videolayer</xsl:text>
        <xsl:value-of select="@style"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@styleIn"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@styleOut"/>
      </xsl:attribute>
      <xsl:if test="@data-autoplayYouTube!=''">
        <xsl:attribute name="data-autoplay">
          <xsl:value-of select="@data-autoplayYouTube"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-autoplayonlyfirsttimeYouTube!=''">
        <xsl:attribute name="data-autoplayonlyfirsttime">
          <xsl:value-of select="@data-autoplayonlyfirsttimeYouTube"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-nextslideatendYouTube!=''">
        <xsl:attribute name="data-nextslideatend">
          <xsl:value-of select="@data-nextslideatendYouTube"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videowidthYouTube!=''">
        <xsl:attribute name="data-videowidth">
          <xsl:value-of select="@data-videowidthYouTube"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videoheightYouTube!=''">
        <xsl:attribute name="data-videoheight">
          <xsl:value-of select="@data-videoheightYouTube"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-ytid!=''">
        <xsl:attribute name="data-ytid">
          <xsl:value-of select="@data-ytid"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videocontrolsYouTube!=''">
        <xsl:attribute name="data-videocontrols">
          <xsl:value-of select="@data-videocontrolsYouTube"/>
        </xsl:attribute>
      </xsl:if>
    </div>
  </xsl:template>

  <!--HTML5-->
  <xsl:template match="Caption[@type='HTML5']" mode="slideCaption">
    <div data-easing="{@data-easing}" data-x="{@data-x}" data-hoffset="{@data-hoffset}" data-y="{@data-y}" data-voffset="{@data-voffset}" data-start="{@data-start}" data-speed="{@data-speed}" data-end="{@data-end}" data-endspeed="{@data-endspeed}" data-endeasing="{@data-endeasing}" data-width="{@data-width}%">
      <xsl:attribute name="class">
        <xsl:text>tp-caption tp-videolayer</xsl:text>
        <xsl:value-of select="@style"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@styleIn"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@styleOut"/>
      </xsl:attribute>
      <xsl:if test="@data-autoplayHTML5!=''">
        <xsl:attribute name="data-autoplay">
          <xsl:value-of select="@data-autoplayHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-autoplayonlyfirsttimeHTML5!=''">
        <xsl:attribute name="data-autoplayonlyfirsttime">
          <xsl:value-of select="@data-autoplayonlyfirsttimeHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videoposterHTML5!=''">
        <xsl:attribute name="data-videoposter">
          <xsl:value-of select="@data-videoposterHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-forcecoverHTML5!=''">
        <xsl:attribute name="data-forcecover">
          <xsl:value-of select="@data-forcecoverHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-forcerewindHTML5!=''">
        <xsl:attribute name="data-forcerewind">
          <xsl:value-of select="@data-forcerewindHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-volumeHTML5!=''">
        <xsl:attribute name="data-volume">
          <xsl:value-of select="@data-volumeHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videowidthHTML5!=''">
        <xsl:attribute name="data-videowidth">
          <xsl:value-of select="@data-videowidthHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videoheightHTML5!=''">
        <xsl:attribute name="data-videoheight">
          <xsl:value-of select="@data-videoheightHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-aspectratioHTML5!=''">
        <xsl:attribute name="data-aspectratio">
          <xsl:value-of select="@data-aspectratioHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videopreloadHTML5!=''">
        <xsl:attribute name="data-videopreload">
          <xsl:value-of select="@data-videopreloadHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videomp4HTML5!=''">
        <xsl:attribute name="data-videomp4">
          <xsl:value-of select="@data-videomp4"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videowebmHTML5!=''">
        <xsl:attribute name="data-videowebm">
          <xsl:value-of select="@data-videowebmHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videoogvHTML5!=''">
        <xsl:attribute name="data-videoogv">
          <xsl:value-of select="@data-videoogvHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videocontrolsHTML5!=''">
        <xsl:attribute name="data-videocontrols">
          <xsl:value-of select="@data-videocontrolsHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videoattributesHTML5!=''">
        <xsl:attribute name="data-videoattributes">
          <xsl:value-of select="@data-videoattributesHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-nextslideatendHTML5!=''">
        <xsl:attribute name="data-nextslideatend">
          <xsl:value-of select="@data-nextslideatendHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videoloopHTML5!=''">
        <xsl:attribute name="data-videoloop">
          <xsl:value-of select="@data-videoloopHTML5"/>
        </xsl:attribute>
      </xsl:if>
    </div>
  </xsl:template>
  
</xsl:stylesheet>
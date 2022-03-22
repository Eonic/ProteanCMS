<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!--   ################   Videos   ###############   -->
  <!-- Video list Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='VideoList']" mode="displayBrief">
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
    <div class="clearfix VideoList">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix VideoList content-scroller</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <div data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1">
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
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <!-- Returns video width -->
  <xsl:template match="Content[@moduleType='Video']" mode="videoWidth">
    <xsl:choose>
      <xsl:when test="@size='Manual'">
        <xsl:value-of select="@width"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>640</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Returns video height -->
  <xsl:template match="Content[@moduleType='Video']" mode="videoHeight">
    <xsl:choose>
      <xsl:when test="@size='Manual'">
        <xsl:value-of select="@height"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="@ratio='SixteenNine'">
            <xsl:text>360</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>480</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Returns classes for video container -->
  <xsl:template match="Content[@moduleType='Video']" mode="videoClasses">
    <xsl:text>Video VideoType</xsl:text>
    <xsl:value-of select="@videoType"/>
    <xsl:text> VideoSize</xsl:text>
    <xsl:value-of select="@size"/>
  </xsl:template>

  <!-- Video Brief -->
  <xsl:template match="Content[@type='Video']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="listItem video">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem video'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <a href="{$parentURL}">
          <xsl:apply-templates select="." mode="displayThumbnail"/>
        </a>
        <a href="{$parentURL}">
          <h3 class="title">
            <xsl:value-of select="Title/node()"/>
          </h3>
        </a>
        <xsl:if test="Author/node()!=''">
          <p class="author">
            <span class="label">
              <!--Author-->
              <xsl:call-template name="term2045" />
              <xsl:text>: </xsl:text>
            </span>
            <xsl:value-of select="Author/node()"/>
          </p>
        </xsl:if>
        <xsl:if test="Copyright/node()!=''">
          <p class="copyright">
            <span class="label">
              <!--Copyright-->
              <xsl:call-template name="term2046" />
              <xsl:text>: </xsl:text>
            </span>
            <xsl:value-of select="Copyright/node()"/>
          </p>
        </xsl:if>
        <xsl:if test="Intro/node()!=''">
          <p class="VideoDescription">
            <xsl:apply-templates select="Intro" mode="cleanXhtml"/>
          </p>
        </xsl:if>
        <div class="entryFooter">
          <xsl:apply-templates select="." mode="displayTags"/>
          <xsl:apply-templates select="." mode="moreLink">
            <xsl:with-param name="link" select="$parentURL"/>
            <xsl:with-param name="altText">
              <xsl:value-of select="Title/node()"/>
            </xsl:with-param>
          </xsl:apply-templates>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='Video']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <div id="Video{@id}" class="detail Video">
      <div class="row">
        <div class="col-lg-4">
          <h1 class="title content-title">
            <xsl:value-of select="Title/node()"/>
          </h1>
          <xsl:if test="Body/node()!=''">
            <p class="description">
              <span class="label">
                <xsl:call-template name="term2092" />
                <xsl:text>: </xsl:text>
              </span>
              <xsl:apply-templates select="Body" mode="cleanXhtml"/>
            </p>
          </xsl:if>
          <xsl:if test="Author/node()!=''">
            <p class="author">
              <span class="label">
                <xsl:call-template name="term2045" />
                <xsl:text>: </xsl:text>
              </span>
              <xsl:value-of select="Author/node()"/>
            </p>
          </xsl:if>
          <xsl:if test="Copyright/node()!=''">
            <p class="copyright">
              <span class="label">
                <xsl:call-template name="term2046" />
                <xsl:text>: </xsl:text>
              </span>
              <xsl:value-of select="Copyright/node()"/>
            </p>
          </xsl:if>
          <div class="entryFooter hidden-xs hidden-sm">
            <div class="tags">
              <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
              <xsl:text> </xsl:text>
            </div>
            <xsl:apply-templates select="." mode="backLink">
              <xsl:with-param name="link" select="$thisURL"/>
              <xsl:with-param name="altText">
                <xsl:call-template name="term2047" />
              </xsl:with-param>
            </xsl:apply-templates>
          </div>
        </div>
        <div class="col-lg-8">
          <xsl:apply-templates select="." mode="VideoDetailDisplay">
            <xsl:with-param name="classes" select="'col-md-8'"/>
          </xsl:apply-templates>
        </div>

        <div class="entryFooter container hidden-md hidden-lg">
          <div class="tags">
            <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
            <xsl:text> </xsl:text>
          </div>
          <xsl:apply-templates select="." mode="backLink">
            <xsl:with-param name="link" select="$thisURL"/>
            <xsl:with-param name="altText">
              <xsl:call-template name="term2047" />
            </xsl:with-param>
          </xsl:apply-templates>
        </div>
      </div>
    </div>
  </xsl:template>

  <!-- Local video -->
  <xsl:template match="Content[@moduleType='Video' and @videoType='Local']" mode="displayBrief">
    <div id="Video{@id}" class="Video">
      <xsl:attribute name="class">
        <xsl:apply-templates select="." mode="videoClasses"/>
      </xsl:attribute>
      <div id="FVPlayer{@id}">
        <a href="http://www.adobe.com/go/getflashplayer">
          <xsl:call-template name="term2004" />
        </a>
        <xsl:text>&#160;</xsl:text>
        <xsl:call-template name="term2005" />
        <xsl:if test="Local/img/@src!=''">
          <xsl:apply-templates select="Local/img" mode="cleanXhtml"/>
        </xsl:if>
      </div>
      <script type="text/javascript">
        <xsl:text>var s1 = new SWFObject('/ewcommon/flash/flvplayer.swf','Flash_</xsl:text>
        <xsl:value-of select="@id"/>
        <xsl:text>', '</xsl:text>
        <xsl:apply-templates select="." mode="videoWidth"/>
        <xsl:text>', '</xsl:text>
        <xsl:apply-templates select="." mode="videoHeight"/>
        <xsl:text>', '7', '7');</xsl:text>
        <xsl:text>s1.addParam('allowfullscreen', 'true');</xsl:text>
        <xsl:text>s1.addParam('wmode', 'transparent');</xsl:text>
        <xsl:text>s1.addVariable('file','</xsl:text>
        <xsl:value-of select="Local/@url"/>
        <xsl:text>');</xsl:text>
        <xsl:if test="Local/img/@src!=''">
          <xsl:text>s1.addVariable('image','</xsl:text>
          <xsl:value-of select="Local/img/@src"/>
          <xsl:text>');</xsl:text>
        </xsl:if>
        <xsl:text>s1.write('FVPlayer</xsl:text>
        <xsl:value-of select="@id"/>
        <xsl:text>');</xsl:text>
      </script>
    </div>
  </xsl:template>

  <!-- YouTube video -->
  <xsl:template match="Content[@moduleType='Video' and @videoType='YouTube']" mode="displayBrief">
    <xsl:variable name="code">
      <xsl:variable name="raw" select="YouTube/@code"/>
      <xsl:choose>
        <!-- http://youtu.be/abcd1234 -->
        <xsl:when test="contains($raw, 'youtu.be/')">
          <xsl:value-of select="substring(substring-after($raw, 'youtu.be/'), 1, 11)"/>
        </xsl:when>
        <!-- http://youtube.com/watch?v=abcd1234 -->
        <xsl:when test="contains($raw, 'v=')">
          <xsl:value-of select="substring(substring-after($raw, 'v='), 1, 11)"/>
        </xsl:when>
        <xsl:when test="contains($raw, 'youtube.com/embed/')">
          <xsl:value-of select="substring(substring-after($raw, 'youtube.com/embed/'), 1, 11)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="substring($raw, 1, 11)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div id="Video{@id}" class="Video">
      <xsl:if test="@size!='Manual'">
        <xsl:attribute name="class">
          <xsl:text>embed-responsive </xsl:text>
          <xsl:choose>
            <xsl:when test="@ratio='FourThree'">embed-responsive-4by3</xsl:when>
            <xsl:otherwise>embed-responsive-16by9</xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </xsl:if>
      <iframe frameborder="0" class="embed-responsive-item" allowfullscreen="allowfullscreen" >
        <xsl:attribute name="src">
          <xsl:text>http</xsl:text>
          <xsl:if test="YouTube/@useHttps='true'">
            <xsl:text>s</xsl:text>
          </xsl:if>
          <xsl:text>://www.youtube.com/embed/</xsl:text>
          <xsl:value-of select="$code"/>
          <xsl:text>?wmode=transparent&amp;rel=0</xsl:text>
          <xsl:if test="YouTube/@showSuggested='true'">&amp;rel=1</xsl:if>
        </xsl:attribute>
        <xsl:choose>
          <xsl:when test="@size='Manual'">
            <xsl:if test="@width!=''">
              <xsl:attribute name="width">
                <xsl:value-of select="@width"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="@height!=''">
              <xsl:attribute name="height">
                <xsl:value-of select="@height"/>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="class">
              <xsl:text>embed-responsive-item</xsl:text>
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text> </xsl:text>
      </iframe>
    </div>
  </xsl:template>

  <!-- Vimeo video -->
  <xsl:template match="Content[@moduleType='Video' and @videoType='Vimeo']" mode="displayBrief">
    <xsl:variable name="code">
      <xsl:variable name="raw" select="Vimeo/@code"/>
      <xsl:choose>
        <xsl:when test="contains($raw, 'vimeo.com/video/')">
          <xsl:value-of select="substring(substring-after($raw, 'vimeo.com/video/'), 1, 9)"/>
        </xsl:when>
        <xsl:when test="contains($raw, 'vimeo.com/')">
          <xsl:value-of select="substring(substring-after($raw, 'vimeo.com/'), 1, 9)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$raw"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div id="Video{@id}" class="Video">
      <xsl:if test="@size!='Manual'">
        <xsl:attribute name="class">
          <xsl:text>embed-responsive </xsl:text>
          <xsl:choose>
            <xsl:when test="@ratio='FourThree'">embed-responsive-4by3</xsl:when>
            <xsl:otherwise>embed-responsive-16by9</xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </xsl:if>
      <iframe frameborder="0" class="embed-responsive-item" allowfullscreen="allowfullscreen" >
        <xsl:attribute name="src">
          <xsl:text>//player.vimeo.com/video/</xsl:text>
          <xsl:value-of select="$code"/>
          <!-- Turn all options off by default -->
          <xsl:text>/?title=0&amp;byline=0&amp;portrait=0&amp;autoplay=0&amp;loop=0</xsl:text>
          <xsl:if test="Vimeo/@title='true'">&amp;title=1</xsl:if>
          <xsl:if test="Vimeo/@byline='true'">&amp;byline=1</xsl:if>
          <xsl:if test="Vimeo/@portrait='true'">&amp;portrait=1</xsl:if>
          <xsl:if test="Vimeo/@autoplay='true'">&amp;autoplay=1</xsl:if>
          <xsl:if test="Vimeo/@loop='true'">&amp;loop=1</xsl:if>
        </xsl:attribute>
        <xsl:choose>
          <xsl:when test="@size='Manual'">
            <xsl:if test="@width!=''">
              <xsl:attribute name="width">
                <xsl:value-of select="@width"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="@height!=''">
              <xsl:attribute name="height">
                <xsl:value-of select="@height"/>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="class">
              <xsl:text>embed-responsive-item</xsl:text>
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text> </xsl:text>
      </iframe>
    </div>
  </xsl:template>

  <!--HTML5-->
  <xsl:template match="Content[@moduleType='Video' and @videoType='HTML5']" mode="displayBrief">
    <xsl:if test="HTML5/@videoMp4!='' or HTML5/@videoGG!='' or  HTML5/@videoWebm!=''">
      <div>
        <xsl:if test="@size!='Manual'">
          <xsl:attribute name="class">
            <xsl:text>embed-responsive </xsl:text>
            <xsl:choose>
              <xsl:when test="@ratio='FourThree'">embed-responsive-4by3</xsl:when>
              <xsl:otherwise>embed-responsive-16by9</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
        </xsl:if>
        <video>
          <xsl:choose>
            <xsl:when test="@size='Manual'">
              <xsl:if test="@width!=''">
                <xsl:attribute name="width">
                  <xsl:value-of select="@width"/>
                </xsl:attribute>
              </xsl:if>
              <xsl:if test="@height!=''">
                <xsl:attribute name="height">
                  <xsl:value-of select="@height"/>
                </xsl:attribute>
              </xsl:if>
            </xsl:when>
            <xsl:otherwise>
              <xsl:attribute name="class">
                <xsl:text>embed-responsive-item</xsl:text>
              </xsl:attribute>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:if test="HTML5/@autoplay='autoplay'">
            <xsl:attribute name="autoplay">
              <xsl:text>autoplay</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@controls='controls'">
            <xsl:attribute name="controls">
              <xsl:text>controls</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@loop='loop'">
            <xsl:attribute name="loop">
              <xsl:text>loop</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@muted='muted'">
            <xsl:attribute name="muted">
              <xsl:text>muted</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@preload='auto'">
            <xsl:attribute name="preload">
              <xsl:value-of select="HTML5/@preload"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@preload='metadata'">
            <xsl:attribute name="preload">
              <xsl:value-of select="HTML5/@preload"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@preload='none'">
            <xsl:attribute name="preload">
              <xsl:value-of select="HTML5/@preload"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/img/@src!=''">
            <xsl:attribute name="poster">
              <xsl:value-of select="HTML5/img/@src"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@videoMp4!=''">
            <source src="{HTML5/@videoMp4}" type="video/mp4"/>
          </xsl:if>
          <xsl:if test="HTML5/@videoGG!=''">
            <source src="{HTML5/@videoGG}" type="video/ogg"/>
          </xsl:if>
          <xsl:if test="HTML5/@videoWebm!=''">
            <source src="{HTML5/@videoWebm}" type="video/webm"/>
          </xsl:if>
        </video>
      </div>
    </xsl:if>
  </xsl:template>

  <!-- Video Detail -->
  <xsl:template match="Content[@type='Video']" mode="VideoDetailDisplay">
    <xsl:param name="classes"/>
    <xsl:apply-templates select="." mode="inlinePopupOptions">
      <xsl:with-param name="class" select="$classes"/>
    </xsl:apply-templates>
    <div id="Video{@id}" class="Video">
      <xsl:attribute name="class">
        <xsl:apply-templates select="." mode="videoClasses"/>
      </xsl:attribute>
      <div id="FVPlayer{@id}" >
        <xsl:if test="VideoSize!='Manual'">
          <xsl:attribute name="class">
            <xsl:text>embed-responsive </xsl:text>
            <xsl:choose>
              <xsl:when test="VideoRatio='FourThree'">embed-responsive-4by3</xsl:when>
              <xsl:otherwise>embed-responsive-16by9 2</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
        </xsl:if>
        <a href="http://www.adobe.com/go/getflashplayer">
          <xsl:call-template name="term2004" />
        </a>
        <xsl:text>&#160;</xsl:text>
        <xsl:call-template name="term2005" />
        <xsl:if test="Local/img/@src!=''">
          <xsl:apply-templates select="Local/img" mode="cleanXhtml"/>
        </xsl:if>
      </div>
      <script type="text/javascript">
        <xsl:text>var s1 = new SWFObject('/ewcommon/flash/flvplayer.swf','Flash_</xsl:text>
        <xsl:value-of select="@id"/>
        <xsl:text>', 'auto</xsl:text>

        <xsl:text>', 'auto</xsl:text>

        <xsl:text>', '7', '7');</xsl:text>
        <xsl:text>s1.addParam('allowfullscreen', 'true');</xsl:text>
        <xsl:text>s1.addParam('wmode', 'transparent');</xsl:text>
        <xsl:choose>
          <xsl:when test="Local/@url!=''">
            <xsl:text>s1.addVariable('file','</xsl:text>
            <xsl:value-of select="Local/@url"/>
            <xsl:text>');</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>s1.addVariable('file','</xsl:text>
            <xsl:value-of select="Movies/filename/@src"/>
            <xsl:text>');</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:if test="Local/img/@src!=''">
          <xsl:text>s1.addVariable('image','</xsl:text>
          <xsl:value-of select="Local/img/@src"/>
          <xsl:text>');</xsl:text>
        </xsl:if>
        <xsl:text>s1.write('FVPlayer</xsl:text>
        <xsl:value-of select="@id"/>
        <xsl:text>');</xsl:text>

        <xsl:text>videoSizeAuto();</xsl:text>
      </script>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='Video' and VideoType='HTML5']" mode="VideoDetailDisplay">
    <xsl:apply-templates select="." mode="inlinePopupOptions"/>
    <xsl:if test="HTML5/@videoMp4!='' or HTML5/@videoGG!='' or  HTML5/@videoWebm!=''">
      <div>
        <xsl:if test="VideoSize!='Manual'">
          <xsl:attribute name="class">
            <xsl:text>embed-responsive </xsl:text>
            <xsl:choose>
              <xsl:when test="VideoRatio='FourThree'">embed-responsive-4by3</xsl:when>
              <xsl:otherwise>embed-responsive-16by9 1</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
        </xsl:if>
        <video>
          <xsl:choose>
            <xsl:when test="VideoSize='Manual'">
              <xsl:if test="VideoWidth!=''">
                <xsl:attribute name="width">
                  <xsl:value-of select="VideoWidth"/>
                </xsl:attribute>
              </xsl:if>
              <xsl:if test="VideoHeight!=''">
                <xsl:attribute name="height">
                  <xsl:value-of select="VideoHeight"/>
                </xsl:attribute>
              </xsl:if>
            </xsl:when>
            <xsl:otherwise>
              <xsl:attribute name="class">
                <xsl:text>embed-responsive-item</xsl:text>
              </xsl:attribute>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:if test="HTML5/@autoplay='autoplay'">
            <xsl:attribute name="autoplay">
              <xsl:text>autoplay</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@controls='controls'">
            <xsl:attribute name="controls">
              <xsl:text>controls</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@loop='loop'">
            <xsl:attribute name="loop">
              <xsl:text>loop</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@muted='muted'">
            <xsl:attribute name="muted">
              <xsl:text>muted</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@preload='auto'">
            <xsl:attribute name="preload">
              <xsl:value-of select="HTML5/@preload"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@preload='metadata'">
            <xsl:attribute name="preload">
              <xsl:value-of select="HTML5/@preload"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@preload='none'">
            <xsl:attribute name="preload">
              <xsl:value-of select="HTML5/@preload"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/img/@src!=''">
            <xsl:attribute name="poster">
              <xsl:value-of select="HTML5/img/@src"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@videoMp4!=''">
            <source src="{HTML5/@videoMp4}" type="video/mp4"/>
          </xsl:if>
          <xsl:if test="HTML5/@videoGG!=''">
            <source src="{HTML5/@videoGG}" type="video/ogg"/>
          </xsl:if>
          <xsl:if test="HTML5/@videoWebm!=''">
            <source src="{HTML5/@videoWebm}" type="video/webm"/>
          </xsl:if>
        </video>
      </div>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Content[@type='Video' and VideoType='Vimeo']" mode="VideoDetailDisplay">
    <xsl:variable name="code">
      <xsl:variable name="raw" select="Vimeo/@code"/>
      <xsl:choose>
        <xsl:when test="contains($raw, 'vimeo.com/video/')">
          <xsl:value-of select="substring(substring-after($raw, 'vimeo.com/video/'), 1, 9)"/>
        </xsl:when>
        <xsl:when test="contains($raw, 'vimeo.com/')">
          <xsl:value-of select="substring(substring-after($raw, 'vimeo.com/'), 1, 9)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$raw"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:apply-templates select="." mode="inlinePopupOptions"/>
    <div id="Video{@id}" class="Video">
      <xsl:if test="VideoSize!='Manual'">
        <xsl:attribute name="class">
          <xsl:text>embed-responsive </xsl:text>
          <xsl:choose>
            <xsl:when test="VideoRatio='FourThree'">embed-responsive-4by3</xsl:when>
            <xsl:otherwise>embed-responsive-16by9 </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </xsl:if>
      <iframe frameborder="0" class="embed-responsive-item" >
        <xsl:attribute name="src">
          <xsl:text>http://player.vimeo.com/video/</xsl:text>
          <xsl:value-of select="$code"/>
          <!-- Turn all options off by default -->
          <xsl:text>/?title=0&amp;byline=0&amp;portrait=0&amp;autoplay=0&amp;loop=0</xsl:text>
          <xsl:if test="Vimeo/@title='true'">&amp;title=1</xsl:if>
          <xsl:if test="Vimeo/@byline='true'">&amp;byline=1</xsl:if>
          <xsl:if test="Vimeo/@portrait='true'">&amp;portrait=1</xsl:if>
          <xsl:if test="Vimeo/@autoplay='true'">&amp;autoplay=1</xsl:if>
          <xsl:if test="Vimeo/@loop='true'">&amp;loop=1</xsl:if>
        </xsl:attribute>
        <xsl:choose>
          <xsl:when test="VideoSize='Manual'">
            <xsl:if test="@width!=''">
              <xsl:attribute name="width">
                <xsl:value-of select="VideoWidth"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="VideoHeight!=''">
              <xsl:attribute name="height">
                <xsl:value-of select="VideoHeight"/>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="class">
              <xsl:text>embed-responsive-item</xsl:text>
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text> </xsl:text>
      </iframe>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='Video' and VideoType='YouTube']" mode="VideoDetailDisplay">
    <xsl:variable name="code">
      <xsl:variable name="raw" select="YouTube/@code"/>
      <xsl:choose>
        <!-- http://youtu.be/abcd1234 -->
        <xsl:when test="contains($raw, 'youtu.be/')">
          <xsl:value-of select="substring(substring-after($raw, 'youtu.be/'), 1, 11)"/>
        </xsl:when>
        <!-- http://youtube.com/watch?v=abcd1234 -->
        <xsl:when test="contains($raw, 'v=')">
          <xsl:value-of select="substring(substring-after($raw, 'v='), 1, 11)"/>
        </xsl:when>
        <xsl:when test="contains($raw, 'youtube.com/embed/')">
          <xsl:value-of select="substring(substring-after($raw, 'youtube.com/embed/'), 1, 11)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="substring($raw, 1, 11)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:apply-templates select="." mode="inlinePopupOptions"/>
    <div id="Video{@id}" class="Video">
      <xsl:if test="VideoSize!='Manual'">
        <xsl:attribute name="class">
          <xsl:text>embed-responsive </xsl:text>
          <xsl:choose>
            <xsl:when test="VideoRatio='FourThree'">embed-responsive-4by3</xsl:when>
            <xsl:otherwise>embed-responsive-16by9 </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </xsl:if>
      <iframe frameborder="0" class="embed-responsive-item" allowfullscreen="allowfullscreen">
        <xsl:attribute name="src">
          <xsl:text>http</xsl:text>
          <xsl:if test="YouTube/@useHttps='true'">
            <xsl:text>s</xsl:text>
          </xsl:if>
          <xsl:text>://www.youtube.com/embed/</xsl:text>
          <xsl:value-of select="$code"/>
          <xsl:text>?wmode=transparent&amp;rel=0</xsl:text>
          <xsl:if test="YouTube/@showSuggested='true'">&amp;rel=1</xsl:if>
        </xsl:attribute>
        <xsl:choose>
          <xsl:when test="VideoSize='Manual'">
            <xsl:if test="VideoWidth!=''">
              <xsl:attribute name="width">
                <xsl:value-of select="VideoWidth"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="VideoHeight!=''">
              <xsl:attribute name="height">
                <xsl:value-of select="VideoHeight"/>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="class">
              <xsl:text>embed-responsive-item</xsl:text>
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text> </xsl:text>
      </iframe>
    </div>
  </xsl:template>

  <!-- ##################################### -->
  <!-- ### End of Video module templates ### -->
  <!-- ##################################### -->
  
</xsl:stylesheet>
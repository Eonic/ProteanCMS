<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
  <!-- ############## News Articles ##############   -->
  
    
  
  <!-- NewsArticle Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='NewsList']" mode="displayBrief">
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
    <!--responsive columns variables-->
    
    <!--end responsive columns variables-->
    <!-- Output Module -->
    <div class="clearfix NewsList">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix NewsList content-scroller</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <div data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1" >
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
            <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefLinked">
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

  <xsl:template match="Content[@type='Module' and @moduleType='NewsListDateMenu']" mode="displayBrief">
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="dateQuery" select="@dateQuery"/>
    <div class="NewsListDateMenu" id="subMenu">
      <ul class="nav nav-pills nav-stacked">
        <xsl:apply-templates select="Menu/MenuItem" mode="submenuitem2"/>
      </ul>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='Module' and @title='NewsListDateMenu']" mode="moduleTitle">
    <xsl:variable name="dateQuery" select="$page/Contents/Content[@type='Module' and @moduleType='NewsListDateMenu']/@dateQuery"/>
    <xsl:variable name="dateTitle">
      <xsl:for-each select="$page/Contents/Content[@type='Module' and @moduleType='NewsListDateMenu']/Menu/MenuItem[@id=$dateQuery]">
        <xsl:value-of select="@name"/>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="title">
      <span>
        <xsl:value-of select="$dateTitle"/>
        <xsl:text> </xsl:text>
      </span>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="@iconStyle='Centre'">
        <div class="center-block">
          <xsl:if test="@icon!=''">
            <i>
              <xsl:attribute name="class">
                <xsl:text>fa fa-3x center-block </xsl:text>
                <xsl:value-of select="@icon"/>
              </xsl:attribute>
              <xsl:text> </xsl:text>
            </i>
            <xsl:text> </xsl:text>
          </xsl:if>
          <xsl:if test="@uploadIcon!='' and @uploadIcon!='_'">
            <span class="upload-icon">
              <img src="{@uploadIcon}" alt="icon" class="center-block img-responsive"/>
            </span>
          </xsl:if>
          <xsl:if test="@title!=''">
            <span>
              <xsl:copy-of select="ms:node-set($title)" />
              <xsl:text> </xsl:text>
            </span>
          </xsl:if>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <xsl:if test="@icon!=''">
          <i>
            <xsl:attribute name="class">
              <xsl:text>fa </xsl:text>
              <xsl:value-of select="@icon"/>
            </xsl:attribute>
            <xsl:text> </xsl:text>
          </i>
          <span class="space">&#160;</span>
        </xsl:if>
        <xsl:if test="@uploadIcon!='' and @uploadIcon!='_'  and @uploadIcon!=' '">
          <img src="{@uploadIcon}" alt="icon"/>
        </xsl:if>
        <xsl:copy-of select="ms:node-set($title)" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="MenuItem" mode="submenuitem2">
    <xsl:variable name="dateQuery" select="ancestor::Content/@dateQuery"/>
    <li>
      <xsl:variable name="class">
        <xsl:if test="position()=1">
          <xsl:text>first </xsl:text>
        </xsl:if>
        <xsl:if test="position()=last()">
          <xsl:text>last </xsl:text>
        </xsl:if>
        <xsl:if test="@id=$dateQuery">
          <xsl:text>active </xsl:text>
        </xsl:if>
        <xsl:if test="descendant::MenuItem[@id=$dateQuery] and @url!='/'">
          <xsl:text>active </xsl:text>
        </xsl:if>
      </xsl:variable>
      <xsl:apply-templates select="self::MenuItem" mode="menuLink">
        <xsl:with-param name="class" select="$class"/>
      </xsl:apply-templates>
      <xsl:if test="count(child::MenuItem[not(DisplayName/@exclude='true')])&gt;0 and descendant-or-self::MenuItem[@id=/Page/@id]">
        <ul>
          <xsl:attribute name="class">
            <xsl:text>nav nav-pills</xsl:text>
            <!--TS Theme specfic setting must not be here - Moved to Layout XSL -->
          </xsl:attribute>
          <xsl:apply-templates select="MenuItem[not(DisplayName/@exclude='true')]" mode="submenuitem"/>
        </ul>
      </xsl:if>
    </li>
  </xsl:template>

  <!-- NewsArticle Brief -->
  <xsl:template match="Content[@type='NewsArticle']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <!-- articleBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="col listItem newsarticle">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'col listItem newsarticle'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <xsl:if test="Images/img/@src!=''">
          <a href="{$parentURL}" title="Read More - {Headline/node()}">
            <xsl:apply-templates select="." mode="displayThumbnail"/>
          </a>
          <!--Accessiblity fix : Separate adjacent links with more than whitespace-->
          <span class="hidden">|</span>
        </xsl:if>
        <h3 class="title" itemprop="headline">
          <a href="{$parentURL}" title="Read More - {Headline/node()}">
            <xsl:apply-templates select="." mode="getDisplayName"/>
          </a>
        </h3>
        <span class="hidden" itemtype="Organization" itemprop="publisher">
          <span itemprop="name">
            <xsl:value-of select="$sitename"/>
          </span>
        </span>
        <xsl:apply-templates select="Content[@type='Contact' and @rtype='Author'][1]" mode="displayAuthorBrief"/>
        <xsl:if test="@publish!=''">
          <p class="date" itemprop="datePublished">
            <xsl:value-of select="/Page/Contents/Content[@name='articleLabel']"/>
            <xsl:call-template name="DisplayDate">
              <xsl:with-param name="date" select="@publish"/>
            </xsl:call-template>
          </p>
        </xsl:if>
        <xsl:if test="@update!=''">
          <p class="hidden" itemprop="dateModified">
            <xsl:value-of select="/Page/Contents/Content[@name='articleLabel']"/>
            <xsl:call-template name="DisplayDate">
              <xsl:with-param name="date" select="@update"/>
            </xsl:call-template>
          </p>
        </xsl:if>

        <xsl:if test="Strapline/node()!=''">
          <div class="summary" itemprop="description">
            <xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
          </div>
        </xsl:if>
        <div class="entryFooter">
          <xsl:apply-templates select="." mode="displayTags"/>
          <xsl:apply-templates select="." mode="moreLink">
            <xsl:with-param name="link" select="$parentURL"/>
            <xsl:with-param name="altText">
              <xsl:value-of select="Headline/node()"/>
            </xsl:with-param>
          </xsl:apply-templates>
          <xsl:text> </xsl:text>
        </div>
      </div>
      <!-- Accessiblity fix : Separate adjacent links with more than whitespace -->
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='NewsArticle']" mode="displayBriefLinked">
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
        <xsl:with-param name="class" select="'listItem list-group-item newsarticle'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>

      </xsl:apply-templates>
      <xsl:choose>
        <xsl:when test="Strapline/descendant-or-self::a">
          <div class="straphaslinks">
            <xsl:if test="not(substring($link,1,1)='/') and (contains($link,'http://') and $linkType='external')">
              <xsl:attribute name="rel">external</xsl:attribute>
              <xsl:attribute name="class">extLink listItem list-group-item newsarticle straphaslinks</xsl:attribute>
            </xsl:if>
            <div class="lIinner">
              <h3 class="title">
                <a href="{$parentURL}">
                  <xsl:apply-templates select="." mode="getDisplayName"/>
                </a>
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
          </div>
        </xsl:when>
        <xsl:otherwise>
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
              <xsl:apply-templates select="." mode="displayTagsNoLink"/>
            </div>
          </a>
        </xsl:otherwise>
      </xsl:choose>

    </div>
  </xsl:template>

  <!-- NewsArticle Detail -->
  <xsl:template match="Content[@type='NewsArticle']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <xsl:variable name="debugMode">
      <xsl:call-template name="getXmlSettings">
        <xsl:with-param name="sectionName">web</xsl:with-param>
        <xsl:with-param name="valueName">debug</xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <div class="detail newsarticle">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail newsarticle'"/>
      </xsl:apply-templates>
      <h2 class="entry-title content-title" itemprop="headline">
        <xsl:apply-templates select="." mode="getDisplayName" />
      </h2>
      <xsl:apply-templates select="." mode="displayDetailImage"/>
      <xsl:apply-templates select="Content[@type='Contact']" mode="displayAuthor"/>
      <xsl:if test="@publish!=''">
        <p class="dtstamp" title="{@publish}" itemprop="datePublished">
          <xsl:call-template name="DisplayDate">
            <xsl:with-param name="date" select="@publish"/>
          </xsl:call-template>
        </p>
      </xsl:if>
      <span class="strapline-detail" itemprop="description">
        <xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
      </span>
      <div class="description entry-content" itemprop="text">
        <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
      </div>
      <xsl:if test="Content[@type='FAQ']">
        <div class="faq-list">
          <a name="pageTop" class="pageTop">&#160;</a>
          <h3>Question and Answer</h3>
          <ul>
            <xsl:apply-templates select="Content[@type='FAQ']" mode="displayFAQMenu"/>
          </ul>
          <xsl:apply-templates select="Content[@type='FAQ']" mode="displayBrief">
            <xsl:with-param name="sortBy" select="@sortBy"/>
          </xsl:apply-templates>
        </div>
      </xsl:if>
      <div class="entryFooter">
        <div class="tags">
          <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
          <xsl:text> </xsl:text>
        </div>
        <xsl:apply-templates select="." mode="backLink">
          <xsl:with-param name="link" select="$thisURL"/>
          <xsl:with-param name="altText">
            <xsl:call-template name="term2006" />
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <xsl:apply-templates select="." mode="ContentDetailCommenting">
        <xsl:with-param name="commentPlatform" select="$page/Contents/Content[@moduleType='NewsList']/@commentPlatform"/>
      </xsl:apply-templates>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='NewsArticle' and ancestor::ContentDetail]" mode="JSONLD">
    [ { "@context": "https://schema.org",
    "@type": "BlogPosting",
    "mainEntityOfPage": {
    "@type": "WebPage",
    "@id": "<xsl:value-of select="$href"/>"
    },
    "headline": "<xsl:apply-templates select="." mode="getDisplayName" />",
    "alternativeHeadline": "<xsl:call-template name="escape-json">
      <xsl:with-param name="string">
        <xsl:apply-templates select="Strapline" mode="flattenXhtml"/>
      </xsl:with-param>
    </xsl:call-template>",
    "image": "<xsl:value-of select="Images/img[@class='detail']/@src"/>",
    <xsl:if test="Content[@type='Tag']">
      <xsl:for-each select="Content[@type='Tag'][1]">
        <xsl:text>"genre": "</xsl:text>
        <xsl:value-of select="Name"/>
        <xsl:text>",
        </xsl:text>
      </xsl:for-each>
    </xsl:if>
    <xsl:if test="@metaKeywords!=''">
      "keywords": "<xsl:value-of select="@metaKeywords"/>",
    </xsl:if>
    "publisher": {
    "@type": "Organization",
    "name": "<xsl:value-of select="$siteName"/>",
    "logo":{
    "@type": "ImageObject",
    "name": "<xsl:value-of select="$siteName"/> Logo",
    "url": "<xsl:value-of select="$siteURL"/><xsl:value-of select="$siteLogo"/>"
    }},
    "url": "<xsl:value-of select="$href"/>",
    "datePublished": "<xsl:value-of select="@publish"/>",
    "dateCreated": "<xsl:value-of select="@publish"/>",
    "dateModified": "<xsl:value-of select="@update"/>",
    <xsl:if test="@metaDescription!=''">
      "description": "<xsl:value-of select="@metaDescription"/>",
    </xsl:if>
    "articleBody": "<xsl:call-template name="escape-json">
      <xsl:with-param name="string">
        <xsl:apply-templates select="Body/*" mode="flattenXhtml"/>
      </xsl:with-param>
    </xsl:call-template>"
    <xsl:if test="Content[@type='Contact' and @rtype='Author']">
      ,
      <xsl:apply-templates select="Content[@type='Contact' and @rtype='Author']" mode="JSONLD"/>
    </xsl:if>}
    <xsl:if test="Content[@type='FAQ']">
      ,  { "@context": "https://schema.org",
      "@type": "FAQPage",
      "mainEntity": [
      <xsl:apply-templates select="Content[@type='FAQ']" mode="JSONLD-list"/>
      ]
      }
    </xsl:if>
    ]
  </xsl:template>

  <xsl:template match="Content[@type='Contact' and ancestor::Content[@type='NewsArticle']]" mode="JSONLD">
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="self::Content" mode="getHref">
        <xsl:with-param name="parId" select="@parId"/>
      </xsl:apply-templates>
    </xsl:variable>
    "author": {
    "@type": "Person",
    "name": "<xsl:value-of select="GivenName"/><xsl:text> </xsl:text><xsl:value-of select="Surname"/>",
    "jobTitle": "<xsl:value-of select="Title"/>",
    "image": "<xsl:value-of select="$siteURL"/><xsl:value-of select="Images/img[@class='detail']/@src"/>",
    "url": "<xsl:value-of select="$parentURL"/>",
    "sameAs" : [
    <xsl:if test="@facebookURL!=''">
      "<xsl:value-of select="@facebookURL"/>"
    </xsl:if>
    <xsl:if test="@linkedInURL!=''">
      "<xsl:value-of select="@linkedInURL"/>"
    </xsl:if>
    <xsl:if test="@twitterURL!=''">
      "<xsl:value-of select="@twitterURL"/>"
    </xsl:if>
    <xsl:if test="@instagramURL!=''">
      "<xsl:value-of select="@instagramURL"/>"
    </xsl:if>]
    }
  </xsl:template>

  <xsl:template match="Content" mode="ContentDetailCommenting">
    <xsl:param name="commentPlatform"/>
    <xsl:variable name="debugMode">
      <xsl:call-template name="getXmlSettings">
        <xsl:with-param name="sectionName">web</xsl:with-param>
        <xsl:with-param name="valueName">debug</xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$commentPlatform='facebook'">
        <div class="fb-comments" data-href="{$href}" data-num-posts="3" data-width="{$page/Contents/Content[@moduleType='NewsList']/@fbCommentsWidth}">
          <xsl:text> </xsl:text>
        </div>
      </xsl:when>
      <xsl:when test="$commentPlatform='disqus'">
        <div id="disqus_thread">
          <xsl:text> </xsl:text>
        </div>
        <script type="text/javascript">
          /* * * CONFIGURATION VARIABLES: EDIT BEFORE PASTING INTO YOUR WEBPAGE * * */
          var disqus_shortname = '<xsl:value-of select="$page/Contents/Content[@moduleType='NewsList' and @commentPlatform='disqus']/@disqusShortname"/>'; // required: replace example with your forum shortname
          var disqus_identifier = 'pageid-<xsl:value-of select="$page/@id"/>-artid-<xsl:value-of select="@id"/>';
          var disqus_title = '<xsl:apply-templates select="." mode="getDisplayName" />';
          var disqus_url = 'http://<xsl:value-of select="$siteURL"/><xsl:value-of select="$href"/>';
          <xsl:choose>
            <xsl:when test="$debugMode='on'">var disqus_developer = 1;</xsl:when>
            <xsl:otherwise>var disqus_developer = 0;</xsl:otherwise>
          </xsl:choose>

          /* * * DON'T EDIT BELOW THIS LINE * * */
          (function() {
          var dsq = document.createElement('script'); dsq.type = 'text/javascript'; dsq.async = true;
          dsq.src = 'http://' + disqus_shortname + '.disqus.com/embed.js';
          (document.getElementsByTagName('head')[0] || document.getElementsByTagName('body')[0]).appendChild(dsq);
          })();
        </script>
        <noscript>
          Please enable JavaScript to view the <a href="http://disqus.com/?ref_noscript">comments powered by Disqus.</a>
        </noscript>
        <a href="http://disqus.com" class="dsq-brlink">
          comments powered by <span class="logo-disqus">Disqus</span>
        </a>
      </xsl:when>
      <xsl:when test="$commentPlatform='livefyre'">
        <!-- START: Livefyre Embed -->
        <div id="livefyre-comments">
          <xsl:text> </xsl:text>
        </div>
        <script type="text/javascript" src="//zor.livefyre.com/wjs/v3.0/javascripts/livefyre.js">
          <xsl:text> </xsl:text>
        </script>
        <script type="text/javascript">
          (function () {
          var articleId = fyre.conv.load.makeArticleId(null);
          fyre.conv.load({}, [{
          el: 'livefyre-comments',
          network: "livefyre.com",
          siteId: "<xsl:value-of select="$page/Contents/Content[@moduleType='NewsList' and @commentPlatform='livefyre']/@livefyreID"/>",
          articleId: articleId,
          signed: false,
          collectionMeta: {
          articleId: articleId,
          url: fyre.conv.load.makeCollectionUrl(),
          }
          }], function() {});
          }());
        </script>
        <!-- END: Livefyre Embed -->
      </xsl:when>
      <xsl:when test="$commentPlatform='intensedebate'">
        <script>
          var idcomments_acct = '<xsl:value-of select="$page/Contents/Content[@moduleType='NewsList' and @commentPlatform='intensedebate']/@intenseDebateID"/>';
          var idcomments_post_id = 'pageid-<xsl:value-of select="$page/@id"/>-artid-<xsl:value-of select="@id"/>';
          var idcomments_post_url = 'http://<xsl:value-of select="$siteURL"/><xsl:value-of select="$href"/>';
          var idcomments_post_title = '<xsl:apply-templates select="." mode="getDisplayName" />';
        </script>
        <span id="IDCommentsPostTitle" style="display:none">
          <xsl:text> </xsl:text>
        </span>
        <script type='text/javascript' src='//www.intensedebate.com/js/genericCommentWrapperV2.js'></script>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">


  <!-- All the Social JS files -->
  <xsl:template name="initialiseSocialBookmarks">
    <xsl:if test="not(/Page/Cart) or not(/Page/Cart/Order/@cmd!='')">
      <!-- facebook -->
      <xsl:variable name="fbAppId">
        <xsl:choose>
          <xsl:when test="$page/Contents/Content[@name='fb-app_id']">
            <xsl:value-of select="$page/Contents/Content[@name='fb-app_id']"/>
          </xsl:when>
          <xsl:otherwise>176558699067891</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <div id="fb-root">
        <xsl:text> </xsl:text>
      </div>
      <script>
        (function(d, s, id) {
        var js, fjs = d.getElementsByTagName(s)[0];
        if (d.getElementById(id)) return;
        js = d.createElement(s); js.id = id;
        js.src = "//connect.facebook.net/en_GB/all.js#xfbml=1&amp;appId=<xsl:value-of select="$fbAppId"/>";
        fjs.parentNode.insertBefore(js, fjs);
        }(document, 'script', 'facebook-jssdk'));
      </script>
      <!-- google -->
      <!-- Place this tag where you want the +1 button to render. -->
      <!-- Place this tag after the last +1 button tag. -->
      <script type="text/javascript">
        <xsl:text>(function() {</xsl:text>
        <xsl:text>var po = document.createElement('script'); po.type = 'text/javascript'; po.async = true;</xsl:text>
        <xsl:text>po.src = 'https://apis.google.com/js/plusone.js';</xsl:text>
        <xsl:text>var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(po, s);</xsl:text>
        <xsl:text>})();</xsl:text>
      </script>
      <!-- Twitter -->
      <script type="text/javascript" src="//platform.twitter.com/widgets.js">&#160;</script>
      <!-- LinkedIn -->
      <script type="text/javascript" src="//platform.linkedin.com/in.js">&#160;</script>
      <!-- Pinterest -->
      <script type="text/javascript" src="//assets.pinterest.com/js/pinit.js">&#160;</script>
    </xsl:if>
  </xsl:template>

  <xsl:template match="@*" mode="getBookmarkURL">
    <xsl:if test="$siteURL=''">
      <xsl:text>http</xsl:text>
      <xsl:if test="$page/Request/ServerVariables/Item[@name='HTTPS']='on'">s</xsl:if>
      <xsl:text>://</xsl:text>
      <xsl:value-of select="$page/Request/ServerVariables/Item[@name='SERVER_NAME']"/>
    </xsl:if>
    <xsl:choose>
      <xsl:when test="$page/ContentDetail">
        <xsl:apply-templates select="$page/ContentDetail/Content" mode="getHref" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="$currentPage" mode="getHref" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="@*" mode="getBookmarkTitle">
    <xsl:choose>
      <xsl:when test="$page/ContentDetail">
        <xsl:apply-templates select="$page/ContentDetail/Content" mode="getDisplayName" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="$currentPage" mode="getDisplayName" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!--  =====================================================================================  -->
  <!--  ==  Social Bookmarks  ===============================================================  -->
  <!--  =====================================================================================  -->

  <!-- module -->
  <xsl:template match="Content[@moduleType='SocialBookmarks']" mode="displayBrief">
    <div class="moduleBookmarks">
      <xsl:variable name="bookmarkSettings">
        <xsl:choose>
          <xsl:when test="@bookmarkSettings='global' and $page/Contents/Content[@type='SocialNetworkingSettings']">
            <xsl:copy-of select="$page/Contents/Content[@type='SocialNetworkingSettings']/Bookmarks"/>
          </xsl:when>
          <xsl:when test="@bookmarkSettings='this' and Content[@type='SocialNetworkingSettings']">
            <xsl:copy-of select="Content[@type='SocialNetworkingSettings']/Bookmarks"/>
          </xsl:when>
          <xsl:otherwise>default</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:apply-templates select="." mode="displayBookmarks">
        <xsl:with-param name="bookmarkSettings" select="ms:node-set($bookmarkSettings)/Bookmarks" />
        <xsl:with-param name="type" select="'MenuItem'" />
      </xsl:apply-templates>
    </div>
  </xsl:template>

  <!-- page -->
  <xsl:template match="Page" mode="socialBookmarks">
    <!-- if bookmarks enabled & NOT set to use modules to do it -->
    <xsl:if test="$page/Contents/Content[@type='SocialNetworkingSettings' and Bookmarks/MenuItem/@position!='module']">
      <xsl:variable name="bookmarkSettings" select="$page/Contents/Content[@type='SocialNetworkingSettings']/Bookmarks" />
      <!-- uses this span to re-write around the page -->
      <span>
        <xsl:attribute name="class">
          <xsl:text>bookmarkPlacement </xsl:text>
          <xsl:value-of select="$bookmarkSettings/MenuItem/@position"/>
        </xsl:attribute>
        <xsl:apply-templates select="." mode="displayBookmarks">
          <xsl:with-param name="bookmarkSettings" select="$bookmarkSettings" />
          <xsl:with-param name="type" select="'MenuItem'" />
        </xsl:apply-templates>
      </span>
      <div class="terminus">&#160;</div>
    </xsl:if>
  </xsl:template>

  <!-- content -->
  <xsl:template match="Content" mode="socialBookmarks">
    <!-- if bookmarks enabled & NOT set to use modules to do it -->
    <xsl:if test="$page/Contents/Content[@type='SocialNetworkingSettings']">
      <xsl:variable name="bookmarkSettings" select="$page/Contents/Content[@type='SocialNetworkingSettings']/Bookmarks" />
      <!-- uses this span to re-write around the page -->
      <div>
        <xsl:attribute name="class">
          <xsl:text>bookmarkPlacement </xsl:text>
          <xsl:value-of select="$bookmarkSettings/Content/@position"/>
        </xsl:attribute>
        <xsl:apply-templates select="." mode="displayBookmarks">
          <xsl:with-param name="bookmarkSettings" select="$bookmarkSettings" />
          <xsl:with-param name="type" select="'Content'" />
        </xsl:apply-templates>
      </div>
      <div class="terminus">&#160;</div>
    </xsl:if>
  </xsl:template>

  <!-- display bookmarks -->
  <xsl:template match="*" mode="displayBookmarks">
    <xsl:param name="bookmarkSettings" />
    <xsl:param name="type" select="'MenuItem'"/>
    <xsl:variable name="layout">
      <xsl:value-of select="$bookmarkSettings/*[name()=$type]/@size" />
      <xsl:if test="$bookmarkSettings/*[name()=$type]/@size='standard' and $bookmarkSettings/*[name()=$type]/@count='true'">
        <xsl:text>-count</xsl:text>
      </xsl:if>
    </xsl:variable>
    <div class="socialBookmarks">
      <xsl:attribute name="class">
        <xsl:text>socialBookmarks bookmarks-</xsl:text>
        <xsl:value-of select="$layout" />
      </xsl:attribute>
      <xsl:apply-templates select="$page/Contents/Content[@type='SocialNetworkingSettings']" mode="inlinePopupOptions">
        <xsl:with-param name="class">
          <xsl:text>socialBookmarks bookmarks-</xsl:text>
          <xsl:value-of select="$layout" />
        </xsl:with-param>
      </xsl:apply-templates>
      <xsl:apply-templates select="$bookmarkSettings/Methods/@*[.='true']" mode="displayBookmark">
        <xsl:sort select="name()" order="descending" data-type="text"/>
        <xsl:with-param name="bookmarkSettings" select="$bookmarkSettings/*[name()=$type]" />
      </xsl:apply-templates>
      <xsl:text> </xsl:text>
    </div>
    <div class="terminus">&#160;</div>
  </xsl:template>

  <!-- Generic catch -->
  <xsl:template match="@*" mode="displayBookmark"></xsl:template>

  <!-- Facebook LIKE -->
  <xsl:template match="@facebook" mode="displayBookmark">
    <xsl:param name="bookmarkSettings" />
    <xsl:variable name="layout" select="$bookmarkSettings/@size"/>
    <span class="bookmark facebook-bookmark">
      <div class="fb-like" data-show-faces="false">
        <xsl:attribute name="data-width">
          <xsl:choose>
            <xsl:when test="$layout='large' or ($layout='standard' and $bookmarkSettings/@count='true')">
              <xsl:text>70</xsl:text>
            </xsl:when>
            <xsl:otherwise>
              <xsl:text>450</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <xsl:attribute name="data-href">
          <xsl:apply-templates select="." mode="getBookmarkURL"/>
        </xsl:attribute>
        <xsl:attribute name="data-layout">
          <xsl:choose>
            <xsl:when test="$layout='large'">box_count</xsl:when>
            <xsl:when test="$layout='standard' and $bookmarkSettings/@count='true'">button_count</xsl:when>
            <xsl:otherwise>standard</xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <xsl:if test="$layout!='large' and $bookmarkSettings/@count!='true'">
          <xsl:attribute name="data-send">false</xsl:attribute>
        </xsl:if>
        <xsl:if test="parent::Methods/@facebookShare='true'">
          <xsl:attribute name="data-share">
            <xsl:text>true</xsl:text>
          </xsl:attribute>
        </xsl:if>
        <!-- needed to keep tag open -->
        <xsl:text> </xsl:text>
      </div>
      <noscript>
        <a target="_blank">
          <xsl:attribute name="href">
            <xsl:text>http://www.facebook.com/sharer.php?u=</xsl:text>
            <xsl:apply-templates select="." mode="getBookmarkURL"/>
            <xsl:text>&amp;t=</xsl:text>
            <xsl:apply-templates select="." mode="getBookmarkTitle"/>
          </xsl:attribute>
          <img src="/ewcommon/images/social-bookmarks/facebook.gif" alt="Facebook" width="44" height="20"/>
        </a>
      </noscript>
    </span>
  </xsl:template>

  <!-- GOOGLE +1 -->
  <xsl:template match="@google" mode="displayBookmark">
    <xsl:param name="bookmarkSettings" />
    <xsl:variable name="layout" select="$bookmarkSettings/@size"/>
    <span class="bookmark google-bookmark">
      <!-- check footer JS for where +1 kicks off. -->
      <div class="g-plusone">
        <xsl:attribute name="data-size">
          <xsl:choose>
            <xsl:when test="$layout='large'">tall</xsl:when>
            <xsl:when test="$layout='standard' and $bookmarkSettings/@count='true'">medium</xsl:when>
            <xsl:otherwise>standard</xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <xsl:attribute name="count">
          <xsl:value-of select="$bookmarkSettings/@count"/>
        </xsl:attribute>
        <xsl:text> </xsl:text>
      </div>
      <noscript>
        <a target="_blank">
          <xsl:attribute name="href">
            <xsl:text>https://plusone.google.com/_/+1/confirm?hl=en&amp;url=</xsl:text>
            <xsl:apply-templates select="." mode="getBookmarkURL"/>
          </xsl:attribute>
          <img src="/ewcommon/images/social-bookmarks/google.gif" alt="Google +1" width="50" height="20"/>
        </a>
      </noscript>
    </span>
  </xsl:template>

  <!-- Twitter Tweek -->
  <xsl:template match="@twitter" mode="displayBookmark">
    <xsl:param name="bookmarkSettings" />
    <xsl:variable name="layout" select="$bookmarkSettings/@size"/>
    <span class="bookmark twitter-bookmark">
      <a href="http://twitter.com/share" class="twitter-share-button" target="_blank">
        <xsl:attribute name="data-url">
          <xsl:apply-templates select="." mode="getBookmarkURL" />
        </xsl:attribute>
        <xsl:attribute name="data-text">
          <xsl:apply-templates select="." mode="getBookmarkTitle" />
          <xsl:text> -</xsl:text>
        </xsl:attribute>
        <xsl:attribute name="data-count">
          <xsl:choose>
            <xsl:when test="$layout='large'">vertical</xsl:when>
            <xsl:when test="$bookmarkSettings/@count='true'">horizontal</xsl:when>
            <xsl:otherwise>none</xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <img src="/ewcommon/images/social-bookmarks/twitter.gif" alt="Twitter" width="55" height="20"/>
      </a>
    </span>
  </xsl:template>

  <!-- LinkedIn -->
  <xsl:template match="@linkedin" mode="displayBookmark">
    <xsl:param name="bookmarkSettings" />
    <xsl:variable name="layout" select="$bookmarkSettings/@size"/>
    <span class="bookmark linkedin-bookmark">
      <script type="in/share">
        <xsl:attribute name="data-url">
          <xsl:apply-templates select="." mode="getBookmarkURL" />
        </xsl:attribute>
        <xsl:attribute name="data-counter">
          <xsl:choose>
            <xsl:when test="$layout='large'">top</xsl:when>
            <xsl:when test="$bookmarkSettings/@count='true'">right</xsl:when>
            <xsl:otherwise></xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <xsl:text> </xsl:text>
      </script>
      <noscript>
        <a target="_blank">
          <xsl:attribute name="href">
            <xsl:text>http://www.linkedin.com/shareArticle?mini=true&amp;url=</xsl:text>
            <xsl:apply-templates select="." mode="getBookmarkURL"/>
            <xsl:text>&amp;title=</xsl:text>
            <xsl:apply-templates select="." mode="getBookmarkTitle"/>
          </xsl:attribute>
          <img src="/ewcommon/images/social-bookmarks/linkedin.gif" alt="LinkedIn" width="61" height="20"/>
        </a>
      </noscript>
    </span>
  </xsl:template>

  <!-- pinterest -->
  <xsl:template match="@pinterest" mode="displayBookmark">
    <xsl:param name="bookmarkSettings" />
    <xsl:variable name="layout" select="$bookmarkSettings/@size"/>
    <xsl:variable name="url">
      <xsl:apply-templates select="." mode="getBookmarkURL"/>
    </xsl:variable>
    <xsl:variable name="media">
      <xsl:text>http</xsl:text>
      <xsl:if test="$page/Request/ServerVariables/Item[@name='HTTPS']='on'">s</xsl:if>
      <xsl:text>://</xsl:text>
      <xsl:value-of select="$page/Request/ServerVariables/Item[@name='SERVER_NAME']"/>
      <xsl:for-each select="$page/ContentDetail/Content">
        <xsl:choose>
          <!-- IF Thumbnail use that -->
          <xsl:when test="Images/img[@class='thumbnail']/@src!=''">
            <xsl:value-of select="Images/img[@class='thumbnail']/@src"/>
          </xsl:when>
          <!-- IF Full Size use that -->
          <xsl:when test="Images/img[@class='detail']/@src!=''">
            <xsl:value-of select="Images/img[@class='detail']/@src"/>
          </xsl:when>
          <!-- ELSE use display -->
          <xsl:otherwise>
            <xsl:value-of select="Images/img[@class='display']/@src"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="description">
      <xsl:for-each select="$page/ContentDetail/Content">
        <xsl:choose>
          <!-- IF Thumbnail use that -->
          <xsl:when test="Images/img[@class='thumbnail']/@alt!=''">
            <xsl:value-of select="Images/img[@class='thumbnail']/@alt"/>
          </xsl:when>
          <!-- IF Full Size use that -->
          <xsl:when test="Images/img[@class='detail']/@alt!=''">
            <xsl:value-of select="Images/img[@class='detail']/@alt"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="@name"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="href">
      <xsl:text>http://pinterest.com/pin/create/button/?url=</xsl:text>
      <xsl:value-of select="$url"/>
      <xsl:text>&amp;media=</xsl:text>
      <xsl:value-of select="$media"/>
      <xsl:text>&amp;description=</xsl:text>
      <xsl:value-of select="$description"/>
    </xsl:variable>
    <xsl:variable name="data-count">
      <xsl:choose>
        <xsl:when test="$layout='large'">vertical</xsl:when>
        <xsl:when test="$bookmarkSettings/@count='true'">horizontal</xsl:when>
        <xsl:otherwise>none</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <span class="bookmark pinterest-bookmark">
      <a href="{$href}" class="pin-it-button" count-layout="{$data-count}">
        <img border="0" src="//assets.pinterest.com/images/PinExt.png" title="Pin It" />
      </a>
    </span>
  </xsl:template>


  <xsl:template match="Content[@moduleType='SocialLinks']" mode="displayBrief">
    <div class="moduleSocialLinks align-{@align}">
      <xsl:choose>
        <xsl:when test="@blank='true'">
          <xsl:apply-templates select="." mode="socialLinksBlank">
            <xsl:with-param name="iconSet" select="@iconSet"/>
            <xsl:with-param name="myName" select="@myName"/>
          </xsl:apply-templates>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="." mode="socialLinks">
            <xsl:with-param name="iconSet" select="@iconSet"/>
            <xsl:with-param name="myName" select="@myName"/>
          </xsl:apply-templates>
        </xsl:otherwise>
      </xsl:choose>
    </div>
  </xsl:template>

  <!-- module -->
  <xsl:template match="Content | ContactPoint" mode="socialLinksBlank">
    <xsl:param name="myName"/>
    <xsl:param name="iconSet"/>
    <div class="socialLinks clearfix iconset-{$iconSet}">
      <xsl:choose>
        <xsl:when test="@uploadSprite!=''">
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" target="_blank" title="{$myName} on Facebook" id="social-id-fb" style="background-image:url({@uploadSprite})" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" target="_blank" title="{$myName} on Twitter" id="social-id-tw" style="background-image:url({@uploadSprite});background-position:128px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" target="_blank" title="{$myName} on LinkedIn" id="social-id-li" style="background-image:url({@uploadSprite});background-position:96px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" target="_blank" title="{$myName} on Google+" id="social-id-gp" style="background-image:url({@uploadSprite});background-position:64px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" target="_blank" title="{$myName} on Pinterest" id="social-id-pi" style="background-image:url({@uploadSprite});background-position:32px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" target="_blank" title="{$myName} on You Tube" id="social-id-yt" style="background-image:url({@uploadSprite});background-position:160px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" target="_blank" title="{$myName} on Instagram" id="social-id-ig" style="background-image:url({@uploadSprite});background-position:192px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$iconSet='icons'">
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" target="_blank" title="{$myName} on Facebook" id="social-id-fb">
              <i class="fab fa-2x fa-facebook">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" target="_blank" title="{$myName} on Twitter" id="social-id-tw">
              <i class="fab fa-2x fa-twitter">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" target="_blank" title="{$myName} on LinkedIn" id="social-id-li">
              <i class="fab fa-2x fa-linkedin">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" target="_blank" title="{$myName} on Google+" id="social-id-gp">
              <i class="fab fa-2x fa-google-plus">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" target="_blank" title="{$myName} on Pinterest" id="social-id-li">
              <i class="fab fa-2x fa-pinterest">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" target="_blank" title="{$myName} on Youtube" id="social-id-yt">
              <i class="fab fa-2x fa-youtube">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" target="_blank" title="{$myName} on Instagram" id="social-id-ig">
              <i class="fab fa-2x fa-instagram">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$iconSet='icons-square'">
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" target="_blank" title="{$myName} on Facebook" id="social-id-fb">
              <i class="fab fa-3x fa-facebook-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" target="_blank" title="{$myName} on Twitter" id="social-id-tw">
              <i class="fab fa-3x fa-twitter-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" target="_blank" title="{$myName} on LinkedIn" id="social-id-li">
              <i class="fab fa-3x fa-linkedin-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" target="_blank" title="{$myName} on Google+" id="social-id-gp">
              <i class="fab fa-3x fa-google-plus-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" target="_blank" title="{$myName} on Pinterest" id="social-id-pi">
              <i class="fab fa-3x fa-pinterest-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" target="_blank" title="{$myName} on Youtube" id="social-id-yt">
              <i class="fab fa-3x fa-youtube-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" target="_blank" title="{$myName} on Instagram" id="social-id-ig">
              <i class="fab fa-3x fa-instagram">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$iconSet='icons-circle'">
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" target="_blank" title="{$myName} on Facebook" id="social-id-fb">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fab fa-facebook fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" target="_blank" title="{$myName} on Twitter" id="social-id-tw">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fab fa-twitter fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" target="_blank" title="{$myName} on LinkedIn" id="social-id-li">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fab fa-linkedin fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" target="_blank" title="{$myName} on Google+" id="social-id-gp">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fab fa-google-plus fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" target="_blank" title="{$myName} on Pinterest" id="social-id-pi">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fab fa-pinterest fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" target="_blank" title="{$myName} on Youtube" id="social-id-yt">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fab fa-youtube fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" target="_blank" title="{$myName} on Instagram" id="social-id-ig">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fab fa-instagram fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$iconSet='plain'">
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" target="_blank" title="{$myName} on Facebook" class="social-id-fb">
              <i class="fa fa-facebook">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" target="_blank" title="{$myName} on Twitter" class="social-id-tw">
              <i class="fab fa-twitter">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" target="_blank" title="{$myName} on LinkedIn" class="social-id-li">
              <i class="fab fa-linkedin">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" target="_blank" title="{$myName} on Google+" class="social-id-gp">
              <i class="fab fa-google-plus">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" target="_blank" title="{$myName} on Pinterest" class="social-id-pi">
              <i class="fab fa-pinterest">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" target="_blank" title="{$myName} on Youtube" class="social-id-yt">
              <i class="fab fa-youtube ">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" target="_blank" title="{$myName} on Instagram" id="social-id-ig">
              <i class="fab fa-instagram">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
        </xsl:when>
        <xsl:otherwise>
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" target="_blank" title="{$myName} on Facebook" id="social-id-fb">
              <img src="/ewcommon/images/icons/social/{$iconSet}/facebook.png" alt="{$myName} on Facebook" title="Follow {$myName} on Facebook" />
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" target="_blank" title="{$myName} on Twitter" id="social-id-tw">
              <img src="/ewcommon/images/icons/social/{$iconSet}/twitter.png" alt="{$myName} on Twitter" title="Follow {$myName} on Twitter" />
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" target="_blank" title="{$myName} on LinkedIn" id="social-id-li">
              <img src="/ewcommon/images/icons/social/{$iconSet}/LinkedIn.png" alt="{$myName} on LinkedIn" title="Follow {$myName} on LinkedIn" />
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" target="_blank" title="{$myName} on Google+" id="social-id-gp">
              <img src="/ewcommon/images/icons/social/{$iconSet}/Googleplus.png" alt="{$myName} on Google+" title="Follow {$myName} on Google+" />
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" target="_blank" title="{$myName} on Pinterest" id="social-id-li">
              <img src="/ewcommon/images/icons/social/{$iconSet}/Pinterest.png" alt="{$myName} on Pinterest" title="Follow {$myName} on Pinterest" />
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" target="_blank" title="{$myName} on YouTube" id="social-id-yt">
              <img src="/ewcommon/images/icons/social/{$iconSet}/YouTube.png" alt="{$myName} on YouTube" title="Follow {$myName} on YouTube" />
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" target="_blank" title="{$myName} on Pinterest" id="social-id-ig">
              <img src="/ewcommon/images/icons/social/{$iconSet}/Instagram.png" alt="{$myName} on Instagram" title="Follow {$myName} on Instagram" />
            </a>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>
    </div>
  </xsl:template>
  <!-- module -->
  <xsl:template match="Content | ContactPoint" mode="socialLinks">
    <xsl:param name="myName"/>
    <xsl:param name="iconSet"/>
    <div class="socialLinks clearfix iconset-{$iconSet}">
      <xsl:choose>
        <xsl:when test="@uploadSprite!=''">
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" title="{$myName} on Facebook" id="social-id-fb" style="background-image:url({@uploadSprite})" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" title="{$myName} on Twitter" id="social-id-tw" style="background-image:url({@uploadSprite});background-position:128px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" title="{$myName} on LinkedIn" id="social-id-li" style="background-image:url({@uploadSprite});background-position:96px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" title="{$myName} on Google+" id="social-id-gp" style="background-image:url({@uploadSprite});background-position:64px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" title="{$myName} on Pinterest" id="social-id-pi" style="background-image:url({@uploadSprite});background-position:32px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" title="{$myName} on You Tube" id="social-id-yt" style="background-image:url({@uploadSprite});background-position:160px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" title="{$myName} on Instagram" id="social-id-ig" style="background-image:url({@uploadSprite});background-position:192px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$iconSet='icons'">
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" title="{$myName} on Facebook" id="social-id-fb">
              <i class="fab fa-2x fa-facebook">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" title="{$myName} on Twitter" id="social-id-tw">
              <i class="fab fa-2x fa-twitter">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" title="{$myName} on LinkedIn" id="social-id-li">
              <i class="fab fa-2x fa-linkedin">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" title="{$myName} on Google+" id="social-id-gp">
              <i class="fab fa-2x fa-google-plus">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" title="{$myName} on Pinterest" id="social-id-li">
              <i class="fab fa-2x fa-pinterest">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" title="{$myName} on Youtube" id="social-id-yt">
              <i class="fab fa-2x fa-youtube">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" title="{$myName} on Instagram" id="social-id-ig">
              <i class="fab fa-2x fa-instagram">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$iconSet='icons-square'">
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" title="{$myName} on Facebook" id="social-id-fb">
              <i class="fab fa-3x fa-facebook-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" title="{$myName} on Twitter" id="social-id-tw">
              <i class="fab fa-3x fa-twitter-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" title="{$myName} on LinkedIn" id="social-id-li">
              <i class="fab fa-3x fa-linkedin-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" title="{$myName} on Google+" id="social-id-gp">
              <i class="fab fa-3x fa-google-plus-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" title="{$myName} on Pinterest" id="social-id-pi">
              <i class="fab fa-3x fa-pinterest-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" title="{$myName} on Youtube" id="social-id-yt">
              <i class="fab fa-3x fa-youtube-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" title="{$myName} on Instagram" id="social-id-ig">
              <i class="fab fa-3x fa-instagram">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@spotifyURL!=''">
            <a href="{@spotifyURL}" title="{$myName} on Spotify" id="social-id-ig">
              <i class="fab fa-3x fa-spotify">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$iconSet='icons-circle'">
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" title="{$myName} on Facebook" id="social-id-fb">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fab fa-facebook-f fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" title="{$myName} on Twitter" id="social-id-tw">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fab fa-twitter fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" title="{$myName} on LinkedIn" id="social-id-li">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fab fa-linkedin-in fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" title="{$myName} on Google+" id="social-id-gp">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fab fa-google-plus fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" title="{$myName} on Pinterest" id="social-id-pi">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fab fa-pinterest fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" title="{$myName} on Youtube" id="social-id-yt">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fab fa-youtube fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" title="{$myName} on Instagram" id="social-id-ig">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fab fa-instagram fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@spotifyURL!=''">
            <a href="{@spotifyURL}" title="{$myName} on Spotify" id="social-id-ig">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fab fa-spotify fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$iconSet='plain'">
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" title="{$myName} on Facebook" class="social-id-fb">
              <i class="fab fa-facebook">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" title="{$myName} on Twitter" class="social-id-tw">
              <i class="fab fa-twitter">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" title="{$myName} on LinkedIn" class="social-id-li">
              <i class="fab fa-linkedin">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" title="{$myName} on Google+" class="social-id-gp">
              <i class="fab fa-google-plus">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" title="{$myName} on Pinterest" class="social-id-pi">
              <i class="fab fa-pinterest">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" title="{$myName} on Youtube" class="social-id-yt">
              <i class="fab fa-youtube ">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" title="{$myName} on Instagram" id="social-id-ig">
              <i class="fab fa-instagram">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@spotifyURL!=''">
            <a href="{@spotifyURL}" title="{$myName} on Spotify" id="social-id-ig">
              <i class="fab fa-spotify">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
        </xsl:when>
        <xsl:otherwise>
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" title="{$myName} on Facebook" id="social-id-fb">
              <img src="/ewcommon/images/icons/social/{$iconSet}/facebook.png" alt="{$myName} on Facebook" title="Follow {$myName} on Facebook" />
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" title="{$myName} on Twitter" id="social-id-tw">
              <img src="/ewcommon/images/icons/social/{$iconSet}/twitter.png" alt="{$myName} on Twitter" title="Follow {$myName} on Twitter" />
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" title="{$myName} on LinkedIn" id="social-id-li">
              <img src="/ewcommon/images/icons/social/{$iconSet}/LinkedIn.png" alt="{$myName} on LinkedIn" title="Follow {$myName} on LinkedIn" />
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" title="{$myName} on Google+" id="social-id-gp">
              <img src="/ewcommon/images/icons/social/{$iconSet}/Googleplus.png" alt="{$myName} on Google+" title="Follow {$myName} on Google+" />
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" title="{$myName} on Pinterest" id="social-id-li">
              <img src="/ewcommon/images/icons/social/{$iconSet}/Pinterest.png" alt="{$myName} on Pinterest" title="Follow {$myName} on Pinterest" />
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" title="{$myName} on YouTube" id="social-id-yt">
              <img src="/ewcommon/images/icons/social/{$iconSet}/YouTube.png" alt="{$myName} on YouTube" title="Follow {$myName} on YouTube" />
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" title="{$myName} on Pinterest" id="social-id-ig">
              <img src="/ewcommon/images/icons/social/{$iconSet}/Instagram.png" alt="{$myName} on Instagram" title="Follow {$myName} on Instagram" />
            </a>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>
    </div>
  </xsl:template>






</xsl:stylesheet>
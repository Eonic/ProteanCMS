<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
  <xsl:strip-space elements="*"/>
  <!-- -->
  <!-- ## GLOBAL VARIABLES ########################################################################   -->
  <!-- ## Variables for all EonicWeb XSLT   #######################################################   -->

  <!-- General node trees -->

  <xsl:variable name="page" select="/Page"/>
  <xsl:variable name="pageId" select="/Page/@id"/>
  <xsl:variable name="artId" select="number(concat(0,/Page/Request/QueryString/Item[@name='artid']))"/>
  <!-- removes xmlns="" on all copy-of responses -->
  <xsl:variable name="appPath">
  	<xsl:choose>
		<xsl:when test="/Page/Request/ServerVariables/Item[@name='APPLICATION_ROOT']/node()!=''">
		<xsl:value-of select="/Page/Request/ServerVariables/Item[@name='APPLICATION_ROOT']/node()"/>
		</xsl:when>
		<xsl:otherwise>
		<xsl:text>/</xsl:text>
		</xsl:otherwise>
	</xsl:choose>
  </xsl:variable>
  <xsl:variable name="menu" select="/Page/Menu"/>
  <xsl:variable name="cart" select="/Page/Cart"/>
  <xsl:variable name="cartPage" select="/Page/Cart[@type='order']/Order/@cmd!=''"/>
  <xsl:variable name="adminMode" select="/Page/@adminMode"/>
  <xsl:variable name="currentPage" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]"/>
  <xsl:variable name="sectionPage" select="/Page/Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]"/>
  <xsl:variable name="subSectionPage" select="/Page/Menu/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]"/>
  <xsl:variable name="subSubSectionPage" select="/Page/Menu/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]"/>
  <xsl:variable name="subSubSubSectionPage" select="/Page/Menu/MenuItem/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]"/>
  <xsl:variable name="subSubSubSubSectionPage" select="/Page/Menu/MenuItem/MenuItem/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]"/>
  <xsl:variable name="MatchHeightType" select="'matchHeight'"/>
  <xsl:variable name="sitename">
    <xsl:choose>
      <xsl:when test="$siteURL=''">
        <xsl:text>http</xsl:text>
        <xsl:if test="$page/Request/ServerVariables/Item[@name='HTTPS']='on'">s</xsl:if>
        <xsl:text>://</xsl:text>
        <xsl:value-of select="$page/Request/ServerVariables/Item[@name='SERVER_NAME']"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$siteURL"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="href">
    <xsl:if test="$siteURL=''">
      <xsl:text>http</xsl:text>
      <xsl:if test="$page/Request/ServerVariables/Item[@name='HTTPS']='on'">s</xsl:if>
      <xsl:text>://</xsl:text>
      <xsl:value-of select="$page/Request/ServerVariables/Item[@name='SERVER_NAME']"/>
    </xsl:if>
    <xsl:choose>
      <xsl:when test="/Page/ContentDetail">
        <xsl:apply-templates select="/Page/ContentDetail/Content" mode="getHref"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="$currentPage" mode="getHref"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="scriptVersion" select="'2'"/>
  <xsl:variable name="ewCmd" select="/Page/Request/QueryString/Item[@name='ewCmd']/node()"/>

  <!-- Widths -->
  <xsl:variable name="fullwidth" select="'960'"/>
  <xsl:variable name="navwidth" select="'200'"/>
  <xsl:variable name="boxpad" select="'15'"/>
  <xsl:variable name="colpad" select="'20'"/>
  <xsl:variable name="jqueryVer" select="'1.11'"/>
  <!-- Dates -->
  <xsl:variable name="today" select="/Page/Request/ServerVariables/Item[@name='Date']/node()"/>
  <xsl:variable name="currentYear" select="substring($today,1,4)"/>
  <xsl:variable name="currentMonth" select="substring($today,6,2)"/>
  <xsl:variable name="currentDay" select="substring($today,9,2)"/>
  <!-- url handling - first four get replaced with hyphens-->
  <xsl:variable name="illegalURLString">
    <xsl:text> /\.:!"'£$%^&amp;*()|</xsl:text>
  </xsl:variable>
  <xsl:variable name="illegalReplaceString" select="'----'" />
  <!-- User variables -->
  <xsl:variable name="userAgent" select="/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT']"/>
  <!--  This variable is used for Front-end rendering and so 
        matches the rendering engine and not necesarily the software version. -->
  <xsl:variable name="browserVersion">
    <xsl:call-template name="getBrowserVersion" />
  </xsl:variable>
  <xsl:variable name="bundleVersion">
    <xsl:if test="/Page/Settings/add[@key='bundleVersion']">
      <xsl:text>?v=</xsl:text>
      <xsl:value-of select="/Page/Settings/add[@key='bundleVersion']/@value"/>
    </xsl:if>
  </xsl:variable>
  <xsl:variable name="paramDelimiter">
    <xsl:choose>
      <xsl:when test="$adminMode">
        <xsl:text>&amp;</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>?</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- Cart Values -->
  <xsl:variable name="currency">
    <xsl:choose>
      <xsl:when test="/Page/Cart">
        <xsl:value-of select="/Page/Cart/@currency"/>
      </xsl:when>
      <xsl:otherwise>GBP</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- Cart Needed for FormatPrice in system translations -->
  <xsl:variable name="currencyCode">
    <xsl:choose>
      <xsl:when test="/Page/Cart">
        <xsl:value-of select="/Page/Cart/@currency"/>
      </xsl:when>
      <xsl:otherwise>GBP</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="currencySymbol">
    <xsl:choose>
      <xsl:when test="/Page/Cart">
        <xsl:value-of select="/Page/Cart/@currencySymbol"/>
      </xsl:when>
      <xsl:otherwise>£</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="querySymbol">
    <xsl:choose>
      <xsl:when test="/Page/@adminMode">&amp;</xsl:when>
      <xsl:otherwise>?</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="pageLang">
    <xsl:choose>
      <xsl:when test="/Page/Contents/Content[@name='XmlLang']">
        <xsl:value-of select="Contents/Content[@name='XmlLang']"/>
      </xsl:when>
      <xsl:when test="@userlang and @userlang!=''">
        <xsl:value-of select="@userlang"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="@translang"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- Navigation types -->
  <xsl:variable name="DescriptiveContentURLs">
    <xsl:call-template name="getXmlSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'DescriptiveContentURLs'"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="siteURL">
    <xsl:variable name="baseUrl">
      <xsl:call-template name="getXmlSettings">
        <xsl:with-param name="sectionName" select="'web'"/>
        <xsl:with-param name="valueName" select="'BaseUrl'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="siteName">
      <xsl:call-template name="getXmlSettings">
        <xsl:with-param name="sectionName" select="'web'"/>
        <xsl:with-param name="valueName" select="'SiteName'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="cartSiteUrl">
      <xsl:call-template name="getXmlSettings">
        <xsl:with-param name="sectionName" select="'cart'"/>
        <xsl:with-param name="valueName" select="'SiteURL'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$cartSiteUrl!=''">
        <xsl:value-of select="$cartSiteUrl"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="$baseUrl!=''">
            <xsl:value-of select="$baseUrl"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="/Page/Request/ServerVariables/Item[@name='SERVER_NAME']"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="GoogleAnalyticsUniversalID">
    <xsl:call-template name="getXmlSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'GoogleAnalyticsUniversalID'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name="GoogleAPIKey">
    <xsl:call-template name="getXmlSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'GoogleAPIKey'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name="GoogleTagManagerID">
    <xsl:if test="not(/Page/@adminMode) and not(/Page/@previewMode='true')">
        <xsl:call-template name="getXmlSettings">
          <xsl:with-param name="sectionName" select="'web'"/>
          <xsl:with-param name="valueName" select="'GoogleTagManagerID'"/>
        </xsl:call-template>
    </xsl:if>
  </xsl:variable>

  <!-- Cal Cmd for Calendars -->
  <xsl:variable name="calendarMonth">
    <xsl:choose>
      <xsl:when test="//QueryString/Item[@name='calcmd'] and //QueryString/Item[@name='monthdate']/node()">
        <xsl:value-of select="//QueryString/Item[@name='monthdate']/node()"/>
      </xsl:when>
      <xsl:when test="number(//QueryString/Item[@name='calcmd']/node())!='NaN'">
        <xsl:value-of select="//QueryString/Item[@name='calcmd']/node()"/>
      </xsl:when>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="alphabet" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'"/>
  <xsl:variable name="alphabetLo" select="'abcdefghijklmnopqrstuvwxyz'"/>


  <!-- Navigation types -->
  <xsl:variable name="ScriptAtBottom">
    <xsl:call-template name="getXmlSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'ScriptAtBottom'"/>
    </xsl:call-template>
  </xsl:variable>
  <!-- -->
  <xsl:variable name="lazy" select="'off'"/>
  <xsl:variable name="placeholder" select="'/ewcommon/images/t22.gif'"/>
  <xsl:variable name="lazyplaceholder" select="''"/>
  <!--####################### Page Level Templates, can be overridden later. ##############################-->
  <!-- -->



  <xsl:template match="Page">
    <xsl:variable name="pageLang">
      <xsl:choose>
        <xsl:when test="/Page/Contents/Content[@name='XmlLang']">
          <xsl:value-of select="Contents/Content[@name='XmlLang']"/>
        </xsl:when>
        <xsl:when test="@userlang and @userlang!=''">
          <xsl:value-of select="@userlang"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@translang"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <html lang="{$pageLang}" xml:lang="{$pageLang}">
      <xsl:apply-templates select="." mode="htmlattr"/>
      <head>
        <xsl:choose>
          <xsl:when test="ContentDetail">
            <xsl:attribute name="prefix">
              <xsl:apply-templates select="ContentDetail/Content" mode="opengraph-namespace"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="prefix">og: http://ogp.me/ns# fb: http://ogp.me/ns/fb# website: http://ogp.me/ns/website#</xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:if test="$GoogleTagManagerID!=''">
          <!-- Google Tag Manager -->
          <script>
            (function(w,d,s,l,i){w[l]=w[l]||[];w[l].push({'gtm.start':
            new Date().getTime(),event:'gtm.js'});var f=d.getElementsByTagName(s)[0],
            j=d.createElement(s),dl=l!='dataLayer'?'&amp;l='+l:'';j.async=true;j.src=
            'https://www.googletagmanager.com/gtm.js?id='+i+dl;f.parentNode.insertBefore(j,f);
            })(window,document,'script','dataLayer','<xsl:value-of select="$GoogleTagManagerID"/>');
          </script>
          <!-- End Google Tag Manager -->
        </xsl:if>
        <xsl:apply-templates select="." mode="metacharset"/>
        <!-- browser title -->
        <title>
          <xsl:apply-templates select="." mode="PageTitle"/>
          <xsl:text> </xsl:text>
        </title>
        <xsl:if test="/Page/@baseUrl">
          <base href="{/Page/@baseUrl}"/>
        </xsl:if>
        <xsl:apply-templates select="." mode="alternatePages"/>

        <!-- IF IE - force to use IE8/9 mode or chromeframe -->
        <!--<meta http-equiv="X-UA-Compatible" content="IE=Edge,chrome=1"/>-->
        <!-- but for now just telling it to not allow compat mode for our sites.-->

        <xsl:if test="$browserVersion='MSIE 8.0' or $browserVersion='MSIE 9.0'">
          <xsl:choose>
            <!-- quick fix - Tiny MCE doesn't work in IE9 so tell to emulate IE8 -->
            <xsl:when test="//textarea[contains(@class,'xhtml')]">
              <meta http-equiv="X-UA-Compatible" content="IE=EmulateIE8" />
            </xsl:when>
            <xsl:otherwise>
              <!-- tell to use the latest of the edge engines. -->
              <meta http-equiv="X-UA-Compatible" content="IE=Edge"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>

        <!-- meta data -->
        <xsl:apply-templates select="." mode="metadata"/>

        <!-- favicon -->
        <xsl:call-template  name="favicon"/>

        <!-- page thumbnail see: http://just-another-coder.blogspot.com/2010/04/link-relimagesrc-whats-this-for.html -->
        <xsl:variable name="pageThumbnailURL">
          <xsl:apply-templates select="." mode="getPageThumbnail" />
        </xsl:variable>
        <xsl:if test="$pageThumbnailURL!=''">
          <link rel="image_src" href="{$pageThumbnailURL}" />
        </xsl:if>

        <!-- Canonical link -->
        <xsl:apply-templates select="/Page" mode="canonicalLink" />

        <!-- If Feed Control Content Type - Let Browser Toolbars know -->
        <xsl:apply-templates select="/Page/Contents/Content[@type='FeedControl']" mode="feedLinks"/>
        <xsl:apply-templates select="//Content[@rss and @rss!='false']" mode="feedLinks"/>

        <!-- common css -->
        <xsl:choose>
          <xsl:when test="not(/Page/Contents/Content[@name='criticalPathCSS']) or $adminMode">
            <xsl:apply-templates select="." mode="commonStyle"/>
          </xsl:when>
          <xsl:otherwise>
            <style>
              <xsl:copy-of select="/Page/Contents/Content[@name='criticalPathCSS']/node()"/>
            </style>
          </xsl:otherwise>
        </xsl:choose>


        <xsl:if test="$ScriptAtBottom!='on'">
          <xsl:apply-templates select="." mode="js"/>
        </xsl:if>
        
        <xsl:apply-templates select="/Page/Contents/Content[@type='PlainText' and @name='jsonld']" mode="JSONLD"/>
      </head>

      <!-- Go build the Body of the HTML doc -->
      <xsl:apply-templates select="." mode="bodyBuilder"/>
      <xsl:if test="/Page/Contents/Content[@name='criticalPathCSS'] and not($adminMode)">
        <xsl:apply-templates select="." mode="commonStyle"/>
      </xsl:if>

    </html>
  </xsl:template>

  <xsl:template match="Page" mode="alternatePages">
    <xsl:choose>
      <xsl:when test="$currentPage/PageVersion[@verType='0']">
        <link rel="alternate" href="{$currentPage/PageVersion[@verType='0']/@url}" hreflang="x-default" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:if test="$currentPage/PageVersion[@verType='3']">
          <link rel="alternate" href="{$currentPage/@url}" hreflang="x-default" />          
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:for-each select="$currentPage/PageVersion[@verType='3']">
      <link rel="alternate" href="{@url}" hreflang="{@lang}" />
    </xsl:for-each>
  </xsl:template>

  <xsl:template match="Page" mode="metacharset">
    <meta http-equiv="Content-Type" content="text/html; charset=UTF-8"/>
  </xsl:template>

  <xsl:template match="Page[Contents/Content[@name='EncType']]" mode="metacharset">
    <meta http-equiv="Content-Type" content="text/html; charset={Contents/Content[@name='EncType' or @name='EncType']}"/>
  </xsl:template>

  <xsl:template match="Page" mode="htmlattr">

  </xsl:template>

  <xsl:template match="Content" mode="opengraph-namespace">
    <xsl:text>og: http://ogp.me/ns# fb: http://ogp.me/ns/fb# article: http://ogp.me/ns/article#</xsl:text>
  </xsl:template>

  <xsl:template name="favicon">
    <!--link rel="icon" type="image/ico" href="{/Page/@baseUrl}/favicon.ico"/-->
  </xsl:template>

  <xsl:template match="Page" mode="PageTitle">
    <xsl:choose>
      <xsl:when test="$page/@adminMode='true'">
        <xsl:variable name="thiscmd" select="$page/@ewCmd"/>
        <xsl:value-of select="$page/descendant-or-self::MenuItem[@cmd=$thiscmd]/@name"/>
      </xsl:when>
      <xsl:when test="/Page/ContentDetail">
        <xsl:choose>
          <xsl:when test="/Page/ContentDetail/Content/@metaTitle!=''">
            <xsl:value-of select="/Page/ContentDetail/Content/@metaTitle"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="/Page/ContentDetail/Content" mode="getDisplayName"/>
            <xsl:if test="/Page/Contents/Content[@name='PageTitle']">
              <xsl:text> - </xsl:text>
              <xsl:value-of select="/Page/Contents/Content[@name='PageTitle']"/>
            </xsl:if>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="not(/Page/ContentDetail) and not(/Page/Contents/Content[@name='PageTitle'])">
        <xsl:apply-templates select="$currentPage" mode="getDisplayName"/>
        <xsl:if test="/Page/Contents/Content[@name='PageTitle']">
          <xsl:text> - </xsl:text>
          <xsl:value-of select="/Page/Contents/Content[@name='PageTitle']"/>
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="/Page/Contents/Content[@name='PageTitle']"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="/" mode="PageTitle">
    <xsl:choose>
      <xsl:when test="$page/@adminMode='true'">
        <xsl:variable name="thiscmd" select="$page/@ewCmd"/>
        <xsl:value-of select="$page/descendant-or-self::MenuItem[@cmd=$thiscmd]/@name"/>
      </xsl:when>
      <xsl:when test="/Page/ContentDetail">
        <xsl:choose>
          <xsl:when test="/Page/ContentDetail/Content/@metaTitle!=''">
            <xsl:value-of select="/Page/ContentDetail/Content/@metaTitle"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="/Page/ContentDetail/Content" mode="getDisplayName"/>
            <xsl:if test="/Page/Contents/Content[@name='PageTitle']">
              <xsl:text> - </xsl:text>
              <xsl:value-of select="/Page/Contents/Content[@name='PageTitle']"/>
            </xsl:if>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:when test="not(/Page/ContentDetail) and not(/Page/Contents/Content[@name='PageTitle'])">
        <xsl:apply-templates select="$currentPage" mode="getDisplayName"/>
        <xsl:if test="/Page/Contents/Content[@name='PageTitle']">
          <xsl:text> - </xsl:text>
          <xsl:value-of select="/Page/Contents/Content[@name='PageTitle']"/>
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="/Page/Contents/Content[@name='PageTitle']"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="Page" mode="commonStyle">
    <xsl:choose>
      <xsl:when test="@cssFramework='bs3' or @adminMode='true'">
        <xsl:call-template name="bundle-css">
          <xsl:with-param name="comma-separated-files">
            <xsl:text>/ewcommon/css/base-bs.less</xsl:text>
          </xsl:with-param>
          <xsl:with-param name="bundle-path">
            <xsl:text>~/Bundles/baseStyle</xsl:text>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="bundle-css">
          <xsl:with-param name="comma-separated-files">
            <xsl:text>/ewcommon/css/base.less</xsl:text>
          </xsl:with-param>
          <xsl:with-param name="bundle-path">
            <xsl:text>~/Bundles/baseStyle</xsl:text>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>

    <xsl:if test="contains($userAgent, 'MSIE 7.0') and not(contains($userAgent, 'Trident/5.0'))">
      <link rel="stylesheet" type="text/css" href="/ewcommon/icons/fa/css/font-awesome-ie7.min.css"/>
    </xsl:if>

    <xsl:if test="not(@adminMode='true')">
      <xsl:apply-templates select="." mode="siteStyle"/>
      <xsl:for-each select="Contents/Content[@name='bespokeCSS' and @type='PlainText']">
        <link rel="stylesheet" type="text/css" href="{node()}"/>
      </xsl:for-each>
      <xsl:if test="Contents/Content[@name='inlineCSS' and @type='PlainText']">
        <style type="text/css">
          <xsl:value-of select="Contents/Content[@name='inlineCSS' and @type='PlainText']/node()"/>
        </style>
      </xsl:if>
      <xsl:if test="//Content[@type='CookiePolicy'] and not(/Page/@adminMode)">
        <link rel="stylesheet" href="/ewcommon/js/jquery/cookiecuttr/cookiecuttr.css"/>
      </xsl:if>
      <xsl:if test="//Content[@moduleType='SlideCarousel'] and not(/Page/@adminMode)">
        <link rel="stylesheet" href="/ewcommon/js/jquery/SlideCarousel/SlideCarousel.css"/>
      </xsl:if>
      <xsl:if test="//Content[@moduleType='SliderGallery'] and not(/Page/@adminMode)">
        <link rel="stylesheet" href="/ewcommon/js/jquery/SliderGallery/skins/tn3/tn3.css"/>
        <link rel="stylesheet" href="/ewcommon/js/jquery/SliderGallery/skins/tn3a/tn3a.css"/>
        <link rel="stylesheet" href="/ewcommon/js/jquery/SliderGallery/skins/tn3e/tn3e.css"/>
        <link rel="stylesheet" href="/ewcommon/js/jquery/SliderGallery/skins/tn3f/tn3f.css"/>
      </xsl:if>
    </xsl:if>
    <xsl:apply-templates select="." mode="adminStyle"/>
    <xsl:if test="/Page/@adminMode">
      <xsl:if test="contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 8') or contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 7') or contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 6.0')">
        <link rel="stylesheet" href="/ewcommon/css/Admin/skins/ie8.less"/>
      </xsl:if>
      <xsl:if test="@ewCmd='EditContent' or @ewCmd='AddContent'">
        <xsl:apply-templates select="." mode="siteStyle"/>
      </xsl:if>
    </xsl:if>
    <xsl:apply-templates select="." mode="resellerStyle"/>
    <xsl:if test="@cssFramework='bs3'">
      <xsl:if test="contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 8') or contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 7') or contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 6.0')">
        <script src="/ewcommon/js/respond.min.js">/* */</script>
        <script src="https://oss.maxcdn.com/libs/html5shiv/3.7.0/html5shiv.js">/* */</script>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Page" mode="resellerStyle">
    <!--placeholder for resellerstyles-->
  </xsl:template>

  <xsl:template match="Page" mode="adminStyle"></xsl:template>


  <!--  ###########################################################################################  -->
  <!--  ##  JAVASCRIPTS  ##########################################################################  -->
  <!--  ###########################################################################################  -->


  <xsl:template match="Page" mode="js">
    <!-- bring in jQuery and standard plugins -->
    <xsl:apply-templates select="." mode="commonJs" />

    <!-- site specific javascripts -->
    <xsl:apply-templates select="." mode="siteJs"/>

    <!-- admin javascripts -->
    <xsl:if test="/Page/@adminMode">
      <xsl:apply-templates select="." mode="adminJs"/>
    </xsl:if>

    <!-- IF IE6 apply PNG Fix as standard -->
    <xsl:if test="contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 6.0') and not(contains(Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'Opera'))">
      <script type="text/javascript" src="/ewcommon/js/pngfix.js" defer="">/* */</script>
    </xsl:if>



    <!--  Google analytics javascript  -->
    <xsl:choose>
      <xsl:when test="$GoogleAnalyticsUniversalID!=''">
        <xsl:apply-templates select="." mode="googleUniversalAnalyticsCode"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="/Page/Contents/Content[@type='MetaData' and @name='MetaGoogleAnalyticsID']" mode="googleAnalyticsCode"/>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:apply-templates select="/Page/Contents/Content[@type='MetaData' and @name='MetaA1WebStatsID']" mode="A1WebStatsCode"/>
    <xsl:apply-templates select="/Page/Contents/Content[@type='MetaData' and @name='MetaWhoIsVisitingID']" mode="MetaWhoIsVisitingCode"/>

  </xsl:template>

  <xsl:template match="Content" mode="contentJS">
  </xsl:template>

  <xsl:template match="Content" mode="contentDetailJS">
  </xsl:template>

  <xsl:template match="Page" mode="commonJs">
    <xsl:call-template name="bundle-js">
      <xsl:with-param name="comma-separated-files">
        <xsl:apply-templates select="." mode="commonJsFiles" />
      </xsl:with-param>
      <xsl:with-param name="bundle-path">
        <xsl:text>~/Bundles/Jquery</xsl:text>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <xsl:template match="Page" mode="commonJsFiles">
    <xsl:choose>
      <xsl:when test="$jqueryVer='3.4'">
        <xsl:text>~/ewcommon/js/jquery/jquery-3.4.1.min.js,</xsl:text>
        <xsl:text>~/ewcommon/js/jquery/jquery-migrate-3.0.1.min.js,</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>~/ewcommon/js/jquery/jquery-1.11.1.min.js,</xsl:text>
        <xsl:text>~/ewcommon/js/jquery/jquery-migrate-1.2.1.min.js,</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text>~/ewcommon/js/jquery/ui/1.11.1/jquery-ui.min.js,</xsl:text>
    <xsl:text>~/ewcommon/bs3/js/bootstrap.js,</xsl:text>
    <xsl:text>~/ewcommon/js/jquery/colorpickersliders/tinycolor.js,</xsl:text>
    <xsl:text>~/ewcommon/js/jquery/colorpickersliders/bootstrap.colorpickersliders.min.js,</xsl:text>
    <xsl:text>~/ewcommon/js/jquery/jquery.matchHeight.js,</xsl:text>
    <xsl:choose>
      <xsl:when test="@cssFramework='bs3'">
        <xsl:text>~/ewcommon/js/jquery/revolution/js/jquery.themepunch.tools.min.js,</xsl:text>
        <xsl:text>~/ewcommon/js/jquery/revolution/js/jquery.themepunch.revolution.js,</xsl:text>
        <!--<xsl:text>~/ewcommon/js/jQuery/parallax/jquery.stellar.min.js,</xsl:text>-->
        <xsl:text>~/ewcommon/js/jQuery/parallax/universal-parallax.js,</xsl:text>
        <xsl:text>~/ewcommon/js/jQuery/jquery.magnific-popup.min.js,</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>~/ewcommon/js/jquery/lightbox/jquery.lightbox-0.5.min.js,</xsl:text>
        <xsl:text>~/ewcommon/js/non-bs3.js,</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text>~/ewcommon/js/common.js</xsl:text>
  </xsl:template>

  <!-- template to bring in all the jQuery and plugins that are as standard on each page -->
  <xsl:template match="Page" mode="jQuery">
    <xsl:choose>
      <xsl:when test="descendant::DisplayName[@paralaxLoad='true']">
        <xsl:call-template name="bundle-js">
          <xsl:with-param name="comma-separated-files">
            <xsl:text>~/ewcommon/js/jquery/isotope/jquery.isotope.min.js,</xsl:text>
            <xsl:text>~/ewcommon/js/jquery/innerFade/jquery.innerfade.js,</xsl:text>
            <xsl:text>~/ewcommon/js/jquery/SliderGallery/js/jquery.tn3.min.js,</xsl:text>
            <xsl:choose>
              <xsl:when test="@cssFramework='bs3'">
              </xsl:when>
              <xsl:otherwise>
                <xsl:text>~/ewcommon/js/jquery/SlideCarousel/jquery.mousewheel.min.js,</xsl:text>
                <xsl:text>~/ewcommon/js/jquery/SlideCarousel/jquery.carousel-1.1.min.js,</xsl:text>
              </xsl:otherwise>
            </xsl:choose>
            <xsl:text>~/ewcommon/js/ace/src-noconflict/ace.js,</xsl:text>
            <xsl:text>~/ewcommon/js/ace/src-noconflict/theme-mono_industrial.js,</xsl:text>
            <xsl:text>~/ewcommon/js/ace/src-noconflict/mode-xml.js,</xsl:text>
            <xsl:text>~/ewcommon/js/ace/jquery-ace.js</xsl:text>
          </xsl:with-param>
          <xsl:with-param name="bundle-path">
            <xsl:text>~/Bundles/JqueryModules</xsl:text>
          </xsl:with-param>
        </xsl:call-template>
        <script src="/ewcommon/js/jquery/slick-carousel/slick2.min.js">/* */</script>
      </xsl:when>
      <xsl:otherwise>
        <xsl:if test="@layout='Modules_Masonary'">
          <script type="text/javascript" src="/ewcommon/js/jquery/isotope/jquery.isotope.min.js" >/* */</script>
        </xsl:if>
        <xsl:if test="//Content[@moduleType='SlideCarousel'] and not(/Page/@adminMode)">
          <script src="/ewcommon/js/jquery/SlideCarousel/jquery.mousewheel.min.js">/* */</script>
          <script src="/ewcommon/js/jquery/SlideCarousel/jquery.carousel-1.1.min.js">/* */</script>
        </xsl:if>
        <xsl:if test="//Content[@moduleType='ImageFader']">
          <script src="/ewcommon/js/jquery/innerFade/jquery.innerfade.js">/* */</script>
        </xsl:if>
        <xsl:if test="//Content[@carousel='true']">
          <script src="/ewcommon/js/jquery/slick-carousel/slick2.min.js">/* */</script>
          <!-- !!! MIN VERSION CAUSES ERROR -->
        </xsl:if>
        <xsl:if test="//Content[@moduleType='SliderGallery'] and not(/Page/@adminMode)">
          <script src="/ewcommon/js/jquery/SliderGallery/js/jquery.tn3.min.js">/* */</script>
        </xsl:if>
        <!-- code formatting plugin -->
        <xsl:if test="//Content[@moduleType='FormattedCode' or @moduleType='EmbeddedHTML']">
          <script type="text/javascript" src="/ewcommon/js/jquery/beautyOfCode/boc.js" >/* */</script>
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>

    <xsl:if test="//Content[@moduleType='Audio']/Path/node()!=''">
      <script type="text/javascript" src="/ewcommon/js/jquery/jplayer/jquery.jplayer.min.js">/* */</script>
      <script type="text/javascript" src="/ewcommon/js/jquery/jplayer/jquery.jplayer.inspector.js">/* */</script>
      <script type="text/javascript">
        <xsl:apply-templates select="." mode="initialiseJplayer"/>
      </script>
    </xsl:if>

    <xsl:if test="//Content[@type='CookiePolicy'] and not(/Page/@adminMode)">
      <script src="/ewcommon/js/jquery/jquery.cookie.js">/* */</script>
      <script src="/ewcommon/js/jquery/cookiecuttr/jquery.cookiecuttr.js">/* */</script>
      <script type="text/javascript">
        <xsl:text>$(document).ready(function () {</xsl:text>
        <xsl:text>$.cookieCuttr(</xsl:text>
        <xsl:apply-templates select="//Content[@type='CookiePolicy']" mode="cookiePolicy"/>
        <xsl:text>);</xsl:text>
        <xsl:text>});</xsl:text>
      </script>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Content[@type='CookiePolicy']" mode="cookiePolicy">
    <xsl:text>{</xsl:text>
    <xsl:choose>
      <xsl:when test="cookieAnayltics/node()='false'">
        <xsl:text>cookieAnalytics:false,</xsl:text>
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="cookieDeclineButton/node()='true'">
        <xsl:text>cookieDeclineButton:true,</xsl:text>
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="cookieAcceptButton/node()='true'">
        <xsl:text>cookieAcceptButton:true,</xsl:text>
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="cookieResetButton/node()='true'">
        <xsl:text>cookieResetButton:true,</xsl:text>
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="cookieOverlayEnabled/node()='true'">
        <xsl:text>cookieOverlayEnabled:true,</xsl:text>
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="cookiePolicyLink/node()!=''">
        <xsl:text>cookiePolicyLink :"</xsl:text>
        <xsl:call-template name="escape-js">
          <xsl:with-param name="string">
            <xsl:value-of select="cookieDomain/node()"/>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:text>",</xsl:text>
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="cookieMessage/node()!=''">
        <xsl:text>cookieMessage:"</xsl:text>
        <xsl:call-template name="escape-js">
          <xsl:with-param name="string">
            <xsl:value-of select="cookieMessage/node()"/>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:text>",</xsl:text>
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="cookieAnalyticsMessage/node()!=''">
        <xsl:text>cookieAnalyticsMessage:"</xsl:text>
        <xsl:call-template name="escape-js">
          <xsl:with-param name="string">
            <xsl:value-of select="cookieAnalyticsMessage/node()"/>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:text>",</xsl:text>
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="cookieWhatAreTheyLink/node()!=''">
        <xsl:text>cookieWhatAreTheyLink:"</xsl:text>
        <xsl:call-template name="escape-js">
          <xsl:with-param name="string">
            <xsl:value-of select="cookieWhatAreTheyLink/node()"/>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:text>",</xsl:text>

      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="cookieErrorMessage/node()!=''">
        <xsl:text>cookieErrorMessage:"</xsl:text>
        <xsl:call-template name="escape-js">
          <xsl:with-param name="string">
            <xsl:value-of select="cookieErrorMessage/node()"/>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:text>",</xsl:text>

      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="cookieNotificationLocationBottom/node()='true'">
        <xsl:text>cookieNotificationLocationBottom:true,</xsl:text>
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="cookieAcceptButtonText/node()!=''">
        <xsl:text>cookieAcceptButtonText:"</xsl:text>
        <xsl:call-template name="escape-js">
          <xsl:with-param name="string">
            <xsl:value-of select="cookieAcceptButtonText/node()"/>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:text>",</xsl:text>

      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="cookieDeclineButtonText/node()!=''">
        <xsl:text>cookieDeclineButtonText:"</xsl:text>
        <xsl:call-template name="escape-js">
          <xsl:with-param name="string">
            <xsl:value-of select="cookieDeclineButtonText/node()"/>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:text>",</xsl:text>

      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="cookieResetButtonText/node()!=''">
        <xsl:text>cookieResetButtonText:"</xsl:text>
        <xsl:call-template name="escape-js">
          <xsl:with-param name="string">
            <xsl:value-of select="cookieResetButtonText/node()"/>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:text>",</xsl:text>

      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="cookieWhatAreLinkText/node()!=''">
        <xsl:text>cookieWhatAreLinkText:"</xsl:text>
        <xsl:call-template name="escape-js">
          <xsl:with-param name="string">
            <xsl:value-of select="cookieWhatAreLinkText/node()"/>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:text>",</xsl:text>

      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="cookiePolicyPage/node()!=''">
        <xsl:text>cookiePolicyPage:"</xsl:text>
        <xsl:call-template name="escape-js">
          <xsl:with-param name="string">
            <xsl:value-of select="cookiePolicyPage/node()"/>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:text>",</xsl:text>

      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="cookiePolicyPageMessage/node()!=''">
        <xsl:text>cookiePolicyPageMessage:"</xsl:text>
        <xsl:call-template name="escape-js">
          <xsl:with-param name="string">
            <xsl:value-of select="cookiePolicyPageMessage/node()"/>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:text>",</xsl:text>

      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="cookieDiscreetLink/node()='true'">
        <xsl:text>cookieDiscreetLink:true,</xsl:text>
      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="cookieDiscreetLinkText/node()!=''">
        <xsl:text>cookieDiscreetLinkText:"</xsl:text>
        <xsl:call-template name="escape-js">
          <xsl:with-param name="string">
            <xsl:value-of select="cookieDiscreetLinkText/node()"/>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:text>",</xsl:text>

      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>
    <xsl:choose>
      <xsl:when test="cookieDiscreetPosition/node()!=''">
        <xsl:text>cookieDiscreetPosition:"</xsl:text>
        <xsl:call-template name="escape-js">
          <xsl:with-param name="string">
            <xsl:value-of select="cookieDiscreetPosition/node()"/>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:text>",</xsl:text>

      </xsl:when>
      <xsl:otherwise></xsl:otherwise>
    </xsl:choose>

    <xsl:text>cookieDomain:"</xsl:text>
    <xsl:call-template name="escape-js">
      <xsl:with-param name="string">
        <xsl:value-of select="cookieDomain/node()"/>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:text>",</xsl:text>
    <xsl:text>}</xsl:text>

  </xsl:template>



  <!-- Javascripts that can be brought in in the footer of the HTML document, e.g. asynchronous scripts -->
  <xsl:template match="Page" mode="footerJs">
    <!-- common javascript -->
    <xsl:if test="$ScriptAtBottom='on'">
      <xsl:apply-templates select="." mode="js"/>
    </xsl:if>
    
    <!-- page module specific javascripts -->
    <xsl:apply-templates select="." mode="jQuery" />
    <!-- page specific javascripts -->
    <xsl:apply-templates select="." mode="pageJs"/>
    
    <xsl:choose>
      <xsl:when test="/Page/ContentDetail/Content">
        <xsl:apply-templates select="/Page/ContentDetail/Content" mode="contentDetailJS"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="/Page/Contents/Content" mode="contentJS"/>
      </xsl:otherwise>
    </xsl:choose>
    <!-- GOOGLE MAPS -->
    <xsl:apply-templates select="." mode="googleMapJS" />
    <!-- Includes initialisation template if at least one method is in use: -->
    <xsl:if test="$page/Contents/Content[(@type='SocialNetworkingSettings' and Bookmarks/Methods/@*='true') or (@moduleType='NewsList' and not(@commentPlatform='' or @commentPlatform='none'))]">
      <xsl:call-template name="initialiseSocialBookmarks"/>
    </xsl:if>

    <!-- EXIT POPUP -->
    <xsl:if test="$page/Contents/Content[@position='ExitModal'] and not($adminMode)">
      <div class="modal exit-modal fade">
        <div class="modal-dialog">
          <xsl:apply-templates select="$page/Contents/Content[@type='Module' and @position = 'ExitModal']" mode="modalBox"/>
        </div>
      </div>
      <script type="text/javascript" src="/ewcommon/js/jquery/exitmodal/jquery.exit-modal.js" async="async">/* */</script>
    </xsl:if>

    <xsl:if test="/Page/Contents/Content[@type='MetaData' and @name='MetaGoogleRemarketingConversionId']">
      <!-- Google Code for Remarketing Tag -->
      <!-- 
      Remarketing tags may not be associated with personally identifiable information or placed on pages related to sensitive categories. See more information and instructions on how to setup the tag on: http://google.com/ads/remarketingsetup
      -->
      <script type="text/javascript">
        var google_conversion_id = <xsl:value-of select="/Page/Contents/Content[@type='MetaData' and @name='MetaGoogleRemarketingConversionId']/node()"/>;
        var google_custom_params = window.google_tag_params;
        var google_remarketing_only = true;
      </script>
      <script type="text/javascript" src="//www.googleadservices.com/pagead/conversion.js">/* */</script>
      <noscript>
        <div style="display:inline;">
          <img height="1" width="1" style="border-style:none;" alt="" src="//googleads.g.doubleclick.net/pagead/viewthroughconversion/955616701/?value=0&amp;guid=ON&amp;script=0"/>
        </div>
      </noscript>
    </xsl:if>
    <xsl:apply-templates select="/Page/Contents/Content[@type='MetaData' and @name='MetaA1WebStatsID']" mode="A1WebStatsCodeFooter"/>
    <xsl:apply-templates select="/Page/Contents/Content[@type='MetaData' and @name='MetaA1WebStatsIDV2']" mode="A1WebStatsCodeV2Footer"/>

    <xsl:if test="Contents/Content[@name='fb-pixel_id']">
      <!-- Facebook Pixel Code -->
      <script>
        <xsl:text>!function(f,b,e,v,n,t,s){if(f.fbq)return;n=f.fbq=function(){n.callMethod?n.callMethod.apply(n,arguments):n.queue.push(arguments)};if(!f._fbq)f._fbq=n;n.push=n;n.loaded=!0;n.version='2.0';n.queue=[];t=b.createElement(e);t.async=!0;t.src=v;s=b.getElementsByTagName(e)[0];s.parentNode.insertBefore(t,s)}(window,document,'script','https://connect.facebook.net/en_US/fbevents.js');fbq('init', '</xsl:text>
        <xsl:value-of select="Contents/Content[@name='fb-pixel_id']"/>
        <xsl:text>');fbq('track', 'PageView');</xsl:text>
        <xsl:for-each select="descendant-or-self::instance[@valid='true']/emailer">
          <xsl:text>fbq('track', 'Lead');</xsl:text>
        </xsl:for-each>
      </script>
      <noscript>
        <img height="1" width="1" src="https://www.facebook.com/tr?id={Contents/Content[@name='fb-pixel_id']}&amp;ev=PageView&amp;noscript=1"/>
      </noscript>
      <!-- End Facebook Pixel Code -->
    </xsl:if>
    <xsl:apply-templates select="/Page/Contents/Content[@type='FacebookChat' and @name='FacebookChat']" mode="FacebookChatCode"/>
    <xsl:apply-templates select="/Page" mode="JSONLD"/>
    <!-- pull in site specific js in footer -->
    <xsl:apply-templates select="." mode="siteFooterJs"/>

  </xsl:template>

  <!-- DUMMY template, as sometimes Functions.xsl gets imported without CommonLayouts where this actually sits -->
  <xsl:template name="initialiseSocialBookmarks"></xsl:template>


  <!-- overidable template to pull in site specific files -->
  <xsl:template match="Page" mode="siteJs"></xsl:template>

  <!-- overide template for site specific footer js -->
  <xsl:template match="Page" mode="siteFooterJs"></xsl:template>

  <!-- overidable template to pull in page specific files -->
  <xsl:template match="Page" mode="pageJs"></xsl:template>

  <xsl:template match="Page" mode="JSONLD">
    <xsl:variable name="jsonld">
      <xsl:choose>
        <xsl:when test="ContentDetail/Content">
          <xsl:apply-templates select="ContentDetail/Content" mode="JSONLD"/>
        </xsl:when>
        <xsl:when test="Contents/Content[@type='Module' and @moduleType='FAQList']">
          <xsl:apply-templates select="Contents/Content[@type='Module' and @moduleType='FAQList']" mode="JSONLD"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="Contents/Content" mode="JSONLD"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test ="$jsonld!=''">
      <script type="application/ld+json">
        <xsl:value-of select="$jsonld"/>
      </script>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Content" mode="JSONLD"></xsl:template>

  <!-- -->
  <!--   ################################################   Meta Tags   ##############################################   -->
  <!-- -->
  <xsl:template name="default-og-img">

  </xsl:template>

  <xsl:template match="Page" mode="metadata">
    <xsl:if test="@cssFramework='bs3' or $adminMode">
      <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
    </xsl:if>

    <xsl:if test="Contents/Content[@name='MetaDescription' or @name='metaDescription'] or ContentDetail">
      <xsl:apply-templates select="." mode="getMetaDescription"/>
    </xsl:if>
    <!--New OG Tags for Facebook-->
    <xsl:apply-templates select="." mode="opengraphdata"/>
    <meta property="og:url" content="{$href}" />

    <!--json-ld-->
    <xsl:apply-templates select="." mode="json-ld"/>
    <xsl:choose>
      <xsl:when test="ContentDetail/Content[@metaKeywords!='']">
        <meta name="keywords" content="{ContentDetail/Content/@metaKeywords}"/>
      </xsl:when>
       <xsl:when test="Contents/Content[@name='MetaKeywords' or @name='metaKeywords']">
         <meta name="keywords" content="{Contents/Content[@name='MetaKeywords' or @name='metaKeywords']}{Contents/Content[@name='MetaKeywords-Specific']}"/>
      </xsl:when>
    
    </xsl:choose>
   
    <xsl:if test="$currentPage/DisplayName/@noindex='true' or (ContentDetail/Content and not(ContentDetail/Content[@parId=/Page/@id]))">
      <!--This content is to be found elsewhere on the site and should not be indexed again-->
      <meta name="ROBOTS" content="NOINDEX, NOFOLLOW"/>
    </xsl:if>

    <!-- STOP SEARCH ENGINES INDEXING DS01 SITES IF SOME PLUM HAS PUBLISHED IT ON THE INTERWEBS -->
    <xsl:if test="contains(/Page/Request/ServerVariables/Item[@name='SERVER_NAME']/node(),'ds01.eonic') and contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT']/node(),'bot')">
      <xsl:comment>STOP SEARCH ENGINES INDEXING DS01 SITES IF SOME PLUM HAS PUBLISHED IT ON THE INTERWEBS</xsl:comment>
      <meta name="ROBOTS" content="NOINDEX, NOFOLLOW"/>
    </xsl:if>
    <xsl:if test="Contents/Content[@name='MetaLocation' or @name='metaLocation']">
      <meta name="location" content="{Contents/Content[@name='MetaLocation' or @name='metaLocation']}"/>
    </xsl:if>
    <xsl:if test="Contents/Content[@name='MetaCategories' or @name='metaCategories']">
      <meta name="categories" content="{Contents/Content[@name='MetaCategories' or @name='metaCategories']}"/>
    </xsl:if>
    <xsl:if test="Contents/Content[@name='MetaClassification' or @name='metaClassification']">
      <meta name="classification" content="{Contents/Content[@name='MetaClassification' or @name='metaClassification']}"/>
    </xsl:if>
    <xsl:if test="Contents/Content[@name='MetaAuthor' or @name='metaAuthor']">
      <meta name="author" content="{Contents/Content[@name='MetaAuthor' or @name='metaAuthor']}"/>
    </xsl:if>
    <xsl:if test="Contents/Content[@name='MetaAbstract' or @name='metaAbstract']">
      <meta name="abstract" content="{Contents/Content[@name='MetaAbstract' or @name='metaAbstract']}"/>
    </xsl:if>
    <xsl:if test="Contents/Content[@name='MetaGoogleVerify']">
      <meta name="google-site-verification" content="{Contents/Content[@name='MetaGoogleVerify']}"/>
    </xsl:if>
    <xsl:if test="Contents/Content[@name='MetaYahooVerify']">
      <meta name="y_key" content="{Contents/Content[@name='MetaYahooVerify']}"/>
    </xsl:if>
    <xsl:if test="Contents/Content[@name='MetaMSNVerify']">
      <meta name="msvalidate.01" content="{Contents/Content[@name='MetaMSNVerify']}"/>
    </xsl:if>
    <xsl:if test="Contents/Content[@name='fb-admins']">
      <meta property="fb:admins" content="{Contents/Content[@name='fb-admins']}"/>
    </xsl:if>
    <xsl:if test="Contents/Content[@name='fb-app_id']">
      <meta property="fb:app_id" content="{Contents/Content[@name='fb-app_id']}"/>
    </xsl:if>
    <xsl:if test="Contents/Content[@name='fb-pages_id']">
      <meta property="fb:pages" content="{Contents/Content[@name='fb-pages_id']}"/>
    </xsl:if>
    <xsl:if test="Contents/Content[@name='pinterestVerify']">
      <meta name="p:domain_verify" content="{Contents/Content[@name='pinterestVerify']}"/>
    </xsl:if>
    <xsl:if test="Contents/Content[@name='GooglePlusPageID']">
      <link href="https://plus.google.com/{Contents/Content[@name='GooglePlusPageID']}" rel="publisher" />
    </xsl:if>
    <xsl:if test="Contents/Content[@name='globalsign-domain-verification']">
      <meta name="_globalsign-domain-verification" content="{Contents/Content[@name='globalsign-domain-verification']}" />
    </xsl:if>
    <!-- important for web indexes -->
    <meta name="generator" content="{/Page/Request/ServerVariables/Item[@name='GENERATOR']/node()}"/>

    <xsl:apply-templates select="." mode="dublincore"/>
    <xsl:apply-templates select="." mode="sitemeta"/>
    
  </xsl:template>


  <xsl:template match="Page" mode="sitemeta">
    
  </xsl:template>
  
  
  <xsl:template match="Page" mode="dublincore">
    
     <xsl:if test="Contents/Content[@type='MetaData' and starts-with(@name,'DC')]">
      <link rel="schema.DC" href="http://purl.org/dc/elements/1.1/"/>
    </xsl:if>
    <xsl:if test="Contents/Content[@name='DCTitle']">
      <meta name="DC.Title" content="{Contents/Content[@type='MetaData' and @name='DCTitle']}" />
    </xsl:if>
    <xsl:if test="Contents/Content[@name='DCCreator']">
      <meta name="DC.Creator" content="{Contents/Content[@type='MetaData' and @name='DCCreator']}" />
    </xsl:if>
    <xsl:if test="Contents/Content[@name='DCSubject']">
      <meta name="DC.Subject" content="{Contents/Content[@type='MetaData' and @name='DCSubject']}" />
    </xsl:if>
    <xsl:if test="Contents/Content[@name='DCDescription']">
      <meta name="DC.Description" content="{Contents/Content[@type='MetaData' and @name='DCDescription']}" />
    </xsl:if>
    <xsl:if test="Contents/Content[@name='DCDate']">
      <meta name="DC.Date" content="{Contents/Content[@type='MetaData' and @name='DCDate']}" />
    </xsl:if>
    <xsl:if test="Contents/Content[@name='DCType']">
      <meta name="DC.Type" content="{Contents/Content[@type='MetaData' and @name='DCType']}" />
    </xsl:if>
    <xsl:if test="Contents/Content[@name='DCFormat']">
      <meta name="DC.Format" content="{Contents/Content[@type='MetaData' and @name='DCFormat']}" />
    </xsl:if>
    <xsl:if test="Contents/Content[@name='DCLanguage']">
      <meta name="DC.Language" content="{Contents/Content[@type='MetaData' and @name='DCLanguage']}" />
    </xsl:if>
    <xsl:if test="Contents/Content[@name='DCRelation']">
      <meta name="DC.Relation" content="{Contents/Content[@type='MetaData' and @name='DCRelation']}" />
    </xsl:if>
    <xsl:if test="Contents/Content[@name='DCCoverage']">
      <meta name="DC.Coverage" content="{Contents/Content[@type='MetaData' and @name='DCCoverage']}" />
    </xsl:if>
  </xsl:template>



  <xsl:template match="Page" mode="opengraphdata">
    <xsl:variable name="pageTitle">
      <xsl:apply-templates select="." mode="PageTitle"/>
    </xsl:variable>
    <meta property="og:type" content="website" />
    <xsl:choose>
      <xsl:when test="/Page/Contents/Content[@type='MetaData' and @name='ogTitle']">
        <meta property="og:title">
          <xsl:attribute name="content">
            <xsl:value-of select="/Page/Contents/Content[@type='MetaData' and @name='ogTitle']/node()"/>
          </xsl:attribute>
        </meta>
      </xsl:when>
      <xsl:otherwise>
        <meta property="og:title" content="{$pageTitle}"/>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:variable name="contentMetaDescription">
      <xsl:call-template name="truncate-string">
        <xsl:with-param name="text">
          <xsl:value-of select="Contents/Content[@name='MetaDescription' or @name='metaDescription']"/>
          <xsl:value-of select="Content[@name='MetaDescription-Specific']"/>
        </xsl:with-param>
        <xsl:with-param name="length" select="160"/>
      </xsl:call-template>
    </xsl:variable>
    <meta property="og:description" content="{$contentMetaDescription}"/>
    <xsl:choose>
      <xsl:when test="/Page/Contents/Content[@type='MetaData' and @name='ogImage']">
        <meta property="og:image">
          <xsl:attribute name="content">
            <xsl:value-of select="/Page/Contents/Content[@type='MetaData' and @name='ogImage']/node()"/>
          </xsl:attribute>
        </meta>
      </xsl:when>
      <xsl:otherwise>
        <xsl:if test="$currentPage/Images/img[@class='display']/@src and $currentPage/Images/img[@class='display']/@src!=''">
          <meta property="og:image">
            <xsl:attribute name="content">
              <xsl:if test="$siteURL=''">
                <xsl:text>http</xsl:text>
                <xsl:if test="$page/Request/ServerVariables/Item[@name='HTTPS']='on'">s</xsl:if>
                <xsl:text>://</xsl:text>
                <xsl:value-of select="$page/Request/ServerVariables/Item[@name='SERVER_NAME']"/>
              </xsl:if>
              <xsl:value-of select="$currentPage/Images/img[@class='display']/@src"/>
            </xsl:attribute>
            <!-- could add code for thumbnailizer in here -->
          </meta>
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="Page[ContentDetail]" mode="opengraphdata">
    <xsl:variable name="pageTitle">
      <xsl:apply-templates select="." mode="PageTitle"/>
    </xsl:variable>
    <xsl:variable name="contentMetaDescription">
      <xsl:call-template name="truncate-string">
        <xsl:with-param name="text">
          <xsl:apply-templates select="/Page/ContentDetail/Content" mode="getContentMetaDescription"/>
        </xsl:with-param>
        <xsl:with-param name="length" select="160"/>
      </xsl:call-template>
    </xsl:variable>
    <meta property="og:title" content="{$pageTitle}"/>
    <meta property="og:description" content="{$contentMetaDescription}"/>
    <meta property="og:image">
      <xsl:attribute name="content">
        <xsl:if test="$siteURL=''">
          <xsl:text>http</xsl:text>
          <xsl:if test="$page/Request/ServerVariables/Item[@name='HTTPS']='on'">s</xsl:if>
          <xsl:text>://</xsl:text>
          <xsl:value-of select="$page/Request/ServerVariables/Item[@name='SERVER_NAME']"/>
        </xsl:if>
        <xsl:choose>
          <!-- IF use display -->
          <xsl:when test="ContentDetail/Content/Images/img[@class='display']/@src and ContentDetail/Content/Images/img[@class='display']/@src!=''">
            <xsl:value-of select="ContentDetail/Content/Images/img[@class='display']/@src"/>
          </xsl:when>
          <!-- Else Full Size use that -->
          <xsl:when test="ContentDetail/Content/Images/img[@class='detail']/@src and ContentDetail/Content/Images/img[@class='detail']/@src!=''">
            <xsl:value-of select="ContentDetail/Content/Images/img[@class='detail']/@src"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="default-og-img"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
    </meta>
    <xsl:apply-templates select="ContentDetail/Content" mode="opengraphdata"/>
  </xsl:template>

  <xsl:template match="Content" mode="opengraphdata">
    <meta property="og:type" content="article" />
  </xsl:template>

  <!--json-ld-->
  <xsl:template match="Page" mode="json-ld">
  </xsl:template>

  <xsl:template match="Page[ContentDetail]" mode="json-ld">
    <xsl:apply-templates select="ContentDetail/Content" mode="json-ld"/>
  </xsl:template>

  <xsl:template match="Content" mode="json-ld">
  </xsl:template>

  <xsl:template match="Page" mode="getMetaDescription">
    <!-- when detail get body -->
    <xsl:choose>
      <xsl:when test="/Page/ContentDetail">
        <xsl:choose>
          <xsl:when test="/Page/ContentDetail/Content/@metaDescription!=''">
            <meta name="description" content="{/Page/ContentDetail/Content/@metaDescription}"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:variable name="contentMetaDescription">
              <xsl:call-template name="truncate-string">
                <xsl:with-param name="text">
                  <xsl:apply-templates select="/Page/ContentDetail/Content" mode="getContentMetaDescription"/>
                </xsl:with-param>
                <xsl:with-param name="length" select="160"/>
              </xsl:call-template>
            </xsl:variable>
            <meta name="description" content="{$contentMetaDescription}"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <meta name="description" content="{Contents/Content[@name='MetaDescription' or @name='metaDescription']}{Content[@name='MetaDescription-Specific']}"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>



  <xsl:template match="Content" mode="getContentMetaDescription">
    <xsl:choose>
      <xsl:when test="Body/node()">
        <xsl:apply-templates select="Body/node()" mode="flattenXhtml"/>
      </xsl:when>
      <xsl:when test="Description/node()">
        <xsl:apply-templates select="Description/node()" mode="flattenXhtml"/>
      </xsl:when>
      <xsl:when test="Strapline/node()">
        <xsl:apply-templates select="Strapline/node()" mode="flattenXhtml"/>
      </xsl:when>
      <xsl:when test="Strap/node()">
        <xsl:apply-templates select="Strap/node()" mode="flattenXhtml"/>
      </xsl:when>
      <xsl:when test="Intro/node()">
        <xsl:apply-templates select="Intro/node()" mode="flattenXhtml"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="." mode="getDisplayName"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!--  ==  Canonical links  ======================================================================  -->
  <xsl:template match="Page" mode="canonicalLink">
    <!-- not admin -->
    <xsl:if test="not(/Page/@adminMode)">
      <!-- not cart page-->
      <xsl:if test="not(Cart/Order and Cart/Order/@cmd!='')">
        <!-- not a steppered page -->
        <xsl:if test="not(/Page/Request/QueryString/Item[starts-with(@name,'startPos')])">

          <xsl:variable name="href">
            <xsl:if test="not(starts-with($currentPage/@url,'http'))">
              <xsl:if test="$siteURL=''">
                <xsl:text>http</xsl:text>
                <xsl:if test="$page/Request/ServerVariables/Item[@name='HTTPS']='on'">s</xsl:if>
                <xsl:text>://</xsl:text>
                <xsl:value-of select="$page/Request/ServerVariables/Item[@name='SERVER_NAME']"/>
              </xsl:if>
            </xsl:if>
            <xsl:choose>
              <xsl:when test="/Page/ContentDetail">
                <xsl:apply-templates select="/Page/ContentDetail/Content" mode="getHref"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:apply-templates select="$currentPage" mode="getHref"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:if test="$href!=''">
            <link rel="canonical" href="{$href}"/>
          </xsl:if>
        </xsl:if>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Page" mode="getPageThumbnail">

    <xsl:choose>
      <xsl:when test="/Page/Cart/Order/@cmd and /Page/Cart/Order/@cmd!=''"></xsl:when>

      <xsl:when test="/Page/ContentDetail">
        <xsl:variable name="content" select="/Page/ContentDetail/Content" />

        <xsl:choose>
          <!-- IF Thumbnail use that -->
          <xsl:when test="$content/Images/img[@class='thumbnail']/@src!=''">
            <xsl:value-of select="$siteURL"/>
            <xsl:value-of select="$content/Images/img[@class='thumbnail']/@src"/>
          </xsl:when>
          <!-- IF Full Size use that -->
          <xsl:when test="$content/Images/img[@class='detail']/@src!=''">
            <xsl:value-of select="$siteURL"/>
            <xsl:value-of select="$content/Images/img[@class='detail']/@src"/>
          </xsl:when>
          <!-- ELSE use display -->
          <xsl:otherwise>
            <xsl:if test="$content/Images/img[@class='display' and @src and @src!='']">
              <xsl:value-of select="$siteURL"/>
              <xsl:value-of select="$content/Images/img[@class='display']/@src"/>
            </xsl:if>

          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>

      <xsl:otherwise>
        <xsl:if test="/Page/Contents/Content[@name='Logo' and img/@src and img/@src!='']">
          <xsl:value-of select="$siteURL"/>
          <xsl:value-of select="/Page/Contents/Content[@name='Logo']/img/@src"/>
        </xsl:if>

      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!--  ##  BUILD PAGE BODY TAG  ###################################################################  -->
  <xsl:template match="Page" mode="bodyBuilder">
    <body>
      <xsl:attribute name="id">
        <xsl:text>page</xsl:text>
        <xsl:value-of select="@id"/>
        <xsl:if test="@artid!=''">
          <xsl:text>-art</xsl:text>
          <xsl:value-of select="@artid"/>
        </xsl:if>
      </xsl:attribute>
      <xsl:if test="$GoogleTagManagerID!=''">
        <!-- Google Tag Manager (noscript) -->
        <noscript>
          <iframe src="https://www.googletagmanager.com/ns.html?id={$GoogleTagManagerID}" height="0" width="0" style="display:none;visibility:hidden"></iframe>
        </noscript>
        <!-- End Google Tag Manager (noscript) -->
      </xsl:if>
      <xsl:apply-templates select="." mode="bodyStyle"/>
      <xsl:apply-templates select="." mode="bodyDisplay"/>
      <xsl:apply-templates select="." mode="footerJs"/>
    </body>
  </xsl:template>

  <xsl:template match="Page" mode="bodyStyle">
    <!-- Placeholder to overide -->
    <xsl:attribute name="class">
      <xsl:value-of select="$MatchHeightType"/>
      <xsl:text>-body</xsl:text>
      <xsl:if test="@adminMode='false'">
        normalMode
      </xsl:if>
      <xsl:if test="@previewMode='true'"> previewMode</xsl:if>
    </xsl:attribute>
    <xsl:apply-templates select="/Page/Contents/Content[@type='MetaData' and @name='MetaLeadForensicsID']" mode="MetaLeadForensicsCode"/>
  </xsl:template>

  <xsl:template match="Page" mode="bodyDisplay">
    <xsl:apply-templates select="." mode="bodyDipslay"/>
  </xsl:template>


  <!--  ############################################################################################ -->
  <!--  ##  STRING FUNCTIONS  ###################################################################### -->
  <!--  ############################################################################################ -->

  <!-- escapes naught characters for use with JavaScript -->
  <xsl:template name="escape-js">
    <xsl:param name="string"/>
    <!-- replace all characters not matching SingleStringCharacter
        or DoubleStringCharacter according to ECMA262.  Note: not all
        characters that should be escaped are legal XML characters:
        "\a", "\b", "\v", and "\f" are not escaped. -->
    <xsl:if test="$string!=''">
      <xsl:value-of select="ew:escapeJs($string)"/>
    </xsl:if>
  </xsl:template>

  <xsl:template name="escape-js-html">
    <xsl:param name="string"/>
    <!-- replace all characters not matching SingleStringCharacter
        or DoubleStringCharacter according to ECMA262.  Note: not all
        characters that should be escaped are legal XML characters:
        "\a", "\b", "\v", and "\f" are not escaped. -->
    <xsl:if test="$string!=''">
      <xsl:copy-of select="ew:escapeJsHTML($string)"/>
    </xsl:if>
  </xsl:template>

  <xsl:template name="escape-js-old">
    <xsl:param name="string"/>
    <!-- replace all characters not matching SingleStringCharacter
        or DoubleStringCharacter according to ECMA262.  Note: not all
        characters that should be escaped are legal XML characters:
        "\a", "\b", "\v", and "\f" are not escaped. -->
    <xsl:call-template name="replace-string">
      <xsl:with-param name="replace">’</xsl:with-param>
      <xsl:with-param name="with">\'</xsl:with-param>
      <xsl:with-param name="text">
        <xsl:call-template name="replace-string">
          <xsl:with-param name="replace">'</xsl:with-param>
          <xsl:with-param name="with">\'</xsl:with-param>
          <xsl:with-param name="text">
            <xsl:call-template name="replace-string">
              <xsl:with-param name="replace">"</xsl:with-param>
              <xsl:with-param name="with">\"</xsl:with-param>
              <xsl:with-param name="text">
                <xsl:call-template name="replace-string">
                  <xsl:with-param name="replace">
                    <xsl:text>#9;</xsl:text>
                  </xsl:with-param>
                  <xsl:with-param name="with">\t</xsl:with-param>
                  <xsl:with-param name="text">
                    <xsl:call-template name="replace-string">
                      <xsl:with-param name="replace">
                        <xsl:text>&#10;</xsl:text>
                      </xsl:with-param>
                      <xsl:with-param name="with">\n</xsl:with-param>
                      <xsl:with-param name="text">
                        <xsl:call-template name="replace-string">
                          <xsl:with-param name="replace">
                            <text>&#13;</text>
                          </xsl:with-param>
                          <xsl:with-param name="with">\r</xsl:with-param>
                          <xsl:with-param name="text">
                            <xsl:call-template name="replace-string">
                              <xsl:with-param name="replace">\</xsl:with-param>
                              <xsl:with-param name="with">\\</xsl:with-param>
                              <xsl:with-param name="text">
                                <xsl:copy-of select="$string"/>
                              </xsl:with-param>
                            </xsl:call-template>
                          </xsl:with-param>
                        </xsl:call-template>
                      </xsl:with-param>
                    </xsl:call-template>
                  </xsl:with-param>
                </xsl:call-template>
              </xsl:with-param>
            </xsl:call-template>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>


  <!-- splits a string into XML items -->
  <xsl:template name="split-string">
    <xsl:param name="list" />
    <xsl:param name="seperator" />
    <xsl:variable name="newlist" select="concat(normalize-space($list),$seperator)" />
    <xsl:variable name="first" select="substring-before($newlist, $seperator)" />
    <xsl:variable name="remaining" select="substring-after($newlist, $seperator)" />
    <xsl:if test="$first!=''">
      <item>
        <xsl:value-of select="$first" />
      </item>
      <xsl:call-template name="split-string">
        <xsl:with-param name="list" select="$remaining" />
        <xsl:with-param name="seperator" select="$seperator" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- -->
  <!--####################### Page Level Templates, can be overridden later. ##############################-->
  <!-- -->
  <xsl:template match="Content[@type='FeedControl']" mode="feedLinks">
    <xsl:variable name="parId" select="@parId"/>
    <xsl:variable name="feedLink">
      <xsl:value-of select="$siteURL"/>
      <xsl:text>/ewcommon/feeds/rss/feed.ashx?pgid=</xsl:text>
      <xsl:value-of select="$parId"/>
    </xsl:variable>
    <link type="application/rss+xml" href="{$feedLink}" title="{channel/title/node()}" rel="alternate" />
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[@type='BlogSettings']" mode="feedLinks">
    <xsl:variable name="parId" select="@parId"/>
    <xsl:variable name="feedLink">
      <xsl:value-of select="$siteURL"/>
      <xsl:text>/ewcommon/feeds/rss/feed.ashx?pgid=</xsl:text>
      <xsl:value-of select="$parId"/>
      <xsl:text>&amp;settingsId=</xsl:text>
      <xsl:value-of select="@id"/>
      <xsl:if test="/Page/ContentDetail">
        <xsl:text>&amp;artid=</xsl:text>
        <xsl:value-of select="/Page/ContentDetail/Content/@id"/>
      </xsl:if>
    </xsl:variable>
    <link rel="alternate" type="application/rss+xml" title="{BlogTitle/node()}" href="{$feedLink}"/>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[@type='BlogSettings' and /Page/ContentDetail]" mode="feedLinks">
  </xsl:template>

  <!--   ################################################   Google Urchin Analytics Code   ##################################################   -->

  <!-- OLD CODE - SEE TEMPLATE BELOW - FOR ASYNCRONOUS CODE -->
  <xsl:template match="Content" mode="googleAnalyticsCode">
    <!-- OLD CODE - SEE TEMPLATE BELOW - FOR ASYNCRONOUS CODE -->
    <xsl:if test="not(/Page/@adminMode)">
      <xsl:if test="node()!=''">
        <script type="text/javascript">
          <xsl:text>var gaJsHost = (("https:" == document.location.protocol) ? "https://ssl." : "http://www.");</xsl:text>
          <xsl:text>document.write(unescape("%3Cscript src='" + gaJsHost + "google-analytics.com/ga.js' type='text/javascript'%3E%3C/script%3E"));</xsl:text>
        </script>
        <script type="text/javascript">
          <xsl:text>var pageTracker = _gat._getTracker("</xsl:text>
          <xsl:value-of select="node()"/>
          <xsl:text>");</xsl:text>
          <xsl:text>pageTracker._initData();</xsl:text>
          <xsl:if test="/Page/Cart">
            <!-- allows cross domain tracking for secure sites -->
            <!--xsl:choose>
						<xsl:when test="contains(substring-after(/Page/@baseUrl,'.'),'/')">
              <xsl:text>pageTracker._setDomainName(".</xsl:text><xsl:value-of select="substring-before(substring-after(/Page/@baseUrl,'.'),'/')"/><xsl:text>");</xsl:text>
						</xsl:when>
						<xsl:otherwise>
              <xsl:text>pageTracker._setDomainName(".</xsl:text><xsl:value-of select="substring-after(/Page/@baseUrl,'.')"/><xsl:text>");</xsl:text>
						</xsl:otherwise>
					</xsl:choose-->
            <xsl:text>pageTracker._setDomainName("none");</xsl:text>
            <xsl:text>pageTracker._setAllowLinker(true);</xsl:text>
            <!--xsl:text>pageTracker._setAllowHash(false);</xsl:text-->
          </xsl:if>
          <xsl:text>pageTracker._trackPageview(</xsl:text>
          <xsl:apply-templates select="/Page" mode="GoogleAnalyticsSpoofPage"/>
          <xsl:text>);</xsl:text>
          <xsl:apply-templates select="/Page" mode="GoogleAnalyticsNewTransaction"/>
        </script>
      </xsl:if>
    </xsl:if>
    <!-- OLD CODE - SEE TEMPLATE BELOW - FOR ASYNCRONOUS CODE -->
  </xsl:template>


  <!-- ASYNCRONOUS ANALYTICS CODE -->
  <xsl:template match="Content" mode="googleAnalyticsCode">
    <xsl:if test="not(/Page/@adminMode) and not(/Page/@previewMode='true')">
      <xsl:if test="node()!=''">
        <script type="text/javascript">
          <xsl:choose>
            <xsl:when test="//Content[@type='CookiePolicy' and @defaultBehaviour='decline']">
              <xsl:text>if (jQuery.cookie('cc_cookie_accept') == "cc_cookie_accept") {</xsl:text>
            </xsl:when>
            <xsl:when test="//Content[@type='CookiePolicy']">
              <xsl:text>if (jQuery.cookie('cc_cookie_decline') == "cc_cookie_decline") {</xsl:text>
              <xsl:text>} else {</xsl:text>
            </xsl:when>
          </xsl:choose>

          <xsl:text>var _gaq = _gaq || [];</xsl:text>

          <!-- set account -->
          <xsl:text>_gaq.push(['_setAccount', '</xsl:text>
          <xsl:value-of select="node()"/>
          <xsl:text>']);</xsl:text>

          <!-- _trackPageview -->
          <xsl:apply-templates select="/Page" mode="googleanalytics_trackPageview" />

          <!-- If cart disable domain tracking-->
          <xsl:if test="/Page/Cart">
            <xsl:if test="/Page/Cart/Order/@cmd='ShowInvoice'">
              <!-- Create/Add Trans object -->
              <xsl:apply-templates select="/Page" mode="googleanalytics_addTrans" />
              <!-- Add Items to Trans Object -->
              <xsl:apply-templates select="/Page/Cart/Order/Item" mode="googleanalytics_addItem" />
              <!-- Track trans object to ANALytics.-->
              <xsl:apply-templates select="/" mode="googleanalytics_trackTrans" />
            </xsl:if>
            <xsl:text>_gaq.push(['_setDomainName', 'none']);</xsl:text>
            <xsl:text>_gaq.push(['_setAllowLinker', true]);</xsl:text>
          </xsl:if>

          <xsl:text>(function() {</xsl:text>
          <xsl:text>var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;</xsl:text>
          <xsl:text>ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';</xsl:text>
          <xsl:text>var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);</xsl:text>
          <xsl:text>})();</xsl:text>
          <xsl:if test="//Content[@type='CookiePolicy']">
            <xsl:text>}</xsl:text>
          </xsl:if>
        </script>
      </xsl:if>
    </xsl:if>
  </xsl:template>
  <!-- -->
  <xsl:template match="Page" mode="googleanalytics_trackPageview">
    <xsl:text>_gaq.push(['_trackPageview']);</xsl:text>
  </xsl:template>
  <!-- -->
  <xsl:template match="Page[Cart/Order and Cart/Order/@cmd!='']" mode="googleanalytics_trackPageview">
    <xsl:text>_gaq.push(['_trackPageview', '</xsl:text>
    <xsl:text>/ShoppingCart/</xsl:text>
    <xsl:value-of select="Cart/Order/@cmd"/>
    <xsl:text>']);</xsl:text>
  </xsl:template>
  <!-- -->
  <xsl:template match="Page[//Content[descendant-or-self::alert/node()='Message Sent']]" mode="googleanalytics_trackPageview">
    <xsl:text>_gaq.push(['_trackPageview', '</xsl:text>
    <xsl:apply-templates select="$currentPage" mode="getHref" />
    <xsl:if test="$currentPage/@id!=//Menu/MenuItem/@id">
      <xsl:text>/</xsl:text>
    </xsl:if>
    <xsl:value-of select="//Content[descendant-or-self::alert/node()='Message Sent']/descendant-or-self::submission/@id"/>
    <xsl:text>/Message-Sent</xsl:text>
    <xsl:text>']);</xsl:text>
  </xsl:template>

  <xsl:template match="Page[//Content[descendant-or-self::alert/node()='Message Sent']]" mode="googleanalytics_trackPageview">
    <xsl:text>_gaq.push(['_trackPageview', '</xsl:text>
    <xsl:apply-templates select="$currentPage" mode="getHref" />
    <xsl:if test="$currentPage/@id!=//Menu/MenuItem/@id">
      <xsl:text>/</xsl:text>
    </xsl:if>
    <xsl:value-of select="//Content[descendant-or-self::alert/node()='Message Sent']/descendant-or-self::submission/@id"/>
    <xsl:text>/Message-Sent</xsl:text>
    <xsl:text>']);</xsl:text>
  </xsl:template>

  <xsl:template match="Page[//Content[@type='Module' and (@moduleType='CampaignMonitorSubscribe')]/descendant-or-self::div[@class='subscribed']]" mode="googleanalytics_trackPageview">
    <xsl:text>_gaq.push(['_trackPageview', '</xsl:text>
    <xsl:apply-templates select="$currentPage" mode="getHref" />
    <xsl:if test="$currentPage/@id!=//Menu/MenuItem/@id">
      <xsl:text>/</xsl:text>
    </xsl:if>
    <xsl:value-of select="//Content[@type='Module' and (@moduleType='CampaignMonitorSubscribe')]/@name"/>
    <xsl:text>/Subscribed</xsl:text>
    <xsl:text>']);</xsl:text>
  </xsl:template>

  <!-- ECommerce Log Transaction-->
  <xsl:template match="Page" mode="googleanalytics_addTrans">
    <xsl:text>_gaq.push(['_addTrans'</xsl:text>
    <!-- order ID -->
    <xsl:text>, '</xsl:text>
    <xsl:value-of select="Cart/Order/@InvoiceRef"/>
    <xsl:text>'</xsl:text>
    <!-- Affliliate / store name -->
    <xsl:text>, '</xsl:text>
    <xsl:variable name="siteName">
      <xsl:call-template name="getXmlSettings">
        <xsl:with-param name="sectionName" select="'web'"/>
        <xsl:with-param name="valueName" select="'SiteName'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$siteName!=''">
        <xsl:call-template name="escape-js">
          <xsl:with-param name="string">
            <xsl:value-of select="$siteName"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>None</xsl:otherwise>
    </xsl:choose>
    <xsl:text>'</xsl:text>
    <!-- Order Total -->
    <xsl:text>, '</xsl:text>
    <xsl:value-of select="Cart/Order/@totalNet"/>
    <xsl:text>'</xsl:text>
    <!-- Tax rate -->
    <xsl:text>, '</xsl:text>
    <xsl:value-of select="Cart/Order/@vatAmt"/>
    <xsl:text>'</xsl:text>
    <!-- Shipping cost -->
    <xsl:text>, '</xsl:text>
    <xsl:value-of select="Cart/Order/@shippingCost"/>
    <xsl:text>'</xsl:text>
    <!-- City -->
    <xsl:text>, '</xsl:text>
    <xsl:call-template name="escape-js">
      <xsl:with-param name="string">
        <xsl:value-of select="Cart/Order/Contact[@type='Delivery Address']/City/node()"/>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:text>'</xsl:text>
    <!-- State -->
    <xsl:text>, '</xsl:text>
    <xsl:call-template name="escape-js">
      <xsl:with-param name="string">
        <xsl:value-of select="Cart/Order/Contact[@type='Delivery Address']/State/node()"/>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:text>'</xsl:text>
    <!-- Country -->
    <xsl:text>, '</xsl:text>
    <xsl:call-template name="escape-js">
      <xsl:with-param name="string">
        <xsl:value-of select="Cart/Order/Contact[@type='Delivery Address']/Country/node()"/>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:text>'</xsl:text>
    <xsl:text>]);</xsl:text>
  </xsl:template>

  <!-- Log Order Items-->
  <xsl:template match="Item" mode="googleanalytics_addItem">
    <xsl:text>_gaq.push(['_addItem'</xsl:text>
    <!-- Order ID -->
    <xsl:text>, '</xsl:text>
    <xsl:call-template name="escape-js">
      <xsl:with-param name="string">
        <xsl:value-of select="/Page/Cart/Order/@InvoiceRef"/>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:text>'</xsl:text>
    <!-- Stock Code -->
    <xsl:text>, '</xsl:text>
    <xsl:call-template name="escape-js">
      <xsl:with-param name="string">
        <xsl:choose>
          <xsl:when test="productDetail/StockCode!=''">
            <xsl:value-of select="productDetail/StockCode/node()"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="@id"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:text>'</xsl:text>
    <!-- Product Name -->
    <xsl:text>, '</xsl:text>
    <xsl:call-template name="escape-js">
      <xsl:with-param name="string">
        <xsl:value-of select="productDetail/Name/node()"/>
      </xsl:with-param>
    </xsl:call-template>

    <xsl:text>'</xsl:text>
    <!-- Category -->
    <xsl:text>, '</xsl:text>
    <xsl:call-template name="escape-js">
      <xsl:with-param name="string">
        <xsl:choose>
          <xsl:when test="Item">
            <xsl:for-each select="Item">
              <xsl:value-of select="option/@groupName"/>
              <xsl:text>: </xsl:text>
              <xsl:value-of select="option/@name"/>
              <xsl:text>, </xsl:text>
            </xsl:for-each>
          </xsl:when>
          <xsl:otherwise>
            <xsl:variable name="parId">
              <xsl:value-of select="@parId"/>
            </xsl:variable>
            <xsl:value-of select="//MenuItem[@id=$parId]/@name" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:text>'</xsl:text>
    <!-- Unit Price -->
    <xsl:text>, '</xsl:text>
    <xsl:apply-templates select="." mode="getTotalUnitPrice"/>
    <xsl:text>'</xsl:text>
    <!-- Quantity -->
    <xsl:text>, '</xsl:text>
    <xsl:value-of select="@quantity"/>
    <xsl:text>'</xsl:text>

    <xsl:text>]);</xsl:text>
  </xsl:template>

  <!-- Track Trans - Called After All other details are populated - Send details to Google.-->
  <xsl:template match="Item" mode="googleanalytics_trackTrans">
    <xsl:text>_gaq.push(['_trackTrans']);</xsl:text>
  </xsl:template>
  <xsl:template match="/" mode="googleanalytics_trackTrans">
    <xsl:text>_gaq.push(['_trackTrans']);</xsl:text>
  </xsl:template>

  <!-- Calculates Item price - inc any price additional options.-->
  <xsl:template match="Item" mode="getTotalUnitPrice">
    <xsl:variable name="itemPrice">
      <xsl:value-of select="@price"/>
    </xsl:variable>
    <!-- Calculate options prices -->
    <xsl:variable name="optionsPrice">
      <xsl:choose>
        <xsl:when test="Item">
          <xsl:value-of select="sum(Item/@price)"/>
        </xsl:when>
        <xsl:otherwise>0</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:apply-templates select="/Page" mode="formatPrice">
      <xsl:with-param name="price" select="number($itemPrice) + number($optionsPrice)"/>
      <xsl:with-param name="currency" select="''"/>
    </xsl:apply-templates>
  </xsl:template>

  <!-- UNIVERSAL ANALYTICS CODE -->

  <xsl:template match="Page" mode="googleUniversalAnalyticsCode">
    <xsl:if test="not(/Page/@adminMode) and not(/Page/@previewMode='true')">
      <script id="GoogleAnalyticsUniversal" data-GoogleAnalyticsUniversalID="{$GoogleAnalyticsUniversalID}">
        (function(i,s,o,g,r,a,m){i['GoogleAnalyticsObject']=r;i[r]=i[r]||function(){
        (i[r].q=i[r].q||[]).push(arguments)},i[r].l=1*new Date();a=s.createElement(o),
        m=s.getElementsByTagName(o)[0];a.async=1;a.src=g;m.parentNode.insertBefore(a,m)
        })(window,document,'script','//www.google-analytics.com/analytics.js','ga');
        ga('create', '<xsl:value-of select="$GoogleAnalyticsUniversalID"/>
        <xsl:text>', 'auto'</xsl:text>
        <xsl:if test="User">
          <xsl:text>,{'userId': '</xsl:text>
          <xsl:value-of select="User/@id"/>
          <xsl:text>'}</xsl:text>
        </xsl:if>
        <xsl:text>);</xsl:text>

        <!-- If cart -->
        <xsl:if test="/Page/Cart">
          ga('require', 'ec');
          <xsl:choose>
            <xsl:when test="/Page/Cart/Order/@cmd='Cart'">
              <xsl:apply-templates select="/Page/Cart/Order/Item" mode="ga-universal-addProduct" />
              ga('ec:setAction','checkout', {'step': 1});
            </xsl:when>
            <xsl:when test="/Page/Cart/Order/@cmd='Logon'">
              <xsl:apply-templates select="/Page/Cart/Order/Item" mode="ga-universal-addProduct" />
              ga('ec:setAction','checkout', {'step': 2});
            </xsl:when>
            <xsl:when test="/Page/Cart/Order/@cmd='Billing'">
              <xsl:apply-templates select="/Page/Cart/Order/Item" mode="ga-universal-addProduct" />
              ga('ec:setAction','checkout', {'step': 3});
            </xsl:when>
            <xsl:when test="/Page/Cart/Order/@cmd='Delivery'">
              <xsl:apply-templates select="/Page/Cart/Order/Item" mode="ga-universal-addProduct" />
              ga('ec:setAction','checkout', {'step': 3});
            </xsl:when>
            <xsl:when test="/Page/Cart/Order/@cmd='ChoosePaymentShippingOption'">
              <xsl:apply-templates select="/Page/Cart/Order/Item" mode="ga-universal-addProduct" />
              ga('ec:setAction','checkout', {'step': 4});
            </xsl:when>
            <xsl:when test="/Page/Cart/Order/@cmd='EnterPaymentDetails'">
              <xsl:apply-templates select="/Page/Cart/Order/Item" mode="ga-universal-addProduct" />
              ga('ec:setAction','checkout', {'step': 5});
            </xsl:when>
            <xsl:when test="/Page/Cart/Order/@cmd='ShowInvoice'">
              <xsl:apply-templates select="/Page/Cart/Order/Item" mode="ga-universal-addProduct" />
              <!-- Create/Add Trans object -->
              <xsl:apply-templates select="/Page" mode="ga-universal-purchase" />
            </xsl:when>
            <xsl:when test="/Page/ContentDetail/Content[@type='Product']">
              <xsl:apply-templates select="/Page/ContentDetail/Content[@type='Product']" mode="ga-universal-addProduct" />
              ga('ec:setAction', 'detail');
            </xsl:when>
            <xsl:otherwise>
              <xsl:apply-templates select="/Page/Contents/Content[@type='Product']" mode="ga-universal-impression" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>
        ga('send', 'pageview');

        <!--Submission of Mailforms-->
        <xsl:for-each select="/Page/Contents/descendant-or-self::instance[@valid='true']/*[name()='emailer']">
          <xsl:text>ga('send', {hitType: 'event', eventCategory: 'MailForm', eventAction: 'submitted', eventLabel: '</xsl:text>
          <xsl:value-of select="*[name()='SubjectLine']/node()"/>
          <xsl:text>'});</xsl:text>
        </xsl:for-each>

        <xsl:for-each select="descendant-or-self::Content[@type='Module' and (@moduleType='CampaignMonitorSubscribe')]/descendant-or-self::div[@class='subscribed']">
          <xsl:text>ga('send', {hitType: 'event', eventCategory: 'MailingList', eventAction: 'subscribe', eventLabel: '</xsl:text>
          <xsl:value-of select="@listID"/>
          <xsl:text>'});</xsl:text>
        </xsl:for-each>


      </script>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Page" mode="ga-universal-purchase">
    <xsl:text>ga('ec:setAction', 'purchase', {</xsl:text>
    <!-- order ID -->
    <xsl:text>'id': '</xsl:text>
    <xsl:value-of select="Cart/Order/@InvoiceRef"/>
    <xsl:text>'</xsl:text>
    <!-- Affliliate / store name -->
    <xsl:text>,'affiliation': '</xsl:text>
    <xsl:variable name="siteName">
      <xsl:call-template name="getXmlSettings">
        <xsl:with-param name="sectionName" select="'web'"/>
        <xsl:with-param name="valueName" select="'SiteName'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$siteName!=''">
        <xsl:call-template name="escape-js">
          <xsl:with-param name="string">
            <xsl:value-of select="$siteName"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>None</xsl:otherwise>
    </xsl:choose>
    <xsl:text>'</xsl:text>
    <!-- Order Total -->
    <xsl:text>,'revenue': '</xsl:text>
    <xsl:value-of select="Cart/Order/@totalNet"/>
    <xsl:text>'</xsl:text>
    <!-- Tax rate -->
    <xsl:text>,'tax': '</xsl:text>
    <xsl:value-of select="Cart/Order/@vatAmt"/>
    <xsl:text>'</xsl:text>
    <!-- Shipping cost -->
    <xsl:text>,'shipping': '</xsl:text>
    <xsl:value-of select="Cart/Order/@shippingCost"/>
    <xsl:text>'</xsl:text>
    <!-- OfferCode -->
    <xsl:text>,'coupon': '</xsl:text>
    <xsl:value-of select="Cart/Order/@shippingCost"/>
    <xsl:text>'</xsl:text>
    <xsl:text>});</xsl:text>
  </xsl:template>

  <!-- Log Order Items-->
  <xsl:template match="Item" mode="ga-universal-addProduct">
    <xsl:text>ga('ec:addProduct', {</xsl:text>
    <!-- Stock Code -->
    <xsl:text>'id': '</xsl:text>
    <xsl:call-template name="escape-js">
      <xsl:with-param name="string">
        <xsl:choose>
          <xsl:when test="productDetail/StockCode!=''">
            <xsl:value-of select="productDetail/StockCode/node()"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="@id"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:text>'</xsl:text>
    <!-- Product Name -->
    <xsl:text>,'name': '</xsl:text>
    <xsl:call-template name="escape-js">
      <xsl:with-param name="string">
        <xsl:value-of select="productDetail/Name/node()"/>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:text>'</xsl:text>
    <!-- Category -->
    <xsl:text>,'category': '</xsl:text>
    <xsl:call-template name="escape-js">
      <xsl:with-param name="string">
        <xsl:choose>
          <xsl:when test="Item">
            <xsl:for-each select="Item">
              <xsl:value-of select="option/@groupName"/>
              <xsl:text>: </xsl:text>
              <xsl:value-of select="option/@name"/>
              <xsl:text>, </xsl:text>
            </xsl:for-each>
          </xsl:when>
          <xsl:otherwise>
            <xsl:variable name="parId">
              <xsl:value-of select="@parId"/>
            </xsl:variable>
            <xsl:value-of select="//MenuItem[@id=$parId]/@name" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:text>'</xsl:text>
    <!-- brand -->
    <xsl:text>,'brand': '</xsl:text>
    <xsl:value-of select="productDetail/Manufacturer/node()"/>
    <xsl:text>'</xsl:text>
    <!-- variant -->
    <xsl:text>,'variant': '</xsl:text>
    <xsl:value-of select="productDetail/StockCode/node()"/>
    <xsl:text>'</xsl:text>
    <!-- Unit Price -->
    <xsl:text>,'price': '</xsl:text>
    <xsl:apply-templates select="." mode="getTotalUnitPrice"/>
    <xsl:text>'</xsl:text>
    <!-- Quantity -->
    <xsl:text>,'quantity': '</xsl:text>
    <xsl:value-of select="@quantity"/>
    <xsl:text>'</xsl:text>
    <xsl:text>});</xsl:text>
  </xsl:template>

  <!-- Log Order Items-->
  <xsl:template match="Content" mode="ga-universal-addProduct">
    <xsl:text>ga('ec:addProduct', {</xsl:text>
    <!-- Stock Code -->
    <xsl:text>'id': '</xsl:text>
    <xsl:call-template name="escape-js">
      <xsl:with-param name="string">
        <xsl:choose>
          <xsl:when test="StockCode!=''">
            <xsl:value-of select="StockCode/node()"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="@id"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:text>'</xsl:text>
    <!-- Product Name -->
    <xsl:text>,'name': '</xsl:text>
    <xsl:call-template name="escape-js">
      <xsl:with-param name="string">
        <xsl:value-of select="Name/node()"/>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:text>'</xsl:text>
    <!-- Category -->
    <xsl:text>,'category': '</xsl:text>
    <xsl:call-template name="escape-js">
      <xsl:with-param name="string">
        <xsl:choose>
          <xsl:when test="Item">
            <xsl:for-each select="Item">
              <xsl:value-of select="option/@groupName"/>
              <xsl:text>: </xsl:text>
              <xsl:value-of select="option/@name"/>
              <xsl:text>, </xsl:text>
            </xsl:for-each>
          </xsl:when>
          <xsl:otherwise>
            <xsl:variable name="parId">
              <xsl:value-of select="@parId"/>
            </xsl:variable>
            <xsl:value-of select="//MenuItem[@id=$parId]/@name" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:text>'</xsl:text>
    <!-- brand -->
    <xsl:text>,'brand': '</xsl:text>
    <xsl:value-of select="Manufacturer/node()"/>
    <xsl:text>'</xsl:text>
    <!-- variant -->
    <xsl:text>,'variant': '</xsl:text>
    <xsl:value-of select="StockCode/node()"/>
    <xsl:text>'</xsl:text>
    <!-- Unit Price -->
    <xsl:text>});</xsl:text>
  </xsl:template>

  <!-- Log Order Items-->
  <xsl:template match="Content" mode="ga-universal-impression">
    <xsl:text>ga('ec:ec:addImpression', {</xsl:text>
    <!-- Stock Code -->
    <xsl:text>'id': '</xsl:text>
    <xsl:call-template name="escape-js">
      <xsl:with-param name="string">
        <xsl:choose>
          <xsl:when test="StockCode!=''">
            <xsl:value-of select="StockCode/node()"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="@id"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:text>'</xsl:text>
    <!-- Product Name -->
    <xsl:text>,'name': '</xsl:text>
    <xsl:call-template name="escape-js">
      <xsl:with-param name="string">
        <xsl:value-of select="Name/node()"/>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:text>'</xsl:text>
    <!-- Category -->
    <xsl:text>,'category': '</xsl:text>
    <xsl:call-template name="escape-js">
      <xsl:with-param name="string">
        <xsl:choose>
          <xsl:when test="Item">
            <xsl:for-each select="Item">
              <xsl:value-of select="option/@groupName"/>
              <xsl:text>: </xsl:text>
              <xsl:value-of select="option/@name"/>
              <xsl:text>, </xsl:text>
            </xsl:for-each>
          </xsl:when>
          <xsl:otherwise>
            <xsl:variable name="parId">
              <xsl:value-of select="@parId"/>
            </xsl:variable>
            <xsl:value-of select="//MenuItem[@id=$parId]/@name" />
          </xsl:otherwise>
        </xsl:choose>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:text>'</xsl:text>
    <!-- brand -->
    <xsl:text>,'brand': '</xsl:text>
    <xsl:value-of select="Manufacturer/node()"/>
    <xsl:text>'</xsl:text>
    <!-- variant -->
    <xsl:text>,'variant': '</xsl:text>
    <xsl:value-of select="StockCode/node()"/>
    <xsl:text>'</xsl:text>
    <xsl:text>,'list': '</xsl:text>
    <xsl:apply-templates select="$currentPage" mode="getDisplayName"/>
    <xsl:text>'</xsl:text>
    <xsl:text>,'position': </xsl:text>
    <xsl:value-of select="position()"/>
    <xsl:text></xsl:text>
    <xsl:text>});</xsl:text>
  </xsl:template>



  <!-- -->
  <!-- NO LONGER IN USE - NOW USE ASYNCHRONOUS CODE ABOVE -->
  <xsl:template match="Page" mode="GoogleAnalyticsNewTransaction">
    <!-- NO LONGER IN USE - NOW USE ASYNCHRONOUS CODE ABOVE -->
    <xsl:if test="Cart/Order/@cmd='ShowInvoice'">
      pageTracker._addTrans(
      "<xsl:value-of select="Cart/Order/@InvoiceRef"/>",	// Order ID
      "None",												// Affiliation
      "<xsl:value-of select="Cart/Order/@totalNet"/>",	// Total
      "<xsl:value-of select="Cart/Order/@vatAmt"/>",		// Tax
      "<xsl:value-of select="Cart/Order/@shippingCost"/>",// Shipping
      "<xsl:value-of select="Cart/Order/Contact[@type='Delivery Address']/City/node()"/>",	// City
      "<xsl:value-of select="Cart/Order/Contact[@type='Delivery Address']/State/node()"/>",	// State
      "<xsl:value-of select="Cart/Order/Contact[@type='Delivery Address']/Country/node()"/>"	// Country
      );
      <xsl:for-each select="Cart/Order/Item">
        pageTracker._addItem(
        "<xsl:value-of select="/Page/Cart/Order/@InvoiceRef"/>",	// Order ID
        "<xsl:value-of select="productDetail/StockCode/node()"/>",  // SKU
        "<xsl:value-of select="productDetail/Name/node()"/>",       // Product Name
        "<xsl:value-of select="productDetail/Options/OptGroup/option/@name"/>",		// Category
        "<xsl:value-of select="@price"/>",                          // Price
        "<xsl:value-of select="@quantity"/>"                        // Quantity
        );
      </xsl:for-each>

      pageTracker._trackTrans();

    </xsl:if>
  </xsl:template>

  <xsl:template match="Content" mode="A1WebStatsCode">
    <xsl:if test="not(/Page/@adminMode) and not(/Page/@previewMode='true')">

    </xsl:if>
  </xsl:template>

  <xsl:template match="Content" mode="A1WebStatsCodeFooter">
    <xsl:variable name="StatsID">
      <xsl:value-of select="node()"/>
    </xsl:variable>
    <xsl:if test="not(/Page/@adminMode) and not(/Page/@previewMode='true')">
      <xsl:if test="node()!=''">
        <script type="text/javascript">
          <xsl:if test="//Content[@type='CookiePolicy']">
            <xsl:text>if (jQuery.cookie('cc_cookie_accept') == "cc_cookie_accept") {</xsl:text>
          </xsl:if>
          <xsl:text>var _pt = ["</xsl:text>
          <xsl:value-of select="node()"/>
          <xsl:text>"];</xsl:text>
          <xsl:text>(function(d,t){</xsl:text>
          <xsl:text>var a=d.createElement(t),s=d.getElementsByTagName(t)[0];a.src=location.protocol+'//a1webstrategy.com/track.js';s.parentNode.insertBefore(a,s);</xsl:text>
          <xsl:text>}(document,'script'));</xsl:text>
          <!-- END A1WebStats Activation Code -->
          <xsl:if test="//Content[@type='CookiePolicy']">
            <xsl:text>}</xsl:text>
          </xsl:if>
        </script>
        <noscript>
          <a href="http://www.a1webstats.com/">
            <img width="1" height="1" src="//www.a1webstats.com/stats/stat-nojs.aspx?ac={$StatsID}" alt="web analytics" />
          </a>
        </noscript>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Content" mode="A1WebStatsCodeV2Footer">
    <xsl:variable name="StatsID">
      <xsl:value-of select="substring-before(node(),'-')"/>
    </xsl:variable>
    <xsl:variable name="PartnerName">
      <xsl:value-of select="substring-after(node(),'-')"/>
    </xsl:variable>
    <xsl:if test="not(/Page/@adminMode) and not(/Page/@previewMode='true')">
      <xsl:if test="node()!=''">
        <script type="text/javascript">
          <xsl:if test="//Content[@type='CookiePolicy']">
            <xsl:text>if (jQuery.cookie('cc_cookie_accept') == "cc_cookie_accept") {</xsl:text>
          </xsl:if>
          <xsl:text>var cid = </xsl:text>
          <xsl:value-of select="$StatsID"/>
          <xsl:text>;</xsl:text>
          <xsl:text>(function() {</xsl:text>
          <xsl:text>window.</xsl:text>
          <xsl:value-of select="$PartnerName"/>
          <xsl:text> = '</xsl:text>
          <xsl:value-of select="$PartnerName"/>
          <xsl:text>';</xsl:text>
          <xsl:text>window.</xsl:text>
          <xsl:value-of select="$PartnerName"/>
          <xsl:text> = window.</xsl:text>
          <xsl:value-of select="$PartnerName"/>
          <xsl:text> || function(){</xsl:text>
          <xsl:text>(window.</xsl:text>
          <xsl:value-of select="$PartnerName"/>
          <xsl:text>.q = window.ga.q || []).push(arguments)</xsl:text>
          <xsl:text>},</xsl:text>
          <xsl:text>window.</xsl:text>
          <xsl:value-of select="$PartnerName"/>
          <xsl:text>.l = 1 * new Date();</xsl:text>
          <xsl:text>var a = document.createElement('script');</xsl:text>
          <xsl:text>var m = document.getElementsByTagName('script')[0];</xsl:text>
          <xsl:text>a.async = 1;</xsl:text>
          <xsl:text>a.src = "https://api1.websuccess-data.com/wltracker.js";</xsl:text>
          <xsl:text>m.parentNode.insertBefore(a,m)</xsl:text>
          <xsl:text>})()</xsl:text>
          <!-- END A1WebStats Activation Code -->
          <xsl:if test="//Content[@type='CookiePolicy']">
            <xsl:text>}</xsl:text>
          </xsl:if>
        </script>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Content" mode="MetaWhoIsVisitingCode">
    <xsl:if test="not(/Page/@adminMode) and not(/Page/@previewMode='true')">
      <script type="text/javascript">
        (function (n) { var t = n.createElement("script"), i; var u = window.location.href; var p = u.split('/')[0]; t.type = "text/javascript"; t.src = p + "//dashboard.whoisvisiting.com/who.js"; i = n.getElementsByTagName("script")[0]; i.parentNode.insertBefore(t, i) })(document); var whoparam = whoparam || []; whoparam.push(["AcNo", "<xsl:value-of select="node()"/>"]); whoparam.push(["SendHit", ""] );
      </script>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Content" mode="FacebookChatCode">
    <xsl:if test="not(/Page/@adminMode)">
      <!-- Load Facebook SDK for JavaScript -->
      <div id="fb-root"></div>
      <script>
        (function(d, s, id) {
        var js, fjs = d.getElementsByTagName(s)[0];
        if (d.getElementById(id)) return;
        js = d.createElement(s); js.id = id;
        js.src = 'https://connect.facebook.net/en_US/sdk/xfbml.customerchat.js#xfbml=1&amp;version=v2.12&amp;autoLogAppEvents=1';
        fjs.parentNode.insertBefore(js, fjs);
        }(document, 'script', 'facebook-jssdk'));
      </script>
      <!-- Your customer chat code -->
      <div class="fb-customerchat"
        attribution="ProteanCMS"
        page_id="{@pageid}">
        <xsl:if test="@theme_color!=''">
          <xsl:attribute name="theme_color">
            <xsl:value-of select="@theme_color"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="@logged_in_greeting!=''">
          <xsl:attribute name="logged_in_greeting">
            <xsl:value-of select="@logged_in_greeting"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="@logged_out_greeting!=''">
          <xsl:attribute name="logged_out_greeting">
            <xsl:value-of select="@logged_out_greeting"/>
          </xsl:attribute>
        </xsl:if>
      </div>

    </xsl:if>
  </xsl:template>

  <xsl:template match="Content[@type='plaintext']" mode="JSONLD">
    <xsl:if test="node()!=''">
        <xsl:value-of select="node()"/>
    </xsl:if>
  </xsl:template>


  <xsl:template match="Content" mode="MetaLeadForensicsCode">
    <xsl:variable name="lfid">
      <xsl:value-of select="node()"/>
    </xsl:variable>
    <xsl:if test="not(/Page/@adminMode) and not(/Page/@previewMode='true')">
      <script type="text/javascript" src="https://secure.leadforensics.com/js/{$lfid}.js" async="async" >/* */</script>
      <noscript>
        <img src="https://secure.leadforensics.com/{$lfid}.png" style="display:none;" />
      </noscript>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Page" mode="A1WebStats_trackPageview">
    <xsl:text>ptInit(ptAccount);</xsl:text>
  </xsl:template>

  <!-- -->
  <xsl:template match="Page[Cart/Order and Cart/Order/@cmd!='']" mode="A1WebStats_trackPageview">
    <xsl:text>ptTrackLink('</xsl:text>
    <xsl:value-of select="$siteURL"/>
    <xsl:text>/ShoppingCart/</xsl:text>
    <xsl:value-of select="Cart/Order/@cmd"/>
    <xsl:text>','</xsl:text>
    <xsl:text>ShoppingCart - </xsl:text>
    <xsl:value-of select="Cart/Order/@cmd"/>
    <xsl:text>');</xsl:text>
  </xsl:template>

  <!-- -->
  <xsl:template match="Page[//Content[descendant-or-self::alert/node()='Message Sent']]" mode="A1WebStats_trackPageview">
    <xsl:text>ptTrackLink('</xsl:text>
    <xsl:value-of select="$href"/>
    <xsl:if test="$currentPage/@id!=//Menu/MenuItem/@id">
      <xsl:text>/</xsl:text>
    </xsl:if>
    <xsl:value-of select="//Content[descendant-or-self::alert/node()='Message Sent']/descendant-or-self::submission/@id"/>
    <xsl:text>/Message-Sent</xsl:text>
    <xsl:text>','</xsl:text>
    <xsl:apply-templates select="." mode="PageTitle"/>
    <xsl:text> - Message Sent</xsl:text>
    <xsl:text>');</xsl:text>
  </xsl:template>

  <!-- -->
  <xsl:template match="Page[Request/Form/Item[@name='searchString']]" mode="A1WebStats_trackPageview">
    <xsl:variable name="searchResults">
      <xsl:apply-templates select="Contents/Content[@type='Module' and @moduleType='Search'][position()=1]" mode="getSearchResults"/>
    </xsl:variable>
    <xsl:text>ptTrackLink('</xsl:text>
    <xsl:value-of select="$href"/>
    <xsl:text>/Query-</xsl:text>
    <xsl:value-of select="Request/Form/Item[@name='searchString']/node()"/>/Results-<xsl:value-of select="count(ms:node-set($searchResults)/*)" />
    <xsl:text>','</xsl:text>
    <xsl:apply-templates select="." mode="PageTitle"/>
    <xsl:text> - </xsl:text>
    <xsl:value-of select="Request/Form/Item[@name='searchString']/node()"/>
    <xsl:text>');</xsl:text>
  </xsl:template>


  <!--   ################################################   Last Updated   ##################################################   -->
  <xsl:template match="/" mode="lastUpdated">
    <xsl:call-template name="Mon_DD_YYYY">
      <xsl:with-param name="date" select="/Page/@updateDate"/>
    </xsl:call-template>
  </xsl:template>

  <!--   ################################################   breadcrumb / PageLocation   ##################################################   -->

  <xsl:template match="MenuItem" mode="breadcrumb">
    <xsl:param name="separator">&gt;</xsl:param>

    <!-- if not excluded from Nav -->
    <xsl:if test="not(DisplayName/@exclude='true')">

      <!-- Link anchor to page -->
      <xsl:apply-templates select="." mode="menuLink"/>

      <!-- IF NOT Current Page; i.e. The last item not followed by &gt; -->
      <xsl:if test="not(@id=/Page/@id)">
        <xsl:value-of select="concat(' ', $separator, ' ')"/>
      </xsl:if>

    </xsl:if>

    <!-- Self calling template to get the next MenuItem -->
    <xsl:apply-templates select="MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="breadcrumb">
      <xsl:with-param name="separator" select="$separator"/>
    </xsl:apply-templates>

    <!-- IF currentPage && In Content Detail -->
    <xsl:if test="@id=/Page/@id and //ContentDetail">
      <xsl:if test="not(DisplayName/@exclude='true')">
        <xsl:value-of select="concat(' ', $separator, ' ')"/>
      </xsl:if>
      <xsl:apply-templates select="//ContentDetail/Content" mode="contentLink"/>
    </xsl:if>

  </xsl:template>

  <xsl:template match="MenuItem" mode="breadcrumbAdmin">
    <xsl:param name="separator">&gt;</xsl:param>

    <!-- if not excluded from Nav -->
    <xsl:if test="not(DisplayName/@exclude='true')">

      <!-- Link anchor to page -->
      <li>
        <xsl:apply-templates select="." mode="menuLink"/>
      </li>

      <!-- IF NOT Current Page; i.e. The last item not followed by &gt; -->
      <!--<xsl:if test="not(@id=/Page/@id)">
        <xsl:value-of select="concat(' ', $separator, ' ')"/>
      </xsl:if>-->

    </xsl:if>

    <!-- Self calling template to get the next MenuItem -->
    <xsl:apply-templates select="MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="breadcrumb">
      <xsl:with-param name="separator" select="$separator"/>
    </xsl:apply-templates>

    <!-- IF currentPage && In Content Detail -->
    <xsl:if test="@id=/Page/@id and //ContentDetail">
      <xsl:if test="not(DisplayName/@exclude='true')">
        <xsl:value-of select="concat(' ', $separator, ' ')"/>
      </xsl:if>
      <xsl:apply-templates select="//ContentDetail/Content" mode="contentLink"/>
    </xsl:if>

  </xsl:template>

  <xsl:template match="MenuItem[ancestor::Page[@cssFramework='bs3']]" mode="breadcrumb">
    <xsl:param name="separator"></xsl:param>

    <!-- if not excluded from Nav -->
    <xsl:if test="not(DisplayName/@exclude='true') and @name!='Information'">
      <xsl:apply-templates select="." mode="breadcrumbLink"/>
    </xsl:if>

    <!-- Self calling template to get the next MenuItem -->
    <xsl:apply-templates select="MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="breadcrumb">
      <xsl:with-param name="separator" select="$separator"/>
    </xsl:apply-templates>

    <!-- IF currentPage && In Content Detail -->
    <xsl:if test="@id=/Page/@id and //ContentDetail">
      <xsl:if test="not(DisplayName/@exclude='true')">
        <xsl:value-of select="concat(' ', $separator, ' ')"/>
      </xsl:if>
      <li class="active">
        <xsl:apply-templates select="//ContentDetail/Content" mode="contentLink"/>
      </li>
    </xsl:if>

  </xsl:template>

  <xsl:template match="MenuItem[ancestor::Page[@cssFramework='bs3']]" mode="breadcrumbLink">
    <xsl:param name="span" select="false()"/>
    <li itemprop="itemListElement" itemscope="" itemtype="http://schema.org/ListItem">
      <xsl:choose>
        <xsl:when test="self::MenuItem[@id=/Page/@id] and not(//ContentDetail)">
          <xsl:attribute name="class">active</xsl:attribute>

          <!-- title attribute -->
          <xsl:attribute name="title">
            <xsl:apply-templates select="." mode="getTitleAttr"/>
          </xsl:attribute>

          <!-- rel attribute -->
          <xsl:choose>
            <!-- Test for Home Page to insert microformat homepage attribute-->
            <xsl:when test="@id=$page/Menu/MenuItem/@id">
              <xsl:attribute name="rel">home</xsl:attribute>
            </xsl:when>
            <!-- stick external rel on external links [@target is a depreciated attribute in xHTML] Most modern browsers open rel="external" in new tab or window without JS intervention -->
            <!-- if starts with / is a relative url but put external if contains http and set to external -->
            <xsl:when test="not(substring(@url,1,1)='/') and (contains(@url,'http://') and DisplayName/@linkType='external')">
              <xsl:attribute name="rel">external</xsl:attribute>
            </xsl:when>
            <!-- When page set to no index add no follow to its link -->
            <xsl:when test="DisplayName/@noindex='true'">
              <xsl:attribute name="rel">nofollow</xsl:attribute>
            </xsl:when>
          </xsl:choose>
          <a itemprop="item">
	     <xsl:attribute name="href">
              <xsl:apply-templates select="self::MenuItem" mode="getHref"/>
            </xsl:attribute>
          <!-- output page name -->
          <span itemprop="name">
            <xsl:apply-templates select="." mode="getDisplayName"/>
          </span>
          <meta itemprop="position" content="{count(parent::MenuItem)+1}" />
          </a>
        </xsl:when>
        <xsl:otherwise>
          <a itemprop="item">

            <!-- get the href -->
            <xsl:attribute name="href">
              <xsl:apply-templates select="self::MenuItem" mode="getHref"/>
            </xsl:attribute>

            <!-- title attribute -->
            <xsl:attribute name="title">
              <xsl:apply-templates select="." mode="getTitleAttr"/>
            </xsl:attribute>

            <!-- check for different states to be applied -->
            <xsl:choose>
              <xsl:when test="self::MenuItem[@id=/Page/@id]">
                <xsl:attribute name="class">active</xsl:attribute>
              </xsl:when>
              <xsl:when test="descendant::MenuItem[@id=/Page/@id] and ancestor::MenuItem">
                <xsl:attribute name="class">on</xsl:attribute>
              </xsl:when>
            </xsl:choose>

            <!-- rel attribute -->
            <xsl:choose>
              <!-- Test for Home Page to insert microformat homepage attribute-->
              <xsl:when test="@id=$page/Menu/MenuItem/@id">
                <xsl:attribute name="rel">home</xsl:attribute>
              </xsl:when>
              <!-- stick external rel on external links [@target is a depreciated attribute in xHTML] Most modern browsers open rel="external" in new tab or window without JS intervention -->
              <!-- if starts with / is a relative url but put external if contains http and set to external -->
              <xsl:when test="not(substring(@url,1,1)='/') and (contains(@url,'http://') and DisplayName/@linkType='external')">
                <xsl:attribute name="rel">external</xsl:attribute>
              </xsl:when>
              <!-- When page set to no index add no follow to its link -->
              <xsl:when test="DisplayName/@noindex='true'">
                <xsl:attribute name="rel">nofollow</xsl:attribute>
              </xsl:when>
            </xsl:choose>
            <!-- output page name -->
            <span itemprop="name">
              <xsl:apply-templates select="." mode="getDisplayName"/>
            </span>
            <meta itemprop="position" content="{count(parent::MenuItem)+1}" />
          </a>

        </xsl:otherwise>
      </xsl:choose>

    </li>
  </xsl:template>

  <xsl:template match="img" mode="jsNiceImage">
    <xsl:element name="img">
      <xsl:attribute name="src">
        <xsl:value-of select="@src"/>
      </xsl:attribute>
      <xsl:attribute name="width">
        <xsl:value-of select="@width"/>
      </xsl:attribute>
      <xsl:attribute name="height">
        <xsl:value-of select="@height"/>
      </xsl:attribute>
      <xsl:attribute name="class">
        <xsl:value-of select="@class"/>
      </xsl:attribute>
      <xsl:attribute name="alt">
        <xsl:call-template name="escape-js">
          <xsl:with-param name="string">
            <xsl:value-of select="@alt"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:attribute>
    </xsl:element>
  </xsl:template>


  <!--   ################################################   Developer Link  ##############################################   -->
  <xsl:template match="/" mode="developerLink">
    <xsl:call-template name="developerLink"/>
  </xsl:template>

  <xsl:template name="developerLink">
    <xsl:variable name="websitecreditURL">
      <xsl:choose>
        <xsl:when test="$page/Settings/add[@key='web.websitecreditURL']/@value!=''">
          <xsl:choose>
            <xsl:when test="starts-with($page/Settings/add[@key='web.websitecreditURL']/@value, 'http')">
              <xsl:value-of select="$page/Settings/add[@key='web.websitecreditURL']/@value"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:text>http://</xsl:text>
              <xsl:value-of select="$page/Settings/add[@key='web.websitecreditURL']/@value"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>https://eonic.com</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="websitecreditText">
      <xsl:choose>
        <xsl:when test="$page/Settings/add[@key='web.websitecreditText']/@value!=''">
          <xsl:value-of select="$page/Settings/add[@key='web.websitecreditText']/@value"/>
        </xsl:when>
        <xsl:otherwise>
            <xsl:text>Site by Eonic</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="websitecreditLogo">
      <xsl:choose>
        <xsl:when test="$page/Settings/add[@key='web.websitecreditLogo']/@value!=''">
          <xsl:value-of select="$page/Settings/add[@key='web.websitecreditLogo']/@value"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>/ewcommon/images/Eonic-Web-Logo-t-white.png</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div id="developerLink">
      <xsl:if test="$page/Settings/add[@key='web.websitecreditURL']/@value!='' or $page/@id = $page/Menu/MenuItem/@id">
        <a href="{$websitecreditURL}" title="{$websitecreditText}" rel="external">
          <xsl:if test="$page/Settings/add[@key='web.websitecreditLogo']/@value=''">
            <xsl:attribute name="class">devText</xsl:attribute>
          </xsl:if>
          <xsl:value-of select="$websitecreditText"/>
        </a>
        <xsl:if test="$websitecreditLogo!=''">
          <a href="{$websitecreditURL}" title="{$websitecreditText}" rel="nofollow external">
            <xsl:if test="$page/Settings/add[@key='web.websitecreditLogo']/@value=''">
              <xsl:attribute name="class">devLogo</xsl:attribute>
            </xsl:if>
            <img src="{$websitecreditLogo}" alt="{$websitecreditText}"/>
          </a>
        </xsl:if>
      </xsl:if>
      <div class="terminus">&#160;</div>
    </div>
  </xsl:template>


  <!--   ##########################################################################################   -->
  <!--   ##  Date formatting  #####################################################################   -->
  <!--   ##########################################################################################   -->

  <xsl:template name="HH_MM">
    <xsl:param name="date"/>
    &#160;<xsl:value-of select="substring($date, 12, 2)"/>:<xsl:value-of select="substring($date, 15, 2)"/>
  </xsl:template>
  <xsl:template name="DD_MM_YY">
    <xsl:param name="date"/>
    <xsl:param name="showTime"/>
    <!-- Day -->
    <xsl:value-of select="substring($date, 9, 2)"/>
    <xsl:text>/</xsl:text>
    <!-- Month -->
    <xsl:value-of select="substring($date,6, 2)"/>
    <xsl:text>/</xsl:text>
    <!-- Year -->
    <xsl:value-of select="substring($date, 3, 2)"/>
    <!-- Time -->
    <xsl:if test="$showTime='true'">
      <xsl:call-template name="HH_MM">
        <xsl:with-param name="date" select="$date"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
  <!-- -->
  <xsl:template name="DD_MM_YYYY">
    <xsl:param name="date"/>
    <xsl:param name="showTime"/>
    <!-- Day -->
    <xsl:value-of select="substring($date, 9, 2)"/>
    <xsl:text>/</xsl:text>
    <!-- Month -->
    <xsl:value-of select="substring($date,6, 2)"/>
    <xsl:text>/</xsl:text>
    <!-- Year -->
    <xsl:value-of select="substring($date, 1, 4)"/>
    <!-- Time -->
    <xsl:if test="$showTime='true'">
      <xsl:call-template name="HH_MM">
        <xsl:with-param name="date" select="$date"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
  <!-- -->
  <xsl:template name="MM_DD">
    <xsl:param name="date"/>
    <!-- Month -->
    <xsl:value-of select="substring($date, 6, 2)"/>
    <xsl:text>/</xsl:text>
    <!-- Day -->
    <xsl:value-of select="substring($date, 9, 2)"/>
    <xsl:text>/</xsl:text>
  </xsl:template>
  <!-- -->
  <xsl:template name="DD">
    <xsl:param name="date"/>
    <!-- Day -->
    <xsl:value-of select="substring($date, 9, 2)"/>
  </xsl:template>
  <!-- -->
  <xsl:template name="Mon_DD_YYYY">
    <xsl:param name="date"/>
    <xsl:param name="showTime"/>
    <xsl:variable name="month" select="number(substring($date, 6, 2))"/>
    <xsl:value-of select="number(substring($date, 9, 2))"/>&#160;<xsl:choose>
      <xsl:when test="$month=1">January</xsl:when>
      <xsl:when test="$month=2">February</xsl:when>
      <xsl:when test="$month=3">March</xsl:when>
      <xsl:when test="$month=4">April</xsl:when>
      <xsl:when test="$month=5">May</xsl:when>
      <xsl:when test="$month=6">June</xsl:when>
      <xsl:when test="$month=7">July</xsl:when>
      <xsl:when test="$month=8">August</xsl:when>
      <xsl:when test="$month=9">September</xsl:when>
      <xsl:when test="$month=10">October</xsl:when>
      <xsl:when test="$month=11">November</xsl:when>
      <xsl:when test="$month=12">December</xsl:when>
      <xsl:otherwise>
        [<xsl:value-of select="$date"/>]
      </xsl:otherwise>
    </xsl:choose>&#160;<xsl:value-of select="substring($date, 1, 4)"/>
    <!-- Time -->
    <xsl:if test="$showTime='true'">
      <xsl:call-template name="HH_MM">
        <xsl:with-param name="date" select="$date"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
  <!-- -->
  <xsl:template name="getMonth">
    <xsl:param name="month"/>
    <xsl:choose>
      <xsl:when test="$month=1">January</xsl:when>
      <xsl:when test="$month=2">February</xsl:when>
      <xsl:when test="$month=3">March</xsl:when>
      <xsl:when test="$month=4">April</xsl:when>
      <xsl:when test="$month=5">May</xsl:when>
      <xsl:when test="$month=6">June</xsl:when>
      <xsl:when test="$month=7">July</xsl:when>
      <xsl:when test="$month=8">August</xsl:when>
      <xsl:when test="$month=9">September</xsl:when>
      <xsl:when test="$month=10">October</xsl:when>
      <xsl:when test="$month=11">November</xsl:when>
      <xsl:when test="$month=12">December</xsl:when>
      <xsl:otherwise>INVALID MONTH</xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- -->
  <!-- -->
  <xsl:template name="getShortMonth">
    <xsl:param name="month"/>
    <xsl:choose>
      <xsl:when test="$month=1">Jan</xsl:when>
      <xsl:when test="$month=2">Feb</xsl:when>
      <xsl:when test="$month=3">Mar</xsl:when>
      <xsl:when test="$month=4">Apr</xsl:when>
      <xsl:when test="$month=5">May</xsl:when>
      <xsl:when test="$month=6">Jun</xsl:when>
      <xsl:when test="$month=7">Jul</xsl:when>
      <xsl:when test="$month=8">Aug</xsl:when>
      <xsl:when test="$month=9">Sep</xsl:when>
      <xsl:when test="$month=10">Oct</xsl:when>
      <xsl:when test="$month=11">Nov</xsl:when>
      <xsl:when test="$month=12">Dec</xsl:when>
      <xsl:otherwise>INVALID MONTH</xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- -->

  <!-- DISPLAYDATE
        USE THIS TEMPLATE TO OVERWRITE DATES ACROSS ALL PAGE LAYOUTS 
        - DEFAULTLY CALL DD_Mon_YYYY
  -->
  <xsl:template name="DisplayDate">
    <xsl:param name="date"/>
    <xsl:param name="showTime"/>
    <xsl:call-template name="DD_Mon_YYYY">
      <xsl:with-param name="date" select="$date"/>
      <xsl:with-param name="showTime" select="$showTime"/>
    </xsl:call-template>
  </xsl:template>
  <!-- -->
  <xsl:template name="DD_Mon_YYYY">
    <xsl:param name="date"/>
    <xsl:param name="showTime"/>
    <xsl:variable name="month" select="number(substring($date, 6, 2))"/>
    <xsl:value-of select="substring($date, 9, 2)"/>
    <xsl:text> </xsl:text>
    <xsl:choose>
      <xsl:when test="$month=1">Jan</xsl:when>
      <xsl:when test="$month=2">Feb</xsl:when>
      <xsl:when test="$month=3">Mar</xsl:when>
      <xsl:when test="$month=4">Apr</xsl:when>
      <xsl:when test="$month=5">May</xsl:when>
      <xsl:when test="$month=6">Jun</xsl:when>
      <xsl:when test="$month=7">Jul</xsl:when>
      <xsl:when test="$month=8">Aug</xsl:when>
      <xsl:when test="$month=9">Sep</xsl:when>
      <xsl:when test="$month=10">Oct</xsl:when>
      <xsl:when test="$month=11">Nov</xsl:when>
      <xsl:when test="$month=12">Dec</xsl:when>
      <xsl:otherwise>INVALID MONTH</xsl:otherwise>
    </xsl:choose>
    <xsl:text> </xsl:text>
    <xsl:value-of select="substring($date, 1, 4)"/>
    <!-- Time -->
    <xsl:if test="$showTime='true'">
      <xsl:call-template name="HH_MM">
        <xsl:with-param name="date" select="$date"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
  <!-- -->
  <xsl:template name="DD_Mon_YY">
    <xsl:param name="date"/>
    <xsl:param name="showTime"/>
    <xsl:variable name="month" select="number(substring($date, 6, 2))"/>
    <xsl:value-of select="substring($date, 9, 2)"/>&#160;<xsl:choose>
      <xsl:when test="$month=1">Jan</xsl:when>
      <xsl:when test="$month=2">Feb</xsl:when>
      <xsl:when test="$month=3">Mar</xsl:when>
      <xsl:when test="$month=4">Apr</xsl:when>
      <xsl:when test="$month=5">May</xsl:when>
      <xsl:when test="$month=6">Jun</xsl:when>
      <xsl:when test="$month=7">Jul</xsl:when>
      <xsl:when test="$month=8">Aug</xsl:when>
      <xsl:when test="$month=9">Sep</xsl:when>
      <xsl:when test="$month=10">Oct</xsl:when>
      <xsl:when test="$month=11">Nov</xsl:when>
      <xsl:when test="$month=12">Dec</xsl:when>
      <xsl:otherwise>INVALID MONTH</xsl:otherwise>
    </xsl:choose>&#160;<xsl:value-of select="substring($date, 3, 2)"/>
    <!-- Time -->
    <xsl:if test="$showTime='true'">
      <xsl:call-template name="HH_MM">
        <xsl:with-param name="date" select="$date"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
  <!-- -->
  <!-- -->
  <xsl:template name="Mon_YYYY">
    <xsl:param name="date"/>
    <xsl:param name="showTime"/>
    <xsl:variable name="month" select="number(substring($date, 6, 2))"/>
    <!--<xsl:value-of select="number(substring($date, 9, 2))"/>
    <xsl:text> </xsl:text>-->
    <xsl:choose>
      <xsl:when test="$month=1">Jan</xsl:when>
      <xsl:when test="$month=2">Feb</xsl:when>
      <xsl:when test="$month=3">Mar</xsl:when>
      <xsl:when test="$month=4">Apr</xsl:when>
      <xsl:when test="$month=5">May</xsl:when>
      <xsl:when test="$month=6">Jun</xsl:when>
      <xsl:when test="$month=7">Jul</xsl:when>
      <xsl:when test="$month=8">Aug</xsl:when>
      <xsl:when test="$month=9">Sep</xsl:when>
      <xsl:when test="$month=10">Oct</xsl:when>
      <xsl:when test="$month=11">Nov</xsl:when>
      <xsl:when test="$month=12">Dec</xsl:when>
      <xsl:otherwise>INVALID MONTH</xsl:otherwise>
    </xsl:choose>
    <xsl:text> </xsl:text>
    <xsl:value-of select="substring($date, 1, 4)"/>
    <!-- Time -->
    <xsl:if test="$showTime='true'">
      <xsl:call-template name="HH_MM">
        <xsl:with-param name="date" select="$date"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
  <!-- -->
  <!-- -->
  <xsl:template name="Month_YYYY">
    <xsl:param name="date"/>
    <xsl:param name="showTime"/>
    <xsl:variable name="month">
      <xsl:choose>
        <xsl:when test="substring($date, 6, 2)!=''">
          <xsl:value-of select="number(substring($date, 6, 2))"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <!--<xsl:value-of select="number(substring($date, 9, 2))"/>
    <xsl:text> </xsl:text>-->
    <xsl:choose>
      <xsl:when test="$month=1">January</xsl:when>
      <xsl:when test="$month=2">February</xsl:when>
      <xsl:when test="$month=3">March</xsl:when>
      <xsl:when test="$month=4">April</xsl:when>
      <xsl:when test="$month=5">May</xsl:when>
      <xsl:when test="$month=6">June</xsl:when>
      <xsl:when test="$month=7">July</xsl:when>
      <xsl:when test="$month=8">August</xsl:when>
      <xsl:when test="$month=9">September</xsl:when>
      <xsl:when test="$month=10">October</xsl:when>
      <xsl:when test="$month=11">November</xsl:when>
      <xsl:when test="$month=12">December</xsl:when>
      <xsl:when test="$month='NaN' and $month!=''">INVALID MONTH</xsl:when>
      <!--<xsl:otherwise>INVALID MONTH</xsl:otherwise>-->
    </xsl:choose>
    <xsl:text> </xsl:text>
    <xsl:value-of select="substring($date, 1, 4)"/>
    <!-- Time -->
    <xsl:if test="$showTime='true'">
      <xsl:call-template name="HH_MM">
        <xsl:with-param name="date" select="$date"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
  <!-- -->
  <xsl:template name="NiceDateToXSDDate">
    <xsl:param name="date"/>
    <xsl:variable name="year" select="number(substring(substring-after($date,' '), 5, 4))"/>
    <xsl:variable name="month" select="substring(substring-after($date,' '), 1, 3)"/>
    <xsl:variable name="day" select="substring($date, 1, 2)"/>
    <xsl:value-of select="$year"/>-<xsl:choose>
      <xsl:when test="$month='Jan'">01</xsl:when>
      <xsl:when test="$month='Feb'">02</xsl:when>
      <xsl:when test="$month='Mar'">03</xsl:when>
      <xsl:when test="$month='Apr'">04</xsl:when>
      <xsl:when test="$month='May'">05</xsl:when>
      <xsl:when test="$month='Jun'">06</xsl:when>
      <xsl:when test="$month='Jul'">07</xsl:when>
      <xsl:when test="$month='Aug'">08</xsl:when>
      <xsl:when test="$month='Sep'">09</xsl:when>
      <xsl:when test="$month='Oct'">10</xsl:when>
      <xsl:when test="$month='Nov'">11</xsl:when>
      <xsl:when test="$month='Dec'">12</xsl:when>
      <xsl:otherwise>
        INVALID MONTH (<xsl:value-of select="$month"/>)
      </xsl:otherwise>
    </xsl:choose>-<xsl:value-of select="format-number(number($day),'00')"/>
  </xsl:template>

  <xsl:template name="CalendarIcon">
    <xsl:param name="date"/>
    <xsl:param name="showTime"/>
    <xsl:variable name="month" select="number(substring($date, 6, 2))"/>

    <time datetime="2014-09-20" class="icon">
      <em>
        <xsl:call-template name="formatdate">
          <xsl:with-param name="date" select="$date" />
          <xsl:with-param name="format" select="'dddd'" />
        </xsl:call-template>
      </em>
      <strong>
        <xsl:choose>
          <xsl:when test="$month=1">January</xsl:when>
          <xsl:when test="$month=2">February</xsl:when>
          <xsl:when test="$month=3">March</xsl:when>
          <xsl:when test="$month=4">April</xsl:when>
          <xsl:when test="$month=5">May</xsl:when>
          <xsl:when test="$month=6">June</xsl:when>
          <xsl:when test="$month=7">July</xsl:when>
          <xsl:when test="$month=8">August</xsl:when>
          <xsl:when test="$month=9">September</xsl:when>
          <xsl:when test="$month=10">October</xsl:when>
          <xsl:when test="$month=11">November</xsl:when>
          <xsl:when test="$month=12">December</xsl:when>
          <xsl:otherwise>
            [<xsl:value-of select="$date"/>]
          </xsl:otherwise>
        </xsl:choose>
      </strong>
      <span>
        <xsl:value-of select="number(substring($date, 9, 2))"/>
      </span>
    </time>
  </xsl:template>



  <!-- receives seconds, returns time duration as per ISO 8601 format 
         - see http://en.wikipedia.org/wiki/ISO_8601#Durations 
         - used in microformats-->
  <xsl:template name="getISO-8601-Duration">
    <xsl:param name="secs" />

    <!-- Period designator -->
    <xsl:text>P</xsl:text>

    <!-- not splitting up larger than days for now, as can't figure out how many days in a month or leap years.
          as 36 days for example is a valid duration anyway. -->

    <!-- days -->
    <xsl:if test="$secs &gt; 86399">
      <xsl:value-of select="floor($secs div 86400)"/>
      <xsl:text>D</xsl:text>
    </xsl:if>

    <!-- Times -->
    <xsl:text>T</xsl:text>
    <!-- hours -->
    <xsl:if test="$secs &gt; 3599">
      <xsl:value-of select="floor(($secs mod 86400) div 3600)"/>
      <xsl:text>H</xsl:text>
    </xsl:if>
    <!-- minutes -->
    <xsl:if test="$secs &gt; 60">
      <xsl:value-of select="floor(($secs mod 3600) div 60)"/>
      <xsl:text>M</xsl:text>
    </xsl:if>
    <!-- seconds -->
    <xsl:if test="$secs mod 60 &gt; 0">
      <xsl:value-of select="$secs mod 60"/>
      <xsl:text>S</xsl:text>
    </xsl:if>
  </xsl:template>



  <!-- -->
  <xsl:template name="getNiceTimeFromSeconds">
    <xsl:param name="secs"/>
    <xsl:param name="format"/>
    <xsl:if test="$secs &gt; 86399">
      <span id="time_d">
        <xsl:value-of select="floor($secs div 86400)"/>
      </span>
      <xsl:choose>
        <xsl:when test="$format='short'">d&#160;</xsl:when>
        <xsl:otherwise>
          &#160;day<xsl:if test="floor($secs div 86400)!=1">s</xsl:if>&#160;
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
    <xsl:if test="$secs &gt; 3599">
      <span id="time_h">
        <xsl:value-of select="floor(($secs mod 86400) div 3600)"/>
      </span>
      <xsl:choose>
        <xsl:when test="$format='short'">h&#160;</xsl:when>
        <xsl:otherwise>
          &#160;hour<xsl:if test="floor(($secs mod 86400) div 3600)!=1">s</xsl:if>&#160;
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
    <xsl:if test="$secs &gt; 60">
      <span id="time_m">
        <xsl:value-of select="floor(($secs mod 3600) div 60)"/>
      </span>
      <xsl:choose>
        <xsl:when test="$format='short'">m&#160;</xsl:when>
        <xsl:otherwise>
          &#160;min<xsl:if test="floor(($secs mod 3600) div 60)!=1">s</xsl:if>&#160;
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
    <span id="time_s">
      <xsl:value-of select="$secs mod 60"/>
    </span>
    <xsl:choose>
      <xsl:when test="$format='short'">s</xsl:when>
      <xsl:otherwise>
        &#160;sec<xsl:if test="$secs mod 60!=1">s</xsl:if>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- -->

  <xsl:template name="getNiceTimeFromMinutes">
    <xsl:param name="mins"/>
    <xsl:param name="format"/>

    <!-- IF more than a day -->
    <xsl:if test="$mins &gt; 1439">
      <span id="time_d">
        <xsl:value-of select="floor($mins div 1440)"/>
      </span>
      <xsl:choose>
        <xsl:when test="$format='short'">
          <xsl:text>d&#160;</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>&#160;day</xsl:text>
          <xsl:if test="floor($mins div 1440)!=1">
            <xsl:text>s</xsl:text>
          </xsl:if>
          <xsl:text>&#160;</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>

    <!-- when more than an hour -->
    <xsl:if test="$mins &gt; 59">
      <span id="time_h">
        <xsl:value-of select="floor(($mins mod 1440) div 60)"/>
      </span>
      <xsl:choose>
        <xsl:when test="$format='short'">
          <xsl:text>h&#160;</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>&#160;hour</xsl:text>
          <xsl:if test="floor(($mins mod 1440) div 60)!=1">
            <xsl:text>s</xsl:text>
          </xsl:if>
          <xsl:text>&#160;</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>

    <!--<xsl:if test="$secs &gt; 60">
      <span id="time_m">
        <xsl:value-of select="floor(($secs mod 3600) div 60)"/>
      </span>
      <xsl:choose>
        <xsl:when test="$format='short'">m&#160;</xsl:when>
        <xsl:otherwise>
          &#160;min<xsl:if test="floor(($secs mod 3600) div 60)!=1">s</xsl:if>&#160;
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>-->

    <xsl:if test="$mins mod 60 !='0'">
      <span id="time_m">
        <xsl:value-of select="$mins mod 60"/>
      </span>
      <xsl:choose>
        <xsl:when test="$format='short'">
          <xsl:text>m</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>&#160;min</xsl:text>
          <xsl:if test="$mins mod 60!=1">s</xsl:if>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>

  <!-- http://www.w3.org/Protocols/rfc822/#z28 -->
  <xsl:template name="date-RFC822">
    <xsl:param name="date"/>

    <xsl:call-template name="formatdate">
      <xsl:with-param name="date" select="$date" />
      <xsl:with-param name="format" select="'ddd, dd MMM yyyy HH:mm:ss'" />
    </xsl:call-template>
    <xsl:text> GMT</xsl:text>

  </xsl:template>


  <!--   ################################################   Menu & Content display name  ##############################################   -->
  <!-- Display Name for a Page -->
  <xsl:template match="MenuItem" mode="getDisplayName">
    <xsl:choose>
      <xsl:when test="DisplayName/node()='_'">
      </xsl:when>
      <xsl:when test="normalize-space(DisplayName/node())!=''">
        <xsl:value-of select="normalize-space(DisplayName/node())"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="@name"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Display Name for a piece of content -->
  <xsl:template match="Content" mode="getDisplayName">
    <xsl:choose>
      <xsl:when test="Name/node()">
        <xsl:value-of select="Name/node()"/>
      </xsl:when>
      <xsl:when test="Headline/node()">
        <xsl:value-of select="Headline/node()"/>
      </xsl:when>
      <xsl:when test="Title/node()">
        <xsl:value-of select="Title/node()"/>
      </xsl:when>
      <xsl:when test="DisplayName/node()">
        <xsl:value-of select="DisplayName/node()"/>
      </xsl:when>
      <xsl:when test="JobTitle/node()">
        <xsl:value-of select="JobTitle/node()"/>
      </xsl:when>
      <xsl:when test="SourceName/node()">
        <xsl:value-of select="SourceName/node()"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="@name"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- IF CONTACT, CONCAT First Name with Surname with Metadata -->
  <xsl:template match="Content[@type='Contact']" mode="getDisplayName">
    <xsl:if test="GivenName/node()!=''">
      <xsl:value-of select="GivenName/node()"/>
      <xsl:text> </xsl:text>
    </xsl:if>
    <xsl:value-of select="Surname/node()"/>
  </xsl:template>

  <!-- Display Name for User Directory Items -->
  <xsl:template match="User" mode="getDisplayName">
    <xsl:choose>
      <xsl:when test="FirstName/node()!='' or LastName!='' or MiddleName/node()!=''">
        <xsl:if test="FirstName/node()!=''">
          <xsl:value-of select="FirstName/node()"/>
          <xsl:text>&#160;</xsl:text>
        </xsl:if>
        <xsl:if test="MiddleName/node()!=''">
          <xsl:value-of select="MiddleName/node()"/>
          <xsl:text>&#160;</xsl:text>
        </xsl:if>
        <xsl:if test="LastName/node()!=''">
          <xsl:value-of select="LastName/node()"/>
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="@name"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Display Name for other Directory Items -->
  <xsl:template match="Group | Company" mode="getDisplayName">
    <xsl:value-of select="@name"/>
  </xsl:template>


  <!-- Main Title - Acts as the prominant h1 for the page -->
  <xsl:template match="/" mode="getMainTitle">
    <xsl:variable name="titleText">
      <xsl:choose>
        <xsl:when test="/Page/ContentDetail and /Page[@cssFramework!='bs3']">
          <xsl:apply-templates select="/Page/ContentDetail/Content" mode="getDisplayName" />
        </xsl:when>
        <xsl:when test="/Page/ContentDetail and /Page[@cssFramework='bs3']">
          <xsl:text> </xsl:text>
        </xsl:when>
        <xsl:when test="/Page/Contents/Content[@name='title']">
          <xsl:copy-of select="/Page/Contents/Content[@name='title']/node()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]" mode="getDisplayName" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test="not(/Page/ContentDetail)">
      <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
        <xsl:with-param name="type">PlainText</xsl:with-param>
        <xsl:with-param name="text">Add alternative title</xsl:with-param>
        <xsl:with-param name="name">title</xsl:with-param>
      </xsl:apply-templates>
    </xsl:if>
    <xsl:if test="$titleText!=''">
      <h1>
        <xsl:copy-of select="$titleText"/>
      </h1>
    </xsl:if>
  </xsl:template>


  <!--  ##  GET HREF TEMPLATES  ##################################################################  -->
  <!--  ##  Used to build the URLs for pages, and content -->

  <!-- Match on Menu Item - Build URL for that MenuItem -->
  <xsl:template match="MenuItem" mode="getHref">
    <!-- absolute url false by default -->
    <xsl:param name="absoluteURL" select="false()" />

    <xsl:variable name="siteURL">
      <xsl:call-template name="getSiteURL"/>
    </xsl:variable>
    
    <xsl:variable name="url" select="@url"/>

    <xsl:choose>
      <xsl:when test="@url!=''">
        <xsl:choose>
          <xsl:when test="format-number(@url,'0')!='NaN'">
            <xsl:value-of select="$page/Menu/descendant-or-self::MenuItem[@id=$url]/@url"/>
          </xsl:when>
          <xsl:when test="contains(@url,'http')">
            <xsl:value-of select="@url"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$siteURL"/>
            <xsl:value-of select="@url"/>
            <xsl:value-of select="/Page/@pageExt"/>
            <xsl:if test="/Page/@adminMode and /Page/@pageExt!='' and /Page/@ewCmd!='ByType'">
              <xsl:text>?pgid=</xsl:text>
              <xsl:value-of select="@id"/>
            </xsl:if>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>/</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Match on Content - Build URL for that CONTENT -->
  <xsl:template match="Content" mode="getHref">
    <xsl:param name="parId"/>
    <xsl:param name="itemMode"/>
    <!-- WHEN currentPageDetail TRUE, the detail can display on the Current page instead of jumping to the Parent page-->
    <xsl:param name="currentPageDetail"/>
    <xsl:variable name="contentParId">
      <xsl:choose>
        <xsl:when test="$parId!=''">
          <xsl:value-of select="$parId"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@parId"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- Get the correct page URL, Current or Parent -->
    <xsl:variable name="pageURL">
      <xsl:choose>
        <xsl:when test="$currentPageDetail='true'">
          <xsl:apply-templates select="$menu/MenuItem/descendant-or-self::MenuItem[@id=/Page/@id]" mode="getHref"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:choose>
            <xsl:when test="$menu/descendant-or-self::MenuItem[@id=$contentParId]/@url='/'">
              <xsl:call-template name="getSiteURL"/>

            </xsl:when>
            <xsl:otherwise>
              <xsl:apply-templates select="$menu/descendant-or-self::MenuItem[@id=$contentParId]" mode="getHref"/>?adminMode=<xsl:value-of select="$adminMode"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="substring-before($pageURL,'?') = ''">
        <xsl:value-of select="$pageURL"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="substring-before($pageURL,'?')"/>
      </xsl:otherwise>
    </xsl:choose>

    <!-- Get the correct content detail call -->
    <xsl:variable name="safeURLName">
      <xsl:apply-templates select="." mode="getSafeURLName"/>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="/Page/@pageExt and /Page/@pageExt!=''">
        <!-- in format = '/{@id}-/{@name}' -->
        <xsl:text>/</xsl:text>
        <xsl:value-of select="@id"/>
        <xsl:text>~</xsl:text>
        <xsl:value-of select="$safeURLName"/>
        <xsl:value-of select="/Page/@pageExt"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="$itemMode='live'">
            <xsl:text>/</xsl:text>
            <xsl:value-of select="@id"/>
            <xsl:text>-/</xsl:text>
            <xsl:value-of select="$safeURLName"/>
          </xsl:when>
          <xsl:when test="$itemMode='artid' or (contains($menu/descendant-or-self::MenuItem[@id=$parId]/@url,'?pgid'))">
            <xsl:if test="$adminMode='false'">
              <xsl:text>?pgid=</xsl:text>
              <xsl:value-of select="$contentParId"/>
            </xsl:if>
            <xsl:text>&amp;artid=</xsl:text>
            <xsl:value-of select="@id"/>
            <xsl:if test="$page/@ewCmd='NormalMail'">
              <xsl:text>&amp;ewCmd=Normal</xsl:text>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:choose>
              <xsl:when test="$DescriptiveContentURLs='true'">
                <xsl:text>/</xsl:text>
                <xsl:value-of select="@id"/>
                <xsl:text>-/</xsl:text>
                <xsl:value-of select="$safeURLName"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:text>/item</xsl:text>
                <xsl:value-of select="@id"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!--  ##  / END GET HREF TEMPLATES  #############################################################  -->





  <!-- BUILD ANCHOR/HYPERLINK to a CONTENT -->
  <xsl:template match="Content" mode="contentLink">
    <xsl:variable name="displayName">
      <xsl:apply-templates select="." mode="getDisplayName"/>
    </xsl:variable>
    <a title="{$displayName}">
      <xsl:attribute name="href">
        <xsl:apply-templates select="." mode="getHref"/>
      </xsl:attribute>
      <xsl:choose>
        <xsl:when test="@id=/Page/@artid">
          <xsl:attribute name="class">active</xsl:attribute>
        </xsl:when>
      </xsl:choose>
      <xsl:value-of select="$displayName"/>
    </a>
  </xsl:template>

  <!-- Build URL to Page -->
  <xsl:template match="MenuItem" mode="menuLink">
    <xsl:param name="span" select="false()"/>
    <xsl:param name="class"/>
    <a>

      <!-- get the href -->
      <xsl:attribute name="href">
        <xsl:apply-templates select="self::MenuItem" mode="getHref"/>
      </xsl:attribute>

      <!-- title attribute -->
      <xsl:attribute name="title">
        <xsl:apply-templates select="." mode="getTitleAttr"/>
      </xsl:attribute>


      <!-- check for different states to be applied -->
      <xsl:attribute name="class">
        <xsl:choose>
          <xsl:when test="self::MenuItem[@id=/Page/@id]">
            <xsl:text>active</xsl:text>
          </xsl:when>
          <xsl:when test="descendant::MenuItem[@id=/Page/@id] and ancestor::MenuItem">
            <xsl:text>on</xsl:text>
          </xsl:when>
        </xsl:choose>
        <xsl:if test="not($adminMode) and ($currentPage = ./parent::MenuItem and DisplayName/@paralaxLoad='true')">
          <xsl:text> paralax-load</xsl:text>
        </xsl:if>
        <xsl:text> </xsl:text>
        <xsl:value-of select="$class"/>
      </xsl:attribute>
      <xsl:if test="not($adminMode) and ($currentPage = ./parent::MenuItem and DisplayName/@paralaxLoad='true')">
        <xsl:attribute name="id">
          <xsl:text>slicelink-</xsl:text>
          <xsl:value-of select="@id"/>
        </xsl:attribute>
      </xsl:if>
      <!-- rel attribute -->
      <xsl:choose>
        <!-- Test for Home Page to insert microformat homepage attribute-->
        <xsl:when test="@id=$page/Menu/MenuItem/@id">
          <xsl:attribute name="rel">home</xsl:attribute>
        </xsl:when>
        <!-- stick external rel on external links [@target is a depreciated attribute in xHTML] Most modern browsers open rel="external" in new tab or window without JS intervention -->
        <!-- if starts with / is a relative url but put external if contains http and set to external -->
        <xsl:when test="not(substring(@url,1,1)='/') and (contains(@url,'http://') and DisplayName/@linkType='external')">
          <xsl:attribute name="rel">external</xsl:attribute>
        </xsl:when>
        <!-- When page set to no index add no follow to its link -->
        <xsl:when test="DisplayName/@noindex='true'">
          <xsl:attribute name="rel">nofollow</xsl:attribute>
        </xsl:when>
      </xsl:choose>

      <!-- output page name -->

      <xsl:choose>
        <xsl:when test="DisplayName[@icon!=''] or DisplayName[@uploadIcon!='']">
          <span class="menu-link-with-icon">
            <xsl:if test="DisplayName[@icon!='']">
              <i>
                <xsl:attribute name="class">
                  <xsl:text>fa </xsl:text>
                  <xsl:value-of select="DisplayName/@icon"/>
                </xsl:attribute>
                <xsl:text> </xsl:text>
              </i>
              <span class="space">&#160;</span>
            </xsl:if>
            <xsl:if test="DisplayName[@uploadIcon!='']">
              <span class="nav-icon">
                <img src="{DisplayName/@uploadIcon}" alt="icon"/>
              </span>
            </xsl:if>
            <xsl:apply-templates select="." mode="getDisplayName"/>
          </span>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="." mode="getDisplayName"/>
        </xsl:otherwise>
      </xsl:choose>

      <!-- add in aditional span - useful for icons or rounded corners -->
      <xsl:if test="$span">
        <span>&#160;</span>
      </xsl:if>
    </a>

  </xsl:template>

  <!-- -->

  <xsl:template match="MenuItem" mode="getTitleAttr">
    <xsl:choose>
      <xsl:when test="DisplayName/@title and DisplayName/@title!=''">
        <xsl:value-of select="DisplayName/@title"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="." mode="getDisplayName"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>



  <!-- -->

  <xsl:template match="Content" mode="getSafeURLName">
    <xsl:variable name="strippedName">
      <xsl:value-of select="translate(@name, translate(@name,'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ',''),'')"/>
    </xsl:variable>
    <xsl:value-of select="translate(translate($strippedName,' ','-'),'+','-')"/>
  </xsl:template>


  <!-- -->

  <xsl:template match="Content[@type='Contact']" mode="getSafeURLName">
      <xsl:variable name="apos">'</xsl:variable>
    <xsl:variable name="name">
      <xsl:choose>
      <xsl:when test="GivenName/node()!=''">
       <xsl:value-of select="translate(GivenName/node(), translate(@name,'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ',''),'')"/>
        <xsl:text>-</xsl:text>
       <xsl:value-of select="translate(Surname/node(), translate(@name,'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ',''),'')"/>
      </xsl:when>
        <xsl:otherwise>
              <xsl:value-of select="translate(@name, translate(@name,'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ',''),'')"/>
      </xsl:otherwise>
        </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="translate(translate($name,' /\.:','----'),$apos,'')"/>
  </xsl:template>

  <!--   ##########################################################   Generic Menus   ##########################################################   -->

  <xsl:template match="MenuItem" mode="mainmenu">
    <xsl:param name="homeLink"/>
    <xsl:param name="span"/>
    <xsl:variable name="liClass">
      <xsl:if test="self::MenuItem[@id=/Page/@id]">
        <xsl:text>active </xsl:text>
      </xsl:if>
      <xsl:if test="contains(DisplayName/@screens,'lg')">
        <xsl:text> hidden-lg-nav</xsl:text>
      </xsl:if>
      <xsl:if test="contains(DisplayName/@screens,'md')">
        <xsl:text> hidden-md-nav</xsl:text>
      </xsl:if>
      <xsl:if test="contains(DisplayName/@screens,'sm')">
        <xsl:text> hidden-sm-nav</xsl:text>
      </xsl:if>
      <xsl:if test="contains(DisplayName/@screens,'xs')">
        <xsl:text> hidden-xs-nav</xsl:text>
      </xsl:if>
      <xsl:choose>
        <xsl:when test="position()=1">
          <!-- If there is no Home link in the menu the first MenuItem in the menu should have a class of 'first' -->
          <xsl:text> first</xsl:text>
        </xsl:when>
        <xsl:when test="position()=last()">
          <xsl:text> last</xsl:text>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <li class="{$liClass}">
      <xsl:apply-templates select="self::MenuItem" mode="menuLink">
        <xsl:with-param name="span" select="$span" />
      </xsl:apply-templates>
    </li>
  </xsl:template>



  <xsl:template match="MenuItem" mode="mainmenudropdown">
    <xsl:param name="homeLink"/>
    <xsl:param name="span"/>
    <xsl:variable name="liClass">
      <xsl:if test="self::MenuItem[@id=/Page/@id]">
        <xsl:text>active </xsl:text>
      </xsl:if>
      <xsl:if test="contains(DisplayName/@screens,'lg')">
        <xsl:text> hidden-lg-nav</xsl:text>
      </xsl:if>
      <xsl:if test="contains(DisplayName/@screens,'md')">
        <xsl:text> hidden-md-nav</xsl:text>
      </xsl:if>
      <xsl:if test="contains(DisplayName/@screens,'sm')">
        <xsl:text> hidden-sm-nav</xsl:text>
      </xsl:if>
      <xsl:if test="contains(DisplayName/@screens,'xs')">
        <xsl:text> hidden-xs-nav</xsl:text>
      </xsl:if>
      <xsl:choose>
        <xsl:when test="position()=1">
          <!-- If there is no Home link in the menu the first MenuItem in the menu should have a class of 'first' -->
          <xsl:text> first</xsl:text>
        </xsl:when>
        <xsl:when test="position()=last()">
          <xsl:text> last</xsl:text>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <li class="{$liClass}">
      <xsl:apply-templates select="self::MenuItem" mode="menuLink">
        <xsl:with-param name="span" select="$span" />
      </xsl:apply-templates>
    </li>
  </xsl:template>

  <xsl:template match="MenuItem[count(MenuItem[not(DisplayName/@exclude='true')])&gt;0 and DisplayName/@exclude!='true']" mode="mainmenudropdown">
    <xsl:param name="homeLink"/>
    <xsl:param name="span"/>
    <xsl:param name="hover"/>
    <xsl:param name="mobileDD"/>
    <xsl:variable name="liClass">
      <xsl:if test="self::MenuItem[@id=/Page/@id]">
        <xsl:text>active </xsl:text>
      </xsl:if>
      <xsl:if test="descendant::MenuItem[@id=/Page/@id]">
        <xsl:text>on </xsl:text>
      </xsl:if>
      <xsl:if test="contains(DisplayName/@screens,'lg')">
        <xsl:text> hidden-lg-nav</xsl:text>
      </xsl:if>
      <xsl:if test="contains(DisplayName/@screens,'md')">
        <xsl:text> hidden-md-nav</xsl:text>
      </xsl:if>
      <xsl:if test="contains(DisplayName/@screens,'sm')">
        <xsl:text> hidden-sm-nav</xsl:text>
      </xsl:if>
      <xsl:if test="contains(DisplayName/@screens,'xs')">
        <xsl:text> hidden-xs-nav</xsl:text>
      </xsl:if>
      <xsl:choose>
        <xsl:when test="position()=1">
          <!-- If there is no Home link in the menu the first MenuItem in the menu should have a class of 'first' -->
          <xsl:text> first</xsl:text>
        </xsl:when>
        <xsl:when test="position()=last()">
          <xsl:text> last</xsl:text>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>

    <li class="{$liClass} dropdown">
      <xsl:if test="$hover='true'">
        <xsl:attribute name="class">
          <xsl:value-of select="$liClass"/>
          <xsl:text> dropdown dropdown-hover-menu</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <a href="{@url}" id="mainNavDD{@id}">
        <xsl:choose>
          <xsl:when test="$hover='true'">
            <xsl:attribute name="data-hover">dropdown</xsl:attribute>
            <xsl:attribute name="data-close-others">true</xsl:attribute>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="data-toggle">dropdown</xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:attribute name="class">
          <xsl:choose>
            <xsl:when test="self::MenuItem[@id=/Page/@id]">
              <xsl:text>active</xsl:text>
            </xsl:when>
            <xsl:when test="descendant::MenuItem[@id=/Page/@id] and ancestor::MenuItem">
              <xsl:text>on</xsl:text>
            </xsl:when>
          </xsl:choose>
        </xsl:attribute>
        <xsl:if test="DisplayName[@icon!='']">
          <i>
            <xsl:attribute name="class">
              <xsl:text>fa </xsl:text>
              <xsl:value-of select="DisplayName/@icon"/>
            </xsl:attribute>
            <xsl:text> </xsl:text>
          </i>&#160;
        </xsl:if>
        <xsl:if test="DisplayName[@uploadIcon!='']">
          <span class="nav-icon">
            <img src="{DisplayName/@uploadIcon}" alt="icon"/>
          </span>
        </xsl:if>
        <xsl:apply-templates select="." mode="getDisplayName"/>
      </a>
      <xsl:if test="$mobileDD='true'">
        <span class="mobile-dd-control">
          <i class="fa fa-angle-down"> </i>
          <i class="fa fa-angle-up"> </i>
        </span>
      </xsl:if>
      <ul class="dropdown-menu" aria-labelledby="mainNavDD{@id}">
        <xsl:apply-templates select="MenuItem[@name!='Information' and @name!='Footer' and not(DisplayName/@exclude='true')]" mode="submenuitem"/>
      </ul>
    </li>
  </xsl:template>

  <!-- Match to catch overrides -->
  <xsl:template match="MenuItem[DisplayName/@exclude='true']" mode="mainmenu">
    <xsl:param name="homeLink"/>
  </xsl:template>

  <!-- Match to catch overrides -->
  <xsl:template match="MenuItem[DisplayName/@exclude='true']" mode="mainmenudropdown">
    <xsl:param name="homeLink"/>
  </xsl:template>

  <!-- Mainmenu match on rootpage -->
  <xsl:template match="Menu/MenuItem" mode="mainmenu">
    <xsl:param name="span"/>
    <ul>
      <!-- Homepage should always have class of 'first' -->
      <li class="first">
        <xsl:apply-templates select="self::MenuItem" mode="menuLink"/>
      </li>
      <xsl:apply-templates select="MenuItem[not(DisplayName/@exclude='true')]" mode="mainmenu">
        <xsl:with-param name="homeLink" select="true()"/>
        <xsl:with-param name="span" select="$span"/>
      </xsl:apply-templates>
    </ul>
    <div class="terminus">&#160;</div>
  </xsl:template>

  <!-- -->


  <xsl:template match="MenuItem" mode="mainmenutabs">
    <xsl:param name="homeLink"/>
    <li>
      <xsl:choose>
        <xsl:when test="position()=1 and not($homeLink)">
          <!-- If there is no Home link in the menu the first MenuItem in the menu should have a class of 'first' -->
          <xsl:attribute name="class">
            <xsl:text>first</xsl:text>
          </xsl:attribute>
        </xsl:when>
        <xsl:when test="position()=last()">
          <xsl:attribute name="class">
            <xsl:text>last</xsl:text>
          </xsl:attribute>
        </xsl:when>
      </xsl:choose>
      <xsl:apply-templates select="self::MenuItem" mode="menuLinkTab"/>
    </li>
  </xsl:template>
  <!-- catch excludes -->
  <xsl:template match="MenuItem[DisplayName/@exclude='true']" mode="mainmenutabs"></xsl:template>

  <!-- override for Root page -->
  <xsl:template match="Menu/MenuItem" mode="mainmenutabs">
    <ul>
      <!-- Homepage should always have class of 'first' -->
      <li class="first">
        <xsl:apply-templates select="self::MenuItem" mode="menuLinkTab"/>
      </li>
      <xsl:apply-templates select="MenuItem[not(DisplayName/@exclude='true')]" mode="mainmenutabs">
        <xsl:with-param name="homeLink" select="true()"/>
      </xsl:apply-templates>
    </ul>
    <div class="terminus">&#160;</div>
  </xsl:template>
  <!-- -->

  <!--SNT:		mainmenu_sidenavtree
				homepage link and all subsequent levels of navigation nested -->
  <xsl:template match="Menu" mode="mainmenu_sidenavtree">
    <xsl:apply-templates select="MenuItem" mode="mainmenu_sidenavtree"/>
  </xsl:template>

  <xsl:template match="MenuItem[parent::Menu]" mode="mainmenu_sidenavtree">
    <xsl:choose>
      <xsl:when test="$page[@cssFramework='bs3']">
        <ul class="nav nav-pills nav-stacked">
          <li>
            <xsl:attribute name="class">
              <xsl:text>first</xsl:text>
            </xsl:attribute>
            <xsl:apply-templates select="self::MenuItem" mode="menuLink"/>

          </li>
          <xsl:apply-templates select="MenuItem[@name!='Information' and @name!='Footer' and not(DisplayName/@exclude='true')]" mode="submenuitem"/>
        </ul>
      </xsl:when>
      <xsl:otherwise>
        <ul>
          <li>
            <xsl:attribute name="class">
              <xsl:text>first</xsl:text>
            </xsl:attribute>
            <xsl:apply-templates select="self::MenuItem" mode="menuLink"/>
          </li>
          <xsl:apply-templates select="MenuItem[not(DisplayName/@exclude='true')]" mode="submenuitem"/>
        </ul>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!--TNSS:	submenu
				second and all subsequent levels of navigation nested  -->
  <xsl:template match="MenuItem" mode="submenu">
    <xsl:param name="sectionHeading"/>
    <xsl:choose>
      <xsl:when test="$page[@cssFramework='bs3']">
        <xsl:if test="$sectionHeading='true'">
          <h4>
            <xsl:apply-templates select="self::MenuItem" mode="menuLink" />
          </h4>
        </xsl:if>
        <ul>
          <xsl:attribute name="class">
            <xsl:text>nav nav-pills nav-stacked</xsl:text>
          </xsl:attribute>
          <xsl:apply-templates select="MenuItem[not(DisplayName/@exclude='true')]" mode="submenuitem"/>
        </ul>
      </xsl:when>
      <xsl:otherwise>
        <ul>
          <xsl:apply-templates select="MenuItem[not(DisplayName/@exclude='true')]" mode="submenuitem"/>
        </ul>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- -->


  <xsl:template match="MenuItem" mode="submenuitem">
    <xsl:choose>
      <xsl:when test="$page[@cssFramework='bs3']">
        <li>
          <xsl:attribute name="class">
            <xsl:if test="position()=1">
              <xsl:text>first </xsl:text>
            </xsl:if>
            <xsl:if test="position()=last()">
              <xsl:text>last </xsl:text>
            </xsl:if>
            <xsl:if test="self::MenuItem[@id=/Page/@id]">
              <xsl:text>active </xsl:text>
            </xsl:if>
            <xsl:if test="descendant::MenuItem[@id=/Page/@id] and @url!='/'">
              <xsl:text>active </xsl:text>
            </xsl:if>
          </xsl:attribute>
          <xsl:apply-templates select="self::MenuItem" mode="menuLink"/>
          <xsl:if test="count(child::MenuItem[not(DisplayName/@exclude='true')])&gt;0 and descendant-or-self::MenuItem[@id=/Page/@id]">
            <ul>
              <xsl:attribute name="class">
                <xsl:text>nav nav-pills</xsl:text>
                <!--TS Theme specfic setting must not be here - Moved to Layout XSL -->
                <!--xsl:if test="$themeLayout='TopNavSideSub' or $themeLayout='SideNav'">
                  <xsl:text> nav-stacked</xsl:text>
                </xsl:if-->
              </xsl:attribute>
              <xsl:apply-templates select="MenuItem[not(DisplayName/@exclude='true')]" mode="submenuitem"/>
            </ul>
          </xsl:if>
        </li>
      </xsl:when>
      <xsl:otherwise>
        <li>
          <xsl:if test="position()=1">
            <xsl:attribute name="class">
              <xsl:text>first</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="position()=last()">
            <xsl:attribute name="class">
              <xsl:text>last</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <xsl:apply-templates select="self::MenuItem" mode="menuLink"/>
          <xsl:if test="count(child::MenuItem[not(DisplayName/@exclude='true')])&gt;0 and descendant-or-self::MenuItem[@id=/Page/@id]">
            <ul>
              <xsl:apply-templates select="MenuItem[not(DisplayName/@exclude='true')]" mode="submenuitem"/>
            </ul>
          </xsl:if>
        </li>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!--TN:	submenu_topnav generic single level of navigation -->

  <!-- -->

  <xsl:template match="MenuItem" mode="submenu_topnav">
    <ul>
      <xsl:apply-templates select="MenuItem[not(DisplayName/@exclude='true')]" mode="submenuitem_topnav"/>
    </ul>
    <div class="terminus">&#160;</div>
  </xsl:template>
  <!-- -->
  <xsl:template match="MenuItem" mode="submenuitem_topnav">
    <li>
      <xsl:if test="position()=1">
        <xsl:attribute name="class">
          <xsl:text>first</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="position()=last()">
        <xsl:attribute name="class">
          <xsl:text>last</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates select="self::MenuItem" mode="menuLink"/>
    </li>
  </xsl:template>

  <!-- Generic Menu Link FOR TABBED Menu's-->
  <xsl:template match="MenuItem" mode="menuLinkTab">
    <xsl:variable name="displayName">
      <xsl:apply-templates select="." mode="getDisplayName"/>
    </xsl:variable>
    <a title="{$displayName}">
      <xsl:attribute name="href">
        <xsl:apply-templates select="self::MenuItem" mode="getHref"/>
      </xsl:attribute>
      <xsl:choose>
        <xsl:when test="self::MenuItem[@id=/Page/@id]">
          <xsl:attribute name="class">active</xsl:attribute>
        </xsl:when>
        <xsl:when test="descendant::MenuItem[@id=/Page/@id] and @url!='/'">
          <xsl:attribute name="class">on</xsl:attribute>
        </xsl:when>
      </xsl:choose>
      <!-- Test for Home Page to insert microformat homepage attribute-->
      <xsl:if test="@url='/'">
        <xsl:attribute name="rel">
          <xsl:text>home</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <span class="navTab-tl">&#160;</span>
      <xsl:value-of select="$displayName"/>
    </a>
  </xsl:template>

  <!-- -->

  <!-- ==================== Generic More Details Links ==================== -->

  <xsl:template match="*" mode="moreLink">
    <xsl:param name="link"/>
    <xsl:param name="linkText"/>
    <xsl:param name="altText"/>
    <xsl:param name="linkType"/>
    <xsl:choose>
      <xsl:when test="$page[@cssFramework='bs3']">
        <div class="morelink">
          <span>
            <a href="{$link}" title="{$altText}" class="btn btn-default btn-sm" itemprop="mainEntityOfPage">
              <xsl:if test="not(substring($link,1,1)='/') and (contains($link,'http://') and $linkType='external')">
                <xsl:attribute name="rel">external</xsl:attribute>
                <xsl:attribute name="class">extLink</xsl:attribute>
              </xsl:if>
              <xsl:choose>
                <xsl:when test="$linkText!=''">
                  <xsl:value-of select="$linkText"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="term2042" />
                </xsl:otherwise>
              </xsl:choose>
              <span class="hidden">
                <xsl:text>about </xsl:text>
                <xsl:value-of select="$altText"/>
              </span>
            </a>
          </span>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <div class="morelink">
          <span>
            <a href="{$link}" title="{$altText}" itemprop="mainEntityOfPage">
              <xsl:if test="not(substring($link,1,1)='/') and (contains($link,'http://') and $linkType='external')">
                <xsl:attribute name="rel">external</xsl:attribute>
                <xsl:attribute name="class">extLink</xsl:attribute>
              </xsl:if>
              <xsl:choose>
                <xsl:when test="$linkText!=''">
                  <xsl:value-of select="$linkText"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="term2042" />
                </xsl:otherwise>
              </xsl:choose>
              <span class="hidden">
                <xsl:text>about </xsl:text>
                <xsl:value-of select="$altText"/>
              </span>
              <span class="gtIcon">&#160;&gt;</span>
            </a>
          </span>
        </div>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="Content[@type='Module']" mode="moreLink">
    <xsl:variable name="link" select="@link"/>
    <xsl:if test="$link!=''">
      <xsl:variable name="numbertest">
        <!-- Test if link is numeric then link is internal-->
        <xsl:call-template name="IsNan">
          <xsl:with-param name="var" select="$link"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:choose>
        <xsl:when test="$page[@cssFramework='bs3']">
          <div class="morelink">
            <span>
              <a title="{@linkText}" class="btn btn-default btn-sm">
                <xsl:choose>
                  <xsl:when test="$numbertest = 'number'">
                    <xsl:variable name="pageId" select="@link"/>
                    <xsl:attribute name="href">
                      <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]" mode="getHref"/>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:choose>
                      <xsl:when test="contains($link,'#')">
                        <xsl:attribute name="class">
                          <xsl:text>btn btn-default btn-sm scroll-to-anchor</xsl:text>
                        </xsl:attribute>
                        <xsl:attribute name="href">
                          <xsl:value-of select="$link"/>
                        </xsl:attribute>
                      </xsl:when>
                      <xsl:when test="contains($link,'http')">
                        <xsl:attribute name="href">
                          <xsl:value-of select="$link"/>
                        </xsl:attribute>
                        <xsl:attribute name="rel">external</xsl:attribute>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:attribute name="href">
                          <xsl:text>http://</xsl:text>
                          <xsl:value-of select="$link"/>
                        </xsl:attribute>
                        <xsl:attribute name="rel">external</xsl:attribute>
                      </xsl:otherwise>
                    </xsl:choose>
                    
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="@linkText"/>
              </a>
            </span>
          </div>
        </xsl:when>
        <xsl:otherwise>
          <div class="morelink">
            <span>
              <a title="{@linkText}">
                <xsl:choose>
                  <xsl:when test="$numbertest = 'number'">
                    <xsl:variable name="pageId" select="@link"/>
                    <xsl:attribute name="href">
                      <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]" mode="getHref"/>
                    </xsl:attribute>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:choose>
                      <xsl:when test="contains($link,'http')">
                        <xsl:attribute name="href">
                          <xsl:value-of select="$link"/>
                        </xsl:attribute>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:attribute name="href">
                          <xsl:text>http://</xsl:text>
                          <xsl:value-of select="$link"/>
                        </xsl:attribute>
                      </xsl:otherwise>
                    </xsl:choose>
                    <xsl:attribute name="rel">external</xsl:attribute>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:value-of select="@linkText"/>
                <!--<span class="gtIcon">&#160;&gt;</span>-->
              </a>
            </span>
          </div>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>

  <xsl:template name="IsNan">
    <xsl:param name="var"/>
    <xsl:if test="string(number($var)) = 'NaN'">
      <xsl:text>string</xsl:text>
    </xsl:if>
    <xsl:if test="string(number($var)) != 'NaN'">
      <xsl:text>number</xsl:text>
    </xsl:if>
  </xsl:template>


  <xsl:template match="Content[@type='Module' and @moduleType='Image']" mode="moreLink"/>

  <!-- -->
  <xsl:template match="Content[@type='Link']" mode="moreLink">
    <xsl:param name="link"/>
    <xsl:param name="altText"/>
    <xsl:choose>
      <xsl:when test="$page[@cssFramework='bs3']">
        <div class="morelink">
          <span>
            <a href="{$link}" title="Click here to go to {link}" class="extLink btn btn-default btn-sm">
              <xsl:if test="contains($link,'www.') or contains($link,'WWW.') or contains($link,'http://') or contains($link,'HTTP://')">
                <xsl:attribute name="rel">external</xsl:attribute>
              </xsl:if>
              <xsl:choose>
                <xsl:when test="contains($link,'www.') or contains($link,'WWW.') or contains($link,'http://') or contains($link,'HTTP://')">
                  <xsl:choose>
                    <xsl:when test="string-length($altText) &gt; 70">
                      <xsl:value-of select="substring($altText,1,60)"/> ......
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="$altText"/>

                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="term2042" />
                  <xsl:text> </xsl:text>
                  <span class="hidden">
                    <xsl:text>about </xsl:text>
                    <xsl:value-of select="altText"/>
                  </span>
                </xsl:otherwise>
              </xsl:choose>
            </a>
          </span>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <span>
          <a href="{$link}" title="Click here to go to {link}" class="extLink">
            <xsl:if test="contains($link,'www.') or contains($link,'WWW.') or contains($link,'http://') or contains($link,'HTTP://')">
              <xsl:attribute name="rel">external</xsl:attribute>
            </xsl:if>
            <xsl:choose>
              <xsl:when test="contains($link,'www.') or contains($link,'WWW.') or contains($link,'http://') or contains($link,'HTTP://')">
                <xsl:choose>
                  <xsl:when test="string-length($altText) &gt; 70">
                    <xsl:value-of select="substring($altText,1,60)"/> ......
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$altText"/>

                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <xsl:otherwise>
                <xsl:call-template name="term2042" />
                <xsl:text> </xsl:text>
                <span class="hidden">
                  <xsl:text>about </xsl:text>
                  <xsl:value-of select="altText"/>
                </span>
                <span class="gtIcon">&#160;&gt;</span>
              </xsl:otherwise>
            </xsl:choose>
          </a>
        </span>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="*" mode="backLink">
    <xsl:param name="link"/>
    <xsl:param name="linkText"/>
    <xsl:param name="altText"/>

    <div class="backlink">

      <span>
        <a href="{$link}" title="{$altText}">
          <xsl:attribute name="title">
            <xsl:choose>
              <xsl:when test="$altText != ''">
                <xsl:value-of select="$altText"/>
              </xsl:when>
              <xsl:otherwise>
                <!-- Back to list -->
                <xsl:call-template name="term2022" />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <xsl:if test="$page[@cssFramework='bs3']">
            <xsl:attribute name="class">btn btn-default btn-xs pull-left</xsl:attribute>
            <i class="fa fa-chevron-left">
              <xsl:text> </xsl:text>
            </i>&#160;
          </xsl:if>
          <xsl:choose>
            <xsl:when test="$linkText!=''">
              <xsl:value-of select="$linkText"/>
            </xsl:when>
            <xsl:otherwise>
              <!-- Back to list -->
              <xsl:call-template name="term2022" />
            </xsl:otherwise>
          </xsl:choose>
          <xsl:if test="$altText !=''">
            <span class="hidden">
              <!-- about -->
              <xsl:call-template name="term2023" />
              <xsl:text>&#160;</xsl:text>
              <xsl:value-of select="altText"/>
            </span>
          </xsl:if>
        </a>
      </span>
    </div>
  </xsl:template>


  <!-- -->
  <!--   ###################################################   Full Size Picture button   ###################################################   -->
  <!-- -->
  <xsl:template match="Content" mode="imageEnlarge">
    <i class="fa fa-search-plus">
      <xsl:text> </xsl:text>
    </i>
    <xsl:text> </xsl:text>
    <!--click to enlarge-->
    <span>
      <xsl:call-template name="term2049" />
    </span>
  </xsl:template>

  <!-- ==================== / Generic Status Legend ==================== -->

  <xsl:template match="MenuItem | Content | ListItem | TreeItem | PageActivity | Subscription" mode="status_legend">
    <a class="status" title="none">
      <xsl:choose>
        <xsl:when test="@status=0">
          <xsl:attribute name="class">
            <xsl:text>status inactive</xsl:text>
            <xsl:if test="MenuItem">Parent</xsl:if>
          </xsl:attribute>
          <xsl:attribute name="title">This content is hidden</xsl:attribute>
        </xsl:when>
        <xsl:when test="@status=1 or @status='-1'">
          <xsl:attribute name="class">
            <xsl:text>status active</xsl:text>
            <xsl:if test="MenuItem">Parent</xsl:if>
          </xsl:attribute>
          <xsl:attribute name="title">This content is live !</xsl:attribute>
        </xsl:when>
        <xsl:when test="@status=2">
          <xsl:attribute name="class">
            <xsl:text>status superceded</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="title">This content has been superceded</xsl:attribute>
        </xsl:when>
        <xsl:when test="@status=3">
          <xsl:attribute name="class">
            <xsl:text>status approval</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="title">This content is awaiting approval</xsl:attribute>
        </xsl:when>
        <xsl:when test="@status=4">
          <xsl:attribute name="class">
            <xsl:text>status editing</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="title">This content is being edited</xsl:attribute>
        </xsl:when>
        <xsl:when test="@status=5">
          <xsl:attribute name="class">
            <xsl:text>status rejected</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="title">This content is on hold/rejected</xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="class">
            <xsl:text>status active</xsl:text>
            <xsl:if test="MenuItem | Content | ListItem | TreeItem | PageActivity">Parent</xsl:if>
          </xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
      &#160;
    </a>
  </xsl:template>


  <xsl:template match="MenuItem | PageVersion" mode="status_legend">
    <xsl:choose>
      <xsl:when test="@status=0">
        <i>
          <xsl:attribute name="class">
            <xsl:text>fa fa-file-text-o fa-lg text-muted status inactive</xsl:text>
            <xsl:if test="MenuItem">Parent</xsl:if>
          </xsl:attribute>
          &#160;
        </i>
      </xsl:when>
      <xsl:when test="@status=1 or @status='-1'">
        <i>
          <xsl:attribute name="class">
            <xsl:text>fa fa-file-text-o fa-lg status active</xsl:text>
            <xsl:if test="MenuItem">Parent</xsl:if>
          </xsl:attribute>
          &#160;
        </i>
      </xsl:when>
      <xsl:when test="@status=2">

      </xsl:when>
      <xsl:when test="@status=3">

      </xsl:when>
      <xsl:when test="@status=4">

      </xsl:when>
      <xsl:when test="@status=5">

      </xsl:when>
      <xsl:otherwise>
        <xsl:attribute name="class">
          <xsl:text>status active</xsl:text>
          <xsl:if test="MenuItem | Content | ListItem | TreeItem | PageActivity">Parent</xsl:if>
        </xsl:attribute>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>



  <xsl:template match="Content | ListItem | PageActivity" mode="status_legend">
    <i class="status" title="none">
      <xsl:choose>
        <xsl:when test="@status=0">
          <xsl:attribute name="class">
            <xsl:text>status fa fa-eye-slash fa-lg inactive</xsl:text>
            <xsl:if test="MenuItem">Parent</xsl:if>
          </xsl:attribute>
          <xsl:attribute name="title">This content is hidden</xsl:attribute>
        </xsl:when>
        <xsl:when test="@status=1 or @status='-1'">
          <xsl:attribute name="class">
            <xsl:text>status fa fa-eye fa-lg active</xsl:text>
            <xsl:if test="MenuItem">Parent</xsl:if>
          </xsl:attribute>
          <xsl:attribute name="title">This content is live</xsl:attribute>
        </xsl:when>
        <xsl:when test="@status=2">
          <xsl:attribute name="class">
            <xsl:text>status fa a-clock-o fa-lg</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="title">This content has been superceded</xsl:attribute>
        </xsl:when>
        <xsl:when test="@status=3">
          <xsl:attribute name="class">
            <xsl:text>status fa fa-eye fa-lg</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="title">This content is awaiting approval</xsl:attribute>
        </xsl:when>
        <xsl:when test="@status=4">
          <xsl:attribute name="class">
            <xsl:text>status fa fa-pencil fa-lg</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="title">This content is being edited</xsl:attribute>
        </xsl:when>
        <xsl:when test="@status=5">
          <xsl:attribute name="class">
            <xsl:text>status fa fa-trash-o fa-lg</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="title">This content is on hold/rejected</xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="class">
            <xsl:text>status active</xsl:text>
            <xsl:if test="MenuItem | Content | ListItem | TreeItem | PageActivity">Parent</xsl:if>
          </xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:text> </xsl:text>
    </i>
  </xsl:template>

  <!--<xsl:template match="MenuItem" mode="status_legend">
    <i class="status" title="none">
      <xsl:choose>
        <xsl:when test="@status=0">
          <xsl:attribute name="class">
            <xsl:text>status hidden </xsl:text>
            <xsl:choose>
              <xsl:when test="MenuItem">icon-folder-close-alt</xsl:when>
              <xsl:otherwise>icon-file-alt</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <xsl:attribute name="title">This content is hidden</xsl:attribute>
        </xsl:when>
        <xsl:when test="@status=1 or @status='-1'">
          <xsl:attribute name="class">
            <xsl:text>status active </xsl:text>
            <xsl:choose>
              <xsl:when test="MenuItem">icon-folder-close-alt</xsl:when>
              <xsl:otherwise>icon-file-alt</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <xsl:attribute name="title">This content is live</xsl:attribute>
        </xsl:when>
        <xsl:when test="@status=2">
          <xsl:attribute name="class">
            <xsl:text>status superceded</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="title">This content has been superceded</xsl:attribute>
        </xsl:when>
        <xsl:when test="@status=3">
          <xsl:attribute name="class">
            <xsl:text>status approval</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="title">This content is awaiting approval</xsl:attribute>
        </xsl:when>
        <xsl:when test="@status=4">
          <xsl:attribute name="class">
            <xsl:text>status editing</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="title">This content is being edited</xsl:attribute>
        </xsl:when>
        <xsl:when test="@status=5">
          <xsl:attribute name="class">
            <xsl:text>status rejected</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="title">This content is on hold/rejected</xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="class">
            <xsl:text>status live</xsl:text>
            <xsl:if test="MenuItem | Content | ListItem | TreeItem | PageActivity">Parent</xsl:if>
          </xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
      &#160;
    </i>
  </xsl:template>-->


  <xsl:template name="status_legend">
    <xsl:param name="status"/>
    <i class="status" title="none">
      <xsl:choose>
        <xsl:when test="$status=0">
          <xsl:attribute name="class">
            <xsl:text>status fa fa-times-circle fa-lg text-danger inactive</xsl:text>
            <xsl:if test="MenuItem">Parent</xsl:if>
          </xsl:attribute>
          <xsl:attribute name="title">This content is hidden</xsl:attribute>
        </xsl:when>
        <xsl:when test="$status=1 or $status='-1'">
          <xsl:attribute name="class">
            <xsl:text>status fa fa-check fa-lg text-success active</xsl:text>
            <xsl:if test="MenuItem">Parent</xsl:if>
          </xsl:attribute>
          <xsl:attribute name="title">This content is live</xsl:attribute>
        </xsl:when>
        <xsl:when test="$status=2">
          <xsl:attribute name="class">
            <xsl:text>status fa a-clock-o fa-lg</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="title">This content has been superceded</xsl:attribute>
        </xsl:when>
        <xsl:when test="$status=3">
          <xsl:attribute name="class">
            <xsl:text>status fa fa-eye fa-lg</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="title">This content is awaiting approval</xsl:attribute>
        </xsl:when>
        <xsl:when test="$status=4">
          <xsl:attribute name="class">
            <xsl:text>status fa fa-pencil fa-lg</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="title">This content is being edited</xsl:attribute>
        </xsl:when>
        <xsl:when test="$status=5">
          <xsl:attribute name="class">
            <xsl:text>status fa fa-trash-o fa-lg</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="title">This content is on hold/rejected</xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="class">
            <xsl:text>status active</xsl:text>
            <xsl:if test="MenuItem | Content | ListItem | TreeItem | PageActivity">Parent</xsl:if>
          </xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:text> </xsl:text>
    </i>
  </xsl:template>


  <xsl:template name="ResultPager">
    <xsl:param name="nRows"/>
    <xsl:param name="nStart"/>
    <xsl:param name="nCount"/>
    <xsl:param name="cPath"/>
    <xsl:param name="nTotal"/>
    <xsl:if test="number($nStart) &gt; number($nRows)">
      <form action="{$cPath}" method="post" id="pagerPrevious" class="resultsPager">
        <input type="hidden" name="nStart" value="{number($nStart)-number($nRows)}"/>
        <input type="submit" name="previous" value="&lt; Previous {$nRows}"/>
      </form>
    </xsl:if>
    <xsl:if test="number($nCount)=number($nRows)">
      <form action="{$cPath}" method="post" id="pagerNext" class="resultsPager">
        <input type="hidden" name="nStart" value="{number($nStart)+number($nRows)}"/>
        <input type="submit" name="next" value="Next {$nRows} &gt;"/>
      </form>
    </xsl:if>
    Results <xsl:value-of select="$nStart"/> to <xsl:value-of select="number($nStart)+number($nCount)-1"/><xsl:if test="nTotal!=''">
      of <xsl:value-of select="nTotal"/>
    </xsl:if>
  </xsl:template>

  <!-- -->
  <xsl:template name="getContentParURL">
    <xsl:param name="parId"/>
    <xsl:choose>
      <xsl:when test="/Page/Menu/descendant-or-self::MenuItem[@id=$parId]/@url='/'"></xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="/Page/Menu/descendant-or-self::MenuItem[@id=$parId]/@url"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="getSecureURL">
    <xsl:value-of select="/Page/Cart/@cartURL"/>
  </xsl:template>

  <xsl:template name="getSiteURL">
    <xsl:choose>
      <xsl:when test="/Page/Cart/@siteURL!=''">
        <xsl:value-of select="/Page/Cart/@siteURL"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="getXmlSettings">
          <xsl:with-param name="sectionName" select="'web'"/>
          <xsl:with-param name="valueName" select="'BaseUrl'"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="*" mode="reportHeader">
    <xsl:param name="sort"/>
    <xsl:param name="bSortFormMethod"/>
    <xsl:if test="not(name()='Email')">
      <th>
        <xsl:choose>
          <xsl:when test="name()='UserXml'">User</xsl:when>
          <xsl:when test="name()='Details'">Website</xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="translate(name(),'_',' ')"/>
            <xsl:if test="$sort=''">
              <xsl:call-template name="SortArrows">
                <xsl:with-param name="sortCol">
                  <xsl:value-of select="position()"/>
                </xsl:with-param>
                <xsl:with-param name="bSortFormMethod" select="$bSortFormMethod"/>
              </xsl:call-template>
            </xsl:if>
          </xsl:otherwise>
        </xsl:choose>
      </th>
    </xsl:if>
  </xsl:template>
  <!-- -->

  <xsl:template match="Voted_For" mode="reportCell">
    <td>
      <xsl:value-of select="Content/Title"/>
    </td>
  </xsl:template>

  <xsl:template match="*" mode="reportCell">
    <td>
      <xsl:choose>

        <xsl:when test="contains(name(),'Date') or contains(name(),'date') and not(contains(name(),'andidate'))">
          <xsl:attribute name="class">nowrap</xsl:attribute>
          <xsl:choose>
            <xsl:when test=".='0001-01-01T00:00:00.0000000-00:00' or .='0001-01-01T00:00:00+00:00'">
              <xsl:apply-templates select="." mode="nullDate"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="DD_Mon_YY">
                <xsl:with-param name="date">
                  <xsl:value-of select="translate(./node(),'_',' ')"/>
                </xsl:with-param>
                <xsl:with-param name="showTime">
                  <xsl:if test="contains(name(),'Taken')">true</xsl:if>
                </xsl:with-param>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>

        <xsl:when test="contains(name(),'Time_Taken')">
          <xsl:attribute name="class">nowrap</xsl:attribute>
          <xsl:call-template name="getNiceTimeFromSeconds">
            <xsl:with-param name="secs" select="."/>
            <xsl:with-param name="format">short</xsl:with-param>
          </xsl:call-template>
        </xsl:when>

        <xsl:when test="(contains(name(),'Percent') or name()='Mark') and (number(.)=number(.))">
          <xsl:value-of select="./node()"/>%
        </xsl:when>

        <xsl:otherwise>
          <xsl:choose>
            <xsl:when test="./node()!=''">
              <xsl:apply-templates select="./node()" mode="cleanXhtml"/>
            </xsl:when>
            <xsl:otherwise>
              &#160;
            </xsl:otherwise>
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>

    </td>
  </xsl:template>

  <xsl:template match="*" mode="nullDate">
    <xsl:text>None</xsl:text>
  </xsl:template>

  <xsl:template match="Email" mode="reportCell"/>

  <xsl:template match="Mark[number(.)=number(.)]" mode="reportCell">
    <td>
      <xsl:value-of select="./node()"/>%
    </td>
  </xsl:template>

  <xsl:template match="Status | nStatus | status" mode="reportCell">
    <td>
      <xsl:call-template name="StatusLegend">
        <xsl:with-param name="status">
          <xsl:value-of select="node()"/>
        </xsl:with-param>
      </xsl:call-template>
    </td>
  </xsl:template>

  <xsl:template match="Username | Full_Name[not(parent::*//Username)]" mode="reportCell">
    <td>
      <a href="mailto:{parent::*//Email/node()}">
        <xsl:value-of select="node()"/>
      </a>
    </td>
  </xsl:template>

  <xsl:template match="Grade" mode="reportCell">
    <td>
      <xsl:attribute name="class">nowrap</xsl:attribute>
      <xsl:value-of select="./node()"/>
    </td>
  </xsl:template>

  <xsl:template match="Company[not(parent::attempt)]" mode="reportCell">
    <td>
      <a href="http://:{Website/node()}">
        <xsl:value-of select="Website/node()"/>
      </a>
    </td>
  </xsl:template>

  <xsl:template match="Details" mode="reportCell">
    <td>
      <a href="http://:{./Company/Website/node()}">
        <xsl:value-of select="./Company/Website/node()"/>
      </a>
    </td>
  </xsl:template>

  <xsl:template match="User" mode="reportCell">
    <td>
      <a href="/{$appPath}?ewCmd=Profile&amp;DirType=User&amp;id={ancestor::user/@id}">
        <span class="btn btn-primary btn-xs">
          <i class="fa fa-user fa-white">
            <xsl:text> </xsl:text>
          </i>
        </span>
        &#160;<xsl:choose>
        <xsl:when test="FirstName and LastName">
          <xsl:value-of select="LastName"/>, <xsl:value-of select="FirstName"/>
        </xsl:when>
        <xsl:when test="User/FirstName and User/LastName">
          <xsl:value-of select="User/LastName"/>, <xsl:value-of select="User/FirstName"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="node()"/>
        </xsl:otherwise>
      </xsl:choose>
      </a>
      
    </td>
  </xsl:template>

  <xsl:template match="UserXml" mode="reportCell">
    <td>
      <xsl:if test="User/LastName/node()!='' or User/FirstName/node()=''">
        <a href="mailto:{User/Email/node()}">
          <xsl:value-of select="User/LastName/node()"/>,&#160;<xsl:value-of select="User/FirstName/node()"/>
        </a>
      </xsl:if>
    </td>
  </xsl:template>

  <xsl:template match="GroupXml" mode="reportCell">
    <td>
      <xsl:value-of select="Group/Name/node()"/>
    </td>
  </xsl:template>

  <xsl:template name="StatusLegend">
    <xsl:param name="status"/>
    <xsl:choose>
      <xsl:when test="$status='0'">
        <a href="#" data-toggle="tooltip" data-placement="right" title="Hidden" data-original-title="Hidden">
          <i class="fa fa-times text-danger" alt="inactive">&#160;</i>
        </a>
      </xsl:when>
      <xsl:when test="$status='-1'">
        <a href="#" data-toggle="tooltip" data-placement="right" title="Live" data-original-title="Live">
          <i class="fa fa-check text-success" alt="live">&#160;</i>
        </a>
      </xsl:when>
      <xsl:when test="$status='1'">
        <a href="#" data-toggle="tooltip" data-placement="right" title="Live" data-original-title="Live">
          <i class="fa fa-check text-success" alt="live">&#160;</i>
        </a>
      </xsl:when>
      <xsl:when test="$status='2'">
        <a href="#" data-toggle="tooltip" data-placement="right" title="Superceeded" data-original-title="Superceeded">
          <i class="fa fa-exclamation text-warning" alt="live">&#160;</i>
        </a>
      </xsl:when>
      <xsl:when test="$status='7'">
        <a href="#" data-toggle="tooltip" data-placement="right" title="Expired" data-original-title="Expired">
          <i class="fa fa-clock-o text-danger">&#160;</i>
        </a>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$status"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Version Control reportCell matches-->
  <xsl:template match="Version" mode="reportCell">
    <td>
      <xsl:value-of select="."/>
      <xsl:if test="parent::node()/@currentLiveVersion!=''">
        <xsl:text> [</xsl:text>
        <xsl:value-of select="parent::node()/@currentLiveVersion"/>
        <xsl:text>]</xsl:text>
      </xsl:if>
    </td>
  </xsl:template>
  <!-- -->
  <xsl:template match="Name[parent::Pending]" mode="reportCell">
    <td>
      <strong>
        <xsl:value-of select="."/>
      </strong>
    </td>
  </xsl:template>
  <!-- -->

  <xsl:template name="truncateString">
    <xsl:param name="string"/>
    <xsl:param name="length"/>

    <xsl:variable name="truncatedString" select="substring($string,1,$length)"/>
    <xsl:variable name="charAfterTruncatedString" select="substring($string,$length,1)"/>
    <xsl:choose>
      <xsl:when test="$charAfterTruncatedString=' ' or $charAfterTruncatedString=''">
        <xsl:value-of select="$truncatedString"/>
        <xsl:if test="$charAfterTruncatedString=' '">...</xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="substring-before-last">
          <xsl:with-param name="input" select="$truncatedString" />
          <xsl:with-param name="marker" select="' '" />
        </xsl:call-template>
        ...
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- -->
  <xsl:template name="substring-before-last">
    <!-- Adapted from dpawson.co.uk -->
    <xsl:param name="input" />
    <xsl:param name="marker" />
    <xsl:if test="contains($input,$marker)">
      <xsl:value-of select="substring-before($input,$marker)"/>
      <xsl:value-of select="$marker"/>
      <xsl:call-template name="substring-before-last">
        <xsl:with-param name="input"
					select="substring-after($input,$marker)" />
        <xsl:with-param name="marker" select="$marker" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
  <!-- -->
  <xsl:template name="substring-after-last">
    <!-- From dpawson.co.uk -->
    <xsl:param name="input" />
    <xsl:param name="marker" />

    <xsl:choose>
      <xsl:when test="contains($input,$marker)">
        <xsl:call-template name="substring-after-last">
          <xsl:with-param name="input"
						select="substring-after($input,$marker)" />
          <xsl:with-param name="marker" select="$marker" />
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$input" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- -->
  <xsl:template name="break">
    <xsl:param name="text" select="."/>
    <xsl:param name="replace"/>
    <xsl:choose>
      <xsl:when test="contains($text, '&#xA;')">
        <xsl:value-of select="substring-before($text, '&#xA;')"/>
        <xsl:choose>
          <xsl:when test="$replace='linebreak'">
            <br/>
          </xsl:when>
          <xsl:otherwise>&#160;</xsl:otherwise>
        </xsl:choose>
        <xsl:call-template name="break">
          <xsl:with-param name="text" select="substring-after($text,'&#xA;')"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$text"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Generic Steppers-->

  <xsl:template match="/" mode="genericStepperBasic">
    <xsl:param name="curPg"/>
    <xsl:param name="prevItem"/>
    <xsl:param name="nextItem"/>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="self::Content" mode="getHref"/>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="/Page[@cssFramework='bs3']">
        <ul class="pager">
          <xsl:choose>
            <xsl:when test="/Page/Contents/Content[contains(@name,$prevItem)]">
              <li class="previous">
                <a href="{$parentURL}?curPg={number($curPg) - 1}" title="go to the previous page">&lt; previous</a>
              </li>
            </xsl:when>
            <xsl:otherwise>
              <li class="previous disabled">
                <span class="ghosted">&lt; previous</span>
              </li>
            </xsl:otherwise>
          </xsl:choose> |
          <xsl:choose>
            <xsl:when test="/Page/Contents/Content[contains(@name,$nextItem)]">
              <li class="next">
                <a href="{$parentURL}?curPg={number($curPg) + 1}" title="go to the next page">next &gt;</a>
              </li>
            </xsl:when>
            <xsl:otherwise>
              <li class="next disabled">
                <span class="ghosted">next &gt;</span>
              </li>
            </xsl:otherwise>
          </xsl:choose>
        </ul>
      </xsl:when>
      <xsl:otherwise>
        <div class="stepper">
          <xsl:choose>
            <xsl:when test="/Page/Contents/Content[contains(@name,$prevItem)]">
              <a href="{$parentURL}?curPg={number($curPg) - 1}" title="go to the previous page">&lt; previous</a>
            </xsl:when>
            <xsl:otherwise>
              <span class="ghosted">&lt; previous</span>
            </xsl:otherwise>
          </xsl:choose> |
          <xsl:choose>
            <xsl:when test="/Page/Contents/Content[contains(@name,$nextItem)]">
              <a href="{$parentURL}?curPg={number($curPg) + 1}" title="go to the next page">next &gt;</a>
            </xsl:when>
            <xsl:otherwise>
              <span class="ghosted">next &gt;</span>
            </xsl:otherwise>
          </xsl:choose>
        </div>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <!-- -->
  <xsl:template match="/" mode="genericStepper">
    <xsl:param name="contentList"/>
    <xsl:param name="noPerPage"/>
    <xsl:param name="startPos"/>
    <xsl:param name="contentType"/>
    <xsl:param name="queryString"/>
    <xsl:param name="queryStringParam"/>
    <xsl:param name="totalCount" />
    <xsl:variable name="thisURL">
      <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]" mode="getHref" />

      <xsl:choose>
        <xsl:when test="contains(/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url,'?')">
          <xsl:text>&amp;</xsl:text>
        </xsl:when>
        <xsl:otherwise>?</xsl:otherwise>
      </xsl:choose>
      <xsl:value-of select="$queryString"/>
      <xsl:if test="$queryString!=''">&amp;</xsl:if>
      <xsl:choose>
        <xsl:when test="$queryStringParam!=''">
          <xsl:value-of select="$queryStringParam"/>
        </xsl:when>
        <xsl:otherwise>
          startPos
        </xsl:otherwise>
      </xsl:choose>

    </xsl:variable>
    <xsl:choose>
      <xsl:when test="/Page[@cssFramework='bs3']">
        <div class="col-md-12">
          <ul class="pager">

            <!-- Back Button-->
            <xsl:choose>
              <xsl:when test="$startPos - $noPerPage='0' and $startPos &gt; ($noPerPage - 1)">
                <xsl:variable name="origURL">
                  <xsl:apply-templates select="$currentPage" mode="getHref"/>
                </xsl:variable>
                <li class="previous">
                  <a href="{$origURL}" title="click here to view the previous page in sequence">&#8592; Back</a>
                </li>
              </xsl:when>
              <xsl:when test="$startPos &gt; ($noPerPage - 1)">
                <li class="previous">
                  <a href="{$thisURL}={$startPos - $noPerPage}" title="click here to view the previous page in sequence">&#8592; Back</a>
                </li>
              </xsl:when>
              <xsl:otherwise>
                <li class="previous disabled">
                  <a href="#">&#8592; Back</a>
                </li>
              </xsl:otherwise>
            </xsl:choose>


            <xsl:choose>
              <xsl:when test="$totalCount &gt; ($startPos +$noPerPage)">
                <li class="next">
                  <a href="{$thisURL}={$startPos+$noPerPage}" title="click here to view the next page in sequence">Next &#8594;</a>
                </li>
              </xsl:when>
              <xsl:otherwise>
                <li class="next disabled">
                  <span class="ghosted">Next &#8594;</span>
                </li>
              </xsl:otherwise>
            </xsl:choose>

            <!-- ### to ### of ### (At the top) -->
            <li class="itemInfo">
              <span class="pager-caption">
                <xsl:if test="$noPerPage!=1">
                  <xsl:value-of select="$startPos + 1"/>
                  <xsl:text> to </xsl:text>
                </xsl:if>
                <xsl:if test="$totalCount &gt;= ($startPos +$noPerPage)">
                  <xsl:value-of select="$startPos + $noPerPage"/>
                </xsl:if>
                <xsl:if test="$totalCount &lt; ($startPos + $noPerPage)">
                  <xsl:value-of select="$totalCount"/>
                </xsl:if> of <xsl:value-of select="$totalCount"/>
              </span>
            </li>
          </ul>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <div class="stepper">
          <p class="stepLinks">
            <!-- Back Button-->
            <xsl:choose>
              <xsl:when test="$startPos - $noPerPage='0' and $startPos &gt; ($noPerPage - 1)">
                <xsl:variable name="origURL">
                  <xsl:apply-templates select="$currentPage" mode="getHref"/>
                </xsl:variable>
                <a href="{$origURL}" title="click here to view the previous page in sequence">&lt; Back</a> |
              </xsl:when>
              <xsl:when test="$startPos &gt; ($noPerPage - 1)">
                <a href="{$thisURL}={$startPos - $noPerPage}" title="click here to view the previous page in sequence">&lt; Back</a> |
              </xsl:when>
              <xsl:otherwise>
                <span class="ghosted">&lt; Back | </span>
              </xsl:otherwise>
            </xsl:choose>
            <!-- Dividers -->
            <!--<xsl:for-each select="ms:node-set($contentList)/*">
          <xsl:choose>
            <xsl:when test="position()-$noPerPage=$startPos">
              <xsl:value-of select="position() div $noPerPage"/> |
            </xsl:when>
            <xsl:otherwise>
              <xsl:if test="(position() ) mod $noPerPage = '0'">
                <a href="{$thisURL}={position()-$noPerPage}">
                  <xsl:value-of select="position() div $noPerPage"/>
                </a> |
              </xsl:if>
              <xsl:if test="(position()) = (count(/Page/Contents/Content[@type=$contentType]) + count(/Page/ContentDetail/Content[@type=$contentType]) + count(/Page/Contents/Contact[@type=$contentType])) and ceiling(position() div $noPerPage)!=(position() div $noPerPage)">
                <xsl:choose>
                  <xsl:when test="$startPos+$noPerPage=(ceiling((position()) div $noPerPage)*$noPerPage)">
                    <span class="active">
                      <xsl:value-of select="ceiling(position() div $noPerPage)"/>
                    </span> |
                  </xsl:when>
                  <xsl:otherwise>
                    <a href="{$thisURL}={position()-(position() mod $noPerPage)}">
                      <xsl:value-of select="ceiling(position() div $noPerPage)"/>
                    </a> |
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:if>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:for-each-->
            <!-- Next Button-->

            <xsl:choose>
              <xsl:when test="$totalCount &gt; ($startPos +$noPerPage)">
                <a href="{$thisURL}={$startPos+$noPerPage}" title="click here to view the next page in sequence">Next &gt;</a>
              </xsl:when>
              <xsl:otherwise>
                <span class="ghosted">Next &gt;</span>
              </xsl:otherwise>
            </xsl:choose>
          </p>
          <!-- ### to ### of ### (At the top) -->
          <p class="itemInfo">
            <xsl:if test="$noPerPage!=1">
              <xsl:value-of select="$startPos + 1"/>
              <xsl:text> to </xsl:text>
            </xsl:if>
            <xsl:if test="$totalCount &gt;= ($startPos +$noPerPage)">
              <xsl:value-of select="$startPos + $noPerPage"/>
            </xsl:if>
            <xsl:if test="$totalCount &lt; ($startPos + $noPerPage)">
              <xsl:value-of select="$totalCount"/>
            </xsl:if> of <xsl:value-of select="$totalCount"/>
          </p>
        </div>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Retrieves the additional Params from the URL -->
  <xsl:template name="getQString">
    <xsl:for-each select="/Page/Request/QueryString/Item">
      <xsl:if test="@name!='path' and @name!='startPos'">
        <xsl:choose>
          <xsl:when test="position()=1">
            <xsl:value-of select="@name"/>
            <xsl:text>=</xsl:text>
            <xsl:value-of select="node()"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>&amp;</xsl:text>
            <xsl:value-of select="@name"/>
            <xsl:text>=</xsl:text>
            <xsl:value-of select="node()"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>
  <!-- -->



  <!-- ## CLEAN XHTML  ##########################################################################   -->
  <!--    Runs through supposid xHTML/XML making sure that it complies to xHTML standards,
          Also a handly place to overide any HTML tag to cause a specific behaviour.
          - e.g match="a" decide if external and do something special with the HTMl like a class or styling.
  -->

  <xsl:template match="*" mode="cleanXhtml">
    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="cleanXhtml"/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="table" mode="cleanXhtml">
    <div class="table-responsive">
      <xsl:element name="{name()}">
        <!-- process attributes -->
        <xsl:for-each select="@*">
          <!-- remove attribute prefix (if any) -->
          <xsl:attribute name="{name()}">
            <xsl:value-of select="." />
          </xsl:attribute>
        </xsl:for-each>
        <xsl:apply-templates mode="cleanXhtml"/>
      </xsl:element>
    </div>
  </xsl:template>




  <!-- IMAGE PROCESSING  -->
  <xsl:template match="img" mode="cleanXhtml">

    <!-- Stick in Variable and then ms:nodest it 
          - ensures its self closing and we can process all nodes!! -->
    <xsl:variable name="img">
      <xsl:element name="img">
        <xsl:choose>
          <xsl:when test="$lazy='on'">
            <xsl:attribute name="data-src">
              <xsl:value-of select="@src"/>
            </xsl:attribute>
            <xsl:attribute name="src">
              <xsl:value-of select="$lazyplaceholder"/>
            </xsl:attribute>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="src">
              <xsl:value-of select="@src"/>
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
       
        <!--<xsl:for-each select="@*[name()!='border' and name()!='align' and name()!='style']">-->
        <xsl:for-each select="@*[name()!='border' and name()!='align' and name()!='src']">

          <xsl:attribute name="{name()}">
            <xsl:choose>

              <!-- ##### @Attribute Conditions ##### -->


              <!-- ##### VALIDATION - Attribute "align" can not be used for this element. ##### -->
              <xsl:when test="name()='class' and (ancestor::img[@align] or contains(ancestor::img/@style,'float: '))">
                <xsl:variable name="align" select="ancestor::img/@align"/>
                <xsl:variable name="float" select="substring-before(substring-after(ancestor::img/@style,'float: '),';')"/>
                <xsl:value-of select="."  />
                <xsl:text> align</xsl:text>
                <xsl:choose>
                  <xsl:when test="@align">
                    <xsl:value-of select="$align"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$float"/>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:if test="$lazy='on'">
                  <xsl:text> lazy</xsl:text>
                </xsl:if>
              </xsl:when>
              <xsl:when test="name()='class'">
                <xsl:value-of select="."  />
                <xsl:if test="$lazy='on'">
                  <xsl:text> lazy</xsl:text>
                </xsl:if>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="."  />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
        </xsl:for-each>

        <!-- ##### VALIDATION - Attribute "align" can not be used for this element. ##### -->
        <xsl:choose>
         <xsl:when test="(not(@class) and (@align or contains(@style,'float: '))) or ancestor::Content[@responsiveImg='true']">
          <xsl:attribute name="class">
            <xsl:variable name="float" select="substring-before(substring-after(@style,'float: '),';')"/>
            <xsl:variable name="align" select="@align"/>
            <xsl:text>align</xsl:text>
            <xsl:choose>
              <xsl:when test="@align">
                <xsl:value-of select="$align"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$float"/>
              </xsl:otherwise>
            </xsl:choose>
            <xsl:if test="ancestor::Content[@responsiveImg='true']">
              <xsl:text> img-responsive</xsl:text>
            </xsl:if>
            <xsl:if test="$lazy='on'">
              <xsl:text> lazy</xsl:text>
            </xsl:if>
          </xsl:attribute>
         </xsl:when>
           <xsl:when test="not(@class) and $lazy='on'">
             <xsl:attribute name="class">
                 <xsl:text>lazy</xsl:text>
             </xsl:attribute>
          </xsl:when>
        </xsl:choose>

        <!-- ##### VALIDATION - required attribute "alt" ##### -->
        <xsl:if test="not(@alt)">
          <xsl:attribute name="alt"></xsl:attribute>
        </xsl:if>

      </xsl:element>
    </xsl:variable>
    <xsl:copy-of select="ms:node-set($img)"/>
  </xsl:template>

  <xsl:template match="img[contains(@class,'replaceFromInstance')]" mode="cleanXhtml">
    <xsl:variable name="replaceClass" select="@replaceClass"/>
    <xsl:variable name="replaceImage" select="(ancestor::Content/model/instance/descendant-or-self::img[@class=$replaceClass])[1]"/>
    <xsl:choose>
      <xsl:when test="$replaceImage/@src!=''">
        <xsl:copy-of select="$replaceImage"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="img">
          <xsl:element name="img">
            <!--<xsl:for-each select="@*[name()!='border' and name()!='align' and name()!='style']">-->
            <xsl:for-each select="@*[name()!='border' and name()!='align']">

              <xsl:attribute name="{name()}">
                <xsl:choose>

                  <!-- ##### @Attribute Conditions ##### -->

                  <xsl:when test="name()='src'">
                    <xsl:choose>
                      <xsl:when test="contains(.,'http://')">
                        <xsl:value-of select="."/>
                      </xsl:when>
                      <xsl:otherwise>
                        <!--<xsl:value-of select="$siteURL"/>-->
                        <xsl:value-of select="."/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:when>

                  <!-- ##### VALIDATION - Attribute "align" can not be used for this element. ##### -->
                  <xsl:when test="name()='class' and (ancestor::img[@align] or contains(ancestor::img/@style,'float: '))">
                    <xsl:variable name="align" select="ancestor::img/@align"/>
                    <xsl:variable name="float" select="substring-before(substring-after(ancestor::img/@style,'float: '),';')"/>
                    <xsl:value-of select="."  />
                    <xsl:text> align</xsl:text>
                    <xsl:choose>
                      <xsl:when test="@align">
                        <xsl:value-of select="$align"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="$float"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="."  />
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:for-each>

            <!-- ##### VALIDATION - Attribute "align" can not be used for this element. ##### -->
            <xsl:if test="not(@class) and (@align or contains(@style,'float: '))">
              <xsl:attribute name="class">
                <xsl:variable name="float" select="substring-before(substring-after(@style,'float: '),';')"/>
                <xsl:variable name="align" select="@align"/>
                <xsl:text>align</xsl:text>
                <xsl:choose>
                  <xsl:when test="@align">
                    <xsl:value-of select="$align"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$float"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>

            <!-- ##### VALIDATION - required attribute "alt" ##### -->
            <xsl:if test="not(@alt)">
              <xsl:attribute name="alt"></xsl:attribute>
            </xsl:if>

          </xsl:element>
        </xsl:variable>
        <xsl:copy-of select="ms:node-set($img)"/>

      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>



  <!-- IMAGE PROCESSING  -->
  <xsl:template match="img[contains(@class,'mceItem')]" mode="cleanXhtml">
    <iframe title="YouTube video player" width="400" height="225" src="http://www.youtube.com/embed/{@alt}" frameborder="0" allowfullscreen="true" style="{@style}">
      <xsl:text> </xsl:text>
    </iframe>
  </xsl:template>


  <xsl:template match="a" mode="cleanXhtml">

    <xsl:element name="{name()}">

      <xsl:for-each select="@*">
        <xsl:attribute name="{name()}">

          <xsl:choose>
            <!-- when we doing the href attribute AND contains pgid -> get proper URL -->
            <xsl:when test="name()='href' and contains(.,'?pgid=')">

              <xsl:call-template name="getHrefFromPgid">
                <xsl:with-param name="url" select="."/>
              </xsl:call-template>

            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="." />
            </xsl:otherwise>
          </xsl:choose>

        </xsl:attribute>
      </xsl:for-each>

      <xsl:apply-templates mode="cleanXhtml"/>

    </xsl:element>
  </xsl:template>


  <xsl:template match="br" mode="cleanXhtml">
    <br/>
  </xsl:template>

  <xsl:template match="hr" mode="cleanXhtml">
    <hr/>
  </xsl:template>

  <!-- Ensure no Empty strong tags-->
  <xsl:template match="strong" mode="cleanXhtml">

    <xsl:element name="{name()}">
      <xsl:for-each select="@*">
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="cleanXhtml"/>
      <xsl:text> </xsl:text>
    </xsl:element>

  </xsl:template>

  <!-- Ensure no Empty bold tags-->
  <xsl:template match="b" mode="cleanXhtml">

    <xsl:element name="{name()}">
      <xsl:for-each select="@*">
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="cleanXhtml"/>
      <xsl:text> </xsl:text>
    </xsl:element>

  </xsl:template>

  <!-- Ensure no Self Closing P and Span and i and em tags-->
  <xsl:template match="p | span | i | em" mode="cleanXhtml">

    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="cleanXhtml"/>
      <xsl:text> </xsl:text>
    </xsl:element>
  </xsl:template>



  <xsl:template match="iframe" mode="cleanXhtml">

    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="cleanXhtml"/>
      <xsl:text> </xsl:text>
    </xsl:element>
  </xsl:template>

  <!-- Special instance link to allow skype links -->
  <xsl:template match="a[@class='skypelink']" mode="cleanXhtml">
    <a href="skype:{.}?call" tile="{@title}">
      <img src="http://mystatus.skype.com/smallclassic/{.}" border="0" alt="Skype Me ! - My Current Skype Status" title="Skype Me ! - My Current Skype Status" />
    </a>
  </xsl:template>

  <xsl:template name="getHrefFromPgid">
    <xsl:param name="url" />
    <xsl:variable name="pageId" select="substring-after($url,'?pgid=')"/>

    <xsl:choose>

      <!-- when its a number - we can get the page -->
      <xsl:when test="number($pageId)!='NaN'">
        <xsl:variable name="pageHref">
          <xsl:apply-templates select="$page/Menu/descendant-or-self::MenuItem[@id=$pageId]" mode="getHref" />
        </xsl:variable>
        <xsl:choose>

          <!-- if page not found because isn't live or no access - just return URL -->
          <xsl:when test="$pageHref=''">
            <xsl:value-of select="$url"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$pageHref"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>

      <!-- otherwise for now just return url, we can extend this to pick up other values later e.g. artid -->
      <xsl:otherwise>
        <xsl:value-of select="$url"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>



  <xsl:template match="*" mode="cleanXhtml-escape-js">
    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:attribute name="{name()}">
          <xsl:call-template name="escape-js">
            <xsl:with-param name="string">
              <xsl:value-of select="." />
            </xsl:with-param>
          </xsl:call-template>
        </xsl:attribute>
      </xsl:for-each>
      <xsl:choose>
        <xsl:when test="*">
          <xsl:apply-templates select="*" mode="cleanXhtml-escape-js"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="escape-js">
            <xsl:with-param name="string">
              <xsl:value-of select="." />
            </xsl:with-param>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:element>
  </xsl:template>

  <!-- ## ENCODE XHTML  ##########################################################################   -->
  <!--    Same purpose as cleanXhtml - But incodes the tags - handy for RSS Feeds or placing
          xHTML in attributes for JS operations e.g. jQuery tooltip.
  -->


  <xsl:template match="*" mode="encodeXhtml">
    <xsl:text>&lt;</xsl:text>
    <xsl:value-of select="name()"/>
    <xsl:for-each select="@*[name()!='xmlns']">
      <xsl:text>&#160;</xsl:text>
      <xsl:value-of select="name()"/>
      <xsl:text>="</xsl:text>
      <xsl:call-template name="escape-js">
        <xsl:with-param name="string">
          <xsl:value-of select="." />
        </xsl:with-param>
      </xsl:call-template>
      <xsl:text>"</xsl:text>
    </xsl:for-each>
    <xsl:text>&gt;</xsl:text>
    <xsl:choose>
      <xsl:when test="*">
        <xsl:apply-templates select="*" mode="encodeXhtml"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="escape-js">
          <xsl:with-param name="string">
            <xsl:value-of select="." />
          </xsl:with-param>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>

    <xsl:text>&lt;/</xsl:text>
    <xsl:value-of select="name()"/>
    <xsl:text>&gt;</xsl:text>
  </xsl:template>

  <xsl:template match="img | *[name()='img']" mode="encodeXhtml">
    <xsl:text>&lt;</xsl:text>
    <xsl:value-of select="name()"/>
    <xsl:for-each select="@*[name()!='xmlns']">
      <xsl:text>&#160;</xsl:text>
      <xsl:value-of select="name()"/>
      <xsl:text>="</xsl:text>
      <xsl:value-of select="." />
      <xsl:text>"</xsl:text>
    </xsl:for-each>
    <xsl:text>/&gt;</xsl:text>
  </xsl:template>

  <xsl:template match="br" mode="encodeXhtml">
    <xsl:text>&lt;br/&gt;</xsl:text>
  </xsl:template>

  <xsl:template match="hr" mode="encodeXhtml">
    <xsl:text>&lt;hr/&gt;</xsl:text>
  </xsl:template>



  <!-- ## FLATTEN XHTML  #######################################################   -->
  <!--    Sometimes, the data is stored in a Formatted Text field with xHTML,
          However when you are outputting you want want a plain text string.
          These templates run through an xHTML nodeset() but keeping basic inline styles.
          Ignore some tags like img, br, hr that don't contain plaintext.
  -->
  <xsl:template match="*" mode="flattenXhtml">
    <xsl:apply-templates mode="flattenXhtml"/>
  </xsl:template>

  <xsl:template match="*[node()='&#160;' or node()=' ']" mode="flattenXhtml"></xsl:template>

  <!-- keep inline styles -->
  <xsl:template match="strong | b | i | s | u | span" mode="flattenXhtml">
    <!--<xsl:element name="{name()}">-->
    <xsl:apply-templates mode="flattenXhtml"/>
    <!--</xsl:element>-->
  </xsl:template>

  <!-- seperate p's -->
  <xsl:template match="p" mode="flattenXhtml">
    <!--  get contents and put in variable -->
    <xsl:variable name="inside">
      <xsl:apply-templates mode="flattenXhtml"/>
    </xsl:variable>
    <xsl:value-of select="$inside" />
    <!-- if last character isn't '.' put it in. -->
    <xsl:if test="substring($inside,string-length($inside))!='.'">
      <xsl:text>.</xsl:text>
    </xsl:if>
    <xsl:text> </xsl:text>
  </xsl:template>

  <!-- keep emphasis for h's -->
  <xsl:template match="h1 | h2 | h3 | h4| h5 | h6" mode="flattenXhtml">
    <!--  get contents and put in variable -->
    <xsl:variable name="inside">
      <xsl:apply-templates mode="flattenXhtml"/>
    </xsl:variable>
    <xsl:value-of select="$inside" />
    <!-- if last character isn't ':' put it in -->
    <xsl:if test="substring($inside,string-length($inside))!=':'">
      <xsl:text>:</xsl:text>
    </xsl:if>
    <xsl:text> </xsl:text>
  </xsl:template>

  <!-- Turn tables into CSV's -->
  <xsl:template match="td | th" mode="flattenXhtml">
    <xsl:apply-templates mode="flattenXhtml"/>
    <xsl:text>, </xsl:text>
  </xsl:template>

  <!-- li's -->
  <xsl:template match="li[parent::ul]" mode="flattenXhtml">
    <xsl:text>&#8226; </xsl:text>
    <xsl:apply-templates mode="flattenXhtml"/>
    <xsl:text>&#160;&#160;</xsl:text>
  </xsl:template>

  <xsl:template match="li[parent::ol]" mode="flattenXhtml">
    <xsl:value-of select="position()"/>
    <xsl:text>. </xsl:text>
    <xsl:apply-templates mode="flattenXhtml"/>
    <xsl:text>&#160;&#160;</xsl:text>
  </xsl:template>

  <xsl:template match="img | br | hr" mode="flattenXhtml">
    <xsl:text> </xsl:text>
  </xsl:template>








  <!-- ## Social Networking Bookmarks  #######################################################   -->

  <xsl:template match="/" mode="socialBookmarks">
    <!-- Currently Image sizes are only 16 and 32 -->
    <xsl:param name="size"/>

    <xsl:variable name="linkURL">
      <xsl:call-template name="getXmlSettings">
        <xsl:with-param name="sectionName" select="'web'"/>
        <xsl:with-param name="valueName" select="'BaseUrl'"/>
      </xsl:call-template>
      <xsl:choose>
        <xsl:when test="/Page/ContentDetail">
          <xsl:apply-templates select="/Page/ContentDetail/Content" mode="getHref"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="$currentPage" mode="getHref"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="linkTitle">
      <xsl:choose>
        <xsl:when test="/Page/ContentDetail">
          <xsl:value-of select="/Page/ContentDetail/Content/@name"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$currentPage/@name"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>


    <!-- FACEBOOK -->
    <a href="http://www.facebook.com/sharer.php?u={$linkURL}" title="Post on Facebook" rel="external">
      <img src="/ewcommon/images/icons/socialnetworking/facebook_{$size}.png" width="{$size}" height="{$size}" alt="Facebook"/>
      <span class="hidden">Facebook | </span>
    </a>
    <!-- Twitter -->
    <a href="http://twitter.com/home?status={$linkTitle} - {$linkURL}" title="Post to Twitter" rel="external">
      <img src="/ewcommon/images/icons/socialnetworking/twitter_{$size}.png" width="{$size}" height="{$size}" alt="Twitter"/>
      <span class="hidden">Twitter | </span>
    </a>
    <!-- DELICIOUS -->
    <a href="http://del.icio.us/post?url={$linkURL}&amp;title={$linkTitle}" title="Post to Delicious" rel="external">
      <img src="/ewcommon/images/icons/socialnetworking/delicious_{$size}.png" width="{$size}" height="{$size}" alt="Delicious"/>
      <span class="hidden">Del.icio.us | </span>
    </a>
    <!-- DIGG IT -->
    <a href="http://digg.com/submit?url={$linkURL}&amp;title={$linkTitle}" title="Dig this" rel="external">
      <img src="/ewcommon/images/icons/socialnetworking/digg_{$size}.png" width="{$size}" height="{$size}" alt="Dig"/>
      <span class="hidden">Digg | </span>
    </a>
    <!-- REDDIT -->
    <a href="http://reddit.com/submit?url={$linkURL}&amp;title={$linkTitle}" title="Post to Reddit" rel="external">
      <img src="/ewcommon/images/icons/socialnetworking/reddit_{$size}.png" width="{$size}" height="{$size}" alt="Reddit"/>
      <span class="hidden">Reddit | </span>
    </a>
    <!-- STUMBLE UPON -->
    <a href="http://www.stumbleupon.com/submit?url={$linkURL}&amp;title={$linkTitle}" title="Post to StumbleUpon" rel="external">
      <img src="/ewcommon/images/icons/socialnetworking/stumbleupon_{$size}.png" width="{$size}" height="{$size}" alt="StumbleUpon"/>
      <span class="hidden">StumbleUpon | </span>
    </a>

  </xsl:template>


  <!-- ## MODULE TITLE AND LINKS  #######################################################   -->
  <!--    Handles The Module Title and Module links 
  -->
  <xsl:template match="Content[@type='Module']" mode="moduleLink">
    <xsl:variable name="pageId" select="@link"/>
    <xsl:choose>
      <xsl:when test="@link!=''">
        <a>
          <xsl:attribute name="href">
            <xsl:choose>
              <xsl:when test="format-number(@link,'0')!='NaN'">
                <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId or PageVersion[@vParId=$pageId]]" mode="getHref"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="@link"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <xsl:attribute name="title">
            <xsl:choose>
              <xsl:when test="format-number(@link,'0')!='NaN'">
                <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId or PageVersion[@vParId=$pageId]]" mode="getDisplayName" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:choose>
                  <xsl:when test="@linkText and @linkText!=''">
                    <xsl:value-of select="@linkText"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="@title"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <xsl:if test="@linkType='external'">
            <xsl:attribute name="rel">external</xsl:attribute>
          </xsl:if>
          <xsl:apply-templates select="." mode="moduleTitle"/>
        </a>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="." mode="moduleTitle"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="Content[@type='Module']" mode="moduleTitle">

    <xsl:variable name="title">
      <span>
        <xsl:value-of select="@title"/>
        <xsl:text> </xsl:text>
      </span>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="@iconStyle='Centre'">

        <div class="center-block center-large">

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
      <xsl:when test="@iconStyle='CentreSmall'">

        <div class="center-block center-small">

          <xsl:if test="@icon!=''">
            <i>
              <xsl:attribute name="class">
                <xsl:text>fa center-block </xsl:text>
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
      <xsl:when test="@iconStyle='Right'">

        <div class="title-align-right">

          <xsl:if test="@icon!=''">
            <i>
              <xsl:attribute name="class">
                <xsl:text>fa </xsl:text>
                <xsl:value-of select="@icon"/>
              </xsl:attribute>
              <xsl:text> </xsl:text>
            </i>
            <xsl:text> </xsl:text>
          </xsl:if>
          <xsl:if test="@uploadIcon!='' and @uploadIcon!='_'">
            <span class="upload-icon">
              <img src="{@uploadIcon}" alt="icon" class="img-responsive"/>
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

  <!-- ## GENERIC CONTENT IMAGERY HANDLING  #######################################################   -->
  <!--    Handles and resizes all the imagery for all Content types.
          Thumbnail, Details and Fullsizes.
  -->
  <xsl:template match="Content | MenuItem | Discount | productDetail" mode="displayThumbnail">
    <xsl:param name="crop" select="false()" />
    <xsl:param name="no-stretch" select="true()" />
    <xsl:param name="width"/>
    <xsl:param name="height"/>
    <xsl:param name="forceResize"/>
    <xsl:param name="class"/>
    <xsl:param name="style"/>
    <!-- IF SO THAT we don't get empty tags if NO IMAGE -->
    <xsl:if test="Images/img[@src and @src!='']">
      <!-- SRC VALUE -->
      <xsl:variable name="src">
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
      </xsl:variable>
      <!-- ALT VALUE -->
      <xsl:variable name="alt">
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
      </xsl:variable>
      <xsl:variable name="max-width">
        <xsl:choose>
          <xsl:when test="$width!=''">
            <xsl:value-of select="$width"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="." mode="getThWidth"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="max-height">
        <xsl:choose>
          <xsl:when test="$height!=''">
            <xsl:value-of select="$height"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="." mode="getThHeight"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="cropvar">
        <xsl:choose>
          <xsl:when test="$crop='true'">
            <xsl:value-of select="true()"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="." mode="getThCrop"/>
          </xsl:otherwise>
        </xsl:choose>

      </xsl:variable>


      <!-- IF Image to resize -->
      <xsl:if test="$src!=''">
        <xsl:variable name="newSrc">
          <xsl:call-template name="resize-image">
            <xsl:with-param name="path" select="$src"/>
            <xsl:with-param name="max-width" select="$max-width"/>
            <xsl:with-param name="max-height" select="$max-height"/>
            <xsl:with-param name="file-prefix">
              <xsl:text>~th-</xsl:text>
              <xsl:value-of select="$max-width"/>
              <xsl:text>x</xsl:text>
              <xsl:value-of select="$max-height"/>
              <xsl:text>/~th-</xsl:text>
              <xsl:if test="$cropvar='true'">
                <xsl:text>crop-</xsl:text>
              </xsl:if>
              <xsl:if test="not($no-stretch)">
                <xsl:text>strch-</xsl:text>
              </xsl:if>
            </xsl:with-param>
            <xsl:with-param name="file-suffix" select="''"/>
            <xsl:with-param name="quality" select="100"/>
            <xsl:with-param name="crop" select="$cropvar" />
            <xsl:with-param name="no-stretch" select="$no-stretch" />
            <xsl:with-param name="forceResize" select="$forceResize" />
          </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="imageSize" select="ew:ImageSize($newSrc)"/>
        
        <xsl:variable name="image">
          <img itemprop="image">
            <!-- SRC -->
            <xsl:choose>
              <xsl:when test="$lazy='on'">
                <xsl:attribute name="data-src">
                  <xsl:value-of select="$newSrc"/>
                </xsl:attribute>
                <xsl:attribute name="src">
                  <xsl:value-of select="$lazyplaceholder"/>
                </xsl:attribute>
              </xsl:when>
              <xsl:otherwise>
                <xsl:attribute name="src">
                  <xsl:value-of select="$newSrc"/>
                </xsl:attribute>
              </xsl:otherwise>
            </xsl:choose>
            <!-- Width -->
            <xsl:attribute name="width">
              <xsl:choose>
                <xsl:when test="contains($newSrc,'awaiting-image-thumbnail.gif')">
                  <xsl:value-of select="$max-width"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="substring-before($imageSize,'x')" />
                </xsl:otherwise>
              </xsl:choose>

            </xsl:attribute>
            <!-- Height -->
            <xsl:attribute name="height">
              <xsl:choose>
                <xsl:when test="contains($newSrc,'awaiting-image-thumbnail.gif')">
                  <xsl:value-of select="$max-height"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="substring-after($imageSize,'x')" />
                </xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
            <!-- Alt -->
            <xsl:attribute name="alt">
              <xsl:value-of select="$alt" />
            </xsl:attribute>
            <!-- Title -->
            <xsl:attribute name="title">
              <xsl:value-of select="$alt" />
            </xsl:attribute>
            <!-- Class -->
            <xsl:attribute name="class">
              <xsl:choose>
                <xsl:when test="$class!=''">
                  <xsl:value-of select="$class" />
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>photo thumbnail resized</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
              <xsl:if test="$lazy='on'">
                <xsl:text> lazy</xsl:text>
              </xsl:if>
            </xsl:attribute>
            <xsl:if test="$style!=''">
              <xsl:attribute name="style">
                <xsl:value-of select="$style" />
              </xsl:attribute>
            </xsl:if>
          </img>
        </xsl:variable>
        <xsl:copy-of select="ms:node-set($image)/*" />
      </xsl:if>
    </xsl:if>

  </xsl:template>

  <xsl:template match="Content | MenuItem | Discount | productDetail" mode="getThCrop">
    <xsl:value-of select="false()"/>
  </xsl:template>

  <xsl:template match="Content[@type='Document' or @type='Review']" mode="displayThumbnail">
    <xsl:param name="crop" select="false()" />
    <xsl:param name="no-stretch" select="true()" />
    <xsl:param name="width"/>
    <xsl:param name="height"/>
    <xsl:param name="forceResize"/>

    <!-- SRC VALUE -->
    <xsl:variable name="src">
      <xsl:value-of select="Path/node()"/>
    </xsl:variable>
    <!-- ALT VALUE -->
    <xsl:variable name="alt">
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
    </xsl:variable>
    <xsl:variable name="max-width">
      <xsl:choose>
        <xsl:when test="$width!=''">
          <xsl:value-of select="$width"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="." mode="getThWidth"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="max-height">
      <xsl:choose>
        <xsl:when test="$height!=''">
          <xsl:value-of select="$height"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="." mode="getThHeight"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- IF Image to resize -->
    <xsl:if test="$src!=''">
      <xsl:variable name="newSrc">
        <xsl:call-template name="resize-image">
          <xsl:with-param name="path" select="$src"/>
          <xsl:with-param name="max-width" select="$max-width"/>
          <xsl:with-param name="max-height" select="$max-height"/>
          <xsl:with-param name="file-prefix">
            <xsl:text>~th-</xsl:text>
            <xsl:value-of select="$max-width"/>
            <xsl:text>x</xsl:text>
            <xsl:value-of select="$max-height"/>
            <xsl:text>/~th-</xsl:text>
            <xsl:if test="$crop">
              <xsl:text>crop-</xsl:text>
            </xsl:if>
            <xsl:if test="not($no-stretch)">
              <xsl:text>strch-</xsl:text>
            </xsl:if>
          </xsl:with-param>
          <xsl:with-param name="file-suffix" select="''"/>
          <xsl:with-param name="quality" select="100"/>
          <xsl:with-param name="crop" select="$crop" />
          <xsl:with-param name="no-stretch" select="$no-stretch" />
          <xsl:with-param name="forceResize" select="$forceResize" />
        </xsl:call-template>
      </xsl:variable>
      <xsl:variable name="imageSize" select="ew:ImageSize($newSrc)"/>
      <xsl:variable name="image">
        <img>
          <!-- SRC -->
          <xsl:attribute name="src">
            <xsl:value-of select="$newSrc"/>
          </xsl:attribute>
          <!-- Width -->
          <xsl:attribute name="width">
            <xsl:choose>
              <xsl:when test="contains($newSrc,'awaiting-image-thumbnail.gif')">
                <xsl:value-of select="$max-width"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="substring-before($imageSize,'x')" />
              </xsl:otherwise>
            </xsl:choose>
            
          </xsl:attribute>
          <!-- Height -->
          <xsl:attribute name="height">
            <xsl:choose>
              <xsl:when test="contains($newSrc,'awaiting-image-thumbnail.gif')">
                <xsl:value-of select="$max-height"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="substring-after($imageSize,'x')" />
              </xsl:otherwise>
            </xsl:choose>
            
          </xsl:attribute>
          <!-- Alt -->
          <xsl:attribute name="alt">
            <xsl:value-of select="$alt" />
          </xsl:attribute>
          <!-- Title -->
          <xsl:attribute name="title">
            <xsl:value-of select="$alt" />
          </xsl:attribute>
          <!-- Class -->
          <xsl:attribute name="class">
            <xsl:text>photo thumbnail resized</xsl:text>
          </xsl:attribute>
        </img>
      </xsl:variable>
      <xsl:copy-of select="ms:node-set($image)/*" />
    </xsl:if>
  </xsl:template>


  <xsl:template match="Content | MenuItem | Company | Item" mode="displayDetailImage">
    <xsl:param name="crop" select="false()" />
    <xsl:param name="no-stretch" select="true()" />
    <xsl:param name="showImage"/>
    <xsl:param name="class"/>
    <xsl:param name="forceResize"/>
    <xsl:variable name="VForceResize">
      <xsl:choose>
        <xsl:when test="$forceResize='false'">false</xsl:when>
        <xsl:otherwise>true</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="imgId">
      <xsl:text>picture_</xsl:text>
      <xsl:value-of select="@id"/>
    </xsl:variable>
    <!-- Needed to create unique grouping for lightbox -->
    <xsl:variable name="parId">
      <xsl:text>group</xsl:text>
      <xsl:value-of select="@type"/>
    </xsl:variable>

    <!-- SRC VALUE -->
    <xsl:variable name="src">
      <xsl:choose>
        <!-- IF use display -->
        <xsl:when test="Images/img[@class='display']/@src and Images/img[@class='display']/@src!=''">
          <xsl:value-of select="Images/img[@class='display']/@src"/>
        </xsl:when>
        <!-- Else Full Size use that -->
        <xsl:when test="Images/img[@class='detail']/@src and Images/img[@class='detail']/@src!=''">
          <xsl:value-of select="Images/img[@class='detail']/@src"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <!-- ALT VALUE -->
    <xsl:variable name="alt">
      <xsl:choose>
        <!-- IF Full Size use that -->
        <xsl:when test="Images/img[@class='detail']/@alt and Images/img[@class='detail']/@alt!=''">
          <xsl:value-of select="Images/img[@class='detail']/@alt"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="." mode="getDisplayName"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="max-width">
      <xsl:apply-templates select="." mode="getDisplayWidth"/>
    </xsl:variable>
    <xsl:variable name="max-height">
      <xsl:apply-templates select="." mode="getDisplayHeight"/>
    </xsl:variable>
    <xsl:variable name="lg-max-width">
      <xsl:apply-templates select="." mode="getFullSizeWidth"/>
    </xsl:variable>
    <xsl:variable name="lg-max-height">
      <xsl:apply-templates select="." mode="getFullSizeHeight"/>
    </xsl:variable>

    <!-- IF Image to resize -->
    <xsl:choose>
      <xsl:when test="$src!=''">
        <xsl:variable name="displaySrc">
          <xsl:call-template name="resize-image">
            <xsl:with-param name="path" select="$src"/>
            <xsl:with-param name="max-width" select="$max-width"/>
            <xsl:with-param name="max-height" select="$max-height"/>
            <xsl:with-param name="file-prefix">
              <xsl:text>~dis-</xsl:text>
              <xsl:value-of select="$max-width"/>
              <xsl:text>x</xsl:text>
              <xsl:value-of select="$max-height"/>
              <xsl:text>/~dis-</xsl:text>
              <xsl:if test="$crop">
                <xsl:text>crop-</xsl:text>
              </xsl:if>
              <xsl:if test="not($no-stretch)">
                <xsl:text>strch-</xsl:text>
              </xsl:if>
            </xsl:with-param>
            <xsl:with-param name="file-suffix" select="''"/>
            <xsl:with-param name="quality" select="100"/>
            <xsl:with-param name="crop" select="$crop"/>
            <xsl:with-param name="no-stretch" select="$no-stretch" />
            <xsl:with-param name="forceResize" select="$VForceResize" />
          </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="detailSrc">
          <xsl:call-template name="resize-image">
            <xsl:with-param name="path" select="$src"/>
            <xsl:with-param name="max-width" select="$lg-max-width"/>
            <xsl:with-param name="max-height" select="$lg-max-height"/>
            <xsl:with-param name="file-prefix">
              <xsl:text>~lg-</xsl:text>
              <xsl:value-of select="$lg-max-width"/>
              <xsl:text>x</xsl:text>
              <xsl:value-of select="$lg-max-width"/>
              <xsl:text>/~lg-</xsl:text>
              <xsl:if test="$crop">
                <xsl:text>crop-</xsl:text>
              </xsl:if>
              <xsl:if test="not($no-stretch)">
                <xsl:text>strch-</xsl:text>
              </xsl:if>
            </xsl:with-param>
            <xsl:with-param name="file-suffix" select="''"/>
            <xsl:with-param name="quality" select="100"/>
            <xsl:with-param name="crop" select="$crop"/>
            <xsl:with-param name="no-stretch" select="$no-stretch" />
            <xsl:with-param name="forceResize" select="$VForceResize" />
          </xsl:call-template>
        </xsl:variable>

        <xsl:choose>
          <xsl:when test="$detailSrc!=''">
            <xsl:choose>
              <xsl:when test="$page/@cssFramework='bs3'">
                <span class="picture {$class}">
                  <xsl:if test="$showImage = 'noshow'">
                    <xsl:attribute name="class">
                      <xsl:text>picture hidden</xsl:text>
                    </xsl:attribute>
                  </xsl:if>
                  <a href="{$detailSrc}" class="responsive-lightbox">
                    <xsl:variable name="newimageSize" select="ew:ImageSize($displaySrc)"/>
                    <xsl:variable name="newimageWidth" select="substring-before($newimageSize,'x')"/>
                    <xsl:variable name="newimageHeight" select="substring-after($newimageSize,'x')"/>
                    <img src="{$displaySrc}" width="{$newimageWidth}" height="{$newimageHeight}" alt="{$alt}" itemprop="image">
                      <xsl:if test="$imgId != ''">
                        <xsl:attribute name="id">
                          <xsl:value-of select="$imgId"/>
                        </xsl:attribute>
                      </xsl:if>
                    </img>
                    <div class="text-muted">
                      <xsl:apply-templates select="self::Content" mode="imageEnlarge"/>
                    </div>
                  </a>
                </span>
              </xsl:when>
              <xsl:otherwise>
                <span class="picture {$class}">
                  <xsl:if test="$showImage = 'noshow'">
                    <xsl:attribute name="class">
                      <xsl:text>picture hidden</xsl:text>
                    </xsl:attribute>
                  </xsl:if>
                  <a href="{$detailSrc}" class="lightbox" rel="lightbox">
                    <xsl:if test="$parId != ''">
                      <xsl:attribute name="rel">
                        <xsl:text>lightbox[</xsl:text>
                        <xsl:value-of select="$parId"/>
                        <xsl:text>]</xsl:text>
                      </xsl:attribute>
                    </xsl:if>
                    <xsl:attribute name="title">
                      <xsl:value-of select="$alt"/>
                    </xsl:attribute>
                    <xsl:variable name="newimageSize" select="ew:ImageSize($displaySrc)"/>
                    <xsl:variable name="newimageWidth" select="substring-before($newimageSize,'x')"/>
                    <xsl:variable name="newimageHeight" select="substring-after($newimageSize,'x')"/>
                    <img src="{$displaySrc}" width="{$newimageWidth}" height="{$newimageHeight}" alt="{$alt}" class="detail photo" itemprop="image">
                      <xsl:if test="$imgId != ''">
                        <xsl:attribute name="id">
                          <xsl:value-of select="$imgId"/>
                        </xsl:attribute>
                      </xsl:if>
                    </img>
                    <span class="imageEnlarge">
                      <xsl:apply-templates select="self::Content" mode="imageEnlarge"/>
                    </span>
                  </a>
                </span>

              </xsl:otherwise>

            </xsl:choose>

          </xsl:when>
          <xsl:otherwise>
            <xsl:variable name="newimageSize" select="ew:ImageSize($displaySrc)"/>
            <xsl:variable name="newimageWidth" select="substring-before($newimageSize,'x')"/>
            <xsl:variable name="newimageHeight" select="substring-after($newimageSize,'x')"/>
            <img src="{$displaySrc}" width="{$newimageWidth}" height="{$newimageHeight}" alt="{$alt}" class="detail photo" id="{$imgId}" itemprop="image"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <!--<xsl:otherwise>
        -->
      <!-- add awaiting image graphic -->
      <!--
        <span class="picture">
          <xsl:if test="$showImage = 'noshow'">
            <xsl:attribute name="class">
              <xsl:text>picture hidden</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <img src="/ewcommon/images/awaiting-image-picture.gif" width="400" height="300" alt="Awaiting Image" class="detail" id="{$imgId}"/>
        </span>
      </xsl:otherwise>-->
    </xsl:choose>
  </xsl:template>

  <xsl:template match="Content | MenuItem" mode="displayDetailImageOnly">
    <xsl:param name="crop" select="false()" />
    <xsl:param name="no-stretch" select="true()" />
    <xsl:param name="showImage"/>
    <xsl:param name="class"/>
    <xsl:param name="forceResize"/>
    <xsl:variable name="VForceResize">
      <xsl:choose>
        <xsl:when test="$forceResize='false'">false</xsl:when>
        <xsl:otherwise>true</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="imgId">
      <xsl:text>picture_</xsl:text>
      <xsl:value-of select="@id"/>
    </xsl:variable>
    <!-- Needed to create unique grouping for lightbox -->
    <xsl:variable name="parId">
      <xsl:text>group</xsl:text>
      <xsl:value-of select="@type"/>
    </xsl:variable>

    <!-- SRC VALUE -->
    <xsl:variable name="src">
      <xsl:choose>
        <!-- IF use display -->
        <xsl:when test="Images/img[@class='display']/@src and Images/img[@class='display']/@src!=''">
          <xsl:value-of select="Images/img[@class='display']/@src"/>
        </xsl:when>
        <!-- Else Full Size use that -->
        <xsl:when test="Images/img[@class='detail']/@src and Images/img[@class='detail']/@src!=''">
          <xsl:value-of select="Images/img[@class='detail']/@src"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <!-- ALT VALUE -->
    <xsl:variable name="alt">
      <xsl:choose>
        <!-- IF Full Size use that -->
        <xsl:when test="Images/img[@class='detail']/@alt and Images/img[@class='detail']/@alt!=''">
          <xsl:value-of select="Images/img[@class='detail']/@alt"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="." mode="getDisplayName"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="max-width">
      <xsl:apply-templates select="." mode="getDisplayWidth"/>
    </xsl:variable>
    <xsl:variable name="max-height">
      <xsl:apply-templates select="." mode="getDisplayHeight"/>
    </xsl:variable>
    <xsl:variable name="lg-max-width">
      <xsl:apply-templates select="." mode="getFullSizeWidth"/>
    </xsl:variable>
    <xsl:variable name="lg-max-height">
      <xsl:apply-templates select="." mode="getFullSizeHeight"/>
    </xsl:variable>
    <!-- IF Image to resize -->
    <xsl:choose>
      <xsl:when test="$src!=''">
        <xsl:variable name="displaySrc">
          <xsl:call-template name="resize-image">
            <xsl:with-param name="path" select="$src"/>
            <xsl:with-param name="max-width" select="$max-width"/>
            <xsl:with-param name="max-height" select="$max-height"/>
            <xsl:with-param name="file-prefix">
              <xsl:text>~dis-</xsl:text>
              <xsl:value-of select="$max-width"/>
              <xsl:text>x</xsl:text>
              <xsl:value-of select="$max-height"/>
              <xsl:text>/~dis-</xsl:text>
              <xsl:if test="$crop">
                <xsl:text>crop-</xsl:text>
              </xsl:if>
              <xsl:if test="not($no-stretch)">
                <xsl:text>strch-</xsl:text>
              </xsl:if>
            </xsl:with-param>
            <xsl:with-param name="file-suffix" select="''"/>
            <xsl:with-param name="quality" select="100"/>
            <xsl:with-param name="crop" select="$crop"/>
            <xsl:with-param name="no-stretch" select="$no-stretch" />
            <xsl:with-param name="forceResize" select="$VForceResize" />
          </xsl:call-template>
        </xsl:variable>

        <xsl:variable name="newimageSize" select="ew:ImageSize($displaySrc)"/>
        <xsl:variable name="newimageWidth" select="substring-before($newimageSize,'x')"/>
        <xsl:variable name="newimageHeight" select="substring-after($newimageSize,'x')"/>
        <img src="{$displaySrc}" width="{$newimageWidth}" height="{$newimageHeight}" alt="{$alt}" class="detail photo">
          <xsl:if test="$imgId != ''">
            <xsl:attribute name="id">
              <xsl:value-of select="$imgId"/>
            </xsl:attribute>
          </xsl:if>
        </img>
      </xsl:when>
      <!--<xsl:otherwise>
        -->
      <!-- add awaiting image graphic -->
      <!--
        <span class="picture">
          <xsl:if test="$showImage = 'noshow'">
            <xsl:attribute name="class">
              <xsl:text>picture hidden</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <img src="/ewcommon/images/awaiting-image-picture.gif" width="400" height="300" alt="Awaiting Image" class="detail" id="{$imgId}"/>
        </span>
      </xsl:otherwise>-->
    </xsl:choose>
  </xsl:template>

  <xsl:template match="Content | MenuItem" mode="displaySubPageThumb">
    <!-- SRC VALUE -->
    <xsl:variable name="src">
      <xsl:value-of select="Images/img[@class='thumbnail']/@src"/>
    </xsl:variable>
    <!-- ALT VALUE -->
    <xsl:variable name="alt">
      <xsl:value-of select="Images/img[@class='thumbnail']/@alt"/>
    </xsl:variable>
    <xsl:variable name="max-width">
      <xsl:apply-templates select="." mode="getsubThWidth"/>
    </xsl:variable>
    <xsl:variable name="max-height">
      <xsl:apply-templates select="." mode="getsubThHeight"/>
    </xsl:variable>

    <!-- IF Image to resize -->
    <xsl:if test="$src!=''">
      <xsl:variable name="newSrc">
        <xsl:call-template name="resize-image">
          <xsl:with-param name="path" select="$src"/>
          <xsl:with-param name="max-width" select="$max-width"/>
          <xsl:with-param name="max-height" select="$max-height"/>
          <xsl:with-param name="file-prefix">
            <xsl:text>~th-</xsl:text>
            <xsl:value-of select="$max-width"/>
            <xsl:text>x</xsl:text>
            <xsl:value-of select="$max-height"/>
            <xsl:text>/~th-</xsl:text>
          </xsl:with-param>
          <xsl:with-param name="file-suffix" select="''"/>
          <xsl:with-param name="quality" select="100"/>
          <xsl:with-param name="crop" select="false()"/>
          <xsl:with-param name="no-stretch" select="true()"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:variable name="newimageSize" select="ew:ImageSize($newSrc)"/>
      <xsl:variable name="newimageWidth" select="substring-before($newimageSize,'x')"/>
      <xsl:variable name="newimageHeight" select="substring-after($newimageSize,'x')"/>
      <img src="{$newSrc}" width="{$newimageWidth}" height="{$newimageHeight}" alt="{$alt}" class="photo thumbnail 3333"/>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Content[@type='Link']" mode="displayListGridThumb">
    <xsl:variable name="Url">
      <xsl:value-of select="number(Url)"/>
    </xsl:variable>
    <!-- SRC VALUE -->
    <xsl:variable name="src">
      <xsl:choose>
        <xsl:when test="Images/img[@class='thumbnail']/@src!=''">
          <xsl:value-of select="Images/img[@class='thumbnail']/@src"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$page//MenuItem[@id=$Url]/Images/img[@class='thumbnail']/@src"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- ALT VALUE -->
    <xsl:variable name="alt">
      <xsl:choose>
        <xsl:when test="Images/img[@class='thumbnail']/@src!=''">
          <xsl:value-of select="Images/img[@class='thumbnail']/@alt"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$page//MenuItem[@id=$Url]/Images/img[@class='thumbnail']/@alt"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="max-width">
      <xsl:apply-templates select="." mode="getsubThWidth"/>
    </xsl:variable>
    <xsl:variable name="max-height">
      <xsl:apply-templates select="." mode="getsubThHeight"/>
    </xsl:variable>
    <!-- IF Image to resize -->
    <xsl:if test="$src!=''">
      <xsl:variable name="newSrc">
        <xsl:call-template name="resize-image">
          <xsl:with-param name="path" select="$src"/>
          <xsl:with-param name="max-width" select="$max-width"/>
          <xsl:with-param name="max-height" select="$max-height"/>
          <xsl:with-param name="file-prefix">
            <xsl:text>~th-</xsl:text>
            <xsl:value-of select="$max-width"/>
            <xsl:text>x</xsl:text>
            <xsl:value-of select="$max-height"/>
            <xsl:text>/~th-</xsl:text>
          </xsl:with-param>
          <xsl:with-param name="file-suffix" select="''"/>
          <xsl:with-param name="quality" select="100"/>
          <xsl:with-param name="crop" select="false()"/>
          <xsl:with-param name="no-stretch" select="true()"/>
        </xsl:call-template>
      </xsl:variable>
      <xsl:variable name="newimageSize" select="ew:ImageSize($newSrc)"/>
      <xsl:variable name="newimageWidth" select="substring-before($newimageSize,'x')"/>
      <xsl:variable name="newimageHeight" select="substring-after($newimageSize,'x')"/>
      <img src="{$newSrc}" width="{$newimageWidth}" height="{$newimageHeight}" alt="{$alt}" class="photo thumbnail"/>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Content | MenuItem" mode="displayLogo">
    <xsl:param name="crop" select="false()" />
    <xsl:param name="no-stretch" select="true()" />
    <xsl:param name="width"/>
    <xsl:param name="height"/>
    <xsl:param name="forceResize"/>
    <xsl:param name="class"/>
    <xsl:param name="style"/>

    <!-- IF SO THAT we don't get empty tags if NO IMAGE -->
    <xsl:if test="Images/img[@src and @src!=''] or Organization/logo/img[@class='logo']/@src!=''">
      <!-- SRC VALUE -->
      <xsl:variable name="src">
        <xsl:choose>
          <!-- IF Thumbnail use that -->
          <xsl:when test="Organization/logo/img[@class='logo']/@src!=''">
            <xsl:value-of select="Organization/logo/img[@class='logo']/@src"/>
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
      </xsl:variable>
      <!-- ALT VALUE -->
      <xsl:variable name="alt">
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
      </xsl:variable>
      <xsl:variable name="max-width">
        <xsl:choose>
          <xsl:when test="$width!=''">
            <xsl:value-of select="$width"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="." mode="getThWidth"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="max-height">
        <xsl:choose>
          <xsl:when test="$height!=''">
            <xsl:value-of select="$height"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="." mode="getThHeight"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>


      <!-- IF Image to resize -->
      <xsl:if test="$src!=''">
        <xsl:variable name="newSrc">
          <xsl:call-template name="resize-image">
            <xsl:with-param name="path" select="$src"/>
            <xsl:with-param name="max-width" select="$max-width"/>
            <xsl:with-param name="max-height" select="$max-height"/>
            <xsl:with-param name="file-prefix">
              <xsl:text>~th-</xsl:text>
              <xsl:value-of select="$max-width"/>
              <xsl:text>x</xsl:text>
              <xsl:value-of select="$max-height"/>
              <xsl:text>/~th-</xsl:text>
              <xsl:if test="$crop">
                <xsl:text>crop-</xsl:text>
              </xsl:if>
              <xsl:if test="not($no-stretch)">
                <xsl:text>strch-</xsl:text>
              </xsl:if>
            </xsl:with-param>
            <xsl:with-param name="file-suffix" select="''"/>
            <xsl:with-param name="quality" select="100"/>
            <xsl:with-param name="crop" select="$crop" />
            <xsl:with-param name="no-stretch" select="$no-stretch" />
            <xsl:with-param name="forceResize" select="$forceResize" />
          </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="imageSize" select="ew:ImageSize($newSrc)"/>
        <xsl:variable name="image">
          <img>
            <!-- SRC -->
            <xsl:attribute name="src">
              <xsl:value-of select="$newSrc"/>
            </xsl:attribute>
            <!-- Width -->
            <xsl:attribute name="width">
              <xsl:value-of select="substring-before($imageSize,'x')" />
            </xsl:attribute>
            <!-- Height -->
            <xsl:attribute name="height">
              <xsl:value-of select="substring-after($imageSize,'x')" />
            </xsl:attribute>
            <!-- Alt -->
            <xsl:attribute name="alt">
              <xsl:value-of select="$alt" />
            </xsl:attribute>
            <!-- Title -->
            <xsl:attribute name="title">
              <xsl:value-of select="$alt" />
            </xsl:attribute>
            <!-- Class -->
            <xsl:attribute name="class">
              <xsl:choose>
                <xsl:when test="$class!=''">
                  <xsl:value-of select="$class" />
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>photo thumbnail resized </xsl:text>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
            <xsl:if test="$style!=''">
              <xsl:attribute name="style">
                <xsl:value-of select="$style" />
              </xsl:attribute>
            </xsl:if>
          </img>
        </xsl:variable>
        <xsl:copy-of select="ms:node-set($image)/*" />
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <!-- -->
  <xsl:template match="Content | MenuItem | Discount | Company | productDetail" mode="getThWidth">100</xsl:template>
  <xsl:template match="Content | MenuItem | Discount | Company | productDetail" mode="getThHeight">100</xsl:template>


  <!-- Get Sub Page Thumbnail Dimensions -->
  <xsl:template match="Content | MenuItem | Discount | Company" mode="getsubThWidth">100</xsl:template>
  <xsl:template match="Content | MenuItem | Discount | Company" mode="getsubThHeight">100</xsl:template>

  <!-- To fit if submenu -->
  <xsl:template match="Content | MenuItem | Discount | Company" mode="getDisplayWidth">300</xsl:template>
  <xsl:template match="Content | MenuItem | Discount | Company" mode="getDisplayHeight">400</xsl:template>

  <!-- To fit 800x600 - fits nicely inside any screen ratio -->
  <xsl:template match="Content | MenuItem | Discount | Company" mode="getFullSizeWidth">750</xsl:template>
  <xsl:template match="Content | MenuItem | Discount | Company" mode="getFullSizeHeight">550</xsl:template>

  <!-- resize image template for Image Content and Modules -->
  <xsl:template match="Content" mode="resize-image">
    <xsl:param name="maxWidth"/>
    <xsl:param name="maxHeight"/>
    <xsl:param name="crop" select="false()"/>
    <xsl:param name="no-stretch" select="true()"/>
    <xsl:variable name="max-width">
      <xsl:choose>
        <xsl:when test="$maxWidth!=''">
          <xsl:value-of select="$maxWidth"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@width"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="max-height">
      <xsl:choose>
        <xsl:when test="$maxHeight!=''">
          <xsl:value-of select="$maxHeight"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@height"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:apply-templates select="." mode="resize-image-params">
      <xsl:with-param name="max-width" select="$max-width"/>
      <xsl:with-param name="max-height" select="$max-height"/>
      <xsl:with-param name="crop" select="$crop"/>
      <xsl:with-param name="no-stretch" select="$no-stretch"/>
    </xsl:apply-templates>
  </xsl:template>

  <xsl:template match="Content" mode="resize-image-params">
    <xsl:param name="max-width" />
    <xsl:param name="max-height" />
    <xsl:param name="crop" select="false()"/>
    <xsl:param name="no-stretch" select="true()"/>
    <xsl:variable name="newSrc">
      <xsl:call-template name="resize-image">
        <xsl:with-param name="path" select="img/@src"/>
        <xsl:with-param name="max-width" select="$max-width"/>
        <xsl:with-param name="max-height" select="$max-height"/>
        <xsl:with-param name="file-prefix">
          <xsl:text>~resized-</xsl:text>
          <xsl:value-of select="$max-width"/>
          <xsl:text>x</xsl:text>
          <xsl:value-of select="$max-height"/>
          <xsl:text>/~resized-</xsl:text>
          <xsl:if test="$crop='true'">
            <xsl:text>crpd-</xsl:text>
          </xsl:if>
          <xsl:if test="$no-stretch='true'">
            <xsl:text>strch-</xsl:text>
          </xsl:if>
        </xsl:with-param>
        <xsl:with-param name="file-suffix" select="''"/>
        <xsl:with-param name="quality" select="100"/>
        <xsl:with-param name="crop" select="$crop"/>
        <xsl:with-param name="no-stretch" select="$no-stretch"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="responsiveImg">
      <xsl:if test="@responsiveImg='true'">
        <xsl:text>img-responsive</xsl:text>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="newimageSize" select="ew:ImageSize($newSrc)"/>
    <xsl:variable name="newimageWidth" select="substring-before($newimageSize,'x')"/>
    <xsl:variable name="newimageHeight" select="substring-after($newimageSize,'x')"/>
    <img src="{$newSrc}" width="{$newimageWidth}" height="{$newimageHeight}" alt="{img/@alt}" class="photo {$responsiveImg}"/>

  </xsl:template>

  <!-- ## GET CONTENT LISTS - This is used in all LISTING MODULES ################################## -->
  <!--    to stick required Content in a text string variable - before output.
          Handles - Ordering and Steppers  
  -->

  <!-- FOR ALL PAGE CONTENT -->
  <xsl:template match="Content[@display='all']" mode="getContent">
    <xsl:param name="contentType" />
    <xsl:param name="startPos" />
    <xsl:param name="parentClass" />
    <xsl:param name="sort" select="@sortBy"/>
    <xsl:param name="order" select="@order"/>
    <xsl:param name="sort-data-type">
      <xsl:call-template name="ordering-data-type">
        <xsl:with-param name="field" select="@sortBy"/>
      </xsl:call-template>
    </xsl:param>
    <xsl:param name="stepCount" select="@stepCount"/>
    <xsl:param name="endPos">
      <xsl:choose>
        <xsl:when test="@stepCount = '0'">
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="number($startPos + concat('0',@stepCount))"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="maxDisplay">
      <xsl:choose>
        <xsl:when test="@maxDisplay &gt; 0">
          <xsl:value-of select="@maxDisplay"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>0</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <!--xsl:if test="$parentClass!=''">
    TS: CANT REMEMBER WHAT THIS IS FOR REMOVED IT SO WE GET BACK A CLEAN CONTENT LIST AND POSITION IS ACCURATE. LET'S SEE WHAT BREAKS
      <Parent class="{$parentClass}"/>
    </xsl:if-->
    <xsl:choose>
      <!-- When Page Order -->
      <xsl:when test="$sort='Position' or not($order) or $order=''">
        <xsl:for-each select="/Page/Contents/Content[@type=$contentType]">
          <xsl:if test="position() &gt; $startPos and position() &lt;= $endPos">
            <xsl:copy-of select="."/>
          </xsl:if>
        </xsl:for-each>
      </xsl:when>
      <xsl:when test="$sort='Price'">
        <xsl:for-each select="/Page/Contents/Content[@type=$contentType and not(Content[@type='SKU'])]">
          <xsl:sort select="Prices/Price[@type='rrp']/node()" order="{$order}" data-type="number"/>
          <xsl:if test="$stepCount = '0' or ($stepCount &gt; 0 and position() &gt; $startPos and position() &lt;= $endPos)">
            <xsl:if test="$maxDisplay = '0' or ($maxDisplay &gt; 0 and position() &lt;= $maxDisplay)">
              <xsl:copy-of select="."/>
            </xsl:if>
          </xsl:if>
        </xsl:for-each>
        <xsl:for-each select="/Page/Contents/Content[@type=$contentType and Content[@type='SKU']]">
          <xsl:sort select="Content[@type='SKU'][1]/Prices/Price[@type='sale']/node()" order="{$order}" data-type="number"/>
          <xsl:if test="$stepCount = '0' or ($stepCount &gt; 0 and position() &gt; $startPos and position() &lt;= $endPos)">
            <xsl:if test="$maxDisplay = '0' or ($maxDisplay &gt; 0 and position() &lt;= $maxDisplay)">
              <xsl:copy-of select="."/>
            </xsl:if>
          </xsl:if>
        </xsl:for-each>
      </xsl:when>
      <xsl:when test="$sort='figure'">
        <xsl:for-each select="/Page/Contents/Content[@type=$contentType]">
          <xsl:sort select="Salary/@figure" order="{$order}" data-type="number"/>
          <xsl:if test="$stepCount = '0' or ($stepCount &gt; 0 and position() &gt; $startPos and position() &lt;= $endPos)">
            <xsl:if test="$maxDisplay = '0' or ($maxDisplay &gt; 0 and position() &lt;= $maxDisplay)">
              <xsl:copy-of select="."/>
            </xsl:if>
          </xsl:if>
        </xsl:for-each>
      </xsl:when>
      <xsl:when test="$sort='StartDate/@sortDate'">
        <xsl:for-each select="/Page/Contents/Content[@type=$contentType]">
          <xsl:sort select="StartDate/@sortDate" order="{$order}" data-type="text"/>
          <xsl:if test="$stepCount = '0' or ($stepCount &gt; 0 and position() &gt; $startPos and position() &lt;= $endPos)">
            <xsl:if test="$maxDisplay = '0' or ($maxDisplay &gt; 0 and position() &lt;= $maxDisplay)">
              <xsl:copy-of select="."/>
            </xsl:if>
          </xsl:if>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <xsl:for-each select="/Page/Contents/Content[@type=$contentType]">
          <xsl:sort select="@*[name()=$sort] | descendant-or-self::*[name()=$sort] | descendant-or-self::*[name()=$sort and @type='sale']" order="{$order}" data-type="{$sort-data-type}"/>
          <xsl:sort select="@update" order="{$order}" data-type="text"/>
          <xsl:if test="$stepCount = '0' or ($stepCount &gt; 0 and position() &gt; $startPos and position() &lt;= $endPos)">
            <xsl:if test="$maxDisplay = '0' or ($maxDisplay &gt; 0 and position() &lt;= $maxDisplay)">
              <xsl:copy-of select="."/>
            </xsl:if>
          </xsl:if>
        </xsl:for-each>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- FOR ALL RELATED CONTENT -->
  <xsl:template match="Content[@display='related' or @display='grabber' or @display='group']" mode="getContent">
    <xsl:param name="contentType" />
    <xsl:param name="startPos" />
    <xsl:param name="parentClass" />
    <xsl:param name="sort-data-type">
      <xsl:call-template name="ordering-data-type">
        <xsl:with-param name="field" select="@sortBy"/>
      </xsl:call-template>
    </xsl:param>
    <xsl:param name="sort" select="@sortBy"/>
    <xsl:param name="order" select="@order"/>
    <xsl:param name="stepCount" select="@stepCount"/>
    <xsl:param name="endPos">
      <xsl:choose>
        <xsl:when test="@stepCount = '0'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="number($startPos + concat('0',@stepCount))"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:if test="$parentClass!=''">
      <Parent class="{$parentClass}"/>
    </xsl:if>
    <xsl:choose>
      <!-- When Page Order -->
      <xsl:when test="$sort='Position' or $sort='' or $order=''">
        <xsl:for-each select="Content[@type=$contentType]">
          <xsl:if test="position() &gt; $startPos and position() &lt;= $endPos">
            <xsl:copy-of select="."/>
          </xsl:if>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <xsl:for-each select="Content[@type=$contentType]">
          <xsl:sort select="@*[name()=$sort] | descendant-or-self::*[name()=$sort]" order="{$order}" data-type="{$sort-data-type}"/>
          <xsl:sort select="@update" order="{$order}" data-type="text"/>
          <xsl:if test="$stepCount = '0' or ($stepCount &gt; 0 and position() &gt; $startPos and position() &lt;= $endPos)">
            <xsl:copy-of select="."/>
          </xsl:if>
        </xsl:for-each>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- FOR ALL RELATED CONTENT -->
  <xsl:template match="Content[@display='relatedTag']" mode="getContent">
    <xsl:param name="contentType" />
    <xsl:param name="startPos" />
    <xsl:param name="parentClass" />
    <xsl:param name="sort-data-type">
      <xsl:call-template name="ordering-data-type">
        <xsl:with-param name="field" select="@sortBy"/>
      </xsl:call-template>
    </xsl:param>
    <xsl:param name="sort" select="@sortBy"/>
    <xsl:param name="order" select="@order"/>
    <xsl:param name="stepCount" select="@stepCount"/>
    <xsl:param name="maxDisplay">
      <xsl:choose>
        <xsl:when test="@maxDisplay &gt; 0">
          <xsl:value-of select="@maxDisplay"/>
        </xsl:when>
        <xsl:otherwise>
          0
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="endPos">
      <xsl:choose>
        <xsl:when test="@maxDisplay &gt; 0">
          <xsl:value-of select="@maxDisplay"/>
        </xsl:when>
        <xsl:when test="@stepCount = '0'">
          <xsl:value-of select="count(Content[@type='Tag']/Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="number($startPos + concat('0',@stepCount))"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <Parent class="{$parentClass}"/>
    <xsl:choose>
      <!-- When Page Order -->
      <xsl:when test="$sort='Position' or $sort='' or $order=''">
        <xsl:for-each select="Content[@type='Tag']/Content[@type=$contentType]">
          <xsl:if test="position() &gt; $startPos and position() &lt;= $endPos">
            <xsl:copy-of select="."/>
          </xsl:if>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <xsl:for-each select="Content[@type='Tag']/Content[@type=$contentType]">
          <xsl:sort select="@*[name()=$sort] | descendant-or-self::*[name()=$sort]" order="{$order}" data-type="{$sort-data-type}"/>
          <xsl:sort select="@update" order="{$order}" data-type="text"/>
          <xsl:if test="($stepCount = '0' and $maxDisplay = '0') or ($stepCount &gt; 0 and position() &gt; $startPos and position() &lt;= $endPos) or ($maxDisplay &gt; 0 and position() &lt;= $maxDisplay)">
            <xsl:copy-of select="."/>
          </xsl:if>
        </xsl:for-each>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!-- FOR ALL MENU ITEMS IN A SUBPAGE LIST  -->
  <xsl:template match="Content[@display='all' and @contentType='MenuItem']" mode="getContent">
    <xsl:param name="contentType" />
    <xsl:param name="startPos" />
    <xsl:param name="parentClass" />
    <xsl:param name="sort-data-type">
      <xsl:call-template name="ordering-data-type">
        <xsl:with-param name="field" select="@sortBy"/>
      </xsl:call-template>
    </xsl:param>
    <xsl:param name="sort" select="@sortBy"/>
    <xsl:param name="order">
      <xsl:choose>
        <xsl:when test="@order = ''">
          <xsl:text>ascending</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@order"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="stepCount" select="@stepCount"/>
    <xsl:param name="endPos">
      <xsl:choose>
        <xsl:when test="@stepCount = '0'">
          <xsl:value-of select="count($currentPage/MenuItem)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="number($startPos + concat('0',@stepCount))"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <Parent class="{$parentClass}"/>
    <xsl:choose>

      <!-- When Menu Order -->
      <xsl:when test="$sort='Position' or $sort='' or $order=''">
        <xsl:for-each select="$currentPage/MenuItem[not(DisplayName/@exclude='true')]">
          <xsl:if test="position() &gt; $startPos and position() &lt;= $endPos">
            <xsl:copy-of select="."/>
          </xsl:if>
        </xsl:for-each>
      </xsl:when>

      <xsl:otherwise>
        <xsl:for-each select="$currentPage/MenuItem[not(DisplayName/@exclude='true')]">
          <xsl:sort select="@*[name()=$sort] | descendant-or-self::*[name()=$sort]" order="{$order}" data-type="{$sort-data-type}"/>
          <xsl:sort select="@update" order="{$order}" data-type="text"/>
          <xsl:if test="$stepCount = '0' or ($stepCount &gt; 0 and position() &gt; $startPos and position() &lt;= $endPos)">
            <xsl:copy-of select="."/>
          </xsl:if>
        </xsl:for-each>
      </xsl:otherwise>

    </xsl:choose>
  </xsl:template>

  <!-- FOR ALL MENU ITEMS IN A SUBPAGE LIST OF A SECTION  -->
  <xsl:template match="Content[@display='related' and @contentType='MenuItem']" mode="getContent">
    <xsl:param name="contentType" />
    <xsl:param name="startPos" />
    <xsl:param name="sort-data-type">
      <xsl:call-template name="ordering-data-type">
        <xsl:with-param name="field" select="@sortBy"/>
      </xsl:call-template>
    </xsl:param>
    <xsl:param name="link" select="@pageLink" />
    <xsl:param name="sort" select="@sortBy" />
    <xsl:param name="order">
      <xsl:choose>
        <xsl:when test="@order = ''">
          <xsl:text>ascending</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@order"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:param name="stepCount" select="@stepCount" />
    <xsl:param name="parentPage" select="//MenuItem[@id=$link]"/>
    <xsl:param name="endPos">
      <xsl:choose>
        <xsl:when test="@stepCount = '0'">
          <xsl:value-of select="count($parentPage/MenuItem)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="number($startPos + concat('0',@stepCount))"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>
    <xsl:choose>

      <!-- When Menu Order -->
      <xsl:when test="$sort='Position' or $sort = ''">
        <xsl:for-each select="$parentPage/MenuItem">
          <xsl:if test="position() &gt; $startPos and position() &lt;= $endPos">
            <xsl:copy-of select="."/>
          </xsl:if>
        </xsl:for-each>
      </xsl:when>

      <xsl:otherwise>
        <xsl:for-each select="$parentPage/MenuItem">
          <xsl:sort select="@*[name()=$sort] | descendant-or-self::*[name()=$sort]" order="{$order}" data-type="{$sort-data-type}"/>
          <xsl:sort select="@update" order="{$order}" data-type="text"/>
          <xsl:if test="$stepCount = '0' or ($stepCount &gt; 0 and position() &gt; $startPos and position() &lt;= $endPos)">
            <xsl:copy-of select="."/>
          </xsl:if>

        </xsl:for-each>
      </xsl:otherwise>

    </xsl:choose>
  </xsl:template>

  <!-- This template determines the ordering datatype -->
  <!-- default is text - with hard coded exceptions for number -->
  <xsl:template name="ordering-data-type">
    <xsl:param name="field" />
    <xsl:choose>
      <xsl:when test="$field='Price'">
        <xsl:text>number</xsl:text>
      </xsl:when>
      <xsl:otherwise>text</xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!-- FOR ALL DIRECTORY ITEMS [User | Group | Company etc.] -->
  <!-- TS Removed company because we use company as a standard content type on sepscience-->
  <!--xsl:template match="Content[@contentType='User' or @contentType='Group' or @contentType='Company']" mode="getContent"-->
  <xsl:template match="Content[@contentType='User' or @contentType='Group']" mode="getContent">
    <xsl:param name="contentType" />
    <xsl:param name="startPos" />
    <xsl:param name="sort-data-type">
      <xsl:call-template name="ordering-data-type">
        <xsl:with-param name="field" select="@sortBy"/>
      </xsl:call-template>
    </xsl:param>
    <xsl:param name="sort" select="@sortBy"/>
    <xsl:param name="order" select="@order"/>
    <xsl:param name="stepCount" select="@stepCount"/>
    <xsl:param name="endPos">
      <xsl:choose>
        <xsl:when test="@stepCount = '0'">
          <xsl:value-of select="count(*[name()=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="number($startPos + concat('0',@stepCount))"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:param>

    <xsl:choose>
      <!-- When Page Order -->
      <xsl:when test="$sort='Position' or $sort='' or $order=''">

        <xsl:for-each select="*[name()=$contentType]">
          <xsl:if test="position() &gt; $startPos and position() &lt;= $endPos">
            <xsl:copy-of select="."/>
          </xsl:if>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <xsl:for-each select="*[name()=$contentType]">
          <xsl:sort select="@*[name()=$sort] | descendant-or-self::*[name()=$sort]" order="{$order}" data-type="{$sort-data-type}"/>
          <xsl:sort select="@update" order="{$order}" data-type="text"/>
          <xsl:if test="$stepCount = '0' or ($stepCount &gt; 0 and position() &gt; $startPos and position() &lt;= $endPos)">
            <xsl:copy-of select="."/>
          </xsl:if>
        </xsl:for-each>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!-- ## ADDRESS TEMPLATES  ################################## -->
  <!--    Standard ways of handling addresses -->
  <xsl:template match="Address" mode="getAddress">
    <xsl:if test="No/node() or Street/node() or Locality/node() or Region/node() or PostCode/node()">
      <p class="adr">
        <xsl:if test="ancestor::Location/Venue/node()!=''">
          <span class="name">
            <xsl:value-of select="ancestor::Location/Venue/node()"/>
          </span>
          <br/>
        </xsl:if>
        <xsl:if test="No/node()!='' or Street/node()!=''">
          <span class="street-address">
            <xsl:value-of select="No"/>
            <xsl:text> </xsl:text>
            <xsl:value-of select="Street"/>
          </span>
          <br/>
        </xsl:if>
        <xsl:if test="Locality/node()!=''">
          <span class="locality">
            <xsl:value-of select="Locality"/>
          </span>
          <br/>
        </xsl:if>
        <xsl:if test="Region/node()">
          <span class="region">
            <xsl:value-of select="Region"/>
          </span>
          <br/>
        </xsl:if>
        <xsl:if test="PostCode/node()">
          <span class="postal-code">
            <xsl:value-of select="PostCode"/>
          </span>
        </xsl:if>
      </p>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Address" mode="csvAddress">
    <xsl:if test="No/node() or Street/node() or Locality/node() or Region/node() or PostCode/node() or PostCode/node()">
      <xsl:if test="No/node()">
        <xsl:value-of select="No"/>
        <xsl:text>,</xsl:text>
      </xsl:if>
      <xsl:if test="Street/node()">
        <xsl:value-of select="Street"/>
        <xsl:text>,</xsl:text>
      </xsl:if>
      <xsl:if test="Locality/node()">
        <xsl:value-of select="Locality"/>
        <xsl:text>,</xsl:text>
      </xsl:if>
      <xsl:if test="Region/node()">
        <xsl:value-of select="Region"/>
        <xsl:text>,</xsl:text>
      </xsl:if>
      <xsl:if test="PostCode/node()">
        <xsl:value-of select="PostCode"/>
        <xsl:text>,</xsl:text>
      </xsl:if>
      <xsl:if test="Country/node()">
        <xsl:value-of select="Country"/>
      </xsl:if>
    </xsl:if>
  </xsl:template>



  <!--  =====================================================================================   -->
  <!--  ==  RSS FEEDS  ======================================================================   -->
  <!--  =====================================================================================   -->

  <xsl:template match="Content" mode="getRssHref">
    <xsl:value-of select="$siteURL"/>
    <xsl:choose>
      <xsl:when test="@rss='this'">
        <xsl:text>/ewcommon/feeds/rss/rss.ashx</xsl:text>
        <xsl:choose>
          <xsl:when test="@rss='this'">
            <xsl:text>?pgid=</xsl:text>
            <xsl:value-of select="/Page/@id"/>
            <xsl:text>&amp;moduleId=</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>?contentType=</xsl:text>
            <xsl:value-of select="@contentType"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>/ewcommon/feeds/generic/feed.ashx</xsl:text>
        <xsl:text>?contentSchema=</xsl:text>
        <xsl:value-of select="@contentType"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- RSS titckler for browsers -->
  <xsl:template match="Content[@rss]" mode="feedLinks">
    <xsl:variable name="href">
      <xsl:apply-templates select="." mode="getRssHref" />
    </xsl:variable>
    <link rel="alternate" type="application/rss+xml" title="{@title}" href="{$href}"/>
  </xsl:template>

  <!-- Module RSS Link -->
  <xsl:template match="Content[@type='Module']" mode="rssLink">
    <xsl:variable name="href">
      <xsl:apply-templates select="." mode="getRssHref" />
    </xsl:variable>
    <a href="{$href}" title="Click to subscribe" class="rsssubscribebutton" rel="external">
      <xsl:call-template name="rssSubscribe"/>
    </a>
  </xsl:template>

  <xsl:template name="rssSubscribe">
    <img src="/ewcommon/images/icons/rss16x16.png" width="16" height="16" alt="RSS" />
  </xsl:template>



  <!--  =====================================================================================   -->
  <!--  ==  Social Bookmarking  =============================================================   -->
  <!--  =====================================================================================   -->

  <!-- Share buttons for Content -->
  <xsl:template match="Content" mode="share">
    SOCIAL NETWORKING TO GO HERE
  </xsl:template>



  <!-- ###################################  Prices   ########################################   -->
  <!-- Display Price -->
  <xsl:template match="Content" mode="displayPrice">
    <xsl:param name="displayOutput"/>
    <xsl:param name="noLabel"/>
    <xsl:if test="Prices/Price[(@currency=$currency and @type='sale')]/node()">
      <xsl:choose>
        <xsl:when test="$displayOutput!=''">
          <xsl:element name="{$displayOutput}">
            <xsl:apply-templates select="." mode="showPrice"/>
          </xsl:element>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="." mode="showPrice"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>

  <xsl:template name="SortArrows">
    <xsl:param name="sortCol"/>
    <xsl:param name="bSortFormMethod"/>

    <div class="sortArrows">
      <xsl:variable name="qsSet" select="/Page/Request/QueryString/Item[@name!='sortCol' and @name!='sortDir']"/>
      <xsl:variable name="qs">
        <xsl:for-each select="$qsSet/.">
          <xsl:if test="not(position()=1)">&amp;</xsl:if>
          <xsl:value-of select="@name"/>=<xsl:value-of select="."/>
        </xsl:for-each>
      </xsl:variable>
      <a href="?{$qs}&amp;sortCol={$sortCol}&amp;sortDir=ascending" title="Sort Ascending">
        <img  src="/ewcommon/images/sortDown.gif" width="11" height="7" class="down" />
      </a>
      <a href="?{$qs}&amp;sortCol={$sortCol}&amp;sortDir=descending" title="Sort Descending">
        <img  src="/ewcommon/images/sortUp.gif" width="11" height="7" class="up" />
      </a>
    </div>
  </xsl:template>


  <!-- Show Price -->
  <xsl:template match="Content" mode="showPrice">
    <xsl:variable name="rrpPrice">
      <xsl:value-of select="Prices/Price[@currency=$currency and @type='rrp']/node()"/>
    </xsl:variable>
    <xsl:variable name="price">
      <xsl:value-of select="Prices/Price[@currency=$currency and @type='sale']/node()"/>
    </xsl:variable>
    <xsl:variable name="id">
      <xsl:choose>
        <xsl:when test="@type='SKU'">
          <xsl:value-of select="../@id"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@id"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- RRP and standard prices must remain within the #price_{$id} -->
    <span class="prices" id="price_{$id}">
      <!-- RRP First -->
      <xsl:if test="$rrpPrice!='' and $rrpPrice &gt; 0">
        <span class="rrpLabel">RRP: </span>
        <span class="rrpPrice">
          <xsl:apply-templates select="$page" mode="formatPrice">
            <xsl:with-param name="price">
              <xsl:value-of select="$rrpPrice"/>
            </xsl:with-param>
            <xsl:with-param name="currency" select="$currencySymbol"/>
          </xsl:apply-templates>
          <xsl:text> </xsl:text>
          <span class="priceSuffix">
            <xsl:value-of select="Prices/Price[@currency=$currency and @type='rrp']/@suffix"/>
          </span>
        </span>
        <xsl:text> - </xsl:text>
      </xsl:if>
      <!-- BUY NOW PRICE -->
      <span class="price">
        <xsl:choose>
          <xsl:when test="format-number($price, '#.00')='NaN'">
            <xsl:value-of select="$price"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="$page" mode="formatPrice">
              <xsl:with-param name="price">
                <xsl:value-of select="$price"/>
              </xsl:with-param>
              <xsl:with-param name="currency" select="$currencySymbol"/>
            </xsl:apply-templates>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:if test="Prices/Price[@currency=$currency and @type='sale']/@suffix!=''">
          <span class="priceSuffix">
            <xsl:value-of select="Prices/Price[@currency=$currency and @type='sale']/@suffix"/>
          </span>
        </xsl:if>
      </span>
    </span>
    <xsl:apply-templates select="." mode="getDiscountInfo"/>
  </xsl:template>

  <!-- Get Discount Info -->
  <xsl:template match="Content | option" mode="getDiscountInfo">
    <xsl:variable name="discount">
      <xsl:choose>
        <xsl:when test="Prices/Price[@type='sale']/@originalPrice">
          <xsl:value-of select="Prices/Price[@type='sale']/@originalPrice"/>
        </xsl:when>
        <xsl:when test="Prices/Price[@type='rrp']/@originalPrice">
          <xsl:value-of select="Prices/Price[@type='rrp']/@originalPrice"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test="$discount!=''">
      <span class="discountinfo">
        <xsl:apply-templates select="Discount/cDescription" mode="cleanXhtml"/>
        <xsl:text> ( </xsl:text>
        <xsl:call-template name="term2017" />&#160;
        <xsl:choose>
          <xsl:when test="format-number($discount, '#.00')='NaN'">
            <xsl:value-of select="$discount"/>
          </xsl:when>
          <xsl:otherwise>
            <span class="rrpPrice">
              <xsl:apply-templates select="/Page" mode="formatPrice">
                <xsl:with-param name="price" select="$discount"/>
                <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
              </xsl:apply-templates>
            </span>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text> )</xsl:text>
      </span>
    </xsl:if>
  </xsl:template>

  <!-- Show Price FOR Subscriptions -->
  <xsl:template match="Content[@type='Subscription']" mode="showPrice">
    <xsl:variable name="price">
      <xsl:value-of select="Prices/Price[@currency=$currency and @type='sale']/node()"/>
    </xsl:variable>

    <xsl:variable name="rptprice">
      <xsl:value-of select="SubscriptionPrices/Price[@currency=$currency and @type='sale']/node()"/>
    </xsl:variable>

    <span class="prices">
      <xsl:if test="$price != $rptprice">
        <!-- BUY NOW PRICE -->
        <span class="price">
          <xsl:choose>
            <xsl:when test="format-number($price, '#.00')='NaN'">
              <xsl:value-of select="$price"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:apply-templates select="$page" mode="formatPrice">
                <xsl:with-param name="price">
                  <xsl:value-of select="$price"/>
                </xsl:with-param>
                <xsl:with-param name="currency" select="$currencySymbol"/>
              </xsl:apply-templates>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:if test="$price&gt;0">
            <span class="priceSuffix">
              <xsl:text> </xsl:text>
              <xsl:value-of select="Prices/Price[@currency=$currency and @type='sale']/@suffix"/>
            </span>
          </xsl:if>
        </span>
        <xsl:if test="$rptprice&gt;0">
          <xsl:text> - </xsl:text>
        </xsl:if>
      </xsl:if>
      <xsl:if test="$rptprice&gt;0">
        <span class="price">
          <xsl:choose>
            <xsl:when test="format-number($rptprice, '#.00')='NaN'">
              <xsl:value-of select="$price"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:apply-templates select="$page" mode="formatPrice">
                <xsl:with-param name="price">
                  <xsl:value-of select="$rptprice"/>
                </xsl:with-param>
                <xsl:with-param name="currency" select="$currencySymbol"/>
              </xsl:apply-templates>
            </xsl:otherwise>
          </xsl:choose>
          <span class="priceSuffix">
            <xsl:text> </xsl:text>
            <xsl:choose>
              <xsl:when test="SubscriptionPrices/Price[@currency=$currency and @type='sale']/@suffix!=''">
                <xsl:value-of select="SubscriptionPrices/Price[@currency=$currency and @type='sale']/@suffix"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:text>per </xsl:text>
                <xsl:value-of select="translate(PaymentUnit/node(),'DWMY','dwmy')"/>
              </xsl:otherwise>
            </xsl:choose>
          </span>
        </span>
      </xsl:if>
    </span>

    <xsl:apply-templates select="." mode="getDiscountInfo"/>
  </xsl:template>

  <!--*********************************** Placeholder for Xform Editing when no Admin XSL used. ************-->
  <xsl:template match="*" mode="editXformMenu">

  </xsl:template>



  <!-- ##  XML TO JSON  ###########################################################################   -->

  <xsl:template match="*" mode="xml2json">
    <xsl:text>{</xsl:text>
    <xsl:apply-templates select="." mode="jsonObjectOrElementProperty"/>
    <xsl:text>};</xsl:text>
  </xsl:template>

  <xsl:template match="*" mode="jsonObjectOrElementProperty">
    <xsl:text>"</xsl:text>
    <xsl:value-of select="name()"/>
    <xsl:text>":</xsl:text>
    <xsl:apply-templates select="." mode="jsonObjectProperties"/>
  </xsl:template>

  <!-- Array Element -->
  <xsl:template match="*" mode="jsonArrayElement">
    <xsl:apply-templates select="." mode="jsonObjectProperties"/>
  </xsl:template>

  <!-- Object Properties -->
  <xsl:template match="*" mode="jsonObjectProperties">
    <xsl:variable name="childName" select="name(*[1])"/>

    <xsl:choose>
      <xsl:when test="not(*|@*)">

        <xsl:text>"</xsl:text>
        <xsl:value-of select="."/>
        <xsl:text>"</xsl:text>
      </xsl:when>
      <xsl:when test="count(*[name()=$childName]) > 1">
        <xsl:text>{"</xsl:text>
        <xsl:value-of select="$childName"/>
        <xsl:text>":[</xsl:text>
        <xsl:apply-templates select="*" mode="jsonArrayElement"/>
        <xsl:text>]}</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>{</xsl:text>
        <!-- Calculate if need to calculate a comma at end of attributes. -->
        <xsl:variable name="totalAtts">
          <xsl:if test="@* and not(*)">
            <xsl:value-of select="count(@*)"/>
          </xsl:if>
        </xsl:variable>
        <xsl:apply-templates select="@*" mode="jsonObjectOrElementProperty">
          <xsl:with-param name="totalAtts" select="$totalAtts"/>
        </xsl:apply-templates>
        <xsl:apply-templates select="*" mode="jsonObjectOrElementProperty"/>
        <xsl:text>}</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:if test="following-sibling::*">,</xsl:if>
  </xsl:template>

  <!-- Attribute Property -->
  <xsl:template match="@*" mode="jsonObjectOrElementProperty">
    <xsl:param name="totalAtts"/>
    <xsl:text>"</xsl:text>
    <xsl:value-of select="name()"/>
    <xsl:text>":"</xsl:text>
    <!--<xsl:value-of select="$totalAtts"/>-<xsl:value-of select="position()"/>~-->
    <xsl:value-of select="."/>
    <xsl:text>"</xsl:text>
    <xsl:if test="$totalAtts='' or (position() &lt; $totalAtts)">
      <xsl:text>,</xsl:text>
    </xsl:if>
  </xsl:template>

  <!-- ## / XML TO JSON  ###########################################################################   -->

  <xsl:template name="getFileTypeName">
    <xsl:param name="extension" />

    <xsl:choose>
      <xsl:when test="$extension='.pdf'">
        <!--Adobe PDF-->
        <xsl:call-template name="term2029" />
      </xsl:when>
      <xsl:when test="$extension='.doc' or $extension='.docx'">
        <!--Word Document-->
        <xsl:call-template name="term2030" />
      </xsl:when>
      <xsl:when test="contains($extension,'.xls') or contains($extension,'.xlsx')">
        <!--Excel Spreadsheet-->
        <xsl:call-template name="term2031" />
      </xsl:when>
      <xsl:when test="contains($extension,'.zip')">
        <!--Zip Archive-->
        <xsl:call-template name="term2032" />
      </xsl:when>
      <xsl:when test="contains($extension,'.ppt')">
        <!--PowerPoint Presentation-->
        <xsl:call-template name="term2033" />
      </xsl:when>
      <xsl:when test="contains($extension,'.zip')">
        <!--PowerPoint Slideshow-->
        <xsl:call-template name="term2034" />
      </xsl:when>
      <xsl:when test="$extension='.mdb' or $extension='.accdb'">
        <!--Access Database-->
        <xsl:call-template name="term2034a" />
      </xsl:when>
      <xsl:when test="contains($extension,'.jpg')">
        <!--JPEG Image file-->
        <xsl:call-template name="term2035" />
      </xsl:when>
      <xsl:when test="contains($extension,'.gif')">
        <!--GIF Image file-->
        <xsl:call-template name="term2036" />
      </xsl:when>
      <xsl:when test="contains($extension,'.png')">
        <!--PNG Image file-->
        <xsl:call-template name="term2037" />
      </xsl:when>

      <xsl:otherwise>
        <!--Unknown File Type-->
        <xsl:call-template name="term2038" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="getFileTypeIcon">
    <xsl:param name="extension" />
    <xsl:choose>
      <xsl:when test="$extension='.pdf'">
        <!--Adobe PDF-->
        <i class="fa fa-file-pdf-o">
          <xsl:text> </xsl:text>
        </i>
      </xsl:when>
      <xsl:when test="$extension='.doc' or $extension='.docx'">
        <!--Word Document-->
        <i class="fa fa-file-word-o">
          <xsl:text> </xsl:text>
        </i>
      </xsl:when>
      <xsl:when test="contains($extension,'.xls') or contains($extension,'.xlsx')">
        <!--Excel Spreadsheet-->
        <i class="fa fa-file-excel-o">
          <xsl:text> </xsl:text>
        </i>
      </xsl:when>
      <xsl:when test="contains($extension,'.zip')">
        <!--Zip Archive-->
        <i class="fa fa-file-archive-o">
          <xsl:text> </xsl:text>
        </i>
      </xsl:when>
      <xsl:when test="contains($extension,'.ppt')">
        <!--PowerPoint Presentation-->

        <i class="fa fa-file-powerpoint-o">
          <xsl:text> </xsl:text>
        </i>
      </xsl:when>
      <xsl:when test="$extension='.mdb' or $extension='.accdb'">
        <!--Access Database-->
        <i class="fa fa-database">
          <xsl:text> </xsl:text>
        </i>
      </xsl:when>
      <xsl:when test="contains($extension,'.jpg')">
        <!--JPEG Image file-->
        <i class="fa fa-file-image-o">
          <xsl:text> </xsl:text>
        </i>
      </xsl:when>
      <xsl:when test="contains($extension,'.gif')">
        <!--GIF Image file-->
        <i class="fa fa-file-image-o">
          <xsl:text> </xsl:text>
        </i>
      </xsl:when>
      <xsl:when test="contains($extension,'.png')">
        <!--PNG Image file-->
        <i class="fa fa-file-image-o">
          <xsl:text> </xsl:text>
        </i>
      </xsl:when>

      <xsl:otherwise>
        <!--Unknown File Type-->
        <i class="fa fa-file-o">
          <xsl:text> </xsl:text>
        </i>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="getBrowserVersion">
    <xsl:choose>
      <!-- When using chromeframe - treat browser as chrome -->
      <xsl:when test="contains($userAgent, 'MSIE') and contains($userAgent, 'chromeframe')">
        <xsl:text>Chrome</xsl:text>
      </xsl:when>
      <!-- IE6 -->
      <xsl:when test="contains($userAgent, 'MSIE 6.0') and not(contains($userAgent, 'Opera'))">
        <xsl:text>MSIE 6.0</xsl:text>
      </xsl:when>
      <!-- IE7 - IE9 Compat reports as IE7, but renders using Trident 5.0 engine -->
      <xsl:when test="contains($userAgent, 'MSIE 7.0') and not(contains($userAgent, 'Trident/5.0'))">
        <xsl:text>MSIE 7.0</xsl:text>
      </xsl:when>
      <!-- IE8 -->
      <xsl:when test="contains($userAgent, 'MSIE 8.0') and contains($userAgent, 'Trident/4.0')">
        <xsl:text>MSIE 8.0</xsl:text>
      </xsl:when>
      <!-- IE9 -->
      <xsl:when test="contains($userAgent, 'MSIE 9.0') and contains($userAgent, 'Trident/5.0')">
        <xsl:text>MSIE 9.0</xsl:text>
      </xsl:when>
      <!-- Firefox -->
      <xsl:when test="contains($userAgent, 'Firefox/')">
        <xsl:text>Firefox</xsl:text>
      </xsl:when>
      <!-- Chrome -->
      <xsl:when test="contains($userAgent, 'Chrome/')">
        <xsl:text>Chrome</xsl:text>
      </xsl:when>
      <!-- Android -->
      <xsl:when test="contains($userAgent, 'WebKit/') and contains($userAgent, 'Android')">
        <xsl:text>Android</xsl:text>
      </xsl:when>
      <!-- Safari -->
      <xsl:when test="contains($userAgent, 'Safari/') and not(contains($userAgent, 'Chrome/'))">
        <xsl:text>Safari</xsl:text>
      </xsl:when>
      <!-- Opera -->
      <xsl:when test="contains($userAgent, 'Opera/')">
        <xsl:text>Opera</xsl:text>
      </xsl:when>

      <!-- Other webkit -->
      <xsl:when test="contains($userAgent, 'WebKit/')">
        <xsl:text>WebKit</xsl:text>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- calculates average review and rounds to nearest integer -->
  <xsl:template match="*" mode="getAverageReviewRating">
    <xsl:value-of select="round(sum(Content[@type='Review']/Rating) div count(Content[@type='Review']))"/>
  </xsl:template>


  <!-- ##########################################################################################################-->

  <!-- ######################################  XSLT EW:FUNCTIONS   ##############################################-->


  <!-- CLEAN XHTML from Feeds ##############################################################################################-->
  <!--  ew:CleanHTML(string) Function 
            Returns a string. The First argument is the string to be returned, after it has run through a clean XHTML
            routine.  e.g changes &lt; to < and &gt; to > etc... -->

  <xsl:template name="clean-HTML">
    <xsl:param name="html"/>
    <xsl:value-of select="ew:CleanHTML($html)"/>
  </xsl:template>

  <xsl:template name="clean-HTML-node">
    <xsl:param name="html"/>
    <xsl:param name="removeTags"/>
    <xsl:value-of select="ew:CleanHTMLElement(ms:node-set($html),$removeTags)"/>
  </xsl:template>


  <!-- TRUNCATE STRING ##############################################################################################-->
  <!--  ew:TruncateString(string,long) Function 
            Returns a string. The First argument is the string to be truncated.  The Second argument is
			the maximum character length that the string is to be truncated to.
			
			This function Truncates the string to lsat whole word and adds ellipses on to the end -->

  <xsl:template name="truncate-string">
    <xsl:param name="text"/>
    <xsl:param name="length"/>
    <xsl:value-of select="ew:TruncateString($text,$length)"/>
  </xsl:template>


  <!-- ## FILE EXISTS FUNCTION ##############################################################################################-->
  <!--    ew:VirtualFileExists(string) Function 
            Returns an Interger 1 or 0. 
            The argument is the suposed file path.  
   -->

  <xsl:template name="virtual-file-exists">
    <xsl:param name="path"/>
    <xsl:value-of select="ew:VirtualFileExists($path)"/>
  </xsl:template>


  <!-- RESIZE IMAGE from original ##############################################################################################-->
  <!--  ew:ResizeImage(string,number,number,string,number) Function 
            Returns a string, path to the new image. 
            The First argument is path of the original image to be resized,
            The Second argument is the max-width of the new image,
            The Third argument is the max height of the new image,
            The Fourth argument is an optional filename prefix [can include new directory path e.g. /thumbnails/tn-] of the new image,
            The Fifth argument is an optional suffix of the new image,
            The Sixth argument is the compression quality of the new image, interger representing a percentage 1-100 -->

  <xsl:template name="resize-image">
    <xsl:param name="path"/>
    <xsl:param name="max-width"/>
    <xsl:param name="max-height"/>
    <xsl:param name="file-prefix"/>
    <xsl:param name="file-suffix"/>
    <xsl:param name="quality"/>
    <xsl:param name="crop" select="false()"/>
    <xsl:param name="no-stretch" select="true()"/>
    <xsl:param name="forceResize"/>

    <xsl:variable name="max-width-calc">
      <xsl:choose>
        <xsl:when test="$max-width!=''">
          <xsl:value-of select="$max-width"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>1200</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="max-height-calc">
      <xsl:choose>
        <xsl:when test="$max-height!=''">
          <xsl:value-of select="$max-height"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>1200</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>


    <!--xsl:choose>
           <xsl:when test="starts-with($path,'/images//FreeStock')">
              <xsl:variable name="newPath" select="concat('/images/~ewlocalFreeStock',substring-after($path.'/images//FreeStock'))"/>
              <xsl:value-of select="ew:ResizeImage($newPath,$max-width,$max-height,$file-prefix,$file-suffix,$quality,$no-stretch,$crop)"/>
          </xsl:when>
          <xsl:otherwise>
              <xsl:variable name="newPath" select="$path"/>
              <xsl:value-of select="ew:ResizeImage($newPath,$max-width,$max-height,$file-prefix,$file-suffix,$quality,$no-stretch,$crop)"/>
          </xsl:otherwise>
      </xsl:choose-->
    <xsl:choose>
      <xsl:when test="$forceResize">
        <xsl:value-of select="ew:ResizeImage2($path,$max-width-calc,$max-height-calc,$file-prefix,$file-suffix,$quality,$no-stretch,$crop,1)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="ew:ResizeImage($path,$max-width-calc,$max-height-calc,$file-prefix,$file-suffix,$quality,$no-stretch,$crop)"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!-- ## GET IMAGE FILE PROPERTIES ##############################################################################################-->
  <!--    ew:ImageWidth(string) Function 
            Returns a string, value of image property. 
            The First argument is path of the image -->

  <xsl:template name="get-image-width">
    <xsl:param name="path"/>
    <xsl:value-of select="ew:ImageWidth($path)"/>
  </xsl:template>

  <xsl:template name="get-image-height">
    <xsl:param name="path"/>
    <xsl:value-of select="ew:ImageHeight($path)"/>
  </xsl:template>


  <!-- ## REPLACE STRING ##############################################################################################-->
  <!--    ew:replacestring(string,string,string) Function 
            Returns a string. The First argument is the string to be returned.  The Second argument is a string 
            to be found within the First argument. The Third argument is a string to replace the Second argument 
            from within the First argument -->

  <xsl:template name="replace-string">
    <xsl:param name="text"/>
    <xsl:param name="replace"/>
    <xsl:param name="with"/>
    <xsl:choose>
      <xsl:when test="$text!='' and $replace!=''">
        <xsl:value-of select="ew:replacestring($text,$replace,$with)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$text"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="clean-title">
    <xsl:param name="text"/>
    <xsl:value-of select="ew:cleantitle($text)"/>
  </xsl:template>

  <!-- ## STRING after last occurance ##########################################################################################-->
  <!--    ew:textafterlast(string,string) Function   
            Returns a string, The remainder of the First argument after the last occurance of the Second
            argument from within the First argument.  If the Second argument is not found the First argument will
            be returned entirely -->

  <xsl:template name="textafterlast">
    <xsl:param name="text"/>
    <xsl:param name="lastOccurance"/>
    <xsl:value-of select="ew:textafterlast($text,$lastOccurance)"/>
  </xsl:template>

  <!-- ## TO TITLE CASE ##############################################################################################-->
  <!--    ew:ToTitleCase(string) Function 
            Returns a string. The First argument is the string to be converted To Title Case.-->

  <xsl:template name="totitlecase">
    <xsl:param name="text"/>
    <xsl:value-of select="ew:ToTitleCase($text)"/>
  </xsl:template>

  <xsl:template name="tolowercase">
    <xsl:param name="text"/>
    <xsl:value-of select="translate($text, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')"/>
  </xsl:template>

  <!-- ## REGULAR EXPRESSION TEST ##############################################################################################-->
  <!--    ew:RegexTest(string,string) Function 
            Returns a boolean - i.e. true() or false()
			      text - the string to test the regular expression against
			      pattern - the actual regular expression pattern to use
			      Note that this ignores the case when checking.-->

  <xsl:template name="regextest">
    <xsl:param name="text"/>
    <xsl:param name="pattern"/>
    <xsl:value-of select="ew:RegexTest($text,$pattern)"/>
  </xsl:template>

  <!-- ## TRIM ##############################################################################################-->
  <!--    ew:Trim(string) Function 
          Returns a string. Trims a string of any whitespace
			    text - the string to trim-->

  <xsl:template name="trim">
    <xsl:param name="text"/>
    <xsl:value-of select="ew:Trim($text)"/>
  </xsl:template>

  <xsl:template name="stripNonAlphaNumberic">
    <xsl:param name="text"/>
    <!--  need fullstop and hypen for decimals and negative numbers -->
    <xsl:value-of select="translate($text, translate($text,'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.- ',''),'')"/>
  </xsl:template>

  <!-- ## FORMAT DATE ##########################################################################################-->
  <!--    ew:formatdate(string,string) Function   
            Returns a string, The First argument is the date string you wish to format.  
            The second argument a VB date format (either pre-defined or user-defined).  Note that formats are case-sensitive.-->

  <xsl:template name="formatdate">
    <xsl:param name="date"/>
    <xsl:param name="format"/>
    <xsl:value-of select="ew:formatdate($date,$format)"/>
  </xsl:template>

  <xsl:template name="formatdateculture">
    <xsl:param name="date"/>
    <xsl:param name="format"/>
    <xsl:param name="culture"/>
    <xsl:variable name="culturemapped">
      <xsl:choose>
        <xsl:when test="$culture='en-pr'">
          <xsl:text>en-gb</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$culture"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="ew:formatdate($date,$format,$culturemapped)"/>
  </xsl:template>

  <!-- ## DATE ADD ##########################################################################################-->
  <!--    ew:dateadd(string,integer,string)
          Returns a string (date in XML date format e.g. '2010-11-26');
          'date' is the start date for the addition
          'period' is an interger for the qty to be added
          'unit' is the IntervalType, what you will be adding.
          IntervalType can be:
            •	"d", "D", "Day", "day", "DAY"
            •	"m", "M", "Month", "month", "MONTH"
            •	"y", "Y", "Year", "year", "YEAR"
   -->
  <xsl:template name="dateadd">
    <xsl:param name="date"/>
    <xsl:param name="period"/>
    <xsl:param name="unit"/>
    <xsl:value-of select="ew:dateadd($date,$period,$unit)"/>
  </xsl:template>

  <!-- ## DATE DIFF ##########################################################################################-->
  <!--    ew:datediff(string,string,string) Function  
            Returns a string (of a number), indicating the difference between two dates, given a unit of time (datepart)
            startdate and enddate are the dates you wish to compare
            datepart is the unit of time you wish to compare by.
            It uses VB date parameters as follows:
               "d" - day 
               "y" - year
               "h" - hour
               "n" - minute
               "m" - month
               "q" - quarter? 
               "s" - seconds
               "w" - week
               "ww" - ? 
               "yyyy" - year?
            -->

  <xsl:template name="datediff">
    <xsl:param name="startdate"/>
    <xsl:param name="enddate"/>
    <xsl:param name="datepart"/>
    <xsl:value-of select="ew:datediff($startdate,$enddate,$datepart)"/>
  </xsl:template>

  <!-- ## Get User XML ##########################################################################################-->
  <!--    ew:GetUserXML(number) Function  
            Returns user XML.
			      id - The id of the User to retrieve
  -->

  <xsl:template name="getUserXML">
    <xsl:param name="id"/>
    <xsl:copy-of select="ew:GetUserXML($id)"/>
  </xsl:template>


  <!-- GET VALUE FROM web.config ##########################################################################################-->
  <!-- ew:EonicConfigValue(string,string) Function 
            Returns a string. The First argument is the section from within the web.config.  The Second argument 
            is the key attribute name from the add node that you want the value of -->

  <xsl:template name="getSettings">
    <xsl:param name="sectionName"/>
    <xsl:param name="valueName" />
    <xsl:value-of select="ew:EonicConfigValue($sectionName,$valueName)"/>
  </xsl:template>

  <xsl:template name="getXmlSettings">
    <xsl:param name="sectionName"/>
    <xsl:param name="valueName" />
    <xsl:value-of select="$page/Settings/add[@key=concat($sectionName,'.',$valueName)]/@value"/>
  </xsl:template>


  <xsl:template name="getServerVariable">
    <xsl:param name="valueName" />
    <xsl:value-of select="ew:ServerVariable($valueName)"/>
  </xsl:template>

  <xsl:template name="getSessionVariable">
    <xsl:param name="valueName" />
    <xsl:value-of select="ew:SessionVariable($valueName)"/>
  </xsl:template>

  <!-- CALCULATE SUBSCRIPTION PRICE ##########################################################################################-->
  <!-- ew:SubscriptionPrice(number,string,number,string) Function 
            Retuns a number, the calculated price of a subscription. 
            The First argument is the Price value.  
            The Second argument is the Payment Unit (Day, Month, Year),
            The Third argument is the Duration Length,
            The Fourth argument is the Duration Units (Day, Month, Year) -->

  <xsl:template match="Content" mode="getSubscriptionPrice">
    <xsl:value-of select="format-number(ew:SubscriptionPrice(Prices/Price[@currency=/Page/Cart/@currency]/node(),PaymentUnit/node(),Duration/Length/node(),Duration/Unit/node()),'0.00')"/>
  </xsl:template>

  <!-- RETURN LOCATION VALUES ##########################################################################################-->
  <!-- ew:GetContentLocations(number, boolean?, number?, boolean?) Function 
            Retuns a CSL (Comma Seperated List) of MenuItems id's where the Content is located,
            The Fisrt argument is the id of the Content we want,
            The Second argument is an optional boolean indicates whether you want to return any primary locations as well. Default value is false.
            The Third argument is an optional integer indicating a page that you may wish to omit from the return string. Default value is 0 
            The Fourth argument is an optional boolean indicates whether you want to return hidden/expired pages as well. Default value is false -->

  <xsl:template name="GetContentLocations">
    <xsl:param name="nContentId"/>
    <xsl:param name="bIncludePrimary" select="false()"/>
    <xsl:param name="bExcludeLocations" select="0"/>
    <xsl:param name="bShowHiddenPages" select="false()"/>
    <xsl:value-of select="ew:GetContentLocations($nContentId,$bIncludePrimary,$bExcludeLocations,$bShowHiddenPages)"/>
  </xsl:template>

  <xsl:template name="GetPageIdFromFref">
    <xsl:param name="fRef"/>
    <xsl:value-of select="ew:GetPageIdFromFref($fRef)"/>
  </xsl:template>

  <xsl:template name="DeletePage">
    <xsl:param name="id"/>
    <xsl:value-of select="ew:DeletePage($id)"/>
  </xsl:template>

  <!-- GET PAGE FOREIGN REF ##########################################################################################-->
  <!-- ew:GetPageFref(number) Function 
            Returns the foreign ref for a page
            The First argument is a number, the id of the page. -->
  <xsl:template name="getPageFref">
    <xsl:param name="id"/>
    <xsl:value-of select="ew:GetPageFref($id)"/>
  </xsl:template>


  <!-- CALCULATE SUBSCRIPTION PRICE ##########################################################################################-->
  <!-- ew:SubscriptionPrice(number,string,number,string) Function 
            Retuns a number, the calculated price of a subscription. 
            The First argument is the Price value.  
            The Second argument is the Payment Unit (Day, Month, Year),
            The Third argument is the Duration Length,
            The Fourth argument is the Duration Units (Day, Month, Year) -->

  <xsl:template match="Content" mode="getSubscriptionPrice">
    <xsl:value-of select="format-number(ew:SubscriptionPrice(Prices/Price[@currency=/Page/Cart/@currency]/node(),PaymentUnit/node(),Duration/Length/node(),Duration/Unit/node()),'0.00')"/>
  </xsl:template>


  <xsl:template name="saveFile">
    <xsl:param name="url"/>
    <xsl:param name="path"/>
    <xsl:if test="$url!='' and $path!=''">
      <xsl:value-of select="ew:SaveImage($url,$path)"/>
    </xsl:if>
  </xsl:template>

  <!-- CALCULATE SHIPPING COST ##########################################################################################-->
  <!-- ew:getShippingCosts(number,number,number) Function 
            Retuns a number, for the total shipping cost in the current currency 
            The First argument is the Weight of the item.  
            The Second argument is the Price of the item,
            The Third argument is the quantity to be purchased, -->
  <xsl:template name="getShippingCosts">
    <xsl:param name="weight" select="'0'" />
    <xsl:param name="price" />
    <xsl:copy-of select="ew:getShippingMethods($weight,$price, 1)"/>
  </xsl:template>


  <xsl:template match="select1 | select" mode="getSelectOptions">

    <xsl:choose>
      <xsl:when test="@assembly!=''">
        <xsl:copy-of select="ew:GetSelectOptions(@assembly,@classPath,@methodName)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy-of select="ew:GetSelectOptions(@query)"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <xsl:template name="getSelectOptionsFunction">
    <xsl:param name="query"/>
    <xsl:param name="assembly"/>
    <xsl:param name="classPath"/>
    <xsl:param name="methodName"/>
    <xsl:choose>
      <xsl:when test="$assembly!=''">
        <xsl:copy-of select="ew:GetSelectOptions($assembly,$classPath,$methodName)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy-of select="ew:GetSelectOptions($query)"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template name="CleanHTMLNode">
    <xsl:param name="htmlstring" />
    <xsl:param name="removetags" />
    <xsl:copy-of select="ew:CleanHTMLElement($htmlstring,$removetags)"/>
  </xsl:template>

  <xsl:template name="CleanHTMLNode2">
    <xsl:param name="htmlstring" />
    <xsl:param name="removetags" />
    <xsl:copy-of select="ew:CleanHTMLNode($htmlstring,$removetags)"/>
  </xsl:template>
  
  
  <!--<xsl:template match="select1 | select" mode="getSelectOptions">
    <xsl:variable name="options">
      <options>
        <xsl:choose>
          <xsl:when test="@assembly!=''">
            <xsl:copy-of select="ew:GetSelectOptions(@assembly,@classPath,@methodName)"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:copy-of select="ew:GetSelectOptions(@query)"/>
          </xsl:otherwise>
        </xsl:choose>
       </options>
    </xsl:variable>
    !
    <xsl:copy-of select="ms:node-set($options)" />
    !
    <xsl:for-each select="ms:node-set($options)">
      <option value="{@value}">
        <xsl:value-of select="label"/>
      </option>
    </xsl:for-each> 
  </xsl:template>-->

  <xsl:template name="GetPageXml">
    <xsl:param name="id"/>
    <xsl:param name="xpath"/>
    <xsl:copy-of select="ew:GetPageXml($id,$xpath)"/>
  </xsl:template>

  <xsl:template name="GetContentDetailXml">
    <xsl:param name="id"/>
    <xsl:copy-of select="ew:GetContentDetailXml($id)"/>
  </xsl:template>

  <xsl:template name="SplitToNodeset">
    <xsl:param name="sourceNode"/>
    <xsl:param name="delimiter"/>
    <xsl:param name="counter"/>
    <xsl:variable name="pageCount" select="count($sourceNode/p[node()=$delimiter]) + 1"/>
    <xsl:choose>
      <xsl:when test="$counter = 1 and $pageCount = 1">
        <contentPage pageNo="{$counter}" class="lastpage">
          <xsl:apply-templates select="$sourceNode/node()" mode="cleanXhtml"/>
        </contentPage>
      </xsl:when>
      <xsl:when test="$counter &lt; $pageCount">
        <contentPage pageNo="{$counter}" pageCount="{$pageCount}" testpos="{number($pageCount)-number($counter)+1}">
          <xsl:choose>
            <xsl:when test="$counter=1">
              <xsl:apply-templates select="$sourceNode/p[node()=$delimiter][position()=$counter]/preceding-sibling::*" mode="cleanXhtml"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:apply-templates select="$sourceNode/p[node()=$delimiter][position()=number($counter)]/preceding-sibling::*[not(following-sibling::*[node()=$delimiter][position()=number($pageCount)-number($counter)+1])]" mode="cleanXhtml"/>
            </xsl:otherwise>
          </xsl:choose>
        </contentPage>
        <xsl:call-template name="SplitToNodeset">
          <xsl:with-param name="sourceNode" select="$sourceNode"/>
          <xsl:with-param name="delimiter" select="$delimiter"/>
          <xsl:with-param name="counter" select="$counter + 1"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <contentPage pageNo="{$counter}" class="lastpage">
          <xsl:apply-templates select="$sourceNode/p[node()=$delimiter][last()]/following-sibling::*" mode="cleanXhtml"/>
        </contentPage>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <xsl:template name="firstWords">
    <xsl:param name="value"/>
    <xsl:param name="count"/>

    <xsl:if test="number($count) >= 1">
      <xsl:value-of select="concat(substring-before($value,' '),' ')"/>
    </xsl:if>
    <xsl:if test="number($count) > 1">
      <xsl:variable name="remaining" select="substring-after($value,' ')"/>
      <xsl:if test="string-length($remaining) > 0">
        <xsl:call-template name="firstWords">
          <xsl:with-param name="value" select="$remaining"/>
          <xsl:with-param name="count" select="number($count)-1"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <xsl:template name="word-count">
    <xsl:param name="data"/>
    <xsl:param name="num"/>
    <xsl:variable name="newdata" select="$data"/>
    <xsl:variable name="remaining" select="substring-after($newdata,' ')"/>

    <xsl:choose>
      <xsl:when test="$remaining">
        <xsl:call-template name="word-count">
          <xsl:with-param name="data" select="$remaining"/>
          <xsl:with-param name="num" select="$num+1"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:when test="$num = 1">
        no words...
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$num"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="*" mode="writeNodes">
    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>


  <!-- The string to URL-encode.
       Note: By "iso-string" we mean a Unicode string where all
       the characters happen to fall in the ASCII and ISO-8859-1
       ranges (32-126 and 160-255) -->
  <xsl:param name="iso-string" select="'&#161;Hola, C&#233;sar!'"/>

  <!-- Characters we'll support.
       We could add control chars 0-31 and 127-159, but we won't. -->
  <xsl:variable name="ascii"> !"#$%&amp;'()*+,-./0123456789:;&lt;=&gt;?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~</xsl:variable>
  <xsl:variable name="latin1">&#160;&#161;&#162;&#163;&#164;&#165;&#166;&#167;&#168;&#169;&#170;&#171;&#172;&#173;&#174;&#175;&#176;&#177;&#178;&#179;&#180;&#181;&#182;&#183;&#184;&#185;&#186;&#187;&#188;&#189;&#190;&#191;&#192;&#193;&#194;&#195;&#196;&#197;&#198;&#199;&#200;&#201;&#202;&#203;&#204;&#205;&#206;&#207;&#208;&#209;&#210;&#211;&#212;&#213;&#214;&#215;&#216;&#217;&#218;&#219;&#220;&#221;&#222;&#223;&#224;&#225;&#226;&#227;&#228;&#229;&#230;&#231;&#232;&#233;&#234;&#235;&#236;&#237;&#238;&#239;&#240;&#241;&#242;&#243;&#244;&#245;&#246;&#247;&#248;&#249;&#250;&#251;&#252;&#253;&#254;&#255;</xsl:variable>

  <!-- Characters that usually don't need to be escaped -->
  <xsl:variable name="safe">!'()*-.0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz~/\</xsl:variable>

  <xsl:variable name="hex" >0123456789ABCDEF</xsl:variable>

  <xsl:template name="url-encode">
    <xsl:param name="str"/>
    <xsl:if test="$str">
      <xsl:variable name="first-char" select="substring($str,1,1)"/>
      <xsl:choose>
        <xsl:when test="contains($safe,$first-char)">
          <xsl:value-of select="$first-char"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="codepoint">
            <xsl:choose>
              <xsl:when test="contains($ascii,$first-char)">
                <xsl:value-of select="string-length(substring-before($ascii,$first-char)) + 32"/>
              </xsl:when>
              <xsl:when test="contains($latin1,$first-char)">
                <xsl:value-of select="string-length(substring-before($latin1,$first-char)) + 160"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:message terminate="no">Warning: string contains a character that is out of range! Substituting "?".</xsl:message>
                <xsl:text>63</xsl:text>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:variable>
          <xsl:variable name="hex-digit1" select="substring($hex,floor($codepoint div 16) + 1,1)"/>
          <xsl:variable name="hex-digit2" select="substring($hex,$codepoint mod 16 + 1,1)"/>
          <xsl:value-of select="concat('%',$hex-digit1,$hex-digit2)"/>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:if test="string-length($str) &gt; 1">
        <xsl:call-template name="url-encode">
          <xsl:with-param name="str" select="substring($str,2)"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <xsl:template name="bundle-js">
    <xsl:param name="comma-separated-files"/>
    <xsl:param name="bundle-path"/>
    <xsl:call-template name="render-js-files">
      <xsl:with-param name="list" select="ew:BundleJS($comma-separated-files,$bundle-path)"/>
    </xsl:call-template>
  </xsl:template>

  <xsl:template name="render-js-files">
    <xsl:param name="list" />
    <xsl:variable name="seperator" select="','"/>
    <xsl:variable name="newlist" select="concat(normalize-space($list),$seperator)" />
    <xsl:variable name="first" select="substring-before($newlist, $seperator)" />
    <xsl:variable name="remaining" select="substring-after($newlist, $seperator)" />
    <xsl:if test="$first!=''">
      <script type="text/javascript" src="{$first}{$bundleVersion}">/* */</script>
      <xsl:call-template name="render-js-files">
        <xsl:with-param name="list" select="$remaining" />
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="bundle-css">
    <xsl:param name="comma-separated-files"/>
    <xsl:param name="bundle-path"/>
    <xsl:call-template name="render-css-files">
      <xsl:with-param name="list" select="ew:BundleCSS($comma-separated-files,$bundle-path)"/>


      <xsl:with-param name="ie8mode">

        <xsl:if test="(contains($userAgent, 'MSIE 7.0') or contains($userAgent, 'MSIE 8.0') or contains($userAgent, 'MSIE 9.0'))">
          <xsl:text>1</xsl:text>

        </xsl:if>
      </xsl:with-param>

    </xsl:call-template>
  </xsl:template>

  <xsl:template name="render-css-files">
    <xsl:param name="list" />
    <xsl:param name="ie8mode"/>
    <xsl:variable name="seperator" select="','"/>
    <xsl:variable name="newlist" select="concat(normalize-space($list),$seperator)" />
    <xsl:variable name="first" select="substring-before($newlist, $seperator)" />
    <xsl:variable name="remaining" select="substring-after($newlist, $seperator)" />
    <xsl:if test="$first!=''">
      <xsl:choose>
        <xsl:when test="(contains($userAgent, 'MSIE 7.0') or contains($userAgent, 'MSIE 8.0') or contains($userAgent, 'MSIE 9.0'))">
          <xsl:if test="$ie8mode='1'">
            <link rel="stylesheet" type="text/css" href="{$first}" />
          </xsl:if>
          <xsl:call-template name="render-css-files">
            <xsl:with-param name="list" select="$remaining" />
            <xsl:with-param name="ie8mode" select="'1'" />
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <link rel="stylesheet" type="text/css" href="{$first}{$bundleVersion}" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>


  <xsl:template match="Page" mode="inlinePopupSingle">
  </xsl:template>

  <xsl:template match="Page" mode="addSingleModule">
  </xsl:template>

  <xsl:template match="Page" mode="addModule">
    <xsl:param name="text"/>
    <xsl:param name="position"/>
    <xsl:param name="class"/>
    <xsl:choose>
      <xsl:when test="/Page/Contents/Content[@position = $position]">
        <xsl:apply-templates select="/Page/Contents/Content[@type='Module' and @position = $position]" mode="displayModule"/>
      </xsl:when>
      <xsl:otherwise>
        <!-- if no contnet, need a space for the compiling of the XSL. -->
        <xsl:text>&#160;</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template match="Page[@cssFramework='bs3']" mode="addModule">
    <xsl:param name="text"/>
    <xsl:param name="position"/>
    <xsl:param name="class"/>
    <!-- THIS IS OVERRIDDEN IN ADMIN MODE BY TEMPLATE IN ADMINWYSIWYG-->
    <xsl:choose>
      <xsl:when test="$position='header' or $position='footer' or ($position='column1' and @layout='Modules_1_column')">
        <xsl:for-each select="/Page/Contents/Content[@type='Module' and @position = $position]">
          <section class="wrapper-sm {@background}">
            <xsl:attribute name="class">
              <xsl:text>wrapper-sm </xsl:text>
              <xsl:value-of select="@background"/>
              <xsl:apply-templates select="." mode="hideScreens" />
              <xsl:if test="@marginBelow='false'">
                <xsl:text> margin-bottom-0 </xsl:text>
              </xsl:if>
            </xsl:attribute>
            <!--<xsl:if test="@data-stellar-background-ratio!='0'">
              <xsl:attribute name="data-stellar-background-ratio">
                <xsl:value-of select="(@data-stellar-background-ratio div 10)"/> test2
              </xsl:attribute>
            </xsl:if>-->
            <xsl:if test="@backgroundImage!=''">
                <xsl:attribute name="style">
                  background-image: url('<xsl:value-of select="@backgroundImage"/>');
                </xsl:attribute>
            </xsl:if>
            <xsl:choose>
              <xsl:when test="@fullWidth='true'">
                <div class="fullwidthContainer">
                  <xsl:apply-templates select="." mode="displayModule"/>
                  <xsl:text> </xsl:text>
                </div>
              </xsl:when>
              <xsl:otherwise>
                <div class="{$class}">
                  <xsl:apply-templates select="." mode="displayModule"/>
                  <xsl:text> </xsl:text>
                </div>
              </xsl:otherwise>
            </xsl:choose>
          </section>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
    
        <xsl:choose>
          <xsl:when test="/Page/Contents/Content[@position = $position]">
            <xsl:choose>
              <xsl:when test="@backgroundImage!=''">
                  <div>
                    <xsl:attribute name="style">
                      background-image: url('<xsl:value-of select="@backgroundImage"/>');
                    </xsl:attribute>
                     <xsl:apply-templates select="/Page/Contents/Content[@type='Module' and @position = $position]" mode="displayModule" />
                  </div>
              </xsl:when>
              <xsl:otherwise>
                <xsl:apply-templates select="/Page/Contents/Content[@type='Module' and @position = $position]" mode="displayModule" />
              </xsl:otherwise>
            </xsl:choose>
                     </xsl:when>
          <xsl:otherwise>
            <!-- if no contnet, need a space for the compiling of the XSL. -->
            <xsl:text>&#160;</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!--responsive column settings-->

  <xsl:template match="*" mode="responsiveColumns">
    <xsl:param name="defaultCols"/>
    <xsl:variable name="smColsEven">
      <xsl:choose>
        <xsl:when test="@smCol='2'">col-sm-6 </xsl:when>
        <xsl:when test="@smCol='3'">col-sm-4 </xsl:when>
        <xsl:when test="@smCol='4'">col-sm-3 </xsl:when>
        <xsl:when test="@smCol='5'">col-sm-2 5-col </xsl:when>
        <xsl:when test="@smCol='6'">col-sm-2 </xsl:when>
        <xsl:otherwise>sm-single-col </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="mdColsEven">
      <xsl:if test="@mdCol='1'"> </xsl:if>
      <xsl:if test="@mdCol='2'">col-md-6 </xsl:if>
      <xsl:if test="@mdCol='3'">col-md-4 </xsl:if>
      <xsl:if test="@mdCol='4'">col-md-3 </xsl:if>
      <xsl:if test="@mdCol='5'">col-md-2 5-col </xsl:if>
      <xsl:if test="@mdCol='6'">col-md-2 </xsl:if>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="@xsCol='2'">mobile-2-col </xsl:when>
      <xsl:otherwise>mobile-1-col </xsl:otherwise>
    </xsl:choose>
    <xsl:if test="@smCol and @smCol!=''">
      <xsl:value-of select="$smColsEven"/>
    </xsl:if>
    <xsl:choose>
      <xsl:when test="@mdCol and @mdCol!=''">
        <xsl:value-of select="$mdColsEven"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>col-md-</xsl:text>
        <xsl:value-of select="$defaultCols"/>
        <xsl:text> </xsl:text>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text>col-lg-</xsl:text>
    <xsl:value-of select="$defaultCols"/>
    <xsl:text> </xsl:text>
  </xsl:template>

  <xsl:template match="*" mode="unevenColumns">
    <xsl:param name="defaultWidth"/>
    <xsl:variable name="smColsUneven">
      <xsl:choose>
        <xsl:when test="@smCol='2'">
          <xsl:text>col-sm-</xsl:text>
          <xsl:value-of select="$defaultWidth"/>
          <xsl:text> </xsl:text>
        </xsl:when>
        <xsl:when test="@smCol='2equal'">col-sm-6 </xsl:when>
        <xsl:otherwise>sm-single-col </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="mdColsUneven">
      <xsl:if test="@mdCol='2'">
        <xsl:text>col-md-</xsl:text>
        <xsl:value-of select="$defaultWidth"/>
        <xsl:text> </xsl:text>
      </xsl:if>
      <xsl:if test="@mdCol='2equal'">col-md-6 </xsl:if>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="@xsCol='2'">mobile-2-col </xsl:when>
      <xsl:otherwise>mobile-1-col </xsl:otherwise>
    </xsl:choose>
    <xsl:if test="@smCol and @smCol!=''">
      <xsl:value-of select="$smColsUneven"/>
    </xsl:if>
    <xsl:choose>
      <xsl:when test="@mdCol and @mdCol!=''">
        <xsl:value-of select="$mdColsUneven"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>col-md-</xsl:text>
        <xsl:value-of select="$defaultWidth"/>
        <xsl:text> </xsl:text>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text>col-lg-</xsl:text>
    <xsl:value-of select="$defaultWidth"/>
    <xsl:text> </xsl:text>
  </xsl:template>

  <xsl:template match="*" mode="uneven5Columns">
    <xsl:param name="defaultWidth"/>
    <xsl:variable name="smColsUneven">
      <xsl:choose>
        <xsl:when test="@smCol='2'">
          <xsl:text>col-sm-</xsl:text>
          <xsl:value-of select="$defaultWidth"/>
          <xsl:text> </xsl:text>
        </xsl:when>
        <xsl:when test="@smCol='2equal'">col-sm-equal-2 </xsl:when>
        <xsl:otherwise>sm-single-col </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="mdColsUneven">
      <xsl:if test="@mdCol='2'">
        <xsl:text>col-md-</xsl:text>
        <xsl:value-of select="$defaultWidth"/>
        <xsl:text> </xsl:text>
      </xsl:if>
      <xsl:if test="@mdCol='2equal'">col-md-equal-2 </xsl:if>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="@xsCol='2'">mobile-2-col </xsl:when>
      <xsl:otherwise>mobile-1-col </xsl:otherwise>
    </xsl:choose>
    <xsl:if test="@smCol and @smCol!=''">
      <xsl:value-of select="$smColsUneven"/>
    </xsl:if>
    <xsl:choose>
      <xsl:when test="@mdCol and @mdCol!=''">
        <xsl:value-of select="$mdColsUneven"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>col-md-</xsl:text>
        <xsl:value-of select="$defaultWidth"/>
        <xsl:text> </xsl:text>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text>col-lg-</xsl:text>
    <xsl:value-of select="$defaultWidth"/>
    <xsl:text> </xsl:text>
  </xsl:template>

</xsl:stylesheet>

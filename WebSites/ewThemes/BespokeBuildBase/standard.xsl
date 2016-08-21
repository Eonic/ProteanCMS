<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">

  <!-- ######################################## IMPORT ALL COMMON XSL's ########################################### -->

  <xsl:import href="../../../../../ewcommon_v5-1/xsl/CommonImports.xsl"/>
  <xsl:import href="../../../../../ewcommon_v5-1/xsl/cart/newcart-bs.xsl"/>
  <!--<xsl:import href="../../xsl/InstalledModules.xsl"/>-->

  <!-- ############################################ THEME VARIABLES ############################################### -->

  <xsl:variable name="theme">BespokeBuildBase</xsl:variable>
  <xsl:variable name="font-import-base">Lato:300,400,700</xsl:variable>
  <xsl:variable name="headings-font-import">Lato:300,400,700</xsl:variable>
  <xsl:variable name="HomeInfo">false</xsl:variable>
  <xsl:variable name="HomeNav">true</xsl:variable>
  <xsl:variable name="NavFix">true</xsl:variable>
  <xsl:variable name="nav-dropdown">hover</xsl:variable><!--true/false/hover-->
  <xsl:variable name="SideSubWidth">3</xsl:variable>
  <xsl:variable name="SideSubWidthCustom"></xsl:variable>
  <xsl:variable name="themeBreadcrumb">false</xsl:variable>
  <xsl:variable name="themeTitle">true</xsl:variable>
  <xsl:variable name="MatchHeightType" select="'no-matchHeight'"/>
  <xsl:variable name="thWidth">500</xsl:variable>
  <xsl:variable name="thHeight">350</xsl:variable>

  <xsl:variable name="membership">
    <xsl:choose>
      <xsl:when test="$page/User">on</xsl:when>
      <xsl:when test="$page/Contents/Content[@name='UserLogon']">on</xsl:when>
      <xsl:otherwise>off</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="cart">
    <xsl:choose>
      <xsl:when test="$page/Cart">on</xsl:when>
      <xsl:otherwise>off</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="search">
    <xsl:choose>
      <xsl:when test="$page/Search">on</xsl:when>
      <xsl:otherwise>off</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>


  <!-- ########################################## CORE TEMPLATES VARIABLES ######################################## -->

  <xsl:variable name="currentPage" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]"/>
  <xsl:variable name="sectionPage" select="/Page/Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]"/>
  <xsl:variable name="siteURL">
    <xsl:call-template name="getSiteURL"/>
  </xsl:variable>

  <!-- ############################################### THEME CSS's ################################################ -->

  <xsl:template match="Page" mode="siteStyle">
    <xsl:call-template name="bundle-css">
      <xsl:with-param name="comma-separated-files">
        <xsl:text>/ewThemes/</xsl:text>
        <xsl:value-of select="$theme"/>
        <xsl:text>/css/bootstrapBase.less</xsl:text>
      </xsl:with-param>
      <xsl:with-param name="bundle-path">
        <xsl:text>~/Bundles/</xsl:text>
        <xsl:value-of select="$theme"/>
      </xsl:with-param>
    </xsl:call-template>

    <xsl:if test="$font-import-base!='none'">
      <link href='http://fonts.googleapis.com/css?family={$font-import-base}' rel='stylesheet' type='text/css' />
    </xsl:if>
    <xsl:if test="$headings-font-import!='none'">
      <link href='http://fonts.googleapis.com/css?family={$headings-font-import}' rel='stylesheet' type='text/css' />
    </xsl:if>
  </xsl:template>

  <!-- ############################################### THEME JS's ################################################ -->

  <xsl:template match="Page" mode="siteJs">
    <xsl:call-template name="bundle-js">
      <xsl:with-param name="comma-separated-files">
        <xsl:text>~/ewThemes/</xsl:text>
        <xsl:value-of select="$theme"/>
        <xsl:text>/js-plugins/moduleAnimate/jquery.appear.js,</xsl:text>
        <xsl:text>~/ewThemes/</xsl:text>
        <xsl:value-of select="$theme"/>
        <xsl:text>/js/jasny-bootstrap.min.js,</xsl:text>
        <xsl:text>~/ewThemes/</xsl:text>
        <xsl:value-of select="$theme"/>
        <xsl:text>/js/responsive-tabs.js,</xsl:text>
        <xsl:text>~/ewThemes/</xsl:text>
        <xsl:value-of select="$theme"/>
        <xsl:text>/js/bootstrap-hover-dropdown.min.js,</xsl:text>
        <xsl:text>~/ewThemes/</xsl:text>
        <xsl:value-of select="$theme"/>
        <xsl:text>/js/theme-specific.js,</xsl:text>
        <xsl:text>~/ewThemes/</xsl:text>
        <xsl:value-of select="$theme"/>
        <xsl:text>/js/jasny-bootstrap.min.js,</xsl:text>
        <xsl:text>~/ewCommon/js/newcart.js</xsl:text>
      </xsl:with-param>
      <xsl:with-param name="bundle-path">
        <xsl:text>~/Bundles/site</xsl:text>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <!-- ############################################ BOX STYLES ############################################### -->

  <xsl:template match="*" mode="siteBoxStyles">
    <xsl:param name="value" />
    <!-- EXAMPLE BESPOKE BOX-->
    <div data-value="panel-action">
      <div class="panel panel-action">
        <div class="panel-heading">Panel Action</div>
        <div class="panel-body">
          <xsl:text> </xsl:text>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='Module']" mode="themeModuleExtras">
    <xsl:if test="@modAnim!=''">
      <xsl:attribute name="data-modAnim">
        <xsl:value-of select="@modAnim"/>
      </xsl:attribute>
      <xsl:attribute name="data-modAnimDelay">
        <xsl:value-of select="@modAnimDelay"/>
      </xsl:attribute>
    </xsl:if>
  </xsl:template>
  <!-- ############################################ LAYOUT BG STYLES ############################################### -->

  <xsl:template match="*[ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="siteBGStyles">
    <xsl:param name="value" />
    <!-- EXAMPLE BESPOKE BOX-->
    <option value="darkBG">
      <xsl:if test="$value='darkBG'">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:text>dark background</xsl:text>
    </option>
  </xsl:template>

  <!-- TinyMCE styles  - leave empty, overwrite as needed per site Example Follows www.tinymce.com/tryit/custom_formats.php -->

  <xsl:template match="textarea" mode="tinymceStyles">
    style_formats: [
    {title: 'Headers', items: [
    {title: 'h1', block: 'h1'},
    {title: 'h2', block: 'h2'},
    {title: 'h3', block: 'h3'},
    {title: 'h4', block: 'h4'},
    {title: 'h5', block: 'h5'},
    {title: 'h6', block: 'h6'}
    ]},

    {title: 'Inline', items: [
    {title: 'Bold', inline: 'b', icon: 'bold'},
    {title: 'Italic', inline: 'i', icon: 'italic'},
    {title: 'Underline', inline: 'span', styles : {textDecoration : 'underline'}, icon: 'underline'},
    {title: 'Strikethrough', inline: 'span', styles : {textDecoration : 'line-through'}, icon: 'strikethrough'},
    {title: 'Superscript', inline: 'sup', icon: 'superscript'},
    {title: 'Subscript', inline: 'sub', icon: 'subscript'},
    {title: 'Code', inline: 'code', icon: 'code'},
    ]},

    {title: 'Blocks', items: [
    {title: 'Paragraph', block: 'p'},
    {title: 'Blockquote', block: 'blockquote'},
    {title: 'Div', block: 'div'},
    {title: 'Pre', block: 'pre'}
    ]},

    {title: 'Alignment', items: [
    {title: 'Left', block: 'div', styles : {textAlign : 'left'}, icon: 'alignleft'},
    {title: 'Center', block: 'div', styles : {textAlign : 'center'}, icon: 'aligncenter'},
    {title: 'Right', block: 'div', styles : {textAlign : 'right'}, icon: 'alignright'},
    {title: 'Justify', block: 'div', styles : {textAlign : 'justify'}, icon: 'alignjustify'}
    ]},
    {title: 'Lead', inline: 'span', classes: 'lead'},
    {title: 'Small', inline: 'span', classes: 'small'},
    {title: 'Button', inline: 'span', classes: 'btn btn-default'}
    ],
  </xsl:template>

  <!-- ############################################ IMAGE SIZES ############################################### -->

  <xsl:template match="Content | MenuItem" mode="getThWidth">
    <xsl:value-of select="$thWidth"/>
  </xsl:template>
  <xsl:template match="Content | MenuItem" mode="getThHeight">
    <xsl:value-of select="$thHeight"/>
  </xsl:template>

  <!-- ############################################ PAGE LAYOUT ############################################### -->

  <xsl:template match="Page" mode="header6">
    <xsl:param name="nav-collapse" />
    <xsl:param name="cart-style" />
    <xsl:param name="login-style" />
    <xsl:param name="login-position" />
    <xsl:param name="login-action" />
    <header class="navbar navbar-default header6">
      <xsl:if test="not($adminMode or /Page[@previewMode='true']) and $NavFix='true'">
        <xsl:attribute name="class">navbar navbar-default navbar-fixed-top header6</xsl:attribute>
      </xsl:if>
      <!-- Brand and toggle get grouped for better mobile display -->
      <xsl:if test="$membership='on'">
        <div id="login-form-bar" class="login-slide-bar clearfix">
          <xsl:if test="/Page/User[@id!='']">
            <xsl:attribute name="class">
              <xsl:text>logged-in</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <div class="container">
            <div class="pull-right login-form">
              <xsl:apply-templates select="/" mode="membershipBrief" />
            </div>
          </div>
        </div>
      </xsl:if>
      <!--LOGO-->
      <div class="navbar-header">
        <div class="container">
          <div class="navbar-brand">
            <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
              <xsl:with-param name="type">Image</xsl:with-param>
              <xsl:with-param name="text">Add Logo</xsl:with-param>
              <xsl:with-param name="name">Logo</xsl:with-param>
              <xsl:with-param name="class">navbar-brand</xsl:with-param>
            </xsl:apply-templates>
            <xsl:apply-templates select="/Page/Contents/Content[@name='Logo']" mode="displayBrief">
              <xsl:with-param name="maxWidth" select="'300'"/>
              <xsl:with-param name="maxHeight" select="'200'"/>
            </xsl:apply-templates>
          </div>

          <div class="pull-right">
            <div class="header-tier1 clearfix">
              <div class="header-tier1-inner">
                <!--INFO NAV-->
                <ul class="nav navbar-nav info-nav not-xs">
                  <xsl:if test="$HomeInfo='true'">
                    <li class="first not-xs">
                      <xsl:if test="$currentPage/@name='Home'">
                        <xsl:attribute name="class">first not-xs active </xsl:attribute>
                      </xsl:if>
                      <a href="/">
                        <xsl:if test="$currentPage/@name='Home'">
                          <xsl:attribute name="class">active </xsl:attribute>
                        </xsl:if>
                        Home
                      </a>
                    </li>
                  </xsl:if>
                  <xsl:for-each select="Menu/MenuItem/MenuItem[@name='Information']/MenuItem[not(DisplayName/@exclude='true')]">
                    <li>
                      <xsl:apply-templates select="." mode="menuLink"/>
                    </li>
                  </xsl:for-each>
                </ul>
                <!--LOGON BUTTON (DESKTOP)-->
                <xsl:if test="$membership='on'">
                  <div class="not-xs login-wrapper">
                    <xsl:apply-templates select="/" mode="loginTop">
                      <xsl:with-param name="apply-login-action">
                        <xsl:value-of select="$login-action"/>
                      </xsl:with-param>
                    </xsl:apply-templates>
                  </div>
                </xsl:if>
              </div>
            </div>
            <div class="header-tier2 clearfix">
              <!--NAV TOGGLE (MOBILE)-->
              <button type="button" class="navbar-toggle collapsed" data-toggle="collapse" data-target="#navbar-main" aria-expanded="false">
                <span class="sr-only">Toggle navigation</span>
                <span class="icon-bar"></span>
                <span class="icon-bar"></span>
                <span class="icon-bar"></span>
              </button>
              <!--CART-->
              <xsl:if test="$cart='on'">
                <xsl:apply-templates select="/" mode="cartBrief">
                  <xsl:with-param name="apply-cart-style">
                    <xsl:value-of select="$cart-style"/>
                  </xsl:with-param>
                </xsl:apply-templates>
              </xsl:if>
              <!--MEMBERSHIP (MOBILE)-->
              <xsl:if test="$membership='on'">
                <div class="xs-login xs-only">
                  <xsl:apply-templates select="/" mode="loginTopxs" >
                    <xsl:with-param name="apply-login-action">
                      <xsl:value-of select="$login-action"/>
                    </xsl:with-param>
                  </xsl:apply-templates>
                </div>
              </xsl:if>
              <!--SEACH (DESKTOP)-->
              <xsl:if test="$search='on'">
                <div class="not-xs search-wrapper">
                  <xsl:apply-templates select="/" mode="searchBrief"/>
                </div>
              </xsl:if>
              <!--SEARCH (MOBILE)-->
              <div class="xs-only xs-search search-slide-btn pull-right">
                <xsl:apply-templates select="/" mode="searchBriefxs"/>
              </div>
              <!--STRAPLINE-->
              <div class="strapline">
                <xsl:apply-templates select="Contents/Content[@name='Strapline']" mode="displayBrief"/>
                <xsl:apply-templates select="/Page" mode="addSingleModule">
                  <xsl:with-param name="text">Add Strapline</xsl:with-param>
                  <xsl:with-param name="position">Strapline</xsl:with-param>
                  <xsl:with-param name="class">strapline</xsl:with-param>
                </xsl:apply-templates>
              </div>
            </div>
          </div>
        </div>
      </div>
      <!--SEACH BAR (MOBILE)-->
      <div class="slide-down-search xs-only" role="menu" aria-labelledby="dLabel">
        <div class="container">
          <span>
            <xsl:apply-templates select="/" mode="searchBrief" />
          </span>
        </div>
      </div>
      <!-- MAIN MENU -->
      <nav class="collapse navbar-collapse" id="navbar-main">
        <div class="container">
          <ul class="nav navbar-nav main-nav">
            <xsl:if test="$HomeNav='true'">
              <li class="first">
                <xsl:if test="$currentPage/@name='Home'">
                  <xsl:attribute name="class">first active </xsl:attribute>
                </xsl:if>
                <a href="/">
                  <xsl:if test="$currentPage/@name='Home'">
                    <xsl:attribute name="class">active </xsl:attribute>
                  </xsl:if>
                  Home
                </a>
              </li>
            </xsl:if>
            <xsl:if test="$HomeInfo='true'">
              <li class="first xs-only">
                <xsl:if test="$currentPage/@name='Home'">
                  <xsl:attribute name="class">first active xs-only </xsl:attribute>
                </xsl:if>
                <a href="/">
                  <xsl:if test="$currentPage/@name='Home'">
                    <xsl:attribute name="class">active xs-only </xsl:attribute>
                  </xsl:if>
                  Home
                </a>
              </li>
            </xsl:if>
            <xsl:choose>
              <xsl:when test="$nav-dropdown='true'">
                <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Information' and @name!='Footer']" mode="mainmenudropdown"/>
              </xsl:when>
              <xsl:when test="$nav-dropdown='hover'">
                <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Information' and @name!='Footer']" mode="mainmenudropdown"><xsl:with-param name="hover">true</xsl:with-param></xsl:apply-templates>
              </xsl:when>
              <xsl:otherwise>
                <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Information' and @name!='Footer']" mode="mainmenu"/>
              </xsl:otherwise>
            </xsl:choose>
          </ul>
          <!--~~~~~~~~~~~~~~ XS INFO MENU ~~~~~~~~~~~~~~ -->
          <xsl:if test="Menu/MenuItem/MenuItem[@name='Information']/MenuItem">
            <ul class="nav navbar-nav info-nav xs-only">
              <xsl:if test="$HomeInfo='true'">
                <li class="first not-xs">
                  <xsl:if test="$currentPage/@name='Home'">
                    <xsl:attribute name="class">first not-xs active </xsl:attribute>
                  </xsl:if>
                  <a href="/">
                    <xsl:if test="$currentPage/@name='Home'">
                      <xsl:attribute name="class">active </xsl:attribute>
                    </xsl:if>
                    Home
                  </a>
                </li>
              </xsl:if>
              <xsl:for-each select="Menu/MenuItem/MenuItem[@name='Information']/MenuItem[not(DisplayName/@exclude='true')]">
                <li>
                  <xsl:apply-templates select="." mode="menuLink"/>
                </li>
              </xsl:for-each>
            </ul>
          </xsl:if>
        </div>
      </nav>
    </header>
  </xsl:template>

  <!-- ############################################ CART ############################################### -->
  <xsl:template match="/" mode="cartBrief">
    <div id="cartBrief">
      <div class="cartinfo">
        <a href="{$siteURL}{$currentPage/@url}?cartCmd=Cart" title="Click here to checkout" role="button">
          <div class="cart-icon">
            <i class="fa fa-shopping-basket">
              <xsl:text> </xsl:text>
            </i>
            <xsl:choose>
              <xsl:when test="/Page/Cart/Order/@itemCount &gt; 0">
                <span id="itemCount">
                  <xsl:value-of select="/Page/Cart/Order/@itemCount"/>
                </span>
              </xsl:when>
              <xsl:otherwise>
                <span id="itemCount">0</span>
              </xsl:otherwise>
            </xsl:choose>
          </div>
          <div class="cart-text not-xs">
            <xsl:choose>
              <xsl:when test="/Page/Cart/Order/@itemCount &gt; 0">
                <div id="itemTotal">
                  <xsl:value-of select="/Page/Cart/@currencySymbol"/>
                  <xsl:value-of select="/Page/Cart/Order/@total"/>
                </div>
              </xsl:when>
              <xsl:otherwise>
                <div id="itemTotal" class="">
                  <xsl:value-of select="/Page/Cart/@currencySymbol"/>
                  <xsl:text>0.00</xsl:text>
                </div>
              </xsl:otherwise>
            </xsl:choose>
            <div class="text-link-cart">view cart</div>
          </div>
        </a>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="/" mode="cartBriefxs">
    <div id="cartBriefxs">
      <div class="dropdown">
        <a data-toggle="dropdown" href="#">
          <i class="fa fa-shopping-basket">
            <xsl:text> </xsl:text>
          </i>
        </a>
        <div class="dropdown-menu" role="menu" aria-labelledby="dLabel">
          <span class="basket-inner">
            <h5>Your basket </h5>
            <p>
              <xsl:choose>
                <xsl:when test="/Page/Cart/Order/@itemCount &gt; 0">
                  <span id="itemCount">
                    <xsl:value-of select="/Page/Cart/Order/@itemCount"/>
                    <span id="itemCountLabel">
                      &#160;item<xsl:if test="/Page/Cart/Order/@itemCount &gt; 1">s</xsl:if>&#160;
                    </span>
                  </span>
                  <span id="itemTotal" class="pull-right">
                    <xsl:value-of select="/Page/Cart/@currencySymbol"/>
                    <xsl:value-of select="/Page/Cart/Order/@total"/>
                  </span>
                </xsl:when>
                <xsl:otherwise>
                  <span id="itemCount">
                    <xsl:text>0&#160;</xsl:text>
                    <span id="itemCountLabel">
                      <xsl:text>items</xsl:text>
                    </span>
                  </span>
                  <span id="itemTotal">
                    <xsl:value-of select="/Page/Cart/@currencySymbol"/>
                    <xsl:text>0.00</xsl:text>
                  </span>
                </xsl:otherwise>
              </xsl:choose>
            </p>
            <a class=""  href="{$siteURL}{$currentPage/@url}?cartCmd=Cart" title="Click here to checkout" role="button">
              View basket&#160;<i class="fa fa-chevron-right">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </span>
        </div>
      </div>
    </div>
  </xsl:template>

  <!-- ############################################ SEARCH ############################################### -->
  <xsl:template match="/" mode="searchBrief">
    <div class="searchBrief">
      <form method="post" action="/information/search" id="searchInputxs" class="ewXform">
        <input type="hidden" name="searchMode" value="REGEX" />
        <input type="hidden" name="contentType" value="Product" />
        <input type="hidden" name="searchFormId" value="8923" />
        <input type="text" class="CTAsearch" name="searchString" id="searchStringxs" value="" placeholder="Search" />
        <button type="submit" class="btn btn-sm btn-primary CTAsearch_button" name="Search" value="Submit">
          <i class="fa fa-search">
            <xsl:text> </xsl:text>
          </i>
          <span class="sr-only">Search</span>
        </button>
      </form>
    </div>
  </xsl:template>

  <xsl:template match="/" mode="searchBriefxs">
    <a href="#" class="search-xs-btn">
      <i class="fa fa-search fa-2x">
        <xsl:text> </xsl:text>
      </i>
      <span class="sr-only">Expand search section</span>
    </a>
  </xsl:template>

  <!-- ############################################ LOGIN ############################################### -->


  <xsl:template match="/" mode="loginTop">
    <xsl:param name="apply-login-action" />
    <div id="loginTop" class="login-slide">

      <div id="signin">
        <a class="loginText login-btn">
          <xsl:if test="/Page/User[@id!='']">
            <xsl:attribute name="class">
              <xsl:text>loginText login-btn logged-in-btn</xsl:text>
            </xsl:attribute>
          </xsl:if>
          Login
        </a>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="/" mode="loginTopxs">
    <xsl:param name="apply-login-action" />
    <div id="loginBriefxs" class="login-slide">
      <div id="signinxs">
        <a class="loginText login-small">
          <i class="fa fa-user fa-2x">
            <xsl:text> </xsl:text>
          </i>
        </a>
      </div>
    </div>
  </xsl:template>

  <!-- ########################################## MEMBERSHIP TEMPLATES ############################################ -->
  <!-- Template to display username when logged in -->
  <xsl:template match="/" mode="loggedIn">
    <div class="cartBriefWelcome">
      <xsl:text>Welcome:&#160;</xsl:text>
      <strong>
        <xsl:choose>
          <xsl:when test="/Page/User/FirstName!=''">
            <!--<xsl:value-of select="/Page/User/FirstName/node()"/>&#160;<xsl:value-of select="/Page/User/LastName/node()"/>&#160;-->
            <xsl:value-of select="/Page/User/FirstName/node()"/>&#160;
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="/Page/User/@name"/>
          </xsl:otherwise>
        </xsl:choose>
      </strong>
    </div>
  </xsl:template>

  <!-- -->
  <xsl:template match="/" mode="membershipBrief">
    <xsl:choose>
      <xsl:when test="/Page/User">
        <div class="userName">
          <xsl:text>Hello, </xsl:text>
          <xsl:choose>
            <xsl:when test="/Page/User/FirstName!=''">
              <xsl:value-of select="/Page/User/FirstName/node()"/>&#160;<xsl:value-of select="/Page/User/LastName/node()"/>&#160;
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="/Page/User/@name"/>&#160;
            </xsl:otherwise>
          </xsl:choose>
          <a href="/?ewCmd=LogOff" title="Logout" class="btn btn-default btn-sm principle" type="button">Logout</a>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='UserLogon']" mode="loginBrief"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[@type='xform' and @name='UserLogon']" mode="loginBrief">
    <form method="post" action="" id="UserLogon" name="UserLogon" onsubmit="form_check(this)" class="ewXform" xmlns="http://www.w3.org/1999/xhtml">
      <xsl:if test="alert">
        <!--<xsl:apply-templates select="alert" mode="xform"/>-->
        <div class="alert alert-warning pull-left">
          <i class="fa fa-exclamation-triangle">
            <xsl:text> </xsl:text>
          </i>
          <xsl:text> </xsl:text>
          <xsl:value-of select="descendant-or-self::alert"/>
        </div>
      </xsl:if>
      <div class="username">
        <label for="cUserName">Email: </label>
        <input type="text" name="cUserName" id="cUserName" class="textbox required" value="" placeholder="username" onfocus="if (this.value=='Please enter Email') {this.value=''}"/>
      </div>
      <div class="password">
        <label for="cPassword">Password: </label>
        <input type="password" name="cPassword" id="cPassword" placeholder="password" class="textbox password required"/>
      </div>
      <div class="submit">
        <button type="ewSubmit" name="ewSubmit" value="Login" class="loginButton button btn btn-default principle">Login</button>
      </div>
    </form>

  </xsl:template>


  <!--################## FOOTER ################## -->
  <xsl:template match="Page" mode="footer1">
    <div id="pagefooter" class="Site clearfix">
      <div class="footer-inner">
        <div class="clearfix footer-main">
          <div class="container">
            <xsl:if test="Menu/MenuItem/MenuItem[@name='Footer']/MenuItem">
              <ul class="nav nav-pills footer-nav hidden-print">
                <xsl:for-each select="Menu/MenuItem/MenuItem[@name='Footer']/MenuItem[not(DisplayName/@exclude='true')]">
                  <li>
                    <xsl:if test="position()=last()">
                      <xsl:attribute name="class">last</xsl:attribute>
                    </xsl:if>
                    <xsl:apply-templates select="." mode="menuLink"/>
                  </li>
                </xsl:for-each>
              </ul>
            </xsl:if>
            <div id="sitefooter">
              <xsl:apply-templates select="/Page" mode="addModule">
                <xsl:with-param name="text">Add Module</xsl:with-param>
                <xsl:with-param name="position">sitefooter</xsl:with-param>
                <xsl:with-param name="class">footer-main</xsl:with-param>
              </xsl:apply-templates>
            </div>
          </div>
        </div>
        <div class="clearfix footer-utility">
          <div class="container">
            <div class="clearfix footer-utility-inner">
              <div id="copyright" class="pull-left">
                <xsl:apply-templates select="/Page" mode="addModule">
                  <xsl:with-param name="text">Copyright</xsl:with-param>
                  <xsl:with-param name="position">copyright</xsl:with-param>
                  <xsl:with-param name="class">pull-left</xsl:with-param>
                </xsl:apply-templates>
              </div>
              <div class="pull-right credit">
                <xsl:apply-templates select="/" mode="developerLink"/>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </xsl:template>
  <!-- ############################################# PLACEHOLDERS ############################################### -->
  <xsl:template match="input[ancestor::Content[@type='Module']] |  textarea[ancestor::Content[@type='Module']] | select1[ancestor::Content[@type='Module']] | upload[ancestor::Content[@type='Module']]" mode="getInlineHint">
    <xsl:variable name="label_low" select="translate(label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
    <xsl:value-of select="$label_low"/>
  </xsl:template>
  <!-- ############################################# BESPOKE ############################################### -->

</xsl:stylesheet>
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">

  <!-- ######################################## IMPORT ALL COMMON XSL's ########################################### -->

  <xsl:import href="../../ptn/core/core.xsl"/>
  <xsl:import href="../../ptn/admin/admin-wysiwyg.xsl"/>
  <xsl:import href="../../ptn/modules/modules.xsl"/>
  <xsl:import href="../../xsl/InstalledModules.xsl"/>
  <xsl:import href="custom-box-styles.xsl"/>
  <xsl:import href="layout-templates/header.xsl"/>


  <!-- ############################################ THEME VARIABLES ############################################### -->

  <xsl:variable name="theme">ptn-base</xsl:variable>
  <!--menu below header-->
  <xsl:variable name="header-layout">header-basic</xsl:variable>
  <!--menu within header-->
  <!--<xsl:variable name="header-layout">header-menu-right</xsl:variable>-->

  <xsl:variable name="font-import-base">Lato:300,400,700</xsl:variable>
  <xsl:variable name="headings-font-import">Lato:300,400,700</xsl:variable>
  <xsl:variable name="HomeInfo">false</xsl:variable>
  <xsl:variable name="HomeNav">true</xsl:variable>
  <xsl:variable name="NavFix">false</xsl:variable>
  <xsl:variable name="nav-dropdown">false</xsl:variable>
  <!--true/false/hover-->
  <xsl:variable name="SideSubWidth">3</xsl:variable>
  <xsl:variable name="SideSubWidthCustom"></xsl:variable>
  <xsl:variable name="themeBreadcrumb">false</xsl:variable>
  <xsl:variable name="themeTitle">true</xsl:variable>
  <xsl:variable name="MatchHeightType" select="'matchHeight'"/>
  <xsl:variable name="thWidth">500</xsl:variable>
  <xsl:variable name="thHeight">350</xsl:variable>
  <!-- forced on, needs fixing-->
  <xsl:variable name="membership">
    <xsl:choose>
      <xsl:when test="$page/User">on</xsl:when>
      <xsl:when test="$page/Contents/Content[@name='UserLogon']">on</xsl:when>
      <xsl:otherwise>on</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="cart">
    <xsl:choose>
      <xsl:when test="$page/Cart">on</xsl:when>
      <xsl:otherwise>off</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <!-- forced on, needs fixing-->
  <xsl:variable name="search">
    <xsl:choose>
      <xsl:when test="$page/Search">on</xsl:when>
      <xsl:otherwise>on</xsl:otherwise>
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
        <xsl:text>/themes/</xsl:text>
        <xsl:value-of select="$theme"/>
        <xsl:text>/css/bootstrap.scss</xsl:text>
      </xsl:with-param>
      <xsl:with-param name="bundle-path">
        <xsl:text>~/Bundles/</xsl:text>
        <xsl:value-of select="$theme"/>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:if test="$font-import-base!='none'">
      <link href='//fonts.googleapis.com/css?family={$font-import-base}' rel='stylesheet' type='text/css' />
    </xsl:if>
    <xsl:if test="$headings-font-import!='none'">
      <link href='//fonts.googleapis.com/css?family={$headings-font-import}' rel='stylesheet' type='text/css' />
    </xsl:if>
  </xsl:template>

  <!-- ############################################### THEME JS's ################################################ -->

  <xsl:template match="Page" mode="siteJs">
    <xsl:call-template name="bundle-js">
      <xsl:with-param name="comma-separated-files">
        <xsl:apply-templates select="." mode="commonJsFiles" />
        <xsl:text>~/ewThemes/</xsl:text>
        <xsl:value-of select="$theme"/>
        <xsl:text>/js/jquery.appear.js,</xsl:text>
        <!--<xsl:text>~/ewThemes/</xsl:text>
        <xsl:value-of select="$theme"/>
        <xsl:text>/js/offcanvas.js,</xsl:text>-->
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
        <xsl:text>/js/smoothproducts.js,</xsl:text>
        <xsl:text>~/ewCommon/js/newcart.js</xsl:text>
      </xsl:with-param>
      <xsl:with-param name="bundle-path">
        <xsl:text>~/Bundles/site</xsl:text>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>

  <!-- ############################################ BOX STYLES ############################################### -->

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


  <!-- ############################################ IMAGE SIZES ############################################### -->

  <xsl:template match="Content | MenuItem" mode="getThWidth">
    <xsl:value-of select="$thWidth"/>
  </xsl:template>
  <xsl:template match="Content | MenuItem" mode="getThHeight">
    <xsl:value-of select="$thHeight"/>
  </xsl:template>

  <xsl:template match="Content | MenuItem | Discount | Company" mode="getDisplayWidth">600</xsl:template>
  <xsl:template match="Content | MenuItem | Discount | Company" mode="getDisplayHeight">600</xsl:template>


  <!-- ############################################ CART ############################################### -->
  <xsl:template match="/" mode="cartBrief">
    <div id="cartBrief">
      <div class="cartinfo">
        <a href="{$currentPage/@url}?cartCmd=Cart" title="Click here to checkout" role="button">
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
            <a class=""  href="{$currentPage/@url}?cartCmd=Cart" title="Click here to checkout" role="button">
              View basket&#160;<i class="fa fa-chevron-right">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </span>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="/" mode="cartSimple">
    <div id="cartBrief">
      <div class="cartinfo">
        <a href="{$currentPage/@url}?cartCmd=Cart" title="Click here to checkout" role="button">
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
        </a>
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

  <xsl:template match="/" mode="searchSimple">
    <form method="post" action="/information/search" id="searchInputxs" class="input-group">
      <input type="text" class="form-control CTAsearch" name="searchString" id="searchStringxs" value="" placeholder="Search" />
      <input type="hidden" name="searchMode" value="REGEX" class="d-none" />
      <input type="hidden" name="contentType" value="Product" class="d-none"/>
      <input type="hidden" name="searchFormId" value="8923" class="d-none"/>
      <button type="submit" class="btn btn-outline-primary" name="Search" value="Submit">
        <i class="fa fa-search">
          <xsl:text> </xsl:text>
        </i>
        <span class="sr-only">Search</span>
      </button>
    </form>
  </xsl:template>

  <!-- ############################################ LOGIN ############################################### -->


  <xsl:template match="/" mode="loginTop">
    <div id="signin">
      <a class="loginText login-btn" data-toggle="modal" data-target="#LoginModal">
        <xsl:if test="/Page/User[@id!='']">
          <xsl:attribute name="class">
            <xsl:text>loginText login-btn logged-in-btn</xsl:text>
          </xsl:attribute>
        </xsl:if>
        <xsl:choose>
          <xsl:when test="/Page/User">My Account</xsl:when>
          <xsl:otherwise>Log in</xsl:otherwise>
        </xsl:choose>
      </a>
    </div>
  </xsl:template>

  <xsl:template match="/" mode="loginTopxs">
    <li id="loginBriefxs" >
      <a class="loginText login-small" data-toggle="modal" data-target="#LoginModal">
        <xsl:choose>
          <xsl:when test="/Page/User">My Account</xsl:when>
          <xsl:otherwise>Log in</xsl:otherwise>
        </xsl:choose>
      </a>
    </li>
  </xsl:template>

  <xsl:template match="/" mode="loginSimple">
    <a class="nav-link login-btn" data-toggle="modal" data-target="#LoginModal">
      <xsl:choose>
        <xsl:when test="/Page/User">My Account</xsl:when>
        <xsl:otherwise>Log in</xsl:otherwise>
      </xsl:choose>
    </a>
  </xsl:template>

  <!-- ########################################## MEMBERSHIP TEMPLATES ############################################ -->
  <!-- Template to display username when logged in -->
  <xsl:template match="/" mode="loggedIn">
    <div class="cartBriefWelcome">
      <xsl:text>Welcome:&#160;</xsl:text>
      <strong>
        <xsl:choose>
          <xsl:when test="/Page/User/FirstName!=''">
            <xsl:value-of select="/Page/User/FirstName/node()"/>&#160;
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="/Page/User/@name"/>
          </xsl:otherwise>
        </xsl:choose>
      </strong>
    </div>
  </xsl:template>

  <xsl:template match="/" mode="membershipBrief">
    <xsl:choose>
      <xsl:when test="/Page/User">
        <a href="/?ewCmd=LogOff" title="Logout" class="btn btn-default btn-sm principle" type="button">
          <i class="fa fa-power-off"> </i> Logout
        </a>
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
        </div>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='UserLogon']" mode="loginBrief"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!-- ############################################# BESPOKE ############################################### -->

</xsl:stylesheet>
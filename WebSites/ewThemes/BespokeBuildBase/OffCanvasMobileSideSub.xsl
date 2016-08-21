<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">

  <!-- ######################################## IMPORT ALL COMMON XSL's ########################################### -->

  <xsl:import href="standard.xsl"/>

  <!-- ############################################## OUTPUT TYPE ################################################# -->

  <xsl:output method="html" indent="yes" standalone="yes" omit-xml-declaration="yes" encoding="UTF-8"/>
  <xsl:template match="/">
    <xsl:text disable-output-escaping='yes'>&lt;!DOCTYPE html></xsl:text>
    <xsl:apply-imports/>
  </xsl:template>

  <!-- ############################################ THEME VARIABLES ############################################### -->

  <xsl:variable name="themeLayout">OffCanvasMobileSideSub</xsl:variable>
  <xsl:template match="Page" mode="header6OC">
    <xsl:param name="nav-collapse" />
    <xsl:param name="cart-style" />
    <xsl:param name="login-style" />
    <xsl:param name="login-position" />
    <xsl:param name="login-action" />
    <header class="navbar navbar-default header6 header-oc">
      <xsl:if test="not($adminMode or /Page[@previewMode='true']) and $NavFix='true'">
        <xsl:attribute name="class">navbar navbar-default navbar-fixed-top header6 header-oc</xsl:attribute>
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
              <button type="button" class="navbar-toggle collapsed" data-toggle="offcanvas" data-target="#navbar-main" aria-expanded="false" data-canvas="body">
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
          <!--SEARCH (MOBILE)-->
          <xsl:if test="$search='on'">
            <div class="xs-only search-oc">
              <xsl:apply-templates select="/" mode="searchBrief"/>
            </div>
          </xsl:if>
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
                <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Information' and @name!='Footer']" mode="mainmenudropdown">
                  <xsl:with-param name="hover">true</xsl:with-param>
                </xsl:apply-templates>
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
  <xsl:template match="Page" mode="bodyDisplay">
    <xsl:variable name="nav-padding">
      <xsl:if test="$currentPage/DisplayName[@navpad='false'] and not($cartPage)">nav-no-padding</xsl:if>
    </xsl:variable>

    <div id="mainTable" class="Site activateAppearAnimation {$nav-padding}">
      <!--<xsl:if test="contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 8') or contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 7') or contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 6.0')">
        <xsl:attribute name="class">
          Site ie8 activateAppearAnimation
        </xsl:attribute>
      </xsl:if>-->
      <!--################## HEADER ################## -->

      <a class="sr-only" href="#content">Skip to main content</a>
      <!--<div class="visible-print-block">
        Name of site here
      </div>-->

      <!--~~~~~~~~~~~~~~ HEADER AND MAIN NAV TEMPLATE CALLED FROM TOOLS/LAYOUT.XSL ~~~~~~~~~~~~~~ -->
      <xsl:apply-templates select="." mode="header6OC">
        <xsl:with-param name="nav-collapse">true</xsl:with-param>
      </xsl:apply-templates>


      <!--################## MAIN CONTENT ################## -->
      <div class="container-wrapper side-nav-layout">
        <xsl:if test="not($adminMode or /Page[@previewMode='true']) and $NavFix='true'">
          <xsl:attribute name="class">
            container-wrapper side-nav-layout fixed-nav-content <xsl:value-of select="$nav-padding"/>
          </xsl:attribute>
        </xsl:if>
        <!--~~~~~~~~~~~~~~ breadcrumb ~~~~~~~~~~~~~~ -->
        <xsl:if test="$themeBreadcrumb='true' and not($cartPage)">
          <xsl:if test="not($currentPage/DisplayName[@nonav='true']) and $currentPage/@name!='Home'">
            <section class="wrapper">
              <div class="container">
                <ol class="breadcrumb">
                  <xsl:apply-templates select="Menu/MenuItem" mode="breadcrumb"/>
                </ol>
              </div>
            </section>
          </xsl:if>
        </xsl:if>
        <xsl:choose>
          <!--~~~~~~~~~~~~~~ pages with side nav ~~~~~~~~~~~~~~ -->
          <xsl:when test="(count($sectionPage[@name!='Information' and @name!='Footer']/MenuItem[not(DisplayName/@exclude='true')])&gt;0 and not($currentPage/DisplayName[@nonav='true']) and $currentPage/@name!='Home') and not($cartPage) or $currentPage[parent::MenuItem[@name='Information']/MenuItem] and (count($currentPage[child::MenuItem[not(DisplayName/@exclude='true')]])&gt;0 and not($currentPage/DisplayName[@nonav='true'])) and not($cartPage) or $currentPage[ancestor::MenuItem[@name='Information']/MenuItem] and not($currentPage[parent::MenuItem[@name='Information']/MenuItem]) and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
            <div id="mainLayout" class="pagewidth activateAppearAnimation">
              <div class="container">
                <div class="row">
                  <div class="col-md-{$SideSubWidth}" id="leftCol">
                    <xsl:if test="$SideSubWidthCustom!=''">
                      <xsl:attribute name="style">
                        <xsl:text>width:</xsl:text>
                        <xsl:value-of select="$SideSubWidthCustom" />
                        <xsl:text>%</xsl:text>
                      </xsl:attribute>
                    </xsl:if>
                    <div id="subMenu" class="hidden-xs hidden-sm">
                      <xsl:choose>
                        <xsl:when test="$currentPage[parent::MenuItem[@name='Information']/MenuItem] and (count($currentPage[child::MenuItem[not(DisplayName/@exclude='true')]])&gt;0) or $currentPage[ancestor::MenuItem[@name='Information']/MenuItem] and not($currentPage[parent::MenuItem[@name='Information']/MenuItem])">
                          <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name='Information']/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="submenu"/>
                        </xsl:when>
                        <xsl:when test="$themeLayout='OffCanvasMobileSideSub'">
                          <xsl:apply-templates select="$sectionPage" mode="submenu">
                            <xsl:with-param name="sectionHeading">true</xsl:with-param>
                          </xsl:apply-templates>
                        </xsl:when>
                      </xsl:choose>
                    </div>
                    <div id="LeftNav">
                      <xsl:apply-templates select="/Page" mode="addModule">
                        <xsl:with-param name="text">Add Module</xsl:with-param>
                        <xsl:with-param name="position">LeftNav</xsl:with-param>
                      </xsl:apply-templates>
                    </div>
                  </div>
                  <div class="col-md-{12 - $SideSubWidth}" id="content">
                    <xsl:if test="$SideSubWidthCustom!=''">
                      <xsl:attribute name="style">
                        <xsl:text>width:</xsl:text>
                        <xsl:value-of select="100 - $SideSubWidthCustom" />
                        <xsl:text>%</xsl:text>
                      </xsl:attribute>
                    </xsl:if>
                    <xsl:if test="$themeTitle='true'">
                      <div id="content" class="sr-only">&#160;</div>
                      <div id="mainTitle">
                        <xsl:apply-templates select="/" mode="getMainTitle" />
                      </div>
                    </xsl:if>
                    <xsl:apply-templates select="." mode="mainLayout"/>
                  </div>
                </div>
              </div>
            </div>
          </xsl:when>
          <!--~~~~~~~~~~~~~~ pages with no side nav ~~~~~~~~~~~~~~ -->
          <xsl:otherwise>
            <div id="mainLayout" class="fullwidth activateAppearAnimation">
              <div id="content" class="sr-only">&#160;</div>
              <xsl:if test="not($cartPage) and $currentPage/@name!='Home'">
                <!--<xsl:if test="$currentPage/@name!='Home'">-->
                <xsl:if test="$themeTitle='true'">
                  <section class="wrapper">
                    <div class="container">
                      <div id="mainTitle">
                        <xsl:apply-templates select="/" mode="getMainTitle" />
                      </div>
                    </div>
                  </section>
                </xsl:if>
              </xsl:if>
              <xsl:apply-templates select="." mode="mainLayout">
                <xsl:with-param name="containerClass" select="'container'"/>
              </xsl:apply-templates>
            </div>
          </xsl:otherwise>
        </xsl:choose>
      </div>
      <!--~~~~~~~~~~~~~~ sub nav for xs screens ~~~~~~~~~~~~~~ -->
      <xsl:if test="(count($sectionPage[@name!='Information' and @name!='Footer']/MenuItem[not(DisplayName/@exclude='true')])&gt;0 or ($currentPage[parent::MenuItem[@name='Information' or @name='Footer']/MenuItem] and count($currentPage[child::MenuItem[not(DisplayName/@exclude='true')]])&gt;0)) or ($currentPage[ancestor::MenuItem[@name='Information' or @name='Footer']/MenuItem] and not($currentPage[parent::MenuItem[@name='Information' or @name='Footer']/MenuItem])) and not($currentPage/DisplayName[@nonav='true']) and $currentPage/@name!='Home' and not($cartPage)">
        <div class="xs-only xs-sub-menu hidden-print container visible-xs visible-sm hidden-print xs-sub-menu container">
          <div class="xs-sub-menu-inner">
            <xsl:choose>
              <xsl:when test="count($sectionPage[@name='Information']/MenuItem)&gt;0">
                <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name='Information']/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="submenu">
                  <xsl:with-param name="sectionHeading">true</xsl:with-param>
                </xsl:apply-templates>
              </xsl:when>
              <xsl:otherwise>
                <xsl:apply-templates select="$sectionPage" mode="submenu">
                  <xsl:with-param name="sectionHeading">true</xsl:with-param>
                </xsl:apply-templates>
              </xsl:otherwise>
            </xsl:choose>
          </div>
        </div>
      </xsl:if>
    </div>


    <!--################## FOOTER ################## -->
    <xsl:apply-templates select="." mode="footer1" />

  </xsl:template>
  <!-- -->

</xsl:stylesheet>
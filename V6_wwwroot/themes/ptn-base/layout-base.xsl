<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">

  <!-- ######################################## IMPORT ALL COMMON XSL's ########################################### -->

  <xsl:import href="shared.xsl"/>

  <!-- ############################################## OUTPUT TYPE ################################################# -->

  <xsl:output method="html" indent="yes" standalone="yes" omit-xml-declaration="yes" encoding="UTF-8"/>
  <xsl:template match="/">
    <xsl:text disable-output-escaping='yes'>&lt;!DOCTYPE html></xsl:text>
    <xsl:apply-imports/>
  </xsl:template>

  <!-- ############################################ THEME VARIABLES ############################################### -->

  <xsl:variable name="themeLayout">layout-clean</xsl:variable>

  <xsl:template match="Page" mode="bodyDisplay">
    <xsl:variable name="nav-padding">
      <xsl:if test="$currentPage/DisplayName[@navpad='false'] and not($cartPage)">mt-0</xsl:if>
    </xsl:variable>
    <xsl:variable name="detail-heading">
      <xsl:if test="$page/ContentDetail">detail-heading</xsl:if>
    </xsl:variable>
    <xsl:variable name="home-class">
      <xsl:if test="$currentPage[@id='1']">home-class</xsl:if>
    </xsl:variable>
    <xsl:variable name="cart-class">
      <xsl:if test="$cartPage"> cart-class </xsl:if>
    </xsl:variable>
    <xsl:variable name="layout-class">
      <xsl:if test="$currentPage/DisplayName/@banner='no-banner'"> no-banner-layout</xsl:if>
    </xsl:variable>
    <div id="mainTable" class="Site activateAppearAnimation {$nav-padding}">
      <xsl:if test="$cartPage">
        <xsl:attribute name="class">Site activateAppearAnimation nav-no-padding</xsl:attribute>
      </xsl:if>
      <!--################## HEADER ################## -->

      <a class="skip" href="#content">Skip to main content</a>
      <xsl:if test="($adminMode or /Page/Contents/Content[@position='site-alert']) and $siteAlert='true'">
        <div id="site-alert" class="clearfix">
          <xsl:apply-templates select="/Page" mode="addModule">
            <xsl:with-param name="text">Add Module</xsl:with-param>
            <xsl:with-param name="name">site-alert</xsl:with-param>
            <xsl:with-param name="position">site-alert</xsl:with-param>
          </xsl:apply-templates>
          <xsl:text> </xsl:text>
        </div>
      </xsl:if>
      <!--~~~~~~~~~~~~~~ HEADER ~~~~~~~~~~~~~~ -->
      <xsl:choose>
        <xsl:when test="$header-layout='header-menu-right'">
          <xsl:apply-templates select="." mode="header-menu-right">
            <xsl:with-param name="nav-collapse">false</xsl:with-param>
            <xsl:with-param name="social-links">true</xsl:with-param>
            <xsl:with-param name="containerClass" select="$container"/>
          </xsl:apply-templates>
        </xsl:when>
        <xsl:when test="$header-layout='header-info-above'">
          <xsl:apply-templates select="." mode="header-info-above">
            <xsl:with-param name="nav-collapse">false</xsl:with-param>
            <xsl:with-param name="social-links">false</xsl:with-param>
            <xsl:with-param name="containerClass" select="$container"/>
          </xsl:apply-templates>
        </xsl:when>
        <xsl:when test="$header-layout='header-one-line'">
          <xsl:apply-templates select="." mode="header-one-line">
            <xsl:with-param name="nav-collapse">false</xsl:with-param>
            <xsl:with-param name="social-links">false</xsl:with-param>
            <xsl:with-param name="containerClass">container-fluid</xsl:with-param>
            <xsl:with-param name="cartClass" select="$cart-class"/>
          </xsl:apply-templates>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="." mode="header-menu-below">
            <xsl:with-param name="nav-collapse">false</xsl:with-param>
            <xsl:with-param name="social-links">true</xsl:with-param>
            <xsl:with-param name="containerClass" select="$container"/>
          </xsl:apply-templates>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:if test="$page/ContentDetail and $themeBreadcrumb='true' and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
        <section class="wrapper detail-breadcrumb-wrapper">
          <div class="{$container}">
            <ol class="breadcrumb detail-breadcrumb">
              <xsl:apply-templates select="Menu/MenuItem" mode="breadcrumb"/>
            </ol>
          </div>
        </section>
      </xsl:if>

      <!--~~~~~~~~~~~~~~ MAIN CONTENT ~~~~~~~~~~~~~~ -->
      <div class="container-wrapper {$detail-heading} {$nav-padding} {$home-class} {$cart-class} {$layout-class}">
        <xsl:if test="not($adminMode or /Page[@previewMode='true']) and $NavFix='true'">
          <xsl:attribute name="class">
            <xsl:text>container-wrapper fixed-nav-content </xsl:text>
            <xsl:value-of select="$nav-padding"/>
            <xsl:text> </xsl:text>
            <xsl:value-of select="$detail-heading"/>
            <xsl:text> </xsl:text>
            <xsl:value-of select="$home-class"/>
            <xsl:value-of select="$cart-class"/>
            <xsl:if test="not($cartPage) and $currentPage/@name!='Home'"> mt-0</xsl:if>
            <xsl:if test="$currentPage/DisplayName/@banner='no-banner' or $cartPage or $page/ContentDetail and ($sub-nav='left' or $sub-nav='top')">
              <xsl:text> bannerless-page</xsl:text>
            </xsl:if>
          </xsl:attribute>
        </xsl:if>
        <div id="mainLayout" class="fullwidth activateAppearAnimation">
          <xsl:if test="$currentPage/DisplayName/@banner='no-banner' or $currentPage/@name='Home' or $cartPage or $page/ContentDetail and ($sub-nav='left' or $sub-nav='top')">
            <div id="content" class="visually-hidden">&#160;</div>
          </xsl:if>
          <xsl:if test="not($cartPage) and $currentPage/@name!='Home' and not($page/ContentDetail) and not($currentPage/DisplayName/@banner='no-banner')">
            <xsl:choose>
              <xsl:when test="$currentPage/DisplayName/@banner='img-banner'">
                <div class="image-banner">
                  <div class="container-fluid">
                    <!--<div class="image-banner-inner" id="banner" style="background-image:url({/Page/Contents/Content[@name='Banner']/img/@src})">-->
                    <div class="image-banner-inner" id="banner">
                      <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
                        <xsl:with-param name="type">Image</xsl:with-param>
                        <xsl:with-param name="text">Add Banner Background</xsl:with-param>
                        <xsl:with-param name="name">Banner</xsl:with-param>
                        <xsl:with-param name="class">image-banner-inner</xsl:with-param>
                      </xsl:apply-templates>
                      <xsl:call-template  name="displayResponsiveImage">
                        <xsl:with-param name="crop" select="true()"/>
                        <xsl:with-param name="no-stretch" select="false()"/>
                        <xsl:with-param name="width" select="'2000'"/>
                        <xsl:with-param name="height" select="'350'"/>
                        <xsl:with-param name="max-width-xs" select="'576'"/>
                        <xsl:with-param name="max-height-xs" select="'220'"/>
                        <xsl:with-param name="max-width-sm" select="'768'"/>
                        <xsl:with-param name="max-height-sm" select="'317'"/>
                        <xsl:with-param name="max-width-md" select="'992'"/>
                        <xsl:with-param name="max-height-md" select="'350'" />
                        <xsl:with-param name="max-width-lg" select="'1200'"/>
                        <xsl:with-param name="max-height-lg" select="'350'" />
                        <xsl:with-param name="max-width-xl" select="'1400'"/>
                        <xsl:with-param name="max-height-xl" select="'350'"/>
                        <xsl:with-param name="max-width-xxl" select="'2000'"/>
                        <xsl:with-param name="max-height-xxl" select="'350'"/>
                        <xsl:with-param name="imageUrl" select="/Page/Contents/Content[@name='Banner']/img/@src"/>
                        <xsl:with-param name="altText" select="''"/>
                        <xsl:with-param name="forceResize" select="true()"/>
                        <xsl:with-param name="class" select="'banner-background'"/>
                        <xsl:with-param name="style" select="''"/>
                      </xsl:call-template>
                      <div class="banner-caption">
                        <div class="banner-caption-inner">
                          <xsl:if test="$themeBreadcrumb='true'">
                            <nav aria-label="breadcrumb">
                              <ol class="breadcrumb">
                                <xsl:apply-templates select="Menu/MenuItem" mode="breadcrumb"/>
                              </ol>
                            </nav>
                          </xsl:if>
                          <div id="content" class="visually-hidden">&#160;</div>
                          <div id="mainTitle">
                            <xsl:apply-templates select="/" mode="getMainTitle" />
                          </div>
                          <xsl:if test="$currentPage/Description/node()">
                            <div class="image-banner-info">
                              <xsl:apply-templates select="$currentPage/Description/node()" mode="cleanXhtml"/>
                            </div>
                          </xsl:if>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </xsl:when>
              <xsl:otherwise>
                <div class="intro-banner">
                  <div class="container-fluid">
                    <div class="intro-banner-inner">
                      <div id="mainTitle">
                        <xsl:if test="$themeBreadcrumb='true'">
                          <nav aria-label="breadcrumb">
                            <ol class="breadcrumb">
                              <xsl:apply-templates select="Menu/MenuItem" mode="breadcrumb"/>
                            </ol>
                            <div id="content" class="visually-hidden">&#160;</div>
                          </nav>
                        </xsl:if>
                        <xsl:apply-templates select="/" mode="getMainTitle" />
                        <xsl:if test="$currentPage/Description/node()">
                          <div class="intro-banner-info">
                            <xsl:apply-templates select="$currentPage/Description/node()" mode="cleanXhtml"/>
                          </div>
                        </xsl:if>
                      </div>
                    </div>
                  </div>
                </div>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:if>
          <xsl:text> </xsl:text>
          <xsl:if test="((count($sectionPage[@name!='Info menu' and @name!='Footer']/MenuItem[not(DisplayName/@exclude='true')])&gt;0 and not($currentPage/DisplayName[@nonav='true']) and $currentPage/@name!='Home') and not($cartPage) or $currentPage[parent::MenuItem[@name='Information']/MenuItem] and (count($currentPage[child::MenuItem[not(DisplayName/@exclude='true')]])&gt;0 and not($currentPage/DisplayName[@nonav='true'])) and not($cartPage) or $currentPage[ancestor::MenuItem[@name='Info menu']/MenuItem] and not($currentPage[parent::MenuItem[@name='Info menu']/MenuItem]) and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)) and ($sub-nav='top')">
            <div class="container">
              <button class="btn btn-primary hidden-lg hidden-xl hidden-xxl xs-menu-btn" type="button" data-bs-toggle="collapse" data-bs-target="#topMenuCollapse" aria-expanded="false" aria-controls="topMenuCollapse">
                <xsl:apply-templates select="$sectionPage/@name" mode="cleanXhtml"/> Menu <i class="fas fa-caret-down"> </i>
              </button>
              <div class="collapse dont-collapse-md" id="topMenuCollapse">
                <div id="topMenu">
                  <xsl:choose>
                    <xsl:when test="$currentPage[parent::MenuItem[@name='Info menu']/MenuItem] and (count($currentPage[child::MenuItem[not(DisplayName/@exclude='true')]])&gt;0) or $currentPage[ancestor::MenuItem[@name='Info menu']/MenuItem] and not($currentPage[parent::MenuItem[@name='Info menu']/MenuItem])">
                      <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name='Info menu']/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="submenu">

                        <xsl:with-param name="class">nav-link</xsl:with-param>
                      </xsl:apply-templates>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:apply-templates select="$sectionPage" mode="topmenu">
                        <xsl:with-param name="class">nav-link</xsl:with-param>
                        <xsl:with-param name="overviewLink">self</xsl:with-param>
                      </xsl:apply-templates>
                    </xsl:otherwise>
                  </xsl:choose>
                </div>
              </div>
            </div>
            <div id="content" class="visually-hidden">&#160;</div>
          </xsl:if>
          <xsl:choose>
            <!--~~~~~~~~~~~~~~ pages with side nav ~~~~~~~~~~~~~~ -->
            <xsl:when test="((count($sectionPage[@name!='Info menu' and @name!='Footer']/MenuItem[not(DisplayName/@exclude='true')])&gt;0 and not($currentPage/DisplayName[@nonav='true']) and $currentPage/@name!='Home') and not($cartPage) or $currentPage[parent::MenuItem[@name='Information']/MenuItem] and (count($currentPage[child::MenuItem[not(DisplayName/@exclude='true')]])&gt;0 and not($currentPage/DisplayName[@nonav='true'])) and not($cartPage) or $currentPage[ancestor::MenuItem[@name='Info menu']/MenuItem] and not($currentPage[parent::MenuItem[@name='Info menu']/MenuItem]) and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)) and ($sub-nav='left' or $sub-nav='right')">
              <div class="container">
                <div class="row">
                  <div class="col-lg-{$SideSubWidth}" id="leftCol">
                    <xsl:attribute name="class">
                      <xsl:text>col-lg-</xsl:text>
                      <xsl:value-of select="$SideSubWidth"/>
                      <xsl:if test="$sub-nav='right'">
                        <xsl:text> order-2</xsl:text>
                      </xsl:if>
                    </xsl:attribute>
                    <xsl:if test="$SideSubWidthCustom!=''">
                      <xsl:attribute name="style">
                        <xsl:text>width:</xsl:text>
                        <xsl:value-of select="$SideSubWidthCustom" />
                        <xsl:text>%</xsl:text>
                      </xsl:attribute>
                    </xsl:if>
                    <button class="btn btn-primary hidden-lg hidden-xl hidden-xxl xs-menu-btn" type="button" data-bs-toggle="collapse" data-bs-target="#subMenuCollapse" aria-expanded="false" aria-controls="subMenuCollapse">
                      <xsl:apply-templates select="$sectionPage/@name" mode="cleanXhtml"/> Menu <i class="fas fa-caret-down"> </i>
                    </button>
                    <div class="collapse dont-collapse-md" id="subMenuCollapse">
                      <div id="subMenu">
                        <xsl:choose>
                          <xsl:when test="$currentPage[parent::MenuItem[@name='Info menu']/MenuItem] and (count($currentPage[child::MenuItem[not(DisplayName/@exclude='true')]])&gt;0) or $currentPage[ancestor::MenuItem[@name='Info menu']/MenuItem] and not($currentPage[parent::MenuItem[@name='Info menu']/MenuItem])">
                            <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name='Info menu']/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="submenu">
                              <xsl:with-param name="sectionHeading">true</xsl:with-param>
                              <xsl:with-param name="class">nav-link</xsl:with-param>
                              <xsl:with-param name="level3">true</xsl:with-param>
                            </xsl:apply-templates>
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:apply-templates select="$sectionPage" mode="submenu">
                              <xsl:with-param name="sectionHeading">true</xsl:with-param>
                              <xsl:with-param name="class">nav-link</xsl:with-param>
                              <xsl:with-param name="level3">true</xsl:with-param>
                            </xsl:apply-templates>
                          </xsl:otherwise>
                        </xsl:choose>
                      </div>
                    </div>
                    <xsl:if test="$sub-nav='left'">
                      <div id="content" class="visually-hidden">&#160;</div>
                    </xsl:if>
                    <xsl:if test="$adminMode or $page/Contents/Content[@position='LeftNav']">
                      <div id="LeftNav">
                        <xsl:apply-templates select="/Page" mode="addModule">
                          <xsl:with-param name="text">Add Module</xsl:with-param>
                          <xsl:with-param name="position">LeftNav</xsl:with-param>
                        </xsl:apply-templates>
                      </div>
                    </xsl:if>
                  </div>
                  <div class="col-lg-{12 - $SideSubWidth}" id="content">
                    <xsl:attribute name="class">
                      <xsl:text>col-lg-</xsl:text>
                      <xsl:value-of select="12 - $SideSubWidth"/>
                      <xsl:if test="$sub-nav='right'">
                        <xsl:text> order-1</xsl:text>
                      </xsl:if>
                    </xsl:attribute>
                    <xsl:if test="$SideSubWidthCustom!=''">
                      <xsl:attribute name="style">
                        <xsl:text>width:</xsl:text>
                        <xsl:value-of select="100 - $SideSubWidthCustom" />
                        <xsl:text>%</xsl:text>
                      </xsl:attribute>
                    </xsl:if>
                    <!--<xsl:if test="$themeTitle='true' and not($page/ContentDetail)">
											<div id="mainTitle">
												<xsl:apply-templates select="/" mode="getMainTitle" />
											</div>
										</xsl:if>-->
                    <xsl:apply-templates select="." mode="mainLayout"/>
                  </div>
                  <xsl:if test="not($cartPage) and $currentPage/@name!='Home' and not($page/ContentDetail) and ($sub-nav='left' or $sub-nav='right')">
                    <div class="container-fluid">
                      <div id="custom">
                        <xsl:apply-templates select="/Page" mode="addModule">
                          <xsl:with-param name="text">Add Module</xsl:with-param>
                          <xsl:with-param name="position">custom</xsl:with-param>
                        </xsl:apply-templates>
                      </div>
                    </div>
                  </xsl:if>
                </div>
              </div>
            </xsl:when>
            <!--~~~~~~~~~~~~~~ pages with no side nav ~~~~~~~~~~~~~~ -->
            <xsl:otherwise>
              <xsl:apply-templates select="." mode="mainLayout">
                <xsl:with-param name="containerClass" select="$container"/>
              </xsl:apply-templates>
            </xsl:otherwise>
          </xsl:choose>
        </div>
      </div>
    </div>
    <xsl:apply-templates select="." mode="footer1">
      <xsl:with-param name="containerClass" select="$container"/>
    </xsl:apply-templates>
    <div class="modal fade" id="LoginModal" tabindex="-1" role="dialog" aria-labelledby="LoginTitle" aria-hidden="true">
      <div class="modal-dialog modal-md" role="document">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="LoginTitle">Log in</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"> </button>
          </div>
          <div class="modal-body">
            <div id="Login">
              <xsl:apply-templates select="/Page" mode="addModule">
                <xsl:with-param name="text">Add Module</xsl:with-param>
                <xsl:with-param name="position">Login</xsl:with-param>
              </xsl:apply-templates>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="modal fade" id="SearchModal" tabindex="-1" role="dialog" aria-labelledby="SearchTitle" aria-hidden="true">
      <div class="modal-dialog modal-md" role="document">
        <div class="modal-content">
          <div class="modal-body">
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"> </button>
            <div id="Search" class="search-wrapper">
              <form method="post" action="/information/search" id="searchInput" class="input-group">
                <label for="searchString" class="visually-hidden">Search</label>
                <input type="text" class="form-control CTAsearch" name="searchString" id="searchString" value="" placeholder="Search" />
                <input type="hidden" name="searchMode" value="REGEX" class="d-none" />
                <input type="hidden" name="contentType" value="Product" class="d-none"/>
                <input type="hidden" name="searchFormId" value="8923" class="d-none"/>
                <button type="submit" class="btn btn-outline-primary" name="Search" value="Submit">
                  <i class="fa fa-search">
                    <xsl:text> </xsl:text>
                  </i>
                  <span class="visually-hidden">Search</span>
                </button>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  </xsl:template>


</xsl:stylesheet>
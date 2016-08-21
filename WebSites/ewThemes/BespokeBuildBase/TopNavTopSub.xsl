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

  <xsl:variable name="themeLayout">TopNavTopSub</xsl:variable>

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
      <xsl:apply-templates select="." mode="header6">
        <xsl:with-param name="nav-collapse">true</xsl:with-param>
      </xsl:apply-templates>

      <!--~~~~~~~~~~~~~~ TOP SUB MENU ~~~~~~~~~~~~~~ -->
      <div class="sub-nav-wrapper not-xs">
        <xsl:apply-templates select="." mode="topSubNav">
          <xsl:with-param name="sub-nav-collapse">true</xsl:with-param>
        </xsl:apply-templates>
      </div>

      <!--################## MAIN CONTENT ################## -->
      <div class="container-wrapper top-nav-layout">
        <xsl:if test="not($adminMode or /Page[@previewMode='true']) and $NavFix='true'">
          <xsl:attribute name="class">
            container-wrapper top-nav-layout fixed-nav-content <xsl:value-of select="$nav-padding"/>
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
      </div>
      <!--~~~~~~~~~~~~~~ sub nav for xs screens ~~~~~~~~~~~~~~ -->
      <xsl:if test="(count($sectionPage[@name!='Information' and @name!='Footer']/MenuItem[not(DisplayName/@exclude='true')])&gt;0 or ($currentPage[parent::MenuItem[@name='Information' or @name='Footer']/MenuItem] and count($currentPage[child::MenuItem[not(DisplayName/@exclude='true')]])&gt;0)) or ($currentPage[ancestor::MenuItem[@name='Information' or @name='Footer']/MenuItem] and not($currentPage[parent::MenuItem[@name='Information' or @name='Footer']/MenuItem])) and not($currentPage/DisplayName[@nonav='true']) and $currentPage/@name!='Home' and $themeLayout!='TopNavSideSub' and not($cartPage) or $themeLayout='SideNav'">
        <div class="xs-only xs-sub-menu hidden-print container">
          <xsl:if test="$themeLayout='TopNavSideSub'">
            <xsl:attribute name="class">
              <xsl:text>visible-xs visible-sm hidden-print xs-sub-menu container</xsl:text>
            </xsl:attribute>
          </xsl:if>
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

  <!-- ############################################ TOP SUB NAV ############################################### -->
  <xsl:template match="Page" mode="topSubNav">
    <xsl:param name="sub-nav-collapse" />
    <xsl:if test="not($currentPage/DisplayName[@nonav='true']) and (count($sectionPage[@name!='Information' and @name!='Footer']/MenuItem[not(DisplayName/@exclude='true')])&gt;0 and $currentPage/@name!='Home')">
      <section class="wrapper top-sub-menu top-sub-menu-1 not-xs">
        <div class="container">
          <div class="top-sub-inner">
            <xsl:apply-templates select="Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="submenu_topnav">
              <xsl:with-param name="apply-sub-nav-collapse">
                <xsl:value-of select="$sub-nav-collapse"/>
              </xsl:with-param>
            </xsl:apply-templates>
          </div>
        </div>
      </section>
      <xsl:if test="count(Menu/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/MenuItem) &gt; 0 and Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]">
        <section class="wrapper top-sub-menu top-sub-menu-2 not-xs">
          <div class="container">
            <div class="top-sub-inner">
              <xsl:apply-templates select="Menu/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="submenu_topnav">
                <xsl:with-param name="apply-sub-nav-collapse">
                  <xsl:value-of select="$sub-nav-collapse"/>
                </xsl:with-param>
              </xsl:apply-templates>
            </div>
          </div>
        </section>
      </xsl:if>
      <xsl:if test="count(Menu/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/MenuItem) &gt; 0 and Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]">
        <section class="wrapper top-sub-menu top-sub-menu-3 not-xs">
          <div class="container">
            <div class="top-sub-inner">
              <xsl:apply-templates select="Menu/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="submenu_topnav">
                <xsl:with-param name="apply-sub-nav-collapse">
                  <xsl:value-of select="$sub-nav-collapse"/>
                </xsl:with-param>
              </xsl:apply-templates>
            </div>
          </div>
        </section>
      </xsl:if>
      <xsl:if test="count(Menu/MenuItem/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/MenuItem) &gt; 0 and Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]">
        <section class="wrapper top-sub-menu top-sub-menu-4 not-xs">
          <div class="container">
            <div class="top-sub-inner">
              <xsl:apply-templates select="Menu/MenuItem/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="submenu_topnav">
                <xsl:with-param name="apply-sub-nav-collapse">
                  <xsl:value-of select="$sub-nav-collapse"/>
                </xsl:with-param>
              </xsl:apply-templates>
            </div>
          </div>
        </section>
      </xsl:if>
      <xsl:if test="count(Menu/MenuItem/MenuItem/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/MenuItem) &gt; 0 and Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]">
        <section class="wrapper top-sub-menu top-sub-menu-5 not-xs">
          <div class="container">
            <div class="top-sub-inner">
              <xsl:apply-templates select="Menu/MenuItem/MenuItem/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="submenu_topnav">
                <xsl:with-param name="apply-sub-nav-collapse">
                  <xsl:value-of select="$sub-nav-collapse"/>
                </xsl:with-param>
              </xsl:apply-templates>
            </div>
          </div>
        </section>
      </xsl:if>
    </xsl:if>
    <xsl:if test="$currentPage[parent::MenuItem[@name='Information']/MenuItem] and (count($currentPage[child::MenuItem[not(DisplayName/@exclude='true')]])&gt;0) and not($currentPage/DisplayName[@nonav='true']) and $themeLayout='TopNav' or $currentPage[ancestor::MenuItem[@name='Information']/MenuItem] and not($currentPage[parent::MenuItem[@name='Information']/MenuItem]) and not($currentPage/DisplayName[@nonav='true']) and $themeLayout='TopNav'">
      <section class="wrapper top-sub-menu top-sub-menu-1 not-xs">
        <div class="container">
          <div class="top-sub-inner">
            <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name='Information']/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="submenu_topnav">
              <xsl:with-param name="apply-sub-nav-collapse">
                <xsl:value-of select="$sub-nav-collapse"/>
              </xsl:with-param>
            </xsl:apply-templates>
          </div>
        </div>
      </section>
    </xsl:if>
    <xsl:if test="count(Menu/MenuItem/MenuItem[@name='Information']/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]])&gt;0 and (count($currentPage[child::MenuItem[not(DisplayName/@exclude='true')]])&gt;0) and not($currentPage/DisplayName[@nonav='true']) and $themeLayout='TopNav' or count(Menu/MenuItem/MenuItem[@name='Information']/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]])&gt;0 and not($currentPage/DisplayName[@nonav='true']) and $themeLayout='TopNav'">
      <section class="wrapper top-sub-menu top-sub-menu-2 not-xs">
        <div class="container">
          <div class="top-sub-inner">
            <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name='Information']/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="submenu_topnav">
              <xsl:with-param name="apply-sub-nav-collapse">
                <xsl:value-of select="$sub-nav-collapse"/>
              </xsl:with-param>
            </xsl:apply-templates>
          </div>
        </div>
      </section>
    </xsl:if>
  </xsl:template>


</xsl:stylesheet>
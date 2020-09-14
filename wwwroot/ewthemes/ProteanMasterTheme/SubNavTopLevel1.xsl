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

  <xsl:variable name="themeLayout">SubNavTopLevel1</xsl:variable>
  <xsl:template match="Page" mode="bodyDisplay">
    <xsl:variable name="nav-padding">
      <xsl:if test="$currentPage/DisplayName[@navpad='false'] and not($cartPage)">nav-no-padding</xsl:if>
    </xsl:variable>
    <xsl:variable name="detail-heading">
      <xsl:if test="$page/ContentDetail">detail-heading</xsl:if>
    </xsl:variable>
    <xsl:variable name="home-class">
      <xsl:if test="$currentPage[@id='1']">home-class</xsl:if>
    </xsl:variable>
    <div id="mainTable" class="Site activateAppearAnimation {$nav-padding}">
      <xsl:if test="$cartPage">
        <xsl:attribute name="class">Site activateAppearAnimation nav-no-padding</xsl:attribute>
      </xsl:if>
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
      <div class="modal fade" id="LoginModal" tabindex="-1" role="dialog" aria-labelledby="LoginModal">
        <div class="modal-dialog modal-md" role="document">
          <div class="modal-content">
            <div class="modal-header">
              <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                <span aria-hidden="true">
                  <i class="fa fa-close"> </i>
                </span>
              </button>
              <h4 class="modal-title">Login</h4>
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
      <xsl:if test="$adminMode or /Page/Contents/Content[@position='site-alert']">
        <div id="site-alert" class="clearfix">
          <xsl:apply-templates select="/Page" mode="addModule">
            <xsl:with-param name="text">Add Module</xsl:with-param>
            <xsl:with-param name="name">site-alert</xsl:with-param>
            <xsl:with-param name="position">site-alert</xsl:with-param>
          </xsl:apply-templates>
          <xsl:text> </xsl:text>
        </div>
      </xsl:if>
      <xsl:apply-templates select="." mode="header-template1">
        <xsl:with-param name="nav-collapse">false</xsl:with-param>
      </xsl:apply-templates>
      <!--~~~~~~~~~~~~~~ TOP SUB MENU ~~~~~~~~~~~~~~ -->
      <xsl:if test="not($cartPage) and $currentPage/@name!='Home' and not($page/ContentDetail)">
        <div id="header-banner" style="background-image:url({/Page/Contents/Content[@name='Banner']/img/@src})">
          <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
            <xsl:with-param name="type">Image</xsl:with-param>
            <xsl:with-param name="text">Add Banner Background</xsl:with-param>
            <xsl:with-param name="name">Banner</xsl:with-param>
          </xsl:apply-templates>
          <div class="header-banner-inner">
            <!--<xsl:apply-templates select="/Page/Contents/Content[@name='Banner']" mode="displayBrief" />-->
            <xsl:if test="not($currentPage/DisplayName[@nonav='true']) and $currentPage/@name!='Home' and $themeBreadcrumb='true'">
              <section class="wrapper">
                <div class="container">
                  <ol class="breadcrumb">
                    <xsl:apply-templates select="Menu/MenuItem" mode="breadcrumb"/>
                  </ol>
                </div>
              </section>
            </xsl:if>
            <xsl:if test="not($cartPage) and $currentPage/@name!='Home'">
              <!--<xsl:if test="$currentPage/@name!='Home'">-->
              <xsl:if test="$themeTitle='true' and not($page/ContentDetail)">
                <section class="wrapper">
                  <div class="container">
                    <div id="mainTitle">
                      <xsl:apply-templates select="/" mode="getMainTitle" />
                    </div>
                  </div>
                </section>
              </xsl:if>
            </xsl:if>
            <div id="header-banner-text" class="container">
              <xsl:apply-templates select="/Page" mode="addModule">
                <xsl:with-param name="text">Add Module</xsl:with-param>
                <xsl:with-param name="position">header-banner-text</xsl:with-param>
                <xsl:with-param name="class">container</xsl:with-param>
              </xsl:apply-templates>
            </div>
          </div>
          <div class="sub-nav-wrapper">
            <xsl:apply-templates select="." mode="topSubNav">
              <xsl:with-param name="sub-nav-collapse">true</xsl:with-param>
            </xsl:apply-templates>
          </div>
        </div>
      </xsl:if>

      <xsl:if test="$page/ContentDetail">
        <div id="header-banner" class="content-detail-header" style="background-image:url({$currentPage/Images/img[@class='thumbnail']/@src})">
          <xsl:if test="not($currentPage/DisplayName[@nonav='true']) and $currentPage/@name!='Home' and $themeBreadcrumb='true'">
            <section class="wrapper">
              <div class="container">
                <ol class="breadcrumb">
                  <xsl:apply-templates select="Menu/MenuItem" mode="breadcrumb"/>
                </ol>
              </div>
            </section>
          </xsl:if>
        </div>
      </xsl:if>


      <!--################## MAIN CONTENT ################## -->
      <div class="container-wrapper side-nav-layout {$detail-heading} {$home-class}">
        <xsl:if test="not($adminMode or /Page[@previewMode='true']) and $NavFix='true'">
          <xsl:attribute name="class">
            container-wrapper side-nav-layout fixed-nav-content <xsl:value-of select="$nav-padding"/> <xsl:value-of select="$detail-heading"/> <xsl:value-of select="$home-class"/>
          </xsl:attribute>
        </xsl:if>
        <!--~~~~~~~~~~~~~~ breadcrumb ~~~~~~~~~~~~~~ -->


        <div id="mainLayout" class="fullwidth activateAppearAnimation">
          <xsl:choose>
            <!--<xsl:when test="(count($sectionPage[@name!='Information' and @name!='Footer']/MenuItem/MenuItem[not(DisplayName/@exclude='true')])&gt;0 and not($currentPage/DisplayName[@nonav='true']) and $currentPage/@name!='Home') and not($cartPage) or $currentPage[parent::MenuItem[@name='Information']/MenuItem] and (count($currentPage[child::MenuItem[not(DisplayName/@exclude='true')]])&gt;0 and not($currentPage/DisplayName[@nonav='true'])) and not($cartPage) or $currentPage[ancestor::MenuItem[@name='Information']/MenuItem] and not($currentPage[parent::MenuItem[@name='Information']/MenuItem]) and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">-->
            <!--<xsl:when test="not($cartPage) and not($currentPage/DisplayName[@nonav='true']) and $currentPage/@name!='Home' and ((count($sectionPage[@name!='Information' and @name!='Footer']/MenuItem/MenuItem[not(DisplayName/@exclude='true')])&gt;0  or ($currentPage[parent::MenuItem[@name='Information']/MenuItem] and (count($currentPage[child::MenuItem[not(DisplayName/@exclude='true')]])&gt;0) or ($currentPage[ancestor::MenuItem[@name='Information']/MenuItem] and not($currentPage[parent::MenuItem[@name='Information']/MenuItem]))">-->
            <!--<xsl:when test="not($cartPage) and not($currentPage/DisplayName[@nonav='true']) and $currentPage/@name!='Home' and count($sectionPage[@name!='Information' and @name!='Footer']/MenuItem/MenuItem[not(DisplayName/@exclude='true')])&gt;0 ">-->
            <xsl:when test="not($cartPage) and not($sectionPage[@name='eCoaching']) and not($currentPage/DisplayName[@nonav='true']) and $currentPage/@name!='Home' and ($sectionPage[@name!='Information' and @name!='Footer'] and count($subSectionPage/MenuItem[not(DisplayName/@exclude='true')])&gt;0) or ($sectionPage[@name='Information']) and count($subSubSectionPage/MenuItem[not(DisplayName/@exclude='true')])&gt;0">
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
                        <xsl:when test="$currentPage[ancestor::MenuItem[@name='Information']]">
                          <xsl:apply-templates select="$subSubSectionPage" mode="submenu"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:apply-templates select="$subSectionPage" mode="submenu">
                            <xsl:with-param name="sectionHeading">false</xsl:with-param>
                          </xsl:apply-templates>
                        </xsl:otherwise>
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
                    <div id="content" class="sr-only">&#160;</div>
                    <xsl:apply-templates select="." mode="mainLayout"/>
                  </div>
                </div>
              </div>
            </xsl:when>
            <xsl:otherwise>
              <div id="content" class="sr-only">&#160;</div>
              <xsl:apply-templates select="." mode="mainLayout">
                <xsl:with-param name="containerClass" select="'container'"/>
              </xsl:apply-templates>
            </xsl:otherwise>
          </xsl:choose>
        </div>
      </div>
      <!--~~~~~~~~~~~~~~ sub nav for xs screens ~~~~~~~~~~~~~~ -->
      <xsl:if test="(count($sectionPage[@name!='Information' and @name!='Footer']/MenuItem[not(DisplayName/@exclude='true')])&gt;0 and not($sectionPage[@name='eCoaching']) and not($cartPage) or ($currentPage[parent::MenuItem[@name='Information' or @name='Footer']/MenuItem]  and not($sectionPage[@name='eCoaching']) and not($cartPage) and count($currentPage[child::MenuItem[not(DisplayName/@exclude='true')]])&gt;0)) or ($currentPage[ancestor::MenuItem[@name='Information' or @name='Footer']/MenuItem] and not($sectionPage[@name='eCoaching']) and not($currentPage[parent::MenuItem[@name='Information' or @name='Footer']/MenuItem])) and not($currentPage/DisplayName[@nonav='true']) and $currentPage/@name!='Home' and not($cartPage)">
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
    <xsl:apply-templates select="." mode="footer1" />

  </xsl:template>
  <!-- -->
  <!-- ############################################ TOP SUB NAV ############################################### -->
  <xsl:template match="Page" mode="topSubNav">
    <xsl:param name="sub-nav-collapse" />
    <xsl:if test="not($currentPage/DisplayName[@nonav='true']) and (count($sectionPage[@name!='Information' and @name!='Footer']/MenuItem[not(DisplayName/@exclude='true')])&gt;0 and $currentPage/@name!='Home') and not($sectionPage[@name='eCoaching'])">
      <section class="wrapper top-sub-menu top-sub-menu-1">
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
      <!--<xsl:if test="count(Menu/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/MenuItem) &gt; 0 and Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]">
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
      </xsl:if>-->
    </xsl:if>
    <xsl:if test="$currentPage[parent::MenuItem[@name='Information']/MenuItem] and (count($currentPage[child::MenuItem[not(DisplayName/@exclude='true')]])&gt;0) and not($currentPage/DisplayName[@nonav='true'])or $currentPage[ancestor::MenuItem[@name='Information']/MenuItem] and not($currentPage[parent::MenuItem[@name='Information']/MenuItem]) and not($currentPage/DisplayName[@nonav='true'])">
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
    <xsl:if test="$sectionPage[@name='eCoaching']">
      <section class="wrapper top-sub-menu top-sub-menu-1">
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
    <!--<xsl:if test="count(Menu/MenuItem/MenuItem[@name='Information']/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]])&gt;0 and (count($currentPage[child::MenuItem[not(DisplayName/@exclude='true')]])&gt;0) and not($currentPage/DisplayName[@nonav='true']) or count(Menu/MenuItem/MenuItem[@name='Information']/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]])&gt;0 and not($currentPage/DisplayName[@nonav='true'])">
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
    </xsl:if>-->
  </xsl:template>

  <!--################## FOOTER ################## -->
  <xsl:template match="Page" mode="footer1">
    <div id="pagefooter" class="Site clearfix">
      <div class="footer-inner">
        <div class="clearfix footer-main">
          <div class="container">
            <xsl:if test="Menu/MenuItem/MenuItem[@name='Footer']/MenuItem and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
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

</xsl:stylesheet>
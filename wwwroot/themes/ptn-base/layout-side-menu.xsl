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

  <xsl:variable name="themeLayout">layout-side-menu</xsl:variable>
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
      <!--################## HEADER ################## -->

      <a class="sr-only" href="#content">Skip to main content</a>
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
      <xsl:choose>
        <xsl:when test="$header-layout='header-flex1'">
          <xsl:apply-templates select="." mode="header-flex1">
            <xsl:with-param name="nav-collapse">false</xsl:with-param>
            <xsl:with-param name="social-links">true</xsl:with-param>
          </xsl:apply-templates>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="." mode="header-template1">
            <xsl:with-param name="nav-collapse">false</xsl:with-param>
          </xsl:apply-templates>
        </xsl:otherwise>
      </xsl:choose>
      

      <!--################## MAIN CONTENT ################## -->
      <div class="container-wrapper side-nav-layout {$detail-heading} {$home-class}">
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
                    <button class="btn btn-primary hidden visible-xs visible-sm xs-menu-btn" type="button" data-toggle="collapse" data-target="#subMenuCollapse" aria-expanded="false" aria-controls="subMenuCollapse">
                      <xsl:apply-templates select="$sectionPage/@name" mode="cleanXhtml"/> Section Menu <i class="fas fa-caret-down"> </i>
                    </button>
                    <div class="collapse dont-collapse-md" id="subMenuCollapse">
                      <div id="subMenu">
                        <xsl:choose>
                          <xsl:when test="$currentPage[parent::MenuItem[@name='Information']/MenuItem] and (count($currentPage[child::MenuItem[not(DisplayName/@exclude='true')]])&gt;0) or $currentPage[ancestor::MenuItem[@name='Information']/MenuItem] and not($currentPage[parent::MenuItem[@name='Information']/MenuItem])">
                            <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name='Information']/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="submenu"/>
                          </xsl:when>
                          <xsl:when test="$themeLayout='TopNavSideSub'">
                            <xsl:apply-templates select="$sectionPage" mode="submenu">
                              <xsl:with-param name="sectionHeading">true</xsl:with-param>
                            </xsl:apply-templates>
                          </xsl:when>
                        </xsl:choose>
                      </div>
                    </div>
                    <xsl:if test="$adminMode or $page/Contents/Content[@position='LeftNav']">
                      <div id="LeftNav">
                        <xsl:apply-templates select="/Page" mode="addModule">
                          <xsl:with-param name="text">Add Module</xsl:with-param>
                          <xsl:with-param name="position">LeftNav</xsl:with-param>
                        </xsl:apply-templates>
                      </div>
                    </xsl:if>
                  </div>
                  <div class="col-md-{12 - $SideSubWidth}" id="content">
                    <xsl:if test="$SideSubWidthCustom!=''">
                      <xsl:attribute name="style">
                        <xsl:text>width:</xsl:text>
                        <xsl:value-of select="100 - $SideSubWidthCustom" />
                        <xsl:text>%</xsl:text>
                      </xsl:attribute>
                    </xsl:if>
                    <xsl:if test="$themeTitle='true' and not($page/ContentDetail)">
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
              <xsl:apply-templates select="." mode="mainLayout">
                <xsl:with-param name="containerClass" select="'container'"/>
              </xsl:apply-templates>
            </div>
          </xsl:otherwise>
        </xsl:choose>
      </div>
    </div>

    <!--################## FOOTER ################## -->
    <xsl:apply-templates select="." mode="footer1" />
  </xsl:template>

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
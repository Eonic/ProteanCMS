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

  <xsl:variable name="themeLayout">NoMenuWithBanner</xsl:variable>
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
      <!--~~~~~~~~~~~~~~ TOP BANNER ~~~~~~~~~~~~~~ -->
      <!--<xsl:if test="not($cartPage) and $currentPage/@name!='Home' and not($page/ContentDetail)">
        <div id="header-banner" style="background-image:url({/Page/Contents/Content[@name='Banner']/img/@src})">
          <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
            <xsl:with-param name="type">Image</xsl:with-param>
            <xsl:with-param name="text">Add Banner Background</xsl:with-param>
            <xsl:with-param name="name">Banner</xsl:with-param>
          </xsl:apply-templates>
          <div class="header-banner-inner">
            --><!--<xsl:apply-templates select="/Page/Contents/Content[@name='Banner']" mode="displayBrief" />--><!--
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
              --><!--<xsl:if test="$currentPage/@name!='Home'">--><!--
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
        </div>
      </xsl:if>-->
      <xsl:if test="not($page/ContentDetail) and not($currentPage/@name='Home') and not($cartPage)">
        <xsl:choose>
          <xsl:when test="/Page/Contents/Content[@name='Banner']/img/@src!='' or $adminMode">
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
              </div>
            </div>
          </xsl:when>
          <xsl:otherwise>
            <section class="wrapper bannerless-page">
              <div class="container">
                <div id="mainTitle">
                  <xsl:apply-templates select="/" mode="getMainTitle" />
                </div>
              </div>
            </section>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>

      <!--<xsl:if test="$page/ContentDetail">
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
      </xsl:if>-->

      <xsl:if test="$page/ContentDetail and $themeBreadcrumb='true' and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
        <section class="wrapper detail-breadcrumb-wrapper">
          <div class="container">
            <ol class="breadcrumb detail-breadcrumb">
              <xsl:apply-templates select="Menu/MenuItem" mode="breadcrumb"/>
            </ol>
          </div>
        </section>
      </xsl:if>



      <!--################## MAIN CONTENT ################## -->
      <div class="container-wrapper side-nav-layout {$detail-heading} {$home-class}">
        <xsl:if test="not($adminMode or /Page[@previewMode='true']) and $NavFix='true'">
          <xsl:attribute name="class">
            container-wrapper side-nav-layout fixed-nav-content <xsl:value-of select="$nav-padding"/> <xsl:value-of select="$detail-heading"/> <xsl:value-of select="$home-class"/>
          </xsl:attribute>
        </xsl:if>


        <div id="mainLayout" class="fullwidth activateAppearAnimation">
          <div id="content" class="sr-only">&#160;</div>
          <xsl:apply-templates select="." mode="mainLayout">
            <xsl:with-param name="containerClass" select="'container'"/>
          </xsl:apply-templates>
        </div>
      </div>
    </div>
    <xsl:apply-templates select="." mode="footer1" />

  </xsl:template>
  <!-- -->

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
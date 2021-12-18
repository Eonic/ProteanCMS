<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
  <!-- HEADER TEMPLATE 1-->
  <!-- main menu full width below logo -->
  <xsl:template match="Page" mode="header-menu-below">
    <xsl:param name="nav-collapse" />
    <xsl:param name="cart-style" />
    <xsl:param name="login-style" />
    <xsl:param name="login-position" />
    <xsl:param name="login-action" />
    <!--<xsl:if test="/Page/User[@id!='']">
      <div id="logged-in-bar">
        <div class="container">
          <xsl:apply-templates select="/" mode="membershipBrief" />
        </div>
      </div>
    </xsl:if>-->
    <header class="navbar navbar-default header header-oc">
      <xsl:if test="not($adminMode or /Page[@previewMode='true']) and $NavFix='true'">
        <xsl:attribute name="class">navbar navbar-default navbar-fixed-top header header-oc</xsl:attribute>
      </xsl:if>
      <!--LOGO-->
      <div class="navbar-header">
        <xsl:if test="$cartPage">
          <xsl:attribute name="class">navbar-header cart-header</xsl:attribute>
        </xsl:if>
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

          <div class="pull-right header-right">
            <div class="header-tier1 clearfix">
              <div class="header-tier1-inner">
                <!--INFO NAV-->
                <xsl:if test="Menu/MenuItem/MenuItem[@name='Information']/MenuItem and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
                  <ul class="nav navbar-nav info-nav not-xs">
                    <xsl:if test="$HomeInfo='true'">
                      <li class="first not-xs">
                        <xsl:apply-templates select="Menu/MenuItem" mode="menuLink"/>
                      </li>
                    </xsl:if>
                    <xsl:for-each select="Menu/MenuItem/MenuItem[@name='Information']/MenuItem[not(DisplayName/@exclude='true')]">
                      <li>
                        <xsl:apply-templates select="." mode="menuLink"/>
                      </li>
                    </xsl:for-each>
                  </ul>
                </xsl:if>
                <!--LOGON BUTTON (DESKTOP)-->
                <xsl:if test="$membership='on'">
                  <div class="not-xs login-wrapper">
                    <xsl:apply-templates select="/" mode="loginTop" />
                  </div>
                </xsl:if>
                <!--SOCIAL-->
                <div class="socialLinksHeader not-xs" id="socialLinksHeader">
                  <!--<xsl:apply-templates select="Contents/Content[@name='socialLinks']" mode="displayBrief"/>-->
                  <xsl:apply-templates select="/Page" mode="addSingleModule">
                    <xsl:with-param name="text">Add Social Links</xsl:with-param>
                    <xsl:with-param name="position">socialLinksHeader</xsl:with-param>
                    <xsl:with-param name="class">socialLinksHeader</xsl:with-param>
                  </xsl:apply-templates>
                </div>
              </div>
            </div>
            <div class="header-tier2 clearfix">
              <!--CART-->
              <xsl:if test="$cart='on' and not($cartPage)">
                <xsl:apply-templates select="/" mode="cartBrief">
                  <xsl:with-param name="apply-cart-style">
                    <xsl:value-of select="$cart-style"/>
                  </xsl:with-param>
                </xsl:apply-templates>
              </xsl:if>
              <!--MEMBERSHIP (MOBILE)-->
              <!--<xsl:if test="$membership='on'">
                <div class="xs-login xs-only">
                  <xsl:apply-templates select="/" mode="loginTopxs" >
                    <xsl:with-param name="apply-login-action">
                      <xsl:value-of select="$login-action"/>
                    </xsl:with-param>
                  </xsl:apply-templates>
                </div>
              </xsl:if>-->
              <!--SEACH (DESKTOP)-->
              <xsl:if test="$search='on' and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
                <div class="not-xs search-wrapper">
                  <xsl:apply-templates select="/" mode="searchBrief"/>
                </div>
              </xsl:if>
              <!--STRAPLINE-->
              <div class="strapline" id="Strapline">
                <xsl:apply-templates select="Contents/Content[@name='Strapline']" mode="displayBrief"/>
                <xsl:apply-templates select="/Page" mode="addSingleModule">
                  <xsl:with-param name="text">Add Strapline</xsl:with-param>
                  <xsl:with-param name="position">Strapline</xsl:with-param>
                  <xsl:with-param name="class">strapline</xsl:with-param>
                </xsl:apply-templates>
              </div>
              <!--NAV TOGGLE (MOBILE)-->
              <xsl:if test="not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
                <button type="button" class="navbar-toggle collapsed" data-toggle="offcanvas" data-target="#navbar-main" aria-expanded="false" data-canvas="body">
                  <span class="sr-only">Toggle navigation</span>
                  <span class="icon-bar"></span>
                  <span class="icon-bar"></span>
                  <span class="icon-bar"></span>
                </button>
              </xsl:if>
            </div>
          </div>
        </div>
      </div>
      <!-- MAIN MENU -->

      <xsl:if test="not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
        <nav class="navbar-collapse navmenu-fixed-right navbar-offcanvas offcanvas-xs" id="navbar-main">
          <div class="container">
            <!--SEARCH (MOBILE)-->
            <xsl:if test="$search='on'">
              <div class="xs-only search-oc">
                <xsl:apply-templates select="/" mode="searchBrief"/>
              </div>
            </xsl:if>
            <ul class="nav navbar-nav main-nav">
              <xsl:if test="$HomeNav='true' or $HomeInfo='true'">
                <li class="first">
                  <xsl:if test="$currentPage/@name='Home'">
                    <xsl:attribute name="class">first active </xsl:attribute>
                  </xsl:if>
                  <xsl:if test="$HomeInfo='true'">
                    <xsl:attribute name="class">xs-only </xsl:attribute>
                  </xsl:if>
                  <xsl:if test="$HomeInfo='true' and $currentPage/@name='Home'">
                    <xsl:attribute name="class">first active xs-only </xsl:attribute>
                  </xsl:if>
                  <xsl:apply-templates select="Menu/MenuItem" mode="menuLink"/>
                </li>
              </xsl:if>
              <xsl:choose>
                <xsl:when test="$nav-dropdown='true'">
                  <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Information' and @name!='Footer']" mode="mainmenudropdown">
                    <xsl:with-param name="overviewLink">true</xsl:with-param>
                  </xsl:apply-templates>
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
                <!--<xsl:if test="$HomeInfo='true'">
                  <li class="first not-xs">
                    <xsl:if test="$currentPage/@name='Home'">
                      <xsl:attribute name="class">first not-xs active </xsl:attribute>
                    </xsl:if>
                    <xsl:apply-templates select="Menu/MenuItem" mode="menuLink"/>
                  </li>
                </xsl:if>-->
                <xsl:for-each select="Menu/MenuItem/MenuItem[@name='Information']/MenuItem[not(DisplayName/@exclude='true')]">
                  <li>
                    <xsl:apply-templates select="." mode="menuLink"/>
                  </li>
                </xsl:for-each>
                <xsl:if test="$membership='on'">
                  <xsl:apply-templates select="/" mode="loginTopxs" >
                    <xsl:with-param name="apply-login-action">
                      <xsl:value-of select="$login-action"/>
                    </xsl:with-param>
                  </xsl:apply-templates>
                </xsl:if>
              </ul>
            </xsl:if>
            <div class="socialLinksHeader xs-only">
              <xsl:apply-templates select="/Page" mode="addSingleModule">
                <xsl:with-param name="text">Add Social Links</xsl:with-param>
                <xsl:with-param name="position">socialLinksHeader</xsl:with-param>
                <xsl:with-param name="class">socialLinksHeader xs-only</xsl:with-param>
              </xsl:apply-templates>
            </div>
          </div>
        </nav>
      </xsl:if>
    </header>
  </xsl:template>



  <!--HEADER FLEX 1-->
  <!--menu, info nav and other elements in 2 tears to right of logo, with flexbox layout-->
  <xsl:template match="Page" mode="header-flex1">
    <xsl:param name="nav-collapse" />
    <xsl:param name="cart-style" />
    <xsl:param name="login-style" />
    <xsl:param name="login-position" />
    <xsl:param name="login-action" />
    <xsl:param name="social-links" />
    <!--<xsl:if test="/Page/User[@id!=''] and $membership='on'">
      <div id="logged-in-bar">
        <div class="container">
          <xsl:apply-templates select="/" mode="membershipBrief" />
        </div>
      </div>
    </xsl:if>-->
    <header class="navbar navbar-default header header-oc">
      <xsl:if test="not($adminMode or /Page[@previewMode='true']) and $NavFix='true'">
        <xsl:attribute name="class">navbar navbar-default navbar-fixed-top header header-oc</xsl:attribute>
      </xsl:if>
      <!-- Brand and toggle get grouped for better mobile display -->
      <!--<xsl:if test="$membership='on'">
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
      </xsl:if>-->
      <!--LOGO-->
      <div class="navbar-header">
        <div class="container flex-container">
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

          <div class="header-right">
            <div class="header-tier1 clearfix">
              <div class="header-tier1-inner">


                <!--SOCIAL-->
                <xsl:if test="$social-links='true'">
                  <div class="socialLinksHeader not-xs" id="socialLinksHeader">
                    <!--<xsl:apply-templates select="Contents/Content[@name='socialLinks']" mode="displayBrief"/>-->
                    <xsl:apply-templates select="/Page" mode="addSingleModule">
                      <xsl:with-param name="text">Add Social Links</xsl:with-param>
                      <xsl:with-param name="position">socialLinksHeader</xsl:with-param>
                      <xsl:with-param name="class">socialLinksHeader</xsl:with-param>
                    </xsl:apply-templates>
                  </div>
                </xsl:if>

                <!--MEMBERSHIP (MOBILE)-->
                <!--<xsl:if test="$membership='on'">
                  <div class="xs-login xs-only">
                    <xsl:apply-templates select="/" mode="loginTopxs" >
                      <xsl:with-param name="apply-login-action">
                        <xsl:value-of select="$login-action"/>
                      </xsl:with-param>
                    </xsl:apply-templates>
                  </div>
                </xsl:if>-->
                <!--SEACH (DESKTOP)-->
                <xsl:if test="$search='on' and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
                  <div class="not-xs search-wrapper">
                    <xsl:apply-templates select="/" mode="searchBrief"/>
                  </div>
                </xsl:if>
                <!--INFO NAV-->
                <xsl:if test="Menu/MenuItem/MenuItem[@name='Information']/MenuItem and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
                  <ul class="nav navbar-nav info-nav not-xs">
                    <xsl:if test="$HomeInfo='true'">
                      <li class="first not-xs">
                        <xsl:apply-templates select="Menu/MenuItem" mode="menuLink"/>
                      </li>
                    </xsl:if>
                    <xsl:for-each select="Menu/MenuItem/MenuItem[@name='Information']/MenuItem[not(DisplayName/@exclude='true')]">
                      <li>
                        <xsl:apply-templates select="." mode="menuLink"/>
                      </li>
                    </xsl:for-each>
                  </ul>
                </xsl:if>
                <!--STRAPLINE-->
                <div class="strapline" id="Strapline">
                  <xsl:apply-templates select="Contents/Content[@name='Strapline']" mode="displayBrief"/>
                  <xsl:apply-templates select="/Page" mode="addSingleModule">
                    <xsl:with-param name="text">Add Strapline</xsl:with-param>
                    <xsl:with-param name="position">Strapline</xsl:with-param>
                    <xsl:with-param name="class">strapline</xsl:with-param>
                  </xsl:apply-templates>
                </div>
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
                <!--CART-->
                <xsl:if test="$cart='on' and not($cartPage)">
                  <xsl:apply-templates select="/" mode="cartBrief">
                    <xsl:with-param name="apply-cart-style">
                      <xsl:value-of select="$cart-style"/>
                    </xsl:with-param>
                  </xsl:apply-templates>
                </xsl:if>
                <!--NAV TOGGLE (MOBILE)-->
                <xsl:if test="not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
                  <button type="button" class="navbar-toggle collapsed" data-toggle="offcanvas" data-target="#navbar-main" aria-expanded="false" data-canvas="body">
                    <span class="sr-only">Toggle navigation</span>
                    <span class="icon-bar"></span>
                    <span class="icon-bar"></span>
                    <span class="icon-bar"></span>
                  </button>
                </xsl:if>
              </div>
            </div>

            <div class="header-tier2 clearfix">

              <xsl:if test="not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
                <nav class="navbar-collapse navmenu-fixed-right navbar-offcanvas offcanvas-xs" id="navbar-main">
                  <!--SEARCH (MOBILE)-->
                  <xsl:if test="$search='on'">
                    <div class="xs-only search-oc">
                      <xsl:apply-templates select="/" mode="searchBrief"/>
                    </div>
                  </xsl:if>
                  <ul class="nav navbar-nav main-nav">
                    <xsl:if test="$HomeNav='true' or $HomeInfo='true'">
                      <li class="first">
                        <xsl:if test="$currentPage/@name='Home'">
                          <xsl:attribute name="class">first active </xsl:attribute>
                        </xsl:if>
                        <xsl:if test="$HomeInfo='true'">
                          <xsl:attribute name="class">xs-only </xsl:attribute>
                        </xsl:if>
                        <xsl:if test="$HomeInfo='true' and $currentPage/@name='Home'">
                          <xsl:attribute name="class">first active xs-only </xsl:attribute>
                        </xsl:if>
                        <xsl:apply-templates select="Menu/MenuItem" mode="menuLink"/>
                      </li>
                    </xsl:if>
                    <xsl:choose>
                      <xsl:when test="$nav-dropdown='true'">
                        <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Information' and @name!='Footer']" mode="mainmenudropdown">
                          <xsl:with-param name="overviewLink">true</xsl:with-param>
                        </xsl:apply-templates>
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
                      <!--<xsl:if test="$HomeInfo='true'">
                  <li class="first not-xs">
                    <xsl:if test="$currentPage/@name='Home'">
                      <xsl:attribute name="class">first not-xs active </xsl:attribute>
                    </xsl:if>
                    <xsl:apply-templates select="Menu/MenuItem" mode="menuLink"/>
                  </li>
                </xsl:if>-->
                      <xsl:for-each select="Menu/MenuItem/MenuItem[@name='Information']/MenuItem[not(DisplayName/@exclude='true')]">
                        <li>
                          <xsl:apply-templates select="." mode="menuLink"/>
                        </li>
                      </xsl:for-each>
                      <xsl:if test="$membership='on'">
                        <xsl:apply-templates select="/" mode="loginTopxs" >
                          <xsl:with-param name="apply-login-action">
                            <xsl:value-of select="$login-action"/>
                          </xsl:with-param>
                        </xsl:apply-templates>
                      </xsl:if>
                    </ul>
                  </xsl:if>
                  <div class="socialLinksHeader xs-only">
                    <xsl:apply-templates select="/Page" mode="addSingleModule">
                      <xsl:with-param name="text">Add Social Links</xsl:with-param>
                      <xsl:with-param name="position">socialLinksHeader</xsl:with-param>
                      <xsl:with-param name="class">socialLinksHeader xs-only</xsl:with-param>
                    </xsl:apply-templates>
                  </div>
                </nav>
              </xsl:if>
            </div>
          </div>
        </div>
      </div>
      <!-- MAIN MENU -->

    </header>
  </xsl:template>

  <!--HEADER MENU RIGHT-->
  <!--bootstrap nav-->
  <xsl:template match="Page" mode="header-menu-right">
    <xsl:param name="nav-collapse" />
    <xsl:param name="cart-style" />
    <xsl:param name="login-style" />
    <xsl:param name="login-position" />
    <xsl:param name="login-action" />
    <xsl:param name="social-links" />
    <header class="navbar navbar-expand-lg navbar-light bg-light">
      <xsl:if test="not($adminMode or /Page[@previewMode='true']) and $NavFix='true'">
        <xsl:attribute name="class">navbar navbar-expand-lg navbar-fixed-top</xsl:attribute>
      </xsl:if>
      <!--LOGO-->
      <div class="container">
        <div class="navbar-brand">
          Site Name
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
        <div class="navbar-content">
          <div class="header-tier1">
            <!--INFO NAV-->
            <xsl:if test="Menu/MenuItem/MenuItem[@name='Information']/MenuItem and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
              <ul class="nav info-nav not-xs">
                <xsl:if test="$HomeInfo='true'">
                  <li class="nav-item">
                    <xsl:apply-templates select="Menu/MenuItem" mode="menuLink"/>
                  </li>
                </xsl:if>
                <xsl:for-each select="Menu/MenuItem/MenuItem[@name='Information']/MenuItem[not(DisplayName/@exclude='true')]">
                  <li class="nav-item">
                    <xsl:apply-templates select="." mode="menuLink">
                      <xsl:with-param name="class">nav-link</xsl:with-param>
                    </xsl:apply-templates>
                  </li>
                </xsl:for-each>
              </ul>
            </xsl:if>
            <!--LOGON BUTTON (DESKTOP)-->
            <xsl:if test="$membership='on'">
              <xsl:apply-templates select="/" mode="loginSimple"/>
            </xsl:if>
            <!--CART-->
            <xsl:if test="$cart='on' and not($cartPage)">
              <xsl:apply-templates select="/" mode="cartSimple"/>
            </xsl:if>
            <!--SEARCH (DESKTOP)-->
            <xsl:if test="$search='on' and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
              <div class="not-xs search-wrapper">
                <xsl:apply-templates select="/" mode="searchSimple"/>
              </div>
            </xsl:if>
          </div>
          <div class="header-tier2">
            <!--NAV TOGGLE (MOBILE)-->
            <xsl:if test="not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
              <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarSupportedContent" aria-controls="navbarSupportedContent" aria-expanded="false" aria-label="Toggle navigation">
                <span class="navbar-toggler-icon"></span>
              </button>
            </xsl:if>
            <div class="collapse navbar-collapse" id="navbarSupportedContent">
              <!-- MENU -->
              <ul class="navbar-nav">
                <xsl:if test="$HomeNav='true' or $HomeInfo='true'">
                  <li>
                    <xsl:attribute name="class"> nav-item </xsl:attribute>
                    <xsl:if test="$currentPage/@name='Home'">
                      <xsl:attribute name="class">active </xsl:attribute>
                    </xsl:if>
                    <xsl:if test="$HomeInfo='true'">
                      <xsl:attribute name="class">xs-only </xsl:attribute>
                    </xsl:if>
                    <xsl:if test="$HomeInfo='true' and $currentPage/@name='Home'">
                      <xsl:attribute name="class">first active xs-only </xsl:attribute>
                    </xsl:if>
                    <xsl:apply-templates select="Menu/MenuItem" mode="menuLink">
                      <xsl:with-param name="class">nav-link</xsl:with-param>
                    </xsl:apply-templates>
                  </li>
                </xsl:if>
                <xsl:choose>
                  <xsl:when test="$nav-dropdown='true'">
                    <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Information' and @name!='Footer']" mode="mainmenudropdown">
                      <xsl:with-param name="overviewLink">true</xsl:with-param>
                    </xsl:apply-templates>
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
              <!--INFO NAV-->
              <xsl:if test="Menu/MenuItem/MenuItem[@name='Information']/MenuItem and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
                <ul class="nav info-nav xs-only">
                  <xsl:for-each select="Menu/MenuItem/MenuItem[@name='Information']/MenuItem[not(DisplayName/@exclude='true')]">
                    <li class="nav-item">
                      <xsl:apply-templates select="." mode="menuLink">
                        <xsl:with-param name="class">nav-link</xsl:with-param>
                      </xsl:apply-templates>
                    </li>
                  </xsl:for-each>
                </ul>
              </xsl:if>
              <!--SOCIAL-->
              <xsl:if test="$social-links='true'">
                <div class="socialLinksHeader not-xs" id="socialLinksHeader">
                  <!--<xsl:apply-templates select="Contents/Content[@name='socialLinks']" mode="displayBrief"/>-->
                  <xsl:apply-templates select="/Page" mode="addSingleModule">
                    <xsl:with-param name="text">Add Social Links</xsl:with-param>
                    <xsl:with-param name="position">socialLinksHeader</xsl:with-param>
                    <xsl:with-param name="class">socialLinksHeader</xsl:with-param>
                  </xsl:apply-templates>
                </div>
              </xsl:if>
            </div>
          </div>
        </div>
      </div>
    </header>
  </xsl:template>

  <!--HEADER BASIC-->
  <!--bootstrap nav-->
  <xsl:template match="Page" mode="header-basic1">
    <xsl:param name="nav-collapse" />
    <xsl:param name="cart-style" />
    <xsl:param name="login-style" />
    <xsl:param name="login-position" />
    <xsl:param name="login-action" />
    <xsl:param name="social-links" />
    <nav class="navbar navbar-expand-lg navbar-light bg-light">
      <xsl:if test="not($adminMode or /Page[@previewMode='true']) and $NavFix='true'">
        <xsl:attribute name="class">navbar navbar-expand-lg navbar-fixed-top</xsl:attribute>
      </xsl:if>
      <!--LOGO-->
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
        <!--NAV TOGGLE (MOBILE)-->
        <xsl:if test="not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
          <button class="navbar-toggler" type="button" data-bs-toggle="offcanvas" data-bs-target="#navbarSupportedContent" aria-controls="navbarSupportedContent" aria-expanded="false" aria-label="Toggle navigation">
            <span class="navbar-toggler-icon"> </span>
          </button>
        </xsl:if>
        <div class="collapse navbar-collapse" id="navbarSupportedContent">
          <!-- MENU -->
          <ul class="navbar-nav">
            <xsl:if test="$HomeNav='true' or $HomeInfo='true'">
              <li>
                <xsl:attribute name="class"> nav-item </xsl:attribute>
                <xsl:if test="$currentPage/@name='Home'">
                  <xsl:attribute name="class">active </xsl:attribute>
                </xsl:if>
                <xsl:if test="$HomeInfo='true'">
                  <xsl:attribute name="class">xs-only </xsl:attribute>
                </xsl:if>
                <xsl:if test="$HomeInfo='true' and $currentPage/@name='Home'">
                  <xsl:attribute name="class">first active xs-only </xsl:attribute>
                </xsl:if>
                <xsl:apply-templates select="Menu/MenuItem" mode="menuLink">
                  <xsl:with-param name="class">nav-link</xsl:with-param>
                </xsl:apply-templates>
              </li>
            </xsl:if>
            <xsl:choose>
              <xsl:when test="$nav-dropdown='true'">
                <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Information' and @name!='Footer']" mode="mainmenudropdown">
                  <xsl:with-param name="overviewLink">true</xsl:with-param>
                </xsl:apply-templates>
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
          <!--SOCIAL-->
          <xsl:if test="$social-links='true'">
            <div class="socialLinksHeader not-xs" id="socialLinksHeader">
              <!--<xsl:apply-templates select="Contents/Content[@name='socialLinks']" mode="displayBrief"/>-->
              <xsl:apply-templates select="/Page" mode="addSingleModule">
                <xsl:with-param name="text">Add Social Links</xsl:with-param>
                <xsl:with-param name="position">socialLinksHeader</xsl:with-param>
                <xsl:with-param name="class">socialLinksHeader</xsl:with-param>
              </xsl:apply-templates>
            </div>
          </xsl:if>
          <!--SEARCH (DESKTOP)-->
          <xsl:if test="$search='on' and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
            <div class="not-xs search-wrapper">
              <xsl:apply-templates select="/" mode="searchSimple"/>
            </div>
          </xsl:if>
          <!--LOGON BUTTON (DESKTOP)-->
          <xsl:if test="$membership='on'">
            <xsl:apply-templates select="/" mode="loginSimple"/>
          </xsl:if>
          <!--CART-->
          <xsl:if test="$cart='on' and not($cartPage)">
            <xsl:apply-templates select="/" mode="cartSimple"/>
          </xsl:if>
        </div>
      </div>
    </nav>
  </xsl:template>
</xsl:stylesheet>

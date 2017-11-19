<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!--
      GUIDE TO HEADERS:
      header1 - long main menu below header
      header2 - menu and other elements to right of logo (using columns)
      header3 - menu to right of logo, other elements in bar above
   -->
  <xsl:variable name="membership">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'Membership'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name="cart">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'Cart'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name="search">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'Search'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name="currentPage" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]"/>
  <xsl:variable name="sectionPage" select="/Page/Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]"/>
  <xsl:variable name="siteURL">
    <xsl:call-template name="getSiteURL"/>
  </xsl:variable>

  <!-- ############################################ HEADERS ############################################### -->
  <xsl:template match="Page" mode="header1">
    <xsl:param name="nav-collapse" />
    <xsl:param name="cart-style" />
    <xsl:param name="login-style" />
    <xsl:param name="login-position" />
    <xsl:param name="login-action" />
    <!-- long main menu below header-->
    <div class="header-wrapper header1">
      <xsl:if test="$membership='on' and $login-position='top'">
        <div id="login-form-bar">
          <xsl:if test="$login-action='slide'">
            <xsl:attribute name="class">
              <xsl:text>login-slide-bar</xsl:text>
            </xsl:attribute>
          </xsl:if>
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
      <div class="container not-xs">
        <div class="clearfix header">
          <!--~~~~~~~~~~~~~~ LOGO ~~~~~~~~~~~~~~ -->
          <div class="pull-left logo">
            <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
              <xsl:with-param name="type">Image</xsl:with-param>
              <xsl:with-param name="text">Add Logo</xsl:with-param>
              <xsl:with-param name="name">Logo</xsl:with-param>
              <xsl:with-param name="class">pull-left logo</xsl:with-param>
            </xsl:apply-templates>
            <xsl:apply-templates select="/Page/Contents/Content[@name='Logo']" mode="displayBrief">
              <xsl:with-param name="maxWidth" select="'300'"/>
              <xsl:with-param name="maxHeight" select="'200'"/>
            </xsl:apply-templates>
          </div>
          <!--~~~~~~~~~~~~~~ STRAPLINE ~~~~~~~~~~~~~~ -->
          <div id="strapline" class="pull-right not-xs">

            <!--~~~~~~~~~~~~~~ INFO MENU ~~~~~~~~~~~~~~ -->
            <xsl:if test="$membership='on' and $login-position='top'">
              <div class="pull-right not-xs">
                <xsl:apply-templates select="/" mode="loginTop">
                  <xsl:with-param name="apply-login-action">
                    <xsl:value-of select="$login-action"/>
                  </xsl:with-param>
                </xsl:apply-templates>
              </div>
            </xsl:if>
            <xsl:if test="Menu/MenuItem/MenuItem[@name='Information']/MenuItem and not($currentPage/DisplayName[@nonav='true'])">
              <ul class="nav nav-pills pull-right info-nav">
                <xsl:if test="$HomeInfo='true'">
                  <li>
                    <xsl:apply-templates select="/Page/Menu/MenuItem" mode="menuLink"/>
                  </li>
                </xsl:if>
                <xsl:for-each select="Menu/MenuItem/MenuItem[@name='Information']/MenuItem[not(DisplayName/@exclude='true')]">
                  <li>
                    <xsl:if test="position()=last()">
                      <xsl:attribute name="class">last</xsl:attribute>
                    </xsl:if>
                    <xsl:apply-templates select="." mode="menuLink"/>
                  </li>
                </xsl:for-each>
              </ul>
            </xsl:if>
            <div class="terminus">&#160;</div>

            <div class="strapline">
              <xsl:apply-templates select="Contents/Content[@name='Strapline']" mode="displayBrief"/>
              <xsl:apply-templates select="/Page" mode="addSingleModule">
                <xsl:with-param name="text">Add Strapline</xsl:with-param>
                <xsl:with-param name="position">Strapline</xsl:with-param>
                <xsl:with-param name="class">pull-left strapline</xsl:with-param>
              </xsl:apply-templates>
            </div>
            <xsl:if test="$cart='on'">
              <xsl:apply-templates select="/" mode="cartBrief">
                <xsl:with-param name="apply-cart-style">
                  <xsl:value-of select="$cart-style"/>
                </xsl:with-param>
              </xsl:apply-templates>
            </xsl:if>
            <xsl:if test="$membership='on' and not($login-position='top')">
              <xsl:apply-templates select="/" mode="loginBrief">
                <xsl:with-param name="apply-login-style">
                  <xsl:value-of select="$login-style"/>
                </xsl:with-param>
              </xsl:apply-templates>
            </xsl:if>
            <xsl:if test="$search='on'">
              <xsl:apply-templates select="/" mode="searchBrief"/>
            </xsl:if>
          </div>
        </div>
      </div>
    </div>
    <div class="nav-wrapper">
      <!--<xsl:if test="not($currentPage/DisplayName[@nonav='true'])">-->
      <div class="navbar navbar-default">
        <div class="container">
          <div class="navbar-header xs-only">
            <xsl:if test="not($currentPage/DisplayName[@nonav='true'])">
              <button type="button" class="navbar-toggle" data-toggle="collapse" data-target="#navbar-main">
                <span class="icon-bar">
                  <xsl:text> </xsl:text>
                </span>
                <span class="icon-bar">
                  <xsl:text> </xsl:text>
                </span>
                <span class="icon-bar">
                  <xsl:text> </xsl:text>
                </span>
              </button>
            </xsl:if>
            <!--~~~~~~~~~~~~~~~~ XS HEADER ~~~~~~~~~~~~~~~~-->
            <div class="navbar-brand">
              <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
                <xsl:with-param name="type">Image</xsl:with-param>
                <xsl:with-param name="text">Add Logo</xsl:with-param>
                <xsl:with-param name="name">Logo</xsl:with-param>
                <xsl:with-param name="class">navbar-brand</xsl:with-param>
              </xsl:apply-templates>
              <xsl:apply-templates select="/Page/Contents/Content[@name='Logo']" mode="displayBrief">
                <xsl:with-param name="maxWidth" select="'150'"/>
                <xsl:with-param name="maxHeight" select="'50'"/>
              </xsl:apply-templates>
            </div>
            <xsl:if test="$search='on'">
              <div class="xs-only xs-search pull-right">
                <xsl:apply-templates select="/" mode="searchBriefxs"/>
              </div>
            </xsl:if>
            <xsl:if test="$cart='on'">
              <div class="xs-cart">
                <xsl:apply-templates select="/" mode="cartBriefxs">
                  <xsl:with-param name="apply-cart-style">
                    <xsl:value-of select="cart-style"/>
                  </xsl:with-param>
                </xsl:apply-templates>
              </div>
            </xsl:if>
            <xsl:if test="$membership='on'">
              <div class="xs-login">
                <xsl:if test="$membership='on' and not($login-position='top')">
                  <xsl:apply-templates select="/" mode="loginBriefxs" />
                </xsl:if>
                <xsl:if test="$membership='on' and $login-position='top'">
                  <xsl:apply-templates select="/" mode="loginTopxs" >
                    <xsl:with-param name="apply-login-action">
                      <xsl:value-of select="$login-action"/>
                    </xsl:with-param>
                  </xsl:apply-templates>
                </xsl:if>
              </div>
            </xsl:if>
          </div>
          <!--~~~~~~~~~~~~~~ MENU ~~~~~~~~~~~~~~ -->
          <xsl:if test="not($currentPage/DisplayName[@nonav='true'])">
            <div class="navbar-collapse collapse" id="navbar-main">
              <xsl:if test="$themeLayout!='SideNav'">
                <xsl:if test="Menu/MenuItem/MenuItem[@name!='Information' and @name!='Footer']">
                  <!--~~~~~~~~~~~~~~ MAIN MENU ~~~~~~~~~~~~~~ -->
                  <ul class="nav navbar-nav long-menu not-xs">
                    <xsl:if test="not(contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 8') or contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 7') or contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 6.0')) and $nav-collapse='true'">
                      <xsl:attribute name="class">
                        nav navbar-nav long-menu not-xs nav-add-more-auto
                      </xsl:attribute>
                    </xsl:if>
                    <xsl:if test="$HomeNav='true'">
                      <li class="first">
                        <xsl:if test="$currentPage/@name='Home'">
                          <xsl:attribute name="class">first active </xsl:attribute>
                        </xsl:if>
                        <a href="/">
                          <xsl:if test="$currentPage/@name='Home'">
                            <xsl:attribute name="class">active </xsl:attribute>
                          </xsl:if>
                          <xsl:text>Home</xsl:text>
                        </a>
                      </li>
                    </xsl:if>
                    <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Information' and @name!='Footer']" mode="mainmenu"/>
                  </ul>
                  <!--~~~~~~~~~~~~~~ XS MENU ~~~~~~~~~~~~~~ -->
                  <ul class="nav navbar-nav long-menu xs-only">
                    <xsl:if test="$HomeNav='true' or $HomeInfo='true'">
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
                    <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Information' and @name!='Footer']" mode="mainmenu"/>
                  </ul>

                  <!--~~~~~~~~~~~~~~ XS INFO MENU ~~~~~~~~~~~~~~ -->
                  <xsl:if test="Menu/MenuItem/MenuItem[@name='Information']/MenuItem">
                    <ul class="nav navbar-nav navbar-right xs-only">
                      <xsl:for-each select="Menu/MenuItem/MenuItem[@name='Information']/MenuItem[not(DisplayName/@exclude='true')]">
                        <li>
                          <xsl:if test="position()=last()">
                            <xsl:attribute name="class">last</xsl:attribute>
                          </xsl:if>
                          <xsl:apply-templates select="." mode="menuLink"/>
                        </li>
                      </xsl:for-each>
                    </ul>
                  </xsl:if>
                </xsl:if>
              </xsl:if>
            </div>
          </xsl:if>
        </div>
      </div>
      <!--</xsl:if>-->
    </div>
  </xsl:template>
  <xsl:template match="Page" mode="header2">
    <!-- logo on left, all other elements on right in seperate column -->
    <xsl:param name="cart-style" />
    <xsl:param name="login-style" />
    <xsl:param name="login-position" />
    <xsl:param name="logo-col-width"/>
    <xsl:param name="login-action" />
    <xsl:param name="order"/>
    <xsl:if test="$membership='on' and $login-position='top'">
      <div id="login-form-bar">
        <xsl:if test="$login-action='slide'">
          <xsl:attribute name="class">
            <xsl:text>login-slide-bar</xsl:text>
          </xsl:attribute>
        </xsl:if>
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
    <div class="header-wrapper header2">
      <div class="container">
        <div class="clearfix header row">
          <div class="col-sm-2 logo-col not-xs">
            <xsl:if test="$logo-col-width!=''">
              <xsl:attribute name="class">
                <xsl:text>col-sm-</xsl:text>
                <xsl:value-of select="$logo-col-width"/>
                <xsl:text>  logo-col not-xs</xsl:text>
              </xsl:attribute>
            </xsl:if>
            <!--~~~~~~~~~~~~~~ LOGO ~~~~~~~~~~~~~~ -->
            <div class="pull-left logo">
              <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
                <xsl:with-param name="type">Image</xsl:with-param>
                <xsl:with-param name="text">Add Logo</xsl:with-param>
                <xsl:with-param name="name">Logo</xsl:with-param>
                <xsl:with-param name="class">pull-left logo</xsl:with-param>
              </xsl:apply-templates>
              <xsl:apply-templates select="/Page/Contents/Content[@name='Logo']" mode="displayBrief">
                <xsl:with-param name="maxWidth" select="'300'"/>
                <xsl:with-param name="maxHeight" select="'200'"/>
              </xsl:apply-templates>
            </div>
          </div>
          <div class="col-sm-10 nav-col">
            <xsl:if test="$logo-col-width!=''">
              <xsl:attribute name="class">
                <xsl:text>col-sm-</xsl:text>
                <xsl:value-of select="12 - $logo-col-width"/>
                <xsl:text> nav-col</xsl:text>
              </xsl:attribute>
            </xsl:if>
            <!--~~~~~~~~~~~~~~ INFO MENU ~~~~~~~~~~~~~~ -->
            <xsl:if test="$membership='on' and $login-position='top'">
              <div class="pull-right not-xs">
                <xsl:apply-templates select="/" mode="loginTop">
                  <xsl:with-param name="apply-login-action">
                    <xsl:value-of select="$login-action"/>
                  </xsl:with-param>
                </xsl:apply-templates>
              </div>
            </xsl:if>
            <xsl:if test="Menu/MenuItem/MenuItem[@name='Information']/MenuItem">
              <ul class="nav nav-pills info-nav not-xs">
                <xsl:if test="$HomeInfo='true'">
                  <li>
                    <xsl:apply-templates select="/Page/Menu/MenuItem" mode="menuLink"/>
                  </li>
                </xsl:if>
                <xsl:for-each select="Menu/MenuItem/MenuItem[@name='Information']/MenuItem[not(DisplayName/@exclude='true')]">
                  <li>
                    <xsl:if test="position()=last()">
                      <xsl:attribute name="class">last</xsl:attribute>
                    </xsl:if>
                    <xsl:apply-templates select="." mode="menuLink"/>
                  </li>
                </xsl:for-each>
              </ul>
            </xsl:if>
            <div class="terminus">&#160;</div>
            <!--~~~~~~~~~~~~~~ STRAPLINE ~~~~~~~~~~~~~~ -->
            <div class="strapline not-xs">
              <xsl:apply-templates select="Contents/Content[@name='Strapline']" mode="displayBrief"/>
              <xsl:apply-templates select="/Page" mode="addSingleModule">
                <xsl:with-param name="text">Add Strapline</xsl:with-param>
                <xsl:with-param name="position">Strapline</xsl:with-param>
                <xsl:with-param name="class">pull-right strapline</xsl:with-param>
              </xsl:apply-templates>
            </div>
            <xsl:if test="$search='on'">
              <xsl:apply-templates select="/" mode="searchBrief"/>
            </xsl:if>
            <xsl:if test="$cart='on'">
              <div class="not-xs">
                <xsl:apply-templates select="/" mode="cartBrief">
                  <xsl:with-param name="apply-cart-style">
                    <xsl:value-of select="$cart-style"/>
                  </xsl:with-param>
                </xsl:apply-templates>
              </div>
            </xsl:if>
            <xsl:if test="$membership='on' and not($login-position='top')">
              <div class="not-xs">
                <xsl:apply-templates select="/" mode="loginBrief">
                  <xsl:with-param name="apply-login-style">
                    <xsl:value-of select="$login-style"/>
                  </xsl:with-param>
                </xsl:apply-templates>
              </div>
            </xsl:if>
            <div class="terminus">&#160;</div>
            <div class="nav-wrapper">
              <xsl:if test="not($currentPage/DisplayName[@nonav='true'])">
                <div class="navbar navbar-default">
                  <div class="row">
                    <div class="navbar-header xs-only">
                      <button type="button" class="navbar-toggle" data-toggle="collapse" data-target="#navbar-main">
                        <span class="icon-bar">
                          <xsl:text> </xsl:text>
                        </span>
                        <span class="icon-bar">
                          <xsl:text> </xsl:text>
                        </span>
                        <span class="icon-bar">
                          <xsl:text> </xsl:text>
                        </span>
                      </button>
                      <!--~~~~~~~~~~~~~~~~ XS HEADER ~~~~~~~~~~~~~~~~-->
                      <div class="navbar-brand">
                        <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
                          <xsl:with-param name="type">Image</xsl:with-param>
                          <xsl:with-param name="text">Add Logo</xsl:with-param>
                          <xsl:with-param name="name">Logo</xsl:with-param>
                          <xsl:with-param name="class">navbar-brand</xsl:with-param>
                        </xsl:apply-templates>
                        <xsl:apply-templates select="/Page/Contents/Content[@name='Logo']" mode="displayBrief">
                          <xsl:with-param name="maxWidth" select="'150'"/>
                          <xsl:with-param name="maxHeight" select="'50'"/>
                        </xsl:apply-templates>
                      </div>
                      <xsl:if test="$search='on'">
                        <div class="xs-only xs-search pull-right">
                          <xsl:apply-templates select="/" mode="searchBriefxs"/>
                        </div>
                      </xsl:if>
                      <xsl:if test="$cart='on'">
                        <div class="xs-cart">
                          <xsl:apply-templates select="/" mode="cartBriefxs" />
                        </div>
                      </xsl:if>
                      <xsl:if test="$membership='on' and not($login-position='top')">
                        <div class="xs-login">
                          <xsl:apply-templates select="/" mode="loginBriefxs" />
                        </div>
                      </xsl:if>
                      <xsl:if test="$membership='on' and $login-position='top'">
                        <xsl:apply-templates select="/" mode="loginTopxs" >
                          <xsl:with-param name="apply-login-action">
                            <xsl:value-of select="$login-action"/>
                          </xsl:with-param>
                        </xsl:apply-templates>
                      </xsl:if>
                    </div>
                    <!--~~~~~~~~~~~~~~ MENU ~~~~~~~~~~~~~~ -->

                    <div class="navbar-collapse collapse" id="navbar-main">
                      <xsl:if test="$themeLayout!='SideNav'">
                        <xsl:if test="Menu/MenuItem/MenuItem[@name!='Information' and @name!='Footer']">
                          <!--~~~~~~~~~~~~~~ MAIN MENU ~~~~~~~~~~~~~~ -->
                          <ul class="nav navbar-nav not-xs" style="margin-left:0;margin-right:0;">
                            <xsl:if test="$HomeNav='true'">
                              <li class="first">
                                <xsl:if test="$currentPage/@name='Home'">
                                  <xsl:attribute name="class">first active </xsl:attribute>
                                </xsl:if>
                                <a href="/">
                                  <xsl:if test="$currentPage/@name='Home'">
                                    <xsl:attribute name="class">active </xsl:attribute>
                                  </xsl:if>
                                  <xsl:text>Home</xsl:text>
                                </a>
                              </li>
                            </xsl:if>
                            <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Information' and @name!='Footer']" mode="mainmenu"/>
                          </ul>
                          <!--~~~~~~~~~~~~~~ XS MENU ~~~~~~~~~~~~~~ -->
                          <ul class="nav navbar-nav long-menu xs-only" style="margin-left:0;margin-right:0;">
                            <xsl:if test="$HomeNav='true' or $HomeInfo='true'">
                              <li class="first">
                                <a href="/">
                                  <i class="icon icon-home icon-lg">
                                    <xsl:text> </xsl:text>
                                  </i>
                                  Home
                                </a>
                              </li>
                            </xsl:if>
                            <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Information' and @name!='Footer']" mode="mainmenu"/>
                          </ul>

                          <!--~~~~~~~~~~~~~~ XS INFO MENU ~~~~~~~~~~~~~~ -->
                          <xsl:if test="Menu/MenuItem/MenuItem[@name='Information']/MenuItem">
                            <ul class="nav navbar-nav navbar-right xs-only" style="margin-left:0;margin-right:0;">
                              <xsl:for-each select="Menu/MenuItem/MenuItem[@name='Information']/MenuItem[not(DisplayName/@exclude='true')]">
                                <li>
                                  <xsl:if test="position()=last()">
                                    <xsl:attribute name="class">last</xsl:attribute>
                                  </xsl:if>
                                  <xsl:apply-templates select="." mode="menuLink"/>
                                </li>
                              </xsl:for-each>
                            </ul>
                          </xsl:if>
                        </xsl:if>
                      </xsl:if>
                    </div>
                  </div>
                </div>
              </xsl:if>
            </div>
          </div>
        </div>
      </div>

    </div>
  </xsl:template>
  <xsl:template match="Page" mode="header3">
    <!-- info menu and other elements in bar above, logo left, nav right-->
    <div class="info-header">
      <div class="container">
        <!--~~~~~~~~~~~~~~ STRAPLINE ~~~~~~~~~~~~~~ -->
        <div class="pull-right strapline">
          <xsl:apply-templates select="Contents/Content[@name='Strapline']" mode="displayBrief"/>
          <xsl:apply-templates select="/Page" mode="addSingleModule">
            <xsl:with-param name="text">Add Strapline</xsl:with-param>
            <xsl:with-param name="position">Strapline</xsl:with-param>
            <xsl:with-param name="class">pull-right strapline</xsl:with-param>
          </xsl:apply-templates>
        </div>
        <!--~~~~~~~~~~~~~~ INFO MENU ~~~~~~~~~~~~~~ -->
        <xsl:if test="Menu/MenuItem/MenuItem[@name='Information']/MenuItem">
          <ul class="nav nav-pills pull-right info-nav not-xs">
            <xsl:if test="$HomeInfo='true'">
              <li>
                <xsl:apply-templates select="/Page/Menu/MenuItem" mode="menuLink"/>
              </li>
            </xsl:if>
            <xsl:for-each select="Menu/MenuItem/MenuItem[@name='Information']/MenuItem[not(DisplayName/@exclude='true')]">
              <li>
                <xsl:if test="position()=last()">
                  <xsl:attribute name="class">last</xsl:attribute>
                </xsl:if>
                <xsl:apply-templates select="." mode="menuLink"/>
              </li>
            </xsl:for-each>
          </ul>
        </xsl:if>
        <div class="xs-search pull-right">
          <div class="dropdown">
            <a data-toggle="dropdown" href="#" class="search-xs-btn">
              <i class="fa fa-search fa-2x">
                <xsl:text> </xsl:text>
              </i>
            </a>
            <div class="dropdown-menu" role="menu" aria-labelledby="dLabel">
              <span>
                <div class="topSearch">
                  <form method="post" action="/information/search" id="searchInputxs" class="ewXform">
                    <input type="hidden" name="searchMode" value="REGEX" />
                    <input type="hidden" name="contentType" value="Product" />
                    <input type="hidden" name="searchFormId" value="8923" />
                    <input type="text" class="CTAsearch" name="searchString" id="searchStringxs" value="" />
                    <button type="submit" class="btn btn-sm btn-primary CTAsearch_button" name="Search" value="Submit">
                      <i class="fa fa-search">
                        <xsl:text> </xsl:text>
                      </i>
                    </button>
                  </form>
                </div>
              </span>
            </div>
          </div>
        </div>
        <xsl:if test="$cart='on'">
          <div class="xs-cart">
            <xsl:apply-templates select="/" mode="cartBriefxs" />
          </div>
        </xsl:if>
        <xsl:if test="$membership='on'">
          <div class="xs-login">
            <xsl:apply-templates select="/" mode="loginBriefxs" />
          </div>
        </xsl:if>
      </div>
    </div>
    <div class="header-wrapper header3">
      <div class="container xs-remove-padding">
        <div class="clearfix header">

          <div class="nav-wrapper">
            <xsl:if test="not($currentPage/DisplayName[@nonav='true'])">

              <div class="navbar navbar-default">
                <div class="navbar-header">
                  <button type="button" class="navbar-toggle" data-toggle="collapse" data-target="#navbar-main">
                    <span class="icon-bar">
                      <xsl:text> </xsl:text>
                    </span>
                    <span class="icon-bar">
                      <xsl:text> </xsl:text>
                    </span>
                    <span class="icon-bar">
                      <xsl:text> </xsl:text>
                    </span>
                  </button>
                  <!--~~~~~~~~~~~~~~~~ LOGO ~~~~~~~~~~~~~~~~-->
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

                </div>
                <!--~~~~~~~~~~~~~~ MENU ~~~~~~~~~~~~~~ -->

                <div class="navbar-collapse collapse" id="navbar-main">
                  <xsl:if test="$themeLayout!='SideNav'">
                    <xsl:if test="Menu/MenuItem/MenuItem[@name!='Information' and @name!='Footer']">
                      <!--~~~~~~~~~~~~~~ MAIN MENU ~~~~~~~~~~~~~~ -->
                      <ul class="nav navbar-nav long-menu not-xs">
                        <xsl:if test="$HomeNav='true'">
                          <li class="first">
                            <xsl:if test="$currentPage/@name='Home'">
                              <xsl:attribute name="class">first active </xsl:attribute>
                            </xsl:if>
                            <a href="/">
                              <xsl:if test="$currentPage/@name='Home'">
                                <xsl:attribute name="class">active </xsl:attribute>
                              </xsl:if>
                              <xsl:text>Home</xsl:text>
                            </a>
                          </li>
                        </xsl:if>
                        <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Information' and @name!='Footer']" mode="mainmenu"/>
                      </ul>
                      <!--~~~~~~~~~~~~~~ MAIN MENU ~~~~~~~~~~~~~~ -->
                      <ul class="nav navbar-nav long-menu xs-only">
                        <xsl:if test="$HomeNav='true' or $HomeInfo='true'">
                          <li class="first">
                            <a href="/">
                              Home
                            </a>
                          </li>
                        </xsl:if>
                        <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Information' and @name!='Footer']" mode="mainmenu"/>
                      </ul>

                      <!--~~~~~~~~~~~~~~ XS INFO MENU ~~~~~~~~~~~~~~ -->
                      <xsl:if test="Menu/MenuItem/MenuItem[@name='Information']/MenuItem">
                        <ul class="nav navbar-nav navbar-right xs-only">
                          <xsl:for-each select="Menu/MenuItem/MenuItem[@name='Information']/MenuItem[not(DisplayName/@exclude='true')]">
                            <li>
                              <xsl:if test="position()=last()">
                                <xsl:attribute name="class">last</xsl:attribute>
                              </xsl:if>
                              <xsl:apply-templates select="." mode="menuLink"/>
                            </li>
                          </xsl:for-each>
                        </ul>
                      </xsl:if>
                    </xsl:if>
                  </xsl:if>
                </div>
              </div>
            </xsl:if>
          </div>
        </div>
      </div>
    </div>
  </xsl:template>
  <xsl:template match="Page" mode="header4">
    <!-- logo on left, all other elements on right in seperate column -->
    <xsl:param name="cart-style" />
    <xsl:param name="login-style" />
    <xsl:param name="login-position" />
    <xsl:param name="logo-col-width"/>
    <xsl:param name="login-action" />
    <xsl:param name="order"/>
    <xsl:if test="$membership='on' and $login-position='top'">
      <div id="login-form-bar">
        <xsl:if test="$login-action='slide'">
          <xsl:attribute name="class">
            <xsl:text>login-slide-bar</xsl:text>
          </xsl:attribute>
        </xsl:if>
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
    <div class="header-wrapper header4">
      <div class="container">
        <div class="clearfix header">
          <!--~~~~~~~~~~~~~~ LOGO ~~~~~~~~~~~~~~ -->
          <div class="pull-left logo not-xs">
            <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
              <xsl:with-param name="type">Image</xsl:with-param>
              <xsl:with-param name="text">Add Logo</xsl:with-param>
              <xsl:with-param name="name">Logo</xsl:with-param>
              <xsl:with-param name="class">pull-left logo</xsl:with-param>
            </xsl:apply-templates>
            <xsl:apply-templates select="/Page/Contents/Content[@name='Logo']" mode="displayBrief">
              <xsl:with-param name="maxWidth" select="'300'"/>
              <xsl:with-param name="maxHeight" select="'200'"/>
            </xsl:apply-templates>
          </div>
          <div class="pull-right header-utility">
            <!--~~~~~~~~~~~~~~ INFO MENU ~~~~~~~~~~~~~~ -->
            <xsl:if test="$membership='on' and $login-position='top'">
              <div class="pull-right not-xs">
                <xsl:apply-templates select="/" mode="loginTop">
                  <xsl:with-param name="apply-login-action">
                    <xsl:value-of select="$login-action"/>
                  </xsl:with-param>
                </xsl:apply-templates>
              </div>
            </xsl:if>
            <xsl:if test="Menu/MenuItem/MenuItem[@name='Information']/MenuItem">
              <ul class="nav nav-pills info-nav not-xs">
                <xsl:if test="$HomeInfo='true'">
                  <li>
                    <xsl:apply-templates select="/Page/Menu/MenuItem" mode="menuLink"/>
                  </li>
                </xsl:if>
                <xsl:for-each select="Menu/MenuItem/MenuItem[@name='Information']/MenuItem[not(DisplayName/@exclude='true')]">
                  <li>
                    <xsl:if test="position()=last()">
                      <xsl:attribute name="class">last</xsl:attribute>
                    </xsl:if>
                    <xsl:apply-templates select="." mode="menuLink"/>
                  </li>
                </xsl:for-each>
              </ul>
            </xsl:if>
            <div class="terminus">&#160;</div>
            <!--~~~~~~~~~~~~~~ STRAPLINE ~~~~~~~~~~~~~~ -->
            <div class="strapline not-xs">
              <xsl:apply-templates select="Contents/Content[@name='Strapline']" mode="displayBrief"/>
              <xsl:apply-templates select="/Page" mode="addSingleModule">
                <xsl:with-param name="text">Add Strapline</xsl:with-param>
                <xsl:with-param name="position">Strapline</xsl:with-param>
                <xsl:with-param name="class">strapline</xsl:with-param>
              </xsl:apply-templates>
            </div>
            <xsl:if test="$search='on'">
              <xsl:apply-templates select="/" mode="searchBrief"/>
            </xsl:if>
            <xsl:if test="$cart='on'">
              <div class="not-xs cart-wrapper">
                <xsl:apply-templates select="/" mode="cartBrief">
                  <xsl:with-param name="apply-cart-style">
                    <xsl:value-of select="$cart-style"/>
                  </xsl:with-param>
                </xsl:apply-templates>
              </div>
            </xsl:if>
            <xsl:if test="$membership='on' and not($login-position='top')">
              <div class="not-xs">
                <xsl:apply-templates select="/" mode="loginBrief">
                  <xsl:with-param name="apply-login-style">
                    <xsl:value-of select="$login-style"/>
                  </xsl:with-param>
                </xsl:apply-templates>
              </div>
            </xsl:if>
          </div>
          <div class="nav-wrapper">
            <xsl:if test="not($currentPage/DisplayName[@nonav='true'])">
              <div class="navbar navbar-default">
                <div class="row">
                  <div class="navbar-header xs-only">
                    <button type="button" class="navbar-toggle" data-toggle="collapse" data-target="#navbar-main">
                      <span class="icon-bar">
                        <xsl:text> </xsl:text>
                      </span>
                      <span class="icon-bar">
                        <xsl:text> </xsl:text>
                      </span>
                      <span class="icon-bar">
                        <xsl:text> </xsl:text>
                      </span>
                    </button>
                    <!--~~~~~~~~~~~~~~~~ XS HEADER ~~~~~~~~~~~~~~~~-->
                    <div class="navbar-brand">
                      <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
                        <xsl:with-param name="type">Image</xsl:with-param>
                        <xsl:with-param name="text">Add Logo</xsl:with-param>
                        <xsl:with-param name="name">Logo</xsl:with-param>
                        <xsl:with-param name="class">navbar-brand</xsl:with-param>
                      </xsl:apply-templates>
                      <xsl:apply-templates select="/Page/Contents/Content[@name='Logo']" mode="displayBrief">
                        <xsl:with-param name="maxWidth" select="'150'"/>
                        <xsl:with-param name="maxHeight" select="'50'"/>
                      </xsl:apply-templates>
                    </div>
                    <xsl:if test="$search='on'">
                      <div class="xs-only xs-search pull-right">
                        <xsl:apply-templates select="/" mode="searchBriefxs"/>
                      </div>
                    </xsl:if>
                    <xsl:if test="$cart='on'">
                      <div class="xs-cart">
                        <xsl:apply-templates select="/" mode="cartBriefxs" />
                      </div>
                    </xsl:if>
                    <xsl:if test="$membership='on' and not($login-position='top')">
                      <div class="xs-login">
                        <xsl:apply-templates select="/" mode="loginBriefxs" />
                      </div>
                    </xsl:if>
                    <xsl:if test="$membership='on' and $login-position='top'">
                      <div class="xs-login">
                        <xsl:apply-templates select="/" mode="loginTopxs" >
                          <xsl:with-param name="apply-login-action">
                            <xsl:value-of select="$login-action"/>
                          </xsl:with-param>
                        </xsl:apply-templates>
                      </div>
                    </xsl:if>
                  </div>
                  <!--~~~~~~~~~~~~~~ MENU ~~~~~~~~~~~~~~ -->

                  <div class="navbar-collapse collapse" id="navbar-main">
                    <xsl:if test="$themeLayout!='SideNav'">
                      <xsl:if test="Menu/MenuItem/MenuItem[@name!='Information' and @name!='Footer']">
                        <!--~~~~~~~~~~~~~~ MAIN MENU ~~~~~~~~~~~~~~ -->
                        <ul class="nav navbar-nav not-xs" style="margin-left:0;margin-right:0;">
                          <xsl:if test="$HomeNav='true'">
                            <li class="first">
                              <xsl:if test="$currentPage/@name='Home'">
                                <xsl:attribute name="class">first active </xsl:attribute>
                              </xsl:if>
                              <a href="/">
                                <xsl:if test="$currentPage/@name='Home'">
                                  <xsl:attribute name="class">active </xsl:attribute>
                                </xsl:if>
                                <xsl:text>Home</xsl:text>
                              </a>
                            </li>
                          </xsl:if>
                          <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Information' and @name!='Footer']" mode="mainmenu"/>
                        </ul>
                        <!--~~~~~~~~~~~~~~ XS MENU ~~~~~~~~~~~~~~ -->
                        <ul class="nav navbar-nav long-menu xs-only" style="margin-left:0;margin-right:0;">
                          <xsl:if test="$HomeNav='true' or $HomeInfo='true'">
                            <li class="first">
                              <a href="/">
                                <i class="icon icon-home icon-lg">
                                  <xsl:text> </xsl:text>
                                </i>
                                Home
                              </a>
                            </li>
                          </xsl:if>
                          <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Information' and @name!='Footer']" mode="mainmenu"/>
                        </ul>

                        <!--~~~~~~~~~~~~~~ XS INFO MENU ~~~~~~~~~~~~~~ -->
                        <xsl:if test="Menu/MenuItem/MenuItem[@name='Information']/MenuItem">
                          <ul class="nav navbar-nav navbar-right xs-only" style="margin-left:0;margin-right:0;">
                            <xsl:for-each select="Menu/MenuItem/MenuItem[@name='Information']/MenuItem[not(DisplayName/@exclude='true')]">
                              <li>
                                <xsl:if test="position()=last()">
                                  <xsl:attribute name="class">last</xsl:attribute>
                                </xsl:if>
                                <xsl:apply-templates select="." mode="menuLink"/>
                              </li>
                            </xsl:for-each>
                          </ul>
                        </xsl:if>
                      </xsl:if>
                    </xsl:if>
                  </div>
                </div>
              </div>
            </xsl:if>
          </div>
        </div>
      </div>
    </div>
  </xsl:template>
  <xsl:template match="Page" mode="header5">
    <xsl:param name="nav-collapse" />
    <xsl:param name="cart-style" />
    <xsl:param name="login-style" />
    <xsl:param name="login-position" />
    <xsl:param name="login-action" />

    <xsl:if test="$membership='on' and $login-position='top'">
      <div id="login-form-bar">
        <xsl:if test="$login-action='slide'">
          <xsl:attribute name="class">
            <xsl:text>login-slide-bar</xsl:text>
          </xsl:attribute>
        </xsl:if>
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
    <div class="navbar navbar-default header5">
      <div class="container-fluid">
        <!-- Brand and toggle get grouped for better mobile display -->
        <div class="navbar-header">
          <div class="container">
            <button type="button" class="navbar-toggle collapsed" data-toggle="collapse" data-target="#navbar-main" aria-expanded="false">
              <span class="sr-only">Toggle navigation</span>
              <span class="icon-bar">
                <xsl:text> </xsl:text>
              </span>
              <span class="icon-bar">
                <xsl:text> </xsl:text>
              </span>
              <span class="icon-bar">
                <xsl:text> </xsl:text>
              </span>
            </button>
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
            <div class="strapline">
              <xsl:apply-templates select="Contents/Content[@name='Strapline']" mode="displayBrief"/>
              <xsl:apply-templates select="/Page" mode="addSingleModule">
                <xsl:with-param name="text">Add Strapline</xsl:with-param>
                <xsl:with-param name="position">Strapline</xsl:with-param>
                <xsl:with-param name="class">pull-left strapline</xsl:with-param>
              </xsl:apply-templates>
            </div>
            <xsl:if test="$cart='on'">
              <xsl:apply-templates select="/" mode="cartBrief">
                <xsl:with-param name="apply-cart-style">
                  <xsl:value-of select="$cart-style"/>
                </xsl:with-param>
              </xsl:apply-templates>
              <div class="xs-cart">
                <xsl:apply-templates select="/" mode="cartBriefxs">
                  <xsl:with-param name="apply-cart-style">
                    <xsl:value-of select="cart-style"/>
                  </xsl:with-param>
                </xsl:apply-templates>
              </div>
            </xsl:if>
            <xsl:if test="$membership='on' and not($login-position='top')">
              <xsl:apply-templates select="/" mode="loginBrief">
                <xsl:with-param name="apply-login-style">
                  <xsl:value-of select="$login-style"/>
                </xsl:with-param>
              </xsl:apply-templates>
              <div class="xs-login">
                <xsl:if test="$membership='on' and not($login-position='top')">
                  <xsl:apply-templates select="/" mode="loginBriefxs" />
                </xsl:if>
                <xsl:if test="$membership='on' and $login-position='top'">
                  <xsl:apply-templates select="/" mode="loginTopxs" >
                    <xsl:with-param name="apply-login-action">
                      <xsl:value-of select="$login-action"/>
                    </xsl:with-param>
                  </xsl:apply-templates>
                </xsl:if>
              </div>
            </xsl:if>
            <xsl:if test="$search='on'">
              <xsl:apply-templates select="/" mode="searchBrief"/>
            </xsl:if>
            <div class="xs-only xs-search pull-right">
              <xsl:apply-templates select="/" mode="searchBriefxs"/>
            </div>
          </div>
        </div>

        <!-- Collect the nav links, forms, and other content for toggling -->
        <div class="collapse navbar-collapse" id="navbar-main">
          <div class="container">
            <!--~~~~~~~~~~~~~~ XS MENU ~~~~~~~~~~~~~~ -->
            <ul class="nav navbar-nav">
              <xsl:if test="$HomeNav='true' or $HomeInfo='true'">
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
              <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Information' and @name!='Footer']" mode="mainmenu"/>
            </ul>

            <!--~~~~~~~~~~~~~~ XS INFO MENU ~~~~~~~~~~~~~~ -->
            <xsl:if test="Menu/MenuItem/MenuItem[@name='Information']/MenuItem">
              <ul class="nav navbar-nav info-nav">
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
        </div>
      </div>
    </div>

  </xsl:template>

  <!-- ############################################ CART ############################################### -->

  <xsl:template match="/" mode="cartBrief">
    <xsl:param name="apply-cart-style"/>
    <xsl:choose>
      <xsl:when test="$apply-cart-style='2'">
        <xsl:apply-templates select="/" mode="cartBrief2" />
      </xsl:when>
      <xsl:when test="$apply-cart-style='3'">
        <xsl:apply-templates select="/" mode="cartBrief2xs" />
      </xsl:when>
      <xsl:when test="$apply-cart-style='4'">
        <xsl:apply-templates select="/" mode="cartBrief4" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="/" mode="cartBrief1" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="/" mode="cartBriefxs">
    <xsl:param name="apply-cart-style"/>
    <xsl:choose>
      <xsl:when test="$apply-cart-style='2'">
        <xsl:apply-templates select="/" mode="cartBrief2xs" />
      </xsl:when>
      <xsl:when test="$apply-cart-style='3'">
        <xsl:apply-templates select="/" mode="cartBrief2xs" />
      </xsl:when>
      <xsl:when test="$apply-cart-style='4'">
        <xsl:apply-templates select="/" mode="cartBrief4xs" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="/" mode="cartBrief1xs" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="/" mode="cartBrief1">
    <div id="cartBrief">
      <div class="cartinfo">
        <i class="fa fa-shopping-basket fa-3x cart-icon">
          <xsl:text> </xsl:text>
        </i>
        <xsl:choose>
          <xsl:when test="/Page/Cart/Order/@itemCount &gt; 0">
            <span id="itemCount">
              <xsl:value-of select="/Page/Cart/Order/@itemCount"/>
              <span id="itemCountLabel">
                &#160;item<xsl:if test="/Page/Cart/Order/@itemCount &gt; 1">s</xsl:if>
              </span>
            </span>
            <span id="itemTotal" class="pull-right">
              <xsl:text>&#160; </xsl:text>
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

            <span id="itemTotal" class="">
              &#160;
              <xsl:value-of select="/Page/Cart/@currencySymbol"/>
              <xsl:text>0.00</xsl:text>
            </span>
          </xsl:otherwise>
        </xsl:choose>
        <br />
        <a class=""  href="{$siteURL}{$currentPage/@url}?cartCmd=Cart" title="Click here to checkout" role="button">
          View basket&#160;<i class="fa fa-chevron-right">
            <xsl:text> </xsl:text>
          </i>
        </a>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="/" mode="cartBrief1xs">
    <div id="cartBriefxs">
      <div class="dropdown">
        <a data-toggle="dropdown" href="#">
          <i class="fa fa-shopping-basket fa-2x">
            <xsl:text> </xsl:text>
          </i>
        </a>
        <div class="dropdown-menu" role="menu" aria-labelledby="dLabel">
          <span class="basket-inner">
            <h5>Your basket</h5>
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
                    <xsl:text>&#160; </xsl:text>
                    <xsl:value-of select="/Page/Cart/@currencySymbol"/>
                    <xsl:text>0.00</xsl:text>
                  </span>
                </xsl:otherwise>
              </xsl:choose>
            </p>
            <a class="btn btn-sm btn-primary"  href="{$siteURL}{$currentPage/@url}?cartCmd=Cart" title="Click here to checkout" role="button">
              View basket&#160;<i class="fa fa-chevron-right">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </span>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="/" mode="cartBrief2">
    <div id="cartBrief">
      <div class="cartinfo">
        <a href="{$siteURL}{$currentPage/@url}?cartCmd=Cart" title="Click here to checkout" role="button">
          <i class="fa fa-shopping-basket">
            <xsl:text> </xsl:text>
          </i>
          <xsl:choose>
            <xsl:when test="/Page/Cart/Order/@itemCount &gt; 0">
              <span id="itemCount">
                <xsl:value-of select="/Page/Cart/Order/@itemCount"/>
                <span id="itemCountLabel">
                  &#160;item<xsl:if test="/Page/Cart/Order/@itemCount &gt; 1">s</xsl:if>
                </span>
              </span>
              <span id="itemTotal">
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
              <span id="itemTotal" class="">
                <strong>
                  &#160;
                  <xsl:value-of select="/Page/Cart/@currencySymbol"/>
                  <xsl:text>0.00</xsl:text>
                </strong>
              </span>
            </xsl:otherwise>
          </xsl:choose>
          <i class="fa fa-chevron-right">
            <xsl:text> </xsl:text>
          </i>
        </a>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="/" mode="cartBrief2xs">
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

  <xsl:template match="/" mode="cartBrief4">
    <div id="cartBrief">
      <div class="cartinfo">
        <a class=""  href="{$siteURL}{$currentPage/@url}?cartCmd=Cart" title="Click here to checkout" role="button">
          <i class="fa fa-shopping-basket">
            <xsl:text> </xsl:text>
          </i>
          <xsl:text> </xsl:text>
          <xsl:choose>
            <xsl:when test="/Page/Cart/Order/@itemCount &gt; 0">
              <span id="itemCount">
                <xsl:value-of select="/Page/Cart/Order/@itemCount"/>
                <span id="itemCountLabel">
                  &#160;item<xsl:if test="/Page/Cart/Order/@itemCount &gt; 1">s</xsl:if>
                </span>
              </span>
              <span id="itemTotal">
                <xsl:value-of select="/Page/Cart/@currencySymbol"/>
                <xsl:value-of select="/Page/Cart/Order/@total"/>
              </span>
            </xsl:when>
            <xsl:otherwise>
              <span>Your Basket is empty</span>
              <!--<span id="itemCount">
                <xsl:text>0&#160;</xsl:text>
                <span id="itemCountLabel">
                  <xsl:text>items</xsl:text>
                </span>
              </span>
              <span id="itemTotal" class="">
                <strong>
                  &#160;
                  <xsl:value-of select="/Page/Cart/@currencySymbol"/>
                  <xsl:text>0.00</xsl:text>
                </strong>
              </span>-->
            </xsl:otherwise>
          </xsl:choose>
          <xsl:text> </xsl:text>
          <i class="fa fa-chevron-right">
            <xsl:text> </xsl:text>
          </i>
        </a>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="/" mode="cartBrief4xs">
    <div id="cartBriefxs ">
      <div class="dropdown">
        <a data-toggle="dropdown" href="#">
          <i class="fa fa-shopping-basket fa-2x">
            <xsl:text> </xsl:text>
          </i>
        </a>
        <div class="dropdown-menu" role="menu" aria-labelledby="dLabel">
          <span class="basket-inner">
            <h5>Your Basket</h5>
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
                    <xsl:text>&#160; </xsl:text>
                    <xsl:value-of select="/Page/Cart/@currencySymbol"/>
                    <xsl:value-of select="/Page/Cart/Order/@total"/>
                  </span>
                </xsl:when>
                <xsl:otherwise>
                  <span>Basket is empty</span>
                  <!--<span id="itemCount">
                <xsl:text>0&#160;</xsl:text>
                <span id="itemCountLabel">
                  <xsl:text>items</xsl:text>
                </span>
              </span>
              <span id="itemTotal" class="">
                <strong>
                  &#160;
                  <xsl:value-of select="/Page/Cart/@currencySymbol"/>
                  <xsl:text>0.00</xsl:text>
                </strong>
              </span>-->
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

  <!-- ############################################# CART TEMPLATES ############################################### -->
  <!-- -->
  <xsl:template match="Order" mode="cartBrief">
    <xsl:variable name="parentURL">
      <xsl:call-template name="getContentParURL"/>
    </xsl:variable>
    <xsl:variable name="secureURL">
      <xsl:call-template name="getSecureURL"/>
    </xsl:variable>
    <xsl:variable name="siteURL">
      <xsl:call-template name="getSiteURL"/>
    </xsl:variable>
    <p class="cartBrief">
      <strong>Your Basket contains: </strong>
      <xsl:choose>
        <xsl:when test="@itemCount &gt; 0">
          <xsl:value-of select="@itemCount"/> item<xsl:if test="@itemCount &gt; 1">s</xsl:if> -  <xsl:value-of select="/Page/Cart/@currencySymbol"/>
          <xsl:value-of select="format-number(@total, '#.00')"/>
          <xsl:text> - </xsl:text>
          <a href="{$siteURL}?cartCmd=Cart" title="Click here to view full details of your shopping cart" class="morelink">View Basket</a>
        </xsl:when>
        <xsl:otherwise>No items</xsl:otherwise>
      </xsl:choose>
    </p>
  </xsl:template>

  <!-- ############################################ LOGIN ############################################### -->

  <xsl:template match="/" mode="loginBrief">
    <xsl:param name="apply-login-style"/>
    <xsl:choose>
      <xsl:when test="$apply-login-style='2'">
        <xsl:apply-templates select="/" mode="loginBrief2" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="/" mode="loginBrief1" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="/" mode="loginBriefxs">
    <xsl:param name="apply-login-style"/>
    <xsl:choose>
      <xsl:when test="$apply-login-style='2'">
        <xsl:apply-templates select="/" mode="loginBrief2xs" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="/" mode="loginBrief1xs" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="/" mode="loginBrief1">
    <div id="loginBrief">
      <div id="signin">
        <p class="loginText">
          <xsl:choose>
            <xsl:when test="/Page/User">
              <i class="fa fa-user fa-3x logged-in-icon">
                <xsl:text> </xsl:text>
              </i>
              <xsl:apply-templates select="/" mode="loggedIn" />
              <a href="/Information/Manage-Account">Manage Account</a>
              <span>
                <xsl:text>&#160;|&#160;</xsl:text>
              </span>
              <a href="?ewCmd=logoff">Log Out</a>
            </xsl:when>
            <xsl:otherwise>
              <i class="fa fa-user fa-3x logged-in-icon">
                <xsl:text> </xsl:text>
              </i>
              <xsl:text>Welcome:&#160;</xsl:text>
              <strong>
                <xsl:text>Guest</xsl:text>
              </strong>
              <br/>
              <a href="/Information/Login-Register" class="">
                Sign in / Register&#160;
                <i class="fa fa-chevron-right">
                  <xsl:text> </xsl:text>
                </i>
              </a>
            </xsl:otherwise>
          </xsl:choose>
        </p>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="/" mode="loginBrief1xs">
    <div id="loginBriefxs">

      <div class="dropdown">
        <a data-toggle="dropdown" href="#">
          <i class="fa fa-user fa-2x">
            <xsl:text> </xsl:text>
          </i>
        </a>
        <div class="dropdown-menu" role="menu" aria-labelledby="dLabel">
          <span class="membership-inner">
            <h5>
              <xsl:choose>
                <xsl:when test="/Page/User">
                  <xsl:apply-templates select="/" mode="loggedIn" />
                </xsl:when>
                <xsl:otherwise>
                  <div class="cartBriefWelcome">

                    <xsl:text>Welcome:&#160;</xsl:text>
                    <strong>
                      <xsl:text>Guest</xsl:text>
                    </strong>
                  </div>
                </xsl:otherwise>
              </xsl:choose>
            </h5>
            <p>
              <xsl:choose>
                <xsl:when test="/Page/User">

                  <a href="/Information/Manage-Account">
                    Manage Account&#160;<i class="fa fa-chevron-right">
                      <xsl:text> </xsl:text>
                    </i>
                  </a>
                  <br />
                  <a href="?ewCmd=logoff">
                    Log Out&#160;<i class="fa fa-chevron-right">
                      <xsl:text> </xsl:text>
                    </i>
                  </a>

                </xsl:when>
                <xsl:otherwise>
                  <a href="/Information/Login-Register" class="">
                    Sign in / Register&#160;
                    <i class="fa fa-chevron-right">
                      <xsl:text> </xsl:text>
                    </i>
                  </a>
                </xsl:otherwise>
              </xsl:choose>
            </p>

          </span>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="/" mode="loginBrief2">
    <div id="loginBrief">
      <div id="signin">
        <p class="loginText">
          <xsl:choose>
            <xsl:when test="/Page/User">
              <xsl:apply-templates select="/" mode="loggedIn" />
              <!--<a href="/Information/Manage-Account">Manage Account</a>-->
              <!--<span>
                <xsl:text>&#160;|&#160;</xsl:text>
              </span>-->
              <a href="?ewCmd=logoff" class="log-out">Log Out</a>
            </xsl:when>
            <xsl:otherwise>
              <a href="/Information/Login-Register" class="">
                <i class="fa fa-user">
                  <xsl:text> </xsl:text>
                </i>
                <xsl:text> </xsl:text>
                <xsl:text>Welcome:&#160;</xsl:text>
                <strong>
                  <xsl:text>Guest</xsl:text>
                </strong>
                <!--Sign in / Register&#160;-->
                <xsl:text> </xsl:text>
                <i class="fa fa-chevron-right">
                </i>
              </a>
            </xsl:otherwise>
          </xsl:choose>
        </p>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="/" mode="loginBrief2xs">
    <div id="loginBriefxs">

      <div class="dropdown">
        <a data-toggle="dropdown" href="#">
           <i class="fa fa-user">
            <xsl:text> </xsl:text>
          </i>
        </a>
        <div class="dropdown-menu" role="menu" aria-labelledby="dLabel">
          <span class="membership-inner">
            <h5>
              <xsl:choose>
                <xsl:when test="/Page/User">
                  <xsl:apply-templates select="/" mode="loggedIn" />
                </xsl:when>
                <xsl:otherwise>
                  <a href="/Information/Login-Register" class="">
                    <div class="cartBriefWelcome">

                      <xsl:text>Welcome:&#160;</xsl:text>
                      <strong>
                        <xsl:text>Guest</xsl:text>
                      </strong>
                    </div>
                  </a>
                </xsl:otherwise>
              </xsl:choose>
            </h5>
            <p>
              <xsl:choose>
                <xsl:when test="/Page/User">

                  <a href="/Information/Manage-Account">
                    Manage Account&#160;<i class="fa fa-chevron-right">
                      <xsl:text> </xsl:text>
                    </i>
                  </a>
                  <br />
                  <a href="?ewCmd=logoff">
                    Log Out&#160;<i class="fa fa-chevron-right">
                      <xsl:text> </xsl:text>
                    </i>
                  </a>
                </xsl:when>
                <xsl:otherwise>
                  <a href="/Information/Login-Register" class="">
                    Sign in / Register&#160;
                    <i class="fa fa-chevron-right">
                      <xsl:text> </xsl:text>
                    </i>
                  </a>
                </xsl:otherwise>
              </xsl:choose>
            </p>

          </span>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="/" mode="loginTop">
    <xsl:param name="apply-login-action" />
    <div id="loginTop">
      <xsl:if test="$apply-login-action='slide'">
        <xsl:attribute name="class">
          <xsl:text>login-slide</xsl:text>
        </xsl:attribute>
      </xsl:if>

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
    <div id="loginBriefxs">
      <xsl:if test="$apply-login-action='slide'">
        <xsl:attribute name="class">
          <xsl:text>login-slide</xsl:text>
        </xsl:attribute>
      </xsl:if>
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
    <form method="post" action="" id="UserLogon" name="UserLogon" onsubmit="form_check(this)" xmlns="http://www.w3.org/1999/xhtml">
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
        <input type="text" name="cUserName" id="cUserName" class="textbox required" value="" onfocus="if (this.value=='Please enter Email') {this.value=''}"/>
      </div>
      <div class="password">
        <label for="cPassword">Password: </label>
        <input type="password" name="cPassword" id="cPassword" class="textbox password required"/>
      </div>
      <div class="submit">
        <button type="ewSubmit" name="ewSubmit" value="Login" class="loginButton button btn btn-default principle">Login</button>
      </div>
    </form>

  </xsl:template>


  <!-- ############################################ SEARCH ############################################### -->
  <xsl:template match="/" mode="searchBrief">
    <div class="searchBrief not-xs">
      <form method="post" action="/information/search" id="searchInputxs" class="ewXform">
        <input type="hidden" name="searchMode" value="REGEX" />
        <input type="hidden" name="contentType" value="Product" />
        <input type="hidden" name="searchFormId" value="8923" />
        <input type="text" class="CTAsearch" name="searchString" id="searchStringxs" value="" />
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
    <div class="dropdown">
      <a data-toggle="dropdown" href="#" class="search-xs-btn">
        <i class="fa fa-search fa-2x">
          <xsl:text> </xsl:text>
        </i>
        <span class="sr-only">Expand search section</span>
      </a>
      <div class="dropdown-menu" role="menu" aria-labelledby="dLabel">
        <span>
          <div class="topSearch">
            <form method="post" action="/information/search" id="searchInputxs" class="ewXform">
              <input type="hidden" name="searchMode" value="REGEX" />
              <input type="hidden" name="contentType" value="Product" />
              <input type="hidden" name="searchFormId" value="8923" />
              <input type="text" class="CTAsearch" name="searchString" id="searchStringxs" value="" />
              <button type="submit" class="btn btn-sm btn-primary CTAsearch_button" name="Search" value="Submit">
                <i class="fa fa-search">
                  <xsl:text> </xsl:text>
                </i>
                <span class="sr-only">Search</span>
              </button>
            </form>
          </div>
        </span>
      </div>
    </div>
  </xsl:template>

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

  <xsl:template match="MenuItem" mode="submenu_topnav">
    <xsl:param name="apply-sub-nav-collapse" />
    <ul>
      <xsl:attribute name="class">
        <xsl:text>nav nav-pills</xsl:text>
      </xsl:attribute>
      <xsl:if test="not(contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 8') or contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 7') or contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 6.0')) and $apply-sub-nav-collapse='true'">
        <xsl:attribute name="class">
          nav nav-pills nav-add-more-auto
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates select="MenuItem[not(DisplayName/@exclude='true')]" mode="submenuitem_topnav"/>
    </ul>
    <div class="terminus">&#160;</div>
  </xsl:template>

  <xsl:template match="MenuItem[not(DisplayName/@exclude='true')]" mode="submenuitem_topnav">
    <li>
      <xsl:if test="position()=last()">
        <xsl:attribute name="class">
          <xsl:text>last</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="position()=1">
        <xsl:attribute name="class">
          <xsl:text>first</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="self::MenuItem[@id=/Page/@id]">
        <xsl:attribute name="class">
          <xsl:text>active </xsl:text>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="descendant::MenuItem[@id=/Page/@id] and @url!='/'">
        <xsl:attribute name="class">
          <xsl:text>active </xsl:text>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="self::MenuItem[@id=/Page/@id] and position()=1 or descendant::MenuItem[@id=/Page/@id] and @url!='/' and position()=1">
        <xsl:attribute name="class">
          <xsl:text>active first </xsl:text>
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates select="self::MenuItem" mode="menuLink"/>
    </li>

  </xsl:template>

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
                <xsl:if test="$themeLayout='TopNavSideSub' or $themeLayout='SideNav'">
                  <xsl:text> nav-stacked</xsl:text>
                </xsl:if>
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
    <div class="col-md-12">
      <ul class="pager">

        <!-- Back Button-->
        <xsl:choose>
          <xsl:when test="$startPos - $noPerPage='0' and $startPos &gt; ($noPerPage - 1)">
            <xsl:variable name="origURL">
              <xsl:apply-templates select="$currentPage" mode="getHref"/>
            </xsl:variable>
            <li class="previous">
              <a href="{$origURL}" title="click here to view the previous page in sequence" class="btn btn-sm btn-default">
                <xsl:apply-templates select="/" mode="pager-arrow-left"/>
              </a>
            </li>
          </xsl:when>
          <xsl:when test="$startPos &gt; ($noPerPage - 1)">
            <li class="previous">
              <a href="{$thisURL}={$startPos - $noPerPage}" title="click here to view the previous page in sequence" class="btn btn-sm btn-default">
                <xsl:apply-templates select="/" mode="pager-arrow-left"/>
              </a>
            </li>
          </xsl:when>
          <xsl:otherwise>
            <li class="previous disabled">
              <a href="#" class="btn btn-sm btn-default">
                <xsl:apply-templates select="/" mode="pager-arrow-left"/>
              </a>
            </li>
          </xsl:otherwise>
        </xsl:choose>
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

        <xsl:choose>
          <xsl:when test="$totalCount &gt; ($startPos +$noPerPage)">
            <li class="next">
              <a href="{$thisURL}={$startPos+$noPerPage}" title="click here to view the next page in sequence" class="btn btn-sm btn-default">
                <xsl:apply-templates select="/" mode="pager-arrow-right"/>
              </a>
            </li>
          </xsl:when>
          <xsl:otherwise>
            <li class="next disabled">
              <a href="#" class="btn btn-sm btn-default">
                <xsl:apply-templates select="/" mode="pager-arrow-right"/>
              </a>
            </li>
          </xsl:otherwise>
        </xsl:choose>

        <!-- ### to ### of ### (At the top) -->

      </ul>
    </div>
  </xsl:template>

  <!-- -->
  <xsl:template match="/" mode="pager-arrow-left">
    <i class="fa fa-chevron-left">&#160;</i>&#160;<xsl:call-template name="term2099"/>
  </xsl:template>
  <xsl:template match="/" mode="pager-arrow-right">
    <xsl:call-template name="term2100"/>&#160;<i class="fa fa-chevron-right">&#160;</i>
  </xsl:template>

</xsl:stylesheet>

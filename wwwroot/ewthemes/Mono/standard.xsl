<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">

  <!-- ######################################## IMPORT ALL COMMON XSL's ########################################### -->

  <xsl:import href="../../../../../ewcommon_v5/xsl/CommonImports.xsl"/>
  <!--<xsl:import href="../../xsl/InstalledModules.xsl"/>-->

  <!-- ############################################## OUTPUT TYPE ################################################# -->

  <xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml11.dtd" encoding="UTF-8"/>
  <xsl:template match="/">
    <xsl:apply-imports/>
  </xsl:template>

  <!-- ############################################ THEME WIDTHS ############################################### -->
  
  <xsl:variable name="fullwidth" select="'960'"/>
  <xsl:variable name="navwidth" select="'170'"/>
  <xsl:variable name="boxpad" select="'15'"/>
  <xsl:variable name="colpad" select="'20'"/>

  <!-- ############################################ THEME VARIABLES ############################################### -->

  <xsl:variable name="theme">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'theme'"/>
      <xsl:with-param name="valueName" select="'CurrentTheme'"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="themeLayout">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'theme'"/>
      <xsl:with-param name="valueName" select="concat($theme,'.Layout')"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="themeColor">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'theme'"/>
      <xsl:with-param name="valueName" select="concat($theme,'.Color')"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="BaseColor">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'theme'"/>
      <xsl:with-param name="valueName" select="concat($theme,'.BaseColor')"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="PrimaryColor">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'theme'"/>
      <xsl:with-param name="valueName" select="concat($theme,'.PrimaryColor')"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="SecondaryColor">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'theme'"/>
      <xsl:with-param name="valueName" select="concat($theme,'.SecondaryColor')"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="TertiaryColor">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'theme'"/>
      <xsl:with-param name="valueName" select="concat($theme,'.TertiaryColor')"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="QuartaryColor">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'theme'"/>
      <xsl:with-param name="valueName" select="concat($theme,'.QuartaryColor')"/>
    </xsl:call-template>
  </xsl:variable>


  <!-- FOR DYNAMIC COLOUR CONFIGURATION -->
  <xsl:variable name="themeColors">
    <xsl:text>?c=</xsl:text>
    <xsl:value-of select="translate($BaseColor,'&#35;','')"/>
    <xsl:text>&amp;c1=</xsl:text>
    <xsl:value-of select="translate($PrimaryColor,'&#35;','')"/>
    <xsl:text>&amp;c2=</xsl:text>
    <xsl:value-of select="translate($SecondaryColor,'&#35;','')"/>
    <xsl:text>&amp;c3=</xsl:text>
    <xsl:value-of select="translate($TertiaryColor,'&#35;','')"/>
    <xsl:text>&amp;c4=</xsl:text>
    <xsl:value-of select="translate($QuartaryColor,'&#35;','')"/>
  </xsl:variable>

  <!-- ########################################## CORE TEMPLATES VARIABLES ######################################## -->

  <xsl:variable name="currentPage" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]"/>
  <xsl:variable name="sectionPage" select="/Page/Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]"/>
  <xsl:variable name="siteURL">
    <xsl:call-template name="getSiteURL"/>
  </xsl:variable>

  <!-- ############################################### THEME CSS's ################################################ -->

  <xsl:template match="Page" mode="siteStyle">
    <link rel="stylesheet" type="text/css" href="/ewcommon/css/layout/dynamiclayout.css.aspx?fullwidth={$fullwidth}&amp;colPad={$colpad}&amp;boxPad={$boxpad}&amp;NavWidth={$navwidth}" />
    <link rel="stylesheet" type="text/css" href="/ewthemes/{$theme}/css/Layout/dynamiclayout.css.aspx?fullwidth={$fullwidth}&amp;colPad={$colpad}&amp;boxPad={$boxpad}&amp;NavWidth={$navwidth}"/>
    <link rel="stylesheet" type="text/css" href="/ewthemes/{$theme}/css/Layout/{$theme}.css"/>
    <link rel="stylesheet" type="text/css" href="/ewthemes/{$theme}/css/Layout/{$themeLayout}.css"/>
    <link rel="stylesheet" type="text/css" href="/ewthemes/{$theme}/css/Color/DynamicColour.css.aspx{$themeColors}"/>
  </xsl:template>

  <!-- ############################################### THEME JS's ################################################ -->
  
  <xsl:template match="Page" mode="siteJs">
    <script src="/ewThemes/{$theme}/js/{$theme}.js" type="text/javascript"></script>
  </xsl:template>
  
  <!-- ############################################ BOX STYLES ############################################### -->

  <!--<xsl:template match="/" mode="siteBoxStyles">
    <xsl:param name="value" />
    <option value="Accent_1_Box">
      <xsl:if test="$value='Accent_1_Box'">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:text>Accent 1 Box</xsl:text>
    </option>
    <option value="Accent_2_Box">
      <xsl:if test="$value='Accent_2_Box'">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:text>Accent 2 Box</xsl:text>
    </option>
    <option value="Accent_3_Box">
      <xsl:if test="$value='Accent_3_Box'">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:text>Accent 3 Box</xsl:text>
    </option>
  </xsl:template>-->

  <!-- ############################################ IMAGE SIZES ############################################### -->
  
  <xsl:template match="Content | MenuItem" mode="getThWidth">100</xsl:template>
  <xsl:template match="Content | MenuItem" mode="getThHeight">100</xsl:template>
  
  <!-- ############################################ PAGE LAYOUT ############################################### -->
  
  <xsl:template match="Page" mode="bodyDisplay">
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
    <div id="mainTable" class="Site">
      <div id="watermark">
        <div class="wrapper">
          <div id="mainHeader">
            <div id="logo">
              <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
                <xsl:with-param name="type">Image</xsl:with-param>
                <xsl:with-param name="text">Add Logo</xsl:with-param>
                <xsl:with-param name="name">Logo</xsl:with-param>
              </xsl:apply-templates>
              <xsl:apply-templates select="/Page/Contents/Content[@name='Logo']" mode="displayBrief"/>
            </div>
            <xsl:if test="Menu/MenuItem/MenuItem[@name='Information']/MenuItem">
              <div id="topNav">
                <ul>
                  <li>
                    <xsl:apply-templates select="/Page/Menu/MenuItem" mode="menuLink"/>
                  </li>
                  <xsl:for-each select="Menu/MenuItem/MenuItem[@name='Information']/MenuItem">
                    <li>
                      <xsl:if test="position()=last()">
                        <xsl:attribute name="class">last</xsl:attribute>
                      </xsl:if>
                      <xsl:apply-templates select="." mode="menuLink"/>
                    </li>
                  </xsl:for-each>
                </ul>
              </div>
            </xsl:if>
            <div id="mainHeaderText">
              <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
                <xsl:with-param name="type">FormattedText</xsl:with-param>
                <xsl:with-param name="text">Add Strapline</xsl:with-param>
                <xsl:with-param name="name">mainHeaderStrapline</xsl:with-param>
              </xsl:apply-templates>
              <xsl:apply-templates select="Contents/Content[@name='mainHeaderStrapline']" mode="displayBrief"/>
            </div>
            <div class="logincartContainer">
              <xsl:if test="$cart='on'">
                <xsl:apply-templates select="/" mode="cartBrief" />
              </xsl:if>
              <xsl:if test="$membership='on'">
                <xsl:apply-templates select="/" mode="loginBrief" />
              </xsl:if>
            </div>
            <div class="terminus">&#160;</div>
          </div>
          <div id="mainMenuContainer">
            <xsl:if test="$themeLayout!='SideNav'">
              <xsl:if test="Menu/MenuItem/MenuItem[@name!='Information']">
                <div id="mainMenu">
                  <ul>
                    <xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Information']" mode="mainmenu"/>
                  </ul>
                  <div class="terminus">&#160;</div>
                </div>
              </xsl:if>
            </xsl:if>
            <xsl:if test="$themeLayout='TopNav'">
              <xsl:if test="count(Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/MenuItem) &gt; 0">
                <div id="mainMenu1">
                  <xsl:apply-templates select="Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="submenu_topnav"/>
                  <div class="terminus">&#160;</div>
                </div>
              </xsl:if>
              <xsl:if test="count(Menu/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/MenuItem) &gt; 0 and Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/@name!='Our Clients'">
                <div id="mainMenu2">
                  <xsl:apply-templates select="Menu/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="submenu_topnav"/>
                </div>
              </xsl:if>
              <xsl:if test="count(Menu/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/MenuItem) &gt; 0 and Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/@name!='Our Clients'">
                <div id="mainMenu3">
                  <xsl:apply-templates select="Menu/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="submenu_topnav"/>
                </div>
              </xsl:if>
              <xsl:if test="count(Menu/MenuItem/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/MenuItem) &gt; 0 and Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/@name!='Our Clients'">
                <div id="mainMenu4">
                  <xsl:apply-templates select="Menu/MenuItem/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="submenu_topnav"/>
                </div>
              </xsl:if>
              <xsl:if test="count(Menu/MenuItem/MenuItem/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/MenuItem) &gt; 0 and Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/@name!='Our Clients'">
                <div id="mainMenu5">
                  <xsl:apply-templates select="Menu/MenuItem/MenuItem/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="submenu_topnav"/>
                </div>
              </xsl:if>
            </xsl:if>
          </div>
          <div id="location">
            <xsl:apply-templates select="Menu/MenuItem" mode="breadcrumb"/>
          </div>

          <!--<div id="mainHeaderAlt">
            <xsl:if test="Contents/Content[@name='Banner Image'] or /Page/@adminMode">
              <xsl:attribute name="style">
                <xsl:text>height:</xsl:text>
                <xsl:value-of select="Contents/Content[@name='Banner Image']/img/@height"/>
                <xsl:text>px;</xsl:text>
                <xsl:text>background:url(</xsl:text>
                <xsl:value-of select="Contents/Content[@name='Banner Image']/img/@src"/>
                <xsl:text>) left top no-repeat;</xsl:text>
              </xsl:attribute>


              <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
                <xsl:with-param name="type">Image</xsl:with-param>
                <xsl:with-param name="text">Add Banner Image</xsl:with-param>
                <xsl:with-param name="name">Banner Image</xsl:with-param>
              </xsl:apply-templates>

              <div id="imageText">
                <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
                  <xsl:with-param name="type">FormattedText</xsl:with-param>
                  <xsl:with-param name="text">Add Image Text</xsl:with-param>
                  <xsl:with-param name="name">imageText</xsl:with-param>
                </xsl:apply-templates>
                <xsl:apply-templates select="Contents/Content[@name='imageText' and (@type='FormattedText')]" mode="displayBrief"/>
              </div>
              --><!--<xsl:apply-templates select="Contents/Content[@name='Banner Image']" mode="displayBrief"/>--><!--
            </xsl:if>
          </div>-->
          <div id="mainLayoutContainer">
            <!--<xsl:copy-of select="$currentPage/MenuItem/@name"/>-->
            <xsl:if test="(count($sectionPage/MenuItem)&gt;0 and $currentPage/@name!='Home' and $themeLayout='TopNavSideSub') or $themeLayout='SideNav'">
              <div id="leftCol">
                <div id="subMenu">
                  <xsl:choose>
                    <xsl:when test="$themeLayout='TopNavSideSub'">
                      <xsl:apply-templates select="$sectionPage" mode="submenu"/>
                    </xsl:when>
                    <xsl:when test="$themeLayout='SideNav'">
                      <xsl:apply-templates select="Menu" mode="mainmenu_sidenavtree"/>
                    </xsl:when>
                  </xsl:choose>
                </div>
                <div class="terminus">&#160;</div>
                <xsl:if test="/Page/AdminMenu">
                  <div>
                    <xsl:apply-templates select="/Page" mode="addModule">
                      <xsl:with-param name="text">Add Module</xsl:with-param>
                      <xsl:with-param name="position">LeftNav</xsl:with-param>
                    </xsl:apply-templates>
                  </div>
                </xsl:if>
              </div>
            </xsl:if>
            <div id="mainLayout">
              <xsl:if test="((count($sectionPage/MenuItem)=0 or $currentPage/@name='Home') and $themeLayout!='SideNav') or $themeLayout='TopNav'">
                <xsl:attribute name="class">fullwidth</xsl:attribute>
              </xsl:if>

              <!--<div id="mainTitle">
                <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
                  <xsl:with-param name="type">PlainText</xsl:with-param>
                  <xsl:with-param name="text">Add Page Title</xsl:with-param>
                  <xsl:with-param name="name">title</xsl:with-param>
                </xsl:apply-templates>
                <xsl:apply-templates select="/" mode="getMainTitle" />
              </div>-->

              <xsl:apply-templates select="." mode="mainLayout"/>
            </div>
            <div class="terminus">&#160;</div>
          </div>
          <div class="terminus">&#160;</div>
          <div id="mainFooter" class="fullwidth">
            <div id="copyright">
              <xsl:apply-templates select="Contents/Content[@name='Copyright' and (@type='Image' or @type='FormattedText' or @type='PlainText')]" mode="displayBrief"/>
            </div>
            <xsl:apply-templates select="/" mode="developerLink"/>
            <div class="terminus">&#160;</div>
          </div>
          <div class="terminus">&#160;</div>
        </div>
      </div>
    </div>
  </xsl:template>
  <!-- -->
  <xsl:template match="Menu/MenuItem" mode="mainmenu_sidenavtree">
    <ul>
      <li>
        <xsl:attribute name="class">
          <xsl:text>first</xsl:text>
        </xsl:attribute>
        <xsl:apply-templates select="self::MenuItem" mode="menuLink"/>
      </li>
      <xsl:apply-templates select="MenuItem[@name!='Additional Information']" mode="submenuitem"/>
    </ul>
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

  <!-- ########################################## CART AND MEMBERSHIP TEMPLATES ############################################ -->

  <xsl:template match="/" mode="loginBrief">
    <div id="loginBrief">
      <div id="loggedIn">
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
      </div>
      <div class="cartDividerDots">&#160;</div>
      <div id="signin">
        <p class="loginText">
          <xsl:choose>
            <xsl:when test="/Page/User">
              <a href="/Information/Manage-Account">Manage Account</a>
              <xsl:text>&#160;|&#160;</xsl:text>
              <a href="?ewCmd=logoff">Log Out</a>
            </xsl:when>
            <xsl:otherwise>
              <a href="/Information/Login-Register">Sign in / Register</a>
            </xsl:otherwise>
          </xsl:choose>
        </p>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="/" mode="cartBrief">
    <div id="cartBrief">
      <div class="cartinfo">
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
              <sup id="itemCountLabel">
                <xsl:text>items</xsl:text>
              </sup>
            </span>
            <span id="itemTotal">
              <xsl:value-of select="/Page/Cart/@currencySymbol"/>
              <xsl:text>0.00</xsl:text>
            </span>
          </xsl:otherwise>
        </xsl:choose>
        <div class="terminus">&#160;</div>
      </div>
      <div class="terminus">
        <xsl:text>&#160;</xsl:text>
      </div>
      <div class="cartDividerDots">&#160;</div>
      <div class="cartBriefButtons">
        <a class="checkoutButton" href="{$siteURL}{$currentPage/@url}?cartCmd=Cart" title="Click here to checkout" >Checkout</a>
        <a class="basketButton" href="{$siteURL}{$currentPage/@url}?cartCmd=Cart" title="Click here to view full details of your basket" >View Basket</a>
      </div>
    </div>
  </xsl:template>

  <!-- ########################################## MEMBERSHIP TEMPLATES ############################################ -->
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
          <a href="/?ewCmd=LogOff" title="Logout" class="button">Logout</a>
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
      <table>
        <tr>
          <td class="username">
            <label for="ewmLogon/username">Username: </label>
            <input type="text" name="cUserName" id="cUserName" class="textbox required" value="" onfocus="if (this.value=='Please enter username') {this.value=''}"/>
          </td>
          <td class="password">
            <label for="ewmLogon/password">Password: </label>
            <input type="password" name="cPassword" id="cPassword" class="textbox password required"/>
          </td>
          <td class="submit">
            <input name="ewmLogon/@ewCmd" class="hidden" value="membershipLogon"/>
            <input type="submit" name="submit" value="Login" class="loginButton button"/>
          </td>
        </tr>
      </table>

      <div class="terminus">&#160;</div>
    </form>
    <xsl:if test="alert">
      <!--<xsl:apply-templates select="alert" mode="xform"/>-->
      <span class="alert">
        <xsl:value-of select="alert"/>
      </span>
    </xsl:if>
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
  
  <!-- ############################################# CART TEMPLATES ############################################### -->
  <!-- -->

  <!-- ############################################# BESPOKE ############################################### -->
  <xsl:template match="*" mode="moreLink">
    <xsl:param name="link"/>
    <xsl:param name="linkText"/>
    <xsl:param name="altText"/>
    <xsl:param name="linkType"/>
    <div class="morelink">
      <span>
        <a href="{$link}" title="{$altText}">
          <xsl:if test="$linkType='external'">
            <xsl:attribute name="rel">external</xsl:attribute>
            <xsl:attribute name="class">extLink</xsl:attribute>
          </xsl:if>
          <xsl:choose>
            <xsl:when test="$linkText!=''">
              <xsl:value-of select="$linkText"/>
            </xsl:when>
            <xsl:otherwise>Read more</xsl:otherwise>
          </xsl:choose>
          <span class="hidden">
            <xsl:text>about </xsl:text>
            <xsl:value-of select="$altText"/>
          </span>
          <span class="gtIcon">
            
          </span>
        </a>
      </span>
    </div>
  </xsl:template>
  
</xsl:stylesheet>
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
	<!--<xsl:variable name="header-layout">header-menu-right</xsl:variable>-->
	<!--menu within header-->
	<xsl:variable name="header-layout">header-one-line</xsl:variable>
		<!-- options are header-menu-right, header-info-above, header-one-line, header-menu-below, -->
	<xsl:variable name="font-import-base">Lato:300,400,700</xsl:variable>
	<xsl:variable name="headings-font-import">Lato:300,400,700</xsl:variable>
	<xsl:variable name="color-mode">default</xsl:variable>
	<xsl:variable name="HomeInfo">false</xsl:variable>
	<xsl:variable name="HomeNav">false</xsl:variable>
	<xsl:variable name="NavFix">true</xsl:variable>
	<xsl:variable name="nav-dropdown">true</xsl:variable>
	<xsl:variable name="sub-nav">false</xsl:variable>
	<xsl:variable name="SideSubWidth">3</xsl:variable>
	<xsl:variable name="SideSubWidthCustom"></xsl:variable>
	<xsl:variable name="themeBreadcrumb">false</xsl:variable>
	<xsl:variable name="themeTitle">true</xsl:variable>
	<xsl:variable name="MatchHeightType" select="''"/>
	<xsl:variable name="thWidth-xs">496</xsl:variable>
	<xsl:variable name="thHeight-xs">496</xsl:variable>
	<xsl:variable name="thWidth">496</xsl:variable>
	<xsl:variable name="thHeight">496</xsl:variable>
	<xsl:variable name="container">container</xsl:variable>
	<xsl:variable name="siteAlert">false</xsl:variable>

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

	<xsl:template match="Page" mode="htmlattr">
		<xsl:if test="$color-mode!='default'">
			<xsl:attribute name="data-bs-theme">
				<xsl:value-of select="$color-mode"/>
			</xsl:attribute>
		</xsl:if>
	</xsl:template>


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
				<xsl:text>~/themes/</xsl:text>
				<xsl:value-of select="$theme"/>
				<xsl:text>/js/jquery.appear.js,</xsl:text>
				<xsl:text>~/themes/</xsl:text>
				<xsl:value-of select="$theme"/>
				<xsl:text>/js/responsive-tabs.js,</xsl:text>
				<xsl:text>~/themes/</xsl:text>
				<xsl:value-of select="$theme"/>
				<xsl:text>/js/smoothproducts.js,</xsl:text>
				<xsl:text>~/themes/</xsl:text>
				<xsl:value-of select="$theme"/>
				<xsl:text>/js/theme-specific.js,</xsl:text>
				<xsl:if test="$cart='on'">
					<xsl:text>/ptn/features/cart/cart.js</xsl:text>
				</xsl:if>
			</xsl:with-param>
			<xsl:with-param name="bundle-path">
				<xsl:text>~/Bundles/site</xsl:text>
			</xsl:with-param>
		</xsl:call-template>

		<xsl:apply-templates select="." mode="siteAdminJs" />
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

	<xsl:template match="Content | MenuItem" mode="getThWidth-xs">
		<xsl:value-of select="$thWidth-xs"/>
	</xsl:template>

	<xsl:template match="Content | MenuItem" mode="getThHeight-xs">
		<xsl:value-of select="$thHeight-xs"/>
	</xsl:template>

	<xsl:template match="Content | MenuItem" mode="getThWidth">
		<xsl:value-of select="$thWidth"/>
	</xsl:template>

	<xsl:template match="Content | MenuItem" mode="getThHeight">
		<xsl:value-of select="$thHeight"/>
	</xsl:template>

	<xsl:template match="Content | MenuItem | Discount | Company" mode="getDisplayWidth">800</xsl:template>
	<xsl:template match="Content | MenuItem | Discount | Company" mode="getDisplayHeight">600</xsl:template>

	<xsl:template match="Item" mode="cartThumbWidth">150</xsl:template>
	<xsl:template match="Item" mode="cartThumbHeight">150</xsl:template>

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
				<a data-bs-toggle="dropdown" href="#">
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
					<span class="visually-hidden">Search</span>
				</button>
			</form>
		</div>
	</xsl:template>

	<xsl:template match="/" mode="searchBriefxs">
		<a href="#" class="search-xs-btn">
			<i class="fa fa-search fa-2x">
				<xsl:text> </xsl:text>
			</i>
			<span class="visually-hidden">Expand search section</span>
		</a>
	</xsl:template>

	<xsl:template match="/" mode="searchSimple">
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
	</xsl:template>

	<xsl:template match="/" mode="searchSimpleXS">
		<form method="post" action="/information/search" id="searchInputxs" class="input-group">
			<label for="searchStringxs" class="visually-hidden">Search</label>
			<input type="text" class="form-control CTAsearch" name="searchString" id="searchStringxs" value="" placeholder="Search" />
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
	</xsl:template>
	<xsl:template match="/" mode="searchModal">
		<button class="btn-clean search-btn" data-bs-toggle="modal" href="#SearchModal" role="button">
			<i class="fa fa-search">
				<xsl:text> </xsl:text>
			</i>
			<span class="visually-hidden">Open Search Modal</span>
		</button>
	</xsl:template>

	<!-- ############################################ LOGIN ############################################### -->


	<!--<xsl:template match="/" mode="loginTop">
    <div id="signin">
      <a class="loginText login-btn" data-bs-toggle="modal" href="#LoginModal" role="button">
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
      <a class="loginText login-small" data-bs-toggle="modal" href="#LoginModal" role="button">
        <xsl:choose>
          <xsl:when test="/Page/User">My Account</xsl:when>
          <xsl:otherwise>Log in</xsl:otherwise>
        </xsl:choose>
      </a>
    </li>
  </xsl:template>-->

	<xsl:template match="/" mode="loginSimple">
		<a class="nav-link login-btn not-xs" data-bs-toggle="modal" href="#LoginModal" role="button">
			<xsl:choose>
				<xsl:when test="/Page/User">My Account</xsl:when>
				<xsl:otherwise>Log in</xsl:otherwise>
			</xsl:choose>
		</a>
	</xsl:template>
	<xsl:template match="/" mode="loginIcon">
		<a class="nav-link login-btn not-xs">
			<xsl:attribute name="href">
				<xsl:choose>
					<xsl:when test="/Page/User">/My-Account</xsl:when>
					<xsl:otherwise>/Login</xsl:otherwise>
				</xsl:choose>
			</xsl:attribute>
			<xsl:attribute name="class">
				<xsl:text>nav-link login-btn not-xs </xsl:text>
				<xsl:if test="/Page/User"> logged-in-icon</xsl:if>
			</xsl:attribute>
			<i class="fas fa-user">
				<xsl:text> </xsl:text>
			</i>
			<span class="visually-hidden">
				<xsl:choose>
					<xsl:when test="/Page/User">My Account</xsl:when>
					<xsl:otherwise>Sign in</xsl:otherwise>
				</xsl:choose>
			</span>
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
					<i class="fa fa-power-off"> </i> Sign out
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

	<!-- ############################################# FOOTER ############################################### -->
	<xsl:template match="Page" mode="footer1">
		<xsl:param name="containerClass" />
		<footer id="pagefooter" class="Site clearfix">
			<div class="footer-inner">
				<div class="clearfix footer-main">
					<div class="{$containerClass}">
						<xsl:if test="Menu/MenuItem/MenuItem[@name='Footer']/MenuItem and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
							<div class="footer-nav-wrapper" role="navigation">
								<ul class="nav footer-nav">
									<xsl:for-each select="Menu/MenuItem/MenuItem[@name='Footer']/MenuItem[not(DisplayName/@exclude='true')]">
										<li class="nav-item">
											<xsl:apply-templates select="." mode="menuLink">
												<xsl:with-param name="class">nav-link</xsl:with-param>
											</xsl:apply-templates>
										</li>
									</xsl:for-each>
								</ul>
							</div>
						</xsl:if>
						<div id="main-footer">
							<xsl:apply-templates select="/Page" mode="addModule">
								<xsl:with-param name="position">main-footer</xsl:with-param>
								<xsl:with-param name="class">footer-main</xsl:with-param>
							</xsl:apply-templates>
						</div>
						<!--<div id="sitefooter">
							<xsl:apply-templates select="/Page" mode="addModule">
								<xsl:with-param name="position">sitefooter</xsl:with-param>
								<xsl:with-param name="class">footer-main</xsl:with-param>
							</xsl:apply-templates>
						</div>-->
					</div>
				</div>
				<div class="clearfix footer-utility">
					<div class="{$containerClass}">
						<div class="clearfix footer-utility-inner">
							<div id="footer-utility">
								<xsl:apply-templates select="/Page" mode="addModule">
									<xsl:with-param name="position">footer-utility</xsl:with-param>
								</xsl:apply-templates>
							</div>
							<!--<div id="copyright">
								<xsl:apply-templates select="/Page" mode="addModule">
									<xsl:with-param name="position">copyright</xsl:with-param>
								</xsl:apply-templates>
							</div>-->
							<!--<div class="credit">
								<xsl:apply-templates select="/" mode="developerLink"/>
							</div>-->
						</div>
					</div>
				</div>
			</div>
			<xsl:if test="$currentPage/@id='1'">
				<div class="dev-credit">
					<div class="container">
						<xsl:apply-templates select="/" mode="developerLink"/>
					</div>
				</div>
			</xsl:if>
		</footer>
	</xsl:template>
	<!-- ############################################# BESPOKE ############################################### -->
	<xsl:template name="developerLink">
		<xsl:variable name="websitecreditURL">
			<xsl:choose>
				<xsl:when test="$page/Settings/add[@key='web.websitecreditURL']/@value!=''">
					<xsl:choose>
						<xsl:when test="starts-with($page/Settings/add[@key='web.websitecreditURL']/@value, 'http')">
							<xsl:value-of select="$page/Settings/add[@key='web.websitecreditURL']/@value"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:text>http://</xsl:text>
							<xsl:value-of select="$page/Settings/add[@key='web.websitecreditURL']/@value"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>https://eonic.com</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="websitecreditText">
			<xsl:choose>
				<xsl:when test="$page/Settings/add[@key='web.websitecreditText']/@value!=''">
					<xsl:value-of select="$page/Settings/add[@key='web.websitecreditText']/@value"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>Built on Protean CMS</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="websitecreditLogo">
			<xsl:if test="$page/Settings/add[@key='web.websitecreditLogo']/@value!=''">
				<xsl:value-of select="$page/Settings/add[@key='web.websitecreditLogo']/@value"/>
			</xsl:if>
		</xsl:variable>
		<div id="developerLink">
			<xsl:if test="$page/Settings/add[@key='web.websitecreditURL']/@value!='' or $page/@id = $page/Menu/MenuItem/@id">
				<a href="{$websitecreditURL}" title="{$websitecreditText}" rel="nofollow external">
					<xsl:if test="$page/Settings/add[@key='web.websitecreditLogo']/@value=''">
						<xsl:attribute name="class">devText</xsl:attribute>
					</xsl:if>
					<!--<xsl:value-of select="$websitecreditText"/>-->
					<span>site by </span>
					<xsl:if test="$websitecreditLogo='' or not($websitecreditLogo)">
						<img src="/ptn/core/images/eonic-digital-white.svg" alt="eonic digital" width="82" height="17"/>
					</xsl:if>
				</a>
				<xsl:if test="$websitecreditLogo!=''">
					<a href="{$websitecreditURL}" title="{$websitecreditText}" rel="nofollow external">
						<xsl:if test="$page/Settings/add[@key='web.websitecreditLogo']/@value=''">
							<xsl:attribute name="class">devLogo</xsl:attribute>
						</xsl:if>
						<img src="{$websitecreditLogo}" alt="{$websitecreditText}"/>

					</a>
				</xsl:if>
			</xsl:if>
		</div>
	</xsl:template>
</xsl:stylesheet>

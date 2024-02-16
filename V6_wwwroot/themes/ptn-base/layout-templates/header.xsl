<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">


	<xsl:template match="Page" mode="header-menu-below">
		<xsl:param name="nav-collapse" />
		<xsl:param name="cart-style" />
		<xsl:param name="social-links" />
		<xsl:param name="containerClass" />
		<header class="navbar navbar-expand-lg header-menu-below">
			<xsl:if test="not($adminMode or /Page[@previewMode='true']) and $NavFix='true'">
				<xsl:attribute name="class">navbar navbar-expand-lg navbar-fixed-top header-menu-below</xsl:attribute>
			</xsl:if>
			<!--LOGO-->
			<div class="{$containerClass} header-inner">
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
				<div class="navbar-content">
					<!--INFO NAV-->
					<xsl:if test="Menu/MenuItem/MenuItem[@name='Info Menu']/MenuItem and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
						<nav class="info-nav" aria-label="Additional Navigation">
							<ul class="navbar-nav not-xs">
								<xsl:if test="$HomeInfo='true'">
									<li class="nav-item">
										<xsl:apply-templates select="Menu/MenuItem" mode="menuLink"/>
									</li>
								</xsl:if>
								<xsl:for-each select="Menu/MenuItem/MenuItem[@name='Info Menu']/MenuItem[not(DisplayName/@exclude='true')]">
									<li class="nav-item">
										<xsl:apply-templates select="." mode="menuLink">
											<xsl:with-param name="class">nav-link</xsl:with-param>
										</xsl:apply-templates>
									</li>
								</xsl:for-each>
								<!--LOGON BUTTON (DESKTOP)-->
								<xsl:if test="$membership='on'">
									<xsl:apply-templates select="/" mode="loginSimple"/>
								</xsl:if>
							</ul>
						</nav>
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

					<!--SOCIAL-->
					<xsl:if test="$social-links='true'">
						<div class="socialLinksHeader not-xs" id="socialLinksHeader">
							<xsl:apply-templates select="/Page" mode="addSingleModule">
								<xsl:with-param name="text">Add Social Links</xsl:with-param>
								<xsl:with-param name="position">socialLinksHeader</xsl:with-param>
								<xsl:with-param name="class">socialLinksHeader not-xs</xsl:with-param>
							</xsl:apply-templates>
						</div>
					</xsl:if>

					<xsl:if test="not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
						<button class="navbar-toggler" type="button" data-bs-toggle="offcanvas" data-bs-target="#navbarSupportedContent" aria-controls="navbarSupportedContent" aria-expanded="false" aria-label="Toggle navigation">
							<span class="navbar-toggler-icon">
								<xsl:text> </xsl:text>
							</span>
						</button>
					</xsl:if>
				</div>
			</div>

			<!--NAV TOGGLE (MOBILE)-->
			<xsl:if test="not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
				<nav class="navbar main-nav" aria-label="Main Navigation">
					<div class="{$containerClass}">
						<div class="offcanvas offcanvas-end" id="navbarSupportedContent">
							<button type="button" class="nav-close-btn text-reset float-end xs-only" data-bs-dismiss="offcanvas" aria-label="Close"></button>
							<!--SEARCH (MOBILE)-->
							<xsl:if test="$search='on' and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
								<div class="xs-only search-wrapper">
									<xsl:apply-templates select="/" mode="searchSimpleXS"/>
								</div>
							</xsl:if>
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
										<xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Info Menu' and @name!='Footer']" mode="mainmenudropdown">
											<xsl:with-param name="overviewLink">true</xsl:with-param>
										</xsl:apply-templates>
									</xsl:when>
									<xsl:when test="$nav-dropdown='hover'">
										<xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Info Menu' and @name!='Footer']" mode="mainmenudropdown">
											<xsl:with-param name="hover">true</xsl:with-param>
										</xsl:apply-templates>
									</xsl:when>
									<xsl:otherwise>
										<xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Info Menu' and @name!='Footer']" mode="mainmenu"/>
									</xsl:otherwise>
								</xsl:choose>
							</ul>
							<!--INFO NAV-->
							<xsl:if test="Menu/MenuItem/MenuItem[@name='Info Menu']/MenuItem and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
								<ul class="navbar-nav info-nav-xs xs-only">
									<xsl:for-each select="Menu/MenuItem/MenuItem[@name='Info Menu']/MenuItem[not(DisplayName/@exclude='true')]">
										<li class="nav-item">
											<xsl:apply-templates select="." mode="menuLink">
												<xsl:with-param name="class">nav-link</xsl:with-param>
											</xsl:apply-templates>
										</li>
									</xsl:for-each>
									<xsl:text> </xsl:text>
								</ul>
							</xsl:if>
							<!--LOGON BUTTON (MOBILE)-->
							<xsl:if test="$membership='on'">
								<div class="xs-only">
									<xsl:apply-templates select="/" mode="loginSimple"/>
								</div>
							</xsl:if>
							<!--SOCIAL (MOBILE)-->
							<xsl:if test="$social-links='true'">
								<div class="socialLinksHeader xs-only" id="socialLinksHeader">
									<xsl:apply-templates select="/Page" mode="addSingleModule">
										<xsl:with-param name="text">Add Social Links</xsl:with-param>
										<xsl:with-param name="position">socialLinksHeader</xsl:with-param>
										<xsl:with-param name="class">socialLinksHeader xs-only</xsl:with-param>
									</xsl:apply-templates>
								</div>
							</xsl:if>
						</div>
					</div>
				</nav>
			</xsl:if>
		</header>
	</xsl:template>

	<!--HEADER MENU RIGHT-->
	<!--bootstrap nav-->
	<xsl:template match="Page" mode="header-menu-right">
		<xsl:param name="nav-collapse" />
		<xsl:param name="cart-style" />
		<xsl:param name="social-links" />
		<xsl:param name="containerClass" />
		<header class="navbar navbar-expand-lg header-menu-right">
			<xsl:if test="not($adminMode or /Page[@previewMode='true']) and $NavFix='true'">
				<xsl:attribute name="class">navbar navbar-expand-lg navbar-fixed-top header-menu-right</xsl:attribute>
			</xsl:if>
			<!--LOGO-->
			<div class="{$containerClass} header-inner">
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
				<div class="navbar-content">
					<!--INFO NAV-->
					<xsl:if test="Menu/MenuItem/MenuItem[@name='Info Menu']/MenuItem and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
						<nav class="info-nav" aria-label="Additional Navigation">
							<ul class="navbar-nav not-xs">
								<xsl:if test="$HomeInfo='true'">
									<li class="nav-item">
										<xsl:apply-templates select="Menu/MenuItem" mode="menuLink"/>
									</li>
								</xsl:if>
								<xsl:for-each select="Menu/MenuItem/MenuItem[@name='Info Menu']/MenuItem[not(DisplayName/@exclude='true')]">
									<li class="nav-item">
										<xsl:apply-templates select="." mode="menuLink">
											<xsl:with-param name="class">nav-link</xsl:with-param>
										</xsl:apply-templates>
									</li>
								</xsl:for-each>
							</ul>
						</nav>
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

					<!--SOCIAL-->
					<xsl:if test="$social-links='true'">
						<div class="socialLinksHeader not-xs" id="socialLinksHeader">
							<xsl:apply-templates select="/Page" mode="addSingleModule">
								<xsl:with-param name="text">Add Social Links</xsl:with-param>
								<xsl:with-param name="position">socialLinksHeader</xsl:with-param>
								<xsl:with-param name="class">socialLinksHeader not-xs</xsl:with-param>
							</xsl:apply-templates>
						</div>
					</xsl:if>
					<!--NAV TOGGLE (MOBILE)-->
					<xsl:if test="not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
						<nav class="navbar main-nav" aria-label="Main Navigation">
							<xsl:if test="not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
								<button class="navbar-toggler" type="button" data-bs-toggle="offcanvas" data-bs-target="#navbarSupportedContent" aria-controls="navbarSupportedContent" aria-expanded="false" aria-label="Toggle navigation">
									<span class="navbar-toggler-icon">
										<xsl:text> </xsl:text>
									</span>
								</button>
							</xsl:if>
							<div class="offcanvas offcanvas-end" id="navbarSupportedContent">
								<button type="button" class="nav-close-btn text-reset float-end xs-only" data-bs-dismiss="offcanvas" aria-label="Close"></button>
								<!--SEARCH (MOBILE)-->
								<xsl:if test="$search='on' and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
									<div class="xs-only search-wrapper">
										<xsl:apply-templates select="/" mode="searchSimpleXS"/>
									</div>
								</xsl:if>
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
											<xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Info Menu' and @name!='Footer']" mode="mainmenudropdown">
												<xsl:with-param name="overviewLink">true</xsl:with-param>
											</xsl:apply-templates>
										</xsl:when>
										<xsl:when test="$nav-dropdown='hover'">
											<xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Info Menu' and @name!='Footer']" mode="mainmenudropdown">
												<xsl:with-param name="hover">true</xsl:with-param>
											</xsl:apply-templates>
										</xsl:when>
										<xsl:otherwise>
											<xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Info Menu' and @name!='Footer']" mode="mainmenu"/>
										</xsl:otherwise>
									</xsl:choose>
								</ul>
								<!--INFO NAV-->
								<xsl:if test="Menu/MenuItem/MenuItem[@name='Info Menu']/MenuItem and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
									<ul class="navbar-nav info-nav-xs xs-only">
										<xsl:for-each select="Menu/MenuItem/MenuItem[@name='Info Menu']/MenuItem[not(DisplayName/@exclude='true')]">
											<li class="nav-item">
												<xsl:apply-templates select="." mode="menuLink">
													<xsl:with-param name="class">nav-link</xsl:with-param>
												</xsl:apply-templates>
											</li>
										</xsl:for-each>
										<xsl:text> </xsl:text>
									</ul>
								</xsl:if>
								<!--LOGON BUTTON (MOBILE)-->
								<xsl:if test="$membership='on'">
									<div class="xs-only">
										<xsl:apply-templates select="/" mode="loginSimple"/>
									</div>
								</xsl:if>
								<!--SOCIAL (MOBILE)-->
								<xsl:if test="$social-links='true'">
									<div class="socialLinksHeader xs-only" id="socialLinksHeader">
										<xsl:apply-templates select="/Page" mode="addSingleModule">
											<xsl:with-param name="text">Add Social Links</xsl:with-param>
											<xsl:with-param name="position">socialLinksHeader</xsl:with-param>
											<xsl:with-param name="class">socialLinksHeader xs-only</xsl:with-param>
										</xsl:apply-templates>
									</div>
								</xsl:if>
							</div>
						</nav>
					</xsl:if>
				</div>
			</div>
		</header>
	</xsl:template>

	<!--HEADER INFO ABOVE-->
	<!--bootstrap nav-->
	<xsl:template match="Page" mode="header-info-above">
		<xsl:param name="nav-collapse" />
		<xsl:param name="cart-style" />
		<xsl:param name="social-links" />
		<xsl:param name="containerClass" />
		<xsl:param name="cartClass" />
		<header class="navbar navbar-expand-xl header-info-above {$cartClass}">
			<xsl:if test="not($adminMode or /Page[@previewMode='true']) and $NavFix='true'">
				<xsl:attribute name="class">navbar navbar-expand-lg navbar-fixed-top header-info-above <xsl:value-of select="$cartClass"/></xsl:attribute>
			</xsl:if>
			<div class="header-above">
				<div class="{$containerClass}">
					<!--INFO NAV-->
					<xsl:if test="Menu/MenuItem/MenuItem[@name='Info Menu']/MenuItem and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
						<nav class="info-nav" aria-label="Additional Navigation">
							<ul class="navbar-nav not-xs">
								<xsl:if test="$HomeInfo='true'">
									<li class="nav-item">
										<xsl:apply-templates select="Menu/MenuItem" mode="menuLink"/>
									</li>
								</xsl:if>
								<xsl:for-each select="Menu/MenuItem/MenuItem[@name='Info Menu']/MenuItem[not(DisplayName/@exclude='true')]">
									<li class="nav-item">
										<xsl:apply-templates select="." mode="menuLink">
											<xsl:with-param name="class">nav-link</xsl:with-param>
										</xsl:apply-templates>
									</li>
								</xsl:for-each>
								<li class="nav-item">
									<!--LOGON BUTTON (DESKTOP)-->
									<xsl:if test="$membership='on'">
										<xsl:apply-templates select="/" mode="loginSimple"/>
									</xsl:if>
								</li>
							</ul>
						</nav>
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

					<!--SOCIAL-->
					<xsl:if test="$social-links='true'">
						<div class="socialLinksHeader not-xs" id="socialLinksHeader">
							<xsl:apply-templates select="/Page" mode="addSingleModule">
								<xsl:with-param name="text">Add Social Links</xsl:with-param>
								<xsl:with-param name="position">socialLinksHeader</xsl:with-param>
								<xsl:with-param name="class">socialLinksHeader not-xs</xsl:with-param>
							</xsl:apply-templates>
						</div>
					</xsl:if>
				</div>
			</div>
			<!--LOGO-->
			<div class="{$containerClass} header-inner">
				<div class="navbar-brand">
					<xsl:apply-templates select="/Page" mode="inlinePopupSingle">
						<xsl:with-param name="type">Image</xsl:with-param>
						<xsl:with-param name="text">Add Logo</xsl:with-param>
						<xsl:with-param name="name">Logo</xsl:with-param>
						<xsl:with-param name="class">navbar-brand</xsl:with-param>
					</xsl:apply-templates>
					<xsl:apply-templates select="/Page/Contents/Content[@name='Logo']" mode="displayBrief">
						<xsl:with-param name="maxWidth" select="'200'"/>
						<xsl:with-param name="maxHeight" select="'200'"/>
					</xsl:apply-templates>
				</div>
				<div class="navbar-content">

					<!--NAV TOGGLE (MOBILE)-->
					<xsl:if test="not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
						<nav class="navbar main-nav" aria-label="Main Navigation">
							<xsl:if test="not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
								<button class="navbar-toggler" type="button" data-bs-toggle="offcanvas" data-bs-target="#navbarSupportedContent" aria-controls="navbarSupportedContent" aria-expanded="false" aria-label="Toggle navigation">
									<span class="navbar-toggler-icon"><xsl:text> </xsl:text></span>
								</button>
							</xsl:if>
							<div class="offcanvas offcanvas-end" id="navbarSupportedContent">
								<button type="button" class="nav-close-btn text-reset float-end xs-only" data-bs-dismiss="offcanvas" aria-label="Close"></button>
								<!--SEARCH (MOBILE)-->
								<xsl:if test="$search='on' and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
									<div class="xs-only search-wrapper">
										<xsl:apply-templates select="/" mode="searchSimpleXS"/>
									</div>
								</xsl:if>
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
											<xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Info Menu' and @name!='Footer']" mode="mainmenudropdown">
												<xsl:with-param name="overviewLink">true</xsl:with-param>
											</xsl:apply-templates>
										</xsl:when>
										<xsl:when test="$nav-dropdown='hover'">
											<xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Info Menu' and @name!='Footer']" mode="mainmenudropdown">
												<xsl:with-param name="hover">true</xsl:with-param>
											</xsl:apply-templates>
										</xsl:when>
										<xsl:otherwise>
											<xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Info Menu' and @name!='Footer']" mode="mainmenu"/>
										</xsl:otherwise>
									</xsl:choose>
								</ul>
								<!--INFO NAV-->
								<xsl:if test="Menu/MenuItem/MenuItem[@name='Info Menu']/MenuItem and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
									<ul class="navbar-nav info-nav-xs xs-only">
										<xsl:for-each select="Menu/MenuItem/MenuItem[@name='Info Menu']/MenuItem[not(DisplayName/@exclude='true')]">
											<li class="nav-item">
												<xsl:apply-templates select="." mode="menuLink">
													<xsl:with-param name="class">nav-link</xsl:with-param>
												</xsl:apply-templates>
											</li>
										</xsl:for-each>
										<xsl:text> </xsl:text>
									</ul>
								</xsl:if>
								<!--LOGON BUTTON (MOBILE)-->
								<xsl:if test="$membership='on'">
									<div class="xs-only">
										<xsl:apply-templates select="/" mode="loginSimple"/>
									</div>
								</xsl:if>
								<!--SOCIAL (MOBILE)-->
								<xsl:if test="$social-links='true'">
									<div class="socialLinksHeader xs-only" id="socialLinksHeader">
										<xsl:apply-templates select="/Page" mode="addSingleModule">
											<xsl:with-param name="text">Add Social Links</xsl:with-param>
											<xsl:with-param name="position">socialLinksHeader</xsl:with-param>
											<xsl:with-param name="class">socialLinksHeader xs-only</xsl:with-param>
										</xsl:apply-templates>
									</div>
								</xsl:if>
							</div>
						</nav>
					</xsl:if>
				</div>
			</div>
		</header>
	</xsl:template>

	<!--HEADER BASIC-->
	<!--bootstrap nav-->
	<xsl:template match="Page" mode="header-basic1">
		<xsl:param name="nav-collapse" />
		<xsl:param name="cart-style" />
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
						<span class="navbar-toggler-icon">
							<xsl:text> </xsl:text>
						</span>
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
								<xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Info Menu' and @name!='Footer']" mode="mainmenudropdown">
									<xsl:with-param name="overviewLink">true</xsl:with-param>
								</xsl:apply-templates>
							</xsl:when>
							<xsl:when test="$nav-dropdown='hover'">
								<xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Info Menu' and @name!='Footer']" mode="mainmenudropdown">
									<xsl:with-param name="hover">true</xsl:with-param>
								</xsl:apply-templates>
							</xsl:when>
							<xsl:otherwise>
								<xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Info Menu' and @name!='Footer']" mode="mainmenu"/>
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

	<!--HEADER ONE LINE-->
	<xsl:template match="Page" mode="header-one-line">
		<xsl:param name="nav-collapse" />
		<xsl:param name="cart-style" />
		<xsl:param name="social-links" />
		<xsl:param name="containerClass" />
		<xsl:param name="cartClass" />
		<header class="navbar navbar-expand-lg header-one-line {$cartClass}">
			<xsl:if test="not($adminMode or /Page[@previewMode='true']) and $NavFix='true'">
				<xsl:attribute name="class">
					navbar navbar-expand-lg navbar-fixed-top header-one-line <xsl:value-of select="$cartClass"/>
				</xsl:attribute>
			</xsl:if>
			<div class="{$containerClass} header-inner">
				<div class="navbar-brand">
					<xsl:apply-templates select="/Page" mode="inlinePopupSingle">
						<xsl:with-param name="type">Image</xsl:with-param>
						<xsl:with-param name="text">Add Logo</xsl:with-param>
						<xsl:with-param name="name">Logo</xsl:with-param>
						<xsl:with-param name="class">navbar-brand</xsl:with-param>
					</xsl:apply-templates>
					<xsl:apply-templates select="/Page/Contents/Content[@name='Logo']" mode="displayBrief">
						<xsl:with-param name="maxWidth" select="'200'"/>
						<xsl:with-param name="maxHeight" select="'200'"/>
					</xsl:apply-templates>
				</div>
				<div class="navbar-content">

					<!--NAV TOGGLE (MOBILE)-->
					<xsl:if test="not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
						<nav class="navbar main-nav" aria-label="Main Navigation">

							<div class="offcanvas offcanvas-end" id="navbarSupportedContent">
								<button type="button" class="nav-close-btn text-reset float-end xs-only" data-bs-dismiss="offcanvas" aria-label="Close">
									<i class="fa fa-times">
										<xsl:text> </xsl:text>
									</i>
								</button>
								<!--SEARCH (MOBILE)-->
								<xsl:if test="$search='on' and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
									<div class="xs-only search-wrapper">
										<xsl:apply-templates select="/" mode="searchSimpleXS"/>
									</div>
								</xsl:if>
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
											<xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Info Menu' and @name!='Footer']" mode="mainmenudropdown">
												<xsl:with-param name="overviewLink">self</xsl:with-param>
												<xsl:with-param name="level2">true</xsl:with-param>
											</xsl:apply-templates>
										</xsl:when>
										<xsl:when test="$nav-dropdown='hover'">
											<xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Info Menu' and @name!='Footer']" mode="mainmenudropdown">
												<xsl:with-param name="hover">true</xsl:with-param>
											</xsl:apply-templates>
										</xsl:when>
										<xsl:otherwise>
											<xsl:apply-templates select="Menu/MenuItem/MenuItem[@name!='Info Menu' and @name!='Footer']" mode="mainmenu"/>
										</xsl:otherwise>
									</xsl:choose>
									<li class="nav-item">
										<a class="nav-link xs-only">
											<xsl:attribute name="href">
												<xsl:choose>
													<xsl:when test="/Page/User">/My-Account</xsl:when>
													<xsl:otherwise>/Login</xsl:otherwise>
												</xsl:choose>
											</xsl:attribute>
											<xsl:choose>
												<xsl:when test="/Page/User">My Account</xsl:when>
												<xsl:otherwise>Log in</xsl:otherwise>
											</xsl:choose>
										</a>
									</li>
								</ul>
								<!--INFO NAV-->
								<xsl:if test="Menu/MenuItem/MenuItem[@name='Info Menu']/MenuItem and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
									<ul class="navbar-nav info-nav-xs xs-only">
										<xsl:for-each select="Menu/MenuItem/MenuItem[@name='Info Menu']/MenuItem[not(DisplayName/@exclude='true')]">
											<li class="nav-item">
												<xsl:apply-templates select="." mode="menuLink">
													<xsl:with-param name="class">nav-link</xsl:with-param>
												</xsl:apply-templates>
											</li>
										</xsl:for-each>
										<xsl:text> </xsl:text>
									</ul>
								</xsl:if>
								<!--LOGON BUTTON (MOBILE)-->
								<xsl:if test="$membership='on'">
									<div class="xs-only">
										<xsl:apply-templates select="/" mode="loginSimple"/>
									</div>
								</xsl:if>
								<!--SOCIAL (MOBILE)-->
								<xsl:if test="$social-links='true'">
									<div class="socialLinksHeader xs-only" id="socialLinksHeader">
										<xsl:apply-templates select="/Page" mode="addSingleModule">
											<xsl:with-param name="text">Add Social Links</xsl:with-param>
											<xsl:with-param name="position">socialLinksHeader</xsl:with-param>
											<xsl:with-param name="class">socialLinksHeader xs-only</xsl:with-param>
										</xsl:apply-templates>
									</div>
								</xsl:if>
							</div>
						</nav>
					</xsl:if>
					<!--SEARCH (DESKTOP)-->
					<xsl:if test="$search='on' and not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
						<div class="not-xs ">
							<xsl:apply-templates select="/" mode="searchModal"/>
						</div>
					</xsl:if>
					<!--LOGON BUTTON (DESKTOP)-->
					<xsl:if test="$membership='on' and not($currentPage/DisplayName[@nonav='true'])">
						<xsl:apply-templates select="/" mode="loginIcon"/>
					</xsl:if>
					<!--CART-->
					<xsl:if test="$cart='on' and not($cartPage) and not(Cart/Order/@status='Empty')">
						<xsl:apply-templates select="/" mode="cartSimple"/>
					</xsl:if>
				</div>
				<div class="header-featured-btn">
					<a href="/Everything-DISC/Products">Buy Now</a>
				</div>
				<xsl:if test="not($currentPage/DisplayName[@nonav='true']) and not($cartPage)">
					<button class="navbar-toggler" type="button" data-bs-toggle="offcanvas" data-bs-target="#navbarSupportedContent" aria-controls="navbarSupportedContent" aria-expanded="false" aria-label="Toggle navigation">
						<span class="navbar-toggler-icon">
							<xsl:text> </xsl:text>
						</span>
					</button>
				</xsl:if>
			</div>
		</header>
	</xsl:template>
</xsl:stylesheet>

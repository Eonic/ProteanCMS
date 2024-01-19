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
						<xsl:with-param name="containerClass" select="$container"/>
					</xsl:apply-templates>
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates select="." mode="header-menu-below">
						<xsl:with-param name="nav-collapse">false</xsl:with-param>
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
			<div class="container-wrapper {$detail-heading} {$nav-padding} {$home-class}">
				<xsl:if test="not($adminMode or /Page[@previewMode='true']) and $NavFix='true'">
					<xsl:attribute name="class">
						<xsl:text>container-wrapper fixed-nav-content </xsl:text>
						<xsl:value-of select="$nav-padding"/>
						<xsl:text> </xsl:text>
						<xsl:value-of select="$detail-heading"/>
						<xsl:text> </xsl:text>
						<xsl:value-of select="$home-class"/>
					</xsl:attribute>
				</xsl:if>
				<div id="mainLayout" class="fullwidth activateAppearAnimation">
					<div id="content" class="visually-hidden">&#160;</div>
					<xsl:apply-templates select="." mode="mainLayout">
						<xsl:with-param name="containerClass" select="$container"/>
					</xsl:apply-templates>
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
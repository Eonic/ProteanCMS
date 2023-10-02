﻿<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
	<xsl:template match="Page" mode="addModuleControls"></xsl:template>
	<xsl:template match="Page" mode="addModuleControlsSection"></xsl:template>

	<xsl:template match="Page" mode="addModule">
		<xsl:param name="text"/>
		<xsl:param name="position"/>
		<xsl:param name="class"/>
		<xsl:param name="width"/>
		<xsl:param name="auto-col"/>
		<xsl:param name="module-type"/>
		<xsl:choose>
			<xsl:when test="$position='header' or $position='footer' or ($position='column1' and @layout='Modules_1_column')">
				<xsl:apply-templates select="." mode="addModuleControlsSection">
					<xsl:with-param name="text" select="$text"/>
					<xsl:with-param name="class" select="$class"/>
					<xsl:with-param name="position" select="$position"/>
				</xsl:apply-templates>
				<xsl:for-each select="/Page/Contents/Content[@type='Module' and @position = $position]">
					<xsl:variable name="backgroundResized">
						<xsl:if test="@backgroundImage!=''">
							<xsl:call-template name="resize-image">
								<xsl:with-param name="path" select="@backgroundImage"/>
								<xsl:with-param name="max-width" select="1920"/>
								<xsl:with-param name="max-height" select="1920"/>
								<xsl:with-param name="file-prefix">
									<xsl:text>~bg-1920</xsl:text>
									<xsl:text>/~bg-</xsl:text>
								</xsl:with-param>
								<xsl:with-param name="file-suffix" select="''"/>
								<xsl:with-param name="quality" select="100"/>
								<xsl:with-param name="crop" select="false()" />
								<xsl:with-param name="no-stretch" select="true()" />
								<xsl:with-param name="forceResize" select="false()" />
							</xsl:call-template>
						</xsl:if>
					</xsl:variable>
					<xsl:variable name="backgroundResized-xs">
						<xsl:if test="@backgroundImage!=''">
							<xsl:call-template name="resize-image">
								<xsl:with-param name="path" select="@backgroundImage"/>
								<xsl:with-param name="max-width" select="575"/>
								<xsl:with-param name="max-height" select="575"/>
								<xsl:with-param name="file-prefix">
									<xsl:text>~bg-575</xsl:text>
									<xsl:text>/~bg-</xsl:text>
								</xsl:with-param>
								<xsl:with-param name="file-suffix" select="''"/>
								<xsl:with-param name="quality" select="100"/>
								<xsl:with-param name="crop" select="false()" />
								<xsl:with-param name="no-stretch" select="true()" />
								<xsl:with-param name="forceResize" select="false()" />
							</xsl:call-template>
						</xsl:if>
					</xsl:variable>
					<xsl:variable name="backgroundResized-sm">
						<xsl:if test="@backgroundImage!=''">
							<xsl:call-template name="resize-image">
								<xsl:with-param name="path" select="@backgroundImage"/>
								<xsl:with-param name="max-width" select="767"/>
								<xsl:with-param name="max-height" select="767"/>
								<xsl:with-param name="file-prefix">
									<xsl:text>~bg-767</xsl:text>
									<xsl:text>/~bg-</xsl:text>
								</xsl:with-param>
								<xsl:with-param name="file-suffix" select="''"/>
								<xsl:with-param name="quality" select="100"/>
								<xsl:with-param name="crop" select="false()" />
								<xsl:with-param name="no-stretch" select="true()" />
								<xsl:with-param name="forceResize" select="false()" />
							</xsl:call-template>
						</xsl:if>
					</xsl:variable>
					<xsl:variable name="backgroundResized-md">
						<xsl:if test="@backgroundImage!=''">
							<xsl:call-template name="resize-image">
								<xsl:with-param name="path" select="@backgroundImage"/>
								<xsl:with-param name="max-width" select="991"/>
								<xsl:with-param name="max-height" select="991"/>
								<xsl:with-param name="file-prefix">
									<xsl:text>~bg-991</xsl:text>
									<xsl:text>/~bg-</xsl:text>
								</xsl:with-param>
								<xsl:with-param name="file-suffix" select="''"/>
								<xsl:with-param name="quality" select="100"/>
								<xsl:with-param name="crop" select="false()" />
								<xsl:with-param name="no-stretch" select="true()" />
								<xsl:with-param name="forceResize" select="false()" />
							</xsl:call-template>
						</xsl:if>
					</xsl:variable>
					<xsl:variable name="backgroundResized-lg">
						<xsl:if test="@backgroundImage!=''">
							<xsl:call-template name="resize-image">
								<xsl:with-param name="path" select="@backgroundImage"/>
								<xsl:with-param name="max-width" select="1199"/>
								<xsl:with-param name="max-height" select="1199"/>
								<xsl:with-param name="file-prefix">
									<xsl:text>~bg-1199</xsl:text>
									<xsl:text>/~bg-</xsl:text>
								</xsl:with-param>
								<xsl:with-param name="file-suffix" select="''"/>
								<xsl:with-param name="quality" select="100"/>
								<xsl:with-param name="crop" select="false()" />
								<xsl:with-param name="no-stretch" select="true()" />
								<xsl:with-param name="forceResize" select="false()" />
							</xsl:call-template>
						</xsl:if>
					</xsl:variable>
					<xsl:variable name="backgroundResized-webp" select="ew:CreateWebP($backgroundResized)"/>
					<xsl:variable name="backgroundResized-xs-webp" select="ew:CreateWebP($backgroundResized-xs)"/>
					<xsl:variable name="backgroundResized-sm-webp" select="ew:CreateWebP($backgroundResized-sm)"/>
					<xsl:variable name="backgroundResized-md-webp" select="ew:CreateWebP($backgroundResized-md)"/>
					<xsl:variable name="backgroundResized-lg-webp" select="ew:CreateWebP($backgroundResized-lg)"/>
					<xsl:variable name="backgroundResized-xxl-webp" select="ew:CreateWebP(@backgroundImage)"/>
					<section class="wrapper-sm {@background}">
						<xsl:attribute name="class">
							<xsl:text>wrapper-sm </xsl:text>
							<xsl:if test="@background!='false'">
								<xsl:value-of select="@background"/>
								<xsl:text> </xsl:text>
							</xsl:if>
							<xsl:apply-templates select="." mode="hideScreens" />
							<xsl:if test="@marginBelow='false'">
								<xsl:text> mb-0 </xsl:text>
							</xsl:if>
							<xsl:if test="@data-stellar-background-ratio!='10'">
								<xsl:text> parallax-wrapper </xsl:text>
							</xsl:if>
							<xsl:if test="@data-stellar-background-ratio!='10'">
								<xsl:text> parallax-wrapper </xsl:text>
							</xsl:if>
							<xsl:if test="@backgroundVideo-mp4!='' and @backgroundVideo-webm!=''">
								<xsl:text> bg-video-wrapper bg-video-wrapper-</xsl:text>
								<xsl:value-of select="@id"/>
							</xsl:if>
						</xsl:attribute>
						<xsl:if test="@data-stellar-background-ratio!='10'">
							<xsl:attribute name="data-parallax-speed">
								<xsl:if test="@data-stellar-background-ratio&lt;'5'">
									<xsl:text>1.3</xsl:text>
								</xsl:if>
								<xsl:if test="@data-stellar-background-ratio&gt;='5' and @data-stellar-background-ratio&lt;'10'">
									<xsl:text>1.6</xsl:text>
								</xsl:if>
								<xsl:if test="@data-stellar-background-ratio&gt;='10' and @data-stellar-background-ratio&lt;'15'">
									<xsl:text>2</xsl:text>
								</xsl:if>
								<xsl:if test="@data-stellar-background-ratio&gt;='15' and @data-stellar-background-ratio&lt;'20'">
									<xsl:text>3</xsl:text>
								</xsl:if>
								<xsl:if test="@data-stellar-background-ratio&gt;='20' and @data-stellar-background-ratio&lt;'25'">
									<xsl:text>4</xsl:text>
								</xsl:if>
								<xsl:if test="@data-stellar-background-ratio&gt;='25'">
									<xsl:text>5</xsl:text>
								</xsl:if>
							</xsl:attribute>
						</xsl:if>
						<xsl:if test="@data-stellar-background-ratio!='0'">
							<!--<xsl:attribute name="data-stellar-background-ratio">
                <xsl:value-of select="(@data-stellar-background-ratio div 10)"/>
              </xsl:attribute>-->
						</xsl:if>
						<xsl:if test="@backgroundImage!=''">
							<xsl:choose>
								<xsl:when test="@data-stellar-background-ratio!='0'">
									<xsl:choose>
										<xsl:when test="@data-stellar-background-ratio!='10'">
											<xsl:attribute name="style">
												<xsl:if test="@minHeight!=''">
													<xsl:text>min-height:</xsl:text>
													<xsl:value-of select="@minHeight"/>
													<xsl:text>px;</xsl:text>
												</xsl:if>
											</xsl:attribute>
											<div class="parallax"
												 data-parallax-image="{$backgroundResized}"  data-parallax-image-webp="{$backgroundResized-webp}"
												 data-parallax-image-xs="{$backgroundResized-xs}"  data-parallax-image-xs-webp="{$backgroundResized-xs-webp}"
												 data-parallax-image-sm="{$backgroundResized-sm}"  data-parallax-image-sm-webp="{$backgroundResized-sm-webp}"
												 data-parallax-image-md="{$backgroundResized-md}"  data-parallax-image-md-webp="{$backgroundResized-md-webp}"
												 data-parallax-image-lg="{$backgroundResized-lg}"  data-parallax-image-lg-webp="{$backgroundResized-lg-webp}"
												 data-parallax-image-xxl="{@backgroundImage}">
												<xsl:text> </xsl:text>
											</div>
										</xsl:when>
										<xsl:otherwise>
											<xsl:attribute name="style">
												<xsl:text>background-image: url('</xsl:text>
												<xsl:value-of select="@backgroundImage"/>
												<xsl:text>');</xsl:text>
												<xsl:if test="@minHeight!=''">
													<xsl:text>min-height:</xsl:text>
													<xsl:value-of select="@minHeight"/>
													<xsl:text>px;</xsl:text>
												</xsl:if>
											</xsl:attribute>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>
									<xsl:attribute name="style">
										<xsl:text>background-image: url('</xsl:text>
										<xsl:value-of select="@backgroundImage"/>
										<xsl:text>');</xsl:text>
										<xsl:if test="@minHeight!=''">
											<xsl:text>min-height:</xsl:text>
											<xsl:value-of select="@minHeight"/>
											<xsl:text>px;</xsl:text>
										</xsl:if>
									</xsl:attribute>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:if>
						<xsl:if test="@backgroundVideo-mp4!='' and @backgroundVideo-webm!=''">
							<style>
								<xsl:text>.bg-video-wrapper-</xsl:text>
								<xsl:value-of select="@id"/>{
								<xsl:if test="@minHeightxs!=''">
									<xsl:text>min-height:</xsl:text>
									<xsl:value-of select="@minHeightxs"/>
									<xsl:text>px!important;</xsl:text>
									<xsl:text>height:</xsl:text>
									<xsl:value-of select="@minHeightxs"/>
									<xsl:text>px!important;</xsl:text>
								</xsl:if>}
								@media(min-width:768px){
								<xsl:text>.bg-video-wrapper-</xsl:text>

								<xsl:value-of select="@id"/>{
								<xsl:if test="@minHeight!=''">
									<xsl:text>min-height:</xsl:text>
									<xsl:value-of select="@minHeight"/>
									<xsl:text>px!important;</xsl:text>
									<xsl:text>height:</xsl:text>
									<xsl:value-of select="@minHeight"/>
									<xsl:text>px!important;</xsl:text>
								</xsl:if>}}
							</style>
							<!--<xsl:attribute name="style">
					  <xsl:if test="@minHeight!=''">
						  <xsl:text>height:</xsl:text>
						  <xsl:value-of select="@minHeight"/>
						  <xsl:text>px;</xsl:text>
					  </xsl:if>
				  </xsl:attribute>-->
							<video preload="true" playsinline="playsinline" autoplay="autoplay" muted="muted" loop="loop" poster="{@backgroundImage}" id="bgvid-{@id}" class="bg-video">
								<xsl:if test="@backgroundVideo-mp4!=''">
									<source src="{@backgroundVideo-mp4}" type="video/mp4"/>
								</xsl:if>
								<xsl:if test="@backgroundVideo-webm!=''">
									<source src="{@backgroundVideo-webm}" type="video/webm"/>
								</xsl:if>
							</video>
							<script>
								document.getElementById('bgvid-<xsl:value-of select="@id"/>').play();
							</script>
						</xsl:if>
						<xsl:choose>
							<xsl:when test="@fullWidth='true'">
								<div class="fullwidthContainer">
									<xsl:apply-templates select="." mode="displayModule"/>
									<xsl:text> </xsl:text>
								</div>
							</xsl:when>
							<xsl:otherwise>
								<div class="{$class} content">
									<xsl:apply-templates select="." mode="displayModule"/>
									<xsl:text> </xsl:text>
								</div>
							</xsl:otherwise>
						</xsl:choose>
					</section>
				</xsl:for-each>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="." mode="addModuleControls">
					<xsl:with-param name="text" select="$text"/>
					<xsl:with-param name="class" select="$class"/>
					<xsl:with-param name="position" select="$position"/>
				</xsl:apply-templates>
				<xsl:choose>
					<xsl:when test="/Page/Contents/Content[@position = $position]">
						<xsl:apply-templates select="/Page/Contents/Content[@type='Module' and @position = $position]" mode="displayModule">
							<xsl:with-param name="auto-col">
								<xsl:if test="$module-type='AutoColumn'">true</xsl:if>
							</xsl:with-param>
							<xsl:with-param name="width">
								<xsl:value-of select="$width"/>
							</xsl:with-param>
						</xsl:apply-templates>
					</xsl:when>
					<xsl:otherwise>
						<!-- if no contnet, need a space for the compiling of the XSL. -->
						<span class="hidden">
							<xsl:text>&#160;</xsl:text>
						</span>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- ## Layout Types are specified in the LayoutsManifest.XML file  ################################   -->
	<xsl:template match="Page" mode="mainLayout">
		<xsl:param name="containerClass"/>
		<xsl:choose>
			<!-- IF QUOTE CMD SHOW QUOTE -->
			<xsl:when test="Cart[@type='quote']/Quote/@cmd!=''">
				<div class="container">
					<xsl:apply-templates select="Cart[@type='quote']/Quote" mode="cartFull"/>
				</div>
			</xsl:when>
			<!-- IF CART CMD SHOW CART -->
			<xsl:when test="Cart[@type='order']/Order/@cmd!=''">
				<div class="container">
					<xsl:apply-templates select="Cart[@type='order']/Order" mode="cartFull"/>
				</div>
			</xsl:when>
			<!-- IF GIFT LIST CMD SHOW GIFT LIST -->
			<xsl:when test="Cart[@type='giftlist']/Order/@cmd!=''">
				<div class="container">
					<xsl:apply-templates select="Cart[@type='giftlist' and @name='cart']/Order" mode="giftlistDetail"/>
				</div>
			</xsl:when>
			<!-- IF ContentDetail Show ContentDetail -->
			<xsl:when test="ContentDetail">
				<div class="detail-container" role="main">
					<xsl:attribute name="class">
						<xsl:text>detail-container </xsl:text>
						<xsl:value-of select="$page/ContentDetail/Content/@type"/>
						<xsl:text>-detail-container</xsl:text>
					</xsl:attribute>
					<xsl:apply-templates select="ContentDetail" mode="ContentDetail"/>
					<xsl:apply-templates select="ContentDetail/Content" mode="socialBookmarks" />
				</div>
			</xsl:when>
			<xsl:otherwise>
				<!-- Otherwise show page layout -->

				<xsl:apply-templates select="." mode="Layout">
					<xsl:with-param name="containerClass" select="$containerClass"/>
				</xsl:apply-templates>


				<xsl:apply-templates select="." mode="socialBookmarks" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>


	<!-- ## Layout Header & Footer   ###################################################################   -->

	<xsl:template match="Page" mode="layoutHeader">
		<xsl:param name="containerClass"/>
		<xsl:if test="/Page/Contents/Content[@name='header' or @position='header']">
			<xsl:apply-templates select="/Page" mode="addModule">
				<xsl:with-param name="text">Add Module</xsl:with-param>
				<xsl:with-param name="position">header</xsl:with-param>
				<xsl:with-param name="class" select="$containerClass"/>
			</xsl:apply-templates>
		</xsl:if>
	</xsl:template>

	<xsl:template match="Page" mode="layoutFooter">
		<xsl:param name="containerClass"/>
		<xsl:if test="/Page/Contents/Content[@name='footer' or @position='footer']">
			<xsl:apply-templates select="/Page" mode="addModule">
				<xsl:with-param name="text">Add Module</xsl:with-param>
				<xsl:with-param name="position">footer</xsl:with-param>
				<xsl:with-param name="class" select="$containerClass"/>
			</xsl:apply-templates>
		</xsl:if>
	</xsl:template>

	<!-- ## Default Layout  ############################################################################   -->
	<xsl:template match="Page" mode="Layout">
		<xsl:param name="containerClass"/>
		<div class="template" id="template_1_Column"  role="main">
			<xsl:apply-templates select="." mode="layoutHeader">
				<xsl:with-param name="containerClass" select="$containerClass"/>
			</xsl:apply-templates>
			<div class="{$containerClass} content">
				<xsl:choose>
					<xsl:when test="@layout=''">
						<xsl:call-template name="term2000" />
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="term2001" />
						<xsl:text>&#160;</xsl:text>
						<strong>
							<xsl:value-of select="@layout"/>
						</strong>
						<xsl:text>&#160;</xsl:text>
						<xsl:call-template name="term2002" />
					</xsl:otherwise>
				</xsl:choose>
				<xsl:text> </xsl:text>
			</div>
			<xsl:apply-templates select="." mode="layoutFooter">
				<xsl:with-param name="containerClass" select="$containerClass"/>
			</xsl:apply-templates>
			<xsl:apply-templates select="." mode="socialBookmarks" />
		</div>
	</xsl:template>

	<!-- ## Error Layout   #############################################################################   -->
	<xsl:template match="Page[@layout='Error']" mode="Layout">
		<xsl:param name="containerClass"/>
		<div class="container content" id="Error" >
			<xsl:choose>
				<xsl:when test="/Page/Contents/Content[@name='1005']">
					<xsl:apply-templates select="/Page/Contents/Content[@name='1005']" mode="displayBrief"/>
				</xsl:when>
				<xsl:when test="Contents/Content">
					<xsl:apply-templates select="Contents/Content" mode="displayBrief"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:call-template name="term2003" />
				</xsl:otherwise>
			</xsl:choose>
		</div>
	</xsl:template>

	<!-- ## Module Layouts   ###########################################################################   -->

	<xsl:template match="Page[@layout='Modules_1_column' or @layout='1_Column' or @type='default']" mode="Layout">
		<xsl:param name="containerClass"/>

		<div id="template_1_Column" class="template template_1_Column"  role="main">
			<xsl:apply-templates select="." mode="layoutHeader">
				<xsl:with-param name="containerClass" select="$containerClass"/>
			</xsl:apply-templates>
			<div>
				<xsl:if test="/Page/Contents/Content[@name='column1' or @position='column1'] or /Page/@adminMode">
					<xsl:apply-templates select="/Page" mode="addModule">
						<xsl:with-param name="text">Add Module</xsl:with-param>
						<xsl:with-param name="position">column1</xsl:with-param>
						<xsl:with-param name="class" select="$containerClass"/>
					</xsl:apply-templates>
				</xsl:if>
			</div>
		</div>
		<xsl:apply-templates select="." mode="layoutFooter">
			<xsl:with-param name="containerClass" select="$containerClass"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="Page[@layout='Modules_2_columns' or @layout='Modules_2_columns_66_33' or @layout='Modules_2_columns_33_66' or @layout='Modules_2_columns_75_25' or @layout='Modules_2_columns_25_75']" mode="Layout">
		<xsl:param name="containerClass"/>
		<xsl:variable name="col1">
			<xsl:choose>
				<xsl:when test="contains(/Page/@layout,'66_33')">
					<xsl:text>col-md-8</xsl:text>
				</xsl:when>
				<xsl:when test="contains(/Page/@layout,'75_25')">
					<xsl:text>col-md-9</xsl:text>
				</xsl:when>
				<xsl:when test="contains(/Page/@layout,'25_75')">
					<xsl:text>col-md-3</xsl:text>
				</xsl:when>
				<xsl:when test="contains(/Page/@layout,'33_66')">
					<xsl:text>col-md-4</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>col-md-6</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="col2">
			<xsl:choose>
				<xsl:when test="contains(/Page/@layout,'66_33')">
					<xsl:text>col-md-4</xsl:text>
				</xsl:when>
				<xsl:when test="contains(/Page/@layout,'75_25')">
					<xsl:text>col-md-3</xsl:text>
				</xsl:when>
				<xsl:when test="contains(/Page/@layout,'25_75')">
					<xsl:text>col-md-9</xsl:text>
				</xsl:when>
				<xsl:when test="contains(/Page/@layout,'33_66')">
					<xsl:text>col-md-8</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>col-md-6</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<div class="template" role="main">
			<xsl:choose>
				<xsl:when test="contains(/Page/@layout,'66_33')">
					<xsl:attribute name="id">template_2_Columns_66_33</xsl:attribute>
					<xsl:attribute name="class">template template_2_Columns_66_33</xsl:attribute>
				</xsl:when>
				<xsl:when test="contains(/Page/@layout,'75_25')">
					<xsl:attribute name="id">template_2_Columns_75_25</xsl:attribute>
					<xsl:attribute name="class">template template_2_Columns_75_25</xsl:attribute>
				</xsl:when>
				<xsl:when test="contains(/Page/@layout,'25_75')">
					<xsl:attribute name="id">template_2_Columns_25_75</xsl:attribute>
					<xsl:attribute name="class">template template_2_Columns_25_75</xsl:attribute>
				</xsl:when>
				<xsl:when test="contains(/Page/@layout,'33_66')">
					<xsl:attribute name="id">template_2_Columns_33_66</xsl:attribute>
					<xsl:attribute name="class">template template_2_Columns_33_66</xsl:attribute>
				</xsl:when>
				<xsl:otherwise>
					<xsl:attribute name="id">template_2_Columns</xsl:attribute>
					<xsl:attribute name="class">template template_2_Columns</xsl:attribute>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:apply-templates select="." mode="layoutHeader">
				<xsl:with-param name="containerClass" select="$containerClass"/>
			</xsl:apply-templates>
			<section>
				<div class="{$containerClass}">
					<div class="row">
						<div id="column1" class="column1 {$col1}">
							<xsl:apply-templates select="/Page" mode="addModule">
								<xsl:with-param name="text">Add Module</xsl:with-param>
								<xsl:with-param name="position">column1</xsl:with-param>
								<xsl:with-param name="class">
									column1 <xsl:value-of select="$col1"/>
								</xsl:with-param>
							</xsl:apply-templates>
						</div>
						<div id="column2" class="column2 {$col2}">
							<xsl:apply-templates select="/Page" mode="addModule">
								<xsl:with-param name="text">Add Module</xsl:with-param>
								<xsl:with-param name="position">column2</xsl:with-param>
								<xsl:with-param name="class">
									column2 <xsl:value-of select="$col2"/>
								</xsl:with-param>
							</xsl:apply-templates>
						</div>
					</div>
				</div>
			</section>
			<xsl:apply-templates select="." mode="layoutFooter">
				<xsl:with-param name="containerClass" select="$containerClass"/>
			</xsl:apply-templates>
		</div>
	</xsl:template>

	<xsl:template match="Page[@layout='Modules_3_columns' or @layout='Modules_3_columns_50_25_25' or @layout='Modules_3_columns_25_25_50']" mode="Layout">
		<xsl:param name="containerClass"/>
		<xsl:variable name="col1">
			<xsl:choose>
				<xsl:when test="contains(/Page/@layout,'50_25_25')">
					<xsl:text>col-md-6</xsl:text>
				</xsl:when>
				<xsl:when test="contains(/Page/@layout,'25_25_50')">
					<xsl:text>col-md-3</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>col-md-4</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="col2">
			<xsl:choose>
				<xsl:when test="contains(/Page/@layout,'50_25_25')">
					<xsl:text>col-md-3</xsl:text>
				</xsl:when>
				<xsl:when test="contains(/Page/@layout,'25_25_50')">
					<xsl:text>col-md-3</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>col-md-4</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="col3">
			<xsl:choose>
				<xsl:when test="contains(/Page/@layout,'50_25_25')">
					<xsl:text>col-md-3</xsl:text>
				</xsl:when>
				<xsl:when test="contains(/Page/@layout,'25_25_50')">
					<xsl:text>col-md-6</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>col-md-4</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<div class="template">
			<xsl:choose>
				<xsl:when test="contains(/Page/@layout,'50_25_25')">
					<xsl:attribute name="id">template_3_Columns_50_25_25</xsl:attribute>
					<xsl:attribute name="class">template template_3_Columns_50_25_25</xsl:attribute>
				</xsl:when>
				<xsl:when test="contains(/Page/@layout,'25_25_50')">
					<xsl:attribute name="id">template_3_Columns_25_25_50</xsl:attribute>
					<xsl:attribute name="class">template template_3_Columns_25_25_50</xsl:attribute>
				</xsl:when>
				<xsl:otherwise>
					<xsl:attribute name="id">template_3_Columns</xsl:attribute>
					<xsl:attribute name="class">template template_3_Columns</xsl:attribute>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:apply-templates select="." mode="layoutHeader">
				<xsl:with-param name="containerClass" select="$containerClass"/>
			</xsl:apply-templates>
			<section>
				<div class="{$containerClass} template template_3_Columns" id="template_3_Columns">
					<div class="row">
						<div id="column1" class="column1 {$col1}">
							<xsl:apply-templates select="/Page" mode="addModule">
								<xsl:with-param name="text">Add Module</xsl:with-param>
								<xsl:with-param name="position">column1</xsl:with-param>
								<xsl:with-param name="class">
									<xsl:text>column1 </xsl:text>
									<xsl:value-of select="$col1"/>
								</xsl:with-param>
							</xsl:apply-templates>
						</div>
						<div id="column2" class="column2 {$col2}">
							<xsl:apply-templates select="/Page" mode="addModule">
								<xsl:with-param name="text">Add Module</xsl:with-param>
								<xsl:with-param name="position">column2</xsl:with-param>
								<xsl:with-param name="class">
									<xsl:text>column2 </xsl:text>
									<xsl:value-of select="$col2"/>
								</xsl:with-param>
							</xsl:apply-templates>
						</div>
						<div id="column3" class="column3 {$col3}">
							<xsl:apply-templates select="/Page" mode="addModule">
								<xsl:with-param name="text">Add Module</xsl:with-param>
								<xsl:with-param name="position">column3</xsl:with-param>
								<xsl:with-param name="class">
									<xsl:text>column3 </xsl:text>
									<xsl:value-of select="$col3"/>
								</xsl:with-param>
							</xsl:apply-templates>
						</div>
					</div>
				</div>
			</section>
			<xsl:apply-templates select="." mode="layoutFooter">
				<xsl:with-param name="containerClass" select="$containerClass"/>
			</xsl:apply-templates>
		</div>
	</xsl:template>

	<xsl:template match="Page[@layout='Modules_4_columns']" mode="Layout">
		<xsl:param name="containerClass"/>
		<xsl:variable name="col4">
			<xsl:text>col-md-3</xsl:text>
		</xsl:variable>
		<div class="template template_4_Columns " id="template_4_Columns">
			<xsl:apply-templates select="." mode="layoutHeader">
				<xsl:with-param name="containerClass" select="$containerClass"/>
			</xsl:apply-templates>
			<section>
				<div class="{$containerClass}">
					<div class="row">
						<div id="column1" class="column1 {$col4}">
							<xsl:apply-templates select="/Page" mode="addModule">
								<xsl:with-param name="text">Add Module</xsl:with-param>
								<xsl:with-param name="position">column1</xsl:with-param>
								<xsl:with-param name="class">
									<xsl:text>column1 </xsl:text>
									<xsl:value-of select="$col4"/>
								</xsl:with-param>
							</xsl:apply-templates>
						</div>
						<div id="column2" class="column2 {$col4}">
							<xsl:apply-templates select="/Page" mode="addModule">
								<xsl:with-param name="text">Add Module</xsl:with-param>
								<xsl:with-param name="position">column2</xsl:with-param>
								<xsl:with-param name="class">
									<xsl:text>column2 </xsl:text>
									<xsl:value-of select="$col4"/>
								</xsl:with-param>
							</xsl:apply-templates>
						</div>
						<div id="column3" class="column3 {$col4}">
							<xsl:apply-templates select="/Page" mode="addModule">
								<xsl:with-param name="text">Add Module</xsl:with-param>
								<xsl:with-param name="position">column3</xsl:with-param>
								<xsl:with-param name="class">
									<xsl:text>column3 </xsl:text>
									<xsl:value-of select="$col4"/>
								</xsl:with-param>
							</xsl:apply-templates>
						</div>
						<div id="column4" class="column4 {$col4}">
							<xsl:apply-templates select="/Page" mode="addModule">
								<xsl:with-param name="text">Add Module</xsl:with-param>
								<xsl:with-param name="position">column4</xsl:with-param>
								<xsl:with-param name="class">
									<xsl:text>column4 </xsl:text>
									<xsl:value-of select="$col4"/>
								</xsl:with-param>
							</xsl:apply-templates>
						</div>
					</div>
				</div>
			</section>
			<xsl:apply-templates select="." mode="layoutFooter">
				<xsl:with-param name="containerClass" select="$containerClass"/>
			</xsl:apply-templates>
		</div>
	</xsl:template>

	<xsl:template match="Page[@layout='Modules_Masonary']" mode="Layout">
		<xsl:param name="containerClass"/>
		<xsl:variable name="colcount" select="'6'"/>
		<div class="template template_Masonary" id="template_Masonary">
			<xsl:apply-templates select="." mode="layoutHeader">
				<xsl:with-param name="containerClass" select="$containerClass"/>
			</xsl:apply-templates>
			<div id="column1">
				<xsl:apply-templates select="/Page" mode="addMasonaryModule">
					<xsl:with-param name="text">Add Module</xsl:with-param>
					<xsl:with-param name="position">column1-1col</xsl:with-param>
					<xsl:with-param name="class">column1-1col</xsl:with-param>
				</xsl:apply-templates>
			</div>
			<xsl:apply-templates select="." mode="layoutFooter">
				<xsl:with-param name="containerClass" select="$containerClass"/>
			</xsl:apply-templates>
		</div>
		<script type="text/javascript">
			$('#isotope-module').isotope({
			itemSelector : '.module',
			containerStyle: {position: 'relative'},
			masonry: {
			columnWidth: 1
			}
			});
			$(function () {
			var zIndexNumber = 10000;
			$('#isotope-module .editable,#isotope-module div.options').each(function () {
			$(this).css('zIndex', zIndexNumber);
			zIndexNumber -= 1;
			});
			});
		</script>
	</xsl:template>

	<xsl:template match="Content[@type='Module']" mode="themeModuleExtras">
		<xsl:if test="@modAnim!=''">
			<xsl:attribute name="data-modanim">
				<xsl:value-of select="@modAnim"/>
			</xsl:attribute>
			<xsl:attribute name="data-modanimdelay">
				<xsl:value-of select="@modAnimDelay"/>
			</xsl:attribute>
			<xsl:attribute name="data-modanimspeed">
				<xsl:value-of select="@modAnimSpeed"/>
			</xsl:attribute>
		</xsl:if>
	</xsl:template>

	<xsl:template match="Content[@type='Module']" mode="themeModuleClassExtras">
		<xsl:if test="@modAnim and @modAnim!=''">
			<xsl:text> moduleAnimate-invisible</xsl:text>
		</xsl:if>
	</xsl:template>

	<xsl:template match="*" mode="themeModuleExtrasListItem">
		<xsl:param name="parentId"/>
		<xsl:param name="pos"/>
		<xsl:variable name="parentModule" select="$page/Contents/Content[@id=$parentId]"/>
		<xsl:if test="$parentModule/@modAnim!=''">
			<xsl:attribute name="data-modanim">
				<xsl:value-of select="$parentModule/@modAnim"/>
			</xsl:attribute>
			<xsl:attribute name="data-modanimdelay">
				<xsl:value-of select="($parentModule/@modAnimDelay * $pos) - $parentModule/@modAnimDelay"/>
			</xsl:attribute>
			<xsl:attribute name="data-modanimspeed">
				<xsl:value-of select="$parentModule/@modAnimSpeed"/>
			</xsl:attribute>
		</xsl:if>
	</xsl:template>

	<xsl:template match="*" mode="themeModuleClassExtrasListItem">
		<xsl:param name="parentId"/>
		<xsl:variable name="parentModule" select="$page/Contents/Content[@id=$parentId]"/>

		<xsl:if test="$parentModule/@modAnim and $parentModule/@modAnim!=''">
			<xsl:text> moduleAnimate-invisible</xsl:text>
		</xsl:if>
	</xsl:template>

	<!-- ## Module Handlers - Boxes, No-Boxes, Links and Titles  #######################################   -->
	<xsl:template match="Content[@type='Module']" mode="displayModule">
		<xsl:param name="auto-col"/>
		<xsl:param name="width"/>
		<xsl:param name="id"/>
		<xsl:choose>
			<xsl:when test="$auto-col='true'">
				<div class="col">
					<xsl:if test="$width!=''">
						<xsl:attribute name="style">
							<xsl:text>width:</xsl:text>
							<xsl:value-of select="$width"/>
							<xsl:text>px;</xsl:text>
						</xsl:attribute>
					</xsl:if>
					<xsl:choose>
						<xsl:when test="@box!='false' and @box!=''">
							<xsl:apply-templates select="." mode="moduleBox">
								<xsl:with-param name="id" select="$id"/>
							</xsl:apply-templates>
						</xsl:when>
						<xsl:otherwise>
							<xsl:apply-templates select="." mode="displayModuleContent">
								<xsl:with-param name="id" select="$id"/>
							</xsl:apply-templates>
						</xsl:otherwise>
					</xsl:choose>
				</div>
			</xsl:when>
			<xsl:otherwise>
				<xsl:choose>
					<xsl:when test="@box!='false' and @box!=''">
						<xsl:apply-templates select="." mode="moduleBox">
							<xsl:with-param name="id" select="$id"/>
						</xsl:apply-templates>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="." mode="displayModuleContent">
							<xsl:with-param name="id" select="$id"/>
						</xsl:apply-templates>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="Content" mode="displayModuleContent">
		<xsl:param name="id"/>
		<xsl:variable name="thisClass">
			<xsl:if test="@iconStyle='Centre'"> module-centred</xsl:if>
			<xsl:if test="@iconStyle='CentreSmall'"> module-centred</xsl:if>
			<xsl:if test="@iconStyle='Right'"> module-right</xsl:if>
			<xsl:if test="@iconStyle='Left'"> module-left</xsl:if>
			<xsl:if test="@icon!='' or @uploadIcon!=''"> module-containing-icon</xsl:if>
		</xsl:variable>
		<div id="mod_{@id}{$id}" class="module nobox pos-{@position} {$thisClass}">
			<xsl:apply-templates select="." mode="themeModuleExtras"/>
			<xsl:if test="@mobileview!=''">
				<xsl:attribute name="data-isMobileView">
					<xsl:value-of select="@mobileview"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:attribute name="class">
				<xsl:text>module nobox pos-</xsl:text>
				<xsl:value-of select="@position"/>
				<xsl:text> module-</xsl:text>
				<xsl:value-of select="@moduleType"/>
				<xsl:if test="@panelImage!=''">
					<xsl:text> panelImage </xsl:text>
				</xsl:if>
				<xsl:if test="@responsiveImg='true'">
					<xsl:text> module-img-responsive</xsl:text>
				</xsl:if>
				<xsl:apply-templates select="." mode="hideScreens" />
				<xsl:apply-templates select="." mode="marginBelow" />
				<xsl:apply-templates select="." mode="themeModuleClassExtras"/>
				<xsl:value-of select="$thisClass"/>
				<xsl:if test="@moduleType='Image'">
					<xsl:text> img-module-flex justify-content-</xsl:text>
					<xsl:value-of select="@position-vertical"/>
					<xsl:text> align-items-</xsl:text>
					<xsl:value-of select="@position-horizontal"/>
				</xsl:if>
			</xsl:attribute>
			<xsl:if test="@contentType='Module'">
				<xsl:attribute name="class">
					<xsl:text>module noboxlayout layoutModule pos-</xsl:text>
					<xsl:value-of select="@position"/>
					<xsl:text> </xsl:text>
					<xsl:if test="not(@position='header') and not(@position='footer') and not(@position='column1')">
						<xsl:value-of select="@background"/>
					</xsl:if>
					<xsl:apply-templates select="." mode="hideScreens" />
					<xsl:apply-templates select="." mode="marginBelow" />
					<xsl:value-of select="$thisClass"/>
				</xsl:attribute>
				<!--<xsl:if test="@backgroundImage!=''">
              <xsl:attribute name="style">
                background-image: url('<xsl:value-of select="@backgroundImage"/>');
              </xsl:attribute>
            </xsl:if>-->
			</xsl:if>
			<xsl:if test="@moduleType='Accordion'">
				<xsl:attribute name="class">
					<xsl:text>module nobox layoutModule accordion-module pos-</xsl:text>
					<xsl:value-of select="@position"/>
					<xsl:apply-templates select="." mode="hideScreens" />
					<xsl:apply-templates select="." mode="marginBelow" />
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="@moduleType='Tabbed'">
				<xsl:attribute name="class">
					<xsl:text>module layoutModule tabbed-module pos-</xsl:text>
					<xsl:value-of select="@position"/>
					<xsl:apply-templates select="." mode="hideScreens" />
					<xsl:apply-templates select="." mode="marginBelow" />
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and @imagePosition='above'">
				<div class="panel-image">
					<img src="{@panelImage}" alt="{@title}" class="img-responsive" />
				</div>
			</xsl:if>
			<xsl:if test="not(@position='header' or @position='footer' or (@position='column1' and $page/@layout='Modules_1_column'))">
				<!--<xsl:if test="@data-stellar-background-ratio!='0'">
              <xsl:attribute name="data-stellar-background-ratio">
                <xsl:value-of select="(@data-stellar-background-ratio div 10)"/> test
              </xsl:attribute>
            </xsl:if>-->
				<xsl:if test="@backgroundImage!=''">
					<!--<xsl:attribute name="style">
                background-image: url('<xsl:value-of select="@backgroundImage"/>');
              </xsl:attribute>-->

					<xsl:choose>
						<xsl:when test="@data-stellar-background-ratio!='0'">
							<xsl:choose>
								<xsl:when test="@data-stellar-background-ratio!='10'">
									<section style="height:100%" class="parallax-wrapper" >
										<xsl:if test="@data-stellar-background-ratio!='10'">
											<xsl:attribute name="data-parallax-speed">
												<xsl:if test="@data-stellar-background-ratio&lt;'5'">
													<xsl:text>1.3</xsl:text>
												</xsl:if>
												<xsl:if test="@data-stellar-background-ratio&gt;='5' and @data-stellar-background-ratio&lt;'10'">
													<xsl:text>1.6</xsl:text>
												</xsl:if>
												<xsl:if test="@data-stellar-background-ratio&gt;='10' and @data-stellar-background-ratio&lt;'15'">
													<xsl:text>2</xsl:text>
												</xsl:if>
												<xsl:if test="@data-stellar-background-ratio&gt;='15' and @data-stellar-background-ratio&lt;'20'">
													<xsl:text>3</xsl:text>
												</xsl:if>
												<xsl:if test="@data-stellar-background-ratio&gt;='20' and @data-stellar-background-ratio&lt;'25'">
													<xsl:text>4</xsl:text>
												</xsl:if>
												<xsl:if test="@data-stellar-background-ratio&gt;='25'">
													<xsl:text>5</xsl:text>
												</xsl:if>
											</xsl:attribute>
										</xsl:if>
										<div class="parallax" data-parallax-image="{@backgroundImage}">

										</div>
									</section>
								</xsl:when>
								<xsl:otherwise>
									<xsl:if test="not(@position='header' or @position='footer' or (@position='column1' and $page/@layout='Modules_1_column'))">
										<xsl:attribute name="style">
											background-image: url('<xsl:value-of select="@backgroundImage"/>');
										</xsl:attribute>
									</xsl:if>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<xsl:otherwise>
							<xsl:attribute name="style">
								background-image: url('<xsl:value-of select="@backgroundImage"/>');
							</xsl:attribute>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:if>
			</xsl:if>
			<xsl:choose>
				<xsl:when test="$page/AdminMenu/descendant-or-self::MenuItem[@cmd='Normal'] and $adminMode">
					<div>
						<xsl:apply-templates select="." mode="inlinePopupOptions" />
						<xsl:text> </xsl:text>
						<xsl:if test="@title!='' or @icon!='' or @uploadIcon!=''">
							<xsl:choose>
								<xsl:when test="@contentType='Module'">
									<h2 class="layout-title">
										<xsl:apply-templates select="." mode="moduleLink"/>
									</h2>
								</xsl:when>
								<xsl:otherwise>
									<xsl:choose>
										<xsl:when test="@heading and @heading!=''">
											<xsl:element name="{@heading}">
												<xsl:attribute name="class">
													<xsl:text>title</xsl:text>
													<xsl:if test="@icon!='' or @uploadIcon!=''">
														<xsl:text> module-with-icon</xsl:text>
													</xsl:if>
												</xsl:attribute>
												<xsl:apply-templates select="." mode="moduleLink"/>
											</xsl:element>
										</xsl:when>
										<xsl:otherwise>
											<h3 class="title">
												<xsl:if test="@icon!='' or @uploadIcon!=''">
													<xsl:attribute name="class">title module-with-icon</xsl:attribute>
												</xsl:if>
												<xsl:apply-templates select="." mode="moduleLink"/>
											</h3>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:if>
					</div>
				</xsl:when>
				<xsl:otherwise>
					<xsl:if test="@title!='' or @icon!='' or @uploadIcon!=''">
						<xsl:choose>
							<xsl:when test="@contentType='Module'">
								<h2 class="layout-title">
									<xsl:apply-templates select="." mode="moduleLink"/>
								</h2>
							</xsl:when>
							<xsl:otherwise>
								<xsl:choose>
									<xsl:when test="@heading and @heading!=''">

										<xsl:element name="{@heading}">
											<xsl:attribute name="class">
												<xsl:text>title</xsl:text>
												<xsl:if test="@icon!='' or @uploadIcon!=''">
													<xsl:text> module-with-icon</xsl:text>
												</xsl:if>
											</xsl:attribute>
											<!--<xsl:if test="@icon!='' or @uploadIcon!=''">
												<xsl:attribute name="class">title module-with-icon</xsl:attribute>
											</xsl:if>-->
											<xsl:apply-templates select="." mode="moduleLink"/>
										</xsl:element>
									</xsl:when>
									<xsl:otherwise>
										<h3 class="title">
											<xsl:if test="@icon!='' or @uploadIcon!=''">
												<xsl:attribute name="class">title module-with-icon</xsl:attribute>
											</xsl:if>
											<xsl:apply-templates select="." mode="moduleLink"/>
										</h3>
									</xsl:otherwise>
								</xsl:choose>
								<!--<h3 class="title">
									<xsl:if test="@icon!='' or @uploadIcon!=''">
										<xsl:attribute name="class">title module-with-icon</xsl:attribute>
									</xsl:if>
									<xsl:apply-templates select="." mode="moduleLink"/>
								</h3>-->
							</xsl:otherwise>
						</xsl:choose>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:if test="@rss and @rss!='false'">
				<xsl:apply-templates select="." mode="rssLink" />
			</xsl:if>
			<xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and not(@imagePosition='above')">
				<div class="panel-image">
					<img src="{@panelImage}" alt="{@title}" class="img-responsive" />
				</div>
			</xsl:if>
			<xsl:apply-templates select="." mode="displayBrief"/>
			<xsl:if test="@linkText!='' and @link!=''">
				<div class="entryFooter">
					<xsl:if test="@iconStyle='Centre' or @iconStyle='CentreSmall'">
						<xsl:attribute name="class">entryFooter center-nobox-footer</xsl:attribute>
					</xsl:if>
					<xsl:apply-templates select="." mode="moreLink">
						<xsl:with-param name="link">
							<xsl:choose>
								<xsl:when test="format-number(@link,'0')!='NaN'">
									<xsl:variable name="pageId" select="@link"/>
									<xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId|PageVersion/@vParId=$pageId]" mode="getHref"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="@link"/>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:with-param>
						<xsl:with-param name="linkText" select="@linkText"/>
						<xsl:with-param name="altText" select="@title"/>
						<xsl:with-param name="linkWindow" select="@linkWindow"/>
						<xsl:with-param name="linkObject" select="@linkObject"/>
					</xsl:apply-templates>
					<xsl:text> </xsl:text>
				</div>
			</xsl:if>
		</div>
	</xsl:template>

	<xsl:template match="Content" mode="hideScreens">
		<xsl:if test="not($adminMode)">
			<xsl:if test="contains(@screens,'lg')">
				<xsl:text> hidden-lg</xsl:text>
			</xsl:if>
			<xsl:if test="contains(@screens,'md')">
				<xsl:text> hidden-md</xsl:text>
			</xsl:if>
			<xsl:if test="contains(@screens,'sm')">
				<xsl:text> hidden-sm</xsl:text>
			</xsl:if>
			<xsl:if test="contains(@screens,'xs')">
				<xsl:text> hidden-xs</xsl:text>
			</xsl:if>
		</xsl:if>
		<xsl:if test="@matchHeight='true'">
			<xsl:text> matchHeight</xsl:text>
		</xsl:if>
	</xsl:template>

	<xsl:template match="Content" mode="marginBelow">
		<xsl:if test="@marginBelow='false'">
			<xsl:text> mb-0 </xsl:text>
		</xsl:if>
	</xsl:template>

	<xsl:template match="Content" mode="moduleBox">
		<xsl:param name="id"/>
		<xsl:choose>
			<xsl:when test="@linkBox='true'">

			</xsl:when>
			<xsl:otherwise>
				<div id="mod_{@id}{$id}">
					<xsl:apply-templates select="." mode="themeModuleExtras"/>
					<!-- define classes for box -->
					<xsl:attribute name="class">
						<xsl:if test="@panelImage!=''">
							<xsl:text>panelImage </xsl:text>
						</xsl:if>
						<xsl:if test="@icon!='' or @uploadIcon!=''">
							<xsl:text>panel-icon </xsl:text>
						</xsl:if>
						<xsl:value-of select="translate(@box,' ','-')"/>

						<xsl:text> module</xsl:text>
						<!-- if no title, we may still want TL/TR for rounded boxs with no title bar,
              stled differently to a title bar. -->
						<xsl:if test="@title=''">
							<xsl:text> boxnotitle</xsl:text>
						</xsl:if>
						pos-<xsl:value-of select="@position"/>
						<xsl:if test="@modAnim and @modAnim!=''">
							<xsl:text> moduleAnimate-invisible</xsl:text>
						</xsl:if>
						<xsl:apply-templates select="." mode="hideScreens" />
						<xsl:apply-templates select="." mode="marginBelow" />
						<!-- <xsl:apply-templates select="." mode="themeModuleExtras"/>-->
					</xsl:attribute>
					<xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and @imagePosition='above'">
						<div class="panel-image">
							<img src="{@panelImage}" alt="{@title}" class="img-responsive" />
						</div>
					</xsl:if>
					<xsl:if test="@title!='' or @icon!='' or @uploadIcon!=''">

						<xsl:apply-templates select="." mode="inlinePopupOptions"/>
						<xsl:if test="@rss and @rss!='false'">
							<xsl:apply-templates select="." mode="rssLink" />
						</xsl:if>
						<h5>
							<xsl:apply-templates select="." mode="moduleLink"/>
						</h5>
					</xsl:if>
					<xsl:if test="not(@listGroup='true')">
						<xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and not(@imagePosition='above')">
							<div class="panel-image">
								<img src="{@panelImage}" alt="{@title}" class="img-responsive" />
							</div>
						</xsl:if>
						<div>
							<xsl:if test="not(@title!='')">
								<xsl:apply-templates select="." mode="inlinePopupOptions"/>
							</xsl:if>
							<xsl:apply-templates select="." mode="displayBrief"/>
						</div>
					</xsl:if>
					<xsl:if test="@listGroup='true'">
						<div class="card-body">
							<xsl:if test="not(@title!='')">
								<xsl:apply-templates select="." mode="inlinePopupOptions">
									<xsl:with-param name="class" select="'card-body'"/>
								</xsl:apply-templates>
							</xsl:if>
							<xsl:apply-templates select="." mode="displayBrief"/>
							<xsl:text> </xsl:text>
						</div>
					</xsl:if>
					<xsl:if test="@linkText!='' and @link!=''">
						<div>
							<xsl:if test="@iconStyle='Centre'">
								<xsl:attribute name="class">center-block-footer</xsl:attribute>
							</xsl:if>
							<xsl:apply-templates select="." mode="moreLink">
								<xsl:with-param name="link">
									<xsl:choose>
										<xsl:when test="format-number(@link,'0')!='NaN'">
											<xsl:variable name="pageId" select="@link"/>
											<xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId or PageVersion[@vParId=$pageId]]" mode="getHref"/>
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="@link"/>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:with-param>
								<xsl:with-param name="linkText" select="@linkText"/>
								<xsl:with-param name="altText" select="@title"/>
							</xsl:apply-templates>
							<xsl:text> </xsl:text>
						</div>
					</xsl:if>
				</div>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="Content[starts-with(@box,'bg') or starts-with(@box,'border') or starts-with(@box,'Default') or starts-with(@box,'card')]" mode="moduleBox">
		<xsl:param name="id"/>
		<xsl:choose>
			<xsl:when test="@linkBox='true'">

			</xsl:when>
			<xsl:otherwise>
				<div id="mod_{@id}{$id}" class="card">
					<xsl:apply-templates select="." mode="themeModuleExtras"/>
					<!-- define classes for box -->
					<xsl:attribute name="class">
						<xsl:text>card </xsl:text>
						<xsl:if test="@panelImage!=''">
							<xsl:text>panelImage </xsl:text>
						</xsl:if>
						<xsl:if test="@icon!='' or @uploadIcon!=''">
							<xsl:text>panel-icon </xsl:text>
						</xsl:if>
						<xsl:choose>
							<xsl:when test="@box='Default Box'">card</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="translate(@box,' ','-')"/>
							</xsl:otherwise>
						</xsl:choose>
						<xsl:text> module</xsl:text>
						<xsl:text> module-</xsl:text>
						<xsl:value-of select="@moduleType"/>
						<!-- if no title, we may still want TL/TR for rounded boxs with no title bar,
              stled differently to a title bar. -->
						<xsl:if test="@title=''">
							<xsl:text> boxnotitle</xsl:text>
						</xsl:if>
						pos-<xsl:value-of select="@position"/>
						<xsl:if test="@modAnim and @modAnim!=''">
							<xsl:text> moduleAnimate-invisible</xsl:text>
						</xsl:if>
						<xsl:apply-templates select="." mode="hideScreens" />
						<xsl:apply-templates select="." mode="marginBelow" />
						<!--<xsl:apply-templates select="." mode="themeModuleExtras"/>-->
					</xsl:attribute>
					<xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and @imagePosition='above'">
						<div class="panel-image">
							<img src="{@panelImage}" alt="{@title}" class="img-responsive" />
						</div>
					</xsl:if>
					<xsl:if test="@title!='' or @icon!='' or @uploadIcon!=''">
						<div class="card-header">
							<xsl:apply-templates select="." mode="inlinePopupOptions">
								<xsl:with-param name="class" select="'card-header'"/>
							</xsl:apply-templates>
							<xsl:if test="@rss and @rss!='false'">
								<xsl:apply-templates select="." mode="rssLink" />
							</xsl:if>
							<!--<xsl:if test="@heading='h2'">
					<h5 class="card-title">
						<xsl:apply-templates select="." mode="moduleLink"/>
					</h5>
				</xsl:if>-->
							<xsl:choose>
								<xsl:when test="@heading and @heading!=''">
									<xsl:element name="{@heading}">
										<xsl:attribute name="class">
											<xsl:text>card-title</xsl:text>
										</xsl:attribute>
										<xsl:apply-templates select="." mode="moduleLink"/>
									</xsl:element>
								</xsl:when>
								<xsl:otherwise>
									<h5 class="card-title">
										<xsl:apply-templates select="." mode="moduleLink"/>
									</h5>
								</xsl:otherwise>
							</xsl:choose>
						</div>
					</xsl:if>
					<xsl:if test="not(@listGroup='true')">
						<xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and not(@imagePosition='above')">
							<div class="panel-image">
								<img src="{@panelImage}" alt="{@title}" class="img-responsive" />
							</div>
						</xsl:if>
						<div class="card-body">
							<xsl:if test="not(@title!='')">
								<xsl:apply-templates select="." mode="inlinePopupOptions">
									<xsl:with-param name="class" select="'card-body'"/>
								</xsl:apply-templates>
							</xsl:if>
							<xsl:apply-templates select="." mode="displayBrief"/>

							<xsl:text> </xsl:text>
						</div>
					</xsl:if>
					<xsl:if test="@listGroup='true'">
						<div class="card-body">
							<xsl:if test="not(@title!='')">
								<xsl:apply-templates select="." mode="inlinePopupOptions">
									<xsl:with-param name="class" select="'card-body'"/>
								</xsl:apply-templates>
							</xsl:if>
							<xsl:apply-templates select="." mode="displayBrief"/>

							<xsl:text> </xsl:text>
						</div>
					</xsl:if>
					<xsl:if test="@linkText!='' and @link!=''">
						<div class="card-footer">
							<xsl:if test="@iconStyle='Centre'">
								<xsl:attribute name="class">card-footer center-block-footer</xsl:attribute>
							</xsl:if>
							<xsl:apply-templates select="." mode="moreLink">
								<xsl:with-param name="link">
									<xsl:choose>
										<xsl:when test="format-number(@link,'0')!='NaN'">
											<xsl:variable name="pageId" select="@link"/>
											<xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId or PageVersion[@vParId=$pageId]]" mode="getHref"/>
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="@link"/>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:with-param>
								<xsl:with-param name="linkText" select="@linkText"/>
								<xsl:with-param name="altText" select="@title"/>
							</xsl:apply-templates>
							<xsl:text> </xsl:text>
						</div>
					</xsl:if>
				</div>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="Content" mode="modalBox">
		<div id="mod_{@id}">
			<xsl:apply-templates select="." mode="themeModuleExtras"/>
			<!-- define classes for box -->
			<xsl:attribute name="class">
				<xsl:text>modal-content </xsl:text>
				<xsl:if test="@icon!='' or @uploadIcon!=''">
					<xsl:text>panel-icon </xsl:text>
				</xsl:if>
				<xsl:choose>
					<xsl:when test="@box='Default Box'">card</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="translate(@box,' ','-')"/>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:text> module</xsl:text>
				<!-- if no title, we may still want TL/TR for rounded boxs with no title bar,
              stled differently to a title bar. -->
				<xsl:if test="@title=''">
					<xsl:text> boxnotitle</xsl:text>
				</xsl:if>
				pos-<xsl:value-of select="@position"/>
				<xsl:apply-templates select="." mode="hideScreens" />
				<xsl:apply-templates select="." mode="marginBelow" />
				<!-- <xsl:apply-templates select="." mode="themeModuleExtras"/>-->
			</xsl:attribute>
			<xsl:if test="@title!='' or @icon!='' or @uploadIcon!=''">
				<div class="modal-header">
					<xsl:apply-templates select="." mode="inlinePopupOptions">
						<xsl:with-param name="class" select="'panel-heading'"/>
					</xsl:apply-templates>
					<xsl:if test="@rss and @rss!='false'">
						<xsl:apply-templates select="." mode="rssLink" />
					</xsl:if>
					<button type="button" class="close" data-dismiss="modal" aria-label="Close">
						<span aria-hidden="true">
							<i class="fa fa-times">&#160;</i>
						</span>
					</button>
					<h4 class="modal-title">
						<xsl:apply-templates select="." mode="moduleLink"/>
					</h4>
				</div>
			</xsl:if>
			<xsl:if test="not(@listGroup='true')">
				<xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_'">
					<div class="panel-image">
						<img src="{@panelImage}" alt="{@title}" class="img-responsive" />
					</div>
				</xsl:if>
				<div class="modal-body">
					<xsl:if test="not(@title!='')">
						<xsl:apply-templates select="." mode="inlinePopupOptions">
							<xsl:with-param name="class" select="'panel-body'"/>
						</xsl:apply-templates>
					</xsl:if>
					<xsl:apply-templates select="." mode="displayBrief"/>
				</div>
			</xsl:if>
			<xsl:if test="@listGroup='true'">
				<div class="card-body">
					<xsl:if test="not(@title!='')">
						<xsl:apply-templates select="." mode="inlinePopupOptions">
							<xsl:with-param name="class" select="'card-body'"/>
						</xsl:apply-templates>
					</xsl:if>
					<xsl:apply-templates select="." mode="displayBrief"/>

					<xsl:text> </xsl:text>
				</div>
			</xsl:if>
			<xsl:if test="@linkText!='' and @link!=''">
				<div class="card-footer">
					<xsl:if test="@iconStyle='Centre'">
						<xsl:attribute name="class">panel-footer center-block-footer</xsl:attribute>
					</xsl:if>
					<xsl:apply-templates select="." mode="moreLink">
						<xsl:with-param name="link">
							<xsl:choose>
								<xsl:when test="format-number(@link,'0')!='NaN'">
									<xsl:variable name="pageId" select="@link"/>
									<xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId or PageVersion[@vParId=$pageId]]" mode="getHref"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="@link"/>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:with-param>
						<xsl:with-param name="linkText" select="@linkText"/>
						<xsl:with-param name="altText" select="@title"/>
					</xsl:apply-templates>
					<xsl:text> </xsl:text>
				</div>
			</xsl:if>
		</div>
	</xsl:template>

	<xsl:template match="Content[starts-with(@box,'alert')]" mode="moduleBox">
		<xsl:param name="id"/>
		<xsl:choose>
			<xsl:when test="@linkBox='true'">
				<div id="mod_{@id}{$id}" class="module">
					<xsl:apply-templates select="." mode="themeModuleExtras"/>
					<div class="linkedPopUp">
						<xsl:apply-templates select="." mode="inlinePopupOptions">
							<xsl:with-param name="class" select="'linkedPopUp'"/>
						</xsl:apply-templates>
					</div>
					<a>
						<xsl:attribute name="href">
							<xsl:choose>
								<xsl:when test="format-number(@link,'0')!='NaN'">
									<xsl:variable name="pageId" select="@link"/>
									<xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId or PageVersion[@vParId=$pageId]]" mode="getHref"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="@link"/>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:attribute>
						<xsl:attribute name="class">
							<xsl:text>linked-panel</xsl:text>
						</xsl:attribute>
						<div class="alert linked-panel">
							<!-- define classes for box -->
							<xsl:attribute name="class">
								<xsl:text>alert </xsl:text>
								<xsl:if test="@panelImage!=''">
									<xsl:text>panelImage alertImage </xsl:text>
								</xsl:if>
								<xsl:choose>
									<xsl:when test="@box='Default Box'">alert-default</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="translate(@box,' ','-')"/>
									</xsl:otherwise>
								</xsl:choose>
								<xsl:text> linked-panel</xsl:text>
								<!-- if no title, we may still want TL/TR for rounded boxs with no title bar,
              stled differently to a title bar. -->
								<xsl:if test="@title=''">
									<xsl:text> boxnotitle</xsl:text>
								</xsl:if>
								pos-<xsl:value-of select="@position"/>
								<xsl:if test="@modAnim and @modAnim!=''">
									<xsl:text> moduleAnimate-invisible</xsl:text>
								</xsl:if>
								<xsl:apply-templates select="." mode="hideScreens" />
								<xsl:apply-templates select="." mode="marginBelow" />
							</xsl:attribute>
							<xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and @imagePosition='above'">
								<div class="panel-image alert-image">
									<img src="{@panelImage}" alt="{@title}" class="img-responsive" />
								</div>
							</xsl:if>
							<xsl:if test="@title!='' or @icon!='' or @uploadIcon!=''">
								<div class="alert-heading">

									<h4 class="alert-title">
										<xsl:apply-templates select="." mode="moduleTitle"/>
									</h4>
								</div>
							</xsl:if>
							<xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and not(@imagePosition='above')">
								<div class="panel-image alert-image">
									<img src="{@panelImage}" alt="{@title}" class="img-responsive" />
								</div>
							</xsl:if>
							<div class="alert-body">
								<xsl:apply-templates select="." mode="displayBrief"/>
							</div>
						</div>
					</a>
				</div>
			</xsl:when>
			<xsl:otherwise>
				<xsl:variable name="class">
					<xsl:text>alert </xsl:text>
					<xsl:if test="@panelImage!=''">
						<xsl:text>panelImage </xsl:text>
					</xsl:if>
					<xsl:choose>
						<xsl:when test="@box='Default Box'">alert-default</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="translate(@box,' ','-')"/>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:text> module</xsl:text>
					<!-- if no title, we may still want TL/TR for rounded boxs with no title bar,
              stled differently to a title bar. -->
					<xsl:if test="@title=''">
						<xsl:text> boxnotitle</xsl:text>
					</xsl:if>
					pos-<xsl:value-of select="@position"/>
					<xsl:if test="@modAnim and @modAnim!=''">
						<xsl:text> moduleAnimate-invisible</xsl:text>
					</xsl:if>
					<xsl:apply-templates select="." mode="hideScreens" />
					<xsl:apply-templates select="." mode="marginBelow" />
				</xsl:variable>
				<div id="mod_{@id}" class="{$class}">
					<xsl:apply-templates select="." mode="themeModuleExtras"/>
					<xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and @imagePosition='above'">
						<div class="panel-image alert-image">
							<img src="{@panelImage}" alt="{@title}" class="img-responsive" />
						</div>
					</xsl:if>
					<xsl:if test="@title!='' or @icon!='' or @uploadIcon!=''">
						<div class="alert-heading">
							<xsl:apply-templates select="." mode="inlinePopupOptions">
								<xsl:with-param name="class" select="'alert-heading'"/>
							</xsl:apply-templates>
							<xsl:if test="@rss and @rss!='false'">
								<xsl:apply-templates select="." mode="rssLink" />
							</xsl:if>
							<h4 class="alert-title">
								<xsl:apply-templates select="." mode="moduleLink"/>
							</h4>
						</div>
					</xsl:if>
					<xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and not(@imagePosition='above')">
						<div class="panel-image alert-image">
							<img src="{@panelImage}" alt="{@title}" class="img-responsive" />
						</div>
					</xsl:if>
					<div class="alert-body">
						<xsl:if test="not(@title!='')">
							<xsl:apply-templates select="." mode="inlinePopupOptions">
								<xsl:with-param name="class" select="'alert-body'"/>
							</xsl:apply-templates>
						</xsl:if>
						<xsl:apply-templates select="." mode="displayBrief"/>
					</div>
					<xsl:if test="@linkText!='' and @link!=''">
						<div class="alert-footer">
							<xsl:apply-templates select="." mode="moreLink">
								<xsl:with-param name="link">
									<xsl:choose>
										<xsl:when test="format-number(@link,'0')!='NaN'">
											<xsl:variable name="pageId" select="@link"/>
											<xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId or PageVersion[@vParId=$pageId]]" mode="getHref"/>
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="@link"/>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:with-param>
								<xsl:with-param name="linkText" select="@linkText"/>
								<xsl:with-param name="altText" select="@title"/>
							</xsl:apply-templates>
							<xsl:text> </xsl:text>
						</div>
					</xsl:if>
				</div>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>


	<xsl:template match="Content[starts-with(@box,'well')]" mode="moduleBox">
		<xsl:param name="id"/>
		<xsl:variable name="class">
			<xsl:text>well </xsl:text>
			<xsl:if test="@panelImage!=''">
				<xsl:text>panelImage </xsl:text>
			</xsl:if>
			<xsl:value-of select="translate(@box,' ','-')"/>
			<xsl:text> module</xsl:text>
			<!-- if no title, we may still want TL/TR for rounded boxs with no title bar,
              stled differently to a title bar. -->
			<xsl:if test="@title=''">
				<xsl:text> boxnotitle</xsl:text>
			</xsl:if>
			<xsl:text> pos-</xsl:text>
			<xsl:value-of select="@position"/>
			<xsl:if test="@modAnim and @modAnim!=''">
				<xsl:text> moduleAnimate-invisible</xsl:text>
			</xsl:if>
			<xsl:apply-templates select="." mode="hideScreens" />
			<xsl:apply-templates select="." mode="marginBelow" />
		</xsl:variable>
		<div id="mod_{@id}{$id}" class="{$class}">
			<xsl:apply-templates select="." mode="themeModuleExtras"/>
			<xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and @imagePosition='above'">
				<div class="panel-image alert-image">
					<img src="{@panelImage}" alt="{@title}" class="img-responsive" />
				</div>
			</xsl:if>
			<xsl:if test="@title!='' or @icon!='' or @uploadIcon!=''">
				<div class="well-heading">
					<xsl:apply-templates select="." mode="inlinePopupOptions">
						<xsl:with-param name="class" select="'well-heading'"/>
					</xsl:apply-templates>
					<xsl:if test="@rss and @rss!='false'">
						<xsl:apply-templates select="." mode="rssLink" />
					</xsl:if>
					<h3 class="well-title">
						<xsl:apply-templates select="." mode="moduleLink"/>
					</h3>
				</div>
			</xsl:if>
			<xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and not(@imagePosition='above')">
				<div class="panel-image well-image">
					<img src="{@panelImage}" alt="{@title}" class="img-responsive" />
				</div>
			</xsl:if>
			<div class="well-body">
				<xsl:if test="not(@title!='')">
					<xsl:apply-templates select="." mode="inlinePopupOptions">
						<xsl:with-param name="class" select="'well-body'"/>
					</xsl:apply-templates>
				</xsl:if>
				<xsl:apply-templates select="." mode="displayBrief"/>
			</div>
			<xsl:if test="@linkText!='' and @link!=''">
				<div class="well-footer">
					<xsl:apply-templates select="." mode="moreLink">
						<xsl:with-param name="link">
							<xsl:choose>
								<xsl:when test="format-number(@link,'0')!='NaN'">
									<xsl:variable name="pageId" select="@link"/>
									<xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId or PageVersion[@vParId=$pageId]]" mode="getHref"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="@link"/>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:with-param>
						<xsl:with-param name="linkText" select="@linkText"/>
						<xsl:with-param name="altText" select="@title"/>
					</xsl:apply-templates>
					<xsl:text> </xsl:text>
				</div>
			</xsl:if>
		</div>
	</xsl:template>

	<xsl:template match="Content[starts-with(@box,'jumbotron')]" mode="moduleBox">
		<xsl:param name="id"/>
		<xsl:variable name="class">
			<xsl:text>jumbotron </xsl:text>
			<xsl:text> module</xsl:text>
			<!-- if no title, we may still want TL/TR for rounded boxs with no title bar,
              stled differently to a title bar. -->
			<xsl:if test="@title=''">
				<xsl:text> boxnotitle</xsl:text>
			</xsl:if>
			<xsl:text> pos-</xsl:text>
			<xsl:value-of select="@position"/>
			<xsl:if test="@modAnim and @modAnim!=''">
				<xsl:text> moduleAnimate-invisible</xsl:text>
			</xsl:if>
			<xsl:apply-templates select="." mode="hideScreens" />
			<xsl:apply-templates select="." mode="marginBelow" />
		</xsl:variable>
		<div id="mod_{@id}{$id}" class="{$class}">
			<xsl:apply-templates select="." mode="themeModuleExtras"/>
			<xsl:if test="@title!='' or @icon!='' or @uploadIcon!=''">
				<div class="jumbotron-heading">
					<xsl:apply-templates select="." mode="inlinePopupOptions">
						<xsl:with-param name="class" select="'well-heading'"/>
					</xsl:apply-templates>
					<xsl:if test="@rss and @rss!='false'">
						<xsl:apply-templates select="." mode="rssLink" />
					</xsl:if>
					<h1 class="well-title">
						<xsl:apply-templates select="." mode="moduleTitle"/>
					</h1>
				</div>
			</xsl:if>
			<div class="jumbotron-body">
				<xsl:if test="not(@title!='')">
					<xsl:apply-templates select="." mode="inlinePopupOptions">
						<xsl:with-param name="class" select="'panel-body'"/>
					</xsl:apply-templates>
				</xsl:if>
				<xsl:apply-templates select="." mode="displayBrief"/>
			</div>
			<xsl:if test="@linkText!='' and @link!=''">
				<div class="jumbotron-footer">
					<xsl:apply-templates select="." mode="moreLink">
						<xsl:with-param name="link">
							<xsl:choose>
								<xsl:when test="format-number(@link,'0')!='NaN'">
									<xsl:variable name="pageId" select="@link"/>
									<xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId or PageVersion[@vParId=$pageId]]" mode="getHref"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="@link"/>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:with-param>
						<xsl:with-param name="linkText" select="@linkText"/>
						<xsl:with-param name="altText" select="@title"/>
					</xsl:apply-templates>
					<xsl:text> </xsl:text>
				</div>
			</xsl:if>
		</div>
	</xsl:template>

	<!-- ## Generic displayBrief   #####################################################################   -->
	<xsl:template match="Content" mode="displayBrief">
		<xsl:apply-templates select="node()" mode="cleanXhtml"/>
	</xsl:template>

	<!-- ## Generic Module displayBrief   #####################################################################   -->
	<xsl:template match="Content[@type='Module']" mode="displayBrief">
		<span class="alert">* Module type unknown *</span>
	</xsl:template>
	
	<!-- ## Generic Module   #####################################################################   -->
	<!-- Module No Carousel -->
	<xsl:template match="Content[@type='Module']" mode="displayBrief">
		<!-- Set Variables -->
		<xsl:variable name="contentType" select="@contentType" />
		<xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
		<xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
		<xsl:variable name="contentList">
			<xsl:apply-templates select="." mode="getContent">
				<xsl:with-param name="contentType" select="$contentType" />
				<xsl:with-param name="startPos" select="$startPos" />
			</xsl:apply-templates>
		</xsl:variable>
		<xsl:variable name="cropSetting">
			<xsl:choose>
				<xsl:when test="@crop='true'">
					<xsl:text>true</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>false</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="totalCount">
			<xsl:choose>
				<xsl:when test="@display='related'">
					<xsl:value-of select="count(Content[@type=$contentType])"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<div class="clearfix {@moduleType}">
			<div>
				<!--responsive columns -->
				<xsl:apply-templates select="." mode="contentColumns"/>
				<!--end responsive columns-->
				<xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
					<xsl:with-param name="sortBy" select="@sortBy"/>
					<xsl:with-param name="parentId" select="@id"/>
					<xsl:with-param name="crop" select="$cropSetting"/>
					<xsl:with-param name="linked" select="@linkArticle"/>
				</xsl:apply-templates>
				<xsl:if test="@stepCount != '0'">
					<xsl:apply-templates select="/" mode="genericStepper">
						<xsl:with-param name="articleList" select="$contentList"/>
						<xsl:with-param name="noPerPage" select="@stepCount"/>
						<xsl:with-param name="startPos" select="$startPos"/>
						<xsl:with-param name="queryStringParam" select="$queryStringParam"/>
						<xsl:with-param name="totalCount" select="$totalCount"/>
					</xsl:apply-templates>
				</xsl:if>
				<xsl:text> </xsl:text>
			</div>
		</div>
	</xsl:template>

	<!-- Module with Swiper -->
	<xsl:template match="Content[@type='Module' and @carousel='true']" mode="displayBrief">
		<xsl:variable name="contentType" select="@contentType" />
		<xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
		<xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
		<xsl:variable name="contentList">
			<xsl:apply-templates select="." mode="getContent">
				<xsl:with-param name="contentType" select="$contentType" />
				<xsl:with-param name="startPos" select="$startPos" />
			</xsl:apply-templates>
		</xsl:variable>
		<xsl:variable name="cropSetting">
			<xsl:choose>
				<xsl:when test="@crop='true'">
					<xsl:text>true</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>false</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="totalCount">
			<xsl:choose>
				<xsl:when test="@display='related'">
					<xsl:value-of select="count(Content[@type=$contentType])"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<div class="swiper-container {@moduleType} content-carousel ">
			<div class="swiper" data-autoplay="{@autoplay}" data-autoplayspeed="{@autoPlaySpeed}" data-id="{@id}" data-xscol="{@xsCol}" data-smcol="{@smCol}" data-mdcol="{@mdCol}" data-lgcol="{@lgCol}" data-xlcol="{@xlCol}" data-xxlcol="{@cols}">
				<div class="swiper-wrapper">
					<xsl:apply-templates select="." mode="contentColumns">
						<xsl:with-param name="carousel" select="@carousel"/>
					</xsl:apply-templates>
					<xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
						<xsl:with-param name="sortBy" select="@sortBy"/>
						<xsl:with-param name="class" select="'swiper-slide'"/>
						<xsl:with-param name="crop" select="$cropSetting"/>
						<xsl:with-param name="linked" select="@linkArticle"/>
					</xsl:apply-templates>
					<xsl:text> </xsl:text>
				</div>
			</div>
			<xsl:if test="@carouselBullets='true'">
				<div class="swiper-pagination" id="swiper-pagination-{@id}">
					<xsl:text> </xsl:text>
				</div>
			</xsl:if>

			<div class="swiper-button-prev" id="swiper-button-prev-{@id}">
				<xsl:text> </xsl:text>
			</div>
			<div class="swiper-button-next" id="swiper-button-next-{@id}">
				<xsl:text> </xsl:text>
			</div>
			<div class="row">
				<span>
					<xsl:text> </xsl:text>
				</span>
			</div>
		</div>
	</xsl:template>


</xsl:stylesheet>
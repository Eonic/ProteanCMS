<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">


  <xsl:import href="mailer-imports.xsl"/>
  <xsl:import href="../../email/email-stationery.xsl"/>

  <xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/>

  <xsl:template match="/">
    <xsl:apply-imports/>
  </xsl:template>

  <xsl:template match="*" mode="subject">
    <xsl:text> </xsl:text>
  </xsl:template>

  <xsl:template match="Page[@adminMode='false']" mode="adminStyle">
    <link type="text/css" rel="stylesheet" href="/ptn/features/mailer/mailer-wysiwyg.scss"/>
  </xsl:template>

  <!--   ########################   Admin Only   ############################   -->


  <!-- ACTUAL EMAIL TRANSMISSION TEMPLATE -->
  <xsl:template match="Page[not(@adminMode)]" mode="bodyBuilder">
    <body style="margin:0;padding:0;" >
      <xsl:apply-templates select="." mode="emailBody"/>
    </body>
  </xsl:template>

  <xsl:template match="Page[@adminMode='false']" mode="bodyBuilder">
    <body style="margin:0;padding:0;margin-top:106px!important;" id="pg{@id}">
      <div class="ptn-edit">
        <xsl:apply-templates select="AdminMenu"/>
      </div>
      <div id="dragableModules" class="Site">
		  <div>
        <xsl:apply-templates select="." mode="emailBody"/>
		  </div>
      </div>
      <div class="ptn-edit">
        <xsl:apply-templates select="." mode="adminFooter"/>
      </div>
		
		
		<xsl:apply-templates select="." mode="footerJs"/>
    </body>
  </xsl:template>

	<!-- Javascripts that can be brought in in the footer of the HTML document, e.g. asynchronous scripts -->
	<xsl:template match="Page" mode="footerJs">
		<xsl:apply-templates select="." mode="js"/>
	</xsl:template>


	<xsl:template match="Page[@adminMode='false']" mode="siteJs">

		<xsl:call-template name="bundle-js">
			<xsl:with-param name="comma-separated-files">
				<xsl:apply-templates select="." mode="commonJsFiles" />
				<xsl:text>~/ptn/core/vue/vue.min.js,</xsl:text>
				<xsl:text>~/ptn/core/vue/axios.min.js,</xsl:text>
				<xsl:text>~/ptn/core/vue/polyfill.js,</xsl:text>
				<xsl:text>~/ptn/core/vue/protean-vue.js,</xsl:text>
				<xsl:text>~/ptn/libs/tinymce/jquery.tinymce.min.js,</xsl:text>
				<!-- Not sure where we are using this please add note if needing to re-add -->
				<!-- <xsl:text>~/ptn/admin/treeview/jquery.treeview.js,</xsl:text> -->
				<xsl:text>~/ptn/admin/treeview/ajaxtreeview.js,</xsl:text>
				<xsl:text>~/ptn/libs/jqueryui/jquery-ui.js,</xsl:text>
				<xsl:text>~/ptn/libs/fancyapps/ui/dist/fancybox.umd.min.js,</xsl:text>
				<xsl:text>~/ptn/libs/jquery.lazy/jquery.lazy.min.js,</xsl:text>
				<xsl:text>~/ptn/admin/admin.js</xsl:text>
			</xsl:with-param>
			<xsl:with-param name="bundle-path">
				<xsl:text>~/Bundles/Admin</xsl:text>
			</xsl:with-param>
		</xsl:call-template>
		<xsl:apply-templates select="." mode="siteAdminJs"/>
		<xsl:apply-templates select="." mode="LayoutAdminJs"/>
	</xsl:template>

  <xsl:template match="Page[@previewMode]" mode="bodyBuilder">
    <body style="margin:0;padding:0;" class="Site">
      <xsl:apply-templates select="PreviewMenu"/>
      <xsl:apply-templates select="." mode="emailBody"/>
      <xsl:apply-templates select="." mode="emailStyle"/>
    </body>
  </xsl:template>

  <!--   ########################   Main Email Layout   ############################   -->

  <xsl:template match="Page" mode="bodyLayout">
    <xsl:apply-templates select="." mode="mainLayout"/>
  </xsl:template>

	<xsl:template match="Page" mode="adminBreadcrumb">
		
	</xsl:template>

	<xsl:template match="Page" mode="addModule">
		<xsl:param name="text"/>
		<xsl:param name="position"/>
		<xsl:param name="class"/>
		<xsl:param name="width"/>
		<xsl:param name="auto-col"/>
		<xsl:param name="module-type"/>

		<xsl:choose>
			<xsl:when test="$position='header' or $position='footer' or $position='custom' or ($position='column1' and @layout='Modules_1_column')">
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
					<section class="">
						<xsl:attribute name="class">
							<xsl:text>wrapper-sm </xsl:text>
							<xsl:if test="@background!='false'">
								<xsl:value-of select="@background"/>
								<xsl:text> </xsl:text>
							</xsl:if>
							<xsl:if test="@bgContentPosition='center'">
								<xsl:text> bg-content-</xsl:text>
								<xsl:value-of select="@bgContentPosition"/>
								<xsl:text> </xsl:text>
							</xsl:if>
							<xsl:text> bg-wrapper-</xsl:text>
							<xsl:value-of select="@id"/>
							<xsl:text> </xsl:text>
							<xsl:apply-templates select="." mode="hideScreens" />
							<xsl:if test="@marginBelow='false'">
								<xsl:text> mb-0 </xsl:text>
							</xsl:if>
							<xsl:if test="@marginBelow!='false' and @marginBelow!='true' and @marginBelow!='' and @marginBelow">
								<xsl:text> </xsl:text>
								<xsl:value-of select="@marginBelow"/>
								<xsl:text> </xsl:text>
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
							<xsl:if test="@fullWidth='narrow'">
								<xsl:text> narrow-container </xsl:text>
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
						<xsl:if test="@backgroundImage='' or not(@backgroundImage)">
							<xsl:attribute name="style">
								<xsl:if test="@minHeight!=''">
									<xsl:text>min-height:</xsl:text>
									<xsl:value-of select="@minHeight"/>
									<xsl:text>px!important;</xsl:text>
								</xsl:if>
								<!--<xsl:if test="@padding-top and @padding-top!=''">
                  <xsl:text>padding-top:</xsl:text>
                  <xsl:value-of select="@padding-top"/>
                  <xsl:text>;</xsl:text>
                </xsl:if>
                <xsl:if test="@padding-bottom and @padding-bottom!=''">
                  <xsl:text>padding-bottom:</xsl:text>
                  <xsl:value-of select="@padding-bottom"/>
                </xsl:if>-->
							</xsl:attribute>
							<xsl:if test="(@padding-top and @padding-top!='') or (@padding-top-xs and @padding-top-xs!='') or (@padding-bottom and @padding-bottom!='') or (@padding-bottom-xs and @padding-bottom-xs!='')  ">
								<style>
									<xsl:text>.bg-wrapper-</xsl:text>
									<xsl:value-of select="@id"/>{
									<xsl:if test="@padding-top-xs and @padding-top-xs!=''">
										<xsl:text>padding-top:</xsl:text>
										<xsl:value-of select="@padding-top-xs"/>
										<xsl:text>!important;</xsl:text>
									</xsl:if>
									<xsl:if test="@padding-bottom-xs and @padding-bottom-xs!=''">
										<xsl:text>padding-bottom:</xsl:text>
										<xsl:value-of select="@padding-bottom-xs"/>
										<xsl:text>!important;</xsl:text>
									</xsl:if>
									}
									@media(min-width:768px){
									<xsl:text>.bg-wrapper-</xsl:text>
									<xsl:value-of select="@id"/>{
									<xsl:if test="@padding-top and @padding-top!=''">
										<xsl:text>padding-top:</xsl:text>
										<xsl:value-of select="@padding-top"/>
										<xsl:text>!important;</xsl:text>
									</xsl:if>
									<xsl:if test="@padding-bottom and @padding-bottom!=''">
										<xsl:text>padding-bottom:</xsl:text>
										<xsl:value-of select="@padding-bottom"/>
										<xsl:text>!important;</xsl:text>
									</xsl:if>}}
								</style>
							</xsl:if>
						</xsl:if>
						<xsl:if test="@backgroundImage!=''">
							<style>
								<xsl:text>.bg-wrapper-</xsl:text>
								<xsl:value-of select="@id"/>{
								<xsl:text>background-image: url('</xsl:text>
								<xsl:value-of select="@backgroundImage"/>
								<xsl:text>');</xsl:text>
								<xsl:if test="@backgroundPosition and @backgroundPosition!=''">
									<xsl:text>background-position:</xsl:text>
									<xsl:value-of select="@backgroundPosition"/>
									<xsl:text>;</xsl:text>
								</xsl:if>

								<xsl:if test="@minHeightxs!=''">
									<xsl:text>min-height:</xsl:text>
									<xsl:value-of select="@minHeightxs"/>
									<xsl:text>px!important;</xsl:text>
									<xsl:text>height:</xsl:text>
									<xsl:value-of select="@minHeightxs"/>
									<xsl:text>px!important;</xsl:text>
								</xsl:if>
								<xsl:if test="@padding-top-xs and @padding-top-xs!=''">
									<xsl:text>padding-top:</xsl:text>
									<xsl:value-of select="@padding-top-xs"/>
									<xsl:text>!important;</xsl:text>
								</xsl:if>
								<xsl:if test="@padding-bottom-xs and @padding-bottom-xs!=''">
									<xsl:text>padding-bottom:</xsl:text>
									<xsl:value-of select="@padding-bottom-xs"/>
									<xsl:text>!important;</xsl:text>
								</xsl:if>
								}
								@media(min-width:768px){
								<xsl:text>.bg-wrapper-</xsl:text>

								<xsl:value-of select="@id"/>{
								<xsl:if test="@minHeight!=''">
									<xsl:text>min-height:</xsl:text>
									<xsl:value-of select="@minHeight"/>
									<xsl:text>px!important;</xsl:text>
									<xsl:text>height:</xsl:text>
									<xsl:value-of select="@minHeight"/>
									<xsl:text>px!important;</xsl:text>
								</xsl:if>
								<xsl:if test="@padding-top and @padding-top!=''">
									<xsl:text>padding-top:</xsl:text>
									<xsl:value-of select="@padding-top"/>
									<xsl:text>!important;</xsl:text>
								</xsl:if>
								<xsl:if test="@padding-bottom and @padding-bottom!=''">
									<xsl:text>padding-bottom:</xsl:text>
									<xsl:value-of select="@padding-bottom"/>
									<xsl:text>!important;</xsl:text>
								</xsl:if>
								}}
							</style>
							<xsl:if test="@data-stellar-background-ratio!='10'">

								<div class="parallax"
								   data-parallax-image="{$backgroundResized}"  data-parallax-image-webp="{$backgroundResized-webp}"
								   data-parallax-image-xs="{$backgroundResized-xs}"  data-parallax-image-xs-webp="{$backgroundResized-xs-webp}"
								   data-parallax-image-sm="{$backgroundResized-sm}"  data-parallax-image-sm-webp="{$backgroundResized-sm-webp}"
								   data-parallax-image-md="{$backgroundResized-md}"  data-parallax-image-md-webp="{$backgroundResized-md-webp}"
								   data-parallax-image-lg="{$backgroundResized-lg}"  data-parallax-image-lg-webp="{$backgroundResized-lg-webp}"
								   data-parallax-image-xxl="{@backgroundImage}">
									<xsl:text> </xsl:text>
								</div>

							</xsl:if>
							<!--<xsl:choose>
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
							</xsl:choose>-->
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
							<xsl:when test="@fullWidth='true-w-padding'">
								<div class="fullwidthContainer fullwidth-w-padding">
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
								<xsl:if test="$module-type='AutoColumn' or /Page/Contents/Content[@colType='auto']">true</xsl:if>
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

</xsl:stylesheet>
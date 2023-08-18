<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

	<!--   ################   Testimonials   ###############   -->

	<!-- Testimonial Module -->

	<!-- wrapping code is generic from layout.xsl with mode displayBrief, override here to customise-->

	<xsl:template match="Content[@type='Module' and @moduleType='TestimonialList']" mode="themeModuleExtras">
		<!-- this is empty because we want this on individual listing panels not the containing module-->
	</xsl:template>

	<xsl:template match="Content[@type='Module' and @moduleType='TestimonialList']" mode="themeModuleClassExtras">
		<!-- this is empty because we want this on individual listing panels not the containing module-->
	</xsl:template>
	<!-- Testimonial Brief -->

	<xsl:template match="Content[@type='Testimonial']" mode="displayBrief">
		<xsl:param name="sortBy"/>
		<xsl:param name="crop"/>
		<xsl:param name="class"/>
		<xsl:param name="parentId"/>
		<xsl:param name="linked"/>
		<xsl:variable name="parentURL">
			<xsl:apply-templates select="." mode="getHref"/>
		</xsl:variable>
		<xsl:variable name="classValues">
			<xsl:text>listItem newsarticle </xsl:text>
			<xsl:if test="$linked='true'">
				<xsl:text> linked-listItem </xsl:text>
			</xsl:if>
			<xsl:value-of select="$class"/>
			<xsl:text> </xsl:text>
			<xsl:apply-templates select="." mode="themeModuleClassExtrasListItem">
				<xsl:with-param name="parentId" select="$parentId"/>
			</xsl:apply-templates>
		</xsl:variable>
		<xsl:variable name="cropSetting">
			<xsl:choose>
				<xsl:when test="$crop='true'">
					<xsl:value-of select="true()"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="false()"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:if test="not(@noLink='true')">
			<!-- Modal -->
			<div class="modal fade" id="quoteModal{@id}" tabindex="-1" aria-labelledby="exampleModalLabel" aria-hidden="true">
				<div class="modal-dialog">
					<div class="modal-content">
						<div class="modal-header">
							<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
						</div>
						<div class="modal-body">

							<xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
						</div>
					</div>
				</div>
			</div>
		</xsl:if>
		<div class="{$classValues}">
			<xsl:apply-templates select="." mode="themeModuleExtrasListItem">
				<xsl:with-param name="parentId" select="$parentId"/>
				<xsl:with-param name="pos" select="position()"/>
			</xsl:apply-templates>
			<xsl:apply-templates select="." mode="inlinePopupOptions">
				<xsl:with-param name="class" select="concat($classValues,' ',$class)"/>
				<xsl:with-param name="sortBy" select="$sortBy"/>
			</xsl:apply-templates>
			<div class="lIinner">
				<xsl:if test="Images/img/@src!=''">
					<xsl:attribute name="class">lIinner quote-with-image</xsl:attribute>
					<!--<a href="{$parentURL}">
						<xsl:attribute name="title">
							<xsl:call-template name="term2042" />
							<xsl:text> - </xsl:text>
							<xsl:value-of select="SourceName/node()"/>
						</xsl:attribute>
						<xsl:apply-templates select="." mode="displayThumbnail">
							<xsl:with-param name="crop" select="$cropSetting" />
							<xsl:with-param name="class">list-image</xsl:with-param>
						</xsl:apply-templates>
					</a>-->
					<span class="detail-img ">
						<xsl:apply-templates select="." mode="displayThumbnail">
							<xsl:with-param name="crop" select="$cropSetting" />
							<xsl:with-param name="class">list-image</xsl:with-param>
						</xsl:apply-templates>
					</span>
				</xsl:if>
				<div class="summary">
					<xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
					<xsl:if test="not(@noLink='true')">
						<div class="entryFooter">
							<!--<xsl:apply-templates select="." mode="displayTags"/>
						<xsl:apply-templates select="." mode="moreLink">
							<xsl:with-param name="link" select="$parentURL"/>
							<xsl:with-param name="altText">
								-->
							<!--Testimonial from-->
							<!--
								<xsl:call-template name="term2041" />
								<xsl:text>&#160;</xsl:text>
								<xsl:value-of select="SourceCompany/node()"/>
							</xsl:with-param>
						</xsl:apply-templates>
						<xsl:text> </xsl:text>-->
							<button type="button" class="btn btn-custom" data-bs-toggle="modal" data-bs-target="#quoteModal{@id}">
								<xsl:if test="$linked='true'">
									<xsl:attribute name="class">btn btn-custom stretched-link</xsl:attribute>
								</xsl:if>
								<xsl:call-template name="term2042" />
								<span class="visually-hidden">
									<xsl:call-template name="term2041" />
									<xsl:text> </xsl:text>
									<xsl:value-of select="SourceName/node()"/>
								</span>
							</button>
						</div>
					</xsl:if>
				</div>
				<div class="quote-credit">
					<div class="quote-name">
						<xsl:value-of select="SourceName/node()"/>
					</div>
					<xsl:if test="SourceCompany/node()!=''">
						<div class="quote-company">
							<xsl:apply-templates select="SourceCompany" mode="displayBrief"/>
						</div>
					</xsl:if>
				</div>

			</div>
		</div>
	</xsl:template>



	<!-- Testimonial Detail -->
	<xsl:template match="Content[@type='Testimonial']" mode="ContentDetail">
		<xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
		<div class="detail testimonial">
			<xsl:apply-templates select="." mode="inlinePopupOptions">
				<xsl:with-param name="class" select="'detail testimonial'"/>
			</xsl:apply-templates>
			<xsl:apply-templates select="." mode="displayDetailImage"/>
			<h2 class="entry-title content-title">
				<xsl:value-of select="SourceCompany/node()"/>
			</h2>
			<p class="lead">
				<xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
			</p>
			<div class="entry-content">
				<xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
			</div>
			<div class="source">
				<p>
					<xsl:if test="SourceName/node()!=''">
						<span class="sourceName">
							<xsl:apply-templates select="SourceName/node()" mode="displayBrief"/>
						</span>
					</xsl:if>
					<xsl:if test="SourceCompany/node()!=''">
						<br />
						<xsl:apply-templates select="SourceCompany/node()" mode="displayBrief"/>
					</xsl:if>
				</p>
			</div>
			<div class="entryFooter">
				<div class="tags">
					<xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
					<xsl:text> </xsl:text>
				</div>
				<xsl:apply-templates select="." mode="backLink">
					<xsl:with-param name="link" select="$thisURL"/>
					<xsl:with-param name="altText">
						<!--click here to return to the testimonial list-->
						<xsl:call-template name="term2043" />
					</xsl:with-param>
				</xsl:apply-templates>
			</div>
		</div>
	</xsl:template>

</xsl:stylesheet>
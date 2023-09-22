<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

	<xsl:template match="Content[@type='LibraryImage']" mode="displayBrief">
		<xsl:param name="sortBy"/>
		<xsl:param name="crop" />
		<xsl:param name="no-stretch"/>
		<xsl:param name="lightbox"/>
		<xsl:param name="showTitle"/>
		<xsl:param name="alignment"/>
		<xsl:param name="alignmentV"/>
		<xsl:param name="class"/>
		<xsl:variable name="cropSetting">
			<xsl:choose>
				<xsl:when test="$crop='false'">
					<xsl:value-of select="false()"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="true()"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="no-stretchSetting">
			<xsl:choose>
				<xsl:when test="$no-stretch='false'">
					<xsl:value-of select="false()"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="true()"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="lg-max-width">
			<xsl:apply-templates select="." mode="getFullSizeWidth"/>
		</xsl:variable>
		<xsl:variable name="lg-max-height">
			<xsl:apply-templates select="." mode="getFullSizeHeight"/>
		</xsl:variable>
		<xsl:variable name="fullSize">
			<xsl:call-template name="resize-image">
				<xsl:with-param name="path" select="Images/img[@class='detail']/@src"/>
				<xsl:with-param name="max-width" select="$lg-max-width"/>
				<xsl:with-param name="max-height" select="$lg-max-height"/>
				<xsl:with-param name="file-prefix">
					<xsl:text>~lg-</xsl:text>
					<xsl:value-of select="$lg-max-width"/>
					<xsl:text>x</xsl:text>
					<xsl:value-of select="$lg-max-width"/>
					<xsl:text>/~lg-</xsl:text>
				</xsl:with-param>
				<xsl:with-param name="file-suffix" select="''"/>
				<xsl:with-param name="quality" select="100"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="lgImgSrc">
			<xsl:choose>
				<xsl:when test="Images/img[@class='detail']/@src != ''">
					<xsl:value-of select="Images/img[@class='detail']/@src"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="Images/img[@class='display']/@src"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="preURL" select="substring(Url,1,3)" />
		<xsl:variable name="url" select="Url/node()" />
		<xsl:variable name="linkURL">
			<xsl:choose>
				<xsl:when test="format-number($url,'0')!='NaN'">
					<xsl:apply-templates select="$page/descendant-or-self::MenuItem[@id=$url]" mode="getHref"/>
				</xsl:when>
				<xsl:when test="@name='link in page'">
					<xsl:value-of select="@InPageID"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:if test="$preURL='www' or $preURL='WWW'">
						<xsl:text>http://</xsl:text>
					</xsl:if>
					<xsl:value-of select="$url"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<div class="grid-item {$class}">
			<xsl:apply-templates select="." mode="inlinePopupOptions">
				<!--<xsl:with-param name="class" select="'grid-item '"/>-->
				<xsl:with-param name="class" select="concat('grid-item  ',$class)"/>
				<xsl:with-param name="sortBy" select="$sortBy"/>
			</xsl:apply-templates>
			<!--<xsl:choose>
				<xsl:when test="$lightbox='false'">-->
					<div class="thumbnail-wrapper">
						<div class="thumbnail">
							<!--<xsl:attribute name="class">
								<xsl:text>thumbnail text-</xsl:text>
								<xsl:value-of select="$alignment"/>
								<xsl:text> d-flex h-100 flex-column</xsl:text>
							</xsl:attribute>-->
							<!--<xsl:attribute name="class">
								<xsl:text>d-flex justify-content-</xsl:text>
								<xsl:value-of select="$alignment"/>
								<xsl:text> align-items-</xsl:text>
								<xsl:value-of select="$alignmentV"/>
							</xsl:attribute>-->


							<xsl:choose>
								<xsl:when test="$url!=''">
									<div class="gallery-img-wrapper">
										<xsl:if test="@maxWidth!=''">
											<xsl:attribute name="style">
												<xsl:text>max-width:</xsl:text>
												<xsl:value-of select="@maxWidth"/>
												<xsl:text>px;</xsl:text>
											</xsl:attribute>
										</xsl:if>
										<a href="{$linkURL}" title="{Name}">
											<xsl:apply-templates select="." mode="displayThumbnail">
												<xsl:with-param name="crop" select="$cropSetting" />
												<xsl:with-param name="class" select="'img-responsive'" />
												<xsl:with-param name="style" select="'overflow:hidden;'" />
												<xsl:with-param name="width" select="$lg-max-width"/>
												<xsl:with-param name="height" select="$lg-max-height"/>
												<xsl:with-param name="no-stretch" select="$no-stretchSetting"/>												
											</xsl:apply-templates>
										</a>
									</div>
									<xsl:if test="(Title/node()!='' or Body/node()!='') and not($showTitle='false')">
										<div class="caption">
											<h4>

												<a href="{$linkURL}" title="{Name}">
													<xsl:value-of select="Title/node()"/>
												</a>
											</h4>
											<xsl:apply-templates select="Body/node()" mode="cleanXhtml" />
										</div>
									</xsl:if>
								</xsl:when>
								<xsl:otherwise>
									<div class="gallery-img-wrapper">
										<xsl:if test="@maxWidth!=''">
											<xsl:attribute name="style">
												<xsl:text>max-width:</xsl:text>
												<xsl:value-of select="@maxWidth"/>
												<xsl:text>px;</xsl:text>
											</xsl:attribute>
										</xsl:if>
										
										<xsl:apply-templates select="." mode="displayThumbnail">
											<xsl:with-param name="crop" select="$cropSetting" />
											<xsl:with-param name="class" select="'img-responsive'" />
											<xsl:with-param name="style" select="'overflow:hidden;'" />
											<xsl:with-param name="width" select="$lg-max-width"/>
											<xsl:with-param name="height" select="$lg-max-height"/>
											<xsl:with-param name="no-stretch" select="$no-stretchSetting"/>
										</xsl:apply-templates>
									</div>
									<xsl:if test="(Title/node()!='' or Body/node()!='') and not($showTitle='false')">
										<div class="caption">
											<h4>
												<xsl:value-of select="Title/node()"/>
											</h4>
											<xsl:apply-templates select="Body/node()" mode="cleanXhtml" />
										</div>
									</xsl:if>
								</xsl:otherwise>
							</xsl:choose>

						</div>
					</div>
				<!--</xsl:when>
				<xsl:otherwise>
					<a href="{$fullSize}" title="{Title/node()} - {Body/node()}" class="responsive-lightbox grid-link">
						<div class="thumbnail">
							<xsl:apply-templates select="." mode="displayThumbnail">
								<xsl:with-param name="crop" select="$cropSetting" />
								<xsl:with-param name="class" select="'img-responsive'" />
								<xsl:with-param name="style" select="'overflow:hidden;'" />
								<xsl:with-param name="width" select="$lg-max-width"/>
								<xsl:with-param name="height" select="$lg-max-height"/>
							</xsl:apply-templates>
							<xsl:if test="Title/node()!='' or Body/node()!=''">
								<div class="caption">
									<h4>
										<xsl:value-of select="Title/node()"/>
									</h4>
									<xsl:apply-templates select="Body/node()" mode="cleanXhtml" />
								</div>
							</xsl:if>
						</div>
					</a>
				</xsl:otherwise>
			</xsl:choose>-->

		</div>
	</xsl:template>

</xsl:stylesheet>
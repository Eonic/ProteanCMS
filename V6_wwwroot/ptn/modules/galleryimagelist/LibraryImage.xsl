<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

	<xsl:template match="Content[@type='LibraryImage']" mode="displayBrief">
		<xsl:param name="sortBy"/>
		<xsl:param name="crop"/>
		<xsl:param name="lightbox"/>
		<xsl:param name="showTitle"/>
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
		<xsl:choose>
			<xsl:when test="$page[@cssFramework='bs3']">
				<div class="grid-item">
					<xsl:apply-templates select="." mode="inlinePopupOptions">
						<xsl:with-param name="class" select="'grid-item '"/>
						<xsl:with-param name="sortBy" select="$sortBy"/>
					</xsl:apply-templates>
					<xsl:choose>
						<xsl:when test="$lightbox='false'">
							<div class="thumbnail-wrapper">
								<div class="thumbnail">
									<xsl:apply-templates select="." mode="displayThumbnail">
										<xsl:with-param name="crop" select="$cropSetting" />
										<xsl:with-param name="class" select="'img-responsive'" />
										<xsl:with-param name="style" select="'overflow:hidden;'" />
										<xsl:with-param name="width" select="$lg-max-width"/>
										<xsl:with-param name="height" select="$lg-max-height"/>
									</xsl:apply-templates>
									<xsl:if test="(Title/node()!='' or Body/node()!='') and not($showTitle='false')">
										<div class="caption">
											<h4>
												<xsl:value-of select="Title/node()"/>
											</h4>
											<xsl:apply-templates select="Body/node()" mode="cleanXhtml" />
										</div>
									</xsl:if>
								</div>
							</div>
						</xsl:when>
						<xsl:otherwise>
							<a href="{$fullSize}" title="{Title/node()} - {Body/node()}" class="responsive-lightbox">
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
					</xsl:choose>

				</div>
			</xsl:when>
			<xsl:otherwise>

				<div class="listItem">
					<xsl:apply-templates select="." mode="inlinePopupOptions">
						<xsl:with-param name="class" select="'listItem libraryimage'"/>
						<xsl:with-param name="sortBy" select="$sortBy"/>
					</xsl:apply-templates>
					<div class="lIinner">
						<h3>
							<xsl:choose>
								<xsl:when test="$fullSize != ''">
									<a href="{$fullSize}" title="{Title/node()}" class="lightbox">
										<xsl:attribute name="title">
											<xsl:value-of select="Title/node()"/>
											<xsl:if test="Author/node()">
												<xsl:text> - </xsl:text>
												<xsl:value-of select="Author/node()"/>
											</xsl:if>
										</xsl:attribute>
										<xsl:apply-templates select="." mode="displayThumbnail">
											<xsl:with-param name="crop" select="true()" />
										</xsl:apply-templates>
									</a>
								</xsl:when>
								<xsl:otherwise>
									<a href="{$fullSize}" title="{Title/node()} - {Body/node()}">
										<xsl:apply-templates select="." mode="displayThumbnail"/>
									</a>
								</xsl:otherwise>

							</xsl:choose>
							<xsl:if test="Title/node()!=''">
								<div class="caption">
									<a href="{$fullSize}" title="{Title/node()} - {Body/node()}" class="title">
										<xsl:value-of select="Title/node()"/>
									</a>
								</div>
							</xsl:if>
						</h3>
					</div>
				</div>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
  
</xsl:stylesheet>
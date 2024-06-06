<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
	<!--   ################   Contact   ###############   -->

	<!-- Contact Brief -->
	<xsl:template match="Content[@type='Contact']" mode="displayBrief">
		<xsl:param name="sortBy"/>
		<xsl:param name="crop"/>
		<xsl:param name="class"/>
		<xsl:param name="parentId"/>
		<xsl:param name="linked"/>
		<xsl:param name="heading"/>
		<xsl:param name="title"/>
		<!-- contactBrief -->
		<xsl:variable name="parentURL">
			<xsl:apply-templates select="self::Content" mode="getHref">
				<xsl:with-param name="parId" select="@parId"/>
			</xsl:apply-templates>
		</xsl:variable>
		<xsl:variable name="preURL" select="substring(Website,1,3)" />
		<xsl:variable name="linkURL">
			<xsl:choose>
				<xsl:when test="$preURL='www' or $preURL='WWW'">
					<xsl:text>http://</xsl:text>
					<xsl:value-of select="Url"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="Url"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="classValues">
			<xsl:text>listItem contact </xsl:text>
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
					<xsl:choose>
						<xsl:when test="@noLink='true'">
							<xsl:apply-templates select="." mode="displayThumbnail">
								<xsl:with-param name="crop" select="$cropSetting" />
								<xsl:with-param name="class">list-image</xsl:with-param>
							</xsl:apply-templates>
						</xsl:when>
						<xsl:otherwise>
							<a href="{$parentURL}" title="View more details on {GivenName/node()} {Surname/node()}" class="list-image-link">
								<xsl:apply-templates select="." mode="displayThumbnail">
									<xsl:with-param name="crop" select="$cropSetting" />
									<xsl:with-param name="no-strecth" select="false()" />
									<xsl:with-param name="class">list-image</xsl:with-param>
								</xsl:apply-templates>
							</a>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:if>
				<div class="media-inner">
					<xsl:choose>
						<xsl:when test="$title!='' and $heading!=''">
							<xsl:variable name="headingNo" select="substring-after($heading,'h')"/>
							<xsl:variable name="headingNoPlus" select="$headingNo + 1"/>
							<xsl:variable name="listHeading">
								<xsl:text>h</xsl:text>
								<xsl:value-of select="$headingNoPlus"/>
							</xsl:variable>
							<xsl:element name="{$listHeading}">
								<xsl:attribute name="class">
									<xsl:text>title</xsl:text>
								</xsl:attribute>
								<a href="{$parentURL}">
									<xsl:apply-templates select="." mode="getDisplayName"/>
								</a>
							</xsl:element>
						</xsl:when>
						<xsl:otherwise>
							<xsl:choose>
								<xsl:when test="$heading=''">
									<xsl:choose>
										<xsl:when test="@noLink='true'">
											<h3 class="title">
												<xsl:apply-templates select="." mode="getDisplayName"/>
											</h3>
										</xsl:when>
										<xsl:otherwise>
											<h3 class="title">
												<a href="{$parentURL}">
													<xsl:apply-templates select="." mode="getDisplayName"/>
												</a>
											</h3>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>
									<xsl:element name="{$heading}">
										<xsl:attribute name="class">
											<xsl:text>title</xsl:text>
										</xsl:attribute>
										<xsl:choose>
											<xsl:when test="@noLink='true'">
												<xsl:apply-templates select="." mode="getDisplayName"/>
											</xsl:when>
											<xsl:otherwise>
												<a href="{$parentURL}">
													<xsl:apply-templates select="." mode="getDisplayName"/>
												</a>
											</xsl:otherwise>
										</xsl:choose>
									</xsl:element>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:if test="Title/node()!=''">
						<h4 class="job-title">
							<xsl:apply-templates select="Title" mode="displayBrief"/>
						</h4>
					</xsl:if>

					<dl class="clearfix dl-horizontal">
						<xsl:if test="Telephone/node()!=''">
							<dt class="tel">
								<xsl:call-template name="term2007" />
								<xsl:text>: </xsl:text>
							</dt>
							<dd class="tel">
								<xsl:apply-templates select="Telephone" mode="displayBrief"/>
							</dd>

						</xsl:if>
						<xsl:if test="Mobile/node()!=''">
							<dt class="mobile">
								<xsl:call-template name="term2080" />
								<xsl:text>: </xsl:text>
							</dt>
							<dd>
								<xsl:apply-templates select="Mobile" mode="displayBrief"/>
							</dd>
						</xsl:if>
						<xsl:if test="Email/node()!=''">
							<dt class="email">
								<xsl:call-template name="term2009" />
								<xsl:text>: </xsl:text>
							</dt>
							<dd>
								<a href="mailto:{Email/node()}">
									<xsl:apply-templates select="Email" mode="displayBrief"/>
								</a>
							</dd>
						</xsl:if>
					</dl>

					<xsl:if test="Profile/node()!=''">
						<xsl:apply-templates select="Profile/node()" mode="cleanXhtml"/>
					</xsl:if>
					<xsl:if test="not(@noLink='true')">
						<div class="entryFooter">
							
							<xsl:apply-templates select="." mode="moreLink">
								<xsl:with-param name="link" select="$parentURL"/>
								<xsl:with-param name="linktext" select="@readmoretext"/>
								<xsl:with-param name="stretchLink" select="$linked"/>
								<xsl:with-param name="altText">
									<xsl:value-of select="GivenName/node()"/>
									<xsl:text> </xsl:text>
									<xsl:value-of select="Surname/node()"/>
								</xsl:with-param>
							</xsl:apply-templates>
							<xsl:text> </xsl:text>
						</div>
					</xsl:if>
				</div>
			</div>
		</div>
	</xsl:template>

	<!-- Contact Brief -->
	<xsl:template match="Content[@type='Contact']" mode="displayAuthor">
		<xsl:param name="sortBy"/>
		<!-- contactBrief -->
		<xsl:variable name="parentURL">
			<xsl:apply-templates select="self::Content" mode="getHref">
				<xsl:with-param name="parId" select="@parId"/>
			</xsl:apply-templates>
		</xsl:variable>
		<xsl:variable name="preURL" select="substring(Website,1,3)" />
		<xsl:variable name="linkURL">
			<xsl:choose>
				<xsl:when test="$preURL='www' or $preURL='WWW'">
					<xsl:text>http://</xsl:text>
					<xsl:value-of select="Url"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="Url"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<div class="author">
			<xsl:if test="Images/img/@src!=''">
				<a href="{$parentURL}" rel="author" title="click here to view more details on {GivenName/node()} {Surname/node()}">
					<xsl:apply-templates select="." mode="displayThumbnail">
						<xsl:with-param name="width">76</xsl:with-param>
						<xsl:with-param name="height">76</xsl:with-param>
						<xsl:with-param name="crop" select="true()"/>
					</xsl:apply-templates>
				</a>
			</xsl:if>
			<div class="author-text">
				<p class="author-name">
					<xsl:text>by </xsl:text>
					<a href="{$parentURL}" rel="author">
						<xsl:attribute name="title">
							<xsl:call-template name="term2072" />
							<xsl:text>&#160;</xsl:text>
							<xsl:value-of select="GivenName/node()"/>
							<xsl:text>&#160;</xsl:text>
							<xsl:value-of select="Surname/node()"/>
						</xsl:attribute>
						<xsl:apply-templates select="." mode="getDisplayName"/>
					</a>
				</p>
				<xsl:if test="Title/node()!=''">
					<p class="author-role">
						<xsl:apply-templates select="Title" mode="displayBrief"/>
						<xsl:if test="Company/node()!=''">
							<xsl:text> - </xsl:text>
							<xsl:apply-templates select="Company" mode="displayBrief"/>
						</xsl:if>
					</p>
				</xsl:if>
			</div>
		</div>
	</xsl:template>

	<!-- Contact Brief -->
	<xsl:template match="Content[@type='Contact']" mode="displayContributor">
		<xsl:param name="sortBy"/>
		<!-- contactBrief -->
		<xsl:variable name="parentURL">
			<xsl:apply-templates select="self::Content" mode="getHref">
				<xsl:with-param name="parId" select="@parId"/>
			</xsl:apply-templates>
		</xsl:variable>
		<xsl:variable name="preURL" select="substring(Website,1,3)" />
		<xsl:variable name="linkURL">
			<xsl:choose>
				<xsl:when test="$preURL='www' or $preURL='WWW'">
					<xsl:text>http://</xsl:text>
					<xsl:value-of select="Url"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="Url"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<div class="contributor row">
			<xsl:if test="Images/img/@src!=''">
				<a href="{$parentURL}" rel="author" title="click here to view more details on {GivenName/node()} {Surname/node()}" class="col col-xs-2 col-md-3">
					<xsl:apply-templates select="." mode="displayThumbnail">
						<xsl:with-param name="width">220</xsl:with-param>
						<xsl:with-param name="height">220</xsl:with-param>
						<xsl:with-param name="crop" select="true()"/>
					</xsl:apply-templates>
				</a>

			</xsl:if>
			<div class="col col-xs-10 col-md-9">
				<h5 class="title">
					<a href="{$parentURL}" rel="author">
						<xsl:attribute name="title">
							<xsl:call-template name="term2072" />
							<xsl:text>&#160;</xsl:text>
							<xsl:value-of select="GivenName/node()"/>
							<xsl:text>&#160;</xsl:text>
							<xsl:value-of select="Surname/node()"/>
						</xsl:attribute>
						<xsl:apply-templates select="." mode="getDisplayName"/>
					</a>
				</h5>
				<xsl:if test="Title/node()!=''">
					<h6 class="title">
						<xsl:apply-templates select="Title" mode="displayBrief"/>
						<xsl:if test="Company/node()!=''">
							<xsl:text> - </xsl:text>
							<xsl:apply-templates select="Company" mode="displayBrief"/>
						</xsl:if>
					</h6>
				</xsl:if>
				<xsl:if test="Profile/node()!=''">
					<p>
						<xsl:apply-templates select="Profile/node()" mode="cleanXhtml"/>
					</p>
				</xsl:if>
				<a href="{$parentURL}" class="btn btn-sm btn-default">
					<xsl:choose>
						<xsl:when test="@morelinktext!=''">
							<xsl:value-of select="@morelinktext"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:text>More about </xsl:text>
							<xsl:value-of select="GivenName/node()"/>
						</xsl:otherwise>
					</xsl:choose>
					
				</a>
			</div>
		</div>
	</xsl:template>

	<!-- Contact Brief -->
	<xsl:template match="Content[@type='Contact']" mode="displayAuthorBrief">
		<xsl:param name="sortBy"/>
		<!-- contactBrief -->
		<xsl:variable name="parentURL">
			<xsl:apply-templates select="self::Content" mode="getHref">
				<xsl:with-param name="parId" select="@parId"/>
			</xsl:apply-templates>
		</xsl:variable>
		<xsl:variable name="preURL" select="substring(Website,1,3)" />
		<xsl:variable name="linkURL">
			<xsl:choose>
				<xsl:when test="$preURL='www' or $preURL='WWW'">
					<xsl:text>http://</xsl:text>
					<xsl:value-of select="Url"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="Url"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<div class="author">
			<xsl:text>by </xsl:text>
			<a href="{$parentURL}" rel="author">
				<span class="author-name">
					<xsl:attribute name="title">
						<xsl:call-template name="term2072" />
						<xsl:text>&#160;</xsl:text>
						<xsl:value-of select="GivenName/node()"/>
						<xsl:text>&#160;</xsl:text>
						<xsl:value-of select="Surname/node()"/>
					</xsl:attribute>
					<xsl:apply-templates select="." mode="getDisplayName"/>
				</span>
				<span class="author-role">
					<xsl:apply-templates select="Title" mode="displayBrief"/>
					<xsl:if test="Company/node()!=''">
						<xsl:text> - </xsl:text>
						<xsl:apply-templates select="Company" mode="displayBrief"/>
					</xsl:if>
				</span>
			</a>
		</div>
	</xsl:template>

	<!-- Contact Detail -->
	<xsl:template match="Content[@type='Contact']" mode="ContentDetail">
		<xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
		<div class="detail contact">
			<xsl:apply-templates select="." mode="inlinePopupOptions">
				<xsl:with-param name="class" select="'detail contact'"/>
			</xsl:apply-templates>
			<div class="row">
				<div class="col-lg-5 col-xl-4">
					<h1 class="detail-title d-lg-none" itemprop="title">
						<span class="given-name">
							<xsl:apply-templates select="GivenName" mode="displayBrief"/>
						</span>
						<xsl:text> </xsl:text>
						<span class="family-name">
							<xsl:apply-templates select="Surname" mode="displayBrief"/>
						</span>
					</h1>
					<xsl:if test="Title/node()!=''">
						<h2 class="d-lg-none" >
							<xsl:apply-templates select="Company" mode="displayBrief"/>
							<span class="title">
								<xsl:apply-templates select="Title" mode="displayBrief"/>
							</span>
						</h2>
					</xsl:if>
					<span class="detail-img ">
						<xsl:apply-templates select="." mode="displayDetailImage"/>
					</span>
					<xsl:if test="Department/node() or Telephone/node() or Mobile/node() or Fax/node() or Email/node() or Website/node()">
						<div class="card contact-card">
							<div class="card-body">
								<dl class="clearfix dl-horizontal">
									<xsl:if test="Department/node()!=''">
										<dt class="department">
											<!-- Department -->
											<xsl:call-template name="term2011" />
											<xsl:text>: </xsl:text>
										</dt>
										<dd>
											<xsl:apply-templates select="Department" mode="displayBrief"/>
										</dd>
									</xsl:if>
									<xsl:if test="Telephone/node()!=''">
										<dt class="tel">
											<xsl:call-template name="term2007" />
											<xsl:text>: </xsl:text>
										</dt>
										<dd class="tel">
											<xsl:apply-templates select="Telephone" mode="displayBrief"/>
										</dd>

									</xsl:if>
									<xsl:if test="Mobile/node()!=''">
										<dt class="mobile">
											<xsl:call-template name="term2080" />
											<xsl:text>: </xsl:text>
										</dt>
										<dd>
											<xsl:apply-templates select="Mobile" mode="displayBrief"/>
										</dd>
									</xsl:if>
									<xsl:if test="Fax/node()!=''">
										<dt class="fax">
											<xsl:call-template name="term2008" />
											<xsl:text>: </xsl:text>
										</dt>
										<dd>
											<xsl:apply-templates select="Fax" mode="displayBrief"/>
										</dd>
									</xsl:if>
									<xsl:if test="Email/node()!=''">
										<dt class="email">
											<xsl:call-template name="term2009" />
											<xsl:text>: </xsl:text>
										</dt>
										<dd>
											<a href="mailto:{Email/node()}">
												<xsl:apply-templates select="Email" mode="displayBrief"/>
											</a>
										</dd>
									</xsl:if>
									<xsl:if test="Website/node()!=''">
										<dt>
											<xsl:call-template name="term2010" />
											<xsl:text>: </xsl:text>
										</dt>
										<dd>
											<a href="{Website/node()}" class="url">
												<xsl:apply-templates select="Website" mode="displayBrief"/>
											</a>
										</dd>
									</xsl:if>
								</dl>

								<xsl:apply-templates mode="getAddress" select="Location/Address"/>
								<xsl:if test="@facebookURL!='' or @twitterURL!='' or instagramURL!='' or linkedInURL!='' or pinterestURL!=''">
									<xsl:apply-templates select="." mode="socialLinks">
										<xsl:with-param name="iconSet">plain</xsl:with-param>
										<xsl:with-param name="myName">
											<xsl:apply-templates select="GivenName" mode="displayBrief"/>
											<xsl:text> </xsl:text>
											<xsl:apply-templates select="Surname" mode="displayBrief"/>
										</xsl:with-param>
									</xsl:apply-templates>
								</xsl:if>
							</div>
						</div>
					</xsl:if>
				</div>
				<div class="col-lg-7 col-xl-8">
					<div class="contact-text">
						<h1 class="detail-title d-none d-lg-block">
							<span class="given-name">
								<xsl:apply-templates select="GivenName" mode="displayBrief"/>
							</span>
							<xsl:text> </xsl:text>
							<span class="family-name">
								<xsl:apply-templates select="Surname" mode="displayBrief"/>
							</span>
						</h1>
						<xsl:if test="Title/node()!=''">
							<h2 class="d-none d-lg-block">
								<xsl:apply-templates select="Company" mode="displayBrief"/>
								<span class="title">
									<xsl:apply-templates select="Title" mode="displayBrief"/>
								</span>
							</h2>
						</xsl:if>
						<xsl:apply-templates select="Description/node()" mode="cleanXhtml"/>
					</div>
					<xsl:when>
					<xsl:choose test="Content[@type='NewsArticle' and @rtype='Curated']">
						<div class="relatedcontent NewsList">
							<h2>
								Selected articles from <xsl:value-of select="GivenName"/><xsl:text> </xsl:text><xsl:value-of select="Surname"/>
							</h2>
							<div class="row cols row-cols-1 row-cols-lg-2">
								<xsl:apply-templates select="Content[@type='NewsArticle' and @rtype='Curated']" mode="displayBrief">
									<xsl:with-param name="sortBy" select="@publishDate"/>
								</xsl:apply-templates>
							</div>
						</div>
					</xsl:choose>
						<xsl:otherwise>
							<xsl:if test="Content[@type='NewsArticle' and @rtype='Author']">
							<div class="relatedcontent NewsList">
								<h2>
									Articles from <xsl:value-of select="GivenName"/><xsl:text> </xsl:text><xsl:value-of select="Surname"/>
								</h2>
								<div class="row cols row-cols-1 row-cols-lg-2">
									<xsl:apply-templates select="Content[@type='NewsArticle' and @rtype='Author']" mode="displayBrief">
										<xsl:with-param name="sortBy" select="@publishDate"/>
									</xsl:apply-templates>
								</div>
							</div>
							</xsl:if>
						</xsl:otherwise>
					</xsl:when>
					<div class="entryFooter">
						<xsl:if test="Content[@type='Tag']">
							<div class="tags">
								<xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
								<xsl:text> </xsl:text>
							</div>
						</xsl:if>
						<xsl:apply-templates select="." mode="backLink">
							<xsl:with-param name="link" select="$thisURL"/>
							<xsl:with-param name="altText">
								<xsl:call-template name="term2012" />
							</xsl:with-param>
						</xsl:apply-templates>
					</div>
				</div>
			</div>



		</div>
	</xsl:template>
</xsl:stylesheet>
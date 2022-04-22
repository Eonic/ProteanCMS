<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
	<!-- ############## News Articles ##############   -->

	<!-- NewsArticle Module -->
	<xsl:template match="Content[@type='Module' and @moduleType='MedsList']" mode="displayBrief">
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
		<!--responsive columns variables-->

		<!--end responsive columns variables-->
		<!-- Output Module -->
		<div class="clearfix MedsList">
			<div>
				<!--responsive columns -->
				<xsl:apply-templates select="." mode="contentColumns"/>
				<!--end responsive columns-->

				<!-- If Stepper, display Stepper -->
				<xsl:choose>
					<xsl:when test="@linkArticle='true'">
						<xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefLinked">
							<xsl:with-param name="sortBy" select="@sortBy"/>
						</xsl:apply-templates>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
							<xsl:with-param name="sortBy" select="@sortBy"/>
						</xsl:apply-templates>
					</xsl:otherwise>
				</xsl:choose>
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

	<!-- NewsArticle Module Swiper -->
	<xsl:template match="Content[@type='Module' and @moduleType='NewsList' and @carousel='true']" mode="displayBrief">
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
		<xsl:variable name="spacebetween">10</xsl:variable>
		<xsl:variable name="spacebetweenLg">10</xsl:variable>
		<!--responsive columns variables-->

		<!--end responsive columns variables-->
		<!-- Output Module -->
		<div class="swiper-container NewsList content-carousel">
			<div class="swiper" data-autoplayspeed="{@autoPlaySpeed}" data-id="{@id}" data-xscol="{@xsCol}" data-smcol="{@smCol}" data-mdcol="{@mdCol}" data-lgcol="{@lgCol}" data-xlcol="{@xlCol}" data-xxlcol="{@cols}" data-spacebetween="{$spacebetween}" data-spacebetweenlg="{$spacebetweenLg}">
				<div class="swiper-wrapper">
					<xsl:choose>
						<xsl:when test="@linkArticle='true'">
							<xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefLinked">
								<xsl:with-param name="sortBy" select="@sortBy"/>
							</xsl:apply-templates>
						</xsl:when>
						<xsl:otherwise>
							<xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
								<xsl:with-param name="sortBy" select="@sortBy"/>
								<xsl:with-param name="class" select="'swiper-slide'"/>
							</xsl:apply-templates>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:text> </xsl:text>
				</div>

				<div class="swiper-pagination" id="swiper-pagination-{@id}">
					<xsl:text> </xsl:text>
				</div>
			</div>
			<div class="swiper-button-prev" id="swiper-button-prev-{@id}">
				<xsl:text> </xsl:text>
			</div>
			<div class="swiper-button-next" id="swiper-button-next-{@id}">
				<xsl:text> </xsl:text>
			</div>
			<div class="row">
				<span>&#160;</span>
			</div>
		</div>
	</xsl:template>




	<!-- NewsArticle Brief -->
	<xsl:template match="Content[@type='Meds']" mode="displayBrief">
		<xsl:param name="sortBy"/>
		<xsl:param name="class"/>
		<!-- articleBrief -->
		<xsl:variable name="parentURL">
			<xsl:apply-templates select="." mode="getHref"/>
		</xsl:variable>
		<div class="listItem newsarticle {$class}">
			<xsl:apply-templates select="." mode="inlinePopupOptions">
				<xsl:with-param name="class" select="concat('listItem newsarticle ',$class)"/>
				<xsl:with-param name="sortBy" select="$sortBy"/>
			</xsl:apply-templates>
			<div class="lIinner">
				<xsl:if test="Images/img/@src!=''">
					<a href="{$parentURL}" title="Read More - {Headline/node()}" class="list-image-link">
						<xsl:apply-templates select="." mode="displayThumbnail"/>
					</a>
					<!--Accessiblity fix : Separate adjacent links with more than whitespace-->
					<span class="hidden">|</span>
				</xsl:if>
				<h3 class="title" itemprop="headline">
					<a href="{$parentURL}" title="Read More - {Headline/node()}">
						<xsl:apply-templates select="." mode="getDisplayName"/>
					</a>
				</h3>
				<span class="hidden" itemtype="Organization" itemprop="publisher">
					<span itemprop="name">
						<xsl:value-of select="$sitename"/>
					</span>
				</span>
				<xsl:apply-templates select="Content[@type='Contact' and @rtype='Author'][1]" mode="displayAuthorBrief"/>
				<xsl:if test="@publish!=''">
					<p class="date" itemprop="datePublished">
						<xsl:value-of select="/Page/Contents/Content[@name='articleLabel']"/>
						<xsl:call-template name="DisplayDate">
							<xsl:with-param name="date" select="@publish"/>
						</xsl:call-template>
					</p>
				</xsl:if>
				<xsl:if test="@update!=''">
					<p class="hidden" itemprop="dateModified">
						<xsl:value-of select="/Page/Contents/Content[@name='articleLabel']"/>
						<xsl:call-template name="DisplayDate">
							<xsl:with-param name="date" select="@update"/>
						</xsl:call-template>
					</p>
				</xsl:if>

				<xsl:if test="Strapline/node()!=''">
					<div class="summary" itemprop="description">
						<xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
					</div>
				</xsl:if>
				<div class="entryFooter">
					<xsl:apply-templates select="." mode="displayTags"/>
					<xsl:apply-templates select="." mode="moreLink">
						<xsl:with-param name="link" select="$parentURL"/>
						<xsl:with-param name="altText">
							<xsl:value-of select="Headline/node()"/>
						</xsl:with-param>
					</xsl:apply-templates>
					<xsl:text> </xsl:text>
				</div>
			</div>
			<!-- Accessiblity fix : Separate adjacent links with more than whitespace -->
		</div>
	</xsl:template>

	<xsl:template match="Content[@type='Meds']" mode="displayBriefLinked">
		<xsl:param name="sortBy"/>
		<xsl:param name="link"/>
		<xsl:param name="altText"/>
		<xsl:param name="linkType"/>
		<!-- articleBrief -->
		<xsl:variable name="parentURL">
			<xsl:apply-templates select="." mode="getHref"/>
		</xsl:variable>
		<div class="listItem newsarticle">
			<xsl:apply-templates select="." mode="inlinePopupOptions">
				<xsl:with-param name="class" select="'listItem newsarticle'"/>
				<xsl:with-param name="sortBy" select="$sortBy"/>

			</xsl:apply-templates>
			<xsl:choose>
				<xsl:when test="Strapline/descendant-or-self::a">
					<div class="straphaslinks">
						<xsl:if test="not(substring($link,1,1)='/') and (contains($link,'http://') and $linkType='external')">
							<xsl:attribute name="rel">external</xsl:attribute>
							<xsl:attribute name="class">extLink listItem list-group-item newsarticle straphaslinks</xsl:attribute>
						</xsl:if>
						<div class="lIinner">
							<h3 class="title">
								<a href="{$parentURL}">
									<xsl:apply-templates select="." mode="getDisplayName"/>
								</a>
							</h3>
							<xsl:if test="Images/img/@src!=''">
								<xsl:apply-templates select="." mode="displayThumbnail"/>
								<!--Accessiblity fix : Separate adjacent links with more than whitespace-->
								<span class="hidden">|</span>
							</xsl:if>
							<xsl:apply-templates select="Content[@type='Contact']" mode="displayAuthorBrief"/>
							<xsl:if test="@publish!=''">
								<p class="date">
									<xsl:value-of select="/Page/Contents/Content[@name='articleLabel']"/>
									<xsl:call-template name="DisplayDate">
										<xsl:with-param name="date" select="@publish"/>
									</xsl:call-template>
								</p>
							</xsl:if>
							<xsl:if test="Strapline/node()!=''">
								<div class="summary">
									<xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
								</div>
							</xsl:if>
						</div>
					</div>
				</xsl:when>
				<xsl:otherwise>
					<a href="{$parentURL}">
						<xsl:if test="not(substring($link,1,1)='/') and (contains($link,'http://') and $linkType='external')">
							<xsl:attribute name="rel">external</xsl:attribute>
							<xsl:attribute name="class">extLink listItem list-group-item newsarticle</xsl:attribute>
						</xsl:if>
						<div class="lIinner">
							<h3 class="title">
								<xsl:apply-templates select="." mode="getDisplayName"/>
							</h3>
							<xsl:if test="Images/img/@src!=''">
								<xsl:apply-templates select="." mode="displayThumbnail"/>
								<!--Accessiblity fix : Separate adjacent links with more than whitespace-->
								<span class="hidden">|</span>
							</xsl:if>
							<xsl:apply-templates select="Content[@type='Contact']" mode="displayAuthorBrief"/>
							<xsl:if test="@publish!=''">
								<p class="date">
									<xsl:value-of select="/Page/Contents/Content[@name='articleLabel']"/>
									<xsl:call-template name="DisplayDate">
										<xsl:with-param name="date" select="@publish"/>
									</xsl:call-template>
								</p>
							</xsl:if>
							<xsl:if test="Strapline/node()!=''">
								<div class="summary">
									<xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
								</div>
							</xsl:if>
							<xsl:apply-templates select="." mode="displayTagsNoLink"/>
						</div>
					</a>
				</xsl:otherwise>
			</xsl:choose>

		</div>
	</xsl:template>

	<!-- NewsArticle Detail -->
	<xsl:template match="Content[@type='Meds']" mode="ContentDetail">
		<xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
		<xsl:variable name="debugMode">
			<xsl:call-template name="getXmlSettings">
				<xsl:with-param name="sectionName">web</xsl:with-param>
				<xsl:with-param name="valueName">debug</xsl:with-param>
			</xsl:call-template>
		</xsl:variable>
		<div class="detail newsarticle">
			<xsl:apply-templates select="." mode="inlinePopupOptions">
				<xsl:with-param name="class" select="'detail newsarticle'"/>
			</xsl:apply-templates>
			<h2 class="entry-title content-title" itemprop="headline">
				<xsl:apply-templates select="." mode="getDisplayName" />
			</h2>
			<xsl:apply-templates select="." mode="displayDetailImage"/>
			<xsl:apply-templates select="Content[@type='Contact']" mode="displayAuthor"/>
			<xsl:if test="@publish!=''">
				<p class="dtstamp" title="{@publish}" itemprop="datePublished">
					<xsl:call-template name="DisplayDate">
						<xsl:with-param name="date" select="@publish"/>
					</xsl:call-template>
				</p>
			</xsl:if>
			<span class="strapline-detail" itemprop="description">
				<xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
			</span>
			<div class="description entry-content" itemprop="text">
				<xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
			</div>
			<xsl:if test="Content[@type='FAQ']">
				<div class="faq-list">
					<a name="pageTop" class="pageTop">&#160;</a>
					<h3>Question and Answer</h3>
					<ul>
						<xsl:apply-templates select="Content[@type='FAQ']" mode="displayFAQMenu"/>
					</ul>
					<xsl:apply-templates select="Content[@type='FAQ']" mode="displayBrief">
						<xsl:with-param name="sortBy" select="@sortBy"/>
					</xsl:apply-templates>
				</div>
			</xsl:if>
			<div class="entryFooter">
				<div class="tags">
					<xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
					<xsl:text> </xsl:text>
				</div>
				<xsl:apply-templates select="." mode="backLink">
					<xsl:with-param name="link" select="$thisURL"/>
					<xsl:with-param name="altText">
						<xsl:call-template name="term2006" />
					</xsl:with-param>
				</xsl:apply-templates>
			</div>
			<xsl:apply-templates select="." mode="ContentDetailCommenting">
				<xsl:with-param name="commentPlatform" select="$page/Contents/Content[@moduleType='NewsList']/@commentPlatform"/>
			</xsl:apply-templates>
		</div>
	</xsl:template>

	
	

</xsl:stylesheet>
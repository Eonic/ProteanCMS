<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

	<!--  ======================================================================================  -->
	<!--  ==  BACKGROUND CAROUSEL - based on bootstrap carousel=================================  -->
	<!--  ======================================================================================  -->
	<xsl:template match="Content[@moduleType='SwiperCarousel']" mode="displayBrief">
		<!-- Set Variables -->
		<xsl:variable name="contentType" select="@contentType" />
		<xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
		<xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
		<xsl:variable name="contentList">
			<Content>
				<xsl:for-each select="@*">
					<xsl:attribute name="{name()}">
						<xsl:value-of select="."/>
					</xsl:attribute>
				</xsl:for-each>
				<xsl:apply-templates select="." mode="getContent">
					<xsl:with-param name="contentType" select="$contentType" />
					<xsl:with-param name="startPos" select="$startPos" />
				</xsl:apply-templates>
			</Content>
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
		<!-- Output Module -->
		<xsl:variable name="id" select="concat('scarousel-',@id)"></xsl:variable>
		<div class="swiper-container">
			<!--<xsl:apply-templates select="." mode="inlinePopupOptions">
				<xsl:with-param name="class" select="swiper-container"/>
			</xsl:apply-templates>-->
			<div id="{$id}" class="swiper"  data-id="{@id}" data-xscol="1" data-smcol="1" data-mdcol="1" data-lgcol="1" data-xlcol="1" data-xxlcol="1" data-spacebetween="0" data-spacebetweenlg="0" data-autoplay="true">
				<xsl:if test="@bullets!='true'">
					<ol class="carousel-indicators">
						<xsl:for-each select="Content[@type='LibraryImageWithLink']">
							<li data-target="#{$id}" data-slide-to="{position()-1}">
								<xsl:if test="position()=1">
									<xsl:attribute name="class">active</xsl:attribute>
								</xsl:if>
								<xsl:text></xsl:text>
							</li>
						</xsl:for-each>
					</ol>
				</xsl:if>
				<div class="swiper-wrapper" style="height:{@height}px">
					<xsl:apply-templates select="Content[@type='SwiperSlide']" mode="displayBrief">
						<xsl:with-param name="sortBy" select="@sortBy"/>
					</xsl:apply-templates>
				</div>
				<div class="swiper-pagination" id="swiper-pagination-{@id}">
					<xsl:text> </xsl:text>
				</div>
			</div>
			<xsl:if test="@arrows!='false'">
				<div class="swiper-button-prev" id="swiper-button-prev-{@id}"></div>
				<div class="swiper-button-next" id="swiper-button-next-{@id}"></div>
			</xsl:if>
		</div>
	</xsl:template>
	<!-- Library Image Brief -->
	<xsl:template match="Content[@type='SwiperSlide']" mode="displayBrief">
		<div class="swiper-slide" style="background-image:url({Images/img[@class='detail']/@src})">
			<xsl:attribute name="class">
				<xsl:text>swiper-slide justify-content-</xsl:text>
				<xsl:value-of select="@position-vertical"/>
			</xsl:attribute>
			<div class="swiper-admin-btns">
				<xsl:apply-templates select="." mode="inlinePopupOptions">
					<xsl:with-param name="class" select="swiper-admin-btns"/>
				</xsl:apply-templates>
			</div>
			<!--<xsl:if test="position()=1">
				<xsl:attribute name="class">swiper-slide</xsl:attribute>
			</xsl:if>-->
			<xsl:if test="(Title/node()!='' and not(@showHeading='false')) or Body/node()!=''">

				<!--<img src="{Images/img[@class='detail']/@src}" alt="{Title/node()}" />-->
				<div class="swiper-caption">
					<xsl:attribute name="class">
						<xsl:text>swiper-caption</xsl:text>
						<xsl:text> align-self-</xsl:text>
						<xsl:value-of select="@position-horizontal"/>
						<xsl:text> bg-</xsl:text>
						<xsl:value-of select="@bg-color"/>
					</xsl:attribute>
					<div class="swiper-caption-inner">

						<xsl:if test="Title/node()!='' and not(@showHeading='false')">
							<h3 class="caption-title">
								<xsl:attribute name="class">
									<xsl:text>caption-title text-</xsl:text>
									<xsl:value-of select="@title-horizontal"/>
								</xsl:attribute>
								<xsl:value-of select="Title/node()"/>
							</h3>
						</xsl:if>
						<xsl:apply-templates select="Body/node()" mode="cleanXhtml"></xsl:apply-templates>
						<xsl:if test="@link!=''">

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
						</xsl:if>
					</div>
				</div>
			</xsl:if>
		</div>
	</xsl:template>

</xsl:stylesheet>
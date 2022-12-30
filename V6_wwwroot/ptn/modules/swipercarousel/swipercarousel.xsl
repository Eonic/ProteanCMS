<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
	<xsl:template match="Content[@moduleType='SwiperCarousel']" mode="contentJS">
		<style>
			#scarousel-<xsl:value-of select="@id"/>{height:<xsl:value-of select="@heightxs"/>px}
			@media(min-width:992px){#scarousel-<xsl:value-of select="@id"/>{height:<xsl:value-of select="@height"/>px}}
		</style>

	</xsl:template>
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
			<div id="{$id}" class="swiper" data-id="{@id}" data-speed="{@speed}" data-effect="{@effect}" data-direction="{@direction}" data-xscol="1" data-smcol="1" data-mdcol="1" data-lgcol="1" data-xlcol="1" data-xxlcol="1" data-spacebetween="0" data-spacebetweenlg="0" data-autoplay="{@autoplay}" data-autoplayspeed="{@interval}">
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
				<!--<div class="swiper-wrapper" style="height:{@height}px">-->
				<div class="swiper-wrapper">
					<xsl:apply-templates select="Content[@type='SwiperSlide']" mode="displayBrief">
						<xsl:with-param name="sortBy" select="@sortBy"/>
						<xsl:with-param name="cHeightOptions" select="@cHeightOptions"/>
					</xsl:apply-templates>
				</div>
				<div class="swiper-pagination" id="swiper-pagination-{@id}">
					<xsl:text> </xsl:text>
				</div>
			</div>
			<xsl:if test="@arrows!='true' or not(@arrows)">
				<div class="swiper-button-prev" id="swiper-button-prev-{@id}"> </div>
				<div class="swiper-button-next" id="swiper-button-next-{@id}"> </div>
			</xsl:if>
		</div>
	</xsl:template>
	<!-- Library Image Brief -->
	<xsl:template match="Content[@type='SwiperSlide']" mode="displayBrief">
		<xsl:param name="cHeightOptions"/>
		<xsl:choose>
			<xsl:when test="@linkSlide='true'">

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
					<xsl:if test="$cHeightOptions='adapt'">
						<img src="{Images/img[@class='detail']/@src}" alt="{Title/node()}" />
					</xsl:if>
					<a>
						<xsl:attribute name="href">
							<xsl:choose>
								<xsl:when test="format-number(@link,'0')!='NaN'">
									<xsl:variable name="pageId" select="@link"/>
									<xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]" mode="getHref"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of select="@link"/>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:attribute>
						<xsl:attribute name="class">
							<xsl:text>swiper-slide-padding swiper-slide-link justify-content-</xsl:text>
							<xsl:value-of select="@position-horizontal"/>
						</xsl:attribute>
						<xsl:if test="(Title/node()!='' and not(@showHeading='false')) or Body/node()!=''">


							<div class="swiper-caption">
								<xsl:attribute name="class">
									<xsl:text>swiper-caption</xsl:text>
									<xsl:text> align-self-</xsl:text>
									<xsl:value-of select="@position-vertical"/>
									<xsl:choose>
										<xsl:when test="@bg-color!=''">
											<xsl:text> bg-</xsl:text>
											<xsl:value-of select="@bg-color"/>
											<xsl:text>-o </xsl:text>
										</xsl:when>
										<xsl:otherwise>
											<xsl:text> nobg-slide</xsl:text>
										</xsl:otherwise>
									</xsl:choose>
									<!--<xsl:value-of select="@bg-cover"/>-->
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
										<button>
											<xsl:attribute name="class">
												<xsl:text>btn </xsl:text>
												<xsl:choose>
													<xsl:when test="@buttonClass and @buttonClass!=''">
														<xsl:value-of select="@buttonClass"/>
													</xsl:when>
													<xsl:otherwise>
														<xsl:text>btn-primary </xsl:text>
													</xsl:otherwise>
												</xsl:choose>
												<xsl:text> </xsl:text>
												<xsl:value-of select="@alignButton"/>
											</xsl:attribute>
											<xsl:value-of select="@linkText"/>
										</button>
										<!--<xsl:apply-templates select="." mode="moreLink">
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
										</xsl:apply-templates>-->
									</xsl:if>
								</div>
							</div>
						</xsl:if>
					</a>
				</div>

			</xsl:when>
			<xsl:otherwise>
				<div style="background-image:url({Images/img[@class='detail']/@src})" class="swiper-slide">
					<!--<xsl:if test="$cHeightOptions!='adapt'">
						<xsl:attribute name="style">background-image:url({Images/img[@class='detail']/@src})</xsl:attribute>
					</xsl:if>-->

					<div class="swiper-admin-btns">
						<xsl:apply-templates select="." mode="inlinePopupOptions">
							<xsl:with-param name="class" select="swiper-admin-btns"/>
						</xsl:apply-templates>
					</div>
					<!--<xsl:if test="position()=1">
				<xsl:attribute name="class">swiper-slide</xsl:attribute>
			</xsl:if>-->
					<xsl:if test="$cHeightOptions='adapt'">
						<img src="{Images/img[@class='detail']/@src}" alt="{Title/node()}" />
					</xsl:if>
					<xsl:if test="(Title/node()!='' and not(@showHeading='false')) or Body/node()!=''">
						<div class="swiper-slide-padding">
							<xsl:attribute name="class">
								<xsl:text> swiper-slide-padding justify-content-</xsl:text>
								<xsl:value-of select="@position-horizontal"/>
							</xsl:attribute>
							<!--<img src="{Images/img[@class='detail']/@src}" alt="{Title/node()}" />-->
							<div class="swiper-caption">
								<xsl:attribute name="class">
									<xsl:text>swiper-caption</xsl:text>
									<xsl:text> align-self-</xsl:text>
									<xsl:value-of select="@position-vertical"/>
									<xsl:choose>
										<xsl:when test="@bg-color!=''">
											<xsl:text> bg-</xsl:text>
											<xsl:value-of select="@bg-color"/>
											<xsl:text>-o </xsl:text>
										</xsl:when>
										<xsl:otherwise>
											<xsl:text> plain-slide</xsl:text>
										</xsl:otherwise>
									</xsl:choose>
									<!--<xsl:value-of select="@bg-cover"/>-->
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
										<a>
											<xsl:attribute name="href">
												<xsl:choose>
													<xsl:when test="format-number(@link,'0')!='NaN'">
														<xsl:variable name="pageId" select="@link"/>
														<xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]" mode="getHref"/>
													</xsl:when>
													<xsl:otherwise>
														<xsl:value-of select="@link"/>
													</xsl:otherwise>
												</xsl:choose>
											</xsl:attribute>
											<xsl:attribute name="class">
												<xsl:text>btn </xsl:text>
												<xsl:choose>
													<xsl:when test="@buttonClass and @buttonClass!=''">
														<xsl:value-of select="@buttonClass"/>
													</xsl:when>
													<xsl:otherwise>
														<xsl:text>btn-primary </xsl:text>
													</xsl:otherwise>
												</xsl:choose>
												<xsl:text> </xsl:text>
												<xsl:value-of select="@alignButton"/>
											</xsl:attribute>
											<xsl:value-of select="@linkText"/>
										</a>
										<!--<xsl:apply-templates select="." mode="moreLink">
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
										<xsl:with-param name="class" select="@alignButton"/>
									</xsl:apply-templates>-->
									</xsl:if>
								</div>
							</div>
						</div>
					</xsl:if>
				</div>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

</xsl:stylesheet>
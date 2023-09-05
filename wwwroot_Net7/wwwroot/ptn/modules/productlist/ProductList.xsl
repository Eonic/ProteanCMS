<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

	<!--   ################   Products   ###############   -->


	<xsl:template match="Content[@type='Module' and @moduleType='ProductList']" mode="themeModuleExtras">
		<!-- this is empty because we want this on individual listing panels not the containing module-->
	</xsl:template>

	<xsl:template match="Content[@type='Module' and @moduleType='ProductList']" mode="themeModuleClassExtras">
		<!-- this is empty because we want this on individual listing panels not the containing module-->
	</xsl:template>

	<!-- Product Brief -->
	<xsl:template match="Content[@type='Product']" mode="displayBrief">
		<xsl:param name="sortBy"/>
		<xsl:param name="crop"/>
		<xsl:param name="class"/>
		<xsl:param name="pos"/>
		<xsl:param name="parentId"/>
		<xsl:param name="linked"/>
		<xsl:variable name="parId">
			<xsl:choose>
				<xsl:when test="@parId &gt; 0">
					<xsl:value-of select="@parId"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="/Page/@id"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="parentURL">
			<xsl:apply-templates select="self::Content" mode="getHref">
				<xsl:with-param name="parId" select="$parId"/>
			</xsl:apply-templates>
		</xsl:variable>
		<xsl:variable name="classValues">
			<xsl:text>listItem product </xsl:text>
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
					<a href="{$parentURL}" class="list-image-link">
						<xsl:apply-templates select="." mode="displayThumbnail">
							<xsl:with-param name="crop" select="$cropSetting" />
							<xsl:with-param name="class">list-image</xsl:with-param>
						</xsl:apply-templates>
					</a>
				</xsl:if>
				<h3 class="title">
					<xsl:variable name="title">
						<xsl:apply-templates select="." mode="getDisplayName"/>
					</xsl:variable>
					<a href="{$parentURL}">
						<xsl:value-of select="$title"/>
					</a>
				</h3>
				<xsl:if test="StockCode/node()!='' or Manufacturer/node()!=''">
					<p class="product-facts">
						<xsl:if test="StockCode/node()!=''">
							<span class="stockCode">
								<span class="item-label">
									<xsl:call-template name="term2014" />
								</span>
								<xsl:text>: </xsl:text>
								<xsl:value-of select="StockCode/node()"/>
							</span>
						</xsl:if>
						<xsl:if test="StockCode/node()!='' and Manufacturer/node()!=''">
							<br/>
						</xsl:if>
						<xsl:if test="Manufacturer/node()!=''">
							<span class="manufacturer">
								<xsl:if test="/Page/Contents/Content[@name='makeLabel']">
									<span class="label">
										<xsl:value-of select="/Page/Contents/Content[@name='makeLabel']"/>&#160;
									</span>
								</xsl:if>
								<span class="brand">
									<xsl:value-of select="Manufacturer/node()"/>
								</span>
							</span>
						</xsl:if>
					</p>
				</xsl:if>
				<xsl:choose>
					<xsl:when test="Content[@type='SKU']">
						<xsl:apply-templates select="Content[@type='SKU'][1]" mode="displayPrice" />
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="." mode="displayPrice" />
					</xsl:otherwise>
				</xsl:choose>
				<!--<xsl:if test="ShortDescription/node()!=''">
					<div class="description">
						<xsl:apply-templates select="ShortDescription/node()" mode="cleanXhtml"/>
					</div>
				</xsl:if>-->
				<div class="entryFooter">
					<xsl:apply-templates select="." mode="moreLink">
						<xsl:with-param name="link" select="$parentURL"/>
						<xsl:with-param name="stretchLink" select="$linked"/>
						<xsl:with-param name="altText">
							<xsl:apply-templates select="." mode="getDisplayName"/>
						</xsl:with-param>
					</xsl:apply-templates>
					<xsl:text> </xsl:text>
				</div>
			</div>
		</div>
	</xsl:template>

	<!-- SKU Brief - work in progress [CR 2011-05-27] -->
	<xsl:template match="Content[@type='SKU']" mode="displayBrief">
		<xsl:param name="sortBy"/>
		<xsl:param name="pos"/>
		<xsl:variable name="parId">
			<xsl:choose>
				<xsl:when test="@parId &gt; 0">
					<xsl:value-of select="@parId"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="/Page/@id"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="parentURL">
			<xsl:apply-templates select="self::Content" mode="getHref">
				<xsl:with-param name="parId" select="$parId"/>
			</xsl:apply-templates>
		</xsl:variable>
		<div class="listItem sku">
			<xsl:apply-templates select="." mode="inlinePopupOptions">
				<xsl:with-param name="class" select="'listItem sku'"/>
				<xsl:with-param name="sortBy" select="$sortBy"/>
			</xsl:apply-templates>
			<div class="lIinner">
				<h3 class="fn title">
					<xsl:variable name="title">
						<xsl:apply-templates select="." mode="getDisplayName"/>
					</xsl:variable>
					<a href="{$parentURL}" title="{$title}">
						<xsl:value-of select="$title"/>
					</a>
				</h3>
				<xsl:if test="Images/img/@src!=''">
					<a href="{$parentURL}" class="url">
						<xsl:apply-templates select="." mode="displayThumbnail"/>
					</a>
				</xsl:if>
				<xsl:if test="StockCode/node()!='' or Manufacturer/node()!=''">
					<p class="product-facts">
						<xsl:if test="StockCode/node()!=''">
							<span class="stockCode">
								<span class="item-label">
									<xsl:call-template name="term2014" />
								</span>
								<xsl:text>: </xsl:text>
								<xsl:value-of select="StockCode/node()"/>
							</span>
						</xsl:if>
						<xsl:if test="StockCode/node()!='' and Manufacturer/node()!=''">
							<br/>
						</xsl:if>
						<xsl:if test="Manufacturer/node()!=''">
							<span class="manufacturer">
								<xsl:if test="/Page/Contents/Content[@name='makeLabel']">
									<span class="label">
										<xsl:value-of select="/Page/Contents/Content[@name='makeLabel']"/>&#160;
									</span>
								</xsl:if>
								<span class="brand">
									<xsl:value-of select="Manufacturer/node()"/>
								</span>
							</span>
						</xsl:if>
					</p>
				</xsl:if>
				<xsl:choose>
					<xsl:when test="Content[@type='SKU']">
						<xsl:apply-templates select="Content[@type='SKU'][1]" mode="displayPrice" />
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="." mode="displayPrice" />
					</xsl:otherwise>
				</xsl:choose>
				<xsl:if test="ShortDescription/node()!=''">
					<div class="description">
						<xsl:apply-templates select="ShortDescription/node()" mode="cleanXhtml"/>
					</div>
				</xsl:if>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Content[@type='Product']" mode="opengraph-namespace">
		<xsl:text>og: http://ogp.me/ns# fb: http://ogp.me/ns/fb# product: http://ogp.me/ns/product#</xsl:text>
	</xsl:template>

	<xsl:template match="Content[@type='Product']" mode="opengraphdata">
		<meta property="og:type" content="product" />
		<meta property="product:upc" content="{StockCode/node()}" />
		<meta property="product:sale_price:currency" content="{$currencyCode}" />
		<xsl:variable name="price">
			<xsl:value-of select="Prices/Price[@currency=$currency and @type='sale']/node()"/>
		</xsl:variable>
		<meta property="product:sale_price:amount" content="{$price}" />
	</xsl:template>

	<!-- Product Detail -->
	<xsl:template match="Content[@type='Product']" mode="ContentDetail">
		<xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
		<xsl:variable name="parId" select="@parId" />
		<div class="detail product-detail">
			<xsl:apply-templates select="." mode="inlinePopupOptions">
				<xsl:with-param name="class" select="'detail product-detail'"/>
			</xsl:apply-templates>
			<div class="">
				<xsl:if test="Images/img[@class='detail']/@src!=''">
					<xsl:attribute name="class">
						<xsl:text>row</xsl:text>
					</xsl:attribute>
				</xsl:if>
				<div class="detail-text">
					<xsl:if test="Images/img[@class='detail']/@src!=''">
						<xsl:attribute name="class">
							<xsl:text>col-lg-6 col-product-info</xsl:text>
						</xsl:attribute>
					</xsl:if>
					<h1 class="detail-title content-title">
						<xsl:value-of select="Name/node()"/>
					</h1>
					<xsl:if test="StockCode/node()!='' or Manufacturer/node()!=''">
						<p class="product-facts">
							<xsl:if test="StockCode/node()!=''">
								<span class="stockCode">
									<span class="item-label">
										<xsl:call-template name="term2014" />
									</span>
									<xsl:text>: </xsl:text>
									<xsl:value-of select="StockCode/node()"/>
								</span>
							</xsl:if>
							<xsl:if test="StockCode/node()!='' and Manufacturer/node()!=''">
								<br/>
							</xsl:if>
							<xsl:if test="Manufacturer/node()!=''">
								<span class="manufacturer">
									<xsl:if test="/Page/Contents/Content[@name='makeLabel']">
										<span class="label">
											<xsl:value-of select="/Page/Contents/Content[@name='makeLabel']"/>&#160;
										</span>
									</xsl:if>
									<span class="brand">
										<xsl:value-of select="Manufacturer/node()"/>
									</span>
								</span>
							</xsl:if>
						</p>
					</xsl:if>
					<xsl:if test="ShortDescription/node()!=''">
						<div class="description">
							<xsl:apply-templates select="ShortDescription/node()" mode="cleanXhtml"/>
						</div>
					</xsl:if>
					<xsl:choose>
						<xsl:when test="Content[@type='SKU']">
							<xsl:apply-templates select="Content[@type='SKU'][1]" mode="displayPrice" />
						</xsl:when>
						<xsl:otherwise>
							<xsl:apply-templates select="." mode="displayPrice" />
						</xsl:otherwise>
					</xsl:choose>


					<xsl:if test="/Page/Cart">
						<xsl:apply-templates select="." mode="addToCartButton"/>
					</xsl:if>
					<xsl:apply-templates select="." mode="SpecLink"/>
				</div>
				<xsl:if test="Images/img[@class='detail']/@src!=''">
					<div class="">
						<xsl:attribute name="class">
							<xsl:text>col-lg-6 col-product-img</xsl:text>
						</xsl:attribute>
						<xsl:choose>
							<xsl:when test="Content[@type='LibraryImage']">
								<div id="productScroller">
									<!--<div class="swiper swiper1" thumbs-swiper=".swiper2" space-between="10" navigation="true">-->
									<div class="swiper" navigation="true">

										<div class="swiper-wrapper">
											<div class="swiper-slide">
												<!--<xsl:choose>
													<xsl:when test="Content[@type='SKU']">
														<xsl:choose>
															<xsl:when test="count(Content[@type='SKU']/Images/img[@class='detail' and @src != '']) &gt; 0">
																<xsl:for-each select="Content[@type='SKU']">
																	<xsl:apply-templates select="." mode="displayDetailImage">
																		<xsl:with-param name="showImage">
																			<xsl:if test="position() != 1">
																				<xsl:text>noshow</xsl:text>
																			</xsl:if>
																		</xsl:with-param>
																	</xsl:apply-templates>
																</xsl:for-each>
															</xsl:when>
															<xsl:otherwise>
																<xsl:apply-templates select="." mode="displayDetailImage"/>
															</xsl:otherwise>
														</xsl:choose>
													</xsl:when>
													<xsl:otherwise>
														<xsl:apply-templates select="." mode="displayDetailImage"/>
													</xsl:otherwise>
												</xsl:choose>-->

												<!--<xsl:apply-templates select="." mode="displayDetailImage"/>-->
												<img src="{Images/img[@class='detail']/@src}"/>
											</div>
											<xsl:apply-templates select="Content[@type='LibraryImage']" mode="scrollerImage"/>
										</div>
										<div class="swiper-button-prev"></div>
										<div class="swiper-button-next"></div>
									</div>
									<!--<div class="swiper swiper2" space-between="10" slides-per-view="4" free-mode="true" watch-slides-progress="true">
										<div class="swiper-wrapper">
											<div class="swiper-slide">
												<xsl:choose>
													<xsl:when test="Content[@type='SKU']">
														<xsl:choose>
															<xsl:when test="count(Content[@type='SKU']/Images/img[@class='detail' and @src != '']) &gt; 0">
																<xsl:for-each select="Content[@type='SKU']">
																	<xsl:apply-templates select="." mode="displayDetailImage">
																		<xsl:with-param name="showImage">
																			<xsl:if test="position() != 1">
																				<xsl:text>noshow</xsl:text>
																			</xsl:if>
																		</xsl:with-param>
																	</xsl:apply-templates>
																</xsl:for-each>
															</xsl:when>
															<xsl:otherwise>
																<xsl:apply-templates select="." mode="displayDetailImage"/>
															</xsl:otherwise>
														</xsl:choose>
													</xsl:when>
													<xsl:otherwise>
														<xsl:apply-templates select="." mode="displayDetailImage"/>
													</xsl:otherwise>
												</xsl:choose>
											</div>
											<xsl:apply-templates select="Content[@type='LibraryImage']" mode="scrollerImage"/>
										</div>
									</div>-->
								</div>
							</xsl:when>
							<xsl:otherwise>
								<span class="detail-img ">
									<xsl:choose>
										<!-- Test whether product has SKU's -->
										<xsl:when test="Content[@type='SKU']">
											<xsl:choose>
												<!--Test whether there're any detailed SKU images-->
												<xsl:when test="count(Content[@type='SKU']/Images/img[@class='detail' and @src != '']) &gt; 0">
													<xsl:for-each select="Content[@type='SKU']">
														<xsl:apply-templates select="." mode="displayDetailImage">
															<!-- hide all but the first image -->
															<xsl:with-param name="showImage">
																<xsl:if test="position() != 1">
																	<xsl:text>noshow</xsl:text>
																</xsl:if>
															</xsl:with-param>
														</xsl:apply-templates>
													</xsl:for-each>
												</xsl:when>
												<xsl:otherwise>
													<!-- If no SKU's have detailed images show default product image -->
													<xsl:apply-templates select="." mode="displayDetailImage"/>
												</xsl:otherwise>
											</xsl:choose>
										</xsl:when>
										<xsl:otherwise>
											<!-- Display Default Image -->
											<xsl:apply-templates select="." mode="displayDetailImage"/>
										</xsl:otherwise>
									</xsl:choose>
								</span>
							</xsl:otherwise>
						</xsl:choose>
					</div>
				</xsl:if>
			</div>
			<xsl:if test="Body/node()!=''">
				<div class="description">
					<xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
				</div>
			</xsl:if>
			<div class="entryFooter">
				<xsl:if test="Content[@type='Tag']">
					<div class="tags">
						<xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
						<xsl:text> </xsl:text>
					</div>
				</xsl:if>
			</div>

			<!--RELATED CONTENT-->
			<xsl:if test="Content">
				<!-- Reviews  -->
				<xsl:if test="Content[@type='Review']">
					<xsl:apply-templates select="." mode="relatedReviews"/>
				</xsl:if>
				<!-- Products  -->
				<xsl:if test="Content[@type='Product']">
					<div class="relatedcontent">
						<xsl:apply-templates select="/Page" mode="inlinePopupSingle">
							<xsl:with-param name="type">PlainText</xsl:with-param>
							<xsl:with-param name="text">Add Title for Related Products</xsl:with-param>
							<xsl:with-param name="name">relatedProductsTitle</xsl:with-param>
						</xsl:apply-templates>
						<h2>
							<xsl:choose>
								<xsl:when test="/Page/Contents/Content[@name='relatedProductsTitle']">
									<xsl:apply-templates select="/Page/Contents/Content[@name='relatedProductsTitle']" mode="displayBrief"/>
								</xsl:when>
								<xsl:otherwise>
									<xsl:text>Related Products</xsl:text>
								</xsl:otherwise>
							</xsl:choose>
						</h2>
						<xsl:apply-templates select="/" mode="List_Related_Products">
							<xsl:with-param name="parProductID" select="@id"/>
						</xsl:apply-templates>
					</div>
				</xsl:if>
			</xsl:if>
			<div class="entryFooter">
				<xsl:apply-templates select="." mode="backLink">
					<xsl:with-param name="link" select="/Page/Request/ServerVariables/Item[@name='PREVIOUS_PAGE']/node()"/>
					<xsl:with-param name="altText">
						<xsl:call-template name="term2015" />
					</xsl:with-param>
				</xsl:apply-templates>
				<xsl:text> </xsl:text>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Content" mode="scrollerImage">
		<xsl:param name="showImage"/>
		<xsl:variable name="imgId">
			<xsl:text>picture_</xsl:text>
			<xsl:value-of select="@id"/>
		</xsl:variable>
		<!-- Needed to create unique grouping for lightbox -->
		<xsl:variable name="parId">
			<xsl:text>group</xsl:text>
			<xsl:value-of select="@type"/>
		</xsl:variable>
		<xsl:variable name="src">
			<xsl:choose>
				<!-- IF use display -->
				<xsl:when test="Images/img[@class='display']/@src and Images/img[@class='display']/@src!=''">
					<xsl:value-of select="Images/img[@class='display']/@src"/>
				</xsl:when>
				<!-- Else Full Size use that -->
				<xsl:when test="Images/img[@class='detail']/@src and Images/img[@class='detail']/@src!=''">
					<xsl:value-of select="Images/img[@class='detail']/@src"/>
				</xsl:when>
			</xsl:choose>
		</xsl:variable>
		<!-- ALT VALUE -->
		<xsl:variable name="alt">
			<xsl:choose>
				<!-- IF Full Size use that -->
				<xsl:when test="Images/img[@class='detail']/@alt and Images/img[@class='detail']/@alt!=''">
					<xsl:value-of select="Images/img[@class='detail']/@alt"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="@name"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="newSrc">
			<xsl:call-template name="resize-image">
				<xsl:with-param name="path" select="$src"/>
				<xsl:with-param name="max-width" select="500"/>
				<xsl:with-param name="max-height" select="150"/>
				<xsl:with-param name="file-prefix">
					<xsl:text>~dis-</xsl:text>
					<xsl:value-of select="500"/>
					<xsl:text>x</xsl:text>
					<xsl:value-of select="150"/>
					<xsl:text>/~dis-</xsl:text>
				</xsl:with-param>
				<xsl:with-param name="file-suffix" select="''"/>
				<xsl:with-param name="quality" select="100"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="largeSrc">
			<xsl:call-template name="resize-image">
				<xsl:with-param name="path" select="$src"/>
				<xsl:with-param name="max-width" select="500"/>
				<xsl:with-param name="max-height" select="500"/>
				<xsl:with-param name="file-prefix">
					<xsl:text>~lg-</xsl:text>
					<xsl:value-of select="500"/>
					<xsl:text>x</xsl:text>
					<xsl:value-of select="500"/>
					<xsl:text>/~lg-</xsl:text>
				</xsl:with-param>
				<xsl:with-param name="file-suffix" select="''"/>
				<xsl:with-param name="quality" select="100"/>
			</xsl:call-template>
		</xsl:variable>
		<div class="swiper-slide">
			<!--<a href="{$largeSrc}" class="responsive-lightbox">
				<xsl:if test="$parId != ''">
					<xsl:attribute name="rel">
						<xsl:text>lightbox[</xsl:text>
						<xsl:value-of select="$parId"/>
						<xsl:text>]</xsl:text>
					</xsl:attribute>
				</xsl:if>
				<img src="{$newSrc}" width="{ew:ImageWidth($newSrc)}" height="{ew:ImageHeight($newSrc)}" alt="{$alt}" class="detail">
					<xsl:if test="$imgId != ''">
						<xsl:attribute name="id">
							<xsl:value-of select="$imgId"/>
						</xsl:attribute>
					</xsl:if>
				</img>
			</a>-->
			<img src="{$newSrc}" width="{ew:ImageWidth($newSrc)}" height="{ew:ImageHeight($newSrc)}" alt="{$alt}" class="detail">
				<xsl:if test="$imgId != ''">
					<xsl:attribute name="id">
						<xsl:value-of select="$imgId"/>
					</xsl:attribute>
				</xsl:if>
			</img>
		</div>
	</xsl:template>

	<!-- List Related Products-->
	<xsl:template match="/" mode="List_Related_Products">
		<xsl:param name="parProductID"/>
		<xsl:for-each select="/Page/ContentDetail/Content/Content">
			<xsl:sort select="@type" order="ascending"/>
			<xsl:sort select="@displayOrder" order="ascending"/>
			<xsl:choose>
				<xsl:when test="@type='Product'">
					<div class="row cols row-cols-1  row-cols-md-2 row-cols-lg-3">
						<xsl:apply-templates select="." mode="displayBrief"/>
					</div>
				</xsl:when>
			</xsl:choose>
		</xsl:for-each>
	</xsl:template>

	<!-- Product Brief -->
	<!--<xsl:template match="Content[@type='Product']" mode="displayBriefRelated">
		<xsl:param name="sortBy"/>
		<xsl:param name="pos"/>
		<xsl:variable name="parId">
			<xsl:choose>
				<xsl:when test="@parId &gt; 0">
					<xsl:value-of select="@parId"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="/Page/@id"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="parentURL">
			<xsl:apply-templates select="self::Content" mode="getHref">
				<xsl:with-param name="parId" select="$parId"/>
			</xsl:apply-templates>
		</xsl:variable>
		<div class="listItem product">
			<xsl:apply-templates select="." mode="inlinePopupOptions">
				<xsl:with-param name="class" select="'listItem product'"/>
				<xsl:with-param name="sortBy" select="$sortBy"/>
			</xsl:apply-templates>
			<div class="lIinner">
				<xsl:if test="Images/img/@src!=''">
					<a href="{$parentURL}" class="url list-image-link">
						<xsl:apply-templates select="." mode="displayThumbnail">
							<xsl:with-param name="width">125</xsl:with-param>
							<xsl:with-param name="height">125</xsl:with-param>
							<xsl:with-param name="forceResize">true</xsl:with-param>
						</xsl:apply-templates>
					</a>
				</xsl:if>
				<h3 class="title">
					<xsl:variable name="title">
						<xsl:apply-templates select="." mode="getDisplayName"/>
					</xsl:variable>
					<a href="{$parentURL}" title="{$title}">
						<xsl:value-of select="$title"/>
					</a>
				</h3>
				<xsl:if test="Manufacturer/node()!=''">
					<p class="manufacturer">
						<xsl:if test="/Page/Contents/Content[@name='makeLabel']">
							<span class="label">
								<xsl:value-of select="/Page/Contents/Content[@name='makeLabel']"/>
							</span>&#160;
						</xsl:if>
						<span class="brand">
							<xsl:value-of select="Manufacturer/node()"/>
						</span>
					</p>
				</xsl:if>
				<xsl:if test="StockCode/node()!=''">
					<p class="sku stockCode">
						<span class="label">
							<xsl:call-template name="term2014" />
							<xsl:text>: </xsl:text>
						</span>
						<xsl:value-of select="StockCode/node()"/>
					</p>
				</xsl:if>
				<xsl:choose>
					<xsl:when test="Content[@type='SKU']">
						<xsl:apply-templates select="Content[@type='SKU'][1]" mode="displayPrice" />
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="." mode="displayPrice" />
					</xsl:otherwise>
				</xsl:choose>
				<xsl:if test="ShortDescription/node()!=''">
					<div class="description">
						<xsl:apply-templates select="ShortDescription/node()" mode="cleanXhtml"/>
					</div>
				</xsl:if>
				<div class="entryFooter">
					<xsl:if test="/Page/Cart">
						<xsl:apply-templates select="." mode="addToCartButton">
							<xsl:with-param name="actionURL" select="$parentURL"/>
						</xsl:apply-templates>
					</xsl:if>
					<xsl:text> </xsl:text>
				</div>
			</div>
		</div>
	</xsl:template>-->

</xsl:stylesheet>
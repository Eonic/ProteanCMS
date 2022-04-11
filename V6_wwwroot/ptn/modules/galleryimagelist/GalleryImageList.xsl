<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

	<!-- Gallery Images List Module -->
	<xsl:template match="Content[(@type='Module' and @moduleType='GalleryImageList') or Content[@type='LibraryImage']]" mode="displayBrief">
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
		<div class="GalleryImageList Grid">
			<div style="justify-content:{@alignment}" >
				<xsl:choose>
					<xsl:when test="@xsCol!='' or @smCol!='' or @mdCol!='' or @lgCol!='' or @xlCol!='' or @xxlCol!=''">
						<!--responsive columns-->
						<xsl:apply-templates select="." mode="contentColumns"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:attribute name="class">inline-gallery</xsl:attribute>
					</xsl:otherwise>
				</xsl:choose>
				<!-- If Stepper, display Stepper -->
				<xsl:if test="@stepCount != '0'">
					<xsl:apply-templates select="/" mode="genericStepper">
						<xsl:with-param name="GalleryImageList" select="$contentList"/>
						<xsl:with-param name="noPerPage" select="@stepCount"/>
						<xsl:with-param name="startPos" select="$startPos"/>
						<xsl:with-param name="queryStringParam" select="$queryStringParam"/>
						<xsl:with-param name="totalCount" select="$totalCount"/>
					</xsl:apply-templates>
				</xsl:if>
				<xsl:apply-templates select="ms:node-set($contentList)/*/*" mode="displayBrief">
					<xsl:with-param name="sortBy" select="@sortBy"/>
					<xsl:with-param name="crop" select="@crop"/>
					<xsl:with-param name="lightbox" select="@lightbox"/>
					<xsl:with-param name="showTitle" select="@showTitle"/>
					<xsl:with-param name="alignment" select="@alignment"/>
					<xsl:with-param name="alignmentV" select="@alignmentV"/>
				</xsl:apply-templates>
			</div>
		</div>
	</xsl:template>

	<!-- Gallery Images List Module With Swiper -->
	<xsl:template match="Content[(@type='Module' and @moduleType='GalleryImageList') and @carousel='true']" mode="displayBrief">
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
		<xsl:variable name="spacebetween">10</xsl:variable>
		<xsl:variable name="spacebetweenLg">10</xsl:variable>
		<!-- Output Module -->
		<div class="swiper-container content-carousel GalleryImageList Grid">
			<div class="swiper" data-id="{@id}" data-xscol="{@xsCol}" data-smcol="{@smCol}" data-mdcol="{@mdCol}" data-lgcol="{@lgCol}" data-xlcol="{@xlCol}" data-xxlcol="{@cols}" data-spacebetween="{$spacebetween}" data-spacebetweenlg="{$spacebetweenLg}" >

				<!--responsive columns-->
				<!--<xsl:apply-templates select="." mode="contentColumns"/>-->

				<div class="swiper-wrapper">

					<!-- If Stepper, display Stepper -->
					<xsl:if test="@stepCount != '0'">
						<xsl:apply-templates select="/" mode="genericStepper">
							<xsl:with-param name="GalleryImageList" select="$contentList"/>
							<xsl:with-param name="noPerPage" select="@stepCount"/>
							<xsl:with-param name="startPos" select="$startPos"/>
							<xsl:with-param name="queryStringParam" select="$queryStringParam"/>
							<xsl:with-param name="totalCount" select="$totalCount"/>
						</xsl:apply-templates>
					</xsl:if>
					<xsl:apply-templates select="ms:node-set($contentList)/*/*" mode="displayBrief">
						<xsl:with-param name="sortBy" select="@sortBy"/>
						<xsl:with-param name="crop" select="@crop"/>
						<xsl:with-param name="lightbox" select="@lightbox"/>
						<xsl:with-param name="showTitle" select="@showTitle"/>
						<xsl:with-param name="alignment" select="@alignment"/>
						<xsl:with-param name="alignmentV" select="@alignmentV"/>
						<xsl:with-param name="class" select="'swiper-slide'"/>
					</xsl:apply-templates>
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
			</div>
		</div>
	</xsl:template>


</xsl:stylesheet>
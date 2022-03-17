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
		<!--responsive columns variables-->
		<xsl:variable name="xsColsToShow">
			<xsl:choose>
				<xsl:when test="@xsCol='2'">2</xsl:when>
				<xsl:otherwise>1</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="smColsToShow">
			<xsl:choose>
				<xsl:when test="@smCol and @smCol!=''">
					<xsl:value-of select="@smCol"/>
				</xsl:when>
				<xsl:otherwise>2</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="mdColsToShow">
			<xsl:choose>
				<xsl:when test="@mdCol and @mdCol!=''">
					<xsl:value-of select="@mdCol"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="@cols"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<!-- Output Module -->
		<div class="GalleryImageList Grid">
			<xsl:if test="@carousel='true'">
				<xsl:attribute name="class">
					<xsl:text>clearfix GalleryImageList Grid content-scroller</xsl:text>
				</xsl:attribute>
			</xsl:if>
			<div class="cols cols{@cols}" data-xscols="{$xsColsToShow}" data-smcols="{$smColsToShow}" data-mdcols="{$mdColsToShow}" data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1" data-dots="{@carouselBullets}" data-height="{@carouselHeight}" >
				<!--responsive columns-->
				<xsl:attribute name="class">
					<xsl:text>cols</xsl:text>
					<xsl:choose>
						<xsl:when test="@xsCol='2'"> mobile-2-col-content</xsl:when>
						<xsl:otherwise> mobile-1-col-content</xsl:otherwise>
					</xsl:choose>
					<xsl:if test="@smCol and @smCol!=''">
						<xsl:text> sm-content-</xsl:text>
						<xsl:value-of select="@smCol"/>
					</xsl:if>
					<xsl:if test="@mdCol and @mdCol!=''">
						<xsl:text> md-content-</xsl:text>
						<xsl:value-of select="@mdCol"/>
					</xsl:if>
					<xsl:text> cols</xsl:text>
					<xsl:value-of select="@cols"/>
					<xsl:if test="@mdCol and @mdCol!=''">
						<xsl:text> content-cols-responsive</xsl:text>
					</xsl:if>
				</xsl:attribute>
				<!--end responsive columns-->
				<xsl:if test="@autoplay !=''">
					<xsl:attribute name="data-autoplay">
						<xsl:value-of select="@autoplay"/>
					</xsl:attribute>
				</xsl:if>
				<xsl:if test="@autoPlaySpeed !=''">
					<xsl:attribute name="data-autoPlaySpeed">
						<xsl:value-of select="@autoPlaySpeed"/>
					</xsl:attribute>
				</xsl:if>
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
				</xsl:apply-templates>
			</div>
		</div>
	</xsl:template>
  
</xsl:stylesheet>
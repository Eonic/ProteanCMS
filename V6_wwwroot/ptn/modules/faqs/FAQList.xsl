<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

	<!-- ############## FAQ Module ##############   -->
	<!-- FAQ Module -->
	<xsl:template match="Content[@type='Module' and @moduleType='FAQList']" mode="displayBrief">
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
		<div class="faqList">
			<a name="pageTop" class="pageTop">
				<xsl:text> </xsl:text>
			</a>
			<div id="pageMenu">
				<ul>
					<xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayFAQMenu"/>
					<xsl:text> </xsl:text>
				</ul>
			</div>
			<div class="cols cols{@cols}">
				<xsl:if test="@stepCount != '0'">
					<xsl:apply-templates select="/" mode="genericStepper">
						<xsl:with-param name="articleList" select="$contentList"/>
						<xsl:with-param name="noPerPage" select="@stepCount"/>
						<xsl:with-param name="startPos" select="$startPos"/>
						<xsl:with-param name="queryStringParam" select="$queryStringParam"/>
						<xsl:with-param name="totalCount" select="$totalCount"/>
					</xsl:apply-templates>
				</xsl:if>
				<xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
					<xsl:with-param name="sortBy" select="@sortBy"/>
					<xsl:with-param name="heading" select="@heading"/>
					<xsl:with-param name="title" select="@title"/>
				</xsl:apply-templates>
				<xsl:text> </xsl:text>
			</div>
		</div>
	</xsl:template>

	<!-- FAQ Menu -->
	<xsl:template match="Content[@type='FAQ']" mode="displayFAQMenu">
		<xsl:variable name="currentUrl">
			<xsl:apply-templates select="$currentPage" mode="getHref"/>
		</xsl:variable>
		<li>
			<a href="#faq-{@id}" title="{@name}">
				<xsl:choose>
					<!-- Older sites might not have the DisplayName Field, had to be introduced to allow ? when used as an FAQ page. -->
					<xsl:when test="DisplayName/node()!=''">
						<xsl:value-of select="DisplayName/node()"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="@name"/>
					</xsl:otherwise>
				</xsl:choose>
			</a>
			<xsl:if test="Strapline/node()!=''">
				<span class="infoTopics">
					<br/>
					<xsl:apply-templates select="Strapline" mode="cleanXhtml"/>
				</span>
			</xsl:if>
		</li>
	</xsl:template>

	<!-- FAQ Brief -->
	<xsl:template match="Content[@type='FAQ']" mode="displayBrief">
		<xsl:param name="sortBy"/>
		<xsl:param name="heading"/>
		<xsl:param name="title"/>
		<div class="listItem faq">
			<xsl:apply-templates select="." mode="inlinePopupOptions">
				<xsl:with-param name="class" select="'listItem'"/>
				<xsl:with-param name="sortBy" select="$sortBy"/>
			</xsl:apply-templates>
			<div class="lIinner">
				<a name="faq-{@id}" class="faq-link">
					<xsl:text> </xsl:text>
				</a>

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
							<xsl:value-of select="@name"/>
						</xsl:element>
					</xsl:when>
					<xsl:otherwise>
						<xsl:choose>
							<xsl:when test="$heading=''">
								<h3 class="title">
									<xsl:value-of select="@name"/>
								</h3>
							</xsl:when>
							<xsl:otherwise>
								<xsl:element name="{$heading}">
									<xsl:attribute name="class">
										<xsl:text>title</xsl:text>
									</xsl:attribute>
									<xsl:value-of select="@name"/>
								</xsl:element>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:otherwise>
				</xsl:choose>
				<!--<h3>
					<xsl:choose>
						-->
				<!-- Older sites might not have the DisplayName Field, had to be introduced to allow ? when used as an FAQ page. -->
				<!--
						<xsl:when test="DisplayName/node()!=''">
							<xsl:value-of select="DisplayName/node()"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="@name"/>
						</xsl:otherwise>
					</xsl:choose>
				</h3>-->
				<xsl:if test="Images/img[@class='thumbnail']/@src!=''">
					<img src="{Images/img[@class='thumbnail']/@src}" width="{Images/img[@class='thumbnail']/@width}" height="{Images/img[@class='thumbnail']/@height}" alt="{Images/img[@class='thumbnail']/@alt}" class="thumbnail"/>
				</xsl:if>
				<div class="description">
					<xsl:apply-templates select="Body" mode="cleanXhtml"/>
				</div>
				<div class="backTop">
					<a href="#pageTop" title="Back to Top">Back To Top</a>
				</div>
			</div>
		</div>
	</xsl:template>


	<xsl:template match="Content[@moduleType='FAQList']" mode="JSONLD">
		{
		"@context": "https://schema.org",
		"@type": "FAQPage",
		"mainEntity": [
		<xsl:apply-templates select="Content[@type='FAQ']" mode="JSONLD-list"/>
		<xsl:apply-templates select="$page/Contents/Content[@type='FAQ']" mode="JSONLD-list"/>
		]
		}
	</xsl:template>

	<xsl:template match="Content[@type='FAQ']" mode="JSONLD-list">
		{
		"@type": "Question",
		"name": "<xsl:call-template name="escape-json">
			<xsl:with-param name="string">
				<xsl:apply-templates select="DisplayName" mode="flattenXhtml"/>
			</xsl:with-param>
		</xsl:call-template>",
		"acceptedAnswer": {
		"@type": "Answer",
		"text": "<xsl:call-template name="escape-json">
			<xsl:with-param name="string">
				<xsl:apply-templates select="Body" mode="flattenXhtml"/>
			</xsl:with-param>
		</xsl:call-template>"
		}
		}
		<xsl:if test="position()!=last()">,</xsl:if>
	</xsl:template>

	<!-- FAQ Module Accordian -->
	<xsl:template match="Content[@type='Module' and @moduleType='FAQList' and @presentationType='accordian']" mode="displayBrief">
		<!-- Set Variables -->
		<xsl:variable name="contentType" select="@contentType"/>
		<xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
		<xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
		<xsl:variable name="contentList">
			<xsl:apply-templates select="." mode="getContent">
				<xsl:with-param name="contentType" select="$contentType"/>
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
		<div class="faqList accordion accordion-module" id="accordion-{@id}">
			<xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayFAQAccordianBrief">
				<xsl:with-param name="parId" select="@id"/>
				<xsl:with-param name="heading" select="@heading"/>
				<xsl:with-param name="title" select="@title"/>
			</xsl:apply-templates>
		</div>
	</xsl:template>

	<!-- FAQ Menu -->
	<xsl:template match="Content[@type='FAQ']" mode="displayFAQAccordianBrief">
		<xsl:param name="sortBy"/>
		<xsl:param name="parId"/>
		<xsl:param name="heading"/>
		<xsl:param name="title"/>
		<div class="accordion-item">

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
							<xsl:text>accordion-header</xsl:text>
						</xsl:attribute>
						<xsl:attribute name="id">
							<xsl:text>heading</xsl:text>
							<xsl:value-of select="@id"/>
						</xsl:attribute>
						<xsl:apply-templates select="." mode="inlinePopupOptions">
							<xsl:with-param name="class" select="'accordion-header'"/>
							<xsl:with-param name="sortBy" select="$sortBy"/>
						</xsl:apply-templates>
						<button role="button" data-bs-toggle="collapse" data-parent="#accordion{@id}" data-bs-target="#accordian-item-{$parId}-{@id}" aria-expanded="false" aria-controls="accordian-item-{$parId}-{@id}" class="accordion-button collapsed">
							<xsl:apply-templates select="DisplayName" mode="cleanXhtml"/>
						</button>
					</xsl:element>
				</xsl:when>
				<xsl:otherwise>
					<xsl:choose>
						<xsl:when test="$heading=''">
							<h3 class="accordion-header" id="heading{@id}">
								<xsl:apply-templates select="." mode="inlinePopupOptions">
									<xsl:with-param name="class" select="'accordion-header'"/>
									<xsl:with-param name="sortBy" select="$sortBy"/>
								</xsl:apply-templates>
								<button role="button" data-bs-toggle="collapse" data-parent="#accordion{@id}" data-bs-target="#accordian-item-{$parId}-{@id}" aria-expanded="false" aria-controls="accordian-item-{$parId}-{@id}" class="accordion-button collapsed">
									<xsl:apply-templates select="DisplayName" mode="cleanXhtml"/>
								</button>
							</h3>
						</xsl:when>
						<xsl:otherwise>
							<xsl:element name="{$heading}">
								<xsl:attribute name="class">
									<xsl:text>accordion-header</xsl:text>
								</xsl:attribute>
								<xsl:attribute name="id">
									<xsl:text>heading</xsl:text>
									<xsl:value-of select="@id"/>
								</xsl:attribute>
								<xsl:apply-templates select="." mode="inlinePopupOptions">
									<xsl:with-param name="class" select="'accordion-header'"/>
									<xsl:with-param name="sortBy" select="$sortBy"/>
								</xsl:apply-templates>
								<button role="button" data-bs-toggle="collapse" data-parent="#accordion{@id}" data-bs-target="#accordian-item-{$parId}-{@id}" aria-expanded="false" aria-controls="accordian-item-{$parId}-{@id}" class="accordion-button collapsed">
									<xsl:apply-templates select="DisplayName" mode="cleanXhtml"/>
								</button>
							</xsl:element>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:otherwise>
			</xsl:choose>
			<!--<h3 class="accordion-header" id="heading{@id}">
				<xsl:apply-templates select="." mode="inlinePopupOptions">
					<xsl:with-param name="class" select="'panel-heading'"/>
					<xsl:with-param name="sortBy" select="$sortBy"/>
				</xsl:apply-templates>
				<button role="button" data-bs-toggle="collapse" data-parent="#accordion{@id}" data-bs-target="#accordian-item-{$parId}-{@id}" aria-expanded="false" aria-controls="accordian-item-{$parId}-{@id}" class="accordion-button">
					<xsl:apply-templates select="DisplayName" mode="cleanXhtml"/>
				</button>
			</h3>-->
			<div id="accordian-item-{$parId}-{@id}" class="accordion-collapse collapse " aria-labelledby="heading{@id}">
				<div class="accordion-body">
					<xsl:if test="Strapline/node()!=''">
						<div class="strapline">
							<xsl:apply-templates select="Strapline" mode="cleanXhtml"/>
						</div>
					</xsl:if>
					<xsl:if test="Body/node()!=''">
						<xsl:apply-templates select="Body" mode="cleanXhtml"/>
					</xsl:if>
				</div>
			</div>
		</div>
	</xsl:template>

</xsl:stylesheet>
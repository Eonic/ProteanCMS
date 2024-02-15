<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
	<!--   ################   Links   ###############   -->
	<!-- Links Module -->
	<xsl:template match="Content[@type='Module' and @moduleType='LinkList']" mode="displayBrief">
		<xsl:variable name="contentType" select="@contentType" />
		<xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
		<xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
		<xsl:variable name="contentList">
			<xsl:apply-templates select="." mode="getContent">
				<xsl:with-param name="contentType" select="$contentType" />
				<xsl:with-param name="startPos" select="$startPos" />
			</xsl:apply-templates>
		</xsl:variable>
		<xsl:variable name="cropSetting">
			<xsl:choose>
				<xsl:when test="@crop='true'">
					<xsl:text>true</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>false</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
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
		<!--end responsive columns variables-->
		<xsl:choose>
			<xsl:when test="@layout='simple'">
				<div class="clearfix LinkListSimple">
					<ul class="nav nav-module">
						<xsl:if test="@align='vertical'">
							<xsl:attribute name="class">nav nav-module flex-column</xsl:attribute>
						</xsl:if>
						<xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefSimple">
							<xsl:with-param name="sortBy" select="@sortBy"/>
						</xsl:apply-templates>
					</ul>
				</div>
			</xsl:when>
			<xsl:otherwise>
				<div class="clearfix Links LinkList">
					<xsl:if test="@carousel='true'">
						<xsl:attribute name="class">
							<xsl:text>clearfix Links LinkList content-scroller</xsl:text>
						</xsl:attribute>
					</xsl:if>
					<div>

						<xsl:apply-templates select="." mode="contentColumns"/>
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
						<xsl:if test="@stepCount != '0'">
							<xsl:apply-templates select="/" mode="genericStepper">
								<xsl:with-param name="linkList" select="$contentList"/>
								<xsl:with-param name="noPerPage" select="@stepCount"/>
								<xsl:with-param name="startPos" select="$startPos"/>
								<xsl:with-param name="queryStringParam" select="$queryStringParam"/>
								<xsl:with-param name="totalCount" select="$totalCount"/>
							</xsl:apply-templates>
						</xsl:if>
						<xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
							<xsl:with-param name="sortBy" select="@sortBy"/>
							<xsl:with-param name="crop" select="$cropSetting"/>
							<xsl:with-param name="button" select="@button"/>
							<xsl:with-param name="imagePosition" select="@imagePosition"/>
							<xsl:with-param name="alignment" select="@alignment"/>
							<xsl:with-param name="linked" select="@linkArticle"/>
						</xsl:apply-templates>
					</div>
				</div>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- Simple Links Brief -->
	<xsl:template match="Content[@type='Link']" mode="displayBriefSimple">
		<xsl:param name="sortBy"/>
		<xsl:variable name="preURL" select="substring(Url,1,3)" />
		<xsl:variable name="url" select="Url/node()" />
		<xsl:variable name="linkURL">
			<xsl:choose>
				<xsl:when test="format-number($url,'0')!='NaN'">
					<xsl:apply-templates select="$page/descendant-or-self::MenuItem[@id=$url]" mode="getHref"/>
				</xsl:when>
				<xsl:when test="@InPageID!=''">
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

		<li class="nav-item">
			<xsl:apply-templates select="." mode="inlinePopupOptions">
				<xsl:with-param name="class" select="'nav-item'"/>
				<xsl:with-param name="sortBy" select="$sortBy"/>
			</xsl:apply-templates>

			<a href="{$linkURL}" title="{Name}" class="nav-link">
				<xsl:if test="not(substring(@linkURL,1,1)='/') and (contains(@linkURL,'http://') and Url/@type='external')">
					<xsl:attribute name="rel">external</xsl:attribute>
					<xsl:attribute name="class">extLink</xsl:attribute>
				</xsl:if>
				<xsl:value-of select="Name"/>
			</a>
		</li>
	</xsl:template>

	<!-- Links Brief -->
	<xsl:template match="Content[@type='Link']" mode="displayBrief">
		<xsl:param name="sortBy"/>
		<xsl:param name="button"/>
		<xsl:param name="crop"/>
		<xsl:param name="imagePosition"/>
		<xsl:param name="alignment"/>
		<xsl:param name="linked"/>
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
		<xsl:variable name="classValues">
			<xsl:text>listItem link </xsl:text>
			<xsl:if test="$linked='true'">
				<xsl:text> linked-listItem </xsl:text>
			</xsl:if>
			<!--<xsl:value-of select="$class"/>-->
			<xsl:text> </xsl:text>
			<!--<xsl:apply-templates select="." mode="themeModuleClassExtrasListItem">
				<xsl:with-param name="parentId" select="$parentId"/>
			</xsl:apply-templates>-->
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
			<xsl:apply-templates select="." mode="inlinePopupOptions">
				<xsl:with-param name="class" select="'listItem link'"/>
				<xsl:with-param name="sortBy" select="$sortBy"/>
			</xsl:apply-templates>
			<div class="lIinner">
				<xsl:if test="$imagePosition='below'">
					<h3 class="title">
						<xsl:attribute name="class">
							<xsl:text>title text-</xsl:text>
							<xsl:value-of select="$alignment"/>
						</xsl:attribute>
						<a href="{$linkURL}" title="{Name}">
							<xsl:if test="not(substring(@linkURL,1,1)='/') and (contains(@linkURL,'http://') and Url/@type='external')">
								<xsl:attribute name="rel">external</xsl:attribute>
								<xsl:attribute name="class">extLink</xsl:attribute>
							</xsl:if>
							<xsl:value-of select="Name"/>
						</a>
					</h3>
				</xsl:if>
				<xsl:if test="Images/img/@src!=''">
					<a href="{$linkURL}" title="Click here to link to {Name}">
						<xsl:if test="not(substring(@linkURL,1,1)='/') and (contains(@linkURL,'http://') and Url/@type='external')">
							<xsl:attribute name="rel">external</xsl:attribute>
						</xsl:if>
						<xsl:apply-templates select="." mode="displayThumbnail">
							<xsl:with-param name="crop" select="$cropSetting" />
						</xsl:apply-templates>
					</a>
				</xsl:if>
				<xsl:if test="not($imagePosition='below')">
					<h3 class="title">
						<xsl:attribute name="class">
							<xsl:text>title text-</xsl:text>
							<xsl:value-of select="$alignment"/>
						</xsl:attribute>
						<a href="{$linkURL}" title="{Name}">
							<xsl:if test="not(substring(@linkURL,1,1)='/') and (contains(@linkURL,'http://') and Url/@type='external')">
								<xsl:attribute name="rel">external</xsl:attribute>
								<xsl:attribute name="class">extLink</xsl:attribute>
							</xsl:if>
							<xsl:value-of select="Name"/>
						</a>
					</h3>
				</xsl:if>
				<xsl:if test="Body/node()!=''">
					<div class="description">
						<xsl:attribute name="class">
							<xsl:text>description text-</xsl:text>
							<xsl:value-of select="$alignment"/>
						</xsl:attribute>
						<xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
					</div>
				</xsl:if>
				<xsl:if test="$linked='true' and $button='false'">
					<a href="{$linkURL}" class="stretched-link">
						<xsl:if test="Url[@target='New Window']">
							<xsl:attribute name="target">
								<xsl:text>_blank</xsl:text>
							</xsl:attribute>
						</xsl:if>
						<span class="visually-hidden">
							<xsl:value-of select="Name/node()"/>
							<xsl:text> </xsl:text>
						</span>
					</a>
				</xsl:if>
				<xsl:if test="not($button='false')">
					<div>
						<xsl:attribute name="class">
							<xsl:text>entryFooter light-flex justify-content-</xsl:text>
							<xsl:value-of select="$alignment"/>
						</xsl:attribute>
						<xsl:apply-templates select="." mode="displayTags"/>
						<xsl:apply-templates select="." mode="moreLink">
							<xsl:with-param name="link" select="$linkURL"/>
							<xsl:with-param name="stretchLink" select="$linked"/>
							<xsl:with-param name="linkType" select="Url/@type"/>
							<xsl:with-param name="altText">
								<xsl:value-of select="Name/node()"/>
							</xsl:with-param>
						</xsl:apply-templates>
						<xsl:text> </xsl:text>
					</div>
				</xsl:if>
			</div>
		</div>
	</xsl:template>
</xsl:stylesheet>
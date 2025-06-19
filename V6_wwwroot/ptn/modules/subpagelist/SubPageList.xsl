<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

	<!--   #############   Sub Page Listing   ###########   -->
	<!-- Sub Page List Module -->
	<xsl:template match="Content[@type='Module' and @moduleType='SubPageList']" mode="displayBrief">
		<!-- Set Variables -->
		<xsl:variable name="contentType" select="@contentType" />
		<xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
		<xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
		<xsl:variable name="link" select="@link"/>
		<xsl:variable name="parentPage" select="//MenuItem[@id=$link]"/>
		<xsl:variable name="contentList">
			<xsl:apply-templates select="." mode="getContent">
				<xsl:with-param name="showHidden" select="@showHidden"/>
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

		<xsl:variable name="showImg">
			<xsl:choose>
				<xsl:when test="@showImg='false'">
					<xsl:text>false</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>true</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="showDesc">
			<xsl:choose>
				<xsl:when test="@showDesc='false'">
					<xsl:text>false</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>true</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="totalCount">
			<xsl:choose>
				<xsl:when test="@display='related'">
					<xsl:value-of select="count($parentPage/MenuItem)"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="count($currentPage/MenuItem)"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="@layout='simple'">
				<div class="clearfix SubPageListSimple">
					<ul class="nav nav-module" role="navigation">
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
				<div class="SubPages">
          <xsl:attribute name="class">
            <xsl:text>SubPages </xsl:text>
            <xsl:text>crop-setting-</xsl:text>
            <xsl:value-of select="$cropSetting"/>
          </xsl:attribute>
          <xsl:if test="@gutter and @gutter!=''">
            <xsl:attribute name="style">
              <xsl:text>padding-left:</xsl:text>
              <xsl:value-of select="@gutter"/>
              <xsl:text>rem;padding-right:</xsl:text>
              <xsl:value-of select="@gutter"/>
              <xsl:text>rem;</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <div data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1" data-dots="{@carouselBullets}" data-height="{@carouselHeight}"  role="navigation" aria-label="{@aria}"  >
            <xsl:if test="@gutter and @gutter!=''">
              <xsl:attribute name="style">
                <xsl:text>margin-left:-</xsl:text>
                <xsl:value-of select="@gutter"/>
                <xsl:text>rem;margin-right:-</xsl:text>
                <xsl:value-of select="@gutter"/>
                <xsl:text>rem;</xsl:text>
              </xsl:attribute>
            </xsl:if>
            <xsl:apply-templates select="." mode="contentColumns"/>
						<xsl:if test="@stepCount != '0'">
							<xsl:apply-templates select="/" mode="genericStepper">
								<xsl:with-param name="contentList" select="$contentList"/>
								<xsl:with-param name="noPerPage" select="@stepCount"/>
								<xsl:with-param name="startPos" select="$startPos"/>
								<xsl:with-param name="queryStringParam" select="$queryStringParam"/>
								<xsl:with-param name="totalCount" select="$totalCount" />
							</xsl:apply-templates>
						</xsl:if>
						<xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
							<xsl:with-param name="sortBy" select="@sortBy"/>
							<xsl:with-param name="crop" select="$cropSetting"/>
							<xsl:with-param name="showImg" select="$showImg"/>
							<xsl:with-param name="showDesc" select="$showDesc"/>
							<xsl:with-param name="showHidden" select="@showHidden"/>
							<xsl:with-param name="fixedThumb" select="@fixedThumb"/>
							<xsl:with-param name="button" select="@button"/>
							<xsl:with-param name="imagePosition" select="@imagePosition"/>
							<xsl:with-param name="alignment" select="@alignment"/>
							<xsl:with-param name="parentId" select="@id"/>
							<xsl:with-param name="linked" select="@linkArticle"/>
							<xsl:with-param name="heading" select="@heading"/>
							<xsl:with-param name="no-heading-content" select="@no-heading-content"/>
						</xsl:apply-templates>
					</div>
				</div>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="Content[@type='Module' and @moduleType='SubPageList']" mode="themeModuleExtras">
		<!-- this is empty because we want this on individual listing panels not the containing module-->
	</xsl:template>

	<xsl:template match="Content[@type='Module' and @moduleType='SubPageList']" mode="themeModuleClassExtras">
		<!-- this is empty because we want this on individual listing panels not the containing module-->
	</xsl:template>

	<!-- Sub Page Content -->
	<xsl:template match="MenuItem" mode="displayBriefSimple">
		<xsl:param name="sortBy"/>
		<xsl:param name="showHidden"/>
		<xsl:variable name="url">
			<xsl:apply-templates select="." mode="getHref"/>
		</xsl:variable>
		<xsl:variable name="pageName">
			<xsl:apply-templates select="." mode="getDisplayName"/>
		</xsl:variable>
		<xsl:if test="(@name!='Info Menu' and (not(DisplayName/@exclude='true'))) or (@name!='Info Menu' and $showHidden='true')">
			<li class="nav-item">
				<xsl:apply-templates select="." mode="inlinePopupOptions">
					<xsl:with-param name="class" select="'nav-item'"/>
					<xsl:with-param name="sortBy" select="$sortBy"/>
				</xsl:apply-templates>
				<a href="{$url}" title="{$pageName}" class="nav-link">
					<!--<xsl:apply-templates select="." mode="menuLink"/>-->
					<xsl:apply-templates select="." mode="getDisplayName"/>
				</a>
			</li>
		</xsl:if>
	</xsl:template>

	<xsl:template match="MenuItem" mode="displayBrief">
		<xsl:param name="sortBy"/>
		<xsl:param name="crop"/>
		<xsl:param name="showImg"/>
		<xsl:param name="showDesc"/>
		<xsl:param name="showHidden"/>
		<xsl:param name="fixedThumb"/>
		<xsl:param name="button"/>
		<xsl:param name="imagePosition"/>
		<xsl:param name="alignment"/>
		<xsl:param name="parentId"/>
		<xsl:param name="linked"/>
		<xsl:param name="heading"/>
		<xsl:param name="no-heading-content"/>
		<xsl:variable name="url">
			<xsl:apply-templates select="." mode="getHref"/>
		</xsl:variable>
		<xsl:variable name="pageName">
			<xsl:apply-templates select="." mode="getDisplayName"/>
		</xsl:variable>
		<xsl:variable name="cropSetting">
			<xsl:choose>
				<xsl:when test="$crop='true'">
					<xsl:text>true</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>false</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:if test="(@name!='Info Menu' and (not(DisplayName/@exclude='true'))) or (@name!='Info Menu' and $showHidden='true')">
			<xsl:variable name="classValues">
				<xsl:text>listItem subpageItem</xsl:text>
				<xsl:if test="$linked='true'">
					<xsl:text> linked-listItem </xsl:text>
				</xsl:if>
				<xsl:apply-templates select="." mode="themeModuleClassExtrasListItem">
					<xsl:with-param name="parentId" select="$parentId"/>
				</xsl:apply-templates>
			</xsl:variable>
			<div class="{$classValues}">
				<xsl:apply-templates select="." mode="themeModuleExtrasListItem">
					<xsl:with-param name="parentId" select="$parentId"/>
					<xsl:with-param name="pos" select="position()"/>
				</xsl:apply-templates>
				<xsl:apply-templates select="." mode="inlinePopupOptions">
					<xsl:with-param name="class" select="$classValues"/>
					<xsl:with-param name="sortBy" select="$sortBy"/>
				</xsl:apply-templates>
				<div class="lIinner">
					<xsl:if test="$imagePosition='below'">
						<xsl:choose>
              <xsl:when test="$no-heading-content='true'">
                <span class="title">
                  <xsl:choose>
                    <xsl:when test="$linked='true'">
                      <xsl:apply-templates select="." mode="getDisplayName"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:apply-templates select="." mode="menuLink"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </span>
              </xsl:when>
							<xsl:when test="$heading='h2'">
								<xsl:element name="{$heading}">
									<xsl:attribute name="class">
										<xsl:text>title text-</xsl:text>
										<xsl:value-of select="$alignment"/>
									</xsl:attribute>
                  <xsl:choose>
                    <xsl:when test="$linked='true'">
                      <xsl:apply-templates select="." mode="getDisplayName"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:apply-templates select="." mode="menuLink"/>
                    </xsl:otherwise>
                  </xsl:choose>
								</xsl:element>
							</xsl:when>
							<xsl:otherwise>
								<h3 class="title">
									<xsl:attribute name="class">
										<xsl:text>title text-</xsl:text>
										<xsl:value-of select="$alignment"/>
									</xsl:attribute>
                  <xsl:choose>
                    <xsl:when test="$linked='true'">
                      <xsl:apply-templates select="." mode="getDisplayName"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:apply-templates select="." mode="menuLink"/>
                    </xsl:otherwise>
                  </xsl:choose>
								</h3>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:if>
					<xsl:if test="Images/img[@src!=''] and not($showImg='false')">
            <xsl:choose>
              <xsl:when test="$linked='true'">
                <span class="list-image-link">
                  <xsl:attribute name="title">
                    <xsl:apply-templates select="." mode="getTitleAttr"/>
                  </xsl:attribute>
                  <xsl:apply-templates select="." mode="displaySubPageThumb">
                    <xsl:with-param name="crop" select="$cropSetting" />
                    <xsl:with-param name="fixedThumb" select="$fixedThumb" />
                  </xsl:apply-templates>
                </span>
              </xsl:when>
              <xsl:otherwise>
                <a href="{$url}"  class="list-image-link">
                  <xsl:attribute name="title">
                    <xsl:apply-templates select="." mode="getTitleAttr"/>
                  </xsl:attribute>
                  <xsl:apply-templates select="." mode="displaySubPageThumb">
                    <xsl:with-param name="crop" select="$cropSetting" />
                    <xsl:with-param name="fixedThumb" select="$fixedThumb" />
                  </xsl:apply-templates>
                </a>
              </xsl:otherwise>
            </xsl:choose>
					</xsl:if>
          <div class="media-inner">
            <xsl:if test="not($imagePosition='below')">
              <xsl:choose>
                <xsl:when test="$no-heading-content='true'">
                  <span class="title">
                    <xsl:choose>
                      <xsl:when test="$linked='true'">
                        <xsl:apply-templates select="." mode="getDisplayName"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:apply-templates select="." mode="menuLink"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </span>
                </xsl:when>
                <xsl:when test="$heading='h2'">
                  <xsl:element name="{$heading}">
                    <xsl:attribute name="class">
                      <xsl:text>title text-</xsl:text>
                      <xsl:value-of select="$alignment"/>
                    </xsl:attribute>
                    <xsl:choose>
                      <xsl:when test="$linked='true'">
                        <xsl:apply-templates select="." mode="getDisplayName"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:apply-templates select="." mode="menuLink"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:element>
                </xsl:when>
                <xsl:otherwise>
                  <h3 class="title">
                    <xsl:attribute name="class">
                      <xsl:text>title text-</xsl:text>
                      <xsl:value-of select="$alignment"/>
                    </xsl:attribute>
                    <xsl:choose>
                      <xsl:when test="$linked='true'">
                        <xsl:apply-templates select="." mode="getDisplayName"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:apply-templates select="." mode="menuLink"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </h3>
                </xsl:otherwise>
              </xsl:choose>

            </xsl:if>
            <xsl:if test="Description/node()!='' and not($showDesc='false')">
              <span class="listDescription">
                <xsl:attribute name="class">
                  <xsl:text>description text-</xsl:text>
                  <xsl:value-of select="$alignment"/>
                </xsl:attribute>
                <xsl:apply-templates select="Description/node()" mode="cleanXhtml" />
                <xsl:text> </xsl:text>
              </span>
            </xsl:if>
            <xsl:if test="$linked='true' and $button='false'">
              <a href="{$url}" class="stretched-link">
                <span class="visually-hidden">
                  <xsl:apply-templates select="." mode="getTitleAttr"/>
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
                <xsl:apply-templates select="." mode="moreLink">
                  <!--<xsl:with-param name="linkText">
									<xsl:call-template name="term2026" />
									<xsl:text>&#160;</xsl:text>
									<xsl:apply-templates select="." mode="getDisplayName" />
								</xsl:with-param>-->
                  <xsl:with-param name="link" select="$url"/>
                  <xsl:with-param name="stretchLink" select="$linked"/>
                  <xsl:with-param name="altText">
                    <xsl:apply-templates select="." mode="getTitleAttr" />
                  </xsl:with-param>
                </xsl:apply-templates>
                <xsl:text> </xsl:text>
              </div>
            </xsl:if>
          </div>
				</div>
			</div>
		</xsl:if>
	</xsl:template>
</xsl:stylesheet>
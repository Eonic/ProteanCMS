<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!-- Tag List Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='TagList']" mode="displayBrief">
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
    <!-- Output Module -->
    <div class="TagsList">

      <div class="cols{@cols}">
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
        <!-- If Stepper, display Stepper -->
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="articleList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <ul>
          <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefListItem">
            <xsl:with-param name="sortBy" select="@sortBy"/>
          </xsl:apply-templates>
        </ul>
      </div>
    </div>
  </xsl:template>

  <!-- Tags List Brief -->
  <xsl:template match="Content[@type='Tag']" mode="displayBriefListItem">
    <xsl:param name="sortBy"/>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <xsl:variable name="name" select="Name/node()"/>
    <li>
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <a href="{$parentURL}" rel="tag">
        <xsl:apply-templates select="Name" mode="displayBrief"/>
        <xsl:if test="@relatedCount!=''">
          (<xsl:value-of select="@relatedCount"/>)
        </xsl:if>
      </a>
    </li>
  </xsl:template>

	<!-- Tags Display -->
	<xsl:template match="Content" mode="displayTags">
		<xsl:param name="sortBy"/>
		<xsl:variable name="articleList">
			<xsl:for-each select="Content[@type='Tag']">
				<xsl:copy-of select="."/>
			</xsl:for-each>
		</xsl:variable>
		<xsl:if test="count(Content[@type='Tag'])&gt;0">
			<div class="tags">
				<p>
					<!--Tags-->
					<xsl:call-template name="term2039" />
					<xsl:text>: </xsl:text>
					<xsl:apply-templates select="ms:node-set($articleList)" mode="displayBrief">
						<xsl:with-param name="sortBy" select="@sortBy"/>
					</xsl:apply-templates>
				</p>
			</div>
		</xsl:if>
	</xsl:template>

	<!-- Tags Brief -->
	<xsl:template match="Content[@type='Tag']" mode="displayBrief">
		<xsl:param name="sortBy"/>
		<xsl:variable name="parentURL">
			<xsl:apply-templates select="." mode="getHref"/>
		</xsl:variable>
		<xsl:variable name="name" select="Name/node()"/>
		<span>
			<xsl:apply-templates select="." mode="inlinePopupOptions">
				<xsl:with-param name="sortBy" select="$sortBy"/>
			</xsl:apply-templates>
			<a href="{$parentURL}" rel="tag">
				<xsl:apply-templates select="Name" mode="displayBrief"/>
				<xsl:if test="@relatedCount!=''">
					&#160;(<xsl:value-of select="@relatedCount"/>)
				</xsl:if>
			</a>
			<xsl:if test="position()!=last()">
				<span class="tag-comma">
					<xsl:text>, </xsl:text>
				</span>
			</xsl:if>
		</span>
	</xsl:template>

	<!-- Tags Brief -->
	<xsl:template match="Content[@type='Tag']" mode="displayBriefNoLink">
		<xsl:param name="sortBy"/>
		<xsl:variable name="parentURL">
			<xsl:apply-templates select="." mode="getHref"/>
		</xsl:variable>
		<xsl:variable name="name" select="Name/node()"/>
		<span>
			<xsl:apply-templates select="." mode="inlinePopupOptions">
				<xsl:with-param name="sortBy" select="$sortBy"/>
			</xsl:apply-templates>
			<xsl:apply-templates select="Name" mode="displayBrief"/>
			<xsl:if test="@relatedCount!=''">
				&#160;(<xsl:value-of select="@relatedCount"/>)
			</xsl:if>
			<xsl:if test="position()!=last()">
				<span class="tag-comma">
					<xsl:text>, </xsl:text>
				</span>
			</xsl:if>
		</span>
	</xsl:template>

	<!-- Tags Detail -->
	<xsl:template match="Content[@type='Tag']" mode="ContentDetail">
		<xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
		<div class="detail tag">
			<xsl:apply-templates select="." mode="inlinePopupOptions">
				<xsl:with-param name="class" select="'detail tag'"/>
			</xsl:apply-templates>
			<h1>
				<xsl:value-of select="Name/node()"/>
			</h1>
			<div class="tags cols cols3">
				<xsl:apply-templates select="Content" mode="displayBrief">
					<xsl:sort select="@publish" order="descending"/>
				</xsl:apply-templates>
				<xsl:text> </xsl:text>
			</div>
			<!-- Terminus class fix to floating content -->
			<div class="terminus">&#160;</div>
			<div class="entryFooter">
				<div class="tags">
					<xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
				</div>
				<xsl:apply-templates select="." mode="backLink">
					<xsl:with-param name="link" select="$thisURL"/>
					<xsl:with-param name="altText">
						<!--click here to return to the tags list-->
						<xsl:call-template name="term2040" />
					</xsl:with-param>
				</xsl:apply-templates>
			</div>
			<div class="terminus">&#160;</div>
		</div>
	</xsl:template>
  
</xsl:stylesheet>
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
    
    <div class="SubPages">
      <div data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1" data-dots="{@carouselBullets}" data-height="{@carouselHeight}" >
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
          <xsl:with-param name="showHidden" select="@showHidden"/>
          <xsl:with-param name="fixedThumb" select="@fixedThumb"/>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <!-- Sub Page Content -->
  <xsl:template match="MenuItem" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <xsl:param name="crop"/>
    <xsl:param name="showHidden"/>
    <xsl:param name="fixedThumb"/>
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
    <xsl:if test="(@name!='Information' and (not(DisplayName/@exclude='true'))) or (@name!='Information' and $showHidden='true')">
      <div class="listItem subpageItem">
        <xsl:apply-templates select="." mode="inlinePopupOptions">
          <xsl:with-param name="class" select="'listItem subpageItem'"/>
          <xsl:with-param name="sortBy" select="$sortBy"/>
        </xsl:apply-templates>
        <div class="lIinner">
          <h3 class="title">
            <xsl:apply-templates select="." mode="menuLink"/>
          </h3>
          <xsl:if test="Images/img[@src!='']">
            <a href="{$url}" title="{$pageName}">
              <xsl:attribute name="title">
                <xsl:apply-templates select="." mode="getTitleAttr"/>
              </xsl:attribute>
              <xsl:apply-templates select="." mode="displaySubPageThumb">
                <xsl:with-param name="crop" select="$cropSetting" />
                <xsl:with-param name="fixedThumb" select="$fixedThumb" />
              </xsl:apply-templates>
            </a>
          </xsl:if>
          <span class="listDescription">
            <xsl:apply-templates select="Description/node()" mode="cleanXhtml" />
            <xsl:text> </xsl:text>
          </span>
          <div class="entryFooter">
            <xsl:apply-templates select="." mode="moreLink">
              <xsl:with-param name="linkText">
                <xsl:call-template name="term2026" />
                <xsl:text>&#160;</xsl:text>
                <xsl:apply-templates select="." mode="getDisplayName" />
              </xsl:with-param>
              <xsl:with-param name="link" select="$url"/>
              <xsl:with-param name="altText">
                <xsl:apply-templates select="." mode="getTitleAttr" />
              </xsl:with-param>
            </xsl:apply-templates>
            <xsl:text> </xsl:text>
          </div>
        </div>
      </div>
    </xsl:if>
  </xsl:template>

 


</xsl:stylesheet>
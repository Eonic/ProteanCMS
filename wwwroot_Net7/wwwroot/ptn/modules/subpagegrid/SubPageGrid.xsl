<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!--   #############   Sub Page Grid   ###########   -->

  <!-- Sub Page Grid Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='SubPageGrid']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="link" select="@link"/>
    <xsl:variable name="parentPage" select="//MenuItem[@id=$link]"/>
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
          <xsl:value-of select="count($parentPage/MenuItem)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count($currentPage/MenuItem)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    
    <div class="SubPages SubPageGrid Grid">
      <div class="cols{@cols}">
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
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="gridDisplayBrief">
          <xsl:with-param name="sortBy" select="@sortBy"/>
          <xsl:with-param name="crop" select="$cropSetting"/>
        </xsl:apply-templates>
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- Sub Page Grid Content -->
  <xsl:template match="MenuItem" mode="gridDisplayBrief">
    <xsl:param name="sortBy"/>
    <xsl:param name="crop"/>
    <xsl:variable name="url">
      <xsl:apply-templates select="." mode="getHref"/>
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
      <!--<xsl:value-of select="$crop"/>-->
    </xsl:variable>
    <xsl:variable name="lg-max-width">
      <xsl:apply-templates select="." mode="getFullSizeWidth"/>
    </xsl:variable>
    <xsl:variable name="lg-max-height">
      <xsl:apply-templates select="." mode="getFullSizeHeight"/>
    </xsl:variable>
    <xsl:if test="@name!='Information' and @name!='Footer' and not(DisplayName/@exclude='true')">
      <div class="grid-item">
        <xsl:apply-templates select="." mode="inlinePopupOptions">
          <xsl:with-param name="class" select="'grid-item hproduct'"/>
          <xsl:with-param name="sortBy" select="$sortBy"/>
        </xsl:apply-templates>
        <a href="{$url}" title="{@name}" class="url">
          <div class="thumbnail">
            <xsl:if test="Images/img[@src!='']">
              <xsl:apply-templates select="." mode="displayThumbnail">
                <xsl:with-param name="crop" select="$cropSetting" />
                <xsl:with-param name="class" select="'img-responsive'" />
                <xsl:with-param name="style" select="'overflow:hidden;'" />
                <xsl:with-param name="width" select="$lg-max-width"/>
                <xsl:with-param name="height" select="$lg-max-height"/>
              </xsl:apply-templates>
            </xsl:if>
            <xsl:if test="DisplayName/@icon!=''">
              <i>
                <xsl:attribute name="class">
                  <xsl:text>fa fa-3x center-block </xsl:text>
                  <xsl:value-of select="DisplayName/@icon"/>
                </xsl:attribute>
                <xsl:text> </xsl:text>
              </i>
              <xsl:text> </xsl:text>
            </xsl:if>
            <xsl:if test="DisplayName/@uploadIcon!='' and DisplayName/@uploadIcon!='_'">
              <span class="upload-icon">
                <img src="{DisplayName/@uploadIcon}" alt="icon" class="center-block img-responsive"/>
              </span>
            </xsl:if>
            <div class="caption">
              <h4>
                <xsl:apply-templates select="." mode="getDisplayName"/>
              </h4>
            </div>
          </div>

        </a>
      </div>
    </xsl:if>
  </xsl:template>
  
</xsl:stylesheet>
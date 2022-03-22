<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!--   #############   Sub Page Menu   ###########   -->
  <!-- Sub Page Menu Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='SubPageMenu']" mode="displayBrief">
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
    <div class="SubPageMenu">
      <div>
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="contentList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount" />
          </xsl:apply-templates>
        </xsl:if>
        <ul data-xscols="{$xsColsToShow}" data-smcols="{$smColsToShow}" data-mdcols="{$mdColsToShow}" data-slidestoshow="{@cols}">
          <!--responsive columns-->
          <xsl:attribute name="class">
            <xsl:text>nav flex-column cols</xsl:text>
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
          <xsl:if test="@homeLink='true'">
            <li class="first">
              <xsl:apply-templates select="$parentPage" mode="menuLink"/>
            </li>
          </xsl:if>
          <xsl:apply-templates select="ms:node-set($contentList)/*[not(DisplayName/@exclude='true')]" mode="displayMenuBrief">
            <xsl:with-param name="sortBy" select="@sortBy"/>
          </xsl:apply-templates>
          <xsl:text> </xsl:text>
        </ul>
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- Sub Page Menu Content -->
  <xsl:template match="MenuItem" mode="displayMenuBrief">
    <xsl:param name="sortBy"/>
    <xsl:variable name="url">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <xsl:variable name="pageName">
      <xsl:apply-templates select="." mode="getDisplayName"/>
    </xsl:variable>
    <xsl:if test="@name!='Information'">
      <li class="nav-item">
        <xsl:apply-templates select="." mode="menuLink">
          <xsl:with-param name="class">nav-link</xsl:with-param>
        </xsl:apply-templates>
      </li>
    </xsl:if>
  </xsl:template>

 
  
</xsl:stylesheet>
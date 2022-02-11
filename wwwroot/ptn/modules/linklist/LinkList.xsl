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
    <div class="clearfix Links">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix Links content-scroller</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <div class="cols cols{@cols}" data-xscols="{$xsColsToShow}" data-smcols="{$smColsToShow}" data-mdcols="{$mdColsToShow}" data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1" data-dots="{@carouselBullets}">
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
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <!-- Links Brief -->
  <xsl:template match="Content[@type='Link']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <xsl:param name="crop"/>
    <xsl:variable name="preURL" select="substring(Url,1,3)" />
    <xsl:variable name="url" select="Url/node()" />
    <xsl:variable name="linkURL">
      <xsl:choose>
        <xsl:when test="format-number($url,'0')!='NaN'">
          <xsl:apply-templates select="$page/descendant-or-self::MenuItem[@id=$url]" mode="getHref"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:if test="$preURL='www' or $preURL='WWW'">
            <xsl:text>http://</xsl:text>
          </xsl:if>
          <xsl:value-of select="$url"/>
        </xsl:otherwise>
      </xsl:choose>
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

    <div class="list-group-item listItem link">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'list-group-item listItem link'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="list-item lIinner">
        <h3 class="title">
          <a href="{$linkURL}" title="{Name}">
            <xsl:if test="not(substring(@linkURL,1,1)='/') and (contains(@linkURL,'http://') and Url/@type='external')">
              <xsl:attribute name="rel">external</xsl:attribute>
              <xsl:attribute name="class">extLink</xsl:attribute>
            </xsl:if>
            <xsl:value-of select="Name"/>
          </a>
        </h3>
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
        <xsl:if test="Body/node()!=''">
          <div class="description">
            <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
          </div>
        </xsl:if>
        <div class="entryFooter">
          <xsl:apply-templates select="." mode="displayTags"/>
          <xsl:apply-templates select="." mode="moreLink">
            <xsl:with-param name="link" select="$linkURL"/>
            <xsl:with-param name="linkType" select="Url/@type"/>
            <xsl:with-param name="altText">
              <xsl:value-of select="Name/node()"/>
            </xsl:with-param>
          </xsl:apply-templates>
          <xsl:text> </xsl:text>
        </div>
      </div>
    </div>
  </xsl:template>
</xsl:stylesheet>
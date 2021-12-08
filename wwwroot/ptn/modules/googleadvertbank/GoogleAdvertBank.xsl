<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!-- ## Google Adverts  ###############################################################################   -->
  <xsl:template match="Content[@type='Module' and @moduleType='GoogleAdvertBank']" mode="displayBrief">
    <xsl:variable name="GoogleAdManagerId" select="/Page/Contents/Content[@name='GoogleAdManagerId']/node()" />
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
    <div class="GoogleAdvertBank">
      <div class="cols{@cols}">
        <xsl:apply-templates select="." mode="inlinePopupRelate">
          <xsl:with-param name="type">GoogleAdvert</xsl:with-param>
          <xsl:with-param name="text">Add Advert</xsl:with-param>
          <xsl:with-param name="name"></xsl:with-param>
          <xsl:with-param name="find">true</xsl:with-param>
        </xsl:apply-templates>
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="articleList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:choose>
          <!-- WHEN NO ID - ADD BUTTON -->
          <xsl:when test="$GoogleAdManagerId=''">
            <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
              <xsl:with-param name="type">PlainText</xsl:with-param>
              <xsl:with-param name="text">Add Google Ad ID</xsl:with-param>
              <xsl:with-param name="name">GoogleAdManagerId</xsl:with-param>
            </xsl:apply-templates>
          </xsl:when>
          <!-- WHEN ID AND ADVERTS - INITIALISE and DISPLAY -->
          <xsl:when test="ms:node-set($contentList)/* and $GoogleAdManagerId!=''">
            <script type='text/javascript' src='//partner.googleadservices.com/gampad/google_service.js'>&#160;</script>
            <script type='text/javascript'>
              <xsl:text>GS_googleAddAdSenseService("</xsl:text>
              <xsl:value-of select="$GoogleAdManagerId"/>
              <xsl:text>");</xsl:text>
              <xsl:text>GS_googleEnableAllServices();</xsl:text>
            </script>
            <script type='text/javascript'>
              <xsl:for-each select="ms:node-set($contentList)/*">
                <xsl:text>GA_googleAddSlot("</xsl:text>
                <xsl:value-of select="$GoogleAdManagerId"/>
                <xsl:text>", "</xsl:text>
                <xsl:value-of select="@adname"/>
                <xsl:text>");</xsl:text>
              </xsl:for-each>
            </script>
            <xsl:if test="/Page/Request/Form/Item[@name='searchString']/node()!=''">
              <script type='text/javascript'>
                <xsl:text>GA_googleAddAttr("search", "</xsl:text>
                <xsl:value-of select="/Page/Request/Form/Item[@name='searchString']/node()"/>
                <xsl:text>");</xsl:text>
              </script>
            </xsl:if>
            <xsl:if test="/Page/ContentDetail/Content[@type='Category']">
              <script type='text/javascript'>
                <xsl:text>GA_googleAddAttr("category", "</xsl:text>
                <xsl:value-of select="/Page/ContentDetail/Content[@type='Category']/@name"/>
                <xsl:text>");</xsl:text>
              </script>
            </xsl:if>
            <script type='text/javascript'>GA_googleFetchAds();</script>
            <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
              <xsl:with-param name="GoogleAdManagerId" select="$GoogleAdManagerId"/>
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
          </xsl:when>
        </xsl:choose>
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='GoogleAdvert']" mode="displayBrief">
    <xsl:param name="GoogleAdManagerId"/>
    <xsl:param name="sortBy"/>
    <div class="listItem googleadvert">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem googleadvert'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <xsl:comment>
        <xsl:text> </xsl:text>
        <xsl:value-of select="$GoogleAdManagerId"/>
        <xsl:text>/</xsl:text>
        <xsl:value-of select="@adname"/>
        <xsl:text> </xsl:text>
      </xsl:comment>
      <xsl:choose>
        <xsl:when test="$page/@adminMode">
          <p>
            <xsl:text>Ad Name: '</xsl:text>
            <xsl:value-of select="@adname"/>
            <xsl:text>'</xsl:text>
          </p>
          <p>
            <xsl:text>Website Placement: '</xsl:text>
            <xsl:value-of select="@name"/>
            <xsl:text>'</xsl:text>
          </p>
          <p>
            <em>Adverts are disabled in admin to avoid false impressions and clicks.</em>
          </p>
        </xsl:when>
        <xsl:otherwise>
          <span class="adContainer" title="{@adname}">
            <script type='text/javascript'>
              <xsl:text>GA_googleFillSlot("</xsl:text>
              <xsl:value-of select="@adname"/>
              <xsl:text>");</xsl:text>
            </script>
          </span>
        </xsl:otherwise>
      </xsl:choose>
    </div>
  </xsl:template>
  
</xsl:stylesheet>
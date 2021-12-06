<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!-- ## Documents ###################################################################################   -->
  <!-- Document Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='DocumentList']" mode="displayBrief">
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
    <xsl:variable name="idsList">
      <xsl:apply-templates select="ms:node-set($contentList)/*" mode="idList" />
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
    <div class="Documents">
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
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="documentList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief" >
          <xsl:with-param name="sortBy" select="@sortBy"/>
          <xsl:with-param name="showThumbnail" select="@showThumbnails"/>
        </xsl:apply-templates>
        <div class="terminus">&#160;</div>
      </div>
      <xsl:if test="@allAsZip='on'">
        <div class="listItem list-group-item">
          <div class="lIinner">
            <a class="docLink zipicon" href="{$appPath}ewcommon/tools/download.ashx?docId={$idsList}&amp;filename=myzip.zip&amp;xPath=/Content/Path">
              <xsl:call-template name="term2074" />
            </a>
          </div>
        </div>
      </xsl:if>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='Document']" mode="idList">
    <xsl:if test="position()!=1">
      <xsl:text>,</xsl:text>
    </xsl:if>
    <xsl:value-of select="@id"/>
  </xsl:template>

  <!-- Document Brief -->
  <xsl:template match="Content[@type='Document']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <xsl:param name="showThumbnail"/>
    <!-- documentBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="self::Content" mode="getHref">
        <xsl:with-param name="parId" select="@parId"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="preURL" select="substring(Website,1,3)" />
    <xsl:variable name="linkURL">
      <xsl:choose>
        <xsl:when test="$preURL='www' or $preURL='WWW'">
          <xsl:text>http://</xsl:text>
          <xsl:value-of select="Url"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="Url"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div class="list-group-item listItem">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'list-group-item listItem'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <xsl:if test="$showThumbnail='true'">
          <xsl:apply-templates select="." mode="displayThumbnail"/>
        </xsl:if>
        <h3 class="title">
          <a rel="external">
            <xsl:if test="$GoogleAnalyticsUniversalID!=''">
              <xsl:attribute name="onclick">
                <xsl:text>ga('send', 'event', 'Document', 'download', 'document-</xsl:text>
                <xsl:value-of select="Title/node()"/>
                <xsl:text>');</xsl:text>
              </xsl:attribute>
            </xsl:if>
            <xsl:attribute name="href">
              <xsl:choose>
                <xsl:when test="contains(Path,'http://')">
                  <xsl:value-of select="Path/node()"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$appPath"/>
                  <xsl:text>ewcommon/tools/download.ashx?docId=</xsl:text>
                  <xsl:value-of select="@id"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
            <xsl:attribute name="title">
              <!-- click here to download a copy of this document -->
              <xsl:call-template name="term2027" />
            </xsl:attribute>
            <xsl:value-of select="Title/node()"/>
          </a>
        </h3>
        <xsl:if test="Body/node()!=''">
          <div class="description">
            <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
          </div>
        </xsl:if>
        <p class="link">
          <a rel="external" class="docLink {substring-after(Path,'.')}icon">
            <xsl:attribute name="href">
              <xsl:choose>
                <xsl:when test="contains(Path,'http://')">
                  <xsl:value-of select="Path/node()"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$appPath"/>
                  <xsl:text>ewcommon/tools/download.ashx?docId=</xsl:text>
                  <xsl:value-of select="@id"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
            <xsl:if test="$GoogleAnalyticsUniversalID!=''">
              <xsl:attribute name="onclick">
                <xsl:text>ga('send', 'event', 'Document', 'download', 'document-</xsl:text>
                <xsl:value-of select="Title/node()"/>
                <xsl:text>');</xsl:text>
              </xsl:attribute>
            </xsl:if>
            <xsl:choose>
              <xsl:when test="contains(Path,'http://')">
                <xsl:attribute name="title">
                  <!-- click here to view this document -->
                  <xsl:call-template name="term2027a" />
                </xsl:attribute>
                <xsl:value-of select="Path/node()"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:attribute name="title">
                  <!-- click here to download a copy of this document -->
                  <xsl:call-template name="term2027" />
                </xsl:attribute>
                <!--Download-->
                <xsl:call-template name="term2028" />
                <xsl:text>&#160;</xsl:text>
                <xsl:call-template name="getFileTypeName">
                  <xsl:with-param name="extension" select="concat('.',substring-after(Path,'.'))"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </a>
        </p>
        <!-- Terminus class fix to floating columns -->
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>
  
</xsl:stylesheet>
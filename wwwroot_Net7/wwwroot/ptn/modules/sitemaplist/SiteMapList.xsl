<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
 
   <!--  ##########################################################################################   -->
  <!--  ## Site Map Templates  ###################################################################   -->
  <!--  ##########################################################################################   -->
  <!-- Site Map Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='SiteMapList']" mode="displayBrief">
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
    <!-- Output Module -->
    <div class="SiteMapList">
      <ul class="sitemap">
        <xsl:apply-templates select="/Page/Menu/MenuItem" mode="sitemap">
          <xsl:with-param name="level">1</xsl:with-param>
          <xsl:with-param name="bDescription">
            <xsl:value-of select="@displayDescription"/>
          </xsl:with-param>
          <xsl:with-param name="showHiddenPages">
            <xsl:value-of select="@showHiddenPages"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </ul>
      <div class="terminus">&#160;</div>
    </div>
  </xsl:template>

  <!-- Site Map Menu Display -->
  <xsl:template match="MenuItem" mode="sitemap">
    <xsl:param name="level"/>
    <xsl:param name="bDescription"/>
    <xsl:param name="showHiddenPages"/>
    <li>
      <xsl:apply-templates select="." mode="menuLink"/>
      <xsl:if test="$bDescription='true' and Description/node()">
        <p>
          <xsl:apply-templates select="Description/node()" mode="flattenXhtml" />
        </p>
      </xsl:if>
    </li>
    <xsl:if test="MenuItem">
      <xsl:apply-templates select="MenuItem" mode="sitemapSubLevel">
        <xsl:with-param name="level">
          <xsl:value-of select="$level"/>
        </xsl:with-param>
        <xsl:with-param name="bDescription" select="$bDescription" />
        <xsl:with-param name="showHiddenPages" select="$showHiddenPages" />
      </xsl:apply-templates>
    </xsl:if>
  </xsl:template>

  <!-- Site Map Sub Menu Display -->
  <xsl:template match="MenuItem" mode="sitemapSubLevel">
    <xsl:param name="level"/>
    <xsl:param name="bDescription" />
    <xsl:param name="showHiddenPages" />
    <xsl:variable name="url">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <xsl:variable name="displayName">
      <xsl:apply-templates select="." mode="getDisplayName"/>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$showHiddenPages='true'">
        <li>
          <xsl:apply-templates select="." mode="menuLink"/>
          <xsl:if test="$bDescription='true' and Description/node()">
            <p>
              <xsl:apply-templates select="Description/node()" mode="flattenXhtml" />
            </p>
          </xsl:if>
          <xsl:if test="MenuItem">
            <ul>
              <xsl:apply-templates select="MenuItem" mode="sitemapSubLevel">
                <xsl:with-param name="level">
                  <xsl:value-of select="$level+1"/>
                </xsl:with-param>
                <xsl:with-param name="bDescription" select="$bDescription" />
                <xsl:with-param name="showHiddenPages" select="$showHiddenPages" />
              </xsl:apply-templates>
            </ul>
          </xsl:if>
        </li>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <!-- when this page excluded from Nav, its children may not be. -->
          <xsl:when test="DisplayName/@exclude='true'">
            <xsl:apply-templates select="MenuItem" mode="sitemapSubLevel">
              <xsl:with-param name="level" select="$level"/>
              <xsl:with-param name="bDescription" select="$bDescription" />
            </xsl:apply-templates>
          </xsl:when>
          <!-- otherwise normal behaviour -->
          <xsl:otherwise>
            <li>
              <xsl:apply-templates select="." mode="menuLink"/>
              <xsl:if test="$bDescription='true' and Description/node()">
                <p>
                  <xsl:apply-templates select="Description/node()" mode="flattenXhtml" />
                </p>
              </xsl:if>
              <xsl:if test="MenuItem">
                <ul>
                  <xsl:apply-templates select="MenuItem" mode="sitemapSubLevel">
                    <xsl:with-param name="level">
                      <xsl:value-of select="$level+1"/>
                    </xsl:with-param>
                    <xsl:with-param name="bDescription" select="$bDescription" />
                  </xsl:apply-templates>
                </ul>
              </xsl:if>
            </li>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
</xsl:stylesheet>
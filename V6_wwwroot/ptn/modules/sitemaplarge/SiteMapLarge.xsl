<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
 
   <!--  ==  Sitemap Large  ==========================================================================  -->

  <xsl:template match="Content[@moduleType='SiteMapLarge']" mode="displayBrief">
    <div class="Sitemap">
      <xsl:apply-templates select="/Page/Menu" mode="sitemapList">
        <xsl:with-param name="cols" select="@cols"/>
        <xsl:with-param name="descriptions" select="@descriptions = 'true'"/>
      </xsl:apply-templates>
    </div>
  </xsl:template>

  <!-- List -->
  <xsl:template match="*" mode="sitemapList"/>

  <xsl:template match="Menu | MenuItem[not(DisplayName/@exclude = 'true')]" mode="sitemapList">
    <xsl:param name="level" select="1"/>
    <xsl:param name="cols" select="1"/>
    <xsl:param name="descriptions" select="false()"/>
    <xsl:if test="MenuItem[not(DisplayName/@exclude = 'true')]">
      <div>
        <xsl:attribute name="class">
          <xsl:text>list listLevel</xsl:text>
          <xsl:value-of select="$level"/>
          <xsl:if test="$level = 2">
            <xsl:text> cols</xsl:text>
            <xsl:value-of select="$cols"/>
          </xsl:if>
        </xsl:attribute>
        <xsl:apply-templates select="MenuItem" mode="sitemapItem">
          <xsl:with-param name="level" select="$level"/>
          <xsl:with-param name="cols" select="$cols"/>
          <xsl:with-param name="descriptions" select="$descriptions"/>
        </xsl:apply-templates>
        <div class="terminus">&#160;</div>
      </div>
    </xsl:if>
  </xsl:template>

  <!-- Item -->
  <xsl:template match="*" mode="sitemapItem"/>

  <xsl:template match="MenuItem[not(DisplayName/@exclude = 'true')]" mode="sitemapItem">
    <xsl:param name="level" select="1"/>
    <xsl:param name="cols" select="1"/>
    <xsl:param name="descriptions" select="false()"/>
    <xsl:variable name="home" select="@id = /Page/Menu/MenuItem/@id"/>
    <xsl:variable name="levelClass" select="concat('Level', $level)"/>
    <div>
      <xsl:attribute name="class">
        <xsl:text>item item</xsl:text>
        <xsl:value-of select="$levelClass"/>
        <xsl:if test="$level = 2">
          <xsl:text> listItem</xsl:text>
        </xsl:if>
      </xsl:attribute>
      <div>
        <xsl:if test="$level = 2">
          <xsl:attribute name="class">
            <xsl:text>lIinner</xsl:text>
          </xsl:attribute>
        </xsl:if>
        <div class="heading heading{$levelClass}">
          <a class="link link{$levelClass}">
            <xsl:attribute name="href">
              <xsl:apply-templates select="." mode="getHref"/>
            </xsl:attribute>
            <xsl:apply-templates select="." mode="getDisplayName"/>
          </a>
          <xsl:if test="$descriptions and Description != ''">
            <div class="description description{$levelClass}">
              <xsl:copy-of select="Description/*"/>
            </div>
          </xsl:if>
        </div>
        <xsl:if test="not($home)">
          <xsl:apply-templates select="." mode="sitemapList">
            <xsl:with-param name="level" select="$level + 1"/>
            <xsl:with-param name="cols" select="$cols"/>
            <xsl:with-param name="descriptions" select="$descriptions"/>
          </xsl:apply-templates>
        </xsl:if>
        <div class="terminus">&#160;</div>
      </div>
    </div>
    <xsl:if test="$home">
      <xsl:apply-templates select="MenuItem" mode="sitemapItem">
        <xsl:with-param name="level" select="$level"/>
        <xsl:with-param name="cols" select="$cols"/>
        <xsl:with-param name="descriptions" select="$descriptions"/>
      </xsl:apply-templates>
    </xsl:if>
  </xsl:template>

  
</xsl:stylesheet>
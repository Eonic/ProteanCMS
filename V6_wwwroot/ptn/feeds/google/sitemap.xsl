<?xml version="1.0" encoding="utf-8"?>

<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns:ew="urn:ew">
	<xsl:import href="../../tools/Functions.xsl"/>
  <xsl:import href="../../localisation/SystemTranslations.xsl"/>
  <xsl:output method="xml" version="1.0" encoding="utf-8" indent="no"/>
  
  <xsl:variable name="httpPrefix">
    <xsl:choose>
      <xsl:when test="/Page/Request/ServerVariables/Item[@name='HTTPS']='on'">
        <xsl:text>https://</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>http://</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="siteURL" select="concat($httpPrefix,/Page/Request/ServerVariables/Item[@name='SERVER_NAME'])"/>
  
	<xsl:variable name="baseURL">
		<xsl:text>http://</xsl:text>
		<xsl:value-of select="/Page/Request/ServerVariables/Item[@name='SERVER_NAME']"/>
	</xsl:variable>
	<xsl:variable name="today" select="/Page/Request/ServerVariables/Item[@name='Date']"/>

	<xsl:template match="Page">
		
		<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9"
  xmlns:image="http://www.google.com/schemas/sitemap-image/1.1"
  xmlns:video="http://www.google.com/schemas/sitemap-video/1.1">
      <xsl:apply-templates select="Menu/MenuItem" mode="listPages">
				<xsl:with-param name="level" select="10"/>
			</xsl:apply-templates>
		</urlset>
		
	</xsl:template>

	<xsl:template match="MenuItem" mode="listPages">
		<xsl:param name="level"/>
		<xsl:if test="not(DisplayName/@noindex='true')">
      <url xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
				<loc>
          <xsl:apply-templates select="." mode="getHref">
            <xsl:with-param name="absoluteURL" select="true()" />
          </xsl:apply-templates>
				</loc>
        <!--priority>
					<xsl:choose>

						<xsl:when test="$level=10">
							<xsl:text>1.0</xsl:text>
						</xsl:when>
     
            <xsl:when test="not(node())">
              <xsl:value-of select="format-number(($level + 1) div 10,'0.0')"/>
            </xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="format-number($level div 10,'0.0')"/>
						</xsl:otherwise>
					</xsl:choose>
				</priority-->
        
        <!-- better to not state than to be incorrect -->
				<!--<lastmod>
					<xsl:value-of select="$today"/>
				</lastmod>
				<changefreq>weekly</changefreq>-->
			</url>
      
		</xsl:if>
		<xsl:if test="count(child::MenuItem)&gt;0">
			<xsl:apply-templates select="MenuItem[not(contains(@url, 'http'))]" mode="listPages">
				<xsl:with-param name="level">
          <xsl:choose>
            <!-- if no index, don't reduce priority for children -->
            <xsl:when test="$level=1 or DisplayName/@noindex='true'">
              <xsl:value-of select="$level"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$level - 1"/>
            </xsl:otherwise>
          </xsl:choose>
				</xsl:with-param>
			</xsl:apply-templates>
		</xsl:if> 
	</xsl:template>

  <!-- Match on Menu Item - Build URL for that MenuItem -->
  <xsl:template match="MenuItem" mode="getHref">
    <!-- absolute url false by default -->
    <xsl:param name="absoluteURL" select="false()" />

    <xsl:variable name="url" select="@url"/>

    <xsl:choose>
      <xsl:when test="@url!=''">
        <xsl:choose>
          <xsl:when test="format-number(@url,'0')!='NaN'">
            <xsl:value-of select="$siteURL"/>
            <xsl:value-of select="$page/Menu/descendant-or-self::MenuItem[@id=$url]/@url"/>
          </xsl:when>
          <xsl:when test="contains(@url,'http')">
            <xsl:value-of select="@url"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$siteURL"/>
            <xsl:value-of select="@url"/>
            <xsl:value-of select="/Page/@pageExt"/>
            <xsl:if test="/Page/@adminMode and /Page/@pageExt!=''">
              <xsl:text>?pgid=</xsl:text>
              <xsl:value-of select="@id"/>
            </xsl:if>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>/</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!-- Overwrite for content menuItems - Not needed, Componant update fixed issue this is correcting. -->
  <!--<xsl:template match="MenuItem[not(node())]" mode="listPages">
    <xsl:param name="level"/>

    <xsl:variable name="url">
      <xsl:value-of select="ew:replacestring(@url,'/Item','/')"/>
      <xsl:text>-/</xsl:text>
      <xsl:value-of select="translate(@name,' /\.','')"/>
    </xsl:variable>

    <xsl:if test="not(contains(@url, 'http'))">
      <url xmlns="http://www.google.com/schemas/sitemap/0.84">
        <loc>
          <xsl:value-of select="$baseURL"/>
          <xsl:value-of select="$url"/>
        </loc>
        <priority>
          <xsl:choose>
            <xsl:when test="$level=10">
              <xsl:text>1.0</xsl:text>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$level div 10"/>
            </xsl:otherwise>
          </xsl:choose>
        </priority>
      </url>
    </xsl:if>
  </xsl:template>-->
</xsl:stylesheet> 


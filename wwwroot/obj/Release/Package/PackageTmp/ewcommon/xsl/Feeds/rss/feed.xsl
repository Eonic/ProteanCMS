<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"  xmlns:date= "http://exslt.org/dates-and-times"  extension-element-prefixes="date" xmlns:atom="http://www.w3.org/2005/Atom" xmlns:ew="urn:ew">
  <xsl:import href="../../../../ewcommon_v5-1/xsl/tools/Functions.xsl"/>
  <xsl:import href="../../../../ewcommon_v5-1/xsl/localisation/SystemTranslations.xsl"/>


  <xsl:output method="xml" omit-xml-declaration="no" indent="yes"/>

  <xsl:variable name="siteURL">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'BaseUrl'"/>
    </xsl:call-template>
  </xsl:variable>
	
  
  <!-- -->
  
  <xsl:template match="Page">
    <xsl:choose>
      <xsl:when test="Request//Item[@name='xmlFeed']">
        <xmlFeed>
          <rss version="2.0" xmlns:atom="http://www.w3.org/2005/Atom">
            <xsl:apply-templates select="Contents/Content/channel"/>
          </rss>
        </xmlFeed>
      </xsl:when>
      <xsl:otherwise>
        <rss version="2.0" xmlns:atom="http://www.w3.org/2005/Atom">
          <xsl:apply-templates select="Contents/Content/channel"/>
        </rss>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="channel">
    <xsl:variable name="type" select="../contentType"/>
    <channel>
      <xsl:if test="link">
        <atom:link href="{link}" rel="self" type="application/rss+xml" />
      </xsl:if>
      <!-- work our the last build date -->
      <lastBuildDate>
        <xsl:for-each select="/Page/Contents/Content[@type=$type]">
          <xsl:sort select="@update" data-type="text" order="descending"/>
          <xsl:if test="position()=1">
            <xsl:call-template name="formatdate">
              <xsl:with-param name="date" select="@update"/>
              <xsl:with-param name="format" select="'r'"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:for-each>
      </lastBuildDate>

		
		
      <xsl:apply-templates mode="cleanXhtml" select="*[(node()!='' or (local-name='cloud' and @*!='')) or (local-name()='image' and url!='')]"/>


      <xsl:choose>
        <xsl:when test="$type='All'">
          <xsl:apply-templates select="/Page/Contents/Content[@type='NewsArticle']" mode="feedItem"/>
        </xsl:when>
        <xsl:when test="$type='NewsArticle'">
          <xsl:apply-templates select="/Page/Contents/Content[@type='NewsArticle']" mode="feedItem">
            <xsl:sort select="@update" data-type="text" order="descending"/>
          </xsl:apply-templates>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="/Page/Contents/Content[@type=$type]" mode="feedItem"/>
        </xsl:otherwise>
      </xsl:choose>
    </channel>
  </xsl:template>

  <xsl:template match="*" mode="cleanXhtml">
    <xsl:element name="{local-name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:attribute name="{local-name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="cleanXhtml"/>
    </xsl:element>
  </xsl:template>
  <!-- -->

	<xsl:template match="image" mode="cleanXhtml">
			<xsl:if test="url/node() !=''">
			<xsl:element name="{local-name()}">
			<!-- process attributes -->
			<xsl:for-each select="@*">
				<!-- remove attribute prefix (if any) -->
				<xsl:attribute name="{local-name()}">
					<xsl:value-of select="." />
				</xsl:attribute>
			</xsl:for-each>
			<xsl:apply-templates mode="cleanXhtml"/>
		</xsl:element>
			</xsl:if>
	</xsl:template>
	<!-- -->

	<xsl:template match="url" mode="cleanXhtml">
		<xsl:if test="./node() !=''">
			<xsl:element name="{local-name()}">
				<!-- process attributes -->
				<xsl:for-each select="@*">
					<!-- remove attribute prefix (if any) -->
					<xsl:attribute name="{local-name()}">
						<xsl:value-of select="." />
					</xsl:attribute>
				</xsl:for-each>
				<xsl:choose>
					<xsl:when test="contains(.,'http://')">
						<xsl:apply-templates mode="cleanXhtml"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:choose>
							<xsl:when test="starts-with(.,'/')">
								<xsl:value-of select="$siteURL"/>
								<xsl:apply-templates mode="cleanXhtml"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="$siteURL"/>
								<xsl:text>/</xsl:text>
								<xsl:apply-templates mode="cleanXhtml"/>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:element>
		</xsl:if>
	</xsl:template>

	<!-- -->
  <xsl:template match="Content" mode="feedItem">
    <xsl:variable name="link">
      <xsl:value-of select="$siteURL"/>
      <xsl:call-template name="getContentParURL">
        <xsl:with-param name="parId">
          <xsl:value-of select="/Page/@id"/>
        </xsl:with-param>
      </xsl:call-template>&amp;artid=<xsl:value-of select="@id"/>
    </xsl:variable>
    <xsl:variable name="body" select="Body"/>
    <item>
      <guid>
        <xsl:value-of select="$link"/>
      </guid>
      <title>
        <xsl:value-of select="Headline"/>
      </title>
      <link>
        <xsl:value-of select="$link"/>
      </link>
      <description>
        <xsl:value-of select="ew:FlattenNodeXml($body)"/>
      </description>
      <pubDate>
        <xsl:call-template name="formatdate">
          <xsl:with-param name="date" select="@update"/>
          <xsl:with-param name="format" select="'r'"/>
        </xsl:call-template>
      </pubDate>
    </item>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[@type='NewsArticle']" mode="feedItem">
    <xsl:variable name="link">
      <xsl:call-template name="getSiteURL"/>
		<xsl:value-of select="$siteURL"/>
		<xsl:apply-templates select="." mode="getHref">
			<xsl:with-param name="parId" select="@parId"/>
		</xsl:apply-templates>
	</xsl:variable>
	<xsl:variable name="body" select="Strapline/node()"/>
    <item>
      <guid>
        <xsl:value-of select="$link"/>
      </guid>
      <title>
        <xsl:value-of select="Headline"/>
      </title>
      <link>
        <xsl:value-of select="$link"/>
      </link>
      <description>
        <xsl:value-of select="ew:FlattenNodeXml($body)"/>
		  <!--<xsl:value-of select="Strapline/node()"/>-->
      </description>
      <pubDate>
        <xsl:call-template name="formatdate">
          <xsl:with-param name="date" select="@update"/>
          <xsl:with-param name="format" select="'r'"/>
        </xsl:call-template>
      </pubDate>
    </item>
  </xsl:template>



  <xsl:template match="*" mode="encode">
    &lt;<xsl:value-of select="local-name()"/><xsl:for-each select="@*[name()!='xmlns']">
      &#160;<xsl:value-of select="local-name()"/>="<xsl:value-of select="." />"
    </xsl:for-each>&gt;<xsl:apply-templates mode="encode"/>&lt;/<xsl:value-of select="local-name()"/>&gt;
  </xsl:template>

  <xsl:template match="img" mode="encode">
    &lt;<xsl:value-of select="local-name()"/><xsl:for-each select="@*[name()!='xmlns']">
      &#160;<xsl:value-of select="local-name()"/>="<xsl:value-of select="." />"
    </xsl:for-each>/&gt;
  </xsl:template>

</xsl:stylesheet>


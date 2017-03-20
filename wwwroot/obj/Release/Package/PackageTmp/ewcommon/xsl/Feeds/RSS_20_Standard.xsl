<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes">
  <xsl:import href="../Tools/Functions.xsl"/>
  <xsl:output method="xml" indent="no" standalone="yes" omit-xml-declaration="yes" encoding="UTF-8"/>
	
  <!--RSS 2.0-->
  <xsl:template match="/rss[@version='2.0']">
    <Feed>
      <xsl:apply-templates select="channel/item" mode="RSS20"/>
    </Feed>
  </xsl:template>
  <!-- -->
  <xsl:template match="item" mode="RSS20">
    <xsl:variable name="cGUID">
      <xsl:value-of select="substring(guid,1,254)"/>
    </xsl:variable>
    <instance>
      <tblContent>
        <nContentKey/>
        <nContentPrimaryId/>
        <nVersion/>
        <cContentForiegnRef>
          <xsl:value-of select="$cGUID"/>
        </cContentForiegnRef>
        <cContentName>
          <xsl:value-of select="title"/>
        </cContentName>
        <cContentSchemaName>FeedItem</cContentSchemaName>
        <cContentXmlBrief>
          <Content>
            <guid>
              <xsl:value-of select="$cGUID"/>
            </guid>
            <url/>
            <Title>
              <xsl:value-of select="title"/>
            </Title>
            <Body>
				<xsl:apply-templates mode="cleanXhtml" select="description/node()"/>
            </Body>
            <Link>
              <xsl:value-of select="link"/>
            </Link>
            <PublishDate>
              <xsl:value-of select="pubDate"/>
            </PublishDate>
            <Category domain="{category/@domain}">
              <xsl:value-of select="category"/>
            </Category>
            <Document url="{enclosure/@url}" length="{enclosure/@length}" type="{enclosure/@type}"/>
            <Images/>
          </Content>
        </cContentXmlBrief>
        <cContentXmlDetail>
          <Content/>
        </cContentXmlDetail>
        <nAuditId/>
        <nAuditKey/>
        <dPublishDate>
          <xsl:value-of select="pubDate"/>
        </dPublishDate>
        <dExpireDate/>
        <dInsertDate/>
        <nInsertDirId/>
        <dUpdateDate/>
        <nUpdateDirId/>
        <nStatus>1</nStatus>
        <cDescription/>
      </tblContent>
    </instance>
  </xsl:template>

  <!--RSS 0.91-->
  <xsl:template match="/rss[@version='0.91']">
    <Feed>
      <xsl:apply-templates select="channel/item" mode="RSS091"/>
    </Feed>
  </xsl:template>
  
  <xsl:template match="item" mode="RSS091">
    <xsl:variable name="cGUID">
      <xsl:value-of select="title"/>
    </xsl:variable>
    <instance>
      <tblContent>
        <nContentKey/>
        <nContentPrimaryId/>
        <nVersion/>
        <cContentForiegnRef>
          <xsl:value-of select="$cGUID"/>
        </cContentForiegnRef>
        <cContentName>
          <xsl:value-of select="title"/>
        </cContentName>
        <cContentSchemaName>FeedItem</cContentSchemaName>
        <cContentXmlBrief>
          <Content>
            <guid>
              <xsl:value-of select="$cGUID"/>
            </guid>
            <url/>
            <Title>
              <xsl:value-of select="title"/>
            </Title>
            <Body>
				<xsl:apply-templates mode="cleanXhtml" select="description/node()"/>
			</Body>
			  <Link>
				  <xsl:value-of select="link"/>
			  </Link>
			  <PublishDate/>
			  <Category/>
			  <Document/>
			  <Images/>
		  </Content>
	  </cContentXmlBrief>
	  <cContentXmlDetail>
		  <Content/>
	  </cContentXmlDetail>
	  <nAuditId/>
	  <nAuditKey/>
	  <dPublishDate/>
	  <dExpireDate/>
	  <dInsertDate/>
	  <nInsertDirId/>
	  <dUpdateDate/>
	  <nUpdateDirId/>
	  <nStatus>1</nStatus>
	  <cDescription/>
  </tblContent>
</instance>
</xsl:template>

<!--RSS 0.92-->
<xsl:template match="/rss[@version='0.92']">
<Feed>
  <xsl:apply-templates select="channel/item" mode="RSS092"/>
</Feed>
</xsl:template>
<xsl:template match="item" mode="RSS092">
<xsl:variable name="cGUID">
  <xsl:value-of select="title"/>
</xsl:variable>
<instance>
  <tblContent>
	  <nContentKey/>
	  <nContentPrimaryId/>
	  <nVersion/>
	  <cContentForiegnRef>
		  <xsl:value-of select="$cGUID"/>
	  </cContentForiegnRef>
	  <cContentName>
		  <xsl:value-of select="title"/>
	  </cContentName>
	  <cContentSchemaName>FeedItem</cContentSchemaName>
	  <cContentXmlBrief>
		  <Content>
			  <guid>
				  <xsl:value-of select="$cGUID"/>
			  </guid>
			  <url/>
			  <Title>
				  <xsl:value-of select="title"/>
			  </Title>
			  <Body>
				  <xsl:apply-templates mode="cleanXhtml" select="description/node()"/>
			  </Body>
            <Link>
              <xsl:value-of select="link"/>
            </Link>
            <PublishDate/>
            <Category/>
            <Document url="{enclosure/@url}" length="{enclosure/@length}" type="{enclosure/@type}"/>
            <Images/>
          </Content>
        </cContentXmlBrief>
        <cContentXmlDetail>
          <Content/>
        </cContentXmlDetail>
        <nAuditId/>
        <nAuditKey/>
        <dPublishDate/>
        <dExpireDate/>
        <dInsertDate/>
        <nInsertDirId/>
        <dUpdateDate/>
        <nUpdateDirId/>
        <nStatus>1</nStatus>
        <cDescription/>
      </tblContent>
    </instance>
  </xsl:template>

  <!--Atom 1.0?-->
  <xsl:template match="/feed">
    <Feed>
      <xsl:apply-templates select="entry" mode="atom"/>
    </Feed>
  </xsl:template>
  <xsl:template match="entry" mode="atom">
    <xsl:variable name="cGUID">
      <xsl:value-of select="id"/>
    </xsl:variable>
    <instance>
      <tblContent>
        <nContentKey/>
        <nContentPrimaryId/>
        <nVersion/>
        <cContentForiegnRef>
          <xsl:value-of select="$cGUID"/>
        </cContentForiegnRef>
        <cContentName>
          <xsl:value-of select="title"/>
        </cContentName>
        <cContentSchemaName>FeedItem</cContentSchemaName>
        <cContentXmlBrief>
          <Content>
            <guid>
              <xsl:value-of select="$cGUID"/>
            </guid>
            <url/>
            <Title>
              <xsl:value-of select="title"/>
            </Title>
            <Body>
				<xsl:apply-templates mode="cleanXhtml" select="content/node()"/>
			</Body>
            <Link/>
            <PublishDate>
              <xsl:value-of select="issued"/>
            </PublishDate>
            <Category/>
            <Document/>
            <Images/>
          </Content>
        </cContentXmlBrief>
        <cContentXmlDetail>
          <Content/>
        </cContentXmlDetail>
        <nAuditId/>
        <nAuditKey/>
        <dPublishDate>
          <xsl:value-of select="issued"/>
        </dPublishDate>
        <dExpireDate/>
        <dInsertDate/>
        <nInsertDirId/>
        <dUpdateDate/>
        <nUpdateDirId/>
        <nStatus>1</nStatus>
        <cDescription/>
      </tblContent>
    </instance>
  </xsl:template>

</xsl:stylesheet>
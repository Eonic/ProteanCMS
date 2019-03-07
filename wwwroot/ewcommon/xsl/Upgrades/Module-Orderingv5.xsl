<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes">
  <xsl:output method="xml" indent="no" standalone="yes" omit-xml-declaration="yes" encoding="UTF-8"/>
  
  <!--  
        THIS FILE IS THE DEFAULT CONTENT UPGRADE FILE.
        IT CURRENTLY WRITES THE NODES BACK EXACTLY AS THEY COME OUT.
        YOU CAN JUMP IN AND ALTER ANY NODE() BY OVERWRITING THE mode="writeNodes" TEMPLATE,
        USING A DIFFERENT NODE MATCH
        
        **NB. MAKE SURE THERE ARE NO EXTRA LINES AFTER THE LAST TAG AS THIS WILL CAUSE AN ERROR**
  -->

  <xsl:template match="/instance">
    <instance>
      <xsl:for-each select="*">
        <xsl:apply-templates select="." mode="writeNodes"/>
      </xsl:for-each>
    </instance>
  </xsl:template>

  <!-- -->

  <xsl:template match="*" mode="writeNodes">
    <xsl:element name="{local-name()}">
      <xsl:for-each select="@*">
        <xsl:attribute name="{local-name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>
  
  <!-- -->

  <xsl:template match="Content" mode="writeNodes">
    <xsl:element name="{local-name()}">
      
      <xsl:for-each select="@*">
        <xsl:choose>

          <xsl:when test="name()='orderBy'">
            <xsl:attribute name="sortBy">
              <xsl:choose>
                <xsl:when test=".='None'">Position</xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="."/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
            <xsl:attribute name="order">ascending</xsl:attribute>
            <xsl:attribute name="contentType">
              <xsl:call-template name="getContentType">
                <xsl:with-param name="moduleType" select="//Content/@moduleType" />
              </xsl:call-template>
            </xsl:attribute>
          </xsl:when>

          <xsl:when test="name()='displayTitle'"></xsl:when>
          <xsl:when test="name()='title' and parent::Content/@displayTitle='false'">
            <xsl:attribute name="title"></xsl:attribute>
          </xsl:when>

          <xsl:otherwise>
            <xsl:attribute name="{local-name()}">
              <xsl:value-of select="." />
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>

      </xsl:for-each>

      <xsl:choose>
        <xsl:when test="@moduleType='EmailForm'">
          <xsl:copy-of select="*"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates mode="writeNodes"/>
        </xsl:otherwise>
      </xsl:choose>

    </xsl:element>
  </xsl:template>

  <!-- -->

  <xsl:template match="img" mode="writeNodes">
    <img src="{@src}" width="{@width}" height="{@height}" alt="{@alt}" class="{@class}"/>
  </xsl:template>

  <!-- -->

  <xsl:template match="br" mode="writeNodes">
    <br/>
  </xsl:template>

  <!-- NEEDED TO NOT STOP XFORMS FROM WORKING -->
  <xsl:template match="*[name='emailer'] | emailer" mode="writeNodes">
    <emailer xmlns="http://www.eonic.co.uk/ewcommon/Services">
      <xsl:apply-templates mode="writeNodes"/>
    </emailer>
  </xsl:template>


  <xsl:template name="getContentType">
    <xsl:param name="moduleType"/>
    <xsl:choose>
      <xsl:when test="$moduleType='NewsList'">NewsArticle</xsl:when>
      <xsl:when test="$moduleType='ContactList'">Contact</xsl:when>
      <xsl:when test="$moduleType='DocumentList'">Document</xsl:when>
      <xsl:when test="$moduleType='EventList'">Event</xsl:when>
      <xsl:when test="$moduleType='LinkList'">Link</xsl:when>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
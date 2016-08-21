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
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:attribute name="{local-name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>
  
  <xsl:template match="cContentSchemaName" mode="writeNodes">
    <cContentSchemaName>Image</cContentSchemaName>
  </xsl:template>
  <!-- -->

  <xsl:template match="img" mode="writeNodes">
    <img src="{@src}" width="{@width}" height="{@height}" alt="{@alt}" class="{@class}" />
  </xsl:template>

  <!-- -->

  <xsl:template match="br" mode="writeNodes">
    <br/>
  </xsl:template>

  <!-- -->

  <xsl:template match="cContentXmlBrief/Content" mode="writeNodes">
    <xsl:element name="{local-name()}">
      <xsl:for-each select="@*">
        <xsl:attribute name="{local-name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:attribute name="internalLink"></xsl:attribute>
      <xsl:attribute name="externalLink"></xsl:attribute>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>

  
  <!-- If In header, Footer or Column, convert to Module -->
  <!-- BESPOKEYNESS -->
  <xsl:template match="cContentSchemaName[//cContentName='header' or //cContentName='footer' or contains(//cContentName,'column')]" mode="writeNodes">
    <xsl:element name="{name()}">Module</xsl:element>
  </xsl:template>
  <!-- -->
  <xsl:template match="tblContent[//cContentName='header' or //cContentName='footer' or contains(//cContentName,'column')]" mode="writeNodes">
    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
      <xsl:if test="not(bCascade)">
        <bCascade/>
      </xsl:if>
    </xsl:element>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[//cContentName='header' or //cContentName='footer' or contains(//cContentName,'column')]" mode="writeNodes">
    <Content moduleType="Image" box="" title="" linkType="internal" >
      <xsl:attribute name="position">
        <xsl:value-of select="//cContentName"/>
      </xsl:attribute>


      <xsl:choose>
        <xsl:when test="Link/@pageId!=''">
          <xsl:attribute name="link">
            <xsl:value-of select="Link/@pageId"/>
          </xsl:attribute>
          <xsl:attribute name="linkType">
            <xsl:text>internal</xsl:text>
          </xsl:attribute>
        </xsl:when>
        <xsl:when test="Link/node()!=''">
          <xsl:attribute name="link">
            <xsl:value-of select="Link/node()"/>
          </xsl:attribute>
          <xsl:attribute name="linkType">
            <xsl:text>external</xsl:text>
          </xsl:attribute>
        </xsl:when>
      </xsl:choose>
    
      
      <xsl:apply-templates select="img" mode="writeNodes"/>
    </Content>
  </xsl:template>
  
</xsl:stylesheet>
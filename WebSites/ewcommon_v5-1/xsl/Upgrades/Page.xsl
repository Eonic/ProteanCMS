<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns:ew="urn:ew">
  <xsl:output method="xml" indent="no" standalone="yes" omit-xml-declaration="yes" encoding="UTF-8"/>

  <!--  
        FOR A LONG TIME - The setup routine setup the first 5 pages missing alot of nodes.
        This file should be kept up to date with the Page.xml     
        We can run this to rectify the problem and add any new developments for Page.
        WH - 2011-02-24
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
    <xsl:element name="{name()}">
      <xsl:for-each select="@*">
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>

  <!-- -->

  <xsl:template match="img" mode="writeNodes">
    <xsl:variable name="img">
      <xsl:element name="{name()}">
        <xsl:for-each select="@*">
          <xsl:attribute name="{name()}">
            <xsl:value-of select="." />
          </xsl:attribute>
        </xsl:for-each>
        <xsl:apply-templates mode="writeNodes"/>
      </xsl:element>
    </xsl:variable>
    <xsl:copy-of select="ms:node-set($img)"/>
  </xsl:template>

  <!-- -->

  <xsl:template match="br" mode="writeNodes">
    <br/>
  </xsl:template>


  <!-- ==  ADD NEW STUFF TO THE ROOT & add in nodes if non ========================================================== -->

  <xsl:template match="cStructDescription" mode="writeNodes">
    <xsl:element name="{name()}">
      <xsl:for-each select="@*">
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>

      <!-- Add Display name-->
      <xsl:if test="not(DisplayName)">
        <DisplayName title="" linkType="internal" exclude="false" noindex="false"/>
      </xsl:if>
      <!-- Add Images -->
      <xsl:if test="not(Images)">
        <Images>
          <img class="icon" />
          <img class="thumbnail" />
          <img class="detail" />
        </Images>
      </xsl:if>
      <!-- Add Description -->
      <xsl:if test="not(Description)">
        <Description>
          <xsl:if test="not(DisplayName) and not(Images) and not(Description)">
            <xsl:apply-templates mode="writeNodes"/>
          </xsl:if>
        </Description>
      </xsl:if>
      <xsl:if test="DisplayName or Images or Description">
        <xsl:apply-templates mode="writeNodes"/>
      </xsl:if>
    </xsl:element>
  </xsl:template>


  <!-- ==  Process Display name attributes  ======================================================= -->

  <xsl:template match="DisplayName" mode="writeNodes">
    <xsl:element name="{name()}">
      <xsl:for-each select="@*">
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>

      <!-- @title -->
      <xsl:if test="not(@title)">
        <xsl:attribute name="title"></xsl:attribute>
      </xsl:if>

      <!-- @linkType -->
      <xsl:if test="not(@linkType)">
        <xsl:attribute name="linkType">
          <xsl:variable name="url" select="//cUrl/node()" />
          <xsl:choose>
            <!-- if URL empty default behaviour, or if number is internal link -->
            <xsl:when test="$url='' or format-number($url,'0')!='NaN'">
              <xsl:text>internal</xsl:text>
            </xsl:when>
            <xsl:otherwise>external</xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </xsl:if>

      <!-- @exclude -->
      <xsl:if test="not(@exclude)">
        <xsl:attribute name="exclude">false</xsl:attribute>
      </xsl:if>
      <!-- @noindex -->
      <xsl:if test="not(@noindex)">
        <xsl:attribute name="noindex">false</xsl:attribute>
      </xsl:if>

      <!-- write nodes -->
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>


  <!-- ==  Process Images  ======================================================= -->
  <xsl:template match="Images" mode="writeNodes">
    <xsl:element name="{name()}">
      <xsl:for-each select="@*">
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
      <!-- thumbnail -->
      <xsl:if test="not(img[@class='thumbnail'])">
        <img class="thumbnail" />
      </xsl:if>
      <!-- icon -->
      <xsl:if test="not(img[@class='icon'])">
        <img class="icon" />
      </xsl:if>
      <!-- detail -->
      <xsl:if test="not(img[@class='detail'])">
        <img class="detail" />
      </xsl:if>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns:ew="urn:ew">
  <xsl:output method="xml" indent="no" standalone="yes" omit-xml-declaration="yes" encoding="UTF-8"/>

  <!--  IMPORTANT -->
  <!--  THIS UPGRADE, upgrades Contacts to have the new Locational information that is essential for
          - Google Maps,
          - Address formattating and standardisation across ew.
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
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
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

  <xsl:template match="Content[@moduleType='SubPageGrid']" mode="writeNodes">
    <xsl:element name="{name()}">
      <xsl:for-each select="@*">
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
		<xsl:attribute name="moduleType">
			<xsl:text>SubPageList</xsl:text>
		</xsl:attribute>
		<xsl:attribute name="contentType">
			<xsl:text>MenuItem</xsl:text>
		</xsl:attribute>
		<xsl:attribute name="linkType">
			<xsl:text>internal</xsl:text>
		</xsl:attribute>
		<xsl:attribute name="listGroup">
			<xsl:text>true</xsl:text>
		</xsl:attribute>
		<xsl:attribute name="align">
			<xsl:text>horizontal</xsl:text>
		</xsl:attribute>
		<xsl:attribute name="heading">
			<xsl:text>h2</xsl:text>
		</xsl:attribute>
		<xsl:attribute name="layout">
			<xsl:text>detailed</xsl:text>
		</xsl:attribute>
		<xsl:attribute name="imagePosition">
			<xsl:text>above</xsl:text>
		</xsl:attribute>
		<xsl:attribute name="crop">
			<xsl:text>false</xsl:text>
		</xsl:attribute>
		<xsl:attribute name="alignment">
			<xsl:text>center</xsl:text>
		</xsl:attribute>
		<xsl:attribute name="fixedThumb">
			<xsl:text>fixed</xsl:text>
		</xsl:attribute>
		<xsl:attribute name="lgCol">
			<xsl:text>3</xsl:text>
		</xsl:attribute>
		<xsl:attribute name="respectFeatured">
			<xsl:text>false</xsl:text>
		</xsl:attribute>
		<xsl:attribute name="numberFeatured">
			<xsl:text></xsl:text>
		</xsl:attribute>
		<xsl:attribute name="showDesc">
			<xsl:text>false</xsl:text>
		</xsl:attribute>
		

		<xsl:apply-templates mode="writeNodes"/>
  
    </xsl:element>
  </xsl:template>

</xsl:stylesheet>
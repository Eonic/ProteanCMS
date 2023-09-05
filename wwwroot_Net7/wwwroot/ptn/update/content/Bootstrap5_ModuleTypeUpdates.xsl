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



  <xsl:template match="Content" mode="writeNodes">
	  <xsl:element name="{name()}">
		  <xsl:for-each select="@*">
			  <xsl:choose>
				  <xsl:when test="name()='moduleType' and (.='3column')">
					  <xsl:attribute name="{name()}">
						  <xsl:text>3Column</xsl:text>
					  </xsl:attribute>
				  </xsl:when>
				  <xsl:when test="name()='moduleType' and (.='4column')">
					  <xsl:attribute name="{name()}">
						  <xsl:text>4Column</xsl:text>
					  </xsl:attribute>
				  </xsl:when>
				  <xsl:when test="name()='moduleType' and (.='5column')">
					  <xsl:attribute name="{name()}">
						  <xsl:text>5Column</xsl:text>
					  </xsl:attribute>
				  </xsl:when>
				  <xsl:when test="name()='moduleType' and (.='6column')">
					  <xsl:attribute name="{name()}">
						  <xsl:text>6Column</xsl:text>
					  </xsl:attribute>
				  </xsl:when>
				  <xsl:otherwise>
					  <xsl:attribute name="{local-name()}">
						  <xsl:value-of select="." />
					  </xsl:attribute>
				  </xsl:otherwise>
			  </xsl:choose>
		  </xsl:for-each>
		  <xsl:apply-templates mode="writeNodes"/>
	  </xsl:element>
  </xsl:template>
  
</xsl:stylesheet>
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
    <xsl:element name="{name()}">
      <xsl:for-each select="@*">
        <xsl:choose>
          <xsl:when test="name()='moduleType' and (.='NewsList' or .='ContactList' or .='DocumentList' or .='EventList' or .='LinkList' or .='OrganisationList' or .='ProductList' or .='RecipeList' or .='SubPageGrid' or .='SubPageList' or .='SubPageMenu' or .='TestimonialList' or .='TicketList' or .='VacanciesList')">
            <xsl:attribute name="{name()}">
              <xsl:value-of select="." />
            </xsl:attribute>
            <xsl:attribute name="listGroup">
              <xsl:text>true</xsl:text>
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

  <!-- -->

  <xsl:template match="img" mode="writeNodes">
    <img src="{@src}" width="{@width}" height="{@height}" alt="{@alt}" class="{@class}" style="{@style}"/>
  </xsl:template>

  <!-- -->

  <xsl:template match="br" mode="writeNodes">
    <br/>
  </xsl:template>

</xsl:stylesheet>
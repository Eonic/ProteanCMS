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

  <xsl:template match="Duration" mode="writeNodes">
    <xsl:element name="{name()}">
      <xsl:for-each select="@*">
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
      <xsl:if test="not(MinimumTerm)">
        <MinimumTerm>1</MinimumTerm>
        <RenewalTerm>1</RenewalTerm>
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <xsl:template match="PaymentUnit" mode="writeNodes">
    <xsl:element name="{name()}">
      <xsl:for-each select="@*">
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
    <xsl:if test="not(ancestor::Content/Paymentfrequency)">
      <Paymentfrequency>1</Paymentfrequency>
      <SubscriptionPrices delayStart="false">
        <Price currency="GBP" type="sale" validGroup="all" suffix=""><xsl:value-of select="ancestor::Content/Prices/Price[@type='sale']/node()"/></Price>
        <Price currency="GBP" type="rrp" suffix=""/>
      </SubscriptionPrices>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Price[@type='sale' and ancestor::Prices]" mode="writeNodes">
        <Price currency="GBP" type="sale" validGroup="all" suffix="">0</Price>
  </xsl:template>

  <xsl:template match="UserGroups" mode="writeNodes">
    <xsl:element name="{name()}">
      <xsl:for-each select="@*">
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
    <Group id=""/>
    <Group id=""/>
    <Group id=""/>
    <Group id=""/>
    <Group id=""/>
    <Group id=""/>
    <Group id=""/>
    <Group id=""/>
    <Group id=""/>
  </xsl:template>

  <xsl:template match="Content[parent::cContentXmlDetail]" mode="writeNodes">
    <Content action="Eonic.Web+Cart+Subscriptions+Modules.Subscribe">
      <xsl:for-each select="*">
        <xsl:apply-templates select="." mode="writeNodes"/>
      </xsl:for-each>
    </Content>
  </xsl:template>
  
</xsl:stylesheet>
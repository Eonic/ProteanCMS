<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns:ew="urn:ew">
  <xsl:output method="xml" indent="no" standalone="yes" omit-xml-declaration="yes" encoding="UTF-8"/>

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
        <img src="{@src}" width="{@width}" height="{@height}" alt="{@alt}" class="{@class}"/>
    </xsl:template>

    <!-- -->

    <xsl:template match="br" mode="writeNodes">
        <br/>
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
        <Content moduleType="Image" box="" title="" link="" linkType="internal" >
            <xsl:attribute name="position">
                <xsl:value-of select="//cContentName"/>
            </xsl:attribute>
            <xsl:apply-templates select="img" mode="writeNodes"/>
        </Content>
    </xsl:template>

    <!-- don't need a detail any more -->
    <xsl:template match="cContentXmlDetail" mode="writeNodes">
        <cContentXmlDetail/>
    </xsl:template>
</xsl:stylesheet>
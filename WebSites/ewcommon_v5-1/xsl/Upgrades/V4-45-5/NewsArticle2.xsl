<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns:ew="urn:ew">
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

    <xsl:template match="cContentXmlBrief/Content" mode="writeNodes">
        <xsl:element name="{name()}">
            <xsl:for-each select="@*">
                <xsl:attribute name="{name()}">
                    <xsl:value-of select="." />
                </xsl:attribute>
            </xsl:for-each>
            <!-- Add showRelatedAttribute - Default Tags-->
            <xsl:attribute name="showRelated">Tag,Contact</xsl:attribute>
            <xsl:apply-templates mode="writeNodes"/>
        </xsl:element>
    </xsl:template>

    <!-- IMAGES on detail -->
    <xsl:template match="Images" mode="writeNodes">
        <xsl:element name="{name()}">
            <!-- process attributes -->
            <xsl:for-each select="@*">
                <!-- remove attribute prefix (if any) -->
                <xsl:attribute name="{name()}">
                    <xsl:value-of select="." />
                </xsl:attribute>
            </xsl:for-each>
            <xsl:apply-templates mode="writeNodes"/>
            <xsl:if test="not(img[@class='display'])">
                <img class="display"/>
            </xsl:if>
            <xsl:if test="not(img[@class='detail'])">
                <img class="detail"/>
            </xsl:if>
        </xsl:element>
    </xsl:template>

    <!-- IMAGES on brief -->
    <xsl:template match="Images[ancestor::cContentXmlBrief]" mode="writeNodes">
        <xsl:element name="{name()}">
            <!-- process attributes -->
            <xsl:for-each select="@*">
                <!-- remove attribute prefix (if any) -->
                <xsl:attribute name="{name()}">
                    <xsl:value-of select="." />
                </xsl:attribute>
            </xsl:for-each>
            <xsl:apply-templates mode="writeNodes"/>
            <xsl:if test="not(img[@class='display'])">
                <xsl:apply-templates select="//cContentXmlDetail/Content/Images/img[@class='display']" mode="writeNodes"/>
            </xsl:if>
            <xsl:if test="not(img[@class='detail'])">
                <xsl:choose>
                    <xsl:when test="//cContentXmlDetail/Content/Images/img[@class='detail']">
                        <xsl:apply-templates select="//cContentXmlDetail/Content/Images/img[@class='detail']" mode="writeNodes"/>
                    </xsl:when>
                    <xsl:otherwise>
                        <img class="detail"/>
                    </xsl:otherwise>
                </xsl:choose>
            </xsl:if>
        </xsl:element>
    </xsl:template>
    
</xsl:stylesheet>
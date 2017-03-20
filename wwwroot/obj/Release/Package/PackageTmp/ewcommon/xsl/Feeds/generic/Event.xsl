<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"  xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns:date= "http://exslt.org/dates-and-times"  extension-element-prefixes="date" xmlns:atom="http://www.w3.org/2005/Atom" xmlns:ew="urn:ew">
  <xsl:import href="../rss/rss.xsl"/>

  <!--  =========================================================================================== -->
  <!--  ==  This file contains the overwrites from RSS.xsl  ======================================= -->
 

  <!-- Different Ordering for Events -->
  <xsl:template match="Contents" mode="channelItems">

    <xsl:for-each select="Content[@type=$contentType]">
      <!-- Nativly list content with oldest date at the top -->
      <xsl:sort select="translate(StartDate,'-','')" order="ascending" data-type="number"/>
      <xsl:sort select="translate(EndDate,'-','')" order="ascending" data-type="number"/>
      <xsl:sort select="@update" order="ascending" data-type="text"/>

      <!-- output -->
      <xsl:choose>
        <!-- When startdate is today or in the future -->
        <xsl:when test="translate(StartDate/node(),'-','') &gt;= translate($today,'-','')">
          <xsl:apply-templates select="." mode="contentItem"/>
        </xsl:when>
        <!-- When End date is today or in the future, it must still be going on, so show -->
        <xsl:when test="EndDate/node() and translate(EndDate/node(),'-','') &gt;= translate($today,'-','')">
          <xsl:apply-templates select="." mode="contentItem"/>
        </xsl:when>
      </xsl:choose>

    </xsl:for-each>
  </xsl:template>

  <!-- Generic Description/Strapline logic -->
  <xsl:template match="Content" mode="buildDescription">
    <div>
      <xsl:variable name="image">
        <xsl:apply-templates select="." mode="displayThumbnail"/>
      </xsl:variable>
      <xsl:apply-templates select="ms:node-set($image)/*" mode="encodeXhtml" />
      <xsl:if test="StartDate/node()">
        <b>
        <xsl:text>Event Date: </xsl:text>
        </b>
        <xsl:call-template name="DisplayDate">
          <xsl:with-param name="date" select="StartDate/node()"/>
        </xsl:call-template>
        <xsl:if test="EndDate/node()!='' and EndDate/node() != StartDate/node()">
          <xsl:text> - </xsl:text>
          <xsl:call-template name="DisplayDate">
            <xsl:with-param name="date" select="EndDate/node()"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:text>&#160;</xsl:text>
        <xsl:if test="Times/@start!=''">
          <xsl:value-of select="translate(Times/@start,',',':')"/>
          <xsl:if test="Times/@end!=''">
            <xsl:text>-</xsl:text>
            <xsl:value-of select="translate(Times/@end,',',':')"/>
          </xsl:if>
        </xsl:if>
        <br/>
      </xsl:if>

      <xsl:if test="Location/Venue/node()">
        <b>
          <xsl:text>Location: </xsl:text>
        </b>
        <xsl:value-of select="Location/Venue/node()"/>
        <xsl:text>, </xsl:text>
        <xsl:if test="Location/Address/No/node()">
          <xsl:value-of select="Location/Address/No/node()"/>
          <xsl:text> </xsl:text>
        </xsl:if>
        <xsl:if test="Location/Address/Street/node()">
          <xsl:value-of select="Location/Address/Street/node()"/>
          <xsl:text>, </xsl:text>
        </xsl:if>
        <xsl:if test="Location/Address/Locality/node()">
          <xsl:value-of select="Location/Address/Locality/node()"/>
          <xsl:text>, </xsl:text>
        </xsl:if>
        <xsl:if test="Location/Address/Region/node()">
          <xsl:value-of select="Location/Address/Region/node()"/>
          <xsl:text>, </xsl:text>
        </xsl:if>
        <xsl:if test="Location/Address/PostCode/node()">
          <xsl:value-of select="Location/Address/PostCode/node()"/>
        </xsl:if>
        <br/>
      </xsl:if>
      
      <xsl:if test="Strap/node()">
        <xsl:apply-templates select="Strap/node()" mode="encodeXhtml" />
        <br/>
      </xsl:if>
      
      
        
    </div>
  </xsl:template>
</xsl:stylesheet>


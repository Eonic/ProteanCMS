<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"  xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns:date= "http://exslt.org/dates-and-times"  extension-element-prefixes="date" xmlns:atom="http://www.w3.org/2005/Atom" xmlns:ew="urn:ew">
  <xsl:import href="../rss/rss.xsl"/>



  <!-- Special Recipe Description overide to include more values -->
  <xsl:template match="Content" mode="buildDescription">
    <div>
      <xsl:variable name="image">
        <xsl:apply-templates select="." mode="displayThumbnail"/>
      </xsl:variable>
      <xsl:apply-templates select="ms:node-set($image)/*" mode="encodeXhtml" />
      
      
      <xsl:choose>
        <xsl:when test="Strapline/node()">
          <xsl:apply-templates select="Strapline/node()" mode="encodeXhtml" />
        </xsl:when>
        <xsl:when test="Strap/node()">
          <xsl:apply-templates select="Strap/node()" mode="encodeXhtml" />
        </xsl:when>
        <xsl:when test="Introduction/node()">
          <xsl:apply-templates select="Introduction/node()" mode="encodeXhtml" />
        </xsl:when>
        <xsl:when test="Summary/node()">
          <xsl:apply-templates select="Summary/node()" mode="encodeXhtml" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="Body/node()" mode="encodeXhtml" />
        </xsl:otherwise>
      </xsl:choose>
      <div class="guide">

        <p>
          <xsl:if test="Yield/node()!=''">
            <strong>
              <xsl:value-of select="Yield/node()" />
            </strong>
            <br/>
          </xsl:if>

          <xsl:if test="PrepTime/node()!=''">
            <span class="label">
              <!-- Preparation time -->
              <xsl:call-template name="term2076"/>
              <xsl:text>: </xsl:text>
            </span>
            <xsl:call-template name="getNiceTimeFromMinutes">
              <xsl:with-param name="mins" select="PrepTime/node()" />
              <xsl:with-param name="format" select="'short'" />
            </xsl:call-template>
            <br/>
          </xsl:if>

          <xsl:if test="CookTime/node()!=''">
            <span class="label">
              <!-- Cooking time -->
              <xsl:call-template name="term2077"/>
              <xsl:text>: </xsl:text>
            </span>
            <xsl:call-template name="getNiceTimeFromMinutes">
              <xsl:with-param name="mins" select="CookTime/node()" />
              <xsl:with-param name="format" select="'short'" />
            </xsl:call-template>
          </xsl:if>

        </p>

      </div>

      <div>
        <h4>
          <xsl:call-template name="term2075"/>
        </h4>
        <xsl:apply-templates select="Ingredients/node()" mode="encodeXhtml" />
      </div>

      <div>
        <h4>
          <!-- Method -->
          <xsl:call-template name="term2078"/>
        </h4>
        <xsl:apply-templates select="Instructions/node()" mode="encodeXhtml" />
      </div>
    </div>
  </xsl:template>

</xsl:stylesheet>


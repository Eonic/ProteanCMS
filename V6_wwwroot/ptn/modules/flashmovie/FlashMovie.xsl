<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!-- ## Flash Movie (SWF) ##########################################################################   -->
  <xsl:template match="Content[@type='Module' and @moduleType='FlashMovie']" mode="displayBrief">
    <div class="flashMovie">
      <!--<xsl:apply-templates select="." mode="inlinePopupOptions"/>-->
      <xsl:choose>
        <xsl:when test="contains(object/@data,'.flv')">
          <div id="FVPlayer{@id}">
            <a href="http://www.adobe.com/go/getflashplayer">
              <xsl:call-template name="term2004" />
            </a>
            <xsl:text>&#160;</xsl:text>
            <xsl:call-template name="term2005" />
            <xsl:if test="object/img/@src!=''">
              <xsl:apply-templates select="object/img" mode="cleanXhtml"/>
            </xsl:if>
          </div>
          <script type="text/javascript">
            <xsl:text>var s1 = new SWFObject("/ewcommon/flash/flvplayer.swf","Flash_</xsl:text>
            <xsl:value-of select="@id"/>
            <xsl:text>","</xsl:text>
            <xsl:value-of select="object/@width"/>
            <xsl:text>","</xsl:text>
            <xsl:value-of select="object/@height"/>
            <xsl:text>","</xsl:text>
            <xsl:value-of select="object/param[@name='ver']/@value"/>
            <xsl:text>","7");</xsl:text>
            <xsl:text>s1.addParam("allowfullscreen","true");</xsl:text>
            <xsl:text>s1.addParam("wmode","transparent");</xsl:text>
            <xsl:text>s1.addVariable("file","</xsl:text>
            <xsl:value-of select="object/@data"/>
            <xsl:text>");</xsl:text>
            <xsl:if test="object/img/@src!=''">
              <xsl:text>s1.addVariable("image","</xsl:text>
              <xsl:value-of select="object/img/@src"/>
              <xsl:text>");</xsl:text>
            </xsl:if>
            <xsl:text>s1.write("FVPlayer</xsl:text>
            <xsl:value-of select="@id"/>
            <xsl:text>");</xsl:text>
          </script>
        </xsl:when>
        <xsl:otherwise>
          <div id="FlashAlternative_{@id}" class="FlashAlternative">
            <xsl:if test="object/img/@src!=''">
              <xsl:apply-templates select="object/img" mode="cleanXhtml"/>
            </xsl:if>
            <xsl:text> </xsl:text>
          </div>
          <script type="text/javascript">
            <xsl:text>var so = new SWFObject("</xsl:text>
            <xsl:value-of select="object/@data"/>
            <xsl:text>", "Flash_</xsl:text>
            <xsl:value-of select="@id"/>
            <xsl:text>", "</xsl:text>
            <xsl:value-of select="object/@width"/>
            <xsl:text>", "</xsl:text>
            <xsl:value-of select="object/@height"/>
            <xsl:text>", "</xsl:text>
            <xsl:value-of select="object/param[@name='ver']/@value"/>
            <xsl:text>", "</xsl:text>
            <xsl:value-of select="object/param[@name='bgcolor']/@value"/>
            <xsl:text>");</xsl:text>
            <xsl:text>so.addParam("wmode","transparent");</xsl:text>
            <xsl:text>so.write("FlashAlternative_</xsl:text>
            <xsl:value-of select="@id"/>
            <xsl:text>");</xsl:text>
          </script>
        </xsl:otherwise>
      </xsl:choose>
    </div>
  </xsl:template>
  
</xsl:stylesheet>
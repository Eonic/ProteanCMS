<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!-- ## Google Ad Module ###########################################################################   -->

  <xsl:template match="Content[@type='Module' and @moduleType='GoogleAd']" mode="contentJS">
    <script async="" src="https://securepubads.g.doubleclick.net/tag/js/gpt.js"></script>
    <script>
      <xsl:text>window.googletag = window.googletag || {cmd: []};
      googletag.cmd.push(function() {
      googletag.defineSlot('</xsl:text>
      <xsl:value-of select="@adName"/>
      <xsl:text>', [</xsl:text>
      <xsl:value-of select="@adWidth"/>
      <xsl:text>, </xsl:text>
      <xsl:value-of select="@adHeight"/>
      <xsl:text>], '</xsl:text>
      <xsl:value-of select="@adPlacement"/>
      <xsl:text>').addService(googletag.pubads());
      googletag.pubads().enableSingleRequest();
      googletag.enableServices();
      });</xsl:text>
    </script>
    <script>
      <xsl:text>googletag.cmd.push(function() { googletag.display("</xsl:text>
      <xsl:value-of select="@adPlacement"/>
      <xsl:text>"); });</xsl:text>
    </script>
  </xsl:template>


  <xsl:template match="Content[@type='Module' and @moduleType='GoogleAd']" mode="displayBrief">
    <xsl:variable name="GoogleAdManagerId" select="/Page/Contents/Content[@name='GoogleAdManagerId']/node()" />
    <div class="googleadvert singleAd">
      <xsl:choose>
        <!-- WHEN NO ID - ADD BUTTON -->
        <xsl:when test="$GoogleAdManagerId=''">
          <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
            <xsl:with-param name="type">PlainText</xsl:with-param>
            <xsl:with-param name="text">Add Google Ad ID</xsl:with-param>
            <xsl:with-param name="name">GoogleAdManagerId</xsl:with-param>
          </xsl:apply-templates>
        </xsl:when>
        <!-- WHEN ID AND ADVERTS - INITIALISE and DISPLAY -->
        <xsl:otherwise>

          <xsl:choose>
            <xsl:when test="$page/@adminMode">
              <p>
                <xsl:text>Ad Name: '</xsl:text>
                <xsl:value-of select="@adName"/>
                <xsl:text>'</xsl:text>
              </p>
              <p>
                <xsl:text>Website Placement: '</xsl:text>
                <xsl:value-of select="@adPlacement"/>
                <xsl:text>'</xsl:text>
              </p>
              <p>
                <em>Adverts are disabled in admin to avoid false impressions and clicks.</em>
              </p>
            </xsl:when>
            <xsl:otherwise>
              <!-- /43122906/Food_Analysis_300x600 -->
              <div id="{@adPlacement}" style="width: {@adWidth}px; height: {@adHeight}px;">
                <xsl:text> </xsl:text>
              </div>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>
    </div>
  </xsl:template>
  
</xsl:stylesheet>
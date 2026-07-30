<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!-- ## Google Ad Module ###########################################################################   -->

  <xsl:template match="Content[@type='Module' and @moduleType='GoogleAd']" mode="contentJS">
	  <xsl:choose>
		  <xsl:when test="$page/@adminMode"></xsl:when>
		  <xsl:otherwise>
    <script defer="defer" src="https://securepubads.g.doubleclick.net/tag/js/gpt.js">&#160;</script>
    <script>
<xsl:text>
function initAds() {
window.googletag = window.googletag || {cmd: []};
googletag.cmd.push(function() {

  // Global config (single request mode)
  googletag.setConfig({
    singleRequest: true
  });

  // Responsive size mapping
  var mapping = googletag.sizeMapping()
    .addSize([768, 0], [[</xsl:text>
      <xsl:value-of select="@adWidth"/>
      <xsl:text>, </xsl:text>
      <xsl:value-of select="@adHeight"/>
      <xsl:text>]])</xsl:text>
    <xsl:text>
    .addSize([0, 0], [[</xsl:text>
      <xsl:value-of select="@adWidthMob"/>
      <xsl:text>, </xsl:text>
      <xsl:value-of select="@adHeightMob"/>
      <xsl:text>]])</xsl:text>
    <xsl:text>
    .build();

  googletag.defineSlot('</xsl:text>
    <xsl:value-of select="@adName"/>
    <xsl:text>', [[</xsl:text>
      <xsl:value-of select="@adWidth"/>
      <xsl:text>, </xsl:text>
      <xsl:value-of select="@adHeight"/>
      <xsl:text>], [</xsl:text>
      <xsl:value-of select="@adWidthMob"/>
      <xsl:text>, </xsl:text>
      <xsl:value-of select="@adHeightMob"/>
      <xsl:text>]], '</xsl:text>
      <xsl:value-of select="@adPlacement"/>
      <xsl:text>')
    .defineSizeMapping(mapping)
    .addService(googletag.pubads());

  googletag.enableServices();
});
});

// Fire when banner loads
window.addEventListener("cookieConsentUpdate", initAds);

// Fire when user clicks accept/reject
window.addEventListener("cookieConsent", 
</xsl:text>
    </script>   
		  </xsl:otherwise>
	  </xsl:choose>
  </xsl:template>


  <xsl:template match="Content[@type='Module' and @moduleType='GoogleAd']" mode="displayBrief">
    <xsl:variable name="GoogleAdManagerId" select="/Page/Contents/Content[@name='GoogleAdManagerId']/node()" />
    <div class="googleadvert singleAd">


          <xsl:choose>
            <xsl:when test="$page/@adminMode">
				<div class="adminBanner" style="width:{@adWidth}px;height:{@adHeight}px;">
              <p>
                <xsl:text>Ad Name: '</xsl:text>
                <xsl:value-of select="@adName"/>
                <xsl:text>'</xsl:text>&#160;&#160;&#160;
                <xsl:text>Website Placement: '</xsl:text>
                <xsl:value-of select="@adPlacement"/>
                <xsl:text>'</xsl:text>
              </p>
              <p>
                <em>Adverts are disabled in admin to avoid false impressions and clicks.</em>
              </p>
					</div>
            </xsl:when>
            <xsl:otherwise>
              <!-- /43122906/Food_Analysis_300x600 -->
              <div id="{@adPlacement}" style="width: {@adWidth}px; height: {@adHeight}px;">
                <xsl:text> </xsl:text>
              </div>
            </xsl:otherwise>
          </xsl:choose>
    </div>
  </xsl:template>
  
</xsl:stylesheet>
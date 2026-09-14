<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!-- ## Google Ad Module ###########################################################################   -->

	<xsl:template match="Content[@type='Module' and @moduleType='GoogleAd']" mode="defineslot">
		<xsl:text>googletag.defineSlot('</xsl:text><xsl:value-of select="@adName"/><xsl:text>', [[</xsl:text><xsl:value-of select="@adWidth"/><xsl:text>, </xsl:text><xsl:value-of select="@adHeight"/><xsl:text>], [</xsl:text><xsl:value-of select="@adWidthMob"/><xsl:text>, </xsl:text><xsl:value-of select="@adHeightMob"/><xsl:text>]], '</xsl:text><xsl:value-of select="@adPlacement"/><xsl:text>').addService(googletag.pubads());
   </xsl:text>
	</xsl:template>

	<xsl:template match="Content[@type='Module' and @moduleType='GoogleAd']" mode="defineslot">
		<xsl:text>
var mapping_</xsl:text>
		<xsl:value-of select="translate(@adPlacement,'-','_')"/>
		<xsl:text> =
    googletag.sizeMapping()
        .addSize([768,0], [[</xsl:text>
		<xsl:value-of select="@adWidth"/>
		<xsl:text>,</xsl:text>
		<xsl:value-of select="@adHeight"/>
		<xsl:text>]])
        .addSize([0,0], [[</xsl:text>
		<xsl:value-of select="@adWidthMob"/>
		<xsl:text>,</xsl:text>
		<xsl:value-of select="@adHeightMob"/>
		<xsl:text>]])
        .build();

googletag.defineSlot('</xsl:text>
		<xsl:value-of select="@adName"/>
		<xsl:text>',
    [[</xsl:text>
		<xsl:value-of select="@adWidth"/>
		<xsl:text>,</xsl:text>
		<xsl:value-of select="@adHeight"/>
		<xsl:text>],
    [</xsl:text>
		<xsl:value-of select="@adWidthMob"/>
		<xsl:text>,</xsl:text>
		<xsl:value-of select="@adHeightMob"/>
		<xsl:text>]],
    '</xsl:text>
		<xsl:value-of select="@adPlacement"/>
		<xsl:text>')
    .defineSizeMapping(mapping_</xsl:text>
		<xsl:value-of select="translate(@adPlacement,'-','_')"/>
		<xsl:text>)
    .addService(googletag.pubads());

</xsl:text>

	</xsl:template>
	
	<xsl:template match="Content[@type='Module' and @moduleType='GoogleAd']" mode="displaySlot">
			<xsl:text>googletag.cmd.push(function() { googletag.display('</xsl:text><xsl:value-of select="@adPlacement"/><xsl:text>'); });</xsl:text>
	</xsl:template>
	
  <xsl:template match="Content[@type='Module' and @moduleType='GoogleAd'][1]" mode="contentJS">
	  <xsl:choose>
		  <xsl:when test="$page/@adminMode"></xsl:when>
		  <xsl:otherwise>


<script><xsl:text>
setTimeout(function () {
  console.log("trigger");
initialiseAds();
}, 1000);

window.addEventListener("cf_consent", function(event) {

    var marketing =
        event.detail &amp;&amp; event.detail.marketing;

    if (marketing) {

        gtag('consent', 'update', {
            ad_storage: 'granted',
            ad_user_data: 'granted',
            ad_personalization: 'granted',
            analytics_storage: 'granted'
        });

        initialiseAds();
    }

});

function initialiseAds() {
  console.log("Initializing Ads");

	if (window.adsLoaded) return;
	window.adsLoaded = true;

	var gpt = document.createElement('script');
	gpt.async = true;
	gpt.src = 'https://securepubads.g.doubleclick.net/tag/js/gpt.js';

document.head.appendChild(gpt);
gpt.onload = function() {
  window.googletag = window.googletag || {cmd: []};
  googletag.cmd.push(function() {</xsl:text>
<xsl:apply-templates select="$page/Contents/Content[@type='Module' and @moduleType='GoogleAd']" mode="defineslot"/>
	<xsl:text>
    googletag.pubads().enableSingleRequest();
    googletag.enableServices();
  });
</xsl:text>
    <xsl:apply-templates select="$page/Contents/Content[@type='Module' and @moduleType='GoogleAd']" mode="displaySlot"/>
	<xsl:text>
	};
};

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
              <div id="{@adPlacement}">
                <xsl:text> </xsl:text>
              </div>
            </xsl:otherwise>
          </xsl:choose>
    </div>
  </xsl:template>
  
</xsl:stylesheet>
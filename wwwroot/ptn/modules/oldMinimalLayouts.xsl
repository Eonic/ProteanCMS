<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew fb g xlink" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew" xmlns:fb="https://www.facebook.com/2008/fbml" xmlns:xlink="http://www.w3.org/2000/svg" xmlns:g="http://base.google.com/ns/1.0">




  <!-- ## Generic displayBrief for Formatted Text and Images   #####################################################################   -->


  <!-- ## Generic displayBrief for Formatted Text and Images   #####################################################################   -->
  <xsl:template match="Content[@moduleType='FormattedCode']" mode="displayBrief">
    <pre class="code">
      <xsl:apply-templates select="node()" mode="cleanXhtml"/>
    </pre>
  </xsl:template>

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

  <!-- Image Module Display with Link -->
  <xsl:template match="Content[@type='Module' and @moduleType='iFrame' ]" mode="displayBrief">
    <xsl:choose>
      <xsl:when test="@responsive='true'">
        <div>
          <xsl:attribute name="class">
            <xsl:value-of select="@aspect"/>
          </xsl:attribute>
          <iframe class="embed-responsive-item" height="100%" width="100%"  >
            <xsl:attribute name="src">
              <xsl:value-of select="iframe/@src"/>
            </xsl:attribute>
            <xsl:attribute name="style">
              <xsl:value-of select="iframe/@style"/>
            </xsl:attribute>
            <xsl:apply-templates select="iframe/node()" mode="cleanXhtml"/>
            <xsl:text> </xsl:text>
          </iframe>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <iframe>
          <xsl:attribute name="src">
            <xsl:value-of select="iframe/@src"/>
          </xsl:attribute>
          <xsl:attribute name="width">
            <xsl:value-of select="iframe/@width"/>
          </xsl:attribute>
          <xsl:attribute name="height">
            <xsl:value-of select="iframe/@height"/>
          </xsl:attribute>
          <xsl:attribute name="style">
            <xsl:value-of select="iframe/@style"/>
          </xsl:attribute>
          <xsl:apply-templates select="iframe/node()" mode="cleanXhtml"/>
          <xsl:text> </xsl:text>
        </iframe>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="Content[@type='Module' and @moduleType='EmbeddedHtml' ]" mode="displayBrief">
    <xsl:apply-templates select="*" mode="cleanXhtml"/>
    <xsl:text> </xsl:text>
  </xsl:template>


  <!-- ########################################################################################   -->
  <!-- ## OLD Googlemaps - Kept in for legacy content  ########################################   -->
  <!-- ########################################################################################   -->
  <xsl:template match="Content[@type='Module' and @moduleType='GoogleMap']" mode="displayBrief">
    <div id="map{@id}" style="width: auto; height: 400px">
      <xsl:text> </xsl:text>
    </div>
    <div id="mapWindow{@id}">
      <xsl:apply-templates select="Location/label" mode="cleanXhtml"/>
    </div>
  </xsl:template>

  <xsl:template match="Page[Contents/Content[@moduleType='GoogleMap']]" mode="pageJs">
    <xsl:variable name="mapContent" select="Contents/Content[@moduleType='GoogleMap']"/>
    <script src="//maps.google.com/maps?file=api&amp;v=2&amp;key={$mapContent/APIKey/node()}"	type="text/javascript">&#160;</script>
    <script src="//www.google.com/uds/api?file=uds.js&amp;v=1.0&amp;key={$mapContent/AjaxAPIKey/node()}"	type="text/javascript">&#160;</script>
    <script src="/ewcommon/js/gmap.js"	type="text/javascript">&#160;</script>
    <script type="text/javascript">
      <xsl:comment>
        <![CDATA[   
			gMapZoom = ]]><xsl:value-of select="$mapContent/Zoom/node()"/><![CDATA[;
			gMapView = ]]><xsl:value-of select="$mapContent/View/node()"/><![CDATA[;
		
			function mapSettings(map) {
			]]><xsl:choose>
          <xsl:when test="$mapContent/Control/node()='Large'">
            <![CDATA[ map.addControl(new GLargeMapControl());  ]]>
          </xsl:when>
          <xsl:when test="$mapContent/Control/node()='Small'">
            <![CDATA[ map.addControl(new GSmallMapControl());]]>
          </xsl:when>
        </xsl:choose>
        <xsl:if test="$mapContent/TypeButtons/node()='On'">
          <![CDATA[ map.addControl(new GMapTypeControl());  ]]>
        </xsl:if><![CDATA[
			}
		]]>
        <![CDATA[	
			if (GBrowserIsCompatible()) {
			var gMapZoom = ]]><xsl:value-of select="$mapContent/Zoom/node()"/><![CDATA[;
			var gMapView = ]]><xsl:value-of select="$mapContent/View/node()"/><![CDATA[;
			var gGeoCode = ']]><xsl:value-of select="$mapContent/Location/@geoCode"/>'<![CDATA[;
			}
			
			function mapLoad() {
				if (GBrowserIsCompatible()) {
				map = new GMap2(document.getElementById("map]]><xsl:value-of select="$mapContent/@id"/><![CDATA["));
				mapSettings(map);
				]]>
        <xsl:choose>
          <xsl:when test="$mapContent/Location/@lat!=''">
            <![CDATA[
					
					var pntLatLong = new GLatLng(]]><xsl:value-of select="$mapContent/Location/@lat"/><![CDATA[,]]><xsl:value-of select="$mapContent/Location/@long"/><![CDATA[);
					map.setCenter(pntLatLong,gMapZoom,gMapView);
					var marker = new GMarker(pntLatLong);
					map.addOverlay(marker);
					marker.openInfoWindowHtml(document.getElementById("mapWindow]]><xsl:value-of select="$mapContent/@id"/><![CDATA["));
			]]>
          </xsl:when>
          <xsl:otherwise>
            <![CDATA[								
					usePointFromPostcode(gGeoCode,function (point) {
					mapLoadAtPoint(point);
					showMarker(point,'mapWindow]]><xsl:value-of select="$mapContent/@id"/><![CDATA[');
					})
					]]>
          </xsl:otherwise>
        </xsl:choose>

        <![CDATA[
			}
			}
			addLoadEvent(mapLoad);
			addUnLoadEvent(GUnload);
			]]>
      </xsl:comment>
    </script>
  </xsl:template>


  <!--  ==========================================================================================  -->
  <!--  ==  GOOGLE MAPS v3 - Latest EW Google Map using Asynchronous API  ========================  -->
  <!--  ==  http://code.google.com/apis/maps/documentation/javascript/  ==========================  -->
  <!--  ==========================================================================================  -->

  <!-- Module -->
  <xsl:template match="Content[@moduleType='GoogleMapv3']" mode="displayBrief">
    <div class="GoogleMap">
      <div id="gmap{@id}" class="gmap-canvas">
        <xsl:if test="@height!=''">
          <xsl:attribute name="data-mapheight">
            <xsl:value-of select="@height"/>
          </xsl:attribute>
        </xsl:if>To see this map you must have Javascript enabled
      </div>
    </div>
    <xsl:if test="/Page/@adminMode">
      <xsl:apply-templates select="Content[@type='Location']" mode="displayBrief"/>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Page" mode="googleMapJS">
    <!-- Initialise any Google Maps -->
    <xsl:if test="//Content[@type='Module' and @moduleType='GoogleMapv3'] | ContentDetail/Content[@type='Organisation' and descendant-or-self::latitude[node()!='']]">
      <xsl:variable name="apiKey">
        <xsl:choose>
          <xsl:when test="//Content[@type='Module' and @moduleType='GoogleMapv3']/@apiKey!=''">
            <xsl:value-of select="//Content[@type='Module' and @moduleType='GoogleMapv3']/@apiKey"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$GoogleAPIKey"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <script type="text/javascript" src="//maps.google.com/maps/api/js?v=3&amp;key={$apiKey}">&#160;</script>
      <script type="text/javascript">
        <xsl:text>function initialiseGMaps(){</xsl:text>
        <xsl:apply-templates select="//Content[@moduleType='GoogleMapv3'] | ContentDetail/Content[@type='Organisation'] " mode="initialiseGoogleMap"/>
        <xsl:text>};</xsl:text>
      </script>
    </xsl:if>
  </xsl:template>

  <!-- Each Map has it's set of values - unique by content id -->
  <xsl:template match="Content" mode="initialiseGoogleMap">
    <xsl:variable name="gMapId" select="concat('gmap',@id)"/>
    <xsl:variable name="mOptionsName" select="concat('mOptions',@id)"/>
    <xsl:variable name="mCentreLoc" select="concat('mCentreLoc',@id)"/>
    <!-- Map Centering Co-ords -->
    <!--if google map id all ready exits not compleat-->
    <!--<xsl:text>if ($('#gmap</xsl:text><xsl:value-of select="@id"/><xsl:text>').lengh) {</xsl:text>
      <xsl:text> $('#gmap</xsl:text><xsl:value-of select="@id"/><xsl:text>').show();</xsl:text>-->
    <xsl:choose>
      <xsl:when test="Location/@loc='address'">
        <!-- Initial set to a location - we will reset this after initialising options - as we have to convert the address -->
        var <xsl:value-of select="$mCentreLoc"/> = new google.maps.LatLng(51.12732,0.260611);
      </xsl:when>
      <xsl:otherwise>
        <!-- if geo (lat/long) - initialise straight -->
        var <xsl:value-of select="$mCentreLoc"/> = new google.maps.LatLng(<xsl:value-of select="Location/Geo/@latitude"/>
        <xsl:text>,</xsl:text>
        <xsl:value-of select="Location/Geo/@longitude"/>
        <xsl:text>);</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
    <!-- Map Options -->
    <xsl:text>var </xsl:text>
    <xsl:value-of select="$mOptionsName"/>
    <xsl:text> = {</xsl:text>
    <xsl:text>zoomControl:</xsl:text>
    <xsl:value-of select="Zoom/@allow"/>
    <xsl:text>,zoom:</xsl:text>
    <xsl:value-of select="Zoom/node()"/>
    <xsl:text>,center:</xsl:text>
    <xsl:value-of select="$mCentreLoc"/>
    <xsl:text>,mapTypeControl:</xsl:text>
    <xsl:value-of select="TypeButtons/node()"/>
    <xsl:text>,mapTypeId:google.maps.MapTypeId.</xsl:text>
    <xsl:value-of select="View/node()"/>
    <xsl:if test="Zoom/@disableMouseWheel='true'">
      ,scrollwheel:  false
    </xsl:if>
    <xsl:text>};</xsl:text>
    <!-- Initialise map item -->
    <xsl:text>var </xsl:text>
    <xsl:value-of select="$gMapId"/>
    <xsl:text> = new google.maps.Map(document.getElementById("</xsl:text>
    <xsl:value-of select="$gMapId"/>
    <xsl:text>"), </xsl:text>
    <xsl:value-of select="$mOptionsName"/>
    <xsl:text>);</xsl:text>
    <xsl:if test="$page/@layout!='Modules_Masonary'">
      <!-- Adjust CSS to size map correctly. -->
      <xsl:text>adjustGMapSizes(</xsl:text>
      <xsl:text>$("#</xsl:text>
      <xsl:value-of select="$gMapId"/>
      <xsl:text>"));</xsl:text>
    </xsl:if>
    <!-- IF Address, go get get geocodings - and reset map options -->
    <xsl:if test="Location/@loc='address'">
      <xsl:apply-templates select="." mode="getGmapLocation">
        <xsl:with-param name="gMapId" select="$gMapId"/>
      </xsl:apply-templates>
    </xsl:if>
    <!--if google map id all ready exits not compleat end -->
    <!--<xsl:text>};</xsl:text>-->
  </xsl:template>

  <!-- Each Map has it's set of values - unique by content id -->
  <xsl:template match="Content[@type='Organisation']" mode="initialiseGoogleMap">
    <xsl:variable name="gMapId" select="concat('gmap',@id)"/>
    <xsl:variable name="mOptionsName" select="concat('mOptions',@id)"/>
    <xsl:variable name="mCentreLoc" select="concat('mCentreLoc',@id)"/>
    <!-- Map Centering Co-ords -->
    <!-- if geo (lat/long) - initialise straight -->
    var <xsl:value-of select="$mCentreLoc"/> = new google.maps.LatLng(<xsl:value-of select="descendant-or-self::latitude/node()"/>
    <xsl:text>,</xsl:text>
    <xsl:value-of select="descendant-or-self::longitude/node()"/>
    <xsl:text>);</xsl:text>
    <!-- Map Options -->
    <xsl:text>var </xsl:text>
    <xsl:value-of select="$mOptionsName"/>
    <xsl:text> = {</xsl:text>
    <xsl:text>zoomControl:true</xsl:text>
    <xsl:text>,zoom:10</xsl:text>
    <xsl:text>,center:</xsl:text>
    <xsl:value-of select="$mCentreLoc"/>
    <xsl:text>,mapTypeControl:true</xsl:text>
    <xsl:text>,mapTypeId:google.maps.MapTypeId.ROADMAP</xsl:text>
    <xsl:text>};</xsl:text>
    <!-- Initialise map item -->
    <xsl:text>var </xsl:text>
    <xsl:value-of select="$gMapId"/>
    <xsl:text> = new google.maps.Map(document.getElementById("</xsl:text>
    <xsl:value-of select="$gMapId"/>
    <xsl:text>"), </xsl:text>
    <xsl:value-of select="$mOptionsName"/>
    <xsl:text>);</xsl:text>
    <!-- Adjust CSS to size map correctly. -->
    <xsl:text>adjustGMapSizes(</xsl:text>
    <xsl:text>$("#</xsl:text>
    <xsl:value-of select="$gMapId"/>
    <xsl:text>"));</xsl:text>

    <xsl:apply-templates select="." mode="getGmapLocation">
      <xsl:with-param name="gMapId" select="$gMapId"/>
    </xsl:apply-templates>

  </xsl:template>

  <!-- Gets Geocode from Postal Address  -->
  <xsl:template match="Content[@type='Organisation']" mode="getGmapLocation">
    <xsl:param name="gMapId" />
    <!--
			Form has already been set with default coords,
			We are doing an address look up and resetting the centering.
		-->

    <xsl:variable name="jsLatLng">
      <xsl:apply-templates select="Organization/location/GeoCoordinates" mode="getJsLatLng"/>
    </xsl:variable>
    <xsl:value-of select="$gMapId"/>.setCenter(<xsl:value-of select="$jsLatLng"/>);
    <xsl:apply-templates select="." mode="setGMapMarker">
      <xsl:with-param name="jsPositionValue" select="$jsLatLng"/>
      <xsl:with-param name="mapId" select="$gMapId"/>
    </xsl:apply-templates>
  </xsl:template>


  <!-- Returns the code for creating a new LatLng object based on the stored geocoordinates -->
  <xsl:template match="Geo" mode="getJsLatLng">
    <xsl:text>new google.maps.LatLng(</xsl:text>
    <xsl:value-of select="@latitude"/>
    <xsl:text>, </xsl:text>
    <xsl:value-of select="@longitude"/>
    <xsl:text>)</xsl:text>
  </xsl:template>

  <xsl:template match="GeoCoordinates" mode="getJsLatLng">
    <xsl:text>new google.maps.LatLng(</xsl:text>
    <xsl:value-of select="latitude"/>
    <xsl:text>, </xsl:text>
    <xsl:value-of select="longitude"/>
    <xsl:text>)</xsl:text>
  </xsl:template>

  <!-- Gets Geocode from Postal Address  -->
  <xsl:template match="Content[Location/@loc='address']" mode="getGmapLocation">
    <xsl:param name="gMapId" />
    <!--
			Form has already been set with default coords,
			We are doing an address look up and resetting the centering.
		-->
    <xsl:choose>
      <xsl:when test="Location/Geo/@latitude != ''">
        <xsl:variable name="jsLatLng">
          <xsl:apply-templates select="Location/Geo" mode="getJsLatLng"/>
        </xsl:variable>
        <xsl:value-of select="$gMapId"/>.setCenter(<xsl:value-of select="$jsLatLng"/>);
        <!-- IF NO LOCATIONS - Marker this location -->
        <xsl:if test="Location/@marker='true'">
          <xsl:apply-templates select="." mode="setGMapMarker">
            <xsl:with-param name="jsPositionValue" select="$jsLatLng"/>
            <xsl:with-param name="mapId" select="$gMapId"/>
          </xsl:apply-templates>
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        var geocoder<xsl:value-of select="@id"/> = new google.maps.Geocoder();
        <!-- Goes and gets the Location of the Address -->
        geocoder<xsl:value-of select="@id"/>.geocode({ 'address': '<xsl:apply-templates select="Location/Address" mode="csvAddress"/>' }, function (results, status) {
        <!-- If result OK - Center map to location -->
        if (status == google.maps.GeocoderStatus.OK) {
        <xsl:value-of select="$gMapId"/>.setCenter(results[0].geometry.location);
        <!-- IF NO LOCATIONS - Marker this location -->
        <xsl:if test="Location/@marker='true'">
          <xsl:apply-templates select="." mode="setGMapMarker">
            <xsl:with-param name="jsPositionValue" select="'results[0].geometry.location'"/>
          </xsl:apply-templates>
        </xsl:if>
        }
        });
      </xsl:otherwise>
    </xsl:choose>
    <xsl:apply-templates select="Content[@type='Location']" mode="getLocationsLocation">
      <xsl:with-param name="mapId" select="@id"/>
    </xsl:apply-templates>
  </xsl:template>

  <!--alternative location set up on map-->
  <xsl:template match="Content[@moduleType='GoogleMapv3']" mode="setGMapMarker">
    <xsl:param name="jsPositionValue"/>
    <xsl:variable name="markerDescription">
      <div class="mapPopup">
        <xsl:if test="Location/Venue/node()">
          <h3>
            <xsl:call-template name="escape-js">
              <xsl:with-param name="string">
                <xsl:value-of select="Location/Venue/node()"/>
              </xsl:with-param>
            </xsl:call-template>
          </h3>
        </xsl:if>
        <div class="map-description">
          <xsl:apply-templates select="Description/*" mode="cleanXhtml"/>
        </div>
      </div>
    </xsl:variable>
    var marker<xsl:value-of select="@id"/> = new google.maps.Marker({
    map: gmap<xsl:value-of select="@id"/>,
    position: <xsl:value-of select="$jsPositionValue"/>
    });
    <!-- If Description - Create the bubble!! -->
    <xsl:if test="Location/Venue/node() or Description/node()">
      <!-- Html String & Info Window-->
      var contentString<xsl:value-of select="@id"/> = '<xsl:call-template name="escape-js">
        <xsl:with-param name="string">
          <xsl:copy-of select="$markerDescription"/>
        </xsl:with-param>
      </xsl:call-template>';
      var infowindow<xsl:value-of select="@id"/> = new google.maps.InfoWindow({
      content: contentString<xsl:value-of select="@id"/>
      });
      <!-- Click Listener -->
      google.maps.event.addListener(marker<xsl:value-of select="@id"/>, 'click', function () {
      infowindow<xsl:value-of select="@id"/>.open(gmap<xsl:value-of select="@id"/>, marker<xsl:value-of select="@id"/>);
      });
    </xsl:if>
  </xsl:template>

  <!--set up bubble for location-->
  <xsl:template match="Content[@type='Location']" mode="setGMapMarker">
    <xsl:param name="jsPositionValue"/>
    <xsl:param name="mapId"/>
    var marker<xsl:value-of select="@id"/> = new google.maps.Marker({
    map: gmap<xsl:value-of select="$mapId"/>,
    position: <xsl:value-of select="$jsPositionValue"/>
    });
    <!-- If Description - Create the bubble!! -->
    <xsl:if test="Strap/node()">
      <xsl:variable name="strapNode">
        <xsl:apply-templates select="." mode="processHTMLforJS"/>
      </xsl:variable>
      <!-- Html String -->
      var contentString<xsl:value-of select="@id"/> = '<xsl:copy-of select="$strapNode"/>';
      <!-- Info window-->
      var infowindow<xsl:value-of select="@id"/> = new google.maps.InfoWindow({
      content: contentString<xsl:value-of select="@id"/>
      });
      <!-- Click Listener -->
      google.maps.event.addListener(marker<xsl:value-of select="@id"/>, 'click', function () {
      infowindow<xsl:value-of select="@id"/>.open(gmap<xsl:value-of select="$mapId"/>, marker<xsl:value-of select="@id"/>);
      });
    </xsl:if>
  </xsl:template>

  <!--set up bubble for location-->
  <xsl:template match="Content[@type='Organisation']" mode="setGMapMarker">
    <xsl:param name="jsPositionValue"/>
    <xsl:param name="mapId"/>
    var marker<xsl:value-of select="@id"/> = new google.maps.Marker({
    map: gmap<xsl:value-of select="$mapId"/>,
    position: <xsl:value-of select="$jsPositionValue"/>
    });
    <!-- If Description - Create the bubble!! -->
    <xsl:if test="Strap/node()">
      <xsl:variable name="strapNode">
        <xsl:apply-templates select="." mode="processHTMLforJS"/>
      </xsl:variable>
      <!-- Html String -->
      var contentString<xsl:value-of select="@id"/> = '<xsl:copy-of select="$strapNode"/>';
      <!-- Info window-->
      var infowindow<xsl:value-of select="@id"/> = new google.maps.InfoWindow({
      content: contentString<xsl:value-of select="@id"/>
      });
      <!-- Click Listener -->
      google.maps.event.addListener(marker<xsl:value-of select="@id"/>, 'click', function () {
      infowindow<xsl:value-of select="@id"/>.open(gmap<xsl:value-of select="$mapId"/>, marker<xsl:value-of select="@id"/>);
      });
    </xsl:if>
  </xsl:template>


  <!-- Plotting location on map-->
  <xsl:template match="Content[@type='Location']" mode="getLocationsLocation">
    <xsl:param name="mapId"/>
    <xsl:choose>
      <xsl:when test="Location/Geo/@latitude != '' and Location/Geo/@longitude != ''">
        <xsl:variable name="jsLatLng">
          <xsl:apply-templates select="Location/Geo" mode="getJsLatLng"/>
        </xsl:variable>
        <xsl:apply-templates select="." mode="setGMapMarker">
          <xsl:with-param name="jsPositionValue" select="$jsLatLng"/>
          <xsl:with-param name="mapId" select="$mapId"/>
        </xsl:apply-templates>
      </xsl:when>
      <xsl:otherwise>
        var geocoder<xsl:value-of select="@id"/> = new google.maps.Geocoder();
        <!-- Goes and gets the Location of the Address -->
        geocoder<xsl:value-of select="@id"/>.geocode({ 'address': '<xsl:apply-templates select="Location/Address" mode="csvAddress"/>' }, function (results, status) {
        <!-- If result OK - Center map to location -->
        if (status == google.maps.GeocoderStatus.OK) {
        <!--  Marker this location -->
        <xsl:apply-templates select="." mode="setGMapMarker">
          <xsl:with-param name="jsPositionValue" select="'results[0].geometry.location'"/>
          <xsl:with-param name="mapId" select="$mapId"/>
        </xsl:apply-templates>
        }
        });
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="Content" mode="processHTMLforJS">
    <xsl:variable name="locName">
      <xsl:apply-templates select="Name/node()" mode="cleanXhtml-escape-js"/>
    </xsl:variable>
    <xsl:variable name="locStrap">
      <xsl:apply-templates select="Strap/*" mode="cleanXhtml"/>
    </xsl:variable>
    <xsl:variable name="locAddress">
      <xsl:apply-templates select="Location/Address" mode="getAddress" />
    </xsl:variable>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="mapPopup">
      <h3>
        <xsl:call-template name="escape-js">
          <xsl:with-param name="string">
            <xsl:value-of select="$locName"/>
          </xsl:with-param>
        </xsl:call-template>
      </h3>
      <xsl:if test="Location/@loc='address'">
        <xsl:apply-templates select="ms:node-set($locAddress)/*" mode="cleanXhtml-escape-js" />
      </xsl:if>
      <xsl:if test="Strap/node()!=''">
        <xsl:call-template name="escape-js-html">
          <xsl:with-param name="string">
            <xsl:copy-of select="$locStrap"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:if>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='Organisation']" mode="processHTMLforJS">
    <xsl:variable name="locName">
      <xsl:apply-templates select="Name/node()" mode="cleanXhtml-escape-js"/>
    </xsl:variable>
    <xsl:variable name="locStrap">
      <p>
        <xsl:copy-of select="Strap/node()"/>
      </p>
    </xsl:variable>
    <xsl:variable name="locAddress">
      <xsl:apply-templates select="Location/Address" mode="getAddress" />
    </xsl:variable>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="mapPopup">
      <h3>
        <xsl:call-template name="escape-js">
          <xsl:with-param name="string">
            <xsl:value-of select="$locName"/>
          </xsl:with-param>
        </xsl:call-template>
      </h3>
      <xsl:if test="Location/@loc='address'">
        <xsl:apply-templates select="ms:node-set($locAddress)/*" mode="cleanXhtml-escape-js" />
      </xsl:if>
      <xsl:if test="Strap/node()!=''">
        <xsl:call-template name="escape-js">
          <xsl:with-param name="string">
            <xsl:value-of select="$locStrap"/>
          </xsl:with-param>
        </xsl:call-template>
        <xsl:copy-of select="$locStrap"/>
        <!--xsl:apply-templates select="ms:node-set($locStrap)/*" mode="cleanXhtml-escape-js" /-->
      </xsl:if>
    </div>
  </xsl:template>

  <xsl:template match="*" mode="popupCleanXhtml">
    <xsl:element name="{local-name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:attribute name="{local-name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:choose>
        <xsl:when test="not(*) and node()">
          <xsl:call-template name="escape-js">
            <xsl:with-param name="string">
              <xsl:value-of select="node()"/>
            </xsl:with-param>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="*" mode="popupCleanXhtml"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:element>
  </xsl:template>

  <xsl:template match="Content[@type='error']" mode="ContentDetail">
    <xsl:apply-templates select="." mode="cleanXhtml"/>
  </xsl:template>



  <!--  ==  END GOOGLE MAPS v3  ==================================================================  -->

  <!-- ################ Location ################ -->

  <xsl:template match="Content[@type='Location']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <!-- articleBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="listItem location">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem location'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <h3 class="title">
          <a href="{$parentURL}" title="Read More - {Name/node()}" class="url summary" name="course_{@id}">
            <xsl:value-of select="Name/node()"/>
          </a>
        </h3>
        <p class="strap">
          <xsl:value-of select="Strap/node()"/>
        </p>
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='Location']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <div class="detail location">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail hnews newsarticle'"/>
      </xsl:apply-templates>
      <h2 class="entry-title content-title">
        <xsl:apply-templates select="." mode="getDisplayName" />
      </h2>
      <xsl:apply-templates select="." mode="displayDetailImage"/>
      <xsl:if test="@publish!=''">
        <p class="dtstamp" title="{@publish}">
          <xsl:call-template name="DisplayDate">
            <xsl:with-param name="date" select="@publish"/>
          </xsl:call-template>
        </p>
      </xsl:if>
      <h5>
        <xsl:apply-templates select="Strap/node()" mode="cleanXhtml"/>
      </h5>
      <div class="description entry-content">
        <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
      </div>
      <xsl:if test="Location/Venue!=''">
        <p class="location vcard">
          <span class="fn org">
            <xsl:value-of select="Location/Venue"/>
          </span>
          <xsl:if test="Location/@loc='address'">
            <xsl:apply-templates select="Location/Address" mode="getAddress" />
          </xsl:if>
          <xsl:if test="Location/@loc='geo'">
            <span class="geo">
              <span class="latitude">
                <span class="value-title" title="Location/Geo/@latitude"/>
              </span>
              <span class="longitude">
                <span class="value-title" title="Location/Geo/@longitude"/>
              </span>
            </span>
          </xsl:if>
        </p>
      </xsl:if>
      <!-- Terminus class fix to floating content -->
      <div class="terminus">&#160;</div>
      <div class="entryFooter">
        <div class="tags">
          <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
          <xsl:text> </xsl:text>
        </div>
        <xsl:apply-templates select="." mode="backLink">
          <xsl:with-param name="link" select="$thisURL"/>
          <xsl:with-param name="altText">
            <xsl:call-template name="term2006" />
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>


  <!-- ## Google Adverts  ###############################################################################   -->
  <xsl:template match="Content[@type='Module' and @moduleType='GoogleAdvertBank']" mode="displayBrief">
    <xsl:variable name="GoogleAdManagerId" select="/Page/Contents/Content[@name='GoogleAdManagerId']/node()" />
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div class="GoogleAdvertBank">
      <div class="cols{@cols}">
        <xsl:apply-templates select="." mode="inlinePopupRelate">
          <xsl:with-param name="type">GoogleAdvert</xsl:with-param>
          <xsl:with-param name="text">Add Advert</xsl:with-param>
          <xsl:with-param name="name"></xsl:with-param>
          <xsl:with-param name="find">true</xsl:with-param>
        </xsl:apply-templates>
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="articleList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
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
          <xsl:when test="ms:node-set($contentList)/* and $GoogleAdManagerId!=''">
            <script type='text/javascript' src='//partner.googleadservices.com/gampad/google_service.js'>&#160;</script>
            <script type='text/javascript'>
              <xsl:text>GS_googleAddAdSenseService("</xsl:text>
              <xsl:value-of select="$GoogleAdManagerId"/>
              <xsl:text>");</xsl:text>
              <xsl:text>GS_googleEnableAllServices();</xsl:text>
            </script>
            <script type='text/javascript'>
              <xsl:for-each select="ms:node-set($contentList)/*">
                <xsl:text>GA_googleAddSlot("</xsl:text>
                <xsl:value-of select="$GoogleAdManagerId"/>
                <xsl:text>", "</xsl:text>
                <xsl:value-of select="@adname"/>
                <xsl:text>");</xsl:text>
              </xsl:for-each>
            </script>
            <xsl:if test="/Page/Request/Form/Item[@name='searchString']/node()!=''">
              <script type='text/javascript'>
                <xsl:text>GA_googleAddAttr("search", "</xsl:text>
                <xsl:value-of select="/Page/Request/Form/Item[@name='searchString']/node()"/>
                <xsl:text>");</xsl:text>
              </script>
            </xsl:if>
            <xsl:if test="/Page/ContentDetail/Content[@type='Category']">
              <script type='text/javascript'>
                <xsl:text>GA_googleAddAttr("category", "</xsl:text>
                <xsl:value-of select="/Page/ContentDetail/Content[@type='Category']/@name"/>
                <xsl:text>");</xsl:text>
              </script>
            </xsl:if>
            <script type='text/javascript'>GA_googleFetchAds();</script>
            <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
              <xsl:with-param name="GoogleAdManagerId" select="$GoogleAdManagerId"/>
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
          </xsl:when>
        </xsl:choose>
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='GoogleAdvert']" mode="displayBrief">
    <xsl:param name="GoogleAdManagerId"/>
    <xsl:param name="sortBy"/>
    <div class="listItem googleadvert">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem googleadvert'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <xsl:comment>
        <xsl:text> </xsl:text>
        <xsl:value-of select="$GoogleAdManagerId"/>
        <xsl:text>/</xsl:text>
        <xsl:value-of select="@adname"/>
        <xsl:text> </xsl:text>
      </xsl:comment>
      <xsl:choose>
        <xsl:when test="$page/@adminMode">
          <p>
            <xsl:text>Ad Name: '</xsl:text>
            <xsl:value-of select="@adname"/>
            <xsl:text>'</xsl:text>
          </p>
          <p>
            <xsl:text>Website Placement: '</xsl:text>
            <xsl:value-of select="@name"/>
            <xsl:text>'</xsl:text>
          </p>
          <p>
            <em>Adverts are disabled in admin to avoid false impressions and clicks.</em>
          </p>
        </xsl:when>
        <xsl:otherwise>
          <span class="adContainer" title="{@adname}">
            <script type='text/javascript'>
              <xsl:text>GA_googleFillSlot("</xsl:text>
              <xsl:value-of select="@adname"/>
              <xsl:text>");</xsl:text>
            </script>
          </span>
        </xsl:otherwise>
      </xsl:choose>
    </div>
  </xsl:template>


  <!-- #### Email Form  ####   -->
  <!-- Email Form Module -->
  <xsl:template match="Content[@type='Module' and (@moduleType='EmailForm' or @moduleType='xForm')]" mode="displayBrief">
    <xsl:choose>
      <xsl:when test="descendant::alert/node()='Message Sent'">
        <xsl:apply-templates select="." mode="mailformSentMessage"/>
      </xsl:when>
      <xsl:otherwise>
        <div class="EmailForm">
          <xsl:if test="@formLayout='horizontal'">
            <xsl:attribute name="class">EmailForm horizontalForm</xsl:attribute>
          </xsl:if>
          <xsl:if test="@hideLabel='true'">
            <xsl:attribute name="class">EmailForm hideLabel</xsl:attribute>
          </xsl:if>
          <!-- display form-->
          <xsl:apply-templates select="." mode="cleanXhtml"/>
        </div>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:if test="/Page/@adminMode">
      <div class="sentMessage">
        <xsl:choose>
          <xsl:when test="Content[@type='FormattedText']">
            <xsl:apply-templates select="Content[@type='FormattedText']" mode="inlinePopupOptions">
              <xsl:with-param name="class" select="'sentMessage'"/>
            </xsl:apply-templates>
            <em>[submission message]</em>
            <xsl:apply-templates select="Content[@type='FormattedText']" mode="displayBrief"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="." mode="inlinePopupRelate">
              <xsl:with-param name="type">FormattedText</xsl:with-param>
              <xsl:with-param name="text">Add submission message</xsl:with-param>
              <xsl:with-param name="name">
                <xsl:text>Submission message </xsl:text>
                <xsl:value-of select="@title"/>
              </xsl:with-param>
              <xsl:with-param name="find">true</xsl:with-param>
            </xsl:apply-templates>
          </xsl:otherwise>
        </xsl:choose>
      </div>
    </xsl:if>
  </xsl:template>

  <!-- Email Sent Message -->
  <xsl:template match="Content" mode="mailformSentMessage">
    <xsl:choose>
      <!-- WHEN RELATED SENT MESSAGE -->
      <xsl:when test="Content[@type='FormattedText']">
        <div class="sentMessage">
          <xsl:apply-templates select="Content[@type='FormattedText']/node()" mode="cleanXhtml" />
        </div>
      </xsl:when>
      <!-- WHEN SENT MESSAGE ON PAGE -->
      <xsl:when test="/Page/Contents/Content[@name = 'sentMessage' and (@type='FormattedText' or @type='Image')]">
        <div class="sentMessage">
          <xsl:apply-templates select="/Page/Contents/Content[@name = 'sentMessage' and (@type='FormattedText' or @type='Image')]" mode="displayBrief"/>
        </div>
      </xsl:when>
      <!-- OTHERWISE SHOW FORM -->
      <xsl:otherwise>
        <xsl:apply-templates select="." mode="cleanXhtml"/>
      </xsl:otherwise>
    </xsl:choose>
    <!-- THEN TRACKING CODE -->
    <xsl:call-template name="LeadTracker"/>
  </xsl:template>

  <!-- Lead Tracker -->
  <xsl:template name="LeadTracker">
    <!-- OVERWRITE THIS TEMPLATE AND INSERT THE APPROPRIATE GOOGLE LEAD TRACKING JAVASCRIPT IF REQUIRED -->
  </xsl:template>

  <!-- Template to show an Xform when found in ContentDetail -->
  <xsl:template match="Content[@type='Module' and (@moduleType='EmailForm' or @moduleType='xForm')]" mode="cleanXhtml">
    <xsl:apply-templates select="." mode="xform"/>
  </xsl:template>

  <!-- X Form Module *** not finished *** PH-->
  <xsl:template match="Content[@type='Module' and @moduleType='XForm']" mode="displayBrief">
    <xsl:choose>
      <xsl:when test="/Page/Contents/Content/@name = 'sentMessage' and /Page/Contents/Content[@type='Module' and @moduleType='EmailForm']/descendant::alert/node()='Message Sent'">
        <xsl:apply-templates select="/" mode="mailformSentMessage"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:if test="@box='false'">
          <div class="EmailForm">
            <xsl:if test="@formLayout='horizontal'">
              <xsl:attribute name="class">EmailForm horizontalForm</xsl:attribute>
            </xsl:if>
            <xsl:if test="@hideLabel='true'">
              <xsl:attribute name="class">EmailForm hideLabel</xsl:attribute>
            </xsl:if>
          </div>
        </xsl:if>
        <xsl:apply-templates select="." mode="xform"/>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:if test="/Page/@adminMode">
      <div id="sentMessage">
        <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
          <xsl:with-param name="type">FormattedText</xsl:with-param>
          <xsl:with-param name="text">Add Sent Message</xsl:with-param>
          <xsl:with-param name="name">sentMessage</xsl:with-param>
        </xsl:apply-templates>
        <xsl:apply-templates select="/Page/Contents/Content[@name = 'sentMessage' and (@type='FormattedText' or @type='Image')]" mode="displayBrief"/>
      </div>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Content[@type='xform']" mode="ContentDetail">
    <xsl:apply-templates select="." mode="xform"/>
  </xsl:template>


  



  

  




  <!--   ################   Training Course   ###############   -->
  <!-- Training Course Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='TrainingCourseList']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!--responsive columns variables-->
    <xsl:variable name="xsColsToShow">
      <xsl:choose>
        <xsl:when test="@xsCol='2'">2</xsl:when>
        <xsl:otherwise>1</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="smColsToShow">
      <xsl:choose>
        <xsl:when test="@smCol and @smCol!=''">
          <xsl:value-of select="@smCol"/>
        </xsl:when>
        <xsl:otherwise>2</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="mdColsToShow">
      <xsl:choose>
        <xsl:when test="@mdCol and @mdCol!=''">
          <xsl:value-of select="@mdCol"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@cols"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!--end responsive columns variables-->
    <div class="clearfix EventsList TrainingList">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix EventsList content-scroller</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <div class="cols cols{@cols}" data-xscols="{$xsColsToShow}" data-smcols="{$smColsToShow}" data-mdcols="{$mdColsToShow}" data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1"  data-dots="{@carouselBullets}" data-height="{@carouselHeight}" >
        <!--responsive columns-->
        <xsl:attribute name="class">
          <xsl:text>cols</xsl:text>
          <xsl:choose>
            <xsl:when test="@xsCol='2'"> mobile-2-col-content</xsl:when>
            <xsl:otherwise> mobile-1-col-content</xsl:otherwise>
          </xsl:choose>
          <xsl:if test="@smCol and @smCol!=''">
            <xsl:text> sm-content-</xsl:text>
            <xsl:value-of select="@smCol"/>
          </xsl:if>
          <xsl:if test="@mdCol and @mdCol!=''">
            <xsl:text> md-content-</xsl:text>
            <xsl:value-of select="@mdCol"/>
          </xsl:if>
          <xsl:text> cols</xsl:text>
          <xsl:value-of select="@cols"/>
          <xsl:if test="@mdCol and @mdCol!=''">
            <xsl:text> content-cols-responsive</xsl:text>
          </xsl:if>
        </xsl:attribute>
        <!--end responsive columns-->
        <xsl:if test="@autoplay !=''">
          <xsl:attribute name="data-autoplay">
            <xsl:value-of select="@autoplay"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="@autoPlaySpeed !=''">
          <xsl:attribute name="data-autoPlaySpeed">
            <xsl:value-of select="@autoPlaySpeed"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
        <xsl:if test="@stepCount != '0'">
          <div class="terminus">&#160;</div>
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="articleList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:text> </xsl:text>
      </div>
    </div>
  </xsl:template>

  <!-- TrainingCourse Brief -->
  <xsl:template match="Content[@type='TrainingCourse']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <!-- articleBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="listItem list-group-item vevent">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem list-group-item vevent'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner media">
        <xsl:if test="Images/img/@src!=''">
          <a href="{$parentURL}" title="Read More - {Headline/node()}">
            <xsl:apply-templates select="." mode="displayThumbnail"/>
          </a>
        </xsl:if>
        <div class="media-body">
          <h4 class="media-heading">
            <a href="{$parentURL}" title="Read More - {Headline/node()}" class="url summary">
              <xsl:apply-templates select="." mode="getDisplayName"/>
            </a>
          </h4>
          <xsl:if test="Strap/node()!=''">
            <div class="summary">
              <xsl:apply-templates select="Strap/node()" mode="cleanXhtml"/>
            </div>
          </xsl:if>
          <div class="Coursedetails">
            <h5>
              <xsl:call-template name="term2115"/>
              <xsl:text>: </xsl:text>
              <strong>
                <xsl:choose>
                  <xsl:when test="Content[@type='Ticket']">
                    <xsl:for-each select="Content[@type='Ticket']">
                      <xsl:sort select="StartDate" order="ascending"/>
                      <xsl:if test="position()=1">
                        <xsl:call-template name="formatdate">
                          <xsl:with-param name="date" select="StartDate" />
                          <xsl:with-param name="format" select="'dddd'" />
                        </xsl:call-template>
                        <xsl:text> </xsl:text>
                        <xsl:call-template name="DD_Mon_YYYY">
                          <xsl:with-param name="date" select="StartDate"/>
                        </xsl:call-template>
                      </xsl:if>
                    </xsl:for-each>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="@Headline"/>
                  </xsl:otherwise>
                </xsl:choose>
              </strong>
            </h5>
            <xsl:for-each select="Content[@type='Ticket']">
              <xsl:sort select="StartDate" order="ascending"/>
              <xsl:if test="position()=1">
                <h5>
                  <!--Time-->
                  <xsl:call-template name="term4027"/>
                  <xsl:text>: </xsl:text>
                  <strong>
                    <xsl:if test="Times/@start!='' and Times/@start!=','">
                      <span class="times">
                        <span class="starttime">
                          <xsl:value-of select="translate(Times/@start,',',':')"/>
                        </span>
                        <xsl:if test="Times/@end!='' and Times/@end!=','">
                          <xsl:text> - </xsl:text>
                          <span class="finstart">
                            <xsl:value-of select="translate(Times/@end,',',':')"/>
                          </span>
                        </xsl:if>
                      </span>
                    </xsl:if>
                  </strong>
                </h5>
              </xsl:if>
            </xsl:for-each>
            <xsl:choose>
              <xsl:when test="Content[@type='Ticket']">
                <h5>
                  <!--Next Course:-->
                  <xsl:call-template name="term2116"/>
                  <xsl:text>: </xsl:text>
                  <strong>
                    <xsl:for-each select="Content[@type='Ticket']">
                      <xsl:sort select="StartDate" order="ascending"/>
                      <xsl:if test="position()=1">
                        <!--!Upgrade needed on price - Will !<xsl:apply-templates select="." mode="displayPrice" />-->
                        <xsl:choose>
                          <xsl:when test="Content[@type='SKU']">
                            <xsl:apply-templates select="Content[@type='SKU'][1]" mode="displayPrice" />
                          </xsl:when>
                          <xsl:otherwise>
                            <xsl:apply-templates select="." mode="displayPrice" />
                          </xsl:otherwise>
                        </xsl:choose>
                      </xsl:if>
                    </xsl:for-each>
                  </strong>
                </h5>
              </xsl:when>
              <xsl:otherwise>
                <xsl:text> </xsl:text>
              </xsl:otherwise>
            </xsl:choose>
          </div>
          <div class="entryFooter">
            <xsl:apply-templates select="." mode="moreLink">
              <xsl:with-param name="link" select="$parentURL"/>
              <xsl:with-param name="altText">
                <xsl:value-of select="Headline/node()"/>
              </xsl:with-param>
            </xsl:apply-templates>
            <xsl:text> </xsl:text>
          </div>
        </div>
        <!-- Accessiblity fix : Separate adjacent links with more than whitespace -->
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- Training Course Detail -->
  <xsl:template match="Content[@type='TrainingCourse']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <xsl:choose>
      <xsl:when test="Content[@type='Ticket'] or @adminMode or $page/Contents/Content[@name='TrainingCourseRequest']">
        <div class="row detail vevent">
          <div class="col-md-8">
            <div class="training-desc">
              <xsl:apply-templates select="." mode="inlinePopupOptions">
                <xsl:with-param name="class" select="'training-desc'"/>
              </xsl:apply-templates>
              <h2 class="content-title summary">
                <xsl:apply-templates select="." mode="getDisplayName"/>
              </h2>
              <xsl:apply-templates select="." mode="displayDetailImage"/>
              <xsl:if test="StartDate!=''">
                <p class="date">
                  <xsl:if test="StartDate/node()!=''">
                    <xsl:call-template name="DisplayDate">
                      <xsl:with-param name="date" select="StartDate/node()"/>
                    </xsl:call-template>
                  </xsl:if>
                  <xsl:if test="EndDate/node()!=StartDate/node()">
                    <xsl:text> to </xsl:text>
                    <xsl:call-template name="DisplayDate">
                      <xsl:with-param name="date" select="EndDate/node()"/>
                    </xsl:call-template>
                  </xsl:if>
                  <xsl:text>&#160;</xsl:text>
                  <xsl:if test="Times/@start!='' and Times/@start!=','">
                    <span class="times">
                      <xsl:value-of select="translate(Times/@start,',',':')"/>
                      <xsl:if test="Times/@end!='' and Times/@end!=','">
                        <xsl:text> - </xsl:text>
                        <xsl:value-of select="translate(Times/@end,',',':')"/>
                      </xsl:if>
                    </span>
                  </xsl:if>
                </p>
              </xsl:if>
              <xsl:if test="Location/Venue!=''">
                <p class="location vcard">
                  <span class="fn org">
                    <xsl:value-of select="Location/Venue"/>
                  </span>
                  <xsl:if test="Location/@loc='address'">
                    <xsl:apply-templates select="Location/Address" mode="getAddress" />
                  </xsl:if>
                  <xsl:if test="Location/@loc='geo'">
                    <span class="geo">
                      <span class="latitude">
                        <span class="value-title" title="Location/Geo/@latitude"/>
                      </span>
                      <span class="longitude">
                        <span class="value-title" title="Location/Geo/@longitude"/>
                      </span>
                    </span>
                  </xsl:if>
                </p>
              </xsl:if>
              <div class="description">
                <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
              </div>
              <div class="entryFooter">
                <div class="tags">
                  <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
                  <xsl:text> </xsl:text>
                </div>
                <xsl:apply-templates select="." mode="backLink">
                  <xsl:with-param name="link" select="$thisURL"/>
                  <xsl:with-param name="altText">
                    <xsl:call-template name="term2013" />
                  </xsl:with-param>
                </xsl:apply-templates>
              </div>
            </div>
          </div>
          <div class="col-md-4"  id="buyPanels">
            <xsl:apply-templates select="." mode="inlinePopupRelate">
              <xsl:with-param name="type">Ticket</xsl:with-param>
              <xsl:with-param name="text">Add Ticket</xsl:with-param>
              <xsl:with-param name="name"></xsl:with-param>
              <xsl:with-param name="find">false</xsl:with-param>
            </xsl:apply-templates>
            <xsl:if test="Content[@type='Ticket']">
              <div class="book-course">
                <h3>Book this course</h3>
                <div class="dates row">
                  <h4>Which Day would you like to attend?</h4>
                  <ul role="tablist">
                    <xsl:apply-templates select="Content[@type='Ticket']" mode="displayBriefDate">
                      <xsl:sort select="StartDate" order="ascending"/>
                    </xsl:apply-templates>
                  </ul>
                </div>
                <div class="ticket-amt tab-content">
                  <xsl:apply-templates select="Content[@type='Ticket']" mode="BuyDateTickets"/>
                </div>
              </div>
            </xsl:if>
            <xsl:if test="$adminMode='true' or not(Content[@type='Ticket'])">
              <div id="enquiry" class="hidden-print book-course course-form">
                <h2 class="title">
                  Request the Course
                </h2>
                <xsl:choose>
                  <xsl:when test="/Page/Contents/Content/@name='sentMessage' and /Page/Contents/Content[@type='xform' and @name='TrainingCourseRequest']/descendant::alert/node()='Message Sent'">
                    <xsl:apply-templates select="/Page/Contents/Content[@name='sentMessage']" mode="mailformSentMessage"/>
                  </xsl:when>
                  <xsl:when test="/Page/Contents/Content[@type='xform' and @name='TrainingCourseRequest']/descendant::alert/node()='Message Sent'">
                    <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='TrainingCourseRequest']/descendant::alert[node()='Message Sent']" mode="xform"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='TrainingCourseRequest']" mode="xform"/>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:if test="/Page/@adminMode">
                  <div id="sentMessage">
                    <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
                      <xsl:with-param name="type">FormattedText</xsl:with-param>
                      <xsl:with-param name="text">Add Sent Message</xsl:with-param>
                      <xsl:with-param name="name">sentMessage</xsl:with-param>
                    </xsl:apply-templates>
                    <xsl:apply-templates select="/Page/Contents/Content[@name='sentMessage' and (@type='FormattedText' or @type='Image')]" mode="displayBrief"/>
                  </div>
                </xsl:if>
              </div>
            </xsl:if>
          </div>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <div class="detail vevent content-title">
          <xsl:apply-templates select="." mode="inlinePopupOptions">
            <xsl:with-param name="class" select="'detail vevent content-title'"/>
          </xsl:apply-templates>
          <h2 class="summary">
            <xsl:apply-templates select="." mode="getDisplayName"/>
          </h2>
          <xsl:apply-templates select="." mode="displayDetailImage"/>
          <xsl:if test="StartDate!=''">
            <p class="date">
              <xsl:if test="StartDate/node()!=''">
                <xsl:call-template name="DisplayDate">
                  <xsl:with-param name="date" select="StartDate/node()"/>
                </xsl:call-template>
              </xsl:if>
              <xsl:if test="EndDate/node()!=StartDate/node()">
                <xsl:text> to </xsl:text>
                <xsl:call-template name="DisplayDate">
                  <xsl:with-param name="date" select="EndDate/node()"/>
                </xsl:call-template>
              </xsl:if>
              <xsl:text>&#160;</xsl:text>
              <xsl:if test="Times/@start!='' and Times/@start!=','">
                <span class="times">
                  <xsl:value-of select="translate(Times/@start,',',':')"/>
                  <xsl:if test="Times/@end!='' and Times/@end!=','">
                    <xsl:text> - </xsl:text>
                    <xsl:value-of select="translate(Times/@end,',',':')"/>
                  </xsl:if>
                </span>
              </xsl:if>
            </p>
          </xsl:if>
          <xsl:if test="Location/Venue!=''">
            <p class="location vcard">
              <span class="fn org">
                <xsl:value-of select="Location/Venue"/>
              </span>
              <xsl:if test="Location/@loc='address'">
                <xsl:apply-templates select="Location/Address" mode="getAddress" />
              </xsl:if>
              <xsl:if test="Location/@loc='geo'">
                <span class="geo">
                  <span class="latitude">
                    <span class="value-title" title="Location/Geo/@latitude"/>
                  </span>
                  <span class="longitude">
                    <span class="value-title" title="Location/Geo/@longitude"/>
                  </span>
                </span>
              </xsl:if>
            </p>
          </xsl:if>
          <xsl:if test="Content[@type='Ticket']">
            <h5>Upcoming Courses:</h5>
            <xsl:apply-templates select="Content[@type='Ticket']" mode="displayBriefDate"/>
          </xsl:if>
          <div class="description">
            <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
          </div>
          <div class="entryFooter">
            <div class="tags">
              <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
              <xsl:text> </xsl:text>
            </div>
            <xsl:apply-templates select="." mode="backLink">
              <xsl:with-param name="link" select="$thisURL"/>
              <xsl:with-param name="altText">
                <xsl:call-template name="term2013" />
              </xsl:with-param>
            </xsl:apply-templates>
          </div>
          <div class="terminus">&#160;</div>
        </div>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- List Related Tickets-->
  <xsl:template match="Content[@type='Ticket']" mode="BuyDateTickets">
    <xsl:param name="sortBy"/>
    <xsl:param name="parProductID"/>
    <div class="tickets panel panel-default tab-pane" id="{@id}-buyPanel" role="tabpanel">
      <form action="" method="post" class="ewXform ProductAddForm">
        <div class="panel-heading">
          <h3 class="panel-title">
            How many people are going to attend?
          </h3>
        </div>
        <div class="ticketsGrouped panel-body">
          <xsl:for-each select=".">
            <xsl:sort select="@type" order="ascending"/>
            <xsl:sort select="@displayOrder" order="ascending"/>
            <xsl:apply-templates select="." mode="displayBriefTicketNew"/>
          </xsl:for-each>
        </div>
        <div class="panel-footer">
          <span class="pull-right">
            <xsl:apply-templates select="/" mode="addtoCartButtons"/>
          </span>
        </div>
      </form>
    </div>
  </xsl:template>

  <!-- List Related Tickets-->
  <xsl:template match="Content" mode="BuyRelatedTickets">
    <xsl:param name="sortBy"/>
    <xsl:param name="parProductID"/>
    <form action="" method="post" class="ewXform ProductAddForm">
      <div class="tickets panel panel-default">
        <div class="ticketsGrouped table">
          <xsl:for-each select="/Page/ContentDetail/Content/Content[@type='Ticket']">
            <xsl:sort select="@type" order="ascending"/>
            <xsl:sort select="@displayOrder" order="ascending"/>
            <xsl:apply-templates select="." mode="displayBriefTicket"/>
          </xsl:for-each>
          <div class="terminus">&#160;</div>
        </div>
        <div class="panel-footer">
          <span class="pull-right">
            <xsl:apply-templates select="/" mode="addtoCartButtons"/>
          </span>
        </div>
      </div>
    </form>
  </xsl:template>

  <!-- Ticket related products -->
  <xsl:template match="Content[@type='Ticket']" mode="displayBriefTicketNew">
    <xsl:param name="sortBy"/>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="ListGroupedTitle cell">
      <xsl:variable name="title">
        <xsl:apply-templates select="." mode="getDisplayName"/>
      </xsl:variable>
      <xsl:value-of select="$title"/>
    </div>
    <div class="ListGroupedTitle cell">
      <xsl:if test="StartDate/node()!=''">
        <xsl:call-template name="DisplayDate">
          <xsl:with-param name="date" select="StartDate/node()"/>
        </xsl:call-template>
      </xsl:if>
      <xsl:if test="EndDate/node()!=StartDate/node()">
        <xsl:text> to </xsl:text>
        <xsl:call-template name="DisplayDate">
          <xsl:with-param name="date" select="EndDate/node()"/>
        </xsl:call-template>
      </xsl:if>
      <xsl:text>&#160;</xsl:text>
      <xsl:if test="Times/@start!='' and Times/@start!=','">
        <span class="times">
          <xsl:value-of select="translate(Times/@start,',',':')"/>
          <xsl:if test="Times/@end!='' and Times/@end!=','">
            <xsl:text> - </xsl:text>
            <xsl:value-of select="translate(Times/@end,',',':')"/>
          </xsl:if>
        </span>
      </xsl:if>
    </div>
    <div class="ListGroupedPrice cell">
      <p class="productBrief">
        <!--!Upgrade needed on price - Will !<xsl:apply-templates select="." mode="displayPrice" />-->
        <xsl:choose>
          <xsl:when test="Content[@type='SKU']">
            <xsl:apply-templates select="Content[@type='SKU'][1]" mode="displayPrice" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="." mode="displayPrice" />
          </xsl:otherwise>
        </xsl:choose>
      </p>
    </div>
    <div class="ListGroupedQty cell pull-right">
      <xsl:choose>
        <xsl:when test="$page/@adminMode">
          <div>
            <xsl:apply-templates select="." mode="inlinePopupOptions" >
              <xsl:with-param name="class" select="'hproduct'"/>
              <xsl:with-param name="sortBy" select="$sortBy"/>
            </xsl:apply-templates>
          </div>
        </xsl:when>
      </xsl:choose>
      <xsl:apply-templates select="." mode="showQuantityGrouped"/>
    </div>
  </xsl:template>

 

 

  <!--   ################   Gallery Images   ###############   -->

  <!-- Images Cover Flow Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='ImageCoverFlow']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$page/@adminMode">
        <div>
          <xsl:apply-templates select="." mode="inlinePopupRelate">
            <xsl:with-param name="type">
              <xsl:value-of select="@contentType"/>
            </xsl:with-param>
            <xsl:with-param name="text">Add Image</xsl:with-param>
            <xsl:with-param name="name"></xsl:with-param>
            <xsl:with-param name="find">true</xsl:with-param>
          </xsl:apply-templates>
          <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
            <xsl:with-param name="sortBy" select="@sortBy"/>
            <xsl:with-param name="module" select="."/>
          </xsl:apply-templates>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <!-- Output Module -->
        <div id="myImageFlow" class="imageflow">
          <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayCoverFlowBrief">
            <xsl:with-param name="sortBy" select="@sortBy"/>
            <xsl:with-param name="module" select="."/>
          </xsl:apply-templates>
        </div>
      </xsl:otherwise>
    </xsl:choose>
    <div class="terminus">&#160;</div>
  </xsl:template>

  <!-- ## IMAGE FLOW IMAGE CONTENT TYPE  ###############################################   -->
  <xsl:template match="Content[@type='Image']" mode="displayCoverFlowBrief">
    <xsl:param name="module"/>
    <xsl:variable name="id">
      <xsl:text>image</xsl:text>
      <xsl:value-of select="@id"/>
    </xsl:variable>
    <xsl:variable name="src">
      <xsl:choose>
        <xsl:when test="$module/@reflection='true'">
          <xsl:text>/ewcommon/tools/reflectImage.ashx?img=</xsl:text>
          <xsl:value-of select="img/@src"/>
          <xsl:text>&amp;red=</xsl:text>
          <xsl:value-of select="$module/@red"/>
          <xsl:text>&amp;blue=</xsl:text>
          <xsl:value-of select="$module/@blue"/>
          <xsl:text>&amp;green=</xsl:text>
          <xsl:value-of select="$module/@green"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="img/@src"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="longdesc">
      <xsl:choose>
        <xsl:when test="@internalLink!=''">
          <xsl:variable name="pageId" select="@internalLink"/>
          <xsl:apply-templates select="$page/descendant-or-self::MenuItem[@id=$pageId]" mode="getHref" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@externalLink"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="title">
      <xsl:choose>
        <xsl:when test="@internalLink!=''">
          <xsl:variable name="pageId" select="@internalLink"/>
          <xsl:apply-templates select="$page/descendant-or-self::MenuItem[@id=$pageId]" mode="getDisplayName" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@name"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <img longdesc="{$longdesc}" id="{$id}" src="{$src}" alt="{img/@alt}" title="{$title}" height="{img/@height}" width="{img/@width}" style="{@style}"/>
  </xsl:template>
  <!-- -->

  <!--   ################   Gallery Images   ###############   -->

  <!-- Gallery Images List Module -->
  <xsl:template match="Content[(@type='Module' and @moduleType='GalleryImageList') or Content[@type='LibraryImage']]" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <Content>
        <xsl:for-each select="@*">
          <xsl:attribute name="{name()}">
            <xsl:value-of select="."/>
          </xsl:attribute>
        </xsl:for-each>
        <xsl:apply-templates select="." mode="getContent">
          <xsl:with-param name="contentType" select="$contentType" />
          <xsl:with-param name="startPos" select="$startPos" />
        </xsl:apply-templates>
      </Content>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!--responsive columns variables-->
    <xsl:variable name="xsColsToShow">
      <xsl:choose>
        <xsl:when test="@xsCol='2'">2</xsl:when>
        <xsl:otherwise>1</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="smColsToShow">
      <xsl:choose>
        <xsl:when test="@smCol and @smCol!=''">
          <xsl:value-of select="@smCol"/>
        </xsl:when>
        <xsl:otherwise>2</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="mdColsToShow">
      <xsl:choose>
        <xsl:when test="@mdCol and @mdCol!=''">
          <xsl:value-of select="@mdCol"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@cols"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- Output Module -->
    <div class="GalleryImageList Grid">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix GalleryImageList Grid content-scroller</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <div class="cols cols{@cols}" data-xscols="{$xsColsToShow}" data-smcols="{$smColsToShow}" data-mdcols="{$mdColsToShow}" data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1" data-dots="{@carouselBullets}" data-height="{@carouselHeight}" >
        <!--responsive columns-->
        <xsl:attribute name="class">
          <xsl:text>cols</xsl:text>
          <xsl:choose>
            <xsl:when test="@xsCol='2'"> mobile-2-col-content</xsl:when>
            <xsl:otherwise> mobile-1-col-content</xsl:otherwise>
          </xsl:choose>
          <xsl:if test="@smCol and @smCol!=''">
            <xsl:text> sm-content-</xsl:text>
            <xsl:value-of select="@smCol"/>
          </xsl:if>
          <xsl:if test="@mdCol and @mdCol!=''">
            <xsl:text> md-content-</xsl:text>
            <xsl:value-of select="@mdCol"/>
          </xsl:if>
          <xsl:text> cols</xsl:text>
          <xsl:value-of select="@cols"/>
          <xsl:if test="@mdCol and @mdCol!=''">
            <xsl:text> content-cols-responsive</xsl:text>
          </xsl:if>
        </xsl:attribute>
        <!--end responsive columns-->
        <xsl:if test="@autoplay !=''">
          <xsl:attribute name="data-autoplay">
            <xsl:value-of select="@autoplay"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="@autoPlaySpeed !=''">
          <xsl:attribute name="data-autoPlaySpeed">
            <xsl:value-of select="@autoPlaySpeed"/>
          </xsl:attribute>
        </xsl:if>
        <!-- If Stepper, display Stepper -->
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="GalleryImageList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:apply-templates select="ms:node-set($contentList)/*/*" mode="displayBrief">
          <xsl:with-param name="sortBy" select="@sortBy"/>
          <xsl:with-param name="crop" select="@crop"/>
          <xsl:with-param name="lightbox" select="@lightbox"/>
          <xsl:with-param name="showTitle" select="@showTitle"/>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <!-- Library Image Brief -->
  <xsl:template match="Content[@type='LibraryImage']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <xsl:param name="crop"/>
    <xsl:param name="lightbox"/>
    <xsl:param name="showTitle"/>
    <xsl:variable name="cropSetting">
      <xsl:choose>
        <xsl:when test="$crop='false'">
          <xsl:value-of select="false()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="true()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="lg-max-width">
      <xsl:apply-templates select="." mode="getFullSizeWidth"/>
    </xsl:variable>
    <xsl:variable name="lg-max-height">
      <xsl:apply-templates select="." mode="getFullSizeHeight"/>
    </xsl:variable>
    <xsl:variable name="fullSize">
      <xsl:call-template name="resize-image">
        <xsl:with-param name="path" select="Images/img[@class='detail']/@src"/>
        <xsl:with-param name="max-width" select="$lg-max-width"/>
        <xsl:with-param name="max-height" select="$lg-max-height"/>
        <xsl:with-param name="file-prefix">
          <xsl:text>~lg-</xsl:text>
          <xsl:value-of select="$lg-max-width"/>
          <xsl:text>x</xsl:text>
          <xsl:value-of select="$lg-max-width"/>
          <xsl:text>/~lg-</xsl:text>
        </xsl:with-param>
        <xsl:with-param name="file-suffix" select="''"/>
        <xsl:with-param name="quality" select="100"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="lgImgSrc">
      <xsl:choose>
        <xsl:when test="Images/img[@class='detail']/@src != ''">
          <xsl:value-of select="Images/img[@class='detail']/@src"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="Images/img[@class='display']/@src"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$page[@cssFramework='bs3']">
        <div class="grid-item">
          <xsl:apply-templates select="." mode="inlinePopupOptions">
            <xsl:with-param name="class" select="'grid-item '"/>
            <xsl:with-param name="sortBy" select="$sortBy"/>
          </xsl:apply-templates>
          <xsl:choose>
            <xsl:when test="$lightbox='false'">
              <div class="thumbnail-wrapper">
                <div class="thumbnail">
                  <xsl:apply-templates select="." mode="displayThumbnail">
                    <xsl:with-param name="crop" select="$cropSetting" />
                    <xsl:with-param name="class" select="'img-responsive'" />
                    <xsl:with-param name="style" select="'overflow:hidden;'" />
                    <xsl:with-param name="width" select="$lg-max-width"/>
                    <xsl:with-param name="height" select="$lg-max-height"/>
                  </xsl:apply-templates>
                  <xsl:if test="(Title/node()!='' or Body/node()!='') and not($showTitle='false')">
                    <div class="caption">
                      <h4>
                        <xsl:value-of select="Title/node()"/>
                      </h4>
                      <xsl:apply-templates select="Body/node()" mode="cleanXhtml" />
                    </div>
                  </xsl:if>
                </div>
              </div>
            </xsl:when>
            <xsl:otherwise>
              <a href="{$fullSize}" title="{Title/node()} - {Body/node()}" class="responsive-lightbox">
                <div class="thumbnail">
                  <xsl:apply-templates select="." mode="displayThumbnail">
                    <xsl:with-param name="crop" select="$cropSetting" />
                    <xsl:with-param name="class" select="'img-responsive'" />
                    <xsl:with-param name="style" select="'overflow:hidden;'" />
                    <xsl:with-param name="width" select="$lg-max-width"/>
                    <xsl:with-param name="height" select="$lg-max-height"/>
                  </xsl:apply-templates>
                  <xsl:if test="Title/node()!='' or Body/node()!=''">
                    <div class="caption">
                      <h4>
                        <xsl:value-of select="Title/node()"/>
                      </h4>
                      <xsl:apply-templates select="Body/node()" mode="cleanXhtml" />
                    </div>
                  </xsl:if>
                </div>
              </a>
            </xsl:otherwise>
          </xsl:choose>

        </div>
      </xsl:when>
      <xsl:otherwise>

        <div class="listItem">
          <xsl:apply-templates select="." mode="inlinePopupOptions">
            <xsl:with-param name="class" select="'listItem libraryimage'"/>
            <xsl:with-param name="sortBy" select="$sortBy"/>
          </xsl:apply-templates>
          <div class="lIinner">
            <h3>
              <xsl:choose>
                <xsl:when test="$fullSize != ''">
                  <a href="{$fullSize}" title="{Title/node()}" class="lightbox">
                    <xsl:attribute name="title">
                      <xsl:value-of select="Title/node()"/>
                      <xsl:if test="Author/node()">
                        <xsl:text> - </xsl:text>
                        <xsl:value-of select="Author/node()"/>
                      </xsl:if>
                    </xsl:attribute>
                    <xsl:apply-templates select="." mode="displayThumbnail">
                      <xsl:with-param name="crop" select="true()" />
                    </xsl:apply-templates>
                  </a>
                </xsl:when>
                <xsl:otherwise>
                  <a href="{$fullSize}" title="{Title/node()} - {Body/node()}">
                    <xsl:apply-templates select="." mode="displayThumbnail"/>
                  </a>
                </xsl:otherwise>

              </xsl:choose>
              <xsl:if test="Title/node()!=''">
                <div class="caption">
                  <a href="{$fullSize}" title="{Title/node()} - {Body/node()}" class="title">
                    <xsl:value-of select="Title/node()"/>
                  </a>
                </div>
              </xsl:if>
            </h3>
          </div>
        </div>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!--   ################   Products   ###############   -->
  <!-- Product Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='ProductList']" mode="displayBrief">
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!--responsive columns variables-->
    <xsl:variable name="xsColsToShow">
      <xsl:choose>
        <xsl:when test="@xsCol='2'">2</xsl:when>
        <xsl:otherwise>1</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="smColsToShow">
      <xsl:choose>
        <xsl:when test="@smCol and @smCol!=''">
          <xsl:value-of select="@smCol"/>
        </xsl:when>
        <xsl:otherwise>2</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="mdColsToShow">
      <xsl:choose>
        <xsl:when test="@mdCol and @mdCol!=''">
          <xsl:value-of select="@mdCol"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@cols"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div class="clearfix ProductList">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix ProductList content-scroller</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <div class="cols cols{@cols}" data-xscols="{$xsColsToShow}" data-smcols="{$smColsToShow}" data-mdcols="{$mdColsToShow}" data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1" >
        <xsl:attribute name="class">
          <xsl:text>cols</xsl:text>
          <xsl:choose>
            <xsl:when test="@xsCol='2'"> mobile-2-col-content</xsl:when>
            <xsl:otherwise> mobile-1-col-content</xsl:otherwise>
          </xsl:choose>
          <xsl:if test="@smCol and @smCol!=''">
            <xsl:text> sm-content-</xsl:text>
            <xsl:value-of select="@smCol"/>
          </xsl:if>
          <xsl:if test="@mdCol and @mdCol!=''">
            <xsl:text> md-content-</xsl:text>
            <xsl:value-of select="@mdCol"/>
          </xsl:if>
          <xsl:text> cols</xsl:text>
          <xsl:value-of select="@cols"/>
          <xsl:if test="@mdCol and @mdCol!=''">
            <xsl:text> content-cols-responsive</xsl:text>
          </xsl:if>
        </xsl:attribute>
        <xsl:if test="@autoplay !=''">
          <xsl:attribute name="data-autoplay">
            <xsl:value-of select="@autoplay"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="@autoPlaySpeed !=''">
          <xsl:attribute name="data-autoPlaySpeed">
            <xsl:value-of select="@autoPlaySpeed"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
        <xsl:if test="@stepCount != '0'">
          <div class="terminus">&#160;</div>
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="articleList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:text> </xsl:text>
      </div>
    </div>
  </xsl:template>

  <!-- Product Brief -->
  <xsl:template match="Content[@type='Product']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <xsl:param name="pos"/>
    <xsl:variable name="parId">
      <xsl:choose>
        <xsl:when test="@parId &gt; 0">
          <xsl:value-of select="@parId"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="/Page/@id"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="self::Content" mode="getHref">
        <xsl:with-param name="parId" select="$parId"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="listItem list-group-item hproduct">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem list-group-item hproduct'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <h3 class="fn title">
          <xsl:variable name="title">
            <xsl:apply-templates select="." mode="getDisplayName"/>
          </xsl:variable>
          <a href="{$parentURL}" title="{$title}">
            <xsl:value-of select="$title"/>
          </a>
        </h3>
        <xsl:if test="Images/img/@src!=''">
          <a href="{$parentURL}" class="url">
            <xsl:apply-templates select="." mode="displayThumbnail"/>
          </a>
        </xsl:if>
        <xsl:if test="Manufacturer/node()!=''">
          <p class="manufacturer">
            <xsl:if test="/Page/Contents/Content[@name='makeLabel']">
              <span class="label">
                <xsl:value-of select="/Page/Contents/Content[@name='makeLabel']"/>
              </span>&#160;
            </xsl:if>
            <span class="brand">
              <xsl:value-of select="Manufacturer/node()"/>
            </span>
          </p>
        </xsl:if>
        <xsl:if test="StockCode/node()!=''">
          <p class="sku stockCode">
            <span class="label">
              <xsl:call-template name="term2014" />
              <xsl:text>: </xsl:text>
            </span>
            <xsl:value-of select="StockCode/node()"/>
          </p>
        </xsl:if>
        <xsl:choose>
          <xsl:when test="Content[@type='SKU']">
            <xsl:apply-templates select="Content[@type='SKU'][1]" mode="displayPrice" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="." mode="displayPrice" />
          </xsl:otherwise>
        </xsl:choose>
        <xsl:if test="ShortDescription/node()!=''">
          <div class="description">
            <xsl:apply-templates select="ShortDescription/node()" mode="cleanXhtml"/>
          </div>
        </xsl:if>
        <div class="entryFooter">
          <xsl:apply-templates select="." mode="moreLink">
            <xsl:with-param name="link" select="$parentURL"/>
            <xsl:with-param name="altText">
              <xsl:apply-templates select="." mode="getDisplayName"/>
            </xsl:with-param>
          </xsl:apply-templates>
          <xsl:text> </xsl:text>
        </div>
        <!-- Terminus class fix to floating columns -->
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- SKU Brief - work in progress [CR 2011-05-27] -->
  <xsl:template match="Content[@type='SKU']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <xsl:param name="pos"/>
    <xsl:variable name="parId">
      <xsl:choose>
        <xsl:when test="@parId &gt; 0">
          <xsl:value-of select="@parId"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="/Page/@id"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="self::Content" mode="getHref">
        <xsl:with-param name="parId" select="$parId"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="listItem sku">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem hproduct'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <h3 class="fn title">
          <xsl:variable name="title">
            <xsl:apply-templates select="." mode="getDisplayName"/>
          </xsl:variable>
          <a href="{$parentURL}" title="{$title}">
            <xsl:value-of select="$title"/>
          </a>
        </h3>
        <xsl:if test="Images/img/@src!=''">
          <a href="{$parentURL}" class="url">
            <xsl:apply-templates select="." mode="displayThumbnail"/>
          </a>
        </xsl:if>
        <xsl:if test="Manufacturer/node()!=''">
          <p class="manufacturer">
            <xsl:if test="/Page/Contents/Content[@name='makeLabel']">
              <span class="label">
                <xsl:value-of select="/Page/Contents/Content[@name='makeLabel']"/>
              </span>&#160;
            </xsl:if>
            <span class="brand">
              <xsl:value-of select="Manufacturer/node()"/>
            </span>
          </p>
        </xsl:if>
        <xsl:if test="StockCode/node()!=''">
          <p class="sku stockCode">
            <span class="label">
              <xsl:call-template name="term2014" />
              <xsl:text>: </xsl:text>
            </span>
            <xsl:value-of select="StockCode/node()"/>
          </p>
        </xsl:if>
        <xsl:choose>
          <xsl:when test="Content[@type='SKU']">
            <xsl:apply-templates select="Content[@type='SKU'][1]" mode="displayPrice" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="." mode="displayPrice" />
          </xsl:otherwise>
        </xsl:choose>
        <xsl:if test="ShortDescription/node()!=''">
          <div class="description">
            <xsl:apply-templates select="ShortDescription/node()" mode="cleanXhtml"/>
          </div>
        </xsl:if>
        <!-- Terminus class fix to floating columns -->
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='Product']" mode="opengraph-namespace">
    <xsl:text>og: http://ogp.me/ns# fb: http://ogp.me/ns/fb# product: http://ogp.me/ns/product#</xsl:text>
  </xsl:template>

  <xsl:template match="Content[@type='Product']" mode="opengraphdata">
    <meta property="og:type" content="product" />
    <meta property="product:upc" content="{StockCode/node()}" />
    <meta property="product:sale_price:currency" content="{$currencyCode}" />
    <xsl:variable name="price">
      <xsl:value-of select="Prices/Price[@currency=$currency and @type='sale']/node()"/>
    </xsl:variable>
    <meta property="product:sale_price:amount" content="{$price}" />
  </xsl:template>

  <!-- Product Detail -->
  <xsl:template match="Content[@type='Product']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <xsl:variable name="parId" select="@parId" />
    <div class="hproduct product detail">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'hproduct product detail'"/>
      </xsl:apply-templates>
      <xsl:choose>
        <!-- Test whether product has SKU's -->
        <xsl:when test="Content[@type='SKU']">
          <xsl:choose>
            <!--Test whether there're any detailed SKU images-->
            <xsl:when test="count(Content[@type='SKU']/Images/img[@class='detail' and @src != '']) &gt; 0">
              <xsl:for-each select="Content[@type='SKU']">
                <xsl:apply-templates select="." mode="displayDetailImage">
                  <!-- hide all but the first image -->
                  <xsl:with-param name="showImage">
                    <xsl:if test="position() != 1">
                      <xsl:text>noshow</xsl:text>
                    </xsl:if>
                  </xsl:with-param>
                </xsl:apply-templates>
              </xsl:for-each>
            </xsl:when>
            <xsl:otherwise>
              <!-- If no SKU's have detailed images show default product image -->
              <xsl:apply-templates select="." mode="displayDetailImage"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <!-- Display Default Image -->
          <xsl:apply-templates select="." mode="displayDetailImage"/>
        </xsl:otherwise>
      </xsl:choose>
      <h2 class="fn content-title">
        <xsl:value-of select="Name/node()"/>
      </h2>
      <xsl:if test="StockCode/node()!=''">
        <p class="stockCode">
          <span class="label">
            <xsl:call-template name="term2014" />
          </span>
          <xsl:text>&#160;</xsl:text>
          <xsl:value-of select="StockCode/node()"/>
        </p>
      </xsl:if>
      <xsl:if test="Manufacturer/node()!=''">
        <p class="manufacturer">
          <xsl:if test="/Page/Contents/Content[@name='makeLabel']">
            <span class="label">
              <xsl:value-of select="/Page/Contents/Content[@name='makeLabel']"/>&#160;
            </span>
          </xsl:if>
          <span class="brand">
            <xsl:value-of select="Manufacturer/node()"/>
          </span>
        </p>
      </xsl:if>
      <xsl:choose>
        <xsl:when test="Content[@type='SKU']">
          <xsl:apply-templates select="Content[@type='SKU'][1]" mode="displayPrice" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="." mode="displayPrice" />
        </xsl:otherwise>
      </xsl:choose>
      <xsl:if test="/Page/Cart">
        <xsl:apply-templates select="." mode="addToCartButton"/>
      </xsl:if>
      <xsl:apply-templates select="." mode="SpecLink"/>
      <xsl:if test="Body/node()!=''">
        <div class="description">
          <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>
      <div class="terminus">&#160;</div>
      <div class="entryFooter">
        <xsl:if test="Content[@type='Tag']">
          <div class="tags">
            <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
            <xsl:text> </xsl:text>
          </div>
        </xsl:if>
        <xsl:apply-templates select="." mode="backLink">
          <xsl:with-param name="link" select="$thisURL"/>
          <xsl:with-param name="altText">
            <xsl:call-template name="term2047" />
          </xsl:with-param>
        </xsl:apply-templates>
        <xsl:text> </xsl:text>
      </div>
      <div class="terminus">&#160;</div>
      <xsl:if test="Content[@type='LibraryImage']">
        <h2>
          <xsl:call-template name="term2073" />
        </h2>
        <div id="productScroller">
          <table id="productScrollerInner">
            <tr>
              <xsl:apply-templates select="Content[@type='LibraryImage']" mode="scollerImage"/>
            </tr>
          </table>
        </div>
        <div class="entryFooter">
          <xsl:apply-templates select="." mode="backLink">
            <xsl:with-param name="link" select="/Page/Request/ServerVariables/Item[@name='PREVIOUS_PAGE']/node()"/>
            <xsl:with-param name="altText">
              <xsl:call-template name="term2015" />
            </xsl:with-param>
          </xsl:apply-templates>
          <xsl:text> </xsl:text>
        </div>
      </xsl:if>

      <!--RELATED CONTENT-->
      <xsl:if test="Content">
        <!-- Reviews  -->
        <xsl:if test="Content[@type='Review']">
          <xsl:apply-templates select="." mode="relatedReviews"/>
        </xsl:if>
        <!-- Products  -->
        <xsl:if test="Content[@type='Product']">
          <div class="relatedcontent">
            <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
              <xsl:with-param name="type">PlainText</xsl:with-param>
              <xsl:with-param name="text">Add Title for Related Products</xsl:with-param>
              <xsl:with-param name="name">relatedProductsTitle</xsl:with-param>
            </xsl:apply-templates>
            <h4>
              <xsl:choose>
                <xsl:when test="/Page/Contents/Content[@name='relatedProductsTitle']">
                  <xsl:apply-templates select="/Page/Contents/Content[@name='relatedProductsTitle']" mode="displayBrief"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>Related Products</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
            </h4>

            <div class="list-group">
              <xsl:apply-templates select="/" mode="List_Related_Products">
                <xsl:with-param name="parProductID" select="@id"/>
              </xsl:apply-templates>
            </div>
          </div>
        </xsl:if>
      </xsl:if>
    </div>
    <div class="terminus">&#160;</div>
  </xsl:template>

  <xsl:template match="Content" mode="scollerImage">
    <xsl:param name="showImage"/>
    <xsl:variable name="imgId">
      <xsl:text>picture_</xsl:text>
      <xsl:value-of select="@id"/>
    </xsl:variable>
    <!-- Needed to create unique grouping for lightbox -->
    <xsl:variable name="parId">
      <xsl:text>group</xsl:text>
      <xsl:value-of select="@type"/>
    </xsl:variable>
    <xsl:variable name="src">
      <xsl:choose>
        <!-- IF use display -->
        <xsl:when test="Images/img[@class='display']/@src and Images/img[@class='display']/@src!=''">
          <xsl:value-of select="Images/img[@class='display']/@src"/>
        </xsl:when>
        <!-- Else Full Size use that -->
        <xsl:when test="Images/img[@class='detail']/@src and Images/img[@class='detail']/@src!=''">
          <xsl:value-of select="Images/img[@class='detail']/@src"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <!-- ALT VALUE -->
    <xsl:variable name="alt">
      <xsl:choose>
        <!-- IF Full Size use that -->
        <xsl:when test="Images/img[@class='detail']/@alt and Images/img[@class='detail']/@alt!=''">
          <xsl:value-of select="Images/img[@class='detail']/@alt"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@name"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="newSrc">
      <xsl:call-template name="resize-image">
        <xsl:with-param name="path" select="$src"/>
        <xsl:with-param name="max-width" select="500"/>
        <xsl:with-param name="max-height" select="150"/>
        <xsl:with-param name="file-prefix">
          <xsl:text>~dis-</xsl:text>
          <xsl:value-of select="500"/>
          <xsl:text>x</xsl:text>
          <xsl:value-of select="150"/>
          <xsl:text>/~dis-</xsl:text>
        </xsl:with-param>
        <xsl:with-param name="file-suffix" select="''"/>
        <xsl:with-param name="quality" select="100"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="largeSrc">
      <xsl:call-template name="resize-image">
        <xsl:with-param name="path" select="$src"/>
        <xsl:with-param name="max-width" select="500"/>
        <xsl:with-param name="max-height" select="500"/>
        <xsl:with-param name="file-prefix">
          <xsl:text>~lg-</xsl:text>
          <xsl:value-of select="500"/>
          <xsl:text>x</xsl:text>
          <xsl:value-of select="500"/>
          <xsl:text>/~lg-</xsl:text>
        </xsl:with-param>
        <xsl:with-param name="file-suffix" select="''"/>
        <xsl:with-param name="quality" select="100"/>
      </xsl:call-template>
    </xsl:variable>
    <td>
      <a href="{$largeSrc}" class="responsive-lightbox">
        <xsl:if test="$parId != ''">
          <xsl:attribute name="rel">
            <xsl:text>lightbox[</xsl:text>
            <xsl:value-of select="$parId"/>
            <xsl:text>]</xsl:text>
          </xsl:attribute>
        </xsl:if>
        <img src="{$newSrc}" width="{ew:ImageWidth($newSrc)}" height="{ew:ImageHeight($newSrc)}" alt="{$alt}" class="detail">
          <xsl:if test="$imgId != ''">
            <xsl:attribute name="id">
              <xsl:value-of select="$imgId"/>
            </xsl:attribute>
          </xsl:if>
        </img>
      </a>
    </td>
  </xsl:template>

  <!-- List Related Products-->
  <xsl:template match="/" mode="List_Related_Products">
    <xsl:param name="parProductID"/>
    <xsl:for-each select="/Page/ContentDetail/Content/Content">
      <xsl:sort select="@type" order="ascending"/>
      <xsl:sort select="@displayOrder" order="ascending"/>
      <xsl:choose>
        <xsl:when test="@type='Product'">
          <xsl:apply-templates select="." mode="displayBriefRelated"/>
        </xsl:when>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>

  <!-- Product Brief -->
  <xsl:template match="Content[@type='Product']" mode="displayBriefRelated">
    <xsl:param name="sortBy"/>
    <xsl:param name="pos"/>
    <xsl:variable name="parId">
      <xsl:choose>
        <xsl:when test="@parId &gt; 0">
          <xsl:value-of select="@parId"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="/Page/@id"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="self::Content" mode="getHref">
        <xsl:with-param name="parId" select="$parId"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="hproduct">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'hproduct'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <xsl:if test="Images/img/@src!=''">
          <a href="{$parentURL}" class="url pull-left">
            <xsl:apply-templates select="." mode="displayThumbnail">
              <xsl:with-param name="width">125</xsl:with-param>
              <xsl:with-param name="height">125</xsl:with-param>
              <xsl:with-param name="forceResize">true</xsl:with-param>
            </xsl:apply-templates>
          </a>
        </xsl:if>
        <h5 class="fn title">
          <xsl:variable name="title">
            <xsl:apply-templates select="." mode="getDisplayName"/>
          </xsl:variable>
          <a href="{$parentURL}" title="{$title}">
            <xsl:value-of select="$title"/>
          </a>
        </h5>
        <xsl:if test="Manufacturer/node()!=''">
          <p class="manufacturer">
            <xsl:if test="/Page/Contents/Content[@name='makeLabel']">
              <span class="label">
                <xsl:value-of select="/Page/Contents/Content[@name='makeLabel']"/>
              </span>&#160;
            </xsl:if>
            <span class="brand">
              <xsl:value-of select="Manufacturer/node()"/>
            </span>
          </p>
        </xsl:if>
        <xsl:if test="StockCode/node()!=''">
          <p class="sku stockCode">
            <span class="label">
              <xsl:call-template name="term2014" />
              <xsl:text>: </xsl:text>
            </span>
            <xsl:value-of select="StockCode/node()"/>
          </p>
        </xsl:if>
        <xsl:choose>
          <xsl:when test="Content[@type='SKU']">
            <xsl:apply-templates select="Content[@type='SKU'][1]" mode="displayPrice" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="." mode="displayPrice" />
          </xsl:otherwise>
        </xsl:choose>
        <xsl:if test="ShortDescription/node()!=''">
          <div class="description">
            <xsl:apply-templates select="ShortDescription/node()" mode="cleanXhtml"/>
          </div>
        </xsl:if>
        <div class="entryFooter text-right">
          <xsl:if test="/Page/Cart">
            <xsl:apply-templates select="." mode="addToCartButton">
              <xsl:with-param name="actionURL" select="$parentURL"/>
            </xsl:apply-templates>
          </xsl:if>
          <xsl:text> </xsl:text>
        </div>
        <!-- Terminus class fix to floating columns -->
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>


  <!-- Product Gallery Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='ProductGallery']" mode="displayBrief">
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div class="ProductGallery Grid">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix ProductGallery Grid content-scroller</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <div class="cols cols{@cols}" data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1" >
        <xsl:if test="@autoplay !=''">
          <xsl:attribute name="data-autoplay">
            <xsl:value-of select="@autoplay"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="@autoPlaySpeed !=''">
          <xsl:attribute name="data-autoPlaySpeed">
            <xsl:value-of select="@autoPlaySpeed"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="articleList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefGallery">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
        <xsl:text> </xsl:text>
      </div>
    </div>
  </xsl:template>

  <!-- Product Gallery Brief -->
  <xsl:template match="Content[@type='Product']" mode="displayBriefGallery">
    <xsl:param name="sortBy"/>
    <xsl:param name="pos"/>
    <xsl:variable name="parId">
      <xsl:choose>
        <xsl:when test="@parId &gt; 0">
          <xsl:value-of select="@parId"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="/Page/@id"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="self::Content" mode="getHref">
        <xsl:with-param name="parId" select="$parId"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="grid-item">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'grid-item hproduct'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <a href="{$parentURL}" title="{@name}" class="url">
        <div class="thumbnail">
          <img src="{Images/img[@class='detail']/@src}" class="img-responsive" style="overflow:hidden;"/>
          <div class="caption">
            <h4>
              <xsl:value-of select="Name/node()"/>
            </h4>
            <xsl:apply-templates select="Body/node()" mode="cleanXhtml" />
            <xsl:choose>
              <xsl:when test="Content[@type='SKU']">
                <xsl:apply-templates select="Content[@type='SKU'][1]" mode="displayPrice" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:apply-templates select="." mode="displayPrice" />
              </xsl:otherwise>
            </xsl:choose>
          </div>
        </div>
      </a>
    </div>
  </xsl:template>

  <!-- Specification Link -->
  <xsl:template match="Content" mode="SpecLink">
    <xsl:if test="SpecificationDocument/node()!=''">
      <p class="doclink">
        <a class="{substring-after(SpecificationDocument/node(),'.')}icon" href="{SpecificationDocument/node()}" title="Click here to download a copy of this document">Download Product Specification</a>
      </p>
    </xsl:if>
  </xsl:template>


  
  <!-- ## Documents ###################################################################################   -->
  <!-- Document Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='DocumentList']" mode="displayBrief">
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="idsList">
      <xsl:apply-templates select="ms:node-set($contentList)/*" mode="idList" />
    </xsl:variable>
    <!--responsive columns variables-->
    <xsl:variable name="xsColsToShow">
      <xsl:choose>
        <xsl:when test="@xsCol='2'">2</xsl:when>
        <xsl:otherwise>1</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="smColsToShow">
      <xsl:choose>
        <xsl:when test="@smCol and @smCol!=''">
          <xsl:value-of select="@smCol"/>
        </xsl:when>
        <xsl:otherwise>2</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="mdColsToShow">
      <xsl:choose>
        <xsl:when test="@mdCol and @mdCol!=''">
          <xsl:value-of select="@mdCol"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@cols"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!--end responsive columns variables-->
    <div class="Documents">
      <div class="cols{@cols}">
        <!--responsive columns-->
        <xsl:attribute name="class">
          <xsl:text>cols</xsl:text>
          <xsl:choose>
            <xsl:when test="@xsCol='2'"> mobile-2-col-content</xsl:when>
            <xsl:otherwise> mobile-1-col-content</xsl:otherwise>
          </xsl:choose>
          <xsl:if test="@smCol and @smCol!=''">
            <xsl:text> sm-content-</xsl:text>
            <xsl:value-of select="@smCol"/>
          </xsl:if>
          <xsl:if test="@mdCol and @mdCol!=''">
            <xsl:text> md-content-</xsl:text>
            <xsl:value-of select="@mdCol"/>
          </xsl:if>
          <xsl:text> cols</xsl:text>
          <xsl:value-of select="@cols"/>
          <xsl:if test="@mdCol and @mdCol!=''">
            <xsl:text> content-cols-responsive</xsl:text>
          </xsl:if>
        </xsl:attribute>
        <!--end responsive columns-->
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="documentList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief" >
          <xsl:with-param name="sortBy" select="@sortBy"/>
          <xsl:with-param name="showThumbnail" select="@showThumbnails"/>
        </xsl:apply-templates>
        <div class="terminus">&#160;</div>
      </div>
      <xsl:if test="@allAsZip='on'">
        <div class="listItem list-group-item">
          <div class="lIinner">
            <a class="docLink zipicon" href="{$appPath}ewcommon/tools/download.ashx?docId={$idsList}&amp;filename=myzip.zip&amp;xPath=/Content/Path">
              <xsl:call-template name="term2074" />
            </a>
          </div>
        </div>
      </xsl:if>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='Document']" mode="idList">
    <xsl:if test="position()!=1">
      <xsl:text>,</xsl:text>
    </xsl:if>
    <xsl:value-of select="@id"/>
  </xsl:template>

  <!-- Document Brief -->
  <xsl:template match="Content[@type='Document']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <xsl:param name="showThumbnail"/>
    <!-- documentBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="self::Content" mode="getHref">
        <xsl:with-param name="parId" select="@parId"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="preURL" select="substring(Website,1,3)" />
    <xsl:variable name="linkURL">
      <xsl:choose>
        <xsl:when test="$preURL='www' or $preURL='WWW'">
          <xsl:text>http://</xsl:text>
          <xsl:value-of select="Url"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="Url"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div class="list-group-item listItem">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'list-group-item listItem'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <xsl:if test="$showThumbnail='true'">
          <xsl:apply-templates select="." mode="displayThumbnail"/>
        </xsl:if>
        <h3 class="title">
          <a rel="external">
            <xsl:if test="$GoogleAnalyticsUniversalID!=''">
              <xsl:attribute name="onclick">
                <xsl:text>ga('send', 'event', 'Document', 'download', 'document-</xsl:text>
                <xsl:value-of select="Title/node()"/>
                <xsl:text>');</xsl:text>
              </xsl:attribute>
            </xsl:if>
            <xsl:attribute name="href">
              <xsl:choose>
                <xsl:when test="contains(Path,'http://')">
                  <xsl:value-of select="Path/node()"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$appPath"/>
                  <xsl:text>ewcommon/tools/download.ashx?docId=</xsl:text>
                  <xsl:value-of select="@id"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
            <xsl:attribute name="title">
              <!-- click here to download a copy of this document -->
              <xsl:call-template name="term2027" />
            </xsl:attribute>
            <xsl:value-of select="Title/node()"/>
          </a>
        </h3>
        <xsl:if test="Body/node()!=''">
          <div class="description">
            <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
          </div>
        </xsl:if>
        <p class="link">
          <a rel="external" class="docLink {substring-after(Path,'.')}icon">
            <xsl:attribute name="href">
              <xsl:choose>
                <xsl:when test="contains(Path,'http://')">
                  <xsl:value-of select="Path/node()"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$appPath"/>
                  <xsl:text>ewcommon/tools/download.ashx?docId=</xsl:text>
                  <xsl:value-of select="@id"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
            <xsl:if test="$GoogleAnalyticsUniversalID!=''">
              <xsl:attribute name="onclick">
                <xsl:text>ga('send', 'event', 'Document', 'download', 'document-</xsl:text>
                <xsl:value-of select="Title/node()"/>
                <xsl:text>');</xsl:text>
              </xsl:attribute>
            </xsl:if>
            <xsl:choose>
              <xsl:when test="contains(Path,'http://')">
                <xsl:attribute name="title">
                  <!-- click here to view this document -->
                  <xsl:call-template name="term2027a" />
                </xsl:attribute>
                <xsl:value-of select="Path/node()"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:attribute name="title">
                  <!-- click here to download a copy of this document -->
                  <xsl:call-template name="term2027" />
                </xsl:attribute>
                <!--Download-->
                <xsl:call-template name="term2028" />
                <xsl:text>&#160;</xsl:text>
                <xsl:call-template name="getFileTypeName">
                  <xsl:with-param name="extension" select="concat('.',substring-after(Path,'.'))"/>
                </xsl:call-template>
              </xsl:otherwise>
            </xsl:choose>
          </a>
        </p>
        <!-- Terminus class fix to floating columns -->
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!--   ################   Tags   ###############   -->

  <!-- Tags Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='TagCloud']" mode="displayBrief">
    <xsl:variable name="articleList">
      <xsl:choose>
        <xsl:when test="@display='alpha'">
          <xsl:for-each select="/Page/Contents/descendant-or-self::Content[@type='Tag' and not(Name = preceding:: Name)]">
            <xsl:sort select="Name" data-type="text" order="descending"/>
            <xsl:copy-of select="."/>
          </xsl:for-each>
        </xsl:when>
        <xsl:otherwise>
          <xsl:for-each select="/Page/Contents/descendant-or-self::Content[@type='Tag' and not(Name/node() = preceding:: Name/node())]">
            <xsl:copy-of select="."/>
          </xsl:for-each>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div class="tagCloud">
      <xsl:apply-templates select="ms:node-set($articleList)" mode="displayBrief">
        <xsl:with-param name="sortBy" select="@sortBy"/>
      </xsl:apply-templates>
    </div>
  </xsl:template>

  <!-- Tags Display -->
  <xsl:template match="Content" mode="displayTags">
    <xsl:param name="sortBy"/>
    <xsl:variable name="articleList">
      <xsl:for-each select="Content[@type='Tag']">
        <xsl:copy-of select="."/>
      </xsl:for-each>
    </xsl:variable>
    <xsl:if test="count(Content[@type='Tag'])&gt;0">
      <div class="tags">
        <!--Tags-->
        <xsl:call-template name="term2039" />
        <xsl:text>: </xsl:text>
        <xsl:apply-templates select="ms:node-set($articleList)" mode="displayBrief">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
      </div>
    </xsl:if>
  </xsl:template>

  <!-- Tags Brief -->
  <xsl:template match="Content[@type='Tag']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <xsl:variable name="name" select="Name/node()"/>
    <span>
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <a href="{$parentURL}" rel="tag">
        <xsl:apply-templates select="Name" mode="displayBrief"/>
        <xsl:if test="@relatedCount!=''">
          &#160;(<xsl:value-of select="@relatedCount"/>)
        </xsl:if>
      </a>
      <xsl:if test="position()!=last()">
        <span class="tag-comma">
          <xsl:text>, </xsl:text>
        </span>
      </xsl:if>
    </span>
  </xsl:template>

  <!-- Tags Detail -->
  <xsl:template match="Content[@type='Tag']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <div class="detail tag">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail tag'"/>
      </xsl:apply-templates>
      <h1>
        <xsl:value-of select="Name/node()"/>
      </h1>
      <div class="tags cols cols3">
        <xsl:apply-templates select="Content" mode="displayBrief">
          <xsl:sort select="@publish" order="descending"/>
        </xsl:apply-templates>
        <xsl:text> </xsl:text>
      </div>
      <!-- Terminus class fix to floating content -->
      <div class="terminus">&#160;</div>
      <div class="entryFooter">
        <div class="tags">
          <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
        </div>
        <xsl:apply-templates select="." mode="backLink">
          <xsl:with-param name="link" select="$thisURL"/>
          <xsl:with-param name="altText">
            <!--click here to return to the tags list-->
            <xsl:call-template name="term2040" />
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div class="terminus">&#160;</div>
    </div>
  </xsl:template>

  <!-- Tags Display -->
  <xsl:template match="Content" mode="displayTagsNoLink">
    <xsl:param name="sortBy"/>
    <xsl:variable name="articleList">
      <xsl:for-each select="Content[@type='Tag']">
        <xsl:copy-of select="."/>
      </xsl:for-each>
    </xsl:variable>
    <xsl:if test="count(Content[@type='Tag'])&gt;0">
      <div class="tags">
        <!--Tags-->
        <xsl:apply-templates select="ms:node-set($articleList)" mode="displayBriefNoLink">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
      </div>
    </xsl:if>
  </xsl:template>

  <!-- Tags Brief -->
  <xsl:template match="Content[@type='Tag']" mode="displayBriefNoLink">
    <xsl:param name="sortBy"/>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <xsl:variable name="name" select="Name/node()"/>
    <span>
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <xsl:apply-templates select="Name" mode="displayBrief"/>
      <xsl:if test="@relatedCount!=''">
        &#160;(<xsl:value-of select="@relatedCount"/>)
      </xsl:if>
      <xsl:if test="position()!=last()">
        <span class="tag-comma">
          <xsl:text>, </xsl:text>
        </span>
      </xsl:if>
    </span>
  </xsl:template>

<!--Luke-->
  <!-- Links Brief -->
  <xsl:template match="Content[@type='Link']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <xsl:param name="crop"/>
    <xsl:variable name="preURL" select="substring(Url,1,3)" />
    <xsl:variable name="url" select="Url/node()" />
    <xsl:variable name="linkURL">
      <xsl:choose>
        <xsl:when test="format-number($url,'0')!='NaN'">
          <xsl:apply-templates select="$page/descendant-or-self::MenuItem[@id=$url]" mode="getHref"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:if test="$preURL='www' or $preURL='WWW'">
            <xsl:text>http://</xsl:text>
          </xsl:if>
          <xsl:value-of select="$url"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="cropSetting">
      <xsl:choose>
        <xsl:when test="$crop='true'">
          <xsl:value-of select="true()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="false()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <div class="list-group-item listItem link">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'list-group-item listItem link'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="list-item lIinner">
        <h3 class="title">
          <a href="{$linkURL}" title="{Name}">
            <xsl:if test="not(substring(@linkURL,1,1)='/') and (contains(@linkURL,'http://') and Url/@type='external')">
              <xsl:attribute name="rel">external</xsl:attribute>
              <xsl:attribute name="class">extLink</xsl:attribute>
            </xsl:if>
            <xsl:value-of select="Name"/>
          </a>
        </h3>
        <xsl:if test="Images/img/@src!=''">
          <a href="{$linkURL}" title="Click here to link to {Name}">
            <xsl:if test="not(substring(@linkURL,1,1)='/') and (contains(@linkURL,'http://') and Url/@type='external')">
              <xsl:attribute name="rel">external</xsl:attribute>
            </xsl:if>
            <xsl:apply-templates select="." mode="displayThumbnail">
              <xsl:with-param name="crop" select="$cropSetting" />
            </xsl:apply-templates>
          </a>
        </xsl:if>
        <xsl:if test="Body/node()!=''">
          <div class="description">
            <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
          </div>
        </xsl:if>
        <div class="entryFooter">
          <xsl:apply-templates select="." mode="displayTags"/>
          <xsl:apply-templates select="." mode="moreLink">
            <xsl:with-param name="link" select="$linkURL"/>
            <xsl:with-param name="linkType" select="Url/@type"/>
            <xsl:with-param name="altText">
              <xsl:value-of select="Name/node()"/>
            </xsl:with-param>
          </xsl:apply-templates>
          <xsl:text> </xsl:text>
        </div>
      </div>
    </div>
  </xsl:template>



 

 

  <!--  ##########################################################################################   -->
  <!--  ## Site Map Templates  ###################################################################   -->
  <!--  ##########################################################################################   -->
  <!-- Site Map Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='SiteMapList']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- Output Module -->
    <div class="SiteMapList">
      <ul class="sitemap">
        <xsl:apply-templates select="/Page/Menu/MenuItem" mode="sitemap">
          <xsl:with-param name="level">1</xsl:with-param>
          <xsl:with-param name="bDescription">
            <xsl:value-of select="@displayDescription"/>
          </xsl:with-param>
          <xsl:with-param name="showHiddenPages">
            <xsl:value-of select="@showHiddenPages"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </ul>
      <div class="terminus">&#160;</div>
    </div>
  </xsl:template>

  <!-- Site Map Menu Display -->
  <xsl:template match="MenuItem" mode="sitemap">
    <xsl:param name="level"/>
    <xsl:param name="bDescription"/>
    <xsl:param name="showHiddenPages"/>
    <li>
      <xsl:apply-templates select="." mode="menuLink"/>
      <xsl:if test="$bDescription='true' and Description/node()">
        <p>
          <xsl:apply-templates select="Description/node()" mode="flattenXhtml" />
        </p>
      </xsl:if>
    </li>
    <xsl:if test="MenuItem">
      <xsl:apply-templates select="MenuItem" mode="sitemapSubLevel">
        <xsl:with-param name="level">
          <xsl:value-of select="$level"/>
        </xsl:with-param>
        <xsl:with-param name="bDescription" select="$bDescription" />
        <xsl:with-param name="showHiddenPages" select="$showHiddenPages" />
      </xsl:apply-templates>
    </xsl:if>
  </xsl:template>

  <!-- Site Map Sub Menu Display -->
  <xsl:template match="MenuItem" mode="sitemapSubLevel">
    <xsl:param name="level"/>
    <xsl:param name="bDescription" />
    <xsl:param name="showHiddenPages" />
    <xsl:variable name="url">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <xsl:variable name="displayName">
      <xsl:apply-templates select="." mode="getDisplayName"/>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$showHiddenPages='true'">
        <li>
          <xsl:apply-templates select="." mode="menuLink"/>
          <xsl:if test="$bDescription='true' and Description/node()">
            <p>
              <xsl:apply-templates select="Description/node()" mode="flattenXhtml" />
            </p>
          </xsl:if>
          <xsl:if test="MenuItem">
            <ul>
              <xsl:apply-templates select="MenuItem" mode="sitemapSubLevel">
                <xsl:with-param name="level">
                  <xsl:value-of select="$level+1"/>
                </xsl:with-param>
                <xsl:with-param name="bDescription" select="$bDescription" />
                <xsl:with-param name="showHiddenPages" select="$showHiddenPages" />
              </xsl:apply-templates>
            </ul>
          </xsl:if>
        </li>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <!-- when this page excluded from Nav, its children may not be. -->
          <xsl:when test="DisplayName/@exclude='true'">
            <xsl:apply-templates select="MenuItem" mode="sitemapSubLevel">
              <xsl:with-param name="level" select="$level"/>
              <xsl:with-param name="bDescription" select="$bDescription" />
            </xsl:apply-templates>
          </xsl:when>
          <!-- otherwise normal behaviour -->
          <xsl:otherwise>
            <li>
              <xsl:apply-templates select="." mode="menuLink"/>
              <xsl:if test="$bDescription='true' and Description/node()">
                <p>
                  <xsl:apply-templates select="Description/node()" mode="flattenXhtml" />
                </p>
              </xsl:if>
              <xsl:if test="MenuItem">
                <ul>
                  <xsl:apply-templates select="MenuItem" mode="sitemapSubLevel">
                    <xsl:with-param name="level">
                      <xsl:value-of select="$level+1"/>
                    </xsl:with-param>
                    <xsl:with-param name="bDescription" select="$bDescription" />
                  </xsl:apply-templates>
                </ul>
              </xsl:if>
            </li>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!--  ==  Sitemap Large  ==========================================================================  -->

  <xsl:template match="Content[@moduleType='SiteMapLarge']" mode="displayBrief">
    <div class="Sitemap">
      <xsl:apply-templates select="/Page/Menu" mode="sitemapList">
        <xsl:with-param name="cols" select="@cols"/>
        <xsl:with-param name="descriptions" select="@descriptions = 'true'"/>
      </xsl:apply-templates>
    </div>
  </xsl:template>

  <!-- List -->
  <xsl:template match="*" mode="sitemapList"/>

  <xsl:template match="Menu | MenuItem[not(DisplayName/@exclude = 'true')]" mode="sitemapList">
    <xsl:param name="level" select="1"/>
    <xsl:param name="cols" select="1"/>
    <xsl:param name="descriptions" select="false()"/>
    <xsl:if test="MenuItem[not(DisplayName/@exclude = 'true')]">
      <div>
        <xsl:attribute name="class">
          <xsl:text>list listLevel</xsl:text>
          <xsl:value-of select="$level"/>
          <xsl:if test="$level = 2">
            <xsl:text> cols</xsl:text>
            <xsl:value-of select="$cols"/>
          </xsl:if>
        </xsl:attribute>
        <xsl:apply-templates select="MenuItem" mode="sitemapItem">
          <xsl:with-param name="level" select="$level"/>
          <xsl:with-param name="cols" select="$cols"/>
          <xsl:with-param name="descriptions" select="$descriptions"/>
        </xsl:apply-templates>
        <div class="terminus">&#160;</div>
      </div>
    </xsl:if>
  </xsl:template>

  <!-- Item -->
  <xsl:template match="*" mode="sitemapItem"/>

  <xsl:template match="MenuItem[not(DisplayName/@exclude = 'true')]" mode="sitemapItem">
    <xsl:param name="level" select="1"/>
    <xsl:param name="cols" select="1"/>
    <xsl:param name="descriptions" select="false()"/>
    <xsl:variable name="home" select="@id = /Page/Menu/MenuItem/@id"/>
    <xsl:variable name="levelClass" select="concat('Level', $level)"/>
    <div>
      <xsl:attribute name="class">
        <xsl:text>item item</xsl:text>
        <xsl:value-of select="$levelClass"/>
        <xsl:if test="$level = 2">
          <xsl:text> listItem</xsl:text>
        </xsl:if>
      </xsl:attribute>
      <div>
        <xsl:if test="$level = 2">
          <xsl:attribute name="class">
            <xsl:text>lIinner</xsl:text>
          </xsl:attribute>
        </xsl:if>
        <div class="heading heading{$levelClass}">
          <a class="link link{$levelClass}">
            <xsl:attribute name="href">
              <xsl:apply-templates select="." mode="getHref"/>
            </xsl:attribute>
            <xsl:apply-templates select="." mode="getDisplayName"/>
          </a>
          <xsl:if test="$descriptions and Description != ''">
            <div class="description description{$levelClass}">
              <xsl:copy-of select="Description/*"/>
            </div>
          </xsl:if>
        </div>
        <xsl:if test="not($home)">
          <xsl:apply-templates select="." mode="sitemapList">
            <xsl:with-param name="level" select="$level + 1"/>
            <xsl:with-param name="cols" select="$cols"/>
            <xsl:with-param name="descriptions" select="$descriptions"/>
          </xsl:apply-templates>
        </xsl:if>
        <div class="terminus">&#160;</div>
      </div>
    </div>
    <xsl:if test="$home">
      <xsl:apply-templates select="MenuItem" mode="sitemapItem">
        <xsl:with-param name="level" select="$level"/>
        <xsl:with-param name="cols" select="$cols"/>
        <xsl:with-param name="descriptions" select="$descriptions"/>
      </xsl:apply-templates>
    </xsl:if>
  </xsl:template>


  <!--   ################   Poll   ###############   -->
  <!-- Poll Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='PollList']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- Output Module -->
    <div class="PollList">
      <div class="cols{@cols}">
        <!-- If Stepper, display Stepper -->
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="articleList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
          <xsl:with-param name="sortBy" select="@sort"/>
        </xsl:apply-templates>
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- Display Poll -->
  <xsl:template match="Content[@type='Poll']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <div class="clearfix list list-group-item listItem poll">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'clearfix list list-group-item listItem poll'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="list-item lIinner">
        <h3 class="title">
          <xsl:value-of select="Title/node()"/>
        </h3>
        <xsl:if test="Description/node()!=''">
          <h4>
            <xsl:value-of select="Description/node()"/>
          </h4>
        </xsl:if>
        <xsl:if test="Images/img/@src and Images/img/@src!=''">
          <xsl:apply-templates select="Images/img" mode="cleanXhtml"/>
        </xsl:if>
        <xsl:apply-templates select="." mode="pollForm"/>
      </div>
    </div>
  </xsl:template>

  <!-- !!!!!!  Not Voted Yet !!!!!! -->
  <xsl:template match="Content[Status/@canVote='true']" mode="pollForm">
    <!-- GET VOTED VALUE - EITHER FROM FORM REQUEST VALUES, OR RESULTS VALUE -->
    <xsl:variable name="votedFor">
      <xsl:variable name="votedName">
        <xsl:text>polloption-</xsl:text>
        <xsl:value-of select="@id"/>
      </xsl:variable>
      <xsl:choose>
        <xsl:when test="/Page/Request/Form/Item[@name=$votedName]">
          <xsl:value-of select="/Page/Request/Form/Item[@name=$votedName]/node()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="Results/@votedFor"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- GET SUBMITTED EMAIL ADDRESS -->
    <xsl:variable name="submittedEmail">
      <xsl:value-of select="/Page/Request/Form/Item[@name='poll-email']/node()"/>
    </xsl:variable>
    <!-- SUM NUMBER OF VOTES -->
    <xsl:variable name="total-votes">
      <xsl:choose>
        <!-- IF VOTES {Calculate Total} ELSE {0} -->
        <xsl:when test="Results/PollResult/@votes">
          <xsl:value-of select="sum(Results/PollResult/@votes)"/>
        </xsl:when>
        <xsl:otherwise>0</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <form id="pollform-{@id}" name="pollform-{@id}" action="" method="post" class="pollform">
      <xsl:apply-templates select="PollItems/Content" mode="pollOption">
        <xsl:with-param name="pollId" select="@id"/>
        <xsl:with-param name="votedFor" select="$votedFor"/>
      </xsl:apply-templates>
      <!-- IF EMAIL RESTRICTION INDENTIFIER -->
      <div class="pollsubmission">
        <xsl:if test="contains(Restrictions/Identifiers/node(),'email')">
          <xsl:if test="not(/Page/User) and $submittedEmail=''">
            <label for="poll-email-{@id}">
              <!--Email-->
              <xsl:call-template name="term2050" />
              <span class="req">*</span>
            </label>
          </xsl:if>
          <input name="poll-email" id="poll-email-{@id}">
            <xsl:attribute name="type">
              <xsl:choose>
                <xsl:when test="/Page/User or $submittedEmail!=''">hidden</xsl:when>
                <xsl:otherwise>text</xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
            <xsl:attribute name="value">
              <xsl:choose>
                <xsl:when test="/Page/User">
                  <xsl:value-of select="/Page/User/Email/node()"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$submittedEmail"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
            <xsl:attribute name="class">
              <xsl:text>textbox short email</xsl:text>
              <xsl:if test="not(/Page/User)">
                <xsl:text> required</xsl:text>
              </xsl:if>
            </xsl:attribute>
          </input>
        </xsl:if>
        <xsl:if test="Status/@validationError!=''">
          <span class="alert">
            <xsl:value-of select="Status/@validationError"/>
          </span>
        </xsl:if>
        <button type="submit" name="pollsubmit-{@id}" value="Submit Vote" class="btn btn-default principle"  onclick="disableButton(this);">Submit Vote</button>
      </div>
    </form>
    <!-- IN ADMIN SHOW RESULTS -->
    <xsl:if test="/Page/@adminMode and not(@id=/Page/Request/QueryString/Item[@name='showPollResults']/node())">
      <xsl:variable name="resultHref">
        <xsl:apply-templates select="//MenuItem[@id=/Page/@id]" mode="getHref"/>
        <xsl:text>&amp;showPollResults=</xsl:text>
        <xsl:value-of select="@id"/>
      </xsl:variable>
      <p>
        <a href="{$resultHref}" title="see results">
          <!--Show Results-->
          <xsl:call-template name="term2051" />
          <xsl:text> &#187;</xsl:text>
        </a>
      </p>
    </xsl:if>
  </xsl:template>

  <!-- Show Poll Results -->
  <xsl:template match="Content[Status/@canVote='true' and @id=/Page/Request/QueryString/Item[@name='showPollResults']/node()]" mode="pollForm">
    <!-- Total Number of Votes -->
    <xsl:variable name="total-votes">
      <xsl:choose>
        <!-- IF VOTES {Calculate Total} ELSE {0} -->
        <xsl:when test="Results/PollResult/@votes">
          <xsl:value-of select="sum(Results/PollResult/@votes)"/>
        </xsl:when>
        <xsl:otherwise>0</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:apply-templates select="." mode="pollResults">
      <xsl:with-param name="total-votes" select="$total-votes"/>
    </xsl:apply-templates>
  </xsl:template>

  <!-- Can No Longer Vote - Inc Just Voted -->
  <xsl:template match="Content[Status/@canVote='false']" mode="pollForm">
    <xsl:variable name="votedFor">
      <xsl:value-of select="Results/@votedFor"/>
    </xsl:variable>
    <xsl:variable name="total-votes">
      <xsl:value-of select="sum(Results/PollResult/@votes)"/>
    </xsl:variable>
    <xsl:variable name="votedName">
      <xsl:text>polloption-</xsl:text>
      <xsl:value-of select="@id"/>
    </xsl:variable>
    <div class="pollresults">
      <xsl:choose>
        <!-- IF Poll is not yet Open -->
        <xsl:when test="dOpenDate/node() and number(translate(dOpenDate,'-','')) &gt; number(translate($today,'-',''))">
          <span class="hint">
            <!--This poll opens for voting at the begining of-->
            <xsl:call-template name="term2052" />
            <xsl:text> </xsl:text>
            <xsl:call-template name="DisplayDate">
              <xsl:with-param name="date" select="dOpenDate/node()"/>
            </xsl:call-template>
          </span>
        </xsl:when>
        <!-- IF Private Voting -->
        <xsl:when test="Resulting/@public='false'">
          <!--The results to this poll are private.-->
          <xsl:call-template name="term2053" />
        </xsl:when>
        <!-- IF closed results AND close date is greater than today-->
        <xsl:when test="(Resulting/@display='closed') and not( format-number(number(translate(dCloseDate/node(),'-','')),'0') &lt; number(translate($today,'-','')) )">
          <span class="hint">
            <!--The results to this poll will be revealed-->
            <xsl:call-template name="term2054" />
            <xsl:choose>
              <xsl:when test="dCloseDate!=''">
                <xsl:text>&#160;</xsl:text>
                <!--at the end of-->
                <xsl:call-template name="term2055" />
                <xsl:text>&#160;</xsl:text>
                <xsl:call-template name="DisplayDate">
                  <xsl:with-param name="date" select="dCloseDate/node()"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:text>&#160;</xsl:text>
                <!--when the poll closes.-->
                <xsl:call-template name="term2056" />
              </xsl:otherwise>
            </xsl:choose>
          </span>
        </xsl:when>
        <!-- Show results -->
        <xsl:otherwise>
          <xsl:apply-templates select="." mode="pollResults">
            <xsl:with-param name="total-votes" select="$total-votes"/>
          </xsl:apply-templates>
        </xsl:otherwise>
      </xsl:choose>
    </div>
    <xsl:if test="Status/@blockReason!=''">
      <!--<xsl:if test="Status/@blockReason!='' and /Page/Request/Form/Item[@name=$votedName]">-->
      <xsl:apply-templates select="." mode="pollBlockReason"/>
    </xsl:if>
  </xsl:template>

  <!-- Poll Blocked Reason -->
  <xsl:template match="Content" mode="pollBlockReason">
    <span class="hint">
      <xsl:choose>
        <xsl:when test="Status/@blockReason='LogFound' or Status/@blockReason='CookieFound'">
          <!--You have already voted on this poll.-->
          <xsl:call-template name="term2057" />
        </xsl:when>
        <xsl:when test="Status/@blockReason='RegisteredUsersOnly'">
          <!--This poll is only available to registered users-->
          <xsl:call-template name="term2058" />
        </xsl:when>
        <xsl:when test="Status/@blockReason='JustVoted'">
          <xsl:choose>
            <xsl:when test="Resulting/VoteConfirmationMessage/node()">
              <xsl:value-of select="Resulting/VoteConfirmationMessage/node()"/>
            </xsl:when>
            <xsl:otherwise>
              <!--Thank You, your vote has been counted.-->
              <xsl:call-template name="term2059" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
      </xsl:choose>
    </span>
  </xsl:template>

  <!-- Poll Option -->
  <xsl:template match="Content" mode="pollOption">
    <xsl:param name="pollId"/>
    <xsl:param name="votedFor"/>
    <div class="polloption">
      <input type="radio" id="polloption-{@id}" name="polloption-{$pollId}" value="{@id}" class="radiocheckbox pollradiocheckbox">
        <xsl:if test="@id=$votedFor">
          <xsl:attribute name="checked">checked</xsl:attribute>
        </xsl:if>
      </input>
      <label for="polloption-{@id}">
        <xsl:value-of select="@name"/>
      </label>
    </div>
  </xsl:template>

  <!-- Poll Results -->
  <xsl:template match="Content" mode="pollResults">
    <xsl:param name="total-votes"/>
    <xsl:apply-templates select="PollItems/Content" mode="pollOptionResult">
      <xsl:with-param name="total-votes" select="$total-votes"/>
    </xsl:apply-templates>
    <p class="polltotalvotes">
      <!--Total Votes-->
      <xsl:call-template name="term2060" />
      <xsl:text>: </xsl:text>
      <xsl:value-of select="$total-votes"/>
    </p>
  </xsl:template>

  <!-- Poll Option Result -->
  <xsl:template match="Content[@type='PollOption']" mode="pollOptionResult">
    <xsl:param name="total-votes"/>
    <xsl:variable name="optionId" select="@id"/>
    <xsl:variable name="votes">
      <xsl:choose>
        <xsl:when test="ancestor::Content[@type='Poll']/Results/PollResult/@entryId=$optionId">
          <xsl:value-of select="ancestor::Content[@type='Poll']/Results/PollResult[@entryId=$optionId]/@votes"/>
        </xsl:when>
        <xsl:otherwise>0</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="percentageVote">
      <xsl:choose>
        <xsl:when test="$total-votes!=0">
          <xsl:value-of select="format-number(($votes div $total-votes)*100 ,'0')"/>
        </xsl:when>
        <xsl:otherwise>0</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div class="optionResult">
      <p>
        <xsl:value-of select="Title/node()"/> - <xsl:value-of select="$percentageVote"/>%
      </p>
      <div class="pollBar">
        <xsl:attribute name="style">
          <xsl:text>width:</xsl:text>
          <xsl:value-of select="$percentageVote"/>
          <xsl:text>%;</xsl:text>
        </xsl:attribute>
        <xsl:text>&#160;</xsl:text>
      </div>
    </div>
  </xsl:template>


  <!--   ################   Feed Items   ###############   -->
  <!-- Feed Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='FeedList']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- Output Module -->
    <div class="FeedList">
      <div class="cols{@cols}">
        <!-- If Stepper, display Stepper -->
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="articleList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- Feed Brief -->
  <xsl:template match="Content[@type='FeedItem']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <xsl:variable name="parentURL">
      <xsl:choose>
        <xsl:when test="Link/node()!=''">
          <xsl:value-of select="Link/node()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="self::Content" mode="getHref">
            <xsl:with-param name="parId" select="@parId"/>
          </xsl:apply-templates>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div class="list-group-item listItem">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'list-group-item listItem list feed'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="list-item lIinner">
        <xsl:if test="Images/img/@src!=''">
          <a href="{$parentURL}" title="Read More - {Headline/node()}">
            <xsl:apply-templates select="." mode="displayThumbnail"/>
          </a>
          <!--Accessiblity fix : Separate adjacent links with more than whitespace-->
          <span class="hidden">|</span>
        </xsl:if>
        <h3 class="title">
          <a href="{$parentURL}" title="Read More - {@name}">
            <xsl:value-of select="@name" />
          </a>
        </h3>
        <xsl:if test="@publish!=''">
          <p class="date">
            <xsl:call-template name="DisplayDate">
              <xsl:with-param name="date" select="@publish"/>
            </xsl:call-template>
          </p>
        </xsl:if>
        <xsl:if test="Body/node()!=''">
          <div class="description">
            <xsl:variable name="normBody">
              <xsl:value-of select="normalize-space(Body)"/>
            </xsl:variable>
            <xsl:call-template name="truncate-string">
              <xsl:with-param name="text" select="$normBody"/>
              <xsl:with-param name="length" select="'250'"/>
            </xsl:call-template>
          </div>
        </xsl:if>
        <div class="entryFooter">
          <xsl:apply-templates select="." mode="displayTags"/>
          <xsl:apply-templates select="." mode="moreLink">
            <xsl:with-param name="link" select="$parentURL"/>
            <xsl:with-param name="altText">
              <xsl:value-of select="@name"/>
            </xsl:with-param>
          </xsl:apply-templates>
          <xsl:text> </xsl:text>
        </div>
        <!-- Accessiblity fix : Separate adjacent links with more than whitespace -->
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- Feed Detail -->
  <xsl:template match="Content[@type='FeedItem']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="self::Content" mode="getHref">
        <xsl:with-param name="parId" select="@parId"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="detail feed">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'list feed'"/>
      </xsl:apply-templates>
      <xsl:apply-templates select="." mode="displayDetailImage"/>
      <h2 class="title content-title">
        <xsl:value-of select="@name"/>
      </h2>
      <xsl:if test="@publish!=''">
        <p class="date">
          <xsl:call-template name="DisplayDate">
            <xsl:with-param name="date" select="@publish"/>
          </xsl:call-template>
        </p>
      </xsl:if>
      <xsl:if test="Body/node()!=''">
        <div class="description">
          <xsl:value-of select="normalize-space(Body)"/>
        </div>
      </xsl:if>
      <xsl:if test="Link/node()!=''">
        <xsl:apply-templates select="." mode="moreLink">
          <xsl:with-param name="link" select="Link/node()"/>
          <xsl:with-param name="altText">
            <xsl:value-of select="@name"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </xsl:if>
      <!-- Terminus class fix to floating content -->
      <div class="terminus">&#160;</div>
      <div class="entryFooter">
        <div class="tags">
          <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
          <xsl:text> </xsl:text>
        </div>
        <xsl:apply-templates select="." mode="backLink">
          <xsl:with-param name="link" select="$thisURL"/>
          <xsl:with-param name="altText">
            <!--click here to return to the feed list-->
            <xsl:call-template name="term2061" />
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>


  <!-- ############## Job Vacancy ##############   -->
  <!-- Job Vacancy Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='VacanciesList']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!--responsive columns variables-->
    <xsl:variable name="xsColsToShow">
      <xsl:choose>
        <xsl:when test="@xsCol='2'">2</xsl:when>
        <xsl:otherwise>1</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="smColsToShow">
      <xsl:choose>
        <xsl:when test="@smCol and @smCol!=''">
          <xsl:value-of select="@smCol"/>
        </xsl:when>
        <xsl:otherwise>2</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="mdColsToShow">
      <xsl:choose>
        <xsl:when test="@mdCol and @mdCol!=''">
          <xsl:value-of select="@mdCol"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@cols"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!--end responsive columns variables-->
    <!-- Output Module -->
    <div class="clearfix VacancyList">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix VacancyList content-scroller</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <div class="cols cols{@cols}" data-xscols="{$xsColsToShow}" data-smcols="{$smColsToShow}" data-mdcols="{$mdColsToShow}" data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1" >
        <!--responsive columns-->
        <xsl:attribute name="class">
          <xsl:text>cols</xsl:text>
          <xsl:choose>
            <xsl:when test="@xsCol='2'"> mobile-2-col-content</xsl:when>
            <xsl:otherwise> mobile-1-col-content</xsl:otherwise>
          </xsl:choose>
          <xsl:if test="@smCol and @smCol!=''">
            <xsl:text> sm-content-</xsl:text>
            <xsl:value-of select="@smCol"/>
          </xsl:if>
          <xsl:if test="@mdCol and @mdCol!=''">
            <xsl:text> md-content-</xsl:text>
            <xsl:value-of select="@mdCol"/>
          </xsl:if>
          <xsl:text> cols</xsl:text>
          <xsl:value-of select="@cols"/>
          <xsl:if test="@mdCol and @mdCol!=''">
            <xsl:text> content-cols-responsive</xsl:text>
          </xsl:if>
        </xsl:attribute>
        <!--end responsive columns-->
        <xsl:if test="@autoplay !=''">
          <xsl:attribute name="data-autoplay">
            <xsl:value-of select="@autoplay"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="@autoPlaySpeed !=''">
          <xsl:attribute name="data-autoPlaySpeed">
            <xsl:value-of select="@autoPlaySpeed"/>
          </xsl:attribute>
        </xsl:if>
        <!-- If Stepper, display Stepper -->
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="vacancyList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:choose>
          <xsl:when test="@linkArticle='true'">
            <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefLinked">
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
            <xsl:text> </xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
            <xsl:text> </xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </div>
    </div>
  </xsl:template>

  <!-- Job Brief -->
  <xsl:template match="Content[@type='Job']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <!-- vacancyBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="listItem list-group-item">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem list-group-item'"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <h3 class="title">
          <a href="{$parentURL}" title="{JobTitle/node()}">
            <xsl:value-of select="JobTitle/node()"/>
          </a>
        </h3>
        <a href="{$parentURL}" title="Read More - {Headline/node()}" class="vacancy-image">
          <xsl:apply-templates select="." mode="displayThumbnail"/>
        </a>
        <div class="vacancy-intro">
          <dl class="dl-horizontal">
            <xsl:if test="@publish and @publish!=''">
              <dt class="date">
                <!--Added on-->
                <xsl:call-template name="term2062" />
                <xsl:text>: </xsl:text>
              </dt>
              <dd>
                <xsl:call-template name="DisplayDate">
                  <xsl:with-param name="date" select="@publish"/>
                </xsl:call-template>
              </dd>
            </xsl:if>
            <xsl:if test="ContractType/node()!=''">
              <dt class="contract">
                <!--Contract Type-->
                <xsl:call-template name="term2063" />
                <xsl:text>: </xsl:text>
              </dt>
              <dd>
                <xsl:value-of select="ContractType/node()"/>
              </dd>
            </xsl:if>
            <xsl:if test="Ref/node()!=''">
              <dt class="ref">
                <!--Ref-->
                <xsl:call-template name="term2064" />
                <xsl:text>: </xsl:text>
              </dt>
              <dd>
                <xsl:value-of select="Ref/node()"/>
              </dd>
            </xsl:if>
            <xsl:if test="Location/node()!=''">
              <dt class="location">
                <!--Location-->
                <xsl:call-template name="term2065" />
                <xsl:text>: </xsl:text>
              </dt>
              <dd>
                <xsl:value-of select="Location/node()"/>
              </dd>
            </xsl:if>
            <xsl:if test="Salary/node()!=''">
              <dt class="salary">
                <!--Salary-->
                <xsl:call-template name="term2066" />
                <xsl:text>: </xsl:text>
              </dt>
              <dd>
                <xsl:value-of select="Salary/node()"/>
              </dd>
            </xsl:if>
            <xsl:if test="ApplyBy/node()!=''">
              <dt class="applyBy">
                <!--Deadline for applications-->
                <xsl:call-template name="term2067" />
                <xsl:text>: </xsl:text>
              </dt>
              <dd>
                <xsl:call-template name="DisplayDate">
                  <xsl:with-param name="date" select="ApplyBy/node()"/>
                </xsl:call-template>
              </dd>
            </xsl:if>
          </dl>
        </div>
        <div class="vacancy-summary">
          <xsl:if test="Summary/node()!=''">
            <xsl:apply-templates select="Summary/node()" mode="cleanXhtml"/>
          </xsl:if>
        </div>
        <div class="entryFooter">
          <xsl:apply-templates select="." mode="displayTags"/>
          <xsl:apply-templates select="." mode="moreLink">
            <xsl:with-param name="link" select="$parentURL"/>
            <xsl:with-param name="altText">
              <xsl:value-of select="JobTitle/node()"/>
            </xsl:with-param>
          </xsl:apply-templates>
          <xsl:text> </xsl:text>
        </div>
      </div>
    </div>
  </xsl:template>

  <!-- Job Brief -->
  <xsl:template match="Content[@type='Job']" mode="displayBriefLinked">
    <xsl:param name="sortBy"/>
    <!-- vacancyBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="listItem list-group-item">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem list-group-item'"/>
      </xsl:apply-templates>
      <a href="{$parentURL}" title="{JobTitle/node()}">
        <div class="lIinner">
          <h3 class="title">
            <xsl:value-of select="JobTitle/node()"/>

          </h3>
          <xsl:apply-templates select="." mode="displayThumbnail"/>
          <div class="vacancy-intro">
            <dl class="dl-horizontal">
              <xsl:if test="@publish and @publish!=''">
                <dt class="date">
                  <!--Added on-->
                  <xsl:call-template name="term2062" />
                  <xsl:text>: </xsl:text>
                </dt>
                <dd>
                  <xsl:call-template name="DisplayDate">
                    <xsl:with-param name="date" select="@publish"/>
                  </xsl:call-template>
                </dd>
              </xsl:if>
              <xsl:if test="ContractType/node()!=''">
                <dt class="contract">
                  <!--Contract Type-->
                  <xsl:call-template name="term2063" />
                  <xsl:text>: </xsl:text>
                </dt>
                <dd>
                  <xsl:value-of select="ContractType/node()"/>
                </dd>
              </xsl:if>
              <xsl:if test="Ref/node()!=''">
                <dt class="ref">
                  <!--Ref-->
                  <xsl:call-template name="term2064" />
                  <xsl:text>: </xsl:text>
                </dt>
                <dd>
                  <xsl:value-of select="Ref/node()"/>
                </dd>
              </xsl:if>
              <xsl:if test="Location/node()!=''">
                <dt class="location">
                  <!--Location-->
                  <xsl:call-template name="term2065" />
                  <xsl:text>: </xsl:text>
                </dt>
                <dd>
                  <xsl:value-of select="Location/node()"/>
                </dd>
              </xsl:if>
              <xsl:if test="Salary/node()!=''">
                <dt class="salary">
                  <!--Salary-->
                  <xsl:call-template name="term2066" />
                  <xsl:text>: </xsl:text>
                </dt>
                <dd>
                  <xsl:value-of select="Salary/node()"/>
                </dd>
              </xsl:if>
              <xsl:if test="ApplyBy/node()!=''">
                <dt class="applyBy">
                  <!--Deadline for applications-->
                  <xsl:call-template name="term2067" />
                  <xsl:text>: </xsl:text>
                </dt>
                <dd>
                  <xsl:call-template name="DisplayDate">
                    <xsl:with-param name="date" select="ApplyBy/node()"/>
                  </xsl:call-template>
                </dd>
              </xsl:if>
            </dl>
          </div>
          <div class="vacancy-summary">
            <xsl:if test="Summary/node()!=''">
              <xsl:apply-templates select="Summary/node()" mode="cleanXhtml"/>
            </xsl:if>
          </div>

        </div>
      </a>
    </div>
  </xsl:template>

  <!-- Job Detail -->
  <xsl:template match="Content[@type='Job']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <div class="detail vacancy" itemscope="" itemtype="http://schema.org/JobPosting" >
      <meta itemprop="specialCommitments" content="" />
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail vacancy'"/>
      </xsl:apply-templates>
      <h1 class="content-title" itemprop="title">
        <xsl:value-of select="JobTitle/node()"/>
      </h1>
      <div class="vacancy-intro">
        <dl class="dl-horizontal">
          <xsl:if test="@publish and @publish!=''">
            <dt class="date" itemprop="datePosted">
              <xsl:call-template name="term2068" />
              <xsl:text>: </xsl:text>
            </dt>
            <dd>
              <xsl:call-template name="DisplayDate">
                <xsl:with-param name="date" select="@publish"/>
              </xsl:call-template>
            </dd>
          </xsl:if>
          <xsl:if test="ApplyBy/node()">
            <dt class="applyBy">
              <!--Deadline for applications-->
              <xsl:call-template name="term2067" />
              <xsl:text>:</xsl:text>
            </dt>
            <dd>
              <xsl:if test="ApplyBy/node()!=''">
                <xsl:call-template name="DisplayDate">
                  <xsl:with-param name="date" select="ApplyBy/node()"/>
                </xsl:call-template>
              </xsl:if>
            </dd>
          </xsl:if>
          <xsl:if test="ContractType/node()">
            <dt class="jobContractType ">
              <!--Contract Type-->
              <xsl:call-template name="term2063" />
              <xsl:text>:</xsl:text>
            </dt>
            <dd class="item"  itemprop="employmentType">
              <xsl:apply-templates select="ContractType/node()" mode="cleanXhtml"/>
            </dd>
          </xsl:if>
          <xsl:if test="Ref/node()">
            <dt class="ref ">
              <!--Reference-->
              <xsl:call-template name="term2069" />
              <xsl:text>:</xsl:text>
            </dt>
            <dd class="item">
              <xsl:apply-templates select="Ref/node()" mode="cleanXhtml"/>
            </dd>
          </xsl:if>
          <xsl:if test="JobIndustry/node()">
            <dt class="Jobindustry ">
              <!--Industry-->
              <xsl:call-template name="term2085" />
              <xsl:text>:</xsl:text>
            </dt>
            <dd itemprop="industry">
              <xsl:apply-templates select="JobIndustry/node()" mode="cleanXhtml"/>
            </dd>
          </xsl:if>
          <xsl:if test="JobOccupation/@ref">
            <dt class="hidden">
              <!--ref-->
              <xsl:call-template name="term2064" />
              <xsl:text>:</xsl:text>
            </dt>
            <dd class="hidden" itemprop="occupationalCategory">
              <xsl:apply-templates select="JobOccupation/@ref" mode="cleanXhtml"/>
            </dd>
          </xsl:if>
          <xsl:if test="JobOccupation/@name and JobOccupation/@name!=''">
            <dt class="JobOccupation ">
              <!--Occupation-->
              <xsl:call-template name="term2086" />
              <xsl:text>:</xsl:text>
            </dt>
            <dd class="item" itemprop="occupationalCategory">
              <xsl:apply-templates select="JobOccupation/@name" mode="cleanXhtml"/>
            </dd>
          </xsl:if>
          <xsl:if test="Location/node()">
            <dt class="jobLocation ">
              <!--Location-->
              <xsl:call-template name="term2065" />
              <xsl:text>:</xsl:text>
            </dt>
            <dd class="item" itemprop="jobLocation" itemscope="" itemtype="http://schema.org/Place">
              <xsl:apply-templates select="Location/node()" mode="displayContent"/>
            </dd>
          </xsl:if>
          <xsl:if test="JobHours/node()">
            <dt class="JobHours">
              <!--work hours-->
              <xsl:call-template name="term2090" />
              <xsl:text>:</xsl:text>
            </dt>
            <dd class="item" itemprop="workHours">
              <xsl:apply-templates select="JobHours/node()" mode="cleanXhtml"/>
            </dd>
          </xsl:if>
          <xsl:if test="Salary/node()!=''">
            <dt class="BaseSalary ">
              <!--Base Salary-->
              <xsl:call-template name="term2087" />
              <xsl:text>:</xsl:text>
            </dt>
            <dd class="item" itemprop="baseSalary">
              <xsl:apply-templates select="Salary/node()" mode="cleanXhtml"/>
            </dd>
          </xsl:if>
          <xsl:if test="Salary/@currency">
            <dt class="Currency hidden">
              <!--Currency-->
              <xsl:call-template name="term2088" />
              <xsl:text>:</xsl:text>
            </dt>
            <dd class="hidden" itemprop="salaryCurrency">
              <xsl:apply-templates select="Salary/@currency" mode="cleanXhtml"/>
            </dd>
          </xsl:if>
          <xsl:if test="Salary/@figure">
            <dt class="Salaryfigre hidden">
              <!--Figure-->
              <xsl:call-template name="term2089" />
              <xsl:text>:</xsl:text>
            </dt>
            <dd class="hidden" >
              <xsl:apply-templates select="Salary/@figure" mode="cleanXhtml"/>
            </dd>
          </xsl:if>
        </dl>
      </div>
      <xsl:apply-templates select="." mode="displayDetailImage"/>
      <xsl:if test="Description/node()">
        <h3 class="Jobdescription">
          <!--Description-->
          <xsl:call-template name="term2092" />
        </h3>
        <div itemprop="description">
          <xsl:apply-templates select="Description/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>
      <xsl:if test="Responsibities/node()">
        <h3 class="Responsibities">
          <!--Responsibilities-->
          <xsl:call-template name="term2084" />
        </h3>
        <div itemprop="responsibilities">
          <xsl:apply-templates select="Responsibities/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>
      <xsl:if test="Skills/node()">
        <h3 class="Skills">
          <xsl:call-template name="term2094" />
        </h3>
        <div itemprop="skills" >
          <xsl:apply-templates select="Skills/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>
      <xsl:if test="EducationRequirements/node()">
        <h3 class="EducationRequirements">
          <xsl:call-template name="term2095" />
        </h3>
        <div itemprop="educationRequirements">
          <xsl:apply-templates select="EducationRequirements/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>
      <xsl:if test="ExperienceRequirements/node()">
        <h3 class="ExperienceRequirements">
          <xsl:call-template name="term2096" />
        </h3>
        <div itemprop="experienceRequirements">
          <xsl:apply-templates select="ExperienceRequirements/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>
      <xsl:if test="Qualifications/node()">
        <h3 class="Qualifications">
          <xsl:call-template name="term2097" />
        </h3>
        <div itemprop="qualifications">
          <xsl:apply-templates select="Qualifications/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>
      <xsl:if test="Incentives/node()">
        <h3 class="Incentives">
          <xsl:call-template name="term2098" />
        </h3>
        <div itemprop="incentives">
          <xsl:apply-templates select="Incentives/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>
      <div class="entryFooter">
        <div class="tags">
          <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
          <xsl:text> </xsl:text>
        </div>
        <xsl:apply-templates select="." mode="backLink">
          <xsl:with-param name="link" select="$thisURL"/>
          <xsl:with-param name="altText">
            <!--click here to return to the news article list-->
            <xsl:call-template name="term2071" />
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>


  <!-- ############## FAQ Module ##############   -->
  <!-- FAQ Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='FAQList']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div class="faqList">
      <a name="pageTop" class="pageTop">&#160;</a>
      <div id="pageMenu">
        <ul>
          <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayFAQMenu"/>
        </ul>
        <div class="terminus">&#160;</div>
      </div>
      <div class="cols cols{@cols}">
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="articleList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- FAQ Menu -->
  <xsl:template match="Content[@type='FAQ']" mode="displayFAQMenu">
    <xsl:variable name="currentUrl">
      <xsl:apply-templates select="$currentPage" mode="getHref"/>
    </xsl:variable>
    <li>
      <a href="#faq-{@id}" title="{@name}">
        <xsl:choose>
          <!-- Older sites might not have the DisplayName Field, had to be introduced to allow ? when used as an FAQ page. -->
          <xsl:when test="DisplayName/node()!=''">
            <xsl:value-of select="DisplayName/node()"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="@name"/>
          </xsl:otherwise>
        </xsl:choose>
      </a>
      <xsl:if test="Strapline/node()!=''">
        <span class="infoTopics">
          <br/>
          <xsl:apply-templates select="Strapline" mode="cleanXhtml"/>
        </span>
      </xsl:if>
    </li>
  </xsl:template>

  <!-- FAQ Brief -->
  <xsl:template match="Content[@type='FAQ']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <div class="listItem faq">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <a name="faq-{@id}" class="faq-link">
          &#160;
        </a>
        <h3>
          <xsl:choose>
            <!-- Older sites might not have the DisplayName Field, had to be introduced to allow ? when used as an FAQ page. -->
            <xsl:when test="DisplayName/node()!=''">
              <xsl:value-of select="DisplayName/node()"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="@name"/>
            </xsl:otherwise>
          </xsl:choose>
        </h3>
        <xsl:if test="Images/img[@class='thumbnail']/@src!=''">
          <img src="{Images/img[@class='thumbnail']/@src}" width="{Images/img[@class='thumbnail']/@width}" height="{Images/img[@class='thumbnail']/@height}" alt="{Images/img[@class='thumbnail']/@alt}" class="thumbnail"/>
        </xsl:if>
        <div class="description">
          <xsl:apply-templates select="Body" mode="cleanXhtml"/>
        </div>
        <div class="terminus">&#160;</div>
        <div class="backTop">
          <a href="#pageTop" title="Back to Top">Back To Top</a>
        </div>
        <!-- Terminus class fix to floating columns -->
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>


  <xsl:template match="Content[@moduleType='FAQList']" mode="JSONLD">
    {
    "@context": "https://schema.org",
    "@type": "FAQPage",
    "mainEntity": [
    <xsl:apply-templates select="Content[@type='FAQ']" mode="JSONLD-list"/>
    <xsl:apply-templates select="$page/Contents/Content[@type='FAQ']" mode="JSONLD-list"/>
    ]
    }
  </xsl:template>

  <xsl:template match="Content[@type='FAQ']" mode="JSONLD-list">
    {
    "@type": "Question",
    "name": "<xsl:call-template name="escape-json">
      <xsl:with-param name="string">
        <xsl:apply-templates select="DisplayName" mode="flattenXhtml"/>
      </xsl:with-param>
    </xsl:call-template>",
    "acceptedAnswer": {
    "@type": "Answer",
    "text": "<xsl:call-template name="escape-json">
      <xsl:with-param name="string">
        <xsl:apply-templates select="Body" mode="flattenXhtml"/>
      </xsl:with-param>
    </xsl:call-template>"
    }
    }
    <xsl:if test="position()!=last()">,</xsl:if>
  </xsl:template>

  <!-- FAQ Module Accordian -->
  <xsl:template match="Content[@type='Module' and @moduleType='FAQList' and @presentationType='accordian']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType"/>
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType"/>
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div class="faqList panel-group accordion-module" id="accordion-{@id}" role="tablist" aria-multiselectable="true">
      <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayFAQAccordianBrief">
        <xsl:with-param name="parId" select="@id"/>
      </xsl:apply-templates>
    </div>
  </xsl:template>

  <!-- FAQ Menu -->
  <xsl:template match="Content[@type='FAQ']" mode="displayFAQAccordianBrief">
    <xsl:param name="sortBy"/>
    <xsl:param name="parId"/>
    <div class="panel-group" id="accordion{@id}" role="tablist" aria-multiselectable="true">
      <div class="panel panel-default">
        <div class="panel-heading" role="tab" id="heading{@id}">
          <xsl:apply-templates select="." mode="inlinePopupOptions">
            <xsl:with-param name="class" select="'panel-heading'"/>
            <xsl:with-param name="sortBy" select="$sortBy"/>
          </xsl:apply-templates>
          <a role="button" data-toggle="collapse" data-parent="#accordion{@id}" href="#accordian-item-{$parId}-{@id}" aria-expanded="false" aria-controls="accordian-item-{$parId}-{@id}" class="accordion-load">
            <h4 class="panel-title">
              <i class="fa fa-caret-down">&#160;</i>&#160;<xsl:apply-templates select="DisplayName" mode="cleanXhtml"/>
            </h4>
          </a>
          <xsl:if test="Strapline/node()!=''">
            <div class="strapline">
              <xsl:apply-templates select="Strapline" mode="cleanXhtml"/>
            </div>
          </xsl:if>
        </div>
        <div id="accordian-item-{$parId}-{@id}" class="panel-collapse collapse " role="tabpanel" aria-labelledby="heading{@id}">
          <div class="panel-body">
            <xsl:if test="Body/node()!=''">
              <xsl:apply-templates select="Body" mode="cleanXhtml"/>
            </xsl:if>
          </div>
        </div>
      </div>
    </div>
  </xsl:template>

  <!-- Image Fader Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='ImageFader']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!--InnerFade Module JS -->

    <!-- ###### -->
    <div id="imageFader">
      <xsl:if test="/Page/@adminMode">
        <div class="ewAdmin clear-fix">
          <xsl:choose>
            <xsl:when test="/Page/Request/QueryString/Item[@name='innerFade']">
              <a href="{$currentPage/@url}" class="btn btn-primary btn-xs">Start Fader</a>
            </xsl:when>
            <xsl:otherwise>
              <a href="{$currentPage/@url}&amp;innerFade=disabled" class="btn btn-primary btn-xs">Stop Fader &amp; Show All</a>
            </xsl:otherwise>
          </xsl:choose>
          <br/>
          <br/>
        </div>
      </xsl:if>
      <div id="featureImages{@id}" class="innerfade">
        <xsl:apply-templates select="Content[@type='Image']" mode="faderImage2">
          <xsl:with-param name="max-width" select="@SlideWidth"/>
          <xsl:with-param name="max-height" select="@SlideHeight"/>
        </xsl:apply-templates>
      </div>
      <div class="terminus">&#160;</div>
    </div>
  </xsl:template>
  <xsl:template match="Content[@type='Module' and @moduleType='ImageFader']" mode="contentJS">
    <xsl:if test="not(/Page/Request/QueryString/Item[@name='innerFade']/node()='disabled')">
      <xsl:variable name="animationtype">
        <xsl:value-of select="@animationtype"/>
      </xsl:variable>
      <xsl:variable name="fadingspeed">
        <xsl:value-of select="@fadingspeed"/>
      </xsl:variable>
      <xsl:variable name="fadebetween">
        <xsl:value-of select="@fadebetween"/>
      </xsl:variable>
      <xsl:variable name="slideshowtype">
        <xsl:value-of select="@slideshowtype"/>
      </xsl:variable>
      <xsl:variable name="containerheight">
        <xsl:apply-templates select="Content[1]" mode="maxFaderHeight">
          <xsl:with-param name="maxheight">
            <xsl:value-of select="Content[1]/img/@height"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </xsl:variable>
      <script type="text/javascript">
        <xsl:text>$(document).ready(function(){$('#featureImages</xsl:text>
        <xsl:value-of select="@id"/>
        <xsl:text>').innerfade({animationtype: 'fade',speed: </xsl:text>
        <xsl:value-of select="$fadingspeed"/>
        <xsl:text>,timeout: </xsl:text>
        <xsl:value-of select="$fadebetween"/>
        <xsl:text>,type: '</xsl:text>
        <xsl:value-of select="$slideshowtype"/>
        <xsl:text>',containerheight: '</xsl:text>
        <xsl:choose>
          <xsl:when test="@SlideHeight!=''">
            <xsl:value-of select="@SlideHeight"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="$containerheight"/>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text>px'});});</xsl:text>
      </script>
    </xsl:if>
  </xsl:template>


  <xsl:template match="Content[@type='Image']" mode="faderImage2">
    <xsl:param name="max-width" />
    <xsl:param name="max-height" />
    <div class="imagesfeature">
      <xsl:apply-templates select="." mode="inlinePopupOptions"/>
      <xsl:choose>
        <xsl:when test="$max-width!=''">
          <xsl:apply-templates select="." mode="resize-image-params">
            <xsl:with-param name="max-width" select="$max-width"/>
            <xsl:with-param name="max-height" select="$max-height"/>
            <xsl:with-param name="crop" select="'true'"/>
          </xsl:apply-templates>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="." mode="displayBrief"/>
        </xsl:otherwise>
      </xsl:choose>
    </div>
  </xsl:template>

  <xsl:template match="Content" mode="maxFaderHeight">
    <xsl:param name="maxheight"/>
    <xsl:choose>
      <xsl:when test="following-sibling::Content">
        <xsl:apply-templates select="following-sibling::Content[1]" mode="maxFaderHeight">
          <xsl:with-param name="maxheight">
            <xsl:choose>
              <xsl:when test="following-sibling::Content[1]/img/@height &gt; $maxheight">
                <xsl:value-of select="following-sibling::Content[1]/img/@height"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$maxheight"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:with-param>
        </xsl:apply-templates>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$maxheight"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Slide Carousel -->
  <xsl:template match="Content[@type='Module' and @moduleType='SlideCarousel']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!--InnerFade Module JS -->
    <xsl:if test="not(/Page/Request/QueryString/Item[@name='innerFade']/node()='disabled')">
      <xsl:variable name="animationtype">
        <xsl:value-of select="@animationtype"/>
      </xsl:variable>
      <xsl:variable name="SlidesShowing">
        <xsl:value-of select="@SlidesShowing"/>
      </xsl:variable>
      <xsl:variable name="AutoPlay">
        <xsl:value-of select="@AutoPlay"/>
      </xsl:variable>
      <xsl:variable name="hAlign">
        <xsl:value-of select="@hAlign"/>
      </xsl:variable>
      <xsl:variable name="vAlign">
        <xsl:value-of select="@vAlign"/>
      </xsl:variable>
      <xsl:variable name="SlideHeight">
        <xsl:value-of select="@SlideHeight"/>
      </xsl:variable>
      <xsl:variable name="SlideWidth">
        <xsl:value-of select="@SlideWidth"/>
      </xsl:variable>
      <xsl:variable name="CarouselHeight">
        <xsl:value-of select="@CarouselHeight"/>
      </xsl:variable>
      <xsl:variable name="CarouselWidth">
        <xsl:value-of select="@CarouselWidth"/>
      </xsl:variable>
      <xsl:variable name="fadebetween">
        <xsl:value-of select="@fadebetween"/>
      </xsl:variable>
      <xsl:variable name="slideshowtype">
        <xsl:value-of select="@slideshowtype"/>
      </xsl:variable>
      <xsl:variable name="containerheight">
        <xsl:apply-templates select="Content[1]" mode="maxFaderHeight">
          <xsl:with-param name="maxheight">
            <xsl:value-of select="Content[1]/img/@height"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </xsl:variable>
      <script type="text/javascript">
        <xsl:text>
        $(document).ready(function(){
        $('.carousel').carousel({hAlign:'</xsl:text>
        <xsl:value-of select="$hAlign"/>
        <xsl:text>',
        vAlign:'</xsl:text>
        <xsl:value-of select="$vAlign"/>
        <xsl:text>',
        hMargin:0.4,
        vMargin:0.2,
        frontWidth:</xsl:text>
        <xsl:value-of select="$SlideWidth"/>
        <xsl:text>,
        frontHeight:</xsl:text>
        <xsl:value-of select="$SlideHeight"/>
        <xsl:text>,
        carouselWidth:</xsl:text>
        <xsl:value-of select="$CarouselWidth"/>
        <xsl:text>,
        carouselHeight:</xsl:text>
        <xsl:value-of select="$CarouselHeight"/>
        <xsl:text>,
        left:0,
        right:0,
        top:27,
        bottom:0,
        backZoom:0.8,
        slidesPerScroll:</xsl:text>
        <xsl:value-of select="$SlidesShowing"/>
        <xsl:text>,
        speed:500,
        buttonNav:'none',
        directionNav:true,
        autoplay:</xsl:text>
        <xsl:value-of select="$AutoPlay"/>
        <xsl:text>,
        autoplayInterval:</xsl:text>
        <xsl:value-of select="$fadebetween"/>
        <xsl:text>,
        pauseOnHover:true,
        mouse:true,
        shadow:true,
        reflection:false,
        reflectionHeight:0.2,
        reflectionOpacity:0.5,
        reflectionColor:'255,255,255',
        description:false, descriptionContainer:'.description',
        backOpacity:1,
        before: function(carousel){},
        after: function(carousel){}
        });
        });
      </xsl:text>
      </script>
    </xsl:if>
    <!-- ###### -->
    <div class="carousel">
      <div class="slides">
        <!--<xsl:apply-templates select="Content[@type='Image']" mode="faderImage"/>-->
        <xsl:apply-templates select="Content[@type='Image']" mode="faderImage2">
          <xsl:with-param name="max-width" select="@SlideWidth"/>
          <xsl:with-param name="max-height" select="@SlideHeight"/>
          <xsl:with-param name="crop" select="'true'"/>
        </xsl:apply-templates>
      </div>
      <div class="terminus">&#160;</div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='Image']" mode="faderImage">
    <div>
      <xsl:apply-templates select="." mode="inlinePopupOptions"/>
      <xsl:apply-templates select="." mode="displayBrief"/>
    </div>
  </xsl:template>

  <xsl:template match="Content" mode="maxFaderHeight">
    <xsl:param name="maxheight"/>
    <xsl:choose>
      <xsl:when test="following-sibling::Content">
        <xsl:apply-templates select="following-sibling::Content[1]" mode="maxFaderHeight">
          <xsl:with-param name="maxheight">
            <xsl:choose>
              <xsl:when test="following-sibling::Content[1]/img/@height &gt; $maxheight">
                <xsl:value-of select="following-sibling::Content[1]/img/@height"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$maxheight"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:with-param>
        </xsl:apply-templates>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$maxheight"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- INFORMATION -->
  <xsl:template match="Content[@type='Information']" mode="displayBrief">
    <div id="{@id}" class="information">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="information"/>
      </xsl:apply-templates>
      <a name="{@id}">
        <h3>
          <xsl:apply-templates select="." mode="getDisplayName" />
        </h3>
      </a>
      <xsl:if test="Images/img/@src!=''">
        <xsl:apply-templates select="." mode="displayThumbnail"/>
      </xsl:if>
      <div class="description">
        <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
      </div>
    </div>
  </xsl:template>

  <!--  ======================================================================================  -->
  <!--  ==  Advanced Carousel  ========================================================================  -->
  <!--  ======================================================================================  -->

  <xsl:template match="Content[@type='Module' and @moduleType='AdvancedCarousel']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- ###### -->
    <div class="advanced-carousel-container" style="height:{@CarouselHeight}px">
      <div class="cover-container">
        <div class="advanced-carousel">
          <ul style="display:none">
            <xsl:choose>
              <xsl:when test="Content[@type='AdvancedCarouselSlide']">
                <xsl:apply-templates select="Content[@type='AdvancedCarouselSlide']" mode="displayBrief"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:apply-templates select="$page/Contents/Content[@type='AdvancedCarouselSlide']" mode="displayBrief"/>
              </xsl:otherwise>
            </xsl:choose>
          </ul>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='Module' and @moduleType='AdvancedCarousel']" mode="contentJS">
    <script type="text/javascript">
      <xsl:text>jQuery(document).ready(function() {</xsl:text>
      <xsl:text>revapi = jQuery('#mod_</xsl:text><xsl:value-of select="@id"/><xsl:text> .advanced-carousel').revolution({</xsl:text>
      delay:<xsl:value-of select="@fadebetween"/>,
      startwidth:<xsl:value-of select="@CarouselWidth"/>,
      startheight:<xsl:value-of select="@CarouselHeight"/>,
      startWithSlide:0,
      fullScreenAlignForce:"off",
      autoHeight:"off",
      minHeight:"off",
      shuffle:"off",
      onHoverStop:"on",
      thumbWidth:100,
      thumbHeight:50,
      <xsl:choose>
        <xsl:when test="count(Content[@type='AdvancedCarouselSlide'])>2">
          thumbAmount:3,
        </xsl:when>
        <xsl:otherwise>
          thumbAmount:<xsl:value-of select="count(Content[@type='AdvancedCarouselSlide'])"/>,
        </xsl:otherwise>
      </xsl:choose>
      hideThumbsOnMobile:"off",
      hideNavDelayOnMobile:1500,
      hideBulletsOnMobile:"off",
      hideArrowsOnMobile:"off",
      hideThumbsUnderResoluition:0,

      hideThumbs:0,
      hideTimerBar:"<xsl:value-of select="@hideTimerBar"/>",

      keyboardNavigation:"on",

      navigationType:"<xsl:value-of select="@navigationType"/>",
      navigationArrows:"<xsl:value-of select="@navigationArrows"/>",
      navigationStyle:"<xsl:value-of select="@navigationStyle"/>",

      navigationHAlign:"<xsl:value-of select="@navigationHAlign"/>",
      navigationVAlign:"<xsl:value-of select="@navigationVAlign"/>",
      navigationHOffset:<xsl:value-of select="@navigationHOffset"/>,
      navigationVOffset:<xsl:value-of select="@navigationVOffset"/>,

      soloArrowLeftHalign:"<xsl:value-of select="@soloArrowLeftHalign"/>",
      soloArrowLeftValign:"<xsl:value-of select="@soloArrowLeftValign"/>",
      soloArrowLeftHOffset:<xsl:value-of select="@soloArrowLeftHOffset"/>,
      soloArrowLeftVOffset:<xsl:value-of select="@soloArrowLeftVOffset"/>,

      soloArrowRightHalign:"<xsl:value-of select="@soloArrowRightHalign"/>",
      soloArrowRightValign:"<xsl:value-of select="@soloArrowRightValign"/>",
      soloArrowRightHOffset:<xsl:value-of select="@soloArrowRightHOffset"/>,
      soloArrowRightVOffset:<xsl:value-of select="@soloArrowRightVOffset"/>,


      touchenabled:"on",
      swipe_velocity:"0.7",
      swipe_max_touches:"1",
      swipe_min_touches:"1",
      drag_block_vertical:"false",

      parallax:"mouse",
      parallaxBgFreeze:"on",
      parallaxLevels:[10,7,4,3,2,5,4,3,2,1],
      parallaxDisableOnMobile:"off",

      stopAtSlide:-1,
      stopAfterLoops:-1,
      hideCaptionAtLimit:0,
      hideAllCaptionAtLilmit:0,
      hideSliderAtLimit:0,

      dottedOverlay:"none",

      spinned:"<xsl:value-of select="@spinner"/>",

      fullWidth:"<xsl:value-of select="@fullWidth"/>",
      forceFullWidth:"off",
      fullScreen:"off",
      fullScreenOffsetContainer:"#topheader-to-offset",
      fullScreenOffset:"0px",

      panZoomDisableOnMobile:"off",

      simplifyAll:"off",

      shadow:0
      <xsl:text>});</xsl:text>
      revapi.bind("revolution.slide.onloaded",function (e) {
      jQuery('.advanced-carousel').find('.dropdown-menu').removeAttr('style');
      });
      revapi.bind("revolution.slide.onchange",function (e) {
      jQuery('.caption').each(function() {
      var thisWidth = $(this).width();
      var leftVal =  parseInt('0' + $(this).css('left').replace(/[^0-9\.]/g, '')) ;
      var dataWidth = $(this).data('width');
      if (dataWidth != 'done') {
      if (dataWidth === undefined) {
      dataWidth = 100
      }
      if (dataWidth == '') {
      dataWidth = 0
      };
      if (dataWidth == '0') {
      $(this).width('auto');
      }
      else
      {
      if (leftVal != 0) {
      $(this).width((thisWidth - leftVal) * (dataWidth / 100));
      $(this).data('width','done');
      }
      }
      };
      });
      });
      <xsl:text>});</xsl:text>


    </script>
  </xsl:template>

  <!-- Library Image Brief -->
  <xsl:template match="Content[@type='AdvancedCarouselSlide']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <xsl:variable name="lg-max-width">
      <xsl:apply-templates select="." mode="getFullSizeWidth"/>
    </xsl:variable>
    <xsl:variable name="lg-max-height">
      <xsl:apply-templates select="." mode="getFullSizeHeight"/>
    </xsl:variable>
    <xsl:variable name="fullSize">
      <xsl:call-template name="resize-image">
        <xsl:with-param name="path" select="Images/img[@class='detail']/@src"/>
        <xsl:with-param name="max-width" select="$lg-max-width"/>
        <xsl:with-param name="max-height" select="$lg-max-height"/>
        <xsl:with-param name="file-prefix">
          <xsl:text>~lg-</xsl:text>
          <xsl:value-of select="$lg-max-width"/>
          <xsl:text>x</xsl:text>
          <xsl:value-of select="$lg-max-width"/>
          <xsl:text>/~lg-</xsl:text>
        </xsl:with-param>
        <xsl:with-param name="file-suffix" select="''"/>
        <xsl:with-param name="quality" select="100"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="lgImgSrc">
      <xsl:choose>
        <xsl:when test="Images/img[@class='detail']/@src != ''">
          <xsl:value-of select="Images/img[@class='detail']/@src"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="Images/img[@class='display']/@src"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="slideLink">
      <xsl:choose>
        <xsl:when test="@internalLink!=''">
          <xsl:variable name="pageId" select="@internalLink"/>
          <xsl:variable name="href">
            <xsl:apply-templates select="//MenuItem[@id=$pageId]" mode="getHref" />
          </xsl:variable>
          <xsl:variable name="title">
            <xsl:apply-templates select="//MenuItem[@id=$pageId]" mode="getTitleAttr" />
          </xsl:variable>
          <xsl:value-of select="$href"/>
        </xsl:when>
        <xsl:when test="@externalLink!=''">
          <xsl:value-of select="@externalLink"/>
        </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <li data-transition="{@data-transition}" data-masterspeed="{@data-masterspeed}" data-slotamount="{@data-slotamount}" data-title="{Title/node()}" data-thumb="{Images/img[@class='detail']/@src}">
      <xsl:if test="not(/Page/@adminMode) and $slideLink!=''">
        <xsl:attribute name="data-link">
          <xsl:value-of select="$slideLink"/>
        </xsl:attribute>
      </xsl:if>
      <img src="{Images/img[@class='detail']/@src}" title="{Title/node()} - {Body/node()}" data-bgrepeat="{Images/@data-bgrepeat}" data-bgfit="{Images/@data-bgfit}" data-bgposition="{Images/@data-bgposition}">
        <xsl:if test="Images/@data-bgfitend!=''">
          <xsl:attribute name="data-bgfitend">
            <xsl:value-of select="Images/@data-bgfitend"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="Images/@data-bgpositionend!=''">
          <xsl:attribute name="data-bgpositionend">
            <xsl:value-of select="Images/@data-bgpositionend"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="Images/@data-kenburns!=''">
          <xsl:attribute name="data-kenburns">
            <xsl:value-of select="Images/@data-kenburns"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="Images/@data-duration!=''">
          <xsl:attribute name="data-duration">
            <xsl:value-of select="Images/@data-duration"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:if test="Images/@data-ease!=''">
          <xsl:attribute name="data-ease">
            <xsl:value-of select="Images/@data-ease"/>
          </xsl:attribute>
        </xsl:if>
      </img>
      <xsl:apply-templates select="Captions/*" mode="slideCaption"/>
      <xsl:if test="/Page/@adminMode">
        <div class="caption"  data-x="right" data-hoffset="-10" data-y="top" data-voffset="10"  data-speed="100" data-start="100">
          <xsl:apply-templates select="." mode="inlinePopupOptions">
            <xsl:with-param name="class" select="'caption'"/>
            <xsl:with-param name="sortBy" select="$sortBy"/>
          </xsl:apply-templates>
        </div>
      </xsl:if>
    </li>
  </xsl:template>

  <xsl:template match="Caption" mode="slideCaption">
    <div data-easing="{@data-easing}" data-x="{@data-x}" data-hoffset="{@data-hoffset}" data-y="{@data-y}" data-voffset="{@data-voffset}" data-start="{@data-start}" data-speed="{@data-speed}" data-end="{@data-end}" data-endspeed="{@data-endspeed}" data-endeasing="{@data-endeasing}" data-width="{@data-width}%">
      <xsl:attribute name="class">
        <xsl:text>caption </xsl:text>
        <xsl:value-of select="@style"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@styleIn"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@styleOut"/>
      </xsl:attribute>
      <xsl:apply-templates select="div/node()" mode="cleanXhtml"/>
    </div>
  </xsl:template>

  <xsl:template match="Caption[@type='image']" mode="slideCaption">
    <div data-easing="{@data-easing}" data-x="{@data-x}" data-hoffset="{@data-hoffset}" data-y="{@data-y}" data-voffset="{@data-voffset}" data-start="{@data-start}" data-speed="{@data-speed}" data-end="{@data-end}" data-endspeed="{@data-endspeed}" data-endeasing="{@data-endeasing}" data-width="{@data-width}%">
      <xsl:attribute name="class">
        <xsl:text>caption image </xsl:text>
        <xsl:value-of select="@style"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@styleIn"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@styleOut"/>
      </xsl:attribute>
      <img src="{@Image}"/>
    </div>
  </xsl:template>

  <!---Vimeo-->
  <xsl:template match="Caption[@type='Vimeo']" mode="slideCaption">
    <div data-easing="{@data-easing}" data-x="{@data-x}" data-hoffset="{@data-hoffset}" data-y="{@data-y}" data-voffset="{@data-voffset}" data-start="{@data-start}" data-speed="{@data-speed}" data-end="{@data-end}" data-endspeed="{@data-endspeed}" data-endeasing="{@data-endeasing}" data-width="{@data-width}%">
      <xsl:attribute name="class">
        <xsl:text>tp-caption tp-videolayer</xsl:text>
        <xsl:value-of select="@style"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@styleIn"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@styleOut"/>
      </xsl:attribute>
      <xsl:if test="@data-autoplayVimeo!=''">
        <xsl:attribute name="data-autoplay">
          <xsl:value-of select="@data-autoplayVimeo"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-autoplayonlyfirsttimeVimeo!=''">
        <xsl:attribute name="data-autoplayonlyfirsttime">
          <xsl:value-of select="@data-autoplayonlyfirsttimeVimeo"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videowidthVimeo!=''">
        <xsl:attribute name="data-videowidth">
          <xsl:value-of select="@data-videowidthVimeo"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videoheightVimeo!=''">
        <xsl:attribute name="data-videoheight">
          <xsl:value-of select="@data-videoheightVimeo"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-nextslideatendVimeo!=''">
        <xsl:attribute name="data-nextslideatend">
          <xsl:value-of select="@data-nextslideatendVimeo"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-vimeoid!=''">
        <xsl:attribute name="data-vimeoid">
          <xsl:value-of select="@data-vimeoid"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videocontrolsVimeo!=''">
        <xsl:attribute name="data-videocontrols">
          <xsl:value-of select="@data-videocontrolsVimeo"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videoattributesVimeo!=''">
        <xsl:attribute name="data-videoattributes">
          <xsl:value-of select="@data-videoattributesVimeo"/>
        </xsl:attribute>
      </xsl:if>
    </div>
  </xsl:template>

  <!--YouTube-->
  <xsl:template match="Caption[@type='YouTube']" mode="slideCaption">
    <div data-easing="{@data-easing}" data-x="{@data-x}" data-hoffset="{@data-hoffset}" data-y="{@data-y}" data-voffset="{@data-voffset}" data-start="{@data-start}" data-speed="{@data-speed}" data-end="{@data-end}" data-endspeed="{@data-endspeed}" data-endeasing="{@data-endeasing}" data-width="{@data-width}%" data-videoattributes="enablejsapi=1&amp;html5=1&amp;hd=1&amp;wmode=opaque&amp;showinfo=0&amp;rel=0">
      <xsl:attribute name="class">
        <xsl:text>tp-caption tp-videolayer</xsl:text>
        <xsl:value-of select="@style"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@styleIn"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@styleOut"/>
      </xsl:attribute>
      <xsl:if test="@data-autoplayYouTube!=''">
        <xsl:attribute name="data-autoplay">
          <xsl:value-of select="@data-autoplayYouTube"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-autoplayonlyfirsttimeYouTube!=''">
        <xsl:attribute name="data-autoplayonlyfirsttime">
          <xsl:value-of select="@data-autoplayonlyfirsttimeYouTube"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-nextslideatendYouTube!=''">
        <xsl:attribute name="data-nextslideatend">
          <xsl:value-of select="@data-nextslideatendYouTube"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videowidthYouTube!=''">
        <xsl:attribute name="data-videowidth">
          <xsl:value-of select="@data-videowidthYouTube"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videoheightYouTube!=''">
        <xsl:attribute name="data-videoheight">
          <xsl:value-of select="@data-videoheightYouTube"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-ytid!=''">
        <xsl:attribute name="data-ytid">
          <xsl:value-of select="@data-ytid"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videocontrolsYouTube!=''">
        <xsl:attribute name="data-videocontrols">
          <xsl:value-of select="@data-videocontrolsYouTube"/>
        </xsl:attribute>
      </xsl:if>
    </div>
  </xsl:template>

  <!--HTML5-->
  <xsl:template match="Caption[@type='HTML5']" mode="slideCaption">
    <div data-easing="{@data-easing}" data-x="{@data-x}" data-hoffset="{@data-hoffset}" data-y="{@data-y}" data-voffset="{@data-voffset}" data-start="{@data-start}" data-speed="{@data-speed}" data-end="{@data-end}" data-endspeed="{@data-endspeed}" data-endeasing="{@data-endeasing}" data-width="{@data-width}%">
      <xsl:attribute name="class">
        <xsl:text>tp-caption tp-videolayer</xsl:text>
        <xsl:value-of select="@style"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@styleIn"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@styleOut"/>
      </xsl:attribute>
      <xsl:if test="@data-autoplayHTML5!=''">
        <xsl:attribute name="data-autoplay">
          <xsl:value-of select="@data-autoplayHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-autoplayonlyfirsttimeHTML5!=''">
        <xsl:attribute name="data-autoplayonlyfirsttime">
          <xsl:value-of select="@data-autoplayonlyfirsttimeHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videoposterHTML5!=''">
        <xsl:attribute name="data-videoposter">
          <xsl:value-of select="@data-videoposterHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-forcecoverHTML5!=''">
        <xsl:attribute name="data-forcecover">
          <xsl:value-of select="@data-forcecoverHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-forcerewindHTML5!=''">
        <xsl:attribute name="data-forcerewind">
          <xsl:value-of select="@data-forcerewindHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-volumeHTML5!=''">
        <xsl:attribute name="data-volume">
          <xsl:value-of select="@data-volumeHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videowidthHTML5!=''">
        <xsl:attribute name="data-videowidth">
          <xsl:value-of select="@data-videowidthHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videoheightHTML5!=''">
        <xsl:attribute name="data-videoheight">
          <xsl:value-of select="@data-videoheightHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-aspectratioHTML5!=''">
        <xsl:attribute name="data-aspectratio">
          <xsl:value-of select="@data-aspectratioHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videopreloadHTML5!=''">
        <xsl:attribute name="data-videopreload">
          <xsl:value-of select="@data-videopreloadHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videomp4HTML5!=''">
        <xsl:attribute name="data-videomp4">
          <xsl:value-of select="@data-videomp4"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videowebmHTML5!=''">
        <xsl:attribute name="data-videowebm">
          <xsl:value-of select="@data-videowebmHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videoogvHTML5!=''">
        <xsl:attribute name="data-videoogv">
          <xsl:value-of select="@data-videoogvHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videocontrolsHTML5!=''">
        <xsl:attribute name="data-videocontrols">
          <xsl:value-of select="@data-videocontrolsHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videoattributesHTML5!=''">
        <xsl:attribute name="data-videoattributes">
          <xsl:value-of select="@data-videoattributesHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-nextslideatendHTML5!=''">
        <xsl:attribute name="data-nextslideatend">
          <xsl:value-of select="@data-nextslideatendHTML5"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-videoloopHTML5!=''">
        <xsl:attribute name="data-videoloop">
          <xsl:value-of select="@data-videoloopHTML5"/>
        </xsl:attribute>
      </xsl:if>
    </div>
  </xsl:template>

  <!-- Library Image Brief -->
  <xsl:template match="Content[@type='LibraryImageWithLink']" mode="displayBriefSliderGalleryBackground">
    <div class="item" style="background-image:url({Images/img[@class='detail']/@src})">
      <xsl:if test="position()=1">
        <xsl:attribute name="class">item active</xsl:attribute>
      </xsl:if>
      <xsl:if test="Title/node()!='' or Body/node()!=''">
        <div class="carousel-caption">
          <div class="carousel-caption-inner">
            <xsl:if test="Title/node()!=''">
              <h3 class="caption-title">
                <xsl:value-of select="Title/node()"/>
              </h3>
            </xsl:if>
            <xsl:apply-templates select="Body/node()" mode="cleanXhtml"></xsl:apply-templates>
            <xsl:if test="@link!=''">
              <xsl:apply-templates select="." mode="moreLink" />
            </xsl:if>
          </div>
        </div>
      </xsl:if>
    </div>
  </xsl:template>
  <!--  ======================================================================================  -->
  <!--  ==  RECIPIES  ========================================================================  -->
  <!--  ======================================================================================  -->
  <!-- Recipe Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='RecipeList']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- Output Module -->
    <div class="RecipeList">
      <div class="cols{@cols}">
        <!-- If Stepper, display Stepper -->
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="articleList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- Recipe Brief -->
  <xsl:template match="Content[@type='Recipe']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <!-- recipeBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="listItem list-group-item recipe hrecipe">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem list-group-item recipe hrecipe'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <h3 class="title">
          <a href="{$parentURL}" title="Read More - {Headline/node()}">
            <xsl:apply-templates select="." mode="getDisplayName"/>
          </a>
        </h3>
        <xsl:if test="Images/img/@src!=''">
          <a href="{$parentURL}" title="Read More - {Headline/node()}">
            <xsl:apply-templates select="." mode="displayThumbnail"/>
          </a>
          <!--Accessiblity fix : Separate adjacent links with more than whitespace-->
          <span class="hidden">|</span>
        </xsl:if>
        <div class="summary">
          <xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
        </div>
        <xsl:if test="Yield/node()!='' or PrepTime/node()!='' or CookTime/node()!=''">
          <div class="guide">
            <p>
              <xsl:if test="Yield/node()!=''">
                <span class="label">
                  <!-- No. of servings -->
                  <xsl:call-template name="term2075a"/>
                  <xsl:text>: </xsl:text>
                </span>
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
        </xsl:if>
        <div class="entryFooter">
          <xsl:apply-templates select="." mode="displayTags"/>
          <xsl:apply-templates select="." mode="moreLink">
            <xsl:with-param name="link" select="$parentURL"/>
            <xsl:with-param name="altText">
              <xsl:value-of select="Headline/node()"/>
            </xsl:with-param>
          </xsl:apply-templates>
          <xsl:text> </xsl:text>
        </div>
        <!-- Accessiblity fix : Separate adjacent links with more than whitespace -->
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- Recipe Detail -->
  <xsl:template match="Content[@type='Recipe']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <div id="template_2_Columns_66_33" class="detail recipe hrecipe template_2_Columns_66_33">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail hrecipe newsarticle'"/>
      </xsl:apply-templates>
      <div id="column1">
        <h1 class="fn">
          <xsl:apply-templates select="." mode="getDisplayName" />
        </h1>
        <xsl:if test="Strapline/node()">
          <div class="summary">
            <xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
          </div>
        </xsl:if>
        <div class="description">
          <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
        </div>
        <xsl:if test="Yield/node()!='' or PrepTime/node()!='' or CookTime/node()!=''">
          <div class="guide">
            <p>
              <xsl:if test="Yield/node()!=''">
                <span class="label">
                  <!-- No. of servings -->
                  <xsl:call-template name="term2075a"/>
                  <xsl:text>: </xsl:text>
                </span>
                <span class="yield">
                  <xsl:value-of select="Yield/node()" />
                </span>
                <br/>
              </xsl:if>
              <xsl:if test="PrepTime/node()!=''">
                <span class="preptime">
                  <span class="label">
                    <!-- Preparation time -->
                    <xsl:call-template name="term2076"/>
                    <xsl:text>: </xsl:text>
                  </span>
                  <xsl:call-template name="getNiceTimeFromMinutes">
                    <xsl:with-param name="mins" select="PrepTime/node()" />
                    <xsl:with-param name="format" select="'short'" />
                  </xsl:call-template>
                  <span class="value-title">
                    <xsl:attribute name="title">
                      <xsl:call-template name="getISO-8601-Duration">
                        <xsl:with-param name="secs" select="PrepTime/node() * 60" />
                      </xsl:call-template>
                    </xsl:attribute>
                  </span>
                </span>
                <br/>
              </xsl:if>
              <xsl:if test="CookTime/node()!=''">
                <span class="cooktime">
                  <span class="label">
                    <!-- Cooking time -->
                    <xsl:call-template name="term2077"/>
                    <xsl:text>: </xsl:text>
                  </span>
                  <xsl:call-template name="getNiceTimeFromMinutes">
                    <xsl:with-param name="mins" select="CookTime/node()" />
                    <xsl:with-param name="format" select="'short'" />
                  </xsl:call-template>
                  <span class="value-title">
                    <xsl:attribute name="title">
                      <xsl:call-template name="getISO-8601-Duration">
                        <xsl:with-param name="secs" select="CookTime/node() * 60" />
                      </xsl:call-template>
                    </xsl:attribute>
                  </span>
                </span>
              </xsl:if>
            </p>
          </div>
        </xsl:if>
        <xsl:if test="Ingredients/node()">
          <div class="ingredient">
            <!-- need both classes for microformats -->
            <h3>
              <!-- Ingredients -->
              <xsl:call-template name="term2075"/>
            </h3>
            <span class="ingredients">
              <xsl:apply-templates select="Ingredients/node()" mode="cleanXhtml"/>
            </span>
          </div>
        </xsl:if>
        <xsl:if test="Instructions/node()">
          <div class="instructions">
            <h3>
              <!-- Method -->
              <xsl:call-template name="term2078"/>
            </h3>
            <xsl:apply-templates select="Instructions/node()" mode="cleanXhtml"/>
          </div>
        </xsl:if>
        <!-- social networking - share -->
        <xsl:apply-templates select="." mode="share" />
        <!-- Reviews -->
        <xsl:apply-templates select="." mode="relatedReviews" />
      </div>
      <div id="column2">
        <xsl:apply-templates select="." mode="displayDetailImage"/>
        <xsl:if test="Equipment/node()">
          <div class="equipment">
            <!-- need both classes for microformats -->
            <h2>
              <!-- Ingredients -->
              <xsl:text>Equipment</xsl:text>
            </h2>
            <span class="equipment">
              <xsl:apply-templates select="Equipment/node()" mode="cleanXhtml"/>
            </span>
            <div class="terminus">&#160;</div>
          </div>
        </xsl:if>
        <div class="credentials">
          <p class="published">
            <!-- Published -->
            <xsl:call-template name="term2079"/>
            <xsl:text> </xsl:text>
            <xsl:call-template name="DisplayDate">
              <xsl:with-param name="date" select="@publish"/>
            </xsl:call-template>
            <span class="value-title" title="{substring(@publish,1,10)}"></span>
          </p>
          <xsl:if test="Content[@type='Contact']">
            <div class="author">
              <xsl:apply-templates select="Content[@type='Contact']" mode="displayBrief" />
            </div>
          </xsl:if>
        </div>
      </div>
      <!-- Terminus class fix to floating content -->
      <div class="terminus">&#160;</div>
      <div class="entryFooter">
        <div class="tags">
          <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
          <xsl:text> </xsl:text>
        </div>
        <xsl:apply-templates select="." mode="backLink">
          <xsl:with-param name="link" select="$thisURL"/>
          <xsl:with-param name="altText">
            <xsl:call-template name="term2006" />
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <!--RELATED CONTENT-->
      <xsl:if test="Content">
        <!-- Recipe  -->
        <xsl:if test="Content[@type='Recipe']">
          <div class="relatedcontent">
            <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
              <xsl:with-param name="type">PlainText</xsl:with-param>
              <xsl:with-param name="text">Add Title for Related Recipes</xsl:with-param>
              <xsl:with-param name="name">relatedRecipesTitle</xsl:with-param>
            </xsl:apply-templates>
            <h4>
              <xsl:choose>
                <xsl:when test="/Page/Contents/Content[@name='relatedRecipesTitle']">
                  <xsl:apply-templates select="/Page/Contents/Content[@name='relatedRecipesTitle']" mode="displayBrief"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>Related Recipes</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
            </h4>
            <xsl:apply-templates select="/" mode="List_Related_Recipes">
              <xsl:with-param name="parRecipeID" select="@id"/>
            </xsl:apply-templates>
          </div>
        </xsl:if>
      </xsl:if>
    </div>
  </xsl:template>

  <!-- List Related Recipes-->
  <xsl:template match="/" mode="List_Related_Recipes">
    <xsl:param name="parProductID"/>
    <xsl:for-each select="/Page/ContentDetail/Content/Content">
      <xsl:sort select="@type" order="ascending"/>
      <xsl:sort select="@displayOrder" order="ascending"/>
      <xsl:choose>
        <xsl:when test="@type='Recipe'">
          <xsl:apply-templates select="." mode="displayBrief"/>
        </xsl:when>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>

  <!-- Recipe Grid Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='RecipeGrid']" mode="displayBrief">
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div class="Recipe Grid">
      <div class="cols{@cols}">
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="articleList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefGrid">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- Recipe Grid Brief -->
  <xsl:template match="Content[@type='Recipe']" mode="displayBriefGrid">
    <xsl:param name="sortBy"/>
    <xsl:param name="pos"/>
    <xsl:variable name="parId">
      <xsl:choose>
        <xsl:when test="@parId &gt; 0">
          <xsl:value-of select="@parId"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="/Page/@id"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="self::Content" mode="getHref">
        <xsl:with-param name="parId" select="$parId"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="grid-item">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'grid-item hproduct'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <a href="{$parentURL}" title="{Name}">
        <div class="thumbnail">
          <xsl:if test="Images/img[@src!='']">
            <img src="{Images/img[@class='detail']/@src}" class="img-responsive" style="overflow:hidden;"/>
          </xsl:if>
          <div class="caption">
            <h4>
              <xsl:apply-templates select="." mode="getDisplayName"/>
            </h4>
          </div>
        </div>
      </a>
    </div>
  </xsl:template>

  <!-- Specification Link -->
  <xsl:template match="Content" mode="SpecLink">
    <xsl:if test="SpecificationDocument/node()!=''">
      <p class="doclink">
        <a class="{substring-after(SpecificationDocument/node(),'.')}icon" href="{SpecificationDocument/node()}" title="Click here to download a copy of this document">Download Product Specification</a>
      </p>
    </xsl:if>
  </xsl:template>


  <!--  ======================================================================================  -->
  <!--  ==  REVIEWS  =========================================================================  -->
  <!--  ======================================================================================  -->
  <!-- Old Template for legacy matching -->
  <xsl:template match="/" mode="List_Related_Reviews">
    <xsl:apply-templates mode="relatedReviews" select="$page/ContentDetail/Content" />
  </xsl:template>

  <!-- related review template, matches on parent Content e.g Product or Recipe -->
  <xsl:template match="Content" mode="relatedReviews">
    <xsl:if test="Content[@type='Review']">
      <div class="relatedcontent reviews">
        <h4>
          <xsl:call-template name="term2016" />
        </h4>
        <div class="hreview-aggregate">
          <xsl:apply-templates select="." mode="aggregateReviews" />
        </div>
        <xsl:apply-templates select="Content[@type='Review']" mode="displayBrief">
          <xsl:sort select="@publish" order="ascending"/>
          <xsl:sort select="@update" order="ascending"/>
        </xsl:apply-templates>
        <div class="terminus">&#160;</div>
      </div>
    </xsl:if>
  </xsl:template>

  <!-- aggregate reviews -->
  <xsl:template match="Content" mode="aggregateReviews">
    <p class="rating-foreground rating">
      <xsl:variable name="OverallRating">
        <xsl:apply-templates select="." mode="getAverageReviewRating"/>
      </xsl:variable>
      <span>
        <xsl:attribute name="class">
          <xsl:text>value-title reviewRate rating</xsl:text>
          <xsl:value-of select="$OverallRating"/>
        </xsl:attribute>
        <xsl:attribute name="title">
          <xsl:value-of select="$OverallRating"/>
        </xsl:attribute>
        <xsl:attribute name="alt">
          <xsl:call-template name="term2020" />
          <xsl:text>:&#160;</xsl:text>
          <xsl:value-of select="$OverallRating"/>
          <xsl:text>&#160;</xsl:text>
          <xsl:call-template name="term2021" />
        </xsl:attribute>
        <xsl:text>&#160;</xsl:text>
      </span>
      <xsl:text> based on </xsl:text>
      <span class="count">
        <xsl:value-of select="count(Content[@type='Review'])"/>
      </span>
      <xsl:text> review</xsl:text>
      <xsl:if test="count(Content[@type='Review']) &gt; 1">
        <xsl:text>s</xsl:text>
      </xsl:if>
    </p>
  </xsl:template>

  <!-- Review Brief -->
  <xsl:template match="Content[@type='Review']" mode="displayBrief">
    <xsl:param name="pos"/>
    <xsl:param name="class"/>
    <xsl:variable name="parId">
      <xsl:choose>
        <xsl:when test="@parId &gt; 0">
          <xsl:value-of select="@parId"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="/Page/@id"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="self::Content" mode="getHref">
        <xsl:with-param name="parId" select="$parId"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="listItem hreview {$class}">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="concat('listItem hreview ',$class)"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <h3 style="display:none;" class="title item">
          <a class="fn" href="{$parentURL}" title="{@name}">
            <xsl:value-of select="/Page/ContentDetail/Content/@name"/>
          </a>
        </h3>
        <xsl:choose>
          <xsl:when test="Path!=''">
            <!-- When is a pdf -->
            <a rel="external">
              <xsl:attribute name="href">
                <xsl:choose>
                  <xsl:when test="contains(Path,'http://')">
                    <xsl:value-of select="Path/node()"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$appPath"/>
                    <xsl:text>ewcommon/tools/download.ashx?docId=</xsl:text>
                    <xsl:value-of select="@id"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
              <xsl:attribute name="title">
              </xsl:attribute>
              <xsl:apply-templates select="." mode="displayThumbnail"/>
            </a>
          </xsl:when>
          <!-- When is a image -->
          <xsl:otherwise>
            <xsl:apply-templates select="." mode="displayDetailImage"/>
          </xsl:otherwise>
        </xsl:choose>

        <xsl:if test="Rating/node()!=''">
          <span class="rating-foreground rating">
            <span>
              <xsl:attribute name="class">
                <xsl:text>value-title reviewRate rating</xsl:text>
                <xsl:value-of select="Rating"/>
              </xsl:attribute>
              <xsl:attribute name="title">
                <xsl:value-of select="Rating"/>
              </xsl:attribute>
              <xsl:attribute name="alt">
                <xsl:call-template name="term2020" />
                <xsl:text>:&#160;</xsl:text>
                <xsl:value-of select="Rating"/>
                <xsl:text>&#160;</xsl:text>
                <xsl:call-template name="term2021" />
              </xsl:attribute>
              <xsl:choose>
                <xsl:when test="Rating='5'">
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                </xsl:when>
                <xsl:when test="Rating='4'">
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                </xsl:when>
                <xsl:when test="Rating='3'">
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>

                </xsl:when>
                <xsl:when test="Rating='2'">
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>

                </xsl:when>
                <xsl:when test="Rating='1'">
                  <i class="fa fa-star">
                    <xsl:text> </xsl:text>
                  </i>
                </xsl:when>
              </xsl:choose>
            </span>
            <br/>
          </span>
        </xsl:if>
        <xsl:call-template name="term2018" />
        <xsl:text>&#160;</xsl:text>
        <span class="reviewer">
          <xsl:value-of select="Reviewer"/>
        </span>
        <xsl:text>&#160;</xsl:text>
        <!--<xsl:call-template name="term2019" />-->
        <xsl:text>&#160;</xsl:text>
        <!--<span class="dtreviewed">
          <xsl:call-template name="DisplayDate">
            <xsl:with-param name="date" select="ReviewDate/node()"/>
          </xsl:call-template>
          <span class="value-title">
            <xsl:attribute name="title">
              <xsl:value-of select="ReviewDate/node()"/>
            </xsl:attribute>
          </span>
        </span>-->



        <span class="summary">
          <xsl:apply-templates select="Summary/node()" mode="cleanXhtml"/>
        </span>
        <!-- Terminus class fix to floating columns -->
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- Review Detail  - a review shouldn't really have a detail - is a simple bit of content. -->
  <xsl:template match="Content[@type='Review']" mode="ContentDetail">
    <xsl:param name="pos"/>
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"/>
    <xsl:variable name="parId">
      <xsl:choose>
        <xsl:when test="@parId &gt; 0">
          <xsl:value-of select="@parId"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="/Page/@id"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="self::Content" mode="getHref">
        <xsl:with-param name="parId" select="$parId"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="detail hreview">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem'"/>
      </xsl:apply-templates>
      <h3 class="title item content-title">
        <a class="fn" href="{$parentURL}" title="{@name}">
          <xsl:value-of select="/Page/ContentDetail/Content/@name"/>
        </a>
      </h3>
      <span class="rating-foreground rating">
        <span>
          <xsl:attribute name="class">
            <xsl:text>value-title reviewRate rating</xsl:text>
            <xsl:value-of select="Rating"/>
          </xsl:attribute>
          <xsl:attribute name="title">
            <xsl:value-of select="Rating"/>
          </xsl:attribute>
          <xsl:attribute name="alt">
            <xsl:call-template name="term2020" />
            <xsl:text>:&#160;</xsl:text>
            <xsl:value-of select="Rating"/>
            <xsl:text>&#160;</xsl:text>
            <xsl:call-template name="term2021" />
          </xsl:attribute>
        </span>
      </span>

      <xsl:apply-templates select="Path/node()" mode="displayThumbnail"/>
      <xsl:call-template name="term2018" />
      <xsl:text>&#160;</xsl:text>
      <span class="reviewer">
        <xsl:value-of select="Reviewer"/>
      </span>
      <xsl:text>&#160;</xsl:text>
      <xsl:call-template name="term2019" />
      <xsl:text>&#160;</xsl:text>
      <!--<span class="dtreviewed">
        <xsl:call-template name="DisplayDate">
          <xsl:with-param name="date" select="ReviewDate/node()"/>
        </xsl:call-template>
        <span class="value-title">
          <xsl:attribute name="title">
            <xsl:value-of select="ReviewDate/node()"/>
          </xsl:attribute>
        </span>
      </span>-->
      <span class="summary">
        <xsl:apply-templates select="Summary" mode="cleanXhtml"/>
      </span>
      <span class="description">
        <xsl:apply-templates select="Description/node()" mode="cleanXhtml"/>
      </span>
      <div class="entryFooter">
        <div class="tags">
          <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
          <xsl:text> </xsl:text>
        </div>
        <xsl:apply-templates select="." mode="backLink">
          <xsl:with-param name="link" select="/Page/Request/ServerVariables/Item[@name='PREVIOUS_PAGE']/node()"/>
        </xsl:apply-templates>
      </div>
      <!-- Terminus class fix to floating columns -->
      <div class="terminus">&#160;</div>
    </div>
  </xsl:template>

  <!--  =====================================================================================  -->
  <!--  ==  Social Bookmarks  ===============================================================  -->
  <!--  =====================================================================================  -->

  <!-- module -->
  <xsl:template match="Content[@moduleType='SocialBookmarks']" mode="displayBrief">
    <div class="moduleBookmarks">
      <xsl:variable name="bookmarkSettings">
        <xsl:choose>
          <xsl:when test="@bookmarkSettings='global' and $page/Contents/Content[@type='SocialNetworkingSettings']">
            <xsl:copy-of select="$page/Contents/Content[@type='SocialNetworkingSettings']/Bookmarks"/>
          </xsl:when>
          <xsl:when test="@bookmarkSettings='this' and Content[@type='SocialNetworkingSettings']">
            <xsl:copy-of select="Content[@type='SocialNetworkingSettings']/Bookmarks"/>
          </xsl:when>
          <xsl:otherwise>default</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:apply-templates select="." mode="displayBookmarks">
        <xsl:with-param name="bookmarkSettings" select="ms:node-set($bookmarkSettings)/Bookmarks" />
        <xsl:with-param name="type" select="'MenuItem'" />
      </xsl:apply-templates>
    </div>
  </xsl:template>

  <!-- page -->
  <xsl:template match="Page" mode="socialBookmarks">
    <!-- if bookmarks enabled & NOT set to use modules to do it -->
    <xsl:if test="$page/Contents/Content[@type='SocialNetworkingSettings' and Bookmarks/MenuItem/@position!='module']">
      <xsl:variable name="bookmarkSettings" select="$page/Contents/Content[@type='SocialNetworkingSettings']/Bookmarks" />
      <!-- uses this span to re-write around the page -->
      <span>
        <xsl:attribute name="class">
          <xsl:text>bookmarkPlacement </xsl:text>
          <xsl:value-of select="$bookmarkSettings/MenuItem/@position"/>
        </xsl:attribute>
        <xsl:apply-templates select="." mode="displayBookmarks">
          <xsl:with-param name="bookmarkSettings" select="$bookmarkSettings" />
          <xsl:with-param name="type" select="'MenuItem'" />
        </xsl:apply-templates>
      </span>
      <div class="terminus">&#160;</div>
    </xsl:if>
  </xsl:template>

  <!-- content -->
  <xsl:template match="Content" mode="socialBookmarks">
    <!-- if bookmarks enabled & NOT set to use modules to do it -->
    <xsl:if test="$page/Contents/Content[@type='SocialNetworkingSettings']">
      <xsl:variable name="bookmarkSettings" select="$page/Contents/Content[@type='SocialNetworkingSettings']/Bookmarks" />
      <!-- uses this span to re-write around the page -->
      <div>
        <xsl:attribute name="class">
          <xsl:text>bookmarkPlacement </xsl:text>
          <xsl:value-of select="$bookmarkSettings/Content/@position"/>
        </xsl:attribute>
        <xsl:apply-templates select="." mode="displayBookmarks">
          <xsl:with-param name="bookmarkSettings" select="$bookmarkSettings" />
          <xsl:with-param name="type" select="'Content'" />
        </xsl:apply-templates>
      </div>
      <div class="terminus">&#160;</div>
    </xsl:if>
  </xsl:template>

  <!-- display bookmarks -->
  <xsl:template match="*" mode="displayBookmarks">
    <xsl:param name="bookmarkSettings" />
    <xsl:param name="type" select="'MenuItem'"/>
    <xsl:variable name="layout">
      <xsl:value-of select="$bookmarkSettings/*[name()=$type]/@size" />
      <xsl:if test="$bookmarkSettings/*[name()=$type]/@size='standard' and $bookmarkSettings/*[name()=$type]/@count='true'">
        <xsl:text>-count</xsl:text>
      </xsl:if>
    </xsl:variable>
    <div class="socialBookmarks">
      <xsl:attribute name="class">
        <xsl:text>socialBookmarks bookmarks-</xsl:text>
        <xsl:value-of select="$layout" />
      </xsl:attribute>
      <xsl:apply-templates select="$page/Contents/Content[@type='SocialNetworkingSettings']" mode="inlinePopupOptions">
        <xsl:with-param name="class">
          <xsl:text>socialBookmarks bookmarks-</xsl:text>
          <xsl:value-of select="$layout" />
        </xsl:with-param>
      </xsl:apply-templates>
      <xsl:apply-templates select="$bookmarkSettings/Methods/@*[.='true']" mode="displayBookmark">
        <xsl:sort select="name()" order="descending" data-type="text"/>
        <xsl:with-param name="bookmarkSettings" select="$bookmarkSettings/*[name()=$type]" />
      </xsl:apply-templates>
      <xsl:text> </xsl:text>
    </div>
    <div class="terminus">&#160;</div>
  </xsl:template>

  <!-- Generic catch -->
  <xsl:template match="@*" mode="displayBookmark"></xsl:template>

  <!-- Facebook LIKE -->
  <xsl:template match="@facebook" mode="displayBookmark">
    <xsl:param name="bookmarkSettings" />
    <xsl:variable name="layout" select="$bookmarkSettings/@size"/>
    <span class="bookmark facebook-bookmark">
      <div class="fb-like" data-show-faces="false">
        <xsl:attribute name="data-width">
          <xsl:choose>
            <xsl:when test="$layout='large' or ($layout='standard' and $bookmarkSettings/@count='true')">
              <xsl:text>70</xsl:text>
            </xsl:when>
            <xsl:otherwise>
              <xsl:text>450</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <xsl:attribute name="data-href">
          <xsl:apply-templates select="." mode="getBookmarkURL"/>
        </xsl:attribute>
        <xsl:attribute name="data-layout">
          <xsl:choose>
            <xsl:when test="$layout='large'">box_count</xsl:when>
            <xsl:when test="$layout='standard' and $bookmarkSettings/@count='true'">button_count</xsl:when>
            <xsl:otherwise>standard</xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <xsl:if test="$layout!='large' and $bookmarkSettings/@count!='true'">
          <xsl:attribute name="data-send">false</xsl:attribute>
        </xsl:if>
        <xsl:if test="parent::Methods/@facebookShare='true'">
          <xsl:attribute name="data-share">
            <xsl:text>true</xsl:text>
          </xsl:attribute>
        </xsl:if>
        <!-- needed to keep tag open -->
        <xsl:text> </xsl:text>
      </div>
      <noscript>
        <a target="_blank">
          <xsl:attribute name="href">
            <xsl:text>http://www.facebook.com/sharer.php?u=</xsl:text>
            <xsl:apply-templates select="." mode="getBookmarkURL"/>
            <xsl:text>&amp;t=</xsl:text>
            <xsl:apply-templates select="." mode="getBookmarkTitle"/>
          </xsl:attribute>
          <img src="/ewcommon/images/social-bookmarks/facebook.gif" alt="Facebook" width="44" height="20"/>
        </a>
      </noscript>
    </span>
  </xsl:template>

  <!-- GOOGLE +1 -->
  <xsl:template match="@google" mode="displayBookmark">
    <xsl:param name="bookmarkSettings" />
    <xsl:variable name="layout" select="$bookmarkSettings/@size"/>
    <span class="bookmark google-bookmark">
      <!-- check footer JS for where +1 kicks off. -->
      <div class="g-plusone">
        <xsl:attribute name="data-size">
          <xsl:choose>
            <xsl:when test="$layout='large'">tall</xsl:when>
            <xsl:when test="$layout='standard' and $bookmarkSettings/@count='true'">medium</xsl:when>
            <xsl:otherwise>standard</xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <xsl:attribute name="count">
          <xsl:value-of select="$bookmarkSettings/@count"/>
        </xsl:attribute>
        <xsl:text> </xsl:text>
      </div>
      <noscript>
        <a target="_blank">
          <xsl:attribute name="href">
            <xsl:text>https://plusone.google.com/_/+1/confirm?hl=en&amp;url=</xsl:text>
            <xsl:apply-templates select="." mode="getBookmarkURL"/>
          </xsl:attribute>
          <img src="/ewcommon/images/social-bookmarks/google.gif" alt="Google +1" width="50" height="20"/>
        </a>
      </noscript>
    </span>
  </xsl:template>

  <!-- Twitter Tweek -->
  <xsl:template match="@twitter" mode="displayBookmark">
    <xsl:param name="bookmarkSettings" />
    <xsl:variable name="layout" select="$bookmarkSettings/@size"/>
    <span class="bookmark twitter-bookmark">
      <a href="http://twitter.com/share" class="twitter-share-button" target="_blank">
        <xsl:attribute name="data-url">
          <xsl:apply-templates select="." mode="getBookmarkURL" />
        </xsl:attribute>
        <xsl:attribute name="data-text">
          <xsl:apply-templates select="." mode="getBookmarkTitle" />
          <xsl:text> -</xsl:text>
        </xsl:attribute>
        <xsl:attribute name="data-count">
          <xsl:choose>
            <xsl:when test="$layout='large'">vertical</xsl:when>
            <xsl:when test="$bookmarkSettings/@count='true'">horizontal</xsl:when>
            <xsl:otherwise>none</xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <img src="/ewcommon/images/social-bookmarks/twitter.gif" alt="Twitter" width="55" height="20"/>
      </a>
    </span>
  </xsl:template>

  <!-- LinkedIn -->
  <xsl:template match="@linkedin" mode="displayBookmark">
    <xsl:param name="bookmarkSettings" />
    <xsl:variable name="layout" select="$bookmarkSettings/@size"/>
    <span class="bookmark linkedin-bookmark">
      <script type="in/share">
        <xsl:attribute name="data-url">
          <xsl:apply-templates select="." mode="getBookmarkURL" />
        </xsl:attribute>
        <xsl:attribute name="data-counter">
          <xsl:choose>
            <xsl:when test="$layout='large'">top</xsl:when>
            <xsl:when test="$bookmarkSettings/@count='true'">right</xsl:when>
            <xsl:otherwise></xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
        <xsl:text> </xsl:text>
      </script>
      <noscript>
        <a target="_blank">
          <xsl:attribute name="href">
            <xsl:text>http://www.linkedin.com/shareArticle?mini=true&amp;url=</xsl:text>
            <xsl:apply-templates select="." mode="getBookmarkURL"/>
            <xsl:text>&amp;title=</xsl:text>
            <xsl:apply-templates select="." mode="getBookmarkTitle"/>
          </xsl:attribute>
          <img src="/ewcommon/images/social-bookmarks/linkedin.gif" alt="LinkedIn" width="61" height="20"/>
        </a>
      </noscript>
    </span>
  </xsl:template>

  <!-- pinterest -->
  <xsl:template match="@pinterest" mode="displayBookmark">
    <xsl:param name="bookmarkSettings" />
    <xsl:variable name="layout" select="$bookmarkSettings/@size"/>
    <xsl:variable name="url">
      <xsl:apply-templates select="." mode="getBookmarkURL"/>
    </xsl:variable>
    <xsl:variable name="media">
      <xsl:text>http</xsl:text>
      <xsl:if test="$page/Request/ServerVariables/Item[@name='HTTPS']='on'">s</xsl:if>
      <xsl:text>://</xsl:text>
      <xsl:value-of select="$page/Request/ServerVariables/Item[@name='SERVER_NAME']"/>
      <xsl:for-each select="$page/ContentDetail/Content">
        <xsl:choose>
          <!-- IF Thumbnail use that -->
          <xsl:when test="Images/img[@class='thumbnail']/@src!=''">
            <xsl:value-of select="Images/img[@class='thumbnail']/@src"/>
          </xsl:when>
          <!-- IF Full Size use that -->
          <xsl:when test="Images/img[@class='detail']/@src!=''">
            <xsl:value-of select="Images/img[@class='detail']/@src"/>
          </xsl:when>
          <!-- ELSE use display -->
          <xsl:otherwise>
            <xsl:value-of select="Images/img[@class='display']/@src"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="description">
      <xsl:for-each select="$page/ContentDetail/Content">
        <xsl:choose>
          <!-- IF Thumbnail use that -->
          <xsl:when test="Images/img[@class='thumbnail']/@alt!=''">
            <xsl:value-of select="Images/img[@class='thumbnail']/@alt"/>
          </xsl:when>
          <!-- IF Full Size use that -->
          <xsl:when test="Images/img[@class='detail']/@alt!=''">
            <xsl:value-of select="Images/img[@class='detail']/@alt"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="@name"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="href">
      <xsl:text>http://pinterest.com/pin/create/button/?url=</xsl:text>
      <xsl:value-of select="$url"/>
      <xsl:text>&amp;media=</xsl:text>
      <xsl:value-of select="$media"/>
      <xsl:text>&amp;description=</xsl:text>
      <xsl:value-of select="$description"/>
    </xsl:variable>
    <xsl:variable name="data-count">
      <xsl:choose>
        <xsl:when test="$layout='large'">vertical</xsl:when>
        <xsl:when test="$bookmarkSettings/@count='true'">horizontal</xsl:when>
        <xsl:otherwise>none</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <span class="bookmark pinterest-bookmark">
      <a href="{$href}" class="pin-it-button" count-layout="{$data-count}">
        <img border="0" src="//assets.pinterest.com/images/PinExt.png" title="Pin It" />
      </a>
    </span>
  </xsl:template>

  <!-- All the Social JS files -->
  <xsl:template name="initialiseSocialBookmarks">
    <xsl:if test="not(/Page/Cart) or not(/Page/Cart/Order/@cmd!='')">
      <!-- facebook -->
      <xsl:variable name="fbAppId">
        <xsl:choose>
          <xsl:when test="$page/Contents/Content[@name='fb-app_id']">
            <xsl:value-of select="$page/Contents/Content[@name='fb-app_id']"/>
          </xsl:when>
          <xsl:otherwise>176558699067891</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <div id="fb-root">
        <xsl:text> </xsl:text>
      </div>
      <script>
        (function(d, s, id) {
        var js, fjs = d.getElementsByTagName(s)[0];
        if (d.getElementById(id)) return;
        js = d.createElement(s); js.id = id;
        js.src = "//connect.facebook.net/en_GB/all.js#xfbml=1&amp;appId=<xsl:value-of select="$fbAppId"/>";
        fjs.parentNode.insertBefore(js, fjs);
        }(document, 'script', 'facebook-jssdk'));
      </script>
      <!-- google -->
      <!-- Place this tag where you want the +1 button to render. -->
      <!-- Place this tag after the last +1 button tag. -->
      <script type="text/javascript">
        <xsl:text>(function() {</xsl:text>
        <xsl:text>var po = document.createElement('script'); po.type = 'text/javascript'; po.async = true;</xsl:text>
        <xsl:text>po.src = 'https://apis.google.com/js/plusone.js';</xsl:text>
        <xsl:text>var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(po, s);</xsl:text>
        <xsl:text>})();</xsl:text>
      </script>
      <!-- Twitter -->
      <script type="text/javascript" src="//platform.twitter.com/widgets.js">&#160;</script>
      <!-- LinkedIn -->
      <script type="text/javascript" src="//platform.linkedin.com/in.js">&#160;</script>
      <!-- Pinterest -->
      <script type="text/javascript" src="//assets.pinterest.com/js/pinit.js">&#160;</script>
    </xsl:if>
  </xsl:template>

  <xsl:template match="@*" mode="getBookmarkURL">
    <xsl:if test="$siteURL=''">
      <xsl:text>http</xsl:text>
      <xsl:if test="$page/Request/ServerVariables/Item[@name='HTTPS']='on'">s</xsl:if>
      <xsl:text>://</xsl:text>
      <xsl:value-of select="$page/Request/ServerVariables/Item[@name='SERVER_NAME']"/>
    </xsl:if>
    <xsl:choose>
      <xsl:when test="$page/ContentDetail">
        <xsl:apply-templates select="$page/ContentDetail/Content" mode="getHref" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="$currentPage" mode="getHref" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="@*" mode="getBookmarkTitle">
    <xsl:choose>
      <xsl:when test="$page/ContentDetail">
        <xsl:apply-templates select="$page/ContentDetail/Content" mode="getDisplayName" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="$currentPage" mode="getDisplayName" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="Content[@moduleType='SocialLinks']" mode="displayBrief">
    <div class="moduleSocialLinks align-{@align}">
      <xsl:choose>
        <xsl:when test="@blank='true'">
          <xsl:apply-templates select="." mode="socialLinksBlank">
            <xsl:with-param name="iconSet" select="@iconSet"/>
            <xsl:with-param name="myName" select="@myName"/>
          </xsl:apply-templates>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="." mode="socialLinks">
            <xsl:with-param name="iconSet" select="@iconSet"/>
            <xsl:with-param name="myName" select="@myName"/>
          </xsl:apply-templates>
        </xsl:otherwise>
      </xsl:choose>
    </div>
  </xsl:template>

  <!-- module -->
  <xsl:template match="Content | ContactPoint" mode="socialLinksBlank">
    <xsl:param name="myName"/>
    <xsl:param name="iconSet"/>
    <div class="socialLinks clearfix iconset-{$iconSet}">
      <xsl:choose>
        <xsl:when test="@uploadSprite!=''">
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" target="_blank" title="{$myName} on Facebook" id="social-id-fb" style="background-image:url({@uploadSprite})" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" target="_blank" title="{$myName} on Twitter" id="social-id-tw" style="background-image:url({@uploadSprite});background-position:128px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" target="_blank" title="{$myName} on LinkedIn" id="social-id-li" style="background-image:url({@uploadSprite});background-position:96px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" target="_blank" title="{$myName} on Google+" id="social-id-gp" style="background-image:url({@uploadSprite});background-position:64px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" target="_blank" title="{$myName} on Pinterest" id="social-id-pi" style="background-image:url({@uploadSprite});background-position:32px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" target="_blank" title="{$myName} on You Tube" id="social-id-yt" style="background-image:url({@uploadSprite});background-position:160px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" target="_blank" title="{$myName} on Instagram" id="social-id-ig" style="background-image:url({@uploadSprite});background-position:192px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$iconSet='icons'">
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" target="_blank" title="{$myName} on Facebook" id="social-id-fb">
              <i class="fa fa-2x fa-facebook">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" target="_blank" title="{$myName} on Twitter" id="social-id-tw">
              <i class="fa fa-2x fa-twitter">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" target="_blank" title="{$myName} on LinkedIn" id="social-id-li">
              <i class="fa fa-2x fa-linkedin">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" target="_blank" title="{$myName} on Google+" id="social-id-gp">
              <i class="fa fa-2x fa-google-plus">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" target="_blank" title="{$myName} on Pinterest" id="social-id-li">
              <i class="fa fa-2x fa-pinterest">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" target="_blank" title="{$myName} on Youtube" id="social-id-yt">
              <i class="fa fa-2x fa-youtube">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" target="_blank" title="{$myName} on Instagram" id="social-id-ig">
              <i class="fa fa-2x fa-instagram">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$iconSet='icons-square'">
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" target="_blank" title="{$myName} on Facebook" id="social-id-fb">
              <i class="fa fa-3x fa-facebook-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" target="_blank" title="{$myName} on Twitter" id="social-id-tw">
              <i class="fa fa-3x fa-twitter-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" target="_blank" title="{$myName} on LinkedIn" id="social-id-li">
              <i class="fa fa-3x fa-linkedin-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" target="_blank" title="{$myName} on Google+" id="social-id-gp">
              <i class="fa fa-3x fa-google-plus-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" target="_blank" title="{$myName} on Pinterest" id="social-id-pi">
              <i class="fa fa-3x fa-pinterest-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" target="_blank" title="{$myName} on Youtube" id="social-id-yt">
              <i class="fa fa-3x fa-youtube-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" target="_blank" title="{$myName} on Instagram" id="social-id-ig">
              <i class="fa fa-3x fa-instagram">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$iconSet='icons-circle'">
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" target="_blank" title="{$myName} on Facebook" id="social-id-fb">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fa fa-facebook fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" target="_blank" title="{$myName} on Twitter" id="social-id-tw">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fa fa-twitter fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" target="_blank" title="{$myName} on LinkedIn" id="social-id-li">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fa fa-linkedin fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" target="_blank" title="{$myName} on Google+" id="social-id-gp">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fa fa-google-plus fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" target="_blank" title="{$myName} on Pinterest" id="social-id-pi">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fa fa-pinterest fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" target="_blank" title="{$myName} on Youtube" id="social-id-yt">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fa fa-youtube fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" target="_blank" title="{$myName} on Instagram" id="social-id-ig">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fa fa-instagram fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$iconSet='plain'">
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" target="_blank" title="{$myName} on Facebook" class="social-id-fb">
              <i class="fa fa-facebook">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" target="_blank" title="{$myName} on Twitter" class="social-id-tw">
              <i class="fa fa-twitter">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" target="_blank" title="{$myName} on LinkedIn" class="social-id-li">
              <i class="fa fa-linkedin">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" target="_blank" title="{$myName} on Google+" class="social-id-gp">
              <i class="fa fa-google-plus">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" target="_blank" title="{$myName} on Pinterest" class="social-id-pi">
              <i class="fa fa-pinterest">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" target="_blank" title="{$myName} on Youtube" class="social-id-yt">
              <i class="fa fa-youtube ">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" target="_blank" title="{$myName} on Instagram" id="social-id-ig">
              <i class="fa fa-instagram">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
        </xsl:when>
        <xsl:otherwise>
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" target="_blank" title="{$myName} on Facebook" id="social-id-fb">
              <img src="/ewcommon/images/icons/social/{$iconSet}/facebook.png" alt="{$myName} on Facebook" title="Follow {$myName} on Facebook" />
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" target="_blank" title="{$myName} on Twitter" id="social-id-tw">
              <img src="/ewcommon/images/icons/social/{$iconSet}/twitter.png" alt="{$myName} on Twitter" title="Follow {$myName} on Twitter" />
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" target="_blank" title="{$myName} on LinkedIn" id="social-id-li">
              <img src="/ewcommon/images/icons/social/{$iconSet}/LinkedIn.png" alt="{$myName} on LinkedIn" title="Follow {$myName} on LinkedIn" />
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" target="_blank" title="{$myName} on Google+" id="social-id-gp">
              <img src="/ewcommon/images/icons/social/{$iconSet}/Googleplus.png" alt="{$myName} on Google+" title="Follow {$myName} on Google+" />
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" target="_blank" title="{$myName} on Pinterest" id="social-id-li">
              <img src="/ewcommon/images/icons/social/{$iconSet}/Pinterest.png" alt="{$myName} on Pinterest" title="Follow {$myName} on Pinterest" />
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" target="_blank" title="{$myName} on YouTube" id="social-id-yt">
              <img src="/ewcommon/images/icons/social/{$iconSet}/YouTube.png" alt="{$myName} on YouTube" title="Follow {$myName} on YouTube" />
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" target="_blank" title="{$myName} on Pinterest" id="social-id-ig">
              <img src="/ewcommon/images/icons/social/{$iconSet}/Instagram.png" alt="{$myName} on Instagram" title="Follow {$myName} on Instagram" />
            </a>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>
    </div>
  </xsl:template>
  <!-- module -->
  <xsl:template match="Content | ContactPoint" mode="socialLinks">
    <xsl:param name="myName"/>
    <xsl:param name="iconSet"/>
    <div class="socialLinks clearfix iconset-{$iconSet}">
      <xsl:choose>
        <xsl:when test="@uploadSprite!=''">
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" title="{$myName} on Facebook" id="social-id-fb" style="background-image:url({@uploadSprite})" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" title="{$myName} on Twitter" id="social-id-tw" style="background-image:url({@uploadSprite});background-position:128px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" title="{$myName} on LinkedIn" id="social-id-li" style="background-image:url({@uploadSprite});background-position:96px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" title="{$myName} on Google+" id="social-id-gp" style="background-image:url({@uploadSprite});background-position:64px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" title="{$myName} on Pinterest" id="social-id-pi" style="background-image:url({@uploadSprite});background-position:32px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" title="{$myName} on You Tube" id="social-id-yt" style="background-image:url({@uploadSprite});background-position:160px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" title="{$myName} on Instagram" id="social-id-ig" style="background-image:url({@uploadSprite});background-position:192px 0" class="social-sprite">
              &#160;
            </a>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$iconSet='icons'">
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" title="{$myName} on Facebook" id="social-id-fb">
              <i class="fa fa-2x fa-facebook">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" title="{$myName} on Twitter" id="social-id-tw">
              <i class="fa fa-2x fa-twitter">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" title="{$myName} on LinkedIn" id="social-id-li">
              <i class="fa fa-2x fa-linkedin">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" title="{$myName} on Google+" id="social-id-gp">
              <i class="fa fa-2x fa-google-plus">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" title="{$myName} on Pinterest" id="social-id-li">
              <i class="fa fa-2x fa-pinterest">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" title="{$myName} on Youtube" id="social-id-yt">
              <i class="fa fa-2x fa-youtube">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" title="{$myName} on Instagram" id="social-id-ig">
              <i class="fa fa-2x fa-instagram">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$iconSet='icons-square'">
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" title="{$myName} on Facebook" id="social-id-fb">
              <i class="fa fa-3x fa-facebook-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" title="{$myName} on Twitter" id="social-id-tw">
              <i class="fa fa-3x fa-twitter-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" title="{$myName} on LinkedIn" id="social-id-li">
              <i class="fa fa-3x fa-linkedin-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" title="{$myName} on Google+" id="social-id-gp">
              <i class="fa fa-3x fa-google-plus-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" title="{$myName} on Pinterest" id="social-id-pi">
              <i class="fa fa-3x fa-pinterest-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" title="{$myName} on Youtube" id="social-id-yt">
              <i class="fa fa-3x fa-youtube-square">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" title="{$myName} on Instagram" id="social-id-ig">
              <i class="fa fa-3x fa-instagram">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@spotifyURL!=''">
            <a href="{@spotifyURL}" title="{$myName} on Spotify" id="social-id-ig">
              <i class="fa fa-3x fa-spotify">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$iconSet='icons-circle'">
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" title="{$myName} on Facebook" id="social-id-fb">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fa fa-facebook fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" title="{$myName} on Twitter" id="social-id-tw">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fa fa-twitter fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" title="{$myName} on LinkedIn" id="social-id-li">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fa fa-linkedin fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" title="{$myName} on Google+" id="social-id-gp">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fa fa-google-plus fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" title="{$myName} on Pinterest" id="social-id-pi">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fa fa-pinterest fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" title="{$myName} on Youtube" id="social-id-yt">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fa fa-youtube fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" title="{$myName} on Instagram" id="social-id-ig">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fa fa-instagram fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
          <xsl:if test="@spotifyURL!=''">
            <a href="{@spotifyURL}" title="{$myName} on Spotify" id="social-id-ig">
              <span class="fa-stack fa-lg">
                <i class="fa fa-circle fa-stack-2x">
                  <xsl:text> </xsl:text>
                </i>
                <i class="fa fa-spotify fa-stack-1x fa-inverse">
                  <xsl:text> </xsl:text>
                </i>
              </span>
            </a>
          </xsl:if>
        </xsl:when>
        <xsl:when test="$iconSet='plain'">
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" title="{$myName} on Facebook" class="social-id-fb">
              <i class="fa fa-facebook">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" title="{$myName} on Twitter" class="social-id-tw">
              <i class="fa fa-twitter">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" title="{$myName} on LinkedIn" class="social-id-li">
              <i class="fa fa-linkedin">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" title="{$myName} on Google+" class="social-id-gp">
              <i class="fa fa-google-plus">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" title="{$myName} on Pinterest" class="social-id-pi">
              <i class="fa fa-pinterest">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" title="{$myName} on Youtube" class="social-id-yt">
              <i class="fa fa-youtube ">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" title="{$myName} on Instagram" id="social-id-ig">
              <i class="fa fa-instagram">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
          <xsl:if test="@spotifyURL!=''">
            <a href="{@spotifyURL}" title="{$myName} on Spotify" id="social-id-ig">
              <i class="fa fa-spotify">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </xsl:if>
        </xsl:when>
        <xsl:otherwise>
          <xsl:if test="@facebookURL!=''">
            <a href="{@facebookURL}" title="{$myName} on Facebook" id="social-id-fb">
              <img src="/ewcommon/images/icons/social/{$iconSet}/facebook.png" alt="{$myName} on Facebook" title="Follow {$myName} on Facebook" />
            </a>
          </xsl:if>
          <xsl:if test="@twitterURL!=''">
            <a href="{@twitterURL}" title="{$myName} on Twitter" id="social-id-tw">
              <img src="/ewcommon/images/icons/social/{$iconSet}/twitter.png" alt="{$myName} on Twitter" title="Follow {$myName} on Twitter" />
            </a>
          </xsl:if>
          <xsl:if test="@linkedInURL!=''">
            <a href="{@linkedInURL}" title="{$myName} on LinkedIn" id="social-id-li">
              <img src="/ewcommon/images/icons/social/{$iconSet}/LinkedIn.png" alt="{$myName} on LinkedIn" title="Follow {$myName} on LinkedIn" />
            </a>
          </xsl:if>
          <xsl:if test="@googlePlusURL!=''">
            <a href="{@googlePlusURL}" title="{$myName} on Google+" id="social-id-gp">
              <img src="/ewcommon/images/icons/social/{$iconSet}/Googleplus.png" alt="{$myName} on Google+" title="Follow {$myName} on Google+" />
            </a>
          </xsl:if>
          <xsl:if test="@pinterestURL!=''">
            <a href="{@pinterestURL}" title="{$myName} on Pinterest" id="social-id-li">
              <img src="/ewcommon/images/icons/social/{$iconSet}/Pinterest.png" alt="{$myName} on Pinterest" title="Follow {$myName} on Pinterest" />
            </a>
          </xsl:if>
          <xsl:if test="@youtubeURL!=''">
            <a href="{@youtubeURL}" title="{$myName} on YouTube" id="social-id-yt">
              <img src="/ewcommon/images/icons/social/{$iconSet}/YouTube.png" alt="{$myName} on YouTube" title="Follow {$myName} on YouTube" />
            </a>
          </xsl:if>
          <xsl:if test="@instagramURL!=''">
            <a href="{@instagramURL}" title="{$myName} on Pinterest" id="social-id-ig">
              <img src="/ewcommon/images/icons/social/{$iconSet}/Instagram.png" alt="{$myName} on Instagram" title="Follow {$myName} on Instagram" />
            </a>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>
    </div>
  </xsl:template>


  


  <!-- Tag List Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='TagList']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!--responsive columns variables-->
    <xsl:variable name="xsColsToShow">
      <xsl:choose>
        <xsl:when test="@xsCol='2'">2</xsl:when>
        <xsl:otherwise>1</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="smColsToShow">
      <xsl:choose>
        <xsl:when test="@smCol and @smCol!=''">
          <xsl:value-of select="@smCol"/>
        </xsl:when>
        <xsl:otherwise>2</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="mdColsToShow">
      <xsl:choose>
        <xsl:when test="@mdCol and @mdCol!=''">
          <xsl:value-of select="@mdCol"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@cols"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!--end responsive columns variables-->
    <!-- Output Module -->
    <div class="TagsList">

      <div class="cols{@cols}">
        <!--responsive columns-->
        <xsl:attribute name="class">
          <xsl:text>cols</xsl:text>
          <xsl:choose>
            <xsl:when test="@xsCol='2'"> mobile-2-col-content</xsl:when>
            <xsl:otherwise> mobile-1-col-content</xsl:otherwise>
          </xsl:choose>
          <xsl:if test="@smCol and @smCol!=''">
            <xsl:text> sm-content-</xsl:text>
            <xsl:value-of select="@smCol"/>
          </xsl:if>
          <xsl:if test="@mdCol and @mdCol!=''">
            <xsl:text> md-content-</xsl:text>
            <xsl:value-of select="@mdCol"/>
          </xsl:if>
          <xsl:text> cols</xsl:text>
          <xsl:value-of select="@cols"/>
          <xsl:if test="@mdCol and @mdCol!=''">
            <xsl:text> content-cols-responsive</xsl:text>
          </xsl:if>
        </xsl:attribute>
        <!--end responsive columns-->
        <!-- If Stepper, display Stepper -->
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="articleList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <ul>
          <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefListItem">
            <xsl:with-param name="sortBy" select="@sortBy"/>
          </xsl:apply-templates>
        </ul>
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- Tags List Brief -->
  <xsl:template match="Content[@type='Tag']" mode="displayBriefListItem">
    <xsl:param name="sortBy"/>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <xsl:variable name="name" select="Name/node()"/>
    <li>
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <a href="{$parentURL}" rel="tag">
        <xsl:apply-templates select="Name" mode="displayBrief"/>
        <xsl:if test="@relatedCount!=''">
          (<xsl:value-of select="@relatedCount"/>)
        </xsl:if>
      </a>
    </li>
  </xsl:template>

  <!-- Tag Cloud Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='TagCloud']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <!-- Output Module -->
    <div class="TagsCloud">
      <div class="cols{@cols}">
        <!-- If Stepper, display Stepper -->
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="articleList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <div id="tagcloud">
          <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefCloudItem">
            <xsl:with-param name="sortBy" select="@sortBy"/>
          </xsl:apply-templates>
        </div>
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- Tags Cloud Brief -->
  <xsl:template match="Content[@type='Tag']" mode="displayBriefCloudItem">
    <xsl:param name="sortBy"/>
    <xsl:variable name="name" select="Name/node()"/>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <xsl:variable name="articleCount" select="count($page/Contents/Content[@type='NewsArticle']/Content[@type='Tag' and Name/node() = $name])"/>
    <xsl:variable name="totalArticleCount" select="count($page/Contents/Content/Content[@type='NewsArticle'])"/>
    <xsl:variable name="tagCloudCount" select="round((($articleCount div $totalArticleCount) * 100)) div 10" />
    <a href="{$parentURL}" rel="tag" class="tag tag{$tagCloudCount}">
      <xsl:apply-templates select="Name" mode="displayBrief"/>
      <xsl:text>&#160;</xsl:text>
    </a>
  </xsl:template>

  <!--Module:Eonic:TwitterProfile:Start-->
  <xsl:template match="Content[@type='Module' and @moduleType='TwitterProfile']" mode="displayBrief">
    <div class="twitterProfile">
      <a class="twitter-timeline"  href="https://twitter.com/{username/node()}">
        Tweets by @<xsl:value-of select="username/node()"/>
      </a>
      <script>!function(d,s,id){var js,fjs=d.getElementsByTagName(s)[0],p=/^http:/.test(d.location)?'http':'https';if(!d.getElementById(id)){js=d.createElement(s);js.id=id;js.src=p+"://platform.twitter.com/widgets.js";fjs.parentNode.insertBefore(js,fjs);}}(document,"script","twitter-wjs");</script>
    </div>
  </xsl:template>
  <!--Module:Eonic:TwitterProfile:End-->

  <!--Module:Eonic:FacebookLikeBox:Start-->
  <xsl:template match="Content[@type='Module' and @moduleType='FacebookLikeBox']" mode="displayBrief">
    <div class="FacebookLikeBox">
      <div id="fb-root">
        <xsl:text> </xsl:text>
      </div>
      <script>
        (function(d, s, id) {
        var js, fjs = d.getElementsByTagName(s)[0];
        if (d.getElementById(id)) return;
        js = d.createElement(s); js.id = id;
        js.src = "//connect.facebook.net/en_GB/all.js#xfbml=1&amp;appId=<xsl:value-of select="appId/node()"/>";
        fjs.parentNode.insertBefore(js, fjs);
        }(document, 'script', 'facebook-jssdk'));
      </script>
      <div class="fb-like-box" data-href="{href/node()}" data-width="{width/node()}" data-height="{height/node()}" data-border-color="{border_colour/node()}">
        <xsl:if test="show_faces/node()='true'">
          <xsl:attribute name="data-show-faces">true</xsl:attribute>
        </xsl:if>
        <xsl:if test="stream/node()='true'">
          <xsl:attribute name="data-stream">true</xsl:attribute>
        </xsl:if>
        <xsl:if test="not(stream/node())">
          <xsl:attribute name="data-stream">false</xsl:attribute>
        </xsl:if>
        <xsl:if test="color_scheme/node()='dark'">
          <xsl:attribute name="data-colorscheme">dark</xsl:attribute>
        </xsl:if>
        <xsl:if test="force_wall/node()='true'">
          <xsl:attribute name="data-forcewall">true</xsl:attribute>
        </xsl:if>
        <xsl:if test="not(force_wall/node())">
          <xsl:attribute name="data-forcewall">false</xsl:attribute>
        </xsl:if>
        <xsl:if test="header/node()='true'">
          <xsl:attribute name="data-header">true</xsl:attribute>
        </xsl:if>
        <xsl:if test="not(header/node())">
          <xsl:attribute name="data-header">false</xsl:attribute>
        </xsl:if>
        <xsl:text> </xsl:text>
      </div>
    </div>
  </xsl:template>
  <!--Module:Eonic:FacebookLikeBox:End-->

  <!--Module:Eonic:GooglePlusBadge:Start-->
  <xsl:template match="Content[@type='Module' and @moduleType='GooglePlusBadge']" mode="displayBrief">
    <div class="GooglePlusBadge">
      <!-- Place this tag where you want the badge to render. -->
      <div class="g-plus" data-href="https://plus.google.com/{GooglePlusId/node()}" data-rel="publisher" data-theme="light"></div>

      <!-- Place this tag after the last badge tag. -->
      <script type="text/javascript">
        window.___gcfg = {lang: 'en-GB'};

        (function() {
        var po = document.createElement('script'); po.type = 'text/javascript'; po.async = true;
        po.src = 'https://apis.google.com/js/plusone.js';
        var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(po, s);
        })();
      </script>
    </div>
  </xsl:template>

  <!--Module:Eonic:GooglePlusBadge:End-->

  <xsl:template match="Content[@moduleType='Audio']" mode="displayBrief">
    <div class="audio">
      <!--  JPlayer  -->
      <xsl:if test="Path/node()!=''">
        <div class="jp-type-single">
          <div id="jquery_jplayer_{@id}" class="jp-jplayer">&#160;</div>
          <div id="jp_interface_{@id}" class="jp-interface">
            <ul class="jp-controls nav navbar-nav">
              <li>
                <a href="#" class="jp-play btn btn-success navbar-btn" tabindex="1">
                  <i class="fa fa-play">
                    <xsl:text> </xsl:text>
                  </i><xsl:text> </xsl:text>play<xsl:text> </xsl:text>
                </a>
              </li>
              <li>
                <a href="#" class="jp-pause btn btn-warning navbar-btn" tabindex="1">
                  <i class="fa fa-pause">
                    <xsl:text> </xsl:text>
                  </i><xsl:text> </xsl:text>pause<xsl:text> </xsl:text>
                </a>
              </li>
              <li>
                <a href="#" class="jp-stop btn btn-danger navbar-btn" tabindex="1">
                  <i class="fa fa-stop">
                    <xsl:text> </xsl:text>
                  </i><xsl:text> </xsl:text>stop<xsl:text> </xsl:text>
                </a>
              </li>
            </ul>
            <div class="terminus">&#160;</div>
          </div>
        </div>
      </xsl:if>
      <div class="description">
        <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
      </div>
      <div class="terminus">&#160;</div>
    </div>
  </xsl:template>

  <xsl:template match="Page" mode="initialiseJplayer">
    <xsl:text>$(document).ready(function(){</xsl:text>
    <xsl:for-each select="/Page/Contents/Content[@moduleType='Audio']">
      <xsl:text>$("#jquery_jplayer_</xsl:text>
      <xsl:value-of select="@id"/>
      <xsl:text>").jPlayer({
		        ready: function () {
			        $(this).jPlayer("setMedia", {
				        mp3: "</xsl:text>
      <xsl:value-of select="Path/node()"/>
      <xsl:text>"
			        }).jPlayer("stop");
		        },
            swfPath: "/ewThemes/Mosaic/js",
            supplied: "mp3",
            cssSelectorAncestor: "#jp_interface_</xsl:text>
      <xsl:value-of select="@id"/>
      <xsl:text>"
	        })
	        .bind($.jPlayer.event.play, function() { // Using a jPlayer event to avoid both jPlayers playing together.
			        $(this).jPlayer("pauseOthers");
	        });
          </xsl:text>
    </xsl:for-each>
    <xsl:text>
        });
        </xsl:text>
  </xsl:template>


  <!--   ################   Slide Gallery   ###############   -->
  <!-- Slide Gallery Module -->
  <xsl:template match="Content[(@type='Module' and @moduleType='SliderGallery') or Content[@type='LibraryImageWithLink']]" mode="displayBrief">
    <!--Moved so we can use within Event / Product templates too-->
    <xsl:apply-templates select="."  mode="displaySlideGallery">
      <xsl:with-param name="contentType" select="@contentType"/>
    </xsl:apply-templates>
  </xsl:template>

  <xsl:template match="Content[(@type='Module' and @moduleType='SliderGallery') or Content[@type='LibraryImageWithLink']]" mode="contentJS">
    <!--Moved so we can use within Event / Product templates too-->
    <xsl:apply-templates select="."  mode="displaySlideGalleryJS"/>
  </xsl:template>

  <!--   ################   Slide Gallery   ###############   -->
  <!-- Slide Gallery Module -->
  <xsl:template match="Content" mode="displaySlideGallery">
    <!-- Set Variables -->
    <xsl:param name="contentType"/>
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="stepCount">
      <xsl:choose>
        <xsl:when test="@stepCount!=''">
          <xsl:value-of select="@stepCount"/>
        </xsl:when>
        <xsl:otherwise>
          5
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <Content>
        <xsl:for-each select="@*">
          <xsl:attribute name="{name()}">
            <xsl:value-of select="."/>
          </xsl:attribute>
        </xsl:for-each>
        <xsl:apply-templates select="." mode="getContent">
          <xsl:with-param name="contentType" select="$contentType" />
          <xsl:with-param name="startPos" select="$startPos" />
        </xsl:apply-templates>
      </Content>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <div class="contentSliderGallery">
      <div id="slider-gallery-{@id}">
        <div class="tn3 album">
          <h4>Slider Gallery</h4>
          <div class="tn3 description">In admin mode the Slider Gallery is disabled, to make editing easier. To see the working gallery, use preview mode</div>
          <ol>
            <xsl:apply-templates select="ms:node-set($contentList)/*/*" mode="displayBriefSliderGallery">
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
          </ol>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content" mode="displaySlideGalleryJS">
    <script type="text/javascript">
      <xsl:text>$(document).ready(function() {
        var tn1 = $('#slider-gallery-</xsl:text>
      <xsl:value-of select="@id"/>
      <xsl:text>').tn3({
        skinDir:"skins",
        autoplay:true,
        </xsl:text>
      <xsl:if test="@height!=''">
        <xsl:text>height:</xsl:text>
        <xsl:value-of select="@height"/>
        <xsl:text>,</xsl:text>
      </xsl:if>
      <xsl:text>
        fullOnly:false,
        responsive:true,
        mouseWheel:false,
        delay:</xsl:text>
      <xsl:choose>
        <xsl:when test="@advancedSpeed!=''">
          <xsl:value-of select="@advancedSpeed"/>
        </xsl:when>
        <xsl:otherwise>
          10
        </xsl:otherwise>
      </xsl:choose>
      <xsl:text>,
        imageClick:"url",
        image:{
        crop:true,
        maxZoom:2,
        align:0,
        overMove:true,
        transitions:[{
        type:"blinds",
        duration:300
        },
        {
        type:"grid",
        duration:160,
        gridX:9,
        gridY:7,
        easing:"easeInCubic",
        sort:"circle"
        },{
        type:"slide",
        duration:430,
        easing:"easeInOutExpo"
        }]
        }
        });
        });</xsl:text>
    </script>

  </xsl:template>
  <!-- Library Image Brief -->
  <xsl:template match="Content[@type='LibraryImageWithLink' or @type='LibraryImage']" mode="displayBriefSliderGallery">
    <li>
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem newsarticle'"/>
      </xsl:apply-templates>
      <h4>
        <xsl:value-of select="Title/node()"/>
      </h4>
      <div class="tn3 description">
        <xsl:apply-templates select="Body/node()" mode="cleanXhtml"></xsl:apply-templates>
      </div>
      <a href="{Images/img[@class='detail']/@src}">
        <xsl:apply-templates select="." mode="displayThumbnail"/>
      </a>
    </li>
  </xsl:template>


  <!--   ################   Carousel Gallery   ###############   -->
  <!--  Module -->
  <xsl:template match="Content[(@type='Module' and @moduleType='Carousel') or Content[@type='LibraryImageWithLink']]" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <Content>
        <xsl:for-each select="@*">
          <xsl:attribute name="{name()}">
            <xsl:value-of select="."/>
          </xsl:attribute>
        </xsl:for-each>
        <xsl:apply-templates select="." mode="getContent">
          <xsl:with-param name="contentType" select="$contentType" />
          <xsl:with-param name="startPos" select="$startPos" />
        </xsl:apply-templates>
      </Content>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- Output Module -->
    <xsl:variable name="id" select="concat('bscarousel-',@id)"></xsl:variable>
    <div id="{$id}" class="carousel slide" data-ride="carousel" data-interval="{@interval}" pause="hover" wrap="true">
      <xsl:if test="@bullets!='true'">
        <ol class="carousel-indicators">
          <xsl:for-each select="Content[@type='LibraryImageWithLink']">
            <li data-target="#{$id}" data-slide-to="{position()-1}">
              <xsl:if test="position()=1">
                <xsl:attribute name="class">active</xsl:attribute>
              </xsl:if>
              <xsl:text></xsl:text>
            </li>
          </xsl:for-each>
        </ol>
      </xsl:if>
      <div class="carousel-inner">
        <xsl:apply-templates select="Content[@type='LibraryImageWithLink']" mode="displayBriefSliderGalleryx">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
      </div>
      <xsl:if test="@arrows!='true'">
        <a class="left carousel-control" href="#{$id}" data-slide="prev">
          <span class="glyphicon glyphicon-chevron-left"></span>
        </a>
        <a class="right carousel-control" href="#{$id}" data-slide="next">
          <span class="glyphicon glyphicon-chevron-right"></span>
        </a>
      </xsl:if>
    </div>
  </xsl:template>
  <!-- Library Image Brief -->
  <xsl:template match="Content[@type='LibraryImageWithLink']" mode="displayBriefSliderGalleryx">
    <div class="item">
      <xsl:if test="position()=1">
        <xsl:attribute name="class">item active</xsl:attribute>
      </xsl:if>
      <xsl:choose>
        <xsl:when test="@link!=''">
          <a>
            <xsl:attribute name="href">
              <xsl:choose>
                <xsl:when test="format-number(@link,'0')!='NaN'">
                  <xsl:variable name="pageId" select="@link"/>
                  <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]" mode="getHref"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="@link"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
            <img src="{Images/img[@class='detail']/@src}" alt="{Title/node()}" />
          </a>
        </xsl:when>
        <xsl:otherwise>
          <img src="{Images/img[@class='detail']/@src}" alt="{Title/node()}" />
        </xsl:otherwise>
      </xsl:choose>
      <xsl:if test="(Title/node()!='' and not(@showHeading='false')) or Body/node()!=''">
        <div class="carousel-caption">
          <xsl:if test="Title/node()!='' and not(@showHeading='false')">
            <h3 class="caption-title">
              <xsl:value-of select="Title/node()"/>
            </h3>
          </xsl:if>
          <xsl:apply-templates select="Body/node()" mode="cleanXhtml"></xsl:apply-templates>
        </div>
      </xsl:if>
    </div>
  </xsl:template>


  <!--  ======================================================================================  -->
  <!--  ==  BACKGROUND CAROUSEL - based on bootstrap carousel=================================  -->
  <!--  ======================================================================================  -->
  <xsl:template match="Content[@moduleType='BackgroundCarousel']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <Content>
        <xsl:for-each select="@*">
          <xsl:attribute name="{name()}">
            <xsl:value-of select="."/>
          </xsl:attribute>
        </xsl:for-each>
        <xsl:apply-templates select="." mode="getContent">
          <xsl:with-param name="contentType" select="$contentType" />
          <xsl:with-param name="startPos" select="$startPos" />
        </xsl:apply-templates>
      </Content>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- Output Module -->
    <xsl:variable name="id" select="concat('bscarousel-',@id)"></xsl:variable>
    <div id="{$id}" class="carousel slide background-carousel" data-ride="carousel" data-interval="{@interval}" pause="hover" wrap="true">
      <xsl:if test="@bullets!='true'">
        <ol class="carousel-indicators">
          <xsl:for-each select="Content[@type='LibraryImageWithLink']">
            <li data-target="#{$id}" data-slide-to="{position()-1}">
              <xsl:if test="position()=1">
                <xsl:attribute name="class">active</xsl:attribute>
              </xsl:if>
              <xsl:text></xsl:text>
            </li>
          </xsl:for-each>
        </ol>
      </xsl:if>
      <div class="carousel-inner" style="height:{@height}px">
        <xsl:apply-templates select="Content[@type='LibraryImageWithLink' or @type='BackgroundCarouselSlide']" mode="displayBriefSliderGalleryBackground">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
      </div>
      <xsl:if test="@arrows!='true'">
        <a class="left carousel-control" href="#{$id}" data-slide="prev">
          <span class="glyphicon glyphicon-chevron-left"></span>
        </a>
        <a class="right carousel-control" href="#{$id}" data-slide="next">
          <span class="glyphicon glyphicon-chevron-right"></span>
        </a>
      </xsl:if>
    </div>
  </xsl:template>
  <!-- Library Image Brief -->
  <xsl:template match="Content[@type='LibraryImageWithLink' or @type='BackgroundCarouselSlide']" mode="displayBriefSliderGalleryBackground">
    <div class="item" style="background-image:url({Images/img[@class='detail']/@src})">
      <xsl:if test="position()=1">
        <xsl:attribute name="class">item active</xsl:attribute>
      </xsl:if>
      <xsl:if test="(Title/node()!='' and not(@showHeading='false')) or Body/node()!=''">
        <div class="carousel-caption">
          <xsl:attribute name="class">
            <xsl:text>carousel-caption container carousel-v-</xsl:text>
            <xsl:value-of select="@position-vertical"/>
            <xsl:text> carousel-h-</xsl:text>
            <xsl:value-of select="@position-horizontal"/>
          </xsl:attribute>
          <div class="carousel-caption-inner">
            <xsl:if test="Title/node()!='' and not(@showHeading='false')">
              <h3 class="caption-title">
                <xsl:value-of select="Title/node()"/>
              </h3>
            </xsl:if>
            <xsl:apply-templates select="Body/node()" mode="cleanXhtml"></xsl:apply-templates>
            <xsl:if test="@link!=''">

              <xsl:apply-templates select="." mode="moreLink">
                <xsl:with-param name="link">
                  <xsl:choose>
                    <xsl:when test="format-number(@link,'0')!='NaN'">
                      <xsl:variable name="pageId" select="@link"/>
                      <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId or PageVersion[@vParId=$pageId]]" mode="getHref"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="@link"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:with-param>
                <xsl:with-param name="linkText" select="@linkText"/>
                <xsl:with-param name="altText" select="@title"/>
              </xsl:apply-templates>
            </xsl:if>
          </div>
        </div>
      </xsl:if>
    </div>
  </xsl:template>

</xsl:stylesheet>

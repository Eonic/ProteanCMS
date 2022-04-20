<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

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


</xsl:stylesheet>
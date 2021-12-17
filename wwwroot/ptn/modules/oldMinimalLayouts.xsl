<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew fb g xlink" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew" xmlns:fb="https://www.facebook.com/2008/fbml" xmlns:xlink="http://www.w3.org/2000/svg" xmlns:g="http://base.google.com/ns/1.0">




  <!-- ## Generic displayBrief for Formatted Text and Images   #####################################################################   -->


  <!-- ## Generic displayBrief for Formatted Text and Images   #####################################################################   -->
  <xsl:template match="Content[@moduleType='FormattedCode']" mode="displayBrief">
    <pre class="code">
      <xsl:apply-templates select="node()" mode="cleanXhtml"/>
    </pre>
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







</xsl:stylesheet>

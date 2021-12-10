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

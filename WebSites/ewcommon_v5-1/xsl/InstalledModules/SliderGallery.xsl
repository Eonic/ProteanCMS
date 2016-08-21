<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">

  <!--   ################   Slide Gallery   ###############   -->

  <!-- Slide Gallery Module -->
  <xsl:template match="Content[(@type='Module' and @moduleType='SliderGallery') or Content[@type='LibraryImageWithLink']]" mode="displayBrief">
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
    <div class="GalleryImageList Grid">

      <div class="cols{@cols}">

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
        <!--<xsl:apply-templates select="ms:node-set($contentList)/*/*" mode="displayBrief">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>-->
        <div class="terminus">&#160;</div>
      </div>
    </div>
    
    <!--<script type="text/javascript">
      $(document).ready(function() {
      var tn1 = $('.mygallery').tn3({
      skinDir:"skins",
      delay:5000,
      skin:["tn3e", "bullets"],
      imageClick:"url",
      image:{
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
      },
      thumbnailer: {
      align: 2,
      mode: "bullets"
      }
      });
      });
    </script>-->
    <script type="text/javascript">
      <xsl:text>$(document).ready(function() {
      var tn1 = $('.mygallery').tn3({
      skinDir:"skins",
      autoplay:true,
      </xsl:text>
      <xsl:if test="@height!=''">
      <xsl:text>height:</xsl:text><xsl:value-of select="@height"/><xsl:text>,</xsl:text>
      </xsl:if>
      <xsl:text>
      fullOnly:false,
      responsive:true,
      mouseWheel:false,
      delay:</xsl:text>
      <xsl:value-of select="@advancedSpeed"/><xsl:text>,
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
    <div class="contentSliderGallery">
      <!--<script type="text/javascript">
        $(document).ready(function() {
        var tn1 = $('.mygallery').tn3({
        skinDir:"skins",
        autoplay:true,
        fullOnly:false,
        responsive:true,
        mouseWheel:false,
        delay:5000,
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
        });
      </script>-->
      
      <div class="mygallery">
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

  <!-- Library Image Brief -->
  <xsl:template match="Content[@type='LibraryImageWithLink']" mode="displayBriefSliderGallery">
    
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

        <!--<xsl:value-of select="Images/img[@class='detail']"/>-->
        <xsl:apply-templates select="." mode="displayThumbnail"/>
      </a>
    </li>

  </xsl:template>
 
</xsl:stylesheet>

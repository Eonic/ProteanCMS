<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew fb g xlink" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew" xmlns:fb="https://www.facebook.com/2008/fbml" xmlns:xlink="http://www.w3.org/2000/svg" xmlns:g="http://base.google.com/ns/1.0">

  <!-- ## Layout Types are specified in the LayoutsManifest.XML file  ################################   -->
  <xsl:template match="Page" mode="mainLayout">
    <xsl:param name="containerClass"/>
    <xsl:choose>
      <!-- IF QUOTE CMD SHOW QUOTE -->
      <xsl:when test="Cart[@type='quote']/Quote/@cmd!=''">
        <div class="container">
          <xsl:apply-templates select="Cart[@type='quote']/Quote" mode="cartFull"/>
        </div>
      </xsl:when>
      <!-- IF CART CMD SHOW CART -->
      <xsl:when test="Cart[@type='order']/Order/@cmd!=''">
        <div class="container">
          <xsl:apply-templates select="Cart[@type='order']/Order" mode="cartFull"/>
        </div>
      </xsl:when>
      <!-- IF GIFT LIST CMD SHOW GIFT LIST -->
      <xsl:when test="Cart[@type='giftlist']/Order/@cmd!=''">
        <div class="container">
          <xsl:apply-templates select="Cart[@type='giftlist' and @name='cart']/Order" mode="giftlistDetail"/>
        </div>
      </xsl:when>
      <!-- IF ContentDetail Show ContentDetail -->
      <xsl:when test="ContentDetail">
        <div class="detail-container">
          <xsl:attribute name="class">
            <xsl:text>detail-container </xsl:text>
            <xsl:value-of select="$page/ContentDetail/Content/@type"/>
            <xsl:text>-detail-container</xsl:text>
          </xsl:attribute>
          <xsl:apply-templates select="ContentDetail" mode="ContentDetail"/>
          <xsl:apply-templates select="ContentDetail/Content" mode="socialBookmarks" />
        </div>
      </xsl:when>
      <xsl:otherwise>
        <!-- Otherwise show page layout -->
        <xsl:apply-templates select="." mode="Layout">
          <xsl:with-param name="containerClass" select="$containerClass"/>
        </xsl:apply-templates>
        <xsl:apply-templates select="." mode="socialBookmarks" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!-- ## Layout Header & Footer   ###################################################################   -->

  <xsl:template match="Page" mode="layoutHeader">
    <xsl:param name="containerClass"/>
    <xsl:if test="/Page/Contents/Content[@name='header' or @position='header'] or /Page/@adminMode">
      <xsl:apply-templates select="/Page" mode="addModule">
        <xsl:with-param name="text">Add Module</xsl:with-param>
        <xsl:with-param name="position">header</xsl:with-param>
        <xsl:with-param name="class" select="$containerClass"/>
      </xsl:apply-templates>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Page" mode="layoutFooter">
    <xsl:param name="containerClass"/>
    <xsl:if test="/Page/Contents/Content[@name='footer' or @position='footer'] or /Page/@adminMode">
      <xsl:apply-templates select="/Page" mode="addModule">
        <xsl:with-param name="text">Add Module</xsl:with-param>
        <xsl:with-param name="position">footer</xsl:with-param>
        <xsl:with-param name="class" select="$containerClass"/>
      </xsl:apply-templates>
    </xsl:if>
  </xsl:template>

  <!-- ## Default Layout  ############################################################################   -->
  <xsl:template match="Page" mode="Layout">
    <xsl:param name="containerClass"/>
    <div class="template" id="template_1_Column">
      <xsl:apply-templates select="." mode="layoutHeader">
        <xsl:with-param name="containerClass" select="$containerClass"/>
      </xsl:apply-templates>
      <div class="{$containerClass} content">
        <xsl:choose>
          <xsl:when test="@layout=''">
            <xsl:call-template name="term2000" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="term2001" />
            <xsl:text>&#160;</xsl:text>
            <strong>
              <xsl:value-of select="@layout"/>
            </strong>
            <xsl:text>&#160;</xsl:text>
            <xsl:call-template name="term2002" />
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text> </xsl:text>
      </div>
      <xsl:apply-templates select="." mode="layoutFooter">
        <xsl:with-param name="containerClass" select="$containerClass"/>
      </xsl:apply-templates>
      <xsl:apply-templates select="." mode="socialBookmarks" />
    </div>
  </xsl:template>

  <!-- ## Error Layout   #############################################################################   -->
  <xsl:template match="Page[@layout='Error']" mode="Layout">
    <xsl:param name="containerClass"/>
    <div class="container content" id="Error" >
      <xsl:choose>
        <xsl:when test="/Page/Contents/Content[@name='1005']">
          <xsl:apply-templates select="/Page/Contents/Content[@name='1005']" mode="displayBrief"/>
        </xsl:when>
        <xsl:when test="Contents/Content">
          <xsl:apply-templates select="Contents/Content" mode="displayBrief"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="term2003" />
        </xsl:otherwise>
      </xsl:choose>
    </div>
  </xsl:template>

  <!-- ## Module Layouts   ###########################################################################   -->

  <xsl:template match="Page[@layout='Modules_1_column' or @layout='1_Column' or @type='default']" mode="Layout">
    <xsl:param name="containerClass"/>
    <div id="template_1_Column" class="template template_1_Column">
      <xsl:apply-templates select="." mode="layoutHeader">
        <xsl:with-param name="containerClass" select="$containerClass"/>
      </xsl:apply-templates>
      <xsl:if test="/Page/Contents/Content[@name='column1' or @position='column1'] or /Page/@adminMode">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">column1</xsl:with-param>
          <xsl:with-param name="class" select="$containerClass"/>
        </xsl:apply-templates>
      </xsl:if>
    </div>
    <xsl:apply-templates select="." mode="layoutFooter">
      <xsl:with-param name="containerClass" select="$containerClass"/>
    </xsl:apply-templates>
  </xsl:template>

  <xsl:template match="Page[@layout='Modules_2_columns' or @layout='Modules_2_columns_66_33' or @layout='Modules_2_columns_33_66' or @layout='Modules_2_columns_75_25' or @layout='Modules_2_columns_25_75']" mode="Layout">
    <xsl:param name="containerClass"/>
    <xsl:variable name="col1">
      <xsl:choose>
        <xsl:when test="contains(/Page/@layout,'66_33')">
          <xsl:text>col-md-8</xsl:text>
        </xsl:when>
        <xsl:when test="contains(/Page/@layout,'75_25')">
          <xsl:text>col-md-9</xsl:text>
        </xsl:when>
        <xsl:when test="contains(/Page/@layout,'25_75')">
          <xsl:text>col-md-3</xsl:text>
        </xsl:when>
        <xsl:when test="contains(/Page/@layout,'33_66')">
          <xsl:text>col-md-4</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>col-md-6</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="col2">
      <xsl:choose>
        <xsl:when test="contains(/Page/@layout,'66_33')">
          <xsl:text>col-md-4</xsl:text>
        </xsl:when>
        <xsl:when test="contains(/Page/@layout,'75_25')">
          <xsl:text>col-md-3</xsl:text>
        </xsl:when>
        <xsl:when test="contains(/Page/@layout,'25_75')">
          <xsl:text>col-md-9</xsl:text>
        </xsl:when>
        <xsl:when test="contains(/Page/@layout,'33_66')">
          <xsl:text>col-md-8</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>col-md-6</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div class="template">
      <xsl:choose>
        <xsl:when test="contains(/Page/@layout,'66_33')">
          <xsl:attribute name="id">template_2_Columns_66_33</xsl:attribute>
          <xsl:attribute name="class">template template_2_Columns_66_33</xsl:attribute>
        </xsl:when>
        <xsl:when test="contains(/Page/@layout,'75_25')">
          <xsl:attribute name="id">template_2_Columns_75_25</xsl:attribute>
          <xsl:attribute name="class">template template_2_Columns_75_25</xsl:attribute>
        </xsl:when>
        <xsl:when test="contains(/Page/@layout,'25_75')">
          <xsl:attribute name="id">template_2_Columns_25_75</xsl:attribute>
          <xsl:attribute name="class">template template_2_Columns_25_75</xsl:attribute>
        </xsl:when>
        <xsl:when test="contains(/Page/@layout,'33_66')">
          <xsl:attribute name="id">template_2_Columns_33_66</xsl:attribute>
          <xsl:attribute name="class">template template_2_Columns_33_66</xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="id">template_2_Columns</xsl:attribute>
          <xsl:attribute name="class">template template_2_Columns</xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:apply-templates select="." mode="layoutHeader">
        <xsl:with-param name="containerClass" select="$containerClass"/>
      </xsl:apply-templates>
      <section>
        <div class="{$containerClass}">
          <div class="row">
            <div id="column1" class="column1 {$col1}">
              <xsl:apply-templates select="/Page" mode="addModule">
                <xsl:with-param name="text">Add Module</xsl:with-param>
                <xsl:with-param name="position">column1</xsl:with-param>
                <xsl:with-param name="class">
                  column1 <xsl:value-of select="$col1"/>
                </xsl:with-param>
              </xsl:apply-templates>
            </div>
            <div id="column2" class="column2 {$col2}">
              <xsl:apply-templates select="/Page" mode="addModule">
                <xsl:with-param name="text">Add Module</xsl:with-param>
                <xsl:with-param name="position">column2</xsl:with-param>
                <xsl:with-param name="class">
                  column2 <xsl:value-of select="$col2"/>
                </xsl:with-param>
              </xsl:apply-templates>
            </div>
            <div class="terminus">&#160;</div>
          </div>
        </div>
      </section>
      <xsl:apply-templates select="." mode="layoutFooter">
        <xsl:with-param name="containerClass" select="$containerClass"/>
      </xsl:apply-templates>
    </div>
  </xsl:template>

  <xsl:template match="Page[@layout='Modules_3_columns' or @layout='Modules_3_columns_50_25_25' or @layout='Modules_3_columns_25_25_50']" mode="Layout">
    <xsl:param name="containerClass"/>
    <xsl:variable name="col1">
      <xsl:choose>
        <xsl:when test="contains(/Page/@layout,'50_25_25')">
          <xsl:text>col-md-6</xsl:text>
        </xsl:when>
        <xsl:when test="contains(/Page/@layout,'25_25_50')">
          <xsl:text>col-md-3</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>col-md-4</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="col2">
      <xsl:choose>
        <xsl:when test="contains(/Page/@layout,'50_25_25')">
          <xsl:text>col-md-3</xsl:text>
        </xsl:when>
        <xsl:when test="contains(/Page/@layout,'25_25_50')">
          <xsl:text>col-md-3</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>col-md-4</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="col3">
      <xsl:choose>
        <xsl:when test="contains(/Page/@layout,'50_25_25')">
          <xsl:text>col-md-3</xsl:text>
        </xsl:when>
        <xsl:when test="contains(/Page/@layout,'25_25_50')">
          <xsl:text>col-md-6</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>col-md-4</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div class="template">
      <xsl:choose>
        <xsl:when test="contains(/Page/@layout,'50_25_25')">
          <xsl:attribute name="id">template_3_Columns_50_25_25</xsl:attribute>
          <xsl:attribute name="class">template template_3_Columns_50_25_25</xsl:attribute>
        </xsl:when>
        <xsl:when test="contains(/Page/@layout,'25_25_50')">
          <xsl:attribute name="id">template_3_Columns_25_25_50</xsl:attribute>
          <xsl:attribute name="class">template template_3_Columns_25_25_50</xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="id">template_3_Columns</xsl:attribute>
          <xsl:attribute name="class">template template_3_Columns</xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:apply-templates select="." mode="layoutHeader">
        <xsl:with-param name="containerClass" select="$containerClass"/>
      </xsl:apply-templates>
      <section>
        <div class="{$containerClass} template template_3_Columns" id="template_3_Columns">
          <div class="row">
            <div id="column1" class="column1 {$col1}">
              <xsl:apply-templates select="/Page" mode="addModule">
                <xsl:with-param name="text">Add Module</xsl:with-param>
                <xsl:with-param name="position">column1</xsl:with-param>
                <xsl:with-param name="class">
                  <xsl:text>column1 </xsl:text>
                  <xsl:value-of select="$col1"/>
                </xsl:with-param>
              </xsl:apply-templates>
            </div>
            <div id="column2" class="column2 {$col2}">
              <xsl:apply-templates select="/Page" mode="addModule">
                <xsl:with-param name="text">Add Module</xsl:with-param>
                <xsl:with-param name="position">column2</xsl:with-param>
                <xsl:with-param name="class">
                  <xsl:text>column2 </xsl:text>
                  <xsl:value-of select="$col2"/>
                </xsl:with-param>
              </xsl:apply-templates>
            </div>
            <div id="column3" class="column3 {$col3}">
              <xsl:apply-templates select="/Page" mode="addModule">
                <xsl:with-param name="text">Add Module</xsl:with-param>
                <xsl:with-param name="position">column3</xsl:with-param>
                <xsl:with-param name="class">
                  <xsl:text>column3 </xsl:text>
                  <xsl:value-of select="$col3"/>
                </xsl:with-param>
              </xsl:apply-templates>
            </div>
          </div>
        </div>
      </section>
      <!-- Terminus class fix to floating columns -->
      <div class="terminus">&#160;</div>
      <xsl:apply-templates select="." mode="layoutFooter">
        <xsl:with-param name="containerClass" select="$containerClass"/>
      </xsl:apply-templates>
    </div>
  </xsl:template>

  <xsl:template match="Page[@layout='Modules_4_columns']" mode="Layout">
    <xsl:param name="containerClass"/>
    <xsl:variable name="col4">
      <xsl:text>col-md-3</xsl:text>
    </xsl:variable>
    <div class="template template_4_Columns " id="template_4_Columns">
      <xsl:apply-templates select="." mode="layoutHeader">
        <xsl:with-param name="containerClass" select="$containerClass"/>
      </xsl:apply-templates>
      <section>
        <div class="{$containerClass}">
          <div class="row">
            <div id="column1" class="column1 {$col4}">
              <xsl:apply-templates select="/Page" mode="addModule">
                <xsl:with-param name="text">Add Module</xsl:with-param>
                <xsl:with-param name="position">column1</xsl:with-param>
                <xsl:with-param name="class">
                  <xsl:text>column1 </xsl:text>
                  <xsl:value-of select="$col4"/>
                </xsl:with-param>
              </xsl:apply-templates>
            </div>
            <div id="column2" class="column2 {$col4}">
              <xsl:apply-templates select="/Page" mode="addModule">
                <xsl:with-param name="text">Add Module</xsl:with-param>
                <xsl:with-param name="position">column2</xsl:with-param>
                <xsl:with-param name="class">
                  <xsl:text>column2 </xsl:text>
                  <xsl:value-of select="$col4"/>
                </xsl:with-param>
              </xsl:apply-templates>
            </div>
            <div id="column3" class="column3 {$col4}">
              <xsl:apply-templates select="/Page" mode="addModule">
                <xsl:with-param name="text">Add Module</xsl:with-param>
                <xsl:with-param name="position">column3</xsl:with-param>
                <xsl:with-param name="class">
                  <xsl:text>column3 </xsl:text>
                  <xsl:value-of select="$col4"/>
                </xsl:with-param>
              </xsl:apply-templates>
            </div>
            <div id="column4" class="column4 {$col4}">
              <xsl:apply-templates select="/Page" mode="addModule">
                <xsl:with-param name="text">Add Module</xsl:with-param>
                <xsl:with-param name="position">column4</xsl:with-param>
                <xsl:with-param name="class">
                  <xsl:text>column4 </xsl:text>
                  <xsl:value-of select="$col4"/>
                </xsl:with-param>
              </xsl:apply-templates>
            </div>
            <!-- Terminus class fix to floating columns -->
            <div class="terminus">&#160;</div>
          </div>
        </div>
      </section>
      <xsl:apply-templates select="." mode="layoutFooter">
        <xsl:with-param name="containerClass" select="$containerClass"/>
      </xsl:apply-templates>
    </div>
  </xsl:template>

  <xsl:template match="Page[@layout='Modules_Masonary']" mode="Layout">
    <xsl:param name="containerClass"/>
    <xsl:variable name="colcount" select="'6'"/>
    <div class="template template_Masonary" id="template_Masonary">
      <xsl:apply-templates select="." mode="layoutHeader">
        <xsl:with-param name="containerClass" select="$containerClass"/>
      </xsl:apply-templates>
      <div id="column1">
        <xsl:apply-templates select="/Page" mode="addMasonaryModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">column1-1col</xsl:with-param>
          <xsl:with-param name="class">column1-1col</xsl:with-param>
        </xsl:apply-templates>
        <div class="terminus">&#160;</div>
      </div>
      <!-- Terminus class fix to floating columns -->
      <div class="terminus">&#160;</div>
      <xsl:apply-templates select="." mode="layoutFooter">
        <xsl:with-param name="containerClass" select="$containerClass"/>
      </xsl:apply-templates>
    </div>
    <script type="text/javascript">
      $('#isotope-module').isotope({
      itemSelector : '.module',
      containerStyle: {position: 'relative'},
      masonry: {
      columnWidth: 1
      }
      });
      $(function () {
      var zIndexNumber = 10000;
      $('#isotope-module .editable,#isotope-module div.options').each(function () {
      $(this).css('zIndex', zIndexNumber);
      zIndexNumber -= 1;
      });
      });
    </script>
  </xsl:template>

  <xsl:template match="Content[@type='Module']" mode="themeModuleExtras">

  </xsl:template>

  <xsl:template match="Content[@type='Module']" mode="themeModuleClassExtras">

  </xsl:template>

  <!-- ## Module Handlers - Boxes, No-Boxes, Links and Titles  #######################################   -->
  <xsl:template match="Content[@type='Module']" mode="displayModule">
    <xsl:choose>
      <xsl:when test="@box!='false' and @box!=''">
        <xsl:apply-templates select="." mode="moduleBox"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="thisClass">
          <xsl:if test="@iconStyle='Centre'"> module-centred</xsl:if>
          <xsl:if test="@iconStyle='CentreSmall'"> module-centred</xsl:if>
          <xsl:if test="@iconStyle='Right'"> module-right</xsl:if>
          <xsl:if test="@iconStyle='Left'"> module-left</xsl:if>
        </xsl:variable>
        <div id="mod_{@id}" class="module nobox pos-{@position}{$thisClass}">
          <xsl:apply-templates select="." mode="themeModuleExtras"/>
          <xsl:if test="@mobileview!=''">
            <xsl:attribute name="data-isMobileView">
              <xsl:value-of select="@mobileview"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:attribute name="class">
            <xsl:text>module nobox pos-</xsl:text>
            <xsl:value-of select="@position"/>
            <xsl:text> module-</xsl:text>
            <xsl:value-of select="@moduleType"/>
            <xsl:if test="@panelImage!=''">
              <xsl:text> panelImage </xsl:text>
            </xsl:if>
            <xsl:if test="@responsiveImg='true'">
              <xsl:text> module-img-responsive</xsl:text>
            </xsl:if>
            <xsl:if test="@modAnim and @modAnim!=''">
              <xsl:text> moduleAnimate-invisible</xsl:text>
            </xsl:if>
            <xsl:apply-templates select="." mode="hideScreens" />
            <xsl:apply-templates select="." mode="marginBelow" />
            <xsl:apply-templates select="." mode="themeModuleClassExtras"/>
            <xsl:value-of select="$thisClass"/>
          </xsl:attribute>
          <xsl:if test="@contentType='Module'">
            <xsl:attribute name="class">
              <xsl:text>module noboxlayout layoutModule pos-</xsl:text>
              <xsl:value-of select="@position"/>
              <xsl:text> </xsl:text>
              <xsl:value-of select="@background"/>
              <xsl:apply-templates select="." mode="hideScreens" />
              <xsl:apply-templates select="." mode="marginBelow" />
              <xsl:value-of select="$thisClass"/>
            </xsl:attribute>
            <!--<xsl:if test="@backgroundImage!=''">
              <xsl:attribute name="style">
                background-image: url('<xsl:value-of select="@backgroundImage"/>');
              </xsl:attribute>
            </xsl:if>-->
          </xsl:if>
          <xsl:if test="@moduleType='Accordion'">
            <xsl:attribute name="class">
              <xsl:text>module nobox layoutModule accordion-module pos-</xsl:text>
              <xsl:value-of select="@position"/>
              <xsl:apply-templates select="." mode="hideScreens" />
              <xsl:apply-templates select="." mode="marginBelow" />
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="@moduleType='Tabbed'">
            <xsl:attribute name="class">
              <xsl:text>module layoutModule tabbed-module pos-</xsl:text>
              <xsl:value-of select="@position"/>
              <xsl:apply-templates select="." mode="hideScreens" />
              <xsl:apply-templates select="." mode="marginBelow" />
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and @imagePosition='above'">
            <div class="panel-image">
              <img src="{@panelImage}" alt="{@title}" class="img-responsive" />
            </div>
          </xsl:if>
          <xsl:if test="not(@position='header' or @position='footer' or (@position='column1' and $page/@layout='Modules_1_column'))">
            <!--<xsl:if test="@data-stellar-background-ratio!='0'">
              <xsl:attribute name="data-stellar-background-ratio">
                <xsl:value-of select="(@data-stellar-background-ratio div 10)"/> test
              </xsl:attribute>
            </xsl:if>-->
            <xsl:if test="@backgroundImage!=''">
              <!--<xsl:attribute name="style">
                background-image: url('<xsl:value-of select="@backgroundImage"/>');
              </xsl:attribute>-->

              <xsl:choose>
                <xsl:when test="@data-stellar-background-ratio!='0'">
                  <xsl:choose>
                    <xsl:when test="@data-stellar-background-ratio!='10'">
                      <section style="height:100%" class="parallax-wrapper" >
                        <xsl:if test="@data-stellar-background-ratio!='10'">
                          <xsl:attribute name="data-parallax-speed">
                            <xsl:if test="@data-stellar-background-ratio&lt;'5'">
                              <xsl:text>1.3</xsl:text>
                            </xsl:if>
                            <xsl:if test="@data-stellar-background-ratio&gt;='5' and @data-stellar-background-ratio&lt;'10'">
                              <xsl:text>1.6</xsl:text>
                            </xsl:if>
                            <xsl:if test="@data-stellar-background-ratio&gt;='10' and @data-stellar-background-ratio&lt;'15'">
                              <xsl:text>2</xsl:text>
                            </xsl:if>
                            <xsl:if test="@data-stellar-background-ratio&gt;='15' and @data-stellar-background-ratio&lt;'20'">
                              <xsl:text>3</xsl:text>
                            </xsl:if>
                            <xsl:if test="@data-stellar-background-ratio&gt;='20' and @data-stellar-background-ratio&lt;'25'">
                              <xsl:text>4</xsl:text>
                            </xsl:if>
                            <xsl:if test="@data-stellar-background-ratio&gt;='25'">
                              <xsl:text>5</xsl:text>
                            </xsl:if>
                          </xsl:attribute>
                        </xsl:if>
                        <div class="parallax" data-parallax-image="{@backgroundImage}">

                        </div>
                      </section>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:attribute name="style">
                        background-image: url('<xsl:value-of select="@backgroundImage"/>');
                      </xsl:attribute>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:attribute name="style">
                    background-image: url('<xsl:value-of select="@backgroundImage"/>');
                  </xsl:attribute>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:if>
          </xsl:if>
          <xsl:choose>
            <xsl:when test="$page/AdminMenu/descendant-or-self::MenuItem[@cmd='Normal'] and $adminMode">
              <div>
                <xsl:apply-templates select="." mode="inlinePopupOptions" />
                <xsl:text> </xsl:text>
                <xsl:if test="@title!='' or @icon!='' or @uploadIcon!=''">
                  <h3 class="title">
                    <xsl:if test="@contentType='Module'">
                      <xsl:attribute name="class">title layout-title</xsl:attribute>
                    </xsl:if>
                    <xsl:if test="@icon!='' or @uploadIcon!=''">
                      <xsl:attribute name="class">title module-with-icon</xsl:attribute>
                    </xsl:if>
                    <xsl:apply-templates select="." mode="moduleLink"/>
                  </h3>
                </xsl:if>
              </div>
            </xsl:when>
            <xsl:otherwise>
              <xsl:if test="@title!='' or @icon!='' or @uploadIcon!=''">
                <h3 class="title">
                  <xsl:if test="@contentType='Module'">
                    <xsl:attribute name="class">title layout-title</xsl:attribute>
                  </xsl:if>
                  <xsl:if test="@icon!='' or @uploadIcon!=''">
                    <xsl:attribute name="class">title module-with-icon</xsl:attribute>
                  </xsl:if>
                  <xsl:apply-templates select="." mode="moduleLink"/>
                </h3>
              </xsl:if>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:if test="@rss and @rss!='false'">
            <xsl:apply-templates select="." mode="rssLink" />
          </xsl:if>
          <div class="terminus">&#160;</div>
          <xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and not(@imagePosition='above')">
            <div class="panel-image">
              <img src="{@panelImage}" alt="{@title}" class="img-responsive" />
            </div>
          </xsl:if>
          <xsl:apply-templates select="." mode="displayBrief"/>
          <xsl:if test="@linkText!='' and @link!=''">
            <div class="entryFooter">
              <xsl:if test="@iconStyle='Centre' or @iconStyle='CentreSmall'">
                <xsl:attribute name="class">entryFooter center-nobox-footer</xsl:attribute>
              </xsl:if>
              <xsl:apply-templates select="." mode="moreLink">
                <xsl:with-param name="link">
                  <xsl:choose>
                    <xsl:when test="format-number(@link,'0')!='NaN'">
                      <xsl:variable name="pageId" select="@link"/>
                      <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId|PageVersion/@vParId=$pageId]" mode="getHref"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="@link"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:with-param>
                <xsl:with-param name="linkText" select="@linkText"/>
                <xsl:with-param name="altText" select="@title"/>
              </xsl:apply-templates>
              <xsl:text> </xsl:text>
            </div>
          </xsl:if>
          <div class="terminus">&#160;</div>
        </div>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="Content" mode="hideScreens">
    <xsl:if test="not($adminMode)">
      <xsl:if test="contains(@screens,'lg')">
        <xsl:text> hidden-lg</xsl:text>
      </xsl:if>
      <xsl:if test="contains(@screens,'md')">
        <xsl:text> hidden-md</xsl:text>
      </xsl:if>
      <xsl:if test="contains(@screens,'sm')">
        <xsl:text> hidden-sm</xsl:text>
      </xsl:if>
      <xsl:if test="contains(@screens,'xs')">
        <xsl:text> hidden-xs</xsl:text>
      </xsl:if>
    </xsl:if>
    <xsl:if test="@matchHeight='true'">
      <xsl:text> matchHeight</xsl:text>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Content" mode="marginBelow">
    <xsl:if test="@marginBelow='false'">
      <xsl:text> margin-bottom-0 </xsl:text>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Content" mode="moduleBox">
    <xsl:choose>
      <xsl:when test="@linkBox='true'">
        <div id="mod_{@id}" class="module">
          <xsl:apply-templates select="." mode="themeModuleExtras"/>
          <xsl:if test="$adminMode">
            <div class="linkedPopUp">
              <xsl:apply-templates select="." mode="inlinePopupOptions">
                <xsl:with-param name="class" select="'linkedPopUp'"/>
              </xsl:apply-templates>
            </div>
          </xsl:if>
          <a>
            <xsl:attribute name="href">
              <xsl:choose>
                <xsl:when test="format-number(@link,'0')!='NaN'">
                  <xsl:variable name="pageId" select="@link"/>
                  <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId or PageVersion[@vParId=$pageId]]" mode="getHref"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="@link"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
            <xsl:attribute name="class">
              <xsl:text>linked-panel</xsl:text>
            </xsl:attribute>
            <div class="panel panel-default linked-panel">
              <!-- define classes for box -->
              <xsl:attribute name="class">
                <xsl:text>panel </xsl:text>
                <xsl:if test="@panelImage!=''">
                  <xsl:text>panelImage </xsl:text>
                </xsl:if>
                <xsl:if test="@icon!='' or @uploadIcon!=''">
                  <xsl:text>panel-icon </xsl:text>
                </xsl:if>
                <xsl:choose>
                  <xsl:when test="@box='Default Box'">panel-default</xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="translate(@box,' ','-')"/>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:text> linked-panel</xsl:text>
                <!-- if no title, we may still want TL/TR for rounded boxs with no title bar,
              stled differently to a title bar. -->
                <xsl:if test="@title=''">
                  <xsl:text> boxnotitle</xsl:text>
                </xsl:if>
                pos-<xsl:value-of select="@position"/>
                <xsl:if test="@modAnim and @modAnim!=''">
                  <xsl:text> moduleAnimate-invisible</xsl:text>
                </xsl:if>
                <xsl:apply-templates select="." mode="hideScreens" />
                <xsl:apply-templates select="." mode="marginBelow" />
              </xsl:attribute>
              <xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and @imagePosition='above'">
                <div class="panel-image">
                  <img src="{@panelImage}" alt="{@title}" class="img-responsive" />
                </div>
              </xsl:if>
              <xsl:if test="@title!='' or @icon!='' or @uploadIcon!=''">
                <div class="panel-heading">
                  <h3 class="panel-title">
                    <xsl:apply-templates select="." mode="moduleTitle"/>
                  </h3>
                </div>
              </xsl:if>
              <xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and not(@imagePosition='above')">
                <div class="panel-image">
                  <img src="{@panelImage}" alt="{@title}" class="img-responsive" />
                </div>
              </xsl:if>
              <div class="panel-body">
                <xsl:if test="@iconStyle='Centre'">
                  <xsl:attribute name="class">panel-body center-block</xsl:attribute>
                </xsl:if>
                <xsl:apply-templates select="." mode="displayBrief"/>
              </div>
              <xsl:if test="@linkText!=''">
                <div class="panel-footer">
                  <xsl:if test="@iconStyle='Centre'">
                    <xsl:attribute name="class">panel-footer center-block-footer</xsl:attribute>
                  </xsl:if>
                  <div class="morelink">
                    <span>
                      <button class="btn btn-default btn-sm">
                        <xsl:value-of select="@linkText"/>
                      </button>
                    </span>
                  </div>
                  <xsl:text> </xsl:text>
                </div>
              </xsl:if>
            </div>
          </a>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <div id="mod_{@id}" class="panel panel-default">
          <xsl:apply-templates select="." mode="themeModuleExtras"/>
          <!-- define classes for box -->
          <xsl:attribute name="class">
            <xsl:text>panel </xsl:text>
            <xsl:if test="@panelImage!=''">
              <xsl:text>panelImage </xsl:text>
            </xsl:if>
            <xsl:if test="@icon!='' or @uploadIcon!=''">
              <xsl:text>panel-icon </xsl:text>
            </xsl:if>
            <xsl:choose>
              <xsl:when test="@box='Default Box'">panel-default</xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="translate(@box,' ','-')"/>
              </xsl:otherwise>
            </xsl:choose>
            <xsl:text> module</xsl:text>
            <!-- if no title, we may still want TL/TR for rounded boxs with no title bar,
              stled differently to a title bar. -->
            <xsl:if test="@title=''">
              <xsl:text> boxnotitle</xsl:text>
            </xsl:if>
            pos-<xsl:value-of select="@position"/>
            <xsl:if test="@modAnim and @modAnim!=''">
              <xsl:text> moduleAnimate-invisible</xsl:text>
            </xsl:if>
            <xsl:apply-templates select="." mode="hideScreens" />
            <xsl:apply-templates select="." mode="marginBelow" />
            <xsl:apply-templates select="." mode="themeModuleExtras"/>
          </xsl:attribute>
          <xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and @imagePosition='above'">
            <div class="panel-image">
              <img src="{@panelImage}" alt="{@title}" class="img-responsive" />
            </div>
          </xsl:if>
          <xsl:if test="@title!='' or @icon!='' or @uploadIcon!=''">
            <div class="panel-heading">
              <xsl:apply-templates select="." mode="inlinePopupOptions">
                <xsl:with-param name="class" select="'panel-heading'"/>
              </xsl:apply-templates>
              <xsl:if test="@rss and @rss!='false'">
                <xsl:apply-templates select="." mode="rssLink" />
              </xsl:if>
              <h3 class="panel-title">
                <xsl:apply-templates select="." mode="moduleLink"/>
              </h3>
            </div>
          </xsl:if>
          <xsl:if test="not(@listGroup='true')">
            <xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and not(@imagePosition='above')">
              <div class="panel-image">
                <img src="{@panelImage}" alt="{@title}" class="img-responsive" />
              </div>
            </xsl:if>
            <div class="panel-body">
              <xsl:if test="not(@title!='')">
                <xsl:apply-templates select="." mode="inlinePopupOptions">
                  <xsl:with-param name="class" select="'panel-body'"/>
                </xsl:apply-templates>
              </xsl:if>
              <xsl:apply-templates select="." mode="displayBrief"/>
            </div>
          </xsl:if>
          <xsl:if test="@listGroup='true'">
            <div class="list-group">
              <xsl:if test="not(@title!='')">
                <xsl:apply-templates select="." mode="inlinePopupOptions">
                  <xsl:with-param name="class" select="'list-group'"/>
                </xsl:apply-templates>
              </xsl:if>
              <xsl:apply-templates select="." mode="displayBrief"/>
            </div>
          </xsl:if>
          <xsl:if test="@linkText!='' and @link!=''">
            <div class="panel-footer">
              <xsl:if test="@iconStyle='Centre'">
                <xsl:attribute name="class">panel-footer center-block-footer</xsl:attribute>
              </xsl:if>
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
              <xsl:text> </xsl:text>
            </div>
          </xsl:if>
        </div>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="Content" mode="modalBox">
    <div id="mod_{@id}">
      <xsl:apply-templates select="." mode="themeModuleExtras"/>
      <!-- define classes for box -->
      <xsl:attribute name="class">
        <xsl:text>modal-content </xsl:text>
        <xsl:if test="@icon!='' or @uploadIcon!=''">
          <xsl:text>panel-icon </xsl:text>
        </xsl:if>
        <xsl:choose>
          <xsl:when test="@box='Default Box'">panel-default</xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="translate(@box,' ','-')"/>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text> module</xsl:text>
        <!-- if no title, we may still want TL/TR for rounded boxs with no title bar,
              stled differently to a title bar. -->
        <xsl:if test="@title=''">
          <xsl:text> boxnotitle</xsl:text>
        </xsl:if>
        pos-<xsl:value-of select="@position"/>
        <xsl:apply-templates select="." mode="hideScreens" />
        <xsl:apply-templates select="." mode="marginBelow" />
        <xsl:apply-templates select="." mode="themeModuleExtras"/>
      </xsl:attribute>
      <xsl:if test="@title!='' or @icon!='' or @uploadIcon!=''">
        <div class="modal-header">
          <xsl:apply-templates select="." mode="inlinePopupOptions">
            <xsl:with-param name="class" select="'panel-heading'"/>
          </xsl:apply-templates>
          <xsl:if test="@rss and @rss!='false'">
            <xsl:apply-templates select="." mode="rssLink" />
          </xsl:if>
          <button type="button" class="close" data-dismiss="modal" aria-label="Close">
            <span aria-hidden="true">
              <i class="fa fa-times">&#160;</i>
            </span>
          </button>
          <h4 class="modal-title">
            <xsl:apply-templates select="." mode="moduleLink"/>
          </h4>
        </div>
      </xsl:if>
      <xsl:if test="not(@listGroup='true')">
        <xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_'">
          <div class="panel-image">
            <img src="{@panelImage}" alt="{@title}" class="img-responsive" />
          </div>
        </xsl:if>
        <div class="modal-body">
          <xsl:if test="not(@title!='')">
            <xsl:apply-templates select="." mode="inlinePopupOptions">
              <xsl:with-param name="class" select="'panel-body'"/>
            </xsl:apply-templates>
          </xsl:if>
          <xsl:apply-templates select="." mode="displayBrief"/>
        </div>
      </xsl:if>
      <xsl:if test="@listGroup='true'">
        <div class="list-group">
          <xsl:if test="not(@title!='')">
            <xsl:apply-templates select="." mode="inlinePopupOptions">
              <xsl:with-param name="class" select="'list-group'"/>
            </xsl:apply-templates>
          </xsl:if>
          <xsl:apply-templates select="." mode="displayBrief"/>
        </div>
      </xsl:if>
      <xsl:if test="@linkText!='' and @link!=''">
        <div class="panel-footer">
          <xsl:if test="@iconStyle='Centre'">
            <xsl:attribute name="class">panel-footer center-block-footer</xsl:attribute>
          </xsl:if>
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
          <xsl:text> </xsl:text>
        </div>
      </xsl:if>
    </div>
  </xsl:template>

  <xsl:template match="Content[starts-with(@box,'alert')]" mode="moduleBox">
    <xsl:choose>
      <xsl:when test="@linkBox='true'">
        <div id="mod_{@id}" class="module">
          <xsl:apply-templates select="." mode="themeModuleExtras"/>
          <div class="linkedPopUp">
            <xsl:apply-templates select="." mode="inlinePopupOptions">
              <xsl:with-param name="class" select="'linkedPopUp'"/>
            </xsl:apply-templates>
          </div>
          <a>
            <xsl:attribute name="href">
              <xsl:choose>
                <xsl:when test="format-number(@link,'0')!='NaN'">
                  <xsl:variable name="pageId" select="@link"/>
                  <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId or PageVersion[@vParId=$pageId]]" mode="getHref"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="@link"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
            <xsl:attribute name="class">
              <xsl:text>linked-panel</xsl:text>
            </xsl:attribute>
            <div class="panel panel-default linked-panel">
              <!-- define classes for box -->
              <xsl:attribute name="class">
                <xsl:text>alert </xsl:text>
                <xsl:if test="@panelImage!=''">
                  <xsl:text>panelImage alertImage </xsl:text>
                </xsl:if>
                <xsl:choose>
                  <xsl:when test="@box='Default Box'">alert-default</xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="translate(@box,' ','-')"/>
                  </xsl:otherwise>
                </xsl:choose>
                <xsl:text> linked-panel</xsl:text>
                <!-- if no title, we may still want TL/TR for rounded boxs with no title bar,
              stled differently to a title bar. -->
                <xsl:if test="@title=''">
                  <xsl:text> boxnotitle</xsl:text>
                </xsl:if>
                pos-<xsl:value-of select="@position"/>
                <xsl:if test="@modAnim and @modAnim!=''">
                  <xsl:text> moduleAnimate-invisible</xsl:text>
                </xsl:if>
                <xsl:apply-templates select="." mode="hideScreens" />
                <xsl:apply-templates select="." mode="marginBelow" />
              </xsl:attribute>
              <xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and @imagePosition='above'">
                <div class="panel-image alert-image">
                  <img src="{@panelImage}" alt="{@title}" class="img-responsive" />
                </div>
              </xsl:if>
              <xsl:if test="@title!='' or @icon!='' or @uploadIcon!=''">
                <div class="alert-heading">

                  <h4 class="alert-title">
                    <xsl:apply-templates select="." mode="moduleTitle"/>
                  </h4>
                </div>
              </xsl:if>
              <xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and not(@imagePosition='above')">
                <div class="panel-image alert-image">
                  <img src="{@panelImage}" alt="{@title}" class="img-responsive" />
                </div>
              </xsl:if>
              <div class="alert-body">
                <xsl:apply-templates select="." mode="displayBrief"/>
              </div>
            </div>
          </a>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="class">
          <xsl:text>alert </xsl:text>
          <xsl:if test="@panelImage!=''">
            <xsl:text>panelImage </xsl:text>
          </xsl:if>
          <xsl:choose>
            <xsl:when test="@box='Default Box'">alert-default</xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="translate(@box,' ','-')"/>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:text> module</xsl:text>
          <!-- if no title, we may still want TL/TR for rounded boxs with no title bar,
              stled differently to a title bar. -->
          <xsl:if test="@title=''">
            <xsl:text> boxnotitle</xsl:text>
          </xsl:if>
          pos-<xsl:value-of select="@position"/>
          <xsl:if test="@modAnim and @modAnim!=''">
            <xsl:text> moduleAnimate-invisible</xsl:text>
          </xsl:if>
          <xsl:apply-templates select="." mode="hideScreens" />
          <xsl:apply-templates select="." mode="marginBelow" />
        </xsl:variable>
        <div id="mod_{@id}" class="{$class}">
          <xsl:apply-templates select="." mode="themeModuleExtras"/>
          <xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and @imagePosition='above'">
            <div class="panel-image alert-image">
              <img src="{@panelImage}" alt="{@title}" class="img-responsive" />
            </div>
          </xsl:if>
          <xsl:if test="@title!='' or @icon!='' or @uploadIcon!=''">
            <div class="alert-heading">
              <xsl:apply-templates select="." mode="inlinePopupOptions">
                <xsl:with-param name="class" select="'alert-heading'"/>
              </xsl:apply-templates>
              <xsl:if test="@rss and @rss!='false'">
                <xsl:apply-templates select="." mode="rssLink" />
              </xsl:if>
              <h4 class="alert-title">
                <xsl:apply-templates select="." mode="moduleLink"/>
              </h4>
            </div>
          </xsl:if>
          <xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and not(@imagePosition='above')">
            <div class="panel-image alert-image">
              <img src="{@panelImage}" alt="{@title}" class="img-responsive" />
            </div>
          </xsl:if>
          <div class="alert-body">
            <xsl:if test="not(@title!='')">
              <xsl:apply-templates select="." mode="inlinePopupOptions">
                <xsl:with-param name="class" select="'alert-body'"/>
              </xsl:apply-templates>
            </xsl:if>
            <xsl:apply-templates select="." mode="displayBrief"/>
          </div>
          <xsl:if test="@linkText!='' and @link!=''">
            <div class="alert-footer">
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
              <xsl:text> </xsl:text>
            </div>
          </xsl:if>
        </div>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template match="Content[starts-with(@box,'well')]" mode="moduleBox">
    <xsl:variable name="class">
      <xsl:text>well </xsl:text>
      <xsl:if test="@panelImage!=''">
        <xsl:text>panelImage </xsl:text>
      </xsl:if>
      <xsl:value-of select="translate(@box,' ','-')"/>
      <xsl:text> module</xsl:text>
      <!-- if no title, we may still want TL/TR for rounded boxs with no title bar,
              stled differently to a title bar. -->
      <xsl:if test="@title=''">
        <xsl:text> boxnotitle</xsl:text>
      </xsl:if>
      <xsl:text> pos-</xsl:text>
      <xsl:value-of select="@position"/>
      <xsl:if test="@modAnim and @modAnim!=''">
        <xsl:text> moduleAnimate-invisible</xsl:text>
      </xsl:if>
      <xsl:apply-templates select="." mode="hideScreens" />
      <xsl:apply-templates select="." mode="marginBelow" />
    </xsl:variable>
    <div id="mod_{@id}" class="{$class}">
      <xsl:apply-templates select="." mode="themeModuleExtras"/>
      <xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and @imagePosition='above'">
        <div class="panel-image alert-image">
          <img src="{@panelImage}" alt="{@title}" class="img-responsive" />
        </div>
      </xsl:if>
      <xsl:if test="@title!='' or @icon!='' or @uploadIcon!=''">
        <div class="well-heading">
          <xsl:apply-templates select="." mode="inlinePopupOptions">
            <xsl:with-param name="class" select="'well-heading'"/>
          </xsl:apply-templates>
          <xsl:if test="@rss and @rss!='false'">
            <xsl:apply-templates select="." mode="rssLink" />
          </xsl:if>
          <h3 class="well-title">
            <xsl:apply-templates select="." mode="moduleLink"/>
          </h3>
        </div>
      </xsl:if>
      <xsl:if test="@panelImage!='' and @panelImage!=' ' and @panelImage!='_' and not(@imagePosition='above')">
        <div class="panel-image well-image">
          <img src="{@panelImage}" alt="{@title}" class="img-responsive" />
        </div>
      </xsl:if>
      <div class="well-body">
        <xsl:if test="not(@title!='')">
          <xsl:apply-templates select="." mode="inlinePopupOptions">
            <xsl:with-param name="class" select="'well-body'"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:apply-templates select="." mode="displayBrief"/>
      </div>
      <xsl:if test="@linkText!='' and @link!=''">
        <div class="well-footer">
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
          <xsl:text> </xsl:text>
        </div>
      </xsl:if>
    </div>
  </xsl:template>

  <xsl:template match="Content[starts-with(@box,'jumbotron')]" mode="moduleBox">
    <xsl:variable name="class">
      <xsl:text>jumbotron </xsl:text>
      <xsl:text> module</xsl:text>
      <!-- if no title, we may still want TL/TR for rounded boxs with no title bar,
              stled differently to a title bar. -->
      <xsl:if test="@title=''">
        <xsl:text> boxnotitle</xsl:text>
      </xsl:if>
      <xsl:text> pos-</xsl:text>
      <xsl:value-of select="@position"/>
      <xsl:if test="@modAnim and @modAnim!=''">
        <xsl:text> moduleAnimate-invisible</xsl:text>
      </xsl:if>
      <xsl:apply-templates select="." mode="hideScreens" />
      <xsl:apply-templates select="." mode="marginBelow" />
    </xsl:variable>
    <div id="mod_{@id}" class="{$class}">
      <xsl:apply-templates select="." mode="themeModuleExtras"/>
      <xsl:if test="@title!='' or @icon!='' or @uploadIcon!=''">
        <div class="jumbotron-heading">
          <xsl:apply-templates select="." mode="inlinePopupOptions">
            <xsl:with-param name="class" select="'well-heading'"/>
          </xsl:apply-templates>
          <xsl:if test="@rss and @rss!='false'">
            <xsl:apply-templates select="." mode="rssLink" />
          </xsl:if>
          <h1 class="well-title">
            <xsl:apply-templates select="." mode="moduleTitle"/>
          </h1>
        </div>
      </xsl:if>
      <div class="jumbotron-body">
        <xsl:if test="not(@title!='')">
          <xsl:apply-templates select="." mode="inlinePopupOptions">
            <xsl:with-param name="class" select="'panel-body'"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:apply-templates select="." mode="displayBrief"/>
      </div>
      <xsl:if test="@linkText!='' and @link!=''">
        <div class="jumbotron-footer">
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
          <xsl:text> </xsl:text>
        </div>
      </xsl:if>
    </div>
  </xsl:template>

  <!-- ## Generic displayBrief   #####################################################################   -->
  <xsl:template match="Content" mode="displayBrief">
    <xsl:apply-templates select="node()" mode="cleanXhtml"/>
  </xsl:template>

  <!-- ## Generic Module displayBrief   #####################################################################   -->
  <xsl:template match="Content[@type='Module']" mode="displayBrief">
    <span class="alert">* Module type unknown *</span>
  </xsl:template>

  <xsl:template match="Content[@moduleType='3columns']" mode="displayBrief">
    <xsl:variable name="responsiveColumns">
      <xsl:apply-templates select="." mode="responsiveColumns">
        <xsl:with-param name="defaultCols" select="'4'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row">
      <div id="column1-{@id}" class="column1 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column3-{@id}" class="column3 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column3-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column3 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='1Column' or @moduleType='Conditional1Column']" mode="displayBrief">
    <div class="row">
      <xsl:if test="$adminMode and @moduleType='Conditional1Column'">
        <xsl:attribute name="class">row conditional-block</xsl:attribute>
        <div class="conditional-note">
          This block is conditional on the querystring containing '<xsl:value-of select="@querystringcontains"/>'
        </div>
      </xsl:if>
      <div id="column1-{@id}" class="column1 col-md-12">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 col-md-12</xsl:text>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2columns5050']" mode="displayBrief">
    <xsl:variable name="responsiveColumns">
      <xsl:apply-templates select="." mode="responsiveColumns">
        <xsl:with-param name="defaultCols" select="'6'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row">
      <div id="column1-{@id}" class="column1 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2Columns3366']" mode="displayBrief">
    <xsl:variable name="unevenColumns1">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'4'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="unevenColumns2">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'8'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row">
      <div id="column1-{@id}" class="column1 {$unevenColumns1}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$unevenColumns1"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$unevenColumns2}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$unevenColumns2"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2Columns6633']" mode="displayBrief">
    <xsl:variable name="unevenColumns1">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'8'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="unevenColumns2">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'4'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row">
      <div id="column1-{@id}" class="column1 {$unevenColumns1}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$unevenColumns1"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$unevenColumns2}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$unevenColumns2"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2Columns2575']" mode="displayBrief">
    <xsl:variable name="unevenColumns1">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'3'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="unevenColumns2">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'9'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row">
      <div id="column1-{@id}" class="column1 {$unevenColumns1}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$unevenColumns1"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$unevenColumns2}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$unevenColumns2"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2Columns7525']" mode="displayBrief">
    <xsl:variable name="unevenColumns1">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'9'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="unevenColumns2">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'3'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row">
      <div id="column1-{@id}" class="column1 {$unevenColumns1}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$unevenColumns1"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$unevenColumns2}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$unevenColumns2"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2Columns1683']" mode="displayBrief">
    <xsl:variable name="unevenColumns1">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'2'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="unevenColumns2">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'10'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row">
      <div id="column1-{@id}" class="column1 {$unevenColumns1}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$unevenColumns1"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$unevenColumns2}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$unevenColumns2"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2Columns8316']" mode="displayBrief">
    <xsl:variable name="unevenColumns1">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'10'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="unevenColumns2">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'2'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row">
      <div id="column1-{@id}" class="column1 {$unevenColumns1}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$unevenColumns1"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$unevenColumns2}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$unevenColumns2"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='4columns']" mode="displayBrief">
    <xsl:variable name="responsiveColumns">
      <xsl:apply-templates select="." mode="responsiveColumns">
        <xsl:with-param name="defaultCols" select="'3'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row">
      <div id="column1-{@id}" class="column1 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column3-{@id}" class="column3 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column3-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column3 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column4-{@id}" class="column4 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column4-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column4 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2Columns4060']" mode="displayBrief">
    <xsl:variable name="unevenColumns1">
      <xsl:apply-templates select="." mode="uneven5Columns">
        <xsl:with-param name="defaultWidth" select="'4'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="unevenColumns2">
      <xsl:apply-templates select="." mode="uneven5Columns">
        <xsl:with-param name="defaultWidth" select="'6'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row fivecolumns">
      <xsl:if test="@mdCol and @mdCol!=''">
        <xsl:attribute name="class">row fivecolumns fivecolumns-lg</xsl:attribute>
      </xsl:if>
      <div id="column1-{@id}" class="column1 {$unevenColumns1}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$unevenColumns1"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$unevenColumns2}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$unevenColumns2"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2Columns6040']" mode="displayBrief">
    <xsl:variable name="unevenColumns1">
      <xsl:apply-templates select="." mode="uneven5Columns">
        <xsl:with-param name="defaultWidth" select="'6'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="unevenColumns2">
      <xsl:apply-templates select="." mode="uneven5Columns">
        <xsl:with-param name="defaultWidth" select="'4'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row fivecolumns">
      <xsl:if test="@mdCol and @mdCol!=''">
        <xsl:attribute name="class">row fivecolumns fivecolumns-lg</xsl:attribute>
      </xsl:if>
      <div id="column1-{@id}" class="column1 {$unevenColumns1}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$unevenColumns1"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$unevenColumns2}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$unevenColumns2"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2Columns2080']" mode="displayBrief">
    <xsl:variable name="unevenColumns1">
      <xsl:apply-templates select="." mode="uneven5Columns">
        <xsl:with-param name="defaultWidth" select="'2'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="unevenColumns2">
      <xsl:apply-templates select="." mode="uneven5Columns">
        <xsl:with-param name="defaultWidth" select="'8'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row fivecolumns">
      <xsl:if test="@mdCol and @mdCol!=''">
        <xsl:attribute name="class">row fivecolumns fivecolumns-lg</xsl:attribute>
      </xsl:if>
      <div id="column1-{@id}" class="column1 {$unevenColumns1}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$unevenColumns1"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$unevenColumns2}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$unevenColumns2"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2Columns8020']" mode="displayBrief">
    <xsl:variable name="unevenColumns1">
      <xsl:apply-templates select="." mode="uneven5Columns">
        <xsl:with-param name="defaultWidth" select="'8'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="unevenColumns2">
      <xsl:apply-templates select="." mode="uneven5Columns">
        <xsl:with-param name="defaultWidth" select="'2'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row fivecolumns">
      <xsl:if test="@mdCol and @mdCol!=''">
        <xsl:attribute name="class">row fivecolumns fivecolumns-lg</xsl:attribute>
      </xsl:if>
      <div id="column1-{@id}" class="column1 {$unevenColumns1}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$unevenColumns1"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$unevenColumns2}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$unevenColumns2"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='5columns']" mode="displayBrief">
    <xsl:variable name="responsiveColumns">
      <xsl:apply-templates select="." mode="responsiveColumns">
        <xsl:with-param name="defaultCols" select="'2'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row fivecolumns">
      <xsl:if test="@mdCol and @mdCol!='' and @mdCol!='5'">
        <xsl:attribute name="class">row fivecolumns-lg</xsl:attribute>
      </xsl:if>
      <div id="column1-{@id}" class="column1 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column3-{@id}" class="column3 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column3-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column3 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column4-{@id}" class="column4 {$responsiveColumns}">

        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column4-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column4 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column5-{@id}" class="column5 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column5-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column5 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='6columns']" mode="displayBrief">
    <xsl:variable name="responsiveColumns">
      <xsl:apply-templates select="." mode="responsiveColumns">
        <xsl:with-param name="defaultCols" select="'2'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row">
      <div id="column1-{@id}" class="column1 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column3-{@id}" class="column3 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column3-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column3 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column4-{@id}" class="column4 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column4-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column4 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column5-{@id}" class="column5 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column5-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column5 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column6-{@id}" class="column6 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column6-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column6 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <!-- ACCORDION -->
  <xsl:template match="Content[@moduleType='Accordion']" mode="displayBrief">
    <div class="panel-group" id="accordion-{@id}">
      <xsl:apply-templates select="/Page" mode="addModule">
        <xsl:with-param name="text">Add Module</xsl:with-param>
        <xsl:with-param name="position">
          <xsl:text>accordion-</xsl:text>
          <xsl:value-of select="@id"/>
        </xsl:with-param>
        <xsl:with-param name="class">
          <xsl:text>panel-group</xsl:text>
        </xsl:with-param>
        <xsl:with-param name="id">
          <xsl:text>accordion</xsl:text>
        </xsl:with-param>
      </xsl:apply-templates>
    </div>
  </xsl:template>

  <xsl:template match="Content[starts-with(@position,'accordion')]" mode="displayModule">
    <xsl:variable name="contentPosition">
      <xsl:value-of select="@position"/>
    </xsl:variable>
    <xsl:variable name="containerID">
      <xsl:value-of select="substring-after(@position, '-')"/>
    </xsl:variable>
    <xsl:variable name="open">
      <xsl:choose>
        <xsl:when test="/Page/descendant::Content[@id=$containerID and @open='true']">
          <xsl:text>true</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div id="mod_{@id}" class="panel panel-default">
      <!-- define classes for box -->
      <xsl:attribute name="class">
        <xsl:text>panel </xsl:text>
        <xsl:choose>
          <xsl:when test="@box='Default Box'">panel-default</xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="translate(@box,' ','-')"/>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text> module</xsl:text>
        <!-- if no title, we may still want TL/TR for rounded boxs with no title bar,
              stled differently to a title bar. -->
        <xsl:if test="@title=''">
          <xsl:text> boxnotitle</xsl:text>
        </xsl:if>
        pos-<xsl:value-of select="@position"/>
        <xsl:if test="@modAnim and @modAnim!=''">
          <xsl:text> moduleAnimate-invisible</xsl:text>
        </xsl:if>
        <xsl:apply-templates select="." mode="hideScreens" />
        <xsl:apply-templates select="." mode="marginBelow" />
      </xsl:attribute>
      <div class="panel-heading">
        <xsl:apply-templates select="." mode="inlinePopupOptions">
          <xsl:with-param name="class" select="'panel-heading'"/>
        </xsl:apply-templates>
        <xsl:if test="@rss and @rss!='false'">
          <xsl:apply-templates select="." mode="rssLink" />
        </xsl:if>
        <a data-toggle="collapse" data-parent="#{@position}" href="#collapse{@id}" class="accordion-load">
          <xsl:if test="$open='true'">
            <xsl:if test="count(./preceding-sibling::Content[@position=$contentPosition])=0">
              <xsl:attribute name="class">
                <xsl:value-of select="@position"/>
                <xsl:text> accordion-open</xsl:text>
              </xsl:attribute>
            </xsl:if>
          </xsl:if>
          <h3 class="panel-title">
            <!--<i class="fa fa-ellipsis-v">&#160;</i>-->
            <i class="fa fa-caret-down">
              <xsl:text> </xsl:text>
            </i>
            <span class="space">&#160;</span>
            <!--<xsl:apply-templates select="." mode="getDisplayName"/>-->
            <xsl:value-of select="@title"/>
          </h3>
        </a>
      </div>
      <div id="collapse{@id}" class="panel-collapse collapse">
        <xsl:if test="$open='true'">
          <xsl:if test="count(./preceding-sibling::Content[@position=$contentPosition])=0">
            <xsl:attribute name="class">
              <xsl:value-of select="@position"/>
              <xsl:text> panel-collapse collapse in</xsl:text>
            </xsl:attribute>
          </xsl:if>
        </xsl:if>
        <xsl:if test="not(@listGroup='true')">
          <div class="panel-body">
            <xsl:if test="not(@title!='')">
              <xsl:apply-templates select="." mode="inlinePopupOptions">
                <xsl:with-param name="class" select="'panel-body'"/>
              </xsl:apply-templates>
            </xsl:if>
            <xsl:apply-templates select="." mode="displayBrief"/>
          </div>
        </xsl:if>
        <xsl:if test="@listGroup='true'">
          <div class="list-group">
            <xsl:if test="not(@title!='')">
              <xsl:apply-templates select="." mode="inlinePopupOptions">
                <xsl:with-param name="class" select="'list-group'"/>
              </xsl:apply-templates>
            </xsl:if>
            <xsl:apply-templates select="." mode="displayBrief"/>
          </div>
        </xsl:if>
        <xsl:if test="@linkText!='' and @link!=''">
          <div class="panel-footer">
            <div class="entryFooter">
              <xsl:apply-templates select="." mode="moreLink">
                <xsl:with-param name="link">
                  <xsl:choose>
                    <xsl:when test="format-number(@link,'0')!='NaN'">
                      <xsl:variable name="pageId" select="@link"/>
                      <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]" mode="getHref"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="@link"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:with-param>
                <xsl:with-param name="linkText" select="@linkText"/>
                <xsl:with-param name="altText" select="@title"/>
              </xsl:apply-templates>
              <xsl:text> </xsl:text>
            </div>
          </div>
        </xsl:if>
      </div>
    </div>
  </xsl:template>

  <!-- TABBED-->
  <xsl:template match="Content[@moduleType='Tabbed']" mode="displayBrief">
    <xsl:variable name="containerID">
      <xsl:value-of select="@id"/>
    </xsl:variable>
    <ul class="nav nav-tabs responsive">
      <!--<xsl:for-each select="/Page/Contents/Content[starts-with(@position,'tabbed')]">-->
      <xsl:for-each select="/Page/Contents/Content[contains(@position, $containerID)]">
        <li>
          <xsl:if test="count(./preceding-sibling::Content[contains(@position, $containerID)])=0">
            <xsl:attribute name="class">
              <xsl:text>active</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <a href="#{@id}" data-toggle="tab">
            <xsl:if test="@icon!=''">
              <i>
                <xsl:attribute name="class">
                  <xsl:text>fa fa-3x center-block </xsl:text>
                  <xsl:value-of select="@icon"/>
                </xsl:attribute>
                <xsl:text> </xsl:text>
              </i>
              <xsl:text> </xsl:text>
            </xsl:if>
            <xsl:if test="@uploadIcon!='' and @uploadIcon!='_'">
              <span class="upload-icon">
                <img src="{@uploadIcon}" alt="icon" class="center-block img-responsive"/>
              </span>
            </xsl:if>
            <!--<xsl:apply-templates select="." mode="getDisplayName"/>-->
            <xsl:value-of select="@title"/>
          </a>
        </li>
      </xsl:for-each>
    </ul>
    <div id="tabbed-{@id}" class="tab-content responsive">
      <xsl:apply-templates select="/Page" mode="addModule">
        <xsl:with-param name="text">Add Tab</xsl:with-param>
        <xsl:with-param name="position">
          <xsl:text>tabbed-</xsl:text>
          <xsl:value-of select="@id"/>
        </xsl:with-param>
        <xsl:with-param name="class">
          <xsl:text>tab-content</xsl:text>
        </xsl:with-param>
        <xsl:with-param name="id">
          <xsl:text>tabbed</xsl:text>
        </xsl:with-param>
      </xsl:apply-templates>
      <xsl:text> </xsl:text>
    </div>
  </xsl:template>

  <!--tabbed with and without box-->
  <xsl:template match="Content[starts-with(@position,'tabbed')]" mode="displayModule">
    <xsl:variable name="contentPosition">
      <xsl:value-of select="@position"/>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="@box!='false' and @box!=''">
        <xsl:apply-templates select="." mode="moduleBox"/>
      </xsl:when>
      <xsl:otherwise>
        <div class="tab-pane {$contentPosition}">
          <xsl:attribute name="id">
            <xsl:value-of select="@id"/>
          </xsl:attribute>
          <xsl:if test="count(./preceding-sibling::Content[@position=$contentPosition])=0">
            <xsl:attribute name="class">
              <xsl:value-of select="@position"/>
              <xsl:text> tab-pane active</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <div id="mod_{@id}" class="module nobox pos-{@position}">
            <xsl:attribute name="class">
              <xsl:text>module nobox pos-</xsl:text>
              <xsl:value-of select="@position"/>
              <xsl:if test="@modAnim and @modAnim!=''">
                <xsl:text> moduleAnimate-invisible</xsl:text>
              </xsl:if>
              <xsl:apply-templates select="." mode="hideScreens" />
              <xsl:apply-templates select="." mode="marginBelow" />
            </xsl:attribute>
            <xsl:if test="@contentType='Module'">
              <xsl:attribute name="class">
                <xsl:text>module nobox layoutModule pos-</xsl:text>
                <xsl:value-of select="@position"/>
                <xsl:if test="@modAnim and @modAnim!=''">
                  <xsl:text> moduleAnimate-invisible</xsl:text>
                </xsl:if>
                <xsl:apply-templates select="." mode="hideScreens" />
                <xsl:apply-templates select="." mode="marginBelow" />
              </xsl:attribute>
            </xsl:if>
            <xsl:choose>
              <xsl:when test="/Page/AdminMenu/descendant-or-self::MenuItem[@cmd='Normal'] and $adminMode">
                <div>
                  <xsl:apply-templates select="." mode="inlinePopupOptions" />
                  <xsl:text> </xsl:text>
                  <xsl:if test="(@title!='' and @moduleType!='Image') or @icon!='' or @uploadIcon!=''">
                    <h3 class="title">
                      <xsl:apply-templates select="." mode="moduleLink"/>
                    </h3>
                  </xsl:if>
                </div>
              </xsl:when>
              <xsl:otherwise>
                <xsl:if test="(@title!='' and @moduleType!='Image') or @icon!='' or @uploadIcon!=''">
                  <h3 class="title">
                    <xsl:apply-templates select="." mode="moduleLink"/>
                  </h3>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>
            <xsl:if test="@rss and @rss!='false'">
              <xsl:apply-templates select="." mode="rssLink" />
            </xsl:if>
            <div class="terminus">&#160;</div>
            <xsl:apply-templates select="." mode="displayBrief"/>
            <xsl:if test="@linkText!='' and @link!=''">
              <div class="entryFooter">
                <xsl:apply-templates select="." mode="moreLink">
                  <xsl:with-param name="link">
                    <xsl:choose>
                      <xsl:when test="format-number(@link,'0')!='NaN'">
                        <xsl:variable name="pageId" select="@link"/>
                        <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]" mode="getHref"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="@link"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:with-param>
                  <xsl:with-param name="linkText" select="@linkText"/>
                  <xsl:with-param name="altText" select="@title"/>
                </xsl:apply-templates>
                <xsl:text> </xsl:text>
              </div>
            </xsl:if>
            <div class="terminus">&#160;</div>
          </div>
        </div>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!--end tabbed with and without box-->

  <xsl:template match="Content[starts-with(@position,'tabbed')]" mode="moduleBox">
    <xsl:variable name="contentPosition">
      <xsl:value-of select="@position"/>
    </xsl:variable>
    <div class="tab-pane {$contentPosition}">
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:if test="count(./preceding-sibling::Content[@position=$contentPosition])=0">
        <xsl:attribute name="class">
          <xsl:value-of select="@position"/>
          <xsl:text> tab-pane active</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <div id="mod_{@id}" class="panel panel-default">
        <!-- define classes for box -->
        <xsl:attribute name="class">
          <xsl:text>panel </xsl:text>
          <xsl:choose>
            <xsl:when test="@box='Default Box'">panel-default</xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="translate(@box,' ','-')"/>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:text> module</xsl:text>
          <!-- if no title, we may still want TL/TR for rounded boxs with no title bar,
              stled differently to a title bar. -->
          <xsl:if test="@title=''">
            <xsl:text> boxnotitle</xsl:text>
          </xsl:if>
          pos-<xsl:value-of select="@position"/>
          <xsl:apply-templates select="." mode="hideScreens" />
          <xsl:apply-templates select="." mode="marginBelow" />
        </xsl:attribute>
        <div class="panel-heading">
          <xsl:apply-templates select="." mode="inlinePopupOptions">
            <xsl:with-param name="class" select="'panel-heading'"/>
          </xsl:apply-templates>
          <xsl:if test="@rss and @rss!='false'">
            <xsl:apply-templates select="." mode="rssLink" />
          </xsl:if>
          <h3 class="panel-title">
            <xsl:apply-templates select="." mode="getDisplayName"/>
          </h3>
        </div>
        <xsl:if test="not(@listGroup='true')">
          <div class="panel-body">
            <xsl:if test="not(@title!='')">
              <xsl:apply-templates select="." mode="inlinePopupOptions">
                <xsl:with-param name="class" select="'panel-body'"/>
              </xsl:apply-templates>
            </xsl:if>
            <xsl:apply-templates select="." mode="displayBrief"/>
          </div>
        </xsl:if>
        <xsl:if test="@listGroup='true'">
          <div class="list-group">
            <xsl:if test="not(@title!='')">
              <xsl:apply-templates select="." mode="inlinePopupOptions">
                <xsl:with-param name="class" select="'list-group'"/>
              </xsl:apply-templates>
            </xsl:if>
            <xsl:apply-templates select="." mode="displayBrief"/>
          </div>
        </xsl:if>
        <xsl:if test="@linkText!='' and @link!=''">
          <div class="panel-footer">
            <div class="entryFooter">
              <xsl:apply-templates select="." mode="moreLink">
                <xsl:with-param name="link">
                  <xsl:choose>
                    <xsl:when test="format-number(@link,'0')!='NaN'">
                      <xsl:variable name="pageId" select="@link"/>
                      <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]" mode="getHref"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="@link"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:with-param>
                <xsl:with-param name="linkText" select="@linkText"/>
                <xsl:with-param name="altText" select="@title"/>
              </xsl:apply-templates>
              <xsl:text> </xsl:text>
            </div>
          </div>
        </xsl:if>
      </div>
    </div>
  </xsl:template>

  <!-- ## Generic displayBrief for Formatted Text and Images   #####################################################################   -->
  <xsl:template match="Content[@moduleType='FormattedText']" mode="displayBrief">
    <xsl:apply-templates select="node()" mode="cleanXhtml"/>
  </xsl:template>

  <!-- ## Generic displayBrief for Formatted Text and Images   #####################################################################   -->
  <xsl:template match="Content[@moduleType='FormattedCode']" mode="displayBrief">
    <pre class="code">
      <xsl:apply-templates select="node()" mode="cleanXhtml"/>
    </pre>
  </xsl:template>

  <!-- ## IMAGE BASIC CONTENT TYPE - Cater for a link  ###############################################   -->
  <xsl:template match="Content[@type='Image']" mode="displayBrief">
    <xsl:param name="maxWidth"/>
    <xsl:param name="maxHeight"/>
    <xsl:param name="noLazy"/>
    <xsl:choose>
      <xsl:when test="@internalLink!=''">
        <xsl:variable name="pageId" select="@internalLink"/>
        <xsl:variable name="href">
          <xsl:apply-templates select="//MenuItem[@id=$pageId]" mode="getHref" />
        </xsl:variable>
        <xsl:variable name="title">
          <xsl:apply-templates select="//MenuItem[@id=$pageId]" mode="getTitleAttr" />
        </xsl:variable>

        <a href="{$href}" title="{$title}">
          <xsl:choose>
            <xsl:when test="img[contains(@src,'.svg')]">
              <svg id="svg-{@position}" width="{img/@width}" height="{img/@height}" viewbox="0 0 {img/@width} {img/@height}" xmlns="http://www.w3.org/2000/svg" xmlns:ev="http://www.w3.org/2001/xml-events" xmlns:xlink="http://www.w3.org/1999/xlink">
                <image id="svg-img-{@position}" xlink:href="{img/@src}" src="{@svgFallback}" width="{img/@width}" height="{img/@height}" class="img-responsive">
                  <xsl:text> </xsl:text>
                </image>
              </svg>
            </xsl:when>
            <xsl:when test="@resize='true'">
              <xsl:apply-templates select="." mode="resize-image">
                <xsl:with-param name="noLazy" select="$noLazy"/>
              </xsl:apply-templates>
            </xsl:when>
            <xsl:when test="$maxWidth!='' or $maxHeight!=''">
              <xsl:apply-templates select="." mode="resize-image">
                <xsl:with-param name="maxWidth" select="$maxWidth"/>
                <xsl:with-param name="maxHeight" select="$maxHeight"/>
                <xsl:with-param name="noLazy" select="$noLazy"/>
              </xsl:apply-templates>
            </xsl:when>
            <xsl:otherwise>
              <xsl:apply-templates select="./node()" mode="cleanXhtml"/>
            </xsl:otherwise>
          </xsl:choose>
        </a>
      </xsl:when>
      <xsl:when test="@externalLink!=''">
        <a href="{@externalLink}" title="Go to {@externalLink}">
          <xsl:if test="not(contains(@externalLink,/Page/Request/ServerVariables/Item[@name='SERVER_NAME']/node())) and contains(@externalLink,'http')">
            <xsl:attribute name="rel">external</xsl:attribute>
            <!-- All browsers open rel externals as new windows anyway. Target not a valid attribute -->
          </xsl:if>
          <xsl:apply-templates select="./node()" mode="cleanXhtml">
            <xsl:with-param name="noLazy" select="'true'"/>
          </xsl:apply-templates>
        </a>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="node()" mode="cleanXhtml"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="Content[@type='Module' and @moduleType='FormattedText']" mode="displayBrief">
    <xsl:if test="node()">
      <div class="FormattedText">
        <xsl:if test="@maxWidth!=''">
          <xsl:choose>
            <xsl:when test="@iconStyle='Centre' or @iconStyle='CentreSmall'">
              <xsl:attribute name="class">FormattedText central-text</xsl:attribute>
              <xsl:attribute name='style'>
                <xsl:text>max-width:</xsl:text>
                <xsl:value-of select="@maxWidth"/>
              </xsl:attribute>
            </xsl:when>
            <xsl:otherwise>
              <xsl:attribute name='style'>
                <xsl:text>max-width:</xsl:text>
                <xsl:value-of select="@maxWidth"/>
              </xsl:attribute>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>
        <xsl:apply-templates select="node()" mode="cleanXhtml"/>
      </div>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Content[@type='Module' and @moduleType='Image']" mode="moduleTitle">
    <xsl:value-of select="@title"/>
  </xsl:template>

  <xsl:template match="Content[@moduleType='Image']" mode="displayBrief">
    <xsl:param name="maxWidth"/>
    <xsl:param name="maxHeight"/>
    <xsl:apply-templates select="." mode="displayBriefImg">
      <xsl:with-param name="maxWidth" select="$maxWidth"/>
      <xsl:with-param name="maxHeight" select="$maxHeight"/>
    </xsl:apply-templates>
  </xsl:template>

  <xsl:template match="Content[@moduleType='Image']" mode="displayBriefImg">
    <xsl:param name="maxWidth"/>
    <xsl:param name="maxHeight"/>
    <xsl:variable name="crop">
      <xsl:choose>
        <xsl:when test="@crop='true'">true</xsl:when>
        <xsl:otherwise>false</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="no-stretch">
      <xsl:choose>
        <xsl:when test="@stretch='true'">false</xsl:when>
        <xsl:otherwise>true</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test="img/@src!=''">
      <xsl:choose>
        <xsl:when test="@resize='true'">
          <xsl:apply-templates select="." mode="resize-image">
            <xsl:with-param name="crop" select="$crop"/>
            <xsl:with-param name="no-stretch" select="$no-stretch"/>
          </xsl:apply-templates>
        </xsl:when>
        <xsl:when test="$maxWidth!='' or $maxHeight!=''">
          <xsl:apply-templates select="." mode="resize-image">
            <xsl:with-param name="maxWidth" select="$maxWidth"/>
            <xsl:with-param name="maxHeight" select="$maxHeight"/>
            <xsl:with-param name="crop" select="$crop"/>
          </xsl:apply-templates>
        </xsl:when>
        <xsl:when test="(@imgDetail and @imgDetail!='') or @lightbox='true'">
          <xsl:choose>
            <xsl:when test="@imgDetail and @imgDetail!=''">
              <!--<xsl:apply-templates select="node()" mode="cleanXhtml"/>-->
              <a href="{@imgDetail}" title="{@title}" class="responsive-lightbox">
                <!--<img src="{@imgDetail}" alt="{@title}"/>-->

                <xsl:apply-templates select="node()" mode="cleanXhtml"/>
              </a>
            </xsl:when>
            <xsl:otherwise>
              <a href="{node()/@src}" title="{@title}" class="responsive-lightbox">
                <xsl:apply-templates select="node()" mode="cleanXhtml"/>
              </a>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="node()" mode="cleanXhtml"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>

  <!-- Image Module Display with Link -->
  <xsl:template match="Content[@type='Module' and @moduleType='Image' and @link!='']" mode="displayBrief">
    <xsl:param name="maxWidth"/>
    <xsl:param name="maxHeight"/>
    <xsl:param name="lazy"/>
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
      <xsl:attribute name="title">
        <xsl:choose>
          <xsl:when test="format-number(@link,'0')!='NaN'">
            <xsl:variable name="pageId" select="@url"/>
            <xsl:value-of select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]/@name"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="@linkText"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <xsl:if test="@linkType='external' and starts-with(@link,'http')">
        <xsl:attribute name="rel">external</xsl:attribute>
      </xsl:if>
      <xsl:apply-templates select="." mode="displayBriefImg">
        <xsl:with-param name="maxWidth" select="$maxWidth"/>
        <xsl:with-param name="maxHeight" select="$maxHeight"/>
      </xsl:apply-templates>
    </a>
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


  <!-- ############## News Articles ##############   -->
  <!-- NewsArticle Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='NewsList']" mode="displayBrief">
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
    <div class="clearfix NewsList">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix NewsList content-scroller</xsl:text>
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
        <xsl:choose>
          <xsl:when test="@linkArticle='true'">
            <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefLinked">
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
          </xsl:otherwise>
        </xsl:choose>
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

  <xsl:template match="Content[@type='Module' and @moduleType='NewsListDateMenu']" mode="displayBrief">
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="dateQuery" select="@dateQuery"/>
    <div class="NewsListDateMenu" id="subMenu">
      <ul class="nav nav-pills nav-stacked">
        <xsl:apply-templates select="Menu/MenuItem" mode="submenuitem2"/>
      </ul>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='Module' and @title='NewsListDateMenu']" mode="moduleTitle">
    <xsl:variable name="dateQuery" select="$page/Contents/Content[@type='Module' and @moduleType='NewsListDateMenu']/@dateQuery"/>
    <xsl:variable name="dateTitle">
      <xsl:for-each select="$page/Contents/Content[@type='Module' and @moduleType='NewsListDateMenu']/Menu/MenuItem[@id=$dateQuery]">
        <xsl:value-of select="@name"/>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="title">
      <span>
        <xsl:value-of select="$dateTitle"/>
        <xsl:text> </xsl:text>
      </span>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="@iconStyle='Centre'">
        <div class="center-block">
          <xsl:if test="@icon!=''">
            <i>
              <xsl:attribute name="class">
                <xsl:text>fa fa-3x center-block </xsl:text>
                <xsl:value-of select="@icon"/>
              </xsl:attribute>
              <xsl:text> </xsl:text>
            </i>
            <xsl:text> </xsl:text>
          </xsl:if>
          <xsl:if test="@uploadIcon!='' and @uploadIcon!='_'">
            <span class="upload-icon">
              <img src="{@uploadIcon}" alt="icon" class="center-block img-responsive"/>
            </span>
          </xsl:if>
          <xsl:if test="@title!=''">
            <span>
              <xsl:copy-of select="ms:node-set($title)" />
              <xsl:text> </xsl:text>
            </span>
          </xsl:if>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <xsl:if test="@icon!=''">
          <i>
            <xsl:attribute name="class">
              <xsl:text>fa </xsl:text>
              <xsl:value-of select="@icon"/>
            </xsl:attribute>
            <xsl:text> </xsl:text>
          </i>
          <span class="space">&#160;</span>
        </xsl:if>
        <xsl:if test="@uploadIcon!='' and @uploadIcon!='_'  and @uploadIcon!=' '">
          <img src="{@uploadIcon}" alt="icon"/>
        </xsl:if>
        <xsl:copy-of select="ms:node-set($title)" />
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="MenuItem" mode="submenuitem2">
    <xsl:variable name="dateQuery" select="ancestor::Content/@dateQuery"/>
    <li>
      <xsl:variable name="class">
        <xsl:if test="position()=1">
          <xsl:text>first </xsl:text>
        </xsl:if>
        <xsl:if test="position()=last()">
          <xsl:text>last </xsl:text>
        </xsl:if>
        <xsl:if test="@id=$dateQuery">
          <xsl:text>active </xsl:text>
        </xsl:if>
        <xsl:if test="descendant::MenuItem[@id=$dateQuery] and @url!='/'">
          <xsl:text>active </xsl:text>
        </xsl:if>
      </xsl:variable>
      <xsl:apply-templates select="self::MenuItem" mode="menuLink">
        <xsl:with-param name="class" select="$class"/>
      </xsl:apply-templates>
      <xsl:if test="count(child::MenuItem[not(DisplayName/@exclude='true')])&gt;0 and descendant-or-self::MenuItem[@id=/Page/@id]">
        <ul>
          <xsl:attribute name="class">
            <xsl:text>nav nav-pills</xsl:text>
            <!--TS Theme specfic setting must not be here - Moved to Layout XSL -->
          </xsl:attribute>
          <xsl:apply-templates select="MenuItem[not(DisplayName/@exclude='true')]" mode="submenuitem"/>
        </ul>
      </xsl:if>
    </li>
  </xsl:template>



  <!-- NewsArticle Brief -->
  <xsl:template match="Content[@type='NewsArticle']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <!-- articleBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="listItem list-group-item newsarticle">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem list-group-item NewsArticle'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner media">
        <xsl:if test="Images/img/@src!=''">
          <a href="{$parentURL}" title="Read More - {Headline/node()}">
            <xsl:apply-templates select="." mode="displayThumbnail"/>
          </a>
          <!--Accessiblity fix : Separate adjacent links with more than whitespace-->
          <span class="hidden">|</span>
        </xsl:if>
        <div class="media-body">
          <h4 class="media-heading" itemprop="headline">
            <a href="{$parentURL}" title="Read More - {Headline/node()}">
              <xsl:apply-templates select="." mode="getDisplayName"/>
            </a>
          </h4>
          <span class="hidden" itemtype="Organization" itemprop="publisher">
            <span itemprop="name">
              <xsl:value-of select="$sitename"/>
            </span>
          </span>
          <xsl:apply-templates select="Content[@type='Contact' and @rtype='Author'][1]" mode="displayAuthorBrief"/>
          <xsl:if test="@publish!=''">
            <p class="date" itemprop="datePublished">
              <xsl:value-of select="/Page/Contents/Content[@name='articleLabel']"/>
              <xsl:call-template name="DisplayDate">
                <xsl:with-param name="date" select="@publish"/>
              </xsl:call-template>
            </p>
          </xsl:if>
          <xsl:if test="@update!=''">
            <p class="hidden" itemprop="dateModified">
              <xsl:value-of select="/Page/Contents/Content[@name='articleLabel']"/>
              <xsl:call-template name="DisplayDate">
                <xsl:with-param name="date" select="@update"/>
              </xsl:call-template>
            </p>
          </xsl:if>

          <xsl:if test="Strapline/node()!=''">
            <div class="summary" itemprop="description">
              <xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
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
        </div>
        <!-- Accessiblity fix : Separate adjacent links with more than whitespace -->
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='NewsArticle']" mode="displayBriefLinked">
    <xsl:param name="sortBy"/>
    <xsl:param name="link"/>
    <xsl:param name="altText"/>
    <xsl:param name="linkType"/>
    <!-- articleBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="listItem list-group-item newsarticle">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem list-group-item newsarticle'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>

      </xsl:apply-templates>
      <xsl:choose>
        <xsl:when test="Strapline/descendant-or-self::a">
          <div class="straphaslinks">
            <xsl:if test="not(substring($link,1,1)='/') and (contains($link,'http://') and $linkType='external')">
              <xsl:attribute name="rel">external</xsl:attribute>
              <xsl:attribute name="class">extLink listItem list-group-item newsarticle straphaslinks</xsl:attribute>
            </xsl:if>
            <div class="lIinner">
              <h3 class="title">
                <a href="{$parentURL}">
                  <xsl:apply-templates select="." mode="getDisplayName"/>
                </a>
              </h3>
              <xsl:if test="Images/img/@src!=''">
                <xsl:apply-templates select="." mode="displayThumbnail"/>
                <!--Accessiblity fix : Separate adjacent links with more than whitespace-->
                <span class="hidden">|</span>
              </xsl:if>
              <xsl:apply-templates select="Content[@type='Contact']" mode="displayAuthorBrief"/>
              <xsl:if test="@publish!=''">
                <p class="date">
                  <xsl:value-of select="/Page/Contents/Content[@name='articleLabel']"/>
                  <xsl:call-template name="DisplayDate">
                    <xsl:with-param name="date" select="@publish"/>
                  </xsl:call-template>
                </p>
              </xsl:if>
              <xsl:if test="Strapline/node()!=''">
                <div class="summary">
                  <xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
                </div>
              </xsl:if>
              <!-- Accessiblity fix : Separate adjacent links with more than whitespace -->
              <div class="terminus">&#160;</div>
            </div>
          </div>
        </xsl:when>
        <xsl:otherwise>
          <a href="{$parentURL}">
            <xsl:if test="not(substring($link,1,1)='/') and (contains($link,'http://') and $linkType='external')">
              <xsl:attribute name="rel">external</xsl:attribute>
              <xsl:attribute name="class">extLink listItem list-group-item newsarticle</xsl:attribute>
            </xsl:if>
            <div class="lIinner">
              <h3 class="title">
                <xsl:apply-templates select="." mode="getDisplayName"/>
              </h3>
              <xsl:if test="Images/img/@src!=''">
                <xsl:apply-templates select="." mode="displayThumbnail"/>
                <!--Accessiblity fix : Separate adjacent links with more than whitespace-->
                <span class="hidden">|</span>
              </xsl:if>
              <xsl:apply-templates select="Content[@type='Contact']" mode="displayAuthorBrief"/>
              <xsl:if test="@publish!=''">
                <p class="date">
                  <xsl:value-of select="/Page/Contents/Content[@name='articleLabel']"/>
                  <xsl:call-template name="DisplayDate">
                    <xsl:with-param name="date" select="@publish"/>
                  </xsl:call-template>
                </p>
              </xsl:if>
              <xsl:if test="Strapline/node()!=''">
                <div class="summary">
                  <xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
                </div>
              </xsl:if>
              <xsl:apply-templates select="." mode="displayTagsNoLink"/>
              <!-- Accessiblity fix : Separate adjacent links with more than whitespace -->
              <div class="terminus">&#160;</div>
            </div>
          </a>
        </xsl:otherwise>
      </xsl:choose>

    </div>
  </xsl:template>

  <!-- NewsArticle Detail -->
  <xsl:template match="Content[@type='NewsArticle']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <xsl:variable name="debugMode">
      <xsl:call-template name="getXmlSettings">
        <xsl:with-param name="sectionName">web</xsl:with-param>
        <xsl:with-param name="valueName">debug</xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <div class="detail newsarticle">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail newsarticle'"/>
      </xsl:apply-templates>
      <h2 class="entry-title content-title" itemprop="headline">
        <xsl:apply-templates select="." mode="getDisplayName" />
      </h2>
      <xsl:apply-templates select="." mode="displayDetailImage"/>
      <xsl:apply-templates select="Content[@type='Contact']" mode="displayAuthor"/>
      <xsl:if test="@publish!=''">
        <p class="dtstamp" title="{@publish}" itemprop="datePublished">
          <xsl:call-template name="DisplayDate">
            <xsl:with-param name="date" select="@publish"/>
          </xsl:call-template>
        </p>
      </xsl:if>
      <span class="strapline-detail" itemprop="description">
        <xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
      </span>
      <div class="description entry-content" itemprop="text">
        <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
      </div>
      <xsl:if test="Content[@type='FAQ']">
        <div class="faq-list">
          <a name="pageTop" class="pageTop">&#160;</a>
          <h3>Question and Answer</h3>
          <ul>
            <xsl:apply-templates select="Content[@type='FAQ']" mode="displayFAQMenu"/>
          </ul>
          <xsl:apply-templates select="Content[@type='FAQ']" mode="displayBrief">
            <xsl:with-param name="sortBy" select="@sortBy"/>
          </xsl:apply-templates>
        </div>
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
      <xsl:apply-templates select="." mode="ContentDetailCommenting">
        <xsl:with-param name="commentPlatform" select="$page/Contents/Content[@moduleType='NewsList']/@commentPlatform"/>
      </xsl:apply-templates>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='NewsArticle' and ancestor::ContentDetail]" mode="JSONLD">
    [ { "@context": "https://schema.org",
    "@type": "BlogPosting",
    "mainEntityOfPage": {
    "@type": "WebPage",
    "@id": "<xsl:value-of select="$href"/>"
    },
    "headline": "<xsl:apply-templates select="." mode="getDisplayName" />",
    "alternativeHeadline": "<xsl:call-template name="escape-json">
      <xsl:with-param name="string">
        <xsl:apply-templates select="Strapline" mode="flattenXhtml"/>
      </xsl:with-param>
    </xsl:call-template>",
    "image": "<xsl:value-of select="Images/img[@class='detail']/@src"/>",
    <xsl:if test="Content[@type='Tag']">
      <xsl:for-each select="Content[@type='Tag'][1]">
        <xsl:text>"genre": "</xsl:text>
        <xsl:value-of select="Name"/>
        <xsl:text>",
        </xsl:text>
      </xsl:for-each>
    </xsl:if>
    <xsl:if test="@metaKeywords!=''">
      "keywords": "<xsl:value-of select="@metaKeywords"/>",
    </xsl:if>
    "publisher": {
    "@type": "Organization",
    "name": "<xsl:value-of select="$siteName"/>",
    "logo":{
    "@type": "ImageObject",
    "name": "<xsl:value-of select="$siteName"/> Logo",
    "url": "<xsl:value-of select="$siteURL"/><xsl:value-of select="$siteLogo"/>"
    }},
    "url": "<xsl:value-of select="$href"/>",
    "datePublished": "<xsl:value-of select="@publish"/>",
    "dateCreated": "<xsl:value-of select="@publish"/>",
    "dateModified": "<xsl:value-of select="@update"/>",
    <xsl:if test="@metaDescription!=''">
      "description": "<xsl:value-of select="@metaDescription"/>",
    </xsl:if>
    "articleBody": "<xsl:call-template name="escape-json">
      <xsl:with-param name="string">
        <xsl:apply-templates select="Body/*" mode="flattenXhtml"/>
      </xsl:with-param>
    </xsl:call-template>"
    <xsl:if test="Content[@type='Contact' and @rtype='Author']">
      ,
      <xsl:apply-templates select="Content[@type='Contact' and @rtype='Author']" mode="JSONLD"/>
    </xsl:if>}
    <xsl:if test="Content[@type='FAQ']">
      ,  { "@context": "https://schema.org",
      "@type": "FAQPage",
      "mainEntity": [
      <xsl:apply-templates select="Content[@type='FAQ']" mode="JSONLD-list"/>
      ]
      }
    </xsl:if>
    ]
  </xsl:template>

  <xsl:template match="Content[@type='Contact' and ancestor::Content[@type='NewsArticle']]" mode="JSONLD">
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="self::Content" mode="getHref">
        <xsl:with-param name="parId" select="@parId"/>
      </xsl:apply-templates>
    </xsl:variable>
    "author": {
    "@type": "Person",
    "name": "<xsl:value-of select="GivenName"/><xsl:text> </xsl:text><xsl:value-of select="Surname"/>",
    "jobTitle": "<xsl:value-of select="Title"/>",
    "image": "<xsl:value-of select="$siteURL"/><xsl:value-of select="Images/img[@class='detail']/@src"/>",
    "url": "<xsl:value-of select="$parentURL"/>",
    "sameAs" : [
    <xsl:if test="@facebookURL!=''">
      "<xsl:value-of select="@facebookURL"/>"
    </xsl:if>
    <xsl:if test="@linkedInURL!=''">
      "<xsl:value-of select="@linkedInURL"/>"
    </xsl:if>
    <xsl:if test="@twitterURL!=''">
      "<xsl:value-of select="@twitterURL"/>"
    </xsl:if>
    <xsl:if test="@instagramURL!=''">
      "<xsl:value-of select="@instagramURL"/>"
    </xsl:if>]
    }
  </xsl:template>

  <xsl:template match="Content" mode="ContentDetailCommenting">
    <xsl:param name="commentPlatform"/>
    <xsl:variable name="debugMode">
      <xsl:call-template name="getXmlSettings">
        <xsl:with-param name="sectionName">web</xsl:with-param>
        <xsl:with-param name="valueName">debug</xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="$commentPlatform='facebook'">
        <div class="fb-comments" data-href="{$href}" data-num-posts="3" data-width="{$page/Contents/Content[@moduleType='NewsList']/@fbCommentsWidth}">
          <xsl:text> </xsl:text>
        </div>
      </xsl:when>
      <xsl:when test="$commentPlatform='disqus'">
        <div id="disqus_thread">
          <xsl:text> </xsl:text>
        </div>
        <script type="text/javascript">
          /* * * CONFIGURATION VARIABLES: EDIT BEFORE PASTING INTO YOUR WEBPAGE * * */
          var disqus_shortname = '<xsl:value-of select="$page/Contents/Content[@moduleType='NewsList' and @commentPlatform='disqus']/@disqusShortname"/>'; // required: replace example with your forum shortname
          var disqus_identifier = 'pageid-<xsl:value-of select="$page/@id"/>-artid-<xsl:value-of select="@id"/>';
          var disqus_title = '<xsl:apply-templates select="." mode="getDisplayName" />';
          var disqus_url = 'http://<xsl:value-of select="$siteURL"/><xsl:value-of select="$href"/>';
          <xsl:choose>
            <xsl:when test="$debugMode='on'">var disqus_developer = 1;</xsl:when>
            <xsl:otherwise>var disqus_developer = 0;</xsl:otherwise>
          </xsl:choose>

          /* * * DON'T EDIT BELOW THIS LINE * * */
          (function() {
          var dsq = document.createElement('script'); dsq.type = 'text/javascript'; dsq.async = true;
          dsq.src = 'http://' + disqus_shortname + '.disqus.com/embed.js';
          (document.getElementsByTagName('head')[0] || document.getElementsByTagName('body')[0]).appendChild(dsq);
          })();
        </script>
        <noscript>
          Please enable JavaScript to view the <a href="http://disqus.com/?ref_noscript">comments powered by Disqus.</a>
        </noscript>
        <a href="http://disqus.com" class="dsq-brlink">
          comments powered by <span class="logo-disqus">Disqus</span>
        </a>
      </xsl:when>
      <xsl:when test="$commentPlatform='livefyre'">
        <!-- START: Livefyre Embed -->
        <div id="livefyre-comments">
          <xsl:text> </xsl:text>
        </div>
        <script type="text/javascript" src="//zor.livefyre.com/wjs/v3.0/javascripts/livefyre.js">
          <xsl:text> </xsl:text>
        </script>
        <script type="text/javascript">
          (function () {
          var articleId = fyre.conv.load.makeArticleId(null);
          fyre.conv.load({}, [{
          el: 'livefyre-comments',
          network: "livefyre.com",
          siteId: "<xsl:value-of select="$page/Contents/Content[@moduleType='NewsList' and @commentPlatform='livefyre']/@livefyreID"/>",
          articleId: articleId,
          signed: false,
          collectionMeta: {
          articleId: articleId,
          url: fyre.conv.load.makeCollectionUrl(),
          }
          }], function() {});
          }());
        </script>
        <!-- END: Livefyre Embed -->
      </xsl:when>
      <xsl:when test="$commentPlatform='intensedebate'">
        <script>
          var idcomments_acct = '<xsl:value-of select="$page/Contents/Content[@moduleType='NewsList' and @commentPlatform='intensedebate']/@intenseDebateID"/>';
          var idcomments_post_id = 'pageid-<xsl:value-of select="$page/@id"/>-artid-<xsl:value-of select="@id"/>';
          var idcomments_post_url = 'http://<xsl:value-of select="$siteURL"/><xsl:value-of select="$href"/>';
          var idcomments_post_title = '<xsl:apply-templates select="." mode="getDisplayName" />';
        </script>
        <span id="IDCommentsPostTitle" style="display:none">
          <xsl:text> </xsl:text>
        </span>
        <script type='text/javascript' src='//www.intensedebate.com/js/genericCommentWrapperV2.js'></script>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!--   ################   Contact   ###############   -->

  <!-- Contact Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='ContactList']" mode="displayBrief">
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
    <div class="clearfix Contacts">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix Contacts content-scroller</xsl:text>
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

  <!-- Contact Brief -->
  <xsl:template match="Content[@type='Contact']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <!-- contactBrief -->
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
    <div class="listItem list-group-item vcard ">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem list-group-item vcard'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner media">
        <!--<h4 class="fn">
          <xsl:choose>
            <xsl:when test="@noLink='true'">
              <xsl:attribute name="title">
                <xsl:call-template name="term2072" />
                <xsl:text>&#160;</xsl:text>
                <xsl:value-of select="GivenName/node()"/>
                <xsl:text>&#160;</xsl:text>
                <xsl:value-of select="Surname/node()"/>
              </xsl:attribute>
              <xsl:apply-templates select="." mode="getDisplayName"/>
            </xsl:when>
            <xsl:otherwise>
              <a href="{$parentURL}">
                <xsl:attribute name="title">
                  <xsl:call-template name="term2072" />
                  <xsl:text>&#160;</xsl:text>
                  <xsl:value-of select="GivenName/node()"/>
                  <xsl:text>&#160;</xsl:text>
                  <xsl:value-of select="Surname/node()"/>
                </xsl:attribute>
                <xsl:apply-templates select="." mode="getDisplayName"/>
              </a>
            </xsl:otherwise>
          </xsl:choose>
        </h4>-->
        <xsl:if test="Images/img/@src!=''">
          <xsl:choose>
            <xsl:when test="@noLink='true'">
              <xsl:apply-templates select="." mode="displayThumbnail"/>
            </xsl:when>
            <xsl:otherwise>
              <a href="{$parentURL}" title="click here to view more details on {GivenName/node()} {Surname/node()}">
                <xsl:apply-templates select="." mode="displayThumbnail"/>
              </a>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>
        <div class="media-body">
          <h4 class="media-heading fn">
            <xsl:choose>
              <xsl:when test="@noLink='true'">
                <xsl:attribute name="title">
                  <xsl:call-template name="term2072" />
                  <xsl:text>&#160;</xsl:text>
                  <xsl:value-of select="GivenName/node()"/>
                  <xsl:text>&#160;</xsl:text>
                  <xsl:value-of select="Surname/node()"/>
                </xsl:attribute>
                <xsl:apply-templates select="." mode="getDisplayName"/>
              </xsl:when>
              <xsl:otherwise>
                <a href="{$parentURL}">
                  <xsl:attribute name="title">
                    <xsl:call-template name="term2072" />
                    <xsl:text>&#160;</xsl:text>
                    <xsl:value-of select="GivenName/node()"/>
                    <xsl:text>&#160;</xsl:text>
                    <xsl:value-of select="Surname/node()"/>
                  </xsl:attribute>
                  <xsl:apply-templates select="." mode="getDisplayName"/>
                </a>
              </xsl:otherwise>
            </xsl:choose>
          </h4>
          <xsl:if test="Title/node()!=''">
            <h5 class="title">
              <xsl:apply-templates select="Title" mode="displayBrief"/>
            </h5>
          </xsl:if>
          <div class="address">
            <xsl:apply-templates mode="getAddress" select="Location/Address"/>
            <xsl:if test="Telephone/node()!=''">
              <p class="tel">
                <strong>
                  <xsl:call-template name="term2007" />
                  <xsl:text>:&#160;</xsl:text>
                </strong>
                <xsl:apply-templates select="Telephone" mode="displayBrief"/>
              </p>
            </xsl:if>
            <xsl:if test="Mobile/node()!=''">
              <p class="mobile">
                <strong>
                  <xsl:call-template name="term2080" />
                  <xsl:text>:&#160;</xsl:text>
                </strong>
                <xsl:apply-templates select="Mobile" mode="displayBrief"/>
              </p>
            </xsl:if>
            <xsl:if test="Fax/node()!=''">
              <p class="fax">
                <strong>
                  <xsl:call-template name="term2008" />
                  <xsl:text>:&#160;</xsl:text>
                </strong>
                <xsl:apply-templates select="Fax" mode="displayBrief"/>
              </p>
            </xsl:if>
            <xsl:if test="Email/node()!=''">
              <p>
                <strong>
                  <xsl:call-template name="term2009" />
                  <xsl:text>: </xsl:text>
                </strong>
                <a href="mailto:{Email/node()}" class="email">
                  <xsl:apply-templates select="Email" mode="displayBrief"/>
                </a>
              </p>
            </xsl:if>
            <xsl:if test="Website/node()!=''">
              <p class="web">
                <strong>
                  <xsl:call-template name="term2010" />
                  <xsl:text>:&#160;</xsl:text>
                </strong>
                <a href="{$linkURL}">
                  <xsl:apply-templates select="Website" mode="displayBrief"/>
                </a>
              </p>
            </xsl:if>
            <xsl:text> </xsl:text>
          </div>
          <xsl:if test="Profile/node()!=''">
            <p>
              <xsl:apply-templates select="Profile/node()" mode="cleanXhtml"/>
            </p>
          </xsl:if>
          <xsl:if test="not(@noLink='true')">
            <div class="entryFooter">
              <xsl:apply-templates select="." mode="moreLink">
                <xsl:with-param name="link" select="$parentURL"/>
                <xsl:with-param name="altText">
                  <xsl:value-of select="Headline/node()"/>
                </xsl:with-param>
              </xsl:apply-templates>
              <xsl:text> </xsl:text>
            </div>
          </xsl:if>
        </div>
        <!-- Terminus class fix to floating columns -->
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- Contact Brief -->
  <xsl:template match="Content[@type='Contact']" mode="displayAuthor">
    <xsl:param name="sortBy"/>
    <!-- contactBrief -->
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
    <div class="author">
      <xsl:if test="Images/img/@src!=''">
        <a href="{$parentURL}" rel="author" title="click here to view more details on {GivenName/node()} {Surname/node()}">
          <xsl:apply-templates select="." mode="displayThumbnail">
            <xsl:with-param name="width">76</xsl:with-param>
            <xsl:with-param name="height">76</xsl:with-param>
            <xsl:with-param name="crop" select="true()"/>
          </xsl:apply-templates>
        </a>
      </xsl:if>
      <xsl:text>by </xsl:text>
      <a href="{$parentURL}" rel="author">
        <xsl:attribute name="title">
          <xsl:call-template name="term2072" />
          <xsl:text>&#160;</xsl:text>
          <xsl:value-of select="GivenName/node()"/>
          <xsl:text>&#160;</xsl:text>
          <xsl:value-of select="Surname/node()"/>
        </xsl:attribute>
        <xsl:apply-templates select="." mode="getDisplayName"/>
      </a>
      <xsl:if test="Title/node()!=''">
        <h6 class="title">
          <xsl:apply-templates select="Title" mode="displayBrief"/>
          <xsl:if test="Company/node()!=''">
            <xsl:text> - </xsl:text>
            <xsl:apply-templates select="Company" mode="displayBrief"/>
          </xsl:if>
        </h6>
      </xsl:if>
    </div>
  </xsl:template>

  <!-- Contact Brief -->
  <xsl:template match="Content[@type='Contact']" mode="displayAuthorBrief">
    <xsl:param name="sortBy"/>
    <!-- contactBrief -->
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
    <div class="author">
      <xsl:text>by </xsl:text>
      <a href="{$parentURL}" rel="author">
        <xsl:attribute name="title">
          <xsl:call-template name="term2072" />
          <xsl:text>&#160;</xsl:text>
          <xsl:value-of select="GivenName/node()"/>
          <xsl:text>&#160;</xsl:text>
          <xsl:value-of select="Surname/node()"/>
        </xsl:attribute>
        <xsl:apply-templates select="." mode="getDisplayName"/>
      </a>
      <xsl:if test="Title/node()!=''">
        <h6 class="title">
          <xsl:apply-templates select="Title" mode="displayBrief"/>
          <xsl:if test="Company/node()!=''">
            <xsl:text> - </xsl:text>
            <xsl:apply-templates select="Company" mode="displayBrief"/>
          </xsl:if>
        </h6>
      </xsl:if>
    </div>
  </xsl:template>

  <!-- Contact Detail -->
  <xsl:template match="Content[@type='Contact']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <div class="detail contact vcard">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail contact'"/>
      </xsl:apply-templates>
      <xsl:apply-templates select="." mode="displayDetailImage"/>
      <h2 class="fn n content-title">
        <span class="given-name">
          <xsl:apply-templates select="GivenName" mode="displayBrief"/>
        </span>
        <xsl:text>&#160;</xsl:text>
        <span class="family-name">
          <xsl:apply-templates select="Surname" mode="displayBrief"/>
        </span>
      </h2>
      <xsl:if test="Title/node()!=''">
        <h3>
          <xsl:apply-templates select="Company" mode="displayBrief"/>
          <span class="space">
            <xsl:text>&#160;</xsl:text>
          </span>
          <span class="title">
            <xsl:apply-templates select="Title" mode="displayBrief"/>
          </span>
        </h3>
      </xsl:if>
      <xsl:if test="Department/node()!=''">
        <p class="department">
          <span class="label">
            <!-- Department -->
            <xsl:call-template name="term2011" />
            <xsl:text>:&#160;</xsl:text>
          </span>
          <span class="roll">
            <xsl:apply-templates select="Department" mode="displayBrief"/>
          </span>
        </p>
      </xsl:if>
      <xsl:apply-templates select="." mode="socialLinks">
        <xsl:with-param name="iconSet">default</xsl:with-param>
        <xsl:with-param name="myName">
          <xsl:apply-templates select="GivenName" mode="displayBrief"/>
          <xsl:text> </xsl:text>
          <xsl:apply-templates select="Surname" mode="displayBrief"/>
        </xsl:with-param>
      </xsl:apply-templates>
      <xsl:apply-templates mode="getAddress" select="Location/Address"/>
      <xsl:if test="Telephone/node() or Fax/node() or Email/node() or Website/node()">
        <div class="telecoms">
          <xsl:if test="Telephone/node()!=''">
            <p>
              <span class="label">
                <xsl:call-template name="term2007" />
                <xsl:text>:&#160;</xsl:text>
              </span>
              <span class="tel">
                <xsl:apply-templates select="Telephone" mode="displayBrief"/>
              </span>
            </p>
          </xsl:if>
          <xsl:if test="Mobile/node()!=''">
            <p class="mobile">
              <span class="label">
                <xsl:call-template name="term2080" />
                <xsl:text>:&#160;</xsl:text>
              </span>
              <xsl:apply-templates select="Mobile" mode="displayBrief"/>
            </p>
          </xsl:if>
          <xsl:if test="Fax/node()!=''">
            <p class="fax">
              <span class="label">
                <xsl:call-template name="term2008" />
                <xsl:text>:&#160;</xsl:text>
              </span>
              <xsl:apply-templates select="Fax" mode="displayBrief"/>
            </p>
          </xsl:if>
          <xsl:if test="Email/node()!=''">
            <p>
              <span class="label">
                <xsl:call-template name="term2009" />
                <xsl:text>: </xsl:text>
              </span>
              <a href="mailto:{Email/node()}">
                <span class="email">
                  <xsl:apply-templates select="Email" mode="displayBrief"/>
                </span>
              </a>
            </p>
          </xsl:if>
          <xsl:if test="Website/node()!=''">
            <p>
              <span class="label">
                <xsl:call-template name="term2010" />
                <xsl:text>:&#160;</xsl:text>
              </span>
              <a href="{Website/node()}" class="url">
                <xsl:apply-templates select="Website" mode="displayBrief"/>
              </a>
            </p>
          </xsl:if>
        </div>
      </xsl:if>
      <xsl:apply-templates select="Description/node()" mode="cleanXhtml"/>
      <div class="NewsList">
        <div>
          <xsl:apply-templates select="Content[@type='NewsArticle']" mode="displayBrief">
            <xsl:with-param name="sortBy" select="@publishDate"/>
          </xsl:apply-templates>
          <div class="terminus">&#160;</div>
        </div>
      </div>
      <!-- Terminus class fix to floating columns -->
      <div class="terminus">&#160;</div>
      <div class="entryFooter">
        <div class="tags">
          <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
          <xsl:text> </xsl:text>
        </div>
        <xsl:apply-templates select="." mode="backLink">
          <xsl:with-param name="link" select="$thisURL"/>
          <xsl:with-param name="altText">
            <xsl:call-template name="term2012" />
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div class="terminus">&#160;</div>
    </div>
  </xsl:template>

  <!--   ################   Organisation   ###############   -->

  <!-- Organisation Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='OrganisationList']" mode="displayBrief">
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
    <div class="Clearfix Contacts OrganisationList">
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
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="contactList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <!-- Organisation Brief -->
  <xsl:template match="Content[@type='Organisation']" mode="displayEventBrief">
    <xsl:param name="sortBy"/>
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
    <div itemscope="" itemtype="{Organization/@itemtype}">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div itemprop="address" itemscope="" itemtype="http://schema.org/PostalAddress">
        <xsl:if test="Organization/location/PostalAddress/name!='' or Organization/location/PostalAddress/streetAddress!='' or Organization/location/PostalAddress/addressLocality!='' or Organization/location/PostalAddress/addressRegion!='' or Organization/location/PostalAddress/postalCode!=''"> </xsl:if>
        <a href="{$parentURL}">
          <span itemprop="name">
            <xsl:value-of select="name"/>
          </span>
          <xsl:text>, </xsl:text>
          <xsl:if test="Organization/location/PostalAddress/name!='' and Organization/location/PostalAddress/name!=name">
            <span itemprop="name">
              <xsl:value-of select="Organization/location/PostalAddress/name"/>
            </span>
            <xsl:text>, </xsl:text>
          </xsl:if>
          <xsl:if test="Organization/location/PostalAddress/streetAddress!=''">
            <span itemprop="streetAddress">
              <xsl:value-of select="Organization/location/PostalAddress/streetAddress"/>
            </span>
            <xsl:text>, </xsl:text>
          </xsl:if>
          <xsl:if test="Organization/location/PostalAddress/addressLocality!=''">
            <span itemprop="addressLocality">
              <xsl:value-of select="Organization/location/PostalAddress/addressLocality"/>
            </span>
            <xsl:text>, </xsl:text>
          </xsl:if>
          <xsl:if test="Organization/location/PostalAddress/addressRegion!=''">
            <span itemprop="addressRegion">
              <xsl:value-of select="Organization/location/PostalAddress/addressRegion"/>
            </span>
            <xsl:text>. </xsl:text>
          </xsl:if>
          <xsl:if test="Organization/location/PostalAddress/postalCode!=''">
            <span itemprop="postalCode">
              <xsl:value-of select="Organization/location/PostalAddress/postalCode"/>
            </span>
            <xsl:text>. </xsl:text>
          </xsl:if>
        </a>
        <span class="pull-right">
          <a href="{$parentURL}" class="btn btn-default btn-sm">
            <i class="fa fa-map-marker">
              <xsl:text> </xsl:text>
            </i>
            <xsl:text> </xsl:text>
            Get Directions
          </a>
        </span>
        <div class="clear-fix">
          <xsl:text> </xsl:text>
        </div>
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- Organisation Brief -->
  <xsl:template match="Content[@type='Organisation']" mode="displayBrief">
    <xsl:param name="sortBy"/>
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
    <div itemscope="" itemtype="{Organization/@itemtype}" class="listItem list-group-item">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem list-group-item'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <h3 class="title">
          <a href="{$parentURL}">
            <span itemprop="name">
              <xsl:value-of select="name"/>
            </span>
          </a>
        </h3>
        <xsl:apply-templates select="." mode="displayLogo"/>
        <div itemprop="address" itemscope="" itemtype="http://schema.org/PostalAddress">
          <xsl:if test="Organization/location/PostalAddress/name!='' or Organization/location/PostalAddress/streetAddress!='' or Organization/location/PostalAddress/addressLocality!='' or Organization/location/PostalAddress/addressRegion!='' or Organization/location/PostalAddress/postalCode!=''"> </xsl:if>
          <xsl:if test="Organization/location/PostalAddress/name!=''">
            <span itemprop="name">
              <xsl:value-of select="Organization/location/PostalAddress/name"/>
            </span>
            <br />
          </xsl:if>
          <xsl:if test="Organization/location/PostalAddress/streetAddress!=''">
            <span itemprop="streetAddress">
              <xsl:value-of select="Organization/location/PostalAddress/streetAddress"/>
            </span>
            <br />
          </xsl:if>
          <xsl:if test="Organization/location/PostalAddress/addressLocality!=''">
            <span itemprop="addressLocality">
              <xsl:value-of select="Organization/location/PostalAddress/addressLocality"/>
            </span>
            <br />
          </xsl:if>
          <xsl:if test="Organization/location/PostalAddress/addressRegion!=''">
            <span itemprop="addressRegion">
              <xsl:value-of select="Organization/location/PostalAddress/addressRegion"/>
            </span>
            <br />
          </xsl:if>
          <xsl:if test="Organization/location/PostalAddress/postalCode!=''">
            <span itemprop="postalCode">
              <xsl:value-of select="Organization/location/PostalAddress/postalCode"/>
            </span>
          </xsl:if>
        </div>
        <xsl:if test="Organization/telephone/node()!=''">
          <p class="tel">
            <strong>
              <xsl:call-template name="term2007" />
              <xsl:text>:&#160;</xsl:text>
            </strong>
            <xsl:apply-templates select="Organization/telephone" mode="cleanXhtml"/>
          </p>
        </xsl:if>
        <xsl:if test="url/node()!=''">
          <p class="web">
            <strong>
              <xsl:call-template name="term2010" />
              <xsl:text>:&#160;</xsl:text>
            </strong>
            <a href="{url/node()}">
              <xsl:apply-templates select="url" mode="cleanXhtml"/>
            </a>
          </p>
        </xsl:if>
        <xsl:if test="description/node()!=''">
          <p class="Description">
            <strong>
              <!--Description-->
              <xsl:call-template name="term3040" />
              <xsl:text>:&#160;</xsl:text>
            </strong>
            <xsl:apply-templates select="description" mode="cleanXhtml"/>
          </p>
        </xsl:if>
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
    </div>
  </xsl:template>

  <!-- Organisation Detail -->
  <xsl:template match="Content[@type='Organisation']" mode="ContentDetail">
    <xsl:param name="sortBy"/>
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
    <div itemscope="" itemtype="{Organization/@itemtype}" class="detail organisation-detail">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail organisation-detail'"/>
        <xsl:with-param name="editLabel" select="@type"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <h2>
        <span itemprop="name">
          <xsl:value-of select="name"/>
        </span>
      </h2>
      <div class="row">
        <div class="col-md-8">
          <span class="picture">
            <xsl:apply-templates select="." mode="displayLogo"/>
          </span>
          <xsl:if test="Organization/contactPoint/ContactPoint/@facebookURL!='' or Organization/contactPoint/ContactPoint/@twitterURL!=''  or Organization/contactPoint/ContactPoint/@linkedInURL!=''  or Organization/contactPoint/ContactPoint/@googlePlusURL!=''  or Organization/contactPoint/ContactPoint/@pinterestURL!=''">
            <xsl:apply-templates select="Organization/contactPoint/ContactPoint" mode="socialLinks">
              <xsl:with-param name="iconSet" select="'icons'"/>
              <xsl:with-param name="myName" select="name"/>
            </xsl:apply-templates>
          </xsl:if>
          <xsl:if test="Organization/legalName/node()!='' or Organization/foundingDate/node()!='' or Organization/taxID/node()!='' or Organization/vatID/node()!='' or Organization/localBusiness/priceRange/node()!='' or Organization/duns/node()!=''">
            <dl class="dl-horizontal">
              <xsl:if test="Organization/legalName/node()!=''">
                <dt class="date">
                  <!--legalName-->
                  <xsl:call-template name="term2103" />
                  <xsl:text>:&#160;</xsl:text>
                </dt>
                <dd>
                  <xsl:apply-templates select="Organization/legalName" mode="cleanXhtml"/>
                </dd>
              </xsl:if>
              <xsl:if test="Organization/foundingDate/node()!=''">
                <dt class="date">
                  <!--foundingDate-->
                  <xsl:call-template name="term2104" />
                  <xsl:text>:&#160;</xsl:text>
                </dt>
                <dd>
                  <xsl:apply-templates select="Organization/foundingDate" mode="cleanXhtml"/>
                </dd>
              </xsl:if>
              <xsl:if test="Organization/taxID/node()!=''">
                <dt class="taxid">
                  <!--Tax ID-->
                  <xsl:call-template name="term2108" />
                  <xsl:text>:&#160;</xsl:text>
                </dt>
                <dd>
                  <xsl:apply-templates select="Organization/taxID" mode="cleanXhtml"/>
                </dd>
              </xsl:if>
              <xsl:if test="Organization/vatID/node()!=''">
                <dt class="">
                  <!--VAT-->
                  <xsl:call-template name="term2109" />
                  <xsl:text>:&#160;</xsl:text>
                </dt>
                <dd>
                  <xsl:apply-templates select="Organization/vatID" mode="cleanXhtml"/>
                </dd>
              </xsl:if>
              <xsl:if test="Organization/localBusiness/currenciesAccepted/node()!=''">
                <dt class="">
                  <!--currenciesAccepted-->
                  <xsl:call-template name="term2110" />
                  <xsl:text>:&#160;</xsl:text>
                </dt>
                <dd>
                  <xsl:apply-templates select="Organization/localBusiness/currenciesAccepted" mode="cleanXhtml"/>
                </dd>
              </xsl:if>
              <xsl:if test="Organization/localBusiness/priceRange/node()!=''">
                <dt class="applyBy">
                  <!--priceRange-->
                  <xsl:call-template name="term2113" />
                  <xsl:text>:&#160;</xsl:text>
                </dt>
                <dd>
                  <xsl:apply-templates select="Organization/localBusiness/priceRange" mode="cleanXhtml"/>
                </dd>
              </xsl:if>
              <xsl:if test="Organization/duns/node()!=''">
                <dt>
                  <!--Dun &amp; Bradstreet Number-->
                  <xsl:call-template name="term2107" />
                  <xsl:text>:&#160;</xsl:text>
                </dt>
                <dd>
                  <xsl:apply-templates select="Organization/duns" mode="cleanXhtml"/>
                </dd>
              </xsl:if>
              <xsl:if test="Organization/@itemtype!=''"> </xsl:if>
            </dl>
          </xsl:if>
          <p itemprop="address" itemscope="" itemtype="http://schema.org/PostalAddress">
            <xsl:if test="Organization/location/PostalAddress/name!=''">
              <span itemprop="name">
                <xsl:value-of select="Organization/location/PostalAddress/name"/>
              </span>
              <br />
            </xsl:if>
            <xsl:if test="Organization/location/PostalAddress/streetAddress!=''">
              <span itemprop="streetAddress">
                <xsl:value-of select="Organization/location/PostalAddress/streetAddress"/>
              </span>
              <br />
            </xsl:if>
            <xsl:if test="Organization/location/PostalAddress/addressLocality!=''">
              <span itemprop="addressLocality">
                <xsl:value-of select="Organization/location/PostalAddress/addressLocality"/>
              </span>
              <br />
            </xsl:if>
            <xsl:if test="Organization/location/PostalAddress/addressRegion!=''">
              <span itemprop="addressRegion">
                <xsl:value-of select="Organization/location/PostalAddress/addressRegion"/>
              </span>
              <br />
            </xsl:if>
            <xsl:if test="Organization/location/PostalAddress/postalCode!=''">
              <span itemprop="postalCode">
                <xsl:value-of select="Organization/location/PostalAddress/postalCode"/>
              </span>
            </xsl:if>
          </p>
          <xsl:if test="Organization/telephone/node()!=''">
            <p class="tel">
              <strong>
                <!--tel-->
                <xsl:call-template name="term2007" />
                <xsl:text>:&#160;</xsl:text>
              </strong>
              <xsl:apply-templates select="Organization/telephone" mode="cleanXhtml"/>
            </p>
          </xsl:if>
          <xsl:if test="Organization/faxNumber/node()!=''">
            <p class="web">
              <strong>
                <!--Fax-->
                <xsl:call-template name="term2008" />
                <xsl:text>:&#160;</xsl:text>
              </strong>
              <xsl:apply-templates select="Organization/faxNumber" mode="cleanXhtml"/>
            </p>
          </xsl:if>
          <xsl:if test="url/node()!=''">
            <p class="web">
              <strong>
                <xsl:call-template name="term2010" />
                <xsl:text>:&#160;</xsl:text>
              </strong>
              <a href="{url/node()}">
                <xsl:apply-templates select="url" mode="cleanXhtml"/>
              </a>
            </p>
          </xsl:if>
          <xsl:if test="Organization/email/node()!=''">
            <p>
              <strong>
                <!--Email-->
                <xsl:call-template name="term2009" />
                <xsl:text>:&#160;</xsl:text>
              </strong>
              <a href="mailto:{Email/node()}" class="email">
                <xsl:apply-templates select="Organization/email" mode="cleanXhtml"/>
              </a>
            </p>
          </xsl:if>
          <xsl:if test="Organization/localBusiness/openingHours/node()!=''">
            <p class="full">
              <strong>
                <!--openingHours-->
                <xsl:call-template name="term2111" />
                <xsl:text>:&#160;</xsl:text>
              </strong>
              <xsl:apply-templates select="Organization/localBusiness/openingHours" mode="cleanXhtml"/>
            </p>
          </xsl:if>
          <xsl:if test="Organization/localBusiness/paymentAccepted/node()!=''">
            <p class="full">
              <strong>
                <!--paymentAccepted-->
                <xsl:call-template name="term2112" />
                <xsl:text>:&#160;</xsl:text>
              </strong>
              <xsl:apply-templates select="Organization/localBusiness/paymentAccepted" mode="cleanXhtml"/>
            </p>
          </xsl:if>
          <xsl:if test="body/node()!=''">
            <p class="full">
              <strong>
                <!--profile-->
                <xsl:call-template name="term2101" />
                <xsl:text>:&#160;</xsl:text>
              </strong>
              <xsl:apply-templates select="body" mode="cleanXhtml"/>
            </p>
          </xsl:if>

        </div>
        <div class="col-md-4">
          <xsl:apply-templates select="." mode="organizationDetailMap"/>
        </div>
      </div>
      <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
      <xsl:apply-templates select="." mode="backLink">
        <xsl:with-param name="link" select="$thisURL"/>
        <xsl:with-param name="altText">
          <!--click here to return to the news article list-->
          <xsl:call-template name="term2071" />
        </xsl:with-param>
      </xsl:apply-templates>
    </div>
  </xsl:template>

  <xsl:template match="Content" mode="organizationDetailMap">
    <div class="GoogleMap">
      <div id="gmap{@id}" class="gmap-canvas" data-mapheight="300">To see this map you must have Javascript enabled</div>
    </div>
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

  <!--   ################   Events   ###############   -->

  <!-- Event Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='EventList']" mode="displayBrief">
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
    <div class="clearfix EventsList">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix EventsList content-scroller</xsl:text>
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
        <xsl:choose>
          <xsl:when test="@linkArticle='true'">
            <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefLinked">
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
          </xsl:otherwise>
        </xsl:choose>
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

  <!-- Event Brief -->
  <xsl:template match="Content[@type='Event']" mode="displayBrief">
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
          <xsl:if test="StartDate/node()!=''">
            <p class="date">
              <span class="dtstart">
                <xsl:call-template name="DisplayDate">
                  <xsl:with-param name="date" select="StartDate/node()"/>
                </xsl:call-template>
                <span class="value-title" title="{StartDate/node()}T{translate(Times/@start,',',':')}" ></span>
              </span>
              <xsl:if test="EndDate/node()!='' and EndDate/node() != StartDate/node()">
                <xsl:text> - </xsl:text>
                <span class="dtend">
                  <xsl:call-template name="DisplayDate">
                    <xsl:with-param name="date" select="EndDate/node()"/>
                  </xsl:call-template>
                  <span class="value-title" title="{EndDate/node()}T{translate(Times/@end,',',':')}"></span>
                </span>
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
            </p>
          </xsl:if>
          <xsl:if test="Strap/node()!=''">
            <div class="summary">
              <xsl:apply-templates select="Strap/node()" mode="cleanXhtml"/>
            </div>
          </xsl:if>
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

  <!-- Event Brief Linked -->
  <xsl:template match="Content[@type='Event']" mode="displayBriefLinked">
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
      <a href="{$parentURL}" title="Read More - {Headline/node()}">
        <div class="lIinner media">
          <xsl:if test="Images/img/@src!=''">
            <xsl:apply-templates select="." mode="displayThumbnail"/>
          </xsl:if>
          <div class="media-body">
            <h4 class="media-heading">
              <xsl:apply-templates select="." mode="getDisplayName"/>
            </h4>
            <xsl:if test="StartDate/node()!=''">
              <p class="date">
                <span class="dtstart">
                  <xsl:call-template name="DisplayDate">
                    <xsl:with-param name="date" select="StartDate/node()"/>
                  </xsl:call-template>
                  <span class="value-title" title="{StartDate/node()}T{translate(Times/@start,',',':')}" ></span>
                </span>
                <xsl:if test="EndDate/node()!='' and EndDate/node() != StartDate/node()">
                  <xsl:text> - </xsl:text>
                  <span class="dtend">
                    <xsl:call-template name="DisplayDate">
                      <xsl:with-param name="date" select="EndDate/node()"/>
                    </xsl:call-template>
                    <span class="value-title" title="{EndDate/node()}T{translate(Times/@end,',',':')}"></span>
                  </span>
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
              </p>
            </xsl:if>
            <xsl:if test="Strap/node()!=''">
              <div class="summary">
                <xsl:apply-templates select="Strap/node()" mode="cleanXhtml"/>
              </div>
            </xsl:if>
          </div>
        </div>
      </a>
    </div>
  </xsl:template>

  <!-- Event Brief -->
  <xsl:template match="Content[@type='Ticket']" mode="displayBriefDate">
    <xsl:param name="sortBy"/>
    <!-- articleBrief -->
    <xsl:variable name="href">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <li class="col-md-4 " role="presentation">
      <xsl:if test="position()=1">
        <xsl:attribute name="class">col-md-4</xsl:attribute>
      </xsl:if>
      <a class="date" role="tab" href="#{@id}-buyPanel" aria-controls="{@id}-buyPanel">
        <xsl:choose>
          <xsl:when test="Stock/node()='0'">
            <xsl:attribute name="class">date booking-full</xsl:attribute>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="data-toggle">tab</xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
        <span class="eventdate">
          <xsl:if test="StartDate/node()!=''">
            <span class="dtstart">
              <xsl:call-template name="CalendarIcon">
                <xsl:with-param name="date" select="StartDate/node()"/>
              </xsl:call-template>
            </span>
          </xsl:if>
          <span class="coursetime">
            <xsl:if test="position()">
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
          </span>
        </span>
        <span class="price text-center">
          <xsl:choose>
            <xsl:when test="Stock/node()='0'">
              <xsl:text>FULL</xsl:text>
            </xsl:when>
            <xsl:otherwise>
              <xsl:apply-templates select="." mode="displayPrice" />
            </xsl:otherwise>
          </xsl:choose>
        </span>
      </a>
    </li>
  </xsl:template>

  <!-- Event Detail -->
  <xsl:template match="Content[@type='Event']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
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
      <xsl:choose>
        <xsl:when test="Content[@type='Ticket']">
          <div class="row">
            <div class="description col-md-8">
              <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
            </div>
            <div class="col-md-4">
              <xsl:apply-templates select="." mode="ticketsGrouped" />
            </div>
          </div>
        </xsl:when>
        <xsl:otherwise>
          <div class="description">
            <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
          </div>
        </xsl:otherwise>
      </xsl:choose>
      <div class="terminus">&#160;</div>
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
  </xsl:template>

  <!-- Event Detail -->
  <xsl:template match="Content[@type='Event']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <div class="detail vevent content-title">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail event'"/>
      </xsl:apply-templates>
      <h2>
        <xsl:apply-templates select="Headline" mode="displayBrief"/>
      </h2>
      <!--RELATED CONTENT-->
      <div class="row">
        <div>
          <xsl:choose>
            <xsl:when test="Content[@type='Ticket']">
              <xsl:attribute name="class">col-md-8</xsl:attribute>
            </xsl:when>
            <xsl:otherwise>
              <xsl:attribute name="class">col-md-12</xsl:attribute>
              <xsl:apply-templates select="." mode="displayDetailImage"/>
            </xsl:otherwise>
          </xsl:choose>
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
          <xsl:if test="Content[@type='Organisation']">
            <xsl:apply-templates select="Content[@type='Organisation']" mode="displayEventBrief"/>
          </xsl:if>
          <div class="description">
            <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
          </div>
        </div>
        <!-- Tickets  -->
        <xsl:if test="Content[@type='Ticket']">
          <div class="col-md-4">
            <div class="clearfix">
              <xsl:apply-templates select="." mode="displayDetailImage"/>
            </div>
            <xsl:apply-templates select="." mode="RelatedTickets">
              <xsl:with-param name="parTicketID" select="@id"/>
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
          </div>
        </xsl:if>
      </div>
      <div class="terminus">&#160;</div>
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
  </xsl:template>

  <!-- List Related Tickets-->
  <xsl:template match="Content" mode="RelatedTickets">
    <xsl:param name="sortBy"/>
    <xsl:param name="parProductID"/>
    <form action="" method="post" class="ewXform ProductAddForm">
      <div class="tickets panel panel-default">
        <xsl:apply-templates select="." mode="inlinePopupRelate">
          <xsl:with-param name="type">Ticket</xsl:with-param>
          <xsl:with-param name="text">Add Ticket</xsl:with-param>
          <xsl:with-param name="name"></xsl:with-param>
          <xsl:with-param name="find">false</xsl:with-param>
        </xsl:apply-templates>
        <table class="ticketsGrouped table">
          <tr>
            <th>
              <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
                <xsl:with-param name="type">PlainText</xsl:with-param>
                <xsl:with-param name="text">Add Title for Tickets</xsl:with-param>
                <xsl:with-param name="name">relatedTicketTitle</xsl:with-param>
              </xsl:apply-templates>
              <xsl:choose>
                <xsl:when test="/Page/Contents/Content[@name='relatedTicketTitle']">
                  <xsl:apply-templates select="/Page/Contents/Content[@name='relatedTicketTitle']" mode="displayBrief"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>Tickets</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
            </th>
            <th>
              <strong>Price:</strong>
            </th>
            <th>
              <strong>Qty:</strong>
            </th>
          </tr>
          <xsl:for-each select="/Page/ContentDetail/Content/Content[@type='Ticket']">
            <xsl:sort select="@type" order="ascending"/>
            <xsl:sort select="@displayOrder" order="ascending"/>
            <xsl:apply-templates select="." mode="displayBriefTicket"/>
          </xsl:for-each>
          <div class="terminus">&#160;</div>
        </table>
        <div class="panel-footer">
          <span class="pull-right">
            <xsl:apply-templates select="/" mode="addtoCartButtons"/>
          </span>
        </div>
      </div>
    </form>
  </xsl:template>

  <!-- Ticket related products -->
  <xsl:template match="Content[@type='Ticket']" mode="displayBriefTicket">
    <xsl:param name="sortBy"/>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <tr>
      <td class="ListGroupedTitle">
        <xsl:variable name="title">
          <xsl:apply-templates select="." mode="getDisplayName"/>
        </xsl:variable>
        <xsl:value-of select="$title"/>
      </td>
      <td class="ListGroupedPrice">
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
      </td>
      <td class="ListGroupedQty">
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
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="Content" mode="showQuantityGrouped">
    <xsl:variable name="id">
      <xsl:choose>
        <xsl:when test="@type='SKU'">
          <xsl:value-of select="../@id"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@id"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div class="input-group qty">
      <span class="input-group-btn">
        <button class=" qtyButton increaseQty btn btn-default" type="button" value="+" onClick="incrementQuantity('qty_{@id}','+')">
          <i class="fa fa-plus">
            <xsl:text> </xsl:text>
          </i>
        </button>
      </span>
      <input type="text" name="qty_{@id}" id="qty_{$id}" value="0" size="1" class="form-control"/>
      <span class="input-group-btn">
        <button class="qtyButton decreaseQty btn btn-default" type="button" value="-" onClick="incrementQuantity('qty_{@id}','-')">
          <i class="fa fa-minus">
            <xsl:text> </xsl:text>
          </i>
        </button>
      </span>
    </div>
  </xsl:template>

  <!--  ==  TICKETS  =================================================================================  -->

  <!-- Product Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='TicketList']" mode="displayBrief">
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
    <div class="clearfix TicketList">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix ProductList content-scroller</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <div class="cols cols{@cols}" data-slidestoshow="{@cols}">
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

  <xsl:template match="Content" mode="relatedTickets">
    <div class="Default-Box box ticketsbox">
      <div class="tl">
        <div class="tr">
          <h2 class="title">Tickets</h2>
        </div>
      </div>
      <div class="content">
        <div class="cols3">
          <xsl:apply-templates select="Content[@type='Ticket']" mode="displayBrief" />
          <div class="terminus">&#160;</div>
        </div>
      </div>
    </div>
  </xsl:template>

  <!-- -->

  <xsl:template match="Content" mode="ticketsGrouped">
    <div class="Default-Box box tickets">
      <div class="tl">
        <div class="tr">
          <h2 class="title">Tickets</h2>
        </div>
      </div>
      <div class="content">
        <form action="" method="post" class="ewXform">
          <table border="0" cellpadding="0" cellspacing="0" class="ticketsGrouped">
            <xsl:apply-templates select="Content[@type='Ticket']" mode="displayGroupedBrief" />
            <tr>
              <td colspan="3">
                <table>
                  <tr>
                    <td>
                      <p>
                        <!--To order, please enter the quantities you require.-->
                        <xsl:call-template name="term3064" />
                      </p>
                    </td>
                    <td class="buttons">
                      <xsl:apply-templates select="/" mode="addtoCartButtons"/>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </form>
      </div>
    </div>
  </xsl:template>

  <!-- -->

  <!-- TICKET Brief -->
  <xsl:template match="Content[@type='Ticket']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <!-- articleBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="listItem ticket list-group-item hproduct">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem ticket list-group-item hproduct'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <h4 class="title">
          <xsl:apply-templates select="." mode="getDisplayName"/>
        </h4>
        <xsl:if test="Images/img/@src!=''">
          <xsl:apply-templates select="." mode="displayThumbnail"/>
        </xsl:if>
        <xsl:if test="StartDate/node()!=''">
          <p class="date">
            <span class="dates">
              <xsl:call-template name="DisplayDate">
                <xsl:with-param name="date" select="StartDate/node()"/>
              </xsl:call-template>
              <xsl:if test="EndDate/node()!='' and EndDate/node() != StartDate/node()">
                <xsl:text> - </xsl:text>
                <xsl:call-template name="DisplayDate">
                  <xsl:with-param name="date" select="EndDate/node()"/>
                </xsl:call-template>
              </xsl:if>
            </span>
            <xsl:text>&#160;</xsl:text>
            <xsl:if test="Times/@start!=''">
              <span class="times">
                <xsl:value-of select="translate(Times/@start,',',':')"/>
                <xsl:if test="Times/@end!=''">
                  <xsl:text> - </xsl:text>
                  <xsl:value-of select="translate(Times/@end,',',':')"/>
                </xsl:if>
              </span>
            </xsl:if>
          </p>
        </xsl:if>
        <!-- PRICES -->
        <xsl:apply-templates select="." mode="displayPrice" />
        <xsl:if test="$page/Cart">
          <xsl:apply-templates select="." mode="addToCartButton">
            <xsl:with-param name="actionURL" select="$parentURL"/>
          </xsl:apply-templates>
        </xsl:if>
        <!-- Accessiblity fix : Separate adjacent links with more than whitespace -->
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='Ticket']" mode="displayGroupedBrief">
    <xsl:param name="sortBy"/>
    <!-- articleBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <tr class="ticket">
      <td>
        <div class="title">
          <xsl:apply-templates select="." mode="getDisplayName"/>
        </div>
        <xsl:if test="StartDate/node()!=''">
          <p class="date">
            <span class="dates">
              <xsl:call-template name="DisplayDate">
                <xsl:with-param name="date" select="StartDate/node()"/>
              </xsl:call-template>
              <xsl:if test="EndDate/node()!='' and EndDate/node() != StartDate/node()">
                <xsl:text> - </xsl:text>
                <xsl:call-template name="DisplayDate">
                  <xsl:with-param name="date" select="EndDate/node()"/>
                </xsl:call-template>
              </xsl:if>
            </span>
            <xsl:text>&#160;</xsl:text>
            <xsl:if test="Times/@start!=''">
              <span class="times">
                <xsl:value-of select="translate(Times/@start,',',':')"/>
                <xsl:if test="Times/@end!=''">
                  <xsl:text> - </xsl:text>
                  <xsl:value-of select="translate(Times/@end,',',':')"/>
                </xsl:if>
              </span>
            </xsl:if>
          </p>
        </xsl:if>
      </td>
      <td class="price">
        <!-- PRICES -->
        <xsl:apply-templates select="." mode="displayPrice" />
      </td>
      <td class="quantity">
        <xsl:choose>
          <xsl:when test="$page/@adminMode">
            <div>
              <xsl:apply-templates select="." mode="inlinePopupOptions">
                <xsl:with-param name="class" select="''"/>
                <xsl:with-param name="sortBy" select="$sortBy"/>
              </xsl:apply-templates>
            </div>
          </xsl:when>
          <xsl:otherwise>
            <input type="text" id="qty_{@id}" name="qty_{@id}" value="0" size="3" class="qtybox textbox"/>
            <xsl:apply-templates select="." mode="Options_List"/>
          </xsl:otherwise>
        </xsl:choose>
      </td>
    </tr>
  </xsl:template>


  <!--  ##  Calendar Layouts   ######################################################################   -->
  <!-- Calendar Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='EventCalendar']" mode="displayBrief">
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
    <div class="EventsCalendar">
      <div class="calendarView">
        <xsl:apply-templates select="CalendarView" mode="displayCalendar" />
        <xsl:text> </xsl:text>
      </div>
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
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- Calendar Display -->
  <xsl:template match="CalendarView" mode="displayCalendar">
    <xsl:param name="calendarName"/>
    <xsl:variable name="calendarYear" select="Calendar/Year"/>
    <xsl:variable name="calendarMonth" select="Calendar/Year/Month"/>
    <xsl:variable name="monthdate" select="Calendar/Year/Month/@dateid"/>
    <xsl:variable name="pageURL">
      <xsl:apply-templates select="/Page/Menu/MenuItem/descendant-or-self::MenuItem[@id=$currentPage/@id]" mode="getHref"/>
    </xsl:variable>
    <xsl:variable name="querySymbol">
      <xsl:choose>
        <xsl:when test="/Page/@adminMode">&amp;</xsl:when>
        <xsl:otherwise>?</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <table cellpadding="0" cellspacing="0" border="0" summary="{Title/node()}">
      <tr class="calendarMonthHeader">
        <th class="previousMonthNav">
          <xsl:variable name="prevMonthCmd">
            <xsl:value-of select="$pageURL"/>
            <xsl:value-of select="$querySymbol"/>
            <xsl:text>calcmd=</xsl:text>
            <xsl:value-of select="$calendarMonth/@prevMonthYear"/>
            <xsl:value-of select="$calendarMonth/@prevMonth"/>
          </xsl:variable>
          <a href="{$prevMonthCmd}" title="{$calendarMonth/@prev} {$calendarMonth/@prevMonthYear}">
            <xsl:text>&lt; </xsl:text>
            <xsl:value-of select="$calendarMonth/@prev"/>
            <xsl:text> </xsl:text>
            <xsl:value-of select="$calendarMonth/@prevMonthYear"/>
          </a>
        </th>
        <th class="currentMonthNav">
          <h3>
            <xsl:value-of select="$calendarMonth/@index"/>
            <xsl:text> </xsl:text>
            <xsl:value-of select="$calendarYear/@index"/>
          </h3>
        </th>
        <th class="nextMonthNav">
          <xsl:variable name="nextMonthCmd">
            <xsl:value-of select="$pageURL"/>
            <xsl:value-of select="$querySymbol"/>
            <xsl:text>calcmd=</xsl:text>
            <xsl:value-of select="$calendarMonth/@nextMonthYear"/>
            <xsl:value-of select="$calendarMonth/@nextMonth"/>
          </xsl:variable>
          <a href="{$nextMonthCmd}" title="{$calendarMonth/@next} {$calendarMonth/@nextMonthYear}">
            <xsl:value-of select="$calendarMonth/@next"/>
            <xsl:text> </xsl:text>
            <xsl:value-of select="$calendarMonth/@nextMonthYear"/>
            <xsl:text> &gt;</xsl:text>
          </a>
        </th>
      </tr>
      <tr>
        <td colspan="3" class="calendarDays">
          <xsl:apply-templates select="$calendarMonth" mode="displayCalendar"/>
        </td>
      </tr>
    </table>
  </xsl:template>

  <xsl:template match="Month" mode="displayCalendar">
    <xsl:variable name="curMonth" select="/Page/Contents/Content/CalendarView/Calendar/Year/Month/@index"/>
    <table cellpadding="0" cellspacing="0" border="0">
      <xsl:attribute name="summary">
        <xsl:call-template name="Month_YYYY">
          <xsl:with-param name="date" select="@dateid"/>
        </xsl:call-template>
      </xsl:attribute>
      <!-- Calendar Header -->
      <tr class="calendarHeader">
        <th>Wk</th>
        <xsl:for-each select="Week[1]/Day">
          <th>
            <xsl:attribute name="class">
              <xsl:text>days</xsl:text>
              <xsl:if test="count(./preceding-sibling::Day)=0">
                <xsl:text> first</xsl:text>
              </xsl:if>
              <xsl:if test="count(./following-sibling::Day)=0">
                <xsl:text> last</xsl:text>
              </xsl:if>
            </xsl:attribute>
            <xsl:value-of select="@day"/>
          </th>
        </xsl:for-each>
      </tr>
      <!-- Calendar Weeks -->
      <xsl:for-each select="Week">
        <tr>
          <td class="weekNumber">
            <div class="cdaytitle">
              <xsl:value-of select="@index"/>
            </div>
          </td>
          <xsl:apply-templates select="Day" mode="displayCalendar">
            <xsl:with-param name="curMonth" select="$curMonth"/>
          </xsl:apply-templates>
        </tr>
      </xsl:for-each>
    </table>
  </xsl:template>

  <xsl:template match="Day" mode="displayCalendar">
    <xsl:param name="curMonth"/>
    <xsl:variable name="today">
      <xsl:value-of select="$currentYear"/>
      <xsl:value-of select="$currentMonth"/>
      <xsl:value-of select="$currentDay"/>
    </xsl:variable>
    <td>
      <xsl:attribute name="class">
        <xsl:text>cday</xsl:text>
        <xsl:if test="$today=@dateid">
          <xsl:text> today</xsl:text>
        </xsl:if>
        <xsl:if test="@month!=$curMonth">
          <xsl:text> anotherMonth</xsl:text>
        </xsl:if>
        <xsl:if test="count(./following-sibling::Day)=0">
          <xsl:text> last</xsl:text>
        </xsl:if>
        <xsl:if test="count(./preceding-sibling::Day)=0">
          <xsl:text> first</xsl:text>
        </xsl:if>
      </xsl:attribute>
      <div class="cdaytitle">
        <xsl:value-of select="@index"/>
      </div>
      <xsl:for-each select="item">
        <xsl:variable name="id" select="@contentid"/>
        <xsl:apply-templates select="/Page/Contents/Content[@id=$id]" mode="calendarEntry" />
      </xsl:for-each>
    </td>
  </xsl:template>

  <!-- Generic Calendar Entry -->
  <xsl:template match="Content" mode="calendarEntry">
    <div>
      <xsl:attribute name="class">
        <xsl:text>calendarentry</xsl:text>
      </xsl:attribute>
      <xsl:value-of select="@name"/>
    </div>
  </xsl:template>

  <!-- Event Calendar Entry -->
  <xsl:template match="Content[@type='Event']" mode="calendarEntry">
    <xsl:variable name="contentHref">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div>
      <xsl:attribute name="class">
        <xsl:text>calendarentry</xsl:text>
      </xsl:attribute>
      <a href="{$contentHref}" title="More details for {Headline/node()}">
        <xsl:value-of select="Headline/node()" />
      </a>
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


  <!--   #############   Sub Page Listing   ###########   -->
  <!-- Sub Page List Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='SubPageList']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="link" select="@link"/>
    <xsl:variable name="parentPage" select="//MenuItem[@id=$link]"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="showHidden" select="@showHidden"/>
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="cropSetting">
      <xsl:choose>
        <xsl:when test="@crop='true'">
          <xsl:text>true</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count($parentPage/MenuItem)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count($currentPage/MenuItem)"/>
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
    <div class="SubPages">
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
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="contentList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount" />
          </xsl:apply-templates>
        </xsl:if>
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
          <xsl:with-param name="sortBy" select="@sortBy"/>
          <xsl:with-param name="crop" select="$cropSetting"/>
          <xsl:with-param name="showHidden" select="@showHidden"/>
          <xsl:with-param name="fixedThumb" select="@fixedThumb"/>
        </xsl:apply-templates>
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- Sub Page Content -->
  <xsl:template match="MenuItem" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <xsl:param name="crop"/>
    <xsl:param name="showHidden"/>
    <xsl:param name="fixedThumb"/>
    <xsl:variable name="url">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <xsl:variable name="pageName">
      <xsl:apply-templates select="." mode="getDisplayName"/>
    </xsl:variable>
    <xsl:variable name="cropSetting">
      <xsl:choose>
        <xsl:when test="$crop='true'">
          <xsl:text>true</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:if test="(@name!='Information' and (not(DisplayName/@exclude='true'))) or (@name!='Information' and $showHidden='true')">
      <div class="list-group-item listItem subpageItem">
        <xsl:apply-templates select="." mode="inlinePopupOptions">
          <xsl:with-param name="class" select="'list-group-item listItem subpageItem'"/>
          <xsl:with-param name="sortBy" select="$sortBy"/>
        </xsl:apply-templates>
        <div class="lIinner">
          <h3 class="title">
            <xsl:apply-templates select="." mode="menuLink"/>
          </h3>
          <xsl:if test="Images/img[@src!='']">
            <a href="{$url}" title="{$pageName}">
              <xsl:attribute name="title">
                <xsl:apply-templates select="." mode="getTitleAttr"/>
              </xsl:attribute>
              <xsl:apply-templates select="." mode="displaySubPageThumb">
                <xsl:with-param name="crop" select="$cropSetting" />
                <xsl:with-param name="fixedThumb" select="$fixedThumb" />
              </xsl:apply-templates>
            </a>
          </xsl:if>
          <span class="listDescription">
            <xsl:apply-templates select="Description/node()" mode="cleanXhtml" />
            <xsl:text> </xsl:text>
          </span>
          <div class="entryFooter">
            <xsl:apply-templates select="." mode="moreLink">
              <xsl:with-param name="linkText">
                <xsl:call-template name="term2026" />
                <xsl:text>&#160;</xsl:text>
                <xsl:apply-templates select="." mode="getDisplayName" />
              </xsl:with-param>
              <xsl:with-param name="link" select="$url"/>
              <xsl:with-param name="altText">
                <xsl:apply-templates select="." mode="getTitleAttr" />
              </xsl:with-param>
            </xsl:apply-templates>
            <xsl:text> </xsl:text>
          </div>
        </div>
        <div class="terminus">&#160;</div>
      </div>
    </xsl:if>
  </xsl:template>

  <!--   #############   Sub Page Menu   ###########   -->
  <!-- Sub Page Menu Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='SubPageMenu']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="link" select="@link"/>
    <xsl:variable name="parentPage" select="//MenuItem[@id=$link]"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count($parentPage/MenuItem)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count($currentPage/MenuItem)"/>
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
    <div class="SubPageMenu">
      <div>
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="contentList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount" />
          </xsl:apply-templates>
        </xsl:if>
        <ul class="cols cols{@cols}" data-xscols="{$xsColsToShow}" data-smcols="{$smColsToShow}" data-mdcols="{$mdColsToShow}" data-slidestoshow="{@cols}">
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
          <xsl:if test="@homeLink='true'">
            <li class="first">
              <xsl:apply-templates select="$parentPage" mode="menuLink"/>
            </li>
          </xsl:if>
          <xsl:apply-templates select="ms:node-set($contentList)/*[not(DisplayName/@exclude='true')]" mode="displayMenuBrief">
            <xsl:with-param name="sortBy" select="@sortBy"/>
          </xsl:apply-templates>
          <xsl:text> </xsl:text>
        </ul>
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- Sub Page Menu Content -->
  <xsl:template match="MenuItem" mode="displayMenuBrief">
    <xsl:param name="sortBy"/>
    <xsl:variable name="url">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <xsl:variable name="pageName">
      <xsl:apply-templates select="." mode="getDisplayName"/>
    </xsl:variable>
    <xsl:if test="@name!='Information'">
      <li>
        <xsl:attribute name="class">
          <xsl:if test="position()=last()">
            <xsl:text>last</xsl:text>
          </xsl:if>
        </xsl:attribute>
        <xsl:apply-templates select="." mode="menuLink"/>
      </li>
    </xsl:if>
  </xsl:template>

  <!--   #############   Sub Page Grid   ###########   -->

  <!-- Sub Page Grid Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='SubPageGrid']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="link" select="@link"/>
    <xsl:variable name="parentPage" select="//MenuItem[@id=$link]"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="cropSetting">
      <xsl:choose>
        <xsl:when test="@crop='true'">
          <xsl:text>true</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count($parentPage/MenuItem)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count($currentPage/MenuItem)"/>
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
    <div class="SubPages SubPageGrid Grid">
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
            <xsl:with-param name="contentList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount" />
          </xsl:apply-templates>
        </xsl:if>
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="gridDisplayBrief">
          <xsl:with-param name="sortBy" select="@sortBy"/>
          <xsl:with-param name="crop" select="$cropSetting"/>
        </xsl:apply-templates>
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- Sub Page Grid Content -->
  <xsl:template match="MenuItem" mode="gridDisplayBrief">
    <xsl:param name="sortBy"/>
    <xsl:param name="crop"/>
    <xsl:variable name="url">
      <xsl:apply-templates select="." mode="getHref"/>
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
      <!--<xsl:value-of select="$crop"/>-->
    </xsl:variable>
    <xsl:variable name="lg-max-width">
      <xsl:apply-templates select="." mode="getFullSizeWidth"/>
    </xsl:variable>
    <xsl:variable name="lg-max-height">
      <xsl:apply-templates select="." mode="getFullSizeHeight"/>
    </xsl:variable>
    <xsl:if test="@name!='Information' and @name!='Footer' and not(DisplayName/@exclude='true')">
      <div class="grid-item">
        <xsl:apply-templates select="." mode="inlinePopupOptions">
          <xsl:with-param name="class" select="'grid-item hproduct'"/>
          <xsl:with-param name="sortBy" select="$sortBy"/>
        </xsl:apply-templates>
        <a href="{$url}" title="{@name}" class="url">
          <div class="thumbnail">
            <xsl:if test="Images/img[@src!='']">
              <xsl:apply-templates select="." mode="displayThumbnail">
                <xsl:with-param name="crop" select="$cropSetting" />
                <xsl:with-param name="class" select="'img-responsive'" />
                <xsl:with-param name="style" select="'overflow:hidden;'" />
                <xsl:with-param name="width" select="$lg-max-width"/>
                <xsl:with-param name="height" select="$lg-max-height"/>
              </xsl:apply-templates>
            </xsl:if>
            <xsl:if test="DisplayName/@icon!=''">
              <i>
                <xsl:attribute name="class">
                  <xsl:text>fa fa-3x center-block </xsl:text>
                  <xsl:value-of select="DisplayName/@icon"/>
                </xsl:attribute>
                <xsl:text> </xsl:text>
              </i>
              <xsl:text> </xsl:text>
            </xsl:if>
            <xsl:if test="DisplayName/@uploadIcon!='' and DisplayName/@uploadIcon!='_'">
              <span class="upload-icon">
                <img src="{DisplayName/@uploadIcon}" alt="icon" class="center-block img-responsive"/>
              </span>
            </xsl:if>
            <div class="caption">
              <h4>
                <xsl:apply-templates select="." mode="getDisplayName"/>
              </h4>
            </div>
          </div>

        </a>
      </div>
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
  <!--   ################   Links   ###############   -->

  <!-- Links Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='LinkList']" mode="displayBrief">
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="cropSetting">
      <xsl:choose>
        <xsl:when test="@crop='true'">
          <xsl:text>true</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
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
    <div class="clearfix Links">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix Links content-scroller</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <div class="cols cols{@cols}" data-xscols="{$xsColsToShow}" data-smcols="{$smColsToShow}" data-mdcols="{$mdColsToShow}" data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1" data-dots="{@carouselBullets}">
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
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="linkList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
          <xsl:with-param name="sortBy" select="@sortBy"/>
          <xsl:with-param name="crop" select="$cropSetting"/>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

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

  <!-- Simple Links Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='LinkListSimple']" mode="displayBrief">
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
    <div class="clearfix LinkListSimple">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix LinkListSimple content-scroller</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <div class="cols cols{@cols}" data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1" data-dots="{@carouselBullets}">
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
            <xsl:with-param name="linkList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefSimple">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <!-- Simple Links Brief -->
  <xsl:template match="Content[@type='Link']" mode="displayBriefSimple">
    <xsl:param name="sortBy"/>
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

    <div class="list-group-item listItem linkSimple">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'list-group-item listItem linkSimple'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <xsl:if test="Images/img/@src!=''">
        <a href="{$linkURL}" title="Click here to link to {Name}">
          <xsl:if test="not(substring(@linkURL,1,1)='/') and (contains(@linkURL,'http://') and Url/@type='external')">
            <xsl:attribute name="rel">external</xsl:attribute>
          </xsl:if>
          <xsl:apply-templates select="." mode="displayThumbnail"/>
        </a>
      </xsl:if>
      <a href="{$linkURL}" title="{Name}">
        <xsl:if test="not(substring(@linkURL,1,1)='/') and (contains(@linkURL,'http://') and Url/@type='external')">
          <xsl:attribute name="rel">external</xsl:attribute>
          <xsl:attribute name="class">extLink</xsl:attribute>
        </xsl:if>
        <xsl:value-of select="Name"/>
      </a>
    </div>
  </xsl:template>

  <!-- Links Grid Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='LinkListGrid']" mode="displayBrief">
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
        <xsl:with-param name="parentClass" select="concat('cols',@cols)" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="cropSetting">
      <xsl:choose>
        <xsl:when test="@crop='true'">
          <xsl:text>true</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
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
    <div class="Links Grid">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix Links Grid content-scroller</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <div class="cols cols{@cols}" data-xscols="{$xsColsToShow}" data-smcols="{$smColsToShow}" data-mdcols="{$mdColsToShow}" data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1" data-dots="{@carouselBullets}" data-height="{@carouselHeight}" >
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
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="linkList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:apply-templates select="ms:node-set($contentList)/node()" mode="displayBriefLinkGrid">
          <xsl:with-param name="sortBy" select="@sortBy"/>
          <xsl:with-param name="crop" select="$cropSetting"/>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <!-- Links Grid Brief-->
  <xsl:template match="Content[@type='Link']" mode="displayBriefLinkGrid">
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
    <xsl:variable name="parentClass" select="preceding-sibling::*[name()='Parent']/@class"/>
    <div class="grid-item">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'grid-item hproduct'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <a href="{$linkURL}" title="{Name}" class="url">
        <xsl:if test="@target!=''">
          <xsl:attribute name="target">
            <xsl:value-of select="@target"/>
          </xsl:attribute>
        </xsl:if>
        <div class="thumbnail">
          <xsl:choose>
            <xsl:when test="$cropSetting='true'">
              <xsl:apply-templates select="." mode="displayThumbnail">
                <xsl:with-param name="crop" select="$cropSetting" />
                <xsl:with-param name="class" select="'img-responsive'" />
                <xsl:with-param name="style" select="'overflow:hidden;'" />
                <!--<xsl:with-param name="width" select="$lg-max-width"/>
              <xsl:with-param name="height" select="$lg-max-height"/>-->
              </xsl:apply-templates>
            </xsl:when>
            <xsl:otherwise>
              <img src="{Images/img[@class='thumbnail']/@src}" class="img-responsive" style="overflow:hidden;"/>
            </xsl:otherwise>
          </xsl:choose>
          <div class="caption">
            <h4>
              <xsl:apply-templates select="." mode="getDisplayName"/>
            </h4>
          </div>
        </div>
      </a>
    </div>
  </xsl:template>

  <!--   ################   Testimonials   ###############   -->

  <!-- Testimonial Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='TestimonialList']" mode="displayBrief">
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
    <!-- Output Module -->
    <div class="clearfix TestimonialList">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix TestimonialList content-scroller</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <div class="cols cols{@cols}" data-xscols="{$xsColsToShow}" data-smcols="{$smColsToShow}" data-mdcols="{$mdColsToShow}" data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1" data-dots="{@carouselBullets}" height="{@carouselHeight}">
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
        <xsl:choose>
          <xsl:when test="@linkArticle='true'">
            <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefTestimonialLinked">
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
          </xsl:otherwise>
        </xsl:choose>
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

  <!-- Testimonial Brief -->
  <xsl:template match="Content[@type='Testimonial']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <!-- testimonialBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="listItem list-group-item">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem list-group-item'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <blockquote>
          <xsl:if test="Images/img/@src!=''">
            <a href="{$parentURL}">
              <xsl:attribute name="title">
                <xsl:call-template name="term2042" />
                <xsl:text> - </xsl:text>
                <xsl:value-of select="SourceCompany/node()"/>
              </xsl:attribute>
              <xsl:apply-templates select="." mode="displayThumbnail"/>
            </a>
            <!--Accessiblity fix : Separate adjacent links with more than whitespace-->
            <span class="hidden">|</span>
          </xsl:if>
          <div class="summary">
            <xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
          </div>
          <footer>
            <cite title="{SourceName/node()}">
              <xsl:value-of select="SourceName/node()"/>
            </cite>
            <br/>
            <xsl:apply-templates select="SourceCompany" mode="displayBrief"/>
            <xsl:if test="not(@noLink='true')">
              <div class="entryFooter">
                <xsl:apply-templates select="." mode="displayTags"/>
                <xsl:apply-templates select="." mode="moreLink">
                  <xsl:with-param name="link" select="$parentURL"/>
                  <xsl:with-param name="altText">
                    <!--Testimonial from-->
                    <xsl:call-template name="term2041" />
                    <xsl:text>&#160;</xsl:text>
                    <xsl:value-of select="SourceCompany/node()"/>
                  </xsl:with-param>
                </xsl:apply-templates>
                <xsl:text> </xsl:text>
              </div>
            </xsl:if>
          </footer>
        </blockquote>
        <!-- Accessiblity fix : Separate adjacent links with more than whitespace -->
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='Testimonial']" mode="displayBriefTestimonialLinked">
    <xsl:param name="sortBy"/>
    <xsl:param name="link"/>
    <xsl:param name="altText"/>
    <xsl:param name="linkType"/>
    <!-- articleBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="listItem list-group-item newsarticle">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'newsarticle'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <a href="{$parentURL}">
        <xsl:if test="not(substring($link,1,1)='/') and (contains($link,'http://') and $linkType='external')">
          <xsl:attribute name="rel">external</xsl:attribute>
          <xsl:attribute name="class">extLink listItem list-group-item newsarticle</xsl:attribute>
        </xsl:if>
        <div class="lIinner">
          <h3 class="title">
            <xsl:apply-templates select="." mode="getDisplayName"/>
          </h3>
          <xsl:if test="Images/img/@src!=''">
            <xsl:apply-templates select="." mode="displayThumbnail"/>
            <!--Accessiblity fix : Separate adjacent links with more than whitespace-->
            <span class="hidden">|</span>
          </xsl:if>
          <xsl:apply-templates select="Content[@type='Contact']" mode="displayAuthorBrief"/>
          <xsl:if test="@publish!=''">
            <p class="date">
              <xsl:value-of select="/Page/Contents/Content[@name='articleLabel']"/>
              <xsl:call-template name="DisplayDate">
                <xsl:with-param name="date" select="@publish"/>
              </xsl:call-template>
            </p>
          </xsl:if>
          <xsl:if test="Strapline/node()!=''">
            <div class="summary">
              <xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
            </div>
          </xsl:if>
          <!-- Accessiblity fix : Separate adjacent links with more than whitespace -->
          <div class="terminus">&#160;</div>
        </div>
      </a>
    </div>
  </xsl:template>


  <!-- Testimonial Detail -->
  <xsl:template match="Content[@type='Testimonial']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <div class="detail testimonial">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail testimonial'"/>
      </xsl:apply-templates>
      <xsl:apply-templates select="." mode="displayDetailImage"/>
      <h2 class="entry-title content-title">
        <xsl:value-of select="SourceCompany/node()"/>
      </h2>
      <p class="lead">
        <xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
      </p>
      <div class="entry-content">
        <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
      </div>
      <div class="terminus">&#160;</div>
      <div class="source">
        <p>
          <xsl:if test="SourceName/node()!=''">
            <span class="sourceName">
              <xsl:apply-templates select="SourceName/node()" mode="displayBrief"/>
            </span>
          </xsl:if>
          <xsl:if test="SourceCompany/node()!=''">
            <br />
            <xsl:apply-templates select="SourceCompany/node()" mode="displayBrief"/>
          </xsl:if>
        </p>
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
            <!--click here to return to the testimonial list-->
            <xsl:call-template name="term2043" />
          </xsl:with-param>
        </xsl:apply-templates>
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
          `         <xsl:if test="@linkedInURL!=''">
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


  <!--   ################   Videos   ###############   -->
  <!-- Video list Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='VideoList']" mode="displayBrief">
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
    <div class="clearfix VideoList">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix VideoList content-scroller</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <div class="cols cols{@cols}" data-xscols="{$xsColsToShow}" data-smcols="{$smColsToShow}" data-mdcols="{$mdColsToShow}" data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1">
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

  <!-- Returns video width -->
  <xsl:template match="Content[@moduleType='Video']" mode="videoWidth">
    <xsl:choose>
      <xsl:when test="@size='Manual'">
        <xsl:value-of select="@width"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>640</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Returns video height -->
  <xsl:template match="Content[@moduleType='Video']" mode="videoHeight">
    <xsl:choose>
      <xsl:when test="@size='Manual'">
        <xsl:value-of select="@height"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="@ratio='SixteenNine'">
            <xsl:text>360</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>480</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Returns classes for video container -->
  <xsl:template match="Content[@moduleType='Video']" mode="videoClasses">
    <xsl:text>Video VideoType</xsl:text>
    <xsl:value-of select="@videoType"/>
    <xsl:text> VideoSize</xsl:text>
    <xsl:value-of select="@size"/>
  </xsl:template>

  <!-- Video Brief -->
  <xsl:template match="Content[@type='Video']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="list-group-item listItem Video">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'list-group-item listItem Video'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <a href="{$parentURL}">
          <xsl:apply-templates select="." mode="displayThumbnail"/>
        </a>
        <a href="{$parentURL}">
          <h3 class="title content-title">
            <xsl:value-of select="Title/node()"/>
          </h3>
        </a>
        <xsl:if test="Author/node()!=''">
          <p class="author">
            <span class="label">
              <!--Author-->
              <xsl:call-template name="term2045" />
              <xsl:text>: </xsl:text>
            </span>
            <xsl:value-of select="Author/node()"/>
          </p>
        </xsl:if>
        <xsl:if test="Copyright/node()!=''">
          <p class="copyright">
            <span class="label">
              <!--Copyright-->
              <xsl:call-template name="term2046" />
              <xsl:text>: </xsl:text>
            </span>
            <xsl:value-of select="Copyright/node()"/>
          </p>
        </xsl:if>
        <xsl:if test="Intro/node()!=''">
          <p class="VideoDescription">
            <xsl:apply-templates select="Intro" mode="cleanXhtml"/>
          </p>
        </xsl:if>
        <div class="entryFooter">
          <xsl:apply-templates select="." mode="displayTags"/>
          <xsl:apply-templates select="." mode="moreLink">
            <xsl:with-param name="link" select="$parentURL"/>
            <xsl:with-param name="altText">
              <xsl:value-of select="Title/node()"/>
            </xsl:with-param>
          </xsl:apply-templates>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='Video']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <div id="Video{@id}" class="detail Video">
      <div class="row">
        <div class="col-md-4">
          <h1 class="title content-title">
            <xsl:value-of select="Title/node()"/>
          </h1>
          <xsl:if test="Body/node()!=''">
            <p class="description">
              <span class="label">
                <xsl:call-template name="term2092" />
                <xsl:text>: </xsl:text>
              </span>
              <xsl:apply-templates select="Body" mode="cleanXhtml"/>
            </p>
          </xsl:if>
          <xsl:if test="Author/node()!=''">
            <p class="author">
              <span class="label">
                <xsl:call-template name="term2045" />
                <xsl:text>: </xsl:text>
              </span>
              <xsl:value-of select="Author/node()"/>
            </p>
          </xsl:if>
          <xsl:if test="Copyright/node()!=''">
            <p class="copyright">
              <span class="label">
                <xsl:call-template name="term2046" />
                <xsl:text>: </xsl:text>
              </span>
              <xsl:value-of select="Copyright/node()"/>
            </p>
          </xsl:if>
          <div class="entryFooter hidden-xs hidden-sm">
            <div class="tags">
              <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
              <xsl:text> </xsl:text>
            </div>
            <xsl:apply-templates select="." mode="backLink">
              <xsl:with-param name="link" select="$thisURL"/>
              <xsl:with-param name="altText">
                <xsl:call-template name="term2047" />
              </xsl:with-param>
            </xsl:apply-templates>
          </div>
          <div class="terminus">&#160;</div>
        </div>
        <div class="col-md-8">
          <xsl:apply-templates select="." mode="VideoDetailDisplay">
            <xsl:with-param name="classes" select="'col-md-8'"/>
          </xsl:apply-templates>
        </div>

        <div class="entryFooter container hidden-md hidden-lg">
          <div class="tags">
            <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
            <xsl:text> </xsl:text>
          </div>
          <xsl:apply-templates select="." mode="backLink">
            <xsl:with-param name="link" select="$thisURL"/>
            <xsl:with-param name="altText">
              <xsl:call-template name="term2047" />
            </xsl:with-param>
          </xsl:apply-templates>
        </div>
      </div>
    </div>
  </xsl:template>

  <!-- Local video -->
  <xsl:template match="Content[@moduleType='Video' and @videoType='Local']" mode="displayBrief">
    <div id="Video{@id}" class="Video">
      <xsl:attribute name="class">
        <xsl:apply-templates select="." mode="videoClasses"/>
      </xsl:attribute>
      <div id="FVPlayer{@id}">
        <a href="http://www.adobe.com/go/getflashplayer">
          <xsl:call-template name="term2004" />
        </a>
        <xsl:text>&#160;</xsl:text>
        <xsl:call-template name="term2005" />
        <xsl:if test="Local/img/@src!=''">
          <xsl:apply-templates select="Local/img" mode="cleanXhtml"/>
        </xsl:if>
      </div>
      <script type="text/javascript">
        <xsl:text>var s1 = new SWFObject('/ewcommon/flash/flvplayer.swf','Flash_</xsl:text>
        <xsl:value-of select="@id"/>
        <xsl:text>', '</xsl:text>
        <xsl:apply-templates select="." mode="videoWidth"/>
        <xsl:text>', '</xsl:text>
        <xsl:apply-templates select="." mode="videoHeight"/>
        <xsl:text>', '7', '7');</xsl:text>
        <xsl:text>s1.addParam('allowfullscreen', 'true');</xsl:text>
        <xsl:text>s1.addParam('wmode', 'transparent');</xsl:text>
        <xsl:text>s1.addVariable('file','</xsl:text>
        <xsl:value-of select="Local/@url"/>
        <xsl:text>');</xsl:text>
        <xsl:if test="Local/img/@src!=''">
          <xsl:text>s1.addVariable('image','</xsl:text>
          <xsl:value-of select="Local/img/@src"/>
          <xsl:text>');</xsl:text>
        </xsl:if>
        <xsl:text>s1.write('FVPlayer</xsl:text>
        <xsl:value-of select="@id"/>
        <xsl:text>');</xsl:text>
      </script>
    </div>
  </xsl:template>

  <!-- YouTube video -->
  <xsl:template match="Content[@moduleType='Video' and @videoType='YouTube']" mode="displayBrief">
    <xsl:variable name="code">
      <xsl:variable name="raw" select="YouTube/@code"/>
      <xsl:choose>
        <!-- http://youtu.be/abcd1234 -->
        <xsl:when test="contains($raw, 'youtu.be/')">
          <xsl:value-of select="substring(substring-after($raw, 'youtu.be/'), 1, 11)"/>
        </xsl:when>
        <!-- http://youtube.com/watch?v=abcd1234 -->
        <xsl:when test="contains($raw, 'v=')">
          <xsl:value-of select="substring(substring-after($raw, 'v='), 1, 11)"/>
        </xsl:when>
        <xsl:when test="contains($raw, 'youtube.com/embed/')">
          <xsl:value-of select="substring(substring-after($raw, 'youtube.com/embed/'), 1, 11)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="substring($raw, 1, 11)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div id="Video{@id}" class="Video">
      <xsl:if test="@size!='Manual'">
        <xsl:attribute name="class">
          <xsl:text>embed-responsive </xsl:text>
          <xsl:choose>
            <xsl:when test="@ratio='FourThree'">embed-responsive-4by3</xsl:when>
            <xsl:otherwise>embed-responsive-16by9</xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </xsl:if>
      <iframe frameborder="0" class="embed-responsive-item" allowfullscreen="allowfullscreen" >
        <xsl:attribute name="src">
          <xsl:text>http</xsl:text>
          <xsl:if test="YouTube/@useHttps='true'">
            <xsl:text>s</xsl:text>
          </xsl:if>
          <xsl:text>://www.youtube.com/embed/</xsl:text>
          <xsl:value-of select="$code"/>
          <xsl:text>?wmode=transparent&amp;rel=0</xsl:text>
          <xsl:if test="YouTube/@showSuggested='true'">&amp;rel=1</xsl:if>
        </xsl:attribute>
        <xsl:choose>
          <xsl:when test="@size='Manual'">
            <xsl:if test="@width!=''">
              <xsl:attribute name="width">
                <xsl:value-of select="@width"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="@height!=''">
              <xsl:attribute name="height">
                <xsl:value-of select="@height"/>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="class">
              <xsl:text>embed-responsive-item</xsl:text>
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text> </xsl:text>
      </iframe>
    </div>
  </xsl:template>

  <!-- Vimeo video -->
  <xsl:template match="Content[@moduleType='Video' and @videoType='Vimeo']" mode="displayBrief">
    <xsl:variable name="code">
      <xsl:variable name="raw" select="Vimeo/@code"/>
      <xsl:choose>
        <xsl:when test="contains($raw, 'vimeo.com/video/')">
          <xsl:value-of select="substring(substring-after($raw, 'vimeo.com/video/'), 1, 9)"/>
        </xsl:when>
        <xsl:when test="contains($raw, 'vimeo.com/')">
          <xsl:value-of select="substring(substring-after($raw, 'vimeo.com/'), 1, 9)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$raw"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div id="Video{@id}" class="Video">
      <xsl:if test="@size!='Manual'">
        <xsl:attribute name="class">
          <xsl:text>embed-responsive </xsl:text>
          <xsl:choose>
            <xsl:when test="@ratio='FourThree'">embed-responsive-4by3</xsl:when>
            <xsl:otherwise>embed-responsive-16by9</xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </xsl:if>
      <iframe frameborder="0" class="embed-responsive-item" allowfullscreen="allowfullscreen" >
        <xsl:attribute name="src">
          <xsl:text>//player.vimeo.com/video/</xsl:text>
          <xsl:value-of select="$code"/>
          <!-- Turn all options off by default -->
          <xsl:text>/?title=0&amp;byline=0&amp;portrait=0&amp;autoplay=0&amp;loop=0</xsl:text>
          <xsl:if test="Vimeo/@title='true'">&amp;title=1</xsl:if>
          <xsl:if test="Vimeo/@byline='true'">&amp;byline=1</xsl:if>
          <xsl:if test="Vimeo/@portrait='true'">&amp;portrait=1</xsl:if>
          <xsl:if test="Vimeo/@autoplay='true'">&amp;autoplay=1</xsl:if>
          <xsl:if test="Vimeo/@loop='true'">&amp;loop=1</xsl:if>
        </xsl:attribute>
        <xsl:choose>
          <xsl:when test="@size='Manual'">
            <xsl:if test="@width!=''">
              <xsl:attribute name="width">
                <xsl:value-of select="@width"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="@height!=''">
              <xsl:attribute name="height">
                <xsl:value-of select="@height"/>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="class">
              <xsl:text>embed-responsive-item</xsl:text>
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text> </xsl:text>
      </iframe>
    </div>
  </xsl:template>

  <!--HTML5-->
  <xsl:template match="Content[@moduleType='Video' and @videoType='HTML5']" mode="displayBrief">
    <xsl:if test="HTML5/@videoMp4!='' or HTML5/@videoGG!='' or  HTML5/@videoWebm!=''">
      <div>
        <xsl:if test="@size!='Manual'">
          <xsl:attribute name="class">
            <xsl:text>embed-responsive </xsl:text>
            <xsl:choose>
              <xsl:when test="@ratio='FourThree'">embed-responsive-4by3</xsl:when>
              <xsl:otherwise>embed-responsive-16by9</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
        </xsl:if>
        <video>
          <xsl:choose>
            <xsl:when test="@size='Manual'">
              <xsl:if test="@width!=''">
                <xsl:attribute name="width">
                  <xsl:value-of select="@width"/>
                </xsl:attribute>
              </xsl:if>
              <xsl:if test="@height!=''">
                <xsl:attribute name="height">
                  <xsl:value-of select="@height"/>
                </xsl:attribute>
              </xsl:if>
            </xsl:when>
            <xsl:otherwise>
              <xsl:attribute name="class">
                <xsl:text>embed-responsive-item</xsl:text>
              </xsl:attribute>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:if test="HTML5/@autoplay='autoplay'">
            <xsl:attribute name="autoplay">
              <xsl:text>autoplay</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@controls='controls'">
            <xsl:attribute name="controls">
              <xsl:text>controls</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@loop='loop'">
            <xsl:attribute name="loop">
              <xsl:text>loop</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@muted='muted'">
            <xsl:attribute name="muted">
              <xsl:text>muted</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@preload='auto'">
            <xsl:attribute name="preload">
              <xsl:value-of select="HTML5/@preload"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@preload='metadata'">
            <xsl:attribute name="preload">
              <xsl:value-of select="HTML5/@preload"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@preload='none'">
            <xsl:attribute name="preload">
              <xsl:value-of select="HTML5/@preload"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/img/@src!=''">
            <xsl:attribute name="poster">
              <xsl:value-of select="HTML5/img/@src"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@videoMp4!=''">
            <source src="{HTML5/@videoMp4}" type="video/mp4"/>
          </xsl:if>
          <xsl:if test="HTML5/@videoGG!=''">
            <source src="{HTML5/@videoGG}" type="video/ogg"/>
          </xsl:if>
          <xsl:if test="HTML5/@videoWebm!=''">
            <source src="{HTML5/@videoWebm}" type="video/webm"/>
          </xsl:if>
        </video>
      </div>
    </xsl:if>
  </xsl:template>

  <!-- Video Detail -->
  <xsl:template match="Content[@type='Video']" mode="VideoDetailDisplay">
    <xsl:param name="classes"/>
    <xsl:apply-templates select="." mode="inlinePopupOptions">
      <xsl:with-param name="class" select="$classes"/>
    </xsl:apply-templates>
    <div id="Video{@id}" class="Video">
      <xsl:attribute name="class">
        <xsl:apply-templates select="." mode="videoClasses"/>
      </xsl:attribute>
      <div id="FVPlayer{@id}" >
        <xsl:if test="VideoSize!='Manual'">
          <xsl:attribute name="class">
            <xsl:text>embed-responsive </xsl:text>
            <xsl:choose>
              <xsl:when test="VideoRatio='FourThree'">embed-responsive-4by3</xsl:when>
              <xsl:otherwise>embed-responsive-16by9 2</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
        </xsl:if>
        <a href="http://www.adobe.com/go/getflashplayer">
          <xsl:call-template name="term2004" />
        </a>
        <xsl:text>&#160;</xsl:text>
        <xsl:call-template name="term2005" />
        <xsl:if test="Local/img/@src!=''">
          <xsl:apply-templates select="Local/img" mode="cleanXhtml"/>
        </xsl:if>
      </div>
      <script type="text/javascript">
        <xsl:text>var s1 = new SWFObject('/ewcommon/flash/flvplayer.swf','Flash_</xsl:text>
        <xsl:value-of select="@id"/>
        <xsl:text>', 'auto</xsl:text>

        <xsl:text>', 'auto</xsl:text>

        <xsl:text>', '7', '7');</xsl:text>
        <xsl:text>s1.addParam('allowfullscreen', 'true');</xsl:text>
        <xsl:text>s1.addParam('wmode', 'transparent');</xsl:text>
        <xsl:choose>
          <xsl:when test="Local/@url!=''">
            <xsl:text>s1.addVariable('file','</xsl:text>
            <xsl:value-of select="Local/@url"/>
            <xsl:text>');</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>s1.addVariable('file','</xsl:text>
            <xsl:value-of select="Movies/filename/@src"/>
            <xsl:text>');</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:if test="Local/img/@src!=''">
          <xsl:text>s1.addVariable('image','</xsl:text>
          <xsl:value-of select="Local/img/@src"/>
          <xsl:text>');</xsl:text>
        </xsl:if>
        <xsl:text>s1.write('FVPlayer</xsl:text>
        <xsl:value-of select="@id"/>
        <xsl:text>');</xsl:text>

        <xsl:text>videoSizeAuto();</xsl:text>
      </script>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='Video' and VideoType='HTML5']" mode="VideoDetailDisplay">
    <xsl:apply-templates select="." mode="inlinePopupOptions"/>
    <xsl:if test="HTML5/@videoMp4!='' or HTML5/@videoGG!='' or  HTML5/@videoWebm!=''">
      <div>
        <xsl:if test="VideoSize!='Manual'">
          <xsl:attribute name="class">
            <xsl:text>embed-responsive </xsl:text>
            <xsl:choose>
              <xsl:when test="VideoRatio='FourThree'">embed-responsive-4by3</xsl:when>
              <xsl:otherwise>embed-responsive-16by9 1</xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
        </xsl:if>
        <video>
          <xsl:choose>
            <xsl:when test="VideoSize='Manual'">
              <xsl:if test="VideoWidth!=''">
                <xsl:attribute name="width">
                  <xsl:value-of select="VideoWidth"/>
                </xsl:attribute>
              </xsl:if>
              <xsl:if test="VideoHeight!=''">
                <xsl:attribute name="height">
                  <xsl:value-of select="VideoHeight"/>
                </xsl:attribute>
              </xsl:if>
            </xsl:when>
            <xsl:otherwise>
              <xsl:attribute name="class">
                <xsl:text>embed-responsive-item</xsl:text>
              </xsl:attribute>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:if test="HTML5/@autoplay='autoplay'">
            <xsl:attribute name="autoplay">
              <xsl:text>autoplay</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@controls='controls'">
            <xsl:attribute name="controls">
              <xsl:text>controls</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@loop='loop'">
            <xsl:attribute name="loop">
              <xsl:text>loop</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@muted='muted'">
            <xsl:attribute name="muted">
              <xsl:text>muted</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@preload='auto'">
            <xsl:attribute name="preload">
              <xsl:value-of select="HTML5/@preload"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@preload='metadata'">
            <xsl:attribute name="preload">
              <xsl:value-of select="HTML5/@preload"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@preload='none'">
            <xsl:attribute name="preload">
              <xsl:value-of select="HTML5/@preload"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/img/@src!=''">
            <xsl:attribute name="poster">
              <xsl:value-of select="HTML5/img/@src"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="HTML5/@videoMp4!=''">
            <source src="{HTML5/@videoMp4}" type="video/mp4"/>
          </xsl:if>
          <xsl:if test="HTML5/@videoGG!=''">
            <source src="{HTML5/@videoGG}" type="video/ogg"/>
          </xsl:if>
          <xsl:if test="HTML5/@videoWebm!=''">
            <source src="{HTML5/@videoWebm}" type="video/webm"/>
          </xsl:if>
        </video>
      </div>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Content[@type='Video' and VideoType='Vimeo']" mode="VideoDetailDisplay">
    <xsl:variable name="code">
      <xsl:variable name="raw" select="Vimeo/@code"/>
      <xsl:choose>
        <xsl:when test="contains($raw, 'vimeo.com/video/')">
          <xsl:value-of select="substring(substring-after($raw, 'vimeo.com/video/'), 1, 9)"/>
        </xsl:when>
        <xsl:when test="contains($raw, 'vimeo.com/')">
          <xsl:value-of select="substring(substring-after($raw, 'vimeo.com/'), 1, 9)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$raw"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:apply-templates select="." mode="inlinePopupOptions"/>
    <div id="Video{@id}" class="Video">
      <xsl:if test="VideoSize!='Manual'">
        <xsl:attribute name="class">
          <xsl:text>embed-responsive </xsl:text>
          <xsl:choose>
            <xsl:when test="VideoRatio='FourThree'">embed-responsive-4by3</xsl:when>
            <xsl:otherwise>embed-responsive-16by9 </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </xsl:if>
      <iframe frameborder="0" class="embed-responsive-item" >
        <xsl:attribute name="src">
          <xsl:text>http://player.vimeo.com/video/</xsl:text>
          <xsl:value-of select="$code"/>
          <!-- Turn all options off by default -->
          <xsl:text>/?title=0&amp;byline=0&amp;portrait=0&amp;autoplay=0&amp;loop=0</xsl:text>
          <xsl:if test="Vimeo/@title='true'">&amp;title=1</xsl:if>
          <xsl:if test="Vimeo/@byline='true'">&amp;byline=1</xsl:if>
          <xsl:if test="Vimeo/@portrait='true'">&amp;portrait=1</xsl:if>
          <xsl:if test="Vimeo/@autoplay='true'">&amp;autoplay=1</xsl:if>
          <xsl:if test="Vimeo/@loop='true'">&amp;loop=1</xsl:if>
        </xsl:attribute>
        <xsl:choose>
          <xsl:when test="VideoSize='Manual'">
            <xsl:if test="@width!=''">
              <xsl:attribute name="width">
                <xsl:value-of select="VideoWidth"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="VideoHeight!=''">
              <xsl:attribute name="height">
                <xsl:value-of select="VideoHeight"/>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="class">
              <xsl:text>embed-responsive-item</xsl:text>
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text> </xsl:text>
      </iframe>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='Video' and VideoType='YouTube']" mode="VideoDetailDisplay">
    <xsl:variable name="code">
      <xsl:variable name="raw" select="YouTube/@code"/>
      <xsl:choose>
        <!-- http://youtu.be/abcd1234 -->
        <xsl:when test="contains($raw, 'youtu.be/')">
          <xsl:value-of select="substring(substring-after($raw, 'youtu.be/'), 1, 11)"/>
        </xsl:when>
        <!-- http://youtube.com/watch?v=abcd1234 -->
        <xsl:when test="contains($raw, 'v=')">
          <xsl:value-of select="substring(substring-after($raw, 'v='), 1, 11)"/>
        </xsl:when>
        <xsl:when test="contains($raw, 'youtube.com/embed/')">
          <xsl:value-of select="substring(substring-after($raw, 'youtube.com/embed/'), 1, 11)"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="substring($raw, 1, 11)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:apply-templates select="." mode="inlinePopupOptions"/>
    <div id="Video{@id}" class="Video">
      <xsl:if test="VideoSize!='Manual'">
        <xsl:attribute name="class">
          <xsl:text>embed-responsive </xsl:text>
          <xsl:choose>
            <xsl:when test="VideoRatio='FourThree'">embed-responsive-4by3</xsl:when>
            <xsl:otherwise>embed-responsive-16by9 </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </xsl:if>
      <iframe frameborder="0" class="embed-responsive-item" allowfullscreen="allowfullscreen">
        <xsl:attribute name="src">
          <xsl:text>http</xsl:text>
          <xsl:if test="YouTube/@useHttps='true'">
            <xsl:text>s</xsl:text>
          </xsl:if>
          <xsl:text>://www.youtube.com/embed/</xsl:text>
          <xsl:value-of select="$code"/>
          <xsl:text>?wmode=transparent&amp;rel=0</xsl:text>
          <xsl:if test="YouTube/@showSuggested='true'">&amp;rel=1</xsl:if>
        </xsl:attribute>
        <xsl:choose>
          <xsl:when test="VideoSize='Manual'">
            <xsl:if test="VideoWidth!=''">
              <xsl:attribute name="width">
                <xsl:value-of select="VideoWidth"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="VideoHeight!=''">
              <xsl:attribute name="height">
                <xsl:value-of select="VideoHeight"/>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="class">
              <xsl:text>embed-responsive-item</xsl:text>
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text> </xsl:text>
      </iframe>
    </div>
  </xsl:template>

  <!-- ##################################### -->
  <!-- ### End of Video module templates ### -->
  <!-- ##################################### -->


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
        <div class="terminus">&#160;</div>
      </div>
    </div>
    <script type="text/javascript">
      <xsl:text>$(document).ready(function() {
      var tn1 = $('.mygallery').tn3({
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
      <xsl:value-of select="@advancedSpeed"/>
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
    <div class="contentSliderGallery">
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

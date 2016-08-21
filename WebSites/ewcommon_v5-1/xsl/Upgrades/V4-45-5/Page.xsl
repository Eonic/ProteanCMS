<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns:ew="urn:ew">
  <xsl:output method="xml" indent="no" standalone="yes" omit-xml-declaration="yes" encoding="UTF-8"/>

  <!--  
        FOR A LONG TIME - The setup routine setup the first 5 pages missing alot of nodes.
        This file should be kept up to date with the Page.xml     
        We can run this to rectify the problem and add any new developments for Page.
        WH - 2011-02-24
  -->

  <xsl:variable name="pageId" select="/instance/tblContentStructure/nStructKey/node()"/>

  <xsl:template match="/instance">
    <xsl:variable name="layout" select="tblContentStructure/cStructLayout/node()" />
    <instance>
      <xsl:for-each select="*">
        <xsl:apply-templates select="." mode="writeNodes"/>
      </xsl:for-each>
        
        <!-- FOR OLD PAGE LAYOUTS, ADD IN THE MODULES NEEDED TO ACHEIVE SAME LAYOUT -->
        <xsl:choose>
            <xsl:when test="not(contains($layout,'Modules_'))">


                <xsl:variable name="modulesNeeded">
                    <!--  returns XML nodes sets, with possibly multiple Module nodes e.g.
                     <Module position="column1" box="false" moduleType="/xForms/Content/Module/ProductList.xml" sortBy="Position" order="" stepCount="4" cols="2"/>
                     Can then be iterated to get the correct instance back, and slot the configurable values in.
               -->
                    <xsl:call-template name="getModulesNeeded">
                        <xsl:with-param name="layout" select="$layout" />
                    </xsl:call-template>
                </xsl:variable>
                <xsl:if test="ms:node-set($modulesNeeded)/*">
                    <Contents>
                        <!-- for each module required -->
                        <xsl:for-each select="ms:node-set($modulesNeeded)/*">
                            <xsl:variable name="myNewContentInstance" select="ew:GetContentInstance(name(),@moduleType)"/>

                            <!-- Then iterate through the nodeset, configuring 
                            ... but just do a copy of for now while testing
                            -->
                            <!--xsl:apply-templates select="ms:node-set($myNewContentInstance)" mode="writeNodes"/-->
                            <xsl:apply-templates select="ms:node-set($myNewContentInstance)" mode="writeContent">
                                <xsl:with-param name="moduleSettings" select="." />
                            </xsl:apply-templates>
                        </xsl:for-each>
                    </Contents>
                </xsl:if>
            </xsl:when>
        </xsl:choose>
    </instance>
  </xsl:template>


  <!-- -->
  <xsl:template match="*" mode="writeNodes">
    <xsl:element name="{name()}">
      <xsl:for-each select="@*">
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>

  <!-- -->

  <xsl:template match="img" mode="writeNodes">
    <xsl:variable name="img">
      <xsl:element name="{name()}">
        <xsl:for-each select="@*">
          <xsl:attribute name="{name()}">
            <xsl:value-of select="." />
          </xsl:attribute>
        </xsl:for-each>
        <xsl:apply-templates mode="writeNodes"/>
      </xsl:element>
    </xsl:variable>
    <xsl:copy-of select="ms:node-set($img)"/>
  </xsl:template>

  <!-- -->

  <xsl:template match="br" mode="writeNodes">
    <br/>
  </xsl:template>


  <!-- ==  ADD NEW STUFF TO THE ROOT & add in nodes if non ========================================================== -->

  <xsl:template match="cStructDescription" mode="writeNodes">
    <xsl:element name="{name()}">
      <xsl:for-each select="@*">
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>

      <!-- Add Display name-->
      <xsl:if test="not(DisplayName)">
        <DisplayName title="" linkType="internal" exclude="false" noindex="false"/>
      </xsl:if>
      <!-- Add Images -->
      <xsl:if test="not(Images)">
        <Images>
          <img class="icon" />
          <img class="thumbnail" />
          <img class="detail" />
        </Images>
      </xsl:if>
      <!-- Add Description -->
      <xsl:if test="not(Description)">
        <Description>
          <xsl:if test="not(DisplayName) and not(Images) and not(Description)">
            <xsl:apply-templates mode="writeNodes"/>
          </xsl:if>
        </Description>
      </xsl:if>
      <xsl:if test="DisplayName or Images or Description">
        <xsl:apply-templates mode="writeNodes"/>
      </xsl:if>
    </xsl:element>
  </xsl:template>


  <!-- ==  Process Display name attributes  ======================================================= -->

  <xsl:template match="DisplayName" mode="writeNodes">
    <xsl:element name="{name()}">
      <xsl:for-each select="@*">
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>

      <!-- @title -->
      <xsl:if test="not(@title)">
        <xsl:attribute name="title"></xsl:attribute>
      </xsl:if>

      <!-- @linkType -->
      <xsl:if test="not(@linkType)">
        <xsl:attribute name="linkType">
          <xsl:variable name="url" select="/instance/tblContentStructure/cUrl" />
          <xsl:choose>
            <!-- if URL empty default behaviour, or if number is internal link -->
            <xsl:when test="$url!='' and format-number($url,'0')='NaN'">
              <xsl:text>external</xsl:text>
            </xsl:when>
            <xsl:otherwise>internal</xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </xsl:if>

      <!-- @exclude -->
      <xsl:if test="not(@exclude)">
        <xsl:attribute name="exclude">false</xsl:attribute>
      </xsl:if>
      <!-- @noindex -->
      <xsl:if test="not(@noindex)">
        <xsl:attribute name="noindex">false</xsl:attribute>
      </xsl:if>

      <!-- write nodes -->
      <xsl:apply-templates mode="writeNodes"/>

    </xsl:element>
  </xsl:template>


  <!-- ==  Process Images  ======================================================= -->
  <xsl:template match="Images" mode="writeNodes">
    <xsl:element name="{name()}">
      <xsl:for-each select="@*">
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
      <!-- thumbnail -->
      <xsl:if test="not(img[@class='thumbnail'])">
        <img class="thumbnail" />
      </xsl:if>
      <!-- icon -->
      <xsl:if test="not(img[@class='icon'])">
        <img class="icon" />
      </xsl:if>
      <!-- detail -->
      <xsl:if test="not(img[@class='detail'])">
        <img class="detail" />
      </xsl:if>
    </xsl:element>
  </xsl:template>

  <!-- Changes all page layouts to be the new Modules - save manual changes -->
  <xsl:template match="cStructLayout" mode="writeNodes">
    <xsl:variable name="layout" select="node()" />
    <xsl:element name="{name()}">
      <xsl:choose>
        <xsl:when test="contains($layout,'Modules_')">
          <xsl:value-of select="$layout"/>
        </xsl:when>

        <!-- 2 column 50/50 -->
        <xsl:when test="contains($layout,'Google_Map_With') or $layout='2_Columns' or $layout='Mailform_Left' or $layout='Mailform_Right'">
          <xsl:text>Modules_2_columns</xsl:text>
        </xsl:when>

        <!-- 2 column 66/33 -->
        <xsl:when test="$layout='Google_Map'">
          <xsl:text>Modules_2_columns_66_33</xsl:text>
        </xsl:when>

        <!-- 2 column 33/66 -->
        <xsl:when test="$layout='Products_Grouped_1' or $layout='Products_Grouped_2' or $layout='Products_Grouped_3'">
          <xsl:text>Modules_2_columns_33_66</xsl:text>
        </xsl:when>

          <!-- 2 column -->
          <xsl:when test="$layout='2_Columns' or $layout='2_Columns_Left_Boxed'">
              <xsl:text>Modules_2_columns</xsl:text>
          </xsl:when>

          <xsl:otherwise>
          <xsl:text>Modules_1_column</xsl:text>
        </xsl:otherwise>
          
      </xsl:choose>
    </xsl:element>
  </xsl:template>
  
  <xsl:template name="getModulesNeeded">
    <xsl:param name="layout" />

    <!-- THINGS TO DO...
          - need to find a way to locate the Box titles, by name on the page, and place in the module title
            and delete the plaintext content
          - For subpage listings, locate the image contnet type by name and place in the pages thumbnail image tag.
    
    -->

    <xsl:choose>

      <!-- Products -->
      <xsl:when test="$layout='Products_List'">
        <Module position="column1" box="false" moduleType="ProductList" sortBy="Position" order="" stepCount="0" cols="3"/>
      </xsl:when>
      <xsl:when test="$layout='Products_List_2col'">
        <Module position="column1" box="true" moduleType="ProductList" sortBy="Position" order="" stepCount="0" cols="3"/>
      </xsl:when>
      <xsl:when test="$layout='Products_4up_stepped'">
        <Module position="column1" box="true" moduleType="ProductList" sortBy="Position" order="" stepCount="0" cols="3"/>
      </xsl:when>
      <xsl:when test="$layout='Products_10up_stepped'">
        <Module position="column1" box="true" moduleType="ProductList" sortBy="Position" order="" stepCount="0" cols="3" title="{ew:ContentQuery('productsListTitle','descendant-or-self::cContentXmlBrief/Content/node()',$pageId)}"/>
      </xsl:when>
      <xsl:when test="$layout='Products_10up_stepped_2col'">
        <Module position="column1" box="true" moduleType="ProductList" sortBy="Position" order="" stepCount="0" cols="3" title="{ew:ContentQuery('productsListTitle','descendant-or-self::cContentXmlBrief/Content/node()',$pageId)}"/>
      </xsl:when>
      <xsl:when test="$layout='Product_Gallery'">
        <Module position="column1" box="false" moduleType="ProductGallery" sortBy="Position" order="" stepCount="0" cols="3"/>
      </xsl:when>
      <xsl:when test="$layout='Product_Gallery_Boxed'">
        <Module position="column1" box="true" moduleType="ProductGallery" sortBy="Position" order="" stepCount="0" cols="3"/>
      </xsl:when>
      <xsl:when test="$layout='Product_Gallery_2'">
        <Module position="column1" box="false" moduleType="ProductGallery" sortBy="Position" order="" stepCount="0" cols="4"/>
      </xsl:when>
      <xsl:when test="$layout='Products_Grouped_1'">
        <Module position="column2" box="false" moduleType="ProductListGrouped" sortBy="Position" order="" stepCount="0" cols="4"/>
      </xsl:when>
      <xsl:when test="$layout='Products_Grouped_2'">
        <Module position="column2" box="false" moduleType="ProductListGrouped" sortBy="Position" order="" stepCount="0" cols="4"/>
      </xsl:when>
      <xsl:when test="$layout='Products_Grouped_3'">
        <Module position="column1" box="false" moduleType="GalleryImageList" sortBy="Position" stepCount="0" cols="2"/>
        <Module position="column2" box="false" moduleType="ProductListGrouped" sortBy="Position" stepCount="0" cols="1"/>
      </xsl:when>
      <!-- Contacts -->
      <xsl:when test="$layout='Contacts_List' or $layout='Contacts_List_Companies'">
        <Module position="column1" box="Default Box" moduleType="ContactList" sortBy="Company" order="ascending" />
      </xsl:when>

      <!-- Documents -->
      <xsl:when test="$layout='Documents' or $layout='Documents_2'">
        <Module position="column1" box="false" moduleType="DocumentList" sortBy="publish" order="descending" />
      </xsl:when>

      <!-- Links -->
      <xsl:when test="$layout='Links'">
        <Module position="column1" box="false" moduleType="LinkList" sortBy="Name" order="ascending" />
      </xsl:when>

      <!-- Goolge Maps -->
      <xsl:when test="$layout='Google_Map' or $layout='Google_Map_LeftCol' or $layout='Google_Map_With_RHS_Modules'">
        <Module position="column1" box="false" moduleType="GoogleMapv3" sortBy="" order="" />
      </xsl:when>
      <xsl:when test="$layout='Google_Map_With_LHS_Modules' or $layout='Google_Map_RightCol'">
        <Module position="column2" box="false"  moduleType="GoogleMapv3" sortBy="" order="" />
      </xsl:when>

      <!-- Library Images -->
      <xsl:when test="$layout='Image_Library_10up_Stepped'">
        <Module position="column1" box="false" moduleType="GalleryImageList" sortBy="Position" order="" stepCount="10" cols="5" />
      </xsl:when>
      <xsl:when test="$layout='Image_Library_9up_Stepped'">
        <Module position="column1" box="false" moduleType="GalleryImageList" sortBy="Position" order="" stepCount="9" cols="3" />
      </xsl:when>
      <xsl:when test="$layout='Image_Library_10up_Stepped_2col'">
        <Module position="column1" box="false" moduleType="GalleryImageList" sortBy="Position" order="" stepCount="10" cols="2" />
      </xsl:when>
      <xsl:when test="$layout='Image_Library_10up_Stepped_1col'">
        <Module position="column1" box="false" moduleType="GalleryImageList" sortBy="Position" order="" stepCount="10" cols="1" />
      </xsl:when>
      
      <xsl:when test="$layout='Sub_Page_Listing'">
        <Module position="column1" box="false" moduleType="SubPageList" sortBy="Position" order="" stepCount="0" cols="1" />
      </xsl:when>
      <xsl:when test="$layout='Sub_Page_Listing_Thumbnail'">
        <Module position="column1" box="false" moduleType="SubPageGrid" sortBy="Position" order="" stepCount="0" cols="4" />
      </xsl:when>
      <xsl:when test="$layout='Sub_Page_Listing_Thumbnail_3'">
        <Module position="column1" box="false" moduleType="SubPageGrid" sortBy="Position" order="" stepCount="0" cols="2" />
      </xsl:when>
      <xsl:when test="$layout='Sub_Page_Listing_Thumbnail_3Col'">
        <Module position="column1" box="false" moduleType="SubPageGrid" sortBy="Position" order="" stepCount="0" cols="3" />
      </xsl:when>      
      <xsl:when test="$layout='Sub_Page_Listing_Thumbnail_4'">
        <Module position="column1" box="false" moduleType="SubPageGrid" sortBy="Position" order="" stepCount="0" cols="4" />
      </xsl:when>

      <xsl:when test="$layout='Contacts_List'">
        <Module position="column1" box="false" moduleType="ContactList" sortBy="Position" order="" stepCount="0" cols="1" />
      </xsl:when>
      <xsl:when test="$layout='Contacts_List_Companies'">
        <Module position="column1" box="false" moduleType="ContactList" sortBy="Position" order="" stepCount="0" cols="1" />
      </xsl:when>
        
        
        <!-- FORMS -->
      <!--<xsl:when test="$layout='Mailform_Left'">
        <Module position="column1" box="false" moduleType="/xForms/Content/Module/GalleryImageList.xml" sortBy="Position" order="" stepCount="10" cols="1" />
      </xsl:when>-->

      <!-- MEMBERSHIP 
      <xsl:when test="$layout='Logon_Register'">
        <Module position="column1" box="false" moduleType="MembershipRegister" />
        <Module position="column2" box="false" moduleType="MembershipRegister" />
      </xsl:when>
-->
      <!-- Search
      <xsl:when test="$layout='Search_Results_Products_Index'">
        <Module position="column1" box="false" moduleType="Search" />
      </xsl:when>
      -->
    </xsl:choose>


  </xsl:template>



  <!-- SO WE HAVE GOT A NEW INSTANCE, WE NOW WANT TO CONFIGURE THE CONTENT TO BE ADDED  -->
  <!-- WRITE CONTENT BACK AS IS GIVEN -->
  <xsl:template match="*" mode="writeContent">
    <xsl:param name="moduleSettings" />
    <xsl:element name="{name()}">
      <xsl:for-each select="@*">
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="writeContent">
        <xsl:with-param name="moduleSettings" select="$moduleSettings" />
      </xsl:apply-templates>
    </xsl:element>
  </xsl:template>

  <!-- -->

  <xsl:template match="img" mode="writeContent">
    <xsl:variable name="img">
      <xsl:element name="{name()}">
        <xsl:for-each select="@*">
          <xsl:attribute name="{name()}">
            <xsl:value-of select="." />
          </xsl:attribute>
        </xsl:for-each>
      </xsl:element>
    </xsl:variable>
    <xsl:copy-of select="ms:node-set($img)"/>
  </xsl:template>

  <!-- -->

  <xsl:template match="br" mode="writeContent">
    <br/>
  </xsl:template>

    <xsl:template match="instance" mode="writeContent">
        <xsl:param name="moduleSettings" />
        <xsl:variable name="position" select="$moduleSettings/@position"/>
    <xsl:element name="{name()}">
      <xsl:for-each select="@*">
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
        <xsl:apply-templates mode="writeContent">
            <xsl:with-param name="moduleSettings" select="$moduleSettings" />
        </xsl:apply-templates>
      <Location primary="true" id="{$pageId}" position="{$position}"/>
    </xsl:element>
  </xsl:template>

  <!-- place in module values. -->

  <xsl:template match="Content" mode="writeContent">
    <xsl:param name="moduleSettings" />
    <xsl:element name="{name()}">
      <xsl:for-each select="@*">
        <xsl:variable name="name" select="name()"/>
        <xsl:attribute name="{$name}">
          <xsl:choose>
            <xsl:when test="$moduleSettings/@*[name()=$name] and $moduleSettings/@*[name()=$name]!=''">
              <xsl:value-of select="$moduleSettings/@*[name()=$name]"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="." />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="writeContent">
        <xsl:with-param name="moduleSettings" select="$moduleSettings" />
      </xsl:apply-templates>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
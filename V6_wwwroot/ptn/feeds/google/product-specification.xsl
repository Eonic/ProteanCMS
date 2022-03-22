<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns:g="http://base.google.com/ns/1.0">

  <!-- 
        ===================================================================================================
        ==  FOR DOCUMENTATION SEE: http://www.google.com/support/merchants/bin/answer.py?hl=en&answer=188494  
        ===================================================================================================
  -->
    
  <xsl:variable name="siteURL" select="concat('https://',/Page/Request/ServerVariables/Item[@name='SERVER_NAME'])"/>
  
  <xsl:variable name="siteName">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'SiteName'"/>
    </xsl:call-template>
  </xsl:variable>

  <!-- productsa across feeds need absolutly unique id's so getting prefix from cart config-->
  <xsl:variable name="uniqueIdPrefix">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'cart'"/>
      <xsl:with-param name="valueName" select="'OrderNoPrefix'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name="nVatRate">
    <xsl:variable name="rateFromConfig">
      <xsl:call-template name="getSettings">
        <xsl:with-param name="sectionName" select="'cart'"/>
        <xsl:with-param name="valueName" select="'TaxRate'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:value-of select="number($rateFromConfig div 100)"/>
  </xsl:variable>
  
  
  <!--RSS 2.0-->
  <xsl:template match="/Page">
    <rss version ="2.0" xmlns:g="http://base.google.com/ns/1.0">
      <channel>
        <title>
          <xsl:choose>
            <xsl:when test="$siteName!=''">
              <xsl:value-of select="$siteName"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$siteURL"/>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:text> products</xsl:text>
        </title>
        <description>
          <xsl:choose>
            <xsl:when test="$siteName!=''">
              <xsl:value-of select="$siteName"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$siteURL"/>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:text> Products</xsl:text>
        </description>
        <link>
          <xsl:value-of select="$siteURL"/>
        </link>
        <xsl:apply-templates select="Contents/Content[@type='Product']" mode="contentItem"/>

      </channel>
    </rss>
  </xsl:template>

  <xsl:template match="Content[@type='Product']" mode="contentItem">
    <xsl:variable name="parId">
      <xsl:value-of select="@parId"/>
    </xsl:variable>
    <xsl:variable name="parentURL">
      <xsl:call-template name="getContentParURL">
        <xsl:with-param name="parId" select="$parId"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="uniqueId">
      <xsl:apply-templates select="." mode="get-google-id"/>
    </xsl:variable>
    
    <item>
      <guid>
        <xsl:value-of select="$uniqueId"/>
      </guid>
      
      <g:id>
        <xsl:value-of select="$uniqueId"/>
      </g:id>
      
      <title>
        <xsl:apply-templates select="." mode="getDisplayName" />
      </title>
      
      <description>
        <xsl:apply-templates select="." mode="get-google-description" />       
      </description>
      
      <g:product_type>
        <xsl:apply-templates select="." mode="get-product_type" />       
      </g:product_type>
      
      <!-- Google product category -->
      <xsl:apply-templates select="." mode="get-google_product_type" />
      
      <link>
        <xsl:apply-templates select="." mode="get-google-url" />     
      </link>
      
      <!-- image link -->
      <xsl:apply-templates select="." mode="get-google-image_link"/>
     
      <!-- additional images -->
      <xsl:apply-templates select="." mode="get-google-additional_image_link"/>
            
      <g:condition>
        <xsl:apply-templates select="." mode="get-google-condition"/>
      </g:condition>

      <g:availability>
        <xsl:apply-templates select="." mode="get-google-availability"/>       
      </g:availability>

      <g:price>
        <xsl:apply-templates select="." mode="get-google-price"/>
      </g:price>

      <xsl:if test="@curDiscountPrice">
        <g:sale_price>
          <xsl:apply-templates select="." mode="get-google-sale_price"/>
        </g:sale_price>
      </xsl:if>

      <!-- PRODUCT IDENTIFYERS -->

      <!-- brand -->
      <g:brand>
        <xsl:apply-templates select="." mode="get-google-brand"/>
      </g:brand>

        <g:gtin>
          <xsl:apply-templates select="." mode="get-google-gtin"/>         
        </g:gtin>

        <g:mpn>
          <xsl:apply-templates select="." mode="get-google-mpn"/>         
        </g:mpn>

      <xsl:apply-templates select="." mode="getShippingCharges" /> 
   
    </item>

  </xsl:template>
  
  
  <!-- HANDLES SKU's as seperate items, using some elements of its parent products.-->
  <xsl:template match="Content[@type='Product' and @SkuOptions='skus']" mode="contentItem">
    <xsl:variable name="parId">
      <xsl:value-of select="@parId"/>
    </xsl:variable>
    <xsl:variable name="uniqueId">
      <xsl:apply-templates select="." mode="get-google-id"/>
    </xsl:variable>

    <xsl:variable name="product" select="."/>

    <!-- for each SKU -->
    <xsl:for-each select="Content[@type='SKU']">

      <item>
        <guid>
          <xsl:value-of select="$uniqueId"/>
        </guid>

        <g:id>
          <xsl:value-of select="$uniqueId"/>
        </g:id>

        <title>
          <xsl:apply-templates select="." mode="getDisplayName" />
        </title>

        <description>
          <xsl:apply-templates select="$product" mode="get-google-description" />
        </description>

        <g:product_type>
          <xsl:apply-templates select="$product" mode="get-product_type" />
        </g:product_type>

        <link>
          <xsl:apply-templates select="$product" mode="get-google-url" />
        </link>

         <!--image link--> 
        <xsl:apply-templates select="$product" mode="get-google-image_link"/>

         <!--additional images--> 
        <xsl:apply-templates select="$product" mode="get-google-additional_image_link"/>

        <g:condition>
          <xsl:apply-templates select="$product" mode="get-google-condition"/>
        </g:condition>

        <g:availability>
          <xsl:apply-templates select="." mode="get-google-availability"/>
        </g:availability>

        <g:price>
          <xsl:apply-templates select="." mode="get-google-price"/>
        </g:price>

        <xsl:if test="@curDiscountPrice">
          <g:sale_price>
            <xsl:apply-templates select="." mode="get-google-sale_price"/>
          </g:sale_price>
        </xsl:if>

         <!--PRODUCT IDENTIFYERS 

         brand--> 
        <g:brand>
          <xsl:apply-templates select="$product" mode="get-google-brand"/>
        </g:brand>

        <g:gtin>
          <xsl:apply-templates select="." mode="get-google-gtin"/>
        </g:gtin>

        <g:mpn>
          <xsl:apply-templates select="." mode="get-google-mpn"/>
        </g:mpn>

        <xsl:apply-templates select="." mode="getShippingCharges" />

      </item>
    </xsl:for-each>
  </xsl:template>

  <xsl:template name="getContentParURL">
    <xsl:param name="parId"/>
    <xsl:choose>
      <xsl:when test="/Page/Menu/descendant-or-self::MenuItem[@id=$parId]/@url='/'"></xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="/Page/Menu/descendant-or-self::MenuItem[@id=$parId]/@url"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- unique item id -->
  <xsl:template match="Content" mode="get-google-id">
    <xsl:value-of select="$uniqueIdPrefix"/>
    <xsl:value-of select="@id"/>
  </xsl:template>
  
  <!-- product description -->
  <xsl:template match="Content" mode="get-google-description">
    <xsl:choose>
      <xsl:when test="Body/node()">
        <xsl:apply-templates select="Body/node()" mode="flattenXhtml"/>
      </xsl:when>
      <xsl:when test="Description/node()">
        <xsl:apply-templates select="Description/node()" mode="flattenXhtml"/>
      </xsl:when>
      <xsl:when test="Strapline/node()">
        <xsl:apply-templates select="Strapline/node()" mode="flattenXhtml"/>
      </xsl:when>
      <xsl:when test="Strap/node()">
        <xsl:apply-templates select="Strap/node()" mode="flattenXhtml"/>
      </xsl:when>
      <xsl:when test="ShortDescription/node()">
        <xsl:apply-templates select="ShortDescription/node()" mode="flattenXhtml"/>
      </xsl:when>
      <xsl:when test="Intro/node()">
        <xsl:apply-templates select="Intro/node()" mode="flattenXhtml"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="." mode="getDisplayName"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <!-- product type -->
  <xsl:template match="Content" mode="get-product_type">
    <xsl:variable name="parId" select="@parId" />
    <xsl:apply-templates select="/Page/Menu/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem/@id=$parId]" mode="getDisplayName" />
  </xsl:template>

  <!-- product type -->
  <xsl:template match="Content" mode="get-google_product_type">
    <!-- Googles product taxonomy http://www.google.com/support/merchants/bin/answer.py?answer=160081 -->
    <xsl:if test="Name/@category and Name/@category!=''">
      <g:google_product_category></g:google_product_category>
    </xsl:if>
  </xsl:template>

  <!-- product url -->
  <xsl:template match="Content" mode="get-google-url">
    <xsl:variable name="pageURL">
      <xsl:apply-templates select="." mode="getHref" />
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="starts-with($pageURL,'http')">
        <xsl:value-of select="$pageURL"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$siteURL"/>
        <xsl:value-of select="$pageURL"/>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text>?utm_source=google-product-search&amp;utm_medium=productFeed</xsl:text>
  </xsl:template>

  <!-- product image link  -->
  <xsl:template match="Content" mode="get-google-image_link">  
    <xsl:if test="Images/img[@src and @src!='']">
      <g:image_link>
        <xsl:value-of select="$siteURL"/>
        <xsl:choose>
          <xsl:when test="Images/img[@class='detail']/@src and Images/img[@class='detail']/@src!=''">
            <xsl:value-of select="Images/img[@class='detail']/@src"/>
          </xsl:when>
          <xsl:when test="Images/img[@class='display']/@src and Images/img[@class='display']/@src!=''">
            <xsl:value-of select="Images/img[@class='display']/@src"/>
          </xsl:when>
          <xsl:when test="Images/img[@class='thumbnail']/@src and Images/img[@class='thumbnail']/@src!=''">
            <xsl:value-of select="Images/img[@class='thumbnail']/@src"/>
          </xsl:when>
        </xsl:choose>
      </g:image_link>
    </xsl:if>
  </xsl:template>

  <!-- product image link  -->
  <xsl:template match="Content" mode="get-google-additional_image_link">
    <xsl:if test="Content[@type='LibraryImage']">
        <!-- only allowed 10 -->
        <xsl:for-each select="Content[@type='LibraryImage' and position() &lt;= 10]">
          <g:additional_image_link>
            <xsl:value-of select="$siteURL"/>
            <xsl:choose>
              <xsl:when test="Images/img[@class='detail']/@src and Images/img[@class='detail']/@src!=''">
                <xsl:value-of select="Images/img[@class='detail']/@src"/>
              </xsl:when>
              <xsl:when test="Images/img[@class='display']/@src and Images/img[@class='display']/@src!=''">
                <xsl:value-of select="Images/img[@class='display']/@src"/>
              </xsl:when>
              <xsl:when test="Images/img[@class='thumbnail']/@src and Images/img[@class='thumbnail']/@src!=''">
                <xsl:value-of select="Images/img[@class='thumbnail']/@src"/>
              </xsl:when>
            </xsl:choose>
          </g:additional_image_link>
        </xsl:for-each>
      </xsl:if>
  </xsl:template>

  <!-- condition -->
  <xsl:template match="Content" mode="get-google-condition">
    <xsl:text>new</xsl:text>
  </xsl:template>

  <!-- availability -->
  <xsl:template match="Content" mode="get-google-availability">
    <xsl:choose>
      <xsl:when test="Stock/node() and Stock/node()=0">
        <xsl:text>out of stock</xsl:text>
      </xsl:when>
      <xsl:otherwise>in stock</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- price -->
  <xsl:template match="Content" mode="get-google-price">
    <xsl:choose>
      <xsl:when test="$nVatRate!='0'"><xsl:value-of select="format-number(Prices/Price[@type='sale' and @validGroup='all']/node() * (1 + $nVatRate),'##.00')"/></xsl:when>
      <xsl:otherwise><xsl:value-of select="format-number(Prices/Price[@type='sale' and @validGroup='all']/node(),'##.00')"/></xsl:otherwise>
    </xsl:choose>
    
    <xsl:text> </xsl:text>
    <xsl:value-of select="Prices/Price[@type='sale' and @validGroup='all']/@currency"/>
  </xsl:template>

  <!-- sale price -->
  <xsl:template match="Content" mode="get-google-sale_price">
    <xsl:value-of select="format-number(@curDiscountPrice,'##.00')"/>
    <xsl:text> </xsl:text>
    <xsl:value-of select="Prices/Price[@type='sale' and @validGroup='all']/@currency"/>
  </xsl:template>

  <!-- brand -->
  <xsl:template match="Content" mode="get-google-brand">
    <xsl:value-of select="Manufacturer/node()"/>
  </xsl:template>

  <!-- Global Trade Item Number (EAN, ISBN, JAN or UPC) -->
  <xsl:template match="Content" mode="get-google-gtin">
     <xsl:value-of select="StockCode/@gtin"/>
  </xsl:template>

  <!-- Manufacturer Part Number -->
  <xsl:template match="Content" mode="get-google-mpn">
    <xsl:choose>
      <xsl:when test="StockCode/@mpn and StockCode/@mpn!=''">
        <xsl:value-of select="StockCode/@mpn"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="StockCode/node()"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <!-- Get shipping costs -->
  <xsl:template match="Content" mode="getShippingCharges">

    <!-- only target countries are; U.S., U.K., Australia, Germany, France, Japan, China, Italy, the Netherlands, and Spain. -->
    <!--<xsl:for-each select="ShippingOptions/Method[LocationISOa2='US' or LocationISOa2='GB' or LocationISOa2='AU' or LocationISOa2='DE' or LocationISOa2='FR' or LocationISOa2='JP' or LocationISOa2='CN' or LocationISOa2='IT' or LocationISOa2='NL' or LocationISOa2='ES']">-->
    <xsl:for-each select="ShippingOptions/Method[LocationISOa2='GB']">
      <xsl:sort select="LocationISOa2" order="ascending" data-type="text"/>
      <g:shipping>
        <g:country>
          <xsl:value-of select="LocationISOa2/node()"/>
        </g:country>
        <g:service>
          <xsl:value-of select="Carrier/node()"/>
          <xsl:if test="ShippingTime/node()">
            <xsl:text> (</xsl:text>
            <xsl:value-of select="ShippingTime/node()"/>
            <xsl:text>)</xsl:text>
          </xsl:if>
        </g:service>
        <g:price>
          <xsl:value-of select="format-number(Cost/node(),'0.00')"/>
        </g:price>
      </g:shipping>      
    </xsl:for-each>

  </xsl:template>
  
</xsl:stylesheet>
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">


    <xsl:variable name="subCmd" select="/Page/Request/QueryString/Item[@name='subCmd']/node()"/>
    
  <!-- ############################################################################################   -->
  <!-- ==  Subscriptions Templates  ===============================================================   -->
  <!-- ############################################################################################   -->
  
  <!-- Subscriptions Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='SubscriptionList']" mode="displayBrief">
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

    <div class="SubscriptionList">
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
    </div>

  </xsl:template>

  <!-- Sub Brief -->
  <xsl:template match="Content[@type='Subscription']" mode="displayBrief">
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
    <!-- hproduct for microformats -->
    <div class="listItem list-group-item hproduct subscription">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem list-group-item hproduct subscription'"/>
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
        <xsl:if test="StockCode/node()!=''">
          <p class="sku stockCode">
            <span class="label">
              <xsl:call-template name="term2014" />
              <xsl:text>: </xsl:text>
            </span>
            <xsl:value-of select="StockCode/node()"/>
          </p>
        </xsl:if>
        <xsl:apply-templates select="." mode="displayPrice" />
        <p class="duration">
          <span class="label">
            <!-- Duration -->
            <xsl:call-template name="term4026" />
            <xsl:text>: </xsl:text>
          </span>
            <xsl:value-of select="Duration/Length/node()"/>
            <xsl:text> </xsl:text>
            <xsl:value-of select="translate(Duration/Unit/node(),'DWMY','dwmy')"/>
            <xsl:if test="number(Duration/Length/node()) &gt; 1">
              <xsl:text>s</xsl:text>
            </xsl:if>
          <span class="suffix">
            <xsl:text> (</xsl:text>
            <xsl:value-of select="translate(Type/node(),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
            <xsl:text>)</xsl:text>
          </span>
        </p>
        <xsl:if test="ShortDescription/node()!=''">
          <div class="description">
            <xsl:apply-templates select="ShortDescription/node()" mode="cleanXhtml"/>
          </div>
        </xsl:if>
        <div class="entryFooter">
          <xsl:apply-templates select="." mode="moreLink">
            <xsl:with-param name="link" select="$parentURL"/>
            <xsl:with-param name="altText">
              <xsl:value-of select="Name/node()"/>
            </xsl:with-param>
          </xsl:apply-templates>
        </div>
      </div>
    </div>
  </xsl:template>

  <!-- Sub Detail -->
  <xsl:template match="Content[@type='Subscription']" mode="ContentDetail">
    <xsl:variable name="thisURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <xsl:variable name="parId" select="@parId" />
    <div class="hproduct subscription detail">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail subscription'"/>
      </xsl:apply-templates>

      <xsl:if test="Images/img/@src!=''">
        <xsl:apply-templates select="." mode="displayDetailImage"/>
      </xsl:if>

      <h2 class="fn">
        <xsl:value-of select="Name/node()"/>
      </h2>
      <xsl:if test="StockCode/node()!=''">
        <p class="stockCode">
          <span class="label">
            <xsl:call-template name="term2014" />
            <xsl:text>: </xsl:text>
          </span>
         
          <xsl:value-of select="StockCode/node()"/>
        </p>
      </xsl:if>
      
      <xsl:apply-templates select="." mode="displayPrice" />

      <p class="duration">
        <span class="label">
          <!-- Duration -->
          <xsl:call-template name="term4026" />
          <xsl:text>: </xsl:text>
        </span>
        <xsl:value-of select="Duration/Length/node()"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="translate(Duration/Unit/node(),'DWMY','dwmy')"/>
        <xsl:if test="number(Duration/Length/node()) &gt; 1">
          <xsl:text>s</xsl:text>
        </xsl:if>
        <span class="suffix">
          <xsl:text> (</xsl:text>
          <xsl:value-of select="translate(Type/node(),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
          <xsl:text>)</xsl:text>
        </span>
      </p>

      <xsl:if test="/Page/Cart">
        <xsl:apply-templates select="." mode="addToCartButton"/>
      </xsl:if>

      <xsl:apply-templates select="." mode="SpecLink"/>

      <xsl:if test="Body/node()!=''">
        <div class="description">
          <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>
      <div class="entryFooter">
        <xsl:if test="Content[@type='Tag']">
          <div class="tags">
            <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
          </div>
        </xsl:if>

        <xsl:if test="$page/Request/ServerVariables/Item[@name='PREVIOUS_PAGE']/node()">
          <xsl:apply-templates select="." mode="backLink">
            <xsl:with-param name="link">
              <xsl:choose>
                <xsl:when test="contains($thisURL,$page/Request/ServerVariables/Item[@name='PREVIOUS_PAGE']/node())">
                  <xsl:apply-templates select="$page/Menu/descendant-or-self::MenuItem[@parId=$parId]" mode="getHref" />
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$page/Request/ServerVariables/Item[@name='PREVIOUS_PAGE']/node()"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:with-param>
            <xsl:with-param name="altText">
              <xsl:call-template name="term2015" />
            </xsl:with-param>
          </xsl:apply-templates>
        </xsl:if>

      </div>


      <!--<xsl:if test="Content[@type='LibraryImage']">
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
        </div>
      </xsl:if>-->

      <!--RELATED CONTENT-->
      <xsl:if test="Content">
        <!-- Reviews  -->
        <xsl:if test="Content[@type='Review']">
          <div class="relatedcontent">
            <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
              <xsl:with-param name="type">PlainText</xsl:with-param>
              <xsl:with-param name="text">Add Title for Related Reviews</xsl:with-param>
              <xsl:with-param name="name">relatedReviewsTitle</xsl:with-param>
            </xsl:apply-templates>
            <h4>
              <xsl:choose>
                <xsl:when test="/Page/Contents/Content[@name='relatedReviewsTitle']">
                  <xsl:apply-templates select="/Page/Contents/Content[@name='relatedReviewsTitle']" mode="displayBrief"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="term2016" />
                </xsl:otherwise>
              </xsl:choose>
            </h4>
            <xsl:apply-templates select="/" mode="List_Related_Reviews">
              <xsl:with-param name="parProductID" select="@id"/>
            </xsl:apply-templates>
          </div>
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
            <xsl:apply-templates select="/" mode="List_Related_Products">
              <xsl:with-param name="parProductID" select="@id"/>
            </xsl:apply-templates>
          </div>
        </xsl:if>
      </xsl:if>
    </div>
  </xsl:template>

  <!-- Subscription with no Price Detail -->
  <xsl:template match="Content[@type='Subscription' and Prices/Price[@type='sale' and @currency=/Page/Cart/@currency]/node()=0]" mode="ContentDetail">
    <xsl:variable name="thisURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <xsl:variable name="parId" select="@parId" />
    <div class="hproduct subscription detail row ">
    <div class="col-md-8">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail subscription'"/>
      </xsl:apply-templates>

      <xsl:if test="Images/img/@src!=''">
        <xsl:apply-templates select="." mode="displayDetailImage"/>
      </xsl:if>

      <h2 class="fn">
        <xsl:value-of select="Name/node()"/>
      </h2>
      <xsl:if test="StockCode/node()!=''">
        <p class="stockCode">
          <span class="label">
            <xsl:call-template name="term2014" />
            <xsl:text>: </xsl:text>
          </span>
         
          <xsl:value-of select="StockCode/node()"/>
        </p>
      </xsl:if>
      
      <xsl:apply-templates select="." mode="displayPrice" />

      <p class="duration">
        <span class="label">
          <!-- Duration -->
          <xsl:call-template name="term4026" />
          <xsl:text>: </xsl:text>
        </span>
        <xsl:value-of select="Duration/Length/node()"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="translate(Duration/Unit/node(),'DWMY','dwmy')"/>
        <xsl:if test="number(Duration/Length/node()) &gt; 1">
          <xsl:text>s</xsl:text>
        </xsl:if>
        <span class="suffix">
          <xsl:text> (</xsl:text>
          <xsl:value-of select="translate(Type/node(),'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
          <xsl:text>)</xsl:text>
        </span>
      </p>

      <xsl:if test="/Page/Cart">
        <xsl:apply-templates select="." mode="addToCartButton"/>
      </xsl:if>

      <xsl:apply-templates select="." mode="SpecLink"/>

      <xsl:if test="Body/node()!=''">
        <div class="description">
          <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>

      <div class="entryFooter">
        <xsl:if test="Content[@type='Tag']">
          <div class="tags">
            <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
          </div>
        </xsl:if>

        <xsl:if test="$page/Request/ServerVariables/Item[@name='PREVIOUS_PAGE']/node()">
          <xsl:apply-templates select="." mode="backLink">
            <xsl:with-param name="link">
              <xsl:choose>
                <xsl:when test="contains($thisURL,$page/Request/ServerVariables/Item[@name='PREVIOUS_PAGE']/node())">
                  <xsl:apply-templates select="$page/Menu/descendant-or-self::MenuItem[@parId=$parId]" mode="getHref" />
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$page/Request/ServerVariables/Item[@name='PREVIOUS_PAGE']/node()"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:with-param>
            <xsl:with-param name="altText">
              <xsl:call-template name="term2015" />
            </xsl:with-param>
          </xsl:apply-templates>
        </xsl:if>

      </div>

      <div class="terminus">&#160;</div>

      <!--<xsl:if test="Content[@type='LibraryImage']">
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
        </div>
      </xsl:if>-->

      <!--RELATED CONTENT-->
      <xsl:if test="Content">
        <!-- Reviews  -->
        <xsl:if test="Content[@type='Review']">
          <div class="relatedcontent">
            <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
              <xsl:with-param name="type">PlainText</xsl:with-param>
              <xsl:with-param name="text">Add Title for Related Reviews</xsl:with-param>
              <xsl:with-param name="name">relatedReviewsTitle</xsl:with-param>
            </xsl:apply-templates>
            <h4>
              <xsl:choose>
                <xsl:when test="/Page/Contents/Content[@name='relatedReviewsTitle']">
                  <xsl:apply-templates select="/Page/Contents/Content[@name='relatedReviewsTitle']" mode="displayBrief"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="term2016" />
                </xsl:otherwise>
              </xsl:choose>
            </h4>
            <xsl:apply-templates select="/" mode="List_Related_Reviews">
              <xsl:with-param name="parProductID" select="@id"/>
            </xsl:apply-templates>
          </div>
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
            <xsl:apply-templates select="/" mode="List_Related_Products">
              <xsl:with-param name="parProductID" select="@id"/>
            </xsl:apply-templates>
          </div>
        </xsl:if>
      </xsl:if>
    </div>
      <div class="col-md-4">
        <xsl:choose>
          <xsl:when test="/Page/User">
            <a href="?subCmd=Subscribe" class="btn btn-action">
            Sign Up<xsl:text> </xsl:text><i class="fa fa-pencil">
              <xsl:text> </xsl:text>
            </i>
            </a>
          </xsl:when>
          <xsl:otherwise>
            <div class="panel panel-default">
              <div class="panel-heading">
                <h4 class="panel-title">Login or register here to sign up for Trial</h4>
              </div>
              <div class="panel-body">
                <xsl:apply-templates select="Content[@type='xform']" mode="xform"/>
              </div>
            </div>
          </xsl:otherwise>
        </xsl:choose>
        
      </div>
    </div>
  </xsl:template>

  <!-- -->
  <xsl:template match="Content[@type='Subscription']" mode="addToCartButton">
    <xsl:variable name="parentURL">
      <xsl:call-template name="getContentParURL"/>
    </xsl:variable>
    <xsl:variable name="siteURL">
      <xsl:call-template name="getSiteURL"/>
    </xsl:variable>
    <xsl:variable name="secureURL">
      <xsl:call-template name="getSecureURL"/>
    </xsl:variable>
    <xsl:variable name="price">
      <xsl:value-of select="Prices/Price[@type='sale' and @currency=$page/Cart/@currency]/node()"/>
    </xsl:variable>
    <div id="cartButtons{@id}" class="cartButtons">
      <xsl:choose>
        <xsl:when test="$price &gt; 0">
          <form action="" method="post" class="ewXform">
            <xsl:apply-templates select="." mode="Options_List"/>
            <xsl:if test="not(format-number($price, '#.00')='NaN')">
              <!-- Hard code 1 qty -->
              <!--input class="qtybox" type="text" name="price_{@id}" id="price_{@id}" value="1"/-->
              <input type="hidden" name="qty_{@id}" id="qty_{@id}" value="1"/>
              <xsl:apply-templates select="/" mode="addtoCartButtons"/>
            </xsl:if>
          </form>
        </xsl:when>
        <xsl:otherwise>
          <!--a href="?ewCmd=subscriptionTrail" class="btn btn-action">
            Sign Up<xsl:text> </xsl:text><i class="fa fa-pencil">
              <xsl:text> </xsl:text>
            </i>
          </a-->
        </xsl:otherwise>
      </xsl:choose>

    </div>

  </xsl:template>

  <!-- Subscriptions Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='ManageUserSubscriptions']" mode="displayBrief">
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:choose>
      <xsl:when test="$subCmd='updateSubPayment'">
        <a href="{$currentPage/@url}">Back to your subscriptions</a>
        <xsl:apply-templates select="Content[@type='xform']" mode="xform"/>
      </xsl:when>
      <xsl:otherwise>
        <div class="SubscriptionList">
            <table class="table table-striped">
              <tbody>
                <xsl:apply-templates select="Subscription" mode="displayBrief">
                  <xsl:with-param name="sortBy" select="@sortBy"/>
                </xsl:apply-templates>
              </tbody>
            </table>
          </div>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <xsl:template match="Subscription" mode="displayBrief">
    <tr>
      <td>
        <xsl:apply-templates select="." mode="status_legend"/>
      </td>
      <td>
        <xsl:value-of select="@name"/>
        <br/>
        <xsl:value-of select="@period"/> -
        <xsl:value-of select="@periodUnit"/>
      </td>
      <td>
        <xsl:apply-templates select="/Page" mode="formatPrice">
              <xsl:with-param name="price" select="@value"/>
              <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
        </xsl:apply-templates>

      </td>
      <td>
        
      </td>
      <td>
        <td>
          <xsl:if test="@paymentStatus='active' or @paymentStatus='Manual' ">
            Start Date:   <xsl:call-template name="DD_Mon_YYYY">
                            <xsl:with-param name="date" select="@startDate"/>
                          </xsl:call-template>

            <xsl:if test="@paymentStatus='active'">
              <br/> Will automatically Renew with
            </xsl:if>
            <xsl:value-of select="@providerName"/>
            <a href="?subCmd=updateSubPayment&amp;subId={@id}" class="btn btn-xs btn-primary">
            <i class="fa fa-money">&#160;</i>&#160;Update Payment Method  
          </a>
          <a href="?subCmd=cancelSub&amp;subId={@id}" class="btn btn-xs btn-warning">
            <i class="fa fa-times">&#160;</i>&#160;Cancel
          </a>
          </xsl:if>
        </td>
      </td>
    </tr>

  </xsl:template>

</xsl:stylesheet>
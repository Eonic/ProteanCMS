<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

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
    <div class="clearfix OrganisationList">
      <div data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1" >
        <xsl:apply-templates select="." mode="contentColumns"/>
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
        <a href="{$parentURL}" class="btn btn-default btn-sm directions">
          <i class="fa fa-map-marker">
            <xsl:text> </xsl:text>
          </i>
          <xsl:text> </xsl:text>
          Get Directions
        </a>
        <div class="clear-fix">
          <xsl:text> </xsl:text>
        </div>
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
    <div itemscope="" itemtype="{Organization/@itemtype}" class="listItem">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem'"/>
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
        <div class="col-lg-8">
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
        <div class="col-lg-4">
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
  
</xsl:stylesheet>
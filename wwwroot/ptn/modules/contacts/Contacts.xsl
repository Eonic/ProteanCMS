<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
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
          <xsl:with-param name="crop" select="$cropSetting"/>
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
    <xsl:param name="crop"/>
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
              <xsl:apply-templates select="." mode="displayThumbnail">
                <xsl:with-param name="crop" select="$cropSetting" />
              </xsl:apply-templates>
            </xsl:when>
            <xsl:otherwise>
              <a href="{$parentURL}" title="click here to view more details on {GivenName/node()} {Surname/node()}">

                <xsl:apply-templates select="." mode="displayThumbnail">
                  <xsl:with-param name="crop" select="$cropSetting" />
                </xsl:apply-templates>
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
  <xsl:template match="Content[@type='Contact']" mode="displayContributor">
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
    <div class="contributor row">
      <xsl:if test="Images/img/@src!=''">
        <a href="{$parentURL}" rel="author" title="click here to view more details on {GivenName/node()} {Surname/node()}" class="col-md-4">
          <xsl:apply-templates select="." mode="displayThumbnail">
            <xsl:with-param name="width">150</xsl:with-param>
            <xsl:with-param name="height">150</xsl:with-param>
            <xsl:with-param name="crop" select="true()"/>
          </xsl:apply-templates>
        </a>
      </xsl:if>
      <div class="col-md-8">
        <h5 class="title">
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
        </h5>
        <xsl:if test="Title/node()!=''">
          <h6 class="title">
            <xsl:apply-templates select="Title" mode="displayBrief"/>
            <xsl:if test="Company/node()!=''">
              <xsl:text> - </xsl:text>
              <xsl:apply-templates select="Company" mode="displayBrief"/>
            </xsl:if>
          </h6>
        </xsl:if>
        <xsl:if test="Profile/node()!=''">
          <p>
            <xsl:apply-templates select="Profile/node()" mode="cleanXhtml"/>
          </p>
        </xsl:if>
        <a href="{$parentURL}" class="btn btn-sm btn-default">
          more about
          <xsl:value-of select="GivenName/node()"/>
        </a>
      </div>
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
</xsl:stylesheet>
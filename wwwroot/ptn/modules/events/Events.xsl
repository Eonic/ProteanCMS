<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

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


  <!-- month heading-->
  <xsl:template name="MonthHeading">
    <xsl:param name="date"/>
    <xsl:variable name="month" select="number(substring($date, 6, 2))"/>
    <xsl:choose>
      <xsl:when test="$month=1">January</xsl:when>
      <xsl:when test="$month=2">February</xsl:when>
      <xsl:when test="$month=3">March</xsl:when>
      <xsl:when test="$month=4">April</xsl:when>
      <xsl:when test="$month=5">May</xsl:when>
      <xsl:when test="$month=6">June</xsl:when>
      <xsl:when test="$month=7">July</xsl:when>
      <xsl:when test="$month=8">August</xsl:when>
      <xsl:when test="$month=9">September</xsl:when>
      <xsl:when test="$month=10">October</xsl:when>
      <xsl:when test="$month=11">November</xsl:when>
      <xsl:when test="$month=12">December</xsl:when>
      <xsl:otherwise>INVALID MONTH</xsl:otherwise>
    </xsl:choose>
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
        <xsl:if test="$page/Contents/Content[@moduleType='EventList' and @groupEventsByMonth='true']">
          <xsl:variable name="thisDate">
            <xsl:value-of select="StartDate/node()"/>
          </xsl:variable>
          <xsl:variable name="lastDate">
            <xsl:value-of select="./preceding-sibling::*[ 1]/StartDate/node()"/>
          </xsl:variable>
          <xsl:variable name="thisMonth">
            <xsl:value-of select="number(substring($thisDate, 6, 2))"/>
          </xsl:variable>
          <xsl:variable name="lastMonth">
            <xsl:value-of select="number(substring($lastDate, 6, 2))"/>
          </xsl:variable>
          <xsl:if test="$thisMonth != $lastMonth">
            <xsl:attribute name="class">
              <xsl:text>lIinner media  month-heading-wrapper </xsl:text>
            </xsl:attribute>
            <h2 class="month-heading">
              <xsl:call-template name="MonthHeading">
                <xsl:with-param name="date" select="StartDate/node()"/>
              </xsl:call-template>
            </h2>
          </xsl:if>
        </xsl:if>
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
                <xsl:call-template name="formatdate">
                  <xsl:with-param name="date" select="StartDate/node()" />
                  <xsl:with-param name="format" select="'ddd, dd MMM yyyy'" />
                </xsl:call-template>
                <span class="value-title" title="{StartDate/node()}T{translate(Times/@start,',',':')}" ></span>
              </span>
              <xsl:if test="EndDate/node()!='' and EndDate/node() != StartDate/node()">
                <xsl:text> - </xsl:text>
                <span class="dtend">
                  <xsl:call-template name="formatdate">
                    <xsl:with-param name="date" select="EndDate/node()" />
                    <xsl:with-param name="format" select="'ddd, dd MMM yyyy'" />
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
          <xsl:if test="@teaserSlug!=''">
            <div class="label label-default">
              <xsl:value-of select="@teaserSlug"/>
            </div>
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
            <xsl:if test="@bookingURL!=''">
              <xsl:text> </xsl:text>
              <a href="{@bookingURL}" class="btn btn-success">
                Book Here&#160;&#160;<i class="fa fa-mouse-pointer">&#160;</i>
              </a>
            </xsl:if>
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
    <div class="detail vevent">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail vevent'"/>
      </xsl:apply-templates>
      <h2 class="content-title">
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
              <div class="col-md-5 pull-right">
                <xsl:apply-templates select="." mode="displayDetailImage"/>
                <xsl:if test="@bookingURL!=''">
                  <xsl:text> </xsl:text>
                  <a href="{@bookingURL}" class="btn btn-success btn-block">
                    Book Here&#160;&#160;<i class="fa fa-mouse-pointer">&#160;</i>
                  </a>
                </xsl:if>
                <xsl:apply-templates select="Content[@type='Contact']" mode="displayContributor"/>
              </div>
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
          <xsl:if test="@teaserSlug!=''">
            <div class="label label-default label-lg">
              <xsl:value-of select="@teaserSlug"/>
            </div>
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




  <xsl:template match="Content[@type='Event' and ancestor::ContentDetail]" mode="JSONLD">
    [ { "@context": "https://schema.org",
    "@type": "Event",
    "name": "<xsl:apply-templates select="." mode="getDisplayName" />",
    "EventStatus": "<xsl:value-of select="@eventStatus"/>",
    "eventAttendanceMode": "OfflineEventAttendanceMode",
    "description": "<xsl:call-template name="escape-json">
      <xsl:with-param name="string">
        <xsl:apply-templates select="Strap/*" mode="flattenXhtml"/>
        <xsl:apply-templates select="Body/*" mode="flattenXhtml"/>
      </xsl:with-param>
    </xsl:call-template>",
    "startDate": "<xsl:value-of select="StartDate/node()"/>",
    "endDate": "<xsl:value-of select="EndDate/node()"/>",
    "image": "<xsl:value-of select="Images/img[@class='detail']/@src"/>",
    <xsl:apply-templates select="Content[@type='Organisation' and @rtype='venue']" mode="JSONLD"/>
    "performer":[
    <xsl:apply-templates select="Content[@type='Performer']" mode="JSONLD"/>
    ],
    "offers": [
    <xsl:apply-templates select="Content[@type='Ticket']" mode="JSONLD"/>
    ],
    "url": "<xsl:value-of select="$href"/>"
    <xsl:apply-templates select="." mode="organiser"/>
    } } ]
  </xsl:template>
  <xsl:template match="Content[@type='Event' and ancestor::ContentDetail]" mode="organiser">
    <!-- Copy this to set the value site wide for the organiastion events.
      ,
      "organizer": {
      "@type": "Organization",
      "name": "Kira and Morrison Music",
      "url": "https://kiraandmorrisonmusic.com"
      }
      -->
  </xsl:template>
  <xsl:template match="Content[@type='Organisation' and @rtype='venue']" mode="JSONLD">
    "location": {
    "@type": "Place",
    "name": "<xsl:value-of select="name/node()"/>",
    "sameAs": "http://www.example.com",
    "address": {
    "@type": "PostalAddress",
    "streetAddress": "<xsl:value-of select="Organization/location/PostalAddress/streetAddress/node()"/>",
    "addressLocality": "<xsl:value-of select="Organization/location/PostalAddress/addressLocality/node()"/>",
    "addressRegion": "<xsl:value-of select="Organization/location/PostalAddress/addressRegion/node()"/>",
    "postalCode": "<xsl:value-of select="Organization/location/PostalAddress/postalCode/node()"/>",
    "addressCountry": "<xsl:value-of select="Organization/location/PostalAddress/addressCountry/node()"/>"
    },
  </xsl:template>

  <xsl:template match="Content[@type='Performer']" mode="JSONLD">
    {"@type": "MusicGroup",
    "name": "<xsl:value-of select="Surname/node()"/>",
    "sameAs": [
    "<xsl:apply-templates select="self::Content" mode="getHref">
      <xsl:with-param name="parId" select="@parId"/>
    </xsl:apply-templates>"
    <xsl:if test="@websiteURL!=''">
      ,"<xsl:value-of select="@websiteURL"/>"
    </xsl:if>
    <xsl:if test="@facebookURL!=''">
      ,"<xsl:value-of select="@facebookURL"/>"
    </xsl:if>
    <xsl:if test="@twitterURL!=''">
      ,"<xsl:value-of select="@twitterURL"/>"
    </xsl:if>
    <xsl:if test="@youtubeURL!=''">
      ,"<xsl:value-of select="@youtubeURL"/>"
    </xsl:if>
    <xsl:if test="@spotifyURL!=''">
      ,"<xsl:value-of select="@spotifyURL"/>"
    </xsl:if>
    <xsl:if test="@pinterestURL!=''">
      ,"<xsl:value-of select="@pinterestURL"/>"
    </xsl:if>    ]
    }
    <xsl:if test="position()!=last()">,</xsl:if>
  </xsl:template>

  <xsl:template match="Content[@type='Ticket']" mode="JSONLD">
    {
    "@type": "Offer",
    "description":"<xsl:value-of select="@name"/>",
    "url": "<xsl:value-of select="$href"/>",
    "price": "<xsl:apply-templates select="." mode="displayPrice" />",
    "priceCurrency": "GBP",
    "availability": "https://schema.org/InStock",
    "validFrom": "<xsl:value-of select="@publishDate"/>"
    }
    <xsl:if test="position()!=last()">,</xsl:if>
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


</xsl:stylesheet>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

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
  
</xsl:stylesheet>
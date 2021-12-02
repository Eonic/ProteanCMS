<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

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
  
</xsl:stylesheet>
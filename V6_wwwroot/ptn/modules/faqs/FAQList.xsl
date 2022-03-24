<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

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
  
</xsl:stylesheet>
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <xsl:variable name="today" select="/Page/Request/ServerVariables/Item[@name='Date']/node()"/>

  <!-- ## Search Layouts are specified in the LayoutsManifest.XML file  ################################   -->
  <xsl:template match="Content[@type='Module' and @moduleType='Search']" mode="displayBrief">
    <xsl:variable name="searchFormId" select="@id" />
    <xsl:variable name="searchMode" select="@searchMode" />
    <xsl:variable name="searchString" select="/Content[@type='SearchHeader']/@searchString"/>

    <!-- Collect and Filter results -->
    <xsl:variable name="searchResults">
      <xsl:apply-templates select="." mode="getSearchResults"/>
    </xsl:variable>

    <div class="searchListing">
        <!-- Display Form -->
      <xsl:apply-templates select="." mode="searchForm">
        <xsl:with-param name="searchFormId" select="$searchFormId" />
        <xsl:with-param name="searchString" select="$searchString" />
      </xsl:apply-templates>

      <xsl:choose>
        <!-- Display Results -->
        <xsl:when test="count(ms:node-set($searchResults)/*) &gt; 0">
          <xsl:apply-templates select="Content[@type='SearchHeader']" mode="searchSummary">
            <xsl:with-param name="searchFormId" select="$searchFormId" />
            <xsl:with-param name="searchString" select="$searchString" />
            <xsl:with-param name="searchResults" select="$searchResults"/>
          </xsl:apply-templates>

          <xsl:apply-templates select="." mode="searchResults">
            <xsl:with-param name="searchResults" select="$searchResults"/>
          </xsl:apply-templates>
        </xsl:when>
        
        <!-- Display No Results IF Search was of that type -->
        <xsl:when test="count(ms:node-set($searchResults)/*) &gt; 0 and @searchMode=/Page/Request/Form/Item[@name='searchMode']/node()">
          <xsl:apply-templates select="Content[@type='SearchHeader']" mode="searchSummary">
            <xsl:with-param name="searchFormId" select="$searchFormId" />
            <xsl:with-param name="searchString" select="$searchString" />
            <xsl:with-param name="searchResults" select="$searchResults"/>
          </xsl:apply-templates>
          <!--xsl:apply-templates select="." mode="searchResults">
            <xsl:with-param name="searchResults" select="$searchResults"/>
          </xsl:apply-templates-->
        </xsl:when>
      </xsl:choose>
    </div>
  </xsl:template>

  <!-- Overwrite this template for bespoke result filtering -->
  <xsl:template match="Content" mode="getSearchResults">
    <xsl:variable name="startPos" select="0" />
    
    <xsl:apply-templates select="." mode="getContent">
      <xsl:with-param name="contentType">
        <xsl:choose>
          <xsl:when test="@contentType!=''">
            <xsl:value-of select="@contentType"/>
          </xsl:when>
          <xsl:otherwise>SearchResult</xsl:otherwise>
        </xsl:choose>
      </xsl:with-param>
      <xsl:with-param name="startPos" select="$startPos" />
    </xsl:apply-templates>
  </xsl:template>

  <!-- Search Form -->
  <xsl:template match="Content" mode="searchForm">
    <xsl:param name="searchFormId" />
    <xsl:param name="searchString" />
    <xsl:variable name="link" select="@link"/>
    <xsl:variable name="searchRedirect">
      <xsl:if test="@link!=''">
        <xsl:apply-templates select="//MenuItem[@id=$link]" mode="getHref"/>
      </xsl:if>
    </xsl:variable><br/>
    <div class="row">
    <div class="col-md-12">
      <form method="post" action="{$searchRedirect}" id="searchInput">
        <input type="hidden" name="searchMode" value="{@searchMode}" />
        <input type="hidden" name="contentType" value="{@contentType}" />
        <input type="hidden" name="searchFormId" value="{$searchFormId}" />
        <div class="input-group">
        <input type="text" class="form-control" name="searchString" id="searchString2" value="{$searchString}" placeholder="Search"/>
        <xsl:if test="@searchMode='USER'">
          <select name="groupId" id="groupId">
            <xsl:for-each select="/Page/User/Group">
              <option value="{@id}">
                <xsl:value-of select="@name"/>
              </option>
            </xsl:for-each>
          </select>
        </xsl:if>
          <span class="input-group-btn">
            <button type="button" class="btn btn-primary" name="Search">
              <i class="fa fa-search">
                <xsl:text> </xsl:text>
              </i>
              <xsl:text> </xsl:text>
              <xsl:choose>
                <xsl:when test="@submitLabel='_'">

                </xsl:when>
                <xsl:when test="@submitLabel!=''">
                  <xsl:value-of select="@submitLabel"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>Submit</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
            </button>
          </span>
        </div>
      </form>
    </div>
    </div>
  </xsl:template>

  <!-- Search Summary -->
  <xsl:template match="Content" mode="searchSummary">
    <xsl:param name="searchFormId" />
    <xsl:param name="searchResults" />
    <xsl:param name="searchString" />
    <xsl:variable name="resultCount" select="count(ms:node-set($searchResults)/Content)" />
    <xsl:variable name="totalResults" select="@totalResults" />
    <xsl:variable name="pageCount" select="@pageCount" />
    <xsl:variable name="pageStart" select="@pageStart" />
    <xsl:variable name="LastLoadResultCount" select="@loadedResult" />
    <xsl:variable name="pageSize" select="@pageSize" />
    <xsl:variable name="resultsReturned" select="@resultsReturned" />
    
    <br/>
    <div class="panel panel-primary text-right search-results-area" id="ResultDiv">
      <div class="panel-body">
      <!--<xsl:text> Total </xsl:text>
      <xsl:value-of select="$resultTotalCount" />
       <xsl:text> records found </xsl:text>
     
      <button type="button" id="btnPrevResult" class="search-result text-right text-green" name="Search">
        Prev <i class=" fa fa-chevron-circle-left text-green"></i>
      </button>&#160;&#160;
      <xsl:if test="$resultTotalCount &gt; 12">
        <button type="button" id="btnNextResult" class="search-result text-right text-green" name="Search">
          Next <i class=" fa fa-chevron-circle-right text-green"></i>
        </button>
      </xsl:if>-->
       <xsl:if test="@pageStart &gt; 1">
      <a type="button" id="btnPrevResult" class="btn btn-default pull-left" href="?searchMode=INDEX&amp;searchString={@searchString}&amp;pageStart={@pageStart - @pageSize}">
        <i class=" fa fa-chevron-circle-left text-green">&#160;</i>&#160;Prev 
      </a>
         </xsl:if>
        <span class="btn">
      <xsl:value-of select="$pageStart" />
      -
      <xsl:choose>
        <xsl:when test="$totalResults &gt;= $resultsReturned">
          <!--<xsl:value-of select="$Hit" />-->
          <xsl:value-of select="$pageStart + $resultsReturned - 1" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$totalResults" />
        </xsl:otherwise>
      </xsl:choose>

      <xsl:text> out of </xsl:text>

      <xsl:value-of select="$totalResults" />
      <xsl:choose>
        <xsl:when test="$totalResults='1'">
          <xsl:text> result </xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text> results </xsl:text>
        </xsl:otherwise>
      </xsl:choose>
      </span>
      <xsl:if test="@pageEnd &lt; $totalResults">
        <a id="btnNextResult" class="btn btn-default" name="Search" href="?searchMode=INDEX&amp;searchString={@searchString}&amp;pageStart={@pageEnd + 1}">
          Next&#160;<i class=" fa fa-chevron-circle-right text-green">&#160;</i>
        </a>
      </xsl:if>
      </div>
    </div>
  </xsl:template>

  <!-- Content Search Results -->
  <xsl:template match="Content[@searchMode='REGEX']" mode="searchResults">
    <xsl:param name="searchResults"/>
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="startPos" select="0" />
    
    <div class="{@contentType}List">
      <div class="cols{@cols}">
        <xsl:apply-templates select="ms:node-set($searchResults)/*" mode="displayBrief">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
        <div class="terminus">&#160;</div>
      </div>
    </div>
    
  </xsl:template>

  <!-- Content Search Results -->
  <xsl:template match="Content[@searchMode='INDEX']" mode="searchResults">
    <xsl:param name="searchResults"/>
    <xsl:variable name="contentType" select="SearchResult" />
    <xsl:variable name="startPos" select="0" />
 <div class="list-group search-results">
        <xsl:apply-templates select="ms:node-set($searchResults)/*" mode="displayBrief">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
</div>
  </xsl:template>
  
  <!-- -->

  <xsl:template match="Content[@type='SearchResult']" mode="displayBrief">
    <xsl:if test="@url!=''">
      <div class="list-group-item searchResult">
        <a href="{translate(@url,'\','/')}" alt="Click here to go to {@pagetitle}">
          <xsl:choose>
            <xsl:when test="@contenttype!='Download'">
              <xsl:choose>
                <xsl:when test="@contenttype='Product'">
                  <i class="fa fa-star fa-lg pull-left">&#160;</i>&#160;
                </xsl:when>
                <xsl:when test="@contenttype='Event'">
                  <i class="fa fa-calendar fa-lg pull-left">&#160;</i>&#160;
                </xsl:when>
                <xsl:when test="@contenttype='NewsArticle' or @contenttype='RegularNewsArticle' or @contenttype='PressRelease'">
                  <i class="fa fa-newspaper-o fa-lg pull-left">&#160;</i>&#160;
                </xsl:when>
                <xsl:when test="@contenttype='Contact'">
                  <i class="fa fa-user fa-lg pull-left">&#160;</i>&#160;
                </xsl:when>
                <xsl:when test="@contenttype='Company'">
                  <i class="fa fa-building fa-lg pull-left">&#160;</i>&#160;
                </xsl:when>
                <xsl:otherwise>
                  <i class="fa fa-file-o fa-lg pull-left">&#160;</i>&#160;
                </xsl:otherwise>
              </xsl:choose>
            </xsl:when>
            <xsl:otherwise>
              <xsl:variable name="docType">
                <xsl:if test="contains(@url,'.doc')">
                  <xsl:text>doc</xsl:text>
                </xsl:if>
                <xsl:if test="contains(@url,'.pdf')">
                  <xsl:text>pdf</xsl:text>
                </xsl:if>
                <xsl:if test="contains(@url,'.ppt')">
                  <xsl:text>ppt</xsl:text>
                </xsl:if>
                <xsl:if test="contains(@url,'.xls')">
                  <xsl:text>xls</xsl:text>
                </xsl:if>
                <xsl:if test="contains(@url,'.zip')">
                  <xsl:text>zip</xsl:text>
                </xsl:if>
                <xsl:if test="contains(@url,'.jpg')">
                  <xsl:text>jpg</xsl:text>
                </xsl:if>
                <xsl:if test="contains(@url,'.gif')">
                  <xsl:text>gif</xsl:text>
                </xsl:if>
                <xsl:if test="contains(@url,'.png')">
                  <xsl:text>png</xsl:text>
                </xsl:if>
              </xsl:variable>
              <xsl:if test="$docType!=''">
                <i class="fa fa-lg fa-file-{$docType}-o pull-left">
                  <xsl:text> </xsl:text>
                </i>
                <xsl:text> </xsl:text>&#160;
              </xsl:if>
            </xsl:otherwise>
          </xsl:choose>
          <span>
            <xsl:value-of select="@name"/>
          </span>
        </a>
        <xsl:variable name="desc">
          <xsl:call-template name="truncateString">
            <xsl:with-param name="string" select="node()"/>
            <xsl:with-param name="length" select="'150'"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:if test="$desc!=''">
           <p>
              <xsl:value-of select="$desc"/>
            </p>
        </xsl:if>
       
        </div>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>
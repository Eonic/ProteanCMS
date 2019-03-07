<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">

  <!-- Book List Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='BookList']" mode="displayBrief">
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

    <!-- Output Module -->
    <div class="clearfix BookList">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix BookList content-scroller</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <div class="cols cols{@cols}" data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1" data-dots="{@carouselBullets}" data-height="{@carouselHeight}" >
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
        <!-- If Stepper, display Stepper -->
        <xsl:if test="@stepCount != '0' and not($page[@cssFramework='bs3'])">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="articleList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:choose>
          <xsl:when test="@linkArticle='true'">
            <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefLinked">
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
          </xsl:when>
          <xsl:when test="@Brief='Cover_Only'">
            <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefCover_Only">
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
          </xsl:when>
          <xsl:when test="@Brief='Cover_and_Title'">
            <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefCover_and_Title">
              <xsl:with-param name="class" select="'list-group-item'"/>
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
          </xsl:when>
          <xsl:when test="@Brief='Cover_Title_Author'">
            <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefCover_Title_Author">
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
          </xsl:when>
          <xsl:when test="@Brief='Cover_Title_Author_Info'">
            <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefCover_Title_Author_Info">
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
          </xsl:when>
          <xsl:when test="@Brief='Single_Book_Layout'">
            <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBriefSingle_Book_Layout">
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:if test="@stepCount != '0' and $page[@cssFramework='bs3']">
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

  <!-- Book Brief Cover_Only -->
  <xsl:template match="Content[@type='Book']" mode="displayBriefCover_Only">
    <xsl:param name="sortBy"/>
    <!-- articleBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="listItem list-group-item Book">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem list-group-item Book '"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner media">
        <xsl:if test="Images/img/@src!=''">
          <a href="{$parentURL}" title="Read More - {BookName/node()}">
            <xsl:apply-templates select="." mode="displayThumbnail"/>
          </a>
          <span class="hidden">|</span>
        </xsl:if>
        <div class="media-body">
          <div class="entryFooter">
            <xsl:apply-templates select="." mode="displayTags"/>
            <xsl:apply-templates select="." mode="moreLink">
              <xsl:with-param name="link" select="$parentURL"/>
              <xsl:with-param name="altText">
                <xsl:value-of select="BookName/node()"/>
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

  <!-- Book Brief Cover_and_Title -->
  <xsl:template match="Content[@type='Book']" mode="displayBriefCover_and_Title">
    <xsl:param name="sortBy"/>
    <xsl:param name="class"/>
    <!-- articleBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="listItem Book {$class}">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem list-group-item Book'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner media">
        <xsl:if test="Images/img/@src!=''">
          <a href="{$parentURL}" title="Read More - {BookName/node()}">
            <xsl:apply-templates select="." mode="displayThumbnail">
         
          <xsl:with-param name="no-stretch" select="false()" />
        </xsl:apply-templates>
          </a>
          <span class="hidden">|</span>
        </xsl:if>
        <div class="media-body">
          <!--<xsl:apply-templates select="." mode="displayDetailImage"/>-->
          <h2 class="entry-title content-title">
            <a href="{$parentURL}" title="Read More - {BookName/node()}">
              <xsl:apply-templates select="BookName/node()" mode="cleanXhtml"/>
            </a>
          </h2>
          <xsl:if test="BookStrapline/node()!=''">
            <h3 class="BookStrapline">
              <xsl:apply-templates select="BookStrapline/node()" mode="cleanXhtml"/>
            </h3>
          </xsl:if>
          <div class="entryFooter">
            <xsl:apply-templates select="." mode="displayTags"/>
            <xsl:apply-templates select="." mode="moreLink">
              <xsl:with-param name="link" select="$parentURL"/>
              <xsl:with-param name="altText">
                <xsl:value-of select="BookName/node()"/>
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

  <!-- Book Brief Cover_Title_Author -->
  <xsl:template match="Content[@type='Book']" mode="displayBriefCover_Title_Author">
    <xsl:param name="sortBy"/>
    <!-- articleBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="listItem list-group-item Book">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem list-group-item Book'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner media">
        <xsl:if test="Images/img/@src!=''">
          <a href="{$parentURL}" title="Read More - {BookName/node()}">
            <xsl:apply-templates select="." mode="displayThumbnail"/>
          </a>
          <span class="hidden">|</span>
        </xsl:if>
        <div class="media-body">
          <!--<xsl:apply-templates select="." mode="displayDetailImage"/>-->
          <h2 class="entry-title content-title">
            <a href="{$parentURL}" title="Read More - {BookName/node()}">
              <xsl:apply-templates select="BookName/node()" mode="cleanXhtml"/>
            </a>
          </h2>
          <xsl:if test="BookTitle/node()!=''">
            <h3 class="BookTitle">
              <xsl:apply-templates select="BookTitle/node()" mode="cleanXhtml"/>
            </h3>
          </xsl:if>
          <xsl:if test="BookStrapline/node()!=''">
            <h4 class="BookStrapline">
              <xsl:apply-templates select="BookStrapline/node()" mode="cleanXhtml"/>
            </h4>
          </xsl:if>
          <xsl:if test="Content[@type='Contact']!=''">
            <div class="bookAuthor">
              <!--<h5>About the Author</h5>-->
              <xsl:apply-templates select="Content[@type='Contact']" mode="displayAuthorBook"/>
            </div>
          </xsl:if>

          <div class="entryFooter">
            <xsl:apply-templates select="." mode="displayTags"/>
            <xsl:apply-templates select="." mode="moreLink">
              <xsl:with-param name="link" select="$parentURL"/>
              <xsl:with-param name="altText">
                <xsl:value-of select="BookName/node()"/>
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

  <!-- Book Brief Cover_Title_Author_Info -->
  <xsl:template match="Content[@type='Book']" mode="displayBriefCover_Title_Author_Info">
    <xsl:param name="sortBy"/>
    <!-- articleBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="listItem list-group-item Book">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem list-group-item Book'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner media">
        <xsl:if test="Images/img/@src!=''">
          <a href="{$parentURL}" title="Read More - {BookName/node()}">
            <xsl:apply-templates select="." mode="displayThumbnail"/>
          </a>
          <span class="hidden">|</span>
        </xsl:if>
        <div class="media-body">
          <!--<xsl:apply-templates select="." mode="displayDetailImage"/>-->
          <h2 class="entry-title content-title">
            <a href="{$parentURL}" title="Read More - {BookName/node()}">
              <xsl:apply-templates select="BookName/node()" mode="cleanXhtml"/>
            </a>
          </h2>
          <xsl:if test="BookTitle/node()!=''">
            <h3 class="BookTitle">
              <xsl:apply-templates select="BookTitle/node()" mode="cleanXhtml"/>
            </h3>
          </xsl:if>
          <xsl:if test="BookStrapline/node()!=''">
            <h4 class="BookStrapline">
              <xsl:apply-templates select="BookStrapline/node()" mode="cleanXhtml"/>
            </h4>
          </xsl:if>
          <xsl:if test="Content[@type='Contact']!=''">
            <div class="bookAuthor">
              <!--<h3>About the Author</h3>-->
              <xsl:apply-templates select="Content[@type='Contact']" mode="displayAuthorBook"/>
            </div>
          </xsl:if>

          <xsl:if test="ShortDescription/node()!=''">
            <div class="ShortDescription">
              <h5>
                <strong>Description</strong>
              </h5>
              <xsl:apply-templates select="ShortDescription/node()" mode="cleanXhtml"/>
            </div>
          </xsl:if>

          <div class="entryFooter">
            <xsl:apply-templates select="." mode="displayTags"/>
            <xsl:apply-templates select="." mode="moreLink">
              <xsl:with-param name="link" select="$parentURL"/>
              <xsl:with-param name="altText">
                <xsl:value-of select="BookName/node()"/>
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

  <!-- Book Brief Single_Book_Layout -->
  <xsl:template match="Content[@type='Book']" mode="displayBriefSingle_Book_Layout">
    <xsl:param name="sortBy"/>
    <!-- articleBrief -->
    <div class="detail Booklist">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail cbook Booklist'"/>
      </xsl:apply-templates>
      <h2 class="entry-title content-title">
        <xsl:apply-templates select="BookName/node()" mode="cleanXhtml"/>
      </h2>
      <xsl:if test="BookTitle/node()!=''">
        <h3 class="BookTitle">
          <xsl:apply-templates select="BookTitle/node()" mode="cleanXhtml"/>
        </h3>
      </xsl:if>
      <xsl:if test="BookStrapline/node()!=''">
        <h4 class="BookStrapline">
          <xsl:apply-templates select="BookStrapline/node()" mode="cleanXhtml"/>
        </h4>
      </xsl:if>
      <xsl:apply-templates select="." mode="displayDetailImage"/>
      <xsl:if test="Category/node()!=''">
        <div>
          <strong>Category: </strong>
          <span class="Category">
            <xsl:apply-templates select="Category/node()" mode="cleanXhtml"/>
          </span>
        </div>
      </xsl:if>
      <xsl:if test="FamilyFriendly/node()!=''">
        <div>
          <strong>FamilyFriendly: </strong>
          <span class="FamilyFriendly">
            <xsl:apply-templates select="FamilyFriendly/node()" mode="cleanXhtml"/>
          </span>
        </div>
      </xsl:if>
      <xsl:if test="SampleOfWork/node()!=''">
        <div class="SampleOfWork">
          <strong>SampleOfWork: </strong>
          <!--need change to oupput a downloadable link rther than html-->
          <xsl:apply-templates select="SampleOfWork/node()" mode="cleanXhtml"/>
        </div>

      </xsl:if>
      <xsl:if test="Copyright/node()!=''">
        <div>
          <strong>Copyright: </strong>
          <span class="Category">
            <xsl:apply-templates select="Copyright/node()" mode="cleanXhtml"/>
          </span>
        </div>
      </xsl:if>
      <xsl:if test="CopyrightYear/node()!=''">
        <div>
          <strong>CopyrightYear: </strong>
          <span class="CopyrightYear">
            <xsl:apply-templates select="CopyrightYear/node()" mode="cleanXhtml"/>
          </span>
        </div>
      </xsl:if>
      <xsl:if test="ShortDescription/node()!=''">
        <h3>Description</h3>
        <div class="ShortDescription">
          <xsl:apply-templates select="ShortDescription/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>
      <!--<xsl:if test="Reviews/node()!=''">
        <h3>Reviews</h3>
        <div class="Reviews">
          <xsl:apply-templates select="Reviews/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>-->
      <xsl:if test="Awards/node()!=''">
        <h3>Awards</h3>
        <div class="Awards">
          <xsl:apply-templates select="Awards/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>

      <xsl:if test="Content[@type='Contact']!=''">
        <div class="rContact">
          <h3>About the Author</h3>
          <xsl:apply-templates select="Content[@type='Contact']" mode="displayBrief"/>
        </div>
      </xsl:if>
      <xsl:if test="Content[@type='Testimonial']!=''">
        <div class="rTestimonial">
          <h1>
            <xsl:text>Testimonials </xsl:text>
          </h1>
          <xsl:apply-templates select="Content[@type='Testimonial']" mode="displayBrief"/>
          <xsl:text> </xsl:text>
        </div>
      </xsl:if>


      <!-- BookEdition's not working  -->
      <xsl:if test="Content[@type='BookEdition']!=''">
        <h2>Book Editions</h2>
        <xsl:apply-templates select="Content[@type='BookEdition']" mode="ContentDetail"/>
      </xsl:if>
      <!-- Terminus class fix to floating content -->
      <div class="terminus">&#160;</div>
      <!-- Book's not working  -->
      <xsl:if test="Content[@type='Book']!=''">
        <div class="rBooks row">
          <xsl:apply-templates select="Content[@type='Book']" mode="CollectionOf"/>
          <xsl:text> </xsl:text>
        </div>
      </xsl:if>

    </div>
  </xsl:template>

  <!-- Book Content Details-->
  <xsl:template match="Content[@type='Book']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>

    <!--<xsl:variable name="debugMode">
      <xsl:call-template name="getXmlSettings">
        <xsl:with-param name="sectionName">web</xsl:with-param>
        <xsl:with-param name="valueName">debug</xsl:with-param>
      </xsl:call-template>
    </xsl:variable>-->
    <div class="row">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'row'"/>
      </xsl:apply-templates>
      <div class="col-md-4">
        <xsl:apply-templates select="." mode="displayDetailImage">
         <xsl:with-param name="no-stretch" select="false()" />
        </xsl:apply-templates>
        <!-- if bookmarks enabled & NOT set to use modules to do it -->
        <xsl:if test="$page/Contents/Content[@type='SocialNetworkingSettings']">
          <xsl:variable name="bookmarkSettings" select="$page/Contents/Content[@type='SocialNetworkingSettings']/Bookmarks" />
          <!-- uses this span to re-write around the page -->
          <div>
            <xsl:apply-templates select="." mode="displayBookmarks">
              <xsl:with-param name="bookmarkSettings" select="$bookmarkSettings" />
              <xsl:with-param name="type" select="'Content'" />
            </xsl:apply-templates>
          </div>
          <div class="terminus">&#160;</div>
        </xsl:if>
      </div>
      <div class="col-md-8">
        <h1 class="entry-title content-title">
          <xsl:apply-templates select="BookName/node()" mode="cleanXhtml"/>
        </h1>
        
        <xsl:if test="BookTitle/node()!=''">
          <h1 class="BookTitle">
            <xsl:apply-templates select="BookTitle/node()" mode="cleanXhtml"/>
          </h1>
        </xsl:if>
        
        <xsl:if test="BookStrapline/node()!=''">
          <h3 class="BookStrapline">
            <xsl:apply-templates select="BookStrapline/node()" mode="cleanXhtml"/>
          </h3>
        </xsl:if>
        
        <xsl:if test="Content[@rtype='Author']">
            <xsl:apply-templates select="Content[@rtype='Author']" mode="displayAuthor"/>
        </xsl:if>
        
        <xsl:if test="Content[@rtype='Translator']">
            <div class="Translator">
              translated by
              <xsl:apply-templates select="Content[@rtype='Translator']" mode="displayAuthorContributors"/>
            </div>
        </xsl:if>
        
        <div class="contributors">
          <xsl:if test="Content[@rtype='Illustrator']">
            <span class="Illustrator">
              <span>
                illustrated by <xsl:apply-templates select="Content[@rtype='Illustrator']" mode="displayAuthorContributors"/>
              </span>
            </span>
          </xsl:if>
          
          <xsl:if test="Content[@rtype='Foreword']">
            <span class="Foreword">
              <span>
                foreword by <xsl:apply-templates select="Content[@rtype='Foreword']" mode="displayAuthorContributors"/>
              </span>
            </span>
          </xsl:if>
          
          <xsl:if test="Content[@rtype='Introduction']">
            <span class="introduction">
              <span>
                introduction by <xsl:apply-templates select="Content[@rtype='Introduction']" mode="displayAuthorContributors"/>
              </span>
            </span>
          </xsl:if>
          
          <xsl:if test="Content[@rtype='Afterword']">
            <span class="Foreword">
              <span>
                afterword by <xsl:apply-templates select="Content[@rtype='Afterword']" mode="displayAuthorContributors"/>
              </span>
            </span>
          </xsl:if>
          
          <xsl:if test="Content[@rtype='Preface']">
            <span class="Preface">
              <span>
                Preface by <xsl:apply-templates select="Content[@rtype='Preface']" mode="displayAuthorContributors"/>
              </span>
            </span>
          </xsl:if>
        </div>
        

        <!--Buying Options-->
        <xsl:if test="Links/node()!=''">
          <div class="dropdown Buyingoptions">
            <button class="btn btn-default dropdown-toggle" type="button" id="dropdownMenu1" data-toggle="dropdown" aria-haspopup="true" aria-expanded="true">
              Buying Options
              <!--<span class="caret"></span>-->
            </button>
            <ul class="dropdown-menu" aria-labelledby="dropdownMenu1">
              <xsl:for-each select="Links/Link">
                <li class="listitem">
                  <div class="Links">
                    <!--<h4>
                    <xsl:value-of select="LinkTitle"/>
                  </h4>-->
                    <a href="{LinkUrl/node()}" class="btn-sm btn-success" target="_blank">
                      <i class="fa fa-external-link"/>
                      <xsl:value-of select="LinkTitle"/>
                    </a>
                  </div>
                </li>
              </xsl:for-each>
            </ul>
          </div>
        </xsl:if>

        <div class="bookdetails">
          <dl class="dl-horizontal" >
            <xsl:if test="ISBN10/node()!=''">
              <dt class="ISBN10">ISBN:</dt>
              <dd>
                <xsl:apply-templates select="ISBN10/node()" mode="cleanXhtml"/>
              </dd>
            </xsl:if>
            <xsl:if test="ISBN13/node()!=''">
              <dt class="ISBN13"> ISBN:</dt>
              <dd>
                <xsl:apply-templates select="ISBN13/node()" mode="cleanXhtml"/>
              </dd>
            </xsl:if>

            <xsl:if test="Content[@rtype='Publisher']">
              <dt class="Publisher">Publisher:</dt>
              <dd>
                <xsl:apply-templates select="Content[@rtype='Publisher']" mode="displayLink"/>
                <xsl:text> </xsl:text>
              </dd>
            </xsl:if>

            <xsl:if test="PublicationYear/node()!=''">
              <dt class="PublicationYear">PublicationYear:</dt>
              <dd>
                <xsl:apply-templates select="PublicationYear/node()" mode="cleanXhtml"/>
              </dd>
            </xsl:if>
            <xsl:if test="Content[@rtype='CopyRight']">
              <dt class="CopyRight">Copyright: </dt>
              <dd>
                <xsl:apply-templates select="Content[@rtype='Copyright']" mode="displayAuthorContributors"/>
              </dd>
            </xsl:if>
            <xsl:if test="CopyRightYear/node()!=''">
              <dt>Copyright Year:</dt>
              <dd>
                <xsl:apply-templates select="CopyRightYear/node()" mode="cleanXhtml"/>
              </dd>
            </xsl:if>
           
            <xsl:if test="Content[@type='BookSeries']">
              <dt>Series:</dt>
              <dd>
                 <xsl:apply-templates select="Content[@type='BookSeries']" mode="displayLink"/>
                  <xsl:text> </xsl:text>
              </dd>
            </xsl:if>
             <xsl:if test="Content[@rtype='Genre']">
              <dt  class="Genre">Genre:</dt>
              <dd  class="Genred">
                 <xsl:apply-templates select="Content[@type='Genre']" mode="displayLink"/>
                  <xsl:text> </xsl:text>
              </dd>
            </xsl:if>
          </dl>
        </div>

        <xsl:if test="Content[@rtype='SampleOfwork']">
          <div class="SampleOfWork">
            <strong>SampleOfWork: </strong>
            <xsl:apply-templates select="Content[@rtype='SampleOfwork']" mode="displayBrief"/>
          </div>
        </xsl:if>

        <xsl:if test="ShortDescription/node()!=''">
          <div class="ShortDescription">
            <xsl:apply-templates select="ShortDescription/node()" mode="cleanXhtml"/>
          </div>
        </xsl:if>
        
        <xsl:if test="FullDescription/node()!=''">
          <div class="FullDescription">
            <xsl:apply-templates select="FullDescription/node()" mode="cleanXhtml"/>
          </div>
        </xsl:if>
      </div><!--col-md-8-->

    </div> <!--end row-->
    <div>
        <xsl:if test="Awards/node()!=''">
           <div class="Awards">
          <h3>Awards</h3>
            <xsl:for-each select="Awards/Award">
              <h5>
                <xsl:value-of select="Title"/>
              </h5>
              <a href="{Url/node()}" class="btn btn-primary" target="_blank">
               Read More
              </a>
            </xsl:for-each>
          </div>
        </xsl:if>

        <xsl:if test="Content[@type='Testimonial']!=''">
          <div class="rTestimonial">
            <h3>
              <xsl:text>Testimonials </xsl:text>
            </h3>
            <xsl:apply-templates select="Content[@type='Testimonial']" mode="displayBrief"/>
            <xsl:text> </xsl:text>
          </div>
        </xsl:if>

        <xsl:if test="Content[@rtype='ProReview']!='' or Content[@rtype='ReaderReview']!=''">
          <h3>Reviews</h3>
          <div class="rProReview row">
          <xsl:if test="Content[@rtype='ProReview']!=''">
          
            <xsl:apply-templates select="Content[@rtype='ProReview']" mode="displayBrief">
              <xsl:with-param name="class" select="'col-md-3'"/>
            </xsl:apply-templates>
            <xsl:text> </xsl:text>
          
          </xsl:if>

        <xsl:if test="Content[@rtype='ReaderReview']!=''">
         
            <xsl:apply-templates select="Content[@rtype='ReaderReview']" mode="displayBrief">
              <xsl:with-param name="class" select="'col-md-3'"/>
            </xsl:apply-templates>
            <xsl:text> </xsl:text>
         
        </xsl:if>
 </div>
        </xsl:if>
    
        <!-- BookEdition's -->
        <xsl:if test="Content[@type='BookEdition']!=''">
          <h2>Book Editions</h2>
          <xsl:apply-templates select="Content[@type='BookEdition']" mode="displayBrief"/>
        </xsl:if>
        
        <xsl:if test="Content[@type='Book']!=''">
	          <h3>Related Books</h3>
          <div class="rBook row">
              <xsl:apply-templates select="Content[@type='Book']" mode="CollectionOf"/>
            <xsl:text> </xsl:text>
          </div>
        </xsl:if>
        
        <xsl:if test="Content[@type='BookSeries' and count(Content) &gt; 0]!=''">
 
            <h3>Other books in this series</h3>
            <div class="BookSeries row">
            <xsl:for-each select="Content[@type='BookSeries']/Content[@type='Book']">

              <xsl:apply-templates select="." mode="displayBriefCover_and_Title">
                <xsl:with-param name="class" select="'col-md-2'"/>
              <xsl:sort select="@publish" order="descending"/>
            </xsl:apply-templates>

            </xsl:for-each>
            <xsl:text> </xsl:text>
          </div>

        </xsl:if>
      
     
        <!-- Terminus class fix to floating content -->
        <div class="terminus">&#160;</div>

        <xsl:apply-templates select="." mode="backLink">
          <xsl:with-param name="link" select="$thisURL"/>
          <xsl:with-param name="altText">
            <xsl:call-template name="term2006" />
          </xsl:with-param>
        </xsl:apply-templates>
  </div>
  </xsl:template>

    <!-- content -->
  <xsl:template match="Content[@type='Book']" mode="socialBookmarks"/>
    
    <!-- display bookmarks -->
  <xsl:template match="*" mode="displayBookmarks">
    <xsl:param name="bookmarkSettings" />
    <xsl:param name="type" select="'MenuItem'"/>
    <xsl:variable name="layout">
      <xsl:value-of select="$bookmarkSettings/*[name()=$type]/@size" />
      <xsl:if test="$bookmarkSettings/*[name()=$type]/@size='standard' and $bookmarkSettings/*[name()=$type]/@count='true'">
        <xsl:text>-count</xsl:text>
      </xsl:if>
    </xsl:variable>
    <div class="socialBookmarks">
      <xsl:attribute name="class">
        <xsl:text>socialBookmarks bookmarks-</xsl:text>
        <xsl:value-of select="$layout" />
      </xsl:attribute>
      <xsl:apply-templates select="$page/Contents/Content[@type='SocialNetworkingSettings']" mode="inlinePopupOptions">
        <xsl:with-param name="class">
          <xsl:text>socialBookmarks bookmarks-</xsl:text>
          <xsl:value-of select="$layout" />
        </xsl:with-param>
      </xsl:apply-templates>
      <div>
        <xsl:apply-templates select="$bookmarkSettings/Methods/@*[.='true']" mode="displayBookmark">
        <xsl:sort select="name()" order="descending" data-type="text"/>
        <xsl:with-param name="bookmarkSettings" select="$bookmarkSettings/*[name()=$type]" />
      </xsl:apply-templates>
      <xsl:text> </xsl:text>
      </div>
    
    </div>
    <div class="terminus">&#160;</div>
  </xsl:template>
  

  <!-- BookEdition Content Detail-->
  <xsl:template match="Content[@type='BookEdition']" mode="displayBrief">
    <xsl:apply-templates select="." mode="inlinePopupOptions"/>
    <xsl:if test="Format='Hardback'">
      <xsl:apply-templates select="." mode="ContentDetailHardback" />
    </xsl:if>
    <xsl:if test="Format='E-Book'">
      <xsl:apply-templates select="." mode="ContentDetailEbook" />
    </xsl:if>
    <xsl:if test="Format='Paperback'">
      <xsl:apply-templates select="." mode="ContentDetailPaperback" />
    </xsl:if>
    <xsl:if test="Format='Audiobook'">
      <xsl:apply-templates select="." mode="ContentDetailAudiobook" />
    </xsl:if>
  </xsl:template>

  <!-- BookEdition Content Detail Hardback-->
  <xsl:template match="Content[@type='BookEdition']" mode="ContentDetailHardback">
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <h2 class="entry-title content-title">
      <a href="{$parentURL}" title="Read More - {EditionName/node()}">
        <xsl:apply-templates select="EditionName/node()" mode="cleanXhtml"/>
      </a>
    </h2>

    <xsl:apply-templates select="." mode="displayDetailImage"/>
    <xsl:if test="Format/node()!=''">
      <div class="Format">
        <h4>
          <xsl:apply-templates select="Format/node()" mode="cleanXhtml"/>
        </h4>
      </div>
    </xsl:if>
    <xsl:if test="PublishedDate/node()!=''">
      <div class="date">
        <strong>
          <xsl:text>Published Date: </xsl:text>
        </strong>
        <!--<xsl:value-of select="PublishedDate/node()"/>-->
        <xsl:call-template name="DisplayDate">
          <xsl:with-param name="date" select="PublishedDate/node()"/>
        </xsl:call-template>
      </div>
    </xsl:if>
    <xsl:if test="Pages/node()!=''">
      <div class="Pages">
        <strong>
          <xsl:text>Pages: </xsl:text>
        </strong>
        <xsl:apply-templates select="Pages/node()" mode="cleanXhtml"/>
      </div>
    </xsl:if>
    <xsl:if test="ProductDimensions/node()!=''">
      <div class="ProductDimensions">
        <strong>
          <xsl:text>ProductDimensions: </xsl:text>
        </strong>
        <xsl:apply-templates select="Pages/node()" mode="cleanXhtml"/>
      </div>
    </xsl:if>
    <xsl:if test="ISBN/node()!=''">
      <div class="ISBN">
        <strong>
          <xsl:text>ISBN: </xsl:text>
        </strong>
        <xsl:apply-templates select="ISBN/node()" mode="cleanXhtml"/>
      </div>
    </xsl:if>
    <xsl:if test="InLanguage/node()!=''">
      <div class="InLanguage">
        <strong>
          <xsl:text>Language: </xsl:text>
        </strong>
        <xsl:apply-templates select="InLanguage/node()" mode="cleanXhtml"/>
      </div>
    </xsl:if>
    <xsl:if test="Translator/node()!=''">
      <div class="Translator">
        <strong>
          <xsl:text>Translator: </xsl:text>
        </strong>
        <xsl:apply-templates select="Translator/node()" mode="cleanXhtml"/>
      </div>
    </xsl:if>
    <xsl:if test="Format/node()!=''">
      <div class="Format">
        <strong>
          <xsl:text>Format: </xsl:text>
        </strong>
        <xsl:apply-templates select="Format/node()" mode="cleanXhtml"/>
      </div>
    </xsl:if>
    
       <!--Buying Options-->
        <xsl:if test="Links/node()!=''">
          <div class="dropdown Buyingoptions">
            <button class="btn btn-default dropdown-toggle" type="button" id="dropdownMenu1" data-toggle="dropdown" aria-haspopup="true" aria-expanded="true">
              Buying Options
              <!--<span class="caret"></span>-->
            </button>
            <ul class="dropdown-menu" aria-labelledby="dropdownMenu1">
              <xsl:for-each select="Links/Link">
                <li class="listitem">
                  <div class="award">
                    <!--<h4>
                    <xsl:value-of select="LinkTitle"/>
                  </h4>-->
                    <a href="{LinkUrl/node()}" class="btn-sm btn-success">
                      <i class="fa fa-external-link"/>
                      <xsl:value-of select="LinkTitle"/>
                    </a>
                  </div>
                </li>
              </xsl:for-each>
            </ul>
          </div>
        </xsl:if>
    <xsl:if test="Content[@type='SKU']!=''">
      <xsl:choose>
        <xsl:when test="Content[@type='SKU']">
          <strong>
            <xsl:text>Price: </xsl:text>
          </strong>
          <xsl:apply-templates select="Content[@type='SKU'][1]" mode="displayPrice" />
        </xsl:when>
        <xsl:otherwise>
          <strong>
            <xsl:text>Price: </xsl:text>
          </strong>
          <xsl:apply-templates select="." mode="displayPrice" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
    <xsl:if test="Content[@type='Organisation']!=''">
      <div class="rBook">
        <xsl:apply-templates select="Content[@type='Organisation']" mode="cleanXhtml"/>
        <xsl:text> </xsl:text>
      </div>
    </xsl:if>
    <xsl:if test="/Page/Cart">
      <xsl:apply-templates select="." mode="addToCartButton"/>
    </xsl:if>
  </xsl:template>

  <!-- BookEdition Content Detail Ebook -->
  <xsl:template match="Content[@type='BookEdition']" mode="ContentDetailEbook">
    <h2 class="entry-title content-title">
      <xsl:apply-templates select="EditionName/node()" mode="cleanXhtml"/>
    </h2>
    <xsl:apply-templates select="." mode="displayDetailImage"/>
    <xsl:if test="Epub/node()!=''">
      <div class="Pages">
        <strong>
          <xsl:text>Epub: </xsl:text>
        </strong>
        <xsl:apply-templates select="Epub/node()" mode="cleanXhtml"/>
      </div>
    </xsl:if>
    <xsl:if test="Epub[@fileSize!='']">
      <div class="Pages">
        <strong>
          <xsl:text>File Size: </xsl:text>
        </strong>
        <xsl:apply-templates select="Epub[@fileSize]" mode="cleanXhtml"/>
      </div>
    </xsl:if>
  </xsl:template>

  <!-- BookEdition Content Detail Paperback -->
  <xsl:template match="Content[@type='BookEdition']" mode="ContentDetailPaperback">
    <h2 class="entry-title content-title">
      <xsl:apply-templates select="EditionName/node()" mode="cleanXhtml"/>
    </h2>
    <xsl:apply-templates select="." mode="displayDetailImage"/>
    <xsl:if test="Format/node()!=''">
      <div class="Format">
        <h4>
          <xsl:apply-templates select="Format/node()" mode="cleanXhtml"/>
        </h4>
      </div>
    </xsl:if>
    <xsl:if test="PublishedDate/node()!=''">
      <div class="date">
        <strong>
          <xsl:text>Published Date: </xsl:text>
        </strong>
        <!--<xsl:value-of select="PublishedDate/node()"/>-->
        <xsl:call-template name="DisplayDate">
          <xsl:with-param name="date" select="PublishedDate/node()"/>
        </xsl:call-template>
      </div>
    </xsl:if>
    <xsl:if test="Pages/node()!=''">
      <div class="Pages">
        <strong>
          <xsl:text>Pages: </xsl:text>
        </strong>
        <xsl:apply-templates select="Pages/node()" mode="cleanXhtml"/>
      </div>
    </xsl:if>
    <xsl:if test="ProductDimensions/node()!=''">
      <div class="ProductDimensions">
        <strong>
          <xsl:text>ProductDimensions: </xsl:text>
        </strong>
        <xsl:apply-templates select="Pages/node()" mode="cleanXhtml"/>
      </div>
    </xsl:if>
    <xsl:if test="ISBN/node()!=''">
      <div class="ISBN">
        <strong>
          <xsl:text>ISBN: </xsl:text>
        </strong>
        <xsl:apply-templates select="ISBN/node()" mode="cleanXhtml"/>
      </div>
    </xsl:if>
    <xsl:if test="Illustrator/node()!=''">
      <div class="Illustrator">
        <strong>
          <xsl:text>Illustrator: </xsl:text>
        </strong>
        <xsl:apply-templates select="Illustrator/node()" mode="cleanXhtml"/>
      </div>
    </xsl:if>
    <xsl:if test="InLanguage/node()!=''">
      <div class="InLanguage">
        <strong>
          <xsl:text>InLanguage: </xsl:text>
        </strong>
        <xsl:apply-templates select="InLanguage/node()" mode="cleanXhtml"/>
      </div>
    </xsl:if>
    <xsl:if test="Translator/node()!=''">
      <div class="Translator">
        <strong>
          <xsl:text>Translator: </xsl:text>
        </strong>
        <xsl:apply-templates select="Translator/node()" mode="cleanXhtml"/>
      </div>
    </xsl:if>
    <xsl:if test="Format/node()!=''">
      <div class="Format">
        <strong>
          <xsl:text>Format: </xsl:text>
        </strong>
        <xsl:apply-templates select="Format/node()" mode="cleanXhtml"/>
      </div>
    </xsl:if>
    <!--Buying Options-->
        <xsl:if test="Links/node()!=''">
          <div class="dropdown Buyingoptions">
            <button class="btn btn-default dropdown-toggle" type="button" id="dropdownMenu1" data-toggle="dropdown" aria-haspopup="true" aria-expanded="true">
              Buying Options
              <!--<span class="caret"></span>-->
            </button>
            <ul class="dropdown-menu" aria-labelledby="dropdownMenu1">
              <xsl:for-each select="Links/Link">
                <li class="listitem">
                  <div class="award">
                    <!--<h4>
                    <xsl:value-of select="LinkTitle"/>
                  </h4>-->
                    <a href="{LinkUrl/node()}" class="btn-sm btn-success">
                      <i class="fa fa-external-link"/>
                      <xsl:value-of select="LinkTitle"/>
                    </a>
                  </div>
                </li>
              </xsl:for-each>
            </ul>
          </div>
        </xsl:if>
    <xsl:if test="Content[@type='SKU']!=''">
      <xsl:choose>
        <xsl:when test="Content[@type='SKU']">
          <strong>
            <xsl:text>Price: </xsl:text>
          </strong>
          <xsl:apply-templates select="Content[@type='SKU'][1]" mode="displayPrice" />
        </xsl:when>
        <xsl:otherwise>
          <strong>
            <xsl:text>Price: </xsl:text>
          </strong>
          <xsl:apply-templates select="." mode="displayPrice" />
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
    <xsl:if test="Content[@type='Organisation']!=''">
      <div class="rBook">
        <xsl:apply-templates select="Content[@type='Organisation']" mode="cleanXhtml"/>
        <xsl:text> </xsl:text>
      </div>
    </xsl:if>
    <xsl:if test="/Page/Cart">
      <xsl:apply-templates select="." mode="addToCartButton"/>
    </xsl:if>
  </xsl:template>

  <!-- BookEdition Content Detail Audiobook -->
  <xsl:template match="Content[@type='BookEdition']" mode="ContentDetailAudiobook">
    <h2 class="entry-title content-title">
      <xsl:apply-templates select="EditionName/node()" mode="cleanXhtml"/>
    </h2>
    <xsl:if test="ISBN/node()!=''">
      <div class="ISBN">
        <strong>
          <xsl:text>ASIN: </xsl:text>
        </strong>
        <xsl:apply-templates select="ISBN/node()" mode="cleanXhtml"/>
      </div>
    </xsl:if>
    <xsl:if test="Pages/node()!=''">
      <div class="Pages">
        <strong>
          <xsl:text>Listerning Length: </xsl:text>
        </strong>
        <xsl:apply-templates select="Pages/node()" mode="cleanXhtml"/>
      </div>
    </xsl:if>
    <xsl:if test="Epub[@bridged]!=''">
      <div class="Pages">
        <strong>
          <xsl:text>Listerning Length: </xsl:text>
        </strong>
        <xsl:apply-templates select="Epub[@bridged]" mode="cleanXhtml"/>
      </div>
    </xsl:if>
    <xsl:apply-templates select="." mode="displayDetailImage"/>
  </xsl:template>

  <!-- BookEdition Content Detail CollectionOf -->
  <xsl:template match="Content[@type='Book']" mode="CollectionOf">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <!--<xsl:variable name="debugMode">
      <xsl:call-template name="getXmlSettings">
        <xsl:with-param name="sectionName">web</xsl:with-param>
        <xsl:with-param name="valueName">debug</xsl:with-param>
      </xsl:call-template>
    </xsl:variable>-->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="list-item Book col-md-2">
        <a href="{$parentURL}" title="Read More - {BookName/node()}">
           <xsl:apply-templates select="." mode="displayThumbnail"/>
        </a>
      <h2 class="entry-title content-title">
        <a href="{$parentURL}" title="Read More - {BookName/node()}">
          <xsl:apply-templates select="BookName/node()" mode="cleanXhtml"/>
        </a>
      </h2>
           <!-- Terminus class fix to floating content -->
      <div class="terminus">&#160;</div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='Contact']" mode="displayAuthorBook">
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
      <!--<xsl:if test="Title/node()!=''">
          <h6 class="title">
            <xsl:apply-templates select="Title" mode="displayBrief"/>
            <xsl:if test="Company/node()!=''">
              <xsl:text> - </xsl:text>
              <xsl:apply-templates select="Company" mode="displayBrief"/>
            </xsl:if>
          </h6>
        </xsl:if>-->
    </div>
  </xsl:template>
  
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
      <xsl:if test="Images/img/@src!=''">
        <a href="{$parentURL}" rel="author" title="click here to view more details on {GivenName/node()} {Surname/node()}">
          <xsl:apply-templates select="." mode="displayThumbnail">
            <xsl:with-param name="width">56</xsl:with-param>
            <xsl:with-param name="height">56</xsl:with-param>
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
  
  <xsl:template match="Content[@type='Contact']" mode="displayAuthorContributors">
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
            <xsl:with-param name="width">56</xsl:with-param>
            <xsl:with-param name="height">56</xsl:with-param>
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
  
   <!-- Tags Brief -->
  <xsl:template match="Content[@type='Genre']" mode="displayLink">
    <xsl:param name="sortBy"/>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <xsl:variable name="name" select="Name/node()"/>
    <span>
      <a href="{$parentURL}" rel="tag">
        <xsl:apply-templates select="Name" mode="displayBrief"/>
        <xsl:if test="@relatedCount!=''">
          &#160;(<xsl:value-of select="@relatedCount"/>)
        </xsl:if>
      </a>
      <xsl:if test="position()!=last()">
        <xsl:text>, </xsl:text>
      </xsl:if>
    </span>
  </xsl:template>
  
  <xsl:template match="Content[@type='Organisation']" mode="displayLink">
    <xsl:param name="sortBy"/>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <xsl:variable name="name" select="name/node()"/>
    <span>
      <a href="{$parentURL}" rel="tag">
        <xsl:apply-templates select="name" mode="displayBrief"/>
        <xsl:if test="@relatedCount!=''">
          &#160;(<xsl:value-of select="@relatedCount"/>)
        </xsl:if>
      </a>
      <xsl:if test="position()!=last()">
        <xsl:text>, </xsl:text>
      </xsl:if>
    </span>
  </xsl:template>

    <!-- Tags Detail Genre -->
  <xsl:template match="Content[@type='Genre']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <div class="detail genre">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail tag'"/>
      </xsl:apply-templates>
      <h1>
        <xsl:value-of select="Name/node()"/>
      </h1>
      <div class="row">
        <xsl:for-each select="Content[@type='Book']">
        <div class="col-md-3">
          <xsl:apply-templates select="." mode="displayBriefCover_and_Title">
          <xsl:sort select="@publish" order="descending"/>
        </xsl:apply-templates>
        </div>
          
        </xsl:for-each>
        
        <xsl:text> </xsl:text>
      </div>
      <!-- Terminus class fix to floating content -->
      <div class="terminus">&#160;</div>
    </div>
  </xsl:template>
  
   <!-- Tags Detail Organisation -->
  <xsl:template match="Content[@type='Organisation']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <div class="detail genre">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail tag'"/>
      </xsl:apply-templates>
      <h1>
        <xsl:value-of select="name/node()"/>
      </h1>
      <div class="row">
        <xsl:for-each select="Content[@type='Book']">
        <div class="col-md-3">
          <xsl:apply-templates select="." mode="displayBriefCover_and_Title">
          <xsl:sort select="@publish" order="descending"/>
        </xsl:apply-templates>
        </div>
        </xsl:for-each>
        <xsl:text> </xsl:text>
      </div>
      <!-- Terminus class fix to floating content -->
      <div class="terminus">&#160;</div>
    </div>
  </xsl:template>
  
   <!-- Tags Detail CollectionOf -->
  <xsl:template match="Content[@rtype='CollectionOf']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <div class="detail genre">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail tag'"/>
      </xsl:apply-templates>
      <h1>
        <xsl:value-of select="name/node()"/>
      </h1>
      <div class="row">
        <xsl:for-each select="Content[@type='Book']">
        <div class="col-md-3">
          <xsl:apply-templates select="." mode="displayBriefCover_and_Title">
          <xsl:sort select="@publish" order="descending"/>
        </xsl:apply-templates>
        </div>
        </xsl:for-each>
        <xsl:text> </xsl:text>
      </div>
      <!-- Terminus class fix to floating content -->
      <div class="terminus">&#160;</div>
    </div>
  </xsl:template>
 
 
  
   <!-- Tags BookSeries -->
  <xsl:template match="Content[@type='BookSeries']" mode="displayLink">
    <xsl:param name="sortBy"/>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <xsl:variable name="name" select="Name/node()"/>
    <span>
      <a href="{$parentURL}" rel="tag">
        <xsl:apply-templates select="Name" mode="displayBrief"/>
        <xsl:if test="@relatedCount!=''">
          &#160;(<xsl:value-of select="@relatedCount"/>)
        </xsl:if>
      </a>
      <xsl:if test="position()!=last()">
        <xsl:text>, </xsl:text>
      </xsl:if>
    </span>
  </xsl:template>
  
    <!-- Book List Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='BookSeriesList']" mode="displayBrief">
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

    <!-- Output Module -->
    <div class="clearfix BookSeriesList">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix BookList content-scroller</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <div class="cols cols{@cols}" data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1" data-dots="{@carouselBullets}" data-height="{@carouselHeight}" >
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
        <!-- If Stepper, display Stepper -->
        <xsl:if test="@stepCount != '0' and not($page[@cssFramework='bs3'])">
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
        <xsl:if test="@stepCount != '0' and $page[@cssFramework='bs3']">
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
  
  <!-- BookSeries displayBrief -->
  <xsl:template match="Content[@type='BookSeries']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <!-- articleBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="listItem list-group-item Book">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem list-group-item Book'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner media">
        <xsl:if test="Images/img/@src!=''">
          <a href="{$parentURL}" title="Read More - {Name/node()}">
            <xsl:apply-templates select="." mode="displayThumbnail"/>
          </a>
          <span class="hidden">|</span>
        </xsl:if>

        <xsl:for-each select="Content[@type='Book']">
          <div class="col-md-3">
            <xsl:apply-templates select="." mode="displayBrief">
              <xsl:sort select="@publish" order="descending"/>
            </xsl:apply-templates>
          </div>
        </xsl:for-each>
        
        <div class="media-body">
          <h2 class="entry-title content-title">
            <a href="{$parentURL}" title="Read More - {Name/node()}">
              <xsl:apply-templates select="Name/node()" mode="cleanXhtml"/>
            </a>
          </h2>
          <div class="entryFooter">
            <xsl:apply-templates select="." mode="displayTags"/>
            <xsl:apply-templates select="." mode="moreLink">
              <xsl:with-param name="link" select="$parentURL"/>
              <xsl:with-param name="altText">
                <xsl:value-of select="Name/node()"/>
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
  
    <!-- BookSeries Detail -->
  <xsl:template match="Content[@type='BookSeries']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <div class="detail genre">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail tag'"/>
      </xsl:apply-templates>
      <h1>
        <xsl:value-of select="Name/node()"/>
      </h1>
      <div class="row">
        <xsl:for-each select="Content[@type='Book']">
        <div class="col-md-3">
          <xsl:apply-templates select="." mode="displayBriefCover_and_Title">
          <xsl:sort select="@publish" order="descending"/>
        </xsl:apply-templates>
        </div>
        </xsl:for-each>
        <xsl:text> </xsl:text>
      </div>
      <!-- Terminus class fix to floating content -->
      <div class="terminus">&#160;</div>
    </div>
  </xsl:template>

 
 
</xsl:stylesheet>

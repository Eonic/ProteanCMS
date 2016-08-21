<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
  <xsl:import href="../Membership/EwUserEditableControlsV4_0.xsl"/>
  
  
	<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ -->
	<!-- ####################################  BLOGGING ######################################## -->
	<!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ -->

  <!-- ################################### User Editable JS ################################## -->

  <xsl:template match="Page[@layout='Blog_1' and not(@adminMode) and User]" mode="pageJs">
      <xsl:if test="not(/Page/@adminMode) or /Page/ContentDetail/Content[@name='EditContent'] or /Page/@ewCmd='Normal'">
        <script type="text/javascript" src="/ewcommon/js/jQuery/jquery-1.3.min.js">&#160;</script>
        <script type="text/javascript" src="/ewcommon/js/jquery/ui/jquery-ui-1.8.7.custom.min.js">&#160;</script>
      </xsl:if>
    <script type="text/javascript" src="/ewcommon/js/jquery/form/jquery.form.js">&#160;</script>
    <script type="text/javascript" src="/ewcommon/js/UserEditable.js">&#160;</script>

    <xsl:if test="not(/Page/User)">
      <xsl:variable name="loginURL">
        <xsl:apply-templates select="/Page/Menu/MenuItem/descendant-or-self::MenuItem[layout/node()='Logon_Register'][1]" mode="getHref"/>
      </xsl:variable>
      <script type="text/javascript">
        window.location = '<xsl:value-of select="$loginURL"/>';
      </script>
    </xsl:if>
  </xsl:template>
  <!-- -->

	<!-- ################################### BLOG OUTPUTS ####################################### -->
	<!-- -->
	<xsl:template match="Content[@type='BlogSettings']" mode="displayContent">
		<dl>
			<xsl:for-each select="*">
				<dt>
					<xsl:value-of select="local-name()"/>
				</dt>
				<dd>
					<xsl:value-of select="node()"/>
				</dd>
			</xsl:for-each>
		</dl>
	</xsl:template>
	<!-- -->
	<xsl:template match="Content[@type='BlogArticle']" mode="displayContent">
		<xsl:variable name="contentURL">
			<xsl:apply-templates select="." mode="getHref"/>
		</xsl:variable>
		<div class="list blog">
			<xsl:apply-templates select="." mode="inlinePopupOptions">
				<xsl:with-param name="class" select="'list blog'"/>
			</xsl:apply-templates>
			<h4>
				<a href="{$contentURL}" title="{@name}">
					<xsl:value-of select="Headline/node()"/>
				</a>
			</h4>
			<p>
				<xsl:call-template name="DD_Mon_YYYY">
					<xsl:with-param name="date" select="@publish"/>
				</xsl:call-template>
				<xsl:text> - </xsl:text>
				<xsl:call-template name="truncate-string">
					<xsl:with-param name="text" select="Article/node()"/>
					<xsl:with-param name="length" select="'100'"/>
				</xsl:call-template>
			</p>
			<xsl:apply-templates select="." mode="moreLink">
				<xsl:with-param name="link" select="$contentURL"/>
				<xsl:with-param name="altText">
					<xsl:value-of select="Headline/node()"/>
				</xsl:with-param>
			</xsl:apply-templates>
		</div>
	</xsl:template>
	<!-- -->
	<!-- -->
	<xsl:template match="Content[@type='BlogArticle']" mode="achiveArticle">
		<xsl:variable name="contentURL">
			<xsl:apply-templates select="." mode="getHref"/>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="@id=/Page/ContentDetail/Content[@type='BlogArticle']/@id">
				<span>
					<xsl:value-of select="Headline/node()"/>
				</span>
			</xsl:when>
			<xsl:otherwise>
				<a href="{$contentURL}" title="{Headline/node()}">
					<xsl:value-of select="Headline/node()"/>
				</a>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	
	<!-- -->
	
	<xsl:template match="Content[@type='BlogArticle']" mode="Content_Detail">
		<xsl:apply-templates select="/Page/ContentDetail/Content" mode="Blog_Detail"/>
	</xsl:template>
	
	<!-- -->
	
	<xsl:template match="Content[@type='BlogArticle' and /Page/@layout='Blog_1']" mode="Content_Detail">
		<xsl:apply-templates select="/Page" mode="Layout"/>
	</xsl:template>
	
	<!-- -->
	
	<xsl:template match="Content[@type='BlogArticle']" mode="Blog_Detail">
		<h1>
			<xsl:value-of select="Headline/node()"/>
		</h1>
		<p>
			<xsl:call-template name="DD_Mon_YYYY">
				<xsl:with-param name="date" select="@publish"/>
			</xsl:call-template>
		</p>
		<xsl:apply-templates select="Article/node()" mode="cleanXhtml"/>
		<p>
			<a href="javascript:history.go(-1)" title="back" class="backlink">&lt; Back</a>
		</p>
    <xsl:if test="Content[@type='Comment'] or (/Page/@adminMode or /Page/User)">
      
        <div id="newComment">
          <xsl:apply-templates select="/Page" mode="inlinePopupAdd">
					<xsl:with-param name="type">Comment</xsl:with-param>
					<xsl:with-param name="text">Add Comment</xsl:with-param>
					<xsl:with-param name="name">New Comment</xsl:with-param>
					<xsl:with-param name="contentParId">
						<xsl:value-of select="@id"/>
					</xsl:with-param>
				</xsl:apply-templates>
          <xsl:apply-templates select="/Page" mode="userEditControl">
            <xsl:with-param name="type">Comment</xsl:with-param>
            <xsl:with-param name="text">Add Comment</xsl:with-param>
            <xsl:with-param name="name">NewBlogComment</xsl:with-param>
            <xsl:with-param name="formName">Comment_User</xsl:with-param>
            <xsl:with-param name="id">newComment</xsl:with-param>
            <xsl:with-param name="parId" select="'0'"/>
            <xsl:with-param name="contentParId" select="@id"/>
            <xsl:with-param name="verId" select="@versionid"/>
          </xsl:apply-templates>
        </div>
			<div id="detailComments">
				<xsl:apply-templates select="Content[@type='Comment']" mode="displayContent">
					<xsl:sort select="@publish" data-type="text" order="ascending"/>
				</xsl:apply-templates>
			</div>
		</xsl:if>
	</xsl:template>
	
	<!-- -->
	
	<xsl:template match="Content[@type='Comment']" mode="displayContent">
		<div class="list comment">
			<xsl:attribute name="class">
				<xsl:text>list comment</xsl:text>
				<xsl:if test="position() mod 2 = 0">
					<xsl:text> commentAlt</xsl:text>
				</xsl:if>
			</xsl:attribute>
			<xsl:apply-templates select="." mode="inlinePopupOptions">
				<xsl:with-param name="class">
					<xsl:text>list comment</xsl:text>
					<xsl:if test="position() mod 2 = 0">
						<xsl:text> commentAlt</xsl:text>
					</xsl:if>
				</xsl:with-param>
			</xsl:apply-templates>

			<xsl:if test="@owner=/Page/User/@id and /Page/User/@pagePermission='AddUpdateOwn'">
				<xsl:if test="not(contains(/Page/Request/*/Item[@name='HTTP_X_REWRITE_URL']/node(),'ajaxContentForm.ashx'))">

					<xsl:apply-templates select="." mode="userEditEditOptions">
						<xsl:with-param name="targetContainerId">
							<xsl:text>comment</xsl:text>
							<xsl:value-of select="@id"/>
						</xsl:with-param>
						<xsl:with-param name="id" select="@id"/>
						<xsl:with-param name="formName" select="'Comment_User'"/>
						<xsl:with-param name="class">
							<xsl:text>comment</xsl:text>
							<xsl:if test="position() mod 2 = 0">
								<xsl:text> commentAlt</xsl:text>
							</xsl:if>
						</xsl:with-param>
						<xsl:with-param name="contentParId" select="parent::Content[@type='BlogArticle']/@id"/>
						<xsl:with-param name="verId" select="@versionid"/>
					</xsl:apply-templates>
				</xsl:if>
			</xsl:if>
			<p>
				<xsl:value-of select="Article/node()" />
			</p>
			<xsl:if test="@status=3">
				<p class="help">This comment is awaiting approval</p>
			</xsl:if>
			<p class="credentials">
				<xsl:call-template name="DD_Mon_YYYY">
					<xsl:with-param name="date" select="@publish"/>
				</xsl:call-template>
        <xsl:if test="User/node()">
				<xsl:text> - </xsl:text>
        </xsl:if>
				<b>
					<xsl:value-of select="User/node()"/>
				</b>
				<xsl:if test="EditRecord/node()!=''">
					<br/>
					<span class="editRecord">
						<xsl:value-of select="EditRecord/node()"/>
					</span>
				</xsl:if>
			</p>
		</div>
	</xsl:template>
  
	<!-- ################################### BLOG LAYOUTS #######################################-->
	<!-- -->
	<xsl:template match="Page[@layout='Blog_1']" mode="Layout">
		<div class="template template_2_Columns_66_33" id="template_Blog_1" >
			<xsl:apply-templates select="/" mode="layoutHeader"/>
			<xsl:apply-templates select="/" mode="BlogLayout"/>
			<xsl:apply-templates select="/" mode="layoutFooter"/>
		</div>
	</xsl:template>
	<!-- -->
	<xsl:template match="/" mode="BlogLayout">
		<xsl:if test="/Page/@adminMode and not(/Page/ContentDetail)">
			<div id="blogSettings">
				<xsl:apply-templates select="/Page" mode="inlinePopupSingle">
					<xsl:with-param name="type">BlogSettings</xsl:with-param>
					<xsl:with-param name="text">Add Blog Settings</xsl:with-param>
					<xsl:with-param name="name">BlogControl</xsl:with-param>
				</xsl:apply-templates>
				<!--<xsl:apply-templates select="/Page/Contents/Content[@type='BlogSettings']" mode="displayContent"/>-->
			</div>
		</xsl:if>
		<xsl:if test="/Page/Contents/Content[@type='BlogSettings']">
			<div>
				<xsl:apply-templates select="/Page" mode="inlinePopupAdd">
					<xsl:with-param name="type">BlogArticle</xsl:with-param>
					<xsl:with-param name="text">Add Article</xsl:with-param>
					<xsl:with-param name="name">New Blog Article</xsl:with-param>
				</xsl:apply-templates>
        
			</div>
		</xsl:if>
		<div id="column1">
			<xsl:apply-templates select="Page" mode="displayBlog"/>
		</div>
		<div id="column2">
			<div class="socialBookmarks">
				<xsl:if test="/Page/Contents/Content[@type='BlogSettings']/SocialBookMarkingLinks='true'">
				<xsl:apply-templates select="/" mode="socialBookmarks">
					<xsl:with-param name="size" select="'32'"/>
				</xsl:apply-templates>
				</xsl:if>
				<xsl:apply-templates select="/Page/Contents/Content[@type='BlogSettings']" mode="rssBrief"/>
			</div>
			<xsl:apply-templates select="/Page/Contents/Content[@type='BlogSettings']/Strapline/node()" mode="cleanXhtml"/>
			<xsl:apply-templates select="/" mode="contentBox">
				<xsl:with-param name="contentMode" select="'blogArchives'"/>
				<xsl:with-param name="contentHardTitle" select="'Archive'"/>
			</xsl:apply-templates>
			<xsl:if test="/Page/Contents/Content[@type='BlogArticle']/Categories/category/node()!=''">
				<xsl:apply-templates select="/" mode="contentBox">
					<xsl:with-param name="contentMode" select="'blogCategories'"/>
					<xsl:with-param name="contentHardTitle" select="'Categories'"/>
				</xsl:apply-templates>
			</xsl:if>
		</div>
	</xsl:template>
	<!-- -->
	<!-- ############################## MAIN BLOG DISPLAY's #####################################-->
	<!-- -->
	<xsl:template match="Page" mode="displayBlog">
		<div id="blogDisplay">
			<h1>
				<xsl:value-of select="/Page/Contents/Content[@type='BlogSettings']/BlogTitle/node()"/>
			</h1>
			<h3>Top 10 Latest Posts</h3>
			<xsl:for-each select="/Page/Contents/Content[@type='BlogArticle']">
				<xsl:sort select="@publish" data-type="text" order="descending"/>
				<xsl:sort select="@update" data-type="text" order="descending"/>
				<xsl:if test="position()&lt;=10">
					<xsl:apply-templates select="." mode="displayContent"/>
				</xsl:if>
			</xsl:for-each>
		</div>
	</xsl:template>
	<!-- -->
	<xsl:template match="Page[not(/Page/Contents/Content[@type='BlogArticle'])]" mode="displayBlog">
		<div id="blogDisplay">
			<h3>There are currently no posts on this Blog</h3>
		</div>
	</xsl:template>
	<!-- -->
	<!-- -->
	<xsl:template match="Page[ContentDetail]" mode="displayBlog">
		<div id="blogDisplay">
			<xsl:apply-templates select="/Page/ContentDetail/Content" mode="Blog_Detail"/>
		</div>
	</xsl:template>
	<!-- -->
	<!-- -->
	<xsl:template match="Page[Request/QueryString/Item[@name='datesorting']]" mode="displayBlog">
		<xsl:variable name="selectedDate" select="/Page/Request/QueryString/Item[@name='datesorting']/node()"/>
		<div id="blogDisplay">
			<h3>
				<xsl:text>Posts publised </xsl:text>
				<xsl:call-template name="Month_YYYY">
					<xsl:with-param name="date" select="$selectedDate"/>
				</xsl:call-template>
			</h3>
			<xsl:for-each select="/Page/Contents/Content[@type='BlogArticle' and contains(@publish,$selectedDate)]">
				<xsl:sort select="@publish" data-type="text" order="descending"/>
				<xsl:sort select="@update" data-type="text" order="descending"/>
				<xsl:apply-templates select="." mode="displayContent"/>
			</xsl:for-each>
		</div>
	</xsl:template>
	<!-- -->
	<!-- -->
	<xsl:template match="Page[Request/QueryString/Item[@name='catsorting']]" mode="displayBlog">
		<xsl:variable name="selectedCat" select="translate(/Page/Request/QueryString/Item[@name='catsorting']/node(),'~',' ')"/>
		<div id="blogDisplay">
			<h3>
				<xsl:text>Posts in the </xsl:text>
				<xsl:value-of select="$selectedCat"/>
				<xsl:text> category</xsl:text>
			</h3>
			<xsl:for-each select="/Page/Contents/Content[@type='BlogArticle' and Categories/category/node()=$selectedCat]">
				<xsl:sort select="@publish" data-type="text" order="descending"/>
				<xsl:sort select="@update" data-type="text" order="descending"/>
				<xsl:apply-templates select="." mode="displayContent"/>
			</xsl:for-each>
		</div>
	</xsl:template>
	<!-- -->
	<!-- ################################### BLOG RSS LINKS #####################################-->
	<!-- -->
	<xsl:template match="Content[@type='BlogSettings']" mode="rssBrief">
		<xsl:variable name="feedURL">
			
			<xsl:text>/ewcommon/feeds/rss/feed.ashx?pgid=</xsl:text>
			<xsl:value-of select="$currentPage/@id"/>
			
			<xsl:text>&amp;settingsId=</xsl:text>
			<xsl:value-of select="@id"/>
			
			<xsl:if test="/Page/ContentDetail">
				<xsl:text>&amp;artid=</xsl:text>
				<xsl:value-of select="/Page/ContentDetail/Content/@id"/>
			</xsl:if>
			
		</xsl:variable>
		<a href="{$feedURL}" title="Subsribe to {@name}">
			<img class="rss-icon" src="/ewcommon/images/icons/socialnetworking/rss_32.png" width="32" height="32" alt="RSS Feed"/>
		</a>
	</xsl:template>
	<!-- -->
	<!-- #################################### BLOG ARCHIVES #####################################-->
	<!-- -->
	<xsl:template match="/" mode="blogArchives">
		<xsl:variable name="currentURL">
			<xsl:apply-templates select="$currentPage" mode="getHref"/>
		</xsl:variable>
		<ul class="blogArchive">
			<li>
				<a href="{$currentURL}" title="view latest posts">
					<xsl:text>Latest</xsl:text>
				</a>
			</li>
			<xsl:for-each select="/Page/Contents/Content[@type='BlogArticle']">
				<xsl:sort select="@publish" order="descending" data-type="text"/>
				<xsl:sort select="@update" data-type="text" order="descending"/>
				<xsl:variable name="year" select="substring(@publish,1,4)"/>
				<xsl:if test="@id=/Page/Contents/Content[@type='BlogArticle' and substring(@publish,1,4)=$year][1]/@id">
					<li>
						<a href="{$currentURL}?datesorting={$year}" title="view posts from {$year}">
							<xsl:value-of select="$year"/>
						</a>
						<xsl:text> (</xsl:text>
						<xsl:value-of select="count(/Page/Contents/Content[@type='BlogArticle' and substring(@publish,1,4)=$year])"/>
						<xsl:text>)</xsl:text>
						<xsl:if test="contains(/Page/Request/QueryString/Item[@name='datesorting']/node(),$year) or contains(/Page/ContentDetail/Content[@type='BlogArticle']/@publish,$year)">
							<ul>
								<xsl:for-each select="/Page/Contents/Content[@type='BlogArticle' and substring(@publish,1,4)=$year]">
									<xsl:sort select="@publish" order="descending" data-type="text"/>
									<xsl:sort select="@update" data-type="text" order="descending"/>
									<xsl:variable name="month" select="substring(@publish,6,2)"/>
									<xsl:if test="@id=/Page/Contents/Content[@type='BlogArticle' and substring(@publish,1,4)=$year and substring(@publish,6,2)=$month][1]/@id">
										<li>
											<a href="{$currentURL}?datesorting={substring(@publish,1,7)}">
												<xsl:call-template name="getShortMonth">
													<xsl:with-param name="month" select="number($month)"/>
												</xsl:call-template>
											</a>
											<xsl:text> (</xsl:text>
											<xsl:value-of select="count(/Page/Contents/Content[@type='BlogArticle' and substring(@publish,1,4)=$year and substring(@publish,6,2)=$month])"/>
											<xsl:text>)</xsl:text>
											<xsl:variable name="currentDate">
												<xsl:value-of select="$year"/>
												<xsl:text>-</xsl:text>
												<xsl:value-of select="$month"/>
											</xsl:variable>
											<xsl:if test="contains(/Page/Request/QueryString/Item[@name='datesorting']/node(),$currentDate) or contains(/Page/ContentDetail/Content[@type='BlogArticle']/@publish,$currentDate)">
												<ul>
													<xsl:for-each select="/Page/Contents/Content[@type='BlogArticle' and substring(@publish,1,4)=$year and substring(@publish,6,2)=$month]">
														<xsl:sort select="@publish" order="descending" data-type="text"/>
														<xsl:sort select="@update" data-type="text" order="descending"/>
														<li>
															<xsl:apply-templates select="." mode="achiveArticle"/>
														</li>
													</xsl:for-each>
												</ul>
											</xsl:if>
										</li>
									</xsl:if>
								</xsl:for-each>
							</ul>
						</xsl:if>
					</li>
				</xsl:if>
			</xsl:for-each>
		</ul>

	</xsl:template>
	<!-- -->
	<!-- ###################################### CATEGORIES ######################################-->
	<!-- -->
	<xsl:template match="/" mode="blogCategories">
		<xsl:variable name="currentURL">
			<xsl:apply-templates select="$currentPage" mode="getHref"/>
		</xsl:variable>
		<ul class="blogArchive">
			<xsl:variable name="catList">
				<xsl:call-template name="getCleanCategories">
					<xsl:with-param name="catList">
						<xsl:for-each select="/Page/Contents/Content[@type='BlogArticle']/Categories/category[/node()!='']">
							<xsl:sort select="node()" data-type="text" order="ascending"/>
							<xsl:value-of select="node()"/>
							<xsl:text>,</xsl:text>
						</xsl:for-each>
					</xsl:with-param>
					<xsl:with-param name="newList" select="''"/>
				</xsl:call-template>
			</xsl:variable>
			<xsl:call-template name="listCategories">
				<xsl:with-param name="catList" select="$catList"/>
			</xsl:call-template>
		</ul>
	</xsl:template>
	<!-- -->
	<xsl:template name="getCleanCategories">
		<xsl:param name="catList"/>
		<xsl:param name="newList"/>
		<xsl:choose>
			<xsl:when test="$catList!=''">
				<xsl:variable name="currentItem" select="substring-before($catList,',')"/>
				<xsl:call-template name="getCleanCategories">
					<xsl:with-param name="catList" select="substring-after($catList,',')"/>
					<xsl:with-param name="newList">
						<xsl:value-of select="$newList"/>
						<xsl:if test="not(contains($newList,$currentItem))">
							<xsl:value-of select="$currentItem"/>
							<xsl:text>,</xsl:text>
						</xsl:if>
					</xsl:with-param>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$newList"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<xsl:template name="listCategories">
		<xsl:param name="catList"/>
		<xsl:if test="$catList!=''">
			<xsl:variable name="currentItem" select="substring-before($catList,',')"/>
			<li>
				<a href="{$currentPage/@url}?catsorting={translate($currentItem,' ','~')}" title="view {$currentItem} posts">
					<xsl:value-of select="$currentItem"/>
				</a>
				<xsl:text> (</xsl:text>
				<xsl:value-of select="count(/Page/Contents/Content[@type='BlogArticle' and Categories/category/node()=$currentItem])"/>
				<xsl:text>)</xsl:text>
				<xsl:if test="contains(translate(/Page/Request/QueryString/Item[@name='catsorting']/node(),'~',' '),$currentItem) or contains(/Page/ContentDetail/Content[@type='BlogArticle']/Categories/category,$currentItem)">
					<ul>
						<xsl:for-each select="/Page/Contents/Content[@type='BlogArticle' and Categories/category/node()=$currentItem]">
							<xsl:sort select="@publish" order="descending" data-type="text"/>
							<xsl:sort select="@update" data-type="text" order="descending"/>
							<li>
								<xsl:apply-templates select="." mode="achiveArticle"/>
							</li>
						</xsl:for-each>
					</ul>
				</xsl:if>
			</li>
			<xsl:call-template name="listCategories">
				<xsl:with-param name="catList">
					<xsl:value-of select="substring-after($catList,',')"/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

</xsl:stylesheet>
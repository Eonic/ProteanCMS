<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!--  ======================================================================================  -->
  <!--  ==  RECIPIES  ========================================================================  -->
  <!--  ======================================================================================  -->
  <!-- Recipe Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='RecipeList']" mode="displayBrief">
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
    <div class="RecipeList">
      <div class="cols{@cols}">
        <!-- If Stepper, display Stepper -->
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

  <!-- Recipe Brief -->
  <xsl:template match="Content[@type='Recipe']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <!-- recipeBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="listItem list-group-item recipe hrecipe">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem list-group-item recipe hrecipe'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <h3 class="title">
          <a href="{$parentURL}" title="Read More - {Headline/node()}">
            <xsl:apply-templates select="." mode="getDisplayName"/>
          </a>
        </h3>
        <xsl:if test="Images/img/@src!=''">
          <a href="{$parentURL}" title="Read More - {Headline/node()}">
            <xsl:apply-templates select="." mode="displayThumbnail"/>
          </a>
          <!--Accessiblity fix : Separate adjacent links with more than whitespace-->
          <span class="hidden">|</span>
        </xsl:if>
        <div class="summary">
          <xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
        </div>
        <xsl:if test="Yield/node()!='' or PrepTime/node()!='' or CookTime/node()!=''">
          <div class="guide">
            <p>
              <xsl:if test="Yield/node()!=''">
                <span class="label">
                  <!-- No. of servings -->
                  <xsl:call-template name="term2075a"/>
                  <xsl:text>: </xsl:text>
                </span>
                <strong>
                  <xsl:value-of select="Yield/node()" />
                </strong>
                <br/>
              </xsl:if>
              <xsl:if test="PrepTime/node()!=''">
                <span class="label">
                  <!-- Preparation time -->
                  <xsl:call-template name="term2076"/>
                  <xsl:text>: </xsl:text>
                </span>
                <xsl:call-template name="getNiceTimeFromMinutes">
                  <xsl:with-param name="mins" select="PrepTime/node()" />
                  <xsl:with-param name="format" select="'short'" />
                </xsl:call-template>
                <br/>
              </xsl:if>
              <xsl:if test="CookTime/node()!=''">
                <span class="label">
                  <!-- Cooking time -->
                  <xsl:call-template name="term2077"/>
                  <xsl:text>: </xsl:text>
                </span>
                <xsl:call-template name="getNiceTimeFromMinutes">
                  <xsl:with-param name="mins" select="CookTime/node()" />
                  <xsl:with-param name="format" select="'short'" />
                </xsl:call-template>
              </xsl:if>
            </p>
          </div>
        </xsl:if>
        <div class="entryFooter">
          <xsl:apply-templates select="." mode="displayTags"/>
          <xsl:apply-templates select="." mode="moreLink">
            <xsl:with-param name="link" select="$parentURL"/>
            <xsl:with-param name="altText">
              <xsl:value-of select="Headline/node()"/>
            </xsl:with-param>
          </xsl:apply-templates>
          <xsl:text> </xsl:text>
        </div>
        <!-- Accessiblity fix : Separate adjacent links with more than whitespace -->
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- Recipe Detail -->
  <xsl:template match="Content[@type='Recipe']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <div id="template_2_Columns_66_33" class="detail recipe hrecipe template_2_Columns_66_33">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail hrecipe newsarticle'"/>
      </xsl:apply-templates>
      <div id="column1">
        <h1 class="fn">
          <xsl:apply-templates select="." mode="getDisplayName" />
        </h1>
        <xsl:if test="Strapline/node()">
          <div class="summary">
            <xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
          </div>
        </xsl:if>
        <div class="description">
          <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
        </div>
        <xsl:if test="Yield/node()!='' or PrepTime/node()!='' or CookTime/node()!=''">
          <div class="guide">
            <p>
              <xsl:if test="Yield/node()!=''">
                <span class="label">
                  <!-- No. of servings -->
                  <xsl:call-template name="term2075a"/>
                  <xsl:text>: </xsl:text>
                </span>
                <span class="yield">
                  <xsl:value-of select="Yield/node()" />
                </span>
                <br/>
              </xsl:if>
              <xsl:if test="PrepTime/node()!=''">
                <span class="preptime">
                  <span class="label">
                    <!-- Preparation time -->
                    <xsl:call-template name="term2076"/>
                    <xsl:text>: </xsl:text>
                  </span>
                  <xsl:call-template name="getNiceTimeFromMinutes">
                    <xsl:with-param name="mins" select="PrepTime/node()" />
                    <xsl:with-param name="format" select="'short'" />
                  </xsl:call-template>
                  <span class="value-title">
                    <xsl:attribute name="title">
                      <xsl:call-template name="getISO-8601-Duration">
                        <xsl:with-param name="secs" select="PrepTime/node() * 60" />
                      </xsl:call-template>
                    </xsl:attribute>
                  </span>
                </span>
                <br/>
              </xsl:if>
              <xsl:if test="CookTime/node()!=''">
                <span class="cooktime">
                  <span class="label">
                    <!-- Cooking time -->
                    <xsl:call-template name="term2077"/>
                    <xsl:text>: </xsl:text>
                  </span>
                  <xsl:call-template name="getNiceTimeFromMinutes">
                    <xsl:with-param name="mins" select="CookTime/node()" />
                    <xsl:with-param name="format" select="'short'" />
                  </xsl:call-template>
                  <span class="value-title">
                    <xsl:attribute name="title">
                      <xsl:call-template name="getISO-8601-Duration">
                        <xsl:with-param name="secs" select="CookTime/node() * 60" />
                      </xsl:call-template>
                    </xsl:attribute>
                  </span>
                </span>
              </xsl:if>
            </p>
          </div>
        </xsl:if>
        <xsl:if test="Ingredients/node()">
          <div class="ingredient">
            <!-- need both classes for microformats -->
            <h3>
              <!-- Ingredients -->
              <xsl:call-template name="term2075"/>
            </h3>
            <span class="ingredients">
              <xsl:apply-templates select="Ingredients/node()" mode="cleanXhtml"/>
            </span>
          </div>
        </xsl:if>
        <xsl:if test="Instructions/node()">
          <div class="instructions">
            <h3>
              <!-- Method -->
              <xsl:call-template name="term2078"/>
            </h3>
            <xsl:apply-templates select="Instructions/node()" mode="cleanXhtml"/>
          </div>
        </xsl:if>
        <!-- social networking - share -->
        <xsl:apply-templates select="." mode="share" />
        <!-- Reviews -->
        <xsl:apply-templates select="." mode="relatedReviews" />
      </div>
      <div id="column2">
        <xsl:apply-templates select="." mode="displayDetailImage"/>
        <xsl:if test="Equipment/node()">
          <div class="equipment">
            <!-- need both classes for microformats -->
            <h2>
              <!-- Ingredients -->
              <xsl:text>Equipment</xsl:text>
            </h2>
            <span class="equipment">
              <xsl:apply-templates select="Equipment/node()" mode="cleanXhtml"/>
            </span>
            <div class="terminus">&#160;</div>
          </div>
        </xsl:if>
        <div class="credentials">
          <p class="published">
            <!-- Published -->
            <xsl:call-template name="term2079"/>
            <xsl:text> </xsl:text>
            <xsl:call-template name="DisplayDate">
              <xsl:with-param name="date" select="@publish"/>
            </xsl:call-template>
            <span class="value-title" title="{substring(@publish,1,10)}"></span>
          </p>
          <xsl:if test="Content[@type='Contact']">
            <div class="author">
              <xsl:apply-templates select="Content[@type='Contact']" mode="displayBrief" />
            </div>
          </xsl:if>
        </div>
      </div>
      <!-- Terminus class fix to floating content -->
      <div class="terminus">&#160;</div>
      <div class="entryFooter">
        <div class="tags">
          <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
          <xsl:text> </xsl:text>
        </div>
        <xsl:apply-templates select="." mode="backLink">
          <xsl:with-param name="link" select="$thisURL"/>
          <xsl:with-param name="altText">
            <xsl:call-template name="term2006" />
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <!--RELATED CONTENT-->
      <xsl:if test="Content">
        <!-- Recipe  -->
        <xsl:if test="Content[@type='Recipe']">
          <div class="relatedcontent">
            <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
              <xsl:with-param name="type">PlainText</xsl:with-param>
              <xsl:with-param name="text">Add Title for Related Recipes</xsl:with-param>
              <xsl:with-param name="name">relatedRecipesTitle</xsl:with-param>
            </xsl:apply-templates>
            <h4>
              <xsl:choose>
                <xsl:when test="/Page/Contents/Content[@name='relatedRecipesTitle']">
                  <xsl:apply-templates select="/Page/Contents/Content[@name='relatedRecipesTitle']" mode="displayBrief"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>Related Recipes</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
            </h4>
            <xsl:apply-templates select="/" mode="List_Related_Recipes">
              <xsl:with-param name="parRecipeID" select="@id"/>
            </xsl:apply-templates>
          </div>
        </xsl:if>
      </xsl:if>
    </div>
  </xsl:template>

  <!-- List Related Recipes-->
  <xsl:template match="/" mode="List_Related_Recipes">
    <xsl:param name="parProductID"/>
    <xsl:for-each select="/Page/ContentDetail/Content/Content">
      <xsl:sort select="@type" order="ascending"/>
      <xsl:sort select="@displayOrder" order="ascending"/>
      <xsl:choose>
        <xsl:when test="@type='Recipe'">
          <xsl:apply-templates select="." mode="displayBrief"/>
        </xsl:when>
      </xsl:choose>
    </xsl:for-each>
  </xsl:template>
  
</xsl:stylesheet>
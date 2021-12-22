<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!-- ############## Job Vacancy ##############   -->
  <!-- Job Vacancy Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='VacanciesList']" mode="displayBrief">
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
    <div class="clearfix VacancyList">
      <xsl:if test="@carousel='true'">
        <xsl:attribute name="class">
          <xsl:text>clearfix VacancyList content-scroller</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <div data-slidestoshow="{@cols}"  data-slideToShow="{$totalCount}" data-slideToScroll="1" >
        <!--responsive columns-->
        <xsl:apply-templates select="." mode="contentColumns"/>
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
        <!-- If Stepper, display Stepper -->
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="vacancyList" select="$contentList"/>
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
            <xsl:text> </xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
            <xsl:text> </xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </div>
    </div>
  </xsl:template>

  <!-- Job Brief -->
  <xsl:template match="Content[@type='Job']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <!-- vacancyBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="listItem">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem'"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <a href="{$parentURL}" title="Read More - {Headline/node()}" class="list-image-link vacancy-image">
          <xsl:apply-templates select="." mode="displayThumbnail"/>
        </a>
        <h3 class="title">
          <a href="{$parentURL}" title="{JobTitle/node()}">
            <xsl:value-of select="JobTitle/node()"/>
          </a>
        </h3>
        <div class="vacancy-intro">
          <dl class="dl-horizontal">
            <xsl:if test="@publish and @publish!=''">
              <dt class="date">
                <!--Added on-->
                <xsl:call-template name="term2062" />
                <xsl:text>: </xsl:text>
              </dt>
              <dd>
                <xsl:call-template name="DisplayDate">
                  <xsl:with-param name="date" select="@publish"/>
                </xsl:call-template>
              </dd>
            </xsl:if>
            <xsl:if test="ContractType/node()!=''">
              <dt class="contract">
                <!--Contract Type-->
                <xsl:call-template name="term2063" />
                <xsl:text>: </xsl:text>
              </dt>
              <dd>
                <xsl:value-of select="ContractType/node()"/>
              </dd>
            </xsl:if>
            <xsl:if test="Ref/node()!=''">
              <dt class="ref">
                <!--Ref-->
                <xsl:call-template name="term2064" />
                <xsl:text>: </xsl:text>
              </dt>
              <dd>
                <xsl:value-of select="Ref/node()"/>
              </dd>
            </xsl:if>
            <xsl:if test="Location/node()!=''">
              <dt class="location">
                <!--Location-->
                <xsl:call-template name="term2065" />
                <xsl:text>: </xsl:text>
              </dt>
              <dd>
                <xsl:value-of select="Location/node()"/>
              </dd>
            </xsl:if>
            <xsl:if test="Salary/node()!=''">
              <dt class="salary">
                <!--Salary-->
                <xsl:call-template name="term2066" />
                <xsl:text>: </xsl:text>
              </dt>
              <dd>
                <xsl:value-of select="Salary/node()"/>
              </dd>
            </xsl:if>
            <xsl:if test="ApplyBy/node()!=''">
              <dt class="applyBy">
                <!--Deadline for applications-->
                <xsl:call-template name="term2067" />
                <xsl:text>: </xsl:text>
              </dt>
              <dd>
                <xsl:call-template name="DisplayDate">
                  <xsl:with-param name="date" select="ApplyBy/node()"/>
                </xsl:call-template>
              </dd>
            </xsl:if>
          </dl>
        </div>
        <div class="vacancy-summary">
          <xsl:if test="Summary/node()!=''">
            <xsl:apply-templates select="Summary/node()" mode="cleanXhtml"/>
          </xsl:if>
        </div>
        <div class="entryFooter">
          <xsl:apply-templates select="." mode="displayTags"/>
          <xsl:apply-templates select="." mode="moreLink">
            <xsl:with-param name="link" select="$parentURL"/>
            <xsl:with-param name="altText">
              <xsl:value-of select="JobTitle/node()"/>
            </xsl:with-param>
          </xsl:apply-templates>
          <xsl:text> </xsl:text>
        </div>
      </div>
    </div>
  </xsl:template>

  <!-- Job Brief -->
  <xsl:template match="Content[@type='Job']" mode="displayBriefLinked">
    <xsl:param name="sortBy"/>
    <!-- vacancyBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="listItem">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem'"/>
      </xsl:apply-templates>
      <a href="{$parentURL}" title="{JobTitle/node()}">
        <div class="lIinner">
          <h3 class="title">
            <xsl:value-of select="JobTitle/node()"/>

          </h3>
          <xsl:apply-templates select="." mode="displayThumbnail"/>
          <div class="vacancy-intro">
            <dl class="dl-horizontal">
              <xsl:if test="@publish and @publish!=''">
                <dt class="date">
                  <!--Added on-->
                  <xsl:call-template name="term2062" />
                  <xsl:text>: </xsl:text>
                </dt>
                <dd>
                  <xsl:call-template name="DisplayDate">
                    <xsl:with-param name="date" select="@publish"/>
                  </xsl:call-template>
                </dd>
              </xsl:if>
              <xsl:if test="ContractType/node()!=''">
                <dt class="contract">
                  <!--Contract Type-->
                  <xsl:call-template name="term2063" />
                  <xsl:text>: </xsl:text>
                </dt>
                <dd>
                  <xsl:value-of select="ContractType/node()"/>
                </dd>
              </xsl:if>
              <xsl:if test="Ref/node()!=''">
                <dt class="ref">
                  <!--Ref-->
                  <xsl:call-template name="term2064" />
                  <xsl:text>: </xsl:text>
                </dt>
                <dd>
                  <xsl:value-of select="Ref/node()"/>
                </dd>
              </xsl:if>
              <xsl:if test="Location/node()!=''">
                <dt class="location">
                  <!--Location-->
                  <xsl:call-template name="term2065" />
                  <xsl:text>: </xsl:text>
                </dt>
                <dd>
                  <xsl:value-of select="Location/node()"/>
                </dd>
              </xsl:if>
              <xsl:if test="Salary/node()!=''">
                <dt class="salary">
                  <!--Salary-->
                  <xsl:call-template name="term2066" />
                  <xsl:text>: </xsl:text>
                </dt>
                <dd>
                  <xsl:value-of select="Salary/node()"/>
                </dd>
              </xsl:if>
              <xsl:if test="ApplyBy/node()!=''">
                <dt class="applyBy">
                  <!--Deadline for applications-->
                  <xsl:call-template name="term2067" />
                  <xsl:text>: </xsl:text>
                </dt>
                <dd>
                  <xsl:call-template name="DisplayDate">
                    <xsl:with-param name="date" select="ApplyBy/node()"/>
                  </xsl:call-template>
                </dd>
              </xsl:if>
            </dl>
          </div>
          <div class="vacancy-summary">
            <xsl:if test="Summary/node()!=''">
              <xsl:apply-templates select="Summary/node()" mode="cleanXhtml"/>
            </xsl:if>
          </div>

        </div>
      </a>
    </div>
  </xsl:template>

  <!-- Job Detail -->
  <xsl:template match="Content[@type='Job']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <div class="detail vacancy" itemscope="" itemtype="http://schema.org/JobPosting" >
      <meta itemprop="specialCommitments" content="" />
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail vacancy'"/>
      </xsl:apply-templates>
      <h2 class="content-title" itemprop="title">
        <xsl:value-of select="JobTitle/node()"/>
      </h2>
      <div class="vacancy-intro">
        <dl class="dl-horizontal">
          <xsl:if test="@publish and @publish!=''">
            <dt class="date" itemprop="datePosted">
              <xsl:call-template name="term2068" />
              <xsl:text>: </xsl:text>
            </dt>
            <dd>
              <xsl:call-template name="DisplayDate">
                <xsl:with-param name="date" select="@publish"/>
              </xsl:call-template>
            </dd>
          </xsl:if>
          <xsl:if test="ApplyBy/node()">
            <dt class="applyBy">
              <!--Deadline for applications-->
              <xsl:call-template name="term2067" />
              <xsl:text>:</xsl:text>
            </dt>
            <dd>
              <xsl:if test="ApplyBy/node()!=''">
                <xsl:call-template name="DisplayDate">
                  <xsl:with-param name="date" select="ApplyBy/node()"/>
                </xsl:call-template>
              </xsl:if>
            </dd>
          </xsl:if>
          <xsl:if test="ContractType/node()">
            <dt class="jobContractType ">
              <!--Contract Type-->
              <xsl:call-template name="term2063" />
              <xsl:text>:</xsl:text>
            </dt>
            <dd class="item"  itemprop="employmentType">
              <xsl:apply-templates select="ContractType/node()" mode="cleanXhtml"/>
            </dd>
          </xsl:if>
          <xsl:if test="Ref/node()">
            <dt class="ref ">
              <!--Reference-->
              <xsl:call-template name="term2069" />
              <xsl:text>:</xsl:text>
            </dt>
            <dd class="item">
              <xsl:apply-templates select="Ref/node()" mode="cleanXhtml"/>
            </dd>
          </xsl:if>
          <xsl:if test="JobIndustry/node()">
            <dt class="Jobindustry ">
              <!--Industry-->
              <xsl:call-template name="term2085" />
              <xsl:text>:</xsl:text>
            </dt>
            <dd itemprop="industry">
              <xsl:apply-templates select="JobIndustry/node()" mode="cleanXhtml"/>
            </dd>
          </xsl:if>
          <xsl:if test="JobOccupation/@ref">
            <dt class="hidden">
              <!--ref-->
              <xsl:call-template name="term2064" />
              <xsl:text>:</xsl:text>
            </dt>
            <dd class="hidden" itemprop="occupationalCategory">
              <xsl:apply-templates select="JobOccupation/@ref" mode="cleanXhtml"/>
            </dd>
          </xsl:if>
          <xsl:if test="JobOccupation/@name and JobOccupation/@name!=''">
            <dt class="JobOccupation ">
              <!--Occupation-->
              <xsl:call-template name="term2086" />
              <xsl:text>:</xsl:text>
            </dt>
            <dd class="item" itemprop="occupationalCategory">
              <xsl:apply-templates select="JobOccupation/@name" mode="cleanXhtml"/>
            </dd>
          </xsl:if>
          <xsl:if test="Location/node()">
            <dt class="jobLocation ">
              <!--Location-->
              <xsl:call-template name="term2065" />
              <xsl:text>:</xsl:text>
            </dt>
            <dd class="item" itemprop="jobLocation" itemscope="" itemtype="http://schema.org/Place">
              <xsl:apply-templates select="Location/node()" mode="displayContent"/>
            </dd>
          </xsl:if>
          <xsl:if test="JobHours/node()">
            <dt class="JobHours">
              <!--work hours-->
              <xsl:call-template name="term2090" />
              <xsl:text>:</xsl:text>
            </dt>
            <dd class="item" itemprop="workHours">
              <xsl:apply-templates select="JobHours/node()" mode="cleanXhtml"/>
            </dd>
          </xsl:if>
          <xsl:if test="Salary/node()!=''">
            <dt class="BaseSalary ">
              <!--Base Salary-->
              <xsl:call-template name="term2087" />
              <xsl:text>:</xsl:text>
            </dt>
            <dd class="item" itemprop="baseSalary">
              <xsl:apply-templates select="Salary/node()" mode="cleanXhtml"/>
            </dd>
          </xsl:if>
          <xsl:if test="Salary/@currency">
            <dt class="Currency hidden">
              <!--Currency-->
              <xsl:call-template name="term2088" />
              <xsl:text>:</xsl:text>
            </dt>
            <dd class="hidden" itemprop="salaryCurrency">
              <xsl:apply-templates select="Salary/@currency" mode="cleanXhtml"/>
            </dd>
          </xsl:if>
          <xsl:if test="Salary/@figure">
            <dt class="Salaryfigre hidden">
              <!--Figure-->
              <xsl:call-template name="term2089" />
              <xsl:text>:</xsl:text>
            </dt>
            <dd class="hidden" >
              <xsl:apply-templates select="Salary/@figure" mode="cleanXhtml"/>
            </dd>
          </xsl:if>
        </dl>
      </div>
      <xsl:apply-templates select="." mode="displayDetailImage"/>
      <xsl:if test="Description/node()">
        <h3 class="Jobdescription">
          <!--Description-->
          <xsl:call-template name="term2092" />
        </h3>
        <div itemprop="description">
          <xsl:apply-templates select="Description/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>
      <xsl:if test="Responsibities/node()">
        <h3 class="Responsibities">
          <!--Responsibilities-->
          <xsl:call-template name="term2084" />
        </h3>
        <div itemprop="responsibilities">
          <xsl:apply-templates select="Responsibities/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>
      <xsl:if test="Skills/node()">
        <h3 class="Skills">
          <xsl:call-template name="term2094" />
        </h3>
        <div itemprop="skills" >
          <xsl:apply-templates select="Skills/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>
      <xsl:if test="EducationRequirements/node()">
        <h3 class="EducationRequirements">
          <xsl:call-template name="term2095" />
        </h3>
        <div itemprop="educationRequirements">
          <xsl:apply-templates select="EducationRequirements/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>
      <xsl:if test="ExperienceRequirements/node()">
        <h3 class="ExperienceRequirements">
          <xsl:call-template name="term2096" />
        </h3>
        <div itemprop="experienceRequirements">
          <xsl:apply-templates select="ExperienceRequirements/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>
      <xsl:if test="Qualifications/node()">
        <h3 class="Qualifications">
          <xsl:call-template name="term2097" />
        </h3>
        <div itemprop="qualifications">
          <xsl:apply-templates select="Qualifications/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>
      <xsl:if test="Incentives/node()">
        <h3 class="Incentives">
          <xsl:call-template name="term2098" />
        </h3>
        <div itemprop="incentives">
          <xsl:apply-templates select="Incentives/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>
      <div class="entryFooter">
        <div class="tags">
          <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
          <xsl:text> </xsl:text>
        </div>
        <xsl:apply-templates select="." mode="backLink">
          <xsl:with-param name="link" select="$thisURL"/>
          <xsl:with-param name="altText">
            <!--click here to return to the news article list-->
            <xsl:call-template name="term2071" />
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>


</xsl:stylesheet>
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

	<!-- ############## Job Vacancy ##############   -->

	<!-- Job Brief -->
	<xsl:template match="Content[@type='Job']" mode="displayBrief">
		<xsl:param name="sortBy"/>
		<xsl:param name="crop"/>
		<xsl:param name="class"/>
		<xsl:param name="parentId"/>
		<xsl:param name="linked"/>
		<xsl:variable name="parentURL">
			<xsl:apply-templates select="." mode="getHref"/>
		</xsl:variable>
		<xsl:variable name="classValues">
			<xsl:text>listItem job </xsl:text>
			<xsl:if test="$linked='true'">
				<xsl:text> linked-listItem </xsl:text>
			</xsl:if>
			<xsl:value-of select="$class"/>
			<xsl:text> </xsl:text>
			<xsl:apply-templates select="." mode="themeModuleClassExtrasListItem">
				<xsl:with-param name="parentId" select="$parentId"/>
			</xsl:apply-templates>
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
		<div class="{$classValues}">
			<xsl:apply-templates select="." mode="themeModuleExtrasListItem">
				<xsl:with-param name="parentId" select="$parentId"/>
				<xsl:with-param name="pos" select="position()"/>
			</xsl:apply-templates>
			<xsl:apply-templates select="." mode="inlinePopupOptions">
				<xsl:with-param name="class" select="concat($classValues,' ',$class)"/>
				<xsl:with-param name="sortBy" select="$sortBy"/>
			</xsl:apply-templates>
			<div class="lIinner">
				<a href="{$parentURL}" title="Read more about {JobTitle/node()}" class="list-image-link job-image">
					<xsl:apply-templates select="." mode="displayThumbnail">
						<xsl:with-param name="crop" select="$cropSetting" />
						<xsl:with-param name="class">list-image</xsl:with-param>
					</xsl:apply-templates>
				</a>
				<h3 class="title">
					<a href="{$parentURL}" title="{JobTitle/node()}">
						<xsl:value-of select="JobTitle/node()"/>
					</a>
				</h3>
				<div class="job-intro">
					<dl class="clearfix dl-horizontal">
						<xsl:if test="@publish and @publish!=''">
							<dt class="job-date">
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
				<div class="job-summary">
					<xsl:if test="Summary/node()!=''">
						<xsl:apply-templates select="Summary/node()" mode="cleanXhtml"/>
					</xsl:if>
				</div>
				<div class="entryFooter">
					<xsl:apply-templates select="." mode="displayTags"/>
					<xsl:apply-templates select="." mode="moreLink">
						<xsl:with-param name="link" select="$parentURL"/>
						<xsl:with-param name="stretchLink" select="$linked"/>
						<xsl:with-param name="altText">
							<xsl:value-of select="JobTitle/node()"/>
						</xsl:with-param>
					</xsl:apply-templates>
					<xsl:text> </xsl:text>
				</div>
			</div>
		</div>
	</xsl:template>


	<!-- Job Detail -->
	<xsl:template match="Content[@type='Job']" mode="ContentDetail">
		<xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
		<div class="detail job" itemscope="" itemtype="http://schema.org/JobPosting" >
			<meta itemprop="specialCommitments" content="" />
			<xsl:apply-templates select="." mode="inlinePopupOptions">
				<xsl:with-param name="class" select="'detail job'"/>
			</xsl:apply-templates>
			<div class="row">
				<div class="col-lg-5 col-xl-4">
					<h1 class="detail-title d-lg-none" itemprop="title">
						<xsl:value-of select="JobTitle/node()"/>
					</h1>
					<xsl:apply-templates select="." mode="displayDetailImage">
						<xsl:with-param name="class">
							detail-img
						</xsl:with-param>
					</xsl:apply-templates>
					<div class="card job-card">
						<div class="card-body">
							<dl class="dl-horizontal">
								<xsl:if test="@publish and @publish!=''">
									<dt class="job-date" itemprop="datePosted">
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
					</div>
				</div>
				<div class="col-lg-7 col-xl-8">
					<h1 class="detail-title d-none d-lg-block" itemprop="title">
						<xsl:value-of select="JobTitle/node()"/>
					</h1>
					<xsl:if test="Description/node()">
						<h2 class="Jobdescription">
							<!--Description-->
							<xsl:call-template name="term2092" />
						</h2>
						<div itemprop="description">
							<xsl:apply-templates select="Description/node()" mode="cleanXhtml"/>
						</div>
					</xsl:if>
					<xsl:if test="Responsibities/node()">
						<h2 class="Responsibities">
							<!--Responsibilities-->
							<xsl:call-template name="term2084" />
						</h2>
						<div itemprop="responsibilities">
							<xsl:apply-templates select="Responsibities/node()" mode="cleanXhtml"/>
						</div>
					</xsl:if>
					<xsl:if test="Skills/node()">
						<h2 class="Skills">
							<xsl:call-template name="term2094" />
						</h2>
						<div itemprop="skills" >
							<xsl:apply-templates select="Skills/node()" mode="cleanXhtml"/>
						</div>
					</xsl:if>
					<xsl:if test="EducationRequirements/node()">
						<h2 class="EducationRequirements">
							<xsl:call-template name="term2095" />
						</h2>
						<div itemprop="educationRequirements">
							<xsl:apply-templates select="EducationRequirements/node()" mode="cleanXhtml"/>
						</div>
					</xsl:if>
					<xsl:if test="ExperienceRequirements/node()">
						<h2 class="ExperienceRequirements">
							<xsl:call-template name="term2096" />
						</h2>
						<div itemprop="experienceRequirements">
							<xsl:apply-templates select="ExperienceRequirements/node()" mode="cleanXhtml"/>
						</div>
					</xsl:if>
					<xsl:if test="Qualifications/node()">
						<h2 class="Qualifications">
							<xsl:call-template name="term2097" />
						</h2>
						<div itemprop="qualifications">
							<xsl:apply-templates select="Qualifications/node()" mode="cleanXhtml"/>
						</div>
					</xsl:if>
					<xsl:if test="Incentives/node()">
						<h2 class="Incentives">
							<xsl:call-template name="term2098" />
						</h2>
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
			</div>
		</div>
	</xsl:template>


</xsl:stylesheet>
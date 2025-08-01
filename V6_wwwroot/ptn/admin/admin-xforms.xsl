<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                  xmlns:v-bind="http://example.com/xml/v-bind" xmlns:v-on="http://example.com/xml/v-on"
                  xmlns:v-for="http://example.com/xml/v-for" xmlns:v-slot="http://example.com/xml/v-slot"
                  xmlns:v-if="http://example.com/xml/v-if" xmlns:v-else="http://example.com/xml/v-else"
                  xmlns:v-model="http://example.com/xml/v-model" xmlns:ew="urn:ew">


	<!-- Default template for all admin forms-->
	<xsl:template match="Content[ancestor::Page[@adminMode='true']] | div[@class='xform' and ancestor::Page[@adminMode='true']]" mode="xform">
		<form method="{model/submission/@method}" action="">
			<xsl:attribute name="class">
				<xsl:text>xform container</xsl:text>
				<xsl:if test="model/submission/@class!=''">
					<xsl:text> </xsl:text>
					<xsl:value-of select="model/submission/@class"/>
				</xsl:if>
			</xsl:attribute>
			<xsl:if test="not(contains(model/submission/@action,'.asmx'))">
				<xsl:attribute name="action">
					<xsl:value-of select="model/submission/@action"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="model/submission/@id!=''">
				<xsl:attribute name="id">
					<xsl:value-of select="model/submission/@id"/>
				</xsl:attribute>
				<xsl:attribute name="name">
					<xsl:value-of select="model/submission/@id"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="model/submission/@event!=''">
				<xsl:attribute name="onsubmit">
					<xsl:value-of select="model/submission/@event"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="descendant::upload">
				<xsl:attribute name="enctype">multipart/form-data</xsl:attribute>
			</xsl:if>
			<xsl:choose>
				<xsl:when test="count(group) = 2 and group[2]/submit and count(group[2]/*[name()!='submit']) = 0">
					<xsl:for-each select="group[1]">
						<!--<xsl:if test="label[position()=1]">
              <div class="">
                <h3 class="x">
                  <xsl:copy-of select="label/node()"/>
                </h3>
              </div>
            </xsl:if>-->
						<div class="">

							<xsl:apply-templates select="." mode="xform"/>
							<!--xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | hint | help | alert | div" mode="xform"/-->
						</div>
					</xsl:for-each>
					<xsl:for-each select="group[2]">
						<xsl:if test="count(submit) &gt; 0">
							<div class="navbar-fixed-bottom">
								<div class="container">
									<!--<xsl:if test="ancestor-or-self::Content/group/descendant-or-self::*[contains(@class,'required')]">
                    <span class="required">
                      <span class="req">*</span>
                      <xsl:text> </xsl:text>
                      <xsl:call-template name="msg_required"/>
                    </span>
                  </xsl:if>-->
									<xsl:apply-templates select="submit" mode="xform"/>
									<div class="footer-status">
										<span>
											<xsl:if test="not($page/ContentDetail/Content/model/instance/*/nStatus='1')">
												<xsl:attribute name="class">text-muted hidden</xsl:attribute>
											</xsl:if>
											<i class="fas fa-eye">
												<xsl:text> </xsl:text>
											</i> Live
										</span>
										<span>
											<xsl:if test="not($page/ContentDetail/Content/model/instance/*/nStatus='0')">
												<xsl:attribute name="class">text-muted hidden</xsl:attribute>
											</xsl:if>
											<i class="fas fa-eye-slash">
												<xsl:text> </xsl:text>
											</i> Hidden
										</span>
									</div>
								</div>
							</div>
						</xsl:if>
					</xsl:for-each>
				</xsl:when>
				<xsl:otherwise>
					<xsl:if test="label[position()=1]">
						<!--div class="">
              <h3 class="">
                <xsl:copy-of select="label/node()"/>
              </h3>
            </div-->
					</xsl:if>
					<div class="">
						<xsl:apply-templates select="group | repeat " mode="xform"/>
						<xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | hint | help | alert | div" mode="xform"/>
					</div>
					<xsl:if test="count(submit) &gt; 0">
						<div class="clearfix navbar-fixed-bottom">
							<!--<xsl:if test="ancestor-or-self::Content/group/descendant-or-self::*[contains(@class,'required')]">
                -->
							<!--<xsl:if test="descendant-or-self::*[contains(@class,'required')]">-->
							<!--
                <span class="required">
                  <xsl:call-template name="msg_required"/>
                  <span class="req">*</span>
                </span>
              </xsl:if>-->
							<xsl:apply-templates select="submit" mode="xform"/>
							<!--<div class="clearfix">&#160;</div>-->
						</div>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>

		</form>
		<xsl:apply-templates select="descendant-or-self::*" mode="xform_modal"/>
	</xsl:template>


	<xsl:template match="group[@ref='EditContent' and parent::Content]" mode="xform">
		<xsl:param name="class"/>
		<div class="{@class}">
			<xsl:if test=" @id!='' ">
				<xsl:attribute name="id">
					<xsl:value-of select="@id"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | group | repeat | hint | help | alert | div | repeat | relatedContent | label[position()!=1] | trigger | script" mode="control-outer"/>
			<xsl:if test="count(submit) &gt; 0">
				<xsl:if test="not(submit[contains(@class,'hideRequired')])">
					<xsl:if test="ancestor::group/descendant-or-self::*[contains(@class,'required')]">
						<label class="required required-message">
							<span class="req">*</span>
							<xsl:text> </xsl:text>
							<xsl:call-template name="msg_required"/>
						</label>
					</xsl:if>
				</xsl:if>
				<!-- For xFormQuiz change how these buttons work -->
				<xsl:apply-templates select="submit" mode="xform"/>
			</xsl:if>
		</div>
	</xsl:template>

	<!-- Default template for admin forms with a single group at the top level-->
	<xsl:template match="Content[ancestor::Page[@adminMode='true'] and count(group) = 1] | div[@class='xform' and count(group) = 1 and ancestor::Page[@adminMode='true']]" mode="xform">
		<form method="{model/submission/@method}" action=""  novalidate="novalidate">
			<xsl:attribute name="class">
				<xsl:text>xform needs-validation container form-single-group</xsl:text>
				<xsl:if test="model/submission/@class!=''">
					<xsl:text> </xsl:text>
					<xsl:value-of select="model/submission/@class"/>
				</xsl:if>
			</xsl:attribute>
			<xsl:if test="not(contains(model/submission/@action,'.asmx'))">
				<xsl:attribute name="action">
					<xsl:value-of select="model/submission/@action"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="model/submission/@id!=''">
				<xsl:attribute name="id">
					<xsl:value-of select="model/submission/@id"/>
				</xsl:attribute>
				<xsl:attribute name="name">
					<xsl:value-of select="model/submission/@id"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="model/submission/@event!=''">
				<xsl:attribute name="onsubmit">
					<xsl:value-of select="model/submission/@event"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="descendant::upload">
				<xsl:attribute name="enctype">multipart/form-data</xsl:attribute>
			</xsl:if>
			<!--<xsl:if test="group/label[position()=1]">
        <div>
          <h3>
            <xsl:apply-templates select="group/label" mode="legend"/>
          </h3>
        </div>
      </xsl:if>-->
			<xsl:for-each select="group">
				<div class="admin-body {@class}">
					<xsl:choose>
						<xsl:when test="contains(@class,'2col') or contains(@class,'2Col') ">
							<div class="row">
								<xsl:for-each select="group | repeat">
									<xsl:apply-templates select="." mode="xform">
										<xsl:with-param name="class">
											<xsl:text>col-md-</xsl:text>
											<xsl:choose>
												<xsl:when test="position()='1'">4</xsl:when>
												<xsl:when test="position()='2'">8</xsl:when>
											</xsl:choose>
										</xsl:with-param>
									</xsl:apply-templates>
								</xsl:for-each>
							</div>
						</xsl:when>
						<xsl:when test="contains(@class,'2col5050') or contains(@class,'2Col5050') ">
							<div class="row">
								<xsl:for-each select="group | repeat">
									<xsl:apply-templates select="." mode="xform">
										<xsl:with-param name="class">
											<xsl:text>col-md-</xsl:text>
											<xsl:choose>
												<xsl:when test="position()='1'">6</xsl:when>
												<xsl:when test="position()='2'">6</xsl:when>
											</xsl:choose>
										</xsl:with-param>
									</xsl:apply-templates>
								</xsl:for-each>
							</div>
						</xsl:when>
						<xsl:when test="contains(@class,'3col') or contains(@class,'3Col') ">
							<div class="row">
								<xsl:for-each select="group | repeat">
									<xsl:apply-templates select="." mode="xform">
										<xsl:with-param name="class">
											<xsl:text>col-lg-4</xsl:text>
										</xsl:with-param>
									</xsl:apply-templates>
								</xsl:for-each>
							</div>
						</xsl:when>
						<!--<xsl:when test="contains(@class,'accordion-form-container') ">
              -->
						<!--<div id="collapseOne" class="accordion-collapse collapse show" aria-labelledby="headingOne" data-bs-parent="#accordionExample">-->
						<!--
                <xsl:for-each select="group | repeat">
                  <xsl:apply-templates select="." mode="xform">
                    <xsl:with-param name="class">
                      <xsl:text>accordion-collapse collapse show</xsl:text>
                    </xsl:with-param>
                  </xsl:apply-templates>
                </xsl:for-each>
              
            </xsl:when>-->
						<xsl:otherwise>
							<xsl:apply-templates select="group | repeat " mode="xform"/>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:apply-templates select="parent::*/alert" mode="xform"/>
					<xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | hint | help | alert | div" mode="control-outer"/>
				</div>
				<xsl:if test="count(submit) &gt; 0">
					<div class="clearfix navbar-fixed-bottom">
						<div class="container">
							<xsl:if test="ancestor-or-self::group/descendant-or-self::*[contains(@class,'required')]">
								<!--<xsl:if test="descendant-or-self::*[contains(@class,'required')]">-->
								<span class="required">
									<span class="req">*</span>
									<xsl:text> </xsl:text>
									<xsl:call-template name="msg_required"/>
								</span>
							</xsl:if>
							<xsl:apply-templates select="submit" mode="xform"/>
							<!--<div class="clearfix">&#160;</div>-->
						</div>
					</div>
				</xsl:if>
			</xsl:for-each>
		</form>
		<xsl:apply-templates select="descendant-or-self::*" mode="xform_modal"/>
	</xsl:template>


	<!-- Template for login, pick page-->
	<xsl:template match="Content[@name='EditPageLayout' or @name='FindRelatedContent' or @name='FindContentToRelate']" mode="xform">
		<form method="{model/submission/@method}" action=""  novalidate="novalidate">
			<xsl:attribute name="class">
				<xsl:text>xform needs-validation</xsl:text>
				<xsl:if test="model/submission/@class!=''">
					<xsl:text> </xsl:text>
					<xsl:value-of select="model/submission/@class"/>
				</xsl:if>
			</xsl:attribute>
			<xsl:if test="not(contains(model/submission/@action,'.asmx'))">
				<xsl:attribute name="action">
					<xsl:value-of select="model/submission/@action"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="model/submission/@id!=''">
				<xsl:attribute name="id">
					<xsl:value-of select="model/submission/@id"/>
				</xsl:attribute>
				<xsl:attribute name="name">
					<xsl:value-of select="model/submission/@id"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="model/submission/@event!=''">
				<xsl:attribute name="onsubmit">
					<xsl:value-of select="model/submission/@event"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="descendant::upload">
				<xsl:attribute name="enctype">multipart/form-data</xsl:attribute>
			</xsl:if>
			<!--<xsl:if test="group/label[position()=1]">
        <div>
          <h3>
            <xsl:apply-templates select="group/label" mode="legend"/>
          </h3>
        </div>
      </xsl:if>-->
			<xsl:for-each select="group">
				<div class="admin-body {@class}">
					<xsl:choose>
						<xsl:when test="contains(@class,'2col') or contains(@class,'2Col') ">
							<div class="row">
								<xsl:for-each select="group | repeat">
									<xsl:apply-templates select="." mode="xform">
										<xsl:with-param name="class">
											<xsl:text>col-md-</xsl:text>
											<xsl:choose>
												<xsl:when test="position()='1'">4</xsl:when>
												<xsl:when test="position()='2'">8</xsl:when>
											</xsl:choose>
										</xsl:with-param>
									</xsl:apply-templates>
								</xsl:for-each>
							</div>
						</xsl:when>
						<xsl:when test="contains(@class,'2col5050') or contains(@class,'2Col5050') ">
							<div class="row">
								<xsl:for-each select="group | repeat">
									<xsl:apply-templates select="." mode="xform">
										<xsl:with-param name="class">
											<xsl:text>col-md-</xsl:text>
											<xsl:choose>
												<xsl:when test="position()='1'">6</xsl:when>
												<xsl:when test="position()='2'">6</xsl:when>
											</xsl:choose>
										</xsl:with-param>
									</xsl:apply-templates>
								</xsl:for-each>
							</div>
						</xsl:when>
						<xsl:when test="contains(@class,'3col') or contains(@class,'3Col') ">
							<div class="row">
								<xsl:for-each select="group | repeat">
									<xsl:apply-templates select="." mode="xform">
										<xsl:with-param name="class">
											<xsl:text>col-lg-4</xsl:text>
										</xsl:with-param>
									</xsl:apply-templates>
								</xsl:for-each>
							</div>
						</xsl:when>
						<!--<xsl:when test="contains(@class,'accordion-form-container') ">
              -->
						<!--<div id="collapseOne" class="accordion-collapse collapse show" aria-labelledby="headingOne" data-bs-parent="#accordionExample">-->
						<!--
                <xsl:for-each select="group | repeat">
                  <xsl:apply-templates select="." mode="xform">
                    <xsl:with-param name="class">
                      <xsl:text>accordion-collapse collapse show</xsl:text>
                    </xsl:with-param>
                  </xsl:apply-templates>
                </xsl:for-each>
              
            </xsl:when>-->
						<xsl:otherwise>
							<xsl:apply-templates select="group | repeat " mode="xform"/>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:apply-templates select="parent::*/alert" mode="xform"/>
					<xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | hint | help | alert | div" mode="control-outer"/>
				</div>
				<xsl:if test="count(submit) &gt; 0">
					<div class="clearfix d-grid gap-2">
						<xsl:if test="ancestor-or-self::group/descendant-or-self::*[contains(@class,'required')]">
							<!--<xsl:if test="descendant-or-self::*[contains(@class,'required')]">-->
							<span class="required">
								<span class="req">*</span>
								<xsl:text> </xsl:text>
								<xsl:call-template name="msg_required"/>
							</span>
						</xsl:if>
						<xsl:apply-templates select="submit" mode="xform"/>
						<!--<div class="clearfix">&#160;</div>-->
					</div>
				</xsl:if>
			</xsl:for-each>
		</form>
		<xsl:apply-templates select="descendant-or-self::*" mode="xform_modal"/>
	</xsl:template>

	<!-- Template for login, pick page-->
	<xsl:template match="Content[@name='UserLogon']" mode="xform">
		<form method="{model/submission/@method}" action=""  novalidate="novalidate">
			<xsl:attribute name="class">
				<xsl:text>xform needs-validation</xsl:text>
				<xsl:if test="model/submission/@class!=''">
					<xsl:text> </xsl:text>
					<xsl:value-of select="model/submission/@class"/>
				</xsl:if>
			</xsl:attribute>
			<xsl:if test="not(contains(model/submission/@action,'.asmx'))">
				<xsl:attribute name="action">
					<xsl:value-of select="model/submission/@action"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="model/submission/@id!=''">
				<xsl:attribute name="id">
					<xsl:value-of select="model/submission/@id"/>
				</xsl:attribute>
				<xsl:attribute name="name">
					<xsl:value-of select="model/submission/@id"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="model/submission/@event!=''">
				<xsl:attribute name="onsubmit">
					<xsl:value-of select="model/submission/@event"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="descendant::upload">
				<xsl:attribute name="enctype">multipart/form-data</xsl:attribute>
			</xsl:if>
			<xsl:for-each select="group">
				<div class="admin-body {@class}">
					<xsl:apply-templates select="label" mode="legend"/>
					<p>Welcome back, please sign in to your account</p>
					<xsl:choose>
						<xsl:when test="contains(@class,'2col') or contains(@class,'2Col') ">
							<div class="row">
								<xsl:for-each select="group | repeat">
									<xsl:apply-templates select="." mode="xform">
										<xsl:with-param name="class">
											<xsl:text>col-md-</xsl:text>
											<xsl:choose>
												<xsl:when test="position()='1'">4</xsl:when>
												<xsl:when test="position()='2'">8</xsl:when>
											</xsl:choose>
										</xsl:with-param>
									</xsl:apply-templates>
								</xsl:for-each>
							</div>
						</xsl:when>
						<xsl:when test="contains(@class,'2col5050') or contains(@class,'2Col5050') ">
							<div class="row">
								<xsl:for-each select="group | repeat">
									<xsl:apply-templates select="." mode="xform">
										<xsl:with-param name="class">
											<xsl:text>col-md-</xsl:text>
											<xsl:choose>
												<xsl:when test="position()='1'">6</xsl:when>
												<xsl:when test="position()='2'">6</xsl:when>
											</xsl:choose>
										</xsl:with-param>
									</xsl:apply-templates>
								</xsl:for-each>
							</div>
						</xsl:when>
						<xsl:when test="contains(@class,'3col') or contains(@class,'3Col') ">
							<div class="row">
								<xsl:for-each select="group | repeat">
									<xsl:apply-templates select="." mode="xform">
										<xsl:with-param name="class">
											<xsl:text>col-lg-4</xsl:text>
										</xsl:with-param>
									</xsl:apply-templates>
								</xsl:for-each>
							</div>
						</xsl:when>
					
						<xsl:otherwise>
						<div>
							<xsl:apply-templates select="group | repeat " mode="xform"/>
						</div>
						</xsl:otherwise>
					</xsl:choose>	
					<xsl:apply-templates select="parent::*/alert" mode="xform"/>						
					<xsl:apply-templates select="legend | input | secret | select | select1 | range | textarea | upload | hint | help | alert | div | submit" mode="control-outer"/>
				
				</div>
				
			</xsl:for-each>
		</form>
		<xsl:apply-templates select="descendant-or-self::*" mode="xform_modal"/>
	</xsl:template>



	<!-- Template for login, pick page-->
	<xsl:template match="Content" mode="xform-card">
		<form method="{model/submission/@method}" action=""  novalidate="novalidate">
			<xsl:attribute name="class">
				<xsl:text>xform needs-validation card card-default</xsl:text>
				<xsl:if test="model/submission/@class!=''">
					<xsl:text> </xsl:text>
					<xsl:value-of select="model/submission/@class"/>
				</xsl:if>
			</xsl:attribute>
			<xsl:if test="not(contains(model/submission/@action,'.asmx'))">
				<xsl:attribute name="action">
					<xsl:value-of select="model/submission/@action"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="model/submission/@id!=''">
				<xsl:attribute name="id">
					<xsl:value-of select="model/submission/@id"/>
				</xsl:attribute>
				<xsl:attribute name="name">
					<xsl:value-of select="model/submission/@id"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="model/submission/@event!=''">
				<xsl:attribute name="onsubmit">
					<xsl:value-of select="model/submission/@event"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="descendant::upload">
				<xsl:attribute name="enctype">multipart/form-data</xsl:attribute>
			</xsl:if>
			<!--<xsl:if test="group/label[position()=1]">
        <div>
          <h3>
            <xsl:apply-templates select="group/label" mode="legend"/>
          </h3>
        </div>
      </xsl:if>-->
			<xsl:for-each select="group">
				<div class="card-body {@class}">
					<xsl:choose>
						<xsl:when test="contains(@class,'2col') or contains(@class,'2Col') ">
							<div class="row">
								<xsl:for-each select="group | repeat">
									<xsl:apply-templates select="." mode="xform">
										<xsl:with-param name="class">
											<xsl:text>col-md-</xsl:text>
											<xsl:choose>
												<xsl:when test="position()='1'">4</xsl:when>
												<xsl:when test="position()='2'">8</xsl:when>
											</xsl:choose>
										</xsl:with-param>
									</xsl:apply-templates>
								</xsl:for-each>
							</div>
						</xsl:when>
						<xsl:when test="contains(@class,'2col5050') or contains(@class,'2Col5050') ">
							<div class="row">
								<xsl:for-each select="group | repeat">
									<xsl:apply-templates select="." mode="xform">
										<xsl:with-param name="class">
											<xsl:text>col-md-</xsl:text>
											<xsl:choose>
												<xsl:when test="position()='1'">6</xsl:when>
												<xsl:when test="position()='2'">6</xsl:when>
											</xsl:choose>
										</xsl:with-param>
									</xsl:apply-templates>
								</xsl:for-each>
							</div>
						</xsl:when>
						<xsl:when test="contains(@class,'3col') or contains(@class,'3Col') ">
							<div class="row">
								<xsl:for-each select="group | repeat">
									<xsl:apply-templates select="." mode="xform">
										<xsl:with-param name="class">
											<xsl:text>col-lg-4</xsl:text>
										</xsl:with-param>
									</xsl:apply-templates>
								</xsl:for-each>
							</div>
						</xsl:when>
						<!--<xsl:when test="contains(@class,'accordion-form-container') ">
              -->
						<!--<div id="collapseOne" class="accordion-collapse collapse show" aria-labelledby="headingOne" data-bs-parent="#accordionExample">-->
						<!--
                <xsl:for-each select="group | repeat">
                  <xsl:apply-templates select="." mode="xform">
                    <xsl:with-param name="class">
                      <xsl:text>accordion-collapse collapse show</xsl:text>
                    </xsl:with-param>
                  </xsl:apply-templates>
                </xsl:for-each>
              
            </xsl:when>-->
						<xsl:otherwise>
							<xsl:apply-templates select="group | repeat " mode="xform"/>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:apply-templates select="parent::*/alert" mode="xform"/>
					<xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | hint | help | alert | div" mode="control-outer"/>
				</div>
				<xsl:if test="count(submit) &gt; 0">
					<div class="card-footer">
						<xsl:if test="ancestor-or-self::group/descendant-or-self::*[contains(@class,'required')]">
							<!--<xsl:if test="descendant-or-self::*[contains(@class,'required')]">-->
							<span class="required">
								<span class="req">*</span>
								<xsl:text> </xsl:text>
								<xsl:call-template name="msg_required"/>
							</span>
						</xsl:if>
						<xsl:apply-templates select="submit" mode="xform"/>
						<!--<div class="clearfix">&#160;</div>-->
					</div>
				</xsl:if>
			</xsl:for-each>
		</form>
		<xsl:apply-templates select="descendant-or-self::*" mode="xform_modal"/>
	</xsl:template>



	<xsl:template match="group[(contains(@class,'2col') or contains(@class,'2Col')) and ancestor::Page[@adminMode='true']]" mode="xform">
		<xsl:if test="label and not(parent::Content)">
			<div class="panel-heading">
				<h3 class="panel-title">
					<xsl:copy-of select="label/node()"/>
				</h3>
			</div>
		</xsl:if>
		<fieldset>
			<xsl:if test="label and ancestor::group">
				<div class="row">
					<legend class="col-md-12">
						<xsl:copy-of select="label/node()"/>
					</legend>
				</div>
			</xsl:if>
			<div class="row">
				<xsl:for-each select="group">
					<xsl:apply-templates select="." mode="xform">
						<xsl:with-param name="class">
							<xsl:text>col-md-</xsl:text>
							<xsl:choose>
								<xsl:when test="position()='1'">4</xsl:when>
								<xsl:when test="position()='2'">8</xsl:when>
							</xsl:choose>
						</xsl:with-param>
					</xsl:apply-templates>
				</xsl:for-each>
			</div>
		</fieldset>
	</xsl:template>

	<xsl:template match="group[(contains(@class,'accordion-form-container'))]" mode="xform">

		<div id="collapseOne" class="accordion-collapse collapse show" aria-labelledby="headingOne" data-bs-parent="#accordionExample">
			<div class="accordion-body">
				<xsl:for-each select="group">
					<xsl:apply-templates select="." mode="xform">
						<xsl:with-param name="class">
							<xsl:text> </xsl:text>

						</xsl:with-param>
					</xsl:apply-templates>
				</xsl:for-each>
			</div>
		</div>
	</xsl:template>


	<!-- -->
	<!-- ========================== CONTROL : PickImage ========================== -->
	<!-- -->
	<xsl:template match="input[contains(@class,'pickImage')]" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<xsl:variable name="niceImg">
			<xsl:apply-templates select="value/img" mode="jsNiceImage"/>
		</xsl:variable>
		<xsl:apply-templates select="self::node()[not(item[toggle])]" mode="xform_legend"/>
		<div>
			<xsl:attribute name="class">pick-image-wrapper</xsl:attribute>
			<div class="previewImage" id="previewImage_{$ref}">
				<span>
					<xsl:choose>
						<xsl:when test="value/img/@src!=''">
							<!--<xsl:value-of select="value/img"/>-->
							<xsl:apply-templates select="value/img" mode="jsNiceImageForm"/>
							<xsl:text> </xsl:text>
						</xsl:when>
						<xsl:otherwise>
							<i class="fas fa-image fa-xxl">&#160;</i>
						</xsl:otherwise>
					</xsl:choose>
				</span>
			</div>
			<div class="form-margin" id="editImage_{$ref}">
				<textarea name="{$ref}" id="{$ref}" readonly="readonly" class="pick-image-textarea">
					<xsl:attribute name="class">
						<xsl:text>form-control pickImageInput pick-image-textarea </xsl:text>
						<xsl:value-of select="@class"/>
					</xsl:attribute>
					<xsl:text></xsl:text>
					<xsl:apply-templates select="value/img" mode="jsNiceImage"/>
					<xsl:text> </xsl:text>
				</textarea>

				<div class="btn-group-spaced">

					<a href="#" onclick="xfrmClearImage('{ancestor::Content/model/submission/@id}','{$ref}','{value/*/@class}');return false" title="edit an image from the image library" class="btn btn-sm btn-danger clearImage">
						<xsl:if test="not(value/img/@src!='')">
							<xsl:attribute name="style">display:none</xsl:attribute>
						</xsl:if>
						<i class="fa fa-times">
							<xsl:text> </xsl:text>
						</i> Remove
					</a>

					<!--<a href="#" onclick="OpenWindow_edit_{$ref}('');return false;" title="edit an image from the image library" class="btn btn-primary">  href="?contentType=popup&amp;ewCmd=ImageLib&amp;targetForm={ancestor::Content/model/submission/@id}&amp;ewCmd2=editImage&amp;imgHtml=' + imgtag + '&amp;targetField={$ref}"-->
					<a class="btn btn-sm btn-primary editImage" data-bs-toggle="modal" data-bs-target="#modal-{$ref}">
						<xsl:if test="not(value/img/@src!='')">
							<xsl:attribute name="style">display:none</xsl:attribute>
						</xsl:if>
						<i class="fas fa-pen">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Edit
					</a>

					<!--<a href="#" onclick="OpenWindow_pick_{$ref}();return false;" title="pick an image from the image library" class="btn btn-primary">-->
					<a data-bs-toggle="modal" href="?contentType=popup&amp;ewCmd=ImageLib&amp;targetForm={ancestor::Content/model/submission/@id}&amp;targetField={$ref}&amp;targetClass={value/*/@class}&amp;fld={@targetFolder}" data-bs-target="#modal-{$ref}" class="btn btn-sm btn-primary pickImage">
						<xsl:if test="value/img/@src!=''">
							<xsl:attribute name="style">display:none</xsl:attribute>
						</xsl:if>
						<i class="fas fa-image">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Pick Image
					</a>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="*" mode="xform_modal">
		<!--do nothing-->
	</xsl:template>

	<xsl:template match="input[contains(@class,'pickImage') or contains(@class,'pickMedia') or contains(@class,'pickDocument')]" mode="xform_modal">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBindForScript"/>
		</xsl:variable>
		<div id="modal-{$ref}" class="modal fade pickImageModal">
			<div class="modal-dialog" id="test">
				<div class="modal-content">
					<div class="modal-body">
						<p class="text-center">
							<h4>
								<i class="fa fa-cog fa-spin fa-2x fa-fw">
									<xsl:text> </xsl:text>
								</i> Loading ...
							</h4>
						</p>
					</div>
				</div>
			</div>
		</div>
	</xsl:template>

	<!-- -->
	<!-- ========================== CONTROL : PickDocument ========================== -->
	<!-- -->
	<xsl:template match="input[contains(@class,'pickDocument')]" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<xsl:variable name="scriptRef">
			<xsl:apply-templates select="." mode="getRefOrBindForScript"/>
		</xsl:variable>
		<div class="input-group" id="editDoc_{$ref}">
			<input name="{$ref}" id="{$ref}" value="{value/node()}">
				<xsl:attribute name="class">
					<xsl:text>form-control </xsl:text>
					<xsl:value-of select="@class"/>
				</xsl:attribute>
			</input>
			<xsl:choose>
				<xsl:when test="value!=''">
					<a href="#" onclick="xfrmClearDocument('{ancestor::Content/model/submission/@id}','{$scriptRef}');return false" title="Clear the document path" class="btn btn-danger">
						<i class="fa fa-times fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Clear
					</a>

				</xsl:when>
				<xsl:otherwise>
					<a data-bs-toggle="modal" href="?contentType=popup&amp;ewCmd=DocsLib&amp;targetForm={ancestor::Content/model/submission/@id}&amp;targetField={$scriptRef}&amp;targetClass={value/*/@class}" data-bs-target="#modal-{$scriptRef}" class="btn btn-primary">
						<i class="fas fa-file">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Pick
					</a>
				</xsl:otherwise>
			</xsl:choose>
		</div>

	</xsl:template>

	<!-- -->
	<!-- ========================== CONTROL : PickMedia ========================== -->
	<!-- -->
	<xsl:template match="input[contains(@class,'pickMedia')]" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<xsl:variable name="scriptRef">
			<xsl:apply-templates select="." mode="getRefOrBindForScript"/>
		</xsl:variable>
		<div class="input-group" id="editDoc_{$ref}">
			<input name="{$ref}" id="{$ref}" value="{value/node()}">
				<xsl:attribute name="class">
					<xsl:text>form-control </xsl:text>
					<xsl:value-of select="@class"/>
				</xsl:attribute>
			</input>
			<xsl:choose>
				<xsl:when test="value!=''">
					<a href="#" onclick="xfrmClearMedia('{ancestor::Content/model/submission/@id}','{$scriptRef}');return false" title="Clear the document path" class="btn btn-danger">
						<i class="fa fa-times fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Clear
					</a>

				</xsl:when>
				<xsl:otherwise>
					<a data-bs-toggle="modal" href="?contentType=popup&amp;ewCmd=MediaLib&amp;targetForm={ancestor::Content/model/submission/@id}&amp;targetField={$scriptRef}&amp;targetClass={value/*/@class}" data-bs-target="#modal-{$scriptRef}" class="btn btn-primary">
						<i class="fa fa-music fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Pick
					</a>
				</xsl:otherwise>
			</xsl:choose>
		</div>
	</xsl:template>

	<xsl:template match="textarea[contains(@class,'xhtml')]" mode="xform_modal">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBindForScript"/>
		</xsl:variable>
		<div id="modal-{$ref}" class="modal fade pickImageModal">
			<div class="modal-dialog" id="test">
				<div class="modal-content">
					<div class="modal-body">
						<p class="text-center">
							<h4>
								<i class="fa fa-cog fa-spin fa-2x fa-fw">
									<xsl:text> </xsl:text>
								</i> Loading ...
							</h4>
						</p>
					</div>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="img" mode="jsNiceImageForm">
		<xsl:variable name="imgUrl">
			<xsl:call-template name="resize-image">
				<xsl:with-param name="path" select="@src"/>
				<xsl:with-param name="max-width" select="'85'"/>
				<xsl:with-param name="max-height" select="'85'"/>
				<xsl:with-param name="file-prefix" select="'~ew/tn-'"/>
				<xsl:with-param name="file-suffix" select="''"/>
				<xsl:with-param name="quality" select="'100'"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="imgWidth">
			<xsl:call-template name="get-image-width">
				<xsl:with-param name="path">
					<xsl:value-of select="$imgUrl"/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="imgHeight">
			<xsl:call-template name="get-image-height">
				<xsl:with-param name="path">
					<xsl:value-of select="$imgUrl"/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="valt">
			<xsl:call-template name="escape-js">
				<xsl:with-param name="string">
					<xsl:value-of select="@alt"/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:variable>
		<img src="{$imgUrl}" width="{$imgWidth}" height="{$imgHeight} " class="{@class}" alt="{$valt}"/>
	</xsl:template>

	<!-- -->
	<!-- ========================== CONTROL : PickImageFile ========================== -->
	<!-- -->
	<xsl:template match="input[contains(@class,'pickImageFile')]" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<xsl:variable name="scriptRef">
			<xsl:apply-templates select="." mode="getRefOrBindForScript"/>
		</xsl:variable>
		<div class="input-group form-margin" id="editImageFile_{$ref}">
			<input name="{$ref}" id="{$ref}" value="{value/node()}" >
				<xsl:attribute name="class">
					<xsl:text>form-control </xsl:text>
					<xsl:value-of select="@class"/>
				</xsl:attribute>
			</input>
			<xsl:choose>
				<xsl:when test="value!=''">
					<a href="#" onclick="xfrmClearImgFile('{ancestor::Content/model/submission/@id}','{$scriptRef}');return false" title="Clear the file path" class="btn btn-danger">
						<i class="fa fa-times fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Clear
					</a>

				</xsl:when>
				<xsl:otherwise>
					<a data-bs-toggle="modal" href="?contentType=popup&amp;ewCmd=ImageLib&amp;ewCmd2=PathOnly&amp;targetForm={ancestor::Content/model/submission/@id}&amp;targetField={$scriptRef}&amp;targetClass={value/*/@class}" data-bs-target="#modal-{$scriptRef}" class="btn btn-primary">
						<i class="fas fa-image">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Pick
					</a>
				</xsl:otherwise>
			</xsl:choose>
		</div>

	</xsl:template>
	<!-- -->




	<!-- -->
	<!-- ========================== CONTROL : Edit X Form ========================== -->
	<!-- -->
	<xsl:template match="input[contains(@class,'editXformButton')]" mode="xform_control">
		<!--input type="hidden" name="xml" value="x"/-->
		<input type="submit" name="submit" value="Edit Questions" class="adminButton"/>
		<!--a href="?ewCmd=EditXForm&amp;artid={/Page/Request/QueryString/Item[@name='id']/node()}" class="textButton">Click Here to Edit this Form</a-->
	</xsl:template>


	<!-- ========================== CONTROL : Google Product Taxonimy ========================== -->
	<!-- -->
	<xsl:template match="input[contains(@class,'googleProductCategories')]" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<xsl:variable name="formName">
			<xsl:value-of select="ancestor::Content/model/submission/@id"/>
		</xsl:variable>
		<input type="text" name="{$ref}-id" id="{$ref}-id" value="{value/node()}" class="googleProductCategories">

		</input>
		<input type="text" name="{$ref}-value" id="googleProductCategories-value" value="{value/node()}">

		</input>

		<!--input type="text" name="{$ref}" id="googleProductCategories-result" value="{value/node()}">
    
    </input-->
		<!--script type="text/javascript" src="/ewcommon/js/jQuery/jquery-option-tree/jquery.optionTree.js">&#160;</script-->
	</xsl:template>

	<xsl:template match="Content[@type='xform']" mode="tinyMCEtinyMCEinit">
		<!-- tinyMCE - Now handled by jquery in commonV4_2.js -->
	</xsl:template>


	<!-- TinyMCE configuration templates -->
	<xsl:template match="textarea" mode="tinymceGeneralOptions">
		<xsl:text>script_url: '/ptn/libs/tinymce/tinymce.min.js',
			mode: "exact",
			theme: "silver",
			width: "auto",
			relative_urls: false,
			plugins: "table paste link image ptnimage media visualchars searchreplace emoticons anchor lists advlist code visualblocks contextmenu fullscreen searchreplace wordcount charmap",
			entity_enconding: "numeric",
            image_advtab: true,
            menubar: "edit insert view format table tools",
            toolbar: "insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image | ptnimage",
			convert_fonts_to_spans: true,
			gecko_spellcheck: true,
			theme_advanced_toolbar_location: "top",
			theme_advanced_toolbar_align: "left",
			paste_create_paragraphs: false,
            link_list: tinymcelinklist,
			paste_use_dialog: true,</xsl:text>
		<xsl:apply-templates select="." mode="tinymceStyles"/>
		<xsl:apply-templates select="." mode="tinymceContentCSS"/>
		<xsl:text>
			auto_cleanup_word: "true"</xsl:text>
	</xsl:template>

	<xsl:template match="textarea" mode="tinymcelinklist">

		<xsl:text>
     [</xsl:text>
		<xsl:apply-templates select="/Page/Menu/MenuItem" mode="tinymcelinklistitem">
			<xsl:with-param name="level" select="number(1)"/>
		</xsl:apply-templates>
		<xsl:text>]</xsl:text>

	</xsl:template>

	<xsl:template match="MenuItem" mode="tinymcelinklistitem">
		<xsl:param name="level"/>

		<xsl:text>{title: '</xsl:text>
		<xsl:call-template name="writeSpacers">
			<xsl:with-param name="spacers" select="$level"/>
		</xsl:call-template>
		<xsl:text>&#160;</xsl:text>
		<xsl:call-template name="escape-js">
			<xsl:with-param name="string">
				<xsl:value-of select="@name"/>
			</xsl:with-param>
		</xsl:call-template>
		<xsl:text>', value: '</xsl:text>
		<xsl:choose>
			<xsl:when test="contains(@url,'?pgid')">
				<xsl:value-of select="substring-before(@url,'?pgid')"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="@url"/>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:text>'}</xsl:text>

		<xsl:if test="count(child::MenuItem[substring-before(@url,'?pgid')!=''])&gt;0">
			<xsl:text>,
	</xsl:text>
			<xsl:if test="MenuItem[substring-before(@url,'?pgid')!='']">
				<xsl:apply-templates select="MenuItem[substring-before(@url,'?pgid')!='']" mode="tinymcelinklistitem">
					<xsl:with-param name="level" select="number($level+1)"/>
				</xsl:apply-templates>
			</xsl:if>
		</xsl:if>
		<xsl:if test="position()!=last()">
			<xsl:text>,
	</xsl:text>
		</xsl:if>
	</xsl:template>

	<!-- TinyMCE styles  - leave empty, overwrite as needed per site Example Follows www.tinymce.com/tryit/custom_formats.php -->
	<!--
  style_formats: [
  {title: 'Bold text', inline: 'b'},
  {title: 'Red text', inline: 'span', styles: {color: '#ff0000'}},
  {title: 'Red header', block: 'h1', styles: {color: '#ff0000'}},
  {title: 'Example 1', inline: 'span', classes: 'example1'},
  {title: 'Example 2', inline: 'span', classes: 'example2'},
  {title: 'Table styles'},
  {title: 'Table row 1', selector: 'tr', classes: 'tablerow1'}
  ],
  -->


	<xsl:template match="textarea[ancestor::Page[Settings/add[@key='theme.BespokeTextClasses']/@value!='']]" mode="tinymceStyles">
		<xsl:variable name="styles">
			<styles>
				<xsl:call-template name="split-string">
					<xsl:with-param name="list" select="/Page/Settings/add[@key='theme.BespokeTextClasses']/@value" />
					<xsl:with-param name="seperator" select="','" />
				</xsl:call-template>
			</styles>
		</xsl:variable>
		style_formats: [
		{title: 'Headers', items: [
		{title: 'Header 1', format: 'h1'},
		{title: 'Header 2', format: 'h2'},
		{title: 'Header 3', format: 'h3'},
		{title: 'Header 4', format: 'h4'},
		{title: 'Header 5', format: 'h5'},
		{title: 'Header 6', format: 'h6'}
		]},
		{title: 'Inline', items: [
		{title: 'Bold', icon: 'bold', format: 'bold'},
		{title: 'Italic', icon: 'italic', format: 'italic'},
		{title: 'Underline', icon: 'underline', format: 'underline'},
		{title: 'Strikethrough', icon: 'strikethrough', format: 'strikethrough'},
		{title: 'Superscript', icon: 'superscript', format: 'superscript'},
		{title: 'Subscript', icon: 'subscript', format: 'subscript'},
		{title: 'Code', icon: 'code', format: 'code'}
		]},
		{title: 'Blocks', items: [
		{title: 'Paragraph', format: 'p'},
		{title: 'Blockquote', format: 'blockquote'},
		{title: 'Div', format: 'div'},
		{title: 'Pre', format: 'pre'}
		]},
		{title: 'Alignment', items: [
		{title: 'Left', icon: 'alignleft', format: 'alignleft'},
		{title: 'Center', icon: 'aligncenter', format: 'aligncenter'},
		{title: 'Right', icon: 'alignright', format: 'alignright'},
		{title: 'Justify', icon: 'alignjustify', format: 'alignjustify'}
		]},
		<xsl:for-each select="ms:node-set($styles)/*/*">
			{title: '<xsl:value-of select="node()"/>', inline:'span', classes: '<xsl:value-of select="node()"/>'},
		</xsl:for-each>
		],
	</xsl:template>

	<xsl:template match="textarea" mode="tinymceContentCSS">

		content_css:"/ptn/admin/skin/tinymce.css",</xsl:template>

	<!-- TinyMCE default configuration -->
	<xsl:template match="textarea" mode="tinymceButtons1">
		<xsl:text>cut,copy,paste,pastetext,pasteword,selectall,search,replace,separator,undo,</xsl:text>
		<xsl:text>redo,separator,removeformat,cleanup,separator,</xsl:text>
		<xsl:text>formatselect,</xsl:text>
	</xsl:template>

	<xsl:template match="textarea" mode="tinymceButtons2">
		<xsl:text>tablecontrols,separator,link,unlink,image,youtube,media,visualchars,separator,code,help</xsl:text>
	</xsl:template>

	<xsl:template match="textarea" mode="tinymceButtons3">
		<xsl:text>bold,italic,underline,strikethrough,separator,justifyleft,justifycenter,justifyright,justifyfull,</xsl:text>
		<xsl:text>separator,bullist,numlist,outdent,indent,hr,|,charmap,emotions,cite</xsl:text>
	</xsl:template>

	<!-- TinyMCE styles (e.g. "Style A=styleA;") - leave empty, overwrite as needed per site -->
	<xsl:template match="textarea" mode="tinymceStyles"></xsl:template>

	<xsl:template match="textarea" mode="tinymceValidElements">
		"a[href|target|title|style|class|onmouseover|onmouseout|onclick|id|name],"
		+ "img[class|src|border=0|alt|title|hspace|vspace|width|height|align|onmouseover|onmouseout|name|style],"
		+ "table[cellspacing|cellpadding|border|height|width|style|class],"
		+ "p[align|style|class],"
		+ "span[class],"
		+ "form[action|method|name|style|class],object[*],param[*],embed[*],"
		+ "input[type|value|name|style|class|src|alt|border|size],"
		+ "textarea[type|value|name|style|class|src|alt|border|size|cols|rows],"
		+ "td/th[colspan|rowspan|align|valign|style|class],"
		+ "h1[style|class],h2[style|class],h3[style|class],h4[style|class],h5[style|class],h6[style|class],"
		+ "ol[style|class],ul[style|class],li[style|class],div[align|style|class|id],span[style|class|id],"
		+ "thead[style|class],tbody[style|class],tr[class],dd[style|class],dl[style|class],dt[style|class],"
		+ "sup,sub,pre,address,strong,b,em,i[class],u,s,hr,blockquote,br,"
		+ "cite[class|id|title],code[class|title],samp,iframe[width|height|src|frameborder|allowfullscreen]"
	</xsl:template>

	<!-- TinyMCE minimal configuration -->
	<xsl:template match="textarea[contains(@class, 'minxhtml')]" mode="tinymceButtons1">
		<xsl:text>bold,italic,underline,bullist,numlist</xsl:text>
	</xsl:template>

	<xsl:template match="textarea[contains(@class, 'minxhtml')]" mode="tinymceButtons2"></xsl:template>

	<xsl:template match="textarea[contains(@class, 'minxhtml')]" mode="tinymceButtons3"></xsl:template>

	<xsl:template match="textarea[contains(@class, 'minxhtml')]" mode="tinymceValidElements">
		<xsl:text>"a[href|target|title|style|class|onmouseover|onmouseout|onclick],"
			+ "p[align|style|class],"
			+ "span[class],"
			+ "h2[style|class],h3[style|class],h4[style|class],h5[style|class],h6[style|class],ol[style|class],"
			+ "ul[style|class],li[style|class],div[align|style|class],span[style|class],"
			+ "dd[style|class],dl[style|class],dt[style|class],sup,sub,pre,address,strong,b,em,i,u,s,hr,blockquote,br"
		</xsl:text>
	</xsl:template>

	<!-- TinyMCE configuration -->
	<xsl:template match="textarea" mode="xform_control_script">
		<script type="text/javascript">
			$('#<xsl:apply-templates select="." mode="getRefOrBind"/>').tinymce({
			<xsl:apply-templates select="." mode="tinymceGeneralOptions"/>,
			theme_modern_buttons1: "<xsl:apply-templates select="." mode="tinymceButtons1"/>",
			theme_modern_buttons2: "<xsl:apply-templates select="." mode="tinymceButtons2"/>",
			theme_modern_buttons3: "<xsl:apply-templates select="." mode="tinymceButtons3"/>",
			theme_modern_blockformats : "p,h1,h2,h3,h4,h5,h6,blockquote,div,dt,dd,code,samp",
			valid_elements: <xsl:apply-templates select="." mode="tinymceValidElements"/>
			});
		</script>
	</xsl:template>


	<!-- Tiny MCE Control -->
	<xsl:template match="textarea[contains(@class,'xhtml')]" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<textarea name="{$ref}" id="{$ref}">
			<xsl:if test="@cols!=''">
				<xsl:attribute name="cols">
					<xsl:value-of select="@cols"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="@rows!=''">
				<xsl:attribute name="rows">
					<xsl:value-of select="@rows"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="@class!=''">
				<xsl:attribute name="class">
					<xsl:value-of select="@class"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="value/node()" mode="cleanXhtml"/>
			<xsl:text> </xsl:text>
		</textarea>
		<!--xsl:apply-templates select="." mode="tinymceConfig"/-->
	</xsl:template>

	<!-- YouTube Video module embed field -->
	<xsl:template match="textarea[contains(@class,'xhtml') and contains(@class,'youtube')]" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<textarea name="{$ref}" id="{$ref}">
			<xsl:if test="@cols!=''">
				<xsl:attribute name="cols">
					<xsl:value-of select="@cols"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="@rows!=''">
				<xsl:attribute name="rows">
					<xsl:value-of select="@rows"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="@class!=''">
				<xsl:attribute name="class">
					<xsl:value-of select="@class"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:copy-of select="value/node()"/>
			<xsl:text> </xsl:text>
		</textarea>
	</xsl:template>
	<!-- End of YouTube Video module embed field -->

	<!-- Minimal Tiny MCE Control For Front end editing - Simple formatting controls -->
	<xsl:template match="textarea[contains(@class,'minxhtml')]" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<textarea name="{$ref}" id="{$ref}">
			<xsl:if test="@cols!=''">
				<xsl:attribute name="cols">
					<xsl:value-of select="@cols"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="@rows!=''">
				<xsl:attribute name="rows">
					<xsl:value-of select="@rows"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="@class!=''">
				<xsl:attribute name="class">
					<xsl:value-of select="@class"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:copy-of select="value/node()"/>
			<xsl:text> </xsl:text>
		</textarea>
		<xsl:apply-templates select="." mode="tinymceConfig"/>
	</xsl:template>




	<!-- CodeMirror Control -->
	<xsl:template match="textarea[contains(@class,'xml')]" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<textarea name="{$ref}" id="{$ref}">
			<xsl:if test="@cols!=''">
				<xsl:attribute name="cols">
					<xsl:value-of select="@cols"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="@rows!=''">
				<xsl:attribute name="rows">
					<xsl:value-of select="@rows"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="@class!=''">
				<xsl:attribute name="class">
					<xsl:value-of select="@class"/>
				</xsl:attribute>
			</xsl:if>
			<!--xsl:apply-templates select="value/TemplateContent/node()" mode="cleanXhtml"/-->
			<xsl:copy-of select="value/node()"/>
			<xsl:text> </xsl:text>
		</textarea>
	</xsl:template>

	<xsl:template match="textarea[contains(@class,'xml')]" mode="xform_control_script">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<script type="text/javascript">
			var editor = CodeMirror.fromTextArea('<xsl:value-of select="$ref"/>', {
			height: "<xsl:value-of select="number(@rows) * 25"/>px",
			parserfile: "parsexml.js",
			stylesheet: "/ptn/libs/codemirror/css/xmlcolors.css",
			path: "/ptn/libs/codemirror/",
			continuousScanning: 500,
			lineNumbers: true,
			reindentOnLoad: true,
			textWrapping: true,
			matchClosing: true
			});
		</script>
	</xsl:template>

	<!-- CodeMirror Control -->
	<xsl:template match="textarea[contains(@class,'code-mirror')]" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<textarea name="{$ref}" id="{$ref}">
			<xsl:if test="@cols!=''">
				<xsl:attribute name="cols">
					<xsl:value-of select="@cols"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="@rows!=''">
				<xsl:attribute name="rows">
					<xsl:value-of select="@rows"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="@class!=''">
				<xsl:attribute name="class">
					<xsl:value-of select="@class"/>
				</xsl:attribute>
			</xsl:if>
			<!--xsl:apply-templates select="value/TemplateContent/node()" mode="cleanXhtml"/-->
			<xsl:copy-of select="value/node()"/>
			<xsl:text> </xsl:text>
		</textarea>
	</xsl:template>

	<xsl:template match="textarea[contains(@class,'code-mirror')]" mode="xform_control_script">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<script src="/ptn/libs/codemirror/lib/codemirror.js">&#160;</script>
		<link rel="stylesheet" href="/ptn/libs/codemirror/lib/codemirror.css"/>

		<script src="/ptn/libs/codemirror/addon/edit/closetag.js">&#160;</script>

		<script src="/ptn/libs/codemirror/addon/fold/xml-fold.js">&#160;</script>

		<script src="/ptn/libs/codemirror/mode/xml/xml.js">&#160;</script>
		<script src="/ptn/libs/codemirror/addon/mode/multiplex.js">&#160;</script>
		<script src="/ptn/libs/codemirror/mode/htmlembedded/htmlembedded.js">&#160;</script>

		<script type="text/javascript">
			var editor = CodeMirror.fromTextArea(document.getElementById("<xsl:value-of select="$ref"/>"), {
			mode: 'text/html',
			autoCloseTags: true,
			height: "<xsl:value-of select="number(@rows) * 25"/>px",
			parserfile: "parsexml.js",
			stylesheet: "/ptn/libs/codemirror/theme/shadowfox.css",
			path: "/ptn/libs/codemirror/",
			continuousScanning: 500,
			lineNumbers: true,
			reindentOnLoad: true,
			textWrapping: true,
			matchClosing: true
			});
		</script>
	</xsl:template>

	<!-- CodeMirror Control -->
	<xsl:template match="textarea[contains(@class,'xFormEditor')]" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<textarea name="{$ref}" id="{$ref}">
			<xsl:if test="@cols!=''">
				<xsl:attribute name="cols">
					<xsl:value-of select="@cols"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="@rows!=''">
				<xsl:attribute name="rows">
					<xsl:value-of select="@rows"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="@class!=''">
				<xsl:attribute name="class">
					<xsl:value-of select="@class"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:copy-of select="value/node()"/>
			<xsl:text> </xsl:text>
		</textarea>
		<script type="text/javascript">
			$('#<xsl:value-of select="$ref"/>').xFormEditor();
		</script>
	</xsl:template>

	<!--Uploader-->

	<xsl:template match="Content[descendant::upload[@class='MultiPowUpload']] | div[@class='xform' and descendant::upload[@class='MultiPowUpload']]" mode="xform">
		<div id="fileupload">
			<!--<form action="/ewcommon/tools/FileTransferHandler.ashx?storageRoot={descendant::input[@bind='fld']/value/node()}" method="post" enctype="multipart/form-data" class="ewXform">-->

			<xsl:apply-templates select="group | repeat | input | secret | select | select1 | range | textarea | upload | hint | help | alert | div" mode="xform"/>

			<div class="terminus">&#160;</div>
			<!--</form>-->
		</div>
	</xsl:template>

	<xsl:template match="group[descendant::upload[@class='MultiPowUpload']]" mode="xform">
		<xsl:param name="class"/>

		<fieldset>
			<xsl:if test="$class!='' or @class!='' ">
				<xsl:attribute name="class">
					<xsl:value-of select="$class"/>
					<xsl:if test="@class!=''">
						<xsl:text> </xsl:text>
						<xsl:value-of select="@class"/>
					</xsl:if>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="label">
				<xsl:apply-templates select="label[position()=1]" mode="legend"/>
			</xsl:if>
			<ol>
				<xsl:for-each select="input | secret | select | select1 | range | textarea | upload | group | repeat | hint | help | alert | div | repeat | relatedContent | label[position()!=1] | trigger | script">
					<xsl:choose>
						<xsl:when test="name()='group'">
							<li>
								<xsl:apply-templates select="." mode="xform"/>
							</li>
						</xsl:when>
						<xsl:otherwise>
							<xsl:apply-templates select="." mode="xform"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:for-each>
			</ol>
		</fieldset>
	</xsl:template>

	<xsl:template match="upload[@class='MultiPowUpload']" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<div id="uploadFiles">
			<xsl:choose>
				<xsl:when test="contains($browserVersion,'Firefox') or contains($browserVersion,'Chrome')">
					<div class="drophere">Drag and drop files here to upload them</div>
					<label class="label">Alternatively, pick files</label>
				</xsl:when>
				<xsl:when test="contains($browserVersion,'MSIE') or contains($browserVersion,'')">
					<div class="hint">Note: You can upload multiple files without needing to refresh the page</div>
					<label class="label">Pick file</label>
				</xsl:when>
			</xsl:choose>

			<!--input type="hidden" name="path" /-->
			<!-- The fileinput-button span is used to style the file input field as button -->
			<span class="btn btn-primary fileinput-button">
				<i class="fa fa-plus fa-white">
					<xsl:text> </xsl:text>
				</i>
				<span>Select files...</span>
				<!-- The file input field used as target for the file upload widget -->
				<input id="fileupload" type="file" name="files[]" multiple=""/>
			</span>
			<span class="fileupload-loading">
				<xsl:text> </xsl:text>
			</span>
		</div>
		<div id="progress" class="progress progress-success progress-striped">
			<div class="bar">
				<xsl:text> </xsl:text>
			</div>
		</div>
		<!-- The table listing the files available for upload/download -->
		<div id="files">
			<xsl:text> </xsl:text>
		</div>
		<table role="presentation" class="table table-striped">
			<tbody class="files" data-bs-toggle="modal-gallery" data-bs-target="#modal-gallery">
				<xsl:text> </xsl:text>
			</tbody>
		</table>


		<script id="template-upload" type="text/x-jquery-tmpl">
			<tr class="template-upload{{if error}} ui-state-error{{/if}}">
				<td class="name">${name}</td>
				<td colspan="2">&#160;</td>
				<td class="size">${sizef}</td>
				{{if error}}
				<td class="error" colspan="2">
					!! Error:
					{{if error === 'maxFileSize'}}File is too big
					{{else error === 'minFileSize'}}File is too small
					{{else error === 'acceptFileTypes'}}Filetype not allowed
					{{else error === 'maxNumberOfFiles'}}Max number of files exceeded
					{{else}}${error}
					{{/if}}
				</td>
				{{else}}
				<td class="progress">
					<div>
						<xsl:text> </xsl:text>
					</div>
				</td>
				<td class="start">
					<button>Start</button>
				</td>
				{{/if}}
			</tr>
		</script>
		<script id="template-download" type="text/x-jquery-tmpl">
			<tr class="template-download{{if error}} ui-state-error{{/if}}">
				{{if error}}
				<td></td>
				<td class="name">${name}</td>
				<td colspan="2">&#160;</td>
				<td class="size">${sizef}</td>
				<td class="error" colspan="2">
					!! Error:
					{{if error === 1}}File exceeds upload_max_filesize (php.ini directive)
					{{else error === 2}}File exceeds MAX_FILE_SIZE (HTML form directive)
					{{else error === 3}}File was only partially uploaded
					{{else error === 4}}No File was uploaded
					{{else error === 5}}Missing a temporary folder
					{{else error === 6}}Failed to write file to disk
					{{else error === 7}}File upload stopped by extension
					{{else error === 'maxFileSize'}}File is too big
					{{else error === 'minFileSize'}}File is too small
					{{else error === 'acceptFileTypes'}}Filetype not allowed
					{{else error === 'maxNumberOfFiles'}}Max number of files exceeded
					{{else error === 'uploadedBytes'}}Uploaded bytes exceed file size
					{{else error === 'emptyResult'}}Empty file upload result
					{{else}}${error}
					{{/if}}
				</td>
				{{else}}
				<td class="name">${name}</td>
				<td colspan="2">&#160;</td>
				<td class="size">${sizef}</td>
				{{/if}}
			</tr>
		</script>



		<!-- The Iframe Transport is required for browsers without support for XHR file uploads -->
		<script src="/ewcommon/js/jQuery/fileUploader/8.2.1/js/jquery.iframe-transport.js"></script>
		<!-- The basic File Upload plugin -->
		<script src="/ewcommon/js/jQuery/fileUploader/8.2.1/js/jquery.fileupload.js"></script>


		<script>
			<xsl:text>
      $('#fileupload').fileupload({
      url: '/?ewCmd=</xsl:text><xsl:value-of select="$page/@ewCmd"/><xsl:text>&amp;ewCmd2=FileUpload&amp;storageRoot=</xsl:text><xsl:value-of select="parent::group/input[@bind='fld']/value/node()"/>',
			dataType: 'json',
			sequentialUploads: true,
			dropZone:$('#uploadFiles'),
			always: function (e, data) {
			$.each(data.files, function (index, file) {
			$('<p/>').text(file.name).appendTo('#files');
			});
			},
			progressall: function (e, data) {
			var progress = parseInt(data.loaded / data.total * 100, 10);
			$('#progress .bar').css(
			'width',
			progress + '%'
			);
			}
			});
		</script>

	</xsl:template>

	<!-- In Admin - Allow for default SITE box styles -->
	<xsl:template match="select1[@appearance='minimal' and contains(@class,'boxStyle')][ancestor::Page[@adminMode='true']]" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<div class="bfh-selectbox boxStyle" data-name="{$ref}" data-value="{value/node()}">

			<xsl:apply-templates select="item" mode="xform_BoxStyles"/>

			<xsl:apply-templates select="." mode="siteBoxStyles">
				<xsl:with-param name="value" select="value/node()" />
			</xsl:apply-templates>

			<xsl:apply-templates select="." mode="bootstrapBoxStyles">
				<xsl:with-param name="value" select="value/node()" />
			</xsl:apply-templates>

		</div>
	</xsl:template>

	<xsl:template match="select1[@appearance='minimal' and contains(@class,'boxStyle')][ancestor::Page[@adminMode='true' and (@ewCmd='AddMailModule' or @ewCmd='EditMailContent' )]]" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<div class="bfh-selectbox boxStyle" data-name="{$ref}" data-value="{value/node()}">
			<xsl:apply-templates select="item" mode="xform_BoxStyles"/>
			<xsl:apply-templates select="." mode="mailBoxStyles">
				<xsl:with-param name="value" select="value/node()" />
			</xsl:apply-templates>
		</div>
	</xsl:template>


	<!-- -->
	<xsl:template match="/" mode="siteBoxStyles">
		<xsl:param name="value" />

		<option value="bespokeBox">
			<xsl:if test="$value='bespokeBox'">
				<xsl:attribute name="selected">selected</xsl:attribute>
			</xsl:if>
			<xsl:text>site's bespoke box</xsl:text>
		</option>

	</xsl:template>

	<xsl:template match="*" mode="siteBoxStyles">
		<xsl:param name="value" />
		<!-- EXAMPLE BESPOKE BOX-->
		<!--<div data-value="panel-primary">
      <div class="panel panel-bespoke">
        <div class="panel-heading">Bespoke Box Style</div>
        <div class="panel-body">
          <xsl:text> </xsl:text>
        </div>
      </div>
    </div>-->
	</xsl:template>

	<xsl:template match="*[ancestor::Page[Settings/add[@key='theme.BespokeBoxStyles']/@value!='']]" mode="siteBoxStyles">
		<xsl:param name="value" />
		<xsl:variable name="styles">
			<styles>
				<xsl:call-template name="split-string">
					<xsl:with-param name="list" select="/Page/Settings/add[@key='theme.BespokeBoxStyles']/@value" />
					<xsl:with-param name="seperator" select="','" />
				</xsl:call-template>
			</styles>
		</xsl:variable>

		<xsl:for-each select="ms:node-set($styles)/*/*">
			<!-- EXAMPLE BESPOKE BOX-->
			<div data-value="{node()}">
				<div class="{node()}">
					<xsl:value-of select="node()"/>
				</div>
			</div>
		</xsl:for-each>

	</xsl:template>

	<!-- -->
	<!--<xsl:template match="*" mode="bootstrapBoxStyles">
    <xsl:param name="value" />
    <div data-value="bg-primary">
      <div class="card bg-primary">
        <div class="card-header">
          <h5 class="card-title">card primary</h5>
        </div>
        <div class="card-body">
          <xsl:text>Example Text </xsl:text>
        </div>
      </div>
    </div>
    <div data-value="bg-secondary">
      <div class="card bg-secondary">
        <div class="card-header">
          <h5 class="card-title">card secondary</h5>
        </div>
        <div class="card-body">
          <xsl:text>Example Text </xsl:text>
        </div>
      </div>
    </div>
    <div data-value="bg-info">
      <div class="card bg-info">
        <div class="card-header">
          <h5 class="card-title">card info</h5>
        </div>
        <div class="card-body">
          <xsl:text>Example Text </xsl:text>
        </div>
      </div>
    </div>
    <div data-value="bg-light">
      <div class="card bg-light">
        <div class="card-header">
          <h5 class="card-title">card light</h5>
        </div>
        <div class="card-body">
          <xsl:text>Example Text </xsl:text>
        </div>
      </div>
    </div>
    <div data-value="bg-dark">
      <div class="card bg-dark">
        <div class="card-header">
          <h5 class="card-title">card dark</h5>
        </div>
        <div class="card-body">
          <xsl:text>Example Text </xsl:text>
        </div>
      </div>
    </div>
    <div data-value="border-primary">
      <div class="card border-primary">
        <div class="card-header">
          <h5 class="card-title">card border primary</h5>
        </div>
        <div class="card-body">
          <xsl:text>Example Text </xsl:text>
        </div>
      </div>
    </div>
    <div data-value="border-secondary">
      <div class="card border-secondary">
        <div class="card-header">
          <h5 class="card-title">card border secondary</h5>
        </div>
        <div class="card-body">
          <xsl:text>Example Text </xsl:text>
        </div>
      </div>
    </div>
    <div data-value="border-info">
      <div class="card border-info">
        <div class="card-header">
          <h5 class="card-title">card border info</h5>
        </div>
        <div class="card-body">
          <xsl:text>Example Text </xsl:text>
        </div>
      </div>
    </div>
    <div data-value="border-light">
      <div class="card border-light">
        <div class="card-header">
          <h5 class="card-title">card border light</h5>
        </div>
        <div class="card-body">
          <xsl:text>Example Text </xsl:text>
        </div>
      </div>
    </div>
    <div data-value="border-dark">
      <div class="card border-dark">
        <div class="card-header">
          <h5 class="card-title">card border dark</h5>
        </div>
        <div class="card-body">
          <xsl:text>Example Text </xsl:text>
        </div>
      </div>
    </div>
    <div data-value="card-info">
      <div class="card bg-info">
        <div class="card-header">
          <h5 class="card-title">card info</h5>
        </div>
        <div class="card-body">
          <xsl:text>Example Text </xsl:text>
        </div>
      </div>
    </div>
    <div data-value="alert-primary">
      <div class="alert alert-primary">
        alert primary
      </div>
    </div>
    <div data-value="alert-secondary">
      <div class="alert alert-secondary">
        alert secondary
      </div>
    </div>
    <div data-value="alert-info">
      <div class="alert alert-info">
        alert info
      </div>
    </div>
    <div data-value="alert-light">
      <div class="alert alert-light">
        alert light
      </div>
    </div>
    <div data-value="alert-dark">
      <div class="alert alert-dark">
        alert dark
      </div>
    </div>
    <div data-value="alert-success">
      <div class="alert alert-success">
        alert success
      </div>
    </div>
    <div data-value="alert-warning">
      <div class="alert alert-warning">
        alert warning
      </div>
    </div>
    <div data-value="alert-danger">
      <div class="alert alert-danger">
        alert danger
      </div>
    </div>
  </xsl:template>-->

	<xsl:template match="*" mode="bootstrapBoxStyles">
		<xsl:param name="value" />
		<div class="box-style-item" data-value="bg-primary">
			card primary
		</div>
		<div class="box-style-item" data-value="bg-secondary">
			card secondary
		</div>
		<div class="box-style-item" data-value="bg-info">
			card info
		</div>
		<div class="box-style-item" data-value="bg-light">
			card light
		</div>
		<div class="box-style-item" data-value="bg-dark">
			card dark
		</div>
		<div class="box-style-item" data-value="border-primary">
			card border primary
		</div>
		<div class="box-style-item" data-value="border-secondary">
			card border secondary
		</div>
		<div class="box-style-item" data-value="border-info">
			card border info
		</div>
		<div class="box-style-item" data-value="border-light">
			card border light
		</div>
		<div class="box-style-item" data-value="border-dark">
			card border dark
		</div>
		<div class="box-style-item" data-value="alert-primary">
			alert primary
		</div>
		<div class="box-style-item" data-value="alert-secondary">
			alert secondary
		</div>
		<div class="box-style-item" data-value="alert-info">
			alert info
		</div>
		<div class="box-style-item" data-value="alert-light">
			alert light
		</div>
		<div class="box-style-item" data-value="alert-dark">
			alert dark
		</div>
		<div class="box-style-item" data-value="alert-success">
			alert success
		</div>
		<div class="box-style-item" data-value="alert-warning">
			alert warning
		</div>
		<div class="box-style-item" data-value="alert-danger">
			alert danger"
		</div>
	</xsl:template>

	<!-- -->
	<xsl:template match="/" mode="mailBoxStyles">
		<xsl:param name="value" />

		<option value="bespokeBox">
			<xsl:if test="$value='bespokeBox'">
				<xsl:attribute name="selected">selected</xsl:attribute>
			</xsl:if>
			<xsl:text>site's bespoke box</xsl:text>
		</option>

	</xsl:template>

	<xsl:template match="*" mode="mailBoxStyles">
		<xsl:param name="value" />

		<option value="bespokeBox">
			<xsl:if test="$value='bespokeBox'">
				<xsl:attribute name="selected">selected</xsl:attribute>
			</xsl:if>
			<xsl:text>site's bespoke box</xsl:text>
		</option>

	</xsl:template>



	<!-- -->
	<xsl:template match="select1[@class='siteTree']" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<xsl:variable name="ref2">
			<xsl:apply-templates select="." mode="getRefOrBindForScript"/>
		</xsl:variable>
		<xsl:variable name="selectedValue">
			<xsl:value-of select="value/node()"/>
		</xsl:variable>
		<xsl:variable name="selectedName">
			<xsl:value-of select="/Page/Menu/descendant-or-self::MenuItem[@id=$selectedValue]/@name"/>
		</xsl:variable>
		<div class="pick-page">
			<input type="hidden" class="form-control" placeholder="select page" name="{$ref}" id="{$ref}" value="{$selectedValue}" test="{$ref}"/>
			<div class="input-group">
				<span class="input-group-btn">
					<!--<a onclick="xfrmClearPickPage('{ancestor::Content/model/submission/@id}','{$ref}')" title="remove page" class="btn btn-default">-->
					<a href="javascript:$('#{$ref}').val('');$('#{$ref}-name').val('');" title="remove page" class="btn btn-outline-primary">
						<i class="fa fa-times fa-white pe-0">
							<xsl:text> </xsl:text>
						</i>
					</a>
				</span>
				<input type="text" class="form-control" placeholder="select page" readonly="readonly" name="{$ref}-name"  value="{$selectedName}" id="{$ref}-name"/>

				<button type="button" class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#{$ref2}-modal">
					<i class="fa fa-file-alt fa-white">
						<xsl:text> </xsl:text>
					</i><xsl:text> </xsl:text>Pick Page
				</button>

			</div>
			<div class="modal fade" id="{$ref2}-modal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel">
				<div class="modal-dialog" role="document">
					<div class="modal-content">
						<div class="modal-body">
							<ul id="MenuTree" class="list-group">
								<xsl:apply-templates select="/Page/Menu/MenuItem" mode="siteTreePage">
									<xsl:with-param name="level">1</xsl:with-param>
									<xsl:with-param name="ref" select="$ref2" />
									<xsl:with-param name="selectedValue" select="$selectedValue" />
								</xsl:apply-templates>
								<xsl:apply-templates select="/Page/Menu/MenuItem/MenuItem[DisplayName/@siteTemplate='micro']" mode="siteTreePage">
									<xsl:with-param name="level">1</xsl:with-param>
									<xsl:with-param name="ref" select="$ref2" />
									<xsl:with-param name="selectedValue" select="$selectedValue" />
								</xsl:apply-templates>
							</ul>
						</div>
					</div>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="MenuItem" mode="siteTreePage">
		<xsl:param name="level"/>
		<xsl:param name="ref"/>
		<xsl:param name="selectedValue"/>
		<xsl:variable name="oldpgid">
			<xsl:choose>
				<xsl:when test="/Page/Request/QueryString/Item[@name='oldPgId']/node()!=''">
					<xsl:value-of select="/Page/Request/QueryString/Item[@name='oldPgId']/node()"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="/Page/@id"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="class">
			<xsl:if test="MenuItem"> expandable</xsl:if>
		</xsl:variable>
		<xsl:if test="not($level=2 and DisplayName/@siteTemplate='micro')">
			<li id="node{@id}" data-tree-level="{$level}">
				<xsl:attribute name="data-tree-parent">
					<xsl:if test="not(DisplayName/@siteTemplate='micro' or @level='1')">
						<xsl:value-of select="./parent::MenuItem/@id"/>
					</xsl:if>
				</xsl:attribute>
				<xsl:attribute name="class">
					<xsl:text>list-group-item level-</xsl:text>
					<xsl:value-of select="$level"/>
					<xsl:text> </xsl:text>
					<xsl:value-of select="$class"/>
					<xsl:if test="@id = $selectedValue">
						<xsl:text> selected</xsl:text>
					</xsl:if>
				</xsl:attribute>
				<div class="pageCell">
					<xsl:choose>
						<xsl:when test="DisplayName/@siteTemplate='micro' or parent::Menu">
							<i class="fa fa-home fa-lg status activeParent">
								&#160;
							</i>

						</xsl:when>
						<xsl:otherwise>
							<xsl:apply-templates select="." mode="status_legend"/>
						</xsl:otherwise>
					</xsl:choose>
					<a href="javascript:$('#{$ref}').val('{@id}');$('#{$ref}-name').val('{@name}');$('#{$ref}-modal .selected').removeClass('selected');$('#{$ref}-modal #node{@id}').addClass('selected');$('#{$ref}-modal').modal('hide');">
						<span class="pageName">
							&#160;
							<xsl:value-of select="@name"/>
						</span>
					</a>
				</div>
			</li>
			<xsl:apply-templates select="MenuItem" mode="siteTreePage">
				<xsl:with-param name="level" select="$level + 1"/>
				<xsl:with-param name="ref" select="$ref"/>
				<xsl:with-param name="selectedValue" select="$selectedValue"/>
			</xsl:apply-templates>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template match="select1[@class='siteTreeName']" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<xsl:variable name="selectedValue">
			<xsl:value-of select="value/node()"/>
		</xsl:variable>
		<select name="{$ref}" id="{$ref}">
			<xsl:attribute name="class">
				<xsl:text>dropdown form-control </xsl:text>
				<xsl:if test="@class!=''">
					<xsl:value-of select="@class"/>
				</xsl:if>
			</xsl:attribute>
			<xsl:if test="@onChange!=''">
				<xsl:attribute name="onChange">
					<xsl:value-of select="@onChange"/>
				</xsl:attribute>
			</xsl:if>
			<option value="">
				<xsl:variable name="label_low" select="translate(label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
				<xsl:text>Not Specified</xsl:text>
			</option>
			<xsl:apply-templates select="/Page/Menu/MenuItem" mode="listPageOptionName">
				<xsl:with-param name="level" select="number(1)"/>
				<xsl:with-param name="selectedValue" select="$selectedValue"/>
			</xsl:apply-templates>
			<!--<xsl:apply-templates select="item" mode="xform_select"/>-->
		</select>
	</xsl:template>



	<xsl:template match="MenuItem" mode="listPageOptionName">
		<xsl:param name="level"/>
		<xsl:param name="selectedValue"/>
		<option>
			<xsl:attribute name="value">
				<xsl:value-of select="@name"/>
			</xsl:attribute>
			<xsl:if test="$selectedValue=@name">
				<xsl:attribute name="selected">selected</xsl:attribute>
			</xsl:if>
			<xsl:call-template name="writeSpacers">
				<xsl:with-param name="spacers" select="$level"/>
			</xsl:call-template>
			<xsl:text>|-&#160;</xsl:text>
			<xsl:value-of select="@name"/>
			<xsl:text> </xsl:text>
		</option>
		<xsl:if test="count(child::MenuItem)&gt;0">
			<xsl:apply-templates select="MenuItem" mode="listPageOptionName">
				<xsl:with-param name="level" select="number($level+1)"/>
				<xsl:with-param name="selectedValue" select="$selectedValue"/>
			</xsl:apply-templates>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<!--BJR SITE TREE BY REF-->
	<xsl:template match="select1[@class='siteTreeRef']" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<xsl:variable name="selectedValue">
			<xsl:value-of select="value/node()"/>
		</xsl:variable>
		<select name="{$ref}" id="{$ref}" class="dropdown form-control">
			<xsl:if test="@class!=''">
				<xsl:attribute name="class">
					<xsl:value-of select="@class"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="@onChange!=''">
				<xsl:attribute name="onChange">
					<xsl:value-of select="@onChange"/>
				</xsl:attribute>
			</xsl:if>
			<option value="">
				<xsl:variable name="label_low" select="translate(label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
				<xsl:text>Not Specified</xsl:text>
			</option>
			<xsl:apply-templates select="/Page/Menu/MenuItem" mode="listPageOptionRef">
				<xsl:with-param name="level" select="number(1)"/>
				<xsl:with-param name="selectedValue" select="$selectedValue"/>
			</xsl:apply-templates>
			<!--<xsl:apply-templates select="item" mode="xform_select"/>-->
		</select>
	</xsl:template>

	<xsl:template match="MenuItem" mode="listPageOptionRef">
		<xsl:param name="level"/>
		<xsl:param name="selectedValue"/>
		<xsl:variable name="refVal">
			<xsl:choose>
				<xsl:when test="ref/node()!=''">
					<xsl:value-of select="ref/node()"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>-None</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<option>
			<xsl:attribute name="value">
				<xsl:value-of select="$refVal"/>
			</xsl:attribute>
			<xsl:if test="$selectedValue=$refVal">
				<xsl:attribute name="selected">selected</xsl:attribute>
			</xsl:if>
			<xsl:call-template name="writeSpacers">
				<xsl:with-param name="spacers" select="$level"/>
			</xsl:call-template>
			<xsl:text>|-&#160;</xsl:text>
			<xsl:value-of select="@name"/>
			<xsl:text> </xsl:text>
		</option>
		<xsl:if test="count(child::MenuItem)&gt;0">
			<xsl:apply-templates select="MenuItem" mode="listPageOptionRef">
				<xsl:with-param name="level" select="number($level+1)"/>
				<xsl:with-param name="selectedValue" select="$selectedValue"/>
			</xsl:apply-templates>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template match="MenuItem" mode="listPageOption">
		<xsl:param name="level"/>
		<xsl:param name="selectedValue"/>
		<option>
			<xsl:attribute name="value">
				<xsl:value-of select="@id"/>
			</xsl:attribute>
			<xsl:if test="$selectedValue=@id">
				<xsl:attribute name="selected">selected</xsl:attribute>
			</xsl:if>
			<xsl:call-template name="writeSpacers">
				<xsl:with-param name="spacers" select="$level"/>
			</xsl:call-template>
			<xsl:text>|-&#160;</xsl:text>
			<xsl:value-of select="@name"/>
			<xsl:if test="@status='0'">
				<xsl:text> (hidden)</xsl:text>
			</xsl:if>
			<xsl:text> </xsl:text>
		</option>
		<xsl:if test="count(child::MenuItem)&gt;0">
			<xsl:apply-templates select="MenuItem" mode="listPageOption">
				<xsl:with-param name="level" select="number($level+1)"/>
				<xsl:with-param name="selectedValue" select="$selectedValue"/>
			</xsl:apply-templates>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template name="writeSpacers">
		<xsl:param name="spacers"/>
		<xsl:text>&#160;&#160;</xsl:text>
		<xsl:if test="number($spacers)&gt;1">
			<xsl:call-template name="writeSpacers">
				<xsl:with-param name="spacers" select="number($spacers)-1"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="getFilterButtons" mode="xform">
		<xsl:variable name="filterButtons">
			<xsl:apply-templates select="." mode="getFilterButtons"/>
			<!--
      <buttons>
        <button>pageFilter<button>
        <button>dateFilter<button>
      <buttons>
      -->
		</xsl:variable>
		<div>
			<xsl:for-each select="ms:node($filterButtons)/button">
				<xsl:variable name="buttonName" select="node()"/>
				<xsl:choose>
					<xsl:when test="ancestor::Content/Content[@filterType=$buttonName]">
						<!-- edit button and show filter details -->
					</xsl:when>
					<xsl:otherwise>
						<!-- add button -->
					</xsl:otherwise>
				</xsl:choose>
			</xsl:for-each>
		</div>
	</xsl:template>

	<!-- ##############################################-Nathan (New) RELATED CONTENT-############################## -->
	<xsl:template match="relatedContent" mode="xform">
		<xsl:param name="contentType" select="@type"/>
		<xsl:param name="relationType" select="@relationType"/>
		<xsl:param name="rcCount" select="count(ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType])"/>
		<xsl:param name="contentTypeLabel" select="label/node()"/>
		<xsl:param name="formName" select="ancestor::Content/model/submission/@id"/>
		<xsl:variable name="RelType">
			<xsl:choose>
				<xsl:when test="contains(@direction,'1way')">
					<xsl:text>1way</xsl:text>
				</xsl:when>
				<xsl:when test="contains(@direction,'2way')">
					<xsl:text>2way</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>2way</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<!--This way we get the type of content we relate to dynamically-->

		<label for="Related_{$relationType}">
			<xsl:choose>
				<xsl:when test="label/node()!=''">
					<xsl:value-of select="label/node()"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:if test="$relationType!=''">
						<xsl:value-of select="$relationType"/>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>

			<!--<small>-->

			<xsl:choose>
				<xsl:when test="contains(@direction,'1way')">
					<xsl:text> (1 Way Relationship)</xsl:text>
				</xsl:when>
				<xsl:when test="contains(@direction,'2way')">
					<xsl:text> (2 Way Relationship)</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text> (2 Way Relationship)</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
			<!--<xsl:value-of select="$relationType"/>-->
			<!--</small>-->
		</label>
		<xsl:choose>
			<xsl:when test="ancestor::Content/model/instance/ContentRelations[@copyRelations='true']">
				Copy the following relationships
			</xsl:when>
			<xsl:otherwise>
				<xsl:variable name="contentCount" select="count(ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType])"/>
				<!-- Limit Number of Related Content-->
				<xsl:if test="contains(@search,'pick')">
					<xsl:variable name="valueList">
						<list>
							<xsl:for-each select="ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType and (@rtype=$relationType or not(@rtype) or  @rtype='')]">
								<item>
									<xsl:value-of select="@id"/>
								</item>
							</xsl:for-each>
						</list>
					</xsl:variable>
					<xsl:variable name="value" select="ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType and (@rtype=$relationType or not(@rtype) or  @rtype='')][1]/@id"/>
					<div class="select-with-button">
						<div class="input-group input-group-sm">
							<select name="Related-{$relationType}" id="Related_{$relationType}" class="form-control form-select">
								<xsl:if test="@maxRelationNo &gt; 1">
									<xsl:attribute name="multiple">multiple</xsl:attribute>
								</xsl:if>
								<xsl:if test="@size &gt; 1">
									<xsl:attribute name="size">
										<xsl:value-of select="@size"/>
									</xsl:attribute>
								</xsl:if>
								<xsl:variable name="reationPickList">
									<xsl:call-template name="getSelectOptionsFunction">
										<xsl:with-param name="query">
											<xsl:text>Content.</xsl:text>
											<xsl:value-of select="$contentType"/>
										</xsl:with-param>
									</xsl:call-template>
								</xsl:variable>
								<option value="">None  </option>
								<xsl:apply-templates select="ms:node-set($reationPickList)/select1/*" mode="xform_select_multi">
									<xsl:with-param name="selectedValues" select="$valueList"/>
								</xsl:apply-templates>
							</select>
						</div>
						<xsl:if test="@maxRelationNo &gt; 1">
							<div class="alert alert-info">
								<i class="fa fa-info">&#160;</i> Press CTRL and click to select more than one option
							</div>
						</xsl:if>
					</div>
				</xsl:if>
				<xsl:if test="not(@maxRelationNo) or @maxRelationNo='' or (@maxRelationNo &gt; $contentCount)">
					<div class="btn-group-spaced">
						<xsl:if test="contains(@search,'multiple')">
							<a data-bs-toggle="modal" id="add-multiple-btn" data-parentid="{/Page/Request/QueryString/Item[@name='id']}"  href="?contentType=popup&amp;ewCmd=ImageLib&amp;targetForm={ancestor::Content/model/submission/@id}&amp;targetField={@type}-{@relationType}&amp;targetClass={value/*/@class}&amp;fld={@targetFolder}&amp;multiple=true" data-bs-target="#modal-{@type}-{@relationType}" class="btn btn-primary btn-sm">
								<i class="fa-solid fa-images fa-white">
									<xsl:text> </xsl:text>
								</i><xsl:text> </xsl:text>Add Multiple
							</a>
						</xsl:if>
						<xsl:if test="contains(@search,'find')">
							<button ref="repeat" type="button" name="RelateFind_{$contentType}_{$RelType}_{$relationType}" value="Find Existing {$contentType}" class="btn btn-primary btn-sm" onclick="disableButton(this);$('#{$formName}').submit();" >
								<i class="fa fa-search fa-white">
									<xsl:text> </xsl:text>
								</i> Find Existing <xsl:value-of select="$contentType"/>
							</button>
						</xsl:if>
						<xsl:if test="contains(@search,'add')">
							<button ref="repeat" type="button" name="RelateAdd_{$contentType}_{$RelType}_{$relationType}" value="Add New" class="btn  btn-primary btn-sm" onclick="disableButton(this);$('#{$formName}').submit();">
								<i class="fa fa-plus fa-white">
									<xsl:text> </xsl:text>
								</i> Add New
							</button>
						</xsl:if>
					</div>
				</xsl:if>
			</xsl:otherwise>
		</xsl:choose>

		<xsl:if test="not(contains(@search,'pick'))">
			<div class="related-content-rows">
				<xsl:apply-templates select="ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType and (@rtype=$relationType or not(@rtype) or  @rtype='')]" mode="relatedRow">
					<xsl:sort select="@status" data-type="number" order="descending"/>
					<xsl:sort select="@displayorder" data-type="number" order="ascending"/>
					<xsl:with-param name="formName" select="$formName" />
					<xsl:with-param name="relationType" select="$relationType" />
					<xsl:with-param name="relationDirection" select="$RelType" />
				</xsl:apply-templates>
			</div>
		</xsl:if>
	</xsl:template>

	<xsl:template match="relatedContent[contains(@search,'multiple')]" mode="xform_control_script">
		<div id="modal-{@type}-{@relationType}" class="modal fade pickImageModal">
	
				<div class="modal-dialog" id="test">
					<div class="modal-content">
						<div class="modal-body">
							<p class="text-center">
								<h4>
									<i class="fa fa-cog fa-spin fa-2x fa-fw">
										<xsl:text> </xsl:text>
									</i> Loading ...
								</h4>
							</p>
						</div>
					</div>
				
			</div>
		</div>
	</xsl:template>

	<xsl:template match="relatedContent[@type='filter']" mode="xform">
		<xsl:param name="contentType" select="@type"/>
		<xsl:param name="relationType" select="@relationType"/>
		<xsl:param name="rcCount" select="count(ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType])"/>
		<xsl:param name="contentTypeLabel" select="label/node()"/>
		<xsl:param name="formName" select="ancestor::Content/model/submission/@id"/>
		<xsl:variable name="RelType">
			<xsl:choose>
				<xsl:when test="contains(@direction,'1way')">
					<xsl:text>1way</xsl:text>
				</xsl:when>
				<xsl:when test="contains(@direction,'2way')">
					<xsl:text>2way</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>2way</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<!--This way we get the type of content we relate to dynamically-->

		<xsl:value-of select="$relationType"/>
		<label for="Related_{$relationType}">
			<xsl:choose>
				<xsl:when test="label/node()!=''">
					<xsl:value-of select="label/node()"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:if test="$relationType!=''">
						<xsl:value-of select="$relationType"/>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>

			<!--<small>-->

			<xsl:choose>
				<xsl:when test="contains(@direction,'1way')">
					<xsl:text> (1 Way Relationship)</xsl:text>
				</xsl:when>
				<xsl:when test="contains(@direction,'2way')">
					<xsl:text> (2 Way Relationship)</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text> (2 Way Relationship)</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:value-of select="$relationType"/>
			<!--</small>-->
		</label>
		<xsl:choose>
			<xsl:when test="ancestor::Content/model/instance/ContentRelations[@copyRelations='true']">
				Copy the following relationships
			</xsl:when>
			<xsl:otherwise>
				<xsl:variable name="contentCount" select="count(ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType])"/>
				<!-- Limit Number of Related Content-->
				<xsl:if test="contains(@search,'pick')">
					<xsl:variable name="valueList">
						<list>
							<xsl:for-each select="ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType and (@rtype=$relationType or not(@rtype) or  @rtype='')]">
								<item>
									<xsl:value-of select="@id"/>
								</item>
							</xsl:for-each>
						</list>
					</xsl:variable>
					<xsl:variable name="value" select="ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType and (@rtype=$relationType or not(@rtype) or  @rtype='')][1]/@id"/>
					<select name="Related-{$relationType}" id="Related_{$relationType}" class="form-control">
						<xsl:if test="@maxRelationNo &gt; 1">
							<xsl:attribute name="multiple">multiple</xsl:attribute>
						</xsl:if>
						<xsl:if test="@size &gt; 1">
							<xsl:attribute name="size">
								<xsl:value-of select="@size"/>
							</xsl:attribute>
						</xsl:if>
						<xsl:variable name="reationPickList">
							<xsl:call-template name="getSelectOptionsFunction">
								<xsl:with-param name="query">
									<xsl:text>Content.</xsl:text>
									<xsl:value-of select="$contentType"/>
								</xsl:with-param>
							</xsl:call-template>
						</xsl:variable>
						<option value="">None  </option>
						<xsl:apply-templates select="ms:node-set($reationPickList)/select1/*" mode="xform_select_multi">
							<xsl:with-param name="selectedValues" select="$valueList"/>
						</xsl:apply-templates>
					</select>
					<xsl:if test="@maxRelationNo &gt; 1">
						<div class="alert alert-info">
							<i class="fa fa-info">&#160;</i> Press CTRL and click to select more than one option
						</div>
					</xsl:if>
					<xsl:if test="contains(@search,'add')">
						<span class="input-group-btn pull-right">
							<button ref="repeat" type="button" name="RelateAdd_{$contentType}_{$RelType}_{$relationType}" value="Add New" class="btn btn-primary" onclick="disableButton(this);$('#{$formName}').submit();">
								<i class="fa fa-plus fa-white">
									<xsl:text> </xsl:text>
								</i> Add
							</button>
						</span>
					</xsl:if>
				</xsl:if>
				<xsl:if test="not(@maxRelationNo) or @maxRelationNo='' or (@maxRelationNo &gt; $contentCount)">
					<xsl:if test="contains(@search,'find')">
						<button ref="repeat" type="button" name="RelateFind_{$contentType}_{$RelType}_{$relationType}" value="Find Existing {$contentType}" class="btn btn-primary" onclick="disableButton(this);$('#{$formName}').submit();" >
							<i class="fa fa-search fa-white">
								<xsl:text> </xsl:text>
							</i> Find Existing <xsl:value-of select="$contentType"/>
						</button>
					</xsl:if>
					<xsl:if test="contains(@search,'add')">
						<button ref="repeat" type="button" name="RelateAdd_{$contentType}_{$RelType}_{$relationType}" value="Add New" class="btn btn-primary" onclick="disableButton(this);$('#{$formName}').submit();">
							<i class="fa fa-plus fa-white">
								<xsl:text> </xsl:text>
							</i> Add New
						</button>
					</xsl:if>
				</xsl:if>
			</xsl:otherwise>
		</xsl:choose>

		<xsl:if test="not(contains(@search,'pick'))">

			<div class="related-content-rows">
				<xsl:apply-templates select="ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType and (@rtype=$relationType or not(@rtype) or  @rtype='')]" mode="relatedRow">
					<xsl:sort select="@status" data-type="number" order="descending"/>
					<xsl:sort select="@displayorder" data-type="number" order="ascending"/>
					<xsl:with-param name="formName" select="$formName" />
					<xsl:with-param name="relationType" select="$relationType" />
					<xsl:with-param name="relationDirection" select="$RelType" />
				</xsl:apply-templates>
			</div>
		</xsl:if>
	</xsl:template>

	<xsl:template match="relatedContent[@type='filter']" mode="xform">
		<xsl:param name="contentType" select="@type"/>
		<xsl:param name="relationType" select="@relationType"/>
		<xsl:param name="rcCount" select="count(ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType])"/>
		<xsl:param name="contentTypeLabel" select="label/node()"/>
		<xsl:param name="formName" select="ancestor::Content/model/submission/@id"/>
		<xsl:variable name="RelType">
			<xsl:choose>
				<xsl:when test="contains(@direction,'1way')">
					<xsl:text>1way</xsl:text>
				</xsl:when>
				<xsl:when test="contains(@direction,'2way')">
					<xsl:text>2way</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>2way</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<!--This way we get the type of content we relate to dynamically-->

		<xsl:value-of select="$relationType"/>
		<label for="Related_{$relationType}">
			<xsl:choose>
				<xsl:when test="label/node()!=''">
					<xsl:value-of select="label/node()"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:if test="$relationType!=''">
						<xsl:value-of select="$relationType"/>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>

			<!--<small>-->

			<xsl:choose>
				<xsl:when test="contains(@direction,'1way')">
					<xsl:text> (1 Way Relationship)</xsl:text>
				</xsl:when>
				<xsl:when test="contains(@direction,'2way')">
					<xsl:text> (2 Way Relationship)</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text> (2 Way Relationship)</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:value-of select="$relationType"/>
			<!--</small>-->
		</label>
		<xsl:choose>
			<xsl:when test="ancestor::Content/model/instance/ContentRelations[@copyRelations='true']">
				Copy the following relationships
			</xsl:when>
			<xsl:otherwise>
				<xsl:variable name="contentCount" select="count(ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType])"/>
				<!-- Limit Number of Related Content-->
				<xsl:if test="contains(@search,'pick')">
					<xsl:variable name="valueList">
						<list>
							<xsl:for-each select="ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType and (@rtype=$relationType or not(@rtype) or  @rtype='')]">
								<item>
									<xsl:value-of select="@id"/>
								</item>
							</xsl:for-each>
						</list>
					</xsl:variable>
					<xsl:variable name="value" select="ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType and (@rtype=$relationType or not(@rtype) or  @rtype='')][1]/@id"/>
					<select name="Related-{$relationType}" id="Related_{$relationType}" class="form-control">
						<xsl:if test="@maxRelationNo &gt; 1">
							<xsl:attribute name="multiple">multiple</xsl:attribute>
						</xsl:if>
						<xsl:if test="@size &gt; 1">
							<xsl:attribute name="size">
								<xsl:value-of select="@size"/>
							</xsl:attribute>
						</xsl:if>
						<xsl:variable name="reationPickList">
							<xsl:call-template name="getSelectOptionsFunction">
								<xsl:with-param name="query">
									<xsl:text>Content.</xsl:text>
									<xsl:value-of select="$contentType"/>
								</xsl:with-param>
							</xsl:call-template>
						</xsl:variable>
						<option value="">None  </option>
						<xsl:apply-templates select="ms:node-set($reationPickList)/select1/*" mode="xform_select_multi">
							<xsl:with-param name="selectedValues" select="$valueList"/>
						</xsl:apply-templates>
					</select>
					<xsl:if test="@maxRelationNo &gt; 1">
						<div class="alert alert-info">
							<i class="fa fa-info">&#160;</i> Press CTRL and click to select more than one option
						</div>
					</xsl:if>
					<xsl:if test="contains(@search,'add')">
						<span class="input-group-btn pull-right">
							<button ref="repeat" type="button" name="RelateAdd_{$contentType}_{$RelType}_{$relationType}" value="Add New" class="btn btn-primary" onclick="disableButton(this);$('#{$formName}').submit();">
								<i class="fa fa-plus fa-white">
									<xsl:text> </xsl:text>
								</i> Add
							</button>
						</span>
					</xsl:if>
				</xsl:if>
				<xsl:if test="not(@maxRelationNo) or @maxRelationNo='' or (@maxRelationNo &gt; $contentCount)">
					<xsl:if test="contains(@search,'find')">
						<button ref="repeat" type="button" name="RelateFind_{$contentType}_{$RelType}_{$relationType}" value="Find Existing {$contentType}" class="btn btn-primary" onclick="disableButton(this);$('#{$formName}').submit();" >
							<i class="fa fa-search fa-white">
								<xsl:text> </xsl:text>
							</i> Find Existing <xsl:value-of select="$contentType"/>
						</button>
					</xsl:if>
					<xsl:if test="contains(@search,'add')">
						<button ref="repeat" type="button" name="RelateAdd_{$contentType}_{$RelType}_{$relationType}" value="Add New" class="btn btn-primary" onclick="disableButton(this);$('#{$formName}').submit();">
							<i class="fa fa-plus fa-white">
								<xsl:text> </xsl:text>
							</i> Add New
						</button>
					</xsl:if>
				</xsl:if>
			</xsl:otherwise>
		</xsl:choose>

		<xsl:if test="not(contains(@search,'pick'))">

			<div class="related-content-rows">
				<xsl:apply-templates select="ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType and (@rtype=$relationType or not(@rtype) or  @rtype='')]" mode="relatedRow">
					<xsl:sort select="@status" data-type="number" order="descending"/>
					<xsl:sort select="@displayorder" data-type="number" order="ascending"/>
					<xsl:with-param name="formName" select="$formName" />
					<xsl:with-param name="relationType" select="$relationType" />
					<xsl:with-param name="relationDirection" select="$RelType" />
				</xsl:apply-templates>
			</div>
		</xsl:if>
	</xsl:template>

	<xsl:template match="Content" mode="relatedRow">
		<xsl:param name="formName"/>
		<xsl:param name="relationType"/>
		<xsl:param name="relationDirection"/>
		<div class="advancedModeRow row" onmouseover="this.className='rowOver row'" onmouseout="this.className='advancedModeRow row'">
			<xsl:if test="@status=0">
				<xsl:attribute name="class">advancedModeRow row inactive-related</xsl:attribute>
			</xsl:if>
			<div class="col-xl-6 col-xxl-7">
				<xsl:if test="@status=0">
					<xsl:attribute name="class">col-xl-5 col-xxl-6</xsl:attribute>
				</xsl:if>
				<!--<xsl:apply-templates select="." mode="status_legend"/>-->
				<xsl:text> </xsl:text>
				<xsl:apply-templates select="." mode="relatedBrief"/>
			</div>
			<xsl:choose>
				<xsl:when test="parent::ContentRelations[@copyRelations='true']">
					<div class="col-xl-6 col-xxl-5">
						<xsl:if test="@status=0">
							<xsl:attribute name="class">col-xl-7 col-xxl-6</xsl:attribute>
						</xsl:if>
						<input type="checkbox" name="Relate_{$relationType}_{$relationDirection}" value="{@id}" checked="checked">
							<xsl:text> </xsl:text>Relate
						</input>
					</div>
				</xsl:when>
				<xsl:otherwise>
					<div class="col-xl-6 col-xxl-5 edit-option-links">
						<xsl:if test="@status=0">
							<xsl:attribute name="class">col-xl-7 col-xxl-6 edit-option-links</xsl:attribute>
						</xsl:if>
						<button type="button" name="RelateTop_{@id}" value=" " class="" onClick="disableButton(this);{$formName}.submit()">
							<i class="fa fa-arrow-up fa-white">
								<xsl:text> </xsl:text>
							</i>
						</button>
						<button type="button" name="RelateUp_{@id}" value=" " class=""  onClick="disableButton(this);{$formName}.submit()">
							<i class="fa fa-chevron-up fa-white">
								<xsl:text> </xsl:text>
							</i>
						</button>
						<button type="button" name="RelateDown_{@id}" value=" " class=""  onClick="disableButton(this);{$formName}.submit()">
							<i class="fa fa-chevron-down fa-white">
								<xsl:text> </xsl:text>
							</i>
						</button>
						<button type="button" name="RelateBottom_{@id}" value=" " class="" onClick="disableButton(this);{$formName}.submit()">
							<i class="fa fa-arrow-down fa-white">
								<xsl:text> </xsl:text>
							</i>
						</button>
						<button type="button" name="RelateEdit_{@id}" value="Edit" class=" " onClick="disableButton(this);{$formName}.submit()">
							<i class="fa fa-edit fa-white">
								<xsl:text> </xsl:text>
							</i>
							<xsl:text> </xsl:text>Edit
						</button>
						<button type="button" name="RelateRemove_{@id}" value="Delete Relation" class="link-danger"  onClick="disableButton(this);{$formName}.submit()">
							<i class="fa fa-minus fa-white">
								<xsl:text> </xsl:text>
							</i>
							<xsl:text> </xsl:text>Remove
						</button>
						<xsl:if test="@status='1'">
							<a href="?ewCmd=HideContent&amp;id={@id}" title="Click here to hide this item" class="">
								<i class="fa fa fa-eye fa-white">
									<xsl:text> </xsl:text>
								</i>
							</a>
						</xsl:if>
						<xsl:if test="@status='0'">
							<a href="?ewCmd=ShowContent&amp;id={@id}" title="Click here to show this item" class="">
								<i class="fa fa-eye-slash fa-white">
									<xsl:text> </xsl:text>
								</i>
							</a>
							<a href="?ewCmd=DeleteContent&amp;id={@id}" title="Click here to delete this item" class="link-danger">
								<i class="fas fa-trash fa-white">
									<xsl:text> </xsl:text>
								</i>
							</a>
						</xsl:if>
					</div>
				</xsl:otherwise>
			</xsl:choose>


		</div>
	</xsl:template>


	<xsl:template match="Content" mode="relatedBrief">
		<xsl:apply-templates select="." mode="getDisplayName" />
	</xsl:template>

	<xsl:template match="Content[@type='Ticket']" mode="relatedBrief">
		<xsl:apply-templates select="." mode="getDisplayName" /> -
		<span class="date">
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
		</span>
	</xsl:template>

	<xsl:template match="Content[@type='NewsArticle']" mode="relatedBrief">
		<strong>
			<xsl:apply-templates select="." mode="getDisplayName" />
		</strong>
		<br/>
		<!--<small>-->
		<xsl:call-template name="truncate-string">
			<xsl:with-param name="text" select="Strapline/node()"/>
			<xsl:with-param name="length" select="140"/>
		</xsl:call-template>
		<!--</small>-->
	</xsl:template>

	<xsl:template match="Content[@type='LibraryImage']" mode="relatedBrief">
		<xsl:apply-templates select="." mode="displayThumbnail">
			<xsl:with-param name="width" select="'50'"/>
			<xsl:with-param name="height" select="'50'"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="Content[@type='Product']" mode="relatedBrief">
		<xsl:apply-templates select="." mode="getDisplayName" />
		<xsl:text> - </xsl:text>
		<xsl:value-of select="StockCode"/>
	</xsl:template>



	<!-- -->
	<xsl:template match="select1[@appearance='full' and @class='PickByImage']" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<!--<xsl:attribute name="class">pickByImage</xsl:attribute>-->
		<!--<input type="hidden" name="{$ref}" value="{value/node()}"/>-->
		<div class="accordion" id="pick-by-image">
			<xsl:apply-templates select="item | choices" mode="xform_imageClick">
				<xsl:with-param name="type">radio</xsl:with-param>
				<xsl:with-param name="ref" select="$ref"/>
			</xsl:apply-templates>
		</div>
	</xsl:template>

	<xsl:template match="choices" mode="xform_imageClick">
		<xsl:param name="type"/>
		<xsl:param name="ref"/>
		<xsl:variable name="makeClass" select="translate(label, ' ', '_')"/>

		<div class="accordion-item">
			<h5 class="accordion-header" id="heading{$makeClass}">
				<button class="accordion-button collapsed" type="button" data-bs-toggle="collapse" data-bs-target="#collapse{$makeClass}" aria-expanded="true" aria-controls="collapse{$makeClass}">
					<xsl:if test="label/@icon!=''">
						<i class="{label/@icon}">&#160;</i>&#160;
					</xsl:if>
					<xsl:apply-templates select="label" mode="xform_legend"/>
				</button>
			</h5>
			<div id="collapse{$makeClass}"  aria-labelledby="heading{$makeClass}" data-bs-parent="#pick-by-image">
				<xsl:attribute name="class">
					<xsl:text>accordion-collapse collapse row </xsl:text>
					<xsl:if test="position()=1">
						<xsl:text> in</xsl:text>
					</xsl:if>
				</xsl:attribute>
				<xsl:apply-templates select="item" mode="xform_imageClick">
					<xsl:with-param name="ref" select="$ref"/>
				</xsl:apply-templates>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="item" mode="xform_imageClick">
		<xsl:param name="ref"/>
		<xsl:variable name="value" select="value/node()"/>
		<xsl:variable name="selectedValue" select="ancestor::select1/value/node()"/>
		<xsl:variable name="isSelected">
			<xsl:if test="$value=$selectedValue">
				<xsl:text>selected</xsl:text>
			</xsl:if>
		</xsl:variable>
		<xsl:variable name="ifExists">
			<xsl:call-template name="virtual-file-exists">
				<xsl:with-param name="path" select="translate(img/@src,' ','-')"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:variable name="icon">
			<xsl:choose>
				<xsl:when test="img/@icon!=''">
					<xsl:value-of select="img/@icon"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>fas fa-puzzle-piece</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<div class="col-sm-6 col-md-4 col-lg-3 col-xl-2 col-xxl-2">
			<button name="{$ref}" value="{value/node()}" class="{$isSelected}">
				<!--<img src="{$imageURL}" class="card-img-top"/>-->
				<i class="fas fa-3x {$icon}">
          <xsl:text> </xsl:text>
        </i>
				<h5>
					<xsl:value-of select="label/node()"/>
				</h5>
				<xsl:copy-of select="div"/>
			</button>
		</div>
	</xsl:template>

	<xsl:template match="item[label/Theme]" mode="xform_imageClick">
		<xsl:param name="ref"/>

		<xsl:variable name="ifExists">
			<xsl:call-template name="virtual-file-exists">
				<xsl:with-param name="path" select="translate(label/Theme/Images/img[@class='thumbnail']/@src,' ','-')"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:variable name="imageURL">
			<xsl:choose>
				<xsl:when test="$ifExists='1'">
					<xsl:value-of select="translate(label/Theme/Images/img[@class='thumbnail']/@src,' ','-')"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>/ewcommon/images/pagelayouts/webTheme.png</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="value" select="label/Theme/@name"/>

		<div class="col-md-4">
			<button name="{$ref}" value="{$value}" class="panel panel-default imageSelect">
				<xsl:if test="$value=ancestor::select1/value/node()">
					<xsl:attribute name="class">
						panel panel-default imageSelect active
					</xsl:attribute>
					<xsl:attribute name="disabled">
						disabled
					</xsl:attribute>
				</xsl:if>
				<img src="{$imageURL}" class="pull-left"/>
				<h5>
					<xsl:value-of select="label/Theme/@name"/>
				</h5>
				<div>
					<xsl:apply-templates select="label/Theme/Description/node()" mode="cleanXhtml"/>
					<!--<small>-->
					<xsl:apply-templates select="label/Theme/Attribution/node()" mode="cleanXhtml"/>
					<!--</small>-->
				</div>
			</button>
		</div>
	</xsl:template>

	<!-- -->
	<xsl:template match="select1[@appearance='minimal' and contains(@class,'iconSelect')]" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<div class="bfh-selectbox pickIcon" data-name="{$ref}" data-value="{value/node()}">
			<xsl:variable name="selectOptions">
				<xsl:apply-templates select="." mode="getSelectOptions"/>
			</xsl:variable>
			<xsl:apply-templates select="ms:node-set($selectOptions)/select1/*" mode="xform_PickIcon">
				<xsl:with-param name="selectedValue" select="value/node()"/>
			</xsl:apply-templates>
			<!--<xsl:apply-templates select="item | choices" mode="xform_PickIcon">
        <xsl:with-param name="type">radio</xsl:with-param>
        <xsl:with-param name="ref" select="$ref"/>
      </xsl:apply-templates>-->
		</div>
	</xsl:template>


	<xsl:template match="choices | itemset" mode="xform_PickIcon">
		<xsl:param name="type"/>
		<xsl:param name="ref"/>
		<xsl:variable name="makeClass" select="translate(label, ' ', '_')"/>
		<!--<div class="title">
      <xsl:apply-templates select="label" mode="xform_legend"/>
    </div>-->
		<xsl:apply-templates select="item" mode="xform_PickIcon">
			<xsl:with-param name="ref" select="$ref"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="item" mode="xform_PickIcon">
		<xsl:param name="ref"/>
		<div data-value="{value/node()}">
			<i class="fa {value/node()} fa-lg">
				<xsl:text> </xsl:text>
			</i>
		</div>
	</xsl:template>

	<xsl:template match="item" mode="xform_BoxStyles">
		<xsl:param name="ref"/>
		<div data-value="{value/node()}">
			<xsl:value-of select="label/node()"/>
		</div>
	</xsl:template>

	<xsl:template match="item[node()='Default Box']" mode="xform_BoxStyles">
		<xsl:param name="ref"/>
		<div data-value="{value/node()}" class="card-default">
			<xsl:value-of select="value/node()"/>
		</div>
	</xsl:template>

	<!-- -->
	<xsl:template match="select1[@appearance='minimal' and contains(@class,'bgStyle')]" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<select name="{$ref}" id="{$ref}">
			<xsl:if test="@class!=''">
				<xsl:attribute name="class">
					<xsl:value-of select="@class"/>
					<xsl:text> dropdown form-control</xsl:text>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="@onChange!=''">
				<xsl:attribute name="onChange">
					<xsl:value-of select="@onChange"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="item" mode="xform_select"/>
			<xsl:apply-templates select="." mode="siteBGStyles">
				<xsl:with-param name="value" select="value/node()" />
			</xsl:apply-templates>
			<xsl:apply-templates select="." mode="bootstrapBGStyles">
				<xsl:with-param name="value" select="value/node()" />
			</xsl:apply-templates>
		</select>
	</xsl:template>

	<xsl:template match="*" mode="siteBGStyles">
		<xsl:param name="value" />
		<!-- EXAMPLE BESPOKE BOX-->
		<!--option value="testBG">
      <xsl:if test="$value='testBG'">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:text>testBG</xsl:text>
    </option-->
	</xsl:template>

	<xsl:template match="*[ancestor::Page[Settings/add[@key='theme.BespokeBackgrounds']/@value!='']]" mode="siteBGStyles">
		<xsl:param name="value" />
		<xsl:variable name="styles">
			<styles>
				<xsl:call-template name="split-string">
					<xsl:with-param name="list" select="/Page/Settings/add[@key='theme.BespokeBackgrounds']/@value" />
					<xsl:with-param name="seperator" select="','" />
				</xsl:call-template>
			</styles>
		</xsl:variable>

		<xsl:for-each select="ms:node-set($styles)/*/*">
			<!-- EXAMPLE BESPOKE BOX-->
			<option value="{node()}">
				<xsl:if test="$value=node()">
					<xsl:attribute name="selected">selected</xsl:attribute>
				</xsl:if>
				<xsl:value-of select="node()"/>
			</option>
		</xsl:for-each>

	</xsl:template>

	<xsl:template match="select1[@appearance='minimal' and contains(@class,'bannerStyles')]" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<select name="{$ref}" id="{$ref}">
			<xsl:if test="@class!=''">
				<xsl:attribute name="class">
					<xsl:value-of select="@class"/>
					<xsl:text> dropdown form-control</xsl:text>
				</xsl:attribute>

			</xsl:if>
			<xsl:if test="@onChange!=''">
				<xsl:attribute name="onChange">
					<xsl:value-of select="@onChange"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="item" mode="xform_select"/>
			<xsl:apply-templates select="." mode="bannerStyles1">
				<xsl:with-param name="value" select="value/node()" />
			</xsl:apply-templates>
		</select>
	</xsl:template>
	
	<xsl:template match="*" mode="bannerStyles">
		<xsl:param name="value" />
		<option value="">
			<xsl:if test="$value=''">
				<xsl:attribute name="selected">selected</xsl:attribute>
			</xsl:if>
			<xsl:text>Basic banner</xsl:text>
		</option>
		<option value="no-banner">
			<xsl:if test="$value='no-banner'">
				<xsl:attribute name="selected">selected</xsl:attribute>
			</xsl:if>
			<xsl:text>No banner</xsl:text>
		</option>
	</xsl:template>
	
	<!-- -->
	<xsl:template match="*" mode="bootstrapBGStyles">
		<xsl:param name="value" />
		<!-- THEIR ARE NO GENRIC BACKGROUNDS-->
	</xsl:template>

	<xsl:template match="select1[@appearance='minimal' and contains(@class,'menuStyles')]" mode="control-outer">
		
	</xsl:template>
	
	<!-- -->
	<xsl:template match="select1[@appearance='minimal' and contains(@class,'cssStyle')]" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>

		<div class="bfh-selectbox cssStyle" data-name="{$ref}" data-value="{value/node()}">

			<xsl:apply-templates select="." mode="siteCssStyles">
				<xsl:with-param name="value" select="value/node()" />
			</xsl:apply-templates>
			<xsl:apply-templates select="item" mode="xFormCssStyles">
				<xsl:with-param name="value" select="value/node()" />
			</xsl:apply-templates>
		</div>

	</xsl:template>

	<xsl:template match="item" mode="xFormCssStyles">
		<xsl:param name="ref"/>
		<div data-value="{value/node()}">
			<xsl:value-of select="label/node()"/>
		</div>
	</xsl:template>

	<xsl:template match="item[node()!='None']" mode="xFormCssStyles">
		<xsl:param name="ref"/>
		<div data-value="{value/node()}">
			<div class="Site">
				<div class="tp-caption {value/node()}">
					<span>
						<xsl:value-of select="label/node()"/>
					</span>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="*" mode="siteCssStyles">
		<xsl:param name="value" />
		<!-- EXAMPLE BESPOKE BOX-->
		<!--
    <div data-value="tint_bg">
      <div class="Site">
        <div class="tp-caption tint_bg">Tinted Grey Backgroud</div>
       </div>
    </div>
    -->
	</xsl:template>

	<xsl:template match="*[ancestor::Page[Settings/add[@key='theme.BespokeTextClasses']/@value!='']]" mode="siteCssStyles">
		<xsl:param name="value" />
		<xsl:variable name="styles">
			<styles>
				<xsl:call-template name="split-string">
					<xsl:with-param name="list" select="/Page/Settings/add[@key='theme.BespokeTextClasses']/@value" />
					<xsl:with-param name="seperator" select="','" />
				</xsl:call-template>
			</styles>
		</xsl:variable>

		<xsl:for-each select="ms:node-set($styles)/*/*">
			<!-- EXAMPLE BESPOKE BOX-->
			<div data-value="{node()}">
				<div class="Site">
					<div class="tp-caption {node()}">
						<xsl:value-of select="node()"/>
					</div>
				</div>
			</div>
		</xsl:for-each>

	</xsl:template>

	<xsl:template match="item[not(toggle)]" mode="xform_radiocheck">
		<xsl:param name="type"/>
		<xsl:param name="ref"/>
		<xsl:param name="selectedValue" />
		<!-- value if passed through -->
		<xsl:variable name="value" select="value"/>
		<xsl:variable name="val" select="value/node()"/>
		<!--<xsl:variable name="class" select="../@class"/>-->
		<xsl:variable name="class" select="ancestor::*[name()='select' or name()='select1' ]/@class"/>
		<div class="form-check">
			<xsl:if test="ancestor::*[contains(@class,'inline-items')]">
				<xsl:attribute name="class">form-check form-check-inline</xsl:attribute>
			</xsl:if>
			<input type="{$type}" class="form-check-input">
				<xsl:choose>
					<xsl:when test="contains(../@class,'alwayson')">
						<xsl:attribute name="name">disabled</xsl:attribute>
						<xsl:attribute name="disabled">disabled</xsl:attribute>
						<xsl:attribute name="checked">checked</xsl:attribute>
					</xsl:when>
					<xsl:otherwise>
						<xsl:if test="$ref!=''">
							<xsl:attribute name="name">
								<xsl:value-of select="$ref"/>
							</xsl:attribute>
							<xsl:attribute name="id">
								<xsl:value-of select="$ref"/>_<xsl:value-of select="$value"/>
							</xsl:attribute>
						</xsl:if>
						<xsl:attribute name="value">
							<xsl:value-of select="value"/>
						</xsl:attribute>
						<xsl:if test="../value/node()=$value">
							<xsl:attribute name="checked">checked</xsl:attribute>
						</xsl:if>
						<xsl:if test="not(../value/node()!='') and not($value!='')">
							<xsl:attribute name="checked">checked</xsl:attribute>
						</xsl:if>
						<!-- Check checkbox should be selected -->
						<xsl:if test="contains($class,'checkboxes') or (contains(ancestor::select/@appearance,'full'))">
							<!-- Run through CSL to see if this should be checked -->
							<xsl:variable name="valueMatch">
								<xsl:call-template name="checkValueMatch">
									<xsl:with-param name="CSLValue" select="ancestor::*[name()='select' or name()='select1' ]/value/node()"/>
									<xsl:with-param name="value" select="$value"/>
									<xsl:with-param name="seperator" select="','"/>
								</xsl:call-template>
							</xsl:variable>
							<xsl:if test="$valueMatch='true'">
								<xsl:attribute name="checked">checked</xsl:attribute>
							</xsl:if>
						</xsl:if>
						<xsl:if test="$type='checkbox'">
							<!-- Run through CSL to see if this should be checked -->
							<xsl:variable name="valueMatch">
								<xsl:call-template name="checkValueMatch">
									<xsl:with-param name="CSLValue" select="$selectedValue"/>
									<xsl:with-param name="value" select="$value"/>
									<xsl:with-param name="seperator" select="','"/>
								</xsl:call-template>
							</xsl:variable>
							<xsl:if test="$valueMatch='true'">
								<xsl:attribute name="checked">checked</xsl:attribute>
							</xsl:if>
						</xsl:if>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:if test="ancestor::select1/item[1]/value/node() = $value">
					<xsl:attribute name="data-fv-notempty">
						<xsl:value-of select="ancestor::select1/@data-fv-notempty"/>
					</xsl:attribute>
					<xsl:attribute name="data-fv-notempty-message">
						<xsl:value-of select="ancestor::select1/@data-fv-notempty-message"/>
					</xsl:attribute>
				</xsl:if>
				<xsl:if test="contains($class,'readonly')">
					<xsl:attribute name="disabled">disabled</xsl:attribute>
				</xsl:if>
			</input>
			<label for="{$ref}_{$value}">
				<xsl:if test="not(contains($class,'multiline'))">
					<xsl:attribute name="class">
						<xsl:text> form-check-label</xsl:text>
					</xsl:attribute>
				</xsl:if>
				<xsl:apply-templates select="label" mode="xform-label"/>
				<!-- needed to stop self closing -->
				<xsl:text> </xsl:text>
			</label>
		</div>
		<xsl:if test="/Page/@ewCmd='EditXForm'">
			<xsl:if test="ancestor::Content/model/instance/results/answers/answer[@ref=$ref]/score[value/node()=$val]">
				<span>
					<xsl:attribute name="class">
						<xsl:if test="ancestor::Content/model/instance/results/answers/answer[@ref=$ref]/score[value/node()=$val]">
							<xsl:text>correct</xsl:text>
						</xsl:if>
					</xsl:attribute>
					(<xsl:value-of select="ancestor::Content/model/instance/results/answers/answer[@ref=$ref]/score[value/node()=$val]/@weighting"/>)
				</span>
			</xsl:if>
		</xsl:if>
	</xsl:template>


	<xsl:template match="submit[contains(@class,'PermissionButton')]" mode="xform">
		<xsl:variable name="class">
			<xsl:text>btn btn-primary</xsl:text>
			<xsl:if test="@class!=''">
				<xsl:text> </xsl:text>
				<xsl:value-of select="@class"/>
			</xsl:if>
		</xsl:variable>
		<button type="submit" name="{@submission}" value="{label/node()}" class="{$class}"  onclick="disableButton(this);">
			<xsl:apply-templates select="label" mode="submitText"/>
		</button>
	</xsl:template>

	<xsl:template match="item[/Page/@adminMode and contains(@class,'multiline')]" mode="xform_radiocheck">
		<div class="radio">
			<xsl:apply-templates select="." mode="xform_radiocheck2"/>
		</div>
	</xsl:template>

	<!-- -->
	<xsl:template match="item" mode="xform_radiocheck">
		<xsl:param name="type"/>
		<xsl:param name="ref"/>
		<xsl:param name="value" select="value"/>
		<xsl:param name="selectedValue">
			<xsl:apply-templates select="ancestor::*[name()='select' or name()='select1']" mode="xform_value"/>
		</xsl:param>
		<xsl:variable name="class" select="ancestor::*[name()='select' or name()='select1' ]/@class"/>

		<span>
			<xsl:attribute name="class">
				<xsl:text>checkbox checkbox-primary</xsl:text>
				<xsl:if test="contains($class,'multiline')">
					<xsl:text> multiline</xsl:text>
				</xsl:if>
			</xsl:attribute>
			<input type="{$type}">
				<xsl:if test="$ref!=''">
					<xsl:attribute name="name">
						<xsl:value-of select="$ref"/>
					</xsl:attribute>
					<xsl:attribute name="id">
						<xsl:value-of select="$ref"/>_<xsl:value-of select="$value"/>
					</xsl:attribute>
				</xsl:if>
				<xsl:attribute name="value">
					<xsl:value-of select="value"/>
				</xsl:attribute>
				<xsl:attribute name="title">
					<xsl:value-of select="@title"/>
				</xsl:attribute>
				<xsl:attribute name="onclick">
					<xsl:value-of select="@onclick"/>
				</xsl:attribute>

				<!-- Check Radio adminButton is selected -->
				<xsl:if test="$selectedValue=$value">
					<xsl:attribute name="checked">checked</xsl:attribute>
				</xsl:if>
				<xsl:if test="not($selectedValue!='') and not($value!='')">
					<xsl:attribute name="checked">checked</xsl:attribute>
				</xsl:if>

				<!-- Check checkbox should be selected -->
				<xsl:if test="contains($type,'checkbox')">
					<!-- Run through CSL to see if this should be checked -->
					<xsl:variable name="valueMatch">
						<xsl:call-template name="checkValueMatch">
							<xsl:with-param name="CSLValue" select="$selectedValue"/>
							<xsl:with-param name="value" select="$value"/>
							<xsl:with-param name="seperator" select="','"/>
						</xsl:call-template>
					</xsl:variable>
					<xsl:if test="$valueMatch='true'">
						<xsl:attribute name="checked">checked</xsl:attribute>
					</xsl:if>
				</xsl:if>
				<xsl:if test="ancestor::select1/item[1]/value/node() = $value">
					<xsl:attribute name="data-fv-notempty">
						<xsl:value-of select="ancestor::select1/@data-fv-notempty"/>
					</xsl:attribute>
					<xsl:attribute name="data-fv-notempty-message">
						<xsl:value-of select="ancestor::select1/@data-fv-notempty-message"/>
					</xsl:attribute>
				</xsl:if>
				<xsl:if test="ancestor::select/item[1]/value/node() = $value">
					<xsl:attribute name="data-fv-choice">
						<xsl:value-of select="ancestor::select/@data-fv-choice"/>
					</xsl:attribute>
					<xsl:attribute name="data-fv-choice-min">
						<xsl:value-of select="ancestor::select/@data-fv-choice-min"/>
					</xsl:attribute>
					<xsl:attribute name="data-fv-choice-max">
						<xsl:value-of select="ancestor::select/@data-fv-choice-max"/>
					</xsl:attribute>
					<xsl:attribute name="data-fv-choice-message">
						<xsl:value-of select="ancestor::select/@data-fv-choice-message"/>
					</xsl:attribute>
					<xsl:if test="ancestor::select/@data-fv-notempty">
						<xsl:attribute name="data-fv-notempty">
							<xsl:value-of select="ancestor::select/@data-fv-notempty"/>
						</xsl:attribute>
						<xsl:attribute name="data-fv-notempty-message">
							<xsl:value-of select="ancestor::select/@data-fv-notempty-message"/>
						</xsl:attribute>
					</xsl:if>
				</xsl:if>
			</input>
			<label for="{$ref}_{$value}">
				<xsl:attribute name="class">
					<xsl:text>radio</xsl:text>
					<xsl:if test="label/@class and label/@class!=''">
						<xsl:text> </xsl:text>
						<xsl:value-of select="label/@class"/>
					</xsl:if>
					<xsl:if test="ancestor::Content[@type='xform' and @name='PayForm']">
						<xsl:text> </xsl:text>
						<xsl:value-of select="translate(value/node(),' /','--')"/>
					</xsl:if>
				</xsl:attribute>
				<!-- for payform to have cc classes-->

				<xsl:apply-templates select="label" mode="xform-label"/>
				<xsl:text> </xsl:text>
			</label>

		</span>
		<!--<xsl:if test="contains($class,'multiline') and position()!=last()">
					<br/>
				</xsl:if>-->

	</xsl:template>

	<!-- Radio Input with dependant Case toggle -->
	<xsl:template match="item[toggle]" mode="xform_radiocheck">
		<xsl:param name="type"/>
		<xsl:param name="ref"/>
		<xsl:param name="dependantClass"/>
		<xsl:variable name="value" select="value"/>
		<xsl:variable name="class" select="../@class"/>
		<div>
			<xsl:attribute name="class">
				<xsl:text>form-check form-check-inline</xsl:text>
				<xsl:if test="contains($class,'multiline')">
					<xsl:text> multiline</xsl:text>
				</xsl:if>
			</xsl:attribute>
			<input class="form-check-input" type="{$type}">
				<xsl:if test="$ref!=''">
					<xsl:attribute name="name">
						<xsl:value-of select="$ref"/>

					</xsl:attribute>
					<xsl:attribute name="id">
						<xsl:value-of select="$ref"/>_<xsl:value-of select="position()"/>
					</xsl:attribute>
				</xsl:if>
				<xsl:attribute name="value">
					<xsl:value-of select="value"/>
				</xsl:attribute>

				<!-- Check Radio adminButton is selected -->
				<xsl:if test="../value/node()=$value">
					<xsl:attribute name="checked">checked</xsl:attribute>
				</xsl:if>

				<!-- Check checkbox should be selected -->
				<xsl:if test="contains($class,'checkboxes')">
					<!-- Run through CSL to see if this should be checked -->
					<xsl:variable name="valueMatch">
						<xsl:call-template name="checkValueMatch">
							<xsl:with-param name="CSLValue" select="../value/node()"/>
							<xsl:with-param name="value" select="$value"/>
							<xsl:with-param name="seperator" select="','"/>
						</xsl:call-template>
					</xsl:variable>
					<xsl:if test="$valueMatch='true'">
						<xsl:attribute name="checked">checked</xsl:attribute>
					</xsl:if>
				</xsl:if>
				<xsl:attribute name="onclick">
					<xsl:text>showDependant('</xsl:text>
					<xsl:value-of select="translate(toggle/@case,'[]#=/','')"/>
					<xsl:text>-dependant','</xsl:text>
					<xsl:value-of select="$dependantClass"/>
					<xsl:text>');</xsl:text>
					<xsl:variable name="cmd">
						<xsl:text>$(&#34;input[type='text'][name='</xsl:text>
						<xsl:value-of select="$ref"/>
						<xsl:text>']&#34;).val('');alert('hi')'</xsl:text>
					</xsl:variable>
					
					<xsl:if test="following-sibling::item[toggle and @bindTo]">
						<xsl:text>clearRadioOther('</xsl:text>
						<xsl:value-of select="$ref"/>
						<xsl:text>');</xsl:text>
					</xsl:if>
				</xsl:attribute>
			</input>
			<label for="{$ref}_{position()}" class="form-check-label {translate(value/node(),'/ ','')}">
				<xsl:value-of select="label/node()"/>
			</label>
		</div>
		<!--<xsl:if test="contains($class,'multiline') and position()!=last()">
      <br/>
    </xsl:if>-->

	</xsl:template>



	<!-- Radio Input with dependant Case toggle and @bindTo -->
	<xsl:template match="item[label/node()='Other']" mode="xform_radiocheck">
		<xsl:param name="type"/>
		<xsl:param name="ref"/>
		<xsl:param name="dependantClass"/>
		<xsl:variable name="bindTo" select="@bindTo"/>
		<xsl:variable name="value">
			<xsl:choose>
				<xsl:when test="preceding-sibling::item[value/node()=../value/node()]">
					<xsl:text></xsl:text>				
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="../value/node()"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="class" select="../@class"/>
		<div>
			<xsl:attribute name="class">
				<xsl:text>form-check form-check-inline</xsl:text>
				<xsl:if test="contains($class,'multiline')">
					<xsl:text> multiline</xsl:text>
				</xsl:if>
			</xsl:attribute>
			<input type="{$type}" class="form-check-input">

				<xsl:attribute name="name">
					<xsl:value-of select="$ref"/>
				</xsl:attribute>
				<xsl:attribute name="id">
					<xsl:value-of select="$ref"/>_<xsl:value-of select="position()"/>
				</xsl:attribute>
				<xsl:attribute name="value">
					<xsl:value-of select="$value"/>
				</xsl:attribute>

				<!-- Check Radio adminButton is selected -->

				<xsl:if test="../value/node()=$value">
					<xsl:attribute name="checked">checked</xsl:attribute>
				</xsl:if>
				
				<!-- Check checkbox should be selected -->
				<xsl:if test="contains($class,'checkboxes')">
					<!-- Run through CSL to see if this should be checked -->
					<xsl:variable name="valueMatch">
						<xsl:call-template name="checkValueMatch">
							<xsl:with-param name="CSLValue" select="../value/node()"/>
							<xsl:with-param name="value" select="$value"/>
							<xsl:with-param name="seperator" select="','"/>
						</xsl:call-template>
					</xsl:variable>
					<xsl:if test="$valueMatch='true'">
						<xsl:attribute name="checked">checked</xsl:attribute>
					</xsl:if>
				</xsl:if>

				<xsl:attribute name="onclick">
					<xsl:text>clearRadioOther('</xsl:text>
					<xsl:value-of select="$ref"/>
					<xsl:text>','</xsl:text>
					<xsl:value-of select="position()"/>
					<xsl:text>' );</xsl:text>
				</xsl:attribute>
				<xsl:if test="ancestor::select1/item[1]/value/node() = $value">
					<xsl:attribute name="data-fv-notempty">
						<xsl:value-of select="ancestor::select1/@data-fv-notempty"/>
					</xsl:attribute>
					<xsl:attribute name="data-fv-notempty-message">
						<xsl:value-of select="ancestor::select1/@data-fv-notempty-message"/>
					</xsl:attribute>
				</xsl:if>
				<xsl:if test="ancestor::select/item[1]/value/node() = $value">
					<xsl:attribute name="data-fv-choice">
						<xsl:value-of select="ancestor::select/@data-fv-choice"/>
					</xsl:attribute>
					<xsl:attribute name="data-fv-choice-min">
						<xsl:value-of select="ancestor::select/@data-fv-choice-min"/>
					</xsl:attribute>
					<xsl:attribute name="data-fv-choice-max">
						<xsl:value-of select="ancestor::select/@data-fv-choice-max"/>
					</xsl:attribute>
					<xsl:attribute name="data-fv-choice-message">
						<xsl:value-of select="ancestor::select/@data-fv-choice-message"/>
					</xsl:attribute>
					<xsl:if test="ancestor::select/@data-fv-notempty">
						<xsl:attribute name="data-fv-notempty">
							<xsl:value-of select="ancestor::select/@data-fv-notempty"/>
						</xsl:attribute>
						<xsl:attribute name="data-fv-notempty-message">
							<xsl:value-of select="ancestor::select/@data-fv-notempty-message"/>
						</xsl:attribute>
					</xsl:if>
				</xsl:if>
			</input>
			<label for="{$ref}_{position()}" class="form-check-label {translate(value/node(),'/ ','')}">			
				&#160;
				<xsl:value-of select="label/node()"/>
			</label>
		</div>
		<xsl:variable name="showOther">
			<xsl:choose>
				<xsl:when test="preceding-sibling::item[value/node()=$value]">
					<xsl:text>hidden</xsl:text>				
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>text</xsl:text>	    
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		
		<input type="{$showOther}" name="{$ref}_other" id="{$ref}_other" value="{$value}" class="form-control form-inline short-input" />
	</xsl:template>

	<xsl:template match="label[ancestor::select[contains(@class,'content')] and Content]" mode="xform-label">
		<xsl:value-of select="Content/@name"/>&#160;<small>
			[<xsl:value-of select="Content/@type"/>]
		</small>
	</xsl:template>


	<xsl:template match="group[@class='modal-confirm']" mode="xform_control_script">
		<script>
			<!-- if  @showonchange id changes then we show on form submit-->
		</script>
	</xsl:template>


	<xsl:template match="group[@class='modal-confirm']" mode="xform">
		<xsl:param name="class"/>
		<div id="modal-confirm" class="modal fade" tabindex="-1">
			<xsl:if test=" @id!='' ">
				<xsl:attribute name="id">
					<xsl:value-of select="@id"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="$class!='' or @class!='' ">
				<xsl:attribute name="class">
					<xsl:value-of select="$class"/>
					<xsl:if test="@class!=''">
						<xsl:text> </xsl:text>
						<xsl:value-of select="@class"/>
					</xsl:if>
					<xsl:for-each select="group">
						<xsl:text> form-group li-</xsl:text>
						<xsl:value-of select="./@class"/>
					</xsl:for-each>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="label[position()=1]" mode="legend"/>
			<xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | group | repeat | hint | help | alert | div | repeat | relatedContent | label[position()!=1] | trigger | script" mode="control-outer"/>

			<button submits="the entire form"/>

		</div>
	</xsl:template>

	<xsl:template match="group[@class='redirect-modal']" mode="xform">
		<xsl:param name="class"/>
		<div id="redirectModal" class="redirectModal modal fade" tabindex="-1">

			<div class="modal-dialog">
				<div class="modal-content">
					<div class="modal-header">
						<label>Do you want to create a redirect?</label>
						<button type="button" class="close" data-dismiss="modal" >
							<span aria-hidden="true">&#215;</span>
						</button>
					</div>
					<div class="modal-body">
						<div class="form-group repeat-group ">
							<fieldset class="rpt-00 row">
								<div class="form-group input-containing col-md-6">
									<label>Old URL</label>
									<div class="control-wrapper input-wrapper appearance-">

										<input type="text" name="OldUrl" id="OldUrl" class="textbox form-control"/>
									</div>
								</div>
								<div class="form-group input-containing col-md-6">
									<label>New URL</label>
									<div class="control-wrapper input-wrapper appearance-">
										<input type="text" name="NewUrl" id="NewUrl" class="textbox form-control"/>
									</div>
								</div>
							</fieldset>
						</div>
						<div>
							<button type="submit" name="redirectType"  value="301Redirect" class="btn btn-primary btnRedirectSave" onclick="return RedirectClick(this.value);">301 Permanant Redirect</button>
							<button type="submit" name="redirectType"  value="302Redirect" class="btn btn-primary btnRedirectSave"  onclick="return RedirectClick(this.value);">302 Temporary Redirect</button>
							<button type="submit" name="redirectType"  value="404Redirect" class="btn btn-primary btnRedirectSave"  onclick="return RedirectClick(this.value);">404 Page Not Found</button>
						</div>

						<xsl:if test="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url!=''">
							<xsl:variable name="objOldUrl" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url" />
							<input name="pageOldUrl" type="hidden" value="{$objOldUrl}" class="hiddenOldUrl" />
						</xsl:if>
						<input name="productOldUrl" type="hidden" class="hiddenProductOldUrl" />
						<input name="productNewUrl" type="hidden" class="hiddenProductNewUrl" />
						<input name="IsParentPage" type="hidden" class="hiddenParentCheck" />
						<input name="pageId" type="hidden"  class="hiddenPageId" />
						<input name="type" type="hidden"  class="hiddenType" />
						<input  name="redirectOption" type="hidden" class="hiddenRedirectType" />
					</div>
				</div>
			</div>
		</div>

		<div id="RedirectionChildConfirmationModal" class="suitableForModal modal fade " tabindex="-1">

			<div class="modal-dialog">
				<div class="modal-content">
					<div class="modal-header">
						<button type="button" class="close" data-dismiss="modal" >
							<span aria-hidden="true">&#215;</span>
						</button>
					</div>
					<div class="modal-body">
						Current page have category/product pages beneath it, do you want to redirect them as well?
					</div>
					<div class="modal-footer">
						<button type="button" class="btn btn-primary" id="btnNocreateRuleForChild" >Cancel</button>
						<button type="button" id="btnYescreateRuleForChild" class="btn btn-primary">Yes </button>
					</div>
				</div>
				<input name="productOldUrl" type="hidden" class="hiddenProductOldUrl" />
				<input name="productNewUrl" type="hidden" class="hiddenProductNewUrl" />
				<input name="IsParent" type="hidden" class="hiddenParentCheck" />
				<input name="pageId" type="hidden"  class="hiddenPageId" />
				<input  name="redirectOption" type="textbox" class="hiddenRedirectType" />
			</div>
		</div>
	</xsl:template>


	<xsl:template match="submit[contains(@class,'getGeocodeButton')]" mode="xform">
		<xsl:variable name="class">
			<xsl:text>btn</xsl:text>
			<xsl:if test="not(contains(@class,'btn-'))">
				<xsl:text> btn-primary</xsl:text>
			</xsl:if>
			<xsl:if test="@class!=''">
				<xsl:text> </xsl:text>
				<xsl:value-of select="@class"/>
			</xsl:if>
		</xsl:variable>
		<xsl:variable name="name">
			<xsl:choose>
				<xsl:when test="@ref!=''">
					<xsl:value-of select="@ref"/>
				</xsl:when>
				<xsl:when test="@submission!=''">
					<xsl:value-of select="@submission"/>
				</xsl:when>
				<xsl:when test="@bind!=''">
					<xsl:value-of select="@bind"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>ewSubmit</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="icon">
			<xsl:choose>
				<xsl:when test="@icon!=''">
					<xsl:value-of select="@icon"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>fa-check</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="buttonValue">
			<xsl:choose>
				<xsl:when test="@value!=''">
					<xsl:value-of select="@value"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="label/node()"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="$GoogleAPIKey!=''">
				<button type="submit" name="{$name}" value="{$buttonValue}" class="{$class}"  onclick="disableButton(this);">
					<xsl:if test="@data-pleasewaitmessage != ''">
						<xsl:attribute name="data-pleasewaitmessage">
							<xsl:value-of select="@data-pleasewaitmessage"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:if test="@data-pleasewaitdetail != ''">
						<xsl:attribute name="data-pleasewaitdetail">
							<xsl:value-of select="@data-pleasewaitdetail"/>
						</xsl:attribute>
					</xsl:if>
					<xsl:if test="not(contains($class,'icon-right'))">
						<i class="fa {$icon} fa-white">
							<xsl:text> </xsl:text>
						</i>
						<xsl:text> </xsl:text>
					</xsl:if>
					<xsl:apply-templates select="label" mode="submitText"/>
					<xsl:if test="contains($class,'icon-right')">
						<xsl:text> </xsl:text>
						<i class="fa {$icon} fa-white">
							<xsl:text> </xsl:text>
						</i>
					</xsl:if>
				</button>
			</xsl:when>
			<xsl:otherwise>
				<div class="alert alert-warning">
					For geo-coding to work you require a Google API Key in the <a href="?ewCmd=WebSettings">config settings</a>
				</div>
			</xsl:otherwise>
		</xsl:choose>

	</xsl:template>

	<xsl:template match="submit[contains(@class,'getGeocodeButton')]" mode="xform_control_script">
		<script type="text/javascript" src="//maps.google.com/maps/api/js?key={$GoogleAPIKey}">&#160;</script>
	</xsl:template>

	<xsl:template match="group[contains(@class,'PermissionButtons')]" mode="xform">
		<xsl:param name="class"/>
		<fieldset>
			<xsl:if test=" @id!='' ">
				<xsl:attribute name="id">
					<xsl:value-of select="@id"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="$class!='' or @class!='' ">
				<xsl:attribute name="class">
					<xsl:value-of select="$class"/>
					<xsl:if test="@class!=''">
						<xsl:text> </xsl:text>
						<xsl:value-of select="@class"/>
					</xsl:if>
					<xsl:for-each select="group">
						<xsl:text> form-group li-</xsl:text>
						<xsl:value-of select="./@class"/>
					</xsl:for-each>
					<xsl:if test="contains(@class,'inline-2-col') or contains(@class,'inline-3-col')">
						<xsl:text> row</xsl:text>
					</xsl:if>
					<xsl:text> </xsl:text>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="label[position()=1]" mode="legend"/>
			<div class="permission-button-wrapper d-grid gap-2">
				<xsl:if test="not(submit[contains(@class,'hideRequired')])">
					<xsl:if test="ancestor::group/descendant-or-self::*[contains(@class,'required')]">
						<label class="required">
							<span class="req">*</span>
							<xsl:text> </xsl:text>
							<xsl:call-template name="msg_required"/>
						</label>
					</xsl:if>
				</xsl:if>
				<!-- For xFormQuiz change how these buttons work -->
				<xsl:apply-templates select="submit" mode="xform"/>
			</div>
		</fieldset>
	</xsl:template>


	<xsl:template match="alert[@class='item-deleted']" mode="xform">
		<div class="alert alert-warning">
			<span class="alert-msg">
				<xsl:if test="Content/@moduleType">
					Module Type: <xsl:value-of select="Content/@moduleType"/>
				</xsl:if>
			</span>
		</div>
	</xsl:template>

	<xsl:template match="group[contains(@class,'tabs')]" mode="xform">
		<div class="row form-tab-wrapper">
			<div class="col-1 col-lg-3 col-xl-2 form-tab-nav-wrapper">
				<div class=" nav flex-column nav-pills tab-nav" id="v-pills-tab" role="tablist" aria-orientation="vertical">
					<xsl:apply-templates select="*" mode="xform-tab-list"/>
				</div>
			</div>
			<div class="col-11 col-lg-9 col-xl-10 form-tab-content-wrapper">
				<div class="tab-content" id="tabs-{@ref}">
					<xsl:apply-templates select="*" mode="xform"/>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="group[parent::group[contains(@class,'tabs')]]" mode="xform-tab-list">
		<xsl:variable name="isopen">
			<xsl:if test="position()=1">
				<xsl:text>active</xsl:text>
			</xsl:if>
		</xsl:variable>
		<xsl:variable name="isclosed">
			<xsl:choose>
				<xsl:when test="position()=1">
					<xsl:text> true</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text> false</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<button class="nav-link {$isopen}" id="tab{position()}" data-bs-toggle="pill" data-bs-target="#heading{position()}" type="button" role="tab" aria-controls="heading{position()}" aria-selected="{$isclosed}">
			<xsl:apply-templates select="label">
				<xsl:with-param name="cLabel">
					<xsl:value-of select="@ref"/>
				</xsl:with-param>
			</xsl:apply-templates>
		</button>
	</xsl:template>
	<xsl:template match="group[parent::group[contains(@class,'tabs')]]" mode="xform">
		<xsl:variable name="isopen">
			<xsl:if test="position()=1">
				<xsl:text>show active</xsl:text>
			</xsl:if>
		</xsl:variable>
		<div id="heading{position()}" class="tab-pane fade {$isopen}" role="tabpanel" aria-labelledby="tab{position()}">
			<xsl:apply-templates select="*" mode="xform"/>
		</div>
	</xsl:template>

	<xsl:template match="div[@class='orderNotes']" mode="xform">
		<div>
		<xsl:if test="./@class">
			<xsl:attribute name="class">
				<xsl:value-of select="./@class"/>
			</xsl:attribute>
		</xsl:if>

		<div class="card">
			<div class="card-body">
			<xsl:for-each select="ul/li">
				<xsl:variable name="wordcount">
					<xsl:call-template name="word-count">
						<xsl:with-param name="data" select="node()"/>
						<xsl:with-param name="num" select="'0'"/>
					</xsl:call-template>
				</xsl:variable>
			<div>
				
				<xsl:choose>
					<xsl:when test="$wordcount > 20">
						<xsl:call-template name="firstWords">
							<xsl:with-param name="value" select="node()"/>
							<xsl:with-param name="count" select="'20'"/>
						</xsl:call-template>
						<a data-bs-toggle="collapse" href="#ordernote-{position()}" role="button" aria-expanded="false" aria-controls="collapseExample">
							&#160;more...
						</a>
						<div class="collapse" id="ordernote-{position()}">
							<xsl:apply-templates select="node()" mode="cleanXhtml"/>
						</div>
					</xsl:when>
					<xsl:otherwise>
						
							<xsl:apply-templates select="node()" mode="cleanXhtml"/>

					</xsl:otherwise>
				</xsl:choose>
	</div>
			
			</xsl:for-each></div>
		</div>
		</div>
	</xsl:template>

	<xsl:template match="group[@class='getFilterButtons']" mode="xform">
		<xsl:variable name="filterButtons">
			<xsl:call-template name="getFilterButtons"/>
		</xsl:variable>
		<xsl:variable name="thisGroup" select="."/>
		<div class="list-group">


			<xsl:for-each select="ms:node-set($filterButtons)/*/*">
				<xsl:variable name="buttonName" select="node()"/>
				<xsl:variable name="filterType" select="@filterType"/>

				<div class="list-group-item row">
					<div class="col-md-3">
						<label>
							<xsl:value-of select="$buttonName"/>
						</label>
					</div>
					<div class="col-md-6">
						<xsl:text> </xsl:text>
					</div>
					<div class="col-md-3">
						<xsl:text> </xsl:text>
						<xsl:choose>
							<xsl:when test="$thisGroup/ancestor::ContentDetail/Content/model/instance/ContentRelations/Content[@filterType=$filterType]">
								<xsl:variable name="relatedContent" select="concat('FilterEdit_',$filterType)" />
								<xsl:variable name="filterId" select="$thisGroup/ancestor::ContentDetail/Content/model/instance/ContentRelations/Content[@filterType=$filterType]/@id"/>

								<button type="submit" name="{concat('FilterRemove_',$filterType)}_{$filterId}" filtertype="{$buttonName}"  class="btn btn-sm btn-danger pull-right">
									<i class="fa fa-times">&#160;</i>&#160;Del
								</button>
								<button type="submit" name="{$relatedContent}_{$filterId}" filtertype="{$buttonName}"  class="btn btn-sm btn-primary pull-right">
									<i class="fa fa-edit">&#160;</i>&#160;Edit
								</button>
							</xsl:when>
							<xsl:otherwise>
								<xsl:variable name="relatedContent" select="concat('FilterAdd_',$filterType)" />
								<xsl:variable name="FilterType" select="concat($relatedContent,'_1Way_~inactive')" />
								<button type="submit" name="{$FilterType}" filtertype="{$buttonName}" class="btn btn-sm btn-primary pull-right">
									<i class="fa fa-plus">&#160;</i>&#160;
									Add
								</button>
							</xsl:otherwise>
						</xsl:choose>

					</div>

				</div>
			</xsl:for-each>
		</div>
	</xsl:template>

<!--
	<xsl:template match="group[contains(@class,'product-specs')]" mode="xform">
		<xsl:param name="class"/>
		<fieldset>
			<xsl:if test=" @id!='' ">
				<xsl:attribute name="id">
					<xsl:value-of select="@id"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="$class!='' or @class!='' ">
				<xsl:attribute name="class">
					<xsl:value-of select="$class"/>
					<xsl:if test="@class!=''">
						<xsl:text> </xsl:text>
						<xsl:value-of select="@class"/>
					</xsl:if>
					<xsl:for-each select="group">
						<xsl:text> form-group li-</xsl:text>
						<xsl:value-of select="./@class"/>
					</xsl:for-each>
					<xsl:if test="contains(@class,'inline-2-col') or contains(@class,'inline-3-col')">
						<xsl:text> row</xsl:text>
					</xsl:if>
					<xsl:text> </xsl:text>
				</xsl:attribute>
			</xsl:if>

			<xsl:variable name="catSpecs" select="ew:GetSpecsOnPageItems($page/@id,$page/@artid)"/>

			<H1>Product Specs</H1>
			<xsl:apply-templates select="label[position()=1]" mode="legend"/>			
			<xsl:apply-templates select="repeat | trigger" mode="control-outer"/>
			
		</fieldset>
	</xsl:template>
-->


	
</xsl:stylesheet>

<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
  <xsl:import href="../Tools/Functions.xsl"/>
  <xsl:import href="../xForms/xForms-bs-mininal.xsl"/>

  <xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml11.dtd" encoding="UTF-8"/>
  <xsl:template match="/">
    <xsl:apply-imports/>
  </xsl:template>

  <xsl:template match="Page">
    <html>
      <head>
        <title>EonicWeb Setup and Maintenance Tool</title>
	 <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
        <link rel="stylesheet" type="text/css" href="/ewcommon/css/base-bs.less" />
        <link type="text/css" rel="stylesheet" href="/ewcommon/css/admin/admin.less" />
        <script src="//ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.min.js">&#160;</script>
        <script src="//ajax.googleapis.com/ajax/libs/jqueryui/1.10.3/jquery-ui.min.js">&#160;</script>
        <script src="/ewcommon/bs3/js/bootstrap.js">&#160;</script>
        <script src="/ewcommon/js/common.js" type="text/javascript">&#160;</script>
	<script src="/ewcommon/js/ewAdmin.js" type="text/javascript">&#160;</script>
	</head>
        <body class="ewAdmin setup">
          <div id="adminHeader" class="affix-top navbar-fixed-top">
            <nav class="navbar navbar-inverse admin-main-menu" role="navigation">
              <div class="navbar-header">
                <button type="button" class="navbar-toggle" data-toggle="collapse" data-target="#bs-admin-navbar-collapse-1">
                  <span class="sr-only">Toggle navigation</span>
                  <span class="icon-bar">
                  </span>
                  <span class="icon-bar">
                  </span>
                  <span class="icon-bar">
                  </span>
                </button>
                <a class="navbar-brand" href="#">
                  <span class="admin-logo-text">
                    eonic<strong>web</strong>5
                  </span>
                  <span class="visible-xs xs-admin-switch">
                    <span class="sectionName">Setup Panel</span>
                    <span class="switch">
                      switch <i class="fa fa-ellipsis-v"></i>
                    </span>
                  </span>
                </a>
              </div>
              <ul class="nav navbar-nav">
			<xsl:apply-templates select="AdminMenu/MenuItem" mode="adminMenuItem"/>
	      </ul>
              <ul class="nav navbar-nav navbar-right">
                <li>
                  <a id="logoff" href="/?ewCmd=LogOff" title="Click here to log off from your active session">
                    <i class="fa fa-power-off">
                    </i> LOG OFF
                  </a>
                </li>
              </ul>
            </nav>
            <nav class="navbar navbar-inverse admin-sub-menu hidden-xs" role="navigation">
              <div class="navbar-header">
                <button class="navbar-toggle" type="button" data-toggle="collapse" data-target="#bs-admin-navbar-collapse-2">
                  <span class="sr-only">Toggle navigation</span>
                  <span class="icon-bar">
                  </span>
                  <span class="icon-bar">
                  </span>
                  <span class="icon-bar">
                  </span>
                </button>
                <a class="navbar-brand" href="#">
                  <span class="sectionName">Setup and Configuration</span>
                </a>
              </div>
              <div class="collapse navbar-collapse" id="bs-admin-navbar-collapse-2">
                <ul class="nav navbar-nav">
                  <xsl:apply-templates select="AdminMenu/MenuItem" mode="Menu3" />
                </ul>
                <ul class="nav navbar-nav navbar-right">
                  <li>
                  </li>
                </ul>
              </div>
            </nav>
			</div>
			<div id="adminLayout">
        <div class="container">
			    <xsl:apply-templates select="."  mode="SetupBody"/>
          <br/>
          <br/>
          <br/>
				</div>
			</div>
			<div id="footer">
				<div id="footerCopyright" class="container text-muted">
					© Eonic Associates LLP | 2002-2016
					T: +44 (0)1892 534044 |
					E: <a href="mailto:info@eonic.co.uk" title="Email Eonic">info@eonic.co.uk</a>
				</div>
			</div>
		</body>
    </html>
  </xsl:template>

  <xsl:template match="MenuItem/MenuItem" mode="adminMenuItem">
    <xsl:param name="level"/>
    <li>
      <xsl:apply-templates select="self::MenuItem" mode="adminLink1">
        <xsl:with-param name="level">
          <xsl:value-of select="$level"/>
        </xsl:with-param>
      </xsl:apply-templates>
    </li>
  </xsl:template>

  <xsl:template match="MenuItem" mode="adminLink1">
    <xsl:if test="@display='true'">
      <xsl:variable name="href">
        <xsl:text>?ewCmd=</xsl:text>
        <xsl:value-of select="@cmd"/>
        <xsl:if test="parent::MenuItem[@cmd!='AdmHome']">
          <xsl:text>&amp;pgid=</xsl:text>
          <xsl:value-of select="/Page/@id"/>
        </xsl:if>
      </xsl:variable>
      <a href="{$href}" title="{Description}">
        <xsl:choose>
          <xsl:when test="@cmd=/Page/@ewCmd">
            <xsl:attribute name="class">active</xsl:attribute>
          </xsl:when>
          <xsl:when test="descendant-or-self::MenuItem/@cmd=/Page/@ewCmd and @cmd!='AdmHome'">
            <xsl:attribute name="class">on</xsl:attribute>
          </xsl:when>
        </xsl:choose>
        <i class="fa {@icon}">&#160;</i>
        <span class="adminSubMenuText">
          <xsl:value-of select="@name"/>
        </span>
      </a>
    </xsl:if>
  </xsl:template>
  
  <xsl:template match="MenuItem" mode="adminLink">
    <xsl:if test="@display='true'">
      <a href="?ewCmd={@cmd}&amp;pgid={@pgid}" title="{Description}">
        <xsl:choose>
          <xsl:when test="self::MenuItem[@cmd=/Page/@ewCmd]">
            <xsl:attribute name="class">active</xsl:attribute>
          </xsl:when>
          <xsl:when test="descendant::MenuItem[@cmd=/Page/@ewCmd] and @cmd!='AdmHome'">
            <xsl:attribute name="class">on</xsl:attribute>
          </xsl:when>
        </xsl:choose>
        <span class="adminSubMenu1TL"> </span>
		<span class="adminSubMenu1Icon" id="adminSubMenu1IconWeb_Settings"> </span>
		<span class="adminSubMenuText">
			<xsl:value-of select="@name"/>
		</span>
      </a>
    </xsl:if>
  </xsl:template>

  <xsl:template match="AdminMenu/MenuItem/MenuItem/MenuItem" mode="Menu3">
    <xsl:if test="((@display='true') and (parent::MenuItem/@cmd=/Page/@ewCmd)) or (@cmd=/Page/@ewCmd) or (preceding-sibling::MenuItem/@cmd=/Page/@ewCmd) or (following-sibling::MenuItem/@cmd=/Page/@ewCmd)">
      <li>
        <a href="?ewCmd={@cmd}&amp;pgid={@pgid}" title="{Description}">
          <i class="fa {@icon}">&#160;</i>&#160;
          <xsl:value-of select="@name"/>
        </a>
      </li>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Page" mode="SetupBody">
    <div id="mainLayout">
      <div class="adminTemplate" id="template_1_Column">
        <div class="content">Please select the operation(s) you wish to perform</div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Page[@layout='AdminXForm']" mode="SetupBody">
      <div class="adminTemplate" id="template_AdminXForm">
        <xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
      </div>
  </xsl:template>

  <xsl:template match="Page[@ewCmd='Home']" mode="SetupBody">
    <div id="mainLayout">
      <div class="adminTemplate" id="template_1_Column">
        <div id="header">
          <h1>EonicWeb Setup and Maintenance</h1>
        </div>
        <div class="content">
          Please choose an action.
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Page[@ewCmd='Setup']" mode="SetupBody">
    <div id="mainLayout">
      <div class="adminTemplate" id="template_1_Column">
        <div id="header">
          <h1>EonicWeb Database Setup</h1>
        </div>
        <div class="content">
          Here you can:
          <ul>
            <li>
              <strong>Clear DB Structure:</strong> Remove all DB tables from the database.
            </li>
            <li>
              <strong>New DB Structure:</strong> Create new DB tables with only default data.
            </li>
            <li>
              <strong>Import V3 Data:</strong> Create new V5 tables and import V3 data.
            </li>
          </ul>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Page[@ewCmd='Maintenance']" mode="SetupBody">
    <div id="mainLayout">
      <div class="adminTemplate" id="template_1_Column">
        <div id="header">
          <h1>EonicWeb Database Maintenance</h1>
        </div>
        <div class="content">
          <h2>Here you can:</h2>
          <ul>
            <li>
              <strong>Upgrade Database with latest changes</strong> Ensure the database is in line with current version.
            </li>
            <li>
              <strong>Clean Audit Table:</strong> Removes all orphan audits.
            </li>
            <li>
              <strong>Import Content:</strong> Import new content to the database.
            </li>
            <li>
              <strong>Import V3 Data:</strong> Upgrade content schemas.
            </li>
          </ul>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Page[@ewCmd='ClearDB']" mode="SetupBody">
    <div id="mainLayout">
      <div class="adminTemplate" id="template_1_Column">
        <div id="header">
          <h1>Clear DB Structure</h1>
        </div>
        <div class="content">
          <xsl:choose>
            <xsl:when test="/Page/@Step=1">
              <h3>Complete</h3>
              <xsl:call-template name="ProgressResponses"/>
            </xsl:when>
            <xsl:when test="/Page/@Step=2">
              <h3>Error</h3>
              An error occured.
              <xsl:call-template name="ProgressResponses"/>
            </xsl:when>
            <xsl:otherwise>
              <p>Doing this will cause all DB tables and data to be removed. Are you sure you wish to continue?</p>
              <a href="/ewcommon/setup/default.ashx" class="btn btn-default">
                <i class="fa fa-times">&#160;</i>&#160;
                Cancel</a>
              &#160;&#160;&#160;
              <a href="/ewcommon/setup/default.ashx?ewCmd=ClearDB&amp;ewCmd2=Do" class="btn btn-danger">
                <i class="fa fa-warning">&#160;</i>&#160;Delete Database</a>
            </xsl:otherwise>
          </xsl:choose>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Page[@ewCmd='NewDatabase']" mode="SetupBody">
    <div id="mainLayout">
      <div class="adminTemplate" id="template_1_Column">
        <div id="header">
          <h1>Database Setup</h1>
        </div>
        <div class="content">
          <xsl:choose>
            <xsl:when test="/Page/@Step=1">
              <h3>Completed</h3>
              <xsl:call-template name="ProgressResponses"/>
            </xsl:when>
            <xsl:when test="/Page/@Step=2">
              <xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
              <!--<xsl:call-template name="ProgressResponses"/>-->
            </xsl:when>
            <xsl:otherwise>
              <p>Doing this will create all DB tables and default data. Are you sure you wish to continue?</p>
              <a href="/ewcommon/setup/default.ashx" class="btn btn-default">
                <i class="fa fa-times">&#160;</i>&#160;
                Cancel
              </a>
              &#160;&#160;&#160;
              <a href="/ewcommon/setup/default.ashx?ewCmd=NewV4&amp;ewCmd2=Do" class="btn btn-danger">
                <i class="fa fa-warning">&#160;</i>&#160;Build New Database
              </a>
            </xsl:otherwise>
          </xsl:choose>
        </div>
      </div>
    </div>
  </xsl:template>
  
  <xsl:template match="Page[@ewCmd='NewV4']" mode="SetupBody">
    <div id="mainLayout">
      <div class="adminTemplate" id="template_1_Column">
        <div id="header">
          <h1>New DB Structure</h1>
        </div>
        <div class="content">
          <xsl:choose>
            <xsl:when test="/Page/@Step=1">
              <h3>Completed</h3>
              <xsl:call-template name="ProgressResponses"/>
            </xsl:when>
            <xsl:when test="/Page/@Step=2">
              <h3>Error</h3>
              An error occured.
              <xsl:call-template name="ProgressResponses"/>
            </xsl:when>
            <xsl:otherwise>
              <p>Doing this will create all DB tables and default data. Are you sure you wish to continue?</p>
              <a href="/ewcommon/setup/default.ashx" class="btn btn-default">
                <i class="fa fa-times">&#160;</i>&#160;
                Cancel
              </a>
              &#160;&#160;&#160;
              <a href="/ewcommon/setup/default.ashx?ewCmd=NewV4&amp;ewCmd2=Do" class="btn btn-danger">
                <i class="fa fa-warning">&#160;</i>&#160;Build New Database</a>
            </xsl:otherwise>
          </xsl:choose>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Page[@ewCmd='ImportV3']" mode="SetupBody">
    <div id="mainLayout">
      <div class="adminTemplate" id="template_1_Column">
        <div id="header">
          <h1>Import V3 Data</h1>
        </div>
        <div class="content">
          <xsl:choose>
            <xsl:when test="/Page/@Step=1">
              <h3>Complete</h3>
              <xsl:call-template name="ProgressResponses"/>
            </xsl:when>
            <xsl:when test="/Page/@Step=2">
              <h3>Error</h3>
              An error occured.
              <xsl:call-template name="ProgressResponses"/>
            </xsl:when>
            <xsl:otherwise>
               <p> Doing this will create all DB tables and import V3 data. <br/>
              This may take a few minutes.<br/>
              <strong>Are you sure you wish to continue?</strong></p>
              <a href="/ewcommon/setup/default.ashx" class="btn btn-default">
                <i class="fa fa-times">&#160;</i>&#160;
                Cancel
              </a>
              &#160;&#160;&#160;
              <a href="/ewcommon/setup/default.ashx?ewCmd=ImportV3&amp;ewCmd2=Do"  class="btn btn-danger">
                <i class="fa fa-warning">&#160;</i>&#160;Import V3 Data</a>
            </xsl:otherwise>
          </xsl:choose>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Page[@ewCmd='UpgradeDB']" mode="SetupBody">
    <div id="mainLayout">
      <div class="adminTemplate" id="template_1_Column">
        <div id="header">
          <h1>Update EonicWeb Database</h1>
        </div>
        <div class="content">
          <xsl:choose>
            <xsl:when test="/Page/@Step=1">
              <h3>Running</h3>
              <xsl:call-template name="ProgressResponses"/>
            </xsl:when>
            <xsl:when test="/Page/@Step=2">
              <h3>Error</h3>
              An error occured.
              <xsl:call-template name="ProgressResponses"/>
            </xsl:when>
            <xsl:otherwise>
              <p>Doing this will update the database to the current version. <br/><br/><br/>
              <strong>Are you sure you wish to continue?</strong></p>
              <a href="/ewcommon/setup/default.ashx" class="btn btn-default">
                <i class="fa fa-times">&#160;</i>&#160;
                Cancel
              </a>
              &#160;&#160;&#160;
              <a href="/ewcommon/setup/default.ashx?ewCmd=UpgradeDB&amp;ewCmd2=Do"  class="btn btn-danger">
                <i class="fa fa-warning">&#160;</i>&#160;Upgrade Database</a>
            </xsl:otherwise>
          </xsl:choose>
        </div>
      </div>
    </div>
  </xsl:template>
	
	<xsl:template match="Page[@ewCmd='ShipLoc']" mode="SetupBody">
		<div id="mainLayout">
			<div class="adminTemplate" id="template_1_Column">
				<div id="header">
					<h1>Import Shipping Locations</h1>
				</div>
				<div class="content">
					<xsl:choose>
						<xsl:when test="/Page/@Step=1">
							<h3>Complete</h3>
							<xsl:call-template name="ProgressResponses"/>
						</xsl:when>
						<xsl:when test="/Page/@Step=2">
							<h3>Error</h3>
							An error occured.
							<xsl:call-template name="ProgressResponses"/>
						</xsl:when>
						<xsl:otherwise>
							<p>Doing this will replace all existing shipping locations <br/>
							Are you sure you wish to continue?</p>
              <a href="/ewcommon/setup/default.ashx" class="btn btn-default">
                <i class="fa fa-times">&#160;</i>&#160;
                Cancel
              </a>
							&#160;&#160;&#160;
							<a href="/ewcommon/setup/default.ashx?ewCmd=ShipLoc&amp;ewCmd2=Do" class="btn btn-danger">
                <i class="fa fa-warning">&#160;</i>&#160;Import Shipping Locations</a>
						</xsl:otherwise>
					</xsl:choose>
				</div>
			</div>
		</div>
	</xsl:template>

  <xsl:template match="Page[@ewCmd='CleanAudit']" mode="SetupBody">
    <div id="mainLayout">
      <div class="adminTemplate" id="template_1_Column">
        <div id="header">
          <h1>Clean Audit Table</h1>
        </div>
        <div class="content">
          <xsl:choose>
            <xsl:when test="/Page/@Step=1">
              <h3>Complete</h3>
              <xsl:call-template name="ProgressResponses"/>
            </xsl:when>
            <xsl:when test="/Page/@Step=2">
              <h3>Error</h3>
              An error occured.
              <xsl:call-template name="ProgressResponses"/>
            </xsl:when>
            <xsl:otherwise>
              <p>Doing this will clean out orphaned audit records. <br/>
              <strong>Are you sure you wish to continue?</strong><br/></p>
              <a href="/ewcommon/setup/default.ashx" class="btn btn-default">
                <i class="fa fa-times">&#160;</i>&#160;
                Cancel
              </a>
              &#160;&#160;&#160;
              <a href="/ewcommon/setup/default.ashx?ewCmd=CleanAudit&amp;ewCmd2=Do" class="btn btn-danger">
                <i class="fa fa-eraser">&#160;</i>&#160;Clean Audit Table</a>
            </xsl:otherwise>
          </xsl:choose>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Page[@ewCmd='OptimiseImages']" mode="SetupBody">
    <div id="mainLayout">
      <div class="adminTemplate" id="template_1_Column">
        <div id="header">
          <h1>Optimise Images</h1>
        </div>
        <div class="content">
          <xsl:choose>
            <xsl:when test="/Page/@Step=1">
              <h3>Complete</h3>
              <xsl:call-template name="ProgressResponses"/>
            </xsl:when>
            <xsl:when test="/Page/@Step=2">
              <h3>Error</h3>
              An error occured.
              <xsl:call-template name="ProgressResponses"/>
            </xsl:when>
            <xsl:otherwise>
              <p>
                Doing this will optimise all the images in the images folder. <br/>
                <strong>Are you sure you wish to continue?</strong><br/>
              </p>
              <a href="/ewcommon/setup/default.ashx" class="btn btn-danger">
                <i class="fa fa-times">&#160;</i>&#160;
                Cancel
              </a>
              &#160;&#160;&#160;
              <a href="/ewcommon/setup/default.ashx?ewCmd=OptimiseImages&amp;ewCmd2=Do" class="btn btn-success">
                <i class="fa fa-eraser">&#160;</i>&#160;Optimise Images
              </a>
            </xsl:otherwise>
          </xsl:choose>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Page[@ewCmd='UpgradeSchema']" mode="SetupBody">
	  <xsl:variable name="upgradetype" select="/Page/ContentDetail/Content/@upgradetype"/>
    <div id="mainLayout">
      <div class="adminTemplate" id="template_1_Column">
        <div id="header">

        </div>
        <div class="content">
          <xsl:choose>
            <xsl:when test="/Page/@Step=1">

				<form name="upgradetypeform" method="post" class="panel">

            <div class="panel-body">
				    <div class="form-group">
								<label>Switch Upgrade Type</label>
								<select name="upgradetype" onChange="this.form.submit();" class="form-control">
									<xsl:for-each select="/Page/ContentDetail/Content/type">
										<option>
											<xsl:if test="node()=$upgradetype">
												<xsl:attribute name="selected">
													<xsl:text>selected</xsl:text>
												</xsl:attribute>
											</xsl:if>
											<xsl:value-of select="node()"/>
										</option>
									</xsl:for-each>
								</select>
							</div>
            </div>
				</form>
				<Form name="upgradecontent" method="post" enctype="multipart/form-data" onSubmit="return form_check(this);" class="panel panel-primary">
					  <div class="panel-heading">
                    <h3 class="panel-title">Upgrade <xsl:value-of select="$upgradetype"/></h3>
          </div>
            <div class="panel-body">
						 <div class="form-group">
                      <label>Content Schema</label>
                      <select name="contentname" class="form-control">
                        <xsl:for-each select="/Page/ContentDetail/Content/option">
                          <option>
                            <xsl:value-of select="node()"/>
                          </option>
                        </xsl:for-each>
                      </select>
                  </div>
                 		 <div class="form-group">
                      <label>Full XSL</label>
                      <input type="file" name="fullxsl" id="fullxsl"  class="form-control"/>
                   </div>
                  <div class="form-group">
                      <label>Additional SQL</label>
                      <textarea name="moresql" cols="30" rows="5"  class="form-control"></textarea>
                  </div>
            <div class="alert alert-success">i.e.<br/>cContentXmlBrief like '%yourword%'</div>
		
					 <div class="form-group">
							<label><input type="checkbox" value="1" name="illegalChars">
								<xsl:attribute name="onClick">
									<xsl:text>
										<![CDATA[if(this.checked){this.form.save.checked=false;this.form.save.disabled=true;}else{this.form.save.disabled=false;}]]>
									</xsl:text>
								</xsl:attribute>
							</input>&#160;Test for illegal XML characters</label>
							
					 </div>
					 <div class="form-group">
								<label><input type="checkbox" value="1" name="showXML"/>&#160;Show Xml</label>
								
					 </div>

								<div class="alert alert-warning">If the set coming back is really really huge, we limit the response to the first 50 records.</div>
		
						 <div class="form-group">
                      <label><input type="checkbox" value="1" name="save"/>&#160;Save Changes?</label>
  
                      
			 </div>	     </div>
						 <div class="panel-footer">
                      <input type="hidden" value="1" name="checkme" />
                      <button type="submit" value="Upgrade Content" name="ewSubmit" onClick="disableButton(this);" class="btn btn-success pull-right">
             Upgrade Content&#160;<i class="fa fa-check">&#160;</i>         
             </button>
             <div class="clearfix">
               <xsl:text> </xsl:text>
             </div>
              </div>	
          
           
              </Form>
              <xsl:call-template name="ProgressResponses"/>
            </xsl:when>
            <xsl:when test="/Page/@Step=2">
              <h3>Error</h3>
              An error occured.
              <xsl:call-template name="ProgressResponses"/>
            </xsl:when>
			  <xsl:otherwise>
             <p> Doing this will upgrade content schemas. <br/>
          <strong>Are you sure you wish to continue?</strong><br/></p>
          <a href="/ewcommon/setup/default.ashx" class="btn btn-default">
            <i class="fa fa-times">&#160;</i>&#160;
            Cancel
          </a>
              &#160;&#160;&#160;
              <a href="/ewcommon/setup/default.ashx?ewCmd=UpgradeSchema&amp;ewCmd2=Do" class="btn btn-danger">
                <i class="fa fa-warning">&#160;</i>&#160;Upgrade Content / Directory</a>
            </xsl:otherwise>
          </xsl:choose>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Page[@ewCmd='ImportContent']" mode="SetupBody">
    <div id="mainLayout">
      <div class="adminTemplate" id="template_1_Column">
        <div id="header">
          <h1>Import Content</h1>
        </div>
        <div class="content">
          <xsl:choose>
            <xsl:when test="/Page/@Step=1">
              <Form name="upgradecontent" method="post" enctype="multipart/form-data">
                <label >
                  <strong>Import Information</strong>
                </label>
                <table>
                  <tr>
                    <td>
                      <label>Content Page</label>
                    </td>
                    <td>
                      <select name="contentid">
                        <xsl:for-each select="/Page/ContentDetail/Content/option[@class='page']">
                          <option value="{@value}">
                            <xsl:value-of select="node()"/>
                          </option>
                        </xsl:for-each>
                      </select>
                    </td>
                  </tr>
                  <tr>
                    <td>
                      <label>Content Schema</label>
                    </td>
                    <td>
                      <select name="contenttype">
                        <xsl:for-each select="/Page/ContentDetail/Content/option[@class='schema']">
                          <option>
                            <xsl:value-of select="node()"/>
                          </option>
                        </xsl:for-each>
                      </select>
                    </td>
                  </tr>
                  <tr>
                    <td>
                      <label>Xml Instances</label>
                    </td>
                    <td>
                      <input type="file" name="instancexml" id="instancexml" />
                    </td>
                  </tr>
                  <tr>
                    <td>
                      &#160;
                    </td>
                    <td>
                      <input type="submit" value="Import" name="submit" />
                    </td>
                  </tr>
                </table>
              </Form>
              <xsl:call-template name="ProgressResponses"/>
            </xsl:when>
            <xsl:when test="/Page/@Step=2">
              <h3>Error</h3>
              An error occured.
              <xsl:call-template name="ProgressResponses"/>
            </xsl:when>
            <xsl:otherwise>
              <p>Doing this will upgrade content schemas. <br/>
              <strong>Are you sure you wish to continue?</strong><br/></p>
              <a href="/ewcommon/setup/default.ashx" class="btn btn-default">
                <i class="fa fa-times">&#160;</i>&#160;
                Cancel
              </a>
              &#160;&#160;&#160;
              <a href="/ewcommon/setup/default.ashx?ewCmd=ImportContent&amp;ewCmd2=Do" class="btn btn-danger">
                <i class="fa fa-warning">&#160;</i>&#160;Continue</a>
            </xsl:otherwise>
          </xsl:choose>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template name="ProgressResponses">
	  <div id="result" class="panel panel-default">
    <xsl:if test="Contents/ProgressResponses/ProgressResponse">
      <div class="panel-body content" style="width: 100%; height: 400px; overflow-y: scroll; scrollbar-arrow-color: blue; scrollbar-face-color: #e7e7e7; scrollbar-3dlight-color: #a0a0a0; scrollbar-darkshadow-color:#888888">
        <xsl:for-each select="Contents/ProgressResponses/ProgressResponse">
          <p>
            <xsl:value-of select="node()" disable-output-escaping="yes"/>
          </p>
        </xsl:for-each>
      </div>
    </xsl:if>
     </div>
  </xsl:template>


  <xsl:template match="Page[@ewCmd='Backup']" mode="SetupBody">
    <div id="mainLayout">
      <div class="adminTemplate" id="template_1_Column">
        <div id="header">
          <h1>Backup Database</h1>
        </div>
        <div class="content">
          <xsl:choose>
            <xsl:when test="/Page/@Step=1">
              <h3>Complete</h3>
              <xsl:call-template name="ProgressResponses"/>
            </xsl:when>
            <xsl:when test="/Page/@Step=2">
              <xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
            </xsl:when>
            <xsl:otherwise>
              <p>Doing this will create a backup of the current website database. <br/>
              <strong>Are you sure you wish to continue?</strong></p>
              <a href="/ewcommon/setup/default.ashx" class="btn btn-default">
                <i class="fa fa-times">&#160;</i>&#160;
                Cancel
              </a>
              &#160;&#160;&#160;
              <a href="/ewcommon/setup/default.ashx?ewCmd=Backup&amp;ewCmd2=Do" class="btn btn-danger">
                <i class="fa fa-warning">&#160;</i>&#160;Backup Database
              </a>
            </xsl:otherwise>
          </xsl:choose>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Page[@ewCmd='Restore']" mode="SetupBody">
    <div id="mainLayout">
      <div class="adminTemplate" id="template_1_Column">
        <div id="header">
          <h1>Restore Database</h1>
        </div>
        <div class="content">
          <xsl:choose>
            <xsl:when test="/Page/@Step=1">
              <h3>Complete</h3>
              <xsl:call-template name="ProgressResponses"/>
            </xsl:when>
            <xsl:when test="/Page/@Step=2">
              <xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
            </xsl:when>
            <xsl:otherwise>
              <p>
                Doing this will restore the specificed database file and <strong>overwrite</strong> the current live data.<br/><br/>
                <strong>Are you sure you wish to continue?</strong>
              </p>
              <a href="/ewcommon/setup/default.ashx" class="btn btn-default">
                <i class="fa fa-times">&#160;</i>&#160;
                Cancel
              </a>
              &#160;&#160;&#160;
              <a href="/ewcommon/setup/default.ashx?ewCmd=Restore&amp;ewCmd2=Do" class="btn btn-danger">
                <i class="fa fa-warning">&#160;</i>&#160;Restore Database over current live data !
              </a>
            </xsl:otherwise>
          </xsl:choose>
        </div>
      </div>
    </div>
  </xsl:template>


  <xsl:template match="text()"></xsl:template>
</xsl:stylesheet>
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
	<xsl:import href="../core/functions.xsl"/>
	<xsl:import href="../core/localisation.xsl"/>
	<xsl:import href="../core/xforms.xsl"/>

	<xsl:output method="html" indent="yes" standalone="yes" omit-xml-declaration="yes" encoding="UTF-8"/>
	<xsl:template match="/">
		<xsl:text disable-output-escaping='yes'>&lt;!DOCTYPE html></xsl:text>
		<xsl:apply-imports/>
	</xsl:template>

	<xsl:template name="proteanCmsLogo">
		<xsl:choose>
			<xsl:when test="$page/Settings/add[@key='web.proteanCmsLogo']/@value!=''">
				<xsl:value-of select="$page/Settings/add[@key='web.proteanCmsLogo']/@value"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>/ptn/admin/skin/images/ptn-logo.png</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="proteanCmsProductName">
		<xsl:choose>
			<xsl:when test="$page/Settings/add[@key='web.proteanCmsProductName']/@value!=''">
				<xsl:value-of select="$page/Settings/add[@key='web.proteanCmsProductName']/@value"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>ProteanCMS</xsl:text>

			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="proteanCmsProductNameText">
		<xsl:choose>
			<xsl:when test="$page/Settings/add[@key='web.proteanCmsProductName']/@value!=''">
				<xsl:value-of select="$page/Settings/add[@key='web.proteanCmsProductName']/@value"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>ProteanCMS</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="proteanCmsCMSName">
		<xsl:choose>
			<xsl:when test="$page/Settings/add[@key='web.proteanCmsCMSName']/@value!=''">
				<xsl:value-of select="$page/Settings/add[@key='web.proteanCmsCMSName']/@value"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="proteanCmsProductName"/>
				<xsl:text> - Content Management System</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="proteanCmsAdminSystemName">
		<xsl:choose>
			<xsl:when test="$page/Settings/add[@key='web.proteanCmsAdminSystemName']/@value!=''">
				<xsl:value-of select="$page/Settings/add[@key='web.proteanCmsAdminSystemName']/@value"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="proteanCmsProductName"/>
				<xsl:text> admin system</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="proteanCmsCopyright">
		<xsl:choose>
			<xsl:when test="$page/Settings/add[@key='web.proteanCmsCopyright']/@value!=''">
				<xsl:value-of select="$page/Settings/add[@key='web.proteanCmsCopyright']/@value"/>
			</xsl:when>
			<xsl:otherwise>
				Eonic Digital LLP.
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="proteanCmsSupportTelephone">
		<xsl:choose>
			<xsl:when test="$page/Settings/add[@key='web.proteanCmsSupportTelephone']/@value!=''">
				<xsl:value-of select="$page/Settings/add[@key='web.proteanCmsSupportTelephone']/@value"/>
			</xsl:when>
			<xsl:otherwise>
				+44 (0)1892 534044
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="proteanCmsWebsite">
		<xsl:choose>
			<xsl:when test="$page/Settings/add[@key='web.proteanCmsWebsite']/@value!=''">
				<xsl:value-of select="$page/Settings/add[@key='web.proteanCmsWebsite']/@value"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>www.proteancms.com</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="proteanCmsSupportEmail">
		<xsl:choose>
			<xsl:when test="$page/Settings/add[@key='web.proteanCmsSupportEmail']/@value!=''">
				<xsl:value-of select="$page/Settings/add[@key='web.proteanCmsSupportEmail']/@value"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>support@eonic.co.uk</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>


	<xsl:variable name="CMSName">
		<xsl:call-template name="proteanCmsProductName"/>
	</xsl:variable>

	<xsl:variable name="BaseUrl">
		<xsl:call-template name="getSettings">
			<xsl:with-param name="sectionName" select="'web'"/>
			<xsl:with-param name="valueName" select="'BaseUrl'"/>
		</xsl:call-template>
	</xsl:variable>

	<xsl:template match="Page">
		<xsl:variable name="CMSLogo">
			<xsl:call-template name="proteanCmsLogo"/>
		</xsl:variable>
		<html>
			<head>
				<title>
					<xsl:call-template name="proteanCmsProductName"/> Setup and Maintenance Tool
				</title>
				<meta name="viewport" content="width=device-width, initial-scale=1" />
				<meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
				<link rel="stylesheet" type="text/css" href="/ptn/setup/setup.scss" />
			</head>
			<body class="ptn-edit setup layout-{@ewCmd}">
				<xsl:if test="AdminMenu/MenuItem">
					<div id="adminHeader" class="affix-top navbar-fixed-top">

						<!-- MAIN MENU -->
						<nav class="navbar navbar-expand-xl navbar-dark admin-main-menu" role="navigation">

							<div class="container-fluid">
								<div class="navbar-brand">
									<div class="navbar-logo">
										<img src="{$CMSLogo}" alt="{$CMSName}" class="cms-logo-dd"/>
									</div>
								</div>
								<div class="collapse navbar-collapse" id="bs-admin-navbar-collapse-1">
									<ul class="nav navbar-nav nav-add-more-auto ">
										<xsl:apply-templates select="AdminMenu/MenuItem" mode="adminMenuItem"/>
									</ul>
								</div>
								<xsl:if test="User">
									<div class="preview">
										<li>
											<a id="logoff" href="/?ewCmd=LogOff" title="Click here to log off from your active session">
												<i class="fa fa-power-off">
												</i> SIGN OUT
											</a>
										</li>
									</div>
								</xsl:if>
							</div>
						</nav>
					</div>
				</xsl:if>
				<div id="adminLayout" class="mt-5">
					<div class="container">
						<xsl:apply-templates select="."  mode="SetupBody"/>
						<br/>
						<br/>
						<br/>
					</div>
				</div>
				<div id="footer" class="fixed-bottom">
					<div id="footerCopyright" class="container text-muted">
						© Eonic Digital LLP | 2002-2021
						T: +44 (0)1892 534044 |
						E: <a href="mailto:support@eonic.co.uk" title="Email Eonic">support@eonic.co.uk</a>
						W: <a href="http://www.proteancms.com">www.proteancms.com</a>
					</div>
				</div>
				<script type="text/javascript" src="/ptn/libs/jquery/dist/jquery.min.js">&#160;</script>
				<script type="text/javascript" src="/ptn/libs/bs5/js/bootstrap.min.js">&#160;</script>
				<script type="text/javascript" src="/ptn/setup/setup.js">&#160;</script>
			</body>
		</html>
	</xsl:template>

	<xsl:template match="MenuItem/MenuItem" mode="adminMenuItem">
		<xsl:param name="level"/>
		<li>
			<xsl:attribute name="class">nav-item dropdown</xsl:attribute>
			<xsl:apply-templates select="self::MenuItem" mode="adminLink1">
				<xsl:with-param name="level">
					<xsl:value-of select="$level"/>
				</xsl:with-param>
			</xsl:apply-templates>
			<ul class="dropdown-menu">
				<xsl:apply-templates select="MenuItem" mode="adminLinkdd" />
			</ul>
		</li>
		<!--<nav class="navbar navbar-inverse admin-sub-menu hidden-xs" role="navigation">
									<div class="collapse navbar-collapse" id="bs-admin-navbar-collapse-2">
										<ul class="nav navbar-nav">
											<xsl:apply-templates select="AdminMenu/MenuItem" mode="Menu3" />
										</ul>

									</div>
								</nav>-->
			
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
				<i class="fas {@icon}">&#160;</i>
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

	<xsl:template match="MenuItem" mode="adminLinkdd">
		<xsl:if test="@display='true'">
			<li>
				<a href="?ewCmd={@cmd}&amp;pgid={@pgid}" title="{Description}" style="display:block">
					<xsl:choose>
						<xsl:when test="self::MenuItem[@cmd=/Page/@ewCmd]">
							<xsl:attribute name="class">active</xsl:attribute>
						</xsl:when>
						<xsl:when test="descendant::MenuItem[@cmd=/Page/@ewCmd] and @cmd!='AdmHome'">
							<xsl:attribute name="class">on</xsl:attribute>
						</xsl:when>
					</xsl:choose>
					<span class="adminSubMenuText">
						<xsl:value-of select="@name"/>
					</span>
				</a>
			</li>
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
		<div id="mainLayout" class="mt-5">
			<div class="adminTemplate" id="template_1_Column">
				<div class="content">Please select the operation(s) you wish to perform</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Page[@layout='AdminXForm' and ContentDetail/Content[@name='UserLogon']]" mode="SetupBody">
		<div class="admin-body" id="template_AdminXForm">
			<span class="text-light logo-text login-logo">
				<img src="/ptn/admin/skin/images/ptn-logo.png" alt="ProteanCMS" class="cms-logo-dd"/>
				<strong>Protean</strong>CMS
			</span>
			<div class="card">
				<div class="card-header">
					<h5>Log in</h5>
				</div>
				<div class="card-body">
					<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
					<xsl:apply-templates select="ContentDetail/Content[contains(@type,'xFormQuiz')]" mode="edit"/>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Page[@layout='AdminXForm' and ContentDetail/Content[@name='WebSettings']]" mode="SetupBody">
		<div class="adminTemplate" id="template_AdminXForm">
			<br/>
			<br/>
			<h2>
				Welcome to your <xsl:call-template name="proteanCmsProductName"/> Setup
			</h2>
			<div class="row">
				<div class="col-md-6">
					<h4>
						<xsl:call-template name="proteanCmsProductName"/> requires access to a MS SQL Server Database.
					</h4>
					<p>Before it will run you need to give it the connection details.</p>
					<p>The servername will be hostname of the server hosting SQL Server. </p>
					<p>If the DB Name specified does not exist it will be created as long as the username and password have sufficent access rights to do so.</p>
					<p>If for any reason this step fails you can edit these settings directly in the /eonic.web.config file.</p>
				</div>
				<div class="col-md-6">
					<div class="card">
						<div class="card-body">
							<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
						</div>
					</div>
				</div>
			</div>

		</div>
	</xsl:template>

	<xsl:template match="Page[@ewCmd='Home']" mode="SetupBody">
		<div id="mainLayout" >
			<div class="adminTemplate" id="template_1_Column">
				<div id="header">
					<h1>
						<xsl:call-template name="proteanCmsProductName"/> Setup and Maintenance
					</h1>
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
					<h1>
						<xsl:call-template name="proteanCmsProductName"/> Database Setup
					</h1>
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

					</ul>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Page[@ewCmd='Maintenance']" mode="SetupBody">
		<div id="mainLayout">
			<div class="adminTemplate" id="template_1_Column">
				<div id="header">
					<h1>Database Maintenance</h1>
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
							<xsl:call-template name="ProgressResponses"/>
						</xsl:when>
						<xsl:when test="/Page/@Step=2">
							<h3>Error</h3>
							An error occured.
							<xsl:call-template name="ProgressResponses"/>
						</xsl:when>
						<xsl:otherwise>
							<p>Doing this will cause all DB tables and data to be removed. Are you sure you wish to continue?</p>
							<a href="/ptn/setup/default.ashx" class="btn btn-default">
								<i class="fa fa-times">&#160;</i>&#160;
								Cancel
							</a>
							&#160;&#160;&#160;
							<a href="/ptn/setup/default.ashx?ewCmd=ClearDB&amp;ewCmd2=Do" class="btn btn-danger">
								<i class="fa fa-warning">&#160;</i>&#160;Delete Database
							</a>
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
							<xsl:call-template name="ProgressResponses"/>
						</xsl:when>
						<xsl:when test="/Page/@Step=2">
							<div class="card card-default">
								<div class="card-body">
									<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
								</div>
							</div>
							<!--<xsl:call-template name="ProgressResponses"/>-->
						</xsl:when>
						<xsl:otherwise>
							<p>Doing this will create all DB tables and default data. Are you sure you wish to continue?</p>
							<a href="/ptn/setup/default.ashx" class="btn btn-default">
								<i class="fa fa-times">&#160;</i>&#160;
								Cancel
							</a>
							&#160;&#160;&#160;
							<a href="/ptn/setup/default.ashx?ewCmd=NewV4&amp;ewCmd2=Do" class="btn btn-danger">
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
							<xsl:call-template name="ProgressResponses"/>
						</xsl:when>
						<xsl:when test="/Page/@Step=2">
							<h3>Error</h3>
							An error occured.
							<xsl:call-template name="ProgressResponses"/>
						</xsl:when>
						<xsl:otherwise>
							<p>Doing this will create all DB tables and default data. Are you sure you wish to continue?</p>
							<a href="/ptn/setup/default.ashx" class="btn btn-default">
								<i class="fa fa-times">&#160;</i>&#160;
								Cancel
							</a>
							&#160;&#160;&#160;
							<a href="/ptn/setup/default.ashx?ewCmd=NewV4&amp;ewCmd2=Do" class="btn btn-danger">
								<i class="fa fa-warning">&#160;</i>&#160;Build New Database
							</a>
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
					<h1>Update Database</h1>
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
							<p>
								Doing this will update the database to the current version. <br/><br/><br/>
								<strong>Are you sure you wish to continue?</strong>
							</p>
							<a href="/ptn/setup/default.ashx" class="btn btn-default">
								<i class="fa fa-times">&#160;</i>&#160;
								Cancel
							</a>
							&#160;&#160;&#160;
							<a href="/ptn/setup/default.ashx?ewCmd=UpgradeDB&amp;ewCmd2=Do"  class="btn btn-danger">
								<i class="fa fa-warning">&#160;</i>&#160;Upgrade Database
							</a>
						</xsl:otherwise>
					</xsl:choose>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Page[@ewCmd='RunTests']" mode="SetupBody">
		<div id="mainLayout">
			<div class="adminTemplate" id="template_1_Column">
				<div id="header">
					<h1>Run Tests</h1>
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
							<p>
								Test various features of the ProteanCMS Platform. <br/><br/><br/>
								<strong>Are you sure you wish to continue?</strong>
							</p>
							<a href="/ptn/setup/default.ashx" class="btn btn-default">
								<i class="fa fa-times">&#160;</i>&#160;
								Cancel
							</a>
							&#160;&#160;&#160;
							<a href="/ptn/setup/default.ashx?ewCmd=RunTests&amp;ewCmd2=Do"  class="btn btn-danger">
								<i class="fa fa-warning">&#160;</i>&#160;Run Tests
							</a>
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
							<p>
								Doing this will replace all existing shipping locations <br/>
								Are you sure you wish to continue?
							</p>
							<a href="/ptn/setup/default.ashx" class="btn btn-default">
								<i class="fa fa-times">&#160;</i>&#160;
								Cancel
							</a>
							&#160;&#160;&#160;
							<a href="/ptn/setup/default.ashx?ewCmd=ShipLoc&amp;ewCmd2=Do" class="btn btn-danger">
								<i class="fa fa-warning">&#160;</i>&#160;Import Shipping Locations
							</a>
						</xsl:otherwise>
					</xsl:choose>
				</div>
			</div>
		</div>
	</xsl:template>


  <xsl:template match="Page[@ewCmd='CleanDatabase']" mode="SetupBody">
    <div id="mainLayout">
      <div class="adminTemplate" id="template_1_Column">
        <div id="header">
          <h1>Clean Database</h1>
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
                Doing this will clean out unused records related to cart,promocode. <br/>
                <strong>Are you sure you wish to continue?</strong><br/>
              </p>
              <a href="/ptn/setup/default.ashx" class="btn btn-default">
                <i class="fa fa-times">&#160;</i>&#160;
                Cancel
              </a>
              &#160;&#160;&#160;
              <a href="/ptn/setup/default.ashx?ewCmd=CleanDatabase&amp;ewCmd2=Do" class="btn btn-danger">
                <i class="fa fa-eraser">&#160;</i>&#160;Clean Database
              </a>
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
							<p>
								Doing this will clean out orphaned audit records. <br/>
								<strong>Are you sure you wish to continue?</strong><br/>
							</p>
							<a href="/ptn/setup/default.ashx" class="btn btn-default">
								<i class="fa fa-times">&#160;</i>&#160;
								Cancel
							</a>
							&#160;&#160;&#160;
							<a href="/ptn/setup/default.ashx?ewCmd=CleanAudit&amp;ewCmd2=Do" class="btn btn-danger">
								<i class="fa fa-eraser">&#160;</i>&#160;Clean Audit Table
							</a>
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
							<a href="/ptn/setup/default.ashx" class="btn btn-danger">
								<i class="fa fa-times">&#160;</i>&#160;
								Cancel
							</a>
							&#160;&#160;&#160;
							<a href="/ptn/setup/default.ashx?ewCmd=OptimiseImages&amp;ewCmd2=Do" class="btn btn-success">
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

							<form name="upgradetypeform" method="post" class="xform card  mb-3">
								<div class="card-body">
									<div class="form-group">
										<label class="form-label">Switch Upgrade Type</label>
										<select name="upgradetype" onChange="this.form.submit();" class="form-select">
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
							<form name="upgradecontent" method="post" enctype="multipart/form-data" onSubmit="return form_check(this);" class="card card-primary mb-3">
								<div class="card-header">
									<h5 class="card-title">
										Upgrade <xsl:value-of select="$upgradetype"/>
									</h5>
								</div>
								<div class="card-body">
									<div class="form-group mb-2">
										<label class="form-label">Content Schema</label>
										<select name="contentname" class="form-control">
											<xsl:for-each select="/Page/ContentDetail/Content/option">
												<option>
													<xsl:value-of select="node()"/>
												</option>
											</xsl:for-each>
										</select>
									</div>
									<div class="form-group mb-2">
										<label class="form-label">Full XSL</label>
										<input type="file" name="fullxsl" id="fullxsl"  class="form-control"/>
									</div>
									<div class="form-group mb-2">
										<label class="form-label">Additional SQL</label>
										<textarea name="moresql" cols="30" rows="5"  class="form-control"></textarea>
									</div>
									<div class="alert alert-success">
										i.e.<br/>cContentXmlBrief like '%yourword%'
									</div>

									<div class="form-check mb-2">
										<input class="form-check-input" type="checkbox" value="1" name="illegalChars">
											<xsl:attribute name="onClick">
												<xsl:text>
										<![CDATA[if(this.checked){this.form.save.checked=false;this.form.save.disabled=true;}else{this.form.save.disabled=false;}]]>
									</xsl:text>
											</xsl:attribute>
										</input>
										<label class="form-check-label">Test for illegal XML characters</label>

									</div>
									<div class="form-check mb-2">
										<input class="form-check-input" type="checkbox" value="1" name="showXML"/>
										<label class="form-check-label" for="showXML">Show Xml</label>
									</div>
									<div class="alert alert-warning">If the set coming back is really really huge, we limit the response to the first 50 records.</div>
									<div class="form-check mb-2">
										<input class="form-check-input" type="checkbox" value="1" name="save"/>
										<label class="form-check-label" for="save">&#160;Save Changes?</label>
									</div>
								</div>
								<div class="card-footer">
									<input type="hidden" value="1" name="checkme" />
									<button type="submit" value="Upgrade Content" name="ewSubmit" onClick="disableButton(this);" class="btn btn-success float-end">
										Upgrade Content&#160;<i class="fa fa-check">&#160;</i>
									</button>
									<div class="clearfix">
										<xsl:text> </xsl:text>
									</div>
								</div>


							</form>
							<xsl:call-template name="ProgressResponses"/>
						</xsl:when>
						<xsl:when test="/Page/@Step=2">
							<h3>Error</h3>
							An error occured.
							<xsl:call-template name="ProgressResponses"/>
						</xsl:when>
						<xsl:otherwise>
							<p>
								Doing this will upgrade content schemas. <br/>
								<strong>Are you sure you wish to continue?</strong><br/>
							</p>
							<a href="/ptn/setup/default.ashx" class="btn btn-default">
								<i class="fa fa-times">&#160;</i>&#160;
								Cancel
							</a>
							&#160;&#160;&#160;
							<a href="/ptn/setup/default.ashx?ewCmd=UpgradeSchema&amp;ewCmd2=Do" class="btn btn-danger">
								<i class="fa fa-warning">&#160;</i>&#160;Upgrade Content / Directory
							</a>
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
							<form name="upgradecontent" method="post" enctype="multipart/form-data" class="card card-primary">
								<div class="card-header">
									<h5>Import Information</h5>
								</div>
								<div class="card-body">
									<div class="mb-3">
										<label  class="form-label">Content Page</label>

										<select name="contentid" class="form-select">
											<xsl:for-each select="/Page/ContentDetail/Content/option[@class='page']">
												<option value="{@value}">
													<xsl:value-of select="node()"/>
												</option>
											</xsl:for-each>
										</select>
									</div>
									<div class="mb-3">
										<label class="form-label">Content Schema</label>

										<select name="contenttype" class="form-select">
											<xsl:for-each select="/Page/ContentDetail/Content/option[@class='schema']">
												<option>
													<xsl:value-of select="node()"/>
												</option>
											</xsl:for-each>
										</select>
									</div>
									<div class="mb-3">
										<label class="form-label">Xml Instances</label>

										<input type="file" name="instancexml" id="instancexml"  class="form-control"/>
									</div>
								</div>
								<div class="card-footer">
									<input type="submit" value="Import" name="submit" class="btn btn-success float-end"/>
								</div>

							</form>
							<xsl:call-template name="ProgressResponses"/>
						</xsl:when>
						<xsl:when test="/Page/@Step=2">
							<h3>Error</h3>
							An error occured.
							<xsl:call-template name="ProgressResponses"/>
						</xsl:when>
						<xsl:otherwise>
							<p>
								Doing this will upgrade content schemas. <br/>
								<strong>Are you sure you wish to continue?</strong><br/>
							</p>
							<a href="/ptn/setup/default.ashx" class="btn btn-default">
								<i class="fa fa-times">&#160;</i>&#160;
								Cancel
							</a>
							&#160;&#160;&#160;
							<a href="/ptn/setup/default.ashx?ewCmd=ImportContent&amp;ewCmd2=Do" class="btn btn-danger">
								<i class="fa fa-warning">&#160;</i>&#160;Continue
							</a>
						</xsl:otherwise>
					</xsl:choose>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template name="ProgressResponses">
		<div class="card card-default mt-3">
			<div id="result" class="card-body content" style="width: 100%; height: 400px; overflow-y: scroll; scrollbar-arrow-color: blue; scrollbar-face-color: #e7e7e7; scrollbar-3dlight-color: #a0a0a0; scrollbar-darkshadow-color:#888888">
				<xsl:for-each select="Contents/ProgressResponses/ProgressResponse">
					<p>
						<xsl:value-of select="node()" disable-output-escaping="yes"/>
					</p>
				</xsl:for-each>
				<xsl:text> </xsl:text>
			</div>
		</div>

		<!-- Modal -->
		<div class="modal fade" id="completeModal" tabindex="-1" role="dialog" aria-labelledby="completeModalLabel">
			<div class="modal-dialog" role="document">
				<div class="modal-content">
					<div class="modal-header">
						<button type="button" class="close" data-bs-dismiss="modal" aria-label="Close">
							<span aria-hidden="true">
								<i class="fa fa-times">&#160;</i>
							</span>
						</button>
						<h4 class="modal-title" id="completeModal">Completed</h4>
					</div>
					<div class="modal-body">
						<a id="completeButton" href="/" class="btn btn-success btn-lg btn-block">Go to your new website</a>
					</div>
				</div>
			</div>
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
							<p>
								Doing this will create a backup of the current website database. <br/>
								<strong>Are you sure you wish to continue?</strong>
							</p>
							<a href="/ptn/setup/default.ashx" class="btn btn-default">
								<i class="fa fa-times">&#160;</i>&#160;
								Cancel
							</a>
							&#160;&#160;&#160;
							<a href="/ptn/setup/default.ashx?ewCmd=Backup&amp;ewCmd2=Do" class="btn btn-danger">
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
							<a href="/ptn/setup/default.ashx" class="btn btn-default">
								<i class="fa fa-times">&#160;</i>&#160;
								Cancel
							</a>
							&#160;&#160;&#160;
							<a href="/ptn/setup/default.ashx?ewCmd=Restore&amp;ewCmd2=Do" class="btn btn-danger">
								<i class="fa fa-warning">&#160;</i>&#160;Restore Database over current live data !
							</a>
						</xsl:otherwise>
					</xsl:choose>
				</div>
			</div>
		</div>
	</xsl:template>
 <xsl:template match="Page[@ewCmd='GitRepository']" mode="SetupBody">

    <div id="mainLayout">
      <div class="adminTemplate" id="template_1_Column">
        <div id="header">
          <h1>Git Operations</h1>
        </div>
        <div class="content">
          <xsl:choose>
            <xsl:when test="/Page/@Step=1">
              <h3>Complete</h3>
              <div class="content">
                <xsl:call-template name="ProgressResponses"/>
              </div>
            </xsl:when>
            <xsl:when test="/Page/@Step=2">
              <h3>Error</h3>
              An error occured.
              <div class="content">
                <xsl:call-template name="ProgressResponses"/>
              </div>
            </xsl:when>
            <xsl:otherwise>
              
              <div class="form-group panel-body">
                <div>
                  <p>
                    Proceeding will pull the latest Git changes for the specified branch.<br/>
                    
                  </p>
                </div>
                <h3>Available Files</h3>
                <table class="table table-bordered table-striped">
                  <thead>
                    <tr>
                      <th>File Name</th>
                      <th>Action</th>
                    </tr>
                  </thead>
                  <tbody>
                    <xsl:for-each select="/Page/files/file">
                      <tr>
                        <td>
                          <xsl:value-of select="@name"/>
                        </td>
                        <td>
                          <a href="/ptn/setup/default.ashx?ewCmd=GitRepository&amp;ewCmd2=Run&amp;GitPS1FilePath={@path}" class="btn btn-primary btn-sm">
                            <i class="fa fa-play"></i>&#160;Run
                          </a>
                        </td>
                      </tr>
                    </xsl:for-each>
                  </tbody>
                </table>
              </div>
             
              &#160;&#160;&#160;
            </xsl:otherwise>
          </xsl:choose>
        </div>
      </div>
    </div>
  </xsl:template>

	<xsl:template match="text()"></xsl:template>
</xsl:stylesheet>
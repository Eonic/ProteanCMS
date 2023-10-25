<?xml version="1.0" encoding="UTF-8"?>
<!--<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
				xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" 
				xmlns="http://www.w3.org/1999/xhtml"  xmlns:ew="urn:ew" 
				xmlns:v-if="http://example.com/xml/v-if" xmlns:v-on="http://example.com/xml/v-on">-->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl ew"
                xmlns:v-bind="http://example.com/xml/v-bind" xmlns:v-on="http://example.com/xml/v-on"
                xmlns:v-for="http://example.com/xml/v-for" xmlns:v-slot="http://example.com/xml/v-slot"
                xmlns:v-if="http://example.com/xml/v-if" xmlns:v-else="http://example.com/xml/v-else"
                xmlns:v-model="http://example.com/xml/v-model" xmlns:ew="urn:ew">

	<xsl:variable name="GoogleAPIKey">
		<xsl:value-of select="$page/Settings/add[@key='web.GoogleAPIKey']/@value"/>
	</xsl:variable>

	<xsl:template name="eonicwebProductName">
		<xsl:choose>
			<xsl:when test="$page/Settings/add[@key='web.eonicwebProductName']/@value!=''">
				<xsl:value-of select="$page/Settings/add[@key='web.eonicwebProductName']/@value"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>ProteanCMS</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="eonicwebCMSName">
		<xsl:choose>
			<xsl:when test="$page/Settings/add[@key='web.eonicwebCMSName']/@value!=''">
				<xsl:value-of select="$page/Settings/add[@key='web.eonicwebCMSName']/@value"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="eonicwebProductName"/>
				<xsl:text> - Content Management System</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="eonicwebAdminSystemName">
		<xsl:choose>
			<xsl:when test="$page/Settings/add[@key='web.eonicwebAdminSystemName']/@value!=''">
				<xsl:value-of select="$page/Settings/add[@key='web.eonicwebAdminSystemName']/@value"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="eonicwebProductName"/>
				<xsl:text> admin system</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="eonicwebCopyright">
		<xsl:choose>
			<xsl:when test="$page/Settings/add[@key='web.eonicwebCopyright']/@value!=''">
				<xsl:value-of select="$page/Settings/add[@key='web.eonicwebCopyright']/@value"/>
			</xsl:when>
			<xsl:otherwise>
				Eonic Digital LLP.
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="eonicwebSupportTelephone">
		<xsl:choose>
			<xsl:when test="$page/Settings/add[@key='web.eonicwebSupportTelephone']/@value!=''">
				<xsl:value-of select="$page/Settings/add[@key='web.eonicwebSupportTelephone']/@value"/>
			</xsl:when>
			<xsl:otherwise>
				+44 (0)1892 534044
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="eonicwebWebsite">
		<xsl:choose>
			<xsl:when test="$page/Settings/add[@key='web.eonicwebWebsite']/@value!=''">
				<xsl:value-of select="$page/Settings/add[@key='web.eonicwebWebsite']/@value"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>eonic.com</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="eonicwebSupportEmail">
		<xsl:choose>
			<xsl:when test="$page/Settings/add[@key='web.eonicwebSupportEmail']/@value!=''">
				<xsl:value-of select="$page/Settings/add[@key='web.eonicwebSupportEmail']/@value"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>support@eonic.com</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template name="eonicwebLogo">
		<xsl:choose>
			<xsl:when test="$page/Settings/add[@key='web.eonicwebLogo']/@value!=''">
				<xsl:value-of select="$page/Settings/add[@key='web.eonicwebLogo']/@value"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>/ewcommon/images/admin/skin/protean-admin-white.png</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:variable name="appPath" select="/Page/Request/ServerVariables/Item[@name='APPLICATION_ROOT']/node()"/>
	<!-- Used across this xsl to generate Admin menus and Breadcrumbs-->
	<xsl:variable name="subMenuCommand">
		<xsl:choose>
			<xsl:when test="/Page/@ewCmd!=''">
				<xsl:value-of select="/Page/@ewCmd"/>
				<xsl:text> </xsl:text>
			</xsl:when>
			<xsl:otherwise>Don't search for this</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

	<xsl:template name="getSiteURL">
		<xsl:choose>
			<xsl:when test="/Page/Cart/@siteURL!=''">
				<xsl:value-of select="/Page/Cart/@siteURL"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="getSettings">
					<xsl:with-param name="sectionName" select="'web'"/>
					<xsl:with-param name="valueName" select="'BaseUrl'"/>
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="Page[@adminMode='false']" mode="adminStyle">
		<xsl:if test="@cssFramework!='bs3'">
			<link type="text/css" rel="stylesheet" href="/ewcommon/css/non-bs-admin.less"/>
		</xsl:if>
		<link type="text/css" rel="stylesheet" href="/ewcommon/css/admin/adminNormal.less"/>

		<!-- IF IE6 BRING IN IE6 files -->
		<xsl:if test="contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 6.0') and not(contains(Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 7')) and not(contains(Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'Opera'))">
			<link type="text/css" rel="stylesheet" href="/ewcommon/css/admin/skins/ie6.css"/>
		</xsl:if>
	</xsl:template>

	<xsl:template match="Page[@adminMode='true']" mode="adminStyle">
		<base href="{$appPath}"/>
		<link type="text/css" rel="stylesheet" href="/ewcommon/css/admin/admin.less?v={$scriptVersion}"/>
		<!-- IF IE6 BRING IN IE6 files -->
		<xsl:if test="contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 6.0') and not(contains(Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 7')) and not(contains(Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'Opera'))">
			<link type="text/css" rel="stylesheet" href="/ewcommon/css/admin/skins/ie6.css?v={$scriptVersion}"/>
		</xsl:if>

		<!-- IF >IE9 BRING IN IE Scripts -->
		<xsl:if test="contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 8') or contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 7') or contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 6.0')">
			<script src="https://oss.maxcdn.com/libs/html5shiv/3.7.0/html5shiv.js"></script>
			<script src="/ewcommon/js/respond.min.js"></script>
		</xsl:if>
	</xsl:template>

	<xsl:template match="Page[@previewMode]" mode="adminStyle">
		<link type="text/css" rel="stylesheet" href="/ewcommon/css/admin/admin.css?v={$scriptVersion}"/>
		<link type="text/css" rel="stylesheet" href="/ewcommon/css/admin/skins/eonic.css?v={$scriptVersion}"/>

	</xsl:template>

	<xsl:template match="Page" mode="adminJs">
		<xsl:if test="ContentDetail/Content[@type='xform']/descendant::submit[contains(@class,'getGeocodeButton')]">
			<script type="text/javascript" src="//maps.google.com/maps/api/js?sensor=false&amp;key={$GoogleAPIKey}">&#160;</script>
		</xsl:if>
		<xsl:call-template name="bundle-js">
			<xsl:with-param name="comma-separated-files">
				<xsl:text>~/ewcommon/js/jQuery/jsScrollPane/jquery.jscrollpane.min.js,</xsl:text>
				<xsl:text>~/ewcommon/js/jQuery/jsScrollPane/jquery.mousewheel.js,</xsl:text>
				<xsl:text>~/ewcommon/js/codemirror/codemirror.js,</xsl:text>
				<xsl:text>~/ewcommon/js/jQuery/jquery.magnific-popup.min.js,</xsl:text>
				<xsl:text>~/ewcommon/js/codemirror/mirrorframe.js,</xsl:text>
				<xsl:text>~/ewcommon/js/vuejs/vue.min.js,</xsl:text>
				<xsl:text>~/ewcommon/js/vuejs/axios.min.js,</xsl:text>
				<xsl:text>~/ewcommon/js/vuejs/polyfill.js,</xsl:text>
				<xsl:text>~/ewcommon/js/vuejs/protean-vue.js,</xsl:text>
				<xsl:text>~/ewcommon/js/ewAdmin.js</xsl:text>
			</xsl:with-param>
			<xsl:with-param name="bundle-path">
				<xsl:text>~/Bundles/Admin</xsl:text>
			</xsl:with-param>
		</xsl:call-template>
		<xsl:apply-templates select="." mode="siteAdminJs"/>
		<xsl:apply-templates select="." mode="LayoutAdminJs"/>
	</xsl:template>

	<xsl:template match="Page" mode="LayoutAdminJs"></xsl:template>


	<!-- -->
	<!--   ##################  Edit Structure - dynamically generated from menu   ##############################   -->
	<!-- -->
	<xsl:template match="Page[@ewCmd='EditStructure' or @ewCmd='DeliveryMethods' or @layout='ImageLib' or @layout='DocsLib' or @layout='MediaLib' or @ewCmd='Permissions' or @ewCmd='MoveContent' or @ewCmd='ShippingLocations' or @layout='MovePage']" mode="adminJs">

		<xsl:variable name="getMenuNoReload">
			<xsl:call-template name="getSettings">
				<xsl:with-param name="sectionName" select="'web'"/>
				<xsl:with-param name="valueName" select="'menuNoReload'"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:variable name="menuNoReload">
			<xsl:choose>
				<xsl:when test="$getMenuNoReload = ''">
					<xsl:value-of select="'true'"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$getMenuNoReload"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<script type="text/javascript" src="/ewcommon/js/jQuery/jsScrollPane/jquery.jscrollpane.min.js">&#160;</script>
		<script type="text/javascript" src="/ewcommon/js/jQuery/jsScrollPane/jquery.mousewheel.js">&#160;</script>
		<!--
	  TS commented out as was breaking bootstrap modal - not sure this is needed ???
	  
    <script type="text/javascript" src="/ewcommon/js/jQuery/simplemodal/jquery.simplemodal-1.4.4.min.js">&#160;</script>
	-->
		<xsl:if test="@cssFramework!='bs3'">
			<script type="text/javascript" src="/ewcommon/js/jQuery/jquery.magnific-popup.min.js">&#160;</script>
		</xsl:if>

		<script type="text/javascript" src="/ewcommon/js/ewAdmin.js">&#160;</script>
		<!--level: <xsl:value-of select="$menuLevelDepth"/>-->

		<xsl:apply-templates select="." mode="LayoutAdminJs"/>

		<xsl:apply-templates select="." mode="xform_control_scripts"/>

	</xsl:template>

	<!-- -->
	<xsl:template match="Page" mode="siteAdminJs"></xsl:template>


	<!-- -->
	<!--   ################################################   menu setup   ##################################################   -->
	<!-- -->
	<xsl:template match="Page[@adminMode='true']" mode="adminPageHeader">
		<h1 class="page-header">
			<i class="fa {/Page/AdminMenu/descendant-or-self::MenuItem[@cmd=/Page/@ewCmd][last()]/@icon}">&#160;</i>&#160;
			<xsl:value-of select="/Page/AdminMenu/descendant-or-self::MenuItem[@cmd=/Page/@ewCmd][last()]/@name"/>
		</h1>
	</xsl:template>

	<!--In admin but not WYSIWYG-->
	<xsl:template match="Page[@adminMode='true']" mode="bodyBuilder">
		<body id="pg_{@id}" class="ewAdmin layout-{@layout}">
			<xsl:apply-templates select="AdminMenu"/>
			<div id="adminLayout">
				<div class="admin-page">
					<xsl:if test="/Page/AdminMenu/descendant-or-self::MenuItem[@cmd=/Page/@ewCmd]/@name!=''">
						<xsl:apply-templates select="." mode="adminPageHeader"/>
					</xsl:if>
					<xsl:choose>
						<xsl:when test="/Page/AdminMenu/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=/Page/@ewCmd]]/MenuItem[@display='true']">
							<xsl:attribute name="class">
								<xsl:text>container row</xsl:text>
							</xsl:attribute>
							<div class="col-md-3">
								<xsl:apply-templates select="." mode="AdminLeftMenu"/>
							</div>
							<div class="col-md-9">
								<xsl:apply-templates select="." mode="Admin"/>
							</div>
						</xsl:when>
						<xsl:otherwise>
							<xsl:apply-templates select="." mode="Admin"/>
						</xsl:otherwise>
					</xsl:choose>
				</div>
			</div>
			<xsl:apply-templates select="." mode="adminFooter"/>
			<xsl:apply-templates select="." mode="footerJs"/>
			<script>keepAlive();</script>
			<div class="modal fade" id="AdminAlertModal" role="dialog" style ="padding-top:15%!important">
				<div class="modal-dialog">
					<div class="modal-content  alert alert-danger" role="alert">
						<div class="modal-body">
							<i id="errorIcon" class="fa fa-exclamation-triangle" aria-hidden="true">&#160;</i>
							<xsl:text disable-output-escaping="yes">&amp;</xsl:text>nbsp;
							<button type="button" class="close" data-dismiss="modal">
								<i class="fa fa-times">&#160;</i>
							</button>
							<span id="errorMessage">&#160;</span>
						</div>
					</div>
				</div>
			</div>
			<iframe id="keepalive" src="/ewCommon/tools/keepalive.ashx" frameborder="0" width="0" height="0" xmlns:ew="urn:ew">Keep Alive frame</iframe>
		</body>
	</xsl:template>



	<!--In admin WYSIWYG mode-->
	<xsl:template match="Page[@adminMode='false']" mode="bodyBuilder">
		<body id="pg_{@id}" class="normalMode">
			<xsl:apply-templates select="." mode="bodyStyle"/>
			<div class="ewAdmin">
				<xsl:apply-templates select="AdminMenu"/>
			</div>
			<div id="dragableModules">
				<xsl:apply-templates select="." mode="bodyDisplay"/>
			</div>
			<div class="ewAdmin">
				<xsl:apply-templates select="." mode="adminFooter"/>
			</div>
			<xsl:apply-templates select="." mode="footerJs"/>
			<iframe id="keepalive" src="/ewCommon/tools/keepalive.ashx" frameborder="0" width="0" height="0" xmlns:ew="urn:ew">Keep Alive frame</iframe>
		</body>
	</xsl:template>

	<xsl:template match="Page[@previewMode]" mode="bodyBuilder">
		<body>
			<xsl:apply-templates select="." mode="bodyStyle"/>
			<xsl:apply-templates select="PreviewMenu"/>
			<xsl:apply-templates select="." mode="bodyDisplay"/>
			<xsl:apply-templates select="." mode="footerJs"/>
		</body>
	</xsl:template>

	<xsl:template match="Page[@layout='Logon']" mode="Admin">
		<div class="adminTemplate" id="template_Logon">
			<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
			<xsl:apply-templates select="ContentDetail/Content[contains(@type,'xFormQuiz')]" mode="edit"/>
		</div>
	</xsl:template>

	<xsl:template match="submit[ancestor::Content[@name='UserLogon'] and ancestor::Page/@adminMode='true']" mode="xform">
		<xsl:variable name="class">
			<xsl:text>adminButton</xsl:text>
			<xsl:if test="@class!=''">
				<xsl:text> </xsl:text>
				<xsl:value-of select="@class"/>
			</xsl:if>
		</xsl:variable>
		<a href="{$appPath}?ewCmd=LogOff" class="btn btn-primary">
			<i class="fa fa-reply">
				<xsl:text> </xsl:text>
			</i> Back to Site
		</a>

		<button type="submit" name="{@submission}" value="{label/node()}" class="btn btn-success {$class}"  onclick="disableButton(this);">
			Login<xsl:text> </xsl:text>
			<i class="fa fa-check">
				<xsl:text> </xsl:text>
			</i>
		</button>
		<br/>
		<br/>
		<!--a href="/?ewCmd=PasswordReminder" class="text-muted">
        <i class="fa fa-chevron-right">
          <xsl:text> </xsl:text>
        </i>
        Password Reminder
      </a-->
	</xsl:template>


	<xsl:template match="label[ancestor::Content[@name='UserLogon'] and parent::group/@ref='UserDetails' and  ancestor::Page/@adminMode='true']" mode="legend">

		<xsl:choose>
			<xsl:when test="$page/Settings/add[@key='web.eonicwebProductName']/@value!=''">
				<xsl:call-template name="eonicwebAdminSystemName"/>
			</xsl:when>
			<xsl:otherwise>
				<img src="/ewcommon/images/admin/skin/protean-admin-black-logon.png" alt="ProteanCMS" width="320px" height="57px"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="PreviewMenu">
		<xsl:variable name="CMSLogo">
			<xsl:call-template name="eonicwebLogo"/>
		</xsl:variable>
		<xsl:variable name="CMSName">
			<xsl:call-template name="eonicwebProductName"/>
		</xsl:variable>
		<div class="ewAdmin">
			<div id="adminHeader" class="affix-top">
				<div id="topRight">
					<div id="adminLogo">
						<img src="{$CMSLogo}" alt="{$CMSName}" class="cms-logo"/>
						<span class="hidden">
							<xsl:call-template name="eonicwebCMSName"/>
						</span>
					</div>

					<div id="sectionIcon" class="sectionfa-Preview">
						<span class="hidden">Preview</span>
					</div>
				</div>
				<div id="headers" class="preview">
					<span id="breadcrumb">
						<xsl:text>Previewing as user: </xsl:text><strong>
							<xsl:value-of select="/Page/User/@name"/>
						</strong> on date: <strong>
							<xsl:value-of select="/Page/Request/ServerVariables/Item[@name='Date']/node()"/>
						</strong>
					</span>
					<xsl:text> </xsl:text>
					<xsl:text> </xsl:text>
					<a href="{$appPath}?ewCmd=Normal&amp;pgid={/Page/@id}" class="btn btn-success" id="previewBack">
						<i class="fa fa-share-alt fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Back to Admin
					</a>
				</div>
				<div class="terminus">
					<xsl:text> </xsl:text>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template name="otherwise">
		<xsl:text>No help available. If you think there should be a help section available here or if you need help using this section please contact support.</xsl:text>
	</xsl:template>

	<xsl:template match="AdminMenu">
		<xsl:variable name="contextCmd">
			<xsl:choose>
				<xsl:when test="$page/@editContext!=''">
					<xsl:value-of select="/Page/@editContext"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="/Page/@ewCmd"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="adminSectionPage" select="/Page/AdminMenu/MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=$contextCmd or contains(@subCmds,$subMenuCommand)]]"/>

		<xsl:variable name="adminContextSectionPage" select="/Page/AdminMenu/MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=/Page/@editContext]]"/>
		<xsl:variable name="CMSLogo">
			<xsl:call-template name="eonicwebLogo"/>
		</xsl:variable>
		<xsl:variable name="CMSName">
			<xsl:call-template name="eonicwebProductName"/>
		</xsl:variable>

		<!-- ADMIN HEADER-->
		<xsl:if test="contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 8') or contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 7') or contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 6.0')">
			<div class="alert alert-danger browser-warning">
				<strong>Please upgrade your browser.</strong> The version of Internet Explorer you are currently using does not support all the features used in this Content Management System. If you do not upgrade your browser you may experience some problems using this system.
			</div>
		</xsl:if>
		<div id="adminHeader" class="affix-top navbar-fixed-top">

			<div class="absolute-admin-logo">
				<img src="{$CMSLogo}" alt="{$CMSName}" class="cms-logo"/>
				<div class="non-department">
					<span class="sectionName">
						<xsl:value-of select="MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=$contextCmd]]/@name"/>
					</span>
					<span class="switch">
						switch <i class="fa fa-ellipsis-v">
							<xsl:text> </xsl:text>
						</i>
					</span>
				</div>
				<div class="department">
					main menu
					<span class="switch">
						close <i class="fa fa-times">
							<xsl:text> </xsl:text>
						</i>
					</span>
				</div>
			</div>
			<nav class="navbar navbar-inverse admin-department-menu admin-department-menu-js" role="navigation">

				<xsl:if test="/Page[@ewCmd='AdmHome']">
					<xsl:attribute name="class">
						<xsl:text>navbar navbar-inverse admin-department-menu dashboard-menu</xsl:text>
					</xsl:attribute>
				</xsl:if>
				<div class="navbar-header">
					<button type="button" class="navbar-toggle" data-toggle="collapse" data-target="#bs-admin-navbar-collapse-0">
						<span class="sr-only">Toggle navigation</span>
						<span class="icon-bar">
							<xsl:text> </xsl:text>
						</span>
						<span class="icon-bar">
							<xsl:text> </xsl:text>
						</span>
						<span class="icon-bar">
							<xsl:text> </xsl:text>
						</span>
					</button>
					<a class="navbar-brand" href="#">
						main menu
					</a>
				</div>


				<div class="collapse navbar-collapse" id="bs-admin-navbar-collapse-0">
					<ul class="nav navbar-nav">
						<xsl:apply-templates select="MenuItem" mode="adminMenuItem">
							<xsl:with-param name="level">1</xsl:with-param>
						</xsl:apply-templates>
						<xsl:apply-templates select="MenuItem/MenuItem" mode="adminMenuItem">
							<xsl:with-param name="level">1</xsl:with-param>
						</xsl:apply-templates>
					</ul>
					<ul class="nav navbar-nav navbar-right">
						<li>
							<a id="myaccount" href="{$appPath}?ewCmd=EditDirItem&amp;DirType=User&amp;id={$page/User/@id}">
								<i class="fa fa-user">
									<xsl:text> </xsl:text>
								</i>
								<xsl:value-of select="$page/User/@name"/>
							</a>
						</li>
						<li>
							<a id="logoff" href="{$appPath}?ewCmd=LogOff" title="Click here to log off from your active session" >
								<i class="fa fa-power-off">
									<xsl:text> </xsl:text>
								</i>
								<span> LOG OFF</span>
							</a>
						</li>
					</ul>
				</div>

			</nav>


			<!-- MAIN MENU -->

			<!-- Brand and toggle get grouped for better mobile display -->
			<nav class="navbar navbar-inverse admin-main-menu" role="navigation">
				<div class="navbar-header">
					<button type="button" class="navbar-toggle" data-toggle="collapse" data-target="#bs-admin-navbar-collapse-1">
						<span class="sr-only">Toggle navigation</span>
						<span class="icon-bar">
							<xsl:text> </xsl:text>
						</span>
						<span class="icon-bar">
							<xsl:text> </xsl:text>
						</span>
						<span class="icon-bar">
							<xsl:text> </xsl:text>
						</span>
					</button>
					<a class="navbar-brand" href="#">
						<span class="admin-logo-text">
							proteancms
						</span>
						<span class="visible-admin-xs xs-admin-switch">
							<span class="sectionName">
								<xsl:value-of select="MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=$contextCmd]]/@name"/>
							</span>
						</span>
					</a>
				</div>

				<!-- Collect the nav links, forms, and other content for toggling -->
				<div class="collapse navbar-collapse" id="bs-admin-navbar-collapse-1">
					<!--<ul class="nav navbar-nav hidden-xs nav-add-more-auto">-->
					<ul class="nav navbar-nav nav-add-more-auto ">

						<xsl:apply-templates select="MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=$contextCmd]]/MenuItem" mode="adminItem1">
							<xsl:with-param name="level">1</xsl:with-param>
						</xsl:apply-templates>

						<xsl:text> </xsl:text>
					</ul>
					<!--<ul class="nav navbar-nav visible-md">
            <xsl:apply-templates select="MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=$contextCmd]]/MenuItem[position()&lt;4]" mode="adminItem1">
              <xsl:with-param name="level">1</xsl:with-param>
            </xsl:apply-templates>
            <xsl:if test="MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=$contextCmd]]/MenuItem[position()&gt;3]">
              <li class="dropdown">
                <a href="#" class="dropdown-toggle" data-toggle="dropdown">
                  More <b class="caret"></b>
                </a>
                <ul class="dropdown-menu">
                  <xsl:apply-templates select="MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=$contextCmd]]/MenuItem[position()&gt;3]" mode="adminItem1">
                    <xsl:with-param name="level">1</xsl:with-param>
                  </xsl:apply-templates>
                </ul>
              </li>
            </xsl:if>
          </ul>
          <ul class="nav navbar-nav visible-sm">
            <xsl:apply-templates select="MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=$contextCmd]]/MenuItem[position()&lt;3]" mode="adminItem1">
              <xsl:with-param name="level">1</xsl:with-param>
            </xsl:apply-templates>
            <xsl:if test="MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=$contextCmd]]/MenuItem[position()&gt;2]">
              <li class="dropdown">
                <a href="#" class="dropdown-toggle" data-toggle="dropdown">
                  More <b class="caret"></b>
                </a>
                <ul class="dropdown-menu">
                  <xsl:apply-templates select="MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=$contextCmd]]/MenuItem[position()&gt;2]" mode="adminItem1">
                    <xsl:with-param name="level">1</xsl:with-param>
                  </xsl:apply-templates>
                </ul>
              </li>
            </xsl:if>
          </ul>-->
					<!--<ul class="nav navbar-nav visible-xs xs-admin-main-menu">
            <xsl:apply-templates select="MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=$contextCmd]]/MenuItem" mode="adminItem1">
              <xsl:with-param name="level">1</xsl:with-param>
            </xsl:apply-templates>
            <xsl:text> </xsl:text>
          </ul>-->
					<ul class="nav navbar-nav navbar-right">
						<li>
							<a id="myaccount" href="{$appPath}?ewCmd=EditDirItem&amp;DirType=User&amp;id={$page/User/@id}">
								<i class="fa fa-user">
									<xsl:text> </xsl:text>
								</i>
								<span>
									<xsl:value-of select="$page/User/@name"/>
								</span>
							</a>
						</li>
						<li>
							<a id="logoff" href="{$appPath}?ewCmd=LogOff" title="Click here to log off from your active session" >
								<i class="fa fa-power-off">
									<xsl:text> </xsl:text>
								</i>
								<span> LOG OFF</span>
							</a>
						</li>
					</ul>
				</div>
				<!-- /.navbar-collapse -->
			</nav>


			<!-- MAIN MENU WORKING-->

			<!--<nav class="navbar navbar-inverse admin-main-menu" role="navigation">
        <div class="navbar-header">
          <button type="button" class="navbar-toggle" data-toggle="collapse" data-target="#bs-example-navbar-collapse-1">
            <span class="sr-only">Toggle navigation</span>
            <span class="icon-bar"></span>
            <span class="icon-bar"></span>
            <span class="icon-bar"></span>
          </button>
          <a class="navbar-brand" href="#">eonic<strong>web</strong>5</a>
        </div>

        <div class="collapse navbar-collapse" id="bs-example-navbar-collapse-1">
          <ul class="nav navbar-nav">
            
            <xsl:apply-templates select="MenuItem/MenuItem[ancestor-or-self::MenuItem/@cmd = $adminSectionPage/@cmd]/MenuItem" mode="adminItem1">
              <xsl:with-param name="level">1</xsl:with-param>
            </xsl:apply-templates>
          </ul>
          <ul class="nav navbar-nav navbar-right">
            <li>
              <a id="logoff" href="{$appPath}?ewCmd=LogOff" title="Click here to log off from your active session" >
                <i class="fa fa-power-off"></i> LOG OFF
              </a>
            </li>
          </ul>
        </div>
      </nav>-->

			<!-- SUB MENU WORKING-->

			<nav class="navbar navbar-inverse admin-sub-menu not-visible-admin-xs" role="navigation">
				<!-- Brand and toggle get grouped for better mobile display -->
				<div class="navbar-header">
					<button type="button" class="navbar-toggle" data-toggle="collapse" data-target="#bs-admin-navbar-collapse-2">
						<span class="sr-only">Toggle navigation</span>
						<span class="icon-bar">
							<xsl:text> </xsl:text>
						</span>
						<span class="icon-bar">
							<xsl:text> </xsl:text>
						</span>
						<span class="icon-bar">
							<xsl:text> </xsl:text>
						</span>
					</button>
					<a class="navbar-brand" href="#">
						<span class="sectionName">
							<xsl:value-of select="MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=$contextCmd]]/@name"/>
						</span>
					</a>
				</div>

				<!-- Collect the nav links, forms, and other content for toggling -->
				<div class="collapse navbar-collapse" id="bs-admin-navbar-collapse-2">
					<xsl:if test="$page/@ewCmd!='MailingList'">
						<ul class="nav navbar-nav nav-add-more-auto">
							<xsl:apply-templates select="MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=$contextCmd]]/MenuItem" mode="adminItem2">
								<xsl:with-param name="level">1</xsl:with-param>
							</xsl:apply-templates>
							<xsl:text> </xsl:text>
						</ul>

						<!--<ul class="nav navbar-nav visible-xs">
            <xsl:apply-templates select="MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=$contextCmd]]/MenuItem" mode="adminItem2">
              <xsl:with-param name="level">1</xsl:with-param>
            </xsl:apply-templates>
            <xsl:text> </xsl:text>
          </ul>-->
					</xsl:if>
					<ul class="nav navbar-nav navbar-right">
						<li>
							<xsl:if test="$page/@ewCmd='Normal'">
								<!--<span class="glyphicon glyphicon-eye-open"></span>-->
								<xsl:apply-templates select="MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=$contextCmd]]/MenuItem" mode="previewLink">
									<xsl:with-param name="level">1</xsl:with-param>
								</xsl:apply-templates>
							</xsl:if>
							<xsl:text> </xsl:text>
						</li>
					</ul>
				</div>
				<!-- /.navbar-collapse -->
			</nav>




			<!--<div id="topRight">
        <div id="adminLogo">
          <span class="hidden"><xsl:call-template name="eonicwebCMSName"/></span>
        </div>
        <div id="sectionIcon">
          <xsl:attribute name="class">
            <xsl:text>sectionfa-</xsl:text>
            <xsl:value-of select="translate(MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=$contextCmd]]/@name,' ','')"/>
          </xsl:attribute>
          <span class="hidden">
            <xsl:value-of select="MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=$contextCmd]]/@name"/>
          </span>
        </div>
        <span class="sectionName">
          <xsl:value-of select="MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=$contextCmd]]/@name"/>
        </span>
      </div>-->

			<div id="headers">



				<xsl:apply-templates select="/Page" mode="UserGuide"/>

				<!-- <a title="Get Help Editing" target="new" id="getHelp">
					<xsl:attribute name="href">
						<xsl:text>http://www.eonic.co.uk?utm_campaign=adminsystem&amp;utm_source=</xsl:text>
						<xsl:value-of select="//ServerVariables/Item[@name='SERVER_NAME']/node()"/>
					</xsl:attribute>
					<xsl:text>Help Editing</xsl:text>
				</a> -->

				<!-- breadcrumb -->

				<!--<xsl:if test="/Page/User">
          <xsl:if test="count(MenuItem/MenuItem[ancestor-or-self::MenuItem/@cmd = $adminSectionPage/@cmd and @cmd!='admin']/MenuItem) &gt; 0">
            <div id="mainAdminMenu">
              <ul>
                    <xsl:apply-templates select="MenuItem/MenuItem[ancestor-or-self::MenuItem/@cmd = $adminSectionPage/@cmd]/MenuItem" mode="adminItem1">
                      <xsl:with-param name="level">1</xsl:with-param>
                    </xsl:apply-templates>               
              </ul>
              -->
				<!--<div class="terminus">&#160;</div>-->
				<!--
            </div>
          </xsl:if>
        </xsl:if>-->
			</div>

			<!--<div class="bar">
        <xsl:if test="/Page/User">
          <a id="logoff" href="{$appPath}?ewCmd=LogOff" title="Click here to log off from your active session" >
            <span class="hidden">Logoff</span>
          </a>
          <a data-toggle="modal" href="#MainMenuModal" id="mainMenuButton" title="Main Menu - Choose from Content, eCommerce and Membership Areas">
            <span class="hidden">Main Menu</span>
          </a>
          <xsl:if test="count(MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=$contextCmd]]/MenuItem) &gt; 0">
            <div id="subAdminMenu">
              <ul>

                    <xsl:apply-templates select="MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=$contextCmd]]/MenuItem" mode="adminItem2">
                      <xsl:with-param name="level">1</xsl:with-param>
                    </xsl:apply-templates>

              </ul>
              <div class="terminus">&#160;</div>
            </div>
          </xsl:if>
        </xsl:if>
      </div>-->
			<!--<div id="MainMenuModal" class="modal fade" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">
        <div class="modal-dialog">
          <div class="modal-content">
            <div class="modal-body">
              <ul class="nav nav-justified">
                <xsl:apply-templates select="MenuItem/MenuItem[position()&lt;4]" mode="adminMenuItem">
                  <xsl:with-param name="level">1</xsl:with-param>
                </xsl:apply-templates>
              </ul>
              <ul class="nav nav-justified">
                <xsl:apply-templates select="MenuItem/MenuItem[position()&gt;3]" mode="adminMenuItem">
                  <xsl:with-param name="level">1</xsl:with-param>
                </xsl:apply-templates>
              </ul>
            </div>
            <div class="modal-footer">
              <a href="{$appPath}?ewCmd=AdmHome" class="btn btn-default">
                <i class="fa fa-home">
                  <xsl:text> </xsl:text>
                </i>Admin Home</a>
              <button type="button" class="btn btn-default" data-dismiss="modal">
                <i class="fa fa-remove">
                  <xsl:text> </xsl:text>
                </i>
                Close</button>
            </div>
          </div>
        </div>
      </div>-->

		</div>
		<div class="terminus">&#160;</div>

		<xsl:if test="not(/Page[@ewCmd='Normal'])">
			<!--<div id="breadcrumb">-->
			<xsl:apply-templates select="/" mode="adminBreadcrumb"/>

			<!--</div>-->
		</xsl:if>
	</xsl:template>
	<!-- -->
	<!--   ################################################   User Guide  ##################################################   -->
	<!-- -->
	<xsl:template match="Page" mode="UserGuide">

		<button id="btnHelpEditing">
			<i class="fas fa-graduation-cap fa-lg">
				<xsl:text> </xsl:text>
			</i>
			<br/>
			<span>
				<xsl:text>USER GUIDE</xsl:text>
			</span>
			<i class="fa fa-chevron-left">
				<xsl:text> </xsl:text>
			</i>
		</button>
		<div id="divHelpBox">
			<div id="helpBox">
				<div class="scroll-pane-arrows">
					<p>
						<xsl:apply-templates select="." mode="UserGuideContent"/>
					</p>
				</div>
			</div>
		</div>
	</xsl:template>


	<xsl:template match="Page" mode="UserGuideContent">
		<xsl:variable name="ewCmd" select="/Page/Request/QueryString/Item[@name='ewCmd']/node()"/>
		<xsl:variable name="cContentSchemaName" select="/Page/ContentDetail/descendant::cContentSchemaName/node()"/>
		<xsl:variable name="moduleType" select="/Page/ContentDetail/descendant::Content/@moduleType"/>
		<a target="_new" id="userGuideURL">
			<xsl:attribute name="href">
				<xsl:text>/ewcommon/tools/UserGuide.ashx?fRef=</xsl:text>
				<xsl:choose>
					<xsl:when test="$ewCmd='EditContent' or $ewCmd='AddModule' or $ewCmd='AddContent'">
						<xsl:choose>
							<xsl:when test="not($cContentSchemaName)">
								<xsl:value-of select="$ewCmd"/>
							</xsl:when>
							<xsl:when test="$cContentSchemaName='Module'">
								<xsl:value-of select="$moduleType"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="$cContentSchemaName"/>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:when>
					<xsl:when test="$ewCmd='admin' or $ewCmd='ByPage' or $ewCmd='Normal' or $ewCmd='Content' or not($ewCmd)">
						<xsl:text>ByPage</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="$ewCmd"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:attribute>
			<xsl:text>User Guide</xsl:text>
		</a>

	</xsl:template>

	<!-- -->
	<!--   ################################################   leftMenu   ##################################################   -->
	<!-- -->
	<xsl:template match="Page" mode="AdminLeftMenu">
		<xsl:if test="/Page/AdminMenu/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=/Page/@ewCmd]]/MenuItem[@display='true']">
			<div class="panel panel-default">
				<div class="panel-heading">
					<h2 class="panel-title">
						<xsl:value-of select="/Page/AdminMenu/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=/Page/@ewCmd]]/@name"/>
					</h2>
				</div>
				<ul class="panel-body">
					<xsl:for-each select="/Page/AdminMenu/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=/Page/@ewCmd]]">
						<li>
							<xsl:apply-templates select="self::MenuItem" mode="adminLink">
								<xsl:with-param name="level">
									4
								</xsl:with-param>
							</xsl:apply-templates>
						</li>
					</xsl:for-each>
					<xsl:for-each select="/Page/AdminMenu/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=/Page/@ewCmd]]/MenuItem">
						<li>
							<xsl:apply-templates select="self::MenuItem" mode="adminLink">
								<xsl:with-param name="level">
									3
								</xsl:with-param>
							</xsl:apply-templates>
						</li>
					</xsl:for-each>
				</ul>
			</div>
		</xsl:if>
	</xsl:template>

	<!-- -->
	<!--   ################################################   breadcrumb   ##################################################   -->
	<!-- -->
	<xsl:template match="MenuItem" mode="adminBreadcrumbSt">
		<xsl:apply-templates select="." mode="adminMenuLinkSt"/>
		<xsl:apply-templates select="MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="adminBreadcrumbSt"/>
	</xsl:template>

	<xsl:template match="MenuItem" mode="adminBreadcrumbId">
		<xsl:param name="thispageid"/>
		<xsl:apply-templates select="descendant-or-self::MenuItem[descendant-or-self::MenuItem[@id=$thispageid]]" mode="adminMenuLinkSt"/>
	</xsl:template>

	<!-- Generic Menu Link -->
	<xsl:template match="MenuItem" mode="adminMenuLinkSt">
		<li>
			<a title="{@name}">
				<xsl:attribute name="href">
					<xsl:apply-templates select="self::MenuItem" mode="getHref"/>
				</xsl:attribute>
				<xsl:choose>
					<xsl:when test="self::MenuItem[@id=/Page/@id]">
						<xsl:attribute name="class">active</xsl:attribute>
					</xsl:when>
					<xsl:when test="descendant::MenuItem[@id=/Page/@id] and ancestor::MenuItem">
						<xsl:attribute name="class">on</xsl:attribute>
					</xsl:when>
				</xsl:choose>
				<xsl:choose>
					<xsl:when test="DisplayName/@siteTemplate='micro'">
						<xsl:value-of select="@name"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="." mode="getDisplayName"/>
					</xsl:otherwise>
				</xsl:choose>

				<xsl:if test="self::MenuItem[@id=/Page/@id]">
					<xsl:if test="@verDesc!=''">
						[ver: <xsl:value-of select="@verDesc"/>]
					</xsl:if>
					<xsl:if test="PageVersion and not(@verDesc)">
						[ver: default]
					</xsl:if>
				</xsl:if>
			</a>
		</li>
	</xsl:template>

	<!-- -->
	<xsl:template match="Page" mode="adminBreadcrumb">
		<ol class="breadcrumb admin-breadcrumb">
			<xsl:apply-templates select="AdminMenu/descendant-or-self::MenuItem[descendant-or-self::MenuItem[@cmd=/Page/@ewCmd or contains(@subCmds,$subMenuCommand)]]" mode="adminLink"/>
		</ol>
	</xsl:template>

	<xsl:template match="Page[@ewCmd='ByPage' or @ewCmd='Normal' or @ewCmd='Advanced' or @ewCmd='EditPage' or @ewCmd='EditPageLayout' or @ewCmd='EditMailLayout'  or @ewCmd='EditPagePermissions' or @ewCmd='EditPageRights' or @ewCmd='LocateSearch']" mode="adminBreadcrumb">
		<xsl:if test="/Page/@id != ''">
			<ol class="breadcrumb admin-breadcrumb">
				<xsl:apply-templates select="/Page/Menu/MenuItem" mode="adminBreadcrumbSt"/>
			</ol>
		</xsl:if>
	</xsl:template>

	<xsl:template match="Page[@ewCmd='EditContent' or contains(@ewCmd,'EditXForm') or @ewCmd='EditMailContent']" mode="adminBreadcrumb">
		<div class="breadcrumb admin-breadcrumb">
			<div class="admin-breadcrumb-inner">
				<xsl:for-each select="ContentDetail/Content/model/instance/tblContent/Location[@primary='true']">
					<ul>
						<li>
							<i class="fa fa-file">&#160;</i> &#160;[Primary page]&#160;
						</li>
						<xsl:apply-templates select="/Page/Menu/MenuItem" mode="adminBreadcrumbId">
							<xsl:with-param name="thispageid" select="@pgid"/>
						</xsl:apply-templates>
					</ul>
				</xsl:for-each>
				<xsl:for-each select="ContentDetail/Content/model/instance/tblContent/Location[@primary!='true']">
					<ul>
						<li>
							<i class="fa fa-file-o">&#160;</i> &#160; [Also on page]&#160;
						</li>
						<xsl:apply-templates select="/Page/Menu/MenuItem" mode="adminBreadcrumbId">
							<xsl:with-param name="thispageid" select="@pgid"/>
						</xsl:apply-templates>
					</ul>
				</xsl:for-each>
			</div>
			<a href="" class="all-breadcrumb">
				<i class="fa fa-angle-down">&#160;</i>&#160;see all locations
			</a>
			<a href="" class="less-breadcrumb">
				<i class="fa fa-angle-up">&#160;</i>&#160;hide locations
			</a>
		</div>
	</xsl:template>

	<xsl:template match="Page[@ewCmd='CopyPage']" mode="adminBreadcrumb">
		<xsl:if test="/Page/@id != ''">
			<ol class="breadcrumb admin-breadcrumb">
				<xsl:apply-templates select="/Page/Menu/MenuItem" mode="breadcrumb"/>
			</ol>
		</xsl:if>
		<xsl:text>[Copying]</xsl:text>
	</xsl:template>

	<xsl:template match="Page[@ewCmd='MoveContent']" mode="adminBreadcrumb">
		<xsl:text>[Move Content]</xsl:text>
	</xsl:template>

	<xsl:template match="Page[@ewCmd='AddPage']" mode="adminBreadcrumb">
		<xsl:text>Adding New Page below : </xsl:text>
	</xsl:template>

	<xsl:template match="Page[@ewCmd='MailingList.CMSession']" mode="adminBreadcrumb">

	</xsl:template>

	<xsl:template match="MenuItem" mode="adminBreadcrumb">

		<ol class="breadcrumb admin-breadcrumb">
			<xsl:if test="@cmd!='AdmHome'">
				<xsl:apply-templates select="self::MenuItem" mode="adminLink"/>
				<xsl:text> / </xsl:text>
			</xsl:if>
			<xsl:apply-templates select="MenuItem[descendant-or-self::MenuItem[@cmd=/Page/@ewCmd or contains(@subCmds,$subMenuCommand)]]" mode="breadcrumbAdmin"/>
		</ol>
	</xsl:template>

	<!-- -->
	<xsl:template match="Page" mode="adminFooter">
		<xsl:variable name="supportEmail">
			<xsl:call-template name="eonicwebSupportEmail"/>
		</xsl:variable>
		<xsl:variable name="supportWebsite">
			<xsl:call-template name="eonicwebWebsite"/>
		</xsl:variable>
		<div id="footer">
			<div id="footerCopyright" class="text-muted">

				<xsl:text>© </xsl:text>
				<xsl:call-template name="eonicwebCopyright"/>
				<xsl:text> 2002-</xsl:text>
				<xsl:value-of select="substring(//ServerVariables/Item[@name='Date'],1,4)"/>
				<xsl:text> | </xsl:text>
				<xsl:call-template name="eonicwebSupportTelephone"/>
				<xsl:text> | </xsl:text>
				<a href="mailto:{$supportEmail}" title="Email Support">
					<xsl:value-of select="$supportEmail"/>
				</a>
				<xsl:text> | </xsl:text>
				<a title="view the latest news">
					<xsl:attribute name="href">
						<xsl:text>http://</xsl:text>
						<xsl:value-of select="$supportWebsite"/>
						<xsl:text>?utm_campaign=cmsadminsystem&amp;utm_source=</xsl:text>
						<xsl:value-of select="//ServerVariables/Item[@name='SERVER_NAME']/node()"/>
					</xsl:attribute>
					<xsl:value-of select="$supportWebsite"/>
				</a>
				<span class="pull-right">
					<xsl:value-of select="substring-before(//ServerVariables/Item[@name='GENERATOR']/node(),', Culture')"/>
				</span>
			</div>
		</div>
		<div id="loading-indicator" class="model" style="display:none">
			<div class="modal-content">
				<div class="modal-body">
					<div class="center-block">
						<i class="fa fa-cog fa-spin fa-5x">
							<xsl:text> </xsl:text>
						</i>
						<h1>Loading...</h1>
					</div>
				</div>
			</div>
		</div>
	</xsl:template>
	<!-- -->
	<xsl:template match="MenuItem" mode="adminMenuItem">
		<xsl:param name="level"/>
		<li>
			<xsl:apply-templates select="." mode="adminLinkMainMenu">
				<xsl:with-param name="level">
					<xsl:value-of select="$level"/>
				</xsl:with-param>
			</xsl:apply-templates>
		</li>
	</xsl:template>
	<!-- -->
	<!-- -->
	<xsl:template match="MenuItem" mode="adminItem1">
		<xsl:param name="level"/>
		<xsl:variable name="contextCmd">
			<xsl:choose>
				<xsl:when test="$page/@editContext!=''">
					<xsl:value-of select="/Page/@editContext"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="/Page/@ewCmd"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<li>
			<xsl:apply-templates select="." mode="adminLink1">
				<xsl:with-param name="level">
					<xsl:value-of select="$level"/>
				</xsl:with-param>
			</xsl:apply-templates>
			<xsl:if test="MenuItem[@cmd=$contextCmd]">
				<ul class="admin-sub-menu xs-admin-sub-menu visible-admin-xs">
					<xsl:apply-templates select="MenuItem" mode="adminItem2">
						<xsl:with-param name="level">1</xsl:with-param>
					</xsl:apply-templates>
					<li>
						<xsl:apply-templates select="MenuItem" mode="previewLink">
							<xsl:with-param name="level">1</xsl:with-param>
						</xsl:apply-templates>
					</li>
				</ul>
			</xsl:if>
		</li>
	</xsl:template>
	<!-- -->
	<!-- -->
	<xsl:template match="MenuItem" mode="adminItem2">
		<xsl:param name="level"/>
		<xsl:if test="@display='true'">
			<li>
				<xsl:apply-templates select="." mode="adminLink2">
					<xsl:with-param name="level">
						<xsl:value-of select="$level"/>
					</xsl:with-param>
				</xsl:apply-templates>
				<xsl:text> </xsl:text>
			</li>
		</xsl:if>
	</xsl:template>

	<xsl:template match="MenuItem" mode="adminLinkMainMenu">
		<xsl:if test="@display='true'">
			<xsl:variable name="href">
				<xsl:value-of select="$appPath"/>
				<xsl:text>?ewCmd=</xsl:text>
				<xsl:value-of select="@cmd"/>
				<xsl:if test="parent::MenuItem[@cmd!='AdmHome']">
					<xsl:text>&amp;pgid=</xsl:text>
					<xsl:value-of select="/Page/@id"/>
				</xsl:if>
			</xsl:variable>
			<a href="{$href}" title="{Description}" class="">
				<xsl:attribute name="class">
					<!--<xsl:text>btn btn-large btn-primary btn-block btn-app </xsl:text>-->
					<xsl:choose>
						<xsl:when test="self::MenuItem[@cmd=/Page/@ewCmd]">
							<xsl:text>active</xsl:text>
						</xsl:when>
						<xsl:when test="descendant::MenuItem[@cmd=/Page/@ewCmd] and @cmd!='AdmHome'">
							<xsl:text>on</xsl:text>
						</xsl:when>
					</xsl:choose>
				</xsl:attribute>
				<i class="fa {@icon} fa-2x">
					<xsl:text> </xsl:text>
				</i>
				<xsl:value-of select="@name"/>
			</a>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template match="MenuItem" mode="adminLink">
		<xsl:if test="@display='true'">
			<xsl:variable name="href">
				<xsl:value-of select="$appPath"/>
				<xsl:text>?ewCmd=</xsl:text>
				<xsl:value-of select="@cmd"/>
				<xsl:if test="parent::MenuItem[@cmd!='AdmHome']">
					<xsl:text>&amp;pgid=</xsl:text>
					<xsl:value-of select="/Page/@id"/>
				</xsl:if>
			</xsl:variable>
			<li>
				<a href="{$href}" title="{Description}">
					<xsl:choose>
						<xsl:when test="self::MenuItem[@cmd=/Page/@ewCmd]">
							<xsl:attribute name="class">active</xsl:attribute>
						</xsl:when>
						<xsl:when test="descendant::MenuItem[@cmd=/Page/@ewCmd] and @cmd!='AdmHome'">
							<xsl:attribute name="class">on</xsl:attribute>
						</xsl:when>
					</xsl:choose>
					<xsl:value-of select="@name"/>
				</a>
			</li>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<!-- -->
	<xsl:template match="MenuItem" mode="adminLink1">
		<xsl:if test="@display='true'">
			<xsl:variable name="href">
				<xsl:value-of select="$appPath"/>
				<xsl:text>?ewCmd=</xsl:text>
				<xsl:value-of select="@cmd"/>
				<xsl:if test="parent::MenuItem[@cmd!='AdmHome'] and @cmd!='MailingList'">
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
				<!--<span class="adminSubMenu1TL">&#160;</span>
        <span class="adminSubMenu1Icon">
          <xsl:variable name="adminSubMenuLiClass">
            <xsl:text>adminSubMenu1Icon</xsl:text>
            <xsl:value-of select="translate(@name,' ','_')"/>
          </xsl:variable>
          <xsl:attribute name="id">
            <xsl:copy-of select="$adminSubMenuLiClass"/>
          </xsl:attribute>
          <xsl:text> </xsl:text>
        </span>-->
				<i class="fa {@icon}">
					<xsl:text> </xsl:text>
				</i>
				<span class="adminSubMenuText">
					<xsl:value-of select="@name"/>
				</span>
			</a>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<!-- -->
	<xsl:template match="MenuItem" mode="adminLink2">
		<xsl:if test="@display='true' and @name!='Preview'">
			<!-- Clone Parent Context-->
			<xsl:variable name="contextclone">
				<xsl:choose>
					<xsl:when test="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/Request//Item[@name='context']]">
						<xsl:value-of select="/Page/Request//Item[@name='context']"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="number('0')"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>

			<!-- Clone Parent Context-->
			<xsl:variable name="href">
				<xsl:value-of select="$appPath"/>
				<xsl:choose>
					<xsl:when test="@cmd='GoToClone' and /Page/@clone">
						<xsl:text>?ewCmd=Normal&amp;pgid=</xsl:text>
						<xsl:choose>

							<!-- If this is a cloned page, then point at the original page -->
							<xsl:when test="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id and @clone]">
								<xsl:value-of select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@clone"/>
							</xsl:when>

							<!-- If this is cloned page's child, then simply point at the page (without the context) -->
							<xsl:otherwise>
								<xsl:value-of select="/Page/@id"/>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>?ewCmd=</xsl:text>
						<xsl:value-of select="@cmd"/>
						<xsl:if test="parent::MenuItem[@cmd!='AdmHome']">
							<xsl:text>&amp;pgid=</xsl:text>
							<xsl:value-of select="/Page/@id"/>
						</xsl:if>
						<xsl:if test="$contextclone &gt; 0">
							<xsl:text>&amp;context=</xsl:text>
							<xsl:value-of select="$contextclone"/>
						</xsl:if>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>
			<a href="{$href}" title="{Description}">
				<xsl:choose>
					<xsl:when test="self::MenuItem[@cmd=/Page/@ewCmd]">
						<xsl:attribute name="class">active</xsl:attribute>
					</xsl:when>
					<xsl:when test="descendant::MenuItem[@cmd=/Page/@ewCmd] and @cmd!='AdmHome'">
						<xsl:attribute name="class">on</xsl:attribute>
					</xsl:when>
				</xsl:choose>
				<span class="adminSubMenu2TL">&#160;</span>
				<i class="fa {@icon}">
					<xsl:text> </xsl:text>
				</i>
				<xsl:value-of select="@name"/>
			</a>
		</xsl:if>
	</xsl:template>

	<xsl:template match="MenuItem" mode="previewLink">
		<xsl:if test="@display='true' and @name='Preview'">
			<!-- Clone Parent Context-->
			<xsl:variable name="contextclone">
				<xsl:choose>
					<xsl:when test="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/Request//Item[@name='context']]">
						<xsl:value-of select="/Page/Request//Item[@name='context']"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="number('0')"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>

			<!-- Clone Parent Context-->
			<xsl:variable name="href">
				<xsl:choose>
					<xsl:when test="@cmd='GoToClone' and /Page/@clone">
						<xsl:text>?ewCmd=Normal&amp;pgid=</xsl:text>
						<xsl:choose>

							<!-- If this is a cloned page, then point at the original page -->
							<xsl:when test="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id and @clone]">
								<xsl:value-of select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@clone"/>
							</xsl:when>

							<!-- If this is cloned page's child, then simply point at the page (without the context) -->
							<xsl:otherwise>
								<xsl:value-of select="/Page/@id"/>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>?ewCmd=</xsl:text>
						<xsl:value-of select="@cmd"/>
						<xsl:if test="parent::MenuItem[@cmd!='AdmHome']">
							<xsl:text>&amp;pgid=</xsl:text>
							<xsl:value-of select="/Page/@id"/>
						</xsl:if>
						<xsl:if test="$contextclone &gt; 0">
							<xsl:text>&amp;context=</xsl:text>
							<xsl:value-of select="$contextclone"/>
						</xsl:if>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>
			<a href="{$href}" title="{Description}">
				<xsl:if test="/Page/@ewCmd='NormalMail'">
					<xsl:attribute name="target">_blank</xsl:attribute>
				</xsl:if>
				<xsl:choose>
					<xsl:when test="self::MenuItem[@cmd=/Page/@ewCmd]">
						<xsl:attribute name="class">active</xsl:attribute>
					</xsl:when>
					<xsl:when test="descendant::MenuItem[@cmd=/Page/@ewCmd] and @cmd!='AdmHome'">
						<xsl:attribute name="class">on</xsl:attribute>
					</xsl:when>
				</xsl:choose>
				<span class="adminSubMenu2TL">&#160;</span>
				<i class="fa {@icon}">
					<xsl:text> </xsl:text>
				</i>
				<span>
					<xsl:value-of select="@name"/>
				</span>
			</a>
		</xsl:if>
	</xsl:template>

	<xsl:template match="MenuItem" mode="button">
		<xsl:if test="@display='true'">
			<!-- Clone Parent Context-->
			<xsl:variable name="contextclone">
				<xsl:choose>
					<xsl:when test="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/Request//Item[@name='context']]">
						<xsl:value-of select="/Page/Request//Item[@name='context']"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="number('0')"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>

			<!-- Clone Parent Context-->
			<xsl:variable name="href">
				<xsl:choose>
					<xsl:when test="@cmd='GoToClone' and /Page/@clone">
						<xsl:text>?ewCmd=Normal&amp;pgid=</xsl:text>
						<xsl:choose>

							<!-- If this is a cloned page, then point at the original page -->
							<xsl:when test="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id and @clone]">
								<xsl:value-of select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@clone"/>
							</xsl:when>

							<!-- If this is cloned page's child, then simply point at the page (without the context) -->
							<xsl:otherwise>
								<xsl:value-of select="/Page/@id"/>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:when>
					<xsl:otherwise>
						<xsl:text>?ewCmd=</xsl:text>
						<xsl:value-of select="@cmd"/>
						<xsl:if test="parent::MenuItem[@cmd!='AdmHome']">
							<xsl:text>&amp;pgid=</xsl:text>
							<xsl:value-of select="/Page/@id"/>
						</xsl:if>
						<xsl:if test="$contextclone &gt; 0">
							<xsl:text>&amp;context=</xsl:text>
							<xsl:value-of select="$contextclone"/>
						</xsl:if>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>
			<li>
				<a href="{$href}" title="{Description}" class="btn btn-lg btn-primary">
					<i class="fa {@icon} fa-large">
						<xsl:text> </xsl:text>
					</i>
					<xsl:text> </xsl:text>
					<xsl:value-of select="@name"/>
				</a>
			</li>
		</xsl:if>
	</xsl:template>


	<xsl:template match="MenuItem[@cmd='PageVersions']" mode="adminLink2">
		<xsl:if test="@display='true'">

			<!-- Clone Parent Context-->
			<xsl:variable name="href">
				<xsl:text>?ewCmd=</xsl:text>
				<xsl:value-of select="@cmd"/>
				<xsl:if test="parent::MenuItem[@cmd!='AdmHome']">
					<xsl:text>&amp;pgid=</xsl:text>
					<xsl:value-of select="/Page/@id"/>
				</xsl:if>
			</xsl:variable>

			<xsl:variable name="VersionParentId">
				<xsl:choose>
					<xsl:when test="/Page/Menu/descendant-or-self::PageVersion[@id=/Page/@id]">
						<xsl:value-of select="/Page/Menu/descendant-or-self::PageVersion[@id=/Page/@id]/parent::MenuItem/@id"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="/Page/@id"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>


			<xsl:variable name="verCount">
				<xsl:choose>
					<xsl:when test="/Page/@ewCmd='PageVersions'">
						<xsl:value-of select="count(/Page/ContentDetail/PageVersions/Version)"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="count(/Page/Menu/descendant-or-self::MenuItem[@id=$VersionParentId]/PageVersion)"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:variable>

			<xsl:choose>
				<xsl:when test="$verCount &gt; 0 ">
					<a href="{$href}" title="{Description}">
						<xsl:choose>
							<xsl:when test="self::MenuItem[@cmd=/Page/@ewCmd]">
								<xsl:attribute name="class">active</xsl:attribute>
							</xsl:when>
							<xsl:when test="descendant::MenuItem[@cmd=/Page/@ewCmd] and @cmd!='AdmHome'">
								<xsl:attribute name="class">on</xsl:attribute>
							</xsl:when>
						</xsl:choose>
						<i class="fa {@icon} fa-large">
							<xsl:text> </xsl:text>
						</i>
						<xsl:value-of select="$verCount"/> : <xsl:value-of select="@name"/>
					</a>
				</xsl:when>
				<xsl:otherwise>
					<a href="{$appPath}?ewCmd=NewPageVersion&amp;pgid={/Page/@id}&amp;vParId={/Page/@id}" title="{Description}">
						<xsl:if test="/Page[@ewCmd='NewPageVersion']">
							<xsl:attribute name="class">active on</xsl:attribute>
						</xsl:if>
						<i class="fa {@icon} fa-large">
							<xsl:text> </xsl:text>
						</i>
						<xsl:text>New Page Version</xsl:text>
					</a>
				</xsl:otherwise>
			</xsl:choose>

		</xsl:if>
	</xsl:template>
	<!-- -->
	<!--   ##############################   submenu for Advanced Mode   ##############################   -->
	<!-- -->
	<xsl:template match="MenuItem" mode="menuitem_am">
		<xsl:param name="level"/>
		<li id="node{@id}" data-tree-level="{$level}" data-tree-parent="{./parent::MenuItem/@id}" >
			<xsl:attribute name="class">
				<xsl:if test="@cloneparent &gt; 0 and not(@cloneparent=@id)">
					<xsl:text>clone context</xsl:text>
					<xsl:value-of select="@cloneparent"/>
				</xsl:if>
				<xsl:text> list-group-item level-</xsl:text>
				<xsl:value-of select="$level"/>
				<xsl:if test="MenuItem"> expandable</xsl:if>
			</xsl:attribute>
			<xsl:apply-templates select="self::MenuItem" mode="menuLink_am"/>
		</li>
		<xsl:if test="descendant-or-self::MenuItem[@id=/Page/@id]/@id">
			<xsl:apply-templates select="MenuItem" mode="menuitem_am">
				<xsl:with-param name="level">
					<xsl:value-of select="$level + 1"/>
				</xsl:with-param>
			</xsl:apply-templates>
		</xsl:if>
	</xsl:template>

	<!-- -->
	<xsl:template match="MenuItem" mode="menuLink_am">
		<div class="pageCell">
			<xsl:variable name="displayName">
				<xsl:apply-templates select="." mode="getDisplayName" />
			</xsl:variable>
			<a href="{$appPath}?pgid={@id}">
				<xsl:apply-templates select="." mode="status_legend"/>
				<span class="pageName">
					<xsl:value-of select="$displayName"/>
				</span>
			</a>
		</div>
	</xsl:template>


	<!-- -->
	<!--   ##############################   Advanced Mode   ##############################   -->
	<!-- -->

	<xsl:template match="Page" mode="Admin">
		<div class="row" id="template_1_Column">
			<div class="col-md-12">
				<div class="panel panel-default">
					<div class="panel-heading">
						<h3 class="title">
							Feature not yet available - <xsl:value-of select="@ewCmd"/>
						</h3>
					</div>
					<div class="panel-body">
						This page is a placeholder for a feature that we plan to make available in due course
					</div>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Page[@layout='AdmHome']" mode="Admin">
		<xsl:variable name="supportEmail">
			<xsl:call-template name="eonicwebSupportEmail"/>
		</xsl:variable>
		<xsl:variable name="supportWebsite">
			<xsl:call-template name="eonicwebWebsite"/>
		</xsl:variable>

		<section>

			<div class="row">
				<div class="col-md-9">
					<div class="row">
						<div class="col-md-4">
							<div class="matchHeight dashboard-first-column">
								<div class="panel panel-default">
									<div class="panel-heading">
										<h3 class="panel-title">What's New</h3>
									</div>
									<div class="panel-body">
										<xsl:choose>
											<xsl:when test="$page/Settings/add[@key='web.eonicwebProductName']/@value!=''">
												<!--xsl:value-of select="$page/Settings/add[@key='web.eonicwebProductName']/@value"/-->
											</xsl:when>
											<xsl:otherwise>
												<h3>
													<strong>ProteanCMS</strong>
												</h3>
												<p>ProteanCMS is fully opensource.</p>
												<a href="https://www.proteancms.com">For more information click here.</a>
											</xsl:otherwise>
										</xsl:choose>
									</div>
								</div>
								<div class="panel panel-default">
									<div class="panel-heading">
										<h3 class="panel-title">Get Help</h3>
									</div>
									<div class="panel-body">
										<xsl:if test="not($page/Settings/add[@key='web.eonicwebProductName']/@value!='')">
											<h5>
												<a href="https://www.facebook.com/proteancms" class="" target="_new">
													<i class="fa fa-facebook-square fa-lg">&#160;</i>&#160;Follow ProteanCMS on Facebook
												</a>
											</h5>
											<h5>
												<a href="https://www.linkedin.com/groups?gid=1840777" class="" target="_new">
													<i class="fa fa-linkedin-square fa-lg">&#160;</i>&#160;Join our LinkedIn Group
												</a>
											</h5>


										</xsl:if>
										<h5>
											<i class="fa fa-phone">&#160;</i>&#160;<xsl:call-template name="eonicwebSupportTelephone"/>
										</h5>
										<h5>
											<i class="fa fa-envelope">&#160;</i>&#160;<a href="mailto:{$supportEmail}" title="Email Support">
												<xsl:value-of select="$supportEmail"/>
											</a>
										</h5>
										<h5>
											<i class="fa fa-globe">&#160;</i>&#160;	<a title="view the latest news">
												<xsl:attribute name="href">
													<xsl:text>http://{$supportWebsite}?utm_campaign=cmsadminsystem&amp;utm_source=</xsl:text>
													<xsl:value-of select="//ServerVariables/Item[@name='SERVER_NAME']/node()"/>
												</xsl:attribute>
												<xsl:value-of select="$supportWebsite"/>
											</a>
										</h5>
									</div>
								</div>

							</div>
						</div>
						<div class="col-md-4">
							<div class="panel panel-default matchHeight">
								<div class="panel-heading">
									<h3 class="panel-title">Performance Tips</h3>
								</div>
								<div class="panel-body">
									<p>Have you checked Google Analytics recently ?</p>
								</div>
							</div>
						</div>
						<div class="col-md-4">
							<div class="panel panel-default matchHeight">
								<div class="panel-heading">
									<h3 class="panel-title">Insights</h3>
								</div>
								<div class="panel-body">
									<div id="insights-section">
										<xsl:for-each select="$page/AdminMenu/MenuItem/Module">
											<xsl:if test="@name != ''">
												<xsl:variable name="id" select="@id"/>
												<xsl:variable name="jsonURL" select="@jsonURL"/>
												<div id="metric_{position()}" class="metric" data-json-url="{$jsonURL}">
													<div class="metric-inner">
														<header class="metric-header">
															<h1 class="metric-title">
																<xsl:value-of select="@name"/>
															</h1>
														</header>
														<div class="metric-body">
															<div class="value">
																<h1 class="metric-value" v-for="result in filterResultArray('metric_{position()}')">
																	<b>{{result.Key}}</b>: {{result.Value}}<br/>
																</h1>
															</div>
															<div class="buttons" style="text-align: center;">
																<xsl:if test="@url!=''">
																	<br/>
																	<a href="{@url}" class="btn btn-default">
																		<xsl:value-of select="@name"/>
																	</a>
																</xsl:if>
															</div>

														</div>
													</div>
												</div>
											</xsl:if>
										</xsl:for-each>
									</div>
								</div>
							</div>
						</div>
					</div>
				</div>
				<div class="col-md-3">
					<div class="panel panel-default matchHeight">
						<div class="panel-heading">
							<h3 class="panel-title">Features Enabled</h3>
						</div>
						<ul class="nav nav-stacked featuresEnabled">

							<li>
								<a href="" class="btn btn-success">
									<i class="fa fa-ok">
										<xsl:text> </xsl:text>
									</i>
									<xsl:text> </xsl:text>
									Content Management<br/>
									<span class="btnNotes">
										<xsl:value-of select="/Page/ContentDetail/Status/Status/@activePageCount"/> active pages of <xsl:value-of select="/Page/ContentDetail/Status/Status/@totalPageCount"/> pages <xsl:value-of select="/Page/ContentDetail/Status/Status/@contentCount"/> items.<br/>
										<xsl:value-of select="/Page/ContentDetail/Status/Status/@totalPageRedirects"/> Page Redirects
									</span>
									<br/><strong>
										<xsl:choose>
											<xsl:when test="/Page/ContentDetail/Status/Status/@activePageCount &lt; 50 and /Page/ContentDetail/Status/Status/@contentCount &lt; 500">
												Lite Licence
											</xsl:when>
											<xsl:otherwise>
												Pro Licence
											</xsl:otherwise>
										</xsl:choose>
									</strong>
								</a>
							</li>
							<li>
								<a href="" class="btn btn-default">
									<i class="fa fa-plus">
										<xsl:text> </xsl:text>
									</i>
									<xsl:text> </xsl:text>Multi-Language
								</a>
							</li>
							<li>
								<a href="" class="btn btn-default">
									<i class="fa fa-plus">
										<xsl:text> </xsl:text>
									</i>
									<xsl:text> </xsl:text>Page Versions
								</a>
							</li>
							<li>
								<xsl:choose>
									<xsl:when test="/Page/ContentDetail/Status/Status/Cart/node() = 'on'">
										<a href="" class="btn btn-success">
											<i class="fa fa-check">
												<xsl:text> </xsl:text>
											</i>
											<xsl:text> </xsl:text>eCommerce
										</a>
									</xsl:when>
									<xsl:otherwise>
										<a href="" class="btn btn-default">
											<i class="fa fa-plus">
												<xsl:text> </xsl:text>
											</i>
											<xsl:text> </xsl:text>eCommerce
										</a>
									</xsl:otherwise>
								</xsl:choose>
							</li>
							<li>
								<xsl:choose>
									<xsl:when test="/Page/ContentDetail/Status/Status/Membership/node() = 'on'">
										<a href="" class="btn btn-success">
											<i class="fa fa-check">
												<xsl:text> </xsl:text>
											</i>
											<xsl:text> </xsl:text>Membership
										</a>
									</xsl:when>
									<xsl:otherwise>
										<a href="" class="btn btn-default">
											<i class="fa fa-plus">
												<xsl:text> </xsl:text>
											</i>
											<xsl:text> </xsl:text>Membership
										</a>
									</xsl:otherwise>
								</xsl:choose>
							</li>
							<li>
								<xsl:choose>
									<xsl:when test="/Page/ContentDetail/Status/Status/MailingList/node() = 'on'">
										<a href="" class="btn btn-success">
											<i class="fa fa-check">
												<xsl:text> </xsl:text>
											</i>
											<xsl:text> </xsl:text>Email Marketing
										</a>
									</xsl:when>
									<xsl:otherwise>
										<a href="" class="btn btn-default">
											<i class="fa fa-plus">
												<xsl:text> </xsl:text>
											</i>
											<xsl:text> </xsl:text>Email Marketing
										</a>
									</xsl:otherwise>
								</xsl:choose>
							</li>
							<li class="active">
								<a href="" class="btn btn-default">
									<i class="fa fa-plus">
										<xsl:text> </xsl:text>
									</i>
									<xsl:text> </xsl:text>SEO Reporting
								</a>
							</li>
							<li>
								<!--Not working-->
								<xsl:choose>
									<xsl:when test="/Page/ContentDetail/Status/Status/PageCache/node() = 'on'">
										<a href="" class="btn btn-success">
											<i class="fa fa-check">
												<xsl:text> </xsl:text>
											</i>
											<xsl:text> </xsl:text>Page Cache
										</a>
									</xsl:when>
									<xsl:otherwise>
										<a href="" class="btn btn-default">
											<i class="fa fa-plus">
												<xsl:text> </xsl:text>
											</i>
											<xsl:text> </xsl:text>Page Cache
										</a>
									</xsl:otherwise>
								</xsl:choose>
							</li>
							<xsl:choose>

								<xsl:when test="/Page/ContentDetail/Status/Status/Debug/node() = 'on'">
									<li class="active">
										<a href="{$appPath}?ewCmd=WebSettings" class="btn btn-danger">
											<i class="fa fa-bug">
												<xsl:text> </xsl:text>
											</i>
											<xsl:text> </xsl:text>Debug Mode Enabled
										</a>
									</li>
									<div class="bg-danger small well-sm">
										Debug mode turns off some compression and performance features. It also reports any errors directly to screen rather than showing a friendly error page.<br/><br/> Debug Mode should be turned <strong>off</strong> on live websites.
									</div>
								</xsl:when>
								<xsl:otherwise>
									<li>
										<a href="{$appPath}?rebundle=true" class="btn btn-warning">
											<i class="fas fa-recycle">
												<xsl:text> </xsl:text>
											</i>
											<xsl:text> </xsl:text>Rebundle JS / CSS files
										</a>
									</li>
								</xsl:otherwise>
							</xsl:choose>
							<li>
								<a href="{$appPath}?recompile=true" class="btn btn-warning bs-please-wait" data-pleasewaitmessage="Recompiling - Please wait a moment.">
									<i class="fas fa-recycle">
										<xsl:text> </xsl:text>
									</i>
									<xsl:text> </xsl:text>Recompile XSLT and Rebundle
								</a>
							</li>
							<xsl:if test="ContentDetail/Status/Status/DBVersion/node()!=ContentDetail/Status/Status/LatestDBVersion/node() and User/@name='Admin'">
								<li>
									<a href="/ewcommon/setup/?ewCmd=UpgradeDB" class="btn btn-default">
										<i class="fa fa-refresh">
											<xsl:text> </xsl:text>
										</i>
										<xsl:text> </xsl:text>Update Schema from <br/><xsl:value-of select="ContentDetail/Status/Status/DBVersion/node()"/> to <xsl:value-of select="ContentDetail/Status/Status/LatestDBVersion/node()"/>
									</a>
								</li>
							</xsl:if>
						</ul>
					</div>
				</div>
			</div>

		</section>
	</xsl:template>

	<xsl:template match="Page[@layout='SettingsDash']" mode="Admin">
		<div class="row">
			<div class="col-md-3 panel">
				<div class="panel-body">
					<ul class="nav nav-stacked">
						<xsl:for-each select="AdminMenu/descendant-or-self::MenuItem[@cmd='SettingsDash']/MenuItem">
							<xsl:apply-templates select="." mode="button">
								<xsl:with-param name="level">1</xsl:with-param>
							</xsl:apply-templates>
						</xsl:for-each>
						<li>
							<a href="?reBundle=true" title="" class="btn btn-lg btn-primary">
								<i class="fa fa-gift fa-large">
									<xsl:text> </xsl:text>
								</i><xsl:text> </xsl:text>
								Rebundle CSS /JS
							</a>
						</li>
					</ul>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Page[@layout='ResetWebConfig']" mode="Admin">
		<div class="row">
			<div class="col-md-3 panel">
				<div class="panel-body">
					<ul class="nav nav-stacked">
						<xsl:for-each select="AdminMenu/descendant-or-self::MenuItem[@cmd='ResetWebConfig']/MenuItem">
							<xsl:apply-templates select="." mode="button">
								<xsl:with-param name="level">1</xsl:with-param>
							</xsl:apply-templates>
						</xsl:for-each>
						<li>

							<a href="{$appPath}?ewCmd=ResetWebConfig&amp;pgid={/Page/@id}" title="" class="btn btn-lg btn-primary">
								<i class="fa fa-gift fa-large">
									<xsl:text> </xsl:text>
								</i><xsl:text> </xsl:text>
								Reset WebConfig
							</a>
						</li>
					</ul>
				</div>
			</div>
		</div>
	</xsl:template>
	<!-- -->
	<xsl:template match="Page[@layout='Advanced' or @layout='AdvancedMail']" mode="Admin">
		<div class="row" id="tpltAdvancedMode">
			<div class="col-md-3">
				<div class="panel panel-default">
					<div class="panel-heading">
						<h4 class="panel-title">Navigate to page</h4>
					</div>
					<ul id="MenuTree" class="list-group treeview">
						<xsl:apply-templates select="Menu/MenuItem" mode="menuitem_am">
							<xsl:with-param name="level" select="'1'"/>
						</xsl:apply-templates>
					</ul>
				</div>
			</div>
			<div class="col-md-9">
				<form action="{$appPath}" method="get" class="ewXform">
					<!--input type="hidden" name="ewCmd" value="BulkContentAction"/>
					<input type="hidden" name="pgid" value="{$page/@id}"/-->
					<div class="panel panel-default">
						<div class="panel-heading">
							<div class="panel-heading-buttons">
								<xsl:apply-templates select="." mode="bulkActionForm"/>
							</div>
							<h4 class="panel-title">
								All content on page - <strong>
									<xsl:apply-templates select="$currentPage" mode="getDisplayName"/>
								</strong>
							</h4>
						</div>
						<div class="panel-group" id="accordion">
							<xsl:for-each select="/Page/ContentDetail/ContentTypes/ContentTypeGroup/ContentType">
								<xsl:apply-templates select="/" mode="ListByContentType">
									<xsl:with-param name="contentType" select="@type"/>
								</xsl:apply-templates>
							</xsl:for-each>
						</div>
					</div>
				</form>
			</div>
		</div>
	</xsl:template>
	<!-- -->
	<xsl:template match="/" mode="siteMenuKey">

	</xsl:template>
	<!-- -->
	<xsl:template match="/" mode="ListByContentType">
		<xsl:param name="contentType"/>
		<div class="panel panel-default">
			<div class="panel-heading">
				<xsl:if test="$contentType!='Module'">
					<div class="pull-right">
						<xsl:apply-templates select="/Page" mode="inlinePopupAdd">
							<xsl:with-param name="type">
								<xsl:value-of select="$contentType"/>
							</xsl:with-param>
							<xsl:with-param name="text">Add New</xsl:with-param>
							<xsl:with-param name="name">
								<xsl:text>New </xsl:text>
								<xsl:value-of select="$contentType"/>
							</xsl:with-param>
						</xsl:apply-templates>
					</div>
				</xsl:if>
				<h6 class="panel-title">
					<a class="accordion-toggle" data-toggle="collapse" data-parent="#accordion" href="#collapse{$contentType}">
						<i class="fa fa-chevron-down">
							<xsl:text> </xsl:text>
						</i>
						<span>
							<xsl:value-of select="$contentType"/> (<xsl:value-of select="count(Page/Contents/Content[@type=$contentType])"/>)
						</span>
					</a>
				</h6>
			</div>
			<div id="collapse{$contentType}" class="panel-collapse collapse">
				<table class="table table-striped-2">
					<xsl:if test="not(Page/Contents/Content[@type=$contentType])">
						<tr>
							<td colspan="3">
								<xsl:text>No </xsl:text>
								<xsl:value-of select="$contentType"/>
								<xsl:text> on this page</xsl:text>
							</td>
						</tr>
					</xsl:if>

					<xsl:apply-templates select="Page/Contents/Content[@type=$contentType]" mode="AdvancedMode">
						<xsl:with-param name="contentType" select="$contentType"/>
					</xsl:apply-templates>
				</table>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="*" mode="bulkActionForm">
		<input type="hidden" name="ewCmd" value="BulkContentAction"/>
		<input type="hidden" name="pgid" value="{$page/@id}"/>

		<div class="form-group bulk-action">
			<div class="input-group">
				<label class="input-group-addon">Bulk Action</label>
				<select class="form-control" name="BulkAction" id="BulkAction">
					<option value="Move">Move</option>
					<option value="Locate">Locate</option>
					<option value="Hide">Hide</option>
					<option value="Show">Show</option>
					<option value="Delete">Delete</option>
				</select>
				<span class="input-group-btn">
					<button type="submit" class="btn btn-primary">Go</button>
				</span>
			</div>
		</div>

	</xsl:template>

	<xsl:template match="Content" mode="AdvancedModeHeader">
		<xsl:param name="contentType"/>
		<tr>
			<th>
				Status
			</th>
			<th>
				Details
			</th>
			<th class="th-form">
				<xsl:apply-templates select="parent::*" mode="bulkActionForm"/>
			</th>
			<th>
				<a href="" class="btn btn-default">Select All</a>
			</th>
		</tr>
	</xsl:template>



	<xsl:template match="Content" mode="AdvancedMode">
		<xsl:param name="contentType"/>
		<tr>
			<td class="status">
				<xsl:apply-templates select="." mode="status_legend"/>
			</td>
			<td class="name">
				<xsl:choose>
					<xsl:when test="$contentType='Module'">
						<b>
							<xsl:value-of select="@title" />
							<xsl:if test="@title=''">No Title</xsl:if>
						</b>
						&#160;-&#160;<xsl:value-of select="@moduleType" />&#160;-&#160;<xsl:value-of select="@position" />
					</xsl:when>
					<xsl:when test="$contentType='FlashMovie' and object/@title!=''">
						<b>
							<xsl:value-of select="object/@title" />
						</b>
					</xsl:when>
					<xsl:when test="@name!=''">
						<b>
							<xsl:value-of select="@name" />
						</b>
					</xsl:when>
					<xsl:when test="@type='Module'">
						<p>
							<xsl:apply-templates select="." mode="getSynopsis" />
						</p>
					</xsl:when>
				</xsl:choose>
				<br/>
				<small>
					<xsl:if test="@publish!=''">
						<span class="publishedOn">
							<b>Published: </b>
							<xsl:call-template name="DD_Mon_YYYY">
								<xsl:with-param name="date" select="@publish"/>
							</xsl:call-template>
						</span>&#160;
					</xsl:if>
					<xsl:if test="@expire!=''">
						<span class="expiresOn">
							<xsl:text> / </xsl:text>
							<b>Expires:</b>
							<xsl:call-template name="DD_Mon_YYYY">
								<xsl:with-param name="date" select="@expire"/>
							</xsl:call-template>
						</span>&#160;
					</xsl:if>
					<xsl:if test="@update!=''">
						<span class="update">
							<b>Last Updated: </b>
							<xsl:call-template name="DD_Mon_YYYY">
								<xsl:with-param name="date" select="@update"/>
							</xsl:call-template>
						</span>
					</xsl:if>
				</small>
			</td>
			<td class="optionsButton">
				<xsl:apply-templates select="." mode="inlineOptionsNoPopup">
					<xsl:with-param name="class" select="'list'"/>
				</xsl:apply-templates>
				<xsl:text> </xsl:text>
			</td>
			<td>
				<div class="checkbox checkbox-primary">
					<input type="checkbox" name="id" value="{@id}" class="styled inventory-bulk-checkbox" data-status="{@status}"/>
					<label>
						<xsl:text> </xsl:text>
					</label>
				</div>
			</td>
		</tr>
	</xsl:template>

	<xsl:template match="Content[@type='Product']" mode="AdvancedModeHeader">
		<xsl:param name="contentType"/>
		<tr>
			<th>
				Status
			</th>
			<th>
				&#160;
			</th>
			<th>
				Manufacturer
			</th>
			<th>
				Product Name
			</th>
			<th>
				Stockcode
			</th>
			<th>
				Stock
			</th>
			<th>
				Price
			</th>
			<th class="th-form">
				<xsl:apply-templates select="parent::*" mode="bulkActionForm"/>
			</th>
			<th>
				<div class="checkbox checkbox-primary">
					<input type="checkbox" class="styled select-all"/>
					<label>
						<xsl:text>All</xsl:text>
					</label>
				</div>
			</th>
		</tr>
	</xsl:template>

	<xsl:template match="Content[@type='Product']" mode="AdvancedMode">
		<xsl:param name="contentType"/>
		<tr>
			<td class="status" rowspan="2">
				<xsl:apply-templates select="." mode="status_legend"/>
			</td>
			<td class="pic" rowspan="2">

				<xsl:apply-templates select="." mode="displayThumbnail">
					<xsl:with-param name="forceResize">true</xsl:with-param>
					<xsl:with-param name="crop">true</xsl:with-param>
					<xsl:with-param name="width">50</xsl:with-param>
					<xsl:with-param name="height">50</xsl:with-param>
				</xsl:apply-templates>

			</td>


			<td class="manufacturer">
				<xsl:value-of select="Manufacturer" />
			</td>
			<td class="name">
				<xsl:value-of select="Name" />
			</td>
			<td class="stockcode">
				<xsl:value-of select="StockCode" />
			</td>
			<td class="stock">
				<xsl:value-of select="Stock" />
			</td>
			<td class="price">
				<xsl:apply-templates select="." mode="displayPrice" />
			</td>
			<td class="optionsButton" nowrap="nowrap" rowspan="2">
				<xsl:apply-templates select="." mode="inlineOptionsNoPopup">
					<xsl:with-param name="class" select="'list'"/>
				</xsl:apply-templates>
				<xsl:text> </xsl:text>
			</td>
			<td>
				<div class="checkbox checkbox-primary">
					<input type="checkbox" name="id" value="{@id}" class="styled"/>
					<label>
						<xsl:text> </xsl:text>
					</label>
				</div>
			</td>
		</tr>
		<tr>
			<td colspan="4" class="detail">
				<xsl:variable name="pageId">
					<xsl:value-of select="@parId"/>
				</xsl:variable>
				<xsl:variable name="pageUrl">
					<xsl:apply-templates select="$page/Menu/descendant-or-self::MenuItem[@id=$pageId]" mode="getHref"/>
				</xsl:variable>
				<a href="?ewCmd=ByType.Product.Location&amp;Location={@parId}">
					<xsl:value-of select="substring-before($pageUrl,'?')"/>
				</a>
			</td>
			<td class="detail">
				&#160;
			</td>
		</tr>
		<xsl:apply-templates select="Content[@type='SKU']" mode="AdvancedMode">
			<xsl:with-param name="contentType" select="'SKU'"/>
			<xsl:with-param name="parId" select="@parId"/>
		</xsl:apply-templates>
	</xsl:template>

	<xsl:template match="Content[@type='SKU']" mode="AdvancedMode">
		<xsl:param name="parId"/>
		<xsl:param name="contentType"/>
		<tr>
			<td class="status">
				<xsl:apply-templates select="." mode="status_legend"/>
			</td>
			<td class="pic">
				<xsl:text>SKU</xsl:text>

			</td>
			<td class="manufacturer">
				<xsl:text> </xsl:text>
			</td>
			<td class="name">
				<xsl:value-of select="Name" />
			</td>
			<td class="stockcode">
				<xsl:value-of select="StockCode" />
			</td>
			<td class="stock">
				<xsl:value-of select="Stock" />
			</td>
			<td class="price">
				<xsl:apply-templates select="." mode="displayPrice" />
			</td>
			<td class="optionsButton" nowrap="nowrap">
				<xsl:apply-templates select="." mode="inlineOptionsNoPopup">
					<xsl:with-param name="class" select="'list'"/>
					<xsl:with-param name="parId" select="$parId"/>
				</xsl:apply-templates>
				<xsl:text> </xsl:text>
			</td>
			<td>
				<div class="checkbox checkbox-primary">
					<input type="checkbox" name="id" value="{@id}" class="styled"/>
					<label>
						<xsl:text> </xsl:text>
					</label>
				</div>
			</td>
		</tr>
	</xsl:template>


	<xsl:template match="Content[@type='FAQ']" mode="AdvancedModeHeader">
		<xsl:param name="contentType"/>
		<tr>
			<th>
				Status
			</th>
			<th>
				Experience
			</th>
			<th>
				Answer
			</th>
			<th style="width:400px" class="th-form">
				<xsl:apply-templates select="parent::*" mode="bulkActionForm"/>
			</th>
		</tr>
	</xsl:template>

	<xsl:template match="Content[@type='FAQ']" mode="AdvancedMode">
		<xsl:param name="contentType"/>
		<xsl:param name="parId"/>
		<tr id="hide-{@id}">
			<td class="status">
				<xsl:apply-templates select="." mode="status_legend"/>
			</td>
			<td class="name">
				<xsl:choose>
					<xsl:when test="$contentType='Module'">
						<b>
							<xsl:value-of select="@title" />
							<xsl:if test="@title=''">No Title</xsl:if>
						</b>
						&#160;-&#160;<xsl:value-of select="@moduleType" />&#160;-&#160;<xsl:value-of select="@position" />
					</xsl:when>
					<xsl:when test="$contentType='FlashMovie' and object/@title!=''">
						<b>
							<xsl:value-of select="object/@title" />
						</b>
					</xsl:when>
					<xsl:when test="@name!=''">
						<b>
							<xsl:value-of select="@name" />
						</b>
					</xsl:when>
					<xsl:when test="@type='Module'">
						<p>
							<xsl:apply-templates select="." mode="getSynopsis" />
						</p>
					</xsl:when>
				</xsl:choose>

			</td>
			<td class="body">
				<strong>
					<xsl:apply-templates select="DisplayName" mode="cleanXhtml"/>
				</strong>
				<br/>
				<xsl:apply-templates select="Body" mode="cleanXhtml"/>
				<br/>
				<small>
					<xsl:if test="@publish!=''">
						<span class="publishedOn">
							<b>Created: </b>
							<xsl:call-template name="DD_Mon_YYYY">
								<xsl:with-param name="date" select="@publish"/>
							</xsl:call-template>
						</span>&#160;
					</xsl:if>
					<xsl:if test="@expire!=''">
						<span class="expiresOn">
							<xsl:text> / </xsl:text>
							<b>Expires:</b>
							<xsl:call-template name="DD_Mon_YYYY">
								<xsl:with-param name="date" select="@expire"/>
							</xsl:call-template>
						</span>&#160;
					</xsl:if>
					<xsl:if test="@update!=''">
						<span class="update">
							<b>Last Updated: </b>
							<xsl:call-template name="DD_Mon_YYYY">
								<xsl:with-param name="date" select="@update"/>
							</xsl:call-template>
						</span>
					</xsl:if>
				</small>
			</td>
			<td class="optionsButton">
				<a href="javascript:markAsRead('{/Page/User/@id}','{@id}')" class="btn btn-xs btn-default" title="Mark as read">
					<i class="fa fa-eye fa-white">
						<xsl:text> </xsl:text>
					</i><xsl:text> </xsl:text>Mark as read
				</a>
				<xsl:choose>
					<xsl:when test="$parId!=''">
						<a href="{$appPath}?ewCmd=EditContent&amp;id={@id}" class="btn btn-xs btn-primary" title="Click here to edit this content">
							<i class="fa fa-edit fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Edit
						</a>
					</xsl:when>
					<xsl:when test="/Page/@id=@parId">
						<a href="{$appPath}?ewCmd=EditContent&amp;id={@id}" class="btn btn-xs btn-primary" title="Click here to edit this content">
							<i class="fa fa-edit fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Edit
						</a>
					</xsl:when>
					<xsl:otherwise>
						<a href="{$appPath}?ewCmd=EditContent&amp;id={@id}" class="btn btn-xs btn-primary" title="Click here to edit this content">
							<i class="fa fa-edit fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Edit
						</a>

					</xsl:otherwise>
				</xsl:choose>
				<xsl:if test="@status='1'">

					<a href="{$appPath}?ewCmd=HideContent&amp;pgid={/Page/@id}&amp;id={@id}" title="Click here to hide this item" class="btn btn-xs btn-primary">
						<i class="fa fa-eye-slash">&#160;</i>&#160;Hide
					</a>

				</xsl:if>
				<xsl:if test="@status='0'">

					<a href="{$appPath}?ewCmd=ShowContent&amp;pgid={/Page/@id}&amp;id={@id}" title="Click here to show this item" class="btn btn-xs btn-success">
						<i class="fa fa-eye">&#160;</i>&#160;Show
					</a>
				</xsl:if>
				<xsl:if test="@status='0'">

					<a href="{$appPath}?ewCmd=DeleteContent&amp;pgid={/Page/@id}&amp;id={@id}" title="Click here to delete this item" class="btn btn-xs btn-danger">
						<i class="fa fa-trash-o">&#160;</i>
					</a>

				</xsl:if>


				<xsl:text> </xsl:text>
			</td>

		</tr>
	</xsl:template>

	<xsl:template match="Page" mode="inlinePopupAdd">
		<xsl:param name="type"/>
		<xsl:param name="text"/>
		<xsl:param name="name"/>
		<xsl:param name="class"/>
		<xsl:param name="find"/>
		<xsl:param name="position"/>
		<xsl:if test="AdminMenu/descendant-or-self::MenuItem[@cmd='AddContent'] and $adminMode">
			<xsl:choose>
				<xsl:when test="contains($type,',')">
					<div class="dropdown pull-right">
						<a href="#" class="btn btn-default btn-xs pull-right" data-toggle="dropdown">
							<xsl:value-of select="$text"/>
						</a>
						<ul class="dropdown-menu">
							<xsl:call-template name="inlinePopupAddOptions">
								<xsl:with-param name="type" select="$type"/>
								<xsl:with-param name="name" select="$name"/>
							</xsl:call-template>
						</ul>
					</div>
				</xsl:when>
				<xsl:when test="$type='Module'">
					<div class="dropdown">
						<a href="#" class="btn btn-default btn-xs pull-right" data-toggle="dropdown">
							<i class="fa fa-plus">&#160;</i>&#160;
							<xsl:value-of select="$text"/>&#160;
							<i class="fa fa-caret-down">&#160;</i>
						</a>
						<ul class="dropdown-menu">
							<li>
								<a href="{$appPath}?ewCmd=AddContent&amp;pgid={/Page/@id}&amp;type={$type}&amp;name={$name}">
									<i class="fa fa-plus">&#160;</i>&#160;<xsl:text>Add New Module</xsl:text>
								</a>
							</li>
						</ul>
					</div>
				</xsl:when>
				<xsl:when test="contains($find,'true')">
					<div class="ewAdmin options">
						<div class="dropdown pull-right">
							<a href="#" class="btn btn-default btn-xs pull-right" data-toggle="dropdown">
								<i class="fa fa-plus">&#160;</i>&#160;
								<xsl:value-of select="$text"/>&#160;
								<i class="fa fa-caret-down">&#160;</i>
							</a>
							<ul class="dropdown-menu">
								<li>
									<a href="{$appPath}?ewCmd=AddContent&amp;pgid={/Page/@id}&amp;type={$type}&amp;name={$name}">
										<i class="fa fa-plus">&#160;</i>&#160;<xsl:text>Add New</xsl:text>
									</a>
								</li>
								<li>
									<a href="{$appPath}?ewCmd=LocateSearch&amp;pgid={/Page/@id}&amp;type={$type}">
										<i class="fa fa-search">&#160;</i>&#160;<xsl:text>Find Existing</xsl:text>
									</a>
								</li>
							</ul>
						</div>
					</div>
				</xsl:when>
				<xsl:when test="contains($find,'only')">
					<div class="ewAdmin options">
						<div class="dropdown pull-right">
							<a href="#" class="btn btn-default btn-xs pull-right" data-toggle="dropdown">
								<i class="fa fa-plus">&#160;</i>&#160;
								<xsl:value-of select="$text"/>&#160;
								<i class="fa fa-caret-down">&#160;</i>
							</a>
							<ul class="dropdown-menu">
								<li>
									<a href="{$appPath}?ewCmd=LocateSearch&amp;pgid={/Page/@id}&amp;type={$type}">
										<i class="fa fa-search">&#160;</i>&#160;<xsl:text>Find Existing</xsl:text>
									</a>
								</li>
							</ul>
						</div>
					</div>
				</xsl:when>
				<xsl:otherwise>
					<div class="ewAdmin options">
						<xsl:variable name="href">
							<xsl:text>?ewCmd=AddContent</xsl:text>
							<xsl:text>&amp;pgid=</xsl:text>
							<xsl:value-of select="/Page/@id"/>
							<xsl:text>&amp;type=</xsl:text>
							<xsl:value-of select="$type"/>
							<xsl:text>&amp;name=</xsl:text>
							<xsl:value-of select="$name"/>
							<xsl:if test="$position!=''">
								<xsl:text>&amp;position=</xsl:text>
								<xsl:value-of select="$position"/>
							</xsl:if>
						</xsl:variable>
						<a class="btn btn-default btn-xs pull-right" href="{$href}">
							<i class="fa fa-plus">
								<xsl:text> </xsl:text>
							</i>
							<xsl:text> </xsl:text>
							<xsl:value-of select="$text"/>
						</a>
					</div>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:if>
	</xsl:template>

	<!-- -->
	<xsl:template name="inlinePopupAddOptions">
		<xsl:param name="type"/>
		<xsl:param name="name"/>
		<xsl:choose>
			<xsl:when test="contains($type,',')">
				<xsl:variable name="contentType" select="substring-before($type,',')"/>
				<xsl:call-template name="inlinePopupAddOption">
					<xsl:with-param name="name" select="$name"/>
					<xsl:with-param name="type" select="$contentType"/>
				</xsl:call-template>

				<!-- This IF condition is to 'idiot proof' this mechanism, safe guarding against a developer leaving a trailing ',' at the end of the TYPE string -->
				<xsl:if test="substring-after($type,',')!=''">
					<xsl:call-template name="inlinePopupAddOptions">
						<xsl:with-param name="name" select="$name"/>
						<xsl:with-param name="type" select="substring-after($type,',')"/>
					</xsl:call-template>
				</xsl:if>

			</xsl:when>
			<xsl:otherwise>
				<xsl:call-template name="inlinePopupAddOption">
					<xsl:with-param name="name" select="$name"/>
					<xsl:with-param name="type" select="$type"/>
				</xsl:call-template>
			</xsl:otherwise>
		</xsl:choose>

	</xsl:template>


	<xsl:template name="inlinePopupAddOption">
		<xsl:param name="type"/>
		<xsl:param name="name"/>
		<li>
			<a href="?ewCmd=AddContent&amp;pgid={/Page/@id}&amp;type={$type}&amp;name={$name}" class="add adminButton">
				<xsl:choose>
					<xsl:when test="$type='PlainText'">
						<xsl:text>As Plain Text</xsl:text>
					</xsl:when>
					<xsl:when test="$type='FormattedText'">
						<xsl:text>As Formatted Text</xsl:text>
					</xsl:when>
					<xsl:when test="$type='Image'">
						<xsl:text>As Image</xsl:text>
					</xsl:when>
					<xsl:when test="$type='FlashMovie'">
						<xsl:text>As Flash Movie</xsl:text>
					</xsl:when>
					<xsl:when test="$type='AdSenseAdvert'">
						<xsl:text>As AdSense Advert</xsl:text>
					</xsl:when>
					<xsl:when test="$type='ImageLink'">
						<xsl:text>As Image Link</xsl:text>
					</xsl:when>

					<xsl:otherwise>
						<xsl:text>As </xsl:text>
						<xsl:value-of select="$type"/>
					</xsl:otherwise>
				</xsl:choose>
			</a>
		</li>
	</xsl:template>


	<!-- -->
	<xsl:template match="Content" mode="inlineOptionsNoPopup">
		<xsl:param name="parId"/>
		<xsl:choose>
			<xsl:when test="$parId!=''">
				<a href="{$appPath}?ewCmd=EditContent&amp;pgid={$parId}&amp;id={@id}" class="btn btn-xs btn-primary" title="Click here to edit this content">
					<i class="fa fa-edit fa-white">
						<xsl:text> </xsl:text>
					</i><xsl:text> </xsl:text>Edit
				</a>
			</xsl:when>
			<xsl:when test="/Page/@id=@parId">
				<a href="{$appPath}?ewCmd=EditContent&amp;pgid={/Page/@id}&amp;id={@id}" class="btn btn-xs btn-primary" title="Click here to edit this content">
					<i class="fa fa-edit fa-white">
						<xsl:text> </xsl:text>
					</i><xsl:text> </xsl:text>Edit
				</a>
			</xsl:when>
			<xsl:otherwise>
				<a href="{$appPath}?ewCmd=EditContent&amp;pgid={@parId}&amp;id={@id}" class="btn btn-xs btn-primary" title="Click here to edit this content">
					<i class="fa fa-edit fa-white">
						<xsl:text> </xsl:text>
					</i><xsl:text> </xsl:text>Edit
				</a>

			</xsl:otherwise>
		</xsl:choose>
		<xsl:if test="not($page/Contents/Content[@type='SearchHeader'])">
			<a href="{$appPath}?ewCmd=MoveTop&amp;pgid={/Page/@id}&amp;id={@id}" class="btn btn-xs btn-primary" title="Move this item to the top">
				<i class="fa fa-arrow-up fa-white">
					<xsl:text> </xsl:text>
				</i>
			</a>
			<a href="{$appPath}?ewCmd=MoveUp&amp;pgid={/Page/@id}&amp;id={@id}" class="btn btn-xs btn-primary" title="Move this item up by one space">
				<i class="fa fa-chevron-up fa-white">
					<xsl:text> </xsl:text>
				</i>
			</a>
			<a href="{$appPath}?ewCmd=MoveDown&amp;pgid={/Page/@id}&amp;id={@id}" class="btn btn-xs btn-primary" title="Move this item down by one space">
				<i class="fa fa-chevron-down fa-white">
					<xsl:text> </xsl:text>
				</i>
			</a>
			<a href="{$appPath}?ewCmd=MoveBottom&amp;pgid={/Page/@id}&amp;id={@id}" class="btn btn-xs btn-primary" title="Move this item to the bottom">
				<i class="fa fa-arrow-down fa-white">
					<xsl:text> </xsl:text>
				</i>
			</a>
		</xsl:if>
		<div class="btn-group">
			<button type="button" class="btn btn-primary btn-xs dropdown-toggle" data-toggle="dropdown">
				<i class="fa fa-white fa-sort-down">
					<xsl:text> </xsl:text>
				</i> Options
			</button>
			<ul class="dropdown-menu" role="menu">
				<li>
					<a href="{$appPath}?ewCmd=MoveContent&amp;pgid={/Page/@id}&amp;id={@id}" title="Click here to move this content">
						<i class="fa fa-mail-forward">&#160;</i>&#160;Move
					</a>
				</li>
				<li>
					<a href="{$appPath}?ewCmd=CopyContent&amp;pgid={/Page/@id}&amp;id={@id}" title="Click here to copy this content">
						<i class="fa fa-copy">&#160;</i>&#160;Copy
					</a>
				</li>
				<li>
					<a href="{$appPath}?ewCmd=LocateContent&amp;pgid={/Page/@id}&amp;id={@id}" title="Click here to set which pages to show this content on">
						<i class="fa fa-mail-reply-all fa-flip-horizontal">&#160;</i>&#160;Locations
					</a>
				</li>
				<xsl:if test="@status='1'">
					<li>
						<a href="{$appPath}?ewCmd=HideContent&amp;pgid={/Page/@id}&amp;id={@id}" title="Click here to hide this item">
							<i class="fa fa-times-circle">&#160;</i>&#160;Hide
						</a>
					</li>
				</xsl:if>
				<xsl:if test="@status='0'">
					<li>
						<a href="{$appPath}?ewCmd=ShowContent&amp;pgid={/Page/@id}&amp;id={@id}" title="Click here to show this item">
							<i class="fa fa-check-square-o">&#160;</i>&#160;Show
						</a>
					</li>
				</xsl:if>
				<xsl:if test="@status='0'">
					<li>
						<a href="{$appPath}?ewCmd=DeleteContent&amp;pgid={/Page/@id}&amp;id={@id}" title="Click here to delete this item">
							<i class="fa fa-trash-o">&#160;</i>&#160;Delete
						</a>
					</li>
				</xsl:if>
			</ul>
		</div>

	</xsl:template>
	<!-- -->
	<xsl:template match="Page[@layout='ByType']" mode="Admin">
		<xsl:variable name="contentType" select="@ewCmd2"/>
		<div id="tpltAdvancedMode">
			<div class="row header-panels">
				<div class="col-md-5">
					<form action="{$appPath}?ewCmd=ByType.{@ewCmd2}.Search&amp;pgid={$page/@id}" method="post" class="ewXform panel panel-default">
						<div class="panel-body form-group">
							<div class="input-group">
								<label class="input-group-addon">
									Search All <xsl:value-of select="@ewCmd2"/>s
								</label>
								<input class="form-control" name="searchString" id="searchString" value="{Contents/Content[@type='SearchHeader']/@SearchString}"/>
								<span class="input-group-btn">
									<button type="submit" class="btn btn-primary">Go</button>
								</span>
							</div>
						</div>
					</form>
				</div>
				<div class="col-md-5">
					<form method="post" action="/?ewCmd=ByType.{@ewCmd2}.Location" class="ewXform panel panel-default" id="LocationFilter" name="LocationFilter">
						<div class="panel-body  form-group">
							<div class="input-group">
								<label for="Location" class="input-group-addon">Select Location</label>
								<div class="control-wrapper select1-wrapper appearance-minimal ">
									<select name="Location" id="Location" class="form-control dropdown submit-on-select">
										<xsl:apply-templates select="ContentDetail/Content[@type='xform']/select1[@ref='Location']/item" mode="xform_select">
											<xsl:with-param name="selectedValue" select="@id"/>
										</xsl:apply-templates>
									</select>
								</div>
							</div>
						</div>
					</form>

					<!--xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/-->
				</div>
			</div>
			<xsl:apply-templates select="/" mode="ListByContentTypeByPage">
				<xsl:with-param name="contentType" select="@ewCmd2"/>
			</xsl:apply-templates>
		</div>
	</xsl:template>

	<!-- -->
	<xsl:template match="/" mode="ListByContentTypeNoColaspe">
		<xsl:param name="contentType"/>
		<xsl:variable name="pgid">
			<xsl:choose>
				<xsl:when test="$page/ContentDetail/Content/select1[@ref='Location']/value/node()!=''">
					<xsl:value-of select="$page/ContentDetail/Content/select1[@ref='Location']/value/node()"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$page/@pgid"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<form action="{$appPath}" method="get" class="ewXform" id="BulkContentAction">
			<input type="hidden" name="ewCmd" value="BulkContentAction"/>
			<input type="hidden" name="pgid" value="{$pgid}"/>
			<div class="panel panel-default">
				<div class="panel-heading">
					<xsl:if test="$contentType!='Module'">
						<xsl:if test="not($page/Contents/Content[@type='SearchHeader'])">
							<xsl:variable name="href">
								<xsl:text>?ewCmd=AddContent</xsl:text>
								<xsl:text>&amp;pgid=</xsl:text>
								<xsl:value-of select="/Page/@id"/>
								<xsl:text>&amp;type=</xsl:text>
								<xsl:value-of select="$contentType"/>
							</xsl:variable>
							<a class="btn btn-primary btn-xs principle" href="{$href}">
								<i class="fa fa-plus">
									<xsl:text> </xsl:text>
								</i>
								<xsl:text> </xsl:text>Add
							</a>
						</xsl:if>
					</xsl:if>
					<h6 class="panel-title">

						<i class="fa fa-chevron-down">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>
						<xsl:value-of select="$contentType"/> (<xsl:value-of select="count(Page/Contents/Content[@type=$contentType])"/>)

					</h6>

				</div>

				<form action="{$appPath}" method="get" class="ewXform">
					<table class="table table-striped-2">
						<xsl:if test="not(Page/Contents/Content[@type=$contentType])">
							<tr>
								<td colspan="3">
									<xsl:text>No </xsl:text>
									<xsl:value-of select="$contentType"/>
									<xsl:text> on this page</xsl:text>
								</td>
							</tr>
						</xsl:if>
						<xsl:apply-templates select="Page/Contents/Content[@type=$contentType][1]" mode="AdvancedModeHeader"/>
						<xsl:apply-templates select="Page/Contents/Content[@type=$contentType]" mode="AdvancedMode"/>
					</table>
				</form>
			</div>
		</form>
	</xsl:template>

	<xsl:template match="/" mode="ListByContentTypeByPage-SortBy">
		<span class="input-group list-control-select">
			<xsl:variable name="sortBy" select="$page/Request/*/Item[@name='sortby']/node()"/>
			<label class="input-group-addon">Sort By</label>
			<select class="form-control submit-on-select" name="sortby" id="sortby" onchange="this.form.submit()">
				<option value="default">
					<xsl:if test="not($sortBy!='')">
						<xsl:attribute name="selected">selected</xsl:attribute>
					</xsl:if>
					Page Position
				</option>
				<option value="name">
					<xsl:if test="$sortBy='name'">
						<xsl:attribute name="selected">selected</xsl:attribute>
					</xsl:if>Name A-Z
				</option>
			</select>
		</span>
	</xsl:template>

	<xsl:template match="/" mode="ListByContentTypeByPage">
		<xsl:param name="contentType"/>
		<xsl:variable name="startPos" select="number(concat(0,/Page/Request/QueryString/Item[@name='startPos']))"/>
		<xsl:variable name="itemCount" select="'100'"/>
		<xsl:variable name="total" select="$page/ContentDetail/@total"/>
		<xsl:variable name="sortBy" select="$page/Request/*/Item[@name='sortby']/node()"/>
		<xsl:variable name="queryString">
			<xsl:text>?</xsl:text>
			<xsl:call-template name="getQString"/>
		</xsl:variable>
		<xsl:variable name="title">
			<xsl:text>Location</xsl:text>
		</xsl:variable>
		<xsl:variable name="pgid">
			<xsl:choose>
				<xsl:when test="$page/ContentDetail/Content/select1[@ref='Location']/value/node()!=''">
					<xsl:value-of select="$page/ContentDetail/Content/select1[@ref='Location']/value/node()"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$page/@pgid"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>



		<div class="panel panel-default">
			<div class="panel-heading row">
				<h6 class="panel-title col-md-3">
					<xsl:value-of select="$contentType"/> (<xsl:value-of select="count(Page/Contents/Content[@type=$contentType])"/>)
				</h6>
				<xsl:if test="$page/ContentDetail/@total > 0">
					<div class="list-controls col-md-9">
						<xsl:if test="not($page/Contents/Content[@type='SearchHeader'])">

							<xsl:variable name="href">
								<xsl:text>?ewCmd=AddContent</xsl:text>
								<xsl:text>&amp;pgid=</xsl:text>
								<xsl:value-of select="/Page/@id"/>
								<xsl:text>&amp;type=</xsl:text>
								<xsl:value-of select="$contentType"/>
							</xsl:variable>
							<div class="stepper-container">
								<a class="btn btn-primary" href="{$href}">
									<i class="fa fa-plus">
										<xsl:text> </xsl:text>
									</i>
									<xsl:text> </xsl:text>Add <xsl:value-of select="$contentType"/>
								</a>
							</div>
						</xsl:if>


						<form method="post" action="?ewCmd={$page/@ewCmd}.{$page/@ewCmd2}.{$page/@ewCmd3}&amp;Location={$page/Request/*/Item[@name='Location']/node()}" id="listReload">
							<div class="list-header-select">
								<span class="input-group">
									<label class="input-group-addon">Items Per Page</label>
									<select class="form-control" name="PageCount" id="PageCount">
										<option value="50">100</option>
										<option value="100">100</option>
										<option value="250">250</option>
										<option value="500">500</option>
										<option value="All">All</option>
									</select>
								</span>
							</div>

							<div class="list-header-select">
								<xsl:apply-templates select="/" mode="ListByContentTypeByPage-SortBy"/>
							</div>
						</form>
						<div class="stepper-container">
							<xsl:apply-templates select="/" mode="adminStepper">
								<xsl:with-param name="itemCount" select="$page/ContentDetail/@rows"/>
								<xsl:with-param name="itemTotal" select="$total"/>
								<xsl:with-param name="startPos" select="$startPos"/>
								<xsl:with-param name="path" select="$queryString"/>
								<xsl:with-param name="itemName" select="$title"/>
							</xsl:apply-templates>
						</div>
					</div>
				</xsl:if>
			</div>
		</div>
		<form action="{$appPath}" method="get" class="ewXform" id="BulkContentAction">
			<input type="hidden" name="ewCmd" value="BulkContentAction"/>
			<input type="hidden" name="pgid" value="{$pgid}"/>
			<table class="table table-striped-2">
				<xsl:if test="not(Page/Contents/Content[@type=$contentType])">
					<tr>
						<td colspan="3">
							<xsl:text>No </xsl:text>
							<xsl:value-of select="$contentType"/>
							<xsl:text> on this page</xsl:text>
						</td>
					</tr>
				</xsl:if>
				<xsl:apply-templates select="Page/Contents/Content[@type=$contentType][1]" mode="AdvancedModeHeader"/>
				<xsl:choose>
					<xsl:when test="$sortBy='name'">
						<xsl:apply-templates select="Page/Contents/Content[@type=$contentType]" mode="AdvancedMode">
							<xsl:sort select="@name" data-type="text"/>
						</xsl:apply-templates>
					</xsl:when>
					<xsl:when test="$sortBy='unit'">
						<xsl:apply-templates select="Page/Contents/Content[@type=$contentType]" mode="AdvancedMode">
							<xsl:sort select="UnitNumber" data-type="number"/>
						</xsl:apply-templates>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="Page/Contents/Content[@type=$contentType]" mode="AdvancedMode"/>
					</xsl:otherwise>
				</xsl:choose>
			</table>
		</form>
		<div class="panel-header">

			<xsl:if test="$page/ContentDetail/@total > 0">
				<div class="pull-right-stepper">
					<xsl:apply-templates select="/" mode="adminStepper">
						<xsl:with-param name="itemCount" select="$page/ContentDetail/@rows"/>
						<xsl:with-param name="itemTotal" select="$total"/>
						<xsl:with-param name="startPos" select="$startPos"/>
						<xsl:with-param name="path" select="$queryString"/>
						<xsl:with-param name="itemName" select="$title"/>
					</xsl:apply-templates>
				</div>
			</xsl:if>
		</div>
	</xsl:template>

	<xsl:template match="Page[@editContext='ByType.FAQ.UserUnRead']" mode="Admin">
		<div id="tpltAdvancedMode">
			<xsl:apply-templates select="/" mode="ListByFAQViewed">
				<xsl:with-param name="contentType" select="@ewCmd2"/>
			</xsl:apply-templates>
		</div>
	</xsl:template>

	<xsl:template match="Page[@editContext='ByType.FAQ.UserRead']" mode="Admin">
		<div id="tpltAdvancedMode">
			<xsl:apply-templates select="/" mode="ListByFAQViewed">
				<xsl:with-param name="contentType" select="@ewCmd2"/>
			</xsl:apply-templates>
		</div>
	</xsl:template>


	<!-- -->
	<xsl:template match="/" mode="ListByFAQViewed">
		<xsl:param name="contentType"/>
		<xsl:variable name="pgid">
			<xsl:choose>
				<xsl:when test="$page/ContentDetail/Content/select1[@ref='Location']/value/node()!=''">
					<xsl:value-of select="$page/ContentDetail/Content/select1[@ref='Location']/value/node()"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$page/@pgid"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<div class="panel panel-default">
			<div class="panel-heading">

				<h6 class="panel-title">
					<i class="fa fa-chevron-down">
						<xsl:text> </xsl:text>
					</i><xsl:text> </xsl:text>
					<xsl:value-of select="$contentType"/> (<xsl:value-of select="count(Page/Contents/Content[@type=$contentType])"/>)
				</h6>
			</div>
			<table class="table table-striped-2">
				<xsl:if test="not(Page/Contents/Content[@type=$contentType])">
					<tr>
						<td colspan="3">
							<xsl:text>No </xsl:text>
							<xsl:value-of select="$contentType"/>
							<xsl:text> on this page</xsl:text>
						</td>
					</tr>
				</xsl:if>
				<xsl:apply-templates select="Page/Contents/Content[@type=$contentType][1]" mode="AdvancedModeHeader"/>
				<xsl:apply-templates select="Page/Contents/Content[@type=$contentType]" mode="AdvancedMode"/>
			</table>
		</div>
	</xsl:template>

	<xsl:template match="Page[@layout='EditStructure']" mode="Admin">
		<div id="tpltEditStructure">
			<ul id="MenuTree" class="list-group">
				<xsl:apply-templates select="Menu/MenuItem" mode="editStructure">
					<xsl:with-param name="level">1</xsl:with-param>
				</xsl:apply-templates>
				<xsl:apply-templates select="Menu/MenuItem/MenuItem[DisplayName/@siteTemplate='micro']" mode="editStructure">
					<xsl:with-param name="level">1</xsl:with-param>
				</xsl:apply-templates>
			</ul>
		</div>
	</xsl:template>

	<xsl:template name="quickjump">

	</xsl:template>
	<!-- -->
	<!-- -->
	<!-- -->

	<xsl:template match="MenuItem" mode="editStructure">
		<xsl:param name="level"/>

		<xsl:variable name="thislevel">
			<xsl:value-of select="$level"/>
		</xsl:variable>

		<xsl:variable name="siteRoot">
			<xsl:call-template name="getSettings">
				<xsl:with-param name="sectionName" select="'web'"/>
				<xsl:with-param name="valueName" select="'RootPageId'"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:variable name="getMenuLevelDepth">
			<xsl:call-template name="getSettings">
				<xsl:with-param name="sectionName" select="'web'"/>
				<xsl:with-param name="valueName" select="'MenuTreeDepth'"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:variable name="menuLevelDepth">
			<xsl:choose>
				<xsl:when test="$getMenuLevelDepth = ''">
					<xsl:value-of select="'0'"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$getMenuLevelDepth"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:if test="not($level=2 and DisplayName/@siteTemplate='micro')">
			<li id="node{@id}" data-tree-level="{$thislevel}">
				<xsl:attribute name="data-tree-parent">
					<xsl:if test="not(DisplayName/@siteTemplate='micro' and $thislevel='1')">
						<xsl:value-of select="./parent::MenuItem/@id"/>
					</xsl:if>
				</xsl:attribute>
				<xsl:attribute name="class">
					<xsl:if test="@cloneparent &gt; 0 and not(@cloneparent=@id)">
						<xsl:text>clone context</xsl:text>
						<xsl:value-of select="@cloneparent"/>
					</xsl:if>
					<xsl:text> list-group-item level-</xsl:text>
					<xsl:value-of select="$thislevel"/>
					<xsl:if test="MenuItem"> expandable</xsl:if>
				</xsl:attribute>

				<div class="pageCell">
					<xsl:variable name="pageLink">
						<xsl:apply-templates select="." mode="getHref" />
						<xsl:text>&amp;ewCmd=Normal</xsl:text>
						<xsl:if test="@cloneparent &gt; 0">
							<xsl:text>&amp;context=</xsl:text>
							<xsl:value-of select="@cloneparent"/>
						</xsl:if>
					</xsl:variable>
					<xsl:variable name="displayName">
						<xsl:apply-templates select="." mode="getDisplayName" />
					</xsl:variable>
					<a href="{$pageLink}" title="{@name}" name="{@id}">
						<xsl:choose>
							<xsl:when test="DisplayName/@siteTemplate='micro' or parent::Menu">
								<i class="fa fa-home fa-lg status activeParent" xmlns="http://www.w3.org/1999/xhtml">
									&#160;
								</i>
								<span class="pageName">
									&#160;
									<xsl:value-of select="@name"/>
								</span>
							</xsl:when>
							<xsl:otherwise>
								<xsl:apply-templates select="." mode="status_legend"/>
								<span class="pageName">
									<xsl:value-of select="$displayName"/>
								</span>
							</xsl:otherwise>
						</xsl:choose>



					</a>
				</div>
				<div class="optionButtons">

					<!-- Clone page note: don't offer menu page options to items that are cloned page child pages-->
					<xsl:choose>

						<xsl:when test="@cloneparent and not(@cloneparent=@id)">
							<a href="{$appPath}?ewCmd=EditPage&amp;pgid={@id}&amp;returnCmd=EditStructure" class="btn btn-xs btn-primary" title="Click here to edit this page">
								<i class="fa fa-edit fa-white">
									<xsl:text> </xsl:text>
								</i><xsl:text> </xsl:text>
								Clone - Edit Master
							</a>
						</xsl:when>
						<xsl:otherwise>

							<!--div class="options"-->
							<!-- href="{$appPath}?ewCmd=MoveUp&amp;pgid={@id}" replaced now with onClick-->
							<!--span class="hidden"> | </span-->
							<a href="{$appPath}?ewCmd=EditPage&amp;pgid={@id}&amp;returnCmd=EditStructure" class="btn btn-xs btn-primary" title="Click here to edit this page">
								<i class="fa fa-cogs fa-white">
									<xsl:text> </xsl:text>
								</i><xsl:text> </xsl:text>
								Page Settings
							</a>
							<!--span class="hidden"> | </span-->
							<a href="{$appPath}?ewCmd=AddPage&amp;parId={@id}&amp;returnCmd=EditStructure" class="btn btn-xs btn-success" title="Click here to add a new child or sub page beneath this page">
								<i class="fa fa-plus fa-white">
									<xsl:text> </xsl:text>
								</i><xsl:text> </xsl:text>Add Page
							</a>
							<!--span class="hidden"> | </span-->
							<xsl:if test="@id!=$siteRoot">
								<a onclick="$('#MenuTree').moveTop({@id});" class="btn btn-arrow btn-default btn-xs move-top" title="Click here to move this page to the top">
									<i class="fa fa-arrow-up fa-white">
										<xsl:text> </xsl:text>
									</i>
								</a>
								<!--span class="hidden"> | </span-->
								<!--<input type="button" class="arrowbutton up" onclick="moveUp({@id});" title="Click here to move this page up by one space"/>-->
								<a onclick="$('#MenuTree').moveUp({@id});" class="btn btn-arrow btn-default btn-xs move-up" title="Click here to move this page up by one space">
									<i class="fa fa-chevron-up fa-white">
										<xsl:text> </xsl:text>
									</i>
								</a>
								<!--span class="hidden"> | </span-->
								<a onclick="$('#MenuTree').moveDown({@id});" class="btn btn-arrow btn-default btn-xs move-down" title="Click here to move this page down by one space">
									<i class="fa fa-chevron-down fa-white">
										<xsl:text> </xsl:text>
									</i>
								</a>
								<!--span class="hidden"> | </span-->
								<a onclick="$('#MenuTree').moveBottom({@id});" class="btn btn-arrow btn-default btn-xs move-bottom" title="Click here to move this page to the bottom">
									<i class="fa fa-arrow-down fa-white">
										<xsl:text> </xsl:text>
									</i>
								</a>

								<!--span class="hidden"> | </span-->
								<a href="{$appPath}?ewCmd=MovePage&amp;pgid={@id}" class="btn btn-xs btn-default" title="Click here to move this page">
									<i class="fa fa-share fa-white">
										<xsl:text> </xsl:text>
									</i><xsl:text> </xsl:text>
									Move
								</a>

								<!--span class="hidden"> | </span-->
								<a href="{$appPath}?ewCmd=CopyPage&amp;pgid={@id}" class="btn btn-xs btn-default" title="Click here to copy this page">
									<i class="fa fa-copy fa-white">
										<xsl:text> </xsl:text>
									</i><xsl:text> </xsl:text>Copy
								</a>
								<!--span class="hidden"> | </span-->
								<xsl:if test="@status='1'">
									<!--a href="{$appPath}?ewCmd=HidePage&amp;pgid={@id}" class="adminButton hide" title="Click here to hide this page">Hide</a-->
									<a onclick="$('#MenuTree').hideButton({@id});" class="btn btn-xs btn-danger btn-hide" title="Click here to hide this page">
										<i class="fa fa-times-circle fa-white">
											<xsl:text> </xsl:text>
										</i><xsl:text> </xsl:text>
										Hide
									</a>
									<!--span class="hidden"> | </span-->
								</xsl:if>
								<xsl:if test="@status='0'">
									<!--a href="{$appPath}?ewCmd=ShowPage&amp;pgid={@id}" class="adminButton show" title="Click here to hide this page">Show</a-->
									<a onclick="$('#MenuTree').showButton({@id});" class="btn btn-xs btn-success btn-show" title="Click here to show this page">
										<i class="fa fa-check-circle fa-white">
											<xsl:text> </xsl:text>
										</i><xsl:text> </xsl:text>Show
									</a>
									<!--span class="hidden"> | </span-->
									<a href="{$appPath}?ewCmd=DeletePage&amp;pgid={@id}" class="btn btn-xs btn-danger btn-del" title="Click here to delete this page">
										<i class="fa fa-trash-o fa-white">
											<xsl:text> </xsl:text>
										</i><xsl:text> </xsl:text>Delete
									</a>
									<!--span class="hidden"> | </span-->
								</xsl:if>
							</xsl:if>
						</xsl:otherwise>

					</xsl:choose>

				</div>
			</li>





			<!--Expand to Level XSL variant-->
			<xsl:if test="$menuLevelDepth &gt; 0">
				<xsl:if test="/Page/Request/QueryString/Item[@name='ewCmd']='EditPage' or 'HidePage' or 'ShowPage' or 'CopyPage'">
					<xsl:if test="descendant-or-self::MenuItem">
						<xsl:if test="$level &lt; $menuLevelDepth">
							<xsl:if test="MenuItem">

								<xsl:apply-templates select="MenuItem" mode="editStructure">
									<xsl:with-param name="level">
										<xsl:value-of select="$level + 1"/>
									</xsl:with-param>
								</xsl:apply-templates>

							</xsl:if>
						</xsl:if>
					</xsl:if>
				</xsl:if>
			</xsl:if>

			<xsl:if test="$menuLevelDepth = 0">
				<xsl:choose>
					<xsl:when test="/Page/Request/QueryString/Item[@name='ewCmd']='AddPage'">
						<xsl:if test="descendant-or-self::MenuItem[@id=/Page/Request/QueryString/Item[@name='parId']]/@id">
							<xsl:if test="MenuItem">

								<xsl:apply-templates select="MenuItem" mode="editStructure">
									<xsl:with-param name="level">
										<xsl:value-of select="$thislevel + 1"/>
									</xsl:with-param>
								</xsl:apply-templates>

							</xsl:if>
						</xsl:if>
					</xsl:when>

					<xsl:when test="/Page/Request/QueryString/Item[@name='ewCmd']='EditPage' or 'HidePage' or 'ShowPage' or 'CopyPage'">
						<xsl:if test="descendant-or-self::MenuItem[@id=/Page/Request/QueryString/Item[@name='pgid']]/@id">
							<xsl:if test="MenuItem"></xsl:if>

							<xsl:apply-templates select="MenuItem" mode="editStructure">
								<xsl:with-param name="level">
									<xsl:value-of select="$thislevel + 1"/>
								</xsl:with-param>
							</xsl:apply-templates>

						</xsl:if>
					</xsl:when>
					<xsl:otherwise>
						<xsl:if test="descendant-or-self::MenuItem[@id=/Page/@id]/@id">
							<xsl:if test="MenuItem">
								<xsl:apply-templates select="MenuItem" mode="editStructure">
									<xsl:with-param name="level">
										<xsl:value-of select="$thislevel + 1"/>
									</xsl:with-param>
								</xsl:apply-templates>
							</xsl:if>
						</xsl:if>
					</xsl:otherwise>
				</xsl:choose>


			</xsl:if>
		</xsl:if>
	</xsl:template>


	<!-- -->
	<!--   ##################  Move Here - dynamically generated from menu   ##############################   -->
	<!-- -->

	<xsl:template match="Page[@layout='MovePage']" mode="Admin">
		<!-- -->
		<div class="row" id="tpltMovePage">
			<div class="col-md-3">
				<div class="panel panel-default">
					<div class="panel-heading">
						<h3 class="panel-title">Instructions</h3>
					</div>
					<div class="panel-body">
						<p>To move the page identified as "moving..." click the move here link on a another page.</p>
						<p>The moved page will be added beneath the page you have selected...</p>
					</div>
				</div>
			</div>
			<div class="col-md-9">
				<div class="panel panel-default">
					<ul id="MenuTree" class="list-group">
						<xsl:apply-templates select="Menu/MenuItem" mode="movePage">
							<xsl:with-param name="level">1</xsl:with-param>
						</xsl:apply-templates>
						<xsl:apply-templates select="Menu/MenuItem/MenuItem[DisplayName/@siteTemplate='micro']" mode="movePage">
							<xsl:with-param name="level">1</xsl:with-param>
						</xsl:apply-templates>
					</ul>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="MenuItem" mode="movePage">
		<xsl:param name="level"/>
		<xsl:if test="not($level=2 and DisplayName/@siteTemplate='micro')">
			<li id="node{@id}" data-tree-level="{$level}">
				<xsl:attribute name="data-tree-parent">
					<xsl:if test="not(DisplayName/@siteTemplate='micro' or @level='1')">
						<xsl:value-of select="./parent::MenuItem/@id"/>
					</xsl:if>
				</xsl:attribute>
				<!--<xsl:apply-templates select="." mode="status_legend"/>-->
				<!--<xsl:if test="(position()+1) mod 2=0">
				<xsl:attribute name="class">alternate</xsl:attribute>
			</xsl:if>-->
				<xsl:attribute name="class">
					<xsl:if test="@cloneparent &gt; 0 and not(@cloneparent=@id)">
						<xsl:text>clone context</xsl:text>
						<xsl:value-of select="@cloneparent"/>
					</xsl:if>
					<xsl:text> list-group-item level-</xsl:text>
					<xsl:value-of select="$level"/>
				</xsl:attribute>
				<div class="pageCell">
					<xsl:choose>
						<xsl:when test="DisplayName/@siteTemplate='micro' or parent::Menu">
							<i class="fa fa-home fa-lg status activeParent">
								&#160;
							</i>
							<span class="pageName">
								&#160;
								<xsl:value-of select="@name"/>
							</span>
						</xsl:when>
						<xsl:otherwise>
							<xsl:apply-templates select="." mode="status_legend"/>
							<span class="pageName">
								<xsl:apply-templates select="." mode="getDisplayName" />
							</span>
						</xsl:otherwise>
					</xsl:choose>

				</div>
				<div class="optionButtons">
					<xsl:choose>
						<!-- Picks up and test when Ajax request for pages -->
						<xsl:when test="//Request/*/Item[@name='movingPageId']">
							<xsl:variable name="movingPageId" select="//Request/*/Item[@name='movingPageId']"/>
							<xsl:choose>
								<!-- this when will never be hit - until componant fixed - need the moving MenuItem in Schema to check -->
								<xsl:when test="ancestor-or-self::MenuItem/@id=$movingPageId">
									<xsl:text>Moving...</xsl:text>
								</xsl:when>
								<xsl:otherwise>

									<a href="{$appPath}?ewCmd=MoveHere&amp;pgid={$movingPageId}&amp;parId={@id}" title="Move this item under here" class="btn btn-primary">
										<i class="fa fa-hand-o-down fa-white">
											<xsl:text> </xsl:text>
										</i><xsl:text> </xsl:text>
										Move Here
									</a>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>

						<!-- normal page load -->
						<xsl:when test="ancestor-or-self::MenuItem/@id=/Page/@id and not(//Request/*/Item[@name='ajaxCmd'])">
							<xsl:text>Moving...</xsl:text>
						</xsl:when>
						<xsl:when test="@clone &gt; 0 or @cloneparent &gt; 0">
							<xsl:text>Cloned...</xsl:text>
						</xsl:when>
						<xsl:otherwise>
							<a href="{$appPath}?ewCmd=MoveHere&amp;pgid={/Page/@id}&amp;parId={@id}" title="Move this item under here" class="btn btn-xs btn-primary">
								<i class="fa fa-hand-o-down fa-white">
									<xsl:text> </xsl:text>
								</i><xsl:text> </xsl:text>Move Here
							</a>
						</xsl:otherwise>
					</xsl:choose>
				</div>
			</li>
			<xsl:if test="descendant-or-self::MenuItem[@id=/Page/@id]/@id or /Page/Request/QueryString/Item[@name='contentType']/node()='ajaxadmin'">
				<xsl:apply-templates select="MenuItem" mode="movePage">
					<xsl:with-param name="level">
						<xsl:value-of select="$level + 1"/>
					</xsl:with-param>
				</xsl:apply-templates>

			</xsl:if>
		</xsl:if>
	</xsl:template>

	<!-- -->
	<!--   ##################  Move Content - dynamically generated from menu   ##############################   -->
	<!-- -->
	<xsl:template match="Page[@layout='MoveContent']" mode="Admin">
		<div class="row" id="tpltMoveContent">
			<div class="col-md-3">
				<div class="panel panel-default">
					<div class="panel-heading">
						<h3 class="panel-title">Instructions</h3>
					</div>
					<div class="panel-body">
						<p>Click the "Move Here" button on the page you want to move your content too.</p>
					</div>
				</div>
			</div>
			<div class="col-md-9">
				<div class="panel panel-default">
					<ul id="MenuTree" class="list-group">
						<xsl:apply-templates select="Menu/MenuItem" mode="moveContent">
							<xsl:with-param name="level">1</xsl:with-param>
						</xsl:apply-templates>
						<xsl:apply-templates select="Menu/MenuItem/MenuItem[DisplayName/@siteTemplate='micro']" mode="moveContent">
							<xsl:with-param name="level">1</xsl:with-param>
						</xsl:apply-templates>
					</ul>
				</div>
			</div>
		</div>
	</xsl:template>


	<xsl:template match="MenuItem" mode="moveContent">
		<xsl:param name="level"/>
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
			<li id="node{@id}" class="list-group-item level-{$level} {$class}" data-tree-level="{$level}">
				<xsl:attribute name="data-tree-parent">
					<xsl:if test="not(DisplayName/@siteTemplate='micro' or @level='1')">
						<xsl:value-of select="./parent::MenuItem/@id"/>
					</xsl:if>
				</xsl:attribute>
				<div class="pageCell">
					<xsl:choose>
						<xsl:when test="DisplayName/@siteTemplate='micro' or parent::Menu">
							<i class="fa fa-home fa-lg status activeParent">
								&#160;
							</i>
							<span class="pageName">
								&#160;
								<xsl:value-of select="@name"/>
							</span>
						</xsl:when>
						<xsl:otherwise>
							<xsl:apply-templates select="." mode="status_legend"/>
							<span class="pageName">
								<xsl:value-of select="@name"/>
							</span>
						</xsl:otherwise>
					</xsl:choose>
				</div>
				<div class="optionButtons">
					<xsl:choose>
						<xsl:when test="ancestor-or-self::MenuItem[@clone &gt; 0]">
							<xsl:text>Cloned...</xsl:text>
						</xsl:when>
						<xsl:when test="@id!=/Page/@id">
							<a href="{$appPath}?ewCmd=MoveContent&amp;pgid={$oldpgid}&amp;parId={@id}&amp;id={/Page/Request/*/Item[@name='id']/node()}" title="Move this item under here" class="btn btn-xs btn-primary">
								<i class="fa fa-share fa-rotate-90 fa-white">
									<xsl:text> </xsl:text>
								</i><xsl:text> </xsl:text>Move Here
							</a>
						</xsl:when>
					</xsl:choose>
				</div>
			</li>

			<xsl:if test="PageVersion">
				<xsl:variable name="data-tree-parent">
					<xsl:value-of select="./parent::MenuItem/@id"/>
				</xsl:variable>
				<xsl:variable name="data-tree-level" select="number($level)+1"/>
				<xsl:for-each select="PageVersion">
					<li id="node{@id}" class="list-group-item level-{$data-tree-level} page-version" data-tree-level="{$data-tree-level}" data-tree-parent="{$data-tree-parent}">
						<div class="pageCell">
							<xsl:apply-templates select="." mode="status_legend"/>
							<span class="pageName">
								Version - <xsl:value-of select="@name"/> - <xsl:value-of select="@desc"/>
								<xsl:value-of select="parent::*/@id"/>
							</span>
						</div>
						<div class="optionButtons">
							<xsl:if test="@id!=/Page/@id">
								<a href="{$appPath}?ewCmd=MoveContent&amp;pgid={$oldpgid}&amp;parId={@id}&amp;id={/Page/Request/QueryString/Item[@name='id']/node()}" title="Move this item under here" class="btn btn-xs btn-primary">
									<i class="fa fa-share fa-rotate-90 fa-white">
										<xsl:text> </xsl:text>
									</i><xsl:text> </xsl:text>Move Here
								</a>
							</xsl:if>
						</div>
					</li>
				</xsl:for-each>
			</xsl:if>
			<xsl:if test="descendant-or-self::MenuItem[@id=/Page/@id]/@id">
				<xsl:apply-templates select="MenuItem" mode="moveContent">
					<xsl:with-param name="level">
						<xsl:value-of select="$level + 1"/>
					</xsl:with-param>
				</xsl:apply-templates>
			</xsl:if>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<!--   ##################  Locate Content - dynamically generated from menu   ##############################   -->

	<xsl:template match="Page[@layout='LocateContent']" mode="Admin">
		<div class="row" id="tpltLocateContent">
			<div class="col-md-3">
				<div class="panel panel-default">
					<div class="panel-heading">Instructions</div>
					<div class="panel-body">

						<p>Any content can be located on any page throughout the site. However it will not be displayed unless the page contains a module that displays that kind of content.</p>
						<p>
							There are two types of relationship, a primary location, that is the parent page of each piece of content every piece of content should have one of these. You can change this by clicking the <a class="adminButton edit">edit relationship</a> button
						</p>
					</div>
				</div>
			</div>
			<form action="?ewCmd=LocateContent&amp;pgid={/Page/@id}&amp;id={/Page/Request/*/Item[@name='id']}" method="post" class="ewXform">
				<input type="hidden" name="id" value="{/Page/Request/*/Item[@name='id']}"/>
				<xsl:variable name="position">
					<xsl:choose>
						<xsl:when test="/Page/ContentDetail/Content/Location[@primary='true']/@position!=''">
							<xsl:value-of select="/Page/ContentDetail/Content/Location[@primary='true']/@position"/>
						</xsl:when>
						<xsl:otherwise>column1</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>
				<input type="hidden" name="position" value="{$position}"/>
				<div class="col-md-9">
					<div class="panel panel-default">
						<div class="panel-heading">
							<button type="submit" name="submit" class="pull-right btn btn-success btn-sm" value="submit">
								<i class="fa fa-check">
									<xsl:text> </xsl:text>
								</i> Save Locations
							</button>
							<div class="clearfix">&#160; </div>
						</div>
						<ul id="MenuTree" class="list-group">
							<xsl:apply-templates select="Menu/MenuItem" mode="LocateContent">
								<xsl:with-param name="level">1</xsl:with-param>
							</xsl:apply-templates>
							<xsl:apply-templates select="Menu/MenuItem/MenuItem[DisplayName/@siteTemplate='micro']" mode="LocateContent">
								<xsl:with-param name="level">1</xsl:with-param>
							</xsl:apply-templates>
						</ul>
						<div class="panel-footer">
							<button type="submit" name="submit" class="pull-right btn btn-success btn-sm" value="submit">
								<i class="fa fa-check">
									<xsl:text> </xsl:text>
								</i> Save Locations
							</button>
							<div class="clearfix">&#160; </div>
						</div>
					</div>
				</div>
			</form>


		</div>
	</xsl:template>

	<!-- ###### NEW AJAX TREE VIEW - COMMENTED UNTIL THE LOCATIONS AND THE TREE VIEW WORK ###### -->
	<xsl:template match="MenuItem" mode="LocateContent">
		<xsl:param name="level"/>
		<xsl:if test="not($level=2 and DisplayName/@siteTemplate='micro')">
			<li id="node{@id}" data-tree-level="{$level}">
				<xsl:attribute name="data-tree-parent">
					<xsl:if test="not(DisplayName/@siteTemplate='micro' or @level='1')">
						<xsl:value-of select="./parent::MenuItem/@id"/>
					</xsl:if>
				</xsl:attribute>
				<xsl:attribute name="class">
					<xsl:if test="@cloneparent &gt; 0 and not(@cloneparent=@id)">
						<xsl:text>clone context</xsl:text>
						<xsl:value-of select="@cloneparent"/>
					</xsl:if>
					<xsl:text> list-group-item level-</xsl:text>
					<xsl:value-of select="$level"/>
					<xsl:if test="MenuItem"> expandable</xsl:if>
				</xsl:attribute>
				<div class="pageCell">
					<xsl:choose>
						<xsl:when test="DisplayName/@siteTemplate='micro' or parent::Menu">
							<i class="fa fa-home fa-lg status activeParent">
								&#160;
							</i>
							<span class="pageName">
								&#160;
								<xsl:value-of select="@name"/>
							</span>
						</xsl:when>
						<xsl:otherwise>

							<xsl:apply-templates select="." mode="status_legend"/>
							<span class="pageName">
								<xsl:value-of select="@name"/>
							</span>
						</xsl:otherwise>
					</xsl:choose>
				</div>
				<div class="optionButtons">
					<xsl:choose>
						<xsl:when test="@Locked"> [Locked]</xsl:when>
						<xsl:when test="@clone &gt; 0 or @cloneparent &gt; 0"> [Cloned]</xsl:when>
						<xsl:when test="not(@id=/Page/ContentDetail/Content/Location[@primary='true']/@pgid)">
							<span class="options">
								<span class="checkbox checkbox-primary">
									<input type="checkbox" name="location" value="{@id}">
										<xsl:if test="@id=/Page/ContentDetail/Content/Location/@pgid">
											<xsl:attribute name="checked">checked</xsl:attribute>
										</xsl:if>
									</input>
									<label>
										<xsl:text> </xsl:text>
									</label>
								</span>
								<xsl:if test="@id=/Page/ContentDetail/Content/Location/@pgid">

									<a href="{$appPath}?ewCmd=LocateContentDetail&amp;pgid={@id}&amp;id={/Page/Request/QueryString/Item[@name='id']}"  class="btn btn-default btn-xs">
										<i class="fa fa-link">&#160;</i>&#160; Relationship
									</a>
								</xsl:if>
							</span>
						</xsl:when>
						<xsl:otherwise>
							<span class="text-muted">Primary</span>&#160;
							<a href="{$appPath}?ewCmd=LocateContentDetail&amp;pgid={@id}&amp;id={/Page/Request/QueryString/Item[@name='id']}"  class="btn btn-default btn-xs">
								<i class="fa fa-link">&#160;</i>&#160; Relationship
							</a>
						</xsl:otherwise>
					</xsl:choose>
				</div>
			</li>
			<xsl:if test="PageVersion">

				<xsl:for-each select="PageVersion">
					<xsl:choose>
						<xsl:when test="not(@id=/Page/ContentDetail/Content/Location[@primary='true']/@pgid)">
							<li id="node{@id}" class="list-group-item level-{$level}" data-tree-level="{$level}" data-tree-parent="{./parent::MenuItem/@id}">
								<div class="pageCell">
									<xsl:apply-templates select="." mode="status_legend"/>
									<span class="pageName">
										<xsl:value-of select="@name"/> - <xsl:value-of select="@desc"/>
									</span>
								</div>
								<div class="optionButtons">
									<div class="checkbox checkbox-primary">
										<input type="checkbox" name="location" value="{@id}">
											<xsl:if test="@id=/Page/ContentDetail/Content/Location/@pgid">
												<xsl:attribute name="checked">checked</xsl:attribute>
											</xsl:if>
										</input>
										<label>
											<xsl:text> </xsl:text>
										</label>
									</div>
									<xsl:if test="@id=/Page/ContentDetail/Content/Location/@pgid">
										<a href="{$appPath}?ewCmd=LocateContentDetail&amp;pgid={@id}&amp;id={/Page/Request/QueryString/Item[@name='id']}" class="btn btn-default btn-xs">
											<i class="fa fa-link">&#160;</i>&#160; Relationship
										</a>
									</xsl:if>
								</div>
							</li>
						</xsl:when>
						<xsl:otherwise>
							<li id="node{@id}" class="list-group-item level-{$level}" data-tree-level="{$level}" data-tree-parent="{./parent::MenuItem/@id}">
								<div class="pageCell">
									<xsl:apply-templates select="." mode="status_legend"/>
									<span class="pageName">
										<xsl:value-of select="@name"/> - <xsl:value-of select="@desc"/>
									</span>
								</div>
								<div class="optionButtons">
									<span class="text-muted">Primary</span>&#160;
									<a href="{$appPath}?ewCmd=LocateContentDetail&amp;pgid={@id}&amp;id={/Page/Request/QueryString/Item[@name='id']}" class="btn btn-default btn-xs">
										<i class="fa fa-link">&#160;</i>&#160;
										Relationship
									</a>
								</div>
							</li>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:for-each>
			</xsl:if>

			<xsl:if test="descendant-or-self::MenuItem[@id=/Page/ContentDetail/Content/Location/@pgid or PageVersion/@id=/Page/ContentDetail/Content/Location/@pgid]/@id">

				<xsl:apply-templates select="MenuItem" mode="LocateContent">
					<xsl:with-param name="level">
						<xsl:value-of select="$level + 1"/>
					</xsl:with-param>
				</xsl:apply-templates>

			</xsl:if>
		</xsl:if>
	</xsl:template>

	<!-- OLD LOCATION TREE VIEW - BROUGHT BACK UNTIL THE LOCATIONS AND THE TREE VIEW WORK -->
	<!--xsl:template match="MenuItem" mode="LocateContent">
		<xsl:param name="level"/>
		<li class="level{$level}">
			<span class="treeNode">
				<xsl:apply-templates select="." mode="status_legend"/>
				<xsl:value-of select="@name"/>
				<xsl:choose>
					<xsl:when test="@Locked"> [Locked]</xsl:when>
					<xsl:when test="not(@id=/Page/ContentDetail/Content/Location[@primary='true']/@pgid)">
						<div class="options">
							<input type="checkbox" name="location" value="{@id}">
								<xsl:if test="@id=/Page/ContentDetail/Content/Location/@pgid">
									<xsl:attribute name="checked">checked</xsl:attribute>
								</xsl:if>
							</input>
							<xsl:if test="@id=/Page/ContentDetail/Content/Location/@pgid">
								<a href="{$appPath}?ewCmd=LocateContentDetail&amp;pgid={@id}&amp;id={/Page/Request/QueryString/Item[@name='id']}" class="adminButton edit">Edit Relationship</a>
							</xsl:if>
						</div>
					</xsl:when>
					<xsl:otherwise>
						<div class="options">[Primary]</div>
						<a href="{$appPath}?ewCmd=LocateContentDetail&amp;pgid={@id}&amp;id={/Page/Request/QueryString/Item[@name='id']}" class="adminButton edit">Edit Relationship</a>
					</xsl:otherwise>
				</xsl:choose>
			</span>
		</li>
		<xsl:if test="MenuItem">
			<ul>
				<xsl:apply-templates select="MenuItem" mode="LocateContent">
					<xsl:with-param name="level">
						<xsl:value-of select="$level + 1"/>
					</xsl:with-param>
				</xsl:apply-templates>
			</ul>
		</xsl:if>
	</xsl:template-->
	<!-- -->
	<!-- BJR -->
	<!--   ##################  Related Search   ##############################   -->
	<!-- -->
	<xsl:template match="Page[@layout='RelatedSearch']" mode="Admin">
		<div class="row" id="tpltRelatedSearch">
			<div class="col-md-4">
				<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
			</div>
			<div class="col-md-8">

				<xsl:text> </xsl:text>
				<xsl:if test="ContentDetail/RelatedResults">
					<form name="myform" action="" method="post" class="panel panel-default">
						<input type="hidden" name="id" value="{ContentDetail/RelatedResults/@nParentID}"/>
						<input type="hidden" name="type" value="{ContentDetail/RelatedResults/@cSchemaName}"/>
						<input type="hidden" name="redirect" value="{/Page/Request/Form/Item[@name='redirect']/node()}"/>
						<div class="panel-heading">
							<h3 class="panel-title">Search Results</h3>
						</div>

						<table cellpadding="0" cellspacing="1" class="table">
							<tbody>
								<tr>
									<th colspan="4">
										<xsl:choose>
											<xsl:when test="ContentDetail/RelatedResults/Content">
												<button type="submit" name="saveRelated" value="Add {ContentDetail/RelatedResults/@cSchemaName}s" class="btn btn-success pull-right">
													<i class="fa fa-plus fa-white">
														<xsl:text> </xsl:text>
													</i><xsl:text> </xsl:text>Add
													<xsl:value-of select="ContentDetail/RelatedResults/@cSchemaName"/>s
												</button>
											</xsl:when>
											<xsl:otherwise>
												<label>
													<xsl:text>No </xsl:text><xsl:value-of select="ContentDetail/RelatedResults/@cSchemaName"/>s Found
												</label>
											</xsl:otherwise>
										</xsl:choose>
									</th>
								</tr>
								<tr>
									<th>Name</th>
									<th>Publish Date</th>
									<th>
										Tick to Relate<br/>
										<button type="button" name="CheckAll" value="Check All" onClick="checkAll(document.myform.list)" class="btn btn-sm btn-success">
											<i class="fa fa-check fa-white">
												<xsl:text> </xsl:text>
											</i> All
										</button>
										<xsl:text> </xsl:text>
										<button type="button" name="UnCheckAll" value="Uncheck All" onClick="uncheckAll(document.myform.list)" class="btn btn-sm  btn-warning">
											<i class="fa fa-close fa-white">
												<xsl:text> </xsl:text>
											</i> All
										</button>
									</th>

									<th>
										Tick to UnRelate<br/>
										<button type="button" name="CheckAll" value="Check All" onClick="checkAll(document.myform.unrelate)" class="btn btn-sm btn-success">
											<i class="fa fa-check fa-white">
												<xsl:text> </xsl:text>
											</i> All
										</button>
										<xsl:text> </xsl:text>
										<button type="button" name="UnCheckAll" value="Uncheck All" onClick="uncheckAll(document.myform.unrelate)" class="btn btn-sm btn-warning">
											<i class="fa fa-close fa-white">
												<xsl:text> </xsl:text>
											</i> All
										</button>
									</th>
								</tr>
								<tr>
									<xsl:for-each select="ContentDetail/RelatedResults/Content">
										<xsl:sort select="@name" />

										<xsl:apply-templates select="." mode="LocateContentNode">
											<xsl:with-param name="indent">&#160;</xsl:with-param>
										</xsl:apply-templates>

									</xsl:for-each>
								</tr>
								<!--<xsl:apply-templates select="ContentDetail/RelatedResults/Content" mode="LocateContentNode"/>-->
								<xsl:if test="ContentDetail/RelatedResults/Content">
									<tr>
										<th>
											<button type="button" name="CheckAll" value="Check All" onClick="checkAll(document.myform.list)" class="btn btn-primary">
												<i class="fa fa-check fa-white">
													<xsl:text> </xsl:text>
												</i> Check All
											</button>
											<xsl:text> </xsl:text>
											<button type="button" name="UnCheckAll" value="Uncheck All" onClick="uncheckAll(document.myform.list)" class="btn btn-primary">
												<i class="fa fa-share fa-white">
													<xsl:text> </xsl:text>
												</i> Uncheck All
											</button>
										</th>
										<th colspan="3">
											<xsl:choose>
												<xsl:when test="ContentDetail/RelatedResults/Content">
													<button type="submit" name="saveRelated" value="Add {ContentDetail/RelatedResults/@cSchemaName}s" class="btn btn-success pull-right">
														<i class="fa fa-plus fa-white">
															<xsl:text> </xsl:text>
														</i><xsl:text> </xsl:text>Add
														<xsl:value-of select="ContentDetail/RelatedResults/@cSchemaName"/>s
													</button>
												</xsl:when>
												<xsl:otherwise>
													<label>
														<xsl:text>No </xsl:text><xsl:value-of select="ContentDetail/RelatedResults/@cSchemaName"/>s Found
													</label>
												</xsl:otherwise>
											</xsl:choose>
										</th>
									</tr>
								</xsl:if>
							</tbody>
						</table>
					</form>
				</xsl:if>
			</div>
		</div>
	</xsl:template>


	<!--   ##################  Parent Change   ##############################   -->
	<!-- -->
	<xsl:template match="Page[@layout='ParentChange']" mode="Admin">
		<div class="row" id="tpltParentChange">
			<div class="col-md-4">
				<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
			</div>
			<div class="col-md-8">

				<xsl:text> </xsl:text>
				<xsl:if test="ContentDetail/RelatedResults">
					<form name="myform" action="" method="post" class="panel panel-default">
						<input type="hidden" name="id" value="{ContentDetail/RelatedResults/@nParentID}"/>
						<input type="hidden" name="type" value="{ContentDetail/RelatedResults/@cSchemaName}"/>
						<input type="hidden" name="redirect" value="{/Page/Request/Form/Item[@name='redirect']/node()}"/>

						<table cellpadding="0" cellspacing="1" class="table">
							<tbody>
								<tr>
									<th>Name</th>
									<th>Move SKU</th>
								</tr>
								<tr>
									<xsl:for-each select="ContentDetail/RelatedResults/Content">
										<xsl:sort select="@name" />
										<xsl:apply-templates select="." mode="LocateParentChange">
											<xsl:with-param name="indent">&#160;</xsl:with-param>
										</xsl:apply-templates>
									</xsl:for-each>
								</tr>

							</tbody>
						</table>
					</form>
				</xsl:if>
			</div>
		</div>
	</xsl:template>


	<xsl:template match="Content" mode="LocateContentNode">
		<xsl:param name="indent"/>
		<xsl:variable name="relationType" select="$page/Request/QueryString/Item[@name='relationType']/node()"/>

		<span class="advancedModeRow locate-content-row" onmouseover="this.className='rowOver'" onmouseout="this.className='advancedModeRow'">
			<tr>
				<td>
					<xsl:apply-templates select="." mode="ContentListName">
						<xsl:with-param name="indent" select="$indent"/>
					</xsl:apply-templates>

				</td>
				<td>
					<xsl:if test="@publishDate!=''">
						<xsl:call-template name="DD_Mon_YYYY">
							<xsl:with-param name="date">
								<xsl:value-of select="@publishDate"/>
							</xsl:with-param>
							<xsl:with-param name="showTime">false</xsl:with-param>
						</xsl:call-template>
					</xsl:if>
				</td>
				<td class="relate">
					<xsl:choose>
						<xsl:when test="@related=1">
						</xsl:when>
						<xsl:when test="@related=1 and not(contains(@sType,$relationType))">
							<xsl:text> </xsl:text>
							<input type="checkbox" name="list" value="{@id}"/>
							<xsl:text> (Related as </xsl:text>
							<xsl:value-of select="@sType"/>
							<xsl:text>)</xsl:text>
							<input type="checkbox" name="unrelate" value="{@id}"/>
						</xsl:when>
						<xsl:otherwise>
							<!--<label>Relate</label>-->
							<!--<input type="checkbox" name="relatecontent_{@id}" value="{@id}"/>-->
							<xsl:text> </xsl:text>
							<input type="checkbox" name="list" value="{@id}"/>
							<!--<xsl:if test="/Page/Request/QueryString/Item[@name='type']">
              <label>2-Way Relationship</label>
              <input type="checkbox" name="reciprocate_{@id}" value="{@id}" />
              </xsl:if>-->
						</xsl:otherwise>
					</xsl:choose>
				</td>
				<td class="unrelate">
					<xsl:choose>
						<xsl:when test="@related=1">
							<input type="checkbox" name="unrelate" value="{@id}"/>
							<xsl:text> </xsl:text>
							<xsl:text>(Related)</xsl:text>
							<xsl:value-of select="$relationType"/>
						</xsl:when>
						<xsl:when test="@related=1 and not(contains(@sType,$relationType))">
							<input type="checkbox" name="unrelate" value="{@id}"/>
							<xsl:text> </xsl:text>
							<xsl:text> (Related as </xsl:text>
							<xsl:value-of select="@sType"/>
							<xsl:text>)</xsl:text>
						</xsl:when>

					</xsl:choose>
				</td>
			</tr>
		</span>
		<!--
    <xsl:apply-templates select="Content" mode="LocateContentNode">
      <xsl:with-param name="indent">
        <xsl:value-of select="$indent"/>
        &#160;&#160;&#160;
      </xsl:with-param>
    </xsl:apply-templates>
	-->

	</xsl:template>


	<xsl:template match="Content" mode="LocateParentChange">
		<xsl:param name="indent"/>
		<xsl:variable name="relationType" select="$page/Request/QueryString/Item[@name='relationType']/node()"/>

		<span class="advancedModeRow locate-content-row" onmouseover="this.className='rowOver'" onmouseout="this.className='advancedModeRow'">
			<tr>
				<td>
					<xsl:apply-templates select="." mode="ContentListName">
						<xsl:with-param name="indent" select="$indent"/>
					</xsl:apply-templates>
				</td>
				<td>
					<xsl:if test="@publishDate!=''">
						<xsl:call-template name="DD_Mon_YYYY">
							<xsl:with-param name="date">
								<xsl:value-of select="@publishDate"/>
							</xsl:with-param>
							<xsl:with-param name="showTime">false</xsl:with-param>
						</xsl:call-template>
					</xsl:if>
				</td>
				<td class="relate">
					<!--<a href="?ewCmd=ParentChange&amp;newParId={@id}" name="updateParent" title="Make Parent" class="btn btn-xs btn-success">
						Make Parent
					</a>-->
					<button type="submit" name="updateParent" value="{@id}" onClick="disableButton(this);?ewCmd=ParentChange&amp;newParId={@id}" class="btn btn-success principle">
						<i class="fa fa-plus fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Make Parent
					</button>

				</td>
			</tr>
		</span>

	</xsl:template>
	<!-- -->
	<!-- BJR -->
	<!--   ##################  Product Groups   ##############################   -->
	<!-- -->
	<xsl:template match="Page[@layout='ProductGroups']" mode="Admin">
		<div class="row" id="tpltProductGroups">
			<div class="col-md-3">
				<div class="alert alert-info">
					<i class="fa fa-info-sign fa-3x pull-right">
						<xsl:text> </xsl:text>
					</i>
					<h4>Hint</h4>
					<p>Product groups enable us to group a range of products together and apply behaviours such as discount rules and shipping methods.</p>
				</div>
			</div>
			<div class="col-md-9">
				<div class="panel panel-default">
					<div class="panel-heading">

						<a href="{$appPath}?ewCmd=AddProductGroups" class="btn btn-success">
							<i class="fa fa-plus fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Add New Group
						</a>
					</div>
					<div class="panel-body">
						<xsl:if test="ContentDetail/ProductCategories">
							<table cellpadding="0" cellspacing="1" class="table">
								<tbody>
									<tr>
										<th></th>
										<th>Product Group</th>
										<th>Group Type</th>
										<th>Product Count</th>
										<th>
											<xsl:text> </xsl:text>
										</th>
									</tr>
									<xsl:for-each select="ContentDetail/ProductCategories/ProductCategory">
										<tr>
											<td>
												<xsl:choose>
													<xsl:when test="Content">
														<a href="{$appPath}?ewCmd=ProductGroups" alt="Click here to collapse" class="btn btn-xs btn-primary">
															<i class="fa fa-minus fa-white">
																<xsl:text> </xsl:text>
															</i>
														</a>
													</xsl:when>
													<xsl:otherwise>
														<a href="{$appPath}?ewCmd=ProductGroups&amp;GrpID={@nCatKey}" alt="Click here to expand" class="btn btn-xs btn-primary">
															<i class="fa fa-plus fa-white">
																<xsl:text> </xsl:text>
															</i>
														</a>
													</xsl:otherwise>
												</xsl:choose>
											</td>
											<td>
												<strong>
													<xsl:value-of select="cCatName/text()"/>
												</strong>
											</td>
											<td>
												<strong>
													<xsl:value-of select="@cCatSchemaName"/>
												</strong>
											</td>
											<td>
												<strong>
													(<xsl:value-of select="@Count"/>)
												</strong>
											</td>
											<td>
												<a href="{$appPath}?ewCmd=AddProductGroupsProduct&amp;GroupId={@nCatKey}" class="btn btn-xs btn-success">
													<i class="fa fa-gift fa-white">
														<xsl:text> </xsl:text>
													</i><xsl:text> </xsl:text>Add Products
												</a>
											</td>

											<td>
												<a href="{$appPath}?ewCmd=EditProductGroups&amp;GroupId={@nCatKey}" class="btn btn-xs btn-primary">
													<i class="fa fa-pencil fa-white">
														<xsl:text> </xsl:text>
													</i><xsl:text> </xsl:text>Edit
												</a>
												<a href="{$appPath}?ewCmd=DeleteProductGroups&amp;GroupId={@nCatKey}" class="btn btn-xs btn-danger">
													<i class="fa fa-trash-o fa-white">
														<xsl:text> </xsl:text>
													</i><xsl:text> </xsl:text>Delete
												</a>
												<a href="/ewcommon/feeds/google/base.ashx?groupId={@nCatKey}" target="_new" class="btn btn-xs btn-default">
													<i class="fa fa-list">
														<xsl:text> </xsl:text>
													</i><xsl:text> </xsl:text>Feed
												</a>
											</td>
										</tr>
										<xsl:for-each select="Content">
											<tr>
												<td> </td>
												<td colspan="3">
													<xsl:apply-templates select="." mode="ContentListName"/>

												</td>
												<td>
													<a href="{$appPath}?ewCmd=RemoveProductGroupsProduct&amp;GroupId={../@nCatKey}&amp;RelId={@relid}" class="btn btn-xs btn-primary">
														<i class="fa fa-remove fa-white">
															<xsl:text> </xsl:text>
														</i>
														<xsl:text> </xsl:text>Remove
													</a>
													<a href="{$appPath}?ewCmd=MoveTop&amp;groupId={../@nCatKey}&amp;id={@id}&amp;lastPage=ProductGroups" title="Move this item to the top" class="btn btn-info btn-sm">
														<i class="fa fa-step-backward fa-rotate-90">&#160;</i>
													</a>
													<a href="{$appPath}?ewCmd=MoveUp&amp;groupId={../@nCatKey}&amp;id={@id}&amp;lastPage=ProductGroups" title="Move this item up by one space" class="btn btn-info btn-sm">
														<i class="fa fa-caret-up fa-lg">&#160;</i>
													</a>
													<a href="{$appPath}?ewCmd=MoveDown&amp;groupId={../@nCatKey}&amp;id={@id}&amp;lastPage=ProductGroups" title="Move this item down by one space" class="btn btn-info btn-sm">
														<i class="fa fa-caret-down fa-lg">&#160;</i>
													</a>
													<a href="{$appPath}?ewCmd=MoveBottom&amp;groupId={../@nCatKey}&amp;id={@id}&amp;lastPage=ProductGroups" title="Move this item to the bottom" class="btn btn-info btn-sm">
														<i class="fa fa-step-forward fa-rotate-90">&#160;</i>
													</a>
												</td>
											</tr>
										</xsl:for-each>
									</xsl:for-each>
								</tbody>
							</table>
						</xsl:if>
					</div>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Content" mode="ContentListName">
		<xsl:param name="indent"/>
		<xsl:value-of select="$indent"/>
		<xsl:choose>
			<xsl:when test="@name!=''">
				<xsl:value-of select="@name"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:copy-of select="Name/node()"/>
			</xsl:otherwise>
		</xsl:choose>
		<xsl:if test="StockCode/node()">
			<xsl:text> - </xsl:text>
			<xsl:value-of select="StockCode/node()"/>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<!-- BJR -->
	<!--   ##################  Discount Rules   ##############################   -->
	<!-- -->
	<xsl:template match="Page[@layout='Discounts']" mode="Admin">
		<div class="row" id="tpltDiscounts">
			<div class="col-md-4">
				<ul class="nav nav-stacked featuresEnabled">
					<li>
						<a href="{$appPath}?ewCmd=ProductGroups" class="btn btn-primary">
							<i class="fa fa-coffee">
								<xsl:text> </xsl:text>
							</i>
							<xsl:text> </xsl:text>Manage Product Groups<br/>
							<span class="btnNotes">
								Create groups of products to apply discounts too.
							</span>
						</a>
					</li>
					<li>
						<a href="{$appPath}?ewCmd=DiscountRules" class="btn btn-primary">
							<i class="fa fa-money">
								<xsl:text> </xsl:text>
							</i>
							<xsl:text> </xsl:text>Manage Discount Rules<br/>
							<span class="btnNotes">
								Create the discounts themselves
							</span>
						</a>
					</li>
				</ul>
			</div>
			<div class="col-md-8">
				<div class="jumbotron">
					To create a discount you must first create groups of products to which that discount applies.
					You must then create discount rules for those groups of products.
				</div>
			</div>
			<!-- Terminus class fix to floating columns -->
			<div class="terminus">&#160;</div>
		</div>
	</xsl:template>




	<xsl:template match="Page[@layout='DiscountRules']" mode="Admin">
		<div class="row" id="tpltDiscountRules">
			<div class="col-md-3">
				<div class="alert alert-info">
					<i class="fa fa-info-sign fa-3x pull-right">
						<xsl:text> </xsl:text>
					</i>
					<h4>Hint</h4>
					<p>Discount rules can be set to apply to groups of products and to groups of users within the system.</p>
					<p>
						<strong>Basic</strong><br/>Money or Percentage off.
					</p>
					<p>
						<strong>Product Quantity / Price Breaks</strong><br/>Money or Percentage off for buying a single product over a certain quantity.
					</p>
					<p>
						<strong>Group Quantity / Price Breaks</strong><br/>Money or Percentage off for buying a number of products from that group over a certain quantity.
					</p>
					<p>
						<strong>X For The Price of Y</strong><br/>Buy X number of a sinple product for the price of Y number of products. e.g. 3 for the price of 2.
					</p>
					<p>
						<strong>Cheapest Item Free</strong><br/>Buy a number of products from a group and get the cheapest free.
					</p>
				</div>
			</div>
			<div class="col-md-9">
				<form action="{$appPath}?ewCmd=DiscountRules" class="ewXform well well-default" name="addDiscountRule" id="addDiscountRule" method="post" onsubmit="return form_check(this)">
					<div class="input-group">
						<span class="input-group-addon">Add Rule &#160;</span>


						<select name="newDiscountType" id="newDiscountType" class="required full form-control">
							<option value="">Select type</option>
							<option value="1">Basic Percentage Discount or Money Off</option>
							<option value="2">Product Quantity/Price Breaks</option>
							<option value="5">Group Quantity/Price Breaks</option>
							<option value="3">X for the Price of Y</option>
							<option value="4">Cheapest Item Free</option>
						</select>
						<span class="input-group-btn">
							<button type="submit" name="addNewDiscountRule" id="addNewDiscountRule"  value="Add" class="btn btn-success principle">
								<i class="fa fa-plus fa-white">
									<xsl:text> </xsl:text>
								</i><xsl:text> </xsl:text>Add
							</button>
						</span>

						<xsl:text> </xsl:text>

					</div>
				</form>

				<xsl:text> </xsl:text>


				<ul class="nav nav-tabs">
					<li role="presentation">
						<xsl:if test="$page/Request/QueryString/Item[@name='isActive']/node()!=0">
							<xsl:attribute name="class">active</xsl:attribute>
						</xsl:if>
						<a href="{$appPath}?ewCmd=DiscountRules&amp;isActive=1"  >
							<xsl:text> Active Codes</xsl:text>
						</a>
					</li>
					<li role="presentation">
						<xsl:if test="$page/Request/QueryString/Item[@name='isActive']/node()=0">
							<xsl:attribute name="class">active</xsl:attribute>
						</xsl:if>
						<a href="{$appPath}?ewCmd=DiscountRules&amp;isActive=0" >
							<xsl:text> InActive Codes</xsl:text>
						</a>
					</li>
				</ul>
				<div class="panel panel-default">

					<table cellpadding="0" cellspacing="1" class="table">
						<tr>
							<th></th>
							<th>Name</th>
							<th>Code</th>
							<th>Type</th>
							<th>

							</th>
						</tr>
						<xsl:for-each select="ContentDetail/DiscountRules/DiscountRule">
							<xsl:sort select="cDiscountName"/>
							<tr>
								<td>
									<xsl:call-template name="status_legend">
										<xsl:with-param name="status">
											<xsl:value-of select="@status"/>
										</xsl:with-param>
									</xsl:call-template>
								</td>
								<td>
									<strong>
										<xsl:value-of select="@cDiscountName"/>
									</strong>
								</td>
								<td>
									<xsl:value-of select="@cDiscountCode"/>
								</td>
								<td>
									<xsl:choose>
										<xsl:when test="nDiscountCat=1">
											<xsl:text>Basic</xsl:text>
										</xsl:when>
										<xsl:when test="nDiscountCat=2">
											<xsl:text>Break by Product</xsl:text>
										</xsl:when>
										<xsl:when test="nDiscountCat=3">
											<xsl:text>X For The Price oF Y</xsl:text>
										</xsl:when>
										<xsl:when test="nDiscountCat=4">
											<xsl:text>Cheapest Item Free</xsl:text>
										</xsl:when>
										<xsl:when test="nDiscountCat=5">
											<xsl:text>Break by Group</xsl:text>
										</xsl:when>
									</xsl:choose>
								</td>
								<td>
									<a href="{$appPath}?ewCmd=EditDiscountRules&amp;DiscId={@nDiscountKey}" class="btn btn-xs btn-primary">
										<i class="fa fa-pencil fa-white">
											<xsl:text> </xsl:text>
										</i><xsl:text> </xsl:text>Edit
									</a>
									<a href="{$appPath}?ewCmd=RemoveDiscountRules&amp;DiscId={@nDiscountKey}" class="btn btn-xs btn-danger">
										<i class="fa fa-trash-o fa-white">
											<xsl:text> </xsl:text>
										</i><xsl:text> </xsl:text>Remove
									</a>
								</td>
							</tr>
							<tr>
								<td>&#160;</td>
								<td colspan="3">
									Allowed User Groups:&#160;<xsl:for-each select="Dir[@nPermLevel='1']">
										<xsl:choose>
											<xsl:when test="@cDirName">
												<xsl:value-of select="@cDirName"/>,&#160;
											</xsl:when>
											<xsl:otherwise>
												All Groups/Users
											</xsl:otherwise>
										</xsl:choose>
									</xsl:for-each>
								</td>
								<td>
									<xsl:if test="Dir[@nPermLevel='0']">
										<xsl:attribute name="rowspan">2</xsl:attribute>
									</xsl:if>
									<a href="{$appPath}?ewCmd=ApplyDirDiscountRules&amp;DiscId={@nDiscountKey}" class="btn btn-xs btn-primary">
										<i class="fa fa-user fa-white">
											<xsl:text> </xsl:text>
										</i><xsl:text> </xsl:text>Select User Groups
									</a>
								</td>
							</tr>
							<xsl:if test="Dir[@nPermLevel='0']">
								<tr>
									<td>&#160;</td>
									<td colspan="3">
										Denied User Groups:&#160;<xsl:for-each select="Dir[@nPermLevel='0']">
											<xsl:choose>
												<xsl:when test="@cDirName">
													<xsl:value-of select="@cDirName"/>,&#160;
												</xsl:when>
												<xsl:otherwise>
													All Groups/Users
												</xsl:otherwise>
											</xsl:choose>
										</xsl:for-each>
									</td>
								</tr>
							</xsl:if>
							<tr>
								<td>&#160;</td>
								<td colspan="3">
									Product Groups:&#160;<xsl:for-each select="ProdCat">
										<xsl:choose>
											<xsl:when test="@cCatName">
												<xsl:value-of select="@cCatName"/>,&#160;
											</xsl:when>
											<xsl:otherwise>
												All Product Groups
											</xsl:otherwise>
										</xsl:choose>
									</xsl:for-each>
								</td>
								<td>
									<a href="{$appPath}?ewCmd=ApplyGrpDiscountRules&amp;DiscId={@nDiscountKey}" class="btn btn-xs btn-primary">
										<i class="fa fa-gift fa-white">
											<xsl:text> </xsl:text>
										</i><xsl:text> </xsl:text>Select Product Groups
									</a>
								</td>
							</tr>
							<tr>
								<td>&#160;</td>
								<td colspan="1">
									<strong>Start Date:</strong>
									&#160;
									<xsl:call-template name="DD_Mon_YYYY">
										<xsl:with-param name="date">
											<xsl:value-of select="@publishDate"/>
										</xsl:with-param>
										<xsl:with-param name="showTime">true</xsl:with-param>
									</xsl:call-template>
								</td>
								<td colspan="3">
									<strong>Expire Date:</strong>
									&#160;
									<xsl:choose>
										<xsl:when test="@expireDate!=''">
											<xsl:call-template name="DD_Mon_YYYY">
												<xsl:with-param name="date">
													<xsl:value-of select="@expireDate"/>
												</xsl:with-param>
												<xsl:with-param name="showTime">true</xsl:with-param>
											</xsl:call-template>
										</xsl:when>
										<xsl:otherwise>
											ongoing
										</xsl:otherwise>
									</xsl:choose>
								</td>
							</tr>
						</xsl:for-each>
					</table>
				</div>
			</div>
		</div>
	</xsl:template>
	<!-- -->


	<!-- -->
	<!--   ##################  PageSettings  ##############################   -->
	<!-- -->
	<xsl:template match="Page[@layout='PageSettings']" mode="Admin">
		<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
		<xsl:apply-templates select="ContentDetail/Content[contains(@type,'xFormQuiz')]" mode="edit"/>
	</xsl:template>

	<!-- -->
	<!--   ##################  PageSettings  ##############################   -->
	<!-- -->
	<xsl:template match="Page[@layout='EditPageSEO']" mode="Admin">
		<ul class="nav nav-tabs">
			<li class="active">
				<a href="#general" data-toggle="tab">General</a>
			</li>
			<li>
				<a href="#dublinCore" data-toggle="tab">Dublin Core</a>
			</li>
			<li>
				<a href="#google" data-toggle="tab">Google</a>
			</li>
			<li>
				<a href="#opengraph" data-toggle="tab">Open Graph</a>
			</li>
			<li>
				<a href="#facebook" data-toggle="tab">Facebook</a>
			</li>
			<li>
				<a href="#twitter" data-toggle="tab">Twitter</a>
			</li>
			<li>
				<a href="#linkedin" data-toggle="tab">LinkedIn</a>
			</li>
			<li>
				<a href="#cookie" data-toggle="tab">Cookie Policy</a>
			</li>
			<li>
				<a href="#settings" data-toggle="tab">Other Settings</a>
			</li>
		</ul>
		<div class="tab-content">
			<div class="tab-pane panel active" id="general">
				<table cellpadding="0" class="table">
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Browser Page Title</xsl:with-param>
						<xsl:with-param name="name">PageTitle</xsl:with-param>
						<xsl:with-param name="type">PlainText</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Alternative Title</xsl:with-param>
						<xsl:with-param name="name">title</xsl:with-param>
						<xsl:with-param name="type">PlainText</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Copyright Message</xsl:with-param>
						<xsl:with-param name="name">Copyright</xsl:with-param>
						<xsl:with-param name="type">FormattedText</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Contact Information</xsl:with-param>
						<xsl:with-param name="name">Contact</xsl:with-param>
						<xsl:with-param name="type">FormattedText</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">JSON-LD</xsl:with-param>
						<xsl:with-param name="name">jsonld</xsl:with-param>
						<xsl:with-param name="type">PlainText</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Exit Modal Popup</xsl:with-param>
						<xsl:with-param name="name">ExitModal</xsl:with-param>
						<xsl:with-param name="type">Module</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Critical Path CSS</xsl:with-param>
						<xsl:with-param name="name">criticalPathCSS</xsl:with-param>
						<xsl:with-param name="type">PlainText</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Meta Refresh</xsl:with-param>
						<xsl:with-param name="name">metaRefresh</xsl:with-param>
						<xsl:with-param name="type">PlainText</xsl:with-param>
					</xsl:call-template>
					<tr>
						<th colspan="3">Meta Tags - Hidden information for search engines.</th>
					</tr>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Page Description</xsl:with-param>
						<xsl:with-param name="name">MetaDescription</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Page Keywords</xsl:with-param>
						<xsl:with-param name="name">MetaKeywords</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Geographical Location</xsl:with-param>
						<xsl:with-param name="name">MetaLocation</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Categories</xsl:with-param>
						<xsl:with-param name="name">MetaCategories</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Classification</xsl:with-param>
						<xsl:with-param name="name">MetaClassification</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Author</xsl:with-param>
						<xsl:with-param name="name">MetaAuthor</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Abstract</xsl:with-param>
						<xsl:with-param name="name">MetaAbstract</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Organisation</xsl:with-param>
						<xsl:with-param name="name">MetaOrganisation</xsl:with-param>
						<xsl:with-param name="type">Organisation</xsl:with-param>
					</xsl:call-template>
				</table>
			</div>
			<div class="tab-pane panel" id="dublinCore">
				<table cellpadding="0" class="table">
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">DC.Title</xsl:with-param>
						<xsl:with-param name="name">DCTitle</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">DC.Creator</xsl:with-param>
						<xsl:with-param name="name">DCCreator</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">DC.Subject</xsl:with-param>
						<xsl:with-param name="name">DCSubject</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">DC.Description</xsl:with-param>
						<xsl:with-param name="name">DCDescription</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">DC.Date</xsl:with-param>
						<xsl:with-param name="name">DCDate</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">DC.Type</xsl:with-param>
						<xsl:with-param name="name">DCType</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">DC.Format</xsl:with-param>
						<xsl:with-param name="name">DCFormat</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">DC.Language</xsl:with-param>
						<xsl:with-param name="name">DCLanguage</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">DC.Relation</xsl:with-param>
						<xsl:with-param name="name">DCRelation</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">DC.Coverage</xsl:with-param>
						<xsl:with-param name="name">DCCoverage</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
				</table>
			</div>
			<div class="tab-pane panel" id="google">
				<table cellpadding="0" class="table">
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Google Verification</xsl:with-param>
						<xsl:with-param name="name">MetaGoogleVerify</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Google Analytics ID</xsl:with-param>
						<xsl:with-param name="name">MetaGoogleAnalyticsID</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Google+ Direct Connect</xsl:with-param>
						<xsl:with-param name="name">GooglePlusPageID</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Google Remarketing Conversion Id</xsl:with-param>
						<xsl:with-param name="name">MetaGoogleRemarketingConversionId</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
				</table>
			</div>

			<!--LUKE OG-->
			<div class="tab-pane panel" id="opengraph">
				<table cellpadding="0" class="table">
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">OpenGraph Title</xsl:with-param>
						<xsl:with-param name="name">ogTitle</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<!-- xsl:call-template name="editNamedContent">
            <xsl:with-param name="desc">OpenGraph Description</xsl:with-param>
            <xsl:with-param name="name">ogdescription</xsl:with-param>
            <xsl:with-param name="type">MetaData</xsl:with-param>
          </xsl:call-template -->
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">OpenGraph Image</xsl:with-param>
						<xsl:with-param name="name">ogimage</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">OpenGraph Image Secure URL</xsl:with-param>
						<xsl:with-param name="name">ogimagesecure</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<!-- xsl:call-template name="editNamedContent">
            <xsl:with-param name="desc">OpenGraph Site Name</xsl:with-param>
            <xsl:with-param name="name">ogsite_name</xsl:with-param>
            <xsl:with-param name="type">MetaData</xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="editNamedContent">
            <xsl:with-param name="desc">OpenGraph Locale</xsl:with-param>
            <xsl:with-param name="name">oglocale</xsl:with-param>
            <xsl:with-param name="type">MetaData</xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="editNamedContent">
            <xsl:with-param name="desc">OpenGraph Type</xsl:with-param>
            <xsl:with-param name="name">ogtype</xsl:with-param>
            <xsl:with-param name="type">MetaData</xsl:with-param>
          </xsl:call-template>
          <xsl:call-template name="editNamedContent">
            <xsl:with-param name="desc">OpenGraph URL</xsl:with-param>
            <xsl:with-param name="name">ogurl</xsl:with-param>
            <xsl:with-param name="type">MetaData</xsl:with-param>
          </xsl:call-template -->
				</table>
			</div>
			<!--END -->
			<div class="tab-pane panel" id="facebook">
				<table cellpadding="0" class="table">
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Facebook UserId</xsl:with-param>
						<xsl:with-param name="name">fb-admins</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Facebook AppId</xsl:with-param>
						<xsl:with-param name="name">fb-app_id</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Facebook Pixel Id</xsl:with-param>
						<xsl:with-param name="name">fb-pixel_id</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Facebook Pages</xsl:with-param>
						<xsl:with-param name="name">fb-pages_id</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>

					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Facebook Chat</xsl:with-param>
						<xsl:with-param name="name">FacebookChat</xsl:with-param>
						<xsl:with-param name="type">FacebookChat</xsl:with-param>
					</xsl:call-template>
				</table>
			</div>
			<!--LUKE 05.10.21-->
			<div class="tab-pane panel" id="twitter">
				<table cellpadding="0" class="table">
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Twitter Card</xsl:with-param>
						<xsl:with-param name="name">twittercard</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Twitter Site</xsl:with-param>
						<xsl:with-param name="name">twittersite</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Twitter Creator</xsl:with-param>
						<xsl:with-param name="name">twittercreator</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Twitter Title</xsl:with-param>
						<xsl:with-param name="name">twittertitle</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Twitter Description</xsl:with-param>
						<xsl:with-param name="name">twitterdescription</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Twitter Image</xsl:with-param>
						<xsl:with-param name="name">twitterimage</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>

				</table>
			</div>
			<div class="tab-pane panel" id="linkedin">
				<table cellpadding="0" class="table">
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">LinkedIn Insight Tag</xsl:with-param>
						<xsl:with-param name="name">LinkedInInsightTag</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">LinkedIn Campaign Id</xsl:with-param>
						<xsl:with-param name="name">LinkedInCampaignId</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
				</table>
			</div>
			<!-- END -->

			<div class="tab-pane panel" id="cookie">
				<table cellpadding="0" class="table">
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Cookie Policy</xsl:with-param>
						<xsl:with-param name="name">CookiePolicy</xsl:with-param>
						<xsl:with-param name="type">CookiePolicy</xsl:with-param>
					</xsl:call-template>

					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Free Cookie Consent from https://www.freeprivacypolicy.com/</xsl:with-param>
						<xsl:with-param name="name">FreeCookieConsent</xsl:with-param>
						<xsl:with-param name="type">FreeCookieConsent</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">CookieFirst from https://www.cookiefirst.com//</xsl:with-param>
						<xsl:with-param name="name">CookieFirst</xsl:with-param>
						<xsl:with-param name="type">CookieFirst</xsl:with-param>
					</xsl:call-template>
				</table>
			</div>
			<div class="tab-pane panel" id="settings">
				<table cellpadding="0" class="table">
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">A1 Webstats ID</xsl:with-param>
						<xsl:with-param name="name">MetaA1WebStatsID</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">A1 Webstats ID V2</xsl:with-param>
						<xsl:with-param name="name">MetaA1WebStatsIDV2</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Who Is Visiting ID</xsl:with-param>
						<xsl:with-param name="name">MetaWhoIsVisitingID</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Lead Forensics ID</xsl:with-param>
						<xsl:with-param name="name">MetaLeadForensicsID</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Yahoo Verification</xsl:with-param>
						<xsl:with-param name="name">MetaYahooVerify</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>

					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">MSN Verification</xsl:with-param>
						<xsl:with-param name="name">MetaMSNVerify</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>

					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">Pinterest Verification</xsl:with-param>
						<xsl:with-param name="name">pinterestVerify</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>

					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">globalsign domain verification</xsl:with-param>
						<xsl:with-param name="name">globalsign-domain-verification</xsl:with-param>
						<xsl:with-param name="type">MetaData</xsl:with-param>
					</xsl:call-template>
					<tr>
						<th colspan="3">Language Settings.</th>
					</tr>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">XmlLang</xsl:with-param>
						<xsl:with-param name="name">XmlLang</xsl:with-param>
						<xsl:with-param name="type">PlainText</xsl:with-param>
					</xsl:call-template>
					<xsl:call-template name="editNamedContent">
						<xsl:with-param name="desc">EncType</xsl:with-param>
						<xsl:with-param name="name">EncType</xsl:with-param>
						<xsl:with-param name="type">PlainText</xsl:with-param>
					</xsl:call-template>
				</table>
			</div>
		</div>

	</xsl:template>

	<xsl:template name="editNamedContent">
		<xsl:param name="desc"/>
		<xsl:param name="name"/>
		<xsl:param name="type"/>
		<xsl:for-each select="/Page/Contents/Content[@name=$name and @type=$type]">

		</xsl:for-each>
		<tr>
			<td>
				<xsl:value-of select="$desc"/>
			</td>
			<td>
				<xsl:choose>
					<xsl:when test="/Page/Contents/Content[@position = $name]">
						<xsl:for-each select="/Page/Contents/Content[@position = $name]/@moduleType">
							<xsl:value-of select="@moduleType"/>
							<br/>
						</xsl:for-each>
					</xsl:when>
					<xsl:otherwise>
						<xsl:for-each select="/Page/Contents/Content[@name=$name and @type=$type and @type!='=CookiePolicy']">
							<xsl:copy-of select="node()"/>
							<br/>
						</xsl:for-each>
					</xsl:otherwise>
				</xsl:choose>
			</td>
			<td>


				<xsl:for-each select="/Page/Contents/Content[@name=$name and @type=$type and @parId!=/Page/@id]">
					<a href="{$appPath}?ewCmd=EditContent&amp;pgid={/Page/@id}&amp;id={@id}" title="Edit master on page: {/Page/Menu/descendant-or-self::MenuItem[@id=/Page/Contents/Content[@name=$name and @type = $type]/@parId]/@name}" class="btn btn-xs btn-primary">
						<i class="fa fa-edit fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Edit Master
					</a>
					<a href="{$appPath}?ewCmd=AddContent&amp;pgid={/Page/@id}&amp;type={$type}&amp;name={$name}" class="btn btn-xs btn-primary">
						<i class="fa fa-plus fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Add Here
					</a>
				</xsl:for-each>
				<xsl:for-each select="/Page/Contents/Content[@name=$name and @type = $type] | /Page/Contents/Content[@type = 'Module' and @position = $name]">
					<a href="{$appPath}?ewCmd=EditContent&amp;pgid={/Page/@id}&amp;id={/Page/Contents/Content[@name=$name and @type=$type]/@id}" title="Click here to edit this content" class="btn btn-xs btn-primary">
						<i class="fa fa-edit fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Edit
					</a>
					<xsl:if test="/Page/Contents/Content[@name=$name and @type = $type and @status='1'] | /Page/Contents/Content[@type = 'Module' and @position = $name and @status='1']">
						<a href="{$appPath}?ewCmd=HideContent&amp;pgid={/Page/@id}&amp;id={@id}" title="Click here to hide this item" class="btn btn-xs btn-danger">
							<i class="fa fa-ban fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Hide
						</a>
					</xsl:if>
					<xsl:if test="/Page/Contents/Content[@name=$name and @type = $type and @status='0'] | /Page/Contents/Content[@type = 'Module' and @position = $name and @status='0']">
						<a href="{$appPath}?ewCmd=ShowContent&amp;pgid={/Page/@id}&amp;id={@id}" title="Click here to show this item" class="btn btn-xs btn-success">
							<i class="fa fa-check-circle-o fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Show
						</a>
						<a href="{$appPath}?ewCmd=DeleteContent&amp;pgid={/Page/@id}&amp;id={@id}" title="Click here to delete this item" class="btn btn-xs btn-danger">
							<i class="fa fa-remove-circle fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Delete
						</a>
						<br/>
					</xsl:if>
				</xsl:for-each>
				<xsl:if test="not(/Page/Contents/Content[@name=$name and @type=$type])">
					<xsl:choose>
						<xsl:when test="$type='Module'">
							<a class="btn btn-default btn-xs pull-right" href="{$appPath}?ewCmd=AddModule&amp;pgid={/Page/@id}&amp;position={$name}">
								<i class="fa fa-th-large">&#160;</i>&#160;<xsl:text>Add Module</xsl:text>
							</a>
						</xsl:when>
						<xsl:otherwise>
							<a href="{$appPath}?ewCmd=AddContent&amp;pgid={/Page/@id}&amp;type={$type}&amp;name={$name}" class="btn btn-xs btn-primary">
								<i class="fa fa-plus fa-white">
									<xsl:text> </xsl:text>
								</i><xsl:text> </xsl:text>Add
							</a>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:if>
			</td>
		</tr>


	</xsl:template>
	<!-- -->
	<!--   ##################  Image / Docs / Media Library   ##############################   -->
	<!-- -->
	<xsl:template match="Page" mode="MaxUploadWidth">0</xsl:template>
	<xsl:template match="Page" mode="MaxUploadHeight">0</xsl:template>

	<xsl:template match="Page[@layout='ImageLib']" mode="MaxUploadWidth">
		<xsl:choose>
			<xsl:when test="$page/Settings/add[@key='web.MaxUploadWidth']/@value!=''">
				<xsl:value-of select="$page/Settings/add[@key='web.MaxUploadWidth']/@value"/>
			</xsl:when>
			<xsl:otherwise>3000</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<xsl:template match="Page[@layout='ImageLib']" mode="MaxUploadHeight">
		<xsl:choose>
			<xsl:when test="$page/Settings/add[@key='web.MaxUploadHeight']/@value!=''">
				<xsl:value-of select="$page/Settings/add[@key='web.MaxUploadHeight']/@value"/>
			</xsl:when>
			<xsl:otherwise>3000</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="Page[@layout='ImageLib' or @layout='DocsLib' or @layout='MediaLib']" mode="adminPageHeader">
		<h1 class="page-header">
			<i class="fa {/Page/AdminMenu/descendant-or-self::MenuItem[@cmd=/Page/@ewCmd]/@icon}">&#160;</i>&#160;
			<xsl:value-of select="/Page/AdminMenu/descendant-or-self::MenuItem[@cmd=/Page/@ewCmd]/@name"/>
			-
			<xsl:value-of select="ContentDetail/*/@name"/>
		</h1>
	</xsl:template>

	<xsl:template match="Page[@layout='ImageLib' or @layout='DocsLib' or @layout='MediaLib']" mode="Admin">
		<xsl:variable name="MaxUploadWidth">
			<xsl:apply-templates select="." mode="MaxUploadWidth"/>
		</xsl:variable>
		<xsl:variable name="MaxUploadHeight">
			<xsl:apply-templates select="." mode="MaxUploadHeight"/>
		</xsl:variable>
		<xsl:variable name="rootPath">
			<xsl:choose>
				<xsl:when test="@layout='ImageLib'">
					<xsl:call-template name="getSettings">
						<xsl:with-param name="sectionName" select="'web'"/>
						<xsl:with-param name="valueName" select="'ImageRootPath'"/>
					</xsl:call-template>
				</xsl:when>
				<xsl:when test="@layout='DocsLib'">
					<xsl:call-template name="getSettings">
						<xsl:with-param name="sectionName" select="'web'"/>
						<xsl:with-param name="valueName" select="'DocRootPath'"/>
					</xsl:call-template>
				</xsl:when>
				<xsl:when test="@layout='MediaLib'">
					<xsl:call-template name="getSettings">
						<xsl:with-param name="sectionName" select="'web'"/>
						<xsl:with-param name="valueName" select="'MediaRootPath'"/>
					</xsl:call-template>
				</xsl:when>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="partPath"  select="translate(descendant::folder[@active='true']/@path,'\','/')"/>

		<xsl:variable name="targetPath">
			<xsl:text>/</xsl:text>
			<xsl:choose>
				<xsl:when test="starts-with($rootPath,'/')">
					<xsl:value-of select="substring-after($rootPath,'/')"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$rootPath"/>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:choose>
				<xsl:when test="starts-with($partPath,'/')">
					<xsl:value-of select="substring-after($partPath,'/')"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$partPath"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="submitPath">
			<xsl:apply-templates select="." mode="SubmitPath"/>
		</xsl:variable>

		<xsl:variable name="pathonly">
			<xsl:if test="$page/Request/QueryString/Item[@name='ewCmd2' and node()='PathOnly']">
				<xsl:text>&amp;pathonly=true</xsl:text>
			</xsl:if>
			<xsl:if test="$page/Request/QueryString/Item[@name='pathonly' and node()='true']">
				<xsl:text>&amp;pathonly=true</xsl:text>
			</xsl:if>
		</xsl:variable>

		<div id="template_FileSystem" class="panel panel-default">

			<div class="panel-body row">

				<div id="MenuTree" class="list-group col-md-3 col-sm-4">
					<xsl:apply-templates select="ContentDetail/folder" mode="FolderTree">
						<xsl:with-param name="level">1</xsl:with-param>
					</xsl:apply-templates>
				</div>

				<div class="col-md-9 col-sm-8">
					<xsl:for-each select="descendant-or-self::folder[@active='true']">
						<ul class="pageControlButtons">
							<xsl:if test="not(contains(/Page/Request/QueryString/Item[@name='contentType'],'popup')) and not(@path='')">
								<li>
									<a href="{$submitPath}ewcmd={/Page/@ewCmd}{$pathonly}&amp;fld={parent::folder/@path}" class="btn btn-primary">
										<xsl:if test="$submitPath!='/?'">
											<xsl:attribute name="data-toggle">modal</xsl:attribute>
											<xsl:attribute name="data-target">
												<xsl:text>#modal-</xsl:text>
												<xsl:value-of select="/Page/Request/QueryString/Item[@name='targetField']/node()"/>
											</xsl:attribute>
										</xsl:if>
										<i class="fa fa-arrow-up fa-white">
											<xsl:text> </xsl:text>
										</i>
										Up Folder
									</a>
								</li>
							</xsl:if>
							<xsl:if test="not(starts-with(/Page/Request/QueryString/Item[@name='fld']/node(),'\FreeStock'))">
								<xsl:if test="contains(/Page/Request/QueryString/Item[@name='multiple'],'true')">
									<li>
										<a href="javascript:;" onclick="getImagePaths();" class="btn btn-success">
											<i class="fa fa-picture-o fa-white">
												<xsl:text> </xsl:text>
											</i><xsl:text> </xsl:text>Add Selected
										</a>
									</li>
								</xsl:if>
								<li>
									<!--<input id="stop" onclick="stop()" type="button" value="stop"/>-->
									<a href="{$submitPath}ewcmd={/Page/@ewCmd}{$pathonly}&amp;ewCmd2=addFolder&amp;fld={@path}&amp;targetForm={/Page/Request/QueryString/Item[@name='targetForm']/node()}&amp;targetField={/Page/Request/QueryString/Item[@name='targetField']/node()}" class="btn btn-success">
										<xsl:if test="$submitPath!='/?'">
											<xsl:attribute name="data-toggle">modal</xsl:attribute>
											<xsl:attribute name="data-target">
												<xsl:text>#modal-</xsl:text>
												<xsl:value-of select="/Page/Request/QueryString/Item[@name='targetField']/node()"/>
											</xsl:attribute>
										</xsl:if>
										<i class="fa fa-folder-open fa-white">
											<xsl:text> </xsl:text>
										</i>&#160;New Folder
									</a>
								</li>
								<li>
									<!-- The fileinput-button span is used to style the file input field as button -->
									<span class="btn btn-success fileinput-button">
										<i class="fa fa-upload fa-white">
											<xsl:text> </xsl:text>
										</i>
										<xsl:text> </xsl:text>
										<span>Upload Files</span>
										<!-- The file input field used as target for the file upload widget -->
										<input id="fileupload" type="file" name="files[]" multiple="" class="fileUploadCheck"/>
									</span>
									<span class="fileupload-loading">
										<xsl:text> </xsl:text>
									</span>
								</li>
								<!--not for popup window or for root..!-->
								<xsl:if test="not(contains(/Page/Request/QueryString/Item[@name='contentType'],'popup')) and not(@path='')">
									<li>
										<a href="{$submitPath}ewcmd={/Page/@ewCmd}&amp;ewCmd2=deleteFolder&amp;fld={@path}" class="btn btn-danger">
											<i class="fa fa-trash-o fa-white">
												<xsl:text> </xsl:text>
											</i>
											Delete Folder
										</a>
									</li>
								</xsl:if>
							</xsl:if>
						</ul>
					</xsl:for-each>
					<form id="testform">
						<div id="uploadFiles">
							<xsl:choose>
								<xsl:when test="contains($browserVersion,'Firefox') or contains($browserVersion,'Chrome')">
									<div id="progress" class="progress progress-success progress-striped">
										<div class="overlay">
											<i class="fas fa-mouse-pointer">&#160;</i> and drop files here to upload
										</div>
										<div class="bar">

										</div>
									</div>
								</xsl:when>
								<xsl:when test="contains($browserVersion,'MSIE') or contains($browserVersion,'')">
									<div class="hint">
										Note: You can upload multiple files without refreshing the page.<br/> You are using Internet Explorer<br/> Try Chrome or Firefox to upload multiple files using drag and drop
									</div>
								</xsl:when>
							</xsl:choose>
							<xsl:for-each select="descendant-or-self::folder[@active='true']">
								<div id="fileupload">
									<xsl:apply-templates select="." mode="ImageFolder">
										<xsl:with-param name="rootPath" select="$rootPath"/>
									</xsl:apply-templates>
								</div>
							</xsl:for-each>
						</div>
					</form>
				</div>
			</div>

			<!-- Terminus class fix to floating columns -->
			<div class="terminus">&#160;</div>
		</div>
	</xsl:template>

	<xsl:template match="Page[@ewCmd='EditContent' or @ewCmd='AddContent' or @ewCmd='EditPage' or @ewCmd='AddPage' or @ewCmd='AddModule'  or @ewCmd='EditMailContent' or @ewCmd='AddMailModule' or @ewCmd='WebSettings']" mode="LayoutAdminJs">
		<!-- The Load Image plugin is included for the preview images and image resizing functionality -->
		<script src="/ewcommon/js/jQuery/fileUploader/loadimage/load-image.all.min.js">/* */</script>
		<!-- The Canvas to Blob plugin is included for image resizing functionality -->
		<script src="/ewcommon/js/jQuery/fileUploader/loadimage/vendor/canvas-to-blob.js">/* */</script>
		<!-- The Iframe Transport is required for browsers without support for XHR file uploads -->
		<script src="/ewcommon/js/jQuery/fileUploader/9.9.3/js/jquery.iframe-transport.js">/* */</script>
		<!-- The basic File Upload plugin -->
		<script src="/ewcommon/js/jQuery/fileUploader/9.9.3/js/jquery.fileupload.js">/* */</script>
		<!-- The File Upload processing plugin -->
		<script src="/ewcommon/js/jQuery/fileUploader/9.9.3/js/jquery.fileupload-process.js">/* */</script>
		<!-- The File Upload image preview & resize plugin -->
		<script src="/ewcommon/js/jQuery/fileUploader/9.9.3/js/jquery.fileupload-image.js">/* */</script>
		<!-- The Image Lazy load plugin -->
		<script src="/ewcommon/js/jQuery/lazy/jquery.lazy.min.js">/* */</script>
	</xsl:template>

	<xsl:template match="Page[@layout='ImageLib' or @layout='DocsLib' or @layout='MediaLib']" mode="LayoutAdminJs">

		<xsl:variable name="MaxUploadWidth">
			<xsl:apply-templates select="." mode="MaxUploadWidth"/>
		</xsl:variable>
		<xsl:variable name="MaxUploadHeight">
			<xsl:apply-templates select="." mode="MaxUploadHeight"/>
		</xsl:variable>
		<xsl:variable name="rootPath">
			<xsl:choose>
				<xsl:when test="@layout='ImageLib'">
					<xsl:call-template name="getSettings">
						<xsl:with-param name="sectionName" select="'web'"/>
						<xsl:with-param name="valueName" select="'ImageRootPath'"/>
					</xsl:call-template>
				</xsl:when>
				<xsl:when test="@layout='DocsLib'">
					<xsl:call-template name="getSettings">
						<xsl:with-param name="sectionName" select="'web'"/>
						<xsl:with-param name="valueName" select="'DocRootPath'"/>
					</xsl:call-template>
				</xsl:when>
				<xsl:when test="@layout='MediaLib'">
					<xsl:call-template name="getSettings">
						<xsl:with-param name="sectionName" select="'web'"/>
						<xsl:with-param name="valueName" select="'MediaRootPath'"/>
					</xsl:call-template>
				</xsl:when>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="partPath"  select="translate(descendant::folder[@active='true']/@path,'\','/')"/>

		<xsl:variable name="targetPath">
			<xsl:text>/</xsl:text>
			<xsl:choose>
				<xsl:when test="starts-with($rootPath,'/')">
					<xsl:value-of select="substring-after($rootPath,'/')"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$rootPath"/>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:choose>
				<xsl:when test="starts-with($partPath,'/')">
					<xsl:value-of select="substring-after($partPath,'/')"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$partPath"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="submitPath">
			<xsl:apply-templates select="." mode="SubmitPath"/>
		</xsl:variable>
		<xsl:variable name="siteURL">
			<xsl:call-template name="getSiteURL"/>
		</xsl:variable>
		<xsl:if test="not(contains(/Page/Request/QueryString/Item[@name='contentType'],'popup'))">
			<!-- The Load Image plugin is included for the preview images and image resizing functionality -->
			<script src="/ewcommon/js/jQuery/fileUploader/loadimage/load-image.all.min.js">/* */</script>
			<!-- The Canvas to Blob plugin is included for image resizing functionality -->
			<script src="/ewcommon/js/jQuery/fileUploader/loadimage/vendor/canvas-to-blob.js">/* */</script>
			<!-- The Iframe Transport is required for browsers without support for XHR file uploads -->
			<script src="/ewcommon/js/jQuery/fileUploader/9.9.3/js/jquery.iframe-transport.js">/* */</script>
			<!-- The basic File Upload plugin -->
			<script src="/ewcommon/js/jQuery/fileUploader/9.9.3/js/jquery.fileupload.js">/* */</script>
			<!-- The File Upload processing plugin -->
			<script src="/ewcommon/js/jQuery/fileUploader/9.9.3/js/jquery.fileupload-process.js">/* */</script>
			<!-- The File Upload image preview & resize plugin -->
			<script src="/ewcommon/js/jQuery/fileUploader/9.9.3/js/jquery.fileupload-image.js">/* */</script>
			<!-- The Image Lazy load plugin -->
			<script src="/ewcommon/js/jQuery/lazy/jquery.lazy.min.js">/* */</script>
		</xsl:if>

		<script>			

			<xsl:text>
       
        var uploadUrl = '/?ewCmd=</xsl:text><xsl:value-of select="$page/@ewCmd"/>\u0026<xsl:text>ewCmd2=FileUpload</xsl:text>\u0026<xsl:text>storageRoot=</xsl:text><xsl:value-of select="$targetPath"/><xsl:text>'

        $('#fileupload').fileupload({
        url: uploadUrl,
        dataType: 'json',
        sequentialUploads: true,
        dropZone:$('#uploadFiles'),
        </xsl:text>
			<xsl:if test="$MaxUploadWidth!='0'">
				<xsl:text>disableImageResize: /Android(?!.*Chrome)|Opera/.test(navigator.userAgent),</xsl:text>
				<xsl:text>imageMaxWidth: </xsl:text>
				<xsl:value-of select="$MaxUploadWidth"/>
				<xsl:text>,</xsl:text>
				<xsl:text>imageMaxHeight: </xsl:text>
				<xsl:value-of select="$MaxUploadHeight"/>
				<xsl:text>,</xsl:text>
			</xsl:if>
			<xsl:apply-templates select="." mode="fileTypeScript"/>
			<xsl:text>
        done: function (e, data) {

        console.log(data);

        $.each(data.files, function (index, file) {

        var targetPath = '</xsl:text><xsl:value-of select="$targetPath"/>';
			var deletePath = '<xsl:value-of select="translate(descendant::folder[@active='true']/@path,'\','/')"/>';
			<xsl:apply-templates select="." mode="newItemScript"/>
			<!--$('#files').prepend(newItem);-->
			var newfilename = "";
			var dataMsg = 'Filename=' + filename;

			var DataUrl = '/ewapi/Cms.Admin/GetExistsFileName';
			$.ajax({
				url: DataUrl,
				data: '',
				type: 'GET',
				success: function (response) {
					newfilename = response;					
					if(newfilename!="")
					{
						$("#oldfilename").html(file.name);
						<!--alert(newfilename);-->	
						if(newfilename.indexOf('true') != -1)
						{
							var arr = newfilename.split(',');
							var extension = arr[0].substring(arr[0].length - 3);							
							$("#changeFilename").modal("show");
							$("#txtfilename").val(arr[0]);						
							<!--$("#lbl1").text(arr[0].substring(0, arr[0].length - 4)+"_1" + '.'+extension);
							$("#lbl2").text(arr[0].substring(0, arr[0].length - 4)+"_2" + '.'+extension);
							$("#lbl3").text(arr[0].substring(0, arr[0].length - 4)+"_3" + '.'+extension);-->
						}	
					}
				}
			});
			
			if(newfilename == "")
			{
				newfilename = filename
			}
			filename = "/" + filename + "/g";
			newItem = newItem.replace(eval(filename), newfilename)
			$('#files').prepend(newItem);
			$('#files .item-image .panel').prepareLibImages();

			$("[data-toggle=popover]").popover({
				html: true,
				container: '#files',
				trigger: 'hover',
				viewport: '#files',
				content: function() {
					return $(this).prev('.popoverContent').html();
				}
			});
			if ($('.pickImageModal').exists()) {
				$('.pickImageModal').find('a[data-toggle!="popover"]').click(function(ev) {
					ev.preventDefault();
					$('.modal-dialog').addClass('loading')
					$('.modal-body').html('<xsl:text disable-output-escaping="yes">&lt;</xsl:text>p class="text-center"<xsl:text disable-output-escaping="yes">&gt;</xsl:text><xsl:text disable-output-escaping="yes">&lt;</xsl:text>h4<xsl:text disable-output-escaping="yes">&gt;</xsl:text><xsl:text disable-output-escaping="yes">&lt;</xsl:text>i class="fa fa-cog fa-spin fa-2x fa-fw"<xsl:text disable-output-escaping="yes">&gt;</xsl:text>;<xsl:text disable-output-escaping="yes">&lt;</xsl:text>/i<xsl:text disable-output-escaping="yes">&gt;</xsl:text>Loading ...<xsl:text disable-output-escaping="yes">&lt;</xsl:text>/h4<xsl:text disable-output-escaping="yes">&gt;</xsl:text><xsl:text disable-output-escaping="yes">&lt;</xsl:text>/p<xsl:text disable-output-escaping="yes">&gt;</xsl:text>');
					var target = $(this).attr("href");
					// load the url and show modal on success
					var currentModal = $('.pickImageModal')
					currentModal.load(target, function() {
						$('.modal-dialog').removeClass('loading')
						currentModal.modal("show");
					});
				});
			};


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

		<script>
			$(function() {
			$('.lazy').lazy();
			});
		</script>

		<div id="changeFilename" class="modal fade" tabindex="-1" role="dialog">
			<div class="modal-dialog" role="document">
				<form id="frmfileData">
					<div class="modal-content">
						<div class="modal-header">
							<button type="button" class="close" data-dismiss="modal">
								<i class="fa fa-times">&#160;</i>
							</button>
							<h4 class="modal-title">File Exists</h4>
						</div>
						<div class="modal-body">
							<p>
								<b>
									<span id="oldfilename">&#160;</span>
								</b> already exists, rename or overwrite?
							</p>
							<div class="form-group">
								
								<label for="txtfilename">Rename file to </label>
								<input type="text" id="txtfilename" value="" class="textbox form-control"/><br/>
								<input type="file" name="postedFile" id="postedFile" class="hidden"/>
								
							</div>
						</div>
						<input type="hidden" id="targetPath" value="{$targetPath}"></input>
						<div class="modal-footer">
							<button type="button" class="btn btn-primary" id="btnSave" onClick="SaveFileName();" >Rename</button>
							<button type="button" id="btnOverWrite" onClick="SaveFileName(true);" class="btn btn-primary">Overwrite</button>
						</div>
					</div>
				</form>
			</div>
		</div>
	</xsl:template>


	<xsl:template match="Page" mode="SubmitPath">
		<xsl:text>/?</xsl:text>
	</xsl:template>

	<xsl:template match="Page[@layout='ImageLib']" mode="fileTypeScript">
		acceptFileTypes: /(\.|\/)(gif|jpe?g|png|tif?f)$/i,
	</xsl:template>



	<xsl:template match="Page[@layout='ImageLib']" mode="newItemScript">

		var guid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {var r = Math.random()*16|0,v=c=='x'?r:r&amp;0x3|0x8;return v.toString(16);});

		<xsl:text disable-output-escaping="yes">var newItem=</xsl:text>'<xsl:text disable-output-escaping="yes">&lt;</xsl:text>div class="item item-image col-md-2 col-sm-4"<xsl:text disable-output-escaping="yes">&gt;</xsl:text>
		<xsl:text disable-output-escaping="yes">&lt;</xsl:text>div class="panel"<xsl:text disable-output-escaping="yes">&gt;</xsl:text>
		<xsl:text disable-output-escaping="yes">&lt;</xsl:text>div class="image-thumbnail"<xsl:text disable-output-escaping="yes">&gt;</xsl:text>
		<xsl:text disable-output-escaping="yes">&lt;</xsl:text>div class="popoverContent" id="imgpopover' + guid + '" role="tooltip"<xsl:text disable-output-escaping="yes">&gt;</xsl:text>
		<xsl:text disable-output-escaping="yes">&lt;</xsl:text>img src="' + targetPath + '/' + file.name + '" class="img-responsive" /<xsl:text disable-output-escaping="yes">&gt;</xsl:text>
		<xsl:text disable-output-escaping="yes">&lt;</xsl:text>div class="popover-description"<xsl:text disable-output-escaping="yes">&gt;</xsl:text>
		<xsl:text disable-output-escaping="yes">&lt;</xsl:text>span class="image-description-name">' + file.name + '<xsl:text disable-output-escaping="yes">&lt;</xsl:text>/span<xsl:text disable-output-escaping="yes">&gt;</xsl:text>
		<xsl:text disable-output-escaping="yes">&lt;</xsl:text>br/<xsl:text disable-output-escaping="yes">&gt;</xsl:text>
		<xsl:text disable-output-escaping="yes">&lt;</xsl:text>/div<xsl:text disable-output-escaping="yes">&gt;</xsl:text>
		<xsl:text disable-output-escaping="yes">&lt;</xsl:text>/div<xsl:text disable-output-escaping="yes">&gt;</xsl:text>
		<xsl:text disable-output-escaping="yes">&lt;</xsl:text>a data-toggle="popover" data-trigger="hover" data-container=".modal-body" data-contentwrapper="#imgpopover' + guid + '" data-placement="top"<xsl:text disable-output-escaping="yes">&gt;</xsl:text>
		<xsl:text disable-output-escaping="yes">&lt;</xsl:text>img src="' + targetPath + '/' + file.name + '" class="img-responsive" /<xsl:text disable-output-escaping="yes">&gt;</xsl:text>
		<xsl:text disable-output-escaping="yes">&lt;</xsl:text>/a<xsl:text disable-output-escaping="yes">&gt;</xsl:text>
		<xsl:text disable-output-escaping="yes">&lt;</xsl:text>/div<xsl:text disable-output-escaping="yes">&gt;</xsl:text>'
		newItem = newItem + '<xsl:text disable-output-escaping="yes">&lt;</xsl:text>div class="description"<xsl:text disable-output-escaping="yes">&gt;</xsl:text>'
		newItem = newItem + '<xsl:text disable-output-escaping="yes">&lt;</xsl:text>a href="{$appPath}?ewCmd=ImageLib&amp;ewCmd2=deleteFile&amp;fld=' + deletePath.replace(/\//g,'\\') + '&amp;file=' + file.name + '" class="btn btn-xs btn-danger"<xsl:text disable-output-escaping="yes">&gt;</xsl:text>
		<xsl:text disable-output-escaping="yes">&lt;</xsl:text>i class="fa fa-trash-o fa-white"<xsl:text disable-output-escaping="yes">&gt;</xsl:text>
		<xsl:text> </xsl:text>
		<xsl:text disable-output-escaping="yes">&lt;</xsl:text>/i<xsl:text disable-output-escaping="yes">&gt;</xsl:text>Delete<xsl:text disable-output-escaping="yes">&lt;</xsl:text>/a<xsl:text disable-output-escaping="yes">&gt;</xsl:text>';
		newItem = newItem + '<xsl:text disable-output-escaping="yes">&lt;</xsl:text>/div<xsl:text disable-output-escaping="yes">&gt;</xsl:text><xsl:text disable-output-escaping="yes">&lt;</xsl:text>div class="img-description"<xsl:text disable-output-escaping="yes">&gt;</xsl:text>
		<xsl:text disable-output-escaping="yes">&lt;</xsl:text>span class="image-description-name">' + file.name + '<xsl:text disable-output-escaping="yes">&lt;</xsl:text>/span<xsl:text disable-output-escaping="yes">&gt;</xsl:text>
		<xsl:text disable-output-escaping="yes">&lt;</xsl:text>br/<xsl:text disable-output-escaping="yes">&gt;</xsl:text><xsl:text disable-output-escaping="yes">&lt;</xsl:text>/div<xsl:text disable-output-escaping="yes">&gt;</xsl:text>';
		newItem = newItem + '<xsl:text disable-output-escaping="yes">&lt;</xsl:text>/div<xsl:text disable-output-escaping="yes">&gt;</xsl:text>';
		var filename = file.name;

	</xsl:template>

	<xsl:template match="folder" mode="ImageFolder">
		<xsl:param name="rootPath"/>
		<xsl:variable name="fileCount" select="count(file)"/>
		<xsl:variable name="popup">
			<xsl:choose>
				<xsl:when test="not(contains(/Page/Request/QueryString/Item[@name='contentType'],'popup'))">
					false
				</xsl:when>
				<xsl:otherwise>
					true
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<div class="alert alert-info">
			<span class="small">
				Showing <xsl:value-of select="$fileCount"/> files
			</span>
		</div>
		<div class="row" id="files">
			<xsl:for-each select="file">
				<div class="item item-image col-lg-2 col-md-3 col-sm-4">
					<div class="panel">
						<div class="image-thumbnail">
							<xsl:variable name="Extension">
								<xsl:value-of select="translate(@Extension,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
							</xsl:variable>
							<xsl:choose>
								<xsl:when test="$Extension='.jpg' or $Extension='.jpeg' or $Extension='.gif' or $Extension='.png' or $Extension='.bmp' or $Extension='.tif' or $Extension='.webp'">
									<xsl:if test="@root">
										<div class="popoverContent" id="imgpopover{position()}" role="tooltip">
											<img src="{concat('/',@root,'/',translate(parent::folder/@path,'\', '/'),'/',@name)}" class="img-responsive"/>
											<div class="popup-description">
												<span class="image-description-name">
													<xsl:value-of select="@name"/>
												</span>
												<br/>
												<xsl:if test="@Extension='.jpg' or @Extension='.jpeg' or @Extension='.gif' or @Extension='.png' or $Extension='.tif' or $Extension='.webp'">
													<xsl:value-of select="@width"/>
													<xsl:text> x </xsl:text>
													<xsl:value-of select="@height"/>
												</xsl:if>
												(<xsl:value-of select="@length"/>kb)
											</div>
										</div>
										<a data-toggle="popover" data-trigger="hover" data-container="body" data-contentwrapper="#imgpopover{position()}" data-placement="top">
											<xsl:choose>
												<xsl:when test="@width&gt;125 and @height&gt;125">
													<img class="lazy" src="/ewcommon/images/loadingImage.gif" data-src="/{@root}{translate(parent::folder/@path,'\', '/')}/{@thumbnail}"/>
												</xsl:when>
												<xsl:otherwise>
													<div class="img-overflow">
														<img src="/{@root}{translate(parent::folder/@path,'\', '/')}/{@name}" class=""  alt=""/>
													</div>
												</xsl:otherwise>
											</xsl:choose>
										</a>
									</xsl:if>
								</xsl:when>
								<xsl:when test="$Extension='.svg'">
									<div class="img-overflow">
										<img src="/{@root}{translate(parent::folder/@path,'\', '/')}/{@name}" width="160" height="160" class="{@class} img-responsive"/>
									</div>
								</xsl:when>
								<xsl:when test="$Extension='.pdf' or $Extension='.doc' or $Extension='.docx'">

								</xsl:when>
								<xsl:when test="$Extension='.swf'">
									<i class="fa fa-flash fa-5x center-block">
										<xsl:text> </xsl:text>
									</i>
								</xsl:when>
								<xsl:when test="$Extension='.flv' or $Extension='.mp4' ">
									<i class="fa fa-film fa-5x center-block">
										<xsl:text> </xsl:text>
									</i>
								</xsl:when>
								<xsl:when test="$Extension='.mp3' or $Extension='.wma'">
									<i class="fa fa-music fa-5x center-block">
										<xsl:text> </xsl:text>
									</i>
								</xsl:when>
								<xsl:otherwise>
									<i class="fa fa-file fa-5x center-block">
										<xsl:text> </xsl:text>
									</i>
								</xsl:otherwise>
							</xsl:choose>
						</div>
						<div class="thumb-button description">
							<xsl:choose>
								<xsl:when test="$popup='true'">
									<xsl:if test="@Extension='.jpg' or @Extension='.jpeg' or @Extension='.gif' or @Extension='.png' or @Extension='.svg'  or @Extension='.tif'  or @Extension='.tiff'">
										<a href="?contentType=popup&amp;ewcmd={/Page/@ewCmd}&amp;ewCmd2=pickImage&amp;fld={parent::folder/@path}&amp;file={@name}{@extension}" data-toggle="modal" data-target="#modal-{/Page/Request/QueryString/Item[@name='targetField']/node()}" class="btn btn-xs btn-info pickImage">
											<i class="fa fa-check-circle-o fa-white">
												<xsl:text> </xsl:text>
											</i>
											Pick Image
										</a>
									</xsl:if>
								</xsl:when>
								<xsl:otherwise>
									<xsl:if test="not(starts-with(/Page/Request/QueryString/Item[@name='fld']/node(),'\FreeStock'))">
										<a class="btn btn-xs btn-primary" href="javascript:CompressImage('/images/{parent::folder/@path}/{@name}{@extension}')">
											<i class="fa fa-compress fa-white">
												<xsl:text> </xsl:text>
											</i>
											<span class="sr-only"> Compress</span>
										</a>
										<a class="btn btn-xs btn-primary" href="{$appPath}?ewCmd={/Page/@ewCmd}&amp;ewCmd2=moveFile&amp;fld={parent::folder/@path}&amp;file={@name}{@extension}">
											<i class="fa fa-arrows fa-white">
												<xsl:text> </xsl:text>
											</i>
											<span class="sr-only"> Move</span>
										</a>
										<a href="{concat('/',@root,'/',translate(parent::folder/@path,'\', '/'),'/',@name)}" class="btn btn-xs btn-warning" download="{@name}">
											<i class="fa fa-download fa-white">
												<xsl:text> </xsl:text>
											</i>
											<span class="sr-only"> Download</span>
										</a>
										<a href="{$appPath}?ewCmd={/Page/@ewCmd}&amp;ewCmd2=deleteFile&amp;fld={parent::folder/@path}&amp;file={@name}{@extension}" class="btn btn-xs btn-danger">
											<i class="fa fa-trash-o fa-white">
												<xsl:text> </xsl:text>
											</i>
											<span class="sr-only">Delete</span>
										</a>
									</xsl:if>
								</xsl:otherwise>
							</xsl:choose>
						</div>
						<div class="img-description">
							<span class="image-description-name">
								<xsl:value-of select="@name"/>
							</span>
							<xsl:choose>
								<xsl:when test="@Extension='.jpg' or @Extension='.jpeg' or @Extension='.gif' or @Extension='.png' or @Extension='.tif'  or  @Extension='.tiff'">
									<xsl:value-of select="@width"/>
									<xsl:text> x </xsl:text>
									<xsl:value-of select="@height"/>
									(<xsl:value-of select="@length"/>kb)
								</xsl:when>
								<xsl:otherwise>
									&#160;
								</xsl:otherwise>
							</xsl:choose>

						</div>
					</div>
				</div>
			</xsl:for-each>
			<xsl:text> </xsl:text>
		</div>
	</xsl:template>

	<xsl:template match="folder[ancestor::Page[@layout='DocsLib']]" mode="ImageFolder">
		<xsl:param name="rootPath"/>

		<table class="table" id="files">
			<thead>
				<tr>
					<th>Filename</th>
					<th>Type</th>
					<th>Options</th>
				</tr>
			</thead>
			<tbody id="files">
				<xsl:for-each select="file">
					<tr>
						<td>
							<i class="icon-file-{translate(@Extension,'.','')}">
								<xsl:text> </xsl:text>
							</i>
							<xsl:text> </xsl:text>
							<a href="{$rootPath}{translate(parent::folder/@path,'\', '/')}/{@name}" target="_new">
								<xsl:value-of select="@name"/>
							</a>
						</td>
						<td>
							<xsl:value-of select="@Extension"/>
						</td>
						<td>
							<xsl:choose>
								<xsl:when test="not(contains(/Page/Request/QueryString/Item[@name='contentType'],'popup'))">
									<a href="{$appPath}?ewCmd={/Page/@ewCmd}&amp;ewCmd2=deleteFile&amp;fld={parent::folder/@path}&amp;file={@name}{@extension}" class="btn btn-xs btn-danger">
										<i class="fa fa-trash-o fa-white">
											<xsl:text> </xsl:text>
										</i>
										<xsl:text> </xsl:text>Delete
									</a>
								</xsl:when>
								<xsl:otherwise>
									<xsl:choose>
										<!--Pick Document-->
										<xsl:when test="/Page/@ewCmd='DocsLib'">
											<a onclick="passDocToForm('{/Page/Request/QueryString/Item[@name='targetForm']/node()}','{/Page/Request/QueryString/Item[@name='targetField']/node()}','/{@root}{translate(parent::folder/@path,'\','/')}/{@name}');" class="btn btn-xs btn-default" href="#">
												<i class="fa fa-file-o fa-white">
													<xsl:text> </xsl:text>
												</i>
												<xsl:text> </xsl:text>Pick
											</a>
										</xsl:when>
										<!--Pick Media-->
										<xsl:when test="/Page/@ewCmd='MediaLib'">
											<a onclick="passMediaToForm('{/Page/Request/QueryString/Item[@name='targetForm']/node()}','{/Page/Request/QueryString/Item[@name='targetField']/node()}','/{@root}{translate(parent::folder/@path,'\','/')}/{@name}');" class="btn btn-xs btn-default" href="#">
												<i class="fa fa-video-camera fa-white">
													<xsl:text> </xsl:text>
												</i>
												<xsl:text> </xsl:text>Pick Media
											</a>
										</xsl:when>
										<!--Pick Other (so far images)-->
										<xsl:otherwise>
											<a href="{$appPath}?ewCmd={/Page/@ewCmd}&amp;ewCmd2=pickImage&amp;fld={parent::folder/@path}&amp;file={@name}{@extension}" class="btn btn-xs btn-default">
												<i class="fa fa-picture-o fa-white">
													<xsl:text> </xsl:text>
												</i>
												<xsl:text> </xsl:text>Pick Image
											</a>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:otherwise>
							</xsl:choose>
						</td>
					</tr>
				</xsl:for-each>
				<xsl:text> </xsl:text>
			</tbody>
		</table>
	</xsl:template>


	<xsl:template match="Page[@layout='DocsLib']" mode="fileTypeScript">
		acceptFileTypes: /(\.|\/)(pdf|doc?x|zip|xls?x|ppt?x)$/i,
	</xsl:template>

	<xsl:template match="Page[@layout='DocsLib']" mode="newItemScript">

		var newItem = '<xsl:text disable-output-escaping="yes">&lt;</xsl:text>tr<xsl:text disable-output-escaping="yes">&gt;</xsl:text><xsl:text disable-output-escaping="yes">&lt;</xsl:text>td<xsl:text disable-output-escaping="yes">&gt;</xsl:text><xsl:text disable-output-escaping="yes">&lt;</xsl:text>i class="icon-file-' + /[^.]+$/.exec(file.name) + '"<xsl:text disable-output-escaping="yes">&gt;</xsl:text> <xsl:text disable-output-escaping="yes">&lt;</xsl:text>/i<xsl:text disable-output-escaping="yes">&gt;</xsl:text> ' + file.name.replace(/\ /g,'-') + '<xsl:text disable-output-escaping="yes">&lt;</xsl:text>/td<xsl:text disable-output-escaping="yes">&gt;</xsl:text><xsl:text disable-output-escaping="yes">&lt;</xsl:text>td<xsl:text disable-output-escaping="yes">&gt;</xsl:text>.' + /[^.]+$/.exec(file.name) + '<xsl:text disable-output-escaping="yes">&lt;</xsl:text>/td<xsl:text disable-output-escaping="yes">&gt;</xsl:text>';
		newItem = newItem + '<xsl:text disable-output-escaping="yes">&lt;</xsl:text>td<xsl:text disable-output-escaping="yes">&gt;</xsl:text><xsl:text disable-output-escaping="yes">&lt;</xsl:text>a href="{$appPath}?ewCmd=DocsLib&amp;ewCmd2=deleteFile&amp;fld=' + deletePath.replace(/\//g,'\\') + '&amp;file=' + file.name + '" class="btn btn-xs btn-danger"><xsl:text disable-output-escaping="yes">&lt;</xsl:text>i class="fa fa-trash-o fa-white"<xsl:text disable-output-escaping="yes">&gt;</xsl:text> <xsl:text disable-output-escaping="yes">&lt;</xsl:text>/i<xsl:text disable-output-escaping="yes">&gt;</xsl:text> Delete<xsl:text disable-output-escaping="yes">&lt;</xsl:text>/a<xsl:text disable-output-escaping="yes">&gt;</xsl:text><xsl:text disable-output-escaping="yes">&lt;</xsl:text>/td<xsl:text disable-output-escaping="yes">&gt;</xsl:text><xsl:text disable-output-escaping="yes">&lt;</xsl:text>/tr<xsl:text disable-output-escaping="yes">&gt;</xsl:text>'

		var filename = file.name;

	</xsl:template>


	<xsl:template match="Page[@layout='MediaLib']" mode="fileTypeScript">
		acceptFileTypes: /(\.|\/)(swf|flv|mp3|mp4|wav|m4a|wma|au|aiff|wmv|m4v|mov|avi|asf)$/i,
	</xsl:template>

	<xsl:template match="Page[@layout='MediaLib']" mode="newItemScript">
		var newItem = '<xsl:text disable-output-escaping="yes">&lt;</xsl:text>div class="item col-md-2 col-sm-4"<xsl:text disable-output-escaping="yes">&gt;</xsl:text><xsl:text disable-output-escaping="yes">&lt;</xsl:text>div class="panel panel-default"><xsl:text disable-output-escaping="yes">&lt;</xsl:text>div class="panel-body"><xsl:text disable-output-escaping="yes">&lt;</xsl:text>div class="ItemThumbnail"<xsl:text disable-output-escaping="yes">&gt;</xsl:text><xsl:text disable-output-escaping="yes">&lt;</xsl:text>img src="' + targetPath + '/' + file.name + '" width="85" height="48 " class="" /<xsl:text disable-output-escaping="yes">&gt;</xsl:text><xsl:text disable-output-escaping="yes">&lt;</xsl:text>/div<xsl:text disable-output-escaping="yes">&gt;</xsl:text>'
		newItem = newItem + '<xsl:text disable-output-escaping="yes">&lt;</xsl:text>div class="description"<xsl:text disable-output-escaping="yes">&gt;</xsl:text>' + file.name + '<xsl:text disable-output-escaping="yes">&lt;</xsl:text>br /<xsl:text disable-output-escaping="yes">&gt;</xsl:text><xsl:text disable-output-escaping="yes">&lt;</xsl:text>/div<xsl:text disable-output-escaping="yes">&gt;</xsl:text>'
		newItem = newItem + '<xsl:text disable-output-escaping="yes">&lt;</xsl:text>a href="{$appPath}?ewCmd=ImageLib&amp;ewCmd2=deleteFile&amp;fld=' + deletePath.replace(/\//g,'\\') + '&amp;file=' + file.name + '" class="btn btn-xs btn-danger"<xsl:text disable-output-escaping="yes">&gt;</xsl:text><xsl:text disable-output-escaping="yes">&lt;</xsl:text>i class="fa fa-trash-o fa-white"<xsl:text disable-output-escaping="yes">&gt;</xsl:text><xsl:text> </xsl:text><xsl:text disable-output-escaping="yes">&lt;</xsl:text>/i<xsl:text disable-output-escaping="yes">&gt;</xsl:text>Delete<xsl:text disable-output-escaping="yes">&lt;</xsl:text>/a<xsl:text disable-output-escaping="yes">&gt;</xsl:text><xsl:text disable-output-escaping="yes">&lt;</xsl:text>/div<xsl:text disable-output-escaping="yes">&gt;</xsl:text><xsl:text disable-output-escaping="yes">&lt;</xsl:text>/div<xsl:text disable-output-escaping="yes">&gt;</xsl:text><xsl:text disable-output-escaping="yes">&lt;</xsl:text>/div<xsl:text disable-output-escaping="yes">&gt;</xsl:text>'

		var filename = file.name;
	</xsl:template>

	<xsl:template match="folder" mode="MediaFolder">
		<table class="adminList">
			<tr>
				<th>Thumbnail</th>
				<th>Filename</th>
				<th>Type</th>
				<th>Sizes</th>
				<th>Options</th>
			</tr>
			<xsl:for-each select="file">
				<tr>
					<td>
						<!--For the document fa-->
						<xsl:choose>
							<xsl:when test="@Extension='.jpg' or @Extension='.jpeg' or @Extension='.gif' or @Extension='.png'">
								<xsl:if test="@root">
									<img src="{@root}/{parent::folder/@path}/{@name}" width="25" height="25" alt=""/>
								</xsl:if>
							</xsl:when>
							<xsl:otherwise>
								<xsl:if test="@icon">
									<img src="{@root}/{@icon}" width="25" height="25" alt=""/>
								</xsl:if>
							</xsl:otherwise>
						</xsl:choose>
						<!--End of the document fa-->
					</td>
					<td>
						<xsl:value-of select="@name"/>
					</td>
					<td>
						<xsl:value-of select="@Extension"/>
					</td>
					<td>
						<xsl:if test="@Extension='.jpg' or @Extension='.jpeg' or @Extension='.gif' or @Extension='.png'">
							<xsl:value-of select="@width"/> x  <xsl:value-of select="@height"/>
						</xsl:if>
					</td>
					<td>
						<xsl:choose>
							<xsl:when test="not(contains(/Page/Request/QueryString/Item[@name='contentType'],'popup'))">
								<a href="{$appPath}?ewCmd={/Page/@ewCmd}&amp;ewCmd2=deleteFile&amp;fld={parent::folder/@path}&amp;file={@name}{@extension}" class="adminButton delete">Delete</a>
							</xsl:when>
							<xsl:otherwise>
								<a onclick="passMediaToForm('{/Page/Request/QueryString/Item[@name='targetForm']/node()}','{/Page/Request/QueryString/Item[@name='targetField']/node()}','/{@root}{translate(parent::folder/@path,'\','/')}/{@name}');" class="adminButton add pickimage" href="#">Pick Media</a>
							</xsl:otherwise>
						</xsl:choose>
					</td>
				</tr>
			</xsl:for-each>
		</table>
	</xsl:template>

	<xsl:template match="folder" mode="FolderTree">
		<xsl:param name="level"/>
		<li id="node{translate(@path,'\','-')}" data-tree-level="{$level}" data-tree-parent="{translate(parent::folder/@path,'\','-')}">
			<xsl:attribute name="class">
				<xsl:text>list-group-item level-</xsl:text>
				<xsl:value-of select="$level"/>
				<xsl:if test="@active='true'">
					<xsl:text> active collapsable</xsl:text>
				</xsl:if>
			</xsl:attribute>
			<a href="{$appPath}?ewCmd={/Page/@ewCmd}&amp;fld={@path}&amp;targetForm={/Page/Request/QueryString/Item[@name='targetForm']/node()}&amp;targetField={/Page/Request/QueryString/Item[@name='targetField']/node()}">
				<i>
					<xsl:attribute name="class">
						<xsl:text>fa fa-lg</xsl:text>
						<xsl:choose>
							<xsl:when test="@active='true'">
								<xsl:text> fa-folder-open-o</xsl:text>
							</xsl:when>
							<xsl:otherwise>
								<xsl:text> fa-folder-o</xsl:text>
							</xsl:otherwise>
						</xsl:choose>
						<xsl:if test="folder"> activeParent</xsl:if>
					</xsl:attribute>
					&#160;
				</i>
				<xsl:value-of select="@name"/>
			</a>
		</li>
		<xsl:if test="folder">
			<xsl:apply-templates select="folder" mode="FolderTree">
				<xsl:with-param name="level">
					<xsl:value-of select="$level + 1"/>
				</xsl:with-param>
			</xsl:apply-templates>
		</xsl:if>
	</xsl:template>

	<!-- -->
	<!--   ##################  EditPermissions - dynamically generated from menu   ##############################   -->
	<!-- -->
	<xsl:template match="Page[@layout='EditStructurePermissions']" mode="Admin">
		<div id="template_EditPermissions" class="row">
			<div class="col-md-3">
				<div class="alert alert-info">
					<i class="fa fa-info-sign fa-3x pull-left">
						<xsl:text> </xsl:text>
					</i>This page allows you to access permissions settings for each page.
				</div>
			</div>
			<div class="col-md-9">
				<div class="panel">
					<ul id="MenuTree" class="treeview">
						<xsl:apply-templates select="Menu/MenuItem" mode="editStructurePermissions">
							<xsl:with-param name="level">1</xsl:with-param>
						</xsl:apply-templates>
					</ul>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="MenuItem" mode="editStructurePermissions">
		<xsl:param name="level"/>
		<li id="node{@id}">
			<table cellspacing="0">
				<tr class="treeNode">
					<td class="status">
						<xsl:apply-templates select="." mode="status_legend"/>
					</td>
					<td class="pageName">
						<xsl:value-of select="@name"/>
					</td>
					<td class="optionButtons">
						<a href="{$appPath}?ewCmd=EditPagePermissions&amp;pgid={@id}" class="btn btn-primary btn-xs" title="Click here to edit this page">
							<i class="fa fa-lock">
								<xsl:text> </xsl:text>
							</i> Edit Permissions
						</a>
					</td>
				</tr>
			</table>
		</li>
		<xsl:if test="MenuItem">
			<ul>
				<xsl:apply-templates select="MenuItem" mode="editStructurePermissions"/>
			</ul>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<!--   ##################  EditPermissions - dynamically generated from menu   ##############################   -->
	<!-- -->
	<xsl:template match="Page[@layout='EditDirectoryItemPermissions']" mode="adminPageHeader">
		<h1 class="page-header">
			<i class="fa fa-key">
				<xsl:text> </xsl:text>
			</i>
			<xsl:text> </xsl:text>
			Permissions for <xsl:value-of select="name(ContentDetail/*)"/> - <xsl:value-of select="ContentDetail/*/@name"/>
		</h1>
	</xsl:template>

	<xsl:template match="Page[@layout='EditDirectoryItemPermissions']" mode="Admin">
		<div class="row" id="template_permissions">
			<div class="col-md-3">
				<div class="panel panel-default">
					<div class="panel-heading">
						<h4 class="panel-title">Edit Permissions</h4>
					</div>
					<div class="panel-body">
						<p>
							OPEN<br/>
							Means that this page can be viewed by everyone. It has no permissions set for it at all. If you set permit for one of these then the page will then be restircted so only members of this group can access it
						</p>
						<p>
							DENIED<br/>
							This page is explicity denied for access by this group/company/dept
						</p>
						<p>
							IMPLIED DENIED<br/>
							This page cannot be viewed by members of this group/company/dept because it has an exclusive permission to be viewed by other entities
						</p>
						<p>
							VIEW<br/>
							This page can be viewed by members of this group
						</p>
						<p>
							VIEW by xxxx<br/>
							This page can be viewed because of permission on xxxx
						</p>
						<p>
							INHERITED<br/>
							This takes its permissions from its parent, they will be overridden by permissions any applied directly to this page.
						</p>
					</div>
				</div>
			</div>
			<div class="col-md-9">
				<form action="?ewCmd=DirPermissions" method="post" class="ewXform">
					<div class="panel panel-default">
						<div class="panel-heading">
							<h4 class="panel-title">
								<xsl:value-of select="name(ContentDetail/*)"/><xsl:text> </xsl:text><xsl:value-of select="ContentDetail/*/@name"/> is a member of the following
							</h4>
						</div>
						<div class="panel-body">

							<ul>
								<xsl:for-each select="ContentDetail/*[1]/Role | ContentDetail/*[1]/Company | ContentDetail/*[1]/Department | ContentDetail/*[1]/Group">
									<li>
										<xsl:value-of select="name()"/> - <xsl:value-of select="@name"/>
									</li>
								</xsl:for-each>
							</ul>
							<h4>And has the following rights to access...</h4>

							<button type="submit" name="submit" value="Save Permissions"  class="btn btn-success principle">
								<i class="fa fa-check">
									<xsl:text> </xsl:text>
								</i>
								<xsl:text> </xsl:text>Save Permissions
							</button>
							<button type="button" name="return" value="Return" onclick="window.history.back();" class="btn btn-primary">
								<i class="fa fa-arrow-left">
									<xsl:text> </xsl:text>
								</i>
								<xsl:text> </xsl:text>Return
							</button>
							<br/>
							<br/>
							<input type="hidden" name="parId" value="{ContentDetail/*[1]/@id}"/>
						</div>

						<ul id="MenuTree" class="list-group treeview">
							<xsl:apply-templates select="ContentDetail/Menu/MenuItem" mode="editDirectoryItemPermissions">
								<xsl:with-param name="level">1</xsl:with-param>
							</xsl:apply-templates>
						</ul>

						<div class="panel-footer">
							<button type="submit" name="submit" value="Save Permissions"  class="btn btn-success principle">
								<i class="fa fa-check">
									<xsl:text> </xsl:text>
								</i>
								<xsl:text> </xsl:text>Save Permissions
							</button>
							<button type="button" name="return" value="Return" onclick="window.history.back();" class="btn btn-primary">
								<i class="fa fa-arrow-left">
									<xsl:text> </xsl:text>
								</i>
								<xsl:text> </xsl:text>Return
							</button>
							<div class="clearfix">&#160; </div>
						</div>
					</div>
				</form>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="MenuItem" mode="editDirectoryItemPermissions">
		<xsl:param name="level"/>
		<li id="node{@id}" data-tree-level="{$level}" data-tree-parent="{./parent::MenuItem/@id}">
			<xsl:attribute name="class">
				<xsl:if test="@cloneparent &gt; 0 and not(@cloneparent=@id)">
					<xsl:text>clone context</xsl:text>
					<xsl:value-of select="@cloneparent"/>
				</xsl:if>
				<xsl:text> list-group-item level-</xsl:text>
				<xsl:value-of select="$level"/>
				<xsl:if test="MenuItem"> collapsable</xsl:if>
			</xsl:attribute>
			<div class="pageCell">
				<xsl:variable name="displayName">
					<xsl:apply-templates select="." mode="getDisplayName" />
				</xsl:variable>
				<a href="{$appPath}?ewCmd=EditPagePermissions&amp;pgid={@id}">
					<span class="status">
						<xsl:apply-templates select="." mode="status_legend"/>
					</span>
					<xsl:value-of select="$displayName"/>
				</a>
			</div>
			<div class="optionButtons">
				<xsl:choose>
					<xsl:when test="contains(@access,'INHERITED DENIED') or ancestor::MenuItem[contains(@access,'DENIED')]">
						<xsl:variable name="ancDeny" select="ancestor::MenuItem[contains(@access,'DENIED')]"/>
						<xsl:choose>
							<xsl:when test="contains(@access,'INHERITED DENIED')">
								<xsl:value-of select="@access"/>
							</xsl:when>
							<xsl:otherwise>
								INHERITED <xsl:value-of select="$ancDeny[1]/@access"/>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="@access"/>
						<xsl:if test="not(contains(@access,'INHERITED'))">
							&#160;&#160;
							<xsl:if test="contains(@access,'DENIED') or contains(@access,'OPEN') or @access = 'VIEW'">
								<input name="page_{@id}" type="radio" id="permit_{@id}" value="permit">
									<xsl:if test="@access = 'VIEW'">
										<xsl:attribute name="checked">&#160;checked</xsl:attribute>
									</xsl:if>
								</input>
								<label for="permit_{@id}">Permit</label>
								&#160;
							</xsl:if>
							<xsl:if test="contains(@access,'OPEN') or contains(@access,'VIEW')">
								<input name="page_{@id}" type="radio" id="deny_{@id}" value="deny">
									<xsl:if test="contains(@access,'DENIED')">
										<xsl:attribute name="checked">&#160;checked</xsl:attribute>
									</xsl:if>
								</input>
								<label for="deny_{@id}">&#160;Deny</label>
								&#160;
							</xsl:if>
						</xsl:if>
						<xsl:if test="contains(@access,'INHERITED VIEW') ">
							<input name="page_{@id}" type="radio" id="permit_{@id}" value="permit">
								<xsl:if test="@access = 'VIEW'">
									<xsl:attribute name="checked">&#160;checked</xsl:attribute>
								</xsl:if>
							</input>
							<label for="permit_{@id}">&#160;Permit</label>
							&#160;
							<input name="page_{@id}" type="radio" id="deny_{@id}" value="deny"/>
							<label for="deny_{@id}">&#160;Deny</label>&#160;
						</xsl:if>
						<xsl:if test="not(contains(@access,'INHERITED DENIED'))">
							<input name="page_{@id}" type="radio" id="none_{@id}" value="remove">
								<xsl:if test="contains(@access,'OPEN') or contains(@access,'INHERITED VIEW')">
									<xsl:attribute name="checked">checked</xsl:attribute>
								</xsl:if>
							</input>
							<label for="none_{@id}">&#160;None</label>
						</xsl:if>
					</xsl:otherwise>
				</xsl:choose>
			</div>
			<xsl:if test="MenuItem">

				<xsl:apply-templates select="MenuItem" mode="editDirectoryItemPermissions">
					<xsl:with-param name="level">
						<xsl:value-of select="$level + 1"/>
					</xsl:with-param>
				</xsl:apply-templates>

			</xsl:if>
		</li>

	</xsl:template>
	<!-- -->
	<!--   ##################  Generic Display Form  ##############################   -->

	<xsl:template match="Page[@layout='Profile']" mode="adminPageHeader">
		<h1 class="page-header">
			<i class="fa {/Page/AdminMenu/descendant-or-self::MenuItem[@cmd=/Page/@ewCmd]/@icon}">&#160;</i>&#160;
			<xsl:value-of select="ContentDetail/*/Name/node()"/> - Profile
		</h1>
	</xsl:template>

	<xsl:template match="Page[@layout='Profile']" mode="Admin">
		<xsl:for-each select="ContentDetail/User">
			<div id="template_ListDirectory" class="panel panel-default">
				<div class="panel-heading">
					<div class="pull-right">
						<a href="{$appPath}?ewCmd=EditDirItem&amp;DirType=User&amp;id={@id}" class="btn btn-xs btn-primary">
							<i class="fa fa-user fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Edit
						</a>
						<a href="{$appPath}?ewCmd=ResetUserPwd&amp;id={@id}" class="btn btn-xs btn-success">
							<i class="fa fa-repeat fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Reset Pwd
						</a>
						<a href="{$appPath}?ewCmd=PreviewOn&amp;PreviewUser={@id}" class="btn btn-xs btn-success">
							<i class="fa fa-user-secret fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Impersonate
						</a>
						<a href="{$appPath}?ewCmd=MaintainRelations&amp;type=Role&amp;id={@id}" class="btn btn-xs btn-primary">
							<i class="fa fa-cog fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Roles
						</a>
						<xsl:if test="/Page/AdminMenu/MenuItem/MenuItem/MenuItem[@cmd='ListGroups']">
							<a href="{$appPath}?ewCmd=MaintainRelations&amp;type=Group&amp;id={@id}" class="btn btn-xs btn-primary">
								<i class="fa fa-glass fa-white">
									<xsl:text> </xsl:text>
								</i>
								<xsl:text> </xsl:text>Groups
							</a>
							<a href="{$appPath}?ewCmd=DirPermissions&amp;parid={@id}" class="btn btn-xs btn-primary">
								<i class="fa fa-lock fa-white">
									<xsl:text> </xsl:text>
								</i>
								<xsl:text> </xsl:text>Permissions
							</a>
						</xsl:if>
						<xsl:if test="/Page/AdminMenu/MenuItem/MenuItem/MenuItem[@cmd='ListCompanies']">
							<a href="{$appPath}?ewCmd=MaintainRelations&amp;type=Company&amp;id={@id}" class="btn btn-xs btn-primary">
								<i class="fa fa-building-o fa-white">
									<xsl:text> </xsl:text>
								</i>
								<xsl:text> </xsl:text>Companies
							</a>

							<a href="{$appPath}?ewCmd=MaintainRelations&amp;type=Department&amp;id={@id}" class="btn btn-xs btn-primary">
								<i class="fa fa-group fa-white">
									<xsl:text> </xsl:text>
								</i><xsl:text> </xsl:text>Dept
							</a>
						</xsl:if>

						<xsl:if test="/Page[@userIntegrations='true']">
							<a href="{$appPath}?ewCmd=UserIntegrations&amp;dirId={@id}" class="btn btn-xs btn-primary">
								<i class="fa fa-random fa-white">
									<xsl:text> </xsl:text>
								</i>
								<xsl:text> </xsl:text>Integrations
							</a>
						</xsl:if>
						<xsl:if test="/Page/AdminMenu/descendant-or-self::MenuItem[@cmd='EditUserContact']">
							<a href="{$appPath}?ewCmd=Profile&amp;DirType=User&amp;id={@id}" class="btn btn-xs btn-primary">
								<i class="fa fa-map-marker fa-white">
									<xsl:text> </xsl:text>
								</i>
								<xsl:text> </xsl:text>Addresses
							</a>
						</xsl:if>
						<xsl:if test="/Page/AdminMenu/MenuItem/MenuItem/MenuItem[@cmd='MemberActivity']">
							<a href="{$appPath}?ewCmd=MemberActivity&amp;UserId={@id}" class="btn btn-xs btn-primary">
								<i class="fa fa-signal fa-white">
									<xsl:text> </xsl:text>
								</i>
								<xsl:text> </xsl:text>Activity
							</a>
						</xsl:if>
						<xsl:choose>
							<xsl:when test="Status='0'">
								<a href="{$appPath}?ewCmd=DeleteDirItem&amp;DirType=User&amp;id={@id}" class="btn btn-xs btn-danger">
									<i class="fa fa-trash-o fa-white">
										<xsl:text> </xsl:text>
									</i>
									<xsl:text> </xsl:text>Delete
								</a>
							</xsl:when>
							<xsl:otherwise>
								<a href="{$appPath}?ewCmd=HideDirItem&amp;DirType=User&amp;id={@id}" class="btn btn-xs btn-danger">
									<i class="fa fa-ban fa-white">
										<xsl:text> </xsl:text>
									</i>
									<xsl:text> </xsl:text>Disable
								</a>
							</xsl:otherwise>
						</xsl:choose>
					</div>
					<h3 class="panel-title">
						User Profile
					</h3>
				</div>
				<div class="panel-body">
					<div class="row">
						<div class="col-md-4">
							<h1>
								<xsl:value-of select="FirstName"/>
								<xsl:text> </xsl:text>
								<xsl:value-of select="LastName"/>
							</h1>
							<a href="mailto:{Email/node()}">
								<xsl:value-of select="Email"/>
							</a><br/>
							<xsl:if test="Position/node()!=''">
								<h3>
									<xsl:value-of select="Position"/>
								</h3>
								<br/>
							</xsl:if>
							<xsl:if test="Company">
								<xsl:choose>
									<xsl:when test="count(Company[@id!=''])=1">Company:</xsl:when>
									<xsl:otherwise>Companies:</xsl:otherwise>
								</xsl:choose>
								<xsl:text> </xsl:text>
								<xsl:apply-templates select="Company[@id!='']" mode="dirProfileLink"/>
							</xsl:if>
							<br/><br/>
							Groups<br/>
							<xsl:for-each select="Group">
								<xsl:apply-templates select="." mode="dirProfileLink"/>
								<xsl:text>, </xsl:text>
							</xsl:for-each>
						</div>
						<div class="col-md-8">

							<xsl:for-each select="Contacts/Contact">
								<!--xsl:apply-templates select="." mode="contactAddressBriefProfile"/-->
								<xsl:apply-templates select="." mode="AdminListContact"/>
								<xsl:if test="position() mod 2=0">
									<div class="terminus">&#160;</div>
								</xsl:if>
							</xsl:for-each>
						</div>
					</div>

				</div>

				<div class="panel panel-default">
					<div class="panel-heading">
						<a href="{$appPath}?ewCmd=EditUserSubscription&amp;id=0&amp;userId={@id}"  class="btn btn-primary btn-sm pull-right">
							<i class="fa fa-plus">&#160;</i>&#160;Add Subscription
						</a>
						<h3 class="panel-title">Subscriptions</h3>
					</div>
					<table class="table">
						<tr>
							<th>Status</th>
							<th>Sub Ref</th>
							<th>Name</th>
							<th>Term</th>
							<th>Start Date</th>
							<th>Expire Date</th>
							<th>&#160;</th>
						</tr>
						<xsl:for-each select="Subscriptions/Subscriptions">
							<tr>
								<td>
									<xsl:value-of select="@nStatus"/>
								</td>
								<td>
									<xsl:value-of select="@nSubKey"/>
								</td>
								<td>
									<xsl:value-of select="Content/Name/node()"/>
								</td>
								<td>
									<xsl:value-of select="@cRenewalStatus"/>
								</td>
								<td>
									<xsl:call-template name="DD_Mon_YYYY">
										<xsl:with-param name="date">
											<xsl:value-of select="@dStartDate"/>
										</xsl:with-param>
										<xsl:with-param name="showTime">false</xsl:with-param>
									</xsl:call-template>

								</td>
								<td>
									<xsl:call-template name="DD_Mon_YYYY">
										<xsl:with-param name="date">
											<xsl:value-of select="@dExpireDate"/>
										</xsl:with-param>
										<xsl:with-param name="showTime">false</xsl:with-param>
									</xsl:call-template>
								</td>
								<td colspan="3">

									<a href="{$appPath}?ewCmd=ManageUserSubscription&amp;id={@nSubKey}"  class="btn btn-primary btn-sm">
										<i class="fa fa-edit">&#160;</i>&#160;Manage
									</a>
									<!--a href="{$appPath}?ewCmd=CancelSubscription&amp;subId={nSubKey/node()}&amp;id={/Page/Request/QueryString/Item[@name='id']/node()}"  class="btn btn-danger btn-sm">
                  <i class="fa fa-edit">&#160;</i>&#160;Cancel
                </a-->

								</td>
							</tr>
							<xsl:if  test="/Page/@ewCmd!='MoveSubscription'">
								<xsl:if test="Subscriptions">
									<tr>
										<td>
											<xsl:apply-templates select="." mode="AdminSubscriptions">
												<xsl:with-param name="GroupID">
													<xsl:value-of select="@nCatKey"/>
												</xsl:with-param>
											</xsl:apply-templates>
										</td>
									</tr>
								</xsl:if>
							</xsl:if>
						</xsl:for-each>
					</table>
				</div>
				<div class="panel panel-default">
					<div class="panel-heading">
						<h4 class="panel-title">Orders</h4>
					</div>
					<table class="panel-body table table-striped">
						<tbody>
							<tr>
								<th>Order Id</th>
								<th>Status</th>
								<th>User</th>
								<th>Email</th>
								<th>Time Placed</th>
								<th>Value</th>
								<th>&#160;</th>
							</tr>
							<xsl:apply-templates select="parent::ContentDetail/Content[@type='order']" mode="ListOrders">
								<xsl:with-param name="startPos"  select="1"/>
								<xsl:with-param name="itemCount" select="'100'"/>
							</xsl:apply-templates>
						</tbody>
					</table>
				</div>
			</div>
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="*" mode="dirProfileLink">
		<a href="?ewCmd=Profile&amp;DirType={name()}&amp;id={@id}">
			<xsl:value-of select="Name/node()"/>
		</a>
	</xsl:template>

	<xsl:template match="Contact" mode="contactAddressBriefProfile">
		<div class="col-md-6">
			<h4>
				<xsl:value-of select="cContactType"/>
			</h4>
			<h3>
				<xsl:value-of select="cContactName"/>
			</h3>
			<p>
				<xsl:if test="cContactCompany/node()!=''">
					<xsl:value-of select="cContactCompany"/>
					<br/>
				</xsl:if>
				<xsl:if test="cContactAddress/node()!=''">
					<xsl:value-of select="cContactAddress"/>
					<br/>
				</xsl:if>
				<xsl:if test="cContactCity/node()!=''">
					<xsl:value-of select="cContactCity"/>
					<br/>
				</xsl:if>
				<xsl:if test="cContactState/node()!=''">
					<xsl:value-of select="cContactState"/>
					<br/>
				</xsl:if>
				<xsl:if test="cContactZip/node()!=''">
					<xsl:value-of select="cContactZip"/>
					<br/>
				</xsl:if>
				<xsl:if test="cContactCountry/node()!=''">
					<xsl:value-of select="cContactCountry"/>
					<br/>
				</xsl:if>
				<xsl:if test="cContactTel/node()!=''">
					<xsl:value-of select="cContactTel"/>
					<br/>
				</xsl:if>
				<xsl:if test="cContactFax/node()!=''">
					<xsl:value-of select="cContactFax"/>
					<br/>
				</xsl:if>
				<xsl:if test="cContactEmail/node()!=''">
					<xsl:value-of select="cContactEmail"/>
					<br/>
				</xsl:if>
			</p>
		</div>
	</xsl:template>


	<xsl:template match="Page[@layout='Profile' and ContentDetail/Company]" mode="Admin">
		<xsl:for-each select="ContentDetail/Company">
			<div id="template_ListDirectory" class="panel panel-default">
				<div class="panel-heading">
					<div class="pull-right">
						<a href="{$appPath}?ewCmd=EditDirItem&amp;DirType=Company&amp;id={@id}" class="btn btn-xs btn-primary">
							<i class="fa fa-user fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Edit
						</a>
						<a href="?ewCmd=ListUsers&amp;parid={@id}" class="btn btn-xs btn-primary">
							<i class="fa fa-user fa-white"> </i> List Users
						</a>

						<xsl:if test="/Page/AdminMenu/MenuItem/MenuItem/MenuItem[@cmd='ListGroups']">
							<a href="{$appPath}?ewCmd=MaintainRelations&amp;type=Group&amp;id={@id}" class="btn btn-xs btn-primary">
								<i class="fa fa-glass fa-white">
									<xsl:text> </xsl:text>
								</i>
								<xsl:text> </xsl:text>Groups
							</a>
							<a href="{$appPath}?ewCmd=DirPermissions&amp;parid={@id}" class="btn btn-xs btn-primary">
								<i class="fa fa-lock fa-white">
									<xsl:text> </xsl:text>
								</i>
								<xsl:text> </xsl:text>Permissions
							</a>
						</xsl:if>
						<xsl:if test="/Page/AdminMenu/descendant-or-self::MenuItem[@cmd='ListUserContacts']">
							<a href="{$appPath}?ewCmd=Profile&amp;DirType=Company&amp;parid={@id}" class="btn btn-xs btn-primary">
								<i class="fa fa-map-marker fa-white">
									<xsl:text> </xsl:text>
								</i>
								<xsl:text> </xsl:text>Addresses
							</a>
						</xsl:if>
						<xsl:choose>
							<xsl:when test="Status='0'">
								<a href="{$appPath}?ewCmd=DeleteDirItem&amp;DirType=User&amp;id={@id}" class="btn btn-xs btn-danger">
									<i class="fa fa-trash-o fa-white">
										<xsl:text> </xsl:text>
									</i>
									<xsl:text> </xsl:text>Delete
								</a>
							</xsl:when>
							<xsl:otherwise>
								<a href="{$appPath}?ewCmd=HideDirItem&amp;DirType=User&amp;id={@id}" class="btn btn-xs btn-danger">
									<i class="fa fa-ban fa-white">
										<xsl:text> </xsl:text>
									</i>
									<xsl:text> </xsl:text>Disable
								</a>
							</xsl:otherwise>
						</xsl:choose>
					</div>
					<h3 class="panel-title">
						Company Profile
					</h3>
				</div>
				<div class="panel-body">
					<div class="row">
						<div class="col-md-4">
							<h1>
								<xsl:value-of select="Name"/>
							</h1>
							<a href="mailto:{Email/node()}">
								<xsl:value-of select="Email"/>
							</a><br/>
							<xsl:if test="Position/node()!=''">
								<h3>
									<xsl:value-of select="Position"/>
								</h3>
								<br/>
							</xsl:if>
							<br/><br/>
							Groups<br/>
							<xsl:for-each select="Group">
								<xsl:apply-templates select="." mode="dirProfileLink"/>
								<xsl:text>, </xsl:text>
							</xsl:for-each>
						</div>
						<div class="col-md-8">
							<a href="{$appPath}?ewCmd=AddUserContact&amp;parid=0&amp;id={/Page/Request/QueryString/Item[@name='id']}" class="btn btn-success btn-sm pull-right">
								<i class="fa fa-plus">&#160;</i>&#160;Add New Address
							</a>
							<xsl:for-each select="Contacts/Contact">

								<xsl:apply-templates select="." mode="AdminListContact"/>
								<xsl:if test="position() mod 2=0">
									<div class="terminus">&#160;</div>
								</xsl:if>
							</xsl:for-each>
						</div>
					</div>
				</div>
			</div>
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="*" mode="dirProfileLink">
		<a href="?ewCmd=Profile&amp;DirType={name()}&amp;id={@id}">
			<xsl:choose>
				<xsl:when test="name()='Company'">
					<i class="fa fa-building-o"> </i>
				</xsl:when>
				<xsl:when test="name()='Group'">
					<i class="fa fa-glass"> </i>
				</xsl:when>
			</xsl:choose>
			<xsl:text> </xsl:text>
			<xsl:value-of select="Name/node()"/>
		</a>
	</xsl:template>

	<xsl:template match="Contact" mode="contactAddressBrief">
		<div class="col-md-6">
			<h4>
				<xsl:value-of select="cContactType"/>
			</h4>
			<h3>
				<xsl:value-of select="cContactName"/>
			</h3>
			<p>
				<xsl:if test="cContactCompany/node()!=''">
					<xsl:value-of select="cContactCompany"/>
					<br/>
				</xsl:if>
				<xsl:if test="cContactAddress/node()!=''">
					<xsl:value-of select="cContactAddress"/>
					<br/>
				</xsl:if>
				<xsl:if test="cContactCity/node()!=''">
					<xsl:value-of select="cContactCity"/>
					<br/>
				</xsl:if>
				<xsl:if test="cContactState/node()!=''">
					<xsl:value-of select="cContactState"/>
					<br/>
				</xsl:if>
				<xsl:if test="cContactZip/node()!=''">
					<xsl:value-of select="cContactZip"/>
					<br/>
				</xsl:if>
				<xsl:if test="cContactCountry/node()!=''">
					<xsl:value-of select="cContactCountry"/>
					<br/>
				</xsl:if>
				<xsl:if test="cContactTel/node()!=''">
					<xsl:value-of select="cContactTel"/>
					<br/>
				</xsl:if>
				<xsl:if test="cContactFax/node()!=''">
					<xsl:value-of select="cContactFax"/>
					<br/>
				</xsl:if>
				<xsl:if test="cContactEmail/node()!=''">
					<xsl:value-of select="cContactEmail"/>
					<br/>
				</xsl:if>
			</p>
		</div>
	</xsl:template>

	<xsl:template match="Page[@layout='ListDirectory']" mode="Admin">
		<xsl:variable name="itemTotal">
			<xsl:choose>
				<xsl:when test="/Page/Request/QueryString/Item[@name='LastNameStarts']">
					<xsl:value-of select="count(ContentDetail/directory/user[starts-with(translate(User/LastName, $alphabetLo, $alphabet),/Page/Request/QueryString/Item[@name='LastNameStarts']/node())])"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="count(ContentDetail/directory/*)"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="itemCount">100</xsl:variable>
		<xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name='startPos']))"/>
		<xsl:variable name="parIdQS">
			<xsl:if test="/Page/Request/QueryString/Item[@name='parid']/node() &gt; 0">
				<xsl:text>&amp;parid=</xsl:text>
				<xsl:value-of select="/Page/Request/QueryString/Item[@name='parid']/node()"/>
			</xsl:if>
		</xsl:variable>

		<div id="template_ListDirectory" class="panel panel-default">
			<div class="panel-heading">
				<h4 class="panel-title">
					<xsl:text>List </xsl:text>
					<xsl:value-of select="ContentDetail/directory/@displayName"/>
					<xsl:text>&#160;</xsl:text>
					<xsl:if test="ContentDetail/directory/@parId">
						<xsl:text>in </xsl:text><xsl:value-of select="ContentDetail/directory/@parType"/>&#160;<xsl:value-of select="ContentDetail/directory/@parName"/>
					</xsl:if>
				</h4>
			</div>
			<div class="panel-body">
				<xsl:apply-templates select="/" mode="alphaStepper">
					<xsl:with-param name="ewCmd" select="/Page/@ewCmd"/>
					<xsl:with-param name="label" select="ContentDetail/directory/@displayName"/>
					<xsl:with-param name="querystringAmendment">
						<xsl:if test="/Page/@parId!=''">
							<xsl:text>parid=</xsl:text>
							<xsl:value-of select="/Page/@parId"/>
						</xsl:if>
					</xsl:with-param>
				</xsl:apply-templates>
				<div class="row">
					<div class="col-md-8">
						<a href="{$appPath}?ewCmd=EditDirItem&amp;DirType={ContentDetail/directory/@itemType}&amp;parid={/Page/@parId}" class="btn btn-sm btn-success">
							<i class="fa fa-plus fa-white">
								<xsl:text> </xsl:text>
							</i>
							<xsl:text> </xsl:text>
							Add New <xsl:value-of select="ContentDetail/directory/@itemType"/>&#160;<xsl:if test="ContentDetail/directory/@parId">
								to <xsl:value-of select="ContentDetail/directory/@parType"/>&#160;<xsl:value-of select="ContentDetail/directory/@parName"/>
							</xsl:if>
						</a>
						<xsl:if test="ContentDetail/directory/@itemType = 'User'">
							<xsl:choose>
								<xsl:when test="/Page/Request/QueryString/Item[@name='status']">
									<a href="{$appPath}?ewCmd=ListUsers&amp;parid={/Page/Request/QueryString/Item[@name='parid']/node()}" class="btn btn-sm btn-primary" >
										<i class="fa fa-user fa-white">
											<xsl:text> </xsl:text>
										</i>
										<xsl:text> </xsl:text>List All Users
									</a>
								</xsl:when>
								<xsl:otherwise>
									<a href="{$appPath}?ewCmd=ListUsers{$parIdQS}&amp;status=1" class="btn btn-sm btn-primary" >
										<i class="fa fa-check-circle-o fa-white">
											<xsl:text> </xsl:text>
										</i>
										<xsl:text> </xsl:text>
										<xsl:text>List </xsl:text>
										<xsl:value-of select="count(ContentDetail/directory/user[Status = '1' or Status = '-1'] )"/>
										<xsl:text> Active Users</xsl:text>
									</a>
									<a href="{$appPath}?ewCmd=ListUsers{$parIdQS}&amp;status=0" class="btn btn-sm btn-primary" >
										<i class="fa fa-ban fa-white">
											<xsl:text> </xsl:text>
										</i>
										<xsl:text> </xsl:text>
										<xsl:text>List </xsl:text>
										<xsl:value-of select="count(ContentDetail/directory/user[Status != '1' and Status != '-1'])"/>
										<xsl:text> Inactive Users</xsl:text>
									</a>
									<xsl:if test="count(ContentDetail/directory/user[Status = '3']) &gt; 0">
										<a href="{$appPath}?ewCmd=ListUsers{$parIdQS}&amp;status=3" class="btn btn-sm btn-info" >
											<i class="fa fa-exclaimation-sign fa-white">
												<xsl:text> </xsl:text>
											</i>
											<xsl:text> </xsl:text>
											<xsl:text>List </xsl:text>
											<xsl:value-of select="count(ContentDetail/directory/user[Status = '3'])"/>
											<xsl:text> Awaiting Approval Users</xsl:text>
										</a>
									</xsl:if>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:if>
						<a href="/ewcommon/tools/excel.ashx?{/Page/Request/ServerVariables/Item[@name='QUERY_STRING']/node()}" class="btn btn-sm btn-primary" target="_new">
							<i class="fa fa-th-list fa-white">
								<xsl:text> </xsl:text>
							</i>
							<xsl:text> </xsl:text>
							Excel Download
						</a>

						<xsl:if test="ContentDetail/directory/@parType='Group' and ContentDetail/directory/@parId and not(ContentDetail/directory/@itemType='User')">
							<a href="{$appPath}?ewCmd=MaintainRelations&amp;type={ContentDetail/directory/@itemType}&amp;id={/Page/@parId}&amp;relateAs=children" class="btn btn-sm btn-primary principle">
								Edit <xsl:value-of select="ContentDetail/directory/@itemType"/>&#160;members of <xsl:value-of select="ContentDetail/directory/@parType"/>&#160;<xsl:value-of select="ContentDetail/directory/@parName"/>
							</a>
						</xsl:if>
					</div>
					<xsl:if test="ContentDetail/directory/@parType='User'">
						<form action="?ewCmd=ListUsers" method="post" id="userSearch" class="col-md-4">
							<div class="input-group">
								<input type="text" name="search" value="{$page/Request/Form/Item[@name='search']}" class="form-control"/>
								<span class="input-group-btn">
									<button type="submit" name="previous" value="Search" class="btn btn-success principle">
										User Search
										<xsl:text> </xsl:text>
										<i class="fa fa-search fa-white">
											<xsl:text> </xsl:text>
										</i>
									</button>
								</span>
							</div>
						</form>
					</xsl:if>
					<xsl:if test="ContentDetail/directory/@parType='Company'">
						<form action="?ewCmd=ListCompanies" method="post" id="companySearch" class="col-md-4">
							<div class="input-group">
								<input type="text" name="search" value="{$page/Request/Form/Item[@name='search']}" class="form-control"/>
								<span class="input-group-btn">
									<button type="submit" name="previous" value="Search" class="btn btn-success principle">
										Company Search
										<xsl:text> </xsl:text>
										<i class="fa fa-search fa-white">
											<xsl:text> </xsl:text>
										</i>
									</button>
								</span>
							</div>
						</form>
					</xsl:if>
				</div>
				<div class="row">
					<div class="col-md-12">
						<div class="pull-right clearfix">
							<xsl:if test="$itemTotal &gt; $itemCount">
								<xsl:apply-templates select="/" mode="adminStepper">
									<xsl:with-param name="itemTotal" select="$itemTotal"/>
									<xsl:with-param name="itemCount" select="$itemCount"/>
									<xsl:with-param name="startPos" select="$startPos"/>
									<xsl:with-param name="path">
										<xsl:text>?ewCmd=List</xsl:text>
										<xsl:value-of select="ContentDetail/directory/@displayName"/>
										<xsl:if test="/Page/Request/QueryString/Item[@name='LastNameStarts']">
											<xsl:text>&amp;LastNameStarts=</xsl:text>
											<xsl:value-of select="/Page/Request/QueryString/Item[@name='LastNameStarts']/node()"/>
										</xsl:if>
										<xsl:if test="/Page/Request/Form/Item[@name='search']">
											<xsl:text>&amp;search=</xsl:text>
											<xsl:value-of select="/Page/Request/Form/Item[@name='search']/node()"/>
										</xsl:if>
										<xsl:if test="/Page/Request/QueryString/Item[@name='search']">
											<xsl:text>&amp;search=</xsl:text>
											<xsl:value-of select="/Page/Request/QueryString/Item[@name='search']/node()"/>
										</xsl:if>
										<xsl:text>&amp;parid=</xsl:text>
										<xsl:value-of select="/Page/Request/QueryString/Item[@name='parid']/node()"/>
										<xsl:if test="/Page/Request/QueryString/Item[@name='status']">
											<xsl:text>&amp;status=</xsl:text>
											<xsl:value-of select="/Page/Request/QueryString/Item[@name='status']/node()"/>
										</xsl:if>
									</xsl:with-param>
									<xsl:with-param name="itemName" select="ContentDetail/directory/@displayName"/>
								</xsl:apply-templates>
							</xsl:if>
						</div>
					</div>
				</div>
			</div>
			<table cellpadding="0" cellspacing="0" class="table table-striped panel-body">
				<thead>
					<tr>
						<th>Status</th>
						<xsl:choose>
							<xsl:when test="ContentDetail/directory/@itemType = 'User'">
								<th>Name</th>
								<th>Username</th>
								<th>
									<!--Member Of-->
								</th>
								<!--<th>Roles</th>-->
							</xsl:when>
							<xsl:otherwise>
								<th>
									<xsl:value-of select="ContentDetail/directory/@itemType"/>
								</th>
								<th>
									<xsl:value-of select="ContentDetail/directory/@parType"/>
								</th>
							</xsl:otherwise>
						</xsl:choose>
						<th>
							<div align="right" class="btn-group">


							</div>
						</th>
					</tr>
				</thead>
				<tbody>
					<xsl:choose>
						<xsl:when test="/Page/Request/QueryString/Item[@name='LastNameStarts']">
							<xsl:choose>
								<xsl:when test="ContentDetail/directory/@itemType='User'">
									<xsl:apply-templates select="ContentDetail/directory/*[starts-with(translate(User/LastName, $alphabetLo, $alphabet),/Page/Request/QueryString/Item[@name='LastNameStarts']/node())]" mode="list">
										<xsl:sort select="User/LastName"/>
										<xsl:with-param name="startPos" select="$startPos"/>
										<xsl:with-param name="noOnPage" select="$itemCount"/>
									</xsl:apply-templates>
								</xsl:when>
								<xsl:otherwise>
									<xsl:apply-templates select="ContentDetail/directory/*[starts-with(translate(Name, $alphabetLo, $alphabet),/Page/Request/QueryString/Item[@name='LastNameStarts']/node())]" mode="list">
										<xsl:sort select="@name"/>
										<xsl:with-param name="startPos" select="$startPos"/>
										<xsl:with-param name="noOnPage" select="$itemCount"/>
									</xsl:apply-templates>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:when>
						<xsl:otherwise>
							<xsl:apply-templates select="ContentDetail/directory/*" mode="list">
								<xsl:sort select="name"/>
								<xsl:with-param name="startPos" select="$startPos"/>
								<xsl:with-param name="noOnPage" select="$itemCount"/>
							</xsl:apply-templates>
						</xsl:otherwise>
					</xsl:choose>
				</tbody>
			</table>
			<div class="panel-body">
				<div class="row">
					<div class="col-md-12">
						<div class="pull-right clearfix">
							<xsl:if test="$itemTotal &gt; $itemCount">
								<xsl:apply-templates select="/" mode="adminStepper">
									<xsl:with-param name="itemTotal" select="$itemTotal"/>
									<xsl:with-param name="itemCount" select="$itemCount"/>
									<xsl:with-param name="startPos" select="$startPos"/>
									<xsl:with-param name="path">
										<xsl:text>?ewCmd=List</xsl:text>
										<xsl:value-of select="ContentDetail/directory/@displayName"/>
										<xsl:if test="/Page/Request/QueryString/Item[@name='LastNameStarts']">
											<xsl:text>&amp;LastNameStarts=</xsl:text>
											<xsl:value-of select="/Page/Request/QueryString/Item[@name='LastNameStarts']/node()"/>
										</xsl:if>
										<xsl:if test="/Page/Request/Form/Item[@name='search']">
											<xsl:text>&amp;search=</xsl:text>
											<xsl:value-of select="/Page/Request/Form/Item[@name='search']/node()"/>
										</xsl:if>
										<xsl:if test="/Page/Request/QueryString/Item[@name='search']">
											<xsl:text>&amp;search=</xsl:text>
											<xsl:value-of select="/Page/Request/QueryString/Item[@name='search']/node()"/>
										</xsl:if>
										<xsl:text>&amp;parid=</xsl:text>
										<xsl:value-of select="/Page/Request/QueryString/Item[@name='parid']/node()"/>
										<xsl:if test="/Page/Request/QueryString/Item[@name='status']">
											<xsl:text>&amp;status=</xsl:text>
											<xsl:value-of select="/Page/Request/QueryString/Item[@name='status']/node()"/>
										</xsl:if>
									</xsl:with-param>
									<xsl:with-param name="itemName" select="ContentDetail/directory/@displayName"/>
								</xsl:apply-templates>
							</xsl:if>
						</div>
					</div>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="user" mode="list">
		<xsl:param name="startPos"/>
		<xsl:param name="noOnPage"/>
		<xsl:if test="position() > $startPos and position() &lt;= ($startPos + $noOnPage)">
			<tr class="midRow">
				<xsl:apply-templates select="Status" mode="reportCell"/>
				<xsl:apply-templates select="User" mode="reportCell"/>
				<xsl:apply-templates select="Username" mode="reportCell"/>
				<td>
					<xsl:choose>
						<xsl:when test="Companies/node()!=''">
							<xsl:apply-templates select="Companies/node()" mode="cleanXhtml"/>
						</xsl:when>
						<xsl:otherwise>
							&#160;
						</xsl:otherwise>
					</xsl:choose>
				</td>
				<td>
					<a href="{$appPath}?ewCmd=EditDirItem&amp;DirType=User&amp;id={@id}" class="btn btn-xs btn-primary">
						<i class="fa fa-user fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Edit
					</a>
					<a href="{$appPath}?ewCmd=ResetUserPwd&amp;id={@id}" class="btn btn-xs btn-success">
						<i class="fa fa-repeat fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Reset Pwd
					</a>
					<a href="{$appPath}?ewCmd=PreviewOn&amp;PreviewUser={@id}" class="btn btn-xs btn-success">
						<i class="fa fa-user-secret fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Impersonate
					</a>
					<a href="{$appPath}?ewCmd=MaintainRelations&amp;type=Role&amp;id={@id}" class="btn btn-xs btn-primary">
						<i class="fa fa-cog fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Roles
					</a>
					<xsl:if test="/Page/AdminMenu/MenuItem/MenuItem/MenuItem[@cmd='ListGroups']">
						<a href="{$appPath}?ewCmd=MaintainRelations&amp;type=Group&amp;id={@id}" class="btn btn-xs btn-primary">
							<i class="fa fa-glass fa-white">
								<xsl:text> </xsl:text>
							</i>
							<xsl:text> </xsl:text>Groups
						</a>
						<a href="{$appPath}?ewCmd=DirPermissions&amp;parid={@id}" class="btn btn-xs btn-primary">
							<i class="fa fa-lock fa-white">
								<xsl:text> </xsl:text>
							</i>
							<xsl:text> </xsl:text>Permissions
						</a>
					</xsl:if>
					<xsl:if test="/Page/AdminMenu/MenuItem/MenuItem/MenuItem[@cmd='ListCompanies']">
						<a href="{$appPath}?ewCmd=MaintainRelations&amp;type=Company&amp;id={@id}" class="btn btn-xs btn-primary">
							<i class="fa fa-building-o fa-white">
								<xsl:text> </xsl:text>
							</i>
							<xsl:text> </xsl:text>Companies
						</a>

						<a href="{$appPath}?ewCmd=MaintainRelations&amp;type=Department&amp;id={@id}" class="btn btn-xs btn-primary">
							<i class="fa fa-group fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Dept
						</a>
					</xsl:if>

					<xsl:if test="/Page[@userIntegrations='true']">
						<a href="{$appPath}?ewCmd=UserIntegrations&amp;dirId={@id}" class="btn btn-xs btn-primary">
							<i class="fa fa-random fa-white">
								<xsl:text> </xsl:text>
							</i>
							<xsl:text> </xsl:text>Integrations
						</a>
					</xsl:if>
					<xsl:if test="/Page/AdminMenu/descendant-or-self::MenuItem[@cmd='EditUserContact']">
						<a href="{$appPath}?ewCmd=ListUserContacts&amp;parid={@id}" class="btn btn-xs btn-primary">
							<i class="fa fa-map-marker fa-white">
								<xsl:text> </xsl:text>
							</i>
							<xsl:text> </xsl:text>Addresses
						</a>
					</xsl:if>
					<xsl:if test="/Page/AdminMenu/MenuItem/MenuItem/MenuItem[@cmd='MemberActivity']">
						<a href="{$appPath}?ewCmd=MemberActivity&amp;UserId={@id}" class="btn btn-xs btn-primary">
							<i class="fa fa-signal fa-white">
								<xsl:text> </xsl:text>
							</i>
							<xsl:text> </xsl:text>Activity
						</a>
					</xsl:if>
					<xsl:apply-templates select="." mode="bespokeUserButtons"/>
					<xsl:choose>
						<xsl:when test="Status='0'">
							<a href="{$appPath}?ewCmd=DeleteDirItem&amp;DirType=User&amp;id={@id}" class="btn btn-xs btn-danger">
								<i class="fa fa-trash-o fa-white">
									<xsl:text> </xsl:text>
								</i>
								<xsl:text> </xsl:text>Delete
							</a>
						</xsl:when>
						<xsl:otherwise>
							<a href="{$appPath}?ewCmd=HideDirItem&amp;DirType=User&amp;id={@id}" class="btn btn-xs btn-danger">
								<i class="fa fa-ban fa-white">
									<xsl:text> </xsl:text>
								</i>
								<xsl:text> </xsl:text>Disable
							</a>
						</xsl:otherwise>
					</xsl:choose>
				</td>
			</tr>
		</xsl:if>
	</xsl:template>

	<xsl:template match="user" mode="bespokeUserButtons">

	</xsl:template>

	<xsl:template match="company" mode="list">
		<xsl:param name="startPos"/>
		<xsl:param name="noOnPage"/>
		<xsl:if test="position() > $startPos and position() &lt;= ($startPos + $noOnPage)">
			<tr onmouseover="this.className='rowOver'" onmouseout="this.className=''">
				<xsl:apply-templates select="Status" mode="reportCell"/>
				<xsl:apply-templates select="Name" mode="reportCell"/>
				<xsl:apply-templates select="Company/Website" mode="reportCell"/>
				<td class="btn-group">
					<a href="{$appPath}?ewCmd=EditDirItem&amp;DirType=Company&amp;id={@id}" class="btn btn-xs btn-primary">
						<i class="fa fa-pencil fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Edit
					</a>&#160;
					<a href="{$appPath}?ewCmd=ListUsers&amp;parid={@id}" class="btn btn-xs btn-primary">
						<i class="fa fa-user fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>List Users
					</a>&#160;

					<xsl:if test="/Page/AdminMenu/descendant-or-self::MenuItem[@cmd='ListContacts']">
						<a href="{$appPath}?ewCmd=ListContacts&amp;parid={@id}" class="btn btn-xs btn-primary">
							<i class="fa fa-map-marker fa-white">
								<xsl:text> </xsl:text>
							</i>
							<xsl:text> </xsl:text>Addresses
						</a>
					</xsl:if>


					<a href="{$appPath}?ewCmd=ListDepartments&amp;parid={@id}" class="btn btn-xs btn-primary">
						<i class="fa fa-users fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>List Depts
					</a>&#160;
					<a href="{$appPath}?ewCmd=ListGroups&amp;parid={@id}" class="btn btn-xs btn-primary">
						<i class="fa fa-glass fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>List Groups
					</a>&#160;
					<xsl:if test="/Page/AdminMenu/descendant-or-self::MenuItem[@cmd='EditDirContact']">
						<a href="{$appPath}?ewCmd=ListDirContacts&amp;parid={@id}" class="btn btn-xs btn-primary">
							<i class="fa fa-map-marker fa-white">
								<xsl:text> </xsl:text>
							</i>
							<xsl:text> </xsl:text>Addresses
						</a>
					</xsl:if>&#160;
					<a href="{$appPath}?ewCmd=MaintainRelations&amp;type=Group&amp;id={@id}" class="btn btn-xs btn-primary">Global Groups</a>&#160;
					<a href="{$appPath}?ewCmd=DirPermissions&amp;parid={@id}" class="btn btn-xs btn-primary">
						<i class="fa fa-lock fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Permissions
					</a>&#160;
					<xsl:if test="Status='0'">
						<a href="{$appPath}?ewCmd=DeleteDirItem&amp;DirType=Company&amp;id={@id}" class="btn btn-xs btn-danger">
							<i class="fa fa-trash-o fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Delete
						</a>
					</xsl:if>
				</td>
			</tr>
		</xsl:if>
	</xsl:template>

	<xsl:template match="department" mode="list">
		<xsl:param name="startPos"/>
		<xsl:param name="noOnPage"/>
		<xsl:if test="position() > $startPos and position() &lt;= ($startPos + $noOnPage)">
			<tr onmouseover="this.className='rowOver'" onmouseout="this.className=''">
				<xsl:apply-templates select="*" mode="reportCell"/>
				<td class="btn-group">
					<a href="{$appPath}?ewCmd=EditDirItem&amp;DirType=Department&amp;id={@id}" class="btn btn-xs btn-primary">
						<i class="fa fa-pencil fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Edit
					</a>&#160;
					<a href="{$appPath}?ewCmd=ListUsers&amp;parid={@id}"  class="btn btn-xs btn-primary">
						<i class="fa fa-users fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>List Users
					</a>&#160;
					<a href="{$appPath}?ewCmd=MaintainRelations&amp;type=Group&amp;id={@id}"  class="btn btn-xs btn-primary">
						<i class="fa fa-globe fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Global Groups
					</a>&#160;
					<a href="{$appPath}?ewCmd=DirPermissions&amp;parid={@id}"  class="btn btn-xs btn-primary">
						<i class="fa fa-lock fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Permissions
					</a>&#160;
					<!--a href="{$appPath}?ewCmd=Messages&amp;id={@id}" class="adminButton">Send Message</a-->&#160;
					<xsl:if test="Status='0'">
						<a href="{$appPath}?ewCmd=DeleteDirItem&amp;DirType=Department&amp;id={@id}" class="btn btn-xs btn-danger">
							<i class="fa fa-trash-o fa-white">
								<xsl:text> </xsl:text>
							</i>Delete
						</a>
					</xsl:if>
				</td>
			</tr>
		</xsl:if>
	</xsl:template>

	<xsl:template match="group" mode="list">
		<xsl:param name="startPos"/>
		<xsl:param name="noOnPage"/>
		<xsl:if test="position() > $startPos and position() &lt;= ($startPos + $noOnPage)">
			<tr onmouseover="this.className='rowOver'" onmouseout="this.className=''">
				<xsl:apply-templates select="*" mode="reportCell"/>
				<td>
					<a href="{$appPath}?ewCmd=EditDirItem&amp;DirType=Group&amp;id={@id}" class="btn btn-xs btn-primary">
						<i class="fa fa-pencil fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Edit
					</a>&#160;
					<a href="{$appPath}?ewCmd=DirMemberships&amp;type=Group&amp;id={@id}&amp;childTypes=User" class="btn btn-xs btn-primary">
						<i class="fa fa-users fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Manage Members
					</a>&#160;
					<a href="{$appPath}?ewCmd=ListUsers&amp;parid={@id}" class="btn btn-xs btn-primary">
						<i class="fa fa-user fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>List Members
					</a>&#160;
					<a href="{$appPath}?ewCmd=DirPermissions&amp;parid={@id}" class="btn btn-xs btn-primary">
						<i class="fa fa-lock fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Member Permissions
					</a>&#160;
					<xsl:choose>
						<xsl:when test="Status='0'">
							<a href="{$appPath}?ewCmd=DeleteDirItem&amp;DirType=Group&amp;id={@id}" class="btn btn-xs btn-danger">
								<i class="fa fa-trash-o fa-white">
									<xsl:text> </xsl:text>
								</i><xsl:text> </xsl:text>Delete
							</a>
							<a href="{$appPath}?ewCmd=showDirItem&amp;DirType=Group&amp;id={@id}" class="btn btn-xs btn-danger">
								<i class="fa fa-trash-o fa-white">
									<xsl:text> </xsl:text>
								</i><xsl:text> </xsl:text>Show
							</a>
						</xsl:when>
						<xsl:otherwise>
							<a href="{$appPath}?ewCmd=HideDirItem&amp;DirType=Group&amp;id={@id}" class="btn btn-xs btn-danger">
								<i class="fa fa-ban fa-white">
									<xsl:text> </xsl:text>
								</i><xsl:text> </xsl:text>Disable
							</a>
						</xsl:otherwise>
					</xsl:choose>
				</td>
			</tr>
		</xsl:if>
	</xsl:template>

	<xsl:template match="role" mode="list">
		<xsl:param name="startPos"/>
		<xsl:param name="noOnPage"/>
		<xsl:if test="position() > $startPos and position() &lt;= ($startPos + $noOnPage)">
			<tr onmouseover="this.className='rowOver'" onmouseout="this.className=''">
				<xsl:apply-templates select="*" mode="reportCell"/>
				<td>
					<a href="{$appPath}?ewCmd=EditRole&amp;DirType=Role&amp;id={@id}" class="btn btn-xs btn-primary">
						<i class="fa fa-pencil fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Edit
					</a>&#160;
					<a href="{$appPath}?ewCmd=ListUsers&amp;parid={@id}" class="btn btn-xs btn-primary">
						<i class="fa fa-user fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>List Members
					</a>&#160;
					<a href="{$appPath}?ewCmd=DirPermissions&amp;parid={@id}" class="btn btn-xs btn-primary">
						<i class="fa fa-lock fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Member Permissions
					</a>&#160;
					<xsl:choose>
						<xsl:when test="Status='0'">
							<a href="{$appPath}?ewCmd=DeleteDirItem&amp;DirType=Role&amp;id={@id}" class="btn btn-xs btn-danger">
								<i class="fa fa-trash-o fa-white">
									<xsl:text> </xsl:text>
								</i><xsl:text> </xsl:text>Delete
							</a>
						</xsl:when>
						<xsl:otherwise>
							<a href="{$appPath}?ewCmd=HideDirItem&amp;DirType=Role&amp;id={@id}" class="btn btn-xs btn-danger">
								<i class="fa fa-trash-o fa-white">
									<xsl:text> </xsl:text>
								</i><xsl:text> </xsl:text>Disable
							</a>
						</xsl:otherwise>
					</xsl:choose>
				</td>
			</tr>
		</xsl:if>
	</xsl:template>

	<xsl:template match="Page[@layout='ListDirRelations']" mode="Admin">
		<xsl:variable name="itemTotal" select="count(ContentDetail/directory/*)"/>
		<xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name='startPos']))"/>
		<div class="panel panel-default" id="template_ListUsers">
			<div class="panel-heading">
				<h4 class="panel-title">
					Add&#160;<xsl:value-of select="ContentDetail/directory/@childType"/>&#160;<strong>
						<xsl:value-of select="ContentDetail/directory/@childName"/>
					</strong> to <strong>
						<xsl:value-of select="ContentDetail/directory/@displayName"/>
					</strong>
				</h4>
			</div>

			<!--h4>
            Listing <xsl:value-of select="count(ContentDetail/directory/*)"/> Active <xsl:value-of select="ContentDetail/directory/@displayName"/>
          </h4-->
			<form action="?ewCmd=SaveDirectoryRelations" method="post">
				<input name="childId" type="hidden" value="{ContentDetail/directory/@childId}"/>
				<input name="parentList" type="hidden">
					<xsl:attribute name="value">
						<xsl:for-each select="ContentDetail/directory/*">
							<xsl:value-of select="@id"/>,
						</xsl:for-each>
					</xsl:attribute>
				</input>
				<xsl:choose>
					<xsl:when test="/Page/Request/QueryString/Item[@name='relateAs']/node()!=''">
						<label for="relateAs" >Relate As</label>
						<input name="relateAs" id="relateAs" type="text" value="{/Page/Request/QueryString/Item[@name='relateAs']/node()}"/>
					</xsl:when>
					<xsl:otherwise>
						<input name="relateAs" id="relateAs" type="hidden"/>
					</xsl:otherwise>
				</xsl:choose>

				<table cellpadding="0" cellspacing="0" class="table table-striped panel-body">
					<tbody>
						<tr>
							<th>Status</th>
							<xsl:choose>
								<xsl:when test="ContentDetail/directory/@itemType = 'User'">
									<th>Username</th>
								</xsl:when>
								<xsl:otherwise>
									<th>
										<xsl:value-of select="ContentDetail/directory/@itemType"/>
									</th>
								</xsl:otherwise>
							</xsl:choose>
							<th>
								<button type="submit" name="Save" class="principle btn btn-success">
									<i class="fa fa-check">
										<xsl:text> </xsl:text>
									</i>
									Save <xsl:value-of select="ContentDetail/directory/@displayName"/>
								</button>
							</th>
						</tr>
						<xsl:apply-templates select="ContentDetail/directory/*" mode="relation">
							<xsl:sort select="name"/>
						</xsl:apply-templates>
						<tr>
							<th>&#160;</th>
							<th>&#160;</th>
							<th>
								<button type="submit" name="Save" class="principle btn btn-success">
									<i class="fa fa-check">
										<xsl:text> </xsl:text>
									</i>
									Save <xsl:value-of select="ContentDetail/directory/@displayName"/>
								</button>
							</th>
						</tr>
					</tbody>
				</table>
			</form>
		</div>
	</xsl:template>

	<xsl:template match="*" mode="relation">
		<tr>
			<td>
				<xsl:call-template name="status_legend">
					<xsl:with-param name="status" select="@status"/>
				</xsl:call-template>
			</td>
			<xsl:if test="@Company">
				<td>
					<xsl:value-of select="@Company"/>
				</td>
			</xsl:if>
			<td>
				<xsl:value-of select="@name"/>
			</td>
			<td>
				<input type="checkbox" name="rel_{@id}">
					<xsl:if test="@related&gt;0">
						<xsl:attribute name="checked">checked</xsl:attribute>
					</xsl:if>
				</input>
			</td>
		</tr>
	</xsl:template>
	<!--BJR EditContacts-->

	<xsl:template match="Page[@layout='ListUserContacts' or @layout='ListDirContacts']" mode="adminPageHeader">
		<h1 class="page-header">
			<i class="fa {/Page/AdminMenu/descendant-or-self::MenuItem[@cmd=/Page/@ewCmd]/@icon}">&#160;</i>&#160;
			<xsl:value-of select="ContentDetail/*/Name/node()"/> - Addresses
		</h1>
	</xsl:template>

	<xsl:template match="Page[@layout='ListUserContacts' or @layout='ListDirContacts']" mode="Admin">
		<xsl:variable name="dirType">
			<xsl:choose>
				<xsl:when test="@layout='ListDirContacts'">Dir</xsl:when>
				<xsl:otherwise>User</xsl:otherwise>
			</xsl:choose>
		</xsl:variable >


		<div class="row" >
			<div class="col-md-12">
				<div class="row" id="template_ListUsersContacts">
					<div class="headerButtons col-md-12 clearfix">
						<a href="{$appPath}?ewCmd=Add{$dirType}Contact&amp;parid={/Page/Request/QueryString/Item[@name='parid']}" class="btn btn-success btn-sm pull-right">
							<i class="fa fa-plus">&#160;</i>&#160;Add New Address
						</a>
					</div>
					<xsl:apply-templates select="/Page/ContentDetail/*/Contacts/Contact" mode="AdminListContact">
						<xsl:sort select="cContactType" data-type="text" order="ascending"/>
					</xsl:apply-templates>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Contact" mode="AdminListContact">
		<xsl:variable name="dirid" select="/Page/Request/QueryString/Item[@name='id']"/>
		<div class="col-md-6">
			<div class="panel panel-default">
				<div class="panel-heading">

					<!--<a href="{$appPath}?ewCmd=EditUserContact&amp;parid={$dirid}&amp;id={nContactKey}" class="btn btn-primary btn-sm pull-right">-->
					<a href="{$appPath}?ewCmd=EditUserContact&amp;parid={nContactKey}&amp;id={$dirid}" class="btn btn-primary btn-sm pull-right">
						<i class="fa fa-edit">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>
						Edit
					</a>
					<a href="{$appPath}?ewCmd=DeleteUserContact&amp;parid={nContactKey}&amp;id={$dirid}" class="btn btn-danger btn-sm pull-right">
						<i class="fa fa-trash-o">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Delete
					</a>

					<h3 class="panel-title">
						<xsl:value-of select="cContactType"/>
					</h3>
				</div>

				<table cellpadding="0" cellspacing="0" class="table panel-body">
					<tr>
						<th>Name</th>
						<td>
							<xsl:value-of select="cContactName"/>
						</td>
						<th>Company</th>
						<td>
							<xsl:value-of select="cContactCompany"/>
						</td>
					</tr>
					<tr>
						<th>Address</th>
						<td>
							<xsl:value-of select="cContactAddress"/>
						</td>
						<th>City</th>
						<td>
							<xsl:value-of select="cContactCity"/>
						</td>
					</tr>
					<tr>
						<th>State</th>
						<td>
							<xsl:value-of select="cContactState"/>
						</td>
						<th>Zip</th>
						<td>
							<xsl:value-of select="cContactZip"/>
						</td>
					</tr>
					<tr>
						<th>Country</th>
						<td colspan="3">
							<xsl:value-of select="cContactCountry"/>
						</td>
					</tr>
					<tr>
						<th>Tel</th>
						<td>
							<xsl:value-of select="cContactTel"/>
						</td>
						<th>Fax</th>
						<td>
							<xsl:value-of select="cContactFax"/>
						</td>
					</tr>
					<tr>
						<th>Email</th>
						<td colspan="3">
							<xsl:value-of select="cContactEmail"/>
						</td>
					</tr>

				</table>
			</div>
		</div>
	</xsl:template>
	<!-- -->

	<xsl:template match="Page[@layout='AddUserContact'] | Page[@layout='EditUserContact'] | Page[@layout='AddDirContact'] | Page[@layout='EditDirContact'] | Page[@layout='EditOrderContact']" mode="Admin">
		<div class="adminTemplate" id="template_AdminXForm">
			<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
		</div>
	</xsl:template>

	<!--   ##################  List Orders  ##############################   -->
	<!-- -->
	<xsl:template match="Page[@layout='Orders']" mode="Admin">
		<xsl:variable name="startPos" select="number(concat(0,/Page/Request/QueryString/Item[@name='startPos']))"/>
		<xsl:variable name="itemCount" select="'100'"/>
		<xsl:variable name="total" select="ContentDetail/@total"/>
		<xsl:variable name="queryString">
			<xsl:text>?</xsl:text>
			<xsl:call-template name="getQString"/>
		</xsl:variable>
		<xsl:variable name="ewCmd" select="/Page/Request/QueryString/Item[@name='ewCmd']"/>
		<xsl:variable name="title">
			<xsl:choose>
				<xsl:when test="@ewCmd='Orders'">New Sales</xsl:when>
				<xsl:when test="@ewCmd='OrdersInProgress'">In Progress</xsl:when>
				<xsl:when test="@ewCmd='OrdersAwaitingPayment'">Awaiting Payment</xsl:when>
				<xsl:when test="@ewCmd='OrdersShipped'">Shipped</xsl:when>
				<xsl:when test="@ewCmd='OrdersFailed'">Failed Transactions</xsl:when>
				<xsl:when test="@ewCmd='OrdersDeposit'">Deposit Paid</xsl:when>
				<xsl:when test="@ewCmd='OrdersHistory'">Full Order History</xsl:when>
			</xsl:choose>
		</xsl:variable>
		<div class="row" >
			<div class="col-md-12">
				<div class="panel panel-default">
					<xsl:choose>
						<xsl:when test="ContentDetail/Content[@type='order'] and not(/Page/Request/QueryString/Item[@name='ewCmd2'])">
							<form action="{$appPath}" method="get" class="ewXform">
								<div class="panel-heading">

									<input type="hidden" name="ewCmd" value="BulkCartAction"/>
									<input type="hidden" name="pgid" value="{$page/@id}"/>
									<div class="panel-heading-buttons col-md-3 pull-right">
										<xsl:if test="@ewCmd='Orders'">
											<div class="form-group bulk-action">
												<div class="input-group">
													<label class="input-group-addon">Bulk Action</label>
													<select class="form-control" name="BulkAction" id="BulkAction">
														<option value="Print">Print Delivery</option>
														<option value="SetInProgress">Move to In Progress</option>
														<option value="SetShipped">Move to Shipped</option>
													</select>
													<span class="input-group-btn">
														<button type="submit" class="btn btn-primary">Go</button>
													</span>
												</div>
											</div>
										</xsl:if>
									</div>
									<div class="pull-right">
										<xsl:apply-templates select="/" mode="adminStepper">
											<xsl:with-param name="itemCount" select="'100'"/>
											<xsl:with-param name="itemTotal" select="$total"/>
											<xsl:with-param name="startPos" select="$startPos"/>
											<xsl:with-param name="path" select="$queryString"/>
											<xsl:with-param name="itemName" select="$title"/>
										</xsl:apply-templates>
									</div>
									<h3 class="panel-title">
										<xsl:value-of select="$title"/>
									</h3>

									<div class="terminus">&#160;</div>
								</div>
								<table class="panel-body table table-striped">
									<tbody>
										<tr>
											<th>Order Id</th>
											<th>Status</th>
											<th>Customer Name</th>
											<th>Email</th>
											<th>Time Placed</th>
											<th>Value</th>
											<xsl:if test="$title='Deposit Paid'">

												<th>Paid</th>
												<th>Outstanding</th>
												<th>Due by</th>
											</xsl:if>
											<th>&#160;</th>
											<th>
												<a href="" class="btn btn-default">All</a>
											</th>
										</tr>
										<xsl:apply-templates select="ContentDetail/Content[@type='order']" mode="ListOrders">
											<xsl:with-param name="startPos"  select="$startPos"/>
											<xsl:with-param name="itemCount" select="'100'"/>
										</xsl:apply-templates>
									</tbody>
								</table>

							</form>
							<div class="panel-footer">
								<div class="pull-right">
									<xsl:apply-templates select="/" mode="adminStepper">
										<xsl:with-param name="itemCount" select="'100'"/>
										<xsl:with-param name="itemTotal" select="$total"/>
										<xsl:with-param name="startPos" select="$startPos"/>
										<xsl:with-param name="path" select="$queryString"/>
										<xsl:with-param name="itemName" select="$title"/>
									</xsl:apply-templates>
								</div>
								<div class="terminus">
									<xsl:text> </xsl:text>
								</div>
							</div>
						</xsl:when>
						<xsl:otherwise>
							<xsl:choose>
								<xsl:when test="/Page/Request/QueryString/Item[@name='ewCmd2']">
									<a href="{$appPath}?ewCmd={$ewCmd}&amp;startPos={$startPos}" class="btn btn-primary pull-right">
										<i class="fa fa-chevron-left">
											<xsl:text> </xsl:text>
										</i>&#160;Back to <xsl:value-of select="$title"/>
									</a>

									<xsl:apply-templates select="ContentDetail/ActivityLog" mode="activityReport"/>

									<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
									<xsl:variable name="currency" select="ContentDetail/Content[@type='order']/@currencySymbol"/>
									<xsl:variable name="statusId" select="ContentDetail/Content[@type='order']/@statusId"/>
									<xsl:variable name="orderId" select="ContentDetail/Content[@type='order']/@id"/>
									<xsl:variable name="orderDate" select="ContentDetail/Content[@type='order']/@created"/>
									<xsl:for-each select="ContentDetail/Content[@type='order']/Order">
										<xsl:apply-templates select="." mode="displayCart">
											<xsl:with-param name="currency" select="$currency"/>
											<xsl:with-param name="statusId" select="$statusId"/>
											<xsl:with-param name="orderId" select="$orderId"/>
											<xsl:with-param name="orderDate" select="$orderDate"/>
										</xsl:apply-templates>
									</xsl:for-each>
								</xsl:when>
								<xsl:otherwise>
									<h3>
										No <xsl:value-of select="$title"/>  found
									</h3>
								</xsl:otherwise>
							</xsl:choose>
						</xsl:otherwise>
					</xsl:choose>
				</div>
			</div>
			<!-- Terminus class fix to floating columns -->
			<div class="terminus">&#160;</div>
		</div>
	</xsl:template>

	<xsl:template match="ActivityLog" mode="activityReport">
	</xsl:template>

	<xsl:template match="Content[@type='order']" mode="ListOrders">
		<xsl:variable name="startPos" select="number(concat(0,/Page/Request/QueryString/Item[@name='startPos']))"/>
		<xsl:variable name="ewCmd" select="/Page/Request/QueryString/Item[@name='ewCmd']"/>
		<tr>
			<td>
				<xsl:value-of select="@id"/>
			</td>
			<td>
				<small>
					(<xsl:value-of select="@statusId"/>)
				</small>&#160;
				<xsl:choose>
					<xsl:when test="@statusId='0'">New</xsl:when>
					<xsl:when test="@statusId='1'">Items Added</xsl:when>
					<xsl:when test="@statusId='2'">Billing Address Added</xsl:when>
					<xsl:when test="@statusId='3'">Delivery Address Added</xsl:when>
					<xsl:when test="@statusId='4'">Confirmed</xsl:when>
					<xsl:when test="@statusId='5'">Pass for Payment</xsl:when>
					<xsl:when test="@statusId='6'">Completed</xsl:when>
					<xsl:when test="@statusId='7'">Refunded</xsl:when>
					<xsl:when test="@statusId='8'">Failed</xsl:when>
					<xsl:when test="@statusId='9'">Shipped</xsl:when>
					<xsl:when test="@statusId='10'">Deposit Paid</xsl:when>
					<xsl:when test="@statusId='11'">Abandoned</xsl:when>
					<xsl:when test="@statusId='12'">Deleted</xsl:when>
					<xsl:when test="@statusId='13'">Awaiting Payment</xsl:when>
					<xsl:when test="@statusId='17'">In Progress</xsl:when>
				</xsl:choose>
			</td>
			<td>
				<xsl:choose>
					<xsl:when test="@userId!=''">
						<a href="{$appPath}?ewCmd=Profile&amp;DirType=User&amp;id={@userId}">
							<span class="btn btn-primary btn-xs">
								<i class="fa fa-user fa-white">
									<xsl:text> </xsl:text>
								</i>
							</span>
							&#160;<xsl:value-of select="Order/Contact[@type='Billing Address']/GivenName/node()"/>
						</a>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="Order/Contact[@type='Billing Address']/GivenName/node()"/>
					</xsl:otherwise>
				</xsl:choose>
			</td>
			<td>
				<a href="mailto:{Order/Contact[@type='Billing Address']/Email/node()}">
					<xsl:value-of select="Order/Contact[@type='Billing Address']/Email/node()"/>
				</a>
			</td>
			<td>
				<!--xsl:value-of select="@created"/-->
				<xsl:choose>
					<xsl:when test="Order/@InvoiceDateTime!=''">
						<xsl:call-template name="DD_Mon_YY">
							<xsl:with-param name="date">
								<xsl:value-of select="Order/@InvoiceDateTime"/>
							</xsl:with-param>
							<xsl:with-param name="showTime">true</xsl:with-param>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="DD_Mon_YY">
							<xsl:with-param name="date">
								<xsl:value-of select="@created"/>
							</xsl:with-param>
							<xsl:with-param name="showTime">true</xsl:with-param>
						</xsl:call-template>
					</xsl:otherwise>
				</xsl:choose>

			</td>
			<td>
				<xsl:value-of select="@currencySymbol"/>&#160;<xsl:value-of select="format-number(Order/@total,'0.00')"/>
				<!-- COMMENTED THIS LINE AS @TOTAL ALREADY INCLUDES THE SHIPPING -->
				<!--<xsl:value-of select="format-number(Order/@total + Order/@shippingCost,'0.00')"/>-->
				&#160;
				<xsl:value-of select="@currency"/>
			</td>
			<xsl:if test="@statusId='Deposit Paid'">
				<td>
					<xsl:value-of select="@currencySymbol"/>&#160;<xsl:value-of select="format-number(Order/@paymentMade,'0.00')"/>
					<!-- COMMENTED THIS LINE AS @TOTAL ALREADY INCLUDES THE SHIPPING -->
					<!--<xsl:value-of select="format-number(Order/@total + Order/@shippingCost,'0.00')"/>-->
					&#160;
					<xsl:value-of select="@currency"/>
				</td>
				<td>
					<xsl:value-of select="@currencySymbol"/>&#160;<xsl:value-of select="format-number(Order/@outstandingAmount,'0.00')"/>
					<!-- COMMENTED THIS LINE AS @TOTAL ALREADY INCLUDES THE SHIPPING -->
					<!--<xsl:value-of select="format-number(Order/@total + Order/@shippingCost,'0.00')"/>-->
					&#160;
					<xsl:value-of select="@currency"/>
				</td>
				<td>
					<xsl:call-template name="DD_Mon_YY">
						<xsl:with-param name="date">
							<xsl:value-of select="Order/Item[1]/productDetail/StartDate/node()"/>
						</xsl:with-param>
						<xsl:with-param name="showTime">false</xsl:with-param>
					</xsl:call-template>
				</td>
			</xsl:if>
			<td>
				<a href="{$appPath}?ewCmd=Orders&amp;ewCmd2=Display&amp;id={@id}" class="btn btn-xs btn-primary">
					<i class="fa fa-eye">
						<xsl:text> </xsl:text>
					</i><xsl:text> </xsl:text>view order
				</a>
				<xsl:if test="@statusId=6">
					<a href="{$appPath}?ewCmd=Orders&amp;ewCmd2=Print&amp;id={@id}" target="_new" class="btn btn-xs btn-primary">
						<i class="fa fa-print">
							<xsl:text> </xsl:text>
						</i>
						<xsl:text> print</xsl:text>
					</a>

				</xsl:if>
				<xsl:if test="@statusId&lt;6 or @statusId=13">
					<a href="{$appPath}?ewCmd=PreviewOn&amp;PreviewUser={@userId}&amp;CartId={@id}&amp;cartCmd=Cart" class="btn btn-xs btn-primary">
						<i class="fa fa-cart-plus">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>complete order
					</a>
				</xsl:if>
				<xsl:if test="@statusId=10">
					<a href="{$appPath}?ewCmd=Orders&amp;ewCmd2=RequestSettlement&amp;id={@id}" target="_new" class="btn btn-xs btn-primary">
						<i class="fa fa-envelope">
							<xsl:text> </xsl:text>
						</i>
						<xsl:text> Send Settlement Request</xsl:text>
					</a>

				</xsl:if>

			</td>
			<td>
				<xsl:if test="@statusId=6">
					<input type="checkbox" name="id" value="{@id}" class="input-control"/>
				</xsl:if>
			</td>
		</tr>
	</xsl:template>

	<!--################################################################-->
	<xsl:template match="Page[@layout='RefundOrder']" mode="Admin">
		<div class="template" id="template_EditStructure">
			<div id="column2">
				<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
			</div>
		</div>
	</xsl:template>

	<!-- -->
	<!--   ##################  List Quotes  ##############################   -->
	<!-- -->
	<xsl:template match="Page[@layout='Quotes']" mode="Admin">
		<div class="template" id="template_EditStructure">
			<div id="column2">
				<xsl:if test="ContentDetail/Content[@type='quote'] and not(/Page/Request/QueryString/Item[@name='ewCmd2'])">
					<table cellpadding="0" cellspacing="1" class="adminList">
						<tbody>
							<tr>
								<th colspan="6">Quotes</th>
							</tr>
							<tr>
								<th>Quote Id</th>
								<th>Status</th>
								<th>Customer Name</th>
								<th>Email</th>
								<th>Time Placed</th>
								<th>&#160;</th>
							</tr>
							<xsl:for-each select="ContentDetail/Content[@type='quote']">
								<tr>
									<td>
										<xsl:value-of select="@id"/>
									</td>
									<td>
										<xsl:value-of select="@statusId"/> - 	<xsl:choose>
											<xsl:when test="@statusId='0'">New</xsl:when>
											<xsl:when test="@statusId='1'">Items Added</xsl:when>
											<xsl:when test="@statusId='2'">Billing Address Added</xsl:when>
											<xsl:when test="@statusId='3'">Delivery Address Added</xsl:when>
											<xsl:when test="@statusId='4'">Confirmed</xsl:when>
											<xsl:when test="@statusId='5'">Pass for Payment</xsl:when>
											<xsl:when test="@statusId='6'">Completed</xsl:when>
											<xsl:when test="@statusId='7'">Refunded</xsl:when>
											<xsl:when test="@statusId='8'">Failed</xsl:when>
											<xsl:when test="@statusId='9'">Shipped</xsl:when>
											<xsl:when test="@statusId='10'">Deposit Paid</xsl:when>
											<xsl:when test="@statusId='11'">Abandoned</xsl:when>
											<xsl:when test="@statusId='17'">In Progress</xsl:when>
										</xsl:choose>
									</td>
									<td>
										<a href="{$appPath}?ewCmd=Quotes&amp;ewCmd2=Display&amp;id={@id}">
											<xsl:value-of select="Quote/Contact[@type='Billing Address']/GivenName/node()"/>
										</a>
									</td>
									<td>
										<xsl:value-of select="Quote/Contact[@type='Billing Address']/Email/node()"/>
									</td>
									<td>
										<xsl:value-of select="@created"/>
										<!--xsl:call-template name="DD_Mon_YY">
                      <xsl:with-param name="date">
                        <xsl:value-of select="@dUpdateDate"/>
                      </xsl:with-param>
                      <xsl:with-param name="showTime">true</xsl:with-param>
                    </xsl:call-template-->
									</td>
									<td>
										<a href="{$appPath}?ewCmd=Quotes&amp;ewCmd2=Display&amp;id={@id}" class="view adminButton">view Quote</a>
									</td>
								</tr>
							</xsl:for-each>
						</tbody>
					</table>
				</xsl:if>
				<xsl:if test="/Page/Request/QueryString/Item[@name='ewCmd2']">
					<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
					<xsl:variable name="currency" select="ContentDetail/Content[@type='quote']/@currencySymbol"/>
					<xsl:variable name="statusId" select="ContentDetail/Content[@type='quote']/@statusId"/>
					<xsl:variable name="orderId" select="ContentDetail/Content[@type='quote']/@id"/>
					<xsl:variable name="orderDate" select="ContentDetail/Content[@type='quote']/@created"/>
					<xsl:for-each select="ContentDetail/Content[@type='order']/Order | ContentDetail/Content[@type='quote']/Quote">
						<xsl:apply-templates select="." mode="displayCart">
							<xsl:with-param name="currency" select="$currency"/>
							<xsl:with-param name="statusId" select="$statusId"/>
							<xsl:with-param name="orderId" select="$orderId"/>
							<xsl:with-param name="orderDate" select="$orderDate"/>
						</xsl:apply-templates>
					</xsl:for-each>
				</xsl:if>
			</div>
			<div id="column1">
				<h5>Quote Instructions</h5>
				<p>View all Quotes placed on your site here.</p>
				<p>Click on the 'VIEW QUOTE' button to view all the details for that Quote.</p>
			</div>
			<!-- Terminus class fix to floating columns -->
			<div class="terminus">&#160;</div>
		</div>
	</xsl:template>
	<!-- -->
	<!--   ################################################   Cart Full  ##############################################   -->
	<!-- -->
	<xsl:template match="Order" mode="displayCart">
		<xsl:param name="currency"/>
		<xsl:param name="statusId"/>
		<xsl:param name="orderId"/>
		<xsl:param name="orderDate"/>
		<div id="cartFull">
			<div class="panel panel-default">
				<div class="panel-heading">
					<xsl:if test="Item/productDetail/@type='Ticket' and Item/productDetail/Ticket">
						<a href="/ewcommon/tools/pageAsPDF.ashx?ewCmd=Orders&amp;ewCmd2=Display&amp;id={$orderId}&amp;filename=Tickets-{$orderId}" class="btn btn-primary btn-sm pull-right" target="_new">
							<i class="fas fa-file-pdf">&#160;</i>&#160;Reprint Tickets
						</a>
					</xsl:if>
					<xsl:if test="Item/productDetail/@type='Ticket' and not(Item/productDetail/Ticket)">
						<a href="/ewcommon/tools/issuetickets.ashx?orderid={$orderId}" class="btn btn-primary btn-sm pull-right" target="_new">
							<i class="fas fa-file-pdf">&#160;</i>&#160;Issue Tickets
						</a>
					</xsl:if>
					<xsl:choose>
						<xsl:when test="(@statusId='6' or @statusId='10') and (PaymentDetails/@provider='JudoPay' or PaymentDetails/@provider='Pay360' or PaymentDetails/@provider='Stripe')">
							<div>
								<a href="?ewCmd=RefundOrder&amp;orderId={$orderId}&amp;id={/Page/Request/QueryString/Item[@name='id']}" class="btn btn-danger btn-sm pull-right">
									<i class="fa fa-money"> </i>
									Refund Order
								</a>
							</div>
						</xsl:when>
						<xsl:otherwise>

						</xsl:otherwise>
					</xsl:choose>
					<h3 class="panel-title">
						<xsl:choose>
							<xsl:when test="$statusId='0'">New</xsl:when>
							<xsl:when test="$statusId='1'">Items Added</xsl:when>
							<xsl:when test="$statusId='2'">Billing Address Added</xsl:when>
							<xsl:when test="$statusId='3'">Delivery Address Added</xsl:when>
							<xsl:when test="$statusId='4'">Confirmed</xsl:when>
							<xsl:when test="$statusId='5'">Pass for Payment</xsl:when>
							<xsl:when test="$statusId='6'">
								<i class="fa fa-check">
									<xsl:text> </xsl:text>
								</i><xsl:text> </xsl:text>Completed
							</xsl:when>
							<xsl:when test="$statusId='7'">Refunded</xsl:when>
							<xsl:when test="$statusId='8'">Failed</xsl:when>
							<xsl:when test="$statusId='9'">
								<i class="fa fa-truck">
									<xsl:text> </xsl:text>
								</i><xsl:text> </xsl:text>Shipped
							</xsl:when>
							<xsl:when test="$statusId='10'">Deposit Paid</xsl:when>
							<xsl:when test="$statusId='11'">Abandoned</xsl:when>

							<xsl:when test="@statusId='17'">In Progress</xsl:when>
						</xsl:choose> Order<xsl:choose>
							<xsl:when test="@cmd='Add' or @cmd='Cart'"> - Contents</xsl:when>
							<xsl:when test="@cmd='Billing'"> - Enter the billing address</xsl:when>
							<xsl:when test="@cmd='Delivery'"> - Enter the delivery address</xsl:when>
							<xsl:when test="@cmd='EnterOptions'"> - Select your delivery options</xsl:when>
							<xsl:when test="@cmd='ShowInvoice' or @cmd='ShowCallBackInvoice'"> - Your invoice</xsl:when>
							<xsl:when test="@cmd='Quit'"> - No items added</xsl:when>
							<xsl:when test="@cmd='ChoosePaymentShippingOption'"> - Enter your payment details</xsl:when>
						</xsl:choose><!--xsl:value-of select="@cmd"/-->
						<xsl:text> - </xsl:text>
						<xsl:value-of select="$orderId"/>
						<xsl:text>        </xsl:text>
						<xsl:if test="PaymentDetails/Refund">
							<label for="refundStatus" style="color:red;">Transaction has been refunded successfully</label>
						</xsl:if>

					</h3>
				</div>
				<div class="panel-body row">
					<div class="col-md-3">
						<h4>Order Details</h4>
						<dl class="dl-horizontal">
							<dt>
								Order Date
							</dt>
							<dd>
								<xsl:call-template name="DD_Mon_YYYY">
									<xsl:with-param name="date">
										<xsl:value-of select="$orderDate"/>
									</xsl:with-param>
									<xsl:with-param name="showTime">true</xsl:with-param>
								</xsl:call-template>
							</dd>
							<dt>
								Order Reference
							</dt>
							<dd>
								<xsl:value-of select="$orderId"/>
							</dd>
							<dt>
								Customer Account
							</dt>
							<dd>
								<xsl:if test="ancestor::Content/User">
									<a href="?ewCmd=Profile&amp;DirType=User&amp;id={ancestor::Content/User/@id}">
										<span class="">
											<i class="fa fa-user fa-white"> </i>
											<xsl:text> </xsl:text>
											<xsl:value-of select="ancestor::Content/User/FirstName/node()"/>
											<xsl:text> </xsl:text>
											<xsl:value-of select="ancestor::Content/User/LastName/node()"/>
										</span>
									</a>
								</xsl:if>
							</dd>

							<xsl:if test="@payableType='deposit' and (@payableAmount &gt; 0) ">
								<dt>
									Payment Received
								</dt>
								<dd>
									<xsl:value-of select="$currency"/>
									<xsl:value-of select="format-number(@paymentMade,'0.00')" />
								</dd>
								<dt>Final Payment Reference/Link</dt>
								<dd>
									<xsl:variable name="secureURL">
										<xsl:text>http</xsl:text>
										<xsl:if test="$page/Request/ServerVariables/Item[@name='HTTPS']='on'">s</xsl:if>
										<xsl:text>://</xsl:text>
										<xsl:value-of select="$page/Request/ServerVariables/Item[@name='SERVER_NAME']"/>
									</xsl:variable>
									<a href="{$secureURL}?cartCmd=Settlement&amp;SettlementRef={@settlementID}">
										<xsl:value-of select="@settlementID" />
									</a>
								</dd>
							</xsl:if>
							<xsl:if test="@paymentMade!=''">
								<dt>Payment Made</dt>
								<dd>
									<xsl:value-of select="$currency"/>
									<xsl:value-of select="format-number(@paymentMade,'0.00')" />
								</dd>
							</xsl:if>
							<xsl:choose>
								<xsl:when test="@outstandingAmount&gt;0">
									<dt>Total Outstanding</dt>
									<dd>
										<xsl:value-of select="$currency"/>
										<xsl:value-of select="format-number(@outstandingAmount, '0.00')"/>
									</dd>
									<dt>Total Payment Due</dt>
									<dd>
										<xsl:value-of select="$currency"/>
										<xsl:value-of select="format-number(@total, '0.00')"/>
									</dd>
								</xsl:when>
								<xsl:otherwise>
								</xsl:otherwise>
							</xsl:choose>


						</dl>
						<xsl:if test="Payment">
							<h4>Payment Details</h4>
							<dl class="dl-horizontal">
								<dt>Payment Method</dt>
								<dd>
									<xsl:value-of select="PaymentDetails/@provider"/>
								</dd>
								<dt>Payment Ref.</dt>
								<dd>
									<xsl:value-of select="PaymentDetails/@ref"/>
								</dd>
								<dt>Payment Acct</dt>
								<dd>
									<xsl:value-of select="PaymentDetails/@acct"/>
								</dd>
								<xsl:for-each select="PaymentDetails/*[local-name()!='Ref' or node()!='']/*">
									<dt>
										<xsl:value-of select="local-name()"/>
									</dt>
									<dd>
										<xsl:value-of select="node()"/>
									</dd>
								</xsl:for-each>
							</dl>
						</xsl:if>


					</div>
					<xsl:if test="Contact[@type='Billing Address']">
						<div id="billingAddress" class="cartAddress col-md-3">
							<a href="?ewCmd=EditOrderContact&amp;orderId={$orderId}&amp;ContactType=Billing" class="btn btn-primary btn-sm pull-right">
								<i class="fa fa-edit"> </i>
								Edit
							</a>
							<xsl:apply-templates select="Contact[@type='Billing Address']" mode="cart"/>
						</div>
					</xsl:if>
					<xsl:if test="Contact[@type='Delivery Address'] and not(@hideDeliveryAddress)">
						<div id="deliveryAddress" class="cartAddress col-md-3">
							<a href="?ewCmd=EditOrderContact&amp;orderId={$orderId}&amp;ContactType=Delivery" class="btn btn-primary btn-sm pull-right">
								<i class="fa fa-edit"> </i>
								Edit
							</a>
							<xsl:apply-templates select="Contact[@type='Delivery Address']" mode="cart"/>
						</div>
					</xsl:if>
					<xsl:if test="DeliveryDetails">
						<div id="carrier-info" class="col-md-3">
							<h4>Shipping Details</h4>
							<dl class="dl-horizontal">
								<xsl:for-each select="DeliveryDetails">
									<dt>Carrier</dt>
									<dd>
										<xsl:value-of select="@carrierName"/>
									</dd>
									<dt>Ref</dt>
									<dd>
										<xsl:value-of select="@ref"/>
									</dd>
									<dt>Collected Date</dt>
									<dd>
										<xsl:call-template name="DD_Mon_YYYY">
											<xsl:with-param name="date">
												<xsl:value-of select="@collectionDate"/>
											</xsl:with-param>
											<xsl:with-param name="showTime">false</xsl:with-param>
										</xsl:call-template>
									</dd>
									<dt>Delivery Date</dt>
									<dd>
										<xsl:call-template name="DD_Mon_YYYY">
											<xsl:with-param name="date">
												<xsl:value-of select="@deliveryDate"/>
											</xsl:with-param>
											<xsl:with-param name="showTime">false</xsl:with-param>
										</xsl:call-template>
									</dd>
									<dt>Notes</dt>
									<dd>
										<xsl:value-of select="@notes"/>
									</dd>
								</xsl:for-each>
							</dl>
						</div>
					</xsl:if>
					<xsl:if test="Payment">
						<a class="btn btn-primary pull-right" role="button" data-toggle="collapse" href="#paymentTable" aria-expanded="false" aria-controls="paymentTable">
							Show Payments&#160;&#160;<i class="fa fa-credit-card">&#160;</i>
						</a>
						<div class="col-md-12">

							<table class="table collapse" id="paymentTable">
								<thead>
									<tr>
										<th scope="col">Date</th>
										<th scope="col">Provider</th>
										<th scope="col">Amount</th>
										<th scope="col">AuthCode</th>
									</tr>
								</thead>
								<tbody>
									<xsl:for-each select="Payment">
										<tr>
											<th scope="row">
												<xsl:call-template name="DD_Mon_YYYY">
													<xsl:with-param name="date">
														<xsl:value-of select="dInsertDate"/>
													</xsl:with-param>
													<xsl:with-param name="showTime">true</xsl:with-param>
												</xsl:call-template>
											</th>
											<td>
												<xsl:value-of select="cPayMthdProviderName"/>
												<br/>
												<small>
													<xsl:value-of select="cPayMthdAcctName"/>
												</small>

											</td>
											<th scope="row">
												<xsl:value-of select="nPaymentAmount"/>
											</th>
											<td>
												<xsl:value-of select="cPayMthdProviderRef"/>
											</td>
										</tr>
									</xsl:for-each>
								</tbody>
							</table>
						</div>
					</xsl:if>
					<xsl:if test="Notes/Notes">
						<br/>
						<br/>
						<xsl:apply-templates select="Notes" mode="displayNotes"/>

					</xsl:if>
				</div>


				<table cellspacing="0" id="cartListing" summary="This table contains a list of the items which you have added to the shopping cart. To change the quantity of an item, replace the number under the Qty column and click on Update Cart." class="table table-striped">
					<tr>
						<th class="heading">&#160;</th>
						<th class="heading quantity">Qty</th>
						<th class="heading description">Description</th>
						<th class="heading ref">Ref</th>
						<th class="heading price">Price</th>
						<th class="heading lineTotal">Line Total</th>
					</tr>
					<xsl:for-each select="Item">
						<xsl:apply-templates select="." mode="orderItemAdmin">
							<xsl:with-param name="currency" select="$currency"/>
						</xsl:apply-templates>
					</xsl:for-each>
					<xsl:if test="@shippingCost &gt; 0">
						<tr>
							<td colspan="5" class="shipping heading">
								<strong>
									Shipping Cost:&#160;
								</strong>
								<xsl:choose>
									<xsl:when test="Shipping">
										<xsl:value-of select="Shipping/Name/node()"/>
										<strong>&#160;-&#160;</strong>
										<xsl:value-of select="Shipping/Carrier/node()"/>
										<strong>&#160;-&#160;</strong>
										<xsl:value-of select="Shipping/DeliveryTime/node()"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="@shippingDesc"/>
									</xsl:otherwise>
								</xsl:choose>
							</td>
							<td class="shipping amount">
								<xsl:value-of select="$currency"/>
								<xsl:value-of select="format-number(@shippingCost,'0.00')"/>
							</td>
						</tr>
					</xsl:if>
					<xsl:if test="@vatRate &gt; 0">
						<tr>
							<td colspan="4">
								<!--xsl:attribute name="rowspan">
									<xsl:call-template name="calcRows">
										<xsl:with-param name="r1"><xsl:choose><xsl:when test="@vatRate &gt; 0">1</xsl:when><xsl:otherwise>0</xsl:otherwise></xsl:choose>	</xsl:with-param>
										<xsl:with-param name="r2"><xsl:choose><xsl:when test="@payableAmount &gt; 0">1</xsl:when><xsl:otherwise>0</xsl:otherwise></xsl:choose>	</xsl:with-param>
										<xsl:with-param name="r3"><xsl:choose><xsl:when test="@paymentMade &gt; 0">1</xsl:when><xsl:otherwise>0</xsl:otherwise></xsl:choose>	</xsl:with-param>
										<xsl:with-param name="r0">2</xsl:with-param>
									</xsl:call-template>
								</xsl:attribute-->
								&#160;
							</td>
							<td class="subTotal heading">
								Sub Total:
							</td>
							<td class="subTotal amount">
								<xsl:value-of select="$currency"/>
								<xsl:value-of select="format-number(@totalNet, '0.00')"/>
							</td>
						</tr>

						<tr>
							<td colspan="4">&#160;</td>
							<td class="vat heading">
								<xsl:choose>
									<xsl:when test="//Cart/Contact/Address/Country='United Kingdom'">VAT at </xsl:when>
									<xsl:otherwise>Tax at </xsl:otherwise>
								</xsl:choose>
								<xsl:value-of select="format-number(@vatRate, '#.00')"/>%:
							</td>
							<td class="vat amount">
								<span class="currency">
									<xsl:value-of select="$currency"/>
								</span>
								<xsl:value-of select="format-number(@vatAmt, '0.00')"/>
							</td>
						</tr>
					</xsl:if>
					<tr>
						<td colspan="4">&#160;</td>
						<td class="total heading">Total Value:</td>
						<td class="total amount">
							<xsl:value-of select="$currency"/>&#160;
							<xsl:value-of select="format-number(@total, '0.00')"/>
						</td>
					</tr>
					<xsl:if test="@paymentMade">
						<xsl:if test="@transStatus='Settlement Paid'">
							<tr>
								<td colspan="4">&#160;</td>
								<td class="total heading">
									Deposit Paid:
								</td>
								<td class="total amount">
									<xsl:value-of select="$currency"/>&#160;
									<xsl:value-of select="format-number(@total - @paymentMade, '0.00')"/>
								</td>
							</tr>
						</xsl:if>
						<tr>
							<td colspan="4">&#160;</td>
							<td class="total heading">
								<xsl:choose>
									<xsl:when test="@transStatus">
										<xsl:value-of select="@transStatus"/>:
									</xsl:when>
									<xsl:when test="@payableType='settlement' and @tranStatus='Settlement Paid'">Payment Received:</xsl:when>
								</xsl:choose>
							</td>
							<td class="total amount">
								<xsl:value-of select="$currency"/>&#160;
								<xsl:value-of select="format-number(@paymentMade, '0.00')"/>
							</td>
						</tr>
					</xsl:if>
					<xsl:if test="@outstandingAmount">
						<tr>
							<td colspan="4">&#160;</td>
							<td class="total heading">
								Amount Outstanding:
							</td>
							<td class="total amount">
								<xsl:value-of select="$currency"/>&#160;
								<xsl:value-of select="format-number(@outstandingAmount, '0.00')"/>
							</td>
						</tr>
					</xsl:if>
					<tr>
						<td colspan="6">
							Seller Notes:<br/>
							<xsl:copy-of select="SellerNotes/node()"/>
						</td>
					</tr>
				</table>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Order" mode="displayNotes">
		<xsl:variable name="parentURL">
			<xsl:call-template name="getContentParURL"/>
		</xsl:variable>
		<div class="col-md-12">
			<div class="notes alert alert-info">
				<i class="fas fa-lg fa-info-circle">&#160;</i>&#160;<strong>Notes from customer:</strong>&#160;&#160;
				<xsl:if test="Notes/Notes/node()!='' or Notes/PromotionalCode/node()!=''">
					<xsl:if test="Notes/Notes/node()!=''">
						<h3>
							<!--Additional information for Your Order-->
							<xsl:call-template name="term3010" />
						</h3>
						<xsl:for-each select="Notes/Notes/*">
							<xsl:if test="node()!=''">
								<p>
									<xsl:choose>
										<xsl:when test="@label and @label!=''">
											<xsl:value-of select="@label"/>
										</xsl:when>
										<xsl:otherwise>
											<xsl:value-of select="name()"/>
										</xsl:otherwise>
									</xsl:choose>
									<xsl:text>: </xsl:text>
									<xsl:value-of select="node()"/>
								</p>
							</xsl:if>
						</xsl:for-each>
					</xsl:if>
					<xsl:if test="Notes/PromotionalCode/node()!=''">
						<p>
							<!--Promotional Code entered-->
							<xsl:call-template name="term3011" />
							<xsl:text>:&#160;</xsl:text>
							<xsl:apply-templates select="Notes/PromotionalCode/node()" mode="cleanXhtml"/>
						</p>
					</xsl:if>
					<xsl:if test="not(@readonly) and Notes/Notes/node()!=''">
						<p class="optionButtons">
							<a href="{$parentURL}?cartCmd=Notes" class="button">
								<xsl:attribute name="title">
									<!--Click here to edit the notes on this order.-->
									<xsl:call-template name="term3012" />
								</xsl:attribute>
								<!--Edit Notes-->
								<xsl:call-template name="term3013" />
							</a>
						</p>
					</xsl:if>
				</xsl:if>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Contact" mode="cart">
		<xsl:param name="parentURL"/>
		<xsl:param name="cartType"/>
		<xsl:variable name="secureURL">
			<xsl:call-template name="getSecureURL"/>
		</xsl:variable>
		<xsl:variable name="type">
			<xsl:value-of select="substring-before(@type,' ')"/>
		</xsl:variable>
		<div>
			<xsl:if test="not(/Page/Cart/Order/@cmd='ShowInvoice') and not(/Page/Cart/Order/@cmd='MakePayment') and (ancestor::*[name()='Cart'])">
				<xsl:if test="/Page/Cart/Order/@cmd!='MakePayment'">
					<a href="{$parentURL}?pgid={/Page/@id}&amp;{$cartType}Cmd={$type}" class="btn btn-default btn-sm pull-right">
						<i class="fa fa-pencil">&#160;</i>&#160;Edit <xsl:value-of select="@type"/>
					</a>
				</xsl:if>
			</xsl:if>
			<h4 class="addressTitle">
				<xsl:value-of select="@type"/>
				<xsl:text>&#160;</xsl:text>
				<!--Details-->
				<!--xsl:call-template name="term3070" /-->
			</h4>
			<p>
				<xsl:value-of select="GivenName"/>
				<br/>
				<xsl:if test="Company/node()!=''">
					<xsl:value-of select="Company"/>,
					<br/>
				</xsl:if>

				<xsl:value-of select="Street"/>,
				<br/>
				<xsl:value-of select="City"/>,
				<br/>
				<xsl:if test="State/node()!=''">
					<xsl:value-of select="State"/>
					.<xsl:text> </xsl:text>
				</xsl:if>
				<xsl:value-of select="PostalCode"/>.
				<br/>
				<xsl:if test="Country/node()!=''">
					<xsl:value-of select="Country"/>
					<br/>
				</xsl:if>
				<!--Tel-->
				<xsl:call-template name="term3071" />
				<xsl:text>:&#160;</xsl:text>
				<xsl:value-of select="Telephone"/>
				<br/>
				<xsl:if test="Fax/node()!=''">
					<!--Fax-->
					<xsl:call-template name="term3072" />
					<xsl:text>:&#160;</xsl:text>
					<xsl:value-of select="Fax"/>
					<br/>
				</xsl:if>
				<xsl:if test="Email/node()!=''">
					<!--Email-->
					<xsl:call-template name="term3073" />
					<xsl:text>:&#160;</xsl:text>
					<xsl:value-of select="Email"/>
					<br/>
				</xsl:if>
			</p>

		</div>

	</xsl:template>

	<!-- ################################# Order Item ######################################## -->
	<!--#-->
	<xsl:template match="Item" mode="product-description">

	</xsl:template>

	<xsl:template match="Item" mode="orderItemAdmin">
		<xsl:param name="editQty"/>
		<xsl:variable name="parentURL">
			<xsl:call-template name="getContentParURL"/>
		</xsl:variable>
		<xsl:variable name="secureURL">
			<xsl:call-template name="getSecureURL"/>
		</xsl:variable>
		<xsl:variable name="siteURL">
			<xsl:call-template name="getSiteURL"/>
		</xsl:variable>
		<tr class="orderItem">
			<td class="cell delete">
				<xsl:choose>
					<xsl:when test="$editQty!='true'">&#160;</xsl:when>
					<xsl:otherwise>
						<a href="{$parentURL}?cartCmd=Remove&amp;id={@id}" title="click here to remove this item from the list">
							<!--BJR - This either doesnt work or is wrong so i have changed it for the moment to work-->
							<!--<img src="{$secureURL}/ewCommon/images/icons/trash.gif" alt="delete icon - click here to remove this item from the list"/>-->
							<img src="/ewCommon/images/icons/delete.png" width="20" height="20" alt="delete icon - click here to remove this item from the list"/>
						</a>
					</xsl:otherwise>
				</xsl:choose>
			</td>
			<td class="cell quantity">
				<xsl:choose>
					<xsl:when test="$editQty!='true'">
						<xsl:value-of select="@quantity"/>
					</xsl:when>
					<xsl:otherwise>
						<input type="text" size="2" name="itemId-{@id}" value="{@quantity}" class="">
							<xsl:if test="../@readonly">
								<xsl:attribute name="readonly">readonly</xsl:attribute>
							</xsl:if>
						</input>
					</xsl:otherwise>
				</xsl:choose>
			</td>
			<td class="cell description">
				<a href="{$siteURL}{@url}" title="">
					<xsl:if test="productDetail/ParentProduct/Content/Name">
						<xsl:value-of select="productDetail/ParentProduct/Content/Name"/> -
					</xsl:if>
					<xsl:value-of select="Name"/>
				</a>
				<xsl:apply-templates select="." mode="product-description"/>
				<!-- ################################# Line Options Info ################################# -->
				<xsl:for-each select="Item">
					<span class="optionList">
						<xsl:choose>
							<xsl:when test="option">
								<xsl:apply-templates select="option" mode="optionDetail"/>
							</xsl:when>
							<xsl:otherwise>
								<br/>
								<xsl:value-of select="Name"/>
							</xsl:otherwise>
						</xsl:choose>

						<!-- <xsl:if test="@price!=0">
							  Remmed by Rob
							  <xsl:value-of select="$currency"/>
							  <xsl:value-of select="format-number(@price,'#0.00')"/>
								
							  <xsl:apply-templates select="/Page" mode="formatPrice">
								  <xsl:with-param name="price" select="@price"/>
								  <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
							  </xsl:apply-templates>
						  </xsl:if>-->
					</span>
				</xsl:for-each>
				<!-- ################################# Line Discount Info ################################# -->
				<xsl:if test="Discount">
					<xsl:for-each select="DiscountPrice/DiscountPriceLine">
						<xsl:sort select="@PriceOrder"/>
						<xsl:variable name="DiscID">
							<xsl:value-of select="@nDiscountKey"/>
						</xsl:variable>
						<div class="discount">
							<xsl:if test="ancestor::Item/Discount[@nDiscountKey=$DiscID]/Images[@class='thumbnail']/@src!=''">
								<xsl:copy-of select="ancestor::Item/Discount[@nDiscountKey=$DiscID]/Images[@class='thumbnail']"/>
							</xsl:if>
							<xsl:value-of select="ancestor::Item/Discount[@nDiscountKey=$DiscID]/@cDiscountName"/>
							<!--RRP-->
							<xsl:call-template name="term3053" />
							<xsl:text>:&#160;</xsl:text>
							<strike>
								<xsl:value-of select="$currency"/>
								<xsl:text>:&#160;</xsl:text>
								<xsl:choose>
									<xsl:when test="position()=1">
										<!-- Remmed by Rob
								  <xsl:value-of select="format-number(ancestor::Item/DiscountPrice/@OriginalUnitPrice,'#0.00')"/>
								  -->
										<xsl:apply-templates select="/Page" mode="formatPrice">
											<xsl:with-param name="price" select="ancestor::Item/DiscountPrice/@OriginalUnitPrice"/>
											<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
										</xsl:apply-templates>
									</xsl:when>
									<xsl:otherwise>

										<!-- Remmed by Rob
								  <xsl:value-of select="format-number(preceding-sibling::DiscountPriceLine/@UnitPrice,'#0.00')"/>
								-->
										<xsl:apply-templates select="/Page" mode="formatPrice">
											<xsl:with-param name="price" select="preceding-sibling::DiscountPriceLine/@UnitPrice"/>
											<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
										</xsl:apply-templates>
									</xsl:otherwise>
								</xsl:choose>
							</strike>
							<!--less-->
							<xsl:call-template name="term3054" />
							<xsl:text>:&#160;</xsl:text>
							<!-- Remmed by Rob 
							  <xsl:value-of select="$currency"/>
                              <xsl:value-of select="format-number(@UnitSaving,'#0.00')"/>
							  -->
							<xsl:apply-templates select="/Page" mode="formatPrice">
								<xsl:with-param name="price" select="@UnitSaving"/>
								<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
							</xsl:apply-templates>
						</div>
					</xsl:for-each>
					<!--More will go here later-->
					<xsl:for-each select="DiscountItem">
						<xsl:variable name="DiscID">
							<xsl:value-of select="@nDiscountKey"/>
						</xsl:variable>
						<div class="discount">
							<xsl:value-of select="ancestor::Item/Discount[@nDiscountKey=$DiscID]/@cDiscountName"/>
							<xsl:value-of select="@oldUnits - @Units"/>&#160;Unit<xsl:if test="(@oldUnits - @Units) > 1">s</xsl:if>
							<!-- Remmed by Rob 
							  <xsl:value-of select="$currency"/>
                              <xsl:value-of select="format-number(@TotalSaving,'#0.00')"/>
							  -->
							<xsl:apply-templates select="/Page" mode="formatPrice">
								<xsl:with-param name="price" select="@TotalSaving"/>
								<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
							</xsl:apply-templates>
						</div>
					</xsl:for-each>
				</xsl:if>
			</td>
			<td class="cell ref">
				<xsl:value-of select="@ref"/>&#160;
				<xsl:for-each select="Item">
					<xsl:apply-templates select="option" mode="optionCodeConcat"/>
				</xsl:for-each>
			</td>
			<xsl:if test="not(/Page/Cart/@displayPrice='false')">
				<td class="cell linePrice">
					<xsl:if test="DiscountPrice/@OriginalUnitPrice &gt; @price">
						<strike>
							<!-- Remmed by Rob 
					  <xsl:value-of select="$currency"/>
                      <xsl:value-of select="format-number(DiscountPrice/@OriginalUnitPrice,'#0.00')"/>
					-->
							<xsl:apply-templates select="/Page" mode="formatPrice">
								<xsl:with-param name="price" select="DiscountPrice/@OriginalUnitPrice"/>
								<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
							</xsl:apply-templates>
						</strike>
						<br/>
					</xsl:if>
					<!-- Remmed by Rob
				  <xsl:value-of select="$currency"/>
                  <xsl:value-of select="format-number(@price,'#0.00')"/>
				  -->
					<xsl:apply-templates select="/Page" mode="formatPrice">
						<xsl:with-param name="price" select="@price"/>
						<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
					</xsl:apply-templates>
					<xsl:for-each select="Item[@price &gt; 0]">
						<br/>
						<span class="optionList">
							<!-- Remmed by Rob 
					  <xsl:value-of select="$currency"/>
                      <xsl:value-of select="format-number(@price,'#0.00')"/>
					  -->
							<xsl:apply-templates select="/Page" mode="formatPrice">
								<xsl:with-param name="price" select="@price"/>
								<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
							</xsl:apply-templates>
						</span>
					</xsl:for-each>
				</td>
				<td class="cell lineTotal">
					<xsl:if test="DiscountPrice/@OriginalUnitPrice!=DiscountPrice/@UnitPrice">
						<xsl:if test="(DiscountPrice/@OriginalUnitPrice * DiscountPrice/@Units) &gt; @itemTotal">
							<strike>
								<!-- Remmed by Rob
						<xsl:value-of select="$currency"/>
                        <xsl:value-of select="format-number(DiscountPrice/@OriginalUnitPrice * DiscountPrice/@Units,'#0.00')"/>
						-->
								<xsl:apply-templates select="/Page" mode="formatPrice">
									<xsl:with-param name="price" select="DiscountPrice/@OriginalUnitPrice * DiscountPrice/@Units"/>
									<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
								</xsl:apply-templates>
							</strike>
							<br/>
						</xsl:if>
					</xsl:if>
					<!-- Remmed by Rob 
				  <xsl:value-of select="$currency"/>
				  -->
					<xsl:choose>
						<xsl:when test="@itemTotal">
							<!-- Remmed by Rob 
					  <xsl:value-of select="format-number(@itemTotal,'#0.00')"/>
					  -->
							<xsl:apply-templates select="/Page" mode="formatPrice">
								<xsl:with-param name="price" select="@itemTotal"/>
								<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
							</xsl:apply-templates>
						</xsl:when>
						<xsl:otherwise>
							<!-- Remmed by Rob 
						<xsl:value-of select="format-number((@price +(sum(*/@price)))* @quantity,'#0.00')"/>
						-->
							<xsl:apply-templates select="/Page" mode="formatPrice">
								<xsl:with-param name="price" select="(@price +(sum(*/@price)))* @quantity"/>
								<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
							</xsl:apply-templates>
						</xsl:otherwise>
					</xsl:choose>
				</td>
			</xsl:if>
		</tr>
	</xsl:template>
	<!-- -->
	<!--   ################################################   Quote Full  ##############################################   -->
	<!-- -->
	<xsl:template match="Quote" mode="displayCart">
		<xsl:param name="currency"/>
		<xsl:param name="statusId"/>
		<xsl:param name="orderId"/>
		<xsl:param name="orderDate"/>
		<div id="cartFull">
			<h2>
				<xsl:choose>
					<xsl:when test="$statusId='0'">New</xsl:when>
					<xsl:when test="$statusId='1'">Items Added</xsl:when>
					<xsl:when test="$statusId='2'">Billing Address Added</xsl:when>
					<xsl:when test="$statusId='3'">Delivery Address Added</xsl:when>
					<xsl:when test="$statusId='4'">Confirmed</xsl:when>
					<xsl:when test="$statusId='5'">Pass for Payment</xsl:when>
					<xsl:when test="$statusId='6'">Completed</xsl:when>
					<xsl:when test="$statusId='7'">Refunded</xsl:when>
					<xsl:when test="$statusId='8'">Failed</xsl:when>
					<xsl:when test="$statusId='9'">Shipped</xsl:when>
					<xsl:when test="$statusId='10'">Deposit Paid</xsl:when>
					<xsl:when test="$statusId='11'">Abandoned</xsl:when>

					<xsl:when test="@statusId='17'">In Progress</xsl:when>
				</xsl:choose> Quote<xsl:choose>
					<xsl:when test="@cmd='Add' or @cmd='Cart'"> - Contents</xsl:when>
					<xsl:when test="@cmd='Billing'"> - Enter the billing address</xsl:when>
					<xsl:when test="@cmd='Delivery'"> - Enter the delivery address</xsl:when>
					<xsl:when test="@cmd='EnterOptions'"> - Select your delivery options</xsl:when>
					<xsl:when test="@cmd='ShowInvoice' or @cmd='ShowCallBackInvoice'"> - Your invoice</xsl:when>
					<xsl:when test="@cmd='Quit'"> - No items added</xsl:when>
					<xsl:when test="@cmd='ChoosePaymentShippingOption'"> - Enter your payment details</xsl:when>
				</xsl:choose><!--xsl:value-of select="@cmd"/-->
			</h2>
			<div id="cartInvoice">
				<p>
					Quote Date:&#160;<xsl:value-of select="$orderDate"/>
				</p>
				<p>
					Quote Reference:&#160;<xsl:value-of select="$orderId"/>
				</p>
				<xsl:if test="@payableType='deposit' and (@payableAmount &gt; 0) ">
					<p>
						Payment Received:&#160;<xsl:value-of select="$currency"/><xsl:value-of select="format-number(@paymentMade,'0.00')" />
					</p>
					<p>
						Final Payment Reference:&#160;<strong>
							<xsl:value-of select="@settlementID" />
						</strong>
					</p>
					<p>
						Thank you for your deposit. To pay the outstanding balance, please note your Final Payment Reference, above.  <em>Instructions on paying the outstanding balance have been e-mailed to you.</em>
					</p>
					<p>
						If you have any queries, please call for assistance.
					</p>
				</xsl:if>
				<xsl:if test="@payableType='settlement' or @payableAmount = 0 ">
					<p>
						Payment Made:&#160;<xsl:value-of select="$currency"/><xsl:value-of select="format-number(@paymentMade,'0.00')" />
					</p>
					<xsl:choose>
						<xsl:when test="@outstandingAmount&gt;0">
							Total Payment Outstanding:&#160;<xsl:value-of select="$currency"/><xsl:value-of select="format-number(@outstandingAmount, '0.00')"/>
						</xsl:when>
						<xsl:otherwise>
							<p>
								Total Payment Received:&#160;<xsl:value-of select="$currency"/><xsl:value-of select="format-number(@total, '0.00')"/> (paid in full)
							</p>
						</xsl:otherwise>
					</xsl:choose>

				</xsl:if>
			</div>

			<!-- Terminus class fix to floating columns -->
			<div class="terminus">&#160;</div>
			<xsl:if test="(Notes) and @cmd!='Notes' ">
				<div class="notes">
					<xsl:for-each select="Notes/*">
						<b>
							<xsl:value-of select="name()"/>
						</b>
						<xsl:copy-of select="./node()"/>
					</xsl:for-each>
					<xsl:apply-templates select="Notes" mode="displayNotes"/>
				</div>
			</xsl:if>
			<xsl:if test="Contact[@type='Delivery Address'] and not(@hideDeliveryAddress)">
				<div id="deliveryAddress" class="cartAddress">
					<xsl:apply-templates select="Contact[@type='Delivery Address']" mode="cart"/>
				</div>
			</xsl:if>
			<xsl:if test="Contact[@type='Billing Address']">
				<div id="billingAddress" class="cartAddress">
					<xsl:apply-templates select="Contact[@type='Billing Address']" mode="cart"/>
				</div>
			</xsl:if>
			<!-- Terminus class fix to floating columns -->
			<div class="terminus">&#160;</div>

			<table cellspacing="0" id="cartListing" summary="This table contains a list of the items which you have added to the shopping cart. To change the quantity of an item, replace the number under the Qty column and click on Update Cart.">
				<tr>
					<th class="heading">&#160;</th>
					<th class="heading quantity">Qty</th>
					<th class="heading description">Description</th>
					<th class="heading ref">Ref</th>
					<th class="heading price">Price</th>
					<th class="heading lineTotal">
						Line Total
					</th>
				</tr>
				<xsl:for-each select="Item">
					<xsl:apply-templates select="." mode="quoteItem">
						<xsl:with-param name="currency" select="$currency"/>
					</xsl:apply-templates>
				</xsl:for-each>
				<xsl:if test="@shippingCost &gt; 0">
					<tr>
						<td colspan="4">&#160;</td>
						<td class="shipping heading">
							<xsl:choose>
								<xsl:when test="/Page/Contents/Content[@name='shippingCostLabel']!=''">
									<xsl:value-of select="/Page/Contents/Content[@name='shippingCostLabel']"/>
								</xsl:when>
								<xsl:otherwise>Shipping Cost:</xsl:otherwise>
							</xsl:choose>
						</td>
						<td class="shipping amount">
							<xsl:value-of select="$currency"/>
							<xsl:value-of select="format-number(@shippingCost,'0.00')"/>
						</td>
					</tr>
				</xsl:if>
				<xsl:if test="@vatRate &gt; 0">
					<tr>
						<td colspan="4">
							<!--xsl:attribute name="rowspan">
									<xsl:call-template name="calcRows">
										<xsl:with-param name="r1"><xsl:choose><xsl:when test="@vatRate &gt; 0">1</xsl:when><xsl:otherwise>0</xsl:otherwise></xsl:choose>	</xsl:with-param>
										<xsl:with-param name="r2"><xsl:choose><xsl:when test="@payableAmount &gt; 0">1</xsl:when><xsl:otherwise>0</xsl:otherwise></xsl:choose>	</xsl:with-param>
										<xsl:with-param name="r3"><xsl:choose><xsl:when test="@paymentMade &gt; 0">1</xsl:when><xsl:otherwise>0</xsl:otherwise></xsl:choose>	</xsl:with-param>
										<xsl:with-param name="r0">2</xsl:with-param>
									</xsl:call-template>
								</xsl:attribute-->
							&#160;
						</td>
						<td class="subTotal heading">
							Sub Total:
						</td>
						<td class="subTotal amount">
							<xsl:value-of select="$currency"/>
							<xsl:value-of select="format-number(@totalNet, '0.00')"/>
						</td>
					</tr>

					<tr>
						<td colspan="4">&#160;</td>
						<td class="vat heading">
							<xsl:choose>
								<xsl:when test="//Cart/Contact/Address/Country='United Kingdom'">VAT at </xsl:when>
								<xsl:otherwise>Tax at </xsl:otherwise>
							</xsl:choose>
							<xsl:value-of select="format-number(@vatRate, '#.00')"/>%:
						</td>
						<td class="vat amount">
							<span class="currency">
								<xsl:value-of select="$currency"/>
							</span>
							<xsl:value-of select="format-number(@vatAmt, '0.00')"/>
						</td>
					</tr>
				</xsl:if>
				<tr>
					<td colspan="4">&#160;</td>
					<td class="total heading">Total Value:</td>
					<td class="total amount">
						<xsl:value-of select="$currency"/>
						<xsl:value-of select="format-number(@total, '0.00')"/>
					</td>
				</tr>
				<xsl:if test="@paymentMade">
					<tr>
						<td colspan="4">&#160;</td>
						<td class="total heading">
							<xsl:choose>
								<xsl:when test="@transStatus">Transaction Made</xsl:when>
								<xsl:when test="@payableType='settlement' and not(@transStatus)">Payment Received</xsl:when>
							</xsl:choose>
						</td>
						<td class="total amount">
							<xsl:value-of select="$currency"/>
							<xsl:value-of select="format-number(@paymentMade, '0.00')"/>
						</td>
					</tr>
				</xsl:if>
				<xsl:if test="@payableAmount">
					<tr>
						<td colspan="4">&#160;</td>
						<td class="total heading">
							<xsl:choose>
								<xsl:when test="@payableType='deposit' and not(@transStatus)">Deposit Payable</xsl:when>
								<xsl:when test="@payableType='settlement' or (@payableType='deposit' and @transStatus)">Amount Outstanding</xsl:when>
							</xsl:choose>
						</td>
						<td class="total amount">
							<xsl:value-of select="$currency"/>
							<xsl:value-of select="format-number(@payableAmount, '0.00')"/>
						</td>
					</tr>
				</xsl:if>
				<tr>
					<td colspan="6">
						Client Notes:<br/>
						<xsl:for-each select="Notes/*">
							<p>
								<b>
									<xsl:value-of select="name()"/>:
								</b>
								<xsl:copy-of select="./node()"/>
							</p>
						</xsl:for-each>
					</td>
				</tr>
				<tr>
					<td colspan="6">
						Seller Notes:<br/>
						<xsl:copy-of select="SellerNotes/node()"/>
					</td>
				</tr>
			</table>
		</div>
	</xsl:template>
	<!-- -->
	<!--   ##################  Shipping Locations   ##############################   -->
	<!-- -->
	<xsl:template match="Page[@layout='ShippingLocations']" mode="Admin">
		<div id="tpltShippingLocations" class="row">
			<div class="col-md-3">
				<div class="alert alert-info">
					<i class="fa fa-info-sign fa-3x pull-right">
						<xsl:text> </xsl:text>
					</i>
					<h4>Hint</h4>
					<p>Shipping locations are heirarchical so you can set a shipping cost globally or by continent.</p>
					<p>You can also set the tax rate by country, once the user has selected delivery country during the cart process the correct tax rate will be allied</p>
				</div>
			</div>

			<div class="col-md-9">
				<div class="panel panel-default">
					<div class="panel-heading">

						<a href="{$appPath}?ewCmd=ShippingLocations&amp;ewcmd2=edit" class="btn btn-success pull-right btn-xs">
							<i class="fa fa-plus fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Add Root Location
						</a>
						<h3 class="panel-title">List of Locations</h3>
					</div>

					<ul id="MenuTree" class="list-group">
						<xsl:apply-templates select="ContentDetail/Tree/Tree/TreeItem" mode="treeShippingLocations">
							<xsl:with-param name="level">1</xsl:with-param>
						</xsl:apply-templates>
					</ul>

				</div>
			</div>
		</div>

	</xsl:template>

	<xsl:template match="TreeItem" mode="treeShippingLocations">
		<xsl:param name="level"/>
		<xsl:variable name ="class">
			<xsl:if test="TreeItem"> collapsable</xsl:if>
		</xsl:variable>
		<li id="node{@id}" data-tree-level="{$level}" data-tree-parent="{./parent::TreeItem/@id}" class="list-group-item level-{$level} {$class}">
			<div class="pageCell">
				<xsl:apply-templates select="." mode="status_legend"/>
				<xsl:value-of select="@Name"/>
			</div>
			<div class="optionButtons">
				<xsl:choose>
					<xsl:when test="/Page/Request/QueryString/Item[@name='ewcmd2']/node()='move'">
						<xsl:choose>
							<xsl:when test="/Page/Request/QueryString/Item[@name='id']/node()=@id">
								Moving...
							</xsl:when>
							<xsl:otherwise>
								<a href="{$appPath}?ewCmd=ShippingLocations&amp;ewcmd2=movehere&amp;parid={@id}&amp;id={/Page/Request/QueryString/Item[@name='id']/node()}" class="btn btn-primary">
									<i class="fa fa-move-here fa-white">
										<xsl:text> </xsl:text>
									</i><xsl:text> </xsl:text>Move Here
								</a>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:when>
					<xsl:otherwise>
						<a href="{$appPath}?ewCmd=ShippingLocations&amp;ewcmd2=edit&amp;id={@id}" class="btn btn-xs btn-primary">
							<i class="fa fa-pencil fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Edit Location
						</a>
						<a href="{$appPath}?ewCmd=ShippingLocations&amp;ewcmd2=edit&amp;parid={@id}" class="btn btn-xs btn-primary">
							<i class="fa fa-plus fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Add Child Location
						</a>
						<a href="{$appPath}?ewCmd=ShippingLocations&amp;ewcmd2=move&amp;id={@id}" class="btn btn-xs btn-default">
							<i class="fa fa-share-alt fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Move
						</a>
						<a href="{$appPath}?ewCmd=ShippingLocations&amp;ewcmd2=delete&amp;id={@id}" class="btn btn-xs btn-danger">
							<i class="fa fa-trash-o fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Delete
						</a>
					</xsl:otherwise>
				</xsl:choose>
			</div>
		</li>
		<xsl:if test="TreeItem">

			<xsl:apply-templates select="TreeItem" mode="treeShippingLocations">
				<xsl:with-param name="level">
					<xsl:value-of select="$level + 1"/>
				</xsl:with-param>
			</xsl:apply-templates>

		</xsl:if>
	</xsl:template>


	<!-- -->
	<!--   ##################  Delivery Methods   ##############################   -->
	<!-- -->
	<xsl:template match="Page[@layout='DeliveryMethods']" mode="Admin">
		<div class="row" id="tpltDeliveryMethods">
			<div class="col-md-3" id="column1">
				<div class="alert alert-info">
					<i class="fa fa-info-sign fa-3x pull-right">
						<xsl:text> </xsl:text>
					</i>
					<h4>Hint</h4>
					You can create all of the delivery methods you need to ship products from your site.<br/><br/>Make sure each delivery method specifies the locations that it is valid for.<br/><br/>Delviery methods can be setup to be only available to certain users groups, if this is empty the delivery method is by default available to all users.
				</div>
			</div>
			<div class="col-md-9" id="column2">
				<div class="panel panel-default" >
					<div class="panel-heading">
						<a href="{$appPath}?ewCmd=DeliveryMethods&amp;ewcmd2=edit" class="btn btn-xs btn-success pull-right">
							<i class="fa fa-plus fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Add New Delivery Method
						</a>
						<h3 class="panel-title">Manage Delivery Methods</h3>
					</div>

					<!--a href="{$appPath}?ewCmd=DeliveryMethods&amp;ewcmd2=editgroup" class="adminButton add">Add New Delivery Group</a-->
					<table class="table table-striped">
						<xsl:for-each select="ContentDetail/List/ListItem">
							<xsl:sort select="@name" order="ascending" data-type="text"/>
							<xsl:apply-templates select="." mode="listDeliveryMethods"/>
						</xsl:for-each>
					</table>

				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="ListItem" mode="listDeliveryMethods">
		<xsl:param name="level"/>
		<tr>
			<td>
				<xsl:apply-templates select="." mode="status_legend"/>
			</td>
			<td>
				<xsl:value-of select="@name"/> - <xsl:value-of select="@carrier"/>
				<!--BJR CURRENCY-->
				<xsl:choose>
					<xsl:when test="@cCurrency and @cCurrency!=''">
						- (<xsl:value-of select="@cCurrency"/>)
					</xsl:when>
					<xsl:otherwise>
						- (All Currencies)
					</xsl:otherwise>
				</xsl:choose>
			</td>
			<td class="options">
				<a href="{$appPath}?ewCmd=DeliveryMethods&amp;ewcmd2=edit&amp;id={@id}" class="btn btn-xs btn-success">
					<i class="fa fa-pencil fa-white">
						<xsl:text> </xsl:text>
					</i><xsl:text> </xsl:text>Edit Method
				</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd=DeliveryMethods&amp;ewcmd2=locations&amp;id={@id}" class="btn btn-xs btn-primary">
					<i class="fa fa-globe fa-white">
						<xsl:text> </xsl:text>
					</i><xsl:text> </xsl:text>Set Locations
				</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd=DeliveryMethods&amp;ewcmd2=permissions&amp;id={@id}" class="btn btn-xs btn-primary">
					<i class="fa fa-user fa-white">
						<xsl:text> </xsl:text>
					</i><xsl:text> </xsl:text>Select User Groups
				</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd=DeliveryMethods&amp;ewcmd2=ShippingGroup&amp;id={@id}&amp;name={@name}" class="btn btn-xs btn-primary">
					<i class="fa fa-user fa-white">
						<xsl:text> </xsl:text>
					</i><xsl:text> </xsl:text>Select Shipping Group
				</a>
				<a href="{$appPath}?ewCmd=DeliveryMethods&amp;ewcmd2=delete&amp;id={@id}" class="btn btn-xs btn-danger">
					<i class="fa fa-trash-o fa-white">
						<xsl:text> </xsl:text>
					</i><xsl:text> </xsl:text>Delete Method
				</a>
				<xsl:text> </xsl:text>

			</td>
		</tr>
	</xsl:template>

	<!-- -->
	<!--   ##################  Delivery Methods   ##############################   -->
	<!-- -->
	<xsl:template match="Page[@layout='Carriers']" mode="Admin">
		<div class="row" id="tpltCarrierss">
			<div class="col-md-3" id="column1">
				<div class="alert alert-info">
					<i class="fa fa-info-sign fa-3x pull-right">
						<xsl:text> </xsl:text>
					</i>
					<h4>Hint</h4>
					Carriers are the shipping companies that you use, when you ship a completed order you can select one of these and send information of a tracking link to your customer.
				</div>
			</div>
			<div class="col-md-9" id="column2">
				<div class="panel panel-default" >
					<div class="panel-heading">
						<a href="{$appPath}?ewCmd=Carriers&amp;ewcmd2=edit" class="btn btn-xs btn-success pull-right">
							<i class="fa fa-plus fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Add New Carrier
						</a>
						<h3 class="panel-title">Manage Carriers</h3>
					</div>

					<!--a href="{$appPath}?ewCmd=DeliveryMethods&amp;ewcmd2=editgroup" class="adminButton add">Add New Delivery Group</a-->
					<table class="table table-striped">
						<xsl:for-each select="ContentDetail/Carriers/Carrier">
							<xsl:sort select="name" order="ascending" data-type="text"/>
							<tr>
								<td>
									<xsl:value-of select="name/node()"/>
								</td>
								<td class="options">
									<a href="{$appPath}?ewCmd=Carriers&amp;ewcmd2=edit&amp;id={@id}" class="btn btn-xs btn-success">
										<i class="fa fa-pencil fa-white">
											<xsl:text> </xsl:text>
										</i><xsl:text> </xsl:text>Edit Carrier
									</a>
									<xsl:text> </xsl:text>
									<a href="{$appPath}?ewCmd=Carriers&amp;ewcmd2=delete&amp;id={@id}" class="btn btn-xs btn-danger">
										<i class="fa fa-trash-o fa-white">
											<xsl:text> </xsl:text>
										</i><xsl:text> </xsl:text>Delete Carrier
									</a>
								</td>
							</tr>
						</xsl:for-each>
					</table>

				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Page[@layout='DeliveryMethodLocations']" mode="Admin">
		<div id="tpltDeliveryMethodLocations" class="row">
			<div class="col-md-3">
				<div class="alert alert-info">
					<i class="fa fa-info-sign fa-3x pull-right">
						<xsl:text> </xsl:text>
					</i>
					<h4>Hint</h4>
					<p>Shipping locations are heirarchical so you can set a shipping cost globally, by continent or by country.</p>
				</div>
			</div>
			<div class="col-md-9">
				<div class="panel panel-default">
					<div class="panel-heading">
						<h3 class="panel-title">List of Locations</h3>
					</div>
					<form class="ewXform" action="?ewcmd=DeliveryMethods&amp;ewcmd2=locations&amp;id={/Page/Request/QueryString/Item[@name='id']/node()}" method="post">
						<div class="form-group">
							<input type="hidden" name="nShpOptId" value="{/Page/Request/QueryString/Item[@name='id']/node()}"/>
							<ul id="MenuTree" class="treeview">
								<xsl:apply-templates select="ContentDetail/Tree/Tree/TreeItem" mode="ListLocationsForm"/>
							</ul>
						</div>
						<div class="form-actions">
							<button type="submit" name="ewSubmit" value="Submit" class="btn btn-success principle">
								<i class="fa fa-ok fa-white">
									<xsl:text> </xsl:text>
								</i>
								<xsl:text> </xsl:text>
								Save Selections
							</button>
							<br/>
						</div>
					</form>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="TreeItem" mode="ListLocationsForm">
		<li id="node{@id}" class="checkbox">

			<input type="checkbox" id="loc-{@id}" name="aLocations" value="{@id}" onclick="checkShippingLocationsForm(this)">
				<xsl:if test="@selected = '1'">
					<xsl:attribute name="checked">true</xsl:attribute>
				</xsl:if>
			</input>
			<label class="treeNode" for="loc-{@id}">
				<xsl:value-of select="@Name"/>
			</label>
			<xsl:if test="TreeItem">
				<ul>
					<xsl:apply-templates select="TreeItem" mode="ListLocationsForm"/>
				</ul>
			</xsl:if>
		</li>
	</xsl:template>

	<!-- -->
	<!--   ##################  Report   ##############################   -->
	<!-- -->
	<xsl:template match="Page[@layout='Report']" mode="Admin">
		<div class="template" id="template_Report">
			<div id="column1 report">
				<div class="btn-group headerButtons">
					<xsl:choose>
						<xsl:when test="/Page/Request/Form/Item[@name='startDate']">
							<a href="/ewcommon/tools/excel.ashx?{/Page/Request/ServerVariables/Item[@name='QUERY_STRING']/node()}&amp;startDate={/Page/Request/Form/Item[@name='startDate']}" class="excel adminButton" target="_new">Excel Download</a>
						</xsl:when>
						<xsl:otherwise>
							<a href="/ewcommon/tools/excel.ashx?{/Page/Request/ServerVariables/Item[@name='QUERY_STRING']/node()}" class="excel adminButton" target="_new">Excel Download</a>
						</xsl:otherwise>
					</xsl:choose>
				</div>
				<xsl:apply-templates select="ContentDetail/report" mode="reportDetail"/>
			</div>
			<!-- Terminus class fix to floating columns -->
			<div class="terminus">&#160;</div>
		</div>
	</xsl:template>
	<!-- Default Report Template Sortable Columns -->

	<xsl:template match="report/course | report/activity | report/certificates" mode="reportDetailListTotals"/>


	<xsl:template match="section" mode="reportDetailListHeader">
		<tr>
			<th>
				List Activity For Company: <xsl:value-of select="parent::activity/@companyName"/>
			</th>
			<th>
				<form action="?{/Page/Request/ServerVariables/Item[@name='QUERY_STRING']/node()}" method="post" id="linksDD" class="monthSelector">
					<label for="nMonthId">List by Month</label>
					<select name="nMonthId" id="links" onchange="javascript:this.form.submit();" >
						<option value="">Total </option>
						<xsl:for-each select="parent::activity/monthSelector/monthSelect">
							<option value="{@id}">
								<xsl:if test="@id=ancestor::activity/@deptId">
									<xsl:attribute name="selected">selected</xsl:attribute>
								</xsl:if>
								<xsl:value-of select="@name"/>
							</option>
						</xsl:for-each>
					</select>
				</form>
			</th>
		</tr>
		<tr>
			<xsl:apply-templates select="*" mode="reportHeader"/>
		</tr>
	</xsl:template>

	<xsl:template match="section" mode="reportDetailList">
		<tr>
			<xsl:if test="(position()+1) mod 2=0">
				<xsl:attribute name="class">alternate</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="*" mode="reportCell"/>
		</tr>
	</xsl:template>

	<xsl:template match="Page[@layout='PaymentProviders']" mode="Admin">
		<div class="row" id="tpltPaymentProviders">
			<div class="col-md-3">
				<div class="panel panel-default">
					<div class="panel-heading">
						<h3 class="title">Information</h3>
					</div>
					<div class="panel-body">
						<p>Here you can edit the settings how your site collects payment</p>
					</div>
				</div>
			</div>
			<div class="col-md-9">
				<div class="panel panel-default">
					<table cellpadding="0" cellspacing="0" class="table">
						<tbody>
							<tr>
								<th>Payment Provider</th>
							</tr>
							<xsl:for-each select="ContentDetail/List/Provider">
								<tr>
									<td>
										<xsl:value-of select="node()"/>
									</td>
									<td class="clearfix">
										<xsl:choose>
											<xsl:when test="@active='true'">
												<a href="{$appPath}?ewCmd=PaymentProviders&amp;ewCmd2=delete&amp;type={node()}" class="btn btn-xs btn-danger pull-right">
													<i class="fa fa-trash-o fa-white">
														<xsl:text> </xsl:text>
													</i><xsl:text> </xsl:text>Delete
												</a>
												<a href="{$appPath}?ewCmd=PaymentProviders&amp;ewCmd2=edit&amp;type={node()}" class="btn btn-xs btn-primary pull-right">
													<i class="fa fa-pencil fa-white">
														<xsl:text> </xsl:text>
													</i><xsl:text> </xsl:text>Edit
												</a>

											</xsl:when>
											<xsl:otherwise>
												<a href="{$appPath}?ewCmd=PaymentProviders&amp;ewCmd2=add&amp;type={node()}" class="btn btn-xs btn-default pull-right">
													<i class="fa fa-plus fa-white">
														<xsl:text> </xsl:text>
													</i><xsl:text> </xsl:text>Add
												</a>
											</xsl:otherwise>
										</xsl:choose>
									</td>
								</tr>
							</xsl:for-each>
						</tbody>
					</table>
				</div>
			</div>


		</div>
	</xsl:template>

	<xsl:template match="Page[@layout='Reports']" mode="Admin">
		<div id="tpltListReports" class="row">
			<div class="col-md-3">
				<div class="panel panel-default">
					<div class="panel-body">
						<p>
							<xsl:call-template name="eonicwebProductName"/>allows for sophisticated reports to be developed and deployed for whatever you might need.
						</p>
						<p>If you require additional reports please contact your website developer for a quote.</p>
					</div>
				</div>
			</div>
			<div class="col-md-9">

				<div class="panel panel-default">
					<table class="table">
						<tbody>
							<xsl:for-each select="ContentDetail/List/Report">
								<tr>
									<td>
										<xsl:value-of select="node()"/>
									</td>
									<td>
										<a href="{$appPath}?ewCmd=Reports&amp;ewCmd2={@type}" class="btn btn-xs btn-primary pull-right">
											<i class="fa fa-chevron-circle-right">
												&#160;
											</i>&#160;Run Report
										</a>
									</td>
								</tr>
							</xsl:for-each>
						</tbody>
					</table>

				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Page[@layout='Reports' and ContentDetail/Content[@type='xform']]" mode="Admin">
		<!--<div class="report" id="template_AdminXForm">-->
		<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
		<xsl:apply-templates select="ContentDetail/Report" mode="defaultReport"/>
		<!--</div>-->
	</xsl:template>

	<xsl:template match="Report" mode="defaultReport">
		<table class="table">
			<tr>
				<xsl:apply-templates select="Item[1]/descendant-or-self::*" mode="Report_ColsHeading"/>
			</tr>
			<xsl:for-each select="Item">
				<span class="advancedModeRow" onmouseover="this.className='rowOver'" onmouseout="this.className='advancedModeRow'">
					<tr>

						<xsl:apply-templates select="descendant-or-self::*" mode="Report_ColsValues"/>

					</tr>
				</span>
			</xsl:for-each>
		</table>
	</xsl:template>

	<xsl:template match="Report[@name='MailFormSubmissions']" mode="defaultReport">
		<table class="table">
			<tr>
				<th>
					Date/Time
				</th>
				<xsl:apply-templates select="Item[last()]/cActivityXml/descendant-or-self::*" mode="Report_ColsHeading"/>
				<th>
					Refering Page
				</th>
			</tr>
			<xsl:for-each select="Item">
				<tr>
					<xsl:apply-templates select="DateTime" mode="Report_ColsValues"/>
					<xsl:apply-templates select="cActivityXml/descendant-or-self::*" mode="Report_ColsValues"/>
					<td>
						<small>
							<xsl:value-of select="cActivityXml/Items/@sessionReferrer"/>
						</small>
					</td>
				</tr>
			</xsl:for-each>
		</table>
	</xsl:template>

	<xsl:template match="*" mode ="Report_ColsHeading">
		<xsl:if test="count(*)=0">
			<th>
				<xsl:value-of select="local-name()"/>
			</th>
		</xsl:if>
	</xsl:template>

	<xsl:template match="Attachments" mode ="Report_ColsHeading">
		<th>
			Document Title
		</th>
	</xsl:template>

	<xsl:template match="Attachment" mode ="Report_ColsHeading">

	</xsl:template>


	<xsl:template match="Content[parent::Attachment]" mode ="Report_ColsHeading">

	</xsl:template>

	<xsl:template match="Attachment" mode ="Report_ColsValues">

	</xsl:template>

	<xsl:template match="*" mode ="Report_ColsValues">
		<xsl:if test="count(*)=0">
			<td>
				<xsl:value-of select="node()"/>
			</td>
		</xsl:if>
	</xsl:template>

	<xsl:template match="*[local-name()='AttachmentIds']" mode ="Report_ColsValues">
		<td>
			<xsl:value-of select="@ids"/>
		</td>
	</xsl:template>

	<xsl:template match="Attachements | Attachments" mode ="Report_ColsValues">
		<td>
			<xsl:for-each select="Attachement | Attachment">
				<xsl:value-of select="Content/@name"/>
			</xsl:for-each>
		</td>
	</xsl:template>


	<!-- -->
	<!--   ##################  Generic Display Form  ##############################   -->
	<!-- -->
	<xsl:template match="Page[@layout='AdminXForm']" mode="Admin">
		<div class="adminTemplate" id="template_AdminXForm">
			<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
			<xsl:apply-templates select="ContentDetail/Content[contains(@type,'xFormQuiz')]" mode="edit"/>
		</div>
	</xsl:template>

	<xsl:template match="Page[@layout='AdminXForm' and ContentDetail/Content[@name='UserLogon']]" mode="Admin">
		<div class="userLogon" id="template_AdminXForm">
			<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
		</div>
	</xsl:template>



	<!-- -->
	<!-- Generic Steppers-->
	<xsl:template match="/" mode="genericStepperBasic">
		<xsl:param name="curPg"/>
		<xsl:param name="prevItem"/>
		<xsl:param name="nextItem"/>
		<xsl:variable name="parentURL">
			<xsl:call-template name="getContentParURL"/>
		</xsl:variable>
		<div class="stepper">
			<xsl:choose>
				<xsl:when test="/Page/Contents/Content[contains(@name,$prevItem)]">
					<a href="{$parentURL}?curPg={number($curPg) - 1}" title="go to the previous page">&lt; previous</a>
				</xsl:when>
				<xsl:otherwise>
					<span class="ghosted">&lt; previous</span>
				</xsl:otherwise>
			</xsl:choose> |
			<xsl:choose>
				<xsl:when test="/Page/Contents/Content[contains(@name,$nextItem)]">
					<a href="{$parentURL}?curPg={number($curPg) + 1}" title="go to the next page">next &gt;</a>
				</xsl:when>
				<xsl:otherwise>
					<span class="ghosted">next &gt;</span>
				</xsl:otherwise>
			</xsl:choose>
		</div>
	</xsl:template>




	<!-- Alpha Stepper-->
	<xsl:template match="/" mode="alphaStepper">
		<xsl:param name="ewCmd"/>
		<xsl:param name="label"/>
		<xsl:param name="querystringAmendment"/>

		<div class="btn-toolbar">
			<div class="btn-group">
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;{$querystringAmendment}" class="all btn btn-default btn-sm">
					<xsl:text>All </xsl:text>
					<xsl:value-of select="$label"/>
				</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=A&amp;{$querystringAmendment}" class="btn btn-default btn-sm">A</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=B&amp;{$querystringAmendment}" class="btn btn-default btn-sm">B</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=C&amp;{$querystringAmendment}" class="btn btn-default btn-sm">C</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=D&amp;{$querystringAmendment}" class="btn btn-default btn-sm">D</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=E&amp;{$querystringAmendment}" class="btn btn-default btn-sm">E</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=F&amp;{$querystringAmendment}" class="btn btn-default btn-sm">F</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=G&amp;{$querystringAmendment}" class="btn btn-default btn-sm">G</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=H&amp;{$querystringAmendment}" class="btn btn-default btn-sm">H</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=I&amp;{$querystringAmendment}" class="btn btn-default btn-sm">I</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=J&amp;{$querystringAmendment}" class="btn btn-default btn-sm">J</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=K&amp;{$querystringAmendment}" class="btn btn-default btn-sm">K</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=L&amp;{$querystringAmendment}" class="btn btn-default btn-sm">L</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=M&amp;{$querystringAmendment}" class="btn btn-default btn-sm">M</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=N&amp;{$querystringAmendment}" class="btn btn-default btn-sm">N</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=O&amp;{$querystringAmendment}" class="btn btn-default btn-sm">O</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=P&amp;{$querystringAmendment}" class="btn btn-default btn-sm">P</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=Q&amp;{$querystringAmendment}" class="btn btn-default btn-sm">Q</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=R&amp;{$querystringAmendment}" class="btn btn-default btn-sm">R</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=S&amp;{$querystringAmendment}" class="btn btn-default btn-sm">S</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=T&amp;{$querystringAmendment}" class="btn btn-default btn-sm">T</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=U&amp;{$querystringAmendment}" class="btn btn-default btn-sm">U</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=V&amp;{$querystringAmendment}" class="btn btn-default btn-sm">V</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=W&amp;{$querystringAmendment}" class="btn btn-default btn-sm">W</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=X&amp;{$querystringAmendment}" class="btn btn-default btn-sm">X</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=Y&amp;{$querystringAmendment}" class="btn btn-default btn-sm">Y</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=Z&amp;{$querystringAmendment}" class="btn btn-default btn-sm">Z</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=0&amp;{$querystringAmendment}" class="btn btn-default btn-sm">0</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=1&amp;{$querystringAmendment}" class="btn btn-default btn-sm">1</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=2&amp;{$querystringAmendment}" class="btn btn-default btn-sm">2</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=3&amp;{$querystringAmendment}" class="btn btn-default btn-sm">3</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=4&amp;{$querystringAmendment}" class="btn btn-default btn-sm">4</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=5&amp;{$querystringAmendment}" class="btn btn-default btn-sm">5</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=6&amp;{$querystringAmendment}" class="btn btn-default btn-sm">6</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=7&amp;{$querystringAmendment}" class="btn btn-default btn-sm">7</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=8&amp;{$querystringAmendment}" class="btn btn-default btn-sm">8</a>
				<xsl:text> </xsl:text>
				<a href="{$appPath}?ewCmd={$ewCmd}&amp;LastNameStarts=9&amp;{$querystringAmendment}" class="btn btn-default btn-sm">9</a>
				<xsl:text> </xsl:text>
			</div>
		</div>
	</xsl:template>
	<!-- -->
	<xsl:template match="/" mode="adminStepper">
		<xsl:param name="itemCount"/>
		<xsl:param name="itemTotal"/>
		<xsl:param name="startPos"/>
		<xsl:param name="path"/>
		<xsl:param name="itemName"/>
		<div class="stepper">
			<span class="itemInfo">
				<xsl:value-of select="$startPos + 1"/> to
				<xsl:if test="$itemTotal &gt;= ($startPos +$itemCount)">
					<xsl:value-of select="$startPos + $itemCount"/>
				</xsl:if>
				<xsl:if test="$itemTotal &lt; ($startPos + $itemCount)">
					<xsl:value-of select="$itemTotal"/>
				</xsl:if> of <xsl:value-of select="$itemTotal"/>
				&#160;<xsl:value-of select="$itemName"/>
			</span>
			<span class="stepLinks">
				<xsl:choose>
					<xsl:when test="$startPos &gt; ($itemCount - 1)">
						<a href="{$path}&amp;startPos={$startPos - $itemCount}" title="click here to view the previous page in sequence" class="btn btn-primary">
							<i class="fa fa-chevron-left fa-white">
								<xsl:text> </xsl:text>
							</i> Back
						</a>
					</xsl:when>
					<xsl:otherwise>
						<span class="btn btn-primary disabled">
							<i class="fa fa-chevron-left fa-white">
								<xsl:text> </xsl:text>
							</i> Back
						</span>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:for-each select="User">
					<xsl:choose>
						<xsl:when test="position()-$itemCount=$startPos">
							<xsl:value-of select="position() div $itemCount"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:if test="(position() ) mod $itemCount = '0'">
								<a href="{$path}&amp;startPos={position()-$itemCount}">
									<xsl:value-of select="position() div $itemCount"/>
								</a> |
							</xsl:if>
							<xsl:if test="(position()) = $itemTotal and ceiling(position() div $itemCount)!=(position() div $itemCount)">
								<xsl:choose>
									<xsl:when test="$startPos+$itemCount=(ceiling((position()) div $itemCount)*$itemCount)">
										<xsl:value-of select="ceiling(position() div $itemCount)"/> |
									</xsl:when>
									<xsl:otherwise>
										<a href="{$path}&amp;startPos={position()-(position() mod $itemCount)}">
											<xsl:value-of select="ceiling(position() div $itemCount)"/>
										</a> |
									</xsl:otherwise>
								</xsl:choose>
							</xsl:if>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:for-each>
				<xsl:choose>
					<xsl:when test="$itemTotal &gt; ($startPos +$itemCount)">
						<a href="{$path}&amp;startPos={$startPos+$itemCount}" title="click here to view the next page in sequence" class="btn btn-primary">
							Next <i class="fa fa-chevron-right fa-white">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:when>
					<xsl:otherwise>
						<span class="btn btn-primary disabled">
							Next <i class="fa fa-chevron-right fa-white">
								<xsl:text> </xsl:text>
							</i>
						</span>
					</xsl:otherwise>
				</xsl:choose>
			</span>

		</div>
	</xsl:template>



	<!-- BJR -->
	<!--   ##################  NewsLetter    ##############################   -->
	<!-- -->
	<xsl:variable name="MailRoot">
		<xsl:value-of select="/Page/Menu[@id='Newsletter']/MenuItem/@id"/>
	</xsl:variable>

	<xsl:template match="Page[@layout='MailingList']" mode="Admin">
		<div class="row" id="template_EditStructure">
			<div class="col-md-3">
				<div class="panel panel-default">
					<div class="panel-heading">
						<h3 class="panel-title">Instructions</h3>
					</div>
					<div class="panel-body">
						<p>This is where you manage all your Newsletters</p>
						<p>
							<a class="btn btn-sm btn-success">
								<i class="fa fa-envelope">
									<xsl:text> </xsl:text>
								</i>&#160;Add New Campaign
							</a> - create a new newsletter
						</p>
						<p>
							<a class="btn btn-xs btn-primary">
								<i class="fa fa-edit">&#160;</i>&#160;View / Edit
							</a> - view and edit the newsletter
						</p>
						<p>
							<a class="btn btn-xs btn-default">
								<i class="fa fa-eye">&#160;</i>&#160;Preview
							</a> - send a single copy of the newsletter to your chosen email address
						</p>
						<p>
							<a class="btn btn-xs btn-success">
								<i class="fa fa-envelope">&#160;</i>&#160;Send
							</a> - send the newsletter to your mailing list
						</p>
						<p>
							<a class="btn btn-xs btn-danger">
								<i class="fa fa-trash-o">&#160;</i>&#160;Delete
							</a> - delete newsletter
						</p>
					</div>
				</div>
			</div>
			<div class="col-md-9 ">
				<div class="panel panel-default">
					<div class="panel-header">
						<h3 class="panel-title">&#160;</h3>
						<a href="{$appPath}?ewCmd=NewMail&amp;parId={$MailRoot}" class="btn btn-sm btn-success pull-right">
							<i class="fa fa-envelope">
								<xsl:text> </xsl:text>
							</i>&#160;Add New Campaign
						</a>
					</div>
					<table class="table">
						<tr>
							<th>Title</th>
							<th>Options</th>
						</tr>
						<xsl:for-each select="/Page/Menu[@id='Newsletter']/MenuItem/MenuItem">
							<xsl:sort select="Sent"/>
							<tr>
								<td>
									<xsl:value-of select="@name"/>
								</td>
								<td>
									<a href="/{$appPath}?ewCmd=NormalMail&amp;pgId={@id}&amp;parId={$MailRoot}" class="btn btn-xs btn-primary">
										<i class="fa fa-edit">&#160;</i>&#160;View / Edit
									</a>
									<a href="/{$appPath}?ewCmd=CopyPage&amp;pgId={@id}&amp;parId={$MailRoot}" class="btn btn-xs btn-primary">
										<i class="fa fa-copy">&#160;</i>&#160;Copy
									</a>
									<a href="/{$appPath}?ewCmd=PreviewMail&amp;pgId={@id}&amp;parId={$MailRoot}" class="btn btn-xs btn-default">
										<i class="fa fa-eye">&#160;</i>&#160;Preview
									</a>
									<a href="/{$appPath}?ewCmd=SendMail&amp;pgId={@id}&amp;parId={$MailRoot}" class="btn btn-xs btn-success">
										<i class="fa fa-envelope">&#160;</i>&#160;Send
									</a>
									<a href="/{$appPath}?ewCmd=DeletePageMail&amp;pgId={@id}&amp;parId={$MailRoot}" class="btn btn-xs btn-danger">
										<i class="fa fa-trash-o">&#160;</i>&#160;Delete
									</a>
								</td>
							</tr>
							<xsl:for-each select="/Page/Menu[@id='Newsletter']/MenuItem/MenuItem/MenuItem">
								<tr>
									<td>
										&#160;&#160;&#160;<xsl:value-of select="@name"/>
									</td>
									<td>
										<a href="/{$appPath}?ewCmd=NormalMail&amp;pgId={@id}&amp;parId={$MailRoot}" class="btn btn-success">View / Edit</a>
										<a href="/{$appPath}?ewCmd=PreviewMail&amp;pgId={@id}&amp;parId={$MailRoot}" class="adminButton show">Preview</a>
										<a href="/{$appPath}?ewCmd=SendMail&amp;pgId={@id}&amp;parId={$MailRoot}" class="adminButton move">Send</a>
										<a href="/{$appPath}?ewCmd=DeletePageMail&amp;pgId={@id}&amp;parId={$MailRoot}" class="adminButton delete">Delete</a>
									</td>
								</tr>
							</xsl:for-each>
						</xsl:for-each>
					</table>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Page[@layout='NewMail' or @layout='AddMailModule' or @layout='EditMailContent']" mode="Admin">
		<div class="adminTemplate" id="template_AdminXForm">
			<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
		</div>
	</xsl:template>

	<xsl:template match="Page[@layout='PreviewMail'] | Page[@layout='MailOptOut'] | Page[@layout='SendMail']" mode="Admin">
		<div class="report" id="template_EditStructure">
			<div id="column2" class="reportSet">
				<xsl:for-each select="ContentDetail/Content[@type='Message']">

					<div class="alert alert-info">
						<i class="fa fa-check fa-2x pull-left">
							<xsl:text> </xsl:text>
						</i>
						<xsl:value-of select="node()"/>
					</div>
				</xsl:for-each>
				<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>

			</div>
			<div id="column1" class="reportSet">
				Instructions go here
			</div>
			<!-- Terminus class fix to floating columns -->
			<div class="terminus">&#160;</div>
		</div>
	</xsl:template>
	<!-- -->
	<xsl:template match="Page[@ewCmd='SendMail']" mode="Admin">
		<div class="row" id="template_EditStructure">
			<div class="col-md-3">
				<div class="panel panel-default">
					<div class="panel-heading">
						<h3 class="panel-title">Sending Options</h3>
					</div>
					<div class="panel-body">
						<p>Configure the sending options, and the membership groups you wish to send to.</p>
					</div>
				</div>
			</div>
			<div class="col-md-9">
				<xsl:for-each select="ContentDetail/Content[@type='Message']">
					<div class="group">
						<h3>Result</h3>
					</div>
					<div class="alert alert-info">
						<i class="fa fa-check fa-2x pull-left">
							<xsl:text> </xsl:text>
						</i>
						<xsl:value-of select="node()"/>
					</div>
				</xsl:for-each>
				<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
				<xsl:apply-templates select="ContentDetail/Report" mode="CampaignReport"/>
			</div>
		</div>
	</xsl:template>
	<!-- -->
	<!-- -->
	<xsl:template match="Page[@layout='PreviewMail' and @ewCmd='PreviewMail']" mode="Admin">
		<div class="row" id="template_EditStructure">
			<div class="col-md-3">
				<div class="panel panel-default">
					<div class="panel-heading">
						<h3 class="panel-title">Send Preview</h3>
					</div>
					<div class="panel-body">
						<p>Send a single copy of the newsletter to your own email address to test that the email sent out is correct.</p>
					</div>
				</div>
			</div>
			<div class="col-md-9">
				<xsl:choose>
					<xsl:when test="ContentDetail/Content[@type='Message']">
						<xsl:for-each select="ContentDetail/Content[@type='Message']">
							<div class="alert alert-info">
								<i class="fa fa-check fa-2x pull-left">
									<xsl:text> </xsl:text>
								</i>
								&#160;
								<xsl:value-of select="node()"/>
							</div>
						</xsl:for-each>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="/Page/ContentDetail/Content[@type='xform']" mode="xform"/>
					</xsl:otherwise>
				</xsl:choose>
			</div>
		</div>
	</xsl:template>
	<!-- -->
	<xsl:template match="Page[@layout='OptOut' and @ewCmd='MailOptOut']" mode="Admin">
		<div class="row">
			<div class="col-md-3">
				<div class="panel">
					<div class="panel-heading">
						<h3 class="panel-title">Newsletter Opt-Outs</h3>
					</div>
					<div class="panel-body">
						<p>Use the Add to List feature to add an email address to the opt-out list.</p>
						<p>Use the Remove from list feature to remove an email address from the opt-out list.</p>
						<p>
							Opt-ed out email addresses are email addresses that are:
							<ul>
								<li>Invalid email addresses</li>
								<li>Of Users who do not wish recieve a newsletter but are members of a user group</li>
							</ul>
						</p>
					</div>
				</div>
			</div>
			<div class="col-md-9">
				<div class="panel">
					<div class="panel-body">
						<xsl:for-each select="ContentDetail/Content[@type='Message']">
							<div class="group">
								<h3>Result</h3>
							</div>
							<xsl:value-of select="node()"/>
						</xsl:for-each>
						<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
					</div>
				</div>
			</div>

		</div>
	</xsl:template>
	<!-- -->
	<xsl:template match="Page[@layout='MailHistory']" mode="Admin">
		<div class="row" id="template_EditStructure">
			<div class="col-md-3">
				<div class="panel panel-default">
					<div class="panel-heading">
						<h3 class="panel-title">Overview</h3>
					</div>
					<div class="panel-body">
						<p>A History of all Newsletters and the Mailing Lists they were sent to.</p>
					</div>
				</div>
			</div>
			<div class="col-md-9">
				<div class="panel panel-default">
					<div class="panel-heading">
						<h3 class="panel-title">Mailing History</h3>
					</div>
					<table class="table panel-body">
						<tr>
							<th>Item Name</th>
							<th>Sent</th>
							<th>Sent By</th>
							<th>To Groups</th>
						</tr>
						<xsl:for-each select="/Page/ContentDetail/ActivityLog/Activity">
							<xsl:sort select="Sent"/>
							<tr>
								<td>
									<xsl:value-of select="@cStructName"/>
								</td>
								<td>
									<xsl:call-template name="DD_MM_YY">
										<xsl:with-param name="date">
											<xsl:value-of select="@dDateTime"/>
										</xsl:with-param>
									</xsl:call-template>
									<xsl:call-template name="HH_MM">
										<xsl:with-param name="date">
											<xsl:value-of select="@dDateTime"/>
										</xsl:with-param>
									</xsl:call-template>
								</td>
								<td>
									<xsl:value-of select="@cDirName"/>
								</td>
								<td>
									<xsl:call-template name="listSentGroups">
										<xsl:with-param name="grpString" select="cActivityDetail/node()"/>
									</xsl:call-template>
								</td>
							</tr>
						</xsl:for-each>
					</table>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template name="listSentGroups">
		<xsl:param name="grpString" />
		<xsl:choose>
			<xsl:when test="contains($grpString,',')">
				<xsl:variable name="newGrpString" select="substring-before($grpString,',')"/>
				<xsl:value-of select="/Page/ContentDetail/ActivityLog/Group[@nDirKey=$newGrpString]/@cDirName"/>
				<xsl:text>, </xsl:text>
				<xsl:call-template name="listSentGroups">
					<xsl:with-param name="grpString" select="substring-after($grpString,',')"/>
				</xsl:call-template>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="/Page/ContentDetail/ActivityLog/Group[@nDirKey=$grpString]/@cDirName"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!--    -->

	<!--BJR Scheduler-->
	<xsl:template match="Page[@layout='ScheduledItems']" mode="Admin">
		<div class="row" id="template_EditStructure">
			<div class="col-md-3">
				<div class="panel panel-default">
					<div class="panel-heading">
						<h3 class="panel-title">Instructions</h3>
					</div>
					<div class="panel-body">
						<p>
							Scheduled items are tasks that can be scheduled to run on a periodic basis, They can update indexes, collect inbound feeds or send email updates. <br/><br/>Please call Support for more information.
						</p>
					</div>
				</div>
			</div>
			<div class="relatedSearchContent col-md-9">
				<div class="panel panel-default">
					<div class="panel-heading">
						<h3 class="panel-title">Scheduled Items</h3>
					</div>
					<div class="panel-body">
						<xsl:for-each select="/Page/ContentDetail/Content[@type='ActionList']/Item">
							<xsl:sort select="node()"/>
							<xsl:variable name="cActionType" select="node()"/>
							<xsl:choose>
								<xsl:when test="$cActionType='DatabaseUpgrade'">
									<h3>Database Upgrade</h3>
									<xsl:choose>
										<xsl:when test="/Page/ContentDetail/Content[@cType=$cActionType]">
										</xsl:when>
										<xsl:otherwise>
											<a href="{$appPath}?ewCmd=AddScheduledItem&amp;type={$cActionType}" class="btn btn-success btn-sm pull-right">
												<i class="fa fa-plus">&#160;</i>&#160;
												Add
											</a>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:when test="$cActionType='Feed'">
									<div class="row">
										<div class="col-md-6">
											<h3>Feeds</h3>
										</div>
										<div class="col-md-6">
											<a href="{$appPath}?ewCmd=AddScheduledItem&amp;type={$cActionType}" class="btn btn-success btn-sm pull-right">
												<i class="fa fa-plus">&#160;</i>&#160;
												Add
											</a>
										</div>
									</div>
								</xsl:when>
								<xsl:when test="$cActionType='LuceneIndex'">
									<h3>Lucene Index</h3>
									<xsl:choose>
										<xsl:when test="/Page/ContentDetail/Content[@cType=$cActionType]">
										</xsl:when>
										<xsl:otherwise>
											<a href="{$appPath}?ewCmd=AddScheduledItem&amp;type={$cActionType}" class="btn btn-success btn-sm pull-right">
												<i class="fa fa-plus">&#160;</i>&#160;
												Add
											</a>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:when test="$cActionType='SubscriptionProcess'">
									<h3>Subscription Process</h3>
									<xsl:choose>
										<xsl:when test="/Page/ContentDetail/Content[@cType=$cActionType]">
										</xsl:when>
										<xsl:otherwise>
											<a href="{$appPath}?ewCmd=AddScheduledItem&amp;type={$cActionType}" class="btn btn-success btn-sm pull-right">
												<i class="fa fa-plus">&#160;</i>&#160;
												Add
											</a>
										</xsl:otherwise>
									</xsl:choose>
								</xsl:when>
								<xsl:otherwise>
									<xsl:if test="$cActionType!=''">
										<div class="row">
											<div class="col-md-6">
												<h3>
													<xsl:value-of select="$cActionType"/>
												</h3>
											</div>
											<div class="col-md-6">
												<a href="{$appPath}?ewCmd=AddScheduledItem&amp;type={$cActionType}" class="btn btn-success btn-sm pull-right">
													<i class="fa fa-plus">&#160;</i>&#160;
													Add
												</a>
											</div>
										</div>
									</xsl:if>
								</xsl:otherwise>
							</xsl:choose>
							<xsl:if test="/Page/ContentDetail/Content[@cType=$cActionType]">
								<table class="table">
									<xsl:for-each select="/Page/ContentDetail/Content[@cType=$cActionType]">
										<xsl:apply-templates select="." mode="ScheduledItem"/>
									</xsl:for-each>
								</table>
							</xsl:if>
						</xsl:for-each>
					</div>
				</div>
			</div>
		</div>
	</xsl:template>


	<xsl:template match="Content" mode="ScheduledItem">
		<tr>
			<!--Last Done-->
			<td>
				<xsl:choose>
					<xsl:when test="@dLastComplete!=''">
						<xsl:call-template name="DD_MM_YY">
							<xsl:with-param name="date">
								<xsl:value-of select="@dLastComplete"/>
							</xsl:with-param>
						</xsl:call-template>
						<xsl:call-template name="HH_MM">
							<xsl:with-param name="date">
								<xsl:value-of select="@dLastComplete"/>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>-</xsl:otherwise>
				</xsl:choose>
			</td>
			<!--Next Due-->
			<td>
				<xsl:choose>
					<xsl:when test="@dLastComplete!=''">
						<xsl:call-template name="DD_MM_YY">
							<xsl:with-param name="date">
								<xsl:value-of select="@dLastComplete"/>
							</xsl:with-param>
						</xsl:call-template>
						<xsl:call-template name="HH_MM">
							<xsl:with-param name="date">
								<xsl:value-of select="@dLastComplete"/>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>-</xsl:otherwise>
				</xsl:choose>
			</td>
			<td>
				<a href="{$appPath}?ewCmd=EditScheduledItem&amp;type={@cType}&amp;id={@nActionKey}" class="btn btn-primary btn-xs">
					<i class="fa fa-edit">&#160;</i>&#160;
					Edit
				</a>
				<xsl:choose>
					<xsl:when test="@Active=1 or @Active=-1">
						<a href="{$appPath}?ewCmd=DeactivateScheduledItem&amp;type={@cType}&amp;id={@nActionKey}" class="btn btn-warning btn-xs">
							<i class="fa fa-ban">&#160;</i>&#160;Deactivate
						</a>
					</xsl:when>
					<xsl:otherwise>
						<a href="{$appPath}?ewCmd=ActivateScheduledItem&amp;type={@cType}&amp;id={@nActionKey}" class="btn btn-default btn-xs">
							<i class="fa fa-check-square">&#160;</i>&#160;Activate
						</a>
						<a href="{$appPath}?ewCmd=DeleteScheduledItem&amp;type={@cType}&amp;id={@nActionKey}" class="btn btn-danger btn-xs">
							<i class="fa fa-trash">&#160;</i>&#160;Delete
						</a>
					</xsl:otherwise>
				</xsl:choose>
				<a href="{$appPath}?ewCmd=ScheduledItemRunNow&amp;type={@cType}&amp;id={@nActionKey}" class="btn btn-success btn-xs">
					<i class="fa fa-play">&#160;</i>&#160;Run Now
				</a>
			</td>
		</tr>
	</xsl:template>



	<!--Feed Item-->
	<xsl:template match="Content[@cType='Feed']" mode="ScheduledItem">
		<xsl:variable name="aPageId" select="cActionXML/descendant-or-self::nPageId/node()"/>
		<tr>
			<!--Page-->
			<td colspan="3">
				<table>
					<tr>
						<td colaspan="3">
							<strong>Items adding to page:&#160;</strong>
							<xsl:value-of select="/Page/Menu/descendant-or-self::MenuItem[@id=$aPageId]/@name"/>
							<br/>
							<strong>Loading from feed at:&#160;</strong>
							<a href="{cActionXML/descendant-or-self::cURL/node()}">
								<xsl:value-of select="cActionXML/descendant-or-self::cURL/node()"/>
							</a>

						</td>
						<!--URL-->


					</tr>
				</table>
			</td>
		</tr>
		<tr>
			<!--Last Done-->
			<td>
				<xsl:choose>
					<xsl:when test="@dLastComplete!=''">
						<xsl:call-template name="DD_MM_YY">
							<xsl:with-param name="date">
								<xsl:value-of select="@dLastComplete"/>
							</xsl:with-param>
						</xsl:call-template>
						<xsl:call-template name="HH_MM">
							<xsl:with-param name="date">
								<xsl:value-of select="@dLastComplete"/>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>-</xsl:otherwise>
				</xsl:choose>
			</td>
			<!--Next Due-->
			<td>
				<xsl:choose>
					<xsl:when test="@dLastComplete!=''">
						<xsl:call-template name="DD_MM_YY">
							<xsl:with-param name="date">
								<xsl:value-of select="@dLastComplete"/>
							</xsl:with-param>
						</xsl:call-template>
						<xsl:call-template name="HH_MM">
							<xsl:with-param name="date">
								<xsl:value-of select="@dLastComplete"/>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>-</xsl:otherwise>
				</xsl:choose>
			</td>
			<td>
				<a href="{$appPath}?ewCmd=EditScheduledItem&amp;type={@cType}&amp;id={@nActionKey}" class="btn btn-primary btn-xs">
					<i class="fa fa-edit">&#160;</i>&#160;
					Edit
				</a>
				<xsl:choose>
					<xsl:when test="@Active=1 or @Active=-1">
						<a href="{$appPath}?ewCmd=DeactivateScheduledItem&amp;type={@cType}&amp;id={@nActionKey}" class="btn btn-warning btn-xs">
							<i class="fa fa-ban">&#160;</i>&#160;Deactivate
						</a>
					</xsl:when>
					<xsl:otherwise>
						<a href="{$appPath}?ewCmd=ActivateScheduledItem&amp;type={@cType}&amp;id={@nActionKey}" class="btn btn-default btn-xs">
							<i class="fa fa-check-square">&#160;</i>&#160;Activate
						</a>
						<a href="{$appPath}?ewCmd=DeleteScheduledItem&amp;type={@cType}&amp;id={@nActionKey}" class="btn btn-danger btn-xs">
							<i class="fa fa-trash">&#160;</i>&#160;Delete
						</a>
					</xsl:otherwise>
				</xsl:choose>
				<a href="{$appPath}?ewCmd=viewLog&amp;logIds=98,44&amp;lastest=50" class="btn btn-default btn-xs">
					<i class="fa fa-list">&#160;</i>&#160;View Log
				</a>
				<a href="{$appPath}?ewCmd=ScheduledItemRunNow&amp;type={@cType}&amp;id={@nActionKey}" class="btn btn-success btn-xs">
					<i class="fa fa-play">&#160;</i>&#160;Run Now
				</a>
			</td>
		</tr>
	</xsl:template>

	<!--Database Upgrade-->
	<xsl:template match="Content[@cType='DatabaseUpgrade']" mode="ScheduledItem">
		<tr>

			<!--Last Done-->
			<td>
				<xsl:choose>
					<xsl:when test="@dLastComplete!=''">
						<xsl:call-template name="DD_MM_YY">
							<xsl:with-param name="date">
								<xsl:value-of select="@dLastComplete"/>
							</xsl:with-param>
						</xsl:call-template>
						<xsl:call-template name="HH_MM">
							<xsl:with-param name="date">
								<xsl:value-of select="@dLastComplete"/>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>-</xsl:otherwise>
				</xsl:choose>
			</td>
			<td>
				<a href="{$appPath}?ewCmd=EditScheduledItem&amp;type={@cType}&amp;id={@nActionKey}" class="btn btn-primary btn-xs">
					<i class="fa fa-edit">&#160;</i>&#160;
					Edit
				</a>
				<xsl:choose>
					<xsl:when test="@Active=1 or @Active=-1">
						<a href="{$appPath}?ewCmd=DeactivateScheduledItem&amp;type={@cType}&amp;id={@nActionKey}" class="btn btn-warning btn-xs">
							<i class="fa fa-ban">&#160;</i>&#160;Deactivate
						</a>
					</xsl:when>
					<xsl:otherwise>
						<a href="{$appPath}?ewCmd=ActivateScheduledItem&amp;type={@cType}&amp;id={@nActionKey}" class="btn btn-default btn-xs">
							<i class="fa fa-check-square">&#160;</i>&#160;Activate
						</a>
					</xsl:otherwise>
				</xsl:choose>
			</td>
		</tr>
	</xsl:template>

	<!--Lucene Index-->
	<xsl:template match="Content[@cType='LuceneIndex']" mode="ScheduledItem">
		<tr>

			<!--Last Done-->
			<td>
				<xsl:choose>
					<xsl:when test="@dLastComplete!=''">
						<xsl:call-template name="DD_MM_YY">
							<xsl:with-param name="date">
								<xsl:value-of select="@dLastComplete"/>
							</xsl:with-param>
						</xsl:call-template>
						<xsl:call-template name="HH_MM">
							<xsl:with-param name="date">
								<xsl:value-of select="@dLastComplete"/>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>-</xsl:otherwise>
				</xsl:choose>
			</td>
			<!--Next Due-->
			<td>
				<xsl:choose>
					<xsl:when test="@dLastComplete!=''">
						<xsl:call-template name="DD_MM_YY">
							<xsl:with-param name="date">
								<xsl:value-of select="@dLastComplete"/>
							</xsl:with-param>
						</xsl:call-template>
						<xsl:call-template name="HH_MM">
							<xsl:with-param name="date">
								<xsl:value-of select="@dLastComplete"/>
							</xsl:with-param>
						</xsl:call-template>
					</xsl:when>
					<xsl:otherwise>-</xsl:otherwise>
				</xsl:choose>
			</td>
			<td>
				<a href="{$appPath}?ewCmd=EditScheduledItem&amp;type={@cType}&amp;id={@nActionKey}" class="btn btn-primary btn-xs">
					<i class="fa fa-edit">&#160;</i>&#160;
					Edit
				</a>
				<xsl:choose>
					<xsl:when test="@Active=1 or @Active=-1">
						<a href="{$appPath}?ewCmd=DeactivateScheduledItem&amp;type={@cType}&amp;id={@nActionKey}" class="btn btn-warning btn-xs">
							<i class="fa fa-ban">&#160;</i>&#160;Deactivate
						</a>
					</xsl:when>
					<xsl:otherwise>
						<a href="{$appPath}?ewCmd=ActivateScheduledItem&amp;type={@cType}&amp;id={@nActionKey}" class="btn btn-default btn-xs">
							<i class="fa fa-check-square">&#160;</i>&#160;Activate
						</a>
					</xsl:otherwise>
				</xsl:choose>
				<a href="{$appPath}?ewCmd=viewLog&amp;logIds=98,44&amp;lastest=50" class="btn btn-default btn-xs">
					<i class="fa fa-list">&#160;</i>&#160;View Log
				</a>
				<a href="{$appPath}?ewCmd=ScheduledItemRunNow&amp;type={@cType}&amp;id={@nActionKey}" class="btn btn-success btn-xs">
					<i class="fa fa-play">&#160;</i>&#160;Run Now
				</a>
			</td>
		</tr>
	</xsl:template>


	<xsl:template match="Page[@layout='EditScheduledItem']" mode="Admin">
		<div class="row" id="template_SystemPages">
			<div class="col-md-12">
				<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Page[@layout='ScheduledItemRunNow']" mode="Admin">
		<div class="row" id="template_SystemPages">
			<div class="col-md-12">
				<textarea rows="20" cols="80" id="ace-edit">
					<xsl:copy-of select="ContentDetail/*"/>
				</textarea>

				<a href="{$appPath}?ewCmd=ScheduledItemRunNow&amp;type={$page/Request/QueryString/Item[@name='type']/node()}&amp;id={$page/Request/QueryString/Item[@name='id']/node()}" class="btn btn-success btn-xs">
					<i class="fa fa-play">
						<xsl:text> </xsl:text>
					</i>&#160;Run Again
				</a>
				<a href="{$appPath}?ewCmd=ScheduledItems" class="btn btn-success btn-xs">
					<i class="fa fa-chevron-left">
						<xsl:text> </xsl:text>
					</i>&#160;Back to List
				</a>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Page[@layout='ScheduledItemRunNow']" mode="LayoutAdminJs">
		<script src="/ewcommon/js/ace/src-noconflict/ace.js" type="text/javascript" charset="utf-8"></script>
		<script>
			$('#ace-edit').val(formatXml($('#ace-edit').val()))
			var editor = ace.edit("ace-edit");
			editor.setOption("maxLines", 1000)
			editor.setTheme("ace/theme/tomorrow");
			editor.session.setMode("ace/mode/xml");
		</script>
	</xsl:template>

	<!--System Pages-->
	<xsl:template match="Page[@layout='SystemPages']" mode="Admin">
		<div class="row" id="template_SystemPages">
			<div  class="col-md-3">
				<div class="panel panel-default">
					<div class="panel-heading">
						<h3 class="title">Instructions</h3>
					</div>
					<div class="panel-body">
						<p>
							System pages get shown to users in certian situations.
						</p>
						<p>
							<strong>Page not found</strong> will be presented when a user types in or clicks on a page that does not or no longer exists.
						</p>
						<p>
							<strong>Login Required</strong> will be presented when a user who is not logged in tries to access a page that has restricted permissions on it.
						</p>
						<p>
							<strong>Access Denied</strong> will be presented when a user who is logged in tries to access a page that they do not have access to.
						</p>
						<p>
							<strong>
								<xsl:call-template name="eonicwebProductName"/> Error
							</strong> will be presented when <xsl:call-template name="eonicwebProductName"/> encounters an error.
						</p>
					</div>
				</div>
			</div>
			<div class="col-md-9">
				<div class="panel panel-default">
					<div class="panel-heading">
						<h3 class="title">System Pages</h3>
					</div>

					<table class="table">
						<xsl:call-template name="SystemPageAdminRow">
							<xsl:with-param name="pageTitle" select="'Page Not Found'"/>
						</xsl:call-template>
						<xsl:call-template name="SystemPageAdminRow">
							<xsl:with-param name="pageTitle" select="'Login Required'"/>
						</xsl:call-template>
						<xsl:call-template name="SystemPageAdminRow">
							<xsl:with-param name="pageTitle" select="'Access Denied'"/>
						</xsl:call-template>
						<xsl:call-template name="SystemPageAdminRow">
							<xsl:with-param name="pageTitle" select="'Protean Error'"/>
						</xsl:call-template>
					</table>

				</div>
			</div>
		</div>
	</xsl:template>


	<xsl:template name="SystemPageAdminRow">
		<xsl:param name="pageTitle"/>
		<xsl:param name="pageId" select="/Page/Menu/MenuItem[@id=/Page/@id]/MenuItem[@name=$pageTitle]/@id"/>
		<tr>
			<td>
				<xsl:value-of select="$pageTitle"/>
			</td>
			<td>
				<xsl:choose>
					<xsl:when test="/Page/Menu/MenuItem[@id=/Page/@id]/MenuItem[@name=$pageTitle]">
						<a href="{$appPath}?ewCmd=ViewSystemPages&amp;pgid={$pageId}" class="btn btn-xs btn-primary">
							<i class="fa fa-pencil fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Edit Page
						</a>
						<!--<a href="{$appPath}?ewCmd=ByPage&amp;pgid={$pageId}" class="adminButton">View</a>-->
						<a href="{$appPath}?ewCmd=EditPage&amp;pgid={$pageId}&amp;BehaviourEditPageCommand=SystemPages" class="btn btn-xs btn-primary">
							<i class="fa fa-cog fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Page Settings
						</a>
						<a href="{$appPath}?ewCmd=EditPageLayout&amp;pgid={$pageId}" class="btn btn-xs btn-primary">
							<i class="fa fa-file fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Page Layout
						</a>
						<a href="{$appPath}?ewCmd=DeletePage&amp;pgid={$pageId}" class="btn btn-xs btn-danger">
							<i class="fa fa-trash-o fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Delete
						</a>
					</xsl:when>
					<xsl:otherwise>
						<a href="{$appPath}?ewCmd=AddPage&amp;parId={/Page/@id}&amp;name={translate($pageTitle,' ','+')}&amp;BehaviourAddPageCommand=ViewSystemPages" class="btn btn-xs btn-success">
							<i class="fa fa-plus fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Add
						</a>
					</xsl:otherwise>
				</xsl:choose>
			</td>
		</tr>
	</xsl:template>


	<!--Subscriptions-->
	<xsl:template match="Page[@layout='Subscriptions']" mode="Admin">
		<div class="row" id="template_Subscriptions">
			<div class="col-md-3">
				<div class="panel panel-default">
					<div class="panel-heading">Instructions</div>
					<div class="panel-body">
						This allows you to manage groups of subscriptions. These are intended to identify different levels of the same service that can be "upgraded" such as silver and gold membership.<br/><br/> If a user has a silver membership the difference in cost of the remainder of the term will be calculated and charged, with the subscription repeating at the full rate.
					</div>
				</div>

			</div>
			<div class="col-md-9">

				<div class="panel panel-default">
					<div class="panel-heading">
						<xsl:if  test="/Page/@ewCmd!='MoveSubscription'">
							<a href="{$appPath}?ewCmd=AddSubscriptionGroup"  class="btn btn-success pull-right">
								<i class="fa fa-plus">&#160;</i>&#160;
								Add Subscription Category
							</a>
						</xsl:if>
						<h3 class="panel-title">Subscription Categories</h3>
					</div>
					<table class="table">
						<xsl:for-each select="/Page/ContentDetail/SubscriptionGroups">
							<tr>
								<td colspan="3">
									<h4>
										<xsl:value-of select="@cCatName"/>
									</h4>
								</td>
								<td colspan="3">
									<xsl:choose>
										<xsl:when test="/Page/@ewCmd='MoveSubscription'">
											<a href="{$appPath}?ewCmd=MoveSubscription&amp;grp={@nCatKey}&amp;id={/Page/Request/QueryString/Item[@name='id']}"  class="btn btn-default">
												<i class="fa fa-move-here fa-white">
													<xsl:text> </xsl:text>
												</i><xsl:text> </xsl:text>Move Here
											</a>
										</xsl:when>
										<xsl:otherwise>
											<a href="{$appPath}?ewCmd=EditSubscriptionGroup&amp;grp={@nCatKey}"  class="btn btn-primary btn-sm pull-right">
												<i class="fa fa-edit">&#160;</i>&#160;Edit
											</a>
											<a href="{$appPath}?ewCmd=AddSubscription&amp;grp={@nCatKey}"  class="btn btn-success btn-sm pull-right">
												<i class="fa fa-plus">&#160;</i>&#160;Add Subscription
											</a>
										</xsl:otherwise>
									</xsl:choose>
								</td>
							</tr>
							<xsl:if  test="/Page/@ewCmd!='MoveSubscription'">
								<xsl:if test="Subscriptions">
									<tr>
										<td>
											<xsl:apply-templates select="." mode="AdminSubscriptions">
												<xsl:with-param name="GroupID">
													<xsl:value-of select="@nCatKey"/>
												</xsl:with-param>
											</xsl:apply-templates>
										</td>
									</tr>
								</xsl:if>
							</xsl:if>
						</xsl:for-each>

						<xsl:if  test="/Page/@ewCmd!='MoveSubscription'">

							<tr>
								<th colspan="3">
									<h4>Orphan Subscriptions</h4>
								</th>
							</tr>
							<xsl:apply-templates select="/Page/ContentDetail" mode="AdminSubscriptions">
								<xsl:with-param name="GroupID">0</xsl:with-param>
							</xsl:apply-templates>

						</xsl:if>
					</table>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="*" mode="AdminSubscriptions">
		<xsl:param name="GroupID"/>

		<tr>

			<th>
				&#160;
				&#160;
				&#160;
			</th>
			<th>Name</th>
			<th>Duration</th>
			<th>Payment</th>
			<th>
				<xsl:choose>
					<xsl:when test="$GroupID>0">
						<a href="{$appPath}?ewCmd=AddSubscription&amp;grp={$GroupID}"  class="btn btn-success btn-xs">
							<i class="fa fa-plus">&#160;</i>&#160;
							Add
						</a>
					</xsl:when>
					<xsl:otherwise>
						<a href="{$appPath}?ewCmd=AddSubscription"  class="btn btn-success btn-xs">
							<i class="fa fa-plus">&#160;</i>&#160;Add
						</a>
					</xsl:otherwise>
				</xsl:choose>
			</th>
		</tr>
		<xsl:apply-templates select="Subscriptions" mode="AdminSubscriptions">
			<xsl:with-param name="GroupID">
				<xsl:value-of select="$GroupID"/>
			</xsl:with-param>
		</xsl:apply-templates>


	</xsl:template>
	<xsl:template match="Subscriptions" mode="AdminSubscriptions">
		<xsl:param name="GroupID"/>
		<tr>
			<td>
				&#160;
				&#160;
				&#160;
			</td>
			<td>
				<xsl:value-of select="@cContentName"/>
			</td>
			<td>
				<xsl:value-of select="Content/Duration/Length"/>&#160;<xsl:value-of select="Content/Duration/Unit"/>
			</td>
			<td>
				<xsl:value-of select="Content/Prices/Price"/> / <xsl:value-of select="Content/PaymentUnit"/>
			</td>
			<td>
				<a href="{$appPath}?ewCmd=EditSubscription&amp;id={@nContentKey}"  class="btn btn-primary btn-xs">
					<i class="fa fa-edit">&#160;</i>&#160;Edit
				</a>
				<a href="{$appPath}?ewCmd=LocateSubscription&amp;id={@nContentKey}" class="btn btn-default btn-xs">
					<i class="fa fa-search">&#160;</i>&#160;Locate
				</a>
				<a href="{$appPath}?ewCmd=MoveSubscription&amp;id={@nContentKey}" class="btn btn-default btn-xs">
					<i class="fa fa-users">&#160;</i>&#160;Change Group
				</a>
				<a href="{$appPath}?ewCmd=ListSubscribers&amp;id={@nContentKey}" class="btn btn-default btn-xs">
					<i class="fa fa-users">&#160;</i>&#160;List Subscribers
				</a>
			</td>
		</tr>
	</xsl:template>
	<!-- -->

	<!--Subscriptions-->
	<xsl:template match="Page[@layout='ListSubscribers']" mode="Admin">
		<div class="row" id="template_Subscriptions">
			<div class="col-md-12">
				<div class="panel panel-default">
					<div class="panel-heading">
						<h3 class="panel-title">Subscribers</h3>
					</div>
					<table class="table">
						<tr>
							<th>Name</th>
							<th>Usernane</th>
							<th>Subscription Name</th>
							<th>Active</th>
							<th>Start Date</th>
							<th>Next Renewal</th>
						</tr>
						<xsl:for-each select="/Page/ContentDetail/Subscribers">
							<tr>
								<td>
									<xsl:value-of select="cDirXml/User/FirstName/node()"/>&#160;<xsl:value-of select="cDirXml/User/LastName/node()"/>
								</td>
								<td>
									<xsl:value-of select="cDirName/node()"/>
								</td>
								<td>
									<xsl:value-of select="cSubName/node()"/>
								</td>
								<td>
									<xsl:value-of select="bPaymentMethodActive/node()"/>
								</td>
								<td>
									<xsl:call-template name="DD_Mon_YYYY">
										<xsl:with-param name="date">
											<xsl:value-of select="dStartDate/node()"/>
										</xsl:with-param>
										<xsl:with-param name="showTime">false</xsl:with-param>
									</xsl:call-template>

								</td>
								<td>
									<xsl:call-template name="DD_Mon_YYYY">
										<xsl:with-param name="date">
											<xsl:value-of select="dExpireDate/node()"/>
										</xsl:with-param>
										<xsl:with-param name="showTime">false</xsl:with-param>
									</xsl:call-template>
								</td>
								<td colspan="3">

									<a href="{$appPath}?ewCmd=ManageUserSubscription&amp;id={nSubKey/node()}"  class="btn btn-primary btn-sm">
										<i class="fa fa-edit">&#160;</i>&#160;Manage
									</a>
									<!--a href="{$appPath}?ewCmd=CancelSubscription&amp;subId={nSubKey/node()}&amp;id={/Page/Request/QueryString/Item[@name='id']/node()}"  class="btn btn-danger btn-sm">
                  <i class="fa fa-edit">&#160;</i>&#160;Cancel
                </a-->

								</td>
							</tr>
							<xsl:if  test="/Page/@ewCmd!='MoveSubscription'">
								<xsl:if test="Subscriptions">
									<tr>
										<td>
											<xsl:apply-templates select="." mode="AdminSubscriptions">
												<xsl:with-param name="GroupID">
													<xsl:value-of select="@nCatKey"/>
												</xsl:with-param>
											</xsl:apply-templates>
										</td>
									</tr>
								</xsl:if>
							</xsl:if>
						</xsl:for-each>

					</table>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Page[@layout='ManageUserSubscription']" mode="Admin">
		<xsl:variable name="thisURL">
			<xsl:apply-templates select="." mode="getHref"/>
		</xsl:variable>
		<xsl:variable name="parId" select="@parId" />
		<xsl:for-each select="ContentDetail/Subscription">

			<div class="subscription detail">
				<div class="row">
					<div class="col-md-6">
						<div class="panel panel-default">
							<div class="panel-heading">
								<a href="?ewCmd=EditUserSubscription&amp;id={@id}" class="btn btn-primary pull-right">
									<i class="fa fa-pencil">&#160;</i>&#160;
									Edit Subscripiton
								</a>
								<h3 class="panel-title">
									<xsl:value-of select="@name"/>
								</h3>
							</div>
							<div class="description panel-body">
								<dl class="tabled">
									<dt>Start Date</dt>
									<dd>
										<xsl:call-template name="formatdate">
											<xsl:with-param name="date" select="@startDate" />
											<xsl:with-param name="format" select="'dddd dd MMM yyyy'" />
										</xsl:call-template>
									</dd>
									<xsl:choose>
										<xsl:when test="renewalStatus='Rolling'">
											<dt>Renewal Date</dt>
											<dd>
												<xsl:call-template name="formatdate">
													<xsl:with-param name="date" select="@expireDate" />
													<xsl:with-param name="format" select="'dddd dd MMM yyyy'" />
												</xsl:call-template>
												<br/>
											</dd>
										</xsl:when>
										<xsl:otherwise>
											<dt>End Date</dt>
											<dd>
												<xsl:call-template name="formatdate">
													<xsl:with-param name="date" select="@expireDate" />
													<xsl:with-param name="format" select="'dddd dd MMM yyyy'" />
												</xsl:call-template>
												<br/>

											</dd>
										</xsl:otherwise>
									</xsl:choose>

									<dt>Renewal Amount</dt>
									<dd>
										£ <xsl:value-of select="format-number(@value, '0.00')"/><br/>
									</dd>
									<dt>Type</dt>
									<dd>
										<xsl:value-of select="@renewalStatus"/>
									</dd>
									<xsl:if test="@renewalStatus='Cancelled'">
										<dt>Cancelled Date</dt>
										<dd>
											<xsl:call-template name="formatdate">
												<xsl:with-param name="date" select="@cancelDate" />
												<xsl:with-param name="format" select="'dddd dd MMM yyyy'" />
											</xsl:call-template>
										</dd>
										<dt>Cancelled Reason</dt>
										<dd>
											<xsl:value-of select="@cancelReason"/>
										</dd>
										<dt>Cancelled By</dt>
										<dd>
											User ID <xsl:value-of select="@cancelUserId"/>
										</dd>
									</xsl:if>
									<dt>Period</dt>
									<dd>
										<xsl:text>Every </xsl:text>
										<xsl:value-of select="@period"/>
										<xsl:text> </xsl:text>
										<xsl:value-of select="Content/Duration/Unit/node()"/>
										<xsl:text>s</xsl:text>
									</dd>
								</dl>

								<xsl:choose>
									<xsl:when test="tblCartPaymentMethod/nStatus='1'">
										<span class="text-success">
											<i class="far fa-lg fa-check-circle">&#160;</i>
											<xsl:value-of select="tblCartPaymentMethod/cPayMthdProviderName/node()"/>
										</span>

										<br/>
										<small>
											<xsl:value-of select="tblCartPaymentMethod/cPayMthdDescription/node()"/>
										</small>
									</xsl:when>
									<xsl:otherwise>
										<span class="text-danger">
											<i class="far fa-lg fa-times-circle">&#160;</i>
											<xsl:value-of select="tblCartPaymentMethod/cPayMthdProviderName/node()"/>
										</span>
										<br/>
										<small>
											<xsl:value-of select="tblCartPaymentMethod/cPayMthdDescription/node()"/>
										</small>
									</xsl:otherwise>
								</xsl:choose>

								<xsl:choose>
									<xsl:when test="tblCartPaymentMethod/nStatus!='0' or @cancelDate!=''">
										<xsl:choose>
											<xsl:when test="@paymentStatus='cancelled' and @providerName='GoCardless' ">
												<div class="alert alert-danger">
													The customer has canceled their GoCardless Direct Debit payment directly with their bank.
												</div>
											</xsl:when>
											<xsl:when test="tblCartPaymentMethod/nStatus='1' or @paymentStatus='Manual' ">
												<div class="alert alert-success">
													<xsl:choose>
														<xsl:when test="tblCartPaymentMethod/cPayMthdAcctName/node()='WorldPay'">
															Credit Card Payment needs to be processed
														</xsl:when>
														<xsl:otherwise>
															Payment be collected via
														</xsl:otherwise>
													</xsl:choose>
													<strong>
														<xsl:value-of select="@providerName"/>
													</strong>
													<br/>
													<xsl:value-of select="@providerName"/> ref: <xsl:value-of select="@providerRef"/>
													<br/>
													<br/>
													<a href="?ewCmd=RenewSubscription&amp;id={@id}" class="btn btn-success">
														<i class="fa fa-repeat">&#160;</i>&#160;Manual Renewal
													</a>
												</div>
											</xsl:when>
											<xsl:otherwise>
												<div class="alert alert-warning">
													We cannot collect payment as we do not have an active payment method.
												</div>
											</xsl:otherwise>
										</xsl:choose>

										<xsl:if test="@cancelDate!=''">
											<br/>
											<xsl:value-of select="@cancelDate"/> -
											<xsl:value-of select="@cancelReason"/>
										</xsl:if>



									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="@paymentStatus"/>
									</xsl:otherwise>

								</xsl:choose>

								<xsl:if test="contains(@paymentStatus,'Expires')">
									<div class="alert alert-success">
										Credit Card Payment needs to be processed <strong>
											<xsl:value-of select="@providerName"/>
										</strong>
										<br/>
										<xsl:value-of select="@paymentStatus"/>
										<br/>
										<xsl:value-of select="@providerName"/> ref: <xsl:value-of select="@providerRef"/>
										<br/>
										<br/>
										<a href="{$appPath}?ewCmd=RenewSubscription&amp;id={@id}" class="btn btn-success">
											<i class="fa fa-repeat">&#160;</i>&#160;Manual Renewal
										</a>
									</div>
								</xsl:if>




							</div>
							<div class="panel-footer form-actions">

								<xsl:if test="@renewalStatus!='Cancelled'">
									<a href="{$appPath}?ewCmd=CancelSubscription&amp;id={@id}" class="btn btn-danger">
										<i class="fa fa-times">&#160;</i>&#160;Cancel Immediately
									</a>
								</xsl:if>
								<xsl:if test="@renewalStatus='Rolling'">
									<a href="{$appPath}?ewCmd=ExpireSubscription&amp;id={@id}" class="btn btn-warning">
										<i class="fa fa-times">&#160;</i>&#160;Set to Expire on Renewal
									</a>
								</xsl:if>

							</div>
						</div>
						<div class="panel panel-default">
							<div class="panel-heading">
								<h3 class="panel-title">Renewal History</h3>
							</div>
							<div class="description panel-body reponsive-table">
								<ul class="list-group">
									<xsl:for-each select="*/Renewal">
										<li class="list-group-item">
											<xsl:call-template name="formatdate">
												<xsl:with-param name="date" select="@startDate" />
												<xsl:with-param name="format" select="'dddd dd MMM yyyy'" />
											</xsl:call-template> to

											<xsl:call-template name="formatdate">
												<xsl:with-param name="date" select="@endDate" />
												<xsl:with-param name="format" select="'dddd dd MMM yyyy'" />
											</xsl:call-template>
											Paid By
											<xsl:value-of select="@providerName"/>

											<a class="pull-right btn btn-default" href="{$appPath}?ewCmd=Orders&amp;ewCmd2=Display&amp;id={@orderId}">Order</a>
										</li>

									</xsl:for-each>
								</ul>
							</div>
						</div>
					</div>
					<div class="col-md-6">
						<div class="panel panel-default">
							<div class="panel-heading">
								<h3 class="panel-title">User</h3>
							</div>
							<div class="description panel-body reponsive-table">
								<dl class="tabled">
									<dt>Name</dt>
									<dd>
										<xsl:value-of select="User/FirstName/node()"/>
										<xsl:text> </xsl:text>
										<xsl:value-of select="User/LastName/node()"/>
									</dd>
									<dt>Email</dt>
									<dd>
										<a href="mailto:{User/Email/node()}">
											<xsl:value-of select="User/Email/node()"/>
										</a>
									</dd>
									<xsl:for-each select="User/Contacts/Contact[cContactType='Billing Address']">
										<dt>Billing Address</dt>
										<dd>
											<xsl:value-of select="cContactAddress/node()"/>
											<xsl:text>, </xsl:text>
											<br/>
											<xsl:value-of select="cContactCity/node()"/>
											<xsl:text>, </xsl:text>
											<br/>
											<xsl:value-of select="cContactState/node()"/>
											<xsl:text>. </xsl:text>
											<xsl:value-of select="cContactZip/node()"/>
											<xsl:text>. </xsl:text>
										</dd>
										<dt>Telephone</dt>
										<dd>
											<a href="tel:{cContactTel/node()}">
												<xsl:value-of select="cContactTel/node()"/>
											</a>
										</dd>
									</xsl:for-each>
									<dt>Groups</dt>
									<dd>
										<xsl:for-each select="User/Group">
											<xsl:value-of select="Name/node()"/>
											<xsl:if test="@id = ancestor::Content/Usergroups/Group/@id">
												[Linked to this subscription]
											</xsl:if>
											<br/>
										</xsl:for-each>
									</dd>

								</dl>
							</div>
						</div>
						<xsl:apply-templates select="." mode="displayNotes"/>
					</div>
				</div>
			</div>
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="help[@class='renew-sub']" mode="xform">
		<div class="alert alert-info">
			<i class="fa fa-info fa-2x pull-left">
				<xsl:text> </xsl:text>
			</i>
			<xsl:for-each select="ancestor::Content/model/instance/Subscription">
				We will collect renewal payment via <strong>
					<xsl:value-of select="@providerName"/>
				</strong>
				<br/>
				<dl class="tabled">
					<dt>Renewal Cost</dt>
					<dd>
						£ <xsl:value-of select="format-number(@value, '0.00')"/><br/>
					</dd>
					<dt>New End Date</dt>
					<dd>
						<xsl:call-template name="formatdate">
							<xsl:with-param name="date" select="@newExpire" />
							<xsl:with-param name="format" select="'dddd dd MMM yyyy'" />
						</xsl:call-template>
					</dd>
				</dl>
			</xsl:for-each>
		</div>
	</xsl:template>

	<xsl:template match="Subscription" mode="displayNotes">

	</xsl:template>


	<!--Subscriptions-->
	<xsl:template match="Page[@layout='UpcomingRenewals' or @layout='ExpiredSubscriptions' or @layout='CancelledSubscriptions' or @layout='RecentRenewals']" mode="Admin">
		<div class="row" id="template_Subscriptions">
			<div class="col-md-12">
				<xsl:if test="@layout='UpcomingRenewals'">
					<div class="alert alert-info">
						<h3>
							<strong>
								<xsl:value-of select="count(/Page/ContentDetail/Subscribers)"/>
							</strong> Upcoming Renewals :
							<strong>
								<xsl:call-template name="formatPrice">
									<xsl:with-param name="price" select="sum(/Page/ContentDetail/Subscribers/nValueNet/node())"/>
									<xsl:with-param name="currency" select="$currencySymbol"/>
								</xsl:call-template>
							</strong>
						</h3>
					</div>
				</xsl:if>

				<div class="panel panel-default">
					<table class="table">
						<tr>
							<th>User</th>
							<th>Usernane</th>
							<th>Subscription</th>
							<th>Rate</th>
							<th>Status</th>
							<th>PayProvider</th>
							<th>Start Date</th>
							<th>Renewal Due</th>
						</tr>
						<xsl:for-each select="/Page/ContentDetail/Subscribers">
							<tr>
								<td>

									<a href="/{$appPath}?ewCmd=Profile&amp;DirType=User&amp;id={nDirId/node()}">
										<span class="btn btn-primary btn-xs">
											<i class="fa fa-user fa-white">
												<xsl:text> </xsl:text>
											</i>
										</span>
										&#160;
										<xsl:value-of select="cDirXml/User/LastName"/>, <xsl:value-of select="cDirXml/User/FirstName"/>

									</a>
								</td>
								<td>
									<xsl:value-of select="cDirName/node()"/>
								</td>
								<td>
									<xsl:value-of select="cSubName/node()"/>
								</td>
								<td>
									<xsl:call-template name="formatPrice">
										<xsl:with-param name="price" select="nValueNet/node()"/>
										<xsl:with-param name="currency" select="$currencySymbol"/>
									</xsl:call-template>
								</td>
								<td>
									<xsl:value-of select="cRenewalStatus/node()"/>
								</td>
								<td>
									<xsl:choose>
										<xsl:when test="nPayMethodStatus='1'">
											<span class="text-success">
												<i class="far fa-lg fa-check-circle">&#160;</i>
												<xsl:value-of select="cPayMthdProviderName/node()"/>
											</span>
											<br/>
											<small>
												<xsl:value-of select="cPayMthdDescription/node()"/>
											</small>
										</xsl:when>
										<xsl:otherwise>
											<span class="text-danger">
												<i class="far fa-lg fa-times-circle">&#160;</i>
												<xsl:value-of select="cPayMthdProviderName/node()"/>
											</span>
											<br/>
											<small>
												<xsl:value-of select="cPayMthdDescription/node()"/>
											</small>
										</xsl:otherwise>
									</xsl:choose>
								</td>
								<td>
									<xsl:call-template name="DD_Mon_YYYY">
										<xsl:with-param name="date">
											<xsl:value-of select="dStartDate/node()"/>
										</xsl:with-param>
										<xsl:with-param name="showTime">false</xsl:with-param>
									</xsl:call-template>

								</td>
								<td>
									<xsl:call-template name="DD_Mon_YYYY">
										<xsl:with-param name="date">
											<xsl:value-of select="dExpireDate/node()"/>
										</xsl:with-param>
										<xsl:with-param name="showTime">false</xsl:with-param>
									</xsl:call-template>
								</td>
								<td colspan="3">

									<a href="{$appPath}?ewCmd=ManageUserSubscription&amp;id={nSubKey/node()}"  class="btn btn-primary btn-sm">
										<i class="fa fa-edit">&#160;</i>&#160;Manage
									</a>

								</td>
							</tr>
							<xsl:if  test="/Page/@ewCmd!='MoveSubscription'">
								<xsl:if test="Subscriptions">
									<tr>
										<td>
											<xsl:apply-templates select="." mode="AdminSubscriptions">
												<xsl:with-param name="GroupID">
													<xsl:value-of select="@nCatKey"/>
												</xsl:with-param>
											</xsl:apply-templates>
										</td>
									</tr>
								</xsl:if>
							</xsl:if>
						</xsl:for-each>

					</table>
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Page[@layout='EditTemplate']" mode="Admin">
		<div class="row" id="tpltAdvancedMode">
			<div class="col-md-12">
				<div class="panel">
					<div class="panel-heading">
						<h4>
							Edit <xsl:value-of select="@ewCmd2"/><xsl:text> Templates</xsl:text>
						</h4>
					</div>
					<div class="panel-body">
						<div class="panel-group">
							<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
						</div>
						<div class="panel-group">
							<xsl:apply-templates select="/" mode="ListByContentTypeNoColaspe">
								<xsl:with-param name="contentType" select="@ewCmd2"/>
							</xsl:apply-templates>
						</div>
					</div>
				</div>
			</div>
		</div>
	</xsl:template>


	<xsl:template match="*" mode="subscriptionTable">

		<table class="table">
			<tr>
				<th>User</th>
				<th>Usernane</th>
				<th>Subscription</th>
				<th>Rate</th>
				<th>Status</th>
				<th>PayProvider</th>
				<th>Start Date</th>
				<th>Renewal Due</th>
				<th>Sent Date</th>
				<th>&#160;</th>
			</tr>
			<xsl:for-each select="Subscribers">
				<tr>
					<td>
						<a href="/{$appPath}?ewCmd=Profile&amp;DirType=User&amp;id={nDirId/node()}">
							<span class="btn btn-primary btn-xs">
								<i class="fa fa-user fa-white">
									<xsl:text> </xsl:text>
								</i>
							</span>
							&#160;
							<xsl:value-of select="cDirXml/User/LastName"/>, <xsl:value-of select="cDirXml/User/FirstName"/>
						</a>
					</td>
					<td>
						<xsl:value-of select="cDirName/node()"/>
					</td>
					<td>
						<xsl:value-of select="cSubName/node()"/>
					</td>
					<td>
						<xsl:call-template name="formatPrice">
							<xsl:with-param name="price" select="nValueNet/node()"/>
							<xsl:with-param name="currency" select="$currencySymbol"/>
						</xsl:call-template>
					</td>
					<td>
						<xsl:value-of select="cRenewalStatus/node()"/>
					</td>
					<td>
						<xsl:choose>
							<xsl:when test="Subscription/Subscription/tblCartPaymentMethod/nStatus='1'">
								<span class="text-success">
									<i class="far fa-lg fa-check-circle">&#160;</i>
									<xsl:value-of select="Subscription/Subscription/tblCartPaymentMethod/cPayMthdProviderName/node()"/>
								</span>

								<br/>
								<small>
									<xsl:value-of select="Subscription/Subscription/tblCartPaymentMethod/cPayMthdDescription/node()"/>
								</small>
							</xsl:when>
							<xsl:otherwise>
								<span class="text-danger">
									<i class="far fa-lg fa-times-circle">&#160;</i>
									<xsl:value-of select="Subscription/Subscription/tblCartPaymentMethod/cPayMthdProviderName/node()"/>
								</span>
								<br/>
								<small>
									<xsl:value-of select="Subscription/Subscription/tblCartPaymentMethod/cPayMthdDescription/node()"/>
								</small>
							</xsl:otherwise>
						</xsl:choose>
					</td>
					<td>
						<xsl:call-template name="DD_Mon_YYYY">
							<xsl:with-param name="date">
								<xsl:value-of select="dStartDate/node()"/>
							</xsl:with-param>
							<xsl:with-param name="showTime">false</xsl:with-param>
						</xsl:call-template>

					</td>
					<td>
						<xsl:call-template name="DD_Mon_YYYY">
							<xsl:with-param name="date">
								<xsl:value-of select="dExpireDate/node()"/>
							</xsl:with-param>
							<xsl:with-param name="showTime">false</xsl:with-param>
						</xsl:call-template>
					</td>
					<td>

						<xsl:value-of select="@actionResult"/>

					</td>
					<td colspan="3">

						<a href="{$appPath}?ewCmd=ManageUserSubscription&amp;id={nSubKey/node()}"  class="btn btn-primary btn-sm">
							<i class="fa fa-edit">&#160;</i>&#160;Manage
						</a>
						<xsl:if test="parent::reminder">
							<xsl:choose>
								<xsl:when test="@actionResult = 'not sent'">
									<a href="{$appPath}?ewCmd=RenewalAlerts&amp;name={ancestor::reminder/@name}&amp;SendId={nSubKey/node()}"  class="btn btn-success btn-sm">
										<i class="fa fa-envelope">&#160;</i>&#160;Send Alert
									</a>
								</xsl:when>
								<xsl:otherwise>
									<a href="{$appPath}?ewCmd=RenewalAlerts&amp;name={ancestor::reminder/@name}&amp;SendId={nSubKey/node()}"  class="btn btn-warning btn-sm">
										<i class="fa fa-envelope">&#160;</i>&#160;Resend
									</a>
								</xsl:otherwise>

							</xsl:choose>

						</xsl:if>

					</td>
				</tr>
				<xsl:if  test="/Page/@ewCmd!='MoveSubscription'">
					<xsl:if test="Subscriptions">
						<tr>
							<td>
								<xsl:apply-templates select="." mode="AdminSubscriptions">
									<xsl:with-param name="GroupID">
										<xsl:value-of select="@nCatKey"/>
									</xsl:with-param>
								</xsl:apply-templates>
							</td>
						</tr>
					</xsl:if>
				</xsl:if>
			</xsl:for-each>

		</table>
	</xsl:template>

	<xsl:template match="Page[@layout='RenewalAlerts']" mode="Admin">
		<div class="row" id="template_Subscriptions">

			<div class="col-md-12">
				<div class="panel panel-default">
					<table class="table">
						<tr>
							<th>Name</th>
							<th>Action</th>
							<th>Period</th>
							<th>Count</th>
							<th>Subject</th>
							<th>
								<a href="{$appPath}?ewCmd=RenewalAlerts&amp;ewCmd2=processAll" class="btn btn-danger pull-right">
									<i class="fa fa-plus">&#160;</i>&#160;Process All
								</a>
								<a href="{$appPath}?ewCmd=RenewalAlerts&amp;ewCmd2=add" class="btn btn-default pull-right" disabled="disabled">
									<i class="fa fa-history">&#160;</i>&#160;Process History
								</a>
							</th>
						</tr>
						<xsl:for-each select="ContentDetail/subscriptionReminders/reminder">
							<tr>
								<td>
									<xsl:value-of select="@name"/>
								</td>
								<td>
									<xsl:value-of select="@action"/>
								</td>
								<td>
									<xsl:value-of select="@period"/>
								</td>
								<td>
									<xsl:value-of select="@count"/>
								</td>
								<td>
									<xsl:value-of select="@subject"/>
								</td>
								<td>
									&#160;
								</td>
							</tr>
							<tr>
								<td colspan="7">
									<xsl:apply-templates select="." mode="subscriptionTable"/>
								</td>
							</tr>
						</xsl:for-each>
					</table>
					<br/>

				</div>
			</div>
		</div>
	</xsl:template>

	<!--LocateSearch-->
	<xsl:template match="Page[@layout='LocateSearch']" mode="Admin">
		<div class="row" id="tpltLocateSearch">
			<div class="col-md-3">
				<xsl:apply-templates select="ContentDetail/Content[@type='xform' and @name='FindContentToRelate']" mode="xform"/>
				<div class="panel panel-default">
					<div class="panel-heading">
						<h3 class="panel-title">Instructions</h3>
					</div>
					<div class="panel-body">
						<p>This enables you to search for items of content that are on other pages of the site to be placed on this current page.</p>
						<p>You can search by page, by section by selecting a parent page and clicking "search children" or by keyword by entering a word into the box.</p>
						<p>To list all content items just hit search without entering anything into the form (This may take some time to come back if your site contains a lot of this type of content)</p>
					</div>
				</div>
			</div>
			<div class="col-md-9">
				<div class="panel panel-default">
					<xsl:apply-templates select="ContentDetail/Content[@type='xform' and @name='SelectContentToLocate']" mode="xform"/>
				</div>
			</div>
		</div>
	</xsl:template>
	<!-- BJR Member Activity Reports-->
	<xsl:template match="Page[@layout='MemberActivityReport']" mode="Admin">
		<div class="row" id="template_EditStructure">
			<xsl:choose>
				<xsl:when test="ContentDetail/Content[@type='xform']">
					<div class="col-md-3">
						<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
					</div>
					<div class="col-md-9 panel">
						<xsl:apply-templates select="ContentDetail/Content[@type='Report']" mode="MemberActivityReport"/>
					</div>
				</xsl:when>
				<xsl:otherwise>
					<div class="col-md-12 panel">
						<a href="{$appPath}?ewCmd=MemberActivity" class="btn btn-primary pull-right">
							<i class="fa fa-mail-reply">
								<xsl:text> </xsl:text>
							</i>Back
						</a>
						<xsl:apply-templates select="ContentDetail/Content[@type='Report']" mode="MemberActivityReport"/>
					</div>
				</xsl:otherwise>
			</xsl:choose>
		</div>
	</xsl:template>

	<xsl:template match="Content[@type='Report']" mode="MemberActivityReport">
		<h4>
			<xsl:value-of select="@name"/>
		</h4>
		<xsl:for-each select="Report">
			<table class="table">
				<tr>
					<xsl:for-each select="Item[1]/*">
						<xsl:variable name="origName" select="local-name()"/>
						<xsl:variable name="newName" >
							<xsl:apply-templates select="." mode="MemberActivityReport_ColsNames"/>
						</xsl:variable>
						<xsl:if test="$newName!=$origName">
							<th>
								<xsl:value-of select="$newName"/>
							</th>
						</xsl:if>
					</xsl:for-each>
				</tr>
				<xsl:for-each select="Item">
					<span class="advancedModeRow" onmouseover="this.className='rowOver'" onmouseout="this.className='advancedModeRow'">
						<tr>
							<xsl:for-each select="*">
								<xsl:apply-templates select="." mode="MemberActivityReport_ColsValues"/>
							</xsl:for-each>
						</tr>
					</span>
				</xsl:for-each>
			</table>
		</xsl:for-each>
	</xsl:template>

	<xsl:template match="Report" mode="CampaignReport">
		<div class="panel panel-default">
			<div class="panel-heading">
				<h3 class="panel-title">
					Campaign Transmission History
				</h3>
			</div>

			<table class="table table-striped">
				<tr>
					<th>Name</th>
					<th>Date Sent</th>
					<th>Recipents</th>
				</tr>
				<xsl:for-each select="Campaign">
					<span class="advancedModeRow" onmouseover="this.className='rowOver'" onmouseout="this.className='advancedModeRow'">
						<tr>
							<td>
								<xsl:value-of select="@name"/>
								<xsl:if test="not(@name)">
									Deleted from Campaign Monitor
								</xsl:if>
							</td>
							<td>
								<xsl:value-of select="@sentDate"/>
							</td>
							<td>
								<xsl:value-of select="@recipients"/>
							</td>
						</tr>
					</span>
				</xsl:for-each>
			</table>
		</div>
	</xsl:template>


	<xsl:template match="*" mode="MemberActivityReport_ColsNames">
		<xsl:choose>
			<xsl:when test="local-name()='nNoPages'">
				<xsl:text> Number of Pages Visited</xsl:text>
			</xsl:when>
			<xsl:when test="local-name()='nSessionSeconds'">
				<xsl:text>Session Length (Minutes)</xsl:text>
			</xsl:when>
			<xsl:when test="local-name()='cDirName'">
				<xsl:text>User Name</xsl:text>
			</xsl:when>
			<xsl:when test="local-name()='dSessionStart'">
				<xsl:text>Start Date / Time</xsl:text>
			</xsl:when>
			<xsl:when test="local-name()='dDateTime'">
				<xsl:text>Date / Time</xsl:text>
			</xsl:when>
			<xsl:when test="local-name()='cSessionId' or local-name()='cSessionID'">
				<xsl:text>Session ID</xsl:text>
			</xsl:when>
			<xsl:when test="local-name()='cActivityType'">
				<xsl:text>Activity Type</xsl:text>
			</xsl:when>
			<xsl:when test="local-name()='cStructName'">
				<xsl:text>Page Name</xsl:text>
			</xsl:when>
			<xsl:when test="local-name()='cContentName'">
				<xsl:text>Content Name</xsl:text>
			</xsl:when>
			<xsl:when test="local-name()='cOtherDetail'">
				<xsl:text>Name</xsl:text>
			</xsl:when>
			<xsl:when test="local-name()='cActivityDetail'">
				<xsl:text>Activity Detail</xsl:text>
			</xsl:when>
			<xsl:when test="local-name()='cPrimaryDetail'">
				<xsl:text>Item Name</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="local-name()"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="*" mode ="MemberActivityReport_ColsValues">
		<xsl:variable name="origName" select="local-name()"/>
		<xsl:variable name="newName" >
			<xsl:apply-templates select="." mode="MemberActivityReport_ColsNames"/>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="$origName='nSessionSeconds'">
				<td>
					<xsl:value-of select="format-number(node() div 60,'#,###.00')"/>
				</td>
			</xsl:when>
			<xsl:when test="$newName!=$origName">
				<td>
					<xsl:choose>
						<xsl:when test="$origName='cDirName'">
							<a href="{$appPath}?ewCmd=MemberActivity&amp;UserId={../nDirKey/node()}" title="View {node()}'s Individual Activity">
								<xsl:value-of select="node()"/>
							</a>
						</xsl:when>
						<xsl:when test="$origName='cSessionId' or $origName='cSessionID'">
							<a href="{$appPath}?ewCmd=MemberActivity&amp;SessionId={node()}" title="View Activity For Session {node()}" class="btn btn-default btn-xs">
								<i class="fa fa-chevron-down">
									<xsl:text> </xsl:text>
								</i>
								Session Detail
							</a>
						</xsl:when>
						<xsl:when test="$origName='dSessionStart' or $origName='dDateTime'">
							<xsl:call-template name="DD_Mon_YYYY">
								<xsl:with-param name="date">
									<xsl:value-of select="node()"/>
								</xsl:with-param>
								<xsl:with-param name="showTime">true</xsl:with-param>
							</xsl:call-template>
						</xsl:when>
						<xsl:when test="$origName='cPrimaryDetail'">
							<xsl:value-of select="node()"/>
							<xsl:if test="parent::*/nSecondaryId/node()!='0'">
								/ <xsl:value-of select="parent::*/nSecondaryId/node()"/>
							</xsl:if>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="node()"/>
						</xsl:otherwise>
					</xsl:choose>
				</td>
			</xsl:when>
		</xsl:choose>
	</xsl:template>

	<!--BJR Codes-->
	<xsl:template match="Page[@layout='DirectoryCodes']" mode="Admin">
		<div class="row" id="template_AdvancedMode">
			<div class="col-md-3">
				<div class="alert alert-info">
					<i class="fa fa-info-sign fa-3x pull-right">
						<xsl:text> </xsl:text>
					</i>
					<h4>Hint</h4>
					<p>Voucher codes are groups of codes that can either be applied to a discount or can enable the user access to a unique subscriber group.</p>
					<p>Codes Must be created in advanced in code sets</p>
				</div>
			</div>
			<div class="col-md-9">
				<div class="panel panel-default">
					<div class="panel-heading">
						<xsl:choose>
							<xsl:when test="ContentDetail/Content[@type='xform']">
								<p class="btn-group headerButtons">
									<a href="{$appPath}?ewCmd=MemberCodes&amp;pgid={/Page/@id}" class="adminButton edit" title="Back to Member Codes">Back to Member Codes</a>
								</p>
							</xsl:when>
							<xsl:otherwise>
								<a href="{$appPath}?ewCmd=MemberCodes&amp;pgid={/Page/@id}&amp;subCmd=AddCodeSet" class="btn btn-success" title="Add a new code set">
									<i class="fa fa-plus fa-white">
										<xsl:text> </xsl:text>
									</i><xsl:text> </xsl:text>Add New Code Set
								</a>
								<h4>
									<xsl:value-of select="@name"/>
								</h4>
							</xsl:otherwise>
						</xsl:choose>
					</div>
					<div class="panel-content">
						<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
						<xsl:apply-templates select="ContentDetail/Content[@type!='xform']" mode="DirectoryCodes"/>
					</div>
				</div>
			</div>

		</div>
	</xsl:template>

	<xsl:template match="Content[@type='CodeList']" mode="DirectoryCodes">
		<table class="table">
			<tr>
				<th>&#160;</th>
				<th>Code Name</th>
				<th>Code Groups</th>
				<th>Total Codes</th>
				<th>Unused</th>
				<th>Used</th>
				<th>Published</th>
				<th>Expires</th>
			</tr>
			<xsl:apply-templates select="tblCodes/Code" mode="DirectoryCodesList"/>
		</table>
	</xsl:template>

	<xsl:template match="Code" mode="DirectoryCodesList">
		<tr onmouseover="this.className='rowOver'" onmouseout="this.className=''">
			<td>
				<xsl:call-template name="StatusLegend">
					<xsl:with-param name="status">
						<xsl:choose>
							<xsl:when test="@nStatus = 1 or not(@nStatus) or @nstatus=''">
								<xsl:text>1</xsl:text>
							</xsl:when>
							<xsl:otherwise>
								<xsl:text>0</xsl:text>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:with-param>
				</xsl:call-template>
			</td>
			<td>
				<xsl:value-of select="@cCodeName"/>
			</td>
			<td>
				<xsl:for-each select="Groups">
					<xsl:if test="position()!=1">
						<br/>
					</xsl:if>
					<a href="{$appPath}?ewCmd=ListUsers&amp;parid={@nDirKey}" title="List Users for '{@cDirName}'">
						<xsl:value-of select="@cDirName"/>
					</a>
				</xsl:for-each>
			</td>
			<td>
				<xsl:value-of select="@nUnused + @nUsed"/>
			</td>
			<td>
				<xsl:value-of select="@nUnused"/>
			</td>
			<td>
				<xsl:value-of select="@nUsed"/>
			</td>
			<td>
				<xsl:if test="@dPublishDate!=''">
					<xsl:call-template name="DD_Mon_YYYY">
						<xsl:with-param name="date">
							<xsl:value-of select="@dPublishDate"/>
						</xsl:with-param>
						<xsl:with-param name="showTime">false</xsl:with-param>
					</xsl:call-template>
				</xsl:if>
			</td>
			<td>
				<xsl:if test="@dExpireDate!=''">
					<xsl:call-template name="DD_Mon_YYYY">
						<xsl:with-param name="date">
							<xsl:value-of select="@dExpireDate"/>
						</xsl:with-param>
						<xsl:with-param name="showTime">false</xsl:with-param>
					</xsl:call-template>
				</xsl:if>
			</td>

			<td class="btn-group">
				<a href="{$appPath}?ewCmd=MemberCodes&amp;pgid={/Page/@id}&amp;id={@nCodeKey}" class="btn btn-default" title="Edit this new code set">View/Add Codes</a>
				<a href="{$appPath}?ewCmd=MemberCodes&amp;pgid={/Page/@id}&amp;id={@nCodeKey}&amp;subCmd=ManageCodeGroups" class="btn btn-default" title="Edit this new code set">Code Memberships</a>
			</td>
		</tr>
	</xsl:template>

	<xsl:template match="Content[@type='CodeSet']" mode="DirectoryCodes">
		<table class="table">
			<tr>
				<th>&#160;</th>
				<th>Code Name</th>
				<th>Code Groups</th>
				<th>Total Codes</th>
				<th>Unused</th>
				<th>Used</th>
				<th>Published</th>
				<th>Expires</th>
				<th class="btn-group headerButtons">
					&#160;
				</th>
			</tr>
			<xsl:apply-templates select="tblCodes/Code" mode="DirectoryCodesList"/>
		</table>

	</xsl:template>

	<xsl:template match="Code" mode="DirectoryCodesList">
		<tr onmouseover="this.className='rowOver'" onmouseout="this.className=''">
			<td>
				<xsl:call-template name="StatusLegend">
					<xsl:with-param name="status">
						<xsl:choose>
							<xsl:when test="@nStatus = 1 or not(@nStatus) or @nstatus=''">
								<xsl:text>1</xsl:text>
							</xsl:when>
							<xsl:otherwise>
								<xsl:text>0</xsl:text>
							</xsl:otherwise>
						</xsl:choose>
					</xsl:with-param>
				</xsl:call-template>
			</td>
			<td>
				<xsl:value-of select="@cCodeName"/>
			</td>
			<td>
				<xsl:for-each select="Groups">
					<xsl:if test="position()!=1">
						<br/>
					</xsl:if>
					<a href="{$appPath}?ewCmd=ListUsers&amp;parid={@nDirKey}" title="List Users for '{@cDirName}'">
						<xsl:value-of select="@cDirName"/>
					</a>
				</xsl:for-each>
			</td>
			<td>
				<xsl:value-of select="@nUnused + @nUsed"/>
			</td>
			<td>
				<xsl:value-of select="@nUnused"/>
			</td>
			<td>
				<xsl:value-of select="@nUsed"/>
			</td>
			<td>
				<xsl:if test="@dPublishDate!=''">
					<xsl:call-template name="DD_Mon_YYYY">
						<xsl:with-param name="date">
							<xsl:value-of select="@dPublishDate"/>
						</xsl:with-param>
						<xsl:with-param name="showTime">false</xsl:with-param>
					</xsl:call-template>
				</xsl:if>
			</td>
			<td>
				<xsl:if test="@dExpireDate!=''">
					<xsl:call-template name="DD_Mon_YYYY">
						<xsl:with-param name="date">
							<xsl:value-of select="@dExpireDate"/>
						</xsl:with-param>
						<xsl:with-param name="showTime">false</xsl:with-param>
					</xsl:call-template>
				</xsl:if>
			</td>
			<td class="btn-group">
				<a href="{$appPath}?ewCmd=MemberCodes&amp;pgid={/Page/@id}&amp;id={@nCodeKey}" class="btn btn-default btn-xs" title="Edit this new code set">
					<i class="fa fa-edit">
						<xsl:text> </xsl:text>
					</i>Edit Group
				</a>
				<a href="{$appPath}?ewCmd=MemberCodes&amp;pgid={/Page/@id}&amp;id={@nCodeKey}&amp;subCmd=ManageCodes" class="btn btn-default btn-xs" title="Generate Codes">
					<i class="fa fa-plus">
						<xsl:text> </xsl:text>
					</i>View/Add Codes
				</a>
				<a href="{$appPath}?ewCmd=MemberCodes&amp;pgid={/Page/@id}&amp;id={@nCodeKey}&amp;subCmd=ManageCodeGroups" class="btn btn-default btn-xs" title="Edit this new code set">
					<i class="fa fa-group">
						<xsl:text> </xsl:text>
					</i>Code Memberships
				</a>
			</td>
		</tr>
	</xsl:template>

	<xsl:template match="Content[@type='SubCodeList']" mode="DirectoryCodes">
		<xsl:apply-templates select="tblCodes" mode="reportDetail"/>
	</xsl:template>

	<!-- -->
	<!--   ##################  Site Index  ##############################   -->
	<!-- -->
	<xsl:template match="Page[@layout='SiteIndex' or @ewCmd='SiteIndex']" mode="Admin">
		<div class="row">
			<div class="col-md-4">
				<div class="panel panel-default">
					<div class="panel-heading">
						<h3 class="panel-title">
							Last Index
						</h3>
					</div>
					<div class="panel-body">
						<xsl:for-each select="ContentDetail/Content/model/instance/IndexInfo/indexInfo">
							<dl class="tabled">
								<dt>Start Time</dt>
								<dd>
									<xsl:value-of select="@startTime"/>
								</dd>
								<dt>End Time</dt>
								<dd>
									<xsl:value-of select="@endTime"/>
								</dd>
								<dt>Pages Index</dt>
								<dd>
									<xsl:value-of select="@pagesIndexed"/>
								</dd>
								<dt>Pages Skipped</dt>
								<dd>
									<xsl:value-of select="@pagesSkipped"/>
								</dd>
								<dt>Content Items Indexed</dt>
								<dd>
									<xsl:value-of select="@contentCount"/>
								</dd>
								<dt>Content Items Skipped</dt>
								<dd>
									<xsl:value-of select="@contentSkipped"/>
								</dd>
								<dt>Documents Indexed</dt>
								<dd>
									<xsl:value-of select="@documentsIndexed"/>
								</dd>
								<dt>Documents Skipped</dt>
								<dd>
									<xsl:value-of select="@documentsSkipped"/>
								</dd>
							</dl>
						</xsl:for-each>
					</div>
				</div>
			</div>
			<div class="col-md-8">
				<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
			</div>
		</div>

	</xsl:template>

	<!--BJR Codes-->
	<xsl:template match="Page[@layout='ManageLookups']" mode="Admin">
		<div class="row" id="template_AdvancedMode">
			<div class="col-md-3">
				<div class="panel panel-default">
					<div class="panel-body">
						Lookups can be used for a range of features in the system. They often are used to provide a list of options for dropdown lists on forms.
					</div>
				</div>
			</div>
			<div class="col-md-9">
				<div class="panel panel-default">
					<div class="panel-heading">
						<p class="btn-group headerButtons">
							<xsl:if test="ContentDetail/Content[@type='xform']">
								<a href="{$appPath}?ewCmd=ManageLookups&amp;pgid={/Page/@id}" class="btn btn-default" title="Back to ManageLookups">
									<i class="fa fa-caret-left">&#160; </i>&#160;Back to Lookups List
								</a>
							</xsl:if>
						</p>
					</div>
					<div class="panel-body">
						<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
					</div>
					<xsl:apply-templates select="ContentDetail/Content[@type!='xform']" mode="ListLookups"/>
				</div>
			</div>

		</div>
	</xsl:template>

	<xsl:template match="Content[@type='Report']" mode="ListLookups">
		<div class="panel-body">
			<form method="get" role="form" class="form-inline">
				<input type="hidden" name="ewCmd" value="ManageLookups"/>
				<input type="hidden" name="lookupId" value="0"/>
				<div class="form-group">
					<label for="Category" >New Category Name</label>

					<input name="Category" id="Category"/>
				</div>
				<div class="form-group">
					<button type="submit" value="Add New Category" class="btn btn-success principle">
						<i class="fa fa-plus fa-white">
							<xsl:text> </xsl:text>
						</i><xsl:text> </xsl:text>Add New Category
					</button>
				</div>

			</form>
		</div>
		<div class="table-responsive">
			<table class="table manage-lookups table-hover ">
				<xsl:apply-templates select="Lookups/Category" mode="LookupList"/>
			</table>
		</div>
	</xsl:template>

	<xsl:template match="Category" mode="LookupList">
		<tr>
			<th colspan="2">
				<xsl:value-of select="@Name"/>
			</th>
			<th class="clearfix buttonCell">
				<a href="{$appPath}?ewCmd=ManageLookups&amp;lookupId=0&amp;Category={@Name}" class="btn btn-success pull-right">
					<i class="fa fa-plus fa-white">
						<xsl:text> </xsl:text>
					</i><xsl:text> </xsl:text>Add New Item
				</a>
			</th>
		</tr>
		<xsl:apply-templates select="Lookup" mode="LookupList"/>
	</xsl:template>

	<xsl:template match="Lookup" mode="LookupList">
		<tr>
			<td>
				<xsl:value-of select="cLkpKey/node()"/>
				<xsl:if test="cLkpParent/node()!=''">
					<xsl:variable name="parId" select="cLkpParent/node()"/>
					<xsl:text> </xsl:text>[<xsl:value-of select="ancestor::Lookups/Category/Lookup[@id=$parId]/cLkpKey/node()"/>]
				</xsl:if>
			</td>
			<td>
				<xsl:value-of select="cLkpValue/node()"/>
			</td>
			<td class="clearfix">
				<a href="{$appPath}?ewCmd=ManageLookups&amp;ewCmd2=delete&amp;lookupId={@id}&amp;Category={../@Name}" class="btn btn-danger btn-xs pull-right">
					<i class="fa fa-trash-o fa-white">
						<xsl:text> </xsl:text>
					</i><xsl:text> </xsl:text>Del
				</a>
				<a href="{$appPath}?ewCmd=ManageLookups&amp;lookupId={@id}&amp;Category={../@Name}" class="btn btn-primary btn-xs pull-right">
					<i class="fa fa-edit fa-white">
						<xsl:text> </xsl:text>
					</i><xsl:text> </xsl:text>Edit
				</a>
				<a href="{$appPath}?ewCmd=ManageLookups&amp;ewCmd2=MoveTop&amp;lookupId={@id}&amp;Category={../@Name}" class="btn btn-arrow btn-primary btn-xs" title="Click here to move this page up by top">
					<i class="fa fa-arrow-up fa-white">
						<xsl:text> </xsl:text>
					</i>
				</a>
				<a href="{$appPath}?ewCmd=ManageLookups&amp;ewCmd2=MoveUp&amp;lookupId={@id}&amp;Category={../@Name}" class="btn btn-arrow btn-primary btn-xs" title="Click here to move this page up by one space">
					<i class="fa fa-chevron-up fa-white">
						<xsl:text> </xsl:text>
					</i>
				</a>
				<a href="{$appPath}?ewCmd=ManageLookups&amp;ewCmd2=MoveDown&amp;lookupId={@id}&amp;Category={../@Name}" class="btn btn-arrow btn-primary btn-xs" title="Click here to move this page down by one space">
					<i class="fa fa-chevron-down fa-white">
						<xsl:text> </xsl:text>
					</i>
				</a>
				<a href="{$appPath}?ewCmd=ManageLookups&amp;ewCmd2=MoveBottom&amp;lookupId={@id}&amp;Category={../@Name}" class="btn btn-arrow btn-primary btn-xs" title="Click here to move this page down by bottom">
					<i class="fa fa-arrow-down fa-white">
						<xsl:text> </xsl:text>
					</i>
				</a>
			</td>
		</tr>
	</xsl:template>

	<xsl:template match="Page[@layout='FileImport']" mode="Admin">
		<div class="template" id="template_EditStructure">
			<div class="row">
				<div class="col-md-4">
					<div class="panel panel-danger">
						<div class="panel-heading">
							<h3>
								<i class="fa fa-exclamation">&#160;</i>&#160;Caution
							</h3>
						</div>
						<div class="panel-body">
							<p>
								Using the Import tool can be destructive as it can overwrite existing data.
								<br/>
								<strong>Please use with caution.</strong>
							</p>
						</div>
					</div>
				</div>
				<div class="col-md-8" >
					<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
				</div>
			</div>
		</div>
	</xsl:template>


	<!-- -->
	<!--   ##################  Content History   ##############################   -->
	<!-- -->
	<!-- -->
	<xsl:template match="Page[@layout='ContentVersions']" mode="Admin">
		<div class="report" id="tpltEditStructure">
			<div>
				<h3>
					History for '<xsl:value-of select="ContentDetail/ContentVersions/Version/@type"/>' '<xsl:value-of select="ContentDetail/ContentVersions/Version/@name"/>'
				</h3>
				<table cellpadding="0" cellspacing="0" class="table">
					<tr>
						<th>Status</th>
						<th>Version</th>
						<th>Published</th>
						<th>Published by</th>
						<th>Updated</th>
						<th>Updated by</th>
						<th>&#160;</th>
					</tr>
					<xsl:for-each select="ContentDetail/ContentVersions/Version">
						<tr>
							<td>
								<xsl:call-template name="StatusLegend">
									<xsl:with-param name="status" select="@status"/>
								</xsl:call-template>
							</td>
							<td>
								<xsl:value-of select="@version"/>
							</td>
							<td>
								<xsl:call-template name="DD_Mon_YY">
									<xsl:with-param name="date" select="@publish"/>
									<xsl:with-param name="showTime" select="'true'"/>
								</xsl:call-template>
							</td>
							<td>
								<xsl:choose>
									<xsl:when test="ownerDetail/User/FirstName/node()!=''">
										<xsl:value-of select="ownerDetail/User/FirstName/node()"/>&#160;<xsl:value-of select="ownerDetail/User/LastName/node()"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="@owner"/>
									</xsl:otherwise>
								</xsl:choose>
							</td>
							<td>
								<xsl:call-template name="DD_Mon_YY">
									<xsl:with-param name="date" select="@update"/>
									<xsl:with-param name="showTime" select="'true'"/>
								</xsl:call-template>
							</td>
							<td>
								<xsl:choose>
									<xsl:when test="updaterDetail/User/FirstName/node()!=''">
										<xsl:value-of select="updaterDetail/User/FirstName/node()"/>&#160;<xsl:value-of select="updaterDetail/User/LastName/node()"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="@updater"/>
									</xsl:otherwise>
								</xsl:choose>
							</td>
							<td>

								<xsl:choose>
									<xsl:when test="@primaryId='0' or @primaryId = @id">
										<a href="{$appPath}?ewCmd=PreviewOn&amp;pgid={/Page/@id}&amp;artid={@primaryId}" class="btn btn-xs btn-primary">
											<i class="fa fa-eye fa-white">
												<xsl:text> </xsl:text>
											</i> View
										</a>
										<a href="{$appPath}?ewCmd=EditContent&amp;pgid={/Page/@id}&amp;id={@id}" class="btn btn-xs btn-primary" title="Click here to edit this content">
											<i class="fa fa-pencil fa-white">
												<xsl:text> </xsl:text>
											</i><xsl:text> </xsl:text>Edit
										</a>
									</xsl:when>
									<xsl:otherwise>
										<a href="{$appPath}?ewCmd=PreviewOn&amp;pgid={/Page/@id}&amp;artid={@primaryId}&amp;verId={@id}" class="btn btn-xs btn-primary">
											<i class="fa fa-eye fa-white">
												<xsl:text> </xsl:text>
											</i>             Preview
										</a>
										<a href="{$appPath}?ewCmd=RollbackContent&amp;pgid={/Page/@id}&amp;id={@primaryId}&amp;verId={@id}" class="btn btn-xs btn-primary" title="Click here to rollback to this version">
											<i class="fa fa-pencil fa-white">
												<xsl:text> </xsl:text>
											</i><xsl:text> </xsl:text>
											Edit &amp; Revert
										</a>
									</xsl:otherwise>
								</xsl:choose>
							</td>
						</tr>
					</xsl:for-each>
				</table>
			</div>
		</div>
	</xsl:template>
	<!-- -->
	<xsl:template match="Page[@layout='VersionControlProcess']" mode="Admin">
		<div class="row" id="tpltListReports">
			<div class="col-md-9">
				<xsl:apply-templates select="ContentDetail/Content/GenericReport" mode="reportDetail"/>
			</div>
			<div class="col-md-3">
				<div class="panel">
					<div class="panel-body">
						<p>This lists all content that is awaiting approval from an administrator.</p>
						<p>To approve the content, vlick the Edit button, review the content, and change the status to Approved or Live.</p>
						<p>The version numbers show the most recent version awaiting approval. Numbers in square brackets indicate the current Live/Approved version.</p>
						<p>Note: this does not track the location of the content or any changes to related items.</p>
					</div>
				</div>
			</div>
		</div>
	</xsl:template>
	<!-- -->

	<xsl:template match="Pending" mode="reportDetailListHeader">
		<tr>
			<td>&#160;</td>
			<xsl:apply-templates select="*" mode="reportHeader"/>
		</tr>
	</xsl:template>

	<!-- REPORT:  Pending Report Detail List -->
	<xsl:template match="Pending" mode="reportDetailList">
		<tr>
			<xsl:if test="(position()+1) mod 2=0">
				<xsl:attribute name="class">alternate</xsl:attribute>
			</xsl:if>
			<td>
				<xsl:call-template name="StatusLegend">
					<xsl:with-param name="status" select="@status"/>
				</xsl:call-template>
			</td>
			<xsl:apply-templates select="*" mode="reportCell"/>
			<xsl:apply-templates select="." mode="reportDetailbtn-group"/>
		</tr>
		<xsl:if test="Name/Related/Content or Name/Locations/Location">
			<tr>
				<xsl:if test="(position()+1) mod 2=0">
					<xsl:attribute name="class">alternate</xsl:attribute>
				</xsl:if>
				<xsl:apply-templates select="Name" mode="Pending"/>
			</tr>
		</xsl:if>
	</xsl:template>

	<xsl:template match="Name[ancestor::*[name()='Pending']]" mode="reportCell">
		<td>
			<a href="/?ewCmd=Normal&amp;pgid={parent::Pending/Metadata/Locations/Location/@pageid}&amp;artid={parent::Pending/@id}">
				<xsl:value-of select="node()"/>
			</a>
		</td>
	</xsl:template>

	<!-- -->
	<xsl:template match="ContentXml" mode="reportHeader">
		<td>
			<xsl:text> </xsl:text>
		</td>
	</xsl:template>

	<!-- -->
	<xsl:template match="ContentXml" mode="reportCell">
		<td>
			<xsl:text> </xsl:text>
		</td>
	</xsl:template>
	<!-- -->
	<xsl:template match="Name" mode="Pending">
		<td colspan="{count(../*)+1}" class="notop">
			<xsl:if test="Locations/Location">
				<dl class="flat">
					<dt>Located on:</dt>
					<xsl:for-each select="Locations/Location">
						<dd>
							<a href="{$appPath}?ewCmd=Normal&amp;pgid={@pageid}" title="View this page">
								<xsl:value-of select="@page"/>
							</a>
						</dd>
					</xsl:for-each>
				</dl>
			</xsl:if>
			<xsl:if test="Related/Content">
				<dl class="flat">
					<dt>Related to:</dt>
					<xsl:for-each select="Related/Content">
						<dd>
							<xsl:value-of select="@name"/>
							<xsl:text> (</xsl:text>
							<xsl:value-of select="@type"/>
							<xsl:text>)</xsl:text>
						</dd>
					</xsl:for-each>
				</dl>
			</xsl:if>
		</td>
	</xsl:template>



	<!-- -->

	<!-- -->
	<!-- REPORT:  Generic Report Buttons - Empty By Default -->
	<xsl:template match="Pending" mode="reportDetailbtn-group">
		<xsl:variable name="versionId">
			<xsl:if test="@versionid!=''">
				<xsl:text>&amp;verId=</xsl:text>
				<xsl:value-of select="@versionid"/>
			</xsl:if>
		</xsl:variable>
		<xsl:variable name="ContentId">
			<xsl:if test="@ContentId!=''">
				<xsl:value-of select="@ContentId"/>
			</xsl:if>
		</xsl:variable>
		<td class="btn-group">
			<xsl:choose>
				<xsl:when test="@type='Review'">
				</xsl:when>
				<xsl:otherwise>
					<a href="{$appPath}?ewCmd=PreviewOn&amp;pgid={@pageid}&amp;artid={@id}{$versionId}" class="btn btn-xs btn-default" title="Click here to edit this content">
						<i class="fa fa-eye">
							<xsl:text> </xsl:text>
						</i>
						<xsl:text> </xsl:text>Preview
					</a>

					<a href="{$appPath}?ewCmd=ContentVersions&amp;id={@id}{$versionId}" class="btn btn-xs btn-default" title="Click here to edit this content">
						<i class="fa fa-history">
							<xsl:text> </xsl:text>
						</i>
						<xsl:text> </xsl:text>History
					</a>
				</xsl:otherwise>
			</xsl:choose>

			<a href="{$appPath}?ewCmd=EditContent&amp;id={@id}{$versionId}&amp;ewRedirCmd=AwaitingApproval" class="btn btn-xs btn-primary" title="Click here to edit this content">
				<i class="fa fa-pencil fa-white">
					<xsl:text> </xsl:text>
				</i>
				<xsl:text> </xsl:text>Edit
			</a>
			<xsl:if test="@status='0' or @status='3'">
				<a href="{$appPath}?ewCmd=DeleteContent&amp;pgid={/Page/@id}&amp;id={@id}" class="btn btn-xs btn-danger" title="Click here to delete this item">
					<i class="fa fa-trash fa-white">
						<xsl:text> </xsl:text>
					</i>
				</a>
			</xsl:if>
		</td>
	</xsl:template>


	<!-- REPORT:  Generic Report Buttons - Empty By Default -->
	<!--
  <xsl:template match="Vote" mode="reportDetailbtn-group">
    <xsl:variable name="id" select="/Page/Request/QueryString/Item[@name='id']"/>
    -->
	<!--<xsl:variable name="pgid" select="/Page/Request/QueryString/Item[@name='pgid']"/>-->
	<!--
    <xsl:variable name="pgid" select="$page/@id"/>
    <td class="btn-group">
      <xsl:choose>
        <xsl:when test="$page/ContentDetail/Content/GenericReport/Vote/Status/node()='1'">
          <a href="{$appPath}?ewCmd=ManagePollVotes&amp;pgid={$pgid}&amp;id={$id}&amp;voteId={@Vote_Id}&amp;ewCmd2=hide" class="adminButton hide" title="Click here to exclude this vote from the poll">Exclude Vote</a>
        </xsl:when>
        <xsl:otherwise>
          <a href="{$appPath}?ewCmd=ManagePollVotes&amp;pgid={$pgid}&amp;id={$id}&amp;voteId={@Vote_Id}&amp;ewCmd2=show" class="adminButton show" title="Click here to reinstate this vote in the poll">Reinstate Vote</a>
        </xsl:otherwise>
      </xsl:choose>
    </td>
  </xsl:template>-->

	<!-- REPORT:  Generic Report Buttons - Empty By Default -->
	<xsl:template match="Vote" mode="reportDetailbtn-group">
		<xsl:variable name="id" select="/Page/Request/QueryString/Item[@name='id']"/>
		<!--<xsl:variable name="pgid" select="/Page/Request/QueryString/Item[@name='pgid']"/>-->
		<xsl:variable name="pgid" select="$page/@id"/>
		<td class="btn-group">
			<a href="{$appPath}?ewCmd=ManagePollVotes&amp;pgid={$pgid}&amp;id={$id}&amp;voteId={@Vote_Id}&amp;ewCmd2=hide" class="adminButton hide" title="Click here to exclude this vote from the poll">Exclude Vote</a>
			<a href="{$appPath}?ewCmd=ManagePollVotes&amp;pgid={$pgid}&amp;id={$id}&amp;voteId={@Vote_Id}&amp;ewCmd2=show" class="adminButton show" title="Click here to reinstate this vote in the poll">Reinstate Vote</a>
		</td>
	</xsl:template>


	<!-- -->
	<!--   ##################  Page Versions  ##############################   -->
	<!-- -->
	<!-- -->
	<xsl:template match="Page[@layout='PageVersions']" mode="Admin">
		<div class="rows" id="tpltEditStructure">
			<div class="cols-md-12">
				<div class="panel">
					<div class="panel-body">
						<p>Version history allows you create different versions of the same page for a number of reasons. Such as for different users, languages, split testing or working versions</p>
						<p>A user will see the first page on this list that matches their own permissions, if the user does not have access to the "original" page they will not see any of the alternative versions either.</p>
					</div>

					<table cellpadding="0" cellspacing="0" class="table">
						<tr>
							<th>Status</th>
							<th>Version</th>
							<th>Type</th>
							<th>Lang</th>
							<th>Audience</th>
							<th>Priority</th>
							<th></th>
							<th>&#160;</th>
						</tr>
						<xsl:for-each select="ContentDetail/PageVersions/Version">
							<tr>
								<td>
									<xsl:call-template name="StatusLegend">
										<xsl:with-param name="status" select="@status"/>
									</xsl:call-template>
								</td>
								<td>
									<a href="{$appPath}?ewCmd=Normal&amp;pgid={@id}" title="Click here to edit this content">
										<xsl:value-of select="@name"/> - <xsl:value-of select="@description"/>
									</a>
								</td>
								<td>
									<xsl:choose>
										<xsl:when test="@type=1">
											Personalisation
										</xsl:when>
										<xsl:when test="@type=2">
											Working Copy
										</xsl:when>
										<xsl:when test="@type=3">
											Language
										</xsl:when>
										<xsl:when test="@type=4">
											Split Test
										</xsl:when>
										<xsl:otherwise>
											Original
										</xsl:otherwise>
									</xsl:choose>
								</td>
								<td>
									<xsl:value-of select="@lang"/>
								</td>
								<td>
									<a href="{$appPath}?ewCmd=EditPagePermissions&amp;pgid={@id}" title="Click here to edit this content">
										<xsl:value-of select="@Groups"/>
										<xsl:if test="@Groups=''">Open</xsl:if>
									</a>
								</td>
								<td>
									<xsl:if test="@primaryId!=@id">
										<!--<input type="button" class="arrowbutton up" onclick="moveUp({@id});" title="Click here to move this page up by one space"/>-->
										<a href="{$appPath}?ewCmd=PageVersionMoveUp&amp;pgid={@id}" class="btn btn-arrow btn-primary btn-xs" title="Click here to move this page up by one space">
											<i class="fa fa-chevron-up fa-white">
												<xsl:text> </xsl:text>
											</i>
										</a>
										<!--span class="hidden"> | </span-->
										<a href="{$appPath}?ewCmd=PageVersionMoveDown&amp;pgid={@id}" class="btn btn-arrow btn-primary btn-xs" title="Click here to move this page down by one space">
											<i class="fa fa-chevron-down fa-white">
												<xsl:text> </xsl:text>
											</i>
										</a>
									</xsl:if>
								</td>
								<td>
									<!--a href="{$appPath}?ewCmd=Normal&amp;pgid={@id}" class="adminButton edit" title="Click here to edit this content">Edit</a-->
									<a href="{$appPath}?ewCmd=NewPageVersion&amp;pgid={@id}&amp;vParId={@primaryId}" class="btn btn-xs btn-success" title="Click here to create a copy version of this page">
										<i class="fa fa-file fa-white">
											<xsl:text> </xsl:text>
										</i><xsl:text> </xsl:text>New Version
									</a>
									<xsl:if test="@primaryId!=@id">
										<a href="{$appPath}?ewCmd=DeletePage&amp;pgid={@id}" class="btn btn-xs btn-danger" title="Click here to delete this page">
											<i class="fa fa-remove fa-white">
												<xsl:text> </xsl:text>
											</i><xsl:text> </xsl:text>Delete
										</a>
									</xsl:if>
								</td>
							</tr>
						</xsl:for-each>
					</table>
				</div>
			</div>
		</div>
	</xsl:template>

	<!-- -->
	<!--   ##################  Poll Management   ##############################   -->
	<!-- -->

	<xsl:template match="Page[@layout='ManagePollVotes' or (@layout='AdminXForm' and @ewCmd='ManagePollVotes')]" mode="Admin">
		<xsl:variable name="pgid" select="$page/@id"/>
		<div class="template" id="template_AdvancedMode">
			<div id="column2">
				<h3>
					<xsl:value-of select="ContentDetail/Content/@name"/>
				</h3>
				<xsl:apply-templates select="ContentDetail/Content/GenericReport" mode="reportDetail"/>
				<p class="btn-group">
					<a href="?pgid={$pgid}" class="adminButton" title="Return to the page">Back</a>

				</p>
			</div>
			<div id="column1">
				<h3>Manage Poll Voted</h3>
				<p>This lists all votes for the requested poll.</p>
				<p>Where data is available it will show the e-mail address and/or IP address of the voter.</p>
				<p>Votes can be excluded by clicking the Exclude Vote button.  To reinstate the vote click the Reinstate vote button.</p>
				<h4>Key</h4>
				<p>
					<img src="/ewcommon/images/icons/live.gif" width="15" height="15" alt="Vote active"/>
					<xsl:text> Vote included</xsl:text>
				</p>
				<p>
					<img src="/ewcommon/images/icons/rejectedf.gif" width="15" height="15" alt="Vote excluded"/>
					<xsl:text> Vote excluded</xsl:text>

				</p>

			</div>
			<div class="terminus">&#160;</div>
		</div>
	</xsl:template>


	<!-- ##################################################################################################### -->
	<!-- #######################################   REPORTING  TEMPLATES  ##################################### -->
	<!-- ##################################################################################################### -->

	<!-- REPORT:  Generic Report Detail -->
	<xsl:template match="tblCodes | genericDatasetRowname | GenericReport" mode="reportDetail">

		<xsl:variable name="order">
			<xsl:choose>
				<xsl:when test="/Page/Request//Item[@name='sortDir']/node()">
					<xsl:value-of select="/Page/Request//Item[@name='sortDir']/node()"/>
				</xsl:when>
				<xsl:otherwise>ascending</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="sortCol" select="/Page/Request//Item[@name='sortCol']/node()"/>
		<div class="panel">
			<div class="panel-heading">
				<div class="btn-group pull-right">
					<a href="/ewcommon/tools/excel.ashx?{/Page/Request/ServerVariables/Item[@name='QUERY_STRING']/node()}" class="btn btn-success btn-xs pull-right" target="_new">
						<i class="fa icon-file-excel">&#160;</i>&#160;Excel Download
					</a>
				</div>
				<div class="title">
					&#160;
				</div>
			</div>
			<table cellpadding="0" class="table panel-body" id="sort_{$sortCol}">
				<xsl:variable name="scName">
					<xsl:apply-templates select="*[1]/*[number($sortCol)]" mode="getContectNodeName"/>
				</xsl:variable>
				<xsl:variable name="sortCells" select="*[not(contains(name(),'Selector'))]/*[number($sortCol)]"/>
				<xsl:variable name="isNumeric">
					<xsl:if test="count($sortCells)=count($sortCells[number(.)=number(.)])">true</xsl:if>
				</xsl:variable>
				<xsl:variable name="datatype">
					<xsl:choose>
						<xsl:when test="$isNumeric='true'">number</xsl:when>
						<xsl:otherwise>text</xsl:otherwise>
					</xsl:choose>
				</xsl:variable>

				<xsl:apply-templates select="*[1]" mode="reportDetailListHeader"/>

				<xsl:choose>
					<xsl:when test="(*[1]/*[number($sortCol)]//LastName and *[1]/*[number($sortCol)]//FirstName) and $scName!='Username'">
						<xsl:apply-templates select="*" mode="reportDetailList">
							<xsl:sort select="*[number($sortCol)]//LastName"  order="{$order}"/>
							<xsl:sort select="*[number($sortCol)]//FirstName"  order="{$order}"/>
						</xsl:apply-templates>
					</xsl:when>
					<xsl:otherwise>

						<xsl:apply-templates select="*" mode="reportDetailList">
							<xsl:sort select="*[number($sortCol)]" order="{$order}" data-type="{$datatype}"/>
						</xsl:apply-templates>
					</xsl:otherwise>
				</xsl:choose>
				<!--xsl:if test="count(*[not(contains(name(),'Selector'))])=0 and name()!='summary'">
				<tr>
					<td>&#160;</td>
				</tr>
			</xsl:if-->
			</table>

			<xsl:apply-templates select="." mode="reportError"/>
		</div>
	</xsl:template>

	<!-- REPORT:  Generic Report Header -->
	<xsl:template match="*" mode="reportDetailListHeader">
		<tr>
			<xsl:apply-templates select="*" mode="reportHeader"/>
		</tr>
	</xsl:template>

	<!-- REPORT:  Generic Report Detail List -->
	<xsl:template match="*" mode="reportDetailList">
		<tr>
			<xsl:if test="(position()+1) mod 2=0">
				<xsl:attribute name="class">alternate</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="*" mode="reportCell"/>
			<xsl:apply-templates select="." mode="reportDetailbtn-group"/>
		</tr>
	</xsl:template>

	<!-- REPORT:  Generic Report Error Handling -->
	<xsl:template match="*" mode="reportError">
		<xsl:if test="@error!=''">
			<div class="alert">
				<xsl:choose>
					<xsl:when test="@error='timeout'">
						<p>The report was unable to retrieve the requested information in good time.  This may be due to the size of the report or the current activity on the website.</p>
						<p>To rectify this, please try the following:</p>
						<ul>
							<li>If there are any filters available for this report, please try narrowing down your criteria.</li>
							<li>Try retrieving the report later.</li>
						</ul>
						<p>
							If the problem persists, then please contact the website administrators.
						</p>
					</xsl:when>
				</xsl:choose>
			</div>
		</xsl:if>
	</xsl:template>

	<!-- ##################################################################################################### -->
	<!-- #######################################    REPORTING  BESPOKE   ##################################### -->
	<!-- ##################################################################################################### -->

	<!-- REPORT:  Member Codes Header -->
	<xsl:template match="Code" mode="reportDetailListHeader">
		<tr>
			<th colspan="{count(*)}">
				Directory Codes
			</th>
		</tr>

		<tr>
			<xsl:apply-templates select="*" mode="reportHeader"/>
		</tr>
	</xsl:template>

	<!-- REPORT:  Specific Report Button Header - Not Empty By Default -->
	<xsl:template match="Vote" mode="reportDetailListButtonHeader">
		<th>
			<xsl:text> </xsl:text>
		</th>
	</xsl:template>
	<!-- ##################################################################################################### -->
	<!-- ################################# CART DOWNLOAD AND ACTIVITY REPORTING ############################## -->
	<!-- ##################################################################################################### -->

	<xsl:template match="Page[@layout='CartReportsMain']" mode="Admin">
		<xsl:variable name="contextCmd">
			<xsl:choose>
				<xsl:when test="$page/@editContext!=''">
					<xsl:value-of select="/Page/@editContext"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="/Page/@ewCmd"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<div class="row" id="tpltCartActivity">
			<div class="col-md-3 btn-group-vertical"  id="column1">
				<xsl:for-each select="AdminMenu/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=$contextCmd]]/MenuItem">
					<a href="{$appPath}?ewCmd={@cmd}" class="btn btn-default">
						<i class="fa {@icon}">&#160;</i> &#160;
						<xsl:value-of select="@name"/>
					</a>
				</xsl:for-each>
			</div>
			<div class="col-md-9" id="column2">
				&#160;
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Page[@layout='CartDownload' or @layout='CartActivity' or @layout='CartActivityDrilldown' or @layout='CartActivityPeriod']" mode="Admin">
		<div class="row" id="tpltCartActivity">
			<div class="col-md-3">
				<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
			</div>
			<div class="col-md-9">
				<xsl:choose>
					<xsl:when test="ContentDetail/Content[@type='Report']/Report">
						<xsl:apply-templates select="ContentDetail/Content[@type='Report']/Report" mode="CartReport"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="." mode="CartReport"/>
					</xsl:otherwise>
				</xsl:choose>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Page" mode="CartReport">

	</xsl:template>

	<xsl:template match="Report" mode="CartReport">
		<div class="panel panel-default">
			<xsl:apply-templates select="." mode="reportHeader"/>
			<xsl:apply-templates select="." mode="reportBody"/>
		</div>
	</xsl:template>

	<!-- REPORT HEADERS -->

	<xsl:template match="Report" mode="reportHeader">

		<div class="panel-heading">
			<h3 class="title">
				<xsl:value-of select="@cOrderType"/>
				<xsl:text>s </xsl:text>
				<xsl:value-of select="@dBegin"/>
				<xsl:text> - </xsl:text>
				<xsl:value-of select="@dEnd"/>
			</h3>
		</div>
		<div class="panel-body">

			<xsl:choose>
				<xsl:when test="@cProductType!=''">
					<xsl:text>Product Type: </xsl:text>
					<xsl:value-of select="@cProductType"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:if test="bsplit=1">
						<xsl:text>By Type</xsl:text>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>
			<br/>
			<xsl:text>Currency: </xsl:text>
			<xsl:choose>
				<xsl:when test="@cCurrencySymbol!=''">
					<xsl:value-of select="@cCurrencySymbol"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>All/None</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
			<br/>
			<xsl:text>Status: </xsl:text>
			<xsl:value-of select="@nOrderStatus1"/>
			<xsl:text>, </xsl:text>
			<xsl:value-of select="@nOrderStatus2"/>
		</div>
	</xsl:template>

	<xsl:template match="Report[@nProductId&gt;0]" mode="reportHeader">
		<div class="panel-heading">
			<h3 class="title">
				<xsl:value-of select="@cOrderType"/>
				<xsl:text>s of </xsl:text>
				<xsl:value-of select="Item/Item_Name"/>
				<xsl:text> for </xsl:text>
				<!--<xsl:value-of select="@type"/>-->
				<xsl:value-of select="@dBegin"/>
				<xsl:text> - </xsl:text>
				<xsl:value-of select="@dEnd"/>
			</h3>
		</div>
		<div class="panel-body">

			<xsl:choose>
				<xsl:when test="@nProductId &gt; 0">
					<xsl:text>Product ID: </xsl:text>
					<xsl:value-of select="@nProductId"/>
				</xsl:when>
				<xsl:when test="@cProductType!=''">
					<xsl:text>Product Type: </xsl:text>
					<xsl:value-of select="@cProductType"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:if test="bsplit=1">
						<xsl:text>By Type</xsl:text>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>
			<br/>
			<xsl:text>Currency: </xsl:text>
			<xsl:choose>
				<xsl:when test="@cCurrencySymbol!=''">
					<xsl:value-of select="@cCurrencySymbol"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>All/None</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
			<br/>
			<xsl:text>Status: </xsl:text>
			<xsl:value-of select="@nOrderStatus1"/>
			<xsl:text>, </xsl:text>
			<xsl:value-of select="@nOrderStatus2"/>
		</div>
	</xsl:template>

	<xsl:template match="Report[@cGrouping='Page']" mode="reportHeader">
		<div class="panel-heading">
			<h3 class="title">
				Sales by Page
			</h3>
		</div>

	</xsl:template>

	<!-- REPORT BODYS -->

	<xsl:template match="Report" mode="reportBody">
		<table class="table">
			<xsl:for-each select="Item">
				<xsl:if test="position()=1">
					<tr>
						<xsl:for-each select="*">
							<xsl:if test="local-name()!='cCartXml' and local-name()!='Currency_Symbol'">
								<th>
									<xsl:value-of select="translate(local-name(),'_',' ')"/>
								</th>
							</xsl:if>
						</xsl:for-each>
					</tr>
				</xsl:if>
				<span class="advancedModeRow" onmouseover="this.className='rowOver'" onmouseout="this.className='advancedModeRow'">
					<tr>
						<xsl:for-each select="*">
							<xsl:if test="local-name()!='cCartXml' and local-name()!='Currency_Symbol' and local-name()!='Total_Cost'">
								<td>
									<xsl:value-of select="node()"/>
								</td>
							</xsl:if>
							<xsl:if test="local-name()='Total_Cost'">
								<td>
									<xsl:call-template name="formatPrice">
										<xsl:with-param name="price" select="node()"/>
										<xsl:with-param name="currency" select="following-sibling::Currency_Symbol/node()"/>
									</xsl:call-template>
								</td>
							</xsl:if>
							<!--<xsl:if test="local-name()='cCartXml'">
              <td>
                <xsl:apply-templates select="." mode="WELLARDSOVERRIDE"/>
              </td>
            </xsl:if>-->
						</xsl:for-each>
					</tr>
				</span>
			</xsl:for-each>
		</table>
	</xsl:template>

	<xsl:template match="Report[Item/cCartXml]" mode="reportBody">
		<xsl:variable name="ItemId" select="@nProductId"/>

		<ul>
			<li>
				<a href="#" onclick="showAndHide('report_Orders','open_report','closed_report');return false;">Orders</a>
			</li>
			<li>
				<a href="#" onclick="showAndHide('report_Attendees','open_report','closed_report');return false;">Attendees</a>
			</li>
		</ul>

		<div id="report_Orders" class="open_report">
			<h3>
				<xsl:value-of select="@cOrderType"/>
				<xsl:text>s</xsl:text>
			</h3>
			<table class="table">
				<xsl:for-each select="Item">
					<xsl:if test="position()=1">
						<tr>
							<xsl:for-each select="*">
								<xsl:if test="local-name()!='cCartXml' and local-name()!='Currency_Symbol'">
									<th>
										<xsl:value-of select="translate(local-name(),'_',' ')"/>
									</th>
								</xsl:if>
							</xsl:for-each>
							<th>&#160;</th>
						</tr>
					</xsl:if>
					<span class="advancedModeRow" onmouseover="this.className='rowOver'" onmouseout="this.className='advancedModeRow'">
						<tr>
							<xsl:for-each select="*">
								<xsl:if test="local-name()!='cCartXml' and local-name()!='Currency_Symbol' and local-name()!='Total_Cost'">
									<td>
										<xsl:value-of select="node()"/>
									</td>
								</xsl:if>
								<xsl:if test="local-name()='Total_Cost'">
									<td>
										<xsl:call-template name="formatPrice">
											<xsl:with-param name="price" select="node()"/>
											<xsl:with-param name="currency" select="following-sibling::Currency_Symbol/node()"/>
										</xsl:call-template>
									</td>
								</xsl:if>
							</xsl:for-each>
							<td align="right">
								<a href="{$appPath}?ewCmd=Orders&amp;ewCmd2=Display&amp;id={Order_Id/node()}" class="btn btn-default">view order</a>
							</td>
						</tr>
					</span>
					<xsl:if test="position()=last()">
						<tr>
							<xsl:for-each select="*">
								<xsl:choose>
									<xsl:when test="local-name()='Quantity'">
										<th>
											Total:<br/><xsl:value-of select="sum(ancestor::Report/Item/Quantity)"/>
										</th>
									</xsl:when>
									<xsl:when test="local-name()='Total_Cost'">
										<th>
											Total:<br/><xsl:value-of select="sum(ancestor::Report/Item/Total_Cost)"/>
										</th>
									</xsl:when>
									<xsl:otherwise>
										<th>
											&#160;
										</th>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:for-each>
							<th>&#160;</th>
						</tr>
					</xsl:if>
				</xsl:for-each>
			</table>
		</div>
		<div id="report_Attendees" class="closed_report">
			<h3>Attendees</h3>
			<table summary="Attendees List" class="adminList">
				<tr>
					<th>
						<xsl:value-of select="@cOrderType"/>
						<xsl:text> #</xsl:text>
					</th>
					<th>Name</th>
					<th>Job Title</th>
					<th>Company</th>
					<th>Address</th>
					<th>Telephone</th>
					<th>Email</th>
					<th>&#160;</th>
				</tr>
				<xsl:for-each select="Item/cCartXml/Order/Notes/ForumTicket[@contentId=$ItemId]/Attendee">
					<xsl:sort data-type="text" select="Name/node()" order="ascending"/>
					<xsl:sort data-type="text" select="ancestor::Item/Order_Id/node()" order="ascending"/>
					<xsl:apply-templates select="." mode="List_Attendees"/>
				</xsl:for-each>
			</table>
		</div>
	</xsl:template>

	<xsl:template match="Report[@cGrouping='Page']" mode="reportBody">
		<ul id="MenuTree">
			<xsl:apply-templates select="MenuItem" mode="cartActivityPageStructure">
				<xsl:with-param name="level">1</xsl:with-param>
			</xsl:apply-templates>
		</ul>


		<table class="adminList">
			<xsl:apply-templates select="MenuItem" mode="CartReportDrilldownList">
				<xsl:with-param name="HEADERON">1</xsl:with-param>
			</xsl:apply-templates>
		</table>


		<!--<xsl:apply-templates select="MenuItem" mode="CartReportDrilldowntree">
      <xsl:with-param name="HEADERON">1</xsl:with-param>
    </xsl:apply-templates>-->
	</xsl:template>

	<xsl:template match="MenuItem" mode="cartActivityPageStructure">
		<xsl:param name="level"/>
		<li class="treeNode" style="display:block;">
			<xsl:apply-templates select="." mode="status_legend"/>
			<xsl:variable name="displayName">
				<xsl:apply-templates select="." mode="getDisplayName" />
			</xsl:variable>
			<a href="{$appPath}?ewCmd=Normal&amp;pgid={@id}" title="{@name}" class="pageName" name="{@id}">
				<xsl:value-of select="$displayName"/>
			</a>
			<xsl:text> - </xsl:text>
			<span class="pageSales">
				<xsl:text>Page Sales (</xsl:text>
				<xsl:value-of select="@PageQuantity"/>
				<xsl:if test="@PageQuantity &gt; 0">
					<xsl:text>&#160;</xsl:text>
					<xsl:call-template name="formatPrice">
						<xsl:with-param name="price" select="@PageCost"/>
						<xsl:with-param name="currency" select="ancestor-or-self::Report/@cCurrencySymbol"/>
					</xsl:call-template>
				</xsl:if>
				<xsl:text>)</xsl:text>
			</span>
			<xsl:if test="@PageCost &lt; @PageAndDescendantCost">
				<xsl:text> - </xsl:text>
				<span class="childPageSales">
					<xsl:text>Child Page Sales (</xsl:text>
					<xsl:value-of select="@PageAndDescendantQuantity"/>
					<xsl:if test="@PageAndDescendantQuantity &gt; 0">
						<xsl:text>&#160;</xsl:text>
						<xsl:call-template name="formatPrice">
							<xsl:with-param name="price" select="@PageAndDescendantCost"/>
							<xsl:with-param name="currency" select="ancestor-or-self::Report/@cCurrencySymbol"/>
						</xsl:call-template>
					</xsl:if>
					<xsl:text>)</xsl:text>
				</span>
			</xsl:if>
			<xsl:if test="MenuItem[@PageCost &gt; 0 or @PageAndDescendantCost &gt; 0]">
				<ul>
					<xsl:apply-templates select="MenuItem[@PageCost &gt; 0 or @PageAndDescendantCost &gt; 0]" mode="cartActivityPageStructure">
						<xsl:with-param name="level">
							<xsl:value-of select="$level + 1"/>
						</xsl:with-param>
					</xsl:apply-templates>
				</ul>
			</xsl:if>
		</li>
	</xsl:template>

	<!-- List Attendee's -->

	<xsl:template match="Attendee" mode="List_Attendees">
		<span class="advancedModeRow" onmouseover="this.className='rowOver'" onmouseout="this.className='advancedModeRow'">
			<tr>
				<td>
					<xsl:value-of select="ancestor::Item/Order_Id/node()"/>
				</td>
				<td>
					<xsl:value-of select="Name/node()"/>
				</td>
				<td>
					<xsl:value-of select="JobTitle/node()"/>
				</td>
				<td>
					<xsl:value-of select="Company/node()"/>
				</td>
				<td>
					<xsl:value-of select="Address/node()"/>
				</td>
				<td>
					<xsl:value-of select="Telephone/node()"/>
				</td>
				<td>
					<a href="mailto:{Email/node()}" title="email {Name/node()}">
						<xsl:value-of select="Email/node()"/>
					</a>
				</td>
				<td align="right">
					<a href="{$appPath}?ewCmd=Orders&amp;ewCmd2=Display&amp;id={ancestor::Item/Order_Id/node()}" class="view adminButton">view order</a>
				</td>
			</tr>
		</span>
	</xsl:template>

	<!--      Cart Activity Drilldown     -->

	<xsl:template match="MenuItem" mode="CartReportDrilldownList">
		<xsl:param name="HEADERON"/>
		<xsl:if test="$HEADERON=1">
			<tr>
				<th>
					Page
				</th>
				<th>
					Page Quantity
				</th>
				<th>
					Page Cost
				</th>
				<th>
					Page And Childs Quantity
				</th>
				<th>
					Page And Childs Cost
				</th>
			</tr>
		</xsl:if>
		<xsl:if test="@PageAndDescendantQuantity &gt; 0">
			<tr>
				<td>
					<xsl:value-of select="@name"/>
				</td>
				<td>
					<xsl:value-of select="@PageQuantity"/>
				</td>
				<td>
					<xsl:value-of select="@PageCost"/>
				</td>
				<td>
					<xsl:value-of select="@PageAndDescendantQuantity - @PageQuantity"/>
				</td>
				<td>
					<xsl:value-of select="@PageAndDescendantCost - @PageCost"/>
				</td>
			</tr>
			<xsl:apply-templates select="MenuItem" mode="CartReportDrilldownList"/>
		</xsl:if>
	</xsl:template>

	<xsl:template match="MenuItem" mode="CartReportDrilldowntree">
		<xsl:param name="HEADERON"/>
		<xsl:if test="@PageAndDescendantQuantity > 0">
			<ul>
				<li>
					<table border="1">
						<xsl:if test="$HEADERON=1">
							<tr>
								<th>
									Page
								</th>
								<th>
									Page Quantity
								</th>
								<th>
									Page Cost
								</th>
								<th>
									Page And Childs Quantity
								</th>
								<th>
									Page And Childs Cost
								</th>
							</tr>
						</xsl:if>
						<tr>
							<td>
								<xsl:value-of select="@name"/>
							</td>
							<td>
								<xsl:value-of select="@PageQuantity"/>
							</td>
							<td>
								<xsl:value-of select="@PageCost"/>
							</td>
							<td>
								<xsl:value-of select="@PageAndDescendantQuantity - @PageQuantity"/>
							</td>
							<td>
								<xsl:value-of select="@PageAndDescendantCost - @PageCost"/>
							</td>
							<td>
								<xsl:apply-templates select="MenuItem" mode="CartReportDrilldowntree"/>
							</td>
						</tr>
					</table>
				</li>
			</ul>
		</xsl:if>
	</xsl:template>


	<xsl:template match="Page[@layout='EventBookings']" mode="Admin">

		<div class="row" id="tpltCartActivity">
			<div class="col-md-3">
				<div class="list-group">
					<xsl:apply-templates select="ContentDetail/Events/Event[Ticket]" mode="eventList"/>
					<xsl:choose>
						<xsl:when test="$page/@ewCmd2='pastbookings'">
							<a class="btn btn-default  btn-block" href="?ewCmd=EventBookings">Future Events</a>
						</xsl:when>
						<xsl:otherwise>
							<a class="btn btn-default  btn-block" href="?ewCmd=EventBookings.pastbookings">Past Events</a>
						</xsl:otherwise>
					</xsl:choose>

				</div>
			</div>
			<div class="col-md-9">
				<table class="table table-responsive">
					<tr xmlns="http://www.w3.org/1999/xhtml">
						<th>
							OrderId
						</th>
						<th>
							Order Date
						</th>
						<th>
							Ticket Type
						</th>
						<th>
							Name
						</th>
						<th>
							Email
						</th>
						<th>
							Quantity
						</th>
						<th>
							Price
						</th>
						<th>
							Amt Recieved
						</th>
						<th>
							Last Payment
						</th>
						<th>
							Outstanding
						</th>
						<th>
							<!--
				<a href="" class="btn btn-success">Download in Excel</a>
				-->
							<a href="/ewcommon/tools/excel.ashx?{/Page/Request/ServerVariables/Item[@name='QUERY_STRING']/node()}" class="btn btn-sm btn-primary" target="_new">
								<i class="fa fa-th-list fa-white">
									<xsl:text> </xsl:text>
								</i>
								<xsl:text> </xsl:text>
								Excel Download
							</a>
						</th>
					</tr>
					<xsl:apply-templates select="ContentDetail/Report/Tickets/Ticket" mode="TicketReport"/>
				</table>
			</div>
		</div>
	</xsl:template>
	<xsl:template match="Ticket" mode="TicketReport">
		<tr xmlns="http://www.w3.org/1999/xhtml">
			<td>
				<xsl:value-of select="OrderId/node()"/>
			</td>
			<td class="text-nowrap">
				<xsl:call-template name="DD_Mon_YYYY">
					<xsl:with-param name="date">
						<xsl:value-of select="dInsertDate/node()"/>
					</xsl:with-param>
					<xsl:with-param name="showTime">true</xsl:with-param>
				</xsl:call-template>
			</td>
			<td>
				<xsl:value-of select="TicketName/node()"/>
			</td>
			<td>
				<xsl:value-of select="CustomerName/node()"/>
			</td>
			<td>
				<xsl:value-of select="Email/node()"/>
			</td>
			<td>
				<xsl:value-of select="Quantity/node()"/>
			</td>
			<td>
				<xsl:value-of select="format-number(Price/node(),'0.00')"/>
			</td>
			<td>
				<xsl:value-of select="format-number(nAmountReceived/node(),'0.00')"/>
			</td>
			<td>

				<xsl:value-of select="format-number(nLastPaymentMade/node(),'0.00')"/>
			</td>
			<td>
				<xsl:value-of select="format-number(BalanceDue/node(),'0.00')"/>
			</td>
			<td>
				<a href="?ewCmd=Orders&amp;ewCmd2=Display&amp;id={OrderId/node()}" class="btn btn-xs btn-primary">
					<i class="fa fa-eye"> </i> view order
				</a>
				<xsl:if test="BalanceDue &gt; 0">
					<a href="?ewCmd=Orders&amp;ewCmd2=RequestSettlement&amp;id={OrderId/node()}" target="_new" class="btn btn-xs btn-warning">
						<i class="fa fa-envelope"> </i> Send Settlement Request
					</a>
				</xsl:if>
			</td>
		</tr>

	</xsl:template>
	<xsl:template match="Event" mode="eventList">
		<a class="btn btn-default btn-block" href="?ewCmd=EventBookings.{$page/@ewCmd2}&amp;EventId={@nContentKey}">
			<xsl:attribute name="class">
				<xsl:choose>
					<xsl:when test="@nContentKey=/Page/ContentDetail/Tickets/@EventId">
						btn btn-primary btn-block
					</xsl:when>
					<xsl:otherwise>
						btn btn-default btn-block
					</xsl:otherwise>
				</xsl:choose>
			</xsl:attribute>
			<small>
				<xsl:value-of select="cContentXmlBrief/Content/StartDate/node()"/>
				<br/>
				<xsl:value-of select="cContentXmlBrief/Content/Headline/node()"/>
			</small>
			<br/>

			<xsl:for-each select="Ticket">
				<small>
					<xsl:value-of select="TicketName/node()"/> - <xsl:value-of select="TotalTicketsSold/node()"/>
				</small>
				<br/>
			</xsl:for-each>
		</a>

	</xsl:template>

	<xsl:template match="Report[@cGrouping='Group']" mode="CartReportDrilldown">
		<table border="1">
			<tr>
				<th>Group Name</th>
				<th>Quantity</th>
				<th>Cost</th>
			</tr>
			<xsl:for-each select="Item">
				<xsl:sort select="nCatKey" order="ascending"/>
				<xsl:variable name="CatKey" select="nCatKey"/>
				<xsl:if test="position()=1 or preceding-sibling::Item/nCatKey!=$CatKey">
					<tr>
						<td>
							<xsl:choose>
								<xsl:when test="$CatKey > 0">
									<xsl:value-of select="cCatName"/>
								</xsl:when>
								<xsl:otherwise>
									Not Grouped
								</xsl:otherwise>
							</xsl:choose>
						</td>
						<td>
							<xsl:value-of select="sum(ancestor-or-self::Report/Item/nQuantity)"/>
						</td>
						<td>
							<xsl:value-of select="sum(ancestor-or-self::Report/Item/nLinePrice)"/>
						</td>
					</tr>
				</xsl:if>
			</xsl:for-each>
		</table>
	</xsl:template>


	<!--      Cart Activity Period     -->


	<xsl:template match="Report" mode="CartReportPeriod">
		<xsl:variable name="minGroup">
			<xsl:value-of select="Item[1]/GroupBy"/>
		</xsl:variable>
		<xsl:variable name="maxGroup">
			<xsl:variable name="nCount">
				<xsl:value-of select="count(Item)"/>
			</xsl:variable>
			<xsl:for-each select="Item">
				<xsl:if test="position()=$nCount">
					<xsl:value-of select="GroupBy"/>
				</xsl:if>
			</xsl:for-each>
		</xsl:variable>
		<xsl:variable name="groupName" select="@cGroup" />
		<xsl:variable name="nDif" select=" $maxGroup - $minGroup +1" />

		<table border="1">
			<tr>
				<td rowspan="2">&#160;</td>
				<xsl:call-template name="GroupCells">
					<xsl:with-param name="startNum" select="$minGroup"/>
					<xsl:with-param name="endNum" select="$maxGroup"/>
				</xsl:call-template>
			</tr>
			<tr>
				<xsl:call-template name="HeaderCells">
					<xsl:with-param name="startNum" select="$minGroup"/>
					<xsl:with-param name="endNum" select="$maxGroup"/>
				</xsl:call-template>
			</tr>
			<tr>
				<xsl:for-each select="Item/cStructName">
					<xsl:sort select="node()"/>
					<xsl:variable name="StructName" select="node()"/>
					<xsl:if test="position()=1 or node()!=$StructName">
						<td>
							<xsl:value-of select="$StructName"/>
						</td>
					</xsl:if>
				</xsl:for-each>
				<xsl:for-each select="Item/cStructName">
					<xsl:sort select="node()"/>
					<xsl:variable name="StructName" select="node()"/>
					<xsl:if test="position()=1 or node()!=$StructName">
						<xsl:call-template name="valueCells">
							<xsl:with-param name="startNum" select="$minGroup"/>
							<xsl:with-param name="endNum" select="$maxGroup"/>
							<xsl:with-param name="structName" select="$StructName"/>
						</xsl:call-template>
					</xsl:if>
				</xsl:for-each>
			</tr>
		</table>
	</xsl:template>

	<xsl:template name="GroupCells">
		<xsl:param name="startNum"/>
		<xsl:param name="endNum"/>
		<xsl:param name="cellName"/>
		<td colspan="2">
			<xsl:value-of select="$cellName"/>
			<xsl:text> </xsl:text>
			<xsl:value-of select="$startNum"/>
		</td>
		<xsl:if test="$endNum > $startNum">
			<xsl:call-template name="GroupCells">
				<xsl:with-param name="startNum" select="$startNum+1"/>
				<xsl:with-param name="endNum" select="$endNum"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="HeaderCells">
		<xsl:param name="startNum"/>
		<xsl:param name="endNum"/>
		<td>Q</td>
		<td>V</td>
		<xsl:if test="$endNum > $startNum">
			<xsl:call-template name="HeaderCells">
				<xsl:with-param name="startNum" select="$startNum+1"/>
				<xsl:with-param name="endNum" select="$endNum"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="valueCells">
		<xsl:param name="startNum"/>
		<xsl:param name="endNum"/>
		<xsl:param name="structName"/>
		<xsl:variable name="Q" select="/Page/ContentDetail/Content/Report/Item[cStructName=$structName and GroupBy=$startNum]/Quantity"/>
		<xsl:variable name="V" select="/Page/ContentDetail/Content/Report/Item[cStructName=$structName and GroupBy=$startNum]/Price"/>
		<td>
			<xsl:value-of select="$Q"/>
		</td>
		<td>
			<xsl:value-of select="$V * $Q"/>
		</td>
		<xsl:if test="$endNum > $startNum">
			<xsl:call-template name="valueCells">
				<xsl:with-param name="startNum" select="$startNum+1"/>
				<xsl:with-param name="endNum" select="$endNum"/>
				<xsl:with-param name="structName" select="$structName"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template match="Content" mode="getSynopsis">
		<xsl:value-of select="@moduleType"/>
		<xsl:text> - </xsl:text>
		<xsl:variable name="normBody">
			<xsl:value-of select="normalize-space(node())"/>
		</xsl:variable>
		<xsl:call-template name="truncate-string">
			<xsl:with-param name="text" select="$normBody"/>
			<xsl:with-param name="length" select="'200'"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="Page[@layout='WebStats']" mode="Admin">
		<xsl:variable name="statsId" select="ew:EonicConfigValue('web','StatsID')"/>

		<xsl:choose>
			<xsl:when test="$statsId!=''">
				<script LANGUAGE="JavaScript">
					function GoToStats() {
					document.statsform.submit();
					};
				</script>
				<form name="statsform" action="http://stats.eonichost.co.uk/Login.aspx" method="post">
					<input type="hidden" name="shortcutLink" value="autologin" id="shortcutLink"/>
					<input type="hidden" name="txtSiteID" id="txtSiteID" value="{ew:EonicConfigValue('web','StatsID')}"/>
					<input type="hidden" name="txtUser" id="txtUser" value="{ew:EonicConfigValue('web','StatsUser')}"/>
					<input type="hidden" name="txtPass" id="txtPass" value="{ew:EonicConfigValue('web','StatsPass')}"/>
				</form>
				<p>
					<a href="JavaScript:GoToStats()">Log into your stats</a>
				</p>
				<!--iframe id="pluginIframe" name="pluginIframe" width="100%" height="1000">
                <p>Your browser does not support iframes.</p>
          </iframe-->
			</xsl:when>
			<xsl:otherwise>
				<div class="report" id="template_1_Column">
					<h2>Enter your Stats logon into Web Settings</h2>
				</div>
			</xsl:otherwise>
		</xsl:choose>

	</xsl:template>

	<xsl:template match="Page[@layout='WebStats']" mode="LayoutAdminJs">
		<xsl:variable name="statsId" select="ew:EonicConfigValue('web','StatsID')"/>

		<xsl:choose>
			<xsl:when test="$statsId!=''">
				<script LANGUAGE="JavaScript">
					function GoToStats() {
					document.statsform.submit();
					};
				</script>
			</xsl:when>
			<xsl:otherwise>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>


	<xsl:template match="Page[@adminMode='true' and @layout='GoogleAnalytics']" mode="bodyBuilder">
		<body id="pg_{@id}" class="ewAdmin">
			<xsl:apply-templates select="AdminMenu"/>
			<div class="row" id="tpltCartActivity">
				<div class="col-md-4">&#160; </div>
				<div class="col-md-4">
					<br/>
					<br/>
					<a class="btn btn-lg btn-primary" href="https://analytics.google.com/analytics/web/">Open to Google Analytics</a>

					<br/>
					<br/>
				</div>
				<div class="col-md-4">&#160; </div>
			</div>
			<!--iframe class="pluginIframe" src="https://analytics.google.com/analytics/web/" width="100%" height="1000">
        <p>Your browser does not support iframes.</p>
      </iframe-->
			<xsl:apply-templates select="." mode="adminFooter"/>
			<iframe id="keepalive" src="/ewCommon/tools/keepalive.ashx" frameborder="0" width="0" height="0" xmlns:ew="urn:ew">Keep Alive frame</iframe>
		</body>
	</xsl:template>


	<xsl:template match="Page[@adminMode='true' and @layout='RavenFrameset']" mode="bodyBuilder">
		<body id="pg_{@id}" class="ewAdmin">
			<xsl:apply-templates select="AdminMenu"/>
			<iframe class="pluginIframe" src="https://eonic.raventools.com/tools/z/#ranking/index" width="100%" height="1000">
				<p>Your browser does not support iframes.</p>
			</iframe>
			<xsl:apply-templates select="." mode="adminFooter"/>
			<iframe id="keepalive" src="/ewCommon/tools/keepalive.ashx" frameborder="0" width="0" height="0" xmlns:ew="urn:ew">Keep Alive frame</iframe>
		</body>
	</xsl:template>



	<!--<xsl:template match="Page[@layout='RavenFrameset']" mode="Admin">
    <xsl:variable name="ravenUser" select="ew:EonicConfigValue('web','RavenUsername')"/>

    <xsl:choose>
      <xsl:when test="$ravenUser!=''">
        <script LANGUAGE="JavaScript">
          function GoToStats() {
          document.statsform.submit();
          };
        </script>
        <form name="statsform" action="https://eonic.raventools.com/tools/m/login/" method="post">
          <input type="hidden" name="ref" id="ref" value=""/>
          <input type="hidden" name="username" id="username" value="{ew:EonicConfigValue('web','RavenUsername')}"/>
          <input type="hidden" name="password" id="password" value="{ew:EonicConfigValue('web','RavenPassword')}"/>
        </form>
        <p>
          <a href="JavaScript:GoToStats()">Log into your stats</a>
        </p>
        -->
	<!--iframe id="pluginIframe" name="pluginIframe" width="100%" height="1000">
                <p>Your browser does not support iframes.</p>
          </iframe-->
	<!--
      </xsl:when>
      <xsl:otherwise>
        <div class="report" id="template_1_Column">
          <h2>Enter your Stats logon into Web Settings</h2>
        </div>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>-->

	<xsl:template match="Page[@adminMode='true' and @layout='MailingList' and @ewCmd2='CMSession']" mode="bodyBuilder">
		<body id="pg_{@id}" class="ewAdmin">
			<xsl:apply-templates select="AdminMenu"/>
			<iframe class="pluginIframe" src="{ContentDetail/SessionPage/@CMUrl}" width="100%" height="1000">
				<p>Your browser does not support iframes.</p>
			</iframe>
			<xsl:apply-templates select="." mode="adminFooter"/>
			<iframe id="keepalive" src="/ewCommon/tools/keepalive.ashx" frameborder="0" width="0" height="0" xmlns:ew="urn:ew">Keep Alive frame</iframe>

			<xsl:apply-templates select="." mode="footerJs"/>
		</body>
	</xsl:template>


	<xsl:template match="Page[@layout='InstallTheme']" mode="Admin">
		<div class="rows">
			<xsl:if test="ContentDetail/@errorMsg!=''">
				<div class="alert alert-danger">
					<i class="fa fa-exclamation-circle fa-2x pull-left"> </i>
					<xsl:value-of select="ContentDetail/@errorMsg"/>
				</div>
			</xsl:if>
			<form action="" method="post">
				<xsl:apply-templates select="ContentDetail/Content[@type='EwTheme']" mode="ThemeInstall"/>
			</form>
		</div>
	</xsl:template>

	<xsl:template match="Content" mode="ThemeInstall">

		<div class="col-md-4">
			<div class="panel panel-default">
				<div class="panel-heading">
					<h3 class="title">
						<xsl:value-of select="ItemName"/>
					</h3>
				</div>
				<div class="panel-body">
					<xsl:choose>
						<xsl:when test="Images/img[@class='detail']/@src!=''">
							<a href="http://www.eonicweb.com/{Images/img[@class='detail']/@src}" class="responsive-lightbox">
								<img src="http://www.eonicweb.com/{Images/img[@class='detail']/@src}" class="img-responsive"/>
							</a>
						</xsl:when>
						<xsl:otherwise>
							<img src="/ewcommon/images/pagelayouts/webtheme.png" class="pull-left"/>
						</xsl:otherwise>
					</xsl:choose>

					<xsl:apply-templates select="Summary" mode="cleanXhtml"/>
				</div>
				<div class="panel-footer">
					<button class="btn btn-success pull-right" name="themeName" value="{ItemName/node()}">
						<i class="fa fa-arrow-down">&#160;</i>&#160;Install Theme
					</button>&#160;<br/>&#160;
				</div>
			</div>
		</div>
	</xsl:template>

	<xsl:template match="Page[@layout='RewriteRules']" mode="Admin">
		<table class="table table-striped">
			<xsl:for-each select="ContentDetail/RewriteRules/rules/rule">
				<tr>
					<td>
						<i class="status" title="none">
							<xsl:choose>
								<xsl:when test="@enabled='false'">
									<xsl:attribute name="class">
										<xsl:text>status fa fa-times-circle fa-lg text-danger</xsl:text>
										<xsl:if test="MenuItem">Parent</xsl:if>
									</xsl:attribute>
									<xsl:attribute name="title">This rule is disabled</xsl:attribute>
								</xsl:when>
								<xsl:otherwise>
									<xsl:attribute name="class">
										<xsl:text>status fa fa-check fa-lg text-success</xsl:text>
										<xsl:if test="MenuItem">Parent</xsl:if>
									</xsl:attribute>
									<xsl:attribute name="title">This rule is live</xsl:attribute>
								</xsl:otherwise>
							</xsl:choose>
							<xsl:text> </xsl:text>
						</i>
					</td>
					<td>
						<strong>
							<xsl:value-of select="@name"/>
						</strong>
					</td>
					<td>
						<xsl:value-of select="@matchDefault"/>
					</td>
					<td>
						<!--
          <button class="btn btn-success pull-right" name="themeName" value="{ItemName/node()}">
            <i class="fa fa-pencil">&#160;</i>&#160;Edit Rule
          </button>
          -->
					</td>
				</tr>
			</xsl:for-each>
		</table>
	</xsl:template>

	<xsl:template match="Email" mode="reportCell"/>

	<xsl:template match="Mark[number(.)=number(.)]" mode="reportCell">
		<td>
			<xsl:value-of select="./node()"/>%
		</td>
	</xsl:template>

	<xsl:template match="Status | nStatus | status" mode="reportCell">
		<td>
			<xsl:call-template name="StatusLegend">
				<xsl:with-param name="status">
					<xsl:value-of select="node()"/>
				</xsl:with-param>
			</xsl:call-template>
		</td>
	</xsl:template>

	<xsl:template match="Username | Full_Name[not(parent::*//Username)]" mode="reportCell">
		<td>
			<a href="mailto:{parent::*//Email/node()}">
				<xsl:value-of select="node()"/>
			</a>
		</td>
	</xsl:template>

	<xsl:template match="Grade" mode="reportCell">
		<td>
			<xsl:attribute name="class">nowrap</xsl:attribute>
			<xsl:value-of select="./node()"/>
		</td>
	</xsl:template>

	<xsl:template match="Company[not(parent::attempt)]" mode="reportCell">
		<td>
			<a href="http://:{Website/node()}">
				<xsl:value-of select="Website/node()"/>
			</a>
		</td>
	</xsl:template>

	<xsl:template match="Website" mode="reportCell">
		<td>
			<a href="http://{node()}" target="_new">
				<xsl:value-of select="node()"/>
			</a>
		</td>
	</xsl:template>

	<xsl:template match="Details" mode="reportCell">
		<td>
			<a href="http://:{./Company/Website/node()}">
				<xsl:value-of select="./Company/Website/node()"/>
			</a>
		</td>
	</xsl:template>

	<xsl:template match="User" mode="reportCell">
		<td>
			<a href="/{$appPath}?ewCmd=Profile&amp;DirType=User&amp;id={ancestor::user/@id}">
				<span class="btn btn-primary btn-xs">
					<i class="fa fa-user fa-white">
						<xsl:text> </xsl:text>
					</i>
				</span>
				&#160;<xsl:choose>
					<xsl:when test="FirstName and LastName">
						<xsl:value-of select="LastName"/>, <xsl:value-of select="FirstName"/>
					</xsl:when>
					<xsl:when test="User/FirstName and User/LastName">
						<xsl:value-of select="User/LastName"/>, <xsl:value-of select="User/FirstName"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="node()"/>
					</xsl:otherwise>
				</xsl:choose>
			</a>
		</td>
	</xsl:template>


	<xsl:template match="Name[ancestor::company]" mode="reportCell">
		<td>
			<a href="/{$appPath}?ewCmd=Profile&amp;DirType=Company&amp;id={ancestor::company/@id}">
				<span class="btn btn-primary btn-xs">
					<i class="fa fa-building fa-white">
						<xsl:text> </xsl:text>
					</i>
				</span>
				&#160;
				<xsl:value-of select="node()"/>
			</a>
		</td>
	</xsl:template>

	<xsl:template match="UserXml" mode="reportCell">
		<td>
			<xsl:choose>
				<xsl:when test="User/type/node()='review'">
					<xsl:value-of select="User/FirstName/node()"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:if test="User/LastName/node()!='' or User/FirstName/node()=''">
						<a href="mailto:{User/Email/node()}">
							<xsl:value-of select="User/LastName/node()"/>,&#160;<xsl:value-of select="User/FirstName/node()"/>
						</a>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>
		</td>
	</xsl:template>

	<xsl:template match="GroupXml" mode="reportCell">
		<td>
			<xsl:value-of select="Group/Name/node()"/>
		</td>
	</xsl:template>

	<xsl:template name="StatusLegend">
		<xsl:param name="status"/>
		<xsl:choose>
			<xsl:when test="$status='0'">
				<a href="#" data-toggle="tooltip" data-placement="right" title="Hidden" data-original-title="Hidden">
					<i class="fa fa-times text-danger" alt="inactive">&#160;</i>
				</a>
			</xsl:when>
			<xsl:when test="$status='-1'">
				<a href="#" data-toggle="tooltip" data-placement="right" title="Live" data-original-title="Live">
					<i class="fa fa-check text-success" alt="live">&#160;</i>
				</a>
			</xsl:when>
			<xsl:when test="$status='1'">
				<a href="#" data-toggle="tooltip" data-placement="right" title="Live" data-original-title="Live">
					<i class="fa fa-check text-success" alt="live">&#160;</i>
				</a>
			</xsl:when>
			<xsl:when test="$status='2'">
				<a href="#" data-toggle="tooltip" data-placement="right" title="Superceeded" data-original-title="Superceeded">
					<i class="fas fa-history text-default" alt="Superceeded">&#160;</i>
				</a>
			</xsl:when>
			<xsl:when test="$status='3'">
				<a href="#" data-toggle="tooltip" data-placement="right" title="Pending" data-original-title="Pending">
					<i class="far fa-pause-circle" alt="live">&#160;</i>
				</a>
			</xsl:when>
			<xsl:when test="$status='4'">
				<a href="#" data-toggle="tooltip" data-placement="right" title="Pending" data-original-title="Preview">
					<i class="far fa-pause-circle" alt="Preview">&#160;</i>
				</a>
			</xsl:when>
			<xsl:when test="$status='7'">
				<a href="#" data-toggle="tooltip" data-placement="right" title="Expired" data-original-title="Expired">
					<i class="fa fa-clock-o text-danger">&#160;</i>
				</a>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$status"/>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<!-- Version Control reportCell matches-->
	<xsl:template match="Version" mode="reportCell">
		<td>
			<xsl:value-of select="."/>
			<xsl:if test="parent::node()/@currentLiveVersion!=''">
				<xsl:text> [</xsl:text>
				<xsl:value-of select="parent::node()/@currentLiveVersion"/>
				<xsl:text>]</xsl:text>
			</xsl:if>
		</td>
	</xsl:template>
	<!-- -->

	<xsl:template match="input[@class='RedirectPage']" mode="xform" >

		<div class="row">

			<div class="col-md-4">
				<div class="input-group col-md-4">
					<span class="input-group-btn">
						<button type="button"  value="Clear" class="btn btn-default btnClear">
							<i class="fa fa-times"/>
						</button>
					</span>
					<input type="text" name="SearchURL" id="SearchURLText" class="form-control" />
					<input type="hidden"  id="totalUrlCount" class="form-control" />
					<span class="input-group-btn">
						<button type="button"  value="Search" class="btn btn-primary btnSearchUrl">Search </button>
					</span>
				</div>
				&#160;   &#160;   &#160;
			</div>

			<div class="col-md-4">
				<lable class="countLable hidden"></lable>
				&#160;   &#160;   &#160;
				<!--<lable class="endLable hidden">You reached at end</lable>-->
			</div>
		</div>


		<div class="control-wrapper RedirectPage" id="RedirectPage">
			<div id="loadSpin" class="loadSpin modal " tabindex="-1" >
				<div class="modal-dialog">
					<div class="modal-content">
						<!--<div class="modal-body">-->
						<lable class="modalLable hidden"></lable>
						<!--<div id="redirectLoad" v-if="loading" class="vueloadimg" v-show="true" >
              <i class="fas fa-spinner fa-spin"> </i>
            </div>-->


						<!--</div>-->
					</div>
				</div>
			</div>
			<div class="form-group">
				<div class="form-group input-containing col-md-6">
					<label >Old URL</label>
				</div>
				<div class="form-group input-containing col-md-6">
					<label  >New URL</label>
				</div>

			</div>
			<div id="addNewUrl" class="form-group  repeat-group newAddFormInline">
				<fieldset class="rpt-00 row">
					<div class="form-group input-containing col-md-5">

						<div class="control-wrapper input-wrapper appearance-">

							<input type="text" name="OldUrlform" id="OldUrlmodal" class="textbox form-control"/>
						</div>
					</div>
					<div class="form-group input-containing col-md-5">

						<div class="control-wrapper input-wrapper appearance-">
							<input type="text" name="NewUrlform" id="NewUrlModal" class="textbox form-control"/>
						</div>
					</div>
					<div class="form-group input-containing col-md-2">

						<div class="control-wrapper input-wrapper appearance-">
							<button type="button"  class="btn btn-primary addRedirectbtn">
								Add new Url
							</button>
						</div>
					</div>

				</fieldset>
			</div>
			<div>
				<div class="form-group repeat-group ListOfNewAddedUrls"  v-for="(urls,index) in newAddedUrlList">
					<fieldset>
						<div class="form-group input-containing col-md-5" >

							<div class="control-wrapper input-wrapper">

								<input type="text" name="OldUrl" v-bind:id="'Old_' + index"  class="form-control addUrlText" v-bind:value="urls.oldUrl"/>
							</div>
						</div>
						<div class="form-group input-containing col-md-5">

							<div class="control-wrapper input-wrapper">
								<input type="text" name="NewUrl" v-bind:id="'New_' + index"  class="form-control addUrlText" v-bind:value="urls.NewUrl"/>
							</div>
						</div>
						<div class="form-group input-containing col-md-1">
							<button type="button"  class="btn btn-primary btn-updateNewUrl hidden" >
								Update
							</button>
							<lable class="tempLableSaveNew hidden">Saved..</lable>
						</div>
						<div class="form-group input-containing col-md-1">

							<div class="control-wrapper input-wrapper">
								<button type="button"  class="btn btn-danger delAddNewUrl">
									<i class="fa fa-times fa-white"> </i> Del
								</button>
							</div>
						</div>

					</fieldset>
				</div>
			</div>
			<div class="scolling-pane">
				<div class="form-group repeat-group parentDivOfRedirect"  v-for="(urls,index) in urlList" >
					<fieldset v-bind:class="'row repeated rpt_'+ index">
						<div class="form-group input-containing col-md-5">
							<div class="control-wrapper input-wrapper appearance-">
								<input type="text"  v-bind:id="'OldUrl_' + index" class="col-md-5 textbox form-control redirecttext" v-bind:value="urls.attributes.key.nodeValue"/>

								<input type="hidden"  class="col-md-5 textbox form-control hiddenOldUrlText" v-bind:value="urls.attributes.key.nodeValue" />
							</div>
						</div>
						<div class="form-group input-containing col-md-5">
							<div class="control-wrapper input-wrapper appearance-">
								<input type="text" v-bind:id="'NewUrl_'+index" class="col-md-5 textbox form-control redirecttext" v-bind:value="urls.attributes.value.nodeValue" />
							</div>
						</div>
						<div class="form-group trigger-group col-md-1">
							<button type="button" value="Del" v-bind:id="'update_' + index" class="btn btn-primary btn-update hidden" >
								Update
							</button>
							<lable class="tempLableSave hidden">Saved..</lable>
						</div>
						<div class="form-group trigger-group col-md-1">
							<button type="button" value="Del" v-bind:id="'del_' + index" class="btn btn-danger btn-delete" >
								<i class="fa fa-times fa-white"> </i> Del
							</button>
						</div>
					</fieldset>
				</div>

			</div>
			<div id="redirectLoad" v-if="loadingscroll" class="vueloadimg" v-show="true" >
				<i class="fas fa-spinner fa-spin"> </i>
			</div>
		</div>

	</xsl:template>

	<!-- code for filter indexes -->

	<xsl:template match="Page[@layout='FilterIndex']" mode="Admin">
		<div class="row" id="template_AdvancedMode">
			<div class="col-md-3">
				<div class="panel panel-default">
					<div class="panel-body">
						Filter Indexes
					</div>
				</div>
			</div>
			<div class="col-md-9">
				<div class="panel panel-default">
					<div class="panel-heading">
						<p class="btn-group headerButtons">
							<xsl:if test="ContentDetail/Content[@type='xform']">
								<a href="{$appPath}?ewCmd=FilterIndex&amp;pgid={/Page/@id}" class="btn btn-default" title="Back to FilterIndexes">
									<i class="fa fa-caret-left">&#160; </i>&#160;Back to FilterIndexex List
								</a>
							</xsl:if>
						</p>
					</div>
					<div class="panel-body">
						<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
					</div>
					<xsl:apply-templates select="ContentDetail/Content[@type!='xform']" mode="ListIndexes"/>
				</div>
			</div>

		</div>
	</xsl:template>

	<xsl:template match="Content[@type='Report']" mode="ListIndexes">
		<!--<div class="panel-body">
			<form method="get" role="form" class="form-inline">
				<input type="hidden" name="ewCmd" value="FilterIndex"/>
				<input type="hidden" name="indexkey" value="0"/>
				

			</form>
		</div>-->
		<div class="table-responsive">
			<table class="table manage-lookups table-hover ">
				<tr>
					<th colspan="2">
						Data Type
					</th>
					<th colspan="2">
						Schema Name
					</th>
					<th colspan="2">
						Defination
					</th>
					<th colspan="2">
						Xpath
					</th>
					<th colspan="2">
						Brief
					</th>
					<th class="clearfix buttonCell">
						<a href="{$appPath}?ewCmd=FilterIndex&amp;pgid={/Page/@id}&amp;id=0&amp;SchemaName={indexkeys/SchemaName/@Name}" class="btn btn-success pull-right">
							<i class="fa fa-plus fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Add New Item
						</a>
						<a href="{$appPath}?ewCmd=FilterIndex&amp;ewCmd2=updateAllRules&amp;pgid={/Page/@id}&amp;id={@nContentIndexDefKey}&amp;SchemaName=null" class="btn btn-primary btn-xs pull-right">
							<i class="fa fa-edit fa-white">
								<xsl:text> </xsl:text>
							</i><xsl:text> </xsl:text>Update All Rules
						</a>
					</th>

				</tr>
				<xsl:apply-templates select="indexkeys" mode="LookupList"/>
			</table>
		</div>
	</xsl:template>

	<!--<xsl:template match="SchemaName" mode="LookupList">
	
		<xsl:apply-templates select="indexkey" mode="LookupList"/>
	</xsl:template>-->

	<xsl:template match="indexkey" mode="LookupList">
		<tr>
			<td colspan="2">
				<xsl:value-of select="nContentIndexDataType/node()"/>
			</td>
			<td colspan="2">
				<xsl:value-of select="cContentSchemaName/node()"/>
			</td>
			<td colspan="2">
				<xsl:value-of select="cDefinitionName/node()"/>
			</td>
			<td colspan="2">
				<xsl:value-of select="cContentValueXpath/node()"/>
			</td>
			<td colspan="2">
				<xsl:value-of select="bBriefNotDetail/node()"/>
			</td>
			<td class="clearfix">
				<a href="{$appPath}?ewCmd=FilterIndex&amp;ewCmd2=delete&amp;pgid={/Page/@id}&amp;id={@nContentIndexDefKey}&amp;SchemaName={../@Name}" class="btn btn-danger btn-xs pull-right">
					<i class="fa fa-trash-o fa-white">
						<xsl:text> </xsl:text>
					</i><xsl:text> </xsl:text>Del
				</a>
				<a href="{$appPath}?ewCmd=FilterIndex&amp;pgid={/Page/@id}&amp;id={@nContentIndexDefKey}&amp;SchemaName={../@Name}" class="btn btn-primary btn-xs pull-right">
					<i class="fa fa-edit fa-white">
						<xsl:text> </xsl:text>
					</i><xsl:text> </xsl:text>Edit
				</a>
				<a href="{$appPath}?ewCmd=FilterIndex&amp;ewCmd2=update&amp;pgid={/Page/@id}&amp;id={@nContentIndexDefKey}&amp;DefName={@cDefinitionName}&amp;SchemaName={../@Name}" class="btn btn-primary btn-xs pull-right">
					<i class="fa fa-recycle fa-white">
						<xsl:text> </xsl:text>
					</i><xsl:text> </xsl:text>Re-Index
				</a>
			</td>
		</tr>
	</xsl:template>

	<!-- code for filter indexes -->

	<xsl:template match="Page[@layout='FilterIndex']" mode="Admin">
		<div class="row" id="template_AdvancedMode">
			<div class="col-md-3">
				<div class="panel panel-default">
					<div class="panel-body">
						Filter Indexes
					</div>
				</div>
			</div>
			<div class="col-md-9">
				<div class="panel panel-default">
					<div class="panel-heading">
						<p class="btn-group headerButtons">
							<xsl:if test="ContentDetail/Content[@type='xform']">
								<a href="{$appPath}?ewCmd=FilterIndex&amp;pgid={/Page/@id}" class="btn btn-default" title="Back to FilterIndexes">
									<i class="fa fa-caret-left">&#160; </i>&#160;Back to FilterIndexex List
								</a>
							</xsl:if>
						</p>
					</div>
					<div class="panel-body">
						<xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
					</div>
					<xsl:apply-templates select="ContentDetail/Content[@type!='xform']" mode="ListIndexes"/>
				</div>
			</div>

		</div>
	</xsl:template>


</xsl:stylesheet>

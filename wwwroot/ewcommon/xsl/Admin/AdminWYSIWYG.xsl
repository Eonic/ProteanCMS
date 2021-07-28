<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml"  xmlns:ew="urn:ew">

	<xsl:template name="eonicwebProductName">
    <xsl:choose>
      <xsl:when test="$page/Settings/add[@key='web.eonicwebProductName']/@value!=''">
        <xsl:value-of select="$page/Settings/add[@key='web.eonicwebProductName']/@value"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Protean</xsl:text>
	      <strong>CMS</strong>
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
        <xsl:text>support@eonic.co.uk</xsl:text>
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

  <!--xsl:template name="getSiteURL">
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
  </xsl:template-->

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

  <xsl:template match="Page[@previewMode]" mode="adminStyle">
    <link type="text/css" rel="stylesheet" href="/ewcommon/css/admin/preview.less?v={$scriptVersion}"/>
  </xsl:template>

  <xsl:template match="Page" mode="adminJs">
    <xsl:if test="ContentDetail/Content[@type='xform']/descendant::submit[contains(@class,'getGeocodeButton')]">
      <script type="text/javascript" src="//maps.google.com/maps/api/js?sensor=false&amp;key=AIzaSyDgWT-s0qLPmpc4aakBNkfWsSapEQLUEbo">&#160;</script>
    </xsl:if>
    <xsl:call-template name="bundle-js">
      <xsl:with-param name="comma-separated-files">
        <xsl:text>~/ewcommon/js/jQuery/jsScrollPane/jquery.jscrollpane.min.js,</xsl:text>
        <xsl:text>~/ewcommon/js/jQuery/jsScrollPane/jquery.mousewheel.js,</xsl:text>
        <xsl:text>~/ewcommon/js/ewAdmin.js,</xsl:text>
        <xsl:text>~/ewcommon/js/codemirror/codemirror.js,</xsl:text>
        <xsl:text>~/ewcommon/js/jQuery/jquery.magnific-popup.min.js,</xsl:text>
        <xsl:text>~/ewcommon/js/codemirror/mirrorframe.js,</xsl:text>
	<xsl:text>~/ewcommon/js/vuejs/vue.min.js,</xsl:text>
	<xsl:text>~/ewcommon/js/vuejs/axios.min.js,</xsl:text>
	<xsl:text>~/ewcommon/js/vuejs/polyfill.js,</xsl:text>
	<xsl:text>~/ewcommon/js/vuejs/protean-vue.js</xsl:text>
      </xsl:with-param>
      <xsl:with-param name="bundle-path">
        <xsl:text>~/Bundles/Admin</xsl:text>
      </xsl:with-param>
    </xsl:call-template>
 
    <xsl:apply-templates select="." mode="siteAdminJs"/>

    <xsl:apply-templates select="." mode="LayoutAdminJs"/>

    <!--xsl:apply-templates select="." mode="xform_control_scripts"/-->

  </xsl:template>

  <!-- -->
  <xsl:template match="Page" mode="siteAdminJs"></xsl:template>
  
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
      <xsl:attribute name="id">
        <xsl:text>page</xsl:text>
        <xsl:value-of select="@id"/>
        <xsl:if test="@artid!=''">
          <xsl:text>-art</xsl:text>
          <xsl:value-of select="@artid"/>
        </xsl:if>
      </xsl:attribute>
      <xsl:apply-templates select="." mode="bodyStyle"/>
      <xsl:apply-templates select="PreviewMenu"/>
      <xsl:apply-templates select="." mode="bodyDisplay"/>
      <xsl:if test="/Page/Contents/Content[@name='criticalPathCSS'] and not($adminMode)">
        <xsl:apply-templates select="." mode="commonStyle"/>
      </xsl:if>
      <xsl:apply-templates select="." mode="footerJs"/>
    </body>
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
            <span class="hidden"><xsl:call-template name="eonicwebCMSName"/></span>
          </div>

          <div id="sectionIcon" class="sectionfa-Preview">
            <span class="hidden">Preview</span>
          </div>
        </div>
        <div id="headers" class="preview form-inline">
          <form class="ewXform" id="previewSettings" action="?ewCmd=PreviewOn">
          <span id="breadcrumb">
            <strong>
              <xsl:value-of select="/Page/PreviewMenu/User/@name"/>
            </strong>
            <xsl:text> impersonating </xsl:text><strong>
              <xsl:choose>
                <xsl:when test="/Page/User">
                  <xsl:value-of select="/Page/User/@name"/>
                </xsl:when>
                <xsl:otherwise>anonymous</xsl:otherwise>
              </xsl:choose>
              
            </strong>
            <xsl:text> </xsl:text>
           
            <label for="PreviewDate"> as of date&#160;</label>
            
            <span class="input-group">
              <input type="text" class="form-control jqDatePicker" name="dPreviewDate" id="dPreviewDate" value="{/Page/@pageViewDate}" oninput="document.getElementById('previewSettings').submit();">
              </input>
              <span class="input-group-btn">
                <label for="dPreviewDate" class="input-group-addon btn btn-default">
                  <i class="fa fa-calendar"> </i>
                </label>
              </span>
             </span>

            &#160;
            <xsl:choose>
              <xsl:when test="/Page/@previewHidden='on'">
                <a href="?ewcmd=PreviewOn&amp;ewCmd2=hideHidden" class="btn btn-default">Hide Hidden</a>
              </xsl:when>
              <xsl:otherwise>
                <a href="?ewcmd=PreviewOn&amp;ewCmd2=showHidden" class="btn btn-default">Show Hidden</a>
              </xsl:otherwise>
            </xsl:choose>
             
          
          </span>
          </form>
          <xsl:text> </xsl:text>
          <xsl:text> </xsl:text>
          <a href="?ewCmd=Normal&amp;pgid={/Page/@id}" class="btn btn-success btn-lg pull-right" id="previewBack">
            Back to Admin<xsl:text> </xsl:text><i class="fa fa-arrow-alt-circle-up fa-white">
            <xsl:text> </xsl:text>
          </i>
        </a>
        </div>
        <div class="terminus"> </div>
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
              <a id="myaccount" href="{$appPath}?ewCmd=EditDirItem&amp;DirType=User&amp;id={User/@id}">
                <i class="fa fa-user">
                  <xsl:text> </xsl:text>
                </i>
                <xsl:value-of select="User/@name"/>
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
            <span class="admin-logo-text">eonic<strong>web</strong>5</span>
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
        <!-- /.navbar-collapse -->
      </nav>

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
            <xsl:if test="/Page/ContentDetail">
              <xsl:variable name="liveURL">
                <xsl:apply-templates select="$page/ContentDetail/Content" mode="getHref">
                  <xsl:with-param name="itemMode" select="'live'"/>
                </xsl:apply-templates>
              </xsl:variable>
              <xsl:variable name="liveName">
                <xsl:value-of select="$page/ContentDetail/Content/@name"/>
              </xsl:variable>
              
              <li class="sharing-tools dropdown">
                <a href="" class="dropdown-toggle" data-toggle="dropdown">
                  <i class="fas fa-share-alt">&#160;</i>
                  Share
                 
                </a>
                <ul class="dropdown-menu">
                  <li>
                    <a href="https://www.facebook.com/sharer/sharer.php?u={$sitename}{$liveURL}" target="_new">
                      <i class="fab fa-facebook">&#160;</i>
                      Facebook</a>
                  </li>
                  <li>
                    <a href="https://twitter.com/home?status={$sitename}{$liveURL}" target="_new">
                      <i class="fab fa-twitter">&#160;</i>Twitter</a>
                  </li>
                  <li>
                    <a href="https://www.linkedin.com/shareArticle?mini=true&amp;url={$sitename}{$liveURL}&amp;title={$liveName}&amp;summary=&amp;source=" target="_new">
                      <i class="fab fa-linkedin">&#160;</i>LinkedIn</a>
                  </li>
                  <li>
                    <a href="{$sitename}{$liveURL}">
                      <i class="fas fa-globe">&#160;</i>
                      Live URL
                    </a>
                  </li>
                </ul>
              </li>
            </xsl:if>
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
            <!--<span class="glyphicon glyphicon-eye-open"></span>-->
            <xsl:apply-templates select="MenuItem/MenuItem/MenuItem/MenuItem[@cmd='PreviewOn']" mode="previewLink">
              <xsl:with-param name="level">1</xsl:with-param>
            </xsl:apply-templates>
            <xsl:text> </xsl:text>
          </li>

        </ul>
        </div>
        <!-- /.navbar-collapse -->

      </nav>
      
      <div id="headers">
        
        

        <xsl:apply-templates select="/Page" mode="UserGuide"/>

      </div>

   
    </div>
    <div class="terminus">&#160;</div>
    <xsl:if test="not(/Page[@ewCmd='Normal'])">
      <!--<div id="breadcrumb">-->
      <ol class="breadcrumb admin-breadcrumb">
        <xsl:apply-templates select="/" mode="adminBreadcrumb"/>
      </ol>
      <!--</div>-->
    </xsl:if>
  </xsl:template>
  
    <xsl:template match="Page" mode="adminBreadcrumb">
     <xsl:apply-templates select="AdminMenu/descendant-or-self::MenuItem[descendant-or-self::MenuItem[@cmd=/Page/@ewCmd or contains(@subCmds,$subMenuCommand)]]" mode="adminLink"/>
  </xsl:template>
  <!-- -->
  <!--   ################################################   User Guide  ##################################################   -->
  <!-- -->
  <xsl:template match="Page" mode="UserGuide">
    <xsl:variable name="ewCmd" select="/Page/Request/QueryString/Item[@name='ewCmd']/node()"/>
    <xsl:variable name="cContentSchemaName" select="/Page/ContentDetail/descendant::cContentSchemaName/node()"/>
    <xsl:variable name="moduleType" select="/Page/ContentDetail/descendant::Content/@moduleType"/>
    <button id="btnHelpEditing">
      <i class="fas fa-graduation-cap fa-lg">
        <xsl:text> </xsl:text>
      </i><br/>
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
            <a target="_new" id="userGuideURL">
              <xsl:attribute name="href">
                <xsl:text>/ewcommon/tools/UserGuide.ashx?fRef=</xsl:text>
                <xsl:choose>
                  <xsl:when test="$ewCmd='EditContent' or $ewCmd='CopyContent' or $ewCmd='AddModule' or $ewCmd='AddContent'">
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
          </p>
        </div>
      </div>
    </div>
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
          <a href="mailto:{$supportEmail}" title="Email Eonic">
            <xsl:value-of select="$supportEmail"/>
          </a>
          <xsl:text> | </xsl:text>
          <a title="view the latest news from eonic">
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
          <xsl:text> </xsl:text><xsl:value-of select="@name"/>
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
            <xsl:value-of select="count(/Page/ContentDetail/PageVersions/Version[@id!=/Page/@id])"/>
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
  <!-- ############### Website Inline Page Editing Buttons ############################## -->
  <!-- -->
  <xsl:template match="Page" mode="inlinePopupPageAdd">
    <xsl:param name="text"/>
    <xsl:if test="AdminMenu/descendant-or-self::MenuItem[@cmd='AddPage'] and $adminMode">
      <div class="ewAdmin options">
        <a href="?ewCmd=AddPage&amp;parId={/Page/@id}" class="add adminButton">
          <xsl:value-of select="$text"/>
        </a>
      </div>
    </xsl:if>
  </xsl:template>

  <xsl:template match="MenuItem" mode="inlinePopupOptions">
    <xsl:param name="class"/>
    <xsl:param name="sortBy"/>
    <xsl:if test="$page/AdminMenu/descendant-or-self::MenuItem[@cmd='EditPage'] and $adminMode">
      <xsl:attribute name="class">
        <xsl:if test="$class!=''">
          <xsl:value-of select="$class"/>
        </xsl:if>
        <xsl:if test="@class!=''">
          <xsl:value-of select="@class"/>
        </xsl:if>
        <xsl:value-of select="@class"/>
        <xsl:text> editable</xsl:text>
      </xsl:attribute>

      <div class="ewAdmin options">
        <div class="dropdown pull-right">
          <a href="#" class="btn btn-primary btn-xs" data-toggle="dropdown">
            <i class="fa fa-pencil-square-o fa-lg">&#160;</i>&#160;
            <xsl:if test="@status=0">[hidden]</xsl:if>
            <i class="fa fa-caret-down">&#160;</i>
          </a>
          <ul class="dropdown-menu">
            <li>
              <a href="?ewCmd=EditPage&amp;pgid={@id}" title="Click here to edit this page">
                <i class="fa fa-pencil-square-o">&#160;</i>&#160;
                Edit Page Settings
              </a>
            </li>
            <li>
              <a href="?ewCmd=MovePage&amp;pgid={@id}" title="Click here to move this page">
                <i class="fa fa-mail-forward">&#160;</i>&#160;Move
              </a>
            </li>
            <xsl:if test="@status='1'">
              <li>
                <a href="?ewCmd=HidePage&amp;pgid={@id}" title="Click here to hide this page">
                  <i class="fa fa-times-circle">
                    <xsl:text> </xsl:text>
                  </i>
                  <xsl:text> </xsl:text>Hide
                </a>
              </li>
            </xsl:if>
            <xsl:if test="@status='0'">
              <li>
                <a href="?ewCmd=ShowPage&amp;pgid={@id}" title="Click here to show this page">
                  <i class="fa fa-check-square-o">&#160;</i>&#160;Show
                </a>
              </li>
              <li>
                <a href="?ewCmd=DeletePage&amp;pgid={@id}" title="Click here to delete this page">
                  <i class="fa fa-trash-o">&#160;</i>&#160;Delete
                </a>
              </li>
            </xsl:if>
            <xsl:if test="$sortBy = 'Position'">
              <li class="updown">
                <a href="?ewCmd=MoveTop&amp;pgid={@id}" class="btn btn-xs" title="Move this page to the top">
                  <i class="fa fa-step-backward fa-rotate-90">&#160;</i>
                </a>
                <a href="?ewCmd=MoveUp&amp;pgid={@id}" class="btn btn-xs" title="Move this page up by one space">
                  <i class="fa fa-caret-up fa-lg">&#160;</i>
                </a>
                <a href="?ewCmd=MoveDown&amp;pgid={@id}" class="btn btn-xs" title="Move this page down by one space">
                  <span>&#160;</span>
                </a>
                <a href="?ewCmd=MoveBottom&amp;pgid={@id}" class="btn btn-xs" title="Move this page to the bottom">
                  <i class="fa fa-step-forward fa-rotate-90">&#160;</i>
                </a>
              </li>
            </xsl:if>
          </ul>
        </div>
      </div>
    </xsl:if>
  </xsl:template>


  <!-- -->
  <!-- ############### Website Inline Content Editing Buttons ############################## -->
  <!-- -->
  <xsl:template match="Page" mode="inlinePopupSingle">
    <xsl:param name="type"/>
    <xsl:param name="text"/>
    <xsl:param name="name"/>
    <xsl:param name="class"/>

    <xsl:choose>
      <xsl:when test="/Page/Contents/Content[@name=$name]">
        <!-- Do we need to tie it down by Type? What about bespoke types?-->
        <!--<xsl:when test="/Page/Contents/Content[@name=$name and (@type='PlainText' or @type='FormattedText' or @type='Image')]">-->
        <!-- The edit buttons only needed, when the basic of content types, others will have the Edit buttons within their displayBrief templates. -->
        <xsl:if test="AdminMenu/descendant-or-self::MenuItem[@cmd='EditContent'] and $adminMode">
          <xsl:apply-templates select="/Page/Contents/Content[@name=$name]" mode="inlinePopupSingleOptions">
            <xsl:with-param name="class">
              <xsl:value-of select="$class"/>
            </xsl:with-param>
            <xsl:with-param name="name">
              <xsl:value-of select="$name"/>
            </xsl:with-param>
            <xsl:with-param name="type">
              <xsl:value-of select="$type"/>
            </xsl:with-param>
          </xsl:apply-templates>
        </xsl:if>
      </xsl:when>
      <xsl:otherwise>
        <xsl:if test="AdminMenu/descendant-or-self::MenuItem[@cmd='AddContent'] and $adminMode">
          <div class="ewAdmin options">
            <xsl:apply-templates select="/Page" mode="inlinePopupAdd">
              <xsl:with-param name="type">
                <xsl:value-of select="$type"/>
              </xsl:with-param>
              <xsl:with-param name="text">
                <xsl:value-of select="$text"/>
              </xsl:with-param>
              <xsl:with-param name="name">
                <xsl:value-of select="$name"/>
              </xsl:with-param>
              <xsl:with-param name="class">
                <xsl:value-of select="$class"/>
              </xsl:with-param>
            </xsl:apply-templates>
          </div>
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!---->
  <xsl:template match="Page" mode="addModule">
    <xsl:param name="text"/>
    <xsl:param name="position"/>
    <xsl:param name="class"/>
    <xsl:if test="AdminMenu/descendant-or-self::MenuItem[@cmd='AddModule'] and $adminMode">
      <xsl:attribute name="class">
        <xsl:text>moduleContainer</xsl:text>
        <xsl:if test="$class!=''">
          <xsl:text> </xsl:text>
          <xsl:value-of select="$class"/>
        </xsl:if>
      </xsl:attribute>
      <div class="ewAdmin options">
        <a class="btn btn-default btn-xs pull-right" href="?ewCmd=AddModule&amp;pgid={/Page/@id}&amp;position={$position}">
          <i class="fa fa-th-large">&#160;</i>&#160;
          <xsl:value-of select="$text"/>
        </a>
      </div>
      <div class="addHere">
        <xsl:text>Add a Module Here </xsl:text>
        <xsl:value-of select="$position"/>
      </div>
    </xsl:if>

    <xsl:choose>
      <xsl:when test="/Page/Contents/Content[@position = $position]">
        <xsl:apply-templates select="/Page/Contents/Content[@type='Module' and @position = $position]" mode="displayModule"/>
      </xsl:when>
      <xsl:otherwise>
        <!-- if no contnet, need a space for the compiling of the XSL. -->
        <xsl:text>&#160;</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>





  <xsl:template match="Page" mode="addModule">
    <xsl:param name="text"/>
    <xsl:param name="position"/>
    <xsl:param name="class"/>
    <xsl:if test="AdminMenu/descendant-or-self::MenuItem[@cmd='AddModule'] and $adminMode">
      <xsl:attribute name="class">
        <xsl:text>moduleContainer</xsl:text>
        <xsl:if test="$class!=''">
          <xsl:text> </xsl:text>
          <xsl:value-of select="$class"/>
        </xsl:if>
      </xsl:attribute>
      <div class="ewAdmin options addmodule">
        <a class="btn btn-default btn-xs pull-right" href="?ewCmd=AddModule&amp;pgid={/Page/@id}&amp;position={$position}">
          <i class="fa fa-th-large">&#160;</i>&#160;
          <xsl:value-of select="$text"/>
        </a>
        <div class="addHere">
          <xsl:text>Add a Module Here </xsl:text>
          <xsl:value-of select="$position"/>
        </div>
      </div>
    </xsl:if>
    <xsl:choose>
      <xsl:when test="/Page/Contents/Content[@position = $position]">
        <xsl:apply-templates select="/Page/Contents/Content[@type='Module' and @position = $position]" mode="displayModule"/>
      </xsl:when>
      <xsl:otherwise>
        <!-- if no contnet, need a space for the compiling of the XSL. -->
        <xsl:text>&#160;</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template match="Page[@cssFramework='bs3']" mode="addModule">
    <xsl:param name="text"/>
    <xsl:param name="position"/>
    <xsl:param name="class"/>
    <xsl:choose>
      <xsl:when test="$position='header' or $position='footer' or ($position='column1' and @layout='Modules_1_column')">
        <xsl:if test="AdminMenu/descendant-or-self::MenuItem[@cmd='AddModule'] and $adminMode">
          <section>
            <xsl:if test="$class='container'">
              <xsl:attribute name="class">wrapper-sm</xsl:attribute>
            </xsl:if>
            <div id="{$position}" class="{$class} moduleContainer">
              <div class="ewAdmin options addmodule">
                <a class="btn btn-default btn-xs pull-right" href="?ewCmd=AddModule&amp;pgid={/Page/@id}&amp;position={$position}">
                  <i class="fa fa-th-large">&#160;</i>&#160;
                  <xsl:value-of select="$text"/>
                </a>
                <div class="addHere">
                  <strong>
                    <xsl:value-of select="$position"/>
                  </strong>
                  <xsl:text> - drag a module here </xsl:text>
                </div>
              </div>
            </div>
          </section>
        </xsl:if>
        <xsl:for-each select="/Page/Contents/Content[@type='Module' and @position = $position]">
          <section class="wrapper-sm {@background}">
            <xsl:attribute name="class">
              <xsl:text>wrapper-sm </xsl:text>
              <xsl:value-of select="@background"/>
              <xsl:apply-templates select="." mode="hideScreens" />
              <xsl:if test="@marginBelow='false'">
                <xsl:text> margin-bottom-0 </xsl:text>
              </xsl:if>
              <xsl:if test="@data-stellar-background-ratio!='10'">
                <xsl:text> parallax-wrapper </xsl:text>
              </xsl:if>
            </xsl:attribute>
            <xsl:if test="@data-stellar-background-ratio!='10'">
              <xsl:attribute name="data-parallax-speed">
                <xsl:if test="@data-stellar-background-ratio&lt;'5'">
                  <xsl:text>1.3</xsl:text>
                </xsl:if>
                <xsl:if test="@data-stellar-background-ratio&gt;='5' and @data-stellar-background-ratio&lt;'10'">
                  <xsl:text>1.6</xsl:text>
                </xsl:if>
                <xsl:if test="@data-stellar-background-ratio&gt;='10' and @data-stellar-background-ratio&lt;'15'">
                  <xsl:text>2</xsl:text>
                </xsl:if>
                <xsl:if test="@data-stellar-background-ratio&gt;='15' and @data-stellar-background-ratio&lt;'20'">
                  <xsl:text>3</xsl:text>
                </xsl:if>
                <xsl:if test="@data-stellar-background-ratio&gt;='20' and @data-stellar-background-ratio&lt;'25'">
                  <xsl:text>4</xsl:text>
                </xsl:if>
                <xsl:if test="@data-stellar-background-ratio&gt;='25'">
                  <xsl:text>5</xsl:text>
                </xsl:if>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="@data-stellar-background-ratio!='0'">
              <!--<xsl:attribute name="data-stellar-background-ratio">
                <xsl:value-of select="(@data-stellar-background-ratio div 10)"/>
              </xsl:attribute>-->

              
            </xsl:if>
            <xsl:if test="@backgroundImage!=''">
              <xsl:choose>
                <xsl:when test="@data-stellar-background-ratio!='0'">
                  <xsl:choose>
                    <xsl:when test="@data-stellar-background-ratio!='10'">
                        <div class="parallax" data-parallax-image="{@backgroundImage}">
                          <xsl:text> </xsl:text>
                        </div>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:attribute name="style">
                        background-image: url('<xsl:value-of select="@backgroundImage"/>');
                      </xsl:attribute>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:attribute name="style">
                    background-image: url('<xsl:value-of select="@backgroundImage"/>');
                  </xsl:attribute>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:if>
            <xsl:choose>
              <xsl:when test="@fullWidth='true'">
                <div class="fullwidthContainer">
                  <xsl:apply-templates select="." mode="displayModule"/>
                  <xsl:text> </xsl:text>
                </div>
              </xsl:when>
              <xsl:otherwise>
                <div class="{$class} content">
                  <xsl:apply-templates select="." mode="displayModule"/>
                  <xsl:text> </xsl:text>
                </div>
              </xsl:otherwise>
            </xsl:choose>
          </section>
        </xsl:for-each>
      </xsl:when>
      <xsl:otherwise>
        <xsl:if test="AdminMenu/descendant-or-self::MenuItem[@cmd='AddModule'] and $adminMode">
          <xsl:attribute name="class">
            <xsl:text>moduleContainer</xsl:text>
            <xsl:if test="$class!=''">
              <xsl:text> </xsl:text>
              <xsl:value-of select="$class"/>
            </xsl:if>
          </xsl:attribute>
          <div class="ewAdmin options addmodule">
            <a class="btn btn-default btn-xs pull-right" href="?ewCmd=AddModule&amp;pgid={/Page/@id}&amp;position={$position}">
              <i class="fa fa-th-large">&#160;</i>&#160;<xsl:value-of select="$text"/>
            </a>
            <div class="addHere">
              <strong>
                <xsl:value-of select="$position"/>
              </strong>
              <xsl:text> - drag a module here </xsl:text>
            </div>
          </div>
        </xsl:if>
        <xsl:choose>
          <xsl:when test="/Page/Contents/Content[@position = $position]">
            <xsl:apply-templates select="/Page/Contents/Content[@type='Module' and @position = $position]" mode="displayModule"/>
          </xsl:when>
          <xsl:otherwise>
            <!-- if no contnet, need a space for the compiling of the XSL. -->
            <xsl:text>&#160;</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!---->
  <xsl:template match="Page" mode="addMasonaryModule">
    <xsl:param name="text"/>
    <xsl:param name="position"/>
    <xsl:param name="class"/>
    <xsl:variable name="posStart" select="substring-before($position,'-')"/>
    <xsl:if test="AdminMenu/descendant-or-self::MenuItem[@cmd='AddModule'] and $adminMode">
      <xsl:attribute name="class">
        <xsl:text>moduleContainer</xsl:text>
        <xsl:if test="$class!=''">
          <xsl:text> </xsl:text>
          <xsl:value-of select="$class"/>
        </xsl:if>
      </xsl:attribute>
      <div class="ewAdmin options addmodule">
        <a class="btn btn-default btn-xs pull-right" href="?ewCmd=AddModule&amp;pgid={/Page/@id}&amp;position={$position}">
          <i class="fa fa-th-large">&#160;</i>&#160;
          <xsl:value-of select="$text"/>
        </a>
        <div class="addHere">
          <xsl:text>Add a Module Here </xsl:text>
          <xsl:value-of select="$position"/>
        </div>
      </div>
    </xsl:if>

    <xsl:choose>
      <xsl:when test="/Page/Contents/Content[starts-with(@position, substring-before($position,'-'))]">
        <div id="isotope-module">
          <xsl:apply-templates select="/Page/Contents/Content[starts-with(@position, $posStart)]" mode="displayModule"/>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <!-- if no contnet, need a space for the compiling of the XSL. -->
        <xsl:text>&#160;</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template match="Page" mode="addSingleModule">
    <xsl:param name="text"/>
    <xsl:param name="position"/>
    <xsl:param name="class"/>
    <xsl:if test="AdminMenu/descendant-or-self::MenuItem[@cmd='AddModule'] and $adminMode">
      <xsl:attribute name="class">
        <xsl:text>moduleContainer</xsl:text>
        <xsl:if test="$class!=''">
          <xsl:text> </xsl:text>
          <xsl:value-of select="$class"/>
        </xsl:if>
      </xsl:attribute>
      <xsl:if test="not(/Page/Contents/Content[@position = $position])">
        <div class="ewAdmin options addmodule">
          <a class="btn btn-default btn-xs pull-right" href="?ewCmd=AddModule&amp;pgid={/Page/@id}&amp;position={$position}">
            <i class="fa fa-th-large">&#160;</i>&#160;<xsl:text>Add Module</xsl:text>
          </a>
          <div class="addHere">
            <strong>
              <xsl:value-of select="$position"/>
            </strong>
          </div>
        </div>

      </xsl:if>
    </xsl:if>

    <xsl:if test="/Page/Contents/Content[@position = $position]">
      <xsl:apply-templates select="/Page/Contents/Content[@type='Module' and @position = $position]" mode="displayModule"/>
    </xsl:if>

  </xsl:template>

  <xsl:template match="Page" mode="addMailModule">
    <xsl:param name="text"/>
    <xsl:param name="position"/>
    <xsl:if test="/Page/@adminMode">

      <div class="ewAdmin options addmodule">
        <a class="btn btn-default btn-xs pull-right" href="?ewCmd=AddMailModule&amp;pgid={/Page/@id}&amp;position={$position}">
          <i class="fa fa-th-large">&#160;</i>&#160;
          <xsl:value-of select="$text"/>
        </a>
        <div class="addHere">
          <xsl:text>Add a Module Here </xsl:text>
          <xsl:value-of select="$position"/>
        </div>
      </div>
    </xsl:if>
    <xsl:if test="/Page/Contents/Content[@position = $position]">
      <xsl:apply-templates select="/Page/Contents/Content[@type='Module' and @position = $position]" mode="displayModule"/>
    </xsl:if>
  </xsl:template>
  <!-- -->
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
          <div class="dropdown pull-right ewAdmin options addmodule">
            <a href="#" class="btn btn-default btn-xs pull-right" data-toggle="dropdown">
              <i class="fa fa-plus">&#160;</i>&#160;
              <xsl:value-of select="$text"/>&#160;
              <i class="fa fa-caret-down">&#160;</i>
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
                <a href="?ewCmd=AddContent&amp;pgid={/Page/@id}&amp;type={$type}&amp;name={$name}">
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
                  <a href="?ewCmd=AddContent&amp;pgid={/Page/@id}&amp;type={$type}&amp;name={$name}">
                    <i class="fa fa-plus">&#160;</i>&#160;<xsl:text>Add New</xsl:text>
                  </a>
                </li>
                <li>
                  <a href="?ewCmd=LocateSearch&amp;pgid={/Page/@id}&amp;type={$type}">
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
                  <a href="?ewCmd=LocateSearch&amp;pgid={/Page/@id}&amp;type={$type}">
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

  <!-- NON MODULE RELATES -->
  <xsl:template match="Content" mode="inlinePopupRelate">
    <xsl:param name="type"/>
    <xsl:param name="text"/>
    <xsl:param name="name"/>
    <xsl:param name="class"/>
    <xsl:param name="find"/>
    <xsl:param name="direction"/>
    <xsl:param name="relationType"/>
    <xsl:variable name="extendedQuery">
      <xsl:if test="$direction!=''">
        <xsl:text>&amp;RelType=</xsl:text>
        <xsl:value-of select="$direction"/>
      </xsl:if>
      <xsl:if test="$relationType!=''">
        <xsl:text>&amp;relationType=</xsl:text>
        <xsl:value-of select="$relationType"/>
      </xsl:if>
    </xsl:variable>
    <xsl:if test="/Page/AdminMenu/descendant-or-self::MenuItem[@cmd='AddContent'] and $adminMode">
      <div class="ewAdmin options">
        <div class="dropdown pull-right">
          <a href="#" class="btn btn-primary btn-xs" data-toggle="dropdown">
            <i class="fa fa-plus">&#160;</i>
            &#160;<xsl:value-of select="$text"/>&#160;
            <i class="fa fa-caret-down">&#160;</i>
          </a>
          <ul class="dropdown-menu">
            <li>
              <a href="?ewCmd=AddContent&amp;contentParId={@id}&amp;pgid={/Page/@id}&amp;type={$type}&amp;name={$name}{$extendedQuery}">
                <i class="fa fa-plus">&#160;</i>&#160;<xsl:text>Add New</xsl:text>
              </a>
            </li>
            <xsl:if test="/Page/AdminMenu/descendant-or-self::MenuItem[@cmd='RelateSearch']">
              <xsl:if test="$find='true'">
                <li>
                  <a href="?ewCmd=RelateSearch&amp;RelParent={@id}&amp;type={$type}&amp;pgid={/Page/@id}&amp;type={$type}&amp;name={$name}{$extendedQuery}">
                    <i class="fa fa-search">&#160;</i>&#160;<xsl:text>Find Existing</xsl:text>
                  </a>
                </li>
              </xsl:if>
            </xsl:if>
          </ul>
        </div>
      </div>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Content[@type='Module']" mode="inlinePopupRelateTop">
    <xsl:variable name="type" select="@contentType"/>
    <xsl:variable name="find" select="'true'"/>
    <xsl:if test="/Page/AdminMenu/descendant-or-self::MenuItem[@cmd='AddContent'] and $adminMode">
      <xsl:choose>
        <xsl:when test="$type='MenuItem'"></xsl:when>
        <xsl:otherwise>

          <xsl:choose>
            <xsl:when test="@display='all'">
              <xsl:choose>
                <xsl:when test="$type='Module' and @moduleType='Tabbed'">
                  <div class="ewAdmin options">
                    <a class="btn btn-primary btn-xs pull-right" href="?ewCmd=AddModule&amp;pgid={$page/@id}&amp;position=tabbed-{@id}">
                      <i class="fa fa-th-large">&#160;</i>&#160;
                      Add Tab
                    </a>
                  </div>
                </xsl:when>
                <xsl:otherwise>
                  <div class="dropdown pull-right">
                    <a href="#" class="btn btn-default btn-xs" data-toggle="dropdown">
                      <i class="fa fa-plus">&#160;</i>&#160;
                      Add&#160;
                      <i class="fa fa-caret-down">&#160;</i>
                    </a>
                    <ul class="dropdown-menu">
                      <li class="title">
                        Add <xsl:value-of select="$type"/> to page&#160;
                      </li>
                      <li class="divider">&#160;</li>
                      <xsl:if test="not($page/@ewCmd='NormalMail')">
                        <!--So we can't add stuff in news letters that should be related from items on the site-->
                        <li>
                          <a href="?ewCmd=AddContent&amp;pgid={/Page/@id}&amp;type={$type}&amp;name={$type}">
                            <i class="fa fa-plus">&#160;</i>&#160;<xsl:text>Add New</xsl:text>
                          </a>
                        </li>
                      </xsl:if>
                      <xsl:if test="/Page/AdminMenu/descendant-or-self::MenuItem[@cmd='RelateSearch']">
                        <xsl:if test="$find='true'">
                          <li>
                            <a href="?ewCmd=LocateSearch&amp;pgid={/Page/@id}&amp;type={$type}">
                              <i class="fa fa-search">&#160;</i>&#160;<xsl:text>Find Existing</xsl:text>
                            </a>
                          </li>
                        </xsl:if>
                      </xsl:if>
                    </ul>
                  </div>
                </xsl:otherwise>
              </xsl:choose>

            </xsl:when>
            <xsl:when test="@display='related'">

              <xsl:variable name="direction" select="'1way'"/>
              <xsl:variable name="relationType" select="''"/>
              <xsl:variable name="extendedQuery">
                <xsl:if test="$direction!=''">
                  <xsl:text>&amp;RelType=</xsl:text>
                  <xsl:value-of select="$direction"/>
                </xsl:if>
                <xsl:text>&amp;relationType=</xsl:text>
                <xsl:value-of select="$relationType"/>

              </xsl:variable>
              <div class="dropdown pull-right">
                <a href="#" class="btn btn-default btn-xs" data-toggle="dropdown">
                  <i class="fa fa-plus">&#160;</i>&#160;
                  Add&#160;
                  <i class="fa fa-caret-down">&#160;</i>
                </a>
                <ul class="dropdown-menu">
                  <li class="title">
                    Add Related <xsl:value-of select="$type"/>&#160;
                  </li>
                  <li class="divider">&#160;</li>
                  <li>
                    <a href="?ewCmd=AddContent&amp;contentParId={@id}&amp;pgid={/Page/@id}&amp;type={$type}&amp;name={$type}{$extendedQuery}">
                      <i class="fa fa-plus">&#160;</i>&#160;<xsl:text>Add New</xsl:text>
                    </a>
                  </li>
                  <xsl:if test="/Page/AdminMenu/descendant-or-self::MenuItem[@cmd='RelateSearch']">
                    <xsl:if test="$find='true'">
                      <li>
                        <a href="?ewCmd=RelateSearch&amp;RelParent={@id}&amp;pgid={/Page/@id}&amp;type={$type}{$extendedQuery}">
                          <i class="fa fa-search">&#160;</i>&#160;<xsl:text>Find Existing</xsl:text>
                        </a>
                      </li>
                    </xsl:if>
                  </xsl:if>
                </ul>
              </div>
            </xsl:when>
            <xsl:when test="@display='relatedTag'">
              <button class="btn btn-default btn-xs pull-right" disabled="disabled">Related Tags</button>
            </xsl:when>
            <xsl:when test="@display='grabber'">
              <button class="btn btn-default btn-xs pull-right" disabled="disabled">Autofill</button>
            </xsl:when>
            <xsl:when test="@display='modules'">
              <xsl:text> </xsl:text>
            </xsl:when>
            <xsl:otherwise>
              <button class="btn btn-default btn-xs pull-right" disabled="disabled">
                @display=<xsl:value-of select="@display"/>
              </button>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>


  <xsl:template match="Content[@type='Module']" mode="inlinePopupRelate">
    <xsl:param name="type"/>
    <xsl:param name="text"/>
    <xsl:param name="name"/>
    <xsl:param name="class"/>
    <xsl:param name="find"/>
    <xsl:if test="/Page/AdminMenu/descendant-or-self::MenuItem[@cmd='AddContent'] and $adminMode">
      <xsl:choose>
        <xsl:when test="@display='all'">
          <xsl:apply-templates select="/Page" mode="inlinePopupAdd">
            <xsl:with-param name="type" select="$type"/>
            <xsl:with-param name="text" select="$text"/>
            <xsl:with-param name="name" select="$name"/>
            <xsl:with-param name="find" select="$find"/>
          </xsl:apply-templates>
        </xsl:when>
        <xsl:otherwise>
          <div class="ewAdmin options">
            <div class="dropdown pull-right">
              <a href="#" class="btn btn-primary btn-xs" data-toggle="dropdown">
                <i class="fa fa-plus">&#160;</i>
                &#160;<xsl:value-of select="$text"/>&#160;
                <i class="fa fa-caret-down">&#160;</i>
              </a>
              <ul class="dropdown-menu">
                <li>
                  <a href="?ewCmd=AddContent&amp;contentParId={@id}&amp;pgid={/Page/@id}&amp;type={$type}&amp;name={$name}">
                    <i class="fa fa-plus">&#160;</i>&#160;<xsl:text>Add New</xsl:text>
                  </a>
                </li>
                <xsl:if test="/Page/AdminMenu/descendant-or-self::MenuItem[@cmd='RelateSearch']">
                  <xsl:if test="$find='true'">
                    <li>
                      <a href="?ewCmd=RelateSearch&amp;RelParent={@id}&amp;type={$type}&amp;pgid={/Page/@id}">
                        <i class="fa fa-search">&#160;</i>&#160;<xsl:text>Find Existing</xsl:text>
                      </a>
                    </li>
                  </xsl:if>
                </xsl:if>
              </ul>
            </div>
          </div>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Content" mode="inlinePopupRelateSingle">
    <xsl:param name="type"/>
    <xsl:param name="text"/>
    <xsl:param name="name"/>
    <xsl:param name="class"/>
    <xsl:param name="find"/>
    <xsl:if test="/Page/AdminMenu/descendant-or-self::MenuItem[@cmd='AddContent'] and $adminMode">
      <xsl:if test="not(Content[@type=$type]) and not(Contnet[@name=$name])">
        <div class="ewAdmin options">
          <a href="#" class="btn btn-default edit pull-right" data-toggle="dropdown">
            <i class="fa fa-plus">&#160;</i>&#160;
            <xsl:value-of select="$text"/>&#160;
            <i class="fa fa-caret-down">&#160;</i>
          </a>
          <ul class="dropdown-menu">
            <li>
              <a href="?ewCmd=AddContent&amp;contentParId={@id}&amp;pgid={$page/@id}&amp;type={$type}&amp;name={$name}">
                <i class="fa fa-plus">&#160;</i>&#160;<xsl:text>Add New</xsl:text>
              </a>
            </li>
            <xsl:if test="/Page/AdminMenu/descendant-or-self::MenuItem[@cmd='RelateSearch']">
              <li>
                <a href="?ewCmd=RelateSearch&amp;RelParent={@id}&amp;type={$type}&amp;pgid={/Page/@id}">
                  <i class="fa fa-search">&#160;</i>&#160;<xsl:text>Find Existing</xsl:text>
                </a>
              </li>
            </xsl:if>
          </ul>
        </div>
      </xsl:if>
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
  <!-- -->

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

  

  <xsl:template match="Content" mode="inlinePopupOptions">
    <xsl:param name="class"/>
    <xsl:param name="editLabel"/>
    <xsl:param name="sortBy"/>
    <!-- sortBy used as a flag, only set on content items to control wheterh to show the re-order buttons -->
    <xsl:variable name="subTypeOption">
      <xsl:if test="@subType!=''">
        <xsl:text>&amp;type=</xsl:text>
        <xsl:value-of select="@subType"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="versionId">
      <xsl:if test="@versionid!=''">
        <xsl:text>&amp;verId=</xsl:text>
        <xsl:value-of select="@versionid"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="modulePosition">
      <xsl:if test="@type='Module'">
        <xsl:text>&amp;position=</xsl:text>
        <xsl:choose>
          <xsl:when test="starts-with(@position,'column1') and $page/@layout='Modules_Masonary'">
            <xsl:text>column1-</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="@position"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="id" select="@id"/>
    <xsl:if test="$page/AdminMenu/descendant-or-self::MenuItem[@cmd='EditContent'] and $adminMode">
      <xsl:attribute name="class">
        <xsl:if test="$class!=''">
          <xsl:value-of select="$class"/>
        </xsl:if>
        <xsl:if test="@class!=''">
          <xsl:value-of select="@class"/>
        </xsl:if>
        <xsl:value-of select="@class"/>
        <xsl:text> editable</xsl:text>
      </xsl:attribute>
      <div>
        <xsl:attribute name="class">
          <xsl:text>ewAdmin options</xsl:text>
          <xsl:if test="@type='Module'">
            <xsl:text> moduleDrag</xsl:text>
          </xsl:if>
        </xsl:attribute>
        <xsl:if test="@type='Module' and not(starts-with(@position,'column1') and $page/@layout='Modules_Masonary')">
          <a href="#" class="btn btn-primary btn-xs drag pull-right">
            <i class="fa fa-arrows">&#160;</i>
            <span>Move in page</span>
          </a>
        </xsl:if>
        <xsl:if test="starts-with(@position,'column1') and $page/@layout='Modules_Masonary'">
          <div class="dropdown pull-right">
            <a href="#" class="btn btn-primary btn-xs" data-toggle="dropdown">
              <i class="fa fa-crop fa-lg">&#160;</i>&#160;
              <i class="fa fa-caret-down">&#160;</i>
            </a>
            <ul class="dropdown-menu">
              <li class="title">box size</li>
              <li class="mansonary-size">
                <a class="btn" href="javascript:setMasonaryModuleWidth({$page/@id},{@id},'column1-1col')">1x1</a>
                <a class="btn" href="javascript:setMasonaryModuleWidth({$page/@id},{@id},'column1-2col')">2x1</a>
                <a class="btn" href="javascript:setMasonaryModuleWidth({$page/@id},{@id},'column1-3col')">3x1</a>
                <a class="btn" href="javascript:setMasonaryModuleWidth({$page/@id},{@id},'column1-4col')">4x1</a>
              </li>
              <li class="mansonary-size">
                <a class="btn" href="javascript:setMasonaryModuleWidth({$page/@id},{@id},'column1-5col')">5x1</a>
                <a class="btn" href="javascript:setMasonaryModuleWidth({$page/@id},{@id},'column1-1col2row')">1x2</a>
                <a class="btn" href="javascript:setMasonaryModuleWidth({$page/@id},{@id},'column1-2col2row')">2x2</a>
                <a class="btn" href="javascript:setMasonaryModuleWidth({$page/@id},{@id},'column1-3col2row')">3x2</a>
              </li>
              <li class="mansonary-size">
                <a class="btn" href="javascript:setMasonaryModuleWidth({$page/@id},{@id},'column1-4col2row')">4x2</a>
                <a class="btn" href="javascript:setMasonaryModuleWidth({$page/@id},{@id},'column1-5col2row')">5x2</a>
                <a class="btn" href="javascript:setMasonaryModuleWidth({$page/@id},{@id},'column1-1col3row')">1x3</a>
                <a class="btn" href="javascript:setMasonaryModuleWidth({$page/@id},{@id},'column1-2col3row')">2x3</a>
              </li>
              <li class="mansonary-size">
                <a class="btn" href="javascript:setMasonaryModuleWidth({$page/@id},{@id},'column1-3col3row')">3x3</a>
                <a class="btn" href="javascript:setMasonaryModuleWidth({$page/@id},{@id},'column1-4col3row')">4x3</a>
                <a class="btn" href="javascript:setMasonaryModuleWidth({$page/@id},{@id},'column1-5col3row')">5x3</a>
                <a class="btn" href="javascript:setMasonaryModuleWidth({$page/@id},{@id},'column1-1col100pc')">1x&#8734;</a>
              </li>
              <li class="mansonary-size">
                <a class="btn" href="javascript:setMasonaryModuleWidth({$page/@id},{@id},'column1-2col100pc')">2x&#8734;</a>
                <a class="btn" href="javascript:setMasonaryModuleWidth({$page/@id},{@id},'column1-3col100pc')">3x&#8734;</a>
                <a class="btn" href="javascript:setMasonaryModuleWidth({$page/@id},{@id},'column1-4col100pc')">4x&#8734;</a>
                <a class="btn" href="javascript:setMasonaryModuleWidth({$page/@id},{@id},'column1-5col100pc')">5x&#8734;</a>
              </li>
            </ul>
          </div>
        </xsl:if>
        <div class="dropdown pull-right">
          <a href="#" class="btn btn-primary btn-xs" data-toggle="dropdown">
            <xsl:choose>
              <xsl:when test="@contentType!=''">
                <i class="fa fa-gears fa-lg">&#160;</i>&#160;
              </xsl:when>
              <xsl:otherwise>
                <i class="fa fa-pencil-square-o fa-lg">&#160;</i>&#160;
              </xsl:otherwise>
            </xsl:choose>

            <!--<xsl:text>Edit </xsl:text>-->
            <xsl:value-of select="$editLabel"/>
            <xsl:if test="@status=0">
              <xsl:text>&#160;[hidden]...</xsl:text>
            </xsl:if>
            <xsl:if test="@status=2">
              <xsl:text>&#160;[superceeded]</xsl:text>
            </xsl:if>
            <xsl:if test="@status=4">
              <xsl:text>&#160;[preview]</xsl:text>
            </xsl:if>
            <i class="fa fa-caret-down">&#160;</i>
          </a>

          <ul class="dropdown-menu">
            <xsl:choose>
              <xsl:when test="@moduleType!=''">
                <li class="title">
                  <xsl:value-of select="@moduleType"/>
                </li>
                <li class="divider">&#160;</li>
              </xsl:when>
              <xsl:otherwise>
                <li class="title">
                  <xsl:value-of select="@type"/>
                </li>
                <li class="divider">&#160;</li>
              </xsl:otherwise>
            </xsl:choose>


            <!-- WHEN CASCADING - Edit on ParId - else changes to Cascade won't stick.-->
            <!--  except we can't tell if cascaded in XML.
                      when we can replace the below false() with a condition
                      and edit on parId
          -->
            <xsl:if test="starts-with(@position,'column1') and $page/@layout='Modules_Masonary'">
              <li class="updown">
                <a href="?ewCmd=MoveTop&amp;pgid={$pageId}&amp;id={@id}{$modulePosition}" title="Move this item to the top" class="btn btn-xs">
                  <i class="fa fa-step-backward fa-rotate-90">&#160;</i>
                </a>
                <a href="?ewCmd=MoveUp&amp;pgid={$pageId}&amp;id={@id}{$modulePosition}" title="Move this item up by one space" class="btn btn-xs">
                  <i class="fa fa-caret-up fa-lg">&#160;</i>
                </a>
                <a href="?ewCmd=MoveDown&amp;pgid={$pageId}&amp;id={@id}{$modulePosition}" title="Move this item down by one space" class="btn btn-xs">
                  <i class="fa fa-caret-down fa-lg">&#160;</i>
                </a>
                <a href="?ewCmd=MoveBottom&amp;pgid={$pageId}&amp;id={@id}{$modulePosition}" title="Move this item to the bottom" class="btn btn-xs">
                  <i class="fa fa-step-forward fa-rotate-90">&#160;</i>
                </a>
              </li>
            </xsl:if>
            <xsl:variable name="isMail">
              <xsl:if test="$page/@ewCmd='NormalMail'">
                <xsl:text>Mail</xsl:text>
              </xsl:if>
            </xsl:variable>
            <xsl:choose>
              <!-- NEED A TRIGGER FOR ONLY CASCADED STUFF TO EDIT ON PARID <xsl:when test="@parId=/Page/@id">-->
              <xsl:when test="false()">
                <li>
                  <a href="?ewCmd=Edit{$isMail}Content&amp;id={@id}&amp;pgid={@parId}" title="Click here to edit this content">
                    <i class="fa fa-pencil-square-o">&#160;</i>&#160;
                    Edit
                  </a>
                </li>
              </xsl:when>
              <xsl:when test="@status='3' and @versionid!=''">
                <li>
                  <a href="?ewCmd=Edit{$isMail}Content&amp;id={@id}&amp;pgid={@parId}&amp;verId={@versionid}" title="Click here to edit this content">
                    <i class="fa fa-pencil-square">&#160;</i>&#160;Edit Pending Change
                  </a>
                </li>
              </xsl:when>
              <xsl:otherwise>
                <li>
                  <a href="?ewCmd=Edit{$isMail}Content&amp;id={@id}&amp;pgid={$pageId}" title="Click here to edit this content">
                    <i class="fa fa-pencil-square">&#160;</i>&#160;Edit
                  </a>
                </li>
              </xsl:otherwise>
            </xsl:choose>

            <xsl:if test="@status!='3'">
              <xsl:if test="$page/AdminMenu/descendant-or-self::MenuItem[@cmd='CopyContent']">
                <li>
                  <a href="?ewCmd=CopyContent&amp;pgid={$pageId}&amp;id={@id}" title="Click here to create a copy">
                    <i class="fa fa-copy">&#160;</i>&#160;Copy
                  </a>
                </li>
              </xsl:if>
              <xsl:if test="$page/AdminMenu/descendant-or-self::MenuItem[@cmd='MoveContent']">
                <li>
                  <a href="?ewCmd=MoveContent&amp;pgid={$pageId}&amp;id={@id}" title="Click here to move to another page">
                    <i class="fa fa-mail-forward">&#160;</i>&#160;Move
                  </a>
                </li>
              </xsl:if>
              <xsl:if test="$page/AdminMenu/descendant-or-self::MenuItem[@cmd='LocateContent']">
                <li>
                  <a href="?ewCmd=LocateContent&amp;pgid={$pageId}&amp;id={@id}" title="Click here to locate on other pages">
                    <i class="fa fa-mail-reply-all fa-flip-horizontal">&#160;</i>&#160;Locations
                  </a>
                </li>
              </xsl:if>
            </xsl:if>
            <xsl:choose>
              <xsl:when test="$page/Contents/Content/Content[@id=$id] and (@parId != $pageId)">
                <li>
                  <a href="?ewCmd=RemoveContentRelation&amp;relId={$page/Contents/Content[Content/@id=$id]/@id}&amp;id={@id}" title="Click here to unrelate this item">
                    <i class="fa fa-chain-broken">&#160;</i>&#160;Un-relate
                  </a>
                </li>
                <li>
                  <a href="?ewCmd=Normal&amp;pgid={@parId}" title="Click here to remove from this page">
                    <i class="fa fa-eye">
                      <xsl:text> </xsl:text>
                    </i>
                    <xsl:text> </xsl:text>
                    View Parent Page
                  </a>
                </li>
              </xsl:when>
              <xsl:when test="@parId!=$page/@id">
                <li>
                  <a href="?ewCmd=RemoveContentLocation&amp;pgid={$page/@id}&amp;id={@id}" title="Click here to remove from this page">
                    <i class="fa fa-times">&#160;</i>&#160;Remove From Page
                  </a>
                </li>
                <li>
                  <a href="?ewCmd=Normal&amp;pgid={@parId}" title="Click here to remove from this page">
                    <i class="fa fa-eye">
                      <xsl:text> </xsl:text>
                    </i>
                    <xsl:text> </xsl:text>
                    Visit Parent Page
                  </a>
                </li>
              </xsl:when>
              <xsl:otherwise>
                <xsl:if test="@status='1'">
                  <xsl:if test="$page/AdminMenu/descendant-or-self::MenuItem[@cmd='HideContent']">
                    <li>
                      <a href="?ewCmd=HideContent&amp;pgid={$pageId}&amp;id={@id}" title="Click here to hide this item">
                        <i class="fa fa-times-circle">
                          <xsl:text> </xsl:text>
                        </i>
                        <xsl:text> </xsl:text>Hide
                      </a>
                    </li>
                  </xsl:if>
                </xsl:if>
                <xsl:if test="@status='0'">
                  <xsl:if test="$page/AdminMenu/descendant-or-self::MenuItem[@cmd='ShowContent']">
                    <li>
                      <a href="?ewCmd=ShowContent&amp;pgid={$pageId}&amp;id={@id}" title="Click here to show this item">
                        <i class="fa fa-check-square-o">&#160;</i>&#160;Show
                      </a>
                    </li>
                  </xsl:if>
                  <xsl:if test="$page/AdminMenu/descendant-or-self::MenuItem[@cmd='DeleteContent']">
                    <li>
                      <a href="?ewCmd=DeleteContent&amp;pgid={$pageId}&amp;id={@id}" title="Click here to delete this item">
                        <i class="fa fa-trash-o">&#160;</i>&#160;Delete
                      </a>
                    </li>
                  </xsl:if>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>

            <xsl:if test="@type='Poll'">
              <xsl:if test="$page/AdminMenu/descendant-or-self::MenuItem[@cmd='ManagePollVotes']">
                <li>
                  <a href="?ewCmd=ManagePollVotes&amp;pgid={$page/@id}&amp;id={@id}" title="Click here to Manage Poll Votes">
                    <i class="fa fa-check-square">&#160;</i>&#160;Manage Votes
                  </a>
                </li>
              </xsl:if>
            </xsl:if>

            <xsl:if test="$page/AdminMenu/descendant-or-self::MenuItem[@cmd='AwaitingApproval']">
              <li>
                <a href="?ewCmd=ContentVersions&amp;pgid={/Page/@id}&amp;id={@id}{$subTypeOption}" title="Click here to view version history">
                  <i class="fa fa-copy">&#160;</i>&#160;Version History -<xsl:value-of select="$sortBy"/>
                </a>
              </li>
            </xsl:if>

            <xsl:choose>
              <xsl:when test="parent::*[name()='Content']">
                <xsl:variable name="parId" select="parent::*[name()='Content']/@id"/>
                <li class="divider">&#160;</li>
                <li class="updown">
                  <a href="?ewCmd=MoveTop&amp;relId={$parId}&amp;id={@id}{$modulePosition}" title="Move this item to the top" class="btn btn-xs">
                    <i class="fa fa-step-backward fa-rotate-90">&#160;</i>
                  </a>
                  <a href="?ewCmd=MoveUp&amp;relId={$parId}&amp;id={@id}{$modulePosition}" title="Move this item up by one space" class="btn btn-xs">
                    <i class="fa fa-caret-up fa-lg">&#160;</i>
                  </a>
                  <a href="?ewCmd=MoveDown&amp;relId={$parId}&amp;id={@id}{$modulePosition}" title="Move this item down by one space" class="btn btn-xs">
                    <i class="fa fa-caret-down fa-lg">&#160;</i>
                  </a>
                  <a href="?ewCmd=MoveBottom&amp;relId={$parId}&amp;id={@id}{$modulePosition}" title="Move this item to the bottom" class="btn btn-xs">
                    <i class="fa fa-step-forward fa-rotate-90">&#160;</i>
                  </a>
                </li>
              </xsl:when>
              <xsl:when test="$sortBy='' or $sortBy='Position'">
                <li class="divider">&#160;</li>
                <li class="updown">
                  <a href="?ewCmd=MoveTop&amp;pgid={$pageId}&amp;id={@id}{$modulePosition}" title="Move this item to the top" class="btn btn-xs">
                    <i class="fa fa-step-backward fa-rotate-90">&#160;</i>
                  </a>
                  <a href="?ewCmd=MoveUp&amp;pgid={$pageId}&amp;id={@id}{$modulePosition}" title="Move this item up by one space" class="btn btn-xs">
                    <i class="fa fa-caret-up fa-lg">&#160;</i>
                  </a>
                  <a href="?ewCmd=MoveDown&amp;pgid={$pageId}&amp;id={@id}{$modulePosition}" title="Move this item down by one space" class="btn btn-xs">
                    <i class="fa fa-caret-down fa-lg">&#160;</i>
                  </a>
                  <a href="?ewCmd=MoveBottom&amp;pgid={$pageId}&amp;id={@id}{$modulePosition}" title="Move this item to the bottom" class="btn btn-xs">
                    <i class="fa fa-step-forward fa-rotate-90">&#160;</i>
                  </a>
                </li>
              </xsl:when>
              <xsl:otherwise>
                <li class="title">
                  <xsl:text>sorted by: </xsl:text>
                  <xsl:value-of select="$sortBy"/>
                </li>
              </xsl:otherwise>
            </xsl:choose>
          </ul>
        </div>
        <xsl:if test="@contentType!=''">
          <xsl:apply-templates select="." mode="inlinePopupRelateTop"/>
        </xsl:if>
      </div>
    </xsl:if>

  </xsl:template>


  <xsl:template match="User | Group | Company" mode="inlinePopupOptions">
    <xsl:param name="class"/>
    <xsl:param name="editLabel"/>
    <xsl:param name="sortBy"/>
    <!-- sortBy used as a flag, only set on content items to control wheterh to show the re-order buttons -->
    <xsl:variable name="subTypeOption">
      <xsl:if test="@subType!=''">
        <xsl:text>&amp;type=</xsl:text>
        <xsl:value-of select="@subType"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="versionId">
      <xsl:if test="@versionid!=''">
        <xsl:text>&amp;verId=</xsl:text>
        <xsl:value-of select="@versionid"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="id" select="@id"/>
    <xsl:if test="$page/AdminMenu/descendant-or-self::MenuItem[@cmd='EditContent']">
      <xsl:attribute name="class">
        <xsl:if test="$class!=''">
          <xsl:value-of select="$class"/>
        </xsl:if>
        <xsl:if test="@class!=''">
          <xsl:value-of select="@class"/>
        </xsl:if>
        <xsl:value-of select="@class"/>
        <xsl:text> editable</xsl:text>
      </xsl:attribute>
      <div>
        <xsl:attribute name="class">
          <xsl:text>ewAdmin options</xsl:text>
        </xsl:attribute>
        <a class="adminButton popup">
          <xsl:attribute name="href">
            <xsl:apply-templates select="$currentPage" mode="getHref" />
            <xsl:text>&amp;ewCmd=EditDirItem&amp;DirType=</xsl:text>
            <xsl:value-of select="name()"/>
            <xsl:text>&amp;id=</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:attribute>
          <xsl:text>Edit </xsl:text>
          <xsl:value-of select="$editLabel"/>
          <xsl:if test="@status=0">
            <xsl:text>&#160;[hidden]</xsl:text>
          </xsl:if>
        </a>
      </div>
    </xsl:if>
  </xsl:template>

  <!-- -->
  <xsl:template match="Content" mode="inlinePopupSingleOptions">
    <xsl:param name="type"/>
    <xsl:param name="class"/>
    <xsl:param name="name"/>
    <xsl:if test="$page/AdminMenu/descendant-or-self::MenuItem[@cmd='EditContent']">
      <xsl:attribute name="class">
        <xsl:if test="$class!=''">
          <xsl:value-of select="$class"/>
        </xsl:if>
        <xsl:if test="@class!=''">
          <xsl:value-of select="@class"/>
        </xsl:if>
        <xsl:value-of select="@class"/>
        <xsl:text> editable</xsl:text>
      </xsl:attribute>
      <div class="ewAdmin dropdown options pull-right">
        <a href="#" class="btn btn-primary btn-xs pull-right" data-toggle="dropdown">
          <i class="fa fa-pencil-square-o fa-lg">&#160;</i>&#160;
          <xsl:if test="@status=0">[hidden]</xsl:if>
          <i class="fa fa-caret-down">&#160;</i>
        </a>
        <ul class="dropdown-menu">
          <!--xsl:attribute name="onmouseover">adminMenu(this,'onMenu')</xsl:attribute>
						<xsl:attribute name="onmouseout">adminMenu(this,'offMenu')</xsl:attribute-->
          <xsl:choose>
            <xsl:when test="/Page/@id=@parId">
              <li class="title">
                <xsl:value-of select="$name"/>
              </li>
              <li>
                <a href="?ewCmd=EditContent&amp;pgid={/Page/@id}&amp;id={@id}" title="Click here to edit this content">
                  <i class="fa fa-pencil-square-o">&#160;</i>&#160;
                  <xsl:text>Edit</xsl:text>
                </a>
              </li>
            </xsl:when>
            <xsl:otherwise>
              <li class="title">
                <xsl:value-of select="$name"/>
                <xsl:text> from page '</xsl:text>
                <xsl:value-of select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/Contents/Content[@name=$name]/@parId]/@name"/>
                <xsl:text>'</xsl:text>
              </li>
              <li>
                <a href="?ewCmd=EditContent&amp;pgid={/Page/Contents/Content[@name=$name]/@parId}&amp;id={@id}" title="Edit master on page: {/Page/Menu/descendant-or-self::MenuItem[@id=/Page/Contents/Content[@name=$name]/@parId]/@name}">
                  <i class="fa fa-pencil-square-o">&#160;</i>&#160;
                  <xsl:text>Edit</xsl:text>
                </a>
              </li>
              <!--  DO WE NEED ALL THESE IF's?? I'm replacing it with one line that takes the Type from the content node.
                    This is to enable ALL content types, new and bespoke. - WILL 2008-07-28 -->
              <li>
                <a href="?ewCmd=AddContent&amp;pgid={/Page/@id}&amp;type={@type}&amp;name={$name}" alt="replace {@type}">
                  <i class="fa fa-pencil-plus">&#160;</i>&#160;
                  <xsl:text>Replace </xsl:text>
                  <xsl:value-of select="@type"/>
                </a>
              </li>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:if test="$page/AdminMenu/descendant-or-self::MenuItem[@cmd='MoveContent']">
            <li>
              <a href="?ewCmd=MoveContent&amp;pgid={/Page/@id}&amp;id={@id}" title="Click here to move this content" >
                <i class="fa fa-mail-forward">&#160;</i>&#160;Move
              </a>
            </li>
          </xsl:if>
          <xsl:if test="$page/AdminMenu/descendant-or-self::MenuItem[@cmd='LocateContent']">
            <li>
              <a href="?ewCmd=LocateContent&amp;pgid={/Page/@id}&amp;id={@id}" title="Click here to set which pages to show this content on">
                <i class="fa fa-mail-reply-all fa-flip-horizontal">&#160;</i>&#160;Locations
              </a>
            </li>
          </xsl:if>
          <xsl:if test="@status='1'">
            <xsl:if test="$page/AdminMenu/descendant-or-self::MenuItem[@cmd='HideContent']">
              <li>
                <a href="?ewCmd=HideContent&amp;pgid={/Page/@id}&amp;id={@id}" title="Click here to hide this item">
                  <i class="fa fa-times-circle">&#160;</i>&#160;Hide
                </a>
              </li>
            </xsl:if>
          </xsl:if>
          <xsl:if test="@status='0'">
            <xsl:if test="$page/AdminMenu/descendant-or-self::MenuItem[@cmd='ShowContent']">
              <li>
                <a href="?ewCmd=ShowContent&amp;pgid={/Page/@id}&amp;id={@id}" title="Click here to show this item">
                  <i class="fa fa-check-square-o">&#160;</i>&#160;Show
                </a>
              </li>
            </xsl:if>
            <xsl:if test="$page/AdminMenu/descendant-or-self::MenuItem[@cmd='DeleteContent']">
              <li>
                <a href="?ewCmd=DeleteContent&amp;pgid={/Page/@id}&amp;id={@id}" title="Click here to delete this item">
                  <i class="fa fa-trash-o">&#160;</i>&#160;Delete
                </a>
              </li>
            </xsl:if>
          </xsl:if>
        </ul>
      </div>
    </xsl:if>
  </xsl:template>


  <!-- -->
  <xsl:template match="Content" mode="adminContentMenuOptions">
    <a href="?ewCmd=MoveTop&amp;pgid={/Page/@id}&amp;id={@id}" class="top" title="Move this item to the top">
      <span>&#160;</span>
    </a>
    <a href="?ewCmd=MoveUp&amp;pgid={/Page/@id}&amp;id={@id}" class="up" title="Move this item up by one space">
      <span>&#160;</span>
    </a>
    <a href="?ewCmd=MoveDown&amp;pgid={/Page/@id}&amp;id={@id}" class="down" title="Move this item down by one space">
      <span>&#160;</span>
    </a>
    <a href="?ewCmd=MoveBottom&amp;pgid={/Page/@id}&amp;id={@id}" class="bottom" title="Move this item to the bottom">
      <span>&#160;</span>
    </a>
    <a href="?ewCmd=EditContent&amp;pgid={/Page/@id}&amp;id={@id}" class="edit">Edit</a>
    <span class="hidden"> | </span>
    <a href="?ewCmd=MoveContent&amp;pgid={/Page/@id}&amp;id={@id}" class="move">Move</a>
    <span class="hidden"> | </span>
    <a href="?ewCmd=CopyContent&amp;pgid={/Page/@id}&amp;id={@id}" class="copy">Copy</a>
    <span class="hidden"> | </span>
    <a href="?ewCmd=LocateContent&amp;pgid={/Page/@id}&amp;id={@id}" class="locations">Locations</a>
    <span class="hidden"> | </span>
    <xsl:if test="@status='1'">
      <a href="?ewCmd=HideContent&amp;pgid={/Page/@id}&amp;id={@id}" class="hide">Hide</a>
      <span class="hidden"> | </span>
    </xsl:if>
    <xsl:if test="@status='0'">
      <a href="?ewCmd=DeleteContent&amp;pgid={/Page/@id}&amp;id={@id}" class="delete">Delete</a>
      <span class="hidden"> | </span>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>

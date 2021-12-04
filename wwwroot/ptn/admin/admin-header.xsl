<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                  xmlns:v-bind="http://example.com/xml/v-bind" xmlns:v-on="http://example.com/xml/v-on"
                  xmlns:v-for="http://example.com/xml/v-for" xmlns:v-slot="http://example.com/xml/v-slot"
                  xmlns:v-if="http://example.com/xml/v-if" xmlns:v-else="http://example.com/xml/v-else"
                  xmlns:v-model="http://example.com/xml/v-model">
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
    <!--<div class="dropdown" style="margin-top:50px;z-index:103">
      <button class="btn btn-secondary dropdown-toggle" type="button" id="dropdownMenuButton" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
        Dropdown button
      </button>
      <div class="dropdown-menu" aria-labelledby="dropdownMenuButton">
        <a class="dropdown-item" href="#">Action</a>
        <a class="dropdown-item" href="#">Another action</a>
        <a class="dropdown-item" href="#">Something else here</a>
      </div>
    </div>-->
    <div id="adminHeader" class="affix-top navbar-fixed-top">
      
      <!-- MAIN MENU -->
      <nav class="navbar navbar-expand-xl navbar-dark bg-dark admin-main-menu" role="navigation">
        
        <div class="container-fluid">
          

          <div class="admin-navbar-brand dropdown">
            <xsl:if test="/Page[@ewCmd='AdmHome']">
              <xsl:attribute name="class">admin-navbar-brand dashboard-logo dropdown</xsl:attribute>
            </xsl:if>
            <button class="navbar-logo dropdown-toggle" type="button" id="dropdownLogoButton" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
              <img src="{$CMSLogo}" alt="{$CMSName}" class="cms-logo"/>
            </button>
            <div class=" admin-logo-dropdown dropdown-menu" aria-labelledby="dropdownLogoButton">
              <xsl:if test="/Page[@ewCmd='AdmHome']">
                <xsl:attribute name="class">dropdown-menu admin-logo-dropdown dashboard-menu</xsl:attribute>
              </xsl:if>
              <div class="clearfix dd-logo">
                <span>
                  <img src="{$CMSLogo}" alt="{$CMSName}" class="cms-logo-dd"/>
                  <strong>Protean</strong>CMS
                </span>
                <a id="logoff" href="{$appPath}?ewCmd=LogOff" title="Click here to log off from your active session" >
                  <i class="fa fa-power-off">
                    <xsl:text> </xsl:text>
                  </i>
                  <span>Log Off </span>
                </a>
              </div>
              <ul class="department-menu-dd">
                <xsl:apply-templates select="MenuItem" mode="adminMenuItem">
                  <xsl:with-param name="level">1</xsl:with-param>
                </xsl:apply-templates>
                <xsl:apply-templates select="MenuItem/MenuItem" mode="adminMenuItem">
                  <xsl:with-param name="level">1</xsl:with-param>
                </xsl:apply-templates>
                <li>
                  <a id="myaccount" href="{$appPath}?ewCmd=EditDirItem&amp;DirType=User&amp;id={$page/User/@id}">
                    <i class="fa fa-user">
                      <xsl:text> </xsl:text>
                    </i>
                    <xsl:value-of select="$page/User/@name"/>
                  </a>
                </li>
              </ul>
              <!--<ul>
              <li>
                <a id="logoff" href="{$appPath}?ewCmd=LogOff" title="Click here to log off from your active session" >
                  <i class="fa fa-power-off">
                    <xsl:text> </xsl:text>
                  </i>
                  <span>Log Off </span>
                </a>
              </li>
            </ul>-->
            </div>
          </div>

          <xsl:if test="not(/Page[@ewCmd='AdmHome'])">
            <button type="button" class="navbar-toggler" data-bs-toggle="collapse" data-bs-target="#bs-admin-navbar-collapse-1" aria-controls="navbarSupportedContent" aria-expanded="false" aria-label="Toggle navigation">
              <span class="fas fa-bars">
                <xsl:text> </xsl:text>
              </span>
            </button>
          </xsl:if>

          <!-- Collect the nav links, forms, and other content for toggling -->
          <div class="collapse navbar-collapse" id="bs-admin-navbar-collapse-1">
            <ul class="nav navbar-nav nav-add-more-auto ">
              <xsl:apply-templates select="MenuItem/MenuItem[descendant-or-self::MenuItem[@cmd=$contextCmd]]/MenuItem" mode="adminItem1">
                <xsl:with-param name="level">1</xsl:with-param>
              </xsl:apply-templates>
              <xsl:text> </xsl:text>
            </ul>
          </div>
        </div>
      </nav>
      <div id="headers">
        <xsl:apply-templates select="/Page" mode="UserGuide"/>
      </div>
    </div>
    
    <xsl:if test="not(/Page[@ewCmd='Normal'])">
      <xsl:apply-templates select="/" mode="adminBreadcrumb"/>
    </xsl:if>
  </xsl:template>


  <!--################## MENU ITEMS#########################-->
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
    <li class="nav-item">
      <xsl:choose>
        <xsl:when test="./MenuItem and not(@cmd='EditStructure') and not(@cmd='ScheduledItems') and not(@cmd='DeliveryMethods')">
          <xsl:attribute name="class">nav-item dropdown</xsl:attribute>
          <xsl:apply-templates select="." mode="adminLink1dd">
            <xsl:with-param name="level">
              <xsl:value-of select="$level"/>
            </xsl:with-param>
          </xsl:apply-templates>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="." mode="adminLink1">
            <xsl:with-param name="level">
              <xsl:value-of select="$level"/>
            </xsl:with-param>
          </xsl:apply-templates>
        </xsl:otherwise>
      </xsl:choose>
      <!--<xsl:if test="MenuItem[@cmd=$contextCmd]">-->
      <ul class="dropdown-menu" aria-labelledby="@name-dd">
        <xsl:apply-templates select="MenuItem" mode="adminItem2">
          <xsl:with-param name="level">1</xsl:with-param>
        </xsl:apply-templates>
        <li>
          <xsl:apply-templates select="MenuItem" mode="previewLink">
            <xsl:with-param name="level">1</xsl:with-param>
          </xsl:apply-templates>
        </li>
      </ul>
      <!--</xsl:if>-->
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
        <i class="fa {@icon}">
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
      <a href="{$href}" title="{Description}" class="nav-link">
        <xsl:choose>
          <xsl:when test="@cmd=/Page/@ewCmd">
            <xsl:attribute name="class">nav-link active</xsl:attribute>
          </xsl:when>
          <xsl:when test="descendant-or-self::MenuItem/@cmd=/Page/@ewCmd and @cmd!='AdmHome'">
            <xsl:attribute name="class">nav-link on</xsl:attribute>
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
  <xsl:template match="MenuItem" mode="adminLink1dd">
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
      <a href="#" title="{Description}" class="nav-link dropdown-toggle" id="@name-dd" role="button" data-bs-toggle="dropdown" aria-expanded="false">
        <xsl:choose>
          <xsl:when test="@cmd=/Page/@ewCmd">
            <xsl:attribute name="class">nav-link dropdown-toggle active</xsl:attribute>
          </xsl:when>
          <xsl:when test="descendant-or-self::MenuItem/@cmd=/Page/@ewCmd and @cmd!='AdmHome'">
            <xsl:attribute name="class">nav-link dropdown-toggle on</xsl:attribute>
          </xsl:when>
        </xsl:choose>
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
      <a href="{$href}" title="{Description}" class="dropdown-item">
        <xsl:choose>
          <xsl:when test="self::MenuItem[@cmd=/Page/@ewCmd]">
            <xsl:attribute name="class">dropdown-item active</xsl:attribute>
          </xsl:when>
          <xsl:when test="descendant::MenuItem[@cmd=/Page/@ewCmd] and @cmd!='AdmHome'">
            <xsl:attribute name="class">dropdown-item on</xsl:attribute>
          </xsl:when>
        </xsl:choose>
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
      <a href="{$href}" title="{Description}" class="dropdown-item">
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
      <a href="{$href}" title="{Description}" class="btn btn-lg btn-primary">
        <i class="fa {@icon} fa-large">
          <xsl:text> </xsl:text>
        </i>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@name"/>
      </a>
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
          <a href="{$appPath}?ewCmd=NewPageVersion&amp;pgid={/Page/@id}&amp;vParId={/Page/@id}" title="{Description}" class="dropdown-item">
            <xsl:if test="/Page[@ewCmd='NewPageVersion']">
              <xsl:attribute name="class">dropdown-item active on</xsl:attribute>
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

  <!--############### PREVIEW #######################-->
  <!--<xsl:template match="PreviewMenu">
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
          <a href="{$appPath}?ewCmd=Normal&amp;pgid={/Page/@id}" class="btn btn-primary" id="previewBack">
            <i class="fa fa-share-alt fa-white">
              <xsl:text> </xsl:text>
            </i><xsl:text> </xsl:text>Back to Admin
          </a>
        </div>
        <div class="terminus"> </div>
      </div>
    </div>
  </xsl:template>-->

  <xsl:template match="PreviewMenu">
    <xsl:variable name="CMSLogo">
      <xsl:call-template name="eonicwebLogo"/>
    </xsl:variable>
    <xsl:variable name="CMSName">
      <xsl:call-template name="eonicwebProductName"/>
    </xsl:variable>
    <div class="ewAdmin">
      <div id="adminHeader" class="affix-top">
        <div id="adminLogo">
          <img src="{$CMSLogo}" alt="{$CMSName}" class="cms-logo"/>
        </div>
        <form class="ewXform" id="previewSettings" action="?ewCmd=PreviewOn">
          <div id="breadcrumb">
            <div class="preview-name">
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

              <label for="PreviewDate"> as of date&#160;</label>&#160;
            </div>
            <div class="input-group">
              <input type="date" class="form-control" name="dPreviewDate" id="dPreviewDate" value="{/Page/@pageViewDate}" onChange="document.getElementById('previewSettings').submit();">
              </input>
            </div>

            &#160;
            <xsl:choose>
              <xsl:when test="/Page/@previewHidden='on'">
                <a href="?ewcmd=PreviewOn&amp;ewCmd2=hideHidden" class="btn btn-default">
                  <i class="fas fa-eye-slash"> </i> Hide Hidden
                </a>
              </xsl:when>
              <xsl:otherwise>
                <a href="?ewcmd=PreviewOn&amp;ewCmd2=showHidden" class="btn btn-default">
                  <i class="fas fa-eye"> </i> Show Hidden
                </a>
              </xsl:otherwise>
            </xsl:choose>
          </div>
        </form>
        <xsl:text> </xsl:text>
        <xsl:text> </xsl:text>
        <a href="?ewCmd=Normal&amp;pgid={/Page/@id}" class="btn btn-default" id="previewBack">
          <i class="fas fa-times"> </i> Exit Preview<xsl:text> </xsl:text>
        </a>
      </div>
    </div>
  </xsl:template>

</xsl:stylesheet>

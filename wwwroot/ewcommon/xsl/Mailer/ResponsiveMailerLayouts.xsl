<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <xsl:variable name="siteURL">
    <xsl:variable name="serverVariableURL">
      <xsl:call-template name="getServerVariable">
        <xsl:with-param name="valueName" select="'HTTP_HOST'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="https">
      <xsl:call-template name="getServerVariable">
        <xsl:with-param name="valueName" select="'HTTPS'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="not($https='on')">
        <xsl:text>http://</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>https://</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:value-of select="$serverVariableURL"/>
  </xsl:variable>



  <xsl:variable name="mailer_utm_campaign">
    <xsl:call-template name="mailer_utm_campaign"/>
  </xsl:variable>

  <xsl:template name="mailer_utm_campaign">
    <xsl:if test="not($adminMode)">
      <xsl:text>utm_medium=email</xsl:text>
      <xsl:text>&amp;utm_source=mailingList</xsl:text>
      <xsl:text>&amp;utm_campaign=</xsl:text>
      <xsl:value-of select="translate(/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]/@name,' ','-')"/>
      <xsl:text>&amp;email=[email]</xsl:text>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Page[@adminMode='false']" mode="adminStyle">
    <link type="text/css" rel="stylesheet" href="/ewcommon/css/admin/admin.less"/>
    <!-- IF IE6 BRING IN IE6 files -->
    <xsl:if test="contains(/Page/Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 6.0') and not(contains(Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'MSIE 7')) and not(contains(Request/ServerVariables/Item[@name='HTTP_USER_AGENT'], 'Opera'))">
      <link type="text/css" rel="stylesheet" href="/ewcommon/css/admin/skins/ie6.css"/>
    </xsl:if>
  </xsl:template>

  <!--####################### Page Level Templates, can be overridden later. ##############################-->
  <xsl:template match="Page">
    <html>
      <xsl:choose>
        <xsl:when test="/Page/Contents/Content[@name='XmlLang']">
          <xsl:attribute name="xml:lang">
            <xsl:value-of select="Contents/Content[@name='XmlLang']"/>
          </xsl:attribute>
          <xsl:attribute name="lang">
            <xsl:value-of select="Contents/Content[@name='XmlLang']"/>
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="xml:lang">
            <xsl:value-of select="@lang"/>
          </xsl:attribute>
          <xsl:attribute name="lang">
            <xsl:value-of select="@lang"/>
          </xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
      <head>
        <title>
          <xsl:apply-templates select="/" mode="PageTitle"/>
          <xsl:text> </xsl:text>
        </title>
        <xsl:if test="/Page/@baseUrl">
          <base href="{/Page/@baseUrl}"/>
        </xsl:if>
        <xsl:if test="/Page/@adminMode">
          <xsl:apply-templates select="." mode="commonStyle"/>
          <xsl:apply-templates select="." mode="js"/>
        </xsl:if>
        <xsl:apply-templates select="." mode="emailStyle"/>
      </head>
      <xsl:apply-templates select="." mode="bodyBuilder"/>
    </html>
  </xsl:template>

  <xsl:template match="Page" mode="commonStyle">
    <link rel="stylesheet" type="text/css" href="/ewcommon/css/base-bs.less"/>
    <xsl:apply-templates select="." mode="adminStyle"/>
    <link rel="stylesheet" type="text/css" href="/ewcommon/css/admin/adminmailer.css"/>
    <xsl:apply-templates select="." mode="siteStyle"/>
  </xsl:template>

  <xsl:template match="Page" mode="siteStyle">
    <!-- Placeholder -->
  </xsl:template>

  <xsl:template match="Page" mode="emailStyle">
    <!-- Placeholder -->
  </xsl:template>

  <!-- -->
  <!--   ################################################  Layout Types are specified in the LayoutsManifest.XML file  #######################   -->

  <xsl:template match="Page" mode="mainLayout">
    <xsl:apply-templates select="." mode="Layout"/>
  </xsl:template>

  <!-- Generic Templates for displaying HTML content instead of using copy-of which introduces xmlns errors -->

  <xsl:template match="Content" mode="displayBrief">
    <xsl:apply-templates select="node()" mode="cleanXhtml"/>
  </xsl:template>

  <!-- Generic Template for displaying Flash Movie -->
  <xsl:template match="Content[@type='FlashMovie']" mode="displayBrief">
    <span class="alert">
      <h3>Flash is not supported in emails</h3>
    </span>
  </xsl:template>
  <!--  -->
  <!--   ################################################   Layout Headers   ##############################################   -->
  <!-- -->
  <xsl:template match="/" mode="layoutHeader">
    <xsl:param name="colspan"/>
    <xsl:if test="/Page/Contents/Content[@position='header'] or /Page/@adminMode">
      <tr>
        <td style="width:100%;vertical-align:top;" valign="top" class="moduleContainer" id="header">
          <xsl:if test="$colspan!=''">
            <xsl:attribute name="colspan">
              <xsl:value-of select="$colspan"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:apply-templates select="/Page"  mode="addMailModule">
            <xsl:with-param name="text">Add Module</xsl:with-param>
            <xsl:with-param name="position">header</xsl:with-param>
          </xsl:apply-templates>
        </td>
      </tr>
    </xsl:if>
  </xsl:template>
  <!-- -->
  <xsl:template match="/" mode="layoutFooter">
    <xsl:param name="colspan"/>
    <xsl:if test="/Page/Contents/Content[@position='footer'] or /Page/@adminMode">
      <tr>
        <td style="width:100%;vertical-align:top;" valign="top" class="moduleContainer" id="footer">
          <xsl:if test="$colspan!=''">
            <xsl:attribute name="colspan">
              <xsl:value-of select="$colspan"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:apply-templates select="/Page"  mode="addMailModule">
            <xsl:with-param name="text">Add Module</xsl:with-param>
            <xsl:with-param name="position">footer</xsl:with-param>
          </xsl:apply-templates>
        </td>
      </tr>
    </xsl:if>
  </xsl:template>

  <!-- ## Default Layout  ############################################################################   -->
  <xsl:template match="Page" mode="Layout">
    <div class="template" id="template_1_Column">
      <xsl:apply-templates select="/" mode="layoutHeader"/>
      <div class="content">
        <xsl:choose>
          <xsl:when test="@layout=''">
            <xsl:call-template name="term2000" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="term2001" />
            <xsl:text>&#160;</xsl:text>
            <strong>
              <xsl:value-of select="@layout"/>
            </strong>
            <xsl:text>&#160;</xsl:text>
            <xsl:call-template name="term2002" />
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text> </xsl:text>
      </div>
      <xsl:apply-templates select="/" mode="layoutFooter"/>
    </div>
  </xsl:template>
  <!-- -->

  <!-- ## Error Layout   #############################################################################   -->
  <xsl:template match="Page[@layout='Error']" mode="Layout">
    <xsl:choose>
      <xsl:when test="/Page/Contents/Content[@name='1005']">
        <xsl:apply-templates select="/Page/Contents/Content[@name='1005']" mode="displayBrief"/>
      </xsl:when>
      <xsl:when test="Contents/Content">
        <xsl:apply-templates select="Contents/Content" mode="displayBrief"/>
      </xsl:when>
      <xsl:otherwise>
        <div class="template" id="Error" >
          <xsl:call-template name="term2003" />
        </div>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- -->

  <!-- ##  Layouts  ##############################################   -->
  <xsl:template match="Page[@layout='Modules_1_Column']" mode="Layout">
    <table cellpadding="0" cellspacing="0" border="0" width="100%" style="width:100% !important" class="template" id="template_2_Columns">
      <xsl:apply-templates select="/" mode="layoutHeader">
        <xsl:with-param name="colspan" select="'1'"/>
      </xsl:apply-templates>
      <tr>
        <td class="moduleContainer" id="column1">
          <xsl:apply-templates select="/Page"  mode="addMailModule">
            <xsl:with-param name="text">Add Module</xsl:with-param>
            <xsl:with-param name="position">column1</xsl:with-param>
          </xsl:apply-templates>
        </td>
      </tr>
      <xsl:apply-templates select="/" mode="layoutFooter">
        <xsl:with-param name="colspan" select="'1'"/>
      </xsl:apply-templates>
    </table>
  </xsl:template>

  <xsl:template match="Page[@layout='Modules_2_Columns']" mode="Layout">
    <table cellpadding="0" cellspacing="0" border="0" width="100%" style="width:100% !important" class="template" id="template_2_Columns">
      <xsl:apply-templates select="/" mode="layoutHeader">
        <xsl:with-param name="colspan" select="'2'"/>
      </xsl:apply-templates>
      <tr>
        <td style="width:50%;vertical-align:top;" valign="top" class="moduleContainer emailCol" id="column1">
          <xsl:apply-templates select="/Page"  mode="addMailModule">
            <xsl:with-param name="text">Add Module</xsl:with-param>
            <xsl:with-param name="position">column1</xsl:with-param>
          </xsl:apply-templates>
        </td>
        <td style="width:50%;vertical-align:top;" valign="top" class="moduleContainer emailCol" id="column2">
          <xsl:apply-templates select="/Page"  mode="addMailModule">
            <xsl:with-param name="text">Add Module</xsl:with-param>
            <xsl:with-param name="position">column2</xsl:with-param>
          </xsl:apply-templates>
        </td>
      </tr>
      <xsl:apply-templates select="/" mode="layoutFooter">
        <xsl:with-param name="colspan" select="'2'"/>
      </xsl:apply-templates>
    </table>
  </xsl:template>

  <xsl:template match="Page[@layout='Modules_2_Columns_66_33']" mode="Layout">
    <table cellpadding="0" cellspacing="0" border="0" width="100%">
      <xsl:apply-templates select="/" mode="layoutHeader">
        <xsl:with-param name="colspan" select="'2'"/>
      </xsl:apply-templates>
      <tr>
        <td id="column1" style="width:66%;vertical-align:top;" valign="top" class="moduleContainer emailCol">
          <xsl:apply-templates select="/Page"  mode="addMailModule">
            <xsl:with-param name="text">Add Module</xsl:with-param>
            <xsl:with-param name="position">column1</xsl:with-param>
          </xsl:apply-templates>
        </td>
        <td id="column2" style="width:34%;vertical-align:top;" valign="top" class="moduleContainer emailCol">
          <xsl:apply-templates select="/Page"  mode="addMailModule">
            <xsl:with-param name="text">Add Module</xsl:with-param>
            <xsl:with-param name="position">column2</xsl:with-param>
          </xsl:apply-templates>
        </td>
      </tr>
      <xsl:apply-templates select="/" mode="layoutFooter">
        <xsl:with-param name="colspan" select="'2'"/>
      </xsl:apply-templates>
    </table>
  </xsl:template>

  <xsl:template match="Page[@layout='Modules_2_Columns_33_66']" mode="Layout">
    <table cellpadding="0" cellspacing="0" border="0" width="100%">
      <xsl:apply-templates select="/" mode="layoutHeader">
        <xsl:with-param name="colspan" select="'2'"/>
      </xsl:apply-templates>
      <tr>
        <td id="column1" style="width:34%;vertical-align:top;" valign="top" class="moduleContainer emailCol">
          <xsl:apply-templates select="/Page"  mode="addMailModule">
            <xsl:with-param name="text">Add Module</xsl:with-param>
            <xsl:with-param name="position">column1</xsl:with-param>
          </xsl:apply-templates>
        </td>
        <td id="column2" style="width:66%;vertical-align:top;" valign="top" class="moduleContainer emailCol">
          <xsl:apply-templates select="/Page"  mode="addMailModule">
            <xsl:with-param name="text">Add Module</xsl:with-param>
            <xsl:with-param name="position">column2</xsl:with-param>
          </xsl:apply-templates>
        </td>
      </tr>
      <xsl:apply-templates select="/" mode="layoutFooter">
        <xsl:with-param name="colspan" select="'2'"/>
      </xsl:apply-templates>
    </table>
  </xsl:template>


  <xsl:template match="Page[@layout='Modules_3_Columns']" mode="Layout">
    <table cellpadding="0" cellspacing="0" border="0" width="100%">
      <xsl:apply-templates select="/" mode="layoutHeader">
        <xsl:with-param name="colspan" select="'3'"/>
      </xsl:apply-templates>
      <tr>
        <td style="width:33%;vertical-align:top;" valign="top" class="moduleContainer emailCol" id="column1">
          <xsl:apply-templates select="/Page"  mode="addMailModule">
            <xsl:with-param name="text">Add Module</xsl:with-param>
            <xsl:with-param name="position">column1</xsl:with-param>
          </xsl:apply-templates>
        </td>
        <td style="width:33%;vertical-align:top;" valign="top" class="moduleContainer emailCol" id="column2">
          <xsl:apply-templates select="/Page"  mode="addMailModule">
            <xsl:with-param name="text">Add Module</xsl:with-param>
            <xsl:with-param name="position">column2</xsl:with-param>
          </xsl:apply-templates>
        </td>
        <td style="width:33%;vertical-align:top;" valign="top" class="moduleContainer emailCol" id="column3">
          <xsl:apply-templates select="/Page"  mode="addMailModule">
            <xsl:with-param name="text">Add Module</xsl:with-param>
            <xsl:with-param name="position">column3</xsl:with-param>
          </xsl:apply-templates>
        </td>
      </tr>
      <xsl:apply-templates select="/" mode="layoutFooter">
        <xsl:with-param name="colspan" select="'2'"/>
      </xsl:apply-templates>
    </table>
  </xsl:template>

  <!--   #######################   Module Handlers   #######################   -->

  <xsl:template match="Content[@type='Module']" mode="displayModule">
    <div class="module" id="mod_{@id}">
      <xsl:choose>
        <xsl:when test="@box!='false' and @box!=''">
          <xsl:apply-templates select="." mode="moduleBox"/>
        </xsl:when>
        <xsl:otherwise>
          <div>
            <xsl:apply-templates select="." mode="inlinePopupOptions" />
          </div>
          <table border="0" cellpadding="0" cellspacing="0" width="100%" style="width:100%;padding-bottom:{$boxMargin}px">
            <tr>
              <td width="100%" style="width:100%;">
                <table border="0" cellpadding="0" cellspacing="0" width="100%" style="width:100%;">
                  <xsl:if test="/Page/@adminMode">
                    <tr>
                      <td width="100%" style="width:100%;">
                        <xsl:if test="@rss and @rss!='false'">
                          <xsl:attribute name="colspan">2</xsl:attribute>
                        </xsl:if>
                      </td>
                    </tr>
                  </xsl:if>
                  <xsl:if test="(@rss and @rss!='false') or @title!=''">
                    <tr>
                      <xsl:if test="@title!=''">
                        <td width="100%" style="width:100%;" class="emailModuleHeadingPadding">
                          <h2 class="title">
                            <xsl:apply-templates select="." mode="moduleLink"/>
                          </h2>
                        </td>
                      </xsl:if>
                      <xsl:if test="@rss and @rss!='false'">
                        <td width="20" style="width:20px;">
                          <xsl:apply-templates select="." mode="rssLink" />
                        </td>
                      </xsl:if>
                    </tr>
                  </xsl:if>
                  <tr>
                    <td style="width:100%;" width="100%">
                      <xsl:if test="@rss and @rss!='false'">
                        <xsl:attribute name="colspan">2</xsl:attribute>
                      </xsl:if>
                      <xsl:apply-templates select="." mode="displayBrief"/>
                    </td>
                  </tr>
                  <xsl:if test="@linkText!='' and @link!=''">
                    <tr>
                      <td width="100%" style="width:100%;" align="right">
                        <xsl:if test="@rss and @rss!='false'">
                          <xsl:attribute name="colspan">2</xsl:attribute>
                        </xsl:if>
                        <xsl:apply-templates select="." mode="moreLinkEmail">
                          <xsl:with-param name="link">
                            <xsl:choose>
                              <xsl:when test="format-number(@link,'0')!='NaN'">
                                <xsl:variable name="pageId" select="@link"/>
                                <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]" mode="getHref"/>
                              </xsl:when>
                              <xsl:otherwise>
                                <xsl:value-of select="@link"/>
                              </xsl:otherwise>
                            </xsl:choose>
                          </xsl:with-param>
                          <xsl:with-param name="linkText" select="@linkText"/>
                          <xsl:with-param name="altText" select="@title"/>
                        </xsl:apply-templates>
                      </td>
                    </tr>
                  </xsl:if>
                </table>
              </td>
            </tr>
          </table>
        </xsl:otherwise>
      </xsl:choose>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='FormattedText' or @moduleType='Image']" mode="displayModule">
    <div class="module" id="mod_{@id}">
      <xsl:choose>
        <xsl:when test="@box!='false' and @box!=''">
          <xsl:apply-templates select="." mode="moduleBox"/>
        </xsl:when>
        <xsl:otherwise>
          <div>
            <xsl:apply-templates select="." mode="inlinePopupOptions" />
          </div>
          <table border="0" cellpadding="0" cellspacing="0" width="100%" style="width:100%;">
            <tr>
              <td width="100%" style="width:100%;" class="emailModulePadding">
                <table border="0" cellpadding="0" cellspacing="0" width="100%" style="width:100%;">
                  <xsl:if test="/Page/@adminMode">
                    <tr>
                      <td width="100%" style="width:100%;">
                        <xsl:if test="@rss and @rss!='false'">
                          <xsl:attribute name="colspan">2</xsl:attribute>
                        </xsl:if>
                      </td>
                    </tr>
                  </xsl:if>
                  <xsl:if test="(@rss and @rss!='false') or @title!=''">
                    <tr>
                      <xsl:if test="@title!=''">
                        <td width="100%" style="width:100%;">
                          <h2 class="title">
                            <xsl:apply-templates select="." mode="moduleLink" />
                          </h2>
                        </td>
                      </xsl:if>
                      <xsl:if test="@rss and @rss!='false'">
                        <td width="20" style="width:20px;">
                          <xsl:apply-templates select="." mode="rssLink" />
                        </td>
                      </xsl:if>
                    </tr>
                  </xsl:if>
                  <tr>
                    <td style="width:100%;" width="100%">
                      <xsl:if test="@rss and @rss!='false'">
                        <xsl:attribute name="colspan">2</xsl:attribute>
                      </xsl:if>
                      <xsl:apply-templates select="." mode="displayBrief"/>
                    </td>
                  </tr>
                  <xsl:if test="@linkText!='' and @link!=''">
                    <tr>
                      <td width="100%" style="width:100%;" align="right">
                        <xsl:if test="@rss and @rss!='false'">
                          <xsl:attribute name="colspan">2</xsl:attribute>
                        </xsl:if>
                        <xsl:apply-templates select="." mode="moreLinkEmail">
                          <xsl:with-param name="link">
                            <xsl:choose>
                              <xsl:when test="format-number(@link,'0')!='NaN'">
                                <xsl:variable name="pageId" select="@link"/>
                                <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]" mode="getHref"/>
                              </xsl:when>
                              <xsl:otherwise>
                                <xsl:value-of select="@link"/>
                              </xsl:otherwise>
                            </xsl:choose>
                          </xsl:with-param>
                          <xsl:with-param name="linkText" select="@linkText"/>
                          <xsl:with-param name="altText" select ="@title"/>
                        </xsl:apply-templates>
                      </td>
                    </tr>
                  </xsl:if>
                </table>
              </td>
            </tr>
          </table>
        </xsl:otherwise>
      </xsl:choose>
    </div>
  </xsl:template>


  <xsl:template match="Content" mode="moduleBox">
    <table width="100%" style="width:100%;">
      <!-- define classes for box -->
      <xsl:attribute name="class">
        <xsl:text>box </xsl:text>
        <xsl:value-of select="translate(@box,' ','-')"/>
        <!-- if no title, we may still want TL/TR for rounded boxs with no title bar,
              stled differently to a title bar. -->
        <xsl:if test="@title=''">
          <xsl:text> boxnotitle</xsl:text>
        </xsl:if>
      </xsl:attribute>
      <xsl:if test="/Page/@adminMode">
        <tr>
          <td>
            <xsl:if test="@rss and @rss!='false'">
              <xsl:attribute name="colspan">2</xsl:attribute>
            </xsl:if>
            <div>
              <xsl:apply-templates select="." mode="inlinePopupOptions" />
            </div>
          </td>
        </tr>
      </xsl:if>
      <tr>
        <td width="100%;" style="width:100%;" class="emailModulePadding">
          <table width="100%" style="width:100%;" border="0" cellpadding="0" cellspacing="0">
            <xsl:if test="@title!=''">
              <tr>
                <td class="emailBoxHeader">
                  <h2 class="title">
                    <xsl:apply-templates select="." mode="moduleLink"/>
                  </h2>
                </td>
              </tr>
            </xsl:if>
            <xsl:if test="@rss and @rss!='false'">
              <tr>
                <td width="20" style="width:20px;" >
                  <xsl:apply-templates select="." mode="rssLink" />
                </td>
              </tr>
            </xsl:if>
            <tr>
              <td width="100%" style="width:100%;" class="content emailBoxContent">
                <xsl:if test="@rss and @rss!='false'">
                  <xsl:attribute name="colspan">2</xsl:attribute>
                </xsl:if>
                <xsl:apply-templates select="." mode="displayBrief"/>
              </td>
            </tr>
            <xsl:if test="@linkText!='' and @link!=''">
              <tr>
                <td width="100%" style="width:100%;" class="emailBoxFooter">
                  <xsl:if test="@rss and @rss!='false'">
                    <xsl:attribute name="colspan">2</xsl:attribute>
                  </xsl:if>
                  <xsl:apply-templates select="." mode="moreLinkEmail">
                    <xsl:with-param name="link">
                      <xsl:choose>
                        <xsl:when test="format-number(@link,'0')!='NaN'">
                          <xsl:variable name="pageId" select="@link"/>
                          <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]" mode="getHref"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <xsl:value-of select="@link"/>
                        </xsl:otherwise>
                      </xsl:choose>
                    </xsl:with-param>
                    <xsl:with-param name="linkText" select="@linkText"/>
                    <xsl:with-param name="altText" select="@title"/>
                  </xsl:apply-templates>
                </td>
              </tr>
            </xsl:if>
          </table>
        </td>
      </tr>
    </table>
  </xsl:template>

  <xsl:template match="Content[@type='Module']" mode="moduleTitle">
    <xsl:value-of select="@title"/>
  </xsl:template>

  <!-- ## Generic displayBrief   #####################################################################   -->
  <xsl:template match="Content" mode="displayBrief">
    <xsl:apply-templates select="node()" mode="cleanXhtml"/>
  </xsl:template>

  <!-- ## Generic Module displayBrief   #####################################################################   -->
  <xsl:template match="Content[@type='Module']" mode="displayBrief">
    <span class="alert">* Module type unknown *</span>
  </xsl:template>

  <!-- ## Generic displayBrief for Formatted Text and Images   #####################################################################   -->
  <xsl:template match="Content[@moduleType='FormattedText']" mode="displayBrief">
    <xsl:apply-templates select="node()" mode="cleanXhtml"/>
  </xsl:template>
  <xsl:template match="Content[@moduleType='Image']" mode="displayBrief">
    <xsl:choose>
      <xsl:when test="@resize='true'">
        <xsl:apply-templates select="." mode="resize-image"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="node()" mode="cleanXhtml"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ## IMAGE BASIC CONTENT TYPE - Cater for a link  ###############################################   -->
  <xsl:template match="Content[@type='Image']" mode="displayBrief">
    <xsl:choose>
      <xsl:when test="@internalLink!=''">
        <xsl:variable name="pageId" select="@internalLink"/>
        <xsl:variable name="href">
          <xsl:apply-templates select="//MenuItem[@id=$pageId]" mode="getHref" />
        </xsl:variable>
        <xsl:variable name="pageName">
          <xsl:apply-templates select="//MenuItem[@id=$pageId]" mode="getDisplayName" />
        </xsl:variable>
        <a href="{$href}?{$mailer_utm_campaign}" title="{$pageName}">
          <xsl:apply-templates select="./node()" mode="cleanXhtml"/>
        </a>
      </xsl:when>
      <xsl:when test="@externalLink!=''">
        <a href="{@externalLink}?{$mailer_utm_campaign}" title="{@name}">
          <xsl:if test="not(contains(@externalLink,/Page/Request/ServerVariables/Item[@name='SERVER_NAME']/node()))">
            <xsl:attribute name="target">_blank</xsl:attribute>
          </xsl:if>
          <xsl:apply-templates select="./node()" mode="cleanXhtml"/>
        </a>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="./node()" mode="cleanXhtml"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- Image Module Display with Link -->
  <xsl:template match="Content[@type='Module' and @moduleType='Image' and @link!='']" mode="displayBrief">
    <a>
      <xsl:attribute name="href">
        <xsl:choose>
          <xsl:when test="format-number(@link,'0')!='NaN'">
            <xsl:variable name="pageId" select="@link"/>
            <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]" mode="getHref"/>
            <xsl:text>?</xsl:text>
            <xsl:call-template name="mailer_utm_campaign"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="@link"/>
            <xsl:text>?</xsl:text>
            <xsl:call-template name="mailer_utm_campaign"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <xsl:attribute name="title">
        <xsl:choose>
          <xsl:when test="format-number(@link,'0')!='NaN'">
            <xsl:variable name="pageId" select="@url"/>
            <xsl:value-of select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]/@name"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="@linkText"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
      <xsl:if test="@linkType='external'">
        <xsl:attribute name="rel">external</xsl:attribute>
      </xsl:if>
      <xsl:apply-templates select="node()" mode="cleanXhtml"/>
    </a>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[@type='Module' and @moduleType='FlashMovie']" mode="displayBrief">
    <h1 style="color:#F00;">FLASH IS NOT SUPPORTED IN EMAILS - Please Delete</h1>
  </xsl:template>

  <xsl:template match="Content[@type='Module' and contains(@moduleType,'ListEmail')]" mode="displayBrief">
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
    <table class="ContentList" cellspacing="0" cellpadding="0" width="100%" style="width:100%;">
      <xsl:if test="count(ms:node-set($contentList)/*) &gt; 0">
        <xsl:call-template name="outputContentListTDs">
          <xsl:with-param name="contentList" select="$contentList" />
          <xsl:with-param name="cols" select="@cols" />
          <xsl:with-param name="sortBy" select="@sortBy" />
          <xsl:with-param name="colWidth">
            <xsl:value-of select="floor(100 div @cols)"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:if>
    </table>
  </xsl:template>

  <xsl:template name="outputContentListTDs">
    <xsl:param name="contentList" />
    <xsl:param name="cols" />
    <xsl:param name="sortBy" />
    <xsl:param name="colWidth" />
    <xsl:if test="ms:node-set($contentList)/*">
      <tr>
        <xsl:for-each select="ms:node-set($contentList)/*[position() &lt;= $cols]">
          <xsl:choose>
            <xsl:when test="$cols='1'">
              <td width="{$colWidth}%" style="width:{$colWidth}%;vertical-align:" class="emailModulePadding" valign="top">
                <xsl:apply-templates select="." mode="displayBrief">
                  <xsl:with-param name="sortBy" select="$sortBy"/>
                  <xsl:with-param name="colCount" select="1"/>
                </xsl:apply-templates>
              </td>
            </xsl:when>
            <xsl:otherwise>
              <td width="{$colWidth}%" style="width:{$colWidth}%;vertical-align:top;" class="emailModulePadding emailCol" valign="top">
                <xsl:apply-templates select="." mode="displayBrief">
                  <xsl:with-param name="sortBy" select="$sortBy"/>
                </xsl:apply-templates>
              </td>
            </xsl:otherwise>
          </xsl:choose>
          <!--<td width="{$colWidth}%" style="width:{$colWidth}%;vertical-align:top;padding:0 {$hPadding}px {$boxMargin}px" valign="top" class="emailCol">
            <xsl:apply-templates select="." mode="displayBrief">
              <xsl:with-param name="sortBy" select="$sortBy"/>
            </xsl:apply-templates>
          </td>-->
        </xsl:for-each>
      </tr>
      <xsl:call-template name="outputContentListTDs">
        <xsl:with-param name="colWidth" select="$colWidth"/>
        <xsl:with-param name="cols" select="@cols" />
        <xsl:with-param name="contentList">
          <xsl:for-each select="ms:node-set($contentList)/*[position() &gt; $cols]">
            <xsl:copy-of select="."/>
          </xsl:for-each>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- NewsArticle Brief -->
  <xsl:template match="Content[@type='NewsArticle']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <xsl:param name="colCount"/>
    <!-- articleBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <xsl:apply-templates select="." mode="inlinePopupOptions">
      <xsl:with-param name="class" select="'listItem emailModulePadding'"/>
      <xsl:with-param name="sortBy" select="$sortBy"/>
    </xsl:apply-templates>
    <table cellpadding="0" cellspacing="0" width="100%" style="width:100%;" class="emailNewsList">
      <xsl:if test="Images/img/@src!='' and $colCount!='1'">
        <tr>
          <td class="emailListImage">
            <a href="{$parentURL}?{$mailer_utm_campaign}" title="Read More - {Headline/node()}">
              <xsl:apply-templates select="." mode="displayThumbnail"/>
            </a>
          </td>
        </tr>
      </xsl:if>
      <tr>
        <xsl:if test="Images/img/@src!='' and $colCount='1'">
          <td class="emailListImage1">
            <a href="{$parentURL}?{$mailer_utm_campaign}" title="Read More - {Headline/node()}">
              <xsl:apply-templates select="." mode="displayThumbnail"/>
            </a>
          </td>
        </xsl:if>
        <td class="emailListSummary">
          <h3 class="title">
            <a href="{$parentURL}?{$mailer_utm_campaign}" title="Read More - {Headline/node()}">
              <xsl:value-of select="Headline/node()" />
            </a>
          </h3>
          <xsl:if test="@publish!=''">
            <p class="date">
              <xsl:value-of select="/Page/Contents/Content[@name='articleLabel']"/>
              <xsl:call-template name="DisplayDate">
                <xsl:with-param name="date" select="@publish"/>
              </xsl:call-template>
            </p>
          </xsl:if>
          <xsl:if test="Strapline/node()!=''">
            <p>
              <xsl:call-template name="truncate-string">
                <xsl:with-param name="text" select="Strapline/node()"/>
                <xsl:with-param name="length" select="130"/>
              </xsl:call-template>
              <xsl:text>&#160;</xsl:text>
            </p>
          </xsl:if>
          <table cellpadding="0" cellspacing="0" class="emailBtnTable">
            <tr>
              <td class="emailBtn">
                <a>
                  <xsl:attribute name="href">
                    <xsl:apply-templates select="." mode="getHref" />
                    <xsl:text>?</xsl:text>
                    <xsl:call-template name="mailer_utm_campaign"/>
                  </xsl:attribute>
                  <xsl:attribute name="title">
                    <xsl:text>view </xsl:text>
                    <xsl:apply-templates select="." mode="getDisplayName" />
                  </xsl:attribute>
                  <xsl:text>Read more</xsl:text>
                </a>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </xsl:template>

  <!-- -->
  <!-- Event Brief -->
  <xsl:template match="Content[@type='Event']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <xsl:param name="colCount"/>
    <!-- articleBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <xsl:apply-templates select="." mode="inlinePopupOptions">
      <xsl:with-param name="class" select="'listItem vevent'"/>
      <xsl:with-param name="sortBy" select="$sortBy"/>
    </xsl:apply-templates>
    <table cellpadding="0" cellspacing="0" width="100%" style="width:100%;" class="emailEventList">
      <xsl:if test="Images/img/@src!='' and $colCount!='1'">
        <tr>
          <td class="emailListImage">
            <a href="{$parentURL}?{$mailer_utm_campaign}" title="Read More - {Headline/node()}">
              <xsl:apply-templates select="." mode="displayThumbnail"/>
            </a>
          </td>
        </tr>
      </xsl:if>
      <tr>
        <xsl:if test="Images/img/@src!='' and $colCount='1'">
          <td class="emailListImage1">
            <a href="{$parentURL}?{$mailer_utm_campaign}" title="Read More - {Headline/node()}">
              <xsl:apply-templates select="." mode="displayThumbnail"/>
            </a>
          </td>
        </xsl:if>
        <td class="emailListSummary">
          <h3 class="title">
            <a href="{$parentURL}?{$mailer_utm_campaign}" title="Read More - {Headline/node()}" class="url summary">
              <xsl:value-of select="Headline/node()"/>
            </a>
          </h3>
          <xsl:if test="StartDate/node()!='' or Location/Venue!=''">
            <p class="eventDetails">
              <xsl:if test="StartDate/node()!=''">
                <span class="date">
                  <span class="dtstart">
                    <xsl:call-template name="DisplayDate">
                      <xsl:with-param name="date" select="StartDate/node()"/>
                    </xsl:call-template>
                    <span class="value-title" title="{StartDate/node()}T{translate(Times/@start,',',':')}" ></span>
                  </span>
                  <xsl:if test="EndDate/node()!='' and EndDate/node() != StartDate/node()">
                    <xsl:text> - </xsl:text>
                    <span class="dtend">
                      <xsl:call-template name="DisplayDate">
                        <xsl:with-param name="date" select="EndDate/node()"/>
                      </xsl:call-template>
                      <span class="value-title" title="{EndDate/node()}T{translate(Times/@end,',',':')}"></span>
                    </span>
                  </xsl:if>
                </span>
                <br/>
              </xsl:if>
              <xsl:if test="Location/Venue!=''">
                <span class="location vcard">
                  <span class="fn org">
                    <xsl:value-of select="Location/Venue"/>
                  </span>
                </span>
              </xsl:if>
            </p>
          </xsl:if>
          <p class="summary">
            <xsl:call-template name="truncate-string">
              <xsl:with-param name="text" select="Strap/node()"/>
              <xsl:with-param name="length" select="130"/>
            </xsl:call-template>
            <!--<xsl:apply-templates select="Strap/node()" mode="cleanXhtml"/>-->
            <xsl:text>&#160;</xsl:text>
            <a style="white-space:nowrap;">
              <xsl:attribute name="href">
                <xsl:apply-templates select="." mode="getHref" />
                <xsl:text>?</xsl:text>
                <xsl:call-template name="mailer_utm_campaign"/>
              </xsl:attribute>
              <xsl:attribute name="title">
                <xsl:text>view </xsl:text>
                <xsl:apply-templates select="." mode="getDisplayName" />
              </xsl:attribute>
              <xsl:text>read more</xsl:text>
            </a>
          </p>
        </td>
      </tr>
    </table>
  </xsl:template>

  <!-- Product Brief new -->
  <xsl:template match="Content[@type='Product']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <xsl:param name="pos"/>
    <xsl:param name="colCount"/>
    <xsl:variable name="parId">
      <xsl:choose>
        <xsl:when test="@parId &gt; 0">
          <xsl:value-of select="@parId"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="/Page/@id"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="self::Content" mode="getHref">
        <xsl:with-param name="parId" select="$parId"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:apply-templates select="." mode="inlinePopupOptions">
      <xsl:with-param name="class" select="'listItem hproduct emailModulePadding'"/>
      <xsl:with-param name="sortBy" select="$sortBy"/>
    </xsl:apply-templates>
    <table cellpadding="0" cellspacing="0" width="100%" style="width:100%;" class="emailProductList">
      <xsl:if test="Images/img/@src!='' and $colCount!='1'">
        <tr>
          <td class="emailListImage">
            <a href="{$parentURL}?{$mailer_utm_campaign}" class="url">
              <xsl:apply-templates select="." mode="displayThumbnail"/>
            </a>
          </td>
        </tr>
      </xsl:if>
      <tr>
        <xsl:if test="Images/img/@src!='' and $colCount='1'">
          <td class="emailListImage1">
            <a href="{$parentURL}?{$mailer_utm_campaign}" class="url">
              <xsl:apply-templates select="." mode="displayThumbnail"/>
            </a>
          </td>
        </xsl:if>
        <td class="emailListSummary">
          <h3 class="fn title">
            <a href="{$parentURL}?{$mailer_utm_campaign}" title="{@name}">
              <xsl:apply-templates select="Name" mode="cleanXhtml"/>
            </a>
          </h3>
          <xsl:if test="Manufacturer/node()!=''">
            <b>
              <xsl:call-template name="term2081" />
              <xsl:text>: </xsl:text>
            </b>
            <xsl:apply-templates select="Manufacturer" mode="cleanXhtml"/>
            <!--</td>-->
          </xsl:if>
          <br/>
          <xsl:choose>
            <xsl:when test="Content[@type='SKU']">
              <xsl:apply-templates select="Content[@type='SKU'][1]" mode="displayPrice" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:apply-templates select="." mode="displayPrice" />
            </xsl:otherwise>
          </xsl:choose>
          <!--ShortDescription-->
          <xsl:if test="ShortDescription/node()!=''">
            <xsl:apply-templates select="ShortDescription/node()" mode="cleanXhtml"/>
          </xsl:if>
          <!--read more-->

          <br/>
          <br/>
          <table cellpadding="0" cellspacing="0">
            <tr>
              <td class="emailBtn">
                <a style="">
                  <xsl:attribute name="href">
                    <xsl:apply-templates select="." mode="getHref" />
                    <xsl:text>?</xsl:text>
                    <xsl:call-template name="mailer_utm_campaign"/>
                  </xsl:attribute>
                  <xsl:attribute name="title">
                    <xsl:text>view </xsl:text>
                    <xsl:apply-templates select="." mode="getDisplayName" />
                  </xsl:attribute>
                  <xsl:text>Read more</xsl:text>
                </a>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </xsl:template>

  <!-- Document Brief -->
  <xsl:template match="Content[@type='Document']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <!-- documentBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="self::Content" mode="getHref">
        <xsl:with-param name="parId" select="@parId"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="preURL" select="substring(Website,1,3)" />
    <xsl:variable name="linkURL">
      <xsl:choose>
        <xsl:when test="$preURL='www' or $preURL='WWW'">
          <xsl:text>http://</xsl:text>
          <xsl:value-of select="Url"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="Url"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:apply-templates select="." mode="inlinePopupOptions">
      <xsl:with-param name="class" select="'list document'"/>
      <xsl:with-param name="sortBy" select="$sortBy"/>
    </xsl:apply-templates>
    <h3 class="title">
      <a href="/ewcommon/tools/download.ashx?docId={@id}" rel="external">
        <xsl:attribute name="title">
          <!-- click here to download a copy of this document -->
          <xsl:apply-templates select="." mode="getDisplayName"/>
        </xsl:attribute>
        <xsl:value-of select="Title/node()"/>
      </a>
    </h3>
    <xsl:if test="Body/node()!=''">
      <xsl:call-template name="truncate-string">
        <xsl:with-param name="text" select="Body/node()"/>
        <xsl:with-param name="length" select="130"/>
      </xsl:call-template>
      <xsl:text>&#160;</xsl:text>
    </xsl:if>
    <p class="link">
      <a href="/ewcommon/tools/download.ashx?docId={@id}" rel="external" class="{substring-after(Path,'.')}icon">
        <xsl:attribute name="title">
          <!-- click here to download a copy of this document -->
          <xsl:apply-templates select="." mode="getDisplayName"/>
        </xsl:attribute>
        <!--Download-->
        <xsl:call-template name="term2028" />
        <xsl:text>&#160;</xsl:text>
        <xsl:choose>
          <xsl:when test="contains(Path,'.pdf')">
            <!--Adobe PDF-->
            <xsl:call-template name="term2029" />
          </xsl:when>
          <xsl:when test="contains(Path,'.doc')">
            <!--Word Document-->
            <xsl:call-template name="term2030" />
          </xsl:when>
          <xsl:when test="contains(Path,'.xls')">
            <!--Excel Spreadsheet-->
            <xsl:call-template name="term2031" />
          </xsl:when>
          <xsl:when test="contains(Path,'.zip')">
            <!--Zip Archive-->
            <xsl:call-template name="term2032" />
          </xsl:when>
          <xsl:when test="contains(Path,'.ppt')">
            <!--PowerPoint Presentation-->
            <xsl:call-template name="term2033" />
          </xsl:when>
          <xsl:when test="contains(Path,'.zip')">
            <!--PowerPoint Slideshow-->
            <xsl:call-template name="term2034" />
          </xsl:when>
          <xsl:when test="contains(Path,'.jpg')">
            <!--JPEG Image file-->
            <xsl:call-template name="term2035" />
          </xsl:when>
          <xsl:when test="contains(Path,'.gif')">
            <!--GIF Image file-->
            <xsl:call-template name="term2036" />
          </xsl:when>
          <xsl:when test="contains(Path,'.png')">
            <!--PNG Image file-->
            <xsl:call-template name="term2037" />
          </xsl:when>
          <xsl:otherwise>
            <!--Unknown File Type-->
            <xsl:call-template name="term2038" />
          </xsl:otherwise>
        </xsl:choose>
      </a>
    </p>
  </xsl:template>


  <!-- Job Brief -->
  <xsl:template match="Content[@type='Job']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <!-- vacancyBrief -->
    <xsl:variable name="parId">
      <xsl:choose>
        <xsl:when test="@parId &gt; 0">
          <xsl:value-of select="@parId"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="/Page/@id"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="self::Content" mode="getHref">
        <xsl:with-param name="parId" select="$parId"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:apply-templates select="." mode="inlinePopupOptions"/>
    <table cellpadding="0" cellspacing="0" width="100%" style="width:100%;" class="emailJobList">
      <tr>
        <td>
          <h3 class="title">
            <a href="{$parentURL}?{$mailer_utm_campaign}" title="{JobTitle/node()}">
              <xsl:value-of select="JobTitle/node()"/>
            </a>
          </h3>
          <xsl:if test="Summary/node()!=''">
            <xsl:call-template name="truncate-string">
              <xsl:with-param name="text" select="Summary/node()"/>
              <xsl:with-param name="length" select="300"/>
            </xsl:call-template>
            <xsl:text>&#160;</xsl:text>
            <br/>
            <br/>
          </xsl:if>
          <b>
            <xsl:call-template name="term2062" />
            <xsl:text>: </xsl:text>
          </b>
          <xsl:call-template name="DisplayDate">
            <xsl:with-param name="date" select="@publish"/>
          </xsl:call-template>
          <br/>
          <xsl:if test="ContractType/node()!=''">
            <!--Contract Type-->
            <b>
              <xsl:call-template name="term2063" />
              <xsl:text>: </xsl:text>
            </b>
            <xsl:value-of select="ContractType/node()"/>
            <br/>
          </xsl:if>
          <xsl:if test="Ref/node()!=''">
            <!--Ref-->
            <b>
              <xsl:call-template name="term2064" />
              <xsl:text>: </xsl:text>
            </b>
            <xsl:value-of select="Ref/node()"/>
            <br/>
          </xsl:if>
          <xsl:if test="Location/node()!=''">
            <!--Location-->
            <b>
              <xsl:call-template name="term2065" />
              <xsl:text>: </xsl:text>
            </b>
            <xsl:value-of select="Location/node()"/>
            <br/>
          </xsl:if>
          <xsl:if test="Salary/node()!=''">
            <!--Salary-->
            <b>
              <xsl:call-template name="term2066" />
              <xsl:text>: </xsl:text>
            </b>
            <xsl:value-of select="Salary/node()"/>
            <br/>
          </xsl:if>
          <xsl:if test="ApplyBy/node()!=''">
            <!--Deadline for applications-->
            <b>
              <xsl:call-template name="term2067" />
              <xsl:text>: </xsl:text>
            </b>
            <xsl:call-template name="DisplayDate">
              <xsl:with-param name="date" select="ApplyBy/node()"/>
            </xsl:call-template>
            <br/>
          </xsl:if>
          <br/>
          <table cellpadding="0" cellspacing="0" >
            <tr>
              <td class="emailBtn">
                <a>
                  <xsl:attribute name="href">
                    <xsl:apply-templates select="." mode="getHref" />
                    <xsl:text>?</xsl:text>
                    <xsl:call-template name="mailer_utm_campaign"/>
                  </xsl:attribute>
                  <xsl:attribute name="title">
                    <xsl:text>view </xsl:text>
                    <xsl:apply-templates select="." mode="getDisplayName" />
                  </xsl:attribute>
                  <xsl:text>Read more</xsl:text>
                </a>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </xsl:template>

  <!--*********************************** Placeholder for Xform Editing when no Admin XSL used. ************-->

  <xsl:template match="/" mode="optOutStatement">
    <xsl:param name="style"/>
    <xsl:choose>
      <xsl:when test="/Page/@adminMode">
        <div>
          <xsl:if test="$style!=''">
            <xsl:attribute name="style">
              <xsl:value-of select="$style"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
            <xsl:with-param name="type">FormattedText</xsl:with-param>
            <xsl:with-param name="text">Add Opt Out Statement</xsl:with-param>
            <xsl:with-param name="name">mailerOptOutStatement</xsl:with-param>
          </xsl:apply-templates>
          <xsl:apply-templates select="/Page/Contents/Content[@name='mailerOptOutStatement']" mode="displayBrief"/>
          <xsl:if test="not(/Page/Contents/Content[@name='mailerOptOutStatement'])">
            <xsl:apply-templates select="/" mode="optOutDefaultStatement"/>
          </xsl:if>
        </div>
      </xsl:when>

      <xsl:otherwise>
        <span>
          <xsl:if test="$style!=''">
            <xsl:attribute name="style">
              <xsl:value-of select="$style"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:choose>
            <xsl:when test="/Page/Contents/Content[@name='mailerOptOutStatement']">
              <xsl:apply-templates select="/Page/Contents/Content[@name='mailerOptOutStatement']" mode="displayBrief"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:apply-templates select="/" mode="optOutDefaultStatement"/>
            </xsl:otherwise>
          </xsl:choose>
        </span>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- -->

  <xsl:template match="/" mode="optOutDefaultStatement">
    <p>
      <xsl:text>To </xsl:text>
      <strong>UNSUBSCRIBE</strong>
      <xsl:text>, please log onto your Account and adjust your newsletter settings </xsl:text>
      <a title="Go to Account">
        <xsl:attribute name="href">
          <xsl:value-of select="$siteURL"/>
          <xsl:value-of select="/Page/Menu[@id='Site']/MenuItem/descendant-or-self::MenuItem[layout/node()='Logon_Register']/@url"/>
        </xsl:attribute>
        <xsl:text>Click Here.</xsl:text>
      </a>
      <br/>
      <xsl:text>However if you did not choose to receive this email please contact the website administrator or reply to sender.</xsl:text>
    </p>

    <unsubscribe>
      <xsl:text>Unsubscribe</xsl:text>
    </unsubscribe>

  </xsl:template>

  <!-- -->
  <xsl:template name="rssSubscribe">
    <img src="{$siteURL}/ewcommon/images/icons/rss16x16.png" width="16" height="16" alt="RSS" style="border:none;"/>
  </xsl:template>

  <xsl:template name="eonicDeveloperLink">
    <xsl:param name="style"/>
    <xsl:variable name="href">
      <xsl:text>http://www.eonic.co.uk/</xsl:text>
      <xsl:call-template name="mailer_utm_campaign_eonic"/>
    </xsl:variable>
    <a class="devLink" href="{$href}" title="This newsletter was generated by eonicweb4. Click here to see what eonic can offer you." target="new" style="{$style}">
      <xsl:text>generated by eonic</xsl:text>
      <strong>web</strong>
      <xsl:text>4</xsl:text>
    </a>
  </xsl:template>
  <!-- -->
  <xsl:template name="mailer_utm_campaign_eonic">
    <xsl:text>?utm_medium=email</xsl:text>
    <xsl:text>&amp;utm_source=</xsl:text>
    <xsl:value-of select="$siteURL"/>
    <xsl:text>&amp;utm_campaign=mailer</xsl:text>
  </xsl:template>
  <!-- -->

  <!-- -->

  <xsl:template match="Content[@type='Module']" mode="moduleLink">
    <xsl:param name="headingColour"/>
    <xsl:choose>
      <xsl:when test="@link!=''">
        <a>
          <xsl:attribute name="href">
            <xsl:choose>
              <xsl:when test="format-number(@link,'0')!='NaN'">
                <xsl:variable name="pageId" select="@link"/>
                <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]" mode="getHref"/>
                <xsl:text>?</xsl:text>
                <xsl:call-template name="mailer_utm_campaign"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="@link"/>
                <xsl:text>?</xsl:text>
                <xsl:call-template name="mailer_utm_campaign"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <xsl:attribute name="title">
            <xsl:choose>
              <xsl:when test="format-number(@link,'0')!='NaN'">
                <xsl:variable name="pageId" select="@link"/>
                <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]" mode="getDisplayName" />
              </xsl:when>
              <xsl:otherwise>
                <xsl:choose>
                  <xsl:when test="@linkText and @linkText!=''">
                    <xsl:value-of select="@linkText"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:apply-templates select="." mode="moduleTitle"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <xsl:if test="@linkType='external'">
            <xsl:attribute name="rel">external</xsl:attribute>
          </xsl:if>
          <xsl:apply-templates select="." mode="moduleTitle"/>
        </a>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="." mode="moduleTitle"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- -->
  <!--<xsl:template match="*" mode="moreLink">
    <xsl:param name="link"/>
    <xsl:param name="linkText"/>
    <xsl:param name="altText"/>
    <table cellpadding="0" cellspacing="0" >
      <tr>
        <td class="emailBtn">
          <a>
            <xsl:choose>
              <xsl:when test="$linkText!=''">
                <xsl:value-of select="$linkText"/>
              </xsl:when>
              <xsl:otherwise>Read more</xsl:otherwise>
            </xsl:choose>
          </a>
        </td>
      </tr>
    </table>
  </xsl:template>-->


  <!--<xsl:template match="p" mode="cleanXhtml">
    <font face="{$bodyFont}" size="{$bodySize}" color="{$bodyColour}">
      <xsl:element name="{name()}">
        -->
  <!-- process attributes -->
  <!--
        <xsl:for-each select="@*">
          -->
  <!-- remove attribute prefix (if any) -->
  <!--
          <xsl:attribute name="{name()}">
            <xsl:value-of select="." />
          </xsl:attribute>
        </xsl:for-each>
        <xsl:apply-templates mode="cleanXhtml"/>
      </xsl:element>
    </font>
  </xsl:template>

  <xsl:template match="h2" mode="cleanXhtml">
    <h2 face="{$bodyFont}" size="{$bodySize}" color="{$bodyColour}" style ="margin-bottom: 10px; margin-top: 20px;">

      -->
  <!-- process attributes -->
  <!--
      <xsl:for-each select="@*">
        -->
  <!-- remove attribute prefix (if any) -->
  <!--
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="cleanXhtml"/>
    </h2>
  </xsl:template>

  <xsl:template match="a" mode="cleanXhtml">
    <xsl:variable name="href">
      <xsl:choose>
        <xsl:when test="contains(@href,'http://')">
          <xsl:value-of select="@href"/>
        </xsl:when>
        <xsl:when test="contains(@href,'https://')">
          <xsl:value-of select="@href"/>
        </xsl:when>
        <xsl:when test="contains(@href,'mailto:')">
          <xsl:value-of select="@href"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$siteURL"/>
          <xsl:value-of select="@href"/>
          <xsl:choose>
            <xsl:when test="contains(@href,'?')">&amp;</xsl:when>
            <xsl:otherwise>
              <xsl:text>?</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:value-of select="$mailer_utm_campaign"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <a href="{$href}" title="{@title}" style="{@style}">
      <xsl:apply-templates mode="cleanXhtml"/>
    </a>
  </xsl:template>-->

  <xsl:template match="img" mode="cleanXhtml">

    <!-- Stick in Variable and then ms:nodest it 
          - ensures its self closing and we can process all nodes!! -->
    <xsl:variable name="img">
      <xsl:element name="img">
        <xsl:for-each select="@*[name()!='onclick']">
          <xsl:attribute name="{name()}">
            <xsl:choose>

              <!-- ##### @Attribute Conditions ##### -->

              <xsl:when test="name()='src'">

                <xsl:variable name="newSrc">
                  <xsl:call-template name="replace-string">
                    <xsl:with-param name="text" select="." />
                    <xsl:with-param name="replace" select="' '" />
                    <xsl:with-param name="with" select="'%20'" />
                  </xsl:call-template>
                </xsl:variable>

                <xsl:choose>
                  <xsl:when test="contains(.,'http://')">
                    <xsl:value-of select="$newSrc"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$siteURL"/>
                    <xsl:value-of select="$newSrc"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>

              <!-- ##### VALIDATION - Attribute "align" can not be used for this element. ##### -->
              <xsl:when test="name()='class' and (ancestor::img[@align] or contains(ancestor::img/@style,'float: '))">
                <xsl:variable name="align" select="ancestor::img/@align"/>
                <xsl:variable name="float" select="substring-before(substring-after(ancestor::img/@style,'float: '),';')"/>
                <xsl:value-of select="."  />
                <xsl:text> align</xsl:text>
                <xsl:choose>
                  <xsl:when test="@align">
                    <xsl:value-of select="$align"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$float"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="."  />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
        </xsl:for-each>

        <!-- ##### VALIDATION - Attribute "align" can not be used for this element. ##### -->
        <xsl:if test="not(@class) and (@align or contains(@style,'float: '))">
          <xsl:attribute name="class">
            <xsl:variable name="float" select="substring-before(substring-after(@style,'float: '),';')"/>
            <xsl:variable name="align" select="@align"/>
            <xsl:text>align</xsl:text>
            <xsl:choose>
              <xsl:when test="@align">
                <xsl:value-of select="$align"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$float"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
        </xsl:if>

        <!-- ##### VALIDATION - required attribute "alt" ##### -->
        <xsl:if test="not(@alt)">
          <xsl:attribute name="alt"></xsl:attribute>
        </xsl:if>

      </xsl:element>
    </xsl:variable>



    <xsl:copy-of select="ms:node-set($img)"/>
  </xsl:template>

  <xsl:template match="Content | MenuItem" mode="displayThumbnail">
    <xsl:param name="style"/>
    <xsl:param name="align"/>
    <xsl:param name="hspace"/>
    <xsl:param name="vspace"/>
    <!-- IF SO THAT we don't get empty tags if NO IMAGE -->
    <xsl:if test="Images/img[@src and @src!='']">

      <!-- SRC VALUE -->
      <xsl:variable name="src">
        <xsl:choose>
          <!-- IF Thumbnail use that -->
          <xsl:when test="Images/img[@class='thumbnail']/@src!=''">
            <xsl:value-of select="Images/img[@class='thumbnail']/@src"/>
          </xsl:when>
          <!-- IF Full Size use that -->
          <xsl:when test="Images/img[@class='detail']/@src!=''">
            <xsl:value-of select="Images/img[@class='detail']/@src"/>
          </xsl:when>
          <!-- ELSE use display -->
          <xsl:otherwise>
            <xsl:value-of select="Images/img[@class='display']/@src"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <!-- ALT VALUE -->
      <xsl:variable name="alt">
        <xsl:choose>
          <!-- IF Thumbnail use that -->
          <xsl:when test="Images/img[@class='thumbnail']/@alt!=''">
            <xsl:value-of select="Images/img[@class='thumbnail']/@alt"/>
          </xsl:when>
          <!-- IF Full Size use that -->
          <xsl:when test="Images/img[@class='detail']/@alt!=''">
            <xsl:value-of select="Images/img[@class='detail']/@alt"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="@name"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <xsl:variable name="max-width">
        <xsl:apply-templates select="." mode="getThWidth"/>
      </xsl:variable>
      <xsl:variable name="max-height">
        <xsl:apply-templates select="." mode="getThHeight"/>
      </xsl:variable>

      <!-- IF Image to resize -->
      <xsl:if test="$src!=''">
        <xsl:variable name="newSrc">
          <xsl:call-template name="resize-image">
            <xsl:with-param name="path" select="$src"/>
            <xsl:with-param name="max-width" select="$max-width"/>
            <xsl:with-param name="max-height" select="$max-height"/>
            <xsl:with-param name="file-prefix">
              <xsl:text>~th-</xsl:text>
              <xsl:value-of select="$max-width"/>
              <xsl:text>x</xsl:text>
              <xsl:value-of select="$max-height"/>
              <xsl:text>/~th-</xsl:text>
            </xsl:with-param>
            <xsl:with-param name="file-suffix" select="''"/>
            <xsl:with-param name="quality" select="100"/>
          </xsl:call-template>
        </xsl:variable>
        <xsl:variable name="image">
          <img>
            <!-- SRC -->
            <xsl:attribute name="src">
              <xsl:value-of select="$siteURL"/>
              <xsl:value-of select="$newSrc"/>
            </xsl:attribute>
            <!-- Width -->
            <xsl:attribute name="width">
              <xsl:value-of select="ew:ImageWidth($newSrc)" />
            </xsl:attribute>
            <!-- Height -->
            <xsl:attribute name="height">
              <xsl:value-of select="ew:ImageHeight($newSrc)" />
            </xsl:attribute>
            <!-- Height -->
            <xsl:attribute name="alt">
              <xsl:value-of select="$alt" />
            </xsl:attribute>
            <!-- Height -->
            <xsl:attribute name="class">
              <xsl:text>photo thumbnail resized</xsl:text>
            </xsl:attribute>
            <xsl:if test="$style!=''">
              <xsl:attribute name="style">
                <xsl:value-of select="$style" />
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="$align!=''">
              <xsl:attribute name="align">
                <xsl:value-of select="$align" />
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="$hspace!=''">
              <xsl:attribute name="hspace">
                <xsl:value-of select="$hspace" />
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="$vspace!=''">
              <xsl:attribute name="vspace">
                <xsl:value-of select="$vspace" />
              </xsl:attribute>
            </xsl:if>
          </img>
        </xsl:variable>
        <xsl:apply-templates select="ms:node-set($image)/*" mode="cleanXhtml" />
      </xsl:if>
    </xsl:if>
  </xsl:template>


  <xsl:template name="getSiteURL">
    <xsl:text>http://</xsl:text>
    <xsl:value-of select="/Page/Request/ServerVariables/Item[@name='SERVER_NAME']/node()"/>
  </xsl:template>


  <xsl:template match="Content[@type='Module']" mode="moreLinkEmail">
    <xsl:variable name="link" select="@link"/>
    <xsl:if test="$link!=''">
      <xsl:variable name="numbertest">
        <!-- Test if link is numeric then link is internal-->
        <xsl:call-template name="IsNan">
          <xsl:with-param name="var" select="$link"/>
        </xsl:call-template>
      </xsl:variable>
      <table cellspacing="0" cellpadding="0" class="emailmorelink">
        <tr>
          <td class="moreLinkBtn">
            <a title="{@linkText}">
              <xsl:choose>
                <xsl:when test="$numbertest = 'number'">
                  <xsl:variable name="pageId" select="@link"/>
                  <xsl:attribute name="href">
                    <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]" mode="getHref"/>
                  </xsl:attribute>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:choose>
                    <xsl:when test="contains($link,'#')">
                      <xsl:attribute name="class">
                        <xsl:text>btn btn-default btn-sm scroll-to-anchor</xsl:text>
                      </xsl:attribute>
                      <xsl:attribute name="href">
                        <xsl:value-of select="$link"/>
                      </xsl:attribute>
                    </xsl:when>
                    <xsl:when test="contains($link,'http')">
                      <xsl:attribute name="href">
                        <xsl:value-of select="$link"/>
                      </xsl:attribute>
                      <xsl:attribute name="rel">external</xsl:attribute>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:attribute name="href">
                        <xsl:text>http://</xsl:text>
                        <xsl:value-of select="$link"/>
                      </xsl:attribute>
                      <xsl:attribute name="rel">external</xsl:attribute>
                    </xsl:otherwise>
                  </xsl:choose>

                </xsl:otherwise>
              </xsl:choose>
              <xsl:value-of select="@linkText"/>
            </a>
          </td>
        </tr>
      </table>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>
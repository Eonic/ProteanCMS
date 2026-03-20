<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">


  <xsl:import href="mailer-imports.xsl"/>
  <xsl:import href="../../email/email-stationery.xsl"/>

  <xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/>

  <xsl:template match="/">
    <xsl:apply-imports/>
  </xsl:template>
  
  <!-- THESE SHOULDN'T BE HERE IDEALLY BUT EMAIL BREAKS IF REMOVED-->
  <xsl:template match="Content[@moduleType='FormattedText']" mode="displayBrief">
    <xsl:apply-templates select="node()" mode="cleanXhtml"/>
  </xsl:template>

  <xsl:template match="Content[@moduleType='Image']" mode="displayBrief">
    <xsl:choose>
      <xsl:when test="@resize='true'">
        <xsl:apply-templates select="." mode="resize-image">
          <xsl:with-param name="rootpath" select="$siteURL"/>
        </xsl:apply-templates>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="node()" mode="cleanXhtml"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- END OF "THESE SHOULDN'T BE HERE IDEALLY BUT EMAIL BREAKS IF REMOVED"-->

  <xsl:template match="*" mode="subject">
    <xsl:text> </xsl:text>
  </xsl:template>

  <xsl:template match="Page[@adminMode='false']" mode="adminStyle">
    <link type="text/css" rel="stylesheet" href="/ptn/features/mailer/mailer-wysiwyg.scss"/>
  </xsl:template>

  <!--   ########################   Admin Only   ############################   -->
  <!-- ACTUAL EMAIL TRANSMISSION TEMPLATE -->
  <xsl:template match="Page[not(@adminMode)]" mode="bodyBuilder">
    <body style="margin:0;padding:0;" >
      <xsl:apply-templates select="." mode="emailBody"/>
    </body>
  </xsl:template>

  <xsl:template match="Page[@adminMode='false']" mode="bodyBuilder">
    <body style="margin:0;padding:0;padding-top:50px!important;" id="pg{@id}" class="email-wysiwyg-body">
      <div class="ptn-edit">
        <xsl:apply-templates select="AdminMenu"/>
      </div>
      <div id="dragableModules" class="Site">
        <div>
          <xsl:apply-templates select="." mode="emailBody"/>
        </div>
      </div>
      <div class="ptn-edit">
        <xsl:apply-templates select="." mode="adminFooter"/>
      </div>
      <xsl:apply-templates select="." mode="footerJs"/>
    </body>
  </xsl:template>

  <!-- Javascripts that can be brought in in the footer of the HTML document, e.g. asynchronous scripts -->
  <xsl:template match="Page" mode="footerJs">
    <xsl:apply-templates select="." mode="js"/>
  </xsl:template>


  <xsl:template match="Page[@adminMode='false']" mode="siteJs">
    <xsl:call-template name="bundle-js">
      <xsl:with-param name="comma-separated-files">
        <xsl:apply-templates select="." mode="commonJsFiles" />
        <xsl:text>~/ptn/core/vue/vue.min.js,</xsl:text>
        <xsl:text>~/ptn/core/vue/axios.min.js,</xsl:text>
        <xsl:text>~/ptn/core/vue/polyfill.js,</xsl:text>
        <xsl:text>~/ptn/core/vue/protean-vue.js,</xsl:text>
        <xsl:text>~/ptn/libs/tinymce/jquery.tinymce.min.js,</xsl:text>
        <!-- Not sure where we are using this please add note if needing to re-add -->
        <!-- <xsl:text>~/ptn/admin/treeview/jquery.treeview.js,</xsl:text> -->
        <xsl:text>~/ptn/admin/treeview/ajaxtreeview.js,</xsl:text>
        <xsl:text>~/ptn/libs/jqueryui/jquery-ui.js,</xsl:text>
        <xsl:text>~/ptn/libs/fancyapps/ui/dist/fancybox.umd.min.js,</xsl:text>
        <xsl:text>~/ptn/libs/jquery.lazy/jquery.lazy.min.js,</xsl:text>
        <xsl:text>~/ptn/admin/admin.js</xsl:text>
      </xsl:with-param>
      <xsl:with-param name="bundle-path">
        <xsl:text>~/Bundles/Admin</xsl:text>
      </xsl:with-param>
    </xsl:call-template>
    <xsl:apply-templates select="." mode="siteAdminJs"/>
    <xsl:apply-templates select="." mode="LayoutAdminJs"/>
  </xsl:template>

  <xsl:template match="Page[@previewMode]" mode="bodyBuilder">
    <body style="margin:0;padding:0;" class="Site">
      <xsl:apply-templates select="PreviewMenu"/>
      <xsl:apply-templates select="." mode="emailBody"/>
      <xsl:apply-templates select="." mode="emailStyle"/>
    </body>
  </xsl:template>

  <!--   ########################   Main Email Layout   ############################   -->

  <xsl:template match="Page" mode="bodyLayout">
    <xsl:apply-templates select="." mode="mainLayout"/>
  </xsl:template>

  <xsl:template match="Page" mode="adminBreadcrumb">

  </xsl:template>

  <xsl:template match="Page" mode="addModuleControls">
    <xsl:param name="text"/>
    <xsl:param name="class"/>
    <xsl:param name="position"/>
    <xsl:if test="AdminMenu/descendant-or-self::MenuItem[@cmd='AddModule'] and $page/@ewCmd!='PreviewOn'">
      <xsl:attribute name="class">
        <xsl:text>moduleContainer</xsl:text>
        <xsl:if test="$class!=''">
          <xsl:text> </xsl:text>
          <xsl:value-of select="$class"/>
        </xsl:if>
      </xsl:attribute>
      <div class="ptn-edit options addmodule">
        <div class="addHere">
          <strong>
            <xsl:value-of select="$position"/>
          </strong>
          <xsl:text> - drag a module here</xsl:text>
        </div>
        <a class="btn btn-primary btn-xs pull-right" href="?ewCmd=AddMailModule&amp;pgid={/Page/@id}&amp;position={$position}">
          <i class="fa fa-plus">&#160;</i>&#160;<span class="sr-only">
            <xsl:value-of select="$text"/>
          </span>
        </a>
      </div>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Page" mode="addModule">
    <xsl:param name="text"/>
    <xsl:param name="position"/>
    <xsl:param name="class"/>
    <xsl:param name="width"/>
    <xsl:param name="auto-col"/>
    <xsl:param name="module-type"/>

    <xsl:apply-templates select="." mode="addModuleControlsSection">
      <xsl:with-param name="text" select="$text"/>
      <xsl:with-param name="class" select="$class"/>
      <xsl:with-param name="position" select="$position"/>
    </xsl:apply-templates>

    <xsl:for-each select="/Page/Contents/Content[@type='Module' and @position = $position]">
      <tr>
        <td>
          <xsl:apply-templates select="." mode="displayModule"/>
          <xsl:text> </xsl:text>
        </td>
      </tr>
    </xsl:for-each>
  </xsl:template>
</xsl:stylesheet>
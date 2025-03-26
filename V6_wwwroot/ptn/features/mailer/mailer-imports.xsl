<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!-- -->
  <!-- ## COMMON IMPORTS ########################################################################   -->
  <!-- ## IMPORTS ALL COMMON USED XSLs   #######################################################   -->

  <xsl:import href="../../core/functions.xsl"/>
  <xsl:import href="../../core/xforms.xsl"/>
  <xsl:import href="../../admin/admin.xsl"/>
  <xsl:import href="../../admin/admin-wysiwyg.xsl"/>
  <xsl:import href="mailer-layouts.xsl"/>
  <xsl:import href="../../core/localisation.xsl"/>

	<xsl:import href="modules/news/news.xsl"/>

  <xsl:template match="Page" mode="footerJs"></xsl:template>

</xsl:stylesheet>
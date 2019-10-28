<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!-- -->
  <!-- ## COMMON IMPORTS ########################################################################   -->
  <!-- ## IMPORTS ALL COMMON USED XSLs   #######################################################   -->

  <xsl:import href="../Tools/Functions.xsl"/>
  <xsl:import href="../xForms/xForms-bs-mininal.xsl"/>
  <xsl:import href="../Admin/Admin.xsl"/>
  <xsl:import href="../Admin/AdminWYSIWYG.xsl"/>
  <xsl:import href="MailerLayouts.xsl"/>
  <xsl:import href="../localisation/SystemTranslations.xsl"/>

  <xsl:template match="Page" mode="footerJs"></xsl:template>

</xsl:stylesheet>
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!-- -->
  <!-- ## MINIMAL IMPORTS ########################################################################   -->
  <!-- ## FILES REQUIRED FOR EVERY BOOTSTRAP3 SITE  #######################################################   -->
  
  <xsl:import href="Tools/Functions.xsl"/>
  <xsl:import href="PageLayouts/MinimalLayouts.xsl"/>
  <xsl:import href="xForms/xForms-bs-mininal.xsl"/>
  <xsl:import href="Admin/AdminWYSIWYG.xsl"/>
  <xsl:import href="localisation/SystemTranslations.xsl"/>
  
</xsl:stylesheet>
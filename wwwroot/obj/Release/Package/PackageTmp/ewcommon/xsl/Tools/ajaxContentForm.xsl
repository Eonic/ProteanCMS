<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
  <xsl:import href="../Tools/EwCommonFunctions.xsl"/>
  <xsl:import href="../xForms/xFormsV4_0.xsl"/>
  
  <xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/> 

  <xsl:template match="/">
   <xsl:apply-imports/>
  </xsl:template>

  <xsl:template match="Page">
    <xsl:apply-templates select="ContentDetail/Content[@type='xform']" mode="xform"/>
    <xsl:apply-templates select="ContentDetail/Content[@type!='xform']" mode="displayContent"/>
  </xsl:template>

  </xsl:stylesheet>
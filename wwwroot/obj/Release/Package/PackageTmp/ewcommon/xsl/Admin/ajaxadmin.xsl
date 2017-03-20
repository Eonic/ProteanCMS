<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
	<xsl:import href="../Tools/Functions.xsl"/>
	<xsl:import href="../xForms/xForms.xsl"/>
  <xsl:import href="../Admin/Admin.xsl"/>
  <xsl:import href="../localisation/SystemTranslations.xsl"/>
	
	<xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/>
	
	<xsl:template match="/">
		<xsl:apply-imports/>
	</xsl:template>
	
	<xsl:template match="Page[Request/*/Item[@name='ajaxCmd']/node()='GetStructureNode']">
    <xsl:variable name="level" select="Menu/@level"/>
		<xsl:variable name="pgid" select="Request/*/Item[@name='pgid']/node()"/>
    <ul class="tree-folder-content">
       <xsl:for-each select="Menu/MenuItem/MenuItem">
        <xsl:apply-templates select="." mode="editStructure">
          <xsl:with-param name="level" select="$level + 1"/>
        </xsl:apply-templates>
      </xsl:for-each>
		</ul>
	</xsl:template>


	<xsl:template match="Page[Request/*/Item[@name='ajaxCmd']/node()='GetMoveNode']">
		<xsl:variable name="pgid" select="Request/*/Item[@name='pgid']/node()"/>
    <xsl:variable name="level" select="Menu/@level"/>
    <ul class="tree-folder-content">
      <xsl:apply-templates select="Menu/descendant-or-self::MenuItem[@id=$pgid]/MenuItem" mode="movePage">
        <xsl:with-param name="level">
          <xsl:value-of select="$level"/>
        </xsl:with-param>
      </xsl:apply-templates>
      <!--<xsl:copy-of select="/"/>-->
		</ul>
	</xsl:template>

	<xsl:template match="Page[Request/*/Item[@name='ajaxCmd']/node()='GetMoveContent']">
		<xsl:variable name="pgid" select="Request/*/Item[@name='pgid']/node()"/>
    <xsl:variable name="level" select="Menu/@level"/>
    <ul class="tree-folder-content">
      <xsl:apply-templates select="Menu/descendant-or-self::MenuItem[@id=$pgid]/MenuItem" mode="moveContent">
        <xsl:with-param name="level">
          <xsl:value-of select="$level + 1"/>
        </xsl:with-param>
      </xsl:apply-templates>
		</ul>
	</xsl:template>

	
	<xsl:template match="Page[Request/*/Item[@name='ajaxCmd']/node()='GetLocateNode']">
		<xsl:variable name="pgid" select="Request/*/Item[@name='pgid']/node()"/>
    <xsl:variable name="level" select="Menu/@level"/>
    <ul class="tree-folder-content">
      <xsl:apply-templates select="Menu/descendant-or-self::MenuItem[@id=$pgid]/MenuItem" mode="LocateContent">
        <xsl:with-param name="level">
          <xsl:value-of select="$level + 1"/>
        </xsl:with-param>
      </xsl:apply-templates>
		</ul>
	</xsl:template>

	<xsl:template match="Page[Request/*/Item[@name='ajaxCmd']/node()='GetAdvNode']">
    <xsl:variable name="level" select="Menu/@level"/>
		<xsl:variable name="pgid" select="Request/*/Item[@name='pgid']/node()"/>
    <ul class="tree-folder-content">
      <xsl:apply-templates select="Menu/descendant-or-self::MenuItem[@id=$pgid]/MenuItem" mode="menuitem_am">
        <xsl:with-param name="level" select="$level + 1"/>
      </xsl:apply-templates>
		</ul>
	</xsl:template>

	<xsl:template match="Page[Request/*/Item[@name='ajaxCmd']/node()='editStructurePermissions']">
		<xsl:variable name="pgid" select="Request/*/Item[@name='pgid']/node()"/>
    <ul class="tree-folder-content">
			<xsl:apply-templates select="Menu/descendant-or-self::MenuItem[@id=$pgid]/MenuItem" mode="editStructurePermissions"/>
		</ul>
	</xsl:template>

</xsl:stylesheet>
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
	<xsl:import href="../core/functions.xsl"/>
	<xsl:import href="../core/xforms.xsl"/>
  <xsl:import href="admin.xsl"/>
  <xsl:import href="../core/localisation.xsl"/>
	
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

	<xsl:template match="Page[Request/*/Item[@name='ajaxCmd']/node()='GetFolderNode']">
		<xsl:variable name="folderPath" select="translate(Request/*/Item[@name='pgid']/node(),'~','\')"/>
		<xsl:variable name="level" select="count(ContentDetail/descendant-or-self::folder[@path=$folderPath]/ancestor::folder)"/>
		<ul class="tree-folder-content">
			<xsl:apply-templates select="ContentDetail/folder/folder" mode="FolderTree">
				<xsl:with-param name="level" select="$level + 2 + number(ContentDetail/folder/@startLevel)"/>
			</xsl:apply-templates>
		</ul>
	</xsl:template>

	<xsl:template match="folder" mode="FolderTree">
		<xsl:param name="level"/>
		<xsl:variable name="filename">
			<xsl:call-template name="url-encode">
				<xsl:with-param name="str" select="@name"/>
			</xsl:call-template>
		</xsl:variable>
		<xsl:variable name="fld">
			<xsl:call-template name="url-encode">
				<xsl:with-param name="str">
					<xsl:value-of select="@path"/>
				</xsl:with-param>
			</xsl:call-template>
		</xsl:variable>
		<li id="node{translate(@path,'\','~')}" data-tree-level="{$level}" data-tree-parent="{translate(parent::folder/@path,'\','~')}">
			<xsl:attribute name="class">
				<xsl:text>list-group-item level-</xsl:text>
				<xsl:value-of select="$level"/>
				<xsl:if test="@active='true'">
					<xsl:text> active collapsable</xsl:text>
				</xsl:if>
			</xsl:attribute>
			<a href="{$appPath}?ewCmd={/Page/@ewCmd}&amp;fld={$fld}&amp;targetForm={/Page/Request/QueryString/Item[@name='targetForm']/node()}&amp;targetField={/Page/Request/QueryString/Item[@name='targetField']/node()}">
				<i>
					<xsl:attribute name="class">
						<xsl:text>fas fa-lg</xsl:text>
						<xsl:choose>
							<xsl:when test="@active='true'">
								<xsl:text> fa-folder-open</xsl:text>
							</xsl:when>
							<xsl:otherwise>
								<xsl:text> fa-folder</xsl:text>
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
			<xsl:if test="descendant-or-self::folder[@active='true']">
				<xsl:apply-templates select="folder" mode="FolderTree">
					<xsl:with-param name="level">
						<xsl:value-of select="$level + 1"/>
					</xsl:with-param>
				</xsl:apply-templates>
			</xsl:if>
		</xsl:if>
	</xsl:template>

	<xsl:template match="Page[Request/*/Item[@name='ajaxCmd']/node()='editStructurePermissions']">
		<xsl:variable name="pgid" select="Request/*/Item[@name='pgid']/node()"/>
		<ul class="tree-folder-content">
			<xsl:apply-templates select="Menu/descendant-or-self::MenuItem[@id=$pgid]/MenuItem" mode="editStructurePermissions"/>
		</ul>
	</xsl:template>

</xsl:stylesheet>
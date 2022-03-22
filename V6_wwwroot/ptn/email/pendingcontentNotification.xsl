<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:import href="emailStationary.xsl"/>
	<xsl:import href="../Admin/Admin.xsl"/>
	<xsl:template match="/">
		<xsl:apply-imports/>
	</xsl:template>
	<xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/>
	<xsl:template match="*" mode="subject">Content awaiting approval</xsl:template>
	<xsl:template match="*" mode="bodyLayout">
		<xsl:variable name="siteURL">
			<xsl:call-template name="getSettings">
				<xsl:with-param name="sectionName" select="'web'"/>
				<xsl:with-param name="valueName" select="'BaseUrl'"/>
			</xsl:call-template>
		</xsl:variable>
		<font face="verdana" size="2" color="#000000">


			<p>
				There is content on <a href="{$siteURL}">
					<xsl:value-of select="$siteURL"/>
				</a> that is awaiting approval.
				You have received this e-mail because you are required to approve the content before it can be displayed on the website.
			</p>
			<p>
				Please find below a list of all content that has been submitted for approval
				<xsl:if test="@since!=''">
					<xsl:text> since </xsl:text>
					<strong>
						<xsl:call-template name="formatdate">
							<xsl:with-param name="date" select="@since"/>
							<xsl:with-param name="format" select="'F'"/>
						</xsl:call-template>
					</strong>

				</xsl:if>
				<xsl:text>.</xsl:text>
			</p>
			<p>
				To view and approve the content, please log on to the site's admin system and click on the Awaiting Approval button.
			</p>

			<xsl:apply-templates select="//GenericReport" mode="pending"/>
		</font>
	</xsl:template>



	<xsl:template match="GenericReport" mode="pending">
		<xsl:variable name="newPending" select="Pending[@currentLiveVersion='' or not(@currentLiveVersion)]"/>
		<xsl:variable name="updatedPending" select="Pending[@currentLiveVersion!='' and @currentLiveVersion]"/>

		<xsl:if test="count($newPending) &gt; 0">
			<h3>New content</h3>
			<ul>
				<xsl:apply-templates select="$newPending" mode="simpleListView">
					<xsl:sort select="Last_Updated" order="descending"/>
				</xsl:apply-templates>
			</ul>
		</xsl:if>

		<xsl:if test="count($updatedPending) &gt; 0">
			<h3>Updated content</h3>
			<ul>
				<xsl:apply-templates select="$updatedPending" mode="simpleListView">
					<xsl:sort select="Last_Updated" order="descending"/>
				</xsl:apply-templates>
			</ul>
		</xsl:if>
	</xsl:template>

	<xsl:template match="Pending" mode="simpleListView">
		<li>
			<strong>
				<xsl:value-of select="Name"/>
			</strong>
			<xsl:text> [</xsl:text>
			<xsl:value-of select="translate(Type,'_',' ')"/>
			<xsl:text>]</xsl:text>
			<em>
				<xsl:text> by </xsl:text>
				<xsl:value-of select="UserXml/User/FirstName"/>
				<xsl:text> </xsl:text>
				<xsl:value-of select="UserXml/User/LastName"/>
			</em>
			<xsl:text> (</xsl:text>
			<xsl:call-template name="formatdate">
				<xsl:with-param name="date" select="Last_Updated"/>
				<xsl:with-param name="format" select="'dd-MMM-yy HH:mm'"/>
			</xsl:call-template>
			<xsl:text>)</xsl:text>
		</li>
	</xsl:template>

	<!-- Hide functional links -->
	<xsl:template name="reportDetailExcelHeader"/>
	<xsl:template match="*" mode="reportDetailListButtons"/>
</xsl:stylesheet>

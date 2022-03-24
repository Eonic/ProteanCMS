<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
  <xsl:import href="../../../../../ewcommon_v5/xsl/Mailer/MailerImports.xsl"/>
  <xsl:import href="Mailer.xsl"/>

  <xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/>
  <xsl:template match="/">
    <xsl:apply-imports/>
  </xsl:template>

	<xsl:variable name="currentPage" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]"/>
	<xsl:variable name="sectionPage" select="/Page/Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]"/>
	<xsl:variable name="siteURL">
		<xsl:text>http://</xsl:text>
		<xsl:value-of select="/Page/Request/ServerVariables/Item[@name='SERVER_NAME']"/>
	</xsl:variable>
	<!-- -->

	<!--   ########################   InlineStyle   ############################   -->

	<xsl:template match="Page" mode="siteStyle">
		<style>
			body{BACKGROUND: #FFF !important;MARGIN: 0px;padding:0;color:#000;font:13px/1.231 verdana,arial,helvetica,clean,sans-serif;}
			table{border-collapse:collapse;}
			#mainTable{text-align: left;}
			div,ul{padding:0px;margin:0px}
			.terminus{line-height:0px;clear:both;}
		</style>
	</xsl:template>

	<!-- ACTUAL EMAIL TRANSMISSION TEMPLATE -->
	<xsl:template match="Page[not(@adminMode)]" mode="bodyBuilder">
		<body style="padding:0 !important;margin:0 !important;background-color:#ffffff;font-size:12pt;">
			<xsl:apply-templates select="." mode="bodyDisplay"/>
		</body>
	</xsl:template>

  <xsl:template match="Page[@adminMode='false']" mode="bodyBuilder">
    <body>
      <div class="ewAdmin">
        <xsl:apply-templates select="AdminMenu"/>
      </div>
      <xsl:apply-templates select="." mode="bodyDisplay"/>
      <div class="ewAdmin">
        <xsl:apply-templates select="." mode="adminFooter"/>
      </div>
    </body>
  </xsl:template>

	<xsl:template match="Page[@previewMode]" mode="bodyBuilder">
		<body style="margin:0!important;padding:0;background-color:#ffffff;font-size:12pt;" bgcolor="#ffffff">
			<xsl:apply-templates select="PreviewMenu"/>
			<xsl:apply-templates select="." mode="bodyDisplay"/>
		</body>
	</xsl:template>

	<xsl:template match="Page" mode="bodyDisplay">
		<div id="mainTable" class="Site">    
			<center>
						<table id="mainTable" cellpadding="0" cellspacing="0" width="650" style="width:650px !important">
							<tr>
								<td id="header" width="650" height="87" align="left" style="text-align:left;background-color: #333333;">
									<a href="{$siteURL}" title="Eonic">
										<img src="{$siteURL}/images/layout/emailStationary/header.jpg" width="650" height="87" alt="Eonic"  style="border:0 !important;"/>
									</a>
								</td>
							</tr>
							<tr>
								<td id="header2" width="650" height="31" align="left" style="text-align:left;background: #333333; padding-left: 10px;">
									<font face="Verdana" size="4" color="#FFFFFF">
										<strong>
											<xsl:choose>
												<xsl:when test="Contents/Content[@name='title']">
													<xsl:apply-templates select="Contents/Content[@name='title']" mode="displayContent"/>
												</xsl:when>
												<xsl:otherwise>
													<xsl:value-of select="$currentPage/@name"/>
												</xsl:otherwise>
											</xsl:choose>
										</strong>
									</font>
								</td>
							</tr>
							<tr>
								<td id="msgBody" width="650" style="border:1px solid #FFF;background:#F2F2F2;">
									<xsl:apply-templates select="." mode="mainLayout"/>
								</td>
							</tr>
							<tr>
								<td id="mailFooter" width="490" align="left" height="98" style="font-family:Arial;font-size:7pt;text-align:left;color:#ffffff;background-color: #333333; padding-left: 10px;">
									Tel:: 08708 361 755 &#160;&#160;&#160; Email:: <a style="color:#ffffff;font-weight:bold;" href="mailto:info@eonic.co.uk">info@eonic.co.uk</a>&#160;&#160;&#160; Web::
									<a style="color:#ffffff;font-weight:bold;"  href="www.eonic.co.uk">www.eonic.co.uk</a>
									<br/>Eonic Ltd | 43 High Street | Tunbridge Wells | Kent | TN1 1XL<br/><br/>
									Registered in the UK: 031 610 57<br/>
									<br/>Would you like email stationery like ours? Give us a call.
								</td>
							</tr>
							<tr>
								<td align="center" style="padding:10px;">
									<font align="center" face="Verdana" size="1">
										<xsl:apply-templates select="/" mode="optOutStatement">
											<xsl:with-param name="style">text-align:center;color:#333333;padding:5px;</xsl:with-param>
										</xsl:apply-templates>
									</font>
									<font align="center" face="Verdana" size="0.8">
										<p style="text-align:center;color: #333;">
											<xsl:call-template name="eonicDeveloperLink">
												<xsl:with-param name="style" select="'color:#333;font-size:11px;text-decoration:none;'"/>
											</xsl:call-template>
										</p>
									</font>
								</td>
							</tr>
						</table>
					</center>
			</div>
	</xsl:template>
	<!--   ##########################################################   Content Boxes   ##########################################################   -->
	<!-- -->
	<xsl:template match="/" mode="contentBox">
		<xsl:param name="contentMode"/>
		<xsl:param name="contentName"/>
		<xsl:param name="contentTitle"/>
		<xsl:param name="contentId"/>
		<div class="box" style="height:1%;margin-bottom:20px;">
			<div class="tl" style="background: #333333">
				<div class="tr" style="margin-left:5px;">
					<xsl:apply-templates select="/Page" mode="inlinePopupSingle">
						<xsl:with-param name="type">PlainText</xsl:with-param>
						<xsl:with-param name="text">
							Add <xsl:value-of select="$contentTitle"/>
						</xsl:with-param>
						<xsl:with-param name="name">
							<xsl:value-of select="$contentTitle"/>
						</xsl:with-param>
						<xsl:with-param name="class">title</xsl:with-param>
					</xsl:apply-templates>
					<h3 style="margin:0;font-size:16px;color:#FFF;">
						<xsl:choose>
							<xsl:when test="/Page/Contents/Content[@name=$contentTitle]">
								<xsl:copy-of select="/Page/Contents/Content[@name=$contentTitle]/node()"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:text>&#160;</xsl:text>
							</xsl:otherwise>
						</xsl:choose>
					</h3>
				</div>
			</div>
			<div class="content" style="border:1px solid #333333;padding:10px;">
				<xsl:apply-templates select="/" mode="contentBox_Contents">
					<xsl:with-param name="contentMode" select="$contentMode"/>
					<xsl:with-param name="contentName" select="$contentName"/>
					<xsl:with-param name="contentTitle" select="$contentTitle"/>
					<xsl:with-param name="contentId" select="$contentId"/>
				</xsl:apply-templates>
			</div>
		</div>
	</xsl:template>
	<!-- -->
</xsl:stylesheet>
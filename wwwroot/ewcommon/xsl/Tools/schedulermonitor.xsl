<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:template match="/">
		<xsl:apply-imports/>
	</xsl:template>
	<xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/>
	<xsl:template match="*">
		<html xml:lang="en-gb">
			<head>

				<style>

					body
					{font-family:Verdana, Arial, Helvetica, sans-serif;background:#fff;margin:10px 0;color:#444444;font-size:.8em;}
					h1
					{margin:1em 0;font-size:1.5em;}
					h2
					{margin:0px 0;font-size:1.1em;}
					h3
					{margin:0px 0;font-size:1.0em;color:#2885E1}
					img
					{border:none;}
					p
					{margin:10px 0 1em;}
					table
					{width:100%;border:1px solid #DBDCE3;border-bottom:none;margin:20px 0;}
					td, th
					{line-height:1.7;text-align:left;padding:3px 10px;border-bottom:1px solid #DBDCE3;vertical-align:top}
					
					th
					{background:#E9F9FF;text-align:right;border-top:1px solid #EBECF3;border-left:1px solid #EBECF3;}
					.tab th, .tab td 
					{text-align: center;}
					td
					{border-top:1px solid #EBECF3;}
					#mainTable
					{margin:auto;width:550px;background:#fff;}
					#mainLayout
					{padding:20px;40px}

				</style>
				<title>
					<xsl:apply-templates mode="summary" select="."/>
				</title>
			</head>
			<body>
				<div id="mainTable">
					<div id="mainLayout">
						<h1>
							<xsl:apply-templates mode="summary" select="."/>
						</h1>
						<h2>The scheduler summary for the past 24 hours is as follows:</h2>
						<table cellspacing="0" id="emailSummary" summary="Summary">
							<xsl:for-each select="//Monitor/*[Total_Tasks]/*">
								<tr>
									<th>
										<xsl:value-of select="translate(name(),'_',' ')"/>
									</th>
									<td>
										<xsl:value-of select="."/>
									</td>
								</tr>
							</xsl:for-each>
						</table>

						<h2>Failure summary (past 24 hours)</h2>
						<table cellspacing="0" id="emailSummary" class="tab" summary="Summary">
							<tr>
								<xsl:for-each select="//Monitor/*[Total_Failures][1]/*">
										<th>
											<xsl:value-of select="translate(name(),'_',' ')"/>
										</th>
								</xsl:for-each>							
							</tr>
							<xsl:for-each select="//Monitor/*[Total_Failures]">
								<tr>
									<xsl:for-each select="*">
										<td>
											<xsl:value-of select="."/>
										</td>
									</xsl:for-each>
								</tr>
							</xsl:for-each>
						</table>						
						
				
					</div>
				</div>
			</body>
		</html>
	</xsl:template>

	<xsl:template match="Monitor" mode="summary">
		<xsl:text>Eonic Scheduler Summary </xsl:text>
		<xsl:if test="number(//Total_Failed) &gt; 0">
			<xsl:text>**</xsl:text>
			<xsl:value-of select="//Total_Failed"/>
			<xsl:text> failures** </xsl:text>
		</xsl:if>
		<xsl:text>[</xsl:text>
		<xsl:value-of select="@DatabaseServer"/>		
		<xsl:text>,</xsl:text>
		<xsl:value-of select="@Database"/>
		<xsl:text>]</xsl:text>
	</xsl:template>

</xsl:stylesheet>
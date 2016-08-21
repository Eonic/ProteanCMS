<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet">
	<xsl:output method="html" indent="yes" omit-xml-declaration="yes" encoding="utf-8"/>
	<xsl:template match="Page">
		<xsl:processing-instruction name="mso-application">
			<xsl:text>progid="Excel.Sheet"</xsl:text>
		</xsl:processing-instruction>
		<html>
			<head>
				<style>

					th, td {font-family:Arial,sans-serif;}
					th, td {font-size:9pt;}

					.date{mso-number-format:"dd\/mm\/yy HH:mm"}

				</style>
			</head>
			<body>
        <!-- Write each sheet -->
				<xsl:for-each select="ContentDetail/report | ContentDetail/directory | Contents/Content/report | ContentDetail/Content/tblCodes | ContentDetail/Content/Report | ContentDetail/Content/GenericReport">
					<table>
						<xsl:choose>
							<xsl:when test="user">
								<!-- Write title row. -->
								<xsl:apply-templates select="user[1]" mode="title"/>
								<!-- Write each row. -->
								<xsl:choose>
									<xsl:when test="/Page/Request/QueryString/Item[@name='LastNameStarts']">
										<xsl:apply-templates select="user[starts-with(User/LastName,/Page/Request/QueryString/Item[@name='LastNameStarts']/node())]" mode="rows"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:apply-templates select="user" mode="rows"/>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:when>
							<xsl:otherwise>
								<!-- Write each row. -->
								<xsl:apply-templates select="certificates/certificate[1] | companies/company[1] | activity/candidate[1] | activity/attempt[1] | PageReport/PageActivity[1] | company[1] | user[1] | role[1] | group[1] | directory/*[1] | course/quiz[1] |  diplomas/*[1] | Code[1] | *[parent::GenericReport][1]" mode="title"/>
								<!-- Write each row. -->
								<!--xsl:apply-templates select="user" mode="rows"/-->
								<xsl:apply-templates select="certificates/certificate | companies/company | activity/candidate | activity/attempt | PageReport/descendant-or-self::PageActivity | company | user | role | group | directory/* | course/quiz |  diplomas/* | Code  | *[parent::GenericReport]" mode="rows"/>
							</xsl:otherwise>
						</xsl:choose>
						<xsl:apply-templates select="." mode="reportTitles"/>
						<xsl:apply-templates select="Item" mode="reportRow"/>
					</table>
					<xsl:if test="summary">
						<table>
							<!-- Write each row. -->
							<xsl:apply-templates select="summary/*[1]" mode="title"/>
							<!-- Write each row. -->
							<xsl:apply-templates select="summary/*" mode="rows"/>
						</table>
					</xsl:if>
				</xsl:for-each>
			</body>
		</html>
	</xsl:template>
	<!-- Row template -->
	<xsl:template match="*" mode="title">
		<tr>
			<xsl:for-each select="*">
				<th>
					<xsl:choose>
						<xsl:when test="name()='UserXml'">Full Name</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="name()"/>
						</xsl:otherwise>
					</xsl:choose>
				</th>
			</xsl:for-each>
		</tr>
	</xsl:template>

	<!-- Row Title Templates -->
	<xsl:template match="*" mode="title">
		<tr>
			<xsl:apply-templates select="*" mode="titleCell">
				<xsl:with-param name="startPos">0</xsl:with-param>
			</xsl:apply-templates>
		</tr>
	</xsl:template>
	<xsl:template match="user" mode="title">
		<tr>
			<xsl:apply-templates select="User/*" mode="titleCell">
				<xsl:with-param name="startPos">0</xsl:with-param>
			</xsl:apply-templates>
			<xsl:apply-templates select="*[name()!='User']" mode="titleCell">
				<xsl:with-param name="startPos">6</xsl:with-param>
			</xsl:apply-templates>
		</tr>
	</xsl:template>
	<xsl:template match="attempt | candidate" mode="title">
		<tr>
			<xsl:apply-templates select="UserXml/User/*" mode="titleCell">
				<xsl:with-param name="startPos">0</xsl:with-param>
			</xsl:apply-templates>
			<xsl:apply-templates select="*[name()!='UserXml']" mode="titleCell">
				<xsl:with-param name="startPos">6</xsl:with-param>
			</xsl:apply-templates>
		</tr>
	</xsl:template>
	<xsl:template match="PageActivity" mode="title">
		<tr>
			<xsl:apply-templates select="*[name()!='PageActivity']" mode="titleCell">
				<xsl:with-param name="startPos">0</xsl:with-param>
			</xsl:apply-templates>
		</tr>
	</xsl:template>
	<xsl:template match="*" mode="titleCell">
		<xsl:param name="startPos"/>
		<td>
			<xsl:choose>
				<xsl:when test="name()='UserXml'">Full Name</xsl:when>
        <!--<xsl:when test="name()='IP_Address'"></xsl:when>-->
				<xsl:otherwise>
					<xsl:value-of select="translate(name(),'_',' ')"/>
				</xsl:otherwise>
			</xsl:choose>
		</td>
	</xsl:template>

	<!-- Row Templates -->
	<xsl:template match="*" mode="rows">
		<tr>
			<xsl:apply-templates select="*" mode="cols">
				<xsl:with-param name="startPos">0</xsl:with-param>
			</xsl:apply-templates>
		</tr>
	</xsl:template>
	<xsl:template match="user" mode="rows">
		<tr>
			<xsl:apply-templates select="User/*" mode="colsText">
				<xsl:with-param name="startPos">0</xsl:with-param>
			</xsl:apply-templates>
			<xsl:apply-templates select="*[name()!='User']" mode="cols">
				<xsl:with-param name="startPos">6</xsl:with-param>
			</xsl:apply-templates>
		</tr>
	</xsl:template>

	<xsl:template match="attempt | candidate" mode="rows">
		<tr>
			<xsl:apply-templates select="UserXml/User/*" mode="cols">
				<xsl:with-param name="startPos">0</xsl:with-param>
			</xsl:apply-templates>
			<xsl:apply-templates select="*[name()!='UserXml']" mode="cols">
				<xsl:with-param name="startPos">6</xsl:with-param>
			</xsl:apply-templates>
		</tr>
	</xsl:template>

	<xsl:template match="PageActivity" mode="title">
		<tr>
			<xsl:apply-templates select="*[name()!='PageActivity']" mode="cols">
				<xsl:with-param name="startPos">0</xsl:with-param>
			</xsl:apply-templates>
		</tr>
	</xsl:template>

	<!-- Cell (i.e., column) template -->
	<xsl:template match="*" mode="cols">
		<xsl:param name="startPos"/>
		<td>
			<xsl:choose>
				<xsl:when test="contains(name(),'Date')">
					<xsl:if test="not(contains(node(),'0001-01-01T00:00:00'))">
						<!--xsl:attribute name="ss:Type">DateTime</xsl:attribute-->
						<!-- <xsl:value-of select="substring(node(),0,24)"/> -->
						<xsl:attribute name="class">date</xsl:attribute>
						<xsl:call-template name="DD_Mon_YYYY">
							<xsl:with-param name="date" select="node()"/>
							<xsl:with-param name="showTime">true</xsl:with-param>
						</xsl:call-template>
					</xsl:if>
				</xsl:when>
				<xsl:when test="name() = 'Status'">
					<xsl:choose>
						<xsl:when test="node()='0'">In-active</xsl:when>
						<xsl:when test="node()='1' or node()='-1' ">Active</xsl:when>
					</xsl:choose>
				</xsl:when>
				<xsl:when test="number(.)=number(.)">
					<!--xsl:attribute name="ss:Type">Number</xsl:attribute-->
					<xsl:attribute name="class">number</xsl:attribute>
					<xsl:value-of select="number(node())"/>
				</xsl:when>
				<xsl:when test="name() = 'User' or name() = 'UserXml'">
					<xsl:choose>
						<xsl:when test="FirstName and LastName">
							<xsl:value-of select="LastName"/>, <xsl:value-of select="FirstName"/>
						</xsl:when>
						<xsl:when test="User/FirstName and User/LastName">
							<a href="mailto:{User/Email}">
								<xsl:value-of select="User/LastName"/>, <xsl:value-of select="User/FirstName"/>
							</a>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="node()"/>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:when>
				<xsl:when test="name()='Voted_For'">
					<xsl:value-of select="descendant-or-self::Title"/>
				</xsl:when>
        <!--<xsl:when test="name()='IP_Address'">
        </xsl:when>-->

				<xsl:otherwise>
					<!--xsl:attribute name="ss:Type">String</xsl:attribute-->
					<xsl:value-of select="node()"/>
				</xsl:otherwise>
			</xsl:choose>
		</td>
	</xsl:template>

	<xsl:template match="*" mode="colsText">
		<xsl:param name="startPos"/>
		<td>
			<xsl:value-of select="node()"/>
		</td>
	</xsl:template>
  
	<xsl:template match="UserXml" mode="colsText">
		<xsl:param name="startPos"/>
		<td>
			<a href="mailto:{User/Email}">
				<xsl:value-of select="User/LastName"/>, <xsl:value-of select="User/FirstName"/>
			</a>
		</td>
	</xsl:template>

	<xsl:template name="DD_Mon_YYYY">
		<xsl:param name="date"/>
		<xsl:param name="showTime"/>
		<xsl:variable name="month" select="number(substring($date, 6, 2))"/>
		<xsl:value-of select="number(substring($date, 9, 2))"/>/<xsl:choose>
			<xsl:when test="$month=1">Jan</xsl:when>
			<xsl:when test="$month=2">Feb</xsl:when>
			<xsl:when test="$month=3">Mar</xsl:when>
			<xsl:when test="$month=4">Apr</xsl:when>
			<xsl:when test="$month=5">May</xsl:when>
			<xsl:when test="$month=6">Jun</xsl:when>
			<xsl:when test="$month=7">Jul</xsl:when>
			<xsl:when test="$month=8">Aug</xsl:when>
			<xsl:when test="$month=9">Sep</xsl:when>
			<xsl:when test="$month=10">Oct</xsl:when>
			<xsl:when test="$month=11">Nov</xsl:when>
			<xsl:when test="$month=12">Dec</xsl:when>
			<xsl:otherwise>INVALID MONTH</xsl:otherwise>
		</xsl:choose>/<xsl:value-of select="substring($date, 1, 4)"/><xsl:if test="$showTime='true'">
			<xsl:call-template name="HH_MM">
				<xsl:with-param name="date" select="$date"/>
			</xsl:call-template>
		</xsl:if>
	</xsl:template>

	<xsl:template name="HH_MM">
		<xsl:param name="date"/><xsl:text> </xsl:text><xsl:value-of select="substring($date, 12, 2)"/>:<xsl:value-of select="substring($date, 15, 2)"/>
	</xsl:template>

	<xsl:template match="*"  mode="reportTitles">
		<!--do nothing-->
	</xsl:template>

	<xsl:template match="*" mode="reportRow">
		test
	</xsl:template>

	<xsl:template match="Report[@cReportType='CartDownload']" mode="reportTitles">
		<tr>
			<th>OrderId</th>
			<th>Status</th>
			<th>DateTime</th>
		</tr>
	</xsl:template>

	<xsl:template match="Item[parent::Report[@cReportType='CartDownload']]" mode="reportRow">
		<xsl:for-each select="cCartXml/Order/Item">
			<xsl:variable name="order" select="parent::Order"/>
			<xsl:variable name="item" select="ancestor::Item"/>
			<tr>
				<td>
					<xsl:value-of select="$item/nCartOrderKey/node()"/>
				</td>
				<td>
					<xsl:value-of select="$order/@status"/>
				</td>
				<td>
					<xsl:value-of select="$item/dInsertDate/node()"/>
				</td>
			</tr>
		</xsl:for-each>
	</xsl:template>

  <xsl:template match="*" mode="encodeXhtml">
    <xsl:text>&lt;</xsl:text>
    <xsl:value-of select="name()"/>
    <xsl:for-each select="@*[name()!='xmlns']">
      <xsl:text>&#160;</xsl:text>
      <xsl:value-of select="name()"/>
      <xsl:text>="</xsl:text>
      <xsl:value-of select="." />
      <xsl:text>"</xsl:text>
    </xsl:for-each>
    <xsl:text>&gt;</xsl:text>
    <xsl:apply-templates mode="encodeXhtml"/>
    <xsl:text>&lt;/</xsl:text>
    <xsl:value-of select="name()"/>
    <xsl:text>&gt;</xsl:text>
  </xsl:template>

  <xsl:template match="img | *[name()='img']" mode="encodeXhtml">
    <xsl:text>&lt;</xsl:text>
    <xsl:value-of select="name()"/>
    <xsl:for-each select="@*[name()!='xmlns']">
      <xsl:text>&#160;</xsl:text>
      <xsl:value-of select="name()"/>
      <xsl:text>="</xsl:text>
      <xsl:value-of select="." />
      <xsl:text>"</xsl:text>
    </xsl:for-each>
    <xsl:text>/&gt;</xsl:text>
  </xsl:template>

  <xsl:template match="br" mode="encodeXhtml">
    <xsl:text>&lt;br/&gt;</xsl:text>
  </xsl:template>

  <xsl:template match="hr" mode="encodeXhtml">
    <xsl:text>&lt;hr/&gt;</xsl:text>
  </xsl:template>



</xsl:stylesheet>

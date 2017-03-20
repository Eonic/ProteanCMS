<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
	<xsl:template match="Page">
		<html xml:lang="en-gb" xmlns="http://www.w3.org/1999/xhtml">
			<head>
				<title>
					<xsl:value-of select="Contents/Content[@name='pageTitle' or @name='PageTitle']"/>
				</title>
			</head>
			<body style="margin:0;background:#fff;">
				<xsl:copy-of select="ContentDetail/Content/Images/img[@class='detail']"/>
			</body>
		</html>
	</xsl:template>
</xsl:stylesheet>

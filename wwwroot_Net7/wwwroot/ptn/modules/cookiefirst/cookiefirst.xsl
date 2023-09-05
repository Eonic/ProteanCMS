<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">

	<xsl:template match="Content[@type='CookieFirst']" mode="headerOnlyContentJS">
		<xsl:if test="not($adminMode)">
			<script src="https://consent.cookiefirst.com/sites/{SiteUrl/node()}-{ApiKey/node()}/consent.js">&#160;</script>
		</xsl:if>
	</xsl:template>

	<xsl:template match="Content[@type='FreeCookieConsent']" mode="contentJS">
		<xsl:if test="not($adminMode)">

			<div id="cookiefirst-policy-page">&#160;</div>
			<!--
			<div>
				This cookie policy has been created and updated by <a href="https://cookiefirst.com/cookie-policy-generator/">Cookie Policy - CookieFirst</a>.
			</div>
-->
		</xsl:if>
	</xsl:template>
	
</xsl:stylesheet>

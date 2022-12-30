<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:import href="../Tools/Functions.xsl"/>
 <xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/>

	
  <xsl:template match="/">
    <xsl:apply-imports/>
  </xsl:template>
 
  <xsl:template match="*" mode="subject">
    <xsl:value-of select="$siteTitle"/> - <xsl:value-of select="@subjectLine"/>
  </xsl:template>


	<xsl:variable name="siteURL">

		<xsl:variable name="serverVariableURL">
			<xsl:call-template name="getServerVariable">
				<xsl:with-param name="valueName" select="'HTTP_HOST'"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:variable name="cartUrl">
			<xsl:call-template name="getSettings">
				<xsl:with-param name="sectionName" select="'cart'"/>
				<xsl:with-param name="valueName" select="'SiteURL'"/>
			</xsl:call-template>
		</xsl:variable>

		<xsl:choose>
			<xsl:when test="$cartUrl!=''">
				<xsl:value-of select="$cartUrl"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>http://</xsl:text>
				<xsl:value-of select="$serverVariableURL"/>
			</xsl:otherwise>
		</xsl:choose>

	</xsl:variable>

	<xsl:variable name="siteTitle">
		<xsl:call-template name="getSettings">
			<xsl:with-param name="sectionName" select="'web'"/>
			<xsl:with-param name="valueName" select="'SiteName'"/>
		</xsl:call-template>
	</xsl:variable>

	<xsl:variable name="SiteLogo">
		<xsl:call-template name="getSettings">
			<xsl:with-param name="sectionName" select="'web'"/>
			<xsl:with-param name="valueName" select="'SiteLogo'"/>
		</xsl:call-template>
	</xsl:variable>

	<xsl:variable name="CompanyName">
		<xsl:call-template name="getSettings">
			<xsl:with-param name="sectionName" select="'web'"/>
			<xsl:with-param name="valueName" select="'CompanyName'"/>
		</xsl:call-template>
	</xsl:variable>

	<xsl:variable name="CompanyAddress">
		<xsl:call-template name="getSettings">
			<xsl:with-param name="sectionName" select="'web'"/>
			<xsl:with-param name="valueName" select="'CompanyAddress'"/>
		</xsl:call-template>
	</xsl:variable>

	<xsl:variable name="CompanyTel">
		<xsl:call-template name="getSettings">
			<xsl:with-param name="sectionName" select="'web'"/>
			<xsl:with-param name="valueName" select="'CompanyTel'"/>
		</xsl:call-template>
	</xsl:variable>

	<xsl:variable name="CompanyEmail">
		<xsl:call-template name="getSettings">
			<xsl:with-param name="sectionName" select="'web'"/>
			<xsl:with-param name="valueName" select="'CompanyEmail'"/>
		</xsl:call-template>
	</xsl:variable>
	<xsl:variable name="VATnumber">
		<xsl:call-template name="getSettings">
			<xsl:with-param name="sectionName" select="'web'"/>
			<xsl:with-param name="valueName" select="'VATnumber'"/>
		</xsl:call-template>
	</xsl:variable>
	<xsl:variable name="CompanyRegNo">
		<xsl:call-template name="getSettings">
			<xsl:with-param name="sectionName" select="'web'"/>
			<xsl:with-param name="valueName" select="'CompanyRegNo'"/>
		</xsl:call-template>
	</xsl:variable>
	<xsl:variable name="CharityRegNo">
		<xsl:call-template name="getSettings">
			<xsl:with-param name="sectionName" select="'web'"/>
			<xsl:with-param name="valueName" select="'CharityRegNo'"/>
		</xsl:call-template>
	</xsl:variable>
	
	<xsl:template match="*">
			<xsl:apply-templates select="." mode="emailBody"/>
	</xsl:template>
	
	<xsl:template match="*" mode="pageTitle">
		<xsl:apply-templates select="." mode="subject"/>
	</xsl:template>

	<xsl:template match="*" mode="emailStyle">		
		
	</xsl:template>

	<xsl:template match="*" mode="emailBody">
	
		<xsl:value-of select="$siteTitle"/>

		<xsl:apply-templates select="." mode="bodyLayout"/>
						
	</xsl:template>


	<xsl:template match="*" mode="bodyLayout">
		Enquiry

		<xsl:for-each select="*">

                <xsl:choose>
                    <xsl:when test="@label and @label!=''">
                      <xsl:value-of select="@label"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="name()"/>
                    </xsl:otherwise>
                  </xsl:choose>
			<xsl:text>      </xsl:text>
   
            <xsl:choose>
              <xsl:when test="name()='Email'">
                      <xsl:value-of select="node()"/>
              </xsl:when>
              <xsl:when test="contains(@type,'date')">

                        <xsl:call-template name="DD_Mon_YYYY">
                          <xsl:with-param name="date" select="node()"/>
                        </xsl:call-template>

              </xsl:when>
              <xsl:otherwise>

                    <xsl:value-of select="node()"/>

              </xsl:otherwise>
            </xsl:choose>

<xsl:text>

</xsl:text>
			&#xD;
			
		</xsl:for-each>
   
        email sent from:<xsl:value-of select="@sessionReferrer"/>
                
  </xsl:template>

 
</xsl:stylesheet>
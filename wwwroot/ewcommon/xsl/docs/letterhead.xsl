<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
  <xsl:output method="xml" indent="yes" omit-xml-declaration="no" encoding="UTF-8"/>
  <xsl:variable name="filePath" select="'D:\HostingSpaces\ewcommon_v5-1\'"/>
  <xsl:variable name="headingfont" select="'Helvetica'"/>
  <xsl:variable name="headingcolor" select="'#333333'"/>
  <xsl:variable name="bodyfont" select="'Helvetica'"/>
  <xsl:variable name="page" select="."/>


  <xsl:variable name="siteTitle">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'SiteName'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name="DocLogo">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'DocumentLogo'"/>
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

  <xsl:template match="*" mode="documentContainer">
    <fo:root xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <fo:layout-master-set>
        <fo:simple-page-master master-name="simple"
                      page-height="29.7cm"
                      page-width="21cm"
                      margin-top="1.0cm"
                      margin-bottom="0cm"
                      margin-left="0.4cm"
                      margin-right="0cm">
          <fo:region-body margin-top="0cm"/>
          <fo:region-before extent="0cm"/>
          <fo:region-after extent="0cm"/>
        </fo:simple-page-master>
      </fo:layout-master-set>
      <xsl:apply-templates select="." mode="documentPage"/>
    </fo:root>
  </xsl:template>


	<xsl:template match="*" mode="documentPage">
		<fo:page-sequence master-reference="allPages"  xmlns:fo="http://www.w3.org/1999/XSL/Format">
			<xsl:apply-templates select="descendant-or-self::*[name()='Order'][1]" mode="PageTitle"/>
			<fo:static-content flow-name="page-footer">
				<fo:block>
				
					<fo:block margin-left="1cm" padding-top="1.2cm">
						<fo:table table-layout="fixed">
							<fo:table-column column-width="9.5cm"/>
							<fo:table-column column-width="9.5cm"/>
							<fo:table-body>
								<fo:table-row>
									<fo:table-cell padding="6pt">
										<fo:block font-size="5pt" text-align="left" font-family="{$bodyfont}" color="#000000" linefeed-treatment="preserve" space-after="2mm" padding-top="2mm">
											<xsl:apply-templates select="Contact[@type='Delivery Address']" mode="AddressBlock"/>
										</fo:block>
										<fo:block font-size="8pt" text-align="left" font-family="{$bodyfont}" color="#000000" linefeed-treatment="preserve" space-after="2mm" padding-top="2mm">
											Ref: <xsl:value-of select="@InvoiceRef"/>
										</fo:block>
										<fo:block font-size="8pt" text-align="left" font-family="{$bodyfont}" color="#000000" linefeed-treatment="preserve" space-after="2mm" padding-top="2mm">
											RETURN TO: <xsl:value-of select="$CompanyAddress"/>
										</fo:block>
									</fo:table-cell>
									<fo:table-cell padding="6pt" keep-together.within-page="always">
										<fo:block font-size="5pt" text-align="left" font-family="{$bodyfont}" color="#000000" linefeed-treatment="preserve" space-after="2mm" padding-top="2mm">
											<xsl:apply-templates select="Contact[@type='Delivery Address']" mode="AddressBlock"/>
										</fo:block>
										<fo:block font-size="8pt" text-align="left" font-family="{$bodyfont}" color="#000000" linefeed-treatment="preserve" space-after="2mm" padding-top="2mm">
											Ref: <xsl:value-of select="@InvoiceRef"/>
										</fo:block>
										<fo:block font-size="8pt" text-align="left" font-family="{$bodyfont}" color="#000000" linefeed-treatment="preserve" space-after="2mm" padding-top="2mm">
											RETURN TO: <xsl:value-of select="$CompanyAddress"/>
										</fo:block>
									</fo:table-cell>
								</fo:table-row>
							</fo:table-body>
						</fo:table>

					</fo:block>

				</fo:block>
			</fo:static-content>
			<fo:flow flow-name="xsl-region-body">
				<xsl:if test="$DocLogo!=''">
					<fo:block-container position="absolute" top="0cm" right="0cm" left="0.5cm" height="10.7cm" width="10cm">
						<fo:block>
							<fo:external-graphic src="{$DocLogo}"></fo:external-graphic>
						</fo:block>
					</fo:block-container>
				</xsl:if>
				<fo:block-container position="absolute" top="0cm" right="0cm" left="10.5cm" height="10.7cm" width="9cm">
					<fo:block>
						<fo:block font-size="14pt" text-align="right" font-family="{$bodyfont}" color="#000000" linefeed-treatment="preserve" space-after="2mm" padding-top="2mm">
							<xsl:value-of select="$CompanyName"/>
						</fo:block>
					</fo:block>
				</fo:block-container>
				<fo:block-container position="absolute" top="1.0cm" right="0cm" left="11.5cm" height="10.7cm" width="8cm">
					<fo:block>
						<fo:block font-size="10pt" text-align="right" font-family="{$bodyfont}" color="#000000" linefeed-treatment="preserve" space-after="2mm" padding-top="2mm">
							<xsl:value-of select="$CompanyAddress"/>
						</fo:block>
						<fo:block font-size="10pt" text-align="right" font-family="{$bodyfont}" color="#000000" linefeed-treatment="preserve" padding-left="5mm">
							tel: <xsl:value-of select="$CompanyTel"/>
						</fo:block>
					</fo:block>
				</fo:block-container>
				<fo:block margin-left="0.5cm" padding-bottom="2.5cm" padding-top="2cm">
					<xsl:apply-templates select="." mode="PageBody"/>
					<xsl:apply-templates select="." mode="documentFooter"/>
				</fo:block>
			</fo:flow>
		</fo:page-sequence>
	</xsl:template>

  <xsl:template match="*" mode="documentFooter">
    <fo:block-container position="absolute" top="26cm" right="0cm" left="0cm" height="1.66cm" width="20cm" border-top-color="#000000" border-top-style="solid" border-top-width="0.2mm"  xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <fo:block-container margin-left="5mm" height="3.66cm" width="20cm">
        <fo:block font-size="13pt" text-align="left" font-family="{$headingfont}" color="#000000" linefeed-treatment="preserve" space-after="2mm" padding-top="2mm">

        </fo:block>
        <fo:block font-size="10pt" text-align="left" font-family="{$bodyfont}" color="#000000" linefeed-treatment="preserve" space-after="2mm" padding-top="2mm">
          <xsl:value-of select="$CompanyAddress"/>
        </fo:block>
        <fo:block font-size="10pt" text-align="left" font-family="{$bodyfont}" color="#000000" linefeed-treatment="preserve" padding-left="5mm">
          tel: <xsl:value-of select="$CompanyTel"/>
        </fo:block>
      </fo:block-container>
    </fo:block-container>
    <fo:block-container position="absolute" top="26.5cm" right="0cm" left="0.5cm" height="3.66cm" width="19cm"  xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <fo:block font-size="8pt" text-align="left" font-family="{$bodyfont}" color="#000000" linefeed-treatment="preserve">
        <fo:block></fo:block>
      </fo:block>
    </fo:block-container>
  </xsl:template>
  
  <xsl:template match="*">

    <xsl:apply-templates select="." mode="documentContainer"/>
    
  </xsl:template>

  <xsl:template match="*" mode="PageBody">
    <fo:block-container xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <fo:block-container position="absolute" top="{position() * 3}.5cm" right="0cm" left="0.5cm" height="2cm" width="19cm">
        <fo:block font-size="16pt" text-align="left" font-family="{$headingfont}" font-weight="bold" color="{$headingcolor}">
          PageBody Not Found for - <xsl:value-of select="name()"/>
        </fo:block>
      </fo:block-container>
    </fo:block-container>
  </xsl:template>

  <xsl:template match="*" mode="PageTitle">
    <fo:title xmlns:fo="http://www.w3.org/1999/XSL/Format">Document</fo:title>
  </xsl:template>

    <xsl:template match="*" mode="transformXhtml">
    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="transformXhtml"/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="p" mode="transformXhtml">
    <fo:block padding-bottom="6pt" xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:apply-templates mode="transformXhtml"/>
    </fo:block>
  </xsl:template>

  <xsl:template match="span" mode="transformXhtml">
    <xsl:apply-templates mode="transformXhtml"/>
  </xsl:template>

  <xsl:template match="strong" mode="transformXhtml">
    <fo:inline font-weight="bold" xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:apply-templates mode="transformXhtml"/>
    </fo:inline>
  </xsl:template>

  <xsl:template match="b" mode="transformXhtml">
    <fo:inline font-weight="bold" xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:apply-templates mode="transformXhtml"/>
    </fo:inline>
  </xsl:template>

  <xsl:template match="i" mode="transformXhtml">
    <fo:inline font-style="italic" xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:apply-templates mode="transformXhtml"/>
    </fo:inline>
  </xsl:template>

  <xsl:template match="em" mode="transformXhtml">
    <fo:inline font-style="italic" xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:apply-templates mode="transformXhtml"/>
    </fo:inline>
  </xsl:template>

  <xsl:template match="sup" mode="transformXhtml">
    <fo:inline vertical-align="super" font-size="8pt" xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:apply-templates/>
    </fo:inline>
  </xsl:template>

  <xsl:template match="sub" mode="transformXhtml">
    <fo:inline vertical-align="sub" font-size="8pt" xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:apply-templates/>
    </fo:inline>
  </xsl:template>

  <xsl:template match="br" mode="transformXhtml">
    <fo:block xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:text> </xsl:text>
    </fo:block>
  </xsl:template>

  <xsl:template match="a" mode="transformXhtml">

  </xsl:template>

  <xsl:template match="img" mode="transformXhtml">
    <fo:block xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <fo:external-graphic src="{$filePath}{@src}"></fo:external-graphic>
      <xsl:text> </xsl:text>
    </fo:block>
  </xsl:template>

  <xsl:template name="formatdate">
    <xsl:param name="date"/>
    <xsl:param name="format"/>
    <xsl:value-of select="ew:formatdate($date,$format)"/>
  </xsl:template>

  <xsl:template match="ul" mode="transformXhtml">
    <fo:list-block xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:apply-templates mode="transformXhtml"/>
    </fo:list-block>
  </xsl:template>

  <xsl:template match="li" mode="transformXhtml">
    <fo:list-item xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <fo:list-item-label xmlns:fo="http://www.w3.org/1999/XSL/Format">
        <fo:block xmlns:fo="http://www.w3.org/1999/XSL/Format">&#x2022;</fo:block>
      </fo:list-item-label>
      <fo:list-item-body padding-left="15pt" xmlns:fo="http://www.w3.org/1999/XSL/Format">
        <fo:block  xmlns:fo="http://www.w3.org/1999/XSL/Format">
          &#160;&#160;&#160;
          <xsl:apply-templates mode="transformXhtml"/>
        </fo:block>
      </fo:list-item-body>
    </fo:list-item>
  </xsl:template>

</xsl:stylesheet>
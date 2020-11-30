<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <xsl:import href="../tools/Functions.xsl"/>
  <xsl:import href="letterhead.xsl"/>
  
  <xsl:output method="xml" indent="yes" omit-xml-declaration="no" encoding="UTF-8"/>


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
      <xsl:for-each select="descendant-or-self::*[name()='Order']">
        <xsl:apply-templates select="." mode="documentPage"/>
		<xsl:apply-templates select="." mode="documentPage"/>
      </xsl:for-each>
    </fo:root>
  </xsl:template>

  <xsl:template match="*" mode="documentFooter">
    
  </xsl:template>


  <xsl:template match="Policy" mode="PageTitle">
    <fo:title xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:text>DeliveryNote-</xsl:text>
      <xsl:value-of select="$page/descendant-or-self::Order[1]/@cartId"/>
    </fo:title>
  </xsl:template>

  <xsl:template match="Contact" mode="AddressBlock">
    <fo:block xmlns:fo="http://www.w3.org/1999/XSL/Format" font-size="12pt" font-family="{$headingfont}">
      <xsl:value-of select="GivenName/node()"/>
    </fo:block>
    <fo:block xmlns:fo="http://www.w3.org/1999/XSL/Format"  font-size="12pt" font-family="{$bodyfont}">
      <xsl:value-of select="Company/node()"/>
    </fo:block>
    <fo:block xmlns:fo="http://www.w3.org/1999/XSL/Format"  font-size="12pt" font-family="{$bodyfont}">
      <xsl:value-of select="Street/node()"/>
    </fo:block>
    <fo:block xmlns:fo="http://www.w3.org/1999/XSL/Format"  font-size="12pt" font-family="{$bodyfont}">
      <xsl:value-of select="City/node()"/>
    </fo:block>
    <fo:block xmlns:fo="http://www.w3.org/1999/XSL/Format"  font-size="12pt" font-family="{$bodyfont}">
      <xsl:value-of select="PostalCode/node()"/>
    </fo:block>
    <fo:block xmlns:fo="http://www.w3.org/1999/XSL/Format"  font-size="12pt" font-family="{$bodyfont}">
      <xsl:value-of select="Country/node()"/>
    </fo:block>

  </xsl:template>


	<xsl:template match="Order" mode="PageBody">
		<xsl:variable name="orderId">
			<xsl:value-of select="$page/descendant-or-self::Order[1]/@cartId"/>
		</xsl:variable>
		<fo:block xmlns:fo="http://www.w3.org/1999/XSL/Format">
			<fo:block padding-top="3.5cm">
				<fo:block font-size="16pt" text-align="left" font-family="{$headingfont}" font-weight="bold" color="{$headingcolor}">
					Delivery Note
				</fo:block>
				<fo:block padding-top="0.3cm">
					<fo:table width="100%">
						<fo:table-column column-width="13cm"/>
						<fo:table-column column-width="6cm"/>
						<fo:table-body>
							<fo:table-row>
								<fo:table-cell>
									<fo:block>
										<xsl:apply-templates select="Contact[@type='Billing Address']" mode="AddressBlock"/>
										<fo:block xmlns:fo="http://www.w3.org/1999/XSL/Format"  font-size="12pt" font-family="{$bodyfont}">
											Tel: <xsl:value-of select="Contact[@type='Billing Address']/Telephone/node()"/>
										</fo:block>
										<fo:block xmlns:fo="http://www.w3.org/1999/XSL/Format"  font-size="12pt" font-family="{$bodyfont}">
											Email: <xsl:value-of select="Contact[@type='Billing Address']/Email/node()"/>
										</fo:block>
									</fo:block>
								</fo:table-cell>
								<fo:table-cell>
									<fo:block>
										<fo:block font-size="12pt" font-family="{$headingfont}">
											Order No.: <xsl:value-of select="@InvoiceRef"/>
										</fo:block>
										<fo:block font-size="12pt" font-family="{$headingfont}">
											Order Date: <xsl:call-template name="formatdate">
												<xsl:with-param name="date" select="@InvoiceDateTime" />
												<xsl:with-param name="format" select="'dd MMM yyyy'" />
											</xsl:call-template>
										</fo:block>
									</fo:block>
								</fo:table-cell>
							</fo:table-row>
						</fo:table-body>
					</fo:table>
				</fo:block>
			</fo:block>

			<fo:block font-size="10pt" font-family="{$headingfont}" padding-top="0.5cm" padding-bottom="1.5cm">
				<fo:block>
					<fo:table>
						<fo:table-column column-width="0.5cm"/>
						<fo:table-column column-width="3.5cm"/>
						<fo:table-column column-width="8cm"/>
						<fo:table-column column-width="2cm"/>
						<fo:table-column column-width="2.5cm"/>
						<fo:table-column column-width="2.5cm"/>
						<fo:table-body start-indent="0mm">
							<fo:table-row border-bottom-color="#000000" >
								<fo:table-cell border-bottom-width="0pt">
								</fo:table-cell>
								<fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
									<fo:block text-align="left" font-weight="bold" color="#000000">Product Code</fo:block>
								</fo:table-cell>
								<fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
									<fo:block text-align="left" font-weight="bold" color="#000000">Product Description</fo:block>
								</fo:table-cell>
								<fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
									<fo:block text-align="right" font-weight="bold" color="#000000">Qty</fo:block>
								</fo:table-cell>
								<fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
									<fo:block text-align="right" font-weight="bold" color="#000000">Price</fo:block>
								</fo:table-cell>
								<fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
									<fo:block text-align="right" font-weight="bold" color="#000000">Line Total</fo:block>
								</fo:table-cell>
							</fo:table-row>
							<xsl:for-each select="Item">
								<fo:table-row border-bottom-color="#000000" border-bottom-width=".25cm" border-bottom-style="solid">
									<fo:table-cell border-bottom-width="0pt">
									</fo:table-cell>
									<fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
										<fo:block text-align="left" color="#000000">
											<xsl:value-of select="productDetail/StockCode/node()"/>
										</fo:block>
									</fo:table-cell>
									<fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
										<fo:block text-align="left" color="#000000">
											<xsl:value-of select="Name/node()"/>
										</fo:block>
									</fo:table-cell>
									<fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
										<fo:block text-align="right" color="#000000">
											<xsl:value-of select="@quantity"/>
										</fo:block>
									</fo:table-cell>
									<fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
										<fo:block text-align="right" color="#000000">
											£ <xsl:value-of select="format-number(@price, '#.00')"/>
										</fo:block>
									</fo:table-cell>
									<fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
										<fo:block text-align="right" color="#000000">
											£ <xsl:value-of select="format-number(@itemTotal, '#.00')"/>
										</fo:block>
									</fo:table-cell>
								</fo:table-row>
							</xsl:for-each>

							<fo:table-row border-bottom-color="#000000" border-bottom-width=".25cm" border-bottom-style="solid">
								<fo:table-cell border-bottom-width="0pt">
								</fo:table-cell>
								<fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
									<fo:block text-align="left" color="#000000">
										Shipping:
									</fo:block>
								</fo:table-cell>
								<fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
									<fo:block text-align="left" color="#000000">
										<xsl:value-of select="@shippingDesc"/>
									</fo:block>
								</fo:table-cell>
								<fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
									<fo:block text-align="right" color="#000000">
									</fo:block>
								</fo:table-cell>
								<fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
									<fo:block text-align="right" color="#000000">
									</fo:block>
								</fo:table-cell>
								<fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
									<fo:block text-align="right" color="#000000">
										£ <xsl:value-of select="format-number(@shippingCost, '#.00')"/>
									</fo:block>
								</fo:table-cell>
							</fo:table-row>

							<fo:table-row border-bottom-color="#000000" border-bottom-width=".25cm" border-bottom-style="solid">
								<fo:table-cell border-bottom-width="0pt">
								</fo:table-cell>
								<fo:table-cell padding="6pt" border-bottom="1pt solid black" border-top="1pt solid black">
									<fo:block text-align="left" color="#000000" font-weight="700">
										Total Cost:
									</fo:block>
								</fo:table-cell>
								<fo:table-cell padding="6pt" border-bottom="1pt solid black" border-top="1pt solid black">
									<fo:block text-align="left" color="#000000">
									</fo:block>
								</fo:table-cell>
								<fo:table-cell padding="6pt" border-bottom="1pt solid black" border-top="1pt  solid black">
									<fo:block text-align="right" color="#000000">
									</fo:block>
								</fo:table-cell>
								<fo:table-cell padding="6pt" border-bottom="1pt solid black" border-top="1pt solid black">
									<fo:block text-align="right" color="#000000">
									</fo:block>
								</fo:table-cell>
								<fo:table-cell padding="6pt" border-bottom="1pt solid black" border-top="1pt solid black">
									<fo:block text-align="right" color="#000000" font-weight="700">
										£ <xsl:value-of select="format-number(@total, '#.00')"/>
									</fo:block>
								</fo:table-cell>
							</fo:table-row>

						</fo:table-body>
					</fo:table>
				</fo:block>
			</fo:block>

			<fo:block>
				<fo:block font-size="10pt" font-family="{$bodyfont}" margin-bottom="0.5pt" space-after="2mm" margin-right="1cm">
					Any shortages or damages must be reported to us within 4 days of delivery. Goods must have been signed for as unchecked if any damage or tampering is
					visible to outer packaging. All goods remain the property of <xsl:value-of select="$CompanyName"/> until paid for in full. Tel: <xsl:value-of select="$CompanyTel"/>, Email: <xsl:value-of select="$CompanyEmail"/>
				</fo:block>
				<fo:block font-size="10pt" font-family="{$bodyfont}" space-after="2mm">
				</fo:block>
			</fo:block>
			<fo:block>
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
						</fo:table-row>
					</fo:table-body>
				</fo:table>

			</fo:block>
		</fo:block>
	</xsl:template>
</xsl:stylesheet>
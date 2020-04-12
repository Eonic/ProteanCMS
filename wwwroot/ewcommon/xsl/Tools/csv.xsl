<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet">
	<xsl:output method="text" indent="yes" omit-xml-declaration="yes" encoding="utf-8"/>
	
  <xsl:template match="Page">

		<xsl:for-each select="ContentDetail/Report | ContentDetail/Content[@type='Report']/Report"">
			<xsl:apply-templates select="." mode="reportTitles"/>
			<xsl:apply-templates select="Item" mode="reportRow"/>
		</xsl:for-each>

	</xsl:template>
	

	<xsl:template match="*"  mode="reportTitles">
		<xsl:for-each select="Item[1]/*">
      <xsl:text></xsl:text>
			<xsl:value-of select="name()"/>
			<xsl:text></xsl:text>
      <xsl:if test="not(position() = last())">
        <xsl:text>,</xsl:text>
      </xsl:if>
    </xsl:for-each>
    <xsl:text>&#10;</xsl:text> 
	</xsl:template>

	<xsl:template match="*" mode="reportRow">
		<xsl:for-each select="*">
      <xsl:text>"</xsl:text>
			<xsl:value-of select="node()"/>
			<xsl:text>"</xsl:text>
      <xsl:if test="not(position() = last())">
        <xsl:text>,</xsl:text>
      </xsl:if>
    </xsl:for-each>
    <xsl:text>&#10;</xsl:text> 
	</xsl:template>
	
	<xsl:template match="Report[@cReportType='CartDownload']" mode="reportTitles">
		<xsl:text>"OrderDate",</xsl:text>
		<xsl:text>"OrderReference",</xsl:text>
		<xsl:text>"Customer Name",</xsl:text>
		<xsl:text>"Bill Address",</xsl:text>
		<xsl:text>"Billing Zip",</xsl:text>
		<xsl:text>"Billing Town",</xsl:text>
		<xsl:text>"Billing Country",</xsl:text>
		<xsl:text>"Delivery Address",</xsl:text>
		<xsl:text>"Delivery Zip",</xsl:text>
		<xsl:text>"Delivery Town",</xsl:text>
		<xsl:text>"Delivery Country",</xsl:text>
		<xsl:text>"Email",</xsl:text>
		<xsl:text>"Shipping Cost",</xsl:text>
    <xsl:text>"Order Total (Net)",</xsl:text>
    <xsl:text>"Order VAT",</xsl:text>
    <xsl:text>"Order Total (Gross)",</xsl:text>
		<xsl:text>"PromoCode",</xsl:text>
		<xsl:text>"OrderLineNo",</xsl:text>
		<xsl:text>"StockCode",</xsl:text>
		<xsl:text>"Description",</xsl:text>
		<xsl:text>"Quantity",</xsl:text>
		<xsl:text>"Unit Price",</xsl:text>
		<xsl:text>"Net (line)",</xsl:text>
		<xsl:text>"Discount (line)"</xsl:text>
		<xsl:text>&#10;</xsl:text>
	</xsl:template>

	<xsl:template match="Item[parent::Report[@cReportType='CartDownload']]" mode="reportRow">
		<xsl:for-each select="cCartXml/Order/Item">
			<xsl:variable name="order" select="parent::Order"/>
			<xsl:variable name="vatRate" select="$order/@vatRate"/>
			<xsl:variable name="item" select="ancestor::Item"/>
			<xsl:variable name="billAddr" select="ancestor::Order/Contact[@type='Billing Address']"/>
			<xsl:variable name="delAddr" select="ancestor::Order/Contact[@type='Delivery Address']"/>
			<xsl:variable name="unitPrice" select="@originalPrice"/>
			<xsl:variable name="linePrice" select="@itemTotal"/>
			<xsl:variable name="lineDiscount" select="@discount"/>

			<xsl:text>"</xsl:text>
			<xsl:value-of select="$order/@InvoiceDate"/>
			<xsl:text>",</xsl:text>
			<xsl:text>"</xsl:text>
			<xsl:value-of select="$order/@InvoiceRef"/>
			<xsl:text>",</xsl:text>
			<xsl:text>"</xsl:text>
			<xsl:value-of select="$billAddr/GivenName"/>
			<xsl:text>",</xsl:text>
			<xsl:text>"</xsl:text>
			<xsl:value-of select="$billAddr/Street"/>
			<xsl:text>",</xsl:text>
			<xsl:text>"</xsl:text>
			<xsl:value-of select="$billAddr/PostalCode"/>
			<xsl:text>",</xsl:text>
			<xsl:text>"</xsl:text>
			<xsl:value-of select="$billAddr/City"/>
			<xsl:text>",</xsl:text>
			<xsl:text>"</xsl:text>
			<xsl:value-of select="$billAddr/Country"/>
			<xsl:text>",</xsl:text>
			<xsl:text>"</xsl:text>
			<xsl:value-of select="$delAddr/Street"/>
			<xsl:text>",</xsl:text>
			<xsl:text>"</xsl:text>
			<xsl:value-of select="$delAddr/PostalCode"/>
			<xsl:text>",</xsl:text>
			<xsl:text>"</xsl:text>
			<xsl:value-of select="$delAddr/City"/>
			<xsl:text>",</xsl:text>
			<xsl:text>"</xsl:text>
			<xsl:value-of select="$delAddr/Country"/>
			<xsl:text>",</xsl:text>
			<xsl:text>"</xsl:text>
			<xsl:value-of select="$billAddr/Email"/>
			<xsl:text>",</xsl:text>
			<xsl:text>"</xsl:text>
			<xsl:value-of select="$order/@shippingCost"/>
			<xsl:text>",</xsl:text>
      <xsl:text>"</xsl:text>
      <xsl:value-of select="$order/@totalNet"/>
      <xsl:text>",</xsl:text>
      <xsl:text>"</xsl:text>
      <xsl:value-of select="$order/@vatAmt"/>
      <xsl:text>",</xsl:text>
			<xsl:text>"</xsl:text>
			<xsl:value-of select="$order/@total"/>
			<xsl:text>",</xsl:text>
			<xsl:text>"</xsl:text>
			<xsl:value-of select="$order/Notes/PromotionalCode"/>
			<xsl:text>",</xsl:text>
			<xsl:text>"</xsl:text>
			<xsl:value-of select="position()"/>
			<xsl:text>",</xsl:text>
			<xsl:text>"</xsl:text>
			<xsl:value-of select="@ref"/>
			<xsl:text>",</xsl:text>
			<xsl:text>"</xsl:text>
			<xsl:value-of select="Name"/>
			<xsl:text>",</xsl:text>
			<xsl:text>"</xsl:text>
			<xsl:value-of select="@quantity"/>
			<xsl:text>",</xsl:text>
			<xsl:text>"</xsl:text>
			<xsl:value-of select="$unitPrice"/>
			<xsl:text>",</xsl:text>
			<xsl:text>"</xsl:text>
			<xsl:value-of select="$linePrice"/>
			<xsl:text>",</xsl:text>
			<xsl:text>"</xsl:text>
			<xsl:value-of select="$lineDiscount"/>
			<xsl:text>"</xsl:text>
			<xsl:text>&#xD;</xsl:text>
		</xsl:for-each>
	</xsl:template>
	
	
</xsl:stylesheet>

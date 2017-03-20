<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt">
	<xsl:import href="Report-Base.xsl"/>
	<xsl:import href="Formats/Report-Format-Loader.xsl"/>
	
	<xsl:template match="/">
		<xsl:apply-imports/>
	</xsl:template>

	<!-- Order download report hardcodes the order of columns -->
	<xsl:template match="Report" mode="reportHeaders">
		<xsl:variable name="orderDownloadHeader">
			<Item>
				<OrderDate/>
				<OrderReference/>
				<Customer_Name/>
				<Billing_Address/>
				<Billing_Zip/>
				<Billing_Town/>
				<Billing_Country/>
				<Delivery_Address/>
				<Delivery_Zip/>
				<Delivery_Town/>
				<Delivery_Country/>
				<Email/>
				<Shipping_Cost/>
				<Order_Total_Net label="Order Total (Net)"/>
				<Order_VAT/>
				<Order_Total_Gross label="Order Total (Gross)"/>
				<PromoCode/>
				<OrderLineNo/>
				<StockCode/>
				<Description/>
				<Quantity/>
				<Unit_Price/>
				<Net_Line label="Net (line)"/>
				<Discount_Line label="Discount (line)"/>
			</Item>
		</xsl:variable>
		<xsl:apply-templates select="ms:node-set($orderDownloadHeader)" mode="reportHeaderRow"/>
	</xsl:template>



	<!-- Order download actually makes each line item a row (not each order) -->
	<xsl:template match="Report" mode="reportRow">
		<xsl:apply-templates select="Item/cCartXml/Order/Item" mode="reportRow"/>
	</xsl:template>
	
	
	<!-- Order download determine the order of the cells-->
	<!-- ROW CELL CHOOSER -->
	<xsl:template match="Item" mode="reportRowCellFilter">
		<xsl:variable name="order" select="parent::Order"/>
		<xsl:variable name="vatRate" select="$order/@vatRate"/>
		<xsl:variable name="item" select="ancestor::Item"/>
		<xsl:variable name="billAddr" select="ancestor::Order/Contact[@type='Billing Address']"/>
		<xsl:variable name="delAddr" select="ancestor::Order/Contact[@type='Delivery Address']"/>
		<xsl:variable name="unitPrice" select="@originalPrice"/>
		<xsl:variable name="linePrice" select="@itemTotal"/>
		<xsl:variable name="lineDiscount" select="@discount"/>

		<xsl:variable name="orderItem">
			<Item>
				<OrderDate>
					<xsl:value-of select="$order/@InvoiceDate"/>
				</OrderDate>
				<OrderReference>
					<xsl:value-of select="$order/@InvoiceRef"/>
				</OrderReference>
				<Customer_Name>
					<xsl:value-of select="$billAddr/GivenName"/>
				</Customer_Name>
				<Billing_Address>
					<xsl:value-of select="$billAddr/Street"/>
				</Billing_Address>
				<Billing_Zip>
					<xsl:value-of select="$billAddr/PostalCode"/>
				</Billing_Zip>
				<Billing_Town>
					<xsl:value-of select="$billAddr/City"/>
				</Billing_Town>
				<Billing_Country>
					<xsl:value-of select="$billAddr/Country"/>
				</Billing_Country>

				<Delivery_Address>
					<xsl:value-of select="$delAddr/Street"/>
				</Delivery_Address>
				<Delivery_Zip>
					<xsl:value-of select="$delAddr/PostalCode"/>
				</Delivery_Zip>
				<Delivery_Town>
					<xsl:value-of select="$delAddr/City"/>
				</Delivery_Town>
				<Delivery_Country>
					<xsl:value-of select="$delAddr/Country"/>
				</Delivery_Country>
				<Email>
					<xsl:value-of select="$billAddr/Email"/>
				</Email>
				<Shipping_Cost>
					<xsl:value-of select="$order/@shippingCost"/>
				</Shipping_Cost>
				<Order_Total_Net label="Order Total (Net)">
					<xsl:value-of select="$order/@totalNet"/>
				</Order_Total_Net>

				<Order_VAT>
					<xsl:value-of select="$order/@vatAmt"/>
				</Order_VAT>
				<Order_Total_Gross label="Order Total (Gross)">
					<xsl:value-of select="$order/@total"/>
				</Order_Total_Gross>
				<PromoCode>
					<xsl:value-of select="$order/Notes/PromotionalCode"/>
				</PromoCode>
				<OrderLineNo>
					<xsl:value-of select="position()"/>
				</OrderLineNo>
				<StockCode>
					<xsl:value-of select="@ref"/>
				</StockCode>
				<Description>
					<xsl:value-of select="Name"/>
				</Description>
				<Quantity>
					<xsl:value-of select="@quantity"/>
				</Quantity>
				<Unit_Price>
					<xsl:value-of select="$unitPrice"/>
				</Unit_Price>
				<Net_Line label="Net (line)">
					<xsl:value-of select="$linePrice"/>
				</Net_Line>
				<Discount_Line label="Discount (line)">
					<xsl:value-of select="$lineDiscount"/>
				</Discount_Line>
			</Item>
		</xsl:variable>
		<xsl:apply-templates select="ms:node-set($orderItem)/*/*" mode="reportCell"/>
	</xsl:template>
	
	
</xsl:stylesheet>

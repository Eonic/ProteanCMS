<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
	<xsl:template match="/">
		<xsl:apply-imports/>
	</xsl:template>
	<xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/>
	<xsl:template match="/Order">
		<html>
			<head>

<style>

body
{font-family:Verdana, Arial, Helvetica, sans-serif;background:#fff;margin:0;color:#666;font-size:70%;}
h1, h2
{font-weight:normal;color:#c3be8d;}
h1
{padding:31px 0;text-transform:lowercase;margin:0 200px 0 0;font-size:220%;}
h2
{margin:1em 0;font-size:130%;}
p
{margin:0 0 1em;}
td, th
{line-height:1.7;text-align:left;padding:3px;}
th.heading
{color:#FFF;background:#999;border-top:1px solid #666;border-bottom:1px solid #666;}
th.lineTotal
{color:#000;background:#ddd;white-space:nowrap;}
td.cell, td.amount
{border-bottom:1px solid #666;}
td.heading, .price, .lineTotal, .amount
{text-align:right;}
td.amount
{background:#ddd;}
.label
{font-weight:bold;}
.orderAddress
{border:1px solid #ccc;padding:0 10px 20px;margin-bottom:20px;}
.terminus
{clear:both;line-height:0;}
.total
{font-weight:700;}
#address
{font-size:90%;color:#c3be8d;text-align:center;padding-top:30px;border-top:1px solid #ccc;}
#billingAddress
{width:275px;float:left;}
#deliveryAddress
{margin-left:305px;}
#logo
{float:right;padding-top:27px;padding-right:10px;}
#mainHeader
{margin-bottom:20px;border-bottom:1px solid #ccc;}
#mainTable
{margin:0 auto;width:600px;padding:0 20px 20px;}
#orderDetails
{margin-bottom:20px;}
table#orderListing
{width:100%;border:none;margin-bottom:20px;}


</style>

			</head>
		<body>
		<div id="mainTable">
			<div id="mainHeader">
				<img src="http://ewhite.uklive.eonic.co.uk/images/layout/receipt_header.gif" alt="Essentially White" id="logo"/>
				<h1>Your Order Acknowledgment from Essentially White</h1>
			</div>
			<div id="orderDetails">
				<span class="label">Reference:&#160;</span><xsl:value-of select="@InvoiceRef" /><br/>
				<span class="label">Order Total:&#160;</span>£ <xsl:value-of select="format-number(@total, '0.00')" /><br/>
				<span class="label">Date:&#160;</span><xsl:value-of select="@InvoiceDate" /><br/>
			</div>
			<div id="billingAddress" class="orderAddress"><xsl:apply-templates select="Contact[@type='Billing Address']" mode="cart"/></div>
			<div id="deliveryAddress" class="orderAddress"><xsl:apply-templates select="Contact[@type='Delivery Address']" mode="cart"/></div>
			<div class="terminus"></div>
			<table cellspacing="0" id="orderListing" summary="This table contains a list of the items which you have bought.">
				<tr>
					<th class="heading qty">Qty</th>
					<th class="heading description">Description</th>
					<th class="heading ref">Ref</th>
					<th class="heading price">Price</th>
					<th class="heading lineTotal">Line Total</th>
				</tr>
				<xsl:for-each select="CartItem">
					<tr>	
						<td class="cell qty"><xsl:value-of select="@quantity"/></td>
						<td class="cell description"><xsl:value-of select="node()"/>&#160;<xsl:value-of select="@option1"/>&#160;<xsl:value-of select="@option2"/></td>	
						<td class="cell ref"><xsl:value-of select="@ref"/></td>	
						<td class="cell price">£ <xsl:value-of select="format-number(@price,'#.00')"/></td>	
						<td class="cell lineTotal">£ <xsl:value-of select="format-number(@price * @quantity,'#.00')"/></td>
					</tr>
				</xsl:for-each>
					<xsl:if test="@shippingCost &gt; 0">
						<tr>
							<td colspan="3">&#160;</td>
							<td class="shipping heading">
								<xsl:choose>
									<xsl:when test="/Page/Contents/Content[@name='shippingCostLabel']!=''">
										<xsl:value-of select="/Page/Contents/Content[@name='shippingCostLabel']" />
									</xsl:when>
									<xsl:otherwise>Shipping Cost:</xsl:otherwise>
								</xsl:choose>
							</td>
							<td class="shipping amount">£ <xsl:value-of select="format-number(@shippingCost,'0.00')" /></td>
						</tr>
					</xsl:if>
					<tr>
						<td colspan="3" rowspan="3">&#160;</td>
						<td class="subTotal heading">Sub Total:</td>
						<td class="subTotal amount">£ <xsl:value-of select="format-number(@totalNet, '0.00')" /></td>
					</tr>
					<xsl:if test="@vatRate &gt; 0">
						<tr>
							<td class="vat heading">
								<xsl:choose>
									<xsl:when test="//Cart/Contact/Address/Country='United Kingdom'">VAT at </xsl:when>
									<xsl:otherwise>Tax at </xsl:otherwise>
								</xsl:choose>
								<xsl:value-of select="format-number(@vatRate, '#.00')" />%:
							</td>
							<td class="vat amount">£ <xsl:value-of select="format-number(@vatAmt, '0.00')" /></td>
						</tr>
					</xsl:if>
					<tr>
						<td class="total heading">Total Value:</td>
						<td class="total amount">£ <xsl:value-of select="format-number(@total, '0.00')" /></td>
					</tr>
				</table>
				<xsl:if test="/Page/Contents/Content[@name='cartMessage']">
					<div class="orderMessage">
						<xsl:copy-of select="/Page/Contents/Content[@name='cartMessage']/node()" />
					</div>
				</xsl:if>
				<p>Once the goods are despatched you will be advised by email and given a shipment number to enable you to 'Track your parcel' on the Internet.</p>
				<p>V.A.T. invoice will be sent to the Billing Address.</p>
				<p id="address">Essentially White Ltd, 1 Ford Place Cottages, Ford Lane, Wrotham Heath, Sevenoaks, Kent, TN15 7SE, UK</p>
			</div>
	</body>
</html>	
	</xsl:template>
	<!-- -->
	<xsl:template match="Contact" mode="cart">
		<h2><xsl:value-of select="@type"/> Details</h2>
		<xsl:value-of select="GivenName"/><br/>
		<xsl:if test="Company/node()!=''"><xsl:value-of select="Company"/><br/></xsl:if>
		<xsl:value-of select="Address/Street"/><br/>
		<xsl:value-of select="Address/City"/><br/>
		<xsl:if test="Address/State/node()!=''"><xsl:value-of select="Address/State"/><br/></xsl:if>
		<xsl:value-of select="Address/PostalCode"/><br/>
		<xsl:if test="Address/Country/node()!=''"><xsl:value-of select="Address/Country"/><br/></xsl:if>
		<span class="label">Tel: </span><xsl:value-of select="Telephone"/><br/>
		<xsl:if test="Fax/node()!=''"><span class="label">Fax: </span><xsl:value-of select="Fax"/><br/></xsl:if>
		<span class="label">Email: </span><xsl:value-of select="Email"/><br/>
	</xsl:template>
	<!-- -->
</xsl:stylesheet>
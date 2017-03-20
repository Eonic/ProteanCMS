<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
	<xsl:template match="/">
		<xsl:apply-imports/>
	</xsl:template>
	<xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN" doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd" encoding="UTF-8"/>
	<xsl:template match="/Page/Contents/Cart">
		<html>
			<head>

<style>

body
{font-family:Verdana, Arial, Helvetica, sans-serif;background:#fff;margin:0;color:#666;font-size:70%;}
a
{color:#c3be8d;text-decoration:none;}
a:hover
{text-decoration:underline;}
#mainTable
{margin:0 auto;width:580px;padding:0 20px 20px;}
#mainHeader
{margin-bottom:20px;border-bottom:1px solid #ccc;}
h1, h2
{font-weight:normal;color:#c3be8d;}
h1
{text-transform:lowercase;margin:0 150px 0 0;line-height:100px;font-size:220%;}
h2
{margin:1em 0;font-size:130%;}
#logo
{float:right;padding-top:17px;padding-right:10px;}
#orderDetails
{margin-bottom:20px;}
.orderAddress
{border:1px solid #ccc;padding:0 10px 20px;margin-bottom:20px;}
#billingAddress
{width:270px;float:left;}
#deliveryAddress
{margin-left:300px;}
.label
{font-weight:bold;}
.terminus
{clear:both;line-height:0;}
table#orderListing
{width:100%;border:none;margin-bottom:10px;}
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
.total
{font-weight:700;}

</style>

			</head>
		<body>
		<div id="mainTable">
			<div id="mainHeader">
				<h1>
					Reminder for Outstanding Balance
				</h1>
				<p>
					This e-mail is a reminder from <a href="http://{/Page/@siteName}" title="Click to visit website"><xsl:value-of select="/Page/@siteName"/></a>.
					The invoice below has an outstanding balance left to be paid. 
					 A reference number for this settlement is listed below: to settle the balance, please click on the link below. 
					 If you have any queries, pelase call for assistance.
				</p>
			</div>
			<div id="orderDetails">
				<span class="label">Reference:&#160;</span><xsl:value-of select="@invoiceRef" /><br/>
				<span class="label">Date:&#160;</span><xsl:call-template name="Mon_DD_YYYY"><xsl:with-param name="date" select="@invoiceDate" /></xsl:call-template><br/>
				<xsl:if test="@AccountID"><strong class="payonAccount"><span class="label">Paid on Account ID:&#160;</span><xsl:value-of select="@AccountID" /></strong><br/></xsl:if>
				<xsl:if test="AdditionalComments"><strong><span class="label">Additional Comments:</span></strong><br/><xsl:value-of select="AdditionalComments" /><br/></xsl:if>
				<span class="label">Order Total:&#160;</span><xsl:value-of select="@currency"/><xsl:value-of select="format-number(@total, '#.00')" /><br/>				
				<xsl:if test="@payableType='deposit' and (@payableAmount &gt; 0)">
					<span class="label">Payment Received:&#160;</span><xsl:value-of select="@currency"/><xsl:value-of select="format-number(@paymentMade,'0.00')" /><br/>
					<span class="label">Payment Outstanding:&#160;</span><strong><xsl:value-of select="@currency"/><xsl:value-of select="format-number(@payableAmount,'0.00')" /></strong><br/>
					<span class="label">Settlement Reference:&#160;</span><strong><a href="http://{/Page/@siteName}/?ewSettlement={@settlementID}" title="Click to pay the outstanding balance."><xsl:value-of select="@settlementID" /></a></strong><br/><br/>
					<p>Please note your Settlement Reference, above. To pay the outstanding balance please click the following link:</p>
					<ul><li><a href="http://{/Page/@siteName}/?ewSettlement={@settlementID}" title="Click to pay the outstanding balance.">http://<xsl:value-of select="/Page/@siteName" />/?ewSettlement=<xsl:value-of select="@settlementID" /></a></li></ul>
					<p>Please keep this e-mail, as the link above is your means of settling the outstanding balance. If you have any queries, please call for assistance.</p>		
				</xsl:if>							
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
						<td class="cell price"><xsl:value-of select="@currency"/> <xsl:value-of select="format-number(@price,'#.00')"/></td>	
						<td class="cell lineTotal"><xsl:value-of select="@currency"/> <xsl:value-of select="format-number(@price * @quantity,'#.00')"/></td>
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
							<td class="shipping amount">
								<xsl:value-of select="@currency" />
								<xsl:value-of select="format-number(@shippingCost,'0.00')" />
							</td>
						</tr>
					</xsl:if>
					<tr>
						<td colspan="3" rowspan="5">&#160;</td>
						<td class="subTotal heading">Sub Total:</td>
						<td class="subTotal amount">
							<xsl:value-of select="@currency" />
							<xsl:value-of select="format-number(@totalNet, '0.00')" />
						</td>
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
							<td class="vat amount">
								<span class="currency">
									<xsl:value-of select="@currency" />
								</span>
								<xsl:value-of select="format-number(@vatAmt, '0.00')" />
							</td>
						</tr>
					</xsl:if>
					<tr>
						<td class="subTotal heading">Payment Received:</td>
						<td class="subTotal amount">
							<xsl:value-of select="@currency" />
							<xsl:value-of select="format-number(@paymentMade, '0.00')" />
						</td>
					</tr>
					<tr>
						<td class="total heading">Amount Outstanding:</td>
						<td class="total amount">
							<strong><xsl:value-of select="@currency" />
							<xsl:value-of select="format-number(@payableAmount, '0.00')" /></strong>
						</td>
					</tr>										
					<tr>
						<td class="subTotal heading">Total Value:</td>
						<td class="subTotal amount">
							<xsl:value-of select="@currency" />
							<xsl:value-of select="format-number(@total, '0.00')" />
						</td>
					</tr>
				</table>
				<xsl:if test="/Page/Contents/Content[@name='cartMessage']">
					<div class="orderMessage">
						<xsl:copy-of select="/Page/Contents/Content[@name='cartMessage']/node()" />
					</div>
				</xsl:if>
			<div><xsl:value-of select="/Page/Contents/Content[@name='cartTerms']"/></div>
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

	<xsl:template name="Mon_DD_YYYY">
		<xsl:param name="date"/>
		<xsl:variable name="month" select="number(substring($date, 6, 2))"/>
		<xsl:value-of select="number(substring($date, 9, 2))"/>&#160;<xsl:choose>
			<xsl:when test="$month=1">January</xsl:when>
			<xsl:when test="$month=2">February</xsl:when>
			<xsl:when test="$month=3">March</xsl:when>
			<xsl:when test="$month=4">April</xsl:when>
			<xsl:when test="$month=5">May</xsl:when>
			<xsl:when test="$month=6">June</xsl:when>
			<xsl:when test="$month=7">July</xsl:when>
			<xsl:when test="$month=8">August</xsl:when>
			<xsl:when test="$month=9">September</xsl:when>
			<xsl:when test="$month=10">October</xsl:when>
			<xsl:when test="$month=11">November</xsl:when>
			<xsl:when test="$month=12">December</xsl:when>
			<xsl:otherwise>INVALID MONTH</xsl:otherwise>
		</xsl:choose>&#160;<xsl:value-of select="substring($date, 1, 4)"/>
	</xsl:template>

</xsl:stylesheet>
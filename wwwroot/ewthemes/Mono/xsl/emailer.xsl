<?xml version="1.0" encoding="UTF-8"?>
	<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html" indent="yes"/>
	<xsl:template match="/">
		<xsl:apply-imports/>
	</xsl:template>
	<xsl:template match="*">
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
		<h2>Email message from the Valet Pro  Website...</h2>
						<xsl:for-each select="*">
			
				<b><xsl:value-of select="name()"/></b> - <xsl:value-of select="node()"/> <br/>
			
			</xsl:for-each>
			</div>
	</body>
</html>	
	
	</xsl:template>
</xsl:stylesheet>
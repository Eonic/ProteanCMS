<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
    <xsl:output method="xml" indent="yes"/>

  <xsl:template  match="Order[@cmd='ShowInvoice' or @cmd='ShowCallbackInvoice']"  mode="orderProcess">
    <InvoiceMailingList>
      <OrderDate><xsl:value-of  select="@InvoiceDate"/></OrderDate>
      <InvoiceRef><xsl:value-of select="@InvoiceRef"/></InvoiceRef>
      <Total> <xsl:value-of select="format-number(@total, '0.00')"/></Total>
      <ShippingDesc><xsl:value-of select="/Page/Cart/Order/@shippingDesc"/></ShippingDesc>
      <ShippingCost>  <xsl:value-of select="@shippingCost"/></ShippingCost>
      <xsl:if test="Contact[@type='Billing Address']">
        <BillingName><xsl:value-of select="Contact[@type='Billing Address']/GivenName/"/></BillingName>
        <BillingEmail><xsl:value-of select="Contact[@type='Billing Address']/Email"/></BillingEmail>
        <BillingMobile> <xsl:value-of select="Contact[@type='Billing Address']/Telephone"/></BillingMobile>
        <BillingAddress> <xsl:value-of select="Contact[@type='Billing Address']/Street"/></BillingAddress>
        <BillingCity><xsl:value-of select="Contact[@type='Billing Address']/City"/></BillingCity>
       <BillingPostCode><xsl:value-of select="Contact[@type='Billing Address']/PostCode"/></BillingPostCode>
       <BillingCountry><xsl:value-of select="Contact[@type='Billing Address']/Country"/></BillingCountry>
      </xsl:if>
        <xsl:if test="Contact[@type='Delivery Address']">
        <DeliveryName><xsl:value-of select="Contact[@type='Delivery Address']/GivenName"/></DeliveryName>
        <DeliveryStreet><xsl:value-of select="Contact[@type='Delivery Address']/Street"/></DeliveryStreet>
        <DeliveryCity> <xsl:value-of select="Contact[@type='Delivery Address']/City"/></DeliveryCity>
        <DeliveryPostCode> <xsl:value-of select="Contact[@type='Delivery Address']/PostalCode"/></DeliveryPostCode>
        <DeliveryCountry><xsl:value-of select="Contact[@type='Delivery Address']/Country"/></DeliveryCountry>
        </xsl:if>
    <xsl:for-each select="Order/Item">
     <div><xsl:value-of  select="@contentId"/></div>
     <div><xsl:value-of  select="@url"/></div>
      <div><xsl:value-of  select="@price"/></div>
     <div><xsl:value-of  select="@quantity"/></div>
      <div><xsl:value-of  select="@itemTotal"/><</div>
    </xsl:for-each>
     <div><xsl:value-of  select="Order/PaymentDetails/@contentId"/></div>
      <div><xsl:value-of  select="Order/PaymentDetails/@ref"/></div>
      <div><xsl:value-of  select="Order/PaymentDetails/@acct"/></div>
     <div><xsl:value-of  select="Order/PaymentDetails/Response/@acct"/></div>
     <div><xsl:value-of  select="Order/PaymentDetails/Response/@AmountPaid"/></div>
    </InvoiceMailingList>
  </xsl:template>
</xsl:stylesheet>

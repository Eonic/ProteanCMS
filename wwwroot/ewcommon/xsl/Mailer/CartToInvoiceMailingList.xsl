<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes"
                xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew"
                xmlns:v-for="https://vuejs.org/v2/api/v-for" xmlns:v-if="http://example.com/xml/v-if"
                xmlns:v-bind="http://example.com/xml/v-bind" xmlns:v-on="http://example.com/xml/v-on">

  <xsl:template  match="Cart/Order">
    <InvoiceMailingList>
      <OrderDate><xsl:value-of  select="@InvoiceDate"/></OrderDate>
      <InvoiceRef><xsl:value-of select="@InvoiceRef"/></InvoiceRef>
      <Total> <xsl:value-of select="format-number(@total, '0.00')"/></Total>
      <ShippingDesc><xsl:value-of select="@shippingDesc"/></ShippingDesc>
      <ShippingCost>  <xsl:value-of select="@shippingCost"/></ShippingCost>
      <xsl:if test="Contact[@type='Billing Address']">
        <BillingName><xsl:value-of select="Contact[@type='Billing Address']/GivenName"/></BillingName>
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
        <ProductID><xsl:value-of  select="@contentId"/></ProductID>
     <ProductUrl><xsl:value-of  select="@url"/></ProductUrl>
      <ProdctPrice><xsl:value-of  select="@price"/></ProdctPrice>
     <ProductQuantity><xsl:value-of  select="@quantity"/></ProductQuantity>
      <ProductTotal><xsl:value-of  select="@itemTotal"/></ProductTotal>
    </xsl:for-each>
     <PaymentId><xsl:value-of  select="PaymentDetails/@contentId"/></PaymentId>
      <PaymentRef><xsl:value-of  select="PaymentDetails/@ref"/></PaymentRef>
      <PaymentAcct><xsl:value-of  select="PaymentDetails/@acct"/></PaymentAcct>
      <PaymentAmountPaid><xsl:value-of  select="PaymentDetails/Response/@AmountPaid"/></PaymentAmountPaid>
    </InvoiceMailingList>
  </xsl:template>
</xsl:stylesheet>

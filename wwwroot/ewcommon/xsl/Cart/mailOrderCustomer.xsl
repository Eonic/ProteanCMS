<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:import href="../Email/emailStationary.xsl"/>
  <xsl:import href="Cart.xsl"/>
  <xsl:import href="Email/emailCart.xsl"/>

  <xsl:template match="/">
    <xsl:apply-imports/>
  </xsl:template>
  <xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/>

  <xsl:template match="*" mode="subject">
    Your order from <xsl:value-of select="$siteTitle"/> - Ref: <xsl:value-of select="Order/@InvoiceRef" />
  </xsl:template>
  
  <xsl:template match="*" mode="pageTitle">
    Your order from <xsl:value-of select="$siteTitle"/>
  </xsl:template>

  <xsl:template match="Order" mode="bodyLayout">
    <xsl:variable name="currency">
      <xsl:choose>
      <xsl:when test="ancestor::Cart/@currencySymbol!=''">
        <xsl:value-of select="ancestor::Cart/@currencySymbol"/>
      </xsl:when>
      <xsl:otherwise>£</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <table cellpadding="10" cellspacing="0" width="100%" id="cartBody">
      <tr>
        <td id="layoutHeader" colspan="2" align="left" style="text-align:left !important;">
          <font face="verdana">
            <font size="2">
              Reference:&#160;<xsl:value-of select="@InvoiceRef" />
              <br/>
              Order Total:&#160;<xsl:value-of select="$currency" /><xsl:value-of select="format-number(@total, '0.00')" />
              <br/>
              Date:&#160;<xsl:value-of select="@InvoiceDate" />
              <br/>
              <xsl:if test="@AccountID">
                <strong class="payonAccount">
                  Paid on Account ID:&#160;<xsl:value-of select="@AccountID" />
                </strong>
                <br/>
              </xsl:if>
              <xsl:if test="AdditionalComments">
                <strong>Additional Comments:</strong>
                <br/>
                <xsl:value-of select="AdditionalComments" />
                <br/>
              </xsl:if>
            </font>
          </font>
        </td>
      </tr>
      <tr>
        <td align="left" style="text-align:left !important;">
          <xsl:apply-templates select="Contact[@type='Billing Address']" mode="cartEmail"/>
        </td>
        <td align="left" style="text-align:left !important;">
          <xsl:apply-templates select="Contact[@type='Delivery Address']" mode="cartEmail"/>
        </td>
      </tr>
      <tr>
        <td colspan="2" align="left" style="text-align:left !important;">
          <font face="verdana">
            <xsl:apply-templates select="." mode="orderItemsEmail"/>
          
          <br/>
          <br/>
          <h2>Thank you for your order !</h2>
          </font>
        </td>
      </tr>
      <tr>
        <td colspan="2" align="left" style="text-align:left !important;">
          <font face="verdana">
            <xsl:if test="/Page/Contents/Content[@name='cartMessage'] or Notes">
              <h3>Additional Information</h3>
              <font size="2">
                <xsl:copy-of select="/Page/Contents/Content[@name='cartMessage']/node()" />
                <br />
                <xsl:copy-of select="Notes/Notes" />
                <br />
                <xsl:value-of select="/Page/Contents/Content[@name='cartTerms']"/>
              </font>
            </xsl:if>
          </font>
        </td>
      </tr>
    </table>
  </xsl:template>
  <!-- -->
</xsl:stylesheet>
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:import href="../../../../../ewcommon_v5-1/xsl/Mailer/MailerImports.xsl"/>
  <xsl:import href="email/emailCart.xsl"/>
  <xsl:import href="../Mailer/EmailStationary.xsl"/>

  <xsl:template match="/">
    <xsl:apply-imports/>
  </xsl:template>
  <xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/>

  <xsl:template match="*">
    <html>
      <head>
        <title>
          <xsl:apply-templates select="." mode="subject"/>
        </title>
        <xsl:apply-templates select="." mode="emailStyle"/>
      </head>
      <xsl:apply-templates select="." mode="emailBody"/>
    </html>
  </xsl:template>
  
  <xsl:template match="*" mode="subject">
    New Order Ref: <xsl:value-of select="Order/@InvoiceRef" />
  </xsl:template>

  <xsl:template match="*" mode="unsubscribeMsg">

  </xsl:template>

  <xsl:template match="Order" mode="mainLayout">
    <table cellpadding="10" cellspacing="0" width="100%" id="cartBody">
      <tr>
        <td id="layoutHeader" colspan="2" align="left" style="text-align:left !important;">
          <font face="{$bodyFont}">
            <h1>Order</h1>
            <font size="2">
              Reference:&#160;<xsl:value-of select="@InvoiceRef" />
              <br/>
              Order Total:&#160;£<xsl:value-of select="format-number(@total, '0.00')" />
              <br/>
              Date:&#160;<xsl:value-of select="@InvoiceDate" />
              <br/>
              <xsl:if test="@AccountID">
                <strong class="payonAccount">
                  Paid on Account ID:&#160;<xsl:value-of select="@AccountID" />
                </strong>
                <br/>
              </xsl:if>
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
              <br/>
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
          <font face="{$bodyFont}">
            <xsl:apply-templates select="." mode="orderItemsEmail"/>
          </font>
        </td>
      </tr>
      <tr>
        <td colspan="2" align="left" style="text-align:left !important;">
          <font face="{$bodyFont}">
            <xsl:if test="/Page/Contents/Content[@name='cartMessage'] or Notes">
              <h3>Additional Information</h3>
              <font size="1">
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
    <div>
      <xsl:value-of select="/Page/Contents/Content[@name='cartTerms']"/>
    </div>
  </xsl:template>
  <!-- -->
</xsl:stylesheet>
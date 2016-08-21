<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:import href="d:\web\ewcommon_v5\xsl\tools\Functions.xsl"/>

  <xsl:output method="text" indent="no"/>

  <xsl:template match="*">
    <xsl:text>Order</xsl:text>&#13;

    <xsl:text>Reference: </xsl:text><xsl:value-of select="@InvoiceRef" />&#13;
    <xsl:text>Order Total: </xsl:text><xsl:value-of select="format-number(@total, '0.00')" />&#13;
    <xsl:text>Date: </xsl:text><xsl:value-of select="@InvoiceDate" />&#13;
    <xsl:if test="@AccountID">
      <xsl:text>Paid on Account ID: </xsl:text><xsl:value-of select="@AccountID" />&#13;
    </xsl:if>
    <xsl:if test="AdditionalComments">
      <xsl:text>Additional Comments: </xsl:text><xsl:value-of select="AdditionalComments" />&#13;
    </xsl:if>

    <xsl:text>Card Details:</xsl:text>

    Card Number: <xsl:value-of select="@CardNumber" />
    Issue Date: <xsl:value-of select="@IssueDate" />
    Expiry Date: <xsl:value-of select="@ExpiryDate" />
    Card Type: <xsl:value-of select="@CardType" />
    Issue Number: <xsl:value-of select="@IssueNumber" />
    CV2: <xsl:value-of select="@CV2" />&#13;&#10;
    
    <xsl:apply-templates select="Contact[@type='Billing Address']" mode="cartEmail"/>&#13;
    
    <xsl:apply-templates select="Contact[@type='Delivery Address']" mode="cartEmail"/>&#13;

    <xsl:apply-templates select="." mode="orderItems"/>&#13;&#10;

    <xsl:if test="/Page/Contents/Content[@name='cartMessage'] or Notes">
      <xsl:text>Additional Information</xsl:text>&#13;
      <xsl:text>Exhibition Name:    </xsl:text><xsl:value-of select="Notes/Exhibition/node()"/>&#13;
      <xsl:if test="Notes/Site/node()">
        <xsl:text>Site:               </xsl:text>
        <xsl:value-of select="Notes/Site/node()"/>&#13;
      </xsl:if>
      <xsl:text>Stand Number:       </xsl:text>
      <xsl:value-of select="Notes/StandNumber/node()"/>&#13;
      <xsl:text>Start Date:         </xsl:text>
      <xsl:call-template name="DD_Mon_YYYY">
        <xsl:with-param name="date" select="Notes/StartDate/node()"/>
      </xsl:call-template>&#13;
      <xsl:text>End Date:           </xsl:text>
      <xsl:call-template name="DD_Mon_YYYY">
        <xsl:with-param name="date" select="Notes/EndDate/node()"/>
      </xsl:call-template>&#13;
      <xsl:if test="Notes/FinishTime/node()">
        <xsl:text>Finish Time:        </xsl:text>
        <xsl:value-of select="Notes/FinishTime/node()"/>&#13;
      </xsl:if>
      <xsl:if test="Notes/otherInfo/node()">
        <xsl:text>Other information:  </xsl:text><xsl:value-of select="Notes/otherInfo/node()"/>&#13;
      </xsl:if>
    </xsl:if>
    <xsl:value-of select="/Page/Contents/Content[@name='cartTerms']"/>

  </xsl:template>
  <!-- -->
  <!-- -->
  <xsl:template match="Contact" mode="cartEmail">
    <xsl:variable name="secureURL">
      <xsl:call-template name="getSecureURL"/>
    </xsl:variable>
    <xsl:value-of select="@type"/><xsl:text> Details</xsl:text>&#13;
    <xsl:value-of select="GivenName"/>&#13;
    <xsl:if test="Company/node()!=''">
      <xsl:value-of select="Company"/>&#13;     
    </xsl:if>
    <xsl:value-of select="Street"/>&#13;
    <xsl:if test="Details/cContactAddress2/node()!=''">
      <xsl:value-of select="Details/cContactAddress2"/>&#13;
    </xsl:if>
    <xsl:value-of select="City"/>&#13;
    <xsl:if test="State/node()!=''">
      <xsl:value-of select="State"/>&#13;
    </xsl:if>
    <xsl:value-of select="PostalCode"/>&#13;
    <xsl:if test="Country/node()!=''">
      <xsl:value-of select="Country"/>&#13;
    </xsl:if>Tel: <xsl:value-of select="Telephone"/>&#13;
    <xsl:if test="Fax/node()!=''">
      <xsl:text>Fax: </xsl:text><xsl:value-of select="Fax"/>&#13;
    </xsl:if>
    <xsl:if test="Email/node()!=''">
      <xsl:text>Email: </xsl:text><xsl:value-of select="Email"/>&#13;
    </xsl:if>
  </xsl:template>
  <!-- -->

  <xsl:template match="Order" mode="orderItems">
    <xsl:text>Qty   Description                         Ref      Price      Linetotal  </xsl:text>&#13;
    <xsl:text>-----------------------------------------------------------------------</xsl:text>&#13;
    <xsl:for-each select="Item">
      <xsl:value-of select="substring(concat(string(@quantity),'         '),0,6)"/>
      <xsl:text> </xsl:text>
      <xsl:value-of select="substring(concat(node(),'  ',@option1,'  ',@option2,'                                   '),0,36)"/>
      <xsl:text> </xsl:text>
      <xsl:value-of select="substring(concat(string(@ref),'        '),0,9)"/>
      <xsl:text> </xsl:text>
      <xsl:value-of select="substring(concat(string(/Page/Cart/@currencySymbol),format-number(@price,'#.00'),'          '),0,11)"/>
      <xsl:text> </xsl:text>
      <xsl:value-of select="substring(concat(string(/Page/Cart/@currencySymbol),format-number(@price * @quantity,'#.00'),'           '),0,11)"/>
      <xsl:text>&#13;</xsl:text>
      <xsl:if test="string-length(concat(node(),'  ',@option1,'  ',@option2))>37">
        <xsl:value-of select="concat('      ',substring(concat(node(),'  ',@option1,'  ',@option2),36,string-length(concat(node(),'  ',@option1,'  ',@option2))))"/>
        <xsl:text>&#13;</xsl:text>
      </xsl:if>
      &#13;
    </xsl:for-each>
    <xsl:text>-----------------------------------------------------------------------</xsl:text>&#13;
    <!-- -->
    <xsl:text>                                          Sub Total:          </xsl:text>
    <xsl:value-of select="substring(concat(string(/Page/Cart/@currencySymbol),format-number(@totalNet,'0.00'),'                                            '),0,40)"/>&#13;
    <xsl:text>                                          </xsl:text><xsl:choose><xsl:when test="//Cart/Contact/Address/Country='United Kingdom'">VAT at </xsl:when><xsl:otherwise>Tax at </xsl:otherwise></xsl:choose><xsl:value-of select="format-number(@vatRate, '#.00')"/><xsl:text>%:      </xsl:text>
    <xsl:value-of select="substring(concat(string(/Page/Cart/@currencySymbol),format-number(@vatAmt,'0.00'),'                                            '),0,40)"/>&#13;
    <xsl:text>                                          Total Value:        </xsl:text>
    <xsl:value-of select="substring(concat(string(/Page/Cart/@currencySymbol),format-number(@total, '#.00'),'                             '),0,40)"/>&#13;
    <xsl:text>----------------------------------------------------------------------</xsl:text>
  </xsl:template>
</xsl:stylesheet>
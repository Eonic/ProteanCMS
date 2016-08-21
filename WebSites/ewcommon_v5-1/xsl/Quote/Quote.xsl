<?xml version="1.0" encoding="UTF-8"?>
<!-- -->
<!--This version created on 15 January 2005-->
<!-- -->
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
  <!-- -->
  <!--   ################################################   Quote Summary ##############################################   -->
  <!-- -->
  <xsl:template match="Quote" mode="cartBrief">
    <xsl:variable name="parentURL">
      <xsl:call-template name="getContentParURL"/>
    </xsl:variable>
    <xsl:variable name="secureURL">
      <xsl:call-template name="getSecureURL"/>
    </xsl:variable>
    <xsl:variable name="siteURL">
      <xsl:call-template name="getSiteURL"/>
    </xsl:variable>
    <table cellspacing="0" id="quoteBrief">
      <tr>
        <td>
          <h2>Your Quote Request</h2>
          <p>
            <xsl:value-of select="@itemCount"/> Items -  <xsl:value-of select="/Page/Contents/Content[@name='currency']"/>
            <xsl:value-of select="format-number(@total, '#.00')"/>
          </p>
          <xsl:if test="@cmd='' and @status!='Empty'">
            <p>
              <a href="{$siteURL}?quoteCmd=Quote" title="Click here to view full details of your Quote" class="morelink">Show Details</a>
            </p>
          </xsl:if>
        </td>
      </tr>
    </table>
  </xsl:template>
  <!-- -->

  <!-- ############### Quote Cart ################## -->
  <!-- -->
  <xsl:template match="Quote" mode="cartFull">
    <xsl:variable name="parentURL">
      <xsl:call-template name="getContentParURL"/>
    </xsl:variable>
    <xsl:variable name="secureURL">
      <xsl:call-template name="getSecureURL"/>
    </xsl:variable>
    <xsl:variable name="siteURL">
      <xsl:call-template name="getSiteURL"/>
    </xsl:variable>
    <div id="cartFull" class="cartFull">
      <xsl:apply-templates select="." mode="quoteProgressLegend"/>
      <xsl:apply-templates select="." mode="quoteProcess"/>
    </div>
  </xsl:template>
  <!-- -->
  <!-- ############### Quote Buttons ################## -->
  <!-- -->
  <xsl:template match="Quote" mode="cartButtons">
    <xsl:variable name="parentURL">
      <xsl:call-template name="getContentParURL"/>
    </xsl:variable>
    <xsl:variable name="secureURL">
      <xsl:call-template name="getSecureURL"/>
    </xsl:variable>
    <xsl:if test="@cmd='Add' or @cmd='Quote' or @cmd='Confirm' or @cmd='EnterOptions' or @cmd='ShowInvoice'  or @cmd='ShowCallBackInvoice'  or @cmd='EnterPaymentDetails'">
      <div class="cartButtons">
        <xsl:choose>
          <xsl:when test="@cmd='Add' or @cmd='Quote'">
            <input type="submit" name="quoteSend" value="Send Quote Request" class="button checkout principle"/>
            <input type="submit" name="quoteBrief" value="Add More Equipment" class="button continue"/>
            <input type="submit" name="quoteUpdate" value="Update Quote" class="button update"/>
            <input type="submit" name="quoteQuit" value="Empty Quote" class="button empty"/>
          </xsl:when>
          <xsl:when test="@cmd='Confirm'">
            <input type="submit" name="quoteSend" value="Proceed" class="button confirm principle" onclick=""/>
            <xsl:if test="not(@readonly)">
              <input type="submit" name="quoteBrief" value="Continue Shopping" class="button continue"/>
              <input type="submit" name="quoteUpdate" value="Update Quote" class="button update"/>
              <input type="submit" name="quoteQuit" value="Empty Quote" class="button empty"/>
            </xsl:if>
          </xsl:when>
          <xsl:when test="@cmd='EnterOptions' or @cmd='EnterPaymentDetails'">
            <xsl:if test="not(@readonly)">
              <input type="submit" name="quoteBrief" value="Continue Shopping" class="button continue"/>
              <input type="submit" name="quoteUpdate" value="Update Quote" class="button update"/>
              <input type="submit" name="quoteQuit" value="Empty Quote" class="button empty"/>
            </xsl:if>
          </xsl:when>
        </xsl:choose>&#160;
      </div>
    </xsl:if>
  </xsl:template>
  <xsl:template match="Quote" mode="quoteProgressLegend">
    <!--xsl:text>cartCmd(*debug): </xsl:text><xsl:value-of select="@cmd"/>
		<div class="progressLegend">
			<span>
				<xsl:if test="@cmd='Notes'">
					<xsl:attribute name="class">active</xsl:attribute>
				</xsl:if>
				<a href="?quoteCmd=Notes">Calculate VA Rating</a>
			</span>
			<span>
				<xsl:if test="@cmd='Search'">
					<xsl:attribute name="class">active</xsl:attribute>
				</xsl:if>
				<br/>Search for UPS</span>
			<span>
				<br/>Logon / Save</span>
			<span>
				<xsl:if test="@cmd='Billing' or @cmd='Delivery'">
					<xsl:attribute name="class">active</xsl:attribute>
				</xsl:if>Contact / Shipping <br/>Details</span>
			<span>
				<xsl:if test="@cmd='ShowInvoice'">
					<xsl:attribute name="class">active</xsl:attribute>
				</xsl:if>Request Contact</span>
			<span>Convert to Order</span>
		</div>
		<div class="terminus">&#160;</div-->
  </xsl:template>
  <!--#-->
  <!--############################## Quote Procees - Default ################################-->
  <!--#-->
  <xsl:template match="Quote" mode="quoteProcess">
    <h2>Your Quote Request - Generic</h2>

    <xsl:apply-templates select="." mode="quoteErrorReports"/>
    <xsl:apply-templates select="." mode="quoteAddresses"/>
    <xsl:apply-templates select="." mode="quoteItems"/>
  </xsl:template>
  <!--#-->
  <!--############################## Quote Procees - Add / Quote ################################-->
  <xsl:template match="Quote[@cmd='Add' or @cmd='Quote']" mode="orderProcessTitle">
    <h2>Your quote request</h2>
  </xsl:template>
  <!--#-->
  <xsl:template match="Quote[@cmd='Add' or @cmd='Quote']" mode="quoteProcess">
    <xsl:apply-templates select="." mode="orderProcessTitle" />
    <xsl:apply-templates select="." mode="quoteErrorReports"/>
    <xsl:apply-templates select="." mode="quoteAddresses"/>
    <form method="post" id="cart">
      <input type="submit" name="quoteNotes" value="Proceed" class="button principle"/>
      <input type="submit" name="quoteBrief" value="Continue Shopping" class="button continue"/>
      <input type="submit" name="quoteUpdate" value="Update Quote" class="button update"/>
      <input type="submit" name="quoteQuit" value="Empty Quote" class="button empty"/>
      <xsl:apply-templates select="." mode="quoteItems">
        <xsl:with-param name="editQty">false</xsl:with-param>
      </xsl:apply-templates>
      <input type="submit" name="quoteNotes" value="Proceed" class="button principle"/>
      <input type="submit" name="quoteBrief" value="Continue Shopping" class="button continue"/>
      <input type="submit" name="quoteUpdate" value="Update Quote" class="button update"/>
      <input type="submit" name="quoteQuit" value="Empty Quote" class="button empty"/>
    </form>
  </xsl:template>
  <!--#-->
  <!--############################## Quote Procees - Notes ################################-->
  <xsl:template match="Quote[@cmd='Notes']" mode="orderProcessTitle">
    <h2>Your quote request - Please tell us any special requirements</h2>
  </xsl:template>
  <!--#-->
  <xsl:template match="Quote[@cmd='Notes']" mode="quoteProcess">
    <xsl:apply-templates select="." mode="orderProcessTitle" />
    <xsl:apply-templates select="." mode="quoteErrorReports"/>
    <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='notesForm']" mode="xform"/>
    <form method="post" id="cart">
      <div class="cartButtons">
        <!--input type="submit" name="quoteLogon" value="Proceed" class="button principle"/-->
        <input type="submit" name="quoteBrief" value="Continue Shopping" class="button continue"/>
        <input type="submit" name="quoteUpdate" value="Update Quote" class="button update"/>
        <input type="submit" name="quoteQuit" value="Empty Quote" class="button empty"/>
      </div>
      <xsl:apply-templates select="." mode="quoteItems">
        <xsl:with-param name="editQty">true</xsl:with-param>
      </xsl:apply-templates>
      <div class="cartButtons">
        <!--input type="submit" name="quoteLogon" value="Proceed" class="button principle"/-->
        <input type="submit" name="quoteBrief" value="Continue Shopping" class="button continue"/>
        <input type="submit" name="quoteUpdate" value="Update Quote" class="button update"/>
        <input type="submit" name="quoteQuit" value="Empty Quote" class="button empty"/>
      </div>
    </form>
  </xsl:template>
  <!--#-->
  <!--############################## Quote Procees - Search ################################-->
  <!--#-->
  <xsl:template match="Quote[@cmd='Search']" mode="quoteProcess">
    <!--h2>Your Quote- Please enter any notes with your order</h2-->
    <xsl:apply-templates select="." mode="quoteErrorReports"/>
    <div id="notesForm">
      <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='notesForm']" mode="xform"/>
    </div>
    <div id="searchResults">
      List Products
      <xsl:apply-templates select="/" mode="List_Products"/>
    </div>
    <form method="post" id="cart">
      <div class="cartButtons">
        <input type="submit" name="quoteBrief" value="Continue Shopping" class="button continue"/>
        <input type="submit" name="quoteUpdate" value="Update Quote" class="button update"/>
        <input type="submit" name="quoteQuit" value="Empty Quote" class="button empty"/>
      </div>
      <xsl:apply-templates select="." mode="quoteItems">
        <xsl:with-param name="editQty">true</xsl:with-param>
      </xsl:apply-templates>
      <div class="cartButtons">
        <input type="submit" name="quoteBrief" value="Continue Shopping" class="button continue"/>
        <input type="submit" name="quoteUpdate" value="Update Quote" class="button update"/>
        <input type="submit" name="quoteQuit" value="Empty Quote" class="button empty"/>
      </div>
    </form>
  </xsl:template>
  <!--#-->
  <!--############################## Quote Procees - Logon ################################-->
  <!--#-->
  <xsl:template match="Quote[@cmd='Logon']" mode="quoteProcess">
    <h2>Registration</h2>
    <xsl:apply-templates select="." mode="quoteErrorReports"/>
    <div id="cartInvoice">
      <a href="pgid={/Page/@id}&amp;quoteCmd=Notes" class="textButton">Proceed Without Registering / Logging In</a>
    </div>
    <div id="cartRegisterBox">
      <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='CartRegistration']" mode="xform"/>
    </div>
    <div id="cartLogonBox">
      <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='UserLogon']" mode="xform"/>
    </div>
  </xsl:template>
  <!--#-->
  <!--############################## Quote Procees - Billing ################################-->
  <!--#-->
  <xsl:template match="Quote[@cmd='Add' or @cmd='Quote']" mode="orderProcessTitle">
    <h2>Your Quote Request - Enter your contact details</h2>
  </xsl:template>
  
  
  <xsl:template match="Quote[@cmd='Billing']" mode="quoteProcess">
    <xsl:apply-templates select="." mode="orderProcessTitle" />
    <h2>Your Quote Request - Enter your contact details</h2>
    <xsl:apply-templates select="." mode="quoteErrorReports"/>
    <xsl:apply-templates select="." mode="quoteEditAddresses"/>
    <xsl:apply-templates select="." mode="displayNotes"/>
    <form method="post" id="cart">
      <input type="submit" name="quoteBrief" value="Continue Shopping" class="button continue"/>
      <input type="submit" name="quoteUpdate" value="Update Quote" class="button update"/>
      <input type="submit" name="quoteQuit" value="Empty Quote" class="button empty"/>
      <xsl:apply-templates select="." mode="quoteItems">
        <xsl:with-param name="editQty">true</xsl:with-param>
      </xsl:apply-templates>
      <input type="submit" name="quoteBrief" value="Continue Shopping" class="button continue"/>
      <input type="submit" name="quoteUpdate" value="Update Quote" class="button update"/>
      <input type="submit" name="quoteQuit" value="Empty Quote" class="button empty"/>
    </form>
  </xsl:template>
  <!--#-->
  <!--############################## Quote Procees - Delivery ################################-->
  <!--#-->
  <xsl:template match="Quote[@cmd='Delivery']" mode="quoteProcess">
    <h2>Your Quote Request - Enter the proposed delivery address</h2>
    <xsl:apply-templates select="." mode="quoteErrorReports"/>
    <xsl:apply-templates select="." mode="quoteEditAddresses"/>
    <xsl:apply-templates select="." mode="displayNotes"/>
    <form method="post" id="cart">
      <input type="submit" name="quoteBrief" value="Continue Shopping" class="button continue"/>
      <input type="submit" name="quoteUpdate" value="Update Quote" class="button update"/>
      <input type="submit" name="quoteQuit" value="Empty Quote" class="button empty"/>
      <xsl:apply-templates select="." mode="quoteItems">
        <xsl:with-param name="editQty">true</xsl:with-param>
      </xsl:apply-templates>
      <input type="submit" name="quoteBrief" value="Continue Shopping" class="button continue"/>
      <input type="submit" name="quoteUpdate" value="Update Quote" class="button update"/>
      <input type="submit" name="quoteQuit" value="Empty Quote" class="button empty"/>
    </form>
  </xsl:template>

  <!--#-->
  <!--############################## Quote Procees - Send Quote Request ################################-->
  <!--#-->
  <xsl:template match="Quote[@cmd='ShowInvoice']" mode="quoteProcess">
    <h2>Your Quote Request - Check the details before forwarding to our sales team.</h2>
    <xsl:apply-templates select="." mode="quoteErrorReports"/>
    <xsl:apply-templates select="." mode="quoteAddresses"/>
    <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and (@name='PayForm' or @name='Secure3D')]" mode="xform"/>
    <form method="post" id="cart">
      <input type="submit" name="quoteSend" value="Send Quote Request" class="button principle"/>
      <input type="submit" name="quoteBrief" value="Continue Shopping" class="button continue"/>
      <input type="submit" name="quoteUpdate" value="Update Quote" class="button update"/>
      <input type="submit" name="quoteQuit" value="Empty Quote" class="button empty"/>
      <xsl:apply-templates select="." mode="quoteItems">
        <xsl:with-param name="editQty">true</xsl:with-param>
      </xsl:apply-templates>
      <input type="submit" name="quoteSend" value="Send Quote Request" class="button principle"/>
      <input type="submit" name="quoteBrief" value="Continue Shopping" class="button continue"/>
      <input type="submit" name="quoteUpdate" value="Update Quote" class="button update"/>
      <input type="submit" name="quoteQuit" value="Empty Quote" class="button empty"/>
    </form>
  </xsl:template>
  <!--#-->
  <!--#-->
  <!--############################## Error Reports ################################-->
  <!--#-->
  <xsl:template match="Quote" mode="quoteErrorReports">
    <xsl:if test="@errorMsg>0">
      <p class="errorMessage">
        <xsl:choose>
          <!-- Error No. 1 - 99 : Standard errors from the Cart -->
          <xsl:when test="@errorMsg='1'">
            <strong>The order has timed out and cannot continue</strong>, due to one of the following two reasons:<br/>
            <br/>
            1. The order had been left for over ten minutes without any updates.  The details are automatically removed for security purposes.<br/>
            <br/>
            2. You may have disabled cookies or they are undetectable.  The shopping cart requires cookies to be enabled in order to proceed.<br/>
            <br/>
            Please ensure cookies are enabled in your browser to continue shopping, or call for assistance.  No transaction has been made.
          </xsl:when>
          <xsl:when test="@errorMsg='2'">
            The item(s) you are trying to add cannot be added to this shopping basket. <br/>
            <br/> Please proceed to the checkout and pay for the items in the basket, and then continue with your shopping.
          </xsl:when>
          <xsl:when test="@errorMsg='3'">
            There is no valid delivery option for this order.  This may be due to a combination of location, price, weight or quantity.<br/>
            <br/> Please call for assistance.
          </xsl:when>
          <!-- Error No. 100+: Payment gateway errors-->
          <!-- Worldpay errors-->
          <xsl:when test="@errorMsg='1000'">
            The transaction was cancelled during the payment processing - this was either at your request or the request of our payment provider, Worldpay.<br/>
            <br/> Please call for more information.
          </xsl:when>
          <xsl:when test="@errorMsg='1001'">
            The order reference could not be found, or the order did not have the correct status.  This may occur if you have tried to pay for the same order twice, or if there has been a long duration between visiting our payment provider, Worldpay's site and entering payment details.<br/>
            <br/> Please call for assistance.
          </xsl:when>
          <xsl:when test="@errorMsg='1002'">
            The payment provider, Worldpay, did not provide a valid response.<br/>
            <br/> Please call for assistance.
          </xsl:when>
          <!-- Error No. 1000+ : Bespoke errors can be put here-->
        </xsl:choose>
      </p>
    </xsl:if>
    <xsl:if test="error/msg">
      <p class="errorMessage">
        <xsl:for-each select="error/msg">
          <xsl:copy-of select="node()"/>
          <br/>
        </xsl:for-each>
      </p>
    </xsl:if>
  </xsl:template>
  <!--#-->

  <!--##############################Quote Addresses ################################-->
  
  <xsl:template match="Quote" mode="quoteEditAddresses">
    <xsl:variable name="parentURL">
      <xsl:call-template name="getContentParURL"/>
    </xsl:variable>
    <xsl:if test="/Page/Contents/Content[@type='xform' and (@name='Delivery Address' or @name='Billing Address') ]">
      <!-- Don't display delivery address if hideDeliveryADress attribute is present -->
      <xsl:if test="not(@hideDeliveryAddress)">
        <xsl:choose>
          <xsl:when test="/Page/Contents/Content[@type='xform' and @name='Delivery Address']">
            <div id="deliveryAddress" class="cartAddress">
              <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='Delivery Address']" mode="xform"/>
            </div>
          </xsl:when>
          <xsl:when test="Contact[@type='Delivery Address']">
            <div id="deliveryAddress" class="cartAddress">
              <xsl:choose>
                <xsl:when test="@giftListId and false()">
                  <p class="addressTitle">Delivery Address Details:</p>
                  <p>
                    Your order will be delivered to : <strong>
                      <xsl:value-of select="Contact[@type='Delivery Address']/GivenName"/>
                    </strong>
                  </p>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:apply-templates select="Contact[@type='Delivery Address']" mode="cart">
                    <xsl:with-param name="parentURL" select="$parentURL"/>
                    <xsl:with-param name="cartType" select="'quote'"/>
                  </xsl:apply-templates>
                </xsl:otherwise>
              </xsl:choose>
            </div>
          </xsl:when>
        </xsl:choose>
      </xsl:if>
      <xsl:choose>
        <xsl:when test="/Page/Contents/Content[@type='xform' and @name='Billing Address']">
          <div id="billingAddress" class="cartAddress">
            <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='Billing Address']" mode="xform"/>
          </div>
        </xsl:when>
        <xsl:when test="Contact[@type='Billing Address']">
          <div id="billingAddress" class="cartAddress">
            <xsl:apply-templates select="Contact[@type='Billing Address']" mode="cart">
              <xsl:with-param name="parentURL" select="$parentURL"/>
              <xsl:with-param name="cartType" select="'quote'"/>
            </xsl:apply-templates>
          </div>
        </xsl:when>
      </xsl:choose>
    </xsl:if>
    <!-- Terminus class fix to floating columns -->
    <div class="terminus">&#160;</div>
  </xsl:template>
  <!-- -->
  <xsl:template match="Quote" mode="quoteItems">
    <xsl:param name="editQty"/>
    <xsl:variable name="parentURL">
      <xsl:call-template name="getContentParURL"/>
    </xsl:variable>
    <xsl:variable name="secureURL">
      <xsl:call-template name="getSecureURL"/>
    </xsl:variable>
    <xsl:variable name="siteURL">
      <xsl:call-template name="getSiteURL"/>
    </xsl:variable>
    <xsl:variable name="currency" select="@currencySymbol"/>
    
    <xsl:apply-templates select="." mode="quoteListing">
      <xsl:with-param name="editQty" select="$editQty"/>
    </xsl:apply-templates>
    <xsl:if test="$editQty='true' and Item">
      <div id="cartLegend">
        <p class="delete">
          <img src="/ewCommon/images/icons/trash.gif" alt="delete icon" class="delete icon"/>
          <span class="hidden">&#160;&#160;</span>Click on this icon to remove an item from the quote.<br/>If you amend the quantity of items please click 'Update Quote' before continuing to browse.
        </p>
      </div>
    </xsl:if>
    <xsl:if test="/Page/Contents/Content[@name='cartMessage']">
      <div class="cartMessage">
        <xsl:copy-of select="/Page/Contents/Content[@name='cartMessage']/node()"/>
      </div>
    </xsl:if>
  </xsl:template>
  <!-- -->
  <xsl:template match="Quote" mode="quoteListing">
    <xsl:param name="editQty"/>
    <table cellspacing="0" id="cartListing" summary="This table contains a list of the items which you have added to the shopping quote. To change the quantity of an item, replace the number under the Qty column and click on 'Update Quote'.">
      <tr>
        <th class="heading delete">&#160;</th>
        <th class="heading quantity">Qty</th>
        <th class="heading description">Description</th>
        <th class="heading ref">Ref</th>
        <th class="heading price">Price</th>
        <th class="heading lineTotal">Line Total</th>
      </tr>
      <xsl:for-each select="Item">
        <xsl:apply-templates select="." mode="quoteItem">
          <xsl:with-param name="editQty" select="$editQty"/>
        </xsl:apply-templates>
      </xsl:for-each>
      <xsl:if test="@shippingCost &gt; 0">
        <tr>
          <td colspan="4">&#160;</td>
          <td class="shipping heading">
            <xsl:choose>
              <xsl:when test="/Page/Contents/Content[@name='shippingCostLabel']!=''">
                <xsl:value-of select="/Page/Contents/Content[@name='shippingCostLabel']"/>
              </xsl:when>
              <xsl:otherwise>Shipping Cost:</xsl:otherwise>
            </xsl:choose>
          </td>
          <td class="shipping amount">
            <xsl:value-of select="$currency"/>
            <xsl:value-of select="format-number(@shippingCost,'0.00')"/>
          </td>
        </tr>
      </xsl:if>
      <xsl:if test="@vatRate &gt; 0">
        <tr>
          <td colspan="4">
            <!--xsl:attribute name="rowspan">
									<xsl:call-template name="calcRows">
										<xsl:with-param name="r1"><xsl:choose><xsl:when test="@vatRate &gt; 0">1</xsl:when><xsl:otherwise>0</xsl:otherwise></xsl:choose>	</xsl:with-param>
										<xsl:with-param name="r2"><xsl:choose><xsl:when test="@payableAmount &gt; 0">1</xsl:when><xsl:otherwise>0</xsl:otherwise></xsl:choose>	</xsl:with-param>
										<xsl:with-param name="r3"><xsl:choose><xsl:when test="@paymentMade &gt; 0">1</xsl:when><xsl:otherwise>0</xsl:otherwise></xsl:choose>	</xsl:with-param>
										<xsl:with-param name="r0">2</xsl:with-param>
									</xsl:call-template>
								</xsl:attribute-->
            &#160;
          </td>
          <td class="subTotal heading">
            Sub Total:
          </td>
          <td class="subTotal amount">
            <xsl:value-of select="$currency"/>
            <xsl:value-of select="format-number(@totalNet, '0.00')"/>
          </td>
        </tr>

        <tr>
          <td colspan="4">&#160;</td>
          <td class="vat heading">
            <xsl:choose>
              <xsl:when test="//Cart/Contact/Address/Country='United Kingdom'">VAT at </xsl:when>
              <xsl:otherwise>Tax at </xsl:otherwise>
            </xsl:choose>
            <xsl:value-of select="format-number(@vatRate, '#.00')"/>%:
          </td>
          <td class="vat amount">
            <span class="currency">
              <xsl:value-of select="/Page/Cart/@currencySymbol"/>
            </span>
            <xsl:value-of select="format-number(@vatAmt, '0.00')"/>
          </td>
        </tr>
      </xsl:if>
      <tr>
        <td colspan="4">&#160;</td>
        <td class="total heading">Total Value:</td>
        <td class="total amount">
          <xsl:value-of select="/Page/Cart/@currencySymbol"/>
          <xsl:value-of select="format-number(@total, '0.00')"/>
        </td>
      </tr>
      <xsl:if test="@paymentMade &gt; 0">
        <tr>
          <td colspan="4">&#160;</td>
          <td class="total heading">
            <xsl:choose>
              <xsl:when test="@transStatus">Transaction Made</xsl:when>
              <xsl:when test="@payableType='settlement' and not(@transStatus)">Payment Received</xsl:when>
            </xsl:choose>
          </td>
          <td class="total amount">
            <xsl:value-of select="$currency"/>
            <xsl:value-of select="format-number(@paymentMade, '0.00')"/>
          </td>
        </tr>
      </xsl:if>
      <xsl:if test="@payableAmount &gt; 0">
        <tr>
          <td colspan="4">&#160;</td>
          <td class="total heading">
            <xsl:choose>
              <xsl:when test="@payableType='deposit' and not(@transStatus)">Deposit Payable</xsl:when>
              <xsl:when test="@payableType='settlement' or (@payableType='deposit' and @transStatus)">Amount Outstanding</xsl:when>
            </xsl:choose>
          </td>
          <td class="total amount">
            <xsl:value-of select="$currency"/>
            <xsl:value-of select="format-number(@payableAmount, '0.00')"/>
          </td>
        </tr>
      </xsl:if>
    </table>
  </xsl:template>
  <!--#-->
  <xsl:template match="Item" mode="quoteItem">
    <xsl:param name="editQty"/>
    <xsl:variable name="parentURL">
      <xsl:call-template name="getContentParURL"/>
    </xsl:variable>
    <xsl:variable name="secureURL">
      <xsl:call-template name="getSecureURL"/>
    </xsl:variable>
    <xsl:variable name="siteURL">
      <xsl:call-template name="getSiteURL"/>
    </xsl:variable>
    <xsl:variable name="currency" select="@currencySymbol"/>
    <tr class="orderItem">
      <td class="cell delete">
        <xsl:choose>
          <xsl:when test="$editQty!='true'">&#160;</xsl:when>
          <xsl:otherwise>
            <a href="{$parentURL}?quoteCmd=Remove&amp;id={@id}" title="click here to remove this item from the list">
              <!--BJR - This either doesnt work or is wrong so i have changed it for the moment to work-->
              <!--<img src="{$secureURL}/ewCommon/images/icons/trash.gif" alt="delete icon - click here to remove this item from the list"/>-->
              <img src="/ewCommon/images/icons/trash.gif" alt="delete icon - click here to remove this item from the list"/>
            </a>
          </xsl:otherwise>
        </xsl:choose>
      </td>
      <td class="cell quantity">
        <xsl:choose>
          <xsl:when test="$editQty!='true'">
            <xsl:value-of select="@quantity"/>
          </xsl:when>
          <xsl:otherwise>
            <input type="text" size="2" name="itemId-{@id}" value="{@quantity}" class="">
              <xsl:if test="../@readonly">
                <xsl:attribute name="readonly">readonly</xsl:attribute>
              </xsl:if>
            </input>
          </xsl:otherwise>
        </xsl:choose>
      </td>
      <td class="cell description">
        <a href="{$siteURL}/item{@id}" title="Clike Here to view this Product">
          <xsl:value-of select="node()"/>
        </a>
        <!-- ################################# Line Options Info ################################# -->
        <xsl:for-each select="Item">
          <span class="optionList">
            <xsl:apply-templates select="option" mode="optionDetail"/>
            <xsl:if test="@price!=0">
              <xsl:value-of select="$currency"/>
              <xsl:value-of select="format-number(@price,'#.00')"/>
            </xsl:if>
          </span>
        </xsl:for-each>
        <!-- ################################# Line Discount Info ################################# -->
        <xsl:if test="Discount">
          <xsl:for-each select="DiscountPrice/DiscountPriceLine">
            <xsl:sort select="@PriceOrder"/>
            <xsl:variable name="DiscID">
              <xsl:value-of select="@nDiscountKey"/>
            </xsl:variable>
            <div class="discount">
              <xsl:if test="ancestor::Item/Discount[@nDiscountKey=$DiscID]/Images[@class='thumbnail']/@src!=''">
                <xsl:copy-of select="ancestor::Item/Discount[@nDiscountKey=$DiscID]/Images[@class='thumbnail']"/>
              </xsl:if>
              <xsl:value-of select="ancestor::Item/Discount[@nDiscountKey=$DiscID]/@cDiscountName"/>
              RRP:&#160;<strike>
                <xsl:value-of select="$currency"/>
                <xsl:choose>
                  <xsl:when test="position()=1">
                    <xsl:value-of select="format-number(ancestor::Item/DiscountPrice/@OriginalUnitPrice,'#.00')"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="format-number(preceding-sibling::DiscountPriceLine/@UnitPrice,'#.00')"/>
                  </xsl:otherwise>
                </xsl:choose>
              </strike>
              less:&#160;
              <xsl:value-of select="$currency"/>
              <xsl:value-of select="format-number(@UnitSaving,'#.00')"/>
            </div>
          </xsl:for-each>
          <!--More will go here later-->
          <xsl:for-each select="DiscountItem">
            <xsl:variable name="DiscID">
              <xsl:value-of select="@nDiscountKey"/>
            </xsl:variable>
            <div class="discount">
              <xsl:value-of select="ancestor::Item/Discount[@nDiscountKey=$DiscID]/@cDiscountName"/>
              <xsl:value-of select="@oldUnits - @Units"/>&#160;Unit<xsl:if test="(@oldUnits - @Units) > 1">s</xsl:if>
              <xsl:value-of select="$currency"/>
              <xsl:value-of select="format-number(@TotalSaving,'#.00')"/>
            </div>
          </xsl:for-each>
        </xsl:if>
      </td>
      <td class="cell ref">
        <xsl:value-of select="@ref"/>&#160;
        <xsl:for-each select="Item">
          <xsl:apply-templates select="option" mode="optionCodeConcat"/>
        </xsl:for-each>
      </td>
      <xsl:if test="not(/Page/Cart/@displayPrice='false')">
        <td class="cell price">
          <xsl:if test="DiscountPrice/@OriginalUnitPrice > @price">
            <strike>
              <xsl:value-of select="$currency"/>
              <xsl:value-of select="format-number(DiscountPrice/@OriginalUnitPrice,'#.00')"/>
            </strike>
            <br/>
          </xsl:if>
          <xsl:value-of select="$currency"/>
          <xsl:value-of select="format-number(@price,'#.00')"/>
          <xsl:for-each select="Item[@price &gt; 0]">
            <br/>
            <span class="optionList">
              <xsl:value-of select="$currency"/>
              <xsl:value-of select="format-number(@price,'#.00')"/>
            </span>
          </xsl:for-each>
        </td>
        <td class="cell lineTotal">
          <xsl:if test="DiscountPrice/@OriginalUnitPrice * DiscountPrice/@Units > @itemTotal">
            <strike>
              <xsl:value-of select="$currency"/>
              <xsl:value-of select="format-number(DiscountPrice/@OriginalUnitPrice * DiscountPrice/@Units,'#.00')"/>
            </strike>
            <br/>
          </xsl:if>
          <xsl:value-of select="$currency"/>
          <xsl:choose>
            <xsl:when test="@itemTotal">
              <xsl:value-of select="format-number(@itemTotal,'#.00')"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="format-number((@price +(sum(*/@price)))* @quantity,'#.00')"/>
            </xsl:otherwise>
          </xsl:choose>
        </td>
      </xsl:if>
    </tr>
  </xsl:template>
  <!-- -->
  <xsl:template match="Quote" mode="displayNotes">
    <xsl:variable name="parentURL">
      <xsl:call-template name="getContentParURL"/>
    </xsl:variable>
    <h3>Additional information for Quote</h3>
    <p>
      <xsl:copy-of select="Notes/node()"/>
    </p>
    <xsl:if test="not(@readonly)">
      <p class="optionButtons">
        <a href="{$parentURL}?quoteCmd=Notes" class="button principle" title="Click here to edit the notes on this order.">Edit Notes</a>
      </p>
    </xsl:if>
  </xsl:template>
  <!-- Displays a list of the option details as selected-->
  <xsl:template match="option" mode="optionDetail">
    <xsl:if test="@groupName!=''">
      <xsl:value-of select="@groupName"/>
      <xsl:text>: </xsl:text>
    </xsl:if>
    <xsl:value-of select="@name"/>
    <xsl:text> </xsl:text>
  </xsl:template>
</xsl:stylesheet>

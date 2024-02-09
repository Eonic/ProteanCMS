<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">

	<xsl:template match="Content" mode="addToCartButton">

		<xsl:param name="actionURL"/>

		<xsl:variable name="parentURL">
			<xsl:call-template name="getContentParURL"/>
		</xsl:variable>
		<xsl:variable name="siteURL">
			<xsl:call-template name="getSiteURL"/>
		</xsl:variable>
		<xsl:variable name="secureURL">
			<xsl:call-template name="getSecureURL"/>
		</xsl:variable>

		<xsl:variable name="price">
			<xsl:choose>
				<xsl:when test="Content[@type='SKU']">
					<xsl:value-of select="Content[@type='SKU'][1]/Prices/Price[@currency=$page/Cart/@currency]/node()"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="Prices/Price[@currency=$page/Cart/@currency]/node()"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<div id="cartButtons{@id}" class="cartButtons">
			<form action="{$actionURL}" method="post" class="ewXform">
				<xsl:apply-templates select="." mode="Options_List"/>
				<xsl:if test="$price&gt;0 and not(format-number($price, '#.00')='NaN')">
					<xsl:choose>
						<xsl:when test="Content[@type='SKU']">
							<xsl:apply-templates select="Content[@type='SKU'][1]" mode="showQuantity"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:apply-templates select="." mode="showQuantity"/>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:apply-templates select="." mode="addtoCartButtons"/>
				</xsl:if>
			</form>
		</div>
	</xsl:template>
	<!-- -->

	<!-- -->
	<!--   ################################################   Cart Full  ##############################################   -->
	<!-- -->
	<xsl:template match="Order" mode="cartFull">
		<xsl:variable name="parentURL">
			<xsl:call-template name="getContentParURL"/>
		</xsl:variable>
		<xsl:variable name="secureURL">
			<xsl:call-template name="getSecureURL"/>
		</xsl:variable>
		<xsl:variable name="siteURL">
			<xsl:call-template name="getSiteURL"/>
		</xsl:variable>
		<xsl:variable name="currentPage" select="/Page/Menu/descendant-or-self::MenuItem[@nId=/Page/@id]"/>
		<div id="cartFull" class="cartFull">
			<!--<xsl:apply-templates select="." mode="orderProgressLegend"/>-->
			<xsl:apply-templates select="." mode="orderAlert"/>
			<xsl:apply-templates select="." mode="orderProcess"/>
		</div>
	</xsl:template>

	<xsl:template match="Order" mode="orderProgressLegend">

	</xsl:template>

	<xsl:template match="Order" mode="orderAlert">

	</xsl:template>

	<!--#-->
	<!--############################## Order Procees - Default ################################-->
	<!--#-->
	<xsl:template match="Order" mode="orderProcessTitle">
		<h2>
			<!--Your Order - Generic - Cmd:-->
			<xsl:call-template name="term3005" />
			<xsl:text> (</xsl:text>
			<xsl:value-of select="@cmd"/>
			<xsl:text>)</xsl:text>
		</h2>
	</xsl:template>

	<xsl:template match="Order[@errorMsg='-1']" mode="orderProcessTitle">
		<h1>
			<!--Your Order - Generic - Cmd:-->
			<xsl:call-template name="term3005a" />
		</h1>
		<form method="post" id="cart" class="ewXform">
			<button type="submit" name="cartBrief" value="Continue Shopping" class="btn btn-custom continue">
				<xsl:call-template name="term3060" />
				<xsl:text> </xsl:text>
			</button>
		</form>
	</xsl:template>

	<xsl:template match="Order[DiscountMessage]" mode="orderAlert">
		<span class="alert">
			<xsl:apply-templates select="DiscountMessage" mode="cleanXhtml"/>
		</span>
	</xsl:template>

	<!--   ################   View Cart ###############   -->

	<xsl:template match="Order" mode="orderProcess">
		<xsl:apply-templates select="." mode="orderProcessTitle"/>
		<xsl:apply-templates select="." mode="orderErrorReports"/>
		<xsl:apply-templates select="." mode="orderAddresses"/>
		<xsl:apply-templates select="." mode="orderItems"/>
	</xsl:template>

	<!--#-->
	<!--############################## Quote Procees - Currency ################################-->
	<!--#-->
	<xsl:template match="Order[@cmd='Currency']" mode="orderProcessTitle">
		<h2>
			<!--Currency Selection-->
			<xsl:call-template name="term3016" />
		</h2>
	</xsl:template>

	<xsl:template match="Order[@cmd='Currency']" mode="orderProcess">
		<xsl:apply-templates select="." mode="orderProcessTitle"/>
		<div id="cartCurrencyBox">
			<xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='Currency']" mode="xform"/>
		</div>
	</xsl:template>

	<!--#-->
	<!--##############################Order Addresses ################################-->
	<!--#-->
	<xsl:template match="Order" mode="orderAddresses">
		<xsl:variable name="parentURL">
			<xsl:call-template name="getContentParURL"/>
		</xsl:variable>
		<div id="order-addresses">
			<xsl:if test="Contact[@type='Billing Address']">
				<div id="billingAddress" class="cartAddress">
					<xsl:apply-templates select="Contact[@type='Billing Address']" mode="cart">
						<xsl:with-param name="parentURL" select="$parentURL"/>
						<xsl:with-param name="cartType" select="'cart'"/>
					</xsl:apply-templates>
				</div>
			</xsl:if>
			<xsl:if test="Contact[@type='Delivery Address'] and not(@hideDeliveryAddress)">
				<div id="deliveryAddress" class="cartAddress">
					<xsl:choose>
						<xsl:when test="@giftListId and false()">
							<p class="addressTitle">
								<!--Delivery Address Details-->
								<xsl:call-template name="term3036" />
								<xsl:text>:</xsl:text>
							</p>
							<p>
								<!--Your order will be delivered to-->
								<xsl:call-template name="term3037" />
								<xsl:text>: </xsl:text>
								<strong>
									<xsl:value-of select="Contact[@type='Delivery Address']/GivenName"/>
								</strong>
							</p>
						</xsl:when>
						<xsl:otherwise>

						</xsl:otherwise>
					</xsl:choose>
					&#160;
				</div>
			</xsl:if>
		</div>
	</xsl:template>

	<xsl:template match="Contact" mode="cart">
		<xsl:param name="parentURL"/>
		<xsl:param name="cartType"/>
		<xsl:variable name="secureURL">
			<xsl:call-template name="getSecureURL"/>
		</xsl:variable>
		<xsl:variable name="type">
			<xsl:value-of select="substring-before(@type,' ')"/>
		</xsl:variable>
		<button class="btn btn-outline-secondary hidden-lg hidden-xl hidden-xxl order-address-btn" type="button" data-bs-toggle="collapse" data-bs-target="#cart-address-collapse" aria-expanded="false" aria-controls="cart-address-collapse">
			<xsl:text>Address Details </xsl:text><i class="fas fa-caret-down">&#160;</i>
		</button>
		<div class="collapse dont-collapse-md" id="cart-address-collapse">
			<div class="row">
				<div class="col-lg-4">
					<div class="card cart-address-card mb-0">
						<div class="card-body">

							<h2>Contact Details</h2>
							<p>
								<xsl:value-of select="GivenName"/>
								<br/>
								<xsl:if test="Company/node()!=''">
									<xsl:value-of select="Company"/>
									<br/>
								</xsl:if>
								<!--Tel-->
								<xsl:call-template name="term3071" />
								<xsl:text>:&#160;</xsl:text>
								<xsl:value-of select="Telephone"/>
								<br/>
								<xsl:if test="Fax/node()!=''">
									<!--Fax-->
									<xsl:call-template name="term3072" />
									<xsl:text>:&#160;</xsl:text>
									<xsl:value-of select="Fax"/>
									<br/>
								</xsl:if>
								<xsl:if test="Email/node()!=''">
									<!--Email-->
									<xsl:call-template name="term3073" />
									<xsl:text>:&#160;</xsl:text>
									<xsl:value-of select="Email"/>
									<br/>
								</xsl:if>
							</p>

						</div>
					</div>
				</div>
				<div class="col-lg-4">
					<xsl:apply-templates select="." mode="contact-card"/>
				</div>
				<div class="col-lg-4">
					<xsl:apply-templates select="parent::Order/Contact[@type='Delivery Address']" mode="contact-card"/>
				</div>
				<div class="col-lg-12">
					<xsl:if test="not(/Page/Cart/Order/@cmd='ShowInvoice') and not(/Page/Cart/Order/@cmd='MakePayment') and (ancestor::*[name()='Cart'])">
						<xsl:if test="/Page/Cart/Order/@cmd!='MakePayment'">
							<a href="{$parentURL}?pgid={/Page/@id}&amp;{$cartType}Cmd={$type}" class="btn  btn-sm btn-outline-primary address-edit-btn">
								<i class="fa fa-pencil me-1">
									<xsl:text> </xsl:text>
								</i>
								<xsl:call-template name="term4022"/>
								<xsl:text> </xsl:text>
							</a>
						</xsl:if>
					</xsl:if>
				</div>
			</div>
		</div>

	</xsl:template>
	<xsl:template match="Contact" mode="contact-card">
		<xsl:param name="parentURL"/>
		<xsl:param name="cartType"/>
		<xsl:variable name="secureURL">
			<xsl:call-template name="getSecureURL"/>
		</xsl:variable>
		<xsl:variable name="type">
			<xsl:value-of select="substring-before(@type,' ')"/>
		</xsl:variable>
		<div class="card cart-address-card  mb-0">
			<div class="card-body">

				<h2 class="addressTitle card-title">
					<xsl:choose>
						<xsl:when test="@type = 'Billing Address'">
							<xsl:call-template name="term4033"/>
						</xsl:when>
						<xsl:when test="@type = 'Delivery Address'">
							<xsl:call-template name="term4034"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="@type"/>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:text> </xsl:text>

				</h2>
				<p>
					<xsl:value-of select="Street"/>
					<br/>
					<xsl:value-of select="City"/>
					<br/>
					<xsl:if test="State/node()!=''">
						<xsl:value-of select="State"/>
						<xsl:text> </xsl:text>
						<br/>
					</xsl:if>
					<xsl:value-of select="PostalCode"/>
					<br/>
					<xsl:if test="Country/node()!=''">
						<xsl:value-of select="Country"/>
						<br/>
					</xsl:if>
				</p>
			</div>
		</div>
	</xsl:template>
	<!--#-->
	<!--############################## Error Reports ################################-->
	<!--#-->
	<xsl:template match="Order" mode="orderErrorReports">
		<xsl:if test="@errorMsg &gt; 0">
			<div class="errorMessage">
				<xsl:choose>
					<!-- Error No. 1 - 99 : Standard errors from the Cart -->
					<xsl:when test="@errorMsg='1'">
						<!--<strong>The order has timed out and cannot continue</strong>, due to one of the following two reasons:<br/><br/>
              1. The order had been left for over ten minutes without any updates.  The details are automatically removed for security purposes.<br/><br/>
              2. You may have disabled cookies or they are undetectable.  The shopping cart requires cookies to be enabled in order to proceed.<br/><br/>
              Please ensure cookies are enabled in your browser to continue shopping, or call for assistance.  No transaction has been made.-->
						<xsl:call-template name="term3030" />
					</xsl:when>
					<xsl:when test="@errorMsg='2'">
						<!--The item(s) you are trying to add cannot be added to this shopping basket. <br/>
              <br/> Please proceed to the checkout and pay for the items in the basket, and then continue with your shopping.-->
						<xsl:call-template name="term3031" />
					</xsl:when>
					<xsl:when test="@errorMsg='3'">
						<!--There is no valid delivery option for this order.  This may be due to a combination of location, price, weight or quantity.<br/>
              <br/> Please call for assistance.-->
						<xsl:call-template name="term3032" />
					</xsl:when>
					<!-- Error No. 100+: Payment gateway errors-->
					<!-- Worldpay errors-->
					<xsl:when test="@errorMsg='1000'">
						<!--The transaction was cancelled during the payment processing - this was either at your request or the request of our payment provider, Worldpay.<br/>
              <br/> Please call for more information.-->
						<xsl:call-template name="term3033" />
					</xsl:when>
					<xsl:when test="@errorMsg='1001'">
						<!--The order reference could not be found, or the order did not have the correct status.  This may occur if you have tried to pay for the same order twice, or if there has been a long duration between visiting our payment provider, Worldpay's site and entering payment details.<br/>
              <br/> Please call for assistance.-->
						<xsl:call-template name="term3034" />
					</xsl:when>
					<xsl:when test="@errorMsg='1002'">
						<!--The payment provider, Worldpay, did not provide a valid response.<br/>
              <br/> Please call for assistance.-->
						<xsl:call-template name="term3035" />
					</xsl:when>
					<!-- Error No. 1000+ : Bespoke errors can be put here-->
				</xsl:choose>
			</div>

		</xsl:if>
		<xsl:if test="error/msg">
			<p class="errorMessage">
				<xsl:for-each select="error/msg">
					<xsl:sort select="@type" order="ascending" data-type="text"/>
					<span class="err_sub_msg {@type}">
						<xsl:copy-of select="node()"/>
					</span>
				</xsl:for-each>
				<a title="review cart" href="?cartCmd=Cart">Review cart &gt;</a>
			</p>
		</xsl:if>
	</xsl:template>

	<!--   ################   New Cart  ###############   -->

	<xsl:template match="Order[@cmd='Add' or @cmd='Cart' or @cmd='Confirm']" mode="orderProcessTitle">
		<!--<h2>-->
		<!--Your Order -->
		<!--xsl:call-template name="term3007" /-->
		<!--</h2>-->
	</xsl:template>

	<xsl:template match="Order[@cmd='Add' or @cmd='Cart' or @cmd='Confirm']" mode="orderProcess">


		<div class="cart-btns-btm clearfix">
			<h1>Your Basket</h1>
			<div class="row">
				<div class="col-lg-8">
					<xsl:apply-templates select="." mode="orderProcessTitle"/>
					<xsl:apply-templates select="." mode="orderErrorReports"/>
					<xsl:apply-templates select="." mode="orderAddresses"/>
				</div>
				<div class="col-lg-4">
					<form method="post" id="cart" class="ewXform">
						<div class="basket card">
							<div class="card-body">
								<xsl:if test="@cmd='Add' or @cmd='Cart'">
									<xsl:apply-templates select="." mode="suggestedItems"/>
								</xsl:if>
								<xsl:apply-templates select="." mode="orderItems">
									<xsl:with-param name="editQty">true</xsl:with-param>
								</xsl:apply-templates>

								<div class="cart-btns-btm clearfix">
									<xsl:apply-templates select="." mode="principleButton">
										<xsl:with-param name="buttonClass">btn-custom</xsl:with-param>
										<xsl:with-param name="buttonTitle">Continue</xsl:with-param>
									</xsl:apply-templates>
								</div>
							</div>
						</div>

						<div class="cart-btns-btm clearfix">
							<form method="post" id="cart" class="ewXform">
								<button type="submit" name="cartBrief" value="Continue Shopping" class="btn btn-link me-2 continue">
									<xsl:call-template name="term3060" />
								</button>
							</form>
							<xsl:if test="parent::Cart/@Process &gt; 3">
								<a href="?cartCmd=Quit" class="btn btn-link text-danger continue">
									<span class="empty-basket-icon">
										<i class="fas fa-trash">
											<xsl:text> </xsl:text>
										</i>
										<xsl:text> </xsl:text>
									</span>Empty Basket
								</a>
							</xsl:if>
						</div>
					</form>
				</div>
			</div>
		</div>
	</xsl:template>

	<!--#-->
	<!--############################## Order Procees - Billing ################################-->
	<!--#-->
	<xsl:template match="Order[@cmd='Billing' or @cmd='Delivery']" mode="orderProcessTitle">

	</xsl:template>

	<xsl:template match="Order[@cmd='Billing' or @cmd='Delivery']" mode="orderProcess">
		<xsl:apply-templates select="." mode="orderProcessTitle"/>
		<xsl:apply-templates select="." mode="orderErrorReports"/>

		<!--<div id="template_1_Column" class="template template_1_Column container">
			<h1>
				<xsl:call-template name="term4031" />
			</h1>
			<div class="row">
				<div class="col-lg-8">
					<xsl:apply-templates select="." mode="orderEditAddresses"/>
				</div>
				<div class="col-lg-4">
					<div class="card">
						<div class="card-body">
									<xsl:for-each select="Item">
									<div class="clearfix cart-item">
										<xsl:apply-templates select="." mode="orderItem">
											<xsl:with-param name="editQty" select="false()"/>
											<xsl:with-param name="showImg" select="'true'"/>
											<xsl:with-param name="cartThumbWidth" select="'50'"/>
											<xsl:with-param name="cartThumbHeight" select="'50'"/>											
										</xsl:apply-templates>
									</div>
								</xsl:for-each>
								<hr/>
								<xsl:apply-templates select="." mode="orderTotals"/>	
						</div>
					</div>
							
							
				</div>
			</div>
			
		</div>-->

		<h1>
			<xsl:call-template name="term4031" />
		</h1>
		<div class="row">
			<div class="col-lg-8">
				<xsl:apply-templates select="." mode="orderEditAddresses"/>
			</div>
			<div class="col-lg-4">
				<div class="card">
					<div class="card-body">
						<xsl:for-each select="Item">
							<div class="clearfix cart-item">
								<xsl:apply-templates select="." mode="orderItem">
									<xsl:with-param name="editQty" select="false()"/>
									<xsl:with-param name="showImg" select="'true'"/>
									<xsl:with-param name="cartThumbWidth" select="'50'"/>
									<xsl:with-param name="cartThumbHeight" select="'50'"/>
								</xsl:apply-templates>
							</div>
						</xsl:for-each>
						<hr/>
						<xsl:apply-templates select="." mode="orderTotals"/>
					</div>
				</div>


			</div>
		</div>

		<xsl:apply-templates select="." mode="displayNotes"/>
	</xsl:template>


	<!--#-->
	<!--##############################Order Edit Addresses ################################-->
	<!--#-->
	<xsl:template match="Order" mode="orderEditAddresses">
		<xsl:variable name="parentURL">
			<xsl:call-template name="getContentParURL"/>
		</xsl:variable>
		<xsl:if test="/Page/Contents/Content[@type='xform' and (@name='Delivery Address' or @name='Billing Address') ]">
			<!-- Don't display delivery address if hideDeliveryADress attribute is present -->
			<div id="edit-addresses">
				<xsl:choose>
					<xsl:when test="/Page/Contents/Content[@type='xform' and @name='Billing Address']">
						<div>
							<div id="billingAddress" class="cartAddress">
								<xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='Billing Address']" mode="xform"/>
							</div>
						</div>
					</xsl:when>
					<xsl:when test="Contact[@type='Billing Address']">
						<div>
							<div id="billingAddress" class="cartAddress">
								<xsl:apply-templates select="Contact[@type='Billing Address']" mode="cart">
									<xsl:with-param name="parentURL" select="$parentURL"/>
									<xsl:with-param name="cartType" select="'cart'"/>
								</xsl:apply-templates>
							</div>
						</div>
					</xsl:when>
				</xsl:choose>
				<xsl:variable name="hideDeliveryAddress"></xsl:variable>
				<xsl:if test="$hideDeliveryAddress=''">
					<xsl:choose>
						<xsl:when test="/Page/Contents/Content[@type='xform' and @name='Delivery Address']">
							<div>
								<div id="deliveryAddress" class="cartAddress delivery-address">
									<xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='Delivery Address']" mode="xform"/>
								</div>
							</div>
						</xsl:when>
						<xsl:when test="Contact[@type='Delivery Address']">
							<div>
								<div id="deliveryAddress" class="cartAddress delivery-address">
									<xsl:choose>
										<xsl:when test="@giftListId and false()">
											<p class="addressTitle">
												<!--Delivery Address Details-->
												<xsl:call-template name="term3036" />
												<xsl:text>:</xsl:text>
											</p>
											<p>
												<!--Your order will be delivered to-->
												<xsl:call-template name="term3037" />
												<xsl:text>: </xsl:text>
												<strong>
													<xsl:value-of select="Contact[@type='Delivery Address']/GivenName"/>
												</strong>
											</p>
										</xsl:when>
										<xsl:otherwise>
											<xsl:apply-templates select="Contact[@type='Delivery Address']" mode="cart">
												<xsl:with-param name="parentURL" select="$parentURL"/>
												<xsl:with-param name="cartType" select="'cart'"/>
											</xsl:apply-templates>
										</xsl:otherwise>
									</xsl:choose>
								</div>
							</div>
						</xsl:when>
					</xsl:choose>
				</xsl:if>
			</div>
		</xsl:if>
	</xsl:template>


	<xsl:template match="Order" mode="orderTotals">
		<div class="product-totals">
			<span class="amount-label">Item Total: </span>
			<span class="amount">
				<xsl:apply-templates select="/Page" mode="formatPrice">
					<xsl:with-param name="price" select="@total - @shippingCost"/>
					<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
				</xsl:apply-templates>
			</span>
		</div>
		<xsl:if test="@shippingType &gt; 0 and @shippingDesc!='No Delivery Required-'">
			<div class="shipping">
				<span class="shipping-title">
					<xsl:call-template name="term3044" />
					<xsl:text>:</xsl:text>					
				</span>
				<span class="amount">
					<xsl:text>&#160;</xsl:text>
					<xsl:apply-templates select="/Page" mode="formatPrice">
						<xsl:with-param name="price" select="@shippingCost"/>
						<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
					</xsl:apply-templates>
				</span>
			</div>
			<div class="shipping-desc">
		<xsl:choose>
						<xsl:when test="/Page/Cart/Order/Shipping">
							<xsl:value-of select="/Page/Cart/Order/Shipping/Name/node()"/>
							<strong>&#160;-&#160;</strong>
							<xsl:value-of select="/Page/Cart/Order/Shipping/Carrier/node()"/>
							<strong>&#160;-&#160;</strong>
							<xsl:value-of select="/Page/Cart/Order/Shipping/DeliveryTime/node()"/>
						</xsl:when>
						<xsl:otherwise>
							<xsl:value-of select="/Page/Cart/Order/@shippingDesc"/>
						</xsl:otherwise>
					</xsl:choose>
			&#160;
			</div>
		</xsl:if>
		<div class="totals-row">
			<xsl:if test="@vatRate &gt; 0">
				<div class="vat-row">
					<div class="subTotal">
						<span>
							<!--Sub Total-->
							<xsl:call-template name="term3045" />
							<xsl:text>: </xsl:text>
						</span>
						<span class=" amount">
							<!--  Remmed by Rob<xsl:value-of select="/Page/Cart/@currencySymbol"/>
                <xsl:value-of select="format-number(@totalNet, '0.00')"/>-->
							<xsl:apply-templates select="/Page" mode="formatPrice">
								<xsl:with-param name="price" select="@totalNet"/>
								<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
							</xsl:apply-templates>
						</span>
					</div>

					<div class="vat">
						<span>
							<xsl:choose>
								<xsl:when test="//Cart/Contact/Address/Country='United Kingdom'">
									<!--VAT at-->
									<xsl:call-template name="term3046" />
									<xsl:text>&#160;</xsl:text>
								</xsl:when>
								<xsl:otherwise>
									<!--Tax at-->
									<xsl:call-template name="term3047" />
									<xsl:text>&#160;</xsl:text>
								</xsl:otherwise>
							</xsl:choose>
							<xsl:value-of select="format-number(@vatRate, '#.00')"/>%:
						</span>
						<span class="amount">
							<!--  Remmed by Rob
				<span class="currency">
                  <xsl:value-of select="/Page/Cart/@currencySymbol"/>
                </span>
                <xsl:value-of select="format-number(@vatAmt, '0.00')"/>
				-->
							<xsl:apply-templates select="/Page" mode="formatPrice">
								<xsl:with-param name="price" select="@vatAmt"/>
								<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
							</xsl:apply-templates>
						</span>
					</div>
				</div>
			</xsl:if>
			<div class="total">
				<span>
					<xsl:choose>
						<xsl:when test="@cmd='ShowInvoice'">
							<xsl:text>Total Paid:&#160;</xsl:text>
						</xsl:when>
						<xsl:otherwise>
							<xsl:text>Total Payable:&#160;</xsl:text>
						</xsl:otherwise>
					</xsl:choose>
				</span>
				<span class="amount">
					<xsl:apply-templates select="/Page" mode="formatPrice">
						<xsl:with-param name="price" select="@total"/>
						<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
					</xsl:apply-templates>
				</span>
			</div>
			<xsl:if test="@paymentMade &gt; 0">
				<div class="cart-row">
					<div class="total">
						<xsl:choose>
							<xsl:when test="@transStatus">
								<!--Transaction Made-->
								<xsl:call-template name="term3049" />
							</xsl:when>
							<xsl:when test="@payableType='settlement' and not(@transStatus)">
								<!--Payment Received-->
								<xsl:call-template name="term3050" />
							</xsl:when>
						</xsl:choose>
					</div>
					<div class="total amount">
						<xsl:value-of select="$currency"/>
						<xsl:value-of select="format-number(@paymentMade, '0.00')"/>
					</div>
				</div>
			</xsl:if>
			<xsl:if test="@payableAmount &lt; @total">

				<div class="total">
					<xsl:choose>
						<xsl:when test="@payableType='deposit' and not(@transStatus)">
							<!--Deposit Payable-->
							<xsl:call-template name="term3051" />
						</xsl:when>
						<xsl:when test="@payableType='settlement' or (@payableType='deposit' and @transStatus)">
							<!--Amount Outstanding-->
							<xsl:call-template name="term3052" />
						</xsl:when>
					</xsl:choose>
				</div>
				<div class="total amount">
					<xsl:value-of select="$currency"/>
					<xsl:value-of select="format-number(@payableAmount, '0.00')"/>
				</div>
			</xsl:if>
		</div>

	</xsl:template>

	<!--#-->
	<!--############################## Quote Procees - Logon ################################-->
	<!--#-->
	<xsl:template match="Order[@cmd='Logon']" mode="orderProcessTitle">
	</xsl:template>

	<xsl:template match="Order[@cmd='Logon']" mode="orderProcess">
		<xsl:apply-templates select="." mode="orderProcessTitle"/>
		<xsl:apply-templates select="." mode="orderErrorReports"/>
		<xsl:variable name="anySub">
			<xsl:for-each select="/Page/Cart/Order/Item">
				<xsl:if test="contentType/node()='Subscription'">
					<xsl:text>SubHere</xsl:text>
				</xsl:if>
			</xsl:for-each>
		</xsl:variable>

		<div class="row">
			<div class="col-lg-8">
				<form method="post" id="cart" class="ewXform">
					<xsl:apply-templates select="." mode="orderItems">
						<xsl:with-param name="editQty">true</xsl:with-param>
					</xsl:apply-templates>


				</form>
				<xsl:if test="@showDiscountCodeBox='true'">
					<div class="row">
						<div class="basket-promo form-group col-lg-4">
							<label name="promotionalcode" id="promotionalcode" for="ex1" class="sr-only">Promo Code</label>
							<div class="input-group">
								<input id="txtPromoCode" placeholder="Enter promo code" class="form-control" type="text" value="{Notes/PromotionalCode/node()}"/>
								<div class="input-group-btn">
									<a href="#" class="btn btn-custom " id="addPromoCode">Add</a>
								</div>
							</div>
						</div>
					</div>
				</xsl:if>
			</div>
			<div class="col-lg-4">
				<div class="card account-options-card">
					<div class="card-body">
						<div class="aoc-inner">
							<xsl:apply-templates select="." mode="orderTotals"/>
							<a data-bs-toggle="modal" data-bs-target="#cartRegisterBox" role="button" class="btn btn-custom btn-block">
								Create a new account
							</a>
							<a data-bs-toggle="modal" data-bs-target="#cartLogonBox" role="button" class="btn btn-outline-primary btn-block">
								Sign in
							</a>
							<xsl:choose>
								<xsl:when test="Item/productDetail[@type='Subscription']">
									<br/>
									<div class="alert alert-info">
										For regular payments you need to create an account with us.
									</div>
								</xsl:when>
								<xsl:otherwise>
									<a href="?pgid={/Page/@id}&amp;cartCmd=Notes" class="without-account-btn">
										Continue without account
									</a>
								</xsl:otherwise>
							</xsl:choose>

							<xsl:apply-templates select="." mode="orderProcessSkipButton"/>
						</div>
					</div>

				</div>

				<div id="cartLogonBox" class="modal fade" tabindex="-1" role="dialog">
					<div class="modal-dialog" role="document">
						<div class="modal-content">
							<div class="modal-header">
								<h4 class="modal-title">Sign in</h4>
								<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close">
									<xsl:text> </xsl:text>
								</button>
							</div>
							<div class="modal-body">
								<xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='UserLogon']" mode="xform"/>
							</div>
						</div>
					</div>
				</div>

				<div id="cartRegisterBox" class="modal fade" tabindex="-1" role="dialog">
					<div class="modal-dialog" role="document">
						<div class="modal-content">
							<div class="modal-header">
								<h3 class="modal-title">Create new account</h3>
								<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close">
									<xsl:text> </xsl:text>
								</button>
							</div>
							<div class="modal-body">
								<xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='CartRegistration']" mode="xform"/>
							</div>
						</div>
					</div>
				</div>
				<div class="cart-btns-btm clearfix hidden-xs">
					<form method="post" id="cart" class="ewXform">
						<button type="submit" name="cartBrief" value="Continue Shopping" class="btn btn-link continue">
							<xsl:call-template name="term3060" />
							<xsl:text> </xsl:text>
						</button>
					</form>
				</div>
			</div>
			<xsl:if test="/Page/Cart/Order/Notes/PromotionalCode!=''">
				<xsl:apply-templates select="." mode="principleButton"/>
				<button type="submit" name="cartBrief" value="Continue Shopping" class="btn btn-link continue">
					<xsl:call-template name="term3060" />
					<xsl:text> </xsl:text>
				</button>
				<button type="submit" name="cartUpdate" value="Update Order" class="btn btn-info btn-sm update">
					<i class="fa fa-refresh">
						<xsl:text> </xsl:text>
					</i>
					<xsl:text> </xsl:text>
					<xsl:call-template name="term3061" />
				</button>
				<button type="submit" name="cartQuit" value="Empty Order" class="btn btn-info btn-sm empty">
					<i class="fa fa-trash-o">
						<xsl:text> </xsl:text>
					</i>
					<xsl:text> </xsl:text>
					<xsl:call-template name="term3062" />
				</button>
				<xsl:apply-templates select="." mode="orderItems">
					<xsl:with-param name="editQty">true</xsl:with-param>
				</xsl:apply-templates>
			</xsl:if>
		</div>
	</xsl:template>

	<xsl:template match="Order[@cmd='Logon']" mode="orderProcessSkipButton">
		<!-- Overriden in Payment Provider-->
	</xsl:template>

	<!--#-->
	<!--############################## Order Procees - Notes ################################-->
	<!--#-->
	<xsl:template match="Order[@cmd='Notes']" mode="orderProcessTitle">

	</xsl:template>

	<xsl:template match="Order[@cmd='Notes']" mode="orderProcess">
		<xsl:apply-templates select="." mode="orderProcessTitle"/>
		<xsl:apply-templates select="." mode="orderErrorReports"/>
		<div class="box blueEdge cartBox">
			<div class="tl">
				<div class="tr">
					<h2 class="title">
						Additional Information
						<!--xsl:call-template name="term3009" /-->
					</h2>
				</div>
			</div>
			<div class="content">
				<xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='notesForm']" mode="xform"/>
			</div>
			<div class="bl">
				<div class="br">&#160;</div>
			</div>
		</div>
		<div class="box blueEdge">
			<form method="post" id="cart">
				<!--<div class="cartButtons">-->
				<!--input type="submit" name="quoteLogon" value="Proceed" class="button principle"/-->
				<button type="submit" name="cartBrief" value="Continue Shopping" class="btn btn-link continue">
					<xsl:call-template name="term3060" />
					<xsl:text> </xsl:text>
				</button>
				<button type="submit" name="cartUpdate" value="Update Order" class="btn btn-info btn-sm update">
					<i class="fa fa-refresh">
						<xsl:text> </xsl:text>
					</i>
					<xsl:text> </xsl:text>
					<xsl:call-template name="term3061" />
				</button>
				<button type="submit" name="cartQuit" value="Empty Order" class="btn btn-info btn-sm empty">
					<i class="fa fa-trash-o">
						<xsl:text> </xsl:text>
					</i>
					<xsl:text> </xsl:text>
					<xsl:call-template name="term3062" />
				</button>
				<!--</div>-->
				<xsl:apply-templates select="." mode="orderItems">
					<xsl:with-param name="editQty">true</xsl:with-param>
				</xsl:apply-templates>
				<!--<div class="cartButtons">-->
				<!--input type="submit" name="quoteLogon" value="Proceed" class="button principle"/-->
				<button type="submit" name="cartBrief" value="Continue Shopping" class="btn btn-link continue">
					<xsl:call-template name="term3060" />
					<xsl:text> </xsl:text>
				</button>
				<button type="submit" name="cartUpdate" value="Update Order" class="btn btn-info btn-sm update">
					<i class="fa fa-refresh">
						<xsl:text> </xsl:text>
					</i>
					<xsl:text> </xsl:text>
					<xsl:call-template name="term3061" />
				</button>
				<button type="submit" name="cartQuit" value="Empty Order" class="btn btn-info btn-sm empty">
					<i class="fa fa-trash-o">
						<xsl:text> </xsl:text>
					</i>
					<xsl:text> </xsl:text>
					<xsl:call-template name="term3062" />
				</button>
				<!--</div>-->
			</form>
		</div>
	</xsl:template>

	<xsl:template match="Order[@cmd='Discounts']" mode="orderProcessTitle">
		<h2>
			<!--Your Order - Please enter a discount code-->
			<xsl:call-template name="term3008" />
		</h2>
	</xsl:template>



	<xsl:template match="Order[@cmd='Discounts']" mode="orderProcess">
		<xsl:apply-templates select="." mode="orderProcessTitle"/>
		<xsl:apply-templates select="." mode="orderErrorReports"/>
		<div class="card-default">
			<div class="card-body">
				<xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='discountsForm']" mode="xform"/>
			</div>
		</div>

		<div class="box blueEdge">
			<form method="post" id="cart">
				<div class="cartButtons">
					<!--input type="submit" name="quoteLogon" value="Proceed" class="button principle"/-->
					<button type="submit" name="cartBrief" value="Continue Shopping" class="btn btn-link continue">
						<xsl:call-template name="term3060" />
						<xsl:text> </xsl:text>
					</button>
					<button type="submit" name="cartUpdate" value="Update Order" class="btn btn-info btn-sm update">
						<i class="fa fa-refresh">
							<xsl:text> </xsl:text>
						</i>
						<xsl:text> </xsl:text>
						<xsl:call-template name="term3061" />
					</button>
					<button type="submit" name="cartQuit" value="Empty Order" class="btn btn-info btn-sm empty">
						<i class="fa fa-trash-o">
							<xsl:text> </xsl:text>
						</i>
						<xsl:text> </xsl:text>
						<xsl:call-template name="term3062" />
					</button>
				</div>
				<div>
					<xsl:apply-templates select="." mode="orderItems">
						<xsl:with-param name="editQty">true</xsl:with-param>
					</xsl:apply-templates>
				</div>
				<div class="cartButtons">
					<button type="submit" name="cartBrief" value="Continue Shopping" class="btn btn-link continue">
						<xsl:call-template name="term3060" />
						<xsl:text> </xsl:text>
					</button>
					<button type="submit" name="cartUpdate" value="Update Order" class="btn btn-info btn-sm update">
						<i class="fa fa-refresh">
							<xsl:text> </xsl:text>
						</i>
						<xsl:text> </xsl:text>
						<xsl:call-template name="term3061" />
					</button>
					<button type="submit" name="cartQuit" value="Empty Order" class="btn btn-info btn-sm empty">
						<i class="fa fa-trash-o">
							<xsl:text> </xsl:text>
						</i>
						<xsl:text> </xsl:text>
						<xsl:call-template name="term3062" />
					</button>
				</div>
			</form>
		</div>
	</xsl:template>

	<!--#-->
	<!--############################## Order Process - Choose Payment / Shipping Options ################################-->
	<!--#-->
	<xsl:template match="Order[@cmd='ChoosePaymentShippingOption']" mode="orderProcessTitle">

	</xsl:template>

	<xsl:template match="Order[@cmd='ChoosePaymentShippingOption']" mode="orderProcess">
		<xsl:apply-templates select="." mode="orderProcessTitle"/>
		<xsl:apply-templates select="." mode="orderErrorReports"/>
		<h1>Review Your Order</h1>
		<div class="row">
			<div class="col-lg-8">
				<div class="cartBox check-address">
					<xsl:apply-templates select="." mode="orderAddresses"/>
				</div>
				<xsl:apply-templates select="." mode="displayNotes"/>
			</div>
			<div class="col-lg-4">
				<div class="cart-summary">
					<div class="card cartBox payment-tcs">
						<div class="card-body">
							<xsl:apply-templates select="." mode="orderItems">
								<xsl:with-param name="editQty">false</xsl:with-param>
								<xsl:with-param name="showImg" select="'true'"/>
								<xsl:with-param name="cartThumbWidth" select="'50'"/>
								<xsl:with-param name="cartThumbHeight" select="'50'"/>
							</xsl:apply-templates>

							<xsl:apply-templates select="." mode="orderTotals"/>

							<xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='optionsForm']" mode="xform"/>
						</div>
					</div>
				</div>
				<div class="cart-btns-btm">
					<form method="post" id="cart" class="ewXform">
						<button type="submit" name="cartBrief" value="Continue Shopping" class="btn btn-link continue">
							<xsl:call-template name="term3060" />
							<xsl:text> </xsl:text>
						</button>
					</form>
				</div>
			</div>
		</div>
	</xsl:template>
	<!--#-->
	<!--############################## Order Process - Enter Payment Details ################################-->
	<!--#-->
	<xsl:template match="Order[@cmd='EnterPaymentDetails' or @cmd='SubmitPaymentDetails']" mode="orderProcessTitle">

	</xsl:template>

	<xsl:template match="Order[(@cmd='EnterPaymentDetails' or @cmd='SubmitPaymentDetails') and /Page/Contents/Content[@type='xform' and (@name='Secure3D' or @name='Secure3DReturn')]]" mode="orderProcessTitle">

	</xsl:template>

	<xsl:template match="Order[@cmd='EnterPaymentDetails' or @cmd='SubmitPaymentDetails']" mode="orderProcess">
		<xsl:apply-templates select="." mode="orderErrorReports"/>
		<div class="card ccForm">

			<h4 class="card-header">
				<xsl:call-template name="term3020" />
			</h4>

			<div class="card-body">
				<xsl:apply-templates select="/Page/Contents/Content[@type='xform' and (contains(@name,'PayForm') or @name='Secure3D' or @name='Secure3DReturn')]" mode="xform"/>
			</div>
		</div>
		<!--form method="post" id="cart" class="ewXform">
      <xsl:apply-templates select="." mode="orderItems"/>
      <input type="submit" name="cartUpdate" value="Revise Order" class="button continue"/>
      <input type="submit" name="cartQuit" value="Cancel Order" class="button empty"/>
      <div class="terminus">&#160;</div>
    </form-->
	</xsl:template>

	<xsl:template match="input[@bind='cContactName']" mode="xform_value_alt">
		<xsl:variable name="inlineHint">
			<xsl:apply-templates select="." mode="getInlineHint"/>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test ="/Page/User">
				<xsl:variable name="userName">
					<xsl:value-of select="/Page/User/FirstName/node()"/>
					<xsl:text> </xsl:text>
					<xsl:value-of select="/Page/User/LastName/node()"/>
				</xsl:variable>
				<xsl:value-of select="$userName"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$inlineHint"/>
			</xsl:otherwise>
		</xsl:choose>

	</xsl:template>

	<xsl:template match="input[@bind='cContactEmail']" mode="xform_value_alt">
		<xsl:variable name="inlineHint">
			<xsl:apply-templates select="." mode="getInlineHint"/>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test ="/Page/User">
				<xsl:variable name="userName">
					<xsl:value-of select="/Page/User/Email/node()"/>
				</xsl:variable>
				<xsl:value-of select="$userName"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="$inlineHint"/>
			</xsl:otherwise>
		</xsl:choose>

	</xsl:template>

	<!-- -->
	<xsl:template match="Order" mode="displayNotes">
		<xsl:variable name="parentURL">
			<xsl:call-template name="getContentParURL"/>
		</xsl:variable>
		<xsl:if test="Notes/Notes/node()!='' or Notes/PromotionalCode/node()!=''">
			<xsl:if test="Notes/Notes/node()!=''">

				<div class="alert alert-success notes">

					<xsl:if test="not(/Page/Cart/Order/@cmd='ShowInvoice') and not(/Page/Cart/Order/@cmd='MakePayment') and (ancestor::*[name()='Cart'])">

						<xsl:if test="not(@readonly) and not(@cartCmd='') and Notes/Notes/node()!=''">
							<a href="{$parentURL}?cartCmd=Notes" class="btn btn-custom pull-right">
								<i class="fa fa-pencil">&#160;</i>&#160;
								<xsl:attribute name="title">
									<!--Click here to edit the notes on this order.-->
									<xsl:call-template name="term3012" />
								</xsl:attribute>
								<!--Edit Notes-->
								<xsl:call-template name="term3013" />
							</a>
						</xsl:if>
					</xsl:if>
					<h4 class="alert-title">
						<!--Additional information for Your Order-->
						<xsl:call-template name="term3010" />
					</h4>


					<xsl:apply-templates select="Notes/Notes/node()" mode="cleanXhtml"/>

				</div>
			</xsl:if>
			<xsl:if test="Notes/PromotionalCode/node()!=''">
				<p>
					<!--Promotional Code entered-->
					<xsl:call-template name="term3011" />
					<xsl:text>:&#160;</xsl:text>
					<xsl:apply-templates select="Notes/PromotionalCode/node()" mode="cleanXhtml"/>
				</p>
			</xsl:if>

		</xsl:if>
	</xsl:template>

	<xsl:template match="*" mode="displayNoteLine">
		<p>
			<xsl:choose>
				<xsl:when test="@label and @label!=''">
					<xsl:value-of select="@label"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="name()"/>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:text>: </xsl:text>
			<xsl:value-of select="node()"/>
		</p>
	</xsl:template>

	<xsl:template match="Item" mode="displayNoteLine">
		<p>
			<xsl:value-of select="@name"/>
			<xsl:text> </xsl:text>
			<xsl:value-of select="@number"/>
			<xsl:text> </xsl:text>
			<xsl:value-of select="FirstName/node()"/>
			<xsl:text> </xsl:text>
			<xsl:value-of select="LastName/node()"/>
			<xsl:text> </xsl:text>
			<xsl:value-of select="Email/node()"/>
			<xsl:text> </xsl:text>
			<xsl:value-of select="JobTitle/node()"/>
			<xsl:text> </xsl:text>
			<xsl:value-of select="Company/node()"/>
		</p>
	</xsl:template>


	<xsl:template match="Order" mode="orderProgressLegend">
		<xsl:variable name="bMembership">
			<xsl:call-template name="getSettings">
				<xsl:with-param name="sectionName" select="'web'"/>
				<xsl:with-param name="valueName" select="'Membership'"/>
			</xsl:call-template>
		</xsl:variable>

		<!--new cart stepper-->
		<div id="cartStepper" class="fuelux">
			<div id="MyWizard" class="wizard">

				<ul class="nav">
					<li>
						<xsl:attribute name="class">
							<xsl:if test="/Page/Cart/Order/@cmd='Cart'">
								<xsl:text> active</xsl:text>
							</xsl:if>
							<xsl:if test="/Page/Cart/Order/@cmd='Logon' or /Page/Cart/Order/@cmd='Notes' or /Page/Cart/Order/@cmd='Delivery' or /Page/Cart/Order/@cmd='Billing' or /Page/Cart/Order/@cmd='ChoosePaymentShippingOption' or /Page/Cart/Order/@cmd='EnterPaymentDetails'">
								<xsl:text> complete</xsl:text>
							</xsl:if>
						</xsl:attribute>
						<span class="badge bg-primary">1</span>
						<span class="step-text">
							<xsl:text>Cart Details</xsl:text>
						</span>
						<span class="chevron">
							<xsl:text> </xsl:text>
						</span>
					</li>
					<xsl:if test="$bMembership='on'">
						<li>
							<xsl:attribute name="class">
								<xsl:if test="/Page/Cart/Order/@cmd='Logon'">
									<xsl:text> active</xsl:text>
								</xsl:if>
								<xsl:if test="/Page/Cart/Order/@cmd='Delivery' or /Page/Cart/Order/@cmd='Notes' or /Page/Cart/Order/@cmd='Billing' or /Page/Cart/Order/@cmd='ChoosePaymentShippingOption' or /Page/Cart/Order/@cmd='EnterPaymentDetails'">
									<xsl:text> complete</xsl:text>
								</xsl:if>
							</xsl:attribute>

							<span class="badge">2</span>
							<span class="step-text">
								<xsl:text>Sign In / Register</xsl:text>
							</span>
							<span class="chevron">
								<xsl:text> </xsl:text>
							</span>
						</li>
					</xsl:if>

					<li>
						<xsl:attribute name="class">
							<xsl:text>step</xsl:text>
							<xsl:if test="/Page/Cart/Order/@cmd='Billing'">
								<xsl:text> active</xsl:text>
							</xsl:if>
							<xsl:if test="/Page/Cart/Order/@cmd='Delivery' or /Page/Cart/Order/@cmd='Notes' or /Page/Cart/Order/@cmd='ChoosePaymentShippingOption' or /Page/Cart/Order/@cmd='EnterPaymentDetails'">
								<xsl:text> complete</xsl:text>
							</xsl:if>
						</xsl:attribute>
						<xsl:choose>
							<xsl:when test="$bMembership='on'">
								<span class="badge">3</span>
							</xsl:when>
							<xsl:otherwise>
								<span class="badge">2</span>
							</xsl:otherwise>
						</xsl:choose>
						<span class="step-text">
							<xsl:text>Address</xsl:text>
						</span>
						<span class="chevron">
							<xsl:text> </xsl:text>
						</span>
					</li>

					<li>
						<xsl:attribute name="class">
							<xsl:text>step</xsl:text>
							<xsl:if test="/Page/Cart/Order/@cmd='ChoosePaymentShippingOption'">
								<xsl:text> active</xsl:text>
							</xsl:if>
							<xsl:if test="/Page/Cart/Order/@cmd='EnterPaymentDetails'">
								<xsl:text> completed</xsl:text>
							</xsl:if>
						</xsl:attribute>
						<xsl:choose>
							<xsl:when test="$bMembership='on'">
								<span class="badge">4</span>
							</xsl:when>
							<xsl:otherwise>
								<span class="badge">3</span>
							</xsl:otherwise>
						</xsl:choose>
						<span class="step-text">
							<xsl:text>Confirm</xsl:text>
						</span>
						<span class="chevron">
							<xsl:text> </xsl:text>
						</span>
					</li>

					<li>
						<xsl:attribute name="class">
							<xsl:text>step last</xsl:text>
							<xsl:if test="/Page/Cart/Order/@cmd='EnterPaymentDetails'">
								<xsl:text> active</xsl:text>
							</xsl:if>
						</xsl:attribute>
						<xsl:choose>
							<xsl:when test="$bMembership='on'">
								<span class="badge">5</span>
							</xsl:when>
							<xsl:otherwise>
								<span class="badge">4</span>
							</xsl:otherwise>
						</xsl:choose>
						<span class="step-text">
							<xsl:text>Payment</xsl:text>
						</span>
						<span class="chevron">
							<xsl:text> </xsl:text>
						</span>
					</li>
				</ul>
			</div>

		</div>
		<!--end cart stepper-->
	</xsl:template>

	<xsl:template match="*[@bind='FirstName_0']" mode="xform_value_alt">
		<xsl:value-of select="/Page/User/FirstName/node()"/>
	</xsl:template>

	<xsl:template match="*[@bind='LastName_0']" mode="xform_value_alt">
		<xsl:value-of select="/Page/User/LastName/node()"/>
	</xsl:template>

	<xsl:template match="*[@bind='Email_0']" mode="xform_value_alt">
		<xsl:value-of select="/Page/User/Email/node()"/>
	</xsl:template>

	<xsl:template match="*[@bind='Company_0']" mode="xform_value_alt">
		<xsl:value-of select="/Page/User/Company/node()"/>
	</xsl:template>

	<xsl:template match="*[@bind='JobTitle_0']" mode="xform_value_alt">
		<xsl:value-of select="/Page/User/Position/node()"/>
	</xsl:template>

	<xsl:template match="Order" mode="suggestedItems">
		<xsl:if test="$page/ContentDetail/Content/Content[@type='Product' and @id!=$page/Cart/Order/Item/@contentId]">
			<div class="relatedcontent card bg-primary">

				<h4 class="card-header">Recommended with this purchase</h4>

				<div class="card-body">
					<xsl:for-each select="$page/ContentDetail/Content/Content[@type='Product' and @id!=$page/Cart/Order/Item/@contentId]">
						<xsl:apply-templates select="." mode="displayBriefRelated"/>
					</xsl:for-each>
				</div>
			</div>
		</xsl:if>
	</xsl:template>

	<xsl:template match="label[parent::textarea[contains(@class,'readonly terms-and-condiditons')]]">

	</xsl:template>

	<xsl:template match="label[parent::item and ancestor::select[@ref='confirmterms']]" mode="xform-label">
		I agree to the
		<a class="" data-bs-toggle="modal" data-bs-target="#terms-modal">
			terms and conditions
		</a>
	</xsl:template>

	<xsl:template match="textarea[contains(@class,'readonly terms-and-condiditons')]" mode="xform_legend">
		<!--<button type="button" class="btn btn-link continue" data-bs-toggle="modal" data-bs-target="#terms-modal">
			View terms and conditions
		</button>-->
	</xsl:template>

	<xsl:template match="textarea[contains(@class,'readonly terms-and-condiditons')]" mode="xform_control">
		<div class="modal modal-xl" tabindex="-1" id="terms-modal">
			<div class="modal-dialog">
				<div class="modal-content">
					<div class="modal-header">
						<h5 class="modal-title">Terms and Conditions</h5>
						<button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
					</div>
					<div class="modal-body">
						<small>
							<xsl:copy-of select="value/node()"/>
						</small>
					</div>
					<div class="modal-footer">
						<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
					</div>
				</div>
			</div>
		</div>
	</xsl:template>



	<xsl:template match="Order" mode="suggestedItems">

	</xsl:template>

	<xsl:template match="Item" mode="CartProductName">
		<xsl:value-of select="Name"/>
	</xsl:template>

	<xsl:template match="Item[contentType='SKU']" mode="CartProductName">
		<xsl:if test="productDetail/ParentProduct">
			<xsl:value-of select="substring(productDetail/ParentProduct/Content/@name,1,25)"/> -
		</xsl:if>
		<xsl:value-of select="Name"/>
	</xsl:template>

	<xsl:template match="Item[contentType='Ticket']" mode="CartProductName">
		<xsl:if test="productDetail/ParentProduct">
			<xsl:value-of select="substring(productDetail/ParentProduct/Content/@name,1,25)"/> -
		</xsl:if><br/>
		<xsl:value-of select="Name"/> -
		<xsl:call-template name="formatdate">
			<xsl:with-param name="date" select="productDetail/StartDate/node()" />
			<xsl:with-param name="format" select="'dddd'" />
		</xsl:call-template>
		<xsl:text> </xsl:text>
		<xsl:call-template name="DD_Mon_YY">
			<xsl:with-param name="date" select="productDetail/StartDate/node()"/>
		</xsl:call-template>
	</xsl:template>

	<xsl:template match="Item" mode="cartThumbWidth">50</xsl:template>
	<xsl:template match="Item" mode="cartThumbHeight">50</xsl:template>

	<xsl:template match="Item" mode="orderItem">
		<xsl:param name="editQty"/>
		<xsl:param name="showImg"/>
		<xsl:variable name="parentURL">
			<xsl:call-template name="getContentParURL"/>
		</xsl:variable>
		<xsl:variable name="secureURL">
			<xsl:call-template name="getSecureURL"/>
		</xsl:variable>
		<xsl:variable name="siteURL">
			<xsl:call-template name="getSiteURL"/>
		</xsl:variable>
		<xsl:variable name="cartThumbWidth">
			<xsl:apply-templates select="." mode="cartThumbWidth"/>
		</xsl:variable>
		<xsl:variable name="cartThumbHeight">
			<xsl:apply-templates select="." mode="cartThumbHeight"/>
		</xsl:variable>
		<xsl:choose>
			<xsl:when test="productDetail/Images/img[@class='detail']/@src!='' and $showImg!='false'">
				<div class="cart-thumbnail">
					<xsl:apply-templates select="productDetail" mode="displayCartImage">
						<xsl:with-param name="forceResize">true</xsl:with-param>
						<xsl:with-param name="crop">true</xsl:with-param>
						<xsl:with-param name="width">
							<xsl:value-of select="$cartThumbWidth" />
						</xsl:with-param>
						<xsl:with-param name="height">
							<xsl:value-of select="$cartThumbHeight" />
						</xsl:with-param>
					</xsl:apply-templates>
				</div>
			</xsl:when>
			<xsl:when test="productDetail/ParentProduct/Content/Images/img[@class='detail']/@src!='' and $showImg!='false'">
				<div class="cart-thumbnail">
					<xsl:apply-templates select="productDetail/ParentProduct/Content" mode="displayCartImage">
						<xsl:with-param name="forceResize">true</xsl:with-param>
						<xsl:with-param name="crop">true</xsl:with-param>
						<xsl:with-param name="width">
							<xsl:value-of select="$cartThumbWidth" />
						</xsl:with-param>
						<xsl:with-param name="height">
							<xsl:value-of select="$cartThumbHeight" />
						</xsl:with-param>
					</xsl:apply-templates>
				</div>
			</xsl:when>
		</xsl:choose>

		<div class="cart-desc">
			<a href="{$siteURL}{@url}" title="">
				<xsl:apply-templates select="." mode="CartProductName"/>
			</a>
			<xsl:if test="@ref and @ref!=''">
				<div class="ref">
					<xsl:value-of select="@ref"/>&#160;
					<xsl:for-each select="Item">
						<xsl:apply-templates select="option" mode="optionCodeConcat"/>
					</xsl:for-each>
				</div>
			</xsl:if>
			<xsl:if test="$editQty!='true'">
				<div class="qty-text">
					<span>Qty: </span>
					<xsl:value-of select="@quantity"/>
				</div>
			</xsl:if>
			<xsl:if test="productDetail[@type='Subscription']">
				<p class="duration">

					<xsl:apply-templates select="/Page" mode="formatPrice">
						<xsl:with-param name="price" select="productDetail/Prices/Price[@type='sale']"/>
						<xsl:with-param name="currency" select="$page/Cart/@currencySymbol"/>
					</xsl:apply-templates>&#160;
					<xsl:value-of select="productDetail/Prices/Price[@type='sale']/@suffix"/>&#160;then
					<xsl:apply-templates select="/Page" mode="formatPrice">
						<xsl:with-param name="price" select="productDetail/SubscriptionPrices/Price[@type='sale']"/>
						<xsl:with-param name="currency" select="$page/Cart/@currencySymbol"/>
					</xsl:apply-templates>&#160;
					<xsl:value-of select="productDetail/SubscriptionPrices/Price[@type='sale']/@suffix"/>
				</p>
			</xsl:if>
		</div>
		<!-- ################################# Line Options Info ################################# -->
		<xsl:if test="Item">
			<span class="optionList">
				<xsl:for-each select="Item">
					<xsl:value-of select="Name"/>
					<xsl:apply-templates select="option" mode="optionDetail"/>
					<xsl:if test="not(position()=last())">
						<xsl:text> / </xsl:text>
					</xsl:if>
				</xsl:for-each>
			</span>
		</xsl:if>
		<!-- ################################# Line Discount Info ################################# -->
		<xsl:if test="Discount">
			<xsl:for-each select="DiscountPrice/DiscountPriceLine[@UnitSaving &gt; 0]">
				<xsl:sort select="@PriceOrder"/>
				<xsl:variable name="DiscID">
					<xsl:value-of select="@nDiscountKey"/>
				</xsl:variable>
				<div class="discount">
					<xsl:if test="ancestor::Item/Discount[@nDiscountKey=$DiscID]/Images[@class='thumbnail']/@src!=''">
						<xsl:copy-of select="ancestor::Item/Discount[@nDiscountKey=$DiscID]/Images[@class='thumbnail']"/>
					</xsl:if>
					<span class="discountName">
						<xsl:value-of select="ancestor::Item/Discount[@nDiscountKey=$DiscID]/cDescription"/>
					</span>
					<xsl:text>&#160;&#160;&#160;</xsl:text>
					<xsl:call-template name="term3054" />
					<xsl:text>:&#160;</xsl:text>
					<xsl:apply-templates select="/Page" mode="formatPrice">
						<xsl:with-param name="price" select="@UnitSaving"/>
						<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
					</xsl:apply-templates>
				</div>
			</xsl:for-each>
			<xsl:for-each select="DiscountItem">
				<xsl:variable name="DiscID">
					<xsl:value-of select="@nDiscountKey"/>
				</xsl:variable>
				<div class="discount">
					<strong>
						DISCOUNT:<xsl:text> </xsl:text><xsl:value-of select="ancestor::Item/Discount[@nDiscountKey=$DiscID]/@cDiscountName"/>
					</strong>
					<xsl:text> </xsl:text>
					(<xsl:value-of select="@oldUnits - @Units"/>&#160;Item<xsl:if test="(@oldUnits - @Units) > 1">s</xsl:if>)
					<xsl:text> </xsl:text>
					Total Saving: <xsl:text> </xsl:text>
					<xsl:apply-templates select="/Page" mode="formatPrice">
						<xsl:with-param name="price" select="@TotalSaving"/>
						<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
					</xsl:apply-templates>

				</div>
			</xsl:for-each>
		</xsl:if>


		<div class="quantity">
			<div class="quantity-input">
				<xsl:if test="$editQty='true'">
					<xsl:choose>
						<xsl:when test="@quantity&lt;'10'">
							<label>Qty:</label>
							<select value="{@quantity}" class="cart-quantity" name="itemId-{@id}">
								<option value="1">
									<xsl:if test="@quantity=1">
										<xsl:attribute name="selected">selected</xsl:attribute>
									</xsl:if>
									<xsl:text>1</xsl:text>
								</option>
								<option value="2">
									<xsl:if test="@quantity=2">
										<xsl:attribute name="selected">selected</xsl:attribute>
									</xsl:if>
									<xsl:text>2</xsl:text>
								</option>
								<option value="3">
									<xsl:if test="@quantity=3">
										<xsl:attribute name="selected">selected</xsl:attribute>
									</xsl:if>
									<xsl:text>3</xsl:text>
								</option>
								<option value="4">
									<xsl:if test="@quantity=4">
										<xsl:attribute name="selected">selected</xsl:attribute>
									</xsl:if>
									<xsl:text>4</xsl:text>
								</option>
								<option value="5">
									<xsl:if test="@quantity=5">
										<xsl:attribute name="selected">selected</xsl:attribute>
									</xsl:if>
									<xsl:text>5</xsl:text>
								</option>
								<option value="6">
									<xsl:if test="@quantity=6">
										<xsl:attribute name="selected">selected</xsl:attribute>
									</xsl:if>
									<xsl:text>6</xsl:text>
								</option>
								<option value="7">
									<xsl:if test="@quantity=7">
										<xsl:attribute name="selected">selected</xsl:attribute>
									</xsl:if>
									<xsl:text>7</xsl:text>
								</option>
								<option value="8">
									<xsl:if test="@quantity=8">
										<xsl:attribute name="selected">selected</xsl:attribute>
									</xsl:if>
									<xsl:text>8</xsl:text>
								</option>
								<option value="9">
									<xsl:if test="@quantity=9">
										<xsl:attribute name="selected">selected</xsl:attribute>
									</xsl:if>
									<xsl:text>9</xsl:text>
								</option>
								<option value="10">10+</option>
							</select>
							<button type="submit" name="cartUpdate" value="Update Order" id="updateQty" class="btn btn-info btn-xs update hidden">
								<i class="fa fa-refresh">
									<xsl:text> </xsl:text>
								</i><xsl:text> </xsl:text>Update
							</button>
						</xsl:when>
						<xsl:otherwise>
							<div class="input-group">
								<input type="text" size="2" name="itemId-{@id}" value="{@quantity}" class="form-control">
									<xsl:if test="../@readonly">
										<xsl:attribute name="readonly">readonly</xsl:attribute>
									</xsl:if>
								</input>
								<button type="submit" name="cartUpdate" value="Update Order" class="btn btn-custom btn-sm update">
									<i class="fa fa-refresh">
										<xsl:text> </xsl:text>
									</i><xsl:text> </xsl:text>Update
								</button>
							</div>
						</xsl:otherwise>
					</xsl:choose>
				</xsl:if>
				&#160;
			</div>
			<xsl:if test="$editQty='true'">
				<div class="delete">
					<a href="{$parentURL}?cartCmd=Remove&amp;id={@id}" title="click here to remove this item from the list" class="delete-link">
						<span>Remove</span>
					</a>
				</div>
			</xsl:if>
			&#160;
		</div>
		<xsl:if test="not(/Page/Cart/@displayPrice='false')">
			<div class="cart-prices">
				<div class="lineTotal">
					<xsl:choose>
						<xsl:when test="@itemTotal">
							<xsl:apply-templates select="/Page" mode="formatPrice">
								<xsl:with-param name="price" select="@itemTotal"/>
								<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
							</xsl:apply-templates>
						</xsl:when>
						<xsl:otherwise>
							<xsl:apply-templates select="/Page" mode="formatPrice">
								<xsl:with-param name="price" select="(@price +(sum(*/@price)))* @quantity"/>
								<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
							</xsl:apply-templates>
						</xsl:otherwise>
					</xsl:choose>
					<xsl:if test="nDepositAmount&gt;0">
						<div class="deposit">
							Deposit: <xsl:apply-templates select="/Page" mode="formatPrice">
								<xsl:with-param name="price" select="(nDepositAmount/node())* @quantity"/>
								<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
							</xsl:apply-templates>
						</div>
					</xsl:if>
				</div>
				<xsl:if test="@quantity!='1'">
					<div class="linePrice">
						<span class="open-braket-cart">
							<xsl:text>(</xsl:text>
						</span>
						<xsl:variable name="itemPrice" select="@itemTotal div @quantity"/>
						<xsl:if test="DiscountPrice/@OriginalUnitPrice &gt; $itemPrice">
							<strike>
								<xsl:apply-templates select="/Page" mode="formatPrice">
									<xsl:with-param name="price" select="DiscountPrice/@OriginalUnitPrice"/>
									<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
								</xsl:apply-templates>
							</strike>
							<br/>
						</xsl:if>
						<xsl:apply-templates select="/Page" mode="formatPrice">
							<xsl:with-param name="price" select="$itemPrice"/>
							<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
						</xsl:apply-templates>
						<xsl:for-each select="Item[@price &gt; 0]">
							<br/>
							<span class="optionList">
								<xsl:apply-templates select="/Page" mode="formatPrice">
									<xsl:with-param name="price" select="@price"/>
									<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
								</xsl:apply-templates>
							</span>
						</xsl:for-each>
						<xsl:text> each</xsl:text>
						<span class="close-braket-cart">
							<xsl:text>)</xsl:text>
						</span>
					</div>
				</xsl:if>
			</div>
		</xsl:if>
	</xsl:template>



	<xsl:template match="Order" mode="principleButton">
		<xsl:param name="buttonTitle"/>
		<!-- Optional, defaults to Proceed -->
		<xsl:param name="buttonCmd"/>
		<!-- Optional, defaults to cartProceed -->
		<xsl:param name="buttonClass"/>
		<!-- Optional -->
		<xsl:variable name="bTitle">
			<xsl:choose>
				<xsl:when test="$buttonTitle=''">
					<!--Proceed-->
					<xsl:call-template name="term3006" />
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$buttonTitle"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="bCmd">
			<xsl:choose>
				<xsl:when test="$buttonCmd=''">cartProceed</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$buttonCmd"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:if test="not(@errorMsg &gt; 0 or error/msg)">
			<!-- Only show button if no errors present -->
			<button type="submit" name="{$bCmd}" value="{$bTitle}" class="btn {$buttonClass} principle">
				<xsl:value-of select="$bTitle" />
			</button>
		</xsl:if>
	</xsl:template>


	<xsl:template match="Content" mode="showQuantity">
		<xsl:variable name="id">
			<xsl:choose>
				<xsl:when test="@type='SKU'">
					<xsl:value-of select="../@id"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="@id"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<div class="qty-product">
			<label for="qty_{$id}" class="qty-label">
				<xsl:choose>
					<xsl:when test="Prices/Price[@currency=/Page/Cart/@currency]/@qtyLabel!=''">
						<xsl:value-of select="Prices/Price[@currency=/Page/Cart/@currency]/@qtyLabel"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:call-template name="term3055" />
						<xsl:text>: </xsl:text>
					</xsl:otherwise>
				</xsl:choose>
				<xsl:text>&#160;</xsl:text>
			</label>
			<div class="input-group">
				<button class="btn qty-minus" type="button" value="-" onClick="incrementQuantity('qty_{@id}','-')">
					<i class="fa fa-minus">
						<xsl:text> </xsl:text>
					</i>
				</button>
				<input type="text" name="qty_{@id}" id="qty_{@id}" value="1" size="3" class="qtybox form-control"/>
				<button class="btn qty-plus" type="button" value="+" onClick="incrementQuantity('qty_{@id}','+')">
					<i class="fa fa-plus">
						<xsl:text> </xsl:text>
					</i>
				</button>
			</div>
		</div>
	</xsl:template>

	<!--#-->
	<!--############################## Order Process - 3DSecureReturn  ################################-->
	<!--#-->

	<xsl:template match="Page[Cart/Order[@cmd='Redirect3ds']]" mode="bodyBuilder">
		<body>
			<xsl:apply-templates select="." mode="bodyStyle"/>
			<div class="Site">
				<xsl:apply-templates select="Cart/Order" mode="orderProcess"/>
			</div>
			<xsl:apply-templates select="." mode="footerJs"/>
		</body>

	</xsl:template>

	<xsl:template match="Order[@cmd='Redirect3ds']" mode="orderProcessTitle">
		<h2>
			<!--Your Order - Please enter your payment details.-->
			Creating Invoice Please wait...
		</h2>
	</xsl:template>

	<xsl:template match="Order[@cmd='Redirect3ds']" mode="orderProcess">
		<xsl:apply-templates select="." mode="orderProcessTitle"/>
		<xsl:apply-templates select="." mode="orderErrorReports"/>
		<xsl:apply-templates select="." mode="orderEditAddresses"/>
		<xsl:apply-templates select="." mode="displayNotes"/>
		<xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='Secure3DReturn']" mode="xform"/>
	</xsl:template>

	<xsl:template match="Content[@name='Secure3D']" mode="xform">
		<form method="{model/submission/@method}" target="threeDS">
			<xsl:attribute name="class">
				<xsl:text>ewXform</xsl:text>
				<xsl:if test="model/submission/@class!=''">
					<xsl:text> </xsl:text>
					<xsl:value-of select="model/submission/@class"/>
				</xsl:if>
			</xsl:attribute>
			<xsl:attribute name="action">
				<xsl:value-of select="model/submission/@action" disable-output-escaping="yes"/>
			</xsl:attribute>
			<xsl:if test="model/submission/@id!=''">
				<xsl:attribute name="id">
					<xsl:value-of select="model/submission/@id"/>
				</xsl:attribute>
				<xsl:attribute name="name">
					<xsl:value-of select="model/submission/@id"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="model/submission/@event!=''">
				<xsl:attribute name="onsubmit">
					<xsl:value-of select="model/submission/@event"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="group | repeat | input | secret | select | select1 | range | textarea | upload | hint | help | alert " mode="xform2"/>
			<xsl:if test="count(submit) &gt; 0">
				<p class="buttons">
					<xsl:if test="descendant-or-self::*[contains(@class,'required')]">
						<span class="required">
							<span class="req">*</span>
							<xsl:call-template name="msg_required"/>
						</span>
					</xsl:if>
					<xsl:apply-templates select="submit" mode="xform"/>
				</p>
			</xsl:if>
		</form>
		<iframe name="threeDS" id="threeDS"></iframe>

	</xsl:template>

	<xsl:template match="Content[@name='Secure3D']" mode="contentJS">
		<script type="text/javascript">$(document).ready(function () {$('#Secure3D .buttons').hide();$('#Secure3D').submit();});</script>
	</xsl:template>

	<xsl:template match="Content[@name='Secure3DReturn']" mode="xform">
		<form method="{model/submission/@method}" action="" target="_top">
			<xsl:attribute name="class">
				<xsl:text>ewXform</xsl:text>
				<xsl:if test="model/submission/@class!=''">
					<xsl:text> </xsl:text>
					<xsl:value-of select="model/submission/@class"/>
				</xsl:if>
			</xsl:attribute>
			<xsl:if test="not(contains(model/submission/@action,'.asmx'))">
				<xsl:attribute name="action">
					<xsl:value-of select="model/submission/@action"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="model/submission/@id!=''">
				<xsl:attribute name="id">
					<xsl:value-of select="model/submission/@id"/>
				</xsl:attribute>
				<xsl:attribute name="name">
					<xsl:value-of select="model/submission/@id"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="model/submission/@event!=''">
				<xsl:attribute name="onsubmit">
					<xsl:value-of select="model/submission/@event"/>
				</xsl:attribute>
			</xsl:if>

			<xsl:if test="descendant::upload">
				<xsl:attribute name="enctype">multipart/form-data</xsl:attribute>
			</xsl:if>
			<div class="buttons">
				<xsl:apply-templates select="group | repeat | input | secret | select | select1 | range | textarea | upload | hint | help | alert " mode="xform2"/>

				<xsl:if test="count(submit) &gt; 0">
					<xsl:apply-templates select="submit" mode="xform"/>
				</xsl:if>
			</div>
		</form>
	</xsl:template>

	<xsl:template match="Content[@name='Secure3DReturn']" mode="contentJS">
		<script type="text/javascript">$(document).ready(function () {$('#Secure3DReturn .buttons').hide();$('#Secure3DReturn').submit();});</script>
	</xsl:template>

	<xsl:template match="Content[@name='Redirect3ds']" mode="xform">
		<form method="{model/submission/@method}" action="" target="_top">
			<xsl:attribute name="class">
				<xsl:text>ewXform</xsl:text>
				<xsl:if test="model/submission/@class!=''">
					<xsl:text> </xsl:text>
					<xsl:value-of select="model/submission/@class"/>
				</xsl:if>
			</xsl:attribute>
			<xsl:if test="not(contains(model/submission/@action,'.asmx'))">
				<xsl:attribute name="action">
					<xsl:value-of select="model/submission/@action"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="model/submission/@id!=''">
				<xsl:attribute name="id">
					<xsl:value-of select="model/submission/@id"/>
				</xsl:attribute>
				<xsl:attribute name="name">
					<xsl:value-of select="model/submission/@id"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="model/submission/@event!=''">
				<xsl:attribute name="onsubmit">
					<xsl:value-of select="model/submission/@event"/>
				</xsl:attribute>
			</xsl:if>

			<xsl:if test="descendant::upload">
				<xsl:attribute name="enctype">multipart/form-data</xsl:attribute>
			</xsl:if>

			<!--<xsl:copy-of select="/" />-->
			<!--xsl:apply-templates select="self::Content" mode="tinyMCEinit"/-->

			<xsl:apply-templates select="group | repeat | input | secret | select | select1 | range | textarea | upload | hint | help | alert " mode="xform2"/>

			<xsl:if test="count(submit) &gt; 0">
				<p class="buttons">
					<xsl:if test="descendant-or-self::*[contains(@class,'required')]">
						<span class="required">
							<span class="req">*</span>
							<xsl:call-template name="msg_required"/>
						</span>
					</xsl:if>
					<xsl:apply-templates select="submit" mode="xform"/>
				</p>
			</xsl:if>
		</form>
	</xsl:template>

	<xsl:template match="Content[@name='Redirect3ds']" mode="contentJS">
		<script type="text/javascript">$(document).ready(function () {$('#Secure3DReturn').submit();});</script>
	</xsl:template>


	<xsl:template match="group | repeat" mode="xform2">
		<xsl:param name="class"/>
		<fieldset>
			<xsl:if test="$class!='' or @class!='' ">
				<xsl:attribute name="class">
					<xsl:value-of select="$class"/>
					<xsl:if test="@class!=''">
						<xsl:text> </xsl:text>
						<xsl:value-of select="@class"/>
					</xsl:if>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="." mode="editXformMenu"/>
			<xsl:if test="label">
				<xsl:apply-templates select="label[position()=1]" mode="legend"/>
			</xsl:if>

			<!-- Qui? -->
			<!--<xsl:text> </xsl:text>-->

			<ul>
				<xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | group | repeat | hint | help | alert | repeat | relatedContent | label[position()!=1] | trigger" mode="xform"/>
				<xsl:if test="count(submit) &gt; 0">
					<li>
						<xsl:if test="descendant-or-self::*[contains(@class,'required')]">
							<label class="required">
								<span class="req">*</span>
								<xsl:call-template name="msg_required"/>
							</label>
						</xsl:if>
						<!-- For xFormQuiz change how these buttons work -->
						<xsl:apply-templates select="submit" mode="xform"/>
					</li>

				</xsl:if>
			</ul>
		</fieldset>
	</xsl:template>
	<!--#-->
	<!--############################## Error Reports ################################-->
	<!--#-->
	<xsl:template match="Order" mode="orderErrorReports">
		<xsl:if test="@errorMsg &gt; 0">
			<div class="alert alert-danger errorMessage">
				<xsl:choose>
					<!-- Error No. 1 - 99 : Standard errors from the Cart -->
					<xsl:when test="@errorMsg='1'">
						<!--<strong>The order has timed out and cannot continue</strong>, due to one of the following two reasons:<br/><br/>
              1. The order had been left for over ten minutes without any updates.  The details are automatically removed for security purposes.<br/><br/>
              2. You may have disabled cookies or they are undetectable.  The shopping cart requires cookies to be enabled in order to proceed.<br/><br/>
              Please ensure cookies are enabled in your browser to continue shopping, or call for assistance.  No transaction has been made.-->
						<xsl:call-template name="term3030" />
					</xsl:when>
					<xsl:when test="@errorMsg='2'">
						<!--The item(s) you are trying to add cannot be added to this shopping basket. <br/>
              <br/> Please proceed to the checkout and pay for the items in the basket, and then continue with your shopping.-->
						<xsl:call-template name="term3031" />
					</xsl:when>
					<xsl:when test="@errorMsg='3'">
						<!--There is no valid delivery option for this order.  This may be due to a combination of location, price, weight or quantity.<br/>
              <br/> Please call for assistance.-->
						<xsl:call-template name="term3032" />
					</xsl:when>
					<!-- Error No. 100+: Payment gateway errors-->
					<!-- Worldpay errors-->
					<xsl:when test="@errorMsg='1000'">
						<!--The transaction was cancelled during the payment processing - this was either at your request or the request of our payment provider, Worldpay.<br/>
              <br/> Please call for more information.-->
						<xsl:call-template name="term3033" />
					</xsl:when>
					<xsl:when test="@errorMsg='1001'">
						<!--The order reference could not be found, or the order did not have the correct status.  This may occur if you have tried to pay for the same order twice, or if there has been a long duration between visiting our payment provider, Worldpay's site and entering payment details.<br/>
              <br/> Please call for assistance.-->
						<xsl:call-template name="term3034" />
					</xsl:when>
					<xsl:when test="@errorMsg='1002'">
						<!--The payment provider, Worldpay, did not provide a valid response.<br/>
              <br/> Please call for assistance.-->
						<xsl:call-template name="term3035" />
					</xsl:when>
					<!-- Error No. 1000+ : Bespoke errors can be put here-->
				</xsl:choose>

				<a href="/" class="btn btn-custom">
					<xsl:call-template name="term3093" />
				</a>

			</div>
		</xsl:if>
		<xsl:if test="error/msg">
			<div class="alert alert-warning errorMessage">
				<i class="fa fa-exclamation-triangle pull-left fa-3x">
					<xsl:text> </xsl:text>
				</i>
				<xsl:text> </xsl:text>
				<xsl:for-each select="error/msg">
					<xsl:sort select="@type" order="ascending" data-type="text"/>
					<span class="err_sub_msg {@type}">
						<xsl:apply-templates select="." mode="cleanXhtml"/>
					</span>
				</xsl:for-each>
			</div>


		</xsl:if>
	</xsl:template>

	<!-- -->
	<xsl:template match="group[contains(@ref,'address') and group[contains(@class,'addressGrp')]]" mode="xform">
		<xsl:param name="class"/>
		<fieldset>

			<xsl:attribute name="class">
				<xsl:value-of select="$class"/>

				<xsl:if test="@class!=''">
					<xsl:text> </xsl:text>
					<xsl:value-of select="@class"/>
				</xsl:if>
				<xsl:if test="not(preceding-sibling::group)">
					<xsl:text> firstAdd</xsl:text>
				</xsl:if>
			</xsl:attribute>

			<xsl:apply-templates select="." mode="editXformMenu"/>
			<xsl:if test="label">
				<xsl:apply-templates select="label[position()=1]" mode="legend"/>
			</xsl:if>

			<div class="row">
				<xsl:choose>
					<xsl:when test="group[div/tblCartContact/cContactType/node()='Delivery Address']">
						<div class="card">
							<div class="card-body">
								<h3>Billing Address</h3>
								<xsl:apply-templates select="group[div/tblCartContact/cContactType/node()='Billing Address']" mode="xform"/>
							</div>
						</div>
						<xsl:if test="not($page/Cart/Order/@hideDeliveryAddress='True')">
							<div class="card">
								<div class="card-body">
									<h3>Delivery Addresses</h3>
									<xsl:apply-templates select="group[@class='collection-options']" mode="xform"/>
									<xsl:apply-templates select="group[div/tblCartContact/cContactType/node()!='Billing Address']" mode="xform"/>
									<div class="pull-right">
										<xsl:apply-templates select="submit" mode="xform"/>
									</div>
								</div>
							</div>
						</xsl:if>

					</xsl:when>
					<xsl:otherwise>
						<div class="col-md-12">
							<xsl:apply-templates select="group[div/tblCartContact/cContactType/node()='Billing Address']" mode="xform"/>
							<div class="pull-right">
								<xsl:apply-templates select="submit" mode="xform"/>
							</div>
						</div>
					</xsl:otherwise>
				</xsl:choose>
			</div>
		</fieldset>
	</xsl:template>

	<xsl:template match="group[contains(@class,'addressGrp')]" mode="xform">
		<xsl:param name="class"/>
		<fieldset>
			<xsl:if test="$class!='' or @class!='' ">
				<xsl:attribute name="class">
					<xsl:value-of select="$class"/>
					<xsl:if test="@class!=''">
						<xsl:text> </xsl:text>
						<xsl:value-of select="@class"/>
					</xsl:if>
					<xsl:text> well</xsl:text>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="." mode="editXformMenu"/>
			<xsl:if test="label">
				<xsl:apply-templates select="label[position()=1]" mode="legend"/>
			</xsl:if>
			<xsl:for-each select="input | secret | select | select1 | range | textarea | upload | group | repeat | hint | help | alert | div | repeat | relatedContent | label[position()!=1] | trigger | script">
				<xsl:choose>
					<xsl:when test="name()='group'">

						<xsl:if test="./@class">
							<xsl:attribute name="class">
								<xsl:text>li-</xsl:text>
								<xsl:value-of select="./@class"/>
							</xsl:attribute>
						</xsl:if>
						<xsl:apply-templates select="." mode="xform"/>

					</xsl:when>
					<xsl:otherwise>
						<xsl:apply-templates select="." mode="xform"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:for-each>
			<xsl:if test="count(submit) &gt; 0">

				<xsl:if test="ancestor::group/descendant-or-self::*[contains(@class,'required')]">
					<label class="required">
						<span class="req">*</span>
						<xsl:text> </xsl:text>
						<xsl:call-template name="msg_required"/>
					</label>
				</xsl:if>
				<!-- For xFormQuiz change how these buttons work -->
				<xsl:apply-templates select="submit" mode="xform"/>

			</xsl:if>

		</fieldset>
	</xsl:template>





	<xsl:template match="div[@class='pickAddress']" mode="xform">
		<div class="pickAddress">
			<div>
				<xsl:if test="tblCartContact/cContactName/node()!=''">
					<strong>
						<xsl:value-of select="tblCartContact/cContactName/node()"/>
					</strong>
					<br/>
				</xsl:if>
				<xsl:if test="tblCartContact/cContactCompany/node()!=''">
					<xsl:value-of select="tblCartContact/cContactCompany/node()"/>
					,
				</xsl:if>
				<xsl:if test="tblCartContact/cContactAddress/node()!=''">
					<xsl:value-of select="tblCartContact/cContactAddress/node()"/>
					,
				</xsl:if>
				<xsl:if test="tblCartContact/cContactTown/node()!=''">
					<xsl:value-of select="tblCartContact/cContactTown/node()"/>
					,
				</xsl:if>
				<xsl:if test="tblCartContact/cContactCity/node()!=''">
					<xsl:value-of select="tblCartContact/cContactCity/node()"/>
					,
				</xsl:if>
				<xsl:value-of select="tblCartContact/cContactState/node()"/>, <xsl:value-of select="tblCartContact/cContactZip/node()"/>
				,
				<xsl:value-of select="tblCartContact/cContactCountry/node()"/>
			</div>
			<div class="pickAddress">

				<xsl:if test="tblCartContact/cContactTel/node()!=''">
					Tel: <xsl:value-of select="tblCartContact/cContactTel/node()"/>
					<xsl:text> &#160;&#160;</xsl:text>
				</xsl:if>
				<xsl:if test="tblCartContact/cContactFax/node()!=''">
					Fax: <xsl:value-of select="tblCartContact/cContactFax/node()"/>
					<xsl:text> &#160;&#160;</xsl:text>
				</xsl:if>
				<xsl:if test="tblCartContact/cContactEmail/node()!=''">
					Email: <xsl:value-of select="tblCartContact/cContactEmail/node()"/>
				</xsl:if>
				<br/>
				<br/>
			</div>
		</div>
	</xsl:template>

	<!-- -->
	<xsl:template match="Content" mode="addtoCartButtons">
		<xsl:if test="/Page/Contents/Content[@type='giftlist' and @name='cart']">
			<button type="submit" name="glAdd" class="btn btn-action">
				<xsl:attribute name="value">
					<!--Add to Gift List-->
					<xsl:call-template name="term3056" />
				</xsl:attribute>
				<xsl:call-template name="term3058" />
			</button>
		</xsl:if>
		<xsl:if test="/Page/Cart[@type='quote']">
			<button type="submit" name="quoteAdd" class="btn btn-action">
				<xsl:attribute name="value">
					<!--Add to Quote-->
					<xsl:call-template name="term3057" />
				</xsl:attribute>
				<xsl:call-template name="term3058" />
			</button>
		</xsl:if>

		<button type="submit" name="cartAdd" class="btn btn-custom">
			<xsl:attribute name="value">
				<!--Add to Cart-->
				<xsl:call-template name="term3058" />
			</xsl:attribute>
			<xsl:call-template name="term3058" />
		</button>
	</xsl:template>

	<xsl:template match="Order[@cmd='ShowInvoice' or @cmd='ShowCallbackInvoice']" mode="orderProcessTitle">
		<h1>
			<!--Your Invoice - Thank you for your order.-->
			<xsl:call-template name="term3021" />
		</h1>
	</xsl:template>

	<xsl:template match="Order[@cmd='ShowInvoice' or @cmd='ShowCallbackInvoice']" mode="orderProcess">
		<xsl:variable name="parentURL">
			<xsl:call-template name="getContentParURL"/>
		</xsl:variable>
		<xsl:variable name="secureURL">
			<xsl:call-template name="getSecureURL"/>
		</xsl:variable>
		<xsl:variable name="siteURL">
			<xsl:call-template name="getSiteURL"/>
		</xsl:variable>
		<xsl:apply-templates select="." mode="orderProcessTitle"/>
		<xsl:apply-templates select="." mode="orderErrorReports"/>
		<!--script>

      // Break out of an iframe, if someone shoves your site
      // into one of those silly top-bar URL shortener things.

      (function(window) {
      if (window.location !== window.top.location) {
      window.top.location = window.location;
      }
      })(this);

    </script-->
		<!--SCRIPT LANGUAGE="javascript">if (top.location != location) top.location.href = location.href;</SCRIPT-->
		<div class="row">
			<div class="col-lg-8">

				<div class="confirmation-addresses">
					<xsl:apply-templates select="." mode="orderAddresses"/>
					<xsl:apply-templates select="." mode="displayNotes"/>
				</div>
			</div>
			<div class="col-lg-4">
				<div id="cartInvoice" class="card cart-receipt-card">
					<div class="card-body">
						<!--Invoice Date-->
						<div class="product-totals">
							<span class="amount-label">
								<xsl:call-template name="term3022" />:
							</span>
							<span class="amount">
								<xsl:value-of select="@InvoiceDate"/>
							</span>
						</div>
						<!--Invoice Reference-->
						<div class="product-totals">
							<span class="amount-label">
								<xsl:call-template name="term3023" />:
							</span>
							<span class="amount">
								<xsl:value-of select="@InvoiceRef"/>
							</span>
						</div>
						<div class="confirmation-cart ">
							<xsl:apply-templates select="." mode="orderItems">
								<xsl:with-param name="editQty">false</xsl:with-param>
								<xsl:with-param name="showImg">true</xsl:with-param>
							</xsl:apply-templates>
						</div>
						<xsl:if test="not(@payableType!='')">
							<xsl:apply-templates select="." mode="orderTotals"/>
						</xsl:if>

						<xsl:if test="@payableType='deposit' and (@payableAmount &gt; 0) ">
							<p>
								<!--Payment Received-->
								<xsl:call-template name="term3024" />
								<xsl:text>:&#160;</xsl:text>
								<xsl:value-of select="$currency"/>
								<xsl:value-of select="format-number(@payableAmount,'0.00')" />
							</p>
							<p>
								<!--Final Payment Reference-->
								<xsl:call-template name="term3025" />
								<xsl:text>:&#160;</xsl:text>
								<strong>
									<a href="{$secureURL}?cartCmd=Settlement&amp;settlementRef={@settlementID}">
										<xsl:value-of select="@settlementID" />
									</a>
								</strong>
							</p>

							<!--Thank you for your deposit. To pay the outstanding balance, please note your Final Payment Reference, above. Instructions on paying the outstanding balance have been e-mailed to you.
			  If you have any queries, please call for assistance.-->
							<xsl:call-template name="term3026" />
						</xsl:if>
						<xsl:if test="@payableType='settlement' or @payableAmount = 0 ">
							<p>
								<!--Payment Made-->
								<xsl:call-template name="term3027" />
								<xsl:text>:&#160;</xsl:text>
								<xsl:value-of select="$currency"/>
								<xsl:value-of select="format-number(@paymentMade,'0.00')" />
							</p>
							<p>
								<!--Total Payment Received-->
								<xsl:call-template name="term3028" />
								<xsl:text>:&#160;</xsl:text>
								<xsl:value-of select="$currency"/>
								<xsl:value-of select="format-number(@total, '0.00')"/>
								<xsl:text>&#160;(</xsl:text>
								<!--paid in full-->
								<xsl:call-template name="term3029" />
								<xsl:text>)</xsl:text>
							</p>
						</xsl:if>
					</div>
				</div>
			</div>
		</div>
		<div class="clearfix mb-1 optionButtons">
			<a href="{$secureURL}?pgid={/Page/@id}&amp;cartCmd=Quit" class="btn btn-outline-primary" target="_parent" title="Click here to close this invoice and return to the site.">

				<xsl:text> </xsl:text>
				<xsl:call-template name="term3078" />
			</a>
		</div>
	</xsl:template>

	<xsl:template match="Order" mode="orderItems">
		<xsl:param name="editQty"/>
		<xsl:param name="showImg"/>
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
		<xsl:if test="count(Item)!=0">
			<div id="cartListing" class="responsive-cart">
				<xsl:for-each select="Item">
					<div class="clearfix cart-item">
						<xsl:apply-templates select="." mode="orderItem">
							<xsl:with-param name="editQty" select="$editQty"/>
							<xsl:with-param name="showImg" select="$showImg"/>
						</xsl:apply-templates>
					</div>
				</xsl:for-each>
			</div>
		</xsl:if>
	</xsl:template>

	<!-- Duplicated Template brings in Content Node - generic for all purchasable content types
        Should be used from now on 
  -->
	<!-- -->
	<xsl:template match="Content" mode="Options_List">
		<xsl:choose>
			<xsl:when test="Content[@type='SKU']">
				<!--and @SkuOptions='skus'-->
				<div class="selectOptions select1-group">
					<select class="skuOptions form-select">
						<!--<xsl:if test="count(Content[@type='SKU']) &gt; 1">
              <option value="">Please select option</option>
            </xsl:if>-->
						<xsl:choose>
							<xsl:when test="$page/ContentDetail">
								<xsl:apply-templates select="Content[@type='SKU']" mode="skuOptions"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:apply-templates select="Content[@type='SKU']" mode="skuOptions"/>
							</xsl:otherwise>
						</xsl:choose>
					</select>
				</div>
			</xsl:when>
			<xsl:otherwise>
				<xsl:for-each select="Options/OptGroup">
					<xsl:if test="@name!=''">
						<div class="selectOptions">
							<xsl:if test="@selectType!='hidden'">
								<label>
									<xsl:value-of select="@name"/>
								</label>
							</xsl:if>
							<xsl:choose>
								<xsl:when test="@selectType='Radio'">
									<xsl:apply-templates select="option" mode="List_Options_Radio">
										<xsl:with-param name="grpIdx" select="position()"/>
									</xsl:apply-templates>
								</xsl:when>
								<xsl:when test="@selectType='CheckBoxes'">
									<xsl:apply-templates select="option" mode="List_Options_CheckBoxes">
										<xsl:with-param name="grpIdx" select="position()"/>
									</xsl:apply-templates>
								</xsl:when>
								<xsl:when test="@selectType='ReadOnly'">
									<xsl:apply-templates select="option" mode="List_Options_ReadOnly">
										<xsl:with-param name="grpIdx" select="position()"/>
									</xsl:apply-templates>
								</xsl:when>
								<xsl:when test="@selectType='TextInput'">
									<xsl:apply-templates select="option" mode="List_Options_TextInput">
										<xsl:with-param name="grpIdx" select="position()"/>
									</xsl:apply-templates>
								</xsl:when>
								<!-- much like textInput but a hidden input -->
								<xsl:when test="@selectType='hidden'">
									<xsl:apply-templates select="option" mode="List_Options_Hidden">
										<xsl:with-param name="grpIdx" select="position()"/>
									</xsl:apply-templates>
								</xsl:when>
								<xsl:otherwise>
									<select size="1" name="opt_{ancestor::Content/@id}_{position()}" class="form-control">
										<xsl:apply-templates select="option" mode="List_Options_Dropdown">
											<xsl:with-param name="grpIdx" select="position()"/>
										</xsl:apply-templates>
									</select>
								</xsl:otherwise>
							</xsl:choose>
						</div>
					</xsl:if>
				</xsl:for-each>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
	<!-- -->
	<!-- SKU node -->
	<xsl:template match="Content" mode="skuOptions">
		<option>
			<xsl:attribute name="value">
				<xsl:value-of select="@id"/>
				<xsl:text>_</xsl:text>
				<xsl:choose>
					<xsl:when test="Prices/Price[@currency = $page/Cart/@currency and @type = 'rrp']!=''">
						<xsl:value-of select="format-number(Prices/Price[@currency = $page/Cart/@currency and @type = 'rrp'],'###,###,##0.00')"/>
					</xsl:when>
					<xsl:otherwise>na</xsl:otherwise>
				</xsl:choose>
				<xsl:text>_</xsl:text>
				<xsl:value-of select="format-number(Prices/Price[@currency = $page/Cart/@currency and @type = 'sale'],'###,###,##0.00')"/>
				<xsl:text>_</xsl:text>
				<xsl:value-of select="parent::Content/@id "/>
			</xsl:attribute>
			<xsl:value-of select="Name"/>
		</option>
	</xsl:template>
	<!-- -->
	<xsl:template match="option" mode="List_Options_Radio">
		<xsl:param name="grpIdx"/>
		<xsl:if test="@name!=''">
			<span class="radiocheckbox">
				<xsl:choose>
					<xsl:when test="position()=1">
						<input type="radio" name="opt_{ancestor::Content/@id}_{$grpIdx}" value="{$grpIdx}_{position()}" id="opt_{ancestor::Content/@id}_{$grpIdx}_{position()}" checked="checked"/>
					</xsl:when>
					<xsl:otherwise>
						<input type="radio" name="opt_{ancestor::Content/@id}_{$grpIdx}" value="{$grpIdx}_{position()}" id="opt_{ancestor::Content/@id}_{$grpIdx}_{position()}" />
					</xsl:otherwise>
				</xsl:choose>
				<label  for="opt_{ancestor::Content/@id}_{$grpIdx}_{position()}">
					<xsl:value-of select="@name"/>
				</label>
			</span>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template match="option" mode="List_Options_CheckBoxes">
		<xsl:param name="grpIdx"/>
		<xsl:if test="@name!=''">
			<span class="radiocheckbox">
				<input type="checkbox" name="opt_{ancestor::Content/@id}_{$grpIdx}" value="{$grpIdx}_{position()}" id="opt_{ancestor::Content/@id}_{$grpIdx}_{position()}" />
				<label  for="opt_{ancestor::Content/@id}_{$grpIdx}_{position()}">
					<xsl:value-of select="@name"/>
					<xsl:if test="Prices/Price/node() and Prices/Price/node()&gt;0">
						<xsl:text> (+</xsl:text>
						<xsl:apply-templates select="/Page" mode="formatPrice">
							<xsl:with-param name="price" select="Prices/Price/node()"/>
							<xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
						</xsl:apply-templates>
						<xsl:text> </xsl:text>
						<xsl:value-of select="Prices/Price/@suffix" />
						<xsl:text>)</xsl:text>

					</xsl:if>
				</label>
			</span>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template match="option" mode="List_Options_Dropdown">
		<xsl:param name="grpIdx"/>
		<xsl:if test="@name!=''">
			<option value="{$grpIdx}_{position()}">
				<xsl:if test="position()=1">
					<xsl:attribute name="selected">selected</xsl:attribute>
				</xsl:if>
				<xsl:value-of select="@name"/>
			</option>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template match="option" mode="List_Options_ReadOnly">
		<xsl:param name="grpIdx"/>
		<input type="hidden" name="opt_{ancestor::Content/@id}_{$grpIdx}" value="{$grpIdx}_{position()}" id="opt_{ancestor::Content/@id}_{$grpIdx}_{position()}"/>
		<label  for="opt_{ancestor::Content/@id}_{$grpIdx}_{position()}">
			<xsl:value-of select="@name"/>
		</label>
	</xsl:template>
	<!-- -->
	<xsl:template match="option" mode="List_Options_Hidden">
		<xsl:param name="grpIdx"/>
		<xsl:if test="@name!=''">
			<input type="hidden" name="opt_{ancestor::Content/@id}_{$grpIdx}" value="{$grpIdx}_{position()}" id="opt_{ancestor::Content/@id}_{$grpIdx}_{position()}"/>
		</xsl:if>
	</xsl:template>
	<!-- -->
	<xsl:template match="option" mode="List_Options_TextInput">
		<xsl:param name="grpIdx"/>
		<input type="text" name="opt_{ancestor::Content[1]/@id}_{$grpIdx}" value="" id="opt_{ancestor::Content[1]/@id}_{$grpIdx}_{position()}"/>
	</xsl:template>
	<!-- -->
	<xsl:template match="option" mode="List_Options_Hidden">
		<xsl:param name="grpIdx"/>
		<input type="hidden" name="opt_{ancestor::Content[1]/@id}_{$grpIdx}" value="" id="opt_{ancestor::Content[1]/@id}_{$grpIdx}_{position()}"/>
	</xsl:template>


	<xsl:template match="Content[model/submission/@id='optionsForm']" mode="xform">
		<form method="{model/submission/@method}" action="" novalidate="novalidate">
			<xsl:attribute name="class">
				<xsl:text>xform needs-validation</xsl:text>
				<xsl:if test="model/submission/@class!=''">
					<xsl:text> </xsl:text>
					<xsl:value-of select="model/submission/@class"/>
				</xsl:if>
			</xsl:attribute>
			<xsl:if test="not(contains(model/submission/@action,'.asmx'))">
				<xsl:attribute name="action">
					<xsl:value-of select="model/submission/@action"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="model/submission/@id!=''">
				<xsl:attribute name="id">
					<xsl:value-of select="model/submission/@id"/>
				</xsl:attribute>
				<xsl:attribute name="name">
					<xsl:value-of select="model/submission/@id"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="model/submission/@event!=''">
				<xsl:attribute name="onsubmit">
					<xsl:value-of select="model/submission/@event"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="descendant::upload">
				<xsl:attribute name="enctype">multipart/form-data</xsl:attribute>
			</xsl:if>

			<xsl:apply-templates select="group | repeat | input | secret | select | select1 | range | textarea | upload | hint | help | alert | div" mode="xform"/>

			<xsl:if test="count(submit) &gt; 0">
				<p class="buttons">
					<xsl:if test="descendant-or-self::*[contains(@class,'required')]">
						<span class="required">
							<span class="req">*</span>
							<xsl:text> </xsl:text>
							<xsl:call-template name="msg_required"/>
						</span>
					</xsl:if>
					<xsl:apply-templates select="submit" mode="xform"/>

				</p>
			</xsl:if>
			<button type="submit" name="submit" disabled="diabled" class="btn btn-custom dummy-pay-button" style="">
				<i class="fa fa-white">&#160;</i> Complete Order
			</button>
		</form>
	</xsl:template>


</xsl:stylesheet>



<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">


    <!-- -->
    <xsl:template match="Order" mode="orderItemsEmail">
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
        <xsl:variable name="currency" select="'£'"/>
        <xsl:if test="$editQty='true' and Item">
            <div id="cartLegend">
                <p class="delete">
                    <img src="/ewCommon/images/icons/trash.gif" alt="delete icon" class="delete icon"/>
                    <span class="hidden">&#160;&#160;</span>Click on this icon to remove an item from the order.<br/>If you amend the quantity of items please click 'Update Order' before continuing to browse.
                </p>
            </div>
        </xsl:if>
        <table cellspacing="0" width="100%" id="cartListing" summary="This table contains a list of the items which you have added to your order. To change the quantity of an item, replace the number under the Qty column and click on Update Order.">
            <tr>
                <th class="heading delete">&#160;</th>
                <th class="heading quantity">
                    <font face="verdana" size="2" align="center">Qty</font>
                </th>
                <th class="heading description">
                    <font face="verdana" size="2" align="left">Description</font>
                </th>
                <th class="heading ref" align="left">
                    <font face="verdana" size="2">Ref</font>
                </th>
                <th class="heading price" align="right">
                    <font face="verdana" size="2">Price</font>
                </th>
                <th class="heading lineTotal" align="right">
                    <font face="verdana" size="2">Line Total</font>
                </th>
            </tr>
            <xsl:for-each select="Item">
                <xsl:apply-templates select="." mode="orderItem">
                    <xsl:with-param name="editQty" select="$editQty"/>
                </xsl:apply-templates>
            </xsl:for-each>
            <xsl:if test="@shippingCost &gt; 0">
                <tr>
                    <td colspan="3">
                      <xsl:value-of select="@shippingDesc"/>
                    </td>
                    <td class="shipping heading" align="right">
                        <font face="verdana" size="2">
                            <xsl:choose>
                                <xsl:when test="/Page/Contents/Content[@name='shippingCostLabel']!=''">
                                    <xsl:value-of select="/Page/Contents/Content[@name='shippingCostLabel']"/>
                                </xsl:when>
                                <xsl:otherwise>Shipping Cost:</xsl:otherwise>
                            </xsl:choose>
                        </font>
                    </td>
                    <td class="shipping amount" align="right">
                        <font face="verdana" size="2">
                            <xsl:value-of select="$currency"/>
                            <xsl:value-of select="format-number(@shippingCost,'0.00')"/>
                        </font>
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
                    <td class="subTotal heading" align="right">
                        <font face="verdana" size="2">
                            Sub Total:
                        </font>
                    </td>
                    <td class="subTotal amount" align="right">
                        <font face="verdana" size="2">
                            <xsl:value-of select="$currency"/>
                            <xsl:value-of select="format-number(@totalNet, '0.00')"/>
                        </font>
                    </td>
                </tr>

                <tr>
                    <td colspan="4">&#160;</td>
                    <td class="vat heading" align="right">
                        <font face="verdana" size="2">
                            <xsl:choose>
                                <xsl:when test="//Cart/Contact/Address/Country='United Kingdom'">VAT at </xsl:when>
                                <xsl:otherwise>Tax at </xsl:otherwise>
                            </xsl:choose>
                            <xsl:value-of select="format-number(@vatRate, '#.00')"/>%:
                        </font>
                    </td>
                    <td class="vat amount" align="right">
                        <font face="verdana" size="2">
                            <span class="currency">
                                <xsl:value-of select="$currency"/>
                            </span>
                            <xsl:value-of select="format-number(@vatAmt, '0.00')"/>
                        </font>
                    </td>
                </tr>
            </xsl:if>
            <tr>
                <td colspan="4">&#160;</td>
                <td class="total heading" align="right">
                    <font face="verdana" size="2">Total Value:</font>
                </td>
                <td class="total amount" align="right">
                    <font face="verdana" size="2">
                        <xsl:value-of select="$currency"/>
                        <xsl:value-of select="format-number(@total, '0.00')"/>
                    </font>
                </td>
            </tr>
            <xsl:if test="@paymentMade &gt; 0">
                <tr>
                    <td colspan="5">&#160;</td>
                    <td class="total heading" align="right">
                        <font face="verdana" size="2">
                            <xsl:choose>
                                <xsl:when test="@transStatus">Transaction Made</xsl:when>
                                <xsl:when test="@payableType='settlement' and not(@transStatus)">Payment Received</xsl:when>
                            </xsl:choose>
                        </font>
                    </td>
                    <td class="total amount" align="right">
                        <font face="verdana" size="2" align="right">
                            <xsl:value-of select="$currency"/>
                            <xsl:value-of select="format-number(@paymentMade, '0.00')"/>
                        </font>
                    </td>
                </tr>
            </xsl:if>
            <xsl:if test="@payableAmount &gt; 0">
                <tr>
                    <td colspan="4">&#160;</td>
                    <td class="total heading" align="right">
                        <font face="verdana" size="2">
                            <xsl:choose>
                                <xsl:when test="@payableType='deposit' and not(@transStatus)">Deposit Payable</xsl:when>
                                <xsl:when test="@payableType='settlement' or (@payableType='deposit' and @transStatus)">Amount Outstanding</xsl:when>
                            </xsl:choose>
                        </font>
                    </td>
                    <td class="total amount" align="right">
                        <font face="verdana" size="2">
                            <xsl:value-of select="$currency"/>
                            <xsl:value-of select="format-number(@payableAmount, '0.00')"/>
                        </font>
                    </td>
                </tr>
            </xsl:if>
        </table>
        <xsl:if test="/Page/Contents/Content[@name='cartMessage']">
            <div class="cartMessage">
                <xsl:copy-of select="/Page/Contents/Content[@name='cartMessage']/node()"/>
            </div>
        </xsl:if>
    </xsl:template>
    <!-- -->
    <!--#-->
    <xsl:template match="Item" mode="orderItem">
        <xsl:param name="editQty"/>
        <xsl:variable name="parentURL">
            <xsl:call-template name="getContentParURL"/>
        </xsl:variable>
        <xsl:variable name="secureURL">
            <xsl:call-template name="getSecureURL"/>
        </xsl:variable>
        <xsl:variable name="currency" select="'£'"/>
        <tr class="orderItem">
            <td class="cell delete">
                <xsl:choose>
                    <xsl:when test="$editQty!='true'">&#160;</xsl:when>
                    <xsl:otherwise>
                        <a href="{$parentURL}?cartCmd=Remove&amp;id={@id}" title="click here to remove this item from the list">
                            <!--BJR - This either doesnt work or is wrong so i have changed it for the moment to work-->
                            <!--<img src="{$secureURL}/ewCommon/images/icons/trash.gif" alt="delete icon - click here to remove this item from the list"/>-->
                            <img src="/ewCommon/images/icons/trash.gif" alt="delete icon - click here to remove this item from the list"/>
                        </a>
                    </xsl:otherwise>
                </xsl:choose>
            </td>
            <td class="cell quantity" align="right">
                <font face="verdana" size="2">
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
                </font>
            </td>
            <td class="cell description">
                <font face="verdana" size="2">
                    <!--<a href="{$siteURL}/item{@id}/" title="">-->
                        <xsl:value-of select="node()"/>
                    <!--</a>-->
                    <!-- ################################# Line Options Info ################################# -->
                    <xsl:for-each select="Item">
                        <br/>
                        <span class="optionList">
                            <xsl:apply-templates select="option" mode="optionDetail"/>
                            <xsl:if test="@price!=0">
                                <strong>
                                    &#160;
                                    <xsl:value-of select="$currency"/>
                                    <xsl:value-of select="format-number(@price,'#.00')"/>
                                </strong>
                            </xsl:if>
                            <xsl:text>,</xsl:text>
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
                </font>
            </td>
            <td class="cell ref">
                <font face="verdana" size="2">
                    <xsl:value-of select="@ref"/>
                    <!--xsl:for-each select="Item">
          <xsl:apply-templates select="option" mode="optionCodeConcat"/>
        </xsl:for-each-->
                </font>
            </td>
            <xsl:if test="not(/Page/Cart/@displayPrice='false')">
                <td class="cell price" align="right">
                    <font face="verdana" size="2">
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
                    </font>
                </td>
                <td class="cell lineTotal" align="right">
                    <font face="verdana" size="2">
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
                    </font>
                </td>
            </xsl:if>
        </tr>
    </xsl:template>
    <!-- -->
    <!-- -->
    <xsl:template match="Contact" mode="cartEmail">
        <xsl:variable name="secureURL">
            <xsl:call-template name="getSecureURL"/>
        </xsl:variable>
        <table cellpadding="10" cellspacing="0" border="0">
            <tr>
                <td>
                    <font face="verdana">
                        <h5>
                            <xsl:value-of select="@type"/> Details
                        </h5>
                        <font size="2">
                            <xsl:value-of select="GivenName"/><br/>
                            <xsl:if test="Company/node()!=''">
                                <xsl:value-of select="Company"/>
                                <br/>
                            </xsl:if>
                            <xsl:value-of select="Street"/>
                            <br/>
                            <xsl:value-of select="City"/>
                            <br/>
                            <xsl:if test="State/node()!=''">
                                <xsl:value-of select="State"/>
                                <br/>
                            </xsl:if>
                            <xsl:value-of select="PostalCode"/>
                            <br/>
                            <xsl:if test="Country/node()!=''">
                                <xsl:value-of select="Country"/>
                                <br/>
                            </xsl:if>
                            Tel: <xsl:value-of select="Telephone"/>
                            <br/>
                            <xsl:if test="Fax/node()!=''">
                                Fax: <xsl:value-of select="Fax"/>
                                <br/>
                            </xsl:if>
                            Email: <xsl:value-of select="Email"/>
                            <br/>
                        </font>
                    </font>
                </td>
            </tr>
        </table>

    </xsl:template>
    <!-- -->
</xsl:stylesheet>

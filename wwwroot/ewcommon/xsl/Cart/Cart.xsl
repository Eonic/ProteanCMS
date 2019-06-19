<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" >
  
  <!--   ################################################   Cart Brief  ##############################################   -->
  <!-- -->
  <xsl:template match="Order" mode="cartBrief">
    <xsl:variable name="parentURL">
      <xsl:call-template name="getContentParURL"/>
    </xsl:variable>
    <xsl:variable name="secureURL">
      <xsl:call-template name="getSecureURL"/>
    </xsl:variable>
    <div id="cartBrief">
      <strong>
        <!--Your Shopping Cart-->
        <xsl:call-template name="term3001" />
      </strong>
      <p>
        <xsl:value-of select="@itemCount"/>
        <xsl:text>&#160;</xsl:text>
        <!--Items-->
        <xsl:call-template name="term3002" />
        <xsl:text>&#160;-&#160;</xsl:text>
        <xsl:value-of select="$currency"/>
        <xsl:value-of select="format-number(@total, '#.00')"/>
      </p>
      <xsl:if test="@cmd='' and @status!='Empty'">
        <p>
          <a href="{$secureURL}?cartCmd=Cart" class="morelink">
            <xsl:attribute name="title">
              <!--Click here to view full details of your shopping cart-->
              <xsl:call-template name="term3003" />
            </xsl:attribute>
            <!--Show Details-->
            <xsl:call-template name="term3004" />
          </a>
        </p>
      </xsl:if>
    </div>
  </xsl:template>
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
      <xsl:apply-templates select="." mode="orderProgressLegend"/>
      <xsl:apply-templates select="." mode="orderAlert"/>
      <xsl:apply-templates select="." mode="orderProcess"/>
      <div class="terminus">&#160;</div>
    </div>
  </xsl:template>

  <xsl:template match="Order" mode="orderProgressLegend">

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
    <h2>
      <!--Your Order - Generic - Cmd:-->
      <xsl:call-template name="term3005a" />
    </h2>
  </xsl:template>

  <xsl:template match="Order" mode="orderAlert">

  </xsl:template>

  <xsl:template match="Order[DiscountMessage]" mode="orderAlert">
    <span class="alert">
      <xsl:apply-templates select="DiscountMessage" mode="cleanXhtml"/>
    </span>
  </xsl:template>

  <xsl:template match="Order" mode="orderProcess">
    <xsl:apply-templates select="." mode="orderProcessTitle"/>
    <xsl:apply-templates select="." mode="orderErrorReports"/>
    <xsl:apply-templates select="." mode="orderAddresses"/>
    <xsl:apply-templates select="." mode="orderItems"/>
  </xsl:template>
  <!--#-->
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
      <input type="submit" name="{$bCmd}" value="{$bTitle}" class="btn btn-success button principle {$buttonClass}"/>
    </xsl:if>
  </xsl:template>
  <!--############################## Order Procees - Add / Quote ################################-->
  <!--#-->
  <xsl:template match="Order[@cmd='Add' or @cmd='Cart' or @cmd='Confirm']" mode="orderProcessTitle">
    <h2>
      <!--Your Order-->
      <xsl:call-template name="term3007" />
    </h2>
  </xsl:template>

  <xsl:template match="Order[@cmd='Add' or @cmd='Cart' or @cmd='Confirm']" mode="orderProcess">
    <xsl:apply-templates select="." mode="orderProcessTitle"/>
    <xsl:apply-templates select="." mode="orderErrorReports"/>
    <xsl:apply-templates select="." mode="orderAddresses"/>
    <div class="terminus">&#160;</div>
    <form method="post" id="cart" class="ewXform">
      <xsl:if test="/Page/Contents/Content[@type='MetaData' and @name='MetaGoogleAnalyticsID']">
        <xsl:attribute name="action">
          <xsl:apply-templates select="$currentPage" mode="getHref"/>
        </xsl:attribute>
        <xsl:attribute name="onsubmit">pageTracker._linkByPost(this)</xsl:attribute>
      </xsl:if>
      <xsl:apply-templates select="." mode="principleButton"/>
      <input type="submit" name="cartBrief" value="Continue Shopping" class="button continue"/>
      <input type="submit" name="cartUpdate" value="Update Order" class="button update"/>
      <input type="submit" name="cartQuit" value="Empty Order" class="button empty"/>
      <xsl:apply-templates select="." mode="orderItems">
        <xsl:with-param name="editQty">true</xsl:with-param>
      </xsl:apply-templates>
      <xsl:apply-templates select="." mode="principleButton"/>
      <input type="submit" name="cartBrief" value="Continue Shopping" class="button continue"/>
      <input type="submit" name="cartUpdate" value="Update Order" class="button update"/>
      <input type="submit" name="cartQuit" value="Empty Order" class="button empty"/>
      <div class="terminus">&#160;</div>
    </form>
  </xsl:template>

  <!--#-->
  <!--############################## Order Procees - Notes ################################-->
  <!--#-->
  <xsl:template match="Order[@cmd='Discounts']" mode="orderProcessTitle">
    <h2>
      <!--Your Order - Please enter a discount code-->
      <xsl:call-template name="term3008" />
    </h2>
  </xsl:template>
  <xsl:template match="Order[@cmd='Discounts']" mode="orderProcess">
    <xsl:apply-templates select="." mode="orderProcessTitle"/>
    <xsl:apply-templates select="." mode="orderErrorReports"/>
    <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='discountsForm']" mode="xform"/>
    <form method="post" id="cart">
      <!--<div class="cartButtons">-->
        <!--input type="submit" name="quoteLogon" value="Proceed" class="button principle"/-->
        <input type="submit" name="cartBrief" value="Continue Shopping" class="button continue"/>
        <input type="submit" name="cartUpdate" value="Update Order" class="button update"/>
        <input type="submit" name="cartQuit" value="Empty Order" class="button empty"/>
      <!--</div>-->
      <xsl:apply-templates select="." mode="orderItems">
        <xsl:with-param name="editQty">true</xsl:with-param>
      </xsl:apply-templates>
      <!--<div class="cartButtons">-->
        <!--input type="submit" name="quoteLogon" value="Proceed" class="button principle"/-->
        <input type="submit" name="cartBrief" value="Continue Shopping" class="button continue"/>
        <input type="submit" name="cartUpdate" value="Update Order" class="button update"/>
        <input type="submit" name="cartQuit" value="Empty Order" class="button empty"/>
      <!--</div>-->
      <div class="terminus">&#160;</div>
    </form>
  </xsl:template>
  <!--#-->
  <!--############################## Order Procees - Notes ################################-->
  <!--#-->
  <xsl:template match="Order[@cmd='Notes']" mode="orderProcessTitle">
    <h2>
      <!--Your Order - Please tell us any special requirements-->
      <xsl:call-template name="term3009" />
    </h2>
  </xsl:template>
  <xsl:template match="Order[@cmd='Notes']" mode="orderProcess">
    <xsl:apply-templates select="." mode="orderProcessTitle"/>
    <xsl:apply-templates select="." mode="orderErrorReports"/>
    <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='notesForm']" mode="xform"/>
    <form method="post" id="cart">
      <!--<div class="cartButtons">-->
        <!--input type="submit" name="quoteLogon" value="Proceed" class="button principle"/-->
        <input type="submit" name="cartBrief" value="Continue Shopping" class="button continue"/>
        <input type="submit" name="cartUpdate" value="Update Order" class="button update"/>
        <input type="submit" name="cartQuit" value="Empty Order" class="button empty"/>
      <!--</div>-->
      <xsl:apply-templates select="." mode="orderItems">
        <xsl:with-param name="editQty">true</xsl:with-param>
      </xsl:apply-templates>
      <!--<div class="cartButtons">-->
        <!--input type="submit" name="quoteLogon" value="Proceed" class="button principle"/-->
        <input type="submit" name="cartBrief" value="Continue Shopping" class="button continue"/>
        <input type="submit" name="cartUpdate" value="Update Order" class="button update"/>
        <input type="submit" name="cartQuit" value="Empty Order" class="button empty"/>
      <!--</div>-->
      <div class="terminus">&#160;</div>
    </form>
  </xsl:template>

  <!-- -->
  <xsl:template match="Order" mode="displayNotes">
    <xsl:variable name="parentURL">
      <xsl:call-template name="getContentParURL"/>
    </xsl:variable>
    <xsl:if test="Notes/Notes/node()!='' or Notes/PromotionalCode/node()!=''">
      <xsl:if test="Notes/Notes/node()!=''">
        <h3>
          <!--Additional information for Your Order-->
          <xsl:call-template name="term3010" />
        </h3>
        <xsl:for-each select="Notes/Notes/*">
          <xsl:if test="node()!=''">
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
          </xsl:if>
        </xsl:for-each>
      </xsl:if>
      <xsl:if test="Notes/PromotionalCode/node()!=''">
        <p>
          <!--Promotional Code entered-->
          <xsl:call-template name="term3011" />
          <xsl:text>:&#160;</xsl:text>
          <xsl:apply-templates select="Notes/PromotionalCode/node()" mode="cleanXhtml"/>
        </p>
      </xsl:if>
      <xsl:if test="not(@readonly) and Notes/Notes/node()!=''">
        <p class="optionButtons">
          <a href="{$parentURL}?cartCmd=Notes" class="button">
            <xsl:attribute name="title">
              <!--Click here to edit the notes on this order.-->
              <xsl:call-template name="term3012" />
            </xsl:attribute>
            <!--Edit Notes-->
            <xsl:call-template name="term3013" />
          </a>
        </p>
      </xsl:if>
    </xsl:if>
  </xsl:template>

  <!--#-->
  <!--############################## Quote Procees - Logon ################################-->
  <!--#-->
  <xsl:template match="Order[@cmd='Logon']" mode="orderProcessTitle">
    <h2>
      <!--Registration-->
      <xsl:call-template name="term3014" />
    </h2>
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
    <xsl:if test="$anySub=''">
      <div id="cartInvoice">
        <a href="?pgid={/Page/@id}&amp;cartCmd=Notes" class="btn btn-success button principle">
          <!--Proceed Without Registering / Logging In-->
          <xsl:call-template name="term3015" />
        </a>
      </div>
    </xsl:if>
    <div id="template_2_Columns" class="template template_2_Columns">
      <div id="column1" class="column1">
        <div id="cartLogonBox" class="box Default-Box cartBox">
          <div class="tl">
            <div class="tr">
              <h2 class="title">Login</h2>
            </div>
          </div>
          <div class="content">
            <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='UserLogon']" mode="xform"/>
          </div>
          <div class="bl">
            <div class="br">&#160;</div>
          </div>
        </div>
      </div>
      <div id="column2" class="column2">
        <div id="cartRegisterBox" class="box Default-Box">
          <div class="tl">
            <div class="tr">
              <h2 class="title">Register</h2>
            </div>
          </div>
          <div class="content">
            <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='CartRegistration']" mode="xform"/>
          </div>
          <div class="bl">
            <div class="br">&#160;</div>
          </div>
          </div>
      </div>
      <xsl:if test="/Page/Cart/Order/Notes/PromotionalCode!=''">
      <xsl:apply-templates select="." mode="principleButton"/>
      <input type="submit" name="cartBrief" value="Continue Shopping" class="button continue"/>
      <input type="submit" name="cartUpdate" value="Update Order" class="button update"/>
      <input type="submit" name="cartQuit" value="Empty Order" class="button empty"/>
      <xsl:apply-templates select="." mode="orderItems">
        <xsl:with-param name="editQty">true</xsl:with-param>
      </xsl:apply-templates>
      </xsl:if>
    </div>
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
  <!--############################## Order Procees - Billing ################################-->
  <!--#-->
  <xsl:template match="Order[@cmd='Billing']" mode="orderProcessTitle">
    <h2>
      <!--Your Order - Enter your contact details-->
      <xsl:call-template name="term3017" />
    </h2>
  </xsl:template>

  <xsl:template match="Order[@cmd='Billing']" mode="orderProcess">
    <xsl:apply-templates select="." mode="orderProcessTitle"/>
    <xsl:apply-templates select="." mode="orderErrorReports"/>
    <xsl:apply-templates select="." mode="orderEditAddresses"/>
    <xsl:apply-templates select="." mode="displayNotes"/>
  </xsl:template>
  <!--#-->
  <!--############################## Quote Procees - Delivery ################################-->
  <!--#-->
  <xsl:template match="Order[@cmd='Delivery']" mode="orderProcessTitle">
    <h2>
      <!--Your Order - Enter the delivery address-->
      <xsl:call-template name="term3018" />
    </h2>
  </xsl:template>

  <xsl:template match="Order[@cmd='Delivery']" mode="orderProcess">
    <xsl:apply-templates select="." mode="orderProcessTitle"/>
    <xsl:apply-templates select="." mode="orderErrorReports"/>
    <xsl:apply-templates select="." mode="orderEditAddresses"/>
    <xsl:apply-templates select="." mode="displayNotes"/>
  </xsl:template>

  <!--#-->
  <!--############################## Order Process - Choose Payment / Shipping Options ################################-->
  <!--#-->
  <xsl:template match="Order[@cmd='ChoosePaymentShippingOption']" mode="orderProcessTitle">
    <h2>
      <!--Your Order - Please enter your preferred payment and shipping methods.-->
      <xsl:call-template name="term3019" />
    </h2>
  </xsl:template>

  <xsl:template match="Order[@cmd='ChoosePaymentShippingOption']" mode="orderProcess">
    <xsl:apply-templates select="." mode="orderProcessTitle"/>
    <xsl:apply-templates select="." mode="orderErrorReports"/>
    <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='optionsForm']" mode="xform"/>
    <xsl:apply-templates select="." mode="orderAddresses"/>
    <xsl:apply-templates select="." mode="displayNotes"/>
    <form method="post" id="cart">
      <input type="submit" name="cartBrief" value="Continue Shopping" class="button continue"/>
      <input type="submit" name="cartUpdate" value="Update Order" class="button update"/>
      <input type="submit" name="cartQuit" value="Empty Order" class="button empty"/>
      <xsl:apply-templates select="." mode="orderItems">
        <xsl:with-param name="editQty">true</xsl:with-param>
      </xsl:apply-templates>
      <input type="submit" name="cartBrief" value="Continue Shopping" class="button continue"/>
      <input type="submit" name="cartUpdate" value="Update Order" class="button update"/>
      <input type="submit" name="cartQuit" value="Empty Order" class="button empty"/>
      <div class="terminus">&#160;</div>
    </form>
  </xsl:template>

  <!--#-->
  <!--############################## Order Process - Enter Payment Details ################################-->
  <!--#-->
  <xsl:template match="Order[@cmd='EnterPaymentDetails' or @cmd='SubmitPaymentDetails']" mode="orderProcessTitle">
    <h2>
      <!--Your Order - Please enter your payment details.-->
      <xsl:call-template name="term3020" />
    </h2>
  </xsl:template>

    <xsl:template match="Order[(@cmd='EnterPaymentDetails' or @cmd='SubmitPaymentDetails') and /Page/Contents/Content[@type='xform' and (@name='Secure3D' or @name='Secure3DReturn')]]" mode="orderProcessTitle">
        <h2>
            <!--Your Order - Please enter your payment details.-->
            <xsl:call-template name="term3020-1" />
        </h2>
    </xsl:template>

  <xsl:template match="Order[@cmd='EnterPaymentDetails' or @cmd='SubmitPaymentDetails']" mode="orderProcess">
    <xsl:apply-templates select="." mode="orderProcessTitle"/>
    <xsl:apply-templates select="." mode="orderErrorReports"/>
    <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and (@name='PayForm' or @name='Secure3D' or @name='Secure3DReturn')]" mode="xform"/>
    <form method="post" id="cart" class="ewXform">
      <xsl:apply-templates select="." mode="orderItems"/>
      <input type="submit" name="cartUpdate" value="Revise Order" class="button continue"/>
      <input type="submit" name="cartQuit" value="Cancel Order" class="button empty"/>
      <div class="terminus">&#160;</div>
    </form>
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
            <div class="terminus">&#160;</div>
        </form>
        <iframe name="threeDS" id="threeDS"></iframe>
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
      <div class="terminus">&#160;</div>
    </form>
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

            <ol>
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
                        <div class="terminus">&#160;</div>
                    </li>

                </xsl:if>
            </ol>
        </fieldset>
    </xsl:template>
  <!--#-->
  <!--############################## Order Process - Show Invoice ################################-->
  <!--#-->
  <xsl:template match="Order[@cmd='ShowInvoice' or @cmd='ShowCallbackInvoice']" mode="orderProcessTitle">
    <script>

      // Break out of an iframe, if someone shoves your site
      // into one of those silly top-bar URL shortener things.
      //
      // Passing `this` and re-aliasing as `window` ensures
      // that the window object hasn't been overwritten.

      (function(window) {
      if (window.location !== window.top.location) {
      window.top.location = window.location;
      }
      })(this);

    </script>
    <h2>
      <!--Your Invoice - Thank you for your order.-->
      <xsl:call-template name="term3021" />
    </h2>
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
      <!--SCRIPT LANGUAGE="javascript">if (top.location != location) top.location.href = location.href;</SCRIPT-->
      <div id="cartInvoice">
      <p class="optionButtons">
        <a href="{$secureURL}?pgid={/Page/@id}&amp;cartCmd=Quit" class="btn btn-default button principle" title="Click here to close this invoice and return to the site.">
          <i class="fa fa-chevron-left">
            <xsl:text> </xsl:text>
          </i><xsl:text> </xsl:text>
          <xsl:call-template name="term3078" />
        </a>
      </p>
      <p>
        <!--Invoice Date-->
        <xsl:call-template name="term3022" />
        <xsl:text>:&#160;</xsl:text>
        <xsl:value-of select="@InvoiceDate"/>
      </p>
      <p>
        <!--Invoice Reference-->
        <xsl:call-template name="term3023" />
        <xsl:text>:&#160;</xsl:text>
        <xsl:value-of select="@InvoiceRef"/>
      </p>
      <xsl:if test="@payableType='deposit' and (@payableAmount &gt; 0) ">
        <p>
          <!--Payment Received-->
          <xsl:call-template name="term3024" />
          <xsl:text>:&#160;</xsl:text>
          <xsl:value-of select="$currency"/>
          <xsl:value-of select="format-number(@paymentMade,'0.00')" />
        </p>
        <p>
          <!--Final Payment Reference-->
          <xsl:call-template name="term3025" />
          <xsl:text>:&#160;</xsl:text>
          <strong>
            <xsl:value-of select="@settlementID" />
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

    <xsl:apply-templates select="." mode="orderAddresses"/>
    <xsl:apply-templates select="." mode="displayNotes"/>
    <form method="post" id="cart">
      <xsl:apply-templates select="." mode="orderItems"/>
    </form>
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
  <!--#-->
  <!--##############################Order Addresses ################################-->
  <!--#-->
  <xsl:template match="Order" mode="orderAddresses">
    <xsl:variable name="parentURL">
      <xsl:call-template name="getContentParURL"/>
    </xsl:variable>
    <div class="template template_2_Columns">

      <xsl:if test="Contact[@type='Billing Address']">
        <div id="column1" class="column1">
          <div id="billingAddress" class="cartAddress box Default-Box">
            <xsl:apply-templates select="Contact[@type='Billing Address']" mode="cart">
              <xsl:with-param name="parentURL" select="$parentURL"/>
              <xsl:with-param name="cartType" select="'cart'"/>
            </xsl:apply-templates>
            <xsl:if test="not(@readonly)">
              <!-- THIS SHOULD BE DONE IN THE ABOVE TEMPLATE - WILL -->
              <!--<p class="optionButtons">
						<a href="{$parentURL}?pgid={/Page/@id}&amp;cartCmd=Billing" title="Click here to edit the details you have entered for the billing address" class="button">Edit Billing Address</a>
					</p>-->
            </xsl:if>
          </div>
        </div>
      </xsl:if>
      <xsl:if test="Contact[@type='Delivery Address'] and not(@hideDeliveryAddress)">
        <div id="column2" class="column2">
          <div id="deliveryAddress" class="cartAddress box Default-Box">
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
                <!-- THIS SHOULD BE DONE IN THE ABOVE TEMPLATE - WILL -->
                <!--<p class="optionButtons">
							<a href="{$parentURL}?pgid={/Page/@id}&amp;cartCmd=Delivery" title="Click here to edit the details you have entered for the delivery address" class="button">Edit Delivery Address</a>
						</p>-->
              </xsl:otherwise>
            </xsl:choose>
          </div>
        </div>
      </xsl:if>


      <div class="terminus">&#160;</div>
    </div>
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
      <div id="template_2_Columns" class="template template_2_Columns">
        
        <xsl:choose>
          <xsl:when test="/Page/Contents/Content[@type='xform' and @name='Billing Address']">
            <div id="column1" class="column1">
              <div id="billingAddress" class="cartAddress box Default-Box">
                <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='Billing Address']" mode="xform"/>
              </div>
            </div>
          </xsl:when>
          <xsl:when test="Contact[@type='Billing Address']">
            <div id="column1" class="column1">
              <div id="billingAddress" class="cartAddress box Default-Box">
                <xsl:apply-templates select="Contact[@type='Billing Address']" mode="cart">
                  <xsl:with-param name="parentURL" select="$parentURL"/>
                  <xsl:with-param name="cartType" select="'cart'"/>
                </xsl:apply-templates>
                <!-- THIS SHOULD BE DONE IN THE ABOVE TEMPLATE - WILL -->
                <!--<p class="optionButtons">
							    <a href="{$parentURL}?pgid={/Page/@id}&amp;cartCmd=Billing" title="Click here to edit the details you have entered for the billing address" class="button">Edit Billing Address</a>
						    </p>-->
              </div>
            </div>
          </xsl:when>
        </xsl:choose>
		  <xsl:if test="not(@hideDeliveryAddress)">
          <xsl:choose>
            <xsl:when test="/Page/Contents/Content[@type='xform' and @name='Delivery Address']">
              <div id="column2" class="column2">
                <div id="deliveryAddress" class="cartAddress box Default-Box">
                  <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='Delivery Address']" mode="xform"/>
                </div>
              </div>
            </xsl:when>
            <xsl:when test="Contact[@type='Delivery Address']">
              <div id="column2" class="column2">
                <div id="deliveryAddress" class="cartAddress box Default-Box">
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
                      <!-- THIS SHOULD BE DONE IN THE ABOVE TEMPLATE - WILL -->
                      <!--<p class="optionButtons">
										<a href="{$parentURL}?pgid={/Page/@id}&amp;cartCmd=Delivery" title="Click here to edit the details you have entered for the delivery address" class="button">Edit Delivery Address</a>
									</p>-->
                    </xsl:otherwise>
                  </xsl:choose>
                </div>
              </div>
            </xsl:when>
          </xsl:choose>
        </xsl:if>
        <div class="terminus">&#160;</div>
      </div>
    </xsl:if>
    <!-- Terminus class fix to floating columns -->
    <div class="terminus">&#160;</div>
  </xsl:template>
  <!--#-->

  <!--##############################Order Edit Addresses ################################-->
  <!--#-->
  <xsl:template match="Order" mode="orderEditAddresses">
    <xsl:variable name="parentURL">
      <xsl:call-template name="getContentParURL"/>
    </xsl:variable>

    <xsl:if test="/Page/Contents/Content[@type='xform' and (@name='Delivery Address' or @name='Billing Address') ]">
      <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and (@name='Delivery Address' or @name='Billing Address')]" mode="xform"/>
    </xsl:if>

    <!-- Terminus class fix to floating columns -->
    <div class="terminus">&#160;</div>
  </xsl:template>
  
  
  <!--############################## Order Detail ################################-->
  <!--#-->
  <xsl:template match="Order" mode="orderItems">
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

    <table cellspacing="0" id="cartListing" summary="This table contains a list of the items which you have added to your order. To change the quantity of an item, replace the number under the Qty column and click on Update Order.">
      <tr>
        <th class="heading delete">&#160;</th>
        <th class="heading quantity">
          <!--Qty-->
          <xsl:call-template name="term3039" />
        </th>
        <th class="heading description">
          <!--Description-->
          <xsl:call-template name="term3040" />
        </th>
        <th class="heading ref">
          <!--Ref-->
          <xsl:call-template name="term3041" />
        </th>
        <th class="heading linePrice">
          <!--Price-->
          <xsl:call-template name="term3042" />
        </th>
        <th class="heading lineTotal">
          <!--Line Total-->
          <xsl:call-template name="term3043" />
        </th>
      </tr>
      <xsl:for-each select="Item">
        <xsl:apply-templates select="." mode="orderItem">
          <xsl:with-param name="editQty" select="$editQty"/>
        </xsl:apply-templates>
      </xsl:for-each>
      <xsl:if test="@shippingType &gt; 0">
        <tr>
          <td colspan="5" class="shipping heading">
            <strong>
              <xsl:choose>
                <xsl:when test="/Page/Contents/Content[@name='shippingCostLabel']!=''">
                  <xsl:value-of select="/Page/Contents/Content[@name='shippingCostLabel']"/>
                </xsl:when>
                <xsl:otherwise>
                  <!--Shipping Cost-->
                  <xsl:call-template name="term3044" />
                  <xsl:text>:</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
              <xsl:text>&#160;</xsl:text>
            </strong>
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


          </td>
          <td class="shipping amount">
            <!-- Remmed by Rob 
				<xsl:value-of select="/Page/Cart/@currencySymbol"/>
                <xsl:value-of select="format-number(@shippingCost,'0.00')"/>-->
            <xsl:apply-templates select="/Page" mode="formatPrice">
              <xsl:with-param name="price" select="@shippingCost"/>
              <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
            </xsl:apply-templates>
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
            <xsl:text>&#160;</xsl:text>
          </td>
          <td class="subTotal heading">
            <!--Sub Total-->
            <xsl:call-template name="term3045" />
            <xsl:text>:</xsl:text>
          </td>
          <td class="subTotal amount">
            <!--  Remmed by Rob<xsl:value-of select="/Page/Cart/@currencySymbol"/>
                <xsl:value-of select="format-number(@totalNet, '0.00')"/>-->
            <xsl:apply-templates select="/Page" mode="formatPrice">
              <xsl:with-param name="price" select="@totalNet"/>
              <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
            </xsl:apply-templates>
          </td>
        </tr>

        <tr>
          <td colspan="4">&#160;</td>
          <td class="vat heading">
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
          </td>
          <td class="vat amount">
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
          </td>
        </tr>
      </xsl:if>
      <tr>
        <td colspan="4">&#160;</td>
        <td class="total heading">
          <!--Total Value-->
          <xsl:call-template name="term3048" />
          <xsl:text>:&#160;</xsl:text>
        </td>
        <td class="total amount">
          <!--  Remmed by Rob
			  <xsl:value-of select="/Page/Cart/@currencySymbol"/>
              <xsl:value-of select="format-number(@total, '0.00')"/>
			  -->
          <xsl:apply-templates select="/Page" mode="formatPrice">
            <xsl:with-param name="price" select="@total"/>
            <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
          </xsl:apply-templates>
        </td>
      </tr>
      <xsl:if test="@paymentMade &gt; 0">
        <tr>
          <td colspan="4">&#160;</td>
          <td class="total heading">
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
              <xsl:when test="@payableType='deposit' and not(@transStatus)">
                <!--Deposit Payable-->
                <xsl:call-template name="term3051" />
              </xsl:when>
              <xsl:when test="@payableType='settlement' or (@payableType='deposit' and @transStatus)">
                <!--Amount Outstanding-->
                <xsl:call-template name="term3052" />
              </xsl:when>
            </xsl:choose>
          </td>
          <td class="total amount">
            <xsl:value-of select="$currency"/>
            <xsl:value-of select="format-number(@payableAmount, '0.00')"/>
          </td>
        </tr>
      </xsl:if>
    </table>
    <xsl:if test="$editQty='true' and Item">
      <div id="cartLegend">
        <p class="delete">
          <img src="/ewCommon/images/icons/delete.png" width="20" height="20" alt="delete icon" class="delete icon"/>
          <span class="hidden">&#160;&#160;</span>
          <!--Click on this icon to remove an item from the order.<br/>If you amend the quantity of items please click 'Update Order' before continuing to browse.-->
          <xsl:call-template name="term3038" />
        </p>
      </div>
    </xsl:if>
    <xsl:if test="/Page/Contents/Content[@name='cartMessage']">
      <div class="cartMessage">
        <xsl:copy-of select="/Page/Contents/Content[@name='cartMessage']/node()"/>
      </div>
    </xsl:if>
  </xsl:template>
  <!--#-->

  <!--  ORDER ITEMS BRIEF, used on cartBrief usually for an ajax style brief.-->
  <xsl:template match="Order" mode="orderItemsBrief">
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

    <table cellspacing="0" id="cartListingBrief" summary="This table contains a list of the items which you have added to your order. To change the quantity of an item, replace the number under the Qty column and click on Update Order.">
    <thead> <tr>
        <th class="heading delete">&#160;</th>
        <th class="heading quantity">
          <!--Qty-->
          <xsl:call-template name="term3039" />
        </th>
        <th class="heading description">
          <!--Description-->
          <xsl:call-template name="term3040" />
        </th>
        <th class="heading ref">
          <!--Ref-->
          <xsl:call-template name="term3041" />
        </th>
        <th class="heading linePrice">
          <!--Price-->
          <xsl:call-template name="term3042" />
        </th>
        <th class="heading lineTotal">
          <!--Line Total-->
          <xsl:call-template name="term3048a" />
        </th>
      </tr>
    </thead>
      <tbody>
        <xsl:for-each select="Item">
          <xsl:apply-templates select="." mode="orderItem">
            <xsl:with-param name="editQty" select="$editQty"/>
          </xsl:apply-templates>
        </xsl:for-each>
      </tbody>
      <tfoot>
      <xsl:if test="@shippingCost &gt; 0">
        <tr>
          <td colspan="5" class="shipping heading">
            <strong>
              <xsl:choose>
                <xsl:when test="/Page/Contents/Content[@name='shippingCostLabel']!=''">
                  <xsl:value-of select="/Page/Contents/Content[@name='shippingCostLabel']"/>
                </xsl:when>
                <xsl:otherwise>
                  <!--Shipping Cost-->
                  <xsl:call-template name="term3044" />
                  <xsl:text>:</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
              <xsl:text>&#160;</xsl:text>
            </strong>
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


          </td>
          <td class="shipping amount">
            <xsl:apply-templates select="/Page" mode="formatPrice">
              <xsl:with-param name="price" select="@shippingCost"/>
              <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
            </xsl:apply-templates>
          </td>
        </tr>
      </xsl:if>
      <xsl:if test="@vatRate &gt; 0">
        <tr>
          <td colspan="4">
            <xsl:text>&#160;</xsl:text>
          </td>
          <td class="subTotal heading">
            <!--Sub Total-->
            <xsl:call-template name="term3045" />
            <xsl:text>:</xsl:text>
          </td>
          <td class="subTotal amount">
            <xsl:apply-templates select="/Page" mode="formatPrice">
              <xsl:with-param name="price" select="@totalNet"/>
              <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
            </xsl:apply-templates>
          </td>
        </tr>

        <tr>
          <td colspan="4">&#160;</td>
          <td class="vat heading">
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
          </td>
          <td class="vat amount">
            <xsl:apply-templates select="/Page" mode="formatPrice">
              <xsl:with-param name="price" select="@vatAmt"/>
              <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
            </xsl:apply-templates>
          </td>
        </tr>
      </xsl:if>
      <tr>
        <td colspan="4">&#160;</td>
        <td class="total heading">
          <!--Total Value-->
          <xsl:call-template name="term3048a" />
          <xsl:text>:&#160;</xsl:text>
        </td>
        <td class="total amount">
          <xsl:apply-templates select="/Page" mode="formatPrice">
            <xsl:with-param name="price" select="@total"/>
            <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
          </xsl:apply-templates>
        </td>
      </tr>
      <xsl:if test="@paymentMade &gt; 0">
        <tr>
          <td colspan="4">&#160;</td>
          <td class="total heading">
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
              <xsl:when test="@payableType='deposit' and not(@transStatus)">
                <!--Deposit Payable-->
                <xsl:call-template name="term3051" />
              </xsl:when>
              <xsl:when test="@payableType='settlement' or (@payableType='deposit' and @transStatus)">
                <!--Amount Outstanding-->
                <xsl:call-template name="term3052" />
              </xsl:when>
            </xsl:choose>
          </td>
          <td class="total amount">
            <xsl:value-of select="$currency"/>
            <xsl:value-of select="format-number(@payableAmount, '0.00')"/>
          </td>
        </tr>
      </xsl:if>
      </tfoot>
    </table>
  </xsl:template>
  
  
  
  <!-- ################################# Order Item ######################################## -->
  <!--#-->
  <xsl:template match="Item" mode="orderItem">
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
    <tr class="orderItem">
      <td class="cell delete">
        <xsl:choose>
          <xsl:when test="$editQty!='true'">&#160;</xsl:when>
          <xsl:otherwise>
            <a href="{$parentURL}?cartCmd=Remove&amp;id={@id}" title="click here to remove this item from the list">
              <!--BJR - This either doesnt work or is wrong so i have changed it for the moment to work-->
              <!--<img src="{$secureURL}/ewCommon/images/icons/trash.gif" alt="delete icon - click here to remove this item from the list"/>-->
              <img src="/ewCommon/images/icons/delete.png" width="20" height="20" alt="delete icon - click here to remove this item from the list"/>
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
        <a href="{$siteURL}{@url}" title="">
          <xsl:value-of select="node()"/>
        </a>
        <!-- ################################# Line Options Info ################################# -->
        <xsl:for-each select="Item">
          <span class="optionList">
            <xsl:apply-templates select="option" mode="optionDetail"/>
            <!-- <xsl:if test="@price!=0">
							  Remmed by Rob
							  <xsl:value-of select="$currency"/>
							  <xsl:value-of select="format-number(@price,'#0.00')"/>
								
							  <xsl:apply-templates select="/Page" mode="formatPrice">
								  <xsl:with-param name="price" select="@price"/>
								  <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
							  </xsl:apply-templates>
						  </xsl:if>-->
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
              <!--RRP-->
              <xsl:call-template name="term3053" />
              <xsl:text>:&#160;</xsl:text>
              <strike>
                <xsl:value-of select="$currency"/><xsl:text>:&#160;</xsl:text>
                <xsl:choose>
                  <xsl:when test="position()=1">
                    <!-- Remmed by Rob
								  <xsl:value-of select="format-number(ancestor::Item/DiscountPrice/@OriginalUnitPrice,'#0.00')"/>
								  -->
                    <xsl:apply-templates select="/Page" mode="formatPrice">
                      <xsl:with-param name="price" select="ancestor::Item/DiscountPrice/@OriginalUnitPrice"/>
                      <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
                    </xsl:apply-templates>
                  </xsl:when>
                  <xsl:otherwise>

                    <!-- Remmed by Rob
								  <xsl:value-of select="format-number(preceding-sibling::DiscountPriceLine/@UnitPrice,'#0.00')"/>
								-->
                    <xsl:apply-templates select="/Page" mode="formatPrice">
                      <xsl:with-param name="price" select="preceding-sibling::DiscountPriceLine/@UnitPrice"/>
                      <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
                    </xsl:apply-templates>
                  </xsl:otherwise>
                </xsl:choose>
              </strike>
              <!--less-->
              <xsl:call-template name="term3054" />
              <xsl:text>:&#160;</xsl:text>
              <!-- Remmed by Rob 
							  <xsl:value-of select="$currency"/>
                              <xsl:value-of select="format-number(@UnitSaving,'#0.00')"/>
							  -->
              <xsl:apply-templates select="/Page" mode="formatPrice">
                <xsl:with-param name="price" select="@UnitSaving"/>
                <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
              </xsl:apply-templates>
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
              <!-- Remmed by Rob 
							  <xsl:value-of select="$currency"/>
                              <xsl:value-of select="format-number(@TotalSaving,'#0.00')"/>
							  -->
              <xsl:apply-templates select="/Page" mode="formatPrice">
                <xsl:with-param name="price" select="@TotalSaving"/>
                <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
              </xsl:apply-templates>
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
        <td class="cell linePrice">
          <xsl:if test="DiscountPrice/@OriginalUnitPrice &gt; @price">
            <strike>
              <!-- Remmed by Rob 
					  <xsl:value-of select="$currency"/>
                      <xsl:value-of select="format-number(DiscountPrice/@OriginalUnitPrice,'#0.00')"/>
					-->
              <xsl:apply-templates select="/Page" mode="formatPrice">
                <xsl:with-param name="price" select="DiscountPrice/@OriginalUnitPrice"/>
                <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
              </xsl:apply-templates>
            </strike>
            <br/>
          </xsl:if>
          <!-- Remmed by Rob
				  <xsl:value-of select="$currency"/>
                  <xsl:value-of select="format-number(@price,'#0.00')"/>
				  -->
          <xsl:apply-templates select="/Page" mode="formatPrice">
            <xsl:with-param name="price" select="@price"/>
            <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
          </xsl:apply-templates>
          <xsl:for-each select="Item[@price &gt; 0]">
            <br/>
            <span class="optionList">
              <!-- Remmed by Rob 
					  <xsl:value-of select="$currency"/>
                      <xsl:value-of select="format-number(@price,'#0.00')"/>
					  -->
              <xsl:apply-templates select="/Page" mode="formatPrice">
                <xsl:with-param name="price" select="@price"/>
                <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
              </xsl:apply-templates>
            </span>
          </xsl:for-each>
        </td>
        <td class="cell lineTotal">
          <xsl:if test="DiscountPrice/@OriginalUnitPrice!=DiscountPrice/@UnitPrice">
            <xsl:if test="(DiscountPrice/@OriginalUnitPrice * DiscountPrice/@Units) &gt; @itemTotal">
              <strike>
                <!-- Remmed by Rob
						<xsl:value-of select="$currency"/>
                        <xsl:value-of select="format-number(DiscountPrice/@OriginalUnitPrice * DiscountPrice/@Units,'#0.00')"/>
						-->
                <xsl:apply-templates select="/Page" mode="formatPrice">
                  <xsl:with-param name="price" select="DiscountPrice/@OriginalUnitPrice * DiscountPrice/@Units"/>
                  <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
                </xsl:apply-templates>
              </strike>
              <br/>
            </xsl:if>
          </xsl:if>
          <!-- Remmed by Rob 
				  <xsl:value-of select="$currency"/>
				  -->
          <xsl:choose>
            <xsl:when test="@itemTotal">
              <!-- Remmed by Rob 
					  <xsl:value-of select="format-number(@itemTotal,'#0.00')"/>
					  -->
              <xsl:apply-templates select="/Page" mode="formatPrice">
                <xsl:with-param name="price" select="@itemTotal"/>
                <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
              </xsl:apply-templates>
            </xsl:when>
            <xsl:otherwise>
              <!-- Remmed by Rob 
						<xsl:value-of select="format-number((@price +(sum(*/@price)))* @quantity,'#0.00')"/>
						-->
              <xsl:apply-templates select="/Page" mode="formatPrice">
                <xsl:with-param name="price" select="(@price +(sum(*/@price)))* @quantity"/>
                <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
              </xsl:apply-templates>
            </xsl:otherwise>
          </xsl:choose>
        </td>
      </xsl:if>
    </tr>
  </xsl:template>
  <!-- Displays a list of the option details as selected-->
  <xsl:template match="option" mode="optionDetail">
    <xsl:if test="@groupName!='' and @name!=''">
      <span class="orderItemOption">
        <xsl:value-of select="@groupName"/>
        <xsl:text>: </xsl:text>
        <xsl:value-of select="@name"/>
        <xsl:text>, </xsl:text>
      </span>
    </xsl:if>
  </xsl:template>

  <xsl:template match="option" mode="optionCodeConcat">
    <xsl:if test="@code!=''">-</xsl:if>
    <xsl:value-of select="@code"/>
  </xsl:template>

  <!-- -->
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
        <div class="terminus">&#160;</div>
      </form>
    </div>
  </xsl:template>
  <!-- -->
  <!-- -->
  <xsl:template match="Content[@type='Subscription']" mode="addToCartButton">
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
      <xsl:value-of select="Prices/Price[@type='sale' and @currency=$page/Cart/@currency]/node()"/>
    </xsl:variable>
    <div id="cartButtons{@id}" class="cartButtons">
      <form action="" method="post" class="ewXform">
        <xsl:apply-templates select="." mode="Options_List"/>
        <xsl:if test="not(format-number($price, '#.00')='NaN')">
          <!-- Hard code 1 qty -->
          <!--input class="qtybox" type="text" name="price_{@id}" id="price_{@id}" value="1"/-->
          <input type="hidden" name="qty_{@id}" id="qty_{@id}" value="1"/>
          <xsl:apply-templates select="." mode="addtoCartButtons"/>
        </xsl:if>
        <div class="terminus">&#160;</div>
      </form>
    </div>
  </xsl:template>
  <!-- -->
  <!-- -->
  <xsl:template match="/" mode="showQuantity">
    <xsl:param name="id"/>
    <label for="qty_{$id}">
      <!--Qty-->
      <xsl:call-template name="term3055" />
      <xsl:text>: </xsl:text>
    </label>
    <input type="text" name="qty_{$id}" id="qty_{$id}" value="1" size="3" class="qtybox textbox"/><input class="qtyButton" type="button" value="+" onClick="incrementQuantity('qty_{$id}','+')"/><input class="qtyButton" type="button" value="-" onClick="incrementQuantity('qty_{$id}','-')"/>
  </xsl:template>

  <!-- -->

  <!-- Duplicated Template brings in Content Node - generic for all purchasable content types
        Should be used from now on -->
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
    <label for="qty_{$id}">
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
    <input type="text" name="qty_{@id}" id="qty_{$id}" value="1" size="3" class="qtybox textbox"/>
    <input class="qtyButton" type="button" value="+" onClick="incrementQuantity('qty_{$id}','+')"/>
    <input class="qtyButton" type="button" value="-" onClick="incrementQuantity('qty_{$id}','-')"/>
  </xsl:template>

  <!-- -->
  <xsl:template match="/" mode="addtoCartButtons">
   <xsl:if test="/Page/Contents/Content[@type='giftlist' and @name='cart']">
      <input type="submit" name="glAdd" class="button">
        <xsl:attribute name="value">
          <!--Add to Gift List-->
          <xsl:call-template name="term3056" />
        </xsl:attribute>
      </input>
    </xsl:if>
    <xsl:if test="/Page/Cart[@type='quote']">
      <input type="submit" name="quoteAdd" class="button">
        <xsl:attribute name="value">
          <!--Add to Quote-->
          <xsl:call-template name="term3057" />
        </xsl:attribute>
      </input>
    </xsl:if>
    <input type="submit" name="cartAdd" class="button">
      <xsl:attribute name="value">
        <!--Add to Cart-->
        <xsl:call-template name="term3058" />
      </xsl:attribute>
    </input>
  </xsl:template>
  
  <xsl:template match="Content" mode="addtoCartButtons">
    <input type="submit" name="cartAdd" class="button">
      <xsl:attribute name="value">
        <!--Add to Cart-->
        <xsl:call-template name="term3058" />
      </xsl:attribute>
    </input>
  </xsl:template>

  <!-- -->
  <xsl:template match="Order" mode="cartButtons">
    <xsl:variable name="parentURL">
      <xsl:call-template name="getContentParURL"/>
    </xsl:variable>
    <xsl:variable name="secureURL">
      <xsl:call-template name="getSecureURL"/>
    </xsl:variable>
    <xsl:if test="@cmd='Add' or @cmd='Cart' or @cmd='Confirm' or @cmd='EnterOptions' or @cmd='ShowInvoice'  or @cmd='ShowCallBackInvoice'  or @cmd='EnterPaymentDetails'">
      <div class="cartButtons">
        <xsl:choose>
          <xsl:when test="@cmd='Add' or @cmd='Cart'">
            <xsl:apply-templates select="." mode="principleButton">
              <xsl:with-param name="buttonTitle">
                <!--Go To Checkout-->
                <xsl:call-template name="term3059" />
              </xsl:with-param>
              <xsl:with-param name="buttonCmd">submit</xsl:with-param>
              <xsl:with-param name="buttonClass">checkout</xsl:with-param>
            </xsl:apply-templates>
            <input type="submit" name="submit" class="button continue">
              <xsl:attribute name="value">
                <!--Continue Shopping-->
                <xsl:call-template name="term3060" />
              </xsl:attribute>
            </input>
            <input type="submit" name="submit" class="button update">
              <xsl:attribute name="value">
                <!--Update Cart-->
                <xsl:call-template name="term3061" />
              </xsl:attribute>
            </input>
            <input type="submit" name="submit" class="button empty">
              <xsl:attribute name="value">
                <!--Empty Cart-->
                <xsl:call-template name="term3062" />
              </xsl:attribute>
            </input>
          </xsl:when>
          <xsl:when test="@cmd='Confirm'">
            <xsl:apply-templates select="." mode="principleButton">
              <xsl:with-param name="buttonClass">confirm</xsl:with-param>
              <xsl:with-param name="buttonCmd">submit</xsl:with-param>
            </xsl:apply-templates>
            <xsl:if test="not(@readonly)">
              <input type="submit" name="submit" class="button continue">
                <xsl:attribute name="value">
                  <!--Continue Shopping-->
                  <xsl:call-template name="term3060" />
                </xsl:attribute>
              </input>
              <input type="submit" name="submit" class="button update">
                <xsl:attribute name="value">
                  <!--Update Cart-->
                  <xsl:call-template name="term3061" />
                </xsl:attribute>
              </input>
              <input type="submit" name="submit" class="button empty">
                <xsl:attribute name="value">
                  <!--Empty Cart-->
                  <xsl:call-template name="term3062" />
                </xsl:attribute>
              </input>>
            </xsl:if>
          </xsl:when>
          <xsl:when test="@cmd='EnterOptions' or @cmd='EnterPaymentDetails'">
            <xsl:if test="not(@readonly)">
              <input type="submit" name="submit" class="button continue">
                <xsl:attribute name="value">
                  <!--Continue Shopping-->
                  <xsl:call-template name="term3060" />
                </xsl:attribute>
              </input>
              <input type="submit" name="submit" class="button update">
                <xsl:attribute name="value">
                  <!--Update Cart-->
                  <xsl:call-template name="term3061" />
                </xsl:attribute>
              </input>
              <input type="submit" name="submit" class="button empty">
                <xsl:attribute name="value">
                  <!--Empty Cart-->
                  <xsl:call-template name="term3062" />
                </xsl:attribute>
              </input>
            </xsl:if>
          </xsl:when>
          <xsl:when test="@cmd='ShowInvoice'  or @cmd='ShowCallBackInvoice'">
            <p class="optionButtons">
              <a href="{$secureURL}?pgid={/Page/@id}&amp;cartCmd=Quit" class="btn btn-primary button principle" title="Click here to close this invoice and return to the site.">
                <i class="fa fa-chevron-left">
                  <xsl:text> </xsl:text>
                </i><xsl:text> </xsl:text>Close Invoice and Return to Site
              </a>
            </p>
          </xsl:when>
        </xsl:choose>&#160;
      </div>
    </xsl:if>
  </xsl:template>
  <!-- -->
  <!-- Deprecated - use match="Content" [CR] 2011-06-07 -->
  <xsl:template match="/" mode="Options_List">
    <xsl:param name="SKU"/>
    <xsl:choose>
      <xsl:when test="$SKU='true'">
        <div class="selectOptions">
          <select class="skuOptions">
            <!--<option value="">Please select option</option>-->
            <xsl:choose>
              <xsl:when test="/Page/ContentDetail">
                <xsl:apply-templates select="/Page/ContentDetail/Content/Content[@type='SKU']" mode="skuOptions"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:apply-templates select="/Page/Contents/Content/Content[@type='SKU']" mode="skuOptions"/>
              </xsl:otherwise>
            </xsl:choose>
          </select>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <xsl:for-each select="/Page/ContentDetail/Content/Options/OptGroup">
          <xsl:if test="@name!=''">
            <div class="selectOptions">
              <h4>
                <xsl:value-of select="@name"/>:
              </h4>
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
                <xsl:otherwise>
                  <select size="1" name="opt_{ancestor::Content/@id}_{position()}" id="opt_{ancestor::Content/@id}_{position()}" class="form-control">
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
  <!-- Deprecated -->

  <!-- SKU node -->
  <xsl:template match="Content" mode="skuOptions">
    <option>
      <xsl:attribute name="value">
        <xsl:value-of select="@id"/>
        <xsl:text>_</xsl:text>
        <xsl:choose>
          <xsl:when test="Prices/Price[@currency = $page/Cart/@currency and @type = 'rrp']!=''">
            <xsl:call-template name="formatPrice">
              <xsl:with-param name="price" select="Prices/Price[@currency = $page/Cart/@currency and @type = 'rrp']"/>
              <xsl:with-param name="currency" select="$page/Cart/@currencySymbol"/>
            </xsl:call-template>
          </xsl:when>
          <xsl:otherwise>na</xsl:otherwise>
        </xsl:choose>
        <xsl:text>_</xsl:text>
        <xsl:call-template name="formatPrice">
          <xsl:with-param name="price" select="Prices/Price[@currency = $page/Cart/@currency and @type='sale']"/>
          <xsl:with-param name="currency" select="$page/Cart/@currencySymbol"/>
        </xsl:call-template>
        <xsl:text>_</xsl:text>
        <xsl:value-of select="parent::Content/@id "/>
      </xsl:attribute>
      <xsl:value-of select="Name"/>
    </option>
  </xsl:template>

  <!-- -->

  <!-- Duplicated Template brings in Content Node - generic for all purchasable content types
        Should be used from now on 
  -->
  <!-- -->
  <xsl:template match="Content" mode="Options_List">
    <xsl:choose>
      <xsl:when test="Content[@type='SKU']">
        <!--and @SkuOptions='skus'-->
        <div class="selectOptions">
          <select class="skuOptions">
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
            <div class="selectOptions form-group row">
              <xsl:if test="@selectType!='hidden'">
                <label class="col-xs-3 control-label">
                  <xsl:value-of select="@name"/>
                </label>
              </xsl:if>
              <div class="col-xs-9">
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
            </div>
          </xsl:if>
        </xsl:for-each>
      </xsl:otherwise>
    </xsl:choose>
    <div class="clearfix">
      <xsl:text> </xsl:text>
    </div>
  </xsl:template>
  <!-- -->
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



  <!-- hidden overide for purchasing tickets -->
  <xsl:template match="option[parent::OptGroup[@name='Event'] and ancestor::Content[@type='Ticket']]" mode="List_Options_Hidden">
    <xsl:param name="grpIdx"/>
    <xsl:variable name="value">
      <!-- get this tickets's first parents name -->
      <xsl:apply-templates select="ancestor::Content[@type='Ticket']/ancestor::Content[1]" mode="getDisplayName" />
    </xsl:variable>
    <input type="hidden" name="opt_{ancestor::Content[1]/@id}_{$grpIdx}" value="{$value}" id="opt_{ancestor::Content[1]/@id}_{$grpIdx}_{position()}"/>
  </xsl:template>

  <!-- hidden overide for purchasing tickets -->
  <xsl:template match="option[parent::OptGroup[@name='Date'] and ancestor::Content[@type='Ticket']]" mode="List_Options_Hidden">
    <xsl:param name="grpIdx"/>
    <xsl:variable name="value">
      <!-- get this tickets's date and times -->
      <xsl:variable name="ticket" select="ancestor::Content[@type='Ticket'][1]"/>
      <xsl:if test="$ticket/StartDate/node()!=''">
        <xsl:call-template name="DisplayDate">
          <xsl:with-param name="date" select="$ticket/StartDate/node()"/>
        </xsl:call-template>
        <xsl:if test="$ticket/EndDate/node()!='' and $ticket/EndDate/node() != $ticket/StartDate/node()">
          <xsl:text> - </xsl:text>
          <xsl:call-template name="DisplayDate">
            <xsl:with-param name="date" select="$ticket/EndDate/node()"/>
          </xsl:call-template>
        </xsl:if>
        <xsl:text>&#160;</xsl:text>
        <xsl:if test="$ticket/Times/@start!=''">
          <xsl:value-of select="translate($ticket/Times/@start,',',':')"/>
          <xsl:if test="$ticket/Times/@end!=''">
            <xsl:text> - </xsl:text>
            <xsl:value-of select="translate($ticket/Times/@end,',',':')"/>
          </xsl:if>
        </xsl:if>
      </xsl:if>
    </xsl:variable>
    <input type="hidden" name="opt_{ancestor::Content[1]/@id}_{$grpIdx}" value="{$value}" id="opt_{ancestor::Content[1]/@id}_{$grpIdx}_{position()}"/>
  </xsl:template>
  
  
  

  <xsl:template match="/" mode="productGroupForm">
    <xsl:variable name="parentURL">
      <xsl:call-template name="getContentParURL"/>
    </xsl:variable>
    <input type="hidden" name="sessionID" value="{/Page/Contents/Cart/@sessionId}"/>
    <input type="hidden" name="ordertype" value="{/Page/Contents/Content[@name='orderType']/node()}"/>
    <table cellspacing="0" id="productsGrouped">
      <xsl:attribute name="summary">
        <!--This form lists product options for this page-->
        <xsl:call-template name="term3065" />
      </xsl:attribute>
      <xsl:if test="/Page/@adminMode">
        <tr>
          <xsl:apply-templates select="/Page" mode="inlinePopupAdd">
            <xsl:with-param name="type">Product</xsl:with-param>
            <xsl:with-param name="text">Add Product</xsl:with-param>
            <xsl:with-param name="name">New Product</xsl:with-param>
          </xsl:apply-templates>
        </tr>
      </xsl:if>
      <xsl:for-each select="/Page/Contents/Content[@type='Product']">
        <xsl:sort select="DisplayOrder" data-type="number" order="ascending"/>
        <xsl:variable name="price">
          <xsl:apply-templates select="." mode="showPrice"/>
        </xsl:variable>
        <tr>
          <th>
            <xsl:if test="/Page/@adminMode">
              <div>
                <xsl:apply-templates select="." mode="inlinePopupOptions">
                  <xsl:with-param name="class" select="'list product'"/>
                </xsl:apply-templates>
              </div>
            </xsl:if>
            <h3>
              <xsl:value-of select="Name/node()"/>
            </h3>
          </th>
          <td class="manufacturer">
            <xsl:choose>
              <xsl:when test="Manufacturer/node()">
                <xsl:value-of select="Manufacturer/node()"/>
              </xsl:when>
              <xsl:otherwise>&#160;</xsl:otherwise>
            </xsl:choose>
          </td>
          <td class="">
            <xsl:choose>
              <xsl:when test="Size/node()">
                <xsl:value-of select="Size/node()"/>
              </xsl:when>
              <xsl:otherwise>&#160;</xsl:otherwise>
            </xsl:choose>
          </td>
          <td>
            <xsl:apply-templates select="." mode="displayPrice">
              <xsl:with-param name="displayOutput">h4</xsl:with-param>
              <xsl:with-param name="noLabel">true</xsl:with-param>
            </xsl:apply-templates>
            <xsl:choose>
              <xsl:when test="Prices/BookingFee/node()!=''">
                <!--Booking Fee-->
                <xsl:call-template name="term3063" />
                <xsl:text>:&#160;</xsl:text>
                <xsl:value-of select="$currency"/>
                <xsl:value-of select="Prices/BookingFee/node()"/>
              </xsl:when>
              <xsl:otherwise>&#160;</xsl:otherwise>
            </xsl:choose>
          </td>
          <!-- add option dropdowns -->
          <td class="quantity">
            <input type="text" id="qty_{@id}" name="qty_{@id}" value="0" size="3" class="qtybox textbox"/>
            <input type="button" value="+" class="qty button" onClick="incrementQuantity('qty_{@id}','+')"/>
            <input type="button" value="-" class="qty button" onClick="incrementQuantity('qty_{@id}','-')"/>
          </td>
        </tr>
      </xsl:for-each>
      <xsl:if test="count(/Page/Contents/Content[@type='Product']) &gt; 0">
        <tr>
          <td colspan="6" class="productGroupedFooter">
            <xsl:choose>
              <xsl:when test="/Page/Contents/Content[@name='productsGroupedFooter']">
                <xsl:copy-of select="/Page/Contents/Content[@name='productsGroupedFooter']/node()"/>
              </xsl:when>
              <xsl:otherwise>
                <p>
                  <!--To order, please enter the quantities you require.-->
                  <xsl:call-template name="term3064" />
                </p>
              </xsl:otherwise>
            </xsl:choose>
          </td>
        </tr>
        <tr>
          <td colspan="6" class="buttons">
            <xsl:apply-templates select="/" mode="addtoCartButtons"/>
          </td>
        </tr>
      </xsl:if>
    </table>
  </xsl:template>
  <!-- -->
  <xsl:template match="/" mode="SubscriptionGroupForm">
    <xsl:variable name="parentURL">
      <xsl:call-template name="getContentParURL"/>
    </xsl:variable>
    <input type="hidden" name="sessionID" value="{/Page/Contents/Cart/@sessionId}"/>
    <input type="hidden" name="ordertype" value="{/Page/Contents/Content[@name='orderType']/node()}"/>
    <table cellspacing="0" id="productsGrouped">
      <xsl:attribute name="summary">
        <!--This form lists product options for this page-->
        <xsl:call-template name="term3065" />
      </xsl:attribute>
      <xsl:if test="/Page/@adminMode">
        <tr>
          <xsl:apply-templates select="/Page" mode="inlinePopupAdd">
            <xsl:with-param name="type">Subscription</xsl:with-param>
            <xsl:with-param name="text">Add Subscription</xsl:with-param>
            <xsl:with-param name="name">New Subscription</xsl:with-param>
          </xsl:apply-templates>
        </tr>
      </xsl:if>
      <xsl:for-each select="/Page/Contents/Content[@type='Subscription']">
        <xsl:sort select="DisplayOrder" data-type="number" order="ascending"/>
        <xsl:variable name="SubID" select="@id"/>
        <xsl:variable name="SubGroupId" select="ContentGroup[@type='Subscription']/@id"/>
        <!--Get The group of this sub-->
        <xsl:variable name="hasThisSub">
          <xsl:value-of select="count(/Page/User/Subscriptions/Subscriptions[@nSubscriptionId=$SubID])" />
        </xsl:variable>
        <!--Get the ids of any other subscriptions on this page with this group-->
        <xsl:variable name="SameSubGroupIds">
          <xsl:text>,</xsl:text>
          <xsl:for-each select="/Page/Contents/Content[@type='Subscription' and ContentGroup/@id=$SubGroupId]">
            <xsl:value-of select="@id"/>
            <xsl:text>,</xsl:text>
          </xsl:for-each>
        </xsl:variable>
        <!--check if we have any in the same group-->
        <xsl:variable name="UserHasSameGroup">
          <xsl:for-each select="/Page/User/Subscriptions/Subscriptions">
            <xsl:variable name="FindSub">
              <xsl:text>,</xsl:text>
              <xsl:value-of select="@nSubscriptionId"/>
              <xsl:text>,</xsl:text>
            </xsl:variable>
            <xsl:if test="contains($SameSubGroupIds,$FindSub)">1</xsl:if>
          </xsl:for-each>
        </xsl:variable>
        <xsl:variable name="RenewText">
          <xsl:choose>
            <xsl:when test="$hasThisSub>0 and $hasThisSub&lt;3">
              <!--Renew-->
              <xsl:call-template name="term3066" />
              <xsl:text>&#160;</xsl:text>
            </xsl:when>
            <xsl:when test="$hasThisSub>1"></xsl:when>
            <xsl:when test="$UserHasSameGroup!=''">
              <!--Change to-->
              <xsl:call-template name="term3067" />
              <xsl:text>&#160;</xsl:text>
            </xsl:when>
            <xsl:otherwise>
              <!--Buy-->
              <xsl:call-template name="term3068" />
              <xsl:text>&#160;</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>

        <xsl:variable name="price">
          <xsl:apply-templates select="." mode="showPrice"/>
        </xsl:variable>

        <tr>
          <form action="{/Page/Request/QueryString/Item[@name='path']}" method="post">
            <th>
              <xsl:apply-templates select="." mode="inlinePopupOptions">
                <xsl:with-param name="class" select="'list product'"/>
              </xsl:apply-templates>
              <h3>
                <xsl:value-of select="$RenewText"/>
                <xsl:value-of select="Name/node()"/>
              </h3>
            </th>
            <td>
              <xsl:value-of select="Duration/Length"/>
              <xsl:text> </xsl:text>
              <xsl:value-of select="Duration/Unit"/>
              <xsl:if test="number(Duration/Length/node())&gt;1">
                <xsl:text>s</xsl:text>
              </xsl:if>
            </td>
            <td>
              <xsl:value-of select="$price"/>
              <xsl:text>&#160;</xsl:text>
              <!-- Per -->
              <xsl:call-template name="term3068" />
              <xsl:text>&#160;</xsl:text>
              <xsl:value-of select="PaymentUnit"/>
              <!--Subscription Total Price-->
              <xsl:text> (</xsl:text>
              <xsl:value-of select="/Page/Cart/@currencySymbol"/>
              <xsl:apply-templates select="." mode="getSubscriptionPrice"/>
              <xsl:text>)</xsl:text>
            </td>
            <!-- add option dropdowns -->
            <td class="quantity">
              <input type="hidden" id="qty_{@id}" name="qty_{@id}" value="1" class="qtybox textbox"/>
              <xsl:if test="$RenewText!=''">
                <xsl:apply-templates select="/" mode="addtoCartButtons"/>
              </xsl:if>
            </td>
          </form>
        </tr>
      </xsl:for-each>
    </table>


  </xsl:template>
  <!-- -->
  <xsl:template match="/" mode="addOptionFromString">
    <xsl:param name="optionString"/>
    <xsl:param name="separator"/>
    <xsl:choose>
      <xsl:when test="contains($optionString, $separator)=false">
        <option value="{$optionString}">
          <xsl:value-of select="$optionString"/>
        </option>
      </xsl:when>
      <xsl:otherwise>
        <option value="{substring-before($optionString , $separator)}">
          <xsl:value-of select="substring-before($optionString , $separator)"/>
        </option>
        <xsl:apply-templates select="/" mode="addOptionFromString">
          <xsl:with-param name="separator">,</xsl:with-param>
          <xsl:with-param name="optionString">
            <xsl:value-of select="substring-after($optionString, $separator)"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- -->
  <xsl:template match="Contact" mode="cart">
    <xsl:param name="parentURL"/>
    <xsl:param name="cartType"/>
    <xsl:variable name="secureURL">
      <xsl:call-template name="getSecureURL"/>
    </xsl:variable>
    <xsl:variable name="type">
      <xsl:value-of select="substring-before(@type,' ')"/>
    </xsl:variable>
    <div>
      <xsl:if test="not(/Page/Cart/Order/@cmd='ShowInvoice') and not(/Page/Cart/Order/@cmd='MakePayment') and (ancestor::*[name()='Cart'])">
        <xsl:if test="/Page/Cart/Order/@cmd!='MakePayment'">
          <a href="{$parentURL}?pgid={/Page/@id}&amp;{$cartType}Cmd={$type}" class="btn btn-default btn-sm pull-right">
            <i class="fa fa-pencil">&#160;</i>&#160;Edit <xsl:value-of select="@type"/>
          </a>
        </xsl:if>
      </xsl:if>
        <h4 class="addressTitle">
          <xsl:value-of select="@type"/>
          <xsl:text>&#160;</xsl:text>
          <!--Details-->
          <!--xsl:call-template name="term3070" /-->
        </h4>
        <p>
          <xsl:value-of select="GivenName"/>
          <br/>
        <xsl:if test="Company/node()!=''">
            <xsl:value-of select="Company"/>,
            <br/>
        </xsl:if>

          <xsl:value-of select="Street"/>,
          <br/>
          <xsl:value-of select="City"/>,
          <br/>
        <xsl:if test="State/node()!=''">
            <xsl:value-of select="State"/>
            .<xsl:text> </xsl:text>
        </xsl:if>
          <xsl:value-of select="PostalCode"/>.
          <br/>
        <xsl:if test="Country/node()!=''">
            <xsl:value-of select="Country"/>
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

  </xsl:template>
  <!-- -->
  <!-- NOT USED LOOK FOR MATCH="Order"-->
  <xsl:template match="Order/Notes" mode="displayNotes">
    <xsl:variable name="secureURL">
      <xsl:call-template name="getSecureURL"/>
    </xsl:variable>
    <h3>
      <!--Your comments sent with this order-->
      <xsl:call-template name="term3074" />
    </h3>

    <xsl:for-each select="Notes/*">
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
    </xsl:for-each>

    <xsl:if test="not(@readonly)">
      <p class="optionButtons">
        <a href="{$secureURL}?pgid={/Page/@id}&amp;cartCmd=Notes" class="textButton" title="Click here to edit the notes on this order.">Edit Details</a>
      </p>
    </xsl:if>
  </xsl:template>
  <!-- -->
  <xsl:template name="calcRows">
    <xsl:param name="r0"/>
    <xsl:param name="r1"/>
    <xsl:param name="r2"/>
    <xsl:param name="r3"/>
    <xsl:value-of select="$r0+$r1+$r2+$r3"/>
  </xsl:template>
  <!-- -->
  <xsl:template name="getSecureURL">
    <xsl:value-of select="/Page/Cart/Order/@cartUrl"/>
  </xsl:template>
  <xsl:template name="getSiteURL">
    <xsl:value-of select="/Page/Cart/Order/@siteUrl"/>
  </xsl:template>
	<!-- Module Layouts -->

	<!-- Display Poll -->
	<xsl:template match="Content[@type='Module' and @moduleType='ListOrders']" mode="displayBrief">
		<div class="list orders">
			<table cellpadding="0" cellspacing="1" class="adminList">
				<tbody>
					<tr>
						<th>Order Id</th>
						<th>Status</th>
						<th>Date Placed</th>
						<th>Value</th>
						<th>&#160;</th>
					</tr>
					<xsl:apply-templates select="Order" mode="ListOrders"/>
				</tbody>
			</table>
			<div class="terminus">&#160;</div>
		</div>
	</xsl:template>


	<xsl:template match="Order" mode="ListOrders">
		<xsl:variable name="startPos" select="number(concat(0,/Page/Request/QueryString/Item[@name='startPos']))"/>
		<xsl:variable name="ewCmd" select="/Page/Request/QueryString/Item[@name='ewCmd']"/>
		<tr>
			<td>
				<xsl:value-of select="@InvoiceRef"/>
			</td>
			<td>
				<xsl:value-of select="@statusId"/> - <xsl:choose>
					<xsl:when test="@statusId='0'">New</xsl:when>
					<xsl:when test="@statusId='1'">Items Added</xsl:when>
					<xsl:when test="@statusId='2'">Billing Address Added</xsl:when>
					<xsl:when test="@statusId='3'">Delivery Address Added</xsl:when>
					<xsl:when test="@statusId='4'">Confirmed</xsl:when>
					<xsl:when test="@statusId='5'">Pass for Payment</xsl:when>
					<xsl:when test="@statusId='6'">Completed</xsl:when>
					<xsl:when test="@statusId='7'">Refunded</xsl:when>
					<xsl:when test="@statusId='8'">Failed</xsl:when>
					<xsl:when test="@statusId='9'">Shipped</xsl:when>
					<xsl:when test="@statusId='10'">Deposit Paid</xsl:when>
					<xsl:when test="@statusId='11'">Abandoned</xsl:when>
					<xsl:when test="@statusId='12'">Deleted</xsl:when>
					<xsl:when test="@statusId='13'">Awaiting Payment</xsl:when>
				</xsl:choose>
			</td>
			<td>
				<xsl:value-of select="@InvoiceDate"/>
			</td>
			<td>
				<xsl:value-of select="/Page/Cart/@currencySymbol"/>
				<xsl:value-of select="format-number(@total,'0.00')"/>				
			</td>
			<td>
				<a href="?OrderId={@id}" class="view button">view order</a>
			</td>
		</tr>
	</xsl:template>

	<xsl:template match="Content[@type='order']" mode="ContentDetail">
		<xsl:apply-templates select="Order" mode="orderAddresses"/>
		<xsl:apply-templates select="Order" mode="orderItems"/>
		<a href="{/Page/Request/ServerVariables/Item[@name='PREVIOUS_PAGE']/node()}" class="button">Back</a>
	</xsl:template>

	<!-- ################################# Discount Templates ################################# -->
  <xsl:template match="Page[@layout='Discounts_Listings']" mode="Layout">
    <div class="template" id="template_Discounts_Listings">
      <xsl:apply-templates select="/" mode="Discounts"/>
    </div>
  </xsl:template>
  <!-- -->
  <xsl:template match="/" mode="Discounts">
    <h2>
      <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
        <xsl:with-param name="type">FormattedText,PlainText</xsl:with-param>
        <xsl:with-param name="text">Add Discounts Title</xsl:with-param>
        <xsl:with-param name="name">discountsTitle</xsl:with-param>
      </xsl:apply-templates>
      <xsl:copy-of select="/Page/Contents/Content[@name='discountsTitle' and (@type='FormattedText' or @type='PlainText')]/node()"/>
    </h2>
    <xsl:for-each select="Page/Discounts/Discount">
      <h1>
        <xsl:value-of select="@cDiscountName"/>
      </h1>
      <xsl:if test="Images/img[@class='detail']/@src!=''">
        <xsl:copy-of select="Images/img[@class='detail']"/>
      </xsl:if>
      <p class="details">
        <strong>
          <!--Details-->
          <xsl:call-template name="term3075" />
          <xsl:text>:&#160;</xsl:text>
        </strong>
        <br/>
        <xsl:copy-of select="cDescription"/>
      </p>
      <p class="terms">
        <strong>
          <!--Terms and Conditions-->
          <xsl:call-template name="term3076" />
          <xsl:text>:&#160;</xsl:text>
        </strong>
        <br/>
        <xsl:copy-of select="cTerms"/>
      </p>
      <h4>
        <!--Available On-->
        <xsl:call-template name="term3077" />
        <xsl:text>:&#160;</xsl:text>
      </h4>
      <xsl:apply-templates select="self::Discount" mode="List_Discount_Product_Gallery"/>
    </xsl:for-each>
  </xsl:template>
  <!-- -->
  <!-- -->
  <xsl:template match="Discount" mode="List_Discount_Product_Gallery">
    <xsl:variable name="currentDiscount" select="@nDiscountKey"/>
    <table cellspacing="0" class="discountProducts">
      <xsl:for-each select="Content[@type='Product']">
        <xsl:if test="(position()+2) mod 3 = 0 ">
          <tr>
            <xsl:apply-templates select="/Page/Discounts/Discount[@nDiscountKey=$currentDiscount]/Content[@type='Product']" mode="productBriefGallery">
              <xsl:with-param name="pos">
                <xsl:value-of select="position()"/>
              </xsl:with-param>
            </xsl:apply-templates>
            <xsl:apply-templates select="/Page/Discounts/Discount[@nDiscountKey=$currentDiscount]/Content[@type='Product']" mode="productBriefGallery">
              <xsl:with-param name="pos">
                <xsl:value-of select="position()+1"/>
              </xsl:with-param>
            </xsl:apply-templates>
            <xsl:apply-templates select="/Page/Discounts/Discount[@nDiscountKey=$currentDiscount]/Content[@type='Product']" mode="productBriefGallery">
              <xsl:with-param name="pos">
                <xsl:value-of select="position()+2"/>
              </xsl:with-param>
            </xsl:apply-templates>
          </tr>
        </xsl:if>
      </xsl:for-each>
    </table>
  </xsl:template>
  <!-- -->

  <xsl:template match="Order" mode="orderProgressLegend">
    <xsl:variable name="bMembership">
      <xsl:call-template name="getSettings">
        <xsl:with-param name="sectionName" select="'web'"/>
        <xsl:with-param name="valueName" select="'Membership'"/>
      </xsl:call-template>
    </xsl:variable>
    <div id="cartStepper">
      <div id="quoteStepsTop">&#160;</div>
      <div id="quoteSteps">
        <div>
          <xsl:attribute name="class">
            <xsl:text>step first</xsl:text>
            <xsl:if test="/Page/Cart/Order/@cmd='Cart'">
              <xsl:text> active</xsl:text>
            </xsl:if>
            <xsl:if test="/Page/Cart/Order/@cmd='Logon' or /Page/Cart/Order/@cmd='Notes' or /Page/Cart/Order/@cmd='Delivery' or /Page/Cart/Order/@cmd='Billing' or /Page/Cart/Order/@cmd='ChoosePaymentShippingOption' or /Page/Cart/Order/@cmd='EnterPaymentDetails'">
              <xsl:text> completed</xsl:text>
            </xsl:if>
          </xsl:attribute>
          <div>
            <!--<xsl:text>Step 1 of 6</xsl:text>
                    <br/>-->
            <xsl:text>Cart Details</xsl:text>
          </div>
        </div>
        <xsl:if test="$bMembership='on'">
        <div>
          <xsl:attribute name="class">
            <xsl:text>step</xsl:text>
            <xsl:if test="/Page/Cart/Order/@cmd='Logon'">
              <xsl:text> active</xsl:text>
            </xsl:if>
            <xsl:if test="/Page/Cart/Order/@cmd='Delivery' or /Page/Cart/Order/@cmd='Notes' or /Page/Cart/Order/@cmd='Billing' or /Page/Cart/Order/@cmd='ChoosePaymentShippingOption' or /Page/Cart/Order/@cmd='EnterPaymentDetails'">
              <xsl:text> completed</xsl:text>
            </xsl:if>
          </xsl:attribute>
          <div>
            <!--<xsl:text>Step 2 of 6</xsl:text>
                    <br/>-->
            <xsl:text>Login / Register</xsl:text>
          </div>
        </div>
        </xsl:if>

        <div>
          <xsl:attribute name="class">
            <xsl:text>step</xsl:text>
            <xsl:if test="/Page/Cart/Order/@cmd='Billing'">
              <xsl:text> active</xsl:text>
            </xsl:if>
            <xsl:if test="/Page/Cart/Order/@cmd='Delivery' or /Page/Cart/Order/@cmd='Notes' or /Page/Cart/Order/@cmd='ChoosePaymentShippingOption' or /Page/Cart/Order/@cmd='EnterPaymentDetails'">
              <xsl:text> completed</xsl:text>
            </xsl:if>
          </xsl:attribute>
          <div>
            <!--<xsl:text>Step 3 of 6</xsl:text>
                    <br/>-->
            <xsl:text>Billing Details</xsl:text>
          </div>
        </div>

        <div>
          <xsl:attribute name="class">
            <xsl:text>step</xsl:text>
            <xsl:if test="/Page/Cart/Order/@cmd='Delivery'">
              <xsl:text> active</xsl:text>
            </xsl:if>
            <xsl:if test="/Page/Cart/Order/@cmd='ChoosePaymentShippingOption' or /Page/Cart/Order/@cmd='EnterPaymentDetails'">
              <xsl:text> completed</xsl:text>
            </xsl:if>
          </xsl:attribute>
          <div>
            <!--<xsl:text>Step 4 of 6</xsl:text>
                    <br/>-->
            <xsl:text>Delivery Details</xsl:text>
          </div>
        </div>

        <div>
          <xsl:attribute name="class">
            <xsl:text>step</xsl:text>
            <xsl:if test="/Page/Cart/Order/@cmd='ChoosePaymentShippingOption'">
              <xsl:text> active</xsl:text>
            </xsl:if>
            <xsl:if test="/Page/Cart/Order/@cmd='EnterPaymentDetails'">
              <xsl:text> completed</xsl:text>
            </xsl:if>
          </xsl:attribute>

          <div>
            <!--<xsl:text>Step 5 of 6</xsl:text>
                    <br/>-->
            <xsl:text>Confirm</xsl:text>
          </div>
        </div>

        <div>
          <xsl:attribute name="class">
            <xsl:text>step last</xsl:text>
            <xsl:if test="/Page/Cart/Order/@cmd='EnterPaymentDetails'">
              <xsl:text> active</xsl:text>
            </xsl:if>
          </xsl:attribute>

          <div>
            <!--<xsl:text>Step 6 of 6</xsl:text>
                    <br/>-->
            <xsl:text>Payment Details</xsl:text>
          </div>
        </div>
      </div>
      <div class="quoteStepsTerminus">&#160;</div>
      <div id="quoteStepsBtm">&#160;</div>
    </div>

  </xsl:template>





  <!-- ========================== GROUP ========================== -->

  <xsl:template match="div[@class='pickAddress']" mode="xform">
    <li class="pickAddress">
      <div>
        <xsl:if test="tblCartContact/cContactType/node()='Billing Address'">
          <h3>Billing Address</h3>
        </xsl:if>

        <xsl:if test="tblCartContact/cContactName/node()!=''">
          <strong>
            <xsl:value-of select="tblCartContact/cContactName/node()"/>
          </strong>
          <br/>
        </xsl:if>
        <xsl:if test="tblCartContact/cContactCompany/node()!=''">
          <xsl:value-of select="tblCartContact/cContactCompany/node()"/>
          <br/>
        </xsl:if>
        <xsl:if test="tblCartContact/cContactAddress/node()!=''">
          <xsl:value-of select="tblCartContact/cContactAddress/node()"/>
          <br/>
        </xsl:if>
        <xsl:if test="tblCartContact/cContactTown/node()!=''">
          <xsl:value-of select="tblCartContact/cContactTown/node()"/>
          <br/>
        </xsl:if>
        <xsl:if test="tblCartContact/cContactCity/node()!=''">
          <xsl:value-of select="tblCartContact/cContactCity/node()"/>
          <br/>
        </xsl:if>
        <xsl:value-of select="tblCartContact/cContactState/node()"/>, <xsl:value-of select="tblCartContact/cContactZip/node()"/>
        <br/>
        <xsl:value-of select="tblCartContact/cContactCountry/node()"/>
      </div>
      <div class="pickAddress">
        <xsl:value-of select="tblCartContact/cContactTel/node()"/>
        <xsl:if test="tblCartContact/cContactTel/node()!=''">
          <br/>
        </xsl:if>
        <xsl:value-of select="tblCartContact/cContactFax/node()"/>
        <xsl:if test="tblCartContact/cContactFax/node()!=''">
          <br/>
        </xsl:if>
        <xsl:value-of select="tblCartContact/cContactEmail/node()"/>
      </div>
    </li>
  </xsl:template>

  <!-- -->
  <xsl:template match="group[contains(@class,'addressGrp')]" mode="xform">
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
      <!-- Qui? -->
      <!--<xsl:text> </xsl:text>-->

      <ol>
        <xsl:for-each select="input | secret | select | select1 | range | textarea | upload | group | repeat | hint | help | alert | div | repeat | relatedContent | label[position()!=1] | trigger">
          <xsl:choose>
            <xsl:when test="name()='group'">
              <li>
                <xsl:apply-templates select="." mode="xform"/>
              </li>
            </xsl:when>
            <xsl:otherwise>
              <xsl:apply-templates select="." mode="xform"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:for-each>

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
            <div class="terminus">&#160;</div>
          </li>

        </xsl:if>
      </ol>
    </fieldset>
  </xsl:template>


  <!-- -->


  <!-- List Voucher Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='VoucherList']" mode="displayBrief">
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div class="ProductList">

      <div class="cols{@cols}">
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="articleList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <!-- Product Brief -->
  <xsl:template match="Content[@type='Voucher']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <xsl:param name="pos"/>
    <xsl:variable name="parId">
      <xsl:choose>
        <xsl:when test="@parId &gt; 0">
          <xsl:value-of select="@parId"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="/Page/@id"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="self::Content" mode="getHref">
        <xsl:with-param name="parId" select="$parId"/>
      </xsl:apply-templates>
    </xsl:variable>

    <div class="listItem hproduct">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem hproduct'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <h3 class="fn title">
          <xsl:variable name="title">
            <xsl:apply-templates select="." mode="getDisplayName"/>
          </xsl:variable>
          <a href="{$parentURL}" title="{$title}">
            <xsl:value-of select="$title"/>
          </a>
        </h3>
        <xsl:if test="Images/img/@src!=''">
          <a href="{$parentURL}" class="url">
            <xsl:apply-templates select="." mode="displayThumbnail"/>
          </a>
        </xsl:if>

        <xsl:if test="Manufacturer/node()!=''">
          <p class="manufacturer">
            <xsl:if test="/Page/Contents/Content[@name='makeLabel']">
              <span class="label">
                <xsl:value-of select="/Page/Contents/Content[@name='makeLabel']"/>
              </span>&#160;
            </xsl:if>
            <span class="brand">
              <xsl:value-of select="Manufacturer/node()"/>
            </span>
          </p>
        </xsl:if>

        <xsl:if test="StockCode/node()!=''">
          <p class="sku stockCode">
            <span class="label">
              <xsl:call-template name="term2014" />
              <xsl:text>: </xsl:text>
            </span>
            <xsl:value-of select="StockCode/node()"/>
          </p>
        </xsl:if>

        <xsl:apply-templates select="." mode="displayPrice" />

        <xsl:if test="ShortDescription/node()!=''">
          <div class="description">
            <xsl:apply-templates select="ShortDescription/node()" mode="cleanXhtml"/>
          </div>
        </xsl:if>

        <div class="entryFooter">
          <xsl:apply-templates select="." mode="moreLink">
            <xsl:with-param name="link" select="$parentURL"/>
            <xsl:with-param name="altText">
              <xsl:apply-templates select="." mode="getDisplayName"/>
            </xsl:with-param>
          </xsl:apply-templates>

          <xsl:text> </xsl:text>
        </div>

        <!-- Terminus class fix to floating columns -->
        <div class="terminus">&#160;</div>
        <hr/>
      </div>
    </div>
  </xsl:template>

  <!-- Product Detail -->
  <xsl:template match="Content[@type='Voucher']" mode="ContentDetail">
    <xsl:variable name="thisURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <xsl:variable name="parId" select="@parId" />
    <div class="hproduct product detail">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'hproduct product detail'"/>
      </xsl:apply-templates>

      <xsl:apply-templates select="." mode="displayDetailImage"/>

      <h2 class="fn content-title">
        <xsl:value-of select="Name/node()"/>
      </h2>
      <xsl:if test="StockCode/node()!=''">
        <p class="stockCode">
          <span class="label">
            <xsl:call-template name="term2014" />
          </span>
          <xsl:text>&#160;</xsl:text>
          <xsl:value-of select="StockCode/node()"/>
        </p>
      </xsl:if>
      <xsl:if test="Manufacturer/node()!=''">
        <p class="manufacturer">
          <xsl:if test="/Page/Contents/Content[@name='makeLabel']">
            <span class="label">
              <xsl:value-of select="/Page/Contents/Content[@name='makeLabel']"/>&#160;
            </span>
          </xsl:if>
          <span class="brand">
            <xsl:value-of select="Manufacturer/node()"/>
          </span>
        </p>
      </xsl:if>

      <xsl:apply-templates select="." mode="displayPrice" />

      <xsl:if test="/Page/Cart">
        <xsl:apply-templates select="." mode="addToCartButton"/>
      </xsl:if>

      <xsl:apply-templates select="." mode="SpecLink"/>

      <xsl:if test="Body/node()!=''">
        <div class="description">
          <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>

      <div class="terminus">&#160;</div>
      <div class="entryFooter">
        <xsl:if test="Content[@type='Tag']">
          <div class="tags">
            <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
            <xsl:text> </xsl:text>
          </div>
        </xsl:if>

        <xsl:if test="$page/Request/ServerVariables/Item[@name='PREVIOUS_PAGE']/node()">
          <xsl:apply-templates select="." mode="backLink">
            <xsl:with-param name="link">
              <xsl:choose>
                <xsl:when test="contains($thisURL,$page/Request/ServerVariables/Item[@name='PREVIOUS_PAGE']/node())">
                  <xsl:apply-templates select="$page/Menu/descendant-or-self::MenuItem[@Id=$parId]" mode="getHref" />
                  <xsl:value-of select="$parId"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$page/Request/ServerVariables/Item[@name='PREVIOUS_PAGE']/node()"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:with-param>
            <xsl:with-param name="altText">
              <xsl:call-template name="term2015" />
            </xsl:with-param>
          </xsl:apply-templates>
        </xsl:if>


        <xsl:text> </xsl:text>

      </div>

      <div class="terminus">&#160;</div>

      <!--RELATED CONTENT-->
      <xsl:if test="Content">
        <!-- Reviews  -->
        <xsl:if test="Content[@type='Review']">
          <xsl:apply-templates select="." mode="relatedReviews"/>
        </xsl:if>

        <!-- Products  -->
        <xsl:if test="Content[@type='Product']">
          <div class="relatedcontent">
            <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
              <xsl:with-param name="type">PlainText</xsl:with-param>
              <xsl:with-param name="text">Add Title for Related Products</xsl:with-param>
              <xsl:with-param name="name">relatedProductsTitle</xsl:with-param>
            </xsl:apply-templates>
            <h4>
              <xsl:choose>
                <xsl:when test="/Page/Contents/Content[@name='relatedProductsTitle']">
                  <xsl:apply-templates select="/Page/Contents/Content[@name='relatedProductsTitle']" mode="displayBrief"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>Related Products</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
            </h4>
            <xsl:apply-templates select="/" mode="List_Related_Products">
              <xsl:with-param name="parProductID" select="@id"/>
            </xsl:apply-templates>
          </div>
        </xsl:if>
      </xsl:if>
    </div>
    <div class="terminus">&#160;</div>
  </xsl:template>

  <!-- List Voucher Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='MyVouchers']" mode="displayBrief">
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <div class="ProductList">
      <div class="cols{@cols}">
        <xsl:apply-templates select="Voucher" mode="displayBrief">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>


  <xsl:template match="Voucher" mode="displayBrief">
    <xsl:variable name="currentUrl">
      <xsl:apply-templates select="$currentPage" mode="getHref"/>
    </xsl:variable>
    <div class="voucher">
      <span class="status">
        <xsl:choose>
          <xsl:when test="@usedDate">Used</xsl:when>
          <xsl:otherwise>Unclaimed</xsl:otherwise>
        </xsl:choose>
      </span>
      <span class="issueDate">
        <xsl:call-template name="formatdate">
          <xsl:with-param name="date" select="@issueDate" />
          <xsl:with-param name="format" select="'ddd, dd MMM yyyy HH:mm:ss'" />
        </xsl:call-template>
      </span>
      <span class="code">
        <xsl:value-of select="code/node()"/>
      </span>
      <xsl:choose>
        <xsl:when test="@usedDate">
          <a class="button" href="{$currentUrl}/?VoucherId={@id}">View Details</a>
        </xsl:when>
        <xsl:otherwise>
          <a class="button" href="{$currentUrl}/?VoucherId={@id}">Issue Voucher</a>
        </xsl:otherwise>
      </xsl:choose>
    </div>
  </xsl:template>
  
  
</xsl:stylesheet>

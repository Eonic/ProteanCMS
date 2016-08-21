<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">

  <!--   ################   New Cart  ###############   -->

  <xsl:template match="Order[@cmd='Add' or @cmd='Cart' or @cmd='Confirm']" mode="orderProcess">
    <xsl:apply-templates select="." mode="orderProcessTitle"/>
    <xsl:apply-templates select="." mode="orderErrorReports"/>
    <xsl:apply-templates select="." mode="orderAddresses"/>
    <div class="terminus">&#160;</div>
    <div class="basket">
      <form method="post" id="cart" class="ewXform">
        <div class="cart-btns-top clearfix">
          <xsl:apply-templates select="." mode="principleButton">
            <xsl:with-param name="buttonClass">btn-action</xsl:with-param>
          </xsl:apply-templates>
          <button type="submit" name="cartBrief" value="Continue Shopping" class="btn btn-info btn-sm continue">
            <i class="fa fa-chevron-left">
              <xsl:text> </xsl:text>
            </i><xsl:text> </xsl:text>Continue Shopping
          </button>
          <!--<button type="submit" name="cartUpdate" value="Update Order" class="btn btn-info btn-sm update">
            <i class="fa fa-refresh">
              <xsl:text> </xsl:text>
            </i><xsl:text> </xsl:text>Update Order
          </button>-->
          <!--<button type="submit" name="cartQuit" value="Empty Order" class="btn btn-info btn-sm empty">
            <i class="fa fa-trash-o">
              <xsl:text> </xsl:text>
            </i><xsl:text> </xsl:text>Empty Order
          </button>-->
        </div>
        <xsl:apply-templates select="." mode="orderItems">
          <xsl:with-param name="editQty">true</xsl:with-param>
        </xsl:apply-templates>
        <xsl:apply-templates select="." mode="principleButton">
          <xsl:with-param name="buttonClass">btn-action</xsl:with-param>
        </xsl:apply-templates>
        <button type="submit" name="cartBrief" value="Continue Shopping" class="btn btn-info btn-sm continue">
          <i class="fa fa-chevron-left">
            <xsl:text> </xsl:text>
          </i><xsl:text> </xsl:text>Continue Shopping
        </button>
        <!--<button type="submit" name="cartUpdate" value="Update Order" class="btn btn-info btn-sm update">
          <i class="fa fa-refresh">
            <xsl:text> </xsl:text>
          </i><xsl:text> </xsl:text>Update Order
        </button>
        <button type="submit" name="cartQuit" value="Empty Order" class="btn btn-info btn-sm empty">
          <i class="fa fa-trash-o">
            <xsl:text> </xsl:text>
          </i><xsl:text> </xsl:text>Empty Order
        </button>-->
      </form>
    </div>
  </xsl:template>

  <!--#-->
  <!--############################## Order Procees - Billing ################################-->
  <!--#-->
  <xsl:template match="Order[@cmd='Billing']" mode="orderProcessTitle">

  </xsl:template>

  <xsl:template match="Order[@cmd='Billing']" mode="orderProcess">
    <xsl:apply-templates select="." mode="orderProcessTitle"/>
    <xsl:apply-templates select="." mode="orderErrorReports"/>
    <div id="template_1_Column" class="template template_1_Column">
      <div class="panel panel-default">
        <div class="panel-heading">
          <h2 class="title">Your Address Details</h2>
        </div>
        <div class="panel-body">
          <xsl:apply-templates select="." mode="orderEditAddresses"/>
        </div>
      </div>
    </div>
    <xsl:apply-templates select="." mode="displayNotes"/>
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

    <div id="template_1_Column" class="template template_1_Column">
      <div id="column1">

        <xsl:if test="$anySub=''">
          <div class="account-btns-top clearfix">
            <!--<button href="?pgid={/Page/@id}&amp;cartCmd=Notes" class="btn pull-right btn-action">
                Proceed without creating an account
                <i class="fa fa-chevron-right">
                  <xsl:text></xsl:text>
                </i>
              </button>-->
            <a href="?pgid={/Page/@id}&amp;cartCmd=Notes" class="btn pull-right btn-action">
              Continue with my order <i class="fa fa-chevron-right">
                <xsl:text> </xsl:text>
              </i>
            </a>
          </div>
        </xsl:if>
        <div class="row">
          <div class="col-md-6">
            <div id="cartLogonBox" class="panel panel-default cartBox">
              <div class="panel-heading">
                <h3 class="title">Logon - I have an account</h3>
              </div>
              <div class="panel-body">
                <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='UserLogon']" mode="xform"/>
              </div>
            </div>
          </div>
          <div class="col-md-6">
            <div id="cartRegisterBox" class="panel panel-default cartBox">
              <div class="panel-heading">
                <h3 class="title">Create new account</h3>
              </div>
              <div class="panel-body">
                <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='CartRegistration']" mode="xform"/>
              </div>
            </div>
          </div>
        </div>
      </div>
      <xsl:if test="/Page/Cart/Order/Notes/PromotionalCode!=''">
        <xsl:apply-templates select="." mode="principleButton"/>
        <button type="submit" name="cartBrief" value="Continue Shopping" class="btn btn-info btn-sm continue">
          <i class="fa fa-chevron-left">
            <xsl:text> </xsl:text>
          </i><xsl:text> </xsl:text>Continue Shopping
        </button>
        <button type="submit" name="cartUpdate" value="Update Order" class="btn btn-info btn-sm update">
          <i class="fa fa-refresh">
            <xsl:text> </xsl:text>
          </i><xsl:text> </xsl:text>Update Order
        </button>
        <button type="submit" name="cartQuit" value="Empty Order" class="btn btn-info btn-sm empty">
          <i class="fa fa-trash-o">
            <xsl:text> </xsl:text>
          </i><xsl:text> </xsl:text>Empty Order
        </button>
        <xsl:apply-templates select="." mode="orderItems">
          <xsl:with-param name="editQty">true</xsl:with-param>
        </xsl:apply-templates>
      </xsl:if>
    </div>
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
        <button type="submit" name="cartBrief" value="Continue Shopping" class="btn btn-info btn-sm continue">
          <i class="fa fa-chevron-left">
            <xsl:text> </xsl:text>
          </i><xsl:text> </xsl:text>Continue Shopping
        </button>
        <button type="submit" name="cartUpdate" value="Update Order" class="btn btn-info btn-sm update">
          <i class="fa fa-refresh">
            <xsl:text> </xsl:text>
          </i><xsl:text> </xsl:text>Update Order
        </button>
        <button type="submit" name="cartQuit" value="Empty Order" class="btn btn-info btn-sm empty">
          <i class="fa fa-trash-o">
            <xsl:text> </xsl:text>
          </i><xsl:text> </xsl:text>Empty Order
        </button>
        <!--</div>-->
        <xsl:apply-templates select="." mode="orderItems">
          <xsl:with-param name="editQty">true</xsl:with-param>
        </xsl:apply-templates>
        <!--<div class="cartButtons">-->
        <!--input type="submit" name="quoteLogon" value="Proceed" class="button principle"/-->
        <button type="submit" name="cartBrief" value="Continue Shopping" class="btn btn-info btn-sm continue">
          <i class="fa fa-chevron-left">
            <xsl:text> </xsl:text>
          </i><xsl:text> </xsl:text>Continue Shopping
        </button>
        <button type="submit" name="cartUpdate" value="Update Order" class="btn btn-info btn-sm update">
          <i class="fa fa-refresh">
            <xsl:text> </xsl:text>
          </i><xsl:text> </xsl:text>Update Order
        </button>
        <button type="submit" name="cartQuit" value="Empty Order" class="btn btn-info btn-sm empty">
          <i class="fa fa-trash-o">
            <xsl:text> </xsl:text>
          </i><xsl:text> </xsl:text>Empty Order
        </button>
        <!--</div>-->
        <div class="terminus">&#160;</div>
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
    <div class="panel panel-default">
      <div class="panel-body">
        <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='discountsForm']" mode="xform"/>
      </div>
    </div>

    <div class="box blueEdge">
      <form method="post" id="cart">
        <div class="cartButtons">
          <!--input type="submit" name="quoteLogon" value="Proceed" class="button principle"/-->
          <button type="submit" name="cartBrief" value="Continue Shopping" class="btn btn-info btn-sm continue">
            <i class="fa fa-chevron-left">
              <xsl:text> </xsl:text>
            </i><xsl:text> </xsl:text>Continue Shopping
          </button>
          <button type="submit" name="cartUpdate" value="Update Order" class="btn btn-info btn-sm update">
            <i class="fa fa-refresh">
              <xsl:text> </xsl:text>
            </i><xsl:text> </xsl:text>Update Order
          </button>
          <button type="submit" name="cartQuit" value="Empty Order" class="btn btn-info btn-sm empty">
            <i class="fa fa-trash-o">
              <xsl:text> </xsl:text>
            </i><xsl:text> </xsl:text>Empty Order
          </button>
          <div class="terminus">&#160;</div>
        </div>
        <div>
          <xsl:apply-templates select="." mode="orderItems">
            <xsl:with-param name="editQty">true</xsl:with-param>
          </xsl:apply-templates>
        </div>
        <div class="cartButtons">
          <button type="submit" name="cartBrief" value="Continue Shopping" class="btn btn-info btn-sm continue">
            <i class="fa fa-chevron-left">
              <xsl:text> </xsl:text>
            </i><xsl:text> </xsl:text>Continue Shopping
          </button>
          <button type="submit" name="cartUpdate" value="Update Order" class="btn btn-info btn-sm update">
            <i class="fa fa-refresh">
              <xsl:text> </xsl:text>
            </i><xsl:text> </xsl:text>Update Order
          </button>
          <button type="submit" name="cartQuit" value="Empty Order" class="btn btn-info btn-sm empty">
            <i class="fa fa-trash-o">
              <xsl:text> </xsl:text>
            </i><xsl:text> </xsl:text>Empty Order
          </button>
          <div class="terminus">&#160;</div>
        </div>
        <div class="terminus">&#160;</div>
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
    <div class="panel panel-default cartBox payment-tcs">
      <div class="panel-heading">
        <h2 class="title">Please agree to our terms of business</h2>

      </div>
      <div class="panel-body">
        <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='optionsForm']" mode="xform"/>
      </div>
    </div>
    <div class="panel panel-default cartBox check-address">
      <div class="panel-body">
        <xsl:apply-templates select="." mode="orderAddresses"/>
      </div>
    </div>
    <xsl:apply-templates select="." mode="displayNotes"/>
    <div class="panel panel-default cart-summary">
      <div class="panel-body">
        <form method="post" id="cart">
          <xsl:apply-templates select="." mode="orderItems">
            <xsl:with-param name="editQty">true</xsl:with-param>
          </xsl:apply-templates>
          <button type="submit" name="cartBrief" value="Continue Shopping" class="btn btn-info btn-sm continue">
            <i class="fa fa-chevron-left">
              <xsl:text> </xsl:text>
            </i><xsl:text> </xsl:text>Continue Shopping
          </button>
          <button type="submit" name="cartUpdate" value="Update Order" class="btn btn-info btn-sm update">
            <i class="fa fa-refresh">
              <xsl:text> </xsl:text>
            </i><xsl:text> </xsl:text>Update Order
          </button>
          <button type="submit" name="cartQuit" value="Empty Order" class="btn btn-info btn-sm empty">
            <i class="fa fa-trash-o">
              <xsl:text> </xsl:text>
            </i><xsl:text> </xsl:text>Empty Order
          </button>
          <div class="terminus">&#160;</div>
        </form>
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
    <div class="panel panel-default ccForm">
      <div class="panel-heading">
        <h2 class="title">Please enter your payment details</h2>
      </div>
      <div class="panel-body">
        <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and (@name='PayForm' or @name='Secure3D' or @name='Secure3DReturn')]" mode="xform"/>
      </div>
    </div>
    <!--form method="post" id="cart" class="ewXform">
      <xsl:apply-templates select="." mode="orderItems"/>
      <input type="submit" name="cartUpdate" value="Revise Order" class="button continue"/>
      <input type="submit" name="cartQuit" value="Cancel Order" class="button empty"/>
      <div class="terminus">&#160;</div>
    </form-->
  </xsl:template>

  <xsl:template match="input[@bind='cContactName']" mode="xform_control">
    <xsl:variable name="label_low" select="translate(label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
    <xsl:variable name="inlineHint">
      <xsl:choose>
        <xsl:when test="hint[@class='inline']">
          <xsl:value-of select="hint[@class='inline']/node()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="msg_required_inline"/>
          <xsl:value-of select="$label_low"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <input type="text" name="{$ref}" id="{$ref}">
      <xsl:choose>
        <xsl:when test="@class!=''">
          <xsl:attribute name="class">
            <xsl:value-of select="@class"/>
            <xsl:text> form-control</xsl:text>
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="class">textbox form-control</xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:choose>
        <xsl:when test="value!=''">
          <xsl:attribute name="value">
            <xsl:value-of select="value"/>
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="value">
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
          </xsl:attribute>
          <xsl:attribute name="onfocus">
            <xsl:text>if (this.value=='</xsl:text>
            <xsl:call-template name="escape-js">
              <xsl:with-param name="string" select="$inlineHint"/>
            </xsl:call-template>
            <xsl:text>') {this.value=''}</xsl:text>
          </xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
    </input>
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
        <xsl:apply-templates select="Notes/Notes/*" mode="displayNoteLine"/>
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


  <xsl:template match="input[@bind='cContactEmail']" mode="xform_control">
    <xsl:variable name="label_low" select="translate(label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
    <xsl:variable name="inlineHint">
      <xsl:choose>
        <xsl:when test="hint[@class='inline']">
          <xsl:value-of select="hint[@class='inline']/node()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="msg_required_inline"/>
          <xsl:value-of select="$label_low"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <input type="text" name="{$ref}" id="{$ref}">
      <xsl:choose>
        <xsl:when test="@class!=''">
          <xsl:attribute name="class">
            <xsl:value-of select="@class"/>
            <xsl:text> form-control</xsl:text>
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="class">textbox form-control</xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:choose>
        <xsl:when test="value!=''">
          <xsl:attribute name="value">
            <xsl:value-of select="value"/>
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="value">
            <xsl:choose>
              <xsl:when test ="/Page/User">
                <xsl:value-of select="/Page/User/Email/node()"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$inlineHint"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
          <xsl:attribute name="onfocus">
            <xsl:text>if (this.value=='</xsl:text>
            <xsl:call-template name="escape-js">
              <xsl:with-param name="string" select="$inlineHint"/>
            </xsl:call-template>
            <xsl:text>') {this.value=''}</xsl:text>
          </xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
    </input>
  </xsl:template>

  <!-- Event Detail -->
  <xsl:template match="Content[@type='Event']" mode="ContentDetail">
    <xsl:variable name="thisURL" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url"></xsl:variable>
    <div class="detail vevent content-title">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'detail event'"/>
      </xsl:apply-templates>
      <h2>
        <xsl:apply-templates select="Headline" mode="displayBrief"/>
      </h2>
      <!--RELATED CONTENT-->
      <div class="row">
        <div>
          <xsl:choose>
            <xsl:when test="Content[@type='Ticket']">
              <xsl:attribute name="class">col-md-8</xsl:attribute>
            </xsl:when>
            <xsl:otherwise>
              <xsl:attribute name="class">col-md-12</xsl:attribute>

              <xsl:apply-templates select="." mode="displayDetailImage"/>
            </xsl:otherwise>
          </xsl:choose>









          <xsl:if test="StartDate!=''">
            <p class="date">
              <xsl:if test="StartDate/node()!=''">
                <xsl:call-template name="DisplayDate">
                  <xsl:with-param name="date" select="StartDate/node()"/>
                </xsl:call-template>
              </xsl:if>
              <xsl:if test="EndDate/node()!=StartDate/node()">
                <xsl:text> to </xsl:text>
                <xsl:call-template name="DisplayDate">
                  <xsl:with-param name="date" select="EndDate/node()"/>
                </xsl:call-template>
              </xsl:if>
              <xsl:text>&#160;</xsl:text>
              <xsl:if test="Times/@start!=''">
                <span class="times">
                  <xsl:value-of select="translate(Times/@start,',',':')"/>
                  <xsl:if test="Times/@end!=''">
                    <xsl:text> - </xsl:text>
                    <xsl:value-of select="translate(Times/@end,',',':')"/>
                  </xsl:if>
                </span>
              </xsl:if>
            </p>
          </xsl:if>
          <xsl:if test="Location/Venue!=''">
            <p class="location vcard">
              <span class="fn org">
                <xsl:value-of select="Location/Venue"/>
              </span>
              <xsl:if test="Location/@loc='address'">
                <xsl:apply-templates select="Location/Address" mode="getAddress" />
              </xsl:if>
              <xsl:if test="Location/@loc='geo'">
                <span class="geo">
                  <span class="latitude">
                    <span class="value-title" title="Location/Geo/@latitude"/>
                  </span>
                  <span class="longitude">
                    <span class="value-title" title="Location/Geo/@longitude"/>
                  </span>
                </span>
              </xsl:if>
            </p>
          </xsl:if>
          <xsl:if test="Content[@type='Organisation']">
            <xsl:apply-templates select="Content[@type='Organisation']" mode="displayEventBrief"/>
          </xsl:if>
          <div class="description">
            <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
          </div>


        </div>
        <!-- Tickets  -->
        <xsl:if test="Content[@type='Ticket']">
          <div class="col-md-4">
            <div class="clearfix">
              <xsl:apply-templates select="." mode="displayDetailImage"/>
            </div>
            <xsl:apply-templates select="." mode="RelatedTickets">
              <xsl:with-param name="parTicketID" select="@id"/>
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
          </div>
        </xsl:if>
      </div>
      <div class="terminus">&#160;</div>
      <div class="entryFooter">
        <div class="tags">
          <xsl:apply-templates select="Content[@type='Tag']" mode="displayBrief"/>
          <xsl:text> </xsl:text>
        </div>
        <xsl:apply-templates select="." mode="backLink">
          <xsl:with-param name="link" select="$thisURL"/>
          <xsl:with-param name="altText">
            <xsl:call-template name="term2013" />
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div class="terminus">&#160;</div>
    </div>
  </xsl:template>

  <!-- List Related Tickets-->
  <xsl:template match="Content" mode="RelatedTickets">
    <xsl:param name="sortBy"/>
    <xsl:param name="parProductID"/>
    <form action="" method="post" class="ewXform ProductAddForm">
      <div class="tickets panel panel-default">
        <xsl:apply-templates select="." mode="inlinePopupRelate">
          <xsl:with-param name="type">Ticket</xsl:with-param>
          <xsl:with-param name="text">Add Ticket</xsl:with-param>
          <xsl:with-param name="name"></xsl:with-param>
          <xsl:with-param name="find">false</xsl:with-param>
        </xsl:apply-templates>
        <table class="ticketsGrouped table">
          <tr>
            <th>
              <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
                <xsl:with-param name="type">PlainText</xsl:with-param>
                <xsl:with-param name="text">Add Title for Tickets</xsl:with-param>
                <xsl:with-param name="name">relatedTicketTitle</xsl:with-param>
              </xsl:apply-templates>
              <xsl:choose>
                <xsl:when test="/Page/Contents/Content[@name='relatedTicketTitle']">
                  <xsl:apply-templates select="/Page/Contents/Content[@name='relatedTicketTitle']" mode="displayBrief"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>Tickets</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
            </th>
            <th>
              <strong>Price:</strong>
            </th>
            <th>
              <strong>Qty:</strong>
            </th>
          </tr>
          <xsl:for-each select="/Page/ContentDetail/Content/Content[@type='Ticket']">
            <xsl:sort select="@type" order="ascending"/>
            <xsl:sort select="@displayOrder" order="ascending"/>
            <xsl:apply-templates select="." mode="displayBriefTicket"/>
          </xsl:for-each>
          <div class="terminus">&#160;</div>
        </table>
        <div class="panel-footer">
          <span class="pull-right">
            <xsl:apply-templates select="/" mode="addtoCartButtons"/>
          </span>
        </div>

      </div>
    </form>
  </xsl:template>

  <!-- Ticket related products -->
  <xsl:template match="Content[@type='Ticket']" mode="displayBriefTicket">
    <xsl:param name="sortBy"/>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <tr>
      <td class="ListGroupedTitle">
        <xsl:variable name="title">
          <xsl:apply-templates select="." mode="getDisplayName"/>
        </xsl:variable>
        <xsl:value-of select="$title"/>
      </td>

      <!--<td class="ListGroupedMake">
        <xsl:if test="Manufacturer/node()!=''">
          <p class="productBrief">
            <xsl:if test="/Page/Contents/Content[@name='makeLabel']">
              <span class="label">
                <xsl:value-of select="/Page/Contents/Content[@name='makeLabel']"/>
              </span>&#160;
            </xsl:if>
            <span class="productBrief">
              <xsl:value-of select="Manufacturer/node()"/>
            </span>
          </p>
        </xsl:if>
      </td>
      <td class="ListGroupedSize">
        <span class="productBrief">
          <xsl:apply-templates select="." mode="Options_List"/>
        </span>
      </td>-->
      <td class="ListGroupedPrice">
        <p class="productBrief">
          <!--!Upgrade needed on price - Will !<xsl:apply-templates select="." mode="displayPrice" />-->
          <xsl:choose>
            <xsl:when test="Content[@type='SKU']">
              <xsl:apply-templates select="Content[@type='SKU'][1]" mode="displayPrice" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:apply-templates select="." mode="displayPrice" />
            </xsl:otherwise>
          </xsl:choose>
        </p>
      </td>

      <td class="ListGroupedQty">
        <xsl:choose>
          <xsl:when test="$page/@adminMode">
            <div>
              <xsl:apply-templates select="." mode="inlinePopupOptions" >
                <xsl:with-param name="class" select="'hproduct'"/>
                <xsl:with-param name="sortBy" select="$sortBy"/>
              </xsl:apply-templates>
            </div>
          </xsl:when>
        </xsl:choose>
        <xsl:apply-templates select="." mode="showQuantityGrouped"/>
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="Content" mode="showQuantityGrouped">
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
    <div class="input-group qty">
      <span class="input-group-btn">
        <button class=" qtyButton increaseQty btn btn-default" type="button" value="+" onClick="incrementQuantity('qty_{@id}','+')">
          <i class="fa fa-plus">
            <xsl:text> </xsl:text>
          </i>
        </button>
      </span>
      <input type="text" name="qty_{@id}" id="qty_{$id}" value="0" size="1" class="form-control"/>
      <span class="input-group-btn">
        <button class="qtyButton decreaseQty btn btn-default" type="button" value="-" onClick="incrementQuantity('qty_{@id}','-')">
          <i class="fa fa-minus">
            <xsl:text> </xsl:text>
          </i>
        </button>
      </span>
    </div>
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

        <ul class="steps">
          <li>
            <xsl:attribute name="class">
              <xsl:if test="/Page/Cart/Order/@cmd='Cart'">
                <xsl:text> active</xsl:text>
              </xsl:if>
              <xsl:if test="/Page/Cart/Order/@cmd='Logon' or /Page/Cart/Order/@cmd='Notes' or /Page/Cart/Order/@cmd='Delivery' or /Page/Cart/Order/@cmd='Billing' or /Page/Cart/Order/@cmd='ChoosePaymentShippingOption' or /Page/Cart/Order/@cmd='EnterPaymentDetails'">
                <xsl:text> complete</xsl:text>
              </xsl:if>
            </xsl:attribute>
            <span class="badge badge-info">1</span>
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
                <xsl:text>Login / Register</xsl:text>
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
    <xsl:choose>
      <xsl:when test="Item">
        <div class="table-responsive">
          <table cellspacing="0" id="cartListing" summary="This table contains a list of the items which you have added to your order. To change the quantity of an item, replace the number under the Qty column and click on Update Order." class="table">
            <thead>
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
            </thead>
            <tbody>
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
                          <xsl:if test="@shippingDefaultDestination!=''">
                            <xsl:value-of select="@shippingDefaultDestination"/>
                            <xsl:text> </xsl:text>
                          </xsl:if>
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
                  <td colspan="3">
                    <xsl:text>&#160;</xsl:text>
                  </td>
                  <td class="subTotal heading" colspan="2">
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
                  <td colspan="3">&#160;</td>
                  <td class="vat heading" colspan="2">
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
                <td colspan="3">&#160;</td>
                <td class="total heading" colspan="2">
                  <!--Total Value-->
                  <xsl:call-template name="term3048" />
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
                  <td colspan="3">&#160;</td>
                  <td class="total heading" colspan="2">
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
                    <xsl:apply-templates select="/Page" mode="formatPrice">
                      <xsl:with-param name="price" select="@paymentMade"/>
                      <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
                    </xsl:apply-templates>
                  </td>
                </tr>
              </xsl:if>
              <xsl:if test="@payableAmount &gt; 0">
                <tr>
                  <td colspan="3">&#160;</td>
                  <td class="total heading" colspan="2">
                    <xsl:choose>
                      <xsl:when test="@payableType='deposit' and not(@transStatus)">
                        <!--Deposit Payable-->
                        <xsl:call-template name="term3051" />
                      </xsl:when>
                      <xsl:when test="@payableType='settlement' or (@payableType='deposit' and @transStatus)">
                        <!--Amount Outstanding-->
                        <xsl:call-template name="term3052" />
                      </xsl:when>
                      <xsl:otherwise>
                        Initial Payment:
                      </xsl:otherwise>
                    </xsl:choose>
                  </td>
                  <td class="total amount">
                    <xsl:apply-templates select="/Page" mode="formatPrice">
                      <xsl:with-param name="price" select="@payableAmount"/>
                      <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
                    </xsl:apply-templates>
                  </td>
                </tr>
              </xsl:if>
              <xsl:if test="@repeatPrice &gt; 0">
                <tr>
                  <td colspan="3">&#160;</td>
                  <td class="total heading" colspan="2">
                    <xsl:choose>
                      <xsl:when test="@payableType='deposit' and not(@transStatus)">
                        <!--Deposit Payable-->
                        <xsl:call-template name="term3051" />
                      </xsl:when>
                      <xsl:when test="@payableType='settlement' or (@payableType='deposit' and @transStatus)">
                        <!--Amount Outstanding-->
                        <xsl:call-template name="term3052" />
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="@repeatInterval"/> Installments:
                      </xsl:otherwise>
                    </xsl:choose>
                  </td>
                  <td class="total amount">
                    <xsl:apply-templates select="/Page" mode="formatPrice">
                      <xsl:with-param name="price" select="@repeatPrice"/>
                      <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
                    </xsl:apply-templates>
                  </td>
                </tr>
              </xsl:if>
            </tbody>
          </table>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <!--CART EMPTY-->
      </xsl:otherwise>
    </xsl:choose>
    <xsl:if test="$editQty='true' and Item">
      <div id="cartLegend" class="alert alert-warning">
        <p class="delete">
          <!--Click on this icon to remove an item from the order.<br/>If you amend the quantity of items please click 'Update Order' before continuing to browse.-->
          <xsl:call-template name="term3038b" />
        </p>
      </div>
    </xsl:if>
    <xsl:if test="/Page/Contents/Content[@name='cartMessage']">
      <div class="cartMessage">
        <xsl:copy-of select="/Page/Contents/Content[@name='cartMessage']/node()"/>
      </div>
    </xsl:if>
  </xsl:template>
  <!-- ################################# Order Item ######################################## -->
  <!--#-->

  <xsl:template match="Item" mode="CartProductName">
    <xsl:value-of select="Name"/>
  </xsl:template>

  <xsl:template match="Item[contentType='Ticket']" mode="CartProductName">
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
    <!--<xsl:if test="Images/img[@class='detail']/@src and Images/img[@class='detail']/@src!=''">-->
      <div class="cart-thumbnail">
        <xsl:apply-templates select="productDetail" mode="displayThumbnail">
          <xsl:with-param name="forceResize">true</xsl:with-param>
<xsl:with-param name="crop">true</xsl:with-param>
	  <xsl:with-param name="width">50</xsl:with-param>
    		<xsl:with-param name="height">50</xsl:with-param>
        </xsl:apply-templates>
      </div>
    <!--</xsl:if>-->
    <div class="description">

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
              <xsl:value-of select="$currency"/>
              <xsl:text>:&#160;</xsl:text>
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
    </div>
    <!--<div class="ref">
      <xsl:value-of select="@ref"/>&#160;
      <xsl:for-each select="Item">
        <xsl:apply-templates select="option" mode="optionCodeConcat"/>
      </xsl:for-each>
    </div>-->

    <div class="quantity">
      <div class="quantity-input">
        <xsl:choose>
          <xsl:when test="$editQty!='true'">
            <xsl:value-of select="@quantity"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:choose>
              <xsl:when test="@quantity&lt;'10'">
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
                  <option value="3"> <xsl:if test="@quantity=3">
                      <xsl:attribute name="selected">selected</xsl:attribute>
                    </xsl:if>
                    <xsl:text>3</xsl:text></option>
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
                <input type="text" size="2" name="itemId-{@id}" value="{@quantity}" class="">
                  <xsl:if test="../@readonly">
                    <xsl:attribute name="readonly">readonly</xsl:attribute>
                  </xsl:if>
                </input>
                <button type="submit" name="cartUpdate" value="Update Order" class="btn btn-info btn-xs update">
                  <i class="fa fa-refresh">
                    <xsl:text> </xsl:text>
                  </i><xsl:text> </xsl:text>Update
                </button>
              </xsl:otherwise>
            </xsl:choose>

          </xsl:otherwise>
        </xsl:choose>
      </div>
      <div class="delete">
        <xsl:choose>
          <xsl:when test="$editQty!='true'">&#160;</xsl:when>
          <xsl:otherwise>
            <a href="{$parentURL}?cartCmd=Remove&amp;id={@id}" title="click here to remove this item from the list" class="text-danger delete-link">
              <!--BJR - This either doesnt work or is wrong so i have changed it for the moment to work-->
              <!--<img src="{$secureURL}/ewCommon/images/icons/trash.gif" alt="delete icon - click here to remove this item from the list"/>-->
              <i class="fa fa-trash-o">
                <xsl:text> </xsl:text>
              </i>
              <span> Delete</span>
            </a>
          </xsl:otherwise>
        </xsl:choose>
      </div>
    </div>
    <xsl:if test="not(/Page/Cart/@displayPrice='false')">
      <div class="linePrice">
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
      </div>
      <!--<div class="lineTotal">
        <xsl:if test="DiscountPrice/@OriginalUnitPrice!=DiscountPrice/@UnitPrice">
          <xsl:if test="(DiscountPrice/@OriginalUnitPrice * DiscountPrice/@Units) &gt; @itemTotal">
            <strike>
              -->
      <!-- Remmed by Rob
						<xsl:value-of select="$currency"/>
                        <xsl:value-of select="format-number(DiscountPrice/@OriginalUnitPrice * DiscountPrice/@Units,'#0.00')"/>
						-->
      <!--
              <xsl:apply-templates select="/Page" mode="formatPrice">
                <xsl:with-param name="price" select="DiscountPrice/@OriginalUnitPrice * DiscountPrice/@Units"/>
                <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
              </xsl:apply-templates>
            </strike>
            <br/>
          </xsl:if>
        </xsl:if>
        -->
      <!-- Remmed by Rob 
				  <xsl:value-of select="$currency"/>
				  -->
      <!--
        <xsl:choose>
          <xsl:when test="@itemTotal">
            -->
      <!-- Remmed by Rob 
					  <xsl:value-of select="format-number(@itemTotal,'#0.00')"/>
					  -->
      <!--
            <xsl:apply-templates select="/Page" mode="formatPrice">
              <xsl:with-param name="price" select="@itemTotal"/>
              <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
            </xsl:apply-templates>
          </xsl:when>
          <xsl:otherwise>
            -->
      <!-- Remmed by Rob 
						<xsl:value-of select="format-number((@price +(sum(*/@price)))* @quantity,'#0.00')"/>
						-->
      <!--
            <xsl:apply-templates select="/Page" mode="formatPrice">
              <xsl:with-param name="price" select="(@price +(sum(*/@price)))* @quantity"/>
              <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
            </xsl:apply-templates>
          </xsl:otherwise>
        </xsl:choose>
      </div>-->
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
      <button type="submit" name="{$bCmd}" value="{$bTitle}" class="btn btn-action pull-right {$buttonClass}">
        <xsl:value-of select="$bTitle" />
        &#160;<i class="fa fa-chevron-right">
          <xsl:text> </xsl:text>
        </i>
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
      <input type="text" name="qty_{@id}" id="qty_{$id}" value="1" size="3" class="qtybox textbox"/>
      <button class="btn btn-action" type="button" value="+" onClick="incrementQuantity('qty_{$id}','+')">
        <i class="fa fa-plus">
          <xsl:text> </xsl:text>
        </i>
      </button>
      <button class="btn btn-action" type="button" value="-" onClick="incrementQuantity('qty_{$id}','-')">
        <i class="fa fa-minus">
          <xsl:text> </xsl:text>
        </i>
      </button>
    </div>
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
        <div class="terminus">&#160;</div>
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
            <div class="col-md-6">
              <h3>Billing Address</h3>
              <xsl:apply-templates select="group[div/tblCartContact/cContactType/node()='Billing Address']" mode="xform"/>
            </div>
            <div class="col-md-6">
              <h3>Delivery Addresses</h3>
              <xsl:apply-templates select="group[@class='collection-options']" mode="xform"/>
              <xsl:apply-templates select="group[div/tblCartContact/cContactType/node()!='Billing Address']" mode="xform"/>
              <div class="pull-right">
                <xsl:apply-templates select="submit" mode="xform"/>
              </div>
            </div>
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
      <ol>
        <xsl:for-each select="input | secret | select | select1 | range | textarea | upload | group | repeat | hint | help | alert | div | repeat | relatedContent | label[position()!=1] | trigger | script">
          <xsl:choose>
            <xsl:when test="name()='group'">
              <li>
                <xsl:if test="./@class">
                  <xsl:attribute name="class">
                    <xsl:text>li-</xsl:text>
                    <xsl:value-of select="./@class"/>
                  </xsl:attribute>
                </xsl:if>
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
            <xsl:if test="ancestor::group/descendant-or-self::*[contains(@class,'required')]">
              <label class="required">
                <span class="req">*</span>
                <xsl:text> </xsl:text>
                <xsl:call-template name="msg_required"/>
              </label>
            </xsl:if>
            <!-- For xFormQuiz change how these buttons work -->
            <xsl:apply-templates select="submit" mode="xform"/>
            <!-- Terminus needed for CHROME ! -->
            <!-- Terminus needed for BREAKS IE 7! -->
            <xsl:if test="$browserVersion!='MSIE 7.0'">
              <div class="terminus">&#160;</div>
            </xsl:if>
          </li>
        </xsl:if>
      </ol>
    </fieldset>
  </xsl:template>





  <xsl:template match="div[@class='pickAddress']" mode="xform">
    <li class="pickAddress">
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
    </li>
  </xsl:template>

  <!-- -->
  <xsl:template match="/" mode="addtoCartButtons">
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
    <button type="submit" name="cartAdd" class="btn btn-action">
      <xsl:attribute name="value">
        <!--Add to Cart-->
        <xsl:call-template name="term3058" />
      </xsl:attribute>
      <xsl:call-template name="term3058" />
      &#160;
      <i class="fa fa-shopping-cart">&#160;</i>
    </button>
  </xsl:template>

  <xsl:template match="Order[@cmd='ShowInvoice' or @cmd='ShowCallbackInvoice']" mode="orderProcessTitle">
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
    <div id="cartInvoice">
      <p class="optionButtons">
        <a href="{$secureURL}?pgid={/Page/@id}&amp;cartCmd=Quit" class="btn btn-default button principle" target="_parent" title="Click here to close this invoice and return to the site.">
          <i class="fa fa-chevron-left">
            <xsl:text> </xsl:text>
          </i>
          <xsl:text> </xsl:text>
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
    <div class="panel panel-default">
      <div class="panel-body">
        <xsl:apply-templates select="." mode="orderAddresses"/>
        <xsl:apply-templates select="." mode="displayNotes"/>
      </div>
    </div>
    <div class="panel panel-default">
      <div class="panel-body">
        <form method="post" id="cart">
          <xsl:apply-templates select="." mode="orderItems"/>
        </form>
      </div>
    </div>
  </xsl:template>

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
    <div id="cartListing" class="responsive-cart">
      <div class="cart-headings hidden-xs">
        <div class="description">
          <!--<xsl:call-template name="term3040" />-->
          &#160;
        </div>
        <!--<div class="ref">
          <xsl:call-template name="term3041" />
        </div>-->
        <div class="quantity">
          <!--Qty-->
          <xsl:call-template name="term3039" />
        </div>
        <div class="linePrice">
          <!--Price-->
          <xsl:call-template name="term3042" />
        </div>
        <!--<div class="lineTotal">
          <xsl:call-template name="term3043" />
        </div>-->
      </div>
      <xsl:for-each select="Item">
        <div class="clearfix cart-item">
          <xsl:apply-templates select="." mode="orderItem">
            <xsl:with-param name="editQty" select="$editQty"/>
          </xsl:apply-templates>
        </div>
      </xsl:for-each>

      <div class="totals-row">
        <xsl:if test="@vatRate &gt; 0">
          <div class="vat-row">
            <div>
              <!--xsl:attribute name="rowspan">
									<xsl:call-template name="calcRows">
										<xsl:with-param name="r1"><xsl:choose><xsl:when test="@vatRate &gt; 0">1</xsl:when><xsl:otherwise>0</xsl:otherwise></xsl:choose>	</xsl:with-param>
										<xsl:with-param name="r2"><xsl:choose><xsl:when test="@payableAmount &gt; 0">1</xsl:when><xsl:otherwise>0</xsl:otherwise></xsl:choose>	</xsl:with-param>
										<xsl:with-param name="r3"><xsl:choose><xsl:when test="@paymentMade &gt; 0">1</xsl:when><xsl:otherwise>0</xsl:otherwise></xsl:choose>	</xsl:with-param>
										<xsl:with-param name="r0">2</xsl:with-param>
									</xsl:call-template>
								</xsl:attribute-->
              <xsl:text>&#160;</xsl:text>
            </div>
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
            <!--Total Value-->
            <xsl:call-template name="term3048" />
            <xsl:text>:&#160;</xsl:text>
          </span>
          <span class="total amount">
            <!--  Remmed by Rob
			  <xsl:value-of select="/Page/Cart/@currencySymbol"/>
              <xsl:value-of select="format-number(@total, '0.00')"/>
			  -->
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
        <xsl:if test="@payableAmount &gt; 0">
          <div class="cart-row">
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
          </div>
        </xsl:if>
      </div>

      <xsl:if test="@shippingType &gt; 0">
        <div class="shipping">
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
          <span class="amount">
            <xsl:text>&#160;</xsl:text>
            <!-- Remmed by Rob 
				<xsl:value-of select="/Page/Cart/@currencySymbol"/>
                <xsl:value-of select="format-number(@shippingCost,'0.00')"/>-->
            <xsl:apply-templates select="/Page" mode="formatPrice">
              <xsl:with-param name="price" select="@shippingCost"/>
              <xsl:with-param name="currency" select="/Page/Cart/@currencySymbol"/>
            </xsl:apply-templates>
          </span>
        </div>
      </xsl:if>
    </div>
    <!--<xsl:if test="/Page/Contents/Content[@name='cartMessage']">
      <div class="cartMessage">
        <xsl:copy-of select="/Page/Contents/Content[@name='cartMessage']/node()"/>
      </div>
    </xsl:if>-->
  </xsl:template>

</xsl:stylesheet>



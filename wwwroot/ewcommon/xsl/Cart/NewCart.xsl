<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">

  <!--   ################   New Cart  ###############   -->

  <xsl:template match="Order[@cmd='Add' or @cmd='Cart' or @cmd='Confirm']" mode="orderProcess">
    <xsl:apply-templates select="." mode="orderProcessTitle"/>
    <xsl:apply-templates select="." mode="orderErrorReports"/>
    <xsl:apply-templates select="." mode="orderAddresses"/>
    <div class="terminus">&#160;</div>
    <div class="box blueEdge">
    <form method="post" id="cart" class="ewXform">
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
      <div class="box blueEdge">
        <div class="tl">
          <div class="tr">
            <h2 class="title">Your Address Details</h2>
          </div>
        </div>
        <div class="content">
          <xsl:apply-templates select="." mode="orderEditAddresses"/>
        </div>
        <div class="bl">
          <div class="br">&#160;</div>
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
      <div id="column1" class="column1">
        <xsl:if test="$anySub=''">
          <div id="NotNowBox" class="box blueEdge cartBox">
            <div class="tl">
              <div class="tr">
                <h2 class="title">Proceed without creating an account</h2>
              </div>
            </div>
            <div class="content">
              <a href="?pgid={/Page/@id}&amp;cartCmd=Notes" class="button principle">
                Continue with my order
              </a>
            </div>
            <div class="bl">
              <div class="br">&#160;</div>
            </div>
          </div>
        </xsl:if>
        <div id="cartLogonBox" class="box blueEdge cartBox">
          <div class="tl">
            <div class="tr">
              <h2 class="title">Logon - I have an account</h2>
            </div>
          </div>
          <div class="content">
            <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='UserLogon']" mode="xform"/>
          </div>
          <div class="bl">
            <div class="br">&#160;</div>
          </div>
        </div>
        <div id="cartRegisterBox" class="box blueEdge">
          <div class="tl">
            <div class="tr">
              <h2 class="title">Create new account</h2>
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
    <div class="box blueEdge cartBox">
      <div class="tl">
        <div class="tr">
          <h2 class="title">Please agree to our terms of business</h2>
        </div>
      </div>
      <div class="content">
        <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='optionsForm']" mode="xform"/>
      </div>
      <div class="bl">
        <div class="br">&#160;</div>
      </div>
    </div>
    <xsl:apply-templates select="." mode="orderAddresses"/>
    <xsl:apply-templates select="." mode="displayNotes"/>
    <div class="box blueEdge">
    <form method="post" id="cart">
      <xsl:apply-templates select="." mode="orderItems">
        <xsl:with-param name="editQty">true</xsl:with-param>
      </xsl:apply-templates>
      <input type="submit" name="cartBrief" value="Continue Shopping" class="button continue"/>
      <input type="submit" name="cartUpdate" value="Update Order" class="button update"/>
      <input type="submit" name="cartQuit" value="Empty Order" class="button empty"/>
      <div class="terminus">&#160;</div>
    </form>
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
    <div class="box blueEdge cartBox">
      <div class="tl">
        <div class="tr">
          <h2 class="title">Please enter your payment details</h2>
        </div>
      </div>
      <div class="content">
        <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and (@name='PayForm' or @name='Secure3D' or @name='Secure3DReturn')]" mode="xform"/>
      </div>
      <div class="bl">
        <div class="br">&#160;</div>
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
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="class">textbox</xsl:attribute>
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
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="class">textbox</xsl:attribute>
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
      <xsl:if test="Content">

        <!-- Tickets  -->
        <xsl:if test="Content[@type='Ticket']">
          <div class="relatedcontent tickets box">
            <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
              <xsl:with-param name="type">PlainText</xsl:with-param>
              <xsl:with-param name="text">Add Title for Tickets</xsl:with-param>
              <xsl:with-param name="name">relatedTicketTitle</xsl:with-param>
            </xsl:apply-templates>
            <h2 class="title">
              <xsl:choose>
                <xsl:when test="/Page/Contents/Content[@name='relatedTicketTitle']">
                  <xsl:apply-templates select="/Page/Contents/Content[@name='relatedTicketTitle']" mode="displayBrief"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:text>Tickets</xsl:text>
                </xsl:otherwise>
              </xsl:choose>
            </h2>
            <div class="content">
            <xsl:apply-templates select="/" mode="RelatedTickets">
              <xsl:with-param name="parTicketID" select="@id"/>
              <xsl:with-param name="sortBy" select="@sortBy"/>
            </xsl:apply-templates>
              </div>
          </div>
        </xsl:if>
      </xsl:if>

      <xsl:apply-templates select="." mode="displayDetailImage"/>
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
      <div class="description">
        <xsl:apply-templates select="Body/node()" mode="cleanXhtml"/>
      </div>
      <div class="terminus">&#160;</div>

      
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
  <xsl:template match="/" mode="RelatedTickets">
    <xsl:param name="sortBy"/>
    <xsl:param name="parProductID"/>
    <form action="" method="post" class="ewXform ProductAddForm">
      <table class="ticketsGrouped">
        <tr>
          <th>
            <xsl:text> </xsl:text>
          </th>
          <!--<th>
                  <strong>
                    Make:
                  </strong>
                </th>
                <th>
                  <strong>Size:</strong>
                </th>-->
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
      <div class="ListGroupCart">
        <xsl:apply-templates select="/" mode="addtoCartButtons"/>
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
          <h3 class="GroupedTitle">
            <xsl:variable name="title">
              <xsl:apply-templates select="." mode="getDisplayName"/>
            </xsl:variable>
            <xsl:value-of select="$title"/>
          </h3>
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
            <xsl:otherwise>
              <xsl:apply-templates select="." mode="showQuantityGrouped"/>
            </xsl:otherwise>
          </xsl:choose>
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
    <input type="text" name="qty_{@id}" id="qty_{$id}" value="0" size="3" class="qtybox textbox"/>
    <input class="qtyButton increaseQty" type="button" value="+" onClick="incrementQuantity('qty_{@id}','+')"/>
    <input class="qtyButton decreaseQty" type="button" value="-" onClick="incrementQuantity('qty_{@id}','-')"/>
  </xsl:template>

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
            <xsl:text>Address Details</xsl:text>
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

</xsl:stylesheet>



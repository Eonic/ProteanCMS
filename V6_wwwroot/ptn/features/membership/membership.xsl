<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!--   ###################################################   XForm Page Layouts   ##############################################   -->
  <!-- -->
  <xsl:template match="/" mode="List_User_Details">
    <span class="label">
      <!--Username-->
      <xsl:call-template name="term4000"/>
      <xsl:text>:&#160;</xsl:text>
    </span>
    <xsl:value-of select="/Page/User/@name"/>
    <br/>
    <span class="label">
      <!--First Name-->
      <xsl:call-template name="term4001"/>
      <xsl:text>:&#160;</xsl:text>
    </span>
    <xsl:value-of select="/Page/User/FirstName/node()"/>
    <br/>
    <span class="label">
      <!--Last Name-->
      <xsl:call-template name="term4002"/>
      <xsl:text>:&#160;</xsl:text>
    </span>
    <xsl:value-of select="/Page/User/LastName/node()"/>
    <br/>
    <xsl:if test="/Page/User/Company/node()!=''">
      <span class="label">
        <!--Company-->
        <xsl:call-template name="term4003"/>
        <xsl:text>:&#160;</xsl:text>
      </span>
      <xsl:value-of select="/Page/User/Company/node()"/>
      <br/>
    </xsl:if>
    <xsl:if test="/Page/User/Position/node()!=''">
      <span class="label">
        <!--Position-->
        <xsl:call-template name="term4004"/>
        <xsl:text>:&#160;</xsl:text>
      </span>
      <xsl:value-of select="/Page/User/Position/node()"/>
      <br/>
    </xsl:if>
    <span class="label">
      <!--Email-->
      <xsl:call-template name="term4005"/>
      <xsl:text>:&#160;</xsl:text>
    </span>
    <a href="mailto:{/Page/User/Email/node()}" title="email {/Page/User/FirstName/node()}">
      <xsl:value-of select="/Page/User/Email/node()"/>
    </a>
    <br/>
    <xsl:if test="/Page/User/Website/node()!=''">
      <span class="label">
        <!--Website-->
        <xsl:call-template name="term4006"/>
        <xsl:text>:&#160;</xsl:text>
      </span>
      <a href="{/Page/User/Website/node()}" title="email {/Page/User/Website/node()}">
        <xsl:value-of select="/Page/User/Website/node()"/>
      </a>
    </xsl:if>
  </xsl:template>
  <!-- -->
  <!-- -->
  <xsl:template match="Page[@layout='Logon_Register']" mode="Layout">
    <div id="template_Logon_Register">

      <div id="column1" class="column1">
        <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='UserMyAccount']" mode="xform"/>
        <xsl:text> </xsl:text>
      </div>
      <div id="column2" class="column2">
        <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='UserLogon']" mode="xform"/>
        <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
          <xsl:with-param name="type">FormattedText</xsl:with-param>
          <xsl:with-param name="text">Add Column 2</xsl:with-param>
          <xsl:with-param name="name">column2</xsl:with-param>
        </xsl:apply-templates>
        <xsl:apply-templates select="/Page/Contents/Content[@name='column2']" mode="displayContent"/>
        <xsl:text> </xsl:text>
      </div>
      <p class="backlink">
        <a href="{/Page/Request/ServerVariables/Item[@name='PREVIOUS_PAGE']/node()}">
          <xsl:text>&lt;&lt;&#160;</xsl:text>
          <!--Back-->
          <xsl:call-template name="term4007"/>
        </a>
      </p>
    </div>
  </xsl:template>

  <xsl:template match="Page[@layout='Register']" mode="Layout">
    <div class="template template_2_Columns" id="template_2_Columns">
      <xsl:apply-templates select="/" mode="layoutHeader"/>
      <div id="column1" class="column1">
        <xsl:choose>
          <xsl:when test="/Page/User">
            <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='UserMyAccount']" mode="xform"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='UserRegister']" mode="xform"/>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text> </xsl:text>
      </div>
      <div id="column2" class="column2">
        <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
          <xsl:with-param name="type">FormattedText</xsl:with-param>
          <xsl:with-param name="text">Add Column 2</xsl:with-param>
          <xsl:with-param name="name">column2</xsl:with-param>
        </xsl:apply-templates>
        <xsl:apply-templates select="/Page/Contents/Content[@name='column2']" mode="displayContent"/>
        <xsl:text> </xsl:text>
      </div>
      <p class="backlink">
        <a href="{/Page/Request/ServerVariables/Item[@name='PREVIOUS_PAGE']/node()}">
          <xsl:text>&lt;&lt;&#160;</xsl:text>
          <!--Back-->
          <xsl:call-template name="term4007"/>
        </a>
      </p>
      <xsl:apply-templates select="/" mode="layoutFooter"/>
    </div>
  </xsl:template>

  <xsl:template match="Page[@layout='Activation_Code']" mode="Layout">

    <div class="template template_2_Columns" id="template_2_Columns">
      <xsl:apply-templates select="/" mode="layoutHeader"/>
      <div id="column1">
        <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
          <xsl:with-param name="type">ActivationCodeXform</xsl:with-param>
          <xsl:with-param name="text">Customise Activation Code form</xsl:with-param>
          <xsl:with-param name="name">ActivationCode</xsl:with-param>
        </xsl:apply-templates>
        <xsl:choose>
          <xsl:when test="/Page[@RedirectReason='activation']/Contents/Content[@type='xform' and @name='ActivationCode']">
            <div>
              <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='ActivationCode']/model/instance/SuccessMessage/node()" mode="cleanXhtml"/>
            </div>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='ActivationCode']" mode="xform"/>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text> </xsl:text>
      </div>
      <div id="column2">
        <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
          <xsl:with-param name="type">FormattedText</xsl:with-param>
          <xsl:with-param name="text">Add Column 2</xsl:with-param>
          <xsl:with-param name="name">column2</xsl:with-param>
        </xsl:apply-templates>
        <xsl:apply-templates select="/Page/Contents/Content[@name='column2']" mode="displayContent"/>
        <xsl:text> </xsl:text>
      </div>
      <p class="backlink">
        <a href="{/Page/Request/ServerVariables/Item[@name='PREVIOUS_PAGE']/node()}">
          <xsl:text>&lt;&lt;&#160;</xsl:text>
          <!--Back-->
          <xsl:call-template name="term4007"/>
        </a>
      </p>
      <xsl:apply-templates select="/" mode="layoutFooter"/>
    </div>
  </xsl:template>

  <xsl:template match="Page[@layout='Password_Reminder']" mode="Layout">
    <div id="template_Logon_Register">
      <xsl:apply-templates select="/" mode="layoutHeader"/>
      <div id="body">
        <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and (@name='PasswordReminder' or @name='ResetAccount')]" mode="xform"/>
        <xsl:text> </xsl:text>
      </div>


      <xsl:apply-templates select="/" mode="layoutFooter"/>
    </div>
  </xsl:template>

  <xsl:template match="Page[@layout='Password_Change']" mode="Layout">
    <div id="template_Logon_Register" class="template">
      <section class="wrapper-sm">
        <div class="container content">
          <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and (@name='ResetPassword')]" mode="xform"/>
          <xsl:text> </xsl:text>
        </div>
      </section>
    </div>
  </xsl:template>

  <xsl:template match="Page[@layout='Account_Reset']" mode="Layout">
    <div id="template_Logon_Register" class="template">
      <div>
        <section class="wrapper-sm">
          <div class="container content">
            <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='ConfirmPassword']" mode="xform"/>
            <xsl:text> </xsl:text>
          </div>
        </section>
      </div>
    </div>
  </xsl:template>


  <xsl:template match="Page[@layout='List_Quotes']" mode="Layout">
    <div id="template_List_Quotes">
      <xsl:apply-templates select="/" mode="layoutHeader"/>
      <h3>
        <!--Your Quotes-->
        <xsl:call-template name="term4008"/>
      </h3>
      <xsl:if test="/Page/Request/QueryString/Item[@name='quoteCmd']/node()='Delete'">
        <p class="deleted">
          <!--The following Quote has been deleted.-->
          <xsl:call-template name="term4009"/>
        </p>
      </xsl:if>
      <xsl:choose>
        <xsl:when test="not(/Page/ContentDetail/Content[@type='quote'])">
          <h3>
            <!--You have no quotes saved-->
            <xsl:call-template name="term4010"/>
          </h3>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="/Page/ContentDetail/Content[@type='quote']" mode="listQuote">
            <xsl:with-param name="editQty" select="'false'"/>
          </xsl:apply-templates>
        </xsl:otherwise>
      </xsl:choose>

      <xsl:apply-templates select="/" mode="layoutFooter"/>
    </div>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[@type='quote']" mode="listQuote">
    <xsl:param name="editQty"/>
    <xsl:if test="Quote">
      <h4>
        <!--Quote-->
        <xsl:call-template name="term4011"/>
        <xsl:text>&#160;</xsl:text>
        <xsl:value-of select="Quote/@InvoiceRef"/>
        <xsl:text> - </xsl:text>
        <xsl:value-of select="@created"/>
        <xsl:choose>
          <xsl:when test="@id=/Page/Cart/Quote/@id">
            <xsl:text>&#160;</xsl:text>
            <!--Current Quote-->
            <xsl:call-template name="term4012"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:choose>
              <xsl:when test="/Page/Request/QueryString/Item[@name='quoteCmd']/node()='Delete'">
                <xsl:variable name="currentPageURL">
                  <xsl:apply-templates select="/Page/Menu/MenuItem/descendant-or-self::MenuItem[@id=/Page/@id]" mode="getHref"/>
                </xsl:variable>
                <a href="{$currentPageURL}" title="Back to Quotes" class="button">
                  <xsl:attribute name="title">
                    <!--Back to Quotes-->
                    <xsl:call-template name="term4013"/>
                  </xsl:attribute>
                  <!--Back to Quotes-->
                  <xsl:call-template name="term4013"/>
                </a>
              </xsl:when>
              <xsl:otherwise>
                <xsl:apply-templates select="." mode="quoteActions"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:otherwise>
        </xsl:choose>
      </h4>
      <!--<xsl:apply-templates select="Quote" mode="quoteItems"/>-->
      <xsl:apply-templates select="Quote" mode="quoteListing">
        <xsl:with-param name="editQty" select="$editQty"/>
      </xsl:apply-templates>
    </xsl:if>
  </xsl:template>
  <!-- -->
  <xsl:template match="Quote" mode="quoteActions">
    <xsl:variable name="currentURL">
      <xsl:apply-templates select="/Page/Menu/MenuItem/descendant-or-self::MenuItem[@id=/Page/@id]" mode="getHref"/>
    </xsl:variable>
    <xsl:text>&#160;</xsl:text>
    <a href="{$currentURL}?quoteCmd=MakeCurrent&amp;OrderID={../@id}" class="button">
      <xsl:attribute name="title">
        <!--Make Current Quote-->
        <xsl:call-template name="term4014"/>
      </xsl:attribute>
      <!--Make Current Quote-->
      <xsl:call-template name="term4014"/>
    </a>
    <a href="{$currentURL}?quoteCmd=Delete&amp;OrderID={../@id}" class="btn btn-primary">
      <xsl:attribute name="title">
        <!--Delete Quote-->
        <xsl:call-template name="term4015"/>
      </xsl:attribute>
      <!--Delete Quote-->
      <xsl:call-template name="term4015"/>
    </a>
  </xsl:template>
  <!-- -->
  <xsl:template match="Page[@layout='List_Orders']" mode="Layout">
    <div id="template_List_Orders">
      <xsl:apply-templates select="/" mode="layoutHeader"/>
      <xsl:choose>
        <xsl:when test="not(/Page/ContentDetail/Content[@type='order'])">
          <h3>You have no orders saved</h3>
        </xsl:when>
        <xsl:otherwise>
          <h3>
            <!--Your Orders-->
            <xsl:call-template name="term4016"/>
          </h3>
          <xsl:apply-templates select="/Page/ContentDetail/Content[@type='order' and number(Order/@statusId) &gt;= 6]" mode="listOrders"/>
        </xsl:otherwise>
      </xsl:choose>

      <xsl:apply-templates select="/" mode="layoutFooter"/>
    </div>
  </xsl:template>
  <!-- -->
  <!-- -->
  <xsl:template match="Content[@type='order']" mode="listOrders">
    <xsl:if test="Order">
      <h4>
        <!--Order-->
        <xsl:call-template name="term4017"/>
        <xsl:text>&#160;</xsl:text>
        <xsl:value-of select="Order/@InvoiceRef"/>
        <xsl:text>&#160;-&#160;</xsl:text>
        <xsl:value-of select="Order/@status"/>
        <xsl:text>&#160;-&#160;</xsl:text>
        <xsl:value-of select="@created"/>
      </h4>
      <xsl:apply-templates select="Order" mode="orderItems"/>
    </xsl:if>
  </xsl:template>
  <!-- -->

  <!--   ###################################################   Membership Sub Routines   ##############################################   -->
  <!-- -->
  <xsl:template match="User" mode="loggedInGreeting">
    <div id="loggedIn">
      <p>
        <!--You are logged in as-->
        <xsl:call-template name="term4018"/>
        <xsl:text>&#160;</xsl:text>
        <xsl:copy-of select="FirstName"/>
        <xsl:text>&#160;</xsl:text>
        <xsl:copy-of select="LastName"/>
        <xsl:text>&#160;</xsl:text>
        <a href="?ewCmd=logoff" title="Logout" class="btn btn-custom">
          <!--Logout-->
          <xsl:call-template name="term4019"/>
          <xsl:text> </xsl:text>
        </a>
      </p>

    </div>
  </xsl:template>
  <!-- -->
  <xsl:template match="/" mode="loginBrief_Col">
    <div id="loginBrief_Col">
      <xsl:choose>
        <xsl:when test="/Page/User">
          <!--Logged in as-->
          <xsl:call-template name="term4020"/>
          <xsl:text>:&#160;</xsl:text>
          <div class="userName">
            <xsl:choose>
              <xsl:when test="/Page/User/FirstName!=''">
                <xsl:value-of select="/Page/User/FirstName/node()"/>
                <xsl:text>&#160;</xsl:text>
                <xsl:value-of select="/Page/User/LastName/node()"/>
                <xsl:text>&#160;</xsl:text>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="/Page/User/@name"/>
              </xsl:otherwise>
            </xsl:choose>
          </div>
          <div>

            <button type="submit" name="Logout" class="btn btn-custom principle" onclick="window.location.href='?ewCmd=logoff'">

              <xsl:call-template name="term4019"/>
              <xsl:text></xsl:text>
            </button>

          </div>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='UserLogon']" mode="loginBrief"/>
        </xsl:otherwise>
      </xsl:choose>

      <!--p>
            <a href="?ewCmd=UserIntegrations&amp;ewCmd2=connect&amp;dirId={$dirId}&amp;integration={$provider}.GetRequestToken" class="button">
                <img src="/ewcommon/images/integrations/sign-in-with-twitter-d.png"/>
            </a>
        </p-->

    </div>
  </xsl:template>
  <!--   #############################################   generic login   #############################################   -->

  <!-- Overide for Login so title isn't a link, that way we use the footer link for password reminder. -->
  <xsl:template match="Content[@type='Module' and @moduleType='MembershipLogon']" mode="moduleLink">
    <xsl:apply-templates select="." mode="moduleTitle"/>
  </xsl:template>

  <xsl:template match="Content[@type='xform' and @name='UserLogon']" mode="loginBrief">
    <form method="post" action="" id="UserLogon" name="UserLogon" class="form-inline" onsubmit="form_check(this)" xmlns="http://www.w3.org/1999/xhtml">
      <div class="form-group">
        <label class="sr-only" for="ewmLogon/username">
          <!--Username-->
          <xsl:call-template name="term4000" />
        </label>
        <input type="text" name="cUserName" id="cUserName" class="form-control required" value="" onfocus="if (this.value=='Please enter username') {this.value=''}" placeholder="Email"/>
      </div>
      <div class="form-group">
        <label class="sr-only" for="ewmLogon/password">
          <!--Password-->
          <xsl:call-template name="term4001" />
        </label>
        <input type="password" name="cPassword" id="cPassword" class="form-control password required"  placeholder="Password"/>
      </div>
      <input name="ewmLogon/@ewCmd" class="hidden" value="membershipLogon"/>
      <button type="submit" name="submit" value="Login" class="btn btn-primary" onclick="disableButton(this);">
        Sign in
      </button>
      <xsl:if test="alert">
        <xsl:apply-templates select="alert" mode="xform"/>
      </xsl:if>
    </form>
  </xsl:template>
  <!--xsl:template match="Page[@layout='User_Contact']" mode="Layout">
    <xsl:apply-templates select="Contents/Content[@type='xform' and @name='Edit_User_Contact']" mode="xform"/>
    <p class="backlink">
      <a href="/My+Account">&lt;&lt; Back</a>
    </p>
  </xsl:template-->
  <!-- -->
  <xsl:template match="Page[@layout='User_Contact']" mode="Layout">
    <div class="template" id="template_User_Addresses">
      <xsl:apply-templates select="/" mode="layoutHeader"/>
      <xsl:choose>
        <xsl:when test="/Page/User">
          <xsl:choose>
            <xsl:when test="Contents/Content[@type='xform' and @name='Edit_User_Contact']">
              <xsl:apply-templates select="Contents/Content[@type='xform' and @name='Edit_User_Contact']" mode="xform"/>
              <p class="backlink">
                <a href="{$currentPage/@url}">
                  <xsl:text>&lt;&lt;&#160;</xsl:text>
                  <!--Back-->
                  <xsl:call-template name="term4007" />
                </a>
              </p>
            </xsl:when>
            <xsl:otherwise>
              <xsl:apply-templates select="/" mode="List_Contact_Details"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="Contents/Content[@type='xform' and @name='UserLogon']" mode="xform"/>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:apply-templates select="/" mode="layoutFooter"/>
    </div>
  </xsl:template>
  <!-- -->
  <xsl:template match="/" mode="List_Contact_Details">
    <p class="addButton">
      <a class="button" href="{$currentPage/@url}?memCmd=addContact">
        <xsl:attribute name="title">
          <!--Add Contact-->
          <xsl:call-template name="term4026" />
        </xsl:attribute>
        <!--+ Add New Contact Address-->
        <xsl:call-template name="term4021" />
      </a>
    </p>
    <xsl:for-each select="/Page/User/Contacts/Contact">
      <xsl:apply-templates select="." mode="contactAddressBrief"/>
      <xsl:text> </xsl:text>
    </xsl:for-each>
  </xsl:template>
  <!-- -->
  <xsl:template match="Contact" mode="contactAddressBrief">
    <div class="list contactAddress">
      <h4>
        <xsl:value-of select="cContactType"/>
      </h4>
      <h3>
        <xsl:value-of select="cContactName"/>
      </h3>
      <p>
        <xsl:if test="cContactCompany/node()!=''">
          <xsl:value-of select="cContactCompany"/>
          <br/>
        </xsl:if>
        <xsl:if test="cContactAddress/node()!=''">
          <xsl:value-of select="cContactAddress"/>
          <br/>
        </xsl:if>
        <xsl:if test="cContactCity/node()!=''">
          <xsl:value-of select="cContactCity"/>
          <br/>
        </xsl:if>
        <xsl:if test="cContactState/node()!=''">
          <xsl:value-of select="cContactState"/>
          <br/>
        </xsl:if>
        <xsl:if test="cContactZip/node()!=''">
          <xsl:value-of select="cContactZip"/>
          <br/>
        </xsl:if>
        <xsl:if test="cContactCountry/node()!=''">
          <xsl:value-of select="cContactCountry"/>
          <br/>
        </xsl:if>
        <xsl:if test="cContactTel/node()!=''">
          <xsl:value-of select="cContactTel"/>
          <br/>
        </xsl:if>
        <xsl:if test="cContactFax/node()!=''">
          <xsl:value-of select="cContactFax"/>
          <br/>
        </xsl:if>
        <xsl:if test="cContactEmail/node()!=''">
          <xsl:value-of select="cContactEmail"/>
          <br/>
        </xsl:if>
      </p>
      <p class="buttons">
        <a class="button" href="{$currentPage/@url}?memCmd=editContact&amp;id={nContactKey}">
          <xsl:attribute name="title">
            <!--Edit Contact-->
            <xsl:call-template name="term4024" />
          </xsl:attribute>
          <!--Edit-->
          <xsl:call-template name="term4022" />
        </a>
        <xsl:text>&#160;&#160;&#160;</xsl:text>
        <a class="btn btn-primary" href="{$currentPage/@url}?memCmd=delContact&amp;id={nContactKey}">
          <xsl:attribute name="title">
            <!--Delete Contact-->
            <xsl:call-template name="term4025" />
          </xsl:attribute>
          <!--Delete-->
          <xsl:call-template name="term4023" />
        </a>
      </p>
    </div>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[@type='Module' and Type/node()='userLogon']" mode="moduleDisplay">
    <xsl:choose>
      <xsl:when test="/Page/User">
        <p>
          <!--Logged in as-->
          <xsl:call-template name="term4020" />
          <xsl:text>:&#160;</xsl:text>
          <xsl:value-of select="/Page/User/@name"/>
        </p>
        <p>
          <xsl:value-of select="/Page/User/FirstName/node()"/>
          <xsl:text>&#160;</xsl:text>
          <xsl:value-of select="/Page/User/LastName/node()"/>
        </p>
        <xsl:if test="/Page/User/Position/node()!=''">
          <p>
            <xsl:value-of select="/Page/User/Position/node()"/>
          </p>
        </xsl:if>
        <p>
          <a href="mailto:{/Page/User/Email/node()}" title="{/Page/User/Email/node()}">
            <xsl:value-of select="/Page/User/Email/node()"/>
          </a>
        </p>
        <div>
          <button type="submit" name="Logout" class="btn btn-custom principle" onclick="window.location.href='?ewCmd=logoff'">
            <xsl:call-template name="term4019"/>
            <xsl:text> </xsl:text>
          </button>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='UserLogon']" mode="xform"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="User" mode="displayUserDetails">
    <xsl:variable name="dirId" select="/Page/User/@id"/>
    <p>
      <!--Logged in as-->
      <xsl:call-template name="term4020" />
      <xsl:text>:&#160;</xsl:text>
      <xsl:value-of select="/Page/User/@name"/>
    </p>
    <p>
      <xsl:value-of select="/Page/User/FirstName/node()"/>
      <xsl:text>&#160;</xsl:text>
      <xsl:value-of select="/Page/User/LastName/node()"/>
    </p>
    <xsl:if test="/Page/User/Position/node()!=''">
      <p>
        <xsl:value-of select="/Page/User/Position/node()"/>
      </p>
    </xsl:if>
    <p>
      <a href="mailto:{/Page/User/Email/node()}" title="{/Page/User/Email/node()}">
        <xsl:value-of select="/Page/User/Email/node()"/>
      </a>
    </p>
    <div class="ewXform">
      <button type="submit" name="Logout" class="btn btn-custom principle" onclick="window.location.href='?ewCmd=logoff'">
        <xsl:call-template name="term4019"/>
        <xsl:text> </xsl:text>
      </button>
    </div>
    <p>
      <!--a href="?ewCmd=UserIntegrations&amp;ewCmd2=connect&amp;dirId={$dirId}&amp;integration=Twitter.GetRequestToken" class="button">
              <img src="/ewcommon/images/integrations/sign-in-with-twitter-d.png"/>
          </a-->
    </p>
  </xsl:template>

  <!-- Membership Login Module -->
  <xsl:template match="Content[@type='Module' and (@moduleType='MembershipLogon')]" mode="displayBrief">
    <xsl:choose>
      <xsl:when test="not(/Page/User)">
        <xsl:apply-templates select="." mode="xform"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="/Page/User" mode="displayUserDetails" />
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text> </xsl:text>
  </xsl:template>

  <!-- Membership Register Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='MembershipRegister']" mode="displayBrief">
	  
    <xsl:choose>
      <xsl:when test="@activationMsg!=''">
        Your activation link has been sent.
      </xsl:when>
      <xsl:when test="$page/User/@status!='1'">
        You must activate your account before you can update your details.
        <br/>
        <a class="btn btn-primary" href="?ewCmd=RegisterResendActivation">Resend Activation Link</a>
      </xsl:when>
      <xsl:otherwise>       
            <xsl:apply-templates select="." mode="xform"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="Content[@type='Module' and @moduleType='MembershipRegister']" mode="cleanXhtml">
	
    <xsl:apply-templates select="." mode="xform"/>
  </xsl:template>


  <xsl:template match="Content[@type='Module' and (@moduleType='MembershipPasswordReminder')]" mode="displayBrief">
    <xsl:apply-templates select="." mode="xform"/>
  </xsl:template>


  <!-- List Users -->
  <xsl:template match="Content[@type='Module' and @moduleType='MembershipListUsers']" mode="displayBrief">
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>

    <div class="{$contentType}List">
      <div class="cols{@cols}">
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
          <xsl:with-param name="sortBy" select="@sortBy"/>
        </xsl:apply-templates>
      </div>
    </div>

  </xsl:template>

  <!-- NewsArticle Brief -->
  <xsl:template match="User" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <!-- articleBrief -->
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <div class="listItem user">
      <!-- not needed for directory items - maybe add edit in admin later. -->
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem user'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <h3 class="title">
          <xsl:apply-templates select="." mode="getDisplayName"/>
        </h3>
        <xsl:if test="Images/img/@src!=''">
          <xsl:apply-templates select="." mode="displayThumbnail"/>
        </xsl:if>
        <p class="username">
          <xsl:text>(</xsl:text>
          <xsl:value-of select="@name"/>
          <xsl:text>)</xsl:text>
        </p>
        <xsl:if test="Position/node()">
          <p class="position">
            <xsl:value-of select="Position/node()"/>
          </p>
        </xsl:if>
        <p class="email">
          <a href="mailto:{Email/node()}">
            <xsl:attribute name="title">
              <xsl:text>Email </xsl:text>
              <xsl:apply-templates select="." mode="getDisplayName"/>
            </xsl:attribute>
            <xsl:value-of select="Email/node()"/>
          </a>
        </p>
        <!--<div class="entryFooter">
          <xsl:apply-templates select="." mode="displayTags"/>
          <xsl:apply-templates select="." mode="moreLink">
            <xsl:with-param name="link" select="$parentURL"/>
            <xsl:with-param name="altText">
              <xsl:value-of select="Headline/node()"/>
            </xsl:with-param>
          </xsl:apply-templates>
        </div>-->
      </div>
    </div>
  </xsl:template>


  <!-- ##### Ecommerce List Orders module ##### -->
  <xsl:template match="Content[@type='Module' and @moduleType='ListOrders']" mode="displayBrief">
    <div class="EcommerceListOrders">
      <xsl:choose>
        <xsl:when test="Order[@statusId&gt;=6]">
          <div class="clearfix ordert-item">
            <xsl:apply-templates select="Order[@statusId&gt;=6]" mode="orderListRow">
              <xsl:sort select="substring-after(@InvoiceRef,'-')" order="descending" data-type="number"/>
            </xsl:apply-templates>
          </div>
        </xsl:when>
        <xsl:otherwise>
          <p>You do not have any previous orders</p>
        </xsl:otherwise>
      </xsl:choose>

    </div>
  </xsl:template>

  <xsl:template match="Order" mode="orderListRow">
    <xsl:variable name="href">
      <xsl:variable name="parentURL">
        <xsl:apply-templates select="$page//MenuItem[@id=$page/@id]" mode="getHref"/>
      </xsl:variable>
      <xsl:value-of select="$parentURL"/>
      <xsl:choose>
        <xsl:when test="contains($parentURL, '?')">&amp;</xsl:when>
        <xsl:otherwise>?</xsl:otherwise>
      </xsl:choose>
      <xsl:text>OrderId=</xsl:text>
      <xsl:value-of select="@id"/>
    </xsl:variable>
    
    <xsl:attribute name="class">
      <xsl:text>order-item</xsl:text>
    </xsl:attribute>
    <div class="order-status">
      Status: <xsl:value-of select="@status"/>
    </div>
    <div class="order-description">
      <a href="{$href}" title="View order">
        <xsl:value-of select="@InvoiceRef"/>
      </a>
    </div>
    <div class="order-date">
      <xsl:if test="@InvoiceDate">
        <xsl:value-of select="@InvoiceDate"/>
      </xsl:if>
    </div>
    <div class="order-price">
      <xsl:call-template name="formatPrice">
        <xsl:with-param name="currency" select="$page/Cart/@currencySymbol"/>
        <xsl:with-param name="price" select="@total" />
      </xsl:call-template>

    </div>
    <div class="order-view">
      <a class="btn btn-custom button principle" href="{$href}" title="View order">View order</a>
    </div>
  </xsl:template>



  <xsl:template match="Order" mode="orderListCartFull">
    <xsl:variable name="href">
      <xsl:variable name="parentURL">
        <xsl:apply-templates select="$page//MenuItem[@id=$page/@id]" mode="getHref"/>
      </xsl:variable>

      <xsl:value-of select="$parentURL"/>
      <xsl:choose>
        <xsl:when test="contains($parentURL, '?')">&amp;</xsl:when>
        <xsl:otherwise>?</xsl:otherwise>
      </xsl:choose>
      <xsl:text>OrderId=</xsl:text>
      <xsl:value-of select="@id"/>
    </xsl:variable>

    <div class="listItem order">
      <h3>
        <xsl:text>Order </xsl:text>
        <xsl:value-of select="@InvoiceRef"/>
        <xsl:text> - </xsl:text>
        <xsl:apply-templates select="." mode="getStatus"/>
        <xsl:if test="@InvoiceDate">
          <xsl:text> - </xsl:text>
          <xsl:value-of select="@InvoiceDate"/>
        </xsl:if>
      </h3>
      <xsl:apply-templates select="." mode="orderItems"/>
      <div class="morelink">
        <a class="btn btn-custom button principle" href="{$href}">View order</a>
      </div>
    </div>
  </xsl:template>

  <!-- Returns a status message -->
  <xsl:template match="Order" mode="getStatus">
    <!--<xsl:value-of select="@statusId"/>
		<xsl:text> - </xsl:text>-->
    <xsl:choose>
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
  </xsl:template>

  <xsl:template match="Content[@type='order']" mode="ContentDetail">
    <xsl:variable name="previousURL">
      <xsl:value-of select="$page/Request/ServerVariables/Item[@name='PREVIOUS_PAGE']"/>
    </xsl:variable>

    <div class="container detail order-detail ">
 
        <xsl:apply-templates select="Order" mode="orderAddressesView"/>
        <xsl:apply-templates select="Order" mode="orderItems"/>

        <div class="morelink">
          <a href="{$previousURL}" class="btn btn-custom back-to-orders">Back to orders</a>
        </div>

    </div>
  </xsl:template>
  <!-- ##### /Ecommerce List Orders module ##### -->

  <!--#-->
  <!--##############################Order Addresses ################################-->
  <!--#-->
  <xsl:template match="Order" mode="orderAddressesView">
    <xsl:variable name="parentURL">
      <xsl:call-template name="getContentParURL"/>
    </xsl:variable>
    <h1>Order Details</h1>
    <div class="row" id="order-addresses">

      <div class="col-lg-4 ">
        <div class="card order-detail-card">
          <div class="card-body">
            <dl class="">
              <dt>
                Order Date
              </dt>
              <dd>
                <xsl:call-template name="DD_Mon_YYYY">
                  <xsl:with-param name="date">
                    <xsl:value-of select="@InvoiceDateTime"/>
                  </xsl:with-param>
                  <xsl:with-param name="showTime">true</xsl:with-param>
                </xsl:call-template>
              </dd>
              <dt>
                Order Reference
              </dt>
              <dd>
                <xsl:value-of select="@InvoiceRef"/>
              </dd>

              <xsl:if test="@payableType='deposit' and (@payableAmount &gt; 0) ">
                <dt>
                  Payment Received
                </dt>
                <dd>
                  <xsl:value-of select="$currency"/>
                  <xsl:value-of select="format-number(@paymentMade,'0.00')" />
                </dd>
                <dt>Final Payment Reference/Link</dt>
                <dd>
                  <xsl:variable name="secureURL">
                    <xsl:text>http</xsl:text>
                    <xsl:if test="$page/Request/ServerVariables/Item[@name='HTTPS']='on'">s</xsl:if>
                    <xsl:text>://</xsl:text>
                    <xsl:value-of select="$page/Request/ServerVariables/Item[@name='SERVER_NAME']"/>
                  </xsl:variable>
                  <a href="{$secureURL}?cartCmd=Settlement&amp;SettlementRef={@settlementID}">
                    <xsl:value-of select="@settlementID" />
                  </a>
                </dd>
              </xsl:if>
              <xsl:if test="@payableType='settlement'">
                <dt>Payment Made</dt>
                <dd>
                  <xsl:value-of select="$currencySymbol"/>&#160;
                  <xsl:value-of select="format-number(@paymentMade,'0.00')" />
                </dd>
                <dt>
                  <strong>Total Outstanding</strong>
                </dt>
                <dd>
                  <strong>
                    <xsl:value-of select="$currencySymbol"/>&#160;<xsl:value-of select="format-number(@outstandingAmount, '0.00')"/>
                  </strong>
                </dd>
              </xsl:if>
            </dl>
            <xsl:variable name="secureURL">
              <xsl:text>http</xsl:text>
              <xsl:if test="$page/Request/ServerVariables/Item[@name='HTTPS']='on'">s</xsl:if>
              <xsl:text>://</xsl:text>
              <xsl:value-of select="$page/Request/ServerVariables/Item[@name='SERVER_NAME']"/>
            </xsl:variable>
            <xsl:if test="@settlementID!=''">
              <a href="{$secureURL}?cartCmd=Settlement&amp;SettlementRef={@settlementID}" class="btn btn-danger">
                Make Final Payment
              </a>
            </xsl:if>
          </div>
        </div>
      </div>
      <div class="col-lg-8">
        <xsl:if test="Contact[@type='Billing Address']">

          <div id="billingAddress" class="cartAddress box Default-Box">
            <xsl:apply-templates select="Contact[@type='Billing Address']" mode="cartlist">
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
        </xsl:if>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Contact" mode="cartlist">
    <xsl:param name="parentURL"/>
    <xsl:param name="cartType"/>
    <xsl:variable name="secureURL">
      <xsl:call-template name="getSecureURL"/>
    </xsl:variable>
    <xsl:variable name="type">
      <xsl:value-of select="substring-before(@type,' ')"/>
    </xsl:variable>
    <button class="btn btn-outline-secondary hidden-lg hidden-xl hidden-xxl order-address-btn" type="button" data-bs-toggle="collapse" data-bs-target="#cart-address-collapse" aria-expanded="false" aria-controls="cart-address-collapse">
      <xsl:text>Address Details </xsl:text>
      <i class="fas fa-caret-down">
        <xsl:text> </xsl:text>
      </i>
    </button>
    <div class="collapse dont-collapse-md" id="cart-address-collapse">
      <div class="row">

        <div class="col-lg-6">
          <xsl:apply-templates select="." mode="contact-cartlist"/>
        </div>
        <div class="col-lg-6">
          <xsl:apply-templates select="parent::Order/Contact[@type='Delivery Address']" mode="contact-cartlist"/>
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

  <xsl:template match="Contact" mode="contact-cartlist">
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

        <h4 class="addressTitle card-title">
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

        </h4>
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
          <xsl:if test="Details/GiftAid/node()!=''">
            <!--Fax-->
            GiftAid
            <xsl:text>:&#160;</xsl:text>
            Agreed
            <br/>
          </xsl:if>
          <xsl:value-of select="Street/node()"/>
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

  <!-- ##### Membership User Contacts module ##### -->
  <xsl:template match="Content[@moduleType='MembershipUserContacts']" mode="displayBrief">
    <xsl:variable name="editForm" select="Content[@name='Edit_User_Contact']"/>
    <div class="MembershipUserContacts">
      <xsl:choose>
        <xsl:when test="$editForm">
          <!-- Add or edit an address -->
          <xsl:apply-templates select="$editForm" mode="xform"/>
        </xsl:when>
        <xsl:otherwise>
          <!-- View addresses -->

          <div class="list orders cols{@cols}">
            <xsl:apply-templates select="$page/User[1]/Contacts/Contact" mode="membershipUserContactsDisplayBrief"/>
            <xsl:text> </xsl:text>
          </div>
          <a class="btn btn-custom add-address-btn" href="{$currentPage/@url}?memCmd=addContact">
            <i class="fa fa-plus">
              <xsl:text> </xsl:text>
            </i>
            <xsl:text> </xsl:text>
            Add New Address
          </a>
        </xsl:otherwise>
      </xsl:choose>

    </div>
  </xsl:template>

  <!-- ##### Membership User Contacts module ##### -->
  <xsl:template match="Content[@moduleType='MembershipCompanyContacts']" mode="displayBrief">
    <xsl:variable name="editForm" select="Content[@name='Edit_User_Contact']"/>
    <div class="MembershipUserContacts">
      <xsl:choose>
        <xsl:when test="$editForm">
          <!-- Add or edit an address -->
          <xsl:apply-templates select="$editForm" mode="xform"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:for-each select="$page/User[1]/Company">
            <xsl:if test="Name">
              <!--code commented by sonali bcoz this functionality allowed only for admin so..-->
              <!--<a class="btn btn-primary principle" href="{$currentPage/@url}?memCmd=addContact&amp;ParentDirId={@id}">
                <i class="fa fa-plus">
                  <xsl:text> </xsl:text>
                </i><xsl:text> </xsl:text>
                Add New Address
              </a>-->
              <h3>
                Addresses for <xsl:value-of select="Name/node()"/>
              </h3>

              <div class="list orders row">
                <xsl:apply-templates select="Contacts/Contact" mode="membershipUserContactsDisplayBrief"/>

              </div>
            </xsl:if>
          </xsl:for-each>
        </xsl:otherwise>
      </xsl:choose>

    </div>
  </xsl:template>

  <xsl:template match="Contact" mode="membershipUserContactsDisplayBrief">
    <div class="listItem contact contactbrief ">
      <div class="lIinner">
        <h3>
          <xsl:value-of select="cContactType"/>
        </h3>

        <xsl:value-of select="cContactName"/>
        <br/>
        <xsl:if test="cContactCompany/node()!=''">
          <xsl:value-of select="cContactCompany"/>
          <br/>
        </xsl:if>
        <xsl:if test="cContactAddress/node()!=''">
          <xsl:value-of select="cContactAddress"/>
          <br/>
        </xsl:if>
        <xsl:if test="cContactCity/node()!=''">
          <xsl:value-of select="cContactCity"/>
          <br/>
        </xsl:if>
        <xsl:if test="cContactState/node()!=''">
          <xsl:value-of select="cContactState"/>
          <br/>
        </xsl:if>
        <xsl:if test="cContactZip/node()!=''">
          <xsl:value-of select="cContactZip"/>
          <br/>
        </xsl:if>
        <xsl:if test="cContactCountry/node()!=''">
          <xsl:value-of select="cContactCountry"/>
          <br/>
        </xsl:if>
        <xsl:if test="cContactTel/node()!=''">
          <xsl:value-of select="cContactTel"/>
          <br/>
        </xsl:if>
        <xsl:if test="cContactFax/node()!=''">
          <xsl:value-of select="cContactFax"/>
          <br/>
        </xsl:if>
        <xsl:if test="cContactEmail/node()!=''">
          <xsl:value-of select="cContactEmail"/>
          <br/>
        </xsl:if>


        <div class="buttons">
          <a class="btn btn-outline-primary" href="{$currentPage/@url}?memCmd=editContact&amp;id={nContactKey}">
            <xsl:attribute name="title">
              <!--Edit Contact-->
              <xsl:call-template name="term4024" />
            </xsl:attribute>
            <i class="fa fa-pen">
              <xsl:text> </xsl:text>
            </i>
            <xsl:text> </xsl:text>
            <!--Edit-->
            <span class="visually-hidden">
              <xsl:call-template name="term4022" />
            </span>
          </a>
          <a class="btn btn-outline-primary" href="{$currentPage/@url}?memCmd=delContact&amp;id={nContactKey}" onclick="return confirm('Are you sure you want to delete this address?');">
            <xsl:attribute name="title">
              <!--Delete Contact-->
              <xsl:call-template name="term4025" />
            </xsl:attribute>
            <i class="fa fa-trash-alt">
              <xsl:text> </xsl:text>
            </i>
            <xsl:text> </xsl:text>
            <!--Delete-->
            <span class="visually-hidden">
              <xsl:call-template name="term4023" />
            </span>
          </a>
        </div>
      </div>
    </div>
  </xsl:template>
  <!-- ##### /Membership User Contacts module ##### -->

</xsl:stylesheet>
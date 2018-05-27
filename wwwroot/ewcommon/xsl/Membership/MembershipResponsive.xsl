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
    <div class="container" id="template_Logon_Register">

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
      <!-- Terminus class fix to floating columns -->
      <p class="backlink">
        <a href="{/Page/Request/ServerVariables/Item[@name='PREVIOUS_PAGE']/node()}">
			<xsl:text>&lt;&lt;&#160;</xsl:text>
			<!--Back-->
			<xsl:call-template name="term4007"/>
		</a>
      </p>
      <div class="terminus">&#160;</div>
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
			<!-- Terminus class fix to floating columns -->
			<p class="backlink">
				<a href="{/Page/Request/ServerVariables/Item[@name='PREVIOUS_PAGE']/node()}">
					<xsl:text>&lt;&lt;&#160;</xsl:text>
					<!--Back-->
					<xsl:call-template name="term4007"/>
				</a>
			</p>
			<div class="terminus">&#160;</div>
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
			<!-- Terminus class fix to floating columns -->
			<p class="backlink">
				<a href="{/Page/Request/ServerVariables/Item[@name='PREVIOUS_PAGE']/node()}">
					<xsl:text>&lt;&lt;&#160;</xsl:text>
					<!--Back-->
					<xsl:call-template name="term4007"/>
				</a>
			</p>
			<div class="terminus">&#160;</div>
			<xsl:apply-templates select="/" mode="layoutFooter"/>
		</div>
	</xsl:template>
	
   <xsl:template match="Page[@layout='Password_Reminder']" mode="Layout">
    <div class="container" id="template_Logon_Register">
      <xsl:apply-templates select="/" mode="layoutHeader"/>
      <div id="body">
        <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and (@name='PasswordReminder' or @name='ResetAccount')]" mode="xform"/>
        <xsl:text> </xsl:text>
      </div>
     
      <!-- Terminus class fix to floating columns -->
      <div class="terminus">&#160;</div>
      <xsl:apply-templates select="/" mode="layoutFooter"/>
    </div>
  </xsl:template>

	<xsl:template match="Page[@layout='Password_Change']" mode="Layout">
		<div class="container" id="template_Logon_Register">
			<xsl:apply-templates select="/" mode="layoutHeader"/>
			<div id="body">
				<xsl:apply-templates select="/Page/Contents/Content[@type='xform' and (@name='ResetPassword')]" mode="xform"/>
        <xsl:text> </xsl:text>
			</div>
			<!-- Terminus class fix to floating columns -->
			<div class="terminus">&#160;</div>
			<xsl:apply-templates select="/" mode="layoutFooter"/>
		</div>
	</xsl:template>	
	
  <xsl:template match="Page[@layout='Account_Reset']" mode="Layout">
    <div class="container" id="template_Logon_Register">
      <xsl:apply-templates select="/" mode="layoutHeader"/>
      <div id="column1">
        <xsl:apply-templates select="/Page/Contents/Content[@type='xform' and @name='ConfirmPassword']" mode="xform"/>
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
      <!-- Terminus class fix to floating columns -->
      <div class="terminus">&#160;</div>
      <xsl:apply-templates select="/" mode="layoutFooter"/>
    </div>
  </xsl:template>


  <xsl:template match="Page[@layout='List_Quotes']" mode="Layout">
    <div class="container" id="template_List_Quotes">
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
      <!-- Terminus class fix to floating columns -->
      <div class="terminus">&#160;</div>
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
		<div class="container" id="template_List_Orders">
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
			<!-- Terminus class fix to floating columns -->
			<div class="terminus">&#160;</div>
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
		  <a href="?ewCmd=logoff" title="Logout" class="btn btn-primary">
        <i class="fa fa-sign-out">
          <xsl:text> </xsl:text>
        </i>
        <xsl:text> </xsl:text>
			  <!--Logout-->
			  <xsl:call-template name="term4019"/>
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

          <button type="submit" name="Logout" class="btn btn-primary principle" onclick="window.location.href='?ewCmd=logoff'">
            <i class="fa fa-sign-out">
              <xsl:text> </xsl:text>
            </i>
            <xsl:call-template name="term4019"/>
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
		<div class="terminus">&#160;</div>
    </div>
  </xsl:template>
  <!--   #############################################   generic login   #############################################   -->
  
  <!-- Overide for Login so title isn't a link, that way we use the footer link for password reminder. -->
  <xsl:template match="Content[@type='Module' and @moduleType='MembershipLogon']" mode="moduleLink">
        <xsl:apply-templates select="." mode="moduleTitle"/>
  </xsl:template>
  
	<xsl:template match="Content[@type='xform' and @name='UserLogon']" mode="loginBrief">
		<form method="post" action="" id="UserLogon" name="UserLogon" onsubmit="form_check(this)" xmlns="http://www.w3.org/1999/xhtml">
			<div class="username">
				<label for="ewmLogon/username">
					<!--Username-->
					<xsl:call-template name="term4000" />
				</label>
				<input type="text" name="cUserName" id="cUserName" class="textbox required" value="" onfocus="if (this.value=='Please enter username') {this.value=''}"/>
			</div>
			<div class="password">
				<label for="ewmLogon/password">
					<!--Password-->
					<xsl:call-template name="term4001" />
				</label>
				<input type="password" name="cPassword" id="cPassword" class="textbox password required"/>
			</div>
			<input name="ewmLogon/@ewCmd" class="hidden" value="membershipLogon"/>
			<div class="logon">
				<input type="submit" name="submit" value="Login" class="btn btn-primary" onclick="disableButton(this);"/>
			</div>
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
      <a class="button" href="{$currentPage/@url}?ewCmd=addContact">
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
      <xsl:if test="position() mod 2=0">
        <div class="terminus">&#160;</div>
      </xsl:if>
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
			<a class="button" href="{$currentPage/@url}?ewCmd=editContact&amp;id={nContactKey}">
				<xsl:attribute name="title">
					<!--Edit Contact-->
					<xsl:call-template name="term4024" />
				</xsl:attribute>
				<!--Edit-->
				<xsl:call-template name="term4022" />
			</a>
			<xsl:text>&#160;&#160;&#160;</xsl:text>
			<a class="btn btn-primary" href="{$currentPage/@url}?ewCmd=delContact&amp;id={nContactKey}">
				<xsl:attribute name="title">
					<!--Delete Contact-->
					<xsl:call-template name="term4025" />
				</xsl:attribute>
				<!--Delete-->
				<xsl:call-template name="term4023" />
			</a>
		</p>
      <div class="terminus">&#160;</div>
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
          <button type="submit" name="Logout" class="btn btn-primary principle" onclick="window.location.href='?ewCmd=logoff'">
            <i class="fa fa-sign-out">
              <xsl:text> </xsl:text>
            </i>
            <xsl:text> </xsl:text>
            <xsl:call-template name="term4019"/>
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
      <button type="submit" name="Logout" class="btn btn-primary principle" onclick="window.location.href='?ewCmd=logoff'">
        <i class="fa fa-sign-out">
          <xsl:text> </xsl:text>
        </i>
        <xsl:text> </xsl:text>
        <xsl:call-template name="term4019"/>
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
          <xsl:apply-templates select="//Content[@type='xform' and @name='UserLogon']" mode="xform"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="/Page/User" mode="displayUserDetails" />
        </xsl:otherwise>
      </xsl:choose>  
    </xsl:template>

    <!-- Membership Register Module -->
    <xsl:template match="Content[@type='Module' and (@moduleType='MembershipRegister')]" mode="displayBrief">
        <xsl:apply-templates select="." mode="xform"/>
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
        <div class="terminus">&#160;</div>      
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
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>


	<!-- ##### Ecommerce List Orders module ##### -->
	<xsl:template match="Content[@type='Module' and @moduleType='EcommerceListOrders']" mode="displayBrief">
		<div class="EcommerceListOrders">
      <xsl:choose>
        <xsl:when test="Order[@statusId&gt;=6]">
          <table>
            <thead>
              <th class="description">Order reference</th>
              <th class="date">Date</th>
              <th class="price">Order total</th>
              <th>&#160;</th>
            </thead>
            <tbody>
              <xsl:apply-templates select="Order[@statusId&gt;=6]" mode="orderListRow">
                <xsl:sort select="substring-after(@InvoiceRef,'-')" order="descending" data-type="number"/>
              </xsl:apply-templates>
            </tbody>
          </table>
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
		
		<tr>
			<xsl:attribute name="class">
				<xsl:text>order</xsl:text>
				<xsl:choose>
					<xsl:when test="position()=1"> first</xsl:when>
					<xsl:when test="position()=last()"> last</xsl:when>
				</xsl:choose>
			</xsl:attribute>
			
			<td class="description">
          <a href="{$href}" title="View order">
            <xsl:value-of select="@InvoiceRef"/>
          </a>
			</td>
      <td class="date">
        <xsl:if test="@InvoiceDate">
          <xsl:value-of select="@InvoiceDate"/>
        </xsl:if>
      </td>
			<td class="price">
        <xsl:call-template name="formatPrice">
          <xsl:with-param name="currency" select="$page/Cart/@currencySymbol"/>
          <xsl:with-param name="price" select="@total" />
        </xsl:call-template>
				
			</td>
			<td class="view">
				<a class="btn btn-primary button principle" href="{$href}" title="View order">View order</a>
			</td>
		</tr>
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
				<a class="btn btn-primary button principle" href="{$href}">View order</a>
			</div>
			<div class="terminus">&#160;</div>
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

		<div class="order orderdetail">
			<xsl:apply-templates select="Order" mode="orderAddresses"/>
			<xsl:apply-templates select="Order" mode="orderItems"/>
			<div class="morelink">
				<a href="{$previousURL}" class="btn btn-primary">Back to orders</a>
			</div>
			<div class="terminus">&#160;</div>
		</div>
	</xsl:template>
	<!-- ##### /Ecommerce List Orders module ##### -->

	
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
					<div class="add">
            <a class="btn btn-primary principle" href="{$currentPage/@url}?ewCmd=addContact">
              <i class="fa fa-plus">
                <xsl:text> </xsl:text>
              </i><xsl:text> </xsl:text>
              Add New Address
            </a>
					</div>
					<div class="list orders cols{@cols}">
						<xsl:apply-templates select="$page/User[1]/Contacts/Contact" mode="membershipUserContactsDisplayBrief"/>
						<div class="terminus">&#160;</div>
					</div>
				</xsl:otherwise>
			</xsl:choose>
			
		</div>
	</xsl:template>

	<xsl:template match="Contact" mode="membershipUserContactsDisplayBrief">
		<div class="listItem contact contactbrief">
      <div class="lIinner">
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
				<a class="btn btn-primary" href="{$currentPage/@url}?ewCmd=editContact&amp;id={nContactKey}">
          <xsl:attribute name="title">
						<!--Edit Contact-->
						<xsl:call-template name="term4024" />
					</xsl:attribute>
          <i class="fa fa-pencil">
            <xsl:text> </xsl:text>
          </i>
          <xsl:text> </xsl:text>
					
					<!--Edit-->
					<xsl:call-template name="term4022" />
				</a>
				<xsl:text>&#160;&#160;&#160;</xsl:text>
				<a class="btn btn-danger" href="{$currentPage/@url}?ewCmd=delContact&amp;id={nContactKey}" onclick="return confirm('Are you sure you want to delete this address?');">
					<xsl:attribute name="title">
						<!--Delete Contact-->
						<xsl:call-template name="term4025" />
					</xsl:attribute>
          <i class="fa fa-trash-o">
            <xsl:text> </xsl:text>
          </i>
          <xsl:text> </xsl:text>
					<!--Delete-->
					<xsl:call-template name="term4023" />
				</a>
			</p>
			<div class="terminus">&#160;</div>
      </div>
		</div>
	</xsl:template>
	<!-- ##### /Membership User Contacts module ##### -->
 
</xsl:stylesheet>
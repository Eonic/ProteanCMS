<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
	<xsl:variable name="currentPage" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]"/>
	<xsl:variable name="sectionPage" select="/Page/Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]"/>
	<xsl:variable name="siteURL"><xsl:call-template name="getSiteURL"/></xsl:variable>
  <!-- DATE AND TIME VARIABLES-->
  <xsl:variable name="currentYear" select="substring(/Page/Request/ServerVariables/Item[@name='Date']/node(),1,4)"/>
  <xsl:variable name="currentMonth" select="substring(/Page/Request/ServerVariables/Item[@name='Date']/node(),6,2)"/>
  <xsl:variable name="currentDay" select="substring(/Page/Request/ServerVariables/Item[@name='Date']/node(),9,2)"/>
	<!-- -->
	<xsl:template match="Page" mode="bodyDipslay">
		<div id="mainTable" class="Site">
			<div id="location">
				<xsl:apply-templates select="Menu/MenuItem" mode="breadcrumb"/>
			</div>
			<div id="mainHeader">
				<div id="headerInfo">
					<xsl:apply-templates select="Contents/Content[@type='xform' and @name='ewmLogon']" mode="xform"/>
					<xsl:apply-templates select="Cart[@type='order']/Order" mode="cartBrief"/>
					<xsl:apply-templates select="Contents/Content[@type='giftlist' and @name='cart']/Order" mode="giftlistSummary"/>
					<xsl:if test="Contents/Content[@name='telNumber']/node()">
						<h1 id="telNumber">
							<xsl:copy-of select="Contents/Content[@name='telNumber']/node()"/>
						</h1>
					</xsl:if>
				</div>
				<xsl:copy-of select="Contents/Content[@name='mainHeader' and (@type='Image' or @type='FormattedText' or @type='PlainText')]/node()"/>
			</div>
			<div id="mainMenu">
				<div id="mainMenuContainer">
					<xsl:apply-templates select="Menu/MenuItem" mode="mainmenu"/>
				</div>
			</div>
			<div id="mainLayoutContainer">
				<xsl:if test="count(Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/MenuItem) &gt; 0">
					<div id="subMenu">
						<xsl:apply-templates select="Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="submenu_topnav"/>
					</div>
				</xsl:if>
				<xsl:if test="count(Menu/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/MenuItem) &gt; 0 and Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/@name!='Our Clients'">
					<div id="subMenu2">
						<xsl:apply-templates select="Menu/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="submenu_topnav"/>
					</div>
				</xsl:if>
				<xsl:if test="count(Menu/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/MenuItem) &gt; 0 and Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/@name!='Our Clients'">
					<div id="subMenu3">
						<xsl:apply-templates select="Menu/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="submenu_topnav"/>
					</div>
				</xsl:if>
				<xsl:if test="count(Menu/MenuItem/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/MenuItem) &gt; 0 and Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/@name!='Our Clients'">
					<div id="subMenu4">
						<xsl:apply-templates select="Menu/MenuItem/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="submenu_topnav"/>
					</div>
				</xsl:if>
				<xsl:if test="count(Menu/MenuItem/MenuItem/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/MenuItem) &gt; 0 and Menu/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]/@name!='Our Clients'">
					<div id="subMenu5">
						<xsl:apply-templates select="Menu/MenuItem/MenuItem/MenuItem/MenuItem/MenuItem/MenuItem[descendant-or-self::MenuItem[@id=/Page/@id]]" mode="submenu_topnav"/>
					</div>
				</xsl:if>
				<div id="mainTitle" class="terminus">
					<h1>
						<xsl:choose>
							<xsl:when test="Contents/Content[@name='title']">
								<xsl:copy-of select="Contents/Content[@name='title']/node()"/>
							</xsl:when>
							<xsl:otherwise>
								<xsl:choose>
									<xsl:when test="Menu/descendant-or-self::MenuItem[@id=/Page/@id]/DisplayName/node()!=''">
										<xsl:value-of select="Menu/descendant-or-self::MenuItem[@id=/Page/@id]/DisplayName"/>
									</xsl:when>
									<xsl:otherwise>
										<xsl:value-of select="Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@name"/>
									</xsl:otherwise>
								</xsl:choose>
							</xsl:otherwise>
						</xsl:choose>
					</h1>
				</div>
				<div id="mainLayout">
					<xsl:apply-templates select="." mode="mainLayout"/>
				</div>
			</div>
			<div id="mainFooter" class="terminus">
				<xsl:copy-of select="Contents/Content[@name='Copyright' and (@type='Image' or @type='FormattedText' or @type='PlainText')]/node()"/>
			</div>
			<xsl:apply-templates select="/" mode="developerLink"/>
		</div>
	</xsl:template>
	<!-- -->
  <!--   #############################################   ALL FORMS IN BOXES   #############################################   -->
  <xsl:template match="group[not(ancestor::Page/@adminMode='true') and not(ancestor::group)]" mode="xform">
    <tr>
      <td colspan="2" class="group {@class} box">
        <xsl:apply-templates select="." mode="editXformMenu"/>
        <xsl:if test="label">
          <div class="tl">
            <div class="tr">
              <h2 class="title">
                <xsl:value-of select="label/node()"/>
              </h2>
            </div>
          </div>
        </xsl:if>
        <table cellspacing="0" class="content">
          <xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | group | hint | help | alert | div | repeat" mode="xform"/>
          <xsl:if test="count(submit) &gt; 0">
            <xsl:if test="descendant-or-self::*[contains(@class,'required')]">
              <tr>
                <td colspan="2">
                  <span class="hint">
                    <strong>*</strong> denotes a required field
                  </span>
                </td>
              </tr>
            </xsl:if>
            <tr>
              <td colspan="2" class="buttons">
                <!-- For xFormQuiz change how these buttons work -->
                <xsl:apply-templates select="submit" mode="xform"/>
              </td>
            </tr>
          </xsl:if>
          <tr>
            <td colspan="2" class="buttons">
              <div class="bl">
                <div class="br">&#160;</div>
              </div>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </xsl:template>
  <!--   ############################################# / ALL FORMS IN BOXES   #############################################   -->
</xsl:stylesheet>
<?xml version="1.0" encoding="UTF-8" ?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:import href="../Tools/Functions.xsl"/>

  <xsl:variable name="siteURL">

    <xsl:variable name="serverVariableURL">
      <xsl:call-template name="getServerVariable">
        <xsl:with-param name="valueName" select="'HTTP_HOST'"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="cartUrl">
      <xsl:call-template name="getSettings">
        <xsl:with-param name="sectionName" select="'cart'"/>
        <xsl:with-param name="valueName" select="'SiteURL'"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:choose>
      <xsl:when test="$cartUrl!=''">
        <xsl:value-of select="$cartUrl"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>http://</xsl:text>
        <xsl:value-of select="$serverVariableURL"/>
      </xsl:otherwise>
    </xsl:choose>
    <!--<xsl:text>http://vicsdemo.proteancms.com</xsl:text>-->

  </xsl:variable>

  <xsl:variable name="siteTitle">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'SiteName'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name="SiteLogo">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'SiteLogo'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name="CompanyName">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'CompanyName'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name="CompanyAddress">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'CompanyAddress'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name="CompanyTel">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'CompanyTel'"/>
    </xsl:call-template>
  </xsl:variable>

  <xsl:variable name="CompanyEmail">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'CompanyEmail'"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="VATnumber">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'VATnumber'"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="CompanyRegNo">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'CompanyRegNo'"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:variable name="CharityRegNo">
    <xsl:call-template name="getSettings">
      <xsl:with-param name="sectionName" select="'web'"/>
      <xsl:with-param name="valueName" select="'CharityRegNo'"/>
    </xsl:call-template>
  </xsl:variable>
  <xsl:template match="*">
    <html>
      <head>
        <title>
          <xsl:apply-templates select="." mode="subject"/>
        </title>
        <xsl:apply-templates select="." mode="emailStyle"/>
        <xsl:if test="/Page/@baseUrl">
          <base href="{/Page/@baseUrl}"/>
        </xsl:if>
        <xsl:if test="/Page/@adminMode">
          <xsl:apply-templates select="." mode="commonStyle"/>
          <xsl:apply-templates select="." mode="js"/>
        </xsl:if>
      </head>
      <xsl:apply-templates select="." mode="emailBody"/>
    </html>
  </xsl:template>
  <xsl:template match="*" mode="pageTitle">
    <xsl:apply-templates select="." mode="subject"/>
  </xsl:template>
  <!--STYLE VARIABLES-->
  <xsl:variable name="hPadding">10</xsl:variable>
  <xsl:variable name="boxMargin">25</xsl:variable>
  <xsl:variable name="emailWidth">620</xsl:variable>
  <xsl:variable name="mainColour">#1ba5d8</xsl:variable>

  <xsl:template match="Content" mode="getThWidth">300</xsl:template>
  <xsl:template match="Content" mode="getThHeight">300</xsl:template>

  <xsl:template match="*" mode="emailStyle">
    <style>
      /*html,
      body{
      margin: 0 auto !important;
      padding: 0 !important;
      height: 100% !important;
      width: 100% !important;
      }*/
      .Mail{margin-top:0;}
      .Mail p{padding:0px; margin-top:0px;}
      .Mail table{margin:0;}

      /*GENERAL STYLES*/
      .emailContentWrapper{padding:20px 0 0;}
      .Mail,
      #emailContent{background:#ECECEC}
      .Mail a{color:<xsl:value-of select="$mainColour"/>;text-decoration:none}
      .Mail td,
      .Mail th{
      font-family:Arial, sans-serif;
      line-height:20px;
      font-size:15px;
      color:#666;
      }
      .emailPaddingBottom{padding-bottom:10px;}

      /*FORM RESULTS*/
      .emailContentFooter{
      font-size:11px!important;
      padding:10px <xsl:value-of select="$hPadding"/>px;
      }
      .emailLabel{
      text-align:left;
      padding-right:10px;
      vertical-align: top;
      }
      .emailAnswer{
      padding-bottom:10px;
      vertical-align: top;
      }

      /*HEADINGS*/
      .Mail h1{
      font-size: 26px;
      line-height: 27px;
      margin:0;
      }
      .Mail h2{
      font-size:18px;
      margin:0;
      }
      .Mail h3.title{margin:0 0 15px;}

      /*BUTTONS*/
      .emailBtnTable{
      margin-top:10px;
      }
      .emailBtn, .moreLinkBtn{
      background:<xsl:value-of select="$mainColour"/>;
      padding:5px 15px;
      border-radius:100px;
      }
      .emailBtn a, .moreLinkBtn a{
      color:#fff;
      text-decoration:none;
      }

      /*BOXES*/
      .Default-Box .emailBoxHeader{
      background:<xsl:value-of select="$mainColour"/>;
      color:#fff;
      padding:20px 20px 0;
      }
      .emailBoxHeader h2{margin:0;}
      .Default-Box .emailBoxContent{
      background:<xsl:value-of select="$mainColour"/>;
      color:#fff;
      padding:20px;
      }
      .Default-Box .emailBoxContent .emailListSummary{
      color:#fff;
      }
      .Default-Box .emailBoxFooter{
      background:<xsl:value-of select="$mainColour"/>;
      color:#fff;
      padding:0 20px 20px;
      }
      .Default-Box a{
      color:#a9fbff;
      text-decoration:none;
      }
      .Default-Box .moreLinkBtn,
      .Default-Box .emailBtn{
      background:#000;
      border-radius:100px;
      padding:5px 15px;
      }
      .Default-Box .moreLinkBtn a{
      color:#fff;
      text-decoration:none;
      }

      /*HEADER*/
      #emailHeader{
      margin-bottom:0;
      border-bottom:4px solid <xsl:value-of select="$mainColour"/>;
      }
      #emailHeader td{
      background:#ffffff;
      }
      #siteLogo{
      max-width:200px;
      }
      #siteLogo img{
      max-width:200px;
      display:block;
      }

      /*FOOTER*/
      .emailFooterWrapper{padding:15px 0;}
      #emailFooter{
      background:#000000;
      }
      #emailFooter td{
      color:#999999;
      font-size:11px;
      }
      #emailFooter a{
      color:#ffffff;
      text-decoration:none;
      }

      /*LIST MODULES*/
      .emailListImage1{
      width:40%;
      padding-right:20px;
      vertical-align:top;
      }
      .emailListImage{
      padding-bottom:20px;
      }
      .emailContentPadding,
      .emailModuleHeadingPadding{
      padding-left:<xsl:value-of select="$hPadding"/>px;
      padding-right:<xsl:value-of select="$hPadding"/>px;
      }
      .emailModulePadding{
      padding:0 <xsl:value-of select="$hPadding"/>px <xsl:value-of select="$boxMargin"/>px;
      }
      .emailBoxContent .emailModulePadding{
      padding:0 0px <xsl:value-of select="$boxMargin"/>px;
      }

      /*IMAGES*/
      .Mail img{
      max-width:100%;
      height:auto;
      }
      .Mail .thumbnail{border:0;padding:0;border-radius:0;}

      /*RESPONSIVE*/
      @media only screen and (max-width: 720px){
      .emailCol,
      .emailLabel,
      .emailAnswer{
      display:block !important;
      width:100% !important;
      }
      .emailListImage1{
      display:block !important;
      width:100% !important;
      vertical-align:top;
      padding:0 0 20px;
      }
      .emailListSummary{
      display:block !important;
      width:100% !important;
      }
      }
    </style>
  </xsl:template>


  <xsl:template match="*" mode="emailBody">
    <xsl:if test="@adminMode='false'">
      <xsl:attribute name="class">normalMode</xsl:attribute>
    </xsl:if>
    <table width="100%" style="margin-bottom:0;" cellpadding="0" cellspacing="0" id="mainTable" class="Mail">
      <tr>
        <td class="MailTD">
          <table width="100%" cellpadding="0" cellspacing="0" id="emailHeader">
            <tr>
              <td>
                <center>
                  <xsl:comment>
                    <xsl:text>[if mso]&gt;
            &lt;table width="620" cellpadding="0" cellspacing="0"&gt;
              &lt;tr&gt;
                &lt;td&gt;
                  &lt;![endif]</xsl:text>
                  </xsl:comment>
                  <table cellpadding="0" cellspacing="0" style="width:100%;max-width:{$emailWidth}px;margin:0 auto;" class="emailWidthContainer">
                    <tr>
                      <td id="siteLogo" style="padding:10px {$hPadding}px">
                        <xsl:choose>
                          <xsl:when test="$SiteLogo!=''">
                            <a href="{$siteURL}" title="{$siteTitle}">
                              <img src="{$siteURL}/{$SiteLogo}" alt="{$siteTitle}"/>
                            </a>
                          </xsl:when>
                          <xsl:otherwise>
                            <a href="{$siteURL}" title="{$siteTitle}">
                              <h2>
                                <xsl:value-of select="$siteTitle"/>
                              </h2>
                            </a>
                          </xsl:otherwise>
                        </xsl:choose>
                      </td>
                    </tr>
                  </table>
                  <xsl:comment>
                    <xsl:text>[if mso]&gt;
          &lt;/td&gt;
            &lt;/tr&gt;
          &lt;/table&gt;
          &lt;![endif]</xsl:text>
                  </xsl:comment>
          </center>
        </td>
            </tr>
          </table>
          <table width="100%" style="margin-bottom:0;" cellpadding="0" cellspacing="0">
            <tr>
              <td style="padding:0;border:0;" id="emailContent">
                <center>
                  <xsl:comment>
                    <xsl:text>[if mso]&gt;
                    &lt;table width="620" cellpadding="0" cellspacing="0"&gt;
                    &lt;tr&gt;
                    &lt;td&gt;
                    &lt;![endif]</xsl:text>
                  </xsl:comment>
                  <table cellpadding="0" cellspacing="0" style="width:100%;max-width:{$emailWidth}px;margin:0 auto;" class="emailWidthContainer">
                    <tr>
                      <td class="emailContentWrapper">
                        <table cellspacing="0" cellpadding="0">
                          <tr>
                            <td class="emailModuleHeadingPadding emailPaddingBottom">
                              <h1>
                                <xsl:apply-templates select="." mode="pageTitle"/>
                              </h1>
                            </td>
                          </tr>
                          <tr>
                            <td>
                              <xsl:apply-templates select="." mode="bodyLayout"/>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>
                  </table>
                  <xsl:comment>
                    <xsl:text>[if mso]&gt;
                    &lt;/td&gt;
                    &lt;/tr&gt;
                    &lt;/table&gt;
                    &lt;![endif]</xsl:text>
                  </xsl:comment>
                </center>
              </td>
            </tr>
          </table>
          <table id="emailFooter" width="100%" cellpadding="0" cellspacing="0">
            <tr>
              <td class="emailFooterWrapper">
                <center>
                  <xsl:comment>
                    <xsl:text>[if mso]&gt;
                    &lt;table width="620" cellpadding="0" cellspacing="0"&gt;
                    &lt;tr&gt;
                    &lt;td&gt;
                    &lt;![endif]</xsl:text>
                  </xsl:comment>
                  <table cellpadding="0" cellspacing="0" style="width:100%;max-width:{$emailWidth}px;margin:0 auto;" class="emailWidthContainer">
                    <tr>
                      <td style="padding:0px {$hPadding}px" width="50%" valign="top" class="emailCol">
                        <xsl:if test="$CompanyTel!=''">
                          <strong>Tel: </strong><a href="tel:{$CompanyTel}">
                            <xsl:value-of select="$CompanyTel"/>
                          </a>&#160;<br/>
                        </xsl:if>
                        <xsl:if test="$CompanyEmail!=''">
                          <strong>Email: </strong><a href="mailto:{$CompanyEmail}">
                            <xsl:value-of select="$CompanyEmail"/>
                          </a>&#160;<br/>
                        </xsl:if>
                        <xsl:if test="$siteURL!=''">
                          <strong>Website: </strong><a href="{$siteURL}">
                            <xsl:value-of select="$siteURL"/>
                          </a>&#160;
                        </xsl:if>
                      </td>
                      <td style="padding:0px {$hPadding}px" valign="top" class="emailCol">
                        <xsl:if test="$CompanyAddress!=''">
                          <xsl:value-of select="$CompanyAddress"/>
                        </xsl:if>
                        <xsl:if test="$CompanyRegNo!=''">
                          <br/>
                          <strong>Registered in UK: </strong>
                          <xsl:value-of select="$CompanyRegNo"/>
                        </xsl:if>
                        <xsl:if test="$VATnumber!=''">
                          <br/>
                          <strong>VAT Number: </strong>
                          <xsl:value-of select="$VATnumber"/>
                        </xsl:if>
                        <br/>
                        <unsubscribe>Unsubscribe</unsubscribe>
                      </td>
                    </tr>
                  </table>
                  <xsl:comment>
                    <xsl:text>[if mso]&gt;
                    &lt;/td&gt;
                    &lt;/tr&gt;
                    &lt;/table&gt;
                    &lt;![endif]</xsl:text>
                  </xsl:comment>
                </center>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
            
  </xsl:template>
</xsl:stylesheet>
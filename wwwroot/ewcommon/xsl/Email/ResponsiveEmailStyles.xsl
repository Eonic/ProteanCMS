<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
  <!--STYLE VARIABLES-->
  <xsl:variable name="hPadding">10</xsl:variable>
  <xsl:variable name="boxMargin">25</xsl:variable>
  <xsl:variable name="emailWidth">620</xsl:variable>
  <xsl:variable name="mainColour">#1ba5d8</xsl:variable>
  <xsl:variable name="highlightColour">#ef890a</xsl:variable>

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
      .cleanLink a,
      a .cleanLink{
      color:<xsl:value-of select="$mainColour"/>!important;
      text-decoration:none!important;
      }
      .Mail td,
      .Mail th{
      font-family:Arial, sans-serif;
      line-height:20px;
      font-size:15px;
      color:#666;
      }
      .emailPaddingBottom{padding-bottom:10px;}

      /*HEADINGS*/
      .Mail h1, .Mail h2, .Mail h3, .Mail h4, .Mail h5, .Mail h6{
      margin:0;
      font-weight:bold;
      }
      .Mail h1{
      font-size: 26px;
      line-height: 27px;
      margin:0;
      }
      .Mail h2{
      font-size:18px;
      margin:0;
      }
      .Mail h2.title{
      font-size:22px;
      padding:0 0 15px;
      }
      .Mail h3.title{
      padding:0 0 15px;
      font-size:18px;
      }

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
      .Mail .Default-Box h1, .Mail .Default-Box h2, .Mail .Default-Box h3, .Mail .Default-Box h4, .Mail .Default-Box h5, .Mail .Default-Box h6{
      color:#fff;
      }
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
      #siteLogo img{
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

      /*FORMS AND CART*/
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
      .emailQuantity,
      .emailProduct,
      .emailLineTotal{
      border-bottom:1px solid #cccccc;
      padding:5px 0;
      }
      .emailCartContent{border-top:1px solid #cccccc;}
      .emailQuantity,
      .emailProduct{
      vertical-align:top;}
      .emailQuantity{
      width: 25px;
      white-space:nowrap!important;
      }
      .emailRef,
      .Mail .emailSmall{font-size:11px;}
      .Mail .emailLineTotal{
      color:<xsl:value-of select="$mainColour"/>;
      font-weight:bold;
      }
      .emailCartTotals{padding-top:10px;}

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
</xsl:stylesheet>

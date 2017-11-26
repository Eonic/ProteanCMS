<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:variable name="siteURL">http://elitemodelsv5.prod.eonic.co.uk</xsl:variable>
  <xsl:template match="*">
    <html>
      <head>
        <title>
          <xsl:apply-templates select="." mode="subject"/>
        </title>
        <style>
          h1
          {margin:5px;margin-left:0;margin-right:0;font-size:1.4em;}
          h2
          {margin:5px;margin-left:0;margin-right:0;font-size:1.3em;}
          h3
          {margin:5px;margin-left:0;margin-right:0;font-size:1.2em;}
          h4
          {margin:5px;margin-left:0;margin-right:0;font-size:1.1em;}
          h5
          {margin:5px;margin-left:0;margin-right:0;font-size:1em;}
          h6
          {margin:5px;margin-left:0;margin-right:0;font-size:0.9em;}
          img
          {border:none;}
          p
          {margin:0 0 1em;}
          td, th
          {line-height:1.7;}
          #msgBody td,
          #msgBody th
          {text-align: left;vertical-align:top !important;line-height:1.2;}
          .alternative th
          {text-align:left;}
          td.price,td.lineTotal,td.amount,td.heading,th.price,th.lineTotal
          {text-align:right !important;}
          td.total
          {background:#F46E30;color:#FFF;font-weight:bold;}
          {text-decoration:underline;}
          #mainTable a{color:#0093D0;font-weight:bold;text-decoration:none;}
          #mainTable a:hover{color:#EE6603 !important}
          #mainTable a:hover img{border:1px solid #E25919 !important}
          #footer a
          {color: #FFFFFF !important;text-decoration:none;}
          .productContainer h4{font-size:12px}
          a.devLink{color:#BFCCD2 !important;text-decoration:none !important;font-weight:normal;}
          a.devLink:hover{color:#EE6603 !important}
          hr{border:1px solid #5C85B7;}
          img.imageFloatRight{float:right; !important}
          img.imageFloatLeft{float:left; !important}
          p {font-size: 11px;}
          #cartListing{border-right:1px solid #474747;}
          #cartListing td.cell,
          #cartListing td.heading
          {border-left:1px solid #474747;border-bottom:1px solid #474747;}
          td.price,td.lineTotal,td.amount,td.heading,th.price,th.lineTotal
          {text-align:right;}
          td.total,#cartListing th
          {background:#474747 url(/images/layout/cartListing_th.gif) top left repeat-x !important;color:#FFF;font-weight:bold;}
          #cartListing th{border: 1px solid #474747;border-right:0;}
          #cartListing td.vat,
          #cartListing td.subTotal,
          #cartListing td.shipping
          {border-left:1px solid #474747;border-bottom:1px solid #474747;background:#eeeeee;}
        </style>
      </head>
      <body bgcolor="#000709" style="font-family:Verdana,Trebuchet MS,Lusidia Sans Unicode,sans-serif;color:#000;font-size:11px;">
        <center>
          <table id="mainTable" cellpadding="0" cellspacing="0" width="650" style="width:650px !important">
            <tr>
              <td id="header" width="650" height="87" align="left" style="text-align:left;">
                <a href="{$siteURL}" title="Elite Models">
                  <img src="{$siteURL}/images/layout/emailStationary/header.gif" width="352" height="87" alt="WEBSITE NAME"  style="border:0 !important;"/>
                </a>
              </td>
            </tr>
            <tr>
              <td id="header2" width="650" height="31" align="left" style="text-align:left;background: #0093D0 url({$siteURL}/images/layout/emailStationary/title_bg.gif) top left no-repeat; padding-left: 10px;">
                <font face="verdana" size="2" color="#FFFFFF">
                  <xsl:apply-templates select="." mode="subject"/>
                </font>
              </td>
            </tr>
            <tr>
              <td id="msgBody" width="650" style="border:1px solid #FFF;background:#FFF">
                <xsl:apply-templates select="." mode="bodyLayout"/>
              </td>
            </tr>
            <tr>
              <td id="footer" width="650" align="left" height="31" style="text-align:left;background: #0093D0 url({$siteURL}/images/layout/emailStationary/footer_bg.gif) left bottom no-repeat; padding-left: 10px;">
                <font face="verdana" size="2" color="#FFFFFF">
                  01234 567890 | <a href="mailto:sales@blah.com" style="color:#FFF;text-decoration:none;font-weight:bold;">SALES</a>
                </font>
              </td>
            </tr>
            <tr>
              <td align="center" style="padding:10px;">
                <font align="center" face="verdana" size="1">
                  <p style="text-align:center;color:#B5C5DA">BLAH BLAH BLAH</p>
                </font>
                <font align="center" face="verdana" size="0.8">
                  <p style="text-align:center;color:#BFCCD2;">
                    <a class="devLink" href="http://www.eonic.co.uk/" title="Generated By EonicWeb4" target="new" style="color:#515151;font-size:11px;text-decoration:none;">generated by eonic<strong>web</strong>4</a>
                  </p>
                </font>
              </td>
            </tr>
          </table>
        </center>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>
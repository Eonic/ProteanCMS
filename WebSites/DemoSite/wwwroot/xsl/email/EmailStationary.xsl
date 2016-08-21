<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:variable name="siteURL">http://demov4.prod.eonic.co.uk</xsl:variable>
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
          #mainTable a{color:#000000;font-weight:bold;text-decoration:none;}
          #mainTable a:hover{color:##696969 !important}
          #mainTable a:hover img{border:1px solid #E25919 !important}
          #footer a
          {color: #ffffff !important;text-decoration:none;font-weight:bold;}
          .productContainer h4{font-size:12px}
          a.devLink{color:#ffffff !important;text-decoration:none !important;font-weight:normal !important;}
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
          {background:#a3a3a3 url(/images/layout/cartListing_th.gif) top left repeat-x !important;color:#FFF;font-weight:bold;}
          #cartListing th{border: 1px solid #474747;border-right:0;}
          #cartListing td.vat,
          #cartListing td.subTotal,
          #cartListing td.shipping
          {border-left:1px solid #474747;border-bottom:1px solid #474747;background:#eeeeee;}
        </style>
      </head>

      <body bgcolor="#ffffff" style="font-family:Arial,Trebuchet MS,Verdana,Lusidia Sans Unicode,sans-serif;color:#333;font-size:11px;">
        <center>
          <table height="100%" width="100%" cellpadding="20" cellspacing="0" border="0">
            <tr>
              <td style="background-color:#6c6c6c !important;" align="middle" bgcolor="#6c6c6c" vAlign="top" width="100%">
                  <table id="mainTable" cellpadding="0" cellspacing="0" width="650" style="width:650px !important">
                  <tr height="120">
                    <td id="header" width="650" height="87" align="left" style="text-align:left;background-color: #333333;">
                      <a href="{$siteURL}" title="Eonic">
                        <img src="{$siteURL}/images/layout/emailStationary/header.jpg" width="650" height="87" alt="Eonic"  style="border:0 !important;"/>
                      </a>
                    </td>
                  </tr>
                  <tr>
                    <td id="header2" width="650" height="32" align="left" style="text-align:left;background-color: #ffffff;;padding-left: 10px;">
                      <font face="arial" size="3" color="#000000" style="font-weight:bold;">
                        <xsl:apply-templates select="." mode="subject"/>
                      </font>
                    </td>
                  </tr>
                  <tr>
                    <td id="msgBody" width="650" style="background:#FFF">
                      <xsl:apply-templates select="." mode="bodyLayout"/>
                    </td>
                  </tr>
                  <tr>
                    <td>
                      <table cellpadding="0" cellspacing="0" border="0" style="background-color: #333333;">
                        <tr>
                          <td id="footer" width="490" align="left" height="98" style="font-family:Arial;font-size:7pt;text-align:left;color:#ffffff;background-color: #333333; padding-left: 10px;">
                            Tel:: 08708 361 755 &#160;&#160;&#160; Email:: <a style="color:#ffffff;font-weight:bold;" href="mailto:info@eonic.co.uk">info@eonic.co.uk</a>&#160;&#160;&#160; Web::
                            <a style="color:#ffffff;font-weight:bold;"  href="www.eonic.co.uk">www.eonic.co.uk</a>
                            <br/>Eonic Ltd | 43 High Street | Tunbridge Wells | Kent | TN1 1XL<br/><br/>
                            Registered in the UK: 031 610 57<br/>
                            <br/>Would you like email stationery like ours? Give us a call.
                          </td>
                          <td width="160" height="98">
                            <img src="{$siteURL}/images/layout/emailStationary/footer.jpg" width="160" height="98" alt="Eonic"  style="border:0 !important;"/>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>
                  <tr>
                    <td align="center" style="padding:10px;">
                      <font align="center" face="verdana" size="0.8">
                        <p style="text-align:center;color:#BFCCD2;">
                          <a class="devLink" href="http://www.eonic.co.uk/" title="Generated By EonicWeb4" target="new" style="color:#333333;font-size:11px;text-decoration:none;font-weight:normal !important;">
                            generated by eonic<strong>web</strong>4
                          </a>
                        </p>
                      </font>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </center>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>
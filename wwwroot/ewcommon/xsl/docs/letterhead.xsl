<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
  <xsl:output method="xml" indent="yes" omit-xml-declaration="no" encoding="UTF-8"/>
  <xsl:variable name="filePath" select="'D:\HostingSpaces\tbc\wwwroot\'"/>
  <xsl:variable name="headingfont" select="'Helvetica'"/>
  <xsl:variable name="headingcolor" select="'#00ADEE'"/>
  <xsl:variable name="bodyfont" select="'Helvetica'"/>
  <xsl:variable name="page" select="."/>
  
  <xsl:template match="*">

    <fo:root xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <fo:layout-master-set>
        <fo:simple-page-master master-name="simple"
                      page-height="29.7cm"
                      page-width="21cm"
                      margin-top="1.0cm"
                      margin-bottom="0cm"
                      margin-left="0.4cm"
                      margin-right="0cm">
          <fo:region-body margin-top="0cm"/>
          <fo:region-before extent="0cm"/>
          <fo:region-after extent="0cm"/>
        </fo:simple-page-master>
      </fo:layout-master-set>

      <fo:page-sequence master-reference="simple">
        <xsl:apply-templates select="descendant-or-self::*[name()='Policy'][1]" mode="PageTitle"/>
        <fo:flow flow-name="xsl-region-body">
          <fo:block-container position="absolute" top="0cm" right="0cm" left="0.5cm" height="10.7cm" width="19cm">
            <fo:block>
              <fo:external-graphic src="{$filePath}/images/pdf/docheader.jpg"></fo:external-graphic>
            </fo:block>
          </fo:block-container>

          <xsl:apply-templates select="descendant-or-self::*[name()='Order'][1]" mode="PageBody"/>
          

          <fo:block-container position="absolute" top="24cm" right="0cm" left="0cm" height="3.66cm" width="20cm" border-top-color="#000000" border-top-style="solid" border-top-width="0.2mm">
            <fo:block-container margin-left="5mm" height="3.66cm" width="20cm">
             <fo:block font-size="13pt" text-align="left" font-family="{$headingfont}" color="#000000" linefeed-treatment="preserve" space-after="2mm" padding-top="2mm">
        
            </fo:block>
              <fo:block font-size="12pt" text-align="left" font-family="{$bodyfont}" color="#000000" linefeed-treatment="preserve" space-after="2mm" padding-top="2mm">
                Contact Details
              </fo:block>
            <fo:block font-size="10pt" text-align="left" font-family="{$bodyfont}" color="#000000" linefeed-treatment="preserve" padding-left="5mm">
              tel: 0
            </fo:block>
            </fo:block-container>
          </fo:block-container>
          <fo:block-container position="absolute" top="26.5cm" right="0cm" left="0.5cm" height="3.66cm" width="19cm">
            <fo:block font-size="8pt" text-align="left" font-family="{$bodyfont}" color="#000000" linefeed-treatment="preserve">
              <fo:block></fo:block>
            </fo:block>
          </fo:block-container>
        </fo:flow>
      </fo:page-sequence>
    </fo:root>
  </xsl:template>

  <xsl:template match="*" mode="PageBody">
    <fo:block-container xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <fo:block-container position="absolute" top="{position() * 3}.5cm" right="0cm" left="0.5cm" height="2cm" width="19cm">
        <fo:block font-size="16pt" text-align="left" font-family="{$headingfont}" font-weight="bold" color="{$headingcolor}">
          PageBody Not Found for - <xsl:value-of select="name()"/>
        </fo:block>
      </fo:block-container>
    </fo:block-container>
  </xsl:template>

  <xsl:template match="*" mode="PageTitle">
    <fo:title xmlns:fo="http://www.w3.org/1999/XSL/Format">Document</fo:title>
  </xsl:template>

    <xsl:template match="*" mode="transformXhtml">
    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="transformXhtml"/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="p" mode="transformXhtml">
    <fo:block padding-bottom="6pt" xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:apply-templates mode="transformXhtml"/>
    </fo:block>
  </xsl:template>

  <xsl:template match="span" mode="transformXhtml">
    <xsl:apply-templates mode="transformXhtml"/>
  </xsl:template>

  <xsl:template match="strong" mode="transformXhtml">
    <fo:inline font-weight="bold" xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:apply-templates mode="transformXhtml"/>
    </fo:inline>
  </xsl:template>

  <xsl:template match="b" mode="transformXhtml">
    <fo:inline font-weight="bold" xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:apply-templates mode="transformXhtml"/>
    </fo:inline>
  </xsl:template>

  <xsl:template match="i" mode="transformXhtml">
    <fo:inline font-style="italic" xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:apply-templates mode="transformXhtml"/>
    </fo:inline>
  </xsl:template>

  <xsl:template match="em" mode="transformXhtml">
    <fo:inline font-style="italic" xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:apply-templates mode="transformXhtml"/>
    </fo:inline>
  </xsl:template>

  <xsl:template match="sup" mode="transformXhtml">
    <fo:inline vertical-align="super" font-size="8pt" xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:apply-templates/>
    </fo:inline>
  </xsl:template>

  <xsl:template match="sub" mode="transformXhtml">
    <fo:inline vertical-align="sub" font-size="8pt" xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:apply-templates/>
    </fo:inline>
  </xsl:template>

  <xsl:template match="br" mode="transformXhtml">
    <fo:block xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:text> </xsl:text>
    </fo:block>
  </xsl:template>

  <xsl:template match="a" mode="transformXhtml">

  </xsl:template>

  <xsl:template match="img" mode="transformXhtml">
    <fo:block xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <fo:external-graphic src="{$filePath}{@src}"></fo:external-graphic>
      <xsl:text> </xsl:text>
    </fo:block>
  </xsl:template>

  <xsl:template name="formatdate">
    <xsl:param name="date"/>
    <xsl:param name="format"/>
    <xsl:value-of select="ew:formatdate($date,$format)"/>
  </xsl:template>

  <xsl:template match="ul" mode="transformXhtml">
    <fo:list-block xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:apply-templates mode="transformXhtml"/>
    </fo:list-block>
  </xsl:template>

  <xsl:template match="li" mode="transformXhtml">
    <fo:list-item xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <fo:list-item-label xmlns:fo="http://www.w3.org/1999/XSL/Format">
        <fo:block xmlns:fo="http://www.w3.org/1999/XSL/Format">&#x2022;</fo:block>
      </fo:list-item-label>
      <fo:list-item-body padding-left="15pt" xmlns:fo="http://www.w3.org/1999/XSL/Format">
        <fo:block  xmlns:fo="http://www.w3.org/1999/XSL/Format">
          &#160;&#160;&#160;
          <xsl:apply-templates mode="transformXhtml"/>
        </fo:block>
      </fo:list-item-body>
    </fo:list-item>
  </xsl:template>

</xsl:stylesheet>
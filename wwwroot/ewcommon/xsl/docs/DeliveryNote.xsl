<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <xsl:import href="../../../../../ewcommon_v5-1/xsl/tools/Functions.xsl"/>
  <xsl:import href="letterhead.xsl"/>
  
  <xsl:output method="xml" indent="yes" omit-xml-declaration="no" encoding="UTF-8"/>

  <xsl:template match="Policy" mode="PageTitle">
    <fo:title xmlns:fo="http://www.w3.org/1999/XSL/Format">
      <xsl:text>DeliveryNote-</xsl:text>
      <xsl:value-of select="$page/descendant-or-self::Order[1]/@cartId"/>
    </fo:title>
  </xsl:template>

  <xsl:template match="Order" mode="PageBody">
    <xsl:variable name="policyId">
      <xsl:choose>
        <xsl:when test="$page/descendant-or-self::Subscription[1]">
          <xsl:value-of select="$page/descendant-or-self::Subscription[1]/@orderId"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$page/descendant-or-self::Order[1]/@cartId"/>
        </xsl:otherwise>
      </xsl:choose>


    </xsl:variable>
    <fo:block-container xmlns:fo="http://www.w3.org/1999/XSL/Format">

      <fo:block-container position="absolute" top="2.5cm" right="0cm" left="0.5cm" height="2cm" width="19cm">
        <fo:block font-size="16pt" text-align="left" font-family="{$headingfont}" font-weight="bold" color="{$headingcolor}">
          Confirmation of Insurance
        </fo:block>
      </fo:block-container>
      <fo:block-container position="absolute" top="3.5cm" right="0cm" left="0.5cm" height="2cm" width="19cm">
        <fo:block font-size="12pt" font-family="{$headingfont}">
        Policy Reference No. <xsl:value-of select="$policyId"/> 
        </fo:block>
      </fo:block-container>
      <fo:block-container position="absolute" top="4.5cm" right="0cm" left="0.5cm" height="2cm" width="19cm">
        <fo:block font-size="10pt" font-family="{$headingfont}">
          We confirm that your household goods and personal effects are insured with Royal &amp; Sun Alliance under Master Policy T002314N. This insurance is in accordance with the Policy Terms and Conditions.
        </fo:block>
      </fo:block-container>
      <fo:block-container position="absolute" top="5.5cm" right="0cm" left="0.5cm" height="20cm" width="19cm">
        <fo:block font-size="10pt" font-family="{$headingfont}">
          <fo:table table-layout="fixed">
            <fo:table-column column-width="5cm"/>
            <fo:table-column column-width="14cm"/>
            <fo:table-body>
              <fo:table-row border-bottom-color="#000000">
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block text-align="right" font-weight="bold" color="#000000">Assured:</fo:block>
                </fo:table-cell>
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block>
                    <xsl:choose>
                      <xsl:when test="$page/descendant-or-self::User[1]">
                        <!--Case for Admin-->
                        <xsl:value-of select="$page/descendant-or-self::Subscription[1]/User/FirstName/node()"/>&#160;
                        <xsl:value-of select="$page/descendant-or-self::Subscription[1]/User/LastName/node()"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <!--Case for Cart-->
                        <xsl:value-of select="../../../Contact[@type='Billing Address']/GivenName/node()"/>
                      </xsl:otherwise>
                    </xsl:choose>

                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <fo:table-row border-bottom-color="#000000" border-bottom-width=".25cm" border-bottom-style="solid">
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block text-align="right" font-weight="bold" color="#000000">Storage Facility:</fo:block>
                </fo:table-cell>
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block>
                    <xsl:value-of select="Schedule/StorageFacility/PostalAddress/Name/node()"/>
                  </fo:block>
                  <fo:block>
                    <xsl:value-of select="Schedule/StorageFacility/PostalAddress/Address1/node()"/>
                  </fo:block>
                  <fo:block>
                    <xsl:value-of select="Schedule/StorageFacility/PostalAddress/Address2/node()"/>
                  </fo:block>
                  <fo:block>
                    <xsl:value-of select="Schedule/StorageFacility/PostalAddress/Address3/node()"/>
                  </fo:block>
                  <fo:block>
                    <xsl:value-of select="Schedule/StorageFacility/PostalAddress/City/node()"/>
                  </fo:block>
                  <fo:block>
                    <xsl:value-of select="Schedule/StorageFacility/PostalAddress/County/node()"/>
                  </fo:block>
                  <fo:block>
                    <xsl:value-of select="Schedule/StorageFacility/PostalAddress/PostalCode/node()"/>
                  </fo:block>
                </fo:table-cell>
                
              </fo:table-row>
              <fo:table-row border-bottom-color="#000000" border-bottom-width=".25cm" border-bottom-style="solid">
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block text-align="right" font-weight="bold" color="#000000">Sum Insured:</fo:block>
                </fo:table-cell>
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block>
                  £ <xsl:value-of select="format-number(Schedule/SumInsured/node(), '0.00')"/>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <fo:table-row border-bottom-color="#000000" border-bottom-width=".25cm" border-bottom-style="solid">
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block text-align="right" font-weight="bold" color="#000000">Single Items</fo:block>
                  <fo:block text-align="right" font-weight="bold" color="#000000">in Excess of £ 1,500:</fo:block>
                </fo:table-cell>
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <xsl:choose>
                    <xsl:when test="Schedule/NamedItems/NamedItem[Value/node()!='']">
                      <xsl:for-each select="Schedule/NamedItems/NamedItem">
                        <fo:block>
                          <xsl:value-of select="Name/node()"/>:&#160;<xsl:value-of select="$currencySymbol"/>&#160;<xsl:value-of select="format-number(Value/node(), '0.00')"/>
                        </fo:block>
                      </xsl:for-each>
                    </xsl:when>
                    <xsl:otherwise>
                      <fo:block>
                        None
                      </fo:block>
                    </xsl:otherwise>
                  </xsl:choose>
                </fo:table-cell>
              </fo:table-row>
              <fo:table-row border-bottom-color="#000000" border-bottom-width=".25cm" border-bottom-style="solid">
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block text-align="right" font-weight="bold" color="#000000">Cover Dates:</fo:block>
                </fo:table-cell>
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block>
                    Date From: 00.01 hrs &#160;<xsl:call-template name="formatdate">
                      <xsl:with-param name="date" select="Schedule/StartDate" />
                      <xsl:with-param name="format" select="'dddd dd MMM yyyy'" />
                    </xsl:call-template>
                  </fo:block>
                  <fo:block>
                    <xsl:choose>
                      <xsl:when test="Schedule/Term/node()='monthly'">
                        Renewal: Every 28 Days 
                      </xsl:when>
                      <xsl:otherwise>
                        Date To: 23.59 hrs <xsl:call-template name="formatdate">
                          <xsl:with-param name="date" select="Schedule/EndDate" />
                          <xsl:with-param name="format" select="'dddd dd MMM yyyy'" />
                        </xsl:call-template>
                      </xsl:otherwise>
                    </xsl:choose>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <!--fo:table-row border-bottom-color="#000000" border-bottom-width=".25cm" border-bottom-style="solid">
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block text-align="right" font-weight="bold" color="#000000">Total Cost:</fo:block>
                  <fo:block text-align="right" color="#000000">(inclusive of all premiums, taxes and fees)</fo:block>
                </fo:table-cell>
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block>£
                    <xsl:value-of select="format-number(Quote/TotalFee, '0.00')"/>
                  </fo:block>                 
                </fo:table-cell>
              </fo:table-row-->
            </fo:table-body>
          </fo:table>
        </fo:block>

      </fo:block-container>
      <fo:block-container position="absolute" top="14.5cm" right="0cm" left="0.5cm" height="5cm" width="19cm">
        <fo:block font-weight="bold" color="#000000" space-after="3mm">Terms and Conditions:</fo:block>
        <fo:block font-size="10pt" font-family="{$bodyfont}" margin-bottom="0.5pt" space-after="2mm">
          See Link: https://www.storeandinsure.co.uk/terms/terms-and-conditions
        </fo:block>
        <fo:block font-size="10pt" font-family="{$bodyfont}" space-after="2mm">
          You are covered for physical loss of or damage to the Property Covered up to the Sum Insured declared arising from fire (and/or the additional perils listed below) occurring during the Period of Insurance shown in the Confirmation of Insurance. The additional perils covered are explosion, lightning, aircraft, earthquake, riot, civil commotion, storm, flood, burst pipes, escape of water from any apparatus or tank or pipe, ingress of rainwater via the roof or due to blocked guttering at the self storage location, impact by road vehicles, sprinkler leakage, theft where entry or exit to Your individual self storage unit was effected by forcible and violent means, malicious damage, moth, insect or vermin from a source outside of the Property Covered.
        </fo:block>
      </fo:block-container>
      <fo:block-container position="absolute" top="19.5cm" right="0cm" left="0.5cm" height="2cm" width="19cm">
        <fo:block font-weight="bold" color="#000000" margin-bottom="0.5pt" space-after="2mm">Claims:</fo:block>
        <fo:block font-size="10pt" font-family="{$bodyfont}">
          In the event of loss or damage which may give rise to a claim under this insurance you must notify us as soon as possible and no later than 7 days after the Policy has expired by emailing us at claims@storeandinsure.co.uk
        </fo:block>

      </fo:block-container>
      <fo:block-container position="absolute" top="21.5cm" right="0cm" left="0.5cm" height="3cm" width="19cm">
        <fo:block font-weight="bold" color="#000000" space-after="3mm">Underwritten by Royal &amp; Sun Alliance plc : 100%</fo:block>
        <fo:block font-size="10pt" font-family="{$bodyfont}">
          Royal &amp; Sun Alliance Insurance plc is regulated by the Financial Conduct Authority and the Prudential Regulation Authority. You can check their status on the Financial Conduct Services register or by contacting the FCA on 0800 111 6768.
        </fo:block>

      </fo:block-container>
    </fo:block-container>


  </xsl:template>
  <!--xsl:template match="." mode="PageBody">

    <fo:block-container xmlns:fo="http://www.w3.org/1999/XSL/Format">

      <fo:block-container position="absolute" top="4.5cm" right="0cm" left="0.5cm" height="2cm" width="19cm">
        <fo:block font-size="16pt" text-align="left" font-family="{$headingfont}" font-weight="bold" color="{$headingcolor}">
          Confirmation of Insurance
        </fo:block>
      </fo:block-container>
      <fo:block-container position="absolute" top="6cm" right="0cm" left="0.5cm" height="2cm" width="19cm">
        Policy Reference No. SI-
      </fo:block-container>
      <fo:block-container position="absolute" top="12cm" right="0cm" left="0.5cm" height="20cm" width="19cm">
        <fo:block font-size="10pt" font-family="{$headingfont}">
          <fo:table table-layout="fixed">
            <fo:table-column column-width="4cm"/>
            <fo:table-column column-width="5.5cm"/>
            <fo:table-column column-width="4cm"/>
            <fo:table-column column-width="5.5cm"/>
            <fo:table-body>
              <fo:table-row border-bottom-color="#000000">
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block text-align="right" font-weight="bold" color="#000000">Location</fo:block>
                </fo:table-cell>
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block>
                    <xsl:value-of select="ContentDetail/Content/CustomerLocation/node()"/>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block text-align="right" font-weight="bold" color="#000000">Hydra-Cell model</fo:block>
                </fo:table-cell>
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block>
                    <xsl:value-of select="ContentDetail/Content/Model/node()"/>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <fo:table-row border-bottom-color="#000000" border-bottom-width=".25cm" border-bottom-style="solid">
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block text-align="right" font-weight="bold" color="#000000">Type of application</fo:block>
                </fo:table-cell>
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block>
                    <xsl:value-of select="ContentDetail/Content/Application/node()"/>
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block text-align="right" font-weight="bold" color="#000000">Flow rate</fo:block>
                </fo:table-cell>
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block>
                    <xsl:value-of select="ContentDetail/Content/FlowRate/node()"/>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <fo:table-row border-bottom-color="#000000" border-bottom-width=".25cm" border-bottom-style="solid">
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block text-align="right" font-weight="bold" color="#000000">Liquid</fo:block>
                </fo:table-cell>
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <xsl:for-each select="ContentDetail/Content/Mediums/Medium">
                    <fo:block>
                      <xsl:value-of select="node()"/>
                    </fo:block>
                  </xsl:for-each>
                </fo:table-cell>
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block text-align="right" font-weight="bold" color="#000000">Pressure</fo:block>
                </fo:table-cell>
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block>
                    <xsl:value-of select="ContentDetail/Content/Pressure/node()"/>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </fo:table-body>
          </fo:table>
        </fo:block>

        <fo:block font-size="9pt" font-family="{$headingfont}">
          <fo:table table-layout="fixed">
            <fo:table-column column-width="4cm"/>
            <fo:table-column column-width="15cm"/>
            <fo:table-body>
              <fo:table-row border-bottom-color="#000000">
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block text-align="right" font-weight="bold" color="#000000">Application details</fo:block>
                </fo:table-cell>
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block>
                    <xsl:apply-templates select="ContentDetail/Content/ApplicationDetails/*" mode="transformXhtml"/>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
              <fo:table-row border-bottom-color="#000000">
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block text-align="right" font-weight="bold" color="#000000">
                    Advantages of
                    Hydra-Cell pump
                    on this application
                  </fo:block>
                </fo:table-cell>
                <fo:table-cell padding="6pt" border-bottom="0.5pt solid black">
                  <fo:block>
                    <xsl:apply-templates select="ContentDetail/Content/Advantages/*" mode="transformXhtml"/>
                  </fo:block>
                </fo:table-cell>
              </fo:table-row>
            </fo:table-body>
          </fo:table>
        </fo:block>
      </fo:block-container>
    </fo:block-container>
  
</xsl:template-->


</xsl:stylesheet>
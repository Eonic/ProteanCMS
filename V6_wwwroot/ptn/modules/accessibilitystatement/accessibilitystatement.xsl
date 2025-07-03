<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <xsl:template match="Content[@type='Module' and @moduleType='accessibilitystatement']" mode="displayBrief">
    <xsl:variable name="siteTitle">
      <xsl:call-template name="getSettings">
        <xsl:with-param name="sectionName" select="'web'"/>
        <xsl:with-param name="valueName" select="'SiteName'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="div-class">
      <xsl:text>accessibilitystatement</xsl:text>
      <xsl:if test="@position='column1' or @position='custom' or @position='header' or @position='footer'"> character-width-80</xsl:if>
    </xsl:variable>
    <div class="accessibility-statement">
      <p>
        <xsl:value-of select="$siteTitle"/>
        <xsl:text> is committed to making its website accessible</xsl:text><xsl:if test="@public-sector='true'">, in accordance with the Public Sector Bodies (Websites and Mobile Applications) (No. 2) Accessibility Regulations 2018</xsl:if><xsl:text>.</xsl:text>
      </p>
      <p>
        This accessibility statement applies to <xsl:value-of select="$siteURL"/>.
      </p>
      <h2>How you should be able to use this website</h2>
      <p>We want as many people as possible to be able to use this website. For example, that means you should be able to:</p>
      <ul>
        <li>change colours, contrast levels and fonts using browser or device settings</li>
        <li>zoom in up to 400% without the text spilling off the screen</li>
        <li>navigate most of the website using a keyboard or speech recognition software</li>
        <li>listen to most of the website using a screen reader (including the most recent versions of JAWS, NVDA and VoiceOver)</li>
      </ul>
      <p>We also make the website text as simple as possible to understand.</p>
      <p>
        AbilityNet has advice on <a href="https://mcmw.abilitynet.org.uk/">making your device easier to use</a> if you have a disability.
      </p>
      <h2>Compliance status</h2>
      <xsl:if test="@compliance-status='a'">
        <p>This website is fully compliant with the Web Content Accessibility Guidelines (WCAG) version 2.2 AA standard.</p>
      </xsl:if>
      <xsl:if test="@compliance-status='b'">
        <p>This website is partially compliant with the Web Content Accessibility Guidelines version 2.2 AA standard, due to the non-compliance(s) and/or the exemptions listed below.</p>
      </xsl:if>
      <xsl:if test="@compliance-status='c'">
        <p>This website is not compliant with the Web Content Accessibility Guidelines version 2.2 AA standard. The non-compliance(s) and/or the exemptions are listed below.</p>
      </xsl:if>
      <h2>Non-accessible content</h2>
      <p>The content listed below is non-accessible for the following reason(s):</p>
      <xsl:if test="body/node()">
        <h3>Non-compliance With the Accessibility Regulations</h3>
        <div class="{$div-class}">
          <xsl:apply-templates select="body/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>
      <xsl:if test="disp-burden/node()">
        <h3>Disproportionate Burden</h3>
        <div class="{$div-class}">
          <xsl:apply-templates select="disp-burden/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>
      <xsl:if test="not-scope/node()">
        <h3>The Content is Not Within the Scope of the Accessibility Regulations</h3>
        <div class="{$div-class}">
          <xsl:apply-templates select="not-scope/node()" mode="cleanXhtml"/>
        </div>
      </xsl:if>
      <h2>Preparation of this accessibility statement</h2>
      <p>
        <xsl:if test="@prep-date">
          <xsl:text>This statement was prepared on </xsl:text>
          <xsl:value-of select="@prep-date"/>
          <xsl:text>.</xsl:text>
        </xsl:if>
      </p>
      <p>
        <xsl:if test="@review-date">
          <xsl:text>The statement was last reviewed on </xsl:text>
          <xsl:value-of select="@review-date"/>
          <xsl:text>.</xsl:text>
        </xsl:if>
      </p>
      <p>
        <xsl:if test="@test-date">
          <xsl:text>This website was last tested in </xsl:text>
          <xsl:value-of select="@test-date"/>
          <xsl:text> against the WCAG 2.2 AA standard. </xsl:text>
        </xsl:if>
        <xsl:if test="@tester">
          <xsl:text>This test of a representative sample of pages was carried out by the </xsl:text>
          <xsl:value-of select="@tester"/>
        </xsl:if>
        <xsl:text>.</xsl:text>
      </p>
      <h2>Feedback and contact information</h2>
      <p>If you find any problems that are not listed on this page or you think we’re not meeting the accessibility requirements please get in touch.</p>
      <p>
        <xsl:if test="contact/node()">
          <xsl:apply-templates select="contact/node()" mode="cleanXhtml"/>
        </xsl:if>
      </p>
      <xsl:if test="@public-sector='true'">
        <h2>Enforcement procedure</h2>
        <p>
          The Equality and Human Rights Commission (EHRC) is responsible for enforcing the Public Sector Bodies (Websites and Mobile Applications) (No. 2) Accessibility Regulations 2018 (the ‘accessibility regulations’). If you’re not happy with how we respond to your complaint, contact the <a href="https://www.equalityadvisoryservice.com/">Equality Advisory and Support Service (EASS)</a>.
        </p>
      </xsl:if>
    </div>
  </xsl:template>

</xsl:stylesheet>
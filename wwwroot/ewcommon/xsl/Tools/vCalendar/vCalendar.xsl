<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"  xmlns:date= "http://exslt.org/dates-and-times"  extension-element-prefixes="date" xmlns:atom="http://www.w3.org/2005/Atom" xmlns:ew="urn:ew">
  <xsl:import href="../../../../ewcommon_v5/xsl/Tools/Functions.xsl"/>

  <!-- THIS XSL GENERATES THE vCalendar/iCalendar for EWContent WH 2010-03-23 -->

  <!-- ATTENTION!!!! BE MINDFUL OF THE LINEBREAKS THE XSL:TEXT's ARE KEEPING!! -->

  <xsl:output method="text" omit-xml-declaration="no" indent="no"/>

  <xsl:variable name="siteURL">
    <xsl:text>http://demo.nae2k12.co.uk.web01.eonichost.co.uk</xsl:text>
    <xsl:value-of select="/Page/Request/ServerVariables/Item[@name='SERVER_NAME']/node()"/>
  </xsl:variable>

  <!-- -->

  <xsl:template match="Page">
    <xsl:apply-templates select="//ContentDetail/Content" mode="vcalendar"/>
  </xsl:template>

  <!-- -->

  <xsl:template match="Content[@type='Event']" mode="vcalendar">
    <!-- DECLARATIONS -->
    <xsl:text>BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//hacksw/handcal//NONSGML v1.0//EN
BEGIN:VEVENT
<!-- START DATE-->
DTSTART:</xsl:text>
    <xsl:value-of select="translate(StartDate,'-','')"/>
    <xsl:text>T</xsl:text>
    <xsl:value-of select="translate(Times/@start,',','')"/>
    <xsl:text>00</xsl:text>
    <xsl:text>
<!-- END DATE-->
DTEND:</xsl:text>
    <xsl:choose>
      <xsl:when test="EndDate/node()!=''">
        <xsl:value-of select="translate(EndDate,'-','')"/>
        <xsl:text>T</xsl:text>
        <xsl:value-of select="translate(Times/@end,',','')"/>
        <xsl:text>00</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="translate(StartDate,'-','')"/>
        <xsl:text>T</xsl:text>
        <xsl:value-of select="translate(Times/@end,',','')"/>
        <xsl:text>00</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text>
<!-- TITLE -->
SUMMARY:</xsl:text>
    <xsl:value-of select="Headline/node()"/>
    <xsl:text>
<!-- DESCRIPTION-->
DESCRIPTION;VALUE=URL:</xsl:text>
    <xsl:apply-templates select="." mode="getHref"/>
    <!-- LOCATION -->
    <xsl:if test="Location/Venue/node()!=''">
      <xsl:text>
LOCATION:</xsl:text>
      <xsl:value-of select="Location/Venue"/>
    </xsl:if>
    <!-- THIS WILL EMAIL THE ORGANISER WITH ANOTHER ENTRY IN 2007 -->
    <!--<xsl:if test="Organizer/@email!=''">
      <xsl:text>
ATTENDEE;ROLE=ORGANIZER:</xsl:text>
      <xsl:value-of select="Organizer/@email"/>
    </xsl:if>-->
    <xsl:text>
<!-- LAST MODIFIED -->
LAST-MODIFIED:</xsl:text>
    <xsl:value-of select="translate(substring(@update, 1,19), '-:','')"/>
    <xsl:text>
<!-- PUBLISHED DATE -->
DCREATED:</xsl:text><xsl:value-of select="translate(substring(@publish, 1,19), '-:','')"/><xsl:text>
<!-- / END DECLARATIONS -->
END:VEVENT
END:VCALENDAR</xsl:text>
  </xsl:template>
</xsl:stylesheet>
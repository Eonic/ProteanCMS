<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"  xmlns:date= "http://exslt.org/dates-and-times"  extension-element-prefixes="date" xmlns:atom="http://www.w3.org/2005/Atom" xmlns:ew="urn:ew">
  <xsl:import href="../../../../ewcommon_v5/xsl/Tools/Functions.xsl"/>

  <!-- THIS XSL GENERATES THE vCalendar/iCalendar for EWContent WH 2010-03-23 -->

  <!-- ATTENTION!!!! BE MINDFUL OF THE LINEBREAKS THE XSL:TEXT's ARE KEEPING!! -->

  <xsl:output method="text" omit-xml-declaration="no" indent="no" encoding="ASCII" />

  <xsl:variable name="siteURL">
    <xsl:text>http://</xsl:text>
    <xsl:value-of select="/Page/Request/ServerVariables/Item[@name='SERVER_NAME']/node()"/>
  </xsl:variable>

  <!-- -->

  <xsl:template match="Page">
    <xsl:apply-templates select="//ContentDetail/Content" mode="vcalendar"/>
  </xsl:template>

  <!-- -->

<xsl:template match="Content[@type='Event']" mode="vcalendar">BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//Apple Inc.//iCal 4.0.4//EN
BEGIN:VEVENT
CREATED:<xsl:value-of select="translate(substring(@publish, 1,19), '-:','')"/>Z
DTEND:<xsl:choose><xsl:when test="EndDate/node()!=''"><xsl:value-of select="translate(EndDate,'-','')"/>T<xsl:value-of select="translate(Times/@end,',','')"/>00</xsl:when><xsl:otherwise><xsl:value-of select="translate(StartDate,'-','')"/>T<xsl:value-of select="translate(Times/@end,',','')"/>00</xsl:otherwise></xsl:choose>
TRANSP:OPAQUE
SUMMARY:<xsl:value-of select="Headline/node()"/>
DESCRIPTION;VALUE=URL:/<xsl:value-of select="$siteURL"/><xsl:apply-templates select="." mode="getHref"/>
DTSTART:<xsl:value-of select="translate(StartDate,'-','')"/>T<xsl:value-of select="translate(Times/@start,',','')"/>00Z
DTSTAMP:<xsl:value-of select="translate(StartDate,'-','')"/>T<xsl:value-of select="translate(Times/@start,',','')"/>00Z
SEQUENCE:0
END:VEVENT
END:VCALENDAR</xsl:template></xsl:stylesheet>
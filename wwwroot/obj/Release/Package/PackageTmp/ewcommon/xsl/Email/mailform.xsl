<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <xsl:import href="emailStationary.xsl"/>

  <xsl:template match="/">
    <xsl:apply-imports/>
  </xsl:template>
  <xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/>

  <xsl:template match="*" mode="subject">
    <xsl:value-of select="$siteTitle"/> - <xsl:value-of select="@subjectLine"/>
  </xsl:template>


  <xsl:template match="*" mode="bodyLayout">
          <h2>Email Enquiry</h2>
          <font face="arial" size="2">
            
        <table width="550" cellspacing="0" cellpadding="10" id="emailSummary" summary="Content submitted from website email form">
        <xsl:for-each select="*">
          <tr>
            <th>
              <font face="verdana" size="2">
                <xsl:choose>
                    <xsl:when test="@label and @label!=''">
                      <xsl:value-of select="@label"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="name()"/>
                    </xsl:otherwise>
                  </xsl:choose>  
              </font>
            </th>
            <xsl:choose>
              <xsl:when test="name()='Email'">
                <td>
                  <font face="verdana" size="2">
                    <a href="mailto:{node()}" title="reply to sender">
                      <xsl:value-of select="node()"/>
                    </a>
                  </font>
                </td>
              </xsl:when>
              <xsl:when test="contains(@type,'date')">
                <td>
                  <font face="verdana" size="2">
                        <xsl:call-template name="DD_Mon_YYYY">
                          <xsl:with-param name="date" select="node()"/>
                        </xsl:call-template>
                  </font>
                </td>
              </xsl:when>
              <xsl:otherwise>
                <td>
                  <font face="verdana" size="2">
                    <xsl:value-of select="node()"/>
                  </font>
                </td>
              </xsl:otherwise>
            </xsl:choose>
          </tr>
        </xsl:for-each>
      </table>        
     </font>
  </xsl:template>

 
</xsl:stylesheet>
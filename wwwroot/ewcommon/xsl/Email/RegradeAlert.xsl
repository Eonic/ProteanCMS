<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:import href="emailStationary.xsl"/>
  <xsl:template match="/">
    <xsl:apply-imports/>
  </xsl:template>
  <xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/>
  <xsl:template match="*" mode="subject">New Trade User Website Application</xsl:template>
  
  <xsl:template match="*" mode="bodyLayout">
    <table cellspacing="0" cellpadding="10" summary="New user account details">
      <tr>
        <td colspan="2">
          <font face="verdana" size="2">
            Dear <xsl:value-of select="User/FirstName"/>,<br/><br/>
            <xsl:apply-templates select="emailer/oBodyXML/Items/Message" mode="cleanXhtml"/>
            
            
          </font>
        </td>
      </tr>
      
   
      
    </table>
  </xsl:template>
</xsl:stylesheet>

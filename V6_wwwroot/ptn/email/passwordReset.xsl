<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:import href="emailStationary.xsl"/>
  <xsl:template match="/">
    <xsl:apply-imports/>
  </xsl:template>
  <xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/>
  <xsl:template match="*" mode="subject">Password Reset</xsl:template>
  <xsl:template match="*" mode="bodyLayout">
    <table cellspacing="0" cellpadding="10" summary="Your user account details">
      <tr>
        <td colspan="2">
          <p>

            <font face="verdana" size="2">
              Dear <xsl:value-of select="User/FirstName/node()"/>&#160;<xsl:value-of select="AccountReset/User/LastName/node()"/>,
            </font>
          </p>
        </td>
      </tr>
      <tr>
        <td colspan="2">
          <p>
            <font face="verdana" size="2">
              <strong>Please find the details of your user account</strong>
            </font>
          </p>
        </td>
      </tr>
      <tr>
        <th>
          <font face="verdana" size="2">Username</font>
        </th>
        <td>
          <font face="verdana" size="2">
            <xsl:value-of select="User/@name"/>
          </font>
        </td>
      </tr>
      <tr>
        <th>
          <font face="verdana" size="2">Email</font>
        </th>
        <td>
          <font face="verdana" size="2">
            <xsl:value-of select="User/Email/node()"/>
          </font>
        </td>
      </tr>
      <tr>
        <th>
          <font face="verdana" size="2">Follow this link to reset your password</font>
        </th>
        <td>
          <font face="verdana" size="2">
            <xsl:variable name="secureLink">
              <xsl:value-of select="$siteURL"/>
			        <xsl:value-of select="@Url"/>
              <xsl:text>?ewCmd=AR&amp;id=</xsl:text>
              <xsl:value-of select="User/@id"/>
              <xsl:text>&amp;AI=</xsl:text>
              <xsl:value-of select="@Link"/>
            </xsl:variable>
            <a href="{$secureLink}" title="Reset password">Click here to reset your password</a>
          </font>
        </td>
      </tr>
    </table>
  </xsl:template>
</xsl:stylesheet>

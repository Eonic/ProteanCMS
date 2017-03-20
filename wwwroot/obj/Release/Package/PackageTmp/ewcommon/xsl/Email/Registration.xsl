<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:import href="EmailStationary.xsl"/>
  <xsl:template match="/">
    <xsl:apply-imports/>
  </xsl:template>
  <xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/>
  <xsl:template match="*" mode="subject">Welcome to our website</xsl:template>
  <xsl:template match="*" mode="bodyLayout">
    <table cellspacing="0" cellpadding="10" summary="Your user account details">
      <tr>
        <td colspan="2">
          <font face="verdana" size="2">
            Dear <xsl:value-of select="FirstName/node()"/>&#160;<xsl:value-of select="LastName/node()"/>,
          </font>
        </td>
      </tr>
      <tr>
        <td colspan="2">
          <font face="verdana" size="2">
            <strong>
              <p>Thank you for registering</p>
              <p>Please find the details of your user account below.</p>
            </strong>
          </font>
        </td>
      </tr>
		<tr>
			<th>
				<font face="verdana" size="2">Email</font>
			</th>
			<td>
				<font face="verdana" size="2">
					<xsl:value-of select="Email/node()"/>
				</font>
			</td>
		</tr>
      <!--<tr>
        <th>
          <font face="verdana" size="2">Username</font>
        </th>
        <td>
          <font face="verdana" size="2">
            <xsl:value-of select="@name"/>
          </font>
        </td>
      </tr>-->
      <tr>
        <th>
          <font face="verdana" size="2">Password</font>
        </th>
        <td>
          <font face="verdana" size="2">
            <xsl:value-of select="Password/node()"/>
          </font>
        </td>
      </tr>

      <tr>
        <th>
          <font face="verdana" size="2">Name</font>
        </th>
        <td>
          <font face="verdana" size="2">
            <xsl:value-of select="FirstName/node()"/>&#160;<xsl:value-of select="LastName/node()"/>
          </font>
        </td>
      </tr>

      <!--<tr>
        <th>
          <font face="verdana" size="2">Position</font>
        </th>
        <td>
          <font face="verdana" size="2">
            <xsl:value-of select="Position/node()"/>
          </font>
        </td>
      </tr>-->

      
    </table>

  </xsl:template>
</xsl:stylesheet>

<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:import href="EmailStationary.xsl"/>
    <xsl:template match="/">
        <xsl:apply-imports/>
    </xsl:template>
    <xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/>
    <xsl:template match="*" mode="subject">Vacancy Application</xsl:template>
    <xsl:template match="*" mode="bodyLayout">
        <xsl:param name="siteURL" />
        <span style="text-align:left !important;">
            <table width="550" cellspacing="0" cellpadding="10" id="emailSummary" summary="Content submitted from website email form">
                <xsl:for-each select="*">
                <xsl:if test="name()='YourEmail'">
                  &#160;
                   </xsl:if>
                    <tr>
                        <th>
                            <font face="verdana" size="2">
                                <xsl:choose>
                                    <xsl:when test="name()='VacancyRef'">
                                        <xsl:text>Job Applying For</xsl:text>
                                    </xsl:when>
                                    <xsl:when test="name()='FriendsEmail'">
                                        <xsl:text>Your Email</xsl:text>
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
        </span>
    </xsl:template>
</xsl:stylesheet>
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns:ew="urn:ew">
  <xsl:output method="xml" indent="no" standalone="yes" omit-xml-declaration="yes" encoding="UTF-8"/>
  <xsl:strip-space elements="*"/>
	
  <xsl:template match="/Workbook/Sheet[@name='Sheet1']">
    <Instances>
      <DeleteNonEntries enabled="false">
        <cDefiningField>cContentSchemaName</cDefiningField>
      </DeleteNonEntries>
  		<xsl:apply-templates select="Row" mode="Instance"/>
    </Instances>
  </xsl:template>

  <xsl:template match="Row" mode="Instance">
    <xsl:variable name="oldInstance" select="ew:ContentQuery(Postcode/node())"/>
    <Instance>
      <tblDirectory>
        <nDirKey/>
        <cDirName/>
        <cDirPassword/>
        <cDirForiegnRef/>
        <cDirSchema>User</cDirSchema>
        <cDirXml>
          <User>
            <FirstName label="First name"/>
            <MiddleName label="Middle name"/>
            <LastName label="Surname"/>
            <Position/>
            <Email/>
            <Notes/>
          </User>
        </cDirXml>
        <nAuditId/>
        <nAuditKey/>
        <dPublishDate/>
        <dExpireDate/>
        <dInsertDate/>
        <nInsertDirId/>
        <dUpdateDate/>
        <nUpdateDirId/>
        <nStatus>1</nStatus>
        <cDescription/>
      </tblDirectory>
    </Instance>
 </xsl:template>




  <xsl:template match="*" mode="cleanXhtml">

    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:if test="name()!='style'">
          <xsl:attribute name="{name()}">
            <xsl:value-of select="." />
          </xsl:attribute>
        </xsl:if>
      </xsl:for-each>
      <xsl:apply-templates mode="cleanXhtml"/>
    </xsl:element>
  </xsl:template>

  <!-- IMAGE PROCESSING  -->
  <xsl:template match="img" mode="cleanXhtml">

    <!-- Stick in Variable and then ms:nodest it 
          - ensures its self closing and we can process all nodes!! -->
    <xsl:variable name="img">
      <xsl:element name="img">
        <xsl:for-each select="@*[name()!='border' and name()!='align' and name()!='style']">

          <xsl:attribute name="{name()}">
            <xsl:choose>

              <!-- ##### @Attribute Conditions ##### -->

              <xsl:when test="name()='src'">
                <xsl:choose>
                  <xsl:when test="contains(.,'http://')">
                    <xsl:value-of select="."/>
                  </xsl:when>
                  <xsl:otherwise>
                    <!--<xsl:value-of select="$siteURL"/>-->
                    <xsl:value-of select="."/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>

              <!-- ##### VALIDATION - Attribute "align" can not be used for this element. ##### -->
              <xsl:when test="name()='class' and (ancestor::img[@align] or contains(ancestor::img/@style,'float: '))">
                <xsl:variable name="align" select="ancestor::img/@align"/>
                <xsl:variable name="float" select="substring-before(substring-after(ancestor::img/@style,'float: '),';')"/>
                <xsl:value-of select="."  />
                <xsl:text> align</xsl:text>
                <xsl:choose>
                  <xsl:when test="@align">
                    <xsl:value-of select="$align"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:value-of select="$float"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="."  />
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
        </xsl:for-each>

        <!-- ##### VALIDATION - Attribute "align" can not be used for this element. ##### -->
        <xsl:if test="not(@class) and (@align or contains(@style,'float: '))">
          <xsl:attribute name="class">
            <xsl:variable name="float" select="substring-before(substring-after(@style,'float: '),';')"/>
            <xsl:variable name="align" select="@align"/>
            <xsl:text>align</xsl:text>
            <xsl:choose>
              <xsl:when test="@align">
                <xsl:value-of select="$align"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="$float"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
        </xsl:if>

        <!-- ##### VALIDATION - required attribute "alt" ##### -->
        <xsl:if test="not(@alt)">
          <xsl:attribute name="alt"></xsl:attribute>
        </xsl:if>

      </xsl:element>
    </xsl:variable>
    <xsl:copy-of select="ms:node-set($img)"/>
  </xsl:template>


  <xsl:template match="br" mode="cleanXhtml">
    <br/>
  </xsl:template>

  <xsl:template match="hr" mode="cleanXhtml">
    <hr/>
  </xsl:template>

</xsl:stylesheet>
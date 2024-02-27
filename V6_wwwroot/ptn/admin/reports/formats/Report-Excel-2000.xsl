<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
xmlns="urn:schemas-microsoft-com:office:spreadsheet"
xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet"
xmlns:x="urn:schemas-microsoft-com:office:excel">


  <xsl:import href="../Report-Base.xsl"/>
  
  <xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes"/>
  
  <xsl:template match="Page">
    <xsl:processing-instruction name="mso-application">
      <xsl:text>progid="Excel.Sheet"</xsl:text>
    </xsl:processing-instruction>
    <Workbook xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:x="urn:schemas-microsoft-com:office:excel"  xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet" xmlns:html="http://www.w3.org/TR/REC-html40">
        <Worksheet ss:Name="Page 1">
          <xsl:for-each select="ContentDetail/Report | ContentDetail/Content[@type='Report']/Report">
          <Table x:FullColumns="1" x:FullRows="1">
            <xsl:apply-templates select="*[position()=1]" mode="reportTitle"/>
            <xsl:apply-templates select="*" mode="reportRow"/>
          </Table>
          </xsl:for-each>
      </Worksheet>
    </Workbook>
  </xsl:template>

  <!-- Row template -->
  <xsl:template match="*" mode="reportTitle">
    <Row ss:AutoFitHeight="1">
      <xsl:for-each select="*">
        <Cell>
          <Data ss:Type="String">
            <xsl:choose>
              <xsl:when test="name()='UserXml'">Full Name</xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="name()"/>
              </xsl:otherwise>
            </xsl:choose>
          </Data>
        </Cell>
      </xsl:for-each>
      </Row>
    </xsl:template>


  <xsl:template match="Item[cCartXml]" mode="reportTitle">
    <Row ss:AutoFitHeight="1">
        <Cell>
          <Data ss:Type="String">
            Order Number
          </Data>
        </Cell>
      <Cell>
        <Data ss:Type="String">
          Customer Name
        </Data>
      </Cell>
      <Cell>
        <Data ss:Type="String">
          Customer Email
        </Data>
      </Cell>
      <Cell>
        <Data ss:Type="String">
          Order Date
        </Data>
      </Cell>
    </Row>
  </xsl:template>


  <!-- ROW BUILDER -->
	<xsl:template match="*" mode="reportRow">
    <Row>
      <xsl:apply-templates select=".//*[not(*)]" mode="reportCell"/>
		</Row>	
  </xsl:template>

  <xsl:template match="Item[cCartXml]" mode="reportRow">
    <!--<Row>
      <Cell>
        <Data ss:Type="String">
          Order Number
        </Data>
      </Cell>
      <Cell>
        <Data ss:Type="String">
          Customer Name
        </Data>
      </Cell>
      <Cell>
        <Data ss:Type="String">
          Customer Email
        </Data>
      </Cell>
      <Cell>
        <Data ss:Type="String">
          Order Date
        </Data>
      </Cell>
    </Row>-->
  </xsl:template>
  
  
	<!-- CELL BUILDER -->
	<xsl:template match="*[not(*)]" mode="reportCell">
		<xsl:variable name="name" select="local-name()"/>
		<xsl:variable name="datatype" select="ancestor::Report/Item[1]/*[local-name()=$name]/@datatype"/>
    <Cell>
      <Data ss:Type="String">
			<xsl:if test="contains(local-name(),'Date') or $datatype='date'">
				<xsl:attribute name="class">
					<xsl:text>date</xsl:text>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="." mode="reportCellValue"/>
		 </Data>
    </Cell>
	</xsl:template>


  <!-- CELL VALUE FORMATTER -->
  <xsl:template match="*" mode="reportCellValue">
    <xsl:variable name="name" select="local-name()"/>
    <xsl:variable name="metadata" select="ancestor::Report/Item[1]/*[local-name()=$name]"/>
    <xsl:variable name="datatype" select="$metadata/@datatype"/>
    <xsl:choose>
      <!-- Type Specific Options -->
      <!-- Date -->
      <xsl:when test="contains(name(),'Date') or $datatype='date'">
        <xsl:if test="not(contains(node(),'0001-01-01T00:00:00'))">
          <xsl:variable name="dateFormat">
            <xsl:text>dd MMM yyyy</xsl:text>
            <xsl:if test="not(contains(name(),'NoTime'))">
              <xsl:text> hh:mm</xsl:text>
            </xsl:if>
          </xsl:variable>
          <xsl:call-template name="formatdate">
            <xsl:with-param name="date" select="node()"/>
            <xsl:with-param name="format" select="$dateFormat"/>
          </xsl:call-template>
        </xsl:if>
      </xsl:when>

      <!-- Status -->
      <xsl:when test="name() = 'Status'">
        <xsl:choose>
          <xsl:when test="node()='0'">In-active</xsl:when>
          <xsl:when test="node()='1' or node()='-1' ">Active</xsl:when>
        </xsl:choose>
      </xsl:when>

      <!-- Status -->
      <xsl:when test="name() = 'User' or name() = 'UserXml'">
        <xsl:choose>
          <xsl:when test="FirstName and LastName">
            <xsl:value-of select="LastName"/>, <xsl:value-of select="FirstName"/>
          </xsl:when>
          <xsl:when test="User/FirstName and User/LastName">
            <xsl:value-of select="User/LastName"/>, <xsl:value-of select="User/FirstName"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="node()"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>

      <xsl:otherwise>
        <xsl:value-of select="node()"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

</xsl:stylesheet>

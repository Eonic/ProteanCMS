<?xml version="1.0" ?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"  xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns:date= "http://exslt.org/dates-and-times"  extension-element-prefixes="date" xmlns:atom="http://www.w3.org/2005/Atom" xmlns:ew="urn:ew">
  <xsl:import href="../rss/rss.xsl"/>

  <xsl:output method="xml" omit-xml-declaration="no" indent="yes" cdata-section-elements="description"/>

  <!-- Standard Content Item -->
  <xsl:template match="Content[@type='Job']" mode="contentItem">
    <item>
      <guid>
        <xsl:value-of select="@id"/>
      </guid>
      <title>
        <xsl:apply-templates select="." mode="getDisplayName"/>
      </title>
      <link>
        <xsl:apply-templates select="." mode="getHref"/>
      </link>
      <description>
        <xsl:variable name="description">
          <xsl:apply-templates select="." mode="buildDescription"/>
        </xsl:variable>
        <xsl:apply-templates select="ms:node-set($description)/*" mode="encodeXhtml" />
      </description>
      <pubDate>
        <xsl:apply-templates select="." mode="getPublishDate" />
      </pubDate>
    </item>
  </xsl:template>

  <xsl:template match="Content[@type='Job']" mode="buildDescription">
    <div>
    <xsl:variable name="image">
      <xsl:apply-templates select="." mode="displayDetail"/>
    </xsl:variable>
    <xsl:if test="ms:node-set($image)/*">
      <a>
        <xsl:apply-templates select="ms:node-set($image)/*" mode="encodeXhtml" />
      </a>
    </xsl:if>
    
    <xsl:apply-templates select="Summary/node()" mode="encodeXhtml" />
    
    <xsl:if test="ContractType/node()!='' or Ref/node()!='' or Location/node()!='' or Salary/node()!='' or ApplyBy/node()!=''">
      <p>
        <xsl:if test="ContractType/node()!=''">
          <xsl:text>Contract type: </xsl:text>
          <xsl:value-of select="ContractType/node()"/>
          <br/>
        </xsl:if>
        <xsl:if test="Ref/node()!=''">
          <xsl:text>Ref: </xsl:text>
          <xsl:value-of select="Ref/node()"/>
          <br/>
        </xsl:if>

        <xsl:if test="Location/node()!=''">
          <xsl:text>Location: </xsl:text>
          <xsl:value-of select="Location/node()"/>
          <br/>

        </xsl:if>
        <xsl:if test="Salary/node()!=''">
          <xsl:text>Salary: </xsl:text>

          <xsl:choose>
            <xsl:when test="format-number(Salary/node(),'0')='NaN'">
              <xsl:if test="not(contains(Salary/node(),'£'))">
                <xsl:text>£</xsl:text>
              </xsl:if>
              <xsl:value-of select="Salary/node()"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:text>£</xsl:text>
              <xsl:value-of select="format-number(Salary/node(),'#,###,###.00')"/>
            </xsl:otherwise>
          </xsl:choose>
          <br/>
        </xsl:if>
        
        <xsl:if test="ApplyBy/node()!=''">
          <xsl:text>Deadline for applications: </xsl:text>
          <xsl:call-template name="DisplayDate">
            <xsl:with-param name="date" select="ApplyBy/node()"/>
          </xsl:call-template>
          <br/>
        </xsl:if>
      </p>
    </xsl:if>
    </div>
  </xsl:template>

</xsl:stylesheet>


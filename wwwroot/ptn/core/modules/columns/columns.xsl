<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <xsl:template match="Content[@moduleType='3columns']" mode="displayBrief">
    <xsl:variable name="responsiveColumns">
      <xsl:apply-templates select="." mode="responsiveColumns">
        <xsl:with-param name="defaultCols" select="'4'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row">
      <div id="column1-{@id}" class="column1 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column3-{@id}" class="column3 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column3-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column3 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='1Column' or @moduleType='Conditional1Column']" mode="displayBrief">
    <div class="row">
      <xsl:if test="@flexbox='true'">
        <xsl:attribute name="class">
          row flexbox-columns flexbox-cols-<xsl:value-of select="@flexColumns"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="$adminMode and @moduleType='Conditional1Column'">
        <xsl:attribute name="class">row conditional-block</xsl:attribute>
        <div class="conditional-note">
          This block is conditional on the querystring containing '<xsl:value-of select="@querystringcontains"/>'
        </div>
      </xsl:if>
      <div id="column1-{@id}" class="column1 col-md-12">

        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 col-md-12</xsl:text>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2columns5050']" mode="displayBrief">
    <xsl:variable name="responsiveColumns">
      <xsl:apply-templates select="." mode="responsiveColumns">
        <xsl:with-param name="defaultCols" select="'6'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row">
      <div id="column1-{@id}" class="column1 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2Columns3366']" mode="displayBrief">
    <xsl:variable name="unevenColumns1">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'4'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="unevenColumns2">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'8'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row">
      <div id="column1-{@id}" class="column1 {$unevenColumns1}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$unevenColumns1"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$unevenColumns2}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$unevenColumns2"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2Columns6633']" mode="displayBrief">
    <xsl:variable name="unevenColumns1">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'8'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="unevenColumns2">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'4'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row">
      <div id="column1-{@id}" class="column1 {$unevenColumns1}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$unevenColumns1"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$unevenColumns2}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$unevenColumns2"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2Columns2575']" mode="displayBrief">
    <xsl:variable name="unevenColumns1">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'3'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="unevenColumns2">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'9'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row">
      <div id="column1-{@id}" class="column1 {$unevenColumns1}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$unevenColumns1"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$unevenColumns2}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$unevenColumns2"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2Columns7525']" mode="displayBrief">
    <xsl:variable name="unevenColumns1">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'9'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="unevenColumns2">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'3'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row">
      <div id="column1-{@id}" class="column1 {$unevenColumns1}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$unevenColumns1"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$unevenColumns2}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$unevenColumns2"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2Columns1683']" mode="displayBrief">
    <xsl:variable name="unevenColumns1">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'2'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="unevenColumns2">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'10'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row">
      <div id="column1-{@id}" class="column1 {$unevenColumns1}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$unevenColumns1"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$unevenColumns2}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$unevenColumns2"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2Columns8316']" mode="displayBrief">
    <xsl:variable name="unevenColumns1">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'10'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="unevenColumns2">
      <xsl:apply-templates select="." mode="unevenColumns">
        <xsl:with-param name="defaultWidth" select="'2'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row">
      <div id="column1-{@id}" class="column1 {$unevenColumns1}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$unevenColumns1"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$unevenColumns2}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$unevenColumns2"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='4columns']" mode="displayBrief">
    <xsl:variable name="responsiveColumns">
      <xsl:apply-templates select="." mode="responsiveColumns">
        <xsl:with-param name="defaultCols" select="'3'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row">
      <div id="column1-{@id}" class="column1 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column3-{@id}" class="column3 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column3-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column3 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column4-{@id}" class="column4 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column4-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column4 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2Columns4060']" mode="displayBrief">
    <xsl:variable name="unevenColumns1">
      <xsl:apply-templates select="." mode="uneven5Columns">
        <xsl:with-param name="defaultWidth" select="'4'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="unevenColumns2">
      <xsl:apply-templates select="." mode="uneven5Columns">
        <xsl:with-param name="defaultWidth" select="'6'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row fivecolumns">
      <xsl:if test="@mdCol and @mdCol!=''">
        <xsl:attribute name="class">row fivecolumns fivecolumns-lg</xsl:attribute>
      </xsl:if>
      <div id="column1-{@id}" class="column1 {$unevenColumns1}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$unevenColumns1"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$unevenColumns2}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$unevenColumns2"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2Columns6040']" mode="displayBrief">
    <xsl:variable name="unevenColumns1">
      <xsl:apply-templates select="." mode="uneven5Columns">
        <xsl:with-param name="defaultWidth" select="'6'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="unevenColumns2">
      <xsl:apply-templates select="." mode="uneven5Columns">
        <xsl:with-param name="defaultWidth" select="'4'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row fivecolumns">
      <xsl:if test="@mdCol and @mdCol!=''">
        <xsl:attribute name="class">row fivecolumns fivecolumns-lg</xsl:attribute>
      </xsl:if>
      <div id="column1-{@id}" class="column1 {$unevenColumns1}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$unevenColumns1"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$unevenColumns2}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$unevenColumns2"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2Columns2080']" mode="displayBrief">
    <xsl:variable name="unevenColumns1">
      <xsl:apply-templates select="." mode="uneven5Columns">
        <xsl:with-param name="defaultWidth" select="'2'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="unevenColumns2">
      <xsl:apply-templates select="." mode="uneven5Columns">
        <xsl:with-param name="defaultWidth" select="'8'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row fivecolumns">
      <xsl:if test="@mdCol and @mdCol!=''">
        <xsl:attribute name="class">row fivecolumns fivecolumns-lg</xsl:attribute>
      </xsl:if>
      <div id="column1-{@id}" class="column1 {$unevenColumns1}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$unevenColumns1"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$unevenColumns2}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$unevenColumns2"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2Columns8020']" mode="displayBrief">
    <xsl:variable name="unevenColumns1">
      <xsl:apply-templates select="." mode="uneven5Columns">
        <xsl:with-param name="defaultWidth" select="'8'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="unevenColumns2">
      <xsl:apply-templates select="." mode="uneven5Columns">
        <xsl:with-param name="defaultWidth" select="'2'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row fivecolumns">
      <xsl:if test="@mdCol and @mdCol!=''">
        <xsl:attribute name="class">row fivecolumns fivecolumns-lg</xsl:attribute>
      </xsl:if>
      <div id="column1-{@id}" class="column1 {$unevenColumns1}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$unevenColumns1"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$unevenColumns2}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$unevenColumns2"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='5columns']" mode="displayBrief">
    <xsl:variable name="responsiveColumns">
      <xsl:apply-templates select="." mode="responsiveColumns">
        <xsl:with-param name="defaultCols" select="'2'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row fivecolumns">
      <xsl:if test="@mdCol and @mdCol!='' and @mdCol!='5'">
        <xsl:attribute name="class">row fivecolumns-lg</xsl:attribute>
      </xsl:if>
      <div id="column1-{@id}" class="column1 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column3-{@id}" class="column3 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column3-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column3 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column4-{@id}" class="column4 {$responsiveColumns}">

        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column4-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column4 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column5-{@id}" class="column5 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column5-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column5 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='6columns']" mode="displayBrief">
    <xsl:variable name="responsiveColumns">
      <xsl:apply-templates select="." mode="responsiveColumns">
        <xsl:with-param name="defaultCols" select="'2'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row">
      <div id="column1-{@id}" class="column1 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column2-{@id}" class="column2 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column2-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column2 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column3-{@id}" class="column3 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column3-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column3 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column4-{@id}" class="column4 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column4-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column4 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column5-{@id}" class="column5 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column5-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column5 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <div id="column6-{@id}" class="column6 {$responsiveColumns}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column6-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column6 </xsl:text>
            <xsl:value-of select="$responsiveColumns"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <!-- ACCORDION -->
  <xsl:template match="Content[@moduleType='Accordion']" mode="displayBrief">
    <div class="panel-group" id="accordion-{@id}">
      <xsl:apply-templates select="/Page" mode="addModule">
        <xsl:with-param name="text">Add Module</xsl:with-param>
        <xsl:with-param name="position">
          <xsl:text>accordion-</xsl:text>
          <xsl:value-of select="@id"/>
        </xsl:with-param>
        <xsl:with-param name="class">
          <xsl:text>panel-group</xsl:text>
        </xsl:with-param>
        <xsl:with-param name="id">
          <xsl:text>accordion</xsl:text>
        </xsl:with-param>
      </xsl:apply-templates>
    </div>
  </xsl:template>

  <xsl:template match="Content[starts-with(@position,'accordion')]" mode="displayModule">
    <xsl:variable name="contentPosition">
      <xsl:value-of select="@position"/>
    </xsl:variable>
    <xsl:variable name="containerID">
      <xsl:value-of select="substring-after(@position, '-')"/>
    </xsl:variable>
    <xsl:variable name="open">
      <xsl:choose>
        <xsl:when test="/Page/descendant::Content[@id=$containerID and @open='true']">
          <xsl:text>true</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>false</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div id="mod_{@id}" class="panel panel-default">
      <!-- define classes for box -->
      <xsl:attribute name="class">
        <xsl:text>panel </xsl:text>
        <xsl:choose>
          <xsl:when test="@box='Default Box'">panel-default</xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="translate(@box,' ','-')"/>
          </xsl:otherwise>
        </xsl:choose>
        <xsl:text> module</xsl:text>
        <!-- if no title, we may still want TL/TR for rounded boxs with no title bar,
              stled differently to a title bar. -->
        <xsl:if test="@title=''">
          <xsl:text> boxnotitle</xsl:text>
        </xsl:if>
        pos-<xsl:value-of select="@position"/>
        <xsl:if test="@modAnim and @modAnim!=''">
          <xsl:text> moduleAnimate-invisible</xsl:text>
        </xsl:if>
        <xsl:apply-templates select="." mode="hideScreens" />
        <xsl:apply-templates select="." mode="marginBelow" />
      </xsl:attribute>
      <div class="panel-heading">
        <xsl:apply-templates select="." mode="inlinePopupOptions">
          <xsl:with-param name="class" select="'panel-heading'"/>
        </xsl:apply-templates>
        <xsl:if test="@rss and @rss!='false'">
          <xsl:apply-templates select="." mode="rssLink" />
        </xsl:if>
        <a data-toggle="collapse" data-parent="#{@position}" href="#collapse{@id}" class="accordion-load">
          <xsl:if test="$open='true'">
            <xsl:if test="count(./preceding-sibling::Content[@position=$contentPosition])=0">
              <xsl:attribute name="class">
                <xsl:value-of select="@position"/>
                <xsl:text> accordion-open</xsl:text>
              </xsl:attribute>
            </xsl:if>
          </xsl:if>
          <h3 class="panel-title">
            <!--<i class="fa fa-ellipsis-v">&#160;</i>-->
            <i class="fa fa-caret-down">
              <xsl:text> </xsl:text>
            </i>
            <span class="space">&#160;</span>
            <!--<xsl:apply-templates select="." mode="getDisplayName"/>-->
            <xsl:value-of select="@title"/>
          </h3>
        </a>
      </div>
      <div id="collapse{@id}" class="panel-collapse collapse">
        <xsl:if test="$open='true'">
          <xsl:if test="count(./preceding-sibling::Content[@position=$contentPosition])=0">
            <xsl:attribute name="class">
              <xsl:value-of select="@position"/>
              <xsl:text> panel-collapse collapse in</xsl:text>
            </xsl:attribute>
          </xsl:if>
        </xsl:if>
        <xsl:if test="not(@listGroup='true')">
          <div class="panel-body">
            <xsl:if test="not(@title!='')">
              <xsl:apply-templates select="." mode="inlinePopupOptions">
                <xsl:with-param name="class" select="'panel-body'"/>
              </xsl:apply-templates>
            </xsl:if>
            <xsl:apply-templates select="." mode="displayBrief"/>
          </div>
        </xsl:if>
        <xsl:if test="@listGroup='true'">
          <div class="list-group">
            <xsl:if test="not(@title!='')">
              <xsl:apply-templates select="." mode="inlinePopupOptions">
                <xsl:with-param name="class" select="'list-group'"/>
              </xsl:apply-templates>
            </xsl:if>
            <xsl:apply-templates select="." mode="displayBrief"/>
          </div>
        </xsl:if>
        <xsl:if test="@linkText!='' and @link!=''">
          <div class="panel-footer">
            <div class="entryFooter">
              <xsl:apply-templates select="." mode="moreLink">
                <xsl:with-param name="link">
                  <xsl:choose>
                    <xsl:when test="format-number(@link,'0')!='NaN'">
                      <xsl:variable name="pageId" select="@link"/>
                      <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]" mode="getHref"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="@link"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:with-param>
                <xsl:with-param name="linkText" select="@linkText"/>
                <xsl:with-param name="altText" select="@title"/>
              </xsl:apply-templates>
              <xsl:text> </xsl:text>
            </div>
          </div>
        </xsl:if>
      </div>
    </div>
  </xsl:template>

  <!-- TABBED-->
  <xsl:template match="Content[@moduleType='Tabbed']" mode="displayBrief">
    <xsl:variable name="containerID">
      <xsl:value-of select="@id"/>
    </xsl:variable>
    <ul class="nav nav-tabs responsive">
      <!--<xsl:for-each select="/Page/Contents/Content[starts-with(@position,'tabbed')]">-->
      <xsl:for-each select="/Page/Contents/Content[contains(@position, $containerID)]">
        <li>
          <xsl:if test="count(./preceding-sibling::Content[contains(@position, $containerID)])=0">
            <xsl:attribute name="class">
              <xsl:text>active</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <a href="#{@id}" data-toggle="tab">
            <xsl:if test="@icon!=''">
              <i>
                <xsl:attribute name="class">
                  <xsl:text>fa fa-3x center-block </xsl:text>
                  <xsl:value-of select="@icon"/>
                </xsl:attribute>
                <xsl:text> </xsl:text>
              </i>
              <xsl:text> </xsl:text>
            </xsl:if>
            <xsl:if test="@uploadIcon!='' and @uploadIcon!='_'">
              <span class="upload-icon">
                <img src="{@uploadIcon}" alt="icon" class="center-block img-responsive"/>
              </span>
            </xsl:if>
            <!--<xsl:apply-templates select="." mode="getDisplayName"/>-->
            <xsl:value-of select="@title"/>
          </a>
        </li>
      </xsl:for-each>
    </ul>
    <div id="tabbed-{@id}" class="tab-content responsive">
      <xsl:apply-templates select="/Page" mode="addModule">
        <xsl:with-param name="text">Add Tab</xsl:with-param>
        <xsl:with-param name="position">
          <xsl:text>tabbed-</xsl:text>
          <xsl:value-of select="@id"/>
        </xsl:with-param>
        <xsl:with-param name="class">
          <xsl:text>tab-content</xsl:text>
        </xsl:with-param>
        <xsl:with-param name="id">
          <xsl:text>tabbed</xsl:text>
        </xsl:with-param>
      </xsl:apply-templates>
      <xsl:text> </xsl:text>
    </div>
  </xsl:template>

  <!--tabbed with and without box-->
  <xsl:template match="Content[starts-with(@position,'tabbed')]" mode="displayModule">
    <xsl:variable name="contentPosition">
      <xsl:value-of select="@position"/>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="@box!='false' and @box!=''">
        <xsl:apply-templates select="." mode="moduleBox"/>
      </xsl:when>
      <xsl:otherwise>
        <div class="tab-pane {$contentPosition}">
          <xsl:attribute name="id">
            <xsl:value-of select="@id"/>
          </xsl:attribute>
          <xsl:if test="count(./preceding-sibling::Content[@position=$contentPosition])=0">
            <xsl:attribute name="class">
              <xsl:value-of select="@position"/>
              <xsl:text> tab-pane active</xsl:text>
            </xsl:attribute>
          </xsl:if>
          <div id="mod_{@id}" class="module nobox pos-{@position}">
            <xsl:attribute name="class">
              <xsl:text>module nobox pos-</xsl:text>
              <xsl:value-of select="@position"/>
              <xsl:if test="@modAnim and @modAnim!=''">
                <xsl:text> moduleAnimate-invisible</xsl:text>
              </xsl:if>
              <xsl:apply-templates select="." mode="hideScreens" />
              <xsl:apply-templates select="." mode="marginBelow" />
            </xsl:attribute>
            <xsl:if test="@contentType='Module'">
              <xsl:attribute name="class">
                <xsl:text>module nobox layoutModule pos-</xsl:text>
                <xsl:value-of select="@position"/>
                <xsl:if test="@modAnim and @modAnim!=''">
                  <xsl:text> moduleAnimate-invisible</xsl:text>
                </xsl:if>
                <xsl:apply-templates select="." mode="hideScreens" />
                <xsl:apply-templates select="." mode="marginBelow" />
              </xsl:attribute>
            </xsl:if>
            <xsl:choose>
              <xsl:when test="/Page/AdminMenu/descendant-or-self::MenuItem[@cmd='Normal'] and $adminMode">
                <div>
                  <xsl:apply-templates select="." mode="inlinePopupOptions" />
                  <xsl:text> </xsl:text>
                  <xsl:if test="(@title!='' and @moduleType!='Image') or @icon!='' or @uploadIcon!=''">
                    <h3 class="title">
                      <xsl:apply-templates select="." mode="moduleLink"/>
                    </h3>
                  </xsl:if>
                </div>
              </xsl:when>
              <xsl:otherwise>
                <xsl:if test="(@title!='' and @moduleType!='Image') or @icon!='' or @uploadIcon!=''">
                  <h3 class="title">
                    <xsl:apply-templates select="." mode="moduleLink"/>
                  </h3>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>
            <xsl:if test="@rss and @rss!='false'">
              <xsl:apply-templates select="." mode="rssLink" />
            </xsl:if>
            <div class="terminus">&#160;</div>
            <xsl:apply-templates select="." mode="displayBrief"/>
            <xsl:if test="@linkText!='' and @link!=''">
              <div class="entryFooter">
                <xsl:apply-templates select="." mode="moreLink">
                  <xsl:with-param name="link">
                    <xsl:choose>
                      <xsl:when test="format-number(@link,'0')!='NaN'">
                        <xsl:variable name="pageId" select="@link"/>
                        <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]" mode="getHref"/>
                      </xsl:when>
                      <xsl:otherwise>
                        <xsl:value-of select="@link"/>
                      </xsl:otherwise>
                    </xsl:choose>
                  </xsl:with-param>
                  <xsl:with-param name="linkText" select="@linkText"/>
                  <xsl:with-param name="altText" select="@title"/>
                </xsl:apply-templates>
                <xsl:text> </xsl:text>
              </div>
            </xsl:if>
            <div class="terminus">&#160;</div>
          </div>
        </div>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!--end tabbed with and without box-->

  <xsl:template match="Content[starts-with(@position,'tabbed')]" mode="moduleBox">
    <xsl:variable name="contentPosition">
      <xsl:value-of select="@position"/>
    </xsl:variable>
    <div class="tab-pane {$contentPosition}">
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:if test="count(./preceding-sibling::Content[@position=$contentPosition])=0">
        <xsl:attribute name="class">
          <xsl:value-of select="@position"/>
          <xsl:text> tab-pane active</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <div id="mod_{@id}" class="panel panel-default">
        <!-- define classes for box -->
        <xsl:attribute name="class">
          <xsl:text>panel </xsl:text>
          <xsl:choose>
            <xsl:when test="@box='Default Box'">panel-default</xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="translate(@box,' ','-')"/>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:text> module</xsl:text>
          <!-- if no title, we may still want TL/TR for rounded boxs with no title bar,
              stled differently to a title bar. -->
          <xsl:if test="@title=''">
            <xsl:text> boxnotitle</xsl:text>
          </xsl:if>
          pos-<xsl:value-of select="@position"/>
          <xsl:apply-templates select="." mode="hideScreens" />
          <xsl:apply-templates select="." mode="marginBelow" />
        </xsl:attribute>
        <div class="panel-heading">
          <xsl:apply-templates select="." mode="inlinePopupOptions">
            <xsl:with-param name="class" select="'panel-heading'"/>
          </xsl:apply-templates>
          <xsl:if test="@rss and @rss!='false'">
            <xsl:apply-templates select="." mode="rssLink" />
          </xsl:if>
          <h3 class="panel-title">
            <xsl:apply-templates select="." mode="getDisplayName"/>
          </h3>
        </div>
        <xsl:if test="not(@listGroup='true')">
          <div class="panel-body">
            <xsl:if test="not(@title!='')">
              <xsl:apply-templates select="." mode="inlinePopupOptions">
                <xsl:with-param name="class" select="'panel-body'"/>
              </xsl:apply-templates>
            </xsl:if>
            <xsl:apply-templates select="." mode="displayBrief"/>
          </div>
        </xsl:if>
        <xsl:if test="@listGroup='true'">
          <div class="list-group">
            <xsl:if test="not(@title!='')">
              <xsl:apply-templates select="." mode="inlinePopupOptions">
                <xsl:with-param name="class" select="'list-group'"/>
              </xsl:apply-templates>
            </xsl:if>
            <xsl:apply-templates select="." mode="displayBrief"/>
          </div>
        </xsl:if>
        <xsl:if test="@linkText!='' and @link!=''">
          <div class="panel-footer">
            <div class="entryFooter">
              <xsl:apply-templates select="." mode="moreLink">
                <xsl:with-param name="link">
                  <xsl:choose>
                    <xsl:when test="format-number(@link,'0')!='NaN'">
                      <xsl:variable name="pageId" select="@link"/>
                      <xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=$pageId]" mode="getHref"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="@link"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:with-param>
                <xsl:with-param name="linkText" select="@linkText"/>
                <xsl:with-param name="altText" select="@title"/>
              </xsl:apply-templates>
              <xsl:text> </xsl:text>
            </div>
          </div>
        </xsl:if>
      </div>
    </div>
  </xsl:template>
  
</xsl:stylesheet>
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!--responsive column settings-->
  <xsl:template match="*" mode="responsiveAutoColumns">
    <xsl:choose>
      <xsl:when test="(@xsCol!='' and @xsCol) or (@smCol!='' and @smCol) or (@mdCol!='' and @mdCol) or (@lgCol!='' and @lgCol) or (@xlCol!='' and @xlCol) or (@xxlCol!='' and @xxlCol)">
        <xsl:if test="@xsCol!='' and @xsCol">
          <xsl:text> row-cols-</xsl:text>
          <xsl:value-of select="@xsCol"/>
        </xsl:if>
        <xsl:if test="@smCol!='' and @smCol">
          <xsl:text> row-cols-sm-</xsl:text>
          <xsl:value-of select="@smCol"/>
        </xsl:if>
        <xsl:if test="@mdCol!='' and @mdCol">
          <xsl:text> row-cols-md-</xsl:text>
          <xsl:value-of select="@mdCol"/>
        </xsl:if>
        <xsl:if test="@lgCol!='' and @lgCol">
          <xsl:text> row-cols-lg-</xsl:text>
          <xsl:value-of select="@lgCol"/>
        </xsl:if>
        <xsl:if test="@xlCol!='' and @xlCol">
          <xsl:text> row-cols-xl-</xsl:text>
          <xsl:value-of select="@xlCol"/>
        </xsl:if>
        <xsl:if test="@xxlCol!='' and @xxlCol">
          <xsl:text> row-cols-xxl-</xsl:text>
          <xsl:value-of select="@xxlCol"/>
        </xsl:if>
      </xsl:when>
      <xsl:otherwise> row-cols-auto</xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template match="*" mode="responsiveColumns-bs5">
    <xsl:param name="defaultCols"/>
    <xsl:variable name="xsColsEven">
      <xsl:choose>
        <xsl:when test="@xsCol='2'">col-6 </xsl:when>
        <xsl:when test="@xsCol='3'">col-4 </xsl:when>
        <xsl:when test="@xsCol='4'">col-3 </xsl:when>
        <xsl:when test="@xsCol='5'">col-5th </xsl:when>
        <xsl:when test="@xsCol='6'">col-2 </xsl:when>
        <xsl:otherwise>col-12 </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="smColsEven">
      <xsl:choose>
        <xsl:when test="@smCol='1'">col-sm-12 </xsl:when>
        <xsl:when test="@smCol='2'">col-sm-6 </xsl:when>
        <xsl:when test="@smCol='3'">col-sm-4 </xsl:when>
        <xsl:when test="@smCol='4'">col-sm-3 </xsl:when>
        <xsl:when test="@smCol='5'">col-sm-5th </xsl:when>
        <xsl:when test="@smCol='6'">col-sm-2 </xsl:when>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="mdColsEven">
      <xsl:if test="@mdCol='1'">col-md-12 </xsl:if>
      <xsl:if test="@mdCol='2'">col-md-6 </xsl:if>
      <xsl:if test="@mdCol='3'">col-md-4 </xsl:if>
      <xsl:if test="@mdCol='4'">col-md-3 </xsl:if>
      <xsl:if test="@mdCol='5'">col-md-5th </xsl:if>
      <xsl:if test="@mdCol='6'">col-md-2 </xsl:if>
    </xsl:variable>
    <xsl:variable name="lgColsEven">
      <xsl:if test="@lgCol='1'">col-lg-12 </xsl:if>
      <xsl:if test="@lgCol='2'">col-lg-6 </xsl:if>
      <xsl:if test="@lgCol='3'">col-lg-4 </xsl:if>
      <xsl:if test="@lgCol='4'">col-lg-3 </xsl:if>
      <xsl:if test="@lgCol='5'">col-lg-5th </xsl:if>
      <xsl:if test="@lgCol='6'">col-lg-2 </xsl:if>
    </xsl:variable>
    <xsl:variable name="xlColsEven">
      <xsl:if test="@xlCol='1'">col-xl-12 </xsl:if>
      <xsl:if test="@xlCol='2'">col-xl-6 </xsl:if>
      <xsl:if test="@xlCol='3'">col-xl-4 </xsl:if>
      <xsl:if test="@xlCol='4'">col-xl-3 </xsl:if>
      <xsl:if test="@xlCol='5'">col-xl-5th </xsl:if>
      <xsl:if test="@xlCol='6'">col-xl-2 </xsl:if>
    </xsl:variable>
    <xsl:variable name="xxlColsEven">
      <xsl:if test="@xxlCol='1'">col-xxl-12 </xsl:if>
      <xsl:if test="@xxlCol='2'">col-xxl-6 </xsl:if>
      <xsl:if test="@xxlCol='3'">col-xxl-4 </xsl:if>
      <xsl:if test="@xxlCol='4'">col-xxl-3 </xsl:if>
      <xsl:if test="@xxlCol='5'">col-xxl-5th </xsl:if>
      <xsl:if test="@xxlCol='6'">col-xxl-2 </xsl:if>
    </xsl:variable>

    <xsl:if test="@xsCol and @xsCol!=''">
      <xsl:value-of select="$xsColsEven"/>
    </xsl:if>
    <xsl:if test="@smCol and @smCol!=''">
      <xsl:value-of select="$smColsEven"/>
    </xsl:if>
    <xsl:if test="@mdCol and @mdCol!=''">
      <xsl:value-of select="$mdColsEven"/>
    </xsl:if>
    <xsl:if test="@lgCol and @lgCol!=''">
      <xsl:value-of select="$lgColsEven"/>
    </xsl:if>
    <xsl:if test="@xlCol and @xlCol!=''">
      <xsl:value-of select="$xlColsEven"/>
    </xsl:if>
    <xsl:if test="@xxlCol and @xxlCol!=''">
      <xsl:value-of select="$xxlColsEven"/>
    </xsl:if>
  </xsl:template>

  <xsl:template match="*" mode="unevenColumns">
    <xsl:param name="defaultWidth"/>
    <xsl:variable name="smColsUneven">
      <xsl:choose>
        <xsl:when test="@smCol='2'">
          <xsl:text>col-sm-</xsl:text>
          <xsl:value-of select="$defaultWidth"/>
          <xsl:text> </xsl:text>
        </xsl:when>
        <xsl:when test="@smCol='2equal'">col-sm-6 </xsl:when>
        <xsl:otherwise>sm-single-col </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="mdColsUneven">
      <xsl:if test="@mdCol='2'">
        <xsl:text>col-md-</xsl:text>
        <xsl:value-of select="$defaultWidth"/>
        <xsl:text> </xsl:text>
      </xsl:if>
      <xsl:if test="@mdCol='2equal'">col-md-6 </xsl:if>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="@xsCol='2'">mobile-2-col </xsl:when>
      <xsl:otherwise>mobile-1-col </xsl:otherwise>
    </xsl:choose>
    <xsl:if test="@smCol and @smCol!=''">
      <xsl:value-of select="$smColsUneven"/>
    </xsl:if>
    <xsl:choose>
      <xsl:when test="@mdCol and @mdCol!=''">
        <xsl:value-of select="$mdColsUneven"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>col-md-</xsl:text>
        <xsl:value-of select="$defaultWidth"/>
        <xsl:text> </xsl:text>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text>col-lg-</xsl:text>
    <xsl:value-of select="$defaultWidth"/>
    <xsl:text> </xsl:text>
  </xsl:template>

  <xsl:template match="*" mode="uneven5Columns">
    <xsl:param name="defaultWidth"/>
    <xsl:variable name="smColsUneven">
      <xsl:choose>
        <xsl:when test="@smCol='2'">
          <xsl:text>col-sm-</xsl:text>
          <xsl:value-of select="$defaultWidth"/>
          <xsl:text> </xsl:text>
        </xsl:when>
        <xsl:when test="@smCol='2equal'">col-sm-equal-2 </xsl:when>
        <xsl:otherwise>sm-single-col </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="mdColsUneven">
      <xsl:if test="@mdCol='2'">
        <xsl:text>col-md-</xsl:text>
        <xsl:value-of select="$defaultWidth"/>
        <xsl:text> </xsl:text>
      </xsl:if>
      <xsl:if test="@mdCol='2equal'">col-md-equal-2 </xsl:if>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="@xsCol='2'">mobile-2-col </xsl:when>
      <xsl:otherwise>mobile-1-col </xsl:otherwise>
    </xsl:choose>
    <xsl:if test="@smCol and @smCol!=''">
      <xsl:value-of select="$smColsUneven"/>
    </xsl:if>
    <xsl:choose>
      <xsl:when test="@mdCol and @mdCol!=''">
        <xsl:value-of select="$mdColsUneven"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>col-md-</xsl:text>
        <xsl:value-of select="$defaultWidth"/>
        <xsl:text> </xsl:text>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text>col-lg-</xsl:text>
    <xsl:value-of select="$defaultWidth"/>
    <xsl:text> </xsl:text>
  </xsl:template>


  <xsl:template match="Content[@moduleType='1Column' or @moduleType='1column' or @moduleType='Conditional1Column']" mode="displayBrief">
    <div class="row">
      <!--<xsl:if test="@flexbox='true'">
        <xsl:attribute name="class">
          row flexbox-columns flexbox-cols-<xsl:value-of select="@flexColumns"/>
        </xsl:attribute>
      </xsl:if>-->
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

  <xsl:template match="Content[@moduleType='AutoColumn']" mode="displayBrief">
    <xsl:variable name="responsiveAutoColumns">
      <xsl:apply-templates select="." mode="responsiveAutoColumns"/>
    </xsl:variable>
    <div id="column1-{@id}" class="row {$responsiveAutoColumns} justify-content-{@alignment}">

      <xsl:apply-templates select="/Page" mode="addModule">
        <xsl:with-param name="text">Add Module</xsl:with-param>
        <xsl:with-param name="position">
          <xsl:text>column1-</xsl:text>
          <xsl:value-of select="@id"/>
        </xsl:with-param>

        <xsl:with-param name="class">
          <!--<xsl:value-of select="$responsiveColumns-bs5"/>-->
          <xsl:text> row justify-content-</xsl:text>
          <xsl:value-of select="@alignment"/>
          <xsl:value-of select="$responsiveAutoColumns"/>
        </xsl:with-param>
        <xsl:with-param name="width">
          <xsl:value-of select="@width"/>
        </xsl:with-param>
        <xsl:with-param name="module-type">
          <xsl:value-of select="@moduleType"/>
        </xsl:with-param>
      </xsl:apply-templates>
    </div>
  </xsl:template>

  <xsl:template match="Content[@moduleType='2Columns5050' or @moduleType='2columns5050']" mode="displayBrief">
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


  <xsl:template match="Content[@moduleType='MultiColumn']" mode="displayBrief">
    <xsl:variable name="responsiveColumns-bs5">
      <xsl:apply-templates select="." mode="responsiveColumns-bs5">
        <xsl:with-param name="defaultCols" select="'2'"/>
      </xsl:apply-templates>
    </xsl:variable>
    <div class="row">
      <div id="column1-{@id}" class="column1 {$responsiveColumns-bs5}">
        <xsl:apply-templates select="/Page" mode="addModule">
          <xsl:with-param name="text">Add Module</xsl:with-param>
          <xsl:with-param name="position">
            <xsl:text>column1-</xsl:text>
            <xsl:value-of select="@id"/>
          </xsl:with-param>
          <xsl:with-param name="class">
            <xsl:text>column1 </xsl:text>
            <xsl:value-of select="$responsiveColumns-bs5"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </div>
      <xsl:if test="@xsCol>='2' or @smCol>='2' or @mdCol>='2' or @lgCol>='2' or @xlCol>='2' or @xxlCol>='2'">
        <div id="column2-{@id}" class="column2 {$responsiveColumns-bs5}">
          <xsl:apply-templates select="/Page" mode="addModule">
            <xsl:with-param name="text">Add Module</xsl:with-param>
            <xsl:with-param name="position">
              <xsl:text>column2-</xsl:text>
              <xsl:value-of select="@id"/>
            </xsl:with-param>
            <xsl:with-param name="class">
              <xsl:text>column2 </xsl:text>
              <xsl:value-of select="$responsiveColumns-bs5"/>
            </xsl:with-param>
          </xsl:apply-templates>
        </div>
      </xsl:if>
      <xsl:if test="@xsCol>='3' or @smCol>='3' or @mdCol>='3' or @lgCol>='3' or @xlCol>='3' or @xxlCol>='3'">
        <div id="column3-{@id}" class="column3 {$responsiveColumns-bs5}">
          <xsl:apply-templates select="/Page" mode="addModule">
            <xsl:with-param name="text">Add Module</xsl:with-param>
            <xsl:with-param name="position">
              <xsl:text>column3-</xsl:text>
              <xsl:value-of select="@id"/>
            </xsl:with-param>
            <xsl:with-param name="class">
              <xsl:text>column3 </xsl:text>
              <xsl:value-of select="$responsiveColumns-bs5"/>
            </xsl:with-param>
          </xsl:apply-templates>
        </div>
      </xsl:if>
      <xsl:if test="@xsCol>='4' or @smCol>='4' or @mdCol>='4' or @lgCol>='4' or @xlCol>='4' or @xxlCol>='4'">
        <div id="column4-{@id}" class="column4 {$responsiveColumns-bs5}">
          <xsl:apply-templates select="/Page" mode="addModule">
            <xsl:with-param name="text">Add Module</xsl:with-param>
            <xsl:with-param name="position">
              <xsl:text>column4-</xsl:text>
              <xsl:value-of select="@id"/>
            </xsl:with-param>
            <xsl:with-param name="class">
              <xsl:text>column4 </xsl:text>
              <xsl:value-of select="$responsiveColumns-bs5"/>
            </xsl:with-param>
          </xsl:apply-templates>
        </div>
      </xsl:if>
      <xsl:if test="@xsCol>='5' or @smCol>='5' or @mdCol>='5' or @lgCol>='5' or @xlCol>='5' or @xxlCol>='5'">
        <div id="column5-{@id}" class="column5 {$responsiveColumns-bs5}">
          <xsl:apply-templates select="/Page" mode="addModule">
            <xsl:with-param name="text">Add Module</xsl:with-param>
            <xsl:with-param name="position">
              <xsl:text>column5-</xsl:text>
              <xsl:value-of select="@id"/>
            </xsl:with-param>
            <xsl:with-param name="class">
              <xsl:text>column5 </xsl:text>
              <xsl:value-of select="$responsiveColumns-bs5"/>
            </xsl:with-param>
          </xsl:apply-templates>
        </div>
      </xsl:if>
      <xsl:if test="@xsCol='6' or @smCol='6' or @mdCol='6' or @lgCol='6' or @xlCol='6' or @xxlCol='6'">
        <div id="column6-{@id}" class="column6 {$responsiveColumns-bs5}">
          <xsl:apply-templates select="/Page" mode="addModule">
            <xsl:with-param name="text">Add Module</xsl:with-param>
            <xsl:with-param name="position">
              <xsl:text>column6-</xsl:text>
              <xsl:value-of select="@id"/>
            </xsl:with-param>
            <xsl:with-param name="class">
              <xsl:text>column6 </xsl:text>
              <xsl:value-of select="$responsiveColumns-bs5"/>
            </xsl:with-param>
          </xsl:apply-templates>
        </div>
      </xsl:if>
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
      <div id="mod_{@id}" class="card">
        <!-- define classes for box -->
        <xsl:attribute name="class">
          <xsl:text>card </xsl:text>
          <xsl:value-of select="translate(@box,' ','-')"/>
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
        <div class="card-heading">
          <xsl:apply-templates select="." mode="inlinePopupOptions">
            <xsl:with-param name="class" select="'panel-heading'"/>
          </xsl:apply-templates>
          <xsl:if test="@rss and @rss!='false'">
            <xsl:apply-templates select="." mode="rssLink" />
          </xsl:if>
          <h3 class="card-title">
            <xsl:apply-templates select="." mode="getDisplayName"/>
          </h3>
        </div>
        <xsl:if test="not(@listGroup='true')">
          <div class="card-body">
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
          <div class="card-footer">
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
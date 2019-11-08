<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">

  <xsl:template match="Page" mode="xform_control_scripts">
    <xsl:apply-templates select="descendant-or-self::textarea[contains(@class,'xhtml')]" mode="xform_control_script"/>
    <xsl:apply-templates select="descendant-or-self::textarea[contains(@class,'xml')]" mode="xform_control_script"/>
  </xsl:template>
  
  <xsl:template match="*" mode="xform_control_script"></xsl:template>

  <xsl:template match="Content[ancestor::Page[@adminMode='true']] | div[@class='xform' and ancestor::Page[@adminMode='true']]" mode="xform">
    <form method="{model/submission/@method}" action="">
      <xsl:attribute name="class">
        <xsl:text>ewXform panel panel-default</xsl:text>
        <xsl:if test="model/submission/@class!=''">
          <xsl:text> </xsl:text>
          <xsl:value-of select="model/submission/@class"/>
        </xsl:if>
      </xsl:attribute>
      <xsl:if test="not(contains(model/submission/@action,'.asmx'))">
        <xsl:attribute name="action">
          <xsl:value-of select="model/submission/@action"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="model/submission/@id!=''">
        <xsl:attribute name="id">
          <xsl:value-of select="model/submission/@id"/>
        </xsl:attribute>
        <xsl:attribute name="name">
          <xsl:value-of select="model/submission/@id"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="model/submission/@event!=''">
        <xsl:attribute name="onsubmit">
          <xsl:value-of select="model/submission/@event"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="descendant::upload">
        <xsl:attribute name="enctype">multipart/form-data</xsl:attribute>
      </xsl:if>
      <xsl:choose>
        <xsl:when test="count(group) = 2 and group[2]/submit and count(group[2]/*) = 1 ">
          <xsl:for-each select="group[1]">
            <xsl:if test="label[position()=1]">
              <div class="panel-heading">
                <h3 class="panel-title">
                  <xsl:copy-of select="label/node()"/>
                </h3>
              </div>
            </xsl:if>
            <div class="panel-body">
              <xsl:apply-templates select="." mode="xform"/>
              <!--xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | hint | help | alert | div" mode="xform"/-->
            </div>
          </xsl:for-each>
          <xsl:for-each select="group[2]">
            <xsl:if test="count(submit) &gt; 0">
              <div class="panel-footer clearfix">
                <xsl:if test="ancestor-or-self::Content/group/descendant-or-self::*[contains(@class,'required')]">
                  <span class="required">
                    <span class="req">*</span>
                    <xsl:text> </xsl:text>
                    <xsl:call-template name="msg_required"/>
                  </span>
                </xsl:if>
                <xsl:apply-templates select="submit" mode="xform"/>
              </div>
            </xsl:if>
          </xsl:for-each>
        </xsl:when>
        <xsl:otherwise>
          <xsl:if test="label[position()=1]">
            <div class="panel-heading">
              <h3 class="panel-title">
                <xsl:copy-of select="label/node()"/>
              </h3>
            </div>
          </xsl:if>
          <div class="panel-body">
            <xsl:apply-templates select="group | repeat " mode="xform"/>
            <xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | hint | help | alert | div" mode="xform"/>
          </div>
          <xsl:if test="count(submit) &gt; 0">
            <div class="panel-footer clearfix">
              <xsl:if test="ancestor-or-self::Content/group/descendant-or-self::*[contains(@class,'required')]">
                <!--<xsl:if test="descendant-or-self::*[contains(@class,'required')]">-->
                <span class="required">
                  <xsl:call-template name="msg_required"/>
                  <span class="req">*</span>
                </span>
              </xsl:if>
              <xsl:apply-templates select="submit" mode="xform"/>
              <!--<div class="clearfix">&#160;</div>-->
            </div>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>

    </form>
    <xsl:apply-templates select="descendant-or-self::*" mode="xform_modal"/>
  </xsl:template>

  <xsl:template match="Content[ancestor::Page[@adminMode='true'] and count(group) = 1] | div[@class='xform' and count(group) = 1 and ancestor::Page[@adminMode='true']]" mode="xform">
    <form method="{model/submission/@method}" action="">
      <xsl:attribute name="class">
        <xsl:text>ewXform panel panel-default</xsl:text>
        <xsl:if test="model/submission/@class!=''">
          <xsl:text> </xsl:text>
          <xsl:value-of select="model/submission/@class"/>
        </xsl:if>
      </xsl:attribute>
      <xsl:if test="not(contains(model/submission/@action,'.asmx'))">
        <xsl:attribute name="action">
          <xsl:value-of select="model/submission/@action"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="model/submission/@id!=''">
        <xsl:attribute name="id">
          <xsl:value-of select="model/submission/@id"/>
        </xsl:attribute>
        <xsl:attribute name="name">
          <xsl:value-of select="model/submission/@id"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="model/submission/@event!=''">
        <xsl:attribute name="onsubmit">
          <xsl:value-of select="model/submission/@event"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="descendant::upload">
        <xsl:attribute name="enctype">multipart/form-data</xsl:attribute>
      </xsl:if>
      <xsl:if test="group/label[position()=1]">
        <div class="panel-heading">
          <h3 class="panel-title">
            <xsl:apply-templates select="group/label" mode="legend"/>
          </h3>
        </div>
      </xsl:if>
      <xsl:for-each select="group">
        <div class="panel-body">
          <xsl:choose>
            <xsl:when test="contains(@class,'2col') or contains(@class,'2Col') ">
              <div class="row">
                <xsl:for-each select="group | repeat">
                  <xsl:apply-templates select="." mode="xform">
                    <xsl:with-param name="class">
                      <xsl:text>col-md-</xsl:text>
                      <xsl:choose>
                        <xsl:when test="position()='1'">4</xsl:when>
                        <xsl:when test="position()='2'">8</xsl:when>
                      </xsl:choose>
                    </xsl:with-param>
                  </xsl:apply-templates>
                </xsl:for-each>
              </div>
            </xsl:when>
            <xsl:when test="contains(@class,'2col5050') or contains(@class,'2Col5050') ">
              <div class="row">
                <xsl:for-each select="group | repeat">
                  <xsl:apply-templates select="." mode="xform">
                    <xsl:with-param name="class">
                      <xsl:text>col-md-</xsl:text>
                      <xsl:choose>
                        <xsl:when test="position()='1'">6</xsl:when>
                        <xsl:when test="position()='2'">6</xsl:when>
                      </xsl:choose>
                    </xsl:with-param>
                  </xsl:apply-templates>
                </xsl:for-each>
              </div>
            </xsl:when>
            <xsl:when test="contains(@class,'3col') or contains(@class,'3Col') ">
              <div class="row">
                <xsl:for-each select="group | repeat">
                  <xsl:apply-templates select="." mode="xform">
                    <xsl:with-param name="class">
                      <xsl:text>col-md-4</xsl:text>
                    </xsl:with-param>
                  </xsl:apply-templates>
                </xsl:for-each>
              </div>
            </xsl:when>
            <xsl:otherwise>
              <xsl:apply-templates select="group | repeat " mode="xform"/>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:apply-templates select="parent::*/alert" mode="xform"/>
          <xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | hint | help | alert | div" mode="xform"/>

        </div>
        <xsl:if test="count(submit) &gt; 0">
          <div class="panel-footer clearfix">
            <xsl:if test="ancestor-or-self::group/descendant-or-self::*[contains(@class,'required')]">
              <!--<xsl:if test="descendant-or-self::*[contains(@class,'required')]">-->
              <span class="required">
                <span class="req">*</span>
                <xsl:text> </xsl:text>
                <xsl:call-template name="msg_required"/>
              </span>
            </xsl:if>
            <xsl:apply-templates select="submit" mode="xform"/>
            <!--<div class="clearfix">&#160;</div>-->
          </div>
        </xsl:if>
      </xsl:for-each>
    </form>
    <xsl:apply-templates select="descendant-or-self::*" mode="xform_modal"/>
  </xsl:template>

  <xsl:template match="label[ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="legend">
    <legend>
      <xsl:apply-templates select="./node()" mode="cleanXhtml"/>
    </legend>
  </xsl:template>

  <xsl:template match="label[ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="xform">
    <label>
      <xsl:apply-templates select="./node()" mode="cleanXhtml"/>
    </label>
  </xsl:template>

  <xsl:template match="label[ancestor::Page[@cssFramework='bs3' or @adminMode='true']]">
    <xsl:param name="cLabel"/>
    <xsl:param name="bRequired"/>
    <xsl:if test ="./node()!=''">
      <label>
        <xsl:if test="$cLabel!=''">
          <xsl:attribute name="for">
            <xsl:value-of select="$cLabel"/>
          </xsl:attribute>
        </xsl:if>
        <xsl:attribute name="class">
          <xsl:if test="$bRequired='true' and not(ancestor::select1[@appearance='full' and value/node()!=''])">
            <xsl:text> required</xsl:text>
          </xsl:if>
          <xsl:if test="parent::input[contains(@class,'readonly')]">
            <xsl:text> readonly</xsl:text>
          </xsl:if>
        </xsl:attribute>
        <xsl:for-each select="@*">
          <xsl:attribute name="{name()}">
            <xsl:value-of select="." />
          </xsl:attribute>
        </xsl:for-each>
        <xsl:choose>
          <!-- for Multilanguage-->
          <xsl:when test="span[contains(@class,'term')]">
            <xsl:apply-templates select="span" mode="term" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="./node()" mode="cleanXhtml"/>
          </xsl:otherwise>
        </xsl:choose>
        <!--<xsl:value-of select="./node()"/>-->
        <xsl:if test="$bRequired='true' and not(ancestor::select1[@appearance='full' and value/node()!=''])">
          <span class="req">*</span>
        </xsl:if>
      </label>
    </xsl:if>
  </xsl:template>

  <xsl:template match="label[parent::input[contains(@class,'hidden')]]">

  </xsl:template>

  <xsl:template match="group[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | repeat[ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="xform">
    <xsl:param name="class"/>
    <fieldset>
      <xsl:if test=" @id!='' ">
        <xsl:attribute name="id">
          <xsl:value-of select="@id"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="$class!='' or @class!='' ">
        <xsl:attribute name="class">
          <xsl:value-of select="$class"/>
          <xsl:if test="@class!=''">
            <xsl:text> </xsl:text>
            <xsl:value-of select="@class"/>
          </xsl:if>
          <xsl:for-each select="group">
            <xsl:text> form-group li-</xsl:text>
            <xsl:value-of select="./@class"/>
          </xsl:for-each>
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates select="." mode="editXformMenu"/>
      <xsl:apply-templates select="label[position()=1]" mode="legend"/>
      <xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | group | repeat | hint | help | alert | div | repeat | relatedContent | label[position()!=1] | trigger | script" mode="control-outer"/>

      <xsl:if test="count(submit) &gt; 0">

        <xsl:choose>
          <xsl:when test="contains(@class,'form-inline')">
            <xsl:apply-templates select="submit" mode="xform"/>
            <!-- Terminus needed for CHROME ! -->
            <!-- Terminus needed for BREAKS IE 7! -->
            <xsl:if test="$browserVersion!='MSIE 7.0'">
              <div class="terminus">&#160;</div>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <div class="form-actions">
              <xsl:if test="not(submit[contains(@class,'hideRequired')])">
                <xsl:if test="ancestor::group/descendant-or-self::*[contains(@class,'required')]">
                  <label class="required">
                    <span class="req">*</span>
                    <xsl:text> </xsl:text>
                    <xsl:call-template name="msg_required"/>
                  </label>
                </xsl:if>
              </xsl:if>
              <!-- For xFormQuiz change how these buttons work -->
              <xsl:apply-templates select="submit" mode="xform"/>
              <!-- Terminus needed for CHROME ! -->
              <!-- Terminus needed for BREAKS IE 7! -->
              <xsl:if test="$browserVersion!='MSIE 7.0'">
                <div class="terminus">&#160;</div>
              </xsl:if>
            </div>
          </xsl:otherwise>
        </xsl:choose>
        
        
      </xsl:if>
    </fieldset>
  </xsl:template>


  <xsl:template match="input | secret | select | select1 | range | textarea | upload | group | repeat | hint | help | alert | div | repeat | relatedContent | label[position()!=1] | trigger | script" mode="control-outer">
    <xsl:choose>
      <xsl:when test="name()='group'">
        <xsl:apply-templates select="." mode="xform"/>
      </xsl:when>
      <xsl:when test="name()='alert'">
        <xsl:apply-templates select="." mode="xform"/>
      </xsl:when>
      <xsl:when test="contains(@class,'hidden')">
        <div class="form-group hidden">
          <xsl:apply-templates select="." mode="xform"/>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <div>
          <xsl:attribute name="class">
            <xsl:choose>
              <xsl:when test="name()='div'">
                <xsl:text>form-text</xsl:text>
              </xsl:when>
              <xsl:otherwise>
                <xsl:text>form-group </xsl:text>
                <xsl:if test="name()='input'">
                  <xsl:value-of select="name()"/>
                  <xsl:text>-containing </xsl:text>
                </xsl:if>
                <xsl:if test="name()!='input'">
                  <xsl:value-of select="name()"/>
                  <xsl:text>-group </xsl:text>
                </xsl:if>
                <xsl:if test="name()='select'">
                  <xsl:choose>
                    <xsl:when test="name()='div'">
                      <xsl:text>form-text</xsl:text>
                    </xsl:when>
                    <xsl:when test="@appearance='full'">
                      <xsl:text>checkbox-group </xsl:text>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:text>select-group</xsl:text>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:if>
                <xsl:if test="name()='select1'">
                  <xsl:choose>
                    <xsl:when test="name()='div'">
                      <xsl:text>form-text</xsl:text>
                    </xsl:when>
                    <xsl:when test="@appearance='full'">
                      <xsl:text>radio-group </xsl:text>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:text> </xsl:text>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:if>
              </xsl:otherwise>
            </xsl:choose>
            <xsl:if test="not(contains(@class,'row'))">
              <xsl:value-of select="./@class"/>
            </xsl:if>
            <xsl:if test="alert">
              <xsl:text> alert-outer</xsl:text>
            </xsl:if>
          </xsl:attribute>
          <xsl:apply-templates select="." mode="xform"/>
        </div>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!--<xsl:template match="select" mode="control-outer">
    <xsl:choose>
      <xsl:when test="name()='group'">
        <xsl:apply-templates select="." mode="xform"/>
      </xsl:when>
      <xsl:when test="contains(@class,'hidden')">
        <div class="form-group hidden">
          <xsl:apply-templates select="." mode="xform"/>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <div>
          <xsl:attribute name="class">
            <xsl:choose>
              <xsl:when test="name()='div'">
                <xsl:text>form-text</xsl:text>
              </xsl:when>
              <xsl:when test="@appearance='full'">
                <xsl:text>form-group checkbox-group </xsl:text>
              </xsl:when>
              <xsl:otherwise>
                <xsl:text>form-group select-group</xsl:text>
              </xsl:otherwise>
            </xsl:choose>
            <xsl:if test="not(contains(@class,'row'))">
              <xsl:value-of select="./@class"/>
            </xsl:if>
            <xsl:if test="alert">
              <xsl:text> alert-outer</xsl:text>
            </xsl:if>
          </xsl:attribute>
          <xsl:apply-templates select="." mode="xform"/>
        </div>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>-->

  <!--<xsl:template match="select1[@appearance='full']" mode="control-outer">
    <xsl:choose>
      <xsl:when test="name()='group'">
        <xsl:apply-templates select="." mode="xform"/>
      </xsl:when>
      <xsl:when test="contains(@class,'hidden')">
        <div class="form-group hidden">
          <xsl:apply-templates select="." mode="xform"/>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <div>
          <xsl:attribute name="class">
            <xsl:choose>
              <xsl:when test="name()='div'">
                <xsl:text>form-text</xsl:text>
              </xsl:when>
              <xsl:otherwise>

                <xsl:text>form-group radio-group </xsl:text>
              </xsl:otherwise>
            </xsl:choose>
            <xsl:if test="not(contains(@class,'row'))">
              <xsl:value-of select="./@class"/>
            </xsl:if>
            <xsl:if test="alert">
              <xsl:text> alert-outer</xsl:text>
            </xsl:if>
          </xsl:attribute>
          <xsl:apply-templates select="." mode="xform"/>
        </div>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>-->

  <xsl:template match="group[ancestor::Page[@cssFramework='bs3' or @adminMode='true'] and contains(@class,'colapse-repeat')]" mode="xform">
    <xsl:param name="class"/>
    <xsl:variable name="mybind" select="@ref"/>
    <xsl:variable name="data-counterprefix" select="@data-counterprefix"/>
    <fieldset role="tablist" aria-multiselectable="true" class="panel-group" id="colapse-{$mybind}">
      <xsl:for-each select="repeat">
        <div class="panel panel-default">
          <div class="panel-heading" role="tab" id="heading-{$mybind}-{position()}">
            <legend class="panel-title">
              <a class="accordion-toggle" data-toggle="collapse" data-parent="#colapse-{$mybind}" href="#colapse-{$mybind}-{position()}" aria-expanded="true" aria-controls="colapse-{$mybind}-{position()}">
                <i class="fa fa-chevron-down">
                  <xsl:text> </xsl:text>
                </i>
                <xsl:text> </xsl:text>
                <xsl:value-of select="$data-counterprefix"/>
                <xsl:value-of select="position()"/>
              </a>
            </legend>
          </div>
          <div id="colapse-{$mybind}-{position()}" role="tabpanel" aria-labelledby="heading-{$mybind}-{position()}" class="panel-collapse collapse">
            <div class="panel-body">
              <xsl:apply-templates select="." mode="xform"/>
            </div>
          </div>
        </div>
      </xsl:for-each>
    </fieldset>
  </xsl:template>


  <xsl:template match="group[(contains(@class,'2col') or contains(@class,'2Col'))][ancestor::Page[@cssFramework='bs3']]" mode="xform">
    <xsl:if test="label and not(parent::Content)">
      <div class="panel-heading">
        <h3 class="panel-title">
          <xsl:copy-of select="label/node()"/>
        </h3>
      </div>
    </xsl:if>
    <fieldset class="row">
      <xsl:if test="label and ancestor::group">
        <legend class="col-md-12">
          <xsl:copy-of select="label/node()"/>
        </legend>
      </xsl:if>

      <xsl:for-each select="group">
        <xsl:apply-templates select="." mode="xform">
          <xsl:with-param name="class">
            <xsl:text>col-md-</xsl:text>
            <xsl:choose>
              <xsl:when test="position()='1'">6</xsl:when>
              <xsl:when test="position()='2'">6</xsl:when>
            </xsl:choose>
          </xsl:with-param>
        </xsl:apply-templates>
      </xsl:for-each>

    </fieldset>
  </xsl:template>

  <xsl:template match="group[(contains(@class,'2col') or contains(@class,'2Col'))][ancestor::Page[@adminMode='false' or @adminMode='true']]" mode="xform">
    <xsl:if test="label and not(parent::Content)">
      <div class="panel-heading">
        <h3 class="panel-title">
          <xsl:copy-of select="label/node()"/>
        </h3>
      </div>
    </xsl:if>
    <fieldset>
      <xsl:if test="label and ancestor::group">
        <div class="row">
          <legend class="col-md-12">
            <xsl:copy-of select="label/node()"/>
          </legend>
        </div>
      </xsl:if>
      <div class="row">
        <xsl:for-each select="group">
          <xsl:apply-templates select="." mode="xform">
            <xsl:with-param name="class">
              <xsl:text>col-md-</xsl:text>
              <xsl:choose>
                <xsl:when test="position()='1'">4</xsl:when>
                <xsl:when test="position()='2'">8</xsl:when>
              </xsl:choose>
            </xsl:with-param>
          </xsl:apply-templates>
        </xsl:for-each>
      </div>
    </fieldset>
  </xsl:template>

  <xsl:template match="group[contains(@class,'3col') or contains(@class,'3Col')][ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="xform">
    <fieldset>
      <xsl:if test="label and ancestor::group">
        <div class="row">
          <legend class="col-md-12">
            <xsl:copy-of select="label/node()"/>
          </legend>
        </div>
      </xsl:if>
      <div class="row">
        <xsl:for-each select="group">
          <xsl:apply-templates select="." mode="xform">
            <xsl:with-param name="class">
              <xsl:text>col-md-4</xsl:text>
            </xsl:with-param>
          </xsl:apply-templates>
        </xsl:for-each>
      </div>
    </fieldset>
  </xsl:template>


  <xsl:template match="input[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | secret[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | select[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | select1[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | range[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | textarea[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | upload[ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="xform">
    <xsl:apply-templates select="." mode="editXformMenu"/>

    <!-- NB : the count(item)!=1 basically stops you from making a one checkbox field (ie a boolean) from being required -->
    <xsl:apply-templates select="label">
      <xsl:with-param name="cLabel">
        <xsl:apply-templates select="." mode="getRefOrBind"/>
      </xsl:with-param>
      <xsl:with-param name="bRequired">
        <xsl:if test="contains(@class,'required') and count(item)!=1">true</xsl:if>
      </xsl:with-param>
    </xsl:apply-templates>

        <div class="control-wrapper {name()}-wrapper appearance-{@appearance} input-group">
          <xsl:attribute name="class">
            <xsl:text>control-wrapper </xsl:text>
            <xsl:value-of select="name()"/>
            <xsl:text>-wrapper appearance-</xsl:text>
            <xsl:value-of select="@appearance"/>
            <xsl:if test="help | hint">
              <xsl:text> input-group</xsl:text>
            </xsl:if>
            <xsl:if test="alert">
              <xsl:text> has-alert</xsl:text>
            </xsl:if>
          </xsl:attribute>
          <xsl:apply-templates select="." mode="xform_control"/>
          <xsl:if test="help">
            <span class="input-group-btn">
              <xsl:apply-templates select="." mode="infoButton"/>
            </span>
          </xsl:if>
          <xsl:if test="hint">
            <span class="input-group-btn">
              <xsl:apply-templates select="." mode="hintButton"/>
            </span>
          </xsl:if>
        </div>
        <xsl:apply-templates select="alert" mode="inlineAlert"/>
        
     

    <xsl:if test="not(contains(@class,'pickImage'))">
      <xsl:apply-templates select="self::node()[not(item[toggle]) and not(hint)]" mode="xform_legend"/>
    </xsl:if>
  </xsl:template>

  <xsl:template match="alert" mode="inlineAlert">
    <div class="alert-wrapper help-block">
      <i class="fa fa-exclamation-triangle">
        <xsl:text> </xsl:text>
      </i>
      <xsl:text> </xsl:text>
      <xsl:choose>
        <xsl:when test="span[contains(@class,'msg-')]">
          <!-- Send to system translations templates -->
          <xsl:apply-templates select="span" mode="term"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="node()" mode="cleanXhtml"/>
        </xsl:otherwise>
      </xsl:choose>
    </div>
  </xsl:template>

  <xsl:template match="input[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | secret[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | select[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | select1[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | range[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | textarea[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | upload[ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="xform_header">
    <xsl:variable name="bind" select="@bind"/>
    <legend>
      <xsl:apply-templates select="label">
        <xsl:with-param name="cLabel">
          <xsl:value-of select="@ref"/>
        </xsl:with-param>
        <!-- NB : the count(item)!=1 basically stops you from making a one checkbox field (ie a boolean) from being required -->
        <xsl:with-param name="bRequired">
          <xsl:if test="(contains(@class,'required') and count(item)!=1) or ancestor::Content/model/bind[@id=$bind]/@required='true()'">true</xsl:if>
        </xsl:with-param>
      </xsl:apply-templates>
    </legend>
  </xsl:template>



  <xsl:template match="input[not(contains(@class,'hidden'))][ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | secret[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | select[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | select1[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | range[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | textarea[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | upload[ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="infoButton">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="ref2">
      <xsl:value-of select="translate($ref,'/','-')"/>
    </xsl:variable>
    <div class="popover-{$ref2} popoverContent" role="tooltip">
      <xsl:copy-of select="help/node()"/>
    </div>
    <button type="button" class="btn btn-info" data-contentwrapper=".popover-{$ref2}" data-toggle="popover" data-container="body" data-placement="left" rel="frmPopover" data-original-title="{label/node()}" title="{label/node()}">
      <i class="fa fa-info">
        <xsl:text> </xsl:text>
      </i>
    </button>
  </xsl:template>


  <xsl:template match="input[not(contains(@class,'hidden'))][ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | secret[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | select[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | select1[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | range[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | textarea[ancestor::Page[@cssFramework='bs3' or @adminMode='true']] | upload[ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="hintButton">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="ref2">
      <xsl:value-of select="translate($ref,'/','-')"/>
    </xsl:variable>
    <div class="popover-{$ref2} popoverContent" role="tooltip">
      <xsl:copy-of select="hint/node()"/>
    </div>
    <button type="button" class="btn btn-primary" data-contentwrapper=".popover-{$ref2}" data-toggle="popover" data-container="body" data-placement="left" rel="frmPopover" data-original-title="{label/node()}" title="{label/node()}">
      <i class="fa fa-lightbulb-o fa-lg">
        <xsl:text> </xsl:text>
      </i>
    </button>
  </xsl:template>

  <xsl:template match="input[contains(@class,'hidden')][ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="xform">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="value">
      <xsl:apply-templates select="." mode="xform_value"/>
    </xsl:variable>
    <input type="hidden" name="{$ref}" id="{$ref}" value="{$value}"/>
  </xsl:template>

  <!-- ## Standard Select1 for Radio Buttons ########################################################### -->
  <xsl:template match="select1[@appearance='full'][ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="value">
      <xsl:value-of select="value/node()"/>
    </xsl:variable>
    <div class="form-inline">
      <!-- when Query to get select options -->
      <xsl:choose>
        <xsl:when test="contains(@class,'ewQuery')">
          <xsl:variable name="selectOptions">
            <xsl:apply-templates select="." mode="getSelectOptions"/>
          </xsl:variable>
          <xsl:apply-templates select="ms:node-set($selectOptions)/select1/*" mode="xform_radiocheck">
            <xsl:with-param name="selectedValue" select="$value"/>
            <xsl:with-param name="type">radio</xsl:with-param>
            <xsl:with-param name="ref" select="$ref"/>
          </xsl:apply-templates>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="item | choices" mode="xform_radiocheck">
            <xsl:with-param name="type">radio</xsl:with-param>
            <xsl:with-param name="ref" select="$ref"/>
          </xsl:apply-templates>
        </xsl:otherwise>
      </xsl:choose>
    </div>
  </xsl:template>

  <!-- ## Select1 for Radio Buttons with Dependant options ############################################# -->

  <xsl:template match="select1[@appearance='full' and item[toggle]][ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="control-outer">

    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>

    <xsl:variable name="value">
      <xsl:value-of select="value/node()"/>
    </xsl:variable>

    <xsl:variable name="selectedCase">
      <xsl:choose>

        <!-- If @bindTo check this isn't selected -->
        <xsl:when test="item[@bindTo]">
          <xsl:variable name="bindToItem" select="item[@bindTo]"/>
          <xsl:choose>
            <xsl:when test="item[@bindTo]/input[@bind=$bindToItem/@bindTo]/value = $bindToItem/value">
              <xsl:value-of select="$bindToItem/toggle/@case"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="item[value/node()=$value]/toggle/@case"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <!-- Default get selected case -->
          <xsl:value-of select="item[value/node()=$value]/toggle/@case"/>
        </xsl:otherwise>
      </xsl:choose>

    </xsl:variable>
    <xsl:variable name="dependantClass">
      <xsl:value-of select="translate($ref,'[]#=/','')"/>
      <xsl:text>-dependant</xsl:text>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="name()='group'">
        <xsl:apply-templates select="." mode="xform"/>
      </xsl:when>
      <xsl:when test="contains(@class,'hidden')">
        <div class="form-group hidden">
          <xsl:apply-templates select="." mode="xform"/>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <div>
          <xsl:attribute name="class">
            <xsl:choose>
              <xsl:when test="name()='div'">
                <xsl:text>form-text</xsl:text>
              </xsl:when>
              <xsl:otherwise>

                <xsl:text>form-group </xsl:text>
              </xsl:otherwise>
            </xsl:choose>
            <xsl:if test="not(contains(@class,'row'))">
              <xsl:value-of select="./@class"/>
            </xsl:if>
          </xsl:attribute>
          <xsl:apply-templates select="." mode="xform"/>
        </div>
        <!-- Output Cases - that not empty -->
        <xsl:apply-templates select="following-sibling::switch[1]/case[node()]" mode="xform" >
          <xsl:with-param name="selectedCase" select="$selectedCase" />
          <xsl:with-param name="dependantClass" select="$dependantClass" />
        </xsl:apply-templates>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="select1[@appearance='full' and item[toggle]][ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="xform_control">

    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>

    <xsl:variable name="value">
      <xsl:value-of select="value/node()"/>
    </xsl:variable>

    <xsl:variable name="selectedCase">
      <xsl:choose>

        <!-- If @bindTo check this isn't selected -->
        <xsl:when test="item[@bindTo]">
          <xsl:variable name="bindToItem" select="item[@bindTo]"/>
          <xsl:choose>
            <xsl:when test="item[@bindTo]/input[@bind=$bindToItem/@bindTo]/value = $bindToItem/value">
              <xsl:value-of select="$bindToItem/toggle/@case"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="item[value/node()=$value]/toggle/@case"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <!-- Default get selected case -->
          <xsl:value-of select="item[value/node()=$value]/toggle/@case"/>
        </xsl:otherwise>
      </xsl:choose>

    </xsl:variable>

    <xsl:variable name="dependantClass">
      <xsl:value-of select="translate($ref,'[]#=/','')"/>
      <xsl:text>-dependant</xsl:text>
    </xsl:variable>
    <div class="form-inline">
      <xsl:apply-templates select="item | choices" mode="xform_radiocheck">
        <xsl:with-param name="type">radio</xsl:with-param>
        <xsl:with-param name="ref" select="$ref"/>
        <xsl:with-param name="dependantClass">
          <xsl:value-of select="translate($ref,'[]#=/','')"/>
          <xsl:text>-dependant</xsl:text>
        </xsl:with-param>
      </xsl:apply-templates>
    </div>
    <xsl:apply-templates select="." mode="xform_legend"/>
    <xsl:if test="item[@bindTo]">
      <script>
        psuedoRadioButtonControl('<xsl:value-of select="$ref"/>','<xsl:value-of select="item[@bindTo]/@bindTo"/>','<xsl:value-of select="item[@bindTo]/value"/>');
      </script>
    </xsl:if>

  </xsl:template>


  <!-- Case -->
  <xsl:template match="case" mode="xform">
    <xsl:param name="class" />
    <xsl:param name="selectedCase" />
    <xsl:param name="dependantClass" />

    <div id="{translate(@id,'[]#=/','')}-dependant">


      <!-- IF CHOSEN CASE - HIDE-->
      <xsl:attribute name="class">
        <xsl:value-of select="$dependantClass" />
        <xsl:if test="@id!=$selectedCase and not(descendant-or-self::alert)">
          <xsl:text> hidden</xsl:text>
        </xsl:if>
      </xsl:attribute>


      <xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | group | repeat | hint | help | alert | div | repeat | relatedContent | label[position()!=1] | trigger" mode="xform"/>

      <xsl:if test="count(submit) &gt; 0">
        <div class="form-group">
          <xsl:if test="descendant-or-self::*[contains(@class,'required')]">
            <label class="required">
              <span class="req">*</span>
              <xsl:text> </xsl:text>
              <xsl:call-template name="msg_required"/>
            </label>
          </xsl:if>

          <xsl:apply-templates select="submit" mode="xform"/>
        </div>
      </xsl:if>
    </div>

  </xsl:template>



  <!-- ========================== CONTROL : Calendar ========================== -->
  <!-- -->
  <xsl:template match="input[contains(@class,'calendar')][ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="formName">
      <xsl:value-of select="ancestor::Content/model/submission/@id"/>
    </xsl:variable>
    <xsl:variable name="displayDate">
      <xsl:if test="value/node()!=''">
        <xsl:call-template name="DD_Mon_YYYY">
          <xsl:with-param name="date">
            <xsl:value-of select="value/node()"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="inlineHint">
      <xsl:apply-templates select="." mode="getInlineHint"/>
    </xsl:variable>
    <div class="controls">
      <div class="input-group">
        <input type="text" name="{$ref}" id="{$ref}" value="{value/node()}" class="hidden "/>
        <input type="text" name="{$ref}-alt" id="{$ref}-alt" value="{$displayDate}" class="jqDatePicker input-small form-control" placeholder="{$inlineHint}"/>
        <span class="input-group-btn">
          <label for="{$ref}-alt" class="input-group-addon btn btn-default">
            <i class="fa fa-calendar">
              <xsl:text> </xsl:text>
            </i>
          </label>
        </span>
      </div>
    </div>

  </xsl:template>

  <!-- ========================== CONTROL : DOB Calendar ========================== -->
  <!-- -->
  <xsl:template match="input[contains(@class,'DOBcalendar')][ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="formName">
      <xsl:value-of select="ancestor::Content/model/submission/@id"/>
    </xsl:variable>
    <xsl:variable name="displayDate">
      <xsl:if test="value/node()!=''">
        <xsl:call-template name="DD_Mon_YYYY">
          <xsl:with-param name="date">
            <xsl:value-of select="value/node()"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="inlineHint">
      <xsl:apply-templates select="." mode="getInlineHint"/>
    </xsl:variable>
    <div class="controls">
      <div class="input-group">
        <input type="text" name="{$ref}" id="{$ref}" value="{value/node()}" class="input-small jqDOBPicker form-control" placeholder="{$inlineHint}"/>
        <span class="input-group-btn">
          <label for="{$ref}" class="input-group-addon btn btn-default">
            <i class="fa fa-calendar">
              <xsl:text> </xsl:text>
            </i>
          </label>
        </span>
      </div>
    </div>

  </xsl:template>

  <!-- -->
  <xsl:template match="select1[@appearance='full' and @class='PickByImage'][ancestor::Page[@adminMode='true']]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <!--<xsl:attribute name="class">pickByImage</xsl:attribute>-->
    <!--<input type="hidden" name="{$ref}" value="{value/node()}"/>-->
    <div class="pickByImage" id="accordion">
      <xsl:apply-templates select="item | choices" mode="xform_imageClick">
        <xsl:with-param name="type">radio</xsl:with-param>
        <xsl:with-param name="ref" select="$ref"/>
      </xsl:apply-templates>
    </div>
  </xsl:template>

  <xsl:template match="choices" mode="xform_imageClick">
    <xsl:param name="type"/>
    <xsl:param name="ref"/>
    <xsl:variable name="makeClass" select="translate(label, ' ', '_')"/>

    <div class="panel panel-default">
      <a class="accordion-toggle accordion-load" data-toggle="collapse" data-parent="#accordion" href="#collapse{$makeClass}">
        <div class="panel-heading">
          <h6 class="panel-title">
            <i class="fa fa-angle-down fa-lg">
              <xsl:text> </xsl:text>
            </i>
            <xsl:text> </xsl:text>
            <xsl:if test="label/@icon">
              <i class="fa {label/@icon}">
                <xsl:text> </xsl:text>
              </i>
              <xsl:text> </xsl:text>
            </xsl:if>
   
            <xsl:apply-templates select="label" mode="xform_legend"/>
          </h6>
        </div>
      </a>
      <div id="collapse{$makeClass}">
        <xsl:attribute name="class">
          <xsl:text>panel-collapse collapse panel-body</xsl:text>
          <xsl:if test="position()=1">
            <xsl:text> in</xsl:text>
          </xsl:if>
        </xsl:attribute>
        <div class=" row choices">
          <xsl:apply-templates select="item" mode="xform_imageClick">
            <xsl:with-param name="ref" select="$ref"/>
          </xsl:apply-templates>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="item" mode="xform_imageClick">
    <xsl:param name="ref"/>
    <xsl:variable name="value" select="value/node()"/>

    <xsl:variable name="ifExists">
      <xsl:call-template name="virtual-file-exists">
        <xsl:with-param name="path" select="translate(img/@src,' ','-')"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="imageURL">
      <xsl:choose>
        <xsl:when test="$ifExists='1'">
          <xsl:value-of select="translate(img/@src,' ','-')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>/ewcommon/images/pagelayouts/tempImage.gif</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div class="col-md-4">
      <button name="{$ref}" value="{value/node()}" class="imageSelect panel panel-default">
        <img src="{$imageURL}" class="pull-left"/>
        <h5>
          <xsl:value-of select="label/node()"/>
        </h5>
        <xsl:copy-of select="div"/>
      </button>
    </div>
  </xsl:template>

  <xsl:template match="item[label/Theme]" mode="xform_imageClick">
    <xsl:param name="ref"/>

    <xsl:variable name="ifExists">
      <xsl:call-template name="virtual-file-exists">
        <xsl:with-param name="path" select="translate(label/Theme/Images/img[@class='thumbnail']/@src,' ','-')"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="imageURL">
      <xsl:choose>
        <xsl:when test="$ifExists='1'">
          <xsl:value-of select="translate(label/Theme/Images/img[@class='thumbnail']/@src,' ','-')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>/ewcommon/images/pagelayouts/webTheme.png</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="value" select="label/Theme/@name"/>

    <div class="col-md-4">
      <button name="{$ref}" value="{$value}" class="panel panel-default imageSelect">
        <xsl:if test="$value=ancestor::select1/value/node()">
          <xsl:attribute name="class">
            panel panel-default imageSelect active
          </xsl:attribute>
          <xsl:attribute name="disabled">
            disabled
          </xsl:attribute>
        </xsl:if>
        <img src="{$imageURL}" class="pull-left"/>
        <h5>
          <xsl:value-of select="label/Theme/@name"/>
        </h5>
        <div>
          <xsl:apply-templates select="label/Theme/Description/node()" mode="cleanXhtml"/>
          <!--<small>-->
          <xsl:apply-templates select="label/Theme/Attribution/node()" mode="cleanXhtml"/>
          <!--</small>-->
        </div>
      </button>
    </div>
  </xsl:template>

  <xsl:template match="hint[ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="xform">
    <div class="alert alert-success">
      <i class="fa fa-info-sign fa-2x pull-left">
        <xsl:text> </xsl:text>
      </i>
      <xsl:copy-of select="node()"/>
    </div>
  </xsl:template>

  <xsl:template match="help[ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="xform">
    <div class="alert alert-info">
      <i class="fa fa-info fa-2x pull-left">
        <xsl:text> </xsl:text>
      </i>
      <xsl:copy-of select="node()"/>
    </div>
  </xsl:template>

  <xsl:template match="alert[ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="xform">
    <xsl:param name="class"/>
    <xsl:variable name="classVal">
      <xsl:choose>
        <xsl:when test="@class!=''">
          <xsl:value-of select="@class"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$class"/>
        </xsl:otherwise>
      </xsl:choose>

    </xsl:variable>
    <div>
      <xsl:if test="@id!=''">
        <xsl:attribute name="id">
          <xsl:value-of select="@id"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:choose>
        <xsl:when test="$classVal!=''">
          <xsl:attribute name="class">
            <xsl:text>alert </xsl:text>
            <xsl:value-of select="$classVal"/>
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="class">
            <xsl:text>alert alert-danger</xsl:text>
          </xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
      <h3>
      <xsl:choose>
        <xsl:when test="$classVal='alert-success'">
          <i class="fa fa-check pull-left">
            <xsl:text> </xsl:text>
          </i>
        </xsl:when>
        <xsl:when test="$classVal!=''">
          <i class="fa fa-exclamation-triangle pull-left">
            <xsl:text> </xsl:text>
          </i>
        </xsl:when>
        <xsl:otherwise>
          <i class="fa fa-exclamation-circle pull-left">
            <xsl:text> </xsl:text>
          </i>
        </xsl:otherwise>
      </xsl:choose>
   
      <xsl:choose>

        <xsl:when test="span[contains(@class,'msg-')]">
          <!-- Send to system translations templates -->
          <xsl:apply-templates select="span" mode="term"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="node()" mode="cleanXhtml"/>
        </xsl:otherwise>
      </xsl:choose>
        </h3>
    </div>
  </xsl:template>

  <xsl:template match="div[ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="xform">
    <xsl:if test="./@class">
      <xsl:attribute name="class">
        <xsl:value-of select="./@class"/>
      </xsl:attribute>
    </xsl:if>
    <xsl:apply-templates select="node()" mode="cleanXhtml"/>
  </xsl:template>


  <!-- -->
  <xsl:template match="select1[@appearance='minimal' and contains(@class,'iconSelect')][ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <div class="bfh-selectbox pickIcon" data-name="{$ref}" data-value="{value/node()}">
      <xsl:variable name="selectOptions">
        <xsl:apply-templates select="." mode="getSelectOptions"/>
      </xsl:variable>
      <xsl:apply-templates select="ms:node-set($selectOptions)/select1/*" mode="xform_PickIcon">
        <xsl:with-param name="selectedValue" select="value/node()"/>
      </xsl:apply-templates>
      <!--<xsl:apply-templates select="item | choices" mode="xform_PickIcon">
        <xsl:with-param name="type">radio</xsl:with-param>
        <xsl:with-param name="ref" select="$ref"/>
      </xsl:apply-templates>-->
    </div>
  </xsl:template>


  <xsl:template match="choices | itemset" mode="xform_PickIcon">
    <xsl:param name="type"/>
    <xsl:param name="ref"/>
    <xsl:variable name="makeClass" select="translate(label, ' ', '_')"/>
    <!--<div class="title">
      <xsl:apply-templates select="label" mode="xform_legend"/>
    </div>-->
    <xsl:apply-templates select="item" mode="xform_PickIcon">
      <xsl:with-param name="ref" select="$ref"/>
    </xsl:apply-templates>
  </xsl:template>

  <xsl:template match="item" mode="xform_PickIcon">
    <xsl:param name="ref"/>
    <div data-value="{value/node()}">
      <i class="fa {value/node()} fa-lg">&#160;</i>
    </div>
  </xsl:template>

  <!-- -->
  <xsl:template match="select1[@appearance='minimal' and contains(@class,'boxStyle')][ancestor::Page[@cssFramework='bs3' and @adminMode='true' and not(@editContext='NormalMail')]]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <div class="bfh-selectbox boxStyle" data-name="{$ref}" data-value="{value/node()}">
      <xsl:apply-templates select="item" mode="xform_BoxStyles"/>
      <xsl:apply-templates select="." mode="siteBoxStyles">
        <xsl:with-param name="value" select="value/node()" />
      </xsl:apply-templates>
      <xsl:apply-templates select="." mode="bootstrapBoxStyles">
        <xsl:with-param name="value" select="value/node()" />
      </xsl:apply-templates>
    </div>
  </xsl:template>

  <xsl:template match="item" mode="xform_BoxStyles">
    <xsl:param name="ref"/>
    <div data-value="{value/node()}">
      <xsl:value-of select="label/node()"/>
    </div>
  </xsl:template>

  <xsl:template match="item[node()='Default Box']" mode="xform_BoxStyles">
    <xsl:param name="ref"/>
    <div data-value="{value/node()}">
      <div class="panel panel-default">
        <div class="panel-heading">
          <h6 class="panel-title">
            <xsl:value-of select="value/node()"/>
          </h6>
        </div>
        <div class="panel-body">
          <xsl:text> </xsl:text>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="*[ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="siteBoxStyles">
    <xsl:param name="value" />
    <!-- EXAMPLE BESPOKE BOX-->
    <!-- div data-value="panel-primary">
      <div class="panel panel-primary">
        <div class="panel-heading">Bespoke Box Style</div>
        <div class="panel-body">
          <xsl:text> </xsl:text>
        </div>
      </div>
    </div -->
  </xsl:template>
  <!-- -->
  <xsl:template match="*[ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="bootstrapBoxStyles">
    <xsl:param name="value" />
    <div data-value="panel-primary">
      <div class="panel panel-primary">
        <div class="panel-heading">
          <h6 class="panel-title">panel-primary</h6>
        </div>
        <div class="panel-body">
          <xsl:text>Example Text </xsl:text>
        </div>
      </div>
    </div>
    <div data-value="panel-success">
      <div class="panel panel-success">
        <div class="panel-heading">
          <h6 class="panel-title">panel-success</h6>
        </div>
        <div class="panel-body">
          <xsl:text>Example Text </xsl:text>
        </div>
      </div>
    </div>
    <div data-value="panel-info">
      <div class="panel panel-info">
        <div class="panel-heading">
          <h6 class="panel-title">panel-info</h6>
        </div>
        <div class="panel-body">
          <xsl:text>Example Text </xsl:text>
        </div>
      </div>
    </div>
    <div data-value="panel-warning">
      <div class="panel panel-warning">
        <div class="panel-heading">
          <h6 class="panel-title">panel-warning</h6>
        </div>
        <div class="panel-body">
          <xsl:text>Example Text </xsl:text>
        </div>
      </div>
    </div>
    <div data-value="panel-danger">
      <div class="panel panel-danger">
        <div class="panel-heading">
          <h6 class="panel-title">panel-danger</h6>
        </div>
        <div class="panel-body">
          <xsl:text>Example Text </xsl:text>
        </div>
      </div>
    </div>
    <div data-value="alert-action">
      <div class="alert alert-action">
        alert-action
      </div>
    </div>
    <div data-value="alert-success">
      <div class="alert alert-success">
        alert-success
      </div>
    </div>
    <div data-value="alert-info">
      <div class="alert alert-info">
        alert-info
      </div>
    </div>
    <div data-value="alert-warning">
      <div class="alert alert-warning">
        alert-warning
      </div>
    </div>
    <div data-value="alert-danger">
      <div class="alert alert-danger">
        alert-danger
      </div>
    </div>
    <div data-value="well">
      <div class="well">
        well
      </div>
    </div>
    <div data-value="well-lg">
      <div class="well well-lg">
        well-lg
      </div>
    </div>
    <div data-value="well-sm">
      <div class="well well-sm">
        well-sm
      </div>
    </div>
    <div data-value="jumbotron">
      <div class="jumbotron">
        <h1>jumbotron</h1>
        <div>Example Text</div>

      </div>
    </div>
  </xsl:template>


  <!-- -->
  <xsl:template match="select1[@appearance='minimal' and contains(@class,'bgStyle')][ancestor::Page[@cssFramework='bs3' and @adminMode='true']]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <select name="{$ref}" id="{$ref}">
      <xsl:if test="@class!=''">
        <xsl:attribute name="class">
          <xsl:value-of select="@class"/>
          <xsl:text> dropdown form-control</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@onChange!=''">
        <xsl:attribute name="onChange">
          <xsl:value-of select="@onChange"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates select="item" mode="xform_select"/>
      <xsl:apply-templates select="." mode="siteBGStyles">
        <xsl:with-param name="value" select="value/node()" />
      </xsl:apply-templates>
      <xsl:apply-templates select="." mode="bootstrapBGStyles">
        <xsl:with-param name="value" select="value/node()" />
      </xsl:apply-templates>
    </select>
  </xsl:template>

  <xsl:template match="*[ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="siteBGStyles">
    <xsl:param name="value" />
    <!-- EXAMPLE BESPOKE BOX-->
    <!--option value="testBG">
      <xsl:if test="$value='testBG'">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:text>testBG</xsl:text>
    </option-->
  </xsl:template>

  <!-- -->
  <xsl:template match="*[ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="bootstrapBGStyles">
    <xsl:param name="value" />
    <!-- THEIR ARE NO GENRIC BACKGROUNDS-->
  </xsl:template>

  <!-- -->
  <xsl:template match="select1[@appearance='minimal' and contains(@class,'cssStyle')][ancestor::Page[@cssFramework='bs3' and @adminMode='true' and not(@editContext='NormalMail')]]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>

    <div class="bfh-selectbox cssStyle" data-name="{$ref}" data-value="{value/node()}">

      <xsl:apply-templates select="." mode="siteCssStyles">
        <xsl:with-param name="value" select="value/node()" />
      </xsl:apply-templates>
      <xsl:apply-templates select="item" mode="xFormCssStyles">
        <xsl:with-param name="value" select="value/node()" />
      </xsl:apply-templates>
    </div>

  </xsl:template>

  <xsl:template match="item" mode="xFormCssStyles">
    <xsl:param name="ref"/>
    <div data-value="{value/node()}">
      <xsl:value-of select="label/node()"/>
    </div>
  </xsl:template>

  <xsl:template match="item[node()!='None']" mode="xFormCssStyles">
    <xsl:param name="ref"/>
    <div data-value="{value/node()}">
      <div class="Site">
        <div class="tp-caption {value/node()}">
          <span>
            <xsl:value-of select="label/node()"/>
          </span>
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="*" mode="siteCssStyles">
    <xsl:param name="value" />
    <!-- EXAMPLE BESPOKE BOX-->
    <!--
    <div data-value="tint_bg">
      <div class="Site">
        <div class="tp-caption tint_bg">Tinted Grey Backgroud</div>
       </div>
    </div>
    -->
  </xsl:template>

</xsl:stylesheet>

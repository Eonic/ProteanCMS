<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">

  <!-- -->
  <!-- ========================== XFORM ========================== -->
  <!-- -->
  <xsl:template match="div" mode="xform">
    <xsl:if test="./@class">
      <xsl:attribute name="class">
        <xsl:value-of select="./@class"/>
      </xsl:attribute>
    </xsl:if>
    <xsl:apply-templates select="node()" mode="cleanXhtml"/>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content | div[@class='xform']" mode="xform">
    <form method="{model/submission/@method}" action="" data-fv-framework="bootstrap"
    data-fv-icon-valid="fa fa-check"
    data-fv-icon-invalid="fa fa-times"
    data-fv-icon-validating="fa fa-refresh">
      <xsl:attribute name="class">
        <xsl:text>ewXform</xsl:text>
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

      <xsl:if test="model/instance/@valid!=''">
        <xsl:attribute name="data-fv-valid">
          <xsl:value-of select="model/instance/@valid"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="descendant::upload">
        <xsl:attribute name="enctype">multipart/form-data</xsl:attribute>
      </xsl:if>

      <!--<xsl:copy-of select="/" />-->
      <!--xsl:apply-templates select="self::Content" mode="tinyMCEinit"/-->

      <xsl:apply-templates select="group | repeat | input | secret | select | select1 | range | textarea | upload | hint | help | alert | div" mode="xform"/>

      <xsl:if test="count(submit) &gt; 0">
        <p class="buttons">
          <xsl:if test="descendant-or-self::*[contains(@class,'required')]">
            <span class="required">
              <span class="req">*</span>
              <xsl:text> </xsl:text>
              <xsl:call-template name="msg_required"/>
            </span>
          </xsl:if>
          <xsl:apply-templates select="submit" mode="xform"/>

        </p>
      </xsl:if>
      <div class="terminus">&#160;</div>
    </form>
  </xsl:template>


  <!-- -->
  <!-- ========================== GROUP ========================== -->
  <!-- -->

  <xsl:template match="group | repeat" mode="xform">
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

  <!-- Switch -->
  <xsl:template match="switch" mode="xform">
    <xsl:apply-templates select="case[node()]" mode="xform"/>
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
        <xsl:text> form-group</xsl:text>
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

  <xsl:template name="msg_required">
    <!-- required input-->
    <xsl:call-template name="term1023"/>
  </xsl:template>


  <!-- ========================== GROUP Horizontal ========================== -->
  <!-- -->
  <xsl:template match="group[contains(@class,'horizontal_cols') and parent::group] | repeat[contains(@class,'horizontal_cols') and parent::group]" mode="xform">
    <div class="responsive-table">
      <table cellspacing="0" class="table form-columns">
        <thead>
          <xsl:if test="label">
            <tr>
              <td colspan="{count(group[1]/input)+1}">
                <h3>
                  <xsl:copy-of select="label/node()"/>
                </h3>
              </td>
            </tr>
          </xsl:if>
          <xsl:apply-templates select="hint | help | alert" mode="xform">
            <xsl:with-param name="cols" select="count(group[1]/input)+1"/>
          </xsl:apply-templates>
          <tr class="horizontal_cols_header">
            <xsl:for-each select="group[1] | repeat[1]">
              <xsl:for-each select="input | secret | select | select1 | range | textarea | upload | hint | help | alert">
                <th>
                  <xsl:apply-templates select="." mode="xform_header"/>
                  <xsl:text> </xsl:text>
                </th>
              </xsl:for-each>
              </xsl:for-each>
          </tr>
        </thead>
        <tbody>
          <xsl:for-each select="group | repeat">
            <tr>
              <xsl:if test="label">
                <th>
                  <xsl:apply-templates select="label">
                    <xsl:with-param name="cLabel">
                      <xsl:value-of select="@ref"/>
                    </xsl:with-param>
                  </xsl:apply-templates>
                </th>
              </xsl:if>
              <xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | hint | help | alert | trigger" mode="xform_cols"/>
            </tr>
            <xsl:if test="*/alert or */hint or */help">
              <tr>
                <xsl:if test="label">
                  <th>&#160;</th>
                </xsl:if>
                <xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload" mode="xform_cols_notes"/>
              </tr>
            </xsl:if>
          </xsl:for-each>
        </tbody>
      </table>
    </div>
    <xsl:if test="count(submit | trigger) &gt; 0">
      <div>
        <!-- For xFormQuiz change how these buttons work -->
        <xsl:apply-templates select="submit | trigger" mode="xform"/>
      </div>
    </xsl:if>
  </xsl:template>


  <!-- -->
  <!-- ========================== GROUP In Columns ========================== -->
  <xsl:template match="group[(contains(@class,'2col') or contains(@class,'2Col'))]" mode="xform">
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


  <xsl:template match="group[contains(@class,'3col') or contains(@class,'3Col')]" mode="xform">
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

  <!-- -->
  <!-- ========================== GROUP In Tabs ========================== -->
  <xsl:template match="group[contains(@class,'xform-tabs')]" mode="xform">
    <div>
      <ul class="nav nav-tabs" role="tablist">
        <xsl:for-each select="group">
          <li role="presentation">
              <xsl:if test="position()=1"><xsl:attribute name="class">active</xsl:attribute></xsl:if>
            <a href="#tab-{@id}" aria-controls="home" role="tab" data-toggle="tab">
              <xsl:apply-templates select="label"/>
            </a>
          </li>
        </xsl:for-each>
      </ul>
      <div class="tab-content">
        <xsl:for-each select="group">
          <div role="tabpanel" class="tab-pane" id="tab-{@id}">
            <xsl:if test="position()=1"><xsl:attribute name="class">tab-pane active</xsl:attribute></xsl:if>
            <xsl:apply-templates select="." mode="xform"/>
          </div>
        </xsl:for-each>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="input | secret | select | select1 | range | textarea | upload" mode="xform_header">
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

  <!-- -->
  <xsl:template match="input | secret | select | select1 | range | textarea | upload" mode="xform_cols">
    <td>
      <xsl:apply-templates select="." mode="xform_control"/>
    </td>
  </xsl:template>

  <xsl:template match="trigger" mode="xform_cols">
    <td>
      <xsl:apply-templates select="." mode="xform"/>
    </td>
  </xsl:template>

  <!-- -->
  <xsl:template match="input | secret | select | select1 | range | textarea | upload" mode="xform_cols_notes">
    <td>
      <xsl:apply-templates select="." mode="xform_legend"/>
    </td>
  </xsl:template>

  <xsl:template match="hint" mode="xform">
    <div class="alert alert-success">
      <i class="fa fa-info-sign fa-2x pull-left">
        <xsl:text> </xsl:text>
      </i>
      <xsl:copy-of select="node()"/>
    </div>
  </xsl:template>

  <xsl:template match="help" mode="xform">
    <div class="alert alert-info">
      <i class="fa fa-info fa-2x pull-left">
        <xsl:text> </xsl:text>
      </i>
      <xsl:copy-of select="node()"/>
    </div>
  </xsl:template>

  <xsl:template match="alert" mode="xform">
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
  <!-- -->
  <!-- ========================== GENERAL : CONTROLS ========================== -->
  <!-- -->




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

  <xsl:template match="input | secret | select | select1 | range | textarea | upload" mode="xform">

    <!-- NB : the count(item)!=1 basically stops you from making a one checkbox field (ie a boolean) from being required -->
    <xsl:apply-templates select="label">
      <xsl:with-param name="cLabel">
        <xsl:apply-templates select="." mode="getRefOrBind"/>
      </xsl:with-param>
      <xsl:with-param name="bRequired">
        <xsl:if test="contains(@class,'required') and count(item)!=1">true</xsl:if>
      </xsl:with-param>
    </xsl:apply-templates>

    <xsl:variable name="fmhz">
      <xsl:if test="ancestor::group[contains(@class,'form-horizontal')]">
        <xsl:text>col-sm-9</xsl:text>
      </xsl:if>
    </xsl:variable>
    
    <div class="control-wrapper {name()}-wrapper appearance-{@appearance} {$fmhz} input-group">
      <xsl:attribute name="class">
        <xsl:text>control-wrapper </xsl:text>
        <xsl:value-of select="name()"/>
        <xsl:text>-wrapper appearance-</xsl:text>
        <xsl:value-of select="@appearance"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="$fmhz"/>
        <xsl:if test="help | hint | alert">
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
      <xsl:if test="alert">
        <span class="input-group-btn">
          <xsl:apply-templates select="." mode="alertButton"/>
        </span>
      </xsl:if>
    </div>
  
    
    <xsl:if test="not(contains(@class,'pickImage'))">
      <xsl:apply-templates select="self::node()[not(item[toggle]) and not(hint)]" mode="xform_legend"/>
    </xsl:if>
  </xsl:template>


  <!-- -->
  <!-- ========================== GENERAL : CONTROL LEGEND ========================== -->
  <!-- -->
  <xsl:template match="input[not(contains(@class,'hidden'))] | secret | select | select1 | range | textarea | upload " mode="xform_legend">
    <xsl:if test="alert">
      <xsl:apply-templates select="alert" mode="inlineAlert"/>
    </xsl:if>
    <xsl:if test="hint[@class!='inline']">
      <span class="hint">
        <xsl:copy-of select="hint/node()"/>
      </span>
    </xsl:if>
    <xsl:if test="help">
      <span class="help">
        <xsl:copy-of select="help/node()"/>
      </span>
    </xsl:if>
  </xsl:template>

  <!-- -->
  <!-- ========================== CONTROL : SUBMIT (with xFormQuiz bespokeness) ========================== -->
  <!-- -->


  <!-- Commented out to test multi-language on shopping cart -->
  
  <!--xsl:template match="submit[@ref!='']" mode="xform">
    <xsl:variable name="class">
      <xsl:text>button</xsl:text>
      <xsl:if test="@class!=''">
        <xsl:text> </xsl:text>
        <xsl:value-of select="@class"/>
      </xsl:if>
    </xsl:variable>
    <input type="submit" name="{@ref}" class="{$class}"  onclick="disableButton(this);">
      <xsl:attribute name="value">
        <xsl:apply-templates select="label" mode="submitText"/>
      </xsl:attribute>
    </input>
  </xsl:template-->

  <xsl:template match="submit" mode="xform">
    <xsl:variable name="class">
      <xsl:text>btn</xsl:text>
      <xsl:if test="not(contains(@class,'btn-'))">
        <xsl:text> btn-success</xsl:text>
      </xsl:if>
      <xsl:if test="@class!=''">
        <xsl:text> </xsl:text>
        <xsl:value-of select="@class"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="name">
      <xsl:choose>
        <xsl:when test="@ref!=''">
          <xsl:value-of select="@ref"/>
        </xsl:when>
        <xsl:when test="@submission!=''">
          <xsl:value-of select="@submission"/>
        </xsl:when>
        <xsl:when test="@bind!=''">
          <xsl:value-of select="@bind"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>ewSubmit</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="icon">
      <xsl:choose>
        <xsl:when test="@icon!=''">
          <xsl:value-of select="@icon"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>fa-check</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="buttonValue">
      <xsl:choose>
        <xsl:when test="@value!=''">
          <xsl:value-of select="@value"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="label/node()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <button type="submit" name="{$name}" value="{$buttonValue}" class="{$class}"  onclick="disableButton(this);">
      <xsl:if test="@data-pleasewaitmessage != ''">
        <xsl:attribute name="data-pleasewaitmessage">
          <xsl:value-of select="@data-pleasewaitmessage"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-pleasewaitdetail != ''">
        <xsl:attribute name="data-pleasewaitdetail">
          <xsl:value-of select="@data-pleasewaitdetail"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="not(contains($class,'icon-right'))">
        <i class="fa {$icon} fa-white">
          <xsl:text> </xsl:text>
        </i>
        <xsl:text> </xsl:text>
      </xsl:if>
      <xsl:apply-templates select="label" mode="submitText"/>
      <xsl:if test="contains($class,'icon-right')">
        <xsl:text> </xsl:text>
        <i class="fa {$icon} fa-white">
          <xsl:text> </xsl:text>
        </i>
      </xsl:if>
    </button>
  </xsl:template>

  <!-- for overloading in translations -->
  <xsl:template match="label" mode="submitText">
    <xsl:choose>
      <!-- for Multilanguage-->
      <xsl:when test="span[contains(@class,'term')]">
        <xsl:apply-templates select="span" mode="term" />
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="node()"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template match="submit[contains(@class,'principle')]" mode="xform">
    <xsl:variable name="class">
      <xsl:text>btn</xsl:text>
      <xsl:if test="not(contains(@class,'btn-'))">
        <xsl:text> btn-success</xsl:text>
      </xsl:if>
      <xsl:if test="@class!=''">
        <xsl:text> </xsl:text>
        <xsl:value-of select="@class"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="name">
      <xsl:choose>
        <xsl:when test="@ref!=''">
          <xsl:value-of select="@ref"/>
        </xsl:when>
        <xsl:when test="@submission!=''">
          <xsl:value-of select="@submission"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>ewSubmit</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="icon">
      <xsl:choose>
        <xsl:when test="@icon!=''">
          <xsl:value-of select="@icon"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>fa-check</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="buttonValue">
      <xsl:choose>
        <xsl:when test="@value!=''">
          <xsl:value-of select="@value"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="label/node()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <button type="submit" name="{$name}" value="{$buttonValue}" class="{$class}"  onclick="disableButton(this);">
      <xsl:if test="@data-pleasewaitmessage != ''">
        <xsl:attribute name="data-pleasewaitmessage">
          <xsl:value-of select="@data-pleasewaitmessage"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-pleasewaitdetail != ''">
        <xsl:attribute name="data-pleasewaitdetail">
          <xsl:value-of select="@data-pleasewaitdetail"/>
        </xsl:attribute>
      </xsl:if>
      <i class="fa {$icon} fa-white">
        <xsl:text> </xsl:text>
      </i>
      <xsl:text> </xsl:text>
      <xsl:apply-templates select="label" mode="submitText"/>
    </button>
  </xsl:template>

  <xsl:template match="submit[@class='principle' and @ref!='']" mode="xform">
    <xsl:variable name="class">
      <xsl:text>btn btn-success</xsl:text>
      <xsl:if test="@class!=''">
        <xsl:text> </xsl:text>
        <xsl:value-of select="@class"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="name">
      <xsl:choose>
        <xsl:when test="@ref!=''">
          <xsl:value-of select="@ref"/>
        </xsl:when>
        <xsl:when test="@submission!=''">
          <xsl:value-of select="@submission"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>ewSubmit</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="icon">
      <xsl:choose>
        <xsl:when test="@icon!=''">
          <xsl:value-of select="@icon"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>fa-check</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="buttonValue">
      <xsl:choose>
        <xsl:when test="@value!=''">
          <xsl:value-of select="@value"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="label/node()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <button type="submit" name="{$name}" value="{$buttonValue}" class="{$class}"  onclick="disableButton(this);">
      <xsl:if test="@data-pleasewaitmessage != ''">
        <xsl:attribute name="data-pleasewaitmessage">
          <xsl:value-of select="@data-pleasewaitmessage"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-pleasewaitdetail != ''">
        <xsl:attribute name="data-pleasewaitdetail">
          <xsl:value-of select="@data-pleasewaitdetail"/>
        </xsl:attribute>
      </xsl:if>
      <i class="fa {$icon} fa-white">
        <xsl:text> </xsl:text>
      </i>
      <xsl:text> </xsl:text>
      <xsl:apply-templates select="label" mode="submitText"/>
    </button>
  </xsl:template>

  <xsl:template match="trigger" mode="xform">
    <xsl:variable name="class">
      <xsl:text>button</xsl:text>
      <xsl:if test="@class!=''">
        <xsl:text> </xsl:text>
        <xsl:value-of select="@class"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="nParentSiblings" select="count(ancestor::repeat[1]/preceding-sibling::repeat) + count(ancestor::repeat[1]/following-sibling::repeat)"/>
    <xsl:if test="$nParentSiblings &gt; 0">
      <xsl:for-each select="delete">
        <xsl:variable name="icon">
          <xsl:choose>
            <xsl:when test="@icon!=''">
              <xsl:value-of select="@icon"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:text>fa-times</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <button type="submit" name="delete:{@bind}" value="{./parent::trigger/label/node()}" class="btn btn-danger btn-delete" onclick="disableButton(this);">
          <i class="fa {$icon} fa-white">
            <xsl:text> </xsl:text>
          </i>
          <xsl:text> </xsl:text>
          <xsl:value-of select="./parent::trigger/label/node()"/>
        </button>
      </xsl:for-each>
    </xsl:if>
    <xsl:for-each select="insert">
      <xsl:variable name="icon">
        <xsl:choose>
          <xsl:when test="@icon!=''">
            <xsl:value-of select="@icon"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>fa-plus</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:variable>
      <div class="rptInsert">
        <button type="submit" name="insert:{@bind}" value="{./parent::trigger/label/node()}" class="btn btn-primary {$class}" onclick="disableButton(this);">
          <i class="fa {$icon} fa-white">
            <xsl:text> </xsl:text>
          </i>
          <xsl:text> </xsl:text>
          <xsl:value-of select="./parent::trigger/label/node()"/>
        </button>
      </div>
    </xsl:for-each>
  </xsl:template>


  <!-- Image submit adminButton - what to do?-->
  <xsl:template match="submit[contains(@class,'image')]" mode="xform">
    <xsl:variable name="class">
      <xsl:text>button</xsl:text>
      <xsl:if test="@class!=''">
        <xsl:text> </xsl:text>
        <xsl:value-of select="@class"/>
      </xsl:if>
    </xsl:variable>
    <input type="submit" name="{label/node()}" value="{label/node()}" class="{$class}"  onclick="disableButton(this);"/>
  </xsl:template>

  <!-- -->
  <!-- ========================== CONTROL : INPUT HIDDEN ========================== -->
  <!-- -->

  <xsl:template match="input[contains(@class,'hidden')]" mode="xform">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="value">
      <xsl:apply-templates select="." mode="xform_value"/>
    </xsl:variable>
    <input type="hidden" name="{$ref}" id="{$ref}" value="{$value}"/>
  </xsl:template>
  <!-- -->
  <!-- ========================== CONTROL : INPUT TEXT ========================== -->
  <!-- -->
  <!--xsl:template match="input[contains(@class,'textbox')]" mode="xform_control"-->

  <xsl:template name="msg_required_inline">Please enter </xsl:template>

  <xsl:template match="*" mode="getInlineHint">
    <xsl:value-of select="@placeholder"/>
  </xsl:template>

  <!-- Allows for very simple overloading of default values for if a user is logged on for instance -->
  <xsl:template match="*" mode="xform_value">
    <xsl:choose>
      <xsl:when test="value!=''">
        <xsl:value-of select="value"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:apply-templates select="." mode="xform_value_alt"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Empty for Overloading -->
  <xsl:template match="*" mode="xform_value_alt">
  </xsl:template>

  <!-- Input xForm control -->
  <xsl:template match="input" mode="xform_control">
    <xsl:variable name="inlineHint">
      <xsl:apply-templates select="." mode="getInlineHint"/>
    </xsl:variable>
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="value">
      <xsl:apply-templates select="." mode="xform_value"/>
    </xsl:variable>
    <input type="text" name="{$ref}" id="{$ref}">
      <xsl:if test="contains(@class,'readonly') or contains(@class,'displayOnly') ">
        <xsl:attribute name="readonly">readonly</xsl:attribute>
      </xsl:if>
      <xsl:if test="contains(@autofocus,'autofocus')">
        <xsl:attribute name="autofocus">autofocus</xsl:attribute>
      </xsl:if>
      <xsl:choose>
        <xsl:when test="@class!=''">
          <xsl:attribute name="class">
            <xsl:value-of select="@class"/>
            <xsl:text> textbox form-control</xsl:text>
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="class">textbox form-control</xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:for-each select="@*">
        <xsl:variable name="nodename" select="name()"/>
        <xsl:if test="starts-with($nodename,'data-fv')">
          <xsl:attribute name="{name()}">
            <xsl:value-of select="." />
          </xsl:attribute>
        </xsl:if>
      </xsl:for-each>
      <xsl:choose>
        <xsl:when test="$value!=''">
          <xsl:attribute name="value">
            <xsl:value-of select="$value"/>
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:if test="$inlineHint!=''">
            <xsl:attribute name="placeholder">
              <xsl:value-of select="$inlineHint"/>
            </xsl:attribute>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:if test="@autocomplete!=''">
        <xsl:attribute name="autocomplete">
          <xsl:value-of select="@autocomplete"/>
        </xsl:attribute>
      </xsl:if>
    </input>
  </xsl:template>


  <!-- CREATE THE NAME attribute for an input field -->
  <xsl:template match="input | secret | select | select1 | range | textarea | upload" mode="getRefOrBind">
    <xsl:if test="@ref!=''">
      <xsl:value-of select="@ref"/>
    </xsl:if>
    <xsl:if test="@bind!=''">
      <xsl:value-of select="@bind"/>
    </xsl:if>

    <!-- IF CHILD OF A HIDDEN CASE add ~inactive to the ref/bind/name -->
    <xsl:variable name="caseId" select="ancestor::case[last()]/@id" />
    <xsl:variable name="thisCaseValue" select="//toggle[@case=$caseId]/preceding-sibling::value/node()" />
    <xsl:variable name="selectedValue" select="//toggle[@case=$caseId]/ancestor::select1/value" />
    <xsl:if test="$thisCaseValue!=$selectedValue">
      <xsl:text>~inactive</xsl:text>
    </xsl:if>
  </xsl:template>


  <xsl:template match="input | secret | select | select1 | range | textarea | upload" mode="getRefOrBindForScript">
    <xsl:if test="@ref!=''">
      <xsl:value-of select="@ref"/>
    </xsl:if>
    <xsl:if test="@bind!=''">
      <xsl:value-of select="@bind"/>
    </xsl:if>
  </xsl:template>


  <!-- PROCESS REQUIRED CLASS - IF Part of Switch and not shown, disable required class -->
  <xsl:template match="input | secret | select | select1 | range | textarea | upload" mode="isRequired">
    <xsl:variable name="caseId" select="ancestor::case[last()]/@id" />
    <xsl:variable name="thisCaseValue" select="//toggle[@case=$caseId]/preceding-sibling::value/node()" />
    <xsl:variable name="selectedValue" select="//toggle[@case=$caseId]/ancestor::select1[last()]/value" />
    <xsl:if test="$thisCaseValue!=$selectedValue">
      <xsl:call-template name="replace-string">
        <xsl:with-param name="text" select="@class"/>
        <xsl:with-param name="replace" select="'required'"/>
        <xsl:with-param name="with" select="'reqinactive'"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>



  <!-- Commented so uses a normal input but with readonly attribute -->
  <!--<xsl:template match="input[contains(@class,'readonly')]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <span id="rov{@bind}" class="readonlyvalue"><xsl:value-of select="value"/></span>
    <input type="hidden" name="{$ref}" id="{$ref}" value="{value}"/>
  </xsl:template>
  -->
  <!-- -->
  <xsl:template match="input[contains(@class,'readonly') and @bind='cPosition']" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>

    <xsl:variable name="value">
      <xsl:choose>
        <xsl:when test="value/node()!='' and value/node()!=' '">
          <xsl:value-of select="value"/>
        </xsl:when>
        <xsl:when test="/Page/Request/Form/Item[@name='cPosition']">
          <xsl:value-of select="/Page/Request/Form/Item[@name='cPosition']/node()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="//ContentDetail/Content/model/instance/tblContent/cContentName/node()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <span id="rov{@bind}" class="readonlyvalue">
      <xsl:value-of select="$value"/>
    </span>
    <input type="hidden" name="{$ref}" id="{$ref}" value="{$value}"/>
  </xsl:template>

  <xsl:template match="label[parent::input[contains(@type,'button') or contains(@class,'button')]]">

  </xsl:template>
  <!-- -->
  <xsl:template match="input[contains(@type,'button') or contains(@class,'button') ]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>

    <xsl:variable name="value">
      <xsl:choose>
        <xsl:when test="value/node()!='' and value/node()!=' '">
          <xsl:value-of select="value"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="//ContentDetail/Content/model/instance/tblContent/cContentName/node()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <input type="button" name="{label/node()}" id="{$ref}" value="{label/node()}">
      <xsl:if test="@class!=''">
        <xsl:attribute name="class">
          <xsl:choose>
            <xsl:when test="ancestor::switch and contains(@class,'required')">
              <xsl:apply-templates select="." mode="isRequired"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="@class"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
      </xsl:if>
    </input>
  </xsl:template>
  <!-- -->
  <!-- -->
  <!-- ========================== CONTROL : ImgVerification ========================== -->
  <!-- -->

  <xsl:template match="input[contains(@class,'capcha') or contains(@class,'imgVerification')]" mode="xform_control">
    <xsl:variable name="label_low" select="translate(label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
    <xsl:variable name="inlineHint">
      <xsl:choose>
        <xsl:when test="hint[@class='inline']">
          <xsl:value-of select="hint[@class='inline']/node()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="msg_required_inline"/>
          <xsl:value-of select="$label_low"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <input type="text" name="{$ref}" id="{$ref}">
      <xsl:choose>
        <xsl:when test="@class!=''">
          <xsl:attribute name="class">
            <xsl:choose>
              <xsl:when test="ancestor::switch and contains(@class,'required')">
                <xsl:apply-templates select="." mode="isRequired"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="@class"/>
              </xsl:otherwise>
            </xsl:choose>
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="class">textbox</xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:choose>
        <xsl:when test="value!=''">
          <xsl:attribute name="value">
            <xsl:value-of select="value"/>
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="value">
            <xsl:value-of select="$inlineHint"/>
          </xsl:attribute>
          <xsl:attribute name="onfocus">
            <xsl:text>if (this.value=='</xsl:text>
            <xsl:value-of select="$inlineHint"/>
            <xsl:text>') {this.value=''}</xsl:text>
          </xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
    </input>
    <img class="imgVerification" src="/ewcommon/tools/imgVerification.ashx"/>
  </xsl:template>


  <!-- -->
  <!-- ========================== CONTROL : CC Expire Date ========================== -->
  <!-- -->
  <xsl:template match="input[contains(@class,'ccExpire')]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="formName">
      <xsl:value-of select="ancestor::Content/model/submission/@id"/>
    </xsl:variable>
    <select name="month_{$ref}" id="month_{$ref}" onchange="makedatemmyy('{$formName}','{$ref}')" class="form-control month">
      <option value="" selected=""/>
      <option value="01">01</option>
      <option value="02">02</option>
      <option value="03">03</option>
      <option value="04">04</option>
      <option value="05">05</option>
      <option value="06">06</option>
      <option value="07">07</option>
      <option value="08">08</option>
      <option value="09">09</option>
      <option value="10">10</option>
      <option value="11">11</option>
      <option value="12">12</option>
    </select>
    <select name="year_{$ref}" onchange="makedatemmyy('{$formName}','{$ref}')" class="form-control year">
      <option value="" selected=""/>
      <xsl:call-template name="getYearDropDown">
        <xsl:with-param name="i" select="0"/>
        <xsl:with-param name="reps" select="10"/>
        <xsl:with-param name="operation" select="'+'"/>
        <xsl:with-param name="year" select="substring(/Page/Request/ServerVariables/Item[@name='Date']/node(),1,4)"/>
      </xsl:call-template>
    </select>
    <input type="hidden" name="{$ref}" value="{value/node()}"/>
    <script type="text/javascript">
      loaddatemmyy('<xsl:value-of select="$formName"/>','<xsl:value-of select="$ref"/>');
    </script>
  </xsl:template>
  <!-- ========================== CONTROL : CC Issue Date ========================== -->
  <!-- -->
  <xsl:template match="input[contains(@class,'ccIssue')]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="formName">
      <xsl:value-of select="ancestor::Content/model/submission/@id"/>
    </xsl:variable>
    <select name="month_{$ref}" id="month_{$ref}" onchange="makedatemmyy('{$formName}','{$ref}')" class="form-control month">
      <option value="" selected=""/>
      <option value="01">01</option>
      <option value="02">02</option>
      <option value="03">03</option>
      <option value="04">04</option>
      <option value="05">05</option>
      <option value="06">06</option>
      <option value="07">07</option>
      <option value="08">08</option>
      <option value="09">09</option>
      <option value="10">10</option>
      <option value="11">11</option>
      <option value="12">12</option>
    </select>
    <select name="year_{$ref}" onchange="makedatemmyy('{$formName}','{$ref}')" class="form-control year">
      <option value="" selected=""/>
      <xsl:call-template name="getYearDropDown">
        <xsl:with-param name="i" select="0"/>
        <xsl:with-param name="reps" select="10"/>
        <xsl:with-param name="operation" select="'-'"/>
        <xsl:with-param name="year" select="substring(/Page/Request/ServerVariables/Item[@name='Date']/node(),1,4)"/>
      </xsl:call-template>
    </select>
    <input type="hidden" name="{$ref}" value="{value/node()}"/>
    <script type="text/javascript">
      loaddatemmyy('<xsl:value-of select="$formName"/>','<xsl:value-of select="$ref"/>');
    </script>
  </xsl:template>

  <!-- -->
  <!-- -->
  <xsl:template name="getYearDropDown">
    <xsl:param name="i"/>
    <xsl:param name="reps"/>
    <xsl:param name="operation"/>
    <xsl:param name="year"/>
    <xsl:param name="value"/>

    <xsl:if test="$i &lt; $reps">
      <option value="{$year}">
        <xsl:if test="$value=$year">
          <xsl:attribute name="selected">selected</xsl:attribute>
        </xsl:if>
        <xsl:value-of select="$year"/>
      </option>
      <xsl:call-template name="getYearDropDown">
        <xsl:with-param name="i" select="$i + 1"/>
        <xsl:with-param name="reps" select="$reps"/>
        <xsl:with-param name="value" select="$value"/>
        <xsl:with-param name="operation" select="$operation"/>
        <xsl:with-param name="year">
          <xsl:choose>
            <xsl:when test="$operation='-'">
              <xsl:value-of select="$year - 1"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$year + 1"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:if>

  </xsl:template>
  <!-- ========================== CONTROL : Future Date ========================== -->
  <!-- -->
  <xsl:template match="input[contains(@class,'futureDate')]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="formName">
      <xsl:value-of select="ancestor::Content/model/submission/@id"/>
    </xsl:variable>
    <select name="day_{$ref}" id="day_{$ref}" onchange="makeXmlDate('{$formName}','{$ref}')" class="dropdown day">
      <option value="" selected=""/>
      <option value="01">1</option>
      <option value="02">2</option>
      <option value="03">3</option>
      <option value="04">4</option>
      <option value="05">5</option>
      <option value="06">6</option>
      <option value="07">7</option>
      <option value="08">8</option>
      <option value="09">9</option>
      <option value="10">10</option>
      <option value="11">11</option>
      <option value="12">12</option>
      <option value="13">13</option>
      <option value="14">14</option>
      <option value="15">15</option>
      <option value="16">16</option>
      <option value="17">17</option>
      <option value="18">18</option>
      <option value="19">19</option>
      <option value="20">20</option>
      <option value="21">21</option>
      <option value="22">22</option>
      <option value="23">23</option>
      <option value="24">24</option>
      <option value="25">25</option>
      <option value="26">26</option>
      <option value="27">27</option>
      <option value="28">28</option>
      <option value="29">29</option>
      <option value="30">30</option>
      <option value="31">31</option>
    </select>
    <select name="month_{$ref}" onchange="makeXmlDate('{$formName}','{$ref}')" class="dropdown month">
      <option value="" selected=""/>
      <option value="01">Jan</option>
      <option value="02">Feb</option>
      <option value="03">Mar</option>
      <option value="04">Apr</option>
      <option value="05">May</option>
      <option value="06">Jun</option>
      <option value="07">Jul</option>
      <option value="08">Aug</option>
      <option value="09">Sep</option>
      <option value="10">Oct</option>
      <option value="11">Nov</option>
      <option value="12">Dec</option>
    </select>
    <select name="year_{$ref}" onchange="makeXmlDate('{$formName}','{$ref}')" class="dropdown year">
      <option value="" selected=""/>
      <option value="2008">2008</option>
      <option value="2009">2009</option>
      <option value="2010">2010</option>
      <option value="2011">2011</option>
      <option value="2012">2012</option>
    </select>
    <input type="hidden" name="{$ref}" value="{value/node()}"/>
    <script type="text/javascript">
      loadXmlDate('<xsl:value-of select="$formName"/>','<xsl:value-of select="$ref"/>');
    </script>
  </xsl:template>
  <!-- -->



  <!-- ========================== CONTROL : Calendar ========================== -->
  <!-- -->
  <xsl:template match="input[contains(@class,'calendar')]" mode="xform_control">
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
        <input type="text" name="{$ref}" id="{$ref}" value="{value/node()}" class="input-small jqDatePicker form-control" placeholder="{$inlineHint}"/>
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


  <!-- ========================== CONTROL :  DOB Calendar ========================== -->
  <!-- -->
  <xsl:template match="input[contains(@class,'DOBcalendar')]" mode="xform_control">
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
  <xsl:template match="input[contains(@class,'time')]" mode="xform_control">
    <xsl:variable name="label_low" select="translate(label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
    <xsl:variable name="inlineHint">
      <xsl:choose>
        <xsl:when test="hint[@class='inline']">
          <xsl:value-of select="hint[@class='inline']/node()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="msg_required_inline"/>
          <xsl:value-of select="$label_low"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="hValue" select="substring-before(value/node(),',')"/>
    <xsl:variable name="mValue" select="substring-after(value/node(),',')"/>
    <!-- HOURS -->
    <select name="{$ref}" id="{$ref}">
      <xsl:attribute name="class">
        <xsl:value-of select="@class"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@class"/>
        <xsl:text>hours</xsl:text>
      </xsl:attribute>
      <xsl:call-template name="getHourOptions">
        <xsl:with-param name="value" select="$hValue"/>
      </xsl:call-template>
    </select>
    <xsl:text> : </xsl:text>
    <!-- MINUTES -->
    <select name="{$ref}" id="{$ref}">
      <xsl:attribute name="class">
        <xsl:value-of select="@class"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="@class"/>
        <xsl:text>minutes</xsl:text>
      </xsl:attribute>
      <xsl:call-template name="getMinuteOptions">
        <xsl:with-param name="value" select="$mValue"/>
      </xsl:call-template>
    </select>
  </xsl:template>
  <!-- -->
  <!-- ========================== CONTROL : Time ========================== -->
  <xsl:template name="getHourOptions">
    <xsl:param name="hours" select="0"/>
    <xsl:param name="value"/>
    <xsl:if test="$hours &lt; 24">
      <xsl:if test="$hours=0">
        <option value="">-</option>
      </xsl:if>
      <option value="{format-number($hours,'00')}">
        <xsl:if test="$value = format-number($hours,'00')">
          <xsl:attribute name="selected">selected</xsl:attribute>
        </xsl:if>
        <xsl:value-of select="format-number($hours,'00')"/>
      </option>
      <xsl:call-template name="getHourOptions">
        <xsl:with-param name="hours" select="$hours + 1"/>
        <xsl:with-param name="value" select="$value"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <xsl:template name="getMinuteOptions">
    <xsl:param name="minutes" select="0"/>
    <xsl:param name="value"/>
    <xsl:if test="$minutes &lt; 60">
      <xsl:if test="$minutes=0">
        <option value="">-</option>
      </xsl:if>
      <option value="{format-number($minutes,'00')}">
        <xsl:if test="$value = format-number($minutes,'00')">
          <xsl:attribute name="selected">selected</xsl:attribute>
        </xsl:if>
        <xsl:value-of select="format-number($minutes,'00')"/>
      </option>
      <xsl:call-template name="getMinuteOptions">
        <xsl:with-param name="minutes" select="$minutes + 5"/>
        <xsl:with-param name="value" select="$value"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- ========================== CONTROL : Colour Picker ========================== -->
  <!-- -->
  <xsl:template match="input[contains(@class,'colorPicker')]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="formName">
      <xsl:value-of select="ancestor::Content/model/submission/@id"/>
    </xsl:variable>
    <input type="text" name="{$ref}" id="{$ref}" value="{value/node()}" class="colorPicker form-control">
      <xsl:if test="value/node()">
        <xsl:attribute name="style">
          <xsl:text>background-color:</xsl:text>
          <xsl:value-of select="value/node()"/>
          <xsl:text>;</xsl:text>
        </xsl:attribute>
      </xsl:if>
    </input>
  </xsl:template>
  <!-- -->
  <!-- ========================== CONTROL : AreaTag ========================== -->
  <!-- -->
  <xsl:template match="input[contains(@class,'AreaTag')]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="formName">
      <xsl:value-of select="ancestor::Content/model/submission/@id"/>
    </xsl:variable>


    <input type="hidden" name="{$ref}" id="{$ref}" class=""/>

    <select name="{$ref}_shape" id="{$ref}_shape" class="dropdown short" onchange="updateAreaTag_{$ref}('{$ref}');">
      <option value="rect">
        <xsl:if test="value/area/@shape = 'rect'">
          <xsl:attribute name="selected">
            <xsl:text>selected</xsl:text>
          </xsl:attribute>
        </xsl:if>
        <xsl:text>rect</xsl:text>
      </option>
      <option value="poly">
        <xsl:if test="value/area/@shape = 'poly'">
          <xsl:attribute name="selected">
            <xsl:text>selected</xsl:text>
          </xsl:attribute>
        </xsl:if>
        <xsl:text>poly</xsl:text>
      </option>
    </select>
    <!-- onchange="updateAreaTag('{$ref}');"-->
    <input name="" id="{$ref}_coords" value="{value/area/@coords}" onchange="updateAreaTag_{$ref}('{$ref}');"></input>
    <script type="text/javascript">

      /* On tag changes to coords or shape*/
      function updateAreaTag_<xsl:value-of select="$ref" />(ref){
      var areaShape_<xsl:value-of select="$ref" /> = document.getElementById('<xsl:value-of select="$ref" />_shape').value;
      var areaCoOrds_<xsl:value-of select="$ref" /> = document.getElementById('<xsl:value-of select="$ref" />_coords').value;

      areaTagVal_<xsl:value-of select="$ref" /> = "<xsl:text disable-output-escaping="yes"><![CDATA[<]]>area shape='" + areaShape_</xsl:text><xsl:value-of select="$ref" /><xsl:text disable-output-escaping="yes"> + "' coords='" + areaCoOrds_</xsl:text><xsl:value-of select="$ref" /><xsl:text disable-output-escaping="yes"> + "' <![CDATA[/>]]></xsl:text>";
      document.getElementById('<xsl:value-of select="$ref" />').value = areaTagVal_<xsl:value-of select="$ref" />;
      }

      /*On load update hidden input box with current values*/
      updateAreaTag_<xsl:value-of select="$ref" />('<xsl:value-of select="$ref" />');

    </script>
  </xsl:template>



  <!-- -->
  <!-- ========================== CONTROL : SECRET ========================== -->
  <!-- -->
  <xsl:template match="secret" mode="xform_control">
    <xsl:variable name="inlineHint">
      <xsl:apply-templates select="." mode="getInlineHint"/>
    </xsl:variable>
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <input type="password" name="{$ref}" id="{$ref}">
      <xsl:attribute name="class">
        <xsl:if test="@class!=''">
          <xsl:value-of select="@class"/>
          <xsl:text> </xsl:text>
        </xsl:if>
        <xsl:text>textbox form-control</xsl:text>
      </xsl:attribute>
      <xsl:choose>
        <xsl:when test="value!=''">
          <xsl:attribute name="value">
            <xsl:copy-of select="value/node()"/>
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:if test="$inlineHint!=''">
            <xsl:attribute name="placeholder">
              <xsl:value-of select="$inlineHint"/>
            </xsl:attribute>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>
    </input>
    <xsl:if test="contains(@class,'strongPassword')">
        <a id="passwordPolicy" class="text-muted" href="#">view our password policy</a>
    </xsl:if>
  </xsl:template>
  <!-- -->
  <xsl:template match="secret[contains(@class,'textbox')]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <input type="text" name="{$ref}" id="{$ref}">
      <xsl:if test="@class!=''">
        <xsl:attribute name="class">
          <xsl:value-of select="@class"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:choose>
        <xsl:when test="value!=''">
          <xsl:attribute name="value">
            <xsl:value-of select="value"/>
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="label_low" select="translate(label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
          <xsl:attribute name="value">
            Please enter <xsl:value-of select="$label_low"/>
          </xsl:attribute>
          <xsl:attribute name="onfocus">
            if (this.value=='Please enter <xsl:value-of select="$label_low"/>') {this.value=''}
          </xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
    </input>
  </xsl:template>
  <!-- -->
  <!-- ========================== CONTROL : TEXTAREA ========================== -->
  <!-- -->
  <xsl:template match="textarea" mode="xform_control">
    <xsl:variable name="inlineHint">
      <xsl:apply-templates select="." mode="getInlineHint"/>
    </xsl:variable>
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <textarea name="{$ref}" id="{$ref}" class="textarea form-control">
      <xsl:if test="@cols!=''">
        <xsl:attribute name="cols">
          <xsl:value-of select="@cols"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@rows!=''">
        <xsl:attribute name="rows">
          <xsl:value-of select="@rows"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@maxlength!=''">
        <xsl:attribute name="maxlength">
          <xsl:value-of select="@maxlength"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@class!=''">
        <xsl:attribute name="class">
          <xsl:text>form-control </xsl:text>
          <xsl:choose>
            <xsl:when test="ancestor::switch and contains(@class,'required')">
              <xsl:apply-templates select="." mode="isRequired"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="@class"/>
            </xsl:otherwise>
          </xsl:choose>
          <xsl:if test="@maxwords!=''">
            <xsl:text> maxwords-</xsl:text>
            <xsl:value-of select="@maxwords"/>
          </xsl:if>
        </xsl:attribute>
      </xsl:if>
      <xsl:choose>
        <xsl:when test="value!=''">
          <xsl:copy-of select="value/node()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:if test="$inlineHint!=''">
            <xsl:attribute name="placeholder">
              <xsl:value-of select="$inlineHint"/>
            </xsl:attribute>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:text></xsl:text>
    </textarea>
  </xsl:template>
  <!-- -->
  <xsl:template match="textarea[contains(@class,'readonly')]" mode="xform_control">
    <div class="textareaReadOnly">
      <xsl:copy-of select="value/node()"/>
      <xsl:text> </xsl:text>
    </div>
  </xsl:template>



  <!-- -->
  <!-- ========================== CONTROL : SELECTS ========================== -->
  <!-- -->
  <xsl:template match="select1[@appearance='minimal'] | select1" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="value">
      <xsl:apply-templates select="." mode="xform_value"/>
    </xsl:variable>

    <select name="{$ref}" id="{$ref}">

      <xsl:attribute name="class">
        <xsl:text>form-control dropdown </xsl:text>
        <xsl:choose>
          <xsl:when test="ancestor::switch and contains(@class,'required')">
            <xsl:apply-templates select="." mode="isRequired"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="@class"/>
          </xsl:otherwise>
        </xsl:choose>

      </xsl:attribute>
      <xsl:if test="contains(@class,'readonly')">
        <xsl:attribute name="disabled">disabled</xsl:attribute>
        </xsl:if>

      <xsl:if test="@onChange!=''">
        <xsl:attribute name="onChange">
          <xsl:value-of select="@onChange"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="not(contains(@class,'keep_empty'))">
        <option value="">
          <xsl:apply-templates select="." mode="getInlineHint"/>
          <!--<xsl:variable name="label_low" select="translate(label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
          <xsl:text>Please select </xsl:text>
          <xsl:value-of select="$label_low"/>-->
          <xsl:text> </xsl:text>
        </option>
      </xsl:if>

      <xsl:choose>

        <!-- when Query to get select options -->
        <xsl:when test="contains(@class,'ewQuery')">
          <xsl:variable name="selectOptions">
            <xsl:apply-templates select="." mode="getSelectOptions"/>
          </xsl:variable>
          <xsl:apply-templates select="ms:node-set($selectOptions)/select1/*" mode="xform_select">
            <xsl:with-param name="selectedValue" select="$value"/>
          </xsl:apply-templates>
        </xsl:when>

        <!-- when alphasort -->
        <xsl:when test="contains(@class,'alphasort')">
          <xsl:apply-templates select="item" mode="xform_select">
            <xsl:sort select="label" order="ascending"/>
            <xsl:with-param name="selectedValue" select="$value"/>
          </xsl:apply-templates>
        </xsl:when>

        <!-- default behaviour - list items -->
        <xsl:otherwise>
          <xsl:apply-templates select="*" mode="xform_select">
            <xsl:with-param name="selectedValue" select="$value"/>
          </xsl:apply-templates>
        </xsl:otherwise>
      </xsl:choose>

    </select>
  </xsl:template>
  <!-- -->
  <!-- ## Standard Select1 for Radio Buttons ########################################################### -->
  <xsl:template match="select1[@appearance='full']" mode="xform_control">
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
      <xsl:text>-dependant form-group</xsl:text>
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

  <xsl:template match="choices" mode="xform_radiocheck">
    <xsl:param name="type"/>
    <xsl:param name="ref"/>
    <xsl:param name="dependantClass"/>
    <xsl:if test="item">
      <xsl:if test="label">
        <legend>
          <xsl:apply-templates select="label" mode="xform_legend"/>
        </legend>
      </xsl:if>
      <fieldset class="choices multiline">
        <xsl:apply-templates select="item" mode="xform_radiocheck">
          <xsl:with-param name="type" select="$type"/>
          <xsl:with-param name="ref" select="$ref"/>
          <xsl:with-param name="dependantClass" select="$dependantClass"/>
        </xsl:apply-templates>
      </fieldset>
    </xsl:if>
  </xsl:template>

  <!-- -->
  <xsl:template match="select1[@appearance='full' and @class='PickByImage']" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <!--<xsl:attribute name="class">pickByImage</xsl:attribute>-->
    <input type="hidden" name="{$ref}" value="{value/node()}"/>
    <div class="adminList" id="accordion">
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
    <div class="header">
      <h6>
        <xsl:apply-templates select="label" mode="xform_legend"/>
      </h6>
    </div>
    <div class="choices">
      <xsl:apply-templates select="item" mode="xform_imageClick">
        <xsl:with-param name="ref" select="$ref"/>
      </xsl:apply-templates>
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

    <div class="item">
      <xsl:if test="ancestor::select1/value/node()=$value">
        <xsl:attribute name="class">selected</xsl:attribute>
      </xsl:if>
      <div class="ItemThumbnail">
        <input type="image" onclick="this.form.{$ref}.value='{value/node()}'" src="{$imageURL}" name="ewsubmit"/>
      </div>
      <h5>
        <xsl:value-of select="label/node()"/>
      </h5>
      <xsl:copy-of select="div"/>

    </div>
  </xsl:template>

  <xsl:template match="item[label/Theme]" mode="xform_imageClick">
    <xsl:param name="ref"/>
    <xsl:variable name="value" select="label/Theme/RootXslt/@src"/>
    <div class="item ThemeButton">
      <xsl:if test="ancestor::select1/value/node()=$value">
        <xsl:attribute name="class">selected</xsl:attribute>
        <xsl:attribute name="disabled">disabled</xsl:attribute>
      </xsl:if>
      <div class="ItemThumbnail">
        <input type="image" onclick="this.form.{$ref}.value='{$value}';this.form.ewSiteTheme.value='{label/Theme/@name}'" src="{translate(label/Theme/Images/img[@class='thumbnail']/@src,' ','-')}" name="ewsubmit"/>
      </div>
      <h5>
        <xsl:value-of select="label/Theme/@name"/>
      </h5>
      <div>
        <xsl:apply-templates select="label/Theme/Description/node()" mode="cleanXhtml"/>
        <!--<small>-->
        <xsl:apply-templates select="label/Theme/Attribution/node()" mode="cleanXhtml"/>
        <!--</small>-->
      </div>
    </div>
  </xsl:template>

  <!-- -->
  <xsl:template match="select[@appearance='minimal'] | select" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="value">
      <xsl:value-of select="value/node()"/>
    </xsl:variable>
    <select multiple="multiple">
      <xsl:if test="$ref!=''">
        <xsl:attribute name="name">
          <xsl:value-of select="$ref"/>
        </xsl:attribute>
        <xsl:attribute name="id">
          <xsl:value-of select="$ref"/>
        </xsl:attribute>
      </xsl:if>

      <xsl:attribute name="class">
        <xsl:if test="@class!=''">
          <xsl:value-of select="@class"/>
        </xsl:if>
        <xsl:text> form-control</xsl:text>
      </xsl:attribute>

      <xsl:if test="@size!=''">
        <xsl:attribute name="size">
          <xsl:value-of select="@size"/>
        </xsl:attribute>
      </xsl:if>

      <option value="">
        <xsl:variable name="label_low" select="translate(label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
        <xsl:text>Please select </xsl:text>
        <xsl:value-of select="$label_low"/>
        <xsl:text> </xsl:text>
      </option>
      <xsl:choose>
        <!-- when Query to get select options -->
        <xsl:when test="contains(@class,'ewQuery')">
          <xsl:variable name="selectOptions">
            <xsl:apply-templates select="." mode="getSelectOptions"/>
          </xsl:variable>
          <xsl:apply-templates select="ms:node-set($selectOptions)/select1/*" mode="xform_select">
            <xsl:with-param name="selectedValue" select="$value"/>
          </xsl:apply-templates>
        </xsl:when>

        <xsl:when test="contains(@class,'alphasort')">
          <xsl:apply-templates select="item" mode="xform_select">
            <xsl:sort select="label" order="ascending"/>
          </xsl:apply-templates>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="*" mode="xform_select"/>
        </xsl:otherwise>
      </xsl:choose>
    </select>
  </xsl:template>
  <!-- -->
  <xsl:template match="select[@appearance='full'] | select[contains(@class,'checkbox')]" mode="xform_control">
    <!--<xsl:attribute name="class">
      <xsl:value-of select="@class"/>
    </xsl:attribute>-->

    <!--<xsl:attribute name="class">
          <xsl:text>testing</xsl:text>
      </xsl:attribute>-->

    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:if test="contains(@class,'selectAll')">
      <span>
        <xsl:attribute name="class">
          <xsl:text>radiocheckbox checkbox</xsl:text>
          <xsl:if test="contains(@class,'multiline')">
            <xsl:text> multiline</xsl:text>
          </xsl:if>
        </xsl:attribute>
        <label for="{@ref}_selectAll">
          <input type="checkbox" name="{@ref}_selectAll" id="{@ref}_selectAll" class="selectAll"/>
          &#160;
          &#160;
          Select All
        </label>
      </span>
    </xsl:if>

    <div class="{@class} list-group">
      <xsl:choose>
        <!-- when Query to get select options -->
        <xsl:when test="contains(@class,'ewQuery')">
          <xsl:variable name="selectedValue">
            <xsl:value-of select="value/node()"/>
          </xsl:variable>
          <xsl:variable name="selectOptions">
            <xsl:apply-templates select="." mode="getSelectOptions"/>
          </xsl:variable>
          <!--<xsl:copy-of select="$selectOptions"/>-->
          <xsl:apply-templates select="ms:node-set($selectOptions)/select1/item" mode="xform_radiocheck">
            <xsl:with-param name="selectedValue" select="$selectedValue"/>
            <xsl:with-param name="class" select="@class"/>
            <xsl:with-param name="ref" select="$ref"/>
            <xsl:with-param name="type">checkbox</xsl:with-param>
          </xsl:apply-templates>
        </xsl:when>

        <xsl:otherwise>
          <xsl:apply-templates select="item | choices" mode="xform_radiocheck">
            <xsl:with-param name="type">checkbox</xsl:with-param>
            <xsl:with-param name="ref" select="$ref"/>
            <xsl:with-param name="class" select="'list-item-group'"/>
          </xsl:apply-templates>
        </xsl:otherwise>

      </xsl:choose>
    </div>
  </xsl:template>

  <!-- -->
  <xsl:template match="itemset" mode="xform_select">
    <xsl:param name="selectedValue"/>
    <xsl:variable name="value" select="value"/>
    <optgroup>
      <xsl:attribute name="label">
        <xsl:value-of select="label/node()"/>
      </xsl:attribute>
      <xsl:apply-templates select="item | itemset" mode="xform_select">
        <xsl:with-param name="selectedValue" select="$selectedValue"/>
      </xsl:apply-templates>
    </optgroup>
  </xsl:template>

  <!-- -->
  <xsl:template match="item" mode="xform_select">
    <xsl:param name="selectedValue"/>
    <xsl:variable name="value" select="value"/>
    <option>
      <xsl:attribute name="value">
        <xsl:value-of select="value"/>
      </xsl:attribute>
      <xsl:if test="ancestor::select1/value/node()=$value or @selected='selected' or contains(concat(',',$selectedValue,','),concat(',',$value,','))">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:copy-of select="label/node()"/>
      <xsl:text> </xsl:text>
    </option>
  </xsl:template>

  <xsl:template match="item" mode="xform_select_multi">
    <xsl:param name="selectedValues"/>
    <xsl:variable name="value" select="value"/>
    <option>
      <xsl:attribute name="value">
        <xsl:value-of select="value"/>
      </xsl:attribute>
      <xsl:if test="ms:node-set($selectedValues)/*[node()=$value]">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:copy-of select="label/node()"/>
      <xsl:text> </xsl:text>
    </option>
  </xsl:template>
  
  <!-- -->
  <xsl:template match="item" mode="xform_radiocheck">
    <xsl:param name="type"/>
    <xsl:param name="ref"/>
    <xsl:param name="value" select="value"/>
    <xsl:param name="selectedValue">
      <xsl:apply-templates select="ancestor::*[name()='select' or name()='select1']" mode="xform_value"/>
    </xsl:param>
    <xsl:variable name="class" select="ancestor::*[name()='select' or name()='select1' ]/@class"/>

    <span>
      <xsl:attribute name="class">
        <xsl:text>radiocheckbox checkbox</xsl:text>
        <xsl:if test="contains($class,'multiline')">
          <xsl:text> multiline</xsl:text>
        </xsl:if>
      </xsl:attribute>


      <label for="{$ref}_{$value}">
        <xsl:attribute name="class">
          <xsl:text>radio</xsl:text>
          <xsl:if test="label/@class and label/@class!=''">
            <xsl:text> </xsl:text>
            <xsl:value-of select="label/@class"/>
          </xsl:if>
          <xsl:if test="ancestor::Content[@type='xform' and @name='PayForm']">
            <xsl:text> </xsl:text>
            <xsl:value-of select="translate(value/node(),' /','--')"/>
          </xsl:if>
        </xsl:attribute>
        <!-- for payform to have cc classes-->
        <input type="{$type}">
          <xsl:if test="$ref!=''">
            <xsl:attribute name="name">
              <xsl:value-of select="$ref"/>
            </xsl:attribute>
            <xsl:attribute name="id">
              <xsl:value-of select="$ref"/>_<xsl:value-of select="$value"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:attribute name="value">
            <xsl:value-of select="value"/>
          </xsl:attribute>
          <xsl:attribute name="title">
            <xsl:value-of select="@title"/>
          </xsl:attribute>
          <xsl:attribute name="onclick">
            <xsl:value-of select="@onclick"/>
          </xsl:attribute>

          <!-- Check Radio adminButton is selected -->
          <xsl:if test="$selectedValue=$value">
            <xsl:attribute name="checked">checked</xsl:attribute>
          </xsl:if>

          <!-- Check checkbox should be selected -->
          <xsl:if test="contains($type,'checkbox')">
            <!-- Run through CSL to see if this should be checked -->
            <xsl:variable name="valueMatch">
              <xsl:call-template name="checkValueMatch">
                <xsl:with-param name="CSLValue" select="$selectedValue"/>
                <xsl:with-param name="value" select="$value"/>
                <xsl:with-param name="seperator" select="','"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:if test="$valueMatch='true'">
              <xsl:attribute name="checked">checked</xsl:attribute>
            </xsl:if>
          </xsl:if>
          <xsl:if test="ancestor::select1/item[1]/value/node() = $value">
            <xsl:attribute name="data-fv-notempty">
              <xsl:value-of select="ancestor::select1/@data-fv-notempty"/>
            </xsl:attribute>
            <xsl:attribute name="data-fv-notempty-message">
              <xsl:value-of select="ancestor::select1/@data-fv-notempty-message"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:if test="ancestor::select/item[1]/value/node() = $value">
            <xsl:attribute name="data-fv-choice">
              <xsl:value-of select="ancestor::select/@data-fv-choice"/>
            </xsl:attribute>
            <xsl:attribute name="data-fv-choice-min">
              <xsl:value-of select="ancestor::select/@data-fv-choice-min"/>
            </xsl:attribute>
            <xsl:attribute name="data-fv-choice-max">
              <xsl:value-of select="ancestor::select/@data-fv-choice-max"/>
            </xsl:attribute>
            <xsl:attribute name="data-fv-choice-message">
              <xsl:value-of select="ancestor::select/@data-fv-choice-message"/>
            </xsl:attribute>
            <xsl:if test="ancestor::select/@data-fv-notempty">
              <xsl:attribute name="data-fv-notempty">
                <xsl:value-of select="ancestor::select/@data-fv-notempty"/>
              </xsl:attribute>
              <xsl:attribute name="data-fv-notempty-message">
                <xsl:value-of select="ancestor::select/@data-fv-notempty-message"/>
              </xsl:attribute>
            </xsl:if>
          </xsl:if>
        </input>
        <xsl:apply-templates select="label" mode="xform-label"/>
        <xsl:text> </xsl:text>
      </label>

    </span>
    <!--<xsl:if test="contains($class,'multiline') and position()!=last()">
					<br/>
				</xsl:if>-->

  </xsl:template>

  <!-- Radio Input with dependant Case toggle -->
  <xsl:template match="item[toggle]" mode="xform_radiocheck">
    <xsl:param name="type"/>
    <xsl:param name="ref"/>
    <xsl:param name="dependantClass"/>

    <xsl:variable name="value" select="value"/>
    <xsl:variable name="class" select="../@class"/>
    <span>
      <xsl:attribute name="class">
        <xsl:text>radiocheckbox checkbox</xsl:text>
        <xsl:if test="contains($class,'multiline')">
          <xsl:text> multiline</xsl:text>
        </xsl:if>
      </xsl:attribute>
      <label for="{$ref}_{position()}" class="radio {translate(value/node(),'/ ','')}">
        <input type="{$type}">
          <xsl:if test="$ref!=''">
            <xsl:attribute name="name">
              <xsl:value-of select="$ref"/>

            </xsl:attribute>
            <xsl:attribute name="id">
              <xsl:value-of select="$ref"/>_<xsl:value-of select="position()"/>
            </xsl:attribute>
          </xsl:if>
          <xsl:attribute name="value">
            <xsl:value-of select="value"/>
          </xsl:attribute>

          <!-- Check Radio adminButton is selected -->
          <xsl:if test="../value/node()=$value">
            <xsl:attribute name="checked">checked</xsl:attribute>
          </xsl:if>

          <!-- Check checkbox should be selected -->
          <xsl:if test="contains($class,'checkboxes')">
            <!-- Run through CSL to see if this should be checked -->
            <xsl:variable name="valueMatch">
              <xsl:call-template name="checkValueMatch">
                <xsl:with-param name="CSLValue" select="../value/node()"/>
                <xsl:with-param name="value" select="$value"/>
                <xsl:with-param name="seperator" select="','"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:if test="$valueMatch='true'">
              <xsl:attribute name="checked">checked</xsl:attribute>
            </xsl:if>
          </xsl:if>

          <xsl:attribute name="onclick">
            <xsl:text>showDependant('</xsl:text>
            <xsl:value-of select="translate(toggle/@case,'[]#=/','')"/>
            <xsl:text>-dependant','</xsl:text>
            <xsl:value-of select="$dependantClass"/>
            <xsl:text>');</xsl:text>
          </xsl:attribute>

        </input>
        &#160;
        <xsl:value-of select="label/node()"/>
      </label>
    </span>
    <!--<xsl:if test="contains($class,'multiline') and position()!=last()">
      <br/>
    </xsl:if>-->

  </xsl:template>


  <!-- Radio Input with dependant Case toggle and @bindTo -->
  <xsl:template match="item[toggle and @bindTo]" mode="xform_radiocheck">
    <xsl:param name="type"/>
    <xsl:param name="ref"/>
    <xsl:param name="dependantClass"/>
    <xsl:variable name="bindTo" select="@bindTo"/>

    <xsl:variable name="value" select="value"/>
    <xsl:variable name="class" select="../@class"/>
    <span>
      <xsl:attribute name="class">
        <xsl:text>radiocheckbox</xsl:text>
        <xsl:if test="contains($class,'multiline')">
          <xsl:text> multiline</xsl:text>
        </xsl:if>
      </xsl:attribute>
      <label for="{$ref}_{position()}" class="radio {translate(value/node(),'/ ','')}">
        <input type="{$type}">

          <xsl:attribute name="name">
            <xsl:value-of select="@bindTo"/>
          </xsl:attribute>
          <xsl:attribute name="id">
            <xsl:value-of select="$ref"/>_<xsl:value-of select="position()"/>
          </xsl:attribute>

          <xsl:attribute name="value">
            <xsl:value-of select="value"/>
          </xsl:attribute>

          <!-- Check Radio adminButton is selected -->

          <xsl:if test="$value=input[@bind=$bindTo]/value">
            <xsl:attribute name="checked">checked</xsl:attribute>
          </xsl:if>

          <!--<xsl:if test="../value/node()=$value">
          <xsl:attribute name="checked">checked</xsl:attribute>
        </xsl:if>-->

          <!-- Check checkbox should be selected -->
          <xsl:if test="contains($class,'checkboxes')">
            <!-- Run through CSL to see if this should be checked -->
            <xsl:variable name="valueMatch">
              <xsl:call-template name="checkValueMatch">
                <xsl:with-param name="CSLValue" select="../value/node()"/>
                <xsl:with-param name="value" select="$value"/>
                <xsl:with-param name="seperator" select="','"/>
              </xsl:call-template>
            </xsl:variable>
            <xsl:if test="$valueMatch='true'">
              <xsl:attribute name="checked">checked</xsl:attribute>
            </xsl:if>
          </xsl:if>

          <xsl:attribute name="onclick">
            <xsl:text>showDependant('</xsl:text>
            <xsl:value-of select="translate(toggle/@case,'[]#=/','')"/>
            <xsl:text>-dependant','</xsl:text>
            <xsl:value-of select="$dependantClass"/>
            <xsl:text>');</xsl:text>
          </xsl:attribute>

        </input>
        &#160;
        <xsl:value-of select="label/node()"/>
      </label>
    </span>
    <xsl:apply-templates select="input[@bind=$bindTo]" mode="xform"/>

    <!-- REMOVED - CSS now displays block on span to make vertically aligned. -->
    <!--<xsl:if test="contains($class,'multiline') and position()!=last()">
      <br/>
    </xsl:if>-->

  </xsl:template>
  <!-- -->
  <xsl:template name="checkValueMatch">
    <xsl:param name="CSLValue"/>
    <xsl:param name="value"/>
    <xsl:param name="seperator"/>
    <xsl:choose>
      <xsl:when test="contains($CSLValue,$seperator)">
        <xsl:variable name="valueTest" select="substring-before($CSLValue,$seperator)"/>
        <xsl:choose>
          <xsl:when test="$valueTest=$value">
            <xsl:text>true</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:call-template name="checkValueMatch">
              <xsl:with-param name="value" select="$value"/>
              <xsl:with-param name="seperator" select="$seperator"/>
              <xsl:with-param name="CSLValue" select="substring-after($CSLValue,$seperator)"/>
            </xsl:call-template>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:if test="$value=$CSLValue">
          <xsl:text>true</xsl:text>
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>
  <!-- -->
  <!-- -->
  <!-- ========================== CONTROL : RANGE ========================== -->
  <!-- -->
  <xsl:template match="range" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <input type="text" id="{$ref}" name="{$ref}" class="slider form-control">
      <xsl:if test="@size!=''">
        <xsl:attribute name="size">
          <xsl:value-of select="@size"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:choose>
        <xsl:when test="value!=''">
          <xsl:attribute name="value">
            <xsl:value-of select="value"/>
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:variable name="label_low" select="translate(label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
          <xsl:attribute name="value">
            <xsl:value-of select="@start"/>
          </xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
    </input>
    <div class="slider">
      <span class="max">
        <xsl:value-of select="@end"/>
      </span>
      <span class="min">
        <xsl:value-of select="@start"/>
      </span>
      <span class="step">
        <xsl:value-of select="@step"/>
      </span>
      <span class="val">
        <xsl:choose>
          <xsl:when test="value!=''">
            <xsl:value-of select="value"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="@start"/>
          </xsl:otherwise>
        </xsl:choose>
      </span>
      <span class="ref">
        <xsl:value-of select="$ref"/>
      </span>
    </div>
    <div class="terminus">&#160;</div>
  </xsl:template>
  <!-- -->
  <!-- ========================== CONTROL : UPLOAD ========================== -->
  <!-- -->
  <xsl:template match="upload" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <!--
			If this has a value, we can assume that a file has been uploaded to it,
			and don't need to show the upload input, UNLESS this item has an alert,
			which would suggest that it has been uploaded, but the form has not been validated.
		-->
    <xsl:choose>
      <xsl:when test="not(alert) and value!=''">
        <xsl:text>Uploaded file: </xsl:text>
        <xsl:value-of select="value"/>
      </xsl:when>
      <xsl:otherwise>
        <div class="input-group">
          <span class="input-group-btn">
            <span class="btn btn-primary btn-file">
              <input type="file" name="{$ref}" id="{$ref}">
                <xsl:if test="@class!=''">
                  <xsl:attribute name="class">
                    <xsl:value-of select="@class"/>
                  </xsl:attribute>
                </xsl:if>
                <xsl:if test="value!=''">
                  <xsl:attribute name="value">
                    <xsl:value-of select="value"/>
                  </xsl:attribute>
                </xsl:if>
              </input>
              <i class="fa fa-file-o">
                <xsl:text> </xsl:text>
              </i>
              <xsl:text> </xsl:text>
              <xsl:choose>
                <xsl:when test="@buttonText!=''">
                  <xsl:value-of select="@buttonText"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:call-template name="term5006"/>
                </xsl:otherwise>
              </xsl:choose>

            </span>
          </span>
          <input type="text" class="form-control" readonly="">
            <xsl:for-each select="@*">
              <xsl:variable name="nodename" select="name()"/>
              <xsl:if test="starts-with($nodename,'data-fv')">
                <xsl:attribute name="{name()}">
                  <xsl:value-of select="." />
                </xsl:attribute>
              </xsl:if>
            </xsl:for-each>
          </input>
        </div>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- -->
  <!-- ========================== XFORM : LABEL ========================== -->
  <!-- -->

  <xsl:template match="label" mode="legend">
    <legend>
      <xsl:choose>
        <!-- for Multilanguage-->
        <xsl:when test="span[contains(@class,'term')]">
          <xsl:apply-templates select="span" mode="term" />
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="./node()" mode="cleanXhtml"/>
        </xsl:otherwise>
      </xsl:choose>
    </legend>
  </xsl:template>

  <xsl:template match="label" mode="xform">
    <label>
      <xsl:apply-templates select="./node()" mode="cleanXhtml"/>
    </label>
  </xsl:template>

  <xsl:template match="label">
    <xsl:param name="cLabel"/>
    <xsl:param name="bRequired"/>
    <xsl:if test ="./node()!='' or span[contains(@class,'term')]">
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
          <xsl:if test="ancestor::group[contains(@class,'form-horizontal')]">
            <xsl:text> col-sm-3 control-label</xsl:text>
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
            <xsl:apply-templates select="." mode="xform-label"/> 
          </xsl:otherwise>
        </xsl:choose>
        <!--<xsl:value-of select="./node()"/>-->
        <xsl:if test="$bRequired='true' and not(ancestor::select1[@appearance='full' and value/node()!=''])">
          <span class="req">*</span>
        </xsl:if>
      </label>
    </xsl:if>
  </xsl:template>

  <xsl:template match="label" mode="xform-label">

      <xsl:apply-templates select="./node()" mode="cleanXhtml"/>
  </xsl:template>

  <xsl:template match="label[parent::input[contains(@class,'hidden')]]">

  </xsl:template>

  <!-- ###################################### Inline Tooltips on hover ############################## -->

  <xsl:template match="input[not(contains(@class,'hidden'))] | secret | select | select1 | range | textarea | upload " mode="xform_legend">
    <!-- Added not(@class) as wouldn't display without a class - WH - 2009-04-24-->

    <xsl:if test="hint[not(contains(@class,'inline'))]">
      <span class="hint">
        <xsl:copy-of select="hint/node()"/>
      </span>
    </xsl:if>
    <xsl:if test="help[not(contains(@class,'inline'))]">
      <span class="help">
        <xsl:copy-of select="help/node()"/>
      </span>
    </xsl:if>
    <xsl:if test="alert">
      <xsl:apply-templates select="alert" mode="inlineAlert"/>
    </xsl:if>
  </xsl:template>

  <xsl:template match="input[not(contains(@class,'hidden'))]" mode="xform_legend">

  </xsl:template>

  <xsl:template match="upload[not(contains(@class,'hidden'))]" mode="xform_legend">

  </xsl:template>

  <xsl:template match="input[not(contains(@class,'hidden'))] | secret | select | select1 | range | textarea | upload" mode="infoButton">
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

  <xsl:template match="input[not(contains(@class,'hidden'))] | secret | select | select1 | range | textarea | upload" mode="alertButton">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="ref2">
      <xsl:value-of select="translate($ref,'/','-')"/>
    </xsl:variable>
    <div class="popover-{$ref2} popoverContent" role="tooltip">
      <xsl:copy-of select="alert/node()"/>
    </div>
    <button type="button" class="btn btn-danger" id="popover-{$ref2}-btn" data-contentwrapper=".popover-{$ref2}" data-toggle="popover" data-container="body" data-placement="left" rel="frmPopover" data-original-title="{label/node()}" title="{label/node()}">
      <i class="fa fa-exclamation-triangle">
        <xsl:text> </xsl:text>
      </i>
    </button>
    <script>
      $(function () {
      <xsl:text>$('#popover-</xsl:text><xsl:value-of select="$ref2"/>
      <xsl:text>-btn')
      .popover('show');</xsl:text>
      });
    </script>
  </xsl:template>

  <!--	
			################################################################
			TEMPLATES TO ENCODE THE TAGS TO ALLOW TO BE PLACED IN ATTRIBUTES 
			FOR JS TO WRITE BACK 
			################################################################
	-->
  <xsl:template match="*" mode="dirtyXhtml">
    <xsl:text>&lt;</xsl:text>
    <xsl:value-of select="name()"/>
    <xsl:if test="@*">
      <xsl:text> </xsl:text>
      <xsl:for-each select="@*">
        <xsl:value-of select="name()"/>
        <xsl:text>=&quot;</xsl:text>
        <xsl:value-of select="."/>
        <xsl:text>&quot; </xsl:text>
      </xsl:for-each>
    </xsl:if>
    <xsl:text>&gt;</xsl:text>
    <xsl:apply-templates mode="dirtyXhtml"/>
    <xsl:text>&lt;/</xsl:text>
    <xsl:value-of select="name()"/>
    <xsl:text>&gt;</xsl:text>
  </xsl:template>
  <!-- -->
  <xsl:template match="br" mode="dirtyXhtml">
    <xsl:text>&lt;br/&gt;</xsl:text>
  </xsl:template>
  <!-- -->
  <xsl:template match="hr" mode="dirtyXhtml">
    <xsl:text>&lt;hr/&gt;</xsl:text>
  </xsl:template>

  <!-- ##################################################################################################### -->
  <!-- ####################################  User Integration Controls   ################################### -->
  <!-- ##################################################################################################### -->

  <xsl:template match="input[contains(@class,'integration')]" mode="xform">
    <div>
      <xsl:apply-templates select="." mode="integrationControl"/>
    </div>
  </xsl:template>

  <!-- Twitter integration control -->
  <xsl:template match="input[@provider='Twitter']" mode="integrationControl">
    <xsl:variable name="dirId" select="/Page/Request/QueryString/Item[@name='dirId']"/>
    <xsl:variable name="provider" select="@provider"/>
    <xsl:variable name="credential" select="ancestor::Content//Credentials[@provider=$provider]"/>
    <xsl:choose>
      <xsl:when test="$credential">
        <p>
          <strong>
            <xsl:text>Connected to </xsl:text>
            <xsl:value-of select="$provider"/>
            <xsl:if test="$credential/ScreenName!=''">
              <xsl:variable name="name">
                <xsl:choose>
                  <xsl:when test="$credential/Name!=''">
                    <xsl:value-of select="$credential/Name"/>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>@</xsl:text>
                    <xsl:value-of select="$credential/ScreenName"/>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:variable>
              <xsl:text> as </xsl:text>
              <a href="http://twitter.com/{$credential/ScreenName}" title="View the Twitter profile for @{$credential/ScreenName}">
                <xsl:value-of select="$name"/>
              </a>
            </xsl:if>
          </strong>
        </p>
        <p>
          <a href="?ewCmd=UserIntegrations.Permissions&amp;dirId={$dirId}&amp;provider={$provider}" class="adminButton">
            <xsl:text>Manage permissions</xsl:text>
          </a>
          <xsl:text> </xsl:text>
          <a href="?ewCmd=UserIntegrations&amp;ewCmd2=removeIntegration&amp;dirId={$dirId}&amp;integration={$provider}.DeleteCredentials" class="adminButton">
            <xsl:text>Remove integration link</xsl:text>
          </a>
        </p>
      </xsl:when>
      <xsl:otherwise>
        <p>
          <a href="?ewCmd=UserIntegrations&amp;ewCmd2=connect&amp;dirId={$dirId}&amp;integration={$provider}.GetRequestToken" class="adminButton">
            <img src="/ewcommon/images/integrations/sign-in-with-twitter-d.png"/>
          </a>
        </p>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Holding template for unimplemented integration controls -->
  <xsl:template match="input" mode="integrationControl">
    <div class="alert">
      <xsl:text>User integration for the following provider could not be found: </xsl:text>
      <xsl:value-of select="@provider"/>
    </div>
  </xsl:template>


  <!-- BILLING ADDRESS - POSTCODE ANYWHERE -->
  <!-- Input xForm control -->
  <xsl:template match="input[contains(@class,'getAddress')]" mode="xform_control">
    <xsl:variable name="inlineHint">
      <xsl:apply-templates select="." mode="getInlineHint"/>
    </xsl:variable>
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <input type="text" name="{$ref}" id="{$ref}">
      <xsl:if test="contains(@class,'readonly')">
        <xsl:attribute name="readonly">readonly</xsl:attribute>
      </xsl:if>
      <xsl:choose>
        <xsl:when test="@class!=''">
          <xsl:attribute name="class">
            <xsl:value-of select="@class"/>
            <xsl:text> textbox</xsl:text>
          </xsl:attribute>
        </xsl:when>
        <xsl:otherwise>
          <xsl:attribute name="class">textbox</xsl:attribute>
        </xsl:otherwise>
      </xsl:choose>
      <xsl:choose>

        <xsl:when test="value!=''">
          <xsl:attribute name="value">
            <xsl:value-of select="value"/>
          </xsl:attribute>
        </xsl:when>

        <xsl:otherwise>

          <xsl:attribute name="value">
            <xsl:value-of select="$inlineHint"/>
          </xsl:attribute>

          <xsl:if test="$inlineHint!=''">
            <xsl:attribute name="onfocus">
              <xsl:text>if(this.value=='</xsl:text>
              <xsl:call-template name="escape-js">
                <xsl:with-param name="string" select="$inlineHint"/>
              </xsl:call-template>
              <xsl:text>'){this.value=''}</xsl:text>
            </xsl:attribute>
          </xsl:if>

        </xsl:otherwise>
      </xsl:choose>

    </input>
    <xsl:variable name="accountId">
      <xsl:call-template name="getSettings">
        <xsl:with-param name="sectionName" select="'web'"/>
        <xsl:with-param name="valueName" select="'AddressRetrievalAccountId'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="accountKey">
      <xsl:call-template name="getSettings">
        <xsl:with-param name="sectionName" select="'web'"/>
        <xsl:with-param name="valueName" select="'AddressRetrievalServiceKey'"/>
      </xsl:call-template>
    </xsl:variable>
    <!--<SCRIPT LANGUAGE="JAVASCRIPT" SRC="https://services.postcodeanywhere.co.uk/popups/javascript.aspx?account_code={$accountId}&amp;license_key={$accountKey}"></SCRIPT>-->

  </xsl:template>

  <!-- output code that is specified in xforms -->
  <xsl:template match="script" mode="xform">
    <xsl:apply-templates select="." mode="cleanXhtml" />
  </xsl:template>





  <xsl:template match="group[contains(@class,'colapse-repeat')]" mode="xform">
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

  <xsl:template match="input[not(contains(@class,'hidden'))] | secret | select | select1 | range | textarea | upload" mode="hintButton">
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

  <xsl:template match="submit[@submission='oAuth']" mode="xform">
    <xsl:variable name="class">
      <xsl:text>btn</xsl:text>
      <xsl:if test="not(contains(@class,'btn-'))">
        <xsl:text> btn-success</xsl:text>
      </xsl:if>
      <xsl:if test="@class!=''">
        <xsl:text> </xsl:text>
        <xsl:value-of select="@class"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="name">
      <xsl:choose>
        <xsl:when test="@ref!=''">
          <xsl:value-of select="@ref"/>
        </xsl:when>
        <xsl:when test="@submission!=''">
          <xsl:value-of select="@submission"/>
        </xsl:when>
        <xsl:when test="@bind!=''">
          <xsl:value-of select="@bind"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>ewSubmit</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="icon">
      <xsl:choose>
        <xsl:when test="@icon!=''">
          <xsl:value-of select="@icon"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>fa-check</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="buttonValue">
      <xsl:choose>
        <xsl:when test="@value!=''">
          <xsl:value-of select="@value"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="label/node()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <button type="submit" name="{$name}" value="{$buttonValue}" class="{$class}"  onclick="disableButton(this);">
      <xsl:if test="@data-pleasewaitmessage != ''">
        <xsl:attribute name="data-pleasewaitmessage">
          <xsl:value-of select="@data-pleasewaitmessage"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@data-pleasewaitdetail != ''">
        <xsl:attribute name="data-pleasewaitdetail">
          <xsl:value-of select="@data-pleasewaitdetail"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="not(contains($class,'icon-right'))">
        <i class="fa {$icon} fa-white">
          <xsl:text> </xsl:text>
        </i>
        <xsl:text> </xsl:text>
      </xsl:if>
      <xsl:apply-templates select="label" mode="submitText"/>
        <xsl:text> </xsl:text>
        <i class="fa fa-arrow-right pull-right">
          <xsl:text> </xsl:text>
        </i>
    </button>
  </xsl:template>
  
</xsl:stylesheet>
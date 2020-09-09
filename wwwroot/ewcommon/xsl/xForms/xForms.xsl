<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">

  <!-- -->
  <!-- ========================== XFORM ========================== -->
  <!-- -->
  <xsl:template match="div" mode="xform">
    <li>
      <xsl:if test="./@class">
        <xsl:attribute name="class">
          <xsl:text>li-</xsl:text>
          <xsl:value-of select="./@class"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates select="node()" mode="cleanXhtml"/>
    </li>
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

  <xsl:template match="Content[@type='xform']" mode="tinyMCEinit">
    <!-- tinyMCE - Now handled by jquery in commonV4_2.js -->
  </xsl:template>
  <!-- -->
  <!-- ========================== GROUP ========================== -->
  <!-- -->
  <xsl:template match="group | repeat" mode="xform">
    <xsl:param name="class"/>
    <fieldset>
      <xsl:if test="$class!='' or @class!='' ">
        <xsl:attribute name="class">
          <xsl:value-of select="$class"/>
          <xsl:if test="@class!=''">
            <xsl:text> </xsl:text>
            <xsl:value-of select="@class"/>
          </xsl:if>
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates select="." mode="editXformMenu"/>
      <xsl:if test="label">
        <xsl:apply-templates select="label[position()=1]" mode="legend"/>
      </xsl:if>
      <ol>
        <xsl:for-each select="input | secret | select | select1 | range | textarea | upload | group | repeat | hint | help | alert | div | repeat | relatedContent | label[position()!=1] | trigger | script">
          <xsl:choose>
            <xsl:when test="name()='group'">
              <li>
                <xsl:if test="./@class">
                  <xsl:attribute name="class">
                    <xsl:text>li-</xsl:text>
                    <xsl:value-of select="./@class"/>
                  </xsl:attribute>
                </xsl:if>
                <xsl:apply-templates select="." mode="xform"/>
              </li>
            </xsl:when>
            <xsl:otherwise>
              <xsl:apply-templates select="." mode="xform"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:for-each>
        <xsl:if test="count(submit) &gt; 0">
          <li>
            <xsl:if test="ancestor::group/descendant-or-self::*[contains(@class,'required')]">
              <label class="required">
                <span class="req">*</span>
                <xsl:text> </xsl:text>
                <xsl:call-template name="msg_required"/>
              </label>
            </xsl:if>
            <!-- For xFormQuiz change how these buttons work -->
            <xsl:apply-templates select="submit" mode="xform"/>
            <!-- Terminus needed for CHROME ! -->
            <!-- Terminus needed for BREAKS IE 7! -->
            <xsl:if test="$browserVersion!='MSIE 7.0'">
              <div class="terminus">&#160;</div>
            </xsl:if>
          </li>
        </xsl:if>
      </ol>
    </fieldset>
  </xsl:template>

  <xsl:template match="label" mode="legend">
    <legend>
      <xsl:apply-templates select="." mode="cleanXhtml"/>
    </legend>
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

    <ol id="{translate(@id,'[]#=/','')}-dependant">


      <!-- IF CHOSEN CASE - HIDE-->
      <xsl:attribute name="class">
        <xsl:value-of select="$dependantClass" />
        <xsl:if test="@id!=$selectedCase and not(descendant-or-self::alert)">
          <xsl:text> hidden</xsl:text>
        </xsl:if>
      </xsl:attribute>


      <xsl:apply-templates select="label | input | secret | select | select1 | range | textarea | upload | group | repeat | hint | help | alert | div | repeat | relatedContent | label[position()!=1] | trigger" mode="xform"/>

      <xsl:if test="count(submit) &gt; 0">
        <li>
          <xsl:if test="descendant-or-self::*[contains(@class,'required')]">
            <label class="required">
              <span class="req">*</span>
              <xsl:text> </xsl:text>
              <xsl:call-template name="msg_required"/>
            </label>
          </xsl:if>

          <xsl:apply-templates select="submit" mode="xform"/>
        </li>
      </xsl:if>
    </ol>

  </xsl:template>

  <xsl:template name="msg_required">
    <!-- required input-->
    <xsl:call-template name="term1023"/>
  </xsl:template>


  <!-- ========================== GROUP Horizontal ========================== -->
  <!-- -->
  <xsl:template match="group[contains(@class,'horizontal_cols') and parent::group] | repeat[contains(@class,'horizontal_cols') and parent::group]" mode="xform">
    <li>
      <table cellspacing="0">
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
              <xsl:if test="label">
                <th>&#160;</th>
              </xsl:if>
              <xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | hint | help | alert" mode="xform_header"/>
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
    </li>
    <xsl:if test="count(submit | trigger) &gt; 0">
      <li>
        <!-- For xFormQuiz change how these buttons work -->
        <xsl:apply-templates select="submit | trigger" mode="xform"/>
      </li>
    </xsl:if>
  </xsl:template>
  <!-- -->
  <!-- ========================== GROUP In Columns ========================== -->

  <xsl:template match="group[contains(@class,'2col') or contains(@class,'2Col') or contains(@class,'3col')]" mode="xform">
    <fieldset>
      <xsl:attribute name="class">
        <xsl:text>cols</xsl:text>
        <xsl:value-of select="count(group)"/>
      </xsl:attribute>
      <!--<xsl:apply-templates select="." mode="editXformMenu"/>-->
      <xsl:if test="label">
        <legend>
          <xsl:copy-of select="label/node()"/>
        </legend>
      </xsl:if>
      <xsl:for-each select="group">
        <xsl:apply-templates select="." mode="xform">
          <xsl:with-param name="class">
            <xsl:text>col</xsl:text>
            <xsl:value-of select="position()"/>
          </xsl:with-param>
        </xsl:apply-templates>
      </xsl:for-each>
    </fieldset>
  </xsl:template>



  <!-- -->
  <!-- ========================== GROUP In Tabs ========================== -->
  <xsl:template match="group[contains(@class,'xform-tabs')]" mode="xform">
    <div>
      <ul class="nav nav-tabs" role="tablist">
        <xsl:for-each select="group">
          <li role="presentation" class="active">
            <a href="#tab-{@id}" aria-controls="home" role="tab" data-toggle="tab">
              <xsl:apply-templates select="label"/>
            </a>
          </li>
        </xsl:for-each>
      </ul>
      <div class="tab-content">
        <xsl:for-each select="group">
          <div role="tabpanel" class="tab-pane active" id="tab-{@id}">
            <xsl:apply-templates select="." mode="xform"/>
          </div>
        </xsl:for-each>
      </div>
    </div>
  </xsl:template>
  
  
  <xsl:template match="input | secret | select | select1 | range | textarea | upload" mode="xform_header">
    <xsl:variable name="bind" select="@bind"/>
    <h3>
      <xsl:apply-templates select="label">
        <xsl:with-param name="cLabel">
          <xsl:value-of select="@ref"/>
        </xsl:with-param>
        <!-- NB : the count(item)!=1 basically stops you from making a one checkbox field (ie a boolean) from being required -->
        <xsl:with-param name="bRequired">
          <xsl:if test="(contains(@class,'required') and count(item)!=1) or ancestor::Content/model/bind[@id=$bind]/@required='true()'">true</xsl:if>
        </xsl:with-param>
      </xsl:apply-templates>
    </h3>
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
    <li>
      <span class="hint">
        <xsl:copy-of select="node()"/>
      </span>
    </li>
  </xsl:template>
  <xsl:template match="help" mode="xform">
    <li>
      <span class="help">
        <xsl:copy-of select="node()"/>
      </span>
    </li>
  </xsl:template>

  <xsl:template match="alert" mode="xform">
    <!--<xsl:if test="node()!=''">-->
    <li>
      <!--<span class="alert">
        <xsl:copy-of select="node()"/>
      </span>-->
      <span class="alert">
        <xsl:choose>
          <xsl:when test="span[contains(@class,'msg-')]">
            <!-- Send to system translations templates -->
            <xsl:apply-templates select="span" mode="term"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="node()" mode="cleanXhtml"/>
          </xsl:otherwise>
        </xsl:choose>
      </span>
    </li>
    <!--</xsl:if>-->
  </xsl:template>
  <!-- -->
  <!-- ========================== GENERAL : CONTROLS ========================== -->
  <!-- -->

  <xsl:template match="input | secret | select | select1 | range | textarea | upload" mode="xform">
    <xsl:apply-templates select="." mode="editXformMenu"/>
    <li>
      <xsl:if test="@class!=''">
        <xsl:attribute name="class">
          <xsl:text>li-</xsl:text>
          <xsl:value-of select="@class"/>
        </xsl:attribute>
      </xsl:if>
      <!-- NB : the count(item)!=1 basically stops you from making a one checkbox field (ie a boolean) from being required -->
      <xsl:apply-templates select="label">
        <xsl:with-param name="cLabel">
          <xsl:apply-templates select="." mode="getRefOrBind"/>
        </xsl:with-param>
        <xsl:with-param name="bRequired">
          <xsl:if test="contains(@class,'required') and count(item)!=1">true</xsl:if>
        </xsl:with-param>
      </xsl:apply-templates>
      <xsl:apply-templates select="." mode="xform_control"/>
      <xsl:if test="not(contains(@class,'pickImage'))">
        <xsl:apply-templates select="self::node()[not(item[toggle])]" mode="xform_legend"/>
      </xsl:if>
    </li>

  </xsl:template>


  <!-- -->
  <!-- ========================== GENERAL : CONTROL LEGEND ========================== -->
  <!-- -->
  <xsl:template match="input[not(contains(@class,'hidden'))] | secret | select | select1 | range | textarea | upload " mode="xform_legend">
    <xsl:if test="alert">
      <span class="alert">
        <xsl:apply-templates select="alert/node()" mode="cleanXhtml"/>
      </span>
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

  <xsl:template match="submit[@ref!='']" mode="xform">
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
  </xsl:template>

  <xsl:template match="submit" mode="xform">
    <xsl:variable name="class">
      <xsl:text>button</xsl:text>
      <xsl:if test="@class!=''">
        <xsl:text> </xsl:text>
        <xsl:value-of select="@class"/>
      </xsl:if>
    </xsl:variable>
    <input type="submit" name="{@submission}" class="{$class}"  onclick="disableButton(this);">
      <xsl:attribute name="value">
        <xsl:apply-templates select="label" mode="submitText"/>
      </xsl:attribute>
    </input>
  </xsl:template>

  <!-- for overloading in translations -->
  <xsl:template match="label" mode="submitText">
    <xsl:value-of select="node()"/>
  </xsl:template>

  <xsl:template match="submit[contains(@class,'PermissionButton') and /Page/@adminMode='true']" mode="xform">
    <xsl:variable name="class">
      <xsl:text>btn btn-primary</xsl:text>
      <xsl:if test="@class!=''">
        <xsl:text> </xsl:text>
        <xsl:value-of select="@class"/>
      </xsl:if>
    </xsl:variable>
    <button type="submit" name="{@submission}" value="{label/node()}" class="{$class}"  onclick="disableButton(this);">
      <xsl:apply-templates select="label" mode="submitText"/>
    </button>

  </xsl:template>

  <xsl:template match="submit[ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="xform">
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

  <xsl:template match="submit[contains(@class,'principle') and (ancestor::Page[@cssFramework='bs3' or @adminMode='true'])]" mode="xform">
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
    <button type="submit" name="{$name}" value="{label/node()}" class="{$class}"  onclick="disableButton(this);">
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



  <xsl:template match="submit[@class='principle' and @ref!='' and (ancestor::Page[@cssFramework='bs3' or @adminMode='true'])]" mode="xform">
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
    <button type="submit" name="{$name}" value="{label/node()}" class="{$class}"  onclick="disableButton(this);">
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
    <li class="hidden">
      <input type="hidden" name="{$ref}" id="{$ref}" value="{$value}"/>
    </li>
  </xsl:template>
  <!-- -->
  <!-- ========================== CONTROL : INPUT TEXT ========================== -->
  <!-- -->
  <!--xsl:template match="input[contains(@class,'textbox')]" mode="xform_control"-->

  <xsl:template name="msg_required_inline">Please enter </xsl:template>

  <xsl:template match="*" mode="getInlineHint">
    <xsl:value-of select="@placeholder"/>
  </xsl:template>


  <!-- This is so we can copy out an easy template to remove inline hints from ALL Forms! -->
  <!--<xsl:template match="input | textarea | select1" mode="getInlineHint">
    <xsl:variable name="label_low" select="translate(label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
    <xsl:choose>
      <xsl:when test="contains(@class,'keep_empty')"></xsl:when>
      <xsl:when test="hint[@class='inline']">
        <xsl:value-of select="hint[@class='inline']/node()"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="msg_required_inline"/>
        <xsl:value-of select="$label_low"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>-->

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
          <xsl:choose>
            <xsl:when test="$page[@cssFramework='bs3']">
              <xsl:if test="$inlineHint!=''">
                <xsl:attribute name="placeholder">
                  <xsl:value-of select="$inlineHint"/>
                </xsl:attribute>
              </xsl:if>
            </xsl:when>
            <xsl:otherwise>
              <xsl:attribute name="value">
                <xsl:value-of select="$inlineHint"/>
              </xsl:attribute>

              <!-- Only need onfocus event if inline hint not empty -->
              <xsl:if test="$inlineHint!='' and not(contains(@class,'no-clear'))">
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


        </xsl:otherwise>
      </xsl:choose>

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
  <!-- ========================== CONTROL : PickImage ========================== -->
  <!-- -->
  <xsl:template match="input[contains(@class,'pickImage')]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="niceImg">
      <xsl:apply-templates select="value/img" mode="jsNiceImage"/>
    </xsl:variable>
    <xsl:apply-templates select="self::node()[not(item[toggle])]" mode="xform_legend"/>
    <div class="input-group" id="editImage_{$ref}">
      <span class="input-group-btn">
        <a href="#" onclick="xfrmClearImage('{ancestor::Content/model/submission/@id}','{$ref}','{value/*/@class}');return false" title="edit an image from the image library" class="btn btn-default">
          <i class="fa fa-times fa-white">
            <xsl:text> </xsl:text>
          </i>
        </a>
      </span>
      <textarea name="{$ref}" id="{$ref}" readonly="readonly">
        <xsl:attribute name="class">
          <xsl:text>form-control pickImageInput </xsl:text>
          <xsl:value-of select="@class"/>
        </xsl:attribute>
        <xsl:text></xsl:text>
        <xsl:apply-templates select="value/img" mode="jsNiceImage"/>
        <xsl:text> </xsl:text>
      </textarea>
      <!--<script language="javascript" type="text/javascript">
      document.forms['<xsl:value-of select="ancestor::Content/model/submission/@id"/>'].elements['<xsl:value-of select="$ref"/>'].value = '<xsl:apply-templates select="value/img" mode="jsNiceImage"/>';
      -->
      <!--<xsl:comment>
        function OpenWindow_edit_<xsl:value-of select="$ref"/>(){
        imgHtml = document.forms['<xsl:value-of select="ancestor::Content/model/submission/@id"/>'].elements['<xsl:value-of select="$ref"/>'].value
        OpenWindow('/ewcommon/admin/popup.ashx?ewCmd=ImageLib<![CDATA[&]]>targetForm=<xsl:value-of select="ancestor::Content/model/submission/@id"/><![CDATA[&ewCmd2=editImage&imgHtml=' + imgHtml + '&]]>targetField=<xsl:value-of select="$ref"/><![CDATA[&]]>targetClass=<xsl:value-of select="value/*/@class"/>','Gallery','toolbar=yes,scrollbars=1,resize=yes,location=yes,menubar=yes,width=800,height=650' );
        };
        function OpenWindow_pick_<xsl:value-of select="$ref"/>(){
        OpenWindow('/ewcommon/admin/popup.ashx?ewCmd=ImageLib<![CDATA[&]]>targetForm=<xsl:value-of select="ancestor::Content/model/submission/@id"/><![CDATA[&]]>targetField=<xsl:value-of select="$ref"/><![CDATA[&]]>targetClass=<xsl:value-of select="value/*/@class"/>','Gallery','toolbar=yes,scrollbars=1,resize=yes,location=yes,menubar=yes,width=800,height=650' );
        };
      </xsl:comment>-->
      <!--
    </script>-->
      <xsl:choose>
        <xsl:when test="value/img/@src!=''">
          <span class="input-group-btn editpick">
            <!--<a href="#" onclick="OpenWindow_edit_{$ref}('');return false;" title="edit an image from the image library" class="btn btn-primary">-->
            <a class="btn btn-primary editImage">
              <i class="fa fa-picture-o fa-white">
                <xsl:text> </xsl:text>
              </i><xsl:text> </xsl:text>Edit
            </a>
          </span>
        </xsl:when>
        <xsl:otherwise>
          <span class="input-group-btn editpick">
            <!--<a href="#" onclick="OpenWindow_pick_{$ref}();return false;" title="pick an image from the image library" class="btn btn-primary">-->
            <a data-toggle="modal" href="?contentType=popup&amp;ewCmd=ImageLib&amp;targetForm={ancestor::Content/model/submission/@id}&amp;targetField={$ref}&amp;targetClass={value/*/@class}" data-target="#modal-{$ref}" class="btn btn-primary">
              <i class="fa fa-picture-o fa-white">
                <xsl:text> </xsl:text>
              </i><xsl:text> </xsl:text>Pick
            </a>
          </span>
        </xsl:otherwise>
      </xsl:choose>
    </div>
    <div class="previewImage" id="previewImage_{$ref}">
      <span>
        <!--<xsl:value-of select="value/img"/>-->
        <xsl:apply-templates select="value/img" mode="jsNiceImageForm"/>
        <xsl:text> </xsl:text>
      </span>
    </div>
  </xsl:template>

  <xsl:template match="*" mode="xform_modal">
    <!--do nothing-->
  </xsl:template>

  <xsl:template match="input[contains(@class,'pickImage')]" mode="xform_modal">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBindForScript"/>
    </xsl:variable>
    <div id="modal-{$ref}" class="modal fade pickImageModal">
      <xsl:text> </xsl:text>
    </div>
  </xsl:template>

  <xsl:template match="textarea[contains(@class,'xhtml')]" mode="xform_modal">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBindForScript"/>
    </xsl:variable>
    <div id="modal-{$ref}" class="modal fade pickImageModal">
      <xsl:text> </xsl:text>
    </div>
  </xsl:template>

  <xsl:template match="img" mode="jsNiceImageForm">
    <xsl:variable name="imgUrl">
      <xsl:call-template name="resize-image">
        <xsl:with-param name="path" select="@src"/>
        <xsl:with-param name="max-width" select="'85'"/>
        <xsl:with-param name="max-height" select="'85'"/>
        <xsl:with-param name="file-prefix" select="'~ew/tn-'"/>
        <xsl:with-param name="file-suffix" select="''"/>
        <xsl:with-param name="quality" select="'100'"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="imgWidth">
      <xsl:call-template name="get-image-width">
        <xsl:with-param name="path">
          <xsl:value-of select="$imgUrl"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="imgHeight">
      <xsl:call-template name="get-image-height">
        <xsl:with-param name="path">
          <xsl:value-of select="$imgUrl"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="valt">
      <xsl:call-template name="escape-js">
        <xsl:with-param name="string">
          <xsl:value-of select="@alt"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <img src="{$imgUrl}" width="{$imgWidth}" height="{$imgHeight} " class="{@class}" alt="{$valt}"/>
  </xsl:template>

  <!-- -->
  <template name="test">
    <xsl:with-param name="string"/>
    <xsl:value-of select="$string"/>
  </template>


  <!-- -->
  <!-- ========================== CONTROL : PickImageFile ========================== -->
  <!-- -->
  <xsl:template match="input[contains(@class,'pickImageFile')]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="scriptRef">
      <xsl:apply-templates select="." mode="getRefOrBindForScript"/>
    </xsl:variable>
    <div class="input-group" id="editImageFile_{$ref}">
      <input name="{$ref}" id="{$ref}" value="{value/node()}" >
        <xsl:attribute name="class">
          <xsl:text>form-control </xsl:text>
          <xsl:value-of select="@class"/>
        </xsl:attribute>
      </input>
      <span class="input-group-btn">
        <xsl:choose>
          <xsl:when test="value!=''">
            <a href="#" onclick="xfrmClearImgFile('{ancestor::Content/model/submission/@id}','{$scriptRef}');return false" title="Clear the file path" class="btn btn-danger">
              <i class="fa fa-trash-o fa-white">
                <xsl:text> </xsl:text>
              </i><xsl:text> </xsl:text>Clear
            </a>

          </xsl:when>
          <xsl:otherwise>
            <a data-toggle="modal" href="?contentType=popup&amp;ewCmd=ImageLib&amp;ewCmd2=PathOnly&amp;targetForm={ancestor::Content/model/submission/@id}&amp;targetField={$scriptRef}&amp;targetClass={value/*/@class}" data-target="#modal-{$scriptRef}" class="btn btn-primary">
              <i class="fa fa-picture-o fa-white">
                <xsl:text> </xsl:text>
              </i><xsl:text> </xsl:text>Pick
            </a>
          </xsl:otherwise>
        </xsl:choose>
      </span>
    </div>

  </xsl:template>
  <!-- -->
  <!-- -->
  <!-- ========================== CONTROL : PickDocument ========================== -->
  <!-- -->
  <xsl:template match="input[contains(@class,'pickDocument')]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="scriptRef">
      <xsl:apply-templates select="." mode="getRefOrBindForScript"/>
    </xsl:variable>
    <div class="input-group" id="editDoc_{$ref}">
      <input name="{$ref}" id="{$ref}" value="{value/node()}">
        <xsl:attribute name="class">
          <xsl:text>form-control </xsl:text>
          <xsl:value-of select="@class"/>
        </xsl:attribute>
      </input>
      <span class="input-group-btn">
        <xsl:choose>
          <xsl:when test="value!=''">
            <a href="#" onclick="xfrmClearDocument('{ancestor::Content/model/submission/@id}','{$scriptRef}');return false" title="Clear the document path" class="btn btn-danger">
              <i class="fa fa-trash-o fa-white">
                <xsl:text> </xsl:text>
              </i><xsl:text> </xsl:text>Clear
            </a>

          </xsl:when>
          <xsl:otherwise>
            <a data-toggle="modal" href="?contentType=popup&amp;ewCmd=DocsLib&amp;targetForm={ancestor::Content/model/submission/@id}&amp;targetField={$scriptRef}&amp;targetClass={value/*/@class}" data-target="#modal-{$scriptRef}" class="btn btn-primary">
              <i class="fa fa-file-o fa-white">
                <xsl:text> </xsl:text>
              </i><xsl:text> </xsl:text>Pick
            </a>
          </xsl:otherwise>
        </xsl:choose>
      </span>
    </div>
    <!--<script language="javascript" type="text/javascript">

      document.forms['<xsl:value-of select="ancestor::Content/model/submission/@id"/>'].elements['<xsl:value-of select="$ref"/>'].value = '<xsl:call-template name="escape-js">
        <xsl:with-param name="string">
          <xsl:copy-of select="value/node()"/>
        </xsl:with-param>
      </xsl:call-template>';
      <xsl:comment>
        function OpenWindow_pick_<xsl:value-of select="$ref"/>(){
        imgHtml = document.forms['<xsl:value-of select="ancestor::Content/model/submission/@id"/>'].elements['<xsl:value-of select="$ref"/>'].value
        OpenWindow('/ewcommon/admin/popup.ashx?ewCmd=DocsLib<![CDATA[&]]>targetForm=<xsl:value-of select="ancestor::Content/model/submission/@id"/><![CDATA[&]]>targetField=<xsl:value-of select="$ref"/>','Documents','toolbar=yes,scrollbars=1,resize=yes,location=yes,menubar=yes,width=800,height=550' );
        };
      </xsl:comment>
    </script>-->
    <!--<xsl:choose>
      <xsl:when test="value!=''">
        <div class="previewImage" id="previewImage_{$ref}">
          <a href="#" onclick="xfrmClearDocument('{ancestor::Content/model/submission/@id}','{$ref}');return false" title="edit an image from the image library" class="adminButton delete">Clear Document</a>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <div class="previewImage" id="previewImage_{$ref}">
          <a href="#" onclick="OpenWindow_pick_{$ref}();return false" title="Pick Document from Library" class="adminButton add pickImage">Pick Document</a>
        </div>
      </xsl:otherwise>
    </xsl:choose>-->
  </xsl:template>

  <xsl:template match="input[contains(@class,'pickDocument')]" mode="xform_modal">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <div id="modal-{$ref}" class="modal fade pickImageModal">
      <xsl:text> </xsl:text>
    </div>
  </xsl:template>
  <!-- -->
  <!-- ========================== CONTROL : PickMedia ========================== -->
  <!-- -->
  <xsl:template match="input[contains(@class,'pickMedia')]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <input name="{$ref}" id="{$ref}" value=""/>
    <script language="javascript" type="text/javascript">
      document.forms['<xsl:value-of select="ancestor::Content/model/submission/@id"/>'].elements['<xsl:value-of select="$ref"/>'].value = '<xsl:copy-of select="value/node()"/>';
      <xsl:comment>
        function OpenWindow_pick_<xsl:value-of select="$ref"/>(){
        OpenWindow('?contentType=popup&amp;ewCmd=MediaLib<![CDATA[&]]>targetForm=<xsl:value-of select="ancestor::Content/model/submission/@id"/><![CDATA[&]]>targetField=<xsl:value-of select="$ref"/><![CDATA[&]]>targetClass=<xsl:value-of select="value/*/@class"/>','MediaLibrary','toolbar=yes,scrollbars=1,resize=yes,location=yes,menubar=yes,width=676,height=550' );
        };
      </xsl:comment>
    </script>
    <xsl:choose>
      <xsl:when test="value!=''">
        <div class="previewImage" id="previewImage_{$ref}">
          <a href="#" onclick="xfrmClearMedia('{ancestor::Content/model/submission/@id}','{$ref}');return false;" title="edit an image from the image library" class="adminButton delete">Clear Media</a>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <div class="previewImage" id="previewImage_{$ref}">
          <a href="#" onclick="OpenWindow_pick_{$ref}();return false" title="Pick Media from Library" class="adminButton add pickImage">Pick Media</a>
        </div>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="input[contains(@class,'pickMedia')]" mode="xform_modal">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBindForScript"/>
    </xsl:variable>
    <div id="modal-{$ref}" class="modal fade pickImageModal">
      <xsl:text> </xsl:text>
    </div>
  </xsl:template>


  <xsl:template match="input[contains(@class,'pickMedia')]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="scriptRef">
      <xsl:apply-templates select="." mode="getRefOrBindForScript"/>
    </xsl:variable>
    <div class="input-group" id="editDoc_{$ref}">
      <input name="{$ref}" id="{$ref}" value="{value/node()}">
        <xsl:attribute name="class">
          <xsl:text>form-control </xsl:text>
          <xsl:value-of select="@class"/>
        </xsl:attribute>
      </input>
      <span class="input-group-btn">
        <xsl:choose>
          <xsl:when test="value!=''">
            <a href="#" onclick="xfrmClearMedia('{ancestor::Content/model/submission/@id}','{$scriptRef}');return false" title="Clear the document path" class="btn btn-danger">
              <i class="fa fa-trash-o fa-white">
                <xsl:text> </xsl:text>
              </i><xsl:text> </xsl:text>Clear
            </a>

          </xsl:when>
          <xsl:otherwise>
            <a data-toggle="modal" href="?contentType=popup&amp;ewCmd=MediaLib&amp;targetForm={ancestor::Content/model/submission/@id}&amp;targetField={$scriptRef}&amp;targetClass={value/*/@class}" data-target="#modal-{$scriptRef}" class="btn btn-primary">
              <i class="fa fa-music fa-white">
                <xsl:text> </xsl:text>
              </i><xsl:text> </xsl:text>Pick
            </a>
          </xsl:otherwise>
        </xsl:choose>
      </span>
    </div>
  </xsl:template>
  <!-- -->
  <!-- ========================== CONTROL : Edit X Form ========================== -->
  <!-- -->
  <xsl:template match="input[contains(@class,'editXformButton')]" mode="xform_control">
    <!--input type="hidden" name="xml" value="x"/-->
    <input type="submit" name="submit" value="Edit Questions" class="adminButton"/>
    <!--a href="?ewCmd=EditXForm&amp;artid={/Page/Request/QueryString/Item[@name='id']/node()}" class="textButton">Click Here to Edit this Form</a-->
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
    <select name="month_{$ref}" id="month_{$ref}" onchange="makedatemmyy('{$formName}','{$ref}')" class="dropdown month">
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
    <select name="year_{$ref}" onchange="makedatemmyy('{$formName}','{$ref}')" class="dropdown year">
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
    <select name="month_{$ref}" id="month_{$ref}" onchange="makedatemmyy('{$formName}','{$ref}')" class="dropdown month">
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
    <select name="year_{$ref}" onchange="makedatemmyy('{$formName}','{$ref}')" class="dropdown year">
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
  <!-- ========================== CONTROL : CC Expire Date ========================== -->
  <!-- -->
  <xsl:template match="input[contains(@class,'ccExpire') and ancestor::Page[@cssFramework='bs3']]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="formName">
      <xsl:value-of select="ancestor::Content/model/submission/@id"/>
    </xsl:variable>
    <select name="month_{$ref}" id="month_{$ref}" onchange="makedatemmyy('{$formName}','{$ref}')" class="form-control month" autocomplete="cc-exp-month">
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
    <select name="year_{$ref}" onchange="makedatemmyy('{$formName}','{$ref}')" class="form-control year" autocomplete="cc-exp-year">
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
  <xsl:template match="input[contains(@class,'ccIssue') and ancestor::Page[@cssFramework='bs3']]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="formName">
      <xsl:value-of select="ancestor::Content/model/submission/@id"/>
    </xsl:variable>
    <select name="month_{$ref}" id="month_{$ref}" onchange="makedatemmyy('{$formName}','{$ref}')" class="form-control month" onfocus="this.removeAttribute('readonly');" readonly="readonly">
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
    <select name="year_{$ref}" onchange="makedatemmyy('{$formName}','{$ref}')" class="form-control year" onfocus="this.removeAttribute('readonly');" readonly="readonly">
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
    <div class="input-group">
      <input type="text" name="{$ref}" id="{$ref}" value="{value/node()}" class="hidden">
        <xsl:text></xsl:text>
      </input>
      <input type="text" name="{$ref}-alt" id="{$ref}-alt" value="{$displayDate}" readonly="readonly" class="form-control">
        <xsl:attribute name="class">
          <xsl:text>jqDatePicker</xsl:text>
          <xsl:if test="@class!=''">
            <xsl:text> </xsl:text>
            <xsl:value-of select="@class"/>
          </xsl:if>
          <!--<xsl:if test="contains(@class,'required')">
          <xsl:choose>
            <xsl:when test="ancestor::switch and contains(@class,'required')">
              <xsl:apply-templates select="." mode="isRequired"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="@class"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>-->
        </xsl:attribute>
        <xsl:text></xsl:text>
      </input>
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
    <input type="text" name="{$ref}" id="{$ref}" value="{value/node()}" class="hidden">
      <xsl:text></xsl:text>
    </input>
    <input type="text" name="{$ref}-alt" id="{$ref}-alt" value="{$displayDate}" readonly="readonly">
      <xsl:attribute name="class">
        <xsl:text>jqDOBPicker</xsl:text>
        <xsl:if test="@class!=''">
          <xsl:text> </xsl:text>
          <xsl:value-of select="@class"/>
        </xsl:if>
        <!--<xsl:if test="contains(@class,'required')">
          <xsl:choose>
            <xsl:when test="ancestor::switch and contains(@class,'required')">
              <xsl:apply-templates select="." mode="isRequired"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="@class"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:if>-->
      </xsl:attribute>
      <xsl:text></xsl:text>
    </input>
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


  <!-- ========================== CONTROL : Google Product Taxonimy ========================== -->
  <!-- -->
  <xsl:template match="input[contains(@class,'googleProductCategories')]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="formName">
      <xsl:value-of select="ancestor::Content/model/submission/@id"/>
    </xsl:variable>
    <input type="text" name="{$ref}-id" id="{$ref}-id" value="{value/node()}" class="googleProductCategories">

    </input>
    <input type="text" name="{$ref}-value" id="googleProductCategories-value" value="{value/node()}">

    </input>

    <!--input type="text" name="{$ref}" id="googleProductCategories-result" value="{value/node()}">
    
    </input-->
    <!--script type="text/javascript" src="/ewcommon/js/jQuery/jquery-option-tree/jquery.optionTree.js">&#160;</script-->
  </xsl:template>

  <!-- -->
  <!-- ========================== CONTROL : xformQuizSel1Ans ========================== -->
  <!-- -->
  <xsl:template match="input[contains(@class,'xformquizSel1Ans')]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="formName">
      <xsl:value-of select="ancestor::Content/model/submission/@id"/>
    </xsl:variable>
    <textarea rows="2" cols="60" name="{$ref}" id="{$ref}" >
      <xsl:value-of select="value/node()"/>
    </textarea>
    <input type="radio" name="correctAns" value="{$ref}"/>
    weighting: <input type="text" name="weighting-{$ref}" id="{$ref}" value="" size="3"/>
  </xsl:template>
  <!-- -->
  <!-- ========================== CONTROL : xformQuizSelAns ========================== -->
  <!-- -->
  <xsl:template match="input[contains(@class,'xformquizSelAns')]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="formName">
      <xsl:value-of select="ancestor::Content/model/submission/@id"/>
    </xsl:variable>
    <textarea rows="2" cols="60" name="{$ref}" id="{$ref}" >
      <xsl:value-of select="value/node()"/>
    </textarea>
    <input type="checkbox" name="correctAns-{$ref}" value="{$ref}"/>
    weighting: <input type="text" name="weighting-{$ref}" id="{$ref}" value="" size="3"/>
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
      <br />
      <a id="passwordPolicy" href="#">Password policy</a>
    </xsl:if>
  </xsl:template>
  <!-- -->
  <xsl:template match="secret[contains(@class,'textbox')]" mode="xform_control">
    <xsl:variable name="inlineHint">
      <xsl:apply-templates select="." mode="getInlineHint"/>
    </xsl:variable>
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
          <xsl:choose>
            <xsl:when test="$page[@cssFramework='bs3']">
              <xsl:if test="$inlineHint!=''">
                <xsl:attribute name="placeholder">
                  <xsl:value-of select="$inlineHint"/>
                </xsl:attribute>
              </xsl:if>
            </xsl:when>
            <xsl:otherwise>
              <xsl:if test="$inlineHint!=''">
                <xsl:attribute name="onfocus">
                  <xsl:text>if(this.value=='</xsl:text>
                  <xsl:call-template name="escape-js">
                    <xsl:with-param name="string" select="$inlineHint"/>
                  </xsl:call-template>
                  <xsl:text>'){this.value=''}</xsl:text>
                </xsl:attribute>
              </xsl:if>
              <xsl:value-of select="$inlineHint"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:otherwise>
      </xsl:choose>
    </textarea>
  </xsl:template>
  <!-- -->
  <xsl:template match="textarea[contains(@class,'readonly')]" mode="xform_control">
    <div class="textareaReadOnly">
      <xsl:copy-of select="value/node()"/>
    </div>
  </xsl:template>

  <!-- TinyMCE configuration templates -->
  <xsl:template match="textarea" mode="tinymceGeneralOptions">
    <xsl:text>script_url: '/ewcommon/js/tinymce/tinymce.min.js',
			mode: "exact",
			theme: "modern",
			width: "auto",
      content_css: ['/ewcommon/js/tinymce/plugins/leaui_code_editor/css/pre.css'],
			relative_urls: false,
			plugins: "table paste link image ewimage media visualchars searchreplace emoticons anchor advlist code visualblocks contextmenu fullscreen searchreplace youtube leaui_code_editor",
			entity_enconding: "numeric",
      image_advtab: true,
      menubar: "edit insert view format table tools",
      toolbar: "insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image ewimage",
			convert_fonts_to_spans: true,
			gecko_spellcheck: true,
			theme_advanced_toolbar_location: "top",
			theme_advanced_toolbar_align: "left",
			paste_create_paragraphs: false,</xsl:text>
    <xsl:apply-templates select="." mode="tinymcelinklist"/>
    <xsl:text>
			paste_use_dialog: true,</xsl:text>
    <xsl:apply-templates select="." mode="tinymceStyles"/>
    <xsl:apply-templates select="." mode="tinymceContentCSS"/>
    <xsl:text>
			auto_cleanup_word: "true"</xsl:text>
  </xsl:template>

  <xsl:template match="textarea" mode="tinymcelinklist">

    <xsl:text>
    link_list: [</xsl:text>
    <xsl:apply-templates select="/Page/Menu/MenuItem" mode="tinymcelinklistitem">
      <xsl:with-param name="level" select="number(1)"/>
    </xsl:apply-templates>
    <xsl:text>],</xsl:text>

  </xsl:template>

  <xsl:template match="MenuItem" mode="tinymcelinklistitem">
    <xsl:param name="level"/>

    <xsl:text>{title: '</xsl:text>
    <xsl:call-template name="writeSpacers">
      <xsl:with-param name="spacers" select="$level"/>
    </xsl:call-template>
    <xsl:text>&#160;</xsl:text>
    <xsl:value-of select="@name"/>
    <xsl:text>', value: '</xsl:text>
    <xsl:choose>
      <xsl:when test="contains(@url,'?pgid')">
        <xsl:value-of select="substring-before(@url,'?pgid')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="@url"/>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:text>'}</xsl:text>

    <xsl:if test="count(child::MenuItem[substring-before(@url,'?pgid')!=''])&gt;0">
      <xsl:text>,
	</xsl:text>
      <xsl:if test="MenuItem[substring-before(@url,'?pgid')!='']">
        <xsl:apply-templates select="MenuItem[substring-before(@url,'?pgid')!='']" mode="tinymcelinklistitem">
          <xsl:with-param name="level" select="number($level+1)"/>
        </xsl:apply-templates>
      </xsl:if>
    </xsl:if>
    <xsl:if test="position()!=last()">
      <xsl:text>,
	</xsl:text>
    </xsl:if>
  </xsl:template>

  <!-- TinyMCE styles  - leave empty, overwrite as needed per site Example Follows www.tinymce.com/tryit/custom_formats.php -->
  <!--
  style_formats: [
  {title: 'Bold text', inline: 'b'},
  {title: 'Red text', inline: 'span', styles: {color: '#ff0000'}},
  {title: 'Red header', block: 'h1', styles: {color: '#ff0000'}},
  {title: 'Example 1', inline: 'span', classes: 'example1'},
  {title: 'Example 2', inline: 'span', classes: 'example2'},
  {title: 'Table styles'},
  {title: 'Table row 1', selector: 'tr', classes: 'tablerow1'}
  ],
  -->
  <xsl:template match="textarea" mode="tinymceStyles"></xsl:template>

  <xsl:template match="textarea" mode="tinymceContentCSS"></xsl:template>

  <!-- TinyMCE default configuration -->
  <xsl:template match="textarea" mode="tinymceButtons1">
    <xsl:text>cut,copy,paste,pastetext,pasteword,selectall,search,replace,separator,undo,</xsl:text>
    <xsl:text>redo,separator,removeformat,cleanup,separator,</xsl:text>
    <xsl:text>formatselect,</xsl:text>
  </xsl:template>

  <xsl:template match="textarea" mode="tinymceButtons2">
    <xsl:text>tablecontrols,separator,link,unlink,image,youtube,media,visualchars,separator,code,help</xsl:text>
  </xsl:template>

  <xsl:template match="textarea" mode="tinymceButtons3">
    <xsl:text>bold,italic,underline,strikethrough,separator,justifyleft,justifycenter,justifyright,justifyfull,</xsl:text>
    <xsl:text>separator,bullist,numlist,outdent,indent,hr,|,charmap,emotions,cite</xsl:text>
  </xsl:template>

  <!-- TinyMCE styles (e.g. "Style A=styleA;") - leave empty, overwrite as needed per site -->
  <xsl:template match="textarea" mode="tinymceStyles"></xsl:template>

  <xsl:template match="textarea" mode="tinymceValidElements">
    "a[href|target|title|style|class|onmouseover|onmouseout|onclick],"
    + "img[class|src|border=0|alt|title|hspace|vspace|width|height|align|onmouseover|onmouseout|name|style],"
    + "table[cellspacing|cellpadding|border|height|width|style|class],"
    + "p[align|style|class],"
    + "span[class],"
    + "form[action|method|name|style|class],object[*],param[*],embed[*],"
    + "input[type|value|name|style|class|src|alt|border|size],"
    + "textarea[type|value|name|style|class|src|alt|border|size|cols|rows],"
    + "td/th[colspan|rowspan|align|valign|style|class],"
    + "h1[style|class],h2[style|class],h3[style|class],h4[style|class],h5[style|class],h6[style|class],"
    + "ol[style|class],ul[style|class],li[style|class],div[align|style|class],span[style|class],"
    + "thead[style|class],tbody[style|class],tr[class],dd[style|class],dl[style|class],dt[style|class],"
    + "sup,sub,pre,address,strong,b,em,i[class],u,s,hr,blockquote,br,"
    + "cite[class|id|title],code[class|title],samp,iframe[width|height|src|frameborder|allowfullscreen]"
  </xsl:template>

  <!-- TinyMCE minimal configuration -->
  <xsl:template match="textarea[contains(@class, 'minxhtml')]" mode="tinymceButtons1">
    <xsl:text>bold,italic,underline,bullist,numlist</xsl:text>
  </xsl:template>

  <xsl:template match="textarea[contains(@class, 'minxhtml')]" mode="tinymceButtons2"></xsl:template>

  <xsl:template match="textarea[contains(@class, 'minxhtml')]" mode="tinymceButtons3"></xsl:template>

  <xsl:template match="textarea[contains(@class, 'minxhtml')]" mode="tinymceValidElements">
    <xsl:text>"a[href|target|title|style|class|onmouseover|onmouseout|onclick],"
			+ "p[align|style|class],"
			+ "span[class],"
			+ "h2[style|class],h3[style|class],h4[style|class],h5[style|class],h6[style|class],ol[style|class],"
			+ "ul[style|class],li[style|class],div[align|style|class],span[style|class],"
			+ "dd[style|class],dl[style|class],dt[style|class],sup,sub,pre,address,strong,b,em,i,u,s,hr,blockquote,br"
		</xsl:text>
  </xsl:template>

  <!-- TinyMCE configuration -->
  <xsl:template match="textarea" mode="tinymceConfig">
    <script type="text/javascript">
      $('#<xsl:apply-templates select="." mode="getRefOrBind"/>').tinymce({
      <xsl:apply-templates select="." mode="tinymceGeneralOptions"/>,
      theme_modern_buttons1: "<xsl:apply-templates select="." mode="tinymceButtons1"/>",
      theme_modern_buttons2: "<xsl:apply-templates select="." mode="tinymceButtons2"/>",
      theme_modern_buttons3: "<xsl:apply-templates select="." mode="tinymceButtons3"/>",
      theme_modern_blockformats : "p,h1,h2,h3,h4,h5,h6,blockquote,div,dt,dd,code,samp",
      valid_elements: <xsl:apply-templates select="." mode="tinymceValidElements"/>
      });
    </script>
  </xsl:template>


  <!-- Tiny MCE Control -->
  <xsl:template match="textarea[contains(@class,'xhtml')]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <textarea name="{$ref}" id="{$ref}">
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
      <xsl:if test="@class!=''">
        <xsl:attribute name="class">
          <xsl:value-of select="@class"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:apply-templates select="value/node()" mode="cleanXhtml"/>
      <xsl:text> </xsl:text>
    </textarea>
    <!--xsl:apply-templates select="." mode="tinymceConfig"/-->
  </xsl:template>

  <!-- TinyMCE configuration -->
  <xsl:template match="textarea" mode="xform_control_script">
    <script type="text/javascript">
      $('#<xsl:apply-templates select="." mode="getRefOrBind"/>').tinymce({
      <xsl:apply-templates select="." mode="tinymceGeneralOptions"/>,
      theme_modern_buttons1: "<xsl:apply-templates select="." mode="tinymceButtons1"/>",
      theme_modern_buttons2: "<xsl:apply-templates select="." mode="tinymceButtons2"/>",
      theme_modern_buttons3: "<xsl:apply-templates select="." mode="tinymceButtons3"/>",
      theme_modern_blockformats : "p,h1,h2,h3,h4,h5,h6,blockquote,div,dt,dd,code,samp",
      valid_elements: <xsl:apply-templates select="." mode="tinymceValidElements"/>
      });
    </script>
  </xsl:template>

  <xsl:template match="Page" mode="xform_control_scripts">
    <xsl:apply-templates select="descendant-or-self::textarea[contains(@class,'xhtml')]" mode="xform_control_script"/>
    <xsl:apply-templates select="descendant-or-self::textarea[contains(@class,'xml')]" mode="xform_control_script"/>
  </xsl:template>

  <!-- YouTube Video module embed field -->
  <xsl:template match="textarea[contains(@class,'xhtml') and contains(@class,'youtube')]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <textarea name="{$ref}" id="{$ref}">
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
      <xsl:if test="@class!=''">
        <xsl:attribute name="class">
          <xsl:value-of select="@class"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:copy-of select="value/node()"/>
      <xsl:text> </xsl:text>
    </textarea>
  </xsl:template>
  <!-- End of YouTube Video module embed field -->

  <!-- Minimal Tiny MCE Control For Front end editing - Simple formatting controls -->
  <xsl:template match="textarea[contains(@class,'minxhtml')]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <textarea name="{$ref}" id="{$ref}">
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
      <xsl:if test="@class!=''">
        <xsl:attribute name="class">
          <xsl:value-of select="@class"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:copy-of select="value/node()"/>
      <xsl:text> </xsl:text>
    </textarea>
    <xsl:apply-templates select="." mode="tinymceConfig"/>
  </xsl:template>

  <!-- CodeMirror Control -->
  <xsl:template match="textarea[contains(@class,'xml')]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <textarea name="{$ref}" id="{$ref}">
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
      <xsl:if test="@class!=''">
        <xsl:attribute name="class">
          <xsl:value-of select="@class"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:copy-of select="value/node()"/>
      <xsl:text> </xsl:text>
    </textarea>
  </xsl:template>


  <xsl:template match="textarea[contains(@class,'xml')]" mode="xform_control_script">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <script type="text/javascript">
      var editor = CodeMirror.fromTextArea('<xsl:value-of select="$ref"/>', {
      height: "<xsl:value-of select="number(@rows) * 25"/>px",
      parserfile: "parsexml.js",
      stylesheet: "/ewcommon/js/codemirror/css/xmlcolors.css",
      path: "/ewcommon/js/codemirror/",
      continuousScanning: 500,
      lineNumbers: true,
      reindentOnLoad: true,
      textWrapping: true,
      matchClosing: true
      });
    </script>
  </xsl:template>



  <!-- CodeMirror Control -->
  <xsl:template match="textarea[contains(@class,'xFormEditor')]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <textarea name="{$ref}" id="{$ref}">
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
      <xsl:if test="@class!=''">
        <xsl:attribute name="class">
          <xsl:value-of select="@class"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:copy-of select="value/node()"/>
      <xsl:text> </xsl:text>
    </textarea>
    <script type="text/javascript">
      $('#<xsl:value-of select="$ref"/>').xFormEditor();
    </script>
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

  <!-- deprecated and worked into the normal select1 template above -->
  <!--<xsl:template match="select1[@class='ewQuerys']" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="selectedValue">
      <xsl:value-of select="value/node()"/>
    </xsl:variable>
    <xsl:variable name="selectOptions">
      <xsl:apply-templates select="." mode="getSelectOptions"/>
    </xsl:variable>
    <select name="{$ref}" id="{$ref}" class="dropdown {selectedValue}">
      <xsl:if test="@class!=''">
        <xsl:attribute name="class">
          <xsl:value-of select="@class"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@onChange!=''">
        <xsl:attribute name="onChange">
          <xsl:value-of select="@onChange"/>
        </xsl:attribute>
      </xsl:if>
      <option value="">
        <xsl:variable name="label_low" select="translate(label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
        <xsl:text>Not Specified</xsl:text>
      </option>
      <xsl:apply-templates select="ms:node-set($selectOptions)/select1/item" mode="xform_select">
        <xsl:with-param name="selectedValue" select="$selectedValue"/>
      </xsl:apply-templates>
    </select> 
  
  </xsl:template>-->

  <!-- -->
  <xsl:template match="select1[@class='siteTree']" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="selectedValue">
      <xsl:value-of select="value/node()"/>
    </xsl:variable>
    <select name="{$ref}" id="{$ref}">

      <xsl:attribute name="class">
        <xsl:text>dropdown form-control </xsl:text>
        <xsl:if test="@class!=''">
          <xsl:value-of select="@class"/>
        </xsl:if>
      </xsl:attribute>

      <xsl:if test="@onChange!=''">
        <xsl:attribute name="onChange">
          <xsl:value-of select="@onChange"/>
        </xsl:attribute>
      </xsl:if>
      <option value="">
        <xsl:variable name="label_low" select="translate(label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
        <xsl:text>Not Specified</xsl:text>
      </option>
      <xsl:apply-templates select="/Page/Menu/MenuItem" mode="listPageOption">
        <xsl:with-param name="level" select="number(1)"/>
        <xsl:with-param name="selectedValue" select="$selectedValue"/>
      </xsl:apply-templates>
      <!--<xsl:apply-templates select="item" mode="xform_select"/>-->
    </select>
  </xsl:template>
  <!-- -->
  <xsl:template match="select1[@class='siteTreeName']" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="selectedValue">
      <xsl:value-of select="value/node()"/>
    </xsl:variable>
    <select name="{$ref}" id="{$ref}">
      <xsl:attribute name="class">
        <xsl:text>dropdown form-control </xsl:text>
        <xsl:if test="@class!=''">
          <xsl:value-of select="@class"/>
        </xsl:if>
      </xsl:attribute>
      <xsl:if test="@onChange!=''">
        <xsl:attribute name="onChange">
          <xsl:value-of select="@onChange"/>
        </xsl:attribute>
      </xsl:if>
      <option value="">
        <xsl:variable name="label_low" select="translate(label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
        <xsl:text>Not Specified</xsl:text>
      </option>
      <xsl:apply-templates select="/Page/Menu/MenuItem" mode="listPageOptionName">
        <xsl:with-param name="level" select="number(1)"/>
        <xsl:with-param name="selectedValue" select="$selectedValue"/>
      </xsl:apply-templates>
      <!--<xsl:apply-templates select="item" mode="xform_select"/>-->
    </select>
  </xsl:template>

  <!-- In Admin - Allow for default SITE box styles -->
  <xsl:template match="select1[@appearance='minimal' and contains(@class,'boxStyle')][ancestor::Page[@adminMode='true']]" mode="xform_control">
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
      <xsl:choose>
        <xsl:when test="contains(@class,'alphasort')">
          <xsl:apply-templates select="item" mode="xform_select">
            <xsl:sort select="label" order="ascending"/>
          </xsl:apply-templates>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="item" mode="xform_select"/>
        </xsl:otherwise>
      </xsl:choose>
      <!-- Need this to be able to Add Site Specific Box Styles -->
      <xsl:choose>
        <xsl:when test="$page/@editContext='NormalMail'">
          <xsl:apply-templates select="/" mode="mailBoxStyles">
            <xsl:with-param name="value" select="value/node()" />
          </xsl:apply-templates>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="/" mode="siteBoxStyles">
            <xsl:with-param name="value" select="value/node()" />
          </xsl:apply-templates>
        </xsl:otherwise>
      </xsl:choose>
    </select>
  </xsl:template>

  <!-- -->
  <xsl:template match="/" mode="siteBoxStyles">
    <xsl:param name="value" />

    <option value="bespokeBox">
      <xsl:if test="$value='bespokeBox'">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:text>site's bespoke box</xsl:text>
    </option>

  </xsl:template>

  <!-- -->
  <xsl:template match="/" mode="mailBoxStyles">
    <xsl:param name="value" />

    <option value="bespokeBox">
      <xsl:if test="$value='bespokeBox'">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:text>site's bespoke box</xsl:text>
    </option>

  </xsl:template>

  <xsl:template match="MenuItem" mode="listPageOptionName">
    <xsl:param name="level"/>
    <xsl:param name="selectedValue"/>
    <option>
      <xsl:attribute name="value">
        <xsl:value-of select="@name"/>
      </xsl:attribute>
      <xsl:if test="$selectedValue=@name">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:call-template name="writeSpacers">
        <xsl:with-param name="spacers" select="$level"/>
      </xsl:call-template>
      <xsl:text>|-&#160;</xsl:text>
      <xsl:value-of select="@name"/>
      <xsl:text> </xsl:text>
    </option>
    <xsl:if test="count(child::MenuItem)&gt;0">
      <xsl:apply-templates select="MenuItem" mode="listPageOptionName">
        <xsl:with-param name="level" select="number($level+1)"/>
        <xsl:with-param name="selectedValue" select="$selectedValue"/>
      </xsl:apply-templates>
    </xsl:if>
  </xsl:template>
  <!-- -->
  <!--BJR SITE TREE BY REF-->
  <xsl:template match="select1[@class='siteTreeRef']" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <xsl:variable name="selectedValue">
      <xsl:value-of select="value/node()"/>
    </xsl:variable>
    <select name="{$ref}" id="{$ref}" class="dropdown form-control">
      <xsl:if test="@class!=''">
        <xsl:attribute name="class">
          <xsl:value-of select="@class"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@onChange!=''">
        <xsl:attribute name="onChange">
          <xsl:value-of select="@onChange"/>
        </xsl:attribute>
      </xsl:if>
      <option value="">
        <xsl:variable name="label_low" select="translate(label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
        <xsl:text>Not Specified</xsl:text>
      </option>
      <xsl:apply-templates select="/Page/Menu/MenuItem" mode="listPageOptionRef">
        <xsl:with-param name="level" select="number(1)"/>
        <xsl:with-param name="selectedValue" select="$selectedValue"/>
      </xsl:apply-templates>
      <!--<xsl:apply-templates select="item" mode="xform_select"/>-->
    </select>
  </xsl:template>

  <xsl:template match="MenuItem" mode="listPageOptionRef">
    <xsl:param name="level"/>
    <xsl:param name="selectedValue"/>
    <xsl:variable name="refVal">
      <xsl:choose>
        <xsl:when test="ref/node()!=''">
          <xsl:value-of select="ref/node()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>-None</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <option>
      <xsl:attribute name="value">
        <xsl:value-of select="$refVal"/>
      </xsl:attribute>
      <xsl:if test="$selectedValue=$refVal">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:call-template name="writeSpacers">
        <xsl:with-param name="spacers" select="$level"/>
      </xsl:call-template>
      <xsl:text>|-&#160;</xsl:text>
      <xsl:value-of select="@name"/>
      <xsl:text> </xsl:text>
    </option>
    <xsl:if test="count(child::MenuItem)&gt;0">
      <xsl:apply-templates select="MenuItem" mode="listPageOptionRef">
        <xsl:with-param name="level" select="number($level+1)"/>
        <xsl:with-param name="selectedValue" select="$selectedValue"/>
      </xsl:apply-templates>
    </xsl:if>
  </xsl:template>
  <!-- -->
  <xsl:template match="MenuItem" mode="listPageOption">
    <xsl:param name="level"/>
    <xsl:param name="selectedValue"/>
    <option>
      <xsl:attribute name="value">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:if test="$selectedValue=@id">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:call-template name="writeSpacers">
        <xsl:with-param name="spacers" select="$level"/>
      </xsl:call-template>
      <xsl:text>|-&#160;</xsl:text>
      <xsl:value-of select="@name"/>
      <xsl:if test="@status='0'">
        <xsl:text> (hidden)</xsl:text>
      </xsl:if>
      <xsl:text> </xsl:text>
    </option>
    <xsl:if test="count(child::MenuItem)&gt;0">
      <xsl:apply-templates select="MenuItem" mode="listPageOption">
        <xsl:with-param name="level" select="number($level+1)"/>
        <xsl:with-param name="selectedValue" select="$selectedValue"/>
      </xsl:apply-templates>
    </xsl:if>
  </xsl:template>
  <!-- -->
  <xsl:template name="writeSpacers">
    <xsl:param name="spacers"/>
    <xsl:text>&#160;&#160;</xsl:text>
    <xsl:if test="number($spacers)&gt;1">
      <xsl:call-template name="writeSpacers">
        <xsl:with-param name="spacers" select="number($spacers)-1"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- ## Standard Select1 for Radio Buttons ########################################################### -->
  <xsl:template match="select1[@appearance='full']" mode="xform_control">

    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>

    <xsl:variable name="value">
      <xsl:value-of select="value/node()"/>
    </xsl:variable>

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

  </xsl:template>

  <!-- ## Select1 for Radio Buttons with Dependant options ############################################# -->
  <xsl:template match="select1[@appearance='full' and item[toggle]]" mode="xform_control">

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
    <div class="{@class}">
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
    
    <!-- Output Cases - that not empty -->
    <xsl:apply-templates select="following-sibling::switch[1]/case[node()]" mode="xform" >
      <xsl:with-param name="selectedCase" select="$selectedCase" />
      <xsl:with-param name="dependantClass" select="$dependantClass" />
    </xsl:apply-templates>

  </xsl:template>

  <!-- -->

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

    <div class="{@class}">
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
      <xsl:apply-templates select="." mode="editXformMenu"/>
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
        <xsl:apply-templates select="label/node()" mode="cleanXhtml"/>
        <xsl:text> </xsl:text>
      </label>

    </span>
    <!--<xsl:if test="contains($class,'multiline') and position()!=last()">
					<br/>
				</xsl:if>-->

  </xsl:template>

  <xsl:template match="item[/Page/@adminMode and contains(@class,'multiline')]" mode="xform_radiocheck">
    <div class="radio">
      <xsl:apply-templates select="." mode="xform_radiocheck2"/>
    </div>
  </xsl:template>

  <!-- Overload for Admin Mode -->
  <xsl:template match="item[/Page/@adminMode='true']" mode="xform_radiocheck">
    <xsl:param name="type"/>
    <xsl:param name="ref"/>
    <xsl:param name="selectedValue" />
    <!-- value if passed through -->
    <xsl:variable name="value" select="value"/>
    <xsl:variable name="val" select="value/node()"/>
    <!--<xsl:variable name="class" select="../@class"/>-->
    <xsl:variable name="class" select="ancestor::*[name()='select' or name()='select1' ]/@class"/>
    <xsl:apply-templates select="." mode="editXformMenu">
      <xsl:with-param name="pos" select="position()"/>
    </xsl:apply-templates>
    <label for="{$ref}_{$value}">
      <xsl:if test="not(contains($class,'multiline'))">
        <xsl:attribute name="class">
          <xsl:text> radio-inline</xsl:text>
        </xsl:attribute>
      </xsl:if>
      <input type="{$type}">
        <xsl:choose>
          <xsl:when test="contains(../@class,'alwayson')">
            <xsl:attribute name="name">disabled</xsl:attribute>
            <xsl:attribute name="disabled">disabled</xsl:attribute>
            <xsl:attribute name="checked">checked</xsl:attribute>
          </xsl:when>
          <xsl:otherwise>
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
            <xsl:if test="../value/node()=$value">
              <xsl:attribute name="checked">checked</xsl:attribute>
            </xsl:if>
            <!-- Check checkbox should be selected -->
            <xsl:if test="contains($class,'checkboxes') or (contains(ancestor::select/@appearance,'full'))">
              <!-- Run through CSL to see if this should be checked -->
              <xsl:variable name="valueMatch">
                <xsl:call-template name="checkValueMatch">
                  <xsl:with-param name="CSLValue" select="ancestor::*[name()='select' or name()='select1' ]/value/node()"/>
                  <xsl:with-param name="value" select="$value"/>
                  <xsl:with-param name="seperator" select="','"/>
                </xsl:call-template>
              </xsl:variable>
              <xsl:if test="$valueMatch='true'">
                <xsl:attribute name="checked">checked</xsl:attribute>
              </xsl:if>
            </xsl:if>
          </xsl:otherwise>
        </xsl:choose>
          <xsl:if test="ancestor::select1/item[1]/value/node() = $value">
            <xsl:attribute name="data-fv-notempty">
              <xsl:value-of select="ancestor::select1/@data-fv-notempty"/>
            </xsl:attribute>
            <xsl:attribute name="data-fv-notempty-message">
              <xsl:value-of select="ancestor::select1/@data-fv-notempty-message"/>
            </xsl:attribute>
          </xsl:if>
      </input>
      <xsl:copy-of select="label/node()"/>
      <!-- needed to stop self closing -->
      <xsl:text> </xsl:text>
    </label>
    <xsl:if test="/Page/@ewCmd='EditXForm'">
      <xsl:if test="ancestor::Content/model/instance/results/answers/answer[@ref=$ref]/score[value/node()=$val]">
        <span>
          <xsl:attribute name="class">
            <xsl:if test="ancestor::Content/model/instance/results/answers/answer[@ref=$ref]/score[value/node()=$val]">
              <xsl:text>correct</xsl:text>
            </xsl:if>
          </xsl:attribute>
          (<xsl:value-of select="ancestor::Content/model/instance/results/answers/answer[@ref=$ref]/score[value/node()=$val]/@weighting"/>)
        </span>
      </xsl:if>
    </xsl:if>
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
  <xsl:template match="upload[ancestor::Page[@cssFramework='bs3']]" mode="xform_control">
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
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="Content[descendant::upload[@class='MultiPowUpload']] | div[@class='xform' and descendant::upload[@class='MultiPowUpload']]" mode="xform">
    <div id="fileupload">
      <!--<form action="/ewcommon/tools/FileTransferHandler.ashx?storageRoot={descendant::input[@bind='fld']/value/node()}" method="post" enctype="multipart/form-data" class="ewXform">-->

      <xsl:apply-templates select="group | repeat | input | secret | select | select1 | range | textarea | upload | hint | help | alert | div" mode="xform"/>




      <div class="terminus">&#160;</div>
      <!--</form>-->
    </div>
  </xsl:template>

  <xsl:template match="group[descendant::upload[@class='MultiPowUpload']]" mode="xform">
    <xsl:param name="class"/>

    <fieldset>
      <xsl:if test="$class!='' or @class!='' ">
        <xsl:attribute name="class">
          <xsl:value-of select="$class"/>
          <xsl:if test="@class!=''">
            <xsl:text> </xsl:text>
            <xsl:value-of select="@class"/>
          </xsl:if>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="label">
        <xsl:apply-templates select="label[position()=1]" mode="legend"/>
      </xsl:if>
      <ol>
        <xsl:for-each select="input | secret | select | select1 | range | textarea | upload | group | repeat | hint | help | alert | div | repeat | relatedContent | label[position()!=1] | trigger | script">
          <xsl:choose>
            <xsl:when test="name()='group'">
              <li>
                <xsl:apply-templates select="." mode="xform"/>
              </li>
            </xsl:when>
            <xsl:otherwise>
              <xsl:apply-templates select="." mode="xform"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:for-each>
      </ol>
    </fieldset>
  </xsl:template>

  <xsl:template match="upload[@class='MultiPowUpload']" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <div id="uploadFiles">
      <xsl:choose>
        <xsl:when test="contains($browserVersion,'Firefox') or contains($browserVersion,'Chrome')">
          <div class="drophere">Drag and drop files here to upload them</div>
          <label class="label">Alternatively, pick files</label>
        </xsl:when>
        <xsl:when test="contains($browserVersion,'MSIE') or contains($browserVersion,'')">
          <div class="hint">Note: You can upload multiple files without needing to refresh the page</div>
          <label class="label">Pick file</label>
        </xsl:when>
      </xsl:choose>

      <!--input type="hidden" name="path" /-->
      <!-- The fileinput-button span is used to style the file input field as button -->
      <span class="btn btn-success fileinput-button">
        <i class="fa fa-plus fa-white">
          <xsl:text> </xsl:text>
        </i>
        <span>Select files...</span>
        <!-- The file input field used as target for the file upload widget -->
        <input id="fileupload" type="file" name="files[]" multiple=""/>
      </span>
      <span class="fileupload-loading">
        <xsl:text> </xsl:text>
      </span>
    </div>
    <div id="progress" class="progress progress-success progress-striped">
      <div class="bar">
        <xsl:text> </xsl:text>
      </div>
    </div>
    <!-- The table listing the files available for upload/download -->
    <div id="files">
      <xsl:text> </xsl:text>
    </div>
    <table role="presentation" class="table table-striped">
      <tbody class="files" data-toggle="modal-gallery" data-target="#modal-gallery">
        <xsl:text> </xsl:text>
      </tbody>
    </table>


    <script id="template-upload" type="text/x-jquery-tmpl">
      <tr class="template-upload{{if error}} ui-state-error{{/if}}">
        <td class="name">${name}</td>
        <td colspan="2">&#160;</td>
        <td class="size">${sizef}</td>
        {{if error}}
        <td class="error" colspan="2">
          !! Error:
          {{if error === 'maxFileSize'}}File is too big
          {{else error === 'minFileSize'}}File is too small
          {{else error === 'acceptFileTypes'}}Filetype not allowed
          {{else error === 'maxNumberOfFiles'}}Max number of files exceeded
          {{else}}${error}
          {{/if}}
        </td>
        {{else}}
        <td class="progress">
          <div></div>
        </td>
        <td class="start">
          <button>Start</button>
        </td>
        {{/if}}
      </tr>
    </script>
    <script id="template-download" type="text/x-jquery-tmpl">
      <tr class="template-download{{if error}} ui-state-error{{/if}}">
        {{if error}}
        <td></td>
        <td class="name">${name}</td>
        <td colspan="2">&#160;</td>
        <td class="size">${sizef}</td>
        <td class="error" colspan="2">
          !! Error:
          {{if error === 1}}File exceeds upload_max_filesize (php.ini directive)
          {{else error === 2}}File exceeds MAX_FILE_SIZE (HTML form directive)
          {{else error === 3}}File was only partially uploaded
          {{else error === 4}}No File was uploaded
          {{else error === 5}}Missing a temporary folder
          {{else error === 6}}Failed to write file to disk
          {{else error === 7}}File upload stopped by extension
          {{else error === 'maxFileSize'}}File is too big
          {{else error === 'minFileSize'}}File is too small
          {{else error === 'acceptFileTypes'}}Filetype not allowed
          {{else error === 'maxNumberOfFiles'}}Max number of files exceeded
          {{else error === 'uploadedBytes'}}Uploaded bytes exceed file size
          {{else error === 'emptyResult'}}Empty file upload result
          {{else}}${error}
          {{/if}}
        </td>
        {{else}}
        <td class="name">${name}</td>
        <td colspan="2">&#160;</td>
        <td class="size">${sizef}</td>
        {{/if}}
      </tr>
    </script>



    <!-- The Iframe Transport is required for browsers without support for XHR file uploads -->
    <script src="/ewcommon/js/jQuery/fileUploader/8.2.1/js/jquery.iframe-transport.js"></script>
    <!-- The basic File Upload plugin -->
    <script src="/ewcommon/js/jQuery/fileUploader/8.2.1/js/jquery.fileupload.js"></script>

    <script>

      $('#fileupload').fileupload({
      url: '/?ewCmd={$page/@ewCmd}&amp;ewCmd2=FileUpload&amp;storageRoot=<xsl:value-of select="parent::group/input[@bind='fld']/value/node()"/>',
      dataType: 'json',
      sequentialUploads: true,
      dropZone:$('#uploadFiles'),
      always: function (e, data) {
      $.each(data.files, function (index, file) {
      $('<p/>').text(file.name).appendTo('#files');
      });
      },
      progressall: function (e, data) {
      var progress = parseInt(data.loaded / data.total * 100, 10);
      $('#progress .bar').css(
      'width',
      progress + '%'
      );
      }
      });
    </script>

  </xsl:template>

  <!-- -->
  <!-- ========================== XFORM : LABEL ========================== -->
  <!-- -->
  <xsl:template match="label" mode="xform">
    <li>
      <h6>
        <xsl:value-of select="./node()"/>
        <!--<xsl:apply-templates select="./node()" mode="cleanXhtml"/>-->
      </h6>
    </li>
  </xsl:template>

  <xsl:template match="label">
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
          <xsl:if test="$bRequired='true' and ancestor::select1[@appearance='full' and value/node()!='']">
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
        <xsl:if test="$bRequired='true'">
          <xsl:text> </xsl:text>
          <span class="req">*</span>
        </xsl:if>
      </label>
    </xsl:if>
  </xsl:template>

  <!-- ##############################################-Nathan (New) RELATED CONTENT-############################## -->
  <xsl:template match="relatedContent" mode="xform">
    <xsl:param name="contentType" select="@type"/>
    <xsl:param name="relationType" select="@relationType"/>
    <xsl:param name="rcCount" select="count(ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType])"/>
    <xsl:param name="contentTypeLabel" select="label/node()"/>
    <xsl:param name="formName" select="ancestor::Content/model/submission/@id"/>
    <xsl:variable name="RelType">
      <xsl:choose>
        <xsl:when test="contains(@direction,'1way')">
          <xsl:text>1way</xsl:text>
        </xsl:when>
        <xsl:when test="contains(@direction,'2way')">
          <xsl:text>2way</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>2way</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!--This way we get the type of content we relate to dynamically-->
    <table class="AdminRelatedContent table">
      <tr>
        <th>
          <xsl:if test="$relationType!=''">
            <label>
            <xsl:value-of select="$relationType"/>
            </label>
          </xsl:if>
          <!--<small>-->
          <br/>
          <xsl:choose>
            <xsl:when test="contains(@direction,'1way')">
              <xsl:text> (1 Way Relationship)</xsl:text>
            </xsl:when>
            <xsl:when test="contains(@direction,'2way')">
              <xsl:text> (2 Way Relationship)</xsl:text>
            </xsl:when>
            <xsl:otherwise>
              <xsl:text> (2 Way Relationship)</xsl:text>
            </xsl:otherwise>
          </xsl:choose>
          <!--</small>-->
        </th>
        <th class="relatedOptionsButton">
          <xsl:choose>
          <xsl:when test="ancestor::Content/model/instance/ContentRelations[@copyRelations='true']">
           Copy the following relationships
          </xsl:when>
            <xsl:otherwise>
           <xsl:variable name="contentCount" select="count(ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType])"/>
          <!-- Limit Number of Related Content-->
            <xsl:if test="contains(@search,'pick')">
              <xsl:variable name="value" select="ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType and (@rtype=$relationType or not(@rtype) or  @rtype='')][1]/@id"/>
              <div class="input-group">
              <select name="Related-{$relationType}" id="Related_{$relationType}" class="form-control" multiple="multiple">
                <xsl:variable name="reationPickList">
                  <xsl:call-template name="getSelectOptionsFunction">
                    <xsl:with-param name="query">Content.<xsl:value-of select="$contentType"/></xsl:with-param>
                  </xsl:call-template>
                </xsl:variable>
                <option value="">None  </option>
                <xsl:apply-templates select="ms:node-set($reationPickList)/select1/*" mode="xform_select">
                  <xsl:with-param name="selectedValue" select="$value"/>
                </xsl:apply-templates>
              </select>
              <xsl:if test="contains(@search,'add')">
                <span class="input-group-btn">
                <button ref="repeat" type="button" name="RelateAdd_{$contentType}_{$RelType}_{$relationType}" value="Add New" class="btn btn-success btn-xs" onclick="disableButton(this);$('#{$formName}').submit();">
                  <i class="fa fa-plus fa-white">
                    <xsl:text> </xsl:text>
                  </i> Add
                </button>
                  </span>
              </xsl:if>
                </div>
            </xsl:if>
           <xsl:if test="not(@maxRelationNo) or @maxRelationNo='' or (@maxRelationNo &gt; $contentCount)">
            <xsl:if test="contains(@search,'find')">
              <button ref="repeat" type="button" name="RelateFind_{$contentType}_{$RelType}_{$relationType}" value="Find Existing {$contentType}" class="btn btn-info btn-xs" onclick="disableButton(this);$('#{$formName}').submit();" >
                <i class="fa fa-search fa-white">
                  <xsl:text> </xsl:text>
                </i> Find Existing <xsl:value-of select="$contentType"/>
              </button>
            </xsl:if>
            <xsl:if test="contains(@search,'add')">
              <button ref="repeat" type="button" name="RelateAdd_{$contentType}_{$RelType}_{$relationType}" value="Add New" class="btn btn-success btn-xs" onclick="disableButton(this);$('#{$formName}').submit();">
                <i class="fa fa-plus fa-white">
                  <xsl:text> </xsl:text>
                </i> Add New
              </button>
            </xsl:if>
          </xsl:if>
          </xsl:otherwise>
            </xsl:choose>
         
        </th>
      </tr>
      <xsl:if test="not(contains(@search,'pick'))">
        <xsl:apply-templates select="ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType and (@rtype=$relationType or not(@rtype) or  @rtype='')]" mode="relatedRow">
          <xsl:sort select="@displayorder" data-type="number" order="ascending"/>
          <xsl:with-param name="formName" select="$formName" />
          <xsl:with-param name="relationType" select="$relationType" />
          <xsl:with-param name="relationDirection" select="$RelType" />
        </xsl:apply-templates>
      </xsl:if>
    </table>
  </xsl:template>

  <xsl:template match="Content" mode="relatedRow">
    <xsl:param name="formName"/>
     <xsl:param name="relationType"/>
        <xsl:param name="relationDirection"/>
    <span class="advancedModeRow" onmouseover="this.className='rowOver'" onmouseout="this.className='advancedModeRow'">
      <tr>
        <td>
          <xsl:apply-templates select="." mode="relatedBrief"/>
        </td>
        <xsl:choose>
          <xsl:when test="parent::ContentRelations[@copyRelations='true']"><td class="relatedOptionsButton">
            <input type="checkbox" name="Relate_{$relationType}_{$relationDirection}" value="{@id}" checked="checked"><xsl:text> </xsl:text>Relate</input>
            </td>
          </xsl:when>
          <xsl:otherwise>
            <td class="relatedOptionsButton">
              <button type="button" name="RelateTop_{@id}" value=" " class="btn btn-arrow btn-primary btn-xs" onClick="disableButton(this);{$formName}.submit()">
                <i class="fa fa-arrow-up fa-white">
                  <xsl:text> </xsl:text>
                </i>
              </button>
              <button type="button" name="RelateUp_{@id}" value=" " class="btn btn-arrow btn-primary btn-xs"  onClick="disableButton(this);{$formName}.submit()">
                <i class="fa fa-chevron-up fa-white">
                  <xsl:text> </xsl:text>
                </i>
              </button>
              <button type="button" name="RelateDown_{@id}" value=" " class="btn btn-arrow btn-primary btn-xs"  onClick="disableButton(this);{$formName}.submit()">
                <i class="fa fa-chevron-down fa-white">
                  <xsl:text> </xsl:text>
                </i>
              </button>
              <button type="button" name="RelateBottom_{@id}" value=" " class="btn btn-arrow btn-primary btn-xs" onClick="disableButton(this);{$formName}.submit()">
                <i class="fa fa-arrow-down fa-white">
                  <xsl:text> </xsl:text>
                </i>
              </button>
              <button type="button" name="RelateEdit_{@id}" value="Edit" class="btn btn-xs btn-primary " onClick="disableButton(this);{$formName}.submit()">
                <i class="fa fa-edit fa-white">
                  <xsl:text> </xsl:text>
                </i>
                <xsl:text> </xsl:text>Edit
              </button>
              <button type="button" name="RelateRemove_{@id}" value="Delete Relation" class="btn  btn-xs btn-danger"  onClick="disableButton(this);{$formName}.submit()">
                <i class="fa fa-minus fa-white">
                  <xsl:text> </xsl:text>
                </i>
                <xsl:text> </xsl:text>Remove
              </button>
              <xsl:if test="@status='1'">
                <a href="?ewCmd=HideContent&amp;id={@id}" title="Click here to hide this item" class="btn btn-xs btn-primary">
                  <i class="fa fa-ban-circle fa-white">
                    <xsl:text> </xsl:text>
                  </i>
                  <xsl:text> </xsl:text>Hide
                </a>
              </xsl:if>
              <xsl:if test="@status='0'">
                <a href="?ewCmd=ShowContent&amp;id={@id}" title="Click here to show this item" class="btn btn-xs btn-success">
                  <i class="fa fa-ok-circle fa-white">
                    <xsl:text> </xsl:text>
                  </i>
                  <xsl:text> </xsl:text>Show
                </a>
                <a href="?ewCmd=DeleteContent&amp;id={@id}" title="Click here to delete this item" class="btn btn-xs btn-danger">
                  <i class="fa fa-remove-circle fa-white">
                    <xsl:text> </xsl:text>
                  </i>
                  <xsl:text> </xsl:text>Delete
                </a>
              </xsl:if>
            </td>
          </xsl:otherwise>
        </xsl:choose>
        
       
      </tr>
    </span>
  </xsl:template>


  <xsl:template match="Content" mode="relatedBrief">
    <xsl:apply-templates select="." mode="getDisplayName" />
  </xsl:template>

  <xsl:template match="Content[@type='Ticket']" mode="relatedBrief">
    <xsl:apply-templates select="." mode="getDisplayName" /> -
    <span class="date">
      <xsl:if test="StartDate/node()!=''">
        <xsl:call-template name="DisplayDate">
          <xsl:with-param name="date" select="StartDate/node()"/>
        </xsl:call-template>
      </xsl:if>
      <xsl:if test="EndDate/node()!=StartDate/node()">
        <xsl:text> to </xsl:text>
        <xsl:call-template name="DisplayDate">
          <xsl:with-param name="date" select="EndDate/node()"/>
        </xsl:call-template>
      </xsl:if>
      <xsl:text>&#160;</xsl:text>
      <xsl:if test="Times/@start!='' and Times/@start!=','">
        <span class="times">
          <xsl:value-of select="translate(Times/@start,',',':')"/>
          <xsl:if test="Times/@end!='' and Times/@end!=','">
            <xsl:text> - </xsl:text>
            <xsl:value-of select="translate(Times/@end,',',':')"/>
          </xsl:if>
        </span>
      </xsl:if>
    </span>
  </xsl:template>

  <xsl:template match="Content[@type='NewsArticle']" mode="relatedBrief">
    <xsl:apply-templates select="." mode="getDisplayName" />
    <br/>
    <!--<small>-->
    <xsl:call-template name="truncate-string">
      <xsl:with-param name="text" select="Strapline/node()"/>
      <xsl:with-param name="length" select="90"/>
    </xsl:call-template>
    <!--</small>-->
  </xsl:template>

  <xsl:template match="Content[@type='LibraryImage']" mode="relatedBrief">
    <xsl:apply-templates select="." mode="displayThumbnail">
      <xsl:with-param name="width" select="'50'"/>
      <xsl:with-param name="height" select="'50'"/>
    </xsl:apply-templates>
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
      <span class="alert alert-danger">
        <xsl:choose>
          <xsl:when test="alert/span[contains(@class,'msg-')]">
            <!-- Send to system translations templates -->
            <xsl:apply-templates select="alert/span" mode="term"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="alert/node()" mode="cleanXhtml"/>
          </xsl:otherwise>
        </xsl:choose>
      </span>
    </xsl:if>
  </xsl:template>

  <xsl:template match="input[not(contains(@class,'hidden')) and ancestor::Page[@cssFramework='bs3']]" mode="xform_legend">

  </xsl:template>

  <xsl:template match="upload[not(contains(@class,'hidden')) and ancestor::Page[@cssFramework='bs3']]" mode="xform_legend">

  </xsl:template>

  <xsl:template match="input[not(contains(@class,'hidden'))] | secret | select | select1 | range | textarea | upload" mode="infoButton">
    <img src="/ewcommon/images/layout/help.png" height="16" width="16" class="helpTip">
      <xsl:attribute name="title">
        <xsl:text>&lt;div class=&#34;tl&#34;&gt;</xsl:text>
        <xsl:text>&lt;div class=&#34;tr&#34;&gt;</xsl:text>
        <xsl:text>&lt;span class=&#34;title&#34;&gt;</xsl:text>
        <xsl:value-of select="label/node()"/>
        <xsl:text>&lt;/span&gt;</xsl:text>
        <xsl:text>&lt;/div&gt;</xsl:text>
        <xsl:text>&lt;/div&gt;</xsl:text>
        <xsl:text> - </xsl:text>
        <xsl:text>&lt;div class=&#34;content&#34;&gt;</xsl:text>
        <xsl:apply-templates select="help/node()" mode="dirtyXhtml"/>
        <xsl:text>&lt;/div&gt;</xsl:text>
        <xsl:text>&lt;div class=&#34;bl&#34;&gt;</xsl:text>
        <xsl:text>&lt;div class=&#34;br&#34;&gt;</xsl:text>
        <xsl:text>&#160;</xsl:text>
        <xsl:text>&lt;/div&gt;</xsl:text>
        <xsl:text>&lt;/div&gt;</xsl:text>
      </xsl:attribute>
    </img>
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


</xsl:stylesheet>

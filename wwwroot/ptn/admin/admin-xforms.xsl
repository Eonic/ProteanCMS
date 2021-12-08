<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                  xmlns:v-bind="http://example.com/xml/v-bind" xmlns:v-on="http://example.com/xml/v-on"
                  xmlns:v-for="http://example.com/xml/v-for" xmlns:v-slot="http://example.com/xml/v-slot"
                  xmlns:v-if="http://example.com/xml/v-if" xmlns:v-else="http://example.com/xml/v-else"
                  xmlns:v-model="http://example.com/xml/v-model">

  <xsl:template match="Content[ancestor::Page[@adminMode='true']] | div[@class='xform' and ancestor::Page[@adminMode='true']]" mode="xform">
    <form method="{model/submission/@method}" action="">
      <xsl:attribute name="class">
        <xsl:text>ewXform container-fluid</xsl:text>
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
        <xsl:when test="count(group) = 2 and group[2]/submit and count(group[2]/*[name()!='submit']) = 0">
          <xsl:for-each select="group[1]">
            <xsl:if test="label[position()=1]">
              <div class="">
                <h3 class="">
                  <xsl:copy-of select="label/node()"/>
                </h3>
              </div>
            </xsl:if>
            <div class="">
              <xsl:apply-templates select="." mode="xform"/>
              <!--xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | hint | help | alert | div" mode="xform"/-->
            </div>
          </xsl:for-each>
          <xsl:for-each select="group[2]">
            <xsl:if test="count(submit) &gt; 0">
              <div class="navbar-fixed-bottom">
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
            <div class="">
              <h3 class="">
                <xsl:copy-of select="label/node()"/>
              </h3>
            </div>
          </xsl:if>
          <div class="">
            <xsl:apply-templates select="group | repeat " mode="xform"/>
            <xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | hint | help | alert | div" mode="xform"/>
          </div>
          <xsl:if test="count(submit) &gt; 0">
            <div class="clearfix">
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
    <form method="{model/submission/@method}" action=""  novalidate="novalidate">
      <xsl:attribute name="class">
        <xsl:text>xform card card-default needs-validation</xsl:text>
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
        <div class="card-header">
          <h3 class="card-title">
            <xsl:apply-templates select="group/label" mode="legend"/>
          </h3>
        </div>
      </xsl:if>
      <xsl:for-each select="group">
        <div class="card-body">
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
          <div class="card-footer clearfix">
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

  <xsl:template match="group[(contains(@class,'2col') or contains(@class,'2Col')) and ancestor::Page[@adminMode='true']]" mode="xform">
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
    <div class="input-group form-margin" id="editImage_{$ref}">
      <a href="#" onclick="xfrmClearImage('{ancestor::Content/model/submission/@id}','{$ref}','{value/*/@class}');return false" title="edit an image from the image library" class="btn btn-info input-group-btn">
        <i class="fa fa-times">
          <xsl:text> </xsl:text>
        </i>
      </a>
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
          <!--<a href="#" onclick="OpenWindow_edit_{$ref}('');return false;" title="edit an image from the image library" class="btn btn-primary">-->
          <a class="btn btn-info input-group-btn editImage">
            <i class="fas fa-image">
              <xsl:text> </xsl:text>
            </i><xsl:text> </xsl:text>Edit
          </a>
        </xsl:when>
        <xsl:otherwise>
          <!--<a href="#" onclick="OpenWindow_pick_{$ref}();return false;" title="pick an image from the image library" class="btn btn-primary">-->
          <a data-toggle="modal" href="?contentType=popup&amp;ewCmd=ImageLib&amp;targetForm={ancestor::Content/model/submission/@id}&amp;targetField={$ref}&amp;targetClass={value/*/@class}&amp;fld={@targetFolder}" data-target="#modal-{$ref}" class="btn btn-info input-group-btn">
            <i class="fas fa-image">
              <xsl:text> </xsl:text>
            </i><xsl:text> </xsl:text>Pick
          </a>
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
      <xsl:choose>
        <xsl:when test="value!=''">
          <a href="#" onclick="xfrmClearImgFile('{ancestor::Content/model/submission/@id}','{$scriptRef}');return false" title="Clear the file path" class="btn btn-danger">
            <i class="fa fa-trash-o fa-white">
              <xsl:text> </xsl:text>
            </i><xsl:text> </xsl:text>Clear
          </a>

        </xsl:when>
        <xsl:otherwise>
          <a data-toggle="modal" href="?contentType=popup&amp;ewCmd=ImageLib&amp;ewCmd2=PathOnly&amp;targetForm={ancestor::Content/model/submission/@id}&amp;targetField={$scriptRef}&amp;targetClass={value/*/@class}" data-target="#modal-{$scriptRef}" class="btn btn-info input-group-btn">
            <i class="fas fa-image">
              <xsl:text> </xsl:text>
            </i><xsl:text> </xsl:text>Pick
          </a>
        </xsl:otherwise>
      </xsl:choose>
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
              <i class="fas fa-file">
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

  <xsl:template match="Content[@type='xform']" mode="tinyMCEtinyMCEinit">
    <!-- tinyMCE - Now handled by jquery in commonV4_2.js -->
  </xsl:template>


  <!-- TinyMCE configuration templates -->
  <xsl:template match="textarea" mode="tinymceGeneralOptions">
    <xsl:text>script_url: '/ptn/core/tinymce/tinymce.min.js',
			mode: "exact",
			theme: "silver",
			width: "auto",
			relative_urls: false,
			plugins: "table paste link image ewimage media visualchars searchreplace emoticons anchor lists advlist code visualblocks contextmenu fullscreen searchreplace wordcount",
			entity_enconding: "numeric",
      image_advtab: true,
      menubar: "edit insert view format table tools",
      toolbar: "insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image ewimage",
			convert_fonts_to_spans: true,
			gecko_spellcheck: true,
			theme_advanced_toolbar_location: "top",
			theme_advanced_toolbar_align: "left",
			paste_create_paragraphs: false,
      link_list: tinymcelinklist,
			paste_use_dialog: true,</xsl:text>
    <xsl:apply-templates select="." mode="tinymceStyles"/>
    <xsl:apply-templates select="." mode="tinymceContentCSS"/>
    <xsl:text>
			auto_cleanup_word: "true"</xsl:text>
  </xsl:template>

  <xsl:template match="textarea" mode="tinymcelinklist">

    <xsl:text>
     [</xsl:text>
    <xsl:apply-templates select="/Page/Menu/MenuItem" mode="tinymcelinklistitem">
      <xsl:with-param name="level" select="number(1)"/>
    </xsl:apply-templates>
    <xsl:text>]</xsl:text>

  </xsl:template>

  <xsl:template match="MenuItem" mode="tinymcelinklistitem">
    <xsl:param name="level"/>

    <xsl:text>{title: '</xsl:text>
    <xsl:call-template name="writeSpacers">
      <xsl:with-param name="spacers" select="$level"/>
    </xsl:call-template>
    <xsl:text>&#160;</xsl:text>
    <xsl:call-template name="escape-js">
      <xsl:with-param name="string">
        <xsl:value-of select="@name"/>
      </xsl:with-param>
    </xsl:call-template>
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


  <xsl:template match="textarea[ancestor::Page[Settings/add[@key='theme.BespokeTextClasses']/@value!='']]" mode="tinymceStyles">
    <xsl:variable name="styles">
      <styles>
        <xsl:call-template name="split-string">
          <xsl:with-param name="list" select="/Page/Settings/add[@key='theme.BespokeTextClasses']/@value" />
          <xsl:with-param name="seperator" select="','" />
        </xsl:call-template>
      </styles>
    </xsl:variable>
    style_formats: [
    {title: 'Headers', items: [
    {title: 'Header 1', format: 'h1'},
    {title: 'Header 2', format: 'h2'},
    {title: 'Header 3', format: 'h3'},
    {title: 'Header 4', format: 'h4'},
    {title: 'Header 5', format: 'h5'},
    {title: 'Header 6', format: 'h6'}
    ]},
    {title: 'Inline', items: [
    {title: 'Bold', icon: 'bold', format: 'bold'},
    {title: 'Italic', icon: 'italic', format: 'italic'},
    {title: 'Underline', icon: 'underline', format: 'underline'},
    {title: 'Strikethrough', icon: 'strikethrough', format: 'strikethrough'},
    {title: 'Superscript', icon: 'superscript', format: 'superscript'},
    {title: 'Subscript', icon: 'subscript', format: 'subscript'},
    {title: 'Code', icon: 'code', format: 'code'}
    ]},
    {title: 'Blocks', items: [
    {title: 'Paragraph', format: 'p'},
    {title: 'Blockquote', format: 'blockquote'},
    {title: 'Div', format: 'div'},
    {title: 'Pre', format: 'pre'}
    ]},
    {title: 'Alignment', items: [
    {title: 'Left', icon: 'alignleft', format: 'alignleft'},
    {title: 'Center', icon: 'aligncenter', format: 'aligncenter'},
    {title: 'Right', icon: 'alignright', format: 'alignright'},
    {title: 'Justify', icon: 'alignjustify', format: 'alignjustify'}
    ]},
    <xsl:for-each select="ms:node-set($styles)/*/*">
      {title: '<xsl:value-of select="node()"/>', inline:'span', classes: '<xsl:value-of select="node()"/>'},
    </xsl:for-each>
    ],
  </xsl:template>

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
    "a[href|target|title|style|class|onmouseover|onmouseout|onclick|id|name],"
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
      <!--xsl:apply-templates select="value/TemplateContent/node()" mode="cleanXhtml"/-->
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

  <!-- ACE Control -->
  <xsl:template match="textarea[contains(@class,'xsl')]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <textarea name="{$ref}" id="{$ref}" class="aceEditor">
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
      <xsl:copy-of select="value/node()"/>
      <xsl:text> </xsl:text>
    </textarea>
    <style type="text/css" media="screen">
      .aceEditor, .ace_editor {
      width: 100%;
      height:600px;
      }
    </style>
    <script src="/ewcommon/js/ace/ace.js" type="text/javascript" charset="utf-8">&#160;</script>
    <script src="/ewcommon/js/xmlbeautify/xmlbeautify.js" type="text/javascript" charset="utf-8">&#160;</script>
    <script>
      alert($('#<xsl:value-of select="$ref"/>').text());
      var xsl2edit = unescapeHTML($('#<xsl:value-of select="$ref"/>').text())
      xsl2edit = new XmlBeautify().beautify(xsl2edit,{indent: "  ",useSelfClosingElement: true})
      xsl2edit = xsl2edit.replace(/xsl-/g,"xsl:");
      $('#<xsl:value-of select="$ref"/>').text(xsl2edit);
      var editor = ace.edit('<xsl:value-of select="$ref"/>');
      editor.setTheme("ace/theme/xcode");
      editor.session.setMode("ace/mode/xml");
      editor.session.setUseWrapMode(true);

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

  <!--Uploader-->



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
      <xsl:text>
      $('#fileupload').fileupload({
      url: '/?ewCmd=</xsl:text><xsl:value-of select="$page/@ewCmd"/><xsl:text>&amp;ewCmd2=FileUpload&amp;storageRoot=</xsl:text><xsl:value-of select="parent::group/input[@bind='fld']/value/node()"/>',
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

  <!-- In Admin - Allow for default SITE box styles -->
  <xsl:template match="select1[@appearance='minimal' and contains(@class,'boxStyle')][ancestor::Page[@adminMode='true']]" mode="xform_control">
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

  <xsl:template match="select1[@appearance='minimal' and contains(@class,'boxStyle')][ancestor::Page[@adminMode='true' and (@ewCmd='AddMailModule' or @ewCmd='EditMailContent' )]]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <div class="bfh-selectbox boxStyle" data-name="{$ref}" data-value="{value/node()}">
      <xsl:apply-templates select="item" mode="xform_BoxStyles"/>
      <xsl:apply-templates select="." mode="mailBoxStyles">
        <xsl:with-param name="value" select="value/node()" />
      </xsl:apply-templates>
    </div>
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

  <xsl:template match="*" mode="siteBoxStyles">
    <xsl:param name="value" />
    <!-- EXAMPLE BESPOKE BOX-->
    <!--<div data-value="panel-primary">
      <div class="panel panel-bespoke">
        <div class="panel-heading">Bespoke Box Style</div>
        <div class="panel-body">
          <xsl:text> </xsl:text>
        </div>
      </div>
    </div>-->
  </xsl:template>

  <xsl:template match="*[ancestor::Page[Settings/add[@key='theme.BespokeBoxStyles']/@value!='']]" mode="siteBoxStyles">
    <xsl:param name="value" />
    <xsl:variable name="styles">
      <styles>
        <xsl:call-template name="split-string">
          <xsl:with-param name="list" select="/Page/Settings/add[@key='theme.BespokeBoxStyles']/@value" />
          <xsl:with-param name="seperator" select="','" />
        </xsl:call-template>
      </styles>
    </xsl:variable>

    <xsl:for-each select="ms:node-set($styles)/*/*">
      <!-- EXAMPLE BESPOKE BOX-->
      <div data-value="{node()}">
        <div class="panel {node()}">
          <div class="panel-heading">
            <xsl:value-of select="node()"/>
          </div>
          <div class="panel-body">
            Example Text
          </div>
        </div>
      </div>
    </xsl:for-each>

  </xsl:template>

  <!-- -->
  <xsl:template match="*" mode="bootstrapBoxStyles">
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
  <xsl:template match="/" mode="mailBoxStyles">
    <xsl:param name="value" />

    <option value="bespokeBox">
      <xsl:if test="$value='bespokeBox'">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:text>site's bespoke box</xsl:text>
    </option>

  </xsl:template>

  <xsl:template match="*" mode="mailBoxStyles">
    <xsl:param name="value" />

    <option value="bespokeBox">
      <xsl:if test="$value='bespokeBox'">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:text>site's bespoke box</xsl:text>
    </option>

  </xsl:template>



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

  <xsl:template match="getFilterButtons" mode="xform">
    <xsl:variable name="filterButtons">
      <xsl:apply-templates select="." mode="getFilterButtons"/>
      <!--
      <buttons>
        <button>pageFilter<button>
        <button>dateFilter<button>
      <buttons>
      -->
    </xsl:variable>
    <div>
      <xsl:for-each select="ms:node($filterButtons)/button">
        <xsl:variable name="buttonName" select="node()"/>
        <xsl:choose>
          <xsl:when test="ancestor::Content/Content[@filterType=$buttonName]">
            <!-- edit button and show filter details -->
          </xsl:when>
          <xsl:otherwise>
            <!-- add button -->
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </div>
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

    <label for="Related_{$relationType}">
      <xsl:choose>
        <xsl:when test="label/node()!=''">
          <xsl:value-of select="label/node()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:if test="$relationType!=''">
            <xsl:value-of select="$relationType"/>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>

      <!--<small>-->

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
      <xsl:value-of select="$relationType"/>
      <!--</small>-->
    </label>
    <xsl:choose>
      <xsl:when test="ancestor::Content/model/instance/ContentRelations[@copyRelations='true']">
        Copy the following relationships
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="contentCount" select="count(ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType])"/>
        <!-- Limit Number of Related Content-->
        <xsl:if test="contains(@search,'pick')">
          <xsl:variable name="valueList">
            <list>
              <xsl:for-each select="ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType and (@rtype=$relationType or not(@rtype) or  @rtype='')]">
                <item>
                  <xsl:value-of select="@id"/>
                </item>
              </xsl:for-each>
            </list>
          </xsl:variable>
          <xsl:variable name="value" select="ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType and (@rtype=$relationType or not(@rtype) or  @rtype='')][1]/@id"/>
          <select name="Related-{$relationType}" id="Related_{$relationType}" class="form-control">
            <xsl:if test="@maxRelationNo &gt; 1">
              <xsl:attribute name="multiple">multiple</xsl:attribute>
            </xsl:if>
            <xsl:if test="@size &gt; 1">
              <xsl:attribute name="size">
                <xsl:value-of select="@size"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:variable name="reationPickList">
              <xsl:call-template name="getSelectOptionsFunction">
                <xsl:with-param name="query">
                  <xsl:text>Content.</xsl:text>
                  <xsl:value-of select="$contentType"/>
                </xsl:with-param>
              </xsl:call-template>
            </xsl:variable>
            <option value="">None  </option>
            <xsl:apply-templates select="ms:node-set($reationPickList)/select1/*" mode="xform_select_multi">
              <xsl:with-param name="selectedValues" select="$valueList"/>
            </xsl:apply-templates>
          </select>
          <xsl:if test="@maxRelationNo &gt; 1">
            <div class="alert alert-info">
              <i class="fa fa-info">&#160;</i> Press CTRL and click to select more than one option
            </div>
          </xsl:if>
          <xsl:if test="contains(@search,'add')">
            <span class="input-group-btn pull-right">
              <button ref="repeat" type="button" name="RelateAdd_{$contentType}_{$RelType}_{$relationType}" value="Add New" class="btn btn-success btn-xs" onclick="disableButton(this);$('#{$formName}').submit();">
                <i class="fa fa-plus fa-white">
                  <xsl:text> </xsl:text>
                </i> Add
              </button>
            </span>
          </xsl:if>
        </xsl:if>
        <xsl:if test="not(@maxRelationNo) or @maxRelationNo='' or (@maxRelationNo &gt; $contentCount)">
          <xsl:if test="contains(@search,'find')">
            <button ref="repeat" type="button" name="RelateFind_{$contentType}_{$RelType}_{$relationType}" value="Find Existing {$contentType}" class="btn btn-info btn-xs pull-right" onclick="disableButton(this);$('#{$formName}').submit();" >
              <i class="fa fa-search fa-white">
                <xsl:text> </xsl:text>
              </i> Find Existing <xsl:value-of select="$contentType"/>
            </button>
          </xsl:if>
          <xsl:if test="contains(@search,'add')">
            <button ref="repeat" type="button" name="RelateAdd_{$contentType}_{$RelType}_{$relationType}" value="Add New" class="btn btn-success btn-xs pull-right" onclick="disableButton(this);$('#{$formName}').submit();">
              <i class="fa fa-plus fa-white">
                <xsl:text> </xsl:text>
              </i> Add New
            </button>
          </xsl:if>
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>

    <xsl:if test="not(contains(@search,'pick'))">

      <xsl:apply-templates select="ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType and (@rtype=$relationType or not(@rtype) or  @rtype='')]" mode="relatedRow">
        <xsl:sort select="@status" data-type="number" order="descending"/>
        <xsl:sort select="@displayorder" data-type="number" order="ascending"/>
        <xsl:with-param name="formName" select="$formName" />
        <xsl:with-param name="relationType" select="$relationType" />
        <xsl:with-param name="relationDirection" select="$RelType" />
      </xsl:apply-templates>

    </xsl:if>
  </xsl:template>

  <xsl:template match="relatedContent[@type='filter']" mode="xform">
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

    <xsl:value-of select="$relationType"/>
    <label for="Related_{$relationType}">
      <xsl:choose>
        <xsl:when test="label/node()!=''">
          <xsl:value-of select="label/node()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:if test="$relationType!=''">
            <xsl:value-of select="$relationType"/>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>

      <!--<small>-->

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
      <xsl:value-of select="$relationType"/>
      <!--</small>-->
    </label>
    <xsl:choose>
      <xsl:when test="ancestor::Content/model/instance/ContentRelations[@copyRelations='true']">
        Copy the following relationships
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="contentCount" select="count(ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType])"/>
        <!-- Limit Number of Related Content-->
        <xsl:if test="contains(@search,'pick')">
          <xsl:variable name="valueList">
            <list>
              <xsl:for-each select="ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType and (@rtype=$relationType or not(@rtype) or  @rtype='')]">
                <item>
                  <xsl:value-of select="@id"/>
                </item>
              </xsl:for-each>
            </list>
          </xsl:variable>
          <xsl:variable name="value" select="ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType and (@rtype=$relationType or not(@rtype) or  @rtype='')][1]/@id"/>
          <select name="Related-{$relationType}" id="Related_{$relationType}" class="form-control">
            <xsl:if test="@maxRelationNo &gt; 1">
              <xsl:attribute name="multiple">multiple</xsl:attribute>
            </xsl:if>
            <xsl:if test="@size &gt; 1">
              <xsl:attribute name="size">
                <xsl:value-of select="@size"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:variable name="reationPickList">
              <xsl:call-template name="getSelectOptionsFunction">
                <xsl:with-param name="query">
                  <xsl:text>Content.</xsl:text>
                  <xsl:value-of select="$contentType"/>
                </xsl:with-param>
              </xsl:call-template>
            </xsl:variable>
            <option value="">None  </option>
            <xsl:apply-templates select="ms:node-set($reationPickList)/select1/*" mode="xform_select_multi">
              <xsl:with-param name="selectedValues" select="$valueList"/>
            </xsl:apply-templates>
          </select>
          <xsl:if test="@maxRelationNo &gt; 1">
            <div class="alert alert-info">
              <i class="fa fa-info">&#160;</i> Press CTRL and click to select more than one option
            </div>
          </xsl:if>
          <xsl:if test="contains(@search,'add')">
            <span class="input-group-btn pull-right">
              <button ref="repeat" type="button" name="RelateAdd_{$contentType}_{$RelType}_{$relationType}" value="Add New" class="btn btn-success btn-xs" onclick="disableButton(this);$('#{$formName}').submit();">
                <i class="fa fa-plus fa-white">
                  <xsl:text> </xsl:text>
                </i> Add
              </button>
            </span>
          </xsl:if>
        </xsl:if>
        <xsl:if test="not(@maxRelationNo) or @maxRelationNo='' or (@maxRelationNo &gt; $contentCount)">
          <xsl:if test="contains(@search,'find')">
            <button ref="repeat" type="button" name="RelateFind_{$contentType}_{$RelType}_{$relationType}" value="Find Existing {$contentType}" class="btn btn-info btn-xs pull-right" onclick="disableButton(this);$('#{$formName}').submit();" >
              <i class="fa fa-search fa-white">
                <xsl:text> </xsl:text>
              </i> Find Existing <xsl:value-of select="$contentType"/>
            </button>
          </xsl:if>
          <xsl:if test="contains(@search,'add')">
            <button ref="repeat" type="button" name="RelateAdd_{$contentType}_{$RelType}_{$relationType}" value="Add New" class="btn btn-success btn-xs pull-right" onclick="disableButton(this);$('#{$formName}').submit();">
              <i class="fa fa-plus fa-white">
                <xsl:text> </xsl:text>
              </i> Add New
            </button>
          </xsl:if>
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>

    <xsl:if test="not(contains(@search,'pick'))">

      <xsl:apply-templates select="ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType and (@rtype=$relationType or not(@rtype) or  @rtype='')]" mode="relatedRow">
        <xsl:sort select="@status" data-type="number" order="descending"/>
        <xsl:sort select="@displayorder" data-type="number" order="ascending"/>
        <xsl:with-param name="formName" select="$formName" />
        <xsl:with-param name="relationType" select="$relationType" />
        <xsl:with-param name="relationDirection" select="$RelType" />
      </xsl:apply-templates>

    </xsl:if>
  </xsl:template>

  <xsl:template match="relatedContent[@type='filter']" mode="xform">
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

    <xsl:value-of select="$relationType"/>
    <label for="Related_{$relationType}">
      <xsl:choose>
        <xsl:when test="label/node()!=''">
          <xsl:value-of select="label/node()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:if test="$relationType!=''">
            <xsl:value-of select="$relationType"/>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>

      <!--<small>-->

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
      <xsl:value-of select="$relationType"/>
      <!--</small>-->
    </label>
    <xsl:choose>
      <xsl:when test="ancestor::Content/model/instance/ContentRelations[@copyRelations='true']">
        Copy the following relationships
      </xsl:when>
      <xsl:otherwise>
        <xsl:variable name="contentCount" select="count(ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType])"/>
        <!-- Limit Number of Related Content-->
        <xsl:if test="contains(@search,'pick')">
          <xsl:variable name="valueList">
            <list>
              <xsl:for-each select="ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType and (@rtype=$relationType or not(@rtype) or  @rtype='')]">
                <item>
                  <xsl:value-of select="@id"/>
                </item>
              </xsl:for-each>
            </list>
          </xsl:variable>
          <xsl:variable name="value" select="ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType and (@rtype=$relationType or not(@rtype) or  @rtype='')][1]/@id"/>
          <select name="Related-{$relationType}" id="Related_{$relationType}" class="form-control">
            <xsl:if test="@maxRelationNo &gt; 1">
              <xsl:attribute name="multiple">multiple</xsl:attribute>
            </xsl:if>
            <xsl:if test="@size &gt; 1">
              <xsl:attribute name="size">
                <xsl:value-of select="@size"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:variable name="reationPickList">
              <xsl:call-template name="getSelectOptionsFunction">
                <xsl:with-param name="query">
                  <xsl:text>Content.</xsl:text>
                  <xsl:value-of select="$contentType"/>
                </xsl:with-param>
              </xsl:call-template>
            </xsl:variable>
            <option value="">None  </option>
            <xsl:apply-templates select="ms:node-set($reationPickList)/select1/*" mode="xform_select_multi">
              <xsl:with-param name="selectedValues" select="$valueList"/>
            </xsl:apply-templates>
          </select>
          <xsl:if test="@maxRelationNo &gt; 1">
            <div class="alert alert-info">
              <i class="fa fa-info">&#160;</i> Press CTRL and click to select more than one option
            </div>
          </xsl:if>
          <xsl:if test="contains(@search,'add')">
            <span class="input-group-btn pull-right">
              <button ref="repeat" type="button" name="RelateAdd_{$contentType}_{$RelType}_{$relationType}" value="Add New" class="btn btn-success btn-xs" onclick="disableButton(this);$('#{$formName}').submit();">
                <i class="fa fa-plus fa-white">
                  <xsl:text> </xsl:text>
                </i> Add
              </button>
            </span>
          </xsl:if>
        </xsl:if>
        <xsl:if test="not(@maxRelationNo) or @maxRelationNo='' or (@maxRelationNo &gt; $contentCount)">
          <xsl:if test="contains(@search,'find')">
            <button ref="repeat" type="button" name="RelateFind_{$contentType}_{$RelType}_{$relationType}" value="Find Existing {$contentType}" class="btn btn-info btn-xs pull-right" onclick="disableButton(this);$('#{$formName}').submit();" >
              <i class="fa fa-search fa-white">
                <xsl:text> </xsl:text>
              </i> Find Existing <xsl:value-of select="$contentType"/>
            </button>
          </xsl:if>
          <xsl:if test="contains(@search,'add')">
            <button ref="repeat" type="button" name="RelateAdd_{$contentType}_{$RelType}_{$relationType}" value="Add New" class="btn btn-success btn-xs pull-right" onclick="disableButton(this);$('#{$formName}').submit();">
              <i class="fa fa-plus fa-white">
                <xsl:text> </xsl:text>
              </i> Add New
            </button>
          </xsl:if>
        </xsl:if>
      </xsl:otherwise>
    </xsl:choose>

    <xsl:if test="not(contains(@search,'pick'))">

      <xsl:apply-templates select="ancestor::Content/model/instance/ContentRelations/Content[@type=$contentType and (@rtype=$relationType or not(@rtype) or  @rtype='')]" mode="relatedRow">
        <xsl:sort select="@status" data-type="number" order="descending"/>
        <xsl:sort select="@displayorder" data-type="number" order="ascending"/>
        <xsl:with-param name="formName" select="$formName" />
        <xsl:with-param name="relationType" select="$relationType" />
        <xsl:with-param name="relationDirection" select="$RelType" />
      </xsl:apply-templates>

    </xsl:if>
  </xsl:template>

  <xsl:template match="Content" mode="relatedRow">
    <xsl:param name="formName"/>
    <xsl:param name="relationType"/>
    <xsl:param name="relationDirection"/>
    <div class="advancedModeRow row" onmouseover="this.className='rowOver row'" onmouseout="this.className='advancedModeRow row'">
      <div class="col-md-7">
        <xsl:apply-templates select="." mode="status_legend"/>
        <xsl:text> </xsl:text>
        <xsl:apply-templates select="." mode="relatedBrief"/>
      </div>
      <xsl:choose>
        <xsl:when test="parent::ContentRelations[@copyRelations='true']">
          <div class="col-md-6">
            <input type="checkbox" name="Relate_{$relationType}_{$relationDirection}" value="{@id}" checked="checked">
              <xsl:text> </xsl:text>Relate
            </input>
          </div>
        </xsl:when>
        <xsl:otherwise>
          <div class="col-md-5 buttons">
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
                <i class="fa fa fa-eye-slash fa-white">
                  <xsl:text> </xsl:text>
                </i>
              </a>
            </xsl:if>
            <xsl:if test="@status='0'">
              <a href="?ewCmd=ShowContent&amp;id={@id}" title="Click here to show this item" class="btn btn-xs btn-success">
                <i class="fa fa-eye fa-white">
                  <xsl:text> </xsl:text>
                </i>
              </a>
              <a href="?ewCmd=DeleteContent&amp;id={@id}" title="Click here to delete this item" class="btn btn-xs btn-danger">
                <i class="fa fa-trash-o fa-white">
                  <xsl:text> </xsl:text>
                </i>
              </a>
            </xsl:if>
          </div>
        </xsl:otherwise>
      </xsl:choose>


    </div>
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



  <!-- -->
  <xsl:template match="select1[@appearance='full' and @class='PickByImage'][ancestor::Page[@adminMode='true']]" mode="xform_control">
    <xsl:variable name="ref">
      <xsl:apply-templates select="." mode="getRefOrBind"/>
    </xsl:variable>
    <!--<xsl:attribute name="class">pickByImage</xsl:attribute>-->
    <!--<input type="hidden" name="{$ref}" value="{value/node()}"/>-->
    <div class="accordion" id="pick-by-image">
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

    <div class="accordion-item">
      <h5 class="accordion-header" id="heading{$makeClass}">
      <button class="accordion-button" type="button" data-bs-toggle="collapse" data-bs-target="#collapse{$makeClass}" aria-expanded="true" aria-controls="collapse{$makeClass}"> 
         <xsl:apply-templates select="label" mode="xform_legend"/>
      </button>
      </h5>
      <div id="collapse{$makeClass}"  aria-labelledby="heading{$makeClass}" data-bs-parent="#pick-by-image">
        <xsl:attribute name="class">
          <xsl:text>accordion-collapse collapse row </xsl:text>
          <xsl:if test="position()=1">
            <xsl:text> in</xsl:text>
          </xsl:if>
        </xsl:attribute>
          <xsl:apply-templates select="item" mode="xform_imageClick">
            <xsl:with-param name="ref" select="$ref"/>
          </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="item" mode="xform_imageClick">
    <xsl:param name="ref"/>
    <xsl:variable name="value" select="value/node()"/>
    <xsl:variable name="selectedValue" select="ancestor::select1/value/node()"/>
    <xsl:variable name="isSelected">
      <xsl:if test="$value=$selectedValue">
        <xsl:text>selected</xsl:text>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="ifExists">
      <xsl:call-template name="virtual-file-exists">
        <xsl:with-param name="path" select="translate(img/@src,' ','-')"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="imageURL">
      <!--<xsl:choose>
        <xsl:when test="$ifExists='1'">
          <xsl:value-of select="translate(img/@src,' ','-')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>/ewcommon/images/pagelayouts/tempImage.gif</xsl:text>
        </xsl:otherwise>
      </xsl:choose>-->
      <xsl:choose>
        <xsl:when test="value='FormattedText'">
          <xsl:text>fas fa-align-left</xsl:text>
        </xsl:when>
        <xsl:when test="value='Image'">
          <xsl:text>far fa-image</xsl:text>
        </xsl:when>
        <xsl:when test="value='Video'">
          <xsl:text>fas fa-play</xsl:text>
        </xsl:when>
        <xsl:when test="value='EmailForm'">
          <xsl:text>fas fa-envelope</xsl:text>
        </xsl:when>
        <xsl:when test="value='ContactList'">
          <xsl:text>far fa-user</xsl:text>
        </xsl:when>
        <xsl:when test="value='DocumentList'">
          <xsl:text>far fa-file-alt</xsl:text>
        </xsl:when>
        <xsl:when test="value='EventList'">
          <xsl:text>far fa-calendar</xsl:text>
        </xsl:when>
        <xsl:when test="value='NewsList'">
          <xsl:text>fas fa-newspaper</xsl:text>
        </xsl:when>
        <xsl:when test="value='OrganisationList'">
          <xsl:text>far fa-building</xsl:text>
        </xsl:when>
        <xsl:when test="value='TrainingCourseList'">
          <xsl:text>fas fa-graduation-cap</xsl:text>
        </xsl:when>
        <xsl:when test="value='TestimonialList'">
          <xsl:text>far fa-comments</xsl:text>
        </xsl:when>
        <xsl:otherwise>
          <xsl:text>fas fa-puzzle-piece</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div class="col-md-2">
      <button name="{$ref}" value="{value/node()}" class="{$isSelected}">
        <!--<img src="{$imageURL}" class="card-img-top"/>-->
        <i class="fas fa-3x {$imageURL}"> </i>
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

  <!-- -->
  <xsl:template match="select1[@appearance='minimal' and contains(@class,'iconSelect')]" mode="xform_control">
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
      <i class="fa {value/node()} fa-lg">
        <xsl:text> </xsl:text>
      </i>
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

  <!-- -->
  <xsl:template match="select1[@appearance='minimal' and contains(@class,'bgStyle')]" mode="xform_control">
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

  <xsl:template match="*" mode="siteBGStyles">
    <xsl:param name="value" />
    <!-- EXAMPLE BESPOKE BOX-->
    <!--option value="testBG">
      <xsl:if test="$value='testBG'">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:text>testBG</xsl:text>
    </option-->
  </xsl:template>

  <xsl:template match="*[ancestor::Page[Settings/add[@key='theme.BespokeBackgrounds']/@value!='']]" mode="siteBGStyles">
    <xsl:param name="value" />
    <xsl:variable name="styles">
      <styles>
        <xsl:call-template name="split-string">
          <xsl:with-param name="list" select="/Page/Settings/add[@key='theme.BespokeBackgrounds']/@value" />
          <xsl:with-param name="seperator" select="','" />
        </xsl:call-template>
      </styles>
    </xsl:variable>

    <xsl:for-each select="ms:node-set($styles)/*/*">
      <!-- EXAMPLE BESPOKE BOX-->
      <option value="{node()}">
        <xsl:if test="$value=node()">
          <xsl:attribute name="selected">selected</xsl:attribute>
        </xsl:if>
        <xsl:value-of select="node()"/>
      </option>
    </xsl:for-each>

  </xsl:template>


  <!-- -->
  <xsl:template match="*" mode="bootstrapBGStyles">
    <xsl:param name="value" />
    <!-- THEIR ARE NO GENRIC BACKGROUNDS-->
  </xsl:template>

  <!-- -->
  <xsl:template match="select1[@appearance='minimal' and contains(@class,'cssStyle')]" mode="xform_control">
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

  <xsl:template match="*[ancestor::Page[Settings/add[@key='theme.BespokeTextClasses']/@value!='']]" mode="siteCssStyles">
    <xsl:param name="value" />
    <xsl:variable name="styles">
      <styles>
        <xsl:call-template name="split-string">
          <xsl:with-param name="list" select="/Page/Settings/add[@key='theme.BespokeTextClasses']/@value" />
          <xsl:with-param name="seperator" select="','" />
        </xsl:call-template>
      </styles>
    </xsl:variable>

    <xsl:for-each select="ms:node-set($styles)/*/*">
      <!-- EXAMPLE BESPOKE BOX-->
      <div data-value="{node()}">
        <div class="Site">
          <div class="tp-caption {node()}">
            <xsl:value-of select="node()"/>
          </div>
        </div>
      </div>
    </xsl:for-each>

  </xsl:template>

  <xsl:template match="item[not(toggle)]" mode="xform_radiocheck">
    <xsl:param name="type"/>
    <xsl:param name="ref"/>
    <xsl:param name="selectedValue" />
    <!-- value if passed through -->
    <xsl:variable name="value" select="value"/>
    <xsl:variable name="val" select="value/node()"/>
    <!--<xsl:variable name="class" select="../@class"/>-->
    <xsl:variable name="class" select="ancestor::*[name()='select' or name()='select1' ]/@class"/>
    <div class="form-check form-check-inline">
      <input type="{$type}" class="form-check-input">
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
      <label for="{$ref}_{$value}">
        <xsl:if test="not(contains($class,'multiline'))">
          <xsl:attribute name="class">
            <xsl:text> form-check-label</xsl:text>
          </xsl:attribute>
        </xsl:if>
        <xsl:apply-templates select="label" mode="xform-label"/>
        <!-- needed to stop self closing -->
        <xsl:text> </xsl:text>
      </label>
    </div>
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


  <xsl:template match="submit[contains(@class,'PermissionButton')]" mode="xform">
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

  <xsl:template match="item[/Page/@adminMode and contains(@class,'multiline')]" mode="xform_radiocheck">
    <div class="radio">
      <xsl:apply-templates select="." mode="xform_radiocheck2"/>
    </div>
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
        <xsl:text>checkbox checkbox-primary</xsl:text>
        <xsl:if test="contains($class,'multiline')">
          <xsl:text> multiline</xsl:text>
        </xsl:if>
      </xsl:attribute>

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
        <xsl:text>form-check form-check-inline</xsl:text>
        <xsl:if test="contains($class,'multiline')">
          <xsl:text> multiline</xsl:text>
        </xsl:if>
      </xsl:attribute>
      <input class="form-check-input" type="{$type}">
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
      <label for="{$ref}_{position()}" class="form-check-label {translate(value/node(),'/ ','')}">
        <xsl:value-of select="label/node()"/>
      </label>
    </span>
    <!--<xsl:if test="contains($class,'multiline') and position()!=last()">
      <br/>
    </xsl:if>-->

  </xsl:template>

  <xsl:template match="label[ancestor::select[contains(@class,'content')] and Content]" mode="xform-label">
    <xsl:value-of select="Content/@name"/>&#160;<small>
      [<xsl:value-of select="Content/@type"/>]
    </small>
  </xsl:template>


  <xsl:template match="group[@class='modal-confirm']" mode="xform_control_script">
    <script>
      <!-- if  @showonchange id changes then we show on form submit-->
    </script>
  </xsl:template>


  <xsl:template match="group[@class='modal-confirm']" mode="xform">
    <xsl:param name="class"/>
    <div id="modal-confirm" class="modal fade" tabindex="-1">
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

      <button submits="the entire form"/>

    </div>
  </xsl:template>

  <xsl:template match="group[@class='redirect-modal']" mode="xform">
    <xsl:param name="class"/>
    <div id="redirectModal" class="redirectModal modal fade" tabindex="-1">

      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <label>Do you want to create a redirect?</label>
            <button type="button" class="close" data-dismiss="modal" >
              <span aria-hidden="true">&#215;</span>
            </button>
          </div>
          <div class="modal-body">
            <div class="form-group repeat-group ">
              <fieldset class="rpt-00 row">
                <div class="form-group input-containing col-md-6">
                  <label>Old URL</label>
                  <div class="control-wrapper input-wrapper appearance-">

                    <input type="text" name="OldUrl" id="OldUrl" class="textbox form-control"/>
                  </div>
                </div>
                <div class="form-group input-containing col-md-6">
                  <label>New URL</label>
                  <div class="control-wrapper input-wrapper appearance-">
                    <input type="text" name="NewUrl" id="NewUrl" class="textbox form-control"/>
                  </div>
                </div>
              </fieldset>
            </div>
            <div>
              <button type="submit" name="redirectType"  value="301Redirect" class="btn btn-primary btnRedirectSave" onclick="return RedirectClick(this.value);">301 Permanant Redirect</button>
              <button type="submit" name="redirectType"  value="302Redirect" class="btn btn-primary btnRedirectSave"  onclick="return RedirectClick(this.value);">302 Temporary Redirect</button>
              <button type="submit" name="redirectType"  value="404Redirect" class="btn btn-primary btnRedirectSave"  onclick="return RedirectClick(this.value);">404 Page Not Found</button>
            </div>

            <xsl:if test="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url!=''">
              <xsl:variable name="objOldUrl" select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url" />
              <input name="pageOldUrl" type="hidden" value="{$objOldUrl}" class="hiddenOldUrl" />
            </xsl:if>
            <input name="productOldUrl" type="hidden" class="hiddenProductOldUrl" />
            <input name="productNewUrl" type="hidden" class="hiddenProductNewUrl" />
            <input name="IsParentPage" type="hidden" class="hiddenParentCheck" />
            <input name="pageId" type="hidden"  class="hiddenPageId" />
            <input name="type" type="hidden"  class="hiddenType" />
            <input  name="redirectOption" type="hidden" class="hiddenRedirectType" />
          </div>
        </div>
      </div>
    </div>

    <div id="RedirectionChildConfirmationModal" class="suitableForModal modal fade " tabindex="-1">

      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <button type="button" class="close" data-dismiss="modal" >
              <span aria-hidden="true">&#215;</span>
            </button>
          </div>
          <div class="modal-body">
            Current page have category/product pages beneath it, do you want to redirect them as well?
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-primary" id="btnNocreateRuleForChild" >Cancel</button>
            <button type="button" id="btnYescreateRuleForChild" class="btn btn-primary">Yes </button>
          </div>
        </div>
        <input name="productOldUrl" type="hidden" class="hiddenProductOldUrl" />
        <input name="productNewUrl" type="hidden" class="hiddenProductNewUrl" />
        <input name="IsParent" type="hidden" class="hiddenParentCheck" />
        <input name="pageId" type="hidden"  class="hiddenPageId" />
        <input  name="redirectOption" type="textbox" class="hiddenRedirectType" />
      </div>
    </div>
  </xsl:template>


  <xsl:template match="submit[contains(@class,'getGeocodeButton')]" mode="xform">
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
    <xsl:choose>
      <xsl:when test="$GoogleAPIKey!=''">
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
      </xsl:when>
      <xsl:otherwise>
        <div class="alert alert-warning">
          For geo-coding to work you require a Google API Key in the <a href="?ewCmd=WebSettings">config settings</a>
        </div>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>


</xsl:stylesheet>

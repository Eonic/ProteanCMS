<?xml version="1.0" encoding="utf-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">

	<xsl:import href="../core/functions.xsl"/>
	<xsl:import href="../core/xforms.xsl"/>
	<xsl:import href="../core/localisation.xsl"/>

  <xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/>

  <xsl:template match="/">
    <xsl:apply-imports/>
  </xsl:template>

	<xsl:variable name="isCSUser">
		<xsl:choose>
			<xsl:when test="$page/PreviewMenu/User/Role[@id='3093']">on</xsl:when>
			<xsl:otherwise>off</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

  <xsl:variable name="lCurly"><![CDATA[{]]></xsl:variable>
  <xsl:variable name="rCurly"><![CDATA[}]]></xsl:variable>
  
  <xsl:template match="Page">
    <div class="modal-dialog">
      <div class="modal-content">
        <div class="modal-header">
			<a type="button" data-bs-dismiss="modal" class="float-end">
				<i class="fa-regular fa-circle-xmark">
					<xsl:text> </xsl:text>
				</i>
			</a>
        </div>
        <div class="modal-body">
          <xsl:copy-of select="ContentDetail/Content"/>
			<xsl:text> </xsl:text>
        </div>
      </div>
    </div>
  </xsl:template>


  <xsl:template match="Page[ContentDetail/Content[@type='FAQ']]">
    <xsl:apply-templates select="ContentDetail/Content" mode="displayBrief"/>
  </xsl:template>

  <xsl:template match="Page[ContentDetail/Content[@type='Product']]">
    <div>
    <xsl:apply-templates select="ContentDetail/Content" mode="commentsSection"/>
		<xsl:text> </xsl:text>
    </div>
  </xsl:template>
	
  <xsl:template match="Page[ContentDetail/Content[@type='Review']]">
    <xsl:apply-templates select="ContentDetail/Content" mode="displayBrief"/>
  </xsl:template>

 


  
  <xsl:template match="Page[ContentDetail/Content[@type='xform']]">
    <div class="modal-dialog">
      <div class="modal-content">
        <div class="modal-header">
          <h4 class="modal-title" id="myModalLabel">
            <xsl:for-each select="ContentDetail/Content[@type='xform']/group/label">
              <xsl:if test="@icon!=''">
                <i class="{@icon}">&#160;</i>&#160;
              </xsl:if>
              <xsl:value-of select="node()"/>
            </xsl:for-each>
          </h4>
          <a type="button" data-bs-dismiss="modal" class="pull-right">
			  <i class="fa-regular fa-circle-xmark">&#160;</i>
		  </a>
        </div>
        <div class="modal-body">
          <xsl:apply-templates select="ContentDetail/Content" mode="xform"/>
          <xsl:apply-templates select="." mode="xform_control_scripts"/>
          <input type="hidden"  id="prodid"></input>			
        </div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content[@type='xform']" mode="displayBrief">
    <xsl:apply-templates select="Content[@type='xform']" mode="xform"/>
  </xsl:template>

  <xsl:template match="Content[@type='error']" mode="displayBrief">
    <xsl:apply-templates select="." mode="cleanXhtml"/>
  </xsl:template>

  <xsl:template match="Page" mode="xform_control_scripts">
    <script type="text/javascript">
		initialiseXforms();
	</script>
	  
      <xsl:apply-templates select="descendant-or-self::textarea[contains(@class,'xhtml')]" mode="xform_control_script"/>
      <xsl:apply-templates select="descendant-or-self::input[contains(@class,'calendar')]" mode="xform_control_script"/>
		<xsl:apply-templates select="descendant-or-self::input[contains(@class,'userUploadImage')]" mode="xform_control_script"/>
   
    <style>
      .datepicker {
      z-index: 1600 !important; /* has to be larger than 1050 */
      }
    </style>
  </xsl:template>


	
  <xsl:template match="input[contains(@class,'calendar')]" mode="xform_control_script">
	  <script type="text/javascript">
		  $( function() {

		  $( "#<xsl:apply-templates select="." mode="getRefOrBind"/>-alt" ).datepicker();
		  } );
	  </script>
  </xsl:template>

  <!-- TinyMCE configuration -->
  <xsl:template match="textarea" mode="xform_control_script">
	  <script type="text/javascript">
      if (typeof tinymce != 'undefined')
      {
         tinymce.remove('#<xsl:apply-templates select="." mode="getRefOrBind"/>');
      }
      $('#<xsl:apply-templates select="." mode="getRefOrBind"/>').tinymce({
      <xsl:apply-templates select="." mode="tinymceGeneralOptions"/>
		  });
		</script>
  </xsl:template>

  <!-- TinyMCE configuration templates -->
  <xsl:template match="textarea" mode="tinymceGeneralOptions">
    <xsl:variable name="control-height">
      <xsl:choose>
        <xsl:when test="@rows!=''">
          <xsl:value-of select="(@rows*30)+70"/>
        </xsl:when>
        <xsl:otherwise>400</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
	  
	  content_style: "p {text-align:center;font-family: Architects Daughter;}",
	  script_url: '/ewThemes/intotheblue2019/js/tinymce/tinymce.min.js',
	  theme : "silver",
	  menubar: false,
	  entity_encoding: "numeric",
	  mobile: {
	      menubar: false,
	      toolbar: 'fontselect fontsizeselect formatselect fontcolor | bold italic aligncenter alignleft alignright alignjustify emoticons forecolor',
	      toolbar_mode: 'sliding',
	      plugins: [
	      'advlist autolink lists link image charmap print preview anchor',
	      'searchreplace visualblocks code fullscreen',
	      'insertdatetime media table paste code help wordcount','emoticons','link'
	      ]
	  },
	  plugins: [
	  'advlist autolink lists link image charmap print preview anchor',
	  'searchreplace visualblocks code fullscreen',
	  'insertdatetime media table paste code help wordcount','emoticons','link'
	  ],
	  height: <xsl:value-of select="$control-height"/>,
			toolbar: 'fontselect fontsizeselect formatselect fontcolor | bold italic aligncenter alignleft alignright alignjustify|emoticons |forecolor|backcolor',
			toolbar_mode: 'sliding'
		

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

  <xsl:template match="textarea" mode="tinymceContentCSS"></xsl:template>


  <xsl:template match="group[parent::Content]" mode="xform">
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
      <xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | group | repeat | hint | help | alert | div | repeat | relatedContent | label[position()!=1] | trigger | script" mode="control-outer"/>
      <xsl:if test="count(submit) &gt; 0">
        <xsl:choose>
          <xsl:when test="contains(@class,'form-inline')">
            <xsl:apply-templates select="submit" mode="xform"/>
            <!-- Terminus needed for CHROME ! -->
            <!-- Terminus needed for BREAKS IE 7! -->
            <xsl:if test="$browserVersion!='MSIE 7.0'">
              <div class="terminus"><xsl:text> </xsl:text></div>
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
                <div class="terminus"><xsl:text> </xsl:text></div>
              </xsl:if>
				<xsl:text> </xsl:text>
            </div>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:if>
    </fieldset>
  </xsl:template>



</xsl:stylesheet>
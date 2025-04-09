<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
  <!-- ############################################ BOX STYLES ############################################### -->

  <xsl:template match="*" mode="siteBoxStyles">
    <xsl:param name="value" />
    <!-- EXAMPLE BESPOKE BOX-->

    <div data-value="heading-only" class="box-style-item">
      Heading Only
    </div>
  </xsl:template>

  <!-- ############################################ LAYOUT BG STYLES ############################################### -->

  <xsl:template match="*[ancestor::Page[@cssFramework='bs3' or @adminMode='true']]" mode="siteBGStyles">
    <xsl:param name="value" />
    <!-- EXAMPLE BESPOKE BOX-->
    <option value="bg-primary text-white">
      <xsl:if test="$value='bg-primary text-white'">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:text>Brand colour background</xsl:text>
    </option>
    <option value="bg-secondary text-white">
      <xsl:if test="$value='bg-secondary text-white'">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:text>Secondary colour background</xsl:text>
    </option>
    <option value="bg-info text-white">
      <xsl:if test="$value='bg-info text-white'">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:text>Info colour background</xsl:text>
    </option>
    <option value="bg-dark">
      <xsl:if test="$value='bg-dark'">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:text>Dark background</xsl:text>
    </option>
    <option value="whiteBG">
      <xsl:if test="$value='whiteBG'">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:text>White background</xsl:text>
    </option>
    <option value="videoBG">
      <xsl:if test="$value='videoBG'">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:text>Video background</xsl:text>
    </option>
    <option value="stretchBG">
      <xsl:if test="$value='stretchBG'">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:text>Stretch background</xsl:text>
    </option>
  </xsl:template>
  <!-- ############################################ HEADING BANNER STYLES ############################################### -->

  <xsl:template match="*" mode="bannerStyles1">
    <xsl:param name="value" />
    <option value="img-banner">
      <xsl:if test="$value='img-banner'">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:text>Image Banner</xsl:text>
    </option>
    <option value="basic-banner">
      <xsl:if test="$value='basic-banner'">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:text>Basic Banner</xsl:text>
    </option>
    <option value="no-banner">
      <xsl:if test="$value='no-banner'">
        <xsl:attribute name="selected">selected</xsl:attribute>
      </xsl:if>
      <xsl:text>No Banner</xsl:text>
    </option>
  </xsl:template>

  <!-- ############################################ MENU DD STYLES ############################################### -->
  <!--<xsl:template match="select1[@appearance='minimal' and contains(@class,'menuStyles')]" mode="control-outer">
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
								<xsl:text>form-margin </xsl:text>
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
						<xsl:if test="ancestor::group[contains(@class,'inline-2-col')] and not(name()='div')">
							<xsl:text> col-lg-6 2-col-inline</xsl:text>
						</xsl:if>
						<xsl:if test="ancestor::group[contains(@class,'inline-3-col')] and not(name()='div')">
							<xsl:text> col-lg-4 3-col-inline</xsl:text>
						</xsl:if>
					</xsl:attribute>
					<xsl:apply-templates select="." mode="xform"/>
				</div>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>

	<xsl:template match="select1[@appearance='minimal' and contains(@class,'menuStyles')]" mode="xform_control">
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
			<xsl:apply-templates select="." mode="menuStyles1">
				<xsl:with-param name="value" select="value/node()" />
			</xsl:apply-templates>
		</select>
	</xsl:template>

	<xsl:template match="*" mode="menuStyles1">
		<xsl:param name="value" />
		<option value="default">
			<xsl:if test="$value='default'">
				<xsl:attribute name="selected">selected</xsl:attribute>
			</xsl:if>
			<xsl:text>Default</xsl:text>
		</option>
		<option value="tiles">
			<xsl:if test="$value='tiles'">
				<xsl:attribute name="selected">selected</xsl:attribute>
			</xsl:if>
			<xsl:text>Tiles</xsl:text>
		</option>
	</xsl:template>-->

  <!-- ############################################ TinyMCE styles ############################################### -->
  <!-- Example Follows www.tinymce.com/tryit/custom_formats.php -->

  <xsl:template match="textarea" mode="tinymceStyles">
    style_formats: [
    {title: 'Headers', items: [
    {title: 'h1', block: 'h1'},
    {title: 'h2', block: 'h2'},
    {title: 'h3', block: 'h3'},
    {title: 'h4', block: 'h4'},
    {title: 'h5', block: 'h5'},
    {title: 'h6', block: 'h6'}
    ]},

    {title: 'Inline', items: [
    {title: 'Bold', inline: 'b', icon: 'bold'},
    {title: 'Italic', inline: 'i', icon: 'italic'},
    {title: 'Underline', inline: 'span', styles : {textDecoration : 'underline'}, icon: 'underline'},
    {title: 'Strikethrough', inline: 'span', styles : {textDecoration : 'line-through'}, icon: 'strikethrough'},
    {title: 'Superscript', inline: 'sup', icon: 'superscript'},
    {title: 'Subscript', inline: 'sub', icon: 'subscript'},
    {title: 'Code', inline: 'code', icon: 'code'},
    {title: 'Lead', inline: 'span', classes: 'lead'},
    {title: 'Small', inline: 'span', classes: 'small'},
    {title: 'Circle Stat', inline: 'span', classes: 'circle-stat'}
    ]},

    {title: 'Blocks', items: [
    {title: 'Paragraph', block: 'p'},
    {title: 'Blockquote', block: 'blockquote'},
    {title: 'Div', block: 'div'},
    {title: 'Pre', block: 'pre'},
    {title: 'Two Columns', block: 'div', classes: 'two-col-text', wrapper : true},
    {title: 'Three Columns', block: 'div', classes: 'three-col-text', wrapper : true}
    ]},

    {title: 'Alignment', items: [
    {title: 'Left', block: 'div', styles : {textAlign : 'left'}, icon: 'alignleft'},
    {title: 'Center', block: 'div', styles : {textAlign : 'center'}, icon: 'aligncenter'},
    {title: 'Right', block: 'div', styles : {textAlign : 'right'}, icon: 'alignright'},
    {title: 'Justify', block: 'div', styles : {textAlign : 'justify'}, icon: 'alignjustify'}
    ]},
    {title: 'Buttons', items: [
    {title: 'Button', inline: 'a', classes: 'btn btn-custom'},
    {title: 'Button Outline', inline: 'a', classes: 'btn btn-outline-primary'},
    {title: 'Button Outline Light', inline: 'a', classes: 'btn btn-outline-light'}
    ]},
    {title: 'Icons', items: [
    {title: 'Phone Icon', inline: 'span', classes: 'phone-icon'},
    {title: 'Email Icon', inline: 'span', classes: 'email-icon'}
    ]},
    ],
  </xsl:template>
</xsl:stylesheet>

<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
  <!-- ############################################ BOX STYLES ############################################### -->

  <xsl:template match="*" mode="siteBoxStyles">
    <xsl:param name="value" />
    <!-- EXAMPLE BESPOKE BOX-->
    <div data-value="panel-action">
      <div class="panel panel-action">
        <div class="panel-heading">
          <h6 class="panel-title">Panel Action</h6>
        </div>
        <div class="panel-body">
          <xsl:text> </xsl:text>
        </div>
      </div>
    </div>
    <div data-value="heading-only">
      <div class="heading-only">
        <div class="panel-heading">
          <h6 class="panel-title">Heading Only</h6>
        </div>
      </div>
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
    <option value="bg-dark text-white">
      <xsl:if test="$value='bg-dark text-white'">
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
  </xsl:template>
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
    ]},

    {title: 'Blocks', items: [
    {title: 'Paragraph', block: 'p'},
    {title: 'Blockquote', block: 'blockquote'},
    {title: 'Div', block: 'div'},
    {title: 'Pre', block: 'pre'}
    ]},

    {title: 'Alignment', items: [
    {title: 'Left', block: 'div', styles : {textAlign : 'left'}, icon: 'alignleft'},
    {title: 'Center', block: 'div', styles : {textAlign : 'center'}, icon: 'aligncenter'},
    {title: 'Right', block: 'div', styles : {textAlign : 'right'}, icon: 'alignright'},
    {title: 'Justify', block: 'div', styles : {textAlign : 'justify'}, icon: 'alignjustify'}
    ]},
    {title: 'Lead', inline: 'span', classes: 'lead'},
    {title: 'Small', inline: 'span', classes: 'small'},
    {title: 'Button', inline: 'span', classes: 'btn btn-default'}
    ],
  </xsl:template>
</xsl:stylesheet>

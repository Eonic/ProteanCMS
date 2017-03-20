<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
  <!-- ######################################## IMPORT ALL COMMON XSL's ########################################### -->

  <xsl:import href="../CommonImports.xsl"/>

  <!-- ############################################## OUTPUT TYPE ################################################# -->

  <xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml11.dtd" encoding="UTF-8"/>
  <xsl:template match="/">
    <xsl:apply-imports/>
  </xsl:template>

  <xsl:variable name="GuideSite" select="'http://eonicweb.com'"/>


  <xsl:template match="Page">
    <xsl:variable name="dirtyGuide">
      <div>
        <xsl:apply-templates select="//Content[@position='header']" mode="displayModule"/>
        <xsl:apply-templates select="//Content[@position='column1']" mode="displayModule"/>
        <xsl:apply-templates select="//Content[@position='footer']" mode="displayModule"/>
        <!--<button id="userguideFormToggle">Need more help? Click here to contact us</button>
        <div id="userguideForm" class="userguideFormHidden">
          <xsl:apply-templates select="//Content[model/submission/@id='userguideForm']" mode="xform"/>
        </div>-->
        <a href="?ewCmd=AdmHome" class="externallink" target="_blank">Need more help? Click here to contact us</a>
      </div>
    </xsl:variable>
    <xsl:apply-templates select="ms:node-set($dirtyGuide)/*" mode="cleanXhtml"/>
  </xsl:template>


  <!--<xsl:template match="Page">
    <xsl:copy-of select="."/>  
  </xsl:template>-->

  <!-- IMAGE PROCESSING  -->
  <xsl:template match="img[not(parent::a)]" mode="cleanXhtml">
    <xsl:variable name="imagepath" select="@src"/>
    <xsl:variable name="newpath" select="@src"/>
    <!-- Stick in Variable and then ms:nodest it 
          - ensures its self closing and we can process all nodes!! -->
    <xsl:variable name="img">

      <xsl:element name="a">
        <xsl:attribute name="href">
          <xsl:value-of select="$GuideSite"/>
          <xsl:value-of select="$imagepath"/>
        </xsl:attribute>
        <xsl:attribute name="class">
          <xsl:if test="@width &gt; 300">
            <xsl:text>lightbox </xsl:text>
          </xsl:if>
        </xsl:attribute>

        <xsl:element name="img">
		      <xsl:attribute name="class">
            <xsl:if test="@width &gt; 300">
	            <xsl:text>img-responsive</xsl:text>
            </xsl:if>
	        </xsl:attribute>
          <xsl:for-each select="@*[name()!='border' and name()!='align' and name()!='style']">

            <xsl:attribute name="{name()}">
              <xsl:choose>

                <!-- ##### @Attribute Conditions ##### -->
                <xsl:when test="name()='src'">
                  <xsl:choose>
                    <xsl:when test="contains(.,'http://')">
                      <xsl:value-of select="."/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="$GuideSite"/>
                      <xsl:value-of select="$imagepath"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>

                <xsl:when test="name()='width'">
                  <xsl:value-of select="ew:ImageWidth($newpath)" />
                </xsl:when>

                <xsl:when test="name()='height'">
                  <xsl:value-of select="ew:ImageHeight($newpath)" />
                </xsl:when>
		<xsl:when test="name()='class'">
                  <xsl:value-of select="."  />
                  <xsl:text> img-responsive</xsl:text>
                </xsl:when>
                <!-- ##### VALIDATION - Attribute "align" can not be used for this element. ##### -->
                <xsl:when test="name()='class' and (ancestor::img[@align] or contains(ancestor::img/@style,'float: '))">
                  <xsl:variable name="align" select="ancestor::img/@align"/>
                  <xsl:variable name="float" select="substring-before(substring-after(ancestor::img/@style,'float: '),';')"/>
                  <xsl:value-of select="."  />
                  <xsl:text> img-responsive align</xsl:text>
                  <xsl:choose>
                    <xsl:when test="@align">
                      <xsl:value-of select="$align"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <xsl:value-of select="$float"/>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="."  />
                </xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
          </xsl:for-each>

          <!-- ##### VALIDATION - Attribute "align" can not be used for this element. ##### -->
          <xsl:if test="not(@class) and (@align or contains(@style,'float: '))">
            <xsl:attribute name="class">
              <xsl:variable name="float" select="substring-before(substring-after(@style,'float: '),';')"/>
              <xsl:variable name="align" select="@align"/>
              <xsl:text>align</xsl:text>
              <xsl:choose>
                <xsl:when test="@align">
                  <xsl:value-of select="$align"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$float"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
          </xsl:if>

          <!-- ##### VALIDATION - required attribute "alt" ##### -->
          <xsl:if test="not(@alt)">
            <xsl:attribute name="alt"></xsl:attribute>
          </xsl:if>

        </xsl:element>

        <xsl:if test="@width &gt; 300">
          <span id="guideZoomIcon">
            <img src="/ewcommon/images/admin/guideZoomIcon.png" width="49" height="43" alt="guideZoomIcon"/>
          </span>
        </xsl:if>

      </xsl:element>
    </xsl:variable>
    <xsl:copy-of select="ms:node-set($img)"/>
  </xsl:template>


  <!--<xsl:template match="*[name()='a' and not(ancestor::Page)]" mode="cleanXhtml">

    <xsl:element name="{name()}">

      <xsl:for-each select="@*">
        <xsl:attribute name="{name()}">

          <xsl:choose>
            <xsl:when test="name()='href' and not(contains(.,'http://'))">
              <xsl:value-of select="$GuideSite"/>
              <xsl:value-of select="." />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="." />
            </xsl:otherwise>
          </xsl:choose>

        </xsl:attribute>
      </xsl:for-each>

      <xsl:apply-templates mode="cleanXhtml"/>

    </xsl:element>
  </xsl:template>-->

  <!--Match on Menu Item - Build URL for that MenuItem-->
  <xsl:template match="MenuItem" mode="getHref">

    <!--absolute url false by default-->

    <xsl:param name="absoluteURL" select="false()" />

    <xsl:variable name="siteURL">
      <xsl:call-template name="getSiteURL"/>
    </xsl:variable>
    <xsl:variable name="url" select="@url"/>

    <xsl:choose>
      <xsl:when test="@url!=''">
        <xsl:choose>
          <xsl:when test="format-number(@url,'0')!='NaN'">
            <xsl:value-of select="$page/Menu/descendant-or-self::MenuItem[@id=$url]/@url"/>
          </xsl:when>
          <xsl:when test="contains(@url,'http')">
            <xsl:value-of select="@url"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>/ewcommon/tools/eonicwebGuide.ashx?fRef=</xsl:text>
            <xsl:value-of select="//MenuItem[@url=$url]/@ref"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>/</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

</xsl:stylesheet>
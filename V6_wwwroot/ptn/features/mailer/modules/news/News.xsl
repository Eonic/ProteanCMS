<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
  <!-- ############## News Articles ##############   -->

  <!-- wrapping code is generic from layout.xsl with mode displayBrief, override here to customise-->

  <xsl:template match="Content[@type='Module' and @moduleType='NewsList']" mode="themeModuleExtras">
    <!-- this is empty because we want this on individual listing panels not the containing module-->
  </xsl:template>

  <xsl:template match="Content[@type='Module' and @moduleType='NewsList']" mode="themeModuleClassExtras">
    <!-- this is empty because we want this on individual listing panels not the containing module-->
  </xsl:template>

  <!-- NewsArticle Brief -->
  <xsl:template match="Content[@type='NewsArticle']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <xsl:param name="crop"/>
    <xsl:param name="class"/>
    <xsl:param name="parentId"/>
    <xsl:param name="linked"/>
    <xsl:param name="itemLayout"/>
    <xsl:param name="heading"/>
    <xsl:param name="title"/>
    <xsl:variable name="parentURL">
      <xsl:apply-templates select="." mode="getHref"/>
    </xsl:variable>
    <xsl:variable name="classValues">
      <xsl:text>listItem newsarticle </xsl:text>
      <xsl:if test="$linked='true'">
        <xsl:text> linked-listItem </xsl:text>
      </xsl:if>
      <xsl:if test="$itemLayout='wide'">
        <xsl:text> wide-item </xsl:text>
      </xsl:if>
      <xsl:value-of select="$class"/>
      <xsl:text> </xsl:text>
      <xsl:apply-templates select="." mode="themeModuleClassExtrasListItem">
        <xsl:with-param name="parentId" select="$parentId"/>
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="cropSetting">
      <xsl:choose>
        <xsl:when test="$crop='true'">
          <xsl:value-of select="true()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="false()"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <td class="{$classValues}">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="concat($classValues,' ',$class)"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <xsl:if test="Images/img/@src!=''">
          <a href="{$parentURL}" class="list-image-link">
            <xsl:apply-templates select="." mode="displayThumbnail">
              <xsl:with-param name="crop" select="$cropSetting" />
              <xsl:with-param name="class">list-image img-fluid</xsl:with-param>
            </xsl:apply-templates>
          </a>
        </xsl:if>
        <div class="media-inner">
          <xsl:choose>
            <xsl:when test="$title!='' and $heading!=''">
              <xsl:variable name="headingNo" select="substring-after($heading,'h')"/>
              <xsl:variable name="headingNoPlus" select="$headingNo + 1"/>
              <xsl:variable name="listHeading">
                <xsl:text>h</xsl:text>
                <xsl:value-of select="$headingNoPlus"/>
              </xsl:variable>
              <xsl:element name="{$listHeading}">
                <xsl:attribute name="class">
                  <xsl:text>title</xsl:text>
                </xsl:attribute>
                <a href="{$parentURL}">
                  <xsl:apply-templates select="." mode="getDisplayName"/>
                </a>
              </xsl:element>
            </xsl:when>
            <xsl:otherwise>
              <xsl:choose>
                <xsl:when test="$heading=''">
                  <h3 class="title">
                    <a href="{$parentURL}">
                      <xsl:apply-templates select="." mode="getDisplayName"/>
                    </a>
                  </h3>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:element name="{$heading}">
                    <xsl:attribute name="class">
                      <xsl:text>title</xsl:text>
                    </xsl:attribute>
                    <a href="{$parentURL}">
                      <xsl:apply-templates select="." mode="getDisplayName"/>
                    </a>
                  </xsl:element>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:otherwise>
          </xsl:choose>
          <span class="news-brief-info">
            <xsl:apply-templates select="Content[@type='Contact' and @rtype='Author'][1]" mode="displayAuthorBrief"/>
            <xsl:if test="@publish!=''">
              <p class="date" itemprop="datePublished">
                <xsl:value-of select="/Page/Contents/Content[@name='articleLabel']"/>
                <xsl:call-template name="DisplayDate">
                  <xsl:with-param name="date" select="@publish"/>
                </xsl:call-template>
              </p>
            </xsl:if>
            <xsl:if test="@update!=''">
              <p class="hidden" itemprop="dateModified">
                <xsl:value-of select="/Page/Contents/Content[@name='articleLabel']"/>
                <xsl:call-template name="DisplayDate">
                  <xsl:with-param name="date" select="@update"/>
                </xsl:call-template>
              </p>
            </xsl:if>
          </span>

          <xsl:if test="Strapline/node()!=''">
            <div class="summary" itemprop="description">
              <xsl:apply-templates select="Strapline/node()" mode="cleanXhtml"/>
            </div>
          </xsl:if>
          <div class="entryFooter">
            <xsl:apply-templates select="." mode="displayTags"/>
            <xsl:apply-templates select="." mode="moreLink">
              <xsl:with-param name="link" select="$parentURL"/>
              <xsl:with-param name="stretchLink" select="$linked"/>
              <xsl:with-param name="altText">
                <xsl:value-of select="Headline/node()"/>
              </xsl:with-param>
            </xsl:apply-templates>
            <xsl:text> </xsl:text>
          </div>
        </div>
      </div>
    </td>
  </xsl:template>

</xsl:stylesheet>
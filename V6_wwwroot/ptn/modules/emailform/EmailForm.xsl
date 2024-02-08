<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!-- #### Email Form  ####   -->
  <!-- Email Form Module -->
  <xsl:template match="Content[@type='Module' and (@moduleType='EmailForm' or @moduleType='xForm')]" mode="displayBrief">
    <xsl:choose>
      <xsl:when test="descendant::alert/node()='Message Sent'">
        <xsl:apply-templates select="." mode="mailformSentMessage"/>
      </xsl:when>
      <xsl:otherwise>
        <div class="EmailForm">
          <xsl:if test="@formLayout='horizontal'">
            <xsl:attribute name="class">EmailForm horizontalForm</xsl:attribute>
          </xsl:if>
          <xsl:if test="@hideLabel='true'">
            <xsl:attribute name="class">EmailForm hideLabel</xsl:attribute>
          </xsl:if>
          <!-- display form-->
          <xsl:apply-templates select="." mode="cleanXhtml"/>
        </div>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:if test="/Page/@adminMode">
      <div class="sentMessage">
        <xsl:choose>
          <xsl:when test="Content[@type='FormattedText']">
            <xsl:apply-templates select="Content[@type='FormattedText']" mode="inlinePopupOptions">
              <xsl:with-param name="class" select="'sentMessage'"/>
            </xsl:apply-templates>
            <em>[submission message]</em>
            <xsl:apply-templates select="Content[@type='FormattedText']" mode="displayBrief"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="." mode="inlinePopupRelate">
              <xsl:with-param name="type">FormattedText</xsl:with-param>
              <xsl:with-param name="text">Add submission message</xsl:with-param>
              <xsl:with-param name="name">
                <xsl:text>Submission message </xsl:text>
                <xsl:value-of select="@title"/>
              </xsl:with-param>
              <xsl:with-param name="find">true</xsl:with-param>
            </xsl:apply-templates>
          </xsl:otherwise>
        </xsl:choose>
      </div>
    </xsl:if>
  </xsl:template>

  <!-- Email Sent Message -->
  <xsl:template match="Content" mode="mailformSentMessage">
    <xsl:choose>
      <!-- WHEN RELATED SENT MESSAGE -->
      <xsl:when test="Content[@type='FormattedText']">
        <div class="sentMessage">
          <xsl:apply-templates select="Content[@type='FormattedText']/node()" mode="cleanXhtml" />
        </div>
      </xsl:when>
      <!-- WHEN SENT MESSAGE ON PAGE -->
      <xsl:when test="/Page/Contents/Content[@name = 'sentMessage' and (@type='FormattedText' or @type='Image')]">
        <div class="sentMessage">
          <xsl:apply-templates select="/Page/Contents/Content[@name = 'sentMessage' and (@type='FormattedText' or @type='Image')]" mode="displayBrief"/>
        </div>
      </xsl:when>
      <!-- OTHERWISE SHOW FORM -->
      <xsl:otherwise>
        <xsl:apply-templates select="." mode="cleanXhtml"/>
      </xsl:otherwise>
    </xsl:choose>
    <!-- THEN TRACKING CODE -->
    <xsl:call-template name="LeadTracker"/>
  </xsl:template>

  <!-- Lead Tracker -->
  <xsl:template name="LeadTracker">
    <!-- OVERWRITE THIS TEMPLATE AND INSERT THE APPROPRIATE GOOGLE LEAD TRACKING JAVASCRIPT IF REQUIRED -->
  </xsl:template>

  <!-- Template to show an Xform when found in ContentDetail -->
  <xsl:template match="Content[@type='Module' and (@moduleType='EmailForm' or @moduleType='xForm')]" mode="cleanXhtml">
    <xsl:apply-templates select="." mode="xform"/>
  </xsl:template>

  <!-- X Form Module *** not finished *** PH-->
  <xsl:template match="Content[@type='Module' and @moduleType='XForm']" mode="displayBrief">
    <xsl:choose>
      <xsl:when test="/Page/Contents/Content/@name = 'sentMessage' and /Page/Contents/Content[@type='Module' and @moduleType='EmailForm']/descendant::alert/node()='Message Sent'">
        <xsl:apply-templates select="/" mode="mailformSentMessage"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:if test="@box='false'">
          <div class="EmailForm">
            <xsl:if test="@formLayout='horizontal'">
              <xsl:attribute name="class">EmailForm horizontalForm</xsl:attribute>
            </xsl:if>
            <xsl:if test="@hideLabel='true'">
              <xsl:attribute name="class">EmailForm hideLabel</xsl:attribute>
            </xsl:if>
          </div>
        </xsl:if>
        <xsl:apply-templates select="." mode="xform"/>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:if test="/Page/@adminMode">
      <div id="sentMessage">
        <xsl:apply-templates select="/Page" mode="inlinePopupSingle">
          <xsl:with-param name="type">FormattedText</xsl:with-param>
          <xsl:with-param name="text">Add Sent Message</xsl:with-param>
          <xsl:with-param name="name">sentMessage</xsl:with-param>
        </xsl:apply-templates>
        <xsl:apply-templates select="/Page/Contents/Content[@name = 'sentMessage' and (@type='FormattedText' or @type='Image')]" mode="displayBrief"/>
      </div>
    </xsl:if>
  </xsl:template>

  <xsl:template match="Content[@type='xform']" mode="ContentDetail">
    <xsl:apply-templates select="." mode="xform"/>
  </xsl:template>
  
</xsl:stylesheet>
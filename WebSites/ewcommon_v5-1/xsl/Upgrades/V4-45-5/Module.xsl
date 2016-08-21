<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns:ew="urn:ew">
  <xsl:output method="xml" indent="no" standalone="yes" omit-xml-declaration="yes" encoding="UTF-8"/>

  <!--  
        THIS FILE IS THE DEFAULT CONTENT UPGRADE FILE.
        IT CURRENTLY WRITES THE NODES BACK EXACTLY AS THEY COME OUT.
        YOU CAN JUMP IN AND ALTER ANY NODE() BY OVERWRITING THE mode="writeNodes" TEMPLATE,
        USING A DIFFERENT NODE MATCH
        
        **NB. MAKE SURE THERE ARE NO EXTRA LINES AFTER THE LAST TAG AS THIS WILL CAUSE AN ERROR**
  -->
  <xsl:variable name="nContentId" select="/instance/tblContent/nContentKey/node()"/>

  <xsl:template match="/instance">
    <instance>
      <xsl:for-each select="*">
        <xsl:apply-templates select="." mode="writeNodes"/>
      </xsl:for-each>
    </instance>
  </xsl:template>

  <!-- -->

  <xsl:template match="*" mode="writeNodes">
    <xsl:element name="{name()}">
      <xsl:for-each select="@*">
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>

  <!-- -->

  <xsl:template match="img" mode="writeNodes">
    <img src="{@src}" width="{@width}" height="{@height}" alt="{@alt}" class="{@class}" style="{@style}"/>
  </xsl:template>

  <!-- -->

  <xsl:template match="br" mode="writeNodes">
    <br/>
  </xsl:template>

  <!-- -->

  <xsl:template match="cContentName" mode="writeNodes">
    <xsl:variable name="moduleName" select="node()" />
    <xsl:element name="{name()}">
      <xsl:choose>
        <!-- Remove IF position is name and replace with Title -->
        <xsl:when test="contains($moduleName,'column') and contains($moduleName,'_m')">
          <xsl:value-of select="//Content/Title/node()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="node()"/>
        </xsl:otherwise>
      </xsl:choose>

    </xsl:element>
  </xsl:template>
  
    <!-- ================================= DEFAULT CONTENT MATCH ================================= -->
  <!-- This should skip anything allready upgraded and leave it the same --> 
  <xsl:template match="Content[not(@moduleType)]" mode="writeNodes">
    <xsl:variable name="position">
      <xsl:variable name="moduleName" select="//cContentName/node()" />
      <xsl:choose>
        <!-- When name is the position, extract position-->
          <xsl:when test="contains($moduleName,'column') and contains($moduleName,'_m')">
            <xsl:value-of select="substring-before($moduleName,'_m')"/>
          </xsl:when>
          <xsl:otherwise>
              <xsl:value-of select="$moduleName"/>
          </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="getPosition" select="ew:UpdatePositions($nContentId,$position)"/>
    <xsl:variable name="moduleType">
      <xsl:apply-templates select="." mode="getModuleType"/>
    </xsl:variable>

    <!-- Write Content Node -->
    <xsl:element name="{name()}">
      <!-- Write Elements -->

      <!-- Module Type -->
      <xsl:attribute name="moduleType">
        <xsl:value-of select="$moduleType"/>
      </xsl:attribute>
      
		<xsl:choose>
			<xsl:when test="Type[node()='List_Articles']">
				<xsl:attribute name="contentType">NewsArticle</xsl:attribute>
			</xsl:when>
			<xsl:when test="Type[node()='List_Products']">
				<xsl:attribute name="contentType">Product</xsl:attribute>
			</xsl:when>
			<xsl:when test="Type[node()='List_Links']">
				<xsl:attribute name="contentType">Link</xsl:attribute>
			</xsl:when>
			<xsl:when test="Type[node()='List_Testimonials']">
				<xsl:attribute name="contentType">Testimonial</xsl:attribute>
			</xsl:when>
			<xsl:when test="Type[node()='List_Events']">
				<xsl:attribute name="contentType">Event</xsl:attribute>
			</xsl:when>
      <xsl:when test="Type[node()='List_Documents' or node()='List_Documents2']">
        <xsl:attribute name="contentType">Document</xsl:attribute>
      </xsl:when>
      <xsl:when test="Type[node()='List_SubPages']">
        <xsl:attribute name="contentType">MenuItem</xsl:attribute>
      </xsl:when>
      <xsl:when test="Type[node()='List_SubPages_Section']">
        <xsl:attribute name="contentType">MenuItem</xsl:attribute>
      </xsl:when>
      <xsl:when test="Type[node()='List_Contacts']">
        <xsl:attribute name="contentType">Contact</xsl:attribute>
      </xsl:when>
      <xsl:when test="Type[node()='List_Vacancies']">
        <xsl:attribute name="contentType">Job</xsl:attribute>
      </xsl:when>
      <xsl:when test="Type[node()='bespoke1' or node()='List_Related_CaseStudies']">
        <xsl:attribute name="contentType">CaseStudy</xsl:attribute>
      </xsl:when>
      <!--<xsl:otherwise>
				<xsl:attribute name="contentType"></xsl:attribute>
			</xsl:otherwise>-->
		</xsl:choose>

      <!-- Box? -->
      <xsl:attribute name="box">
          <xsl:if test="Boxed/node()='true'">
            <xsl:value-of select="Boxed/@color"/>
          </xsl:if>
      </xsl:attribute>

      <!-- icon? -->
      <xsl:attribute name="icon">
        <xsl:choose>
          <xsl:when test="Boxed/node()='true'">
            <xsl:value-of select="Boxed/@icon"/>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text></xsl:text>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>

      <!-- Title -->
      <xsl:attribute name="title">
        <xsl:if test="Title/@display!='false'">
          <xsl:value-of select="Title/node()"/>
        </xsl:if>
      </xsl:attribute>

      <!-- Link -->
      <xsl:attribute name="link">
        <xsl:if test="Type/node()='BasicContentTypes'">
          <xsl:variable name="relContentType" select="ew:ContentQuery(concat('content_Mod',$nContentId),'descendant-or-self::cContentSchemaName/node()',0)"/>
          <xsl:if test="$relContentType='ImageLink'">
            <xsl:value-of select="ew:ContentQuery(concat('content_Mod',$nContentId),'descendant-or-self::Content/Link/@pageId',0)"/>
          </xsl:if>
        </xsl:if>
        <xsl:if test="Type/node()='List_SubPages_Section'">
          <xsl:value-of select="Type/@pgId"/>
        </xsl:if>
      </xsl:attribute>

      <!-- LinkText -->
      <xsl:attribute name="linkText">
        <xsl:text></xsl:text>
      </xsl:attribute>

      <!-- orderBy -->
      <xsl:attribute name="sortBy">
        <xsl:call-template name="getSortBy">
          <xsl:with-param name="moduleType" select="$moduleType" />
        </xsl:call-template>
      </xsl:attribute>

      <xsl:attribute name="order">
        <xsl:choose>
          <xsl:when test="$moduleType='NewsList'">descending</xsl:when>
          <xsl:otherwise>ascending</xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>

      <!-- Colums -->
      <xsl:attribute name="cols">
        <xsl:text>1</xsl:text>
      </xsl:attribute>

      <!-- Step Count -->
      <xsl:attribute name="stepCount">
        <xsl:text>0</xsl:text>
      </xsl:attribute>

      <!-- Display -->
      <xsl:attribute name="display">
        <xsl:choose>
          <xsl:when test="Type[node()='List_Related_CaseStudies']">
            <xsl:text>related</xsl:text>
          </xsl:when>
          <xsl:when test="Type[node()='List_SubPages_Section']">
            <xsl:text>related</xsl:text>
          </xsl:when>
          <xsl:otherwise>
            <xsl:text>all</xsl:text>
          </xsl:otherwise>
        </xsl:choose>
        
      </xsl:attribute>

      <!-- POSITION -->
      <xsl:attribute name="position">
        <xsl:value-of select="$position"/>
      </xsl:attribute>

      <xsl:apply-templates select="." mode="getModuleContent"/>

    </xsl:element>
  </xsl:template>

  <!-- ================================= MODULE TYPES =================================-->
  <xsl:template match="Content" mode="getModuleType">
    <!-- Default is what it was before, to alert to bespoke types.-->
    <xsl:value-of select="Type/node()"/>

  </xsl:template>
  <!-- -->
  <xsl:template match="Content[not(Type/node())]" mode="getModuleType">FormattedText</xsl:template>
  <!-- -->
  <xsl:template match="Content[Type/node()='BasicContentTypes']" mode="getModuleType">
    <xsl:variable name="relContentType" select="ew:ContentQuery(concat('content_Mod',$nContentId),'descendant-or-self::cContentSchemaName/node()',0)"/>
    <xsl:choose>
      <xsl:when test="$relContentType='ImageLink'">
        <xsl:text>Image</xsl:text>
      </xsl:when>
      <xsl:when test="$relContentType='FormattedText'">
        <xsl:text>FormattedText</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$relContentType"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[Type/node()='List_Articles']" mode="getModuleType">
    <xsl:text>NewsList</xsl:text>
  </xsl:template>
  
 
  <!-- -->
  <xsl:template match="Content[Type/node()='List_Products']" mode="getModuleType">
    <xsl:text>ProductList</xsl:text>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[Type/node()='List_Events']" mode="getModuleType">
    <xsl:text>EventList</xsl:text>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[Type/node()='List_Links']" mode="getModuleType">
    <xsl:text>LinkList</xsl:text>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[Type/node()='List_Testimonials']" mode="getModuleType">
    <xsl:text>TestimonialList</xsl:text>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[Type/node()='List_Documents2']" mode="getModuleType">
    <xsl:text>DocumentList</xsl:text>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[Type/node()='List_of_Polls']" mode="getModuleType">
    <xsl:text>PollList</xsl:text>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[Type/node()='List_SubPages']" mode="getModuleType">
    <xsl:text>SubPageList</xsl:text>
  </xsl:template>
  <xsl:template match="Content[Type/node()='List_SubPages_Section']" mode="getModuleType">
    <xsl:text>SubPageList</xsl:text>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[Type/node()='List_FLVMovies']" mode="getModuleType">
    <xsl:text>VideoList</xsl:text>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[Type/node()='List_SiteMap']" mode="getModuleType">
    <xsl:text>SiteMapList</xsl:text>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[Type/node()='List_Feeds']" mode="getModuleType">
    <xsl:text>FeedList</xsl:text>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[Type/node()='MailFormHandler']" mode="getModuleType">
    <xsl:text>xForm</xsl:text>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[Type/node()='userLogon']" mode="getModuleType">
    <xsl:text>MembershipLogon</xsl:text>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[Type/node()='cover_flow']" mode="getModuleType">
    <xsl:text>CaseStudyImageFlow</xsl:text>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[Type/node()='ImageLink']" mode="getModuleType">
    <xsl:text>Image</xsl:text>
  </xsl:template>
  <!-- -->
  <xsl:template match="Content[Type/node()='List_Contacts']" mode="getModuleType">
    <xsl:text>ContactList</xsl:text>
  </xsl:template>

  <xsl:template match="Content[Type/node()='List_Vacancies']" mode="getModuleType">
    <xsl:text>VacanciesList</xsl:text>
  </xsl:template>
  <xsl:template match="Content[Type/node()='bespoke1']" mode="getModuleType">
    <xsl:text>CaseStudiesList</xsl:text>
  </xsl:template>

  <xsl:template match="Content[Type/node()='List_Related_CaseStudies']" mode="getModuleType">
    <xsl:text>CaseStudiesList</xsl:text>
  </xsl:template>
  

  <xsl:template match="Content" mode="getModuleContent">
    <!--<xsl:copy-of select="Header/node()"/>-->
    <xsl:apply-templates select="Header/node()" mode="writeNodes"/>
    <xsl:apply-templates select="Footer/node()" mode="writeNodes"/>
  </xsl:template>
  
  <xsl:template match="Content[Type/node()='BasicContentTypes']" mode="getModuleContent">
    <xsl:variable name="relContentType" select="ew:ContentQuery(concat('content_Mod',$nContentId),'descendant-or-self::cContentSchemaName/node()',0)"/>
    <xsl:variable name="relContentId" select="ew:ContentQuery(concat('content_Mod',$nContentId),'descendant-or-self::nContentKey/node()',0)"/>
    <xsl:choose>
      <xsl:when test="$relContentType='FormattedText'">
        <xsl:variable name="oContent" select="ew:ContentQuery(concat('content_Mod',$nContentId),'descendant-or-self::Content',0)"/>
        <xsl:copy-of select="ms:node-set($oContent)/node()"/>
        <!--xsl:variable name="delete" select="ew:DeleteContent($relContentId)"/-->
      </xsl:when>
      <xsl:when test="$relContentType='Image'">
        <xsl:copy-of select="ew:ContentQuery(concat('content_Mod',$nContentId),'descendant-or-self::Content/img',0)"/>
        <!--xsl:variable name="delete" select="ew:DeleteContent($relContentId)"/-->
      </xsl:when>
      <xsl:when test="$relContentType='ImageLink'">
        <xsl:copy-of select="ew:ContentQuery(concat('content_Mod',$nContentId),'descendant-or-self::Content/img',0)"/>
        <!--xsl:variable name="delete" select="ew:DeleteContent($relContentId)"/-->
      </xsl:when>
      <xsl:when test="$relContentType='PlainText'">
        <xsl:copy-of select="ew:ContentQuery(concat('content_Mod',$nContentId),'descendant-or-self::Content/node()',0)"/>
        <!--xsl:variable name="delete" select="ew:DeleteContent($relContentId)"/-->
      </xsl:when>
      <xsl:when test="$relContentType='AdSenseAdvert'">
        <xsl:copy-of select="ew:ContentQuery(concat('content_Mod',$nContentId),'descendant-or-self::Content/node()',0)"/>
        <!--xsl:variable name="delete" select="ew:DeleteContent($relContentId)"/-->
      </xsl:when>
      <xsl:when test="$relContentType='Error-ContentMissing'">
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy-of select="ew:ContentQuery(concat('content_Mod',$nContentId),'descendant-or-self::Content/node()',0)"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="Content[Type/node()='MailFormHandler']" mode="getModuleContent">
    <xsl:variable name="oContent" select="ew:ContentQuery('mailform','descendant-or-self::Content',0)"/>
    <xsl:copy-of select="ms:node-set($oContent)/node()"/>
  </xsl:template>

  <!-- ================================= SORTBY @sortBy =================================-->
  <xsl:template name="getSortBy">
    <xsl:param name="moduleType" />
    <xsl:choose>
      <xsl:when test="$moduleType='NewsList'">
        <xsl:text>publish</xsl:text>
      </xsl:when>
      <xsl:when test="$moduleType='ProductList'">
        <xsl:text>Position</xsl:text>
      </xsl:when>
      <xsl:when test="$moduleType='EventList'">
        <xsl:text>StartDate</xsl:text>
      </xsl:when>
      <xsl:when test="$moduleType='LinkList'">
        <xsl:text>Name</xsl:text>
      </xsl:when>
      <xsl:when test="$moduleType='TestimonialList'">
        <xsl:text>publish</xsl:text>
      </xsl:when>
      <xsl:when test="$moduleType='DocumentList'">
        <xsl:text>ascending</xsl:text>
      </xsl:when>
      <xsl:when test="$moduleType='DocumentList'">
        <xsl:text>ascending</xsl:text>
      </xsl:when>
      <xsl:when test="$moduleType='PollList'">
        <xsl:text>ascending</xsl:text>
      </xsl:when>
      <xsl:when test="$moduleType='SubPageList'">
        <xsl:text>Position</xsl:text>
      </xsl:when>
      <xsl:when test="$moduleType='VideoList'">
        <xsl:text>publish</xsl:text>
      </xsl:when>
      <xsl:when test="$moduleType='SiteMapList'">
        <xsl:text>Position</xsl:text>
      </xsl:when>
      <xsl:when test="$moduleType='FeedList'">
        <xsl:text>publish</xsl:text>
      </xsl:when>
      <xsl:when test="$moduleType='MailFormHandler'">
        <xsl:text>Position</xsl:text>
      </xsl:when>
      <xsl:when test="$moduleType='MailFormHandler'">
        <xsl:text>Position</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>name</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
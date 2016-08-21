<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">

  <xsl:template match="Page[@layout='Synchronisation']" mode="Admin">
    <div class="template" id="template_Synchronisation">
      <div id="column2">
          <h3>Available Synchronisation Actions</h3>
        <table cellpadding="0" cellspacing="0" class="adminList">
          <th>Action</th>
          <th>&#160;</th>
          <xsl:for-each select="ContentDetail/SyncAction">
            <tr>
              <td>
                <xsl:value-of select="node()"/>
              </td>
              <td>
                <a href="?ewCmd=Sync&amp;ewCmd2={node()}" class="button">Run Now</a>
              </td>
            </tr>
          </xsl:for-each>
        </table>
      </div>
      <div id="column1">

      </div>
      <div class="terminus">&#160;</div>
    </div>
  </xsl:template>

  <xsl:template match="Page[@layout='SynchronisationResults']" mode="Admin">
    <div class="template" id="template_Synchronisation">
      <div id="column2">
        <h3>
          <xsl:text>Synchronisation Results For: </xsl:text>
          <xsl:value-of select="/Page/Request/QueryString/Item[@name='ewCmd2']/node()"/>
        </h3>
        <xsl:apply-templates select="ContentDetail/Result/Results" mode="TableDrillDown"/>
        <xsl:if test="/Page/ContentDetail/Result/ul/li/strong/node()='Error'">
          <xsl:copy-of select="/Page/ContentDetail/Result"/>
        </xsl:if>
      </div>
      <div id="column1">

      </div>
      <div class="terminus">&#160;</div>
    </div>
  </xsl:template>
  <xsl:template match="Page[@layout='SynchronisationXForm']" mode="Admin">
    <div class="template" id="template_Synchronisation">
      <div id="column2">
        <xsl:apply-templates select="/Page/ContentDetail/Content[@type='xform']" mode="xform"/>
      </div>
      <div id="column1">
        <!--<iframe src="/ewcommon/KeepAlive/KA.aspx" width="400px" />-->
      </div>
      <div class="terminus">&#160;</div>
    </div>
  </xsl:template>
  
  <xsl:template match="*" mode="TableDrillDown">
    <ul>
      <li>
        <strong><xsl:value-of select="local-name()"/>
        </strong>
        <xsl:if test="count(*)=0">
          <xsl:text>: </xsl:text><xsl:value-of select="node()"/>
        </xsl:if>
        <xsl:if test="@*">
          <ul>
            <xsl:for-each select="@*">
              <li>
                <strong>
                  <xsl:value-of select="local-name()"/>
                </strong>:<xsl:value-of select="."/>
              </li>
            </xsl:for-each>
          </ul> 
        </xsl:if>
        <xsl:apply-templates select="*" mode="TableDrillDown"/>
      </li>
    </ul>
  </xsl:template>
</xsl:stylesheet> 


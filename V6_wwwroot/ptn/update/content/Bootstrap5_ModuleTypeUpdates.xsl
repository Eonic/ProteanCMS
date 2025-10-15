<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns:ew="urn:ew">
  <xsl:output method="xml" indent="no" standalone="yes" omit-xml-declaration="yes" encoding="UTF-8"/>

  <!--  IMPORTANT -->
  <!--  THIS UPGRADE, upgrades Contacts to have the new Locational information that is essential for
          - Google Maps,
          - Address formattating and standardisation across ew.
  -->

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
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:attribute name="{name()}">
          <xsl:value-of select="." />
        </xsl:attribute>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>

  <!-- -->

  <xsl:template match="img" mode="writeNodes">
    <img src="{@src}" width="{@width}" height="{@height}" alt="{@alt}" class="{@class}" />
  </xsl:template>

  <!-- -->

  <xsl:template match="br" mode="writeNodes">
    <br/>
  </xsl:template>

  <!-- 100 -->
  <xsl:template match="Content[@moduleType='1Column']" mode="writeNodes">
    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:choose>
          <xsl:when test="name()='moduleType'">
            <xsl:attribute name="{name()}">
              <xsl:text>MultiColumn</xsl:text>
            </xsl:attribute>
            <xsl:attribute name="colType">
              <xsl:text>true</xsl:text>
            </xsl:attribute>
            <xsl:if test="name()='xsCol'">
              <!--Extra small means screens under 576px wide-->
              <xsl:attribute name="{name()}">
                <xsl:text>1</xsl:text>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='smCol'">
              <!--Small screens are 576px-767px-->
              <xsl:attribute name="{name()}">
                <xsl:value-of select="@xsCol"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='mdCol'">
              <!--Medium screens are 768px-991px-->
              <xsl:attribute name="{name()}">
                <xsl:value-of select="@smCol"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='lgCol'">
              <!--Large screens are 992px-1199px-->
              <xsl:attribute name="{name()}">
                <xsl:value-of select="@mdCol"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='xlCol'">
              <!--Extra large screens are 1200px-1399px-->
              <xsl:attribute name="{name()}">
                <xsl:text>1</xsl:text>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="{name()}">
              <xsl:value-of select="." />
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>

  <!-- 33 33 33 -->
  <xsl:template match="Content[@moduleType='3Columns']" mode="writeNodes">
    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:choose>
          <xsl:when test="name()='moduleType'">
            <xsl:attribute name="{name()}">
              <xsl:text>MultiColumn</xsl:text>
            </xsl:attribute>
            <xsl:attribute name="colType">
              <xsl:text>true</xsl:text>
            </xsl:attribute>
            <xsl:if test="name()='xsCol'">
              <!--Extra small means screens under 576px wide-->
              <xsl:attribute name="{name()}">
                <xsl:text>1</xsl:text>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='smCol'">
              <!--Small screens are 576px-767px-->
              <xsl:attribute name="{name()}">
                <xsl:value-of select="@xsCol"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='mdCol'">
              <!--Medium screens are 768px-991px-->
              <xsl:attribute name="{name()}">
                <xsl:value-of select="@smCol"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='lgCol'">
              <!--Large screens are 992px-1199px-->
              <xsl:attribute name="{name()}">
                <xsl:value-of select="@mdCol"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='xlCol'">
              <!--Extra large screens are 1200px-1399px-->
              <xsl:attribute name="{name()}">
                <xsl:text>3</xsl:text>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="{name()}">
              <xsl:value-of select="." />
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>

  <!-- 25 25 25 25 -->
  <xsl:template match="Content[@moduleType='4Columns']" mode="writeNodes">
    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:choose>
          <xsl:when test="name()='moduleType'">
            <xsl:attribute name="{name()}">
              <xsl:text>MultiColumn</xsl:text>
            </xsl:attribute>
            <xsl:attribute name="colType">
              <xsl:text>true</xsl:text>
            </xsl:attribute>
            <xsl:if test="name()='xsCol'">
              <!--Extra small means screens under 576px wide-->
              <xsl:attribute name="{name()}">
                <xsl:text>1</xsl:text>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='smCol'">
              <!--Small screens are 576px-767px-->
              <xsl:attribute name="{name()}">
                <xsl:value-of select="@xsCol"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='mdCol'">
              <!--Medium screens are 768px-991px-->
              <xsl:attribute name="{name()}">
                <xsl:value-of select="@smCol"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='lgCol'">
              <!--Large screens are 992px-1199px-->
              <xsl:attribute name="{name()}">
                <xsl:value-of select="@mdCol"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='xlCol'">
              <!--Extra large screens are 1200px-1399px-->
              <xsl:attribute name="{name()}">
                <xsl:text>4</xsl:text>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="{name()}">
              <xsl:value-of select="." />
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>

  <!-- 20 20 20 20 20 -->
  <xsl:template match="Content[@moduleType='5Columns']" mode="writeNodes">
    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:choose>
          <xsl:when test="name()='moduleType'">
            <xsl:attribute name="{name()}">
              <xsl:text>MultiColumn</xsl:text>
            </xsl:attribute>
            <xsl:attribute name="colType">
              <xsl:text>true</xsl:text>
            </xsl:attribute>
            <xsl:if test="name()='xsCol'">
              <!--Extra small means screens under 576px wide-->
              <xsl:attribute name="{name()}">
                <xsl:text>1</xsl:text>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='smCol'">
              <!--Small screens are 576px-767px-->
              <xsl:attribute name="{name()}">
                <xsl:value-of select="@xsCol"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='mdCol'">
              <!--Medium screens are 768px-991px-->
              <xsl:attribute name="{name()}">
                <xsl:value-of select="@smCol"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='lgCol'">
              <!--Large screens are 992px-1199px-->
              <xsl:attribute name="{name()}">
                <xsl:value-of select="@mdCol"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='xlCol'">
              <!--Extra large screens are 1200px-1399px-->
              <xsl:attribute name="{name()}">
                <xsl:text>5</xsl:text>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="{name()}">
              <xsl:value-of select="." />
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>

  <!-- 16 16 16 16 16 16 -->
  <xsl:template match="Content[@moduleType='6Columns']" mode="writeNodes">
    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:choose>
          <xsl:when test="name()='moduleType'">
            <xsl:attribute name="{name()}">
              <xsl:text>MultiColumn</xsl:text>
            </xsl:attribute>
            <xsl:attribute name="colType">
              <xsl:text>true</xsl:text>
            </xsl:attribute>
            <xsl:if test="name()='xsCol'">
              <!--Extra small means screens under 576px wide-->
              <xsl:attribute name="{name()}">
                <xsl:text>1</xsl:text>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='smCol'">
              <!--Small screens are 576px-767px-->
              <xsl:attribute name="{name()}">
                <xsl:value-of select="@xsCol"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='mdCol'">
              <!--Medium screens are 768px-991px-->
              <xsl:attribute name="{name()}">
                <xsl:value-of select="@smCol"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='lgCol'">
              <!--Large screens are 992px-1199px-->
              <xsl:attribute name="{name()}">
                <xsl:value-of select="@mdCol"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='xlCol'">
              <!--Extra large screens are 1200px-1399px-->
              <xsl:attribute name="{name()}">
                <xsl:text>6</xsl:text>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="{name()}">
              <xsl:value-of select="." />
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>

  <!-- 50 50 -->
  <xsl:template match="Content[@moduleType='2columns5050']" mode="writeNodes">
    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:choose>
          <xsl:when test="name()='moduleType'">
            <xsl:attribute name="{name()}">
              <xsl:text>MultiColumn</xsl:text>
            </xsl:attribute>
            <xsl:attribute name="colType">
              <xsl:text>true</xsl:text>
            </xsl:attribute>
            <xsl:if test="name()='xsCol'">
              <!--Extra small means screens under 576px wide-->
              <xsl:attribute name="{name()}">

              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='smCol'">
              <!--Small screens are 576px-767px-->
              <xsl:attribute name="{name()}">
                <xsl:value-of select="@xsCol"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='mdCol'">
              <!--Medium screens are 768px-991px-->
              <xsl:attribute name="{name()}">
                <xsl:value-of select="@smCol"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='lgCol'">
              <!--Large screens are 992px-1199px-->
              <xsl:attribute name="{name()}">
                <xsl:value-of select="@mdCol"/>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='xlCol'">
              <!--Extra large screens are 1200px-1399px-->
              <xsl:attribute name="{name()}">
                <xsl:text>2</xsl:text>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="{name()}">
              <xsl:value-of select="." />
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>
  
  <!-- 83 16 -->
  <xsl:template match="Content[@moduleType='2columns8316']" mode="writeNodes">
    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:choose>
          <xsl:when test="name()='moduleType'">
            <xsl:attribute name="{name()}">
              <xsl:text>MultiColumn</xsl:text>
            </xsl:attribute>
            <xsl:attribute name="colType">
              <xsl:text>false</xsl:text>
            </xsl:attribute>
            <xsl:if test="name()='xsCol'">
              <!--Extra small means screens under 576px wide-->
              <xsl:attribute name="{name()}">

              </xsl:attribute>
            </xsl:if>

            <xsl:if test="name()='smCol'">
              <!--Small screens are 576px-767px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@xsCol='2'">
                    <xsl:text>5-6-1-6</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>

            <xsl:if test="name()='mdCol'">
              <!--Medium screens are 768px-991px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@smCol='2equal'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:when test="@smCol='2'">
                    <xsl:text>5-6-1-6</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='lgCol'">
              <!--Large screens are 992px-1199px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@mdCol='2equal'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:when test="@mdCol='2'">
                    <xsl:text>5-6-1-6</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='xlCol'">
              <!--Extra large screens are 1200px-1399px-->
              <xsl:attribute name="{name()}">
                <xsl:text>5-6-1-6</xsl:text>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="{name()}">
              <xsl:value-of select="." />
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>
  
  <!-- 80 20 ** inc. XXL to not default to 1 -->
  <xsl:template match="Content[@moduleType='2columns8020']" mode="writeNodes">
    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:choose>
          <xsl:when test="name()='moduleType'">
            <xsl:attribute name="{name()}">
              <xsl:text>MultiColumn</xsl:text>
            </xsl:attribute>
            <xsl:attribute name="colType">
              <xsl:text>false</xsl:text>
            </xsl:attribute>
            <xsl:if test="name()='xsCol'">
              <!--Extra small means screens under 576px wide-->
              <xsl:attribute name="{name()}">

              </xsl:attribute>
            </xsl:if>
            
            <xsl:if test="name()='smCol'">
              <!--Small screens are 576px-767px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@xsCol='2'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>
            
            <xsl:if test="name()='mdCol'">
              <!--Medium screens are 768px-991px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@smCol='2equal'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:when test="@smCol='2'">
                    <xsl:text>4-5-1-5</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='lgCol'">
              <!--Large screens are 992px-1199px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@mdCol='2equal'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:when test="@mdCol='2'">
                    <xsl:text>4-5-1-5</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='xlCol'">
              <!--Extra large screens are 1200px-1399px-->
              <xsl:attribute name="{name()}">
                <xsl:text>4-5-1-5</xsl:text>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='xxlCol'">
              <!--Extra large screens are 1200px-1399px-->
              <xsl:attribute name="{name()}">
                <xsl:text>4-5-1-5</xsl:text>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="{name()}">
              <xsl:value-of select="." />
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>

  <!-- 75 25 -->
  <xsl:template match="Content[@moduleType='2columns7525']" mode="writeNodes">
    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:choose>
          <xsl:when test="name()='moduleType'">
            <xsl:attribute name="{name()}">
              <xsl:text>MultiColumn</xsl:text>
            </xsl:attribute>
            <xsl:attribute name="colType">
              <xsl:text>false</xsl:text>
            </xsl:attribute>
            <xsl:if test="name()='xsCol'">
              <!--Extra small means screens under 576px wide-->
              <xsl:attribute name="{name()}">

              </xsl:attribute>
            </xsl:if>

            <xsl:if test="name()='smCol'">
              <!--Small screens are 576px-767px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@xsCol='2'">
                    <xsl:text>3-4-1-4</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>

            <xsl:if test="name()='mdCol'">
              <!--Medium screens are 768px-991px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@smCol='2equal'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:when test="@smCol='2'">
                    <xsl:text>3-4-1-4</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='lgCol'">
              <!--Large screens are 992px-1199px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@mdCol='2equal'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:when test="@mdCol='2'">
                    <xsl:text>3-4-1-4</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='xlCol'">
              <!--Extra large screens are 1200px-1399px-->
              <xsl:attribute name="{name()}">
                <xsl:text>3-4-1-4</xsl:text>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="{name()}">
              <xsl:value-of select="." />
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>
  
  <!-- 66 33 -->
  <xsl:template match="Content[@moduleType='2columns6633']" mode="writeNodes">
    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:choose>
          <xsl:when test="name()='moduleType'">
            <xsl:attribute name="{name()}">
              <xsl:text>MultiColumn</xsl:text>
            </xsl:attribute>
            <xsl:attribute name="colType">
              <xsl:text>false</xsl:text>
            </xsl:attribute>
            <xsl:if test="name()='xsCol'">
              <!--Extra small means screens under 576px wide-->
              <xsl:attribute name="{name()}">

              </xsl:attribute>
            </xsl:if>
            
            <xsl:if test="name()='smCol'">
              <!--Small screens are 576px-767px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@xsCol='2'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>
            
            <xsl:if test="name()='mdCol'">
              <!--Medium screens are 768px-991px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@smCol='2equal'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:when test="@smCol='2'">
                    <xsl:text>2-3-1-3</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='lgCol'">
              <!--Large screens are 992px-1199px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@mdCol='2equal'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:when test="@mdCol='2'">
                    <xsl:text>2-3-1-3</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='xlCol'">
              <!--Extra large screens are 1200px-1399px-->
              <xsl:attribute name="{name()}">
                <xsl:text>2-3-1-3</xsl:text>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="{name()}">
              <xsl:value-of select="." />
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>

  <!-- 60 40 ** inc. XXL to not default to 1 -->
  <xsl:template match="Content[@moduleType='2columns6040']" mode="writeNodes">
    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:choose>
          <xsl:when test="name()='moduleType'">
            <xsl:attribute name="{name()}">
              <xsl:text>MultiColumn</xsl:text>
            </xsl:attribute>
            <xsl:attribute name="colType">
              <xsl:text>false</xsl:text>
            </xsl:attribute>
            <xsl:if test="name()='xsCol'">
              <!--Extra small means screens under 576px wide-->
              <xsl:attribute name="{name()}">

              </xsl:attribute>
            </xsl:if>

            <xsl:if test="name()='smCol'">
              <!--Small screens are 576px-767px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@xsCol='2'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>

            <xsl:if test="name()='mdCol'">
              <!--Medium screens are 768px-991px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@smCol='2equal'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:when test="@smCol='2'">
                    <xsl:text>3-5-2-5</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='lgCol'">
              <!--Large screens are 992px-1199px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@mdCol='2equal'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:when test="@mdCol='2'">
                    <xsl:text>3-5-2-5</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='xlCol'">
              <!--Extra large screens are 1200px-1399px-->
              <xsl:attribute name="{name()}">
                <xsl:text>3-5-2-5</xsl:text>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='xxlCol'">
              <!--Extra large screens are 1200px-1399px-->
              <xsl:attribute name="{name()}">
                <xsl:text>3-5-2-5</xsl:text>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="{name()}">
              <xsl:value-of select="." />
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>


  <!-- 16 83 -->
  <xsl:template match="Content[@moduleType='2columns1683']" mode="writeNodes">
    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:choose>
          <xsl:when test="name()='moduleType'">
            <xsl:attribute name="{name()}">
              <xsl:text>MultiColumn</xsl:text>
            </xsl:attribute>
            <xsl:attribute name="colType">
              <xsl:text>false</xsl:text>
            </xsl:attribute>
            <xsl:if test="name()='xsCol'">
              <!--Extra small means screens under 576px wide-->
              <xsl:attribute name="{name()}">

              </xsl:attribute>
            </xsl:if>

            <xsl:if test="name()='smCol'">
              <!--Small screens are 576px-767px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@xsCol='2'">
                    <xsl:text>1-6-5-6</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>

            <xsl:if test="name()='mdCol'">
              <!--Medium screens are 768px-991px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@smCol='2equal'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:when test="@smCol='2'">
                    <xsl:text>1-6-5-6</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='lgCol'">
              <!--Large screens are 992px-1199px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@mdCol='2equal'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:when test="@mdCol='2'">
                    <xsl:text>1-6-5-6</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='xlCol'">
              <!--Extra large screens are 1200px-1399px-->
              <xsl:attribute name="{name()}">
                <xsl:text>1-6-5-6</xsl:text>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="{name()}">
              <xsl:value-of select="." />
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>

  <!-- 20 80 ** inc. XXL to not default to 1 -->
  <xsl:template match="Content[@moduleType='2columns2080']" mode="writeNodes">
    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:choose>
          <xsl:when test="name()='moduleType'">
            <xsl:attribute name="{name()}">
              <xsl:text>MultiColumn</xsl:text>
            </xsl:attribute>
            <xsl:attribute name="colType">
              <xsl:text>false</xsl:text>
            </xsl:attribute>
            <xsl:if test="name()='xsCol'">
              <!--Extra small means screens under 576px wide-->
              <xsl:attribute name="{name()}">

              </xsl:attribute>
            </xsl:if>

            <xsl:if test="name()='smCol'">
              <!--Small screens are 576px-767px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@xsCol='2'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>

            <xsl:if test="name()='mdCol'">
              <!--Medium screens are 768px-991px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@smCol='2equal'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:when test="@smCol='2'">
                    <xsl:text>1-5-4-5</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='lgCol'">
              <!--Large screens are 992px-1199px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@mdCol='2equal'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:when test="@mdCol='2'">
                    <xsl:text>1-5-4-5</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='xlCol'">
              <!--Extra large screens are 1200px-1399px-->
              <xsl:attribute name="{name()}">
                <xsl:text>1-5-4-5</xsl:text>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='xxlCol'">
              <!--Extra large screens are 1200px-1399px-->
              <xsl:attribute name="{name()}">
                <xsl:text>1-5-4-5</xsl:text>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="{name()}">
              <xsl:value-of select="." />
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>

  <!-- 25 75 -->
  <xsl:template match="Content[@moduleType='2columns2575']" mode="writeNodes">
    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:choose>
          <xsl:when test="name()='moduleType'">
            <xsl:attribute name="{name()}">
              <xsl:text>MultiColumn</xsl:text>
            </xsl:attribute>
            <xsl:attribute name="colType">
              <xsl:text>false</xsl:text>
            </xsl:attribute>
            <xsl:if test="name()='xsCol'">
              <!--Extra small means screens under 576px wide-->
              <xsl:attribute name="{name()}">

              </xsl:attribute>
            </xsl:if>

            <xsl:if test="name()='smCol'">
              <!--Small screens are 576px-767px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@xsCol='2'">
                    <xsl:text>1-4-3-4</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>

            <xsl:if test="name()='mdCol'">
              <!--Medium screens are 768px-991px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@smCol='2equal'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:when test="@smCol='2'">
                    <xsl:text>1-4-3-4</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='lgCol'">
              <!--Large screens are 992px-1199px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@mdCol='2equal'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:when test="@mdCol='2'">
                    <xsl:text>1-4-3-4</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='xlCol'">
              <!--Extra large screens are 1200px-1399px-->
              <xsl:attribute name="{name()}">
                <xsl:text>1-4-3-4</xsl:text>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="{name()}">
              <xsl:value-of select="." />
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>

  <!-- 33 66 -->
  <xsl:template match="Content[@moduleType='2columns3366']" mode="writeNodes">
    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:choose>
          <xsl:when test="name()='moduleType'">
            <xsl:attribute name="{name()}">
              <xsl:text>MultiColumn</xsl:text>
            </xsl:attribute>
            <xsl:attribute name="colType">
              <xsl:text>false</xsl:text>
            </xsl:attribute>
            <xsl:if test="name()='xsCol'">
              <!--Extra small means screens under 576px wide-->
              <xsl:attribute name="{name()}">

              </xsl:attribute>
            </xsl:if>

            <xsl:if test="name()='smCol'">
              <!--Small screens are 576px-767px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@xsCol='2'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>

            <xsl:if test="name()='mdCol'">
              <!--Medium screens are 768px-991px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@smCol='2equal'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:when test="@smCol='2'">
                    <xsl:text>1-3-2-3</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='lgCol'">
              <!--Large screens are 992px-1199px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@mdCol='2equal'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:when test="@mdCol='2'">
                    <xsl:text>1-3-2-3</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='xlCol'">
              <!--Extra large screens are 1200px-1399px-->
              <xsl:attribute name="{name()}">
                <xsl:text>1-3-2-3</xsl:text>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="{name()}">
              <xsl:value-of select="." />
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>

  <!-- 40 60 ** inc. XXL to not default to 1 -->
  <xsl:template match="Content[@moduleType='2columns4060']" mode="writeNodes">
    <xsl:element name="{name()}">
      <!-- process attributes -->
      <xsl:for-each select="@*">
        <!-- remove attribute prefix (if any) -->
        <xsl:choose>
          <xsl:when test="name()='moduleType'">
            <xsl:attribute name="{name()}">
              <xsl:text>MultiColumn</xsl:text>
            </xsl:attribute>
            <xsl:attribute name="colType">
              <xsl:text>false</xsl:text>
            </xsl:attribute>
            <xsl:if test="name()='xsCol'">
              <!--Extra small means screens under 576px wide-->
              <xsl:attribute name="{name()}">

              </xsl:attribute>
            </xsl:if>

            <xsl:if test="name()='smCol'">
              <!--Small screens are 576px-767px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@xsCol='2'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>

            <xsl:if test="name()='mdCol'">
              <!--Medium screens are 768px-991px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@smCol='2equal'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:when test="@smCol='2'">
                    <xsl:text>2-5-3-5</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='lgCol'">
              <!--Large screens are 992px-1199px-->
              <xsl:attribute name="{name()}">
                <xsl:choose>
                  <xsl:when test="@mdCol='2equal'">
                    <xsl:text>1-2-1-2</xsl:text>
                  </xsl:when>
                  <xsl:when test="@mdCol='2'">
                    <xsl:text>2-5-3-5</xsl:text>
                  </xsl:when>
                  <xsl:otherwise>
                    <xsl:text>1</xsl:text>
                  </xsl:otherwise>
                </xsl:choose>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='xlCol'">
              <!--Extra large screens are 1200px-1399px-->
              <xsl:attribute name="{name()}">
                <xsl:text>2-5-3-5</xsl:text>
              </xsl:attribute>
            </xsl:if>
            <xsl:if test="name()='xxlCol'">
              <!--Extra large screens are 1200px-1399px-->
              <xsl:attribute name="{name()}">
                <xsl:text>2-5-3-5</xsl:text>
              </xsl:attribute>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="{name()}">
              <xsl:value-of select="." />
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>



  <xsl:template match="Content" mode="writeNodes">
    <xsl:element name="{name()}">
      <xsl:for-each select="@*">
        <xsl:choose>
          <xsl:when test="name()='moduleType' and (.='3column')">
            <xsl:attribute name="{name()}">
              <xsl:text>3Column</xsl:text>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="name()='moduleType' and (.='4column')">
            <xsl:attribute name="{name()}">
              <xsl:text>4Column</xsl:text>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="name()='moduleType' and (.='5column')">
            <xsl:attribute name="{name()}">
              <xsl:text>5Column</xsl:text>
            </xsl:attribute>
          </xsl:when>
          <xsl:when test="name()='moduleType' and (.='6column')">
            <xsl:attribute name="{name()}">
              <xsl:text>6Column</xsl:text>
            </xsl:attribute>
          </xsl:when>
          <xsl:otherwise>
            <xsl:attribute name="{local-name()}">
              <xsl:value-of select="." />
            </xsl:attribute>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
      <xsl:apply-templates mode="writeNodes"/>
    </xsl:element>
  </xsl:template>

</xsl:stylesheet>
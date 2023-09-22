<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <!--   ################   Poll   ###############   -->
  <!-- Poll Module -->
  <xsl:template match="Content[@type='Module' and @moduleType='PollList']" mode="displayBrief">
    <!-- Set Variables -->
    <xsl:variable name="contentType" select="@contentType" />
    <xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
    <xsl:variable name="startPos" select="number(concat('0',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
    <xsl:variable name="contentList">
      <xsl:apply-templates select="." mode="getContent">
        <xsl:with-param name="contentType" select="$contentType" />
        <xsl:with-param name="startPos" select="$startPos" />
      </xsl:apply-templates>
    </xsl:variable>
    <xsl:variable name="totalCount">
      <xsl:choose>
        <xsl:when test="@display='related'">
          <xsl:value-of select="count(Content[@type=$contentType])"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="count(/Page/Contents/Content[@type=$contentType])"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- Output Module -->
    <div class="PollList">
      <div class="cols{@cols}">
        <!-- If Stepper, display Stepper -->
        <xsl:if test="@stepCount != '0'">
          <xsl:apply-templates select="/" mode="genericStepper">
            <xsl:with-param name="articleList" select="$contentList"/>
            <xsl:with-param name="noPerPage" select="@stepCount"/>
            <xsl:with-param name="startPos" select="$startPos"/>
            <xsl:with-param name="queryStringParam" select="$queryStringParam"/>
            <xsl:with-param name="totalCount" select="$totalCount"/>
          </xsl:apply-templates>
        </xsl:if>
        <xsl:apply-templates select="ms:node-set($contentList)/*" mode="displayBrief">
          <xsl:with-param name="sortBy" select="@sort"/>
        </xsl:apply-templates>
      </div>
    </div>
  </xsl:template>

  <!-- Display Poll -->
  <xsl:template match="Content[@type='Poll']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <div class="clearfix listItem poll">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'clearfix listItem poll'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <h3 class="title">
          <xsl:value-of select="Title/node()"/>
        </h3>
        <xsl:if test="Description/node()!=''">
          <h4>
            <xsl:value-of select="Description/node()"/>
          </h4>
        </xsl:if>
        <xsl:if test="Images/img/@src and Images/img/@src!=''">
          <xsl:apply-templates select="Images/img" mode="cleanXhtml"/>
        </xsl:if>
        <xsl:apply-templates select="." mode="pollForm"/>
      </div>
    </div>
  </xsl:template>

  <!-- !!!!!!  Not Voted Yet !!!!!! -->
  <xsl:template match="Content[Status/@canVote='true']" mode="pollForm">
    <!-- GET VOTED VALUE - EITHER FROM FORM REQUEST VALUES, OR RESULTS VALUE -->
    <xsl:variable name="votedFor">
      <xsl:variable name="votedName">
        <xsl:text>polloption-</xsl:text>
        <xsl:value-of select="@id"/>
      </xsl:variable>
      <xsl:choose>
        <xsl:when test="/Page/Request/Form/Item[@name=$votedName]">
          <xsl:value-of select="/Page/Request/Form/Item[@name=$votedName]/node()"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="Results/@votedFor"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <!-- GET SUBMITTED EMAIL ADDRESS -->
    <xsl:variable name="submittedEmail">
      <xsl:value-of select="/Page/Request/Form/Item[@name='poll-email']/node()"/>
    </xsl:variable>
    <!-- SUM NUMBER OF VOTES -->
    <xsl:variable name="total-votes">
      <xsl:choose>
        <!-- IF VOTES {Calculate Total} ELSE {0} -->
        <xsl:when test="Results/PollResult/@votes">
          <xsl:value-of select="sum(Results/PollResult/@votes)"/>
        </xsl:when>
        <xsl:otherwise>0</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <form id="pollform-{@id}" name="pollform-{@id}" action="" method="post" class="pollform">
      <xsl:apply-templates select="PollItems/Content" mode="pollOption">
        <xsl:with-param name="pollId" select="@id"/>
        <xsl:with-param name="votedFor" select="$votedFor"/>
      </xsl:apply-templates>
      <!-- IF EMAIL RESTRICTION INDENTIFIER -->
      <div class="pollsubmission">
        <xsl:if test="contains(Restrictions/Identifiers/node(),'email')">
          <xsl:if test="not(/Page/User) and $submittedEmail=''">
            <label for="poll-email-{@id}">
              <!--Email-->
              <xsl:call-template name="term2050" />
              <span class="req">*</span>
            </label>
          </xsl:if>
          <input name="poll-email" id="poll-email-{@id}">
            <xsl:attribute name="type">
              <xsl:choose>
                <xsl:when test="/Page/User or $submittedEmail!=''">hidden</xsl:when>
                <xsl:otherwise>text</xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
            <xsl:attribute name="value">
              <xsl:choose>
                <xsl:when test="/Page/User">
                  <xsl:value-of select="/Page/User/Email/node()"/>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:value-of select="$submittedEmail"/>
                </xsl:otherwise>
              </xsl:choose>
            </xsl:attribute>
            <xsl:attribute name="class">
              <xsl:text>textbox short email</xsl:text>
              <xsl:if test="not(/Page/User)">
                <xsl:text> required</xsl:text>
              </xsl:if>
            </xsl:attribute>
          </input>
        </xsl:if>
        <xsl:if test="Status/@validationError!=''">
          <span class="alert">
            <xsl:value-of select="Status/@validationError"/>
          </span>
        </xsl:if>
        <button type="submit" name="pollsubmit-{@id}" value="Submit Vote" class="btn btn-primary"  onclick="disableButton(this);">Submit Vote</button>
      </div>
    </form>
    <!-- IN ADMIN SHOW RESULTS -->
    <xsl:if test="/Page/@adminMode and not(@id=/Page/Request/QueryString/Item[@name='showPollResults']/node())">
      <xsl:variable name="resultHref">
        <xsl:apply-templates select="//MenuItem[@id=/Page/@id]" mode="getHref"/>
        <xsl:text>&amp;showPollResults=</xsl:text>
        <xsl:value-of select="@id"/>
      </xsl:variable>
      <p>
        <a href="{$resultHref}" title="see results">
          <!--Show Results-->
          <xsl:call-template name="term2051" />
          <xsl:text> &#187;</xsl:text>
        </a>
      </p>
    </xsl:if>
  </xsl:template>

  <!-- Show Poll Results -->
  <xsl:template match="Content[Status/@canVote='true' and @id=/Page/Request/QueryString/Item[@name='showPollResults']/node()]" mode="pollForm">
    <!-- Total Number of Votes -->
    <xsl:variable name="total-votes">
      <xsl:choose>
        <!-- IF VOTES {Calculate Total} ELSE {0} -->
        <xsl:when test="Results/PollResult/@votes">
          <xsl:value-of select="sum(Results/PollResult/@votes)"/>
        </xsl:when>
        <xsl:otherwise>0</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:apply-templates select="." mode="pollResults">
      <xsl:with-param name="total-votes" select="$total-votes"/>
    </xsl:apply-templates>
  </xsl:template>

  <!-- Can No Longer Vote - Inc Just Voted -->
  <xsl:template match="Content[Status/@canVote='false']" mode="pollForm">
    <xsl:variable name="votedFor">
      <xsl:value-of select="Results/@votedFor"/>
    </xsl:variable>
    <xsl:variable name="total-votes">
      <xsl:value-of select="sum(Results/PollResult/@votes)"/>
    </xsl:variable>
    <xsl:variable name="votedName">
      <xsl:text>polloption-</xsl:text>
      <xsl:value-of select="@id"/>
    </xsl:variable>
    <div class="pollresults">
      <xsl:choose>
        <!-- IF Poll is not yet Open -->
        <xsl:when test="dOpenDate/node() and number(translate(dOpenDate,'-','')) &gt; number(translate($today,'-',''))">
          <span class="hint">
            <!--This poll opens for voting at the begining of-->
            <xsl:call-template name="term2052" />
            <xsl:text> </xsl:text>
            <xsl:call-template name="DisplayDate">
              <xsl:with-param name="date" select="dOpenDate/node()"/>
            </xsl:call-template>
          </span>
        </xsl:when>
        <!-- IF Private Voting -->
        <xsl:when test="Resulting/@public='false'">
          <!--The results to this poll are private.-->
          <xsl:call-template name="term2053" />
        </xsl:when>
        <!-- IF closed results AND close date is greater than today-->
        <xsl:when test="(Resulting/@display='closed') and not( format-number(number(translate(dCloseDate/node(),'-','')),'0') &lt; number(translate($today,'-','')) )">
          <span class="hint">
            <!--The results to this poll will be revealed-->
            <xsl:call-template name="term2054" />
            <xsl:choose>
              <xsl:when test="dCloseDate!=''">
                <xsl:text>&#160;</xsl:text>
                <!--at the end of-->
                <xsl:call-template name="term2055" />
                <xsl:text>&#160;</xsl:text>
                <xsl:call-template name="DisplayDate">
                  <xsl:with-param name="date" select="dCloseDate/node()"/>
                </xsl:call-template>
              </xsl:when>
              <xsl:otherwise>
                <xsl:text>&#160;</xsl:text>
                <!--when the poll closes.-->
                <xsl:call-template name="term2056" />
              </xsl:otherwise>
            </xsl:choose>
          </span>
        </xsl:when>
        <!-- Show results -->
        <xsl:otherwise>
          <xsl:apply-templates select="." mode="pollResults">
            <xsl:with-param name="total-votes" select="$total-votes"/>
          </xsl:apply-templates>
        </xsl:otherwise>
      </xsl:choose>
    </div>
    <xsl:if test="Status/@blockReason!=''">
      <!--<xsl:if test="Status/@blockReason!='' and /Page/Request/Form/Item[@name=$votedName]">-->
      <xsl:apply-templates select="." mode="pollBlockReason"/>
    </xsl:if>
  </xsl:template>

  <!-- Poll Blocked Reason -->
  <xsl:template match="Content" mode="pollBlockReason">
    <span class="hint">
      <xsl:choose>
        <xsl:when test="Status/@blockReason='LogFound' or Status/@blockReason='CookieFound'">
          <!--You have already voted on this poll.-->
          <xsl:call-template name="term2057" />
        </xsl:when>
        <xsl:when test="Status/@blockReason='RegisteredUsersOnly'">
          <!--This poll is only available to registered users-->
          <xsl:call-template name="term2058" />
        </xsl:when>
        <xsl:when test="Status/@blockReason='JustVoted'">
          <xsl:choose>
            <xsl:when test="Resulting/VoteConfirmationMessage/node()">
              <xsl:value-of select="Resulting/VoteConfirmationMessage/node()"/>
            </xsl:when>
            <xsl:otherwise>
              <!--Thank You, your vote has been counted.-->
              <xsl:call-template name="term2059" />
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
      </xsl:choose>
    </span>
  </xsl:template>

  <!-- Poll Option -->
  <xsl:template match="Content" mode="pollOption">
    <xsl:param name="pollId"/>
    <xsl:param name="votedFor"/>
    <div class="polloption">
      <input type="radio" id="polloption-{@id}" name="polloption-{$pollId}" value="{@id}" class="radiocheckbox pollradiocheckbox">
        <xsl:if test="@id=$votedFor">
          <xsl:attribute name="checked">checked</xsl:attribute>
        </xsl:if>
      </input>
      <label for="polloption-{@id}">
        <xsl:value-of select="@name"/>
      </label>
    </div>
  </xsl:template>

  <!-- Poll Results -->
  <xsl:template match="Content" mode="pollResults">
    <xsl:param name="total-votes"/>
    <xsl:apply-templates select="PollItems/Content" mode="pollOptionResult">
      <xsl:with-param name="total-votes" select="$total-votes"/>
    </xsl:apply-templates>
    <p class="polltotalvotes">
      <!--Total Votes-->
      <xsl:call-template name="term2060" />
      <xsl:text>: </xsl:text>
      <xsl:value-of select="$total-votes"/>
    </p>
  </xsl:template>

  <!-- Poll Option Result -->
  <xsl:template match="Content[@type='PollOption']" mode="pollOptionResult">
    <xsl:param name="total-votes"/>
    <xsl:variable name="optionId" select="@id"/>
    <xsl:variable name="votes">
      <xsl:choose>
        <xsl:when test="ancestor::Content[@type='Poll']/Results/PollResult/@entryId=$optionId">
          <xsl:value-of select="ancestor::Content[@type='Poll']/Results/PollResult[@entryId=$optionId]/@votes"/>
        </xsl:when>
        <xsl:otherwise>0</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="percentageVote">
      <xsl:choose>
        <xsl:when test="$total-votes!=0">
          <xsl:value-of select="format-number(($votes div $total-votes)*100 ,'0')"/>
        </xsl:when>
        <xsl:otherwise>0</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div class="optionResult">
      <p>
        <xsl:value-of select="Title/node()"/> - <xsl:value-of select="$percentageVote"/>%
      </p>
      <div class="bg-primary pollBar">
        <xsl:attribute name="style">
          <xsl:text>width:</xsl:text>
          <xsl:value-of select="$percentageVote"/>
          <xsl:text>%;</xsl:text>
        </xsl:attribute>
        <xsl:text>&#160;</xsl:text>
      </div>
    </div>
  </xsl:template>
  
</xsl:stylesheet>
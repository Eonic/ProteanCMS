<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">

  <xsl:template match="Content[@moduleType='SocialLinks']" mode="displayBrief">
    <div class="moduleSocialLinks align-{@align}">
      <!--<xsl:choose>
				<xsl:when test="@blank='true'">
					<xsl:apply-templates select="." mode="socialLinksBlank">
						<xsl:with-param name="iconSet" select="@iconSet"/>
						<xsl:with-param name="myName" select="@myName"/>
            <xsl:with-param name="align" select="@align"/>
            <xsl:with-param name="icon-size" select="@icon-size"/>
					</xsl:apply-templates>
				</xsl:when>
				<xsl:otherwise>
					<xsl:apply-templates select="." mode="socialLinks">
						<xsl:with-param name="iconSet" select="@iconSet"/>
						<xsl:with-param name="myName" select="@myName"/>
            <xsl:with-param name="align" select="@align"/>
            <xsl:with-param name="icon-size" select="@icon-size"/>
            <xsl:with-param name="blank" select="@blank"/>
					</xsl:apply-templates>
				</xsl:otherwise>
			</xsl:choose>-->
      <xsl:apply-templates select="." mode="socialLinks">
        <xsl:with-param name="iconSet" select="@iconSet"/>
        <xsl:with-param name="myName" select="@myName"/>
        <xsl:with-param name="align" select="@align"/>
        <xsl:with-param name="icon-size" select="@icon-size"/>
        <xsl:with-param name="blank" select="@blank"/>
        <xsl:with-param name="spacing" select="@spacing"/>
        <xsl:with-param name="spacing-unit" select="@spacing-unit"/>
        <xsl:with-param name="layout" select="@layout"/>
        <xsl:with-param name="fb-order" select="@fb-order"/>
        <xsl:with-param name="x-order" select="@x-order"/>
        <xsl:with-param name="li-order" select="@li-order"/>
        <xsl:with-param name="p-order" select="@p-order"/>
        <xsl:with-param name="yt-order" select="@yt-order"/>
        <xsl:with-param name="i-order" select="@i-order"/>
        <xsl:with-param name="bs-order" select="@bs-order"/>
      </xsl:apply-templates>
    </div>
  </xsl:template>

  <!-- module -->
  <xsl:template match="Content | ContactPoint" mode="socialLinksBlank">
    <xsl:param name="myName"/>
    <xsl:param name="iconSet"/>
    <xsl:param name="align"/>
    <xsl:param name="icon-size"/>
    <div class="socialLinks clearfix iconset-{$iconSet} align-{$align}">
      <xsl:if test="@facebookURL!=''">
        <a href="{@facebookURL}" aria-label="{$myName} on Facebook" class="social-id-fb">
          <xsl:attribute name="target">_blank</xsl:attribute>
          <i>
            <xsl:attribute name="class">
              <xsl:text>fa-brands fa-facebook </xsl:text>
              <xsl:value-of select="$icon-size"/>
            </xsl:attribute>
            <span class="visually-hidden">
              <xsl:value-of select="$myName"/> on Facebook
            </span>
          </i>
        </a>
      </xsl:if>
      <xsl:if test="@twitterURL!=''">
        <a href="{@twitterURL}" target="_blank" aria-label="{$myName} on Twitter" class="social-id-tw">
          <i>
            <xsl:attribute name="class">
              <xsl:text>fa-brands fa-x-twitter </xsl:text>
              <xsl:value-of select="$icon-size"/>
            </xsl:attribute>
            <span class="visually-hidden">
              <xsl:value-of select="$myName"/> on X
            </span>
          </i>
        </a>
      </xsl:if>
      <xsl:if test="@linkedInURL!=''">
        <a href="{@linkedInURL}" target="_blank" aria-label="{$myName} on LinkedIn" class="social-id-li">
          <i>
            <xsl:attribute name="class">
              <xsl:text>fa-brands fa-linkedin </xsl:text>
              <xsl:value-of select="$icon-size"/>
            </xsl:attribute>
            <span class="visually-hidden">
              <xsl:value-of select="$myName"/> on LinkedIn
            </span>
          </i>
        </a>
      </xsl:if>
      <xsl:if test="@pinterestURL!=''">
        <a href="{@pinterestURL}" target="_blank" aria-label="{$myName} on Pinterest" class="social-id-pi">
          <i>
            <xsl:attribute name="class">
              <xsl:text>fa-brands fa-pinterest </xsl:text>
              <xsl:value-of select="$icon-size"/>
            </xsl:attribute>
            <span class="visually-hidden">
              <xsl:value-of select="$myName"/> on Pintrest
            </span>
          </i>
        </a>
      </xsl:if>
      <xsl:if test="@youtubeURL!=''">
        <a href="{@youtubeURL}" target="_blank" aria-label="{$myName} on Youtube" class="social-id-yt">
          <i>
            <xsl:attribute name="class">
              <xsl:text>fa-brands fa-youtube </xsl:text>
              <xsl:value-of select="$icon-size"/>
            </xsl:attribute>
            <span class="visually-hidden">
              <xsl:value-of select="$myName"/> on Youtube
            </span>
          </i>
        </a>
      </xsl:if>
      <xsl:if test="@instagramURL!=''">
        <a href="{@instagramURL}" target="_blank" aria-label="{$myName} on Instagram" class="social-id-ig">
          <i>
            <xsl:attribute name="class">
              <xsl:text>fa-brands fa-instagram </xsl:text>
              <xsl:value-of select="$icon-size"/>
            </xsl:attribute>
            <span class="visually-hidden">
              <xsl:value-of select="$myName"/> on Instagram
            </span>
          </i>
        </a>
      </xsl:if>
      <xsl:if test="@blueSkyURL!=''">
        <a href="{@blueSkyURL}" target="_blank" aria-label="{$myName} on Bluesky" class="social-id-bs">
          <i>
            <xsl:attribute name="class">
              <xsl:text>fa-brands  fa-bluesky </xsl:text>
              <xsl:value-of select="$icon-size"/>
            </xsl:attribute>
            <span class="visually-hidden">
              <xsl:value-of select="$myName"/> on BlueSky
            </span>
          </i>
        </a>
      </xsl:if>
    </div>
  </xsl:template>
	
  <!-- module -->
  <xsl:template match="Content | ContactPoint" mode="socialLinks">
    <xsl:param name="myName"/>
    <xsl:param name="iconSet"/>
    <xsl:param name="align"/>
    <xsl:param name="icon-size"/>
    <xsl:param name="blank"/>
    <xsl:param name="spacing"/>
    <xsl:param name="spacing-unit"/>
    <xsl:param name="layout"/>
    <xsl:param name="fb-order"/>
    <xsl:param name="x-order"/>
    <xsl:param name="li-order"/>
    <xsl:param name="p-order"/>
    <xsl:param name="yt-order"/>
    <xsl:param name="i-order"/>
    <xsl:param name="bs-order"/>
    <xsl:variable name="half-spacing" select="$spacing div 2" />
    <xsl:variable name="order-class">
      <xsl:if test="($fb-order and $fb-order!='') or ($x-order and $x-order!='') or ($li-order and $li-order!='') or ($p-order and $p-order!='') or ($yt-order and $yt-order!='') or ($i-order and $i-order!='') or ($bs-order and $bs-order!='')">
        <xsl:text> ordering</xsl:text>
      </xsl:if>
    </xsl:variable>

	  <xsl:variable name="icon-class">
		  <xsl:choose>
			  <xsl:when test="$iconSet='icons-square'">fa-solid fa-square-full</xsl:when>
			  <xsl:when test="$iconSet='icons-circle'">fa-solid fa-circle</xsl:when>
			  <xsl:when test="$iconSet='icons-circle-thin'">fa-thin fa-circle</xsl:when>
			  <xsl:otherwise></xsl:otherwise>
		  </xsl:choose>
		  
	  </xsl:variable>
	  
    <div class="socialLinks clearfix iconset-{$iconSet} align-{$align} layout-{$layout} {$order-class}" style="margin-left:-{$half-spacing}{$spacing-unit};margin-right:-{$half-spacing}{$spacing-unit}">
  
      <xsl:if test="@facebookURL!=''">
        <a href="{@facebookURL}" aria-label="{$myName} on Facebook" class="social-id-fb" style="padding-left:{$half-spacing}{$spacing-unit};padding-right:{$half-spacing}{$spacing-unit};padding-bottom:{$spacing}{$spacing-unit};order:{$fb-order}">
          <xsl:if test="$blank='true'">
            <xsl:attribute name="target">_blank</xsl:attribute>
          </xsl:if>
			<span class="fa-stack fa-lg">
				<i class="{$icon-class} fa-stack-2x">&#160;</i>
				<i class="fa-brands fa-facebook-f fa-stack-1x fa-inverse">&#160;</i>
				<span class="visually-hidden">
					<xsl:value-of select="$myName"/> on Facebook
				</span>
			</span>         
          <xsl:if test="$layout='vertical'">
            <xsl:text>Facebook</xsl:text>
          </xsl:if>
        </a>
      </xsl:if>
      <xsl:if test="@twitterURL!=''">
        <a href="{@twitterURL}" aria-label="{$myName} on Twitter" class="social-id-tw" style="padding-left:{$half-spacing}{$spacing-unit};padding-right:{$half-spacing}{$spacing-unit};padding-bottom:{$spacing}{$spacing-unit};order:{$x-order}">
          <xsl:if test="$blank='true'">
            <xsl:attribute name="target">_blank</xsl:attribute>
          </xsl:if>
			<span class="fa-stack fa-lg">
				<i class="{$icon-class} fa-stack-2x">&#160;</i>
				<i class="fa-brands fa-x-twitter fa-stack-1x fa-inverse">&#160;</i>
				<span class="visually-hidden">
					<xsl:value-of select="$myName"/> on X
				</span>
			</span>
          <xsl:if test="$layout='vertical'">
            <xsl:text>X</xsl:text>
          </xsl:if>
        </a>
      </xsl:if>
      <xsl:if test="@linkedInURL!=''">
        <a href="{@linkedInURL}" aria-label="{$myName} on LinkedIn" class="social-id-li" style="padding-left:{$half-spacing}{$spacing-unit};padding-right:{$half-spacing}{$spacing-unit};padding-bottom:{$spacing}{$spacing-unit};order:{$li-order}">
          <xsl:if test="$blank='true'">
            <xsl:attribute name="target">_blank</xsl:attribute>
          </xsl:if>
			<span class="fa-stack fa-lg">
				<i class="{$icon-class} fa-stack-2x">&#160;</i>
				<i class="fa-brands fa-linkedin-in fa-stack-1x fa-inverse">&#160;</i>
				<span class="visually-hidden">
                    <xsl:value-of select="$myName"/> on LinkedIn
                </span>
			</span>
          <xsl:if test="$layout='vertical'">
            <xsl:text>LinkedIn</xsl:text>
          </xsl:if>
        </a>
      </xsl:if>
      <xsl:if test="@pinterestURL!=''">
        <a href="{@pinterestURL}" aria-label="{$myName} on Pinterest" class="social-id-pi" style="padding-left:{$half-spacing}{$spacing-unit};padding-right:{$half-spacing}{$spacing-unit};padding-bottom:{$spacing}{$spacing-unit};order:{$p-order}">
          <xsl:if test="$blank='true'">
            <xsl:attribute name="target">_blank</xsl:attribute>
          </xsl:if>
			<span class="fa-stack fa-lg">
				<i class="{$icon-class} fa-stack-2x">&#160;</i>
				<i class="fa-brands fa-pinterest-p fa-stack-1x fa-inverse">&#160;</i>
				<span class="visually-hidden">
					<xsl:value-of select="$myName"/> on Pinterest
				</span>
			</span>
          <xsl:if test="$layout='vertical'">
            <xsl:text>Pintrest</xsl:text>
          </xsl:if>
        </a>
      </xsl:if>
      <xsl:if test="@youtubeURL!=''">
        <a href="{@youtubeURL}" aria-label="{$myName} on Youtube" class="social-id-yt" style="padding-left:{$half-spacing}{$spacing-unit};padding-right:{$half-spacing}{$spacing-unit};padding-bottom:{$spacing}{$spacing-unit};order:{$yt-order}">
          <xsl:if test="$blank='true'">
            <xsl:attribute name="target">_blank</xsl:attribute>
          </xsl:if>
			<span class="fa-stack fa-lg">
				<i class="{$icon-class} fa-stack-2x">&#160;</i>
				<i class="fa-brands fa-youtube fa-stack-1x fa-inverse">&#160;</i>
				<span class="visually-hidden">
					<xsl:value-of select="$myName"/> on YouTube
				</span>
			</span>
          <xsl:if test="$layout='vertical'">
            <xsl:text>Youtube</xsl:text>
          </xsl:if>
        </a>
      </xsl:if>
      <xsl:if test="@instagramURL!=''">
        <a href="{@instagramURL}" aria-label="{$myName} on Instagram" class="social-id-ig" style="padding-left:{$half-spacing}{$spacing-unit};padding-right:{$half-spacing}{$spacing-unit};padding-bottom:{$spacing}{$spacing-unit};order:{$i-order}">
          <xsl:if test="$blank='true'">
            <xsl:attribute name="target">_blank</xsl:attribute>
          </xsl:if>
			<span class="fa-stack fa-lg">
				<i class="{$icon-class} fa-stack-2x">&#160;</i>
				<i class="fa-brands fa-instagram fa-stack-1x fa-inverse">&#160;</i>
				<span class="visually-hidden">
					<xsl:value-of select="$myName"/> on Instagram
				</span>
			</span>
          <xsl:if test="$layout='vertical'">
            <xsl:text>Instagram</xsl:text>
          </xsl:if>
        </a>
      </xsl:if>
      <xsl:if test="@spotifyURL!=''">
        <a href="{@spotifyURL}" aria-label="{$myName} on Spotify" class="social-id-isp" style="padding-left:{$half-spacing}{$spacing-unit};padding-right:{$half-spacing}{$spacing-unit};padding-bottom:{$spacing}{$spacing-unit};">
          <xsl:if test="$blank='true'">
            <xsl:attribute name="target">_blank</xsl:attribute>
          </xsl:if>
			<span class="fa-stack fa-lg">
				<i class="{$icon-class} fa-stack-2x">&#160;</i>
				<i class="fa-brands fa-spotify fa-stack-1x fa-inverse">&#160;</i>
				<span class="visually-hidden">
					<xsl:value-of select="$myName"/> on Spotify
				</span>
			</span>
          <xsl:if test="$layout='vertical'">
            <xsl:text>Spotify</xsl:text>
          </xsl:if>
        </a>
      </xsl:if>
      <xsl:if test="@blueSkyURL!=''">
        <a href="{@blueSkyURL}" aria-label="{$myName} on Bluesky" class="social-id-bs" style="padding-left:{$half-spacing}{$spacing-unit};padding-right:{$half-spacing}{$spacing-unit};padding-bottom:{$spacing}{$spacing-unit};order:{$bs-order}">
          <xsl:if test="$blank='true'">
            <xsl:attribute name="target">_blank</xsl:attribute>
          </xsl:if>
			<span class="fa-stack fa-lg">
				<i class="{$icon-class} fa-stack-2x">&#160;</i>
				<i class="fa-brands fa-bluesky fa-stack-1x fa-inverse">&#160;</i>
				<span class="visually-hidden">
					<xsl:value-of select="$myName"/> on BlueSky
				</span>
			</span>
          <xsl:if test="$layout='vertical'">
            <xsl:text>BlueSky</xsl:text>
          </xsl:if>
        </a>
      </xsl:if>
    </div>
  </xsl:template>

</xsl:stylesheet>
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
      <!--<xsl:choose>
				<xsl:when test="@uploadSprite!=''">
					<xsl:if test="@facebookURL!=''">
						<a href="{@facebookURL}" target="_blank" title="{$myName} on Facebook" id="social-id-fb" style="background-image:url({@uploadSprite})" class="social-sprite">
							&#160;
						</a>
					</xsl:if>
					<xsl:if test="@twitterURL!=''">
						<a href="{@twitterURL}" target="_blank" title="{$myName} on Twitter" id="social-id-tw" style="background-image:url({@uploadSprite});background-position:128px 0" class="social-sprite">
							&#160;
						</a>
					</xsl:if>
					<xsl:if test="@linkedInURL!=''">
						<a href="{@linkedInURL}" target="_blank" title="{$myName} on LinkedIn" id="social-id-li" style="background-image:url({@uploadSprite});background-position:96px 0" class="social-sprite">
							&#160;
						</a>
					</xsl:if>
					<xsl:if test="@googlePlusURL!=''">
						<a href="{@googlePlusURL}" target="_blank" title="{$myName} on Google+" id="social-id-gp" style="background-image:url({@uploadSprite});background-position:64px 0" class="social-sprite">
							&#160;
						</a>
					</xsl:if>
					<xsl:if test="@pinterestURL!=''">
						<a href="{@pinterestURL}" target="_blank" title="{$myName} on Pinterest" id="social-id-pi" style="background-image:url({@uploadSprite});background-position:32px 0" class="social-sprite">
							&#160;
						</a>
					</xsl:if>
					<xsl:if test="@youtubeURL!=''">
						<a href="{@youtubeURL}" target="_blank" title="{$myName} on You Tube" id="social-id-yt" style="background-image:url({@uploadSprite});background-position:160px 0" class="social-sprite">
							&#160;
						</a>
					</xsl:if>
					<xsl:if test="@instagramURL!=''">
						<a href="{@instagramURL}" target="_blank" title="{$myName} on Instagram" id="social-id-ig" style="background-image:url({@uploadSprite});background-position:192px 0" class="social-sprite">
							&#160;
						</a>
					</xsl:if>
				</xsl:when>
				<xsl:when test="$iconSet='icons'">
					<xsl:if test="@facebookURL!=''">
						<a href="{@facebookURL}" target="_blank" title="{$myName} on Facebook" class="social-id-fb">
							<i class="fab fa-2x fa-facebook">
								<span class="visually-hidden">
									<xsl:value-of select="$myName"/> on Facebook
								</span>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@twitterURL!=''">
						<a href="{@twitterURL}" target="_blank" title="{$myName} on Twitter" class="social-id-tw">

							<i class="fa-brands fa-2x fa-x-twitter">
								<span class="visually-hidden">
									<xsl:value-of select="$myName"/> on Twitter
								</span>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@linkedInURL!=''">
						<a href="{@linkedInURL}" target="_blank" title="{$myName} on LinkedIn" class="social-id-li">
							<i class="fab fa-2x fa-linkedin">
								<span class="visually-hidden">
									<xsl:value-of select="$myName"/> on LinkedIn
								</span>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@googlePlusURL!=''">
						<a href="{@googlePlusURL}" target="_blank" title="{$myName} on Google+" class="social-id-gp">
							<i class="fab fa-2x fa-google-plus">
								<span class="visually-hidden">
									<xsl:value-of select="$myName"/> on Google+
								</span>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@pinterestURL!=''">
						<a href="{@pinterestURL}" target="_blank" title="{$myName} on Pinterest" class="social-id-li">
							<i class="fab fa-2x fa-pinterest">
								<span class="visually-hidden">
									<xsl:value-of select="$myName"/> on Pintrest
								</span>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@youtubeURL!=''">
						<a href="{@youtubeURL}" target="_blank" title="{$myName} on Youtube" class="social-id-yt">
							<i class="fab fa-2x fa-youtube">
								<span class="visually-hidden">
									<xsl:value-of select="$myName"/> on Youtube
								</span>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@instagramURL!=''">
						<a href="{@instagramURL}" target="_blank" title="{$myName} on Instagram" class="social-id-ig">
							<i class="fab fa-2x fa-instagram">
								<span class="visually-hidden">
									<xsl:value-of select="$myName"/> on Instagram
								</span>
							</i>
						</a>
					</xsl:if>
				</xsl:when>
				<xsl:when test="$iconSet='icons-square'">
					<xsl:if test="@facebookURL!=''">
						<a href="{@facebookURL}" target="_blank" title="{$myName} on Facebook" class="social-id-fb">
							<i class="fab fa-3x fa-facebook-square">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@twitterURL!=''">
						<a href="{@twitterURL}" target="_blank" title="{$myName} on Twitter" class="social-id-tw">
							<i class="fa-brands fa-3x fa-x-twitter-square">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@linkedInURL!=''">
						<a href="{@linkedInURL}" target="_blank" title="{$myName} on LinkedIn" class="social-id-li">
							<i class="fab fa-3x fa-linkedin-square">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@googlePlusURL!=''">
						<a href="{@googlePlusURL}" target="_blank" title="{$myName} on Google+" class="social-id-gp">
							<i class="fab fa-3x fa-google-plus-square">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@pinterestURL!=''">
						<a href="{@pinterestURL}" target="_blank" title="{$myName} on Pinterest" class="social-id-pi">
							<i class="fab fa-3x fa-pinterest-square">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@youtubeURL!=''">
						<a href="{@youtubeURL}" target="_blank" title="{$myName} on Youtube" class="social-id-yt">
							<i class="fab fa-3x fa-youtube-square">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@instagramURL!=''">
						<a href="{@instagramURL}" target="_blank" title="{$myName} on Instagram" class="social-id-ig">
							<i class="fab fa-3x fa-instagram">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
				</xsl:when>
				<xsl:when test="$iconSet='icons-circle'">
					<xsl:if test="@facebookURL!=''">
						<a href="{@facebookURL}" target="_blank" title="{$myName} on Facebook" class="social-id-fb">
							<span class="fa-stack fa-lg">
								<i class="fab fa-circle fa-stack-2x">
									<xsl:text> </xsl:text>
								</i>
								<i class="fab fa-facebook fa-stack-1x fa-inverse">
									<xsl:text> </xsl:text>
								</i>
							</span>
						</a>
					</xsl:if>
					<xsl:if test="@twitterURL!=''">
						<a href="{@twitterURL}" target="_blank" title="{$myName} on Twitter" class="social-id-tw">
							<span class="fa-stack fa-lg">
								<i class="fab fa-circle fa-stack-2x">
									<xsl:text> </xsl:text>
								</i>
								<i class="fa-brands fa-x-twitter fa-stack-1x fa-inverse">
									<xsl:text> </xsl:text>
								</i>
							</span>
						</a>
					</xsl:if>
					<xsl:if test="@linkedInURL!=''">
						<a href="{@linkedInURL}" target="_blank" title="{$myName} on LinkedIn" class="social-id-li">
							<span class="fa-stack fa-lg">
								<i class="fab fa-circle fa-stack-2x">
									<xsl:text> </xsl:text>
								</i>
								<i class="fab fa-linkedin fa-stack-1x fa-inverse">
									<xsl:text> </xsl:text>
								</i>
							</span>
						</a>
					</xsl:if>
					<xsl:if test="@googlePlusURL!=''">
						<a href="{@googlePlusURL}" target="_blank" title="{$myName} on Google+" class="social-id-gp">
							<span class="fa-stack fa-lg">
								<i class="fa fa-circle fa-stack-2x">
									<xsl:text> </xsl:text>
								</i>
								<i class="fab fa-google-plus fa-stack-1x fa-inverse">
									<xsl:text> </xsl:text>
								</i>
							</span>
						</a>
					</xsl:if>
					<xsl:if test="@pinterestURL!=''">
						<a href="{@pinterestURL}" target="_blank" title="{$myName} on Pinterest" class="social-id-pi">
							<span class="fa-stack fa-lg">
								<i class="fa fa-circle fa-stack-2x">
									<xsl:text> </xsl:text>
								</i>
								<i class="fab fa-pinterest fa-stack-1x fa-inverse">
									<xsl:text> </xsl:text>
								</i>
							</span>
						</a>
					</xsl:if>
					<xsl:if test="@youtubeURL!=''">
						<a href="{@youtubeURL}" target="_blank" title="{$myName} on Youtube" class="social-id-yt">
							<span class="fa-stack fa-lg">
								<i class="fa fa-circle fa-stack-2x">
									<xsl:text> </xsl:text>
								</i>
								<i class="fab fa-youtube fa-stack-1x fa-inverse">
									<xsl:text> </xsl:text>
								</i>
							</span>
						</a>
					</xsl:if>
					<xsl:if test="@instagramURL!=''">
						<a href="{@instagramURL}" target="_blank" title="{$myName} on Instagram" class="social-id-ig">
							<span class="fa-stack fa-lg">
								<i class="fa fa-circle fa-stack-2x">
									<xsl:text> </xsl:text>
								</i>
								<i class="fab fa-instagram fa-stack-1x fa-inverse">
									<xsl:text> </xsl:text>
								</i>
							</span>
						</a>
					</xsl:if>
				</xsl:when>

				<xsl:otherwise>
					<xsl:if test="@facebookURL!=''">
						<a href="{@facebookURL}" target="_blank" title="{$myName} on Facebook" class="social-id-fb">
							<i class="fab fa-facebook">
								<span class="visually-hidden">
									<xsl:value-of select="$myName"/> on Facebook
								</span>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@twitterURL!=''">
						<a href="{@twitterURL}" target="_blank" title="{$myName} on Twitter" class="social-id-tw">
							<i class="fa-brands fa-x-twitter">
								<span class="visually-hidden">
									<xsl:value-of select="$myName"/> on Twitter
								</span>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@linkedInURL!=''">
						<a href="{@linkedInURL}" target="_blank" title="{$myName} on LinkedIn" class="social-id-li">
							<i class="fab fa-linkedin">
								<span class="visually-hidden">
									<xsl:value-of select="$myName"/> on LinkedIn
								</span>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@googlePlusURL!=''">
						<a href="{@googlePlusURL}" target="_blank" title="{$myName} on Google+" class="social-id-gp">
							<i class="fab fa-google-plus">
								<span class="visually-hidden">
									<xsl:value-of select="$myName"/> on Google+
								</span>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@pinterestURL!=''">
						<a href="{@pinterestURL}" target="_blank" title="{$myName} on Pinterest" class="social-id-pi">
							<i class="fab fa-pinterest">
								<span class="visually-hidden">
									<xsl:value-of select="$myName"/> on Pintrest
								</span>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@youtubeURL!=''">
						<a href="{@youtubeURL}" target="_blank" title="{$myName} on Youtube" class="social-id-yt">
							<i class="fab fa-youtube ">
								<span class="visually-hidden">
									<xsl:value-of select="$myName"/> on Youtube
								</span>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@instagramURL!=''">
						<a href="{@instagramURL}" target="_blank" title="{$myName} on Instagram" class="social-id-ig">
							<i class="fab fa-instagram">
								<span class="visually-hidden">
									<xsl:value-of select="$myName"/> on Instagram
								</span>
							</i>
						</a>
					</xsl:if>
				</xsl:otherwise>-->
      <!--<xsl:otherwise>
					<xsl:if test="@facebookURL!=''">
						<a href="{@facebookURL}" target="_blank" title="{$myName} on Facebook" id="social-id-fb">
							<img src="/ptn/core/icons/social/{$iconSet}/facebook.png" alt="{$myName} on Facebook" title="Follow {$myName} on Facebook" />
						</a>
					</xsl:if>
					<xsl:if test="@twitterURL!=''">
						<a href="{@twitterURL}" target="_blank" title="{$myName} on Twitter" id="social-id-tw">
							<img src="/ptn/core/icons/social/{$iconSet}/twitter.png" alt="{$myName} on Twitter" title="Follow {$myName} on Twitter" />
						</a>
					</xsl:if>
					<xsl:if test="@linkedInURL!=''">
						<a href="{@linkedInURL}" target="_blank" title="{$myName} on LinkedIn" id="social-id-li">
							<img src="/ptn/core/icons/social/{$iconSet}/LinkedIn.png" alt="{$myName} on LinkedIn" title="Follow {$myName} on LinkedIn" />
						</a>
					</xsl:if>
					<xsl:if test="@googlePlusURL!=''">
						<a href="{@googlePlusURL}" target="_blank" title="{$myName} on Google+" id="social-id-gp">
							<img src="/ptn/core/icons/social/{$iconSet}/Googleplus.png" alt="{$myName} on Google+" title="Follow {$myName} on Google+" />
						</a>
					</xsl:if>
					<xsl:if test="@pinterestURL!=''">
						<a href="{@pinterestURL}" target="_blank" title="{$myName} on Pinterest" id="social-id-li">
							<img src="/ptn/core/icons/social/{$iconSet}/Pinterest.png" alt="{$myName} on Pinterest" title="Follow {$myName} on Pinterest" />
						</a>
					</xsl:if>
					<xsl:if test="@youtubeURL!=''">
						<a href="{@youtubeURL}" target="_blank" title="{$myName} on YouTube" id="social-id-yt">
							<img src="/ptn/core/icons/social/{$iconSet}/YouTube.png" alt="{$myName} on YouTube" title="Follow {$myName} on YouTube" />
						</a>
					</xsl:if>
					<xsl:if test="@instagramURL!=''">
						<a href="{@instagramURL}" target="_blank" title="{$myName} on Pinterest" id="social-id-ig">
							<img src="/ptn/core/icons/social/{$iconSet}/Instagram.png" alt="{$myName} on Instagram" title="Follow {$myName} on Instagram" />
						</a>
					</xsl:if>
				</xsl:otherwise>-->
      <!--
			</xsl:choose>-->
      <xsl:if test="@facebookURL!=''">
        <a href="{@facebookURL}"  title="{$myName} on Facebook" class="social-id-fb">
          <xsl:attribute name="target">_blank</xsl:attribute>
          <i>
            <xsl:attribute name="class">
              <xsl:text>fab fa-facebook </xsl:text>
              <xsl:value-of select="$icon-size"/>
            </xsl:attribute>
            <span class="visually-hidden">
              <xsl:value-of select="$myName"/> on Facebook
            </span>
          </i>
        </a>
      </xsl:if>
      <xsl:if test="@twitterURL!=''">
        <a href="{@twitterURL}" target="_blank" title="{$myName} on Twitter" class="social-id-tw">
          <i>
            <xsl:attribute name="class">
              <xsl:text>fab fa-x-twitter </xsl:text>
              <xsl:value-of select="$icon-size"/>
            </xsl:attribute>
            <span class="visually-hidden">
              <xsl:value-of select="$myName"/> on X
            </span>
          </i>
        </a>
      </xsl:if>
      <xsl:if test="@linkedInURL!=''">
        <a href="{@linkedInURL}" target="_blank" title="{$myName} on LinkedIn" class="social-id-li">
          <i>
            <xsl:attribute name="class">
              <xsl:text>fab fa-linkedin </xsl:text>
              <xsl:value-of select="$icon-size"/>
            </xsl:attribute>
            <span class="visually-hidden">
              <xsl:value-of select="$myName"/> on LinkedIn
            </span>
          </i>
        </a>
      </xsl:if>
      <xsl:if test="@pinterestURL!=''">
        <a href="{@pinterestURL}" target="_blank" title="{$myName} on Pinterest" class="social-id-pi">
          <i>
            <xsl:attribute name="class">
              <xsl:text>fab fa-pinterest </xsl:text>
              <xsl:value-of select="$icon-size"/>
            </xsl:attribute>
            <span class="visually-hidden">
              <xsl:value-of select="$myName"/> on Pintrest
            </span>
          </i>
        </a>
      </xsl:if>
      <xsl:if test="@youtubeURL!=''">
        <a href="{@youtubeURL}" target="_blank" title="{$myName} on Youtube" class="social-id-yt">
          <i>
            <xsl:attribute name="class">
              <xsl:text>fab fa-youtube </xsl:text>
              <xsl:value-of select="$icon-size"/>
            </xsl:attribute>
            <span class="visually-hidden">
              <xsl:value-of select="$myName"/> on Youtube
            </span>
          </i>
        </a>
      </xsl:if>
      <xsl:if test="@instagramURL!=''">
        <a href="{@instagramURL}" target="_blank" title="{$myName} on Instagram" class="social-id-ig">
          <i>
            <xsl:attribute name="class">
              <xsl:text>fab fa-instagram </xsl:text>
              <xsl:value-of select="$icon-size"/>
            </xsl:attribute>
            <span class="visually-hidden">
              <xsl:value-of select="$myName"/> on Instagram
            </span>
          </i>
        </a>
      </xsl:if>
      <xsl:if test="@blueSkyURL!=''">
        <a href="{@blueSkyURL}" target="_blank" title="{$myName} on Bluesky" class="social-id-bs">
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
    <div class="socialLinks clearfix iconset-{$iconSet} align-{$align} layout-{$layout} {$order-class}" style="margin-left:-{$half-spacing}{$spacing-unit};margin-right:-{$half-spacing}{$spacing-unit}">
      
      <!--<xsl:choose>
				<xsl:when test="@uploadSprite!=''">
					<xsl:if test="@facebookURL!=''">
						<a href="{@facebookURL}" title="{$myName} on Facebook" style="background-image:url({@uploadSprite})" class="social-id-fb social-sprite">
							&#160;
						</a>
					</xsl:if>
					<xsl:if test="@twitterURL!=''">
						<a href="{@twitterURL}" title="{$myName} on Twitter" style="background-image:url({@uploadSprite});background-position:128px 0" class="social-id-tw social-sprite">
							&#160;
						</a>
					</xsl:if>
					<xsl:if test="@linkedInURL!=''">
						<a href="{@linkedInURL}" title="{$myName} on LinkedIn" style="background-image:url({@uploadSprite});background-position:96px 0" class="social-id-li social-sprite">
							&#160;
						</a>
					</xsl:if>
					<xsl:if test="@googlePlusURL!=''">
						<a href="{@googlePlusURL}" title="{$myName} on Google+" style="background-image:url({@uploadSprite});background-position:64px 0" class="social-id-gp social-sprite">
							&#160;
						</a>
					</xsl:if>
					<xsl:if test="@pinterestURL!=''">
						<a href="{@pinterestURL}" title="{$myName} on Pinterest" style="background-image:url({@uploadSprite});background-position:32px 0" class="social-id-pi social-sprite">
							&#160;
						</a>
					</xsl:if>
					<xsl:if test="@youtubeURL!=''">
						<a href="{@youtubeURL}" title="{$myName} on You Tube"  style="background-image:url({@uploadSprite});background-position:160px 0" class="social-id-yt social-sprite">
							&#160;
						</a>
					</xsl:if>
					<xsl:if test="@instagramURL!=''">
						<a href="{@instagramURL}" title="{$myName} on Instagram" style="background-image:url({@uploadSprite});background-position:192px 0" class="social-id-ig social-sprite">
							&#160;
						</a>
					</xsl:if>
				</xsl:when>
				<xsl:when test="$iconSet='icons'">
					<xsl:if test="@facebookURL!=''">
						<a href="{@facebookURL}" title="{$myName} on Facebook" class="social-id-fb">
							<i class="fab fa-2x fa-facebook">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@twitterURL!=''">
						<a href="{@twitterURL}" title="{$myName} on Twitter" class="social-id-tw">
							<i class="fa-brands fa-2x fa-x-twitter">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@linkedInURL!=''">
						<a href="{@linkedInURL}" title="{$myName} on LinkedIn" class="social-id-li">
							<i class="fab fa-2x fa-linkedin">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@googlePlusURL!=''">
						<a href="{@googlePlusURL}" title="{$myName} on Google+" class="social-id-gp">
							<i class="fab fa-2x fa-google-plus">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@pinterestURL!=''">
						<a href="{@pinterestURL}" title="{$myName} on Pinterest" class="social-id-li">
							<i class="fab fa-2x fa-pinterest">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@youtubeURL!=''">
						<a href="{@youtubeURL}" title="{$myName} on Youtube" class="social-id-yt">
							<i class="fab fa-2x fa-youtube">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@instagramURL!=''">
						<a href="{@instagramURL}" title="{$myName} on Instagram" class="social-id-ig">
							<i class="fab fa-2x fa-instagram">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
				</xsl:when>
				<xsl:when test="$iconSet='icons-square'">
					<xsl:if test="@facebookURL!=''">
						<a href="{@facebookURL}" title="{$myName} on Facebook" class="social-id-fb">
							<i class="fab fa-3x fa-facebook-square">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@twitterURL!=''">
						<a href="{@twitterURL}" title="{$myName} on Twitter" class="social-id-tw">
							<i class="fa-brands fa-3x fa-x-twitter-square">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@linkedInURL!=''">
						<a href="{@linkedInURL}" title="{$myName} on LinkedIn" class="social-id-li">
							<i class="fab fa-3x fa-linkedin-square">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@googlePlusURL!=''">
						<a href="{@googlePlusURL}" title="{$myName} on Google+" class="social-id-gp">
							<i class="fab fa-3x fa-google-plus-square">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@pinterestURL!=''">
						<a href="{@pinterestURL}" title="{$myName} on Pinterest" class="social-id-pi">
							<i class="fab fa-3x fa-pinterest-square">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@youtubeURL!=''">
						<a href="{@youtubeURL}" title="{$myName} on Youtube" class="social-id-yt">
							<i class="fab fa-3x fa-youtube-square">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@instagramURL!=''">
						<a href="{@instagramURL}" title="{$myName} on Instagram" class="social-id-ig">
							<i class="fab fa-3x fa-instagram">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@spotifyURL!=''">
						<a href="{@spotifyURL}" title="{$myName} on Spotify" class="social-id-ig">
							<i class="fab fa-3x fa-spotify">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
				</xsl:when>
				<xsl:when test="$iconSet='icons-circle'">
					<xsl:if test="@facebookURL!=''">
						<a href="{@facebookURL}" title="{$myName} on Facebook" class="social-id-fb">
							<span class="fa-stack fa-lg">
								<i class="fa fa-circle fa-stack-2x">
									<xsl:text> </xsl:text>
								</i>
								<i class="fab fa-facebook fa-stack-1x fa-inverse">
									<xsl:text> </xsl:text>
								</i>
							</span>
						</a>
					</xsl:if>
					<xsl:if test="@twitterURL!=''">
						<a href="{@twitterURL}" title="{$myName} on Twitter" class="social-id-tw">
							<span class="fa-stack fa-lg">
								<i class="fa fa-circle fa-stack-2x">
									<xsl:text> </xsl:text>
								</i>
								<i class="fa-brands fa-x-twitter fa-stack-1x fa-inverse">
									<xsl:text> </xsl:text>
								</i>
							</span>
						</a>
					</xsl:if>
					<xsl:if test="@linkedInURL!=''">
						<a href="{@linkedInURL}" title="{$myName} on LinkedIn" class="social-id-li">
							<span class="fa-stack fa-lg">
								<i class="fa fa-circle fa-stack-2x">
									<xsl:text> </xsl:text>
								</i>
								<i class="fab fa-linkedin fa-stack-1x fa-inverse">
									<xsl:text> </xsl:text>
								</i>
							</span>
						</a>
					</xsl:if>
					<xsl:if test="@googlePlusURL!=''">
						<a href="{@googlePlusURL}" title="{$myName} on Google+" class="social-id-gp">
							<span class="fa-stack fa-lg">
								<i class="fa fa-circle fa-stack-2x">
									<xsl:text> </xsl:text>
								</i>
								<i class="fab fa-google-plus fa-stack-1x fa-inverse">
									<xsl:text> </xsl:text>
								</i>
							</span>
						</a>
					</xsl:if>
					<xsl:if test="@pinterestURL!=''">
						<a href="{@pinterestURL}" title="{$myName} on Pinterest" class="social-id-pi">
							<span class="fa-stack fa-lg">
								<i class="fa fa-circle fa-stack-2x">
									<xsl:text> </xsl:text>
								</i>
								<i class="fab fa-pinterest fa-stack-1x fa-inverse">
									<xsl:text> </xsl:text>
								</i>
							</span>
						</a>
					</xsl:if>
					<xsl:if test="@youtubeURL!=''">
						<a href="{@youtubeURL}" title="{$myName} on Youtube" class="social-id-yt">
							<span class="fa-stack fa-lg">
								<i class="fa fa-circle fa-stack-2x">
									<xsl:text> </xsl:text>
								</i>
								<i class="fab fa-youtube fa-stack-1x fa-inverse">
									<xsl:text> </xsl:text>
								</i>
							</span>
						</a>
					</xsl:if>
					<xsl:if test="@instagramURL!=''">
						<a href="{@instagramURL}" title="{$myName} on Instagram" class="social-id-ig">
							<span class="fa-stack fa-lg">
								<i class="fa fa-circle fa-stack-2x">
									<xsl:text> </xsl:text>
								</i>
								<i class="fa-brands fa-instagram fa-stack-1x fa-inverse">
									<xsl:text> </xsl:text>
								</i>
							</span>
						</a>
					</xsl:if>
					<xsl:if test="@spotifyURL!=''">
						<a href="{@spotifyURL}" title="{$myName} on Spotify" class="social-id-ig">
							<span class="fa-stack fa-lg">
								<i class="fa fa-circle fa-stack-2x">
									<xsl:text> </xsl:text>
								</i>
								<i class="fab fa-spotify fa-stack-1x fa-inverse">
									<xsl:text> </xsl:text>
								</i>
							</span>
						</a>
					</xsl:if>
				</xsl:when>
				<xsl:when test="$iconSet='plain'">
					<xsl:if test="@facebookURL!=''">
						<a href="{@facebookURL}" title="{$myName} on Facebook" class="social-id-fb">
							<i class="fab fa-facebook">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@twitterURL!=''">
						<a href="{@twitterURL}" title="{$myName} on Twitter" class="social-id-tw">
							<i class="fa-brands fa-x-twitter">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@linkedInURL!=''">
						<a href="{@linkedInURL}" title="{$myName} on LinkedIn" class="social-id-li">
							<i class="fab fa-linkedin">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@googlePlusURL!=''">
						<a href="{@googlePlusURL}" title="{$myName} on Google+" class="social-id-gp">
							<i class="fab fa-google-plus">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@pinterestURL!=''">
						<a href="{@pinterestURL}" title="{$myName} on Pinterest" class="social-id-pi">
							<i class="fab fa-pinterest">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@youtubeURL!=''">
						<a href="{@youtubeURL}" title="{$myName} on Youtube" class="social-id-yt">
							<i class="fab fa-youtube ">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@instagramURL!=''">
						<a href="{@instagramURL}" title="{$myName} on Instagram" class="social-id-ig">
							<i class="fab fa-instagram">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
					<xsl:if test="@spotifyURL!=''">
						<a href="{@spotifyURL}" title="{$myName} on Spotify" class="social-id-isp">
							<i class="fab fa-spotify">
								<xsl:text> </xsl:text>
							</i>
						</a>
					</xsl:if>
				</xsl:when>
				<xsl:otherwise>
					<xsl:if test="@facebookURL!=''">
						<a href="{@facebookURL}" title="{$myName} on Facebook" class="social-id-fb">
							<img src="/ptn/core/icons/social/{$iconSet}/facebook.png" alt="{$myName} on Facebook" title="Follow {$myName} on Facebook" />
						</a>
					</xsl:if>
					<xsl:if test="@twitterURL!=''">
						<a href="{@twitterURL}" title="{$myName} on Twitter" class="social-id-tw">
							<img src="/ptn/core/icons/social/{$iconSet}/twitter.png" alt="{$myName} on Twitter" title="Follow {$myName} on Twitter" />
						</a>
					</xsl:if>
					<xsl:if test="@linkedInURL!=''">
						<a href="{@linkedInURL}" title="{$myName} on LinkedIn" class="social-id-li">
							<img src="/ptn/core/icons/social/{$iconSet}/LinkedIn.png" alt="{$myName} on LinkedIn" title="Follow {$myName} on LinkedIn" />
						</a>
					</xsl:if>
					<xsl:if test="@googlePlusURL!=''">
						<a href="{@googlePlusURL}" title="{$myName} on Google+" class="social-id-gp">
							<img src="/ptn/core/icons/social/{$iconSet}/Googleplus.png" alt="{$myName} on Google+" title="Follow {$myName} on Google+" />
						</a>
					</xsl:if>
					<xsl:if test="@pinterestURL!=''">
						<a href="{@pinterestURL}" title="{$myName} on Pinterest" class="social-id-li">
							<img src="/ptn/core/icons/social/{$iconSet}/Pinterest.png" alt="{$myName} on Pinterest" title="Follow {$myName} on Pinterest" />
						</a>
					</xsl:if>
					<xsl:if test="@youtubeURL!=''">
						<a href="{@youtubeURL}" title="{$myName} on YouTube" class="social-id-yt">
							<img src="/ptn/core/icons/social/{$iconSet}/YouTube.png" alt="{$myName} on YouTube" title="Follow {$myName} on YouTube" />
						</a>
					</xsl:if>
					<xsl:if test="@instagramURL!=''">
						<a href="{@instagramURL}" title="{$myName} on Pinterest" class="social-id-ig">
							<img src="/ptn/core/icons/social/{$iconSet}/Instagram.png" alt="{$myName} on Instagram" title="Follow {$myName} on Instagram" />
						</a>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>-->
      <xsl:if test="@facebookURL!=''">
        <a href="{@facebookURL}" title="{$myName} on Facebook" class="social-id-fb" style="padding-left:{$half-spacing}{$spacing-unit};padding-right:{$half-spacing}{$spacing-unit};padding-bottom:{$spacing}{$spacing-unit};order:{$fb-order}">
          <xsl:if test="$blank='true'">
            <xsl:attribute name="target">_blank</xsl:attribute>
          </xsl:if>
          <i>
            <xsl:attribute name="class">
              <xsl:text>fab fa-facebook </xsl:text>
              <xsl:value-of select="$icon-size"/>
            </xsl:attribute>
            <span class="visually-hidden">
              <xsl:value-of select="$myName"/> on Facebook
            </span>
          </i>
          <xsl:if test="$layout='vertical'">
            <xsl:text>Facebook</xsl:text>
          </xsl:if>
        </a>
      </xsl:if>
      <xsl:if test="@twitterURL!=''">
        <a href="{@twitterURL}" title="{$myName} on Twitter" class="social-id-tw" style="padding-left:{$half-spacing}{$spacing-unit};padding-right:{$half-spacing}{$spacing-unit};padding-bottom:{$spacing}{$spacing-unit};order:{$x-order}">
          <xsl:if test="$blank='true'">
            <xsl:attribute name="target">_blank</xsl:attribute>
          </xsl:if>
          <i>
            <xsl:attribute name="class">
              <xsl:text>fab fa-x-twitter </xsl:text>
              <xsl:value-of select="$icon-size"/>
            </xsl:attribute>
            <span class="visually-hidden">
              <xsl:value-of select="$myName"/> on X
            </span>
          </i>
          <xsl:if test="$layout='vertical'">
            <xsl:text>X</xsl:text>
          </xsl:if>
        </a>
      </xsl:if>
      <xsl:if test="@linkedInURL!=''">
        <a href="{@linkedInURL}" title="{$myName} on LinkedIn" class="social-id-li" style="padding-left:{$half-spacing}{$spacing-unit};padding-right:{$half-spacing}{$spacing-unit};padding-bottom:{$spacing}{$spacing-unit};order:{$li-order}">
          <xsl:if test="$blank='true'">
            <xsl:attribute name="target">_blank</xsl:attribute>
          </xsl:if>
          <i>
            <xsl:attribute name="class">
              <xsl:text>fab fa-linkedin </xsl:text>
              <xsl:value-of select="$icon-size"/>
            </xsl:attribute>
            <span class="visually-hidden">
              <xsl:value-of select="$myName"/> on LinkedIn
            </span>
          </i>
          <xsl:if test="$layout='vertical'">
            <xsl:text>LinkedIn</xsl:text>
          </xsl:if>
        </a>
      </xsl:if>
      <xsl:if test="@pinterestURL!=''">
        <a href="{@pinterestURL}" title="{$myName} on Pinterest" class="social-id-pi" style="padding-left:{$half-spacing}{$spacing-unit};padding-right:{$half-spacing}{$spacing-unit};padding-bottom:{$spacing}{$spacing-unit};order:{$p-order}">
          <xsl:if test="$blank='true'">
            <xsl:attribute name="target">_blank</xsl:attribute>
          </xsl:if>
          <i>
            <xsl:attribute name="class">
              <xsl:text>fab fa-pinterest </xsl:text>
              <xsl:value-of select="$icon-size"/>
            </xsl:attribute>
            <span class="visually-hidden">
              <xsl:value-of select="$myName"/> on Pintrest
            </span>
          </i>
          <xsl:if test="$layout='vertical'">
            <xsl:text>Pintrest</xsl:text>
          </xsl:if>
        </a>
      </xsl:if>
      <xsl:if test="@youtubeURL!=''">
        <a href="{@youtubeURL}" title="{$myName} on Youtube" class="social-id-yt" style="padding-left:{$half-spacing}{$spacing-unit};padding-right:{$half-spacing}{$spacing-unit};padding-bottom:{$spacing}{$spacing-unit};order:{$yt-order}">
          <xsl:if test="$blank='true'">
            <xsl:attribute name="target">_blank</xsl:attribute>
          </xsl:if>
          <i>
            <xsl:attribute name="class">
              <xsl:text>fab fa-youtube </xsl:text>
              <xsl:value-of select="$icon-size"/>
            </xsl:attribute>
            <span class="visually-hidden">
              <xsl:value-of select="$myName"/> on Youtube
            </span>
          </i>
          <xsl:if test="$layout='vertical'">
            <xsl:text>Youtube</xsl:text>
          </xsl:if>
        </a>
      </xsl:if>
      <xsl:if test="@instagramURL!=''">
        <a href="{@instagramURL}" title="{$myName} on Instagram" class="social-id-ig" style="padding-left:{$half-spacing}{$spacing-unit};padding-right:{$half-spacing}{$spacing-unit};padding-bottom:{$spacing}{$spacing-unit};order:{$i-order}">
          <xsl:if test="$blank='true'">
            <xsl:attribute name="target">_blank</xsl:attribute>
          </xsl:if>
          <i>
            <xsl:attribute name="class">
              <xsl:text>fab fa-instagram </xsl:text>
              <xsl:value-of select="$icon-size"/>
            </xsl:attribute>
            <span class="visually-hidden">
              <xsl:value-of select="$myName"/> on Instagram
            </span>
          </i>
          <xsl:if test="$layout='vertical'">
            <xsl:text>Instagram</xsl:text>
          </xsl:if>
        </a>
      </xsl:if>
      <xsl:if test="@spotifyURL!=''">
        <a href="{@spotifyURL}" title="{$myName} on Spotify" class="social-id-isp" style="padding-left:{$half-spacing}{$spacing-unit};padding-right:{$half-spacing}{$spacing-unit};padding-bottom:{$spacing}{$spacing-unit};">
          <xsl:if test="$blank='true'">
            <xsl:attribute name="target">_blank</xsl:attribute>
          </xsl:if>
          <i>
            <xsl:attribute name="class">
              <xsl:text>fab fa-spotify </xsl:text>
              <xsl:value-of select="$icon-size"/>
            </xsl:attribute>
            <span class="visually-hidden">
              <xsl:value-of select="$myName"/> on Spotify
            </span>
          </i>
          <xsl:if test="$layout='vertical'">
            <xsl:text>Spotify</xsl:text>
          </xsl:if>
        </a>
      </xsl:if>
      <xsl:if test="@blueSkyURL!=''">
        <a href="{@blueSkyURL}" title="{$myName} on Bluesky" class="social-id-bs" style="padding-left:{$half-spacing}{$spacing-unit};padding-right:{$half-spacing}{$spacing-unit};padding-bottom:{$spacing}{$spacing-unit};order:{$bs-order}">
          <xsl:if test="$blank='true'">
            <xsl:attribute name="target">_blank</xsl:attribute>
          </xsl:if>
          <!--<i>
            <xsl:attribute name="class">
              <xsl:text>fa-brands  fa-bluesky </xsl:text>
              <xsl:value-of select="$icon-size"/>
            </xsl:attribute>
            <span class="visually-hidden">
              <xsl:value-of select="$myName"/> on BlueSky
            </span>
          </i>-->
          <svg role="img" aria-label="Bluesky logo" id="svg-Bluesky" width="24" height="24" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg" xmlns:ev="http://www.w3.org/2001/xml-events" xmlns:xlink="http://www.w3.org/1999/xlink">
            <image id="svg-img-Bluesky" xlink:href="/ptn/modules/sociallinks/bluesky-brands-solid.svg"  width="24" height="24" class="img-responsive"> </image>
          </svg>
          <span class="visually-hidden">
            <xsl:value-of select="$myName"/> on BlueSky
          </span>
          <xsl:if test="$layout='vertical'">
            <xsl:text>BlueSky</xsl:text>
          </xsl:if>
        </a>
      </xsl:if>
    </div>
  </xsl:template>

</xsl:stylesheet>
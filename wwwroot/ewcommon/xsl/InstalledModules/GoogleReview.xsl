<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml"
                xmlns:v-for="https://vuejs.org/v2/api/v-for" xmlns:v-if="http://example.com/xml/v-if"
                xmlns:v-bind="http://example.com/xml/v-bind" xmlns:v-on="http://example.com/xml/v-on">


  <xsl:template match="Content[@type='Review']" mode="getThWidth">285</xsl:template>
  <xsl:template match="Content[@type='Review']" mode="getThHeight">214</xsl:template>

  <xsl:template match="Content[@type='Review']" mode="getThWidth-xxs">260</xsl:template>
  <xsl:template match="Content[@type='Review']" mode="getThHeight-xxs">195</xsl:template>

  <xsl:template match="Content[@type='Review']" mode="getThWidth-xs">365</xsl:template>
  <xsl:template match="Content[@type='Review']" mode="getThHeight-xs">274</xsl:template>

  <xsl:template match="Content[@type='Review']" mode="getThWidth-sm">270</xsl:template>
  <xsl:template match="Content[@type='Review']" mode="getThHeight-sm">203</xsl:template>

  <xsl:template match="Content[@type='Review']" mode="getThWidth-md">285</xsl:template>
  <xsl:template match="Content[@type='Review']" mode="getThHeight-md">214</xsl:template>

  <xsl:template match="Content[@type='Review']" mode="getThWidth-lg">285</xsl:template>
  <xsl:template match="Content[@type='Review']" mode="getThHeight-lg">214</xsl:template>

  <xsl:template match="Content[@type='Review']" mode="getThWidth-xl">285</xsl:template>
  <xsl:template match="Content[@type='Review']" mode="getThHeight-xl">214</xsl:template>



  <xsl:template match="Content[@type='Module' and @moduleType='GoogleReview']" mode="displayBrief">
    <xsl:variable name="GoogleAPIUrl" select="@googleReviewAPIUrl"></xsl:variable>

    <input type="hidden" value="{$GoogleAPIUrl}" id="googleAPIUrl"/>
    <div class="product-video">
      <!-- Google Reviews Section -->
      <div class="Reviews" id="product-reviews">

        <h4>Reviews</h4>
        <div class="review-header"  v-if="GoogleReviewResponse.GoogleReview.Content.length>0">

          <p class="star-heading">
            {{reviewCount }} reviewCount
          </p>

          <span class="stars">
            <xsl:text> </xsl:text>
            <i
              v-for="n in Math.round(Number(averageRating))"
              key="'avg'+n"
              class="fa fa-star">
              <xsl:text> </xsl:text>
            </i>
          </span>

          <div class="review-stats">
            Average Rating: <strong>{{averageRating}}</strong><br />
            Total Reviews: <strong>{{reviewCount }}</strong><br />
          </div>

          <p>Find out what other customers think of the experience</p>
        </div>

        <!--<div id="reviewText"> Top Reviews</div>-->

        <div id="reviewsTab">
          <div class="relatedcontent reviews">

            <div
              v-for="(review, index) in GoogleReviewResponse.GoogleReview.Content"
              v-if="parseInt(review.Rating) >= parseInt(GoogleReviewResponse.GoogleReview.RatingLimit._ratingLimit)"
              key="index"
              class="listItem review reviewlist googleReviewlist" >
              <div class="lIinner TopReviews">


                <div class="cta-img">
                  <a
                 class=""
                 data-remote="false"
                 data-toggle="modal"
                 v-on:click="OpenReviewImagePopup(review.Images?.img?._src || '/ewcommon/images/awaiting-image-thumbnail.gif')"
                 v-bind:src="review.Images?.img?._src || '/ewcommon/images/awaiting-image-thumbnail.gif'"
      >
                    <picture>

                      <source
                        type="image/webp"
                        media="(max-width: 575px)"
                        srcset="review.Images?.img?._src || '/ewcommon/images/~th-xxs-260x274/~th-crop-awaiting-image-thumbnail.webp'"
          />
                      <source
                        type="image/webp"
                        media="(max-width: 767px)"
                        srcset="review.Images?.img?._src || ' /ewcommon/images/~th-xs-365x274/~th-crop-awaiting-image-thumbnail.webp'"
          />
                      <source
                        type="image/webp"
                        media="(max-width: 991px)"
                        srcset="review.Images?.img?._src || '/ewcommon/images/~th-sm-270x203/~th-crop-awaiting-image-thumbnail.webp'"
          />
                      <source
                        type="image/webp"
                        media="(max-width: 1199px)"
                        srcset="review.Images?.img?._src || '/ewcommon/images/~th-md-285x214/~th-crop-awaiting-image-thumbnail.webp'"
          />
                      <source
                        type="image/webp"
                        media="(min-width: 1200px)"
                        srcset="review.Images?.img?._src || '/ewcommon/images/~th-lg-285x214/~th-crop-awaiting-image-thumbnail.webp'"
          />
                      <img
                        v-bind:src="review.Images?.img?._src "
                        width="285"
                        height="214"
                        class="photo"
                        loading="lazy"
                        alt="Customer Photo"
                        title="Customer Photo"
          />
                    </picture>
                  </a>
                </div>

                <span class="rating-foreground rating">
                  <span class="sr-only"> Rating: {{ review.Rating }} star</span>
                  <span class="value-title">
                    <xsl:text> </xsl:text>
                    <i v-for="n in parseInt(review.Rating)" key="'star'+index+n" class="fa fa-star">
                      <xsl:text> </xsl:text>
                    </i>
                  </span>
                </span>
                <div class="reviewer">{{ review.Reviewer }}</div>
                <div class="review-date">{{ review.reviewSinceDate }}</div>

                <div class="summary" v-if="review.Summary">
                  <span v-if="wordCount(review.Summary) &lt; 20">
                    {{ review.Summary }}
                  </span>
                  <span v-else="">
                    {{ firstWords(review.Summary, 20) }}...
                    <div class="readmore">
                      <a href="#" v-on:click.prevent="showModal(review)">Read More</a>
                    </div>
                  </span>
                </div>
                <div class="modal fade" tabindex="-1" role="dialog" id="ShowReviewSummary" aria-labelledby="ShowReview" aria-hidden="true"  style="z-index:9997;">
                  <div class="modal-dialog"  role="document">
                    <div class="modal-content">
                      <div class="modal-body review-modal">
                        <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                          <span aria-hidden="true">
                            <xsl:text> </xsl:text>
                            <i class="fa fa-times-circle">
                              <xsl:text> </xsl:text>
                            </i>
                          </span>
                        </button>

                        <span class="rating-foreground rating">
                          <span class="sr-only" v-if="modalReview">Rating: {{ modalReview.Rating }} star</span>
                          <span class="value-title reviewRate rating5" v-if="modalReview">
                            <xsl:text> </xsl:text>
                            <i v-for="n in parseInt(modalReview.Rating)" key="'star'+n" class="fa fa-star">
                              <xsl:text> </xsl:text>
                            </i>
                          </span>
                        </span>

                        <div class="reviewer" v-if="modalReview">{{ modalReview.Reviewer }}</div>

                        <!-- Date -->
                        <div class="review-date" v-if="modalReview">{{ modalReview.reviewSinceDate }}</div>

                        <!-- Full Summary -->
                        <div class="summary" v-if="modalReview">
                          {{ modalReview.Summary }}
                        </div>

                      </div>
                    </div>
                  </div>
                </div>
              </div>

            </div>
          </div>
        </div>
      </div>
    </div>
  </xsl:template>
  <xsl:template match="Content[@type='Module' and @moduleType='GoogleReview']" mode="contentJS">
    <xsl:call-template name="bundle-js">
      <xsl:with-param name="comma-separated-files">
        <xsl:text>~/ewThemes/intotheblue2019/js/Vuejs/vue.min.js,</xsl:text>
        <xsl:text>~/ewThemes/intotheblue2019/js/Vuejs/polyfill.js,</xsl:text>
        <xsl:text>~/ewCommon/js/vuejs/product-vue.js</xsl:text>
      </xsl:with-param>
      <xsl:with-param name="bundle-path">
        <xsl:text>~/Bundles/GoogleReview</xsl:text>
      </xsl:with-param>
    </xsl:call-template>
  </xsl:template>


</xsl:stylesheet>

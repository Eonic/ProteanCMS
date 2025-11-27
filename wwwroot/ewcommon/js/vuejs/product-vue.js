
document.addEventListener("DOMContentLoaded", function () {



    var addGoogleReviewsAPIUrl = '/ewapi/Cms.Content/GetGoogleReviews';

    var isGoogleReview = document.querySelector(".GoogleReviews");
    if (isGoogleReview) {
        new Vue({
            el: '.GoogleReviews',
            data: {
                GoogleReviewResponse: null,
                modalReview: null

            },
            computed: {
                // Filtered reviews: 4+ stars or rating limit
                filteredReviews: function () {
                    if (
                        !this.GoogleReviewResponse ||
                        !this.GoogleReviewResponse.GoogleReview ||
                        !Array.isArray(this.GoogleReviewResponse.GoogleReview.Content)
                    ) {
                        return [];
                    }

                    var ratingLimit = parseInt(
                        this.GoogleReviewResponse.GoogleReview.RatingLimit &&
                            this.GoogleReviewResponse.GoogleReview.RatingLimit._ratingLimit
                            ? this.GoogleReviewResponse.GoogleReview.RatingLimit._ratingLimit
                            : 0
                    );

                    return this.GoogleReviewResponse.GoogleReview.Content.filter(function (review) {
                        if (!review || review.Rating == null) return false;
                        var ratingValue = parseInt(review.Rating);
                        return !isNaN(ratingValue) && ratingValue >= ratingLimit;
                    });
                },

                // ✅ Total count of ALL reviews
                totalReviewCount: function () {
                    if (
                        !this.GoogleReviewResponse ||
                        !this.GoogleReviewResponse.GoogleReview
                    ) {
                        return 0;
                    }

                    // Prefer API-provided count
                    if (this.GoogleReviewResponse.GoogleReview.TotalReviewCount) {
                        return this.GoogleReviewResponse.GoogleReview.TotalReviewCount;
                    }
                },

                // ✅ Average rating of ALL reviews
                overallAverageRating: function () {
                    if (
                        !this.GoogleReviewResponse ||
                        !this.GoogleReviewResponse.GoogleReview
                    ) {
                        return 0;
                    }

                    // Prefer API-provided value
                    if (this.GoogleReviewResponse.GoogleReview.AverageRating) {
                        return parseFloat(this.GoogleReviewResponse.GoogleReview.AverageRating).toFixed(2);
                    }
                },

                // Filtered review count (4+ stars)
                reviewCount: function () {
                    return this.filteredReviews.length;
                },

                // Average rating for filtered reviews
                averageRating: function () {
                    if (this.filteredReviews.length === 0) return 0;

                    var total = this.filteredReviews.reduce(function (sum, review) {
                        if (!review || review.Rating == null) return sum;
                        var val = parseInt(review.Rating);
                        return sum + (isNaN(val) ? 0 : val);
                    }, 0);

                    return (total / this.filteredReviews.length).toFixed(2);
                }
            },
            methods: {
                // Fetch Google Reviews
                getGoogleReviews: function () {
                    var self = this;

                    axios.post(addGoogleReviewsAPIUrl)
                        .then(function (response) {
                            self.GoogleReviewResponse = response.data;

                            // ⭐ INIT SWIPER ONLY AFTER DATA IS AVAILABLE
                            self.$nextTick(() => {
                                new Swiper(".myReviewSwiper", {
                                    slidesPerView: 3,
                                    spaceBetween: 25,
                                    loop: false,
                                    navigation: {
                                        nextEl: ".swiper-button-next",
                                        prevEl: ".swiper-button-prev"
                                    },
                                    pagination: {
                                        el: ".swiper-pagination",
                                        clickable: true
                                    },
                                    breakpoints: {
                                        0: { slidesPerView: 1 },
                                        600: { slidesPerView: 2 },
                                        1000: { slidesPerView: 3 }
                                    }
                                });
                            });
                        })
                        .catch(function (error) {
                            console.error("Error fetching Google Reviews:", error);
                        });
                },

                wordCount: function (text) {
                    if (!text) return 0;
                    return text.trim().split(/\s+/).length;
                },


                firstWords: function (text, n) {
                    if (!text) return '';
                    return text.trim().split(/\s+/).slice(0, n).join(' ');
                },


                showModal: function (review) {
                    this.modalReview = review;
                    $('#ShowReviewSummary').modal('show');
                }
            },
            mounted: function () {
                var self = this;
                this.$nextTick(function () {
                    self.getGoogleReviews();
                });
            }
        });
    }

});


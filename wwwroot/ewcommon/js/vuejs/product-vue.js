
    document.addEventListener("DOMContentLoaded", function () {

    var addGoogleReviewsAPIUrl = '/ewapi/Cms.Content/GetGoogleReviews';

    var isGoogleReview = document.querySelector(".Reviews");
    if (isGoogleReview) {
        new Vue({
            el: '.Reviews',
            data: {
                GoogleReviewResponse: null,
                modalReview: null
            },
            computed: {
                //  Filtered reviews based on rating limit (safe checks)
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
                        //  Skip null or invalid reviews safely
                        debugger;
                        if (!review || review.Rating == null) return false;
                        var ratingValue = parseInt(review.Rating);
                        return !isNaN(ratingValue) && ratingValue >= ratingLimit;
                    });
                },

                //  Count of reviews meeting rating condition
                reviewCount: function () {
                    return this.filteredReviews.length;
                },

                // Average rating with safe reduce
                averageRating: function () {
                    if (this.filteredReviews.length === 0) return 0;

                    var total = this.filteredReviews.reduce(function (sum, review) {
                        debugger
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
                    var apiurl = document.getElementById("googleAPIUrl")
                        ? document.getElementById("googleAPIUrl").value
                        : null;

                    if (apiurl) {
                        var inputJson = { apiurl: apiurl };
                        axios.post(addGoogleReviewsAPIUrl, inputJson)
                            .then(function (response) {
                                debugger;
                                console.log("Google Reviews Response:", response.data);
                                self.GoogleReviewResponse = response.data;
                            })
                            .catch(function (error) {
                                console.error("Error fetching Google Reviews:", error);
                            });
                    } else {
                        console.warn("Missing Google API URL value.");
                    }
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


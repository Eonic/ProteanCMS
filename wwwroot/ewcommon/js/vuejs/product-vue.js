var addGoogleReviewsAPIUrl = '/ewapi/Cms.Content/GetGoogleReviews';

const isGoogleReview = document.querySelector(".Reviews");
if (isGoogleReview) {
    new Vue({
        el: '.Reviews',
        data: {
            GoogleReviewResponse: null,
            modalReview: null 
        },
        computed: {
            // Filtered reviews with rating > 4
            filteredReviews() {
                
                if (!this.GoogleReviewResponse || !this.GoogleReviewResponse.GoogleReview) return [];

                const ratingLimit = parseInt(this.GoogleReviewResponse.GoogleReview.RatingLimit._ratingLimit);

                return this.GoogleReviewResponse.GoogleReview.Content
                    .filter(review => parseInt(review.Rating) >= ratingLimit)
                    .sort((a, b) => b.time - a.time);
            },
            // Count of reviews > 4
            reviewCount() {
                return this.filteredReviews.length;
            },
            // Average rating of reviews > 4
            averageRating() {
                if (this.filteredReviews.length === 0) return 0;
                const total = this.filteredReviews.reduce(
                    (sum, review) => sum + parseInt(review.Rating),
                    0
                );
                return (total / this.filteredReviews.length).toFixed(2);
            }
        },
        methods: {
            getGoogleReviews: function () {
                var self = this;
                var apiurl = $("#googleAPIUrl").val();
                if (apiurl) {
                    var inputJson = { apiurl: apiurl };
                    axios.post(addGoogleReviewsAPIUrl, inputJson)
                        .then(function (response) {
                           
                            self.GoogleReviewResponse = response.data;
                            return self.GoogleReviewResponse;
                        });
                }
            },
            // Count words in a string
            wordCount(text) {
                if (!text) return 0;
                return text.trim().split(/\s+/).length;
            },
            // Get first N words
            firstWords(text, n) {
                if (!text) return '';
                return text.trim().split(/\s+/).slice(0, n).join(' ');
            },
            // Show modal with full review
            showModal(review) {
               
                this.modalReview = review;  // store the clicked review for display
                $('#ShowReviewSummary').modal('show'); 
                
            }
        },
        mounted: function () {
            this.$nextTick(() => {
                this.getGoogleReviews();
            });
        }
    });
}

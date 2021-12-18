
$(window).load(function () {
    $('.matchHeight-body .grid-item').matchHeight();
    $('.matchHeight-body .listItem').matchHeight();

    PageContentActions();
    matchHeightResponsive();
});
$(window).resize(function () {
    $(".matchHeight").css('height', 'auto');
    $("div[class$='-body'][class!='media-body'],.list-group").css('height', 'auto');
    matchHeightResponsive();
    // autoAdjustFloatingColumns();
    $(".content-scroller .cols").each(function () {
        var equalHeight = $(this).data("height");
        if (equalHeight === true) {
            $(this).find(".lIinner").css('height', 'auto');
            setTimeout(function () {
                matchHeightScroller();
            }, 1000);
        }
    });
});
function PageContentActions() {
    contentScroller();
}
function contentScroller() {

    $(".content-scroller .cols").each(function () {
        var slidestoShow = $(this).data("slidestoshow");
        var xsSlides = $(this).data("xscols");
        var smSlides = $(this).data("smcols");
        var mdSlides = $(this).data("mdcols");
        var autoplay = $(this).data("autoplay");
        var autoplaySpeed = $(this).data("autoplayspeed");
        var equalHeight = $(this).data("height");
        var vCssEase = ($(this).data("cssease") === undefined ? "ease" : $(this).data("cssease"));
        var vSpeed = ($(this).data("speed") === undefined ? "300" : $(this).data("speed"));
        var breakpoint = 768;
        var dots = $(this).data("dots");
        $(this).on('init', function (event, slick) {
            // alert(equalHeight);
            if (equalHeight === undefined || equalHeight === true) {
                var highestBox = 0;
                //MATCH HEIGHT FOR CONTENT SCROLLER
                //find highest item in current section
                $(this).find(".lIinner, .grid-item").each(function () {
                    if ($(this).outerHeight() > highestBox) {
                        highestBox = $(this).outerHeight();
                    }
                });
                // alert('highestbox=' + highestBox);
                //add heights to items
                $(this).find(".lIinner").outerHeight(highestBox);
                $(this).find(".slick-slide").outerHeight(highestBox);
            }
        });
        $(this).not('.slick-initialized').slick({
            dots: dots,
            infinite: true,
            slidesToShow: slidestoShow,
            slidesToScroll: 1,
            speed: vSpeed,
            autoplay: autoplay,
            autoplaySpeed: autoplaySpeed,
            cssEase: vCssEase,
            responsive: [
                {
                    breakpoint: 575,
                    settings: {
                        slidesToShow: 1,
                        slidesToScroll: 1
                    }
                },
                {
                    breakpoint: breakpoint,
                    settings: {
                        slidesToShow: xsSlides,
                        slidesToScroll: 1
                    }
                },
                {
                    breakpoint: 991,
                    settings: {
                        slidesToShow: smSlides,
                        slidesToScroll: 1
                    }
                },
                {
                    breakpoint: 1199,
                    settings: {
                        slidesToShow: mdSlides,
                        slidesToScroll: 1
                    }
                }
            ]
        });

    });
}
/*  ==  Scripts for Both the site and the admin system for all websites and themes  =====================  */

$(document).ready(function () {
    //$(".navbar-brand").click(function () {
    //    $(this).toggleClass('show-click');
    //});
    $(".navbar-brand").on('click', function () {
        $(this).toggleClass('show-click');
    });
});

$(window).on("load", function () {
  //  $('.matchHeight-body .grid-item').matchHeight();
  //  $('.matchHeight-body .listItem').matchHeight();
    PageContentActions();
    matchHeightResponsive();
})

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

/*  ==  EXTEND JQUERY  ============================================================================  */

// Simple .exists() function - $(selector).exists(); - return true or false
jQuery.fn.exists = function () { return jQuery(this).length > 0; }

// EXTENTION TO GET ALL URL PARAMS IN AN OBJECT
$.extend({
    getURLParams: function () {
        var vars = [], hash;
        var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
        for (var i = 0; i < hashes.length; i++) {
            hash = hashes[i].split('=');
            vars.push(hash[0]);
            vars[hash[0]] = hash[1];
        }
        return vars;
    },
    getUrlParam: function (name) {
        return $.getURLParams()[name];
    }
});

/*  ==  END EXTEND JQUERY  ========================================================================  */

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

function matchHeightResponsive() {
    $("section").each(function () {
        var highestBox = 0;

        //MATCH HEIGHT FOR PANELS, ALERTS AND WELLS
        //find highest box in currrent section
        $(this).find(".matchHeight").each(function () {
            if ($(this).outerHeight() > highestBox) {
                highestBox = $(this).outerHeight();
            }
        });

        //add heights to boxes
        $(this).find(".matchHeight").outerHeight(highestBox);
        //give body a height, to line up footers
        $(this).find(".matchHeight").each(function () {
            var headingHeight = $(this).find("div[class*='-heading']").outerHeight();
            var footerHeight = $(this).find("div[class*='-footer']").outerHeight();
            var imageHeight = $(this).find("div[class*='-image']").outerHeight();
            var bodyHeight = highestBox - (headingHeight + footerHeight + imageHeight);
            $(this).find("div[class$='-body'][class!='media-body'],.list-group").outerHeight(bodyHeight);
        });

    });
}


function matchHeightScroller() {
    $("section:has(.content-scroller)").each(function () {
        var highestBox = 0;
        //MATCH HEIGHT FOR CONTENT SCROLLER
        //find highest item in current section
        $(this).find(".content-scroller .lIinner,.content-scroller .grid-item").each(function () {
            if ($(this).outerHeight() > highestBox) {
                highestBox = $(this).outerHeight();
            }
        });
        // alert('highestbox=' + highestBox);
        //add heights to items
        $(this).find(".content-scroller .lIinner").outerHeight(highestBox);
        $(this).find(".content-scroller .slick-slide").outerHeight(highestBox);
    });
}


function matchHeight(oGroup) {
    var tallest = 0;
    var innerTallest = 0;

    oGroup.each(function () {
        $(this).find('.lIinner').css({ 'height': 'auto' });
        thisHeight = parseInt($(this).height()) + parseInt($(this).css('padding-top').replace('px', '')) + parseInt($(this).css('padding-bottom').replace('px', ''));
        // alert($(this).css('padding-top'));
        if (thisHeight > tallest) {
            tallest = thisHeight;
        }
    });
    oGroup.each(function () {
        if (tallest > 0) {
            $(this).css({ 'height': tallest });
        } else {
            $(this).css({ 'height': 'auto' });
        }
    });
    oGroup.each(function () {
        if ($(this).find('.lIinner').length > 0) {
            thisInnerHeight = parseInt($(this).find('.lIinner').height()) + parseInt($(this).find('.lIinner').css('padding-top').replace('px', '')) + parseInt($(this).find('.lIinner').css('padding-bottom').replace('px', ''));
            // Also do innerHeight
            if (thisInnerHeight > innerTallest) {
                innerTallest = thisInnerHeight;
            }
        }
    });
    oGroup.each(function () {
        // Also do innerHeight
        if (innerTallest > 0) {
            $(this).find('.lIinner').css({ 'height': innerTallest });
        } else {
            $(this).find('.lIinner').css({ 'height': 'auto' });
        }
    });
}

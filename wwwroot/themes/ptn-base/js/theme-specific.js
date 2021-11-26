$(document).ready(function () {
    (function ($) {
        fakewaffle.responsiveTabs(['xs', 'sm']);
    })(jQuery);

    var footer_height = $('#pagefooter').height();

    $('#pagefooter').css('height', footer_height);
    $('#mainTable').css('margin-bottom', -footer_height);
    $('#mainTable').css('padding-bottom', footer_height);

    $(window).resize(function () {
        $('#pagefooter').css('height', 'auto');
        var footer_height = $('#pagefooter').height();

        $('#pagefooter').css('height', footer_height);
        $('#mainTable').css('margin-bottom', -footer_height);
        $('#mainTable').css('padding-bottom', footer_height);
    });
    $('.mobile-dd-control').click(function () {
        $(this).parent().find(".dropdown-menu").toggle();
        $(this).parent().find(".fa-angle-up").toggle();
        $(this).parent().find(".fa-angle-down").toggle();
    });

    
});
/*
|--------------------------------------------------------------------------
| EVENTS TRIGGER AFTER ALL IMAGES ARE LOADED
|--------------------------------------------------------------------------
*/
$(window).load(function () {
    /*
|--------------------------------------------------------------------------
| APPEAR
|--------------------------------------------------------------------------
*/

    if ($('.activateAppearAnimation').length) {

        AnimAppear();

        $('.reloadAnim').click(function (e) {

            $(this).parent().parent().find('img').removeClass().addClass('img-responsive');

            AnimAppear();
            e.preventDefault();
        });
    }
    if ($('.navbar-fixed-top').length) {
        fixedNav();
    }

    $('.sp-wrap').smoothproducts();

});
$(window).resize(function () {
    if ($('.navbar-fixed-top').length) {
        fixedNav();
    }
});
$(window).scroll(function () {
    if ($('.navbar-fixed-top').length) {
        if ($(this).scrollTop() > 1) {
            $('.navbar-fixed-top').addClass("navbar-fixed-smaller");
        }
        else {
            $('.navbar-fixed-top').removeClass("navbar-fixed-smaller");
        }
    }
});
/* FIXED NAV*/
function fixedNav() {
    //var headerHeight = $('header').height();
   // $('.fixed-nav-content').css('padding-top',headerHeight);
}
/* Appear function */
function AnimAppear() {
    $("[data-modAnim]").each(function () {

        var $this = $(this);

        //$this.addClass("moduleAnimate-invisible");

        if ($(window).width() > 767) {

            $this.appear(function () {

                var delay = ($this.data("modanimdelay") ? $this.data("modanimdelay") : 1);
                if (delay > 1) $this.css("animation-delay", delay + "ms");

                $this.addClass("moduleAnimate-animated");
                $this.addClass('moduleAnimate-' + $this.data("modanim"));

                setTimeout(function () {
                    $this.addClass("moduleAnimate-visible");
                }, delay);

            }, { accX: 0, accY: 0 });

        } else {
            $this.addClass("moduleAnimate-visible");
        }
    });
}
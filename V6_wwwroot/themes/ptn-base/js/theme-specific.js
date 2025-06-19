$(document).ready(function () {
    (function ($) {
        fakewaffle.responsiveTabs(['xs', 'sm']);
    })(jQuery);

    //accessible hover start
    $('.dropdown').on('show.bs.dropdown', function (e) {
        $(this).find('.dropdown-menu').first().stop(true, true).slideDown(100);
    });

    $('.dropdown').on('hide.bs.dropdown', function (e) {
        $(this).find('.dropdown-menu').first().stop(true, true).slideUp(100);
    });
    //accessible hover end

    mobileMenu();

    $('[data-bs-toggle="popover"]').popover();
    
});

/*
|--------------------------------------------------------------------------
| EVENTS TRIGGER AFTER ALL IMAGES ARE LOADED
|--------------------------------------------------------------------------
*/
$(window).on("load", function () {
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
    $("[data-modanim]").each(function () {
        var $this = $(this);

        //$this.addClass("moduleAnimate-invisible");

        if ($(window).width() > 767) {

            $this.appear(function () {

                var delay = ($this.data("modanimdelay") ? $this.data("modanimdelay") : 1);
                if (delay > 1) $this.css("animation-delay", delay + "ms");

                $this.addClass("moduleAnimate-animated");
                $this.addClass($this.data("modanim"));

                $this.addClass('animate__' + $this.data("modanim"));
                $this.addClass($this.data("modanimspeed"));

                //  alert($this.data("modanim") + ' ' + $this.data("modanimspeed"));

                setTimeout(function () {
                    $this.addClass("moduleAnimate-visible");
                }, delay);

            }, { accX: 0, accY: 0 });

        } else {
            $this.addClass("moduleAnimate-visible");
        }
    });
}
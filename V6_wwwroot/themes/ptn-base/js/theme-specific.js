$(document).ready(function () {
    (function ($) {
        fakewaffle.responsiveTabs(['xs', 'sm']);
    })(jQuery);

    $('.navbar-nav > .dropdown-hover-menu').hover(function () {
        $(this).find('.dropdown-menu').show();
        $(this).addClass('show dropdown-active');
    }, function () {
        $(this).find('.dropdown-menu').hide();
        $(this).removeClass('show dropdown-active');
        $(this).find('.dropdown-mobile-next > ul').hide();
    });
    
    $('.mobile-dd-control').click(function () {
        $(this).parent().find(".dropdown-menu").toggle();
        $(this).parent().find(".fa-angle-up").toggle();
        $(this).parent().find(".fa-angle-down").toggle();
    });
    $('.mainnav-toggler').click(function () {
        $('body').toggleClass('active-menu');
        $('.mainnav-collapse').slideDown('fast');
        $(this).toggleClass('active-nav-toggle');

        if ($(this).attr('aria-expanded') == 'true') {
            $(this).attr('aria-expanded', 'false');
        }
        else {
            $(this).attr('aria-expanded', 'true');
        }
    });
    $('.nav-close-btn').click(function () {
        $('.navbar-collapse').slideUp('fast');
        $('body').toggleClass('active-menu');
        $(".dropdown-active").removeClass('dropdown-active');
    });
    $('.dropdown-mobile-btn').click(function () {
        var currentHeight = $(this).parent().find("> .nav-pills").height();
        var newHeight = $(this).parent().parent().height();
        $(this).parent().find("> .nav-pills").addClass('dropdown-active');
        $(this).parent('li').parent('ul').addClass('menu-no-scroll');
        if (newHeight > currentHeight) {
            $(this).parent().find('.nav-pills').height(newHeight);
        }
    });
    $('.dropdown-toggle').click(function () {
        $(this).parent().find("> .dropdown-menu").addClass('dropdown-active');
        $(this).parent('li').parent('ul').addClass('menu-no-scroll');
    });
    $('.menu-back').click(function () {
        $(this).parent().removeClass('dropdown-active');
        $(this).parent('li').parent('ul').removeClass('menu-no-scroll');
    });
    
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
    $("[data-modAnim]").each(function () {

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
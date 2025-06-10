$(document).ready(function () {
    (function ($) {
        fakewaffle.responsiveTabs(['xs', 'sm']);
    })(jQuery);

    //$('.navbar-nav > .dropdown-hover-menu').hover(function () {
    //    $(this).find('.dropdown-menu').show();
    //    $(this).addClass('show dropdown-active');
    //}, function () {
    //    $(this).find('.dropdown-menu').hide();
    //    $(this).removeClass('show dropdown-active');
    //    $(this).find('.dropdown-mobile-next > ul').hide();
    //});
    //accessible hover start
    $('.dropdown').on('show.bs.dropdown', function (e) {
        $(this).find('.dropdown-menu').first().stop(true, true).slideDown(100);
    });

    $('.dropdown').on('hide.bs.dropdown', function (e) {
        $(this).find('.dropdown-menu').first().stop(true, true).slideUp(100);
    });
    //accessible hover end

    //$('.dropdown-menu').css('visibility', 'hidden');
    //MOBILE MENU

    //hide lower level menu screens from keyboards and screen readers
    if (window.matchMedia("(max-width: 992px)").matches) {
        $('.dropdown-menu').find('.dropdown-item').attr('tabindex', -1);
        $('.dropdown-menu').find('button').attr('tabindex', -1);
        $('.dropdown-menu').attr('aria-hidden', true);
    }

    //stop keyboard tab leaving menu
    var lastLi = $('.mainnav-collapse ul.navbar-nav li:last-child a');
    lastLi.on('keydown', function (e) {
        if (!e.shiftKey && e.key === 'Tab') {
            e.preventDefault();
            $(this).parent().parent().parent().find('.nav-close-btn').focus();
        }

    });

    //what does this bit do?
    $('.mobile-dd-control').click(function () {
        $(this).parent().find(".dropdown-menu").toggle();
        $(this).parent().find(".fa-angle-up").toggle();
        $(this).parent().find(".fa-angle-down").toggle();
    });

    //click hamburger to open menu
    $('.mainnav-toggler').click(function () {
        $('body').toggleClass('active-menu');
        $('.mainnav-collapse').slideDown('fast');
        $('.nav-close-btn').focus();
        $(this).toggleClass('active-nav-toggle');

        if ($(this).attr('aria-expanded') == 'true') {
            $(this).attr('aria-expanded', 'false');
        }
        else {
            $(this).attr('aria-expanded', 'true');
        }
    });

    //close whole menu
    $('.nav-close-btn').click(function () {
        $('.navbar-collapse').slideUp('fast');
        $('body').toggleClass('active-menu');
        $(".dropdown-active").removeClass('dropdown-active');
        $('.mainnav-toggler').focus();
    });
    var lastLi = $('.nav-close-btn');
    lastLi.on('keydown', function (e) {
        if (e.shiftKey && e.key === 'Tab') {
            e.preventDefault();
        }

    });

    //not sure what this bit does
    $('.dropdown-mobile-btn').click(function () {
        var currentHeight = $(this).parent().find("> .nav-pills").height();
        var newHeight = $(this).parent().parent().height();
        $(this).parent().find("> .nav-pills").addClass('dropdown-active');
        //$(this).parent().find("> .nav-pills").find('dropdown-active').attr("tabindex", 0)
        $(this).parent('li').parent('ul').addClass('menu-no-scroll');
        if (newHeight > currentHeight) {
            $(this).parent().find('.nav-pills').height(newHeight);
        }
    });

    //click link on top level list
    $('.dropdown-toggle').click(function () {
        $(this).parent().find("> .dropdown-menu").addClass('dropdown-active');
        $(this).parent('li').parent('ul').addClass('menu-no-scroll');
        //$(this).parent().find('.dropdown-menu').css('visibility', 'visible');
        if ($('> .dropdown-menu button').is(':visible')) {
            alert('i see you');
            $(this).parent().find("> .dropdown-menu button").focus();
        }
        //setTimeout(function () {
        //    $(this).parent().find("> .dropdown-menu button").focus();
        //    $(this).parent().find("> .dropdown-menu button").css('background', 'red');
        //    alert('done');
        //}, 1000); 
        $(this).parent().find("> .dropdown-menu .dropdown-item").attr('tabindex', 0);
        $(this).parent().find("> .dropdown-menu button").attr('tabindex', 0);

        $('.dropdown-menu').attr('aria-hidden', false);
        $(this).parent().find("> .dropdown-menu button").focus();
    });

    //back button on sub menus
    $('.menu-back').click(function () {
        //$(this).parent().parent().find('.dropdown-toggle').css('background', 'red');
        $(this).parent().parent().find('.dropdown-toggle').focus();
        $(this).parent().removeClass('dropdown-active');
        $(this).parent('li').parent('ul').removeClass('menu-no-scroll');
        $('.dropdown-menu').find('.dropdown-item').attr('tabindex', -1);
        $('.dropdown-menu').find('button').attr('tabindex', -1);
        $('.dropdown-menu').attr('aria-hidden', true);
        //$(this).parent().css('visibility', 'hidden');

    });
    var lastLi = $('.menu-back');
    lastLi.on('keydown', function (e) {
        if (e.shiftKey && e.key === 'Tab') {
            e.preventDefault();
        }

    });

    //stop tabbing from leaving mobile menu screen
    var lastLi = $('ul.dropdown-menu li:last-child a');
    lastLi.on('keydown', function (e) {
        if (e.key === 'Tab' || e.keyCode === 9) {
            e.preventDefault();
            $(this).parent().parent().find('button').focus();
        }

    });

    //END MOBILE MENU
    $('[data-bs-toggle="popover"]').popover();
    
});
//function moveFocus() {

//    $('.navbar-toggler').keypress(function (e) {

//        var key = e.which;

//        if (key == 13) {
//            $(this).parent().find('.nav-close-btn').addClass('test');
//            $(this).parent().find('.nav-close-btn').focus();
//        }
//    });
//}

//function closeBtnFocus() {
//}

//function trapFocus(element) {
//    var focusableEls = element.querySelectorAll('a[href]:not([disabled]), button:not([disabled]), textarea:not([disabled]), input[type="text"]:not([disabled]), input[type="radio"]:not([disabled]), input[type="checkbox"]:not([disabled]), select:not([disabled])');
//    var firstFocusableEl = focusableEls[0];
//    var lastFocusableEl = focusableEls[focusableEls.length - 1];
//    var KEYCODE_TAB = 9;

//    element.addEventListener('keydown', function (e) {
//        var isTabPressed = (e.key === 'Tab' || e.keyCode === KEYCODE_TAB);

//        if (!isTabPressed) {
//            return;
//        }

//        if (e.shiftKey) /* shift + tab */ {
//            if (document.activeElement === firstFocusableEl) {
//                lastFocusableEl.focus();
//                e.preventDefault();
//            }
//        } else /* tab */ {
//            if (document.activeElement === lastFocusableEl) {
//                firstFocusableEl.focus();
//                e.preventDefault();
//            }
//        }
//    });
//}

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
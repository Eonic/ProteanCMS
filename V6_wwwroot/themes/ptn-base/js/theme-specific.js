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

(function ($) {
    // Call this whenever the DOM might have (re)rendered Swiper controls
    function patchSwiperNav(context) {
        var $root = context ? $(context) : $(document);

        // Swiper default nav elements are often <div>. We patch both next/prev.
        $root.find('.swiper-button-next, .swiper-button-prev').each(function () {
            var $btn = $(this);

            // Avoid repeatedly binding
            if ($btn.data('kb-patched')) return;
            $btn.data('kb-patched', true);

            // Make it focusable + announce as a button
            // (Prefer not to override if the library already sets these.)
            if (!$btn.attr('tabindex')) $btn.attr('tabindex', '0');
            if (!$btn.attr('role')) $btn.attr('role', 'button');

            // Add a label if missing (helps SR users)
            if (!$btn.attr('aria-label')) {
                $btn.attr('aria-label', $btn.hasClass('swiper-button-next') ? 'Next slide' : 'Previous slide');
            }

            // If it's visually disabled, keep it out of tab order.
            // Swiper uses swiper-button-disabled on nav.
            if ($btn.hasClass('swiper-button-disabled')) {
                $btn.attr('aria-disabled', 'true').attr('tabindex', '-1');
            } else {
                // Only remove aria-disabled if we set it previously
                if ($btn.attr('aria-disabled') === 'true') $btn.removeAttr('aria-disabled');
                if ($btn.attr('tabindex') === '-1') $btn.attr('tabindex', '0');
            }

            // Key support: Enter/Space -> click
            $btn.on('keydown.swiperKbPatch', function (e) {
                var key = e.key || e.keyCode;

                var isEnter = key === 'Enter' || key === 13;
                var isSpace = key === ' ' || key === 'Spacebar' || key === 32;

                // Prevent page scroll on Space
                if (isSpace) e.preventDefault();

                if ((isEnter || isSpace) && !$(this).hasClass('swiper-button-disabled')) {
                    e.preventDefault();
                    // Trigger Swiper’s existing click handler
                    $(this).trigger('click');
                }
            });
        });
    }

    // Initial run
    $(function () {
        patchSwiperNav();

        // Re-apply when Swiper / the library mutates the DOM (common with auto updates)
        // MutationObserver is better than polling and doesn’t require changing the library.
        if (window.MutationObserver) {
            var observer = new MutationObserver(function (mutations) {
                // Patch only if relevant nodes changed (cheap filter)
                for (var i = 0; i < mutations.length; i++) {
                    var m = mutations[i];
                    if (m.addedNodes && m.addedNodes.length) {
                        patchSwiperNav(document);
                        break;
                    }
                }
            });

            observer.observe(document.documentElement, { childList: true, subtree: true });
        } else {
            // Fallback: periodic patch (older browsers)
            setInterval(function () { patchSwiperNav(); }, 1000);
        }

        // Also handle Swiper toggling disabled class after slide changes.
        // A lightweight way: watch for focus/click and re-evaluate.
        $(document).on('focus mouseenter click', '.swiper-button-next, .swiper-button-prev', function () {
            patchSwiperNav(document);
        });
    });
})(jQuery);
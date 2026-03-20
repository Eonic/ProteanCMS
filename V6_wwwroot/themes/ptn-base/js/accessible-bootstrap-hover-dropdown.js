/**

 * Project: Vic's Accessible Bootstrap Hover Dropdown
 
 * Homepage: http://cameronspear.com/blog/bootstrap-dropdown-on-hover-plugin/
 */
; (function ($, window, undefined) {
    // outside the scope of the jQuery plugin to
    // keep track of all dropdowns
    var $allDropdowns = $();

    // if instantlyCloseOthers is true, then it will instantly
    // shut other nav items when a new one is hovered over
    $.fn.dropdownHover = function (options) {
        // don't do anything if touch is supported
        // (plugin causes some issues on mobile)
        if ('ontouchstart' in document) return this; // don't want to affect chaining

        // the element we really care about
        // is the dropdown-toggle's parent
        $allDropdowns = $allDropdowns.add(this.parent());

        return this.each(function () {
            var $this = $(this),
                $parent = $this.parent(),
                defaults = {
                    delay: 400,
                    hoverDelay: 200,
                    instantlyCloseOthers: true
                },
                data = {
                    delay: $(this).data('delay'),
                    //hoverDelay: $(this).data('hover-delay'),
                    instantlyCloseOthers: $(this).data('close-others')
                },
                hideEvent = 'hide.bs.dropdown',
                settings = $.extend(true, {}, defaults, options, data),
                timeout,
                timeoutHover;

            $parent.hover(function (event) {
                //window.clearTimeout(timeoutHover);
                $this.parents('.navbar-nav').find('.dropdown').removeClass('show');
                $this.parents('.navbar-nav').find('.dropdown-menu').removeClass('show');
                $this.parents('.navbar-nav').find('.nav-link').attr('aria-expanded', false);
         
                //$this.attr('aria-expanded', 'true');
                //$parent.find('.dropdown-menu').first().stop(true, true).slideDown(100);
                //$parent.addClass('show');
                openDropdown(event);
            }, function () {
                // clear timer for hover event
                window.clearTimeout(timeoutHover)
                timeout = window.setTimeout(function () {
                    $this.attr('aria-expanded', false);
                    $parent.removeClass('show');
                    $parent.find('.dropdown-menu').removeClass('show');
                    $this.trigger(hideEvent);
                }, settings.delay);
            });

            function openDropdown(event) {

                // clear dropdown timeout here so it doesnt close before it should
                window.clearTimeout(timeout);
                // restart hover timer
                window.clearTimeout(timeoutHover);

                // delay for hover event.  
                timeoutHover = window.setTimeout(function () {
                    $allDropdowns.find(':focus').blur();

                    if (settings.instantlyCloseOthers === true)
                        $allDropdowns.removeClass('show');

                    // clear timer for hover event
                    window.clearTimeout(timeoutHover);
                    $parent.find('.nav-link').attr('aria-expanded', true);
                    $this.addClass('hover-active');
                    $parent.addClass('show');
                    $parent.find('.dropdown-menu').first().stop(true, true).slideDown(100);                    
                }, settings.hoverDelay);
            }
        });
    };

    $(document).ready(function () {
        $('[data-hover="dropdown"]').dropdownHover();
    });

    $(document).on('keyup', function (evt) {
        if (evt.keyCode == 27) {
            $('.navbar-nav .dropdown-menu').hide();
            $('.navbar-nav .nav-item').removeClass('show');
            $('.navbar-nav .nav-item .nav-link').attr('aria-expanded', false);
        }
    });
})(jQuery, window);
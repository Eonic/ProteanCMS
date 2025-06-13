function mobileMenu() {

    //hide lower level menu screens from keyboards and screen readers
    if ($(window).width() > 991) {
        $('.dropdown-menu').find('.dropdown-item').attr('tabindex', -1);
        $('.dropdown-menu').find('button').attr('tabindex', -1);
        $('.dropdown-menu').attr('aria-hidden', true);
    }

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

    //stop keyboard tab leaving main menu
    if ($(window).width() < 992) {
        // send last item back to close button when tab pressed
        var lastLiMain = $('ul.navbar-nav li:last-child a');
        lastLiMain.on('keydown', function (e) {
            if (e.key === 'Tab') {
                e.preventDefault();
                $(this).parent().parent().parent().find('.nav-close-btn').focus();
            }
        });

        // stop shift + tab working on menu close button
        var lastLiClose = $('.nav-close-btn');
        lastLiClose.on('keydown', function (e) {
            if (e.shiftKey && e.key === 'Tab') {
                e.preventDefault();
            }

        });
    }

    //click link on top level list
    $('.dropdown-toggle').click(function () {
        $(this).parent().find("> .dropdown-menu").addClass('dropdown-active');
        $(this).parent('li').parent('ul').addClass('menu-no-scroll');
        if ($('> .dropdown-menu button').is(':visible')) {
            alert('i see you');
            $(this).parent().find("> .dropdown-menu button").focus();
        }

        $(this).parent().find("> .dropdown-menu .dropdown-item").attr('tabindex', 0);
        $(this).parent().find("> .dropdown-menu button").attr('tabindex', 0);

        $('.dropdown-menu').attr('aria-hidden', false);
        $(this).parent().find("> .dropdown-menu button").focus();
    });

//SUB MENUS
    //back button on sub menus
    $('.menu-back').click(function () {
        $(this).parent().parent().find('.dropdown-toggle').focus();
        $(this).parent().removeClass('dropdown-active');
        $(this).parent('li').parent('ul').removeClass('menu-no-scroll');
        $('.dropdown-menu').find('.dropdown-item').attr('tabindex', -1);
        $('.dropdown-menu').find('button').attr('tabindex', -1);
        $('.dropdown-menu').attr('aria-hidden', true);

    });

    //on sub menus, stop user tabbing out of menu
    if ($(window).width() < 992) {
        //stop shift + tab working when on menu back button
        var lastLi = $('.menu-back');
        lastLi.on('keydown', function (e) {
            if (e.shiftKey && e.key === 'Tab') {
                e.preventDefault();
            }
        });

        //stop tab on last sub menu item
        var lastLi = $('ul.dropdown-menu li:last-child a');
        lastLi.on('keydown', function (e) {
            if (e.key === 'Tab' || e.keyCode === 9) {
                e.preventDefault();
                $(this).parent().parent().find('button').focus();
            }
        });
    }

    //is this code for third level menus?
    $('.dropdown-mobile-btn').click(function () {
        var currentHeight = $(this).parent().find("> .nav-pills").height();
        var newHeight = $(this).parent().parent().height();
        $(this).parent().find("> .nav-pills").addClass('dropdown-active');
        $(this).parent('li').parent('ul').addClass('menu-no-scroll');
        if (newHeight > currentHeight) {
            $(this).parent().find('.nav-pills').height(newHeight);
        }
    });
}
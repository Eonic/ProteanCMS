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
    $('form.xform').prepareXform();
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

/*
---------------------- Handle Eonic Xforms-------------------------
*/
$.fn.prepareXform = function () {

    //---- Hide feilds using Javascript

    $(this).find('input.jsHide').each(function () {
        $(this).parent().addClass('hidden');
    });

    $(this).find('div.jsHide').each(function () {
        $(this).addClass('hidden');
    });


    //---------------------- Datepicker ----------------------------

    $(this).find('input.hasDatepicker').datepicker('destroy');
    $(this).find('input.hasDatepicker').removeClass('hasDatepicker');


    //---------------------- DOBpicker ----------------------------
    if ($(this).find('input.jqDOBPicker').exists()) {
        $.datepicker.setDefaults($.datepicker.regional['']);

        $(this).find('input.jqDOBPicker').each(function (i) {

            $(this).datepicker({
                closeAtTop: false,

                closeText: 'x',
                showButtonPanel: true,
                showOn: "focus",
                dateFormat: 'd M yy',
                altField: '#' + $(this).attr('id').replace('-alt', ''),
                altFormat: 'yy-mm-dd',
                mandatory: $(this).hasClass('required'),
                changeMonth: true,
                changeYear: true,
                yearRange: '-95:+0'
            });
        });
    };

    //    var datePickerSettings = ;

    if ($(this).find('input.jqDatePicker').exists()) {
        $.datepicker.setDefaults($.datepicker.regional['']);
        $(this).find('input.jqDatePicker').each(function (i) {
            $(this).datepicker({
                closeAtTop: false,
                closeText: 'x',
                showButtonPanel: true,
                showOn: "focus",
                dateFormat: 'd M yy',
                altField: '#' + $(this).attr('id').replace('-alt', ''),
                altFormat: 'yy-mm-dd',
                mandatory: $(this).hasClass('required')
            });
        });
    };

    if ($(this).find('.btn-file').exists()) {

        $(document).on('change', '.btn-file :file', function () {
            var input = $(this),
                numFiles = input.get(0).files ? input.get(0).files.length : 1,
                label = input.val().replace(/\\/g, '/').replace(/.*\//, '');
            input.trigger('fileselect', [numFiles, label]);
        });

        $(document).ready(function () {
            $('.btn-file :file').on('fileselect', function (event, numFiles, label) {

                var input = $(this).parents('.input-group').find(':text'),
                    log = numFiles > 1 ? numFiles + ' files selected' : label;

                if (input.length) {
                    input.val(log);
                } else {
                    if (log) alert(log);
                }

            });
        });
    };

    //---------------------- Slider ----------------------------

    $(this).find('div.slider').each(function (i) {
        $(this).slider({
            range: 'min',
            value: parseInt($(this).children('span.val').text()),
            min: parseInt($(this).children('span.min').text()),
            max: parseInt($(this).children('span.max').text()),
            step: parseInt($(this).children('span.step').text()),
            slide: function (event, ui) {
                $("#" + $(this).children('span.ref').text()).val(ui.value);
            }
        });
    });

    /*----------------------Colourpicker----------------------------*/

    if ($(this).find('input.colorPicker').exists()) {
        $(this).find('input.colorPicker').ColorPickerSliders({
            hsvpanel: true,
            previewformat: 'hex',
            placement: 'bottom',
            size: 'large'
        });
    }
    /*----------------------GoogleProductCategories----------------------------*/


    $(this).find('textarea[maxlength]').keyup(function () {
        //get textarea text and maxlength attribute value
        var t = $(this);
        var text = t.val();
        var limit = t.attr('maxlength');
        //if textarea text is greater than maxlength limit, truncate and re-set text
        if (text.length > limit) {
            text = text.substring(0, limit);
            t.val(text);
        }
    });

    $(this).find("textarea[class^='maxwords-'],textarea[class*=' maxwords-']").each(function () {
        var t = $(this);
        var sClass = t.attr('class');
        var maxwords = parseInt(/maxwords-(\d+)/.exec(sClass)[1], 10);
        $(this).textareaCounter({
            limit: maxwords
        });
    })

    /*----------------------StrongPasswords----------------------------*/
    if (typeof password_strength == 'function') {
        var myPSPlugin = $("input.strongPassword").password_strength();
    }

    $(this).find("input.strongPassword").closest("form").find(".button").click(function () {

        if (myPSPlugin.metReq()) {
            return true;
        }
        else {
            //alert("Password not strong engough!")
            displayErrorMessage('Password not strong engough!', 'fa fa-info-circle');
            return false;
        }
        //return myPSPlugin.metReq(); //return true or false
    });

    $(this).find("[id$='passwordPolicy']").click(function (event) {
        var width = 200, height = 400, left = (screen.width / 2) - (width / 2),
            top = (screen.height / 2) - (height / 2);
        window.open("/ewcommon/tools/passwordpolicydisplay.ashx", 'Password_policy', 'width=' + width + ',height=' + height + ',left=' + left + ',top=' + top);
        event.preventDefault();
        return false;
    });

    if ($(this).find('input#onLoad').exists()) {
        $(this).find('input#onLoad').each(function (i) {
            var t = $(this);
            var text = t.val();
            // alert(text);
            eval(text);
        });
    };

    if ($(this).find('select.submit-on-select').exists()) {
        $(this).find('select.submit-on-select').each(function (i) {
            $(this).change(function () {
                $(this).closest('form').submit();
            });
        });
    }

    if ($(this).find('.contentLocations').exists()) {
        $(this).find('.contentLocations').each(function (i) {
            var classString = $(this).attr('class').match(/([^\?]*)pickLimit\-(\d*)/);
            var cbLimit = classString[2];
            //  alert(cbLimit);
            $(this).accordion({
                header: 'legend',
                collapsible: true,
                autoHeight: false
            });
            $(this).prepend('<div class="selectedValues"></div>');
            var $checkboxes_to_limit = $('input', this);
            $('input', this).each(function (i) {
                if ($(this).is(':checked')) {
                    // add to selected values
                    $(this).parent().children('label').clone().insertAfter($(this).closest('.contentLocations').children('.selectedValues').children('h4'))

                    if (cbLimit > 0) {
                        if ($checkboxes_to_limit.filter(":checked").length >= cbLimit) {
                            $checkboxes_to_limit.not(":checked").attr("disabled", "disabled");
                        }
                        else {
                            $checkboxes_to_limit.removeAttr("disabled");
                        }
                    }
                }
                $(this).click(function (i) {
                    //alert($(this).clone().wrap('<div></div>').parent().html());
                    //$(this).closest('.contentLocations').prepend($(this).parent().clone().wrap('<div></div>').parent().html())
                    if ($(this).is(':checked')) {
                        // add to selected values
                        $(this).parent().children('label').clone().insertAfter($(this).closest('.contentLocations').children('.selectedValues').children('h4'))
                        if (cbLimit > 0) {
                            if ($checkboxes_to_limit.filter(":checked").length >= cbLimit) {
                                $checkboxes_to_limit.not(":checked").attr("disabled", "disabled");
                            }
                            else {
                                $checkboxes_to_limit.removeAttr("disabled");
                            }
                        }
                    }
                    else {
                        // remove from selectedValues
                        var forValue = $(this).parent().children('label').attr('for');
                        //alert(forValue);
                        $(this).closest('.contentLocations').children('.selectedValues').children('label[for*="' + forValue + '"]').remove();
                        if (cbLimit > 0) {
                            if ($checkboxes_to_limit.filter(":checked").length >= cbLimit) {
                                $checkboxes_to_limit.not(":checked").attr("disabled", "disabled");
                            }
                            else {
                                $checkboxes_to_limit.removeAttr("disabled");
                            }
                        }
                    }
                });
            });
        });
    };

};
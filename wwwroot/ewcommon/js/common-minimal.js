// (c) Eonic Digital LLP. 2002-2020
// Authority: Trevor Spink

var obj = null;

// Simple .exists() function - $(selector).exists(); - return true or false
jQuery.fn.exists = function () { return jQuery(this).length > 0; }

/* MAIN PAGE READY METHOD or All site, All pages - Keep Smart! */
$(document).ready(function () {
    cleanDatepicker();
    initialiseXforms();
    contentSwiper();

    $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
        $('.content-scroller .cols').resize();
        // alert('hi');
    })

   // Run ewAfterCommon if it exists
    if (typeof ewAfterCommon == 'function') {
        ewAfterCommon();
    }
    // Give the first form element focus
    // $('#mainLayout').find(':input:visible:enabled:first:').focus();
    if (typeof popover == 'function') {
        if ($(".mypopover").exists()) {
            $(".mypopover").popover({ trigger: 'hover' })
        }

        $('[rel=frmPopover]').popover({
            html: true,
            trigger: 'hover',
            content: function () {
                return $($(this).data('contentwrapper')).html();
            }
        });
    }

    $('.accordion-module .panel-heading a').click(function () {
        $(this).removeClass('accordion-load');
        $(this).removeClass('accordion-open');
    });

    $('.scroll-to-anchor').bind('click', function (event) {
        var $anchor = $(this);
        $('html, body').stop().animate({
            scrollTop: ($($anchor.attr('href')).offset().top - 50)
        }, 1250, 'easeInOutExpo');
        event.preventDefault();
    });
});

/*
|--------------------------------------------------------------------------
| EVENTS TRIGGER AFTER ALL IMAGES ARE LOADED
|--------------------------------------------------------------------------
*/
$(window).on("load",function () {
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
function contentSwiper() {
    $(".swiper").each(function () {

        var padding = $(this).parent().find('.row span').css("padding-left");
        if (padding != undefined) {
            padding = padding.substring(0, padding.length - 2);
        } else {
            padding = 0;
        }

        var swiperId = $(this).data("id");
        var slidestoShow = $(this).data("slidestoshow");
        var defaultSlides = 1;
        var xsSlides = $(this).data("xscol");
        if (xsSlides === '') { xsSlides = defaultSlides } else { defaultSlides = xsSlides };
        var smSlides = $(this).data("smcol");
        if (smSlides === '') { smSlides = defaultSlides } else { defaultSlides = smSlides };
        var mdSlides = $(this).data("mdcol");
        if (mdSlides === '') { mdSlides = defaultSlides } else { defaultSlides = mdSlides };
        var lgSlides = $(this).data("lgcol");
        if (lgSlides === '') { lgSlides = defaultSlides } else { defaultSlides = lgSlides };
        var xlSlides = $(this).data("xlcol");
        if (xlSlides === '') { xlSlides = defaultSlides } else { defaultSlides = xlSlides };
        var xxlSlides = $(this).data("xxlcol");
        if (xxlSlides === '') { xxlSlides = defaultSlides };

        // alert(swiperId + "default" + defaultSlides + "," + xxlSlides + "," + lgSlides + "," + mdSlides + "," + mdSlides + "," + smSlides + "," + xsSlides);


        var lgHeight = $(this).data("lgHeight");
        var spacebetween = parseInt(padding) * 2;
        var spacebetweenlg = parseInt(padding) * 2;
        //var spacebetweenxs = $(this).data("spacebetweenxs");
        //var spacebetweenlg = $(this).data("spacebetweenlg");

        var objAutoplay = $(this).data("autoplay");
        var autoplaySpeed = $(this).data("autoplayspeed");
        if (objAutoplay == false) {
            objAutoplay = undefined
        }
        else {
            objAutoplay = { delay: autoplaySpeed }
        };

        var equalHeight = $(this).data("height");
        var vCssEase = ($(this).data("cssease") === undefined ? "ease" : $(this).data("cssease"));
        var vSpeed = ($(this).data("speed") === undefined ? 300 : $(this).data("speed"));
        var vDirection = ($(this).data("direction") === undefined ? 'horizontal' : $(this).data("direction"));
        var vEffect = ($(this).data("effect") === undefined ? undefined : $(this).data("effect"));
        var vFadeEffect = undefined;
        if (vEffect === 'fade') {
            vFadeEffect = { crossFade: true }
        }
        var breakpoint = 768;
        var dots = $(this).data("dots");
        if (dots == true) { dots = false };
        const swiper = new Swiper(this, {
            // Optional parameters
            slidesPerView: xsSlides,
            spaceBetween: spacebetween,
            loop: true,
            speed: vSpeed,
            direction: vDirection,
            effect: vEffect,
            fadeEffect: vFadeEffect,
            loopFillGroupWithBlank: true,
            watchOverflow: true,
            autoplay: objAutoplay,
            stopOnLastSlide: false,
            pagination: {
                el: "#swiper-pagination-" + swiperId,
                clickable: true,
            },
            navigation: {
                nextEl: "#swiper-button-next-" + swiperId,
                prevEl: "#swiper-button-prev-" + swiperId,
            },
            breakpoints: {
                576: {
                    slidesPerView: smSlides,
                },
                768: {
                    slidesPerView: mdSlides,
                },
                992: {
                    slidesPerView: lgSlides,
                    spaceBetween: spacebetweenlg,
                },
                1200: {
                    slidesPerView: xlSlides,
                    spaceBetween: spacebetweenlg,
                },
                1400: {
                    slidesPerView: xxlSlides,
                    spaceBetween: spacebetweenlg,
                },
            },
        });
        $(this).addClass('swiper-loaded');

    });

}
function PageContentActions() {

    if (typeof universalParallax === 'function') {
        var parallaxspeed = $('.parallax-wrapper').last().data("parallax-speed");
        if (parallaxspeed === '') {
            parallaxspeed = 1;
        }
        new universalParallax().init({
            speed: parallaxspeed
        });
    }
    contentScroller();
}

function contentScroller() {

    //CAN WE USE MATCH HEIGHT SCROLLER HERE? WHY IS THIS DIFFERENT ON WINDOWS RESIZE?
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

var resizedoit;

$(window).resize(function () {
    clearTimeout(resizedoit);
    //  resizedoit = setTimeout(navAddMoreToFit(), 500);
});

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

function initialiseXforms() {
    if ($("form.ewXform").exists()) {
        $('form.ewXform').prepareXform();

        if ($.browser.msie && $.browser.version <= 9 || $.browser.opera) {
            $("input[placeholder], textarea[placeholder]").each(function () {
                var val = $(this).attr("placeholder");
                if (this.value == "") {
                    this.value = val;
                }
                $(this).focus(function () {
                    if (this.value == val) {
                        this.value = "";
                    }
                }).blur(function () {
                    if ($.trim(this.value) == "") {
                        this.value = val;
                    }
                })
            });

            // Clear default placeholder values on form submit
            $('form').submit(function () {
                $(this).find("input[placeholder], textarea[placeholder]").each(function () {
                    if (this.value == $(this).attr("placeholder")) {
                        this.value = "";
                    }
                });
            });
        }
    }

    if ($("#Secure3D").exists()) {
        var encoded = $('form#Secure3D').attr('action');
        $('#Secure3D').attr('action', encoded.replace(/&amp;/g, '&'));

        //	alert($('form#Secure3D').attr('action'));
        $('#Secure3D').submit();
        $('#Secure3D').hide();
    }
}

/*  ===============================================================================================  */
/*  ==  EXTEND JQUERY  ============================================================================  */
/*  ===============================================================================================  */



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

/*  ===============================================================================================  */
/*  ==  END EXTEND JQUERY  ========================================================================  */
/*  ===============================================================================================  */

$(function () {
    // target _blank workaround - added Robw 23/2/10
    $('a[rel*=external]').click(function () {
        window.open(this.href);
        return false;
    });
});


/*
---------------------- Handle Eonic Xforms-------------------------
*/
$.fn.prepareXform = function () {
    //---- Removes whitespace from textarea we cant do in xslt
    $(this).find('textarea').each(function () {
        $(this).val($(this).val().trim());
    });

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

if ($('.jqDatePicker').exists()) {
    $(".jqDatePicker").on("change", function () {
        var id = $(this).attr("id");
        var val = $("label[for='" + id + "']").text();
        $("#msg").text(val + " changed");
    });
};

function cleanDatepicker() {
    var old_fn = $.datepicker._updateDatepicker;

    $.datepicker._updateDatepicker = function (inst) {
        old_fn.call(this, inst);
        var buttonPane = $(this).datepicker("widget").find(".ui-datepicker-buttonpane");
        $("<button type='button' class='ui-datepicker-clean ui-state-default ui-priority-primary ui-corner-all'>Clear</button>").appendTo(buttonPane).click(function (ev) {
            $.datepicker._clearDate(inst.input);
        });
    }
}


/*  USED IN ALL EW:xFORMS - For when an Radio Button Toggles a switch /case */
function showDependant(dependant, allDependants) {

    // Hide unwanted Dependants

        $("." + allDependants).addClass('hidden');

        // Make required inactive to avoid JS validation
        $("." + allDependants).find('.required').each(function () {
            $(this).removeClass('required');
            $(this).addClass('reqinactive');
        })

        // Make all now hidden fields inactive so values are lost when submitted.
        $("." + allDependants).find(":input").not(':submit').each(function () {
            var fieldName = $(this).attr('name');
            var tempFieldName = fieldName + '~inactive';
            //    alert("hide as " + tempFieldName);
            $(this).attr('name', tempFieldName);
            //   $(this).attr('id', $(this).attr('id') + '~inactive');
        });

    // Show wanted Dependants
    $("#" + dependant).removeClass('hidden');

    // Find all inactive required fields and make required again for JS Validation
    $("#" + dependant).find('.reqinactive').each(function () {
        $(this).removeClass('reqinactive');
        $(this).addClass('required');
    });

    // Find all inactive inputs, and re-activate,
    $("#" + dependant).find(":input").not(':submit').each(function () {
        var fieldName = $(this).attr('name');
        var tempFieldName = fieldName.replace(/~inactive/gi, ''); /* g-  required for global replace, i - required for case-insesitivity */
        $(this).attr('name', tempFieldName);

        var fieldId = $(this).attr('id');
        var tempFieldId = fieldId.replace(/~inactive/gi, ''); /* g-  required for global replace, i - required for case-insesitivity */
        $(this).attr('id', tempFieldId);
        //  alert("enable " + tempFieldName);
        //  $(this).attr('id', $(this).attr('name').replace('~inactive', ''));
    });

    $("#" + dependant).prepareXform();
    $("#" + dependant).trigger('bespokeXform');
}

function hideAllDependants(thisId, allDependants) {
    
    // Hide unwanted Dependants
    //if (donothide != true) {
        $("." + allDependants).addClass('hidden');

        // Make required inactive to avoid JS validation
        $("." + allDependants).find('.required').each(function () {
            $(this).removeClass('required');
            $(this).addClass('reqinactive');
        })

        // Make all now hidden fields inactive so values are lost when submitted.
        $("." + allDependants).find(":input").not(':submit').each(function () {
            var fieldName = $(this).attr('name');
            var tempFieldName = fieldName + '~inactive';
            //    alert("hide as " + tempFieldName);
            $(this).attr('name', tempFieldName);
            //   $(this).attr('id', $(this).attr('id') + '~inactive');
        });
    //}
}

function hideCase(thisId) {
    // Show wanted Dependants
    $("#" + thisId).addClass('hidden');

    // Find all inactive required fields and make required again for JS Validation
    $("#" + thisId).find('.reqinactive').each(function () {
        $(this).removeClass('required');
        $(this).addClass('reqinactive');
    });
}

function showCase(thisId) {
    // Show wanted Dependants
    $("#" + thisId).removeClass('hidden');

    // Find all inactive required fields and make required again for JS Validation
    $("#" + thisId).find('.reqinactive').each(function () {
        $(this).removeClass('reqinactive');
        $(this).addClass('required');
    });

    // Find all inactive inputs, and re-activate,
    $("#" + thisId).find(":input").not(':submit').each(function () {
        var fieldName = $(this).attr('name');
        var tempFieldName = fieldName.replace(/~inactive/gi, ''); /* g-  required for global replace, i - required for case-insesitivity */
        $(this).attr('name', tempFieldName);

        var fieldId = $(this).attr('id');
        var tempFieldId = fieldId.replace(/~inactive/gi, ''); /* g-  required for global replace, i - required for case-insesitivity */
        $(this).attr('id', tempFieldId);
        //  alert("enable " + tempFieldName);
        //  $(this).attr('id', $(this).attr('name').replace('~inactive', ''));
    });

    $("#" + thisId).prepareXform();
    $("#" + thisId).trigger('bespokeXform');
}

function showHideDependant(bindVar) {

    //get this list of service chkbxs under bindVar
    var servicesObjs = $("[name='" + bindVar + "']");
    var serviceIds = [];
    $.each(servicesObjs, function (key, value) { //get Ids of the services
        serviceIds.push(value.id);
    });

    //get Ids of the services checked
    var servcsSelected = [];
    $.each(serviceIds, function (key, value) { 
        if ($('#' + value).is(":checked")) {
            servcsSelected.push(value);
        }
    });
    
    //get cases/Qs for all services checked
    var QsForServcChckd = [];
    var QsForServcChckdDpdnt = [];
    $.each(servcsSelected, function (key, value) {
        QsForServcChckd = ($('#' + value).data('showhide').split(','));
        for (var i = 0; i < QsForServcChckd.length; i++) {
            if (jQuery.inArray(QsForServcChckd[i] + '-dependant', QsForServcChckdDpdnt) == -1) { //check for duplicate
                QsForServcChckdDpdnt.push(QsForServcChckd[i] + '-dependant');
            }
        }
    });
    
    //hide all cases/Qs
    var QArray = [];
    $('.' + bindVar + '-dependant').each(function () {
        QArray.push(this.id);
    });
    $.each(QArray, function (key, value) {
        hideCase(value);
    });

    //show all cases/Qs for services selected
    $.each(QsForServcChckdDpdnt, function (key, value) {
        showCase(value);
    });
}


/*  USED IN ALL EW:xFORMS - To re-enable radio button functionality when renaming a radio button */
function psuedoRadioButtonControl(sBindName, sBindToName, sBindToValue) {

    $("input[id^='" + sBindName + "']").click(function () {

        // Remove all checked
        $("input[id^='" + sBindName + "']").attr('checked', '');

        // Make selected checked
        $(this).attr('checked', 'checked');

        // If Pseudo radio clicked
        if ($(this).val() == sBindToValue) {

            // Assign value to hidden field
            $("input[name='" + sBindToName + "'][type='hidden']").val(sBindToValue);

            // Make pseudo input inactive, to avoid CSL with hidden input 
            var fieldName = $(this).attr('name');
            var tempFieldName = fieldName + '~inactive';
            $(this).attr('name', tempFieldName);

        }

        // Else not Pseudo radio
        else {

            // Empty Hidden field
            $("input[name='" + sBindToName + "'][type='hidden']").val('');

            // re-activate pseudo radio
            var fieldName = $("input[name^='" + sBindToName + "']").attr('name');
            var tempFieldName = fieldName.replace(/~inactive/gi, ''); /* g-  required for global replace, i - required for case-insesitivity */
            $("input[name^='" + sBindToName + "']").attr('name', tempFieldName);

        }

    });

};


function autoAdjustFloatingColumns() {
    /*WRONG - THIS CALCULATES ALL COLUMNS, WHETHER IN SAME CONTEXT OR NOT */
    /*matchHeightCol($(".cols2 .listItem"), '2');*/
    /*matchHeightCol($(".cols3 .listItem"), '3');*/

    /* Use .each() to keep context */
    $("div:not(.content-scroller) > div[class^='cols']").find('.listItem').css({ 'height': 'auto' });
    $("div:not(.content-scroller) > .cols2").each(function () {
        /*added if to fix applications on Wanner*/
        if ($(this).find(".listItem").length > 1) { matchHeightCol($(this).find(".listItem"), '2'); }

    });
    $("div:not(.content-scroller) > .cols3").each(function () {
        matchHeightCol($(this).find(".listItem"), '3');
    });
    $("div:not(.content-scroller) > .cols4").each(function () {
        matchHeightCol($(this).find(".listItem"), '4');
    });
    $("div:not(.content-scroller) > .cols5").each(function () {
        matchHeightCol($(this).find(".listItem"), '5');
    });
    $("div:not(.content-scroller) > .cols6").each(function () {
        matchHeightCol($(this).find(".listItem"), '6');
    });
}

function matchHeightCol(oGroup, nCols) {
    var nRowCount = 0;
    var nColCount = 1;
    var nElemTotal = oGroup.length;
    var nElemCount = 0;
    var moduleId = oGroup.first().parents('.module').attr('id')
    oGroup.each(function () {
        nElemCount++;
        $(this).addClass(moduleId + '-row-' + nRowCount);
        if (nColCount == nCols) {
            matchHeight($(this).parent('.cols' + nCols).find('.' + moduleId + '-row-' + nRowCount));
            nRowCount++;
            nColCount = 1;
        } else {
            $(this).addClass('rowMargin');
            nColCount++;
            if (nElemTotal == nElemCount) {
                matchHeight($(this).parent('.cols' + nCols).find('.' + moduleId + '-row-' + nRowCount));
            }
        }
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

// Common functions
var lockmenu = 0;
var menuNode;

function OpenWindow(theURL, winName, features) {
    //featureStr = 'toolbar=yes,scrollbars=yes,resize=yes,location=no,menubar=yes,width=700,height=560' 
    featureStr = features;
    // these lines make the pickImage facility work in firefox, every other time?? 
    // theURL = unescape(theURL);
    //theURL=theURL.replace('&amp;',/\&/g).replace('&amp;',/\&/g).replace('&amp;',/\&/g).replace('&amp;',/\&/g).replace('&amp;',/\&/g);

    opener = window.open(theURL, winName, featureStr);
    opener.focus();
}


function divSwitch(parentContainerId, activeId) {

    parentContainer = document.getElementById(parentContainerId)
    children = parentContainer.getElementsByTagName("div")
    activeId = document.getElementById(activeId)

    // hide all the children of the parent container
    for (var i = 0; i < children.length; i++) {
        if (children[i].id != 'imageDescription') {
            children[i].style.display = "none";
        }
    }

    // show the activeId
    activeId.style.display = "block";
}

function gallerySwitch(imgName, imgSrc, nWidth, nHeight, cCaption) {
    var img = document.images["pic"]
    var cap = document.getElementById("imgCaption")

    // Change the img
    img.src = imgSrc;
    img.width = nWidth
    img.height = nHeight

    // Change the caption
    // cap.firstChild.nodeValue = cCaption

}

function makedate(formName, dateField) {
    var dayval;

    if ("undefined" == typeof (document.forms[formName].elements['day_' + dateField])) {
        dayval = '01';
    } else {
        dayval = document.forms[formName].elements['day_' + dateField].value
    }
    document.forms[formName].elements[dateField].value = dayval + " " + document.forms[formName].elements['month_' + dateField].value + " " + document.forms[formName].elements['year_' + dateField].value;
}

function loaddate(formName, dateField) {
    dateStr = document.forms[formName].elements[dateField].value
    dateArr = dateStr.split(" ", 4)

    document.forms[formName].elements['day_' + dateField].value = dateArr[0]
    document.forms[formName].elements['month_' + dateField].value = dateArr[1]
    document.forms[formName].elements['year_' + dateField].value = dateArr[2]
    // alert(document.forms[formName].elements['month_' + dateField].selectedIndex)
}


// XML DATE VARIANTS
function makeXmlDate(formName, dateField) {
    var f = document.forms[formName]
    var dayval;

    if ("undefined" == typeof (f.elements['day_' + dateField])) {
        dayval = '01';
    } else {
        dayval = f.elements['day_' + dateField].value
    }

    f.elements[dateField].value = f.elements['year_' + dateField].value + "-" + f.elements['month_' + dateField].value + "-" + dayval;
    //alert(f.elements[dateField].value)
}

function loadXmlDate(formName, dateField) {
    var f = document.forms[formName]

    dateStr = f.elements[dateField].value
    dateArr = dateStr.split("-", 4)

    f.elements['day_' + dateField].value = dateArr[2].substr(0, 2)
    f.elements['month_' + dateField].value = dateArr[1]
    f.elements['year_' + dateField].value = dateArr[0]
    // alert(document.forms[formName].elements['month_' + dateField].selectedIndex)
}

function makedatemmyy(formName, dateField) {
    document.forms[formName].elements[dateField].value = document.forms[formName].elements['month_' + dateField].value + " " + document.forms[formName].elements['year_' + dateField].value;
}

function makedatemmyyObfuscated(formName, dateField) {
    document.forms[formName].elements[dateField].value = document.forms[formName].elements['YGJNO_' + dateField].value + " " + document.forms[formName].elements['FJFKT_' + dateField].value;
}

function loaddatemmyy(formName, dateField) {
    dateStr = document.forms[formName].elements[dateField].value;
    dateArr = dateStr.split(" ", 4);

    document.forms[formName].elements['month_' + dateField].value = dateArr[0];
    document.forms[formName].elements['year_' + dateField].value = dateArr[1];
    // alert(document.forms[formName].elements['month_' + dateField].selectedIndex)
}


function loaddatemmyyObfuscated(formName, dateField) {
    dateStr = document.forms[formName].elements[dateField].value;
    dateArr = dateStr.split(" ", 4);

    document.forms[formName].elements['YGJNO_' + dateField].value = dateArr[0];
    document.forms[formName].elements['FJFKT_' + dateField].value = dateArr[1];
    // alert(document.forms[formName].elements['month_' + dateField].selectedIndex)
}

//define objects for the main list
function ListItem(nvalue, description) {
    //function for defining the elements of the main list
    this.nvalue = nvalue;
    this.description = description;
}

//define objects for the dependent list
function ListSubItem(category, nvalue, description) {
    //function for defining the elements of the sublists
    this.category = category;
    this.nvalue = nvalue;
    this.description = description;
}


function formReset(oForm) {
    oForm.reset()
}

function form_check_old(oForm) {
    return true;
}

var disableButtonMessage = "Please wait..."

function disableButton(oBtn, disableMessage) {

    var oElem, i;
    var oForm = oBtn.form
    if (disableMessage) {
        disableButtonMessage = disableMessage
    }

    // Create a hidden input field spoofing the information of the submit button
    var oNewElem = document.createElement('input')
    oNewElem.type = 'hidden'
    oNewElem.id = 'ewSubmitClone_' + oBtn.id
    oNewElem.name = 'ewSubmitClone_' + oBtn.name
    oNewElem.value = oBtn.value
    oForm.appendChild(oNewElem)

}

function form_disable_button(oForm, btnID, btnName) {
    for (i = 0; i < oForm.length; i++) {
        oElem = oForm.elements[i];
        if (oElem.id == btnID && oElem.name == btnName) {
            oElem.id += '_old'
            oElem.name += '_old'
            oElem.value = disableButtonMessage
        }
    }
}

function form_check(oForm) {
    var oElem, i, bValid, cCheckRadio, cTestName, oOptions, bSelected, nElemTypeGroup, cId, cName;
    var aRadioCheck = new Array();
    var bHasDisabledButton = false
    bValid = true;

    for (i = 0; i < oForm.length; i++) {
        oElem = oForm.elements[i]

        switch (oElem.type) {
            case "text": nElemTypeGroup = 1; break;
            case "textarea": nElemTypeGroup = 1; break;
            case "checkbox": nElemTypeGroup = 3; break;
            case "radio": nElemTypeGroup = 3; break;
            case "select-one": nElemTypeGroup = 2; break;
            case "select-multiple": nElemTypeGroup = 2; break;
            default: nElemTypeGroup = 0; break;
        }

        // Check if the element is required
        if (oElem.className.toLowerCase().indexOf("required") >= 0) {
            switch (nElemTypeGroup) {

                case 1:
                    if (form_check_value(oElem.value)) {
                        form_alert("required", oElem);
                        bValid = false;
                    }
                    break;
                case 2:
                    if (oElem.selectedIndex < 0) {
                        form_alert("required", oElem);
                        bValid = false;
                    }
                    else if (form_check_value(oElem.options[oElem.selectedIndex].text)) {
                        form_alert("required", oElem);
                        bValid = false;
                    }
                    break;
                case 3:
                    // Check if the checkbox group has already been checked through
                    cCheckRadio = "," + aRadioCheck.join(",") + ",";
                    cTestName = "," + oElem.name + ",";
                    if (cCheckRadio.indexOf(cTestName) < 0) {

                        // Not found - let's do the checks
                        aRadioCheck.push(oElem.name);
                        oOptions = document.getElementsByName(oElem.name);
                        bSelected = false;
                        if (oOptions.length > 1) {
                            for (i = 0; i < oOptions.length; i++) { if (oOptions[i].checked) { bSelected = true; break } }
                        }
                        if (!bSelected && oOptions.length > 1) {
                            form_alert("required", oElem);
                            bValid = false;
                        }
                    }
                    break;
            }
            if (!bValid) { break; }
        }
    }

    if (bValid) {
        for (i = 0; i < oForm.length; i++) {
            oElem = oForm.elements[i];
            if (oElem.type == 'text' || oElem.type == 'textarea') {
                if (form_check_value(oElem.value)) { oElem.value = ''; }
            }
        }
    }

    if (bValid) {
        // Check for Disabled Buttons
        for (i = 0; i < oForm.length; i++) {
            oElem = oForm.elements[i];
            if (oElem.type == "hidden" && oElem.id.indexOf("ewSubmitClone_") == 0) {
                cId = oElem.id.replace(/ewSubmitClone_/, "")
                cName = oElem.name.replace(/ewSubmitClone_/, "")
                form_disable_button(oForm, cId, cName)
                oElem.id = cId
                oElem.name = cName
                bHasDisabledButton = true;
            }
        }

        if (bHasDisabledButton) {
            for (i = 0; i < oForm.length; i++) {
                oElem = oForm.elements[i];
                if (oElem.type == "submit" || oElem.type == "button") { oElem.disabled = true; }
            }
        }
    }

    return bValid;
}

//bootstrap validation alert
function displayErrorMessage() {
    if (document.getElementById("xFrmAlertModal") != null) {
        var iconClassName = document.getElementById("errorIcon").className;
        $('#xFrmAlertModal #errorMessage').text(arguments[0]);
        $("#xFrmAlertModal #errorIcon").removeClass(iconClassName);
        $("#xFrmAlertModal #errorIcon").addClass(arguments[1]);
        $('#xFrmAlertModal').modal();
    } else { alert(arguments[0]); }
}

//bootstrap validation alert
function displaySuccessMessage() {
    if (document.getElementById("xFrmAlertModal") != null) {
        var iconClassName = document.getElementById("errorIcon").className;
        $('#xFrmAlertModal #errorMessage').text(arguments[0]);
        $("#xFrmAlertModal #errorIcon").removeClass(iconClassName);
        $("#xFrmAlertModal #errorIcon").addClass(arguments[1]);
       
        if (arguments.length == 3) {
            if (arguments[2] != '') {
                
                $('#xFrmAlertModal .modal-content').removeClass('alert alert-danger');
                $('#xFrmAlertModal .modal-content').addClass(arguments[2]);
            }
        }

        $('#xFrmAlertModal').modal();
    } else { alert(arguments[0]); }
}



/////
function form_alert(cAlertType, oElem) {

    var cLabel
    // Get the label	
    cLabel = form_get_label(oElem);
    switch (cAlertType) {

        case "required":
            if (cLabel == "") {
                // alert("You must complete all the required information");
                displayErrorMessage('You must complete all the required information.', 'fa fa-exclamation-triangle');
            }
            else {
                // $(oElem).insertAfter("<div>This must be completed</div>")
                // alert("Please fill in the field: " + cLabel);
                displayErrorMessage("Please fill in the field: " + cLabel, 'fa fa-exclamation-triangle');
            }
            oElem.focus();
            break;
    }
}


function form_get_label(oElem) {

    var cLabel, oLabels, i;

    cLabel = "";
    oLabels = document.getElementsByTagName("label");

    for (i = 0; i < oLabels.length; i++) {
        if (oLabels[i].htmlFor == oElem.name) { cLabel = oLabels[i].firstChild.nodeValue; break; }
    }

    return cLabel;


}

function form_check_value(cValue) {

    var aKeyDefs = new Array("please enter", "[please enter", "[select ", "please select", "[please select", "por favor preencha o", "Svp complet")
    var bIsDefault, i

    bIsDefault = false
    cValue = cValue.toString()
    cValue = cValue.toLowerCase();
    for (i = 0; i < aKeyDefs.length; i++) {
        if (cValue.substr(0, aKeyDefs[i].length).toLowerCase() == aKeyDefs[i].toLowerCase() || cValue == "") { bIsDefault = true }
    }

    return bIsDefault
}

function form_batch_checkbox(oForm, bChecked) {
    var oElem, i
    a = 0
    for (i = 0; i < oForm.length; i++) {
        oElem = oForm.elements[i];
        if (oElem.type == "checkbox") { oElem.checked = bChecked; }
    }
}

/*  ==  Contrain proportions control on forms ================================================ */
// sControlId = name of constrain control
// sCaseID = id of switch
// sWidthId = Input id of width value
// sHeightId = Input id of height value
// sImageFieldId - Input of the image that is being resized

function initialiseConstrainProportions(sControlId, sCaseID, sWidthId, sHeightId, sImageFieldId) {
    // set global var
    window.bDimensionsConstrain = false;
    window.nImageDimensionsRatio = 1;
    if ($("input[name='" + sControlId + "']").is(":checked")) {
        bDimensionsConstrain = true;
        recalculateDimensionsRatio(sWidthId, sHeightId);
    }
    if ($("input[name^='" + sWidthId + "']").val() != '') {
        recalculateDimensionsRatio(sWidthId, sHeightId);
    }

    // listeners for contrain tick
    $("input[name^='" + sControlId + "']").change(function () {
        if ($(this).is(":checked")) {
            bDimensionsConstrain = true;
            // reset ratio
            recalculateDimensionsRatio(sWidthId, sHeightId);
        } else {
            bDimensionsConstrain = false;
        }

    });

    // if resize values are empty, fill with existing image sizes
    var bEmpty = true;
    $("#" + sCaseID).change(function () {
        bEmpty = true;
        if ($("input[name^='" + sWidthId + "']").val() != '') {
            bEmpty = false;
        }
        if ($("input[name^='" + sHeightId + "']").val() != '') {
            bEmpty = false;
        }
        // if empty get dimensions
        if (bEmpty) {
            var imageTag = $("#" + sImageFieldId).val();
            imageTag = $(imageTag);
            var imageWidth = imageTag.attr('width');
            var imageHeight = imageTag.attr('height');
            $("input[name^='" + sWidthId + "']").val(imageWidth);
            $("input[name^='" + sHeightId + "']").val(imageHeight);
        }
        // update global ratio
        recalculateDimensionsRatio(sWidthId, sHeightId);
    });


    // listeners for key taps on width
    $("input[name^='" + sWidthId + "']").keyup(function (event) {
        if (bDimensionsConstrain) {
            var nKeyPressed = Number(event.keyCode);
            // only allow 0-9, backspace and delete
            if ((nKeyPressed >= 48 && nKeyPressed <= 57) || (nKeyPressed >= 96 && nKeyPressed <= 105) || (nKeyPressed == 8 || nKeyPressed == 46)) {
                var widthValue = $(this).val();
                var newHeightValue = Math.round(widthValue * nImageDimensionsRatio);
                $("input[name^='" + sHeightId + "']").val(newHeightValue);
            } else {
                // remove last character typed
                var removeLastTyped = $(this).val().substring(0, ($(this).val().length - 1));
                $(this).val(removeLastTyped);
                //alert('This must be a whole number');
                displayErrorMessage('This must be a whole number', 'fa fa-exclamation-triangle');
            }
        }
    });
    // listeners for key taps on height
    $("input[name^='" + sHeightId + "']").keyup(function (event) {
        if (bDimensionsConstrain) {
            var nKeyPressed = Number(event.keyCode);
            // only allow 0-9, backspace and delete
            if ((nKeyPressed >= 48 && nKeyPressed <= 57) || (nKeyPressed >= 96 && nKeyPressed <= 105) || (nKeyPressed == 8 || nKeyPressed == 46)) {
                var heightValue = $(this).val();
                var newWidthValue = Math.round(heightValue / nImageDimensionsRatio);
                $("input[name^='" + sWidthId + "']").val(newWidthValue);
            } else {
                // remove last character typed
                var removeLastTyped = $(this).val().substring(0, ($(this).val().length - 1));
                $(this).val(removeLastTyped);
                // alert('This must be a whole number');
                displayErrorMessage('This must be a whole number', 'fa fa-exclamation-triangle');
            }
        }
    });
}
function recalculateDimensionsRatio(sWidthId, sHeightId) {
    nImageDimensionsRatio = Number($("input[name^='" + sHeightId + "']").val()) / Number($("input[name^='" + sWidthId + "']").val());
}

//Handle the back-forward cache on Safari/Mac.
if (navigator.userAgent.indexOf('Safari') != -1 && navigator.userAgent.indexOf('Chrome') == -1) {
    $(window).bind("pageshow", function (event) {
        if (event.originalEvent.persisted) {
            window.location.reload()
        }
    });
}
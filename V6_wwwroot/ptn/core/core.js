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
    if (typeof universalParallax === 'function') {
        var parallaxspeed = $('.parallax-wrapper').last().data("parallax-speed");
        if (parallaxspeed === '') {
            parallaxspeed = 1;
        }
        new universalParallax().init({
            speed: parallaxspeed
        });
    }
    //contentScroller();
    contentSwiper();
}

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
            vFadeEffect =  {  crossFade: true   }
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

function displayErrorMessage() {
    if ($('#xFrmAlertModal').exists()) {
        var iconClassName = document.getElementById("errorIcon").className;
        if ($('#xFrmAlertModal #errorMessage').html() == "&nbsp;" || arguments[2]==true) {
            $('#xFrmAlertModal #errorMessage').html('');
            $('#xFrmAlertModal #errorMessage').html(arguments[0]);
            $("#xFrmAlertModal #errorIcon").removeClass(iconClassName);
            $("#xFrmAlertModal #errorIcon").addClass(arguments[1]);
        }
        else {
            var NewAlert = '<br/><i class="' + iconClassName + '">&nbsp;</i>&nbsp; <span>' + arguments[0] + '</span>';
            $("#xFrmAlertModal .modal-body").append(NewAlert);
        }
       
        let xFrmAlertModal = bootstrap.Modal.getOrCreateInstance(document.getElementById('xFrmAlertModal')) // Returns a Bootstrap modal instance
        xFrmAlertModal.show();
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

        $('#xFrmAlertModal').show();
    } else { alert(arguments[0]); }
}


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
            var myDatePicker = $(this);
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
            $(this).nextAll('label').on("click", function () {
                myDatePicker.datepicker("show");
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
        $(this).nextAll('label').on("click", function () {
            myDatePicker.datepicker("show");
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
        $("#xFrmAlertModal #errorMessage").load("/ptn/tools/passwordpolicydisplay.ashx");
        let xFrmAlertModal = bootstrap.Modal.getOrCreateInstance(document.getElementById('xFrmAlertModal')) // Returns a Bootstrap modal instance
        var iconClassName = document.getElementById("errorIcon").className;
        $("#xFrmAlertModal #errorIcon").removeClass(iconClassName);
        xFrmAlertModal.show();
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

    if ($(this).find('select.select-other').exists()) {
      
        $(this).find('select.select-other').each(function (i) {
            var selectcontrol = $(this);
            var otherTextInput = $("#" + $(this).data("target"));
            var isInDropdown = false;
            otherTextInput.attr('readonly', 'readonly');
            otherTextInput.hide();
            otherTextInput.prev("label").hide();

            $(this).find("option").each(function () {
                if ($(this).val() == otherTextInput.val()) {

                    $(this).attr('selected', 'selected');
                    isInDropdown = true;
                }
                if (isInDropdown == false) {
                    if ($(this).val() == "Other") {
                        $(this).attr('selected', 'selected');
                        otherTextInput.removeAttr('readonly');
                        otherTextInput.show();
                        otherTextInput.prev("label").show();
                    }
                }
            });

            selectcontrol.on('change', function () {
                //alert($(this).find(":selected").val())
                if ($(this).find(":selected").val() == "Other") {
                    otherTextInput.removeAttr('readonly');
                    otherTextInput.show();
                    otherTextInput.prev("label").show();
                    otherTextInput.val('');
                }
                else {
                    otherTextInput.val(selectcontrol.find(":selected").val());
                    otherTextInput.attr('readonly', 'readonly');
                    otherTextInput.hide();
                    otherTextInput.prev("label").hide();
                }
            });
        }); 
    };

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

        // Fetch all the forms we want to apply custom Bootstrap validation styles to
        const forms = document.querySelectorAll('.needs-validation')

        // Loop over them and prevent submission
        Array.from(forms).forEach(form => {
            form.addEventListener('submit', event => {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                }

                form.classList.add('was-validated')
            }, false)
        })



};



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
    $("." + allDependants).find(":input").not(':button').not(':submit').each(function () {
        var fieldName = $(this).attr('name');
        var tempFieldName = fieldName + '~inactive';
        //    alert("hide as " + tempFieldName);
        $(this).attr('name', tempFieldName);
        //   $(this).attr('id', $(this).attr('id') + '~inactive');
    });

    const aDependant = dependant.split(",");

    for (let index = 0; index < aDependant.length; ++index) {
        const sDependant = aDependant[index];
        // ...use `element`...
        $("#" + sDependant).removeClass('hidden');

        // Find all inactive required fields and make required again for JS Validation
        $("#" + sDependant).find('.reqinactive').each(function () {
            $(this).removeClass('reqinactive');
            $(this).addClass('required');
        });
        // Find all inactive inputs, and re-activate,
        $("#" + sDependant).find(":input").not(':button').not(':submit').each(function () {
            var fieldName = $(this).attr('name');
            var tempFieldName = fieldName.replace(/~inactive/gi, ''); /* g-  required for global replace, i - required for case-insesitivity */
            $(this).attr('name', tempFieldName);
            var fieldId = $(this).attr('id');
            var tempFieldId = fieldId.replace(/~inactive/gi, ''); /* g-  required for global replace, i - required for case-insesitivity */
            $(this).attr('id', tempFieldId);
        });
        $("#" + sDependant).prepareXform();
        $("#" + sDependant).trigger('bespokeXform');
    }
}

function clearRadioOther(ref,position) {
    $("input[id='" + ref + "_other']").attr('type', 'input');
    $("input[id='" + ref + "_other']").on('input', function () {
        $("input[id='" + ref + "_" + position + "']").val($("input[id='" + ref + "_other']").val())
    });

    $("input[id^='" + ref + "_']").not("[id='" + ref + "_" + position + "']").not("[id='" + ref + "_other']").on('change', function () {
        $("input[id='" + ref + "_other']").attr('type', 'hidden');
    });
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
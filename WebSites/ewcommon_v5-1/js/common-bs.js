// (c) Eonic Ltd 2002-2010
// Authority: Trevor Spink, Will Hancock, Perry Harlock

var obj = null;
var oQueryParams = {};


/* MAIN PAGE READY METHOD or All site, All pages - Keep Smart! */
$(document).ready(function () {
    oQueryParams = $.getURLParams();
    autoAdjustFloatingColumns();
    initialiseXforms();
    initialiseLightBox();
    initialiseProductSKUs();
    initialiseGoogleMaps();
    positionSocialBookmarks();
    videoSizeAuto();

    //if (typeof stopKeepAlives == 'undefined') { keepAlive(); }

    //IN DEV. initialiseGoogleAdverts();
    //IN DEV. initialisecoverFlow();

    // Run ewAfterCommon if it exists
    if (typeof ewAfterCommon == 'function') {
        ewAfterCommon();
    }
    // Give the first form element focus
    // $('#mainLayout').find(':input:visible:enabled:first:').focus();



});

function initialiseXforms() {
    if ($("form.ewXform").exists()) {
        $('form.ewXform').prepareXform();


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

function initialiseLightBox() {
    if ($('a.lightbox').exists()) {
        $('a.lightbox').lightBox();
    }
}

function initialisecoverFlow() {
    if ($(".ImageCoverFlow").exists()) {
        $("head").append('<script type="text/javascript" src="/ewcommon/js/imageflow/imageflow.js"></script>');
    }
}





function keepAlive() {
    /* call keep alive every 10 mins */
    $.get("/ewcommon/tools/keepalive.ashx", function () { setTimeout("keepAlive();", 600000); });

}

/*Change Price on Selected SKU Option - this function uses 'Live' to cater for content inserted via ajax*/
function initialiseProductSKUs() {

//    $('.skuOptions').each(function () {
//        var addButton = $(this).parents('form').find('.button[name="cartAdd"]');
//        var options = $(this).find('option').length;
//        if (options > 1 && !$('.ProductListGroup').exists()) {
//            addButton.hide();
//        }
//    });

    $('.skuOptions').change(function () {
        obj = this;
        var skuElement = obj.value.split('_');
        var addButton = $(this).parents('form').find('.button[name="cartAdd"]');

        var priceId = '#price_' + skuElement[3];
        var priceId2 = '#price_' + skuElement[3] + '_2';
        var pictureId = '#picture_' + skuElement[0];
        var rrp = skuElement[1];
        var salePrice = skuElement[2];
        var skuName = 'qty_' + skuElement[0];
        //var itemId = '#cartButtons' + skuElement[3] + ', #cartButtons' + skuElement[3] + '_2';
        var options = $(this).find('option').length;
        var skuId = '#qty_' + skuElement[3];

        var productGroup = $('.ProductListGroup').exists();

        if (skuName != '') {
            $(skuId)
            .attr('name', skuName);
            //.attr('id', skuId)

        }

        if (rrp != 'na') {
            $(priceId + ' span.rrpPrice')
            .html(rrp);
        }

        if (salePrice != '') {
            $(priceId + ' span.price, ' + priceId + ' span.price')
            .html(salePrice);
        }

        if ($('.product .picture').length > 1) {
            $('.product .picture').addClass('hidden');
            $(pictureId).parents('span.picture').removeClass('hidden');
        }

        // if Products Grouped template is used the Add to Cart button must not be hidden
        //        if (!productGroup && options > 1) {
        //            //alert('test');
        //            if (!$(this).find('option:first').is(':selected')) {
        //                addButton.show();
        //            } else {
        //                addButton.hide();
        //            }
        //        }

    });
}

/*  ===============================================================================================  */
/*  ==  EXTEND JQUERY  ============================================================================  */
/*  ===============================================================================================  */

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

/*  ===============================================================================================  */
/*  ==  END EXTEND JQUERY  ========================================================================  */
/*  ===============================================================================================  */




function initialiseGoogleAdverts() {
    if ($(".googleadvert").exists()) {
        var gAdverts = new Array();
        $(".googleadvert .adcontainer").each(function () {
            gAdverts.push($(this).attr('title'));
        });
        var initialisationScript = getGoogleAdScript(gAdverts);
        //$("head").append('<script type="text/javascript">alert("TEST");</script>');

    }
}

function getGoogleAdScript(gAdvertList) {

}


$.fn.prepareEditable = function () {

    $('.editable').hover(function () {
        if (obj) {
            obj.find('.inlinePopupOptions').hide('fast');
            obj = null;
        } //if
        $(this).find('.inlinePopupOptions').show('fast');
    }, function () {
        obj = $(this);
        setTimeout(
            "editableCheckHover()",
            400);
    });
};

function editableCheckHover() {
    if (obj) {
        obj.find('.inlinePopupOptions').hide('fast');
    };
};




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



    $('[data-rel=tooltip]').tooltip({ container: 'body' });
    $('[data-rel=popover]').popover({ container: 'body' });
    $('[data-toggle="popover"]').popover();

 //---------------------- Datepicker ----------------------------
    cleanDatepicker() 

    //---------------------- DOBpicker ----------------------------
    if ($('input.jqDOBPicker').exists()) {
        $.datepicker.setDefaults($.datepicker.regional['']);

        $('input.jqDOBPicker').each(function (i) {

            $(this).datepicker({
                closeAtTop: false,

                closeText: 'x',
                showButtonPanel: true,
                showOn: "both",
                buttonImage: "/ewcommon/images/icons/calendar.gif",
                buttonImageOnly: true,
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

   

    if ($('input.jqDatePicker').exists()) {
        $.datepicker.setDefaults($.datepicker.regional['']);

        $('input.jqDatePicker').each(function (i) {

            $(this).datepicker({
                closeAtTop: false,
                closeText: 'x',
                //showButtonPanel: true,
                showOn: "both",
                dateFormat: 'd M yy',
                altField: '#' + $(this).attr('id').replace('-alt', ''),
                altFormat: 'yy-mm-dd',
                mandatory: $(this).hasClass('required')
            });
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
    //---------------------- Slider ----------------------------

    $('div.slider').each(function (i) {
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

    /*----------------------Colourpicker----------------------------
    Just errors*/
    $('input.colorPicker').gccolor();

    /*----------------------GoogleProductCategories----------------------------
    Just errors*/
    if ($('input.googleProductCategories').exists()) {
        $('input.googleProductCategories').googleProductCategories();
    };

    $('textarea[maxlength]').keyup(function () {
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

    $("textarea[class^='maxwords-'],textarea[class*=' maxwords-']").each(function () {
        var t = $(this);
        var sClass = t.attr('class');
        var maxwords = parseInt(/maxwords-(\d+)/.exec(sClass)[1], 10);
        $(this).textareaCounter({ 
        limit: maxwords
        });
    })

    /*----------------------StrongPasswords----------------------------*/

    var myPSPlugin = $("input.strongPassword").password_strength();

    $("input.strongPassword").closest("form").find(".button").click(function () {
        return myPSPlugin.metReq(); //return true or false
    });

    $("[id$='passwordPolicy']").click(function (event) {
        var width = 200, height = 400, left = (screen.width / 2) - (width / 2),
                 top = (screen.height / 2) - (height / 2);
        window.open("/ewcommon/tools/passwordpolicy.ashx", 'Password_policy', 'width=' + width + ',height=' + height + ',left=' + left + ',top=' + top);
        event.preventDefault();
        return false;
    });

    if ($('input#onLoad').exists()) {
        $('input#onLoad').each(function (i) {
            var t = $(this);
            var text = t.val();
            // alert(text);
            eval(text);
        });
    };

    if ($('.contentLocations').exists()) {
        $('.contentLocations').each(function (i) {
            var classString = $(this).attr('class').match(/([^\?]*)pickLimit\-(\d*)/);
            var cbLimit = classString[2];
          //  alert(cbLimit);
            $(this).accordion({
                header: 'legend',
                collapsible: true,
                autoHeight: false
            });
            $(this).prepend('<div class="selectedValues"><h4>Selected Values</h4></div>')
            var $checkboxes_to_limit = $('input', this)
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


/*  USED IN ALL EW:xFORMS - For when an Radio Button Toggles a switch /case */
function showDependant(dependant, allDependants) {

    // Hide unwanted Dependants
    $("." + allDependants).addClass('hidden');

    // Make required inactive to avoid JS validation
    $("." + allDependants).find('.required').each(function () {
        $(this).removeClass('required');
        $(this).addClass('reqinactive')
    })

    // Make all now hidden fields inactive so values are lost when submitted.
    $("." + allDependants).find(":input").not(':submit').each(function () {
        var fieldName = $(this).attr('name');
        var tempFieldName = fieldName + '~inactive';
        //alert("hide as" +tempFieldName);
        $(this).attr('name', tempFieldName);
        $(this).attr('id', tempFieldName);
    });


    // Show wanted Dependants
    $("#" + dependant).removeClass('hidden');

    // Find all inactive required fields and make required again for JS Validation
    $("#" + dependant).find('.reqinactive').each(function () {
        $(this).removeClass('reqinactive');
        $(this).addClass('required');

    })

    // Find all inactive inputs, and re-activate,
    $("#" + dependant).find(":input").not(':submit').each(function () {
        var fieldName = $(this).attr('name');
        var tempFieldName = fieldName.replace(/~inactive/gi, ''); /* g-  required for global replace, i - required for case-insesitivity */
        $(this).attr('name', tempFieldName);
        //alert("enable " + tempFieldName);
        $(this).attr('id', tempFieldName);
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
    $(".cols2").each(function () {
        matchHeightCol($(this).find(".listItem"), '2');
    });
    $(".cols3").each(function () {
        matchHeightCol($(this).find(".listItem"), '3');
    });
    $(".cols4").each(function () {
        matchHeightCol($(this).find(".listItem"), '4');
    });
    $(".cols5").each(function () {
        matchHeightCol($(this).find(".listItem"), '5');
    });
    $(".cols6").each(function () {
        matchHeightCol($(this).find(".listItem"), '6');
    });
}

function matchHeightCol(oGroup, nCols) {
    var nRowCount = 0;
    var nColCount = 1;
    var nElemTotal = oGroup.length;
    var nElemCount = 0;
    oGroup.each(function () {
        nElemCount++;
        $(this).addClass('row' + nRowCount);
        if (nColCount == nCols) {
            matchHeight($(this).parent('.cols' + nCols).find('.row' + nRowCount));
            nRowCount++;
            nColCount = 1;
        } else {
            $(this).addClass('rowMargin');
            nColCount++;
            if (nElemTotal == nElemCount) {
                matchHeight($(this).parent('.cols' + nCols).find('.row' + nRowCount));
            }
        }
    });
}

function matchHeight(oGroup) {
    var tallest = 0;
    var innerTallest = 0;

    oGroup.each(function () {
        thisHeight = $(this).height();
        thisInnerHeight = $(this).find('.lIinner').height();
 
        // Also do innerHeight
        if (thisInnerHeight > innerTallest) {
            innerTallest = thisInnerHeight;
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

    oGroup.each(function () {
        thisHeight = $(this).height();
        thisInnerHeight = $(this).find('.lIinner').height();

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


function getChildUL(obj) {
    for (i = 0; i < obj.childNodes.length; i++) {
        child = obj.childNodes[i]
        if (child.className != null) {
            if (child.className.indexOf("inlinePopupLabel") >= 0) {
                for (j = 0; j < child.childNodes.length; j++) {
                    if (child.childNodes[j].nodeName == "UL") { return child.childNodes[j]; break; }
                }
            }
        }
    }
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

function getAppVersion() {
    appname = navigator.appName;
    appversion = navigator.appVersion;
    majorver = appversion.substring(0, 1);
    if ((appname == "Netscape") && (majorver >= 3)) return 1;
    if ((appname == "Microsoft Internet Explorer") && (majorver >= 4)) return 1;
    return 0;
}



function setHome(arg, urlName) {

    if (navigator.appVersion.charAt(navigator.appVersion.indexOf("MSIE") + 5) >= 5 && navigator.platform.indexOf("Win16") == -1
 && navigator.platform.indexOf("Mac") == -1) {
        arg.style.behavior = 'url(#default#homepage)';
        arg.setHomePage(urlName);
        arg.href = "#";
        return true;
    }
    else { return false }
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

function loaddatemmyy(formName, dateField) {
    dateStr = document.forms[formName].elements[dateField].value
    dateArr = dateStr.split(" ", 4)

    document.forms[formName].elements['month_' + dateField].value = dateArr[0]
    document.forms[formName].elements['year_' + dateField].value = dateArr[1]
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



function setEditableDropdown(editfield, formName, fieldName) {
    //show the selected values
    var lastIndex;
    lastIndex = document.forms[formName].elements[fieldName].options.length - 1;
    document.forms[formName].elements[fieldName].options[lastIndex].selected = true;
    document.forms[formName].elements[fieldName][document.forms[formName].elements[fieldName].selectedIndex].value = editfield.value
}


// Admin function for Shipping Options Valid Regions List
function checkShippingLocationsForm(oCurrent) {
    toggleShipLocList(oCurrent.parentNode, oCurrent.checked, true)
}

function toggleShipLocList(oLI, bChecked, bIsTopLevel) {

    var oChildren, oChild, oListChildren, i, j, bCheckbox

    // Check the children, if they exist
    oChildren = oLI.childNodes
    bCheckbox = false
    for (i = 0; i < oChildren.length; i++) {
        oChild = oChildren.item(i)
        if (oChild.nodeName == 'SPAN' && !bIsTopLevel) { (bChecked) ? oChild.className = 'locationName locationDisabled' : oChild.className = 'locationName' }
        if (oChild.type == 'checkbox' && !bIsTopLevel) { oChild.disabled = bChecked; bCheckbox = oChild.checked; }
        if (oChild.nodeName == 'UL') {
            oListChildren = oChild.childNodes
            for (j = 0; j < oListChildren.length; j++) {
                toggleShipLocList(oListChildren.item(j), (bChecked || bCheckbox), false)
            }
        }
    }

}

function setupShipLocUL(oUL) {
    var oChildren, oChild
    // Check the children LIs
    oChildren = oUL.childNodes
    for (i = 0; i < oChildren.length; i++) {
        oChild = oChildren.item(i)
        if (oChild.nodeName == 'LI') { setupShipLocList(oChild, false) }
    }
}

function setupShipLocList(oLI, bChecked) {

    var oChildren, oChild, oListChildren, i, j, bParCheck

    // Check the children, if they exist
    oChildren = oLI.childNodes
    bParCheck = bChecked
    for (i = 0; i < oChildren.length; i++) {
        oChild = oChildren.item(i)
        if (oChild.nodeName == 'SPAN') { (bChecked) ? oChild.className = 'locationName locationDisabled' : oChild.className = 'locationName' }
        if (oChild.type == 'checkbox') {
            oChild.disabled = bChecked;
            if (!bParCheck) { bParCheck = oChild.checked }
        }
        if (oChild.nodeName == 'UL') {
            oListChildren = oChild.childNodes
            for (j = 0; j < oListChildren.length; j++) {
                setupShipLocList(oListChildren.item(j), bParCheck)
            }
        }
    }

}

//validate forms, checking that all required fields/options have been specified
function validate(oForm) {
    emailAdd = oForm.sEmail.value.indexOf("@");
    //	txt=oForm.txt.value;
    submitOK = "True";

    if (oForm.sCompany.value == "") {
        alert("Please enter your Company");
        submitOK = "False";
    }

    if (emailAdd == -1) {
        alert("Please enter a valid e-mail address");
        submitOK = "False";
    }

    if (submitOK == "False") {
        return false;
    }
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
                        form_alert("required", oElem)
                        bValid = false
                    }
                    break;
                case 2:
                    if (oElem.selectedIndex < 0) {
                        form_alert("required", oElem)
                        bValid = false
                    }
                    else if (form_check_value(oElem.options[oElem.selectedIndex].text)) {
                        form_alert("required", oElem)
                        bValid = false
                    }
                    break;
                case 3:
                    // Check if the checkbox group has already been checked through
                    cCheckRadio = "," + aRadioCheck.join(",") + ","
                    cTestName = "," + oElem.name + ","
                    if (cCheckRadio.indexOf(cTestName) < 0) {

                        // Not found - let's do the checks
                        aRadioCheck.push(oElem.name)
                        oOptions = document.getElementsByName(oElem.name)
                        bSelected = false;
                        if (oOptions.length > 1) {
                            for (i = 0; i < oOptions.length; i++) { if (oOptions[i].checked) { bSelected = true; break } }
                        }
                        if (!bSelected && oOptions.length > 1) {
                            form_alert("required", oElem)
                            bValid = false
                        }
                    }
                    break;
            }
            if (!bValid) { break }
        }
    }

    if (bValid) {
        for (i = 0; i < oForm.length; i++) {
            oElem = oForm.elements[i]
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

function form_alert(cAlertType, oElem) {

    var cLabel
    // Get the label	
    cLabel = form_get_label(oElem)
    switch (cAlertType) {

        case "required":
            if (cLabel == "") {
                alert("You must complete all the required information")
            }
            else {
                alert("You have not any information in the field : " + cLabel)
            }
            oElem.focus()
            break;
    }
}


function form_get_label(oElem) {

    var cLabel, oLabels, i

    cLabel = ""
    oLabels = document.getElementsByTagName("label")

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

function pollCheck(o) {
    var pVal;
    pVal = false;
    for (i = 0; i < o.length; i++) {
        if (o.elements[i].type == 'radio' && o.elements[i].checked) { pVal = true; }
    }
    if (!pVal) { alert('You have not selected anything to vote for!'); }
    return pVal;
}

// AliG functions - recursive position finders.
function getLeft(obj) {
    var nLeft = obj.offsetLeft;
    while ((obj = obj.offsetParent) != null) { nLeft += obj.offsetLeft; }
    return nLeft;
}

function getTop(obj) {
    var nTop = obj.offsetTop;
    while ((obj = obj.offsetParent) != null) { nTop += obj.offsetTop; }
    return nTop;
}

function getRight(obj) {
    var nRight = obj.offsetRight;
    while ((obj = obj.offsetParent) != null) { nRight += obj.offsetRight; }
    return nRight;
}

function getBottom(obj) {
    var nBottom = obj.offsetBottom;
    while ((obj = obj.offsetParent) != null) { nBottom += obj.offsetBottom; }
    return nBottom;
}

function getWidth(obj) {
    var nWidth = obj.offsetWidth;
    return nWidth;
}

function getScrollTop() {
    return document.documentElement['scrollTop'] ? document.documentElement['scrollTop'] : document.body['scrollTop'];
}

function getScrollLeft() {
    return document.documentElement['scrollLeft'] ? document.documentElement['scrollLeft'] : document.body['scrollLeft'];
}
function getVisHeight() {
    return document.documentElement['clientHeight'] ? document.documentElement['clientHeight'] : document.body['clientHeight']
}
function getVisWidth() {
    return document.documentElement['clientWidth'] ? document.documentElement['clientWidth'] : document.body['clientWidth']
}
//
//
//
//


// Expand and Detract Menu Admin-Page Layouts

function hideAndShow(hiddenElement, operation) {
    if (operation = 1) {
        document.getElementById(hiddenElement).style.display = "block";
    }
    if (operation = 0) {
        document.getElementById(hiddenElement).style.display = "none";
    }
}

function Hashtable() {
    this.clear = hashtable_clear;
    this.containsKey = hashtable_containsKey;
    this.containsValue = hashtable_containsValue;
    this.get = hashtable_get;
    this.isEmpty = hashtable_isEmpty;
    this.keys = hashtable_keys;
    this.put = hashtable_put;
    this.remove = hashtable_remove;
    this.size = hashtable_size;
    this.toString = hashtable_toString;
    this.values = hashtable_values;
    this.hashtable = new Array();
}

/*=======Private methods for internal use only========*/

function hashtable_clear() {
    this.hashtable = new Array();
}

function hashtable_containsKey(key) {
    var exists = false;
    for (var i in this.hashtable) {
        if (i == key && this.hashtable[i] != null) {
            exists = true;
            break;
        }
    }
    return exists;
}

function hashtable_containsValue(value) {
    var contains = false;
    if (value != null) {
        for (var i in this.hashtable) {
            if (this.hashtable[i] == value) {
                contains = true;
                break;
            }
        }
    }
    return contains;
}

function hashtable_get(key) {
    return this.hashtable[key];
}

function hashtable_isEmpty() {
    return (parseInt(this.size()) == 0) ? true : false;
}

function hashtable_keys() {
    var keys = new Array();
    for (var i in this.hashtable) {
        if (this.hashtable[i] != null)
            keys.push(i);
    }
    return keys;
}

function hashtable_put(key, value) {
    if (key == null || value == null) {
        throw "NullPointerException {" + key + "},{" + value + "}";
    } else {
        this.hashtable[key] = value;
    }
}

function hashtable_remove(key) {
    var rtn = this.hashtable[key];
    this.hashtable[key] = null;
    return rtn;
}

function hashtable_size() {
    var size = 0;
    for (var i in this.hashtable) {
        if (this.hashtable[i] != null)
            size++;
    }
    return size;
}

function hashtable_toString() {
    var result = "";
    for (var i in this.hashtable) {
        if (this.hashtable[i] != null)
            result += "{" + i + "},{" + this.hashtable[i] + "}\n";
    }
    return result;
}

function hashtable_values() {
    var values = new Array();
    for (var i in this.hashtable) {
        if (this.hashtable[i] != null)
            values.push(this.hashtable[i]);
    }
    return values;
}

var _priceHash = new Hashtable();

function updatePrice(priceId, optionId, value) {
    //alert(priceId + ' - ' + value);
    nPrice = (document.getElementById(priceId).innerHTML * 1)
    //add to array

    if (_priceHash.containsKey(optionId) == true) {
        //deduct the old price
        nPrice = nPrice - _priceHash.get(optionId);
        _priceHash.put(optionId, value);
    }
    else {
        _priceHash.put(optionId, value)
    }

    document.getElementById(priceId).innerHTML = formatAsMoney(nPrice + (value * 1))
}
//  Adds up all the selected items in the options forms and changes the price to include the option cost

function updatePriceCbox(priceId, optionId, value, checkboxElmt) {

    if (checkboxElmt.checked) {
        updatePrice(priceId, optionId, value)
    }
    else {
        nPrice = (document.getElementById(priceId).innerHTML * 1)
        nPrice = nPrice - value;
        _priceHash.remove(optionId)
        document.getElementById(priceId).innerHTML = formatAsMoney(nPrice)
    }
}

function formatAsMoney(mnt) {
    mnt -= 0;
    mnt = (Math.round(mnt * 100)) / 100;
    return (mnt == Math.floor(mnt)) ? mnt + '.00'
              : ((mnt * 10 == Math.floor(mnt * 10)) ?
                       mnt + '0' : mnt);
}


// Takes an array and a SELECT element and calls the updateprice function
function updateSelectPrice(obj, priceId, optionId, priceArray) {

    // Get the selected element
    var index = obj.selectedIndex
    updatePrice(priceId, optionId, priceArray[index]);
}

//  Increments the quantity
//  Does not allow a negative quantity !!

function incrementQuantity(inputName, operator) {
    if (operator == '+') {
        document.getElementById(inputName).value = (document.getElementById(inputName).value * 1) + 1;
    }
    else {
        if (document.getElementById(inputName).value > 0) {
            document.getElementById(inputName).value = (document.getElementById(inputName).value * 1) - 1;
        }

    }


}

function PadDigits(n, totalDigits) {
    n = n.toString();
    var pd = '';
    if (totalDigits > n.length) {
        for (i = 0; i < (totalDigits - n.length); i++) {
            pd += '0';
        }
    }
    return pd + n.toString();
}


function checkAll(field) {

    for (i = 0; i < field.length; i++) {
        field[i].checked = true;
    }

}
function uncheckAll(field) {



    for (i = 0; i < field.length; i++) {
        field[i].checked = false;
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
                alert('This must be a whole number');
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
                alert('This must be a whole number');
            }
        }
    });
}
function recalculateDimensionsRatio(sWidthId,sHeightId) {
    nImageDimensionsRatio = Number($("input[name^='" + sHeightId + "']").val()) / Number($("input[name^='" + sWidthId + "']").val());
}

/*  ==  GOOGLE MAPS  ================================================ */
function initialiseGoogleMaps() {
    if ($(".gmap-canvas").exists()) {
        // this function is added to the page HTML in the footer if present
        initialiseGMaps();
    }
}

function adjustGMapSizes(mapCanvas) {
    var mapWidth = mapCanvas.width();
    /* take 20% off height - odd optical illusion where height looks longer than width even when square */
    mapHeight = mapWidth - Math.round((mapWidth / 100) * 20);
    mapCanvas.css('height', mapHeight);
}
/*  ==  END GOOGLE MAPS  ================================================ */

/*  ==  Social Bookmarks  =============================================== */

/**
 * Moves "top" bookmark placements to be the first element inside their parent
 */
function positionSocialBookmarks() {
	// Get the .bookmarkPlacement container
	var bookmarkPlacement = $('.bookmarkPlacement.top-left, .bookmarkPlacement.top-right');

	// Only continue if there is a bookmark placement container that needs moving
	if (bookmarkPlacement.exists()) {
		var bookmarkTimeout = null; // The identifier for the timeout (can use this to clear the timeout)
		var bookmarkDelay = 20;		// How often to check for bookmark readiness (in ms)

		/**
		 * Sets the timeout for the checkBookmarkReadiness function
		 */
		function setBookmarkTimeout() {
			bookmarkTimeout = setTimeout(checkBookmarkReadiness, bookmarkDelay);
		}

		/**
		 * Check how ready all the bookmarks are
		 */
		function checkBookmarkReadiness() {
			// Let's be optimistic and presume we're ready!
			var ready = true;

			// The names of the different bookmark types, and what to look for once they're ready
			var bookmarkTypes = [
				{ name: 'google',		type: 'iframe' },
				{ name: 'facebook',		type: 'iframe' },
				{ name: 'twitter',		type: 'iframe' },
				{ name: 'linkedin',		type: 'span' },
				{ name: 'orkut',		type: 'span' }
			];

			// Loop through the bookmark types and check readiness
			for (var index in bookmarkTypes) {
				var bookmarkType = bookmarkTypes[index];
				var bookmark = bookmarkPlacement.find('.' + bookmarkType.name + '-bookmark');
				if (bookmark.exists()) {
				    ready = bookmark.find(bookmarkType.type).exists();
				}
			}

			// Move bookmark placement if ready, or schedule another check
			if (ready) {
				moveBookmarkPlacement();
			} else {
				setBookmarkTimeout();
			}
		}

		/**
		 * Moves the bookmark placement container to the top of its parent
		 */
		function moveBookmarkPlacement() {
			bookmarkPlacement.parent().prepend(bookmarkPlacement);
		}

		// Start the timeout
		setBookmarkTimeout();
	}
}


/**
 * Video module auto resize
 */
function videoSizeAuto() {
	$('.VideoSizeAuto').each(function () {
		var $this = $(this),
			video = $this.find('iframe, object, embed'),
			ratio = video.width() / video.height(),
			width = $this.width(),
			height = Math.round(width / ratio);

		// Add 20px height adjustment for FLVPlayer control bar
		if ($this.hasClass('VideoTypeLocal')) {
			height += 20;
		}
		// Add 27px height adjustment for YouTube control bar
		if ($this.hasClass('VideoTypeYouTube')) {
			height += 27;
		}

		video.width(width).height(height);
	});
}
/* End of Video module auto resize */


/**
*JQuery TinyMCE
*/

(function (b) { var c, a = []; function e(g, f, i) { var h; h = b.fn[f]; b.fn[f] = function () { var j; if (g !== "after") { j = i.apply(this, arguments); if (j !== undefined) { return j } } j = h.apply(this, arguments); if (g !== "before") { i.apply(this, arguments) } return j } } b.fn.tinymce = function (i) { var h = this, g, j = "", f; if (!h.length) { return } if (!i) { return tinyMCE.get(this[0].id) } function k() { if (d) { d(); d = null } h.each(function (m, p) { var l, o = p.id || tinymce.DOM.uniqueId(); p.id = o; l = new tinymce.Editor(o, i); l.render() }) } if (!window.tinymce && !c && (g = i.script_url)) { c = 1; if (/_(src|dev)\.js/g.test(g)) { j = "_src" } window.tinyMCEPreInit = { base: g.substring(0, g.lastIndexOf("/")), suffix: j, query: "" }; b.getScript(g, function () { tinymce.dom.Event.domLoaded = 1; c = 2; k(); b.each(a, function (l, m) { m() }) }) } else { if (c === 1) { a.push(k) } else { k() } } }; b.extend(b.expr[":"], { tinymce: function (f) { return f.id && !!tinyMCE.get(f.id) } }); function d() { function f() { this.find("span.mceEditor,div.mceEditor").each(function (j, k) { var h; if (h = tinyMCE.get(k.id.replace(/_parent$/, ""))) { h.remove() } }) } function g(i) { var h; if (i !== undefined) { f.call(this); this.each(function (k, l) { var j; if (j = tinyMCE.get(l.id)) { j.setContent(i) } }) } else { if (this.length > 0) { if (h = tinyMCE.get(this[0].id)) { return h.getContent() } } } } e("both", "text", function (h) { if (h !== undefined) { return g.call(this, h) } if (this.length > 0) { if (ed = tinyMCE.get(this[0].id)) { return ed.getContent().replace(/<[^>]+>/g, "") } } }); b.each(["val", "html"], function (j, h) { e("both", h, g) }); b.each(["append", "prepend"], function (j, h) { e("before", h, function (i) { if (i !== undefined) { this.each(function (l, m) { var k; if (k = tinyMCE.get(m.id)) { if (h === "append") { k.setContent(k.getContent() + i) } else { k.setContent(i + k.getContent()) } } }) } }) }); e("both", "attr", function (h, i) { if (h && h === "value") { return g.call(this, i) } }); b.each(["remove", "replaceWith", "replaceAll", "empty"], function (j, h) { e("before", h, f) }) } })(jQuery);

/**
* GcColor colorpicker plug-in for jQuery
* Originaly written by Stefan Petre <www.eyecon.ro>
* @author Gusts 'gusC' Kaksis <gusts.kaksis@gmail.com>
* @version 1.0.3
*/
(function ($) {
    var gcColor = function () {
        var defaults = {
            onOpen: function () { },
            onClose: function () { },
            onChange: function () { },
            useButton: true,
            defaultColor: '#FF0000'
        },
		_uiInstalled = (typeof $.ui == 'undefined' ? false : true),
		_dialogBody = '<div id="gccolor-dialog" style="display: none;">'
  		+ '<div id="gccolor-color">'
  		+ '<div>'
  		+ '<div></div>'
  		+ '</div>'
  		+ '</div>'
			+ '<div id="gccolor-hue">'
			+ '<div></div>'
			+ '</div>'
			+ '<div id="gccolor-new-color"></div>'
			+ '<div id="gccolor-current-color"></div>'
			+ '<div id="gccolor-hex"><input type="text" maxlength="6" size="6" /></div>'
			+ '<div id="gccolor-rgb-r" class="gccolor-field"><input type="text" maxlength="3" size="3" /><span></span></div>'
			+ '<div id="gccolor-rgb-g" class="gccolor-field"><input type="text" maxlength="3" size="3" /><span></span></div>'
			+ '<div id="gccolor-rgb-b" class="gccolor-field"><input type="text" maxlength="3" size="3" /><span></span></div>'
			+ '<div id="gccolor-hsb-h" class="gccolor-field"><input type="text" maxlength="3" size="3" /><span></span></div>'
			+ '<div id="gccolor-hsb-s" class="gccolor-field"><input type="text" maxlength="3" size="3" /><span></span></div>'
			+ '<div id="gccolor-hsb-b" class="gccolor-field"><input type="text" maxlength="3" size="3" /><span></span></div>'
			+ '<div id="gccolor-submit"></div>'
			+ '</div>',
		_startHue = function (e) {
		    $(document).data('gccolor').dragItem = {
		        lastX: e.pageX,
		        lastY: e.pageY
		    };
		    _dragHue(e);
		    $(document).bind('mousemove', _dragHue);
		    $(document).bind('mouseup', _endHue);
		},
		_dragHue = function (e) {
		    var item = $('#gccolor-hue');
		    var y = e.pageY - item.offset().top;
		    if (y < 0) {
		        y = 0;
		    } else if (y > 150) {
		        y = 150;
		    }
		    $(document).data('gccolor').hsb.h = 359 - Math.round((y / 150) * 359);
		    if (!$.browser.msie) {
		        _setFromHSB($(document).data('gccolor').hsb);
		    } else {
		        $('#gccolor-hue div').css('top', y + 'px');
		    }
		    changeColor();
		    return false;
		},
		_endHue = function (e) {
		    $(document).unbind('mousemove', _dragHue);
		    $(document).unbind('mouseup', _endHue);
		    $(document).data('gccolor').dragItem = null;
		    if ($.browser.msie) {
		        _setFromHSB($(document).data('gccolor').hsb);
		    }
		},
		_startColor = function (e) {
		    $(document).data('gccolor').dragItem = {
		        lastX: e.pageX,
		        lastY: e.pageY
		    };
		    _dragColor(e);
		    $(document).bind('mousemove', _dragColor);
		    $(document).bind('mouseup', _endColor);
		},
		_dragColor = function (e) {
		    var item = $('#gccolor-color > div');
		    var x = e.pageX - item.offset().left;
		    var y = e.pageY - item.offset().top;
		    if (x < 0) {
		        x = 0;
		    } else if (x > 150) {
		        x = 150;
		    }
		    if (y < 0) {
		        y = 0;
		    } else if (y > 150) {
		        y = 150;
		    }
		    $(document).data('gccolor').hsb.s = Math.round((x / 150) * 100);
		    $(document).data('gccolor').hsb.b = 100 - Math.round((y / 150) * 100);
		    if (!$.browser.msie) {
		        _setFromHSB($(document).data('gccolor').hsb);
		    } else {
		        $('#gccolor-color > div div').css('top', y + 'px');
		        $('#gccolor-color > div div').css('left', x + 'px');
		    }
		    changeColor();
		    return false;
		},
		_endColor = function (e) {
		    $(document).unbind('mousemove', _dragColor);
		    $(document).unbind('mouseup', _endColor);
		    $(document).data('gccolor').dragItem = null;
		    if ($.browser.msie) {
		        _setFromHSB($(document).data('gccolor').hsb);
		    }
		},
		_startUnit = function (e) {
		    $(document).data('gccolor').dragItem = {
		        item: $(this),
		        lastX: e.pageX,
		        lastY: e.pageY
		    };
		    $(document).bind('mousemove', _dragUnit);
		    $(document).bind('mouseup', _endUnit);
		},
		_dragUnit = function (e) {
		    var item = $(document).data('gccolor').dragItem.item;
		    var name = item.parent().attr('id')
		    var deltaY = e.pageY - $(document).data('gccolor').dragItem.lastY;
		    var prevVal = parseInt(item.prev().val());
		    var newVal = prevVal;
		    if (deltaY < 0) {
		        newVal = prevVal + 1;
		    } else if (deltaY > 0) {
		        newVal = prevVal - 1;
		    }
		    if (name == 'gccolor-hsb-h') {
		        if (newVal > 359) {
		            newVal = 0;
		        } else if (newVal <= 0) {
		            newVal = 359;
		        }
		    } else if (name == 'gccolor-hsb-s' || name == 'gccolor-hsb-b') {
		        if (newVal > 100) {
		            newVal = 100;
		        } else if (newVal <= 0) {
		            newVal = 0;
		        }
		    } else {
		        if (newVal > 255) {
		            newVal = 255;
		        } else if (newVal <= 0) {
		            newVal = 0;
		        }
		    }
		    var rgb = _HSBtoRGB($(document).data('gccolor').hsb);
		    switch (name) {
		        case 'gccolor-hsb-h':
		            $(document).data('gccolor').hsb.h = newVal;
		            break;
		        case 'gccolor-hsb-s':
		            $(document).data('gccolor').hsb.s = newVal;
		            break;
		        case 'gccolor-hsb-b':
		            $(document).data('gccolor').hsb.b = newVal;
		            break;
		        case 'gccolor-rgb-r':
		            rgb.r = newVal;
		            $(document).data('gccolor').hsb = _RGBtoHSB(rgb);
		            break;
		        case 'gccolor-rgb-g':
		            rgb.g = newVal;
		            $(document).data('gccolor').hsb = _RGBtoHSB(rgb);
		            break;
		        case 'gccolor-rgb-b':
		            rgb.b = newVal;
		            $(document).data('gccolor').hsb = _RGBtoHSB(rgb);
		            break;
		    }
		    if (!$.browser.msie) {
		        _setFromHSB($(document).data('gccolor').hsb);
		    }
		    changeColor();
		    item.prev().val(newVal);
		    $(document).data('gccolor').dragItem.lastY = e.pageY;
		},
		_endUnit = function (e) {
		    $(document).unbind('mousemove', _dragUnit);
		    $(document).unbind('mouseup', _endUnit);
		    $(document).data('gccolor').dragItem = null;
		    if ($.browser.msie) {
		        _setFromHSB($(document).data('gccolor').hsb);
		    }
		},
		_HEXtoRGB = function (hex) {
		    var hex = parseInt(((hex.indexOf('#') > -1) ? hex.substring(1) : hex), 16);
		    return { r: hex >> 16, g: (hex & 0x00FF00) >> 8, b: (hex & 0x0000FF) };
		},
		_HSBtoRGB = function (hsb) {
		    var b = Math.ceil(hsb.b * 2.55)
		    if (hsb.b == 0) {
		        return { r: 0, g: 0, b: 0 };
		    } else if (hsb.s == 0) {
		        return { r: b, g: b, b: b };
		    }
		    var Hi = Math.floor(hsb.h / 60);
		    var f = hsb.h / 60 - Hi;
		    var p = Math.round(hsb.b * (100 - hsb.s) * 0.0255);
		    var q = Math.round(hsb.b * (100 - f * hsb.s) * 0.0255);
		    var t = Math.round(hsb.b * (100 - (1 - f) * hsb.s) * 0.0255);
		    switch (Hi) {
		        case 0: return { r: b, g: t, b: p }; break;
		        case 1: return { r: q, g: b, b: p }; break;
		        case 2: return { r: p, g: b, b: t }; break;
		        case 3: return { r: p, g: q, b: b }; break;
		        case 4: return { r: t, g: p, b: b }; break;
		        case 5: return { r: b, g: p, b: q }; break;
		    }
		    return { r: 0, g: 0, b: 0 };
		},
		_RGBtoHSB = function (rgb) {
		    var hsb = {};
		    hsb.b = Math.max(Math.max(rgb.r, rgb.g), rgb.b);
		    hsb.s = (hsb.b <= 0) ? 0 : Math.round(100 * (hsb.b - Math.min(Math.min(rgb.r, rgb.g), rgb.b)) / hsb.b);
		    hsb.b = Math.round((hsb.b / 255) * 100);
		    if ((rgb.r == rgb.g) && (rgb.g == rgb.b)) hsb.h = 0;
		    else if (rgb.r >= rgb.g && rgb.g >= rgb.b) hsb.h = 60 * (rgb.g - rgb.b) / (rgb.r - rgb.b);
		    else if (rgb.g >= rgb.r && rgb.r >= rgb.b) hsb.h = 60 + 60 * (rgb.g - rgb.r) / (rgb.g - rgb.b);
		    else if (rgb.g >= rgb.b && rgb.b >= rgb.r) hsb.h = 120 + 60 * (rgb.b - rgb.r) / (rgb.g - rgb.r);
		    else if (rgb.b >= rgb.g && rgb.g >= rgb.r) hsb.h = 180 + 60 * (rgb.b - rgb.g) / (rgb.b - rgb.r);
		    else if (rgb.b >= rgb.r && rgb.r >= rgb.g) hsb.h = 240 + 60 * (rgb.r - rgb.g) / (rgb.b - rgb.g);
		    else if (rgb.r >= rgb.b && rgb.b >= rgb.g) hsb.h = 300 + 60 * (rgb.r - rgb.b) / (rgb.r - rgb.g);
		    else hsb.h = 0;
		    hsb.h = Math.round(hsb.h);
		    return hsb;
		},
		_RGBtoHEX = function (rgb) {
		    var hex = [
				rgb.r.toString(16),
				rgb.g.toString(16),
				rgb.b.toString(16)
			];
		    $.each(hex, function (nr, val) {
		        if (val.length == 1) {
		            hex[nr] = '0' + val;
		        }
		    });
		    return hex.join('');
		},
		_setFields = function (hsb, rgb, hex) {
		    $('#gccolor-hsb-h input').val(hsb.h);
		    $('#gccolor-hsb-s input').val(hsb.s);
		    $('#gccolor-hsb-b input').val(hsb.b);
		    $('#gccolor-rgb-r input').val(rgb.r);
		    $('#gccolor-rgb-g input').val(rgb.g);
		    $('#gccolor-rgb-b input').val(rgb.b);
		    $('#gccolor-hex input').val(hex);
		    $('#gccolor-new-color').css('background-color', '#' + hex);
		    var colorBGhex = _RGBtoHEX(_HSBtoRGB({ h: hsb.h, s: 100, b: 100 }));
		    $('#gccolor-color').css('background-color', '#' + colorBGhex);
		    $('#gccolor-color > div div').css('top', (((100 - hsb.b) / 100) * 150) + 'px');
		    $('#gccolor-color > div div').css('left', ((hsb.s / 100) * 150) + 'px');
		    $('#gccolor-hue div').css('top', (150 - ((hsb.h / 359) * 150)) + 'px');
		},
		_setFromHSB = function (hsb) {
		    $(document).data('gccolor').hsb = hsb;
		    var rgb = _HSBtoRGB(hsb);
		    var hex = _RGBtoHEX(rgb);
		    _setFields(hsb, rgb, hex);
		},
		_setFromRGB = function (rgb) {
		    var hex = _RGBtoHEX(rgb);
		    var hsb = _RGBtoHSB(rgb);
		    $(document).data('gccolor').hsb = hsb;
		    _setFields(hsb, rgb, hex);
		},
		_setFromHEX = function (hex) {
		    var rgb = _HEXtoRGB(hex);
		    var hsb = _RGBtoHSB(rgb);
		    $(document).data('gccolor').hsb = hsb;
		    _setFields(hsb, rgb, hex);
		},
		closeOnEsc = function (e) {
		    if (e.keyCode == 27) {
		        $('#gccolor-dialog').hide();
		        var data = $(document).data('gccolor');
		        closeDialog(data.target, data.options, true);
		    }
		},
		openDialog = function (target, options) {
		    $(document).data('gccolor', {
		        target: target,
		        options: options,
		        hsb: { h: 0, s: 100, b: 100 },
		        dragItem: null
		    });
		    if (typeof options.onOpen == 'function') {
		        options.onOpen(target);
		    }
		    var hexColor = $(target).val().replace('#', '').toUpperCase();
		    if (hexColor.length <= 0) {
		        hexColor = options.defaultColor;
		    }
		    _setFromHEX(hexColor);
		    $('#gccolor-current-color').css('background-color', '#' + hexColor);

		    $('#gccolor-submit').click(function () {
		        closeDialog($(document).data('gccolor').target, options, false);
		    });

		    $('#gccolor-dialog').css('top', $(target).offset().top + $(target).outerHeight());
		    $('#gccolor-dialog').css('left', $(target).offset().left);
		    $('#gccolor-dialog').show('slide', { direction: 'up' }, 1000);
		    $(document).keyup(closeOnEsc);
		},
		changeColor = function () {
		    if (typeof $(document).data('gccolor').options.onChange == 'function') {
		        $(document).data('gccolor').options.onChange($(document).data('gccolor').target, _RGBtoHEX(_HSBtoRGB($(document).data('gccolor').hsb)));
		    }
		},
		closeDialog = function (target, options, cancel) {
		    $(document).unbind('keyup', closeOnEsc);
		    if (typeof options.onClose == 'function') {
		        options.onClose(target, $('#gccolor-hex input').val(), cancel);
		    }
		    if (!cancel) {
		        $(target).val('#' + $('#gccolor-hex input').val());
                $(target).css('background-color','#' + $('#gccolor-hex input').val());
		    }
		    $('#gccolor-dialog').hide();
		    $('#gccolor-dialog').dialog('destroy');
		};

        return {
            init: function (options) {
                options = $.extend({}, defaults, options || {});
                if (_uiInstalled) {
                    if (!$('#gccolor-dialog').is('div')) {
                        // We need only one instance
                        $('body').append(_dialogBody);
                    }
                } else {
                    alert('Sorry, jQuery UI plug-in is required for GcColor to work!');
                }

                $('#gccolor-dialog span').bind('mousedown', _startUnit);
                $('#gccolor-hue').bind('mousedown', _startHue);
                $('#gccolor-color > div').bind('mousedown', _startColor);

                return this.each(function () {
                    if (options.useButton) {
                        $(this).wrap('<span class="gccolor-wrapper"></span>')
                        $(this).after('<a href="Javascript:;" class="gccolor-button">Pick a color!</a>');
                        var button = $(this).next();
                        $(this).width($(this).width() - 22);
                        $(this).css('margin-right', '24px');
                        button.css('left', ($(this).position().left + $(this).outerWidth(true) - 22) + 'px');
                        button.click(function () {
                            openDialog($(this).prev(), options);
                        });
                    } else {
                        $(this).click(function () {
                            openDialog($(this), options);
                        });
                    }
                });
            }
        };
    } ();
    $.fn.extend({
        gccolor: gcColor.init
    });
})(jQuery);


/**
* googleProductCategories

*/


(function($){
$.fn.googleProductCategories = function() {
    
    var options = {
        empty_value: 'null',
        indexed: true,  // the data in tree is indexed by values (ids), not by labels
        on_each_change: '/ewcommon/js/jquery/jquery-option-tree/demo/get-subtree.php', // this file will be called with 'id' parameter, JSON data must be returned
        choose: function (level) {
            return 'Choose level ' + level;
        },
        loading_image: '/ewcommon/images/admin/ajax-loader2.gif',
        show_multiple: 5, // if true - will set the size to show all options
        choose: '' // no choose item
    };

 //   alert($(this).html);

    var displayParents = function () {
      //  alert(this.value + ':' + labels.join(' > '));
        var labels = []; // initialize array
        $(this).siblings('select') // find all select
               .find(':selected') // and their current options
                .each(function () { labels.push($(this).text()); }); // and add option text to array
        $('input#googleProductCategories-value').val(labels.join(' > '))
        //$('<textarea>').text(labels.join(' > ')).appendTo('#demo7-result');  // and display the labels
    }

    $.getJSON('/ewcommon/js/jquery/jquery-option-tree/demo/get-subtree.php', function (tree) { // initialize the tree by loading the file first
        //alert($(inputControl).html);
        $('input.googleProductCategories').optionTree(tree, options).change(displayParents);      
    });
    };
}(jQuery));


/**
* SWFObject v1.4.4: Flash Player detection and embed - http://blog.deconcept.com/swfobject/
*
* SWFObject is (c) 2006 Geoff Stearns and is released under the MIT License:
* http://www.opensource.org/licenses/mit-license.php
*
* **SWFObject is the SWF embed script formerly known as FlashObject. The name was changed for
*   legal reasons.
*/
if (typeof deconcept == "undefined") { var deconcept = new Object(); }
if (typeof deconcept.util == "undefined") { deconcept.util = new Object(); }
if (typeof deconcept.SWFObjectUtil == "undefined") { deconcept.SWFObjectUtil = new Object(); }
deconcept.SWFObject = function (_1, id, w, h, _5, c, _7, _8, _9, _a, _b) {
    if (!document.getElementById) { return; }
    this.DETECT_KEY = _b ? _b : "detectflash";
    this.skipDetect = deconcept.util.getRequestParameter(this.DETECT_KEY);
    this.params = new Object();
    this.variables = new Object();
    this.attributes = new Array();
    if (_1) { this.setAttribute("swf", _1); }
    if (id) { this.setAttribute("id", id); }
    if (w) { this.setAttribute("width", w); }
    if (h) { this.setAttribute("height", h); }
    if (_5) { this.setAttribute("version", new deconcept.PlayerVersion(_5.toString().split("."))); }
    this.installedVer = deconcept.SWFObjectUtil.getPlayerVersion();
    if (c) { this.addParam("bgcolor", c); }
    var q = _8 ? _8 : "high";
    this.addParam("quality", q);
    this.setAttribute("useExpressInstall", _7);
    this.setAttribute("doExpressInstall", false);
    var _d = (_9) ? _9 : window.location;
    this.setAttribute("xiRedirectUrl", _d);
    this.setAttribute("redirectUrl", "");
    if (_a) { this.setAttribute("redirectUrl", _a); } 
};
deconcept.SWFObject.prototype = { setAttribute: function (_e, _f) {
    this.attributes[_e] = _f;
}, getAttribute: function (_10) {
    return this.attributes[_10];
}, addParam: function (_11, _12) {
    this.params[_11] = _12;
}, getParams: function () {
    return this.params;
}, addVariable: function (_13, _14) {
    this.variables[_13] = _14;
}, getVariable: function (_15) {
    return this.variables[_15];
}, getVariables: function () {
    return this.variables;
}, getVariablePairs: function () {
    var _16 = new Array();
    var key;
    var _18 = this.getVariables();
    for (key in _18) { _16.push(key + "=" + _18[key]); }
    return _16;
}, getSWFHTML: function () {
    var _19 = "";
    if (navigator.plugins && navigator.mimeTypes && navigator.mimeTypes.length) {
        if (this.getAttribute("doExpressInstall")) {
            this.addVariable("MMplayerType", "PlugIn");
        }
        _19 = "<embed type=\"application/x-shockwave-flash\" src=\"" + this.getAttribute("swf") + "\" width=\"" + this.getAttribute("width") + "\" height=\"" + this.getAttribute("height") + "\"";
        _19 += " id=\"" + this.getAttribute("id") + "\" name=\"" + this.getAttribute("id") + "\" ";
        var _1a = this.getParams();
        for (var key in _1a) { _19 += [key] + "=\"" + _1a[key] + "\" "; }
        var _1c = this.getVariablePairs().join("&");
        if (_1c.length > 0) { _19 += "flashvars=\"" + _1c + "\""; } _19 += "/>";
    } else {
        if (this.getAttribute("doExpressInstall")) { this.addVariable("MMplayerType", "ActiveX"); }
        _19 = "<object id=\"" + this.getAttribute("id") + "\" classid=\"clsid:D27CDB6E-AE6D-11cf-96B8-444553540000\" width=\"" + this.getAttribute("width") + "\" height=\"" + this.getAttribute("height") + "\">";
        _19 += "<param name=\"movie\" value=\"" + this.getAttribute("swf") + "\" />";
        var _1d = this.getParams();
        for (var key in _1d) { _19 += "<param name=\"" + key + "\" value=\"" + _1d[key] + "\" />"; }
        var _1f = this.getVariablePairs().join("&");
        if (_1f.length > 0) { _19 += "<param name=\"flashvars\" value=\"" + _1f + "\" />"; } _19 += "</object>";
    }
    return _19;
}, write: function (_20) {
    if (this.getAttribute("useExpressInstall")) {
        var _21 = new deconcept.PlayerVersion([6, 0, 65]);
        if (this.installedVer.versionIsValid(_21) && !this.installedVer.versionIsValid(this.getAttribute("version"))) {
            this.setAttribute("doExpressInstall", true);
            this.addVariable("MMredirectURL", escape(this.getAttribute("xiRedirectUrl")));
            document.title = document.title.slice(0, 47) + " - Flash Player Installation";
            this.addVariable("MMdoctitle", document.title);
        } 
    }
    if (this.skipDetect || this.getAttribute("doExpressInstall") || this.installedVer.versionIsValid(this.getAttribute("version"))) {
        var n = (typeof _20 == "string") ? document.getElementById(_20) : _20;
        n.innerHTML = this.getSWFHTML(); return true;
    } else { if (this.getAttribute("redirectUrl") != "") { document.location.replace(this.getAttribute("redirectUrl")); } }
    return false;
} 
};
deconcept.SWFObjectUtil.getPlayerVersion = function () {
    var _23 = new deconcept.PlayerVersion([0, 0, 0]);
    if (navigator.plugins && navigator.mimeTypes.length) {
        var x = navigator.plugins["Shockwave Flash"];
        if (x && x.description) { _23 = new deconcept.PlayerVersion(x.description.replace(/([a-zA-Z]|\s)+/, "").replace(/(\s+r|\s+b[0-9]+)/, ".").split(".")); }
    } else {
        try { var axo = new ActiveXObject("ShockwaveFlash.ShockwaveFlash.7"); }
        catch (e) {
            try {
                var axo = new ActiveXObject("ShockwaveFlash.ShockwaveFlash.6");
                _23 = new deconcept.PlayerVersion([6, 0, 21]); axo.AllowScriptAccess = "always";
            }
            catch (e) { if (_23.major == 6) { return _23; } } try { axo = new ActiveXObject("ShockwaveFlash.ShockwaveFlash"); }
            catch (e) { } 
        } if (axo != null) { _23 = new deconcept.PlayerVersion(axo.GetVariable("$version").split(" ")[1].split(",")); } 
    }
    return _23;
};
deconcept.PlayerVersion = function (_27) {
    this.major = _27[0] != null ? parseInt(_27[0]) : 0;
    this.minor = _27[1] != null ? parseInt(_27[1]) : 0;
    this.rev = _27[2] != null ? parseInt(_27[2]) : 0;
};
deconcept.PlayerVersion.prototype.versionIsValid = function (fv) {
    if (this.major < fv.major) { return false; }
    if (this.major > fv.major) { return true; }
    if (this.minor < fv.minor) { return false; }
    if (this.minor > fv.minor) { return true; }
    if (this.rev < fv.rev) {
        return false;
    } return true;
};
deconcept.util = { getRequestParameter: function (_29) {
    var q = document.location.search || document.location.hash;
    if (q) {
        var _2b = q.substring(1).split("&");
        for (var i = 0; i < _2b.length; i++) {
            if (_2b[i].substring(0, _2b[i].indexOf("=")) == _29) {
                return _2b[i].substring((_2b[i].indexOf("=") + 1));
            } 
        } 
    }
    return "";
} 
};
deconcept.SWFObjectUtil.cleanupSWFs = function () {
    if (window.opera || !document.all) { return; }
    var _2d = document.getElementsByTagName("OBJECT");
    for (var i = 0; i < _2d.length; i++) {
        _2d[i].style.display = "none"; for (var x in _2d[i]) {
            if (typeof _2d[i][x] == "function") { _2d[i][x] = function () { }; } 
        } 
    } 
};
deconcept.SWFObjectUtil.prepUnload = function () {
    __flash_unloadHandler = function () { };
    __flash_savedUnloadHandler = function () { };
    if (typeof window.onunload == "function") {
        var _30 = window.onunload;
        window.onunload = function () {
            deconcept.SWFObjectUtil.cleanupSWFs(); _30();
        };
    } else { window.onunload = deconcept.SWFObjectUtil.cleanupSWFs; } 
};
if (typeof window.onbeforeunload == "function") {
    var oldBeforeUnload = window.onbeforeunload;
    window.onbeforeunload = function () {
        deconcept.SWFObjectUtil.prepUnload();
        oldBeforeUnload();
    };
} else { window.onbeforeunload = deconcept.SWFObjectUtil.prepUnload; }
if (Array.prototype.push == null) {
    Array.prototype.push = function (_31) {
        this[this.length] = _31;
        return this.length;
    };
}
var getQueryParamValue = deconcept.util.getRequestParameter;
var FlashObject = deconcept.SWFObject;
var SWFObject = deconcept.SWFObject;


/*
 * jQuery optionTree Plugin
 * version: 1.3
 * @requires jQuery v1.3 or later
 *
 * Dual licensed under the MIT and GPL licenses:
 *   http://www.opensource.org/licenses/mit-license.php
 *   http://www.gnu.org/licenses/gpl.html
 *
 * @version $Id: jquery.optionTree.js 13 2011-03-31 08:51:59Z kkotowicz $
 * @author  Krzysztof Kotowicz <kkotowicz at gmail dot com>
 * @see http://code.google.com/p/jquery-option-tree/
 * @see http://blog.kotowicz.net/search/label/option
 */

/**
 * Converts passed JSON option tree into dynamically created <select> elements allowing you to
 * choose nested options.
 *
 * @param String tree options tree
 * @param array options additional options (optional)
 */
(function($){
$.fn.optionTree = function(tree, options) {

    options = $.extend({
        choose: 'Choose...', // string with text or function that will be passed current level and returns a string
        show_multiple: false, // show multiple values (if true takes number of items as size, or number (eg. 12) to show fixed size)
        preselect: {},
        loading_image: '', // show an ajax loading graphics (animated gif) while loading ajax (eg. /ajax-loader.gif)
        select_class: '',
        leaf_class: 'final',
        empty_value: '', // what value to set the input to if no valid option was selected
        on_each_change: false, // URL to lazy load (JSON, 'id' parameter will be added) or function. See default_lazy_load
        set_value_on: 'leaf', // leaf - sets input value only when choosing leaf node. 'each' - sets value on each level change.
                              // makes sense only then indexed=true
        indexed: false,
        preselect_only_once: false // if true, once preselected items will be chosen, the preselect list is cleared. This is to allow
                                    // changing the higher level options without automatically changing lower levels when a whole subtree is in preselect list
    }, options || {});

    var cleanName = function (name) {
        return name.replace(/_*$/, '');
    };

    var removeNested = function (name) {
        $("select[name^='"+ name + "']").remove();
    };

    var setValue = function(name, value) {
        $("input[name='" + cleanName(name) + "']").val(value).change();
    };

    // default lazy loading function
    var default_lazy_load = function(value) {
        var input = this;
        if ( options.loading_image !== '' ) {
          // show loading animation
          $("<img>")
            .attr('src', options.loading_image)
            .attr('class', 'optionTree-loader')
            .insertAfter(input);
        }

        $.getJSON(options.lazy_load, {id: value}, function(tree) {
            $('.optionTree-loader').remove();
            var prop;
            for (prop in tree) {
                if (tree.hasOwnProperty(prop)) { // tree not empty
                    $(input).optionTree(tree, options);
                    return;
                }
            }
            // tree empty, call value switch
            $(input).optionTree(value, options);
        });
    };

    if (typeof options.on_each_change === 'string') { // URL given as an onchange
        options.lazy_load = options.on_each_change;
        options.on_each_change = default_lazy_load;
    }

    var isPreselectedFor = function(clean, v) {
      if (!options.preselect || !options.preselect[clean]) {
        return false;
      }

      if ($.isArray(options.preselect[clean])) {
        return $.inArray(v, options.preselect[clean]) !== -1;
      }

      return (options.preselect[clean] === v);
    };

    return this.each(function() {
        var name = $(this).attr('name') + "_";

        // remove all dynamic options of lower levels
        removeNested(name);

        if (typeof tree === "object") { // many options exists for current nesting level

            // create select element with all the options
            // and bind onchange event to recursively call this function

            var $select = $("<select>").attr('name',name)
            .change(function() {
                if (this.options[this.selectedIndex].value !== '') {
                    if ($.isFunction(options.on_each_change)) {
                      removeNested(name + '_');
                        options.on_each_change.apply(this, [this.options[this.selectedIndex].value, tree]);
                    } else {
                      // call with value as a first parameter
                        $(this).optionTree(tree[this.options[this.selectedIndex].value], options);
                    }
                    if (options.set_value_on === 'each') {
                      setValue(name, this.options[this.selectedIndex].value);
                    }
                } else {
                  removeNested(name + '_');
                    setValue(name, options.empty_value);
                }
            });

            var text_to_choose = '';

            if (jQuery.isFunction(options.choose)) {
                var level = $(this).siblings().andSelf().filter('select').length;
                text_to_choose = options.choose.apply(this, [level]);
            } else if ( options.choose !== '' ) {
                text_to_choose = options.choose;
            }

            // if show multiple -> show open select
            var count_tree_objects = 0;
            if ( text_to_choose !== '' ) {
              // we have a default value
              count_tree_objects++;
            }
            if (options.show_multiple > 1) {
                count_tree_objects = options.show_multiple;
            } else if (options.show_multiple === true) {
              $.each(tree, function() {
                 count_tree_objects++;
              });
            }
            if ( count_tree_objects > 1 ){
              $select.attr('size', count_tree_objects);
            }

            if ($(this).is('input')) {
                $select.insertBefore(this);
            } else {
                $select.insertAfter(this);
            }

            if (options.select_class) {
                $select.addClass(options.select_class);
            }

            if ( text_to_choose !== '' ) {
              $("<option>").html(text_to_choose).val('').appendTo($select);
            }

            var foundPreselect = false;
            $.each(tree, function(k, v) {
                var label, value;
                if (options.indexed) {
                    label = v;
                    value = k;
                } else {
                    label = value = k;
                }
                var o = $("<option>").html(label)
                    .attr('value', value);
                var clean = cleanName(name);
                    if (options.leaf_class && typeof value !== 'object') { // this option is a leaf node
                        o.addClass(options.leaf_class);
                    }

                    o.appendTo($select);
                    if (isPreselectedFor(clean, value)) {
                      o.get(0).selected = true;
                      foundPreselect = true;
                    }
            });

            if (foundPreselect) {
              $select.change();
            }

            if (!foundPreselect && options.preselect_only_once) { // clear preselect on first not-found level
                options.preselect[cleanName(name)] = null;
            }

        } else if (options.set_value_on === 'leaf') { // single option is selected by the user (function called via onchange event())
            if (options.indexed) {
                setValue(name, this.options[this.selectedIndex].value);
            } else {
                setValue(name, tree);
            }
        }
    });

};
}(jQuery));

/**
 * jQuery.textareaCounter
 * Version 1.0
 * Copyright (c) 2011 c.bavota - http://bavotasan.com
 * Dual licensed under MIT and GPL.
 * Date: 10/20/2011
**/

(function($){
	$.fn.textareaCounter = function(options) {
		// setting the defaults
		// $("textarea").textareaCounter({ limit: 100 });
		var defaults = {
			limit: 100,
            uniqueId: ''
		};	
		var options = $.extend(defaults, options);
 
		// and the plugin begins
		return this.each(function() {
			var obj, text, wordcount, limited;

			obj = $(this);

            var counterTextId = 'counter-text-' + obj.attr('id')

			obj.after('<span style="font-size: 11px; clear: both; margin-top: 3px; display: block;" id="'+ counterTextId +'">Max. '+options.limit+' words</span>');

			obj.keyup(function() {
			    text = obj.val();
			    if(text === "") {
			    	wordcount = 0;
			    } else {
				    wordcount = $.trim(text).split(" ").length;
				}
			    if(wordcount > options.limit) {
			        $("#" + counterTextId).html('<span style="color: #DD0000;">0 words left</span>');
					limited = $.trim(text).split(" ", options.limit);
					limited = limited.join(" ");
					$(this).val(limited);
			    } else {
			        $("#" + counterTextId).html((options.limit - wordcount)+' words left');
			    } 
			});
		});
	};
})(jQuery);

/*Password Strength Indicator using jQuery and XMLBy: Bryian Tan (bryian.tan at ysatech.com)*/

(function ($) {

    var password_Strength = new function () {

        //return count that match the regular expression
        this.countRegExp = function (passwordVal, regx) {
            var match = passwordVal.match(regx);
            return match ? match.length : 0;
        }

        this.getStrengthInfo = function (passwordVal) {
            var len = passwordVal.length;
            var pStrength = 0; //password strength
            var msg = "", inValidChars = ""; //message

            //get special characters from xml file
            var allowableSpecilaChars = new RegExp("[" + password_settings.specialChars + "]", "g")

            var nums = this.countRegExp(passwordVal, /\d/g), //numbers
			lowers = this.countRegExp(passwordVal, /[a-z]/g),
			uppers = this.countRegExp(passwordVal, /[A-Z]/g), //upper case
			specials = this.countRegExp(passwordVal, allowableSpecilaChars), //special characters
			spaces = this.countRegExp(passwordVal, /\s/g);

            //check for invalid characters
            inValidChars = passwordVal.replace(/[a-z]/gi, "") + inValidChars.replace(/\d/g, "");
            inValidChars = inValidChars.replace(/\d/g, "");
            inValidChars = inValidChars.replace(allowableSpecilaChars, "");

            //check space
            if (spaces > 0) {
                return "No spaces!";
            }

            //invalid characters
            if (inValidChars !== '') {
                return "Invalid character: " + inValidChars;
            }

            //max length
            if (len > password_settings.maxLength) {
                return "Password too long!";
            }

            //GET NUMBER OF CHARACTERS left
            if ((specials + uppers + nums + lowers) < password_settings.minLength) {
                msg += password_settings.minLength - (specials + uppers + nums + lowers) + " more characters, ";
            }

            //at the "at least" at the front
            if (specials == 0 || uppers == 0 || nums == 0 || lowers == 0) {
                msg += "At least ";
            }

            //GET NUMBERS
            if (nums >= password_settings.numberLength) {
                nums = password_settings.numberLength;
            }
            else {
                msg += (password_settings.numberLength - nums) + " more numbers, ";
            }

            //special characters
            if (specials >= password_settings.specialLength) {
                specials = password_settings.specialLength
            }
            else {
                msg += (password_settings.specialLength - specials) + " more symbol, ";
            }

            //upper case letter
            if (uppers >= password_settings.upperLength) {
                uppers = password_settings.upperLength
            }
            else {
                msg += (password_settings.upperLength - uppers) + " Upper case characters, ";
            }

            //strength for length
            if ((len - (uppers + specials + nums)) >= (password_settings.minLength - password_settings.numberLength - password_settings.specialLength - password_settings.upperLength)) {
                pStrength += (password_settings.minLength - password_settings.numberLength - password_settings.specialLength - password_settings.upperLength);
            }
            else {
                pStrength += (len - (uppers + specials + nums));
            }

            //password strength
            pStrength += uppers + specials + nums;

            //detect missing lower case character
            if (lowers === 0) {
                if (pStrength > 1) {
                    pStrength -= 1; //Reduce 1
                }
                msg += "1 lower case character, ";
            }

            //strong password
            if (pStrength == password_settings.minLength && lowers > 0) {
                msg = "Strong password!";
            }

            return msg + ';' + pStrength;
        }

        this.testPassword = function (inputField, container, password_settings) {
                
                var passwordVal = $(inputField).val(); //get textbox value

                //set met requirement to false
                password_settings.metRequirement = false;

                if (passwordVal.length > 0) {

                    var msgNstrength = password_Strength.getStrengthInfo(passwordVal);

                    var msgNstrength_array = msgNstrength.split(";"), strengthPercent = 0,
                    barWidth = password_settings.barWidth, backColor = password_settings.barColor;

                    //calculate the bar indicator length
                    if (msgNstrength_array.length > 1) {
                        strengthPercent = (msgNstrength_array[1] / password_settings.minLength) * barWidth;
                    }

                    $("[id='PasswordStrengthBorder']").css({ display: 'inline', width: barWidth });

                    //use multiple colors
                    if (password_settings.useMultipleColors == "1") {
                        //first 33% is red
                        if (parseInt(strengthPercent) >= 0 && parseInt(strengthPercent) <= (barWidth * .33)) {
                            backColor = "red";
                        }
                        //33% to 66% is blue
                        else if (parseInt(strengthPercent) >= (barWidth * .33) && parseInt(strengthPercent) <= (barWidth * .67)) {
                            backColor = "blue";
                        }
                        else {
                            backColor = password_settings.barColor;
                        }
                    }

                    $("[id='PasswordStrengthBar']").css({ display: 'inline', width: strengthPercent, 'background-color': backColor });

                    //remove last "," character
                    if (msgNstrength_array[0].lastIndexOf(",") !== -1) {
                        container.text(msgNstrength_array[0].substring(0, msgNstrength_array[0].length - 2));
                    }
                    else {
                        container.text(msgNstrength_array[0]);
                    }

                    if (strengthPercent == barWidth) {
                        password_settings.metRequirement = true;
                    }

                }
                else {
                    container.text('');
                    $("[id='PasswordStrengthBorder']").css("display", "none"); //hide
                    $("[id='PasswordStrengthBar']").css("display", "none"); //hide
                }
        }

    }

    //default setting
    var password_settings = {
        minLength: 6,
        maxLength: 25,
        specialLength: 0,
        upperLength: 1,
        numberLength: 1,
        barWidth: 200,
        barColor: 'Red',
        specialChars: '!@#$', //allowable special characters
        metRequirement: false,
        useMultipleColors: 0
    };

    //password strength plugin 
    $.fn.password_strength = function (options) {

        //check if password met requirement
        this.metReq = function () {
            return password_settings.metRequirement;
        }

        //read password setting from xml file
        $.ajax({
            type: "GET",
            url: "/ewcommon/tools/passwordpolicy.ashx", //use absolute link if possible
            dataType: "xml",
            success: function (xml) {

                $(xml).find('Password').each(function () {
                    var _minLength = $(this).find('minLength').text(),
                    _maxLength = $(this).find('maxLength').text(),
                    _numsLength = $(this).find('numsLength').text(),
                    _upperLength = $(this).find('upperLength').text(),
                    _specialLength = $(this).find('specialLength').text(),
                    _barWidth = $(this).find('barWidth').text(),
                    _barColor = $(this).find('barColor').text(),
                    _specialChars = $(this).find('specialChars').text(),
                    _useMultipleColors = $(this).find('useMultipleColors').text();

                    //set variables
                    password_settings.minLength = parseInt(_minLength);
                    password_settings.maxLength = parseInt(_maxLength);
                    password_settings.specialLength = parseInt(_specialLength);
                    password_settings.upperLength = parseInt(_upperLength);
                    password_settings.numberLength = parseInt(_numsLength);
                    password_settings.barWidth = parseInt(_barWidth);
                    password_settings.barColor = _barColor;
                    password_settings.specialChars = _specialChars;
                    password_settings.useMultipleColors = _useMultipleColors;
                });
            }
        });

        return this.each(function () {

            //bar position
            var barLeftPos = $("[id='" + this.id + "']").position().left - 10;
            var barTopPos = $("[id='" + this.id + "']").position().top + $("[id='" + this.id + "']").height() + 30;

            //password indicator text container
            var container = $('<span></span>')
            .css({ position: 'absolute', top: barTopPos - 6, left: barLeftPos + 15, 'font-size': '75%', display: 'block', width: password_settings.barWidth + 40 });

            //add the container next to textbox
            $(this).after(container);
            $(this).css({ 'margin-bottom': 40});
            //bar border and indicator div
            var passIndi = $('<div id="PasswordStrengthBorder"></div><div id="PasswordStrengthBar" class="BarIndicator"></div>')
            .css({ position: 'absolute', display: 'none' })
            .eq(0).css({ height: 3, top: barTopPos - 16, left: barLeftPos + 15, 'border-style': 'solid', 'border-width': 1, padding: 2 }).end()
            .eq(1).css({ height: 5, top: barTopPos - 14, left: barLeftPos + 17 }).end()

            //set max length of textbox
            //$("[id='" + this.id + "']").attr('maxLength', password_settings.maxLength);

            //add the boder and div
            container.before(passIndi);

            //run once on load
            password_Strength.testPassword(this, container, password_settings);

            $(this).keyup(function () {

                password_Strength.testPassword(this, container, password_settings);

            });
        });
    };

})(jQuery);

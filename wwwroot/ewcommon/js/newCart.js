﻿$(document).ready(function () {
    if ($("form#contact").exists()) {
   
        if ($('#cDelContactAddress').val() == $('#cContactAddress').val()) {
            if ($('#cContactAddress').val() == '') {
                //empty so hide delivery
                addDeliveryAddress();
            }
            else {
                //same so hide delivery
                addDeliveryAddress();
            }
        }
        else {
            //tick deliver top this address
            $('input[name="cIsDelivery"]').attr('checked', 'checked')
        }

        //when is delivery clicked
        $('input[name="cIsDelivery"]').click(function () {
           // alert($(this).attr('value'));
            if ($(this).attr('value') == 'true') {
                resetDelAddress();
            }
            else {
                if ($.isNumeric($(this).attr('value')) == true) {

                    blankoutFormFields($('#cDelContactName'),'Collection');
                    blankoutFormFields($('#cDelContactCompany'), 'Collection');
                    blankoutFormFields($('#cDelContactAddress'), 'Collection');
                    blankoutFormFields($('#cDelContactCity'), 'Collection');
                    blankoutFormFields($('#cDelContactState'), 'Collection');
                    blankoutFormFields($('#cDelContactZip'), 'Collection');
                    blankoutFormFields($('#cDelContactCountry'), $('#cContactCountry').val());
                    blankoutFormFields($('#cDelContactTel'), 'Collection');
                    blankoutFormFields($('#cDelContactFax'), 'Collection');

                } else {
                    addDeliveryAddress();
                }
                
            }
        });

        //when form submitted
        $('input[name="cartBillAddress"]').click(function () {

            if ($('#cIsDelivery_false').exists()) {
                if ($('#cIsDelivery_false:checked').val() == 'false') {
                    //   alert('do nowt');
                    addDeliveryAddress();
                }
            }
            else {
                if ($('#cIsDelivery_true:checked').val() != 'true') {
                    //   alert('do nowt');
                    addDeliveryAddress();
                }
            };
        });

        //when form submitted
        $('button[name="cartBillAddress"]').click(function () {
            // alert($('#cIsDelivery_false:checked').val());
            if ($('#cIsDelivery_false').exists()) {
                if ($('#cIsDelivery_false:checked').val() == 'false') {
                    //   alert('do nowt');
                    addDeliveryAddress();
                }
            }
            else {
                if ($('#cIsDelivery_true:checked').val() != 'true') {
                    //   alert('do nowt');
                    addDeliveryAddress();
                }
            }
        });
    }

    if ($("form#PayForm").exists()) {

        if ($('.radiocheckbox input[type=radio][value="Solo"]').is(':checked')) {
            $('.ccIssue').slideDown();
            $('.issueNumber').slideDown();
        }
        if ($('.radiocheckbox input[type=radio][value="SOLO"]').is(':checked')) {
            $('.ccIssue').slideDown();
            $('.issueNumber').slideDown();
        }

        if ($('.radiocheckbox input[type=radio][value="Switch"]').is(':checked')) {
            $('.ccIssue').slideDown();
            $('.issueNumber').slideDown();
        }

        if ($('.radiocheckbox input[type=radio][value="MAESTRO"]').is(':checked')) {
            $('.ccIssue').slideDown();
            $('.issueNumber').slideDown();
        }

        $('.radiocheckbox input').click(function () {
            if (($(this).is(':checked')) && (($(this).attr('value') == 'Switch') || ($(this).attr('value') == 'MAESTRO') || ($(this).attr('value') == 'SOLO') || ($(this).attr('value') == 'Solo'))) {
                $('.ccIssue').slideDown();
                $('.issueNumber').slideDown();
            }
            else {
                $('.ccIssue').slideUp();
                $('.issueNumber').slideUp();
            }
        });

    }

    cartActionListeners();

});

/* Function to trigger listners for the cartListing */
function cartActionListeners() {

    /* Add to cart button action */
    $('#addPromoCode').click(function (clicky) {
        clicky.preventDefault();
        addPromoCodeAPIUrl = '/ewapi/Cms.Cart/AddDiscountCode';
        discountCode = $('#txtPromoCode').val();

       // alert(discountCode);
        var inputJson = {
            Code: discountCode
        };

        $.ajax(addPromoCodeAPIUrl, {
            data: JSON.stringify(inputJson),
            contentType: 'application/json',
            type: 'POST'
        }).done(function (data) {
            if (data === discountCode) {
                location.reload();
            }
            else {
                $('.promo-alert').remove();
                $('.basket-promo').append('<div class="alert promo-alert">' + data + '</div>')
            }
        }
        );
        return false;
    });

   // alert("listeners on");
}


function resetDelAddress() {
    $('#cDelContactName').attr('value', '');
    $('#cDelContactName').removeAttr('readonly');
    $('#cDelContactName').removeClass('greyed');
    $('#cDelContactCompany').attr('value', '');
    $('#cDelContactCompany').removeAttr('readonly');
    $('#cDelContactCompany').removeClass('greyed');
    $('#cDelContactAddress').attr('value', '');
    $('#cDelContactAddress').removeAttr('readonly');
    $('#cDelContactAddress').removeClass('greyed');
    $('#cDelContactCity').attr('value', '');
    $('#cDelContactCity').removeAttr('readonly');
    $('#cDelContactCity').removeClass('greyed');
    $('#cDelContactState').attr('value', '');
    $('#cDelContactState').removeAttr('readonly');
    $('#cDelContactState').removeClass('greyed');
    $('#cDelContactZip').attr('value', '');
    $('#cDelContactZip').removeAttr('readonly');
    $('#cDelContactZip').removeClass('greyed');
    $('#cDelContactCountry').attr('value', '');
    $('#cDelContactCountry').removeAttr('readonly');
    $('#cDelContactCountry').removeClass('greyed');
    $('#cDelContactTel').attr('value', '');
    $('#cDelContactTel').removeAttr('readonly');
    $('#cDelContactTel').removeClass('greyed');
    $('#cDelContactFax').attr('value', '');
    $('#cDelContactFax').removeAttr('readonly');
    $('#cDelContactFax').removeClass('greyed');
    $(".column2 .group label").each(function () {
        if ($(this).attr('for') != 'cIsDelivery_1') {
            $(this).removeClass('greyed');
        }
    });
}

function addDeliveryAddress() {
    var a = '';
    blankoutFormFields($('#cDelContactName'), $('#cContactName').val());
    blankoutFormFields($('#cDelContactCompany'), $('#cContactCompany').val());
    blankoutFormFields($('#cDelContactAddress'), $('#cContactAddress').val());
    blankoutFormFields($('#cDelContactCity'), $('#cContactCity').val());
    blankoutFormFields($('#cDelContactState'), $('#cContactState').val());
    blankoutFormFields($('#cDelContactZip'), $('#cContactZip').val());
    blankoutFormFields($('#cDelContactCountry'), $('#cContactCountry').val());
    blankoutFormFields($('#cDelContactTel'), $('#cContactTel').val());
    blankoutFormFields($('#cDelContactFax'), $('#cContactFax').val());

    var foo = [];
    $('#cDelContactCountry option:selected').each(function (i, selected) {
        foo[i] = $(selected).text();
    });

    $(".column2 .group label").each(function () {
        if ($(this).attr('for') != 'cIsDelivery_1') {
            $(this).addClass('greyed');
        }
    });
    /*alert('See all Billing details copied across to Delivery form...');*/
}

function blankoutFormFields(oInput, val) {
    oInput.attr('value', val);
    oInput.attr('readonly', 'readonly');
    oInput.addClass('greyed');
}

$("#cIsCSUser_true").change(function () {

    if ($('#cIsCSUser_true').exists()) {
        if ($('#cIsCSUser_true:checked').val() == 'true') {
            $("#cContactEmail").val("noreply@intotheblue.com");
            $("#cContactEmail").prop("readonly", true);
        }
        else {
            $("#cContactEmail").val("Please enter email");
            $("#cContactEmail").removeAttr('readonly');
        }
    }

});


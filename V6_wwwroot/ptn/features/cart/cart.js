$(document).ready(function () {

    $(".pay-button").hide();

    if ($("form#contact").exists()) {

        $(".delivery-address").hide();

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
        $('input[name="cIsDelivery"]').change(function () {

            if ($('input[name="cIsDelivery"]').is(":checked")) {
                $(".delivery-address").show();
                resetDelAddress();
            } 
            if ($('input[name="cIsDelivery"]').is(":checked") == false) {
                $(".delivery-address").hide();
                if ($.isNumeric($(this).attr('value')) == true) {

                    blankoutFormFields($('#cDelContactName'), 'Collection');
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
        $(".pay-button").hide();
        $("#confirmterms_Agree").change(function () {
            if (this.checked) {
                $(".pay-button").show();
                enablePayPal();
            } else {
                $(".pay-button").hide();
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

    $('.responsive-cart .cart-quantity').on('change', function () {
        $('#updateQty').click();
    });

    initialiseProductSKUs();
});


$("#confirmterms_Agree").change(function () {
    if (this.checked) {
        //$("#confirmterms_Agree").attr("disabled", true);
        $(".dummy-pay-button").hide();
        $(".pay-button").show();
        enablePayPal();
    } else {
        $(".dummy-pay-button").show();
        $(".pay-button").hide();
    }
});

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
            $('.qtybox').attr('name', skuName);
            //.attr('id', skuId)

        }

        if (rrp != 'na') {
            $(priceId + " span.rrpPrice span[itemprop='price'],")
                .html(rrp);
        }

        if (salePrice != '') {
            $(priceId + " span.price span[itemprop='price'], " + priceId + " span.price span[itemprop='price'],")
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

    $(".column2 .group label").each(function () {
        if ($(this).attr('for') != 'cIsDelivery_1') {
            $(this).addClass('greyed');
        }
    });
    /*alert('See all Billing details copied across to Delivery form...');*/
}

function blankoutFormFields(oInput, val) {
    $(oInput).val(val);
    //oInput.attr('value', val);
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

function incrementQuantityNum(inputName, operator, amount) {
    if (operator == '+') {
        document.getElementById(inputName).value = (document.getElementById(inputName).value * 1) + amount;
    }
    else {
        if (document.getElementById(inputName).value > 0) {
            document.getElementById(inputName).value = (document.getElementById(inputName).value * 1) - amount;
        }

    }
}
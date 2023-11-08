﻿
(function () {
    'use strict'

    // Fetch all the forms we want to apply custom Bootstrap validation styles to
    var forms = document.querySelectorAll('.needs-validation')

    // Loop over them and prevent submission
    Array.prototype.slice.call(forms)
        .forEach(function (form) {
            form.addEventListener('submit', function (event) {
                if (!form.checkValidity()) {
                    event.preventDefault()
                    event.stopPropagation()
                }
                form.classList.add('was-validated')
            }, false)
        })
})()

function form_check(oForm) {
    return true;
}

function keepAlive() {
    /* call keep alive every 10 mins */
    $.get("/ptn/tools/keepalive.ashx", function () { setTimeout("keepAlive();", 600000); });
}

var redirectAPIUrl = '/ewapi/Cms.Admin/RedirectPage';
var IsParentPageAPI = '/ewapi/Cms.Admin/IsParentPage';
var checkiFrameLoaded;

$(document).ready(function () {
    $(".all-breadcrumb").click(function () {
        $(".admin-breadcrumb").addClass("breadcrumb-height");
        $(".all-breadcrumb").hide();
        $(".less-breadcrumb").show();
        return false;
    });
    $(".less-breadcrumb").click(function () {
        $(".admin-breadcrumb").removeClass("breadcrumb-height");
        $(".all-breadcrumb").show();
        $(".less-breadcrumb").hide();
        return false;
    });
    if ($(".admin-breadcrumb").height() < $(".admin-breadcrumb-inner").height()) {
        $(".all-breadcrumb").show();

    };
    $("[data-bs-toggle=popover]").popover({
        html: true,
        content: function () {
            return $(this).next('.popover-content').html();
        }
    });

    $('#tpltAdvancedMode #accordion .panel a').addClass('accordion-load');

    $('#accordion .panel a').click(function () {
        $(this).removeClass('accordion-load');
        $(this).removeClass('accordion-open');
    });

    


    initialiseGeocoderButton();

    $('form.xform').prepareAdminXform();

    //$(document).prepareEditable();

    //Setup the mainmenu popup 
    /*$('#adminOptions').dialog({
    bgiframe: true,
    height: 275,
    width: 800,
    modal: true,
    autoOpen: false,
    position: 'center'
    });*/

    // PICK IMAGE - run through all image containers, making same height as row.
    if ($('.pickByImage').exists()) {
        matchHeightCol($(this).find('.item'), '2');
    }

    if ($('select.jobOccupationDropdown').exists()) {
        $.getJSON('/ewcommon/jsondata/joboccupations.json', function (obj) {
            $.each(obj, function (key, value) {
                var option = $('<option />').val(value.Code).text(value.Title);
                $("select.jobOccupationDropdown").append(option);
            });
            $('select.jobOccupationDropdown').on('change', function () {
                var thisVal = $(this).val()
                $.each(obj, function (key, value) {
                    if (thisVal == value.Code) {
                        $("input.jobOccupationDropdownName").val(value.Title);
                        $("input.jobOccupationDropdownDesc").val(value.Description);
                    }
                });
            });
        });
    }

    $('#mainMenuButton').click(function () {

        //$('#adminOptions').dialog('open');
        $('#adminOptions').modal({
            minHeight: 275,
            minWidth: 800,
            overlayClose: true,
            autoResize: true,
            autoPosition: true,
            opacity: 80,
            zIndex: 90000,
            containerId: 'adminOptions-container'
        });
    });

    $('.pleaseWait').click(function () {
        var pleasewaitmessage = $(this).data('pleasewaitmessage')
        if (pleasewaitmessage == '') {
            pleasewaitmessage = 'Please Wait...'
        }
        $.modal('<div class="ewModal"><span><img src="/ewcommon/images/admin/ajax-loader2.gif"/>' + pleasewaitmessage + '<br/>This may take some time</span></div>', {
            minHeight: 100,
            minWidth: 300,
            overlayClose: true,
            autoResize: true,
            autoPosition: true,
            opacity: 80,
            zindex: 10000,
            containerId: 'adminOptions-container'
        });
    });

    $('.bs-please-wait').click(function () {
        var pleasewaitmessage = $(this).data('pleasewaitmessage')
        if (pleasewaitmessage == '') {
            pleasewaitmessage = 'Please Wait...'
        }
        var pleasewaitdetail = $(this).data('pleasewaitdetail')
        //alert(pleasewaitmessage);
        waitingDialog.show(pleasewaitmessage, pleasewaitdetail, { dialogSize: 'sm', progressType: 'warning' });

    });



    var waitingDialog = waitingDialog || (function ($) {
        'use strict';

        // Creating modal dialog's DOM
        var $dialog = $(
            '<div class="modal fade" data-backdrop="static" data-keyboard="false" tabindex="-1" role="dialog" aria-hidden="true" style="padding-top:15%; overflow-y:visible;">' +
            '<div class="modal-dialog modal-m">' +
            '<div class="modal-content">' +
            '<div class="modal-header"><h3 style="margin:0;"><i class="fa fa-coffee"> </i> <span><span></h3></div>' +
            '<div class="modal-body">' +
            '<p></p>' +
            '<div class="progress progress-striped active" style="margin-bottom:0;"><div class="progress-bar" style="width: 100%"></div></div>' +
            '</div>' +
            '</div></div></div>');

        return {
            /**
             * Opens our dialog
             * @param message Custom message
             * @param options Custom options:
             * 				  options.dialogSize - bootstrap postfix for dialog size, e.g. "sm", "m";
             * 				  options.progressType - bootstrap postfix for progress bar type, e.g. "success", "warning".
             */
            show: function (message, detail, options) {
                // Assigning defaults
                if (typeof options === 'undefined') {
                    options = {};
                }
                if (typeof message === 'undefined') {
                    message = 'Loading';
                }
                var settings = $.extend({
                    dialogSize: 'm',
                    progressType: '',
                    onHide: null // This callback runs after the dialog was hidden
                }, options);

                // Configuring dialog
                $dialog.find('.modal-dialog').attr('class', 'modal-dialog').addClass('modal-' + settings.dialogSize);
                $dialog.find('.progress-bar').attr('class', 'progress-bar');
                if (settings.progressType) {
                    $dialog.find('.progress-bar').addClass('progress-bar-' + settings.progressType);
                }
                $dialog.find('h3 span').text(message);
                $dialog.find('p').text(detail);
                // Adding callbacks
                if (typeof settings.onHide === 'function') {
                    $dialog.off('hidden.bs.modal').on('hidden.bs.modal', function (e) {
                        settings.onHide.call($dialog);
                    });
                }
                // Opening dialog
                $dialog.modal();
            },
            /**
             * Closes dialog
             */
            hide: function () {
                $dialog.modal('hide');
            }
        };

    })(jQuery);

    // ON ADMIN MENU CLICK 
    //$('#mainMenuButtonadminOptions').click(function (e) {
    //    e.preventDefault();
    //    $('#adminOptions').modal({ onOpen: modalOpen });
    //});


    $('#ThemePreset').change(function () {
        $("#WebSettings").submit();
    });

    var treeviewPath = getAdminAjaxTreeViewPath();

    $('#tpltAdvancedMode #MenuTree').ajaxtreeview({
        loadPath: treeviewPath,
        ajaxCmd: 'GetAdvNode',
        hide: true
    });

    $('#tpltEditStructure #MenuTree').ajaxtreeview({
        loadPath: treeviewPath,
        ajaxCmd: 'GetStructureNode',
        hide: true
    });

    $('#tpltMovePage #MenuTree').ajaxtreeview({
        loadPath: treeviewPath,
        ajaxCmd: 'GetMoveNode',
        hide: true
    });

    $('#tpltMoveContent #MenuTree').ajaxtreeview({
        loadPath: treeviewPath,
        ajaxCmd: 'GetMoveContent',
        hide: true
    });

    $('#tpltLocateContent #MenuTree').ajaxtreeview({
        loadPath: treeviewPath,
        ajaxCmd: 'GetLocateNode',
        hide: true
    });

    $('#tpltShippingLocations #MenuTree').ajaxtreeview({
        loadPath: treeviewPath,
        ajaxCmd: '',
        hide: true
    });

    $('#template_permissions #MenuTree').ajaxtreeview({
        loadPath: treeviewPath,
        ajaxCmd: '',
        hide: true
    });

    $('#template_FileSystem').find('#MenuTree').ajaxtreeview({
        loadPath: treeviewPath,
        ajaxCmd: 'GetFolderNode',
        openLevel: 2,
        hide: true
    });

    $('.pick-page #MenuTree').ajaxtreeview({
        loadPath: treeviewPath,
        ajaxCmd: '',
        openLevel: 2,
        hide: true
    });


    $(function () {
        var zIndexNumber = 9000;
        $('div.options:not(#isotope-module div.options)').each(function () {
            $(this).css('zIndex', zIndexNumber);
            zIndexNumber -= 1;
        });
    });

    $(function () {
        var zIndexNumber = 20000;
        $('#isotope-module .editable,#isotope-module div.options').each(function () {
            $(this).css('zIndex', zIndexNumber);
            zIndexNumber -= 1;
        });
    });

    $("a.popup").adminPopup();

    initialiseHelptips();

    setEditImage();

    $('.update-content-value-dd').each(function () {

        $(this).find('li a').click(function () {
            var thisButton = $(this);
            var thisLabel = thisButton.closest('.dropdown').find('.updatedValue');
            var thisToggle = thisButton.closest('.dropdown').find('.dropdown-toggle');
            $.ajax({
                type: 'post',
                url: $(this).attr('href'),
                dataType: 'html',
                success: function (msg) {
                    thisLabel.text(thisButton.text());
                    thisToggle.dropdown("toggle");
                }
            });
            return false;
        });
    });

    $('#files .item-image .panel').prepareLibImages();

    //select all checkboxes
    $(".select-all").change(function () {  //"select all" change 
        $(".checkbox input:not(:disabled)").prop('checked', $(this).prop("checked")); //change all ".checkbox" checked status
    });

    //".checkbox" change 
    $('.checkbox input').change(function () {
        //uncheck "select all", if one of the listed checkbox item is unchecked
        if (false === $(this).prop("checked")) { //if this item is unchecked
            $(".select-all").prop('checked', false); //change "select all" checked status to false
        }
        //check "select all" if all checkbox items are checked
        if ($('.checkbox input:checked').length === $('.checkbox input').length) {
            $(".select-all").prop('checked', true);
        }
    });

    $('#BulkContentAction').on('submit', function (e) {
        e.preventDefault();
        var ids = $('input[type=checkbox][name=id]:checked').map(
            function () { return this.value; }).get().join();
        var bulkAction = $('select[name=BulkAction]').val();
        var pgid = $('input[name=pgid]').val();
        window.location = '?ewCmd=BulkContentAction&BulkAction=' + bulkAction + "&pgid=" + pgid + '&id=' + ids;
    });

    $('#BulkAction').on('change', function (e) {
        //default all enabled
        var inventoryProductCheckboxes = $('.inventory-bulk-checkbox');
        inventoryProductCheckboxes.prop('disabled', false);

        //handle delete case
        var valueSelected = this.value;
        if (valueSelected.toLowerCase() === "delete") {
            var activeProductCheckboxes = $('.inventory-bulk-checkbox[data-status="1"]');
            activeProductCheckboxes.prop('checked', false);
            activeProductCheckboxes.prop('disabled', true);
        }

        //select all value based on items checked.
        if (inventoryProductCheckboxes.length == $(".inventory-bulk-checkbox:checked").length) {
            $(".select-all").prop('checked', true);
        } else {
            $(".select-all").prop('checked', false);
        }
    });

    prepareAjaxModals()

});

function preparePickImageModal(CurrentModalPath) {
    var treeviewPath = getAdminAjaxTreeViewPath();
    var currentModal = $(CurrentModalPath);
    var multiple = "";

    if ($('#template_FileSystem #MenuTree').data('multiple') == 1) {
       multiple = "&multiple=true"        
    };
        //activateTreeview
    if ($('#template_FileSystem #MenuTree').exists()) {
        $('#template_FileSystem #MenuTree').ajaxtreeview({
            loadPath: treeviewPath + "&popup=true&libType=" + $('#template_FileSystem #MenuTree').data("lib-type").replace("Lib", "") + "&targetForm=" + $('#template_FileSystem #MenuTree').data("target-form") + "&targetField=" + $('#template_FileSystem #MenuTree').data("target-field") + "&targetClass=" + $('#template_FileSystem #MenuTree').data("target-class") + multiple,
            ajaxCmd: 'GetFolderNode',
            openLevel: 2,
            hide: true
        });
    };

    $('#files .item-image .panel').prepareLibImages();

    currentModal.find("[data-bs-toggle=popover]").popover({
            html: true,
            container: '#files',
            trigger: 'hover',
            viewport: '#files',
            content: function () {
                return currentModal.prev('.popoverContent').html();
        }
    });
    currentModal.find("a[data-bs-toggle!='popover']").click(function (ev) {
        
        ev.preventDefault();
            currentModal.find('.modal-dialog').addClass('loading')
            currentModal.find('.modal-content div').html('<div><p class="text-center"><h4><i class="fa fa-cog fa-spin fa-2x fa-fw"> </i> Loading ...</h4></p></div>');
        var target = $(this).attr("href");
        // load the url and call this again on success
        if (target != '#') {
        
            currentModal.find(".modal-content div").load(target, function () {
                $('.modal-dialog').removeClass('loading')
                preparePickImageModal(CurrentModalPath)
                $('.lazy').lazy();
                primeFileUpload();
            });
        };
    });

    $("#SelectAll").click(function (ev) {
        ev.preventDefault();
        $(".multicheckbox").each(function () {
            $(".multicheckbox").attr("checked", "checked");
        });
        return false;
    });

    currentModal.find('form:not([id="imageDetailsForm"])').on('submit', function (event) {

            event.preventDefault()
            var formData = $(this).serialize();
            var targetUrl = $(this).attr("action") + '&contentType=popup';
            $(this).find('.modal-content').html('<p class="text-center"><h4><i class="fa fa-cog fa-spin fa-2x fa-fw"> </i> Loading ...</h4></p>');

            $.ajax({
                type: 'post',
                url: targetUrl,
                data: formData,
                dataType: 'html',
                success: function (msg) {
                    //$(this).find('.modal-dialog').removeClass('loading')
                    currentModal.find(".modal-content div").html(msg);
                    currentModal.trigger('loaded');
                }
            });
    });
}


function prepareAjaxModals() {
    $('a[data-bs-toggle="modal"]').off('click');
    $('a[data-bs-toggle="modal"]').on('click', function (e) {
        e.preventDefault();
        var link = $(this)
        resetAjaxModal(link.attr('data-bs-target'))
        var content = $(link.attr('data-bs-target') + " .modal-content");
        content.load(link.attr("href"));
    });
}

function resetAjaxModal(ref) {
    $(ref).find('.modal-body').html('<p class="text-center"><h4><i class="fa fa-cog fa-spin fa-2x fa-fw"> </i> Loading...</h4></p>');
}

function setEditImage() {
    $('a.editImage').each(function (index, value){
        var targetForm = $(this).parents('form').attr('id');
        var targetField = $(this).parents('.form-margin').children('textarea').attr('id');
        var imgtag = $(this).parents('.form-margin').children('textarea').val();
        imgtag = $.trim(imgtag);
        imgtag = encodeURIComponent(imgtag);
        var cName = "";
        var linkUrl = '?contentType=popup&ewCmd=ImageLib&targetForm=' + targetForm + '&ewCmd2=editImage&imgHtml=' + imgtag + '&targetField=' + targetField
        var link = $(this)
        var content = $(link.attr('data-bs-target') + " .modal-content");
        link.attr("href", linkUrl)
    });
};

function initialiseHelptips() {
    $(".helpTip").tooltip({
        track: true,
        relative: true,
        hide: { duration: 1000000 },
        content: function () {
            var element = $(this);
            return element.attr("title");
        }
    });
};

function S4() {
    return Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1);
}

function getUploadedImageHtmlPopup(appPath, fld, targetPath, targetField, filename) {

    var guid = (S4() + S4() + "-" + S4() + "-4" + S4().substr(0, 3) + "-" + S4() + "-" + S4() + S4() + S4()).toLowerCase();

    var newItem = '<div class="item item-image col">';
    newItem = newItem + '<div class="panel">';
    newItem = newItem + '<div class="image-thumbnail">	';
    newItem = newItem + '<div class="popoverContent" id="imgpopover' + guid + '" role="tooltip">';
    newItem = newItem + '<img src="' + targetPath + '/' + filename.replace(/\ /g, '-') + '" class="img-responsive" />';
    newItem = newItem + '<div class="popover-description">';
    newItem = newItem + '<span class="image-description-name">' + filename.replace(/\ /g, '-') + '</span>';
    newItem = newItem + '<br/>';
    newItem = newItem + '</div>';
    newItem = newItem + '</div>';
    newItem = newItem + '<a data-bs-toggle="popover" data-trigger="hover" data-container="body" data-contentwrapper="#imgpopover' + guid + '" data-placement="top">';
    newItem = newItem + '<img src="' + targetPath + '/' + filename.replace(/\ /g, '-') + '" class="img-responsive" />';
    newItem = newItem + '  </a>';
    newItem = newItem + '</div>';

    newItem = newItem + '<div class="img-description"> ';
    newItem = newItem + '  <span class="image-description-name">' + filename.replace(/\ /g, '-') + '</span>';
    newItem = newItem + '	  </div>';
    newItem = newItem + '<div class="pick-btn">      ';
    newItem = newItem + '<a href="' + appPath + '?contentType=popup&amp;ewcmd=ImageLib&amp;ewCmd2=pickImage&amp;fld=' + fld + '&amp;file=' + filename.replace(/\ /g, '-') + '" class="btn btn-sm btn-primary pickImage">';
    newItem = newItem + '<i class="fa fa-picture-o fa-white"> ';
    newItem = newItem + '<xsl:text> </xsl:text>';
    newItem = newItem + ' </i> Pick Image';
    newItem = newItem + ' </a>';
    newItem = newItem + '</div>';
    newItem = newItem + '</div>';
    newItem = newItem + '</div>';

    return newItem;
}

function getUploadedImagePathPopup(appPath, fld, targetPath, targetField, filename) {

    var guid = (S4() + S4() + "-" + S4() + "-4" + S4().substr(0, 3) + "-" + S4() + "-" + S4() + S4() + S4()).toLowerCase();

    var imgfullpath = targetPath + '/' + filename.replace(/\ /g, '-');

    var img = new Image();
    var imgheight = 0;
    var imgwidth = 0;
    img.onload = function () {
        imgheight = img.height;
        imgwidth = img.width;
    }
    img.src = imgfullpath;

    var newItem = '<div class="item item-image col"> ';
    newItem = newItem + '<div class="panel">';
    newItem = newItem + '<div class="image-thumbnail">';
    newItem = newItem + '<div class="popoverContent" id="imgpopover' + guid + '" role="tooltip">';
    newItem = newItem + filename.replace(/\ /g, '-') + '<br />';
    newItem = newItem + '</div>';
    newItem = newItem + '<a data-toggle="popover" data-trigger="hover" data-container="body" data-contentwrapper="#imgpopover' + guid + '" data-placement="top">';
    newItem = newItem + '<img src="' + imgfullpath + '" class="img-responsive" />';
    newItem = newItem + '</a>';
    newItem = newItem + '</div>';
    newItem = newItem + '<div class="img-description"><span class="image-description-name">' + filename.replace(/\ /g, ' -') + '</span>' + imgheight + 'x' + imgwidth +'</div>';
    newItem = newItem + '<div class="pick-btn">      ';
    newItem = newItem + '<a onclick="passImgFileToForm(\'EditContent\',\'' + targetField + '\',\'' + targetPath + '/' + filename.replace(/\ /g, '-') + '\');" class="btn btn-sm btn-primary pickImage">';
    newItem = newItem + '<i class="fa fa-picture-o fa-white">';
    newItem = newItem + '<xsl:text> </xsl:text>';
    newItem = newItem + '</i> Pick Image';
    newItem = newItem + '</a>';
    newItem = newItem + '</div>';
    newItem = newItem + '</div>';
    newItem = newItem + '</div>';

    return newItem;
}


(function () {
    $.fn.jqueryLoad = $.fn.load;

    $.fn.load = function (url, params, callback) {
        var $this = $(this);
        var cb = $.isFunction(params) ? params : callback || $.noop;
        var wrapped = function (responseText, textStatus, XMLHttpRequest) {
            cb(responseText, textStatus, XMLHttpRequest);
            $this.trigger('loaded');
        };

        if ($.isFunction(params)) {
            params = wrapped;
        } else {
            callback = wrapped;
        }

        $this.jqueryLoad(url, params, callback);

        return this;
    };
})();



(function ($) {
    $.fn.prepareLibImages = function () {
        return this.each(function () {
            var thisButton = $(this)
            thisButton.hover(function () {
                thisButton.closest('.panel').find('.description').show()
            }, function () {
                thisButton.closest('.panel').find('.description').hide()
            });
        });
    };
}(jQuery));

$.fn.prepareAdminXform = function () {

    if ($("#_selectAll").exists()) {
        $("#_selectAll").click(function () {
            var checked = this.checked;
            $("input[name=Results]").each(function () {
                this.checked = checked;
            });
        });
    }

    if ($(this).find('select.fontSelect').exists()) {
        $(this).find('select.fontSelect').each(function (i) {

            $(this).change(function () {
                var myname = $(this).attr('name')
                var myvalue = $(this).val()
                $('input#' + myname + '-family').val(myvalue.substr(0, myvalue.indexOf('|')))
                $('input#' + myname + '-import').val(myvalue.substr(myvalue.indexOf('|') + 1, myvalue.length))
            });
        });
    };

    if ($(this).find('textarea.pickImage').exists()) {
        $(this).find('textarea.pickImage').each(function (i) {
            $(this).text($(this).text().replace('></img>', '/>'))
            $(this).text($(this).text().replace('">', '"/>'))
        });
    }

    if ($(this).find('.field-char-count').exists()) {

        $(this).find('.field-char-count').each(function (i) {
            var fieldRef = $(this).data("fieldref");

            checkTextAreaMaxLength(fieldRef, event);

            $("#" + fieldRef).on("keyup", function (event) {
                checkTextAreaMaxLength(fieldRef, event);
            });
        });
    }

    if ($(this).find("#dEventDate-alt").exists() && $(this).find("#dEventEndDate-alt").exists()) {

        $(document).on("change", "#dEventDate-alt", function () {
            setDefaultEventEndDate();
        });

        $(document).on("change", "#dEventEndDate-alt", function () {
            var startEventDate = $("#EditContent #dEventDate-alt").val();
            var endEventDate = $("#EditContent #dEventEndDate-alt").val();
            if (Date.parse(endEventDate) < Date.parse(startEventDate)) {
                $("#EditContent #dEventEndDate").val('');
                $("#EditContent #dEventEndDate-alt").val('');
            }
        });
    }

    if ($(this).find('input.constrain-proportions').exists()) {
        $(this).find('input.constrain-proportions').each(function (i) {
            initialiseConstrainProportions('cConstrainProportions', 'cResizeImage_2', 'cImageWidth', 'cImageHeight', 'cContentImage');
        });
    }

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
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'))
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl)
    })


};

function passImgToForm(targetForm, targetField) {
    cUrl = document.forms['imageDetailsForm'].elements['cPathName'].value
    cAlt = document.forms['imageDetailsForm'].elements['cDesc'].value
    cWidth = document.forms['imageDetailsForm'].elements['nWidth'].value
    cHeight = document.forms['imageDetailsForm'].elements['nHeight'].value
    cName = document.forms['imageDetailsForm'].elements['cName'].value

    cImgHtml = '<img src="' + cUrl.replace(/ /g, "%20") + '" width="' + '75' + '" height="' + ((cHeight / cWidth) * 75) + '" alt="' + cAlt + '"'
    if (cName != '') {
        cImgHtml = cImgHtml + ' class="' + cName + '"'
    }
    cImgHtml = cImgHtml + '/>'

    imgtag = '<img src="' + cUrl.replace(/ /g, "%20") + '" width="' + cWidth + '" height="' + cHeight + '" alt="' + cAlt + '"'
    if (cName != '') {
        imgtag = imgtag + ' class="' + cName + '"'
    }
    imgtag = imgtag + '/>'

    // alert(targetField + '-' + $('#' + targetField).hasClass('xhtml'));

    if ($('#' + targetField).hasClass('xhtml')) {
        tinymce.activeEditor.insertContent(imgtag);
    }
    else {

        $('#' + targetField).val(imgtag);
        editDiv = $('#editImage_' + targetField);
        previewDiv = $('#previewImage_' + targetField + ' span');
        // previewDiv.innerHTML = '<a href="#" onclick="OpenWindow_edit_' + targetField + '()" title="edit an image from the image library" class="btn btn-sm btn-primary"><i class="icon-edit icon-white"> </i> Edit image</a><br/><a href="#" onclick="xfrmClearImage(\'EditContent\',\'' + targetField + '\',\'' + cName + '\');return false" title="Clear Image" class="btn btn-sm btn-danger"><i class="icon-remove-circle icon-white"> </i> Clear image</a>';
        $(editDiv).find('.btn-group-spaced .clearImage').show();
        $(editDiv).find('.btn-group-spaced .editImage').show();
        $(editDiv).find('.btn-group-spaced .pickImage').hide();
        //add preview Image
        previewDiv.html(cImgHtml)
        setEditImage();
      //  $('<div class="previewImage" id="previewImage_' + targetField + '"><span>' + cImgHtml + '</span></div>').insertAfter(editDiv);
      //  previewDiv.remove();
    }
    var thisModal = bootstrap.Modal.getInstance($("#modal-" + targetField))
    thisModal.hide();
    $("#modal-" + targetField).removeData('bs.modal');
    resetAjaxModal(targetField);
  
}

function passDocToForm(targetForm, targetField, cUrl) {
    $('#' + targetField).val(cUrl);
    buttonDiv = $('#editDoc_' + targetField + '  .input-group-btn');
    buttonDiv.replaceWith("<a href=\"#\" onclick=\"xfrmClearDocument('" + targetForm + "','" + targetField + "');return false\" title=\"Remove current Document reference\" class=\"btn btn-danger\"><i class=\"fas fa-times fa-white\"> </i> Clear</a>")
    var thisModal = bootstrap.Modal.getInstance($("#modal-" + targetField))
    thisModal.hide();
    $("#modal-" + targetField).removeData('bs.modal');
    resetAjaxModal(targetField);
}

function passMediaToForm(targetForm, targetField, cUrl) {
    $('#' + targetField).val(cUrl);
    buttonDiv = $('#editDoc_' + targetField + '  .input-group-btn');
    buttonDiv.replaceWith("<a href=\"#\" onclick=\"xfrmClearMedia('" + targetForm + "','" + targetField + "');return false\" title=\"Remove current Image reference\" class=\"btn btn-danger input-group-btn\"><i class=\"fas fa-times fa-white\"> </i> Clear</a>")
    var thisModal = bootstrap.Modal.getInstance($("#modal-" + targetField))
    thisModal.hide();
    $("#modal-" + targetField).removeData('bs.modal');
    resetAjaxModal(targetField);
}

function passImgFileToForm(targetForm, targetField, cUrl) {
    $('#' + targetField).val(cUrl);
    buttonDiv = $('#editImageFile_' + targetField + '  .input-group-btn');
    buttonDiv.replaceWith("<a href=\"#\" onclick=\"xfrmClearImgFile('" + targetForm + "','" + targetField + "');return false\" title=\"Remove current File reference\" class=\"btn btn-danger input-group-btn\"><i class=\"fas fa-times fa-white\"> </i> Clear</a>")
    var thisModal = bootstrap.Modal.getInstance($("#modal-" + targetField))
    thisModal.hide();
    $("#modal-" + targetField).removeData('bs.modal');
    resetAjaxModal(targetField);
}

function xfrmClearImage(formRef, fieldRef, className) {
    document.forms[formRef].elements[fieldRef].value = '<img class="' + className + '"/>';
    previewDiv = $('#previewImage_' + fieldRef + ' span');
    editDiv = $('#editImage_' + fieldRef);
    previewDiv.html('<i class="fas fa-image">&#160;</i>');
    editDiv.find('a.clearImage').hide();
    editDiv.find('a.editImage').hide();
    editDiv.find('a.pickImage').show();
}

function xfrmClearDocument(formRef, fieldRef) {
    document.forms[formRef].elements[fieldRef].value = '';
    buttonDiv = $('#editDoc_' + fieldRef + '  .input-group-btn');
    buttonDiv.replaceWith('<a data-bs-toggle="modal" href="?contentType=popup&ewCmd=DocsLib&amp;targetForm=' + formRef + '&amp;targetField=' + fieldRef + '" title="pick an document from the image library" data-bs-target="#modal-' + fieldRef + '" class="btn btn-primary input-group-btn"><i class="fas fa-image fa-white"> </i> Pick</a>')
}

function xfrmClearMedia(formRef, fieldRef) {
    document.forms[formRef].elements[fieldRef].value = '';
    buttonDiv = $('#editDoc_' + fieldRef + '  .input-group-btn');
    buttonDiv.replaceWith('<a data-bs-toggle="modal" href="?contentType=popup&ewCmd=MediaLib&amp;targetForm=' + formRef + '&amp;targetField=' + fieldRef + '" title="pick an document from the image library" data-bs-target="#modal-' + fieldRef + '" class="btn btn-primary input-group-btn"><i class="fa fa-music fa-white"> </i> Pick</a>')
}

function xfrmClearImgFile(formRef, fieldRef) {
    document.forms[formRef].elements[fieldRef].value = '';
    buttonDiv = $('#editImageFile_' + fieldRef + '  .input-group-btn');
    buttonDiv.replaceWith('<a data-bs-toggle="modal" href="?contentType=popup&ewCmd=ImageLib&amp;ewCmd2=PathOnly&amp;targetForm=' + formRef + '&amp;targetField=' + fieldRef + '" title="pick an document from the image library" data-bs-target="#modal-' + fieldRef + '" class="btn btn-primary input-group-btn"><i class="fas fa-image fa-white"> </i> Pick</a>')
}

function xfrmClearCalendar(formRef, fieldRef) {
    document.forms[formRef].elements[fieldRef].value = '';
    document.getElementById('dateDisplay-' + fieldRef).innerHTML = '';
}

function passFilePathToForm(targetField, filepath) {
    opener.document.forms['EditContent'].elements[targetField].value = filepath;
    window.close();
}

function updatePreviewImage(formRef, fieldRef) {
    imgtag = document.forms[formRef].elements[fieldRef].value;
    previewDiv = document.getElementById('previewImage_' + fieldRef);
    previewDiv.innerHTML = '<a href="#" onclick="OpenWindow_edit_' + fieldRef + '();return false" title="edit an image from the image library" class="btn btn-sm btn-primary input-group-btn"><i class="fa-picture-o fa-white"> </i> Edit</a>' + imgtag;
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

function markAsRead(userId, artId) {
    var logActivityAPIUrl = "/ewapi/Cms.Content/LogActivity";

    var jsObj = { 'type': 'PageViewed', 'userId': userId, 'pageId': '', 'artId': artId };

    $.ajax(logActivityAPIUrl, {
        data: JSON.stringify(jsObj),
        contentType: 'application/json',
        type: 'POST'
    }).done(function () {
        $('#hide-' + artId).remove();

    });

}

// This function returns the value of a queryString
function getParameterByName(qStringName) {
    qStringName = qStringName.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regexS = "[\\?&]" + qStringName + "=([^&#]*)";
    var regex = new RegExp(regexS);
    var results = regex.exec(window.location.href);
    if (results == null)
        return "";
    else
        return decodeURIComponent(results[1].replace(/\+/g, " "));
}

/**
* Adds an event to the "get geocode" button on Location edit page
*/
function initialiseGeocoderButton() {
    $('.getGeocodeButton').click(function (e) {
        // Prevent form submission
        e.preventDefault();

        // Store initial label
        var $this = $(this);
        var label = $this.val();

        // Create an array of address details
        var address = [
            $('#cContentLocationNo').val(),
            $('#cContentLocationStreet').val(),
            $('#cContentLocationTown').val(),
            $('#cContentLocationRegion').val(),
            $('#cContentLocationPostCode').val(),
        ];

        // Turn address array into a comma separated string
        var addressString = address.join(',');
        //alert(addressString);
        // Change label
        $this.val('Please wait...');

        var geocoder = new google.maps.Geocoder();

        geocoder.geocode({ address: addressString }, function (results, status) {
            if (status == google.maps.GeocoderStatus.OK) {

                // Set lat and long values in relevant inputs
                var location = results[0].geometry.location;
                $('#cContentLocationLat').val(location.lat());
                $('#cContentLocationLong').val(location.lng());

            } else {

                alert(status + ' Couldn\'t find the latitude and longitude for the address provided. Try including more details.(' + addressString + ')');

            }

            // Change to initial button label
            $this.val(label);
        });
    });

    $('.getGeocodeButton2').click(function (e) {
        // Prevent form submission
        e.preventDefault();

        // Store initial label
        var $this = $(this);
        var label = $this.val();

        // Create an array of address details
        var address = [
            $('#cLocationName').val(),
            $('#cLocationStreet').val(),
            $('#cLocationTown').val(),
            $('#cLocationRegion').val(),
            $('#cLocationPostCode').val(),
        ];

        // Turn address array into a comma separated string
        var addressString = address.join(',');

        // Change label
        $this.val('Please wait...');

        var geocoder = new google.maps.Geocoder();

        geocoder.geocode({ address: addressString }, function (results, status) {
            if (status == google.maps.GeocoderStatus.OK) {

                // Set lat and long values in relevant inputs
                var location = results[0].geometry.location;
                $('#cLocationLat').val(location.lat());
                $('#cLocationLong').val(location.lng());

            } else {

                alert('Couldn\'t find the latitude and longitude for the address provided. Try including more details.(' + addressString + ')');

            }

            // Change to initial button label
            $this.val(label);
        });
    });


}

//User Guide

$(function () {
    //$('#btnHelpEditing span i').addClass('fa-chevron-up');
    $('#btnHelpEditing').click(function () {

        $('#helpBox p').load($('#userGuideURL').attr('href'), afterLoad); // Load in userGuideURL defined in admin.xsl
        if ($('#divHelpBox').hasClass('open')) // Animation of the user guide pane and button (sliding)
        { $('#divHelpBox').animate({ right: -350 }, { duration: 500 }).removeClass('open'); }
        else
            $('#divHelpBox').animate({ right: 0 }, { duration: 500 }).addClass('open');
        if ($('#btnHelpEditing').hasClass('open')) {
            $('#btnHelpEditing').animate({ right: 0 }, { duration: 500 }).css({ backgroundPosition: '0 0' }).removeClass('open');
        }
        else
            $('#btnHelpEditing').animate({ right: 350 }, { duration: 500 }).css({ backgroundPosition: '-34px 0' }).addClass('open');
        $('#btnHelpEditing span i').toggleClass('fa-chevron-down');
    });


    $(window).scroll(resizeHelpBox);
    $(window).resize(resizeHelpBox);
    //    $(window).scroll(resizeScroll);
    //    $(window).resize(resizeScroll);
    resizeHelpBox();
});
function resizeHelpBox() { // Resizes the user guide pane to always fill the height of the window
    var top = $('#adminHeader').height() - $(window).scrollTop(),
        top = top > 0 ? top : 0, // Don't want top to be less than zero
        height = $(window).height() - top - 5,
        $btn = $('#btnHelpEditing');
    $('#divHelpBox').css('top', top);
    $btn.css('top', top + (height - $btn.outerHeight()) / 2);
    $('.scroll-pane-arrows').css('height', height);
}
function afterLoad() { // After the content from the user guide is loaded
    //resizeScroll();
    $('#helpBox .SubPages .subpageItem .entryFooter').remove();
    $('#helpBox .module .tl .title a').contents().unwrap(); // Remove the links around titles
    $('#helpBox a.lightbox').magnificPopup({ type: 'image' });
    $('.scroll-pane-arrows').addClass('nobackground'); // Remove loading.gif
    //    $('#userguideFormToggle').click(function () {
    //        if ($('#userguideForm').hasClass('userguideFormVisibile')) {
    //            $('#userguideForm').removeClass('userguideFormVisibile')
    //        }
    //        else
    //            $('#userguideForm').addClass('userguideFormVisibile')
    //    });
    $('#helpBox a').not('.lightbox, .externallink').click(function (e) { // When clicking a link in the user guide that isn't an image
        e.preventDefault();
        $('.scroll-pane-arrows').removeClass('nobackground'); // Add loading.gif
        $('#helpBox p').empty() // Clear contents so the loading.gif can be seen and prevent multiples links being clicked
        $('#helpBox p').load(this.href, afterLoad); // Load in the relevant user guide content of the new link
    });
}
//function resizeScroll() {
//    var top = 0;
//    top = $('#adminHeader').height() - $(window).scrollTop();
//    if (top < 0) top = 0;
//    $('.scroll-pane-arrows').jScrollPane(
//		{
//		    showArrows: true,
//		    horizontalGutter: 10
//		});
//}

/* ==========================================================
* bootstrap-formhelpers-selectbox.js
* https://github.com/vlamanna/BootstrapFormHelpers
* ==========================================================
* Copyright 2012 Vincent Lamanna
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
* ========================================================== */

+function ($) {

    'use strict';


    /* SELECTBOX CLASS DEFINITION
    * ========================= */

    var toggle = '[data-toggle=bfh-selectbox]',
        BFHSelectBox = function (element, options) {
            this.options = $.extend({}, $.fn.bfhselectbox.defaults, options);
            this.$element = $(element);

            this.initSelectBox();
        };

    BFHSelectBox.prototype = {

        constructor: BFHSelectBox,

        initSelectBox: function () {
            var options;

            options = '';
            this.$element.children('div').each(function () {
                options = options + '<li><a tabindex="-1" href="#" data-option="' + $(this).data('value') + '">' + $(this).html() + '</a></li>';
            });

            this.$element.html(
                '<input type="hidden" name="' + this.options.name + '" value="">' +
                '<a class="bfh-selectbox-toggle ' + this.options.input + '" role="button" data-toggle="bfh-selectbox" href="#">' +
                '<span class="bfh-selectbox-option"></span>' +
                '<span class="' + this.options.icon + ' selectbox-caret"></span>' +
                '</a>' +
                '<div class="bfh-selectbox-options">' +
                '<div role="listbox">' +
                '<ul role="option">' +
                '</ul>' +
                '</div>' +
                '</div>'
            );

            this.$element.find('[role=option]').html(options);

            if (this.options.filter === true) {
                this.$element.find('.bfh-selectbox-options').prepend('<div class="bfh-selectbox-filter-container"><input type="text" class="bfh-selectbox-filter form-control"></div>');
            }

            this.$element.val(this.options.value);

            this.$element
                .on('click.bfhselectbox.data-api touchstart.bfhselectbox.data-api', toggle, BFHSelectBox.prototype.toggle)
                .on('keydown.bfhselectbox.data-api', toggle + ', [role=option]', BFHSelectBox.prototype.keydown)
                .on('mouseenter.bfhselectbox.data-api', '[role=option] > li > a', BFHSelectBox.prototype.mouseenter)
                .on('click.bfhselectbox.data-api', '[role=option] > li > a', BFHSelectBox.prototype.select)
                .on('click.bfhselectbox.data-api', '.bfh-selectbox-filter', function () { return false; })
                .on('propertychange.bfhselectbox.data-api change.bfhselectbox.data-api input.bfhselectbox.data-api paste.bfhselectbox.data-api', '.bfh-selectbox-filter', BFHSelectBox.prototype.filter);
        },

        toggle: function (e) {
            var $this,
                $parent,
                isActive;

            $this = $(this);
            $parent = getParent($this);

            if ($parent.is('.disabled') || $parent.attr('disabled') !== undefined) {
                return true;
            }

            isActive = $parent.hasClass('open');

            clearMenus();

            if (!isActive) {
                $parent.trigger(e = $.Event('show.bfhselectbox'));

                if (e.isDefaultPrevented()) {
                    return true;
                }

                $parent
                    .toggleClass('open')
                    .trigger('shown.bfhselectbox')
                    .find('[role=option] > li > [data-option="' + $parent.val() + '"]').focus();
            }

            return false;
        },

        filter: function () {
            var $this,
                $parent,
                $items;

            $this = $(this);
            $parent = getParent($this);

            $items = $('[role=option] li a', $parent);
            $items
                .hide()
                .filter(function () {
                    return ($(this).text().toUpperCase().indexOf($this.val().toUpperCase()) !== -1);
                })
                .show();
        },

        keydown: function (e) {
            var $this,
                $items,
                $parent,
                $subItems,
                isActive,
                index,
                selectedIndex;

            if (!/(38|40|27)/.test(e.keyCode)) {
                return true;
            }

            $this = $(this);

            e.preventDefault();
            e.stopPropagation();

            $parent = getParent($this);
            isActive = $parent.hasClass('open');

            if (!isActive || (isActive && e.keyCode === 27)) {
                if (e.which === 27) {
                    $parent.find(toggle).focus();
                }

                return $this.click();
            }

            $items = $('[role=option] li:not(.divider) a:visible', $parent);

            if (!$items.length) {
                return true;
            }

            $('body').off('mouseenter.bfh-selectbox.data-api', '[role=option] > li > a', BFHSelectBox.prototype.mouseenter);
            index = $items.index($items.filter(':focus'));

            if (e.keyCode === 38 && index > 0) {
                index = index - 1;
            }

            if (e.keyCode === 40 && index < $items.length - 1) {
                index = index + 1;
            }

            if (!index) {
                index = 0;
            }

            $items.eq(index).focus();
            $('body').on('mouseenter.bfh-selectbox.data-api', '[role=option] > li > a', BFHSelectBox.prototype.mouseenter);
        },

        mouseenter: function () {
            var $this;

            $this = $(this);

            $this.focus();
        },

        select: function (e) {
            var $this,
                $parent,
                $span,
                $input;

            $this = $(this);

            e.preventDefault();
            e.stopPropagation();

            if ($this.is('.disabled') || $this.attr('disabled') !== undefined) {
                return true;
            }

            $parent = getParent($this);

            $parent.val($this.data('option'));
            $parent.trigger('change.bfhselectbox');

            clearMenus();
        }

    };

    function clearMenus() {
        var $parent;

        $(toggle).each(function (e) {
            $parent = getParent($(this));

            if (!$parent.hasClass('open')) {
                return true;
            }

            $parent.trigger(e = $.Event('hide.bfhselectbox'));

            if (e.isDefaultPrevented()) {
                return true;
            }

            $parent
                .removeClass('open')
                .trigger('hidden.bfhselectbox');
        });
    }

    function getParent($this) {
        return $this.closest('.bfh-selectbox');
    }


    /* SELECTBOX PLUGIN DEFINITION
    * ========================== */

    var old = $.fn.bfhselectbox;

    $.fn.bfhselectbox = function (option) {
        return this.each(function () {
            var $this,
                data,
                options;

            $this = $(this);
            data = $this.data('bfhselectbox');
            options = typeof option === 'object' && option;
            this.type = 'bfhselectbox';

            if (!data) {
                $this.data('bfhselectbox', (data = new BFHSelectBox(this, options)));
            }
            if (typeof option === 'string') {
                data[option].call($this);
            }
        });
    };

    $.fn.bfhselectbox.Constructor = BFHSelectBox;

    $.fn.bfhselectbox.defaults = {
        icon: 'caret',
        input: 'form-control',
        name: '',
        value: '',
        filter: false
    };


    /* SELECTBOX NO CONFLICT
    * ========================== */

    $.fn.bfhselectbox.noConflict = function () {
        $.fn.bfhselectbox = old;
        return this;
    };


    /* SELECTBOX VALHOOKS
    * ========================== */

    var origHook;
    if ($.valHooks.div) {
        origHook = $.valHooks.div;
    }
    $.valHooks.div = {
        get: function (el) {
            if ($(el).hasClass('bfh-selectbox')) {
                return $(el).find('input[type="hidden"]').val();
            } else if (origHook) {
                return origHook.get(el);
            }
        },
        set: function (el, val) {
            var $el,
                html;

            if ($(el).hasClass('bfh-selectbox')) {

                $el = $(el);
                if ($el.find('li a[data-option=\'' + val + '\']').length > 0) {
                    html = $el.find('li a[data-option=\'' + val + '\']').html();
                } else if ($el.find('li a').length > 0) {
                    html = $el.find('li a').eq(0).html();
                } else {
                    val = '';
                    html = '';
                }

                $el.find('input[type="hidden"]').val(val);
                $el.find('.bfh-selectbox-option').html(html);
            } else if (origHook) {
                return origHook.set(el, val);
            }
        }
    };


    /* SELECTBOX DATA-API
    * ============== */

    $(document).ready(function () {
        $('div.bfh-selectbox').each(function () {
            var $selectbox;

            $selectbox = $(this);

            $selectbox.bfhselectbox($selectbox.data());
        });
    });


    /* APPLY TO STANDARD SELECTBOX ELEMENTS
    * =================================== */

    $(document)
        .on('click.bfhselectbox.data-api', clearMenus);

}(window.jQuery);


jQuery.fn.reverse = function () {
    return this.pushStack(this.get().reverse(), arguments);
};

function unescapeHTML(escapedHTML) {
    return escapedHTML.replace(/&lt;/g, '<').replace(/&gt;/g, '>').replace(/&amp;/g, '&');
}

function formatXml(xmlstr) {
    var formatted = '';
    var reg = /(>)(<)(\/*)/g;
    xmlstr = xmlstr.replace(reg, '$1\r\n$2$3');
    var pad = 0;
    jQuery.each(xml.split('\r\n'), function (index, node) {
        var indent = 0;
        if (node.match(/.+<\/\w[^>]*>$/)) {
            indent = 0;
        } else if (node.match(/^<\/\w/)) {
            if (pad != 0) {
                pad -= 1;
            }
        } else if (node.match(/^<\w[^>]*[^\/]>.*$/)) {
            indent = 1;
        } else {
            indent = 0;
        }

        var padding = '';
        for (var i = 0; i < pad; i++) {
            padding += '  ';
        }

        formatted += padding + node + '\r\n';
        pad += indent;
    });

    return formatted;
}

var prettifyXml = function (sourceXml) {
    var xmlDoc = new DOMParser().parseFromString(sourceXml, 'application/xml');
    var xsltDoc = new DOMParser().parseFromString([
        // describes how we want to modify the XML - indent everything
        '<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform">',
        '  <xsl:output omit-xml-declaration="yes" indent="yes"/>',
        '    <xsl:template match="node()|@*">',
        '      <xsl:copy><xsl:apply-templates select="node()|@*"/></xsl:copy>',
        '    </xsl:template>',
        '</xsl:stylesheet>',
    ].join('\n'), 'application/xml');

    var xsltProcessor = new XSLTProcessor();
    xsltProcessor.importStylesheet(xsltDoc);
    var resultDoc = xsltProcessor.transformToDocument(xmlDoc);
    var resultXml = new XMLSerializer().serializeToString(resultDoc);
    return resultXml;
};

function formatXml2(xml) {
    var out = "";
    var tab = "    ";
    var indent = 0;
    var inClosingTag = false;
    var dent = function (no) {
        out += "\n";
        for (var i = 0; i < no; i++)
            out += tab;
    }


    for (var i = 0; i < xml.length; i++) {
        var c = xml.charAt(i);
        if (c == '<') {
            // handle </
            if (xml.charAt(i + 1) == '/') {
                inClosingTag = true;
                dent(--indent);
            }
            out += c;
        } else if (c == '>') {
            out += c;
            // handle />
            if (xml.charAt(i - 1) == '/') {
                out += "\n";
                //dent(--indent)
            } else {
                if (!inClosingTag)
                    dent(++indent);
                else {
                    out += "\n";
                    inClosingTag = false;
                }
            }
        } else {
            out += c;
        }
    }
    return out;
}

function checkTextAreaMaxLength(textBoxRef, e) {
    var myTextBox = $("#" + textBoxRef);
    var maxLength = parseInt(myTextBox.data("length"));

    if (!checkSpecialKeys(e)) {
        // if (myTextBox.value.val() > maxLength - 1) {
        //    $("#" + textBoxRef + "-char-count").
        //  }
    }
    $("#" + textBoxRef + "-char-count").html(myTextBox.val().length);
    return true;
}

function setDefaultEventEndDate() {
    var startEventDateAlt = $("#EditContent #dEventDate-alt").val();
    var startEventDate = $("#EditContent #dEventDate").val();

    var endEventDate = $("#EditContent #dEventEndDate-alt").val();
    if ((startEventDateAlt != null && startEventDateAlt != "") && (endEventDate == null || endEventDate == "" || Date.parse(endEventDate) < Date.parse(startEventDateAlt))) {
        $("#EditContent #dEventEndDate").val(startEventDate);
        $("#EditContent #dEventEndDate-alt").val(startEventDateAlt);
    }
}

/*
Checks if the keyCode pressed is inside special chars
-------------------------------------------------------
@prerequisite:	e = e.keyCode object for the key pressed
*/
function checkSpecialKeys(e) {
    if (e.keyCode != 8 && e.keyCode != 46 && e.keyCode != 37 && e.keyCode != 38 && e.keyCode != 39 && e.keyCode != 40)
        return false;
    else
        return true;
}


/**
*JQuery TinyMCE V4
*/

!function (t) { function e() { function e(t) { "remove" === t && this.each(function (t, e) { var n = r(e); n && n.remove() }), this.find("span.mceEditor,div.mceEditor").each(function (t, e) { var n = tinymce.get(e.id.replace(/_parent$/, "")); n && n.remove() }) } function i(t) { var n, i = this; if (null != t) e.call(i), i.each(function (e, n) { var i; (i = tinymce.get(n.id)) && i.setContent(t) }); else if (i.length > 0 && (n = tinymce.get(i[0].id))) return n.getContent() } function r(t) { var e = null; return t && t.id && a.tinymce && (e = tinymce.get(t.id)), e } function c(t) { return !!(t && t.length && a.tinymce && t.is(":tinymce")) } var u = {}; t.each(["text", "html", "val"], function (e, a) { var o = u[a] = t.fn[a], s = "text" === a; t.fn[a] = function (e) { var a = this; if (!c(a)) return o.apply(a, arguments); if (e !== n) return i.call(a.filter(":tinymce"), e), o.apply(a.not(":tinymce"), arguments), a; var u = "", l = arguments; return (s ? a : a.eq(0)).each(function (e, n) { var i = r(n); u += i ? s ? i.getContent().replace(/<(?:"[^"]*"|'[^']*'|[^'">])*>/g, "") : i.getContent({ save: !0 }) : o.apply(t(n), l) }), u } }), t.each(["append", "prepend"], function (e, i) { var a = u[i] = t.fn[i], o = "prepend" === i; t.fn[i] = function (t) { var e = this; return c(e) ? t !== n ? (e.filter(":tinymce").each(function (e, n) { var i = r(n); i && i.setContent(o ? t + i.getContent() : i.getContent() + t) }), a.apply(e.not(":tinymce"), arguments), e) : void 0 : a.apply(e, arguments) } }), t.each(["remove", "replaceWith", "replaceAll", "empty"], function (n, i) { var r = u[i] = t.fn[i]; t.fn[i] = function () { return e.call(this, i), r.apply(this, arguments) } }), u.attr = t.fn.attr, t.fn.attr = function (e, a) { var o = this, s = arguments; if (!e || "value" !== e || !c(o)) return a !== n ? u.attr.apply(o, s) : u.attr.apply(o, s); if (a !== n) return i.call(o.filter(":tinymce"), a), u.attr.apply(o.not(":tinymce"), s), o; var l = o[0], p = r(l); return p ? p.getContent({ save: !0 }) : u.attr.apply(t(l), s) } } var n, i, r = [], a = window; t.fn.tinymce = function (n) { function c() { var i = [], r = 0; e && (e(), e = null), l.each(function (t, e) { var a, c = e.id, u = n.oninit; c || (e.id = c = tinymce.DOM.uniqueId()), tinymce.get(c) || (a = new tinymce.Editor(c, n, tinymce.EditorManager), i.push(a), a.on("init", function () { var t, e = u; l.css("visibility", ""), u && ++r == i.length && ("string" == typeof e && (t = -1 === e.indexOf(".") ? null : tinymce.resolve(e.replace(/\.\w+$/, "")), e = tinymce.resolve(e)), e.apply(t || tinymce, i)) })) }), t.each(i, function (t, e) { e.render() }) } var u, o, s, l = this, p = ""; if (!l.length) return l; if (!n) return tinymce.get(l[0].id); if (l.css("visibility", "hidden"), a.tinymce || i || !(u = n.script_url)) 1 === i ? r.push(c) : c(); else { i = 1, o = u.substring(0, u.lastIndexOf("/")), -1 != u.indexOf(".min") && (p = ".min"), a.tinymce = a.tinyMCEPreInit || { base: o, suffix: p }, -1 != u.indexOf("gzip") && (s = n.language || "en", u = u + (/\?/.test(u) ? "&" : "?") + "js=true&core=true&suffix=" + escape(p) + "&themes=" + escape(n.theme || "") + "&plugins=" + escape(n.plugins || "") + "&languages=" + (s || ""), a.tinyMCE_GZ || (a.tinyMCE_GZ = { start: function () { function e(t) { tinymce.ScriptLoader.markDone(tinymce.baseURI.toAbsolute(t)) } e("langs/" + s + ".js"), e("themes/" + n.theme + "/theme" + p + ".js"), e("themes/" + n.theme + "/langs/" + s + ".js"), t.each(n.plugins.split(","), function (t, n) { n && (e("plugins/" + n + "/plugin" + p + ".js"), e("plugins/" + n + "/langs/" + s + ".js")) }) }, end: function () { } })); var f = document.createElement("script"); f.type = "text/javascript", f.onload = f.onreadystatechange = function (e) { e = e || event, ("load" == e.type || /complete|loaded/.test(f.readyState)) && (tinymce.dom.Event.domLoaded = 1, i = 2, n.script_loaded && n.script_loaded(), c(), t.each(r, function (t, e) { e() })) }, f.src = u, document.body.appendChild(f) } return l }, t.extend(t.expr[":"], { tinymce: function (t) { return !!(t.id && "tinymce" in window && tinymce.get(t.id)) } }) }(jQuery);

$(document).ready(function () {

    if (window.location.href.indexOf("PerPageCount") > -1) {
        $(".PrevPage").show();
    }
    else {
        $(".PrevPage").hide();
    }
});

$(document).on('click', '.nextPage', function () {


    var strUrl = window.location.href;
    var rangeForSearchForpage = 0;
    if (strUrl.indexOf("PerPageCount") > -1) {
        var url = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
        for (var i = 0; i < url.length; i++) {
            var urlparam = url[i].split('=');

            if (urlparam[0] == "page") {
                rangeForSearchForpage = urlparam[1];
            }
        }
        strUrl = strUrl.replace("page=" + rangeForSearchForpage, "page=" + (parseInt(rangeForSearchForpage) + 1));
        $(".PrevPage").show();
        window.document.location = strUrl;
    }
    else {
        strUrl = strUrl + "&page=1&PerPageCount=10";
        $(".PrevPage").show();
        window.document.location = strUrl;
    }
});

$(document).on('click', '.PrevPage', function () {


    var strUrl = window.location.href;

    var rangeForSearchForpage = 0;
    if (strUrl.indexOf("PerPageCount") > -1) {
        var url = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
        for (var i = 0; i < url.length; i++) {
            var urlparam = url[i].split('=');

            if (urlparam[0] == "page") {
                rangeForSearchForpage = urlparam[1];
            }
        }
        if (rangeForSearchForpage > 0) {
            strUrl = strUrl.replace("page=" + rangeForSearchForpage, "page=" + (parseInt(rangeForSearchForpage) - 1));
            if (rangeForSearchForpage == 1) {
                strUrl = strUrl.substring(0, strUrl.indexOf('&page'));
                $(".PrevPage").hide();
            }
        }
        else {
            strUrl = strUrl.replace("&page", "");
            $(".PrevPage").hide();
        }
        window.document.location = strUrl;
    }


});

function ValidateContentForm(event) {
    if (form_check(event)) {
        var pageId = this.getQueryStringParam('pgid');
        $(".hiddenType").val("Page");
        $(".hiddenPageId").val(pageId);
        var newStructName = $("#cStructName").val();
        editPage.structNameOnChange(newStructName);

    }
}

function RedirectClick(redirectType) {

    //var redirectType = $("redirectType").val();
    //alert(redirectType);
    if (redirectType == "404Redirect") {
        $("input[name*='redirectOption']").val("");
        if ($(".btnSubmitPage").length > 0) {
            $(".hiddenParentCheck").val("false");
            $("#redirectModal").modal("hide");
            $(".btnSubmitPage").click();
        }
    }
    else {

        $("input[name*='redirectOption']").val(redirectType);

        var pageId = $(".hiddenPageId").val();
        var type = $(".hiddenType").val();
        var inputJson = { pageId: pageId };
        var isParent = "false";
        if (type == "Page") {
            axios.post(IsParentPageAPI, inputJson)
                .then(function (response) {

                    if (response.data == "True") {
                        isParent = response.data;


                        $("#RedirectionChildConfirmationModal").modal("show");
                        $("#btnYescreateRuleForChild").removeAttr("disabled");
                        $("#btnNocreateRuleForChild").removeAttr("disabled");
                        $("input[name*='IsParent']").val(isParent);
                        $("#redirectModal").modal("hide");
                    }
                    else {
                        var newUrl = $("#NewUrl").val();
                        var oldUrl = $("#OldUrl").val();
                        inputJson = { redirectType: redirectType, oldUrl: oldUrl, newUrl: newUrl, pageId: pageId, isParent: isParent, pageType: type };
                        axios.post(redirectAPIUrl, inputJson)
                            .then(function (response) {
                                if (response.data == "success") {
                                    $("#redirectModal").modal("hide");
                                    $(".hiddenParentCheck").val("false");
                                    localStorage.originalStructName = newUrl;

                                }
                            });



                    }
                });
        }
        else if (type == "Product") {
            var newUrl = $("#NewUrl").val();
            var oldUrl = $("#OldUrl").val();
            inputJson = { redirectType: redirectType, oldUrl: oldUrl, newUrl: newUrl, pageId: pageId, isParent: isParent, pageType: type };
            axios.post(redirectAPIUrl, inputJson)
                .then(function (response) {
                    if (response.data == "success") {
                        $("#redirectModal").modal("hide");
                        $(".hiddenParentCheck").val("false");
                        localStorage.originalStructName = newUrl;
                        document.createElement('form').submit.call(document.EditContent);
                    }
                });
        }

        //}

    }




}//);

function CreateRedirectRule() {

    var newUrl = $("#NewUrl").val();
    var oldUrl = $("#OldUrl").val();
    var type = $(".hiddenType").val();
    inputJson = { redirectType: redirectType, oldUrl: oldUrl, newUrl: newUrl, pageId: pageId, isParent: isParent, pageType: type };
    axios.post(redirectAPIUrl, inputJson)
        .then(function (response) {
            if (response.data == "success") {
                $("#redirectModal").modal("hide");

            }
        });
}

$(document).on("click", "#btnNocreateRuleForChild", function (event) {

    $(".hiddenParentCheck").val("false");
    $("#RedirectionChildConfirmationModal").modal("hide");
    $("#redirectModal").modal("hide");
    document.createElement('form').submit.call(document.EditPage);

});

$(document).on("click", "#btnYescreateRuleForChild", function (event) {

    var pageId = $(".hiddenPageId").val();
    var redirectType = $("input[name*='redirectOption']").val();
    //alert(redirectType);
    var newUrl = $("#NewUrl").val();
    var oldUrl = $("#OldUrl").val();
    var type = $(".hiddenType").val();
    var isParent = $("input[name*='IsParent']").val();
    inputJson = { redirectType: redirectType, oldUrl: oldUrl, newUrl: newUrl, pageId: pageId, isParent: isParent, pageType: type };
    axios.post(redirectAPIUrl, inputJson)
        .then(function (response) {
            if (response.data == "success") {
                $("#RedirectionChildConfirmationModal").modal("hide");
                $("#redirectModal").modal("hide");
                document.createElement('form').submit.call(document.EditPage);


            }
        });

});

const editPageElement = document.querySelector("#EditPage");
if (editPageElement) {
    window.editPage = new Vue({
        el: "#EditPage",
        data: {
            structName: "",
            originalStructureName: ""
        },
        methods: {
            createRedirects: function () {
                $("#redirectModal").modal("hide");
                var redirectType = $(".redirectStatus:checked").val();

                if (redirectType == "" || redirectType == "404Redirect" || redirectType == undefined) {
                    return false;
                }
                else {

                    var newUrl = $("#cStructName").val();
                    var inputJson = { redirectType: redirectType, oldUrl: newUrl };
                    axios.post(IsUrlPResentAPI, inputJson)
                        .then(function (response) {

                            if (response.data == "True") {
                                if (confirm("Old url is already exist. Do you want to replace it?")) {

                                    $("#cRedirect").val(redirectType);

                                    var inputJson = { redirectType: redirectType, oldUrl: localStorage.originalStructName, newUrl: newUrl };
                                    axios.post(redirectUrl, inputJson)
                                        .then(function (response) {
                                            if (response.data == "success") {
                                                $("#redirectModal").modal("hide");

                                            }

                                        });
                                }
                                else {
                                    return false;
                                }
                            }
                            else {

                                $("#cRedirect").val(redirectType);
                                $("#redirectModal").modal("hide");

                            }
                        });


                }

            },

            structNameOnChange: function (newStructName) {
                if (localStorage.originalStructName && localStorage.originalStructName != "" && localStorage.originalStructName != newStructName) {
                    $('.btnRedirectSave').removeAttr("disabled");
                    $("#redirectModal").modal("show");
                    $("#OldUrl").val(localStorage.originalStructName);
                    $("#NewUrl").val(newStructName);
                    this.structName = newStructName;
                    $(".hiddenPageId").val(localStorage.pageId);
                    event.preventDefault();

                }
                else {

                    return true;
                }

            }
        },

        mounted: function () {

            var cStructName = document.getElementById('cStructName');
            if (cStructName != null) {
                this.structName = cStructName.value;
            }

            //clean the storage for struct name when page changes.
            let pageId = this.getQueryStringParam('pgid');
            if (!localStorage.pageId || localStorage.pageId != pageId) {
                localStorage.removeItem('originalStructName');
            }
            localStorage.pageId = pageId;
            localStorage.originalStructName = this.structName;
        }
    });

}


//End Page Edit


function ValidateProductForm(event) {

    if (form_check(event)) {
        var productId = this.getQueryStringParam('id');
        $(".hiddenParentCheck").val("False");
        $(".hiddenType").val("Product");
        $(".hiddenPageId").val(productId);
        var cNewContentPath = $("#cContentPath").val();
        return editProduct.UrlPathOnChange(cNewContentPath);

    }
}
//Edit Product
const editProductElement = $(".ProductSub").length;
if (editProductElement > 0) {
    window.editProduct = new Vue({
        el: ".ProductSub",
        data: {
            urlPathInput: "",
            originalPathName: ""
        },
        methods: {
            storedPath: function () {

                var cContentPath = $("#cContentPath").val();
                if (cContentPath != null) {
                    this.urlPathInput = cContentPath;
                }

                //clean the storage for struct name when page changes.
                let productId = this.getQueryStringParam('id');
                if (!localStorage.pageId || localStorage.pageId != productId) {
                    localStorage.removeItem('originalPathName');
                }
                localStorage.pageId = productId;
                //alert(this.urlPathInput);
                localStorage.originalPathName = this.urlPathInput;
            },
            UrlPathOnChange: function (newContentPath) {

                if (localStorage.originalPathName && localStorage.originalPathName != "" && localStorage.originalPathName != newContentPath) {
                    $('.btnRedirectSave').removeAttr("disabled");
                    $("#redirectModal").modal("show");
                    $("#OldUrl").val(localStorage.originalPathName);
                    $("#NewUrl").val(newContentPath);
                    this.cContentPath = newContentPath;
                    $(".hiddenPageId").val(localStorage.pageId);
                    $(".hiddenProductOldUrl").val(localStorage.originalPathName);
                    $(".hiddenProductNewUrl").val(newContentPath);
                    $(".hiddenRedirectType").val("301Redirect");
                    event.preventDefault();

                }
                else {

                    return true;
                }

            },
        },
        mounted: function () {
            this.storedPath();
        }
    });
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
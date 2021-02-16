
var checkiFrameLoaded;

$(document).ready(function () {

    $("[data-toggle=popover]").popover({
        html: true,
        content: function () {
            return $(this).prev('.popoverContent').html();
        }
    });

    $('#tpltAdvancedMode #accordion .panel a').addClass('accordion-load');

    $('#accordion .panel a').click(function () {
        $(this).removeClass('accordion-load');
        $(this).removeClass('accordion-open');
    });

    $(".admin-department-menu-js").hide();
    $(".absolute-admin-logo .department").hide();

    $(".absolute-admin-logo").click(function () {
        $(".admin-department-menu-js").toggle("slide");
        $(".absolute-admin-logo .department").toggle();
        $(".absolute-admin-logo .non-department").toggle();

    });

    //    $("#adminHeader .navbar-brand").click(function () {
    //        $(".admin-department-menu-js").slideDown();
    //        return false;
    //    });

    //    $(".admin-department-menu .navbar-brand").click(function () {
    //        $(".admin-department-menu-js").slideUp();
    //        return false;
    //    });

    $(".admin-main-menu .navbar-brand").hover(function () {
        $(".admin-sub-menu .navbar-brand").addClass("brand-hover")
    }, function () {
        $(".admin-sub-menu .navbar-brand").removeClass("brand-hover")
    });

    $(".admin-sub-menu .navbar-brand").hover(function () {
        $(".admin-main-menu .navbar-brand").addClass("brand-hover")
    }, function () {
        $(".admin-main-menu .navbar-brand").removeClass("brand-hover")
    });


    initialiseGeocoderButton();

    $('form.ewXform').prepareAdminXform();

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
    $('#mainMenuButtonadminOptions').click(function (e) {
        e.preventDefault();
        $('#adminOptions').modal({ onOpen: modalOpen });
    });

    $("#accordion").accordion({
        autoHeight: false,
        collapsible: true,
        header: 'div.header',
        navigation: true,
        heightStyle: 'content'
    });

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

    //   $('#tpltDeliveryMethodLocations #MenuTree').ajaxtreeview({
    //       loadPath: treeviewPath,
    //       ajaxCmd: 'GetLocateNode',
    //       hide: true
    //   });

    $('#template_permissions #MenuTree').ajaxtreeview({
        loadPath: treeviewPath,
        ajaxCmd: '',
        hide: true
    });

    $('#template_FileSystem #MenuTree').ajaxtreeview({
        loadPath: treeviewPath,
        ajaxCmd: '',
        openLevel: 2,
        hide: true
    });



    $('div.module div.moduleDrag').closest('.module').draggable({
        cursor: 'move',
        containment: '#dragableModules',
        handle: 'a.drag',
        revert: 'invalid',
        zindex: '10000',
        start: function (ev, ui) {
            shinkModuleContents($(this));
        },
        stop: function (ev, ui) {
            growModuleContents($(this));
        }
    });

    $('.moduleContainer .addmodule').droppable({
        accept: '.module',
        activeClass: 'droppable-active',
        hoverClass: 'droppable-hover',
        tolerance: 'pointer',
        drop: function (ev, ui) {
            acceptModule(ui.draggable, $(this));
            //reset dragg
            ui.draggable.attr('style', 'z-index: ' + ui.draggable.css('z-index') + ';')
        }
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
    //   $('.pickImageModal').on('shown.bs.modal', function () {
    $('.pickImageModal').on('loaded', function () {
        var currentModal = $(this)
        //activateTreeview
        $('#template_FileSystem #MenuTree').ajaxtreeview({
            loadPath: treeviewPath,
            ajaxCmd: '',
            openLevel: 2,
            hide: true
        });

        $('#files .item-image .panel').prepareLibImages();

        $("[data-toggle=popover]").popover({
            html: true,
            container: '#files',
            trigger: 'hover',
            viewport: '#files',
            content: function () {
                return $(this).prev('.popoverContent').html();
            }
        });

        $(this).find('a[data-toggle!="popover"]').click(function (ev) {
            ev.preventDefault();
            $('.modal-dialog').addClass('loading')
            $('.modal-body').html('<p class="text-center"><h4><i class="fa fa-cog fa-spin fa-2x fa-fw"> </i> Loading ...</h4></p>');
            var target = $(this).attr("href");
            // load the url and show modal on success
            currentModal.load(target, function () {
                $('.modal-dialog').removeClass('loading')
                currentModal.modal("show");

            });
        });

        $(this).find('form').on('submit', function (event) {

            event.preventDefault()
            var formData = $(this).serialize();
            var targetUrl = $(this).attr("action") + '&contentType=popup';
            $('.modal-dialog').addClass('loading')
            $('.modal-body').html('<p class="text-center"><h4><i class="fa fa-cog fa-spin fa-2x fa-fw"> </i> Loading ...</h4></p>');

            $.ajax({
                type: 'post',
                url: targetUrl,
                data: formData,
                dataType: 'html',
                success: function (msg) {
                    $('.modal-dialog').removeClass('loading')
                    $(".modal").html(msg);
                    currentModal.trigger('loaded');
                }
            });
        });
    });

    //    $('.pickImageModal').on('hidden.bs.modal', function () {
    //       // alert('bye');
    //        $(this).removeData('bs.modal');
    //    });

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
        $(".checkbox input").prop('checked', $(this).prop("checked")); //change all ".checkbox" checked status
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

});

function setEditImage() {
    $('a.editImage').click(function () {
        var targetForm = $(this).parents('form').attr('id');
        var imgtag = $(this).parents('.input-group').children('textarea').val();
        imgtag = encodeURIComponent(imgtag);
        var targetField = $(this).parents('.input-group').children('textarea').attr('id');
        var cName = "";
        var linkUrl = '?contentType=popup&ewCmd=ImageLib&targetForm=' + targetForm + '&ewCmd2=editImage&imgHtml=' + imgtag + '&targetField=' + targetField
        // alert(linkUrl);
        // $(this).attr("href", linkUrl); 
        $('#modal-' + targetField).load(linkUrl, function (e) { $('#modal-' + targetField).modal('show'); });
        return false;

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


function setMasonaryModuleWidth(pageId, contentId, newPos) {

    var ajaxurl = '?ewCmd=UpdatePosition' + decodeURIComponent("%26") + 'pgid=' + pageId + decodeURIComponent("%26") + 'id=' + contentId + decodeURIComponent("%26") + 'position=' + newPos + decodeURIComponent("%26") + 'reorder=false'
    //alert(ajaxurl);
    $.ajax({
        url: ajaxurl,
        success: function () {
            $('#mod_' + contentId).removeClass(function (index, css) {
                return (css.match(/\bpos-\S+/g) || []).join(' ');
            });
            $('#mod_' + contentId).addClass('pos-' + newPos);
            $('#isotope-module').isotope('reLayout');
            $(function () {
                var zIndexNumber = 9000;
                $('.editable,div.options,div.ewPopMenu').each(function () {
                    $(this).css('zIndex', zIndexNumber);
                    zIndexNumber -= 1;
                });
            });
        }
    });
};

function acceptModule($drag, $drop) {
    var pageId = $('body').attr('id').replace('pg_', '')
    var contentId = $drag.attr('id').replace('mod_', '')
    var ajaxurl = '?ewCmd=UpdatePosition' + decodeURIComponent("%26") + 'pgid=' + pageId + decodeURIComponent("%26") + 'id=' + contentId + decodeURIComponent("%26") + 'position=' + $drop.parents('.moduleContainer').attr('id')
    //alert(ajaxurl);  
    $.ajax({
        url: ajaxurl,
        success: function () {
            // alert('dropped on ' + $drop.attr('id'));
            $drag.insertAfter($drop);
            $drag.attr('style', 'position: relative;');
        }
    });
};

function shinkModuleContents($drag) {
    $drag.children(':not(.editable)').slideUp('slow');
    $drag.css("width", 200);
    $drag.css("float", "right");
    $drag.addClass("dragging");
};

function growModuleContents($drag) {
    $drag.children(':not(.editable)').slideDown('slow');
    $drag.css("width", "auto");
    $drag.css("float", "none");
    $drag.removeClass("dragging");
};

function modalOpen(dialog) {
    dialog.overlay.fadeIn('slow', function () {
        dialog.container.fadeIn('slow', function () {
            dialog.data.slideDown('slow');
        });
    });
}
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

    // $('.ewXform label').each(function () {
    //     if ($(this).parent().is('span.radiocheckbox')) {

    //      }
    //     else {
    // think of a better method, this screws design when there is HTML in a label, 
    //typically: 
    //      <p>TEXT</p>
    //      :
    //$(this).append(':')
    //     }
    //  });

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
        previewDiv = $('#previewImage_' + targetField);
        // previewDiv.innerHTML = '<a href="#" onclick="OpenWindow_edit_' + targetField + '()" title="edit an image from the image library" class="btn btn-sm btn-primary"><i class="icon-edit icon-white"> </i> Edit image</a><br/><a href="#" onclick="xfrmClearImage(\'EditContent\',\'' + targetField + '\',\'' + cName + '\');return false" title="Clear Image" class="btn btn-sm btn-danger"><i class="icon-remove-circle icon-white"> </i> Clear image</a>';
        $(editDiv).find('.editpick').html('<a title="edit an image from the image library" class="btn btn-primary editImage"><i class="fa fa-edit fa-white"> </i> Edit</a>');
        $(editDiv).find('a.editImage').click(function () {
            $(this).attr("href", '?contentType=popup&ewCmd=ImageLib&amp;targetForm=' + targetForm + '&amp;ewCmd2=editImage&amp;imgHtml=' + imgtag + '&amp;targetField=' + targetField + '&amp;targetClass=' + cName);
            return true;
        });
        //add preview Image
        $('<div class="previewImage" id="previewImage_' + targetField + '"><span>' + cImgHtml + '</span></div>').insertAfter(editDiv);
        previewDiv.remove();
    }
    $(".pickImageModal").modal("hide").removeData();
    // $(".pickImageModal").html("");
    setEditImage();
}

function passDocToForm(targetForm, targetField, cUrl) {
    $('#' + targetField).val(cUrl);
    buttonDiv = $('#editDoc_' + targetField + '  .input-group-btn');
    buttonDiv.html("<a href=\"#\" onclick=\"xfrmClearDocument('" + targetForm + "','" + targetField + "');return false\" title=\"Remove current Document reference\" class=\"btn btn-danger\"><i class=\"fa fa-trash-o fa-white\"> </i> Clear</a>")
    $(".pickImageModal").modal("hide").removeData();
    $(".pickImageModal").html("");
}

function passMediaToForm(targetForm, targetField, cUrl) {
    $('#' + targetField).val(cUrl);
    buttonDiv = $('#editDoc_' + targetField + '  .input-group-btn');
    buttonDiv.html("<a href=\"#\" onclick=\"xfrmClearMedia('" + targetForm + "','" + targetField + "');return false\" title=\"Remove current Image reference\" class=\"btn btn-danger\"><i class=\"fa fa-trash-o fa-white\"> </i> Clear</a>")
    $(".pickImageModal").modal("hide").removeData();
    $(".pickImageModal").html("");
}

function passImgFileToForm(targetForm, targetField, cUrl) {
    $('#' + targetField).val(cUrl);
    buttonDiv = $('#editImageFile_' + targetField + '  .input-group-btn');
    buttonDiv.html("<a href=\"#\" onclick=\"xfrmClearImgFile('" + targetForm + "','" + targetField + "');return false\" title=\"Remove current File reference\" class=\"btn btn-danger\"><i class=\"fa fa-trash-o fa-white\"> </i> Clear</a>")
    $(".pickImageModal").modal("hide").removeData();
    $(".pickImageModal").html("");
}

function xfrmClearImage(formRef, fieldRef, className) {
    document.forms[formRef].elements[fieldRef].value = '<img class="' + className + '"/>';
    previewDiv = $('#previewImage_' + fieldRef);
    editDiv = $('#editImage_' + fieldRef);
    previewDiv.remove();
    editDiv.find('a.btn-danger').remove();
    editDiv.find('a.editImage').remove();
    editDiv.find('span.editpick').html('<a data-toggle="modal" href="?contentType=popup&ewCmd=ImageLib&amp;targetForm=' + formRef + '&amp;targetField=' + fieldRef + '&amp;targetClass=' + className + '" title="pick an image from the image library" data-target="#modal-' + fieldRef + '" class="btn btn-primary"><i class="fa fa-picture-o fa-white"> </i> Pick</a>');
    //	alert(previewDiv.innerHTML);
}

function xfrmClearDocument(formRef, fieldRef) {
    document.forms[formRef].elements[fieldRef].value = '';
    buttonDiv = $('#editDoc_' + fieldRef + '  .input-group-btn');
    buttonDiv.html('<a data-toggle="modal" href="?contentType=popup&ewCmd=DocsLib&amp;targetForm=' + formRef + '&amp;targetField=' + fieldRef + '" title="pick an document from the image library" data-target="#modal-' + fieldRef + '" class="btn btn-primary"><i class="fa fa-picture-o fa-white"> </i> Pick</a>')
}

function xfrmClearMedia(formRef, fieldRef) {
    document.forms[formRef].elements[fieldRef].value = '';
    buttonDiv = $('#editDoc_' + fieldRef + '  .input-group-btn');
    buttonDiv.html('<a data-toggle="modal" href="?contentType=popup&ewCmd=MediaLib&amp;targetForm=' + formRef + '&amp;targetField=' + fieldRef + '" title="pick an document from the image library" data-target="#modal-' + fieldRef + '" class="btn btn-primary"><i class="fa fa-music fa-white"> </i> Pick</a>')
}

function xfrmClearImgFile(formRef, fieldRef) {
    document.forms[formRef].elements[fieldRef].value = '';
    buttonDiv = $('#editImageFile_' + fieldRef + '  .input-group-btn');
    buttonDiv.html('<a data-toggle="modal" href="?contentType=popup&ewCmd=ImageLib&amp;ewCmd2=PathOnly&amp;targetForm=' + formRef + '&amp;targetField=' + fieldRef + '" title="pick an document from the image library" data-target="#modal-' + fieldRef + '" class="btn btn-primary"><i class="fa fa-picture-o fa-white"> </i> Pick</a>')
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
    previewDiv.innerHTML = '<a href="#" onclick="OpenWindow_edit_' + fieldRef + '();return false" title="edit an image from the image library" class="btn btn-sm btn-primary"><i class="fa-picture-o fa-white"> </i> Edit</a>' + imgtag;
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

/*
* AjaxTreeview 1.0.8
* ------------------
* A treeview that uses Ajax to load branches in.
*
* Web.config nodes that can be used
* ---------------------------------
* <add key="menuTreeDepth" value="3"/> - Numerical value to open tree to (both for XSL and JS versions)
* <add key="menuNoReload" value="true"/> - True (no Reload), False (Reload)
*
* Updates
* -------
* 1.0.8 - Added functionality to not need reloads, also pre-open layers is now handled in the XSL.
Original preload function has been kept but is unused.
* 1.0.7 - Added Show and Hide functions, recursively gained previous additions
* 1.0.6 - Added button locking for the Move Buttons to stop stupid spam
* 1.0.5 - Added random a= extension to Move Commands to bypass IE's caching, anims on success also added
* 1.0.4 - Level Paramater Added, to handle preloading to a certain level
* 1.0.3 - Added code from Jquery home page to support Url parameters,
*          and coded support for "move" paramater
* 1.0.2 - Settings param now supported
* 1.0.1 - Reworked .js to be standalone, initTree renamed buildTree
*
* Author 2008 Nathan Brown
*
* Latest Update : 02-Dec-2008
*
*/

; (function ($) {

    $.extend($.fn, {

        // Constructor
        ajaxtreeview: function (settings) {
            if ($(this).length > 0) {
                $(this).addClass("treeview");
                // Check if levels have been defined, if so, pre-open			
                if (settings.level > 0) {
                    $(this).expandToLevel(settings);
                }



                // Add the control classes to the tree
                $(this).find('li').each(function () {
                    // alert($(this).attr('id') + '==' + 'node' + $(this).next().data("tree-parent"))
                    if ($(this).attr('id') == 'node' + $(this).next().data("tree-parent")) {
                        $(this).removeClass('expandable').addClass('collapsable');
                    }
                    else {
                        $(this).removeClass('collapsable').addClass('expandable');
                    }
                });

                // $(this).find('li:has(ul):has(".activeParent,.inactiveParent")').addClass('collapsable');
                // $(this).find('li:not(:has(ul)):has(".activeParent,.inactiveParent")').addClass('expandable');

                // Call buildtree
                // New (Hide) Version (v1.0.8)
                if (settings.hide) {
                    $(this).buildTree_noreload(settings);
                }
                // Old (Empty) Version (v1.0.0 -> v1.0.7)
                else {
                    $(this).buildTree(settings);

                }

                if (settings.openLevel > 0) {
                    $(this).startLevel(settings);
                    $(this).buildTree_noreload(settings);
                }
            }
        },

        startLevel: function (settings) {

            $("#MenuTree li[data-tree-level='" + settings.openLevel + "']").each(function () {
                //unless you have an active descendant
                if ($(this).activeChild() == false) {
                    $(this).hideChildren();
                }
            });
        },

        activeChild: function () {
            var isActive = false;
            var nodeId = $(this).attr('id').replace(/node/, "");
            $("#MenuTree li[data-tree-parent='" + nodeId + "']").each(function () {
                if ($(this).hasClass('active') || $(this).activeChild()) {
                    isActive = true;
                }
            });
            return isActive;
        },

        // Method for if the level param is in play
        expandToLevel: function (settings) {

            $('#MenuTree li.levelExpandable:has(".activeParent,.inactiveParent")').each(function () {
                $(this).removeClass('levelExpandable');
                $(this).removeClass('expandable');
                var ewPageId = $(this).attr('id').replace(/node/, "");
                $(this).insertAfter('<div class="loadnode">Loading <i class="fa fa-cog fa-spin fa-fw"> </i></div>')
                    .load(settings.loadPath, { ajaxCmd: settings.ajaxCmd, pgid: ewPageId }, function () {
                        $(this).children().find('li:has(".activeParent,.inactiveParent")').addClass('expandable');
                        $("#MenuTree").buildTree(settings);
                        settings.level = settings.level - 1;
                        if (settings.level > 0) {
                            $(this).children().find('li:has(".activeParent,.inactiveParent")').addClass('levelExpandable');
                            $("#MenuTree").expandToLevel(settings);
                        }
                    });
            });
        },


        // This function handles the tree's classes and mouse bindings for the "hit area"'s
        buildTree: function (settings) {
            // Add Hit area's (the clickable part)
            $('#MenuTree li.collapsable:not(:has(i.hitarea)):has(".activeParent,.inactiveParent")').prepend('<i class="hitarea collapsable-hitarea fa fa-chevron-down"> </i>');
            $('#MenuTree li.expandable:not(:has(i.hitarea)):has(".activeParent,.inactiveParent")').prepend('<i class="hitarea expandable-hitarea fa fa-chevron-right"> </i>');
            // Sort out assignments of the last tag
            $('#MenuTree').applyLast();
            // Remove any mouse bindings currently on the hitarea's
            $('#MenuTree li div.hitarea').unbind("click");

            //Mouse binding for open nodes
            $('#MenuTree li.collapsable').find('.hitarea').unbind("click").click(function () {
                // Remove old class assingments

                $(this).removeClass('collapsable-hitarea').addClass('expandable-hitarea');
                $(this).removeClass('fa-chevron-down').addClass('fa-chevron-right');

                alert('empty');

                // Remove the child tree

                $(this).parent().find('ul').empty();

                // Reset Class Status
                $(this).parent().removeClass('collapsable').addClass('expandable');
                // Set kids to be closed again
                $(this).children().find('li:has(".activeParent,.inactiveParent")').addClass('expandable');
                // Calling a rebuild assings the correct functionality
                $("#MenuTree").buildTree(settings)
                //settings doesn't work in the next line? so used above
                //setTimeout('$("#MenuTree").buildTree()',1);
            });


            //Mouse binding for closed nodes
            $('#MenuTree li.expandable').find('.hitarea').unbind("click").click(function () {
                // Unbind after first click to prevent stupid users multiclicking
                $('#MenuTree li i.hitarea').unbind("click");
                // Remove current classes from the hit-area
                $(this).removeClass('expandable-hitarea').addClass('collapsable-hitarea');
                $(this).removeClass('fa-chevron-right').addClass('fa-chevron-down');
                // Get the node's ID (used in load)
                var ewPageId = (this.parentNode.getAttribute('id').replace(/node/, ""));
                // Sort the parent node's class
                $(this).parent().removeClass('expandable').addClass('collapsable');
                // Append the loading line
                //  $(this).parent().find('ul').append('<div class="loadnode">Loading <i class="fa fa-cog fa-spin fa-2x fa-fw"> </i></div>')
                var ewCloneContextId = 0;

                // Test for context (cloned pages)
                if ($(this).parent().hasClass('clone')) {
                    var re = /.*context(\d+).*/g;

                    if ((matches = re.exec(this.parentNode.className)) != null) {
                        ewCloneContextId = matches[1];
                    }

                }

                // If move has been flagged then use a different load
                if (settings.move) {
                    var originalPageId = $("#MenuTree").urlParam('pgid');
                    $(this).parent().find('ul').load(settings.loadPath, { ajaxCmd: settings.ajaxCmd, pgid: originalPageId, expId: ewPageId, context: ewCloneContextId }, function () {
                        // Find out which of the kids have kids
                        $(this).children().find('li').has('i.activeParent').addClass('expandable');
                        // Rebuild the tree
                        $("#MenuTree").buildTree(settings)

                    });
                }
                // Else for everything use, use the regular loading sequence
                else {


                    $(this).parent().find('ul').load(settings.loadPath, { ajaxCmd: settings.ajaxCmd, pgid: ewPageId, context: ewCloneContextId }, function () {
                        // Find out which of the kids have kids
                        //alert('test');
                        $(this).children().find('li').has('i.activeParent,i.inactiveParent').addClass('expandable');
                        // Rebuild the tree
                        $("#MenuTree").buildTree(settings)

                    });
                }

            });

        },



        // Same as above, prototype buildTree for no reloads      
        buildTree_noreload: function (settings) {
            // Add Hit area's (the clickable part)
            $('#MenuTree li.collapsable:not(:has(.hitarea)):has(".activeParent,.inactiveParent")').prepend('<i class="hitarea collapsable-hitarea fa fa-chevron-down"> </i>');
            $('#MenuTree li.expandable:not(:has(.hitarea)):has(".activeParent,.inactiveParent")').prepend('<i class="hitarea expandable-hitarea fa fa-chevron-right"> </i>');
            // Sort out assignments of the last tag
            $('#MenuTree').applyLast();
            // Remove any mouse bindings currently on the hitarea's
            $('#MenuTree li div.hitarea').unbind("click");

            // alert('treeload');

            //Mouse binding for open nodes
            $('#MenuTree').find('.collapsable-hitarea').unbind("click").click(function () {


                $(this).parent().hideChildren();

                // Reset Class Status

                // Set kids to be closed again
                ///////////////////////////// !!! what happens here? assume that they aren't open?
                //$(this).children().find('li:has(".activeParent,.inactiveParent")').addClass('expandable');			
                // Calling a rebuild assings the correct functionality
                $("#MenuTree").buildTree_noreload(settings)
            });


            //Mouse binding for closed nodes (First Time)
            $('#MenuTree li.expandable').find('.hitarea').unbind("click").click(function () {
                // Unbind after first click to prevent stupid users multiclicking
                $('#MenuTree li i.hitarea').unbind("click");
                // Remove current classes from the hit-area
                $(this).removeClass('expandable-hitarea').addClass('collapsable-hitarea');
                $(this).removeClass('fa-chevron-right').addClass('fa-chevron-down');
                // Sort the parent node's class
                $(this).parent().removeClass('expandable').addClass('collapsable');

                // Get the node's ID (used in load)
                var ewPageId = (this.parentNode.getAttribute('id').replace(/node/, ""));

                // Append the loading line
                var parentNode = $(this).parent()
                $('<li id="loading-node" class="list-group-item"><div class="loadnode">Loading <i class="fa fa-cog fa-spin fa-fw"></i></div></li>').insertAfter(parentNode);

                // If move has been flagged then use a different load
                var ewCloneContextId = 0;

                // Test for context (cloned pages)
                if ($(this).parent().hasClass('clone')) {
                    var re = /.*context(\d+).*/g;

                    if ((matches = re.exec(this.parentNode.className)) != null) {
                        ewCloneContextId = matches[1];
                    }

                }

                if (settings.move) {
                    var originalPageId = $("#MenuTree").urlParam('pgid');
                    $(this).parent().next().load(settings.loadPath, { ajaxCmd: settings.ajaxCmd, pgid: originalPageId, expId: ewPageId, context: ewCloneContextId }, function () {
                        // Find out which of the kids have kids
                        $(this).children().find('li:has(".activeParent,.inactiveParent")').addClass('expandable');
                        // Rebuild the tree
                        $("#MenuTree").buildTree_noreload(settings)
                    });
                }
                else {

                    var loadNode = $(this).parent().next()
                    loadNode.load(settings.loadPath, { ajaxCmd: settings.ajaxCmd, pgid: ewPageId, context: ewCloneContextId }, function (data) {

                        var $results = $(loadNode).find('ul .list-group-item');
                        if ($results.length == 0) {
                            alert($(loadNode).html());
                        }
                        else {
                            $(loadNode).find("ul .list-group-item").insertAfter(parentNode)
                        }
                        loadNode.remove()

                        // Find out which of the kids have kids
                        loadNode.children().find('li').has('.activeParent,.inactiveParent').addClass('expandable');
                        // Rebuild the tree
                        $("#MenuTree").buildTree_noreload(settings)
                    });


                }

            });

            //Mouse binding for closed nodes (No Reload)
            $('#MenuTree li.expandable_loaded').find('.hitarea').unbind("click").click(function () {
                $('#MenuTree li i.hitarea').unbind("click");
                $(this).removeClass('expandable-hitarea').addClass('collapsable-hitarea');
                $(this).removeClass('fa-chevron-right').addClass('fa-chevron-down');
                // Get the node's ID (used in load) - ??? Needed anymore?
                //var ewPageId = (this.parentNode.getAttribute('id').replace(/node/,""));
                $(this).parent().removeClass('expandable_loaded').addClass('collapsable');

                var ewPageId = (this.parentNode.getAttribute('id').replace(/node/, ""));
                $(this).parent().parent().find('li[data-tree-parent="' + ewPageId + '"]').show();

                $("#MenuTree").buildTree_noreload(settings)
            });
        },

        hideChildren: function () {
            $(this).find('.hitarea').removeClass('collapsable-hitarea').addClass('expandable-hitarea');
            $(this).find('.hitarea').removeClass('fa-chevron-down').addClass('fa-chevron-right');
            // alert($(this).html)
            var ewPageId = ($(this).attr('id').replace(/node/, ""));
            // alert(ewPageId);
            $(this).parent().find('li[data-tree-parent="' + ewPageId + '"]').each(function (index) {
                $(this).find('.hitarea').removeClass('collapsable-hitarea').addClass('expandable-hitarea');
                $(this).find('.hitarea').removeClass('fa-chevron-down').addClass('fa-chevron-right');
                $(this).removeClass('collapsable').addClass('expandable');
                $(this).hideChildren();
                $(this).hide();
            });

            $(this).removeClass('collapsable').removeClass('expandable').addClass('expandable_loaded');

        },

        checkChildren: function () {
            $('#MenuTree li').each(function () {
                var thisParentId = $(this).data('tree-parent')

                if ($(this).prevAll('li[data-tree-parent="' + thisParentId + '"]').length == 0) {
                    //if first amoung siblings
                    if ($(this).prev('li[id="node' + thisParentId + '"]').length == 0) {
                        //if incorrect parent
                        if ($('#MenuTree li[id="node' + thisParentId + '"] li[data-tree-parent="' + thisParentId + '"]').length == 0) {
                            //no siblings moved allready
                            // $('#MenuTree li[id="node' + thisParentId + '"]').after($(this))
                            $(this).nextAll('li[data-tree-parent="' + thisParentId + '"]').reverse().each(function () {
                                $('#MenuTree li[id="node' + thisParentId + '"]').after($(this))
                            });
                            $('#MenuTree li[id="node' + thisParentId + '"]').after($(this))
                        }
                    }
                }
            })
        },


        //This function sorts the flagging of "last" nodes (used for the gfx)
        applyLast: function () {
            //Hide and show Up Down Buttons
            $('#MenuTree li').each(function () {

                var thisParentId = $(this).data('tree-parent')
                //if this not has a previous sibling with the same data-tree-parent then hide the up arrows
                if ($(this).prevAll('li[data-tree-parent="' + thisParentId + '"]').length == 0) {
                    $(this).find('a.move-up').addClass("disabled")
                    $(this).find('a.move-top').addClass("disabled")
                }
                else {
                    $(this).find('a.move-up').removeClass("disabled")
                    $(this).find('a.move-top').removeClass("disabled")
                }
                if ($(this).nextAll('li[data-tree-parent="' + thisParentId + '"]').length == 0) {
                    $(this).find('a.move-down').addClass("disabled")
                    $(this).find('a.move-bottom').addClass("disabled")
                }
                else {
                    $(this).find('a.move-down').removeClass("disabled")
                    $(this).find('a.move-bottom').removeClass("disabled")
                }
                //if this not has a following sibling with the same data-tree-parent then hide the down arrows
            })
        },

        // The next set of functions handle the movement buttons (mainly animation)
        // Take in the node's id as input			
        moveUp: function (moveId) {
            var moveIdNode = "node" + moveId;

            if (!($('#MenuTree li#' + moveIdNode).hasClass("locked"))) {
                $('#MenuTree li#' + moveIdNode).addClass("locked");
                var thisParentId = $('#MenuTree li#' + moveIdNode).data('tree-parent')
                //Construct the node name
                $('#MenuTree li#' + moveIdNode).fadeTo("fast", 0.25);
                //IE is stupid, so append random numbers to the end
                var i = Math.round(10000 * Math.random());
                // Pass out the command to move the node
                $.ajax({
                    url: '?ewCmd=MoveUp' + decodeURIComponent("%26") + 'pgid=' + moveId + '&a=' + i,
                    success: function () {
                        //Animate
                        $('#MenuTree li#' + moveIdNode).prevAll('li[data-tree-parent="' + thisParentId + '"]').first().hide().fadeIn("fast");
                        $('#MenuTree li#' + moveIdNode).prevAll('li[data-tree-parent="' + thisParentId + '"]').first().before($('#MenuTree li#' + moveIdNode));
                        $('#MenuTree').applyLast();
                        $('#MenuTree').checkChildren();
                        $('#MenuTree li#' + moveIdNode).fadeTo("fast", 1.0);
                        $('#MenuTree li#' + moveIdNode).removeClass("locked");
                        // alert('move up');

                    }
                });


            }

        },

        moveDown: function (moveId) {
            var moveIdNode = "node" + moveId;

            if (!($('#MenuTree li#' + moveIdNode).hasClass("locked"))) {
                $('#MenuTree li#' + moveIdNode).addClass("locked");

                var thisParentId = $('#MenuTree li#' + moveIdNode).data('tree-parent')

                $('#MenuTree li#' + moveIdNode).fadeTo("fast", 0.25);
                var i = Math.round(10000 * Math.random());
                $.ajax({
                    url: '?ewCmd=MoveDown' + decodeURIComponent("%26") + 'pgid=' + moveId + '&a=' + i,
                    success: function () {
                        $('#MenuTree li#' + moveIdNode).prevAll('li[data-tree-parent="' + thisParentId + '"]').first().hide().fadeIn("fast");
                        $('#MenuTree li#' + moveIdNode).nextAll('li[data-tree-parent="' + thisParentId + '"]').first().after($('#MenuTree li#' + moveIdNode));
                        $('#MenuTree').applyLast();
                        $('#MenuTree').checkChildren();
                        $('#MenuTree li#' + moveIdNode).fadeTo("fast", 1.0);
                        $('#MenuTree li#' + moveIdNode).removeClass("locked");
                        // alert('move down');

                    }
                });
            }

        },

        moveTop: function (moveId) {
            var moveIdNode = "node" + moveId;

            if (!($('#MenuTree li#' + moveIdNode).hasClass("locked"))) {
                $('#MenuTree li#' + moveIdNode).addClass("locked");
                $('#MenuTree li#' + moveIdNode).fadeTo("fast", 0.25);
                var thisParentId = $('#MenuTree li#' + moveIdNode).data('tree-parent')
                var i = Math.round(10000 * Math.random());
                $.ajax({
                    url: '?ewCmd=MoveTop' + decodeURIComponent("%26") + 'pgid=' + moveId + '&a=' + i,
                    success: function () {
                        $('#MenuTree li#' + moveIdNode).prev('li[data-tree-parent="' + thisParentId + '"]').hide().fadeIn("fast");
                        $('#MenuTree li#' + moveIdNode).prevAll('li[data-tree-parent="' + thisParentId + '"]:last').before($('#MenuTree li#' + moveIdNode));
                        $('#MenuTree').applyLast();
                        $('#MenuTree').checkChildren();
                        $('#MenuTree li#' + moveIdNode).fadeTo("fast", 1.0);
                        $('#MenuTree li#' + moveIdNode).removeClass("locked");
                    }
                });
            }
        },

        moveBottom: function (moveId) {
            var moveIdNode = "node" + moveId;

            if (!($('#MenuTree li#' + moveIdNode).hasClass("locked"))) {
                $('#MenuTree li#' + moveIdNode).addClass("locked");
                $('#MenuTree li#' + moveIdNode).fadeTo("fast", 0.25);
                var thisParentId = $('#MenuTree li#' + moveIdNode).data('tree-parent')
                var i = Math.round(10000 * Math.random());
                $.ajax({
                    url: '?ewCmd=MoveBottom' + decodeURIComponent("%26") + 'pgid=' + moveId + '&a=' + i,
                    success: function () {
                        $('#MenuTree li#' + moveIdNode).next().hide('li[data-tree-parent="' + thisParentId + '"]').fadeIn("fast");
                        $('#MenuTree li#' + moveIdNode).nextAll('li[data-tree-parent="' + thisParentId + '"]:last').after($('#MenuTree li#' + moveIdNode));
                        $('#MenuTree').applyLast();
                        $('#MenuTree').checkChildren();
                        $('#MenuTree li#' + moveIdNode).fadeTo("fast", 1.0);
                        $('#MenuTree li#' + moveIdNode).removeClass("locked");
                    }
                });
            }
        },

        hideButton: function (hideId) {

            var hideIdNode = "node" + hideId;
            if (!($('#MenuTree li#' + hideIdNode).hasClass("locked"))) {
                $('#MenuTree li#' + hideIdNode).addClass("locked");

                $('#MenuTree li#' + hideIdNode).fadeTo("fast", 0.25);
                var i = Math.round(10000 * Math.random());
                $.ajax({
                    url: '?ewCmd=HidePage' + decodeURIComponent("%26") + 'pgid=' + hideId + '&a=' + i,
                    success: function () {

                        $('#MenuTree li#' + hideIdNode + ' a.btn-hide').remove();
                        $('#MenuTree li#' + hideIdNode + ' a.btn-show').remove();
                        $('#MenuTree li#' + hideIdNode + ' div.optionButtons:first').append(' <a onclick="$(\'#MenuTree\').showButton(' + hideId + ');" class="btn btn-xs btn-success btn-show" title="Click here to show this page"><i class="fa fa-check-circle fa-white"> </i> Show</a>');
                        $('#MenuTree li#' + hideIdNode + ' div.optionButtons:first').append(' <a href="?ewCmd=DeletePage&amp;pgid=' + hideId + '" class="btn btn-xs btn-danger btn-del" title="Click here to delete this page"><i class="fa fa-trash-o fa-white"> </i> Delete</a>');

                        if ($('#MenuTree li#' + hideIdNode + ' i.status').hasClass('active')) {
                            $('#MenuTree li#' + hideIdNode + ' i.status').removeClass('active')
                            $('#MenuTree li#' + hideIdNode + ' i.status').addClass('inactive')
                            $('#MenuTree li#' + hideIdNode + ' i.status').addClass('text-muted')

                        }
                        else if ($('#MenuTree li#' + hideIdNode + ' i.status').hasClass('activeParent')) {
                            $('#MenuTree li#' + hideIdNode + ' i.status').removeClass('activeParent')
                            $('#MenuTree li#' + hideIdNode + ' i.status').addClass('inactiveParent')
                            $('#MenuTree li#' + hideIdNode + ' i.status').addClass('text-muted')
                        }

                        $('#MenuTree li#' + hideIdNode).fadeTo("fast", 1.0);
                        $('#MenuTree li#' + hideIdNode).removeClass("locked");
                        $('#MenuTree').applyLast();
                    }
                });
            }
        },

        showButton: function (showId) {
            var showIdNode = "node" + showId;
            if (!($('#MenuTree li#' + showIdNode).hasClass("locked"))) {
                $('#MenuTree li#' + showIdNode).addClass("locked");

                $('#MenuTree li#' + showIdNode).fadeTo("fast", 0.25);
                var i = Math.round(10000 * Math.random());
                $.ajax({
                    url: '?ewCmd=ShowPage' + decodeURIComponent("%26") + 'pgid=' + showId + '&a=' + i,
                    success: function () {
                        //Sort out removal of button and then addition of others
                        $('#MenuTree li#' + showIdNode + ' a.btn-del:first').remove();
                        $('#MenuTree li#' + showIdNode + ' a.btn-show').remove();
                        $('#MenuTree li#' + showIdNode + ' div.optionButtons:first').append('<a onclick="$(\'#MenuTree\').hideButton(' + showId + ');" class="btn btn-xs btn-danger btn-hide" title="Click here to hide this page"><i class="fa fa-times-circle fa-white"> </i> Hide</a>');

                        if ($('#MenuTree li#' + showIdNode + ' i.status').hasClass('inactive')) {
                            $('#MenuTree li#' + showIdNode + ' i.status').removeClass('inactive')
                            $('#MenuTree li#' + showIdNode + ' i.status').addClass('active')
                            $('#MenuTree li#' + showIdNode + ' i.status').removeClass('text-muted')
                        }
                        else if ($('#MenuTree li#' + showIdNode + ' i.status').hasClass('inactiveParent')) {
                            $('#MenuTree li#' + showIdNode + ' i.status').removeClass('inactiveParent')
                            $('#MenuTree li#' + showIdNode + ' i.status').addClass('activeParent')
                            $('#MenuTree li#' + showIdNode + ' i.status').removeClass('text-muted')
                        }

                        $('#MenuTree li#' + showIdNode).fadeTo("fast", 1.0);
                        $('#MenuTree li#' + showIdNode).removeClass("locked");
                        $('#MenuTree').applyLast();
                    }
                });
            }
        },


        // Function taken from Jquery home page (aka not written by me!)
        // Used for taking a param out of the url
        urlParam: function (param) {
            var regex = '[?&]' + param + '=([^&#]*)';
            var results = (new RegExp(regex)).exec(window.location.href);
            if (results) return results[1];
            return '';
        },

        // Function for admin popup menus -  Trevor Spink
        adminPopup: function (settings) {
            $(this).parent().mouseover(function () {
                //var divPos = $(this).children('div.ewPopMenu').offset();
                $(this).parents('td.optionsButton').css('zIndex', 10000)
                $(this).children('a.popup').addClass('popupOpen')
                $(this).children('a.popup').removeClass('popup')
                $(this).children('div.ewPopMenu').show();
                // $(this).children('div.ewPopMenu').dialog({ modal: true });
            }
            );
            $(this).parent().mouseout(function () {
                $(this).parents('td.optionsButton').css('zIndex', '')
                $(this).children('a.popupOpen').addClass('popup')
                $(this).children('a.popupOpen').removeClass('popupOpen')
                $(this).children('div.ewPopMenu').hide();
            }
            );
        }

    });

})(jQuery);



/*  When moving pages we need to pass through original page being moved 
to avoid looping MenuItems - WH 2010-07-30 
*/
function getAdminAjaxTreeViewPath() {
    var treeviewPath = '?contentType=ajaxadmin'
    if ($("#tpltMovePage").exists()) {
        var movingPageId = getParameterByName('pgid');
        treeviewPath = treeviewPath + '&movingPageId=' + movingPageId;
    }
    if ($("#tpltMoveContent").exists()) {
        var movingContentId = getParameterByName('id');
        var currentPageId = getParameterByName('parId');
        var startingPageId = getParameterByName('pgid');
        treeviewPath = treeviewPath + '&oldPgId=' + startingPageId + '&id=' + movingContentId;
        //alert(treeviewPath);
    }
    return treeviewPath;
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
    //alert('hi');
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
        alert(addressString);
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

                alert(status + 'Couldn\'t find the latitude and longitude for the address provided. Try including more details.');

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

        alert(addressString); 

        var geocoder = new google.maps.Geocoder();

        geocoder.geocode({ address: addressString }, function (results, status) {
            if (status == google.maps.GeocoderStatus.OK) {

                // Set lat and long values in relevant inputs
                var location = results[0].geometry.location;
                $('#cLocationLat').val(location.lat());
                $('#cLocationLong').val(location.lng());

            } else {

                alert(status + '111 Couldn\'t find the latitude and longitude for the address provided. Try including more details.');

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

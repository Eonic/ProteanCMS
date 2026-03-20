$(document).ready(function () {
    $(".ptn-edit .dropdown .dropdown-toggle").click(function () {
        //$(this).parents(".ptn-edit").toggle(function () {
        //    $(this).addClass("active-admin-dd");
        //}, function () {
        //    $(this).removeClass("active-admin-dd");
        //});
        // $(this).parents(".ptn-edit").addClass("active-admin-dd");
        $(this).parents(".ptn-edit").toggleClass("active-admin-dd");
        $(this).parents(".module-containing-icon").toggleClass("active-admin-dd-wrapper");
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
});


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
    var pageId = $('body').attr('id').replace('pg', '')
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
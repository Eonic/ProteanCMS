$(document).ready(function () {


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
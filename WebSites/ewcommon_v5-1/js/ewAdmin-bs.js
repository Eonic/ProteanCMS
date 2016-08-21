
var checkiFrameLoaded;

$(document).ready(function () {

   initialiseGeocoderButton();

    $('form.ewXform').prepareAdminXform();

    //$(document).prepareEditable();

    // PICK IMAGE - run through all image containers, making same height as row.
    if ($(".pickByImage").exists()) {
        matchHeightCol($(this).find(".item"), '2');
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

        $.modal('<div class="ewModal"><span><img src="/ewcommon/images/admin/ajax-loader2.gif"/>Please Wait...<br/>This may take some time</span></div>', {
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


    var treeviewPath = getAdminAjaxTreeViewPath();

    $('#tpltAdvancedMode #MenuTree').ajaxtreeview({
        loadPath: treeviewPath,
        ajaxCmd: 'GetAdvNode'
    });

    $('#tpltEditStructure #MenuTree').ajaxtreeview({
        loadPath: treeviewPath,
        ajaxCmd: 'GetStructureNode',
        hide: true
    });

    $('#tpltMovePage #MenuTree').ajaxtreeview({
        loadPath: treeviewPath,
        ajaxCmd: 'GetMoveNode',
        hide: false
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
        ajaxCmd: 'GetLocateNode',
        hide: false
    });

    $('#tpltDeliveryMethodLocations #MenuTree').ajaxtreeview({
        loadPath: treeviewPath,
        ajaxCmd: 'GetLocateNode',
        hide: true
    });

    $('#template_FileSystem #MenuTree').ajaxtreeview({
        loadPath: treeviewPath,
        ajaxCmd: 'GetLocateNode',
        hide: true
    });

    $('#template_EditPermissions #MenuTree').ajaxtreeview({
        loadPath: treeviewPath,
        ajaxCmd: 'editStructurePermissions',
        hide: true
    });

    $('#template_permissions #MenuTree').ajaxtreeview({
        loadPath: treeviewPath,
        ajaxCmd: 'editStructurePermissions',
        hide: true
    });

    $('div.module div.moduleDrag').draggable({
        cursor: 'move',
        containment: '#mainTable',
        handle: 'a.drag',
        revert: 'invalid',
        zindex: '10000',
        start: function (ev, ui) {
            shinkModuleContents($(this).parents('.module'));
        },
        stop: function (ev, ui) {
            growModuleContents($(this).parents('.module'));
        }
    });

    $('.moduleContainer').droppable({
        accept: 'div.moduleDrag',
        activeClass: 'droppable-active',
        hoverClass: 'droppable-hover',
        drop: function (ev, ui) {
            acceptModule(ui.draggable.parents('.module'), $(this));
            //reset dragg
            ui.draggable.attr('style', 'z-index: ' + ui.draggable.css('z-index') + ';')
        }
    });

    $(function () {
        var zIndexNumber = 9000;
        $('.editable,div.options,div.ewPopMenu').each(function () {
            $(this).css('zIndex', zIndexNumber);
            zIndexNumber -= 1;
        });
    });

    $("a.popup").adminPopup();

    // initialiseHelptips();



    $("[data-toggle=popover]").popover({
        html: true,
        content: function () {
            return $(this).prev().html();
        }
    });

});

function initialiseHelptips() {

        $(".helpTip").tooltip({
            track: true,
            relative:true,
            hide: { duration: 1000000 } ,
            content: function () {
                var element = $(this);
                return element.attr("title");
            }
        });

};
   

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
    var ajaxurl = '?ewCmd=UpdatePosition' + decodeURIComponent("%26") + 'pgid=' + pageId + decodeURIComponent("%26") + 'id=' + contentId + decodeURIComponent("%26") + 'position=' + $drop.attr('id')
    //alert(ajaxurl);  
    $.ajax({
        url: ajaxurl,
        success: function () {
            // alert('dropped on ' + $drop.attr('id'));
            $drag.insertAfter($drop.find('.addHere'));
            $drag.attr('style', 'position: relative;');
        }
    });
};

function shinkModuleContents($drag) {

    $drag.children(':not(.editable)').slideUp('slow');


};

function growModuleContents($drag) {

    $drag.children(':not(.editable)').slideDown('slow');


};

function modalOpen(dialog) {
    dialog.overlay.fadeIn('slow', function () {
        dialog.container.fadeIn('slow', function () {
            dialog.data.slideDown('slow');
        });
    });
}



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
                var myvalue = $(this).value
                alert(myname);
                $(name = myname + '-family').val(myvalue.substr(0, myvalue.indexOf('|')))
                $(name = myname + '-import').val(myvalue.substr(myvalue.indexOf('|') + 1, myvalue.length))
            });
        });
    };

};

function passImgToForm(targetForm, targetFeild) {
    cUrl = document.forms['imageDetailsForm'].elements['cPathName'].value
    cAlt = document.forms['imageDetailsForm'].elements['cDesc'].value
    cWidth = document.forms['imageDetailsForm'].elements['nWidth'].value
    cHeight = document.forms['imageDetailsForm'].elements['nHeight'].value
    cName = document.forms['imageDetailsForm'].elements['cName'].value
    cImgHtml = '<img src="' + cUrl + '" width="' + cWidth + '" height="' + cHeight + '" alt="' + cAlt + '"'
    if (cName != '') {
        cImgHtml = cImgHtml + ' class="' + cName + '"'
    }
    cImgHtml = cImgHtml + '/>'
    opener.document.forms[targetForm].elements[targetFeild].value = cImgHtml;
    imgtag = cImgHtml;
    previewDiv = opener.document.getElementById('previewImage_' + targetFeild);
    previewDiv.innerHTML = '<span>' + imgtag + '</span><a href="#" onclick="OpenWindow_edit_' + targetFeild + '()" title="edit an image from the image library" class="btn btn-sm btn-primary"><i class="icon-edit icon-white"> </i> Edit image</a><br/><a href="#" onclick="xfrmClearImage(\'EditContent\',\'' + targetFeild + '\',\'' + cName + '\');return false" title="Clear Image" class="btn btn-sm btn-danger"><i class="icon-remove-circle icon-white"> </i> Clear image</a>';
    window.close();
}

function passDocToForm(targetForm, targetFeild, cUrl) {
    opener.document.forms[targetForm].elements[targetFeild].value = cUrl;
    previewDiv = opener.document.getElementById('previewImage_' + targetFeild);
    previewDiv.innerHTML = "<a href=\"#\" onclick=\"xfrmClearDocument('EditContent','" + targetFeild + "');return false\" title=\"Remove current Document reference\" class=\"adminButton delete\">Clear document</a>"
    window.close();
}

function passMediaToForm(targetForm, targetFeild, cUrl) {
    opener.document.forms[targetForm].elements[targetFeild].value = cUrl;
    previewDiv = opener.document.getElementById('previewImage_' + targetFeild);
    previewDiv.innerHTML = "<a href=\"#\" onclick=\"xfrmClearDocument('EditContent','" + targetFeild + "');return false\" title=\"Remove current Media reference\" class=\"adminButton delete\">Clear media</a>"
    window.close();
}
function xfrmClearImage(formRef, fieldRef, className) {
    document.forms[formRef].elements[fieldRef].value = '<img class="' + className + '"/>';
    previewDiv = document.getElementById('previewImage_' + fieldRef);
    previewDiv.innerHTML = '<a href="#" onclick="OpenWindow_pick_' + fieldRef + '();return false" title="pick an image from the image library" class="btn btn-sm btn-primary"><i class="icon-picture icon-white"> </i> Pick image</a> No image'
    //	alert(previewDiv.innerHTML);
}

function xfrmClearDocument(formRef, fieldRef) {
    document.forms[formRef].elements[fieldRef].value = '';
    previewDiv = document.getElementById('previewImage_' + fieldRef);
    previewDiv.innerHTML = '<a href="#" onclick="OpenWindow_pick_' + fieldRef + '();return false" title="pick an document from the document library" class="btn btn-sm btn-primary"><i class="icon-file icon-white"> </i> Pick document</a>'
}

function xfrmClearCalendar(formRef, fieldRef) {
    document.forms[formRef].elements[fieldRef].value = '';
    document.getElementById('dateDisplay-' + fieldRef).innerHTML = '';
}

function passFilePathToForm(targetFeild, filepath) {
    opener.document.forms['EditContent'].elements[targetFeild].value = filepath;
    window.close();
}

function updatePreviewImage(formRef, fieldRef) {
    imgtag = document.forms[formRef].elements[fieldRef].value;
    previewDiv = document.getElementById('previewImage_' + fieldRef);
    previewDiv.innerHTML = '<a href="#" onclick="OpenWindow_edit_' + fieldRef + '();return false" title="edit an image from the image library" class="button">Edit image</a>' + imgtag;
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
                    $(this).find('li:not(:has(ul)):has("a.activeParent,a.hiddenParent")').addClass('levelExpandable').append('<ul/>');
                    $(this).expandToLevel(settings);
                }
                // Add the control classes to the tree
                $(this).find('li:has(ul):has("a.activeParent,a.hiddenParent")').addClass('collapsable');
                $(this).find('li:not(:has(ul)):has("a.activeParent,a.hiddenParent")').addClass('expandable');


                // Call buildtree
                // New (Hide) Version (v1.0.8)
                if (settings.hide) {
                    $(this).buildTree_noreload(settings);
                }
                // Old (Empty) Version (v1.0.0 -> v1.0.7)
                else {
                    $(this).buildTree(settings);
                }
            }
        },



        // Method for if the level param is in play
        expandToLevel: function (settings) {

            $('#MenuTree li.levelExpandable:has("a.activeParent,a.hiddenParent")').each(function () {
                $(this).removeClass('levelExpandable');
                $(this).removeClass('expandable');
                var ewPageId = $(this).attr('id').replace(/node/, "");
                $(this).find('ul').append('<div class="loadnode">Loading</div>')
                    .load(settings.loadPath, { ajaxCmd: settings.ajaxCmd, pgid: ewPageId }, function () {
                        $(this).children().find('li:has("a.activeParent,a.hiddenParent")').addClass('expandable');
                        $("#MenuTree").buildTree(settings);
                        settings.level = settings.level - 1;
                        if (settings.level > 0) {
                            $(this).children().find('li:has("a.activeParent,a.hiddenParent")').addClass('levelExpandable');
                            $("#MenuTree").expandToLevel(settings);
                        }
                    });
            });
        },


        // This function handles the tree's classes and mouse bindings for the "hit area"'s
        buildTree: function (settings) {
            // Add Hit area's (the clickable part)
            $('#MenuTree li.collapsable:not(:has(div.hitarea)):has("a.activeParent,a.hiddenParent")').prepend('<div class="hitarea collapsable-hitarea"/>');
            $('#MenuTree li.expandable:not(:has(div.hitarea)):has("a.activeParent,a.hiddenParent")').prepend('<div class="hitarea expandable-hitarea"/>');
            // Sort out assignments of the last tag
            $('#MenuTree').applyLast();
            $('#MenuTree li:not(:has(ul)):has("a.activeParent,a.hiddenParent")').append('<ul/>');
            // Remove any mouse bindings currently on the hitarea's
            $('#MenuTree li div.hitarea').unbind("click");


            //Mouse binding for open nodes
            $('#MenuTree li.collapsable').find('div.hitarea').unbind("click").click(function () {
                // Remove old class assingments
                $(this).removeClass('collapsable-hitarea').addClass('expandable-hitarea');
                // Remove the child tree
                $(this).parent().find('ul').empty();

                // Reset Class Status
                $(this).parent().removeClass('collapsable').addClass('expandable');
                // Set kids to be closed again
                $(this).children().find('li:has("a.activeParent,a.hiddenParent")').addClass('expandable');
                // Calling a rebuild assings the correct functionality
                $("#MenuTree").buildTree(settings)
                //settings doesn't work in the next line? so used above
                //setTimeout('$("#MenuTree").buildTree()',1);
            });


            //Mouse binding for closed nodes
            $('#MenuTree li.expandable').find('div.hitarea').unbind("click").click(function () {
                // Unbind after first click to prevent stupid users multiclicking
                $('#MenuTree li div.hitarea').unbind("click");
                // Remove current classes from the hit-area
                $(this).removeClass('expandable-hitarea').addClass('collapsable-hitarea');
                // Get the node's ID (used in load)
                var ewPageId = (this.parentNode.getAttribute('id').replace(/node/, ""));
                // Sort the parent node's class
                $(this).parent().removeClass('expandable').addClass('collapsable');
                // Append the loading line
                $(this).parent().find('ul').append('<div class="loadnode">Loading...</div>')

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
                        $(this).children().find('li:has("a.activeParent,a.hiddenParent")').addClass('expandable');
                        // Rebuild the tree
                        $("#MenuTree").buildTree(settings)
                    });
                }
                // Else for everything use, use the regular loading sequence
                else {



                    $(this).parent().find('ul').load(settings.loadPath, { ajaxCmd: settings.ajaxCmd, pgid: ewPageId, context: ewCloneContextId }, function () {
                        // Find out which of the kids have kids
                        $(this).children().find('li:has("a.activeParent,a.hiddenParent")').addClass('expandable');
                        // Rebuild the tree
                        $("#MenuTree").buildTree(settings)
                    });
                }

            });

        },



        // Same as above, prototype buildTree for no reloads      
        buildTree_noreload: function (settings) {
            // Add Hit area's (the clickable part)
            $('#MenuTree li.collapsable:not(:has(div.hitarea)):has("a.activeParent,a.hiddenParent")').prepend('<div class="hitarea collapsable-hitarea"/>');
            $('#MenuTree li.expandable:not(:has(div.hitarea)):has("a.activeParent,a.hiddenParent")').prepend('<div class="hitarea expandable-hitarea"/>');
            // Sort out assignments of the last tag
            $('#MenuTree').applyLast();
            $('#MenuTree li:not(:has(ul)):has("a.activeParent,a.hiddenParent")').append('<ul/>');
            // Remove any mouse bindings currently on the hitarea's
            $('#MenuTree li div.hitarea').unbind("click");


            //Mouse binding for open nodes
            $('#MenuTree li.collapsable').find('div.hitarea').unbind("click").click(function () {
                // Remove old class assingments
                $(this).removeClass('collapsable-hitarea').addClass('expandable-hitarea');
                // Remove the child tree
                $(this).parent().find('ul:first').hide();

                // Reset Class Status
                $(this).parent().removeClass('collapsable').addClass('expandable_loaded');
                // Set kids to be closed again
                ///////////////////////////// !!! what happens here? assume that they aren't open?
                //$(this).children().find('li:has("a.activeParent,a.hiddenParent")').addClass('expandable');			
                // Calling a rebuild assings the correct functionality
                $("#MenuTree").buildTree_noreload(settings)
            });


            //Mouse binding for closed nodes (First Time)
            $('#MenuTree li.expandable').find('div.hitarea').unbind("click").click(function () {
                // Unbind after first click to prevent stupid users multiclicking
                $('#MenuTree li div.hitarea').unbind("click");
                // Remove current classes from the hit-area
                $(this).removeClass('expandable-hitarea').addClass('collapsable-hitarea');
                // Get the node's ID (used in load)
                var ewPageId = (this.parentNode.getAttribute('id').replace(/node/, ""));
                // Sort the parent node's class
                $(this).parent().removeClass('expandable').addClass('collapsable');
                // Append the loading line
                $(this).parent().find('ul').append('<div class="loadnode">Loading...</div>')
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
                    $(this).parent().find('ul').load(settings.loadPath, { ajaxCmd: settings.ajaxCmd, pgid: originalPageId, expId: ewPageId, context: ewCloneContextId }, function () {
                        // Find out which of the kids have kids
                        $(this).children().find('li:has("a.activeParent,a.hiddenParent")').addClass('expandable');
                        // Rebuild the tree
                        $("#MenuTree").buildTree_noreload(settings)
                    });
                }
                else {

                    $(this).parent().find('ul').load(settings.loadPath, { ajaxCmd: settings.ajaxCmd, pgid: ewPageId, context: ewCloneContextId }, function () {
                        // Find out which of the kids have kids
                        $(this).children().find('li:has("a.activeParent,a.hiddenParent")').addClass('expandable');
                        // Rebuild the tree
                        $("#MenuTree").buildTree_noreload(settings)
                    });
                }

            });

            //Mouse binding for closed nodes (No Reload)
            $('#MenuTree li.expandable_loaded').find('div.hitarea').unbind("click").click(function () {
                $('#MenuTree li div.hitarea').unbind("click");
                $(this).removeClass('expandable-hitarea').addClass('collapsable-hitarea');
                // Get the node's ID (used in load) - ??? Needed anymore?
                //var ewPageId = (this.parentNode.getAttribute('id').replace(/node/,""));
                $(this).parent().removeClass('expandable_loaded').addClass('collapsable');

                $(this).parent().find('ul:first').show();
                $("#MenuTree").buildTree_noreload(settings)
            });


        },

        //This function sorts the flagging of "last" nodes (used for the gfx)
        applyLast: function () {
            //Start by assigning the new last nodes
            $('#MenuTree li.collapsable:last-child').addClass('lastCollapsable').removeClass('lastExpandable').children('div.hitarea').addClass('lastCollapsable-hitarea').removeClass('lastExpandable-hitarea');
            $('#MenuTree li.expandable:last-child').addClass('lastExpandable').removeClass('lastCollapsable').children('div.hitarea').addClass('lastExpandable-hitarea').removeClass('lastCollapsable-hitarea');
            $('#MenuTree li.expandable_loaded:last-child').addClass('lastExpandable').removeClass('lastCollapsable').children('div.hitarea').addClass('lastExpandable-hitarea').removeClass('lastCollapsable-hitarea');
            $('#MenuTree li:not(:has("a.activeParent,a.hiddenParent")):last-child').addClass('last');

            //Then remove past last flags from no longer last nodes
            $('#MenuTree li.last:not(:last-child)').removeClass('last');
            $('#MenuTree li.collapsable:not(:last-child)').removeClass('lastCollapsable').children('div.hitarea').removeClass('lastCollapsable-hitarea');
            $('#MenuTree li.expandable:not(:last-child)').removeClass('lastExpandable').children('div.hitarea').removeClass('lastExpandable-hitarea');
            $('#MenuTree li.expandable_loaded:not(:last-child)').removeClass('lastExpandable').children('div.hitarea').removeClass('lastExpandable-hitarea');
        },

        // The next set of functions handle the movement buttons (mainly animation)
        // Take in the node's id as input			
        moveUp: function (moveId) {
            var moveIdNode = "node" + moveId;

            if (!($('#MenuTree li #' + moveIdNode).hasClass("locked"))) {
                $('#MenuTree li #' + moveIdNode).addClass("locked");

                //Construct the node name
                $('#MenuTree li #' + moveIdNode).fadeTo("fast", 0.25);
                //IE is stupid, so append random numbers to the end
                var i = Math.round(10000 * Math.random());
                // Pass out the command to move the node
                $.ajax({
                    url: '?ewCmd=MoveUp' + decodeURIComponent("%26") + 'pgid=' + moveId + '&a=' + i,
                    success: function () {
                        //Animate
                        $('#MenuTree li #' + moveIdNode).prev().hide().fadeIn("fast");
                        $('#MenuTree li #' + moveIdNode).prev().before($('#MenuTree li #' + moveIdNode));
                        $('#MenuTree').applyLast();
                        $('#MenuTree li #' + moveIdNode).fadeTo("fast", 1.0);
                        $('#MenuTree li #' + moveIdNode).removeClass("locked");
                    }
                });


            }

        },

        moveDown: function (moveId) {
            var moveIdNode = "node" + moveId;

            if (!($('#MenuTree li #' + moveIdNode).hasClass("locked"))) {
                $('#MenuTree li #' + moveIdNode).addClass("locked");

                $('#MenuTree li #' + moveIdNode).fadeTo("fast", 0.25);
                var i = Math.round(10000 * Math.random());
                $.ajax({
                    url: '?ewCmd=MoveDown' + decodeURIComponent("%26") + 'pgid=' + moveId + '&a=' + i,
                    success: function () {
                        $('#MenuTree li #' + moveIdNode).next().hide().fadeIn("fast");
                        $('#MenuTree li #' + moveIdNode).next().after($('#MenuTree li #' + moveIdNode));
                        $('#MenuTree').applyLast();
                        $('#MenuTree li #' + moveIdNode).fadeTo("fast", 1.0);
                        $('#MenuTree li #' + moveIdNode).removeClass("locked");
                    }
                });
            }

        },

        moveTop: function (moveId) {
            var moveIdNode = "node" + moveId;

            if (!($('#MenuTree li #' + moveIdNode).hasClass("locked"))) {
                $('#MenuTree li #' + moveIdNode).addClass("locked");

                $('#MenuTree li #' + moveIdNode).fadeTo("fast", 0.25);
                var i = Math.round(10000 * Math.random());
                $.ajax({
                    url: '?ewCmd=MoveTop' + decodeURIComponent("%26") + 'pgid=' + moveId + '&a=' + i,
                    success: function () {
                        $('#MenuTree li #' + moveIdNode).prev().hide().fadeIn("fast");
                        $('#MenuTree li #' + moveIdNode).prevAll(':first-child').before($('#MenuTree li #' + moveIdNode));
                        $('#MenuTree').applyLast();
                        $('#MenuTree li #' + moveIdNode).fadeTo("fast", 1.0);
                        $('#MenuTree li #' + moveIdNode).removeClass("locked");
                    }
                });
            }
        },

        moveBottom: function (moveId) {
            var moveIdNode = "node" + moveId;

            if (!($('#MenuTree li #' + moveIdNode).hasClass("locked"))) {
                $('#MenuTree li #' + moveIdNode).addClass("locked");

                $('#MenuTree li #' + moveIdNode).fadeTo("fast", 0.25);
                var i = Math.round(10000 * Math.random());
                $.ajax({
                    url: '?ewCmd=MoveBottom' + decodeURIComponent("%26") + 'pgid=' + moveId + '&a=' + i,
                    success: function () {
                        $('#MenuTree li #' + moveIdNode).next().hide().fadeIn("fast");
                        $('#MenuTree li #' + moveIdNode).nextAll(':last-child').after($('#MenuTree li #' + moveIdNode));
                        $('#MenuTree').applyLast();
                        $('#MenuTree li #' + moveIdNode).fadeTo("fast", 1.0);
                        $('#MenuTree li #' + moveIdNode).removeClass("locked");
                    }
                });
            }
        },

        hideButton: function (hideId) {

            var hideIdNode = "node" + hideId;
            if (!($('#MenuTree li #' + hideIdNode).hasClass("locked"))) {
                $('#MenuTree li #' + hideIdNode).addClass("locked");

                $('#MenuTree li #' + hideIdNode).fadeTo("fast", 0.25);
                var i = Math.round(10000 * Math.random());
                $.ajax({
                    url: '?ewCmd=HidePage' + decodeURIComponent("%26") + 'pgid=' + hideId + '&a=' + i,
                    success: function () {

                        $('#MenuTree li #' + hideIdNode + ' a.btn-hide:first').remove();
                        $('#MenuTree li #' + hideIdNode + ' td.optionButtons:first').append(' <a onclick="$(\'#MenuTree\').showButton(' + hideId + ');" class="btn btn-sm btn-success btn-show" title="Click here to show this page"><i class="icon-ok-circle icon-white"> </i>Show</a>');
                        $('#MenuTree li #' + hideIdNode + ' td.optionButtons:first').append(' <a href="?ewCmd=DeletePage&amp;pgid=' + hideId + '" class="btn btn-sm btn-danger btn-del" title="Click here to delete this page"><i class="icon-remove-circle icon-white"> </i>Delete</a>');

                        if ($('#MenuTree li #' + hideIdNode + ' td.status a:first').hasClass('live')) {
                            $('#MenuTree li #' + hideIdNode + ' td.status a.btn-show:first').remove();
                            $('#MenuTree li #' + hideIdNode + ' td.status:first').append('<a class="status btn-hide" title="This content is hidden">   </a>');
                        }
                        else if ($('#MenuTree li #' + hideIdNode + ' td.status a:first').hasClass('activeParent')) {
                            $('#MenuTree li #' + hideIdNode + ' td.status a.activeParent:first').remove();
                            $('#MenuTree li #' + hideIdNode + ' td.status:first').append('<a class="status hiddenParent" title="This content is hidden">   </a>');
                        }

                        $('#MenuTree li #' + hideIdNode).fadeTo("fast", 1.0);
                        $('#MenuTree li #' + hideIdNode).removeClass("locked");
                        $('#MenuTree').applyLast();
                    }
                });
            }
        },

        showButton: function (showId) {
            var showIdNode = "node" + showId;
            if (!($('#MenuTree li #' + showIdNode).hasClass("locked"))) {
                $('#MenuTree li #' + showIdNode).addClass("locked");

                $('#MenuTree li #' + showIdNode).fadeTo("fast", 0.25);
                var i = Math.round(10000 * Math.random());
                $.ajax({
                    url: '?ewCmd=ShowPage' + decodeURIComponent("%26") + 'pgid=' + showId + '&a=' + i,
                    success: function () {
                        //Sort out removal of button and then addition of others
                        $('#MenuTree li #' + showIdNode + ' a.btn-show:first').remove();
                        $('#MenuTree li #' + showIdNode + ' a.btn-del:first').remove();
                        $('#MenuTree li #' + showIdNode + ' td.optionButtons:first').append('<a onclick="$(\'#MenuTree\').hideButton(' + showId + ');" class="btn btn-sm btn-danger btn-hide" title="Click here to hide this page"><i class="icon-ban-circle icon-white"> </i>Hide</a>');

                        if ($('#MenuTree li #' + showIdNode + ' td.status a:first').hasClass('hide')) {
                            $('#MenuTree li #' + showIdNode + ' td.status a.btn-hide:first').remove();
                            $('#MenuTree li #' + showIdNode + ' td.status:first').append('<a class="status btn-show" title="This content is live">   </a>');
                        }
                        else if ($('#MenuTree li #' + showIdNode + ' td.status a:first').hasClass('hiddenParent')) {
                            $('#MenuTree li #' + showIdNode + ' td.status a.hiddenParent:first').remove();
                            $('#MenuTree li #' + showIdNode + ' td.status:first').append('<a class="status activeParent" title="This content is live">   </a>');
                        }

                        $('#MenuTree li #' + showIdNode).fadeTo("fast", 1.0);
                        $('#MenuTree li #' + showIdNode).removeClass("locked");
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
    var treeviewPath = '/ewcommon/tools/ajaxadmin.ashx'
    if ($("#tpltMovePage").exists()) {
        var movingPageId = getParameterByName('pgid');
        treeviewPath = treeviewPath + '?movingPageId=' + movingPageId;
    }
    if ($("#tpltMoveContent").exists()) {
        var movingContentId = getParameterByName('id');
        var currentPageId = getParameterByName('parId');
        var startingPageId = getParameterByName('pgid');
        treeviewPath = treeviewPath + '?oldPgId=' + startingPageId + '&id=' + movingContentId;
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

                alert('Couldn\'t find the latitude and longitude for the address provided. Try including more details.');

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

                alert('Couldn\'t find the latitude and longitude for the address provided. Try including more details.');

            }

            // Change to initial button label
            $this.val(label);
        });
    });


}

//User Guide

$(function () {
    $('#btnHelpEditing').click(function () {
        $('#helpBox p').load($('#userGuideURL').attr('href'), afterLoad); // Load in userGuideURL defined in admin.xsl
        if ($('#divHelpBox').hasClass('open')) // Animation of the user guide pane and button (sliding)
            { $('#divHelpBox').animate({ right: -345 }, { duration: 500 }).removeClass('open'); }
        else
            $('#divHelpBox').animate({ right: 0 }, { duration: 500 }).addClass('open');
        if ($('#btnHelpEditing').hasClass('open'))
            { $('#btnHelpEditing').animate({ right: 0 }, { duration: 500 }).css({ backgroundPosition: '0 0' }).removeClass('open'); }
        else
            $('#btnHelpEditing').animate({ right: 345 }, { duration: 500 }).css({ backgroundPosition: '-34px 0' }).addClass('open');
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
    $('#helpBox a.lightbox').lightBox();
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


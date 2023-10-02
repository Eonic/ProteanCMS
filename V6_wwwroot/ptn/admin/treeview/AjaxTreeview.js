
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

            ThisTree = $(this);

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

            $(this).find("li[data-tree-level='" + settings.openLevel + "']").each(function () {
                //unless you have an active descendant
                if ($(this).activeChild() == false) {
                    $(this).hideChildren();
                }
            });
        },

        activeChild: function () {
            var isActive = false;
            var nodeId = $(this).attr('id').replace(/node/, "");
            $(this).find("li[data-tree-parent='" + nodeId + "']").each(function () {
                if ($(this).hasClass('active') || $(this).activeChild()) {
                    isActive = true;
                }
            });
            return isActive;
        },

        // Method for if the level param is in play
        expandToLevel: function (settings) {

            $(this).find('li.levelExpandable:has(".activeParent,.inactiveParent")').each(function () {
                $(this).removeClass('levelExpandable');
                $(this).removeClass('expandable');
                var ewPageId = $(this).attr('id').replace(/node/, "");
                $(this).insertAfter('<div class="loadnode">Loading <i class="fa fa-cog fa-spin fa-fw"> </i></div>')
                    .load(settings.loadPath, { ajaxCmd: settings.ajaxCmd, pgid: ewPageId }, function () {
                        $(this).children().find('li:has(".activeParent,.inactiveParent")').addClass('expandable');
                        ThisTree.buildTree(settings);
                        settings.level = settings.level - 1;
                        if (settings.level > 0) {
                            $(this).children().find('li:has(".activeParent,.inactiveParent")').addClass('levelExpandable');
                            ThisTree.expandToLevel(settings);
                        }
                    });
            });
        },


        // This function handles the tree's classes and mouse bindings for the "hit area"'s
        buildTree: function (settings) {
            // Add Hit area's (the clickable part)

            $(this).find('li.collapsable:not(:has(i.hitarea)):has(".activeParent,.inactiveParent")').prepend('<i class="hitarea collapsable-hitarea fa fa-chevron-down"> </i>');
            $(this).find('li.expandable:not(:has(i.hitarea)):has(".activeParent,.inactiveParent")').prepend('<i class="hitarea expandable-hitarea fa fa-chevron-right"> </i>');

            // Sort out assignments of the last tag
            $('#MenuTree').applyLast();
            // Remove any mouse bindings currently on the hitarea's
            $(this).find('li div.hitarea').unbind("click");

            //Mouse binding for open nodes
            $(this).find('li.collapsable').find('.hitarea').unbind("click").click(function () {
                // Remove old class assingments

                $(this).removeClass('collapsable-hitarea').addClass('expandable-hitarea');
                $(this).removeClass('fa-chevron-down').addClass('fa-chevron-right');

                // Remove the child tree

                $(this).parent().find('ul').empty();

                // Reset Class Status
                $(this).parent().removeClass('collapsable').addClass('expandable');
                // Set kids to be closed again
                $(this).children().find('li:has(".activeParent,.inactiveParent")').addClass('expandable');
                // Calling a rebuild assings the correct functionality
                ThisTree.buildTree(settings)
                //settings doesn't work in the next line? so used above
                //setTimeout('$("#MenuTree").buildTree()',1);
            });


            //Mouse binding for closed nodes
            $(this).find('li.expandable').find('.hitarea').unbind("click").click(function () {
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
                        ThisTree.buildTree(settings)

                    });
                }
                // Else for everything use, use the regular loading sequence
                else {


                    $(this).parent().find('ul').load(settings.loadPath, { ajaxCmd: settings.ajaxCmd, pgid: ewPageId, context: ewCloneContextId }, function () {
                        // Find out which of the kids have kids
                        //alert('test');
                        $(this).children().find('li').has('i.activeParent,i.inactiveParent').addClass('expandable');
                        // Rebuild the tree
                        ThisTree.buildTree(settings)

                    });
                }

            });


            $(this).find('.btn-hide').unbind("click").click(function () {
                var pageId = (this.parentNode.getAttribute('id').replace(/node/, ""));
                alert(pageId);
                $(this).hideButton(pageId);
            });

            $(this).find('.btn-show').unbind("click").click(function () {
                var pageId = (this.parentNode.getAttribute('id').replace(/node/, ""));
                alert(pageId);
                $(this).showButton(pageId);
            });

        },



        // Same as above, prototype buildTree for no reloads      
        buildTree_noreload: function (settings) {

            alert("Build tree no reload");

            // Add Hit area's (the clickable part)
            $(this).find('li.collapsable:not(:has(i.hitarea)):has(".activeParent,.inactiveParent")').prepend('<i class="hitarea collapsable-hitarea fa fa-chevron-down"> </i>');
            $(this).find('li.expandable:not(:has(i.hitarea)):has(".activeParent,.inactiveParent")').prepend('<i class="hitarea expandable-hitarea fa fa-chevron-right"> </i>');

            // Sort out assignments of the last tag
            $('#MenuTree').applyLast();
            // Remove any mouse bindings currently on the hitarea's
            $('#MenuTree li div.hitarea').unbind("click");

            // alert('treeload');

            //Mouse binding for open nodes
            $(this).find('.collapsable-hitarea').unbind("click").click(function () {

                $(this).parent().hideChildren();

                // Reset Class Status

                // Set kids to be closed again
                ///////////////////////////// !!! what happens here? assume that they aren't open?
                //$(this).children().find('li:has(".activeParent,.inactiveParent")').addClass('expandable');			
                // Calling a rebuild assings the correct functionality
                ThisTree.buildTree_noreload(settings)
            });


            //Mouse binding for closed nodes (First Time)
            $(this).find('li.expandable').find('.hitarea').unbind("click").click(function () {
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
                        $(this).buildTree_noreload(settings)
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
                        $(this).buildTree_noreload(settings)
                    });


                }

            });

            //Mouse binding for closed nodes (No Reload)
            $(this).find('li.expandable_loaded').find('.hitarea').unbind("click").click(function () {
                $('#MenuTree li i.hitarea').unbind("click");
                $(this).removeClass('expandable-hitarea').addClass('collapsable-hitarea');
                $(this).removeClass('fa-chevron-right').addClass('fa-chevron-down');
                // Get the node's ID (used in load) - ??? Needed anymore?
                //var ewPageId = (this.parentNode.getAttribute('id').replace(/node/,""));
                $(this).parent().removeClass('expandable_loaded').addClass('collapsable');

                var ewPageId = (this.parentNode.getAttribute('id').replace(/node/, ""));
                $(this).parent().parent().find('li[data-tree-parent="' + ewPageId + '"]').show();

                ThisTree.buildTree_noreload(settings)
            });

            $(this).find('.btn-hide').unbind("click").click(function () {
                var pageId = this.parentNode.parentNode.getAttribute('id').replace("node", "");
                $(this).hideButton(pageId);
                $(this).buildTree_noreload(settings)
            });

            $(this).find('.btn-show').unbind("click").click(function () {
                var pageId = this.parentNode.parentNode.getAttribute('id').replace("node", "");
                $(this).showButton(pageId);
                $(this).buildTree_noreload(settings)
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
            $(this).find('li').each(function () {
                var thisParentId = $(this).data('tree-parent')

                if ($(this).prevAll('li[data-tree-parent="' + thisParentId + '"]').length == 0) {
                    //if first amoung siblings
                    if ($(this).prev('li[id="node' + thisParentId + '"]').length == 0) {
                        //if incorrect parent
                        if (ThisTree.find('li[id="node' + thisParentId + '"] li[data-tree-parent="' + thisParentId + '"]').length == 0) {
                            //no siblings moved allready
                            // $('#MenuTree li[id="node' + thisParentId + '"]').after($(this))
                            $(this).nextAll('li[data-tree-parent="' + thisParentId + '"]').reverse().each(function () {
                                ThisTree.find('li[id="node' + thisParentId + '"]').after($(this))
                            });
                            ThisTree.find('li[id="node' + thisParentId + '"]').after($(this))
                        }
                    }
                }
            })
        },


        //This function sorts the flagging of "last" nodes (used for the gfx)
        applyLast: function () {
            //Hide and show Up Down Buttons
            $(this).find('li').each(function () {

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
            var ThisTree = $('#MenuTree');
            if (!(ThisTree.find('li#' + moveIdNode).hasClass("locked"))) {
                ThisTree.find('li#' + moveIdNode).addClass("locked");
                var thisParentId = ThisTree.find('li#' + moveIdNode).data('tree-parent')
                //Construct the node name
                ThisTree.find('li#' + moveIdNode).fadeTo("fast", 0.25);
                //IE is stupid, so append random numbers to the end
                var i = Math.round(10000 * Math.random());
                // Pass out the command to move the node
                $.ajax({
                    url: '?ewCmd=MoveUp' + decodeURIComponent("%26") + 'pgid=' + moveId + '&a=' + i,
                    success: function () {
                        //Animate
                        ThisTree.find('li#' + moveIdNode).prevAll('li[data-tree-parent="' + thisParentId + '"]').first().hide().fadeIn("fast");
                        ThisTree.find('li#' + moveIdNode).prevAll('li[data-tree-parent="' + thisParentId + '"]').first().before($('#MenuTree li#' + moveIdNode));
                        ThisTree.applyLast();
                        ThisTree.checkChildren();
                        ThisTree.find('li#' + moveIdNode).fadeTo("fast", 1.0);
                        ThisTree.find('li#' + moveIdNode).removeClass("locked");
                        // alert('move up');

                    }
                });


            }

        },

        moveDown: function (moveId) {
            var moveIdNode = "node" + moveId;

            var ThisTree = $('#MenuTree');
            if (!(ThisTree.find('li#' + moveIdNode).hasClass("locked"))) {
                ThisTree.find(' li#' + moveIdNode).addClass("locked");

                var thisParentId = ThisTree.find('li#' + moveIdNode).data('tree-parent')

                ThisTree.find('li#' + moveIdNode).fadeTo("fast", 0.25);
                var i = Math.round(10000 * Math.random());
                $.ajax({
                    url: '?ewCmd=MoveDown' + decodeURIComponent("%26") + 'pgid=' + moveId + '&a=' + i,
                    success: function () {
                        ThisTree.find('li#' + moveIdNode).prevAll('li[data-tree-parent="' + thisParentId + '"]').first().hide().fadeIn("fast");
                        ThisTree.find('li#' + moveIdNode).nextAll('li[data-tree-parent="' + thisParentId + '"]').first().after(ThisTree.find('li#' + moveIdNode));
                        ThisTree.applyLast();
                        ThisTree.checkChildren();
                        ThisTree.find('li#' + moveIdNode).fadeTo("fast", 1.0);
                        ThisTree.find('li#' + moveIdNode).removeClass("locked");
                        // alert('move down');

                    }
                });
            }

        },

        moveTop: function (moveId) {
            var moveIdNode = "node" + moveId;
            var ThisTree = $('#MenuTree');
            //alert(moveId);
            if (!(ThisTree.find('li#' + moveIdNode).hasClass("locked"))) {
                ThisTree.find('li#' + moveIdNode).addClass("locked");
                ThisTree.find('li#' + moveIdNode).fadeTo("fast", 0.25);
                var thisParentId = ThisTree.find('li#' + moveIdNode).data('tree-parent')
                var i = Math.round(10000 * Math.random());
                $.ajax({
                    url: '?ewCmd=MoveTop' + decodeURIComponent("%26") + 'pgid=' + moveId + '&a=' + i,
                    success: function () {
                        ThisTree.find('li#' + moveIdNode).prev('li[data-tree-parent="' + thisParentId + '"]').hide().fadeIn("fast");
                        ThisTree.find('li#' + moveIdNode).prevAll('li[data-tree-parent="' + thisParentId + '"]:last').before(ThisTree.find('li#' + moveIdNode));
                        ThisTree.applyLast();
                        ThisTree.checkChildren();
                        ThisTree.find('li#' + moveIdNode).fadeTo("fast", 1.0);
                        ThisTree.find('li#' + moveIdNode).removeClass("locked");
                    }
                });
            }
        },

        moveBottom: function (moveId) {
            var moveIdNode = "node" + moveId;
            var ThisTree = $('#MenuTree');

            if (!(ThisTree.find('li#' + moveIdNode).hasClass("locked"))) {
                ThisTree.find('li#' + moveIdNode).addClass("locked");
                ThisTree.find('li#' + moveIdNode).fadeTo("fast", 0.25);
                var thisParentId = ThisTree.find('li#' + moveIdNode).data('tree-parent')
                var i = Math.round(10000 * Math.random());
                $.ajax({
                    url: '?ewCmd=MoveBottom' + decodeURIComponent("%26") + 'pgid=' + moveId + '&a=' + i,
                    success: function () {
                        ThisTree.find('li#' + moveIdNode).next().hide('li[data-tree-parent="' + thisParentId + '"]').fadeIn("fast");
                        ThisTree.find('li#' + moveIdNode).nextAll('li[data-tree-parent="' + thisParentId + '"]:last').after(ThisTree.find('li#' + moveIdNode));
                        ThisTree.applyLast();
                        ThisTree.checkChildren();
                        ThisTree.find('li#' + moveIdNode).fadeTo("fast", 1.0);
                        ThisTree.find('li#' + moveIdNode).removeClass("locked");
                    }
                });
            }
        },

        hideButton: function (hideId) {

            var hideIdNode = "node" + hideId;
            var ThisTree = $('#MenuTree');
            if (!(ThisTree.find('li#' + hideIdNode).hasClass("locked"))) {
                ThisTree.find('li#' + hideIdNode).addClass("locked");
                ThisTree.find('li#' + hideIdNode).fadeTo("fast", 0.25);
                var i = Math.round(10000 * Math.random());
                var callUrl = '?ewCmd=HidePage&pgid=' + hideId + '&a=' + i;
                $.ajax({
                    url: callUrl,
                    success: function () {
                        ThisTree.find('li#' + hideIdNode + ' a.btn-hide').remove();
                        ThisTree.find('li#' + hideIdNode + ' a.btn-show').remove();
                        ThisTree.find('li#' + hideIdNode + ' div.optionButtons:first').append(' <a class="btn btn-xs btn-primary btn-show" title="Click here to show this page"><i class="fas fa-eye fa-white"> </i> Show</a>');
                        ThisTree.find('li#' + hideIdNode + ' div.optionButtons:first').append(' <a href="?ewCmd=DeletePage&amp;pgid=' + hideId + '" class="text-danger plain-link btn-del" title="Click here to delete this page"><i class="fas fa-trash-alt fa-white"> </i> Delete</a>');

                        if (ThisTree.find('li#' + hideIdNode + ' .pageCell i').hasClass('active')) {
                            ThisTree.find('li#' + hideIdNode + ' .pageCell i').removeClass('active');
                            ThisTree.find('li#' + hideIdNode + ' .pageCell i').addClass('inactive');
                            ThisTree.find('li#' + hideIdNode + ' .pageCell i').removeClass('fas');
                            ThisTree.find('li#' + hideIdNode + ' .pageCell i').addClass('far');
                        }
                        ThisTree.find('li#' + hideIdNode).addClass('inactive-row');
                        ThisTree.find('li#' + hideIdNode).fadeTo("fast", 1.0);
                        ThisTree.find('li#' + hideIdNode).removeClass("locked");
                        ThisTree.applyLast();
                        ThisTree.find('li#' + hideIdNode + ' .btn-show').click(function () {
                            $(this).showButton(hideId);
                        });
                    }
                });
            }
        },

        showButton: function (showId) {
            var showIdNode = "node" + showId;
            var ThisTree = $('#MenuTree');
            if (!(ThisTree.find('li#' + showIdNode).hasClass("locked"))) {
                ThisTree.find('li#' + showIdNode).addClass("locked");
                ThisTree.find('li#' + showIdNode).fadeTo("fast", 0.25);
                var i = Math.round(10000 * Math.random());
                var callurl = '?ewCmd=ShowPage&pgid=' + showId + '&a=' + i;
                $.ajax({
                    url: callurl,
                    success: function () {
                        //Sort out removal of button and then addition of others
                        ThisTree.find('li#' + showIdNode + ' a.btn-del:first').remove();
                        ThisTree.find('li#' + showIdNode + ' a.btn-show').remove();
                        ThisTree.find('li#' + showIdNode + ' div.optionButtons:first').append('<a class="btn btn-xs btn-primary btn-hide" title="Click here to hide this page"><i class="fas fa-eye-slash fa-white"> </i> Hide</a>');

                        if (ThisTree.find('li#' + showIdNode + ' .pageCell i').hasClass('inactive')) {
                            ThisTree.find('li#' + showIdNode + ' .pageCell i').removeClass('inactive');
                            ThisTree.find('li#' + showIdNode + ' .pageCell i').addClass('active');
                            ThisTree.find('li#' + showIdNode + ' .pageCell i').removeClass('far');
                            ThisTree.find('li#' + showIdNode + ' .pageCell i').addClass('fas');
                        }

                        ThisTree.find('li#' + showIdNode).removeClass('inactive-row');
                        ThisTree.find('li#' + showIdNode).fadeTo("fast", 1.0);
                        ThisTree.find('li#' + showIdNode).removeClass("locked");
                        ThisTree.applyLast();
                        ThisTree.find('li#' + showIdNode + ' .btn-hide').click(function () {
                            $(this).hideButton(showId);
                        });
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

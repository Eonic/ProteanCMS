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

 
 ;(function($) {

     $.extend($.fn, {

         // Constructor
         ajaxtreeview: function(settings) {
             
             $('#MenuTree').hide

             $('#MenuTree').addClass("treeview");
             // Check if levels have been defined, if so, pre-open			
             if (settings.level > 0) {
                 $('#MenuTree li:not(:has(ul)):has("a.activeParent,a.hiddenParent")').addClass('levelExpandable').append('<ul/>');
                 $('#MenuTree').expandToLevel(settings);
             }
             // Add the control classes to the tree
             $('#MenuTree li:has(ul):has("a.activeParent,a.hiddenParent")').addClass('collapsable');
             $('#MenuTree li:not(:has(ul)):has("a.activeParent,a.hiddenParent")').addClass('expandable');


             // Call buildtree
             // New (Hide) Version (v1.0.8)
             if (settings.hide) {
                 $('#MenuTree').buildTree_noreload(settings);
             }
             // Old (Empty) Version (v1.0.0 -> v1.0.7)
             else {
                 $('#MenuTree').buildTree(settings);
             }

         },



         // Method for if the level param is in play
         expandToLevel: function(settings) {
             $('#MenuTree li.levelExpandable:has("a.activeParent,a.hiddenParent")').each(function() {
                 $(this).removeClass('levelExpandable');
                 $(this).removeClass('expandable');
                 var ewPageId = $(this).attr('id').replace(/node/, "");
                 $(this).find('ul').append('<div class="loadnode">Loading</div>')
                    .load(settings.loadPath, { ajaxCmd: settings.ajaxCmd, pgid: ewPageId }, function() {
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
         buildTree: function(settings) {
             // Add Hit area's (the clickable part)
             $('#MenuTree li.collapsable:not(:has(div.hitarea)):has("a.activeParent,a.hiddenParent")').prepend('<div class="hitarea collapsable-hitarea"/>');
             $('#MenuTree li.expandable:not(:has(div.hitarea)):has("a.activeParent,a.hiddenParent")').prepend('<div class="hitarea expandable-hitarea"/>');
             // Sort out assignments of the last tag
             $('#MenuTree').applyLast();
             $('#MenuTree li:not(:has(ul)):has("a.activeParent,a.hiddenParent")').append('<ul/>');
             // Remove any mouse bindings currently on the hitarea's
             $('#MenuTree li div.hitarea').unbind("click");


             //Mouse binding for open nodes
             $('#MenuTree li.collapsable').find('div.hitarea').unbind("click").click(function() {
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
             $('#MenuTree li.expandable').find('div.hitarea').unbind("click").click(function() {
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
                     $(this).parent().find('ul').load(settings.loadPath, { ajaxCmd: settings.ajaxCmd, pgid: originalPageId, expId: ewPageId, context: ewCloneContextId }, function() {
                         // Find out which of the kids have kids
                         $(this).children().find('li:has("a.activeParent,a.hiddenParent")').addClass('expandable');
                         // Rebuild the tree
                         $("#MenuTree").buildTree(settings)
                     });
                 }
                 // Else for everything use, use the regular loading sequence
                 else {



                     $(this).parent().find('ul').load(settings.loadPath, { ajaxCmd: settings.ajaxCmd, pgid: ewPageId, context: ewCloneContextId }, function() {
                         // Find out which of the kids have kids
                         $(this).children().find('li:has("a.activeParent,a.hiddenParent")').addClass('expandable');
                         // Rebuild the tree
                         $("#MenuTree").buildTree(settings)
                     });
                 }

             });

         },



         // Same as above, prototype buildTree for no reloads      
         buildTree_noreload: function(settings) {
             // Add Hit area's (the clickable part)
             $('#MenuTree li.collapsable:not(:has(div.hitarea)):has("a.activeParent,a.hiddenParent")').prepend('<div class="hitarea collapsable-hitarea"/>');
             $('#MenuTree li.expandable:not(:has(div.hitarea)):has("a.activeParent,a.hiddenParent")').prepend('<div class="hitarea expandable-hitarea"/>');
             // Sort out assignments of the last tag
             $('#MenuTree').applyLast();
             $('#MenuTree li:not(:has(ul)):has("a.activeParent,a.hiddenParent")').append('<ul/>');
             // Remove any mouse bindings currently on the hitarea's
             $('#MenuTree li div.hitarea').unbind("click");


             //Mouse binding for open nodes
             $('#MenuTree li.collapsable').find('div.hitarea').unbind("click").click(function() {
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
             $('#MenuTree li.expandable').find('div.hitarea').unbind("click").click(function() {
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
                     $(this).parent().find('ul').load(settings.loadPath, { ajaxCmd: settings.ajaxCmd, pgid: originalPageId, expId: ewPageId, context: ewCloneContextId }, function() {
                         // Find out which of the kids have kids
                         $(this).children().find('li:has("a.activeParent,a.hiddenParent")').addClass('expandable');
                         // Rebuild the tree
                         $("#MenuTree").buildTree_noreload(settings)
                     });
                 }
                 else {

                     $(this).parent().find('ul').load(settings.loadPath, { ajaxCmd: settings.ajaxCmd, pgid: ewPageId, context: ewCloneContextId }, function() {
                         // Find out which of the kids have kids
                         $(this).children().find('li:has("a.activeParent,a.hiddenParent")').addClass('expandable');
                         // Rebuild the tree
                         $("#MenuTree").buildTree_noreload(settings)
                     });
                 }

             });

             //Mouse binding for closed nodes (No Reload)
             $('#MenuTree li.expandable_loaded').find('div.hitarea').unbind("click").click(function() {
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
         applyLast: function() {
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
         moveUp: function(moveId) {
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
                     success: function() {
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

         moveDown: function(moveId) {
             var moveIdNode = "node" + moveId;

             if (!($('#MenuTree li #' + moveIdNode).hasClass("locked"))) {
                 $('#MenuTree li #' + moveIdNode).addClass("locked");

                 $('#MenuTree li #' + moveIdNode).fadeTo("fast", 0.25);
                 var i = Math.round(10000 * Math.random());
                 $.ajax({
                     url: '?ewCmd=MoveDown' + decodeURIComponent("%26") + 'pgid=' + moveId + '&a=' + i,
                     success: function() {
                         $('#MenuTree li #' + moveIdNode).next().hide().fadeIn("fast");
                         $('#MenuTree li #' + moveIdNode).next().after($('#MenuTree li #' + moveIdNode));
                         $('#MenuTree').applyLast();
                         $('#MenuTree li #' + moveIdNode).fadeTo("fast", 1.0);
                         $('#MenuTree li #' + moveIdNode).removeClass("locked");
                     }
                 });
             }

         },

         moveTop: function(moveId) {
             var moveIdNode = "node" + moveId;

             if (!($('#MenuTree li #' + moveIdNode).hasClass("locked"))) {
                 $('#MenuTree li #' + moveIdNode).addClass("locked");

                 $('#MenuTree li #' + moveIdNode).fadeTo("fast", 0.25);
                 var i = Math.round(10000 * Math.random());
                 $.ajax({
                     url: '?ewCmd=MoveTop' + decodeURIComponent("%26") + 'pgid=' + moveId + '&a=' + i,
                     success: function() {
                         $('#MenuTree li #' + moveIdNode).prev().hide().fadeIn("fast");
                         $('#MenuTree li #' + moveIdNode).prevAll(':first-child').before($('#MenuTree li #' + moveIdNode));
                         $('#MenuTree').applyLast();
                         $('#MenuTree li #' + moveIdNode).fadeTo("fast", 1.0);
                         $('#MenuTree li #' + moveIdNode).removeClass("locked");
                     }
                 });
             }
         },

         moveBottom: function(moveId) {
             var moveIdNode = "node" + moveId;

             if (!($('#MenuTree li #' + moveIdNode).hasClass("locked"))) {
                 $('#MenuTree li #' + moveIdNode).addClass("locked");

                 $('#MenuTree li #' + moveIdNode).fadeTo("fast", 0.25);
                 var i = Math.round(10000 * Math.random());
                 $.ajax({
                     url: '?ewCmd=MoveBottom' + decodeURIComponent("%26") + 'pgid=' + moveId + '&a=' + i,
                     success: function() {
                         $('#MenuTree li #' + moveIdNode).next().hide().fadeIn("fast");
                         $('#MenuTree li #' + moveIdNode).nextAll(':last-child').after($('#MenuTree li #' + moveIdNode));
                         $('#MenuTree').applyLast();
                         $('#MenuTree li #' + moveIdNode).fadeTo("fast", 1.0);
                         $('#MenuTree li #' + moveIdNode).removeClass("locked");
                     }
                 });
             }
         },

         hideButton: function(hideId) {

             var hideIdNode = "node" + hideId;
             if (!($('#MenuTree li #' + hideIdNode).hasClass("locked"))) {
                 $('#MenuTree li #' + hideIdNode).addClass("locked");

                 $('#MenuTree li #' + hideIdNode).fadeTo("fast", 0.25);
                 var i = Math.round(10000 * Math.random());
                 $.ajax({
                     url: '?ewCmd=HidePage' + decodeURIComponent("%26") + 'pgid=' + hideId + '&a=' + i,
                     success: function() {

                         $('#MenuTree li #' + hideIdNode + ' a.hideButton:first').remove();
                         $('#MenuTree li #' + hideIdNode + ' td.optionButtons:first').append('<a onclick="$(\'#MenuTree\').showButton(' + hideId + ');" class="button show" title="Click here to show this page">Show</a>');
                         $('#MenuTree li #' + hideIdNode + ' td.optionButtons:first').append('<a href="?ewCmd=DeletePage&amp;pgid=' + hideId + '" class="button delete" title="Click here to delete this page">Delete</a>');

                         if ($('#MenuTree li #' + hideIdNode + ' td.status a:first').hasClass('live')) {
                             $('#MenuTree li #' + hideIdNode + ' td.status a.live:first').remove();
                             $('#MenuTree li #' + hideIdNode + ' td.status:first').append('<a class="status hide" title="This content is hidden">   </a>');
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

         showButton: function(showId) {
             var showIdNode = "node" + showId;
             if (!($('#MenuTree li #' + showIdNode).hasClass("locked"))) {
                 $('#MenuTree li #' + showIdNode).addClass("locked");

                 $('#MenuTree li #' + showIdNode).fadeTo("fast", 0.25);
                 var i = Math.round(10000 * Math.random());
                 $.ajax({
                     url: '?ewCmd=ShowPage' + decodeURIComponent("%26") + 'pgid=' + showId + '&a=' + i,
                     success: function() {
                         //Sort out removal of button and then addition of others
                         $('#MenuTree li #' + showIdNode + ' a.show:first').remove();
                         $('#MenuTree li #' + showIdNode + ' a.delete:first').remove();
                         $('#MenuTree li #' + showIdNode + ' td.optionButtons:first').append('<a onclick="$(\'#MenuTree\').hideButton(' + showId + ');" class="button hideButton" title="Click here to hide this page">Hide</a>');

                         if ($('#MenuTree li #' + showIdNode + ' td.status a:first').hasClass('hide')) {
                             $('#MenuTree li #' + showIdNode + ' td.status a.hide:first').remove();
                             $('#MenuTree li #' + showIdNode + ' td.status:first').append('<a class="status live" title="This content is live">   </a>');
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
         urlParam: function(param) {
             var regex = '[?&]' + param + '=([^&#]*)';
             var results = (new RegExp(regex)).exec(window.location.href);
             if (results) return results[1];
             return '';
         }


     });

 })(jQuery);
/*
 * jQuery UI Ajax Accordion 1.0 (22-Oct-08)
 *
 * @Author - Nathan Brown '08
 * 
 * Used to replace general jQuery Accordion
 * This version implements Ajax load procedures
 *
 */

 ;(function($){
 
     $.extend($.fn, {
     // Constructor
        ajaxaccordion: function(settings){			
            // The only setting currently in use is collapse
			$('#listA').runFirst();			
			$('#listA').initAccordion(settings);
        },
     
     runFirst: function(){ // Assigns Closed to all nodes
        $('#listA a').addClass('Closed');
        //$('#listA a').addClass('unlock');(old unlock code)
     },
     
     initAccordion: function(settings){ // Recursive function to handle accordion's operations
        // All close node's click functions
        $('#listA a.Closed').unbind("click").click(function() {
        //,a.unlock (old unlock code)
        
            $(this).unbind("click");
            // Get the id of this node to open
            var PageId = this.getAttribute('id');
            
            // If collapse is enabled, close all other nodes
            if (settings.collapse){
                $('#listA a.Open').find('div.newsarticle').slideUp("slow",function(){
                    $(this).parent().parent().find('div').remove('div');
                });
                // Apply relevant classes
                $('#listA a.Open').swapClass('Closed','Open');
                $('#listA a').unbind("click");
            }
            
            // Assign classes
            $(this).swapClass('Open','Closed');
            
            // Get this page's location
            var pagePath = $(this).getPagePath()
            // Save the "span" text to add back in later
            var saveText = $(this).find('span').text();
            // Assign loading variables
            $(this).find('span').text('..Loading....');
            $(this).append('<div class="loadnodeacc"></div>')
                           
                // Load the node and assign the new click functionality          
                $(this).find('span').next().hide().load(pagePath,{ajax: "accordion", acro: PageId},function(){
                    // Animate
                    $(this).slideDown("slow", function(){
                    // Assign new "click", handles closing
                    $('#listA a.Open').unbind("click").click(function() {
                            $(this).unbind("click");
                            // Assign classes 
                            $(this).swapClass('Closed','Open');
                            // Animate then remove the node
                            $(this).find('div.newsarticle').slideUp("slow",function(){
                                $(this).parent().parent().find('div').remove('div');
                                $(this).initAccordion(settings);
                            });
                            
                        });
                    });
                    // Replace loading tag with original span text
                    $(this).parent().find('span').text(saveText);
                    $(this).initAccordion(settings);
                });

        });
        return true;

     },
     
     getPagePath: function(){ //Taken from Trev's Ajax Accordion
	    //fix for ie adding the #link to document location(MS! I ask you!)
	    var strLastHash = document.location.toString().lastIndexOf('#')
	    if (strLastHash == -1){
	        var pagePath = document.location.toString();
	    }
	    else{
	        var pagePath = document.location.toString().substring(0,strLastHash);
	    }
	    return pagePath;
			
	},
     
     swapClass: function(addC, removeC){
        $(this).addClass(addC);
        $(this).removeClass(removeC);
     }
      
     });
 
 })(jQuery);
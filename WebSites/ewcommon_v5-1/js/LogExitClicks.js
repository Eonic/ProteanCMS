$(document).ready(function () {

    var pageId = $('body').attr('id').match(/page(\d*)/)[1];

    if ($('body').attr('id').match(/art(\d*)/)) {
        var artId = $('body').attr('id').match(/art(\d*)/)[1];
    }
    else {
        var artId = 0;
    }
 //   alert(artId);

    $('a[href^=http]').add('a[href^=mailto]').click(function () {

        var targetURL = $(this).attr('href');
        var loc = targetURL,
            index = loc.indexOf('#');
            if (index > 0) {
                targetURL = loc.substring(0, index);
            }

        var type;
	var destination;
        if (targetURL.match("^mailto")) { 
	        type = 'email';
	        destination = 'email';
	      }
        if (targetURL.match("^http")) { 
	        type = 'website' ;
	        destination = encodeURI(targetURL);
	      }
        var ajaxURL = '/ewcommon/tools/ajaxActivityLog.ashx?artId=' + artId + '&pgid=' + pageId + '&destination=' + destination + "&type=" + type;
        //alert(ajaxURL);
      //  prompt("Copy to clipboard: Ctrl+C, Enter", ajaxURL);
        $.ajax(ajaxURL);
        return true;
    });

});


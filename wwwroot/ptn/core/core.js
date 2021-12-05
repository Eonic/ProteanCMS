$(document).ready(function () {
    $(".navbar-brand").click(function () {
        $(this).toggleClass('show-click');
    });
});
/*  ===============================================================================================  */
/*  ==  EXTEND JQUERY  ============================================================================  */
/*  ===============================================================================================  */

// Simple .exists() function - $(selector).exists(); - return true or false
jQuery.fn.exists = function () { return jQuery(this).length > 0; }

// EXTENTION TO GET ALL URL PARAMS IN AN OBJECT
$.extend({
    getURLParams: function () {
        var vars = [], hash;
        var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
        for (var i = 0; i < hashes.length; i++) {
            hash = hashes[i].split('=');
            vars.push(hash[0]);
            vars[hash[0]] = hash[1];
        }
        return vars;
    },
    getUrlParam: function (name) {
        return $.getURLParams()[name];
    }
});

/*  ===============================================================================================  */
/*  ==  END EXTEND JQUERY  ========================================================================  */
/*  ===============================================================================================  */

// Give the first form element focus
// $('#mainLayout').find(':input:visible:enabled:first:').focus();
if (typeof popover == 'function') {
    if ($(".mypopover").exists()) {
        $(".mypopover").popover({ trigger: 'hover' })
    }

    $('[rel=frmPopover]').popover({
        html: true,
        trigger: 'hover',
        content: function () {
            return $($(this).data('contentwrapper')).html();
        }
    });
}
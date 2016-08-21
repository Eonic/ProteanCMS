// REMOVE LIST GROUP ITEM ON NON BS SITES

$(document).ready(function () {
    $('div').removeClass('list-group-item');
    $('li').removeClass('list-group-item');
    $('div').removeClass('media');
    $('.normalMode input').removeClass('form-control');

    var bootstrapTooltip = $.fn.tooltip.noConflict() // return $.fn.button to previously assigned value
    $.fn.bootstrapTooltip = bootstrapTooltip
});

/*
* xFormEditor 1.0.0
* ------------------
* a jquery tool to enable xforms to be edited.
*
* Author 2012 Trevor Spink
*
* Latest Update : 15-Jun-2012
*
*/

;(function ($) {
var sForm
var xFormDom
var xInstance
var xBind
var xModel
var xRootGroup
    
    bindOptions = '<option value="false"><label>None</label></option><option value="Default Box"><label>Default Box</label></option>'
    requiredOptions = '<option value="true()"><label>True</label></option><option value="false()"><label>False</label></option>'
    
    $.extend($.fn, {

               

        // Constructor
        xFormEditor: function (settings) {
       // $(this).hide();
        $(this).after('<div id="xformEditor"><h4>xForm Editor</h4><div id="xFormControls"></div></div>');
        sForm = $(this).val();
        //translate chars
        sForm = sForm.replace('&lt;','<');
        sForm = sForm.replace('&gt;','>');
        sForm = '<xForm>' + sForm + '</xForm>';
        xFormDom = $.parseXML(sForm);
        $xForm = $(xFormDom);
        xInstance = $xForm.find('model instance');

        //loop through groups and output
        $xForm.find('> group').first().xfeRenderGroup({
                location : $('#xFormControls')
        });
        
//        $xForm.find('input').xfeRenderControls({
//                location : $('#xFormControls')
//        });

        // alert(xmlToString($(xInstance)[0]))
        },

        xfeRenderGroup: function (settings) {
            label = $(this).find('> label').first();
            labelName = "no label";
            if (label) {labelName = $(label).text();};

            controls = '<div class="options"><a href="#" class="adminButton popup">Add</a>'
                       + '<div class="ewPopMenu">'
                       + '<a href="#" class="adminButton addgroup" title="Add a Group to the form">New Group</a>'
                       + '<a href="#" class="adminButton addinput" title="Add a Textbox">Single-line Text</a>'
                       + '<a href="#" class="adminButton addinput" title="Add a Textarea">Multi-line Text</a>'
                       + '<a href="#" class="adminButton addinput" title="Add a Formatted Text">HTML Text</a>'
                       + '<a href="#" class="adminButton addinput" title="Add a Read only">Readonly</a>'
                       + '<a href="#" class="adminButton addinput" title="Add a Dropdown">Dropdown</a>'
                       + '<a href="#" class="adminButton addinput" title="Add a Radio Buttons">Radios Buttons</a>'
                       + '<a href="#" class="adminButton addinput" title="Add a Checkboxes">Checkboxes</a>'
                       + '<a href="#" class="adminButton addinput" title="Add a DatePicker">Datepicker</a>'
                       + '</div></div>'
                       +'<span class=buttons><a href="#" class="adminButton edit">Edit</a><a href="#" class="adminButton delete">Del</a></span>';
            editForm = '<form class="colapse"></form>';
            returnHtml = '<div class="group"><div class="groupHeader"><span class="title">Group - ' + labelName + '</span>' + controls + editForm + '</div><div class="groupControls"></div></div>';

            $(settings.location).last().append(returnHtml);
            
//            groupControlNode = $(settings.location).find('#groupControls')
//            alert(xmlToString($(settings.location).find('#groupControls').last()[0]));

          //  alert(xmlToString($(this)[0]));

            $(this).find('> *').each( function()
                {$(this) .xfeRenderControls({
                  location : $(settings.location).find('.groupControls')
                });
            });
           
        },

        xfeRenderControls: function(settings) {
         //   alert(xmlToString($(this)[0]));
            nodeType = $(this)[0].nodeName;
            if (nodeType == 'input'){
                $(this).xfeRenderSingleLine(settings)
            } else if (nodeType == 'textarea'){
                $(this).xfeRenderTextArea(settings)
            } else {
                $(settings.location).append('<div>'+ nodeType + '|' + $(this).attr('bind') + '</div>')
            }
            
           // 
        },

        xfeRenderSingleLine: function(settings) {
            label = $(this).find('label');
            labelName = "no label";
            if (label) {labelName = $(label).text();};

            controls = '<span class=buttons><a href="#" class="adminButton edit">Edit</a><a href="#" class="adminButton delete">Del</a></span>';
            editForm = '<form class="colapse">'
                        + '<fieldset><p>Appearance</p><ol>'
                        + '<li><label for="label">Label</label><input type="text" id="label"/></li>' 
                        + '</ol></fieldset>';
                        + '<fieldset><p>Bindings</p><ol>'
                        + '<li><label for="bind">Save To</label><select id="bind">' + bindOptions + '</select></li>' 
                        + '<li><label for="required">Required</label><select id="required">' + requiredOptions + '</select></li>' 
                        + '<li><label for="fn">Fieldname</label><input type="text" id="fn"/></li>' 
                        + '</ol></fieldset><input type="submit" name="ewSubmit" value="Save xForm" class="button principle" onclick="return(false);" />'
                        + '</form>';

            returnHtml = '<div class="control singleline"><span class="title">Single Line - ' + labelName + '</span>' + controls + editForm + '<div class="singleLineControls"></div></div>';
          
            $(settings.location).append(returnHtml)
            $(settings.location).find('.colapse').hide()
        },

        xfeRenderTextArea: function(settings) {
            label = $(this).find('label');
            labelName = "no label";
            if (label) {labelName = $(label).text();};

            controls = '<span class=buttons><a href="#" class="adminButton edit">Edit</a><a href="#" class="adminButton delete">Del</a></span>';
            editForm = '<form class="colapse"></form><div class="singleLineControls"></div>';
            returnHtml = '<div class="control singleline"><span class="title">Textarea - ' + labelName + '</span>' + controls + editForm + '</div>';
            
            $(settings.location).append(returnHtml)
        }



    });

})(jQuery);

function xmlToString(xmlData) {  
 
    var xmlString; 
    //IE 
    if (window.ActiveXObject){ 
        xmlString = xmlData.xml; 
    } 
    // code for Mozilla, Firefox, Opera, etc. 
    else{ 
        xmlString = (new XMLSerializer()).serializeToString(xmlData[0]); 
    } 
    return xmlString; 
} 
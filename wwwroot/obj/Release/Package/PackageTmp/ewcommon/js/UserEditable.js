// To handle user editable content via Ajax

function getContentForm(args) {

    var submitUrl = "/ewcommon/tools/ajaxContentForm.ashx?ajaxCmd=Edit"

    //alert(submitUrl);
    if (typeof args.targetId != 'undefined') {
        
        submitUrl += "&type=" + Url.encode(typeof args.type != 'undefined' ? args.type : "")
        submitUrl += "&pgid=" + Url.encode(typeof args.pgid != 'undefined' ? args.pgid : "")
        submitUrl += "&name=" + Url.encode(typeof args.name != 'undefined' ? args.name : "")
        submitUrl += "&id=" + Url.encode(typeof args.id != 'undefined' ? args.id : "")
        submitUrl += "&formName=" + Url.encode(typeof args.formName != 'undefined' ? args.formName : "")
        submitUrl += "&targetId=" + Url.encode(args.targetId)
        submitUrl += "&contentParId=" + Url.encode(typeof args.contentParId != 'undefined' ? args.contentParId : "")
        submitUrl += "&verId=" + Url.encode(typeof args.verId != 'undefined' ? args.verId : "")


        $('#' + args.targetId).html('<div class="loadingLine">Loading...</div>')


        $('#' + args.targetId).load(submitUrl, function () {
            $('#' + args.targetId).ajaxStop(function () {
                prepareAjaxForm(args.targetId, submitUrl);
            });
        });

    } 
    return false;

}


function getUserContentForm(formTargetId, contentType, contentName, pageId, contentId, formName, contentParId) {

    return getContentForm({ targetId: formTargetId, type: contentType, pgid: pageId, name: contentName, id: contentId, formName: formName, contentParId: contentParId });

}


function prepareAjaxForm(formTargetId, submitUrl) {

    // Unbind any previous submit events
    $('#' + formTargetId + ' form').unbind("submit");

    // Bind a new submit event
    $('#' + formTargetId + ' form').bind("submit", function () {
        $(this).ajaxSubmit(
            {
                
                target: '#' + formTargetId,
                url: submitUrl
            }
        );
        //$('#' + formTargetId).ajaxStop();
        // always return false to prevent standard browser submit and page navigation 
        return false;
    });
}

/**
*
*  URL encode / decode
*  http://www.webtoolkit.info/
*
**/

var Url = {

    // public method for url encoding
    encode: function (string) {
        return escape(this._utf8_encode(string));
    },

    // public method for url decoding
    decode: function (string) {
        return this._utf8_decode(unescape(string));
    },

    // private method for UTF-8 encoding
    _utf8_encode: function (string) {
        string = string.replace(/\r\n/g, "\n");
        var utftext = "";

        for (var n = 0; n < string.length; n++) {

            var c = string.charCodeAt(n);

            if (c < 128) {
                utftext += String.fromCharCode(c);
            }
            else if ((c > 127) && (c < 2048)) {
                utftext += String.fromCharCode((c >> 6) | 192);
                utftext += String.fromCharCode((c & 63) | 128);
            }
            else {
                utftext += String.fromCharCode((c >> 12) | 224);
                utftext += String.fromCharCode(((c >> 6) & 63) | 128);
                utftext += String.fromCharCode((c & 63) | 128);
            }

        }

        return utftext;
    },

    // private method for UTF-8 decoding
    _utf8_decode: function (utftext) {
        var string = "";
        var i = 0;
        var c = c1 = c2 = 0;

        while (i < utftext.length) {

            c = utftext.charCodeAt(i);

            if (c < 128) {
                string += String.fromCharCode(c);
                i++;
            }
            else if ((c > 191) && (c < 224)) {
                c2 = utftext.charCodeAt(i + 1);
                string += String.fromCharCode(((c & 31) << 6) | (c2 & 63));
                i += 2;
            }
            else {
                c2 = utftext.charCodeAt(i + 1);
                c3 = utftext.charCodeAt(i + 2);
                string += String.fromCharCode(((c & 15) << 12) | ((c2 & 63) << 6) | (c3 & 63));
                i += 3;
            }

        }

        return string;
    }

}

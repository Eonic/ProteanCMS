// Jquery implementation of Postcode Anywhere
// By Trevor Spink - Eonic Associates LLP. www.eonic.co.uk

// give the postcode input tag a class of getAddress
// give all address input tags you want to populate classes of 
// pcaDepartment
// pcaBuildingName
// pcaBuildingNumber
// pcaPrimaryStreet
// pcaPostTown
// pcaCounty
// pcaPostcode
// etc....

// warning only one control per page at the moment.

// REQUIRES JQUERY

// Add this code to the client page with account details in.
//$(document).ready(function () {
//    $('input.getAddress').each(function (i) {
//        $(this).pcaPopup({
//            acct: '',
//            key: ''
//         });
//    });
//});
// or run this code after loading by ajax
//$('input.getAddress').each(function (i) {
//    $(this).pcaPopup({
//        acct: '',
//        key: ''
//    });
//});

(function ($) {
    $.fn.pcaPopup = function (options) {
        // setting the defaults

        var defaults = {
            acct: '',
            key: ''
        };

        var options = $.extend(defaults, options);

        // and the plugin begins
        return this.each(function () {
            var obj, text, wordcount, limited;

            obj = $(this);

            obj.after('<input type="button" value="Find Address" class="pcaButton"/><select id="pcaBuilding" style="display:none;"/>');
            obj.parent().find('.pcaButton').click(function () {
               // obj.parent().find('#pcaBuilding option').remove();
                PostcodeAnywhere_Interactive_Find_v1_10Begin(options.key, obj.val(), 'English', '', options.acct);
                obj.parent().find('#pcaBuilding').attr('size', '10');
                obj.parent().find('#pcaBuilding').attr('style', '');
            });
            obj.parent().find('#pcaBuilding').change(function () {
                PostcodeAnywhere_Interactive_RetrieveById_v1_30Begin(options.key, $(this).val(), 'English', '', options.acct)
                //reset the control
                $(this).attr('size', '1');
            });
        });
    };
})(jQuery);


function PostcodeAnywhere_Interactive_Find_v1_10Begin(Key, SearchTerm, PreferredLanguage, Filter, UserName) {
    var script = document.createElement("script"),
        head = document.getElementsByTagName("head")[0],
        url = "//services.postcodeanywhere.co.uk/PostcodeAnywhere/Interactive/Find/v1.10/json2.ws?";
    // Build the query string
   url += "&Key=" + encodeURIComponent(Key);
   url += "&SearchTerm=" + encodeURIComponent(SearchTerm);
   url += "&PreferredLanguage=" + encodeURIComponent(PreferredLanguage);
   url += "&Filter=" + encodeURIComponent(Filter);
   url += "&UserName=" + encodeURIComponent(UserName);
   url += "&CallbackFunction=PostcodeAnywhere_Interactive_Find_v1_10End";
   script.src = url;
   // Make the request
   script.onload = script.onreadystatechange = function () {
       if (!this.readyState || this.readyState === "loaded" || this.readyState === "complete") {
          script.onload = script.onreadystatechange = null;
           if (head && script.parentNode)
              head.removeChild(script);
       }
   }

   head.insertBefore(script, head.firstChild);
}

function PostcodeAnywhere_Interactive_Find_v1_10End(response) {
    // Test for an error
    if (response.length == 1 && typeof(response[0].Error) != "undefined") {
       // Show the error message
        alert(response[0].Description);
   }
    else {
        // Check if there were any items found
       if (response.length == 0)
            alert("Sorry, there were no results");
       else {
       
       $('#pcaBuilding option').remove();

       document.getElementById("pcaBuilding").style.display = '';
           for (var i=0;i<=response.length-1;i++){
      			var opt = document.createElement("option");
                document.getElementById("pcaBuilding").options.add(opt);
                opt.text = response[i].StreetAddress + ", " + response[i].Place;
                opt.value = response[i].Id;
               }
        }
    }
}

function PostcodeAnywhere_Interactive_RetrieveById_v1_30Begin(Key, Id, PreferredLanguage, UserName) {
    var script = document.createElement("script"),
        head = document.getElementsByTagName("head")[0],
        url = "//services.postcodeanywhere.co.uk/PostcodeAnywhere/Interactive/RetrieveById/v1.30/json2.ws?";
    // Build the query string
    url += "&Key=" + encodeURIComponent(Key);
    url += "&Id=" + encodeURIComponent(Id);
    url += "&PreferredLanguage=" + encodeURIComponent(PreferredLanguage);
    url += "&UserName=" + encodeURIComponent(UserName);
   url += "&CallbackFunction=PostcodeAnywhere_Interactive_RetrieveById_v1_30End";
    script.src = url;
    // Make the request
   script.onload = script.onreadystatechange = function () {
        if (!this.readyState || this.readyState === "loaded" || this.readyState === "complete") {
            script.onload = script.onreadystatechange = null;
            if (head && script.parentNode)
               head.removeChild(script);
     }
    }
  head.insertBefore(script, head.firstChild);
}

function PostcodeAnywhere_Interactive_RetrieveById_v1_30End(response) {
   // Test for an error
    if (response.length == 1 && typeof(response[0].Error) != "undefined") {
        // Show the error message
       alert(response[0].Description);
    }
    else {
     // Check if there were any items found
       if (response.length == 0)
            alert("Sorry, there were no results");
    else {

        $('.pcaCompany').val(response[0].Company);
        $('.pcaDepartment').val(response[0].Department);
        $('.pcaBuildingName').val(response[0].BuildingName);
        $('.pcaBuildingNumber').val(response[0].BuildingNumber);
        $('.pcaPrimaryStreet').val(response[0].PrimaryStreet);
        $('.pcaPrimaryNameNumberStreet').val(response[0].BuildingName + ' ' + response[0].BuildingNumber + ' ' + response[0].PrimaryStreet);
        $('.pcaPostTown').val(response[0].PostTown);
        $('.pcaCounty').val(response[0].County);
        $('.pcaPostcode').val(response[0].Postcode);

        // add additional returned fields as required, being lazy, these are only the ones I needed.

       }
   }
}


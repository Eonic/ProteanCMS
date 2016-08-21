/* 
    EONIC LTD - Created 2010-11-19
    AUTHORS: WILL HANCOCK 

    Integration with PostCode Anywhere - Web Service
    More Info - http://www.postcodeanywhere.co.uk/support/webservices/PostcodeAnywhereInternational/Interactive/RetrieveByPostalCode/v1.1/default.aspx

*/

function PostcodeAnywhere_Interactive_RetrieveByPostcodeAndBuilding_v1_10Begin(Key, Postcode, Building, UserName) {
    var scriptTag = document.getElementById("PCA38d38252878f434581f85b249661cd94");
    var headTag = document.getElementsByTagName("head").item(0);
    var strUrl = "";
    
    //Build the url
    strUrl = "http://services.postcodeanywhere.co.uk/PostcodeAnywhere/Interactive/RetrieveByPostcodeAndBuilding/v1.10/json.ws?";
    strUrl += "&Key=" + escape(Key);
    strUrl += "&Postcode=" + escape(Postcode);
    strUrl += "&Building=" + escape(Building);
    strUrl += "&UserName=" + escape(UserName);
    strUrl += "&CallbackFunction=PostcodeAnywhere_Interactive_RetrieveByPostcodeAndBuilding_v1_10End";

    //Make the request
    if (scriptTag) {
        try {
            headTag.removeChild(scriptTag);
        }
        catch (e) {
            //Ignore
        }
    }
    scriptTag = document.createElement("script");
    scriptTag.src = strUrl
    scriptTag.type = "text/javascript";
    scriptTag.id = "PCA38d38252878f434581f85b249661cd94";
    headTag.appendChild(scriptTag);

    // EONIC - WH=======================================================

    $(".getAddress").addClass('loading');


    // /EONIC - WH=======================================================

}

function PostcodeAnywhere_Interactive_RetrieveByPostcodeAndBuilding_v1_10End(response) {
    //Test for an error
    if (response.length == 1 && typeof (response[0].Error) != 'undefined') {
        //Show the error message
        alert('error' + response[0].Description);
        
    }
    else {
    
        //Check if there were any items found
        if (response.length == 0) {
            alert("Sorry, no matching items found");
        }
        else {
          
            //PUT YOUR CODE HERE
            //FYI: The output is an array of key value pairs (e.g. response[0].Udprn), the keys being:
            //Udprn
            //Company
            //Department
            //Line1
            //Line2
            //Line3
            //Line4
            //Line5
            //PostTown
            //County
            //Postcode
            //Mailsort
            //Barcode
            //Type
            //DeliveryPointSuffix
            //SubBuilding
            //BuildingName
            //BuildingNumber
            //PrimaryStreet
            //SecondaryStreet
            //DoubleDependentLocality
            //DependentLocality
            //PoBox
            //CountryName

            // Got results, lets go do something with them
            addressHandler(response);
            
            
            /* / EONIC CODE GOES HERE */
        }
    }
}



// EONIC CODE
function addressHandler(response) {
//response.length
    for (var i = 0; i <= 5; i++) {

        var responseString = response[i].Udprn + ', ' + response[i].Company + ', ' + response[i].PostTown + ', ' + response[i].BuildingNumber + ', ' + response[i].CountryName + ', ';
        alert(responseString);
    }



    $(".getAddress").addClass('loading');
}

/* 
	CurrencyConverter class
	
	v.1.0 - Ali Granger - 2007-09-13
	
	This will convert (e.g.) 
	
		$ <SPAN class="someclass">1000.00</SPAN>  
		
		to  $ <SPAN class="someclass">1000.00</SPAN> <SPAN class="price_converted">(Approx. £ 2000.00)</SPAN>
	 
	Prices should be identified by tag name and tag class.
	
	Function relies on /ewcommon/tools/ajax_proxy.asp

	Call this with the convertCurrencies function.
*/

// ================ START CLASS ================
function CurrencyConverter(from,to,tag,classP,toSymbol,footerId) {

	this._validCurrencies = false;
	this._validConversionRates = false;

	this.setCurrencies(from,to); 
	this._matchTag = tag.toString();
	this._matchClass = classP.toString();
	this._toCurrencySymbol = toSymbol.toString();
	this._addFooterObjectId = footerId.toString();
	

};

// CurrencyConverter Properties
CurrencyConverter.prototype._fromCurrency;			// The 3-letter country code, converting FROM (e.g. "USD")
CurrencyConverter.prototype._toCurrency;			// The 3-letter country code, converting TO (e.g. "GBP")
CurrencyConverter.prototype._matchTag;				// The tag that the price will be found in (e.g. "SPAN")
CurrencyConverter.prototype._matchClass;			// The class of the tag that the price will be found in.  Optional.
CurrencyConverter.prototype._toCurrencySymbol;		// The symbol for the currency that we are converting to.
CurrencyConverter.prototype._addFooterObjectId;		// The ID of the element that you want to add the disclaimer to. Optional
CurrencyConverter.prototype._exchangeRate;			// The exchange rate that gets calculated.
CurrencyConverter.prototype._xmlResponse;			// The xml response of the HTTP Req (as text)
CurrencyConverter.prototype._validCurrencies;		// Are the currencies given valid?
CurrencyConverter.prototype._validConversionRates;  // Are the conrsionRates found valid?

// CurrencyConverter Methods

// setCurrencies
// Adds validity checks
CurrencyConverter.prototype.setCurrencies = function(from,to) {
	this._fromCurrency = from.toString().toUpperCase();
	this._toCurrency = to.toString().toUpperCase();
	if (this._fromCurrency.length==3 && this._toCurrency.length==3) {this._validCurrencies = true;}
	return;
}

// calculateExchangeDate
// xmlDoc is a text listing of the XML feed from ECB
CurrencyConverter.prototype.calculateExchangeRate = function(xmlDoc) {

	this._validConversionRates = false;
	this._xmlResponse = xmlDoc;
	
	var fromNode, toNode, fromRate, toRate, exchangeRate,cPattern, re, matches
	
	// Process the Response - wanted to do this as XMl, but selectsinglenode wasn't working, so RE
	if (this._fromCurrency=='EUR') {
	    fromRate = 1;
	}
	else {
	    cPattern = "currency='" + this._fromCurrency + "' rate='([\\d\\.]+)'";
	    re = new RegExp(cPattern,"i");
	    matches = re.exec(this._xmlResponse)
	    fromRate = parseFloat(matches[1]);
	}

	if (this._toCurrency=='EUR') {	
	    toRate = 1;
	}
	else {
	    cPattern = "currency='" + this._toCurrency + "' rate='([\\d\\.]+)'";
	    re = new RegExp(cPattern,"gi");
	    matches = re.exec(this._xmlResponse)
	    toRate = parseFloat(matches[1]);
	}
	
	if(fromRate>0 && toRate>0) {
		this._exchangeRate = toRate / fromRate;
		this._validConversionRates = true;
	}
	return;
}

// convertPrices
// Find prices on page, and add conversion
CurrencyConverter.prototype.convertPrices = function() {
	if(this._validConversionRates) {
	
		var convPrice,newHTML,i;
	
		// Go and update the site
		var e=document.getElementsByTagName(this._matchTag);
		
		// Update individual prices
		for(i=0;i<e.length;i++){
		
			// Check if the class matches (or is blank)
			if (this._matchClass=='' || this._matchClass==e[i].className) {
				convPrice = parseFloat(e[i].innerHTML) * this._exchangeRate
				convHTML = "<span class=\"price_converted\">(Approx. "
				convHTML += this._toCurrencySymbol + " " 
				convHTML += convPrice.toFixed(2) + " <span class=\"price_convert_footnote\">&dagger;</span>)</span>" 
				e[i].parentNode.innerHTML += convHTML
			}
		}
		
		// Add a footnote - if an ID exists.
		if(e.length>0 && this._addFooterObjectId!='') {
			var f,fHTML,fTime
			
			f = document.createElement('p');
			
			var re = /time='([\d-]+)'/i;
			var matches = re.exec(this._xmlResponse);
			fTime = matches[1];
	
			fHTML = "&dagger; Automatic currency conversion from "
			fHTML += this._fromCurrency + " to " + this._toCurrency
			fHTML += " are a <strong>guideline only</strong>, and may not reflect your "
			fHTML += " bank's actual conversion.  Exchange rates used are updated daily by European Central Bank. Last update: " + fTime
			
			f.className = "price_convert_disclaimer";
			f.innerHTML = fHTML;
			
			if(document.getElementById(this._addFooterObjectId)){document.getElementById(this._addFooterObjectId).appendChild(f);}
		}
		
	}
}
// ================ END CLASS ================


// ================ Code to call ================

var xmlHttp, ccGlobal;

function convertCurrencies(from,to,tag,classP,toSymbol,footerId)
{ 

	xmlHttp=GetXmlHttpObject();
	if (xmlHttp==null)
	{
		// No browser support for AJAX - don't run anything else. 
		return;
	}
	
	// AJAX is supported - crated the global CC object
	ccGlobal = new CurrencyConverter(from,to,tag,classP,toSymbol,footerId)

	// Firefox restricts AJAX calls to anything other than same domain, so use proxy
	if (ccGlobal._validCurrencies) {
		var url="http://" + location.host + "/ewcommon/tools/ajax_proxy.asp?feed=currency_converter";
		xmlHttp.onreadystatechange=currencyStateChanged;
		xmlHttp.open("GET",url,true);
		xmlHttp.send(null);
	}
}

function currencyStateChanged() 
{ 
	if (xmlHttp.readyState==4)
	{
		
		ccGlobal.calculateExchangeRate(xmlHttp.responseText);
		ccGlobal.convertPrices();
	}
}

function GetXmlHttpObject()
{
	var xmlHttp=null;
	
	try {
		netscape.security.PrivilegeManager.enablePrivilege("UniversalBrowserRead");
	} catch (e) {
		
	}
	

	
	try {xmlHttp=new XMLHttpRequest();} // Firefox, Opera 8.0+, Safari
	catch (e)
	{
		try{xmlHttp=new ActiveXObject("Msxml2.XMLHTTP");}// Internet Explorer
		catch (e){xmlHttp=new ActiveXObject("Microsoft.XMLHTTP");}// Internet Explorer
	}

	return xmlHttp;
}



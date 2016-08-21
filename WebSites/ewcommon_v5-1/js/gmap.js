var localSearch = new GlocalSearch();
var map;

var icon = new GIcon();
icon.image = "http://www.google.com/mapfiles/marker.png";
icon.shadow = "http://www.google.com/mapfiles/shadow50.png";
icon.iconSize = new GSize(20, 34);
icon.shadowSize = new GSize(37, 34);
icon.iconAnchor = new GPoint(10, 34);
icon.infoWindowAnchor = new GPoint(9, 2);
icon.infoShadowAnchor = new GPoint(18, 25);
        
function usePointFromPostcode(postcode, callbackFunction) {
	localSearch.setSearchCompleteCallback(null, 
		function() {
			
			if (localSearch.results[0])
			{		
				var resultLat = localSearch.results[0].lat;
				var resultLng = localSearch.results[0].lng;
				var point = new GLatLng(resultLat,resultLng);
				callbackFunction(point);
			}else{
				alert("Postcode not found!");
			}
		});	
	localSearch.execute(postcode + ", UK");
}

function mapLoadAtPoint(point) {
	if (GBrowserIsCompatible()) {
		//map.addControl(new GLargeMapControl());
		//map.addControl(new GMapTypeControl());
		map.setCenter(point, gMapZoom, gMapView);
	}
}

function placeMarkerAtPoint(point)
{
	var marker = new GMarker(point,icon);
	map.addOverlay(marker);
}

function setCenterToPoint(point)
{
	map.setCenter(point, gMapZoom);
}

function showPointLatLng(point)
{
	alert("Latitude: " + point.lat() + "\nLongitude: " + point.lng());
}

function showMarker(point,mapWindow)
{
        var marker = new GMarker(point);
		map.addOverlay(marker);
		marker.openInfoWindowHtml(document.getElementById(mapWindow));
}			      		


function showPoint(point)
{
        var marker = new GMarker(point);       
		map.addOverlay(marker);
}	

function createMarker(point, gMapIdx) {
          // Create a lettered icon for this point using our icon class
          
          var letter = String.fromCharCode("A".charCodeAt(0) + gMapIdx);
          var letteredIcon = new GIcon(icon);
          letteredIcon.image = "http://www.google.com/mapfiles/marker" + letter + ".png";
          // Set up our GMarkerOptions object
          markerOptions = { icon:letteredIcon };
          var marker = new GMarker(point, markerOptions);

          GEvent.addListener(marker, "click", function() {
            marker.openInfoWindowHtml(document.getElementById("mapWindow_"+ gMapIdx));
          });
         map.addOverlay(marker);
        }
        
function addLoadEvent(func) {
  var oldonload = window.onload;
  if (typeof window.onload != 'function') {
    window.onload = func;
  } else {
    window.onload = function() {
      oldonload();
      func();
    }
  }
}

function addUnLoadEvent(func) {
	var oldonunload = window.onunload;
	if (typeof window.onunload != 'function') {
	  window.onunload = func;
	} else {
	  window.onunload = function() {
	    oldonunload();
	    func();
	  }
	}
}


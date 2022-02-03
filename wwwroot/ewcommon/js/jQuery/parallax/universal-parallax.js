'use strict';

/**
* @version 1.2.1
* @author Marius Hansen <marius.o.hansen@gmail.com>
* @license MIT
* @description Easy parallax plugin using pure javascript. Cross browser support, including mobile platforms. Based on goodparallax
* @copyright © Marius Hansen 2018
*/

var windowHeight = window.innerHeight,
    windowHeightExtra = 0;
var safari = /^((?!chrome|android).)*safari/i.test(navigator.userAgent),
    mobile = /Mobi/.test(navigator.userAgent);

// check if safari - extend height
if (safari && !mobile) {
	windowHeightExtra = window.outerHeight - window.innerHeight;
}

if (mobile) {
	windowHeight = window.screen.availHeight; // stops from jumping
	windowHeightExtra = (window.screen.availHeight - window.innerHeight) / 2; // prevents white spaces
}

// position parallax
function positionParallax(container, speed, parallax, elem) {
	var bgScroll = container.top / speed - windowHeightExtra;
	parallax[elem].style.top = bgScroll + 'px';
}

// animate parallax
function animateParallax(parallax, speed) {
	for (var i = 0; parallax.length > i; i++) {
		var container = parallax[i].parentElement.parentElement.getBoundingClientRect();

		if (container.top + container.height >= 0 && container.top <= windowHeight) {
			positionParallax(container, speed, parallax, i);
		}
	}
}

// determine height
function calculateHeight(parallax, speed) {
	for (var i = 0; parallax.length > i; i++) {
		var container = parallax[i].parentElement.parentElement.getBoundingClientRect();
		var elemOffsetTop = (windowHeight - container.height) / 2;
		var bgHeight = container.height + (elemOffsetTop - elemOffsetTop / speed) * 2;

		parallax[i].style.height = bgHeight + windowHeightExtra * 2 + 'px';

		positionParallax(container, speed, parallax, i);
	}
}

 function supportsWebp() {
	 var elem = document.createElement('canvas');
	 if (!!(elem.getContext && elem.getContext('2d'))) {
		 // was able or not to get WebP representation
		 return elem.toDataURL('image/webp').indexOf('data:image/webp') == 0;
	 }
	 // very old browser like IE 8, canvas not supported
	 return false;
}

var universalParallax = function universalParallax() {
	var up = function up(parallax, speed) {

		// check that speed is not a negative number
		if (speed < 1) {
			speed = 1;
		}

		// set height on elements
		calculateHeight(parallax, speed);

		// recalculate height on resize
		if (!mobile) {
			window.addEventListener("resize", function () {
				windowHeight = window.innerHeight;
				calculateHeight(parallax, speed);
			});
		}

		// Add scroll event listener
		window.addEventListener("scroll", function () {
			animateParallax(parallax, speed);
		});
	};

	// Ready all elements
	this.init = function (param) {
		if (typeof param === 'undefined') {
			param = {};
		}

		param = {
			speed: typeof param.speed !== 'undefined' ? param.speed : 1.5,
			className: typeof param.className !== 'undefined' ? param.className : 'parallax'
		};
		//var parallax = document.getElementsByClassName(param.className);
        var parallax = $('*:not(.parallax__container) > .parallax');

		for (var i = 0; parallax.length > i; i++) {
			// make container div
			var wrapper = document.createElement('div');
			parallax[i].parentNode.insertBefore(wrapper, parallax[i]);
			wrapper.appendChild(parallax[i]);
			var parallaxContainer = parallax[i].parentElement;
			parallaxContainer.className += 'parallax__container';

			// // parent elem need position: relative for effect to work - if not already defined, add it
			if (window.getComputedStyle(parallaxContainer.parentElement, null).getPropertyValue('position') !== 'relative') {
				parallaxContainer.parentElement.style.position = 'relative';
			}
			var imgData = parallax[i].dataset.parallaxImage;
			var imgDataWebp = parallax[i].dataset.parallaxImageWebp
			var windowWidth = window.innerWidth	|| document.documentElement.clientWidth	|| document.body.clientWidth;;
			if (windowWidth < 575) {
				imgData = parallax[i].dataset.parallaxImageXs
				imgDataWebp = parallax[i].dataset.parallaxImageXsWebp
			};
			if (windowWidth >= 575 && windowWidth < 768 ) {
				imgData = parallax[i].dataset.parallaxImageSm
				imgDataWebp = parallax[i].dataset.parallaxImageSmWebp
			};
			if (windowWidth >= 768 && windowWidth < 992) {
				imgData = parallax[i].dataset.parallaxImageMd
				imgDataWebp = parallax[i].dataset.parallaxImageMdWebp
			};
			if (windowWidth >= 992 && windowWidth < 1200) {
				imgData = parallax[i].dataset.parallaxImageLg
				imgDataWebp = parallax[i].dataset.parallaxImageLgWebp
			};
			if (windowWidth > 1920) {
				imgData = parallax[i].dataset.parallaxImageXxl
				imgDataWebp = parallax[i].dataset.parallaxImageXxlWebp
			};
			
			if (supportsWebp()) {
				imgData = imgDataWebp;
			};
			//debugger;
			// add image to div if none is specified
			if (typeof imgData !== 'undefined') {
				var newImg = new Image();
				newImg.src = imgData;
				newImg.onload = function () {
					var curHeight = newImg.height;
					var curWidth = newImg.width;

					parallax[i].style.backgroundImage = 'url(' + imgData + ')';
					// if no other class than .parallax is specified, add CSS
					if (parallax[i].classList.length === 1 && parallax[i].classList[0] === 'parallax') {
						Object.assign(parallax[i].style, {
							'background-repeat': 'no-repeat',
							'background-position': 'center',
							'background-size': 'cover'
						});
						Object.assign(parallax[i].parentNode.parentNode.style, {
							'height': curHeight + 'px',
						});
					};
					// when init completed, run function
					up(parallax, param.speed);
				};
			}
		};

	};
};

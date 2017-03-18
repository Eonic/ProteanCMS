/*
 *  Bootstrap Color Picker Sliders - v3.0.1
 *
 *  Bootstrap 3 optimized responsive color selector with HSV, HSL, RGB and CIE-Lch (which supports human perceived lightness) selectors and color swatches.
 *  http://www.virtuosoft.eu/code/bootstrap-colorpickersliders/
 *
 *  Made by István Ujj-Mészáros
 *  Under Apache License v2.0 License
 *
 *  Requirements:  *
 *      TinyColor: https://github.com/bgrins/TinyColor/




 *
 *  Using color math algorithms from EasyRGB Web site:/
 *      http://www.easyrgb.com/index.php?X=MATH */

// TinyColor v1.1.2
// https://github.com/bgrins/TinyColor
// Brian Grinstead, MIT License

(function () {

    var trimLeft = /^[\s,#]+/,
        trimRight = /\s+$/,
        tinyCounter = 0,
        math = Math,
        mathRound = math.round,
        mathMin = math.min,
        mathMax = math.max,
        mathRandom = math.random;

    function tinycolor(color, opts) {

        color = (color) ? color : '';
        opts = opts || {};

        // If input is already a tinycolor, return itself
        if (color instanceof tinycolor) {
            return color;
        }
        // If we are called as a function, call using new instead
        if (!(this instanceof tinycolor)) {
            return new tinycolor(color, opts);
        }

        var rgb = inputToRGB(color);
        this._originalInput = color,
        this._r = rgb.r,
        this._g = rgb.g,
        this._b = rgb.b,
        this._a = rgb.a,
        this._roundA = mathRound(100 * this._a) / 100,
        this._format = opts.format || rgb.format;
        this._gradientType = opts.gradientType;

        // Don't let the range of [0,255] come back in [0,1].
        // Potentially lose a little bit of precision here, but will fix issues where
        // .5 gets interpreted as half of the total, instead of half of 1
        // If it was supposed to be 128, this was already taken care of by `inputToRgb`
        if (this._r < 1) { this._r = mathRound(this._r); }
        if (this._g < 1) { this._g = mathRound(this._g); }
        if (this._b < 1) { this._b = mathRound(this._b); }

        this._ok = rgb.ok;
        this._tc_id = tinyCounter++;
    }

    tinycolor.prototype = {
        isDark: function () {
            return this.getBrightness() < 128;
        },
        isLight: function () {
            return !this.isDark();
        },
        isValid: function () {
            return this._ok;
        },
        getOriginalInput: function () {
            return this._originalInput;
        },
        getFormat: function () {
            return this._format;
        },
        getAlpha: function () {
            return this._a;
        },
        getBrightness: function () {
            //http://www.w3.org/TR/AERT#color-contrast
            var rgb = this.toRgb();
            return (rgb.r * 299 + rgb.g * 587 + rgb.b * 114) / 1000;
        },
        getLuminance: function () {
            //http://www.w3.org/TR/2008/REC-WCAG20-20081211/#relativeluminancedef
            var rgb = this.toRgb();
            var RsRGB, GsRGB, BsRGB, R, G, B;
            RsRGB = rgb.r / 255;
            GsRGB = rgb.g / 255;
            BsRGB = rgb.b / 255;

            if (RsRGB <= 0.03928) { R = RsRGB / 12.92; } else { R = Math.pow(((RsRGB + 0.055) / 1.055), 2.4); }
            if (GsRGB <= 0.03928) { G = GsRGB / 12.92; } else { G = Math.pow(((GsRGB + 0.055) / 1.055), 2.4); }
            if (BsRGB <= 0.03928) { B = BsRGB / 12.92; } else { B = Math.pow(((BsRGB + 0.055) / 1.055), 2.4); }
            return (0.2126 * R) + (0.7152 * G) + (0.0722 * B);
        },
        setAlpha: function (value) {
            this._a = boundAlpha(value);
            this._roundA = mathRound(100 * this._a) / 100;
            return this;
        },
        toHsv: function () {
            var hsv = rgbToHsv(this._r, this._g, this._b);
            return { h: hsv.h * 360, s: hsv.s, v: hsv.v, a: this._a };
        },
        toHsvString: function () {
            var hsv = rgbToHsv(this._r, this._g, this._b);
            var h = mathRound(hsv.h * 360), s = mathRound(hsv.s * 100), v = mathRound(hsv.v * 100);
            return (this._a == 1) ?
              "hsv(" + h + ", " + s + "%, " + v + "%)" :
              "hsva(" + h + ", " + s + "%, " + v + "%, " + this._roundA + ")";
        },
        toHsl: function () {
            var hsl = rgbToHsl(this._r, this._g, this._b);
            return { h: hsl.h * 360, s: hsl.s, l: hsl.l, a: this._a };
        },
        toHslString: function () {
            var hsl = rgbToHsl(this._r, this._g, this._b);
            var h = mathRound(hsl.h * 360), s = mathRound(hsl.s * 100), l = mathRound(hsl.l * 100);
            return (this._a == 1) ?
              "hsl(" + h + ", " + s + "%, " + l + "%)" :
              "hsla(" + h + ", " + s + "%, " + l + "%, " + this._roundA + ")";
        },
        toHex: function (allow3Char) {
            return rgbToHex(this._r, this._g, this._b, allow3Char);
        },
        toHexString: function (allow3Char) {
            return '#' + this.toHex(allow3Char);
        },
        toHex8: function () {
            return rgbaToHex(this._r, this._g, this._b, this._a);
        },
        toHex8String: function () {
            return '#' + this.toHex8();
        },
        toRgb: function () {
            return { r: mathRound(this._r), g: mathRound(this._g), b: mathRound(this._b), a: this._a };
        },
        toRgbString: function () {
            return (this._a == 1) ?
              "rgb(" + mathRound(this._r) + ", " + mathRound(this._g) + ", " + mathRound(this._b) + ")" :
              "rgba(" + mathRound(this._r) + ", " + mathRound(this._g) + ", " + mathRound(this._b) + ", " + this._roundA + ")";
        },
        toPercentageRgb: function () {
            return { r: mathRound(bound01(this._r, 255) * 100) + "%", g: mathRound(bound01(this._g, 255) * 100) + "%", b: mathRound(bound01(this._b, 255) * 100) + "%", a: this._a };
        },
        toPercentageRgbString: function () {
            return (this._a == 1) ?
              "rgb(" + mathRound(bound01(this._r, 255) * 100) + "%, " + mathRound(bound01(this._g, 255) * 100) + "%, " + mathRound(bound01(this._b, 255) * 100) + "%)" :
              "rgba(" + mathRound(bound01(this._r, 255) * 100) + "%, " + mathRound(bound01(this._g, 255) * 100) + "%, " + mathRound(bound01(this._b, 255) * 100) + "%, " + this._roundA + ")";
        },
        toName: function () {
            if (this._a === 0) {
                return "transparent";
            }

            if (this._a < 1) {
                return false;
            }

            return hexNames[rgbToHex(this._r, this._g, this._b, true)] || false;
        },
        toFilter: function (secondColor) {
            var hex8String = '#' + rgbaToHex(this._r, this._g, this._b, this._a);
            var secondHex8String = hex8String;
            var gradientType = this._gradientType ? "GradientType = 1, " : "";

            if (secondColor) {
                var s = tinycolor(secondColor);
                secondHex8String = s.toHex8String();
            }

            return "progid:DXImageTransform.Microsoft.gradient(" + gradientType + "startColorstr=" + hex8String + ",endColorstr=" + secondHex8String + ")";
        },
        toString: function (format) {
            var formatSet = !!format;
            format = format || this._format;

            var formattedString = false;
            var hasAlpha = this._a < 1 && this._a >= 0;
            var needsAlphaFormat = !formatSet && hasAlpha && (format === "hex" || format === "hex6" || format === "hex3" || format === "name");

            if (needsAlphaFormat) {
                // Special case for "transparent", all other non-alpha formats
                // will return rgba when there is transparency.
                if (format === "name" && this._a === 0) {
                    return this.toName();
                }
                return this.toRgbString();
            }
            if (format === "rgb") {
                formattedString = this.toRgbString();
            }
            if (format === "prgb") {
                formattedString = this.toPercentageRgbString();
            }
            if (format === "hex" || format === "hex6") {
                formattedString = this.toHexString();
            }
            if (format === "hex3") {
                formattedString = this.toHexString(true);
            }
            if (format === "hex8") {
                formattedString = this.toHex8String();
            }
            if (format === "name") {
                formattedString = this.toName();
            }
            if (format === "hsl") {
                formattedString = this.toHslString();
            }
            if (format === "hsv") {
                formattedString = this.toHsvString();
            }

            return formattedString || this.toHexString();
        },

        _applyModification: function (fn, args) {
            var color = fn.apply(null, [this].concat([].slice.call(args)));
            this._r = color._r;
            this._g = color._g;
            this._b = color._b;
            this.setAlpha(color._a);
            return this;
        },
        lighten: function () {
            return this._applyModification(lighten, arguments);
        },
        brighten: function () {
            return this._applyModification(brighten, arguments);
        },
        darken: function () {
            return this._applyModification(darken, arguments);
        },
        desaturate: function () {
            return this._applyModification(desaturate, arguments);
        },
        saturate: function () {
            return this._applyModification(saturate, arguments);
        },
        greyscale: function () {
            return this._applyModification(greyscale, arguments);
        },
        spin: function () {
            return this._applyModification(spin, arguments);
        },

        _applyCombination: function (fn, args) {
            return fn.apply(null, [this].concat([].slice.call(args)));
        },
        analogous: function () {
            return this._applyCombination(analogous, arguments);
        },
        complement: function () {
            return this._applyCombination(complement, arguments);
        },
        monochromatic: function () {
            return this._applyCombination(monochromatic, arguments);
        },
        splitcomplement: function () {
            return this._applyCombination(splitcomplement, arguments);
        },
        triad: function () {
            return this._applyCombination(triad, arguments);
        },
        tetrad: function () {
            return this._applyCombination(tetrad, arguments);
        }
    };

    // If input is an object, force 1 into "1.0" to handle ratios properly
    // String input requires "1.0" as input, so 1 will be treated as 1
    tinycolor.fromRatio = function (color, opts) {
        if (typeof color == "object") {
            var newColor = {};
            for (var i in color) {
                if (color.hasOwnProperty(i)) {
                    if (i === "a") {
                        newColor[i] = color[i];
                    }
                    else {
                        newColor[i] = convertToPercentage(color[i]);
                    }
                }
            }
            color = newColor;
        }

        return tinycolor(color, opts);
    };

    // Given a string or object, convert that input to RGB
    // Possible string inputs:
    //
    //     "red"
    //     "#f00" or "f00"
    //     "#ff0000" or "ff0000"
    //     "#ff000000" or "ff000000"
    //     "rgb 255 0 0" or "rgb (255, 0, 0)"
    //     "rgb 1.0 0 0" or "rgb (1, 0, 0)"
    //     "rgba (255, 0, 0, 1)" or "rgba 255, 0, 0, 1"
    //     "rgba (1.0, 0, 0, 1)" or "rgba 1.0, 0, 0, 1"
    //     "hsl(0, 100%, 50%)" or "hsl 0 100% 50%"
    //     "hsla(0, 100%, 50%, 1)" or "hsla 0 100% 50%, 1"
    //     "hsv(0, 100%, 100%)" or "hsv 0 100% 100%"
    //
    function inputToRGB(color) {

        var rgb = { r: 0, g: 0, b: 0 };
        var a = 1;
        var ok = false;
        var format = false;

        if (typeof color == "string") {
            color = stringInputToObject(color);
        }

        if (typeof color == "object") {
            if (color.hasOwnProperty("r") && color.hasOwnProperty("g") && color.hasOwnProperty("b")) {
                rgb = rgbToRgb(color.r, color.g, color.b);
                ok = true;
                format = String(color.r).substr(-1) === "%" ? "prgb" : "rgb";
            }
            else if (color.hasOwnProperty("h") && color.hasOwnProperty("s") && color.hasOwnProperty("v")) {
                color.s = convertToPercentage(color.s);
                color.v = convertToPercentage(color.v);
                rgb = hsvToRgb(color.h, color.s, color.v);
                ok = true;
                format = "hsv";
            }
            else if (color.hasOwnProperty("h") && color.hasOwnProperty("s") && color.hasOwnProperty("l")) {
                color.s = convertToPercentage(color.s);
                color.l = convertToPercentage(color.l);
                rgb = hslToRgb(color.h, color.s, color.l);
                ok = true;
                format = "hsl";
            }

            if (color.hasOwnProperty("a")) {
                a = color.a;
            }
        }

        a = boundAlpha(a);

        return {
            ok: ok,
            format: color.format || format,
            r: mathMin(255, mathMax(rgb.r, 0)),
            g: mathMin(255, mathMax(rgb.g, 0)),
            b: mathMin(255, mathMax(rgb.b, 0)),
            a: a
        };
    }


    // Conversion Functions
    // --------------------

    // `rgbToHsl`, `rgbToHsv`, `hslToRgb`, `hsvToRgb` modified from:
    // <http://mjijackson.com/2008/02/rgb-to-hsl-and-rgb-to-hsv-color-model-conversion-algorithms-in-javascript>

    // `rgbToRgb`
    // Handle bounds / percentage checking to conform to CSS color spec
    // <http://www.w3.org/TR/css3-color/>
    // *Assumes:* r, g, b in [0, 255] or [0, 1]
    // *Returns:* { r, g, b } in [0, 255]
    function rgbToRgb(r, g, b) {
        return {
            r: bound01(r, 255) * 255,
            g: bound01(g, 255) * 255,
            b: bound01(b, 255) * 255
        };
    }

    // `rgbToHsl`
    // Converts an RGB color value to HSL.
    // *Assumes:* r, g, and b are contained in [0, 255] or [0, 1]
    // *Returns:* { h, s, l } in [0,1]
    function rgbToHsl(r, g, b) {

        r = bound01(r, 255);
        g = bound01(g, 255);
        b = bound01(b, 255);

        var max = mathMax(r, g, b), min = mathMin(r, g, b);
        var h, s, l = (max + min) / 2;

        if (max == min) {
            h = s = 0; // achromatic
        }
        else {
            var d = max - min;
            s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
            switch (max) {
                case r: h = (g - b) / d + (g < b ? 6 : 0); break;
                case g: h = (b - r) / d + 2; break;
                case b: h = (r - g) / d + 4; break;
            }

            h /= 6;
        }

        return { h: h, s: s, l: l };
    }

    // `hslToRgb`
    // Converts an HSL color value to RGB.
    // *Assumes:* h is contained in [0, 1] or [0, 360] and s and l are contained [0, 1] or [0, 100]
    // *Returns:* { r, g, b } in the set [0, 255]
    function hslToRgb(h, s, l) {
        var r, g, b;

        h = bound01(h, 360);
        s = bound01(s, 100);
        l = bound01(l, 100);

        function hue2rgb(p, q, t) {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1 / 6) return p + (q - p) * 6 * t;
            if (t < 1 / 2) return q;
            if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
            return p;
        }

        if (s === 0) {
            r = g = b = l; // achromatic
        }
        else {
            var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            var p = 2 * l - q;
            r = hue2rgb(p, q, h + 1 / 3);
            g = hue2rgb(p, q, h);
            b = hue2rgb(p, q, h - 1 / 3);
        }

        return { r: r * 255, g: g * 255, b: b * 255 };
    }

    // `rgbToHsv`
    // Converts an RGB color value to HSV
    // *Assumes:* r, g, and b are contained in the set [0, 255] or [0, 1]
    // *Returns:* { h, s, v } in [0,1]
    function rgbToHsv(r, g, b) {

        r = bound01(r, 255);
        g = bound01(g, 255);
        b = bound01(b, 255);

        var max = mathMax(r, g, b), min = mathMin(r, g, b);
        var h, s, v = max;

        var d = max - min;
        s = max === 0 ? 0 : d / max;

        if (max == min) {
            h = 0; // achromatic
        }
        else {
            switch (max) {
                case r: h = (g - b) / d + (g < b ? 6 : 0); break;
                case g: h = (b - r) / d + 2; break;
                case b: h = (r - g) / d + 4; break;
            }
            h /= 6;
        }
        return { h: h, s: s, v: v };
    }

    // `hsvToRgb`
    // Converts an HSV color value to RGB.
    // *Assumes:* h is contained in [0, 1] or [0, 360] and s and v are contained in [0, 1] or [0, 100]
    // *Returns:* { r, g, b } in the set [0, 255]
    function hsvToRgb(h, s, v) {

        h = bound01(h, 360) * 6;
        s = bound01(s, 100);
        v = bound01(v, 100);

        var i = math.floor(h),
            f = h - i,
            p = v * (1 - s),
            q = v * (1 - f * s),
            t = v * (1 - (1 - f) * s),
            mod = i % 6,
            r = [v, q, p, p, t, v][mod],
            g = [t, v, v, q, p, p][mod],
            b = [p, p, t, v, v, q][mod];

        return { r: r * 255, g: g * 255, b: b * 255 };
    }

    // `rgbToHex`
    // Converts an RGB color to hex
    // Assumes r, g, and b are contained in the set [0, 255]
    // Returns a 3 or 6 character hex
    function rgbToHex(r, g, b, allow3Char) {

        var hex = [
            pad2(mathRound(r).toString(16)),
            pad2(mathRound(g).toString(16)),
            pad2(mathRound(b).toString(16))
        ];

        // Return a 3 character hex if possible
        if (allow3Char && hex[0].charAt(0) == hex[0].charAt(1) && hex[1].charAt(0) == hex[1].charAt(1) && hex[2].charAt(0) == hex[2].charAt(1)) {
            return hex[0].charAt(0) + hex[1].charAt(0) + hex[2].charAt(0);
        }

        return hex.join("");
    }
    // `rgbaToHex`
    // Converts an RGBA color plus alpha transparency to hex
    // Assumes r, g, b and a are contained in the set [0, 255]
    // Returns an 8 character hex
    function rgbaToHex(r, g, b, a) {

        var hex = [
            pad2(convertDecimalToHex(a)),
            pad2(mathRound(r).toString(16)),
            pad2(mathRound(g).toString(16)),
            pad2(mathRound(b).toString(16))
        ];

        return hex.join("");
    }

    // `equals`
    // Can be called with any tinycolor input
    tinycolor.equals = function (color1, color2) {
        if (!color1 || !color2) { return false; }
        return tinycolor(color1).toRgbString() == tinycolor(color2).toRgbString();
    };
    tinycolor.random = function () {
        return tinycolor.fromRatio({
            r: mathRandom(),
            g: mathRandom(),
            b: mathRandom()
        });
    };


    // Modification Functions
    // ----------------------
    // Thanks to less.js for some of the basics here
    // <https://github.com/cloudhead/less.js/blob/master/lib/less/functions.js>

    function desaturate(color, amount) {
        amount = (amount === 0) ? 0 : (amount || 10);
        var hsl = tinycolor(color).toHsl();
        hsl.s -= amount / 100;
        hsl.s = clamp01(hsl.s);
        return tinycolor(hsl);
    }

    function saturate(color, amount) {
        amount = (amount === 0) ? 0 : (amount || 10);
        var hsl = tinycolor(color).toHsl();
        hsl.s += amount / 100;
        hsl.s = clamp01(hsl.s);
        return tinycolor(hsl);
    }

    function greyscale(color) {
        return tinycolor(color).desaturate(100);
    }

    function lighten(color, amount) {
        amount = (amount === 0) ? 0 : (amount || 10);
        var hsl = tinycolor(color).toHsl();
        hsl.l += amount / 100;
        hsl.l = clamp01(hsl.l);
        return tinycolor(hsl);
    }

    function brighten(color, amount) {
        amount = (amount === 0) ? 0 : (amount || 10);
        var rgb = tinycolor(color).toRgb();
        rgb.r = mathMax(0, mathMin(255, rgb.r - mathRound(255 * -(amount / 100))));
        rgb.g = mathMax(0, mathMin(255, rgb.g - mathRound(255 * -(amount / 100))));
        rgb.b = mathMax(0, mathMin(255, rgb.b - mathRound(255 * -(amount / 100))));
        return tinycolor(rgb);
    }

    function darken(color, amount) {
        amount = (amount === 0) ? 0 : (amount || 10);
        var hsl = tinycolor(color).toHsl();
        hsl.l -= amount / 100;
        hsl.l = clamp01(hsl.l);
        return tinycolor(hsl);
    }

    // Spin takes a positive or negative amount within [-360, 360] indicating the change of hue.
    // Values outside of this range will be wrapped into this range.
    function spin(color, amount) {
        var hsl = tinycolor(color).toHsl();
        var hue = (mathRound(hsl.h) + amount) % 360;
        hsl.h = hue < 0 ? 360 + hue : hue;
        return tinycolor(hsl);
    }

    // Combination Functions
    // ---------------------
    // Thanks to jQuery xColor for some of the ideas behind these
    // <https://github.com/infusion/jQuery-xcolor/blob/master/jquery.xcolor.js>

    function complement(color) {
        var hsl = tinycolor(color).toHsl();
        hsl.h = (hsl.h + 180) % 360;
        return tinycolor(hsl);
    }

    function triad(color) {
        var hsl = tinycolor(color).toHsl();
        var h = hsl.h;
        return [
            tinycolor(color),
            tinycolor({ h: (h + 120) % 360, s: hsl.s, l: hsl.l }),
            tinycolor({ h: (h + 240) % 360, s: hsl.s, l: hsl.l })
        ];
    }

    function tetrad(color) {
        var hsl = tinycolor(color).toHsl();
        var h = hsl.h;
        return [
            tinycolor(color),
            tinycolor({ h: (h + 90) % 360, s: hsl.s, l: hsl.l }),
            tinycolor({ h: (h + 180) % 360, s: hsl.s, l: hsl.l }),
            tinycolor({ h: (h + 270) % 360, s: hsl.s, l: hsl.l })
        ];
    }

    function splitcomplement(color) {
        var hsl = tinycolor(color).toHsl();
        var h = hsl.h;
        return [
            tinycolor(color),
            tinycolor({ h: (h + 72) % 360, s: hsl.s, l: hsl.l }),
            tinycolor({ h: (h + 216) % 360, s: hsl.s, l: hsl.l })
        ];
    }

    function analogous(color, results, slices) {
        results = results || 6;
        slices = slices || 30;

        var hsl = tinycolor(color).toHsl();
        var part = 360 / slices;
        var ret = [tinycolor(color)];

        for (hsl.h = ((hsl.h - (part * results >> 1)) + 720) % 360; --results;) {
            hsl.h = (hsl.h + part) % 360;
            ret.push(tinycolor(hsl));
        }
        return ret;
    }

    function monochromatic(color, results) {
        results = results || 6;
        var hsv = tinycolor(color).toHsv();
        var h = hsv.h, s = hsv.s, v = hsv.v;
        var ret = [];
        var modification = 1 / results;

        while (results--) {
            ret.push(tinycolor({ h: h, s: s, v: v }));
            v = (v + modification) % 1;
        }

        return ret;
    }

    // Utility Functions
    // ---------------------

    tinycolor.mix = function (color1, color2, amount) {
        amount = (amount === 0) ? 0 : (amount || 50);

        var rgb1 = tinycolor(color1).toRgb();
        var rgb2 = tinycolor(color2).toRgb();

        var p = amount / 100;
        var w = p * 2 - 1;
        var a = rgb2.a - rgb1.a;

        var w1;

        if (w * a == -1) {
            w1 = w;
        } else {
            w1 = (w + a) / (1 + w * a);
        }

        w1 = (w1 + 1) / 2;

        var w2 = 1 - w1;

        var rgba = {
            r: rgb2.r * w1 + rgb1.r * w2,
            g: rgb2.g * w1 + rgb1.g * w2,
            b: rgb2.b * w1 + rgb1.b * w2,
            a: rgb2.a * p + rgb1.a * (1 - p)
        };

        return tinycolor(rgba);
    };


    // Readability Functions
    // ---------------------
    // <http://www.w3.org/TR/2008/REC-WCAG20-20081211/#contrast-ratiodef (WCAG Version 2)

    // `contrast`
    // Analyze the 2 colors and returns the color contrast defined by (WCAG Version 2)
    tinycolor.readability = function (color1, color2) {
        var c1 = tinycolor(color1);
        var c2 = tinycolor(color2);
        return (Math.max(c1.getLuminance(), c2.getLuminance()) + 0.05) / (Math.min(c1.getLuminance(), c2.getLuminance()) + 0.05);
    };

    // `isReadable`
    // Ensure that foreground and background color combinations meet WCAG2 guidelines.
    // The third argument is an optional Object.
    //      the 'level' property states 'AA' or 'AAA' - if missing or invalid, it defaults to 'AA';
    //      the 'size' property states 'large' or 'small' - if missing or invalid, it defaults to 'small'.
    // If the entire object is absent, isReadable defaults to {level:"AA",size:"small"}.

    // *Example*
    //    tinycolor.isReadable("#000", "#111") => false
    //    tinycolor.isReadable("#000", "#111",{level:"AA",size:"large"}) => false

    tinycolor.isReadable = function (color1, color2, wcag2) {
        var readability = tinycolor.readability(color1, color2);
        var wcag2Parms, out;

        out = false;

        wcag2Parms = validateWCAG2Parms(wcag2);
        switch (wcag2Parms.level + wcag2Parms.size) {
            case "AAsmall":
            case "AAAlarge":
                out = readability >= 4.5;
                break;
            case "AAlarge":
                out = readability >= 3;
                break;
            case "AAAsmall":
                out = readability >= 7;
                break;
        }
        return out;

    };

    // `mostReadable`
    // Given a base color and a list of possible foreground or background
    // colors for that base, returns the most readable color.
    // Optionally returns Black or White if the most readable color is unreadable.
    // *Example*
    //    tinycolor.mostReadable(tinycolor.mostReadable("#123", ["#124", "#125"],{includeFallbackColors:false}).toHexString(); // "#112255"
    //    tinycolor.mostReadable(tinycolor.mostReadable("#123", ["#124", "#125"],{includeFallbackColors:true}).toHexString();  // "#ffffff"
    //    tinycolor.mostReadable("#a8015a", ["#faf3f3"],{includeFallbackColors:true,level:"AAA",size:"large"}).toHexString(); // "#faf3f3"
    //    tinycolor.mostReadable("#a8015a", ["#faf3f3"],{includeFallbackColors:true,level:"AAA",size:"small"}).toHexString(); // "#ffffff"


    tinycolor.mostReadable = function (baseColor, colorList, args) {
        var bestColor = null;
        var bestScore = 0;
        var readability;
        var includeFallbackColors, level, size;
        args = args || {};
        includeFallbackColors = args.includeFallbackColors;
        level = args.level;
        size = args.size;

        for (var i = 0; i < colorList.length ; i++) {
            readability = tinycolor.readability(baseColor, colorList[i]);
            if (readability > bestScore) {
                bestScore = readability;
                bestColor = tinycolor(colorList[i]);
            }
        }

        if (tinycolor.isReadable(baseColor, bestColor, { "level": level, "size": size }) || !includeFallbackColors) {
            return bestColor;
        }
        else {
            args.includeFallbackColors = false;
            return tinycolor.mostReadable(baseColor, ["#fff", "#000"], args);
        }
    };


    // Big List of Colors
    // ------------------
    // <http://www.w3.org/TR/css3-color/#svg-color>
    var names = tinycolor.names = {
        aliceblue: "f0f8ff",
        antiquewhite: "faebd7",
        aqua: "0ff",
        aquamarine: "7fffd4",
        azure: "f0ffff",
        beige: "f5f5dc",
        bisque: "ffe4c4",
        black: "000",
        blanchedalmond: "ffebcd",
        blue: "00f",
        blueviolet: "8a2be2",
        brown: "a52a2a",
        burlywood: "deb887",
        burntsienna: "ea7e5d",
        cadetblue: "5f9ea0",
        chartreuse: "7fff00",
        chocolate: "d2691e",
        coral: "ff7f50",
        cornflowerblue: "6495ed",
        cornsilk: "fff8dc",
        crimson: "dc143c",
        cyan: "0ff",
        darkblue: "00008b",
        darkcyan: "008b8b",
        darkgoldenrod: "b8860b",
        darkgray: "a9a9a9",
        darkgreen: "006400",
        darkgrey: "a9a9a9",
        darkkhaki: "bdb76b",
        darkmagenta: "8b008b",
        darkolivegreen: "556b2f",
        darkorange: "ff8c00",
        darkorchid: "9932cc",
        darkred: "8b0000",
        darksalmon: "e9967a",
        darkseagreen: "8fbc8f",
        darkslateblue: "483d8b",
        darkslategray: "2f4f4f",
        darkslategrey: "2f4f4f",
        darkturquoise: "00ced1",
        darkviolet: "9400d3",
        deeppink: "ff1493",
        deepskyblue: "00bfff",
        dimgray: "696969",
        dimgrey: "696969",
        dodgerblue: "1e90ff",
        firebrick: "b22222",
        floralwhite: "fffaf0",
        forestgreen: "228b22",
        fuchsia: "f0f",
        gainsboro: "dcdcdc",
        ghostwhite: "f8f8ff",
        gold: "ffd700",
        goldenrod: "daa520",
        gray: "808080",
        green: "008000",
        greenyellow: "adff2f",
        grey: "808080",
        honeydew: "f0fff0",
        hotpink: "ff69b4",
        indianred: "cd5c5c",
        indigo: "4b0082",
        ivory: "fffff0",
        khaki: "f0e68c",
        lavender: "e6e6fa",
        lavenderblush: "fff0f5",
        lawngreen: "7cfc00",
        lemonchiffon: "fffacd",
        lightblue: "add8e6",
        lightcoral: "f08080",
        lightcyan: "e0ffff",
        lightgoldenrodyellow: "fafad2",
        lightgray: "d3d3d3",
        lightgreen: "90ee90",
        lightgrey: "d3d3d3",
        lightpink: "ffb6c1",
        lightsalmon: "ffa07a",
        lightseagreen: "20b2aa",
        lightskyblue: "87cefa",
        lightslategray: "789",
        lightslategrey: "789",
        lightsteelblue: "b0c4de",
        lightyellow: "ffffe0",
        lime: "0f0",
        limegreen: "32cd32",
        linen: "faf0e6",
        magenta: "f0f",
        maroon: "800000",
        mediumaquamarine: "66cdaa",
        mediumblue: "0000cd",
        mediumorchid: "ba55d3",
        mediumpurple: "9370db",
        mediumseagreen: "3cb371",
        mediumslateblue: "7b68ee",
        mediumspringgreen: "00fa9a",
        mediumturquoise: "48d1cc",
        mediumvioletred: "c71585",
        midnightblue: "191970",
        mintcream: "f5fffa",
        mistyrose: "ffe4e1",
        moccasin: "ffe4b5",
        navajowhite: "ffdead",
        navy: "000080",
        oldlace: "fdf5e6",
        olive: "808000",
        olivedrab: "6b8e23",
        orange: "ffa500",
        orangered: "ff4500",
        orchid: "da70d6",
        palegoldenrod: "eee8aa",
        palegreen: "98fb98",
        paleturquoise: "afeeee",
        palevioletred: "db7093",
        papayawhip: "ffefd5",
        peachpuff: "ffdab9",
        peru: "cd853f",
        pink: "ffc0cb",
        plum: "dda0dd",
        powderblue: "b0e0e6",
        purple: "800080",
        rebeccapurple: "663399",
        red: "f00",
        rosybrown: "bc8f8f",
        royalblue: "4169e1",
        saddlebrown: "8b4513",
        salmon: "fa8072",
        sandybrown: "f4a460",
        seagreen: "2e8b57",
        seashell: "fff5ee",
        sienna: "a0522d",
        silver: "c0c0c0",
        skyblue: "87ceeb",
        slateblue: "6a5acd",
        slategray: "708090",
        slategrey: "708090",
        snow: "fffafa",
        springgreen: "00ff7f",
        steelblue: "4682b4",
        tan: "d2b48c",
        teal: "008080",
        thistle: "d8bfd8",
        tomato: "ff6347",
        turquoise: "40e0d0",
        violet: "ee82ee",
        wheat: "f5deb3",
        white: "fff",
        whitesmoke: "f5f5f5",
        yellow: "ff0",
        yellowgreen: "9acd32"
    };

    // Make it easy to access colors via `hexNames[hex]`
    var hexNames = tinycolor.hexNames = flip(names);


    // Utilities
    // ---------

    // `{ 'name1': 'val1' }` becomes `{ 'val1': 'name1' }`
    function flip(o) {
        var flipped = {};
        for (var i in o) {
            if (o.hasOwnProperty(i)) {
                flipped[o[i]] = i;
            }
        }
        return flipped;
    }

    // Return a valid alpha value [0,1] with all invalid values being set to 1
    function boundAlpha(a) {
        a = parseFloat(a);

        if (isNaN(a) || a < 0 || a > 1) {
            a = 1;
        }

        return a;
    }

    // Take input from [0, n] and return it as [0, 1]
    function bound01(n, max) {
        if (isOnePointZero(n)) { n = "100%"; }

        var processPercent = isPercentage(n);
        n = mathMin(max, mathMax(0, parseFloat(n)));

        // Automatically convert percentage into number
        if (processPercent) {
            n = parseInt(n * max, 10) / 100;
        }

        // Handle floating point rounding errors
        if ((math.abs(n - max) < 0.000001)) {
            return 1;
        }

        // Convert into [0, 1] range if it isn't already
        return (n % max) / parseFloat(max);
    }

    // Force a number between 0 and 1
    function clamp01(val) {
        return mathMin(1, mathMax(0, val));
    }

    // Parse a base-16 hex value into a base-10 integer
    function parseIntFromHex(val) {
        return parseInt(val, 16);
    }

    // Need to handle 1.0 as 100%, since once it is a number, there is no difference between it and 1
    // <http://stackoverflow.com/questions/7422072/javascript-how-to-detect-number-as-a-decimal-including-1-0>
    function isOnePointZero(n) {
        return typeof n == "string" && n.indexOf('.') != -1 && parseFloat(n) === 1;
    }

    // Check to see if string passed in is a percentage
    function isPercentage(n) {
        return typeof n === "string" && n.indexOf('%') != -1;
    }

    // Force a hex value to have 2 characters
    function pad2(c) {
        return c.length == 1 ? '0' + c : '' + c;
    }

    // Replace a decimal with it's percentage value
    function convertToPercentage(n) {
        if (n <= 1) {
            n = (n * 100) + "%";
        }

        return n;
    }

    // Converts a decimal to a hex value
    function convertDecimalToHex(d) {
        return Math.round(parseFloat(d) * 255).toString(16);
    }
    // Converts a hex value to a decimal
    function convertHexToDecimal(h) {
        return (parseIntFromHex(h) / 255);
    }

    var matchers = (function () {

        // <http://www.w3.org/TR/css3-values/#integers>
        var CSS_INTEGER = "[-\\+]?\\d+%?";

        // <http://www.w3.org/TR/css3-values/#number-value>
        var CSS_NUMBER = "[-\\+]?\\d*\\.\\d+%?";

        // Allow positive/negative integer/number.  Don't capture the either/or, just the entire outcome.
        var CSS_UNIT = "(?:" + CSS_NUMBER + ")|(?:" + CSS_INTEGER + ")";

        // Actual matching.
        // Parentheses and commas are optional, but not required.
        // Whitespace can take the place of commas or opening paren
        var PERMISSIVE_MATCH3 = "[\\s|\\(]+(" + CSS_UNIT + ")[,|\\s]+(" + CSS_UNIT + ")[,|\\s]+(" + CSS_UNIT + ")\\s*\\)?";
        var PERMISSIVE_MATCH4 = "[\\s|\\(]+(" + CSS_UNIT + ")[,|\\s]+(" + CSS_UNIT + ")[,|\\s]+(" + CSS_UNIT + ")[,|\\s]+(" + CSS_UNIT + ")\\s*\\)?";

        return {
            rgb: new RegExp("rgb" + PERMISSIVE_MATCH3),
            rgba: new RegExp("rgba" + PERMISSIVE_MATCH4),
            hsl: new RegExp("hsl" + PERMISSIVE_MATCH3),
            hsla: new RegExp("hsla" + PERMISSIVE_MATCH4),
            hsv: new RegExp("hsv" + PERMISSIVE_MATCH3),
            hsva: new RegExp("hsva" + PERMISSIVE_MATCH4),
            hex3: /^([0-9a-fA-F]{1})([0-9a-fA-F]{1})([0-9a-fA-F]{1})$/,
            hex6: /^([0-9a-fA-F]{2})([0-9a-fA-F]{2})([0-9a-fA-F]{2})$/,
            hex8: /^([0-9a-fA-F]{2})([0-9a-fA-F]{2})([0-9a-fA-F]{2})([0-9a-fA-F]{2})$/
        };
    })();

    // `stringInputToObject`
    // Permissive string parsing.  Take in a number of formats, and output an object
    // based on detected format.  Returns `{ r, g, b }` or `{ h, s, l }` or `{ h, s, v}`
    function stringInputToObject(color) {

        color = color.replace(trimLeft, '').replace(trimRight, '').toLowerCase();
        var named = false;
        if (names[color]) {
            color = names[color];
            named = true;
        }
        else if (color == 'transparent') {
            return { r: 0, g: 0, b: 0, a: 0, format: "name" };
        }

        // Try to match string input using regular expressions.
        // Keep most of the number bounding out of this function - don't worry about [0,1] or [0,100] or [0,360]
        // Just return an object and let the conversion functions handle that.
        // This way the result will be the same whether the tinycolor is initialized with string or object.
        var match;
        if ((match = matchers.rgb.exec(color))) {
            return { r: match[1], g: match[2], b: match[3] };
        }
        if ((match = matchers.rgba.exec(color))) {
            return { r: match[1], g: match[2], b: match[3], a: match[4] };
        }
        if ((match = matchers.hsl.exec(color))) {
            return { h: match[1], s: match[2], l: match[3] };
        }
        if ((match = matchers.hsla.exec(color))) {
            return { h: match[1], s: match[2], l: match[3], a: match[4] };
        }
        if ((match = matchers.hsv.exec(color))) {
            return { h: match[1], s: match[2], v: match[3] };
        }
        if ((match = matchers.hsva.exec(color))) {
            return { h: match[1], s: match[2], v: match[3], a: match[4] };
        }
        if ((match = matchers.hex8.exec(color))) {
            return {
                a: convertHexToDecimal(match[1]),
                r: parseIntFromHex(match[2]),
                g: parseIntFromHex(match[3]),
                b: parseIntFromHex(match[4]),
                format: named ? "name" : "hex8"
            };
        }
        if ((match = matchers.hex6.exec(color))) {
            return {
                r: parseIntFromHex(match[1]),
                g: parseIntFromHex(match[2]),
                b: parseIntFromHex(match[3]),
                format: named ? "name" : "hex"
            };
        }
        if ((match = matchers.hex3.exec(color))) {
            return {
                r: parseIntFromHex(match[1] + '' + match[1]),
                g: parseIntFromHex(match[2] + '' + match[2]),
                b: parseIntFromHex(match[3] + '' + match[3]),
                format: named ? "name" : "hex"
            };
        }

        return false;
    }

    function validateWCAG2Parms(parms) {
        // return valid WCAG2 parms for isReadable.
        // If input parms are invalid, return {"level":"AA", "size":"small"}
        var level, size;
        parms = parms || { "level": "AA", "size": "small" };
        level = (parms.level || "AA").toUpperCase();
        size = (parms.size || "small").toLowerCase();
        if (level !== "AA" && level !== "AAA") {
            level = "AA";
        }
        if (size !== "small" && size !== "large") {
            size = "small";
        }
        return { "level": level, "size": size };
    }
    // Node: Export function
    if (typeof module !== "undefined" && module.exports) {
        module.exports = tinycolor;
    }
        // AMD/requirejs: Define the module
    else if (typeof define === 'function' && define.amd) {
        define(function () { return tinycolor; });
    }
        // Browser: Expose to window
    else {
        window.tinycolor = tinycolor;
    }

})();

(function($) {
  'use strict';

  $.fn.ColorPickerSliders = function(options) {

    return this.each(function() {

      var alreadyinitialized = false,
          settings,
          triggerelement = $(this),
          triggerelementisinput = triggerelement.is('input'),
          container,
          popover_container,
          elements,
          connectedinput = false,
          swatches,
          groupingname = '',
          rendermode = false,
          visible = false,
          MAXLIGHT = 101, // 101 needed for bright colors (maybe due to rounding errors)
          dragTarget = false,
          lastUpdateTime = 0,
          _moveThrottleTimer = null,
          _throttleDelay = 70,
          _inMoveHandler = false,
          _lastMoveHandlerRun = 0,
          color = {
            tiny: null,
            hsla: null,
            rgba: null,
            hsv: null,
            cielch: null
          },
      MAXVALIDCHROMA = 144;   // maximum valid chroma value found convertible to rgb (blue)

      init();

      function _initSettings() {
        if (typeof options === 'undefined') {
          options = {};
        }

        settings = $.extend({
          color: 'hsl(342, 52%, 70%)',
          size: 'default', // sm | default | lg
          placement: 'auto',
          trigger: 'focus', // focus | manual
          preventtouchkeyboardonshow: true, // makes the input readonly and needs a second click to be editable
          title: '',
          hsvpanel: false,
          sliders: true,
          grouping: true,
          swatches: ['FFFFFF', 'C0C0C0', '808080', '000000', 'FF0000', '800000', 'FFFF00', '808000', '00FF00', '008000', '00FFFF', '008080', '0000FF', '000080', 'FF00FF', '800080'], // array or false to disable swatches
          customswatches: 'colorpickkersliders', // false or a grop name
          connectedinput: false, // can be a jquery object or a selector
          flat: false,
          updateinterval: 30, // update interval of the sliders while in drag (ms)
          previewontriggerelement: true,
          previewcontrasttreshold: 15,
          previewformat: 'rgb', // rgb | hsl | hex
          erroneousciecolormarkers: true,
          invalidcolorsopacity: 1, // everything below 1 causes slightly slower responses
          finercierangeedges: true, // can be disabled for faster responses
          titleswatchesadd: 'Add color to swatches',
          titleswatchesremove: 'Remove color from swatches',
          titleswatchesreset: 'Reset to default swatches',
          order: {},
          labels: {},
          onchange: function() {
          }
        }, options);

        if (options.hasOwnProperty('order')) {
          settings.order = $.extend({
            opacity: false,
            hsl: false,
            rgb: false,
            cie: false,
            preview: false
          }, options.order);
        }
        else {
          settings.order = {
            opacity: 0,
            hsl: 1,
            rgb: 2,
            cie: 3, // cie sliders can increase response time of all sliders!
            preview: 4
          };
        }

        if (!options.hasOwnProperty('labels')) {
          options.labels = {};
        }

        settings.labels = $.extend({
          hslhue: 'HSL-Hue',
          hslsaturation: 'HSL-Saturation',
          hsllightness: 'HSL-Lightness',
          rgbred: 'RGB-Red',
          rgbgreen: 'RGB-Green',
          rgbblue: 'RGB-Blue',
          cielightness: 'CIE-Lightness',
          ciechroma: 'CIE-Chroma',
          ciehue: 'CIE-hue',
          opacity: 'Opacity',
          preview: 'Preview'
        }, options.labels);
      }

      function init() {
        if (alreadyinitialized) {
          return;
        }

        alreadyinitialized = true;

        rendermode = $.fn.ColorPickerSliders.detectWhichGradientIsSupported();

        if (rendermode === 'filter') {
          rendermode = false;
        }

        if (!rendermode && $.fn.ColorPickerSliders.svgSupported()) {
          rendermode = 'svg';
        }

        _initSettings();

        // force preview when browser doesn't support css gradients
        if ((!settings.order.hasOwnProperty('preview') || settings.order.preview === false) && !rendermode) {
          settings.order.preview = 10;
        }

        _initConnectedElements();
        _initColor();
        _initConnectedinput();
        _updateTriggerelementColor();
        _updateConnectedInput();

        if (settings.flat) {
          showFlat();
        }

        _bindEvents();
      }

      function _buildComponent() {
        _initElements();
        _renderSwatches();
        _updateAllElements();
        _bindControllerEvents();
      }

      function _initColor() {
        if (triggerelementisinput) {
          color.tiny = tinycolor(triggerelement.val());

          if (!color.tiny.isValid()) {
            color.tiny = tinycolor(settings.color);
          }
        }
        else {
          color.tiny = tinycolor(settings.color);
        }

        color.hsla = color.tiny.toHsl();
        color.rgba = color.tiny.toRgb();
        color.hsv = color.tiny.toHsv();
        color.cielch = $.fn.ColorPickerSliders.rgb2lch(color.rgba);
      }

      function _initConnectedinput() {
        if (settings.connectedinput) {
          if (settings.connectedinput instanceof jQuery) {
            connectedinput = settings.connectedinput;
          }
          else {
            connectedinput = $(settings.connectedinput);
          }
        }
      }

      function updateColor(newcolor, disableinputupdate) {
        var updatedcolor = tinycolor(newcolor);

        if (updatedcolor.isValid()) {
          color.tiny = updatedcolor;
          color.hsla = updatedcolor.toHsl();
          color.rgba = updatedcolor.toRgb();
          color.hsv = updatedcolor.toHsv();
          color.cielch = $.fn.ColorPickerSliders.rgb2lch(color.rgba);

          if (settings.flat || visible) {
            container.removeClass('cp-unconvertible-cie-color');
            _updateAllElements(disableinputupdate);
          }
          else {
            if (!disableinputupdate) {
              _updateConnectedInput();
            }
            _updateTriggerelementColor();
          }

          return true;
        }
        else {
          return false;
        }
      }

      function show(disableLastlyUsedGroupUpdate) {
        if (settings.flat) {
          return;
        }

        if (visible) {
          // repositions the popover
          triggerelement.popover('hide');
          triggerelement.popover('show');
          _bindControllerEvents();
          return;
        }

        showPopover(disableLastlyUsedGroupUpdate);

        visible = true;
      }

      function hide() {
        visible = false;
        hidePopover();
      }

      function showPopover(disableLastlyUsedGroupUpdate) {
        if (popover_container instanceof jQuery) {
          return;
        }

        if (typeof disableLastlyUsedGroupUpdate === 'undefined') {
          disableLastlyUsedGroupUpdate = false;
        }

        popover_container = $('<div class="cp-popover-container"></div>').appendTo('body');

        container = $('<div class="cp-container"></div>').appendTo(popover_container);
        container.html(_getControllerHtml());

        switch (settings.size) {
          case 'sm':
            container.addClass('cp-container-sm');
            break;
          case 'lg':
            container.addClass('cp-container-lg');
            break;
        }

        _buildComponent();

        if (!disableLastlyUsedGroupUpdate) {
          activateLastlyUsedGroup();
        }

        triggerelement.popover({
          html: true,
          animation: false,
          trigger: 'manual',
          title: settings.title,
          placement: settings.placement,
          container: popover_container,
          content: function() {
            return container;
          }
        });

        triggerelement.popover('show');
      }

      function hidePopover() {
        popover_container.remove();
        popover_container = null;

        triggerelement.popover('destroy');
      }

      function _getControllerHtml() {
        var sliders = [],
            color_picker_html = '';

        if (settings.sliders) {

          if (settings.order.opacity !== false) {
            sliders[settings.order.opacity] = '<div class="cp-slider cp-opacity cp-transparency"><span>' + settings.labels.opacity + '</span><div class="cp-marker"></div></div>';
          }

          if (settings.order.hsl !== false) {
            sliders[settings.order.hsl] = '<div class="cp-slider cp-hslhue cp-transparency"><span>' + settings.labels.hslhue + '</span><div class="cp-marker"></div></div><div class="cp-slider cp-hslsaturation cp-transparency"><span>' + settings.labels.hslsaturation + '</span><div class="cp-marker"></div></div><div class="cp-slider cp-hsllightness cp-transparency"><span>' + settings.labels.hsllightness + '</span><div class="cp-marker"></div></div>';
          }

          if (settings.order.rgb !== false) {
            sliders[settings.order.rgb] = '<div class="cp-slider cp-rgbred cp-transparency"><span>' + settings.labels.rgbred + '</span><div class="cp-marker"></div></div><div class="cp-slider cp-rgbgreen cp-transparency"><span>' + settings.labels.rgbgreen + '</span><div class="cp-marker"></div></div><div class="cp-slider cp-rgbblue cp-transparency"><span>' + settings.labels.rgbblue + '</span><div class="cp-marker"></div></div>';
          }

          if (settings.order.cie !== false) {
            sliders[settings.order.cie] = '<div class="cp-slider cp-cielightness cp-transparency"><span>' + settings.labels.cielightness + '</span><div class="cp-marker"></div></div><div class="cp-slider cp-ciechroma cp-transparency"><span>' + settings.labels.ciechroma + '</span><div class="cp-marker"></div></div><div class="cp-slider cp-ciehue cp-transparency"><span>' + settings.labels.ciehue + '</span><div class="cp-marker"></div></div>';
          }

          if (settings.order.preview !== false) {
            sliders[settings.order.preview] = '<div class="cp-preview cp-transparency"><input type="text" readonly="readonly"></div>';
          }

        }

        if (settings.grouping) {
          if (!!settings.hsvpanel + !!(settings.sliders && sliders.length > 0) + !!settings.swatches > 1) {
            color_picker_html += '<ul class="cp-pills">';
          }
          else {
            color_picker_html += '<ul class="cp-pills hidden">';
          }

          if (settings.hsvpanel) {
            color_picker_html += '<li><a href="#" class="cp-pill-hsvpanel">HSV panel</a></li>';
          }
          if (settings.sliders && sliders.length > 0) {
            color_picker_html += '<li><a href="#" class="cp-pill-sliders">Sliders</a></li>';
          }
          if (settings.swatches) {
            color_picker_html += '<li><a href="#" class="cp-pill-swatches">Swatches</a></li>';
          }

          color_picker_html += '</ul>';
        }

        if (settings.hsvpanel) {
          color_picker_html += '<div class="cp-hsvpanel">' +
              '<div class="cp-hsvpanel-sv"><span></span><div class="cp-marker-point"></div></div>' +
              '<div class="cp-hsvpanel-h"><span></span><div class="cp-hsvmarker-vertical"></div></div>' +
              '<div class="cp-hsvpanel-a cp-transparency"><span></span><div class="cp-hsvmarker-vertical"></div></div>' +
              '</div>';
        }

        if (settings.sliders) {
          color_picker_html += '<div class="cp-sliders">';

          for (var i = 0; i < sliders.length; i++) {
            if (typeof sliders[i] === 'undefined') {
              continue;
            }

            color_picker_html += sliders[i];
          }

          color_picker_html += '</div>';

        }

        if (settings.swatches) {
          color_picker_html += '<div class="cp-swatches clearfix"><button type="button" class="add btn btn-default" title="' + settings.titleswatchesadd + '"><span class="glyphicon glyphicon-floppy-save"></span></button><button type="button" class="remove btn btn-default" title="' + settings.titleswatchesremove + '"><span class="glyphicon glyphicon-trash"></span></button><button type="button" class="reset btn btn-default" title="' + settings.titleswatchesreset + '"><span class="glyphicon glyphicon-repeat"></span></button><ul></ul></div>';
        }

        return color_picker_html;
      }

      function _initElements() {
        elements = {
          actualswatch: false,
          swatchescontainer: $('.cp-swatches', container),
          swatches: $('.cp-swatches ul', container),
          swatches_add: $('.cp-swatches button.add', container),
          swatches_remove: $('.cp-swatches button.remove', container),
          swatches_reset: $('.cp-swatches button.reset', container),
          all_sliders: $('.cp-sliders, .cp-preview input', container),
          hsvpanel: {
            sv: $('.cp-hsvpanel-sv', container),
            sv_marker: $('.cp-hsvpanel-sv .cp-marker-point', container),
            h: $('.cp-hsvpanel-h', container),
            h_marker: $('.cp-hsvpanel-h .cp-hsvmarker-vertical', container),
            a: $('.cp-hsvpanel-a span', container),
            a_marker: $('.cp-hsvpanel-a .cp-hsvmarker-vertical', container)
          },
          sliders: {
            hue: $('.cp-hslhue span', container),
            hue_marker: $('.cp-hslhue .cp-marker', container),
            saturation: $('.cp-hslsaturation span', container),
            saturation_marker: $('.cp-hslsaturation .cp-marker', container),
            lightness: $('.cp-hsllightness span', container),
            lightness_marker: $('.cp-hsllightness .cp-marker', container),
            opacity: $('.cp-opacity span', container),
            opacity_marker: $('.cp-opacity .cp-marker', container),
            red: $('.cp-rgbred span', container),
            red_marker: $('.cp-rgbred .cp-marker', container),
            green: $('.cp-rgbgreen span', container),
            green_marker: $('.cp-rgbgreen .cp-marker', container),
            blue: $('.cp-rgbblue span', container),
            blue_marker: $('.cp-rgbblue .cp-marker', container),
            cielightness: $('.cp-cielightness span', container),
            cielightness_marker: $('.cp-cielightness .cp-marker', container),
            ciechroma: $('.cp-ciechroma span', container),
            ciechroma_marker: $('.cp-ciechroma .cp-marker', container),
            ciehue: $('.cp-ciehue span', container),
            ciehue_marker: $('.cp-ciehue .cp-marker', container),
            preview: $('.cp-preview input', container)
          },
          all_pills: $('.cp-pills', container),
          pills: {
            hsvpanel: $('.cp-pill-hsvpanel', container),
            sliders: $('.cp-pill-sliders', container),
            swatches: $('.cp-pill-swatches', container)
          }
        };

        if (!settings.customswatches) {
          elements.swatches_add.hide();
          elements.swatches_remove.hide();
          elements.swatches_reset.hide();
        }
      }

      function showFlat() {
        if (settings.flat) {
          if (triggerelementisinput) {
            container = $('<div class="cp-container"></div>').insertAfter(triggerelement);
          }
          else {
            container = $('<div class="cp-container"></div>');
            triggerelement.append(container);
          }

          container.append(_getControllerHtml());

          _buildComponent();

          activateLastlyUsedGroup();
        }
      }

      function _initConnectedElements() {
        if (settings.connectedinput instanceof jQuery) {
          settings.connectedinput.add(triggerelement);
        }
        else if (settings.connectedinput === false) {
          settings.connectedinput = triggerelement;
        }
        else {
          settings.connectedinput = $(settings.connectedinput).add(triggerelement);
        }
      }

      function _bindEvents() {
        triggerelement.on('colorpickersliders.updateColor', function(e, newcolor) {
          updateColor(newcolor);
        });

        triggerelement.on('colorpickersliders.show', function() {
          show();
        });

        triggerelement.on('colorpickersliders.hide', function() {
          hide();
        });

        if (!settings.flat && settings.trigger === 'focus') {
          // we need tabindex defined to be focusable
          if (typeof triggerelement.attr('tabindex') === 'undefined') {
            triggerelement.attr('tabindex', -1);
          }

          if (settings.preventtouchkeyboardonshow) {
            $(triggerelement).prop('readonly', true).addClass('cp-preventtouchkeyboardonshow');

            $(triggerelement).on('click', function(ev) {
              if (visible) {
                $(triggerelement).prop('readonly', false);
                ev.stopPropagation();
              }
            });
          }

          // buttons doesn't get focus in webkit browsers
          // https://bugs.webkit.org/show_bug.cgi?id=22261
          // and only input and button are focusable on iPad
          // so it is safer to register click on any other than inputs
          if (!triggerelementisinput) {
            $(triggerelement).on('click', function(ev) {
              show();

              ev.stopPropagation();
            });
          }

          $(triggerelement).on('focus', function(ev) {
            show();

            ev.stopPropagation();
          });

          $(triggerelement).on('blur', function(ev) {
            hide();

            if (settings.preventtouchkeyboardonshow) {
              $(triggerelement).prop('readonly', true);
            }

            ev.stopPropagation();
          });
        }

        if (connectedinput) {
          connectedinput.on('keyup change', function() {
            var $input = $(this);

            updateColor($input.val(), true);
          });
        }

      }

      function _bindControllerEvents() {
        container.on('contextmenu', function(ev) {
          ev.preventDefault();
          return false;
        });

        $(document).on('colorpickersliders.changeswatches', function() {
          _renderSwatches();
        });

        elements.swatches.on('touchstart mousedown click', 'li span', function(ev) {
          var color = $(this).css('background-color');
          updateColor(color);
          //_updateAllElements();
          ev.preventDefault();
        });

        elements.swatches_add.on('touchstart mousedown click', function(ev) {
          _addCurrentColorToSwatches();
          ev.preventDefault();
          ev.stopPropagation();
        });

        elements.swatches_remove.on('touchstart mousedown click', function(ev) {
          _removeActualColorFromSwatches();
          ev.preventDefault();
          ev.stopPropagation();
        });

        elements.swatches_reset.on('touchstart touchend mousedown click', function(ev) {
          // prevent multiple fire on android...
          if (ev.type === 'click' || ev.type === 'touchend') {
            _resetSwatches();
          }
          ev.preventDefault();
          ev.stopImmediatePropagation();
        });

        elements.sliders.hue.parent().on('touchstart mousedown', function(ev) {
          ev.preventDefault();

          if (ev.which > 1) {
            return;
          }

          dragTarget = 'hue';

          var percent = _updateMarkerPosition(dragTarget, ev);

          _updateColorsProperty('hsla', 'h', 3.6 * percent);

          _updateAllElements();
        });

        elements.sliders.saturation.parent().on('touchstart mousedown', function(ev) {
          ev.preventDefault();

          if (ev.which > 1) {
            return;
          }

          dragTarget = 'saturation';

          var percent = _updateMarkerPosition(dragTarget, ev);

          _updateColorsProperty('hsla', 's', percent / 100);

          _updateAllElements();
        });

        elements.sliders.lightness.parent().on('touchstart mousedown', function(ev) {
          ev.preventDefault();

          if (ev.which > 1) {
            return;
          }

          dragTarget = 'lightness';

          var percent = _updateMarkerPosition(dragTarget, ev);

          _updateColorsProperty('hsla', 'l', percent / 100);

          _updateAllElements();
        });

        elements.sliders.opacity.parent().on('touchstart mousedown', function(ev) {
          ev.preventDefault();

          if (ev.which > 1) {
            return;
          }

          dragTarget = 'opacity';

          var percent = _updateMarkerPosition(dragTarget, ev);

          _updateColorsProperty('hsla', 'a', percent / 100);

          _updateAllElements();
        });

        elements.sliders.red.parent().on('touchstart mousedown', function(ev) {
          ev.preventDefault();

          if (ev.which > 1) {
            return;
          }

          dragTarget = 'red';

          var percent = _updateMarkerPosition(dragTarget, ev);

          _updateColorsProperty('rgba', 'r', 2.55 * percent);

          _updateAllElements();
        });

        elements.sliders.green.parent().on('touchstart mousedown', function(ev) {
          ev.preventDefault();

          if (ev.which > 1) {
            return;
          }

          dragTarget = 'green';

          var percent = _updateMarkerPosition(dragTarget, ev);

          _updateColorsProperty('rgba', 'g', 2.55 * percent);

          _updateAllElements();
        });

        elements.sliders.blue.parent().on('touchstart mousedown', function(ev) {
          ev.preventDefault();

          if (ev.which > 1) {
            return;
          }

          dragTarget = 'blue';

          var percent = _updateMarkerPosition(dragTarget, ev);

          _updateColorsProperty('rgba', 'b', 2.55 * percent);

          _updateAllElements();
        });

        elements.hsvpanel.sv.on('touchstart mousedown', function(ev) {
          ev.preventDefault();

          if (ev.which > 1) {
            return;
          }

          dragTarget = 'hsvsv';

          var percent = _updateHsvpanelMarkerPosition('sv', ev);

          _updateColorsProperty('hsv', 's', percent.horizontal / 100);
          _updateColorsProperty('hsv', 'v', (100 - percent.vertical) / 100);

          _updateAllElements();
        });

        elements.hsvpanel.h.on('touchstart mousedown', function(ev) {
          ev.preventDefault();

          if (ev.which > 1) {
            return;
          }

          dragTarget = 'hsvh';

          var percent = _updateHsvpanelMarkerPosition('h', ev);

          _updateColorsProperty('hsv', 'h', 3.6 * percent.vertical);

          _updateAllElements();
        });

        elements.hsvpanel.a.on('touchstart mousedown', function(ev) {
          ev.preventDefault();

          if (ev.which > 1) {
            return;
          }

          dragTarget = 'hsva';

          var percent = _updateHsvpanelMarkerPosition('a', ev);

          _updateColorsProperty('hsv', 'a', (100 - percent.vertical) / 100);

          _updateAllElements();
        });

        elements.sliders.cielightness.parent().on('touchstart mousedown', function(ev) {
          ev.preventDefault();

          if (ev.which > 1) {
            return;
          }

          dragTarget = 'cielightness';

          var percent = _updateMarkerPosition(dragTarget, ev);

          _updateColorsProperty('cielch', 'l', (MAXLIGHT / 100) * percent);

          _updateAllElements();
        });

        elements.sliders.ciechroma.parent().on('touchstart mousedown', function(ev) {
          ev.preventDefault();

          if (ev.which > 1) {
            return;
          }

          dragTarget = 'ciechroma';

          var percent = _updateMarkerPosition(dragTarget, ev);

          _updateColorsProperty('cielch', 'c', (MAXVALIDCHROMA / 100) * percent);

          _updateAllElements();
        });

        elements.sliders.ciehue.parent().on('touchstart mousedown', function(ev) {
          ev.preventDefault();

          if (ev.which > 1) {
            return;
          }

          dragTarget = 'ciehue';

          var percent = _updateMarkerPosition(dragTarget, ev);

          _updateColorsProperty('cielch', 'h', 3.6 * percent);

          _updateAllElements();
        });

        elements.sliders.preview.on('click', function() {
          this.select();
        });

        $(document).on('touchmove mousemove', function(ev) {
          if (!dragTarget) {
            return;
          }

          if (new Date().getTime() - _lastMoveHandlerRun > _throttleDelay && !_inMoveHandler) {
            moveHandler(dragTarget, ev);
          }
          else {
            setMoveHandlerTimer(dragTarget, ev);
          }
        });

        $(document).on('touchend mouseup', function(ev) {
          if (ev.which > 1) {
            return;
          }

          if (dragTarget) {
            dragTarget = false;
            ev.preventDefault();
          }
        });

        elements.pills.hsvpanel.on('click', function(ev) {
          ev.preventDefault();

          activateGroupHsvpanel();
        });

        elements.pills.sliders.on('click', function(ev) {
          ev.preventDefault();

          activateGroupSliders();
        });

        elements.pills.swatches.on('click', function(ev) {
          ev.preventDefault();

          activateGroupSwatches();
        });

        if (!settings.flat) {
          popover_container.on('touchstart mousedown', '.popover', function(ev) {
            ev.preventDefault();
            ev.stopPropagation();

            return false;
          });
        }
      }

      function setConfig(name, value) {
        try {
          localStorage.setItem('cp-userdata-' + name, JSON.stringify(value));
        }
        catch (err) {
        }
      }

      function getConfig(name) {
        try {
          var r = JSON.parse(localStorage.getItem('cp-userdata-' + name));

          return r;
        }
        catch (err) {
          return null;
        }
      }

      function getUsedGroupName() {
        if (groupingname !== '') {
          return groupingname;
        }

        if (elements.pills.hsvpanel.length === 0) {
          groupingname += '_hsvpanel_';
        }
        if (elements.pills.sliders.length === 0) {
          groupingname += '_sliders_';
        }
        if (elements.pills.swatches.length === 0) {
          groupingname += '_swatches_';
        }

        return groupingname;
      }

      function getLastlyUsedGroup() {
        return getConfig('config_activepill' + getUsedGroupName());
      }

      function setLastlyUsedGroup(value) {
        return setConfig('config_activepill' + getUsedGroupName(), value);
      }

      function activateLastlyUsedGroup() {
        switch (getLastlyUsedGroup()) {
          case 'hsvpanel':
            activateGroupHsvpanel();
            break;
          case 'sliders':
            activateGroupSliders();
            break;
          case 'swatches':
            activateGroupSwatches();
            break;
          default:
            if (elements.pills.hsvpanel.length) {
              activateGroupHsvpanel();
              break;
            }
            else if (elements.pills.sliders.length) {
              activateGroupSliders();
              break;
            }
            else if (elements.pills.swatches.length) {
              activateGroupSwatches();
              break;
            }
        }
      }

      function activateGroupHsvpanel() {
        if (elements.pills.hsvpanel.length === 0) {
          return false;
        }

        $('a', elements.all_pills).removeClass('active');
        elements.pills.hsvpanel.addClass('active');

        container.removeClass('sliders-active swatches-active').addClass('hsvpanel-active');

        setLastlyUsedGroup('hsvpanel');

        _updateAllElements(true);

        show(true);

        return true;
      }

      function activateGroupSliders() {
        if (elements.pills.sliders.length === 0) {
          return false;
        }

        $('a', elements.all_pills).removeClass('active');
        elements.pills.sliders.addClass('active');

        container.removeClass('hsvpanel-active swatches-active').addClass('sliders-active');

        setLastlyUsedGroup('sliders');

        _updateAllElements(true);

        show(true);

        return true;
      }

      function activateGroupSwatches() {
        if (elements.pills.swatches.length === 0) {
          return false;
        }

        $('a', elements.all_pills).removeClass('active');
        elements.pills.swatches.addClass('active');

        container.removeClass('hsvpanel-active sliders-active').addClass('swatches-active');

        setLastlyUsedGroup('swatches');

        _updateAllElements(true);

        show(true);

        return true;
      }

      function setMoveHandlerTimer(dragTarget, ev) {
        clearTimeout(_moveThrottleTimer);
        _moveThrottleTimer = setTimeout(function() {
          moveHandler(dragTarget, ev);
        }, _throttleDelay);
      }

      function moveHandler(dragTarget, ev) {
        var percent;

        if (_inMoveHandler) {
          setMoveHandlerTimer(dragTarget, ev);
          return;
        }

        _inMoveHandler = true;
        _lastMoveHandlerRun = new Date().getTime();

        if (dragTarget === 'hsvsv') {
          percent = _updateHsvpanelMarkerPosition('sv', ev);
        }
        else if (dragTarget === 'hsvh') {
          percent = _updateHsvpanelMarkerPosition('h', ev);
        }
        else if (dragTarget === 'hsva') {
          percent = _updateHsvpanelMarkerPosition('a', ev);
        }
        else {
          percent = _updateMarkerPosition(dragTarget, ev);
        }

        switch (dragTarget) {
          case 'hsvsv':
            _updateColorsProperty('hsv', 's', percent.horizontal / 100);
            _updateColorsProperty('hsv', 'v', (100 - percent.vertical) / 100);
            break;
          case 'hsvh':
            _updateColorsProperty('hsv', 'h', 3.6 * percent.vertical);
            break;
          case 'hsva':
            _updateColorsProperty('hsv', 'a', (100 - percent.vertical) / 100);
            break;
          case 'hue':
            _updateColorsProperty('hsla', 'h', 3.6 * percent);
            break;
          case 'saturation':
            _updateColorsProperty('hsla', 's', percent / 100);
            break;
          case 'lightness':
            _updateColorsProperty('hsla', 'l', percent / 100);
            break;
          case 'opacity':
            _updateColorsProperty('hsla', 'a', percent / 100);
            break;
          case 'red':
            _updateColorsProperty('rgba', 'r', 2.55 * percent);
            break;
          case 'green':
            _updateColorsProperty('rgba', 'g', 2.55 * percent);
            break;
          case 'blue':
            _updateColorsProperty('rgba', 'b', 2.55 * percent);
            break;
          case 'cielightness':
            _updateColorsProperty('cielch', 'l', (MAXLIGHT / 100) * percent);
            break;
          case 'ciechroma':
            _updateColorsProperty('cielch', 'c', (MAXVALIDCHROMA / 100) * percent);
            break;
          case 'ciehue':
            _updateColorsProperty('cielch', 'h', 3.6 * percent);
            break;
        }

        _updateAllElements();

        ev.preventDefault();
        _inMoveHandler = false;
      }

      function _parseCustomSwatches() {
        swatches = [];

        for (var i = 0; i < settings.swatches.length; i++) {
          var color = tinycolor(settings.swatches[i]);

          if (color.isValid()) {
            swatches.push(color.toRgbString());
          }
        }
      }

      function _renderSwatches() {
        if (!settings.swatches) {
          return;
        }

        if (settings.customswatches) {
          var customswatches = false;

          try {
            customswatches = JSON.parse(localStorage.getItem('swatches-' + settings.customswatches));
          }
          catch (err) {
          }

          if (customswatches) {
            swatches = customswatches;
          }
          else {
            _parseCustomSwatches();
          }
        }
        else {
          _parseCustomSwatches();
        }

        if (swatches instanceof Array) {
          elements.swatches.html('');
          for (var i = 0; i < swatches.length; i++) {
            var color = tinycolor(swatches[i]);

            if (color.isValid()) {
              var span = $('<span></span>').css('background-color', color.toRgbString());
              var button = $('<div class="btn btn-default cp-swatch"></div>');

              button.append(span);

              elements.swatches.append($('<li></li>').append(button));
            }
          }
        }

        _findActualColorsSwatch();
      }

      function _findActualColorsSwatch() {
        var found = false;

        $('span', elements.swatches).filter(function() {
          var swatchcolor = $(this).css('background-color');

          swatchcolor = tinycolor(swatchcolor);
          swatchcolor.alpha = Math.round(swatchcolor.alpha * 100) / 100;

          if (swatchcolor.toRgbString() === color.tiny.toRgbString()) {
            found = true;

            var currentswatch = $(this).parent();

            if (!currentswatch.is(elements.actualswatch)) {
              if (elements.actualswatch) {
                elements.actualswatch.removeClass('actual');
              }
              elements.actualswatch = currentswatch;
              currentswatch.addClass('actual');
            }
          }
        });

        if (!found) {
          if (elements.actualswatch) {
            elements.actualswatch.removeClass('actual');
            elements.actualswatch = false;
          }
        }

        if (elements.actualswatch) {
          elements.swatches_add.prop('disabled', true);
          elements.swatches_remove.prop('disabled', false);
        }
        else {
          elements.swatches_add.prop('disabled', false);
          elements.swatches_remove.prop('disabled', true);
        }
      }

      function _storeSwatches() {
        localStorage.setItem('swatches-' + settings.customswatches, JSON.stringify(swatches));
      }

      function _addCurrentColorToSwatches() {
        swatches.unshift(color.tiny.toRgbString());
        _storeSwatches();

        $(document).trigger('colorpickersliders.changeswatches');
      }

      function _removeActualColorFromSwatches() {
        var index = swatches.indexOf(color.tiny.toRgbString());

        if (index !== -1) {
          swatches.splice(index, 1);

          _storeSwatches();
          $(document).trigger('colorpickersliders.changeswatches');
        }
      }

      function _resetSwatches() {
        if (confirm('Do you really want to reset the swatches? All customizations will be lost!')) {
          _parseCustomSwatches();

          _storeSwatches();

          $(document).trigger('colorpickersliders.changeswatches');
        }
      }

      function _updateColorsProperty(format, property, value) {
        switch (format) {
          case 'hsv':

            color.hsv[property] = value;
            color.tiny = tinycolor({h: color.hsv.h, s: color.hsv.s, v: color.hsv.v, a: color.hsv.a});
            color.rgba = color.tiny.toRgb();
            color.hsla = color.tiny.toHsl();
            color.cielch = $.fn.ColorPickerSliders.rgb2lch(color.rgba);

            break;

          case 'hsla':

            color.hsla[property] = value;
            color.tiny = tinycolor({h: color.hsla.h, s: color.hsla.s, l: color.hsla.l, a: color.hsla.a});
            color.rgba = color.tiny.toRgb();
            color.hsv = color.tiny.toHsv();
            color.cielch = $.fn.ColorPickerSliders.rgb2lch(color.rgba);

            container.removeClass('cp-unconvertible-cie-color');

            break;

          case 'rgba':

            color.rgba[property] = value;
            color.tiny = tinycolor({r: color.rgba.r, g: color.rgba.g, b: color.rgba.b, a: color.hsla.a});
            color.hsla = color.tiny.toHsl();
            color.hsv = color.tiny.toHsv();
            color.cielch = $.fn.ColorPickerSliders.rgb2lch(color.rgba);

            container.removeClass('cp-unconvertible-cie-color');

            break;

          case 'cielch':

            color.cielch[property] = value;
            color.rgba = $.fn.ColorPickerSliders.lch2rgb(color.cielch);
            color.tiny = tinycolor(color.rgba);
            color.hsla = color.tiny.toHsl();
            color.hsv = color.tiny.toHsv();

            if (settings.erroneousciecolormarkers) {
              if (color.rgba.isok) {
                container.removeClass('cp-unconvertible-cie-color');
              }
              else {
                container.addClass('cp-unconvertible-cie-color');
              }
            }

            break;
        }
      }

      function _updateMarkerPosition(slidername, ev) {
        var percent = $.fn.ColorPickerSliders.calculateEventPositionPercentage(ev, elements.sliders[slidername]);

        elements.sliders[slidername + '_marker'].data('position', percent);

        return percent;
      }

      function _updateHsvpanelMarkerPosition(marker, ev) {
        var percents = $.fn.ColorPickerSliders.calculateEventPositionPercentage(ev, elements.hsvpanel.sv, true);

        elements.hsvpanel[marker + '_marker'].data('position', percents);

        return percents;
      }

      var updateAllElementsTimeout;

      function _updateAllElementsTimer(disableinputupdate) {
        updateAllElementsTimeout = setTimeout(function() {
          _updateAllElements(disableinputupdate);
        }, settings.updateinterval);
      }

      function _updateAllElements(disableinputupdate) {
        clearTimeout(updateAllElementsTimeout);

        Date.now = Date.now || function() {
          return +new Date();
        };

        if (Date.now() - lastUpdateTime < settings.updateinterval) {
          _updateAllElementsTimer(disableinputupdate);
          return;
        }

        if (typeof disableinputupdate === 'undefined') {
          disableinputupdate = false;
        }

        lastUpdateTime = Date.now();

        if (settings.hsvpanel !== false && (!settings.grouping || getLastlyUsedGroup() === 'hsvpanel')) {
          _renderHsvsv();
          _renderHsvh();
          _renderHsva();
        }

        if (settings.sliders && (!settings.grouping || getLastlyUsedGroup() === 'sliders')) {
          if (settings.order.opacity !== false) {
            _renderOpacity();
          }

          if (settings.order.hsl !== false) {
            _renderHue();
            _renderSaturation();
            _renderLightness();
          }

          if (settings.order.rgb !== false) {
            _renderRed();
            _renderGreen();
            _renderBlue();
          }

          if (settings.order.cie !== false) {
            _renderCieLightness();
            _renderCieChroma();
            _renderCieHue();
          }

          if (settings.order.preview !== false) {
            _renderPreview();
          }
        }

        if (!disableinputupdate) {
          _updateConnectedInput();
        }

        if ((100 - color.cielch.l) * color.cielch.a < settings.previewcontrasttreshold) {
          elements.all_sliders.css('color', '#000');
          if (triggerelementisinput && settings.previewontriggerelement) {
            triggerelement.css('background', color.tiny.toRgbString()).css('color', '#000');
          }
        }
        else {
          elements.all_sliders.css('color', '#fff');
          if (triggerelementisinput && settings.previewontriggerelement) {
            triggerelement.css('background', color.tiny.toRgbString()).css('color', '#fff');
          }
        }

        if (settings.swatches && (!settings.grouping || getLastlyUsedGroup() === 'swatches')) {
          _findActualColorsSwatch();
        }

        settings.onchange(container, color);

        triggerelement.data('color', color);
      }

      function _updateTriggerelementColor() {
        if (triggerelementisinput && settings.previewontriggerelement) {
          if ((100 - color.cielch.l) * color.cielch.a < settings.previewcontrasttreshold) {
            triggerelement.css('background', color.tiny.toRgbString()).css('color', '#000');
          }
          else {
            triggerelement.css('background', color.tiny.toRgbString()).css('color', '#fff');
          }
        }
      }

      function _updateConnectedInput() {
        if (connectedinput) {
          connectedinput.each(function(index, element) {
            var $element = $(element),
                format = $element.data('color-format') || settings.previewformat;

            switch (format) {
              case 'hex':
                if (color.hsla.a < 1) {
                  $element.val(color.tiny.toRgbString());
                }
                else {
                  $element.val(color.tiny.toHexString());
                }
                break;
              case 'hsl':
                $element.val(color.tiny.toHslString());
                break;
              case 'rgb':
                /* falls through */
              default:
                $element.val(color.tiny.toRgbString());
                break;
            }
          });
        }
      }

      function _renderHsvsv() {
        elements.hsvpanel.sv.css('background', tinycolor('hsv(' + color.hsv.h + ',100%,100%)').toRgbString());

        elements.hsvpanel.sv_marker.css('left', color.hsv.s * 100 + '%').css('top', 100 - color.hsv.v * 100 + '%');
      }

      function _renderHsvh() {
        elements.hsvpanel.h_marker.css('top', color.hsv.h / 360 * 100 + '%');
      }

      function _renderHsva() {
        setGradient(elements.hsvpanel.a, $.fn.ColorPickerSliders.getScaledGradientStops(color.hsla, 'a', 1, 0, 2), true);

        elements.hsvpanel.a_marker.css('top', 100 - color.hsv.a * 100 + '%');
      }

      function _renderHue() {
        setGradient(elements.sliders.hue, $.fn.ColorPickerSliders.getScaledGradientStops(color.hsla, 'h', 0, 360, 7));

        elements.sliders.hue_marker.css('left', color.hsla.h / 360 * 100 + '%');
      }

      function _renderSaturation() {
        setGradient(elements.sliders.saturation, $.fn.ColorPickerSliders.getScaledGradientStops(color.hsla, 's', 0, 1, 2));

        elements.sliders.saturation_marker.css('left', color.hsla.s * 100 + '%');
      }

      function _renderLightness() {
        setGradient(elements.sliders.lightness, $.fn.ColorPickerSliders.getScaledGradientStops(color.hsla, 'l', 0, 1, 3));

        elements.sliders.lightness_marker.css('left', color.hsla.l * 100 + '%');
      }

      function _renderOpacity() {
        setGradient(elements.sliders.opacity, $.fn.ColorPickerSliders.getScaledGradientStops(color.hsla, 'a', 0, 1, 2));

        elements.sliders.opacity_marker.css('left', color.hsla.a * 100 + '%');
      }

      function _renderRed() {
        setGradient(elements.sliders.red, $.fn.ColorPickerSliders.getScaledGradientStops(color.rgba, 'r', 0, 255, 2));

        elements.sliders.red_marker.css('left', color.rgba.r / 255 * 100 + '%');
      }

      function _renderGreen() {
        setGradient(elements.sliders.green, $.fn.ColorPickerSliders.getScaledGradientStops(color.rgba, 'g', 0, 255, 2));

        elements.sliders.green_marker.css('left', color.rgba.g / 255 * 100 + '%');
      }

      function _renderBlue() {
        setGradient(elements.sliders.blue, $.fn.ColorPickerSliders.getScaledGradientStops(color.rgba, 'b', 0, 255, 2));

        elements.sliders.blue_marker.css('left', color.rgba.b / 255 * 100 + '%');
      }

      function _extendCieGradientStops(gradientstops, property) {
        if (settings.invalidcolorsopacity === 1 || !settings.finercierangeedges) {
          return gradientstops;
        }

        gradientstops.sort(function(a, b) {
          return a.position - b.position;
        });

        var tmparray = [];

        for (var i = 1; i < gradientstops.length; i++) {
          if (gradientstops[i].isok !== gradientstops[i - 1].isok) {
            var steps = Math.round(gradientstops[i].position) - Math.round(gradientstops[i - 1].position),
                extendedgradientstops = $.fn.ColorPickerSliders.getScaledGradientStops(gradientstops[i].rawcolor, property, gradientstops[i - 1].rawcolor[property], gradientstops[i].rawcolor[property], steps, settings.invalidcolorsopacity, gradientstops[i - 1].position, gradientstops[i].position);

            for (var j = 0; j < extendedgradientstops.length; j++) {
              if (extendedgradientstops[j].isok !== gradientstops[i - 1].isok) {
                tmparray.push(extendedgradientstops[j]);

                if (j > 0) {
                  tmparray.push(extendedgradientstops[j - 1]);
                }

                break;
              }
            }
          }
        }

        return $.merge(tmparray, gradientstops);
      }

      function _renderCieLightness() {
        var gradientstops = $.fn.ColorPickerSliders.getScaledGradientStops(color.cielch, 'l', 0, 100, 10, settings.invalidcolorsopacity);

        gradientstops = _extendCieGradientStops(gradientstops, 'l');

        setGradient(elements.sliders.cielightness, gradientstops);

        elements.sliders.cielightness_marker.css('left', color.cielch.l / MAXLIGHT * 100 + '%');
      }

      function _renderCieChroma() {
        var gradientstops = $.fn.ColorPickerSliders.getScaledGradientStops(color.cielch, 'c', 0, MAXVALIDCHROMA, 5, settings.invalidcolorsopacity);

        gradientstops = _extendCieGradientStops(gradientstops, 'c');

        setGradient(elements.sliders.ciechroma, gradientstops);

        elements.sliders.ciechroma_marker.css('left', color.cielch.c / MAXVALIDCHROMA * 100 + '%');
      }

      function _renderCieHue() {
        var gradientstops = $.fn.ColorPickerSliders.getScaledGradientStops(color.cielch, 'h', 0, 360, 28, settings.invalidcolorsopacity);

        gradientstops = _extendCieGradientStops(gradientstops, 'h');

        setGradient(elements.sliders.ciehue, gradientstops);

        elements.sliders.ciehue_marker.css('left', color.cielch.h / 360 * 100 + '%');
      }

      function _renderPreview() {
        elements.sliders.preview.css('background', $.fn.ColorPickerSliders.csscolor(color.rgba));

        var colorstring;

        switch (settings.previewformat) {
          case 'hex':
            if (color.hsla.a < 1) {
              colorstring = color.tiny.toRgbString();
            }
            else {
              colorstring = color.tiny.toHexString();
            }
            break;
          case 'hsl':
            colorstring = color.tiny.toHslString();
            break;
          case 'rgb':
            /* falls through */
          default:
            colorstring = color.tiny.toRgbString();
            break;
        }

        elements.sliders.preview.val(colorstring);
      }

      function setGradient(element, gradientstops, vertical) {
        if (typeof vertical === 'undefined') {
          vertical = false;
        }

        gradientstops.sort(function(a, b) {
          return a.position - b.position;
        });

        switch (rendermode) {
          case 'noprefix':
            $.fn.ColorPickerSliders.renderNoprefix(element, gradientstops, vertical);
            break;
          case 'webkit':
            $.fn.ColorPickerSliders.renderWebkit(element, gradientstops, vertical);
            break;
          case 'ms':
            $.fn.ColorPickerSliders.renderMs(element, gradientstops, vertical);
            break;
          case 'svg': // can not repeat, radial can be only a covering ellipse (maybe there is a workaround, need more investigation)
            $.fn.ColorPickerSliders.renderSVG(element, gradientstops, vertical);
            break;
          case 'oldwebkit':   // can not repeat, no percent size with radial gradient (and no ellipse)
            $.fn.ColorPickerSliders.renderOldwebkit(element, gradientstops, vertical);
            break;
        }
      }

    });

  };

  $.fn.ColorPickerSliders.getEventCoordinates = function(ev) {
    if (typeof ev.pageX !== 'undefined') {
      return {
        pageX: ev.originalEvent.pageX,
        pageY: ev.originalEvent.pageY
      };
    }
    else if (typeof ev.originalEvent.touches !== 'undefined') {
      return {
        pageX: ev.originalEvent.touches[0].pageX,
        pageY: ev.originalEvent.touches[0].pageY
      };
    }
  };

  $.fn.ColorPickerSliders.calculateEventPositionPercentage = function(ev, containerElement, both) {
    if (typeof (both) === 'undefined') {
      both = false;
    }

    var c = $.fn.ColorPickerSliders.getEventCoordinates(ev);

    var xsize = containerElement.width(),
        offsetX = c.pageX - containerElement.offset().left;

    var horizontal = offsetX / xsize * 100;

    if (horizontal < 0) {
      horizontal = 0;
    }

    if (horizontal > 100) {
      horizontal = 100;
    }

    if (both) {
      var ysize = containerElement.height(),
          offsetY = c.pageY - containerElement.offset().top;

      var vertical = offsetY / ysize * 100;

      if (vertical < 0) {
        vertical = 0;
      }

      if (vertical > 100) {
        vertical = 100;
      }

      return {
        horizontal: horizontal,
        vertical: vertical
      };
    }

    return horizontal;
  };

  $.fn.ColorPickerSliders.getScaledGradientStops = function(color, scalableproperty, minvalue, maxvalue, steps, invalidcolorsopacity, minposition, maxposition) {
    if (typeof invalidcolorsopacity === 'undefined') {
      invalidcolorsopacity = 1;
    }

    if (typeof minposition === 'undefined') {
      minposition = 0;
    }

    if (typeof maxposition === 'undefined') {
      maxposition = 100;
    }

    var gradientStops = [],
        diff = maxvalue - minvalue,
        isok = true;

    for (var i = 0; i < steps; ++i) {
      var currentstage = i / (steps - 1),
          modifiedcolor = $.fn.ColorPickerSliders.modifyColor(color, scalableproperty, currentstage * diff + minvalue),
          csscolor;

      if (invalidcolorsopacity < 1) {
        var stagergb = $.fn.ColorPickerSliders.lch2rgb(modifiedcolor, invalidcolorsopacity);

        isok = stagergb.isok;
        csscolor = $.fn.ColorPickerSliders.csscolor(stagergb, invalidcolorsopacity);
      }
      else {
        csscolor = $.fn.ColorPickerSliders.csscolor(modifiedcolor, invalidcolorsopacity);
      }

      gradientStops[i] = {
        color: csscolor,
        position: currentstage * (maxposition - minposition) + minposition,
        isok: isok,
        rawcolor: modifiedcolor
      };
    }

    return gradientStops;
  };

  $.fn.ColorPickerSliders.getGradientStopsCSSString = function(gradientstops) {
    var gradientstring = '',
        oldwebkit = '',
        svgstoppoints = '';

    for (var i = 0; i < gradientstops.length; i++) {
      var el = gradientstops[i];

      gradientstring += ',' + el.color + ' ' + el.position + '%';
      oldwebkit += ',color-stop(' + el.position + '%,' + el.color + ')';

      var svgcolor = tinycolor(el.color);

      svgstoppoints += '<stop ' + 'stop-color="' + svgcolor.toHexString() + '" stop-opacity="' + svgcolor.toRgb().a + '"' + ' offset="' + el.position / 100 + '"/>';
    }

    return {
      noprefix: gradientstring,
      oldwebkit: oldwebkit,
      svg: svgstoppoints
    };
  };

  $.fn.ColorPickerSliders.renderNoprefix = function(element, gradientstops, vertical) {
    if (typeof vertical === 'undefined') {
      vertical = false;
    }

    var css,
        stoppoints = $.fn.ColorPickerSliders.getGradientStopsCSSString(gradientstops).noprefix;

    if (!vertical) {
      css = 'linear-gradient(to right';
    }
    else {
      css = 'linear-gradient(to bottom';
    }

    css += stoppoints + ')';

    element.css('background-image', css);
  };

  $.fn.ColorPickerSliders.renderWebkit = function(element, gradientstops, vertical) {
    if (typeof vertical === 'undefined') {
      vertical = false;
    }

    var css,
        stoppoints = $.fn.ColorPickerSliders.getGradientStopsCSSString(gradientstops).noprefix;

    if (!vertical) {
      css = '-webkit-linear-gradient(left';
    }
    else {
      css = '-webkit-linear-gradient(top';
    }

    css += stoppoints + ')';

    element.css('background-image', css);
  };

  $.fn.ColorPickerSliders.renderOldwebkit = function(element, gradientstops, vertical) {
    if (typeof vertical === 'undefined') {
      vertical = false;
    }

    var css,
        stoppoints = $.fn.ColorPickerSliders.getGradientStopsCSSString(gradientstops).oldwebkit;

    if (!vertical) {
      css = '-webkit-gradient(linear, 0% 0%, 100% 0%';
    }
    else {
      css = '-webkit-gradient(linear, 0% 0%, 0 100%';
    }
    css += stoppoints + ')';

    element.css('background-image', css);
  };

  $.fn.ColorPickerSliders.renderMs = function(element, gradientstops, vertical) {
    if (typeof vertical === 'undefined') {
      vertical = false;
    }

    var css,
        stoppoints = $.fn.ColorPickerSliders.getGradientStopsCSSString(gradientstops).noprefix;

    if (!vertical) {
      css = '-ms-linear-gradient(to right';
    }
    else {
      css = '-ms-linear-gradient(to bottom';
    }

    css += stoppoints + ')';

    element.css('background-image', css);
  };

  $.fn.ColorPickerSliders.renderSVG = function(element, gradientstops, vertical) {
    if (typeof vertical === 'undefined') {
      vertical = false;
    }

    var svg = '',
        svgstoppoints = $.fn.ColorPickerSliders.getGradientStopsCSSString(gradientstops).svg;

    if (!vertical) {
      svg = '<svg xmlns="http://www.w3.org/2000/svg" width="100%" height="100%" viewBox="0 0 1 1" preserveAspectRatio="none"><linearGradient id="vsgg" gradientUnits="userSpaceOnUse" x1="0" y1="0" x2="100%" y2="0">';
    }
    else {
      svg = '<svg xmlns="http://www.w3.org/2000/svg" width="100%" height="100%" viewBox="0 0 1 1" preserveAspectRatio="none"><linearGradient id="vsgg" gradientUnits="userSpaceOnUse" x1="0" y1="0" x2="0" y2="100%">';
    }

    svg += svgstoppoints;
    svg += '</linearGradient><rect x="0" y="0" width="1" height="1" fill="url(#vsgg)" /></svg>';
    svg = 'url(data:image/svg+xml;base64,' + $.fn.ColorPickerSliders.base64encode(svg) + ')';

    element.css('background-image', svg);
  };

  /* source: http://phpjs.org/functions/base64_encode/ */
  $.fn.ColorPickerSliders.base64encode = function(data) {
    var b64 = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=';
    var o1, o2, o3, h1, h2, h3, h4, bits, i = 0,
        ac = 0,
        enc = '',
        tmp_arr = [];

    if (!data) {
      return data;
    }

    do {
      o1 = data.charCodeAt(i++);
      o2 = data.charCodeAt(i++);
      o3 = data.charCodeAt(i++);

      bits = o1 << 16 | o2 << 8 | o3;

      h1 = bits >> 18 & 0x3f;
      h2 = bits >> 12 & 0x3f;
      h3 = bits >> 6 & 0x3f;
      h4 = bits & 0x3f;

      tmp_arr[ac++] = b64.charAt(h1) + b64.charAt(h2) + b64.charAt(h3) + b64.charAt(h4);
    } while (i < data.length);

    enc = tmp_arr.join('');

    var r = data.length % 3;

    return (r ? enc.slice(0, r - 3) : enc) + '==='.slice(r || 3);
  };

  $.fn.ColorPickerSliders.isGoodRgb = function(rgb) {
    // the default acceptable values are out of 0..255 due to
    // rounding errors with yellow and blue colors (258, -1)
    var maxacceptable = 258;
    var minacceptable = -1;

    if (rgb.r > maxacceptable || rgb.g > maxacceptable || rgb.b > maxacceptable || rgb.r < minacceptable || rgb.g < minacceptable || rgb.b < minacceptable) {
      return false;
    }
    else {
      rgb.r = Math.min(255, rgb.r);
      rgb.g = Math.min(255, rgb.g);
      rgb.b = Math.min(255, rgb.b);
      rgb.r = Math.max(0, rgb.r);
      rgb.g = Math.max(0, rgb.g);
      rgb.b = Math.max(0, rgb.b);

      return true;
    }
  };

  $.fn.ColorPickerSliders.rgb2lch = function(rgb) {
    var lch = $.fn.ColorPickerSliders.CIELab2CIELCH($.fn.ColorPickerSliders.XYZ2CIELab($.fn.ColorPickerSliders.rgb2XYZ(rgb)));

    if (rgb.hasOwnProperty('a')) {
      lch.a = rgb.a;
    }

    return lch;
  };

  $.fn.ColorPickerSliders.lch2rgb = function(lch, invalidcolorsopacity) {
    if (typeof invalidcolorsopacity === 'undefined') {
      invalidcolorsopacity = 1;
    }

    var rgb = $.fn.ColorPickerSliders.XYZ2rgb($.fn.ColorPickerSliders.CIELab2XYZ($.fn.ColorPickerSliders.CIELCH2CIELab(lch)));

    if ($.fn.ColorPickerSliders.isGoodRgb(rgb)) {
      if (lch.hasOwnProperty('a')) {
        rgb.a = lch.a;
      }

      rgb.isok = true;

      return rgb;
    }

    var tmp = $.extend({}, lch),
        lastbadchroma = tmp.c,
        lastgoodchroma = -1,
        loops = 0;

    do {
      ++loops;

      tmp.c = lastgoodchroma + ((lastbadchroma - lastgoodchroma) / 2);

      rgb = $.fn.ColorPickerSliders.XYZ2rgb($.fn.ColorPickerSliders.CIELab2XYZ($.fn.ColorPickerSliders.CIELCH2CIELab(tmp)));

      if ($.fn.ColorPickerSliders.isGoodRgb(rgb)) {
        lastgoodchroma = tmp.c;
      }
      else {
        lastbadchroma = tmp.c;
      }
    } while (Math.abs(lastbadchroma - lastgoodchroma) > 0.9 && loops < 100);

    if (lch.hasOwnProperty('a')) {
      rgb.a = lch.a;
    }

    rgb.r = Math.max(0, rgb.r);
    rgb.g = Math.max(0, rgb.g);
    rgb.b = Math.max(0, rgb.b);

    rgb.r = Math.min(255, rgb.r);
    rgb.g = Math.min(255, rgb.g);
    rgb.b = Math.min(255, rgb.b);

    if (invalidcolorsopacity < 1) {
      if (rgb.hasOwnProperty('a')) {
        rgb.a = rgb.a * invalidcolorsopacity;
      }
      else {
        rgb.a = invalidcolorsopacity;
      }
    }

    rgb.isok = false;

    return rgb;
  };

  $.fn.ColorPickerSliders.modifyColor = function(color, property, value) {
    var modifiedcolor = $.extend({}, color);

    if (!color.hasOwnProperty(property)) {
      throw('Missing color property: ' + property);
    }

    modifiedcolor[property] = value;

    return modifiedcolor;
  };

  $.fn.ColorPickerSliders.csscolor = function(color, invalidcolorsopacity) {
    if (typeof invalidcolorsopacity === 'undefined') {
      invalidcolorsopacity = 1;
    }

    var $return = false,
        tmpcolor = $.extend({}, color);

    if (tmpcolor.hasOwnProperty('c')) {
      // CIE-LCh
      tmpcolor = $.fn.ColorPickerSliders.lch2rgb(tmpcolor, invalidcolorsopacity);
    }

    if (tmpcolor.hasOwnProperty('h')) {
      // HSL
      $return = 'hsla(' + tmpcolor.h + ',' + tmpcolor.s * 100 + '%,' + tmpcolor.l * 100 + '%,' + tmpcolor.a + ')';
    }

    if (tmpcolor.hasOwnProperty('r')) {
      // RGB
      if (tmpcolor.a < 1) {
        $return = 'rgba(' + Math.round(tmpcolor.r) + ',' + Math.round(tmpcolor.g) + ',' + Math.round(tmpcolor.b) + ',' + tmpcolor.a + ')';
      }
      else {
        $return = 'rgb(' + Math.round(tmpcolor.r) + ',' + Math.round(tmpcolor.g) + ',' + Math.round(tmpcolor.b) + ')';
      }
    }

    return $return;
  };

  $.fn.ColorPickerSliders.rgb2XYZ = function(rgb) {
    var XYZ = {};

    var r = (rgb.r / 255);
    var g = (rgb.g / 255);
    var b = (rgb.b / 255);

    if (r > 0.04045) {
      r = Math.pow(((r + 0.055) / 1.055), 2.4);
    }
    else {
      r = r / 12.92;
    }

    if (g > 0.04045) {
      g = Math.pow(((g + 0.055) / 1.055), 2.4);
    }
    else {
      g = g / 12.92;
    }

    if (b > 0.04045) {
      b = Math.pow(((b + 0.055) / 1.055), 2.4);
    }
    else {
      b = b / 12.92;
    }

    r = r * 100;
    g = g * 100;
    b = b * 100;

    // Observer = 2°, Illuminant = D65
    XYZ.x = r * 0.4124 + g * 0.3576 + b * 0.1805;
    XYZ.y = r * 0.2126 + g * 0.7152 + b * 0.0722;
    XYZ.z = r * 0.0193 + g * 0.1192 + b * 0.9505;

    return XYZ;
  };

  $.fn.ColorPickerSliders.XYZ2CIELab = function(XYZ) {
    var CIELab = {};

    // Observer = 2°, Illuminant = D65
    var X = XYZ.x / 95.047;
    var Y = XYZ.y / 100.000;
    var Z = XYZ.z / 108.883;

    if (X > 0.008856) {
      X = Math.pow(X, 0.333333333);
    }
    else {
      X = 7.787 * X + 0.137931034;
    }

    if (Y > 0.008856) {
      Y = Math.pow(Y, 0.333333333);
    }
    else {
      Y = 7.787 * Y + 0.137931034;
    }

    if (Z > 0.008856) {
      Z = Math.pow(Z, 0.333333333);
    }
    else {
      Z = 7.787 * Z + 0.137931034;
    }

    CIELab.l = (116 * Y) - 16;
    CIELab.a = 500 * (X - Y);
    CIELab.b = 200 * (Y - Z);

    return CIELab;
  };

  $.fn.ColorPickerSliders.CIELab2CIELCH = function(CIELab) {
    var CIELCH = {};

    CIELCH.l = CIELab.l;
    CIELCH.c = Math.sqrt(Math.pow(CIELab.a, 2) + Math.pow(CIELab.b, 2));

    CIELCH.h = Math.atan2(CIELab.b, CIELab.a);  //Quadrant by signs

    if (CIELCH.h > 0) {
      CIELCH.h = (CIELCH.h / Math.PI) * 180;
    }
    else {
      CIELCH.h = 360 - (Math.abs(CIELCH.h) / Math.PI) * 180;
    }

    return CIELCH;
  };

  $.fn.ColorPickerSliders.CIELCH2CIELab = function(CIELCH) {
    var CIELab = {};

    CIELab.l = CIELCH.l;
    CIELab.a = Math.cos(CIELCH.h * 0.01745329251) * CIELCH.c;
    CIELab.b = Math.sin(CIELCH.h * 0.01745329251) * CIELCH.c;

    return CIELab;
  };

  $.fn.ColorPickerSliders.CIELab2XYZ = function(CIELab) {
    var XYZ = {};

    XYZ.y = (CIELab.l + 16) / 116;
    XYZ.x = CIELab.a / 500 + XYZ.y;
    XYZ.z = XYZ.y - CIELab.b / 200;

    if (Math.pow(XYZ.y, 3) > 0.008856) {
      XYZ.y = Math.pow(XYZ.y, 3);
    }
    else {
      XYZ.y = (XYZ.y - 0.137931034) / 7.787;
    }

    if (Math.pow(XYZ.x, 3) > 0.008856) {
      XYZ.x = Math.pow(XYZ.x, 3);
    }
    else {
      XYZ.x = (XYZ.x - 0.137931034) / 7.787;
    }

    if (Math.pow(XYZ.z, 3) > 0.008856) {
      XYZ.z = Math.pow(XYZ.z, 3);
    }
    else {
      XYZ.z = (XYZ.z - 0.137931034) / 7.787;
    }

    // Observer = 2°, Illuminant = D65
    XYZ.x = 95.047 * XYZ.x;
    XYZ.y = 100.000 * XYZ.y;
    XYZ.z = 108.883 * XYZ.z;

    return XYZ;
  };

  $.fn.ColorPickerSliders.XYZ2rgb = function(XYZ) {
    var rgb = {};

    // Observer = 2°, Illuminant = D65
    XYZ.x = XYZ.x / 100;        // X from 0 to 95.047
    XYZ.y = XYZ.y / 100;        // Y from 0 to 100.000
    XYZ.z = XYZ.z / 100;        // Z from 0 to 108.883

    rgb.r = XYZ.x * 3.2406 + XYZ.y * -1.5372 + XYZ.z * -0.4986;
    rgb.g = XYZ.x * -0.9689 + XYZ.y * 1.8758 + XYZ.z * 0.0415;
    rgb.b = XYZ.x * 0.0557 + XYZ.y * -0.2040 + XYZ.z * 1.0570;

    if (rgb.r > 0.0031308) {
      rgb.r = 1.055 * (Math.pow(rgb.r, 0.41666667)) - 0.055;
    }
    else {
      rgb.r = 12.92 * rgb.r;
    }

    if (rgb.g > 0.0031308) {
      rgb.g = 1.055 * (Math.pow(rgb.g, 0.41666667)) - 0.055;
    }
    else {
      rgb.g = 12.92 * rgb.g;
    }

    if (rgb.b > 0.0031308) {
      rgb.b = 1.055 * (Math.pow(rgb.b, 0.41666667)) - 0.055;
    }
    else {
      rgb.b = 12.92 * rgb.b;
    }

    rgb.r = Math.round(rgb.r * 255);
    rgb.g = Math.round(rgb.g * 255);
    rgb.b = Math.round(rgb.b * 255);

    return rgb;
  };

  $.fn.ColorPickerSliders.detectWhichGradientIsSupported = function() {
    var testelement = document.createElement('detectGradientSupport').style;

    try {
      testelement.backgroundImage = 'linear-gradient(to top left, #9f9, white)';
      if (testelement.backgroundImage.indexOf('gradient') !== -1) {
        return 'noprefix';
      }

      testelement.backgroundImage = '-webkit-linear-gradient(left top, #9f9, white)';
      if (testelement.backgroundImage.indexOf('gradient') !== -1) {
        return 'webkit';
      }

      testelement.backgroundImage = '-ms-linear-gradient(left top, #9f9, white)';
      if (testelement.backgroundImage.indexOf('gradient') !== -1) {
        return 'ms';
      }

      testelement.backgroundImage = '-webkit-gradient(linear, left top, right bottom, from(#9f9), to(white))';
      if (testelement.backgroundImage.indexOf('gradient') !== -1) {
        return 'oldwebkit';
      }
    }
    catch (err) {
      try {
        testelement.filter = 'progid:DXImageTransform.Microsoft.gradient(startColorstr="#ffffff",endColorstr="#000000",GradientType=0)';
        if (testelement.filter.indexOf('DXImageTransform') !== -1) {
          return 'filter';
        }
      }
      catch (err) {
      }
    }

    return false;
  };

  $.fn.ColorPickerSliders.svgSupported = function() {
    return !!document.createElementNS && !!document.createElementNS('http://www.w3.org/2000/svg', 'svg').createSVGRect;
  };

})(jQuery);

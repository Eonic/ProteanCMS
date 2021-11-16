define(["require", "exports", "../../utils/format", "./arVat", "./atVat", "./beVat", "./bgVat", "./brVat", "./chVat", "./cyVat", "./czVat", "./deVat", "./dkVat", "./eeVat", "./esVat", "./fiVat", "./frVat", "./gbVat", "./grVat", "./hrVat", "./huVat", "./ieVat", "./isVat", "./itVat", "./ltVat", "./luVat", "./lvVat", "./mtVat", "./nlVat", "./noVat", "./plVat", "./ptVat", "./roVat", "./rsVat", "./ruVat", "./seVat", "./siVat", "./skVat", "./veVat", "./zaVat"], function (require, exports, format_1, arVat_1, atVat_1, beVat_1, bgVat_1, brVat_1, chVat_1, cyVat_1, czVat_1, deVat_1, dkVat_1, eeVat_1, esVat_1, fiVat_1, frVat_1, gbVat_1, grVat_1, hrVat_1, huVat_1, ieVat_1, isVat_1, itVat_1, ltVat_1, luVat_1, lvVat_1, mtVat_1, nlVat_1, noVat_1, plVat_1, ptVat_1, roVat_1, rsVat_1, ruVat_1, seVat_1, siVat_1, skVat_1, veVat_1, zaVat_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    function vat() {
        var COUNTRY_CODES = [
            'AR',
            'AT',
            'BE',
            'BG',
            'BR',
            'CH',
            'CY',
            'CZ',
            'DE',
            'DK',
            'EE',
            'EL',
            'ES',
            'FI',
            'FR',
            'GB',
            'GR',
            'HR',
            'HU',
            'IE',
            'IS',
            'IT',
            'LT',
            'LU',
            'LV',
            'MT',
            'NL',
            'NO',
            'PL',
            'PT',
            'RO',
            'RU',
            'RS',
            'SE',
            'SK',
            'SI',
            'VE',
            'ZA',
        ];
        return {
            validate: function (input) {
                var value = input.value;
                if (value === '') {
                    return { valid: true };
                }
                var opts = Object.assign({}, { message: '' }, input.options);
                var country = value.substr(0, 2);
                if ('function' === typeof opts.country) {
                    country = opts.country.call(this);
                }
                else {
                    country = opts.country;
                }
                if (COUNTRY_CODES.indexOf(country) === -1) {
                    return { valid: true };
                }
                var result = {
                    meta: {},
                    valid: true,
                };
                switch (country.toLowerCase()) {
                    case 'ar':
                        result = (0, arVat_1.default)(value);
                        break;
                    case 'at':
                        result = (0, atVat_1.default)(value);
                        break;
                    case 'be':
                        result = (0, beVat_1.default)(value);
                        break;
                    case 'bg':
                        result = (0, bgVat_1.default)(value);
                        break;
                    case 'br':
                        result = (0, brVat_1.default)(value);
                        break;
                    case 'ch':
                        result = (0, chVat_1.default)(value);
                        break;
                    case 'cy':
                        result = (0, cyVat_1.default)(value);
                        break;
                    case 'cz':
                        result = (0, czVat_1.default)(value);
                        break;
                    case 'de':
                        result = (0, deVat_1.default)(value);
                        break;
                    case 'dk':
                        result = (0, dkVat_1.default)(value);
                        break;
                    case 'ee':
                        result = (0, eeVat_1.default)(value);
                        break;
                    case 'el':
                        result = (0, grVat_1.default)(value);
                        break;
                    case 'es':
                        result = (0, esVat_1.default)(value);
                        break;
                    case 'fi':
                        result = (0, fiVat_1.default)(value);
                        break;
                    case 'fr':
                        result = (0, frVat_1.default)(value);
                        break;
                    case 'gb':
                        result = (0, gbVat_1.default)(value);
                        break;
                    case 'gr':
                        result = (0, grVat_1.default)(value);
                        break;
                    case 'hr':
                        result = (0, hrVat_1.default)(value);
                        break;
                    case 'hu':
                        result = (0, huVat_1.default)(value);
                        break;
                    case 'ie':
                        result = (0, ieVat_1.default)(value);
                        break;
                    case 'is':
                        result = (0, isVat_1.default)(value);
                        break;
                    case 'it':
                        result = (0, itVat_1.default)(value);
                        break;
                    case 'lt':
                        result = (0, ltVat_1.default)(value);
                        break;
                    case 'lu':
                        result = (0, luVat_1.default)(value);
                        break;
                    case 'lv':
                        result = (0, lvVat_1.default)(value);
                        break;
                    case 'mt':
                        result = (0, mtVat_1.default)(value);
                        break;
                    case 'nl':
                        result = (0, nlVat_1.default)(value);
                        break;
                    case 'no':
                        result = (0, noVat_1.default)(value);
                        break;
                    case 'pl':
                        result = (0, plVat_1.default)(value);
                        break;
                    case 'pt':
                        result = (0, ptVat_1.default)(value);
                        break;
                    case 'ro':
                        result = (0, roVat_1.default)(value);
                        break;
                    case 'rs':
                        result = (0, rsVat_1.default)(value);
                        break;
                    case 'ru':
                        result = (0, ruVat_1.default)(value);
                        break;
                    case 'se':
                        result = (0, seVat_1.default)(value);
                        break;
                    case 'si':
                        result = (0, siVat_1.default)(value);
                        break;
                    case 'sk':
                        result = (0, skVat_1.default)(value);
                        break;
                    case 've':
                        result = (0, veVat_1.default)(value);
                        break;
                    case 'za':
                        result = (0, zaVat_1.default)(value);
                        break;
                    default:
                        break;
                }
                var message = (0, format_1.default)(input.l10n && input.l10n.vat ? opts.message || input.l10n.vat.country : opts.message, input.l10n && input.l10n.vat && input.l10n.vat.countries
                    ? input.l10n.vat.countries[country.toUpperCase()]
                    : country.toUpperCase());
                return Object.assign({}, { message: message }, result);
            },
        };
    }
    exports.default = vat;
});

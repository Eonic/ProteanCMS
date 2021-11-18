define(["require", "exports", "../../utils/format", "./arId", "./baId", "./bgId", "./brId", "./chId", "./clId", "./cnId", "./coId", "./czId", "./dkId", "./esId", "./fiId", "./frId", "./hkId", "./hrId", "./idId", "./ieId", "./ilId", "./isId", "./krId", "./ltId", "./lvId", "./meId", "./mkId", "./mxId", "./myId", "./nlId", "./noId", "./peId", "./plId", "./roId", "./rsId", "./seId", "./siId", "./smId", "./thId", "./trId", "./twId", "./uyId", "./zaId"], function (require, exports, format_1, arId_1, baId_1, bgId_1, brId_1, chId_1, clId_1, cnId_1, coId_1, czId_1, dkId_1, esId_1, fiId_1, frId_1, hkId_1, hrId_1, idId_1, ieId_1, ilId_1, isId_1, krId_1, ltId_1, lvId_1, meId_1, mkId_1, mxId_1, myId_1, nlId_1, noId_1, peId_1, plId_1, roId_1, rsId_1, seId_1, siId_1, smId_1, thId_1, trId_1, twId_1, uyId_1, zaId_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    function id() {
        var COUNTRY_CODES = [
            'AR',
            'BA',
            'BG',
            'BR',
            'CH',
            'CL',
            'CN',
            'CO',
            'CZ',
            'DK',
            'EE',
            'ES',
            'FI',
            'FR',
            'HK',
            'HR',
            'ID',
            'IE',
            'IL',
            'IS',
            'KR',
            'LT',
            'LV',
            'ME',
            'MK',
            'MX',
            'MY',
            'NL',
            'NO',
            'PE',
            'PL',
            'RO',
            'RS',
            'SE',
            'SI',
            'SK',
            'SM',
            'TH',
            'TR',
            'TW',
            'UY',
            'ZA',
        ];
        return {
            validate: function (input) {
                if (input.value === '') {
                    return { valid: true };
                }
                var opts = Object.assign({}, { message: '' }, input.options);
                var country = input.value.substr(0, 2);
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
                        result = (0, arId_1.default)(input.value);
                        break;
                    case 'ba':
                        result = (0, baId_1.default)(input.value);
                        break;
                    case 'bg':
                        result = (0, bgId_1.default)(input.value);
                        break;
                    case 'br':
                        result = (0, brId_1.default)(input.value);
                        break;
                    case 'ch':
                        result = (0, chId_1.default)(input.value);
                        break;
                    case 'cl':
                        result = (0, clId_1.default)(input.value);
                        break;
                    case 'cn':
                        result = (0, cnId_1.default)(input.value);
                        break;
                    case 'co':
                        result = (0, coId_1.default)(input.value);
                        break;
                    case 'cz':
                        result = (0, czId_1.default)(input.value);
                        break;
                    case 'dk':
                        result = (0, dkId_1.default)(input.value);
                        break;
                    case 'ee':
                        result = (0, ltId_1.default)(input.value);
                        break;
                    case 'es':
                        result = (0, esId_1.default)(input.value);
                        break;
                    case 'fi':
                        result = (0, fiId_1.default)(input.value);
                        break;
                    case 'fr':
                        result = (0, frId_1.default)(input.value);
                        break;
                    case 'hk':
                        result = (0, hkId_1.default)(input.value);
                        break;
                    case 'hr':
                        result = (0, hrId_1.default)(input.value);
                        break;
                    case 'id':
                        result = (0, idId_1.default)(input.value);
                        break;
                    case 'ie':
                        result = (0, ieId_1.default)(input.value);
                        break;
                    case 'il':
                        result = (0, ilId_1.default)(input.value);
                        break;
                    case 'is':
                        result = (0, isId_1.default)(input.value);
                        break;
                    case 'kr':
                        result = (0, krId_1.default)(input.value);
                        break;
                    case 'lt':
                        result = (0, ltId_1.default)(input.value);
                        break;
                    case 'lv':
                        result = (0, lvId_1.default)(input.value);
                        break;
                    case 'me':
                        result = (0, meId_1.default)(input.value);
                        break;
                    case 'mk':
                        result = (0, mkId_1.default)(input.value);
                        break;
                    case 'mx':
                        result = (0, mxId_1.default)(input.value);
                        break;
                    case 'my':
                        result = (0, myId_1.default)(input.value);
                        break;
                    case 'nl':
                        result = (0, nlId_1.default)(input.value);
                        break;
                    case 'no':
                        result = (0, noId_1.default)(input.value);
                        break;
                    case 'pe':
                        result = (0, peId_1.default)(input.value);
                        break;
                    case 'pl':
                        result = (0, plId_1.default)(input.value);
                        break;
                    case 'ro':
                        result = (0, roId_1.default)(input.value);
                        break;
                    case 'rs':
                        result = (0, rsId_1.default)(input.value);
                        break;
                    case 'se':
                        result = (0, seId_1.default)(input.value);
                        break;
                    case 'si':
                        result = (0, siId_1.default)(input.value);
                        break;
                    case 'sk':
                        result = (0, czId_1.default)(input.value);
                        break;
                    case 'sm':
                        result = (0, smId_1.default)(input.value);
                        break;
                    case 'th':
                        result = (0, thId_1.default)(input.value);
                        break;
                    case 'tr':
                        result = (0, trId_1.default)(input.value);
                        break;
                    case 'tw':
                        result = (0, twId_1.default)(input.value);
                        break;
                    case 'uy':
                        result = (0, uyId_1.default)(input.value);
                        break;
                    case 'za':
                        result = (0, zaId_1.default)(input.value);
                        break;
                    default:
                        break;
                }
                var message = (0, format_1.default)(input.l10n && input.l10n.id ? opts.message || input.l10n.id.country : opts.message, input.l10n && input.l10n.id && input.l10n.id.countries
                    ? input.l10n.id.countries[country.toUpperCase()]
                    : country.toUpperCase());
                return Object.assign({}, { message: message }, result);
            },
        };
    }
    exports.default = id;
});

define(["require", "exports", "./jmbg"], function (require, exports, jmbg_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    function siId(value) {
        return {
            meta: {},
            valid: (0, jmbg_1.default)(value, 'SI'),
        };
    }
    exports.default = siId;
});

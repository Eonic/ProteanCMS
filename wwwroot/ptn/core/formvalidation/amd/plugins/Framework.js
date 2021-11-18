var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (Object.prototype.hasOwnProperty.call(b, p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        if (typeof b !== "function" && b !== null)
            throw new TypeError("Class extends value " + String(b) + " is not a constructor or null");
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
define(["require", "exports", "../core/Plugin", "../utils/classSet", "../utils/closest", "./Message"], function (require, exports, Plugin_1, classSet_1, closest_1, Message_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var Framework = (function (_super) {
        __extends(Framework, _super);
        function Framework(opts) {
            var _this = _super.call(this, opts) || this;
            _this.results = new Map();
            _this.containers = new Map();
            _this.opts = Object.assign({}, {
                defaultMessageContainer: true,
                eleInvalidClass: '',
                eleValidClass: '',
                rowClasses: '',
                rowValidatingClass: '',
            }, opts);
            _this.elementIgnoredHandler = _this.onElementIgnored.bind(_this);
            _this.elementValidatingHandler = _this.onElementValidating.bind(_this);
            _this.elementValidatedHandler = _this.onElementValidated.bind(_this);
            _this.elementNotValidatedHandler = _this.onElementNotValidated.bind(_this);
            _this.iconPlacedHandler = _this.onIconPlaced.bind(_this);
            _this.fieldAddedHandler = _this.onFieldAdded.bind(_this);
            _this.fieldRemovedHandler = _this.onFieldRemoved.bind(_this);
            _this.messagePlacedHandler = _this.onMessagePlaced.bind(_this);
            return _this;
        }
        Framework.prototype.install = function () {
            var _a;
            var _this = this;
            (0, classSet_1.default)(this.core.getFormElement(), (_a = {},
                _a[this.opts.formClass] = true,
                _a['fv-plugins-framework'] = true,
                _a));
            this.core
                .on('core.element.ignored', this.elementIgnoredHandler)
                .on('core.element.validating', this.elementValidatingHandler)
                .on('core.element.validated', this.elementValidatedHandler)
                .on('core.element.notvalidated', this.elementNotValidatedHandler)
                .on('plugins.icon.placed', this.iconPlacedHandler)
                .on('core.field.added', this.fieldAddedHandler)
                .on('core.field.removed', this.fieldRemovedHandler);
            if (this.opts.defaultMessageContainer) {
                this.core.registerPlugin('___frameworkMessage', new Message_1.default({
                    clazz: this.opts.messageClass,
                    container: function (field, element) {
                        var selector = 'string' === typeof _this.opts.rowSelector
                            ? _this.opts.rowSelector
                            : _this.opts.rowSelector(field, element);
                        var groupEle = (0, closest_1.default)(element, selector);
                        return Message_1.default.getClosestContainer(element, groupEle, _this.opts.rowPattern);
                    },
                }));
                this.core.on('plugins.message.placed', this.messagePlacedHandler);
            }
        };
        Framework.prototype.uninstall = function () {
            var _a;
            this.results.clear();
            this.containers.clear();
            (0, classSet_1.default)(this.core.getFormElement(), (_a = {},
                _a[this.opts.formClass] = false,
                _a['fv-plugins-framework'] = false,
                _a));
            this.core
                .off('core.element.ignored', this.elementIgnoredHandler)
                .off('core.element.validating', this.elementValidatingHandler)
                .off('core.element.validated', this.elementValidatedHandler)
                .off('core.element.notvalidated', this.elementNotValidatedHandler)
                .off('plugins.icon.placed', this.iconPlacedHandler)
                .off('core.field.added', this.fieldAddedHandler)
                .off('core.field.removed', this.fieldRemovedHandler);
            if (this.opts.defaultMessageContainer) {
                this.core.off('plugins.message.placed', this.messagePlacedHandler);
            }
        };
        Framework.prototype.onIconPlaced = function (_e) { };
        Framework.prototype.onMessagePlaced = function (_e) { };
        Framework.prototype.onFieldAdded = function (e) {
            var _this = this;
            var elements = e.elements;
            if (elements) {
                elements.forEach(function (ele) {
                    var _a;
                    var groupEle = _this.containers.get(ele);
                    if (groupEle) {
                        (0, classSet_1.default)(groupEle, (_a = {},
                            _a[_this.opts.rowInvalidClass] = false,
                            _a[_this.opts.rowValidatingClass] = false,
                            _a[_this.opts.rowValidClass] = false,
                            _a['fv-plugins-icon-container'] = false,
                            _a));
                        _this.containers.delete(ele);
                    }
                });
                this.prepareFieldContainer(e.field, elements);
            }
        };
        Framework.prototype.onFieldRemoved = function (e) {
            var _this = this;
            e.elements.forEach(function (ele) {
                var _a;
                var groupEle = _this.containers.get(ele);
                if (groupEle) {
                    (0, classSet_1.default)(groupEle, (_a = {},
                        _a[_this.opts.rowInvalidClass] = false,
                        _a[_this.opts.rowValidatingClass] = false,
                        _a[_this.opts.rowValidClass] = false,
                        _a));
                }
            });
        };
        Framework.prototype.prepareFieldContainer = function (field, elements) {
            var _this = this;
            if (elements.length) {
                var type = elements[0].getAttribute('type');
                if ('radio' === type || 'checkbox' === type) {
                    this.prepareElementContainer(field, elements[0]);
                }
                else {
                    elements.forEach(function (ele) { return _this.prepareElementContainer(field, ele); });
                }
            }
        };
        Framework.prototype.prepareElementContainer = function (field, element) {
            var _a;
            var selector = 'string' === typeof this.opts.rowSelector ? this.opts.rowSelector : this.opts.rowSelector(field, element);
            var groupEle = (0, closest_1.default)(element, selector);
            if (groupEle !== element) {
                (0, classSet_1.default)(groupEle, (_a = {},
                    _a[this.opts.rowClasses] = true,
                    _a['fv-plugins-icon-container'] = true,
                    _a));
                this.containers.set(element, groupEle);
            }
        };
        Framework.prototype.onElementValidating = function (e) {
            var _a;
            var elements = e.elements;
            var type = e.element.getAttribute('type');
            var element = 'radio' === type || 'checkbox' === type ? elements[0] : e.element;
            var groupEle = this.containers.get(element);
            if (groupEle) {
                (0, classSet_1.default)(groupEle, (_a = {},
                    _a[this.opts.rowInvalidClass] = false,
                    _a[this.opts.rowValidatingClass] = true,
                    _a[this.opts.rowValidClass] = false,
                    _a));
            }
        };
        Framework.prototype.onElementNotValidated = function (e) {
            this.removeClasses(e.element, e.elements);
        };
        Framework.prototype.onElementIgnored = function (e) {
            this.removeClasses(e.element, e.elements);
        };
        Framework.prototype.removeClasses = function (element, elements) {
            var _a;
            var _this = this;
            var type = element.getAttribute('type');
            var ele = 'radio' === type || 'checkbox' === type ? elements[0] : element;
            elements.forEach(function (ele) {
                var _a;
                (0, classSet_1.default)(ele, (_a = {},
                    _a[_this.opts.eleValidClass] = false,
                    _a[_this.opts.eleInvalidClass] = false,
                    _a));
            });
            var groupEle = this.containers.get(ele);
            if (groupEle) {
                (0, classSet_1.default)(groupEle, (_a = {},
                    _a[this.opts.rowInvalidClass] = false,
                    _a[this.opts.rowValidatingClass] = false,
                    _a[this.opts.rowValidClass] = false,
                    _a));
            }
        };
        Framework.prototype.onElementValidated = function (e) {
            var _a, _b;
            var _this = this;
            var elements = e.elements;
            var type = e.element.getAttribute('type');
            var element = 'radio' === type || 'checkbox' === type ? elements[0] : e.element;
            elements.forEach(function (ele) {
                var _a;
                (0, classSet_1.default)(ele, (_a = {},
                    _a[_this.opts.eleValidClass] = e.valid,
                    _a[_this.opts.eleInvalidClass] = !e.valid,
                    _a));
            });
            var groupEle = this.containers.get(element);
            if (groupEle) {
                if (!e.valid) {
                    this.results.set(element, false);
                    (0, classSet_1.default)(groupEle, (_a = {},
                        _a[this.opts.rowInvalidClass] = true,
                        _a[this.opts.rowValidatingClass] = false,
                        _a[this.opts.rowValidClass] = false,
                        _a));
                }
                else {
                    this.results.delete(element);
                    var isValid_1 = true;
                    this.containers.forEach(function (value, key) {
                        if (value === groupEle && _this.results.get(key) === false) {
                            isValid_1 = false;
                        }
                    });
                    if (isValid_1) {
                        (0, classSet_1.default)(groupEle, (_b = {},
                            _b[this.opts.rowInvalidClass] = false,
                            _b[this.opts.rowValidatingClass] = false,
                            _b[this.opts.rowValidClass] = true,
                            _b));
                    }
                }
            }
        };
        return Framework;
    }(Plugin_1.default));
    exports.default = Framework;
});

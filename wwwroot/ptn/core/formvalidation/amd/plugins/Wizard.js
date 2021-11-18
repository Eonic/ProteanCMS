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
define(["require", "exports", "../core/Plugin", "../utils/classSet", "./Excluded"], function (require, exports, Plugin_1, classSet_1, Excluded_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var Wizard = (function (_super) {
        __extends(Wizard, _super);
        function Wizard(opts) {
            var _this = _super.call(this, opts) || this;
            _this.currentStep = 0;
            _this.numSteps = 0;
            _this.stepIndexes = [];
            _this.opts = Object.assign({}, {
                activeStepClass: 'fv-plugins-wizard--active',
                onStepActive: function () { },
                onStepInvalid: function () { },
                onStepValid: function () { },
                onValid: function () { },
                stepClass: 'fv-plugins-wizard--step',
            }, opts);
            _this.prevStepHandler = _this.onClickPrev.bind(_this);
            _this.nextStepHandler = _this.onClickNext.bind(_this);
            return _this;
        }
        Wizard.prototype.install = function () {
            var _a;
            var _this = this;
            this.core.registerPlugin(Wizard.EXCLUDED_PLUGIN, this.opts.isFieldExcluded ? new Excluded_1.default({ excluded: this.opts.isFieldExcluded }) : new Excluded_1.default());
            var form = this.core.getFormElement();
            this.steps = [].slice.call(form.querySelectorAll(this.opts.stepSelector));
            this.numSteps = this.steps.length;
            this.steps.forEach(function (s) {
                var _a;
                (0, classSet_1.default)(s, (_a = {},
                    _a[_this.opts.stepClass] = true,
                    _a));
            });
            (0, classSet_1.default)(this.steps[0], (_a = {},
                _a[this.opts.activeStepClass] = true,
                _a));
            this.stepIndexes = Array(this.numSteps)
                .fill(0)
                .map(function (_, i) { return i; });
            this.prevButton = form.querySelector(this.opts.prevButton);
            this.nextButton = form.querySelector(this.opts.nextButton);
            this.prevButton.addEventListener('click', this.prevStepHandler);
            this.nextButton.addEventListener('click', this.nextStepHandler);
        };
        Wizard.prototype.uninstall = function () {
            this.core.deregisterPlugin(Wizard.EXCLUDED_PLUGIN);
            this.prevButton.removeEventListener('click', this.prevStepHandler);
            this.nextButton.removeEventListener('click', this.nextStepHandler);
            this.stepIndexes.length = 0;
        };
        Wizard.prototype.getCurrentStep = function () {
            return this.currentStep;
        };
        Wizard.prototype.goToPrevStep = function () {
            var _this = this;
            var prevStep = this.currentStep - 1;
            if (prevStep < 0) {
                return;
            }
            var prevUnskipStep = this.opts.isStepSkipped
                ? this.stepIndexes
                    .slice(0, this.currentStep)
                    .reverse()
                    .find(function (value, _) {
                    return !_this.opts.isStepSkipped({
                        currentStep: _this.currentStep,
                        numSteps: _this.numSteps,
                        targetStep: value,
                    });
                })
                : prevStep;
            this.goToStep(prevUnskipStep);
            this.onStepActive();
        };
        Wizard.prototype.goToNextStep = function () {
            var _this = this;
            this.core.validate().then(function (status) {
                if (status === 'Valid') {
                    var nextStep = _this.currentStep + 1;
                    if (nextStep >= _this.numSteps) {
                        _this.currentStep = _this.numSteps - 1;
                    }
                    else {
                        var nextUnskipStep = _this.opts.isStepSkipped
                            ? _this.stepIndexes.slice(nextStep, _this.numSteps).find(function (value, _) {
                                return !_this.opts.isStepSkipped({
                                    currentStep: _this.currentStep,
                                    numSteps: _this.numSteps,
                                    targetStep: value,
                                });
                            })
                            : nextStep;
                        nextStep = nextUnskipStep;
                        _this.goToStep(nextStep);
                    }
                    _this.onStepActive();
                    _this.onStepValid();
                    if (nextStep === _this.numSteps) {
                        _this.onValid();
                    }
                }
                else if (status === 'Invalid') {
                    _this.onStepInvalid();
                }
            });
        };
        Wizard.prototype.goToStep = function (index) {
            var _a, _b;
            (0, classSet_1.default)(this.steps[this.currentStep], (_a = {},
                _a[this.opts.activeStepClass] = false,
                _a));
            (0, classSet_1.default)(this.steps[index], (_b = {},
                _b[this.opts.activeStepClass] = true,
                _b));
            this.currentStep = index;
        };
        Wizard.prototype.onClickPrev = function () {
            this.goToPrevStep();
        };
        Wizard.prototype.onClickNext = function () {
            this.goToNextStep();
        };
        Wizard.prototype.onStepActive = function () {
            var e = {
                numSteps: this.numSteps,
                step: this.currentStep,
            };
            this.core.emit('plugins.wizard.step.active', e);
            this.opts.onStepActive(e);
        };
        Wizard.prototype.onStepValid = function () {
            var e = {
                numSteps: this.numSteps,
                step: this.currentStep,
            };
            this.core.emit('plugins.wizard.step.valid', e);
            this.opts.onStepValid(e);
        };
        Wizard.prototype.onStepInvalid = function () {
            var e = {
                numSteps: this.numSteps,
                step: this.currentStep,
            };
            this.core.emit('plugins.wizard.step.invalid', e);
            this.opts.onStepInvalid(e);
        };
        Wizard.prototype.onValid = function () {
            var e = {
                numSteps: this.numSteps,
            };
            this.core.emit('plugins.wizard.valid', e);
            this.opts.onValid(e);
        };
        Wizard.EXCLUDED_PLUGIN = '___wizardExcluded';
        return Wizard;
    }(Plugin_1.default));
    exports.default = Wizard;
});

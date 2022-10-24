! function (e) {
    var t = {};

    function i(n) {
        if (t[n]) return t[n].exports;
        var s = t[n] = {
            i: n,
            l: !1,
            exports: {}
        };
        return e[n].call(s.exports, s, s.exports, i), s.l = !0, s.exports
    }
    i.m = e, i.c = t, i.d = function (e, t, n) {
        i.o(e, t) || Object.defineProperty(e, t, {
            enumerable: !0,
            get: n
        })
    }, i.r = function (e) {
        "undefined" != typeof Symbol && Symbol.toStringTag && Object.defineProperty(e, Symbol.toStringTag, {
            value: "Module"
        }), Object.defineProperty(e, "__esModule", {
            value: !0
        })
    }, i.t = function (e, t) {
        if (1 & t && (e = i(e)), 8 & t) return e;
        if (4 & t && "object" == typeof e && e && e.__esModule) return e;
        var n = Object.create(null);
        if (i.r(n), Object.defineProperty(n, "default", {
            enumerable: !0,
            value: e
        }), 2 & t && "string" != typeof e)
            for (var s in e) i.d(n, s, function (t) {
                return e[t]
            }.bind(null, s));
        return n
    }, i.n = function (e) {
        var t = e && e.__esModule ? function () {
            return e.default
        } : function () {
            return e
        };
        return i.d(t, "a", t), t
    }, i.o = function (e, t) {
        return Object.prototype.hasOwnProperty.call(e, t)
    }, i.p = "", i(i.s = 93)
}({
    1: function (e, t, i) {
        "use strict";
        t.a = function () {
            var e = [];
            return {
                addHandler: function (t) {
                    if ("function" == typeof t) {
                        for (var i = 0; i < e.length; i++)
                            if (e[i] === t) return void console.log("The handler already in the list");
                        e.push(t)
                    } else console.log("The handler must be a function")
                },
                removeHandler: function (t) {
                    for (var i = 0; i < e.length; i++)
                        if (e[i] === t) return void e.splice(i, 1);
                    console.log("could not find observer in list of observers")
                },
                trigger: function (t) {
                    for (var i = e.slice(0), n = 0; n < i.length; n++) i[n](t)
                }
            }
        }
    },
    2: function (e, t, i) {
        "use strict";

        function n(e) {
            return (n = "function" == typeof Symbol && "symbol" == typeof Symbol.iterator ? function (e) {
                return typeof e
            } : function (e) {
                return e && "function" == typeof Symbol && e.constructor === Symbol && e !== Symbol.prototype ? "symbol" : typeof e
            })(e)
        }

        function s(e, t) {
            for (var i = 0; i < t.length; i++) {
                var n = t[i];
                n.enumerable = n.enumerable || !1, n.configurable = !0, "value" in n && (n.writable = !0), Object.defineProperty(e, n.key, n)
            }
        }
        var a = function () {
            function e() {
                ! function (e, t) {
                    if (!(e instanceof t)) throw new TypeError("Cannot call a class as a function")
                }(this, e)
            }
            var t, i, a;
            return t = e, (i = [{
                key: "isNumber",
                value: function (e) {
                    return !!(("number" == typeof e || "string" == typeof e) & isFinite(e))
                }
            }, {
                key: "toNumber",
                value: function (e) {
                    return !!this.isNumber(e) && +e
                }
            }, {
                key: "isDOMEl",
                value: function (e) {
                    return !(!this.isObject(e) || e.constructor === Object || !this.isNumber(e.nodeType) || 1 != +e.nodeType)
                }
            }, {
                key: "isObject",
                value: function (e) {
                    return "object" === n(e) && null !== e
                }
            }]) && s(t.prototype, i), a && s(t, a), e
        }();
        t.a = new a
    },
    93: function (e, t, i) {
        "use strict";
        i.r(t);
        var n = i(1),
            s = i(2);

        function a(e, t) {
            for (var i = 0; i < t.length; i++) {
                var n = t[i];
                n.enumerable = n.enumerable || !1, n.configurable = !0, "value" in n && (n.writable = !0), Object.defineProperty(e, n.key, n)
            }
        }
        var o = s.a,
            l = n.a,
            u = function () {
                function e() {
                    ! function (e, t) {
                        if (!(e instanceof t)) throw new TypeError("Cannot call a class as a function")
                    }(this, e),
                      
                        this.minLimit = 0,
                        this.maxLimit = 200,
                        this.valuesCount = this.maxLimit - this.minLimit,
                        this.singleValue = 50,
                        this.rangeValueMin = 20,
                        this.rangeValueMax = 80,
                        this.singleSelected = (this.singleValue - this.minLimit) / this.valuesCount * 100,
                        this.rangeSelected = (this.rangeValueMax - this.rangeValueMin) / this.valuesCount * 100,
                        this.step = 1, this.type = "single", this.typeConstants = {
                        singleValue: "single",
                        rangeValue: "range"
                    }, this._addEvents()
                }
                var t, i, n;
                return t = e, (i = [{
                    key: "recalculateValue",
                    value: function () {
                        return this.type === this.typeConstants.singleValue ? this.setSingleValue(null, !0) : this.type === this.typeConstants.rangeValue ? this.setRangeValue(null, !0) : void 0
                    }
                }, {
                    key: "setAValueTo",
                    value: function (e, t, i) {
                        var n = Math.round(+e / this.step) * this.step;
                        n < this.minLimit ? (this[t] = this.minLimit, i || console.log("The value was equated to the minimum, because it is less than the minimum value.")) : n > this.maxLimit ? (this[t] = this.maxLimit, i || console.log("The value was equated to the maximum, because it is more than the maximum value.")) : this[t] = n
                    }
                }, {
                    key: "setType",
                    value: function (e) {
                        for (var t in this.typeConstants)
                            if (e === this.typeConstants[t]) return this.type = e, this.typeUpdateEvent.trigger({
                                value: this.type,
                                typeConstants: Object.assign({}, this.typeConstants)
                            }), {
                                value: this.type,
                                typeConstants: Object.assign({}, this.typeConstants)
                            }
                    }
                }, {
                    key: "setLimits",
                    value: function (e, t) {
                        o.isObject(e) || (e = {});
                        var i = o.isNumber(e.minLimit) ? +e.minLimit : this.minLimit,
                            n = o.isNumber(e.maxLimit) ? +e.maxLimit : this.maxLimit;
                        return i < n && (this.minLimit = i, this.maxLimit = n), i === n && (this.minLimit = i, this.maxLimit = n + 1, t || console.log("Maximum limit was increased by 1, because the minimum limit is equal to the maximum limit.")), i > n && (this.minLimit = n, this.maxLimit = i, t || console.log("Limits was reversed, because the maximum limit is less than the minimum limit.")), this.valuesCount = this.maxLimit - this.minLimit, this.limitsUpdateEvent.trigger({
                            minLimit: this.minLimit,
                            maxLimit: this.maxLimit,
                            valuesCount: this.valuesCount
                        }), {
                            minLimit: this.minLimit,
                            maxLimit: this.maxLimit,
                            valuesCount: this.valuesCount
                        }
                    }
                }, {
                    key: "setStep",
                    value: function (e) {
                        if (o.isNumber(e) && !(+e <= 0)) return this.step = +e, this.stepUpdateEvent.trigger(this.step), this.step
                    }
                }, {
                    key: "setSingleValue",
                    value: function (e, t) {
                        return e = o.isNumber(e) ? +e : this.singleValue, this.setAValueTo(e, "singleValue", t), this.singleSelected = (this.singleValue - this.minLimit) / this.valuesCount * 100, this.valueUpdateEvent.trigger({
                            value: this.singleValue,
                            selected: this.singleSelected
                        }), {
                            value: this.singleValue,
                            selected: this.singleSelected
                        }
                    }
                }, {
                    key: "setRangeValue",
                    value: function (e, t) {
                        o.isObject(e) || (e = {});
                        var i = o.isNumber(e.minValue) ? +e.minValue : this.rangeValueMin,
                            n = o.isNumber(e.maxValue) ? +e.maxValue : this.rangeValueMax;
                        if (i === n && (n += this.step, t || console.log("The maximum value was increased by step size, because minimum value is equal to maximum value.")), i > n) {
                            var s = n;
                            n = i, i = s, t || console.log("The values was reversed, because maximum value is less than minimum value.")
                        }
                        return this.setAValueTo(i, "rangeValueMin", t), this.setAValueTo(n, "rangeValueMax", t), this.rangeSelected = (this.rangeValueMax - this.rangeValueMin) / this.valuesCount * 100, this.valueUpdateEvent.trigger({
                            minValue: this.rangeValueMin,
                            maxValue: this.rangeValueMax,
                            selected: this.rangeSelected
                        }), {
                            minValue: this.rangeValueMin,
                            maxValue: this.rangeValueMax,
                            selected: this.rangeSelected
                        }
                    }
                }, {
                    key: "setNearestValue",
                    value: function (e, t, i) {
                        if (o.isNumber(e)) return e = !1 === t ? Math.round(+e) : Math.round(this.valuesCount * +e / 100 + this.minLimit), this.type === this.typeConstants.singleValue ? this.setSingleValue(e, i) : this.type === this.typeConstants.rangeValue ? e < (this.rangeValueMin + this.rangeValueMax) / 2 ? this.setRangeValue({
                            minValue: +e
                        }, !0) : this.setRangeValue({
                            maxValue: +e
                        }, !0) : void 0
                    }
                }, {
                    key: "getType",
                    value: function () {
                        return {
                            value: this.type,
                            typeConstants: Object.assign({}, this.typeConstants)
                        }
                    }
                }, {
                    key: "getLimits",
                    value: function () {
                        return {
                            minLimit: this.minLimit,
                            maxLimit: this.maxLimit,
                            valuesCount: this.valuesCount
                        }
                    }
                }, {
                    key: "getStep",
                    value: function () {
                        return this.step
                    }
                }, {
                    key: "getValue",
                    value: function () {
                        return this.type === this.typeConstants.singleValue ? {
                            value: this.singleValue,
                            selected: this.singleSelected
                        } : this.type === this.typeConstants.rangeValue ? {
                            minValue: this.rangeValueMin,
                            maxValue: this.rangeValueMax,
                            selected: this.rangeSelected
                        } : void 0
                    }
                }, {
                    key: "_addEvents",
                    value: function () {
                        this.valueUpdateEvent = l(), this.limitsUpdateEvent = l(), this.stepUpdateEvent = l(), this.percentageUpdateEvent = l(), this.typeUpdateEvent = l()
                    }
                }]) && a(t.prototype, i), n && a(t, n), e
            }();

        function h(e, t) {
            for (var i = 0; i < t.length; i++) {
                var n = t[i];
                n.enumerable = n.enumerable || !1, n.configurable = !0, "value" in n && (n.writable = !0), Object.defineProperty(e, n.key, n)
            }
        }
        var r = n.a,
            d = s.a,
            v = function () {
                function e() {
                    ! function (e, t) {
                        if (!(e instanceof t)) throw new TypeError("Cannot call a class as a function")
                    }(this, e), this.$roots = $(document.body), this.divisionsCount = 5, this.valueNoteDisplay = !0, this.theme = {
                        value: "default",
                        className: "theme",
                        oldValue: null
                    }, this.direction = {
                        value: "horizontal",
                        className: "direction",
                        oldValue: null
                    }, this.directionConstants = {
                        horizontalValue: "horizontal",
                        verticalValue: "vertical"
                    }, this.$base = $("<div class='wrunner'>"), this.$outer = $("<div class='wrunner__outer'>").appendTo(this.$base), this.$path = $("<div class='wrunner__path'>").appendTo(this.$outer), this.$pathPassed = $("<div class='wrunner__pathPassed'>").appendTo(this.$path), this.$handle = $("<div class='wrunner__handle'>"), this.$handleMin = $("<div class='wrunner__handle'>"), this.$handleMax = $("<div class='wrunner__handle'>"), this.$valueNote = $("<div class='wrunner__valueNote'>"), this.$valueNoteMin = $("<div class='wrunner__valueNote'>"), this.$valueNoteMax = $("<div class='wrunner__valueNote'>"), this.$divisions = $("<div class='wrunner__divisions'>").appendTo(this.$outer), this.divisionsList = [], this._addEvents(), this._addListenners()
                }
                var t, i, n;
                return t = e, (i = [{
                    key: "updateDOM",
                    value: function (e) {
                        e.value === e.typeConstants.singleValue && (this.$handleMin.detach(), this.$handleMax.detach(), this.$valueNoteMin.detach(), this.$valueNoteMax.detach(), this.$handle.appendTo(this.$path), this.$valueNote.appendTo(this.$outer)), e.value === e.typeConstants.rangeValue && (this.$handle.detach(), this.$valueNote.detach(), this.$handleMin.appendTo(this.$path), this.$handleMax.appendTo(this.$path), this.$valueNoteMin.appendTo(this.$outer), this.$valueNoteMax.appendTo(this.$outer))
                    }
                }, {
                    key: "append",
                    value: function () {
                        return this.$base.appendTo(this.$roots), this.$roots
                    }
                }, {
                    key: "applyStyles",
                    value: function () {
                        var e = [this.theme, this.direction];
                        [this.$base, this.$outer, this.$path, this.$pathPassed, this.$divisions, this.$handle, this.$handleMin, this.$handleMax, this.$valueNote, this.$valueNoteMin, this.$valueNoteMax].concat(this.divisionsList).forEach((function (t) {
                            for (var i in e) {
                                var n = t[0].classList[0],
                                    s = e[i].oldValue,
                                    a = e[i].value;
                                s && t.removeClass(n + "_" + e[i].className + "_" + s), t.addClass(n + "_" + e[i].className + "_" + a)
                            }
                        }))
                    }
                }, {
                    key: "drawValue",
                    value: function (e, t, i) {
                        var n, s, a = e.selected,
                            o = this.direction.value,
                            l = this.directionConstants,
                            u = i.value,
                            h = i.typeConstants;
                        if ([this.$pathPassed, this.$handle, this.$handleMin, this.$handleMax, this.$valueNote, this.$valueNoteMin, this.$valueNoteMax].forEach((function (e) {
                            e.attr("style", "")
                        })), u === h.singleValue && (this.$valueNote.text(e.value), o === l.horizontalValue && (this.$pathPassed.css("width", a + "%"), this.$handle.css("left", a + "%"), n = this.$path.outerWidth(), s = this.$valueNote.outerWidth(), this.$valueNote.css("left", (n * a / 100 - s / 2) / n * 100 + "%")), o === l.verticalValue && (this.$pathPassed.css("height", a + "%"), this.$handle.css("top", 100 - a + "%"), n = this.$path.outerHeight(), s = this.$valueNote.outerHeight(), this.$valueNote.css("top", 100 - (n * a / 100 + s / 2) / n * 100 + "%"))), u === h.rangeValue) {
                            var r, d, v = (e.minValue - t.minLimit) / t.valuesCount * 100;
                            this.$valueNoteMin.text(e.minValue), this.$valueNoteMax.text(e.maxValue), o === l.horizontalValue && (this.$pathPassed.css("width", a + "%"), this.$pathPassed.css("left", v + "%"), this.$handleMin.css("left", v + "%"), this.$handleMax.css("left", v + a + "%"), n = this.$path.outerWidth(), r = this.$valueNoteMin.outerWidth(), d = this.$valueNoteMax.outerWidth(), this.$valueNoteMin.css("left", (n * v / 100 - r / 2) / n * 100 + "%"), this.$valueNoteMax.css("left", (n * (v + a) / 100 - d / 2) / n * 100 + "%")), o === l.verticalValue && (this.$pathPassed.css("height", a + "%"), this.$pathPassed.css("top", 100 - a - v + "%"), this.$handleMax.css("top", 100 - v - a + "%"), this.$handleMin.css("top", 100 - v + "%"), n = this.$path.outerHeight(), r = this.$valueNoteMin.outerHeight(), d = this.$valueNoteMax.outerHeight(), this.$valueNoteMin.css("top", 100 - (n * v / 100 + r / 2) / n * 100 + "%"), this.$valueNoteMax.css("top", 100 - (n * (v + a) / 100 + d / 2) / n * 100 + "%"))
                        }
                    }
                }, {
                    key: "applyValueNoteDisplay",
                    value: function () {
                        var e = this;
                        [this.$valueNote, this.$valueNoteMin, this.$valueNoteMax].forEach((function (t) {
                            var i = t[0].classList[0];
                            t.removeClass(i + "_display_" + (e.valueNoteDisplay ? "hidden" : "visible")).addClass(i + "_display_" + (e.valueNoteDisplay ? "visible" : "hidden"))
                        }))
                    }
                }, {
                    key: "generateDivisions",
                    value: function () {
                        this.$divisions.empty(), this.divisionsList.length = 0;
                        for (var e = this.divisionsCount; e > 0; e--) {
                            var t = $("<div class='wrunner__division'>");
                            this.divisionsList.push(t), t.appendTo(this.$divisions)
                        }
                    }
                }, {
                    key: "setRoots",
                    value: function (e) {
                        if (d.isDOMEl($(e)[0])) return this.$roots = $(e), this.rootsUpdateEvent.trigger(this.$roots), this.$roots
                    }
                }, {
                    key: "setDivisionsCount",
                    value: function (e, t) {
                        if (d.isNumber(e) && !(e < 0)) return 1 == (e = Math.round(+e)) && (e++, t || console.log("Count was increased by one, cause it may not be equal to one.")), this.divisionsCount = +e, this.divisionsCountUpdateEvent.trigger(this.divisionsCount), this.divisionsCount
                    }
                }, {
                    key: "setTheme",
                    value: function (e) {
                        if ("string" == typeof e) return this.theme.oldValue = this.theme.value, this.theme.value = e, this.themeUpdateEvent.trigger(this.theme.value), this.theme.value
                    }
                }, {
                    key: "setDirection",
                    value: function (e) {
                        if ("string" == typeof e)
                            for (var t in this.directionConstants)
                                if (e === this.directionConstants[t]) return this.direction.oldValue = this.direction.value, this.direction.value = e, this.directionUpdateEvent.trigger({
                                    value: this.direction.value,
                                    constants: Object.assign({}, this.directionConstants)
                                }), {
                                    value: this.direction.value,
                                    constants: Object.assign({}, this.directionConstants)
                                }
                    }
                }, {
                    key: "setValueNoteDisplay",
                    value: function (e) {
                        if ("boolean" == typeof e) return this.valueNoteDisplay = e, this.valueNoteDisplayUpdateEvent.trigger(this.valueNoteDisplay), this.valueNoteDisplay
                    }
                }, {
                    key: "getRoots",
                    value: function () {
                        return this.$roots
                    }
                }, {
                    key: "getTheme",
                    value: function () {
                        return this.theme.value
                    }
                }, {
                    key: "getDirection",
                    value: function () {
                        return {
                            value: this.direction.value,
                            constants: Object.assign({}, this.directionConstants)
                        }
                    }
                }, {
                    key: "getValueNoteDisplay",
                    value: function () {
                        return this.valueNoteDisplay
                    }
                }, {
                    key: "getDivisionsCount",
                    value: function () {
                        return this.divisionsCount
                    }
                }, {
                    key: "_addEvents",
                    value: function () {
                        this.UIMouseActionEvent = r(), this.rootsUpdateEvent = r(), this.themeUpdateEvent = r(), this.directionUpdateEvent = r(), this.valueNoteDisplayUpdateEvent = r(), this.divisionsCountUpdateEvent = r()
                    }
                }, {
                    key: "_addListenners",
                    value: function () {
                        $(this.$path).on("mousedown.downAction", this._mouseDownActionHandler.bind(this))
                    }
                }, {
                    key: "_mouseDownActionHandler",
                    value: function (e) {
                        if (0 === e.button) {
                            var t = !1,
                                i = function (e) {
                                    var t, i, n, s, a = this.direction.value,
                                        o = this.directionConstants;
                                    a === o.horizontalValue && (t = this.$path.outerWidth(), i = this.$path[0].getBoundingClientRect().left, s = e.clientX), a === o.verticalValue && (t = this.$path.outerHeight(), i = this.$path[0].getBoundingClientRect().top, s = e.clientY), n = i + t, s < i - 10 || s > n + 10 || (a === o.horizontalValue && this.UIMouseActionEvent.trigger((s - i) / t * 100), a === o.verticalValue && this.UIMouseActionEvent.trigger(100 - (s - i) / t * 100))
                                }.bind(this),
                                n = function (e) {
                                    var n = $(e.target);
                                    $(document.body).off("mousemove.moveAction", i), t || n.is(this.$handle) || n.is(this.$handleMin) || n.is(this.$handleMax) || i(e)
                                }.bind(this);
                            $(document.body).one("mousemove", (function () {
                                return t = !0
                            })), $(document.body).on("mousemove.moveAction", i), $(document.body).one("mouseup", n)
                        }
                    }
                }]) && h(t.prototype, i), n && h(t, n), e
            }();

        function p(e, t) {
            for (var i = 0; i < t.length; i++) {
                var n = t[i];
                n.enumerable = n.enumerable || !1, n.configurable = !0, "value" in n && (n.writable = !0), Object.defineProperty(e, n.key, n)
            }
        }
        var c = function () {
            function e(t) {
                ! function (e, t) {
                    if (!(e instanceof t)) throw new TypeError("Cannot call a class as a function")
                }(this, e), t = t || {}, this.model = t.model, this.view = t.view, this._applyDefaultEvents(), this._applyUserEvents(t.userOptions), this._applyUserOptions(t.userOptions), this._initInstance(), this._triggerEvents()
            }
            var t, i, n;
            return t = e, (i = [{
                key: "onValueUpdate",
                value: function (e) {
                    this.model.valueUpdateEvent.addHandler(e)
                }
            }, {
                key: "onStepUpdate",
                value: function (e) {
                    this.model.stepUpdateEvent.addHandler(e)
                }
            }, {
                key: "onLimitsUpdate",
                value: function (e) {
                    this.model.limitsUpdateEvent.addHandler(e)
                }
            }, {
                key: "onTypeUpdate",
                value: function (e) {
                    this.model.typeUpdateEvent.addHandler(e)
                }
            }, {
                key: "onThemeUpdate",
                value: function (e) {
                    this.view.themeUpdateEvent.addHandler(e)
                }
            }, {
                key: "onDirectionUpdate",
                value: function (e) {
                    this.view.directionUpdateEvent.addHandler(e)
                }
            }, {
                key: "onValueNoteDisplayUpdate",
                value: function (e) {
                    this.view.valueNoteDisplayUpdateEvent.addHandler(e)
                }
            }, {
                key: "onRootsUpdate",
                value: function (e) {
                    this.view.rootsUpdateEvent.addHandler(e)
                }
            }, {
                key: "onDivisionsCountUpdate",
                value: function (e) {
                    this.view.divisionsCountUpdateEvent.addHandler(e)
                }
            }, {
                key: "_applyDefaultEvents",
                value: function () {
                    this.model.typeUpdateEvent.addHandler(function (e) {
                        this.view.updateDOM(this.model.getType()), this.model.recalculateValue()
                    }.bind(this)), this.model.limitsUpdateEvent.addHandler(function (e) {
                        this.model.recalculateValue()
                    }.bind(this)), this.model.stepUpdateEvent.addHandler(function (e) {
                        this.model.recalculateValue()
                    }.bind(this)), this.model.valueUpdateEvent.addHandler(function (e) {
                        this.view.drawValue(this.model.getValue(), this.model.getLimits(), this.model.getType())
                    }.bind(this)), this.view.rootsUpdateEvent.addHandler(function (e) {
                        this.view.append(), this.view.drawValue(this.model.getValue(), this.model.getLimits(), this.model.getType())
                    }.bind(this)), this.view.UIMouseActionEvent.addHandler(function (e) {
                        this.model.setNearestValue(e, !0, !0)
                    }.bind(this)), this.view.themeUpdateEvent.addHandler(function (e) {
                        this.view.applyStyles(), this.view.drawValue(this.model.getValue(), this.model.getLimits(), this.model.getType())
                    }.bind(this)), this.view.directionUpdateEvent.addHandler(function (e) {
                        this.view.applyStyles(), this.view.drawValue(this.model.getValue(), this.model.getLimits(), this.model.getType())
                    }.bind(this)), this.view.valueNoteDisplayUpdateEvent.addHandler(function (e) {
                        this.view.applyValueNoteDisplay(), this.view.drawValue(this.model.getValue(), this.model.getLimits(), this.model.getType())
                    }.bind(this)), this.view.divisionsCountUpdateEvent.addHandler(function (e) {
                        this.view.generateDivisions(), this.view.applyStyles()
                    }.bind(this))
                }
            }, {
                key: "_initInstance",
                value: function () {
                    this.view.updateDOM(this.model.getType()), this.view.generateDivisions(), this.view.applyValueNoteDisplay(), this.view.applyStyles(), this.view.append()
                }
            }, {
                key: "_applyUserEvents",
                value: function (e) {
                    void 0 !== (e = e || {}).onTypeUpdate && this.onTypeUpdate(e.onTypeUpdate), void 0 !== e.onLimitsUpdate && this.onLimitsUpdate(e.onLimitsUpdate), void 0 !== e.onStepUpdate && this.onStepUpdate(e.onStepUpdate), void 0 !== e.onValueUpdate && this.onValueUpdate(e.onValueUpdate), void 0 !== e.onRootsUpdate && this.onRootsUpdate(e.onRootsUpdate), void 0 !== e.onThemeUpdate && this.onThemeUpdate(e.onThemeUpdate), void 0 !== e.onDirectionUpdate && this.onDirectionUpdate(e.onDirectionUpdate), void 0 !== e.onDivisionsCountUpdate && this.onDivisionsCountUpdate(e.onDivisionsCountUpdate), void 0 !== e.onValueNoteDisplayUpdate && this.onValueNoteDisplayUpdate(e.onValueNoteDisplayUpdate)
                }
            }, {
                key: "_applyUserOptions",
                value: function (e) {
                    e = e || {}, this.view.setRoots(e.$roots), void 0 !== e.type && this.model.setType(e.type), void 0 !== e.limits && this.model.setLimits(e.limits), void 0 !== e.step && this.model.setStep(e.step), void 0 !== e.singleValue && this.model.setSingleValue(e.singleValue), void 0 !== e.rangeValue && this.model.setRangeValue(e.rangeValue), void 0 !== e.divisionsCount && this.view.setDivisionsCount(e.divisionsCount), void 0 !== e.theme && this.view.setTheme(e.theme), void 0 !== e.direction && this.view.setDirection(e.direction), void 0 !== e.valueNoteDisplay && this.view.setValueNoteDisplay(e.valueNoteDisplay)
                }
            }, {
                key: "_triggerEvents",
                value: function () {
                    this.model.valueUpdateEvent.trigger(this.model.getValue()), this.model.typeUpdateEvent.trigger(this.model.getType()), this.model.stepUpdateEvent.trigger(this.model.step), this.model.limitsUpdateEvent.trigger(this.model.getLimits()), this.view.themeUpdateEvent.trigger(this.view.getTheme()), this.view.directionUpdateEvent.trigger(this.view.getDirection()), this.view.valueNoteDisplayUpdateEvent.trigger(this.view.getValueNoteDisplay()), this.view.rootsUpdateEvent.trigger(this.view.getRoots()), this.view.divisionsCountUpdateEvent.trigger(this.view.getDivisionsCount())
                }
            }]) && p(t.prototype, i), n && p(t, n), e
        }();

        function m(e, t, i) {
            return t in e ? Object.defineProperty(e, t, {
                value: i,
                enumerable: !0,
                configurable: !0,
                writable: !0
            }) : e[t] = i, e
        } ! function (e) {
            e.fn.wRunner = function (e) {
                (e = e || {}).$roots = this;
                var t = new function () {
                    this.Model = u, this.View = v, this.Presenter = c
                };
                return function () {
                    var i = new t.Model,
                        n = new t.View,
                        s = new t.Presenter({
                            model: i,
                            view: n,
                            userOptions: e
                        }),
                        a = {
                            setType: i.setType.bind(i),
                            setLimits: i.setLimits.bind(i),
                            setStep: i.setStep.bind(i),
                            setSingleValue: i.setSingleValue.bind(i),
                            setRangeValue: i.setRangeValue.bind(i),
                            setNearestValue: i.setNearestValue.bind(i),
                            setRoots: n.setRoots.bind(n),
                            setTheme: n.setTheme.bind(n),
                            setDirection: n.setDirection.bind(n),
                            setValueNoteDisplay: n.setValueNoteDisplay.bind(n),
                            setDivisionsCount: n.setDivisionsCount.bind(n)
                        },
                        o = {
                            getType: i.getType.bind(i),
                            getLimits: i.getLimits.bind(i),
                            getStep: i.getStep.bind(i),
                            getValue: i.getValue.bind(i),
                            getRoots: n.getRoots.bind(n),
                            getTheme: n.getTheme.bind(n),
                            getDirection: n.getDirection.bind(n),
                            getValueNoteDisplay: n.getValueNoteDisplay.bind(n),
                            getDivisionsCount: n.getDivisionsCount.bind(n)
                        },
                        l = {
                            onTypeUpdate: s.onTypeUpdate.bind(s),
                            onLimitsUpdate: s.onLimitsUpdate.bind(s),
                            onStepUpdate: s.onStepUpdate.bind(s),
                            onValueUpdate: s.onValueUpdate.bind(s),
                            onRootsUpdate: s.onRootsUpdate.bind(s),
                            onThemeUpdate: s.onThemeUpdate.bind(s),
                            onDirectionUpdate: s.onDirectionUpdate.bind(s),
                            onDivisionsCountUpdate: s.onDivisionsCountUpdate.bind(s),
                            onValueNoteDisplayUpdate: s.onValueNoteDisplayUpdate.bind(s)
                        };
                    return function (e) {
                        for (var t = 1; t < arguments.length; t++) {
                            var i = null != arguments[t] ? arguments[t] : {},
                                n = Object.keys(i);
                            "function" == typeof Object.getOwnPropertySymbols && (n = n.concat(Object.getOwnPropertySymbols(i).filter((function (e) {
                                return Object.getOwnPropertyDescriptor(i, e).enumerable
                            })))), n.forEach((function (t) {
                                m(e, t, i[t])
                            }))
                        }
                        return e
                    }({}, a, o, l)
                }()
            }
        }($)
    }
});
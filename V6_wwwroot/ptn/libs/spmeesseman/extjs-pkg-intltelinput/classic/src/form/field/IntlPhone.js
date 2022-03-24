Ext.define('Ext.ux.form.field.IntlPhone', 
{
    extend: 'Ext.form.field.Text',
    xtype: 'intlphonefield',

    requires: [
    ],
       
    inputWrapCls: undefined,        // let the intl-tel-field do its own input field styling
    inputWrapFocusCls: undefined,   // this is probably the wrong way to do this, but it works
    inputWrapInvalidCls: undefined, // these input wraps set overflow:hidden and cut off the flag list
    liquidLayout: false,
    
    fieldLabel: 'Phone Number',
    tooltip: 'Select your country and phone numnber.  Your country code will be automatically determined',
    input: null,
    inputId: 'intltelinput',
    inputType: 'tel',
    iti: null,
    itiInstanceNumber: 0,

    config:
    {
        cleanRawValue: true,
        logFn: Ext.emptyFn,
        logValueFn: Ext.emptyFn
    },

    statics:
    {
        idCounter: 0
    },

    initComponent: function()
    {
        this.callParent(arguments);
        //
        // Dyanmically set the input id so we can have multiple components rendered at the same time
        //
        this.itiInstanceNumber = Ext.ux.form.field.IntlPhone.idCounter;
        Ext.ux.form.field.IntlPhone.idCounter++;
        this.inputId = 'intltelinput-el-' + this.itiInstanceNumber.toString();
    },

    listeners:
    {
        afterrender: function(cmp, eopts)
        {
            cmp.logFn('IntlTelField after render event', 1);
            cmp.logValueFn('   cmp', cmp, 3);

            if (!window.intlTelInput) {
                console.error('Could not find intl-tel-input window definition');
                return;
            }
            //
            // Should be able to find the component...
            //
            cmp.input = document.querySelector('#' + cmp.inputId);
            if (cmp.input) 
            {
                var options = {
                    separateDialCode: true,
                    preferredCountries: [ "us", "ca", "gb" ],
                    utilsScript: Ext.manifest.resources.base + '/resources/intltelinput/js/utils.js'
                };

                cmp.logFn('   initializing intl-tel-input element', 1);
                cmp.logValueFn('      inputId', cmp.inputId, 2);
                cmp.logValueFn('      utils script location', options.utilsScript, 2);
                cmp.logValueFn('      options', options, 3);

                //
                // Initialize the underlying control/field
                //
                window.intlTelInput(cmp.input, options);

                //
                // Check to see we have what we expect...
                //
                if (!window.intlTelInputGlobals || !window.intlTelInputGlobals.instances || 
                    !window.intlTelInputGlobals.instances[cmp.itiInstanceNumber]) {
                    console.error('Could not create intl-tel-input element');
                    return;
                }
                cmp.iti = window.intlTelInputGlobals.instances[cmp.itiInstanceNumber];
                cmp.iti.extCmp = cmp;
                cmp.logFn('   intl-tel-input created successfully', 2);
                cmp.logValueFn('   iti', cmp.iti, 3);

                //
                // If the js wasnt included in the package.json, this fn would need to be called
                //
                //window.intlTelInputGlobals.loadUtils(options.utilsScript);

                //
                // Set up event lsistenrs
                //
                cmp.input.addEventListener("countrychange", cmp.onCountryChange);
                cmp.input.addEventListener("open:countrydropdown", cmp.onCountryExpand);
                cmp.input.addEventListener("close:countrydropdown", cmp.onCountryCollapse);
            }
            else {
                console.error('Could not find intl-tel-input element');
            }
        },

        beforedestroy: function(cmp)
        {
            if (cmp.input && window.iti)
            {
                cmp.input.removeEventListener("countrychange", cmp.onCountryChange);
                cmp.input.removeEventListener("open:countrydropdown", cmp.onCountryExpand);
                cmp.input.removeEventListener("close:countrydropdown", cmp.onCountryCollapse);
            }
        }
    },


    onCountryChange: function()
    {
        var me = this,
            countryData;

        if (!me.iti) {
            return;
        }
        
        countryData = me.iti.getSelectedCountryData();

        me.logFn('IntlTelField:  Country change', 2);
        me.logValueFn('   ', countryData, 3);
    },


    onCountryCollapse: function()
    {
        var me = this;
        if (!me.iti) {
            return;
        }
        me.logFn('IntlTelField:  Country dropdown collapse', 4);
    },


    onCountryExpand: function()
    {
        var me = this;
        if (!me.iti) {
            return;
        }
        me.logFn('IntlTelField:  Country dropdown expand', 4);
    },


    getNumber: function()
    {
        var me = this;
        if (!me.iti) {
            return '';
        }
        var num = me.iti.getNumber();
        me.logValueFn('IntlTelField:  Tel get number', num, 3);
        return num;
    },

    
    getRawValue: function()
    {
        var me = this,
            v = (me.inputEl ? me.inputEl.getValue() : Ext.valueFrom(me.rawValue, ''));

        //
        // Use iti.getNumber()
        //
        if (me.iti) {
            v = me.iti.getNumber();
            if (me.cleanRawValue) { // remove '+' and other format chars if set in config
                v = v.replace(/[-\()\+]/g, '');
            }
        }

        me.rawValue = v;

        return v;
    },


    setRawValue: function(value) 
    {
        var me = this,
            rawValue = me.rawValue;

        if (!me.transformRawValue.$nullFn) {
            value = me.transformRawValue(value);
        }

        value = Ext.valueFrom(value, '');

        if (rawValue === undefined || rawValue !== value) {
            me.rawValue = value;

            // Some Field subclasses may not render an inputEl
            if (me.inputEl) {
                me.bindChangeEvents(false);
                me.inputEl.dom.value = value;
                me.bindChangeEvents(true);
            }
        }

        if (me.rendered && me.reference) {
            me.publishState('rawValue', value);
        }

        //
        // Call iti setNumber()
        //
        if (me.iti) {
            me.iti.setNumber(value);
        }

        return value;
    }

});

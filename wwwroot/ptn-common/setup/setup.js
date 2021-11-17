document.addEventListener('DOMContentLoaded', function (e) {
    FormValidation.formValidation($('#UserLogon'), {
        plugins: {
            declarative: new FormValidation.plugins.Declarative(),
            icon: new FormValidation.plugins.Icon({
                valid: 'fa fa-check',
                invalid: 'fa fa-times',
                validating: 'fa fa-refresh',
            }),
        }
    });
});



function form_check(oForm) {
    return true;
}


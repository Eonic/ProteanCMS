document.addEventListener('DOMContentLoaded', function (e) {
    FormValidation.formValidation(document.getElementById('UserLogon'), {
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
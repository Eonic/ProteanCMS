var manageRedirectsAPIUrl = "/ewapi/Cms.Admin/ManageRedirects";


Vue.mixin({
    methods: {
        //global methods here.
        getQueryStringParam: function (paramName) {
            let urlParams = new URLSearchParams(window.location.search);
            let paramValue = urlParams.get(paramName);
            return paramValue;
        }
    }
});

//Edit Page
const editPageElement = document.querySelector("#EditPage");
if (editPageElement) {
    window.editPage = new Vue({
        el: "#EditPage",
        data: {
            structName: ""          
        },
        methods: {
            createRedirects: function () {
                debugger;
                var redirectTypeElement = document.querySelector("input[name='redirectType']")
                var redirectType = redirectTypeElement != null ? redirectTypeElement.value : "";
                if (redirectType == "" || redirectType == "404") {
                    return;
                }

                let urlParams = new URLSearchParams(window.location.search);
                let pageId = this.getQueryStringParam('pgid');

                var inputJson = {
                    pageId: pageId,
                    redirectType: redirectType,
                    oldUrl: localStorage.originalStructName,
                    newUrl: this.structName
                };
                var self = this;
                axios.post(manageRedirectsAPIUrl, inputJson)
                    .then(function (response) {
                        debugger;
                        //handle success.
                        redirectModal.showRedirectModal = false;
                        window.location.href = "?ewCmd=Normal";
                    });
            }
        },
        watch: {
            // whenever StructName changes, this function will run
            structName: function (newStructName) {
                debugger;                
                if (localStorage.originalStructName && localStorage.originalStructName != "" && localStorage.originalStructName != newStructName) {
                    redirectModal.toggleModal();
                } else {
                    redirectModal.showRedirectModal = false;
                }
                //localStorage.originalStructName = newStructName;
            }
        },
        mounted: function () {
            debugger;
            var cStructName = document.getElementById('cStructName');
            if (cStructName != null) {
                this.structName = cStructName.value;
            }

            //clean the storage for struct name when page changes.
            let pageId = this.getQueryStringParam('pgid');
            if (!localStorage.pageId || localStorage.pageId != pageId) {
                localStorage.removeItem('originalStructName');                
            }
            localStorage.pageId = pageId;
        }
    });
}

const redirectModalElement = document.querySelector("#redirectModal");
if (redirectModalElement) {
    window.redirectModal = new Vue({
        el: "#redirectModal",
        data: {
            showRedirectModal: false
        },
        methods: {
            toggleModal: function () {
                $("#redirectModal").removeClass("hidden");
                this.showRedirectModal = !this.showRedirectModal;
            }
        }
    });
}

//Insights
const insightsSectionElement = document.querySelector("#insights-section");
if (insightsSectionElement) {
    window.insightsSection = new Vue({
        el: "#insights-section",
        data: {
        },
        methods: {
             getMetricData: async function (metricId, apiUrl) {
                //debugger;
                let self = this;
                let inputJson = this.getParamObject(apiUrl);
                let metricElement = document.getElementById(metricId);
                let valueSpanElement = metricElement.getElementsByClassName("metric-value");

                //add loader
                metricElement.classList.add("metric-loader");

                await axios.post(apiUrl, inputJson)
                    .then(function (response) {
                        //debugger;
                        //handle success.
                        let metricElement = document.getElementById(metricId);
                        let valueSpanElement = metricElement.getElementsByClassName("metric-value");
                        if (valueSpanElement != null && valueSpanElement.length > 0) {
                            valueSpanElement[0].innerText = response.data;                            
                        }
                        metricElement.classList.remove("metric-loader");
                    });
            },
            getParamObject: function (apiUrl) {
                let paramCollection = apiUrl.substring(apiUrl.indexOf('?') + 1);
                let paramArray = paramCollection.split('&');

                var inputJson = {};

                for (var arrIndex = 0; arrIndex < paramArray.length; arrIndex++) {
                    var param = paramArray[arrIndex]
                    if (param != null) {
                        var paramKeyValue = param.split('=');
                        inputJson[paramKeyValue[0]] = paramKeyValue[1];
                    }
                }
                return inputJson;
            }
        },
        mounted: function () {
            //debugger;
            let metricsList = document.getElementsByClassName("metric");
            if (metricsList != null && metricsList.length > 0) {
                for (let metricIndex = 0; metricIndex < metricsList.length; metricIndex++) {
                    let metric = metricsList[metricIndex];
                    let apiUrl = metric.getAttribute("data-json-url");
                    if (apiUrl != null && apiUrl.length > 0) {
                        let metricElementId = metric.getAttribute("id");
                        this.getMetricData(metricElementId, apiUrl);
                    }
                }
            }
        }
    });
}
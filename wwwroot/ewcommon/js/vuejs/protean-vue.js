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

                var redirectType = $(".redirectStatus:checked").val();
                //var redirectType = redirectTypeElement != null ? redirectTypeElement.value : "";
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
                    localStorage.originalStructName = newStructName;

                }

            }
        },
        mounted: function () {
            //debugger;
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
const addNewUrlModalElement = document.querySelector("#addNewUrl");
if (addNewUrlModalElement) {
    window.addNewUrl = new Vue({
        el: "#addNewUrl",
        data: {
            showAddNewUrl: false
        },
        methods: {
            toggleModal: function () {
               
                //$("#addNewUrl").attr("data-dismiss","modal");
                this.showAddNewUrl = !this.showAddNewUrl;
            },
            SaveNewUrl: function () {
                debugger;
                var oldUrl = $("#OldUrlmodal").val();
                var NewUrl = $("#NewUrlModal").val();
                if (oldUrl != "") {
                    RedirectPage.addNewUrl(oldUrl, NewUrl);
                }

            }
        }
    });
}
$(".btnaddNewUrl").click(function () {
    addNewUrl.toggleModal();
   
});
$(".close").click(function () {
    $('#addNewUrl').modal('hide');
})
$('.addRedirectbtn').on('click', function (event) {

    debugger;
    var oldUrl = $("#OldUrlmodal").val();
    var NewUrl = $("#NewUrlModal").val();
    if (oldUrl != "") {
        RedirectPage.addNewUrl(oldUrl, NewUrl);
    }

});
$('.btnSearchUrl').on('click', function (event) {
    debugger;
   
    RedirectPage.getSearchList();
    
});



var paginationRedirectsAPIUrl = '/ewapi/Cms.Admin/redirectPagination';
var paginationAddNewUrlAPIUrl = '/ewapi/Cms.Admin/AddNewUrl';
var SearchUrlAPIUrl = '/ewapi/Cms.Admin/searchUrl';

const rediectElement = document.querySelector("#RedirectPage");
if (rediectElement) {
    window.RedirectPage = new Vue({
        el: '#RedirectPage',
        data: {
            position: 0,
            urlList: [],
            type: ''
        },
        methods: {
            getPerminentList: function () {
                var totalCountOfLoad = $(".parentDivOfRedirect").length;
                var that = this;
                var strUrl = window.location.href;
                if (strUrl.indexOf("ewCmd") > -1) {
                    var url = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
                    for (var i = 0; i < url.length; i++) {
                        var urlparam = url[i].split('=');
                        if (urlparam[0] == "ewCmd") {
                            type = urlparam[1];
                        }
                    }
                }

                var inputJson = { redirectType: type, loadCount: totalCountOfLoad};
                axios.post(paginationRedirectsAPIUrl, inputJson)
                    .then(function (response) {
                       
                        
                        var xmlString = response.data;
                        var xmlDocument = $.parseXML(xmlString);
                        var xml = $(xmlDocument);
                        if (that.urlList != '') {
                            var tempUrlList = xml[0].childNodes[0].childNodes;
                            that.urlList = $.merge($.merge([], that.urlList), tempUrlList);
                           
                        }
                        else {
                           
                            that.urlList = xml[0].childNodes[0].childNodes;
                        }

                    });
            },

            addNewUrl: function (oldUrl,NewUrl) {
               
                var that = this;
                var strUrl = window.location.href;
                if (strUrl.indexOf("ewCmd") > -1) {
                    var url = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
                    for (var i = 0; i < url.length; i++) {
                        var urlparam = url[i].split('=');
                        if (urlparam[0] == "ewCmd") {
                            type = urlparam[1];
                        }
                    }
                }

                var inputJson = { redirectType: type, oldUrl: oldUrl, newUrl: NewUrl };
                axios.post(paginationAddNewUrlAPIUrl, inputJson)
                    .then(function (response) {
                        debugger;
                        $('#addNewUrl').modal('hide');
                        $(".save301RedirectForm").click();

                    });
            },

            getSearchList: function () {
                debugger;
                var that = this;
                var searchObj = $("#SearchURLText").val();
                var strUrl = window.location.href;
                if (strUrl.indexOf("ewCmd") > -1) {
                    var url = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
                    for (var i = 0; i < url.length; i++) {
                        var urlparam = url[i].split('=');
                        if (urlparam[0] == "ewCmd") {
                            type = urlparam[1];
                        }
                    }
                }

                var inputJson = { redirectType: type, searchObj: searchObj};
                axios.post(SearchUrlAPIUrl, inputJson)
                    .then(function (response) {
                        debugger;
                       
                        var xmlString = response.data;
                        var xmlDocument = $.parseXML(xmlString);
                        var xml = $(xmlDocument);
                        that.urlList = xml[0].childNodes[0].childNodes;
                       
                       
                    });
            },
        },
        mounted: function () {
            this.getPerminentList();
           
        }
        //}
    });
}

//$('.301RedirectBody').on('mousewheel', function (event) {
//    RedirectPage.getPerminentList();
//});




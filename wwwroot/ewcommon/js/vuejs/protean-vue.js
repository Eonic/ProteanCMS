var manageRedirectsAPIUrl = "/ewapi/Cms.Admin/ManageRedirects";
var paginationRedirectsAPIUrl = '/ewapi/Cms.Admin/loadUrlsForPagination';
var paginationAddNewUrlAPIUrl = '/ewapi/Cms.Admin/AddNewUrl';
var SearchUrlAPIUrl = '/ewapi/Cms.Admin/searchUrl';
var SaveUrlAPIUrl = '/ewapi/Cms.Admin/saveUrls';
var deleteUrlsAPIUrl = '/ewapi/Cms.Admin/deleteUrls';
var IsUrlPResentAPI = '/ewapi/Cms.Admin/IsUrlPresent';


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
            showAddNewUrl: false,
            show: false,
            loading: false
        },
        methods: {
            toggleModal: function () {

                //$("#addNewUrl").attr("data-dismiss","modal");
                this.showAddNewUrl = !this.showAddNewUrl;
            },
            SaveNewUrl: function () {
                $('#addNewUrl').modal('hide');
                $(".vueloadimgforModal").removeClass("hidden");
                debugger;
                var oldUrl = $("#OldUrlmodal").val();
                var NewUrl = $("#NewUrlModal").val();
                type = RedirectPage.redirectType();
                if (oldUrl != "") {
                    var inputJson = { redirectType: type, oldUrl: oldUrl };
                    axios.post(IsUrlPResentAPI, inputJson)
                        .then(function (response) {

                            if (response.data == "True") {
                                if (confirm("Old url is already exist. Do you want to replace it?")) {
                                    $(".vueloadimgforModal").addClass("hidden");
                                    RedirectPage.addNewUrl(oldUrl, NewUrl);
                                }
                                else {
                                    $(".vueloadimgforModal").addClass("hidden");
                                    return false;
                                }
                            }
                            else {
                                $(".vueloadimgforModal").addClass("hidden");
                                RedirectPage.addNewUrl(oldUrl, NewUrl);

                            }
                            $("#OldUrlmodal").text("");
                            $("#NewUrlModal").text("");
                        });


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

$('.btnSearchUrl').on('click', function (event) {
    var searchObj = $("#SearchURLText").val();
    if (searchObj == "") {
        location.reload();

    }
    else { RedirectPage.getSearchList(searchObj); }


});
$(document).on("click", ".btn-update", function (event) {

    debugger;
    var oldUrl = "";
    var NewUrl = "";

    var parentDiv = $(this).closest('.parentDivOfRedirect');
    var input = $(parentDiv).find('input[type="text"]');
    oldUrl = $(input[0]).val();
    NewUrl = $(input[1]).val();
    hiddenOldUrl = $(parentDiv).find('input[type="hidden"]').val();
    type = RedirectPage.redirectType();
    if (oldUrl != "" && oldUrl!= hiddenOldUrl) {
        var inputJson = { redirectType: type, oldUrl: oldUrl };
        axios.post(IsUrlPResentAPI, inputJson)
            .then(function (response) {
                debugger;
                if (response.data == "True") {
                    if (confirm("Old url is already exist. Do you want to replace it?")) {
                        RedirectPage.addNewUrl(oldUrl, NewUrl);
                    }
                    else {
                        return false;
                    }
                }
                else {
                    RedirectPage.saveUrl(oldUrl, NewUrl, hiddenOldUrl);

                }
               
            });


    }

    else {
        RedirectPage.saveUrl(oldUrl, NewUrl, hiddenOldUrl);

    }
   
    $(this).hide();
});
$(document).on("click", ".btn-delete", function (event) {

    var oldUrl = "";
    var NewUrl = "";
    var parentDiv = $(this).closest('.parentDivOfRedirect');
    var input = $(parentDiv).find('input[type="text"]');
    oldUrl = $(input[0]).val();
    NewUrl = $(input[1]).val();

    RedirectPage.DeleteUrl(oldUrl, NewUrl);
});

$(document).on("mouseup", ".redirecttext", function (event) {

    var parentDiv = $(this).closest('.parentDivOfRedirect');
    var button = $(parentDiv).find('button[type="button"]');
    $(".btn-update").addClass("hidden");
    $(button[0]).removeClass("hidden");
});





const rediectElement = document.querySelector("#RedirectPage");
if (rediectElement) {
    window.RedirectPage = new Vue({
        el: '#RedirectPage',
        data: {
            position: 0,
            urlList: [],
            type: '',
            show: false,
            loading : false
        },
        methods: {
            getPermanentList: function () {

                var totalCountOfLoad = $(".parentDivOfRedirect").length;
                var that = this;
                type = this.redirectType();
                var inputJson = { redirectType: type, loadCount: totalCountOfLoad };
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

            addNewUrl: function (oldUrl, NewUrl) {

                var that = this;
                that.show = true;
                that.loading = true;
                type = this.redirectType();

                var inputJson = { redirectType: type, oldUrl: oldUrl, newUrl: NewUrl };
                axios.post(paginationAddNewUrlAPIUrl, inputJson)
                    .then(function (response) {
                        if (response.data == "success") {
                            that.show = false;
                            that.loading = false;
                            //alert("Url saved successfully!");
                        }
                        location.reload();
                    });
            },

            getSearchList: function (searchObj) {

                var that = this;
                that.show = true;
                that.loading = true;
                type = this.redirectType();
                var inputJson = { redirectType: type, searchObj: searchObj };
                axios.post(SearchUrlAPIUrl, inputJson)
                    .then(function (response) {

                        var xmlString = response.data;
                        var xmlDocument = $.parseXML(xmlString);
                        var xml = $(xmlDocument);
                        that.urlList = xml[0].childNodes[0].childNodes;
                        that.show = false;
                        that.loading = false;

                    });

            },
            saveUrl: function (oldUrl, newUrl, hiddenOldUrl) {

                var that = this;
                that.show = true;
                that.loading = true;
                type = this.redirectType();
                var inputJson = { redirectType: type, oldUrl: oldUrl, NewUrl: newUrl, hiddenOldUrl: hiddenOldUrl };
                axios.post(SaveUrlAPIUrl, inputJson)
                    .then(function (response) {
                        if (response.data == "success") {
                            that.show = false;
                            that.loading = false;
                            //alert("Saved successfully")
                        }
                        location.reload();

                    });
            },
            DeleteUrl: function (oldUrl, NewUrl) {
               
                var that = this;
                that.show = true;
                that.loading = true;
                
                type = this.redirectType();
                var inputJson = { redirectType: type, oldUrl: oldUrl, NewUrl: NewUrl };
                axios.post(deleteUrlsAPIUrl, inputJson)
                    .then(function (response) {
                        
                        if (response.data == "success") {
                            that.loading = false;
                            that.show = false;
                            //alert("Deleted successfully")
                        }
                        location.reload();

                    });
            },
            redirectType: function () {

                var that = this;
                var strUrl = window.location.href;
                if (strUrl.indexOf("ewCmd") > -1) {
                    var url = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
                    for (var i = 0; i < url.length; i++) {
                        var urlparam = url[i].split('=');
                        if (urlparam[0] == "ewCmd") {
                            type = urlparam[1];
                            if (type == "301Redirect") {
                                type = 301;
                            } else {
                                if (type == "302Redirect") {
                                    type = 302;
                                }

                            }
                        }
                    }
                }
                return type

            },
        },
        mounted: function () {
            this.getPermanentList();

        }
        //}
    });
}

$(window).bind('scroll', function () {
    var totalCountOfLoad = $(".parentDivOfRedirect").length;
     if ($(window).scrollTop() >= $('.301RedirectBody').offset().top + $('.301RedirectBody').outerHeight() - window.innerHeight) {
        
             alert(totalCountOfLoad + ' url loaded..next 50 will loaded');
             RedirectPage.getPermanentList();
        
    }

});




var manageRedirectsAPIUrl = "/ewapi/Cms.Admin/ManageRedirects";
var paginationRedirectsAPIUrl = '/ewapi/Cms.Admin/loadUrlsForPagination';
var paginationAddNewUrlAPIUrl = '/ewapi/Cms.Admin/AddNewUrl';
var SearchUrlAPIUrl = '/ewapi/Cms.Admin/searchUrl';
var SaveUrlAPIUrl = '/ewapi/Cms.Admin/saveUrls';
var deleteUrlsAPIUrl = '/ewapi/Cms.Admin/deleteUrls';
var IsUrlPResentAPI = '/ewapi/Cms.Admin/IsUrlPresent';
var LoadAllURLAPI = '/ewapi/Cms.Admin/loadAllUrls';


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

$(document).on("change", "#cStructName", function (event) {
    var newStructName = $(this).val();
    editPage.structNameOnChange(newStructName);

});

//Edit Page
const editPageElement = document.querySelector("#EditPage");
if (editPageElement) {
    window.editPage = new Vue({
        el: "#EditPage",
        data: {
            structName: "",
            originalStructureName: ""
        },
        methods: {
            createRedirects: function () {
                debugger;
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
            },
            structNameOnChange: function (newStructName) {

                if (localStorage.originalStructName && localStorage.originalStructName != "" && localStorage.originalStructName != newStructName) {

                    redirectModal.toggleModal();
                    $("#OldPageName").val(localStorage.originalStructName);
                    $("#NewPageName").val(newStructName);

                }

            }
        },
        watch: {
            // whenever StructName changes, this function will run
            //structName: function (newStructName) {
            //    debugger;
            //    if (localStorage.originalStructName && localStorage.originalStructName != "" && localStorage.originalStructName != newStructName) {

            //        redirectModal.toggleModal();

            //    } else {
            //        redirectModal.showRedirectModal = false;
            //        localStorage.originalStructName = newStructName;

            //    }

            //}
        },
        mounted: function () {

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
            localStorage.originalStructName = this.structName;
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

                $("#redirectModal").modal("show");

            }
        }
    });
}
//$(".btnaddNewUrl").click(function () {
//    //addNewUrl.toggleModal();
//    $(".newAddFormInline").removeClass("hidden");
//});
$(document).on("click", ".addRedirectbtn", function (event) {

    debugger;
    RedirectPage.SaveNewUrl();
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
$('.btnClear').on('click', function (event) {

    location.reload();

});


$(document).on("click", ".btn-update", function (event) {

    $(this).addClass("hidden")
    var parentDiv = $(this).closest('.parentDivOfRedirect');
    var savedlbl = $(parentDiv).find('.tempLableSave');

    $(savedlbl).removeClass("hidden");
    var oldUrl = "";
    var NewUrl = "";

    var parentDiv = $(this).closest('.parentDivOfRedirect');
    var input = $(parentDiv).find('input[type="text"]');
    oldUrl = $(input[0]).val();
    NewUrl = $(input[1]).val();
    var index = $(input[0]).attr("id").split('_').pop();;




    hiddenOldUrl = $(parentDiv).find('input[type="hidden"]').val();
    RedirectPage.loading = true;
    RedirectPage.show = true;
    type = RedirectPage.redirectType();
    if (oldUrl != "" && oldUrl != hiddenOldUrl) {
        var inputJson = { redirectType: type, oldUrl: oldUrl };
        axios.post(IsUrlPResentAPI, inputJson)
            .then(function (response) {

                if (response.data == "True") {
                    if (confirm("Old url is already exist. Do you want to replace it?")) {
                        RedirectPage.addNewUrl(oldUrl, NewUrl);
                    }
                    else {
                        return false;
                    }
                }
                else {
                    RedirectPage.saveUrl(oldUrl, NewUrl, hiddenOldUrl, index);

                }

            });


    }

    else {
        RedirectPage.saveUrl(oldUrl, NewUrl, hiddenOldUrl, index);

    }
    // $(this).hide();
    setTimeout(function () {
        $(savedlbl).addClass("hidden");
    }, 10000);

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
    $(".tempLableSave").addClass("hidden");
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
            loading: false,
            loadingscroll:false,
            newAddedUrlList: [],
        },
        methods: {
            getPermanentList: function () {
               
                var that = this;
                var totalCountOfLoad = $(".parentDivOfRedirect").length;


                that.loading = true;
                that.show = true;
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
                        that.loading = false;
                        that.show = false;
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
                            that.urlList
                        }
                        //location.reload();
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
                        $(".newAddFormInline").addClass("hidden");

                    });

            },
            saveUrl: function (oldUrl, newUrl, hiddenOldUrl, index) {

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
                            var flag = "saveURL";
                            that.reloadPermanentList(flag, index);

                        }

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
            scrollEvent: function () {

                var that = this;
                that.show = true;
                that.loading = true;
                window.setTimeout(function () {
                    if ($(window).scrollTop() >= $('.scolling-pane').offset().top + $('.scolling-pane').outerHeight() - window.innerHeight) {
                        debugger;
                        //var lastDiv = $(".parentDivOfRedirect").last();
                        //var span = "<br></br><span><div id='redirectLoad' class='vueloadimg' v-if='loadingscroll' v-show='true'><i class='fas fa-spinner fa-spin'> </i></div ></span>"
                        //$(lastDiv).after(span);
                        that.getPermanentList();
                    }

                }, 1000);

                that.show = false;
                that.loading = false;


            },

            reloadPermanentList: function (flag, index) {

                var searchObj = $("#SearchURLText").val();
                if (searchObj != "") {

                    RedirectPage.getSearchList(searchObj);
                }
                else {


                    var that = this;

                    var totalCountOfLoad = ($(".parentDivOfRedirect").length);
                    if (totalCountOfLoad < 50) {
                        totalCountOfLoad = 50;
                    }
                    if (flag == "saveURL") {
                        that.urlList = [];

                    }
                    that.loading = true;
                    that.show = true;
                    type = this.redirectType();
                    var inputJson = { redirectType: type, loadCount: totalCountOfLoad, flag: flag, index: index };
                    axios.post(LoadAllURLAPI, inputJson)
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
                            that.loading = false;
                            that.show = false;
                        });
                }
            },
            SaveNewUrl: function () {
                debugger;
                var that = this;
                var oldUrl = $("#OldUrlmodal").val();
                var NewUrl = $("#NewUrlModal").val();
                that.loading = true;
                that.show = true;
                type = RedirectPage.redirectType();
                if (oldUrl != "") {
                    var inputJson = { redirectType: type, oldUrl: oldUrl };
                    axios.post(IsUrlPResentAPI, inputJson)
                        .then(function (response) {

                            if (response.data == "True") {
                                if (confirm("Old url is already exist. Do you want to replace it?")) {

                                    that.addNewUrl(oldUrl, NewUrl);
                                }
                                else {

                                    return false;
                                }
                            }
                            else {

                                that.addNewUrl(oldUrl, NewUrl);

                            }
                            debugger;
                            if (that.newAddedUrlList != '') {
                                var tempUrlList = { 'oldUrl': oldUrl,'NewUrl': NewUrl }
                                that.newAddedUrlList = tempUrlList;//$.merge($.merge([], that.newAddedUrlList), tempUrlList);


                            }
                            else {

                                that.newAddedUrlList = { 'oldUrl': oldUrl, 'NewUrl': NewUrl }
                            }
                           
                            $("#OldUrlmodal").val("");
                            $("#NewUrlModal").val("");
                            that.loading = true;
                            that.show = true;
                            $(".ListOfNewAddedUrls").removeClass("hidden");
                        });


                }

            }
        },
        mounted: function () {
            this.getPermanentList();

        }
        //}
    });
}

$('.scolling-pane').on('scroll', function () {
    if ($(this).scrollTop() + $(this).innerHeight() >= $(this)[0].scrollHeight) {

        RedirectPage.getPermanentList();

    }
});





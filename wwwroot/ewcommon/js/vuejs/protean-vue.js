var manageRedirectsAPIUrl = "/ewapi/Cms.Admin/ManageRedirects";
var paginationRedirectsAPIUrl = '/ewapi/Cms.Admin/loadUrlsForPagination';
var paginationAddNewUrlAPIUrl = '/ewapi/Cms.Admin/AddNewUrl';
var SearchUrlAPIUrl = '/ewapi/Cms.Admin/searchUrl';
var SaveUrlAPIUrl = '/ewapi/Cms.Admin/saveUrls';
var deleteUrlsAPIUrl = '/ewapi/Cms.Admin/deleteUrls';
var IsUrlPResentAPI = '/ewapi/Cms.Admin/IsUrlPresent';
var LoadAllURLAPI = '/ewapi/Cms.Admin/loadAllUrls';
var getTotalNumberOfUrls = '/ewapi/Cms.Admin/getTotalNumberOfUrls';
var getTotalNumberOfSearchUrls = '/ewapi/Cms.Admin/getTotalNumberOfSearchUrls';
var IsParentPageAPI = '/ewapi/Cms.Admin/IsParentPage';


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



////Edit Page

$(document).on("click", ".btnSavePage", function (event) {

    var newStructName = $("#cStructName").val();
    editPage.structNameOnChange(newStructName);

});

$(document).on("click", "#btnRedirectSave", function (event) {
    debugger;
    if ($(".btnSubmitProduct").length > 0) {
        $(".btnSubmitProduct").click();
        $("#redirectModal").modal("hide");
    }
    if ($(".btnSubmitPage").length > 0) {

        let pageId = $(".hiddenpageId").val(); 
        var inputJson = { pageId: pageId };
        axios.post(IsParentPageAPI, inputJson)
            .then(function (response) {
                debugger;
                if (response.data == "True") {
                    if (confirm("This Page have child. Do you want to redirect it?")) {
                        $(".hiddenParentCheck").val(response.data);
                        $("#redirectModal").modal("hide");
                        $(".btnSubmitPage").click();

                    }
                    else {
                        $(".hiddenParentCheck").val("false");
                        $("#redirectModal").modal("hide");
                        $(".btnSubmitPage").click();


                    }
                }
                else {
                    debugger;
                    $(".hiddenParentCheck").val("false");
                    $("#redirectModal").modal("hide");
                    $(".btnSubmitPage").click();
                }
            });
                    }
                }
                else {
                    $(".hiddenParentCheck").val("false");
                    $("#redirectModal").modal("hide");
                    $(".btnSubmitPage").click();
                }
            });

    }

   
});

    }

   
});








$(document).on("click", ".addRedirectbtn", function (event) {
    $(".countLable").addClass("hidden");
    $(".modalLable").addClass("hidden");
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
    else { RedirectPage.getSearchList(searchObj, 0); }


});
$('.btnClear').on('click', function (event) {

    location.reload();

});


$(document).on("click", ".btn-update", function (event) {

    $(".modalLable").addClass("hidden");
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
    var index = $(input[0]).attr("id").split('_').pop();

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
                        RedirectPage.urlList[index].attributes[0].nodeValue = oldUrl;
                        RedirectPage.urlList[index].attributes[1].nodeValue = NewUrl;
                        var flag = "saveURL";
                        RedirectPage.reloadPermanentList(flag);
                    }
                    else {
                        $(input[0]).val(RedirectPage.urlList[index].attributes[0].nodeValue);
                        $(input[1]).val(RedirectPage.urlList[index].attributes[1].nodeValue);
                        $("#loadSpin").modal("hide");
                        that.show = false;
                        that.loading = false;
                        return false;
                    }
                }
                else {
                    RedirectPage.saveUrl(oldUrl, NewUrl, hiddenOldUrl, index);
                    RedirectPage.urlList[index].attributes[0].nodeValue = oldUrl;
                    RedirectPage.urlList[index].attributes[1].nodeValue = NewUrl;

                }

            });


    }

    else {
        RedirectPage.saveUrl(oldUrl, NewUrl, hiddenOldUrl, index);
        RedirectPage.urlList[index].attributes[0].nodeValue = oldUrl;
        RedirectPage.urlList[index].attributes[1].nodeValue = NewUrl;
    }

    // $(this).hide();
    setTimeout(function () {
        $(savedlbl).addClass("hidden");
    }, 10000);

});
$(document).on("click", ".btn-delete", function (event) {
    $(".countLable").addClass("hidden");
    var oldUrl = "";
    var NewUrl = "";
    var parentDiv = $(this).closest('.parentDivOfRedirect');
    var input = $(parentDiv).find('input[type="text"]');
    oldUrl = $(input[0]).val();
    NewUrl = $(input[1]).val();
    var index = $(input[0]).attr("id").split('_').pop();
    RedirectPage.DeleteUrl(oldUrl, NewUrl);

    RedirectPage.urlList.splice(index, 1);
    //RedirectPage.newAddedUrlList.splice(index, 1);
});


$(document).on("click", ".delAddNewUrl", function (event) {
    $(".countLable").addClass("hidden");
    var oldUrl = "";
    var NewUrl = "";
    var parentDiv = $(this).closest('.ListOfNewAddedUrls');
    var input = $(parentDiv).find('input[type="text"]');
    oldUrl = $(input[0]).val();
    NewUrl = $(input[1]).val();
    var index = $(input[0]).attr("id").split('_').pop();
    RedirectPage.DeleteUrl(oldUrl, NewUrl);

    RedirectPage.newAddedUrlList.splice(index, 1);

});

$(document).on("focus", ".redirecttext", function (event) {

    var parentDiv = $(this).closest('.parentDivOfRedirect');
    var button = $(parentDiv).find('button[type="button"]');

    $(".btn-update").addClass("hidden");
    $(".btn-updateNewUrl").addClass("hidden");
    $(".tempLableSave").addClass("hidden");
    $(button[0]).removeClass("hidden");

});
$(document).on("focus", ".addUrlText", function (event) {

    var parentDiv = $(this).closest('.ListOfNewAddedUrls');
    var button = $(parentDiv).find('button[type="button"]');

    $(".btn-updateNewUrl").addClass("hidden");
    $(".btn-update").addClass("hidden");
    $(".tempLableSaveNew").addClass("hidden");
    $(button[0]).removeClass("hidden");

});

$(document).on("click", ".btn-updateNewUrl", function (event) {

    $(".modalLable").addClass("hidden");
    $(this).addClass("hidden")
    var parentDiv = $(this).closest('.ListOfNewAddedUrls');
    var savedlbl = $(parentDiv).find('.tempLableSaveNew');

    $(savedlbl).removeClass("hidden");
    var oldUrl = "";
    var NewUrl = "";

    var parentDiv = $(this).closest('.ListOfNewAddedUrls');
    var input = $(parentDiv).find('input[type="text"]');
    oldUrl = $(input[0]).val();
    NewUrl = $(input[1]).val();
    var index = $(input[0]).attr("id").split('_').pop();
    type = RedirectPage.redirectType();
    if (oldUrl != "") {
        var inputJson = { redirectType: type, oldUrl: oldUrl };
        axios.post(IsUrlPResentAPI, inputJson)
            .then(function (response) {

                if (response.data == "True") {
                    if (confirm("Old url is already exist. Do you want to replace it?")) {
                        RedirectPage.addNewUrl(oldUrl, NewUrl);
                    }
                    else {
                        $("#loadSpin").modal("hide");
                        that.show = false;
                        that.loading = false;
                        return false;
                    }
                }
                else {
                    RedirectPage.saveUrl(oldUrl, NewUrl, "");


                }

                RedirectPage.newAddedUrlList[index] = { 'oldUrl': oldUrl, 'NewUrl': NewUrl };
            });


    }

    setTimeout(function () {
        $(savedlbl).addClass("hidden");
    }, 10000);

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
            loadingscroll: false,
            newAddedUrlList: [],
            totalCountofUrl: '',
            scrollStatus: 0,
        },
        methods: {
            getPermanentList: function () {

                var that = this;
                var totalCountOfLoad = $(".parentDivOfRedirect").length;
                var totalToDispaly = $("#totalUrlCount").val();
                $("#loadSpin").modal("show");

                var lableDisplay = "Loading next 50 of " + totalToDispaly + " lines";
                $(".modalLable").text(lableDisplay);
                //that.loading = true;
                //that.show = true;
                type = this.redirectType();

                var inputJson = { redirectType: type, loadCount: totalCountOfLoad };
                axios.post(paginationRedirectsAPIUrl, inputJson)
                    .then(function (response) {


                        if (response.data != "") {
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

                        }

                        $("#loadSpin").modal("hide");
                        //that.loading = false;
                        //that.show = false;

                        var totalCountOfLoadlist = that.urlList.length;
                        var totalToDispalylist = $("#totalUrlCount").val();
                        if (totalToDispalylist == "") {
                            totalToDispalylist = 0;
                        }
                        $(".countLable").text("Loaded " + totalCountOfLoadlist + " of " + totalToDispalylist + " lines");
                        $(".countLable").removeClass("hidden");
                    });
            },

            addNewUrl: function (oldUrl, NewUrl) {

                var that = this;
                $(".modalLable").addClass("hidden");
                //$("#loadSpin").modal("show");

                //that.show = true;
                //that.loading = true;
                type = this.redirectType();

                var inputJson = { redirectType: type, oldUrl: oldUrl, newUrl: NewUrl };
                axios.post(paginationAddNewUrlAPIUrl, inputJson)
                    .then(function (response) {
                        if (response.data == "success") {

                            //$("#loadSpin").modal("hide");
                            //that.show = false;
                            //that.loading = false;
                            //alert("Url saved successfully!");
                            that.urlList;
                            // that.getTotalUrlCount();
                        }
                        //location.reload();
                    });
            },

            getSearchList: function (searchObj, searchLoadCount) {

                var that = this;
                $(".modalLable").addClass("hidden");
                $(".countLable").addClass("hidden");
                $("#loadSpin").modal("show");
                that.show = true;
                that.loading = true;
                type = this.redirectType();
                var totalCountOfLoad = searchLoadCount;
                var inputJson = { redirectType: type, searchObj: searchObj, loadCount: totalCountOfLoad };
                axios.post(SearchUrlAPIUrl, inputJson)
                    .then(function (response) {

                        if (response.data != "") {
                            if (searchLoadCount == 0) {
                                that.urlList = [];
                            }
                            var xmlString = response.data;
                            var xmlDocument = $.parseXML(xmlString);
                            var xml = $(xmlDocument);

                            if (that.urlList != '') {
                                var tempUrlList = xml[0].childNodes[0].childNodes;
                                that.urlList = $.merge($.merge([], that.urlList), tempUrlList);

                            }
                            else {

                                that.urlList = xml[0].childNodes[0].childNodes;
                                if (that.urlList.length == 0) {
                                    $("#loadSpin").modal("hide");
                                }
                            }

                        }

                        $(".newAddFormInline").addClass("hidden");
                        that.getTotalUrlOfSearchCount();
                        that.newAddedUrlList = [];
                        $("#loadSpin").modal("hide");
                        //that.show = false;
                        //that.loading = false;


                    });

            },
            saveUrl: function (oldUrl, newUrl, hiddenOldUrl) {

                var that = this;
                $("#loadSpin").modal("show");
                that.show = true;
                that.loading = true;
                type = this.redirectType();
                var inputJson = { redirectType: type, oldUrl: oldUrl, NewUrl: newUrl, hiddenOldUrl: hiddenOldUrl };
                axios.post(SaveUrlAPIUrl, inputJson)
                    .then(function (response) {

                        if (response.data == "success") {
                            $("#loadSpin").modal("hide");
                            that.show = false;
                            that.loading = false;
                            var flag = "saveURL";
                            that.reloadPermanentList(flag);

                        }

                    });
            },
            DeleteUrl: function (oldUrl, NewUrl) {

                var that = this;
                $(".modalLable").addClass("hidden");
                $("#loadSpin").modal("show");
                that.show = false;
                that.loading = false;
                that.show = true;
                that.loading = true;

                type = this.redirectType();
                var inputJson = { redirectType: type, oldUrl: oldUrl, NewUrl: NewUrl };
                axios.post(deleteUrlsAPIUrl, inputJson)
                    .then(function (response) {

                        if (response.data == "success") {
                            $("#loadSpin").modal("hide");
                            that.show = false;
                            that.loading = false;

                            that.reloadPermanentList("deleteUrl");

                        }
                        //location.reload();

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
                            //if (type == "301Redirect") {
                            //    type = 301;
                            //} else {
                            //    if (type == "302Redirect") {
                            //        type = 302;
                            //    }

                            //}
                        }
                    }
                }
                return type

            },


            reloadPermanentList: function (flag) {

                $(".modalLable").addClass("hidden");
                var searchObj = $("#SearchURLText").val();
                if (searchObj != "") {

                    RedirectPage.getSearchList(searchObj, 0);
                }
                else {


                    var that = this;

                    var totalCountOfLoad = ($(".parentDivOfRedirect").length);
                    if (totalCountOfLoad < 50) {
                        totalCountOfLoad = 50;
                    }
                    if (flag == "saveURL" || flag == "deleteUrl") {
                        that.urlList = [];
                        if (flag == "deleteUrl") {
                            var totalCountOfLoad = ($(".parentDivOfRedirect").length - 1);
                        }

                    }
                    that.scrollStatus = 1;
                    $("#loadSpin").modal("show");
                    that.loading = true;
                    that.show = true;
                    type = this.redirectType();
                    var inputJson = { redirectType: type, loadCount: totalCountOfLoad, flag: flag };
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

                            that.getTotalUrlCount();
                            that.scrollStatus = 0;
                            $("#loadSpin").modal("hide");
                            that.loading = false;
                            that.show = false;
                        });
                }
            },
            SaveNewUrl: function () {
                $(".modalLable").addClass("hidden");
                var that = this;
                var oldUrl = $("#OldUrlmodal").val();
                var NewUrl = $("#NewUrlModal").val();
                $("#loadSpin").modal("show");
                that.loading = true;
                that.show = true;
                var flag = "saveURL";
                type = RedirectPage.redirectType();
                if (oldUrl != "") {
                    var inputJson = { redirectType: type, oldUrl: oldUrl };
                    axios.post(IsUrlPResentAPI, inputJson)
                        .then(function (response) {

                            if (response.data == "True") {
                                if (confirm("Old url is already exist. Do you want to replace it?")) {

                                    that.addNewUrl(oldUrl, NewUrl);

                                    if (that.newAddedUrlList != '') {
                                        oldindex = that.newAddedUrlList.findIndex(x => x.oldUrl === oldUrl);

                                        if (oldindex != -1) {
                                            that.newAddedUrlList[oldindex] = { 'oldUrl': oldUrl, 'NewUrl': NewUrl };
                                        }
                                    }

                                }
                                else {
                                    $("#loadSpin").modal("hide");
                                    $("#OldUrlmodal").val("");
                                    $("#NewUrlModal").val("");
                                    that.loading = false;
                                    that.show = false;
                                    return false;
                                }
                            }
                            else {

                                that.addNewUrl(oldUrl, NewUrl);

                                if (that.newAddedUrlList != '') {
                                    var index = that.newAddedUrlList.length;
                                    var tempUrlList = { 'oldUrl': oldUrl, 'NewUrl': NewUrl };
                                    that.newAddedUrlList[index] = tempUrlList;

                                }
                                else {

                                    that.newAddedUrlList[0] = { 'oldUrl': oldUrl, 'NewUrl': NewUrl };

                                }

                            }
                            that.reloadPermanentList(flag);
                            // that.getTotalUrlCount();
                            $("#OldUrlmodal").val("");
                            $("#NewUrlModal").val("");
                            $("#loadSpin").modal("hide");
                            //that.loading = false;
                            //that.show = false;

                        });


                }

            },

            getTotalUrlCount: function () {

                var that = this;
                $("#loadSpin").modal("show");
                that.show = true;
                that.loading = true;
                type = this.redirectType();
                var inputJson = { redirectType: type };
                axios.post(getTotalNumberOfUrls, inputJson)
                    .then(function (response) {

                        if (response.data != "") {

                            that.totalCount = response.data;
                            $("#totalUrlCount").val(response.data);
                            if (response.data != that.urlList.length) {
                                var totalCountOfLoadlist = that.urlList.length + that.newAddedUrlList.length;
                            }
                            else {
                                var totalCountOfLoadlist = that.urlList.length;
                            }

                            $(".countLable").text("Loaded " + totalCountOfLoadlist + " of " + response.data + " lines");
                            $(".countLable").removeClass("hidden");
                            if (totalCountOfLoadlist == response.data) {
                                $(".endLable").removeClass("hidden");
                            }
                        }

                    });

            },
            getTotalUrlOfSearchCount: function () {
                var searchObj = $("#SearchURLText").val();
                var that = this;
                $("#loadSpin").modal("show");
                that.show = true;
                that.loading = true;
                type = this.redirectType();
                var inputJson = { redirectType: type, searchObj: searchObj };
                axios.post(getTotalNumberOfSearchUrls, inputJson)
                    .then(function (response) {

                        if (response.data != "" || response.data == 0) {
                            $("#totalUrlCount").val(response.data);
                            var totalCountOfLoadlist = that.urlList.length + that.newAddedUrlList.length;

                            $(".countLable").text("Loaded " + totalCountOfLoadlist + " of " + response.data + " lines");
                            $(".countLable").removeClass("hidden");
                            if (totalCountOfLoadlist == response.data) {
                                $(".endLable").removeClass("hidden");
                            }

                        }
                        $("#loadSpin").modal("hide");
                    });

            },
        },
        mounted: function () {
            this.getTotalUrlCount();
            this.getPermanentList();

        }

    });
}
$(".endLable").addClass("hidden");
$('.scolling-pane').on('scroll', function () {

    var searchObj = $("#SearchURLText").val();

    if ($(this).scrollTop() + $(this).innerHeight() >= $(this)[0].scrollHeight) {

        var totalCount = $("#totalUrlCount").val();
        var loadCount = $(".parentDivOfRedirect").length;
        if (totalCount != loadCount) {
            if (searchObj == "") {
                if (RedirectPage.scrollStatus == 0) {


                    $(".countLable").addClass("hidden");
                    RedirectPage.getPermanentList();
                }
            }
            else {
                var totalCountOfLoad = $(".parentDivOfRedirect").length;
                $(".modalLable").removeClass("hidden");
                RedirectPage.getSearchList(searchObj, totalCountOfLoad);
            }
        }

    }
});



//Insights
const insightsSectionElement = document.querySelector("#insights-section");
if (insightsSectionElement) {
    window.insightsSection = new Vue({
        el: "#insights-section",
        data: {
            resultArray: {}
        },
        methods: {
            getMetricData: function (metricId, apiUrl) {
                let self = this;
                let inputJson = this.getParamObject(apiUrl);
                let metricElement = document.getElementById(metricId);

                //add loader
                metricElement.classList.add("metric-loader");

                axios.post(apiUrl, inputJson)
                    .then(function (response) {
                        //handle success.
                        let metricElement = document.getElementById(metricId);
                        Vue.set(self.resultArray, metricId, response.data);

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
            },
            filterResultArray: function (metricId) {
                return this.resultArray[metricId];
            }
        },
        mounted: function () {
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
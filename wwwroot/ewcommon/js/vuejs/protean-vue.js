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
                debugger;
                $("#addNewUrl").removeClass("hidden");
                this.showAddNewUrl = !this.showAddNewUrl;
            }
        }
    });
}
$(".btnaddNewUrl").click(function () {
    
    debugger;
    //addNewUrl.toggleModal();
    $("#divAddNewUrl").removeClass("hidden");
    
    var totalCountOfLoad = $(".repeat-group").length;
    totalCountOfLoad = totalCountOfLoad - 1;
    $(".oldUrl").attr("id", "OldUrl_" + totalCountOfLoad);
    $(".newUrl").attr("id", "NewUrl_" + totalCountOfLoad);
    $(".oldUrl").attr("name", "OldUrl_" + totalCountOfLoad);
    $(".newUrl").attr("name", "NewUrl_" + totalCountOfLoad);
    $("#divAddNewUrl button[value=Del]").attr("name", "delete:urlRepeat_" + totalCountOfLoad);
    $("#fieldsetId").attr("class", "rpt_" + totalCountOfLoad);
    var div = $("#divAddNewUrl").html();
    //$("#parentDivOfRedirect").append(div);
});


var paginationRedirectsAPIUrl = '/ewapi/Cms.Admin/redirectPagination';
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
        },
        mounted: function () {
            this.getPerminentList();

        }
        //}
    });
}

$('.301RedirectBody').on('mousewheel', function (event) {
    RedirectPage.getPerminentList();
});

//function loadConfirmation(id) {

   
//    var data = "/ewcommon/tools/ajaxContentForm.ashx?ajaxCmd=BespokeProvider&provider=IntoTheBlue&method=IntoTheBlue.Web.Forms.SaveCSRConfirmation&Id=" + id;
//    $.ajax({
//        url: data,
//        type: 'GET',
//        success: function (AjaxResponse) {

//            $("#ConfirmationModal").html(AjaxResponse);
//            $('#ConfirmationModal').modal('show');
//            $('#prodid').val(id);
//            $("#strSalesource").attr("disabled", "disabled");

//        }
//    });

//}




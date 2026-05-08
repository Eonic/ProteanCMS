
$(document).ready(function () {
    setTimeout(function () {
        $("#Location").after(
            '<span class="useMyLocation" class="mylocation">' +
            '<i class="fas fa-location-arrow"></i> Use my current location' +
            '</span>'
        );
    }, 500);

    
    

    setTimeout(function () {
        
        // Add the "Use my current location" span after the input
        if ($("#filterAllModal .useMyLocation").length == 0) {
            $("#filterAllModal #Location").before('<label for="Distance" class="" style="font-weight: 600;color: #15306D !important;">Location</label>');
            //$("#filterAllModal #Location").after(
            //    '<span class="useMyLocation">' +
            //    '<i class="fas fa-location-arrow"></i> Use my current location' +
            //    '</span>'
            //);
            
        }
    }, 500);
    $(".modal-section .distance").hide();

    var stickyTop = $('.ContentFilter-wrapper').offset().top;
    var deductHeight = $(".side-nav-layout").offset().top;
    $(window).scroll(function () {
        var windowTop = $(window).scrollTop();
        if (stickyTop < windowTop && deductHeight) {
            $('.ContentFilter-wrapper').css('position', 'fixed');
            $('.ContentFilter-wrapper').addClass('sticky-filter');
        } else {
            $('.ContentFilter-wrapper').css('position', 'relative');
            $('.ContentFilter-wrapper').removeClass('sticky-filter');
        }
    });

    
    var totalCount = 0;
    var ncountForPage = $('input[name="PageFilter"]:checked').length;
    if (ncountForPage > 0) {
        totalCount = totalCount + ncountForPage / 2;
        $('label[for="PageFilter"]').append('<span class="badge ms-2" id="ProductCount" style="margin-left:3px;">' + ncountForPage / 2 + '</span>');
    }
    var ncountForPrice = $('input[name="PriceBandFilter"]:checked').length;
    if (ncountForPrice > 0) {
        totalCount = totalCount + ncountForPrice / 2;
        $('label[for="PriceBandFilter"]').append('<span class="badge ms-2" id="ProductCount" style="margin-left:3px;">' + ncountForPrice / 2 + '</span>');

    }
    if ($('.locationfilter').hasClass('active-filter')) {
        totalCount = totalCount + 1;
        $('label[for="Distance"]').append('<span class="badge ms-2" id="ProductCount" style="margin-left:3px;">1</span>');
    }
    if ($('.groupsizefilter').hasClass('active-filter')) {
        totalCount = totalCount + 1;
        $('.GroupSizeSliderMainDiv label:first').append('<span class="badge ms-2" id="ProductCount" style="margin-left:3px;">1</span>');
    }
    if ($('.agefilter').hasClass('active-filter')) {
        totalCount = totalCount + 1;
        $('.AgeSliderMainDiv label:first').append('<span class="badge ms-2" id="ProductCount" style="margin-left:3px;">1</span>');
    }
    if ($('.weightfilter').hasClass('active-filter')) {
        totalCount = totalCount + 1;
        $('.WeightSliderMainDiv label:first').append('<span class="badge ms-2" id="ProductCount" style="margin-left:3px;">1</span>');
    }
    if ($('.pricefilter').hasClass('active-filter')) {
        totalCount = totalCount + 1;
        $('.histogramSliderMainDivPrice label:first').append('<span class="badge ms-2" id="ProductCount" style="margin-left:3px;">1</span>');
    }

    var ncountForOffer = $('input[name="OfferFilter"]:checked').length;
    if (ncountForOffer > 0) {

        totalCount = totalCount + ncountForOffer / 2;
        $('label[for="OfferFilter"]').append('<span class="badge ms-2" id="ProductCount" style="margin-left:3px;">' + ncountForOffer / 2 + '</span>');


    }


    if ($(".btnAllFilters").length > 0) {
        if (totalCount > 0) {

            $('.btnAllFilters').append('<span class="badge ms-2" id="ProductCount" style="margin-left:3px;">' + totalCount + '</span>');
            $("#cntAppliedFilter").text(totalCount);
        }
    }
    if ($(".btnAllFiltersMB").length > 0) {
        if (totalCount > 0) {

            $('.btnAllFiltersMB').append('<span class="badge ms-2" id="ProductCount" style="margin-left:3px;">' + totalCount + '</span>');
            $("#cntAppliedFilter").text(totalCount);
        }
    }
    if ($(".filter-xs-btn").length > 0) {
        if (totalCount > 0) {

            $('.filter-xs-btn').append('<span class="badge ms-2" id="ProductCount" style="margin-left:3px;">' + totalCount + '</span>');

        }
    }

    //$('#btnSubmitFilters').prop('disabled', true);
    $('.btnAllFilters').click(function (e) {
        e.preventDefault();
        var $modal = $('#filterAllModal');

        // Tell the modal to show ALL sections for this trigger
        $modal.data('section', 'all');

        $modal.modal('show');
        var formActions = $(".filter-main .form-actions").last().detach();
        formActions.removeClass("form-actions").addClass("modal-body-actions");
        $("#filterAllModal .modal-body .textOfSpan").after(formActions);

        if ($('input[name="OfferFilter"]:checked').length > 0) {
            var modalBodyActions = $("#filterAllModal .modal-body .modal-body-actions");
            if (modalBodyActions.length == 0) {
                modalBodyActions = $('<div class="modal-body-actions"></div>');
                $("#filterAllModal .modal-body").append(modalBodyActions);

            }
            var lastFormActions = $(".offerfilter .form-actions button").last().detach();
            lastFormActions.removeClass("hidden");
            modalBodyActions.append(lastFormActions);
            $(".offerfilter .form-actions").hide();
        }

    });

    $(document).on("click", ".remove-filter-btn", function () {
        let checkboxId = $(this).data("checkbox");
        $("#" + checkboxId).prop("checked", false).trigger("change");
        $("#filterAllModal").modal("hide");
    });
    $('.filter-main .form-group').click(function () {
        $(this).children('.control-wrapper').toggle();
    });
    const rhsSortCount = $('.rhs-filter-btns .sortOptionsWrapper').length;
    if (rhsSortCount > 0) {

        $('#filters-container .sortOptionsWrapper').hide();

        $('#productSortOptions').closest('.sortOptionsWrapper').hide();
    }


    if ($('.active-filter').length > 0) {
        $('.clear-all-link, #resetFilters, .textOfSpan').removeClass('visually-hidden');
    } else {
        $('.clear-all-link, #resetFilters, .textOfSpan').addClass('visually-hidden');
    }

    $("#Location").attr("placeholder", "Town or Postcode");
    var hdnLocationHideByDefault = $("#LocationHideByDefault").val();
    if ($(".btnShowMoreFilter").length > 0) {
        if (hdnLocationHideByDefault == '') {
            hdnLocationHideByDefault = $("LocationHideByDefault").val();

            $("#LocationHideByDefault").val(hdnLocationHideByDefault);
            if ($("#LocationHideByDefault") != undefined) {
                if ($("#LocationHideByDefault").val() == "true") {
                    // $('.distance').addClass("hidden");
                }
            }
        }
    }

    $("#productSortByOptions").on('change', function () {
        var sortBy = $(this).val();
        //alert(sortBy);
        if (sortBy != '') {
            $("#SortBy").val(sortBy);
            $("#SearchLoader .modal-body h2").text("Just one second whilst we find the best experiences for you!");
            $("#SearchLoader .modal-body h2").css("font-size", "1.3em");
            $('#SearchLoader').modal('show');

            if (sortBy == 'distance') {
                $('#SearchLoader').modal('hide');
                $('#postcodeSortModal').modal('show');
                return;


            }
            if (sortBy == 'rating') {
                $('#SearchLoader').modal('hide');
                $(".btnRatingSubmit").click();
                $('#ContentFilter').submit();


            }
            if (sortBy == 'asc' || sortBy == 'desc') {

                $("#productSortByOptions").val(sortBy)
                $("#MinPrice").val($("#histogramSliderPrice-sliderMin").val());
                $("#MaxPrice").val($("#histogramSliderPrice-sliderMax").val());
                $(".btnPriceSubmit").click();
                $('#ContentFilter').submit();
            }

        }
    });

    $("#btnSortWithLocation").on('click', function () {
        $("#Location").val($("#txtLocationFrom").val());
        $("#filterAllModal #Location").val($("#txtLocationFrom").val());
        $(".btnLocationSub").click();
        $('#ContentFilter').submit();
    });




    if ($(".btnShowMoreFilter").length == 0 || $("#cShowMore").val() != '') {
        $("#cShowMore").val('1');


        $(".btnShowMoreFilter").addClass('hidden');
        //$(".btnHideFilter").removeClass('hidden');
        $(".filter").removeClass('hidden');
        $(".filter").closest(".form-group").removeClass("hidden");
        $(".histogramSliderMainDivPrice").removeClass("hidden");
        $(".AgeSliderMainDiv").removeClass("hidden");
        $('.WeightSliderMainDiv').removeClass("hidden");
        $('.GroupSizeSliderMainDiv').removeClass("hidden");
        $('.SubmitOfferFilter').removeClass("hidden");
        $("label[for='OfferFilter']").closest(".form-group").removeClass("hidden");
        $('.distance').removeClass("hidden");
    }




    $(".btnShowMoreFilter").on('click', function (e) {
        e.preventDefault();
        $(".btnShowMoreFilter").addClass('hidden');
        //$(".btnHideFilter").removeClass('hidden');
        $(".filter").removeClass('hidden');
        $(".filter").closest(".form-group").removeClass("hidden");
        $(".histogramSliderMainDivPrice").removeClass("hidden");
        $(".AgeSliderMainDiv").removeClass("hidden");
        $('.WeightSliderMainDiv').removeClass("hidden");
        $('.GroupSizeSliderMainDiv').removeClass("hidden");
        $('.distance').removeClass("hidden");
        $('.SubmitOfferFilter').removeClass("hidden");
        $("label[for='OfferFilter']").closest(".form-group").removeClass("hidden");
        $("#cShowMore").val(1);


    });

    function weightConverterKgToStone(valNum, lblId) {
        return valNum * 0.1574;
    }

    //Age filter

    $(document).ready(function () {
        $(".form-inline").each(function () {
            if ($(this).is(":empty")) {
                $(this).html("&nbsp;"); // adds a non-breaking space
            }
        });
        $(".btnAllFilters").on("click", function () {
            var locationText = $("#Location").val();
            $(".modal-section #Location").val(locationText);

            if ($("#filterAllModal .AgeSliderMainDiv").length == 2) {
                LoadAgeFilter('#filterAllModal');
            }

            if ($("#filterAllModal .GroupSizeSliderMainDiv ").length == 2) {
                LoadGroupSizeFilter('#filterAllModal');
            }

            if ($("#filterAllModal .WeightSliderMainDiv ").length == 2) {
                LoadWeightFilter('#filterAllModal');
            }
                
         
        });
    });

    
    function LoadAgeFilter(filterAllModal) {

        var ageSliderFrom = '';
        var ageSliderTo = '';
        ageSliderFrom = $("#SliderMinAge").val();
        ageSliderTo = $("#SliderMaxAge").val();
        var hdnAgeHideByDefault = $("#AgeHideByDefault").val();

        if ($(".AgeSlider").length > 0) {
            $(".AgeSlider").css("margin-right", "20px")
        }

        if (ageSliderFrom == '' && ageSliderTo == '') {
            if ($(".btnCrossForAge").length > 0) {
                var name = $(".btnCrossForAge").val();
                var values = name.split(',');
                ageSliderFrom = values[0];
                ageSliderTo = values[1];

                $("#MinAge").val(ageSliderFrom);
                $("#MaxAge").val(ageSliderTo);
                $(".minage").val(ageSliderFrom);
                $(".maxage").val(ageSliderTo);
                $("#SliderMinAge").val(ageSliderFrom);
                $("#SliderMaxAge").val(ageSliderTo);
            }
            else {

                ageSliderFrom = '1';
                ageSliderTo = '99';
               // $("#SliderMinAge").val(ageSliderFrom);
                $("#SliderMaxAge").val(ageSliderTo);
            }
        }

        if ($(".btnShowMoreFilter").length > 0) {

            if (hdnAgeHideByDefault == '') {
                hdnAgeHideByDefault = $("#AgeHideByDefault").val();

                $("#AgeHideByDefault").val(hdnAgeHideByDefault);
                if ($("#AgeHideByDefault") != undefined) {
                    if ($("#AgeHideByDefault").val() == "true") {
                        $('.AgeSliderMainDiv').addClass("hidden");
                    }
                }
            }

        }






        if ($(filterAllModal + " .AgeSliderMainDiv").length > 0) {

            var ageClearAll = $(filterAllModal + " .AgeSliderclearAll").detach();
            var ageSlider = $(filterAllModal + " .AgeSlider").detach();



            $(filterAllModal + " .AgeSliderMainDiv div").empty();



            $(filterAllModal + " .AgeSliderMainDiv div").append(ageClearAll).append(ageSlider);

            $(filterAllModal + " .AgeSliderMainDiv").not(':first').remove();
            $(filterAllModal + " .AgeSliderclearAll").not(':first').remove();
            $(filterAllModal + " .AgeSlider").not(':first').remove();


            $(filterAllModal + " .AgeSliderMainDiv input").hide();

            //$(".AgeSliderMainDiv .input-wrapper .AgeSlider ").css('margin-bottom','20px');
            if ($(filterAllModal + " .AgeSliderMainDiv div")[0] != undefined) {
                $(filterAllModal + " .AgeSliderMainDiv div")[0].append($(".form-actions .btnAgeSubmit")[0]);
                $(filterAllModal + " .btnAgeSubmit").removeClass("hidden");
                $(".btnAgeSubmit").eq(1).addClass("hidden");


            }

            if ($(filterAllModal + " .AgeSliderclearAll")[0] != undefined) {
                var clearAllButton = $('<button>', {

                    class: 'btnClearAll', // optional classes
                    click: function (event) {
                        event.preventDefault();
                        $('#SearchLoader').modal('show');
                        $("#allfilters #MinAge").val(null);
                        $("#allfilters #MaxAge").val(null);
                        $("#filterAllModal .minage").val(null);
                        $("#filterAllModal .maxage").val(null);
                       
                        $(this).hide();
                        $('#ContentFilter').submit();
                    }
                });
                clearAllButton.html('<i class="fa fa-times"></i> Clear Filter')
                    .css({
                        'margin-bottom': '15px',
                        'width': '100%'
                    });

                $(filterAllModal + " .AgeSliderclearAll")[0].append(clearAllButton[0]);
            }
            var sAgeFrom = '';
            var sAgeTo = '';

            if ($(filterAllModal + " #MinAge").val() != '') {
                sAgeFrom = $(filterAllModal + " #MinAge").val();
            }

            if ($(filterAllModal + " #MaxAge").val() != '') {
                sAgeTo = $(filterAllModal + " #MaxAge").val();
            }

            if (sAgeFrom == '') {
                sAgeFrom = ageSliderFrom;
            }
            if (sAgeTo == '') {
                sAgeTo = ageSliderTo;
            }

            if ($(filterAllModal + ".wrunner.wrunner_theme_default.wrunner_direction_horizontal").length <= 1) {
                var wRunnerAge = $(filterAllModal + ' .AgeSlider').wRunner({
                    type: 'range',
                    rangeValue: {
                        minValue: sAgeFrom,
                        maxValue: sAgeTo,
                    },
                    limits: {
                        minLimit: 1,
                        maxLimit: ageSliderTo,
                    },
                    valueNoteDisplay: true,
                    preventOverlap: true,
                    onValueUpdate: function (values) {

                        //$("#lblAge").text("" + values.minValue + " - " + values.maxValue + " "); 
                        $('#txtMinAge').val(values.minValue);
                        $('#txtMaxAge').val(values.maxValue);
                        $("#filterAllModal .minage").val(values.minValue);
                        $("#filterAllModal .maxage").val(values.maxValue);

                        var left1 = parseFloat($(filterAllModal + ' .AgeSlider .wrunner__valueNote').eq(0).css('left'))
                        var left2 = parseFloat($(filterAllModal + ' .AgeSlider .wrunner__valueNote').eq(1).css('left'))

                        var diff = Math.abs(left1 - left2);

                        if (diff < 50) {

                            $(filterAllModal + ' .AgeSlider .wrunner__valueNote').eq(0).css('margin-left', '-18px');
                            $(filterAllModal + ' .AgeSlider .wrunner__valueNote').eq(1).css('margin-left', '18px');

                        }

                    }
                })


                $(filterAllModal + ' .AgeSlider .wrunner__valueNote').eq(0).css('left', '-8.5%');
                $(filterAllModal + ' .AgeSlider .wrunner__valueNote').eq(1).css('left', '91.5%');

                //$('.AgeSlider .wrunner').append('<label id="lblAge" style="text-align:center;">&#160;</label>');

                /* var AgeValue = wRunnerAge.getValue();*/
                if ($("#filterAllModal .inline-textbox-containerAge").length == 0) { }
                var txtMinAge = $('<input type="text" id="txtMinAge"  class="txtFilter minage"></input>');

                txtMinAge.val(sAgeFrom);
                var txtMaxAge = $('<input type="text" id="txtMaxAge"  class="txtFilter maxage"></input>');
                txtMaxAge.val(sAgeTo);
                var lblMinAge = $('<label text="From" style="display: inline;" class="lblSliders" for="txtMinAge">From</label>');
                var lblMaxAge = $('<label text="To" class="lblSliders" for="txtMaxAge">To</label>');
                var container = $("<div></div>").addClass("inline-textbox-containerAge clearfix");

                container.append(lblMinAge);
                container.append(lblMaxAge);
                container.append(txtMinAge);
                container.append(txtMaxAge);

                //container.insertAfter("#histogramSliderWeight");

                $(filterAllModal + ' .AgeSlider .wrunner').append(container);
            }

               

           // }

        }

        

    }

    LoadAgeFilter('.dropdown-filters');
    $(".btnAgeSubmit").on('click', function (e) {
        
        e.preventDefault();
        $("#SearchLoader .modal-body h2").text("Just one second whilst we find the best experiences for you!");
        $("#SearchLoader .modal-body h2").css("font-size", "1.3em");
        $('#SearchLoader').modal('show');
        var sAgeFrom = $('.AgeSlider .wrunner__valueNote')[0].innerText;
        var sAgeTo = $('.AgeSlider .wrunner__valueNote')[1].innerText;
        
        // $("#ContentFilter").submit();
        $("#MinAge").val(sAgeFrom);
        $("#MaxAge").val(sAgeTo);
        $(".minage").val(sAgeFrom);
        $(".maxage").val(sAgeTo);
        //$("#SliderMinAge").val(sAgeFrom);
        //$("#SliderMaxAge").val(sAgeTo);
        $('#ContentFilter').submit();

    });

    //Age filter code end



    //Group slider Code started
    function LoadGroupSizeFilter(filterAllModal) {

        var groupSizeSliderFrom = '';
        var groupSizeSliderTo = '';
        groupSizeSliderFrom = $("#SliderGroupSizeFrom").val();
        groupSizeSliderTo = $("#SliderGroupSizeTo").val();
        var hdnGroupSizeHideByDefault = $("#GroupSizeHideByDefault").val();

        if ($(".GroupSizeSlider").length > 0) {
            $(".GroupSizeSlider").css("margin-right", "20px")
        }

        if (groupSizeSliderFrom == '' && groupSizeSliderTo == '') {
            if ($(".btnCrossForGroupSize").length > 0) {
                var name = $(".btnCrossForGroupSize").val();
                var values = name.split(',');
                groupSizeSliderFrom = values[0];
                groupSizeSliderTo = values[1];

                $("#GroupSizeFrom").val(groupSizeSliderFrom);
                $("#GroupSizeTo").val(groupSizeSliderTo);
                $(".mingroupsize").val(groupSizeSliderFrom);
                $(".maxgroupsize").val(groupSizeSliderTo);
                //$("#SliderGroupSizeFrom").val(groupSizeSliderFrom);
                //$("#SliderGroupSizeTo").val(groupSizeSliderTo);
            }
            else {

                groupSizeSliderFrom = '1';
                groupSizeSliderTo = '15';
                $("#SliderGroupSizeTo").val(groupSizeSliderTo);
                $("#SliderGroupSizeFrom").val(groupSizeSliderFrom);
            }
        }

        if ($(".btnShowMoreFilter").length > 0) {

            if (hdnGroupSizeHideByDefault == '') {
                hdnGroupSizeHideByDefault = $("#GroupSizeHideByDefault").val();

                $("#GroupSizeHideByDefault").val(hdnGroupSizeHideByDefault);
                if ($("#GroupSizeHideByDefault") != undefined) {
                    if ($("#GroupSizeHideByDefault").val() == "true") {
                        $('.GroupSizeSliderMainDiv').addClass("hidden");
                    }
                }
            }

        }






        if ($(filterAllModal + " .GroupSizeSliderMainDiv").length > 0) {

            var groupSizeClearAll = $(filterAllModal + " .GroupSizeSliderclearAll").detach();
            var GroupSizeSlider = $(filterAllModal + " .GroupSizeSlider").detach();



            $(filterAllModal + " .GroupSizeSliderMainDiv div").empty();



            $(filterAllModal + " .GroupSizeSliderMainDiv div").append(groupSizeClearAll).append(GroupSizeSlider);

            $(filterAllModal + " .GroupSizeSliderMainDiv").not(':first').remove();
            $(filterAllModal + " .GroupSizeSliderclearAll").not(':first').remove();
            $(filterAllModal + " .GroupSizeSlider").not(':first').remove();


            $(filterAllModal + " .GroupSizeSliderMainDiv input").hide();

            //$(".GroupSizeSliderMainDiv .input-wrapper .GroupSizeSlider ").css('margin-bottom','20px');
            if ($(filterAllModal + " .GroupSizeSliderMainDiv div")[0] != undefined) {
                $(filterAllModal + " .GroupSizeSliderMainDiv div")[0].append($(".form-actions .btnGroupSizeSubmit")[0]);
                $(filterAllModal + " .btnGroupSizeSubmit").removeClass("hidden");
                $(".btnGroupSizeSubmit").eq(1).addClass("hidden");


            }

            if ($(filterAllModal + " .GroupSizeSliderclearAll")[0] != undefined) {
                var clearAllButton = $('<button>', {

                    class: 'btnClearAll', // optional classes
                    click: function (event) {
                        event.preventDefault();
                        $('#SearchLoader').modal('show');
                        $("#allfilters #GroupSizeFrom").val(null);
                        $("#allfilters #GroupSizeTo").val(null);
                        $("#filterAllModal .mingroupsize").val(null);
                        $("#filterAllModal #maxgroupsize").val(null);

                        $(this).hide();
                        $('#ContentFilter').submit();
                    }
                });
                clearAllButton.html('<i class="fa fa-times"></i> Clear Filter')
                    .css({
                        'margin-bottom': '15px',
                        'width': '100%'
                    });

                $(filterAllModal + " .GroupSizeSliderclearAll")[0].append(clearAllButton[0]);
            }
            var sGroupSizeFrom = '';
            var sGroupSizeTo = '';

            if ($(filterAllModal + " #GroupSizeFrom").val() != '') {
                sGroupSizeFrom = $(filterAllModal + " #GroupSizeFrom").val();
            }

            if ($(filterAllModal + " #GroupSizeTo").val() != '') {
                sGroupSizeTo = $(filterAllModal + " #GroupSizeTo").val();
            }

            if (sGroupSizeFrom == '') {
                sGroupSizeFrom = groupSizeSliderFrom;
            }
            if (sGroupSizeTo == '') {
                sGroupSizeTo = groupSizeSliderTo;
            }

            if ($(filterAllModal+ ".wrunner.wrunner_theme_default.wrunner_direction_horizontal").length <= 1) {
                var wRunnerGroupSize = $(filterAllModal + ' .GroupSizeSlider').wRunner({
                    type: 'range',
                    rangeValue: {
                        minValue: sGroupSizeFrom,
                        maxValue: sGroupSizeTo,
                    },
                    limits: {
                        minLimit: 1,
                        maxLimit: sGroupSizeTo,
                    },
                    valueNoteDisplay: true,
                    preventOverlap: true,
                    onValueUpdate: function (values) {

                        //$("#lblAge").text("" + values.minValue + " - " + values.maxValue + " "); 
                        $(filterAllModal + ' #txtMinGroup').val(values.minValue);
                        $(filterAllModal + ' #txtMaxGroup').val(values.maxValue);
                        $("#filterAllModal .mingroupsize").val(values.minValue);
                        $("#filterAllModal .maxgroupsize").val(values.maxValue);


                        var left1 = parseFloat($(filterAllModal + ' .GroupSizeSlider .wrunner__valueNote').eq(0).css('left'))
                        var left2 = parseFloat($(filterAllModal + ' .GroupSizeSlider .wrunner__valueNote').eq(1).css('left'))

                        var diff = Math.abs(left1 - left2);

                        if (diff < 50) {

                            $(filterAllModal + ' .GroupSizeSlider .wrunner__valueNote').eq(0).css('margin-left', '-18px');
                            $(filterAllModal + ' .GroupSizeSlider .wrunner__valueNote').eq(1).css('margin-left', '18px');

                        }

                    }
                })


                $(filterAllModal + ' .GroupSizeSlider .wrunner__valueNote').eq(0).css('left', '-8.5%');
                $(filterAllModal + ' .GroupSizeSlider .wrunner__valueNote').eq(1).css('left', '91.5%');

                
                var txtMinGroup = $('<input type="text" id="txtMinGroup"  class="txtFilter mingroupsize"></input>');

                txtMinGroup.val(sGroupSizeFrom);
                var txtMaxGroup = $('<input type="text" id="txtMaxGroup"  class="txtFilter maxgroupsize"></input>');
                txtMaxGroup.val(sGroupSizeTo);
                var lblMinGroup = $('<label text="From" style="display: inline;" class="lblSliders" for="txtMinGroup">From</label>');
                var lblMaxGroup = $('<label text="To" class="lblSliders" for="txtMaxGroup">To</label>');
                var container = $("<div></div>").addClass("inline-textbox-containerGroup clearfix");

                container.append(lblMinGroup);
                container.append(lblMaxGroup);
                container.append(txtMinGroup);
                container.append(txtMaxGroup);

                //container.insertAfter("#histogramSliderWeight");

                $(filterAllModal + ' .GroupSizeSlider .wrunner').append(container);

              

            }

        }



    }
   

    //End Group size


    LoadGroupSizeFilter('.dropdown-filters');
    $(".btnGroupSizeSubmit").on('click', function (e) {

        e.preventDefault();
        $("#SearchLoader .modal-body h2").text("Just one second whilst we find the best experiences for you!");
        $("#SearchLoader .modal-body h2").css("font-size", "1.3em");
        $('#SearchLoader').modal('show');
        var sGroupSizeFrom = $('.GroupSizeSlider .wrunner__valueNote')[0].innerText;
        var sGroupSizeTo = $('.GroupSizeSlider .wrunner__valueNote')[1].innerText;
        // $("#ContentFilter").submit();
        $("#GroupSizeFrom").val(sGroupSizeFrom);
        $("#GroupSizeTo").val(sGroupSizeTo);
        $("#sliderGroupSizeFrom").val(sGroupSizeFrom);
        $("#sliderGroupSizeTo").val(sGroupSizeTo);
        $('#ContentFilter').submit();

    });

    //start Weight filter

    function LoadWeightFilter(filterAllModal) {
        var sliderFrom = '';
        var sliderTo = '';
        sliderFrom = $("#sliderForm").val();
        sliderTo = $("#sliderTo").val();
        var hdnWeightHideByDefault = $(filterAllModal+ " #WeightHideByDefault").val();


        if (sliderFrom == '' && sliderTo == '') {
            if ($(filterAllModal + " .btnCrossForWeight").length > 0) {

                var name = $(filterAllModal + " .btnCrossForWeight").val();
                var values = name.split(',');
                sliderFrom = values[0];
                sliderTo = values[1];
                $("#From").val(sliderFrom);
                $("#To").val(sliderTo);
                $("#sliderForm").val(sliderFrom);
                $("#sliderTo").val(sliderTo);
            }
            else {
                sliderFrom = '10';
                sliderTo = '250';
            }
        }

        if ($(filterAllModal + ".btnShowMoreFilter").length > 0) {
            if (hdnWeightHideByDefault == '') {
                hdnWeightHideByDefault = $(filterAllModal + " #WeightHideByDefault").val();
                $(filterAllModal + " #WeightHideByDefault").val(hdnWeightHideByDefault);
                if ($(filterAllModal + " #WeightHideByDefault") != undefined) {
                    if ($(filterAllModal + " #WeightHideByDefault").val() == "true") {
                        $(filterAllModal + ' .WeightSliderMainDiv').addClass("hidden");
                    }
                }
            }

        }
        if ($(filterAllModal + " .WeightSliderMainDiv").length > 0) {
            var weightClearAll = $(filterAllModal + " .WeightClearAll").detach();
            var weightSlider = $(filterAllModal + " .WeightSlider").detach();
            $(filterAllModal +  " .WeightSliderMainDiv div").append(weightClearAll).append(weightSlider);

            //$(".WeightSlider").detach().appendTo(".WeightSliderMainDiv div");
            $(filterAllModal + " .WeightSliderMainDiv input").hide();
            if ($(filterAllModal + " .WeightSliderMainDiv")[0] != undefined) {
                $(filterAllModal + " .WeightSliderMainDiv div")[0].append($(".form-actions .btnWeightSubmit")[0]);
                $(filterAllModal + " .btnWeightSub").removeClass("hidden");
                // $(".WeightSliderMainDiv div").css('width', '90%');

            }

            if ($(filterAllModal + " .WeightClearAll")[0] != undefined) {
                var clearAllButton = $('<button>', {

                    class: 'btnClearAll', // optional classes
                    click: function () {
                        event.preventDefault();
                        $('#SearchLoader').modal('show');

                        $("#From").val(null);
                        $("#To").val(null);
                        $("#sliderForm").val(null);
                        $("#sliderTo").val(null);
                        $(this).hide();
                        $('#ContentFilter').submit();
                    }
                });
                clearAllButton.html('<i class="fa fa-times"></i> Clear Filter')
                    .css({
                        'margin-bottom': '15px',
                        'width': '100%'
                    });

                $(filterAllModal + " .WeightClearAll")[0].append(clearAllButton[0]);
            }
            var wRunnerWeight = $(filterAllModal + ' .WeightSlider').wRunner({
                type: 'range',
                rangeValue: {
                    minValue: sliderFrom,
                    maxValue: sliderTo,
                },
                limits: {
                    minLimit: 0,
                    maxLimit: 250,
                },
                valueNoteDisplay: true,
                onValueUpdate: function (values) {
                    var minValueInStone = weightConverterKgToStone(values.minValue);
                    var maxValueInStone = weightConverterKgToStone(values.maxValue);
                    $("#lblInStone").text(" or from " + minValueInStone.toFixed(2) + " - " + maxValueInStone.toFixed(2) + " stone");
                    $("#lblInKg").text("Weight from " + values.minValue + " - " + values.maxValue + " kg");
                    $('#txtMinWeight').val(values.minValue);
                    $('#txtMaxWeight').val(values.maxValue);

                    var left1 = parseFloat($(filterAllModal + ' .WeightSlider .wrunner__valueNote').eq(0).css('left'))
                    var left2 = parseFloat($(filterAllModal + ' .WeightSlider .wrunner__valueNote').eq(1).css('left'))

                    var diff = Math.abs(left1 - left2);

                    if (diff < 70) {

                        $(filterAllModal + ' .WeightSlider .wrunner__valueNote').eq(0).css('margin-left', '-18px');
                        $(filterAllModal + ' .WeightSlider .wrunner__valueNote').eq(1).css('margin-left', '18px');

                    }

                }
            })

            if ($(filterAllModal + ' .WeightSlider .wrunner').length > 0) {

                const addEightPointFive = s => `${+s.replace(/%$/, '') - 8.5}%`;
                var leftHandle = $(filterAllModal + " .WeightSlider .wrunner__handle_direction_horizontal")[0].style.left;
                var rightHandle = $(filterAllModal + " .WeightSlider .wrunner__handle_direction_horizontal")[1].style.left;

                $(filterAllModal + ' .WeightSlider .wrunner__valueNote')[0].style.left = addEightPointFive(leftHandle);
                $(filterAllModal + ' .WeightSlider .wrunner__valueNote')[1].style.left = addEightPointFive(rightHandle);
            }

            $(filterAllModal + ' .btnWeightSubmit').click(function (e) {
                e.preventDefault();
                $("#SearchLoader .modal-body h2").text("Just one second whilst we find the best experiences for you!");
                $("#SearchLoader .modal-body h2").css("font-size", "1.3em");
                $('#SearchLoader').modal('show');
                var sWeightFrom = $(filterAllModal + '.WeightSlider .wrunner__valueNote')[0].innerText;
                var sWeightTo = $(filterAllModal + ' .WeightSlider .wrunner__valueNote')[1].innerText;
                var minValue = sWeightFrom;
                var minValueInStone = weightConverterKgToStone(sWeightFrom);
                var maxValue = sWeightTo;
                var maxValueInStone = weightConverterKgToStone(sWeightTo);
                $("#FromInStone").val(minValueInStone);
                $("#ToInStone").val(maxValueInStone);
                if ($("#lblInStone").val() != '') {
                    $("#lblInStone").val();
                }
                if ($("#lblInKg").val() != '') {
                    $("#lblInKg").val();
                }
                $("#lblInStone").text(" Weight from " + minValueInStone.toFixed(2) + " - " + maxValueInStone.toFixed(2) + "in stone");
                //$("#lblInKg").text("Weight from " + minValue + " - " + maxValue + " kg"); 
                $("#From").val(sWeightFrom);
                $("#To").val(sWeightTo);


                $("#sliderForm").val(sWeightFrom);
                $("#sliderTo").val(sWeightTo);
                $("#ContentFilter").submit();
            });


            var txtMinWeight = $('<input type="text" id="txtMinWeight"  class="txtFilter"></input>');
            txtMinWeight.val(sliderFrom);
            var txtMaxWeight = $('<input type="text" id="txtMaxWeight"  class="txtFilter"></input>');
            txtMaxWeight.val(sliderTo);
            var lblMinWeight = $('<label text="From" style="display: inline;" class="lblSliders" for="txtMinWeight">From</label>');
            var lblMaxWeight = $('<label text="To" class="lblSliders" for="txtMaxWeight">To</label>');
            var container = $("<div></div>").addClass(filterAllModal + " inline-textbox-containerWeight");
            container.append(lblMinWeight);
            container.append(lblMaxWeight);
            container.append(txtMinWeight);
            container.append(txtMaxWeight);

            //container.insertAfter("#histogramSliderWeight"); 
            $(filterAllModal + ' .WeightSlider .wrunner').append(container);


            $(filterAllModal + ' .WeightSlider .wrunner').append('<label id="lblInStone"> </label>');

            var weightValue = wRunnerWeight.getValue();

            //var minValue = $(".wrunner__valueNote ")[0].innerText;
            var minValue = weightValue.minValue;
            var minValueInStone = weightConverterKgToStone(minValue);
            //var maxValue = $(".wrunner__valueNote ")[1].innerText;
            var maxValue = weightValue.maxValue;
            var maxValueInStone = weightConverterKgToStone(maxValue);
            //$("#lblInKg").text("Weight from " + minValue + " - " + maxValue + " kg"); 
            $("#lblInStone").text("Weight from " + minValueInStone.toFixed(2) + " - " + maxValueInStone.toFixed(2) + " in stone");

        }
    }
    $(document).on('mousedown', '.useMyLocation, .useMyLocation *', function (e) {

        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(function (position) {
                var lat = position.coords.latitude;
                var lng = position.coords.longitude;

                var geocoder = new google.maps.Geocoder();
                var latlng = { lat: lat, lng: lng };

                geocoder.geocode({ location: latlng }, function (results, status) {
                    if (status === "OK" && results[0]) {

                        $("#Location").val(results[0].formatted_address);
                        $("#Location").val(results[0].formatted_address);
                    } else {
                        alert("Unable to detect location.");
                    }
                });
            }, function (error) {
                alert("Location access denied.");
            });
        } else {
            alert("Geolocation not supported in this browser.");
        }
    });
});

//LoadWeightFilter('.dropdown-filters');

//End Weight filter


$('.btnCrossForGroupSize').on("mousedown", function (event) {

    $('#SearchLoader').modal('show');
    event.preventDefault();
    $("#GroupSizeFrom").val(null);
    $("#GroupSizeTo").val(null);
    $(this).hide();
    $('#ContentFilter').submit();
});

$('.btnCross').on('click', function () {

    $('#SearchLoader').modal('show');
    $(this).hide();
    $('#ContentFilter').submit();
});

$('.locationCrossBtn').on('click', function (event) {
    event.preventDefault();
    $("#Location").val(null);
    $("#filterAllModal #Location").val(null);
    $("#hidDistance").val(null);
    $('#SearchLoader').modal('show');
    $(this).hide();
    $('#ContentFilter').submit();
});


$('.btnCrossForWeight').on("click", function (event) {
    event.preventDefault();
    $('#SearchLoader').modal('show');

    $("#From").val(null);
    $("#To").val(null);
    $("#sliderForm").val(null);
    $("#sliderTo").val(null);
    $(this).hide();
    $('#ContentFilter').submit();
});
$('.clear-all-link, #resetFilters').on('click', function () {
    $('#SearchLoader').modal('show');
    sessionStorage.clear(); // or remove specific keys
    localStorage.clear();   // optional
    $('input[type="hidden"]').val('');
    setTimeout(function () {
        window.location.href = window.location.origin + window.location.pathname;
    }, 25);

});

$(document).ready(function () {

    $("#filterAllModal #btnSubmitFilters").on('click', function (e) {

        e.preventDefault();

        var locationTextModal = $(".modal-section #Location").val();
        $("#Location").val(locationTextModal);

        $('#filterAllModal').modal('hide');
        $("#SearchLoader .modal-body h2").text("Just one second whilst we find the best experiences for you!");
        $("#SearchLoader .modal-body h2").css("font-size", "1.3em");
        $('#SearchLoader').modal('show');
        if ($('#filterAllModal .AgeSlider .wrunner__valueNote').length != 0) {
            var sAgeFrom = $('#filterAllModal .AgeSlider .wrunner__valueNote')[0].innerText;
            var sAgeTo = $('#filterAllModal .AgeSlider .wrunner__valueNote')[1].innerText;

            // $("#ContentFilter").submit();
            $("#MinAge").val(sAgeFrom);
            $("#MaxAge").val(sAgeTo);
            $(".minage").val(sAgeFrom);
            $(".maxage").val(sAgeTo); 
        }

        if ($('#filterAllModal .GroupSizeSlider  .wrunner__valueNote').length != 0) {
            var sGroupSizeFrom = $('#filterAllModal .GroupSizeSlider  .wrunner__valueNote')[0].innerText;
            var sGroupSizeTo = $('#filterAllModal .GroupSizeSlider  .wrunner__valueNote')[1].innerText;

            // $("#ContentFilter").submit();
            $("#GroupSizeFrom").val(sGroupSizeFrom);
            $("#GroupSizeTo").val(sGroupSizeTo);
            $(".mingroupsize").val(sGroupSizeFrom);
            $(".maxgroupsize").val(sGroupSizeTo);
        }
        $('#ContentFilter').submit();

    });

    if ($(".locationCrossBtn").length > 0) {

        var value = $(".locationCrossBtn").val();
        if (value.indexOf(',') > -1) {
            var arry = value.split(',');

            $("#Location").val(arry[0]);
            var distance = arry[1].split(' ');
            $("#hidDistance").val(distance[1]);
        }
    }
    //Page filter clear button code
    if ($(".PageClearAll").length > 0) {
        var pageClearAll = $(".PageClearAll").detach(); // safely remove and keep it
        $(".SubmitPageFilter .appearance-full").prepend(pageClearAll);

        if ($(".PageClearAll")[0] != undefined) {
            var clearAllButton = $('<button>', {

                class: 'btnClearAll', // optional classes
                click: function () {
                    $('#SearchLoader').modal('show');
                    $(this).hide();
                    $(".SubmitPageFilter").find(".radiocheckbox").find("input[type='checkbox']").prop("checked", false);
                    $('#ContentFilter').submit();
                }
            });
            clearAllButton.html('<i class="fa fa-times"></i> Clear Filter')
                .css({
                    'margin-bottom': '15px',
                    'width': '100%'
                });

            $(".PageClearAll")[0].append(clearAllButton[0]);
        }

    }

    //PriceBand clear button code
    if ($(".PriceBandClearFilter").length > 0) {
        var PriceBandClearFilter = $(".PriceBandClearFilter").detach(); // safely remove and keep it
        $(".SubmitPriceBandFilter .appearance-full").prepend(PriceBandClearFilter);

        if ($(".PriceBandClearFilter")[0] != undefined) {
            var clearFilterButton = $('<button>', {

                class: 'PriceBandClearFilter', // optional classes
                click: function () {
                    $('#SearchLoader').modal('show');
                    $(this).hide();
                    $(".SubmitPriceBandFilter").find(".radiocheckbox").find("input[type='checkbox']").prop("checked", false);
                    $('#ContentFilter').submit();
                }
            });
            clearFilterButton.html('<i class="fa fa-times"></i> Clear Filter')
                .css({
                    'margin-bottom': '15px',
                    'width': '100%'
                });

            $(".PriceBandClearFilter")[0].append(clearFilterButton[0]);
        }

    }

    //Price filter clear button
    if ($(".PriceClearAll").length > 0) {
        var priceClearAll = $(".PriceClearAll").detach(); // safely remove and keep it
        $(".histogramSliderMainDivPrice .appearance-").prepend(priceClearAll);

        if ($(".PriceClearAll")[0] != undefined) {
            var clearAllButton = $('<button>', {

                class: 'btnClearAll', // optional classes
                click: function () {
                    event.preventDefault();
                    $('#SearchLoader').modal('show');
                    $("#MinPrice").val(null);
                    $("#MaxPrice").val(null);
                    $('#ContentFilter').submit();
                }
            });
            clearAllButton.html('<i class="fa fa-times"></i> Clear Filter')
                .css({
                    'margin-bottom': '15px',
                    'width': '100%'
                });

            $(".PriceClearAll")[0].append(clearAllButton[0]);
        }
    }

    $(".histogramSliderMainDivPrice .appearance-").css("width", "325px");


    //Location
    if ($(".postCode")[0] != undefined) {
        var container = $(".distance .appearance-full");
        if ($(".LocationClearAll").length > 0) {
            container.prepend($(".postCode").eq(0));         // Third
            container.prepend($(".postCode").eq(1));         // Second
            container.prepend($(".LocationClearAll"));       // First

        }
        else {
            $(".distance .appearance-full")[0].prepend($(".postCode")[1]);
            $(".distance .appearance-full")[0].prepend($(".postCode")[0]);
        }

        $(".distance .appearance-full")[0].append($(".form-actions .btnLocationSub")[0]);
        $(".btnLocationSub").removeClass("hidden");
        $(".btnLocationSub").eq(1).addClass("hidden");
    }

    if ($(".LocationClearAll")[0] != undefined) {
        var clearAllButton = $('<button>', {

            class: 'btnClearAll', // optional classes
            click: function () {
                event.preventDefault();
                $("#Location").val(null);
                $("#filterAllModal #Location").val(null);
                $("#hidDistance").val(null);
                $('#SearchLoader').modal('show');
                $(this).hide();
                $('#ContentFilter').submit();
            }
        });
        clearAllButton.html('<i class="fa fa-times"></i> Clear Filter')
            .css({
                'margin-bottom': '15px',
                'width': '100%'
            });

        $(".LocationClearAll")[0].append(clearAllButton[0]);
    }
    $(".btnLocationSub").on('click', function (e) {
        e.preventDefault();
        var postcode = $('#Location').val();

        if (postcode != "") {
            $("#SearchLoader .modal-body h2").text("Just one second whilst we find the best experiences for you!");
            $("#SearchLoader .modal-body h2").css("font-size", "1.3em");
            $('#SearchLoader').modal('show');
            $(".hidpostcode").val(postcode);
            $('#ContentFilter').submit();
        }
        else {
            $('#Location').css('border-color', 'red');
            $('#filterAllModal #Location').css('border-color', 'red');
            $('#SearchLoader').modal('hide');
            return false;
        }
    });
    // histogram parameters
    var histogramType;
    var mainDiv;
    var mainClass;
    var productListCount;
    var products;
    var step;
    function DisplayHistogram(mainDiv, mainClass, histogramType) {

        mainDiv = mainDiv + histogramType;
        mainClass = mainClass + histogramType;

        if ($("." + mainDiv)[0] != undefined) {
            var minSlideRange = 0;
            var sliderMin = "";
            var sliderMax = "";
            sliderMin = $("#SliderMin" + histogramType)[0].value;
            sliderMax = $("#SliderMax" + histogramType)[0].value;

            if (sliderMax == '') {
                sliderMin = '<xsl:value-of select="$page/Contents/Content/Content/model/instance/PriceFilter/@SliderMinPrice"/>';
                sliderMax = '<xsl:value-of select="$page/Contents/Content/Content/model/instance/PriceFilter/@SliderMaxPrice"/>';

            }

            var maxLimit = $("#Max" + histogramType + "Limit")[0].value;
            var min = $("#Min" + histogramType)[0].value;
            var max = $("#Max" + histogramType)[0].value;

            if (max == "" || max.indexOf('+') != -1) {
                max = parseInt(sliderMax);
            }
            else {
                if (max.indexOf('£') != -1) {
                    max = max.replace('£', '');
                }
                if (max.indexOf('+') != -1) {
                    max = max.replace('+', '');
                }
            }

            if (min == "") {
                min = parseInt(sliderMin);

            }
            else {
                if (min.indexOf('£') != -1) {

                    min = min.replace('£', '');
                }
            }

            step = $("#" + histogramType + "Step")[0].value;
            productListCount = $("#" + histogramType + "ListCount")[0].value;
            products = productListCount.split(",");

            //var maxSlideRange = products.length * step; 
            var maxSlideRange = sliderMax;

            var numBins = products.length;
            //$("#" + mainClass + " .selected-range")[0].text(""sliderMin + sliderMax); 
            var iDiv = document.createElement('div');
            iDiv.className = 'form-group select-group ' + mainClass;

            numBins = numBins - 1;
            $("." + mainDiv).append(iDiv);
            $("." + mainClass).attr('id', mainClass);
            $("#" + mainClass).detach().appendTo("." + mainDiv + " div");
            $("." + mainDiv + " input").hide();

            data = dataFactory(numBins, true);

            $("#" + mainClass).histogramSlider({
                data: data,
                sliderRange: [0, sliderMax],

                optimalRange: 0,
                selectedRange: [min, max],
                numberOfBins: numBins,
                showTooltips: false,
                showSelectedRange: true,
                customText: '',
                maxLimit: maxLimit
            });

            if ($("." + mainDiv)[0] != undefined) {

                $("." + mainDiv + " div")[0].append($(".form-actions .btn" + histogramType + "Submit")[0]);

                $(".btn" + histogramType + "Submit").removeClass("hidden");
                $("#histogramSliderPrice-slider").css("margin-left", "18px");

                var txtMinPrice = $('<input type="text" id="histogramSliderPrice-sliderMin"  class="txtFilter"></input>');
                txtMinPrice.val(min);
                var txtMaxPrice = $('<input type="text" id="histogramSliderPrice-sliderMax"  class="txtFilter"></input>');
                if (max > maxLimit) {
                    txtMaxPrice.val(maxLimit + '+');
                } else {
                    txtMaxPrice.val(max);
                }
                var container = $("<div></div>").addClass("wrapper");
                var lblMinPrice = $('<label text="From" style="display: inline;" class="lblSliders" for="histogramSliderPrice-sliderMin">From (\u00A3)</label>');
                var lblMaxPrice = $('<label text="To" class="lblSliders" for="histogramSliderPrice-sliderMax" >To (\u00A3)</label>');
                // $("." + mainDiv + " div")[0].append($(".remove-" + histogramType + "Filter")[0]);
                var container = $("<div></div>").addClass("inline-textbox-containerPrice");
                container.append(lblMinPrice);
                container.append(lblMaxPrice);
                container.append(txtMinPrice);
                container.append(txtMaxPrice);

                container.insertAfter("#histogramSliderPrice");
                $("#" + mainClass + " .selected-range").hide();

                $("#histogramSliderPrice-slider span:first-of-type").attr("title", min)
                var MaxPriceLimit = $("#MaxPriceLimit").val();

                if (max > MaxPriceLimit) {
                    $("#histogramSliderPrice-slider span:last-of-type").attr("title", MaxPriceLimit + '+');
                }
                else {
                    $("#histogramSliderPrice-slider span:last-of-type").attr("title", max);
                }


                $("#histogramSliderPrice-slider span:first-of-type").attr("id", "ValueNoteSlider");
                $("#histogramSliderPrice-slider span:last-of-type").attr("data-bs-toggle", "popover");


            }

            $(".btn" + histogramType + "Submit").click(function (event) {

                event.preventDefault();
                $("#SearchLoader .modal-body h2").text("Just one second whilst we find the best experiences for you!");
                $("#SearchLoader .modal-body h2").css("font-size", "1.3em");
                $('#SearchLoader').modal('show');
                if ($("#" + mainClass + " .selected-range")[0] != undefined) {
                    var range = $("#" + mainClass + " .selected-range")[0].innerText.split("-");

                    $('#Min' + histogramType).val(range[0]);

                    $('#Max' + histogramType).val(range[1]);
                    $('#ContentFilter').submit();
                }
            });

            $('.btnCrossFor' + histogramType).on('mousedown', function (event) {
                event.preventDefault();
                $('#SearchLoader').modal('show');
                $("#Min" + histogramType).val(null);
                $("#Max" + histogramType).val(null);
                $('#ContentFilter').submit();
            });

        }
    }

    $('.btnCrossForAge').on("mousedown", function (event) {

        event.preventDefault();
        $('#SearchLoader').modal('show');
        $("#MinAge").val(null);
        $("#MaxAge").val(null);
        $(this).hide();
        $('#ContentFilter').submit();
    });


    function dataFactory(numberOfBins, group) {
        var data = { "items": [] };


        if (products.length != undefined) {
            for (var i = 0; i < products.length - 1; i++) {
                var arrProduct = products[i].split(":");



                if (group) {
                    data.items.push({ "value": parseInt(arrProduct[0]) * step, "count": parseFloat(arrProduct[1]) });


                } else {
                    data.items.push({ "value": parseInt(arrProduct[0]) * step });
                }
            }

            return data;
        }
    }



    //mainDiv = 'histogramSliderMainDiv';
    //mainClass = 'histogramSlider';
    //histogramType = 'Price';

    //DisplayHistogram(mainDiv, mainClass, histogramType);
    DisplayHistogram('histogramSliderMainDiv', 'histogramSlider', 'Price');


    if ($(".btnShowMoreFilter").length > 0) {

        if ($("#PriceHideByDefault") != undefined) {
            if ($("#PriceHideByDefault").val() == "true") {

                $('.histogramSliderMainDivPrice').addClass("hidden");
            }
        }

        if ($("#AgeHideByDefault") != undefined) {
            if ($("#AgeHideByDefault").val() == "true") {
                $('.AgeSliderMainDiv').addClass("hidden");
            }
        }

        if ($("#WeightHideByDefault") != undefined) {
            if ($("#WeightHideByDefault").val() == "true") {
                $('.WeightSliderMainDiv').addClass("hidden");
            }
        }


        if ($("#GroupSizeHideByDefault") != undefined) {
            if ($("#GroupSizeHideByDefault").val() == "true") {
                $('.GroupSizeSliderMainDiv').addClass("hidden");
            }
        }


        if ($("#LocationHideByDefault") != undefined) {
            if ($("#LocationHideByDefault").val() == "true") {
                $('.distance').addClass("hidden");
            }
        }

        if ($("#OfferHideByDefault") != undefined) {
            if ($("#OfferHideByDefault").val() == "true") {
                $("label[for='OfferFilter']").closest(".form-group").addClass("hidden");
            }
        }
    }
    //Price filter textchange event code
    $(document).on('input', '#histogramSliderPrice-sliderMin ,#histogramSliderPrice-sliderMax', function () {

        var id = $(this).attr('id');
        var Minamount;
        var Maxamount;
        var products;
        var sliderMax;
        var numBins;
        var maxLimit;
        var step;
        var mainClass;
        if (id.includes('Price')) {
            Minamount = $("#histogramSliderPrice-sliderMin").val();
            Maxamount = $("#histogramSliderPrice-sliderMax").val();
            productListCount = $("#PriceListCount")[0].value;
            products = productListCount.split(",");
            sliderMax = $("#SliderMaxPrice")[0].value;
            numBins = products.length;
            numBins = numBins - 1;
            maxLimit = $("#MaxPriceLimit")[0].value;
            step = $("#PriceStep").val();
            mainClass = '#histogramSliderPrice'
        }


        if (parseFloat(Minamount) < parseFloat(Maxamount)) {


            if ($.isNumeric(Minamount) && $.isNumeric(Maxamount)) {

                data = dataFactory(numBins, true, step);
                $(mainClass).histogramSlider({
                    data: data,
                    sliderRange: [0, sliderMax],

                    optimalRange: 0,
                    selectedRange: [Minamount, Maxamount],
                    numberOfBins: numBins,
                    showTooltips: false,
                    showSelectedRange: true,
                    customText: '',
                    maxLimit: maxLimit
                });
                $(mainClass + "-slider-value").hide();

                function dataFactory(numberOfBins, group, step) {
                    var data = { "items": [] };


                    if (products.length != undefined) {
                        for (var i = 0; i < products.length - 1; i++) {
                            var arrProduct = products[i].split(":");

                            if (group) {
                                data.items.push({ "value": parseInt(arrProduct[0]) * step, "count": parseFloat(arrProduct[1]) });


                            } else {
                                data.items.push({ "value": parseInt(arrProduct[0]) * step });
                            }
                        }

                        return data;
                    }
                }

            } else {
                console.log("Not a number");
            }
        }
    });

    //Weight,age and group size filter text change event
    $(document).on('input', '#txtMinWeight ,#txtMaxWeight, #txtMinGroup,#txtMaxGroup,#txtMinAge,#txtMaxAge', function () {

        var id = $(this).attr('id');
        var MinValue;
        var MaxValue;
        var mainClass;

        if (id.includes('Weight')) {
            MinValue = $("#txtMinWeight").val();
            MaxValue = $("#txtMaxWeight").val();
            if ($.isNumeric(MinValue) && $.isNumeric(MaxValue)) {
                if (parseFloat(MinValue) < parseFloat(MaxValue)) {
                    /* debugger;*/
                    SliderMaxValue = $("#sliderTo").val();

                    rightValuePer = (parseFloat(MaxValue) / parseFloat(SliderMaxValue)) * 100;
                    leftValuePer = (parseFloat(MinValue) / parseFloat(SliderMaxValue)) * 100;
                    $('.WeightSlider .wrunner__pathPassed').css("width", parseFloat(rightValuePer - leftValuePer) + '%');
                    $('.WeightSlider .wrunner__pathPassed').css("left", leftValuePer + '%');

                    $('.WeightSlider .wrunner__handle')[0].style.left = leftValuePer + '%';
                    $('.WeightSlider .wrunner__handle')[1].style.left = rightValuePer + '%';

                    $('.WeightSlider .wrunner__valueNote')[0].innerText = MinValue;
                    $('.WeightSlider .wrunner__valueNote')[1].innerText = MaxValue;

                    $('.WeightSlider .wrunner__valueNote')[0].style.left = leftValuePer + '%';
                    $('.WeightSlider .wrunner__valueNote')[1].style.left = rightValuePer + '%';
                    if (parseFloat(MaxValue) > 250) {
                        $('.WeightSlider .wrunner__handle')[1].style.left = '100%';
                        $('.WeightSlider .wrunner__pathPassed').css("width", '100%');
                        $('.WeightSlider .wrunner__valueNote')[1].style.left = '100%';
                    }

                    var minValueInStone = weightConverterKgToStone(MinValue);
                    var maxValueInStone = weightConverterKgToStone(MaxValue);
                    $("#lblInStone").text(" or from " + minValueInStone.toFixed(2) + " - " + maxValueInStone.toFixed(2) + " stone");

                }
            }
        }
        else {

            if (id.includes('Group')) {
                MinValue = $("#txtMinGroup").val();
                MaxValue = $("#txtMaxGroup").val();
                if ($.isNumeric(MinValue) && $.isNumeric(MaxValue)) {
                    if (parseFloat(MinValue) < parseFloat(MaxValue)) {

                        SliderMaxValue = $("#sliderGroupSizeTo").val();

                        rightValuePer = (parseFloat(MaxValue) / parseFloat(SliderMaxValue)) * 100;
                        leftValuePer = (parseFloat(MinValue) / parseFloat(SliderMaxValue)) * 100;
                        $('.GroupSizeSlider .wrunner__pathPassed').css("width", parseFloat(rightValuePer - leftValuePer) + '%');
                        $('.GroupSizeSlider .wrunner__pathPassed').css("left", leftValuePer + '%');

                        $('.GroupSizeSlider .wrunner__handle')[0].style.left = leftValuePer + '%';
                        $('.GroupSizeSlider .wrunner__handle')[1].style.left = rightValuePer + '%';

                        $('.GroupSizeSlider .wrunner__valueNote')[0].innerText = MinValue;
                        $('.GroupSizeSlider .wrunner__valueNote')[1].innerText = MaxValue;

                        $('.GroupSizeSlider .wrunner__valueNote')[0].style.left = leftValuePer + '%';
                        $('.GroupSizeSlider .wrunner__valueNote')[1].style.left = rightValuePer + '%';

                        if (parseFloat(MaxValue) > 15) {
                            $('.GroupSizeSlider .wrunner__handle')[1].style.left = '100%';
                            $('.GroupSizeSlider .wrunner__pathPassed').css("width", '100%');
                            $('.GroupSizeSlider .wrunner__valueNote')[1].style.left = '100%';
                        }
                    }
                }
            }
            else {
                if (id.includes('Age')) {
                    MinValue = $("#txtMinAge").val();
                    MaxValue = $("#txtMaxAge").val();
                    if ($.isNumeric(MinValue) && $.isNumeric(MaxValue)) {
                        if (parseFloat(MinValue) < parseFloat(MaxValue)) {

                            SliderMaxValue = $("#SliderMaxAge").val();

                            rightValuePer = (parseFloat(MaxValue) / parseFloat(SliderMaxValue)) * 100;
                            leftValuePer = (parseFloat(MinValue) / parseFloat(SliderMaxValue)) * 100;
                            $('.AgeSlider .wrunner__pathPassed').css("width", parseFloat(rightValuePer - leftValuePer) + '%');
                            $('.AgeSlider .wrunner__pathPassed').css("left", leftValuePer + '%');

                            $('.AgeSlider .wrunner__handle')[0].style.left = leftValuePer + '%';
                            $('.AgeSlider .wrunner__handle')[1].style.left = rightValuePer + '%';

                            $('.AgeSlider .wrunner__valueNote')[0].innerText = MinValue;
                            $('.AgeSlider .wrunner__valueNote')[1].innerText = MaxValue;

                            $('.AgeSlider .wrunner__valueNote')[0].style.left = leftValuePer + '%';
                            $('.AgeSlider .wrunner__valueNote')[1].style.left = rightValuePer + '%';
                            if (parseFloat(MaxValue) > 99) {
                                $('.AgeSlider .wrunner__handle')[1].style.left = '100%';
                                $('.AgeSlider .wrunner__pathPassed').css("width", '100%');
                                $('.AgeSlider .wrunner__valueNote')[1].style.left = '100%';
                            }
                        }
                    }
                }

            }
        }
    });

    $(document).on('keydown', '#txtMinWeight, #txtMaxWeight, #txtMinGroup, #txtMaxGroup, #txtMinAge, #txtMaxAge, #Location, #histogramSliderPrice-sliderMin, #histogramSliderPrice-sliderMax', '#txtLocationFrom', function (e) {
        if (e.key === 'Enter' || e.keyCode === 13) {
            e.preventDefault();
            return false;
        }
    });
    function weightConverterKgToStone(valNum, lblId) {
        return valNum * 0.1574;
    }
});

//pagefilter
$('#filterAllModal .SubmitPageFilter .radiocheckbox label').on('click', function (e) {
   
    var isChecked = $(this).find("input[type='checkbox']").prop("checked");
    var id = $(this).find("input[type='checkbox']").prop("id");

    $("#filterAllModal .SubmitPageFilter").find(".radiocheckbox").find("input[type='checkbox'][id='" + id + "'").prop("checked", !isChecked);
    $("#allfilters .SubmitPageFilter").find(".radiocheckbox").find("input[type='checkbox'][id='" + id + "'").prop("checked", !isChecked);
    e.preventDefault();
});
$('#filterAllModal .SubmitPageFilter .radiocheckbox').on('change', function (e) {

    var isChecked = $(this).find("input[type='checkbox']").prop("checked");
    var id = $(this).find("input[type='checkbox']").prop("id");

    $("#filterAllModal .SubmitPageFilter").find(".radiocheckbox").find("input[type='checkbox'][id='" + id + "'").prop("checked", isChecked);
    $("#allfilters .SubmitPageFilter").find(".radiocheckbox").find("input[type='checkbox'][id='" + id + "'").prop("checked", isChecked);
    //e.preventDefault();
});


$('#allfilters .SubmitPageFilter .radiocheckbox').on('change', function (e) {
    if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) == false) {
        var isChecked = $(this).find("input[type='checkbox']").prop("checked");
        var id = $(this).find("input[type='checkbox']").prop("id");
        e.preventDefault();
        $("#filterAllModal .SubmitPageFilter").find(".radiocheckbox").find("input[type='checkbox'][id='" + id + "'").prop("checked", isChecked);
        $("#allfilters .SubmitPageFilter").find(".radiocheckbox").find("input[type='checkbox'][id='" + id + "'").prop("checked", isChecked);
        $("#SearchLoader .modal-body h2").text("Just one second whilst we find the best experiences for you!");
        $("#SearchLoader .modal-body h2").css("font-size", "1.3em");
        $('#SearchLoader').modal('show');
        $(".showfiltertarget").click();
    }
});

//offerfilter
$('#filterAllModal .SubmitOfferFilter .radiocheckbox label').on('click', function (e) {

    var isChecked = $(this).find("input[type='checkbox']").prop("checked");
    var id = $(this).find("input[type='checkbox']").prop("id");

    $("#filterAllModal .SubmitOfferFilter").find(".radiocheckbox").find("input[type='checkbox'][id='" + id + "'").prop("checked", !isChecked);
    $("#allfilters .SubmitOfferFilter").find(".radiocheckbox").find("input[type='checkbox'][id='" + id + "'").prop("checked", !isChecked);
    e.preventDefault();
});

$('#filterAllModal .SubmitOfferFilter .radiocheckbox').on('change', function (e) {

    var isChecked = $(this).find("input[type='checkbox']").prop("checked");
    var id = $(this).find("input[type='checkbox']").prop("id");

    $("#filterAllModal .SubmitOfferFilter").find(".radiocheckbox").find("input[type='checkbox'][id='" + id + "'").prop("checked", !isChecked);
    $("#allfilters .SubmitOfferFilter").find(".radiocheckbox").find("input[type='checkbox'][id='" + id + "'").prop("checked", !isChecked);
    e.preventDefault();
});

$('#allfilters .SubmitOfferFilter .radiocheckbox').on('change', function () {
    if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) == false) {
        var isChecked = $(this).find("input[type='checkbox']").prop("checked");
        var id = $(this).find("input[type='checkbox']").prop("id");

        $("#filterAllModal .SubmitOfferFilter").find(".radiocheckbox").find("input[type='checkbox'][id='" + id + "'").prop("checked", isChecked);
        $("#allfilters .SubmitOfferFilter").find(".radiocheckbox").find("input[type='checkbox'][id='" + id + "'").prop("checked", isChecked);
        $("#SearchLoader .modal-body h2").text("Just one second whilst we find the best experiences for you!");
        $("#SearchLoader .modal-body h2").css("font-size", "1.3em");
        $('#SearchLoader').modal('show');
        $(".showfiltertarget").click();
    }
});

// Offer Clear Filter

if ($(".OfferClearAll").length > 0) {
    var offerClearAll = $(".OfferClearAll").detach();
    $(".form-group .SubmitOfferFilter").eq(0)
        .find(".appearance-full .checkbox.SubmitOfferFilter.filter-selected.list-group")
        .before(offerClearAll);

    if ($(".OfferClearAll")[0] != undefined) {
        var clearAllButton = $('<button>', {

            class: 'btnClearAll', // optional classes
            click: function () {
                $('#SearchLoader').modal('show');
                $(this).hide();
                var id = $(this).find("input[type='checkbox']").prop("id");
                $("#filterAllModal .SubmitOfferFilter").find(".radiocheckbox").find("input[type='checkbox'][id='" + id + "'").prop("checked", false);
                $("#allfilters .SubmitOfferFilter").find(".radiocheckbox").find("input[type='checkbox'][id='" + id + "'").prop
                $('#ContentFilter').submit();
            }
        });
        clearAllButton.html('<i class="fa fa-times"></i> Clear Filter')
            .css({
                'margin-bottom': '15px',
                'width': '100%'
            });

        $("#allfilters .OfferClearAll")[0].append(clearAllButton[0]);
    }
}


//Price band filter
$('#filterAllModal .PriceBandfilter  .radiocheckbox label').on('click', function (e) {

    var isChecked = $(this).find("input[type='checkbox']").prop("checked");
    var id = $(this).find("input[type='checkbox']").prop("id");

    $("#filterAllModal .PriceBandfilter ").find(".radiocheckbox").find("input[type='checkbox'][id='" + id + "'").prop("checked", !isChecked);
    $("#allfilters .PriceBandfilter ").find(".radiocheckbox").find("input[type='checkbox'][id='" + id + "'").prop("checked", !isChecked);
    e.preventDefault();
});

$('#filterAllModal .PriceBandfilter  .radiocheckbox').on('click', function (e) {

    var isChecked = $(this).find("input[type='checkbox']").prop("checked");
    var id = $(this).find("input[type='checkbox']").prop("id");

    $("#filterAllModal .PriceBandfilter ").find(".radiocheckbox").find("input[type='checkbox'][id='" + id + "'").prop("checked", !isChecked);
    $("#allfilters .PriceBandfilter ").find(".radiocheckbox").find("input[type='checkbox'][id='" + id + "'").prop("checked", !isChecked);
    e.preventDefault();
});

$('#allfilters .PriceBandfilter .radiocheckbox').on('change', function () {
    if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) == false) {
        var isChecked = $(this).find("input[type='checkbox']").prop("checked");
        var id = $(this).find("input[type='checkbox']").prop("id");

        $("#filterAllModal .PriceBandfilter ").find(".radiocheckbox").find("input[type='checkbox'][id='" + id + "'").prop("checked", isChecked);
        $("#allfilters .PriceBandfilter ").find(".radiocheckbox").find("input[type='checkbox'][id='" + id + "'").prop("checked", isChecked);
        $("#SearchLoader .modal-body h2").text("Just one second whilst we find the best experiences for you!");
        $("#SearchLoader .modal-body h2").css("font-size", "1.3em");
        $('#SearchLoader').modal('show');
        $(".showfiltertarget").click();
    }
});




$('button.filter-applied').on('click', function () {
    
    $("#filterAllModal").modal("hide");
    $('#SearchLoader').modal('show');
   
    var removeClass = $(this).attr("class").split(" ").find(c => c.startsWith("remove-"));
    var filterName = removeClass.replace("remove-", "");
    
    if (filterName == 'GroupSizeFilter') {
        $("#GroupSizeFrom").val(null);
        $("#GroupSizeTo").val(null);
        $(".mingroupsize").val(null);
        $(".maxgroupsize").val(null);
    } else if (filterName == 'PriceFilter') {
        $("#MinPrice").val(null);
        $("#MaxPrice").val(null);
        $('#ContentFilter').submit();
    }
    else if (filterName == 'LocationFilter') {
        $("#Location").val(null);
        $("#hidDistance").val(null);
       // $(".SubmitOfferFilter").find(".radiocheckbox").find("input[type='checkbox']").prop("checked", false);
        $('#ContentFilter').submit();
    } else if (filterName == 'AgeFilter') {
        $("#MinAge").val(null);
        $("#MaxAge").val(null);
        $(".minage").val(null);
        $(".maxage").val(null);
        $('#ContentFilter').submit();
    }
    else if (filterName == 'WeightFilter') {
        $("#From").val(null);
        $("#To").val(null);
        $("#sliderForm").val(null);
        $("#sliderTo").val(null);
        $('#ContentFilter').submit();
    } 
    else if (filterName == 'OfferFilter') {
        $('#SearchLoader').modal('show');
        $(this).hide();
        $(".SubmitOfferFilter").find(".radiocheckbox").find("input[type='checkbox']").prop("checked", false);
        $('#ContentFilter').submit();
    } 
    else if (filterName == 'PageFilter') {
        $('#SearchLoader').modal('show');
        var name = $(this).attr("name");
        $(this).hide();
        $(".SubmitPageFilter").find(".radiocheckbox").find("input[type='checkbox'][id='" + name +"'").prop("checked", false);
        $('#ContentFilter').submit();
    } 
    else if (filterName == 'PriceBandFilter') {
        $('#SearchLoader').modal('show');
        var name = $(this).attr("name");
        $(this).hide();
        $(".SubmitPriceBandFilter").find(".radiocheckbox").find("input[type='checkbox'][id='" + name + "'").prop("checked", false);
        $('#ContentFilter').submit();
    } 

    //$("#" + this.name).prop("checked", false);
    ////document.getElementById("ContentFilter").submit();
    //$('#ContentFilter').submit();
});




$('.filter-xs-btn').click(function () {
    $(this).siblings('#ContentFilter').fadeIn('fast');
});
$('.btnAllFiltersMB').click(function () {
   
    var formActions = $(".filter-main .form-actions").last().detach();
    formActions.removeClass("form-actions").addClass("modal-Popup-actions");
    var btn1 = formActions.find("button.showfiltertarget");
    var btn2 = formActions.find("button.btnRatingSubmit");
    if (btn1.length > 0) {
        btn1.addClass("hidden"); // hide it
    }
    if (btn2.length > 0) {
        btn2.addClass("hidden"); // hide it
    }
    $(".filter-main").prepend(formActions);
    if ($('input[name="OfferFilter"]:checked').length > 0) {
        
        var lastFormActions = $(".offerfilter .form-actions button").last().detach();
        lastFormActions.removeClass("hidden");
        formActions.append(lastFormActions);
        $(".offerfilter .form-actions").hide();
    }
     $("#ContentFilter").fadeIn("fast");
    $('.btnAllFiltersMB').hide();
});
$('#ContentFilter .fa-times').click(function () {
    $('#ContentFilter').fadeOut('fast');
    $('.btnAllFiltersMB').show();
});
$('.filter  > .form-group > label').hover(function () {

    // $('#ContentFilter  > fieldset > .form-group > .control-wrapper').hide();

    $('label').not(this).siblings('.control-wrapper').hide();

    $('label').not(this).removeClass('active');

});

$('#ContentFilter fieldset .form-group .control-wrapper').click(function (event) {
    //if ($(event.target).is('#useMyLocation, #useMyLocation *')) {
    //    return; // don’t block click on the span or its children (icon)
    //}
    event.stopPropagation();

});
window.addEventListener("scroll", function () {
    const div = document.querySelector(".movableDiv");
    const offsetTop = div.offsetTop;

    if (window.scrollY > offsetTop) {
        div.classList.add("fixedDiv");
    } else {
        div.classList.remove("fixedDiv");
    }
});

(function ($) {
    $(function () {
        // Config
        var BREAKPOINT = 992;    // BS3 md breakpoint (enable bottom-sheet only below this)
        var THRESHOLD = 120;     // px required to dismiss (tweak)
        var RELEASE_TRANS = 'transform 220ms cubic-bezier(.2,.8,.2,1)';
        var MAX_DRAG_RATIO = 0.9; // max drag relative to viewport height

        // Select the modals you want to behave as bottom-sheet
        var $modals = $('.modal.bottom-sheet');

        if (!$modals.length) return;

        // For each modal, maintain its own state & handlers
        $modals.each(function () {
            var $modal = $(this);
            var $dialog = $modal.find('.modal-dialog');
            var $handle = $modal.find('.handle');

            if (!$dialog.length || !$handle.length) return;

            var dragging = false;
            var startY = 0;
            var currentY = 0;
            var rafId = null;
            var bound = false;

            function inMobileBreakpoint() {
                return window.innerWidth < BREAKPOINT;
            }

            function applyTransform(y) {
                if (y < 0) y = 0;
                var maxDrag = window.innerHeight * MAX_DRAG_RATIO;
                if (y > maxDrag) y = maxDrag;
                $dialog.css('transform', 'translate3d(0,' + Math.round(y) + 'px,0)');
            }

            function startDrag(clientY, ev) {
                dragging = true;
                startY = clientY;
                currentY = clientY;
                // immediate response while dragging
                $dialog.css('transition', 'none');
                $('body').addClass('bs-bottomsheet-dragging');
                if (ev && ev.preventDefault) ev.preventDefault();
            }

            function moveDrag(clientY, ev) {
                if (!dragging) return;
                currentY = clientY;
                var delta = currentY - startY;
                if (delta < 0) delta = 0;
                if (rafId) cancelAnimationFrame(rafId);
                rafId = requestAnimationFrame(function () {
                    applyTransform(delta);
                });
                if (ev && ev.preventDefault) ev.preventDefault();
            }

            function animateHideAndThenBootstrapHide() {
                // Animate off-screen
                $dialog.css({
                    transition: RELEASE_TRANS,
                    transform: 'translate3d(0,100vh,0)'
                });

                // Ensure we only hide once
                var called = false;
                function finish() {
                    if (called) return;
                    called = true;
                    // call BS3 plugin hide (uses jQuery plugin)
                    $modal.modal('hide');
                    // clear inline styles shortly after hide to keep reopen clean
                    setTimeout(function () {
                        $dialog.css({ transform: '', transition: '' });
                    }, 20);
                }

                // Wait for transitionend (with safety fallback)
                $dialog.one('transitionend webkitTransitionEnd oTransitionEnd MSTransitionEnd', finish);
                setTimeout(finish, 400); // fallback in case transitionend doesn't fire
            }

            function endDrag() {
                if (!dragging) return;
                dragging = false;
                var delta = currentY - startY;
                $('body').removeClass('bs-bottomsheet-dragging');

                if (delta >= THRESHOLD) {
                    animateHideAndThenBootstrapHide();
                } else {
                    // snap back
                    $dialog.css({
                        transition: RELEASE_TRANS,
                        transform: 'translate3d(0,0,0)'
                    });
                    setTimeout(function () { $dialog.css('transition', ''); }, 260);
                }
            }

            // Pointer start handler (unified)
            function pointerStartHandler(e) {
                var clientY;

                // Touch events carry touches in originalEvent
                if (e.type === 'touchstart') {
                    clientY = e.originalEvent.touches && e.originalEvent.touches[0] && e.originalEvent.touches[0].clientY;
                } else {
                    clientY = e.clientY || (e.originalEvent && e.originalEvent.clientY);
                }
                if (typeof clientY === 'undefined') return;

                startDrag(clientY, e);

                // bind move/up on document (namespaced)
                $(document).on('mousemove.bsBottomSheet touchmove.bsBottomSheet pointermove.bsBottomSheet', function (ev) {
                    var cy;
                    if (ev.type.indexOf('touch') === 0) {
                        cy = ev.originalEvent.touches && ev.originalEvent.touches[0] && ev.originalEvent.touches[0].clientY;
                    } else {
                        cy = ev.clientY || (ev.originalEvent && ev.originalEvent.clientY);
                    }
                    if (typeof cy !== 'undefined') moveDrag(cy, ev);
                });

                $(document).on('mouseup.bsBottomSheet touchend.bsBottomSheet pointerup.bsBottomSheet pointercancel.bsBottomSheet touchcancel.bsBottomSheet', function () {
                    $(document).off('.bsBottomSheet');
                    endDrag();
                });

                if (e.preventDefault) e.preventDefault();
            }

            // Bind handlers (only when in mobile breakpoint)
            function bindIfNeeded() {
                if (bound) return;
                if (!inMobileBreakpoint()) return;
                bound = true;

                // Attach start events appropriate to environment
                if (window.PointerEvent) {
                    $handle.on('pointerdown.bsBottomSheet', pointerStartHandler);
                } else {
                    $handle.on('touchstart.bsBottomSheet mousedown.bsBottomSheet', pointerStartHandler);
                }

                // Ensure we clear transforms if the modal is hidden by other means
                $modal.on('hidden.bs.modal.bsBottomSheet', function () {
                    $dialog.css({ transform: '', transition: '' });
                    $('body').removeClass('bs-bottomsheet-dragging');
                });

                // If the modal is shown while bound, ensure dialog starts at 0 transform
                $modal.on('show.bs.modal.bsBottomSheet', function () {
                    $dialog.css({ transform: 'translate3d(0,0,0)' });
                });
            }

            function unbindIfNeeded() {
                if (!bound) return;
                bound = false;
                $handle.off('.bsBottomSheet');
                $modal.off('.bsBottomSheet');
                $(document).off('.bsBottomSheet');
                // remove any dragging locks / inline styles
                $dialog.css({ transform: '', transition: '' });
                $('body').removeClass('bs-bottomsheet-dragging');
            }

            // On init, bind only if currently below breakpoint
            function checkAndToggleBinding() {
                if (inMobileBreakpoint()) {
                    bindIfNeeded();
                } else {
                    unbindIfNeeded();
                }
            }

            // Initial check
            checkAndToggleBinding();

            // Re-check on resize (debounced)
            var resizeTimeout = null;
            $(window).on('resize.bsBottomSheet', function () {
                if (resizeTimeout) clearTimeout(resizeTimeout);
                resizeTimeout = setTimeout(function () {
                    checkAndToggleBinding();
                }, 120);
            });

            // Also ensure that if modal is currently open and we resize above breakpoint, we clear transforms
            $modal.on('shown.bs.modal', function () {
                if (!inMobileBreakpoint()) {
                    // clean any transform/transition so modal behaves normally on larger screens
                    $dialog.css({ transform: '', transition: '' });
                }
            });
        }); // end each modal
    });
})(jQuery);

$('#filterAllModal').on('show.bs.modal', function (e) {
    var $modal = $(this);
    var fromBtn = $(e.relatedTarget || null);
    var section =
        $modal.data('section') ||                       // programmatic path
        (fromBtn.length ? fromBtn.data('section') : '') // data-API path
        || 'all';

    var $sections = $modal.find('.modal-section').addClass('visually-hidden');
    if (section === 'all') {
        $sections.removeClass('visually-hidden');
        $modal.find('.modal-title').text('All Filters');
    } else {
        $modal.find('#' + section).removeClass('visually-hidden');
        $modal.find('.modal-title').text(section.replace(/^section/, 'Section '));
    }
});
$('#filterAllModal').on('hidden.bs.modal', function () {
    $(this).removeData('section');
});

if ($("#allfilters .offerfilter div:first").length > 0) {
    if ($("#allfilters .offerfilter div:first").hasClass("hidden")) {
        $("#allfilters .offerfilter").addClass("hidden");
    }
}
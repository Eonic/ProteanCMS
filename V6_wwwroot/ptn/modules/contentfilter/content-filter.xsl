<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
	<xsl:template match="Content[@type='Module' and @contentType='ContentFilter']" mode="displayBrief">
		<xsl:choose>
			<xsl:when test="Content[@type='xform']">
				<xsl:apply-templates select="Content[@type='xform']" mode="xform"/>
			</xsl:when>
		</xsl:choose>
		<style>
			#histogramSliderPrice{
			max-width: 250px;
			margin: auto;
			}
		</style>

		<style>
			#histogramSliderAge{
			max-width: 250px;
			margin: auto;
			}

		</style>

	</xsl:template>

	<xsl:template match="Content[@type='Module' and @contentType='ContentFilter']" mode="contentJS">
		<script type="text/javascript" src="/ewthemes/intotheblue2019/js/productlist.js?v=48831">
			<xsl:text> </xsl:text>
		</script>
		<link rel="stylesheet" href="/ewthemes/Intotheblue2019/js/WrunnerRangeSlider/css/wrunner-default-theme.css" />
		<script src="/ewthemes/Intotheblue2019/js/WrunnerRangeSlider/js/wrunner-native.js" >
			<xsl:text> </xsl:text>
		</script>
		<script src="/ewthemes/Intotheblue2019/js/WrunnerRangeSlider/js/wrunner-jquery.js" >
			<xsl:text> </xsl:text>
		</script>
		<script type="text/javascript">

			$(document).ready(function () {

			$("#Location").attr("placeholder", "Town or Postcode");

			function weightConverterKgToStone(valNum, lblId) {
			return valNum * 0.1574;
			}




			//Group Size filter

			var GroupsliderFrom = '';
			var GroupsliderTo = '';
			GroupsliderFrom = $("#GroupSizeFrom").val();
			GroupsliderTo = $("#sliderGroupSizeTo").val();
			<xsl:text disable-output-escaping="yes">
			if (GroupsliderFrom == '' &amp;&amp; GroupsliderTo == '') {
			if ($(".btnCrossForGroupSize").length > 0) {
			var name = $(".btnCrossForGroupSize").val();
			var values = name.split(',');
			GroupsliderFrom = values[0];
			GroupsliderTo = values[1];

			$("#GroupFrom").val(GroupsliderFrom);
			$("#GroupTo").val(GroupsliderTo);
$("#GroupSizeFrom").val(GroupsliderFrom);
			$("#GroupSizeTo").val(GroupsliderTo);
			$("#sliderGroupSizeFrom").val(GroupsliderFrom);
			$("#sliderGroupSizeTo").val(GroupsliderTo);
			}</xsl:text>
			else {
			GroupsliderFrom = '<xsl:value-of select="$page/Contents/Content[@contentType='ContentFilter']/Content[@filterType='GroupSizeFilter']/@fromGroupSize"/>';
			GroupsliderTo = '<xsl:value-of select="$page/Contents/Content[@contentType='ContentFilter']/Content[@filterType='GroupSizeFilter']/@toGroupSize"/>';

			}
			}
			if ($(".GroupSizeSliderMainDiv") != undefined) {

			$(".GroupSizeSlider").detach().appendTo(".GroupSizeSliderMainDiv div");
			$(".GroupSizeSliderMainDiv input").hide();
			$(".GroupSizeSliderMainDiv div").css('width','100%');
			if( /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)==false ) {
			$(".GroupSizeSliderMainDiv .input-wrapper").css('width','140%');
			}
			//$(".GroupSizeSliderMainDiv .input-wrapper .GroupSizeSlider ").css('margin-bottom','20px');
			if($(".GroupSizeSliderMainDiv div")[0]!=undefined){
			$(".GroupSizeSliderMainDiv div")[0].append($(".form-actions .btnGroupSizeSub")[0]);
			$(".btnGroupSizeSub").removeClass("hidden");
			}

			var sGroupSizeFrom = '';
			var sGroupSizeTo = '';

			if($("#GroupSizeFrom").val()!='')
			{
			sGroupSizeFrom = $("#GroupSizeFrom").val();
			}

			if($("#GroupSizeTo").val()!='')
			{
			sGroupSizeTo = $("#GroupSizeTo").val();
			}
			//alert(sGroupSizeFrom);
			if(sGroupSizeFrom=='')
			{
			sGroupSizeFrom=GroupsliderFrom;
			}
			if(sGroupSizeTo=='')
			{
			sGroupSizeTo=GroupsliderTo;
			}

			var wRunnerGroupSize = $('.GroupSizeSlider').wRunner({
			type: 'range',
			rangeValue: {
			minValue: sGroupSizeFrom,
			maxValue: sGroupSizeTo,
			},
			limits: {
			minLimit: 1,
			maxLimit:GroupsliderTo,
			},
			valueNoteDisplay:false,
			onValueUpdate:function(values){
			$("#lblGroupSize").text("" + values.minValue + " - " + values.maxValue + " ");
			}
			})

			$('.GroupSizeSlider .wrunner').append('<label id="lblGroupSize" style="text-align:center;">&#160;</label>');
			var groupSizeValue = wRunnerGroupSize.getValue();

			$("#lblGroupSize").text("" + groupSizeValue.minValue + " - " + groupSizeValue.maxValue + " ");



			}
			$(".btnGroupSizeSub").on('click', function () {

			$('#SearchLoader').modal('show');
			var sGroupSizeFrom = $('.GroupSizeSlider .wrunner__valueNote')[0].innerText;
			var sGroupSizeTo = $('.GroupSizeSlider .wrunner__valueNote')[1].innerText;
			// $("#ContentFilter").submit();
			$("#GroupSizeFrom").val(sGroupSizeFrom);
			$("#GroupSizeTo").val(sGroupSizeTo);
			$("#sliderGroupSizeFrom").val(sGroupSizeFrom);
			$("#sliderGroupSizeTo").val(sGroupSizeTo);


			});


			// Weight filter
			var sliderFrom = '';
			var sliderTo = '';
			sliderFrom = $("#sliderForm").val();
			sliderTo = $("#sliderTo").val();

			<xsl:text disable-output-escaping="yes">
			if (sliderFrom == ''  &amp;&amp; sliderTo == '') {
			if ($(".btnCrossForWeight").length > 0) {

			var name = $(".btnCrossForWeight").val();
			var values = name.split(',');
			sliderFrom = values[0];
			sliderTo = values[1];
			$("#From").val(sliderFrom);
			$("#To").val(sliderTo);
			$("#sliderForm").val(sliderFrom);
			$("#sliderTo").val(sliderTo);
			}</xsl:text>
			else {
			sliderFrom = '<xsl:value-of select="$page/Contents/Content[@contentType='ContentFilter']/Content[@filterType='WeightFilter']/@fromWeight"/>';
			sliderTo = '<xsl:value-of select="$page/Contents/Content[@contentType='ContentFilter']/Content[@filterType='WeightFilter']/@toWeight"/>';
			}
			}


			if ($(".WeightSliderMainDiv") != undefined) {

			$(".WeightSlider").detach().appendTo(".WeightSliderMainDiv div");
			$(".WeightSliderMainDiv input").hide();
			if($(".WeightSliderMainDiv")[0]!=undefined)
			{
			$(".WeightSliderMainDiv div")[0].append($(".form-actions .btnWeightSub")[0]);
			$(".btnWeightSub").removeClass("hidden");
			}




			var wRunnerWeight = $('.WeightSlider').wRunner({
			type: 'range',
			rangeValue: {
			minValue: sliderFrom,
			maxValue: sliderTo,
			},
			limits: {
			minLimit: 0,
			maxLimit: 250,
			},
			valueNoteDisplay:false,
			onValueUpdate:function(values){
			var minValueInStone = weightConverterKgToStone(values.minValue);
			var maxValueInStone = weightConverterKgToStone(values.maxValue);
			$("#lblInStone").text(" or from " + minValueInStone.toFixed(2) + " - " + maxValueInStone.toFixed(2) + " stone");
			$("#lblInKg").text("Weight from " + values.minValue + " - " + values.maxValue + " kg");
			}
			})


			$('.btnWeightSub').click(function () {
			$('#SearchLoader').modal('show');
			var sWeightFrom = $('.WeightSlider .wrunner__valueNote')[0].innerText;
			var sWeightTo = $('.WeightSlider .wrunner__valueNote')[1].innerText;
			var minValue = sWeightFrom;
			var minValueInStone = weightConverterKgToStone(sWeightFrom);
			var maxValue = sWeightTo;
			var maxValueInStone = weightConverterKgToStone(sWeightTo);
			$("#FromInStone").val(minValueInStone);
			$("#ToInStone").val(maxValueInStone);
			if($("#lblInStone").val()!='')
			{
			$("#lblInStone").val();
			}
			if($("#lblInKg").val()!='')
			{
			$("#lblInKg").val();
			}
			$("#lblInStone").text(" or from " + minValueInStone.toFixed(2) + " - " + maxValueInStone.toFixed(2) + " stone");
			$("#lblInKg").text("Weight from " + minValue + " - " + maxValue + " kg");
			$("#From").val(sWeightFrom);
			$("#To").val(sWeightTo);
			$("#sliderForm").val(sWeightFrom);
			$("#sliderTo").val(sWeightTo);
			$("#ContentFilter").submit();
			if (sWeightFrom != sWeightTo) {
			// $(".showexperiences").click();

			}
			});

			$('.WeightSlider .wrunner').append('<label id="lblInKg">&#160;</label>');
			$('.WeightSlider .wrunner').append('<br/>');
			$('.WeightSlider .wrunner').append('<label id="lblInStone">&#160;</label>');

			var weightValue = wRunnerWeight.getValue();

			//var minValue = $(".wrunner__valueNote ")[0].innerText;
			var minValue = weightValue.minValue;
			var minValueInStone = weightConverterKgToStone(minValue);
			//var maxValue = $(".wrunner__valueNote ")[1].innerText;
			var maxValue = weightValue.maxValue;
			var maxValueInStone = weightConverterKgToStone(maxValue);
			$("#lblInKg").text("Weight from " + minValue + " - " + maxValue + " kg");
			$("#lblInStone").text(" or from " + minValueInStone.toFixed(2) + " - " + maxValueInStone.toFixed(2) + " stone");

			}
			});


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
			});

			$('.locationCrossBtn').on('mousedown', function (event) {
			event.preventDefault();
			$("#Location").val(null);
			$("#hidDistance").val(null);
			$('#SearchLoader').modal('show');
			$(this).hide();
			$('#ContentFilter').submit();
			});


			$('.btnCrossForWeight').on("mousedown", function (event) {
			event.preventDefault();
			$('#SearchLoader').modal('show');

			$("#From").val(null);
			$("#To").val(null);
			$(this).hide();
			$('#ContentFilter').submit();
			});
		</script>
		<script src="https://code.jquery.com/ui/1.12.1/jquery-ui.js" integrity="sha256-T0Vest3yCU7pafRw9r+settMBX6JkKN06dqBnpQ8d30=" crossorigin="anonymous">
			<xsl:text> </xsl:text>
		</script>
		<!--<link rel="stylesheet" type="text/css" href="https://code.jquery.com/ui/1.12.1/themes/base/jquery-ui.css">
      <xsl:text> </xsl:text>
    </link>-->
		<link rel="stylesheet" type="text/css" href="/ewthemes/IntoTheBlue2019/js/HistogramSlider/css/histogram.slider.css">
			<xsl:text> </xsl:text>
		</link>
		<script src="https://cdnjs.cloudflare.com/ajax/libs/jqueryui-touch-punch/0.2.3/jquery.ui.touch-punch.min.js">
			<xsl:text> </xsl:text>
		</script>
		<script src="/ewthemes/IntoTheBlue2019/js/HistogramSlider/js/histogram.slider.js">
			<xsl:text> </xsl:text>
		</script>
		<script type="text/javascript">

			<xsl:text disable-output-escaping="yes">
      $(document).ready(function () {


      if ($(".locationCrossBtn").length &gt; 0) {

      var value = $(".locationCrossBtn").val();
      if (value.indexOf(',') &gt; -1)
      {
      var arry = value.split(',');

      $("#Location").val(arry[0]);
      var distance = arry[1].split(' ');
      $("#hidDistance").val(distance[1]);
      }
      }
      if ($(".btnCrossForAge").length &gt; 0) {
      var value = $(".btnCrossForAge").val();
      var arry = value.split(',');

      $("#MinAge").val(arry[0]);
      var ages = arry[1].split(' ');
      $("#MaxAge").val(arry[1]);
      }</xsl:text>


			<!--// histogram parameters-->
			var histogramType;
			var mainDiv;
			var mainClass;
			var productListCount;
			var products;
			var step;

			if ($(".postCode")[0] != undefined) {
			$(".distance .appearance-full")[0].prepend($(".postCode")[1]);
			$(".distance .appearance-full")[0].prepend($(".postCode")[0]);
			$(".distance .appearance-full")[0].append($(".form-actions .btnLocationSub")[0]);
			$(".btnLocationSub").removeClass("hidden");
			}

			function DisplayHistogram(mainDiv, mainClass, histogramType) {

			mainDiv = mainDiv + histogramType;
			mainClass = mainClass + histogramType;

			if ($("." + mainDiv)[0] != undefined) {
			var minSlideRange = 0;
			var sliderMin="";
			var sliderMax="";
			sliderMin = $("#SliderMin" + histogramType)[0].value;
			sliderMax = $("#SliderMax" + histogramType)[0].value;

			if(sliderMax=='')
			{
			sliderMin = '<xsl:value-of select="$page/Contents/Content/Content/model/instance/PriceFilter/@SliderMinPrice"/>';
			sliderMax = '<xsl:value-of select="$page/Contents/Content/Content/model/instance/PriceFilter/@SliderMaxPrice"/>';

			}

			var maxLimit=$("#Max" + histogramType+"Limit")[0].value;
			var min = $("#Min" + histogramType)[0].value;
			var max = $("#Max" + histogramType)[0].value;

			if (max == "") {
			max = parseInt(sliderMax);
			}
			else
			{
			if (max.indexOf('£') != -1)
			{
			max=max.replace('£','');
			}
			if (max.indexOf('+') != -1)
			{
			max=max.replace('+','');
			}
			}

			if (min == "") {
			min = parseInt(sliderMin);

			}
			else
			{
			if (min.indexOf('£') != -1) {

			min=min.replace('£','');
			}}

			step = $("#" + histogramType + "Step")[0].value;
			productListCount = $("#" + histogramType + "ListCount")[0].value;
			products = productListCount.split(",");

			<!--var maxSlideRange = products.length * step; -->
			var maxSlideRange = sliderMax;

			var numBins = products.length;
			<!--$("#" + mainClass + " .selected-range")[0].text(""sliderMin + sliderMax); -->
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
			customText:'£',
			maxLimit:maxLimit
			});

			if($("."+ mainDiv)[0]!=undefined)
			{
			$("."+ mainDiv +" div")[0].append($(".form-actions .btn"+histogramType+"Submit")[0]);
			$(".btn"+ histogramType+ "Submit").removeClass("hidden");
			}

			$(".btn"+histogramType+"Submit").click(function () {
			$('#SearchLoader').modal('show');
			if ($("#" + mainClass + " .selected-range")[0] != undefined) {
			var range = $("#" + mainClass + " .selected-range")[0].innerText.split("-");

			$('#Min' + histogramType).val(range[0]);


			$('#Max' + histogramType).val(range[1]);


			//document.ContentFilter.submit();
			//$(".showexperiences").click();
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

			function DisplayHistogramAge(mainDivAge, mainClassAge, histogramTypeAge) {

			mainDiv = mainDivAge + histogramTypeAge;
			mainClass = mainClassAge + histogramTypeAge;

			if ($("." + mainDiv)[0] != undefined) {
			var minSlideRange = 0;

			var sliderMin = '<xsl:value-of select="$page/Contents/Content[@contentType='ContentFilter']/Content[@filterType='AgeFilter']/@fromAge"/>';
			var sliderMax = '<xsl:value-of select="$page/Contents/Content[@contentType='ContentFilter']/Content[@filterType='AgeFilter']/@toAge"/>';

			var min = $("#Min" + histogramTypeAge)[0].value;
			var max = $("#Max" + histogramTypeAge)[0].value;
			if (max == "") {
			max = parseInt(sliderMax);
			}
			if (min == "") {
			min = parseInt(sliderMin);
			}
			step =parseInt('<xsl:value-of select="$page/Contents/Content[@contentType='ContentFilter']/Content[@filterType='AgeFilter']/@step"/>');

			productListCount = '<xsl:value-of select="$page/Contents/Content[@contentType='ContentFilter']/Content[@type='xform']/model/instance/AgeFilter/@AgeCountList"/>';
			if(productListCount=="")
			{
			productListCount= $("#AgeListCount").val();
			}
			products = productListCount.split(",");

			<!--var maxSlideRange = products.length * step;-->
			var maxSlideRange = parseInt(sliderMax);

			var numBins = parseInt(products.length);

			<!--$("#" + mainClass + " .selected-range")[0].text(""sliderMin + sliderMax); -->
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
			showValues: true,
			showSelectedRange: true,
			customText:''
			});

			if($("."+ mainDiv)[0]!=undefined)
			{
			$("."+ mainDiv +" div")[0].append($(".form-actions .btnAgeSubmit")[0]);
			$(".btnAgeSubmit").removeClass("hidden");
			}

			$(".btnAgeSubmit").click(function () {
			$('#SearchLoader').modal('show');
			if ($("#" + mainClass + " .selected-range")[0] != undefined) {
			var range = $("#" + mainClass + " .selected-range")[0].innerText.split("-");

			$('#Min' + histogramTypeAge).val(range[0]);
			$('#Max' + histogramTypeAge).val(range[1]);

			//$(".showexperiences").click();
			//document.ContentFilter.submit();
			}
			});

			$('.btnCrossFor' + histogramTypeAge).on('mousedown', function (event) {
			event.preventDefault();
			$('#SearchLoader').modal('show');
			$("#Min" + histogramTypeAge).val(null);
			$("#Max" + histogramTypeAge).val(null);
			$("#ContentFilter").submit();
			});

			}
			}

			function dataFactory(numberOfBins, group) {
			var data = { "items": [] };

			<xsl:text disable-output-escaping="yes">
        if (products.length != undefined) {
            for (var i = 0; i &lt; products.length - 1; i++) {
                var arrProduct = products[i].split(":");
				
					
				
               if (group) {
                   data.items.push({ "value": parseInt(arrProduct[0]) * step, "count": parseFloat(arrProduct[1])});
                 

                } else {
                    data.items.push({ "value": parseInt(arrProduct[0]) * step });
               }
           }

            return data;
        } </xsl:text >
			}

			mainDiv = 'histogramSliderMainDiv';
			mainClass = 'histogramSlider';
			histogramType = 'Price';

			DisplayHistogram(mainDiv, mainClass, histogramType);

			mainDivAge = 'histogramSliderMainDiv';
			mainClassAge = 'histogramSlider';
			histogramTypeAge = 'Age';
			DisplayHistogramAge(mainDivAge, mainClassAge, histogramTypeAge);

			});


		</script>

	</xsl:template>
	<!-- ## Layout Types are specified in the LayoutsManifest.XML file  ################################   -->
	<xsl:template match="Content[@name='ContentFilter']" mode="xform">
		<button class="btn btn-success hidden-sm hidden-md hidden-lg filter-xs-btn">
			<i class="fas fa-sliders-h">
				<xsl:text> </xsl:text>
			</i> Filter Experiences
		</button>
		<form method="{model/submission/@method}" action="" data-fv-framework="bootstrap"
			data-fv-icon-valid="fa fa-check"
			data-fv-icon-invalid="fa fa-times"
			data-fv-icon-validating="fa fa-refresh">
			<xsl:attribute name="class">
				<xsl:text>ewXform</xsl:text>
				<xsl:if test="model/submission/@class!=''">
					<xsl:text> </xsl:text>
					<xsl:value-of select="model/submission/@class"/>
				</xsl:if>
			</xsl:attribute>
			<xsl:if test="not(contains(model/submission/@action,'.asmx'))">
				<xsl:attribute name="action">
					<xsl:value-of select="model/submission/@action"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="model/submission/@id!=''">
				<xsl:attribute name="id">
					<xsl:value-of select="model/submission/@id"/>
				</xsl:attribute>
				<xsl:attribute name="name">
					<xsl:value-of select="model/submission/@id"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="model/submission/@event!=''">
				<xsl:attribute name="onsubmit">
					<xsl:value-of select="model/submission/@event"/>
				</xsl:attribute>
			</xsl:if>

			<xsl:if test="model/instance/@valid!=''">
				<xsl:attribute name="data-fv-valid">
					<xsl:value-of select="model/instance/@valid"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:if test="descendant::upload">
				<xsl:attribute name="enctype">multipart/form-data</xsl:attribute>
			</xsl:if>
			<div class="hidden-sm hidden-md hidden-lg filter-xs-heading">
				<h3>
					<!--<i class="fas fa-sliders-h"> </i>--> Filter Experiences
				</h3>
				<i class="fas fa-times">
					<xsl:text> </xsl:text>
				</i>
			</div>
			<!--<xsl:copy-of select="/" />-->
			<!--xsl:apply-templates select="self::Content" mode="tinyMCEinit"/-->

			<xsl:apply-templates select="group | repeat | input | secret | select | select1 | range | textarea | upload | hint | help | alert | div" mode="xform"/>

			<xsl:if test="count(submit) &gt; 0">
				<p class="buttons">
					<xsl:if test="descendant-or-self::*[contains(@class,'required')]">
						<span class="required">
							<span class="req">*</span>
							<xsl:text> </xsl:text>
							<xsl:call-template name="msg_required"/>
						</span>
					</xsl:if>
					<xsl:apply-templates select="submit" mode="xform"/>

				</p>
			</xsl:if>


			<div class="terminus">
				<xsl:text> </xsl:text>
			</div>

		</form>

	</xsl:template>
	<xsl:template match="submit[contains(@class,'clear-filters')]" mode="xform">
		<xsl:variable name="class">
			<xsl:text>btn</xsl:text>
			<xsl:if test="not(contains(@class,'btn-'))">
				<xsl:text> btn-success</xsl:text>
			</xsl:if>
			<xsl:if test="@class!=''">
				<xsl:text> </xsl:text>
				<xsl:value-of select="@class"/>
			</xsl:if>
		</xsl:variable>
		<xsl:variable name="name">
			<xsl:choose>
				<xsl:when test="@ref!=''">
					<xsl:value-of select="@ref"/>
				</xsl:when>
				<xsl:when test="@submission!=''">
					<xsl:value-of select="@submission"/>
				</xsl:when>
				<xsl:when test="@bind!=''">
					<xsl:value-of select="@bind"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>ewSubmit</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="icon">
			<xsl:choose>
				<xsl:when test="@icon!=''">
					<xsl:value-of select="@icon"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>fa-check</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="buttonValue">
			<xsl:choose>
				<xsl:when test="@value!=''">
					<xsl:value-of select="@value"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="label/node()"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<div class="panel panel-default no-results-panel">
			<h4>No results found</h4>
			<button type="submit" name="{$name}" value="{$buttonValue}" class="btnRemoveAllFilters btn btn-success"  onclick="disableButton(this);">
				<xsl:if test="@data-pleasewaitmessage != ''">
					<xsl:attribute name="data-pleasewaitmessage">
						<xsl:value-of select="@data-pleasewaitmessage"/>
					</xsl:attribute>
				</xsl:if>
				<xsl:if test="@data-pleasewaitdetail != ''">
					<xsl:attribute name="data-pleasewaitdetail">
						<xsl:value-of select="@data-pleasewaitdetail"/>
					</xsl:attribute>
				</xsl:if>
				<xsl:if test="not(contains($class,'icon-right'))">
					<i class="fa {$icon} fa-white">
						<xsl:text> </xsl:text>
					</i>

				</xsl:if>
				<!--<xsl:apply-templates select="label" mode="submitText"/>-->
				remove all filters
				<xsl:if test="contains($class,'icon-right')">
					<xsl:text> </xsl:text>
					<i class="fa {$icon} fa-white">
						<xsl:text> </xsl:text>
					</i>
				</xsl:if>
			</button>
		</div>


	</xsl:template>


	<xsl:template match="Page[Contents/Content/@moduleType='ContentFilter' and Contents/Content/@resultCount &gt; 0]" mode="mainLayout">

		<div class="container">
			<xsl:apply-templates select="Contents/Content[@moduleType='ContentFilter']/Content[@type='xform']" mode="xform"/>
			<div id="AppliedFilter">
				<xsl:text> </xsl:text>
			</div>
		</div>
		<br/>
		<div class="container">

			<xsl:variable name="contentType" select="@contentType" />
			<xsl:variable name="queryStringParam" select="concat('startPos',@id)"/>
			<xsl:variable name="startPos" select="number(concat('1',/Page/Request/QueryString/Item[@name=$queryStringParam]))"/>
			<xsl:variable name="nextCounter" select="number('1')" />


			<div id="mod" class="module nobox pos-column1 module-ProductList">
				<div class="terminus">
					<xsl:text> </xsl:text>
				</div>
				<div class="clearfix ProductList">
					<div class="cols mobile-1-col-content sm-content-2 md-content-3 cols4 content-cols-responsive" data-xscols="1" data-smcols="2" data-mdcols="3" data-slidestoshow="4" data-slidetoshow="20" data-slidetoscroll="1">
						<xsl:apply-templates select="Contents/Content[@moduleType='ContentFilter']/Content[@type='Product']" mode="FilterResult" />
					</div>
					<!--Add next button code here-->
					<div class="terminus">
						<xsl:text> </xsl:text>
					</div>
					<xsl:apply-templates select="/" mode="genericStepperNextButtonFilter">
						<xsl:with-param name="noPerPage" select="24"/>
						<xsl:with-param name="startPos" select="$startPos"/>
						<xsl:with-param name="totalCount" select="Contents/Content/@resultCount"/>
						<xsl:with-param name="filterTarget" select="Contents/Content/@filterTarget"/>
					</xsl:apply-templates>
				</div>
			</div>

		</div>

	</xsl:template>


	<xsl:template match="Contents/Content[@moduleType='ContentFilter']/Content[@type='Product']" mode="FilterResult">
		<xsl:variable name="parId">
			<xsl:choose>
				<xsl:when test="@parId &gt; 0">
					<xsl:value-of select="@parId"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="/Page/@id"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="parentURL">
			<xsl:apply-templates select="self::Content" mode="getHref">
				<xsl:with-param name="parId" select="$parId"/>
			</xsl:apply-templates>
		</xsl:variable>
		<xsl:variable name="src">
			<xsl:choose>
				<!--IF Thumbnail use that-->
				<xsl:when test="Images/img[@class='thumbnail']/@src!=''">
					<xsl:value-of select="Images/img[@class='thumbnail']/@src"/>
				</xsl:when>
				<!--IF Full Size use that-->
				<xsl:when test="Images/img[@class='detail']/@src!=''">
					<xsl:value-of select="Images/img[@class='detail']/@src"/>
				</xsl:when>
				<!--ELSE use display-->
				<xsl:otherwise>
					<xsl:value-of select="Images/img[@class='display']/@src"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>
		<xsl:variable name="carttn">
			<xsl:call-template name="resize-image">
				<xsl:with-param name="path" select="$src"/>
				<xsl:with-param name="max-width" select="50"/>
				<xsl:with-param name="max-height" select="50"/>
				<xsl:with-param name="file-prefix">
					<xsl:text>~th-50x50</xsl:text>
					<xsl:text>/~th-</xsl:text>
					<xsl:text>crop-</xsl:text>
				</xsl:with-param>
				<xsl:with-param name="file-suffix" select="''"/>
				<xsl:with-param name="quality" select="100"/>
				<xsl:with-param name="crop" select="true()" />
				<xsl:with-param name="no-stretch" select="true()" />
				<xsl:with-param name="forceResize" select="true()" />
			</xsl:call-template>
		</xsl:variable>

		<div class="listItem list-group-item product">
			<xsl:apply-templates select="." mode="inlinePopupOptions">
				<xsl:with-param name="class" select="'listItem list-group-item product'"/>
				<xsl:with-param name="sortBy" select="'Position'"/>
			</xsl:apply-templates>
			<xsl:if test="(self::Content/@status='0')">
				<div class="btn btn-primary btn-xs btn-bg-alert">Product Not Available For Sale</div>
			</xsl:if>


			<div class="lIinner">

				<a href="{$parentURL}">
					<xsl:attribute name="class">
						<xsl:text>url pb-wrapper</xsl:text>
						<xsl:if test="@distance and @distance!=''"> distance-wrapper</xsl:if>
					</xsl:attribute>
					<div class="pb-image-wrap">
						<xsl:if test="Images/img/@src!=''">
							<xsl:apply-templates select="." mode="displayThumbnail">
								<xsl:with-param name="crop" select="true()"/>
							</xsl:apply-templates>

						</xsl:if>
						<xsl:text> </xsl:text>
					</div>
					<div class="pb-bottom">
						<!--rating placeholder-->

						<xsl:if test="@ratingCount!='' and @ratingCount!='0'">
							<div class="ratings box-rating">
								<span class="stars">
									<xsl:if test="@ratingAvg&gt;=1">
										<i class="fa fa-star">
											<xsl:text> </xsl:text>
										</i>
									</xsl:if>
									<xsl:if test="@ratingAvg&gt;=1.5 and @ratingAvg&lt;2">
										<i class="fas fa-star-half">
											<xsl:text> </xsl:text>
										</i>
									</xsl:if>
									<xsl:if test="@ratingAvg&gt;=2">
										<i class="fa fa-star">
											<xsl:text> </xsl:text>
										</i>
									</xsl:if>
									<xsl:if test="@ratingAvg&gt;=2.5 and @ratingAvg&lt;3">
										<i class="fas fa-star-half">
											<xsl:text> </xsl:text>
										</i>
									</xsl:if>
									<xsl:if test="@ratingAvg&gt;=3">
										<i class="fa fa-star">
											<xsl:text> </xsl:text>
										</i>
									</xsl:if>
									<xsl:if test="@ratingAvg&gt;=3.5 and @ratingAvg&lt;4">
										<i class="fas fa-star-half">
											<xsl:text> </xsl:text>
										</i>
									</xsl:if>
									<xsl:if test="@ratingAvg&gt;=4">
										<i class="fa fa-star">
											<xsl:text> </xsl:text>
										</i>
									</xsl:if>
									<xsl:if test="@ratingAvg&gt;=4.5 and @ratingAvg&lt;5">
										<i class="fas fa-star-half">
											<xsl:text> </xsl:text>
										</i>
									</xsl:if>
									<xsl:if test="@ratingAvg&gt;=5">
										<i class="fa fa-star">
											<xsl:text> </xsl:text>
										</i>
									</xsl:if>
									<xsl:text> </xsl:text>
								</span>
								<span class="hidden">
									<xsl:value-of select="@ratingCount"/>
									<xsl:text> </xsl:text>
								</span>
								<span class="hidden">5</span>
								<span class="based-on">
									Based On
									<span>
										<xsl:value-of select="@ratingCount"/>
									</span><xsl:text> Review</xsl:text>
									<xsl:if test="@ratingCount!=1">
										<xsl:text>s</xsl:text>
									</xsl:if>
								</span>
							</div>
						</xsl:if>
						<div class="pb-blue">
							<xsl:choose>
								<xsl:when test="$page/@id='4272'">
									<h3 class="title">
										<xsl:variable name="title">
											<xsl:choose>
												<xsl:when test="SearchLinkText!=''">
													<xsl:value-of select="SearchLinkText/node()"/>
												</xsl:when>
												<xsl:otherwise>
													<xsl:apply-templates select="." mode="getDisplayName"/>
												</xsl:otherwise>
											</xsl:choose>
										</xsl:variable>
										<xsl:value-of select="$title"/>
									</h3>
								</xsl:when>
								<xsl:otherwise>
									<h2 class="title">
										<xsl:variable name="title">
											<xsl:choose>
												<xsl:when test="SearchLinkText!=''">
													<xsl:value-of select="SearchLinkText/node()"/>
												</xsl:when>
												<xsl:otherwise>
													<xsl:apply-templates select="." mode="getDisplayName"/>
												</xsl:otherwise>
											</xsl:choose>
										</xsl:variable>
										<xsl:value-of select="$title"/>
									</h2>
								</xsl:otherwise>
							</xsl:choose>
							<!--strapline placeholder-->
							<p class="sr-subhead hidden-xs">

								<xsl:call-template name="firstWords">
									<xsl:with-param name="value">
										<xsl:value-of select="ShortDescription"/>
									</xsl:with-param>
									<xsl:with-param name="count" select="'10'"/>
								</xsl:call-template>
								<xsl:text>...</xsl:text>

							</p>
							<!--location placeholder-->
							<p class="sr-loc">
								<i class="fa fa-map-marker icn">
									<xsl:text> </xsl:text>
								</i>
								<xsl:text> </xsl:text>
								<xsl:value-of select="LocationMap/@shortDescription"/>
							</p>
							<!--distance placeholder-->
							<xsl:if test="@distance and @distance!=''">
								<p class="sr-loc">
									<i class="fas fa-ruler">
										<xsl:text> </xsl:text>
									</i>
									<xsl:text> </xsl:text> <xsl:value-of select="@distance"/> miles away
								</p>
							</xsl:if>
						</div>
						<div class="pb-price">
							<xsl:if test="Sticker/@color!=''">
								<xsl:attribute name="class">
									<xsl:text>pb-price pb-</xsl:text>
									<xsl:value-of select="Sticker/@color"/>
								</xsl:attribute>
							</xsl:if>
							<!--price placeholder-->
							<div class="sr-explorMore">
								<p>
									<xsl:if test="Sticker/node()!=''">
										<xsl:value-of select="Sticker/node()"/>
										<xsl:text> </xsl:text>
									</xsl:if>
									<xsl:choose>
										<xsl:when test="count(Content[@type='SKU'])&gt;1">
											<span class="textGreen">From </span>
										</xsl:when>
										<xsl:otherwise>
											<span class="textGreen">Buy For </span>
										</xsl:otherwise>
									</xsl:choose>
									<xsl:choose>
										<xsl:when test="Content[@type='SKU']">
											<xsl:variable name="lowestSKUPrice">
												<xsl:for-each select="Content[(@type='SKU' and @status='1')]">
													<xsl:sort select="Prices/Price[(@currency=$currency and @type='sale')]/node()" data-type="number" order="ascending"/>
													<xsl:if test="position() = 1">
														<xsl:apply-templates select="." mode="displayPrice" />
													</xsl:if>
												</xsl:for-each>
											</xsl:variable>
											<xsl:value-of select="$lowestSKUPrice" />
										</xsl:when>
										<xsl:otherwise>
											<xsl:apply-templates select="." mode="displayPrice" />
										</xsl:otherwise>
									</xsl:choose>
									<i class="fa fa-arrow-circle-right">
										<xsl:text> </xsl:text>
									</i>
								</p>
							</div>
						</div>
					</div>
				</a>
				<!--<xsl:if test="Manufacturer/node()!=''">
					<p class="manufacturer">
						<xsl:if test="/Page/Contents/Content[@name='makeLabel']">
							<span class="label">
								<xsl:value-of select="/Page/Contents/Content[@name='makeLabel']"/>
							</span>&#160;
						</xsl:if>
						<span class="brand">
							<xsl:value-of select="Manufacturer/node()"/>
						</span>
					</p>
				</xsl:if>-->
				<div class="explorEventInfoBtn">
					<span class="box-corner">
						<xsl:text> </xsl:text>
					</span>
					<span class="box-icn">
						<i class="fa fa-info-circle" aria-hidden="true">
							<xsl:text> </xsl:text>
						</i>
						<i class="fa fa-times-circle">
							<xsl:text> </xsl:text>
						</i>
					</span>
				</div>
				<div class="pb-overlay">
					<div class="overlay-title">
						<xsl:variable name="title">
							<xsl:apply-templates select="." mode="getDisplayName"/>
						</xsl:variable>
						<xsl:value-of select="$title"/>
					</div>
					<div class="description">
						<xsl:apply-templates select="Summary/node()" mode="cleanXhtml"/>
						<xsl:text> </xsl:text>
					</div>
					<div class="entryFooter">
						<xsl:apply-templates select="." mode="moreLink">
							<xsl:with-param name="link" select="$parentURL"/>
							<xsl:with-param name="altText">
								<xsl:apply-templates select="." mode="getDisplayName"/>
							</xsl:with-param>
						</xsl:apply-templates>
						<xsl:text> </xsl:text>
					</div>
				</div>
				<xsl:text> </xsl:text>

			</div>
			<xsl:text> </xsl:text>



		</div>

	</xsl:template>

	<!-- Stepper button template goes here-->
	<!-- Load More Button click-->
	<xsl:template match="/" mode="genericStepperNextButtonFilter">
		<xsl:param name="noPerPage"/>
		<xsl:param name="startPos"/>
		<xsl:param name="totalCount" />
		<xsl:param name="filterTarget"/>
		<xsl:param name="queryString"/>

		<xsl:variable name="thisURL">
			<xsl:apply-templates select="/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]" mode="getHref" />
			<xsl:choose>
				<xsl:when test="contains(/Page/Menu/descendant-or-self::MenuItem[@id=/Page/@id]/@url,'?')">
					<xsl:text>&amp;</xsl:text>
				</xsl:when>
				<xsl:otherwise>?</xsl:otherwise>
			</xsl:choose>
			<xsl:value-of select="$queryString"/>

		</xsl:variable>
		<xsl:choose>
			<xsl:when test="/Page[@cssFramework='bs3']">
				<div>
					<div v-if="loading" class="vueloadimg" v-show="true" >
						<div id="loading" class="panel hidden">
							<div class="panel-body">
								<h4 style="align:center;">
									<i class="fas fa-spinner fa-spin">
										<xsl:text> </xsl:text>
									</i>  Please wait... loading more experiences
									<!--<div class="{$totalCount} {$noPerPage} {$startPos}">&#160;</div>-->
								</h4>
							</div>
						</div>
					</div>
				</div>
				<div id="divloadmore" class="col-md-12">
					<ul class="pager">
						<xsl:choose>
							<xsl:when test="$totalCount &gt; ($startPos +$noPerPage)">
								<a href="javascript:;" onclick="getLoadMoreDataFilter({$startPos}, {$noPerPage}, {$totalCount}, event)"  id="lnkproductfilter" title="click here to view the next page in sequence">
									&#160;
								</a>
							</xsl:when>
							<xsl:otherwise>
							</xsl:otherwise>
						</xsl:choose>

						<!-- ### to ### of ### (At the top) -->
						<li class="itemInfo">
							<span class="pager-caption">
								<xsl:if test="$noPerPage!=1">
									<xsl:value-of select="'1'"/>
									<xsl:text> to </xsl:text>
								</xsl:if>
								<xsl:if test="$totalCount &gt;= ($startPos +$noPerPage)">
									<xsl:value-of select="$startPos + $noPerPage"/>
								</xsl:if>
								<xsl:if test="$totalCount &lt; ($startPos + $noPerPage)">
									<xsl:value-of select="$totalCount"/>
								</xsl:if> of <xsl:value-of select="$totalCount"/>
							</span>
							<lable class="countLable hidden">
								<xsl:text> </xsl:text>
							</lable>
						</li>

					</ul>
				</div>
				<div id="divLoadMoreButtonclickFilter">
				</div>

				<input type="hidden" id="Producturl" value="{$thisURL}" />
				<input type="hidden"  id="hitcount" value="1"/>
				<input type="hidden"  id="totalcount" value="{$totalCount}"/>
				<input type="hidden"  id="displaycount" value ="{$noPerPage}"/>
				<input type="hidden"  id="nextTotalcount" value="2"/>
				<input type="hidden"  id="firstdisplaycount" value ="{$noPerPage}"/>
				<input type="hidden"  id="filterTarget" value ="{$filterTarget}"/>
			</xsl:when>
			<xsl:otherwise>
				<div class="stepper">
					<p class="stepLinks">
						<!-- Next Button-->
						<xsl:choose>
							<xsl:when test="$totalCount &gt; ($startPos +$noPerPage)">
								<a href="javascript:;" onclick="getLoadMoreDataFilter({$startPos}, {$noPerPage}, {$totalCount}, event)"  id="lnkproduct" title="click here to view the next page in sequence">
								</a>
							</xsl:when>
							<xsl:otherwise>

							</xsl:otherwise>
						</xsl:choose>
					</p>
					<!-- ### to ### of ### (At the top) -->
					<p class="itemInfo">
						<xsl:if test="$noPerPage!=1">
							<xsl:value-of select="0"/>
							<xsl:text> to </xsl:text>
						</xsl:if>
						<xsl:if test="$totalCount &gt;= ($startPos +$noPerPage)">
							<xsl:value-of select="$startPos + $noPerPage"/>
						</xsl:if>
						<xsl:if test="$totalCount &lt; ($startPos + $noPerPage)">
							<xsl:value-of select="$totalCount"/>
						</xsl:if> of <xsl:value-of select="$totalCount"/>
						<lable class="countLable hidden"></lable>
					</p>
				</div>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>
</xsl:stylesheet>

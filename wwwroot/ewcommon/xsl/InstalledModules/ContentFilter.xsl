<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
	<xsl:template match="Content[@type='Module' and @contentType='ContentFilter']" mode="displayBrief">
		<xsl:choose>
			<xsl:when test="Content[@type='xform']">
				<xsl:apply-templates select="Content[@type='xform']" mode="xform"/>
			</xsl:when>
		</xsl:choose>
	
	</xsl:template>

	<xsl:template match="Content[@type='Module' and @contentType='ContentFilter']" mode="contentJS">

		<link rel="stylesheet" href="/ewthemes/Intotheblue2019/js/WrunnerRangeSlider/css/wrunner-default-theme.css" />
		<script src="/ewthemes/Intotheblue2019/js/WrunnerRangeSlider/js/wrunner-native.js" >
			<xsl:text> </xsl:text>
		</script>
		<script src="/ewthemes/Intotheblue2019/js/WrunnerRangeSlider/js/wrunner-jquery.js" >
			<xsl:text> </xsl:text>
		</script>
		<script>
			
			
			$(document).ready(function () {
					
				
				//wRunner plugin initialization in jQuery
				$('.WeightSlider').wRunner({
				type: 'range',
				rangeValue: {
					 minValue: $("#From").val(),
					maxValue: $("#To").val(),
                 },
				})
				
				
				
			});
			$('.WeightSlider').click(function(){
			var sWeight=$('.WeightSlider')[0].innerText;
			if(sWeight!=undefined){
				if(sWeight!=''){
					var aWeight = sWeight.split("\n");
					$("#From").val(aWeight[0]);
					$("#To").val(aWeight[1]);
					 document.getElementById("ContentFilter").submit();
				}
			}
			})
		</script>
		
	</xsl:template>

</xsl:stylesheet>

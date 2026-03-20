<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
  
  <xsl:template match="Content[@type='Module' and (@moduleType='DonateButton')][1]" mode="contentJS">
	<script type="text/javascript">
    $(function () {
        $("#donationAmount").change(function () {
            if ($(this).val() == 'Other') {
                 $("#donationAmount-x").removeAttr("disabled");				
				 $("#donationAmount-x").show();
				 $("#donationAmount-x").attr("name","donationAmount");
				 $("#donationAmount-x").attr("id","donationAmount");
				 
			    $("#donationAmount").attr("disabled");
                $("#donationAmount").hide();
				 $("#donationAmount").attr("name","donationAmount-y");
				 $("#donationAmount").attr("id","donationAmount-y");				 
               
            } else {
                $("#txtOther").attr("disabled", "disabled");
            }
        });
    });
</script>
	
   </xsl:template>
		
  <xsl:template match="Content[@moduleType='DonateButton']" mode="displayBrief">
    
    <form action="" method="post" class="ewXform donate-form">
      <div class="qty-product hidden">
        <label for="qty_{@id}" class="qty-label">Qty: </label>
        <input type="hidden" name="qty_{@id}" id="qty_{@id}" value="1" size="3" class="qtybox form-control" />          
      </div>
		<div class="input-group">
			<span class="input-group-text">
				£
			</span>
             <select name="donationAmount" id="donationAmount" class="form-control" placeholder="amount">
			        <option value="" disabled="disabled" >Please select</option>
                    <option value="5.00">5.00</option>
                    <option value="10.00">10.00</option>
                    <option value="15.00">15.00</option>
                    <option value="20.00">20.00</option>
                    <option value="Other">Other</option>
			    </select>
			<input name="donationAmount-x" id="donationAmount-x" disabled="disabled" class="form-control" style="display:none" value="50.00"/>
		
      <button type="submit" name="cartAdd" class="btn btn-primary" value="Add to Cart">
        Donate
      </button>
			</div>
    </form>
    
  </xsl:template>




</xsl:stylesheet>
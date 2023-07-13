<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt ew" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml" xmlns:ew="urn:ew">
  
  <xsl:template match="Content[@type='Module' and (@moduleType='DonateButton')]" mode="contentJS">
	<script type="text/javascript">
    $(function () {
        $("#donationAmount").change(function () {
            if ($(this).val() == 'Other') {
			    $("#donationAmount").attr("disabled");
                $("#donationAmount").hide();
				 $("#donationAmount").attr("name",'donationAmount-y');
				 $("#donationAmount").attr("id",'donationAmount-y');
                $("#donationAmount-x").removeAttr("disabled");
				
				 $("#donationAmount-x").show();
				 $("#donationAmount-x").attr("id",'donationAmount');
				 $("#donationAmount").attr("name",'donationAmount');
               
            } else {
                $("#txtOther").attr("disabled", "disabled");
            }
        });
    });
</script>
	
   </xsl:template>
		
  <xsl:template match="Content[@type='Module' and (@moduleType='DonateButton')]" mode="displayBrief">
    
    <form action="" method="post" class="ewXform donate-form">
      <div class="qty-product hidden">
        <label for="qty_{@id}" class="qty-label">Qty: </label>
        <input type="hidden" name="qty_{@id}" id="qty_{@id}" value="1" size="3" class="qtybox form-control" />          
      </div>
		<div class="input-group">
			<span class="input-group-addon">
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
			<div class="input-group-btn">
      <button type="submit" name="cartAdd" class="btn btn-primary" value="Add to Cart">
        Donate
      </button></div>
			</div>
    </form>
    
  </xsl:template>


  <!-- Product Brief -->
  <xsl:template match="Content[@type='Product']" mode="displayBrief">
    <xsl:param name="sortBy"/>
    <xsl:param name="pos"/>
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
    <div class="listItem list-group-item hproduct">
      <xsl:apply-templates select="." mode="inlinePopupOptions">
        <xsl:with-param name="class" select="'listItem list-group-item hproduct'"/>
        <xsl:with-param name="sortBy" select="$sortBy"/>
      </xsl:apply-templates>
      <div class="lIinner">
        <h3 class="fn title">
          <xsl:variable name="title">
            <xsl:apply-templates select="." mode="getDisplayName"/>
          </xsl:variable>
            <xsl:value-of select="$title"/>
        </h3>
        <xsl:if test="Images/img/@src!=''">
          <a href="{$parentURL}" class="url">
            <xsl:apply-templates select="." mode="displayThumbnail"/>
          </a>
        </xsl:if>
        <xsl:if test="Manufacturer/node()!=''">
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
        </xsl:if>
        <xsl:if test="StockCode/node()!=''">
          <p class="sku stockCode">
            <span class="label">
              <xsl:call-template name="term2014" />
              <xsl:text>: </xsl:text>
            </span>
            <xsl:value-of select="StockCode/node()"/>
          </p>
        </xsl:if>
        <xsl:choose>
          <xsl:when test="Content[@type='SKU']">
            <xsl:apply-templates select="Content[@type='SKU'][1]" mode="displayPrice" />
          </xsl:when>
          <xsl:otherwise>
            <xsl:apply-templates select="." mode="displayPrice" />
          </xsl:otherwise>
        </xsl:choose>
        <xsl:if test="ShortDescription/node()!=''">
          <div class="description">
            <xsl:apply-templates select="ShortDescription/node()" mode="cleanXhtml"/>
          </div>
        </xsl:if>
        <div class="entryFooter">
          <xsl:apply-templates select="." mode="addToCartButton"/>
        </div>
        <!-- Terminus class fix to floating columns -->
        <div class="terminus">&#160;</div>
      </div>
    </div>
  </xsl:template>

  <xsl:template match="Content" mode="showQuantity">
    <xsl:variable name="id">
      <xsl:choose>
        <xsl:when test="@type='SKU'">
          <xsl:value-of select="../@id"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@id"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <div class="qty-product">
      <input type="hidden" name="qty_{@id}" id="qty_{@id}" value="1"  class=""/>
    </div>
  </xsl:template>

  <!-- -->



</xsl:stylesheet>
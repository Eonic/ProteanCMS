<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
<!-- General -->


<!-- xforms -->
	<xsl:template name="msg_required">	Campo de preenchimento obrigatório</xsl:template>

	<xsl:template name="msg_required_inline">Por favor preencha o </xsl:template>
<!-- -->
<!-- ========================== XFORM : LABEL ========================== -->
<!-- -->


	<xsl:template match="label">
		<xsl:param name="cLabel"/>
		<xsl:param name="bRequired"/>
		<label>
			<xsl:if test="$cLabel!=''">
				<xsl:attribute name="for"><xsl:value-of select="$cLabel"/></xsl:attribute>
			</xsl:if>
			<xsl:choose>
				<xsl:when test="self::label[parent::select/@bind='cIsDelivery']">Entregar neste endereço</xsl:when>
				<xsl:when test="self::label[parent::select1/@ref='nShipOptKey']">Modo de entrega</xsl:when>
				<xsl:when test="self::label[parent::textarea/@ref='terms']">Termos e Condições</xsl:when>
				<xsl:when test="self::label[parent::input/@ref='creditCard/number']">Número do cartão</xsl:when>
				<xsl:when test="self::label[parent::select1/@ref='creditCard/type']">Tipo de cartão</xsl:when>
				<xsl:when test="self::label[parent::input/@ref='creditCard/expireDate']">Data de validade</xsl:when>
				<xsl:when test="self::label[parent::input/@ref='creditCard/issueNumber']">Número de emissão</xsl:when>
				<xsl:when test="self::label[parent::input/@ref='creditCard/issueDate']">Data de emissão</xsl:when>
				<xsl:when test="self::label[parent::input/@ref='creditCard/CV2']">Código</xsl:when>
				<xsl:otherwise><xsl:copy-of select="node()"/></xsl:otherwise>
			</xsl:choose>
			<xsl:if test="$bRequired='true'">
				<span class="req">*</span>
			</xsl:if>
		</label>
	</xsl:template>
	
	<!--
	I Agree to terms and conditions
		
	Title - Make Payment of x Curr by credit/Debit Card
		
	JS Required Alert - You have not any information in the field :
	-->
	
	<!--xsl:template match="submit" mode="xform">
		<xsl:variable name="class">
			<xsl:text>button</xsl:text><xsl:if test="@class!=''"><xsl:text> </xsl:text><xsl:value-of select="@class"/></xsl:if>
		</xsl:variable>
		<input type="submit" name="{@submission}" class="{$class}"  onclick="disableButton(this,'Por favor espera');">		<xsl:attribute name="value">
		<xsl:choose>
			<xsl:when test="label/node()='Submit'">Submeta</xsl:when>
			<xsl:when test="label/node()='Make Secure Payment'">Faça O Pagamento Seguro</xsl:when>
			<xsl:otherwise><xsl:value-of select="label/node()"/></xsl:otherwise>
		</xsl:choose>
		</xsl:attribute>
		</input>
	</xsl:template-->
  
  <xsl:template match="submit" mode="xform">
    <xsl:variable name="class">
      <xsl:text>button</xsl:text>
      <xsl:if test="@class!=''">
        <xsl:text> </xsl:text>
        <xsl:value-of select="@class"/>
      </xsl:if>
    </xsl:variable>
    <xsl:variable name="submission">
      <xsl:choose>
        <xsl:when test="@ref!=''">
          <xsl:value-of select="@ref"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="@submission"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <input type="submit" name="{$submission}" class="{$class}"  onclick="disableButton(this,'Por favor espera');">
      <xsl:attribute name="value">
        <xsl:choose>
          <!-- Submit -->
          <xsl:when test="label/node()='Submit'">Submeta</xsl:when>
          <!-- Make Secure Payment -->
          <xsl:when test="label/node()='Make Secure Payment'">Faça O Pagamento Seguro</xsl:when>
          <xsl:otherwise>
            <xsl:value-of select="label/node()"/>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:attribute>
    </input>

  </xsl:template>
<!-- -->
	<!-- ========================== GENERAL : CONTROL LEGEND ========================== -->
	<!-- -->
	<xsl:template match="input[not(contains(@class,'hidden'))] | secret | select | select1 | range | textarea" mode="xform_legend">
		<xsl:if test="alert">
			<span class="alert">
				<xsl:choose>
					<xsl:when test="self::input[@ref='creditCard/CV2']">Forneça por favor os últimos 3 dígitos na tira da assinatura</xsl:when>
					<xsl:when test="self::input[@ref='terms']">Você deve concordar aos termos e aos contiditons a proseguir</xsl:when>
					<xsl:otherwise><xsl:copy-of select="alert/node()"/></xsl:otherwise>
				</xsl:choose>
			</span>
		</xsl:if>
		<xsl:if test="hint">
			<span class="hint">
				<xsl:copy-of select="hint/node()"/>
			</span>
		</xsl:if>
		<xsl:if test="help">
			<span class="help">
				<xsl:choose>
					<xsl:when test="self::input[@ref='creditCard/CV2']">Os últimos 3 dígitos imprimiram na tira da assinatura na parte traseira do cartão.</xsl:when>
					<xsl:when test="self::input[@ref='creditCard/issueNumber']">Requerido para alguns maestro e cartões de solo.</xsl:when>
					<xsl:when test="self::input[@ref='creditCard/issueDate']">A data do começo no cartão, se houver um.</xsl:when>
					<xsl:otherwise><xsl:copy-of select="help/node()"/></xsl:otherwise>
				</xsl:choose>
			</span>
		</xsl:if>
	</xsl:template>

	<!-- -->
	<!-- ========================== GROUP ========================== -->
	<!-- ========================== GROUP ========================== -->
	
	<xsl:template match="group[@ref='options']" mode="xform">
		<tr>
			<td colspan="2" class="group {@class}">
				<div>
					<xsl:apply-templates select="." mode="editXformMenu"/>
					<xsl:if test="label">
						<h3>
							Seleccione a opção de entrega
						</h3>
					</xsl:if>
				</div>
				<table cellspacing="0">
					<xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | group | hint | help | alert | div | repeat" mode="xform"/>
					<xsl:if test="count(submit) &gt; 0">
					<xsl:if test="descendant-or-self::*[contains(@class,'required')]"><tr><td colspan="2"><span class="hint"><strong>*</strong> <xsl:call-template name="msg_required"/></span></td></tr></xsl:if>
						<tr>
							<td colspan="2" class="buttons">
								<!-- For xFormQuiz change how these buttons work -->
								<xsl:apply-templates select="submit" mode="xform"/>
							</td>
						</tr>
					</xsl:if>
				</table>
			</td>
		</tr>
	</xsl:template>

<!-- -->
	<!-- ========================== CONTROL : SELECTS ========================== -->
	<!-- -->
	<xsl:template match="select1[@appearance='minimal']" mode="xform_control">
		<xsl:variable name="ref">
			<xsl:apply-templates select="." mode="getRefOrBind"/>
		</xsl:variable>
		<select name="{$ref}" id="{$ref}" class="dropdown">
			<xsl:if test="@class!=''">
				<xsl:attribute name="class"><xsl:value-of select="@class"/></xsl:attribute>
			</xsl:if>
			<xsl:if test="@onChange!=''">
				<xsl:attribute name="onChange">
					<xsl:value-of select="@onChange"/>
				</xsl:attribute>
			</xsl:if>
			<option value="">
				<xsl:variable name="label_low" select="translate(label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
				<xsl:text>Por favor seleto </xsl:text>
				<xsl:value-of select="$label_low"/>
			</option>
			<xsl:apply-templates select="item" mode="xform_select"/>
		</select>
	</xsl:template>
	<!-- -->
	<xsl:template match="item[not(/Page/@adminMode)]" mode="xform_radiocheck">
		<xsl:param name="type"/>
		<xsl:param name="ref"/>
		<xsl:variable name="value" select="value"/>
		<xsl:variable name="class" select="../@class"/>
		<xsl:choose>
			<xsl:when test="$isQuizLayout='true'">
				<tr class="option">
					<td class="radiocheck">
						<xsl:apply-templates select="." mode="editXformMenu">
							<xsl:with-param name="pos" select="position()"/>
						</xsl:apply-templates>
						<input type="{$type}">
							<xsl:if test="$ref!=''">
								<xsl:attribute name="name">
									<xsl:value-of select="$ref"/>
								</xsl:attribute>
								<xsl:attribute name="id">
									<xsl:value-of select="$ref"/>_<xsl:value-of select="position()"/>
								</xsl:attribute>
							</xsl:if>
							<xsl:attribute name="class">
								radiocheckbox<xsl:if test="$class!=''">
									&#160;<xsl:value-of select="$class"/>
								</xsl:if>
							</xsl:attribute>
							<xsl:attribute name="value">
								<xsl:value-of select="value"/>
							</xsl:attribute>
							<xsl:if test="../value/node()=$value">
								<xsl:attribute name="checked">checked</xsl:attribute>
							</xsl:if>
						</input>
					</td>
					<th>
						<label for="{$ref}_{position()}">
							<xsl:if test="$class!=''">
								<xsl:attribute name="class">
									<xsl:value-of select="$class"/>
								</xsl:attribute>
							</xsl:if>
							<xsl:value-of select="label"/>
						</label>

						<!--xsl:if test="contains($class,'multiline') and position()!=last()">
							<br/>
						</xsl:if-->
					</th>
				</tr>
			</xsl:when>
			<xsl:otherwise>
				<xsl:apply-templates select="." mode="editXformMenu">
					<xsl:with-param name="pos" select="position()"/>
				</xsl:apply-templates>
				<span>
					<xsl:attribute name="class">radiocheckbox</xsl:attribute>
					<input type="{$type}">
						<xsl:if test="$ref!=''">
							<xsl:attribute name="name">
								<xsl:value-of select="$ref"/>
							</xsl:attribute>
							<xsl:attribute name="id">
								<xsl:value-of select="$ref"/>_<xsl:value-of select="position()"/>
							</xsl:attribute>
						</xsl:if>
						<xsl:attribute name="value">
							<xsl:value-of select="value"/>
						</xsl:attribute>
						<xsl:if test="../value/node()=$value">
							<xsl:attribute name="checked">checked</xsl:attribute>
						</xsl:if>
					</input>
					<label for="{$ref}_{position()}" class="radio {translate(value/node(),'/ ','')}">
						<xsl:choose>
							<xsl:when test="label/node()='I agree to the Terms and Conditions'">Eu concordo aos termos e às circunstâncias</xsl:when>
							<xsl:otherwise>
								<xsl:value-of select="label/node()"/>
							</xsl:otherwise>
						</xsl:choose>
					</label>
				</span>
				<xsl:if test="contains($class,'multiline') and position()!=last()">
					<br/>
				</xsl:if>
			</xsl:otherwise>
		</xsl:choose>

	</xsl:template>
	<!-- -->
	
	<!-- -->

</xsl:stylesheet>
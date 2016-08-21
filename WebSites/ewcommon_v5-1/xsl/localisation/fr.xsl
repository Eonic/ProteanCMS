<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
  <!-- General -->


  <!-- xforms -->
  <!-- Denotes a Required Field -->
  <xsl:template name="msg_required">	Champ obligatoire</xsl:template>

  <!-- Please complete  -->
  <xsl:template name="msg_required_inline">Svp complet </xsl:template>
  <!-- -->
  <!-- ========================== XFORM : LABEL ========================== -->
  <!-- -->


  <xsl:template match="label">
    <xsl:param name="cLabel"/>
    <xsl:param name="bRequired"/>
    <label>
      <xsl:if test="$cLabel!=''">
        <xsl:attribute name="for">
          <xsl:value-of select="$cLabel"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:choose>
        <!--  -->
        <!-- Deliver to this Address -->
        <xsl:when test="self::label[parent::select/@bind='cIsDelivery']">Entregar neste endereço</xsl:when>
        <!-- Amount to be paid -->
        <xsl:when test="self::label[parent::input/@ref='creditCard/amount']">Montant à payer</xsl:when>
        <!--  -->
        <!-- Delivery Method -->
        <xsl:when test="self::label[parent::select1/@ref='nShipOptKey']">Méthode De la Livraison</xsl:when>
        <!-- Terms and Conditions -->
        <xsl:when test="self::label[parent::textarea/@ref='terms']">Conditions générales de vente</xsl:when>
        <!-- Card Number -->
        <xsl:when test="self::label[parent::input/@ref='creditCard/number']">Numéro de carte bancaire</xsl:when>
        <!-- Card Type -->
        <xsl:when test="self::label[parent::select1/@ref='creditCard/type']">Type de carte bancaire</xsl:when>
        <!-- Expire Date -->
        <xsl:when test="self::label[parent::input/@ref='creditCard/expireDate']">Date d’expiration</xsl:when>
        <!-- Issue Number -->
        <xsl:when test="self::label[parent::input/@ref='creditCard/issueNumber']">Numéro d’émission</xsl:when>
        <!-- Issue Date -->
        <xsl:when test="self::label[parent::input/@ref='creditCard/issueDate']">Date d’émission</xsl:when>
        <!-- Security Code -->
        <xsl:when test="self::label[parent::input/@ref='creditCard/CV2']">Cryptogramme*</xsl:when>
        
          
        <xsl:otherwise>
          <xsl:copy-of select="node()"/>
        </xsl:otherwise>
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
        <xsl:when test="@ref!=''"><xsl:value-of select="@ref"/></xsl:when>
        <xsl:otherwise><xsl:value-of select="@submission"/></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <input type="submit" name="{$submission}" class="{$class}"  onclick="disableButton(this,'Attendez svp');">
      <xsl:attribute name="value">
        <xsl:choose>
          <!-- Submit -->
          <xsl:when test="label/node()='Submit'">Étape suivante</xsl:when>
          <!-- Make Secure Payment -->
          <xsl:when test="label/node()='Make Secure Payment'">Effectuer un paiement sécurisé</xsl:when>
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
          <!-- Please give the last 3 digits printed on the signature strip on the back of the card. -->
          <xsl:when test="self::input[@ref='creditCard/CV2']">Les trois derniers chiffres apparaissant sur le panneau signature au verso de votre carte bancaire.</xsl:when>
          <!-- You must agree to the terms and contiditons to proceed -->
          <xsl:when test="self::input[@ref='terms']">Pour confirmer votre réservation, veuillez prendre connaissance de nos Conditions générales de ventes puis cliquer sur le bouton ENVOYER pour enregistrer votre demande</xsl:when>
          <!-- The amount entered was not a number.  Please ensure that you do not enter any symbols, such as currency symbols. -->
          <xsl:when test="alert/span[@class='note-trans-dpnan']">La quantité écrite n'était pas un nombre. Veuillez s'assurer que vous n'écrivez aucun symbole, tel que des symboles monétaires.</xsl:when>
          <!-- The amount entered was not valid, please ensure that the amount is between X and Y -->
          <xsl:when test="alert/span[@class='note-trans-dpamount']">La quantité écrite était inadmissible, s'assurent svp que la quantité est de <xsl:value-of select="alert/span/span[@class='dpmin']"/> à <xsl:value-of select="alert/span/span[@class='dpmax']"/></xsl:when>
          <xsl:otherwise>
            <xsl:copy-of select="alert/node()"/>
          </xsl:otherwise>
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
          <!-- The last 3 digits printed on the signature strip on the back of the card. -->
          <xsl:when test="self::input[@ref='creditCard/CV2']">Les 3 derniers chiffres ont imprimé sur la bande de signature sur le dos de la carte.</xsl:when>
          <!-- Required for some Maestro and Solo cards. -->
          <xsl:when test="self::input[@ref='creditCard/issueNumber']">Requis pour certaines cartes Maestro</xsl:when>
          <!-- The start date on the card, if there is one. -->
          <xsl:when test="self::input[@ref='creditCard/issueDate']">La date de début sur la carte, s'il y a d'une.</xsl:when>
          <!-- Deposit (Must be a minimum of X up to a total value of Y) -->
          <xsl:when test="help/span[@class='hint-trans-dpamount']">Je règle mon voyage en ligne et je ne perds pas de temps. Si mon départ a lieu dans plus de 42 jours, seuls 20% du montant total de mon dossier seront prélevés (les 80% restants seront prélevés 42 jours avant mon départ).<!--Dépôt (Doit être un minimum  d'<xsl:value-of select="help/span/span[@class='dpmin']"/> jusqu'à une valeur totale d'<xsl:value-of select="help/span/span[@class='dpmax']"/>--></xsl:when>
          <!-- You may get re-directed to your own bank for further authentication. This addtional step helps protect you from online fraud. -->
          <xsl:when test="help/span[@class='note-trans-fraud']">Vous pouvez obtenir réorienté à votre propre banque pour davantage d'authentification. Les aides additionnelles de cette étape vous protègent contre la fraude.</xsl:when>
          <xsl:otherwise>
            <xsl:copy-of select="help/node()"/>
          </xsl:otherwise>
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
              <!-- Select a Delivery Option -->
              Choisissez une option de la livraison
            </h3>
          </xsl:if>
        </div>
        <table cellspacing="0">
          <xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | group | hint | help | alert | div | repeat" mode="xform"/>
          <xsl:if test="count(submit) &gt; 0">
            <xsl:if test="descendant-or-self::*[contains(@class,'required')]">
              <tr>
                <td colspan="2">
                  <span class="hint">
                    <strong>*</strong>
                    <xsl:call-template name="msg_required"/>
                  </span>
                </td>
              </tr>
            </xsl:if>
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
        <xsl:attribute name="class">
          <xsl:value-of select="@class"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="@onChange!=''">
        <xsl:attribute name="onChange">
          <xsl:value-of select="@onChange"/>
        </xsl:attribute>
      </xsl:if>
      <option value="">
        <xsl:variable name="label_low" select="translate(label,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
        <!-- Please Select -->
        <xsl:text>Svp choisi </xsl:text>
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
              <!-- I Agree to the Terms and Conditions -->
              <xsl:when test="label/node()='I agree to the Terms and Conditions'">Je suis d'accord sur les modalités et les conditions</xsl:when>
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
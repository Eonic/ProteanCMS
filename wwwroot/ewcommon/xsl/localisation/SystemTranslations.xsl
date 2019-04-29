<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
  <!--<xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/>-->


  <xsl:variable name="lang">
    <xsl:choose>
      <xsl:when test="/Page/@lang!='' and /Page/@lang!=/Page/@userlang ">
        <xsl:value-of select="/Page/@lang"/>
      </xsl:when>
      <xsl:when test="/Page/@userlang and /Page/@userlang!=''">
        <xsl:value-of select="/Page/@userlang"/>
      </xsl:when>
      <xsl:when test="/Page/@translang and /Page/@translang!=''">
        <xsl:value-of select="/Page/@translang"/>
      </xsl:when>
      <xsl:otherwise>en-gb</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <!-- ################################################################################################ --> 
  <!-- ################################ TRANSLATIONS FOR HARD-CODED TEXT ############################## -->
  <!-- ################################################################################################ -->

  <!-- Number translations at bottom of File -->

  
  
  <!-- ################################################################################################ -->
  <!-- EonicWeb Component phrases -->
  <!-- 1000+ -->
  <!-- ################################################################################################ -->

  <!-- Default template -->
  <xsl:template match="span" mode="term">
    <xsl:apply-templates select="." mode="cleanXhtml" />
  </xsl:template>

  <xsl:template name="msg_required_inline">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">hit keys for </xsl:when>
      <xsl:when test="$lang='es'">Por favor escriba </xsl:when>
      <xsl:when test="$lang='de'">Geben Sie bitte </xsl:when>
      <xsl:when test="$lang='de-at'">Geben Sie bitte </xsl:when>
      <xsl:when test="$lang='de-sw'">Geben Sie bitte </xsl:when>
      <xsl:when test="$lang='fi'">Kirjoita </xsl:when>
      <xsl:when test="$lang='fr'">S'il vous plaît type de </xsl:when>
      <xsl:when test="$lang='it'">Si prega di inserire </xsl:when>
      <xsl:when test="$lang='nl'">Gelieve </xsl:when>
      <xsl:when test="$lang='ar-ae'">الرجاء كتابة </xsl:when>
      <xsl:when test="$lang='ar-om'">الرجاء كتابة </xsl:when>
      <xsl:when test="$lang='ar-qu'">الرجاء كتابة </xsl:when>
      <xsl:otherwise>Please enter </xsl:otherwise>
    </xsl:choose> 
  </xsl:template>

  <!-- Submit Button Text -->
  
  <xsl:template match="label[node()='Make Secure Payment']" mode="submitText">
    <xsl:call-template name="term4046"/>
  </xsl:template>

  <xsl:template match="label[node()='Proceed']" mode="submitText">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">Keep Goin'</xsl:when>
      <xsl:when test="$lang='de'">Weiter</xsl:when>
      <xsl:otherwise>Proceed</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="label[node()='Continue']" mode="submitText">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">Keep Goin'</xsl:when>
      <xsl:when test="$lang='de'">Weiter</xsl:when>
      <xsl:otherwise>Continue</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="label[node()='Submit']" mode="submitText">
    <xsl:call-template name="term4044"/>
  </xsl:template>


  <!-- 1000 This must be a number -->
  <xsl:template match="span[@class='msg-1000']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">'tis not a number</xsl:when>
      <xsl:when test="$lang='de'">Bitte geben Sie eine Nummer ein</xsl:when>
      <xsl:otherwise>This must be a number</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 1001 This must be a valid date -->
  <xsl:template match="span[@class='msg-1001']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">'tis not a date</xsl:when>
      <xsl:when test="$lang='de'">Bitte geben Sie ein gültiges Datum ein</xsl:when>
      <xsl:otherwise>This must be a valid date</xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!-- 1002 This must be a valid email address -->
  <xsl:template match="span[@class='msg-1002']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">'tis not a magick address</xsl:when>
      <xsl:when test="$lang='es'">Deberá proporcionar una dirección de correo electrónico válida</xsl:when>
      <xsl:when test="$lang='de'">Bitte geben Sie eine gültige E-mail Adresse ein</xsl:when>
      <xsl:when test="$lang='fi'">Sähköpostiosoitteen on oltava voimassa</xsl:when>
      <xsl:when test="$lang='fr'">Cette adresse électronique doit être valide</xsl:when>
      <xsl:when test="$lang='it'">Deve essere un indirizzo e-mail valido</xsl:when>
      <xsl:when test="$lang='nl'">Dit moet een geldig e-mailadres zijn</xsl:when>
      <xsl:otherwise>This must be a valid email address</xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- 1002 This must be a valid email address -->
  <xsl:template match="span[@class='msg-1002a']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">'tis not all a magick address</xsl:when>
      <xsl:when test="$lang='es'">Deberá proporcionar una dirección de correo electrónico válida</xsl:when>
      <xsl:when test="$lang='de'">Bitte geben Sie eine gültige E-mail Adresse ein</xsl:when>
      <xsl:when test="$lang='fi'">Sähköpostiosoitteen on oltava voimassa</xsl:when>
      <xsl:when test="$lang='fr'">Cette adresse électronique doit être valide</xsl:when>
      <xsl:when test="$lang='it'">Deve essere un indirizzo e-mail valido</xsl:when>
      <xsl:when test="$lang='nl'">Dit moet een geldig e-mailadres zijn</xsl:when>
      <xsl:otherwise>These must all be valid email addresses</xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!-- 1003 Please enter the valid image code -->
  <xsl:template match="span[@class='msg-1003']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">'tis not a valid image code</xsl:when>
      <xsl:when test="$lang='de'">Bitte geben Sie den gültigen Code der Abbildung ein</xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="."/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 1004 Invalid File Extension -->
  <xsl:template match="span[@class='msg-1004']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">'this not a proper file extension</xsl:when>
      <xsl:when test="$lang='de'">ungültiges Format</xsl:when>
      <xsl:otherwise>Invalid file extension</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 1005 This must be a valid format -->
  <xsl:template match="span[@class='msg-1005']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">'tis not a valid format</xsl:when>
      <xsl:otherwise>This must be a valid format</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 1006 No File Selected -->
  <xsl:template match="span[@class='msg-1006']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">Ya must select a file.</xsl:when>
      <xsl:when test="$lang='de'">Kein Dokument ausgewählt</xsl:when>
      <xsl:otherwise>No file selected</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 1007 This must be completed -->
  <xsl:template match="span[@class='msg-1007']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='de'">Bitte vervollständigen</xsl:when>
      <xsl:when test="$lang='en-pr'">Ya must fill this out</xsl:when>
      <xsl:when test="$lang='es'">Debe completar esta sección</xsl:when>
      <xsl:when test="$lang='fi'">Tämä täytyy täyttää</xsl:when> 
      <xsl:when test="$lang='fr'">Saisie obligatoire</xsl:when>
      <xsl:when test="$lang='it'">Da completare</xsl:when>
      <xsl:when test="$lang='nl'">Invullen verplicht</xsl:when>
      <xsl:otherwise>This must be completed</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 1008 This information must be valid -->
  <xsl:template match="span[@class='msg-1008']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">T'information must be valid</xsl:when>
      <xsl:when test="$lang='de'">Die Information muss gültig sein</xsl:when>
      <xsl:when test="$lang='fr'">Ces informations doivent être valides</xsl:when>
      <xsl:when test="$lang='it'">Queste informazioni devono essere valide</xsl:when>
      <xsl:otherwise>This information must be valid</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 1009 The file you are uploading is too large -->
  <xsl:template match="span[@class='msg-1009']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">The file be too large</xsl:when>
      <xsl:when test="$lang='de'">Das Dokument, das Sie hoch laden ist zu groß</xsl:when>
      <xsl:otherwise>The file you are uploading is too large</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 1010 Your details have been updated -->
  <xsl:template match="span[@class='msg-1010']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">Yar be updated</xsl:when>
      <xsl:when test="$lang='de'">Ihre Details wurde überarbeitet</xsl:when>
      <xsl:when test="$lang='fr'">Vos informations ont été mises à jour</xsl:when>
      <xsl:when test="$lang='it'">I tuoi dati sono stati aggiornati</xsl:when>
      <xsl:otherwise>Your details have been updated.</xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!-- 1011 Your password has been emailed to you -->
  <xsl:template match="span[@class='msg-1011']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">Ya secret has set sail for ya</xsl:when>
      <xsl:when test="$lang='de'">Ihr Passwort wurde per E-mail an Sie gesandt</xsl:when>
      <xsl:when test="$lang='fr'">Votre mot de passe vous a été envoyé par courrier électronique</xsl:when>
      <xsl:when test="$lang='it'">La password ti è stata inviata per e-mail</xsl:when>
      <xsl:otherwise>Your password has been emailed to you</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 1012 User account is not active -->
  <xsl:template match="span[@class='msg-1012']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='de'">Benutzerkonto ist nicht aktiv</xsl:when>
      <xsl:when test="$lang='fr'">Compte utilisateur non activé</xsl:when>
      <xsl:when test="$lang='it'">Il tuo conto utente non è attivo</xsl:when>
      <xsl:when test="$lang='en-pr'">Tha deckswab is in Davey Jones locker</xsl:when>
      <xsl:otherwise>User account is not active</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 1013 User account has been disabled -->
  <xsl:template match="span[@class='msg-1013']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='fr'">Compte utilisateur désactivé</xsl:when>
      <xsl:when test="$lang='it'">Conto utente disabilitato</xsl:when>
      <xsl:when test="$lang='de'">Benutzerkonto wurde gesperrt</xsl:when>
      <xsl:when test="$lang='en-pr'">Tha deckswab has been banished</xsl:when>
      <xsl:otherwise>User account has been disabled</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 1014 The Password is not valid -->
  <xsl:template match="span[@class='msg-1014']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='de'">Ihr passwort ist ungültig</xsl:when>
      <xsl:when test="$lang='en-pr'">Ya secret is wrong</xsl:when>
      <xsl:when test="$lang='es'">La contraseña no es válida</xsl:when>
      <xsl:when test="$lang='fi'">Salasana ei kelpaa</xsl:when>
      <xsl:when test="$lang='fr'">Mot de passe non valide</xsl:when>
      <xsl:when test="$lang='it'">Password non valida</xsl:when>
      <xsl:when test="$lang='nl'">Wachtwoord ongeldig</xsl:when>
      <xsl:otherwise>The password is not valid</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 1015 The username was not found -->
  <xsl:template match="span[@class='msg-1015']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='de'">Ihr benutzername wurde nicht gefunden</xsl:when>
      <xsl:when test="$lang='en-pr'">Ya moniker were nay found</xsl:when>
      <xsl:when test="$lang='es'">No se encontró el nombre de usuario</xsl:when>
      <xsl:when test="$lang='fi'">Käyttäjätunnusta ei löydy</xsl:when>
      <xsl:when test="$lang='fr'">Nom d'utilisateur introuvable</xsl:when>
      <xsl:when test="$lang='it'">Nome utente non trovato</xsl:when>
      <xsl:when test="$lang='nl'">Gebruikersnaam niet gevonden</xsl:when>
      <xsl:otherwise>These credentials do not match a valid account</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 1016 User account has expired -->
  <xsl:template match="span[@class='msg-1016']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='fr'">Compte utilisateur désactivé</xsl:when>
      <xsl:when test="$lang='it'">Conto utente scaduto</xsl:when>
      <xsl:when test="$lang='de'">Benutzerkonto ist abgelaufen</xsl:when>
      <xsl:when test="$lang='en-pr'">Tha deckswab has expired</xsl:when>
      <xsl:otherwise>User account has expired</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 1017 Passwords must match -->
  <xsl:template match="span[@class='msg-1017']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='fr'">Mots de passe différents</xsl:when>
      <xsl:when test="$lang='it'">Le password devono corrispondere</xsl:when>
      <xsl:when test="$lang='de'">Passwort muss übereinstimmen</xsl:when>
      <xsl:when test="$lang='en-pr'">Ya secrets must match</xsl:when>
      <xsl:otherwise>Passwords must match</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 1018 Passwords must be 4 characters long -->
  <xsl:template match="span[@class='msg-1018']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='fr'">La longueur des mots de passe doit être égale à 4 caractères</xsl:when>
      <xsl:when test="$lang='it'">Le password devono essere lunghe 4 caratteri</xsl:when>
      <xsl:when test="$lang='de'">Passwort muss 4 stellig sein</xsl:when>
      <xsl:when test="$lang='en-pr'">Ya secrets must be 4 characters long</xsl:when>
      <xsl:otherwise>Passwords must be 4 characters long</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 1019 Registration Code Incorrect -->
  <xsl:template match="span[@class='msg-1019']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='de'">Registrationscode ist inkorrekt</xsl:when>
      <xsl:when test="$lang='en-pr'">Yar special code is not good</xsl:when>
      <xsl:otherwise>Registration code incorrect</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 1020 This user has been added -->
  <xsl:template match="span[@class='msg-1020']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='fr'">Utilisateur ajouté</xsl:when>
      <xsl:when test="$lang='it'">Questo utente è stato aggiunto</xsl:when>
      <xsl:when test="$lang='de'">Dieser Benutzer wurde hinzugefügt</xsl:when>
      <xsl:when test="$lang='en-pr'">This swab has been added</xsl:when>
      <xsl:otherwise>This user has been added</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 1020 This user has been added -->
  <xsl:template match="span[@class='msg-1029']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='fr'">Thanks for registering you have been sent an email with a link you must click to activate your account</xsl:when>
      <xsl:when test="$lang='it'">Thanks for registering you have been sent an email with a link you must click to activate your account</xsl:when>
      <xsl:when test="$lang='de'">Thanks for registering you have been sent an email with a link you must click to activate your account</xsl:when>
      <xsl:when test="$lang='en-pr'">Thanks for registering you have been sent an email with a link you must click to activate your account</xsl:when>
      <xsl:otherwise>Thanks for registering you have been sent an email with a link you must click to activate your account</xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!-- 1030 The code you have provided is invalid for this transaction -->
  <xsl:template match="span[@class='msg-1030']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='fr'">TBC</xsl:when>
      <xsl:when test="$lang='it'">TBC</xsl:when>
      <xsl:when test="$lang='de'">TBC</xsl:when>
      <xsl:when test="$lang='en-pr'">TBC</xsl:when>
      <xsl:otherwise>The code you have provided is invalid for this transaction</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 1030 The code you have provided is invalid for this transaction -->
  <xsl:template match="span[@class='msg-1031']" mode="term">
    <xsl:choose>
      <xsl:when test="$lang='fr'">TBC</xsl:when>
      <xsl:when test="$lang='it'">TBC</xsl:when>
      <xsl:when test="$lang='de'">TBC</xsl:when>
      <xsl:when test="$lang='en-pr'">TBC</xsl:when>
      <xsl:otherwise>This email address already has an account, please use password reminder facility</xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <!-- 1021 This username already exists in <membership>. Please select another. -->
  <xsl:template name="term1021">
    <xsl:param name="p1"/>
    <xsl:choose>
      <xsl:when test="$lang='fr'">
        <xsl:text>This username already exists in </xsl:text>
        <xsl:value-of select="$p1"/>
        <xsl:text>. Please select another.</xsl:text>
      </xsl:when>
      <xsl:when test="$lang='it'">
        <xsl:text>Questo nome utente esiste già in </xsl:text>
        <xsl:value-of select="$p1"/>
        <xsl:text>. Sciegline un altro.</xsl:text>
      </xsl:when>
      <xsl:when test="$lang='de'">
        <xsl:text>Dieser Benutzername besteht bereits</xsl:text>
        <xsl:value-of select="$p1"/>
        <xsl:text>, Bitte anderen wählen</xsl:text>
      </xsl:when>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Tha moniker has been taken, in </xsl:text>
        <xsl:value-of select="$p1"/>
        <xsl:text>, choose another one.</xsl:text>
      </xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- 1022 This username already exists. Please select another. -->
  <xsl:template name="term1022">
    <xsl:choose>
      <xsl:when test="$lang='fr'">Ce nom d'utilisateur existe déjà, Sélectionnez-en un autre.</xsl:when>
      <xsl:when test="$lang='it'">Questo nome utente esiste già. Sciegline un altro.</xsl:when>
      <xsl:when test="$lang='de'">Dieser Benutzername besteht bereits, bitte anderen wählen.</xsl:when>
      <xsl:when test="$lang='en-pr'">Tha moniker has been taken, choose another one.</xsl:when>
      <xsl:otherwise>This username already exists. Please select another.</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 1023 Required input -->
  <xsl:template name="term1023">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">completed the' must</xsl:when>
      <xsl:when test="$lang='fr'">entrée requis</xsl:when>
      <xsl:when test="$lang='it'">ingresso richiesto</xsl:when>
      <xsl:when test="$lang='de'">erforderliche Eingabe</xsl:when>

      <xsl:when test="$lang='es'">campos necesarios </xsl:when>
      <xsl:when test="$lang='de'">erforderlichen Felder </xsl:when>
      <xsl:when test="$lang='de-at'">erforderlichen Felder </xsl:when>
      <xsl:when test="$lang='de-sw'">erforderlichen Felder </xsl:when>
      <xsl:when test="$lang='fr'">champs requis </xsl:when>
      <xsl:when test="$lang='it'">campi richiesti </xsl:when>
      <xsl:when test="$lang='nl'">verplichte velden </xsl:when>
      <xsl:when test="$lang='sv'">obligatoriska fält </xsl:when>
      <xsl:when test="$lang='tr'">gerekli alanlar </xsl:when>
      <xsl:when test="$lang='he-il'">שדות חובה </xsl:when>
    
      <xsl:otherwise>required input</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- 1022 This username already exists. Please select another. -->
  <xsl:template name="term1024">
    <xsl:choose>
      <xsl:when test="$lang='fr'">TBC</xsl:when>
      <xsl:when test="$lang='it'">TBC</xsl:when>
      <xsl:when test="$lang='de'">TBC</xsl:when>
      <xsl:when test="$lang='en-pr'">Ahhhrr... This email address already has an account, please use password reminder facility.</xsl:when>
      <xsl:otherwise>This email address already has an account, please use password reminder facility.</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ################################################################################################ -->
  <!-- EonicWeb Common Template phrases -->
  <!-- 2000+ -->
  <!-- ################################################################################################ -->

  <!-- 2000 Error Messages. -->
  <xsl:template name="term2000">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">Ye template cannot be spyed.</xsl:when>
      <xsl:otherwise>The template cannot be found</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2001">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">Ye template</xsl:when>
      <xsl:otherwise>The template</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2002">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">not be here.</xsl:when>
      <xsl:otherwise>is not available in the XSLT.</xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template name="term2003">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <h2>Unfortunately this site has experienced an error.</h2>
        <h3>We take all errors very seriously.</h3>
        <p>
          This error has been recorded and details sent to <a href="http://www.eonic.co.uk">Eonic</a> who provide technical support for this website.
        </p>
        <p>
          Eonic welcome any feedback that helps us improve our service and that of our clients, please email any supporting information you might have as to how this error arose to <a href="mailto:support@eonic.co.uk">support@eonic.co.uk</a> or alternatively you are welcome call us on +44 (0)1892 534044 between 9.30am and 5.00pm GMT.
		</p>
        <p>Please contact the owner of this website for any enquiries specific to the products and services outlined within this site.</p>
        <a href="javascript:history.back();">Click here to return to the previous page.</a>
      </xsl:when>
      <xsl:otherwise>
        <h2>Unfortunately this site has experienced an error.</h2>
        <h3>We take all errors very seriously.</h3>
        <p>
          This error has been recorded and details sent to <a href="http://www.eonic.co.uk">Eonic</a> who provide technical support for this website.
        </p>
        <p>
          Eonic welcome any feedback that helps us improve our service and that of our clients, please email any supporting information you might have as to how this error arose to <a href="mailto:support@eonic.co.uk">support@eonic.co.uk</a> or alternatively you are welcome call us on +44 (0)1892 534044 between 9.30am and 5.00pm GMT.
		</p>
        <p>Please contact the owner of this website for any enquiries specific to the products and services outlined within this site.</p>
        <a href="javascript:history.back();">Click here to return to the previous page.</a>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template name="term2004">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">Get Flash Player</xsl:when>
      <xsl:otherwise>Get Flash Player</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2005">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">to see this video.</xsl:when>
      <xsl:otherwise>to see this video.</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2006">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">Click here to return to the news article list</xsl:when>
      <xsl:otherwise>Click here to return to the news article list</xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template match="span[@class='term2007']" mode="term">
    <xsl:call-template name="term2007" />
  </xsl:template>

  <xsl:template name="term2007">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Blower</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Tel</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2008">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Paper Blower</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Fax</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2009">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Ahh-mail</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Email</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2010">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Magic book address</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Website</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2011">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Department</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Department</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2012">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Click here to return to the contact list</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Click here to return to the contact list</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2013">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Click here to return to the event list</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Click here to return to the event list</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2014">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Stock code</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Stock code</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2015">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Click here to return to the product list</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Click here to return to the product list</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2016">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Reviews</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Reviews</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2017">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Ye olde price</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>RRP</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2018">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Reviewed by</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Reviewed by</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2019">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>on</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>on</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2020">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Rating</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Rating</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2021">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>stars</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>stars</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2022">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Back to list</xsl:text>
      </xsl:when>
      <xsl:when test="$lang='jp'">
        <xsl:text>リストに戻る</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Back to list</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2023">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>about</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>about</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2024">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Website by</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Website by</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template name="term2025">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Eonic</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Eonic</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2026">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Go to</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Go to</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2027">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Click here to download a copy of this document</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Click here to download a copy of this document</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2027a">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Click here to view this document</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Click here to view this document</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2028">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Download</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Download</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2029">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Adobe PDF</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Adobe PDF</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2030">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Word document</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Word document</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2031">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Excel spreadsheet</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Excel spreadsheet</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2032">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Zip archive</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Zip archive</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2033">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>PowerPoint presentation</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>PowerPoint presentation</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2034">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>PowerPoint slideshow</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>PowerPoint slideshow</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2034a">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Arrrrghcess database</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Access database</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2035">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>JPEG image file</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>JPEG image file</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2036">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>GIF image file</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>GIF image file</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2037">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>PNG image file</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>PNG image file</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2038">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>unknown file type</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>unknown file type</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2039">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Tags</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Tags</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2040">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Click here to return to the tags list</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Click here to return to the tags list</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2041">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Testimonial from</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Testimonial from</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2042">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Read more</xsl:text>
      </xsl:when>
      <xsl:when test="$lang='jp'">
        <xsl:text>もっと読む.</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Read more </xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2043">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Click here to return to the testimonial list</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Click here to return to the testimonial list</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2044">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <a href="http://www.macromedia.com/go/getflashplayer">
          <xsl:text>Get Flash Player</xsl:text>
        </a>
        <xsl:text>to see this video.</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <a href="http://www.macromedia.com/go/getflashplayer">
          <xsl:text>Get Flash Player</xsl:text>
        </a>
        <xsl:text>to see this video.</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2045">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Author</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Author</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2046">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Copyright</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Copyright</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2047">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Click here to return to the video list</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Click here to return to the video list</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2048">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Click here to return to the contact list</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Click here to return to the contact list</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2049">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Click to enlarge</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Click to enlarge</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2050">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Email</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Email</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2051">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Show results</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Show results</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2052">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>This poll opens for voting at the beginning of</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>This poll opens for voting at the beginning of</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2053">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>The results to this poll are private.</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>The results to this poll are private.</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2054">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>The results to this poll will be revealed</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>The results to this poll will be revealed</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2055">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>at the end of</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>at the end of</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2056">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>when the poll closes.</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>when the poll closes.</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2057">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>You have already voted on this poll.</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>You have already voted on this poll.</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2058">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>This poll is only available to registered users</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>This poll is only available to registered users</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2059">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Thank you, your vote has been counted.</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Thank you, your vote has been counted.</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2060">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Total votes</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Total votes</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2061">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Click here to return to the feed list</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Click here to return to the feed list</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2062">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Added on</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Added on</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2063">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Contract type</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Contract type</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2064">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Ref</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Ref</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2065">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Location</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Location</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2066">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Salary</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Salary</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2067">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Application Deadline</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Application Deadline</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2068">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Added on</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Added on</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2069">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Reference</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Reference</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2070">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Contract type</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Contract type</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2071">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Click here to return to the news article list</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Click here to return to the news article list</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2072">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Click here to read more information about</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Click here to read more information about</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template name="term2073">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Image gallery</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Image gallery</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2074">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Get 'em all</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Download all in a Zip file</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  
  <!--  ==  Recipes  =============================================================================  -->
  <xsl:template name="term2075">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Aye, in'edients</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Ingredients</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2075a">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Ye number of serrrrrvings</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>No. of servings</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2076">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Prep-aaarrrgh-ration time</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Preparation time</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2077">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Cookin' time</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Cooking time</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2078">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>'Ow ta doit!</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Method</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2079">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Written</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Published</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template match="span[@class='term2080']" mode="term">
    <xsl:call-template name="term2080" />
  </xsl:template>

  <xsl:template name="term2080">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Magic Pocket Talkie Box</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Mobile</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template name="term2081">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Shipwright</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Manufacturer</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template name="term2082">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Size</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Size</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!--  ==  Job Vacancies =============================================================================  -->
 
  <xsl:template name="term2084">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Responsibilities</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Responsibilities</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2085">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Industry</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Industry</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2086">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Occupation</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Occupation</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2087">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Base Salary</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Base Salary</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2088">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Currency</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Currency</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2089">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Figure</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Figure</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2090">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Work Hours</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Work Hours</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2092">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Description</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Description</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template name="term2093">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Responsibities</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Responsibities</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
    
  </xsl:template>
  <xsl:template name="term2094">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Skills</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Skills</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
    
  </xsl:template>
  <xsl:template name="term2095">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Education Requirements</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Education Requirements</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template name="term2096">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Experience Requirements</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Experience Requirements</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template name="term2097">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Qualifications</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Qualifications</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template name="term2098">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Incentives</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Incentives</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template name="term2099">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Previous</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Previous</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template name="term2100">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Next</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Next</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
    <xsl:template name="term2100a">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Find out more</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Find out more</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

 
 <!--organisation-->

  <xsl:template name="term2101">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Profile</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Profile</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2102">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Type of Organization </xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Type of Organization </xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2103">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>legal Name </xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Legal Name </xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2104">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>foundingDate</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Founding Date </xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2105">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Latitude</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Latitude </xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2106">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Longitude</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Longitude </xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2107">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Dun &amp; Bradstreet Number</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Dun &amp; Bradstreet Number </xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2108">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Money grabber ID</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Tax ID </xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

    <xsl:template name="term2109">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text> another Money grabber ID</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Vax ID </xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2110">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text> currenciesAccepted</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text> Currencies Accepted</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

   <xsl:template name="term2111">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text> opening Hours</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Opening Hours </xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

    <xsl:template name="term2112">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>payment Accepted</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Payment Accepted </xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

    <xsl:template name="term2113">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>priceRange</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Price Range </xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2114">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Address</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Address </xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!--Training Course Module -->


  <xsl:template name="term2115">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Next Course</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Next Course</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2116">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text> Ticket Cost</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text> Ticket Cost</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term2117">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Get 'em</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Place Order</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  




  <!-- ################################################################################################ -->
  <!-- EonicWeb Cart Template phrases -->
  <!-- 3000+ -->
  <!-- ################################################################################################ -->

  <xsl:template name="term3001">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Your Shopping Cart</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Your Shopping Cart</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3002">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Items</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Items</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3003">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Click here to view full details of your shopping cart</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Click here to view full details of your shopping cart</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3004">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Show details</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Show details</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template name="term3005a">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Tis Nowt 'ere</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Your basket is empty</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template name="term3005">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Ya ordez - timeout</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>The order has timed out and cannot continue</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3006">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Proceed</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Proceed</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3007">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Ya Ordez</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Your Order</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3008">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Ya Ordez - Please enter a discount code</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Your Order - Please enter a discount code</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3009">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Ya Ordez - What else ya want to be saying</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Your Order - Please tell us any special requirements</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template name="term3010">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Additional information for your order</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Additional information for your order</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3011">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Haggling code</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Promotional code entered</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3012">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Click here to edit the notes on this order.</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Click here to edit the notes on this order.</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3013">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Edit Notes</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Edit Notes</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3014">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Registration</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Registration</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3015">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Proceed without registering</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Proceed without registering</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3016">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Currency selection</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Currency selection</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3017">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Ya Ordez - Enter your billing details</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Enter your billing details</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3018">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Ya Ordez - Enter the delivery address</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Enter the delivery address</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3019">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Ya Ordez - Please enter your preferred payment and shipping methods.</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Please enter your preferred payment and shipping methods.</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3020">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Ya Ordez - Please enter your payment details.</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Please enter your payment details.</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
    <xsl:template name="term3020-1">
        <xsl:choose>
            <xsl:when test="$lang='en-pr'">
                <xsl:text>3D Secure - Please Belt and Braces Yer Plastic.</xsl:text>
            </xsl:when>
            <xsl:otherwise>
                <xsl:text>3D Secure - Please Validate Your Card.</xsl:text>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
    
  <xsl:template name="term3021">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Ya Receipt - Thank you for your order.</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Your Invoice - Thank you for your order.</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>



  <xsl:template name="term3022">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Invoice Date</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Invoice Date</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3023">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Invoice Reference</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Invoice Reference</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3024">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Payment received</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Payment received</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3025">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Final Payment Reference</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Final Payment Reference</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3026">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <p>
          Thank you for your deposit. To pay the outstanding balance, please note your Final Payment Reference, above.  <em>Instructions on paying the outstanding balance have been e-mailed to you.</em>
        </p>
        <p>
          If you have any queries, please call for assistance.
        </p>
      </xsl:when>
      <xsl:otherwise>
        <p>
          Thank you for your deposit. To pay the outstanding balance, please note your Final Payment Reference, above.  <em>Instructions on paying the outstanding balance have been e-mailed to you.</em>
        </p>
        <p>
          If you have any queries, please call for assistance.
        </p>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3027">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Payment made</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Payment made</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3028">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Total Payment Received</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Total Payment Received</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3029">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>paid in full</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>paid in full</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3030a">
    Please add items to your shopping basket 
  </xsl:template>

  <xsl:template name="term3030">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <p>
          <strong>The order has timed out and cannot continue</strong>, this may be due to some of the following reasons:
        </p>
        <ol>
          <li>You may have disabled cookies or they are undetectable.  The shopping cart requires cookies to be enabled in order to proceed.</li>
          <li>The order had been left for over ten minutes without any updates.  The details are automatically removed for security purposes.</li>
          <li>The session has been lost due to network connection issues at your end, our end or somewhere in between.  The details are automatically removed for security purposes.</li>
        </ol>
        <p>Please ensure cookies are enabled in your browser to continue shopping, or call for assistance.</p>
        <p>
          <b>No transaction has been made.</b>
        </p>
      </xsl:when>
      <xsl:otherwise>
        <p>
          <strong>The order has timed out and cannot continue</strong>, this may be due to some of the following reasons:
        </p>
        <ol>
          <li>You may have disabled cookies or they are undetectable.  The shopping cart requires cookies to be enabled in order to proceed.</li>
          <li>The order had been left for over ten minutes without any updates.  The details are automatically removed for security purposes.</li>
          <li>The session has been lost due to network connection issues at your end, our end or somewhere in between.  The details are automatically removed for security purposes.</li>
        </ol>
        <p>Please ensure cookies are enabled in your browser to continue shopping, or call for assistance.</p>
        <p>
          <b>No transaction has been made.</b>
        </p>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3031">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        The item(s) you are trying to add cannot be added to this shopping basket.<br/><br/>
        Please proceed to the checkout and pay for the items in the basket, and then continue with your shopping.
      </xsl:when>
      <xsl:otherwise>
        The item(s) you are trying to add cannot be added to this shopping basket.<br/><br/>
        Please proceed to the checkout and pay for the items in the basket, and then continue with your shopping.
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3032">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        There is no valid delivery option for this order.  This may be due to a combination of location, price, weight or quantity.<br/><br/>
        Please call for assistance.
      </xsl:when>
      <xsl:otherwise>
        There is no valid delivery option for this order.  This may be due to a combination of location, price, weight or quantity.<br/><br/>
        Please call for assistance.
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3033">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        The transaction was cancelled during the payment processing - this was either at your request or the request of our payment provider, Worldpay.<br/><br/>
        Please call for more information.
      </xsl:when>
      <xsl:otherwise>
        The transaction was cancelled during the payment processing - this was either at your request or the request of our payment provider, Worldpay.<br/><br/>
        Please call for more information.
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3034">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        The order reference could not be found, or the order did not have the correct status.  This may occur if you have tried to pay for the same order twice, or if there has been a long duration between visiting our payment provider, Worldpay's site and entering payment details.<br/><br/>
        Please call for assistance.
      </xsl:when>
      <xsl:otherwise>
        The order reference could not be found, or the order did not have the correct status.  This may occur if you have tried to pay for the same order twice, or if there has been a long duration between visiting our payment provider, Worldpay's site and entering payment details.<br/><br/>
        Please call for assistance.
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3035">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        The payment provider, Worldpay, did not provide a valid response.<br/><br/>
        Please call for assistance.
      </xsl:when>
      <xsl:otherwise>
        The payment provider, Worldpay, did not provide a valid response.<br/><br/>
        Please call for assistance.
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3036">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>
					Delivery Address Details
				</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>
					Delivery Address Details
				</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3037">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Your order will be delivered to</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Your order will be delivered to</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3038">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        Click on this icon to remove an item from the order.<br/><br/>
        If you amend the quantity of items please click 'Update Order' before continuing to browse.
      </xsl:when>
      <xsl:otherwise>
        Click on this icon to remove an item from the order.<br/><br/>
        If you amend the quantity of items please click 'Update Order' before continuing to browse.
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3038b">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        If you amend the quantity of items please click 'Update Order' before continuing to browse.
      </xsl:when>
      <xsl:otherwise>
        If you amend the quantity of items please click 'Update Order' before continuing to browse.
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3039">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Qty</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Qty</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3040">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Description</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Description</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3041">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Ref</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Ref</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3042">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Price</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Price</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

    <xsl:template name="term3042a">
        <xsl:choose>
            <xsl:when test="$lang='en-pr'">
                <xsl:text>VAT</xsl:text>
            </xsl:when>
            <xsl:otherwise>
                <xsl:text>VAT</xsl:text>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>

  <xsl:template name="term3043">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Line Total</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Line Total</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3044">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Shipping Cost</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Shipping Cost</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3045">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Sub Total</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Sub Total</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3046">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>VAT at</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>VAT at</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3047">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Tax at</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Tax at</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3048a">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Total</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Total</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3048b">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Total</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Total Items</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3048c">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Total</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Total Ex Tax</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template name="term3048d">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Total</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Tax at</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3048">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Total Value</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Total Value</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3049">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Transaction Made</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Transaction Made</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3050">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Payment Received</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Payment Received</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3051">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Deposit Payable</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Deposit Payable</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3052">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Amount Outstanding</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Amount Outstanding</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3053">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>RRP</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>RRP</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template name="term3054">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>save</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>save</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3055">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Qty</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Qty</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3056">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Add to Gift List</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Add to Gift List</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3057">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Add to Quote</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Add to Quote</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3058">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Add to Cart</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Add to Cart</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3059">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Go To Checkout</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Go To Checkout</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3060">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Continue Shopping</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Continue Shopping</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3061">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Update Order</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Update Order</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3062">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Empty Order</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Empty Order</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3063">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Booking Fee</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Booking Fee</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3064">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>To order, please enter the quantities you require.</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>To order, please enter the quantities you require.</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3065">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>This form lists product options for this page</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>This form lists product options for this page</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3066">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Renew</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Renew</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3067">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Change to</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Change to</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3068">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Buy</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Buy</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template name="term3069">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Per</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Per</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3070">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Details</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Details</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3071">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Blower</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Tel</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3072">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Fax</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Fax</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3073">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Email</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Email</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3074">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Your comments sent with this order</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Your comments sent with this order</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3075">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Details</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Details</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3076">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Terms and Conditions</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Terms and Conditions</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3077">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Available On</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Available On</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3078">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Arrr ye be done 'ere</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>          
          Close Invoice and Return to Site</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3079">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Instrument</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>          
          Instrument</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>



  <xsl:template name="term3080">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Instrument</xsl:text>
      </xsl:when>
      <xsl:otherwise>     
         <span> You have requested more items than are currently <em>in stock</em> for <strong></strong> (only <span class="quantity-available="></span> available).</span><br/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3081">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Please adjust the quantities you require, or call for assistance.</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Please adjust the quantities you require, or call for assistance.</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3082">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Show Order Details</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Show Order Details</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3083">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Update</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Update</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3084">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Delivery Method</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Delivery Method</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3085">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>I agree to the Terms and Conditions</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>I agree to the Terms and Conditions</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template name="term3086">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>(Cookies disabled)</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>(Cookies disabled)</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3087">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Cookies disabled</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>This may be due to some of the following reasons</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3088">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>You may have disabled cookies or they are undetectable. The shopping cart requires cookies to be enabled in order to proceed.</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>You may have disabled cookies or they are undetectable. The shopping cart requires cookies to be enabled in order to proceed.</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3089">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>The order had been left for over ten minutes without any updates. The details are automatically removed for security purposes.</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>The order had been left for over ten minutes without any updates. The details are automatically removed for security purposes.</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3090">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>No translation has been made.</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>No translation has been made.</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template name="term3091">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>There is no valid delivery option for this order. This may be due to a combination of location, price, weight or quantity.</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>There is no valid delivery option for this order. This may be due to a combination of location, price, weight or quantity.</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3092">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Please call for assistance.</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Please call for assistance.</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term3093">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Ye Arrh back to Port</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Back to Home Page</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <!-- ################################################################################################ -->
  <!-- EonicWeb Membership Template phrases -->
  <!-- 4000+ -->
  <!-- ################################################################################################ -->
  <!-- FORM LABEL MATCHES -->
  <xsl:template match="span[@class='term4000']" mode="term">
    <xsl:call-template name="term4000" />
  </xsl:template>
  <xsl:template name="term4000">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">Moniker</xsl:when>
      <xsl:when test="$lang='es'">Nombre de usuario</xsl:when>
      <xsl:when test="$lang='fr'">Nom d'utilisateur</xsl:when>
      <xsl:when test="$lang='de'">Benutzername</xsl:when>
      <xsl:when test="$lang='nl'">Gebruikersnaam</xsl:when>
      <xsl:when test="$lang='fi'">Käyttäjätunnus</xsl:when>
      <xsl:otherwise>
        <xsl:text>Username</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="span[@class='term4001']" mode="term">
    <xsl:call-template name="term4001" />
  </xsl:template>
  
  <xsl:template name="term4001">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">Passkey</xsl:when>
      <xsl:when test="$lang='es'">Contraseña</xsl:when>
      <xsl:when test="$lang='fr'">Mot de passe</xsl:when>
      <xsl:when test="$lang='de'">Kennwort</xsl:when>
      <xsl:when test="$lang='nl'">Wachtwoord</xsl:when>
      <xsl:when test="$lang='fi'">Salasana</xsl:when>
      <xsl:otherwise>
        <xsl:text>Password</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="span[@class='term4002']" mode="term">
    <xsl:call-template name="term4002" />
  </xsl:template>

  <xsl:template name="term4002">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Last name</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Last name</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="span[@class='term4003']" mode="term">
    <xsl:call-template name="term4003" />
  </xsl:template>

  <xsl:template name="term4003">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Ship</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Company</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="span[@class='term4004']" mode="term">
    <xsl:call-template name="term4004" />
  </xsl:template>

  <xsl:template name="term4004">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Job</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Position</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="span[@class='term4005']" mode="term">
    <xsl:call-template name="term4005" />
  </xsl:template>
  
  <xsl:template name="term4005">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Magic post</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Email</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4006">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>web coords</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Website</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4007">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Astern</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Back</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4008">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Your quotes</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Your quotes</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4009">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>The following quote has been deleted.</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>The following quote has been deleted.</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4010">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>You have no quotes saved</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>You have no quotes saved</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4011">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Quote</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Quote</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4012">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Current quote</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Current quote</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4013">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Back to quotes</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Back to quotes</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4014">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Make current quote</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Make current quote</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4015">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Delete quote</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Delete quote</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4016">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>You have no orders saved</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>You have no orders saved</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4017">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Order</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Order</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4018">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>You are logged in as</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>You are logged in as</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4019">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Logout</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Logout</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template name="term4020">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Logged in as</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Logged in as</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4021">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>+ Add new contact address</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>+ Add new contact address</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4022">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Edit</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Edit</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4023">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Delete</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Delete</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4024">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Edit contact</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Edit contact</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4025">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Delete contact</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Delete contact</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

 
  <xsl:template name="term4026">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Time t'be payin'</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Duration</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4027">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Time</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Time</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="span[@class='term4028']" mode="term">
    <xsl:call-template name="term4028" />
  </xsl:template>
  
  <xsl:template name="term4028">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Logon</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Logon</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="span[@class='term4029']" mode="term">
    <xsl:call-template name="term4029" />
  </xsl:template>

  <xsl:template name="term4029">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>First name</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>First name</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4030">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Chest</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Box</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4031">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Your Address Details</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Your Address Details</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4032">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Use This Address</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Use This Address</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="span[@class='term4033']" mode="term">
    <xsl:call-template name="term4033" />
  </xsl:template>

  <xsl:template name="term4033">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Billing Address</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Billing Address</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="span[@class='term4034']" mode="term">
    <xsl:call-template name="term4034" />
  </xsl:template>

  <xsl:template name="term4034">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Delivery Address</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Delivery Address</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="span[@class='term4035']" mode="term">
    <xsl:call-template name="term4035" />
  </xsl:template>

  <xsl:template name="term4035">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Name</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Name</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

    <xsl:template match="span[@class='term4036']" mode="term">
    <xsl:call-template name="term4036" />
  </xsl:template>
  
  <xsl:template name="term4036">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Company</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Company</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="span[@class='term4037']" mode="term">
    <xsl:call-template name="term4037" />
  </xsl:template>

  <xsl:template name="term4037">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Address</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Address</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="span[@class='term4038']" mode="term">
    <xsl:call-template name="term4038" />
  </xsl:template>

  <xsl:template name="term4038">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Port</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>City</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="span[@class='term4039']" mode="term">
    <xsl:call-template name="term4039" />
  </xsl:template>

  <xsl:template name="term4039">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>County</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>County / State</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template match="span[@class='term4040']" mode="term">
    <xsl:call-template name="term4040" />
  </xsl:template>
  
  <xsl:template name="term4040">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Postcode</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Postcode</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="span[@class='term4041']" mode="term">
    <xsl:call-template name="term4041" />
  </xsl:template>

  <xsl:template name="term4041">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Country</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Country</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="span[@class='term4042']" mode="term">
    <xsl:call-template name="term4042" />
  </xsl:template>
  
  <xsl:template name="term4042">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Tel</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Tel</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="span[@class='term4043']" mode="term">
    <xsl:call-template name="term4043" />
  </xsl:template>
  
  <xsl:template name="term4043">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Fax</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Fax</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4044">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Submit</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Submit</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template name="term4045">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Please agree to our terms of business</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Please agree to our terms of business</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4046">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Continue With Purchase</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Continue With Purchase</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4047">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Select Delivery Option</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Select Delivery Option</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4048">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Confirm Order - Enter Purchase Order Reference.</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Confirm Order - Enter Purchase Order Reference.</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4049">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Purchase Order Reference</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Purchase Order Reference</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4050">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Comments</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Comments</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4051">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Please enter any comments on your order here</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Please enter any comments on your order here</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term4052">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Password Reset</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Password Reset</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <!-- ################################################################################################ -->
  <!-- SOCIAL NETWORKING!!!!  NO. 5000 - 5500-->
  <!-- ################################################################################################ -->
  
  <!-- Follow on Twitter-->
  <xsl:template name="term5000">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Stalk on't Twittaaaarrrh</xsl:text>
      </xsl:when>
      <xsl:when test="$lang='it'">
        <xsl:text>Seguiteci su Twitter</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Follow on Twitter</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term5001">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Facebook</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Facebook</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term5002">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Twitter</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Twtter</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term5003">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Linkin</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Linkin</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term5004">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Google+</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Google+</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term5005">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>pinterestURL</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>pinterestURL</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="term5006">
    <xsl:choose>
      <xsl:when test="$lang='en-pr'">
        <xsl:text>Browse...</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>Browse..</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- ################################################################################################ -->
  <!-- Language number formatting -->
  <!-- 1,000.5 - 1.000,5 -->
  <!-- ################################################################################################ -->

<!-- This template is old, need to be superceded in Common XSL's to use template below.-->
  <xsl:template match="Page" mode="formatPrice">
    <xsl:param name="price"/>
    <xsl:param name="currency"/>
  <xsl:param name="pCurrencyCode"/>
    <xsl:variable name="vCurrencyCode">
      <xsl:choose>
        <xsl:when test="pCurrencyCode!=''">
          <xsl:value-of select="$pCurrencyCode"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$currencyCode"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
      <xsl:choose>
          <xsl:when test="$lang='de' or $lang='fr' or $lang='fi' or $lang='sp' or $lang='pt'">
            <span itemprop="price" content="{format-number($price,'###,###,##0.00')}">
              <xsl:value-of select="format-number($price,'###,###,##0.00')"/>
            </span>
            <span itemprop="priceCurrency" content="{$vCurrencyCode}">
              <xsl:value-of select="$currency"/>
            </span>
          </xsl:when>
          <xsl:otherwise>
            <span itemprop="priceCurrency" content="{$vCurrencyCode}"><xsl:value-of select="$currency"/></span>
            <span itemprop="price" content="{format-number($price,'###,###,##0.00')}"><xsl:value-of select="format-number($price,'###,###,##0.00')"/></span>
          </xsl:otherwise>
      </xsl:choose>
  </xsl:template>

  <!-- -->

  <xsl:template name="formatPrice">
    <xsl:param name="price"/>
    <xsl:param name="currency"/>
    <xsl:param name="pCurrencyCode"/>
    <xsl:variable name="vCurrencyCode">
      <xsl:choose>
        <xsl:when test="pCurrencyCode!=''">
          <xsl:value-of select="$pCurrencyCode"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$currencyCode"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
      <xsl:choose>
          <xsl:when test="$lang='de' or $lang='fr' or $lang='fi' or $lang='sp' or $lang='pt'">
            <span itemprop="price" content="{format-number($price,'###,###,##0.00')}">
              <xsl:value-of select="format-number($price,'###,###,##0.00')"/>
            </span>
            <span itemprop="priceCurrency" content="{$vCurrencyCode}">
              <xsl:value-of select="$currency"/>
            </span>
          </xsl:when>
          <xsl:otherwise>
            <span itemprop="priceCurrency" content="{$vCurrencyCode}"><xsl:value-of select="$currency"/></span>
            <span itemprop="price" content="{format-number($price,'###,###,##0.00')}"><xsl:value-of select="format-number($price,'###,###,##0.00')"/></span>
          </xsl:otherwise>
      </xsl:choose>
  </xsl:template>


</xsl:stylesheet>
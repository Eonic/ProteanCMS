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
		Please enter
	</xsl:template>

	<!-- Submit Button Text -->

	<xsl:template match="label[node()='Make Secure Payment']" mode="submitText">
		<xsl:call-template name="term4046"/>
	</xsl:template>

	<xsl:template match="label[node()='Proceed']" mode="submitText">
		Proceed
	</xsl:template>

	<xsl:template match="label[node()='Continue']" mode="submitText">
		Continue
	</xsl:template>

	<xsl:template match="label[node()='Submit']" mode="submitText">
		<xsl:call-template name="term4044"/>
	</xsl:template>

	<!-- 1000 This must be a number -->
	<xsl:template match="span[@class='msg-1000']" mode="term">
		This must be a number
	</xsl:template>

	<!-- 1001 This must be a valid date -->
	<xsl:template match="span[@class='msg-1001']" mode="term">
		This must be a valid date
	</xsl:template>

	<!-- 1002 This must be a valid email address -->
	<xsl:template match="span[@class='msg-1002']" mode="term">
		This must be a valid email address
	</xsl:template>
	<!-- 1002 This must be a valid email address -->
	<xsl:template match="span[@class='msg-1002a']" mode="term">
		These must all be valid email addresses
	</xsl:template>
	<!-- 1003 Please enter the valid image code -->
	<xsl:template match="span[@class='msg-1003']" mode="term">
		<xsl:value-of select="."/>
	</xsl:template>

	<!-- 1004 Invalid File Extension -->
	<xsl:template match="span[@class='msg-1004']" mode="term">
		Invalid file extension
	</xsl:template>

	<!-- 1005 This must be a valid format -->
	<xsl:template match="span[@class='msg-1005']" mode="term">
		This must be a valid format
	</xsl:template>

	<!-- 1006 No File Selected -->
	<xsl:template match="span[@class='msg-1006']" mode="term">
		No file selected
	</xsl:template>

	<!-- 1007 This must be completed -->
	<xsl:template match="span[@class='msg-1007']" mode="term">
		This must be completed
	</xsl:template>

	<!-- 1008 This information must be valid -->
	<xsl:template match="span[@class='msg-1008']" mode="term">
		<xsl:value-of select="span[@class='labelName']/node()"/>
		This information must be valid
	</xsl:template>

	<!-- 1009 The file you are uploading is too large -->
	<xsl:template match="span[@class='msg-1009']" mode="term">
		The file you are uploading is too large
	</xsl:template>

	<!-- 1010 Your details have been updated -->
	<xsl:template match="span[@class='msg-1010']" mode="term">
		Your details have been updated.
	</xsl:template>

	<!-- 1011 Your password has been emailed to you -->
	<xsl:template match="span[@class='msg-1011']" mode="term">
		Your password has been emailed to you
	</xsl:template>

	<!-- 1012 User account is not active -->
	<xsl:template match="span[@class='msg-1012']" mode="term">
		User account is not active
	</xsl:template>

	<!-- 1013 User account has been disabled -->
	<xsl:template match="span[@class='msg-1013']" mode="term">
		User account has been disabled
	</xsl:template>

	<!-- 1014 The Password is not valid -->
	<xsl:template match="span[@class='msg-1014']" mode="term">
		The password is not valid
	</xsl:template>

	<!-- 1015 The username was not found -->
	<xsl:template match="span[@class='msg-1015']" mode="term">
		These credentials do not match a valid account
	</xsl:template>

	<!-- 1016 User account has expired -->
	<xsl:template match="span[@class='msg-1016']" mode="term">
		User account has expired
	</xsl:template>

	<!-- 1017 Passwords must match -->
	<xsl:template match="span[@class='msg-1017']" mode="term">
		Passwords must match
	</xsl:template>

	<!-- 1018 Passwords must be 4 characters long -->
	<xsl:template match="span[@class='msg-1018']" mode="term">
		Passwords must be 4 characters long
	</xsl:template>

	<!-- 1019 Registration Code Incorrect -->
	<xsl:template match="span[@class='msg-1019']" mode="term">
		Registration code incorrect
	</xsl:template>

	<!-- 1020 This user has been added -->
	<xsl:template match="span[@class='msg-1020']" mode="term">
		This user has been added
	</xsl:template>

	<!-- 1020 This user has been added -->
	<xsl:template match="span[@class='msg-1029']" mode="term">
		Thanks for registering you have been sent an email with a link you must click to activate your account
	</xsl:template>

	<!-- 1030 The code you have provided is invalid for this transaction -->
	<xsl:template match="span[@class='msg-1030']" mode="term">
		The code you have provided is invalid for this transaction
	</xsl:template>

	<!-- 1030 The code you have provided is invalid for this transaction -->
	<xsl:template match="span[@class='msg-1031']" mode="term">
		This email address already has an account, please use password reminder facility
	</xsl:template>

	<!-- 1007 This must be completed -->
	<xsl:template match="span[@class='msg-1032']" mode="term">
		Please confirm you are not a robot
	</xsl:template>

	<!-- 1007 This must be completed -->
	<xsl:template match="span[@class='msg-1033']" mode="term">
		<xsl:value-of select="span[@class='labelName']/node()"/>
		The file you are uploading is too large
	</xsl:template>

	<!-- 1007 This must be completed -->
	<xsl:template match="span[@class='msg-1034']" mode="term">
		<xsl:value-of select="span[@class='labelName']/node()"/>
		Must be unique
	</xsl:template>

	<!-- 1007 This must be completed -->
	<xsl:template match="span[@class='msg-1035']" mode="term">
		<xsl:value-of select="span[@class='labelName']/node()"/>
		This information must be valid
	</xsl:template>


	<!-- 1021 This username already exists in <membership>. Please select another. -->
	<xsl:template name="term1021">
		<xsl:param name="p1"/>
		<xsl:text>This username already exists in </xsl:text>
		<xsl:value-of select="$p1"/>
		<xsl:text>. Please select another.</xsl:text>
	</xsl:template>

	<!-- 1022 This username already exists. Please select another. -->
	<xsl:template name="term1022">
		This username already exists. Please select another.
	</xsl:template>

	<!-- 1023 Required input -->
	<xsl:template name="term1023">
		required input
	</xsl:template>

	<!-- 1022 This username already exists. Please select another. -->
	<xsl:template name="term1024">
		This email address already has an account, please use password reminder facility.
	</xsl:template>

	<!-- ################################################################################################ -->
	<!-- EonicWeb Common Template phrases -->
	<!-- 2000+ -->
	<!-- ################################################################################################ -->

	<!-- 2000 Error Messages. -->
	<xsl:template name="term2000">
		The template cannot be found
	</xsl:template>

	<xsl:template name="term2001">
		The template
	</xsl:template>

	<xsl:template name="term2002">
		is not available in the XSLT.
	</xsl:template>

	<xsl:template name="term2003">
		<h2>Uh oh, something went wrong!</h2>
		<h3>We need your help in reporting this.</h3>
		<p>Please contact the owner of this website and let them know you have experienced an error.</p>
		<p>Any information on how you arrived at this page would be much appreciated.</p>
		<a href="javascript:history.back();">Click here to return to the previous page.</a>
	</xsl:template>

	<xsl:template name="term2004">
		Get Flash Player
	</xsl:template>

	<xsl:template name="term2005">
		to see this video.
	</xsl:template>

	<xsl:template name="term2006">
		Click here to return to the news article list
	</xsl:template>

	<xsl:template match="span[@class='term2007']" mode="term">
		<xsl:call-template name="term2007" />
	</xsl:template>

	<xsl:template name="term2007">
		Tel
	</xsl:template>

	<xsl:template name="term2008">
		<xsl:text>Fax</xsl:text>
	</xsl:template>

	<xsl:template name="term2009">
		<xsl:text>Email</xsl:text>
	</xsl:template>

	<xsl:template name="term2010">
		<xsl:text>Website</xsl:text>
	</xsl:template>

	<xsl:template name="term2011">
		<xsl:text>Department</xsl:text>
	</xsl:template>

	<xsl:template name="term2012">
		<xsl:text>Click here to return to the contact list</xsl:text>
	</xsl:template>

	<xsl:template name="term2013">
		<xsl:text>Click here to return to the event list</xsl:text>
	</xsl:template>

	<xsl:template name="term2014">
		<xsl:text>Stock code</xsl:text>
	</xsl:template>

	<xsl:template name="term2015">
		<xsl:text>Click here to return to the product list</xsl:text>
	</xsl:template>

	<xsl:template name="term2016">
		<xsl:text>Reviews</xsl:text>
	</xsl:template>

	<xsl:template name="term2017">
		<xsl:text>RRP</xsl:text>
	</xsl:template>

	<xsl:template name="term2018">
		<xsl:text>Reviewed by</xsl:text>
	</xsl:template>

	<xsl:template name="term2019">
		<xsl:text>on</xsl:text>
	</xsl:template>

	<xsl:template name="term2020">
		<xsl:text>Rating</xsl:text>
	</xsl:template>

	<xsl:template name="term2021">
		<xsl:text>stars</xsl:text>
	</xsl:template>

	<xsl:template name="term2022">
		<xsl:text>Back to list</xsl:text>
	</xsl:template>

	<xsl:template name="term2023">
		<xsl:text>about</xsl:text>
	</xsl:template>

	<xsl:template name="term2024">
		<xsl:text>Website by</xsl:text>
	</xsl:template>

	<xsl:template name="term2025">
		<xsl:text>Eonic</xsl:text>
	</xsl:template>

	<xsl:template name="term2026">
		<xsl:text>Go to</xsl:text>
	</xsl:template>

	<xsl:template name="term2027">
		<xsl:text>Click here to download a copy of this document</xsl:text>
	</xsl:template>

	<xsl:template name="term2027a">
		<xsl:text>Click here to view this document</xsl:text>
	</xsl:template>

	<xsl:template name="term2028">
		<xsl:text>Download</xsl:text>
	</xsl:template>

	<xsl:template name="term2029">
		<xsl:text>Adobe PDF</xsl:text>
	</xsl:template>

	<xsl:template name="term2030">
		<xsl:text>Word document</xsl:text>
	</xsl:template>

	<xsl:template name="term2031">
		<xsl:text>Excel spreadsheet</xsl:text>
	</xsl:template>

	<xsl:template name="term2032">
		<xsl:text>Zip archive</xsl:text>
	</xsl:template>

	<xsl:template name="term2033">
		<xsl:text>PowerPoint presentation</xsl:text>
	</xsl:template>

	<xsl:template name="term2034">
		<xsl:text>PowerPoint slideshow</xsl:text>
	</xsl:template>

	<xsl:template name="term2034a">
		<xsl:text>Access database</xsl:text>
	</xsl:template>

	<xsl:template name="term2035">
		<xsl:text>JPEG image file</xsl:text>
	</xsl:template>

	<xsl:template name="term2036">
		<xsl:text>GIF image file</xsl:text>
	</xsl:template>

	<xsl:template name="term2037">
		<xsl:text>PNG image file</xsl:text>
	</xsl:template>

	<xsl:template name="term2038">
		<xsl:text>unknown file type</xsl:text>
	</xsl:template>

	<xsl:template name="term2039">
		<xsl:text>Tags</xsl:text>
	</xsl:template>

	<xsl:template name="term2040">
		<xsl:text>Click here to return to the tags list</xsl:text>
	</xsl:template>

	<xsl:template name="term2041">
		<xsl:text>Testimonial from</xsl:text>
	</xsl:template>

	<xsl:template name="term2042">
		<xsl:text>Read more </xsl:text>
	</xsl:template>

	<xsl:template name="term2043">
		<xsl:text>Click here to return to the testimonial list</xsl:text>
	</xsl:template>

	<xsl:template name="term2044">
		<a href="http://www.macromedia.com/go/getflashplayer">
			<xsl:text>Get Flash Player</xsl:text>
		</a>
		<xsl:text>to see this video.</xsl:text>
	</xsl:template>

	<xsl:template name="term2045">
		<xsl:text>Author</xsl:text>
	</xsl:template>

	<xsl:template name="term2046">
		<xsl:text>Copyright</xsl:text>
	</xsl:template>

	<xsl:template name="term2047">
		<xsl:text>Click here to return to the video list</xsl:text>
	</xsl:template>

	<xsl:template name="term2048">
		<xsl:text>Click here to return to the contact list</xsl:text>
	</xsl:template>

	<xsl:template name="term2049">
		<xsl:text>Click to enlarge</xsl:text>
	</xsl:template>

	<xsl:template name="term2050">
		<xsl:text>Email</xsl:text>
	</xsl:template>

	<xsl:template name="term2051">
		<xsl:text>Show results</xsl:text>
	</xsl:template>

	<xsl:template name="term2052">
		<xsl:text>This poll opens for voting at the beginning of</xsl:text>
	</xsl:template>

	<xsl:template name="term2053">
		<xsl:text>The results to this poll are private.</xsl:text>
	</xsl:template>

	<xsl:template name="term2054">
		<xsl:text>The results to this poll will be revealed</xsl:text>
	</xsl:template>

	<xsl:template name="term2055">
		<xsl:text>at the end of</xsl:text>
	</xsl:template>

	<xsl:template name="term2056">
		<xsl:text>when the poll closes.</xsl:text>
	</xsl:template>

	<xsl:template name="term2057">
		<xsl:text>You have already voted on this poll.</xsl:text>
	</xsl:template>

	<xsl:template name="term2058">
		<xsl:text>This poll is only available to registered users</xsl:text>
	</xsl:template>

	<xsl:template name="term2059">
		<xsl:text>Thank you, your vote has been counted.</xsl:text>
	</xsl:template>

	<xsl:template name="term2060">
		<xsl:text>Total votes</xsl:text>
	</xsl:template>

	<xsl:template name="term2061">
		<xsl:text>Click here to return to the feed list</xsl:text>
	</xsl:template>

	<xsl:template name="term2062">
		<xsl:text>Added on</xsl:text>
	</xsl:template>

	<xsl:template name="term2063">
		<xsl:text>Contract type</xsl:text>
	</xsl:template>

	<xsl:template name="term2064">
		<xsl:text>Ref</xsl:text>
	</xsl:template>

	<xsl:template name="term2065">
		<xsl:text>Location</xsl:text>
	</xsl:template>

	<xsl:template name="term2066">
		<xsl:text>Salary</xsl:text>
	</xsl:template>

	<xsl:template name="term2067">
		<xsl:text>Application Deadline</xsl:text>
	</xsl:template>

	<xsl:template name="term2068">
		<xsl:text>Added on</xsl:text>
	</xsl:template>

	<xsl:template name="term2069">
		<xsl:text>Reference</xsl:text>
	</xsl:template>

	<xsl:template name="term2070">
		<xsl:text>Contract type</xsl:text>
	</xsl:template>

	<xsl:template name="term2071">
		<xsl:text>Click here to return to the news article list</xsl:text>
	</xsl:template>

	<xsl:template name="term2072">
		<xsl:text>Click here to read more information about</xsl:text>
	</xsl:template>

	<xsl:template name="term2073">
		<xsl:text>Image gallery</xsl:text>
	</xsl:template>

	<xsl:template name="term2074">
		<xsl:text>Download all in a Zip file</xsl:text>
	</xsl:template>


	<!--  ==  Recipes  =============================================================================  -->
	<xsl:template name="term2075">
		<xsl:text>Ingredients</xsl:text>
	</xsl:template>

	<xsl:template name="term2075a">
		<xsl:text>No. of servings</xsl:text>
	</xsl:template>

	<xsl:template name="term2076">
		<xsl:text>Preparation time</xsl:text>
	</xsl:template>

	<xsl:template name="term2077">
		<xsl:text>Cooking time</xsl:text>
	</xsl:template>

	<xsl:template name="term2078">
		<xsl:text>Method</xsl:text>
	</xsl:template>

	<xsl:template name="term2079">
		<xsl:text>Published</xsl:text>
	</xsl:template>

	<xsl:template match="span[@class='term2080']" mode="term">
		<xsl:call-template name="term2080" />
	</xsl:template>

	<xsl:template name="term2080">
		<xsl:text>Mobile</xsl:text>
	</xsl:template>


	<xsl:template name="term2081">
		<xsl:text>Manufacturer</xsl:text>
	</xsl:template>

	<xsl:template name="term2082">
		<xsl:text>Size</xsl:text>
	</xsl:template>

	<xsl:template name="term2083">
		<xsl:text>Brand</xsl:text>
	</xsl:template>

	<!--  ==  Job Vacancies =============================================================================  -->

	<xsl:template name="term2084">
		<xsl:text>Responsibilities</xsl:text>
	</xsl:template>

	<xsl:template name="term2085">
		<xsl:text>Industry</xsl:text>
	</xsl:template>

	<xsl:template name="term2086">
		<xsl:text>Occupation</xsl:text>
	</xsl:template>

	<xsl:template name="term2087">
		<xsl:text>Base Salary</xsl:text>
	</xsl:template>

	<xsl:template name="term2088">
		<xsl:text>Currency</xsl:text>
	</xsl:template>

	<xsl:template name="term2089">
		<xsl:text>Figure</xsl:text>
	</xsl:template>

	<xsl:template name="term2090">
		<xsl:text>Work Hours</xsl:text>
	</xsl:template>

	<xsl:template name="term2092">
		<xsl:text>Description</xsl:text>
	</xsl:template>

	<xsl:template name="term2093">
		<xsl:text>Responsibities</xsl:text>
	</xsl:template>

	<xsl:template name="term2094">
		<xsl:text>Skills</xsl:text>
	</xsl:template>

	<xsl:template name="term2095">
		<xsl:text>Education Requirements</xsl:text>
	</xsl:template>

	<xsl:template name="term2096">
		<xsl:text>Experience Requirements</xsl:text>
	</xsl:template>

	<xsl:template name="term2097">
		<xsl:text>Qualifications</xsl:text>
	</xsl:template>

	<xsl:template name="term2098">
		<xsl:text>Incentives</xsl:text>
	</xsl:template>

	<xsl:template name="term2099">
		<xsl:text>Previous</xsl:text>
	</xsl:template>

	<xsl:template name="term2100">
		<xsl:text>Next</xsl:text>
	</xsl:template>

	<xsl:template name="term2100a">
		<xsl:text>Find out more</xsl:text>
	</xsl:template>

	<!--Organisation-->

	<xsl:template name="term2101">
		<xsl:text>Profile</xsl:text>
	</xsl:template>

	<xsl:template name="term2102">
		<xsl:text>Type of Organization </xsl:text>
	</xsl:template>

	<xsl:template name="term2103">
		<xsl:text>Legal Name </xsl:text>
	</xsl:template>

	<xsl:template name="term2104">
		<xsl:text>Founding Date </xsl:text>
	</xsl:template>

	<xsl:template name="term2105">
		<xsl:text>Latitude </xsl:text>
	</xsl:template>

	<xsl:template name="term2106">
		<xsl:text>Longitude </xsl:text>
	</xsl:template>

	<xsl:template name="term2107">
		<xsl:text>Dun &amp; Bradstreet Number </xsl:text>
	</xsl:template>

	<xsl:template name="term2108">
		<xsl:text>Tax ID </xsl:text>
	</xsl:template>

	<xsl:template name="term2109">
		<xsl:text>Vax ID </xsl:text>
	</xsl:template>

	<xsl:template name="term2110">
		<xsl:text> Currencies Accepted</xsl:text>
	</xsl:template>

	<xsl:template name="term2111">
		<xsl:text>Opening Hours </xsl:text>
	</xsl:template>

	<xsl:template name="term2112">
		<xsl:text>Payment Accepted </xsl:text>
	</xsl:template>

	<xsl:template name="term2113">
		<xsl:text>Price Range </xsl:text>
	</xsl:template>

	<xsl:template name="term2114">
		<xsl:text>Address </xsl:text>
	</xsl:template>

	<!--Training Course Module -->

	<xsl:template name="term2115">
		<xsl:text>Next Course</xsl:text>
	</xsl:template>

	<xsl:template name="term2116">
		<xsl:text> Ticket Cost</xsl:text>
	</xsl:template>

	<xsl:template name="term2117">
		<xsl:text>Place Order</xsl:text>
	</xsl:template>


	<!-- ################################################################################################ -->
	<!-- EonicWeb Cart Template phrases -->
	<!-- 3000+ -->
	<!-- ################################################################################################ -->

	<xsl:template name="term3001">
		<xsl:text>Your Shopping Cart</xsl:text>
	</xsl:template>

	<xsl:template name="term3002">
		<xsl:text>Items</xsl:text>
	</xsl:template>

	<xsl:template name="term3003">
		<xsl:text>Click here to view full details of your shopping cart</xsl:text>
	</xsl:template>

	<xsl:template name="term3004">
		<xsl:text>Show details</xsl:text>
	</xsl:template>

	<xsl:template name="term3005a">
		<xsl:text>Your basket is empty</xsl:text>
	</xsl:template>

	<xsl:template name="term3005">
		<xsl:text>The order has timed out and cannot continue</xsl:text>
	</xsl:template>

	<xsl:template name="term3006">
		<xsl:text>Proceed</xsl:text>
	</xsl:template>

	<xsl:template name="term3007">
		<xsl:text>Your Order</xsl:text>
	</xsl:template>

	<xsl:template name="term3008">
		<xsl:text>Your Order - Please enter a discount code</xsl:text>
	</xsl:template>

	<xsl:template name="term3009">
		<xsl:text>Your Order - Please tell us any special requirements</xsl:text>
	</xsl:template>

	<xsl:template name="term3010">
		<xsl:text>Additional information for your order</xsl:text>
	</xsl:template>

	<xsl:template name="term3011">
		<xsl:text>Promotional code entered</xsl:text>
	</xsl:template>

	<xsl:template name="term3012">
		<xsl:text>Click here to edit the notes on this order.</xsl:text>
	</xsl:template>

	<xsl:template name="term3013">
		<xsl:text>Edit Notes</xsl:text>
	</xsl:template>

	<xsl:template name="term3014">
		<xsl:text>Registration</xsl:text>
	</xsl:template>

	<xsl:template name="term3015">
		<xsl:text>Proceed without registering</xsl:text>
	</xsl:template>

	<xsl:template name="term3016">
		<xsl:text>Currency selection</xsl:text>
	</xsl:template>

	<xsl:template name="term3017">
		<xsl:text>Enter your billing details</xsl:text>
	</xsl:template>

	<xsl:template name="term3018">
		<xsl:text>Enter the delivery address</xsl:text>
	</xsl:template>

	<xsl:template name="term3019">
		<xsl:text>Please enter your preferred payment and shipping methods.</xsl:text>
	</xsl:template>

	<xsl:template name="term3020">
		<xsl:text>Please enter your payment details.</xsl:text>
	</xsl:template>

	<xsl:template name="term3020-1">
		<xsl:text>3D Secure - Please Validate Your Card.</xsl:text>
	</xsl:template>

	<xsl:template name="term3021">
		<xsl:text>Your Invoice - Thank you for your order.</xsl:text>
	</xsl:template>

	<xsl:template name="term3022">
		<xsl:text>Invoice Date</xsl:text>
	</xsl:template>

	<xsl:template name="term3023">
		<xsl:text>Invoice Reference</xsl:text>
	</xsl:template>

	<xsl:template name="term3024">
		<xsl:text>Payment received</xsl:text>
	</xsl:template>

	<xsl:template name="term3025">
		<xsl:text>Final Payment Reference</xsl:text>
	</xsl:template>

	<xsl:template name="term3026">
		<p>
			Thank you for your deposit. To pay the outstanding balance, please note your Final Payment Reference, above.  <em>Instructions on paying the outstanding balance have been e-mailed to you.</em>
		</p>
		<p>
			If you have any queries, please call for assistance.
		</p>
	</xsl:template>

	<xsl:template name="term3027">
		<xsl:text>Payment made</xsl:text>
	</xsl:template>

	<xsl:template name="term3028">
		<xsl:text>Total Payment Received</xsl:text>
	</xsl:template>

	<xsl:template name="term3029">
		<xsl:text>paid in full</xsl:text>
	</xsl:template>

	<xsl:template name="term3030a">
		Please add items to your shopping basket
	</xsl:template>

	<xsl:template name="term3030">
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
	</xsl:template>

	<xsl:template name="term3031">
		The item(s) you are trying to add cannot be added to this shopping basket.<br/><br/>
		Please proceed to the checkout and pay for the items in the basket, and then continue with your shopping.
	</xsl:template>

	<xsl:template name="term3032">
		There is no valid delivery option for this order.  This may be due to a combination of location, price, weight or quantity.<br/><br/>
		Please call for assistance.
	</xsl:template>

	<xsl:template name="term3033">
		The transaction was cancelled during the payment processing - this was either at your request or the request of our payment provider, Worldpay.<br/><br/>
		Please call for more information.
	</xsl:template>

	<xsl:template name="term3034">
		The order reference could not be found, or the order did not have the correct status.  This may occur if you have tried to pay for the same order twice, or if there has been a long duration between visiting our payment provider, Worldpay's site and entering payment details.<br/><br/>
		Please call for assistance.
	</xsl:template>

	<xsl:template name="term3035">
		The payment provider, Worldpay, did not provide a valid response.<br/><br/>
		Please call for assistance.
	</xsl:template>

	<xsl:template name="term3036">
		<xsl:text>Delivery Address Details</xsl:text>
	</xsl:template>

	<xsl:template name="term3037">
		<xsl:text>Your order will be delivered to</xsl:text>
	</xsl:template>

	<xsl:template name="term3038">
		Click on this icon to remove an item from the order.<br/><br/>
		If you amend the quantity of items please click 'Update Order' before continuing to browse.
	</xsl:template>

	<xsl:template name="term3038b">
		If you amend the quantity of items please click 'Update Order' before continuing to browse.
	</xsl:template>

	<xsl:template name="term3039">
		<xsl:text>Qty</xsl:text>
	</xsl:template>

	<xsl:template name="term3040">
		<xsl:text>Description</xsl:text>
	</xsl:template>

	<xsl:template name="term3041">
		<xsl:text>Ref</xsl:text>
	</xsl:template>

	<xsl:template name="term3042">
		<xsl:text>Price</xsl:text>
	</xsl:template>

	<xsl:template name="term3042a">
		<xsl:text>VAT</xsl:text>
	</xsl:template>

	<xsl:template name="term3043">
		<xsl:text>Line Total</xsl:text>
	</xsl:template>

	<xsl:template name="term3044">
		<xsl:text>Shipping Cost</xsl:text>
	</xsl:template>

	<xsl:template name="term3045">
		<xsl:text>Sub Total</xsl:text>
	</xsl:template>

	<xsl:template name="term3046">
		<xsl:text>VAT at</xsl:text>
	</xsl:template>

	<xsl:template name="term3047">
		<xsl:text>Tax at</xsl:text>
	</xsl:template>

	<xsl:template name="term3048a">
		<xsl:text>Total</xsl:text>
	</xsl:template>

	<xsl:template name="term3048b">
		<xsl:text>Total Items</xsl:text>
	</xsl:template>

	<xsl:template name="term3048c">
		<xsl:text>Total Ex Tax</xsl:text>
	</xsl:template>

	<xsl:template name="term3048d">
		<xsl:text>Tax at</xsl:text>
	</xsl:template>

	<xsl:template name="term3048">
		<xsl:text>Total Value</xsl:text>
	</xsl:template>

	<xsl:template name="term3049">
		<xsl:text>Payment Made</xsl:text>
	</xsl:template>

	<xsl:template name="term3050">
		<xsl:text>Payment Received</xsl:text>
	</xsl:template>

	<xsl:template name="term3051">
		<xsl:text>Deposit Payable</xsl:text>
	</xsl:template>

	<xsl:template name="term3051a">
		<xsl:text>Deposit Paid</xsl:text>
	</xsl:template>

	<xsl:template name="term3052">
		<xsl:text>Amount Outstanding</xsl:text>
	</xsl:template>

	<xsl:template name="term3053">
		<xsl:text>RRP</xsl:text>
	</xsl:template>

	<xsl:template name="term3054">
		<xsl:text>save</xsl:text>
	</xsl:template>

	<xsl:template name="term3055">
		<xsl:text>Qty</xsl:text>
	</xsl:template>

	<xsl:template name="term3056">
		<xsl:text>Add to Gift List</xsl:text>
	</xsl:template>

	<xsl:template name="term3057">
		<xsl:text>Add to Quote</xsl:text>
	</xsl:template>

	<xsl:template name="term3058">
		<xsl:text>Add to Cart</xsl:text>
	</xsl:template>

	<xsl:template name="term3059">
		<xsl:text>Go To Checkout</xsl:text>
	</xsl:template>

	<xsl:template name="term3060">
		<xsl:text>Continue Shopping</xsl:text>
	</xsl:template>

	<xsl:template name="term3061">
		<xsl:text>Update Order</xsl:text>
	</xsl:template>

	<xsl:template name="term3062">
		<xsl:text>Empty Order</xsl:text>
	</xsl:template>

	<xsl:template name="term3063">
		<xsl:text>Booking Fee</xsl:text>
	</xsl:template>

	<xsl:template name="term3064">
		<xsl:text>To order, please enter the quantities you require.</xsl:text>
	</xsl:template>

	<xsl:template name="term3065">
		<xsl:text>This form lists product options for this page</xsl:text>
	</xsl:template>

	<xsl:template name="term3066">
		<xsl:text>Renew</xsl:text>
	</xsl:template>

	<xsl:template name="term3067">
		<xsl:text>Change to</xsl:text>
	</xsl:template>

	<xsl:template name="term3068">
		<xsl:text>Buy</xsl:text>
	</xsl:template>

	<xsl:template name="term3069">
		<xsl:text>Per</xsl:text>
	</xsl:template>

	<xsl:template name="term3070">
		<xsl:text>Details</xsl:text>
	</xsl:template>

	<xsl:template name="term3071">
		<xsl:text>Tel</xsl:text>
	</xsl:template>

	<xsl:template name="term3072">
		<xsl:text>Fax</xsl:text>
	</xsl:template>

	<xsl:template name="term3073">
		<xsl:text>Email</xsl:text>
	</xsl:template>

	<xsl:template name="term3074">
		<xsl:text>Your comments sent with this order</xsl:text>
	</xsl:template>

	<xsl:template name="term3075">
		<xsl:text>Details</xsl:text>
	</xsl:template>

	<xsl:template name="term3076">
		<xsl:text>Terms and Conditions</xsl:text>
	</xsl:template>

	<xsl:template name="term3077">
		<xsl:text>Available On</xsl:text>
	</xsl:template>

	<xsl:template name="term3078">
		<xsl:text>Close Invoice and Return to Site</xsl:text>
	</xsl:template>

	<xsl:template name="term3079">
		<xsl:text>Instrument</xsl:text>
	</xsl:template>

	<xsl:template name="term3080">
		<span>
			You have requested more items than are currently <em>in stock</em> for <strong></strong> (only <span class="quantity-available="></span> available).
		</span>
		<br/>
	</xsl:template>

	<xsl:template name="term3081">
		<xsl:text>Please adjust the quantities you require, or call for assistance.</xsl:text>
	</xsl:template>

	<xsl:template name="term3082">
		<xsl:text>Show Order Details</xsl:text>
	</xsl:template>

	<xsl:template name="term3083">
		<xsl:text>Update</xsl:text>
	</xsl:template>

	<xsl:template name="term3084">
		<xsl:text>Delivery Method</xsl:text>
	</xsl:template>

	<xsl:template name="term3085">
		<xsl:text>I agree to the Terms and Conditions</xsl:text>
	</xsl:template>

	<xsl:template name="term3086">
		<xsl:text>(Cookies disabled)</xsl:text>
	</xsl:template>

	<xsl:template name="term3087">
		<xsl:text>This may be due to some of the following reasons</xsl:text>
	</xsl:template>

	<xsl:template name="term3088">
		<xsl:text>You may have disabled cookies or they are undetectable. The shopping cart requires cookies to be enabled in order to proceed.</xsl:text>
	</xsl:template>

	<xsl:template name="term3089">
		<xsl:text>The order had been left for over ten minutes without any updates. The details are automatically removed for security purposes.</xsl:text>
	</xsl:template>

	<xsl:template name="term3090">
		<xsl:text>No translation has been made.</xsl:text>
	</xsl:template>

	<xsl:template name="term3091">
		<xsl:text>There is no valid delivery option for this order. This may be due to a combination of location, price, weight or quantity.</xsl:text>
	</xsl:template>

	<xsl:template name="term3092">
		<xsl:text>Please call for assistance.</xsl:text>
	</xsl:template>

	<xsl:template name="term3093">
		<xsl:text>Back to Home Page</xsl:text>
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
		<xsl:text>Username</xsl:text>
	</xsl:template>

	<xsl:template match="span[@class='term4001']" mode="term">
		<xsl:call-template name="term4001" />
	</xsl:template>

	<xsl:template name="term4001">
		<xsl:text>Password</xsl:text>
	</xsl:template>

	<xsl:template match="span[@class='term4002']" mode="term">
		<xsl:call-template name="term4002" />
	</xsl:template>

	<xsl:template name="term4002">
		<xsl:text>Last name</xsl:text>
	</xsl:template>

	<xsl:template match="span[@class='term4003']" mode="term">
		<xsl:call-template name="term4003" />
	</xsl:template>

	<xsl:template name="term4003">
		<xsl:text>Company</xsl:text>
	</xsl:template>

	<xsl:template match="span[@class='term4004']" mode="term">
		<xsl:call-template name="term4004" />
	</xsl:template>

	<xsl:template name="term4004">
		<xsl:text>Position</xsl:text>
	</xsl:template>

	<xsl:template match="span[@class='term4005']" mode="term">
		<xsl:call-template name="term4005" />
	</xsl:template>

	<xsl:template name="term4005">
		<xsl:text>Email</xsl:text>
	</xsl:template>

	<xsl:template name="term4006">
		<xsl:text>Website</xsl:text>
	</xsl:template>

	<xsl:template name="term4007">
		<xsl:text>Back</xsl:text>
	</xsl:template>

	<xsl:template name="term4008">
		<xsl:text>Your quotes</xsl:text>
	</xsl:template>

	<xsl:template name="term4009">
		<xsl:text>The following quote has been deleted.</xsl:text>
	</xsl:template>

	<xsl:template name="term4010">
		<xsl:text>You have no quotes saved</xsl:text>
	</xsl:template>

	<xsl:template name="term4011">
		<xsl:text>Quote</xsl:text>
	</xsl:template>

	<xsl:template name="term4012">
		<xsl:text>Current quote</xsl:text>
	</xsl:template>

	<xsl:template name="term4013">
		<xsl:text>Back to quotes</xsl:text>
	</xsl:template>

	<xsl:template name="term4014">
		<xsl:text>Make current quote</xsl:text>
	</xsl:template>

	<xsl:template name="term4015">
		<xsl:text>Delete quote</xsl:text>
	</xsl:template>

	<xsl:template name="term4016">
		<xsl:text>You have no orders saved</xsl:text>
	</xsl:template>

	<xsl:template name="term4017">
		<xsl:text>Order</xsl:text>
	</xsl:template>

	<xsl:template name="term4018">
		<xsl:text>You are logged in as</xsl:text>
	</xsl:template>

	<xsl:template name="term4019">
		<xsl:text>Logout</xsl:text>
	</xsl:template>

	<xsl:template name="term4020">
		<xsl:text>Logged in as</xsl:text>
	</xsl:template>

	<xsl:template name="term4021">
		<xsl:text>+ Add new contact address</xsl:text>
	</xsl:template>

	<xsl:template name="term4022">
		<xsl:text>Edit</xsl:text>
	</xsl:template>

	<xsl:template name="term4023">
		<xsl:text>Delete</xsl:text>
	</xsl:template>

	<xsl:template name="term4024">
		<xsl:text>Edit contact</xsl:text>
	</xsl:template>

	<xsl:template name="term4025">
		<xsl:text>Delete contact</xsl:text>
	</xsl:template>

	<xsl:template name="term4026">
		<xsl:text>Duration</xsl:text>
	</xsl:template>

	<xsl:template name="term4027">
		<xsl:text>Time</xsl:text>
	</xsl:template>

	<xsl:template match="span[@class='term4028']" mode="term">
		<xsl:call-template name="term4028" />
	</xsl:template>

	<xsl:template name="term4028">
		<xsl:text>Logon</xsl:text>
	</xsl:template>

	<xsl:template match="span[@class='term4029']" mode="term">
		<xsl:call-template name="term4029" />
	</xsl:template>

	<xsl:template name="term4029">
		<xsl:text>First name</xsl:text>
	</xsl:template>

	<xsl:template name="term4030">
		<xsl:text>Box</xsl:text>
	</xsl:template>

	<xsl:template name="term4031">
		<xsl:text>Your Address Details</xsl:text>
	</xsl:template>

	<xsl:template name="term4032">
		<xsl:text>Use This Address</xsl:text>
	</xsl:template>

	<xsl:template match="span[@class='term4033']" mode="term">
		<xsl:call-template name="term4033" />
	</xsl:template>

	<xsl:template name="term4033">
		<xsl:text>Billing Address</xsl:text>
	</xsl:template>

	<xsl:template match="span[@class='term4034']" mode="term">
		<xsl:call-template name="term4034" />
	</xsl:template>

	<xsl:template name="term4034">
		<xsl:text>Delivery Address</xsl:text>
	</xsl:template>

	<xsl:template match="span[@class='term4035']" mode="term">
		<xsl:call-template name="term4035" />
	</xsl:template>

	<xsl:template name="term4035">
		<xsl:text>Name</xsl:text>
	</xsl:template>

	<xsl:template match="span[@class='term4036']" mode="term">
		<xsl:call-template name="term4036" />
	</xsl:template>

	<xsl:template name="term4036">
		<xsl:text>Company</xsl:text>
	</xsl:template>

	<xsl:template match="span[@class='term4037']" mode="term">
		<xsl:call-template name="term4037" />
	</xsl:template>

	<xsl:template name="term4037">
		<xsl:text>Address</xsl:text>
	</xsl:template>

	<xsl:template match="span[@class='term4038']" mode="term">
		<xsl:call-template name="term4038" />
	</xsl:template>

	<xsl:template name="term4038">
		<xsl:text>City</xsl:text>
	</xsl:template>

	<xsl:template match="span[@class='term4039']" mode="term">
		<xsl:call-template name="term4039" />
	</xsl:template>

	<xsl:template name="term4039">
		<xsl:text>County / State</xsl:text>
	</xsl:template>

	<xsl:template match="span[@class='term4040']" mode="term">
		<xsl:call-template name="term4040" />
	</xsl:template>

	<xsl:template name="term4040">
		<xsl:text>Postcode</xsl:text>
	</xsl:template>

	<xsl:template match="span[@class='term4041']" mode="term">
		<xsl:call-template name="term4041" />
	</xsl:template>

	<xsl:template name="term4041">
		<xsl:text>Country</xsl:text>
	</xsl:template>

	<xsl:template match="span[@class='term4042']" mode="term">
		<xsl:call-template name="term4042" />
	</xsl:template>

	<xsl:template name="term4042">
		<xsl:text>Tel</xsl:text>
	</xsl:template>

	<xsl:template match="span[@class='term4043']" mode="term">
		<xsl:call-template name="term4043" />
	</xsl:template>

	<xsl:template name="term4043">
		<xsl:text>Fax</xsl:text>
	</xsl:template>

	<xsl:template name="term4044">
		<xsl:text>Submit</xsl:text>
	</xsl:template>

	<xsl:template name="term4045">
		<xsl:text>Please agree to our terms of business</xsl:text>
	</xsl:template>

	<xsl:template name="term4046">
		<xsl:text>Continue With Purchase</xsl:text>
	</xsl:template>

	<xsl:template name="term4047">
		<xsl:text>Select Delivery Option</xsl:text>
	</xsl:template>

	<xsl:template name="term4048">
		<xsl:text>Confirm Order - Enter Purchase Order Reference.</xsl:text>
	</xsl:template>

	<xsl:template name="term4049">
		<xsl:text>Purchase Order Reference</xsl:text>
	</xsl:template>

	<xsl:template name="term4050">
		<xsl:text>Comments</xsl:text>
	</xsl:template>

	<xsl:template name="term4051">
		<xsl:text>Please enter any comments on your order here</xsl:text>

	</xsl:template>

	<xsl:template name="term4052">
		<xsl:text>Password Reset</xsl:text>
	</xsl:template>

	<xsl:template name="term4060">
		<xsl:text>Thank You, Your payment method has been updated</xsl:text>
	</xsl:template>

	<!-- ################################################################################################ -->
	<!-- SOCIAL NETWORKING!!!!  NO. 5000 - 5500-->
	<!-- ################################################################################################ -->

	<!-- Follow on Twitter-->
	<xsl:template name="term5000">
				<xsl:text>Follow on Twitter</xsl:text>
	</xsl:template>

	<xsl:template name="term5001">
				<xsl:text>Facebook</xsl:text>
	</xsl:template>

	<xsl:template name="term5002">
				<xsl:text>Twtter</xsl:text>
	</xsl:template>

	<xsl:template name="term5003">
		<xsl:text>LinkedIn</xsl:text>
	</xsl:template>

	<xsl:template name="term5004">
					<xsl:text>Google+</xsl:text>
			</xsl:template>

	<xsl:template name="term5005">
						<xsl:text>PinterestURL</xsl:text>
			
	</xsl:template>

	<xsl:template name="term5006">
						<xsl:text>Browse..</xsl:text>
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
				<span itemprop="priceCurrency" content="{$vCurrencyCode}">
					<xsl:value-of select="$currency"/>
				</span>
				<span itemprop="price" content="{format-number($price,'###,###,##0.00')}">
					<xsl:value-of select="format-number($price,'###,###,##0.00')"/>
				</span>
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
				<span itemprop="priceCurrency" content="{$vCurrencyCode}">
					<xsl:value-of select="$currency"/>
				</span>
				<span itemprop="price" content="{format-number($price,'###,###,##0.00')}">
					<xsl:value-of select="format-number($price,'###,###,##0.00')"/>
				</span>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:template>


</xsl:stylesheet>
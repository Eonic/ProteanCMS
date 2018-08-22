# Protean CMS (formerly EonicWeb5)
.Net / XSLT based Content Management System, with eCommerce and Membership facilies.

## Project Description
Protean CMS is a extremely robust and flexible platform. Every aspect can be themed using XSLT, with a large range of content types Modules available out of the box, and a number of high quality themes ready to go. New Modules can easily be created using xForms and XSLT and .Net assemblies.

The CMS contains full eCommerce and user membership functionality.

## Background
Developed over 14 years by leading web design company eonic.co.uk it has been used on hundreds of web projects and has the capacity to cope with almost any customer requirement.

Until recently the project has been managed internally at Eonic and has only recently moved to GitHub, hence the lack of history found here.

## List of Features
http://www.proteancms.com/Why-eonicweb5/Feature-List


## Example Websites
https://www.proteancms.com/Web-Design-Portfolio

## Developers
The team welcome new developers and contributors to the platform, we will actively support and encourage those that are contributing to the platform.

## Quick Guide Getting it going locally...

Get a clone or download the files, create a new site locally in IIS and point it to the wwwroot folder in your downloaded files.

Create an Empty SQL server database

complete the following settings in eonic.web.config
```xml
  <add key="DatabaseType" value="SQL" />
  <add key="DatabaseServer" value="" />
  <add key="DatabaseAuth" value="user id=user; password=pwd" />
  <add key="DatabaseName" value="" />
  <add key="DatabaseUsername" value="user" />
  <add key="DatabasePassword" value="pwd" />
```
  and go to http://mylocalproject/ewcommon/setup and install the DB.

  Any problems email trevor@eonic.co.uk or raise an issue at
  https://github.com/Eonic/proteancms/issues

  Detailed installer guides and videos will come shortly.


## Partners
Commercial hosting, consultancy and support is available for any new requirements please go to the website.

For more information go to http://www.proteancms.com




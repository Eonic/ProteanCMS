<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" exclude-result-prefixes="#default ms dt" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ms="urn:schemas-microsoft-com:xslt" xmlns:dt="urn:schemas-microsoft-com:datatypes" xmlns="http://www.w3.org/1999/xhtml">
  <xsl:import href="../core/functions.xsl"/>
  <xsl:import href="../core/xforms.xsl"/>
  <xsl:import href="admin-xforms.xsl"/>
  <xsl:import href="admin.xsl"/>
  <xsl:import href="../core/localisation.xsl"/>

  <xsl:output method="xml" indent="yes" standalone="yes" omit-xml-declaration="yes" doctype-public="-//W3C//DTD XHTML 1.1//EN" doctype-system="http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd" encoding="UTF-8"/>

  <xsl:template match="/">
    <xsl:apply-imports/>
  </xsl:template>

  <!--<xsl:template match="Page">
		<html>
			<head>
				<title>Pick <xsl:value-of select="Request/QueryString/Item[@name='targetClass']/node()"/></title>
				<xsl:apply-templates select="." mode="metadata"/>
				<xsl:apply-templates select="." mode="style"/>
				<xsl:apply-templates select="." mode="js"/>
				<xsl:apply-templates select="." mode="adminJs"/>
			</head>
			<body class="ewAdmin" id="popupWindow">
				<div>
					<xsl:apply-templates select="." mode="Admin"/>
				</div>
			</body>
		</html>
  </xsl:template>-->

  <xsl:template match="Page">
    <div class="modal-dialog" id="popup1">
      <div class="modal-content">
        <div class="modal-body">
          <xsl:apply-templates select="." mode="Admin"/>
        </div>
      </div>
      <xsl:apply-templates select="." mode="LayoutAdminJs"/>
    </div>
  </xsl:template>

  <!--xsl:template match="Page" mode="style">
    <link rel="stylesheet" type="text/css" href="/ewcommon/css/base.css" />
    <link rel="stylesheet" type="text/css" href="/ewcommon/js/jQuery/ui/1.10.2/css/smoothness/jquery-ui-1.10.2.custom.min.css" />
    <link rel="stylesheet" type="text/css" href="/ewcommon/css/Layout/dynamiclayout.css.aspx?fullwidth=900&amp;colPad=20&amp;boxPad=15&amp;NavWidth=170" />
    <link rel="stylesheet" type="text/css" href="/ewThemes/EyecarePlans/css/eyecareplans.css" />
    <meta name="viewport" content="initial-scale=1" />
    <link rel="stylesheet" type="text/css" href="/ewThemes/EyecarePlans/css/simplemodal.css" media="screen" />
    <link type="text/css" rel="stylesheet" href="/ewcommon/css/admin/admin.css" />
    <link type="text/css" rel="stylesheet" href="/ewcommon/css/admin/skins/eonic.css" />
    <link type="text/css" rel="stylesheet" href="/ewcommon/js/jquery/treeview/jquery.treeview.css" />
    <link type="text/css" rel="stylesheet" href="/ewcommon/js/jQuery/jsScrollPane/jquery.jscrollpane.css" />
    <link type="text/css" rel="stylesheet" href="/ewcommon/js/jquery/gccolor.1.0.3/css/gccolor.css" />
  </xsl:template-->

  <xsl:template match="Page" mode="SubmitPath">
    <xsl:value-of select="$appPath"/>
    <xsl:text>?contentType=popup&amp;</xsl:text>
  </xsl:template>

  <xsl:template match="Page[@layout='ImageLib']" mode="newItemScript">
    <xsl:variable name="fld">
      <xsl:call-template name="url-encode">
        <xsl:with-param name="str">
          <xsl:call-template name="escape-js">
            <xsl:with-param name="string">
              <xsl:value-of select="Request/QueryString/Item[@name='fld']/node()"/>
            </xsl:with-param>
          </xsl:call-template>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="targetFeild">
      <xsl:call-template name="escape-js">
        <xsl:with-param name="string">
          <xsl:value-of select="/Page/Request/QueryString/Item[@name='targetField']/node()"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="callScript">
      <xsl:call-template name="escape-js">
        <xsl:with-param name="string">
          <xsl:text>javascript:&#36;('#modal-</xsl:text>
          <xsl:value-of select="$targetFeild"/>
          <xsl:text>').load('</xsl:text>
          <xsl:value-of select="$appPath"/>
          <xsl:text>?contentType=popup&amp;ewcmd=</xsl:text>
          <xsl:value-of select="$page/@ewCmd"/>
          <xsl:text>&amp;ewCmd2=pickImage&amp;fld=</xsl:text>
          <xsl:value-of select="$fld"/>
          <xsl:text>&amp;file=</xsl:text>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="callScript2">
      <xsl:call-template name="escape-js">
        <xsl:with-param name="string">
          <xsl:text>',function(e){&#36;('#modal-</xsl:text>
          <xsl:value-of select="$targetFeild"/>
          <xsl:text>').modal('show');});</xsl:text>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    function S4() {
    return Math.floor((1 + Math.random()) * 0x10000).toString(16).substring(1);
    }

    var guid = (S4() + S4() + "-" + S4() + "-4" + S4().substr(0,3) + "-" + S4() + "-" + S4() + S4() + S4()).toLowerCase();

    var newItem = '<div class="image-thumbnail">
      <div class="popoverContent" id="imgpopover' + guid + '" role="tooltip">
        <img src="' + targetPath + '/' + file.name + '" class="img-responsive" />
        <div class="popover-description">
          <span class="image-description-name">' + file.name + '</span>
          <br/>
        </div>
      </div>';
      newItem = newItem + '<a data-toggle="popover" data-trigger="hover" data-container="body" data-contentwrapper="#imgpopover' + guid + '" data-placement="top">
        <img src="' + targetPath + '/' + file.name + '" class="img-responsive" />
      </a>
    </div>';
    newItem = newItem + '<div class="description">
      ';
      newItem = newItem + '<a href="{$appPath}?contentType=popup&amp;ewcmd=ImageLib&amp;ewCmd2=pickImage&amp;fld={$fld}&amp;file=' + file.name + '" data-toggle="modal" data-target="#modal-{$targetFeild}" class="btn btn-xs btn-info">
        <i class="fa fa-picture-o fa-white">
          <xsl:text> </xsl:text>
        </i> Pick Image
      </a>';
      newItem = newItem + '
    </div><div class="img-description">
      <span class="image-description-name">' + file.name + '</span>
    </div>';
    newItem = '<div class="item item-image col-md-3 col-sm-4">
      <div class="panel">' + newItem + '</div>
    </div>';
  </xsl:template>

  <xsl:template match="Page[@layout='ImageLib' and (Request/QueryString/Item[@name='ewCmd2' and node()='PathOnly'] or Request/QueryString/Item[@name='pathOnly' and node()='true'])]" mode="newItemScript">

    <xsl:variable name="fld">
      <xsl:call-template name="url-encode">
        <xsl:with-param name="str">
          <xsl:call-template name="escape-js">
            <xsl:with-param name="string">
              <xsl:value-of select="Request/QueryString/Item[@name='fld']/node()"/>
            </xsl:with-param>
          </xsl:call-template>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="targetFeild">
      <xsl:call-template name="escape-js">
        <xsl:with-param name="string">
          <xsl:value-of select="/Page/Request/QueryString/Item[@name='targetField']/node()"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="callScript">
      <xsl:call-template name="escape-js">
        <xsl:with-param name="string">
          <xsl:text>javascript:&#36;('#modal-</xsl:text>
          <xsl:value-of select="$targetFeild"/>
          <xsl:text>').load('</xsl:text>
          <xsl:value-of select="$appPath"/>
          <xsl:text>?contentType=popup&amp;ewcmd=</xsl:text>
          <xsl:value-of select="$page/@ewCmd"/>
          <xsl:text>&amp;ewCmd2=pickImage&amp;fld=</xsl:text>
          <xsl:value-of select="$fld"/>
          <xsl:text>&amp;file=</xsl:text>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="callScript2">
      <xsl:call-template name="escape-js">
        <xsl:with-param name="string">
          <xsl:text>',function(e){&#36;('#modal-</xsl:text>
          <xsl:value-of select="$targetFeild"/>
          <xsl:text>').modal('show');});</xsl:text>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <xsl:text>var guid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {var r = Math.random()*16|0,v=c=='x'?r:r</xsl:text>
    <xsl:text disable-output-escaping="yes"><![CDATA[&]]></xsl:text>
    <xsl:text>0x3|0x8;return v.toString(16);});</xsl:text>

    var newItem = '<div class="image-thumbnail">
      <div class="popoverContent" id="imgpopover' + guid + '" role="tooltip">
        ' + file.name.replace(/\ /g,'-') + '<br />
      </div>';
      newItem = newItem + '<a data-toggle="popover" data-trigger="hover" data-container="body" data-contentwrapper="#imgpopover' + guid + '" data-placement="top">
        <img src="' + targetPath + '/' + file.name.replace(/\ /g,'-') + '" class="img-responsive" />
      </a>
    </div>';
    newItem = newItem + '<div class="description"></div>';
    newItem = newItem + '<a onclick="passImgFileToForm(\'EditContent\',\'{$targetFeild}\',\'' + targetPath + '/' + file.name.replace(/\ /g,'-') + '\');" class="btn btn-xs btn-info">
      <i class="fa fa-picture-o fa-white">
        <xsl:text> </xsl:text>
      </i> Pick Image
    </a>';
    newItem = '<div class="item item-image col-md-3 col-sm-4">
      <div class="panel">' + newItem + '</div>
    </div>';
  </xsl:template>

  <xsl:template match="Page[@layout='DocsLib']" mode="newItemScript">
    var newItem = '<tr>
      <td>
        <i class="icon-file-' + /[^.]+$/.exec(file.name) + '"> </i> ' + file.name.replace(/\ /g,'-') + '
      </td>
      <td>.' + /[^.]+$/.exec(file.name) + '</td>
      <td>
        ';
        <!--newItem = newItem + '<a onclick="passDocToForm(\'EditContent\',\'cContentDocPath\',\' + targetPath + \'/\' + file.name + \');" class="btn btn-xs btn-default" href="#">';-->
        newItem = newItem + '<a onclick="passDocToForm(\'EditContent\',\'cContentDocPath\',\'' + targetPath + '/' + file.name.replace(/\ /g,'-') + '\');" class="btn btn-xs btn-default" href="#">
          ';
          newItem = newItem + '<i class="fa fa-file-o fa-white"> </i> Pick
        </a>
      </td>
    </tr>';
  </xsl:template>

  <xsl:template match="folder" mode="FolderTree">
    <xsl:param name="level"/>
    <xsl:variable name="fld">
      <xsl:call-template name="url-encode">
        <xsl:with-param name="str">
          <xsl:value-of select="@path"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="pathonly">
      <xsl:if test="$page/Request/QueryString/Item[@name='ewCmd2' and node()='PathOnly']">
        <xsl:text>&amp;ewCmd2=PathOnly</xsl:text>
      </xsl:if>
      <xsl:if test="$page/Request/QueryString/Item[@name='pathonly' and node()='true']">
        <xsl:text>&amp;pathonly=true</xsl:text>
      </xsl:if>
    </xsl:variable>
    <li id="node{translate(@path,'\','-')}" data-tree-level="{$level}" data-tree-parent="{translate(parent::folder/@path,'\','-')}">
      <xsl:attribute name="class">
        <xsl:text>list-group-item level-</xsl:text>
        <xsl:value-of select="$level"/>
        <xsl:if test="@active='true'">
          <xsl:text> active </xsl:text>
        </xsl:if>
      </xsl:attribute>
      <a href="{$appPath}?contentType=popup&amp;ewcmd={/Page/@ewCmd}{$pathonly}&amp;fld={$fld}&amp;targetForm={/Page/Request/QueryString/Item[@name='targetForm']/node()}&amp;targetField={/Page/Request/QueryString/Item[@name='targetField']/node()}" data-toggle="modal" data-target="#modal-{/Page/Request/QueryString/Item[@name='targetField']/node()}">
        <i>
          <xsl:attribute name="class">
            <xsl:text>fa fa-lg</xsl:text>
            <xsl:choose>
              <xsl:when test="@active='true'">
                <xsl:text> fa-folder-open-o</xsl:text>
              </xsl:when>
              <xsl:otherwise>
                <xsl:text> fa-folder-o</xsl:text>
              </xsl:otherwise>
            </xsl:choose>
            <xsl:if test="folder"> activeParent</xsl:if>
          </xsl:attribute>
          &#160;
        </i>
        <xsl:value-of select="@name"/>
      </a>
    </li>

    <xsl:if test="folder">
      <xsl:apply-templates select="folder" mode="FolderTree">
        <xsl:with-param name="level">
          <xsl:value-of select="$level + 1"/>
        </xsl:with-param>
      </xsl:apply-templates>
    </xsl:if>
  </xsl:template>

  <xsl:template match="folder" mode="ImageFolder">
    <xsl:variable name="fld">
      <xsl:call-template name="url-encode">
        <xsl:with-param name="str">
          <xsl:value-of select="@path"/>
        </xsl:with-param>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="pathonly">
      <xsl:if test="$page/Request/QueryString/Item[@name='ewCmd2' and node()='PathOnly']">
        <xsl:text>&amp;ewCmd2=PathOnly</xsl:text>
      </xsl:if>
      <xsl:if test="$page/Request/QueryString/Item[@name='pathonly' and node()='true']">
        <xsl:text>&amp;pathonly=true</xsl:text>
      </xsl:if>
    </xsl:variable>

    <xsl:variable name="fileCount" select="count(file)"/>
    <xsl:variable name="itemCount" select="'24'"/>

    <xsl:variable name="startPos">
      <xsl:choose>
        <xsl:when test="/Page/Request/QueryString/Item[@name='startPos']/node()!=''">
          <xsl:value-of select="/Page/Request/QueryString/Item[@name='startPos']/node()"/>
        </xsl:when>
        <xsl:otherwise>0</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="endPos" select="($startPos + $itemCount - 1)"/>

    <xsl:choose>
      <xsl:when test="$fld='' and $fileCount &gt; $itemCount and not(/Page/Request/QueryString/Item[@name='showall']/node()='all')">
        <div class="alert alert-info">
          <xsl:if test="$startPos&gt;=$itemCount">
            <a class="btn btn-primary btn-sm" href="?contentType=popup&amp;ewcmd={/Page/@ewCmd}{$pathonly}&amp;fld={$fld}&amp;startPos={($startPos - $itemCount)}">
              Previous <xsl:value-of select="$itemCount"/>
            </a>
          </xsl:if>

          <a class="btn btn-primary btn-sm pull-right" href="?contentType=popup&amp;ewcmd={/Page/@ewCmd}{$pathonly}&amp;fld={$fld}&amp;showall=all">
            Show All
          </a>

          <a class="btn btn-primary btn-sm pull-right" href="?contentType=popup&amp;ewcmd={/Page/@ewCmd}{$pathonly}&amp;fld={$fld}&amp;startPos={($startPos+$itemCount)}">
            Next <xsl:value-of select="$itemCount"/>
          </a>
          <span class="small">
            Showing <xsl:value-of select="($startPos + 1)"/> to <xsl:value-of select="$endPos"/> of <xsl:value-of select="$fileCount"/> files
          </span>

        </div>
      </xsl:when>
      <xsl:otherwise>
        <div class="mb-1">
          <span class="text-muted small">
            <xsl:value-of select="$fld"/> contains <xsl:value-of select="$fileCount"/> files
            <xsl:value-of select="$page/Request/QueryString/Item[@name='pathonly']"/>
          </span>
        </div>

      </xsl:otherwise>
    </xsl:choose>
    <div class="row row-cols-auto" id="files">
      <xsl:choose>
        <xsl:when test="$fld='' and $fileCount &gt; $itemCount and not(/Page/Request/QueryString/Item[@name='showall']/node()='all')">

          <xsl:apply-templates select="file[position() &gt;= $startPos and position() &lt;= $endPos]" mode="ImageFile">
            <xsl:with-param name="fld" select="$fld"/>
          </xsl:apply-templates>
        </xsl:when>
        <xsl:otherwise>
          <xsl:apply-templates select="file" mode="ImageFile">
            <xsl:with-param name="fld" select="$fld"/>
          </xsl:apply-templates>
        </xsl:otherwise>
      </xsl:choose>

      <xsl:text> </xsl:text>
    </div>
  </xsl:template>

  <xsl:template match="folder[ancestor::Page[@layout='DocsLib']]" mode="ImageFolder">
    <table class="table" id="files">
      <thead>
        <tr>
          <th>Filename</th>
          <th>Type</th>
          <th>Options</th>
        </tr>
      </thead>
      <tbody id="files">
        <xsl:for-each select="file">
          <tr>
            <td>
              <i class="icon-file-{translate(@Extension,'.','')}">
                <xsl:text> </xsl:text>
              </i>
              <xsl:text> </xsl:text>
              <xsl:value-of select="@name"/>
            </td>
            <td>
              <xsl:value-of select="@Extension"/>
            </td>
            <td>
              <xsl:choose>
                <xsl:when test="not(contains(/Page/Request/QueryString/Item[@name='contentType'],'popup'))">
                  <a href="?ewcmd={/Page/@ewCmd}&amp;ewCmd2=deleteFile&amp;fld={parent::folder/@path}&amp;file={@name}{@extension}" class="btn btn-xs btn-danger">
                    <i class="fa fa-trash-o fa-white">
                      <xsl:text> </xsl:text>
                    </i>
                    <xsl:text> </xsl:text>Delete
                  </a>
                </xsl:when>
                <xsl:otherwise>
                  <xsl:choose>
                    <!--Pick Document-->
                    <xsl:when test="/Page/@ewCmd='DocsLib'">
                      <a onclick="passDocToForm('{/Page/Request/QueryString/Item[@name='targetForm']/node()}','{/Page/Request/QueryString/Item[@name='targetField']/node()}','/{translate(@root,'\','/')}{translate(parent::folder/@path,'\','/')}/{@name}');" class="btn btn-xs btn-default" href="#">
                        <i class="fa fa-file-o fa-white">
                          <xsl:text> </xsl:text>
                        </i>
                        <xsl:text> </xsl:text>Pick
                      </a>
                    </xsl:when>
                    <!--Pick Media-->
                    <xsl:when test="/Page/@ewCmd='MediaLib'">
                      <a onclick="passMediaToForm('{/Page/Request/QueryString/Item[@name='targetForm']/node()}','{/Page/Request/QueryString/Item[@name='targetField']/node()}','/{translate(@root,'\','/')}{translate(parent::folder/@path,'\','/')}/{@name}');" class="btn btn-xs btn-default" href="#">
                        <i class="fa fa-video-camera fa-white">
                          <xsl:text> </xsl:text>
                        </i>
                        <xsl:text> </xsl:text>Pick Media
                      </a>
                    </xsl:when>
                    <!--Pick Other (so far images)-->
                    <xsl:otherwise>
                      <a href="?ewcmd={/Page/@ewCmd}&amp;ewCmd2=pickImage&amp;fld={parent::folder/@path}&amp;file={@name}{@extension}" class="btn btn-sm btn-primary">
                        Pick Image
                      </a>
                    </xsl:otherwise>
                  </xsl:choose>
                </xsl:otherwise>
              </xsl:choose>
            </td>
          </tr>
        </xsl:for-each>
        <xsl:text> </xsl:text>
      </tbody>
    </table>
  </xsl:template>

  <xsl:template match="file" mode="ImageFile">
    <xsl:param name="fld"/>
    <xsl:variable name="filename">
      <xsl:call-template name="url-encode">
        <xsl:with-param name="str" select="@name"/>
      </xsl:call-template>
    </xsl:variable>
    <div class="item item-image col">
      <div class="panel">
        <div class="image-thumbnail">
          <xsl:variable name="Extension">
            <xsl:value-of select="translate(@Extension,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz')"/>
          </xsl:variable>
          <xsl:choose>
            <xsl:when test="$Extension='.jpg' or $Extension='.jpeg' or $Extension='.gif' or $Extension='.png' or $Extension='.bmp' or $Extension='.tiff' or $Extension='.tif' ">
              <xsl:if test="@root">

                <!--xsl:variable name="imgUrl">
                                                        <xsl:call-template name="resize-image">
                                                            <xsl:with-param name="path" select="concat('/',@root,'/',translate(parent::folder/@path,'\', '/'),'/',@name)"/>
                                                            <xsl:with-param name="max-width" select="'165'"/>
                                                            <xsl:with-param name="max-height" select="'165'"/>
                                                            <xsl:with-param name="file-prefix" select="'~ew/tn7-'"/>
                                                            <xsl:with-param name="file-suffix" select="''"/>
                                                            <xsl:with-param name="quality" select="'99'"/>
                                                            <xsl:with-param name="crop" select="'true'"/>
                                                        </xsl:call-template>
                                                    </xsl:variable>
                                                    <xsl:variable name="imgWidth">
                                                        <xsl:call-template name="get-image-width">
                                                            <xsl:with-param name="path">
                                                                <xsl:value-of select="$imgUrl"/>
                                                            </xsl:with-param>
                                                        </xsl:call-template>
                                                    </xsl:variable>
                                                    <xsl:variable name="imgHeight">
                                                        <xsl:call-template name="get-image-height">
                                                            <xsl:with-param name="path">
                                                                <xsl:value-of select="$imgUrl"/>
                                                            </xsl:with-param>
                                                        </xsl:call-template>
                                                    </xsl:variable-->
                <div class="popoverContent" id="imgpopover{position()}" role="tooltip">
                  <img src="{concat('/',@root,'/',translate(parent::folder/@path,'\', '/'),'/',@name)}" class="img-responsive"/>
                  <div class="popup-description">
                    <span class="image-description-name">
                      <xsl:value-of select="@name"/>
                    </span>
                    <br/>
                    <xsl:if test="@Extension='.jpg' or @Extension='.jpeg' or @Extension='.gif' or @Extension='.png' or @Extension='.tiff' or @Extension='.tif' ">
                      <xsl:value-of select="@width"/>
                      <xsl:text> x </xsl:text>
                      <xsl:value-of select="@height"/>
                    </xsl:if>
                  </div>
                </div>
                <a rel="popover" data-toggle="popover" data-trigger="hover" data-container=".pickImageModal" data-contentwrapper="#imgpopover{position()}" data-placement="top">
                  <xsl:choose>
                    <xsl:when test="@width&gt;160 and @height&gt;160">

                      <img src="/ewcommon/tools/adminthumb.ashx?path=/{@root}{translate(parent::folder/@path,'\', '/')}/{@name}" class="{@class} img-responsive"/>
                    </xsl:when>
                    <xsl:otherwise>
                      <div class="img-overflow">
                        <img src="/{@root}{translate($fld,'\', '/')}/{@name}" alt="" />
                      </div>
                    </xsl:otherwise>
                  </xsl:choose>
                </a>
              </xsl:if>
            </xsl:when>
            <xsl:when test="$Extension='.svg'">
              <div class="img-overflow">
                <img src="/{@root}{translate($fld,'\', '/')}/{@name}" width="160" height="160" class="{@class} img-responsive"/>
              </div>
            </xsl:when>
            <xsl:otherwise>
              <xsl:if test="@icon">
                <img src="/ewcommon/images/icons/{@icon}" width="15" height="15" alt=""/>
              </xsl:if>
            </xsl:otherwise>
          </xsl:choose>
        </div>
        <div class="img-description">
          <span class="image-description-name">
            <xsl:value-of select="@name"/>
          </span>
          <xsl:if test="@Extension='.jpg' or @Extension='.jpeg' or @Extension='.gif' or @Extension='.png' or @Extension='.tiff' or @Extension='.tif'">
            <xsl:value-of select="@width"/>
            <xsl:text> x </xsl:text>
            <xsl:value-of select="@height"/>
          </xsl:if>
        </div>
        <div class="pick-btn">
          <xsl:choose>
            <xsl:when test="not(contains(/Page/Request/QueryString/Item[@name='contentType'],'popup'))">
              <xsl:if test="not(starts-with(/Page/Request/QueryString/Item[@name='fld']/node(),'FreeStock'))">
                <a href="?ewcmd={/Page/@ewCmd}&amp;ewCmd2=deleteFile&amp;fld={$fld}&amp;file={@name}{@extension}" class="btn btn-xs btn-danger">
                  <i class="fa fa-trash-o fa-white">
                    <xsl:text> </xsl:text>
                  </i> Delete
                </a>
              </xsl:if>
            </xsl:when>
            <xsl:when test="/Page/@ewCmd='DocsLib'">
              <a onclick="passDocToForm('{/Page/Request/QueryString/Item[@name='targetForm']/node()}','{/Page/Request/QueryString/Item[@name='targetField']/node()}','/{translate(@root,'\','/')}{translate($fld,'\','/')}/{$filename}');" class="btn btn-xs btn-default" href="#">
                <i class="fa fa-file-o fa-white">
                  <xsl:text> </xsl:text>
                </i>
                <xsl:text> </xsl:text>Pick Document
              </a>
            </xsl:when>
            <!--Pick Media-->
            <xsl:when test="/Page/@ewCmd='MediaLib'">
              <a onclick="passMediaToForm('{/Page/Request/QueryString/Item[@name='targetForm']/node()}','{/Page/Request/QueryString/Item[@name='targetField']/node()}','/{translate(@root,'\','/')}{translate($fld,'\','/')}/{$filename}');" class="btn btn-xs btn-default" href="#">
                <i class="fa fa-video-camera fa-white">
                  <xsl:text> </xsl:text>
                </i>
                <xsl:text> </xsl:text>Pick Media
              </a>
            </xsl:when>
            <xsl:when test="/Page[@ewCmd='ImageLib' and Request/QueryString/Item[@name='ewCmd2']/node()='PathOnly'] or $page/Request/QueryString/Item[@name='pathonly' and node()='true']">
              <xsl:if test="@Extension='.jpg' or @Extension='.jpeg' or @Extension='.gif' or @Extension='.png' or @Extension='.svg' or @Extension='.tiff' or @Extension='.tif' ">
                <a onclick="passImgFileToForm('{/Page/Request/QueryString/Item[@name='targetForm']/node()}','{/Page/Request/QueryString/Item[@name='targetField']/node()}','/{translate(@root,'\','/')}{translate($fld,'\','/')}/{$filename}');" class="btn btn-sm btn-primary" href="#">
                  
                  <xsl:text> </xsl:text>Pick Image
                </a>
              </xsl:if>
            </xsl:when>
            <xsl:otherwise>
              <xsl:if test="@Extension='.jpg' or @Extension='.jpeg' or @Extension='.gif' or @Extension='.png' or @Extension='.svg' or @Extension='.tiff' or @Extension='.tif'">
                <a href="{$appPath}?contentType=popup&amp;ewcmd={/Page/@ewCmd}&amp;ewCmd2=pickImage&amp;fld={$fld}&amp;file={$filename}{@extension}" data-toggle="modal" data-target="#modal-{/Page/Request/QueryString/Item[@name='targetField']/node()}" class="btn btn-sm btn-primary pickImage">
                  
                  Pick Image
                </a>
              </xsl:if>
            </xsl:otherwise>
          </xsl:choose>
        </div>
      </div>
    </div>

  </xsl:template>

</xsl:stylesheet>
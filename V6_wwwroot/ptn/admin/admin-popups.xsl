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

  <xsl:template match="Page">
    <div class="modal-dialog" id="popup1">
      <div class="modal-content">
          <xsl:apply-templates select="." mode="Admin"/>
      </div>
      <xsl:apply-templates select="." mode="LayoutAdminJs"/>
    </div>
  </xsl:template>

  <xsl:template match="Page" mode="SubmitPath">
    <xsl:value-of select="$appPath"/>
    <xsl:text>?contentType=popup&amp;</xsl:text>
  </xsl:template>


	<xsl:template match="Page[@layout='ImageLib' or @layout='DocsLib' or @layout='MediaLib']" mode="Admin">
		<xsl:variable name="MaxUploadWidth">
			<xsl:apply-templates select="." mode="MaxUploadWidth"/>
		</xsl:variable>
		<xsl:variable name="MaxUploadHeight">
			<xsl:apply-templates select="." mode="MaxUploadHeight"/>
		</xsl:variable>
		<xsl:variable name="rootPath">
			<xsl:choose>
				<xsl:when test="@layout='ImageLib'">
					<xsl:call-template name="getSettings">
						<xsl:with-param name="sectionName" select="'web'"/>
						<xsl:with-param name="valueName" select="'ImageRootPath'"/>
					</xsl:call-template>
				</xsl:when>
				<xsl:when test="@layout='DocsLib'">
					<xsl:call-template name="getSettings">
						<xsl:with-param name="sectionName" select="'web'"/>
						<xsl:with-param name="valueName" select="'DocRootPath'"/>
					</xsl:call-template>
				</xsl:when>
				<xsl:when test="@layout='MediaLib'">
					<xsl:call-template name="getSettings">
						<xsl:with-param name="sectionName" select="'web'"/>
						<xsl:with-param name="valueName" select="'MediaRootPath'"/>
					</xsl:call-template>
				</xsl:when>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="partPath"  select="translate(descendant::folder[@active='true']/@path,'\','/')"/>

		<xsl:variable name="targetPath">
			<xsl:text>/</xsl:text>
			<xsl:choose>
				<xsl:when test="starts-with($rootPath,'/')">
					<xsl:value-of select="substring-after($rootPath,'/')"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$rootPath"/>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:choose>
				<xsl:when test="starts-with($partPath,'/')">
					<xsl:value-of select="substring-after($partPath,'/')"/>
				</xsl:when>
				<xsl:otherwise>
					<xsl:value-of select="$partPath"/>
				</xsl:otherwise>
			</xsl:choose>
		</xsl:variable>

		<xsl:variable name="submitPath">
			<xsl:apply-templates select="." mode="SubmitPath"/>
		</xsl:variable>

		<xsl:variable name="pathonly">
			<xsl:if test="$page/Request/QueryString/Item[@name='ewCmd2' and node()='PathOnly']">
				<xsl:text>&amp;pathonly=true</xsl:text>
			</xsl:if>
			<xsl:if test="$page/Request/QueryString/Item[@name='pathonly' and node()='true']">
				<xsl:text>&amp;pathonly=true</xsl:text>
			</xsl:if>
		</xsl:variable>

		<div id="template_FileSystem" class="modal-body">

			<div class="row">

				<div id="MenuTree" class="list-group col-md-3 col-sm-4 mb-3">
					<xsl:if test="contains(/Page/Request/QueryString/Item[@name='contentType'],'popup')">
						<xsl:attribute name="class">list-group col-md-4 col-lg-3 col-xxl-2 mb-3</xsl:attribute>
					</xsl:if>
					<xsl:apply-templates select="ContentDetail/folder" mode="FolderTree">
						<xsl:with-param name="level">1</xsl:with-param>
					</xsl:apply-templates>
				</div>

				<div class="col-md-9 col-sm-8">
					<xsl:if test="contains(/Page/Request/QueryString/Item[@name='contentType'],'popup')">
						<xsl:attribute name="class">col-md-8 col-lg-9 col-xxl-10</xsl:attribute>
					</xsl:if>
					<xsl:for-each select="descendant-or-self::folder[@active='true']">
						<div class="btn-group-spaced mb-1">
							<xsl:if test="not(contains(/Page/Request/QueryString/Item[@name='contentType'],'popup')) and not(@path='')">

								<a href="{$submitPath}ewcmd={/Page/@ewCmd}{$pathonly}&amp;fld={parent::folder/@path}" class="btn btn-sm btn-outline-primary">
									<xsl:if test="$submitPath!='/?'">
										<xsl:attribute name="data-bs-toggle">modal</xsl:attribute>
										<xsl:attribute name="data-target">
											<xsl:text>#modal-</xsl:text>
											<xsl:value-of select="/Page/Request/QueryString/Item[@name='targetField']/node()"/>
										</xsl:attribute>
									</xsl:if>
									<i class="fa fa-arrow-up fa-white">
										<xsl:text> </xsl:text>
									</i>
									Up Folder
								</a>

							</xsl:if>
							<xsl:if test="not(starts-with(/Page/Request/QueryString/Item[@name='fld']/node(),'\FreeStock'))">

								<a href="{$submitPath}ewcmd={/Page/@ewCmd}{$pathonly}&amp;ewCmd2=addFolder&amp;fld={@path}&amp;targetForm={/Page/Request/QueryString/Item[@name='targetForm']/node()}&amp;targetField={/Page/Request/QueryString/Item[@name='targetField']/node()}" class="btn btn-sm btn-outline-primary">
									<xsl:if test="$submitPath!='/?'">
										<xsl:attribute name="data-bs-toggle">modal</xsl:attribute>
										<xsl:attribute name="data-target">
											<xsl:text>#modal-</xsl:text>
											<xsl:value-of select="/Page/Request/QueryString/Item[@name='targetField']/node()"/>
										</xsl:attribute>
									</xsl:if>
									<i class="fas fa-folder-open fa-white">
										<xsl:text> </xsl:text>
									</i>&#160;New Folder
								</a>

								<!-- The fileinput-button span is used to style the file input field as button -->
								<span class="btn btn-sm btn-outline-primary fileinput-button">
									<i class="fa fa-upload fa-white">
										<xsl:text> </xsl:text>
									</i>
									<xsl:text> </xsl:text>
									<span>Upload Files</span>
									<!-- The file input field used as target for the file upload widget -->
									<input id="fileupload" type="file" name="files[]" multiple="" class="fileUploadCheck"/>
								</span>

								<!--not for popup window or for root..!-->
								<xsl:if test="not(contains(/Page/Request/QueryString/Item[@name='contentType'],'popup')) and not(@path='')">
									<xsl:if test="parent::folder">

										<a href="{$submitPath}ewcmd={/Page/@ewCmd}&amp;ewCmd2=deleteFolder&amp;fld={@path}" class="btn btn-sm btn-outline-danger">
											<i class="fas fa-trash fa-white">
												<xsl:text> </xsl:text>
											</i>
											Delete Folder
										</a>
									</xsl:if>

								</xsl:if>
							</xsl:if>
							<div id="progress">
								<div class="bar" style="width: 0%;"></div>
							</div>
						</div>
					</xsl:for-each>
					<div id="uploadFiles">
						<xsl:choose>
							<xsl:when test="contains($browserVersion,'Firefox') or contains($browserVersion,'Chrome')">
								<div id="progress" class="progress">
									<div class="overlay">
										<i class="fas fa-mouse-pointer">&#160;</i> and drop files here to upload
									</div>
									<div class="overlay loading-counter">
										&#160;loading
										<span class="count">0</span>%
									</div>
									<div class="progress-bar progress-bar-striped bg-info" role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100">

									</div>
								</div>
							</xsl:when>
							<xsl:when test="contains($browserVersion,'MSIE') or contains($browserVersion,'')">
								<div class="hint">
									Note: You can upload multiple files without refreshing the page.<br/> You are using Internet Explorer<br/> Try Chrome or Firefox to upload multiple files using drag and drop
								</div>
							</xsl:when>
						</xsl:choose>
						<xsl:for-each select="descendant-or-self::folder[@active='true']">
							<div id="fileupload">
								<xsl:apply-templates select="." mode="ImageFolder">
									<xsl:with-param name="rootPath" select="$rootPath"/>
								</xsl:apply-templates>
							</div>
						</xsl:for-each>
					</div>
				</div>
			</div>

		</div>

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

	var newItem = getUploadedImageHtmlPopup('<xsl:value-of select="$appPath"/>','<xsl:value-of select="$fld"/>',targetPath,'<xsl:value-of select="$targetFeild"/>',file.name)
	prepareAjaxModals();
	
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

	  var newItem = getUploadedImagePathPopup('<xsl:value-of select="$appPath"/>','<xsl:value-of select="$fld"/>',targetPath,'<xsl:value-of select="$targetFeild"/>',file.name)
	  prepareAjaxModals();
	  
  </xsl:template>

  <xsl:template match="Page[@layout='DocsLib']" mode="newItemScript">
    var newItem = '<tr>';
		newItem = newItem + '<td>';
			newItem = newItem + '<i class="icon-file-' + /[^.]+$/.exec(file.name) + '"> </i> ' + file.name.replace(/\ /g,'-') + ';
		newItem = newItem + '</td>';
		newItem = newItem + '<td>.' + /[^.]+$/.exec(file.name) + '</td>';
		newItem = newItem + '<td>';
        <!--newItem = newItem + '<a onclick="passDocToForm(\'EditContent\',\'cContentDocPath\',\' + targetPath + \'/\' + file.name + \');" class="btn btn-xs btn-default" href="#">';-->
        newItem = newItem + '<a onclick="passDocToForm(\'EditContent\',\'cContentDocPath\',\'' + targetPath + '/' + file.name.replace(/\ /g,'-') + '\');" class="btn btn-xs btn-default" href="#">';
          newItem = newItem + '<i class="fa fa-file-o fa-white"> </i> Pick';
			newItem = newItem + '</a>';
			newItem = newItem + '</td>';
		newItem = newItem + '</tr>';
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
					  <xsl:when test="@width&gt;125 and @height&gt;125">
						  <img class="lazy" src="/ptn/core/images/loader.gif" data-src="/{@root}{translate(parent::folder/@path,'\', '/')}/{@thumbnail}"/>
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

	<xsl:template match="Content" mode="xform">
		<form method="{model/submission/@method}" action="">
			<xsl:attribute name="class">
				<xsl:text>xform </xsl:text>
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
			<xsl:if test="descendant::upload">
				<xsl:attribute name="enctype">multipart/form-data</xsl:attribute>
			</xsl:if>
			<xsl:choose>
				<xsl:when test="count(group) = 2 and group[2]/submit and count(group[2]/*[name()!='submit']) = 0">
					<xsl:for-each select="group[1]">
						<div>
							<xsl:apply-templates select="." mode="xform"/>
						</div>
					</xsl:for-each>
					<xsl:for-each select="group[2]">
						<xsl:if test="count(submit) &gt; 0">			
						    <div class="modal-footer">
							    <xsl:apply-templates select="submit" mode="xform"/>
									<div class="footer-status">
										<span>
											<i class="fas fa-eye"> </i> Live
										</span>
										<span class="text-muted hidden">
											<i class="fas fa-eye-slash"> </i> Hidden
										</span>
									</div>
							</div>
						</xsl:if>
					</xsl:for-each>
				</xsl:when>
				<xsl:otherwise>
					<xsl:if test="label[position()=1]">

					</xsl:if>
			
		
						<xsl:apply-templates select="group | repeat " mode="xform"/>
						<xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | hint | help | alert | div" mode="xform"/>
		
					<xsl:if test="count(submit) &gt; 0">
						<div class="modal-footer">
							<xsl:apply-templates select="submit" mode="xform"/>
						</div>
					</xsl:if>
				</xsl:otherwise>
			</xsl:choose>

		</form>
		<xsl:apply-templates select="descendant-or-self::*" mode="xform_modal"/>
	</xsl:template>

	<xsl:template match="group[parent::Content]" mode="xform">
		<xsl:param name="class"/>
		<div class="modal-body">
			<xsl:if test=" @id!='' ">
				<xsl:attribute name="id">
					<xsl:value-of select="@id"/>
				</xsl:attribute>
			</xsl:if>
			<xsl:apply-templates select="label[position()=1]" mode="legend"/>

			<xsl:apply-templates select="input | secret | select | select1 | range | textarea | upload | group | repeat | alert | div | repeat | relatedContent | label[position()!=1] | trigger | script" mode="control-outer"/>
	    </div>
		<xsl:if test="count(submit) &gt; 0">
				<xsl:if test="not(submit[contains(@class,'hideRequired')])">
					<xsl:if test="ancestor::group/descendant-or-self::*[contains(@class,'required')]">
						<label class="required required-message">
							<span class="req">*</span>
							<xsl:text> </xsl:text>
							<xsl:call-template name="msg_required"/>
						</label>
					</xsl:if>
				</xsl:if>
				<div class="modal-footer">
				<xsl:apply-templates select="submit" mode="xform"/>
				</div>
			</xsl:if>
		
	</xsl:template>

</xsl:stylesheet>
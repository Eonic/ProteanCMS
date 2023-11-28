// ***********************************************************************
// $Library:     Protean.fsHelper.WebFile
// $Revision:    4.0  
// $Date:        2011-03-01
// $Author:      Ali Granger
// &Website:     www.eonic.co.uk
// &Licence:     All Rights Reserved.
// $Copyright:   Copyright (c) 2002 - 2011 EonicWeb Ltd.
// ***********************************************************************

using System;
using System.IO;
using Microsoft.VisualBasic;
using Protean.Tools.Integration.Twitter;

namespace Protean
{

    public partial class fsHelper
    {

        /// <summary>
        /// WebFile is a lightweight representation of a file that is web accessible.
        /// Its main purpose is to produce an object that can be easily XML serialized and passed to XSL for transformation
        /// As it is written with XmlSerializer in mind the properties have to not be readonly, although they
        /// naturally would be.  At the moment soem of the Set methods for such properties will do nothing - thi
        /// class is not intended for deserialization.
        /// </summary>
        /// <remarks>To initialise the virtual paths use the WebFileHelper class</remarks>
        public class WebFile
        {

            private string _absolutepath = string.Empty;
            private string _virtualpath = string.Empty;
            private string _extension = string.Empty;
            private string _filename = string.Empty;
            private bool _exists = false;

            private bool _extendedPropertiesProcessed = false;

            private Protean.fsHelper.LibraryType _libraryType = fsHelper.LibraryType.Undefined;
            private fsHelper.WebFileExtendedProperties _extendedProperties;

            // Constructor for XmlSerializer
            private WebFile()
            {

            }

            public WebFile(string fullpath, string virtualPath, bool processExtendedProperties = false)
            {
                SetPhysicalPath(fullpath);
                _virtualpath = virtualPath;
                if (processExtendedProperties)
                {
                    _extendedProperties = new fsHelper.WebFileExtendedProperties(this);
                    _extendedProperties.Process();
                    _extendedPropertiesProcessed = true;
                }
            }

            public string AbsolutePath
            {
                get
                {
                    return _absolutepath;
                }
                set
                {
                    _absolutepath = value;
                }
            }


            public string VirtualPath
            {
                get
                {
                    return _virtualpath;
                }
                set
                {
                    _virtualpath = value;
                }
            }

            public string Extension
            {
                get
                {
                    return _extension;
                }
                set
                {
                    _extension = value;
                }
            }

            public string FileName
            {
                get
                {
                    return _filename;
                }
                set
                {
                    _filename = value;
                }
            }

            public bool Exists
            {
                get
                {
                    return _exists;
                }
                set
                {
                    _exists = value;
                }
            }

            public string Type
            {
                get
                {
                    return Enum.GetName(typeof(LibraryType), _libraryType);
                }
                set
                {
                    // _filename = value
                }
            }

            protected internal LibraryType LibraryType
            {
                get
                {
                    return _libraryType;
                }
            }

            public fsHelper.WebFileExtendedProperties ExtendedProperties
            {
                get
                {
                    return _extendedProperties;
                }
                set
                {
                    // _filename = value
                }
            }

            private void SetPhysicalPath(string physicalPath)
            {

                var fi = new FileInfo(physicalPath);
                _exists = fi.Exists;
                _absolutepath = physicalPath;
                if (_exists)
                {
                    _extension = Strings.LCase(fi.Extension);
                    _filename = fi.Name;
                    _libraryType = GetLibraryTypeFromExtension(_extension);
                }

            }





        }




    }
}
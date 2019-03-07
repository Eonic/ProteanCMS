'***********************************************************************
' $Library:     Protean.fsHelper.WebFile
' $Revision:    4.0  
' $Date:        2011-03-01
' $Author:      Ali Granger
' &Website:     www.eonic.co.uk
' &Licence:     All Rights Reserved.
' $Copyright:   Copyright (c) 2002 - 2011 EonicWeb Ltd.
'***********************************************************************

Option Strict Off
Option Explicit On
Imports System.Data
Imports System.Data.SqlClient
Imports System.Xml
Imports System.IO
Imports System.Configuration
Imports System.Collections
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Security.Principal
Imports System.Web.Configuration
Imports Protean.Tools.DelegateWrappers
Imports System

Partial Public Class fsHelper

    ''' <summary>
    ''' WebFile is a lightweight representation of a file that is web accessible.
    ''' Its main purpose is to produce an object that can be easily XML serialized and passed to XSL for transformation
    ''' As it is written with XmlSerializer in mind the properties have to not be readonly, although they
    ''' naturally would be.  At the moment soem of the Set methods for such properties will do nothing - thi
    ''' class is not intended for deserialization.
    ''' </summary>
    ''' <remarks>To initialise the virtual paths use the WebFileHelper class</remarks>
    Public Class WebFile

        Private _absolutepath As String = String.Empty
        Private _virtualpath As String = String.Empty
        Private _extension As String = String.Empty
        Private _filename As String = String.Empty
        Private _exists As Boolean = False

        Private _extendedPropertiesProcessed As Boolean = False

        Private _libraryType As fsHelper.LibraryType = fsHelper.LibraryType.Undefined
        Private _extendedProperties As WebFileExtendedProperties

        ' Constructor for XmlSerializer
        Private Sub New()

        End Sub

        Public Sub New(ByVal fullpath As String, ByVal virtualPath As String, Optional ByVal processExtendedProperties As Boolean = False)
            SetPhysicalPath(fullpath)
            _virtualpath = virtualPath
            If processExtendedProperties Then
                _extendedProperties = New WebFileExtendedProperties(Me)
                _extendedProperties.Process()
                _extendedPropertiesProcessed = True
            End If
        End Sub

        Public Property AbsolutePath() As String
            Get
                Return _absolutepath
            End Get
            Set(ByVal value As String)
                _absolutepath = value
            End Set
        End Property


        Public Property VirtualPath() As String
            Get
                Return _virtualpath
            End Get
            Set(ByVal value As String)
                _virtualpath = value
            End Set
        End Property

        Public Property Extension() As String
            Get
                Return _extension
            End Get
            Set(ByVal value As String)
                _extension = value
            End Set
        End Property

        Public Property FileName() As String
            Get
                Return _filename
            End Get
            Set(ByVal value As String)
                _filename = value
            End Set
        End Property

        Public Property Exists() As Boolean
            Get
                Return _exists
            End Get
            Set(ByVal value As Boolean)
                _exists = value
            End Set
        End Property

        Public Property Type() As String
            Get
                Return [Enum].GetName(GetType(LibraryType), _libraryType)
            End Get
            Set(ByVal value As String)
                '_filename = value
            End Set
        End Property

        Protected Friend ReadOnly Property LibraryType() As LibraryType
            Get
                Return _libraryType
            End Get
        End Property

        Public Property ExtendedProperties() As WebFileExtendedProperties
            Get
                Return _extendedProperties
            End Get
            Set(ByVal value As WebFileExtendedProperties)
                '_filename = value
            End Set
        End Property

        Private Sub SetPhysicalPath(ByVal physicalPath As String)

            Dim fi As New FileInfo(physicalPath)
            _exists = fi.Exists
            _absolutepath = physicalPath
            If _exists Then
                _extension = LCase(fi.Extension)
                _filename = fi.Name
                _libraryType = GetLibraryTypeFromExtension(_extension)
            End If

        End Sub





    End Class




End Class







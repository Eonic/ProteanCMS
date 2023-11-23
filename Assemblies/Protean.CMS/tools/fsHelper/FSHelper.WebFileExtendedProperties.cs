// ***********************************************************************
// $Library:     Protean.fsHelper.WebFileExtendedProperties
// $Revision:    4.0  
// $Date:        2011-03-01
// $Author:      Ali Granger
// &Website:     www.eonic.co.uk
// &Licence:     All Rights Reserved.
// $Copyright:   Copyright (c) 2002 - 2011 EonicWeb Ltd.
// ***********************************************************************

using System;
using System.Windows.Media.Imaging;

namespace Protean
{


    public partial class fsHelper
    {

        public class WebFileExtendedProperties
        {

            private WebFile _webFile = default;

            private int _width;
            private int _height;


            private WebFileExtendedProperties()
            {

            }

            public WebFileExtendedProperties(WebFile baseFile)
            {
                _webFile = baseFile;
            }

            public int Width
            {
                get
                {
                    return _width;
                }
                set
                {
                    // _filename = value
                }
            }

            public int Height
            {
                get
                {
                    return _height;
                }
                set
                {
                    // _filename = value
                }
            }

            /// <summary>
        /// Attempts to retrieve library type based additional properties
        /// </summary>
        /// <remarks></remarks>
            public void Process()
            {
                try
                {


                    if (_webFile is not null && _webFile.Exists)
                    {
                        switch (_webFile.LibraryType)
                        {

                            case var @case when @case == LibraryType.Image:
                                {
                                    // original
                                    // Dim imageFromFile As System.Drawing.Image = System.Drawing.Image.FromFile(_webFile.AbsolutePath)
                                    // _width = imageFromFile.Width
                                    // _height = imageFromFile.Height

                                    // didn't work
                                    // Dim oSize As Size = GetDimensions(_webFile.AbsolutePath)
                                    // _width = oSize.Width
                                    // _height = oSize.Height

                                    BitmapSource imageFromFile = BitmapFrame.Create(new Uri(_webFile.AbsolutePath));
                                    _width = imageFromFile.PixelWidth;
                                    _height = imageFromFile.PixelHeight;
                                    break;
                                }



                        }
                    }
                }
                catch (Exception ex)
                {
                    _width = 0;
                    _height = 0;
                }
            }


            // Const errorMessage As String = "Could not recognise image format."
            // TS--- Need to get the below function running in .net 2 without Linq
            // Waiting for .Net 4
            // Private Shared imageFormatDecoders As New Dictionary(Of Byte(), Func(Of BinaryReader, Size))() From { _
            // {New Byte() {&H42, &H4d}, DecodeBitmap}, _
            // {New Byte() {&H47, &H49, &H46, &H38, &H37, &H61}, DecodeGif}, _
            // {New Byte() {&H47, &H49, &H46, &H38, &H39, &H61}, DecodeGif}, _
            // {New Byte() {&H89, &H50, &H4e, &H47, &Hd, &Ha, &H1a, &Ha}, DecodePng}, _
            // {New Byte() {&Hff, &Hd8}, DecodeJfif}}


            // ''' <summary>        
            // ''' Gets the dimensions of an image.        
            // ''' </summary>        
            // ''' <param name="path">The path of the image to get the dimensions of.</param>        
            // ''' <returns>The dimensions of the specified image.</returns>        
            // ''' <exception cref="ArgumentException">The image was of an unrecognised format.</exception>        
            // Public Shared Function GetDimensions(ByVal path As String) As Size
            // Try
            // Using binaryReader As New BinaryReader(File.OpenRead(path))
            // Try
            // Return GetDimensions(binaryReader)
            // Catch e As ArgumentException
            // Dim newMessage As String = String.Format("{0} file: '{1}' ", errorMessage, path)

            // Throw New ArgumentException(newMessage, "path", e)
            // End Try
            // End Using
            // Catch generatedExceptionName As ArgumentException
            // 'do it the old fashioned way

            // Using b As New Bitmap(path)
            // Return b.Size
            // End Using
            // End Try
            // End Function

            // ''' <summary>        
            // ''' Gets the dimensions of an image.        
            // ''' </summary>        
            // ''' <param name="path">The path of the image to get the dimensions of.</param>        
            // ''' <returns>The dimensions of the specified image.</returns>        
            // ''' <exception cref="ArgumentException">The image was of an unrecognised format.</exception>            
            // Public Shared Function GetDimensions(ByVal binaryReader As BinaryReader) As Size
            // Dim maxMagicBytesLength As Integer = imageFormatDecoders.Keys.OrderByDescending(Function(x) x.Length).First().Length
            // Dim magicBytes As Byte() = New Byte(maxMagicBytesLength - 1) {}
            // For i As Integer = 0 To maxMagicBytesLength - 1
            // magicBytes(i) = binaryReader.ReadByte()
            // For Each kvPair As var In imageFormatDecoders
            // If StartsWith(magicBytes, kvPair.Key) Then
            // Return kvPair.Value(binaryReader)
            // End If
            // Next
            // Next

            // Throw New ArgumentException(errorMessage, "binaryReader")
            // End Function

            // Private Shared Function StartsWith(ByVal thisBytes As Byte(), ByVal thatBytes As Byte()) As Boolean
            // For i As Integer = 0 To thatBytes.Length - 1
            // If thisBytes(i) <> thatBytes(i) Then
            // Return False
            // End If
            // Next

            // Return True
            // End Function

            // Private Shared Function ReadLittleEndianInt16(ByVal binaryReader As BinaryReader) As Short
            // Dim bytes As Byte() = New Byte(2 - 1) {}

            // For i As Integer = 0 To 2 - 1
            // bytes(2 - 1 - i) = binaryReader.ReadByte()
            // Next
            // Return BitConverter.ToInt16(bytes, 0)
            // End Function

            // Private Shared Function ReadLittleEndianUInt16(ByVal binaryReader As BinaryReader) As UShort
            // Dim bytes As Byte() = New Byte(2 - 1) {}

            // For i As Integer = 0 To 2 - 1
            // bytes(2 - 1 - i) = binaryReader.ReadByte()
            // Next
            // Return BitConverter.ToUInt16(bytes, 0)
            // End Function

            // Private Shared Function ReadLittleEndianInt32(ByVal binaryReader As BinaryReader) As Integer
            // Dim bytes As Byte() = New Byte(4 - 1) {}
            // For i As Integer = 0 To 4 - 1
            // bytes(4 - 1 - i) = binaryReader.ReadByte()
            // Next
            // Return BitConverter.ToInt32(bytes, 0)
            // End Function

            // Private Shared Function DecodeBitmap(ByVal binaryReader As BinaryReader) As Size
            // binaryReader.ReadBytes(16)
            // Dim width As Integer = binaryReader.ReadInt32()
            // Dim height As Integer = binaryReader.ReadInt32()
            // Return New Size(width, height)
            // End Function

            // Private Shared Function DecodeGif(ByVal binaryReader As BinaryReader) As Size
            // Dim width As Integer = binaryReader.ReadInt16()
            // Dim height As Integer = binaryReader.ReadInt16()
            // Return New Size(width, height)
            // End Function

            // Private Shared Function DecodePng(ByVal binaryReader As BinaryReader) As Size
            // binaryReader.ReadBytes(8)
            // Dim width As Integer = ReadLittleEndianInt32(binaryReader)
            // Dim height As Integer = ReadLittleEndianInt32(binaryReader)
            // Return New Size(width, height)
            // End Function

            // Private Shared Function DecodeJfif(ByVal binaryReader As BinaryReader) As Size
            // While binaryReader.ReadByte() = &HFF
            // Dim marker As Byte = binaryReader.ReadByte()
            // Dim chunkLength As Short = ReadLittleEndianInt16(binaryReader)
            // If marker = &HC0 Then
            // binaryReader.ReadByte()
            // Dim height As Integer = ReadLittleEndianInt16(binaryReader)
            // Dim width As Integer = ReadLittleEndianInt16(binaryReader)
            // Return New Size(width, height)
            // End If

            // If chunkLength < 0 Then
            // Dim uchunkLength As UShort = CUShort(chunkLength)
            // binaryReader.ReadBytes(uchunkLength - 2)
            // Else
            // binaryReader.ReadBytes(chunkLength - 2)
            // End If
            // End While

            // Throw New ArgumentException(errorMessage)
            // End Function

        }

    }
}
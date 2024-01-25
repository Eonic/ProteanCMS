using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualBasic; // Install-Package Microsoft.VisualBasic
//using TinifyAPI;
using System.Drawing.Drawing2D;
using static Protean.Tools.Text;
using TinifyAPI;
using Exception = System.Exception;
using System.Runtime.InteropServices.ComTypes;
using System.Text.RegularExpressions;

namespace Protean.Tools
{
    public class Image
    {
        #region Declarations
        private string cLocation; // Location of the file to load
        private bool bKeepRelational = true; // keep sizes relation or skew
        private System.Drawing.Bitmap oImg; // the base image
        private System.Drawing.Bitmap oSourceImg; // the base image
        private Graphics oGraphics;
        public event OnErrorEventHandler OnError;

        public delegate void OnErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);
        private const string mcModuleName = "Protean.Tools.Image";
        private bool bCrop = false; // Crop the image?
        private bool bNoStretch = false; // Don't Expand, only Shrink

        private int nMaxHeightCrop = 0;
        private int nMaxWidthCrop = 0;

        public string TinifyKey = "";

        public System.Drawing.Image Image1
        {
            get
            {
                return oImg;
            }
            set
            {
                oImg =(System.Drawing.Bitmap)value;
            }
        }

        #endregion


        #region Public Subs
        public Image(string Location)
        {
            try
            {
                cLocation = Location; // set the location
                ReLoad(); // load the image
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""));
            }
        }

        public void Close()
        {
            // closes
            try
            {
                if (oGraphics != null)
                {
                    oGraphics.Dispose();
                    oGraphics = null;
                }
                if (oImg != null)
                {
                    oImg.Dispose();
                    oImg = null;
                }
                if (oSourceImg != null)
                {
                    oSourceImg.Dispose();
                    oSourceImg = null;
                }
                
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Close", ex, ""));
            }
            finally
            {

            }
        }

        public void ReLoad()
        {
            // load the image
            try
            {
                if (File.Exists(cLocation))
                {
                    using (var stream = File.OpenRead(cLocation))
                    {
                        oImg = (Bitmap)System.Drawing.Image.FromStream(stream);
                    }
                    // oImg = System.Drawing.Image.FromFile(cLocation)
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ReLoad", ex, ""));
            }
        }

        public void UploadProcessing(string _WatermarkText, string _WatermarkImgPath)
        {
            try
            {
                if (!string.IsNullOrEmpty(_WatermarkText))
                {
                    AddWatermark(oImg, _WatermarkText, _WatermarkImgPath);
                }

                // overwrite
                var imgFile = new FileInfo(cLocation);
                CompressImage(imgFile, true);
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""));
            }
        }

        public void SetMaxSize(int nWidth, int nHeight)
        {
            // decides wether it needs to be sized
            try
            {
                if (oImg != null)
                {
                    if (nWidth > 0 & !(nWidth == oImg.Width) | nHeight > 0 & !(nHeight == oImg.Height))
                    {
                        if (bKeepRelational)
                        {
                            ResizeMax(nWidth, nHeight);
                        }
                        else
                        {
                            Resize(nWidth, nHeight);
                        }
                    }
                    else if (nWidth == oImg.Width & nHeight == oImg.Height)
                    {
                        //System.Drawing.Image img = (System.Drawing.Image)oImg;
                        oImg = (Bitmap)ImageResize(oImg, nHeight, nWidth);
                    }
                }
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SetMaxSize", ex, ""));
            }
        }

        public void DeleteOriginal()
        {
            // deletes the original image
            try
            {
                File.Delete(cLocation);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "DeleteOriginal", ex, ""));
            }
        }



        public void Reflect(Color _BackgroundColor, int _Reflectivity)
        {
            // Calculate the size of the new image
            try
            {
                AddReflection(_BackgroundColor, _Reflectivity);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Reflect", ex, ""));
            }
        }

        #endregion

        #region Public Functions
        public bool Save(string cPath, int nCompression = 25, string serverPath = "")
        {
            // saves the file to designated location
            try
            {

                cPath = cPath.Replace("/","\\");
               // cPath = cPath.Replace("\\\\", "\\");

                // check the compression ratio
                if (nCompression > 100)
                    nCompression = 100;
                if (nCompression < 1)
                    nCompression = 1;
                // saves with compression and formatting
                return SaveJPGWithCompressionSetting(oImg, cPath, nCompression, serverPath);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Save", ex, ""));
                return false;
            }
        }

        public string CreateThumbnail(string VirtualPath, string VirtualThumbnailPath)
        {
            // saves the file to designated location
            //int nCompression = 50;
            try
            {
                //string thumbnailVirtualPath = "";
                var fi = new FileInfo(cLocation);
                string thumbnailPath = cLocation.Remove(cLocation.Length - fi.Name.Length) + VirtualThumbnailPath.Replace("/","") + @"\";
                var thfi = new FileInfo(thumbnailPath + fi.Name.Replace(".gif",".png"));

                if (thfi.Exists == false)
                {

                    KeepXYRelation = true;
                    NoStretch = true;
                    IsCrop = false;
                    SetMaxSize(195, 195);
                    Save(thumbnailPath + fi.Name, 101, thumbnailPath);

                }
                thfi = null;
                return VirtualThumbnailPath + "/" + fi.Name.Replace(".gif", ".png");
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Save", ex, ""));
                return "Error";
            }
        }

        #endregion

        #region Public Properties

        public int Width
        {
            // returns current width
            get
            {
                return oImg.Width;
            }
            set
            {
                // sets the width to value specified
                if (value >= 1)
                    Resize(value, 0);
            }
        }

        public int Height
        {
            get
            {
                // returns current height
                return oImg.Height;
            }
            set
            {
                // resizes
                if (value >= 1)
                    Resize(0, value);
            }
        }

        public bool KeepXYRelation
        {
            // simple property for the relationship between X and Y
            get
            {
                return bKeepRelational;
            }
            set
            {
                bKeepRelational = value;
            }
        }

        public bool IsCrop
        {
            // Property for if image is wanting to be cropped
            get
            {
                return bCrop;
            }
            set
            {
                bCrop = value;
            }
        }

        public bool NoStretch
        {
            // Property for if image is wanting to be cropped
            get
            {
                return bNoStretch;
            }
            set
            {
                bNoStretch = value;
            }
        }


        #endregion

        #region Private Procedures
        private void Resize(int nWidth, int nHeight)
        {
            // decides on the new sizes for the image
            try
            {
                int nNewWidth;
                int nNewHeight;

                if (!bKeepRelational)
                {
                    if (nWidth > 0)
                    {
                        nNewWidth = nWidth;
                        nNewHeight = oImg.Height;
                    }
                    else
                    {
                        nNewHeight = nHeight;
                        nNewWidth = oImg.Width;
                    }
                }
                else
                {
                    float nPercent;
                    if (nWidth > 0)
                    {
                        nPercent = (float)nWidth / (float)oImg.Width;
                        nNewWidth = Convert.ToInt32(oImg.Width * nPercent);
                        nNewHeight = Convert.ToInt32(oImg.Height * nPercent);
                    }
                    else
                    {
                        nPercent = (float)nHeight / (float)oImg.Height;
                        nNewWidth = Convert.ToInt32(oImg.Width * nPercent);
                        nNewHeight = Convert.ToInt32(oImg.Height * nPercent);
                    }
                }
                if (nNewHeight > 0 && nNewWidth > 0)
                {
                    oImg = (Bitmap)ImageResize(oImg, nNewHeight, nNewWidth);
                }
              
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Resize", ex, ""));
            }
        }

        private void ResizeMax(int nMaxWidth, int nMaxHeight)
        {
            try
            {

                if (bCrop)
                {
                    // For Cropped images, we want to expand to the biggest variant
                    nMaxHeightCrop = nMaxHeight;
                    nMaxWidthCrop = nMaxWidth;

                    // NB 13th April 2010 Changes!

                    float nXCalc = (float)oImg.Width / (float)nMaxWidth;
                    float yXCalc = (float)oImg.Height / (float)nMaxHeight;


                    if (bNoStretch == false)
                    {

                        if (yXCalc < nXCalc)
                        {
                            Resize(0, nMaxHeight);
                        }
                        else
                        {
                            Resize(nMaxWidth, 0);
                        }
                    }


                    // If not Stretch
                    // If both bigger, shrink
                    else if (oImg.Width >= nMaxWidth & oImg.Height >= nMaxHeight)
                    {
                        if (yXCalc < nXCalc)
                        {
                            Resize(0, nMaxHeight);
                        }
                        else
                        {
                            Resize(nMaxWidth, 0);
                        }
                    }
                    // Else Shrink accordingly
                    else if (oImg.Width < nMaxWidth & oImg.Height >= nMaxHeight)
                    {
                        Resize(nMaxWidth, 0);
                    }
                    else if (oImg.Height < nMaxHeight & oImg.Width >= nMaxWidth)
                    {
                        Resize(0, nMaxHeight);
                    }
                    else if (oImg.Width < nMaxWidth & oImg.Height < nMaxHeight)
                    {
                        // Do nothing for if both smaller!
                        // Blow it up....
                        if (oImg.Width < oImg.Height)
                        {
                            Resize(0, nMaxHeight);
                        }
                        else
                        {
                            Resize(nMaxWidth, 0);
                        }
                    }
                }

                // If oImg.Height < oImg.Width Then
                // Resize(0, nMaxHeight)
                // Else
                // Resize(nMaxWidth, 0)
                // End If

                // ElseIf bNoStretch Then
                // If ((oImg.Height > nMaxHeight) Or (oImg.Width > nMaxWidth)) Then
                // 'Squares, since they are squares just shrink to the smaller side
                // If oImg.Height = oImg.Width Then
                // If nMaxWidth > nMaxHeight Then
                // Resize(0, nMaxHeight)
                // Else
                // Resize(nMaxWidth, 0)
                // End If


                // 'Rectangles
                // ElseIf oImg.Height > oImg.Width Then
                // Resize(0, nMaxHeight)
                // Else
                // Resize(nMaxWidth, 0)
                // End If
                // ' No need to do anything if both sides are smaller
                // End If

                else
                {
                    // For regular stretches, make this biggest possible within the max height/width bounds

                    // NB 20-Feb-2009 Redone, old if statements feel a bit clumsy
                    double nMaxResizedArea = nMaxWidth * nMaxHeight;
                    double nXscaledArea = nMaxWidth * (oImg.Height * (nMaxWidth / oImg.Width));
                    double nYscaledArea = oImg.Width * (nMaxHeight / oImg.Height) * nMaxHeight;


                    if (bNoStretch & !(oImg.Width >= nMaxWidth & oImg.Height >= nMaxHeight))
                    {
                        // IF Both bigger skip
                        if (oImg.Width >= nMaxWidth)
                        {
                            Resize(nMaxWidth, 0);
                        }
                        else if (oImg.Height >= nMaxHeight)
                        {
                            Resize(0, nMaxHeight);
                        }
                        else
                        {
                            // increase size of the image
                            Resize(oImg.Width, oImg.Height);
                            // Do nothing if both smaller!
                        }
                    }

                    // Regular aka no Crop/Stretch tests
                    else if (nXscaledArea <= nMaxResizedArea & nYscaledArea <= nMaxResizedArea)
                    {
                        if (nXscaledArea >= nYscaledArea)
                        {
                            Resize(nMaxWidth, 0);
                        }
                        else
                        {
                            Resize(0, nMaxHeight);
                        }
                    }
                    else if (nXscaledArea <= nMaxResizedArea)
                    {
                        Resize(nMaxWidth, 0);
                    }
                    else
                    {
                        Resize(0, nMaxHeight);
                    }

                    // If oImg.Height = oImg.Width Then
                    // If nMaxWidth > nMaxHeight Then
                    // Resize(0, nMaxHeight)
                    // Else
                    // Resize(nMaxWidth, 0)
                    // End If
                    // ElseIf oImg.Height > oImg.Width Then
                    // Resize(0, nMaxHeight)
                    // Else
                    // Resize(nMaxWidth, 0)
                    // End If

                }
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ResizeMax", ex, ""));
            }
        }

        private System.Drawing.Image ImageResize(System.Drawing.Image oImage, int nHeight, int nWidth)
        {
            // does the actual resize
            try
            {
                oSourceImg = new Bitmap(oImage);
                oImg = new Bitmap(nWidth, nHeight);
                oGraphics = Graphics.FromImage(oImg);
                // GFX Additions--
                // Trev
                // oGraphics.Clear(Color.White)
                oGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                oGraphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                // NB
                oGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                oGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                // --
                oGraphics.DrawImage(oSourceImg, 0, 0, oImg.Width, oImg.Height);
                oImage = oImg;
                // Added--
                if (bCrop)
                    oImage = CropImage(oImage);
                // -------
                return oImage;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ImageResize", ex, ""));
                return oImage;
            }

        }

        public ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            // gets jpg encoder info
            try
            {
                int j;
                ImageCodecInfo[] encoders;
                encoders = ImageCodecInfo.GetImageEncoders();
                var loopTo = encoders.Length - 1;
                for (j = 0; j <= loopTo; j++)
                {
                    if (encoders[j].MimeType == mimeType)
                        return encoders[j];
                }
                return default;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetEncoderInfo", ex, ""));
                return default;
            }
        }

        private bool SaveJPGWithCompressionSetting(System.Drawing.Image theImg, string szFileName, long compression, string serverPath = "")
        {
            // save the image
            string cProcessInfo = "";
            try
            {
                if (theImg != null)
                {
                    if (!string.IsNullOrEmpty(serverPath))
                    {
                        if (Directory.Exists(serverPath) == false)
                        {
                            Directory.CreateDirectory(serverPath);
                        }
                    }

                    if (szFileName.EndsWith(".gif"))
                    {

                        // Save to memory using the Png format
                        //  MemoryStream ms = new MemoryStream();
                        //   theImg.Save(ms, ImageFormat.Png);
                        //   theImg = new Bitmap(ms);             
                        //    theImg.Save(Strings.Replace(szFileName, ".gif", ".png"));
                        //    var imgFile = new FileInfo(Strings.Replace(szFileName, ".gif", ".png"));

                        theImg.Save(Strings.Replace(szFileName, ".gif", ".png"));

                        var imgFile = new FileInfo(szFileName);

                        if (compression == 100L)
                        {
                            CompressImage(imgFile, true);
                        }
                        else
                        {
                            CompressImage(imgFile, false);
                        }
                    }

                    else if (szFileName.EndsWith(".png"))
                    {
                        theImg.Save(szFileName, ImageFormat.Png);

                        var imgFile = new FileInfo(szFileName);
                        if (compression == 100L)
                        {
                            CompressImage(imgFile, true);
                        }
                        else
                        {
                            CompressImage(imgFile, false);
                        }
                    }

                    else
                    {
                        var eps = new EncoderParameters(1);
                        eps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
                        string cEncoder = "image/jpeg";
                        var ici = GetEncoderInfo(cEncoder);

                        if (File.Exists(szFileName))
                        {
                            File.Delete(szFileName);
                        }

                        try
                        {
                            if (theImg != null)
                            {
                                // TS Added to avoid GDI+ Errors

                                var newImg = new Bitmap(theImg);
                                theImg.Dispose();
                                theImg = default;
                                newImg.Save(szFileName, ici, eps);
                                newImg.Dispose();

                                // theImg.Save(szFileName, ici, eps);


                            }
                        }

                        catch (Exception)
                        {

                            File.Delete(szFileName);

                            var newImg = new Bitmap(theImg);
                            theImg.Dispose();
                            theImg = default;
                            newImg.Save(szFileName, ici, eps);
                            newImg.Dispose();

                        }

                        var imgFile = new FileInfo(szFileName);
                        if (compression == 100)
                        {
                            CompressImage(imgFile, true);
                        }
                        else
                        {
                            CompressImage(imgFile, false);
                        }
                        imgFile.Refresh();

                    }//cProcessInfo = cProcessInfo;
                    return true;
                }
                else {
                    return false;
                }
                
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "SaveJPGWithCompressionSetting", ex, cProcessInfo));
                return false;
            }
            finally
            {
                if (theImg != null)
                {
                    theImg.Dispose();
                }
                Close();
            }
        }

        public async void TinyCompress(string filepathFrom, string filepathTo)
        {
            string cProcessInfo = "";
            try
            {
                Tinify.Key = TinifyKey;
                try
                {

                    bool bIsValid = Tinify.Validate().GetAwaiter().GetResult();
                    if (bIsValid == true)
                    {
                        cProcessInfo = "Key Validation Succeeded";
                    }
                }
                catch
                {
                    cProcessInfo = "Key Validation Failed";
                }

                var compressionsThisMonth = TinifyAPI.Tinify.CompressionCount;
                Task<TinifyAPI.Source> tinifyImg = TinifyAPI.Tinify.FromFile(filepathFrom);
                var newImage = tinifyImg.GetAwaiter().GetResult();
                if (newImage != null)
                {
                    newImage.ToFile(filepathTo).GetAwaiter().GetResult();
                }
                else
                {
                    cProcessInfo = "Compression Failed" + filepathFrom;
                }
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "TinyCompress", ex, cProcessInfo));
            }


        }

        public long CompressImage(FileInfo imgfileInfo, bool lossless, short Quality = 0, string fileSuffix = "")
        {
            var difference = default(long);
            try
            {
                string ext = Strings.LCase(imgfileInfo.Extension);
                if (ext == ".jpg" | ext == ".jpeg" | ext == ".png" | ext == ".gif" | ext == ".webp")
                {
                    if (!string.IsNullOrEmpty(TinifyKey))
                    {

                        string NewFileName = imgfileInfo.FullName.Replace(ext, fileSuffix + ext);
                        long initialFileSize = imgfileInfo.Length;

                        TinyCompress(imgfileInfo.FullName, NewFileName);

                        imgfileInfo.Refresh();
                        difference = initialFileSize - imgfileInfo.Length;
                    }

                    else
                    {

                        // Compress the File using ImageMagick
                        switch (Strings.LCase(imgfileInfo.Extension) ?? "")
                        {
                            case ".gif":
                                {

                                    difference = imgfileInfo.Length;
                                    var optimizer = new ImageMagick.ImageOptimizers.GifOptimizer();
                                    if (lossless)
                                    {
                                        optimizer.LosslessCompress(imgfileInfo);
                                    }
                                    else
                                    {
                                        // optimizer.LosslessCompress = True
                                        try
                                        {
                                            optimizer.Compress(imgfileInfo);
                                        }
                                        catch (Exception ex)
                                        {
                                            optimizer = default;
                                            OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CompressImage", ex, ""));
                                        }
                                    }
                                    imgfileInfo.Refresh();
                                    difference = difference - imgfileInfo.Length;
                                    optimizer = default;
                                    break;
                                }
                            case ".png":
                                {
                                    string NewFileName = imgfileInfo.FullName.Replace(".png", fileSuffix + ".png");
                                    if (Quality > 0)
                                    {
                                        using (var mi = new ImageMagick.MagickImage(imgfileInfo.FullName, ImageMagick.MagickFormat.Png))
                                        {
                                            mi.Format = mi.Format; // Get Or Set the format Of the image.
                                            mi.Quality = Quality;
                                            mi.Write(NewFileName);
                                        }
                                    }
                                    var newImgFile = new FileInfo(NewFileName);
                                    difference = imgfileInfo.Length;
                                    var optimizer = new ImageMagick.ImageOptimizers.PngOptimizer();
                                    if (lossless)
                                    {
                                        optimizer.LosslessCompress(newImgFile);
                                    }
                                    else
                                    {
                                        optimizer.OptimalCompression = true;
                                        try
                                        {
                                            optimizer.Compress(newImgFile);
                                        }
                                        catch (Exception ex)
                                        {
                                            optimizer = default;
                                            OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CompressImage", ex, ""));
                                        }
                                    }
                                    newImgFile.Refresh();
                                    difference = difference - newImgFile.Length;
                                    optimizer = default;
                                    break;
                                }
                            case ".jpg":
                            case ".jpeg":
                                {

                                    string NewFileName = imgfileInfo.FullName.Replace(".jpg", fileSuffix + ".jpg");
                                    difference = imgfileInfo.Length;

                                    if (Quality > 0)
                                    {
                                        using (var mi = new ImageMagick.MagickImage(imgfileInfo.FullName, ImageMagick.MagickFormat.Jpg))
                                        {
                                            mi.Format = mi.Format; // Get Or Set the format Of the image.
                                            mi.Quality = Quality;
                                            mi.Write(NewFileName);
                                        }
                                    }
                                    var newImgFile = new FileInfo(NewFileName);

                                    var optimizer = new ImageMagick.ImageOptimizers.JpegOptimizer();
                                    if (lossless)
                                    {
                                        optimizer.LosslessCompress(newImgFile);
                                    }
                                    else
                                    {
                                        optimizer.OptimalCompression = true;
                                        optimizer.Progressive = true;

                                        try
                                        {
                                            optimizer.Compress(newImgFile);
                                        }

                                        catch (Exception ex)
                                        {
                                            optimizer = default;
                                            OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CompressImage", ex, ""));
                                        }
                                    }

                                    newImgFile.Refresh();
                                    difference = difference - newImgFile.Length;

                                    newImgFile = null;
                                    optimizer = default;
                                    break;
                                }
                            case ".webp":
                                {
                                    break;
                                }

                        }

                    }
                }

                return difference;
            }

            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CompressImage", ex, ""));
                return 0L;
            }
        }


        private System.Drawing.Image CropImage(System.Drawing.Image oImage)
        {
            try
            {
                GraphicsUnit pixel = GraphicsUnit.Pixel;
                var oBitmapOrig = new Bitmap(oImage);
                RectangleF sRect = oBitmapOrig.GetBounds(ref pixel);
                var oBitmapCrop = new Bitmap(nMaxWidthCrop, nMaxHeightCrop);
                Graphics oGraphics = Graphics.FromImage(oBitmapCrop);
                oGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                oGraphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                oGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                oGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                if (oImage.Width == nMaxWidthCrop & nMaxHeightCrop > 0)
                {
                    // If the Width is perfect, crop the Height
                    int nNewY = (oImage.Height - nMaxHeightCrop) / 2;
                    sRect.Inflate(-0, (nNewY * -1));
                    oGraphics.DrawImage(oBitmapOrig, 0, 0, sRect, GraphicsUnit.Pixel);
                }
                else if (nMaxWidthCrop > 0)
                {
                    // Else crop the width
                    int nNewW = (oImage.Width - nMaxWidthCrop) / 2;
                    sRect.Inflate((nNewW * -1), -0);
                    oGraphics.DrawImage(oBitmapOrig, 0, 0, sRect, GraphicsUnit.Pixel);
                }
                oImage = oBitmapCrop;
                return oImage;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CropImage", ex, ""));
                return oImage;
            }
        }

        private System.Drawing.Image AddReflection(Color _BackgroundColor, int _Reflectivity)
        {

            System.Drawing.Image _image = oImg;
            // Calculate the size of the new image
            int height =Convert.ToInt32(_image.Height + _image.Height * (_Reflectivity / 255f));
            var newImage = new Bitmap(_image.Width, height, PixelFormat.Format24bppRgb);
            newImage.SetResolution(_image.HorizontalResolution, _image.VerticalResolution);

            using (Graphics graphics__1 = Graphics.FromImage(newImage))
            {
                // Initialize main graphics buffer
                graphics__1.Clear(_BackgroundColor);
                graphics__1.DrawImage(_image, new Point(0, 0));
                graphics__1.InterpolationMode = InterpolationMode.HighQualityBicubic;
                var destinationRectangle = new Rectangle(0, _image.Size.Height, _image.Size.Width, _image.Size.Height);

                // Prepare the reflected image
                int reflectionHeight = _image.Height * _Reflectivity / 255;
                System.Drawing.Image reflectedImage = new Bitmap(_image.Width, reflectionHeight);

                // Draw just the reflection on a second graphics buffer
                using (Graphics gReflection = Graphics.FromImage(reflectedImage))
                {
                    gReflection.DrawImage(_image, new Rectangle(0, 0, reflectedImage.Width, reflectedImage.Height), 0, _image.Height - reflectedImage.Height, reflectedImage.Width, reflectedImage.Height, GraphicsUnit.Pixel);
                }
                reflectedImage.RotateFlip(RotateFlipType.RotateNoneFlipY);
                var imageRectangle = new Rectangle(destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, (int)Math.Round(destinationRectangle.Height * _Reflectivity / 255d));

                // Draw the image on the original graphics
                graphics__1.DrawImage(reflectedImage, imageRectangle);

                // Finish the reflection using a gradiend brush
                var brush = new LinearGradientBrush(imageRectangle, Color.FromArgb(255 - _Reflectivity, _BackgroundColor), _BackgroundColor, 90, false);
                graphics__1.FillRectangle(brush, imageRectangle);
            }

            oImg = newImage;
            return oImg;

        }

        private System.Drawing.Image AddWatermark(System.Drawing.Image imgPhoto, string _WatermarkText, string _WatermarkImgPath)
        {

            try
            {

                int phWidth = imgPhoto.Width;
                int phHeight = imgPhoto.Height;

                var bmPhoto = new Bitmap(phWidth, phHeight, PixelFormat.Format24bppRgb);
                bmPhoto.SetResolution(72, 72);

                Graphics grPhoto = Graphics.FromImage(bmPhoto);

                grPhoto.SmoothingMode = SmoothingMode.AntiAlias;
                grPhoto.DrawImage(imgPhoto, new Rectangle(0, 0, phWidth, phHeight), 0, 0, phWidth, phHeight, GraphicsUnit.Pixel);


                // Write the Watermark Text centred at the bottom of the image
                // Check the font size
                int[] sizes = new int[] { 48, 24, 20, 16, 14, 12, 10, 8, 6, 4 };
                Font crFont = default;
                var crSize = new SizeF();
                for (int i = 0; i <= 9; i++)
                {
                    crFont = new Font("arial", sizes[i], FontStyle.Bold);
                    crSize = grPhoto.MeasureString(_WatermarkText, crFont);

                    if ((ushort)Math.Round(crSize.Width) < (ushort)Math.Round(phWidth * 0.66d))
                    {
                        break;
                    }
                }
                // Place at the bottom
                int yPixlesFromBottom = (int)Math.Round(Math.Truncate(phHeight * 0.05d));
                float yPosFromBottom = phHeight - yPixlesFromBottom - crSize.Height / 2f;
                float xCenterOfImg = (float)(phWidth / 2d);

                // Write the text
                var StrFormat = new StringFormat();
                StrFormat.Alignment = StringAlignment.Center;

                var semiTransBrush2 = new SolidBrush(Color.FromArgb(153, 0, 0, 0));

                grPhoto.DrawString(_WatermarkText, crFont, semiTransBrush2, new PointF(xCenterOfImg + 1f, yPosFromBottom + 1f), StrFormat);

                var semiTransBrush = new SolidBrush(Color.FromArgb(153, 255, 255, 255));

                grPhoto.DrawString(_WatermarkText, crFont, semiTransBrush, new PointF(xCenterOfImg, yPosFromBottom), StrFormat);

                // Now add the image watermark
                if (!string.IsNullOrEmpty(_WatermarkImgPath))
                {

                    System.Drawing.Image imgWatermark = System.Drawing.Image.FromFile(_WatermarkImgPath);
                    int wmWidth = imgWatermark.Width;
                    int wmHeight = imgWatermark.Height;

                    var bmWatermark = new Bitmap(bmPhoto);
                    bmWatermark.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

                    Graphics grWatermark = Graphics.FromImage(bmWatermark);


                    var imageAttributes = new ImageAttributes();
                    var colorMap = new ColorMap();

                    colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
                    colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
                    ColorMap[] remapTable = new[] { colorMap };

                    imageAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

                    float[][] colorMatrixElements = new[] { new float[] { 1.0f, 0f, 0f, 0f, 0f }, new float[] { 0f, 1.0f, 0f, 0f, 0f }, new float[] { 0f, 0f, 1.0f, 0f, 0f }, new float[] { 0f, 0f, 0f, 0.3f, 0f }, new float[] { 0f, 0f, 0f, 0f, 1.0f } };

                    var wmColorMatrix = new ColorMatrix(colorMatrixElements);

                    imageAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    int xPosOfWm = phWidth - wmWidth - 10;
                    int yPosOfWm = 10;

                    grWatermark.DrawImage(imgWatermark, new Rectangle(xPosOfWm, yPosOfWm, wmWidth, wmHeight), 0, 0, wmWidth, wmHeight, GraphicsUnit.Pixel, imageAttributes);

                    oImg = bmWatermark;


                    grWatermark.Dispose();
                }
                else
                {
                    oImg = bmPhoto;
                }
                Save(cLocation);
                grPhoto.Dispose();
                return oImg;                
            }

            catch (Exception)
            {
                return imgPhoto;
            }

        }

        public System.Drawing.Image AddWatermark(string _WatermarkText, string _WatermarkImgPath)
        {
            try
            {
                int phWidth = oImg.Width;
                int phHeight = oImg.Height;

                Bitmap bmPhoto = new Bitmap(phWidth, phHeight, PixelFormat.Format24bppRgb);
                bmPhoto.SetResolution(72, 72);

                Graphics grPhoto = Graphics.FromImage(bmPhoto);

                grPhoto.SmoothingMode = SmoothingMode.AntiAlias;
                grPhoto.DrawImage(oImg, new Rectangle(0, 0, phWidth, phHeight), 0, 0, phWidth, phHeight, GraphicsUnit.Pixel);


                // Write the Watermark Text centred at the bottom of the image
                // Check the font size
                int[] sizes = new int[] { 48, 24, 20, 16, 14, 12, 10, 8, 6, 4 };
                Font crFont = null/* TODO Change to default(_) if this is not a reference type */;
                SizeF crSize = new SizeF();
                for (int i = 0; i <= 9; i++)
                {
                    crFont = new Font("arial", sizes[i], FontStyle.Bold);
                    crSize = grPhoto.MeasureString(_WatermarkText, crFont);

                    if (System.Convert.ToUInt16(crSize.Width) < System.Convert.ToUInt16(phWidth * 0.66))
                        break;
                }
                // Place at the bottom
                int yPixlesFromBottom = System.Convert.ToInt32(Math.Truncate(phHeight * 0.05));
                float yPosFromBottom =(float) ((phHeight - yPixlesFromBottom) - (crSize.Height / (double)2));
                float xCenterOfImg = (float)(phWidth / (double)2);

                // Write the text
                StringFormat StrFormat = new StringFormat();
                StrFormat.Alignment = StringAlignment.Center;

                SolidBrush semiTransBrush2 = new SolidBrush(Color.FromArgb(153, 0, 0, 0));

                grPhoto.DrawString(_WatermarkText, crFont, semiTransBrush2, new PointF(xCenterOfImg + 1, yPosFromBottom + 1), StrFormat);

                SolidBrush semiTransBrush = new SolidBrush(Color.FromArgb(153, 255, 255, 255));

                grPhoto.DrawString(_WatermarkText, crFont, semiTransBrush, new PointF(xCenterOfImg, yPosFromBottom), StrFormat);

                // Now add the image watermark
                if (_WatermarkImgPath != "")
                {
                    System.Drawing.Image imgWatermark = System.Drawing.Image.FromFile(_WatermarkImgPath);
                    int wmWidth = imgWatermark.Width;
                    int wmHeight = imgWatermark.Height;

                    Bitmap bmWatermark = new Bitmap(bmPhoto);
                    bmWatermark.SetResolution(oImg.HorizontalResolution, oImg.VerticalResolution);

                    Graphics grWatermark = Graphics.FromImage(bmWatermark);


                    ImageAttributes imageAttributes = new ImageAttributes();
                    ColorMap colorMap = new ColorMap();

                    colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
                    colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
                    ColorMap[] remapTable = new[] { colorMap };

                    imageAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

                    float[][] colorMatrixElements = new[] { new float[] { 1.0F, 0F, 0F, 0F, 0F }, new float[] { 0F, 1.0F, 0F, 0F, 0F }, new float[] { 0F, 0F, 1.0F, 0F, 0F }, new float[] { 0F, 0F, 0F, 1.0F, 0F }, new float[] { 0F, 0F, 0F, 0F, 1.0F } };

                    ColorMatrix wmColorMatrix = new ColorMatrix(colorMatrixElements);

                    imageAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    int xPosOfWm = 0;
                    int yPosOfWm = 0;

                    grWatermark.DrawImage(imgWatermark, new Rectangle(xPosOfWm, yPosOfWm, wmWidth, wmHeight), 0, 0, wmWidth, wmHeight, GraphicsUnit.Pixel, imageAttributes);

                    oImg = bmWatermark;


                    grWatermark.Dispose();
                }
                else
                    oImg = bmPhoto;
                grPhoto.Dispose();
                // Save(cLocation)
                return oImg;
                
            }
            catch (Exception)
            {
                return oImg;
            }
        }


        public System.Drawing.Image AddWatermark(Bitmap oImg, string _WatermarkText, string _WatermarkImgPath)
        {

            try
            {

                int phWidth = oImg.Width;
                int phHeight = oImg.Height;

                var bmPhoto = new Bitmap(phWidth, phHeight, PixelFormat.Format24bppRgb);
                bmPhoto.SetResolution(72, 72);

                Graphics grPhoto = Graphics.FromImage(bmPhoto);

                grPhoto.SmoothingMode = SmoothingMode.AntiAlias;
                grPhoto.DrawImage(oImg, new Rectangle(0, 0, phWidth, phHeight), 0, 0, phWidth, phHeight, GraphicsUnit.Pixel);


                // Write the Watermark Text centred at the bottom of the image
                // Check the font size
                int[] sizes = new int[] { 48, 24, 20, 16, 14, 12, 10, 8, 6, 4 };
                Font crFont = default;
                var crSize = new SizeF();
                for (int i = 0; i <= 9; i++)
                {
                    crFont = new Font("arial", sizes[i], FontStyle.Bold);
                    crSize = grPhoto.MeasureString(_WatermarkText, crFont);

                    if ((ushort)Math.Round(crSize.Width) < (ushort)Math.Round(phWidth * 0.66d))
                    {
                        break;
                    }
                }
                // Place at the bottom
                int yPixlesFromBottom = (int)Math.Round(Math.Truncate(phHeight * 0.05d));
                float yPosFromBottom = phHeight - yPixlesFromBottom - crSize.Height / 2f;
                float xCenterOfImg = (float)(phWidth / 2d);

                // Write the text
                var StrFormat = new StringFormat();
                StrFormat.Alignment = StringAlignment.Center;

                var semiTransBrush2 = new SolidBrush(Color.FromArgb(153, 0, 0, 0));

                grPhoto.DrawString(_WatermarkText, crFont, semiTransBrush2, new PointF(xCenterOfImg + 1f, yPosFromBottom + 1f), StrFormat);

                var semiTransBrush = new SolidBrush(Color.FromArgb(153, 255, 255, 255));

                grPhoto.DrawString(_WatermarkText, crFont, semiTransBrush, new PointF(xCenterOfImg, yPosFromBottom), StrFormat);

                // Now add the image watermark
                if (!string.IsNullOrEmpty(_WatermarkImgPath))
                {

                    System.Drawing.Image imgWatermark = System.Drawing.Image.FromFile(_WatermarkImgPath);
                    int wmWidth = imgWatermark.Width;
                    int wmHeight = imgWatermark.Height;

                    var bmWatermark = new Bitmap(bmPhoto);
                    bmWatermark.SetResolution(oImg.HorizontalResolution, oImg.VerticalResolution);

                    Graphics grWatermark = Graphics.FromImage(bmWatermark);


                    var imageAttributes = new ImageAttributes();
                    var colorMap = new ColorMap();

                    colorMap.OldColor = Color.FromArgb(255, 0, 255, 0);
                    colorMap.NewColor = Color.FromArgb(0, 0, 0, 0);
                    ColorMap[] remapTable = new[] { colorMap };

                    imageAttributes.SetRemapTable(remapTable, ColorAdjustType.Bitmap);

                    float[][] colorMatrixElements = new[] { new float[] { 1.0f, 0f, 0f, 0f, 0f }, new float[] { 0f, 1.0f, 0f, 0f, 0f }, new float[] { 0f, 0f, 1.0f, 0f, 0f }, new float[] { 0f, 0f, 0f, 1.0f, 0f }, new float[] { 0f, 0f, 0f, 0f, 1.0f } };

                    var wmColorMatrix = new ColorMatrix(colorMatrixElements);

                    imageAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    int xPosOfWm = 0;
                    int yPosOfWm = 0;

                    grWatermark.DrawImage(imgWatermark, new Rectangle(xPosOfWm, yPosOfWm, wmWidth, wmHeight), 0, 0, wmWidth, wmHeight, GraphicsUnit.Pixel, imageAttributes);

                    oImg = bmWatermark;


                    grWatermark.Dispose();
                }
                else
                {
                    oImg = bmPhoto;
                }
                // Save(cLocation)
                grPhoto.Dispose();
                return oImg;
                
            }

            catch (Exception)
            {
                return oImg;
            }

        }

        //private string[] _RandomFontFamily_ff = default;

        #endregion

        /// <summary>
        /// CAPTCHA image generation class
        /// </summary>
        /// <remarks>
        /// Adapted from the excellent code at 
        /// http://www.codeproject.com/aspnet/CaptchaImage.asp
        /// 
        /// Jeff Atwood
        /// http://www.codinghorror.com/
        /// </remarks>
        public partial class CaptchaImage
        {

            private int _height;
            private int _width;
            private Random _rand;
            private DateTime _generatedAt;
            private string _randomText;
            private int _randomTextLength;
            private string _randomTextChars;
            private string _fontFamilyName;
            private FontWarpFactor _fontWarp;
            private BackgroundNoiseLevel _backgroundNoise;
            private LineNoiseLevel _lineNoise;
            private string _guid;
            private string _fontWhitelist= "arial;arial black;comic sans ms;courier new;" + "lucida console;lucida sans unicode;microsoft sans serif;" + "tahoma;times new roman;trebuchet ms;verdana";

            public event OnErrorEventHandler OnError;

            public delegate void OnErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);
            private const string mcModuleName = "Protean.Tools.Image.CaptchaImage";

            #region   Public Enums

            /// <summary>
            /// Amount of random font warping to apply to rendered text
            /// </summary>
            public enum FontWarpFactor
            {
                None,
                Low,
                Medium,
                High,
                Extreme
            }

            /// <summary>
            /// Amount of background noise to add to rendered image
            /// </summary>
            public enum BackgroundNoiseLevel
            {
                None,
                Low,
                Medium,
                High,
                Extreme
            }

            /// <summary>
            /// Amount of curved line noise to add to rendered image
            /// </summary>
            public enum LineNoiseLevel
            {
                None,
                Low,
                Medium,
                High,
                Extreme
            }

            #endregion

            #region   Public Properties

            /// <summary>
            /// Returns a GUID that uniquely identifies this Captcha
            /// </summary>
            public string UniqueId
            {
                get
                {
                    return _guid;
                }
            }

            /// <summary>
            /// Returns the date and time this image was last rendered
            /// </summary>
            public DateTime RenderedAt
            {
                get
                {
                    return _generatedAt;
                }
            }

            /// <summary>
            /// Font family to use when drawing the Captcha text. If no font is provided, a random font will be chosen from the font whitelist for each character.
            /// </summary>
            public string Font
            {
                get
                {
                    return _fontFamilyName;
                }
                set
                {
                    try
                    {
                        var font1 = new Font(value, 12.0f);
                        _fontFamilyName = value;
                        font1.Dispose();
                    }
                    catch (Exception)
                    {
                        _fontFamilyName = System.Drawing.FontFamily.GenericSerif.Name;
                    }
                }
            }

            /// <summary>
            /// Amount of random warping to apply to the Captcha text.
            /// </summary>
            public FontWarpFactor FontWarp
            {
                get
                {
                    return _fontWarp;
                }
                set
                {
                    _fontWarp = value;
                }
            }

            /// <summary>
            /// Amount of background noise to apply to the Captcha image.
            /// </summary>
            public BackgroundNoiseLevel BackgroundNoise
            {
                get
                {
                    return _backgroundNoise;
                }
                set
                {
                    _backgroundNoise = value;
                }
            }

            public LineNoiseLevel LineNoise
            {
                get
                {
                    return _lineNoise;
                }
                set
                {
                    _lineNoise = value;
                }
            }

            /// <summary>
            /// A string of valid characters to use in the Captcha text. 
            /// A random character will be selected from this string for each character.
            /// </summary>
            public string TextChars
            {
                get
                {
                    return _randomTextChars;
                }
                set
                {
                    _randomTextChars = value;
                    _randomText = GenerateRandomText();
                }
            }

            /// <summary>
            /// Number of characters to use in the Captcha text. 
            /// </summary>
            public int TextLength
            {
                get
                {
                    return _randomTextLength;
                }
                set
                {
                    _randomTextLength = value;
                    _randomText = GenerateRandomText();
                }
            }

            /// <summary>
            /// Returns the randomly generated Captcha text.
            /// </summary>
            public string Text
            {
                get
                {
                    return _randomText;
                }
            }

            /// <summary>
            /// Width of Captcha image to generate, in pixels 
            /// </summary>
            public int Width
            {
                get
                {
                    return _width;
                }
                set
                {
                    if (value <= 60)
                    {
                        throw new ArgumentOutOfRangeException("width", value, "width must be greater than 60.");
                    }
                    _width = value;
                }
            }

            /// <summary>
            /// Height of Captcha image to generate, in pixels 
            /// </summary>
            public int Height
            {
                get
                {
                    return _height;
                }
                set
                {
                    if (value <= 30)
                    {
                        throw new ArgumentOutOfRangeException("height", value, "height must be greater than 30.");
                    }
                    _height = value;
                }
            }

            /// <summary>
            /// A semicolon-delimited list of valid fonts to use when no font is provided.
            /// </summary>
            public string FontWhitelist
            {
                get
                {
                    return _fontWhitelist;
                }
                set
                {
                    _fontWhitelist = value;
                }
            }

            #endregion

            public CaptchaImage()
            {
                try
                {
                    _rand = new Random();
                    _fontWarp = FontWarpFactor.Medium;
                    _backgroundNoise = BackgroundNoiseLevel.Low;
                    _lineNoise = LineNoiseLevel.Low;
                    _width = 180;
                    _height = 40;
                    _randomTextLength = 5;
                    _randomTextChars = "ACDEFGHJKLNPQRTUVXYZ2346789";
                    _fontFamilyName = "";
                    // -- a list of known good fonts in on both Windows XP and Windows Server 2003
                    _fontWhitelist = "arial;arial black;comic sans ms;courier new;" + "lucida console;lucida sans unicode;microsoft sans serif;" + "tahoma;times new roman;trebuchet ms;verdana";


                    _randomText = GenerateRandomText();
                    _generatedAt = DateTime.Now;
                    _guid = Guid.NewGuid().ToString();
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""));
                }
            }

            /// <summary>
            /// Forces a new Captcha image to be generated using current property value settings.
            /// </summary>
            public Bitmap RenderImage()
            {
                return GenerateImagePrivate();
            }

            /// <summary>
            /// Returns a random font family from the font whitelist
            /// </summary>
            private string RandomFontFamily()
            {
                try
                {
                    string[] ff = null;
                   
                    // -- small optimization so we don't have to split for each char
                    if(_fontWhitelist != string.Empty)
                    {
                        ff = _fontWhitelist.Split(';');                        
                    }
                    if (ff != null)
                    {
                        return ff[_rand.Next(0, ff.Length - 1)];
                    }
                    else
                    {
                        return null;
                    }

                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "RandomFontFamily", ex, ""));
                    return null;
                }
            }

            /// <summary>
            /// generate random text for the CAPTCHA
            /// </summary>
            private string GenerateRandomText()
            {
                try
                {
                    // Dim sb As New System.Text.StringBuilder(_randomTextLength)
                    // Dim maxLength As Integer = _randomTextChars.Length
                    // For n As Integer = 0 To _randomTextLength - 1
                    // sb.Append(_randomTextChars.Substring(_rand.Next(maxLength), 1))
                    // Next
                    // Return sb.ToString
                    bool lowercase = true;
                    long size = _randomTextLength;

                    TextOptions options = TextOptions.UseAlpha | TextOptions.UseNumeric | TextOptions.UnambiguousCharacters;

                    if (lowercase)
                        options = options | TextOptions.LowerCase;
                    return Protean.Tools.Text.RandomPassword(Convert.ToInt32(size), _randomTextChars, options);
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GenerateRandomText", ex, ""));
                    return null;
                }
            }

            /// <summary>
            /// Returns a random point within the specified x and y ranges
            /// </summary>
            private PointF RandomPoint(int xmin, int xmax, ref int ymin, ref int ymax)
            {
                return new PointF(_rand.Next(xmin, xmax), _rand.Next(ymin, ymax));
            }

            /// <summary>
            /// Returns a random point within the specified rectangle
            /// </summary>
            private PointF RandomPoint(Rectangle rect)
            {
                int argymin = rect.Top;
                int argymax = rect.Bottom;
                return RandomPoint(rect.Left, rect.Width, ref argymin, ref argymax);
            }

            /// <summary>
            /// Returns a GraphicsPath containing the specified string and font
            /// </summary>
            private GraphicsPath TextPath(string s, Font f, Rectangle r)
            {
                var sf = new StringFormat();
                sf.Alignment = StringAlignment.Near;
                sf.LineAlignment = StringAlignment.Near;
                var gp = new GraphicsPath();
                gp.AddString(s, f.FontFamily, (int)f.Style, f.Size, r, sf);
                return gp;
            }

            /// <summary>
            /// Returns the CAPTCHA font in an appropriate size 
            /// </summary>
            private Font GetFont()
            {
                try
                {
                    var fsize = default(float);
                    string fname = _fontFamilyName;
                    if (string.IsNullOrEmpty(fname))
                    {
                        fname = RandomFontFamily();
                    }
                    switch (FontWarp)
                    {
                        case FontWarpFactor.None:
                            {
                                fsize = Convert.ToInt32(_height * 0.7d);
                                break;
                            }
                        case FontWarpFactor.Low:
                            {
                                fsize = Convert.ToInt32(_height * 0.8d);
                                break;
                            }
                        case FontWarpFactor.Medium:
                            {
                                fsize = Convert.ToInt32(_height * 0.85d);
                                break;
                            }
                        case FontWarpFactor.High:
                            {
                                fsize = Convert.ToInt32(_height * 0.9d);
                                break;
                            }
                        case FontWarpFactor.Extreme:
                            {
                                fsize = Convert.ToInt32(_height * 0.95d);
                                break;
                            }
                    }
                    return new Font(fname, fsize, FontStyle.Bold);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetFont", ex, ""));
                    return default;
                }
            }

            /// <summary>
            /// Renders the CAPTCHA image
            /// </summary>
            private Bitmap GenerateImagePrivate()
            {
                try
                {

                    Font fnt = default;
                    Rectangle rect;
                    Brush br;
                    var bmp = new Bitmap(_width, _height, PixelFormat.Format32bppArgb);
                    Graphics gr = Graphics.FromImage(bmp);
                    gr.SmoothingMode = SmoothingMode.AntiAlias;

                    // -- fill an empty white rectangle
                    rect = new Rectangle(0, 0, _width, _height);
                    br = new SolidBrush(Color.White);
                    gr.FillRectangle(br, rect);

                    int charOffset = 0;
                    double charWidth = _width / (double)_randomTextLength;
                    Rectangle rectChar;

                    foreach (char c in _randomText)
                    {
                        // -- establish font and draw area
                        fnt = GetFont();
                        rectChar = new Rectangle(Convert.ToInt32(charOffset * charWidth), 0, Convert.ToInt32(charWidth), _height);

                        // -- warp the character
                        var gp = TextPath(c.ToString(), fnt, rectChar);
                        WarpText(gp, rectChar);

                        // -- draw the character
                        br = new SolidBrush(Color.Black);
                        gr.FillPath(br, gp);

                        charOffset += 1;
                    }

                    AddNoise(gr, rect);
                    AddLine(gr, rect);

                    // -- clean up unmanaged resources
                    fnt.Dispose();
                    br.Dispose();
                    gr.Dispose();

                    return bmp;
                }

                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""));
                    return default;
                }
            }


            /// <summary>
            /// Warp the provided text GraphicsPath by a variable amount
            /// </summary>
            private void WarpText(GraphicsPath textPath, Rectangle rect)
            {
                var WarpDivisor = default(float);
                var RangeModifier = default(float);

                switch (_fontWarp)
                {
                    case FontWarpFactor.None:
                        {
                            return;
                        }
                    case FontWarpFactor.Low:
                        {
                            WarpDivisor = 6f;
                            RangeModifier = 1f;
                            break;
                        }
                    case FontWarpFactor.Medium:
                        {
                            WarpDivisor = 5f;
                            RangeModifier = 1.3f;
                            break;
                        }
                    case FontWarpFactor.High:
                        {
                            WarpDivisor = 4.5f;
                            RangeModifier = 1.4f;
                            break;
                        }
                    case FontWarpFactor.Extreme:
                        {
                            WarpDivisor = 4f;
                            RangeModifier = 1.5f;
                            break;
                        }
                }

                RectangleF rectF;
                rectF = new RectangleF(Convert.ToSingle(rect.Left), 0f, Convert.ToSingle(rect.Width), rect.Height);

                int hrange = Convert.ToInt32(rect.Height / WarpDivisor);
                int wrange = Convert.ToInt32(rect.Width / WarpDivisor);
                int left = rect.Left - Convert.ToInt32(wrange * RangeModifier);
                int top = rect.Top - Convert.ToInt32(hrange * RangeModifier);
                int width = rect.Left + rect.Width + Convert.ToInt32(wrange * RangeModifier);
                int height = rect.Top + rect.Height + Convert.ToInt32(hrange * RangeModifier);

                if (left < 0)
                    left = 0;
                if (top < 0)
                    top = 0;
                if (width > Width)
                    width = Width;
                if (height > Height)
                    height = Height;

                int argymax = top + hrange;
                var leftTop = RandomPoint(left, left + wrange, ref top, ref argymax);
                int argymax1 = top + hrange;
                var rightTop = RandomPoint(width - wrange, width, ref top, ref argymax1);
                int argymin = height - hrange;
                var leftBottom = RandomPoint(left, left + wrange, ref argymin, ref height);
                int argymin1 = height - hrange;
                var rightBottom = RandomPoint(width - wrange, width, ref argymin1, ref height);

                PointF[] points = new PointF[] { leftTop, rightTop, leftBottom, rightBottom };
                var m = new Matrix();
                m.Translate(0, 0);
                textPath.Warp(points, rectF, m, WarpMode.Perspective, 0);
            }


            /// <summary>
            /// Add a variable level of graphic noise to the image
            /// </summary>
            private void AddNoise(Graphics graphics1, Rectangle rect)
            {
                var density = default(int);
                var size = default(int);

                switch (_backgroundNoise)
                {
                    case BackgroundNoiseLevel.None:
                        {
                            return;
                        }
                    case BackgroundNoiseLevel.Low:
                        {
                            density = 30;
                            size = 40;
                            break;
                        }
                    case BackgroundNoiseLevel.Medium:
                        {
                            density = 18;
                            size = 40;
                            break;
                        }
                    case BackgroundNoiseLevel.High:
                        {
                            density = 16;
                            size = 39;
                            break;
                        }
                    case BackgroundNoiseLevel.Extreme:
                        {
                            density = 12;
                            size = 38;
                            break;
                        }
                }

                var br = new SolidBrush(Color.Black);
                int max = Convert.ToInt32(Math.Max(rect.Width, rect.Height) / (double)size);

                for (int i = 0, loopTo = Convert.ToInt32(rect.Width * rect.Height / (double)density); i <= loopTo; i++)
                    graphics1.FillEllipse(br, _rand.Next(rect.Width), _rand.Next(rect.Height), _rand.Next(max), _rand.Next(max));
                br.Dispose();
            }

            /// <summary>
            /// Add variable level of curved lines to the image
            /// </summary>
            private void AddLine(Graphics graphics1, Rectangle rect)
            {

                var length = default(int);
                var width = default(float);
                var linecount = default(int);

                switch (_lineNoise)
                {
                    case LineNoiseLevel.None:
                        {
                            return;
                        }
                    case LineNoiseLevel.Low:
                        {
                            length = 4;
                            width = Convert.ToSingle(_height / 31.25d); // 1.6
                            linecount = 1;
                            break;
                        }
                    case LineNoiseLevel.Medium:
                        {
                            length = 5;
                            width = Convert.ToSingle(_height / 27.7777d); // 1.8
                            linecount = 1;
                            break;
                        }
                    case LineNoiseLevel.High:
                        {
                            length = 3;
                            width = Convert.ToSingle(_height / 25d); // 2.0
                            linecount = 2;
                            break;
                        }
                    case LineNoiseLevel.Extreme:
                        {
                            length = 3;
                            width = Convert.ToSingle(_height / 22.7272d); // 2.2
                            linecount = 3;
                            break;
                        }
                }

                var pf = new PointF[length + 1];
                var p = new Pen(Color.Black, width);

                for (int l = 1, loopTo = linecount; l <= loopTo; l++)
                {
                    for (int i = 0, loopTo1 = length; i <= loopTo1; i++)
                        pf[i] = RandomPoint(rect);
                    graphics1.DrawCurve(p, pf, Convert.ToInt32(1.75d));
                }

                p.Dispose();
            }

        }
    }
}

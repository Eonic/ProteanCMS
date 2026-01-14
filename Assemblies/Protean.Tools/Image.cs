//using TinifyAPI;
using SkiaSharp;
using System;
using System.IO;
using System.Threading.Tasks;
using TinifyAPI;
using static Protean.Tools.Text;
using Exception = System.Exception;


namespace Protean.Tools
{
    public partial class Image
    {
        #region Declarations
        private string cLocation; // Location of the file to load
        private bool bKeepRelational = true; // keep sizes relation or skew
        private SKBitmap oImg; // the base image
        private SKBitmap oSourceImg; // the base image
        private SKCanvas oCanvas;
        public event OnErrorEventHandler OnError;

        public delegate void OnErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);
        private const string mcModuleName = "Protean.Tools.Image";
        private bool bCrop = false; // Crop the image?
        private bool bNoStretch = false; // Don't Expand, only Shrink

        private int nMaxHeightCrop = 0;
        private int nMaxWidthCrop = 0;

        public string TinifyKey = "";

        public SKBitmap Image1
        {
            get
            {
                return oImg;
            }
            set
            {
                oImg = value;
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
                
                // Verify image loaded successfully
                if (oImg == null)
                {
                    throw new InvalidOperationException($"Failed to load image from: {Location}");
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""));
                // Re-throw to prevent object creation with null image
                // throw;
            }
        }

        public void Close()
        {
            // closes
            try
            {
                if (oCanvas != null)
                {
                    oCanvas.Dispose();
                    oCanvas = null;
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
                if (!File.Exists(cLocation))
                {
                    throw new FileNotFoundException($"Image file not found: {cLocation}", cLocation);
                }
                
                oImg = SKBitmap.Decode(cLocation);  // ✅ SkiaSharp method
                if (oImg == null)
                {
                    throw new InvalidOperationException($"Failed to decode image: {cLocation}");
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ReLoad", ex, ""));
                // Re-throw to prevent continued execution with null image
                throw;
            }
        }

        public void UploadProcessing(string _WatermarkText, string _WatermarkImgPath)
        {
            try
            {
                if (oImg == null)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "UploadProcessing", 
                        new InvalidOperationException("Cannot process upload: Image is null"), ""));
                    return;
                }
                
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
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "UploadProcessing", ex, ""));
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
                        oImg = ImageResize(oImg, nHeight, nWidth);  // ✅ No cast needed
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



        public void Reflect(SKColor _BackgroundColor, int _Reflectivity) 
        {
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

                cPath = cPath.Replace("/", "\\");
                // cPath = cPath.Replace("\\\\", "\\");

                // check the compression ratio
                if (nCompression > 100)
                    nCompression = 100;
                if (nCompression < 1)
                    nCompression = 1;
                // saves with compression and formatting
                if (oImg != null)
                {
                    return SaveJPGWithCompressionSetting(oImg, cPath, nCompression, serverPath);
                }
                else {
                    return false;
                }
                
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
                string thumbnailPath = cLocation.Remove(cLocation.Length - fi.Name.Length) + VirtualThumbnailPath.Replace("/", "") + @"\";
                var thfi = new FileInfo(thumbnailPath + fi.Name.Replace(".gif", ".png"));

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
                return "?error=" + ex.Message + ex.StackTrace;
            }
        }



        #endregion

        #region Public Properties

        public int Width
        {
            get
            {
                return oImg?.Width ?? 0;  // ✅ Add null check
            }
            set
            {
                if (value >= 1)
                    Resize(value, 0);
            }
        }

        public int Height
        {
            get
            {
                return oImg?.Height ?? 0;  // ✅ Add null check
            }
            set
            {
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
                if (oImg == null)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Resize", 
                        new InvalidOperationException("Cannot resize: Image is null"), ""));
                    return;
                }
                
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
                    oImg = ImageResize(oImg, nNewHeight, nNewWidth);
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
                if (oImg == null)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ResizeMax", 
                        new InvalidOperationException("Cannot resize: Image is null"), ""));
                    return;
                }

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

        private SKBitmap ImageResize(SKBitmap oImage, int nHeight, int nWidth)
        {
            // does the actual resize using SkiaSharp
            try
            {
                if (oImage == null)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ImageResize", 
                        new ArgumentNullException(nameof(oImage), "Source image is null"), ""));
                    return oImage;
                }
                
                if (nWidth <= 0 || nHeight <= 0)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ImageResize", 
                        new ArgumentException($"Invalid dimensions: {nWidth}x{nHeight}"), ""));
                    return oImage;
                }

                oSourceImg = oImage.Copy();
                
                if (oSourceImg == null)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ImageResize", 
                        new InvalidOperationException("Failed to copy source image"), ""));
                    return oImage;
                }

                // Create new bitmap with target dimensions
                var resizedBitmap = new SKBitmap(nWidth, nHeight, oImage.ColorType, oImage.AlphaType);

                using (var canvas = new SKCanvas(resizedBitmap))
                using (var paint = new SKPaint())
                {
                    // High quality settings
                    paint.IsAntialias = true;
                    paint.FilterQuality = SKFilterQuality.High;

                    // Clear canvas
                    canvas.Clear(SKColors.Transparent);

                    // Draw resized image
                    canvas.DrawBitmap(oSourceImg,
                        new SKRect(0, 0, oSourceImg.Width, oSourceImg.Height),
                        new SKRect(0, 0, nWidth, nHeight),
                        paint);
                }

                oImg = resizedBitmap;

                // Add crop if needed
                if (bCrop)
                {
                    oImg = CropImage(oImg);
                }

                return oImg;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "ImageResize", ex, ""));
                return oImage;
            }
        }

        /*
        public ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            // OBSOLETE: This method is no longer needed with SkiaSharp
            // SkiaSharp handles encoding internally
            return null;
        }
        */

        private bool SaveJPGWithCompressionSetting(SKBitmap theImg, string szFileName, long compression, string serverPath = "")
        {
            // save the image using SkiaSharp
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

                    // Determine format and quality
                    SKEncodedImageFormat format;
                    int quality = compression > 100 ? 100 : (int)compression;

                    if (szFileName.EndsWith(".gif"))
                    {
                        szFileName = szFileName.Replace(".gif", ".png");
                        format = SKEncodedImageFormat.Png;
                        quality = 100; // PNG is lossless
                    }
                    else if (szFileName.EndsWith(".png"))
                    {
                        format = SKEncodedImageFormat.Png;
                        quality = 100; // PNG is lossless
                    }
                    else if (szFileName.EndsWith(".webp"))
                    {
                        format = SKEncodedImageFormat.Webp;
                    }
                    else
                    {
                        format = SKEncodedImageFormat.Jpeg;
                    }

                    // Delete existing file
                    if (File.Exists(szFileName))
                    {
                        File.Delete(szFileName);
                    }

                    // Save using SkiaSharp
                    using (var image = SKImage.FromBitmap(theImg))
                    using (var data = image.Encode(format, quality))
                    using (var stream = File.OpenWrite(szFileName))
                    {
                        data.SaveTo(stream);
                    }

                    // Compress if needed
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

                    return true;
                }
                else
                {
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
                string ext = imgfileInfo.Extension?.ToLower();
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
                        switch ((imgfileInfo.Extension ?? "").ToLower())
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
                                            mi.Quality = Convert.ToUInt16(Quality);
                                            mi.Write(NewFileName);
                                        }
                                    }
                                    var newImgFile = new FileInfo(NewFileName);
                                    difference = imgfileInfo.Length;
                                    ImageMagick.ImageOptimizers.PngOptimizer optimizer = new ImageMagick.ImageOptimizers.PngOptimizer();
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
                                            mi.Quality = Convert.ToUInt16(Quality);
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


        private SKBitmap CropImage(SKBitmap oImage)
        {
            try
            {
                if (oImage == null)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CropImage", 
                        new ArgumentNullException(nameof(oImage), "Image to crop is null"), ""));
                    return oImage;
                }
                
                if (nMaxWidthCrop <= 0 || nMaxHeightCrop <= 0)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CropImage", 
                        new ArgumentException($"Invalid crop dimensions: {nMaxWidthCrop}x{nMaxHeightCrop}"), ""));
                    return oImage;
                }
                
                var cropped = new SKBitmap(nMaxWidthCrop, nMaxHeightCrop, oImage.ColorType, oImage.AlphaType);

                using (var canvas = new SKCanvas(cropped))
                using (var paint = new SKPaint())
                {
                    paint.IsAntialias = true;
                    paint.FilterQuality = SKFilterQuality.High;

                    SKRect sourceRect;
                    SKRect destRect = new SKRect(0, 0, nMaxWidthCrop, nMaxHeightCrop);

                    if (oImage.Width == nMaxWidthCrop && nMaxHeightCrop > 0)
                    {
                        // If the Width is perfect, crop the Height
                        int nNewY = (oImage.Height - nMaxHeightCrop) / 2;
                        sourceRect = new SKRect(0, nNewY, oImage.Width, nNewY + nMaxHeightCrop);
                    }
                    else if (nMaxWidthCrop > 0)
                    {
                        // Else crop the width
                        int nNewW = (oImage.Width - nMaxWidthCrop) / 2;
                        sourceRect = new SKRect(nNewW, 0, nNewW + nMaxWidthCrop, oImage.Height);
                    }
                    else
                    {
                        sourceRect = new SKRect(0, 0, oImage.Width, oImage.Height);
                    }

                    canvas.DrawBitmap(oImage, sourceRect, destRect, paint);
                }

                return cropped;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "CropImage", ex, ""));
                return oImage;
            }
        }

        private SKBitmap AddReflection(SKColor _BackgroundColor, int _Reflectivity)
        {
            try
            {
                if (oImg == null)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AddReflection", 
                        new InvalidOperationException("Cannot add reflection: Image is null"), ""));
                    return oImg;
                }
                
                SKBitmap _image = oImg;

                // Calculate the size of the new image
                int height = Convert.ToInt32(_image.Height + _image.Height * (_Reflectivity / 255f));
                var newImage = new SKBitmap(_image.Width, height, _image.ColorType, _image.AlphaType);

                using (var canvas = new SKCanvas(newImage))
                using (var paint = new SKPaint())
                {
                    paint.IsAntialias = true;
                    paint.FilterQuality = SKFilterQuality.High;

                    // Clear background
                    canvas.Clear(_BackgroundColor);

                    // Draw original image
                    canvas.DrawBitmap(_image, 0, 0, paint);

                    // Prepare the reflected image
                    int reflectionHeight = _image.Height * _Reflectivity / 255;
                    var reflectedImage = new SKBitmap(_image.Width, reflectionHeight, _image.ColorType, _image.AlphaType);

                    using (var reflCanvas = new SKCanvas(reflectedImage))
                    {
                        var sourceRect = new SKRect(0, _image.Height - reflectionHeight, _image.Width, _image.Height);
                        var destRect = new SKRect(0, 0, _image.Width, reflectionHeight);
                        reflCanvas.DrawBitmap(_image, sourceRect, destRect, paint);
                    }

                    // Flip the reflection vertically - FIXED
                    var flipped = new SKBitmap(reflectedImage.Width, reflectedImage.Height, reflectedImage.ColorType, reflectedImage.AlphaType);
                    using (var flipCanvas = new SKCanvas(flipped))
                    {
                        // Create scale matrix for vertical flip
                        var matrix = SKMatrix.CreateScale(1, -1);

                        // Translate to compensate for the flip
                        matrix = matrix.PostConcat(SKMatrix.CreateTranslation(0, reflectedImage.Height));

                        flipCanvas.SetMatrix(matrix);
                        flipCanvas.DrawBitmap(reflectedImage, 0, 0);
                    }
                    reflectedImage.Dispose();
                    reflectedImage = flipped;

                    // Draw reflected image
                    canvas.DrawBitmap(reflectedImage, 0, _image.Height, paint);

                    // Apply gradient overlay
                    var imageRect = new SKRect(0, _image.Height, _image.Width, _image.Height + reflectionHeight);
                    var colors = new SKColor[] {
                _BackgroundColor.WithAlpha((byte)(255 - _Reflectivity)),
                _BackgroundColor
            };
                    var positions = new float[] { 0.0f, 1.0f };

                    using (var shader = SKShader.CreateLinearGradient(
                        new SKPoint(0, imageRect.Top),
                        new SKPoint(0, imageRect.Bottom),
                        colors,
                        positions,
                        SKShaderTileMode.Clamp))
                    {
                        paint.Shader = shader;
                        canvas.DrawRect(imageRect, paint);
                    }

                    reflectedImage.Dispose();
                }

                oImg = newImage;
                return oImg;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AddReflection", ex, ""));
                return oImg;
            }
        }

        private SKBitmap AddWatermarkOld(SKBitmap imgPhoto, string _WatermarkText, string _WatermarkImgPath)
        {
            try
            {
                int phWidth = imgPhoto.Width;
                int phHeight = imgPhoto.Height;

                var bmPhoto = new SKBitmap(phWidth, phHeight, imgPhoto.ColorType, imgPhoto.AlphaType);

                using (var canvas = new SKCanvas(bmPhoto))
                using (var paint = new SKPaint())
                {
                    paint.IsAntialias = true;
                    paint.FilterQuality = SKFilterQuality.High;

                    // Draw original image
                    canvas.DrawBitmap(imgPhoto, 0, 0, paint);

                    // Add text watermark
                    if (!string.IsNullOrEmpty(_WatermarkText))
                    {
                        // Find appropriate font size
                        int[] sizes = new int[] { 48, 24, 20, 16, 14, 12, 10, 8, 6, 4 };
                        SKFont font = null;

                        foreach (int size in sizes)
                        {
                            font = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), size);
                            float textWidth = paint.MeasureText(_WatermarkText);

                            if (textWidth < phWidth * 0.66)
                            {
                                break;
                            }
                        }

                        // Calculate position (bottom center)
                        int yPixelsFromBottom = (int)(phHeight * 0.05);
                        float textWidth2 = paint.MeasureText(_WatermarkText);
                        float xCenterOfImg = (phWidth - textWidth2) / 2f;
                        float yPosFromBottom = phHeight - yPixelsFromBottom;

                        // Draw shadow
                        paint.Color = new SKColor(0, 0, 0, 153);
                        canvas.DrawText(_WatermarkText, xCenterOfImg + 1, yPosFromBottom + 1, font, paint);

                        // Draw text
                        paint.Color = new SKColor(255, 255, 255, 153);
                        canvas.DrawText(_WatermarkText, xCenterOfImg, yPosFromBottom, font, paint);

                        font.Dispose();
                    }

                    // Add image watermark
                    if (!string.IsNullOrEmpty(_WatermarkImgPath) && File.Exists(_WatermarkImgPath))
                    {
                        using (var imgWatermark = SKBitmap.Decode(_WatermarkImgPath))
                        {
                            if (imgWatermark != null)
                            {
                                int wmWidth = imgWatermark.Width;
                                int wmHeight = imgWatermark.Height;
                                int xPosOfWm = phWidth - wmWidth - 10;
                                int yPosOfWm = 10;

                                // Apply transparency
                                paint.Color = paint.Color.WithAlpha(76); // 0.3 opacity (76/255)
                                canvas.DrawBitmap(imgWatermark, xPosOfWm, yPosOfWm, paint);
                            }
                        }
                    }
                }

                oImg = bmPhoto;
                Save(cLocation);
                return oImg;
            }
            catch (Exception)
            {
                return imgPhoto;
            }
        }

        public SKBitmap AddWatermark(string _WatermarkText, string _WatermarkImgPath)
        {
            try
            {
                if (oImg == null)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AddWatermark", 
                        new InvalidOperationException("Cannot add watermark: Image is null"), ""));
                    return oImg;
                }
                
                int phWidth = oImg.Width;
                int phHeight = oImg.Height;

                var bmPhoto = new SKBitmap(phWidth, phHeight, oImg.ColorType, oImg.AlphaType);

                using (var canvas = new SKCanvas(bmPhoto))
                using (var paint = new SKPaint())
                {
                    paint.IsAntialias = true;
                    paint.FilterQuality = SKFilterQuality.High;

                    // Draw original image
                    canvas.DrawBitmap(oImg, 0, 0, paint);

                    // Add text watermark
                    if (!string.IsNullOrEmpty(_WatermarkText))
                    {
                        // Find appropriate font size
                        int[] sizes = new int[] { 48, 24, 20, 16, 14, 12, 10, 8, 6, 4 };
                        SKFont font = null;

                        foreach (int size in sizes)
                        {
                            font = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), size);
                            float textWidth = paint.MeasureText(_WatermarkText);

                            if (textWidth < phWidth * 0.66)
                            {
                                break;
                            }
                        }

                        // Calculate position
                        int yPixelsFromBottom = (int)(phHeight * 0.05);
                        float textWidth2 = paint.MeasureText(_WatermarkText);
                        float xCenterOfImg = (phWidth - textWidth2) / 2f;
                        float yPosFromBottom = phHeight - yPixelsFromBottom;

                        // Draw shadow
                        paint.Color = new SKColor(0, 0, 0, 153);
                        canvas.DrawText(_WatermarkText, xCenterOfImg + 1, yPosFromBottom + 1, font, paint);

                        // Draw text
                        paint.Color = new SKColor(255, 255, 255, 153);
                        canvas.DrawText(_WatermarkText, xCenterOfImg, yPosFromBottom, font, paint);

                        font.Dispose();
                    }

                    // Add image watermark
                    if (!string.IsNullOrEmpty(_WatermarkImgPath) && File.Exists(_WatermarkImgPath))
                    {
                        using (var imgWatermark = SKBitmap.Decode(_WatermarkImgPath))
                        {
                            if (imgWatermark != null)
                            {
                                int wmWidth = imgWatermark.Width;
                                int wmHeight = imgWatermark.Height;
                                int xPosOfWm = 0;
                                int yPosOfWm = 0;

                                paint.Color = paint.Color.WithAlpha(255);
                                canvas.DrawBitmap(imgWatermark, xPosOfWm, yPosOfWm, paint);
                            }
                        }
                    }
                }

                oImg = bmPhoto;
                return oImg;
            }
            catch (Exception)
            {
                return oImg;
            }
        }


        public SKBitmap AddWatermark(SKBitmap oImgParam, string _WatermarkText, string _WatermarkImgPath)
        {
            // Note: Renamed parameter from oImg to oImgParam to avoid confusion with field
            try
            {
                if (oImgParam == null)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "AddWatermark", 
                        new ArgumentNullException(nameof(oImgParam), "Image parameter is null"), ""));
                    return oImgParam;
                }
                
                int phWidth = oImgParam.Width;
                int phHeight = oImgParam.Height;

                var bmPhoto = new SKBitmap(phWidth, phHeight, oImgParam.ColorType, oImgParam.AlphaType);

                using (var canvas = new SKCanvas(bmPhoto))
                using (var paint = new SKPaint())
                {
                    paint.IsAntialias = true;
                    paint.FilterQuality = SKFilterQuality.High;

                    // Draw original image
                    canvas.DrawBitmap(oImgParam, 0, 0, paint);

                    // Add text watermark
                    if (!string.IsNullOrEmpty(_WatermarkText))
                    {
                        // Find appropriate font size
                        int[] sizes = new int[] { 48, 24, 20, 16, 14, 12, 10, 8, 6, 4 };
                        SKFont font = null;

                        foreach (int size in sizes)
                        {
                            font = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), size);
                            float textWidth = paint.MeasureText(_WatermarkText);

                            if (textWidth < phWidth * 0.66)
                            {
                                break;
                            }
                        }

                        // Calculate position
                        int yPixelsFromBottom = (int)(phHeight * 0.05);
                        float textWidth2 = paint.MeasureText(_WatermarkText);
                        float xCenterOfImg = (phWidth - textWidth2) / 2f;
                        float yPosFromBottom = phHeight - yPixelsFromBottom;

                        // Draw shadow
                        paint.Color = new SKColor(0, 0, 0, 153);
                        canvas.DrawText(_WatermarkText, xCenterOfImg + 1, yPosFromBottom + 1, font, paint);

                        // Draw text
                        paint.Color = new SKColor(255, 255, 255, 153);
                        canvas.DrawText(_WatermarkText, xCenterOfImg, yPosFromBottom, font, paint);

                        font.Dispose();
                    }

                    // Add image watermark
                    if (!string.IsNullOrEmpty(_WatermarkImgPath) && File.Exists(_WatermarkImgPath))
                    {
                        using (var imgWatermark = SKBitmap.Decode(_WatermarkImgPath))
                        {
                            if (imgWatermark != null)
                            {
                                int wmWidth = imgWatermark.Width;
                                int wmHeight = imgWatermark.Height;
                                int xPosOfWm = 0;
                                int yPosOfWm = 0;

                                paint.Color = paint.Color.WithAlpha(255);
                                canvas.DrawBitmap(imgWatermark, xPosOfWm, yPosOfWm, paint);
                            }
                        }
                    }
                }

                this.oImg = bmPhoto;
                return this.oImg;
            }
            catch (Exception)
            {
                return oImgParam;
            }
        }

        //private string[] _RandomFontFamily_ff = default;

        #endregion

    }
}

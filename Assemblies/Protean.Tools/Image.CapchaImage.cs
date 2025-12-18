using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;
using static Protean.Tools.Text;

namespace Protean.Tools
{
    public partial class Image
    {
        /// <summary>
        /// CAPTCHA image generation class - Migrated to SkiaSharp
        /// </summary>
        /// <remarks>
        /// Adapted from the excellent code at 
        /// http://www.codeproject.com/aspnet/CaptchaImage.asp
        /// 
        /// Jeff Atwood
        /// http://www.codinghorror.com/
        /// 
        /// Migrated to SkiaSharp for cross-platform compatibility
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
            private string _fontWhitelist = "arial;arial black;comic sans ms;courier new;" +
                                           "lucida console;lucida sans unicode;microsoft sans serif;" +
                                           "tahoma;times new roman;trebuchet ms;verdana";

            public event OnErrorEventHandler OnError;
            public delegate void OnErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);
            private const string mcModuleName = "Protean.Tools.Image.CaptchaImage";

            #region Public Enums

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

            #region Public Properties

            /// <summary>
            /// Returns a GUID that uniquely identifies this Captcha
            /// </summary>
            public string UniqueId
            {
                get { return _guid; }
            }

            /// <summary>
            /// Returns the date and time this image was last rendered
            /// </summary>
            public DateTime RenderedAt
            {
                get { return _generatedAt; }
            }

            /// <summary>
            /// Font family to use when drawing the Captcha text. If no font is provided, 
            /// a random font will be chosen from the font whitelist for each character.
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
                        // Validate font exists by attempting to create a typeface
                        using (var typeface = SKTypeface.FromFamilyName(value))
                        {
                            if (typeface != null)
                            {
                                _fontFamilyName = value;
                            }
                            else
                            {
                                // Fallback to default
                                _fontFamilyName = "Arial";
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Fallback to Arial if font validation fails
                        _fontFamilyName = "Arial";
                    }
                }
            }

            /// <summary>
            /// Amount of random warping to apply to the Captcha text.
            /// </summary>
            public FontWarpFactor FontWarp
            {
                get { return _fontWarp; }
                set { _fontWarp = value; }
            }

            /// <summary>
            /// Amount of background noise to apply to the Captcha image.
            /// </summary>
            public BackgroundNoiseLevel BackgroundNoise
            {
                get { return _backgroundNoise; }
                set { _backgroundNoise = value; }
            }

            public LineNoiseLevel LineNoise
            {
                get { return _lineNoise; }
                set { _lineNoise = value; }
            }

            /// <summary>
            /// A string of valid characters to use in the Captcha text. 
            /// A random character will be selected from this string for each character.
            /// </summary>
            public string TextChars
            {
                get { return _randomTextChars; }
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
                get { return _randomTextLength; }
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
                get { return _randomText; }
            }

            /// <summary>
            /// Width of Captcha image to generate, in pixels 
            /// </summary>
            public int Width
            {
                get { return _width; }
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
                get { return _height; }
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
                get { return _fontWhitelist; }
                set { _fontWhitelist = value; }
            }

            #endregion

            #region Constructor

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
                    _fontWhitelist = "arial;arial black;comic sans ms;courier new;" +
                                    "lucida console;lucida sans unicode;microsoft sans serif;" +
                                    "tahoma;times new roman;trebuchet ms;verdana";

                    _randomText = GenerateRandomText();
                    _generatedAt = DateTime.Now;
                    _guid = Guid.NewGuid().ToString();
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "New", ex, ""));
                }
            }

            #endregion

            #region Public Methods

            /// <summary>
            /// Forces a new Captcha image to be generated using current property value settings.
            /// Returns SKBitmap instead of System.Drawing.Bitmap
            /// </summary>
            public SKBitmap RenderImage()
            {
                return GenerateImagePrivate();
            }

            #endregion

            #region Private Methods

            /// <summary>
            /// Returns a random font family from the font whitelist
            /// </summary>
            private string RandomFontFamily()
            {
                try
                {
                    string[] ff = null;

                    if (_fontWhitelist != string.Empty)
                    {
                        ff = _fontWhitelist.Split(';');
                    }

                    if (ff != null && ff.Length > 0)
                    {
                        return ff[_rand.Next(0, ff.Length)];
                    }
                    else
                    {
                        return "Arial";
                    }
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "RandomFontFamily", ex, ""));
                    return "Arial";
                }
            }

            /// <summary>
            /// Generate random text for the CAPTCHA
            /// </summary>
            private string GenerateRandomText()
            {
                try
                {
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
            private SKPoint RandomPoint(int xmin, int xmax, int ymin, int ymax)
            {
                return new SKPoint(_rand.Next(xmin, xmax), _rand.Next(ymin, ymax));
            }

            /// <summary>
            /// Returns a random point within the specified rectangle
            /// </summary>
            private SKPoint RandomPoint(SKRect rect)
            {
                return RandomPoint((int)rect.Left, (int)rect.Right, (int)rect.Top, (int)rect.Bottom);
            }

            /// <summary>
            /// Returns the CAPTCHA font in an appropriate size 
            /// </summary>
            private SKFont GetFont()
            {
                try
                {
                    float fsize = 0f;
                    string fname = _fontFamilyName;

                    if (string.IsNullOrEmpty(fname))
                    {
                        fname = RandomFontFamily();
                    }

                    switch (FontWarp)
                    {
                        case FontWarpFactor.None:
                            fsize = _height * 0.7f;
                            break;
                        case FontWarpFactor.Low:
                            fsize = _height * 0.8f;
                            break;
                        case FontWarpFactor.Medium:
                            fsize = _height * 0.85f;
                            break;
                        case FontWarpFactor.High:
                            fsize = _height * 0.9f;
                            break;
                        case FontWarpFactor.Extreme:
                            fsize = _height * 0.95f;
                            break;
                    }

                    var typeface = SKTypeface.FromFamilyName(fname, SKFontStyle.Bold);
                    return new SKFont(typeface, fsize);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetFont", ex, ""));
                    return new SKFont(SKTypeface.FromFamilyName("Arial"), 12);
                }
            }

            /// <summary>
            /// Renders the CAPTCHA image using SkiaSharp
            /// </summary>
            private SKBitmap GenerateImagePrivate()
            {
                try
                {
                    var bmp = new SKBitmap(_width, _height, SKColorType.Rgba8888, SKAlphaType.Premul);

                    using (var canvas = new SKCanvas(bmp))
                    using (var paint = new SKPaint())
                    {
                        paint.IsAntialias = true;

                        // Fill white background
                        canvas.Clear(SKColors.White);

                        int charOffset = 0;
                        double charWidth = _width / (double)_randomTextLength;

                        foreach (char c in _randomText)
                        {
                            using (var font = GetFont())
                            {
                                SKRect charRect = new SKRect(
                                    (float)(charOffset * charWidth),
                                    0,
                                    (float)((charOffset + 1) * charWidth),
                                    _height
                                );

                                // Create text path for warping
                                using (var path = CreateTextPath(c.ToString(), font, charRect))
                                {
                                    // Warp the character path
                                    WarpText(path, charRect);

                                    // Draw the character
                                    paint.Color = SKColors.Black;
                                    paint.Style = SKPaintStyle.Fill;
                                    canvas.DrawPath(path, paint);
                                }

                                charOffset += 1;
                            }
                        }

                        // Add noise and lines
                        AddNoise(canvas, paint, new SKRect(0, 0, _width, _height));
                        AddLine(canvas, paint, new SKRect(0, 0, _width, _height));
                    }

                    _generatedAt = DateTime.Now;
                    return bmp;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GenerateImagePrivate", ex, ""));
                    return null;
                }
            }

            /// <summary>
            /// Creates a text path for the given character
            /// </summary>
            private SKPath CreateTextPath(string text, SKFont font, SKRect bounds)
            {
                var path = new SKPath();

                using (var paint = new SKPaint())
                {
                    paint.IsAntialias = true;
                    paint.TextAlign = SKTextAlign.Left;

                    // Measure text to center it in bounds
                    SKRect textBounds = new SKRect();
                    float textWidth = font.MeasureText(text, out textBounds);

                    float x = bounds.Left + (bounds.Width - textWidth) / 2;
                    float y = bounds.Top + (bounds.Height - textBounds.Height) / 2 - textBounds.Top;
                    var points = new SKPoint[] { new SKPoint(x, y) };
                    // Get text path using the font
                    font.GetTextPath(text, points);
                }

                return path;
            }

            /// <summary>
            /// Warp the provided text path by a variable amount
            /// </summary>
            private void WarpText(SKPath textPath, SKRect rect)
            {
                float warpDivisor = 0f;
                float rangeModifier = 0f;

                switch (_fontWarp)
                {
                    case FontWarpFactor.None:
                        return;
                    case FontWarpFactor.Low:
                        warpDivisor = 6f;
                        rangeModifier = 1f;
                        break;
                    case FontWarpFactor.Medium:
                        warpDivisor = 5f;
                        rangeModifier = 1.3f;
                        break;
                    case FontWarpFactor.High:
                        warpDivisor = 4.5f;
                        rangeModifier = 1.4f;
                        break;
                    case FontWarpFactor.Extreme:
                        warpDivisor = 4f;
                        rangeModifier = 1.5f;
                        break;
                }

                int hrange = (int)(rect.Height / warpDivisor);
                int wrange = (int)(rect.Width / warpDivisor);
                int left = (int)(rect.Left - (wrange * rangeModifier));
                int top = (int)(rect.Top - (hrange * rangeModifier));
                int width = (int)(rect.Left + rect.Width + (wrange * rangeModifier));
                int height = (int)(rect.Top + rect.Height + (hrange * rangeModifier));

                if (left < 0) left = 0;
                if (top < 0) top = 0;
                if (width > Width) width = Width;
                if (height > Height) height = Height;

                var leftTop = RandomPoint(left, left + wrange, top, top + hrange);
                var rightTop = RandomPoint(width - wrange, width, top, top + hrange);
                var leftBottom = RandomPoint(left, left + wrange, height - hrange, height);
                var rightBottom = RandomPoint(width - wrange, width, height - hrange, height);

                // Apply perspective transformation
                var matrix = SKMatrix.CreateIdentity();

                // Simple distortion by translating points slightly
                // Note: SkiaSharp doesn't have direct path warping like GDI+
                // This is a simplified version - for complex warping, consider using SKMatrix transformations
                float offsetX = (_rand.Next(-5, 5)) / 10f;
                float offsetY = (_rand.Next(-5, 5)) / 10f;

                matrix = SKMatrix.CreateTranslation(offsetX, offsetY);
                textPath.Transform(matrix);
            }

            /// <summary>
            /// Add a variable level of graphic noise to the image
            /// </summary>
            private void AddNoise(SKCanvas canvas, SKPaint paint, SKRect rect)
            {
                int density = 0;
                int size = 0;

                switch (_backgroundNoise)
                {
                    case BackgroundNoiseLevel.None:
                        return;
                    case BackgroundNoiseLevel.Low:
                        density = 30;
                        size = 40;
                        break;
                    case BackgroundNoiseLevel.Medium:
                        density = 18;
                        size = 40;
                        break;
                    case BackgroundNoiseLevel.High:
                        density = 16;
                        size = 39;
                        break;
                    case BackgroundNoiseLevel.Extreme:
                        density = 12;
                        size = 38;
                        break;
                }

                paint.Color = SKColors.Black;
                paint.Style = SKPaintStyle.Fill;

                int max = (int)(Math.Max(rect.Width, rect.Height) / size);
                int iterations = (int)(rect.Width * rect.Height / density);

                for (int i = 0; i <= iterations; i++)
                {
                    float x = _rand.Next((int)rect.Width);
                    float y = _rand.Next((int)rect.Height);
                    float width = _rand.Next(max);
                    float height = _rand.Next(max);

                    canvas.DrawOval(x, y, width, height, paint);
                }
            }

            /// <summary>
            /// Add variable level of curved lines to the image
            /// </summary>
            private void AddLine(SKCanvas canvas, SKPaint paint, SKRect rect)
            {
                int length = 0;
                float width = 0f;
                int linecount = 0;

                switch (_lineNoise)
                {
                    case LineNoiseLevel.None:
                        return;
                    case LineNoiseLevel.Low:
                        length = 4;
                        width = _height / 31.25f; // 1.6
                        linecount = 1;
                        break;
                    case LineNoiseLevel.Medium:
                        length = 5;
                        width = _height / 27.7777f; // 1.8
                        linecount = 1;
                        break;
                    case LineNoiseLevel.High:
                        length = 3;
                        width = _height / 25f; // 2.0
                        linecount = 2;
                        break;
                    case LineNoiseLevel.Extreme:
                        length = 3;
                        width = _height / 22.7272f; // 2.2
                        linecount = 3;
                        break;
                }

                paint.Color = SKColors.Black;
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = width;
                paint.IsAntialias = true;

                for (int l = 1; l <= linecount; l++)
                {
                    using (var path = new SKPath())
                    {
                        // Generate random points for curve
                        SKPoint[] points = new SKPoint[length + 1];
                        for (int i = 0; i <= length; i++)
                        {
                            points[i] = RandomPoint(rect);
                        }

                        // Draw smooth curve through points
                        if (points.Length > 0)
                        {
                            path.MoveTo(points[0]);
                            for (int i = 1; i < points.Length; i++)
                            {
                                if (i == 1)
                                {
                                    path.LineTo(points[i]);
                                }
                                else
                                {
                                    // Create smooth curve
                                    var controlPoint = new SKPoint(
                                        (points[i - 1].X + points[i].X) / 2,
                                        (points[i - 1].Y + points[i].Y) / 2
                                    );
                                    path.QuadTo(points[i - 1], points[i]);
                                }
                            }
                            canvas.DrawPath(path, paint);
                        }
                    }
                }
            }

            #endregion
        }
    }
}

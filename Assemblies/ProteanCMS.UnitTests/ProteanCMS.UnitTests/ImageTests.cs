using Microsoft.VisualStudio.TestTools.UnitTesting;
using Protean.Tools;
using SkiaSharp;
using System;
using System.IO;

namespace Protean.ToolsTests
{
    /// <summary>
    /// Unit tests for Protean.Tools.Image class with SkiaSharp implementation.
    /// Tests verify image loading, resizing, cropping, watermarking, reflection effects,
    /// and proper resource management after migration from System.Drawing to SkiaSharp.
    /// 
    /// These tests ensure cross-platform compatibility and validate that all image
    /// manipulation operations work correctly with the new SkiaSharp backend.
    /// </summary>
    [TestClass]
    public class ImageTests
    {
        private string _testImagesPath;
        private string _testOutputPath;

        [TestInitialize]
        public void TestInitialize()
        {
            // Setup test directories
            _testImagesPath = Path.Combine(Path.GetTempPath(), "ImageTests", "Input");
            _testOutputPath = Path.Combine(Path.GetTempPath(), "ImageTests", "Output");

            Directory.CreateDirectory(_testImagesPath);
            Directory.CreateDirectory(_testOutputPath);

            // Create test images
            CreateTestImage("test.jpg", 800, 600, SKColors.Blue);
            CreateTestImage("test.png", 400, 300, SKColors.Red);
            CreateTestImage("test-large.jpg", 2000, 1500, SKColors.Green);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Clean up test directories
            try
            {
                if (Directory.Exists(_testImagesPath))
                    Directory.Delete(_testImagesPath, true);
                if (Directory.Exists(_testOutputPath))
                    Directory.Delete(_testOutputPath, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        #region Helper Methods

        /// <summary>
        /// Creates a test image with specified dimensions and color
        /// </summary>
        private void CreateTestImage(string filename, int width, int height, SKColor color)
        {
            var path = Path.Combine(_testImagesPath, filename);

            using (var bitmap = new SKBitmap(width, height))
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(color);

                // Add some content for better testing
                using (var paint = new SKPaint())
                {
                    paint.Color = SKColors.White;
                    paint.TextSize = 48;
                    paint.IsAntialias = true;

                    canvas.DrawText("TEST", width / 2 - 50, height / 2, paint);
                }

                using (var image = SKImage.FromBitmap(bitmap))
                using (var data = image.Encode(
                    filename.EndsWith(".png") ? SKEncodedImageFormat.Png : SKEncodedImageFormat.Jpeg,
                    90))
                using (var stream = File.OpenWrite(path))
                {
                    data.SaveTo(stream);
                }
            }
        }

        #endregion

        #region Basic Image Operations Tests

        [TestMethod]
        [TestCategory("Image")]
        [TestCategory("Loading")]
        [Priority(1)]
        [Description("Verifies image can be loaded successfully")]
        public void Image_LoadJpeg_Success()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagesPath, "test.jpg");

            // Act
            var image = new Image(imagePath);

            // Assert
            Assert.IsNotNull(image, "Image should not be null");
            Assert.IsNotNull(image.Image1, "Image1 property should not be null");
            Assert.AreEqual(800, image.Width, "Width should be 800");
            Assert.AreEqual(600, image.Height, "Height should be 600");

            // Cleanup
            image.Close();
        }

        [TestMethod]
        [TestCategory("Image")]
        [TestCategory("Loading")]
        [Priority(1)]
        [Description("Verifies PNG image can be loaded successfully")]
        public void Image_LoadPng_Success()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagesPath, "test.png");

            // Act
            var image = new Image(imagePath);

            // Assert
            Assert.IsNotNull(image.Image1);
            Assert.AreEqual(400, image.Width);
            Assert.AreEqual(300, image.Height);

            // Cleanup
            image.Close();
        }

        [TestMethod]
        [TestCategory("Image")]
        [TestCategory("Loading")]
        [Priority(1)]
        [Description("Verifies loading non-existent image returns gracefully")]
        public void Image_LoadNonExistent_DoesNotThrow()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagesPath, "nonexistent.jpg");

            // Act
            var image = new Image(imagePath);

            // Assert
            Assert.IsNotNull(image, "Image instance should be created");
            Assert.AreEqual(0, image.Width, "Width should be 0 for failed load");
            Assert.AreEqual(0, image.Height, "Height should be 0 for failed load");

            // Cleanup
            image.Close();
        }

        #endregion

        #region Resize Tests

        [TestMethod]
        [TestCategory("Image")]
        [TestCategory("Resize")]
        [Priority(2)]
        [Description("Verifies image can be resized maintaining aspect ratio")]
        public void Image_ResizeWithAspectRatio_Success()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagesPath, "test.jpg");
            var image = new Image(imagePath);
            image.KeepXYRelation = true;

            // Act
            image.SetMaxSize(400, 400);

            // Assert
            Assert.AreEqual(400, image.Width, "Width should be 400");
            Assert.AreEqual(300, image.Height, "Height should be 300 (maintaining aspect ratio)");

            // Cleanup
            image.Close();
        }

        [TestMethod]
        [TestCategory("Image")]
        [TestCategory("Resize")]
        [Priority(2)]
        [Description("Verifies image can be resized without maintaining aspect ratio")]
        public void Image_ResizeWithoutAspectRatio_Success()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagesPath, "test.jpg");
            var image = new Image(imagePath);
            image.KeepXYRelation = false;

            // Act
            image.Width = 500;

            // Assert
            Assert.AreEqual(500, image.Width, "Width should be 500");
            Assert.AreEqual(600, image.Height, "Height should remain 600");

            // Cleanup
            image.Close();
        }

        [TestMethod]
        [TestCategory("Image")]
        [TestCategory("Resize")]
        [Priority(2)]
        [Description("Verifies NoStretch prevents image enlargement")]
        public void Image_NoStretch_PreventsEnlargement()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagesPath, "test.png");
            var image = new Image(imagePath);
            image.NoStretch = true;
            image.KeepXYRelation = true;

            // Act
            image.SetMaxSize(800, 600); // Larger than original

            // Assert - Should not enlarge
            Assert.AreEqual(400, image.Width, "Width should not enlarge");
            Assert.AreEqual(300, image.Height, "Height should not enlarge");

            // Cleanup
            image.Close();
        }

        #endregion

        #region Crop Tests

        [TestMethod]
        [TestCategory("Image")]
        [TestCategory("Crop")]
        [Priority(2)]
        [Description("Verifies image cropping works correctly")]
        public void Image_CropToSize_Success()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagesPath, "test.jpg");
            var image = new Image(imagePath);
            image.IsCrop = true;
            image.KeepXYRelation = true;

            // Act
            image.SetMaxSize(200, 200);

            // Assert
            Assert.AreEqual(200, image.Width, "Width should be 200");
            Assert.AreEqual(200, image.Height, "Height should be 200");

            // Cleanup
            image.Close();
        }

        #endregion

        #region Save Tests

        [TestMethod]
        [TestCategory("Image")]
        [TestCategory("Save")]
        [Priority(2)]
        [Description("Verifies image can be saved as JPEG")]
        public void Image_SaveAsJpeg_Success()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagesPath, "test.png");
            var outputPath = Path.Combine(_testOutputPath, "output.jpg");
            var image = new Image(imagePath);

            // Act
            bool result = image.Save(outputPath, 85);

            // Assert
            Assert.IsTrue(result, "Save should return true");
            Assert.IsTrue(File.Exists(outputPath), "Output file should exist");

            // Verify saved image
            var savedImage = new Image(outputPath);
            Assert.AreEqual(400, savedImage.Width);
            Assert.AreEqual(300, savedImage.Height);

            // Cleanup
            image.Close();
            savedImage.Close();
        }

        [TestMethod]
        [TestCategory("Image")]
        [TestCategory("Save")]
        [Priority(2)]
        [Description("Verifies image can be saved as PNG")]
        public void Image_SaveAsPng_Success()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagesPath, "test.jpg");
            var outputPath = Path.Combine(_testOutputPath, "output.png");
            var image = new Image(imagePath);

            // Act
            bool result = image.Save(outputPath, 100);

            // Assert
            Assert.IsTrue(result, "Save should return true");
            Assert.IsTrue(File.Exists(outputPath), "Output file should exist");

            // Cleanup
            image.Close();
        }

        [TestMethod]
        [TestCategory("Image")]
        [TestCategory("Save")]
        [Priority(2)]
        [Description("Verifies GIF is converted to PNG on save")]
        public void Image_SaveGifAsPng_Success()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagesPath, "test.jpg");
            var outputPath = Path.Combine(_testOutputPath, "output.gif");
            var expectedPath = Path.Combine(_testOutputPath, "output.png");
            var image = new Image(imagePath);

            // Act
            bool result = image.Save(outputPath, 100);

            // Assert
            Assert.IsTrue(result, "Save should return true");
            Assert.IsTrue(File.Exists(expectedPath), "PNG file should exist");
            Assert.IsFalse(File.Exists(outputPath), "GIF file should not exist");

            // Cleanup
            image.Close();
        }

        #endregion

        #region Thumbnail Tests

        [TestMethod]
        [TestCategory("Image")]
        [TestCategory("Thumbnail")]
        [Priority(2)]
        [Description("Verifies thumbnail creation")]
        public void Image_CreateThumbnail_Success()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagesPath, "test.jpg");
            var image = new Image(imagePath);
            var thumbnailDir = Path.Combine(_testOutputPath, "thumbs");
            Directory.CreateDirectory(thumbnailDir);

            // Act
            string result = image.CreateThumbnail("", "/thumbs");

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsTrue(result.Contains("thumbs"), "Result should contain thumbs path");

            // Cleanup
            image.Close();
        }

        #endregion

        #region Watermark Tests

        [TestMethod]
        [TestCategory("Image")]
        [TestCategory("Watermark")]
        [Priority(3)]
        [Description("Verifies text watermark can be added")]
        public void Image_AddTextWatermark_Success()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagesPath, "test.jpg");
            var image = new Image(imagePath);

            // Act
            var result = image.AddWatermark("WATERMARK TEST", "");

            // Assert
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual(800, result.Width, "Width should be preserved");
            Assert.AreEqual(600, result.Height, "Height should be preserved");

            // Cleanup
            image.Close();
        }

        [TestMethod]
        [TestCategory("Image")]
        [TestCategory("Watermark")]
        [Priority(3)]
        [Description("Verifies watermark with empty text doesn't fail")]
        public void Image_AddEmptyWatermark_DoesNotThrow()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagesPath, "test.jpg");
            var image = new Image(imagePath);

            // Act
            var result = image.AddWatermark("", "");

            // Assert
            Assert.IsNotNull(result, "Result should not be null");

            // Cleanup
            image.Close();
        }

        #endregion

        #region Reflection Tests

        [TestMethod]
        [TestCategory("Image")]
        [TestCategory("Reflection")]
        [Priority(3)]
        [Description("Verifies reflection effect can be applied")]
        public void Image_AddReflection_Success()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagesPath, "test.jpg");
            var image = new Image(imagePath);
            int originalHeight = image.Height;

            // Act
            image.Reflect(SKColors.White, 128);

            // Assert
            Assert.IsTrue(image.Height > originalHeight, "Height should increase after reflection");

            // Cleanup
            image.Close();
        }

        #endregion

        #region Resource Management Tests

        [TestMethod]
        [TestCategory("Image")]
        [TestCategory("Disposal")]
        [Priority(1)]
        [Description("Verifies proper resource cleanup on Close")]
        public void Image_Close_DisposesResources()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagesPath, "test.jpg");
            var image = new Image(imagePath);

            // Act
            image.Close();

            // Assert
            Assert.AreEqual(0, image.Width, "Width should be 0 after close");
            Assert.AreEqual(0, image.Height, "Height should be 0 after close");
        }

        [TestMethod]
        [TestCategory("Image")]
        [TestCategory("Disposal")]
        [Priority(1)]
        [Description("Verifies ReLoad works after Close")]
        public void Image_ReLoadAfterClose_Success()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagesPath, "test.jpg");
            var image = new Image(imagePath);
            image.Close();

            // Act
            image.ReLoad();

            // Assert
            Assert.AreEqual(800, image.Width, "Width should be restored");
            Assert.AreEqual(600, image.Height, "Height should be restored");

            // Cleanup
            image.Close();
        }

        #endregion

        #region Integration Tests

        [TestMethod]
        [TestCategory("Image")]
        [TestCategory("Integration")]
        [Priority(3)]
        [Description("Verifies complete resize and save workflow")]
        public void Image_ResizeAndSave_EndToEnd()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagesPath, "test-large.jpg");
            var outputPath = Path.Combine(_testOutputPath, "resized.jpg");
            var image = new Image(imagePath);
            image.KeepXYRelation = true;

            // Act
            image.SetMaxSize(800, 600);
            bool saveResult = image.Save(outputPath, 85);

            // Assert
            Assert.IsTrue(saveResult, "Save should succeed");
            Assert.IsTrue(File.Exists(outputPath), "Output file should exist");

            // Verify output
            var outputImage = new Image(outputPath);
            Assert.AreEqual(800, outputImage.Width, "Output width should be 800");
            Assert.IsTrue(outputImage.Height <= 600, "Output height should be <= 600");

            // Cleanup
            image.Close();
            outputImage.Close();
        }

        [TestMethod]
        [TestCategory("Image")]
        [TestCategory("Integration")]
        [Priority(3)]
        [Description("Verifies complete crop, watermark and save workflow")]
        public void Image_CropWatermarkAndSave_EndToEnd()
        {
            // Arrange
            var imagePath = Path.Combine(_testImagesPath, "test.jpg");
            var outputPath = Path.Combine(_testOutputPath, "processed.jpg");
            var image = new Image(imagePath);
            image.IsCrop = true;
            image.KeepXYRelation = true;

            // Act
            image.SetMaxSize(300, 300);
            image.AddWatermark("© 2025", "");
            bool saveResult = image.Save(outputPath, 90);

            // Assert
            Assert.IsTrue(saveResult, "Save should succeed");
            Assert.IsTrue(File.Exists(outputPath), "Output file should exist");
            Assert.AreEqual(300, image.Width, "Width should be 300");
            Assert.AreEqual(300, image.Height, "Height should be 300");

            // Cleanup
            image.Close();
        }

        #endregion
    }

    /// <summary>
    /// Unit tests for Protean.Tools.Image.CaptchaImage class.
    /// Tests verify CAPTCHA text generation, image rendering, noise effects,
    /// font handling, and overall CAPTCHA quality after SkiaSharp migration.
    /// </summary>
    [TestClass]
    public class CaptchaImageTests
    {
        #region Basic Functionality Tests

        [TestMethod]
        [TestCategory("CaptchaImage")]
        [TestCategory("Initialization")]
        [Priority(1)]
        [Description("Verifies CaptchaImage can be instantiated with defaults")]
        public void CaptchaImage_Constructor_Success()
        {
            // Act
            var captcha = new Image.CaptchaImage();

            // Assert
            Assert.IsNotNull(captcha, "CaptchaImage should not be null");
            Assert.AreEqual(180, captcha.Width, "Default width should be 180");
            Assert.AreEqual(40, captcha.Height, "Default height should be 40");
            Assert.AreEqual(5, captcha.TextLength, "Default text length should be 5");
            Assert.IsNotNull(captcha.Text, "Text should be generated");
            Assert.IsNotNull(captcha.UniqueId, "UniqueId should be set");
        }

        [TestMethod]
        [TestCategory("CaptchaImage")]
        [TestCategory("Properties")]
        [Priority(1)]
        [Description("Verifies CaptchaImage properties can be set")]
        public void CaptchaImage_SetProperties_Success()
        {
            // Arrange
            var captcha = new Image.CaptchaImage();

            // Act
            captcha.Width = 200;
            captcha.Height = 50;
            captcha.TextLength = 6;
            captcha.Font = "Arial";
            captcha.FontWarp = Image.CaptchaImage.FontWarpFactor.High;
            captcha.BackgroundNoise = Image.CaptchaImage.BackgroundNoiseLevel.Medium;
            captcha.LineNoise = Image.CaptchaImage.LineNoiseLevel.High;

            // Assert
            Assert.AreEqual(200, captcha.Width);
            Assert.AreEqual(50, captcha.Height);
            Assert.AreEqual(6, captcha.TextLength);
            Assert.AreEqual("Arial", captcha.Font);
            Assert.AreEqual(Image.CaptchaImage.FontWarpFactor.High, captcha.FontWarp);
            Assert.AreEqual(Image.CaptchaImage.BackgroundNoiseLevel.Medium, captcha.BackgroundNoise);
            Assert.AreEqual(Image.CaptchaImage.LineNoiseLevel.High, captcha.LineNoise);
        }

        [TestMethod]
        [TestCategory("CaptchaImage")]
        [TestCategory("Validation")]
        [Priority(1)]
        [Description("Verifies width validation throws exception")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CaptchaImage_SetWidthTooSmall_ThrowsException()
        {
            // Arrange
            var captcha = new Image.CaptchaImage();

            // Act
            captcha.Width = 50; // Should throw
        }

        [TestMethod]
        [TestCategory("CaptchaImage")]
        [TestCategory("Validation")]
        [Priority(1)]
        [Description("Verifies height validation throws exception")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CaptchaImage_SetHeightTooSmall_ThrowsException()
        {
            // Arrange
            var captcha = new Image.CaptchaImage();

            // Act
            captcha.Height = 20; // Should throw
        }

        #endregion

        #region Text Generation Tests

        [TestMethod]
        [TestCategory("CaptchaImage")]
        [TestCategory("TextGeneration")]
        [Priority(2)]
        [Description("Verifies CAPTCHA text is generated with correct length")]
        public void CaptchaImage_GenerateText_CorrectLength()
        {
            // Arrange
            var captcha = new Image.CaptchaImage();
            captcha.TextLength = 8;

            // Act
            string text = captcha.Text;

            // Assert
            Assert.IsNotNull(text, "Text should not be null");
            Assert.AreEqual(8, text.Length, "Text length should be 8");
        }

        [TestMethod]
        [TestCategory("CaptchaImage")]
        [TestCategory("TextGeneration")]
        [Priority(2)]
        [Description("Verifies CAPTCHA text uses only allowed characters")]
        public void CaptchaImage_GenerateText_UsesAllowedChars()
        {
            // Arrange
            var captcha = new Image.CaptchaImage();
            captcha.TextChars = "ABC123";
            captcha.TextLength = 10;

            // Act
            string text = captcha.Text;

            // Assert
            Assert.IsNotNull(text, "Text should not be null");
            foreach (char c in text)
            {
                Assert.IsTrue("ABC123".Contains(c.ToString().ToUpper()),
                    $"Character '{c}' should be from allowed set");
            }
        }

        [TestMethod]
        [TestCategory("CaptchaImage")]
        [TestCategory("TextGeneration")]
        [Priority(2)]
        [Description("Verifies each CAPTCHA generates unique text")]
        public void CaptchaImage_GenerateMultiple_UniqueTexts()
        {
            // Arrange & Act
            var captcha1 = new Image.CaptchaImage();
            var captcha2 = new Image.CaptchaImage();
            var captcha3 = new Image.CaptchaImage();

            // Assert
            Assert.AreNotEqual(captcha1.Text, captcha2.Text, "Text should be unique");
            Assert.AreNotEqual(captcha2.Text, captcha3.Text, "Text should be unique");
            Assert.AreNotEqual(captcha1.UniqueId, captcha2.UniqueId, "UniqueId should be unique");
        }

        #endregion

        #region Image Rendering Tests

        [TestMethod]
        [TestCategory("CaptchaImage")]
        [TestCategory("Rendering")]
        [Priority(2)]
        [Description("Verifies CAPTCHA image can be rendered")]
        public void CaptchaImage_RenderImage_Success()
        {
            // Arrange
            var captcha = new Image.CaptchaImage();

            // Act
            var bitmap = captcha.RenderImage();

            // Assert
            Assert.IsNotNull(bitmap, "Bitmap should not be null");
            Assert.AreEqual(180, bitmap.Width, "Bitmap width should be 180");
            Assert.AreEqual(40, bitmap.Height, "Bitmap height should be 40");

            // Cleanup
            bitmap.Dispose();
        }

        [TestMethod]
        [TestCategory("CaptchaImage")]
        [TestCategory("Rendering")]
        [Priority(2)]
        [Description("Verifies CAPTCHA with custom size renders correctly")]
        public void CaptchaImage_RenderCustomSize_Success()
        {
            // Arrange
            var captcha = new Image.CaptchaImage();
            captcha.Width = 300;
            captcha.Height = 80;

            // Act
            var bitmap = captcha.RenderImage();

            // Assert
            Assert.IsNotNull(bitmap);
            Assert.AreEqual(300, bitmap.Width);
            Assert.AreEqual(80, bitmap.Height);

            // Cleanup
            bitmap.Dispose();
        }

        [TestMethod]
        [TestCategory("CaptchaImage")]
        [TestCategory("Rendering")]
        [Priority(3)]
        [Description("Verifies CAPTCHA with different warp levels renders")]
        public void CaptchaImage_RenderWithWarping_AllLevels()
        {
            // Test all warp levels
            var warpLevels = new[]
            {
                Image.CaptchaImage.FontWarpFactor.None,
                Image.CaptchaImage.FontWarpFactor.Low,
                Image.CaptchaImage.FontWarpFactor.Medium,
                Image.CaptchaImage.FontWarpFactor.High,
                Image.CaptchaImage.FontWarpFactor.Extreme
            };

            foreach (var warpLevel in warpLevels)
            {
                // Arrange
                var captcha = new Image.CaptchaImage();
                captcha.FontWarp = warpLevel;

                // Act
                var bitmap = captcha.RenderImage();

                // Assert
                Assert.IsNotNull(bitmap, $"Bitmap should render with {warpLevel} warp");
                Assert.AreEqual(180, bitmap.Width);
                Assert.AreEqual(40, bitmap.Height);

                // Cleanup
                bitmap.Dispose();
            }
        }

        [TestMethod]
        [TestCategory("CaptchaImage")]
        [TestCategory("Rendering")]
        [Priority(3)]
        [Description("Verifies CAPTCHA with different noise levels renders")]
        public void CaptchaImage_RenderWithNoise_AllLevels()
        {
            // Test all background noise levels
            var noiseLevels = new[]
            {
                Image.CaptchaImage.BackgroundNoiseLevel.None,
                Image.CaptchaImage.BackgroundNoiseLevel.Low,
                Image.CaptchaImage.BackgroundNoiseLevel.Medium,
                Image.CaptchaImage.BackgroundNoiseLevel.High,
                Image.CaptchaImage.BackgroundNoiseLevel.Extreme
            };

            foreach (var noiseLevel in noiseLevels)
            {
                // Arrange
                var captcha = new Image.CaptchaImage();
                captcha.BackgroundNoise = noiseLevel;

                // Act
                var bitmap = captcha.RenderImage();

                // Assert
                Assert.IsNotNull(bitmap, $"Bitmap should render with {noiseLevel} noise");

                // Cleanup
                bitmap.Dispose();
            }
        }

        [TestMethod]
        [TestCategory("CaptchaImage")]
        [TestCategory("Rendering")]
        [Priority(3)]
        [Description("Verifies CAPTCHA with different line noise levels renders")]
        public void CaptchaImage_RenderWithLines_AllLevels()
        {
            // Test all line noise levels
            var lineLevels = new[]
            {
                Image.CaptchaImage.LineNoiseLevel.None,
                Image.CaptchaImage.LineNoiseLevel.Low,
                Image.CaptchaImage.LineNoiseLevel.Medium,
                Image.CaptchaImage.LineNoiseLevel.High,
                Image.CaptchaImage.LineNoiseLevel.Extreme
            };

            foreach (var lineLevel in lineLevels)
            {
                // Arrange
                var captcha = new Image.CaptchaImage();
                captcha.LineNoise = lineLevel;

                // Act
                var bitmap = captcha.RenderImage();

                // Assert
                Assert.IsNotNull(bitmap, $"Bitmap should render with {lineLevel} lines");

                // Cleanup
                bitmap.Dispose();
            }
        }

        #endregion

        #region Font Tests

        [TestMethod]
        [TestCategory("CaptchaImage")]
        [TestCategory("Font")]
        [Priority(2)]
        [Description("Verifies custom font can be set")]
        public void CaptchaImage_SetCustomFont_Success()
        {
            // Arrange
            var captcha = new Image.CaptchaImage();

            // Act
            captcha.Font = "Courier New";
            var bitmap = captcha.RenderImage();

            // Assert
            Assert.IsNotNull(bitmap, "Should render with custom font");
            Assert.AreEqual("Courier New", captcha.Font);

            // Cleanup
            bitmap.Dispose();
        }

        [TestMethod]
        [TestCategory("CaptchaImage")]
        [TestCategory("Font")]
        [Priority(2)]
        [Description("Verifies invalid font falls back to default")]
        public void CaptchaImage_SetInvalidFont_FallsBackToDefault()
        {
            // Arrange
            var captcha = new Image.CaptchaImage();

            // Act
            captcha.Font = "NonExistentFont12345";
            var bitmap = captcha.RenderImage();

            // Assert
            Assert.IsNotNull(bitmap, "Should render with fallback font");
            Assert.AreEqual("Arial", captcha.Font, "Should fallback to Arial");

            // Cleanup
            bitmap.Dispose();
        }

        [TestMethod]
        [TestCategory("CaptchaImage")]
        [TestCategory("Font")]
        [Priority(2)]
        [Description("Verifies font whitelist is used for random fonts")]
        public void CaptchaImage_RandomFont_UsesWhitelist()
        {
            // Arrange
            var captcha = new Image.CaptchaImage();
            captcha.FontWhitelist = "Arial;Courier New";
            captcha.Font = ""; // Empty to trigger random

            // Act
            var bitmap = captcha.RenderImage();

            // Assert
            Assert.IsNotNull(bitmap, "Should render with random font from whitelist");

            // Cleanup
            bitmap.Dispose();
        }

        #endregion

        #region Integration Tests

        [TestMethod]
        [TestCategory("CaptchaImage")]
        [TestCategory("Integration")]
        [Priority(3)]
        [Description("Verifies complete CAPTCHA generation and save workflow")]
        public void CaptchaImage_GenerateAndSave_EndToEnd()
        {
            // Arrange
            var captcha = new Image.CaptchaImage();
            captcha.Width = 250;
            captcha.Height = 60;
            captcha.TextLength = 6;
            captcha.FontWarp = Image.CaptchaImage.FontWarpFactor.Medium;
            captcha.BackgroundNoise = Image.CaptchaImage.BackgroundNoiseLevel.Low;
            captcha.LineNoise = Image.CaptchaImage.LineNoiseLevel.Medium;

            var outputPath = Path.Combine(Path.GetTempPath(), "captcha_test.png");

            try
            {
                // Act
                var bitmap = captcha.RenderImage();

                using (var image = SKImage.FromBitmap(bitmap))
                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (var stream = File.OpenWrite(outputPath))
                {
                    data.SaveTo(stream);
                }

                // Assert
                Assert.IsTrue(File.Exists(outputPath), "CAPTCHA file should be saved");
                Assert.IsNotNull(captcha.Text, "CAPTCHA text should be available");
                Assert.AreEqual(6, captcha.Text.Length, "Text should be 6 characters");

                // Cleanup
                bitmap.Dispose();
            }
            finally
            {
                if (File.Exists(outputPath))
                    File.Delete(outputPath);
            }
        }

        [TestMethod]
        [TestCategory("CaptchaImage")]
        [TestCategory("Integration")]
        [Priority(3)]
        [Description("Verifies CAPTCHA uniqueness across multiple generations")]
        public void CaptchaImage_GenerateMultiple_AllUnique()
        {
            // Arrange
            const int count = 10;
            var captchas = new Image.CaptchaImage[count];
            var texts = new string[count];
            var ids = new string[count];

            // Act
            for (int i = 0; i < count; i++)
            {
                captchas[i] = new Image.CaptchaImage();
                texts[i] = captchas[i].Text;
                ids[i] = captchas[i].UniqueId;
            }

            // Assert
            // Check all texts are unique
            for (int i = 0; i < count; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    Assert.AreNotEqual(texts[i], texts[j],
                        $"CAPTCHA texts at {i} and {j} should be different");
                    Assert.AreNotEqual(ids[i], ids[j],
                        $"CAPTCHA IDs at {i} and {j} should be different");
                }
            }
        }

        [TestMethod]
        [TestCategory("CaptchaImage")]
        [TestCategory("Performance")]
        [Priority(3)]
        [Description("Verifies CAPTCHA generation performance")]
        public void CaptchaImage_PerformanceBenchmark_Success()
        {
            // Arrange
            const int iterations = 100;
            var startTime = DateTime.Now;

            // Act
            for (int i = 0; i < iterations; i++)
            {
                var captcha = new Image.CaptchaImage();
                using (var bitmap = captcha.RenderImage())
                {
                    // Just render, don't save
                }
            }

            var elapsed = DateTime.Now - startTime;

            // Assert
            Assert.IsTrue(elapsed.TotalSeconds < 10,
                $"Should generate 100 CAPTCHAs in less than 10 seconds (took {elapsed.TotalSeconds:F2}s)");
        }

        #endregion
    }
}
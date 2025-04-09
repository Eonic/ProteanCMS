using System.Drawing;
using static Protean.Tools.Text;

namespace Protean
{

    public class ImgVerify
    {

        private System.Web.HttpContext moCtx = System.Web.HttpContext.Current;
        public System.Web.SessionState.HttpSessionState goSession;

        public ImgVerify()
        {
            goSession = moCtx.Session;
        }

        /// <summary>
        /// Returns a random password 
        /// DEPRECATED: use Protean.Tools.Text.RandomPassword instead
        /// </summary>
        /// <param name="size">The length of the password required</param>
        /// <param name="lowerCase">Convert the password to lowercase</param>
        /// <returns>String - a randomly generated password</returns>
        /// <remarks>DEPRECATED: use Protean.Tools.Text.RandomPassword instead</remarks>
        public static string getRandomPassword(int size, bool lowerCase)
        {

            var options = TextOptions.UseAlpha | TextOptions.UseNumeric;
            if (lowerCase)
                options = options | TextOptions.LowerCase;
            return RandomPassword(size, options: options);

        }


        public Bitmap generateImage(string sTextToImg = "")
        {
            // 
            if (string.IsNullOrEmpty(sTextToImg))
            {
                sTextToImg = getRandomPassword(5, true);
            }
            // Here, i haven't used any try..catch 
            // Dim pxImagePattern As PixelFormat = PixelFormat.Format32bppArgb
            // Dim bmpImage As New Bitmap(1, 1, pxImagePattern)
            // Dim fntImageFont As New Font("Trebuchets", 14)
            // Dim gdImageGrp As Graphics = Graphics.FromImage(bmpImage)
            // Dim iWidth As Single = gdImageGrp.MeasureString(sTextToImg, fntImageFont).Width
            // Dim iHeight As Single = gdImageGrp.MeasureString(sTextToImg, fntImageFont).Height
            // bmpImage = New Bitmap(CInt(iWidth), CInt(iHeight), pxImagePattern)
            // gdImageGrp = Graphics.FromImage(bmpImage)
            // gdImageGrp.Clear(Color.White)
            // gdImageGrp.TextRenderingHint = TextRenderingHint.AntiAlias
            // gdImageGrp.DrawString(sTextToImg, fntImageFont, New SolidBrush(Color.Red), 0, 0)
            // gdImageGrp.Flush()
            // goSession("imgVerification") = sTextToImg
            // Return bmpImage

            var ci = new Tools.Image.CaptchaImage();

            System.Drawing.Bitmap b = ci.RenderImage();

            goSession["imgVerification"] = ci.Text;

            return b;

        }

    }


    //public class ImageHelper : Tools.Image
    //{

    //    public ImageHelper(string Location) : base(Location)
    //    {
    //    }

    //    // For the PDF Thumbnail functionality to work you need to copy (GhostScript) gsdll.dll into Windows/System32

    //    public class PDFThumbNail
    //    {
    //        public string FilePath;
    //        public string newImageFilepath;
    //        public short maxWidth;
    //        public System.Web.HttpServerUtility goServer;
    //    }

    //    public void GeneratePDFThumbNail(PDFThumbNail PDFThumbNail)
    //    {
    //        try
    //        {
    //            string cCheckServerPath = PDFThumbNail.newImageFilepath.Substring(0, PDFThumbNail.newImageFilepath.LastIndexOf("/") + 1);
    //            cCheckServerPath = goServer.MapPath(cCheckServerPath);
    //            if (Directory.Exists(cCheckServerPath) == false)
    //            {
    //                Directory.CreateDirectory(cCheckServerPath);
    //            }

    //            var LayerBuilder = new SoundInTheory.DynamicImage.Fluent.PdfLayerBuilder().SourceFileName(PDFThumbNail.goServer.MapPath(PDFThumbNail.FilePath)).PageNumber(1).WithFilter(SoundInTheory.DynamicImage.Fluent.FilterBuilder.Resize.ToWidth(500)).WithFilter(SoundInTheory.DynamicImage.Fluent.FilterBuilder.Resize.ToWidth(PDFThumbNail.maxWidth)).WithFilter(SoundInTheory.DynamicImage.Fluent.FilterBuilder.Border.Width(1).Fill(SoundInTheory.DynamicImage.Colors.Black));


    //            var imgComp = new SoundInTheory.DynamicImage.Fluent.CompositionBuilder().WithLayer(LayerBuilder);
    //            imgComp.ImageFormat(SoundInTheory.DynamicImage.DynamicImageFormat.Png);
    //            imgComp.SaveTo(PDFThumbNail.goServer.MapPath(PDFThumbNail.newImageFilepath));
    //            imgComp = null;
    //            LayerBuilder = null;
    //        }
    //        catch (Exception ex)
    //        {
    //            Information.Err().Raise(8032, "PDFThumbNail", ex.Message);
    //        }
    //    }
    //}
}
using SelectPdf;
using System;
using System.IO;
using System.Xml;

namespace Protean.Tools
{
    public class PDF

    {
        public static event ErrorEventHandler OnError;
        public delegate void ErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);
        private const string mcModuleName = "Protean.Tools.PDF";

        public MemoryStream GetPDFstream(XmlElement xPageXml, string XsltPath, string cFontPath, string cPageSize = "")
        {
            {
                try
                {
                    System.IO.MemoryStream ofileStream = new System.IO.MemoryStream();
                    string styleFile = System.Convert.ToString(XsltPath);
                    Protean.Tools.Xslt.Transform oTransform = new Protean.Tools.Xslt.Transform();
                    oTransform.XslTFile = XsltPath;
                    oTransform.Xml = xPageXml;
                    oTransform.Compiled = false;

                    string pdfDocumentXml = oTransform.Process();
                    //check if 'html' node exists in string
                    if (pdfDocumentXml.ToLower().Contains("<html"))
                    {  //PDF code
                        HtmlToPdf converter = new HtmlToPdf();
                        converter.Options.MaxPageLoadTime = 15;
                        //PdfCustomPageSize pageSize = PdfCustomPageSize.A4;

                        if (cPageSize != string.Empty)
                        {
                            if (cPageSize.ToLower() == "a4")
                            {
                                PdfCustomPageSize pageSize = PdfCustomPageSize.A4;
                            }
                            else if (cPageSize.ToLower() == "a5")
                            {

                                converter.Options.AutoFitWidth = HtmlToPdfPageFitMode.ShrinkOnly;
                                converter.Options.AutoFitHeight = HtmlToPdfPageFitMode.ShrinkOnly;
                                converter.Options.PdfPageSize = PdfPageSize.A5;

                                PdfCustomPageSize pageSize = PdfCustomPageSize.A5;
                            }
                            else
                            {
                                PdfCustomPageSize pageSize = PdfCustomPageSize.A4;
                            }
                        }
                        else
                        {
                            PdfCustomPageSize pageSize = PdfCustomPageSize.A4;
                        }
                        PdfDocument pdfDoc = converter.ConvertHtmlString(pdfDocumentXml, "");
                        pdfDoc.Save(ofileStream);
                        byte[] oStreamByteArray = ofileStream.ToArray();
                        converter = null;
                        pdfDoc.Close();
                        pdfDoc = null;

                        return new MemoryStream(oStreamByteArray);

                    }
                    else
                    {


                        Fonet.FonetDriver oFoNet = new Fonet.FonetDriver();
                        System.IO.StringReader oTxtReader = new System.IO.StringReader(pdfDocumentXml);
                        oFoNet.CloseOnExit = false;

                        Fonet.Render.Pdf.PdfRendererOptions rendererOpts = new Fonet.Render.Pdf.PdfRendererOptions();

                        rendererOpts.Author = "ProteanCMS";
                        rendererOpts.EnablePrinting = true;
                        rendererOpts.FontType = Fonet.Render.Pdf.FontType.Embed;
                        // rendererOpts.Kerning = True
                        // rendererOpts.EnableCopy = True

                        DirectoryInfo dir = new DirectoryInfo(cFontPath);
                        DirectoryInfo[] subDirs = dir.GetDirectories();
                        FileInfo[] files = dir.GetFiles();
                        // FileInfo fi = default(FileInfo);

                        foreach (FileInfo fi in files)
                        {
                            string cExt = fi.Extension.ToLower();
                            switch (cExt)
                            {
                                case ".otf":
                                    {
                                        rendererOpts.AddPrivateFont(fi);
                                        break;
                                    }
                            }
                        }

                        oFoNet.Options = rendererOpts;
                        oFoNet.Render(oTxtReader, ofileStream);
                        byte[] Buffer = ofileStream.ToArray();

                        return new MemoryStream(Buffer);
                    }

                }


                catch (Exception ex)
                {
                    OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetPDFstream", ex, ""));
                    return null;
                }

            }

        }
        public MemoryStream GetPDFstream(string XmlFO, string FontPath)
        {
            {
                try
                {

                    string foNetXml = XmlFO;

                    // now we use FO.Net to generate our PDF

                    Fonet.FonetDriver oFoNet = new Fonet.FonetDriver();
                    System.IO.MemoryStream ofileStream = new System.IO.MemoryStream();
                    System.IO.StringReader oTxtReader = new System.IO.StringReader(foNetXml);
                    oFoNet.CloseOnExit = false;

                    Fonet.Render.Pdf.PdfRendererOptions rendererOpts = new Fonet.Render.Pdf.PdfRendererOptions();

                    rendererOpts.Author = "ProteanCMS";
                    rendererOpts.EnablePrinting = true;
                    rendererOpts.FontType = Fonet.Render.Pdf.FontType.Embed;
                    // rendererOpts.Kerning = True
                    // rendererOpts.EnableCopy = True

                    DirectoryInfo dir = new DirectoryInfo(FontPath);
                    DirectoryInfo[] subDirs = dir.GetDirectories();
                    FileInfo[] files = dir.GetFiles();
                    // FileInfo fi = default(FileInfo);

                    foreach (FileInfo fi in files)
                    {
                        string cExt = fi.Extension.ToLower();
                        switch (cExt)
                        {
                            case ".otf":
                                {
                                    rendererOpts.AddPrivateFont(fi);
                                    break;
                                }
                        }
                    }

                    oFoNet.Options = rendererOpts;
                    oFoNet.Render(oTxtReader, ofileStream);


                    byte[] Buffer = ofileStream.ToArray();

                    return new MemoryStream(Buffer);

                }
                catch (Exception ex)
                {
                    OnError?.Invoke(null/* TODO Change to default(_) if this is not a reference type */, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "GetPDFstream", ex, ""));
                    return null;
                }
            }
        }


        public class PDFThumbNail
        {
            public string FilePath;
            public string newImageFilepath;
            public short maxWidth;
            public System.Web.HttpServerUtility goServer;
        }

        public void GeneratePDFThumbNail(PDFThumbNail PDFThumbNail)
        {
            try
            {
                string cCheckServerPath = PDFThumbNail.newImageFilepath.Substring(0, PDFThumbNail.newImageFilepath.LastIndexOf("/") + 1);
                cCheckServerPath = PDFThumbNail.goServer.MapPath(cCheckServerPath);
                if (Directory.Exists(cCheckServerPath) == false)
                {
                    Directory.CreateDirectory(cCheckServerPath);
                }

                var LayerBuilder = new SoundInTheory.DynamicImage.Fluent.PdfLayerBuilder().SourceFileName(PDFThumbNail.goServer.MapPath(PDFThumbNail.FilePath)).PageNumber(1).WithFilter(SoundInTheory.DynamicImage.Fluent.FilterBuilder.Resize.ToWidth(500)).WithFilter(SoundInTheory.DynamicImage.Fluent.FilterBuilder.Resize.ToWidth(PDFThumbNail.maxWidth)).WithFilter(SoundInTheory.DynamicImage.Fluent.FilterBuilder.Border.Width(1).Fill(SoundInTheory.DynamicImage.Colors.Black));


                var imgComp = new SoundInTheory.DynamicImage.Fluent.CompositionBuilder().WithLayer(LayerBuilder);
                imgComp.ImageFormat(SoundInTheory.DynamicImage.DynamicImageFormat.Png);
                imgComp.SaveTo(PDFThumbNail.goServer.MapPath(PDFThumbNail.newImageFilepath));
                imgComp = null;
                LayerBuilder = null;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error 8032 in PDFThumbNail: {ex.Message}");
            }
        }

        public void Close()
        {
            // closes
            try
            {

            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, new Protean.Tools.Errors.ErrorEventArgs(mcModuleName, "Close", ex, ""));
            }
            finally
            {

            }
        }
    }
}

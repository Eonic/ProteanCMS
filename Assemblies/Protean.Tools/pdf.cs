using System;
using System.Linq;
using System.IO;
using System.Xml;
using SelectPdf;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Web;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Web.UI.WebControls;
using System.Runtime.InteropServices.ComTypes;

namespace Protean.Tools
{
    public class PDF

    {
        public static event ErrorEventHandler OnError;
        public delegate void ErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);
        private const string mcModuleName = "Protean.Tools.PDF";

        public MemoryStream GetPDFstream(XmlElement xPageXml, string XsltPath, string FontPath)
        {
            {
                try
                {
                    System.IO.MemoryStream ofileStream = new System.IO.MemoryStream();
                    // Next we transform using into FO.Net Xml
                    string filePath = HttpContext.Current.Server.MapPath(XsltPath);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xPageXml.OuterXml);
                    XPathDocument xPathDoc = new XPathDocument(filePath);
                    XsltArgumentList argsList = new XsltArgumentList();
                    argsList.AddParam("GiftMessageHtmlString", "", xPageXml.OuterXml.ToString());
                    XslCompiledTransform transform = new XslCompiledTransform(true);
                    transform.Load(filePath);
                    System.IO.TextWriter sWriter = new System.IO.StringWriter();
                    System.Xml.XmlTextReader oReader = new XmlTextReader(new StringReader(doc.OuterXml));
                    transform.Transform(oReader, argsList, sWriter);
                    string htmlString = sWriter.ToString();

                    //string styleFile = HttpContext.Current.Server.MapPath(XsltPath);

                    //TextWriter sWriter = new StringWriter();
                    //XmlDocument xmlDoc = new XmlDocument();
                    //xmlDoc.LoadXml(xPageXml.OuterXml.ToString());
                    //Protean.Tools.Xslt.Transform oTransform = new Protean.Tools.Xslt.Transform();

                    //oTransform.XslTFile = styleFile;
                    ////oTransform.Xml = xmlDoc;
                    //oTransform.Compiled = false;

                    //string foNetXml = oTransform.Process();
                    string baseUrl = "";

                    // now we use FO.Net to generate our PDF
                    if (htmlString.Contains("<html"))
                    {  //PDF code
                        //HtmlToPdf converter = new HtmlToPdf();
                        //converter.Options.WebPageWidth = 1024;
                        //converter.Options.WebPageHeight = 0;

                        //PdfDocument pdfDoc = converter.ConvertHtmlString(htmlString, baseUrl);

                        //pdfDoc.Save(ofileStream);
                        //byte[] Buffer = ofileStream.ToArray();
                        var writer = new StreamWriter(ofileStream);
                        writer.Write(htmlString);
                        writer.Flush();
                        ofileStream.Position = 0;
                        byte[] Buffer = ofileStream.ToArray();

                        return new MemoryStream(Buffer);

                    }
                    else
                    {


                        Fonet.FonetDriver oFoNet = new Fonet.FonetDriver();
                        System.IO.StringReader oTxtReader = new System.IO.StringReader(htmlString);
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
    }
}

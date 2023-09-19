using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;


namespace Protean.Tools
{
    public class PDF

    {
        public static event ErrorEventHandler OnError;
        public delegate void ErrorEventHandler(object sender, Protean.Tools.Errors.ErrorEventArgs e);
        private const string mcModuleName = "Protean.Tools.PDF";

        public MemoryStream GetPDFstream(XmlElement xPageXml ,string XsltPath, string FontPath) {
            {
                try
                {
                    // Next we transform using into FO.Net Xml

                    string styleFile = System.Convert.ToString(XsltPath);

                    
                Protean.Tools.Xslt.Transform oTransform = new Protean.Tools.Xslt.Transform();
                oTransform.XslTFile = XsltPath;
                oTransform.Xml = xPageXml;
                oTransform.Compiled = false;

                string foNetXml = oTransform.Process();                           

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

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Syncfusion.Drawing;
using Syncfusion.Pdf.HtmlToPdf;

namespace TrifortPdfServiceDocker.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PdfController : ControllerBase
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public PdfController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        //[HttpPost]
        //[Route("FromUrl")]
        //public IActionResult ConvertFromUrl([FromBody] ConvertFromUrlRequest request)
        //{
        //    //Initialize HTML to PDF converter with Blink settings
        //    HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.Blink);
        //    BlinkConverterSettings blinkConverterSettings = new BlinkConverterSettings();

        //    //blinkConverterSettings.BlinkPath = Directory.GetCurrentDirectory() + @"/BlinkBinariesWindows";
        //    blinkConverterSettings.BlinkPath = @"/BlinkBinariesWindows";

        //    //Set page margins
        //    blinkConverterSettings.Margin = new PdfMargins() { All = 0 };

        //    //Set page orientation
        //    //blinkConverterSettings.Orientation = providedSettings.Orientation ?? PdfPageOrientation.Portrait;

        //    //Set rotation
        //    blinkConverterSettings.PageRotateAngle = (PdfPageRotateAngle)0;

        //    //Enable Javascript
        //    blinkConverterSettings.EnableJavaScript = true;
        //    //Enable Hyperlink
        //    blinkConverterSettings.EnableHyperLink = true;
        //    //Enable Form
        //    blinkConverterSettings.EnableForm = false;
        //    //Enabel Toc
        //    blinkConverterSettings.EnableToc = false;
        //    //Enable Bookmark
        //    blinkConverterSettings.EnableBookmarks = false;

        //    //if (request.Cookies != null)
        //    //{
        //    //    var cookieStrings = request.Cookies.Split(";");
        //    //    var cookies = new Syncfusion.HtmlConverter.CookieCollection();

        //    //    foreach (var cookieString in cookieStrings)
        //    //    {
        //    //        var cookieNameAndDescription = cookieString.Split("=");
        //    //        cookies.Add(cookieNameAndDescription[0].Trim(), cookieNameAndDescription[1].Trim());
        //    //    }

        //    //    blinkConverterSettings.Cookies = cookies;
        //    //}

        //    //Set WebKit viewport size
        //    int viewportWidth = 1024;

        //    int viewportHeight = 0;

        //    //Assign WebKit settings to HTML converter
        //    htmlConverter.ConverterSettings = blinkConverterSettings;

        //    //Convert url to pdf
        //    PdfDocument document = htmlConverter.Convert(request.Url);

        //    MemoryStream stream = new MemoryStream();

        //    //Save and close the output PDF document
        //    document.Save(stream);
        //    document.Close();

        //    return File(stream.ToArray(), System.Net.Mime.MediaTypeNames.Application.Pdf, "Test.pdf");
        //}

        [HttpPost]
        [Route("FromHtml")]
        public IActionResult ConvertFromHtml([FromBody] ConvertFromHtmlRequest request)
        {
            //Initialize HTML to PDF converter with Webkit settings
            //HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.WebKit);
            //WebKitConverterSettings webKitConverterSettings = new WebKitConverterSettings();
            //webKitConverterSettings.WebKitPath = Directory.GetCurrentDirectory() + @"/QtBinariesWindows";
            //webKitConverterSettings.Margin = new PdfMargins() { All = 0 };
            //webKitConverterSettings.Orientation = PdfPageOrientation.Portrait;
            //webKitConverterSettings.PageRotateAngle = (PdfPageRotateAngle)0;
            //webKitConverterSettings.EnableJavaScript = true;
            //webKitConverterSettings.EnableHyperLink = false;
            //webKitConverterSettings.EnableForm = true;
            //webKitConverterSettings.EnableToc = false;
            //webKitConverterSettings.EnableBookmarks = false;

            //Initialize HTML to PDF converter with Blink settings
            HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.Blink);
            BlinkConverterSettings blinkConverterSettings = new BlinkConverterSettings();
            blinkConverterSettings.Margin = new PdfMargins() { All = 0 };
            blinkConverterSettings.Orientation = PdfPageOrientation.Portrait;
            blinkConverterSettings.PageRotateAngle = (PdfPageRotateAngle)0;
            blinkConverterSettings.EnableJavaScript = true;
            blinkConverterSettings.EnableHyperLink = false;
            blinkConverterSettings.EnableForm = false;
            blinkConverterSettings.EnableToc = false;
            blinkConverterSettings.EnableBookmarks = false;

            //Set Blink path
            blinkConverterSettings.BlinkPath = Path.Combine(_hostingEnvironment.ContentRootPath, "BlinkBinariesLinux");

            //Set command line arguments to run without sandbox.
            blinkConverterSettings.CommandLineArguments.Add("--no-sandbox");
            blinkConverterSettings.CommandLineArguments.Add("--disable-setuid-sandbox");

            if (!string.IsNullOrWhiteSpace(request.HeaderHtml))
                blinkConverterSettings.PdfHeader = CreateHeader(request.HeaderHtml);

            if (!string.IsNullOrWhiteSpace(request.FooterHtml))
                blinkConverterSettings.PdfFooter = CreateFooter(request.FooterHtml);

            //Assign WebKit settings to HTML converter
            htmlConverter.ConverterSettings = blinkConverterSettings;

            //Convert url to pdf
            //For now just hardcode the baseUrl for the images and resources
            PdfDocument document = htmlConverter.Convert(request.Html, "http://qtrakwebdev.azurewebsites.net/");

            if (!string.IsNullOrWhiteSpace(request.WatermarkText))
            {
                PdfPageBase loadedPage = document.Pages[0];

                PdfGraphics graphics = loadedPage.Graphics;

                PdfGraphicsState state = graphics.Save();

                graphics.SetTransparency(0.25f);

                //Translate the coordinate system to the center of image
                graphics.TranslateTransform(PdfPageSize.A4.Width / 2, PdfPageSize.A4.Height / 2);
                //Rotate the coordinate system
                graphics.RotateTransform(-40);
                graphics.TranslateTransform(-PdfPageSize.A4.Width / 2, -PdfPageSize.A4.Height / 2);

                //set the font

                PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 200);
                PdfStringFormat format = new PdfStringFormat();

                format.Alignment = PdfTextAlignment.Center;
                format.LineAlignment = PdfVerticalAlignment.Middle;

                // watermark text.

                PdfStringLayouter layouter = new PdfStringLayouter();
                // Initializing PdfStringLayoutResult in order to determine whether the string is clipped or not
                PdfStringLayoutResult result = layouter.Layout(request.WatermarkText, font, new PdfStringFormat(PdfTextAlignment.Center), PdfPageSize.A4);
                RectangleF bounds = new RectangleF(0, 0, PdfPageSize.A4.Width, PdfPageSize.A4.Height);
                //This condition will pass if there is clipped text present with the given bounds value
                if (result.Lines != null && result.Lines.Length > 1)
                {
                    //Method which resize the font size in order to fit the text inside the specified bounds
                    PdfFont resizedFont = GetResizedFont(request.WatermarkText, bounds, font);
                    //Draw the text
                    graphics.DrawString(request.WatermarkText, resizedFont, PdfBrushes.Black, bounds, format);
                }
                else
                {
                    //Draw the text
                    graphics.DrawString(request.WatermarkText, font, PdfBrushes.Black, bounds, format);
                }

                graphics.Restore(state);

                //PdfTemplate watermarkTemplate = CreateWatermarkTemplate(request.WatermarkText);

                //graphics.Save();

                //graphics.SetTransparency(0.75f);

                //graphics.DrawPdfTemplate(watermarkTemplate, new PointF(0, 500), PdfPageSize.A4);

                //graphics.SetTransparency(0.75f);

                //graphics.Restore();
            }



            MemoryStream stream = new MemoryStream();

            //Save and close the output PDF document
            document.Save(stream);
            document.Close();

            var byteArray = stream.ToArray();

            return File(byteArray, "application/octet-stream");
        }

        private PdfPageTemplateElement CreateHeader(string headerHtml)
        {
            HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.Blink);
            BlinkConverterSettings blinkConverterSettings = new BlinkConverterSettings();
            blinkConverterSettings.BlinkPath = Path.Combine(_hostingEnvironment.ContentRootPath, "BlinkBinariesLinux");
            blinkConverterSettings.PdfPageSize = new SizeF(PdfPageSize.A4.Width, 60);
            blinkConverterSettings.CommandLineArguments.Add("--no-sandbox");
            blinkConverterSettings.CommandLineArguments.Add("--disable-setuid-sandbox");
            htmlConverter.ConverterSettings = blinkConverterSettings;

            PdfDocument headerDocument = htmlConverter.Convert(headerHtml, "http://qtrakwebdev.azurewebsites.net/");

            RectangleF bounds = new RectangleF(0, 0, headerDocument.Pages[0].GetClientSize().Width, 30);

            PdfPageTemplateElement header = new PdfPageTemplateElement(bounds);

            header.Graphics.DrawPdfTemplate(headerDocument.Pages[0].CreateTemplate(), bounds.Location, bounds.Size);

            return header;
        }

        private PdfPageTemplateElement CreateFooter(string footerHtml)
        {
            HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.Blink);
            BlinkConverterSettings blinkConverterSettings = new BlinkConverterSettings();
            blinkConverterSettings.BlinkPath = Path.Combine(_hostingEnvironment.ContentRootPath, "BlinkBinariesLinux");
            blinkConverterSettings.PdfPageSize = new SizeF(PdfPageSize.A4.Width, 60);
            blinkConverterSettings.CommandLineArguments.Add("--no-sandbox");
            blinkConverterSettings.CommandLineArguments.Add("--disable-setuid-sandbox");
            htmlConverter.ConverterSettings = blinkConverterSettings;

            PdfDocument footerDocument = htmlConverter.Convert(footerHtml, "http://qtrakwebdev.azurewebsites.net/");

            RectangleF bounds = new RectangleF(0, 0, footerDocument.Pages[0].GetClientSize().Width, 60);

            PdfPageTemplateElement footer = new PdfPageTemplateElement(bounds);

            footer.Graphics.DrawPdfTemplate(footerDocument.Pages[0].CreateTemplate(), bounds.Location, bounds.Size);

            PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 8);
            PdfSolidBrush brush = new PdfSolidBrush(Color.Black);
            PdfPageNumberField pageNumber = new PdfPageNumberField(font, brush);
            PdfPageCountField count = new PdfPageCountField(font, brush);
            PdfCompositeField compositeField = new PdfCompositeField(font, brush, "Page {0} of {1}", pageNumber, count);
            compositeField.Bounds = footer.Bounds;
            compositeField.Draw(footer.Graphics, new PointF(30, 40));

            return footer;
        }

        private PdfTemplate CreateWatermarkTemplate(string watermarkHtml)
        {
            HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter(HtmlRenderingEngine.Blink);
            BlinkConverterSettings blinkConverterSettings = new BlinkConverterSettings();
            blinkConverterSettings.BlinkPath = Path.Combine(_hostingEnvironment.ContentRootPath, "BlinkBinariesLinux");
            //blinkConverterSettings.PdfPageSize = PdfPageSize.A4;
            blinkConverterSettings.CommandLineArguments.Add("--no-sandbox");
            blinkConverterSettings.CommandLineArguments.Add("--disable-setuid-sandbox");
            blinkConverterSettings.ViewPortSize = PdfPageSize.A4.ToSize();
            htmlConverter.ConverterSettings = blinkConverterSettings;

            PdfDocument watermarkDocument = htmlConverter.Convert(watermarkHtml, "http://qtrakwebdev.azurewebsites.net/");

            //watermarkDocument.Pages[0].Graphics.SetTransparency(1f);

            return watermarkDocument.Pages[0].CreateTemplate();
        }

        private PdfFont GetResizedFont(string text, RectangleF bounds, PdfFont font)
        {
            PdfFont pdfFont = font;
            var fontSize = pdfFont.Size;
            while (fontSize > 0f && pdfFont.MeasureString(text).Width > bounds.Width)
            {
                fontSize -= 0.5f;
                if (font is PdfCjkStandardFont)
                    pdfFont = new PdfCjkStandardFont((PdfCjkStandardFont)font, fontSize, font.Style);
                else if (font is PdfStandardFont)
                    pdfFont = new PdfStandardFont((PdfStandardFont)font, fontSize, font.Style);
                else if (font is PdfTrueTypeFont)
                    pdfFont = new PdfTrueTypeFont((PdfTrueTypeFont)font, fontSize);

            }
            //Assign the resized font
            font = pdfFont;
            return pdfFont;
        }
    }
}

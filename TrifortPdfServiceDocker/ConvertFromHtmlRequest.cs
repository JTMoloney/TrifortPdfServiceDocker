using Syncfusion.HtmlConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrifortPdfServiceDocker
{
    public class ConvertFromHtmlRequest
    {
        public string HeaderHtml { get; set; }
        public string Html { get; set; }
        public string FooterHtml { get; set; }
        public string WatermarkText { get; set; }
        //public BlinkConverterSettings? Settings { get; set; }
    }
}

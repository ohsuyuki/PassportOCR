using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Tesseract;

namespace PassportOCR
{
    class OcrEngine
    {
        virtual public bool Init()
        {
            return true;
        }

        virtual public string Go(string filePath)
        {
            return "Todo implement";
        }

        virtual public string Go(string filePath, int top, int left, int buttom, int right)
        {
            return "Todo implement";
        }

        protected string GetMrz(string text)
        {
            Match match = Regex.Match(text, "^(?<mrz>.*<.*?)$", RegexOptions.Singleline);
            return match.Groups["mrz"].Value;
        }
    }

    class OcrEngineTesseract : OcrEngine
    {
        override public string Go(string filePath)
        {
            var page = tesseract_.Process(new Bitmap(filePath));
            return GetMrz(page.GetText());
        }

        override public string Go(string filePath, int top, int left, int buttom, int right)
        {
            var page = tesseract_.Process(new Bitmap(filePath), new Rect(left, top, right-left, buttom-top));
            return page.GetText().Trim('\n');
        }

        private TesseractEngine tesseract_ = new TesseractEngine(Directory.GetCurrentDirectory() + "\\tesseract", "eng");
    }

    class OcrEngineIMdOcr90 : OcrEngine
    {
        override public string Go(string filePath)
        {
            return "";
        }
    }
}

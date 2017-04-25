using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassportOCR
{
    class Program
    {
        public const string PASSPORT_IMAGE = "passportImage";
        public const string EXPECTED = "expected";
        public const string ACTUAL = "actual";
        public const string MISS_COUNT = "missCount";
        public const string MISS_PAIR = "missPair";
        public const string MRZ_RECT = "mrzRect";

        public const string MISS_PAIR_SEPARATOR = "|";

        public const string TESSERACT = "";

        static void Main(string[] args)
        {
            string engine = args[0];
            string input = args[1];
            string output = args[2];

            OcrEngine ocrEngine = CreateOcrEngine(engine);

            List<Dictionary<string, object>> inputDataList = CreateInputData(input);

            List<Dictionary<string, object>> ocrEngineResultList = new List<Dictionary<string, object>>();
            foreach (Dictionary<string, object> inputData in inputDataList)
            {
                string iunputFilePath = (string)(inputData[PASSPORT_IMAGE]);
                Rectangle rect = (Rectangle)(inputData[MRZ_RECT]);
                string ocrResult = ocrEngine.Go(iunputFilePath, rect);

                string expected = (string)(inputData[EXPECTED]);
                Dictionary<string, object> compareResult = Compare(ocrResult, expected);

                ocrEngineResultList.Add(compareResult);
            }

            Save(output, ocrEngineResultList);
        }

        static OcrEngine CreateOcrEngine(string engine)
        {
            if (string.Compare(engine, "Tesseract", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return new OcrEngineTesseract();
            }
            else if (string.Compare(engine, "IMdOcr", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return new OcrEngineIMdOcr90();
            }
            else
            {
                throw new NotImplementedException(string.Format("{0} is not implement", engine));
            }
        }

        static List<Dictionary<string, object>> CreateInputData(string inputFilePath)
        {
            List<Dictionary<string, object>> inputDataList = new List<Dictionary<string, object>>();
            using (StreamReader sr = new StreamReader(inputFilePath))
            {
                while (sr.Peek() >= 0)
                {
                    string line = sr.ReadLine();

                    if(line.StartsWith("//"))
                    {
                        continue;
                    }

                    string[] parserd = line.Split(',');
                    if(parserd == null || parserd.Length < 6)
                    {
                        continue;
                    }

                    int x = int.Parse(parserd[2]);
                    int y = int.Parse(parserd[3]);
                    int width = int.Parse(parserd[4]) - x;
                    int height = int.Parse(parserd[5]) - y;

                    Dictionary<string, object> inputData = new Dictionary<string, object>
                    {
                        { PASSPORT_IMAGE, parserd[0] },
                        { EXPECTED, parserd[1].Replace('&', '\n')},
                        { MRZ_RECT, new Rectangle(x, y, width, height)}
                    };
                    inputDataList.Add(inputData);
                }
                return inputDataList;
            }
        }

        static Dictionary<string, object> Compare(string actual, string expected)
        {
            Dictionary<string, object> compareResult = new Dictionary<string, object>
            {
                { EXPECTED, expected.Replace('\n','&')},
                { ACTUAL, actual.Replace('\n', '&')},
            };

            int missCount = 0;
            List<string> missPair = new List<string>();

            string[] actualLines = actual.Split('\n');
            string[] expectedLine = expected.Split('\n');

            int firstLine = GetNextLine(actualLines, 0);
            missCount += CompareCore(actualLines[firstLine], expectedLine[0], missPair);

            int secondLine = GetNextLine(actualLines, firstLine + 1);
            missCount += CompareCore(actualLines[secondLine], expectedLine[1], missPair);

            compareResult.Add(MISS_COUNT, missCount);
            compareResult.Add(MISS_PAIR, missPair);

            return compareResult;
        }

        static void Save(string ouputFilePath, List<Dictionary<string, object>> outputData)
        {
            using (StreamWriter sw = new StreamWriter(ouputFilePath))
            {
                foreach (Dictionary<string, object> data in outputData)
                {
                    string dataStr = string.Format("{0},{1},{2}", data[MISS_COUNT], data[EXPECTED], data[ACTUAL]);
                    List<string> missPair = (List<string>)(data[MISS_PAIR]);
                    if (missPair != null)
                    {
                        dataStr = string.Format("{0},{1}", dataStr, string.Join(",", missPair));
                    }
                    sw.WriteLine(dataStr);
                }
            }
        }

        static int GetNextLine(string[] lines, int startLine)
        {
            for (int i = startLine; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i]) == false)
                {
                    return i;
                }
            }
            return -1;
        }

        static int CompareCore(string actual, string expected, List<string> missPair)
        {
            int missCount = 0;
            for (int i = 0; i < actual.Length; i++)
            {
                if (actual[i] != expected[i])
                {
                    missCount++;
                    missPair.Add(string.Format("{0}{1}{2}", expected[i], MISS_PAIR_SEPARATOR, actual[i]));
                }
            }
            return missCount;
        }
    }
}

using System;
using System.Collections.Generic;
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
                string ocrResult = ocrEngine.Go(iunputFilePath, 566, 146, 655, 1023);

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
                    string[] parserd = line.Split(',');

                    Dictionary<string, object> inputData = new Dictionary<string, object>
                    {
                        { PASSPORT_IMAGE, parserd[0] },
                        { EXPECTED, parserd[1].Replace('&', '\n')}
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
            for (int i=0; i<actual.Length; i++)
            {
                if(actual[i] != expected[i])
                {
                    missCount++;
                    missPair.Add(string.Format("{0}{1}{2}", expected[i], MISS_PAIR_SEPARATOR, actual[i]));
                }
            }
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
                    if(missPair != null)
                    {
                        dataStr = string.Format("{0},{1}", dataStr, string.Join(",", missPair));
                    }
                    sw.WriteLine(dataStr);
                }
            }
        }
    }
}

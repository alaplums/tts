using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TTS_server_alap_alpha_v1
{
   static class LookUpFile
    {

        private static string FilePath = Path.GetDirectoryName((new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath);
        private static Dictionary<string, string> lookUpTable = new Dictionary<string, string>();
        private static Dictionary<string, string> MathCharLookUp = new Dictionary<string, string>();

        private static void ReadFile()
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(FilePath + "\\lookup.txt");
                string[] Mathlines = System.IO.File.ReadAllLines(FilePath + "\\MathChar.txt");
                // System.IO.File.ReadAllLines(@"D:\study\MS\Thesis\Latex Code\tts-server\TTS_server_alap_alpha_v1\bin\Debug\lookup.txt");

                if (lookUpTable.Count <= 0)
                {
                    foreach (string line in lines)
                    {
                        string[] KeyValue = line.Split(',');
                        lookUpTable.Add(KeyValue[0], KeyValue[1]);
                    }
                }
                if (MathCharLookUp.Count <= 0)
                {
                    foreach (string line in Mathlines)
                    {
                        string[] KeyValue = line.Split(',');
                        MathCharLookUp.Add(KeyValue[0], KeyValue[1]);
                    }
                }
            } catch(Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            
        }

        public static Dictionary<string, string> GetLookTable()
        {
            if (lookUpTable.Count <= 0)
            {
                ReadFile();
                return lookUpTable;
            }
            else
            {
                return lookUpTable;
            }
        }
        public static Dictionary<string, string> GetMathCharLookTable()
        {           
                ReadFile();
                return MathCharLookUp;             
        }
    }
}

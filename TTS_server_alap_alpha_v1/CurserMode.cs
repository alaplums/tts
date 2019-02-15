using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTS_server_alap_alpha_v1
{
    static class CurserMode
    {
        static Dictionary<string, string> MathCharlookupTable = new Dictionary<string, string>();
        public static Boolean IsCurserMode(string Data)
        {
            return checkStaticString(Data);
        }

        private static Boolean checkStaticString(string Data)
        {
            if (Data.Length == 1 || Data.All(char.IsDigit) || Data.Equals("space") || Data.Equals("no further text")
                || Data.Equals("empty line") || Data.Contains("Caps") || Data.Contains(ConfigurationManager.AppSettings["DataReceived_EditorCmnd"]))
            {
                return true;
            }
            else
                return false;
        }
        private static void MathCharLookupTable()
        {
            MathCharlookupTable = LookUpFile.GetMathCharLookTable();
        }

        public static string ParseSource(string dataReceived)
        {
            if (MathCharlookupTable.Count <= 0)
                MathCharLookupTable();
            string ValueByKey = "";

            string symbols = String.Join("", MathCharlookupTable.Keys);
            foreach (char c in symbols)
            {
                bool hasValue = false;
                if (dataReceived.Contains(c.ToString()))
                {
                    hasValue = MathCharlookupTable.TryGetValue(c.ToString(), out ValueByKey);
                    if (hasValue)
                    {
                        dataReceived = dataReceived.Replace(c.ToString(), " " + ValueByKey + " ");
                    }
                }
            }      
            return dataReceived;
        }
    }
}

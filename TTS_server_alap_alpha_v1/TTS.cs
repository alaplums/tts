using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace TTS_server_alap_alpha_v1
{
    static class TTS
    {
       private static SpeechSynthesizer reader;
       private static SpeechSynthesizer tempReader;

        public static void setReader()
        {
            reader = new SpeechSynthesizer();
            tempReader = new SpeechSynthesizer();
            
        }
        public static SpeechSynthesizer getReader()
        {
            return reader;
        }
        public static void VBRCharModeTTSSpeak(string dataReceived)
        {
            //reader.Rate = -3;
            PromptBuilder pb2 = new PromptBuilder();
            string str2 = "<say-as interpret-as=\"characters\">" + dataReceived + "</say-as>";
            pb2.AppendSsmlMarkup(str2);
            reader.SpeakAsync(pb2);
            //reader.Rate = 0;
        }
        public static void Speak(string FinalTextString)
        {
            if (reader.State == SynthesizerState.Paused)
            {
                tempReader.SpeakAsyncCancelAll();
                tempReader.SpeakAsync("TTS is in Pause State. Please Press Caps lock  E  to continue with TTS");
                Console.WriteLine("End of Speach ");
            }
            else
            {
                PromptBuilder pb2 = new PromptBuilder(new System.Globalization.CultureInfo("en-US"));
               // string str2 = "<say-as interpret-as=\"characters\">" + dataReceived + "</say-as>";
                //pb2.AppendSsmlMarkup(FinalTextString);
                
                if (FinalTextString.Contains("asteriklinebreakasterik")) //split string at lineBreak and append break
                {
                    string[] lineBreak = new string[] { "asteriklinebreakasterik" };
                    string[] result = FinalTextString.Split(lineBreak, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string v in result)
                    {
                        string[] pauses = new string[] { "\r\n" };
                        string[] pausesResult = v.Split(pauses, StringSplitOptions.RemoveEmptyEntries);
                        foreach(var r in pausesResult)
                        {
                            pb2.AppendText(r);
                            pb2.AppendBreak(PromptBreak.Small);
                        }
                        pb2.AppendBreak(TimeSpan.FromSeconds(2));
                    }

                    reader.SpeakAsync(pb2);
                }
                else
                {
                    reader.SpeakAsync(FinalTextString);
                }
                Console.WriteLine("End of Speach ");
            }
        }
        public static void TTS_Pause()
        {
            reader.Pause();
        }

        public static void TTS_Resume()
        {
            reader.Resume();
        }

        public static void TTS_SetSpeed(int speed)
        {
            reader.Rate = speed;
        }

        public static void TTS_ChangeVoice(string voiceName)
        {
            reader.SelectVoice(voiceName);
            //reader.GetInstalledVoices();
        }
    }
}

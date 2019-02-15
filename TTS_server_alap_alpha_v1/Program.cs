using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Configuration;
using System.Diagnostics;
using System.Windows.Automation;
using System.Runtime.InteropServices;

namespace TTS_server_alap_alpha_v1 
{
    class Program
    {
        const int PORT_NO = 5000;
        static string ValueByKey = "";
        static bool isError = false;
        static bool isPrevError = false;
        //static bool isMisSpelled = false;
        static bool isSpeicalCommand = false;
        static bool isVoiceChange = false;
        const string SERVER_IP = "127.0.0.1";
        static Dictionary<string, string> lookupTable = new Dictionary<string, string>();
        static List<string> keyList = new List<string>(lookupTable.Keys);
        public static string strAppendLatexDocument = "";
        public static bool isPDfAccMode = false;
        static string currentVoice = "Microsoft David Desktop";
        static int currentSpeed = 0;
        static string focusedUIElement = "";

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags,
        UIntPtr dwExtraInfo);


        static void Main(string[] args)
        {


            try
            {
                while (true)
                    SetServer();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Resetting server" + ex.Message);
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = TTSProperties.CMND_KILL_TTS;
                process.StartInfo = startInfo;
                process.Start();
                //SetServer();
            }
        }

        private static void SetServer()
        {
            try
            {
                TTS.setReader();

                AutomationFocusChangedEventHandler focusHandler = null;
                focusHandler = new AutomationFocusChangedEventHandler(OnFocusChange);
                Automation.AddAutomationFocusChangedEventHandler(focusHandler);

                IPAddress localAdd = IPAddress.Parse(SERVER_IP);

                TcpListener listener = new TcpListener(localAdd, PORT_NO);
                Console.WriteLine(ConfigurationManager.AppSettings["ServerInitalState"]);
                listener.Start();

                //---incoming client connected--- 
                TcpClient client = listener.AcceptTcpClient();

                listener.Stop();
                bool isVrbstyChar = false;
                bool isPDFMode = false;



                while (client.Connected)
                {
                    NetworkStream nwStream = client.GetStream();

                    byte[] buffer = new byte[client.ReceiveBufferSize];

                    //---read incoming stream--- 
                    int bytesRead = 0;
                    string dataReceived = string.Empty;

                    try
                    {
                        bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
                    }
                    catch (IOException e)
                    {
                        // break;  
                    }
                    //---convert the data received into a string--- 
                    dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    //Console.WriteLine("Recevie Data From Client: " + dataReceived);

                    if (dataReceived.Length == 1)
                    {
                        CancelCurrentStream();
                        dataReceived = CurserMode.ParseSource(dataReceived);
                        TTS.Speak(dataReceived);
                    }
                    else if (dataReceived.Equals(ConfigurationManager.AppSettings["DataReceived_CHK_CAPSLOCK"]))
                    {
                        if (Console.CapsLock)
                        {
                            const int KEYEVENTF_EXTENDEDKEY = 0x1;
                            const int KEYEVENTF_KEYUP = 0x2;
                            const byte VK_LSHIFT = 0xA0; // left shift key
                            const byte VK_CAPSLOCK = 0x14;

                            keybd_event(VK_LSHIFT, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0); //press the shift key
                            //press Caps Lock
                            keybd_event(VK_CAPSLOCK, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
                            keybd_event(VK_CAPSLOCK, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0); 
                            keybd_event(VK_LSHIFT, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0); //release the shift key              
                        }
                    }
                    else if (dataReceived.Equals(ConfigurationManager.AppSettings["DataReceived_Next"])
                            || dataReceived.Equals(ConfigurationManager.AppSettings["DataReceived_Stop"]))
                    {
                        if (TTS.getReader().State == SynthesizerState.Paused)
                        {
                            TTS.getReader().Dispose();
                            TTS.setReader();
                            TTS.TTS_ChangeVoice(currentVoice);//restore previous voice
                            TTS.TTS_SetSpeed(currentSpeed);//restore previous speed
                        }
                        else if (!isError)
                            TTS.getReader().SpeakAsyncCancelAll();
                        if (dataReceived.Equals(ConfigurationManager.AppSettings["DataReceived_Next"]))
                        {
                            isError = true;
                            isPrevError = true;
                        }
                        dataReceived = string.Empty;
                        isPDFMode = false;
                        //isVrbstyChar = false;
                    }
                    // for PDF mode
                    else if (dataReceived.Equals(ConfigurationManager.AppSettings["DataReceived_PDFAccessibilityMode"]) || isPDfAccMode)
                    {
                        if (isPDfAccMode)
                        {
                            if (dataReceived.ToLower().Contains("\\end{document}"))
                            {
                                dataReceived = strAppendLatexDocument + dataReceived;
                                strAppendLatexDocument = "";

                                MathMode obj = new MathMode(dataReceived);
                                obj.filterLatexSourceCodeUsingOpenDetex();
                                //PdfAccessibilityMode obj = new PdfAccessibilityMode(dataReceived);
                                //obj.ConvertLatexEquationToText();

                                //---write back the text to the client---
                                byte[] sendingClientBuffer = Encoding.ASCII.GetBytes(MathMode.latexDocument);
                                Console.WriteLine("Sending back Client : " + MathMode.latexDocument);
                                nwStream.Write(sendingClientBuffer, 0, sendingClientBuffer.Length);
                                MathMode.latexDocument = "";
                                dataReceived = null;
                                nwStream.Flush();
                                isPDfAccMode = false;
                            }
                            else
                            {
                                strAppendLatexDocument = dataReceived + "\n";
                            }
                        }
                        else
                        {
                            dataReceived = "";
                            isPDfAccMode = true;
                        }
                    }

                    else if (isPDFMode)
                    {
                        MathMode mathObj = new MathMode(dataReceived);
                        mathObj.filterLatexSourceCodeUsingOpenDetex();
                        dataReceived = "";
                        isPDFMode = false;
                    }

                    else if (dataReceived.Equals(ConfigurationManager.AppSettings["DataReceived_PDFMode"]))
                    {
                        CancelCurrentStream();
                        isPDFMode = true;
                        dataReceived = string.Empty;
                        Console.WriteLine(TTSProperties.MSG_PDF_MODE_ENABLED);
                    }
                    else if (dataReceived.Equals(ConfigurationManager.AppSettings["DataReceived_Pause"]))
                    {
                        TTS.TTS_Pause();
                        dataReceived = string.Empty;
                        Console.WriteLine(TTSProperties.MSG_TTS_PAUSED);
                    }
                    else if (dataReceived.Equals(ConfigurationManager.AppSettings["DataReceived_Resume"]))
                    {
                        TTS.TTS_Resume();
                        dataReceived = string.Empty;
                        Console.WriteLine(TTSProperties.MSG_TTS_RESUMED);
                    }
                    else if (dataReceived.Equals(ConfigurationManager.AppSettings["DataReceived_WordVrbsty"]))
                    {
                        isVrbstyChar = false;
                        dataReceived = string.Empty;
                    }
                    else if (dataReceived.Equals(ConfigurationManager.AppSettings["DataReceived_CharVrbsty"]))
                    {
                        isVrbstyChar = true;
                        dataReceived = string.Empty;
                    }
                    else if (dataReceived.Equals(ConfigurationManager.AppSettings["DataReceived_SpecialCmnd"]))
                    {
                        if (!isError)
                            CancelCurrentStream();
                        isSpeicalCommand = true;
                        dataReceived = string.Empty;
                    }
                    else if (dataReceived.Equals(ConfigurationManager.AppSettings["DataReceived_IncreaseSpeed"]))
                    {
                        int speed = TTS.getReader().Rate + 1;
                        currentSpeed = speed;
                        TTS.TTS_SetSpeed(speed);
                        dataReceived = string.Empty;
                        Console.WriteLine(TTSProperties.MSG_SPEED_INCREASED);
                    }
                    else if (dataReceived.Equals(ConfigurationManager.AppSettings["DataReceived_DecreaseSpeed"]))
                    {
                        int speed = TTS.getReader().Rate - 1;
                        currentSpeed = speed;
                        TTS.TTS_SetSpeed(speed);
                        dataReceived = string.Empty;
                        Console.WriteLine(TTSProperties.MSG_SPEED_DECREASED);
                    }
                    else if (CurserMode.IsCurserMode(dataReceived))
                    {
                        string editorCmndValue = ConfigurationManager.AppSettings["DataReceived_EditorCmnd"];
                        if (dataReceived.Contains(editorCmndValue))
                        {
                            dataReceived = dataReceived.Contains(editorCmndValue) ? dataReceived.Replace(editorCmndValue, "") : dataReceived;
                            if (isVrbstyChar && !isError)
                            {
                                CancelCurrentStream();
                                //dataReceived = Encoding.Default.GetString(buffer, 0, bytesRead);
                                TTS.VBRCharModeTTSSpeak(dataReceived);
                                //isVrbstyChar = false;
                                continue;
                            }
                        }
                        dataReceived = CurserMode.ParseSource(dataReceived);
                        if (!isError)
                            CancelCurrentStream();
                        TTS.Speak(dataReceived);
                    }
                    else if (dataReceived.Equals(ConfigurationManager.AppSettings["DataReceived_ChangeVoice"]))
                    {
                        isVoiceChange = true;
                        dataReceived = string.Empty;
                    }
                    else if (isVoiceChange)
                    {
                        isVoiceChange = false;
                        currentVoice = dataReceived;
                        TTS.TTS_ChangeVoice(dataReceived);
                        dataReceived = string.Empty;
                        Console.WriteLine(TTSProperties.MSG_VOICE_CHANGED);
                    }
                    else if (!string.IsNullOrEmpty(dataReceived))
                    {
                        LookupTable();
                        if (isVrbstyChar && !isError && !isSpeicalCommand)
                        {
                            CancelCurrentStream();
                            //dataReceived = Encoding.Default.GetString(buffer, 0, bytesRead);
                            TTS.VBRCharModeTTSSpeak(dataReceived);
                            //isVrbstyChar = false;
                            continue;
                        }
                        dataReceived = TextLineParsing(dataReceived);
                        if (!isError)
                            CancelCurrentStream();
                        TTS.Speak(dataReceived);
                        isError = false;
                        isSpeicalCommand = false;
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void CancelCurrentStream()
        {
            if (TTS.getReader().State == SynthesizerState.Speaking && !isError && !isPrevError)
            {
                TTS.getReader().SpeakAsyncCancelAll();
                TTS.setReader();
                TTS.TTS_ChangeVoice(currentVoice);//restore previous voice
                TTS.TTS_SetSpeed(currentSpeed);//restore previous speed
            }

            if (isPrevError) isPrevError = false;
        }

        private static string TextLineParsing(string DataStr)
        {
            string ParsedString = string.Empty;
            string[] Lines = DataStr.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach (string Line in Lines)
            {
                if (Line.Contains("\\"))
                {
                    string TempStr = LatexParsing(Line);
                    ParsedString = ParsedString + TempStr;
                }
                else
                {
                    ParsedString = ParsedString + Line;
                }
            }
            ParsedString = ParsedString.Replace(@"{", lookupTable[@"{"]).Replace(@"}", lookupTable[@"}"]);
            return ParsedString;
        }

        private static string LatexParsing(string dataReceived)
        {
            string[] LatexErrorText = dataReceived.Split(' ');
            foreach (string word in LatexErrorText)
            {
                if (word.Contains("\\") || dataReceived.Contains(@"^") || dataReceived.Contains(@"}") || dataReceived.Contains(@"{"))
                {
                    bool hasValue = lookupTable.TryGetValue(word, out ValueByKey);
                    if (hasValue)
                    {
                        dataReceived = dataReceived.Replace(word, ValueByKey);
                    }
                }
            }
            return dataReceived;
        }

        private static void LookupTable()
        {
            lookupTable = LookUpFile.GetLookTable();
        }

        private static void OnFocusChange(object src, AutomationFocusChangedEventArgs e)
        {
            AutomationElement elementFocused = src as AutomationElement;

            try
            {
                focusedUIElement = elementFocused.GetCurrentPropertyValue(AutomationElement.NameProperty).ToString();
                Console.WriteLine("focusedUIElement " + elementFocused.GetCurrentPropertyValue(ValuePattern.ValueProperty).ToString());
                if (!String.IsNullOrEmpty(focusedUIElement) && focusedUIElement != "Text Editor")
                {
                    CancelCurrentStream();
                    focusedUIElement = shortcutKeysTable(focusedUIElement);
                    TTS.Speak(focusedUIElement);
                }
            }
            catch (Exception ex)
            {
                //throw;
            }
          
        }

        private static string shortcutKeysTable(string focusedUIElement)
        {
            if (focusedUIElement.ToLower().Equals(LatexCommands.DESC_CURSOR_CONT_TTS))
            {
                focusedUIElement = focusedUIElement + LatexCommands.SHRTCUT_CURSOR_CONT_TTS;
            }
            else if (focusedUIElement.ToLower().Equals(LatexCommands.DESC_DECREASE_SPEED))
            {
                focusedUIElement = focusedUIElement + LatexCommands.SHRTCUT_DECREASE_SPEED;
            }
            else if (focusedUIElement.ToLower().Equals(LatexCommands.DESC_INCREASE_SPEED))
            {
                focusedUIElement = focusedUIElement + LatexCommands.SHRTCUT_INCREASE_SPEED;
            }
            else if (focusedUIElement.ToLower().Equals(LatexCommands.DESC_MATH_MODE))
            {
                focusedUIElement = focusedUIElement + LatexCommands.SHRTCUT_MATH_MODE;
            }
            else if (focusedUIElement.ToLower().Equals(LatexCommands.DESC_PDF_ACCESSIBILITY))
            {
                focusedUIElement = focusedUIElement + LatexCommands.SHRTCUT_PDF_ACCESSIBILITY;
            }
            else if (focusedUIElement.ToLower().Equals(LatexCommands.DESC_TTS_OFF))
            {
                focusedUIElement = focusedUIElement + LatexCommands.SHRTCUT_TTS_OFF;
            }
            else if (focusedUIElement.ToLower().Equals(LatexCommands.DESC_TTS_PAUSE))
            {
                focusedUIElement = focusedUIElement + LatexCommands.SHRTCUT_TTS_PAUSE;
            }
            else if (focusedUIElement.ToLower().Equals(LatexCommands.DESC_TTS_RESUME))
            {
                focusedUIElement = focusedUIElement + LatexCommands.SHRTCUT_TTS_RESUME;
            }
            else if (focusedUIElement.ToLower().Equals(LatexCommands.DESC_TTS_START))
            {
                focusedUIElement = focusedUIElement + LatexCommands.SHRTCUT_TTS_START;
            }
            else if (focusedUIElement.ToLower().Equals(LatexCommands.DESC_TTS_STOP))
            {
                focusedUIElement = focusedUIElement + LatexCommands.SHRTCUT_TTS_STOP;
            }
            else if (focusedUIElement.ToLower().Equals(LatexCommands.DESC_VERB_CHAR))
            {
                focusedUIElement = focusedUIElement + LatexCommands.SHRTCUT_VERB_CHAR;
            }
            else if (focusedUIElement.ToLower().Equals(LatexCommands.DESC_VERB_WORD))
            {
                focusedUIElement = focusedUIElement + LatexCommands.SHRTCUT_VERB_WORD;
            }

            return focusedUIElement;
        }
    }
}

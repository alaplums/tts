using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TTS_server_alap_alpha_v1
{
    public class DetexModification
    {
        static Dictionary<string, string> MathCharlookupTable = new Dictionary<string, string>();
        static List<string> latexExtractedCommands = new List<string>();

        public static bool  isVerbatim = false;
        public static bool isTitleNote = false;
        Stack<char> titleNoteStack = new Stack<char>();

        public DetexModification()
        {
        }

        public string manualDetex(string aLine)
        {
            if(aLine != null)
            {
                if (checkForLatexTextCommands(aLine))
                {
                    handleLatexExtractableCommands(ref aLine);
                }
                if (isTitleNote)
                {
                    titleNoteStackLogic(ref aLine);
                }
                handleVerbatim(ref aLine);
                handleReplaceLineLatexCommands(ref aLine);
                handlemiscellenousLatexCommands(ref aLine);
                
            }
            return aLine;
        }

        private void handleVerbatim(ref string aLine)
        {
            if (aLine.Contains("\\begin{verbatim}"))
            {
                isVerbatim = true;
                aLine = aLine.Replace("\\begin{verbatim}", "");
            }
            if (aLine.Contains("\\end{verbatim}"))
            {
                isVerbatim = false;
                aLine = aLine.Replace("\\end{verbatim}", "");
            }
            if (isVerbatim)
            {
                aLine = CurserMode.ParseSource(aLine);
            }
        }

        private string handleReplaceLineLatexCommands(ref string aLine)
        {
            string lowerLine = aLine.ToLower();
            LatexCommands.populatereplaceLineLatexCommands();
            if (LatexCommands.getReplaceLineLatexCommands().Where(x => lowerLine.Contains(x)).Count() > 0)
            {
                aLine = " ";
            }
            return aLine;
        }

        private string handleLatexExtractableCommands(ref string aLine)
        {
            string lowerLine = aLine.ToLower();
            lowerLine = Regex.Replace(lowerLine,@"\s", "");
            LatexCommands.populateLatexExtractableCommandsDict();

            foreach (string command in LatexCommands.getLatexExtractableCommandsList())
            {
                if (latexExtractedCommands.Contains(command))
                {
                    string ValueType = "";
                    bool hasValue = LatexCommands.getLatexExtractableCommandsDict().TryGetValue(command,out ValueType);
                    if (hasValue)
                    {
                        aLine = aLine.ToLower().Replace(command," " + ValueType + " ");
                    }
                    if (command.Equals("\\titlenote") || command.Equals("\\footnote"))
                    {
                        isTitleNote = true;
                    }
                }
            }
            return aLine;
        }

        private string handlemiscellenousLatexCommands(ref string aLine)
        {
            string lowerLine = aLine.ToLower();
            lowerLine = Regex.Replace(lowerLine, @"\s", "");
            LatexCommands.populateMiscellenousLatexTextCommands();
            LatexCommands.populateMiscellenousLatexTextCommandsDict();
            foreach (string command in LatexCommands.getMiscellenousLatexTextCommands())
            {
                if (lowerLine.Contains(command))
                {
                    string ValueType = "";
                    bool hasValue = LatexCommands.getMiscellenousLatexTextCommandsDict().TryGetValue(command, out ValueType);
                    if (hasValue)
                    {
                        aLine = aLine.ToLower().Replace(command, " " + ValueType + " ");
                    }
                }
            }
            return aLine;
        }

        private bool checkForLatexTextCommands(string aLine)
        {
            LatexCommands.populatelatexExtractableCommandsList();
            latexExtractedCommands.Clear(); //clear previous commands 
            extractLatex(aLine);
            foreach (var v in LatexCommands.getLatexExtractableCommandsList())
            {
                if (latexExtractedCommands.Contains(v))
                {
                    return true;
                }
            }
            return false;
        }

        private void extractLatex(string line)
        {
            string pattern = @"^(\s\\|\\)([a-zA-Z]*)(\s|[a-zA-Z])\b|(\s\\|\\)([a-zA-Z]*)(\s|[a-zA-Z])"; //to extract latex command
            Match m = Regex.Match(line, pattern);
            while (m.Success)
            {
                latexExtractedCommands.Add((m.Value).Trim());
                m = m.NextMatch();
            }
        }

        private void titleNoteStackLogic(ref string aline)
        {
            string command = aline.Contains("\\titlenote") ? "\\titlenote" : "\\footnote";
            //replace [] arguments
            aline = aline.Trim();
            if(aline.Contains("[") && aline.Contains("]"))
            {
                aline = aline.Replace(aline.Substring(aline.IndexOf('['), aline.IndexOf(']') + 1 - aline.IndexOf('[')), "");
            }
            string replaceText = aline.Contains("\\titlenote") ? " title note " : " foot note ";
            Stack<char> origExpression = new Stack<char>();
            char[] titleNoteArray;
            string orignalline = "";
            string charArrayLine = aline;
            if (aline.Contains(command))
            {
                charArrayLine = aline.Substring(aline.IndexOf(command));
                charArrayLine = charArrayLine.Replace(command, "");
                aline = aline.Replace(command, replaceText);

            }
            titleNoteArray = charArrayLine.ToArray();
            int i = 0;

            while (i < titleNoteArray.Length)
            {
                char c = titleNoteArray[i];
                StackLogic:
                if (c != '}')
                {
                    titleNoteStack.Push(c);
                    origExpression.Push(c);
                    i++;
                }
                else
                {
                    while (titleNoteStack.Peek() != '{')
                    {
                        titleNoteStack.Pop();
                    }
                    titleNoteStack.Pop();

                    if (titleNoteStack.Count > 0)
                    {
                        i++;
                        goto StackLogic;
                    }
                    if (titleNoteStack.Count == 0)
                    {
                        isTitleNote = false;
                        while (origExpression.Count != 0)
                        {
                            orignalline += origExpression.Pop().ToString();
                        }
                        orignalline = new String(orignalline.ToCharArray().Reverse().ToArray());
                        aline = aline.Replace(orignalline, orignalline + ". . .end note. . .");
                        //aline = aline + ". . . end title note . . .";
                    }
                    return;
                }
            }

        }
    }
}

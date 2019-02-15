using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using static TTS_server_alap_alpha_v1.PdfMode;

namespace TTS_server_alap_alpha_v1
{
    

    public class PdfAccessibilityMode
    {
        public static string latexEquationText = "";
        string LatexEquation = ""; 

        static Dictionary<string, string> lookupTable = new Dictionary<string, string>();
        static Dictionary<string, string> mathCharLookUpTable = new Dictionary<string, string>();
        List<string> algorithmCommandsArray = new List<string>(); //alogorithms commands
        List<string> textCommands = new List<string>(); //text commands
        List<string> mathEquaionCommands = new List<string>(); //extracted latex commands form the equation
        List<String> declaredAlgoCommandsList = new List<string>(); //list of declared algo commadns
        Stack<char> ifInstructionSet = new Stack<char>();
        Stack<char> eifConditon = new Stack<char>();
        Dictionary<string, string> declaredAlgoCommandsDict = new Dictionary<string, string>(); //list of declared algo commands with lookup text
        List<String> declaredAlgoFunctions = new List<string>(); //list of declared algo commadns
        List<string> multipleCommands = new List<string>(); //list of list/proof/theroem commands
        List<string> tableCommands = new List<string>(); //list of table commands
        Dictionary<string, int> countOfRowSpan = new Dictionary<string, int>();
        List<string> rowSpanList = new List<string>();
        List<string> columnAfterRowSpan = new List<string>();
        List<string> figureCommands = new List<string>(); //list of figure commands
        Dictionary<int, string> diffDegrees = new Dictionary<int, string>(); //differential degree
        List<string> setCommands = new List<string>(); //list of set latex commands
        Dictionary<string, string> declaredCommands = new Dictionary<string, string>(); //list of declared commands 
        List<string> combinatoricsCommands = new List<string>(); //list of multi-coefficent commands
        List<string> matricesCommands = new List<string>(); //list of matrices commands
        Stack<char> borderMatrixStack = new Stack<char>();
        Dictionary<int, string> roots = new Dictionary<int, string>(); //roots
        static List<string> symbols = new List<string>() { "+", "-", "/", "*" };
        Stack<string> endCommands = new Stack<string>();
        Stack<char> bracketStack = new Stack<char>();
        string arrayAlign = " ";
        int matrixColumnCount = 0;
        bool isBorderMatrix = false;
        bool isBorderMatrixEndLine = false;
        bool isEIf = false;
        string ValueByKey = "";
        string verticalBorder = "";
        string algoParsed = "";
        int matrixRowCount = 0;
        int rowCount = 0;
        String parsedEquation;
        public static string ToolTipTextLatex = "";
        public static string LatexDocument = "";
        public static string StrAltEquation="";
        bool IsBeginDocument = false;
        bool IsTagPackage = false;
        public PdfAccessibilityMode(string LatexEquation)
        {
            this.LatexEquation = LatexEquation;
        }

        private void populateAlgorithmCommands()
        {
            if (algorithmCommandsArray.Count <= 0)
            {
                algorithmCommandsArray.Add("\\begin{algorithmic}");
                algorithmCommandsArray.Add("\\end{algorithmic}");
                algorithmCommandsArray.Add("\\begin{algorithm}");
                algorithmCommandsArray.Add("\\end{algorithm}");
                algorithmCommandsArray.Add("\\if");
                algorithmCommandsArray.Add("\\else");
                algorithmCommandsArray.Add("\\endif");
                algorithmCommandsArray.Add("\\procedure");
                algorithmCommandsArray.Add("\\comment");
                algorithmCommandsArray.Add("\\while");
                algorithmCommandsArray.Add("\\endwhile");
                algorithmCommandsArray.Add("\\endprocedure");
                algorithmCommandsArray.Add("\\for");
                algorithmCommandsArray.Add("\\state");
                algorithmCommandsArray.Add("\\elsif");
                algorithmCommandsArray.Add("\\elseif");
                algorithmCommandsArray.Add("\\foreach");
                algorithmCommandsArray.Add("\\for");
                algorithmCommandsArray.Add("\\lfor");
                algorithmCommandsArray.Add("\\uif");
                algorithmCommandsArray.Add("\\uelseif");
                algorithmCommandsArray.Add("\\uelse");
                algorithmCommandsArray.Add("\\kwresult");
                algorithmCommandsArray.Add("\\kwdata");
                algorithmCommandsArray.Add("\\kwto");
                algorithmCommandsArray.Add("\\tcc");
                algorithmCommandsArray.Add("\\tcp");
                algorithmCommandsArray.Add("\\SetKwData");
                algorithmCommandsArray.Add("\\SetKwFunction");
                algorithmCommandsArray.Add("\\SetKwInOut");
                algorithmCommandsArray.Add("\\lif");
                algorithmCommandsArray.Add("\\lelse");
                algorithmCommandsArray.Add("\\lforeach");
                algorithmCommandsArray.Add("\\eif");
                algorithmCommandsArray.Add("\\caption");
            }
        }

        private void populateTextCommands()
        {
            if (textCommands.Count <= 0)
            {
                textCommands.Add("\\text");
                textCommands.Add("\\textrm");
                textCommands.Add("\\textbf");
            }
        }

        private void extractLatex(string mathEquation)
        {
            string pattern = @"^(\s\\|\\)([a-zA-Z]*)(\s|[a-zA-Z])\b|(\s\\|\\)([a-zA-Z]*)(\s|[a-zA-Z])"; //to extract latex command
            Match m = Regex.Match(mathEquation, pattern);
            while (m.Success)
            {
                mathEquaionCommands.Add((m.Value).Trim());
                m = m.NextMatch();
            }
        }
        private bool checkForAlgorithms(string aLine, List<string> declatedAlgoCommandsList)
        {
            if (aLine.Contains("\\begin{algorithmic}") || aLine.Contains("\\end{algorithmic}")
                    || aLine.Contains("\\begin{algorithm}") || aLine.Contains("\\end{algorithm}"))
            {
                return true;
            }
            mathEquaionCommands.Clear(); //clear previous commands 
            string tempParsedEquation = aLine;
            extractLatex(aLine);
            foreach (var v in declatedAlgoCommandsList)
            {
                if (mathEquaionCommands.ConvertAll(d => d.ToLower()).Contains(v.ToLower()))
                {
                    return true;
                }
            }
            foreach (var v in algorithmCommandsArray)
            {
                if (mathEquaionCommands.ConvertAll(d => d.ToLower()).Contains(v.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }

        private string extractCondtionString(string mutatedaLine, char openingBracket, char closingBracket, bool isEIf)
        {
            Stack<char> conditionStack = new Stack<char>();
            Stack<char> origCondition = new Stack<char>();
            Char[] charArrayofLine = mutatedaLine.ToCharArray();
            string condtionString = "";
            foreach (var v in charArrayofLine)
            {
                if (v != closingBracket)
                {
                    conditionStack.Push(v);
                    origCondition.Push(v);
                }
                else
                {
                    conditionStack.Push(closingBracket); //push } bracket
                    origCondition.Push(closingBracket);
                    while (conditionStack.Count != 0 && conditionStack.Peek() != openingBracket)
                    {
                        conditionStack.Pop();
                    }
                    conditionStack.Pop(); //to remove { bracket

                    if (conditionStack.Count <= 0)
                    {
                        break;
                    }
                }
            }
            if (conditionStack.Count == 1 && conditionStack.Peek() == '{')
            {
                conditionStack.Pop();
                origCondition.Pop();
            }
            if (isEIf)
            {
                if (conditionStack.Count == 0)
                {
                    while (origCondition.Count() > 0)
                    {
                        condtionString = condtionString + origCondition.Pop();

                    }
                    condtionString = new String(condtionString.ToCharArray().Reverse().ToArray());
                }

            }
            else
            {
                while (origCondition.Count() > 0)
                {
                    condtionString = condtionString + origCondition.Pop();

                }
                condtionString = new String(condtionString.ToCharArray().Reverse().ToArray());
            }
            return condtionString;
        }

        private string extractCondOrIf(string aLine, string mutatedaLine, Stack<char> lineStack, string replaceText, bool condIfSameLine)
        {
            Stack<char> currentLine = new Stack<char>();
            string currLine = "";
            if (lineStack.Peek() == '0') //pop the dummy character
            {
                lineStack.Pop();
            }
            if (lineStack.Count != 0) //pop instructions on previous line
            {
                while (lineStack.Count != 0)
                {
                    lineStack.Pop();
                }
                lineStack.Push('{');//push the { bracket for if instruction back 

            }

            Char[] charArrayofLine;
            charArrayofLine = mutatedaLine.ToCharArray();
            foreach (var v in charArrayofLine)
            {
                if (v != '}')
                {
                    lineStack.Push(v);
                    currentLine.Push(v);
                }
                else
                {
                    lineStack.Push('}'); //push } bracket
                    currentLine.Push('}');
                    while (lineStack.Count != 0 && lineStack.Peek() != '{')
                    {
                        lineStack.Pop();
                    }
                    if (lineStack.Count != 0 && lineStack.Peek() == '{')
                    {
                        lineStack.Pop(); //to remove { bracket

                    }

                    if (lineStack.Count <= 0)
                    {
                        while (currentLine.Count() > 0)
                        {
                            currLine = currLine + currentLine.Pop();
                        }
                        currLine = new String(currLine.ToCharArray().Reverse().ToArray());
                        aLine = aLine.ToLower().Replace(currLine, currLine + replaceText);
                        break;
                    }
                }
            }

            return aLine;
        }

        private string handleAlgorithms(string aLine)
        {
            List<string> declaredAlgoCommandsListLocal = new List<string>();
            populateAlgorithmCommands();
            populateTextCommands();
            mathEquaionCommands.Clear();
            bool hasValue = false;
            bool isFunction = false;
            bool condIfSameLine = false;
            string mutatedaLine = "";
            List<string> commentCommandsList = new List<string>();

            if (checkForAlgorithms(aLine.ToLower(), declaredAlgoCommandsList) || ifInstructionSet.Count > 0 || eifConditon.Count > 0) //eif instruction has begun
            {
                if (aLine.Contains("\\SetKwData") || aLine.Contains("\\SetKwFunction") || aLine.Contains("\\SetKwInOut"))
                {
                    string pattern = @"({)(\w*)(})|({)(\w*\s)(}|\w)"; //to extract latex command
                    Match m = Regex.Match(aLine, pattern);
                    string value = "";
                    while (m.Success)
                    {
                        if (m.Value.StartsWith("\\SetKwData") || m.Value.StartsWith("\\SetKwFunction") || m.Value.StartsWith("\\SetKwInOut"))
                        {
                            if (m.Value.StartsWith("\\SetKwFunction"))
                            {
                                isFunction = true;
                            }
                            m = m.NextMatch();
                            continue;
                        }
                        value = m.Value; //because m.Value is read-only
                        value = value.ToLower();
                        if (value.Contains("{"))
                        {
                            value = value.Replace("{", "");
                        }
                        if (value.Contains("}"))
                        {
                            value = value.Replace("}", "");
                        }
                        declaredAlgoCommandsListLocal.Add((value).Trim());
                        m = m.NextMatch();
                    }
                    for (int i = 0; i <= (declaredAlgoCommandsListLocal.Count / 2) + 1; i += 2)
                    {
                        declaredAlgoCommandsDict.Add("\\" + declaredAlgoCommandsListLocal.ElementAt(i), declaredAlgoCommandsListLocal.ElementAt(i + 1));
                        declaredAlgoCommandsList.Add("\\" + declaredAlgoCommandsListLocal.ElementAt(i));
                        if (isFunction)
                        {
                            declaredAlgoFunctions.Add("\\" + declaredAlgoCommandsListLocal.ElementAt(i));
                        }
                    }
                    return "";
                }

                if (aLine.Contains("\\begin{algorithmic}") || aLine.Contains("\\end{algorithmic}")
                    || aLine.Contains("\\begin{algorithm}") || aLine.Contains("\\end{algorithm}"))
                {
                    if (aLine.Contains("begin"))
                    {
                        aLine = aLine.Replace(aLine, "begin algorithm");
                    }
                    else
                    {
                        aLine = aLine.Replace(aLine, "end algorithm");
                    }
                }
                else
                {
                    string loweraLine = aLine.ToLower().TrimStart();

                    if (loweraLine.Contains("\\tcp") || loweraLine.Contains("\\tcc"))
                    {
                        string commentCommand = "";
                        commentCommand = loweraLine.Contains("\\tcp") ? "\\tcp" : "\\tcc";
                        mutatedaLine = loweraLine;
                        mutatedaLine = Regex.Replace(mutatedaLine, @"\s", ""); //remove all spaces
                        if (mutatedaLine.Contains("(" + commentCommand))
                        {
                            mutatedaLine = mutatedaLine.Substring(mutatedaLine.IndexOf("(" + commentCommand));
                        }
                        else
                        {
                            mutatedaLine = mutatedaLine.Substring(mutatedaLine.IndexOf(commentCommand));
                        }

                        if (mutatedaLine.Contains("\\tcc*[") || mutatedaLine.Contains("\\tcp*["))
                        {
                            string pattern = @"(\*\[\w*\])";
                            Match m = Regex.Match(mutatedaLine, pattern);
                            while (m.Success)
                            {
                                commentCommandsList.Add((m.Value).Trim());
                                m = m.NextMatch();
                            }
                        }

                        string condtionString = extractCondtionString(mutatedaLine, '(', ')', isEIf);
                        aLine = aLine.ToLower().Replace(condtionString, "");
                        aLine = aLine + condtionString;
                        if (commentCommandsList.Count > 0)
                        {

                            foreach (var v in commentCommandsList)
                            {
                                if (aLine.Contains(v))
                                {
                                    aLine = aLine.Replace(v, "");
                                }
                            }
                        }
                        loweraLine = aLine.ToLower();
                    }

                    if (ifInstructionSet.Count > 0)
                    {
                        mutatedaLine = loweraLine;
                        aLine = extractCondOrIf(aLine, mutatedaLine, ifInstructionSet, " \n else ", condIfSameLine);

                    }

                    if (eifConditon.Count > 0)
                    {
                        mutatedaLine = loweraLine;
                        aLine = extractCondOrIf(aLine, mutatedaLine, eifConditon, " then ", condIfSameLine);
                        if (eifConditon.Count == 0)
                        {
                            if (aLine.EndsWith("{"))
                            {
                                ifInstructionSet.Push('{');
                            }
                            else if (!String.IsNullOrEmpty(aLine.Substring(aLine.IndexOf(" then ") + (" then ").Length)))
                            {
                                condIfSameLine = true;
                                string ifCond = aLine.Substring(aLine.IndexOf(" then ") + (" then ").Length);
                                string ifInstruction = "";
                                try //tocheck
                                {
                                    ifInstruction = extractCondtionString(ifCond, '{', '}', isEIf);
                                }
                                catch (Exception ex)
                                {

                                }
                                if (!String.IsNullOrEmpty(ifInstruction))
                                {
                                    aLine = aLine.ToLower().Replace(ifInstruction, ifInstruction + " else ");
                                }
                                else
                                {
                                    Char[] ifCondArray = ifCond.ToCharArray();
                                    foreach (var v in ifCondArray)
                                    {
                                        ifInstructionSet.Push(v);
                                    }
                                }
                            }
                            else
                            {
                                ifInstructionSet.Push('0'); //dummy value to indicate the start of if instruction set
                            }
                        }
                    }

                    if (loweraLine.Contains("\\eif"))
                    {
                        mutatedaLine = loweraLine;
                        mutatedaLine = mutatedaLine.Replace("\\eif", "");
                        isEIf = true; //to indicate the eif
                        string condtionString = "";
                        char[] lineArray = mutatedaLine.ToCharArray();
                        try
                        {
                            condtionString = extractCondtionString(mutatedaLine, '{', '}', isEIf);
                            isEIf = false;
                        }
                        catch (Exception ex)
                        {

                        }
                        if (!String.IsNullOrEmpty(condtionString))
                        {
                            aLine = aLine.ToLower().Replace("\\eif", "");
                            aLine = aLine.ToLower().Replace(condtionString, " if " + condtionString + " then ");

                            if (aLine.EndsWith("{"))
                            {
                                ifInstructionSet.Push('{');
                            }
                            else if (!String.IsNullOrEmpty(aLine.Substring(aLine.IndexOf(condtionString + " then ") + (condtionString + " then ").Length)))
                            {
                                condIfSameLine = true;
                                string ifCond = aLine.Substring(aLine.IndexOf(condtionString + " then ") + (condtionString + " then ").Length);
                                string ifInstruction = "";
                                try //tocheck
                                {
                                    isEIf = true;
                                    ifInstruction = extractCondtionString(ifCond, '{', '}', isEIf);
                                    isEIf = false;
                                }
                                catch (Exception ex)
                                {

                                }
                                if (!String.IsNullOrEmpty(ifInstruction))
                                {
                                    aLine = aLine.ToLower().Replace(ifInstruction, ifInstruction + " else ");
                                }
                                else
                                {
                                    Char[] ifCondArray = ifCond.ToCharArray();
                                    foreach (var v in ifCondArray)
                                    {
                                        ifInstructionSet.Push(v);
                                    }
                                }
                            }
                            else
                            {
                                ifInstructionSet.Push('0'); //dummy value to indicate the start of if instruction set
                            }
                        }
                        else //condtion string is on next line 
                        {
                            aLine = aLine.ToLower().Replace("\\eif", " if ");
                            if (aLine.EndsWith("{"))
                            {
                                eifConditon.Push('{');
                            }
                            else
                            {
                                eifConditon.Push('0'); //dummy value to indicate the start of if instruction set
                            }
                        }
                    }

                    if (loweraLine.Contains("\\while") || loweraLine.Contains("\\if") || loweraLine.Contains("\\for") || loweraLine.Contains("\\foreach"))
                    {
                        string replaceText = "";
                        if (loweraLine.Contains("\\while") || loweraLine.Contains("\\for") || loweraLine.Contains("\\foreach"))
                        {
                            replaceText = " do ";
                        }
                        else if (loweraLine.Contains("\\if") || loweraLine.Contains("\\elseif"))
                        {
                            replaceText = " then ";
                        }

                        aLine = aLine + replaceText;
                        loweraLine = aLine.ToLower(); // update loweraLine
                    }

                    if (loweraLine.Contains("\\lif") || loweraLine.Contains("\\lforeach") || loweraLine.Contains("\\lfor")
                        || loweraLine.Contains("\\uif") || loweraLine.Contains("\\uelseif"))
                    {
                        mutatedaLine = loweraLine;
                        string replaceText = "";
                        if (loweraLine.Contains("\\lforeach") || loweraLine.Contains("\\lfor"))
                        {
                            replaceText = " do ";
                            mutatedaLine = mutatedaLine.Contains("\\lforeach") ? mutatedaLine.Replace("\\lforeach", "") : mutatedaLine.Replace("\\lfor", "");
                        }
                        else if (loweraLine.Contains("\\lif")
                            || loweraLine.Contains("\\uif") || loweraLine.Contains("\\uelseif"))
                        {
                            replaceText = " then ";
                            if (mutatedaLine.Contains("\\lif"))
                            {
                                mutatedaLine = mutatedaLine.Replace("\\lif", "");
                            }
                            else if (mutatedaLine.Contains("\\uif"))
                            {
                                mutatedaLine = mutatedaLine.Replace("\\uif", "");
                            }
                            else
                            {
                                mutatedaLine = mutatedaLine.Replace("\\uelseif", "");
                            }

                        }

                        string condtionString = extractCondtionString(mutatedaLine, '{', '}', isEIf);
                        aLine = aLine.ToLower().Replace(condtionString, condtionString + replaceText);
                        loweraLine = aLine.ToLower();
                    }

                    foreach (var v in declaredAlgoFunctions)
                    {
                        if (loweraLine.Contains(v))
                        {
                            mutatedaLine = loweraLine;
                            mutatedaLine = mutatedaLine.Substring(mutatedaLine.IndexOf(v));
                            mutatedaLine = mutatedaLine.Replace(v, "");
                            string condtionString = extractCondtionString(mutatedaLine, '{', '}', isEIf);
                            aLine = aLine.ToLower().Replace(condtionString, "(" + condtionString + ")");
                            loweraLine = aLine.ToLower();
                        }
                    }

                    foreach (var v in algorithmCommandsArray)
                    {
                        if (v.ToLower().Equals("\\while") || v.ToLower().Equals("\\for") || v.ToLower().Equals("\\foreach")
                            || v.ToLower().Equals("\\if") || v.ToLower().Equals("\\else") || v.ToLower().Equals("\\elsif") || v.ToLower().Equals("\\elseif"))
                        {
                            continue;
                        }
                        if (mathEquaionCommands.ConvertAll(d => d.ToLower()).Contains(v.ToLower()))
                        {
                            hasValue = lookupTable.TryGetValue(v, out ValueByKey);
                            if (hasValue)
                            {
                                aLine = aLine.ToLower().Replace(v.ToLower(), " " + ValueByKey + " ");
                            }
                        }
                    }

                    foreach (var v in textCommands)
                    {
                        if (mathEquaionCommands.ConvertAll(d => d.ToLower()).Contains(v.ToLower()))
                        {
                            aLine = aLine.ToLower().Replace(v.ToLower(), "");
                        }
                    }

                    foreach (var v in declaredAlgoCommandsDict)
                    {
                        if (aLine.ToLower().Contains(v.Key))
                        {
                            aLine = aLine.ToLower().Replace(v.Key, " " + v.Value + " ");
                        }
                    }
                }
            }
            return aLine;
        }

        private void populateMultipleCommands()
        {
            if (multipleCommands.Count <= 0)
            {
                multipleCommands.Add("\\begin{itemize}");
                multipleCommands.Add("\\end{itemize}");
                multipleCommands.Add("\\item");
                multipleCommands.Add("\\begin{enumerate}");
                multipleCommands.Add("\\end{enumerate}");
                multipleCommands.Add("\\begin{theorem}");
                multipleCommands.Add("\\end{theorem}");
                multipleCommands.Add("\\begin{proof}");
                multipleCommands.Add("\\end{proof}");
                multipleCommands.Add("\\begin{proposition}");
                multipleCommands.Add("\\end{proposition}");
            }
        }

        private bool checkForMultipleCommands(string aLine)
        {
            populateMultipleCommands();
            if (multipleCommands.Where(x => aLine.Contains(x)).Count() > 0)
            {
                return true;
            }
            mathEquaionCommands.Clear(); //clear previous commands 
            string tempParsedEquation = aLine;
            extractLatex(aLine);
            foreach (var v in multipleCommands)
            {
                if (mathEquaionCommands.ConvertAll(d => d.ToLower()).Contains(v.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
        private string handleMultipleCommands(string aLine)
        {
            string tag = "";
            bool hasValue = false;
            string noSpacesLine = Regex.Replace(aLine, @"\s", "");
            if (aLine.Contains("\\begin{itemize}") || aLine.Contains("\\end{itemize}"))
            {
                tag = aLine.Contains("\\begin{itemize}") ? "\\begin{itemize}" : "\\end{itemize}";
                hasValue = lookupTable.TryGetValue(tag, out ValueByKey);
                if (hasValue)
                {
                    aLine = aLine.Replace(tag, " " + ValueByKey + " ");
                }
            }
            else if (aLine.Contains("\\begin{enumerate}") || aLine.Contains("\\end{enumerate}"))
            {
                tag = aLine.Contains("\\begin{enumerate}") ? "\\begin{enumerate}" : "\\end{enumerate}";
                hasValue = lookupTable.TryGetValue(tag, out ValueByKey);
                if (hasValue)
                {
                    aLine = aLine.Replace(tag, " " + ValueByKey + " ");
                }
            }
            else if (aLine.Contains("\\begin{theorem}") || aLine.Contains("\\end{theorem}"))
            {
                tag = aLine.Contains("\\begin{theorem}") ? "\\begin{theorem}" : "\\end{theorem}";
                hasValue = lookupTable.TryGetValue(tag, out ValueByKey);
                if (hasValue)
                {
                    aLine = aLine.Replace(tag, " " + ValueByKey + " ");
                }
            }
            else if (aLine.Contains("\\begin{proof}") || aLine.Contains("\\end{proof}"))
            {
                string proofTitle = " ";
                tag = aLine.Contains("\\begin{proof}") ? "\\begin{proof}" : "\\end{proof}";
                if (tag.Equals("\\begin{proof}") && noSpacesLine.Contains("\\begin{proof}["))
                {
                    proofTitle = Regex.Match(noSpacesLine, @"(?<=\\begin{proof}\[)\w*(?!<\])").Value; //extract text between [ ]
                    aLine = noSpacesLine.Replace("[" + proofTitle + "]", "");
                }
                hasValue = lookupTable.TryGetValue(tag, out ValueByKey);
                if (hasValue)
                {
                    aLine = aLine.Replace(tag, " " + ValueByKey + " " + proofTitle + " ");
                }
            }
            else if (aLine.Contains("\\begin{proposition}") || aLine.Contains("\\end{proposition}"))
            {
                tag = aLine.Contains("\\begin{proposition}") ? "\\begin{proposition}" : "\\end{proposition}";
                hasValue = lookupTable.TryGetValue(tag, out ValueByKey);
                if (hasValue)
                {
                    aLine = aLine.Replace(tag, " " + ValueByKey + " ");
                }
            }

            if (aLine.Contains("\\item"))
            {
            Item:
                hasValue = lookupTable.TryGetValue("\\item", out ValueByKey);
                if (hasValue)
                {
                    aLine = aLine.Replace("\\item", " " + ValueByKey + ". . .");
                }
                if (aLine.Contains("\\item"))
                {
                    goto Item;
                }
            }
            return aLine;
        }

        private void populateTableCommands()
        {
            if (tableCommands.Count <= 0)
            {
                tableCommands.Add("\\begin{tabular}");
                tableCommands.Add("\\end{tabular}");

            }
        }
        private bool checkForTables(string aLine)
        {
            if (aLine.Contains("\\begin{tabular}") || aLine.Contains("\\end{tabular}") || rowCount > 0)
            {
                return true;
            }
            populateTableCommands();
            mathEquaionCommands.Clear(); //clear previous commands 
            string tempParsedEquation = aLine;
            extractLatex(aLine);
            foreach (var v in tableCommands)
            {
                if (mathEquaionCommands.ConvertAll(d => d.ToLower()).Contains(v.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }

        public static string ReplaceFirstOccurrence(string aLine, string Find, string Replace)
        {
            int Place = aLine.IndexOf(Find);
            string result = aLine.Remove(Place, Find.Length).Insert(Place, Replace);
            return result;
        }

        private static string handleColumnSpans(string aLine, ref int columnCount)
        {
            string spanCommand = "";
            int span = 0;
            string tempALine = aLine;
            string tempALinesbstring = tempALine;
            string spanTag = aLine.Contains("multicolumn") ? "multicolumn" : "multirow";
            string spanText = aLine.Contains("multicolumn") ? " " : "row";
            Match m;
            string pattern = @"\\" + (spanTag + "({[^}]*})({[^}]*})({[^}]*})");
            m = Regex.Match(aLine, pattern);

            if (aLine.IndexOf(m.Value) > 0)
            {
                tempALinesbstring = aLine.Substring(0, aLine.IndexOf(m.Value));
                while (tempALinesbstring.Contains("&"))
                {
                    columnCount = columnCount + 1;
                    aLine = ReplaceFirstOccurrence(aLine, "&", ". . .Column " + columnCount + ". . .");
                    tempALinesbstring = ReplaceFirstOccurrence(tempALinesbstring, "&", ""); //to end loop
                }
            }

            spanCommand = m.Value;
            string tempSpanCommand = spanCommand;
            tempSpanCommand = tempSpanCommand.Replace("\\" + spanTag, " " + spanText + " span to ");
            //extract span
            span = Convert.ToInt32(tempSpanCommand.Substring(tempSpanCommand.IndexOf("{") + 1, tempSpanCommand.IndexOf("}") - tempSpanCommand.IndexOf("{") - 1));
            int i = 0;
            i = spanTag == "multicolumn" ? span - 1 : 0;
            columnCount = columnCount + i;

            //replace span { }
            tempSpanCommand = ReplaceFirstOccurrence(tempSpanCommand, "{", "");
            tempSpanCommand = ReplaceFirstOccurrence(tempSpanCommand, "}", ". . .");
            //replace width with ""
            string width = tempSpanCommand.Substring(tempSpanCommand.IndexOf("{"), tempSpanCommand.IndexOf("}") - tempSpanCommand.IndexOf("{") + 1);
            tempSpanCommand = tempSpanCommand.Replace(width, " ");
            aLine = aLine.Replace(spanCommand, tempSpanCommand);

            return aLine;
        }

        private string handleRowSpan(string aLine, List<string> rowSpanList, List<string> columnAfterRowSpan)
        {
            List<string> tempRowSpanList = new List<string>(rowSpanList); ;
            List<string> tempColumnAfterRowSpan = new List<string>(columnAfterRowSpan);

            foreach (var column in tempColumnAfterRowSpan)
            {
                foreach (var rowspan in tempRowSpanList)
                {
                    if (rowspan.Contains(column))
                    {
                        string modifiedRowSPan = " " + rowspan.Replace(column, "") + " ";
                        if (column == "Column 0") //append rowspan in case of last column
                        {
                            aLine = aLine + modifiedRowSPan;
                        }
                        else
                        {
                            aLine = aLine.Insert(aLine.IndexOf(column), modifiedRowSPan);
                        }
                        string tempRowSpan = rowspan.Replace("row span to ", "");
                        tempRowSpan = tempRowSpan.Replace(column, "");
                        int rowSpanNumber = Convert.ToInt32(Regex.Match(tempRowSpan, @"(\d+)").Value);
                        rowSpanNumber = rowSpanNumber - 1;

                        if (rowSpanNumber <= 1) //spans only 1 row
                        {
                            rowSpanList.Remove(rowspan);
                            columnAfterRowSpan.Remove(column);
                        }
                        else //spans more than 1 row
                        {
                            if (countOfRowSpan.ContainsKey(rowspan))
                            {
                                int span = 0;
                                bool spanValue = countOfRowSpan.TryGetValue(rowspan, out span);
                                countOfRowSpan[rowspan] = span - 1;
                            }
                            else
                            {
                                countOfRowSpan.Add(rowspan, rowSpanNumber - 1);
                            }
                        }

                        if (countOfRowSpan.ContainsKey(rowspan)) //keep count of row span 
                        {
                            int span = 0;
                            bool spanValue = countOfRowSpan.TryGetValue(rowspan, out span);
                            if (spanValue && span == 0)
                            {
                                rowSpanList.Remove(rowspan); //remove incorparated spans
                                columnAfterRowSpan.Remove(column);
                            }
                        }
                    }
                }
            }
            return aLine;
        }

        private void extractRowSpans(string aLine, ref List<string> rowSpanList, ref List<string> columnAfterRowSpan)
        {
            string copiedAline = aLine;
            string rowSpanPattern = @"row span to(.*?)Column (\d+)";
            Match m;
            m = Regex.Match(copiedAline, rowSpanPattern);

            while (m.Success) //gather all row spans along with next Column
            {
                string negRowSpan = m.Value;
                copiedAline = copiedAline.Replace(m.Value, "");
                if (!negRowSpan.Contains("row span to -"))
                {
                    negRowSpan = negRowSpan.Insert(negRowSpan.IndexOf("to ") + 3, "-");
                    rowSpanList.Add(negRowSpan);
                }
                m = m.NextMatch();
            }

            string colPattern = @"Column (\d+)";
            foreach (var rowspan in rowSpanList)
            {
                m = Regex.Match(rowspan, colPattern);

                while (m.Success)
                {
                    if (!columnAfterRowSpan.Contains(m.Value))
                    {
                        columnAfterRowSpan.Add(m.Value);
                    }
                    m = m.NextMatch();
                }
            }

            if (copiedAline.Contains("row span"))//last column has row span
            {
                string lastColRowSpan = copiedAline.Substring(copiedAline.IndexOf("row span")) + "Column 0";
                if (!lastColRowSpan.Contains("row span to -"))
                {
                    lastColRowSpan = lastColRowSpan.Insert(lastColRowSpan.IndexOf("to ") + 3, "-");
                    rowSpanList.Add(lastColRowSpan);
                    columnAfterRowSpan.Add("Column 0"); //column 0 indicates last column of the row
                }
            }
        }

        private string handleTables(string aLine)
        {
            string trimmedAline = Regex.Replace(aLine, @"\s+", "");
            List<int> spanArray = new List<int>();

            int columnCount = 1;
            string tag = "";
            if (aLine.Contains("\\begin{tabular}") || aLine.Contains("\\end{tabular}"))
            {
                tag = aLine.Contains("\\begin{tabular}") ? "\\begin{tabular}" : "\\end{tabular}";

                if (tag == "\\begin{tabular}")
                {
                    aLine = "start of table";
                    rowCount = rowCount + 1;
                }
                else
                {
                    aLine = "end of table";
                    rowCount = 0;
                }
            }
            else
            {
                if (trimmedAline.Equals("\\hline") || String.IsNullOrEmpty(trimmedAline))
                {
                    return aLine;
                }

                if (aLine.Contains("\\cline"))
                {
                    string pattern = @"\\cline({([^()]|())*]*})";
                    Match m = Regex.Match(aLine, pattern);
                    while (m.Success)
                    {
                        aLine = aLine.Replace(m.Value, "");
                        m = m.NextMatch();
                    }
                }
                if (!trimmedAline.Equals("\\hline"))
                {
                    string rowNumber = " Row " + rowCount;
                    aLine = rowNumber + " Column " + columnCount + ". . ." + aLine;
                }
                while (aLine.Contains("\\multirow") || aLine.Contains("\\multicolumn"))
                {
                    aLine = handleColumnSpans(aLine, ref columnCount);
                    columnCount = columnCount + 1;
                    if (aLine.Contains("&"))
                    {
                        aLine = ReplaceFirstOccurrence(aLine, "&", ". . .Column " + columnCount + ". . .");
                    }
                }

                Char[] aLineArray = aLine.ToCharArray();

                foreach (var v in aLineArray)
                {

                    if (v.Equals('&'))
                    {
                        columnCount = columnCount + 1;
                        aLine = ReplaceFirstOccurrence(aLine, "&", ". . .Column " + columnCount + ". . .");
                    }
                }

                if (!trimmedAline.Equals("\\hline") || !String.IsNullOrEmpty(trimmedAline))
                {
                    rowCount = rowCount + 1;
                }
            }
            aLine = aLine + (". . .");

            aLine = handleRowSpan(aLine, rowSpanList, columnAfterRowSpan);

            if (aLine.Contains("row span"))
            {
                extractRowSpans(aLine, ref rowSpanList, ref columnAfterRowSpan);
            }
            return aLine;
        }

        private string handleFigures(string aLine)
        {
            List<string> latexCommands = new List<string>();
            bool hasValue = false;
            if (aLine.Contains("\\begin{figure}") || aLine.Contains("\\end{figure}"))
            {
                string cmnd = aLine.Contains("\\begin{figure}") ? "\\begin{figure}" : "\\end{figure}";
                hasValue = lookupTable.TryGetValue(cmnd, out ValueByKey);
                if (hasValue)
                {
                    aLine = aLine.Replace(cmnd, " " + ValueByKey + " ");
                }
            }
            string pattern = @"^(\s\\|\\)([a-zA-Z]*)(\s|[a-zA-Z])\b|(\s\\|\\)([a-zA-Z]*)(\s|[a-zA-Z])"; //to extract latex command
            Match m = Regex.Match(aLine, pattern);
            while (m.Success)
            {
                latexCommands.Add((m.Value).Trim());
                m = m.NextMatch();
            }

            foreach (var v in latexCommands)
            {
                foreach (var figureCommad in figureCommands)
                {
                    if (v == figureCommad)
                    {
                        hasValue = lookupTable.TryGetValue(v, out ValueByKey);
                        if (hasValue)
                        {
                            aLine = aLine.Replace(v, " " + ValueByKey + " ");
                        }
                    }
                }
            }
            return aLine;
        }

        private void miscellenousChecks(ref string aLine, ref string parsedCode, string trimmedAline)
        {
            if (aLine != null && !trimmedAline.StartsWith("%"))
            {
                aLine = handleAlgorithms(aLine);
            }

            if (aLine != null && !trimmedAline.StartsWith("%") && checkForMultipleCommands(aLine))
            {
                aLine = handleMultipleCommands(aLine);
            }
            // handle tables 
            //if (aLine != null && !trimmedAline.StartsWith("%") && checkForTables(aLine))
            //{
            //    aLine = handleTables(aLine);
            //}

            if (aLine != null && !trimmedAline.StartsWith("%") && checkForFigures(aLine))
            {
                aLine = handleFigures(aLine);
            }
        }

        private void populateFigureCommands()
        {
            if (figureCommands.Count <= 0)
            {
                figureCommands.Add("\\begin{figure}");
                figureCommands.Add("\\end{figure}");
                figureCommands.Add("\\alt");
            }
        }

        private bool checkForFigures(string aLine)
        {
            populateFigureCommands();
            foreach (var v in figureCommands)
            {
                if (aLine.ToLower().Contains(v.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }

        private void handlePowerSub(ref string mathEquation)
        {
            Stack<char> powerStack = new Stack<char>();
            Stack<char> origExpStack = new Stack<char>();
            mathEquation = Regex.Replace(mathEquation, @"(?<=^|_)\s*(?<!{|[^ ])", ""); //replace spaces between ^/_ and { 
            Char[] mathCharArray = mathEquation.ToCharArray();
            int i = 0;
            char c = mathCharArray[i];
            string appendText = "";
            while (i < mathCharArray.Count())
            {
                if ((c == '^' || c == '_'))
                {
                    if (mathCharArray[i + 1] == '{')
                    {
                        origExpStack.Push(mathCharArray[i]);
                        appendText = c.Equals('^') ? ". . .end power. . ." : " . . .end sub. . . ";
                    StackLogic:
                        i = i + 1;
                        while (mathCharArray[i] != '}')
                        {
                            powerStack.Push(mathCharArray[i]);
                            origExpStack.Push(mathCharArray[i]);
                            i += 1;
                        }
                        powerStack.Push(mathCharArray[i]);//push '}'
                        origExpStack.Push(mathCharArray[i]);//push '}'          
                        while (powerStack.Peek() != '{') //pop embeded brackets
                        {
                            powerStack.Pop();
                        }
                        powerStack.Pop(); //pop '{'
                        if (powerStack.Count > 0)
                            goto StackLogic;
                        //pop stack until empty
                        string expression = "";
                        while (origExpStack.Count != 0)
                        {
                            expression += origExpStack.Pop().ToString();
                        }
                        expression = new String(expression.ToCharArray().Reverse().ToArray());

                        if (!mathEquation.Contains(expression + appendText))
                        {
                            mathEquation = mathEquation.Replace(expression, expression + appendText);
                            mathCharArray = mathEquation.ToCharArray(); //update mathCharArray 
                        }
                    }
                    else
                    {
                        i = i + 1;
                        appendText = c.Equals('^') ? ". . .end power. . ." : " . . .end sub. . . ";
                        string replaceExpression = c.Equals('^') ? "^" : "_";
                        if (mathCharArray[i] == '\\')//in case of no brackets and a latex command as super or sub script
                        {
                            string latexCommand = mathCharArray[i].ToString(); ;
                            while (!mathCharArray[i + 1].Equals(' ') && Regex.Match(mathCharArray[i + 1].ToString(), @"[a-zA-Z\s]").Success)
                            {
                                i++;
                                latexCommand += mathCharArray[i].ToString();
                            }
                            replaceExpression = replaceExpression + latexCommand;
                        }
                        else
                        {
                            replaceExpression = replaceExpression + mathCharArray[i];
                        }
                        if (!mathEquation.Contains(replaceExpression + appendText))
                            mathEquation = mathEquation.Replace(replaceExpression, replaceExpression + appendText);
                        mathCharArray = mathEquation.ToCharArray(); //update mathCharArray
                    }
                }
                i = i + 1;
                if (i < mathCharArray.Count())
                    c = mathCharArray[i];
            }
        }
        public static bool IsNumeric(object Expression)
        {
            double retNum;
            bool isNum = Double.TryParse(Convert.ToString(Expression), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }

        private void populateDiffDegree()
        {
            if (diffDegrees.Count <= 0)
            {
                diffDegrees.Add(1, "first");
                diffDegrees.Add(2, "second");
                diffDegrees.Add(3, "third");
            }
        }

        private string stackLogicforDiff_Binom(string mathEquation, string command)
        {
            string expression = "";
            int endIndex = mathEquation.IndexOf(command) + command.Length - 1;
            int startIndex = mathEquation.IndexOf(command);
            Stack<char> origExpression = new Stack<char>();
            Stack<char> commandStack = new Stack<char>();
            Char[] chararray = mathEquation.ToCharArray();
            int i = endIndex;
            while (chararray[startIndex] != '{')
            {
                origExpression.Push(chararray[startIndex]);
                startIndex++;
            }
            while (i < chararray.Length)
            {
            StackLogic:
                if (chararray[i + 1] != '}')
                {
                    if (chararray[i + 1] == '{')
                    {
                        i++;
                        commandStack.Push(chararray[i]);
                        origExpression.Push(chararray[i]);
                        while (chararray[i + 1] != '}')
                        {
                            i++;
                            commandStack.Push(chararray[i]);
                            origExpression.Push(chararray[i]);
                        }
                    }
                    else { i++; }
                }
                else //if (chararray[i + 1] == '{')
                {
                    i++;
                    commandStack.Push(chararray[i]);
                    origExpression.Push(chararray[i]);
                    while (commandStack.Peek() != '{')
                    {
                        commandStack.Pop();
                    }
                    commandStack.Pop();
                    if (commandStack.Count > 0)
                        goto StackLogic;
                    while (origExpression.Count != 0)
                    {
                        expression += origExpression.Pop().ToString();
                    }
                    expression = new String(expression.ToCharArray().Reverse().ToArray());
                    break;
                }
                //else
                //{
                //    i++;
                //}
            }

            return expression;
        }

        private string handleDerivatives(string mathEquation)
        {
            mathEquaionCommands.Clear();
            populateDiffDegree();
            string pattern = "";
            string command = "";
            string replaceText = "";
            string tempMathEquation = mathEquation;
            if (mathEquation.Contains("\\dv"))
            {
                command = "\\dv";
                replaceText = "derivative";
                pattern = @"(\\dv|\s\\dv)([^}]*)}";
            }
            else if (mathEquation.Contains("\\pdv"))
            {
                command = "\\pdv";
                replaceText = "partial derivative";
                pattern = @"(\\pdv|\s\\pdv)([^}]*)}";
            }
            else if (mathEquation.Contains("\\fdv"))
            {
                command = "\\fdv";
                replaceText = "functional derivative";
                pattern = @"(\\fdv|\s\\fdv)([^}]*)}";
            }
            Match m = Regex.Match(mathEquation, pattern);
            //while (m.Success)
            //{
            //    mathEquaionCommands.Add((m.Value).Trim());
            //    m = m.NextMatch();
            //}

            //while (m.Success)
            //{
            //    mathEquaionCommands.Add(stackLogicforDiff_Binom(mathEquation, command));
            //    m = m.NextMatch();
            //}
            while (tempMathEquation.Contains(command))
            {
                mathEquaionCommands.Add(stackLogicforDiff_Binom(tempMathEquation, command));
                tempMathEquation = ReplaceFirstOccurrence(tempMathEquation, command, "");
            }

            string powerString = null; //differential degree
            foreach (var v in mathEquaionCommands)
            {
                if (mathEquation.Contains(v + "{"))
                {
                    mathEquation = mathEquation.Replace(v, v + " with respect to ");
                }
            }
            string lastPowerString = "";
            foreach (var v in mathEquaionCommands)
            {
                if (v.Contains("["))
                {
                    string pat = v.Split('[', ']')[1];
                    powerString = pat;
                }
                if (powerString != null)
                {
                    if (!lastPowerString.Equals(powerString))
                    {
                        string replacePowerString = "[" + powerString + "]";
                        if (IsNumeric(powerString))
                        {
                            bool hasValue;
                            hasValue = diffDegrees.TryGetValue(Convert.ToInt32(powerString), out ValueByKey);
                            if (hasValue)
                            {
                                mathEquation = mathEquation.Replace(replacePowerString, " " + ValueByKey + " degree " + replaceText);
                            }
                            else
                            {
                                mathEquation = mathEquation.Replace(replacePowerString, " " + powerString + "th " + " degree " + replaceText);
                            }
                        }
                        else
                        {
                            mathEquation = mathEquation.Replace(replacePowerString, " " + powerString + " degree " + replaceText);
                        }
                    }
                }
                else
                {
                    mathEquation = mathEquation.Replace(command, " " + replaceText + " ");

                }
                lastPowerString = powerString;
            }
            mathEquation = mathEquation.Replace(command, "");
            if (mathEquation.Contains("\\dv") || mathEquation.Contains("\\pdv") || mathEquation.Contains("\\fdv"))
            {
                mathEquation = handleDerivatives(mathEquation);
            }

            return mathEquation;
        }

        private void populateSetCommands()
        {
            if (setCommands.Count <= 0)
            {
                setCommands.Add("\\emptyset");
                setCommands.Add("\\o");
                setCommands.Add("\varnothing");
                setCommands.Add("\\sqsubset");
                setCommands.Add("\\sqsubseteq");
                setCommands.Add("\\sqsupset");
                setCommands.Add("\\sqsupseteq");
                setCommands.Add("\\subset");
                setCommands.Add("\\not\\subset");
                setCommands.Add("\\subseteq");
                setCommands.Add("\\nsubseteq");
                setCommands.Add("\\supset");
                setCommands.Add("\\not\\supset");
                setCommands.Add("\\supseteq");
                setCommands.Add("\\nsupseteq");
                setCommands.Add("\\forall");
                setCommands.Add("\\in");
                setCommands.Add("\\notin");
                setCommands.Add("\\ni");
                setCommands.Add("\\cap");
                setCommands.Add("\\bigcap");
                setCommands.Add("\\cup");
                setCommands.Add("\\bigcup");
                setCommands.Add("\\setminus");
                setCommands.Add("\\sqcap");
                setCommands.Add("\\sqcup");
                setCommands.Add("\\overline");
                setCommands.Add("\\mid");
                setCommands.Add("\\times");
                setCommands.Add("\\exists");
                setCommands.Add("\\nexists");
                setCommands.Add("\\mathcal{P}(S)");
                setCommands.Add("\\mathbf{card}");
                setCommands.Add("\\mathbb");
                setCommands.Add("\\quad");
                setCommands.Add("\\text");
                setCommands.Add("\\to");
                setCommands.Add("\\alpha");
                setCommands.Add("\\Alpha");
                setCommands.Add("\\beta");
                setCommands.Add("\\Beta");
                setCommands.Add("\\gamma");
                setCommands.Add("\\Gamma");
                setCommands.Add("\\pi");
                setCommands.Add("\\Pi");
                setCommands.Add("\\phi");
                setCommands.Add("\\varphi");
                setCommands.Add("\\Phi");
                setCommands.Add("\\theta");
                setCommands.Add("\\Theta");
                setCommands.Add("\\partial");
                setCommands.Add("\\textrm");
                setCommands.Add("\\equiv");
                setCommands.Add("\\neg");
                setCommands.Add("\\land");
                setCommands.Add("\\lor");
                setCommands.Add("\\rightarrow");
                setCommands.Add("\\Rightarrow");
                setCommands.Add("\\Leftarrow");
                setCommands.Add("\\leftarrow");
                setCommands.Add("\\gets");
                setCommands.Add("\\mapsto");
                setCommands.Add("\\implies");
                setCommands.Add("\\leftrightarrow");
                setCommands.Add("\\iff");
                setCommands.Add("\\Leftrightarrow");
                setCommands.Add("\\top");
                setCommands.Add("\\bot");
                setCommands.Add("\\overrightarrow");
                setCommands.Add("\\bmod");
                setCommands.Add("\\not");
                setCommands.Add("\\textbf");
                setCommands.Add("\\dots");
                setCommands.Add("\\ldots");
                setCommands.Add("\\cdots");
                setCommands.Add("\\vdots");
                setCommands.Add("\\ddots");
                setCommands.Add("\\mbox");
                setCommands.Add("\\left");
                setCommands.Add("\\right");
                setCommands.Add("\\wedge");
                setCommands.Add("\\vee");
                setCommands.Add("\\kappa");
                setCommands.Add("\\exp");
                setCommands.Add("\\log");
                setCommands.Add("\\max");
                setCommands.Add("\\min");
                setCommands.Add("\\tilde");
                setCommands.Add("\\dot");
                setCommands.Add("\\vec");
                setCommands.Add("\\cong");
                setCommands.Add("\\cdot");
                setCommands.Add("\\ln");
                setCommands.Add("\\sim");
                setCommands.Add("\\simeq");
                setCommands.Add("\\circ");
                setCommands.Add("\\propto");
                setCommands.Add("\\doteq");
                setCommands.Add("\\approx");
                setCommands.Add("\\parallel");
                setCommands.Add("\\smile");
                setCommands.Add("\\perp");
                setCommands.Add("\\nparallel");
                setCommands.Add("\\sphericalangle");
                setCommands.Add("\\frown");
                setCommands.Add("\\measuredangle");
                setCommands.Add("\\pm");
                setCommands.Add("\\mp");
                setCommands.Add("\\div");
                setCommands.Add("\\ast");
                setCommands.Add("\\star");
                setCommands.Add("\\dagger");
                setCommands.Add("\\ddagger");
                setCommands.Add("\\uplus");
                setCommands.Add("\\diamond");
                setCommands.Add("\\bigtriangleup");
                setCommands.Add("\\bigtriangledown");
                setCommands.Add("\\triangleleft");
                setCommands.Add("\\trigangleright");
                setCommands.Add("\\bigcirc");
                setCommands.Add("\\bullet");
                setCommands.Add("\\oplus");
                setCommands.Add("\\ominus");
                setCommands.Add("\\otimes");
                setCommands.Add("\\amalg");
                setCommands.Add("\\uparrow");
                setCommands.Add("\\downarrow");
                setCommands.Add("\\langle");
                setCommands.Add("\\lceil");
                setCommands.Add("\\lfloor");
                setCommands.Add("\\backslash");
                setCommands.Add("\\rangle");
                setCommands.Add("\\rceil");
                setCommands.Add("\\rfloor");
                setCommands.Add("\\latex");
                setCommands.Add("\\prec");
                setCommands.Add("\\preceq");
                setCommands.Add("\\succ");
                setCommands.Add("\\succeq");
                setCommands.Add("\\longleftarrow");
                setCommands.Add("\\Longleftarrow");
                setCommands.Add("\\longrightarrow");
                setCommands.Add("\\Longrightarrow");
            }
        }

        private bool checkForSets(string mathEquation)
        {
            populateSetCommands();
            mathEquaionCommands.Clear(); //clear previous commands 
            string tempParsedEquation = mathEquation;
            extractLatex(mathEquation);
            foreach (var v in declaredCommands)
            {
                if (mathEquaionCommands.Contains(v.Key))
                {
                    return true;
                }
            }
            foreach (var v in setCommands)
            {
                if (mathEquaionCommands.Contains(v))
                {
                    return true;
                }
            }
            return false;
        }

        private string handleSets(String mathEquation)
        {
            if (mathEquaionCommands.Contains("\\mathbf") && mathEquation.ToLower().Contains("\\mathbf{card}"))
            {
                mathEquation = mathEquation.Replace("\\mathbf{card}", "set cardinality");
            }
            bool hasValue = false;
            string tempMathEquation = mathEquation;
            foreach (var v in declaredCommands)
            {
                if (mathEquaionCommands.Contains(v.Key))
                {
                    tempMathEquation = tempMathEquation.Replace(v.Key, " " + v.Value + " ");
                }
            }
            foreach (var v in setCommands)
            {
                if (mathEquaionCommands.Contains(v))
                {
                    hasValue = lookupTable.TryGetValue(v, out ValueByKey);
                    if (hasValue)
                    {
                        tempMathEquation = tempMathEquation.Replace(v, " " + ValueByKey + " "); //replace latex command with readable text
                        if (v == "\\textrm" || v == "\\textbf")
                        {
                            if (tempMathEquation.Contains("rm"))
                            {
                                tempMathEquation = tempMathEquation.Replace("rm", "");
                            }
                            if (tempMathEquation.Contains("bf"))
                            {
                                tempMathEquation = tempMathEquation.Replace("bf", "");
                            }
                        }
                    }
                    if (v == "\\mathbb")
                    {
                        tempMathEquation = tempMathEquation.Replace("\\mathbb", "latextext");
                    }
                }
            }
            return tempMathEquation;
        }

        private string handleDifferentials(string mathEquation)
        {
            populateDiffDegree();
            string command = "\\dd";
            string replaceText = "differential";
            var trimMathEquation = mathEquation;
            trimMathEquation = Regex.Replace(trimMathEquation, @"\s+", "");
            if (trimMathEquation.Contains("\\dd["))
            {
                int pFrom = trimMathEquation.IndexOf("\\dd[") + "\\d[".Length;
                int pTo = trimMathEquation.IndexOf("]") + 1;

                String result = trimMathEquation.Substring(pFrom, pTo - pFrom);
                String trimmedResult = result.Replace("[", "");
                trimmedResult = trimmedResult.Replace("]", "");

                if (!String.IsNullOrEmpty(result))
                {
                    if (IsNumeric(trimmedResult))
                    {
                        bool hasValue;
                        hasValue = diffDegrees.TryGetValue(Convert.ToInt32(trimmedResult), out ValueByKey);
                        if (hasValue)
                        {
                            mathEquation = mathEquation.Replace(result, "");
                            mathEquation = mathEquation.Replace(command, " " + ValueByKey + " degree " + replaceText);
                        }
                        else
                        {
                            mathEquation = mathEquation.Replace(result, "");
                            mathEquation = mathEquation.Replace(command, " " + result + " degree " + replaceText);
                        }
                    }
                    else
                    {
                        mathEquation = mathEquation.Replace(result, "");
                        mathEquation = mathEquation.Replace(command, " " + result + " degree " + replaceText);
                    }
                }
                else
                {
                    mathEquation = mathEquation.Replace(command, replaceText);
                }
            }
            else
            {
                mathEquation = mathEquation.Replace(command, replaceText);
            }
            return mathEquation;
        }

        private void populateCombinatoricsCommands()
        {
            if (combinatoricsCommands.Count <= 0)
            {
                combinatoricsCommands.Add("\\choose");
                combinatoricsCommands.Add("\\binom");
                combinatoricsCommands.Add("\\brack");
                combinatoricsCommands.Add("\\brace");
            }
        }

        private bool checkForCombinatorics(string mathEquation)
        {
            populateCombinatoricsCommands();
            mathEquaionCommands.Clear(); //clear previous commands 
            string tempParsedEquation = mathEquation;
            extractLatex(mathEquation);
            foreach (var v in combinatoricsCommands)
            {
                if (mathEquaionCommands.ConvertAll(d => d.ToLower()).Contains(v.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }
        private string handleCombinatorics(string mathEquation)
        {
            mathEquaionCommands.Clear();
            Stack<char> row1Stack = new Stack<char>();
            Stack<char> row2Stack = new Stack<char>();
            Stack<char> origExpStack = new Stack<char>();
            Char[] charArray = mathEquation.ToCharArray();
            string row1Exp = "";
            string row2Exp = "";

            string openingBracket = "";
            string closingBracket = "";

            string command = "";
            int startIndex = 0;
            int endIndex = 0;
            string tempMathEquation = mathEquation;
            if (mathEquation.Contains("\\binom"))
            {
                mathEquaionCommands.Add(stackLogicforDiff_Binom(tempMathEquation, "\\binom"));
                tempMathEquation = ReplaceFirstOccurrence(mathEquation, "\\binom", "");
            }


            foreach (var v in mathEquaionCommands)
            {
                mathEquation = mathEquation.Replace(v + " ", v);
                if (mathEquation.Contains(v + "{"))
                {
                    mathEquation = mathEquation.Replace(v + "{", v.Remove(v.Length - 1) + " \\choose ");
                    mathEquation = ReplaceFirstOccurrence(mathEquation, "\\binom", "");
                }
            }

            charArray = mathEquation.ToCharArray();
            if (mathEquation.Contains("\\choose"))
            {
                command = "\\choose";
                openingBracket = "(";
                closingBracket = ")";
            }
            else if (mathEquation.Contains("\\brack"))
            {
                command = "\\brack";
                openingBracket = "[";
                closingBracket = "]";

            }
            else if (mathEquation.Contains("\\brace"))
            {
                command = "\\brace";
                openingBracket = "opening curly bracket";
                closingBracket = "closing curly bracket";

            }

            startIndex = mathEquation.IndexOf(command);
            endIndex = startIndex + command.Length - 1;

            int i = startIndex;
            int j = endIndex;

            row1Stack.Push('}'); //push dummy opening and closing bracket
            row2Stack.Push('{');
            while (i > 0)
            {
            StackLogic:
                if (charArray[i - 1] != '{')
                {
                    i--;
                    row1Stack.Push(charArray[i]);
                    origExpStack.Push(charArray[i]);
                }
                else
                {
                    row1Stack.Push('{');
                    i--;
                    origExpStack.Push(charArray[i]);

                    while (row1Stack.Peek() != '}')
                    {
                        row1Stack.Pop();
                    }
                    row1Stack.Pop();
                    if (row1Stack.Count > 0)
                        goto StackLogic;
                    //pop stack until empty
                    while (origExpStack.Count != 0)
                    {
                        row1Exp += origExpStack.Pop().ToString();
                    }
                    //row1Exp = new String(row1Exp.ToCharArray().Reverse().ToArray());
                    break;
                }
            }

            while (j < charArray.Count())
            {
            StackLogic:
                if (charArray[j + 1] != '}')
                {
                    j++;
                    row2Stack.Push(charArray[j]);
                    origExpStack.Push(charArray[j]);
                }
                else
                {
                    row2Stack.Push('}');
                    j++;
                    origExpStack.Push(charArray[j]);

                    while (row2Stack.Peek() != '{')
                    {
                        row2Stack.Pop();
                    }
                    row2Stack.Pop();
                    if (row2Stack.Count > 0)
                        goto StackLogic;
                    //pop stack until empty
                    while (origExpStack.Count != 0)
                    {
                        row2Exp += origExpStack.Pop().ToString();
                    }
                    row2Exp = new String(row2Exp.ToCharArray().Reverse().ToArray());
                    break;
                }
            }

            mathEquation = mathEquation.Replace(row1Exp, openingBracket + row1Exp);
            mathEquation = mathEquation.Replace(row2Exp, row2Exp + closingBracket);
            mathEquation = ReplaceFirstOccurrence(mathEquation, command, "asteriklinebreakasterik");

            if (mathEquation.Contains("\\choose") || mathEquation.Contains("\\brace") || mathEquation.Contains("\\brack"))
            {
                mathEquation = handleCombinatorics(mathEquation);
            }
            return mathEquation;
        }

        private void populateMatricesCommands()
        {
            if (matricesCommands.Count <= 0)
            {
                matricesCommands.Add("\\begin{matrix}");
                matricesCommands.Add("\\end{matrix}");
                matricesCommands.Add("\\begin{smallmatrix}");
                matricesCommands.Add("\\end{smallmatrix}");
                matricesCommands.Add("\\begin{pmatrix}");
                matricesCommands.Add("\\end{pmatrix}");
                matricesCommands.Add("\\begin{bmatrix}");
                matricesCommands.Add("\\end{bmatrix}");
                matricesCommands.Add("\\begin{vmatrix}");
                matricesCommands.Add("\\end{vmatrix}");
                matricesCommands.Add("\\begin{array}");
                matricesCommands.Add("\\end{array}");
                matricesCommands.Add("\\begin{cases}");
                matricesCommands.Add("\\end{cases}");
                matricesCommands.Add("\\bordermatrix");
            }
        }

        private bool checkForMatrices(string aLine)
        {
            populateMatricesCommands();
            if (matrixRowCount > 0 || isBorderMatrix || isBorderMatrixEndLine)
            {
                return true;
            }
            foreach (var v in matricesCommands)
            {
                if (aLine.Replace("*", "").ToLower().Contains(v.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }

        private string handleMatrices(string aLine)
        {
            Match m;
            string pattern = @"\\begin{.*?}|\\end{.*?}";
            int columnCount = 0;
            int prevColumnCount = matrixColumnCount;
            matrixColumnCount = columnCount;
            string tempALine = aLine;
            string cmdDsc = "";
            string mValue = "";
            char[] borderMatrixArray;
            if (aLine.Contains("begin") || aLine.Contains("end"))
            {
                m = Regex.Match(aLine, pattern);
                mValue = m.Value;
                mValue = mValue.Replace("*", "");//* used for alignment purposes
                bool hasValue = false;
                while (m.Success)
                {
                    mValue = m.Value;
                    mValue = mValue.Replace("*", "");//* used for alignment purposes
                    hasValue = lookupTable.TryGetValue(mValue, out ValueByKey);
                    if (hasValue)
                    {
                        cmdDsc = ValueByKey;
                        aLine = aLine.Replace(m.Value, " " + ValueByKey + " . . .");
                        tempALine = tempALine.Replace(m.Value, "");
                    }

                    if (aLine.Contains("start") && !m.Value.Contains("end")) //insert size right after the start line
                    {
                        aLine = aLine.Insert(aLine.IndexOf(cmdDsc) + cmdDsc.Length, " . . . matrix size . . .");
                    }

                    m = m.NextMatch();
                }

                if (aLine.Contains("start")) //increase row size by 1 to indicate start of matrix
                {
                    matrixRowCount = 1;
                }
            }
            if (aLine.Contains("start of array") && tempALine.Contains("{"))
            {
                if (aLine.Contains("\\{"))
                {
                    aLine = aLine.Replace("\\{", "leftBracket");
                }
                if (tempALine.Contains("}"))
                {
                    tempALine = tempALine.Replace(tempALine.Substring(tempALine.IndexOf('{'), tempALine.IndexOf('}') + 1 - tempALine.IndexOf('{')), "");
                    aLine = aLine.Replace(aLine.Substring(aLine.IndexOf('{'), aLine.IndexOf('}') + 1 - aLine.IndexOf('{')), "");
                }
                if (aLine.Contains("leftBracket"))
                {
                    aLine = aLine.Replace("leftBracket", "\\{");
                }
            }

            if (aLine.Contains("\\bordermatrix"))
            {
                isBorderMatrix = true;
                aLine = aLine.Replace("\\bordermatrix", "start of border matrix. . .horizontal border. . .");
            }

            if (isBorderMatrix)
            {
                string charLine = "";
                if (tempALine.Contains("\\bordermatrix"))
                {
                    charLine = (tempALine.Substring(tempALine.IndexOf("\\bordermatrix") + ("\\bordermatrix").Length));
                    tempALine = tempALine.Replace("\\bordermatrix", "");
                }
                else
                {
                    charLine = tempALine;
                }

                borderMatrixArray = charLine.ToArray();
                foreach (char c in borderMatrixArray)
                {
                    if (c != '}')
                    {
                        borderMatrixStack.Push(c);
                    }
                    else
                    {
                        while (borderMatrixStack.Peek() != '{')
                        {
                            borderMatrixStack.Pop();
                        }
                        borderMatrixStack.Pop();
                    }
                }

                if (borderMatrixStack.Count == 0)
                {
                    isBorderMatrix = false;
                    isBorderMatrixEndLine = true;
                    aLine = aLine.Remove(aLine.LastIndexOf("}"), 1);
                    aLine = aLine + " parentheses end of border matrix";
                }
            }

            if (tempALine.Contains('[') && tempALine.Contains(']'))
            {
                string alignment = tempALine.Substring(tempALine.IndexOf('['), tempALine.IndexOf(']') + 1 - tempALine.IndexOf('['));
                if (Regex.Match(alignment, @"\b(?!(?:.\B)*(.)(?:\B.)*\1)[lcr]+\b").Success) //[] contains any combination of lcr(used for alignment)
                {
                    tempALine = tempALine.Replace(alignment, "");
                    aLine = aLine.Replace(aLine.Substring(aLine.IndexOf('['), aLine.IndexOf(']') + 1 - aLine.IndexOf('[')), "");
                }
            }

            pattern = @"^(\s\\|\\)(\w*)(\s|\w)\b|(\s\\|\\)(\w*)(\s|\w)\b";
            m = Regex.Match(tempALine, pattern);
            while (m.Success)
            {
                if (!m.Value.Equals("\\cr"))
                {
                    tempALine.Replace(m.Value, ""); //removing latex command 
                }
                m = m.NextMatch();
            }
            Char[] aLineCharArray = tempALine.ToCharArray();
            char previousChar = ' ';
            char prevPrevChar = ' ';
            columnCount = 1;
            foreach (char c in aLineCharArray)
            {
                if (c.Equals('&'))
                {
                    columnCount = columnCount + 1;
                    matrixColumnCount = columnCount;
                }

                if (isBorderMatrix || isBorderMatrixEndLine)
                {
                    if (c.Equals('r') && previousChar.Equals('c') && prevPrevChar.Equals('\\'))
                    {
                        matrixRowCount = matrixRowCount + 1;
                        columnCount = 1; //reset column count for next row
                    }
                }
                else
                {
                    if (c.Equals('\\') && previousChar.Equals('\\'))
                    {
                        matrixRowCount = matrixRowCount + 1;
                        columnCount = 1; //reset column count for next row
                    }
                }
                prevPrevChar = previousChar;
                previousChar = c;
            }

            if (matrixColumnCount == 0) { matrixColumnCount = prevColumnCount; }
            if (isBorderMatrix && matrixRowCount == 1)
            {
                aLine = aLine + " verticalBorder ";
            }
            if (isBorderMatrix || isBorderMatrixEndLine)
            {
                if (aLine.Contains('&') && matrixRowCount != 1)
                {
                    string tempVerticalBorder = aLine.Substring(0, aLine.IndexOf('&'));
                    verticalBorder = verticalBorder + ". . ." + tempVerticalBorder;
                    aLine = aLine.Replace(tempVerticalBorder, "");
                }
            }
            aLine = aLine.Replace("&", " . . . ");
            StringBuilder completedWord = new StringBuilder();
            if (aLine.Contains("\\\\") || aLine.Contains("\\cr"))
            {

                string[] testarray = aLine.Contains("\\\\") ? new string[] { "\\\\" } : new string[] { "\\cr" };
                string[] result = aLine.Split(testarray, StringSplitOptions.RemoveEmptyEntries);
                foreach (string v in result)
                {
                    if (!String.IsNullOrWhiteSpace(v) && !v.Contains("end of border matrix") && !v.Contains("verticalBorder"))
                    {
                        completedWord.Append(v);
                        completedWord.Append(" asteriklinebreakasterik ");
                    }
                    else
                    {
                        completedWord.Append(v);
                    }

                }
            }
            if (String.IsNullOrEmpty(completedWord.ToString()))
            {
                return aLine;
            }
            else
            {
                return completedWord.ToString();
            }
        }

        private void populateRoots()
        {
            if (roots.Count <= 0)
            {
                roots.Add(2, "square");
                roots.Add(3, "cube");
            }
        }
        public static bool getSymbols(String symbolArg)
        {
            foreach (String symbol in symbols)
            {
                if (symbol.Equals(symbolArg))
                {
                    return true;
                }
            }
            return false;
        }
        private bool? checkNumerator(XmlNode Root, bool? isNumerator)
        {
            if (Root.ParentNode.LocalName == "mrow" && Root.PreviousSibling == null)
            {
                if (isNumerator == true)
                {
                    parsedEquation += " fraction numerator ";// + Root.FirstChild.InnerText + ". . .";
                    if (Root.FirstChild != null && (Root.FirstChild.NextSibling == null || Root.FirstChild.NextSibling.LocalName == "span"))
                        isNumerator = false;
                    else
                        isNumerator = true;
                }
                else
                {
                    isNumerator = true;
                    parsedEquation += " denominator ";
                }
            }

            return isNumerator;
        }
        public void handleFracionalEquation(System.Xml.XmlNode Root, ref bool? isNumerator)  //this function uses inorder binary tree traversal logic
        {
            bool hasValue;
            if (Root.LocalName == "mo")
            {
                isNumerator = checkNumerator(Root, isNumerator);
                if (Root.ParentNode.FirstChild.LocalName == "span")
                    Root.ParentNode.RemoveChild(Root.ParentNode.FirstChild);
                String temp = "<temp>" + Root.ParentNode.InnerXml.PadLeft(6);
                temp = temp.PadRight(7) + "</temp>";
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(temp);
                mathBusinesLogic(doc);  //handle summations and integration when it comes in between fractional equation 
            }
            else if (Root.LocalName == "mi")
            {
                isNumerator = checkNumerator(Root, isNumerator);
                String temp = "<temp>" + Root.ParentNode.InnerXml.PadLeft(6);
                temp = temp.PadRight(7) + "</temp>";
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(temp);
                mathBusinesLogic(doc);  //handle summations and integration when it comes in between fractional equation
            }
            else if (Root.LocalName == "msqrt")
            {
                isNumerator = checkNumerator(Root, isNumerator);
                String temp = "<temp>" + Root.OuterXml.PadLeft(6);
                temp = temp.PadRight(7) + "</temp>";
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(temp);
                mathBusinesLogic(doc);
            }
            else if (Root.LocalName == "mroot")
            {
                isNumerator = checkNumerator(Root, isNumerator);
                String temp = "<temp>" + Root.ParentNode.InnerXml.PadLeft(6);
                temp = temp.PadRight(7) + "</temp>";
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(temp);
                mathBusinesLogic(doc);
            }
            else if (Root.LocalName == "#comment" || Root.LocalName == "#text")
            {
                if (Root.InnerText.Trim() != "\\limits")
                {
                    isNumerator = checkNumerator(Root, isNumerator);
                    String temp = "<temp>" + Root.ParentNode.InnerXml.PadLeft(6);
                    temp = temp.PadRight(7) + "</temp>";
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(temp);
                    mathBusinesLogic(doc);
                }
            }
            else if (Root != null && Root.LocalName != "#text" && Root.LocalName != "span")
            {
                if (Root.FirstChild != null)
                    if (Root.FirstChild.PreviousSibling == null && Root.FirstChild.FirstChild != null && Root.FirstChild.FirstChild.LocalName == "mfrac" && Root.FirstChild.FirstChild.PreviousSibling == null)
                        parsedEquation = " fraction numerator. . . " + parsedEquation;
                handleFracionalEquation(Root.FirstChild, ref isNumerator);
                if (Root.LocalName == "mfrac")
                {
                    hasValue = lookupTable.TryGetValue("\\frac", out ValueByKey);
                    if (hasValue)
                    {
                        parsedEquation += " " + ValueByKey + " ";
                    }
                    if (Root.FirstChild.NextSibling.FirstChild.LocalName == "mfrac")
                    {
                        parsedEquation += " denominator ";
                    }
                    if (Root.FirstChild.NextSibling != null && Root.FirstChild.NextSibling.LocalName == "mrow" && Root.FirstChild.NextSibling.FirstChild.LocalName != "mfrac")
                        isNumerator = false;
                }
                else if (Root.LocalName == "mrow" && Root.FirstChild != null && Root.FirstChild.LocalName == "span")
                {
                    if (isNumerator == true)
                    {
                        parsedEquation += " fraction numerator " + Root.FirstChild.InnerText + ". . .";
                        if (Root.FirstChild.NextSibling == null || Root.FirstChild.NextSibling.LocalName == "span")
                            isNumerator = false;
                        else
                            isNumerator = true;
                    }
                    else
                    {
                        isNumerator = true;
                        parsedEquation += " denominator " + Root.FirstChild.InnerText + ". . .";
                    }
                }
                else if (Root.FirstChild != null && Root.FirstChild.NextSibling != null && getSymbols(Root.FirstChild.NextSibling.InnerText.Trim()))
                    parsedEquation += Root.FirstChild.NextSibling.InnerText;
                if (Root.FirstChild != null && Root.FirstChild.NextSibling != null && getSymbols(Root.FirstChild.NextSibling.InnerText.Trim()))
                    handleFracionalEquation(Root.FirstChild.NextSibling.NextSibling, ref isNumerator);
                else if (Root.FirstChild != null && Root.FirstChild.NextSibling != null)
                {
                    handleFracionalEquation(Root.FirstChild.NextSibling, ref isNumerator);
                    if (Root.NextSibling != null && Root.NextSibling.NextSibling != null && Root.NextSibling.NextSibling.LocalName == "mfrac" && Root.NextSibling.NextSibling.NextSibling == null && Root.NextSibling.LocalName != "mo")
                    {
                        if (Root.PreviousSibling != null)
                        {
                            if (Root.NextSibling.LocalName == "span")
                            {
                                parsedEquation += " " + Root.NextSibling.InnerText;
                            }
                            handleFracionalEquation(Root.NextSibling.NextSibling, ref isNumerator);
                        }
                    }
                }
            }
            else if (Root.PreviousSibling != null && Root.PreviousSibling.LocalName == "mo" && Root.LocalName == "span" && Root.InnerText != "_")
            {
                parsedEquation += Root.InnerText;
            }
            else if (Root.PreviousSibling != null && Root.PreviousSibling.LocalName == "span" && Root.LocalName == "span")
            {
                parsedEquation += Root.InnerText;
                if (Root.NextSibling != null)
                    handleFracionalEquation(Root.NextSibling, ref isNumerator);
            }

            if (Root.LocalName == "mrow" && Root.NextSibling == null && Root.PreviousSibling.LocalName == "mrow")
            {
                parsedEquation += " end Frac. . . ";
            }
        }

        private void mathBusinesLogic(XmlDocument doc)
        {
            String tempRootXML = "";
            foreach (XmlNode v in doc.DocumentElement.ChildNodes)
            {
                string temp = "";
                string vInerText = v.InnerText;
                vInerText = vInerText.Trim();
                bool hasValue = false;
                if (v.Name == "mroot")
                {
                    populateRoots();
                    XmlNode LastChild = v.LastChild;          //extract root
                    tempRootXML = "<temp>" + LastChild.InnerXml.PadLeft(6);
                    tempRootXML = tempRootXML.PadRight(7) + "</temp>";
                    XmlDocument docRoot = new XmlDocument();
                    docRoot.LoadXml(tempRootXML);
                    if (docRoot.ChildNodes.Count == 1 && docRoot.FirstChild.FirstChild.LocalName == "span" && docRoot.FirstChild.FirstChild.InnerText.All(char.IsDigit))
                    {
                        XmlNode x = docRoot.FirstChild.FirstChild;
                        bool hasValueRoots;
                        hasValueRoots = roots.TryGetValue(Convert.ToInt32(x.InnerText), out ValueByKey);
                        if (hasValueRoots)
                        {
                            parsedEquation += ValueByKey + " root inside radical. . .";
                        }
                        else
                        {
                            parsedEquation += x.InnerText + "th root inside radical. . .";
                        }
                    }
                    else //root is an expression
                    {
                        parsedEquation += ". . .root. . . ";
                        mathBusinesLogic(docRoot);
                        parsedEquation += " inside radical ";
                    }
                    v.RemoveChild(LastChild);   //remove root 
                    XmlElement e = v.FirstChild.OwnerDocument.CreateElement("span");
                    e.InnerText = ". . .end radical. . .";
                    v.FirstChild.AppendChild(e); //append node to indicate end of radical
                    String tempXML = "<temp>" + v.InnerXml.PadLeft(6);  //equation inside radical
                    tempXML = tempXML.PadRight(7) + "</temp>";
                    XmlDocument docSqrt = new XmlDocument();
                    docSqrt.LoadXml(tempXML);
                    mathBusinesLogic(docSqrt);
                }
                else if (v.Name == "msqrt")
                {
                    hasValue = lookupTable.TryGetValue("\\sqrt", out ValueByKey);
                    if (hasValue)
                    {
                        parsedEquation += " " + ValueByKey + " inside radical. . .";
                    }
                    XmlElement e = v.FirstChild.OwnerDocument.CreateElement("span");
                    e.InnerText = ". . .end radical. . .";
                    v.FirstChild.AppendChild(e); //append node to indicate end of radical
                    String tempXML = v.InnerXml;
                    XmlDocument docSqrt = new XmlDocument();
                    docSqrt.LoadXml(tempXML);
                    mathBusinesLogic(docSqrt);
                }
                else if (v.LocalName == "mfrac")
                {
                    bool? isNumerator = true;
                    handleFracionalEquation(v, ref isNumerator);
                }
                else if (v.LocalName == "mo" || vInerText == "\\iiiint" || vInerText == "\\idotsint" || v.LocalName == "#text" || v.LocalName == "mi" || vInerText == "\\prod"
                     || vInerText == "\\liminf" || vInerText == "\\limsup")
                {
                    if (!(v.LocalName == "#text"))
                    {
                        temp = v.LastChild != null ? v.LastChild.InnerText : vInerText;
                    }
                    else
                    {
                        temp = v.NextSibling != null ? v.NextSibling.InnerText : v.InnerText;
                    }

                    if (v.InnerText == "{" || v.InnerText == "}")
                    {
                        parsedEquation += " " + v.InnerText + " "; //to parse braces in case of sets
                        continue;
                    }
                    if (v.InnerText == "|") //double vertical bar
                    {
                        parsedEquation += " " + v.InnerText + v.InnerText + " ";
                        continue;
                    }
                    temp = String.Join("", temp.Where(c => Char.IsLetter(c)));
                    temp = '\\' + temp;
                    hasValue = lookupTable.TryGetValue(temp, out ValueByKey);
                    if (hasValue)
                    {
                        parsedEquation += " " + ValueByKey + " ";
                    }
                    if (v.LocalName == "mo" && v.FirstChild != null && (v.FirstChild.InnerText.Contains(". . .end upper limit. . .") || v.FirstChild.InnerText.Contains(". . .end lower limit. . .")))
                    {
                        parsedEquation += " " + v.FirstChild.InnerText + " ";
                    }
                }
                else if (vInerText == ("\\limits"))
                {
                    continue;
                }
                else if (v.Name == "mrow")
                {
                    String tempXML = "<temp>" + v.InnerXml.PadLeft(6);
                    tempXML = tempXML.PadRight(7) + "</temp>";

                    XmlDocument docRoot = new XmlDocument();
                    docRoot.LoadXml(tempXML);
                    mathBusinesLogic(docRoot);
                }
                else if (v.LocalName == "span" && v.InnerText.Contains("&gt;"))
                {
                    parsedEquation += " " + v.InnerText.Replace("&gt;", " greater than ") + " ";
                }
                else if (v.LocalName == "span" && v.InnerText.Contains("&lt;"))
                {
                    parsedEquation += " " + v.InnerText.Replace("&lt;", " less than ") + " ";
                }
                else if (v.LocalName == "#comment")
                {
                    continue;
                }
                else
                {
                    parsedEquation += " " + v.InnerText + " ";
                }
            }
        }

        private string mathSymbolLookup(String parsedEquation)
        {
            string symbols = String.Join("", mathCharLookUpTable.Keys);
            foreach (char c in symbols)
            {
                if (parsedEquation.Contains(c.ToString()))
                {
                    bool hasValue;
                    hasValue = mathCharLookUpTable.TryGetValue(c.ToString(), out ValueByKey);
                    if (hasValue)
                    {
                        parsedEquation = parsedEquation.Replace(c.ToString(), " " + ValueByKey + " ");
                    }
                }
            }
            parsedEquation = Regex.Replace(parsedEquation, "dot   dot   dot", ". . .");
            return parsedEquation;
        }
        private string mathParsing(string mathEquation, ref string parsedCode)
        {
            if (mathEquation.Contains("^") || mathEquation.Contains("_"))
            {
                handlePowerSub(ref mathEquation);
            }
            parsedEquation = "";
            if (mathEquation.Contains("\\dv") || mathEquation.Contains("\\pdv") || mathEquation.Contains("\\fdv"))  //if differential equation
            {
                mathEquation = handleDerivatives(mathEquation);
            }
            if (checkForSets(mathEquation)) //if set equation
            {
                mathEquation = handleSets(mathEquation);
            }
            if (mathEquation.Contains("\\dd"))
            {
                mathEquation = handleDifferentials(mathEquation);
            }
            if (checkForCombinatorics(mathEquation))
            {
                mathEquation = handleCombinatorics(mathEquation);
            }
            if (mathEquation != null && checkForMatrices(mathEquation))
            {
                if (mathEquation.Contains("end") && !mathEquation.Contains("end sub") && !mathEquation.Contains("end power") && !mathEquation.Contains("end frac"))
                {
                    if (matrixColumnCount == 0)//only one column
                        matrixColumnCount++;
                    string matrixSize = matrixRowCount + " by " + matrixColumnCount;
                    parsedCode = parsedCode.Replace("matrix size", matrixSize);
                    matrixRowCount = 0;
                    matrixColumnCount = 0;
                    parsedCode = parsedCode.Replace(arrayAlign, " ");
                }
                mathEquation = handleMatrices(mathEquation);
                if (mathEquation.Contains("end") && mathEquation.Contains("start")
                    && !mathEquation.Contains("end sub") && !mathEquation.Contains("end power") && !mathEquation.Contains("end frac")) //for inline matrix
                {
                    if (matrixColumnCount == 0)//only one column
                        matrixColumnCount++;
                    string matrixSize = matrixRowCount + " by " + matrixColumnCount;
                    mathEquation = mathEquation.Replace("matrix size", matrixSize);
                    matrixRowCount = 0;
                    matrixColumnCount = 0;
                }
                if (isBorderMatrixEndLine)
                {
                    isBorderMatrixEndLine = false;
                    string matrixSize = (matrixRowCount - 1) + " by " + (matrixColumnCount - 1);
                    parsedCode = parsedCode.Replace("verticalBorder", "vertical border" + verticalBorder + ". . . parentheses " + matrixSize);
                    verticalBorder = "";
                    matrixRowCount = 0;
                    matrixColumnCount = 0;
                    parsedCode = parsedCode.Replace(arrayAlign, " ");
                }
            }
            if (mathEquation.Contains("\\\\")) //remove line break slashes
            {
                mathEquation = mathEquation.Replace("\\\\", "").Trim();
            }
            String XMLTokens = MathML.ConvertLatextToMathMl(mathEquation);
            if (XMLTokens.Contains("out of range") || XMLTokens.Contains("Unexpected format") || XMLTokens.Contains("Object reference not set"))
            {
                if (isBorderMatrixEndLine || isBorderMatrix)
                {
                    return mathEquation;
                }
                return parsedEquation;
            }//invalid xml

            XMLTokens = "<temp>" + XMLTokens.PadLeft(6);
            XMLTokens = XMLTokens.PadRight(7) + "</temp>";
            XmlDocument doc = new XmlDocument();
            mathEquation = "\\begin{document}\n" + mathEquation + "\n" + "\\end{document}";
            doc.LoadXml(XMLTokens);

            mathBusinesLogic(doc);
            parsedEquation = parsedEquation.Replace("&", "");//replace & used for spacing
            parsedEquation = parsedEquation.Replace("!", "factorial"); //repalce ! with factorial 
            parsedEquation = mathSymbolLookup(parsedEquation);
            return parsedEquation;
        }

        private void multiLineEquationCheck(StringReader sourceCodeSR, ref string aLine, ref string parsedCode, string startTag, string endTag, ref string trimmedAline, ref string noSpacesLine)
        {
            bool isEndTagSpotted = false;
            int endTagFlag = 0;
            if (trimmedAline.Equals(startTag))
            {
                aLine = sourceCodeSR.ReadLine();
                trimmedAline = aLine.Trim();
            }
            while (aLine != null && !trimmedAline.Equals(endTag) && !isEndTagSpotted)
            {
                if (!String.IsNullOrEmpty(aLine.Replace(startTag, "")))
                {
                    aLine = aLine.Replace(startTag, "");
                }
                if (aLine != "")
                {
                    if (aLine != null && aLine.StartsWith("%")) //skip commented out lines
                    {
                        aLine = "";
                        continue;
                    }
                    noSpacesLine = Regex.Replace(aLine, @"\s", "");
                    if (aLine != null && (noSpacesLine.Contains("\\right.") || noSpacesLine.Contains("\\left."))) //replace the empty counterpart
                    {
                        aLine = aLine.Replace("\\right", "");
                        aLine = aLine.Replace("\\left", "");
                        aLine = aLine.Replace(".", "");
                    }
                    aLine = inlineEquationCheck(aLine, ref parsedCode, sourceCodeSR, ref noSpacesLine, ref trimmedAline);
                    miscellenousChecks(ref aLine, ref parsedCode, trimmedAline);

                    parsedEquation = mathParsing(aLine, ref parsedCode);

                    if (parsedEquation.Contains("\\\\")) //remove line break slashes
                    {
                        parsedEquation = parsedEquation.Replace("\\\\", "").Trim();
                    }

                    if (String.IsNullOrEmpty(parsedEquation))
                    {
                        parsedCode = parsedCode + aLine + "\n";
                    }
                    else
                    {
                        String a1 = aLine.Replace(aLine, parsedEquation);
                        String a2 = a1.Replace(startTag, "");
                        a2 = a1.Replace(endTag, "");
                        parsedCode = parsedCode + a2 + "\n";

                        if (matrixRowCount == 1 && !aLine.Contains("\\begin") && !isBorderMatrix) //replace alignment
                        {
                            parsedCode = parsedCode.Replace(parsedEquation, "");
                        }
                    }

                }
                aLine = sourceCodeSR.ReadLine();
                if (endTagFlag == 1) { isEndTagSpotted = true; } //end Tag was in the previous line

                if (aLine != null)
                {
                    if (aLine.Contains(endTag))//&& !aLine.Trim().Equals(endTag))
                    {
                        string tempALine = Regex.Replace(aLine, @"\s", "");
                        if (tempALine.Equals(endTag))
                        {
                            endTagFlag = 1;
                            aLine = sourceCodeSR.ReadLine();
                        }
                        else
                        {
                            aLine = aLine.Replace(endTag, "");
                            endTagFlag = 1;
                        }
                    }
                    trimmedAline = aLine.Trim();
                }
            }
        }

        private string inlineEquationCheck(string aLine, ref string parsedCode, StringReader sourceCodeCSR, ref string noSpacesLine, ref string trimmedAline)
        {
        CheckForTags:
            Dictionary<string, string> tags = new Dictionary<string, string>();
            string startTag = "";
            string endTag = "";
            if (aLine == null) { return ""; };
            if (aLine.Contains("$") && !aLine.Contains("$$"))
            {
                tags.Add("$", "$");
            }
            if (aLine.Contains("\\("))
            {
                tags.Add("\\(", "\\)");
            }
            if (aLine.Contains("\\begin{math}"))
            {
                tags.Add("\\begin{math}", "\\end{math}");
            }
            if (aLine.Contains("$$"))
            {
                tags.Add("$$", "$$");
            }
            if (aLine.Contains("\\["))
            {
                tags.Add("\\[", "\\]");
            }
            if (aLine.Contains("\\begin{equation}"))
            {
                tags.Add("\\begin{equation}", "\\end{equation}");
            }
            if (aLine.Contains("\\begin{displaymath}"))
            {
                tags.Add("\\begin{displaymath}", "\\end{displaymath}");
            }
            if (aLine.Contains("\\begin{eqnarray*}"))
            {
                tags.Add("\\begin{eqnarray*}", "\\end{eqnarray*}");
            }
            if (aLine.Contains("\\begin{eqnarray}"))
            {
                tags.Add("\\begin{eqnarray}", "\\end{eqnarray}");
            }
            if (aLine.Contains("\\begin{align*}"))
            {
                tags.Add("\\begin{align*}", "\\end{align*}");
            }
            if (aLine.Contains("\\begin{align}"))
            {
                tags.Add("\\begin{align}", "\\end{align}");
            }
            if (tags.Count == 0) //no inline tag is found in the equation
            {
                return aLine;
            }
            string compaLine = aLine;
            List<String> equationsToBeParsedList = new List<string>();
            foreach (var tag in tags)
            {
                startTag = tag.Key;
                endTag = tag.Value;
            ExtractEquation:
                int startIndex = compaLine.IndexOf(startTag) + startTag.Length;
                int endIndex = endTag == "$" || endTag == "$$" ? compaLine.IndexOf(endTag, startIndex) : compaLine.IndexOf(endTag);
                if (endIndex < 0)
                {
                    multiLineEquationCheck(sourceCodeCSR, ref aLine, ref parsedCode, startTag, endTag, ref trimmedAline, ref noSpacesLine);
                    goto CheckForTags;
                }
                String equationToBeParsed = startIndex > 0 && endIndex > 0 ? compaLine.Substring(startIndex, endIndex - startIndex) : ""; //this is the equation you need to 
                StrAltEquation = equationToBeParsed;
                if (equationToBeParsed != null)
                {
                    equationsToBeParsedList.Add(equationToBeParsed);
                }
                compaLine = compaLine.Replace(startTag + equationToBeParsed + endTag, "");
                if (compaLine.Contains(startTag))
                {
                    goto ExtractEquation;
                }
            }
            String a2 = "";
            String a1 = aLine; 
            if (aLine != null) 
            {
                foreach (var equation in equationsToBeParsedList) //parse equations
                {
                    if (!String.IsNullOrEmpty(equation))
                    {
                        parsedEquation = mathParsing(equation, ref parsedCode);
                        a1 = a1.Replace(equation, parsedEquation); //because strings are immutable

                    }
                }
                a2 = a1;
                foreach (var tag in tags) //replace tags
                {
                    startTag = tag.Key;
                    endTag = tag.Value;
                    a2 = a2.Replace(startTag, "");
                    a2 = a2.Replace(endTag, "");
                }
            }
            return a2;
        }

        public string handleAlgorithms3(string code)
        {
            string parsedcode = code;
            string orignalLine = "";
            StringReader sourceCodeSR = new StringReader(code);
            string aLine = sourceCodeSR.ReadLine();
            aLine = aLine.ToLower();
            Stack<char> stack1 = new Stack<char>();

            Char[] aLineCharArray = aLine.ToCharArray();
            orignalLine = aLine;
            while (aLine != null)
            {
                if (!String.IsNullOrEmpty(aLine) && (aLine.Contains("\\while") || aLine.Contains("\\if") || aLine.Contains("for") || bracketStack.Count > 0))
                {
                    {
                        if ((aLine.Contains("\\while")))
                        {
                            aLine = aLine.Replace("\\while", " while ");
                            endCommands.Push("end while");
                            endCommands.Push("end");
                        }
                        if (aLine.Contains("\\for"))
                        {
                            aLine = aLine.Replace("\\for", " for ");
                            endCommands.Push("end for");
                            endCommands.Push("end");
                        }
                        if (aLine.Contains("\\foreach"))
                        {
                            aLine = aLine.Replace("\\foreach", " for each ");
                            endCommands.Push("end for each");
                            endCommands.Push("end");
                        }
                        if (aLine.Contains("\\elseif"))
                        {
                            aLine = aLine.Replace("\\elseif", " else if ");
                            endCommands.Push("end else if");
                            endCommands.Push("end");

                        }
                        else
                        {
                            if (aLine.Contains("if"))
                            {
                                aLine = aLine.Replace("\\if", " if ");
                                endCommands.Push("end if");
                                endCommands.Push("end");
                            }
                        }
                        if (aLine.Contains("else") && !aLine.Contains("\\elseif") && !aLine.Contains("else if"))
                        {
                            aLine = aLine.Replace("\\else", " else ");
                            endCommands.Push("end else");
                        }
                    }

                    if (aLine.EndsWith("{"))
                    {
                        //do nothing;
                    }
                    else
                    {
                        bracketStack.Push('0'); //to indicate the start of instruction set
                    }
                }
                if (bracketStack.Count != 0)
                {
                    if (bracketStack.Peek() == '0')
                    {
                        bracketStack.Pop();
                    }
                    foreach (char c in aLineCharArray)
                    {
                        if (c != '}')
                        {
                            bracketStack.Push(c);
                        }
                        else
                        {
                            if (!aLine.Contains("end") && endCommands.Count > 0)
                            {
                                if (endCommands.Peek() == "end")
                                {
                                    endCommands.Pop();
                                }
                                else
                                {
                                    aLine = aLine + endCommands.Pop();
                                }
                            }
                            while (bracketStack.Count != 0 && bracketStack.Peek() != '{')
                            {
                                bracketStack.Pop();
                            }
                            if (bracketStack.Count != 0 && bracketStack.Peek() == '{')
                            {
                                bracketStack.Pop();
                            }
                        }
                    }
                }
                algoParsed = algoParsed + aLine + "\n";
                aLine = sourceCodeSR.ReadLine();
                if (aLine != null)
                {
                    aLine = aLine.ToLower();
                    aLineCharArray = aLine.ToCharArray();
                }
                orignalLine = aLine;
            }
            return algoParsed;
        }

        private static String StartDeTexProcess()
        {
            PlatformID platform = Environment.OSVersion.Platform;
            string output = "";
            if (platform.ToString() == Platform.Win32NT.ToString() || platform.ToString() == Platform.WIN64NT.ToString())
            {
                ProcessStartInfo psiWindows;
                Process pWindows;
                string detexPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())) + "\\opendetex";
                psiWindows = new ProcessStartInfo("cmd.exe", "/K " + "cd " + detexPath);
                psiWindows.CreateNoWindow = true;
                psiWindows.UseShellExecute = false;
                psiWindows.RedirectStandardInput = true;
                psiWindows.RedirectStandardOutput = true;
                pWindows = Process.Start(psiWindows);
                pWindows.StandardInput.WriteLine("detex input.tex");
                pWindows.StandardInput.Flush();
                pWindows.StandardInput.Close();
                output = pWindows.StandardOutput.ReadToEnd();
                output = output.Replace(detexPath + ">", "");
                output = output.Replace("detex input.tex", "");
                pWindows.WaitForExit();
            }
            else
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "detex";
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                string arguementPath = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())) + "/opendetex"; //bash script to run detex, output is written in output.txt
                psi.Arguments = arguementPath + "/input.txt";
                Process p = Process.Start(psi);
                output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }
            return output;
        }

        public void filterLatexSourceCodeUsingOpenDetex()
        {
            lookupTable = LookUpFile.GetLookTable();
            mathCharLookUpTable = LookUpFile.GetMathCharLookTable();

            StringReader sourceCodeSR = new StringReader(this.LatexEquation);
            String aLine = "";
            //string path = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            //path = path + "/opendetex/input.txt";
            //System.IO.StreamWriter writer = new System.IO.StreamWriter(path);

            //string pathTex = Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory()));
            //pathTex = pathTex + "\\opendetex\\input.tex";
            //System.IO.StreamWriter writerTex = new System.IO.StreamWriter(pathTex);
            String parsedCode = "";

            while (aLine != null)
            {
                aLine = sourceCodeSR.ReadLine();      
                string trimmedAline = aLine;
                trimmedAline = aLine != null ? trimmedAline.Trim() : ""; //trimmed aLine for comparison 

                if (!IsBeginDocument)
                {
                   if (trimmedAline.Contains("[tagged]") && trimmedAline.Contains("{accessibility}"))
                      {
                        //|| trimmedAline.ToLower().Contains("\\usepackage[tagged]{accessibility}"
                        if (!trimmedAline.StartsWith("%"))
                        {
                            IsTagPackage = true;
                        }
                      }
                    if (trimmedAline.ToLower().Contains("\\begin{document}") || trimmedAline.ToLower().Equals("\\begin{document}"))
                    {
                        if (!IsTagPackage)
                        {
                            parsedCode = parsedCode + "\\usepackage[tagged]{accessibility}" + "\n";
                        }
                        IsBeginDocument = true;
                    }
                }

                string noSpacesLine = aLine != null ? Regex.Replace(aLine, @"\s", "") : "";
                if (aLine != null && (noSpacesLine.Contains("\\right.") || noSpacesLine.Contains("\\left."))) //replace the empty counterpart
                {
                    aLine = aLine.Replace("\\right", "");
                    aLine = aLine.Replace("\\left", "");
                    aLine = aLine.Replace(".", "");
                }

                if (aLine != null && aLine.StartsWith("\\DeclareMathOperator"))
                {
                    string pattern = @"({)(\w*)(})|({\\|\\)(\w*)(}|\w)"; //to extract latex command
                    Match m = Regex.Match(aLine, pattern);
                    string value = "";
                    while (m.Success)
                    {
                        if (m.Value.StartsWith("DeclareMathOperator"))
                        {
                            m = m.NextMatch();
                            continue;
                        }
                        value = m.Value; //because m.Value is read-only
                        if (value.Contains("{"))
                        {
                            value = value.Replace("{", "");
                        }
                        if (value.Contains("}"))
                        {
                            value = value.Replace("}", "");
                        }
                        mathEquaionCommands.Add((value).Trim());
                        m = m.NextMatch();
                    }
                    declaredCommands.Add(mathEquaionCommands.ElementAt(0), mathEquaionCommands.ElementAt(1));
                    mathEquaionCommands.Clear();
                }

                miscellenousChecks(ref aLine, ref parsedCode, trimmedAline);

                if (aLine != null && trimmedAline.StartsWith("%"))
                {
                    //aLine = "";
                    if (parsedCode.Trim().Equals(""))
                    {
                        parsedCode = aLine + "\n";
                        continue;
                    }
                    else
                    {
                        parsedCode = parsedCode + aLine + "\n";
                        continue;
                    }
                }
                else if (aLine != null && aLine.StartsWith("*customcode*"))
                {
                    aLine = aLine.Replace("*customcode*", " ");
                    parsedCode = parsedCode + aLine + "\n";
                    continue;
                }
                //mine 
                //if ( aLine!=null && aLine.Contains("\\pdftooltip"))
                //{
                //    if (parsedCode.Equals(""))
                //    {
                //        parsedCode = aLine + "\n";

                //    }
                //    else
                //    {
                //        parsedCode = parsedCode + aLine + "\n";
                //    }
                //    if (matrixRowCount == 1 && !aLine.Contains("\\begin")) //replace alignment
                //    {
                //        parsedCode = parsedCode.Replace(aLine, "");
                //    }
                //}
                //else if (aLine != null && aLine.Contains("\\pdftooltip"))
                //{
                //    if (parsedCode.Equals(""))
                //    {
                //        parsedCode = aLine + "\n";

                //    }
                //    else
                //    {
                //        parsedCode = parsedCode + aLine + "\n";
                //    }
                //}
                else if (aLine != null && (aLine.Contains("$$") || aLine.Contains("\\(") || aLine.Contains("\\begin{math}") || aLine.Contains("$")
                            || aLine.Contains("\\[") || aLine.Contains("\\begin{equation}") || aLine.Contains("\\begin{displaymath}")
                            || aLine.Contains("\\begin{eqnarray*}") || aLine.Contains("\\begin{eqnarray}")
                            || aLine.Contains("\\begin{align*}") || aLine.Contains("\\begin{align}")))
                {
                    string a2;
                    a2 = inlineEquationCheck(aLine, ref parsedCode, sourceCodeSR, ref noSpacesLine, ref trimmedAline);
                    //ToolTipTextLatex = @"\pdftooltip{" + trimmedAline + "}{" + a2 + "}" + "\n";
                    //ToolTipTextLatex = @"begin{equation}\alt{"+a2+"}"+trimmedAline+"\\end{equation}";
                    ToolTipTextLatex = @"\begin{equation}\alt{" + a2 + "}" + StrAltEquation+ "\\end{equation}";
                    StrAltEquation = "";
                    parsedCode = parsedCode + ToolTipTextLatex + "\n";
                    //  parsedCode = parsedCode + a2 + "\n"; //write line with parsed equation into an input file. input file will be used by detex
                    // ToolTipTextLatex = a2 + "\n";
                }
                else
                {
                    //   aLine = detexModification.manualDetex(aLine);
                    if (parsedCode.Equals(""))
                    {
                        parsedCode = aLine + "\n";

                    }
                    else
                    {
                        parsedCode = parsedCode + aLine + "\n";
                    }
                    if (matrixRowCount == 1 && !aLine.Contains("\\begin")) //replace alignment
                    {
                        parsedCode = parsedCode.Replace(aLine, "");
                    }
                }
            }

            if (parsedCode.ToLower().Contains("\\while") || parsedCode.ToLower().Contains("\\if") || parsedCode.ToLower().Contains("\\for")
                || parsedCode.ToLower().Contains("\\else") || parsedCode.ToLower().Contains("\\elseif"))
            {
                parsedCode = handleAlgorithms3(parsedCode);
            }
            LatexDocument = parsedCode+"\n";
            //LatexDocument = @"u r hey\nhow\nhello\n";

            // LatexDocument = @"\\pdftooltip{$\\frac{ 3} { 4}$}{ fraction numerator 3. . .over  denominator 4. . .end Frac. . .   }\nhello\n";

            //parsedCode = parsedCode.Replace("mbox", "");
            //parsedCode = parsedCode.Replace("latextext", "");
            //writer.Write(parsedCode);
            //writer.Close();
            //writerTex.Write(parsedCode);
            //writerTex.Close();
            //String pdfAccessibleText = StartDeTexProcess();
            //pdfAccessibleText = mathSymbolLookup(pdfAccessibleText);
            //if (!IsSpeak)
            //{
            //    TTS.Speak(pdfAccessibleText);
            //}
        }
        public void ConvertLatexEquationToText()
        {
            //PdfMode obj = new PdfMode(LatexEquation);
            //obj.filterLatexSourceCodeUsingOpenDetex();
            filterLatexSourceCodeUsingOpenDetex();
        }
    }

    //public string FilterLatexEquation(string Equation, string EquationText)
    //{
    //    if (i == 0)
    //    {
    //        LatexEquation = @"\pdftooltip{$" + Equation + "$}" + EquationText + "\n";
    //        i++;
    //    }
    //    else
    //    {
    //        //  strLatex = @""+ strLatex + ""+@"\pdftooltip{$" + Equation + "$}" + EquationText;
    //        LatexEquation = @"" + LatexEquation + "" + @"\pdftooltip{$" + Equation + "$}" + EquationText;
    //    }
    //    latexEquationText = LatexEquation;
    //    return LatexEquation;
    //}

    //}
}

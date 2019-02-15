using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTS_server_alap_alpha_v1
{
    class LatexCommands
    {
        #region LatexMathTags
        public static string DOLLAR_TAG = "$";
        public static string DOUBLE_DOLLAR_TAG = "$$";
        public static string OPEN_PARANTHESIS_TAG = "\\(";
        public static string CLOSE_PARANTHESIS_TAG = "\\)";
        public static string BEGIN_MATH_TAG = "\\begin{math}";
        public static string END_MATH_TAG = "\\end{math}";
        public static string OPEN_SQUAREBRAC_TAG = "\\[";
        public static string CLOSE_SQUAREBRAC_TAG = "\\]";
        public static string BEGIN_EQUATION_TAG = "\\begin{equation}";
        public static string END_EQUATION_TAG = "\\end{equation}";
        public static string BEGIN_DISPLAY_MATH_TAG = "\\begin{displaymath}";
        public static string END_DISPLAY_MATH_TAG = "\\end{displaymath}";
        public static string BEGIN_EQN_ARRAY_ASTRIK_TAG = "\\begin{eqnarray*}";
        public static string END_EQN_ARRAY_ASTRIK_TAG = "\\end{eqnarray*}";
        public static string BEGIN_EQN_ARRAY_TAG = "\\begin{eqnarray}";
        public static string END_EQN_ARRAY_TAG = "\\end{eqnarray}";
        public static string BEGIN_ALIGN_ASTRIK_TAG = "\\begin{align*}";
        public static string END_ALIGN_ASTRIK_TAG = "\\end{align*}";
        public static string BEGIN_ALIGN_TAG = "\\begin{align}";
        public static string END_ALIGN_TAG = "\\end{align}";
        #endregion

        #region PDFMode
        static List<string> multipleCommands = new List<string>(); //list of list/proof/theroem commands
        static List<string> tableCommands = new List<string>(); //list of table commands
        static List<string> matricesCommands = new List<string>(); //list of matrices commands
        static List<string> figureCommands = new List<string>(); //list of figure commands
        static List<string> combinatoricsCommands = new List<string>(); //list of multi-coefficent commands
        static List<string> setCommands = new List<string>(); //list of set latex commands
        static List<string> textCommands = new List<string>(); //text commands
        static List<string> algorithmCommands = new List<string>(); //alogorithms commands
        static Dictionary<int, string> diffDegrees = new Dictionary<int, string>(); //differential degree
        static Dictionary<int, string> roots = new Dictionary<int, string>(); //roots

        static public Dictionary<int, string> getRoots()
        {
            return roots;
        }

        static public Dictionary<int, string> getDiffDegrees()
        {
            return diffDegrees;
        }

        static public List<string> getAlgorithmCommands()
        {
            return algorithmCommands;
        }

        static public List<string> getTextCommands()
        {
            return textCommands;
        }

        static public List<string> getSetCommands()
        {
            return setCommands;
        }

        static public List<string> getCombinatoricsCommands()
        {
            return combinatoricsCommands;
        }

        static public List<string> getFigureCommands()
        {
            return figureCommands;
        }

        static public List<string> getMatricesCommands()
        {
            return matricesCommands;
        }

        static public List<string> getTableCommands()
        {
            return tableCommands;
        }

        static public List<string> getMultipleCommands()
        {
            return multipleCommands;
        }

        static public void populateSetCommands()
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
                setCommands.Add("\\mathcode"); //extractLatex() won't pick \\mathcode`\\,=\"213B
                setCommands.Add("\\bigoplus");
            }
        }

        static public void populateDiffDegree()
        {
            if (diffDegrees.Count <= 0)
            {
                diffDegrees.Add(1, "first");
                diffDegrees.Add(2, "second");
                diffDegrees.Add(3, "third");
            }
        }

        static public void populateRoots()
        {
            if (roots.Count <= 0)
            {
                roots.Add(2, "square");
                roots.Add(3, "cube");
            }
        }

        static public void populateTextCommands()
        {
            if (textCommands.Count <= 0)
            {
                textCommands.Add("\\text");
                textCommands.Add("\\textrm");
                textCommands.Add("\\textbf");
            }
        }

        static public void populateFigureCommands()
        {
            if (figureCommands.Count <= 0)
            {
                figureCommands.Add("\\begin{figure}");
                figureCommands.Add("\\end{figure}");
                figureCommands.Add("\\begin{subfigure}");
                figureCommands.Add("\\end{subfigure}");
                figureCommands.Add("\\alt");
                figureCommands.Add("\\includegraphics");
            }
        }

        static public void populateMultipleCommands()
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

        static public void populateMatricesCommands()
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

        static public void populateCombinatoricsCommands()
        {
            if (combinatoricsCommands.Count <= 0)
            {
                combinatoricsCommands.Add("\\choose");
                combinatoricsCommands.Add("\\binom");
                combinatoricsCommands.Add("\\brack");
                combinatoricsCommands.Add("\\brace");
            }
        }

        static public void populateAlgorithmCommands()
        {
            if (algorithmCommands.Count <= 0)
            {
                algorithmCommands.Add("\\begin{algorithmic}");
                algorithmCommands.Add("\\end{algorithmic}");
                algorithmCommands.Add("\\begin{algorithm}");
                algorithmCommands.Add("\\end{algorithm}");
                algorithmCommands.Add("\\if");
                algorithmCommands.Add("\\else");
                algorithmCommands.Add("\\endif");
                algorithmCommands.Add("\\procedure");
                algorithmCommands.Add("\\comment");
                algorithmCommands.Add("\\while");
                algorithmCommands.Add("\\endwhile");
                algorithmCommands.Add("\\endprocedure");
                algorithmCommands.Add("\\for");
                algorithmCommands.Add("\\state");
                algorithmCommands.Add("\\elsif");
                algorithmCommands.Add("\\elseif");
                algorithmCommands.Add("\\foreach");
                algorithmCommands.Add("\\lfor");
                algorithmCommands.Add("\\uif");
                algorithmCommands.Add("\\uelseif");
                algorithmCommands.Add("\\uelse");
                algorithmCommands.Add("\\kwresult");
                algorithmCommands.Add("\\kwdata");
                algorithmCommands.Add("\\kwto");
                algorithmCommands.Add("\\tcc");
                algorithmCommands.Add("\\tcp");
                algorithmCommands.Add("\\SetKwData");
                algorithmCommands.Add("\\SetKwFunction");
                algorithmCommands.Add("\\SetKwInOut");
                algorithmCommands.Add("\\lif");
                algorithmCommands.Add("\\lelse");
                algorithmCommands.Add("\\lforeach");
                algorithmCommands.Add("\\eif");
                algorithmCommands.Add("\\caption");
            }
        }
        #endregion

        #region DetexProcess
        static List<string> replaceLineLatexCommands = new List<string>();
        static List<string> latexExtractableCommandsList = new List<string>();
        static Dictionary<string, string> latexExtractableCommandsDict = new Dictionary<string, string>();
        static List<string> miscellenousLatexTextCommands = new List<string>(); //commands that don't get catched by extractLatex() functions
        static Dictionary<string, string> miscellenousLatexTextCommandsDict = new Dictionary<string, string>();

        static public List<string> getReplaceLineLatexCommands()
        {
            return replaceLineLatexCommands;
        }
        static public List<string> getLatexExtractableCommandsList()
        {
            return latexExtractableCommandsList;
        }
        static public Dictionary<string, string> getLatexExtractableCommandsDict()
        {
            return latexExtractableCommandsDict;
        }
        static public List<string> getMiscellenousLatexTextCommands()
        {
            return miscellenousLatexTextCommands;
        }
        static public Dictionary<string, string> getMiscellenousLatexTextCommandsDict()
        {
            return miscellenousLatexTextCommandsDict;
        }

        static private string currentDate()
        {
            string date = "";
            int month = DateTime.Now.Month;
            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
            int day = DateTime.Now.Day;
            int year = DateTime.Now.Year;
            date = monthName + " " + day + " , " + year;
            return date;
        }

        static public void populatereplaceLineLatexCommands()
        {
            if (replaceLineLatexCommands.Count <= 0)
            {
                replaceLineLatexCommands.Add("\\declaresymbolfont");
                replaceLineLatexCommands.Add("\\renewenvironment");
                replaceLineLatexCommands.Add("\\newtheorem");
                replaceLineLatexCommands.Add("\\declaremathsymbol");
                replaceLineLatexCommands.Add("\\def");
                replaceLineLatexCommands.Add("\\lhead");
                replaceLineLatexCommands.Add("\\rhead");
                replaceLineLatexCommands.Add("\\newcounter");
                replaceLineLatexCommands.Add("\\nocite");
                replaceLineLatexCommands.Add("\\doi");
                replaceLineLatexCommands.Add("\\isbn");
                replaceLineLatexCommands.Add("\\conferenceinfo");
                replaceLineLatexCommands.Add("\\acmprice");
                replaceLineLatexCommands.Add("\\conferenceinfo");
                replaceLineLatexCommands.Add("\\setcopyright");
                replaceLineLatexCommands.Add("\\\numberofauthors");
            }
        }

        static public void populatelatexExtractableCommandsList()
        {
            if (latexExtractableCommandsList.Count <= 0)
            {
                latexExtractableCommandsList.Add("\\textbackslash");
                latexExtractableCommandsList.Add("\\latex");
                latexExtractableCommandsList.Add("\\latex");
                latexExtractableCommandsList.Add("\\tex");
                latexExtractableCommandsList.Add("\\today");
                latexExtractableCommandsList.Add("\\maketitle");
                latexExtractableCommandsList.Add("\\titlenote");
                latexExtractableCommandsList.Add("\\footnote");
                latexExtractableCommandsList.Add("\\keywords");
            }
        }

        static public void populateMiscellenousLatexTextCommands()
        {
            if (miscellenousLatexTextCommands.Count <= 0)
            {
                miscellenousLatexTextCommands.Add("-");
                miscellenousLatexTextCommands.Add("\\begin{abstract}");
                miscellenousLatexTextCommands.Add("\\$");
                miscellenousLatexTextCommands.Add("\\%");
                miscellenousLatexTextCommands.Add("\\tex\\");
                miscellenousLatexTextCommands.Add("\\latex\\");
                miscellenousLatexTextCommands.Add("\\verb*");
            }
        }


        static public void populateMiscellenousLatexTextCommandsDict()
        {
            if (miscellenousLatexTextCommandsDict.Count <= 0)
            {
                miscellenousLatexTextCommandsDict.Add("-", "dash");
                miscellenousLatexTextCommandsDict.Add("\\begin{abstract}", "abstract");
                miscellenousLatexTextCommandsDict.Add("\\$", "dollar sign");
                miscellenousLatexTextCommandsDict.Add("\\%", "percentage");
                miscellenousLatexTextCommandsDict.Add("\\tex\\", "latex");
                miscellenousLatexTextCommandsDict.Add("\\latex\\", "latex");
                miscellenousLatexTextCommandsDict.Add("\\verb*", "\\verb");
            }
        }
        static public void populateLatexExtractableCommandsDict()
        {
            if (latexExtractableCommandsDict.Count <= 0)
            {
                latexExtractableCommandsDict.Add("\\textbackslash", "\\");
                latexExtractableCommandsDict.Add("\\latex", "latex");
                latexExtractableCommandsDict.Add("\\tex", "latex");
                latexExtractableCommandsDict.Add("\\today", currentDate());
                latexExtractableCommandsDict.Add("\\maketitle", currentDate());
                latexExtractableCommandsDict.Add("\\keywords", "keywords");
            }
        }
        #endregion

        #region ShortcutKeys
        public static string DESC_CURSOR_CONT_TTS = "cursor controlled tts";
        public static string DESC_DECREASE_SPEED = "decrease speed";
        public static string DESC_INCREASE_SPEED = "increase speed";
        public static string DESC_MATH_MODE = "math mode";
        public static string DESC_PDF_ACCESSIBILITY = "pdf accessibility";
        public static string DESC_TTS_OFF = "tts off";
        public static string DESC_TTS_PAUSE = "tts pause";
        public static string DESC_TTS_RESUME = "tts resume";
        public static string DESC_TTS_START = "tts start";
        public static string DESC_TTS_STOP = "tts stop";
        public static string DESC_VERB_CHAR = "verbosity by character";
        public static string DESC_VERB_WORD = "verbosity by word";

        public static string SHRTCUT_CURSOR_CONT_TTS = "CapsLock I";
        public static string SHRTCUT_DECREASE_SPEED = "CapsLock -";
        public static string SHRTCUT_INCREASE_SPEED = "CapsLock +";
        public static string SHRTCUT_MATH_MODE = "CapsLock D";
        public static string SHRTCUT_PDF_ACCESSIBILITY = "CapsLock A";
        public static string SHRTCUT_TTS_OFF = "CapsLock Q";
        public static string SHRTCUT_TTS_PAUSE = "CapsLock P";
        public static string SHRTCUT_TTS_RESUME = "CapsLock R";
        public static string SHRTCUT_TTS_START = "CapsLock S";
        public static string SHRTCUT_TTS_STOP = "CapsLock E";
        public static string SHRTCUT_VERB_CHAR = "CapsLock C";
        public static string SHRTCUT_VERB_WORD = "CapsLock W";
        #endregion
    }
}

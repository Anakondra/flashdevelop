using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using ScintillaNet.Configuration;
using ScintillaNet.Enums;
using ScintillaNet.Lexers;

namespace FastColoredTextBoxNS
{
    public static class CustomHighlighter
    {
        public static ScintillaNet.Configuration.Language Language
        {
            set
            {
                language = value;
                for (int i = 0; i < 32; i++)
                {
                    regexes[i] = new Regex("");

                    UseStyle style = language.GetUseStyle(i);
                    if (style != null)
                        styles[i] = new TextStyle(
                            BrushFromRGB(style.ForegroundColor),
                            BrushFromRGB(style.BackgroundColor),
                            System.Drawing.FontStyle.Regular);
                }
                InitCppRegexes();
            }
            get { return language; }
        }

        private static Brush BrushFromRGB(int color)
        {
            return new SolidBrush(Color.FromArgb(color & 0xFF, color >> 8 & 0xFF, color >> 16 & 0xFF));
        }

        private static ScintillaNet.Configuration.Language language;
        private static FastColoredTextBox editor;
        private static Regex[] regexes = new Regex[32];
        private static Style[] styles = new Style[32];

        /**
        * Built in styles:
        * DefaultStyle, SelectionStyle, FoldedBlockStyle, BracketsStyle, BracketsStyle2, BracketsStyle3
        * Built in colors:
        * BackColor (BackBrush too), ForeColor, CurrentLineColor, ChangedLineColor, BookmarkColor, LineNumberColor, IndentBackColor, PaddingBackColor, 
        * DisabledColor, CaretColor, ServiceLinesColor, FoldingIndicatorColor, ServiceColors (has 6), 
        */

        static class CPP
        {
            public const int DEFAULT = 0;
            public const int COMMENT = 1;
            public const int COMMENTLINE = 2;
            public const int COMMENTDOC = 3;
            public const int NUMBER = 4;
            public const int WORD = 5;
            public const int STRING = 6;
            public const int CHARACTER = 7;
            public const int UUID = 8;
            public const int PREPROCESSOR = 9;
            public const int OPERATOR = 10;
            public const int IDENTIFIER = 11;
            public const int STRINGEOL = 12;
            public const int VERBATIM = 13;
            public const int REGEX = 14;
            public const int COMMENTLINEDOC = 15;
            public const int WORD2 = 16;
            public const int COMMENTDOCKEYWORD = 17;
            public const int COMMENTDOCKEYWORDERROR = 18;
            public const int GLOBALCLASS = 19;
            public const int STRINGRAW = 20;
            public const int TRIPLEVERBATIM = 21;
            public const int HASHQUOTEDSTRING = 22;
            public const int PREPROCESSORCOMMENT = 23;
            public const int WORD3 = 24;
            public const int WORD4 = 25;
            public const int WORD5 = 26;
            public const int GDEFAULT = 32;
            public const int LINENUMBER = 33;
            public const int BRACELIGHT = 34;
            public const int BRACEBAD = 35;
            public const int CONTROLCHAR = 36;
            public const int INDENTGUIDE = 37;
            public const int LASTPREDEFINED = 39;
        }

        public static void Init(FastColoredTextBox fctb)
        {
            editor = fctb;
            editor.AllowSeveralTextStyleDrawing = true;
            editor.TextChanged += OnTextChangedDelayed;
        }

        private static void InitCppRegexes()
        {
            regexes[CPP.COMMENT] = new Regex(@"//.*$", RegexOptions.Multiline | RegexOptions.Compiled);
            regexes[CPP.STRING] = new Regex(@"""""|''|"".*?[^\\]""|'.*?[^\\]'", RegexOptions.Compiled);
        }

        private static void OnTextChangedDelayed(Object sender, TextChangedEventArgs e)
        {
            if (Language == null) return;

            Range range = editor.Range;

            // set options
            range.tb.CommentPrefix = "//";
            range.tb.LeftBracket = '(';
            range.tb.RightBracket = ')';
            range.tb.LeftBracket2 = '{';
            range.tb.RightBracket2 = '}';
            range.tb.LeftBracket3 = '[';
            range.tb.RightBracket3 = ']';
            range.tb.AutoIndentCharsPatterns = @"^\s*[\w\.]+(\s\w+)?\s*(?<range>=)\s*(?<range>[^;]+);^\s*(case|default)\s*[^:]*(?<range>:)\s*(?<range>[^;]+);";
            range.tb.BracketsHighlightStrategy = BracketsHighlightStrategy.Strategy1;

            range.ClearStyle(styles);
            //highlight syntax
            for (int i = 0; i < 32; i++)
                if (styles[i] != null)
                    range.SetStyle(styles[i], regexes[i]);

            // set folding markers
            range.ClearFoldingMarkers();
            range.SetFoldingMarkers("{", "}"); //allow to collapse brackets block
            range.SetFoldingMarkers(@"//{", @"//}"); //allow to collapse #region blocks
            range.SetFoldingMarkers(@"/\*", @"\*/"); //allow to collapse comment block
        }

    }

}

